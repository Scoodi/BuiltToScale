using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    private Transform blockSlotContainer;
    private Transform blockSlotTemplate;

    private void Awake()
    {
        blockSlotContainer = transform.Find("BlockSlotContainer");
        blockSlotTemplate = blockSlotContainer.Find("BlockSlotTemplate");
    }
    private void Start()
    {
        RefreshInventoryBlocks();
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
            Image image = blockSlotRectTransform.transform.Find("BlockButtonSlot").GetComponent<Image>();
            image.sprite = block.GetComponentInChildren<SpriteRenderer>().sprite;
            y++;
            count++;
        }
    }

    public void OnClickSpawnObject(GameObject gO)
    {
        Vector3 mousepos = Input.mousePosition;
        mousepos = Camera.main.ScreenToWorldPoint(mousepos);
        mousepos.z = 0;
        Debug.Log(gO.name);
        Instantiate(inventory.GetLoadedBlocks()[int.Parse(gO.name)], mousepos, Quaternion.identity);
    }

    public void SetItemStorage(Inventory inventory)
    {
        this.inventory = inventory;
    }

    
}
