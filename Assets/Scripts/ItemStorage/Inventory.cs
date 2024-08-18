using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum Difficulty
{
    Easy, Medium, Hard
}

public class Inventory : MonoBehaviour
{
    [SerializeField] private LevelSO levelBlocks;
    [SerializeField] private List<GameObject> loadedBlocks;


    public Difficulty difficulty;

    public void Awake()
    {
        loadedBlocks = new List<GameObject>();

        //difficulty = Difficulty.Easy;

        Debug.Log("Item storage created!");

        LoadBlocks();
        
    }

    public List<GameObject> GetLoadedBlocks()
    {
        return loadedBlocks;
    }

    public void LoadBlocks()
    {
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
}
