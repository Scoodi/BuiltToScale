using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using TMPro;

public class LevelScript : MonoBehaviour
{
    [SerializeField] private LevelSO firstLevel;
    public static LevelScript Instance { get; private set; }
    public LevelSO currentLevel;
    [SerializeField] private GameObject currentLevelPrefab;
    [SerializeField] private float maxVelocity = 0.1f;
    [SerializeField] private Image fadeInOutOverlay;
    [SerializeField] private float fadeInOutTime = 1f;
    [SerializeField] private Camera levelCamera;
    [SerializeField] private GameObject pauseMenu;
    public bool gamePaused = false;

    [SerializeField] private bool isFirstLevel = true;
    [SerializeField] private bool isCheckingPieces = false;
    //TODO add reference to inventory

    [SerializeField] private GameObject playModeButton;
    [SerializeField] private Button buildModeButton;

    [SerializeField] private TMP_Text countdownText;

    [Header("Countdown Colors")]
    [SerializeField] private Color countdownColor;
    [SerializeField] private Color piecesSettledColor;
    [SerializeField] private Color piecesNotSettledColor;

    [Header("AudioClips")]
    [SerializeField] private AudioClip penClickSound;
    [SerializeField] private AudioClip piecesNotSetSound;
    [SerializeField] private AudioClip piecesSetSound;
    [SerializeField] private AudioClip[] countdownSound;



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PlatformerCharacterScript.Instance.swapModeAction.performed += _ => TryGoToPlatforming();
        PlatformerCharacterScript.Instance.pauseAction.performed += _ => PauseGame();
        if (isFirstLevel)
        {
            LevelCompleted(firstLevel);
        }
        SoundManager.Instance.InitialiseMusic();
        InventoryUI.Instance.inventory.LoadBlocks();

    }
    public void TryGoToPlatforming()
    {
        if (PlatformerCharacterScript.Instance.building)
        {
            SoundManager.Instance.PlaySFXClip(penClickSound, Camera.main.transform);
            StartCoroutine("SettleCountdown");
        }
        else
        {
            Debug.Log("Restart level to rebuild");
        };
    }

    bool CheckIfSettled(int cdNum)
    {
        bool settled = true;
        foreach (Rigidbody2D rb in InventoryUI.Instance.placedRBs)
        {
            if (rb.velocity.magnitude > maxVelocity)
            {
                isCheckingPieces = false;
                Debug.Log("Pieces not settled!");
                countdownText.color = piecesNotSettledColor;
                countdownText.text = "Pieces not settled!";
                SoundManager.Instance.PlaySFXClip(piecesNotSetSound, Camera.main.transform);
                countdownText.DOFade(0, 3f);
                settled = false; break;
            }
        }
        if(settled)
        {
            SoundManager.Instance.PlaySFXClip(countdownSound[cdNum], Camera.main.transform);
        }    
            

        return settled;
    }

    IEnumerator SettleCountdown()
    {
        if (!isCheckingPieces)
        {
            isCheckingPieces = true;
            for (int i = 0; i < 3; i++)
            {
                countdownText.color = countdownColor;
                countdownText.DOKill();
                countdownText.text = (3 - i).ToString();
                if (!CheckIfSettled(2-i))
                {
                    StopCoroutine("SettleCountdown");
                }
                Debug.Log("Waited " + i + " seconds");
                yield return new WaitForSeconds(1f);
            }
            if (!CheckIfSettled(0))
            {
                StopCoroutine("SettleCountdown");
            } else
            {
                InventoryUI.Instance.DestroyCurrentBlock();
                InventoryUI.Instance.HideGamepadCursor();
                PlatformerCharacterScript.Instance.SwapMode();
                buildModeButton.gameObject.SetActive(false);
                playModeButton.SetActive(true);
                SoundManager.Instance.PlaySFXClip(piecesSetSound, Camera.main.transform);
                countdownText.color = piecesSettledColor;
                countdownText.text = "Pieces Settled!";
                countdownText.DOFade(0, 3f);

                Debug.Log("Settled");
                isCheckingPieces = false;
            }
            
        }
    }

    public void LevelCompleted(LevelSO nextLevel)
    {
        StartCoroutine(LevelTransition(nextLevel));
    }

    IEnumerator LevelTransition(LevelSO nextLevel)
    {
        if (currentLevel != null)
        {
            fadeInOutOverlay.DOFade(1, fadeInOutTime);
            yield return new WaitForSeconds(fadeInOutTime);
        }
        PlatformerCharacterScript.Instance.loading = true;
        LoadLevel(nextLevel);
        if (isFirstLevel)
        {
            SoundManager.Instance.InitialiseMusic();
            isFirstLevel = false;
        }
        else
        {
            SoundManager.Instance.ChangeMusicOnLevelChange();
        }
        fadeInOutOverlay.DOFade(0, fadeInOutTime);
        yield return new WaitForSeconds(fadeInOutTime);
        PlatformerCharacterScript.Instance.loading = false;
    }

    void LoadLevel(LevelSO levelToLoad)
    {
        if (currentLevel != null)
        {
            while (InventoryUI.Instance.placedRBs.Count > 0)
            {
                Rigidbody2D toRemove = InventoryUI.Instance.placedRBs[0];
                InventoryUI.Instance.placedRBs.Remove(toRemove);
                Destroy(toRemove.gameObject);

            }
            Destroy(currentLevelPrefab);
        }
        levelCamera.orthographicSize = levelToLoad.cameraSize;
        currentLevelPrefab = Instantiate(levelToLoad.levelPrefab);
        PlatformerCharacterScript.Instance.ResetPlayer(levelToLoad.playerSpawnPos);
        currentLevel = levelToLoad;
        InventoryUI.Instance.ReloadInventoryBlocks();
    }

    public void PauseGame()
    {
        if (gamePaused)
        {
            Time.timeScale = 1.0f;
            gamePaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            gamePaused = true;
        }
        pauseMenu.SetActive(gamePaused);
    }

}
