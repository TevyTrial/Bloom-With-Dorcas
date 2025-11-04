using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    /*
    public static SceneTransitionManager Instance;

    //The scenes the player can enter
    public enum Location { Garden, Home, Shop}
    public Location currentLocation;

    //The player's transform
    Transform playerPoint;
    
    //Check if the screen has finished fading out
    bool screenFadedOut; 

    private void Awake()
    {
        //If there is more than 1 instance, destroy GameObject
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; // Important: stop execution after destroying
        }
        else
        {
            //Set the static instance to this instance
            Instance = this; 
        }

        //Make the gameobject persistent across scenes
        DontDestroyOnLoad(gameObject);

        //OnLocationLoad will be called when the scene is loaded
        SceneManager.sceneLoaded += OnLocationLoad;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed
        SceneManager.sceneLoaded -= OnLocationLoad;
        
        // Clear the static instance if this is the current instance
        if(Instance == this)
        {
            Instance = null;
        }
    }

    //Switch the player to another scene
    public void SwitchLocation(Location locationToSwitch)
    {
        //Call a fadeout
        UIManager.Instance.FadeOutScreen();
        screenFadedOut = false;
        StartCoroutine(ChangeScene(locationToSwitch)); 
    }

    IEnumerator ChangeScene(Location locationToSwitch)
    {
        //Find the player before scene transition
        playerPoint = FindAnyObjectByType<PlayerController>()?.transform;
        
        if(playerPoint != null)
        {
            //Disable the player's CharacterController component
            CharacterController playerCharacter = playerPoint.GetComponent<CharacterController>();
            if(playerCharacter != null)
            {
                playerCharacter.enabled = false;
            }
        }
        
        //Wait for the scene to finish fading out before loading the next scene
        while (!screenFadedOut)
        {
            yield return new WaitForSeconds(0.1f); 
        }

        //Reset the boolean
        screenFadedOut = false;
        UIManager.Instance.ResetFadeDefault();
        
        //Load scene asynchronously to avoid threading issues
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(locationToSwitch.ToString());
        
        //Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    //Called when the screen has faded out
    public void OnFadeOutComplete()
    {
        screenFadedOut = true;
    }

    //Called when a scene is loaded
    public void OnLocationLoad(Scene scene, LoadSceneMode mode)
    {
        // Safety check: make sure this object still exists
        if(this == null || Instance != this)
        {
            return;
        }

        //The location the player is coming from when the scene loads
        Location oldLocation = currentLocation;

        //Get the new location by converting the string of our current scene into a Location enum value
        Location newLocation = (Location) Enum.Parse(typeof(Location), scene.name);

        //If the player is not coming from any new place, stop executing the function
        if (currentLocation == newLocation) return; 

        StartCoroutine(PositionPlayer(oldLocation, newLocation));
    }

    IEnumerator PositionPlayer(Location oldLocation, Location newLocation)
    {
        //Wait a frame for scene to fully load
        yield return null;

        //Re-find the player in the new scene
        playerPoint = FindAnyObjectByType<PlayerController>()?.transform;
        
        if(playerPoint == null)
        {
            Debug.LogError("Player not found in the new scene!");
            yield break;
        }

        //Find the start point
        Transform startPoint = LocationManager.Instance?.GetPlayerStartingPosition(oldLocation);

        if(startPoint == null)
        {
            Debug.LogError($"No start point found for entering from {oldLocation}");
            yield break;
        }

        //Disable the player's CharacterController component
        CharacterController playerCharacter = playerPoint.GetComponent<CharacterController>();
        if(playerCharacter != null)
        {
            playerCharacter.enabled = false; 

            //Change the player's position to the start point
            playerPoint.position = startPoint.position;
            playerPoint.rotation = startPoint.rotation;

            //Wait another frame before re-enabling
            yield return null;

            //Re-enable player character controller so he can move
            playerCharacter.enabled = true;
        }

        //Save the current location that we just switched to
        currentLocation = newLocation; 
    }*/
}