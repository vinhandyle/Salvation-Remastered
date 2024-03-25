using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines all scene-related actions.
/// </summary>
public class SceneController : Singleton<SceneController>
{
    public string currentScene { get; private set; }
    public string currentLevel { get; private set; }
    public string prevLevel { get; private set; }
    private const string menuScene = "Menu";

    /// <summary>
    /// Returns true if the player is in a combat or exploration level.
    /// </summary>
    public bool InLevel()
    {
        return currentLevel != "Menu" && currentLevel != "Hub";
    }

    /// <summary>
    /// Checks if the specified scene is currently open.
    /// </summary>
    public bool IsSceneOpen(string scene)
    {
        return GetAllScenes().Any(s => s.name == scene);
    }

    /// <summary>
    /// Get a list of all open scenes.
    /// </summary>
    public Scene[] GetAllScenes()
    {
        int countLoaded = SceneManager.sceneCount;
        Scene[] loadedScenes = new Scene[countLoaded];

        for (int i = 0; i < countLoaded; i++)
        {
            loadedScenes[i] = SceneManager.GetSceneAt(i);
        }

        return loadedScenes;
    }

    #region Scene Load/Unload

    /// <summary>
    /// Boot up the main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(menuScene);
        GameStateManager.Instance.UpdateState(GameStateManager.GameState.PREGAME);
    }

    /// <summary>
    /// Loads the scene with the given name.
    /// </summary>
    public void LoadScene(string scene, bool single = true)
    {
        LoadSceneMode mode = single ? LoadSceneMode.Single : LoadSceneMode.Additive;
        prevLevel = currentLevel;

        AsyncOperation ao = SceneManager.LoadSceneAsync(scene, mode);
        StartCoroutine(SceneProgress(ao, scene, 0));
        currentScene = scene;
        currentLevel = single ? scene : currentLevel;

        if (scene == menuScene)
            GameStateManager.Instance.UpdateState(GameStateManager.GameState.PREGAME);
        else if (mode == LoadSceneMode.Single)
            GameStateManager.Instance.UpdateState(GameStateManager.GameState.RUNNING);

        // Play music
        // AudioController.Instance.ClearEffects();

        switch (scene)
        {
            case menuScene:
                //AudioController.Instance.PlayTrack(0);
                break;
        }
    }

    /// <summary>
    /// Unloads the scene with the given name.
    /// </summary>
    public void UnloadScene(string scene)
    {
        prevLevel = currentLevel;

        AsyncOperation ao = SceneManager.UnloadSceneAsync(scene);
        StartCoroutine(SceneProgress(ao, scene, 1));
        currentScene = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Use to track the progress of loading a scene.
    /// </summary>
    private IEnumerator SceneProgress(AsyncOperation ao, string scene, int type)
    {
        if (ao == null)
        {
            Debug.LogError(string.Format("Unable to {0} {1}", type == 0 ? "load" : "unload", scene));
            yield break;
        }

        while (!ao.isDone)
        {
            Debug.Log(string.Format("{0} {1} in progress: {2}%", type == 0 ? "Loading" : "Unloading", scene, Mathf.Clamp(ao.progress / 0.9f, 0, 1) * 100));
            yield return null;
        }

        // Finish loading scene before checking for gauntlet menu
        GauntletMenu gMenu = FindObjectOfType<GauntletMenu>();
        if (gMenu != null) gMenu.SetActive(false);
    }

    #endregion
}
