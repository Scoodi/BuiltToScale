using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] private bool cutSceneNext = false;
    [SerializeField] private string cutsceneName;

    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip stageCompleteSound;
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
                SoundManager.Instance.PlaySFXClip(stageCompleteSound, Camera.main.transform);
                SceneManager.LoadScene(cutsceneName);
            } else
            {
                SoundManager.Instance.PlaySFXClip(levelCompleteSound, Camera.main.transform);
                LevelScript.Instance.LevelCompleted(LevelScript.Instance.currentLevel.nextLevel);
            }

        }
    }
}
