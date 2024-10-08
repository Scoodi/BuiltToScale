using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    public Inventory inventory;

    private Transform blockSlotContainer;
    private Transform blockSlotTemplate;

    [SerializeField] private float mouseDeltaActivation = 0.1f;
    private bool usingGamepad = false;
    private Vector2 cursorPos = Vector2.zero;
    private Vector2 storedMousePos = Vector2.zero;
    private float currentBlockRotation = 0f;
    public List<Rigidbody2D> placedRBs = new List<Rigidbody2D>();
    GameObject currentBlock = null;

    public List<RectTransform> buttonPositions = new List<RectTransform>();
    public List<bool> IsButtonActive = new List<bool>();

    [SerializeField] private Image GamePadCursor;
    private int currentGamepadPos = 0;

    [Header("Inventory UI")]
    [Tooltip("Button height spacing")]
    [SerializeField] private float blockSlotHeight = 125f;
    [Tooltip("X offset for buttons from the anchor point in the top left corner.")]
    [SerializeField] private float xPositionOffset = 75f;
    [Tooltip("Y offset for buttons from the anchor point in the top left corner.")]
    [SerializeField] private float yPositionOffset = -75f;


    [Header("Audio Clips")]
    public AudioClip placeBlockSound;
    public AudioClip selectBlockSound;

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
        blockSlotContainer = transform.Find("BlockSlotContainer");
        blockSlotTemplate = blockSlotContainer.Find("BlockSlotTemplate");
    }
    private void Start()
    {
        //RefreshInventoryBlocks();
        cursorPos = Input.mousePosition;
        PlatformerCharacterScript.Instance.placeAction.performed += _ => TryDropBlock();
        PlatformerCharacterScript.Instance.placeMouseAction.performed += _ => TryDropBlock();
        PlatformerCharacterScript.Instance.cursorUpAction.performed += _ => MoveGamepadCursorUp();
        PlatformerCharacterScript.Instance.cursorDownAction.performed += _ => MoveGamepadCursorDown();
        PlatformerCharacterScript.Instance.placeAction.performed += _ => OnClickSpawnObject(buttonPositions[currentGamepadPos].gameObject);
        PlatformerCharacterScript.Instance.cancelAction.performed += _ => DestroyCurrentBlock();
    }

    private void Update()
    {
        if (LevelScript.Instance.gamePaused)
        {
            return;
        }
        if (((Vector2)Input.mousePosition -storedMousePos).magnitude > mouseDeltaActivation)
        {
            HideGamepadCursor();
            usingGamepad = false;
        }
        if (PlatformerCharacterScript.Instance.moveAction.ReadValue<Vector2>().magnitude > mouseDeltaActivation && PlatformerCharacterScript.Instance.building) 
        {
            ShowGamepadCursor();
            usingGamepad = true;
        }
        currentBlockRotation += PlatformerCharacterScript.Instance.rotateAction.ReadValue<float>() * 1.3f * Time.deltaTime;
        if (usingGamepad)
        {
            cursorPos += PlatformerCharacterScript.Instance.moveAction.ReadValue<Vector2>() * 10.0f * Time.deltaTime;
        } else
        {
            cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (currentBlock != null)
        {
            currentBlock.transform.position = cursorPos;
            currentBlock.transform.rotation = Quaternion.EulerRotation(Vector3.forward * currentBlockRotation);
        }
        storedMousePos = Input.mousePosition;
    }


    private void RefreshInventoryBlocks()
    {
        int y = 0;
        int count = 0;

        RectTransform UISize = GetComponent<RectTransform>();
        UISize.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((Math.Abs(yPositionOffset)/2f) + (blockSlotHeight * inventory.GetLoadedBlocks().Count)));

        foreach(GameObject block in inventory.GetLoadedBlocks())
        {
            RectTransform blockSlotRectTransform = Instantiate(blockSlotTemplate, blockSlotContainer).GetComponent<RectTransform>();
            blockSlotRectTransform.gameObject.SetActive(true);
            blockSlotRectTransform.gameObject.name = count.ToString();
            blockSlotRectTransform.anchoredPosition = new Vector2(xPositionOffset, yPositionOffset - (y * blockSlotHeight));
            buttonPositions.Add(blockSlotRectTransform);
            IsButtonActive.Add(true);
            Image image = blockSlotRectTransform.transform.Find("BlockButtonSlot").GetComponent<Image>();
            image.sprite = block.GetComponentInChildren<SpriteRenderer>().sprite;
            y++;
            count++;
        }
    }

    public void ReloadInventoryBlocks()
    {
        while(buttonPositions.Count > 0)
        {
            RectTransform toRemove = buttonPositions[0];
            IsButtonActive.Remove(IsButtonActive[0]);
            buttonPositions.Remove(toRemove);
            Destroy(toRemove.gameObject);
        }
        inventory.LoadNewLevelBlocks();
        RefreshInventoryBlocks();
        currentGamepadPos = 0;
        GamePadCursor.transform.position = buttonPositions[0].position;
    }

    public void OnClickSpawnObject(GameObject obj)
    {
        if(IsButtonActive[int.Parse(obj.name)] == true && PlatformerCharacterScript.Instance.building && !LevelScript.Instance.gamePaused)
        {
            DestroyCurrentBlock();
            if (currentBlock == null)
            {
                Vector3 mousepos = Input.mousePosition;
                mousepos = Camera.main.ScreenToWorldPoint(mousepos);
                mousepos.z = 0;
                //Debug.Log(gO.name);
                //cursorPos = mousepos;
                currentGamepadPos = int.Parse(obj.name);
                currentBlock = Instantiate(inventory.GetLoadedBlocks()[int.Parse(obj.name)], mousepos, Quaternion.identity);
                //buttonPositions[int.Parse(obj.name)].GetComponentInChildren<Button>().interactable = false;
                //IsButtonActive[int.Parse(obj.name)] = false;
            }
            
        }
    }

    public void MoveGamepadCursorUp()
    {
        if(PlatformerCharacterScript.Instance.building && !LevelScript.Instance.gamePaused)
        {
            ShowGamepadCursor();
            if (currentGamepadPos > 0)
            {
                // allow move up
                GamePadCursor.transform.position = buttonPositions[currentGamepadPos - 1].position;
                SoundManager.Instance.PlaySFXClip(selectBlockSound, GamePadCursor.transform);
                currentGamepadPos--;
                DestroyCurrentBlock();
                OnClickSpawnObject(buttonPositions[currentGamepadPos].gameObject);
            }
        }
        
    }

    public void MoveGamepadCursorDown()
    {
        if (PlatformerCharacterScript.Instance.building && !LevelScript.Instance.gamePaused)
        {
            ShowGamepadCursor();
            if (currentGamepadPos < buttonPositions.Count - 1)
            {
                // allow move down
                GamePadCursor.transform.position = buttonPositions[currentGamepadPos + 1].position;
                SoundManager.Instance.PlaySFXClip(selectBlockSound, GamePadCursor.transform);
                currentGamepadPos++;
                DestroyCurrentBlock();
                OnClickSpawnObject(buttonPositions[currentGamepadPos].gameObject);
            }
        }
    }

    void TryDropBlock()
    {
        if (currentBlock != null && !LevelScript.Instance.gamePaused)
        {
            bool canPlace = true;
            foreach (Collider2D col in currentBlock.GetComponents<Collider2D>())
            {
                List<Collider2D> overlapResults = new List<Collider2D>();
                Physics2D.OverlapCollider(col, new ContactFilter2D(), overlapResults);
                if (overlapResults.Count != 0)
                {
                    canPlace = false;
                }
            }
            if (canPlace)
            {
                currentBlock.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                foreach (Collider2D col in currentBlock.GetComponents<Collider2D>())
                {
                    col.isTrigger = false;
                }
                SoundManager.Instance.PlaySFXClip(placeBlockSound, currentBlock.transform);
                placedRBs.Add(currentBlock.GetComponent<Rigidbody2D>());
                buttonPositions[currentGamepadPos].GetComponentInChildren<Button>().interactable = false;
                IsButtonActive[currentGamepadPos] = false;
                currentBlock = null;
            }
            
        }
    }

    public void ShowGamepadCursor()
    {
        GamePadCursor.color = new Color(GamePadCursor.color.r, GamePadCursor.color.g, GamePadCursor.color.b, 0.3f);
    }

    public void HideGamepadCursor() 
    {
        GamePadCursor.color = new Color(GamePadCursor.color.r, GamePadCursor.color.g, GamePadCursor.color.b, 0f);
    }

    public void DestroyCurrentBlock()
    {
        if(currentBlock != null)
        {
            Destroy(currentBlock);
            currentBlock = null;
        }
        
    }

    public void SetItemStorage(Inventory inventory)
    {
        this.inventory = inventory;
    }

    
}
