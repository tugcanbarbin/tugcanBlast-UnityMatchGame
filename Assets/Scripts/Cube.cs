using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;



public class Cube : MonoBehaviour
{
    bool visited = false;
    public CubeType cubeType;
    ParticleController particleController;

    public enum CubeType
    {
        yellow,
        red,
        blue,
        green,
        purple,
        duck,
        balloon,
        rocket
    }
    private List<CubeType> specialCubes = new List<CubeType> { CubeType.duck, CubeType.balloon };
    private void Start()
    {
        particleController = ParticleController.instance;
    }
    public void Choose()
    {
        // do not execute if the game is frozen by another execution
        if (!InputController.instance.IsStarted())
        {
            Debug.Log("Game canvas input is blocked");
            return;
        }
        if (PowerUpController.instance.powerUpIsActive)
        {
            GameController.instance.PowerUpHandler(PowerUpController.instance.currentPowerUp, gameObject);
            return;
        }
        // do not take balloon or duck input even if button component opens accidently
        if (cubeType == CubeType.balloon || cubeType == CubeType.duck)
        {
            return;
        }

        if (cubeType == CubeType.rocket) // if cube is a rocket
        {
            //start an event
            UIController.instance.UpdateMoves(-1);
            InputController.instance.SetTouch(false);

            // search other rockets nearby
            int listCount = GameController.instance.NeighbourSearch(gameObject);
            List<GameController.SearchElement> searchList = GameController.instance.GetSearchList();

            // merge the rockets
            for (int i = 0; i < listCount; i++)
            {
                searchList[i].cube.transform.DOMove(transform.position, 0.4f).SetEase(Ease.InBack);
            }

            //choose the correct action for given rocket number
            RocketChooseExecution(listCount);
            return;
        }
        else // if a normal cube
        {
            // Search neighbour blocks, return the number of cubes included balloons
            int listCount = GameController.instance.NeighbourSearch(gameObject);
            // Get the count of normal blocks
            int blockCount = GameController.instance.SearchListBlockCount();
            List<GameController.SearchElement> searchList = GameController.instance.GetSearchList();

            // can execute
            if (blockCount > 1)
            {
                InputController.instance.SetTouch(false);
                UIController.instance.UpdateMoves(-1);
            }
            else
            {
                // reset
                searchList.Clear();
                return;
            }

            // if there are more than 4, create a rocket
            if (blockCount > 4)
            {
                Cube.CubeType clickedCubeType = searchList[0].cube.GetComponent<Cube>().cubeType;
                for (int i = 0; i < listCount; i++)
                {
                    if (searchList[i].cube.GetComponent<Cube>().cubeType == clickedCubeType)
                        searchList[i].cube.transform.DOMove(transform.position, 1f).SetEase(Ease.InOutBack);
                }
                StartCoroutine(RocketBuffCollect(listCount, searchList, 1f));
            }
            else // else normal collect action
            {
                NormalCollect(listCount, searchList);
            }
        }
    }

    #region utils
    public bool IsVisited() {return visited;}
    public void SetVisited(bool b) { visited = b; }
    #endregion

    #region Rocket utils
    public void RocketChooseExecution(int numberOfRockets = 1)
    {

        if (numberOfRockets > 1)
        {
            //merge execution
            StartCoroutine( MultipleRocketExecute(0.5f));
        }
        else
        {
            SingleRocketExecute();
        }
    }
    IEnumerator MultipleRocketExecute(float time)
    {
        yield return new WaitForSeconds(time);
        InputController.instance.AddCurrentEvents(1);

        // Merge rockets
        List<GameController.SearchElement> searchList = GameController.instance.GetSearchList();
        Vector2Int clickedCubeIndex = new Vector2Int(searchList[0].i, searchList[0].j);
        int listCount = searchList.Count;
        // Destroy other rockets
        for (int m = listCount - 1; m > 0; m--)
        {
            GameController.SearchElement currentCube = searchList[m];
            GameController.instance.ClearACell(currentCube.i, currentCube.j);
            searchList.RemoveAt(m);
            UIController.instance.UpdateGoals(currentCube.cube, -1);
            Destroy(currentCube.cube);
        }
        // merge animation
        gameObject.transform.GetChild(2).gameObject.SetActive(true);
        DOTween.Sequence().Append(transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f))
            .Append(transform.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.1f))
            .Append(transform.DOScale(new Vector3(1f, 1, 1f), 0.1f));

        // execute
        transform.GetComponent<Animator>().SetTrigger("Execute");
        //get up, down, left, right side's cubes
        List<GameController.SearchElement> leftCubes = new List<GameController.SearchElement>();
        List<GameController.SearchElement> rightCubes = new List<GameController.SearchElement>();
        List<GameController.SearchElement> downCubes = new List<GameController.SearchElement>();
        List<GameController.SearchElement> upCubes = new List<GameController.SearchElement>();
        GameController.instance.ClearACell(clickedCubeIndex.x, clickedCubeIndex.y);
        GameController.instance.RocketSideSearch(gameObject,ref downCubes ,ref upCubes , ref leftCubes, ref rightCubes, clickedCubeIndex.x, clickedCubeIndex.y);

        // Execute the destroy action
        StartCoroutine(DirectedDestroy(rightCubes, 0.08f));
        StartCoroutine(DirectedDestroy(leftCubes, 0.08f));
        StartCoroutine(DirectedDestroy(upCubes, 0.08f));
        StartCoroutine(DirectedDestroy(downCubes, 0.08f));
        StartCoroutine(DestroyRocket(1.4f));
    }
    private void SingleRocketExecute()
    {
        InputController.instance.AddCurrentEvents(1);
        transform.GetComponent<Animator>().SetTrigger("Execute");

        //get both side's cubes
        List<GameController.SearchElement> leftCubes = new List<GameController.SearchElement>();
        List<GameController.SearchElement> rightCubes = new List<GameController.SearchElement>();
        int i = -1, j = -1;
        GameController.instance.FindCoordinate(gameObject, ref i, ref j);
        if (i == -1 || j == -1)
        {
            Debug.Log("Could not find the object in single rocket");
        };
        GameController.instance.ClearACell(i, j);

        //get left, right side's cubes
        GameController.instance.RocketSideSearch(gameObject, ref leftCubes, ref rightCubes, i, j);

        // execute the destroy action
        StartCoroutine(DirectedDestroy(rightCubes, 0.08f));
        StartCoroutine(DirectedDestroy(leftCubes, 0.08f));
        StartCoroutine(DestroyRocket(1.4f));
    }

    IEnumerator DestroyRocket(float time)
    {
        yield return new WaitForSeconds(time);
        InputController.instance.AddCurrentEvents(-1);
        GameController.instance.ClearSearchList();
        Destroy(this.gameObject);

    }
    public IEnumerator DirectedDestroy(List<GameController.SearchElement> sideCubes, float delayPerDestroy)
    {             
        int i = 0;///b,d
        int length = sideCubes.Count;
        while (i < length)
        {
            if (sideCubes.Count < 1) yield break; // if list is empty return

            yield return new WaitForSeconds(delayPerDestroy);

            if (sideCubes[0].cube != null) // null check
            {
                Cube currentCube = sideCubes[0].cube.GetComponent<Cube>();
                if (currentCube.cubeType == CubeType.rocket)
                {
                    // execute that rocket too
                    sideCubes.RemoveAt(0);
                    currentCube.RocketChooseExecution();
                }
                else if (currentCube.cubeType == CubeType.duck)
                {
                    // do nothing to duck
                    sideCubes.RemoveAt(0);
                }
                else
                {
                    Collect(sideCubes, 0, sideCubes[0]);
                }
                i++;
            }
            else
            {
                sideCubes.RemoveAt(0);
            }
        }
    }
    #endregion

    #region Collect Utils
    IEnumerator RocketBuffCollect(int listCount, List<GameController.SearchElement> searchList, float time)
    {//TODO:goal
        yield return new WaitForSeconds(time);
        Vector2Int clickedCubeIndex = new Vector2Int(searchList[0].i,searchList[0].j);

        //clear all cubes
        for (int i = listCount-1; i > -1; i--)
        {
            GameController.SearchElement currentCube = searchList[i];
            GameController.instance.ClearACell(currentCube.i, currentCube.j);
            searchList.RemoveAt(i);
            GameObject targetUI = UIController.instance.UpdateGoals(currentCube.cube, -1);
            if (targetUI != null)//TODO: cube is goal , update the goal
            {
                ParticleController.instance.GoalCollectParticHandler(targetUI.transform.position);
                AudioController.instance.PlayCubeCollect();
            }
            Destroy(currentCube.cube);
        }
        AudioController.instance.PlayBalloonExplode();

        // create a rocket
        GameObject rocket = GameController.instance.InstantiateRocket(clickedCubeIndex.x, clickedCubeIndex.y);//x = i, y = j
        //random rotated vertical-horizontal rocket
        int[] rotateDegrees = { 0, 90 };
        rocket.transform.localRotation = Quaternion.Euler(new Vector3(0, 0,
            rotateDegrees[Random.Range(0, 2)]));

        rocket.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        // rocket creation anim
        DOTween.Sequence().Append(rocket.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f))
            .Append(rocket.transform.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.1f))
            .Append(rocket.transform.DOScale(new Vector3(1f, 1, 1f), 0.1f));

        // release the game
        GameController.instance.DropCubes();
    }

    private void NormalCollect(int listCount, List<GameController.SearchElement> searchList)
    {
        for (int i = listCount - 1; i > -1; i--)
        {
            GameController.SearchElement currentCube = searchList[i];
            Collect(searchList, i, currentCube);
        }
        GameController.instance.DropCubes();
    }

    private void Collect(List<GameController.SearchElement> searchList, int i, GameController.SearchElement currentCube)
    {
        if (currentCube.cube.GetComponent<Cube>().cubeType == CubeType.rocket)
        {
            Debug.Log("wrong if");
            searchList.RemoveAt(i);
            return;
        }
        
        GameObject targetUI = UIController.instance.UpdateGoals(currentCube.cube, -1);
        if (targetUI != null)//TODO: cube is goal , update the goal
        {
            if (specialCubes.Contains(currentCube.cube.GetComponent<Cube>().cubeType)) // if special type cube
            {
                if (currentCube.cube.GetComponent<Cube>().cubeType == CubeType.balloon) // if balloons
                {
                    GameController.instance.ClearACell(currentCube.i, currentCube.j);
                    searchList.RemoveAt(i);
                    AudioController.instance.PlayBalloonExplode();

                    currentCube.cube.transform.GetComponent<Button>().enabled = false;
                    currentCube.cube.transform.GetComponent<BoxCollider2D>().enabled = false;

                    ParticleController.instance.GoalCollectParticHandler(targetUI.transform.position);
                    AudioController.instance.PlayCubeCollect();

                    Destroy(currentCube.cube);
                }
                else
                {
                    searchList.RemoveAt(i);
                }
            }
            else // normal cube collect with goal update
            {
                GameController.instance.ClearACell(currentCube.i, currentCube.j);
                searchList.RemoveAt(i);

                AudioController.instance.PlayCubeExplode();
                ParticleController.instance.CubeExplodeParticHandler(currentCube.cube.GetComponent<Cube>().cubeType, currentCube.cube.transform.position);
                currentCube.cube.transform.DOMove(targetUI.transform.position, 0.8f).SetEase(Ease.InBack);
                AudioController.instance.PlayCubeCollect(0.8f);
                ParticleController.instance.GoalCollectParticHandler(targetUI.transform.position, 0.8f);

                currentCube.cube.transform.GetComponent<Button>().enabled = false;
                currentCube.cube.transform.GetComponent<BoxCollider2D>().enabled = false;
                currentCube.cube.transform.parent = UIController.instance.topBar.transform;
                currentCube.cube.transform.SetAsLastSibling();
                Destroy(currentCube.cube, 0.8f);
            }
            //dotween
            

            //yield return new waitforsec
            //
        }
        else
        {
            // special cube collect without goal update
            if (specialCubes.Contains(currentCube.cube.GetComponent<Cube>().cubeType))
            {
                if (currentCube.cube.GetComponent<Cube>().cubeType == CubeType.balloon) // if balloon
                {
                    GameController.instance.ClearACell(currentCube.i, currentCube.j);
                    searchList.RemoveAt(i);
                    AudioController.instance.PlayBalloonExplode();
                    ParticleController.instance.CubeExplodeParticHandler(currentCube.cube.GetComponent<Cube>().cubeType, currentCube.cube.transform.position);
                    Destroy(currentCube.cube);
                }
                else // other objects that are at search with special type, do nothing, ex. duck
                {
                    searchList.RemoveAt(i);
                }
            }
            else // normal cube collect without goal update
            {
                GameController.instance.ClearACell(currentCube.i, currentCube.j);
                searchList.RemoveAt(i);
                AudioController.instance.PlayCubeExplode();

                ParticleController.instance.CubeExplodeParticHandler(currentCube.cube.GetComponent<Cube>().cubeType, currentCube.cube.transform.position);
                Destroy(currentCube.cube);
            }
            

        }
    }
    #endregion
}
