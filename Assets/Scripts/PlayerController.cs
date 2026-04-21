using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

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
    private float minDrift = 0;
    private string Action = "none";
    [SerializeField] private List<GameObject> ParticleSystems;

    public GameObject forCamera;
    private float angle_horizontal = 0;
    private float angle_vertical = 0.4f;
    private float distance = 6f; // カメラの距離
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Drift.performed += startAction;
        inputActions.Player.Drift.canceled += endAction;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Drift.performed -= startAction;
        inputActions.Player.Drift.canceled -= endAction;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector2 angle = inputActions.Player.Look.ReadValue<Vector2>();
        Vector2 angle_l = inputActions.Player.Move.ReadValue<Vector2>();
        if (Action != "ChargeJump")
        {
            if (Action == "Drift" && Math.Abs(angle.x + angle_l.x) < 0.2)
            {
                angle_horizontal += minDrift / -50;
            }
            else
            {
                angle_horizontal += (angle.x + angle_l.x) / -50;
            }
        }
        angle_vertical += angle.y / -50;
        mainCamera.transform.position = new Vector3(
            transform.position.x + Mathf.Cos(angle_horizontal) * Mathf.Cos(angle_vertical) * distance,
            transform.position.y + Mathf.Sin(angle_vertical) * distance,
            transform.position.z + Mathf.Sin(angle_horizontal) * Mathf.Cos(angle_vertical) * distance
        );
        mainCamera.transform.LookAt(forCamera.transform);
    }

    void FixedUpdate()
    {
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

        if (Action == "Drift")
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
            (Action == "Drift" ? 15f : speed / 15) * Time.fixedDeltaTime
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
        // if ((Action == "Drift") != lastDrift)
        // {
        //     foreach (GameObject obj in ParticleSystems)
        //     {
        //         obj.SetActive(Action == "Drift");
        //     }
        // }
        lastDrift = Action == "Drift";
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
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0f, currentVelocity.z),
            new Vector3(targetVelocity.x, 0f, targetVelocity.z),
            0.1f
        );

        rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    void startAction(InputAction.CallbackContext context)
    {
        // Debug.Log("Rボタンが押された");
        Vector2 angle = inputActions.Player.Look.ReadValue<Vector2>();
        Vector2 angle_l = inputActions.Player.Move.ReadValue<Vector2>();
        if (Mathf.Abs(angle.x + angle_l.x) > 0.2f)
        {
            Action = "Drift";
            minDrift = ((angle.x + angle_l.x) > 0) ? 0.4f : -0.4f;
        }
        else
        {
            Action = "ChargeJump";
        }
        foreach (GameObject obj in ParticleSystems)
        {
            obj.SetActive(true);
        }
        Debug.Log(Action);
    }

    void endAction(InputAction.CallbackContext context)
    {
        // Debug.Log("Rボタンが押された");
        Action = "None";
        foreach (GameObject obj in ParticleSystems)
        {
            obj.SetActive(false);
        }
    }
}
