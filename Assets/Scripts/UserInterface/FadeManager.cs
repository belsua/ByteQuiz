using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    GameObject canvas;
    Image fadeImage;
    AudioSource audioSource;

    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Backdrop");
        fadeImage = canvas.GetComponent<Image>();
        audioSource = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioSource>();
    }

    public void FadeToScene(string sceneName)
    {
        canvas.GetComponent<Canvas>().sortingOrder = 7;
        StartCoroutine(FadeOutIn(sceneName));
        if (audioSource != null) StartCoroutine(FadeAudio(0));
    }

    IEnumerator FadeOutIn(string sceneName)
    {
        yield return StartCoroutine(Fade(1));
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Fade(float target)
    {
        float time = 0;
        float startAlpha = fadeImage.color.a;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, target, time / duration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }
    }

    public IEnumerator FadeAudio(float target)
    {
        float time = 0;
        float startVolume = audioSource.volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, target, time / duration);
            yield return null;
        }
    }
}
