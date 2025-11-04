using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YesNoPrompt : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI promptText;
    Action OnYesSelected = null;

    public void CreatePrompt (string message, Action OnYesSelected) {
        // Set the action
        this.OnYesSelected = OnYesSelected;
        // Set the prompt message
        promptText.text = message;
    }

    public void Answer(bool yes) {
        // Execute the action if yes
        if(yes && OnYesSelected != null) {
            OnYesSelected();
        }
        //Reset the action
        OnYesSelected = null;

        gameObject.SetActive(false);
    }
}
