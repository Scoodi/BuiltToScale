using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    [SerializeField] private Inventory inventory;

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
        RefreshInventoryBlocks();
        cursorPos = Input.mousePosition;
        PlatformerCharacterScript.Instance.placeAction.performed += _ => TryDropBlock();
    }

    private void Update()
    {
        if (((Vector2)Input.mousePosition -storedMousePos).magnitude > mouseDeltaActivation)
        {
            usingGamepad = false;
        }
        if (PlatformerCharacterScript.Instance.moveAction.ReadValue<Vector2>().magnitude > mouseDeltaActivation) 
        {
            usingGamepad = true;
        }
        currentBlockRotation += PlatformerCharacterScript.Instance.rotateAction.ReadValue<float>() * 0.01f;
        if (usingGamepad)
        {
            cursorPos += PlatformerCharacterScript.Instance.moveAction.ReadValue<Vector2>() * 0.1f;
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
        float blockSlotCellSize = 110f;

        RectTransform UISize = GetComponent<RectTransform>();
        UISize.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50 + (blockSlotCellSize * inventory.GetLoadedBlocks().Count + 1));

        foreach(GameObject block in inventory.GetLoadedBlocks())
        {
            RectTransform blockSlotRectTransform = Instantiate(blockSlotTemplate, blockSlotContainer).GetComponent<RectTransform>();
            blockSlotRectTransform.gameObject.SetActive(true);
            blockSlotRectTransform.gameObject.name = count.ToString();
            blockSlotRectTransform.anchoredPosition = new Vector2(75, -75 - (y * blockSlotCellSize));
            buttonPositions.Add(blockSlotRectTransform);
            IsButtonActive.Add(true);
            Image image = blockSlotRectTransform.transform.Find("BlockButtonSlot").GetComponent<Image>();
            image.sprite = block.GetComponentInChildren<SpriteRenderer>().sprite;
            y++;
            count++;
        }
    }

    public void OnClickSpawnObject(GameObject obj)
    {
        if (currentBlock == null)
        {
            Vector3 mousepos = Input.mousePosition;
            mousepos = Camera.main.ScreenToWorldPoint(mousepos);
            mousepos.z = 0;
            //Debug.Log(gO.name);
            cursorPos = mousepos;
            currentBlock = Instantiate(inventory.GetLoadedBlocks()[int.Parse(obj.name)], mousepos, Quaternion.identity);
            buttonPositions[int.Parse(obj.name)].GetComponentInChildren<Button>().interactable = false;
            IsButtonActive[int.Parse(obj.name)] = false;
        }
    }

    void TryDropBlock()
    {
        if (currentBlock != null)
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
                placedRBs.Add(currentBlock.GetComponent<Rigidbody2D>());
                currentBlock = null;
            }
            
        }
    }

    public void SetItemStorage(Inventory inventory)
    {
        this.inventory = inventory;
    }

    
}
