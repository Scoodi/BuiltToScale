using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Inventory : MonoBehaviour
{
    public enum Difficulty
    {
        Easy, Medium, Hard
    }

    [SerializeField] private LevelSO levelBlocks;
    [SerializeField] private List<GameObject> loadedBlocks;


    public Difficulty difficulty;

    public void Awake()
    {
        loadedBlocks = new List<GameObject>();

        //difficulty = Difficulty.Easy;

        Debug.Log("Item storage created!");
        
    }

    private void Start()
    {
        LoadBlocks();
    }

    public List<GameObject> GetLoadedBlocks()
    {
        return loadedBlocks;
    }

    public void LoadBlocks()
    {
        levelBlocks = LevelScript.Instance.currentLevel;

        if( difficulty == Difficulty.Easy )
        {
            foreach( GameObject block in levelBlocks.Easy)
            {
                loadedBlocks.Add(block);
            }
        }
        else if(difficulty == Difficulty.Medium)
        {
            foreach (GameObject block in levelBlocks.Medium)
            {
                loadedBlocks.Add(block);
            }
        }
        else if (difficulty == Difficulty.Hard)
        {
            foreach (GameObject block in levelBlocks.Hard)
            {
                loadedBlocks.Add(block);
            }
        }
    }

    public void ClearBlocks()
    {
        foreach(GameObject block in loadedBlocks)
        {
            GameObject blockToRemove = block;
            loadedBlocks.Remove(blockToRemove);
            Destroy(blockToRemove);
        }
    }
}
