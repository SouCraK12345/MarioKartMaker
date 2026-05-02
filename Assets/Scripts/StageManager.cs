using System.Collections.Generic;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class GlobalVariables
{
    public static string playMode;
    public static bool playModeChanged;
}


public class StageManager : MonoBehaviour
{
    public GameObject MainPlayer;
    public GameObject RunningUI;
    public GameObject PauseUI;
    public List<GameObject> PauseButtons;
    private bool pauseMenuOpening;
    private int PauseButtonIndex;
    private InputSystem_Actions inputActions;
    void Start()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
        MainPlayer.GetComponent<CPU>().autoDriving = true;
        MainPlayer.GetComponent<CPU>().autoCamera = true;
        RunningUI.SetActive(false);
        GlobalVariables.playMode = "Opening";
        PauseButtonIndex = 1;
    }

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        GetComponent<PlayerInput>().neverAutoSwitchControlSchemes = true;
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.UI.Pause.performed += PauseAction;
        inputActions.UI.Navigate.performed += PauseNavigate;
        inputActions.UI.Submit.performed += PauseSubmit;
    }
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.UI.Pause.performed -= PauseAction;
        inputActions.UI.Navigate.performed -= PauseNavigate;
        inputActions.UI.Submit.performed -= PauseSubmit;
    }

    void Update()
    {
        if (GlobalVariables.playModeChanged)
        {
            GlobalVariables.playModeChanged = false;
            if (GlobalVariables.playMode == "FreeRun")
            {
                MainPlayer.GetComponent<CPU>().autoDriving = false;
                MainPlayer.GetComponent<CPU>().autoCamera = false;
                MainPlayer.GetComponent<PlayerController>().started = true;
                MainPlayer.GetComponent<PlayerController>().driving = true;
            }
        }
    }
    void PauseAction(InputAction.CallbackContext ctx)
    {
        PauseUI.SetActive(true);
        pauseMenuOpening = true;
        ChangeColorOfPauseButtons();
    }
    void PauseNavigate(InputAction.CallbackContext ctx)
    {
        if (!pauseMenuOpening) return;
        Vector2 direction = ctx.ReadValue<Vector2>();
        PauseButtonIndex += -((int)direction.y);
        ChangeColorOfPauseButtons();
    }
    void ChangeColorOfPauseButtons()
    {
        int index = 0;
        bool isButton0 = PauseButtonIndex % 2 == 0;
        foreach (var item in PauseButtons)
        {
            item.GetComponent<Animator>().SetBool("Selected", index == 0 ? isButton0 : !isButton0);
            index++;
        }
    }
    void PauseSubmit(InputAction.CallbackContext ctx)
    {
        if (PauseButtonIndex % 2 == 0)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
        else
        {
            PauseUI.SetActive(false);
            pauseMenuOpening = false;
        }
    }
}
