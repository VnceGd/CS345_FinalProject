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
    private bool isDead = false;
    public float crouchHeight = 0.6f;
    private float maxSpeed = 10f;
    public float moveSpeed = 7000f;
    public float moveDeceleration = 400f;

    public float jumpForce = 7f;
    private float jumpHeightTimer;
    private int curJumpCount;
    public int maxJumpCount = 2;
    public float gracePeriod = 0.1f;
    private float gracePeriodTimer;

    // Ground and Wall Check
    private bool grounded;
    private bool colliding;
    private bool onWall;
    private bool onBouncy;
    private float wallRunTimer;
    public float wallRunDuration = 1f;
    public float climbSpeed = 2f;
    private RaycastHit observedObj;

    // Dash
    private bool dashReady = true;
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
        if (playerBody.velocity.magnitude < maxSpeed)
        {
            playerBody.AddForce(move_velocity * (1 - (playerBody.velocity.magnitude / maxSpeed)));
        }
        if (grounded && move_velocity.magnitude < 0.1f)
        {
            playerBody.AddForce(Time.deltaTime * moveDeceleration * -playerBody.velocity);
        }
        else
        {
            playerBody.AddForce(Time.deltaTime * moveDeceleration * 0.5f * 
                                new Vector3(-playerBody.velocity.x, 0f, -playerBody.velocity.z));
        }

        // Camera Movement
        float mouse_x = Input.GetAxis("Mouse X");

        if (Mathf.Abs(mouse_x) > 0.0f)
        {
            transform.Rotate(Vector3.up * mouse_x * Time.deltaTime * cameraSensitivity);
        }

        charRotate += mouse_x * Time.deltaTime * cameraSensitivity;
        camRotate -= Input.GetAxis("Mouse Y") * Time.deltaTime * cameraSensitivity;
        camRotate = Mathf.Clamp(camRotate, -90f, 90f);
        playerCamera.transform.rotation = Quaternion.Euler(camRotate, charRotate, 0f);

        // Check grounded
        if (Physics.Raycast(transform.position, Vector3.down, 1.25f))
        {
            if (!grounded)
            {
                playerCamera.GetComponent<Animation>().Play();
            }
            grounded = true;
            gracePeriodTimer = gracePeriod;
            curJumpCount = 0;
        }
        else
        {
            gracePeriodTimer -= Time.deltaTime;
            if (gracePeriodTimer <= 0f)
            {
                grounded = false;
            }
        }

        //Check if Alive

        if (isDead)
        {
            Scene cur_Scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(cur_Scene.name, LoadSceneMode.Single);
        }

        // Crouching
        if (Input.GetKeyDown(KeyCode.C))
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
            curJumpCount++;
            if (grounded || (curJumpCount < maxJumpCount && jumpHeightTimer > MAXAIRTIME))
            {
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
                            playerBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                        }
                    }

                }
                else
                {
                    playerBody.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
            if (jumpHeightTimer < MAXAIRTIME)
            {
                //playerBody.AddForce(Vector3.up * jumpForce);
                playerBody.velocity = new Vector3(playerBody.velocity.x,
                                                  jumpForce,
                                                  playerBody.velocity.z);
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
                float h = Mathf.Abs(h_input);
                float v = Mathf.Abs(v_input);

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
                    playerBody.velocity = (h_dash + v_dash) * dashForce;
                }
                else
                {
                    if (grounded)
                    {
                        playerBody.velocity = transform.forward * dashForce;
                    }
                    else
                    {
                        playerBody.velocity = transform.forward * dashForce * 0.5f;
                    }
                }
                chromaticAberration.enabled.value = true;

                dashReady = false;
            }
            dashCooldownSlider.value = dashCooldownSlider.maxValue;
        }
        else
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
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
        if (Input.GetKeyDown(KeyCode.M))
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

    private void OnCollisionEnter(Collision collision)
    {
        if(!grounded)
        {
            colliding = true;
        }
        if (collision.gameObject.tag.Equals("BounceBlock"))
        {
            onBouncy = true;
        }
        if (collision.gameObject.tag.Equals("Hazard"))
        {
            isDead = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
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
        onBouncy = false;
        playerBody.constraints = RigidbodyConstraints.FreezeRotation;
        playerBody.AddForce(Vector3.down, ForceMode.Impulse);
    }
}
