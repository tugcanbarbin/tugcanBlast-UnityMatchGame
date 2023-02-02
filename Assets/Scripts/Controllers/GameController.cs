using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    [Header("Scriptable object references")]
    public LayoutSO layout;
    public BorderImagesSO borderSprites;
    public SpawnObjectsSO spawnObjects;

    [Header("UI and Grid References")]
    public GameObject bordersParent;
    public GameObject cubesParent;

    [Header("Special blocks")]
    public GameObject rocketPrefab;

    [Header("Game Settings")]
    public int spawnY = 400;

    // These matrixes are used to control grid movements and assignments
    private Vector2[,] playFieldCoords; // to keep track of coordinates
    private GameObject[,] playFieldCubes; // to access objects
    private bool[,] playFieldBool; // to keep track of null condition

    // grid variables
    int columnLength;
    int rowLength;
    int startX;
    int startY;
    int paddingX;
    int paddingY;

    float scaleFactor; // canvas scale factor

    //search related variables
    public struct SearchElement
    {
        public GameObject cube;
        public int i;
        public int j;
        public SearchElement(GameObject obj, int x, int y)
        {
            cube = obj;
            i = x;
            j = y;
        }
    }
    private List<SearchElement> searchList = new List<SearchElement>();

    private void Awake()
    {
        instance = this; // singleton gamecontroller
    }

    private void Start()
    {
        if (layout != null && bordersParent != null) // reference control
        {
            columnLength = layout.matrix.Length;
            rowLength = layout.matrix[0].Row.Length;

            // create exact matrixes for control of the grid
            playFieldCoords = new Vector2[columnLength, rowLength];
            playFieldCubes = new GameObject[columnLength, rowLength];
            playFieldBool = new bool[columnLength, rowLength];

            // initialize the border of grid
            InitializeTheBorder();

            // initialize the starting cubes 
            InitializeTheStartingCubes();

            //drop the cubes if there is any and start the game
            DropCubes();
        }
        else
        {
            Debug.LogWarning("UI or grid reference is null in GameController Object!");
        }

    }

    #region Start Initialization Functions
    private void InitializeTheBorder()
    {
        //get the scale factor from canvas
        scaleFactor = bordersParent.transform.parent.transform.parent.localScale.x;

        //padding adjustment
        paddingX = layout.paddingXtoLeft;
        paddingY = layout.paddingYtoTop;

        // if odd number, then pad it with half of the object, original interval is 100
        #region Explanation
        // for even numbers
        // different blocks for top left and right corners and edges
        // also 1 extra border block for given blocks
        // eg. to complete a top row with 8 blocks
        // 1 for 0,0;
        // 3 top lined border blocks to left
        // 1 left top corner border block at the left corner
        // 3 top lined border blocks to right
        // 1 right top corner border block at the right corner
        // Total 9 border blocks

        // Also, for odd numbers
        // eg. to complete a top row with 7 blocks
        // 1 for -50,50;
        // 2 top lined border blocks to left
        // 1 left top corner border block at the left corner
        // 3 top lined border blocks to right
        // 1 right top corner border block at the right corner
        // so 1 block will be more at the left side
        // total 9 border blocks
        // so padding added to center the grid again with half of the object size
        #endregion // explanation for block adjustments
        if (rowLength % 2 == 1)
        { 
            paddingX += -50;
        }
        if (columnLength % 2 == 1) 
        {
            paddingY += 50;
        }  
        startX = -100 * rowLength / 2 + paddingX;
        startY = 100 * columnLength / 2 + paddingY;

        // initiate the sprites
        // base point is startX,startY + paddings as top left corner
        for (int i = 0; i < columnLength+1; i++) // for length of a row
        {
            for (int j = 0; j < rowLength+1; j++) // for length of a column
            {
                // top row
                if (i == 0) 
                {
                    // top left corner
                    if (j == 0)
                    {
                        InstantiateBorder(borderSprites.leftTopBorderImage, scaleFactor, startX, startY, i, j);
                    }
                    // top right corner
                    else if (j == rowLength) 
                    {
                        InstantiateBorder(borderSprites.RightTopBorderImage, scaleFactor, startX, startY, i, j);
                    }
                    // top line
                    else
                    {
                        InstantiateBorder(borderSprites.TopLineBorderImage, scaleFactor, startX, startY, i, j);
                    }
                }
                // bottom row
                else if (i == columnLength ) 
                {
                    // bottom left corner
                    if (j == 0) 
                    {
                        InstantiateBorder(borderSprites.leftBottomBorderImage, scaleFactor, startX, startY, i, j);
                    }
                    // bottom right corner 
                    else if (j == rowLength)
                    {
                        InstantiateBorder(borderSprites.rightBottomBorderImage, scaleFactor, startX, startY, i, j);
                    }
                    // bottom line
                    else
                    {
                        InstantiateBorder(borderSprites.bottomLineBorderImage, scaleFactor, startX, startY, i, j);
                    }
                }
                else 
                {
                    //  left line
                    if (j == 0) 
                    {
                        InstantiateBorder(borderSprites.leftLineBorderImage, scaleFactor, startX, startY, i, j);
                    }
                    // rightLine
                    else if (j == rowLength)
                    {
                        InstantiateBorder(borderSprites.rightLineBorderImage, scaleFactor, startX, startY, i, j);
                    }
                    // Centers
                    else
                    {
                        InstantiateBorder(borderSprites.blackBorderImage, scaleFactor, startX, startY, i, j);
                    }
                }

            }
        }
    }
    private void InitializeTheStartingCubes()
    {
        // initiate the sprites
        // base point is startX,startY + paddings as top left corner
        for (int i = columnLength-1; i > -1; i--) // for length of a row
        {
            for (int j = 0; j < rowLength; j++) // for length of a column
            {
                if (layout.matrix[i].Row[j] != null)
                {
                    //  game cubes start is different than border, +50 for x - 50 for y
                    InstantiateGameCube(layout.matrix[i].Row[j], scaleFactor, startX+50, startY-50, i, j);
                }
            }
        }
    }
    private void InstantiateBorder(GameObject prefab ,float scaleFactor, int startX, int startY, int i, int j)
    {

        var temp = Instantiate(prefab,
            bordersParent.transform.position + new Vector3(startX * scaleFactor + ((j * 100) * scaleFactor), startY * scaleFactor + ((i * -100) * scaleFactor), 0), Quaternion.identity, bordersParent.transform);
    }
    private void InstantiateGameCube(GameObject prefab, float scaleFactor, int startX, int startY, int i, int j)
    {

        var temp = Instantiate(prefab,
            cubesParent.transform.position + new Vector3(startX * scaleFactor + ((j * 100) * scaleFactor), startY * scaleFactor + ((i * -100) * scaleFactor), 0), Quaternion.identity, cubesParent.transform);
        playFieldCoords[i, j] = new Vector2(temp.transform.position.x, temp.transform.position.y);

        playFieldCubes[i, j] = temp;

        playFieldBool[i, j] = true;
    }
    #endregion

    //override // top to down spawn effect

    #region Instantiate In Runtime Functions
    private GameObject InstantiateGameCube(GameObject prefab, float scaleFactor, int startX, int startY, int i, int j, int spawnY)  
    {
        var temp = Instantiate(prefab, cubesParent.transform.position + new Vector3(startX * scaleFactor + (j * 100) * scaleFactor,  (rowLength-i-1) * scaleFactor *100, 0),
            Quaternion.identity, cubesParent.transform);
        // Drop effect of cubes( move and scale punch)
        DOTween.Sequence()
                .Append(temp.transform.DOMoveY(cubesParent.transform.position.y + startY * scaleFactor + (i * -100) * scaleFactor, 0.5f).SetEase(Ease.InCubic))
                .Append(temp.transform.DOScale(temp.transform.localScale*1.1f, 0.1f))
                .Append(temp.transform.DOScale(temp.transform.localScale * 0.9f, 0.1f))
                .Append(temp.transform.DOScale(temp.transform.localScale * 1f, 0.1f));

        // assign the coordinates
        playFieldCoords[i, j] = new Vector2(temp.transform.position.x, startY * scaleFactor + ((i * -100) * scaleFactor));
        playFieldCubes[i, j] = temp;
        playFieldBool[i, j] = true;

        // return the game object
        return temp;
    }
    public GameObject InstantiateRocket(int i, int j)
    {
        var temp = Instantiate(rocketPrefab,
            cubesParent.transform.position + new Vector3((startX+50) * scaleFactor + (j * 100 * scaleFactor),
            (startY - 50) * scaleFactor + (i * -100 * scaleFactor), 0 ),
            Quaternion.identity, cubesParent.transform);

        //assign the values to matrixes
        playFieldCubes[i, j] = temp;
        playFieldBool[i, j] = true;

        // return the rocket object
        return temp;
    }
    #endregion

    #region Grid Movement
    public void DropCubes()
    {
        for (int j = 0; j < rowLength; j++) // row
        {
            int nullCount = 0;
            for (int i = columnLength - 1; i > -1; i--) // column
            {

                if (playFieldBool[i, j] == false) 
                {
                    // count the empty blocks in a column
                    nullCount++;
                }
                else
                {
                    if (i + nullCount < columnLength && nullCount > 0)
                    {
                        // shift the blocks to the bottom
                        playFieldCubes[i, j].transform.DOLocalMoveY(playFieldCubes[i, j].transform.localPosition.y - nullCount * 100, 0.5f);
                        playFieldCubes[i + nullCount, j] = playFieldCubes[i, j];

                        playFieldBool[i + nullCount, j] = true;
                        playFieldCubes[i, j] = null;
                        playFieldBool[i, j] = false;
                    }
                }
            }
        }
        //after the shifting blocks to buttom, fill the empty grid
        FillTheGrid();

    }  
    private void FillTheGrid()
    {
        for (int j = rowLength - 1; j > -1; j--) // for length of a row
        {
            for (int i = columnLength - 1; i > -1; i--) // for length of a column
            {
                if (playFieldCubes[i, j] == null)
                {
                    //Get Random Cube
                    int random = (int)Mathf.Floor(UnityEngine.Random.Range(0, spawnObjects.spawnObjects.Length));

                    // game cubes start is different than border, +50 for x - 50 for y
                    // spawn a random type cube
                    InstantiateGameCube(spawnObjects.spawnObjects[random], scaleFactor, startX + 50, startY - 50, i, j, spawnY);
                }
            }
        }

        if (!UIController.instance.isGameEnded) //game continues
        {
            StartCoroutine(InputController.instance.SetTouchWithDelay(true, 0.8f));
        }
        else // game finished
        {
            StartCoroutine(InputController.instance.SetTouchWithDelay(false, 0.8f));
            if (UIController.instance.goalsCompleted) // goals are completed 
            {
                // moves times random rockets
                //TODO:
                // execution of rockets
            }
        }
    }
    #endregion

    #region Grid Search
    // get the list of blocks in the range of rocket execution
    public void RocketSideSearch(GameObject cube, ref List<SearchElement> leftSide, ref List<SearchElement> rightSide, int i = -1, int j = -1)
    {
        int iRight = i + 1;
        int iLeft = i - 1;
        int jRight = j + 1;
        int jLeft = j - 1;
        if (cube.transform.localRotation.eulerAngles.z < 1)// if 0, rocket is horizontal
        {
            while (jRight < rowLength)
            {
                if (playFieldCubes[i, jRight] != null)
                {
                    AddToSideList(rightSide, i, jRight);
                }
                jRight++;
            }
            while (jLeft > -1)
            {
                if (playFieldCubes[i, jLeft] != null)
                {
                    AddToSideList(leftSide, i, jLeft);
                }
                jLeft--;
            }
        }
        else // if vertical
        {
            while (iRight < columnLength)
            {
                if (playFieldCubes[iRight, j] != null)
                {
                    AddToSideList(leftSide, iRight, j);
                }
                iRight++;

            }
            while (iLeft > -1)
            {
                if (playFieldCubes[iLeft, j] != null)
                {
                    AddToSideList(leftSide, iLeft, j);
                }
                iLeft--;
            }
        }


    }
    // override for merged rocket
    public void RocketSideSearch(GameObject cube, ref List<SearchElement> downSide, ref List<SearchElement> upSide, ref List<SearchElement> leftSide,ref List<SearchElement> rightSide, int i = -2, int j = -2)
    {
        if (i < 0 || j < 0)
        {
            Debug.LogWarning("index is out of bonds");
            return;
        }

        int iRight = i+1;
        int iLeft = i-1;
        int jRight = j+1;
        int jLeft = j-1;

        while (jRight < rowLength)
        {
            if (playFieldCubes[i, jRight] != null)
            {
                AddToSideList(rightSide, i, jRight);
            }
            jRight++;
        }
        while (jLeft > -1 )
        {
            if (playFieldCubes[i, jLeft] != null)
            {
                AddToSideList(leftSide, i, jLeft);
            }
            jLeft--;
        }
        while (iRight < columnLength )
        {
            if (playFieldCubes[iRight, j] != null)
            {
                AddToSideList(upSide, iRight, j);
            }
            iRight++;

        }
        while (iLeft > -1)
        {
            if (playFieldCubes[iLeft, j] != null)
            {
                AddToSideList(downSide, iLeft, j);
            }
            iLeft--;
        }


    }

    public int NeighbourSearch(GameObject cube, int i = -1, int j = -1)
    {
        // for the first call find the object i,j
        FindCoordinate(cube, ref i, ref j);
        // if could not find throw exception as -1
        if (i == -1 || j == -1)
        {
            Debug.Log("Cube could not find");
            return -1;
        }

        Cube currentCube = cube.GetComponent<Cube>();
        SearchElement currentElement = new SearchElement(cube, i, j);

        // check if cube is already visited, if visited return
        if (searchList.Contains(currentElement)) return 0;
        // else add to searchList
        searchList.Add(currentElement);

        // exceptions
        if (currentCube.cubeType == Cube.CubeType.balloon) return 0;

        
        List<Cube.CubeType> allowedTypes;
        if (currentCube.cubeType == Cube.CubeType.rocket)
        {
            // only search rockets, if rocket is clicked
            allowedTypes = new List<Cube.CubeType>();
        }
        else
        {
            //special cubes that will be destroyed if nearby
            allowedTypes = new List<Cube.CubeType>() { Cube.CubeType.balloon };
        }
        allowedTypes.Add(currentCube.cubeType);
        // recursively call the function with the neighbour which is same type and in the range;
        if (i + 1 < columnLength && playFieldCubes[i + 1, j] != null && allowedTypes.Contains(playFieldCubes[i + 1, j].GetComponent<Cube>().cubeType))
        {
            NeighbourSearch(playFieldCubes[i + 1, j], i + 1, j);
        }
        if (i - 1 > -1 && playFieldCubes[i - 1, j] != null && allowedTypes.Contains(playFieldCubes[i - 1, j].GetComponent<Cube>().cubeType))
        {
            NeighbourSearch(playFieldCubes[i - 1, j], i - 1, j);
        }
        if (j + 1 < rowLength && playFieldCubes[i, j + 1] != null && allowedTypes.Contains(playFieldCubes[i, j + 1].GetComponent<Cube>().cubeType))
        {
            NeighbourSearch(playFieldCubes[i, j + 1], i, j + 1);
        }
        if (j - 1 > -1 && playFieldCubes[i, j - 1] != null && allowedTypes.Contains(playFieldCubes[i, j - 1].GetComponent<Cube>().cubeType))
        {
            NeighbourSearch(playFieldCubes[i, j - 1], i, j - 1);
        }
        return searchList.Count;
    }
    // Number of normal blocks in searchList
    public int SearchListBlockCount()
    {
        int blockSize = 1;
        Cube.CubeType clickedCubeType = searchList[0].cube.GetComponent<Cube>().cubeType;
        for (int i = 1; i < searchList.Count; i++)
        {
            if (searchList[i].cube.GetComponent<Cube>().cubeType == clickedCubeType)
                blockSize++;
        }
        return blockSize;
    }
    
    public void ClearSearchList()
    {
        searchList.Clear();
    }
    public List<SearchElement> GetSearchList()
    {
        return searchList;
    }

    // find the grid coordinate of given gameobject
    public void FindCoordinate(GameObject cube, ref int i, ref int j)
    {
        if (i == -1 || j == -1)
        {
            for (int a = 0; a < columnLength; a++)
            {
                for (int b = 0; b < rowLength; b++)
                {
                    if (playFieldCubes[a, b] != null  && playFieldCubes[a, b].GetInstanceID() == cube.GetInstanceID())
                    {
                        i = a;
                        j = b;
                    }
                }
            }
        }
    }

    //clear a grid cell 
    public void ClearACell(int row, int column)
    {
        playFieldCubes[row, column] = null;

        playFieldBool[row, column] = false;
    }

    // check if there is any ducks at the bottom. if yes, collect them
    public bool CheckForDucks()
    {
        bool found = false;
        for (int j = 0; j < rowLength; j++)
        {
            if (playFieldCubes[columnLength - 1, j] != null)
            {
                if (playFieldCubes[columnLength - 1, j].GetComponent<Cube>().cubeType == Cube.CubeType.duck)
                {
                    found = true;
                    GameObject current = playFieldCubes[columnLength - 1, j];
                    ClearACell(columnLength - 1, j);
                    GameObject targetUI = UIController.instance.UpdateGoals(current, -1);
                    AudioController.instance.PlayDuck();
                    if (targetUI != null)
                    {
                        current.transform.DOMove(targetUI.transform.position, 0.8f).SetEase(Ease.InBack);
                        current.transform.GetComponent<Button>().enabled = false;
                        current.transform.GetComponent<BoxCollider2D>().enabled = false;
                        current.transform.parent = UIController.instance.topBar.transform;
                        current.transform.SetAsLastSibling();

                        ParticleController.instance.GoalCollectParticHandler(targetUI.transform.position,0.8f);

                        Destroy(current, 0.8f);
                    }
                    else
                    {
                        Destroy(current);
                    }
                }
            }
        }
        return found;
    }
    #endregion

    #region utils
    private void AddToSideList(List<SearchElement> rightSide, int i, int j)
    {
        SearchElement current;
        current.cube = playFieldCubes[i, j];
        current.i = i;
        current.j = j;
        rightSide.Add(current);
    }

    private void FixTheHierarchy()
    {
        for (int i = columnLength - 1; i > -1; i--) // for length of a row
        {
            for (int j = 0; j < rowLength; j++) // for length of a column
            {
                if (playFieldCubes[i,j] != null)
                {
                    playFieldCubes[i, j].transform.SetAsLastSibling();
                }
            }
        }
    }
    #endregion

    #region Powerups
    public void MixTheGrid()
    {
        List<Vector2> visited = new List<Vector2>();
        int maxIteration = (columnLength - 1) * (rowLength - 1) / 2;


        for (int i = 0; i < maxIteration; i++)
        {
            // take two random coordinates that are not visited and not equal
            int iRand1 = -1, iRand2 = -1, jRand1 = -1, jRand2 = -1;
            while ( (iRand1 == iRand2 && jRand1 == jRand2) // two coords are  equal 
                ||  visited.Contains(new Vector2(iRand1,jRand1)) // node 1 is not visited
                ||  visited.Contains(new Vector2(iRand2,jRand2)) // node 2 is not visited
                )
            {
                iRand1 = UnityEngine.Random.Range(0, columnLength);
                jRand1 = UnityEngine.Random.Range(0, rowLength);
                iRand2 = UnityEngine.Random.Range(0, columnLength);
                jRand2 = UnityEngine.Random.Range(0, rowLength);
            }
            Debug.Log(iRand1 + " " + jRand1 + " " + iRand2 + " " + jRand2 );

            GameObject tempObj= playFieldCubes[iRand1, jRand1];

            playFieldCubes[iRand2, jRand2].transform.DOMove(playFieldCoords[iRand1,jRand1], 1f).SetEase(Ease.InOutBack);
            tempObj.transform.DOMove(playFieldCoords[iRand2,jRand2], 1f).SetEase(Ease.InOutBack);

            playFieldCubes[iRand1, jRand1] = playFieldCubes[iRand2, jRand2];
            playFieldCubes[iRand2, jRand2] = tempObj;

            FixTheHierarchy();
        }
    }

    public void PowerUpHandler(PowerUpController.PowerUpType type , GameObject clickedCube)
    {
        InputController.instance.SetTouch(false);
        Cube cube;
        clickedCube.TryGetComponent<Cube>(out cube);
        int i = -1, j = -1;
        FindCoordinate(clickedCube, ref i, ref j);
        if (type == PowerUpController.PowerUpType.Hammer)
            HammerHandler(clickedCube, cube, i, j);
        else if (type == PowerUpController.PowerUpType.Fist)
        {
            FistHandler(i);
        }
        else if (type == PowerUpController.PowerUpType.Anchor)
        {
            AnchorHandler(j);
        }

    }

    private void FistHandler(int i)
    {
        int j = 0;
        List<SearchElement> rowList = new List<SearchElement>();
        while (j < rowLength)
        {
            if (playFieldCubes[i, j] != null)
            {
                AddToSideList(rowList, i, j);
            }
            j++;
        }
        Cube temp = new Cube();
        StartCoroutine(temp.DirectedDestroy(rowList, 0.06f));
        PowerUpController.instance.powerUpIsActive = false;
        Invoke("DropCubes",1f);

    }
    private void AnchorHandler(int j)
    {
        int i = 0;
        List<SearchElement> columnList = new List<SearchElement>();
        while (i < columnLength)
        {
            if (playFieldCubes[i, j] != null)
            {
                AddToSideList(columnList, i, j);
            }
            i++;
        }
        Cube temp = new Cube();
        StartCoroutine(temp.DirectedDestroy(columnList, 0.06f));
        PowerUpController.instance.powerUpIsActive = false;
        Invoke("DropCubes",1f);
    }

    private void HammerHandler(GameObject clickedCube, Cube cube, int i, int j)
    {
        GameObject targetUI = UIController.instance.UpdateGoals(clickedCube, -1);
        if (targetUI != null)// if goal
        {
            if (cube.cubeType == Cube.CubeType.duck)
            {
                // fill if duck needs an action
            }
            else if (cube.cubeType == Cube.CubeType.balloon)
            {
                ClearACell(i, j);
                AudioController.instance.PlayBalloonExplode();

                clickedCube.transform.GetComponent<Button>().enabled = false;
                clickedCube.transform.GetComponent<BoxCollider2D>().enabled = false;

                ParticleController.instance.GoalCollectParticHandler(targetUI.transform.position);
                AudioController.instance.PlayCubeCollect();

                Destroy(clickedCube);
            }
            else if (cube.cubeType == Cube.CubeType.rocket)
            {
                cube.RocketChooseExecution();
                return;
            }
            else
            {
                ClearACell(i, j);

                AudioController.instance.PlayCubeExplode();
                ParticleController.instance.CubeExplodeParticHandler(cube.cubeType, clickedCube.transform.position);
                clickedCube.transform.DOMove(targetUI.transform.position, 0.8f).SetEase(Ease.InBack);
                AudioController.instance.PlayCubeCollect(0.8f);
                ParticleController.instance.GoalCollectParticHandler(targetUI.transform.position, 0.8f);

                clickedCube.transform.GetComponent<Button>().enabled = false;
                clickedCube.transform.GetComponent<BoxCollider2D>().enabled = false;
                clickedCube.transform.parent = UIController.instance.topBar.transform;
                clickedCube.transform.SetAsLastSibling();
                Destroy(clickedCube, 0.8f);
            }
        }
        else // if not goal
        {
            if (cube.cubeType == Cube.CubeType.duck)
            {
                // fill if duck needs an action
            }
            else if (cube.cubeType == Cube.CubeType.balloon)
            {
                ClearACell(i, j);
                AudioController.instance.PlayBalloonExplode();

                Destroy(clickedCube);
            }
            else if (cube.cubeType == Cube.CubeType.rocket)
            {
                cube.RocketChooseExecution();
                return;
            }
            else
            {
                ClearACell(i, j);

                AudioController.instance.PlayCubeExplode();
                ParticleController.instance.CubeExplodeParticHandler(cube.cubeType, clickedCube.transform.position);

                Destroy(clickedCube);
            }
        }


        PowerUpController.instance.powerUpIsActive = false;
        DropCubes();
    }

    #endregion
}

