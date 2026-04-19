using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
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
    private bool lastDrift = false;
    [SerializeField] private List<GameObject> ParticleSystems;
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
        Vector2 angle_l = inputActions.Player.Move.ReadValue<Vector2>();
        bool drift = driftKey == 1f && Mathf.Abs(angle.x + angle_l.x) > 0.2f;
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
        if(drift != lastDrift)
        {
            foreach (GameObject obj in ParticleSystems)
            {
                obj.SetActive(drift);
            }
        }
        lastDrift = drift;
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

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }
}
