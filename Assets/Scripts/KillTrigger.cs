using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip fallOffSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {

            StartCoroutine("WaitForFallSound");

        }

        if (collision.CompareTag("Block"))
        {
            Destroy(collision.gameObject);
        }
    }

    IEnumerator WaitForFallSound()
    {
        SoundManager.Instance.PlaySFXClip(fallOffSound, Camera.main.transform);
        yield return new WaitForSeconds(fallOffSound.length);
        LevelScript.Instance.LevelCompleted(LevelScript.Instance.currentLevel);
    }
}
