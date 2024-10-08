using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundScript : MonoBehaviour
{
    [SerializeField] private AudioClip[] selectAudio;
    [SerializeField] private AudioClip pressAudio;

    public void PlaySelectSound()
    {
        if(gameObject.GetComponent<Button>().interactable)
        {
            SoundManager.Instance.PlaySFXClip(selectAudio[Random.Range(0, selectAudio.Length - 1)], Camera.main.transform);
        }
    }

    public void PlayPressSound()
    {
        SoundManager.Instance.PlaySFXClip(pressAudio, Camera.main.transform);
    }

}
