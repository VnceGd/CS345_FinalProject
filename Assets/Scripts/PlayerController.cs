using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Constants
    private readonly float MAXSPEED = 10f;
    private readonly float MAXAIRTIME = .4f;
    private readonly float STARTINGMASS = 1f;

    // Camera
    private Camera playerCamera;
    public float cameraSensitivity = 50f;
    private bool cursorLocked;

    // Player Movement
    private Rigidbody playerBody;
    public float moveSpeed;
    public float moveAcceleration = 10f;
    public float moveDeceleration = 5f;
    public float jumpForce = 3f;
    public float jumpHeightTimer;
    public float gracePeriod = 0.1f;

    // Ground and Wall Check
    public bool grounded;
    public bool colliding;

    // Dash
    public bool dashReady = true;
    public float dashTimer;
    public float dashCooldown = 3f;
    public float dashForce = 50f;

    // UI Elements
    public Slider dashCooldownSlider;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = GetComponentInChildren<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        dashCooldownSlider.maxValue = dashCooldown;

        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Ground Movement
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");
        Vector3 moveVelocity = Vector3.zero;

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
            moveVelocity += Time.deltaTime * transform.right * h_input * moveSpeed;
        }
        if (Mathf.Abs(v_input) > 0f)
        {
            if (moveSpeed < MAXSPEED)
            {
                moveSpeed += Time.deltaTime * moveAcceleration;
            }
            moveVelocity += Time.deltaTime * transform.forward * v_input * moveSpeed;
        }
        playerBody.MovePosition(transform.position + moveVelocity);

        // Camera Movement
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

        // Check grounded
        if (Physics.Raycast(transform.position, Vector3.down, 2f))
        {
            grounded = true;
            gracePeriod = 0.1f;
        }
        else
        {
            gracePeriod -= Time.deltaTime;
            if (gracePeriod <= 0f)
            {
                grounded = false;
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grounded)
            {
                jumpHeightTimer = 0f;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpHeightTimer = MAXAIRTIME;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            jumpHeightTimer += Time.deltaTime;
            if (colliding) // Reduce mass to simulate lowered gravity while attached to wall
            {
                playerBody.mass = STARTINGMASS / 2;
            }
            if (jumpHeightTimer < MAXAIRTIME)
            {
                playerBody.velocity = Vector3.up * jumpForce;
            }
        }

        // Dashing
        if(dashReady)
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
                    playerBody.AddForce((h_dash + v_dash) * dashForce);
                }
                else
                {
                    playerBody.AddForce(transform.forward * dashForce);
                }
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
    }

    private void OnCollisionExit(Collision collision)
    {
        colliding = false;
        playerBody.mass = STARTINGMASS;
    }
}
