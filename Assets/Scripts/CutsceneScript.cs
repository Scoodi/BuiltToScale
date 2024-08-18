using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CutsceneScript : MonoBehaviour
{
    public InputActionAsset actions;

    private InputAction placeAction;
    private InputAction placeMouseAction;

    public DialogueSO[] dialogueSOs;
    private int currentDialogue = 0;

    public TMP_Text nameDisplay;
    public TMP_Text speechDisplay;

    public string nextLevel = "Uninitialised";
    void Awake()
    {
        InitialiseInputActions();
        LoadDialogue(dialogueSOs[0]);
    }

    private void InitialiseInputActions()
    {
        actions.FindActionMap("Platforming").Enable();
        placeAction = actions.FindActionMap("Platforming").FindAction("Place");
        placeMouseAction = actions.FindActionMap("Platforming").FindAction("PlaceMouse");
        placeAction.performed += _ => AdvanceDialogue();
        placeMouseAction.performed += _ => AdvanceDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadDialogue (DialogueSO dialogueToLoad)
    {
        nameDisplay.text = dialogueToLoad.SpeakerName;
        speechDisplay.text = dialogueToLoad.SpeakerText;
    }

    void AdvanceDialogue ()
    {
        if (currentDialogue < dialogueSOs.Length - 1)
        {
            currentDialogue++;
            LoadDialogue(dialogueSOs[currentDialogue]);
        } else
        {
            SceneManager.LoadScene(nextLevel);
        }

    }
}
