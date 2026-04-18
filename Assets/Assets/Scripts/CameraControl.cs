using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private GameObject mainCamera;
    public float angle;
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
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        angle += inputActions.Player.Look.ReadValue<Vector2>().x / 50;
        mainCamera.transform.localPosition = new Vector3(Mathf.Sin(angle) * 5, 1f, Mathf.Cos(angle) * 5);
        mainCamera.transform.LookAt(this.transform);
    }
}
