using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] private bool cutSceneNext = false;
    [SerializeField] private string cutsceneName;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Level Complete!");
            if (cutSceneNext)
            {
                SceneManager.LoadScene(cutsceneName);
            } else
            {
                LevelScript.Instance.LevelCompleted(LevelScript.Instance.currentLevel.nextLevel);
            }

        }
    }
}
