using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

// written by Google Gemini 3 Pro (i'm too lazy okay sorry also im running low on time and there's more important stuff to do :wilted_rose:)
// checked & revised by andy
public class LoadingScreen : MonoBehaviour
{
    [Header("Fake Load Settings")]
    [SerializeField] float minWaitTime = 1.5f;
    [SerializeField] float maxWaitTime = 3.5f;

    [Header("Optional UI Refs")]
    [SerializeField] CanvasGroup uiCanvasGroup; // if fading in/out is desired

    private void Start()
    {
        // safety check in case someone tries to play the loading scene directly in editor
        if (string.IsNullOrEmpty(SceneLoader.TargetSceneName))
        {
            Debug.LogWarning("set a target scene dumbass");
            SceneLoader.TargetSceneName = "MainMenu"; // whatever default is
        }

        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        // fade in
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0;
            yield return uiCanvasGroup.DOFade(1, 0.5f).WaitForCompletion();
        }

        // fake wait
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        // load scene async
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneLoader.TargetSceneName);
        op.allowSceneActivation = false; // pause it right at 90% ready

        // wait until it's actually loaded in memory
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // fade before switch
        if (uiCanvasGroup != null)
        {
            yield return uiCanvasGroup.DOFade(0, 0.5f).WaitForCompletion();
        }

        // finish
        op.allowSceneActivation = true;
    }
}