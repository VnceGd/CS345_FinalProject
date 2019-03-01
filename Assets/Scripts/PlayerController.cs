using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PlayerController : MonoBehaviour
{
    // Constants
    private float MAXSPEED = 10f;
    private readonly float MAXAIRTIME = .25f;

    // Camera
    private Camera playerCamera;
    public float cameraSensitivity = 50f;
    private bool cursorLocked;
    private float camRotate;
    private float charRotate;
    private ChromaticAberration chromaticAberration;

    // Player Movement
    private Rigidbody playerBody;
    private bool isCrouched;
    public float crouchHeight = 0.6f;
    public float moveSpeed;
    public float moveDeceleration = 5f;
    public float jumpForce = 3f;
    public float jumpHeightTimer;
    public int curJumpCount;
    public int maxJumpCount = 2;
    public float gracePeriod = 0.1f;

    // Ground and Wall Check
    public bool grounded;
    public bool colliding;
    public bool onWall;
    public float wallRunTimer;
    public float wallRunDuration = 2f;
    public float climbSpeed = 2f;
    public RaycastHit observedObj;

    // Dash
    public bool dashReady = true;
    public float dashTimer;
    public float dashCooldown = 3f;
    public float dashForce = 20f;
    public float dashDuration = 0.3f;

    // UI Elements
    public Slider dashCooldownSlider;
    public RawImage climb;

    // Start is called before the first frame update
    public void Start()
    {
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
        if (playerBody.velocity.magnitude < MAXSPEED)
        {
            playerBody.AddForce(move_velocity * (1 - (playerBody.velocity.magnitude / MAXSPEED)));
        }
        if (grounded)
        {
            playerBody.AddForce(Time.deltaTime * moveDeceleration * -playerBody.velocity);
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
        if (Physics.Raycast(transform.position, Vector3.down, 2f))
        {
            if (!grounded)
            {
                playerCamera.GetComponent<Animation>().Play();
                grounded = true;
            }
            gracePeriod = 0.1f;
            curJumpCount = 0;
        }
        else
        {
            gracePeriod -= Time.deltaTime;
            if (gracePeriod <= 0f)
            {
                grounded = false;
            }
        }

        // Crouching
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isCrouched)
            {
                transform.localScale = Vector3.one;
                isCrouched = false;
                MAXSPEED = 10f;
            }
            else
            {
                transform.localScale = Vector3.one * crouchHeight;
                isCrouched = true;
                MAXSPEED = 7f;
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            curJumpCount++;
            if (grounded || curJumpCount < maxJumpCount)
            {
                jumpHeightTimer = 0f;
                wallRunTimer = 0f;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpHeightTimer = MAXAIRTIME;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            jumpHeightTimer += Time.deltaTime;
            if (colliding)
            {
                wallRunTimer += Time.deltaTime;
                if (!onWall && wallRunTimer < wallRunDuration)
                {
                    playerBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
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
                    playerBody.velocity = transform.forward * dashForce;
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
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!grounded)
        {
            colliding = true;
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
        playerBody.constraints = RigidbodyConstraints.FreezeRotation;
        playerBody.AddForce(Vector3.down, ForceMode.Impulse);
    }
}
