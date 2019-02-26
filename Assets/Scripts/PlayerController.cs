using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Constants
    private float MAXSPEED = 10f;
    private readonly float MAXAIRTIME = .4f;
    private readonly float STARTINGMASS = 1f;

    // Camera
    private Camera playerCamera;
    public float cameraSensitivity = 50f;
    private bool cursorLocked;
    private float camRotate;
    private float charRotate;

    // Player Movement
    private Rigidbody playerBody;
    private bool isCrouched;
    public float crouchHeight = 0.6f;
    public float moveSpeed;
    public float moveAcceleration = 10f;
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
    public float wallRunDuration = 1f;
    public float climbSpeed = 2f;
    public RaycastHit observedObj;

    // Dash
    public bool dashReady = true;
    public float dashTimer;
    public float dashCooldown = 3f;
    public float dashForce = 20f;
    public GameObject dashEffect;

    // UI Elements
    public Slider dashCooldownSlider;
    public RawImage climb;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = GetComponentInChildren<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        dashCooldownSlider.maxValue = dashCooldown;

        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        climb.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Ground Movement
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");
        Vector3 move_velocity = Vector3.zero;

        if (moveSpeed > 0f)
        {
            moveSpeed -= Time.deltaTime * moveDeceleration;
        }
        if (Mathf.Abs(h_input) > 0f)
        {
            if (moveSpeed < MAXSPEED)
            {
                moveSpeed += Time.deltaTime * moveAcceleration;
            }
            move_velocity += Time.deltaTime * transform.right * h_input * moveSpeed;
        }
        if (Mathf.Abs(v_input) > 0f)
        {
            if (moveSpeed < MAXSPEED)
            {
                moveSpeed += Time.deltaTime * (MAXSPEED - (moveSpeed / MAXSPEED));
            }
            move_velocity += Time.deltaTime * transform.forward * v_input * moveSpeed;

            if (v_input > 0f && onWall)
            {
                playerBody.velocity = Vector3.up * climbSpeed;
            }
        }
        //playerBody.AddForce(move_velocity * (1 - (playerBody.velocity.magnitude / MAXSPEED)));
        playerBody.MovePosition(transform.position + move_velocity);

        // Camera Movement

        //Horizontal Rotation
        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.0f)
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * cameraSensitivity);
        }

        charRotate += Input.GetAxis("Mouse X") * Time.deltaTime * cameraSensitivity;
        camRotate -= Input.GetAxis("Mouse Y") * Time.deltaTime * cameraSensitivity;
        camRotate = Mathf.Clamp(camRotate, -90f, 90f);
        playerCamera.transform.rotation = Quaternion.Euler(camRotate, charRotate, 0f);

        /*
        float mouse_x = Input.GetAxis("Mouse X");
        float mouse_y = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouse_x) > 0f)
        {
            transform.Rotate(Time.deltaTime * Vector3.up * mouse_x * cameraSensitivity);
        }
        if (Mathf.Abs(mouse_y) > 0f)
        {
            playerCamera.transform.Rotate(Time.deltaTime * Vector3.left * mouse_y * cameraSensitivity);
        }
        */
        // Check grounded
        if (Physics.Raycast(transform.position, Vector3.down, 2f))
        {
            grounded = true;
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
                moveAcceleration = 10f;
                MAXSPEED = 10f;
            }
            else
            {
                transform.localScale = Vector3.one * crouchHeight;
                isCrouched = true;
                moveAcceleration = 7f;
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
            playerBody.mass = STARTINGMASS;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            jumpHeightTimer += Time.deltaTime;
            if (colliding) // Reduce mass to simulate lowered gravity while attached to wall
            {
                wallRunTimer += Time.deltaTime;
                if (wallRunTimer < wallRunDuration)
                {
                    playerBody.mass = STARTINGMASS / 100f;
                }
            }
            if (jumpHeightTimer < MAXAIRTIME)
            {
                playerBody.velocity = Vector3.up * jumpForce;
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
                    //playerBody.AddForce((h_dash + v_dash) * dashForce, ForceMode.Impulse);
                }
                else
                {
                    playerBody.velocity = transform.forward * dashForce;
                    //playerBody.AddForce(transform.forward * dashForce, ForceMode.Impulse);
                }
                dashEffect.GetComponent<ParticleSystem>().Play();

                dashReady = false;
            }
            dashCooldownSlider.value = dashCooldownSlider.maxValue;
        }
        else
        {
            dashTimer += Time.deltaTime;
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
        colliding = true;
        //if (collision.gameObject.tag.Equals("BounceBlock"))
        //{
        //    this.GetComponent<Rigidbody>().AddForce(new Vector3(0f, 13f), ForceMode.Impulse);
        //}
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("ClimbableWall"))
        {
            onWall = true;
            curJumpCount = 0;

            //if (Input.GetKey(KeyCode.W))
            //{
            //        this.GetComponent<Rigidbody>().AddForce(new Vector3(0f, .25f), ForceMode.Impulse);
            //}
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        colliding = false;
        onWall = false;
        playerBody.mass = STARTINGMASS;
        playerBody.AddForce(Vector3.down, ForceMode.Impulse);
    }
}
