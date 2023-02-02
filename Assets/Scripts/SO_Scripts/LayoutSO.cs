using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Layout", menuName = "SO/Layout")]

[System.Serializable]
public class LayoutSO : ScriptableObject
{
    public int paddingXtoLeft = 0;
    public int paddingYtoTop = 0;

    [Serializable]
    public class RowArray
    {
        public GameObject[] Row;
    }
    [SerializeField] [NonReorderable] public RowArray[] matrix;
}
