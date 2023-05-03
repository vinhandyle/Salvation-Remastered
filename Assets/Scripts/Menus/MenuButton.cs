using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The buttons in the menu.
/// </summary>
public class MenuButton : MonoBehaviour
{
    [SerializeField] private bool levelOnly;

    private void Awake()
    {
        if (levelOnly) gameObject.SetActive(SceneController.Instance.InLevel());
    }

    /// <summary>
    /// Begin new save file.
    /// </summary>
    public void NewGame()
    {
        // Prep new save file
        SceneController.Instance.LoadScene("Hub");
    }

    /// <summary>
    /// Continue from the previous save file progress.
    /// </summary>
    public void ContinueGame()
    {
        // Open submenu with 3 save slots
        SaveManager.Instance.LoadGame();
    }

    /// <summary>
    /// [Un]pause the game.
    /// </summary>
    public void TogglePause()
    {
        GameStateManager.Instance.TogglePause();
    }

    /// <summary>
    /// Reset all enemies and return to the beginning of the level.
    /// </summary>
    public void RestartLevel()
    {
        SceneController.Instance.LoadScene(SceneController.Instance.currentLevel);
    }

    /// <summary>
    /// Return to the hub level.
    /// </summary>
    public void ReturnToHub()
    {
        SceneController.Instance.LoadScene("Hub");
    }

    /// <summary>
    /// Return to the main menu.
    /// </summary>
    public void ReturnToMenu()
    {
        SceneController.Instance.LoadScene("Menu");
    }

    /// <summary>
    /// Close the game.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
