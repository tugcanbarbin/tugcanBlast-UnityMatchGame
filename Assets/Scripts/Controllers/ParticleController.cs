using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public static ParticleController instance;

    [SerializeField] GameObject cubeExplode1;
    [SerializeField] GameObject cubeExplode2;
    [SerializeField] GameObject GoalCollect;
    //public enum CubeType
    //{
    //    yellow,
    //    red,
    //    blue,
    //    green,
    //    purple,
    //    duck,
    //    balloon,
    //    rocket
    //}

    private List<Color> cubeColors;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Color yellowColor = new Color(0.9058824f, 0.764706f, 0f); //
        Color redColor = new Color(0.9019608f, 0, 0f); //
        Color blueColor = new Color(0.227451f, 0.6196079f, 0.8509805f); //
        Color greenColor = new Color(0.2647057f, 0.7264151f, 0f); //
        Color purpleColor = new Color(0.6588235f, 0f, 0.6980392f); //
        Color duckColor = new Color(0.9058824f, 0.764706f, 0f);
        Color balloonColor = new Color(0.9647059f, 0.2745098f, 0.5058824f);



        cubeColors = new List<Color>() { yellowColor, redColor, blueColor, greenColor, purpleColor,duckColor,balloonColor };
    }

    public void CubeExplodeParticHandler(Cube.CubeType currentType, Vector3 pos, float delay = 0)
    {
        StartCoroutine(CubeExplodePartic(currentType,pos, delay));
    }
    public void GoalCollectParticHandler(Vector3 pos, float delay = 0)
    {
        StartCoroutine(GoalCollectPartic(pos,delay));
    }

    IEnumerator CubeExplodePartic(Cube.CubeType currentType, Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay);


        GameObject partic = Instantiate(cubeExplode1, pos, Quaternion.identity);
        GameObject partic2 = Instantiate(cubeExplode2, pos, Quaternion.identity);

        ParticleSystem.MainModule settings = partic.GetComponent<ParticleSystem>().main;
        settings.startColor = cubeColors[(int)currentType];
        ParticleSystem.MainModule settings2 = partic2.GetComponent<ParticleSystem>().main;
        settings2.startColor = cubeColors[(int)currentType];

    }
    IEnumerator GoalCollectPartic(Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject partic = Instantiate(GoalCollect, pos, Quaternion.identity);
    }
}
