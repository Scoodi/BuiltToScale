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
    
    private bool cutsceneDone = false;

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
        Debug.Log(cutsceneDone);
        if (cutsceneDone)
        {
            //return;
        }
        if (SceneManager.GetActiveScene().name.Contains("Cutscene"))
        {
            if (currentDialogue < dialogueSOs.Length - 1)
            {
                currentDialogue++;
                Debug.Log(currentDialogue);
                LoadDialogue(dialogueSOs[currentDialogue]);
            }
            else if (PlayerPrefs.GetInt("CutsceneMode") == 0)
            {
                Debug.Log("Loading next level");
                PlayerPrefs.SetInt("CurrentStage", nextStage);
                cutsceneDone = true;
                SceneManager.LoadScene(nextLevel);
            } else if (nextCutscene != "Uninitialised")
            {
                SceneManager.LoadScene(nextCutscene);
                cutsceneDone = true;
            } else
            {
                PlayerPrefs.SetInt("CutsceneMode", 0);
                cutsceneDone = true;
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
