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
    public GameObject MainCamera;
    public GameObject MapCamera;
    public GameObject RunningUI;
    public GameObject PauseUI;
    public List<GameObject> PauseButtons;
    private bool mapOpening;
    private bool pauseMenuOpening;
    private int PauseButtonIndex;
    private InputSystem_Actions inputActions;
    public AudioClip ShowMapSound;
    public AudioClip CloseMapSound;
    private AudioSource audioSource;
    void Start()
    {
        Cursor.visible = false;
        MapCamera.SetActive(false);
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
        MainPlayer.GetComponent<CPU>().autoDriving = true;
        MainPlayer.GetComponent<CPU>().autoCamera = true;
        RunningUI.SetActive(false);
        GlobalVariables.playMode = "Opening";
        PauseButtonIndex = 1;
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.UI.Pause.performed += PauseAction;
        inputActions.UI.Navigate.performed += PauseNavigate;
        inputActions.UI.Submit.performed += PauseSubmit;
        inputActions.UI.Map.performed += SwitchMap;
    }
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.UI.Pause.performed -= PauseAction;
        inputActions.UI.Navigate.performed -= PauseNavigate;
        inputActions.UI.Submit.performed -= PauseSubmit;
        inputActions.UI.Map.performed -= SwitchMap;
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
    void SwitchMap(InputAction.CallbackContext ctx)
    {
        if (GlobalVariables.playMode == "FreeRun")
        {
            if (!mapOpening)
            {
                audioSource.PlayOneShot(ShowMapSound);
                Invoke("OpenMap", 0.5f);
            }
            else
            {
                audioSource.PlayOneShot(CloseMapSound);
                Invoke("CloseMap", 0.2f);
            }
        }
    }
    void OpenMap()
    {
        mapOpening = true;
        MainPlayer.GetComponent<PlayerController>().driving = false;
        MainPlayer.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        MainCamera.SetActive(false);
        Vector3 playerPosition = MainPlayer.transform.position;
        playerPosition.y = 1000;
        MapCamera.transform.position = playerPosition;
        MapCamera.SetActive(true);
    }
    void CloseMap()
    {
        mapOpening = false;
        MainPlayer.GetComponent<PlayerController>().driving = true;
        MainPlayer.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        MainCamera.SetActive(true);
        MapCamera.SetActive(false);
    }
    public void CancelAction(InputAction.CallbackContext ctx)
    {
        if (GlobalVariables.playMode == "FreeRun")
        {
            if (mapOpening)
            {
                audioSource.PlayOneShot(CloseMapSound);
                Invoke("CloseMap", 0.2f);
            }
        }
    }
}
