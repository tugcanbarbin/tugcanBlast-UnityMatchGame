using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Spawn Object SO", menuName = "SO/SpawnObjects")]

[System.Serializable]
public class SpawnObjectsSO : ScriptableObject
{

    public GameObject[] spawnObjects;

}
