using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

public class MapCameraController : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    public StageManager StageManagerObj;
    public GameObject StartFreeRunObj;
    private bool StartFreeRunVisible;
    public float cameraMovingSpeed;
    public GameObject MapIconContainer;
    public BlackOutMaker BlackOutVideoPlayer;
    private Transform[] children; // 12行目: 型を指定して宣言のみ行う
    private int selectedChildrenIndex;
    private string closestPlace;
    void Start()
    {
        var childIndex = 0;
        children = new Transform[MapIconContainer.transform.childCount];
        // 子オブジェクトを順番に配列に格納
        foreach (Transform child in MapIconContainer.transform)
        {
            children[childIndex++] = child;
        }
    }
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.UI.Cancel.performed += CancelAction;
    }
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.UI.Cancel.performed -= CancelAction;
    }
    void Update()
    {
        if (StartFreeRunVisible)
        {
            transform.position = new Vector3(
                transform.position.x * 0.7f + children[selectedChildrenIndex].position.x * 0.3f,
                transform.position.y, // 元のy座標を維持
                transform.position.z * 0.7f + children[selectedChildrenIndex].position.z * 0.3f
            );
        }
        else
        {
            Vector2 dir = inputActions.UI.MapMove.ReadValue<Vector2>() * Time.deltaTime * 60 * cameraMovingSpeed;
            transform.position += new Vector3(dir.x, 0, dir.y);
        }
    }
    public string OnSubmit()
    {
        if (StartFreeRunVisible)
        {
            BlackOutVideoPlayer.BlackOut();
            StartFreeRunVisible = false;
            Invoke("BlackOpenPlay", 1.5f);
            inputActions.UI.Cancel.performed -= CancelAction;
            return closestPlace;
        }
        else
        {
            StartFreeRunVisible = true;
            StartFreeRunObj.SetActive(true);
            float shortest_distance = float.MaxValue;
            int index = 0;
            foreach (var i in children)
            {
                float distance = Vector3.Distance(transform.position, i.position);
                if (distance < shortest_distance)
                {
                    selectedChildrenIndex = index;
                    shortest_distance = distance;
                    closestPlace = i.name;
                }
                index++;
            }
            return "";
        }
    }
    void CancelAction(InputAction.CallbackContext ctx)
    {
        if (StartFreeRunVisible)
        {
            HideFreeRunDialog();
        }
        else
        {
            StageManagerObj.CancelAction(ctx);
        }
    }

    public void HideFreeRunDialog()
    {
        StartFreeRunVisible = false;
        StartFreeRunObj.SetActive(false);
    }

    void BlackOpenPlay()
    {
        BlackOutVideoPlayer.BlackOpen();
        inputActions.UI.Cancel.performed += CancelAction;
    }
}
