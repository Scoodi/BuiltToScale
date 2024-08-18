using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

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

    [SerializeField] private bool isFirstLevel = true;
    //TODO add reference to inventory

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
        if (isFirstLevel)
        {
            LevelCompleted(firstLevel);
        }
        SoundManager.Instance.InitialiseMusic();
        InventoryUI.Instance.inventory.LoadBlocks();

    }
    public void TryGoToPlatforming ()
    {
        if (PlatformerCharacterScript.Instance.building)
        {
            StartCoroutine("SettleCountdown");
        } else
        {
            Debug.Log("Restart level to rebuild");
        };
    }

    bool CheckIfSettled ()
    {
        bool settled = true;
        foreach (Rigidbody2D rb in InventoryUI.Instance.placedRBs)
        {
            if (rb.velocity.magnitude > maxVelocity)
            {
                Debug.Log("Pieces not settled!");
                settled = false; break;
            }
        }
        return settled;
    }

    IEnumerator SettleCountdown ()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!CheckIfSettled())
            {
                StopCoroutine("SettleCountdown");
            }
            Debug.Log("Waited " + i + " seconds");
            yield return new WaitForSeconds(1f);
        }
        InventoryUI.Instance.DestroyCurrentBlock();
        InventoryUI.Instance.HideGamepadCursor();
        PlatformerCharacterScript.Instance.SwapMode();
        Debug.Log("Settled");
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
        if(isFirstLevel)
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
        PlatformerCharacterScript.Instance.ResetPlayer(levelToLoad.playerSpawnPos, levelToLoad.playerSpawnFacingRight);
        currentLevel = levelToLoad;
        InventoryUI.Instance.ReloadInventoryBlocks();
    }
}
