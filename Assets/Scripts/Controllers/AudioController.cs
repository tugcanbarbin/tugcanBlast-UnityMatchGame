using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    [SerializeField] AudioSource source;
    [SerializeField] AudioClip balloonExplode;
    [SerializeField] AudioClip cubeCollect;
    [SerializeField] AudioClip cubeExplode;
    [SerializeField] AudioClip duck;

    private bool audioActive = true;

    private void Awake()
    {
        instance = this;
    }

    public void PlayBalloonExplode(float delay = 0)
    {
        if (audioActive)
            StartCoroutine(BalloonExplodeCoroutine(delay));
    }
    public void PlayCubeCollect(float delay = 0)
    {
        if (audioActive)
            StartCoroutine(CubeCollectCoroutine(delay));
    }
    public void PlayCubeExplode(float delay = 0)
    {
        if (audioActive)
            StartCoroutine(CubeExplodeCoroutine(delay));
    }
    public void PlayDuck(float delay = 0)
    {
        if(audioActive)
            StartCoroutine(DuckCoroutine(delay));
    }
    public void SetAudioActive(bool b = true)
    {
        audioActive = b;
    }   

    #region coroutines
    IEnumerator BalloonExplodeCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(balloonExplode);
    }
    IEnumerator CubeCollectCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(cubeCollect);
    }
    IEnumerator CubeExplodeCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(cubeExplode);
    }
    IEnumerator DuckCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(duck);
    }
    #endregion
}
