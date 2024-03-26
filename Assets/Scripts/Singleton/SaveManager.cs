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
    private PlayerDataStruct playerData;

    private void Start()
    {
        playerData = new PlayerDataStruct();
        LoadGameFromDisk();
        SceneController.Instance.LoadMainMenu();
    }

    /// <summary>
    /// Save the progress of the current play session.
    /// </summary>
    public void SavePlayerData()
    {
        playerData.fullCam = PlayerData.Instance.fullCam;

        playerData.bestTimes = PlayerData.Instance.bestTimes;
        playerData.noHits = PlayerData.Instance.noHits;
        playerData.expertNoHits = PlayerData.Instance.expertNoHits;
        playerData.challenges = PlayerData.Instance.challenges;
        playerData.expertChallenges = PlayerData.Instance.expertChallenges;

        playerData.dmgMult = PlayerData.Instance.dmgMult;
        playerData.equipped = PlayerData.Instance.equipped;
        playerData.unlocked = PlayerData.Instance.unlocked;
        playerData.expertMode = PlayerData.Instance.expertMode;
    }

    /// <summary>
    /// Load the progress of the most recent play session.
    /// </summary>
    public void LoadPlayerData()
    {
        PlayerData.Instance.newSave = false;
        PlayerData.Instance.fullCam = playerData.fullCam;

        PlayerData.Instance.bestTimes = playerData.bestTimes;
        PlayerData.Instance.noHits = playerData.noHits;
        PlayerData.Instance.expertNoHits = playerData.expertNoHits;
        PlayerData.Instance.challenges = playerData.challenges;
        PlayerData.Instance.expertChallenges = playerData.expertChallenges;

        PlayerData.Instance.dmgMult = playerData.dmgMult;
        PlayerData.Instance.equipped = playerData.equipped;
        PlayerData.Instance.unlocked = playerData.unlocked;
        PlayerData.Instance.expertMode = playerData.expertMode;
    }

    /// <summary>
    /// Save game progress stored in the application onto the local disk.
    /// Location: \AppData\LocalLow\DefaultCompany\Salvation Remastered
    /// </summary>
    private void SaveGameToDisk()
    {
        SavePlayerData();

        // Save audio settings last
        playerData.masterVolume = AudioController.Instance.GetVolume("Master");
        playerData.musicVolume = AudioController.Instance.GetVolume("Music");
        playerData.sfxVolume = AudioController.Instance.GetVolume("SFX");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");
        bf.Serialize(file, playerData);
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
                playerData = (PlayerDataStruct)bf.Deserialize(file);
                LoadPlayerData();

                AudioController.Instance.ChangeVolume("Master", playerData.masterVolume);
                AudioController.Instance.ChangeVolume("Music", playerData.musicVolume);
                AudioController.Instance.ChangeVolume("SFX", playerData.sfxVolume);
            }
            catch (Exception ex)
            {               
                Debug.Log(ex);
            }
            file.Close();            
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameToDisk();
    }
}

[Serializable]
public struct PlayerDataStruct
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool fullCam;

    public Dictionary<string, float> bestTimes;
    public HashSet<string> noHits;
    public HashSet<string> expertNoHits;
    public HashSet<string> challenges;
    public HashSet<string> expertChallenges;

    public int dmgMult;
    public bool[] equipped;
    public bool[] unlocked;
    public bool expertMode;
}