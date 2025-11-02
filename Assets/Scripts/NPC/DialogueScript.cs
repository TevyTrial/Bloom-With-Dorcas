using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DialogueEditor;

public class DialogueScript : MonoBehaviour
{
    public NPCConversation conversation;
    private bool isInConversation = false;

    public bool IsInConversation() 
    {
        return isInConversation;
    }

    public void StartConversation() 
    {
        Debug.Log("Starting conversation");
        
        if (ConversationManager.Instance != null && conversation != null)
        {
            isInConversation = true;
            UIManager.Instance.HideInteractTooltip();
            ConversationManager.Instance.StartConversation(conversation);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ConversationManager.OnConversationEnded += OnConversationEnded;
        }
        else
        {
            Debug.LogError("ConversationManager.Instance or conversation is null!");
        }
    }

    private void OnConversationEnded() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isInConversation = false;
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }
}