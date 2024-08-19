using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public enum MenuScreens
    {
        Options,
        Tutorial,
        Settings,
        Credits
    }
    [SerializeField] private GameObject currentScreen;

    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject creditsScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
