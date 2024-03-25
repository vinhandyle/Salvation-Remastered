using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Save system for cross-scene and play session data persistence.
/// </summary>
public class SaveManager : Singleton<SaveManager>
{    
    public PlayerController player;

    [SerializeField] private string firstLevelToLoad;
    [SerializeField] private int checkpointStart;


    private void Start()
    {
        LoadGameFromDisk();
        SceneController.Instance.LoadMainMenu();
    }

    #region Save

    /// <summary>
    /// Save the progress of the current play session.
    /// </summary>
    public void SavePlayerInfo(string level = "")
    {
        
    }

    #endregion

    #region Load

    /// <summary>
    /// Load the progress of the most recent play session.
    /// </summary>
    private IEnumerator LoadPlayerInfo(string level, int checkpointID = -1)
    {       
        SceneController.Instance.LoadScene(level);

        // Wait for player object in next scene to link with this
        while (SceneManager.GetActiveScene().name != level || player == null)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Load the specified level with the player at the specified position.
    /// </summary>
    public void LoadLevel(string level, Vector2 pos)
    {
        StartCoroutine(LoadPlayerInfo(level));
    }

    /// <summary>
    /// Load the progress of the most recent play session.
    /// </summary>
    public void LoadGame()
    {
        //LoadLevel(playerData.level, new Vector2(playerData.posX, playerData.posY));
    }

    #endregion

    #region Disk

    /// <summary>
    /// Save game progress stored in the application onto the local disk.
    /// Location: \AppData\LocalLow\DefaultCompany\Salvation Remastered
    /// </summary>
    private void SaveGameToDisk()
    {
        // Do this in case the player force closes the application
        if (GameStateManager.Instance.currentState == GameStateManager.GameState.PAUSED)
            GameStateManager.Instance.TogglePause();

        if (GameStateManager.Instance.currentState == GameStateManager.GameState.RUNNING)
            SavePlayerInfo(SceneController.Instance.currentScene);
        
        // Save audio settings last
        PlayerData.Instance.masterVolume = AudioController.Instance.GetVolume("Master");
        PlayerData.Instance.musicVolume = AudioController.Instance.GetVolume("Music");
        PlayerData.Instance.sfxVolume = AudioController.Instance.GetVolume("SFX");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");
        bf.Serialize(file, PlayerData.Instance);
        file.Close();
    }

    /// <summary>
    /// Reads save data from local disk and loads it into the application.
    /// Location: \AppData\LocalLow\DefaultCompany\Salvation Remastered
    /// </summary>
    private void LoadGameFromDisk()
    {
        // Open previous save
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);

            try
            {
                PlayerData.Instance.LoadData(bf.Deserialize(file));
            }
            catch (Exception ex)
            {               
                Debug.Log(ex);
            }
            file.Close();            
        }

        AudioController.Instance.ChangeVolume("Master", PlayerData.Instance.masterVolume);
        AudioController.Instance.ChangeVolume("Music", PlayerData.Instance.musicVolume);
        AudioController.Instance.ChangeVolume("SFX", PlayerData.Instance.sfxVolume);
    }

    #endregion

    private void OnApplicationQuit()
    {
        SaveGameToDisk();
    }
}
