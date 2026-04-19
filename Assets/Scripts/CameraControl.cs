using Unity.VisualScripting;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject mainCamera;
    private float angle_horizontal = 0;
    private float angle_vertical = 0.4f;
    private float distance = 10f; // カメラの距離
    private InputSystem_Actions inputActions;

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

    // Update is called once per frame
    void Update()
    {
        Vector2 angle = inputActions.Player.Look.ReadValue<Vector2>();
        Vector2 angle_l = inputActions.Player.Move.ReadValue<Vector2>();
        angle_horizontal += (angle.x + angle_l.x) / -50;
        angle_vertical += angle.y / -50;
        mainCamera.transform.position = new Vector3(
            transform.position.x + Mathf.Cos(angle_horizontal) * Mathf.Cos(angle_vertical) * distance,
            transform.position.y + Mathf.Sin(angle_vertical) * distance,
            transform.position.z + Mathf.Sin(angle_horizontal) * Mathf.Cos(angle_vertical) * distance
        );
        mainCamera.transform.LookAt(this.transform);
    }
}
