using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    public GameObject mainCamera;
    public float speed = 20f;
    public float maxSpeed = 20f;
    public float rotationSpeed = 2f;
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
        float accelerator = inputActions.Player.Accelerator.ReadValue<float>();
        if (accelerator == 1f)
        {
            rb.AddForce(transform.forward * speed);
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(forward);

            // 補間（0〜1の割合）
            Quaternion newRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(newRotation);


            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
    }
}
