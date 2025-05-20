using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextLevelMenu : MonoBehaviour
{
    private GameObject player;
    private PlayerController playerController;
    public Button continueButton;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerController>();
        continueButton.onClick.AddListener(() => NextLevelLoad());
    }

    public void NextLevelLoad()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(currentSceneIndex);
        Debug.Log(SceneManager.sceneCountInBuildSettings - 1);

        if (currentSceneIndex <= SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
            playerController.ResetScreen();
        }
    }
}
