using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [SerializeField] private HealthManager player;
    [SerializeField] private List<HealthManager> boss;
    private string currentLevel;
    private bool challengeCompleted;

    private void Awake()
    {
        currentLevel = SceneController.Instance.currentLevel;
    }

    private void Update()
    {
        if (SceneController.Instance.currentScene == "Win")
        {
            if (player.noHit)
                PlayerData.Instance.UpdateAchievement(currentLevel, 0);

            challengeCompleted = IsChallengeCompleted();
            if (challengeCompleted)
                PlayerData.Instance.UpdateAchievement(currentLevel, 1);
        }
    }

    private bool IsChallengeCompleted()
    {
        switch (currentLevel)
        {
            case "Boss01":
                return NoUpgradesChallenge();

            default:
                return false;
        }
    }

    #region Challenges

    /// <summary>
    /// Player can only equip Gun and have no movement upgrades.
    /// </summary>
    /// <returns></returns>
    private bool NoUpgradesChallenge()
    {
        for (int i = 0; i < PlayerData.Instance.equipped.Length; i++)
        {
            if (i != 0 && PlayerData.Instance.equipped[i])
                return false;
        }

        return !PlayerData.Instance.canDash &&
            !PlayerData.Instance.canWallJump &&
            !PlayerData.Instance.canDoubleJump;
    }

    #endregion

    public List<int> SpecialRanksAchieved()
    {
        List<int> ranks = new List<int>();

        if (PlayerData.Instance.dmgMult > 0)
        {
            if (PlayerData.Instance.expertMode)
            {
                if (player.noHit) ranks.Add(2);
                if (challengeCompleted) ranks.Add(3);
            }
            else
            {
                if (player.noHit) ranks.Add(0);
                if (challengeCompleted) ranks.Add(1);
            }
        }

        return ranks;
    }
}
