using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinMenu : MonoBehaviour
{
    [SerializeField] private List<Image> imgs;

    private void Awake()
    {
        AchievementManager am = FindObjectOfType<AchievementManager>();
        List<int> ranks = am.SpecialRanksAchieved();

        for (int i = 0; i < imgs.Count; i++)
        {
            imgs[i].enabled = ranks.Contains(i);
        }

        // Center visible ranks
        if (ranks.Count == 1)
        {
            imgs[ranks[0]].transform.localPosition = new Vector2(0, 35);
        }
        else
        {
            imgs[ranks[0]].transform.localPosition = new Vector2(-35, 35);
            imgs[ranks[1]].transform.localPosition = new Vector2(35, 35);
        }
    }
}
