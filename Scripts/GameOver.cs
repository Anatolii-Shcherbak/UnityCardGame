using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

    public Text scoreText = null;
    public Text KillsText = null;

    private void Awake()
    {
        scoreText.text = "Score: " + GameController.instance.playerScore.ToString();
        KillsText.text = "Demons Killed: " + GameController.instance.playersKill.ToString();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif

    }

    public void NewGame()
    {
        SceneManager.LoadScene(1);
    }

}
