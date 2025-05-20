using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenu : MonoBehaviour
{
    private CanvasGroup canvasGroup;

  // Reference to other UI elements you want to check before opening pause menu
    public GameObject[] blockingMenus; // Assign in Inspector
    private VGDPixelPioneersProject controls;
    private InputAction pauseAction;


    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        controls = new VGDPixelPioneersProject();
        pauseAction = controls.Player.Pause;
    }

    // void Start()
    // {
        
    // }
    void Update()
    {
        if (pauseAction.WasPressedThisFrame())
        {
            // If trying to open pause menu, first check if any other UI menus are active
            if (!canvasGroup.interactable)
            {
                foreach (GameObject menu in blockingMenus)
                {
                    Debug.Log($"{menu.name} is {(menu.activeSelf ? "active" : "inactive")}");
                    if (menu != null && menu.activeSelf)
                    {
                        Debug.Log("Pause menu blocked by: " + menu.name);
                        return;
                    }
                }
            }

            // Toggle pause menu visibility
            bool willOpen = !canvasGroup.interactable;

            Cursor.visible = willOpen;
            Cursor.lockState = willOpen ? CursorLockMode.None : CursorLockMode.Locked;

            canvasGroup.interactable = willOpen;
            canvasGroup.blocksRaycasts = willOpen;
            canvasGroup.alpha = willOpen ? 1f : 0f;
            Time.timeScale = willOpen ? 0f : 1f;
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown (KeyCode.Escape)) {
    //         if (canvasGroup.interactable) {
    //             Cursor.visible = false;
    //             Cursor.lockState = CursorLockMode.Locked;

    //             Time.timeScale = 1f;
    //             canvasGroup.interactable = false;
    //             canvasGroup.blocksRaycasts = false;
    //             canvasGroup.alpha = 0f;
    //         } else {
    //             Cursor.visible = true;
    //             Cursor.lockState = CursorLockMode.None;

    //             canvasGroup.interactable = true;
    //             canvasGroup.blocksRaycasts = true;
    //             canvasGroup.alpha = 1f;
    //             Time.timeScale = 0f;
    //         }
    //     }
    // }
}
