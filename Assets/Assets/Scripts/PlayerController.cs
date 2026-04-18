using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

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

    void Update()
    {
        // Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        // Debug.Log(move);
        float accelerator = inputActions.Player.Accelerator.ReadValue<float>();
    }
}
