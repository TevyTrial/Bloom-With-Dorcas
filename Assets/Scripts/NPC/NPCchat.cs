using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCchat : MonoBehaviour
{
    public GameObject Character;
    public GameObject subtitle;
    public float interactionDistance = 6f;

    private Camera cam;
    private bool isTalking = false;

    void Start()
    {
        cam = Camera.main;
        if(subtitle != null)
            subtitle.SetActive(false);
    }

    void Update()
    {
        // Only handle dialogue input, not tooltip
        if (Input.GetKeyDown(KeyCode.T) && !isTalking)
        {
            RaycastHit hit;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                if (hit.collider.CompareTag("NPC") && hit.collider.gameObject == gameObject)
                {
                    StartDialogue();
                }
            }
        }
    }

    public void StartDialogue()
    {
        isTalking = true;
        if (subtitle != null)
        {
            subtitle.SetActive(true);
            subtitle.GetComponent<TextMeshProUGUI>().text = "Hello, Nice to meet you!";
            StartCoroutine(HideSubtitleAfterDelay(2f));
        }
    }

    IEnumerator HideSubtitleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (subtitle != null)
        {
            subtitle.SetActive(false);
        }
        isTalking = false;
    }
}