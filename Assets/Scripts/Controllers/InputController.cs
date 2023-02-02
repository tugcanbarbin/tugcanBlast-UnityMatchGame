using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController instance;

    GameController gameController;

    bool isStarted = false;
    [HideInInspector] public int currentEventNumber = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameController = GameController.instance;
    }
    public void SetTouch(bool b)
    {
        isStarted = b;
    }
    // in order to wait for chain of rocket animations
    public void AddCurrentEvents(int number)
    {
        currentEventNumber += number;

        if (currentEventNumber == 0)
        {
            GameController.instance.DropCubes();
        }
        else
        {
            isStarted = false;
        }
    }
    public IEnumerator AddCurrentEventsWithDelay(int number, float time)
    {
        yield return new WaitForSeconds(time);
        AddCurrentEvents(number);
    }

    public IEnumerator SetTouchWithDelay(bool b, float time)
    {
        yield return new WaitForSeconds(time);
        if (!gameController.CheckForDucks())
        {
            isStarted = b;
        }
        else
        {
            gameController.DropCubes();
        }
    }

    public bool IsStarted()
    {
        return isStarted;
    }

}
