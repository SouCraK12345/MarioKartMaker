using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    public GameObject mainCamera;
    public float acceleration = 20f;
    public float speedDivisor = 1.033f;
    public float speed = 0;
    public float rotationSpeed = 2f;
    private int touchingStages = 0;
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float driftKey = inputActions.Player.Drift.ReadValue<float>();
        Vector2 angle = inputActions.Player.Look.ReadValue<Vector2>();
        bool drift = driftKey == 1f && angle.x > 0.2f;
        float accelerator = inputActions.Player.Accelerator.ReadValue<float>();
        if (accelerator == 1f)
        {
            speed++;
        }
        float back = inputActions.Player.Back.ReadValue<float>();
        if (back == 1f)
        {
            speed--;
        }
        speed /= speedDivisor;

        if (drift)
        {
            rb.AddForce(transform.forward.normalized * speed);
        }
        else
        {
            move(transform.forward.normalized * speed);
        }

        // 方向の補正
        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(forward);

        // 補間（0〜1の割合）
        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            (drift ? 15f : speed / 15) * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
        if (touchingStages > 0)
        {
            speedDivisor = 1.033f;
        }
        else
        {
            speedDivisor = 1.066f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Stage")
        {
            touchingStages++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Stage")
        {
            touchingStages--;
        }
    }

    void move(Vector3 targetVelocity)
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 0.1f);
    }
}
