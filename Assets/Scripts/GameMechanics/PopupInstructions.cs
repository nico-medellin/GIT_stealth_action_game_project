using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PopupInstructions : MonoBehaviour
{
    public Canvas canvas;

    public TextMeshProUGUI inventoryCount;
    public TextMeshProUGUI prisonersRemainingCount;
    public TextMeshProUGUI stunInventoryCount;

    void Start() {
        Time.timeScale = 0f;
        inventoryCount.enabled = false;
        stunInventoryCount.enabled = false;
        prisonersRemainingCount.enabled = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (canvas != null) {
                canvas.gameObject.SetActive(false);
                Time.timeScale = 1f;
                inventoryCount.enabled = true;
                stunInventoryCount.enabled = true;
                prisonersRemainingCount.enabled = true;
                gameObject.SetActive(false);
            }
            else {
                Debug.LogError("PopupInstructions not set");
            }
        }
    }
}
