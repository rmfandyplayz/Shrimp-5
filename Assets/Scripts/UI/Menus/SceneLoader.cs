using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // stored here so the loading scene knows where to go next
    public static string TargetSceneName;

    public static void Load(string sceneName)
    {
        TargetSceneName = sceneName;
        SceneManager.LoadScene("FakeLoadingScene");
    }
}