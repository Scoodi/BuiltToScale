using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Inventory;

public class MenuScript : MonoBehaviour
{
    public enum MenuScreens
    {
        Tutorial,
        Settings,
        Credits,
        Difficulty,
        LevelSelect
    }
    [SerializeField] private GameObject currentScreen;

    [SerializeField] private GameObject[] screens;

    [SerializeField] private string[] stageIntroCutscenes;

    void Awake()
    {
        InitialisePlayerPrefs();
    }

    void InitialisePlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("Difficulty"))
        {
            PlayerPrefs.SetInt("Difficulty", 1);
        }
        if (!PlayerPrefs.HasKey("LevelToLoad"))
        {
            PlayerPrefs.SetString("LevelToLoad", "GameScene");
        }
        if (!PlayerPrefs.HasKey("CurrentStage"))
        {
            PlayerPrefs.SetInt("CurrentStage", 0);
        }
        if (!PlayerPrefs.HasKey("CutsceneMode"))
        {
            PlayerPrefs.SetInt ("CutsceneMode", 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCutsceneMode(bool cutsceneMode)
    {
        if (cutsceneMode)
        {
            PlayerPrefs.SetInt("CutsceneMode", 1);
        } else
        {
            PlayerPrefs.SetInt("CutsceneMode", 0);
        }

    }

    public void SetStageToLoad(int stageIndex)
    {
        PlayerPrefs.SetInt("CurrentStage", stageIndex);
    }

    public void SwitchToMenu (string menu)
    {
        for (int i = 0; i < screens.Length-1; i++)
        {
            if (((MenuScreens)i).ToString() != menu)
            {
                screens[i].SetActive(false);
            } else
            {
                screens[i].SetActive(true);
            }
        }
    }

    public void StartGame (int difficulty)
    {
        PlayerPrefs.SetInt("Difficulty", difficulty);
        //SceneManager.LoadScene("GameScene");
        SceneManager.LoadScene("Cutscene 1 - Opening");
    }

    public void StartCutscene(int cutsceneStage)
    {
        SetCutsceneMode(true);
        SceneManager.LoadScene(stageIntroCutscenes[cutsceneStage]);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
