using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The buttons in the menu.
/// </summary>
public class MenuButton : MonoBehaviour
{
    [SerializeField] private bool levelOnly;
    [SerializeField] private bool notNewGame;

    protected virtual void Awake()
    {
        if (levelOnly) gameObject.SetActive(SceneController.Instance.InLevel());
        if (notNewGame) gameObject.SetActive(!PlayerData.Instance.newSave);
    }

    #region Main Menu

    /// <summary>
    /// Begin new save file.
    /// </summary>
    public void NewGame()
    {
        PlayerData.Instance.ResetData();
        SceneController.Instance.LoadScene("Hub");
    }

    /// <summary>
    /// Continue from the previous save file progress.
    /// </summary>
    public void ContinueGame()
    {        
        SceneController.Instance.LoadScene("Hub");
    }

    /// <summary>
    /// Return to the main menu.
    /// </summary>
    public void ReturnToMenu()
    {
        PlayerData.Instance.newSave = false;
        SceneController.Instance.LoadScene("Menu");
    }

    /// <summary>
    /// Close the game.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Level

    /// <summary>
    /// [Un]pause the game.
    /// </summary>
    public void TogglePause()
    {
        GameStateManager.Instance.TogglePause();
    }

    /// <summary>
    /// Return to the hub level.
    /// </summary>
    public void ReturnToHub()
    {
        SceneController.Instance.LoadScene("Hub");
        BossGauntlet.Instance.ExitGauntlet();
    }

    /// <summary>
    /// Reset all enemies and return to the beginning of the level.
    /// </summary>
    public void RestartLevel()
    {       
        if (BossGauntlet.Instance.inGauntlet)
        {
            SceneController.Instance.LoadScene(BossGauntlet.Instance.gauntletStart);
            SceneController.Instance.LoadScene(BossGauntlet.Instance.gauntletRoot, false);
            BossGauntlet.Instance.ResetGauntlet();
        }
        else
        {
            SceneController.Instance.LoadScene(SceneController.Instance.currentLevel);
        }
    }

    public void ContinueGauntlet(string finalLevel)
    {
        string currentLevel = SceneController.Instance.currentLevel;
        int nextLevel = int.Parse(currentLevel[4].ToString()) * 10 +
                        int.Parse(currentLevel[5].ToString()) + 1;

        if (currentLevel == finalLevel)
        {
            string gauntletRoot = BossGauntlet.Instance.gauntletRoot;

            PlayerData.Instance.UpdateBestTime(gauntletRoot + (PlayerData.Instance.expertMode ? "E" : ""), BossGauntlet.Instance.timer);

            if (BossGauntlet.Instance.noHit)
                PlayerData.Instance.UpdateAchievement(gauntletRoot, 0);

            ReturnToHub();
        }
        else
        {
            FindObjectOfType<GauntletMenu>().SetActive(false);
            SceneController.Instance.LoadScene(
                string.Format("Boss{0}{1}", nextLevel / 10, nextLevel % 10)
                );
            SceneController.Instance.LoadScene(gameObject.scene.name, false);
        }
    }

    #endregion
}
