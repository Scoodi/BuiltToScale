using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LevelInventorySO")]
public class LevelInventorySO : ScriptableObject
{
    public List<GameObject> Easy;
    public List<GameObject> Medium;
    public List<GameObject> Hard;
}
