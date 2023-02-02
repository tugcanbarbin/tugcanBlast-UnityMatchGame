using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Image SO", menuName = "SO/BorderImages")]
public class BorderImagesSO : ScriptableObject
{
    public GameObject leftTopBorderImage;
    public GameObject RightTopBorderImage;
    public GameObject leftBottomBorderImage;
    public GameObject rightBottomBorderImage;

    public GameObject TopLineBorderImage;
    public GameObject rightLineBorderImage;
    public GameObject leftLineBorderImage;
    public GameObject bottomLineBorderImage;

    public GameObject blackBorderImage;
}
