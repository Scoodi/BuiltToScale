using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LevelSO")]
public class LevelSO : ScriptableObject
{
    public List<GameObject> Easy;
    public List<GameObject> Medium;
    public List<GameObject> Hard;
}
