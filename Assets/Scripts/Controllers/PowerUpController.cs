using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    public static PowerUpController instance;

    public enum PowerUpType
    {
        Hammer,
        Fist, 
        Anchor
    }

    public bool powerUpIsActive = false;
    public PowerUpType currentPowerUp = new PowerUpType();

    private void Awake()
    {
        instance = this;
    }

    public void Dice()
    {
        if (!InputController.instance.IsStarted())
            return;
        InputController.instance.SetTouch(false);
        GameController.instance.MixTheGrid();
        StartCoroutine(InputController.instance.SetTouchWithDelay(true,1.1f));
    }

    public void Hammer()
    {

        powerUpIsActive = true;
        currentPowerUp = PowerUpType.Hammer;

    }
    public void Anchor()
    {

        powerUpIsActive = true;
        currentPowerUp = PowerUpType.Anchor;
    }
    public void Fist()
    {

        powerUpIsActive = true;
        currentPowerUp = PowerUpType.Fist;

    }

}
