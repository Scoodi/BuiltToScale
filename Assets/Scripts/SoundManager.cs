using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    [Header("AudioSources")]
    [SerializeField] private AudioSource musicSourceOne;
    [SerializeField] private AudioSource musicSourceTwo;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource soundFXObject;

    [SerializeField] private AudioClip testClipOne;
    [SerializeField] private AudioClip testClipTwo;
    [SerializeField] private AudioClip testClipThree;

    [SerializeField] private bool isMainMenu = false;
    private float masterVolume;
    private float sfxVolume;
    private float musicVolume;

    private int activeSource = 1;

    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
        //SetSfxVolume(PlayerPrefs.GetFloat("SfxVolume"));
        SetMusicVolume(0.6f);
        SetSfxVolume(1.0f);
        //ambientSource.volume = 0.5f;
    }

    public void Start()
    {
       
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            PlaySFXClip(testClipOne, Camera.main.transform);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlaySFXClip(testClipTwo, Camera.main.transform);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlaySFXClip(testClipThree, Camera.main.transform);
        }
    }

    public void InitialiseMusic()
    {
        if (!isMainMenu)
        {
            musicSourceOne.clip = LevelScript.Instance.currentLevel.levelMusic;
        }
        musicSourceOne.Play();
        //ambientSource.Play();
    }
    // Ideally would want to make these through events which could be subscribed to SoundEvents and then the Settings menu could invoke/call them.
    // Jammin Jammin!
    public void SetMusicVolume(float vol)
    {
        float volumeToSet = vol * GetMasterVolume();
        musicSourceOne.volume = volumeToSet;
        musicSourceTwo.volume = volumeToSet;
        musicVolume = volumeToSet;
    }

    public void SetSfxVolume(float vol)
    {
        float volumeToSet = vol * GetMasterVolume();
        sfxVolume = volumeToSet;
        soundFXObject.volume = volumeToSet;
        // set the sfxSource volume in PP
    }

    public void SetMasterVolume(float vol)
    {
        masterVolume = vol;
        // set the sfxSource volume in PP
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }

    public void ChangeMusicOnLevelChange()
    {
        // figure this out :)
        if(musicSourceOne.clip == null && musicSourceTwo == null)
        {
            return;
        }    
        if (activeSource == 1 && musicSourceOne.clip != LevelScript.Instance.currentLevel.levelMusic)
        {
            musicSourceTwo.volume = 0f;
            musicSourceOne.DOFade(0f, 0.5f);
            musicSourceTwo.clip = LevelScript.Instance.currentLevel.levelMusic;
            musicSourceTwo.time = musicSourceOne.time;
            musicSourceTwo.DOFade(GetMusicVolume(), 0.5f).OnComplete(() => musicSourceOne.Stop());
            musicSourceTwo.Play();
            activeSource = 2;
        }
        else if (activeSource == 2 && musicSourceTwo.clip != LevelScript.Instance.currentLevel.levelMusic)
        {
            musicSourceOne.volume = 0f;
            musicSourceTwo.DOFade(0f, 0.5f);
            musicSourceOne.clip = LevelScript.Instance.currentLevel.levelMusic;
            musicSourceOne.time = musicSourceTwo.time;
            musicSourceOne.DOFade(GetMusicVolume(), 0.5f).OnComplete(() => musicSourceTwo.Stop());
            musicSourceOne.Play();
            activeSource = 1;
        }
        
    }

    public void PlaySFXClip(AudioClip audioClip, Transform spawnTransform)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = sfxVolume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}
