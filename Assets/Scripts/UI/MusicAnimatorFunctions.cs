using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicAnimatorFunctions : MonoBehaviour
{
    [SerializeField] MusicMenuButtonController musicMenuButtonController;
    public bool disableOnce;

    void PlaySound(AudioClip whichSound){
        if(!disableOnce){
            musicMenuButtonController.audioSource.PlayOneShot (whichSound);
        }else{
            disableOnce = false;
        }
    }
}
