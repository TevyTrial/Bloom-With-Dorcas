using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    
    // Update is called once per frame
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
        //Implement resume functionality
        Time.timeScale = 1f;
        GameIsPaused = false;
        //Hide pause menu UI here
        pauseMenuUI.SetActive(false);
    }

    void Pause()
    {
        //Implement pause functionality
        Time.timeScale = 0f;
        GameIsPaused = true;
        //Show pause menu UI here
        pauseMenuUI.SetActive(true);
    }
}
