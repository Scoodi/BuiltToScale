using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public int nextStage;
    public TMP_Text nameDisplay;
    public TMP_Text speechDisplay;

    public string nextCutscene = "Uninitialised";
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
        if (dialogueToLoad.audio != null)
        {
            SoundManager.Instance.PlaySFXClip(dialogueToLoad.audio, Camera.main.transform);
        }
    }

    void AdvanceDialogue ()
    {
        if (SceneManager.GetActiveScene().name.Contains("Cutscene"))
        {
            if (currentDialogue < dialogueSOs.Length - 1)
            {
                currentDialogue++;
                LoadDialogue(dialogueSOs[currentDialogue]);
            }
            else if (PlayerPrefs.GetInt("CutsceneMode") == 0)
            {
                PlayerPrefs.SetInt("CurrentStage", nextStage);
                SceneManager.LoadScene(nextLevel);
            } else if (nextCutscene != "Uninitialised")
            {
                SceneManager.LoadScene(nextCutscene);
            } else
            {
                PlayerPrefs.SetInt("CutsceneMode", 0);
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
