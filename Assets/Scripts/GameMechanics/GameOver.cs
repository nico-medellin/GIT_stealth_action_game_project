using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    public bool gameOver = false;
    public GameObject player;
    public Canvas canvas;
    public TextMeshProUGUI inventoryCount;
    public TextMeshProUGUI prisonersRemainingCount;
    public TextMeshProUGUI stunInventoryCount;
    void Start()
    {
        canvas.gameObject.SetActive(false);

    }

    void Update()
    {
        gameOver = player.GetComponent<PlayerController>().gameOver;
        if (gameOver == true ) {
            canvas.gameObject.SetActive(true);
            inventoryCount.enabled = false;
            stunInventoryCount.enabled = false;
            prisonersRemainingCount.enabled = false;
            Time.timeScale = 0f;
            if (Input.GetKeyDown(KeyCode.Return)) {
                string currentSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentSceneName);
            }
        }
    }
}
