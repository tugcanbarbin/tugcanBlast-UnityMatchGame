using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public GameObject topBar;

    public int moves = 1;

    [System.Serializable]
    public struct Goal
    {
        public GameObject block;
        public int number;
    }
    [Header("Goal can be maximum size of 3")]
    [NonReorderable] public Goal[] goals;

    TextMeshProUGUI movesText;
    GameObject goalParent;
    List<TextMeshProUGUI> goalTexts;
    List<GameObject> goalModels;

    public bool goalsCompleted = false;
    public bool isGameEnded = false;

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        goalTexts = new List<TextMeshProUGUI>();
        goalModels = new List<GameObject>();
        // get the references
        movesText = topBar.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        goalParent = topBar.transform.GetChild(0).GetChild(goals.Length).gameObject;
        goalParent.SetActive(true);

        // assign the moves
        movesText.text = moves.ToString();

        // assign goals due to number of goals and given prefabs
        for (int i = 0; i < goals.Length; i++)
        {
            TextMeshProUGUI tempText = goalParent.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();

            GameObject tempGoalModel = Instantiate(goals[i].block, goalParent.transform.GetChild(i));
            tempGoalModel.transform.localPosition = Vector3.zero;
            // disable the cube functions
            tempGoalModel.transform.GetComponent<Button>().enabled = false;
            tempGoalModel.transform.GetComponent<BoxCollider2D>().enabled = false;

            // assign the goal text and arrange the hierarchy for visibility
            tempText.transform.SetAsLastSibling();
            tempText.text = goals[i].number.ToString();
            goalTexts.Add(tempText);
            goalModels.Add(tempGoalModel);

        }

    }

    public void UpdateMoves(int add)
    {
        moves += add;
        if (moves < 0) moves = 0;
        movesText.text = moves.ToString();

    }
    public GameObject UpdateGoals(GameObject block, int add)
    {
        GameObject goal = null;
        for (int i = 0; i < goals.Length; i++)
        {
            // SAME block and >0
            if (block.CompareTag(goals[i].block.tag) && goals[i].number>0)
            {
                goal = goalTexts[i].gameObject;

                var screenToWorldPosition = Camera.main.ScreenToWorldPoint(goal.transform.position);

                //Debug.Log(goal.transform.position); // bu
                //Debug.Log(screenToWorldPosition);

                goals[i].number += add;
                
                // precaution for possible negative number
                if (goals[i].number < 0) goals[i].number = 0;

                DOTween.Complete(this.GetInstanceID());
                DOTween.Sequence().SetId(this.GetInstanceID()).Append(goalModels[i].transform.DOScale(goalModels[i].transform.localScale * 1.25f, 0.3f))
                    .Append(goalModels[i].transform.DOScale(goalModels[i].transform.localScale , 0.3f));

                goalTexts[i].text = goals[i].number.ToString();

                if (goals[i].number < 1)
                {
                    Debug.Log("single goal completed " + " Goal tick");
                    goalTexts[i].text = "OK";
                }
            }
        }
        goalsCompleted = true;
        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].number > 0)
            {
                goalsCompleted = false;
            }
        }

        if (goalsCompleted || moves < 1)
        {
            isGameEnded = true;
            // finish the game
            if(goalsCompleted)
                Debug.Log("level completed");
            else
                Debug.Log("level failed, out of moves");
        }

        return goal;
    }
}
