using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Diagnostics;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private Rigidbody rb;
    public GameObject mainCamera;
    public float acceleration = 20f;
    public float speedDivisor = 1.033f;
    public float speed = 0;
    public float rotationSpeed = 2f;
    [SerializeField] private float chargeJumpThreshold = 1f;
    [SerializeField] private float chargeJumpReleaseForce = 10f;
    [SerializeField] private float driftBoostAmount = 10f;
    private int touchingStages = 0;
    private int touchingObjects = 0;
    private bool lastDrift = false;
    private float minDrift = 0;
    public string Action = "None";
    private float chargeJumpStartTime = -1f;
    private float driftStartTime = -1f;
    private float driftAngleSum = 0f; // ドリフト中の累積角度
    [SerializeField] private float driftBoostAngleThreshold = 60f; // ドリフト加速の角度閾値（度）
    [SerializeField] private List<GameObject> ParticleSystems;
    public int jumpCount = 0;
    public GameObject forCamera;
    private float angle_horizontal = 0;
    private float angle_vertical = 0.4f;
    private float distance = 6f; // カメラの距離
    public Animator model;
    public Transform model_transform;
    private int wallrun_direction = 0;
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
            if (Action == "Drift" && Math.Abs(angle.x + angle_l.x) < 0.4)
            {
                angle_horizontal += minDrift / -35 * (Time.deltaTime * 60);
            }
            else
            {
                angle_horizontal += (angle.x + angle_l.x) / -35 * (Time.deltaTime * 60);
            }
        }
        angle_vertical += angle.y / -50;
        mainCamera.transform.position = new Vector3(
            transform.position.x + Mathf.Cos(angle_horizontal) * Mathf.Cos(angle_vertical) * distance,
            transform.position.y + Mathf.Sin(angle_vertical) * distance,
            transform.position.z + Mathf.Sin(angle_horizontal) * Mathf.Cos(angle_vertical) * distance
        );
        mainCamera.transform.LookAt(forCamera.transform);

        // ドリフト中の累積角度を加算
        if (Action == "Drift")
        {
            driftAngleSum += Mathf.Abs(angle.x + angle_l.x) * (Time.deltaTime * 60); // 1フレームごとの角度変化を加算
        }

        int phase = 0;
        if ((Time.time - chargeJumpStartTime >= chargeJumpThreshold && Action == "ChargeJump") ||
            (Action == "Drift" && driftAngleSum >= driftBoostAngleThreshold))
        {
            phase = 1;
        }
        string[] colors = { "#00EDFF", "#ff2600" };

        foreach (GameObject obj in ParticleSystems)
        {
            var ps = obj.GetComponent<ParticleSystem>();
            var main = ps.main;

            if (ColorUtility.TryParseHtmlString(colors[phase], out var color))
            {
                main.startColor = color;
            }
        }


        if (Action == "WallRun")
        {
            if (wallrun_direction == 1)
            {
                rb.AddForce(transform.right * 5, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(transform.right * -5, ForceMode.Acceleration);
            }

            RaycastHit hit;

            // 壁がなければ壁走りを終了
            if (!Physics.Raycast(transform.position, transform.right, out hit, 1f) && !Physics.Raycast(transform.position, -transform.right, out hit, 1f))
            {
                stopWallRun();
            }
        }
        if (jumpCount > 0 && Action != "WallRun")
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.right, out hit, 1f))
            {
                if (hit.collider.gameObject.name != "Player")
                {
                    startWallRun(1);
                }
            }
            else if (Physics.Raycast(transform.position, -transform.right, out hit, 1f))
            {
                if (hit.collider.gameObject.name != "Player")
                {
                    startWallRun(2);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (touchingObjects > 0)
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
        }

        if (Action == "Drift")
        {
            rb.AddForce(transform.forward.normalized * speed / 1.5f);
        }
        else
        {
            move(transform.forward.normalized * speed);
        }

        // ウォールラン中にスティック上下で上下移動
        if (Action == "WallRun")
        {
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            float verticalInput = moveInput.y;
            if (Mathf.Abs(verticalInput) > 0.05f)
            {
                float wallRunVerticalForce = 8f; // 必要に応じて調整
                rb.AddForce(transform.up * verticalInput * wallRunVerticalForce, ForceMode.Acceleration);
            }
        }

        if (Action != "WallRun")
        {
            // 方向の補正
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0f;

            // if (forward.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(forward);

            // 補間（0〜1の割合）
            Quaternion newRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                (Action == "Drift" ? 15f : speed / 15) * Time.fixedDeltaTime
            );

            rb.MoveRotation(newRotation);
        }
        if (touchingStages > 0 || (Action == "WallRun"))
        {
            speedDivisor = 1.033f;
        }
        else
        {
            speedDivisor = 1.066f;
        }
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
        touchingObjects++;
        jumpCount = 0;
    }

    void OnCollisionExit(Collision collision)
    {
        touchingObjects--;
    }

    void startAction(InputAction.CallbackContext context)
    {
        if (touchingObjects == 0)
        {
            return;
        }
        if (Action == "WallRun")
        {
            Action = "None";
            stopWallRun();
            jumpCount++;
            rb.AddForce(transform.up * chargeJumpReleaseForce, ForceMode.Impulse);
            if (wallrun_direction == 1)
            {
                model.SetTrigger("JumpAction_Left");
                rb.AddForce(-transform.right * 30, ForceMode.Impulse);
            }
            else
            {
                model.SetTrigger("JumpAction_Right");
                rb.AddForce(transform.right * 30, ForceMode.Impulse);
            }
        }
        // Debug.Log("Rボタンが押された");
        Vector2 angle = inputActions.Player.Look.ReadValue<Vector2>();
        Vector2 angle_l = inputActions.Player.Move.ReadValue<Vector2>();
        if (Mathf.Abs(angle.x + angle_l.x) > 0.2f)
        {
            Action = "Drift";
            minDrift = ((angle.x + angle_l.x) > 0) ? 0.4f : -0.4f;
            chargeJumpStartTime = -1f;
            driftStartTime = Time.time;
        }
        else
        {
            Action = "ChargeJump";
            chargeJumpStartTime = Time.time;
            driftStartTime = -1f;
        }
        foreach (GameObject obj in ParticleSystems)
        {
            obj.SetActive(true);
        }
    }

    void endAction(InputAction.CallbackContext context)
    {
        Vector2 angle = inputActions.Player.Look.ReadValue<Vector2>();
        Vector2 angle_l = inputActions.Player.Move.ReadValue<Vector2>();
        // Debug.Log("Rボタンが押された");
        bool releasedDrift = Action == "Drift"
            && driftStartTime >= 0f
            && driftAngleSum >= driftBoostAngleThreshold;
        bool releasedChargedJump = Action == "ChargeJump"
            && chargeJumpStartTime >= 0f
            && Time.time - chargeJumpStartTime >= chargeJumpThreshold;

        if (releasedDrift)
        {
            speed += driftBoostAmount;
        }

        if (releasedChargedJump)
        {
            jumpCount++;
            rb.AddForce(transform.up * chargeJumpReleaseForce, ForceMode.Impulse);
            if (angle.x + angle_l.x > 0.2f)
            {
                model.SetTrigger("JumpAction_Right");
                rb.AddForce(transform.right * 30, ForceMode.Impulse);
            }
            if (angle.x + angle_l.x < -0.2f)
            {
                model.SetTrigger("JumpAction_Left");
                rb.AddForce(-transform.right * 30, ForceMode.Impulse);
            }
        }

        chargeJumpStartTime = -1f;
        driftStartTime = -1f;
        driftAngleSum = 0f; // リセット
        Action = "None";
        foreach (GameObject obj in ParticleSystems)
        {
            obj.SetActive(false);
        }
    }

    void startWallRun(int direction)
    {
        wallrun_direction = direction;
        Action = "WallRun";
        rb.useGravity = false;
        model.SetInteger("WallRun", direction);
        UnityEngine.Debug.Log("WallRun Start: " + direction);
    }

    void stopWallRun()
    {
        model.SetInteger("WallRun", 0);
        Action = "None";
        rb.useGravity = true;
        UnityEngine.Debug.Log("WallRun Stop");
    }
}
