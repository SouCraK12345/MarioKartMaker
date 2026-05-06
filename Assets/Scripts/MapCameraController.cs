using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

public class MapCameraController : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    public float cameraMovingSpeed;
    // public GameObject StageManager;
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }
    void Osable()
    {
        inputActions.Disable();
    }
    void Update()
    {
        Vector2 dir = inputActions.UI.MapMove.ReadValue<Vector2>() * Time.deltaTime * 60 * cameraMovingSpeed;
        transform.position += new Vector3(dir.x, 0, dir.y);
    }
}
