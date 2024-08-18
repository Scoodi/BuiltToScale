using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LevelSO")]
public class LevelSO : ScriptableObject
{
    public int levelNumber = -1;
    public GameObject levelPrefab;
    public float cameraSize = 10;
    public List<GameObject> Easy;
    public List<GameObject> Medium;
    public List<GameObject> Hard;

    public Vector2 playerSpawnPos;
    public bool playerSpawnFacingRight;
}
