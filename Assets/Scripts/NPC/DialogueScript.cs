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
            ConversationManager.OnConversationEnded += LockCursorAgain;
        }
        else
        {
            Debug.LogError("ConversationManager.Instance or conversation is null!");
        }
    }

    private void LockCursorAgain() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isInConversation = false;
        ConversationManager.OnConversationEnded -= LockCursorAgain;
    }
}