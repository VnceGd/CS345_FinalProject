using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Managers
    private MainMenuManager mainMenuManager;

    // Constants
    private readonly float MAXAIRTIME = .25f;

    // Camera
    private Camera playerCamera;
    public float cameraSensitivity = 50f;
    private bool cursorLocked;
    private float camRotate;
    private float charRotate;
    private ChromaticAberration chromaticAberration;

    // Player Movement / Status
    private Rigidbody playerBody;
    private bool isCrouched;
    public float crouchHeight = 0.6f;
    private float maxSpeed = 10f;
    public float moveSpeed = 7000f;
    public float moveDeceleration = 400f;

    public float jumpForce = 7f;
    public float jumpHeightTimer;
    public int curJumpCount;
    public int maxJumpCount = 2;
    public float gracePeriod = 0.1f;
    public float gracePeriodTimer;

    // Ground and Wall Check
    public bool grounded;
    public bool colliding;
    private bool onWall;
    public bool onBouncy;
    private float wallRunTimer;
    public float wallRunDuration = 1f;
    public float climbSpeed = 2f;
    private RaycastHit observedObj;

    // Dash
    private bool dashReady = true;
    private bool dashing;
    private float dashTimer;
    public float dashCooldown = 3f;
    public float dashForce = 20f;
    public float dashDuration = 0.3f;

    // UI Elements
    public Slider dashCooldownSlider;
    public RawImage climb;

    // Start is called before the first frame update
    public void Start()
    {
        mainMenuManager = GameObject.Find("Menu Manager").GetComponent<MainMenuManager>();

        playerBody = GetComponentInChildren<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        PostProcessVolume volume = playerCamera.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out chromaticAberration);

        dashCooldownSlider.maxValue = dashCooldown;

        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        climb.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        // Ground Movement
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");
        Vector3 move_velocity = Vector3.zero;

        if (!dashing)
        {
            if (Mathf.Abs(h_input) > 0f)
            {
                move_velocity += Time.deltaTime * transform.right * h_input * moveSpeed;
            }
            if (Mathf.Abs(v_input) > 0f)
            {
                move_velocity += Time.deltaTime * transform.forward * v_input * moveSpeed;

                if (v_input > 0f && onWall)
                {
                    playerBody.velocity = Vector3.up * climbSpeed;
                }
            }
        }
        if (playerBody.velocity.magnitude < maxSpeed)
        {
            // Accelerate the player
            playerBody.AddForce(move_velocity * (1 - (playerBody.velocity.magnitude / maxSpeed)));
        }
        if (move_velocity.magnitude < 0.1f)
        {
            // Decelerate the player
            if (grounded && !onBouncy)
            {
                playerBody.AddForce(Time.deltaTime * moveDeceleration * -playerBody.velocity);
            }
            else
            {
                playerBody.AddForce(Time.deltaTime * moveDeceleration * 0.5f *
                                    new Vector3(-playerBody.velocity.x, 0f, -playerBody.velocity.z));
            }
        }

        // Camera Movement
        float mouse_x = Input.GetAxis("Mouse X");
        float mouse_y = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouse_x) > 0.0f)
        {
            transform.Rotate(Vector3.up * mouse_x * Time.deltaTime * cameraSensitivity);
            charRotate += mouse_x * Time.deltaTime * cameraSensitivity;
        }
        if (Mathf.Abs(mouse_y) > 0.0f)
        {
            camRotate -= Input.GetAxis("Mouse Y") * Time.deltaTime * cameraSensitivity;
        }
        camRotate = Mathf.Clamp(camRotate, -90f, 90f);
        playerCamera.transform.rotation = Quaternion.Euler(camRotate, charRotate, 0f);

        // Check grounded
        if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            if (!onBouncy && !grounded)
            {
                playerCamera.GetComponent<Animation>().Play();
                grounded = true;
            }
            gracePeriodTimer = gracePeriod;
            curJumpCount = 0;
        }
        else
        {
            if (gracePeriodTimer < 0f)
            {
                grounded = false;
            }
            else
            {
                gracePeriodTimer -= Time.deltaTime;
            }
        }

        // Crouching
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isCrouched)
            {
                transform.localScale = Vector3.one;
                isCrouched = false;
                maxSpeed = 10f;
            }
            else
            {
                transform.localScale = Vector3.one * crouchHeight;
                isCrouched = true;
                maxSpeed = 7f;
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //curJumpCount++;
            if (grounded || (maxJumpCount > curJumpCount++ && jumpHeightTimer > MAXAIRTIME))
            {
                grounded = false;
                jumpHeightTimer = 0f;
                wallRunTimer = 0f;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //jumpHeightTimer = MAXAIRTIME;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            jumpHeightTimer += Time.deltaTime;
            if (colliding)
            {
                wallRunTimer += Time.deltaTime;
                if (!onWall && !onBouncy && wallRunTimer < wallRunDuration)
                {
                    if (Physics.Raycast(transform.position, transform.right, 1f) ||
                        Physics.Raycast(transform.position, -transform.right, 1f))
                    {
                        if (!grounded)
                        {
                            playerBody.constraints = RigidbodyConstraints.FreezePositionY | 
                                                     RigidbodyConstraints.FreezeRotation;
                        }
                    }
                }
                else
                {
                    playerBody.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
            if (grounded || (maxJumpCount > curJumpCount && jumpHeightTimer <= MAXAIRTIME))
            {
                //playerBody.AddForce(Vector3.up * jumpForce);
                playerBody.velocity = new Vector3(playerBody.velocity.x, jumpForce, playerBody.velocity.z);
            }
        }

        // Wall Climbing

        if (Physics.Raycast(transform.position, transform.forward, out observedObj, 5.0f))
        {
            if (observedObj.collider.tag.Equals("ClimbableWall"))
            {
                climb.gameObject.SetActive(true);
            }
        }
        else
        {
            climb.gameObject.SetActive(false);
        }

        // Dashing
        if (dashReady)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Vector3 h_dash = Vector3.zero;
                Vector3 v_dash = Vector3.zero;

                if (Mathf.Abs(h_input) > 0f)
                {
                    if (Mathf.Abs(v_input) > 0f)
                    {
                        playerBody.velocity = transform.forward * dashForce;
                    }
                    else
                    {
                        playerBody.velocity = transform.right * Mathf.Clamp(h_input, -1f, 1f) * dashForce;
                    }
                }
                else if (Mathf.Abs(v_input) > 0f)
                {
                    if (Mathf.Abs(h_input) > 0f)
                    {
                        playerBody.velocity = transform.forward * dashForce;
                    }
                    else
                    {
                        playerBody.velocity = transform.forward * Mathf.Clamp(v_input, -1f, 1f) * dashForce;
                    }
                }
                else
                {
                    playerBody.velocity = transform.forward * dashForce;
                }
                // Multi-direction dashing
                /*
                if (h > 0f || v > 0f)
                {
                    if (h > 0f)
                    {
                        h_dash = transform.right * (h_input / h);
                    }
                    if (v > 0f)
                    {
                        v_dash = transform.forward * (v_input / v);
                    }
                    playerBody.velocity = ((Vector3.Normalize(h_dash) / 2) + (Vector3.Normalize(v_dash) / 2))
                                        * dashForce;
                }
                else
                {
                    playerBody.velocity = transform.forward * dashForce;
                    //if (grounded)
                    //{
                    //    playerBody.velocity = transform.forward * dashForce;
                    //}
                    //else
                    //{
                    //    playerBody.velocity = transform.forward * dashForce * 0.5f;
                    //}
                }
                */
                chromaticAberration.enabled.value = true;
                dashing = true;
                dashReady = false;
            }
            dashCooldownSlider.value = dashCooldownSlider.maxValue;
        }
        else
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
                dashing = false;
                chromaticAberration.enabled.value = false;
                playerBody.AddForce(Time.deltaTime * moveDeceleration * -1f *
                                    new Vector3(playerBody.velocity.x, 0f, playerBody.velocity.z));
            }
            if (dashTimer >= dashCooldown)
            {
                dashReady = true;
                dashTimer = 0f;
            }
            dashCooldownSlider.value = dashTimer;
        }

        // Lock or Unlock Cursor
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
            {
                cursorLocked = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                cursorLocked = true;
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (mainMenuManager)
            {
                mainMenuManager.ToggleMenu();
            }
        }
    }

    public void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Environment"))
        {
            onBouncy = false;
        }
        if (collision.gameObject.tag.Equals("BounceBlock"))
        {
            onBouncy = true;
        }
        if (collision.gameObject.tag.Equals("Hazard"))
        {
            Death();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!grounded)
        {
            colliding = true;
        }
        if (collision.gameObject.tag.Equals("ClimbableWall"))
        {
            onWall = true;
            curJumpCount = 0;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        colliding = false;
        onWall = false;
        playerBody.constraints = RigidbodyConstraints.FreezeRotation;
        playerBody.AddForce(Vector3.down, ForceMode.Impulse);
    }
}
