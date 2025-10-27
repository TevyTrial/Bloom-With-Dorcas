using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public enum Location { Home, Shop, Garden };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(gameObject);
        }
        else if (Instance == null)  
        {
            Instance = this;
        }
        // Make persistent across scenes
        DontDestroyOnLoad(gameObject);
    }

    public void SwitchLocation(Location locationToSwitch){

        string sceneName = locationToSwitch.ToString(); 
        SceneManager.LoadScene(sceneName);

    }


}
