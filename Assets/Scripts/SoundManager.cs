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
        //ambientSource.volume = 0.5f;
    }

    public void Start()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume"));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
        SetSfxVolume(PlayerPrefs.GetFloat("SfxVolume"));
    }

    public void Update()
    {
        
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
        PlayerPrefs.SetFloat("MusicVolume", volumeToSet);
    }

    public void SetSfxVolume(float vol)
    {
        float volumeToSet = vol * GetMasterVolume();
        soundFXObject.volume = volumeToSet;
        PlayerPrefs.SetFloat("SfxVolume", volumeToSet);
    }

    public void SetMasterVolume(float vol)
    {
        float sfxVolumeToSet = vol * GetSfxVolume();
        float musicVolumeToSet = vol * GetMusicVolume();
        soundFXObject.volume = sfxVolumeToSet;
        musicSourceOne.volume = musicVolumeToSet;
        musicSourceTwo.volume = musicVolumeToSet;
        PlayerPrefs.SetFloat("MasterVolume", vol);
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume");
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume");
    }

    public float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat("SfxVolume");
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

        audioSource.volume = GetSfxVolume();

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}
