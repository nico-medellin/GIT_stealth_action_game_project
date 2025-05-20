using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameStarter : MonoBehaviour
{


    public void StartGame()

    {
        SceneManager.LoadScene("Level_1"); //starts the first level.
        Time.timeScale = 1f;


    }

}
