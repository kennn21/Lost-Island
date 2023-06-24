using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    //GameObjects
    public GameObject PauseScreen;
    public GameObject InventoryScreen;
    public GameObject SettingsScreen;

    //UI States
    public bool isPaused;
    public bool isInventoryOpen;

    //Get player inputs
    [Header("Keybinds")]
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode openInventoryKey = KeyCode.Tab;

    void Update()
    {
        PlayerInputs();
    }
    
    private void PlayerInputs()
    {
        //PAUSE MENU
        if (Input.GetKeyDown(pauseKey))
        {
            if(!isPaused && !isInventoryOpen)
            {
                PauseGame();
                return;
            }
            if(!isPaused && isInventoryOpen)
            {
                CloseInventory();
                return;
            }
            if(isPaused)
            {
                ResumeGame();
                return;
            }
            return;
        }

        //INVENTORY MENU
        if(Input.GetKeyDown(openInventoryKey))
        {
            if(!isInventoryOpen && !isPaused)
            {
                OpenInventory();
                return;
            }
            if(isInventoryOpen && !isPaused)
            {
                CloseInventory();
                return;
            }
            return;
        }
    }

    //Pause Menu Functions
    public void PauseGame()
    {
        UnlockCursor();
        Time.timeScale = 0;
        PauseScreen.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        CloseSettings();
        LockCursor();
        Time.timeScale = 1;
        PauseScreen.SetActive(false);
        isPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game. This will only work in the build version of the game, not in the unity preview.");
        Application.Quit();
    }

    //Inventory Menu Functions
    public void CloseInventory()
    {
        isInventoryOpen = false;
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
    }

    public void OpenSettings()
    {
        PauseScreen.SetActive(false);
        SettingsScreen.SetActive(true);
    }

    public void CloseSettings()
    {
        PauseScreen.SetActive(true);
        SettingsScreen.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene("MainScene");
    }

    //Misc Functions
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }



}
