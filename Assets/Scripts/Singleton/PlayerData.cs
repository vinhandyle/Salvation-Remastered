using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : Singleton<PlayerData>
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    public Dictionary<string, float> bestTimes;
    public HashSet<string> noHits;
    public HashSet<string> expertNoHits;
    public HashSet<string> challenges;
    public HashSet<string> expertChallenges;

    public int dmgMult;
    public bool expertMode;
    public bool canDash;
    public bool canWallJump;
    public bool canDoubleJump;

    public int currentEquipped;
    public bool[] equipped;
    public bool[] unlocked;
    public bool unlockAll { get => true; }

    protected override void Awake()
    {
        base.Awake();

        bestTimes = new Dictionary<string, float>();
        noHits = new HashSet<string>();
        expertNoHits = new HashSet<string>();
        challenges = new HashSet<string>();
        expertChallenges = new HashSet<string>();

        equipped = new bool[7];
        unlocked = new bool[7];
        equipped[0] = true;        
        unlocked[0] = true;
        dmgMult = 1;

        for (int i = 1; i < equipped.Length; i++)
            equipped[i] = true;
    }

    public void LoadData(object data)
    { 
    
    }

    public void UpdateBestTime(string key, float time)
    {
        if (dmgMult > 0)
        {
            if (!bestTimes.ContainsKey(key))
                bestTimes.Add(key, time);
            else if (time < bestTimes[key])
                bestTimes[key] = time;
        }
    }

    public void UpdateAchievement(string key, int type)
    {
        if (dmgMult > 0)
        {
            if (type == 0)
            {
                if (key.EndsWith('E'))
                {
                    if (!expertNoHits.Contains(key))
                        expertNoHits.Add(key);
                }
                else
                {
                    if (!noHits.Contains(key))
                        noHits.Add(key);
                }
            }
            else
            {
                if (key.EndsWith('E'))
                {
                    if (!expertChallenges.Contains(key))
                        expertChallenges.Add(key);
                }
                else
                {
                    if (!challenges.Contains(key))
                        challenges.Add(key);
                }
            }
        }
    }
}


