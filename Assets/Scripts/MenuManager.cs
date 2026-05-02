using System;
using System.Collections.Generic;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [SerializeField] private List<Animator> Menus;
    [SerializeField] private List<Animator> HomeSelector;
    [SerializeField] private List<Animator> SoloPlayMenu;
    private int showingMenuIndex = 0;
    private int selectedIndex = 0;
    public AudioClip MovingCursor;
    public AudioClip Confirm;
    public AudioClip Cancel;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        inputActions = new InputSystem_Actions();
        audioSource = GetComponent<AudioSource>();
        selectedIndex = 0;
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.UI.Navigate.performed += NavigateAction;
        inputActions.UI.Submit.performed += SubmitAction;
        inputActions.UI.Cancel.performed += CancelAction;
    }
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.UI.Navigate.performed -= NavigateAction;
        inputActions.UI.Submit.performed -= SubmitAction;
        inputActions.UI.Cancel.performed -= CancelAction;
    }

    // Update is called once per frame
    void Update()
    {
        int fixedIndex = (selectedIndex % HomeSelector.Count + HomeSelector.Count) % HomeSelector.Count;
        int index = 0;
        var selectedMenu = new List<List<Animator>> { HomeSelector, SoloPlayMenu };
        foreach (var animator in selectedMenu[showingMenuIndex])
        {
            animator.SetBool("Selected", index == fixedIndex);
            index++;
        }
        index = 0;
        foreach (var animator in Menus)
        {
            animator.SetBool("ShowMenu", index == showingMenuIndex);
            index++;
        }
    }
    void NavigateAction(InputAction.CallbackContext ctx)
    {
        // ナビゲーション方向を取得
        Vector2 direction = ctx.ReadValue<Vector2>();
        selectedIndex += -((int)direction.y);
        audioSource.PlayOneShot(MovingCursor);
    }
    void SubmitAction(InputAction.CallbackContext ctx)
    {
        if (showingMenuIndex == 0)
        {
            if (selectedIndex == 0)
            {
                showingMenuIndex = 1;
                selectedIndex = 0;
            }
        }
        audioSource.PlayOneShot(Confirm);
    }
    void CancelAction(InputAction.CallbackContext ctx)
    {
        if (showingMenuIndex == 1)
        {
            showingMenuIndex = 0;
            selectedIndex = 0;
        }
        audioSource.PlayOneShot(Cancel);
    }
}
