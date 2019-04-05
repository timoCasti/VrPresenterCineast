using System;
using System.Collections;
using UnityEngine;

public class ClosenessDetector : MonoBehaviour
{
    private AudioSource audioSource;
    private bool downloading;

    public float maxDistance = 2;
    private bool playing;
    public string url;

    private WWW www = null;


    // Use this for initialization
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }


    private void Update()
    {
        var cameraPosition = Camera.allCameras[0].transform.position;
        var objectPosition = gameObject.transform.position;

        if (!string.IsNullOrEmpty(url) && audioSource.clip == null && !downloading)
        {
            downloading = true;
            StartCoroutine(LoadAudio(url));
        }

        var dist = Vector3.Distance(cameraPosition, objectPosition);

        if (Math.Abs(dist) < maxDistance)
            Play();
        else
            Stop();
    }

    private IEnumerator LoadAudio(string url)
    {
        using (var www = new WWW(url))
        {
            yield return www;

            if (www.isDone)
            {
                var audioClip = www.GetAudioClip(false, true);
                audioSource.clip = audioClip;
            }
        }
    }


    /// <summary>
    /// </summary>
    public void Play()
    {
        if (audioSource != null && audioSource.clip != null && playing == false)
        {
            audioSource.Play();
            playing = true;
        }
    }

    /// <summary>
    /// </summary>
    public void Stop()
    {
        if (audioSource != null && audioSource.clip != null && playing)
        {
            audioSource.Stop();
            playing = false;
        }
    }
}