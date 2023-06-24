using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public string sceneName;
    public GameObject LoadingScreen;

    public void StartGame()
    {
        LoadingScreen.SetActive(true);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game. This will only work in the build version of the game, not in the unity preview.");
        Application.Quit();
    }
}
