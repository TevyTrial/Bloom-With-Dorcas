using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController cameraController;
    
    private void Start()
    {
        // Auto-find controllers if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
        
        // Make sure the game starts unpaused
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        
        // Re-enable player and camera
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        
        // Disable player and camera
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
    }
    
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}