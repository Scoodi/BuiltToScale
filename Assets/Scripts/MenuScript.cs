using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void StartGame ()
    {
        SceneManager.LoadScene("GameScene");
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
