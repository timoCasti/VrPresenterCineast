using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class AudioLoader : MonoBehaviour
    {
        private string _lastUrl;

        private bool _loaded;
        private AudioSource audioSource;


        // Use this for initialization
        private void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        private IEnumerator LoadAudio(string url)
        {
            if (_loaded && _lastUrl.Equals(url))
            {
                Play();
                yield break;
            }

            using (var www = new WWW(url))
            {
                yield return www;

                if (www.isDone)
                {
                    var audioClip = www.GetAudioClip(false, true);
                    audioSource.clip = audioClip;
                    audioSource.volume = 0.2f;
                    audioSource.loop = true;
                    audioSource.Play();
                    _loaded = true;
                    _lastUrl = url;
                }
            }
        }

        /// <summary>
        ///     Plays the audio which was previosuly loaded via ReloadAudio().
        /// </summary>
        public void Play()
        {
            if (_loaded)
                audioSource.Play();
            else
                ReloadAudio(_lastUrl);
        }

        /// <summary>
        /// </summary>
        public void Stop()
        {
            audioSource.Stop();
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        public void ReloadAudio(string url)
        {
            if (!string.IsNullOrEmpty(url)) StartCoroutine(LoadAudio(url));
        }
    }
}