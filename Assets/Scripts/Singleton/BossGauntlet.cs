using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGauntlet : Singleton<BossGauntlet>
{   
    public string gauntletRoot { get; private set; }
    public string gauntletStart { get; private set; }
    public float timer { get; private set; }

    public bool inGauntlet { get; private set; }

    public bool noHit { get; private set; }

    public void InitGauntlet(string root, string start)
    {
        gauntletRoot = root;
        gauntletStart = start;
        inGauntlet = true;
        noHit = true;
        timer = 0;
    }

    public void ExitGauntlet()
    {
        gauntletRoot = "";
        gauntletStart = "";
        inGauntlet = false;
    }

    public void ResetGauntlet()
    {
        InitGauntlet(gauntletRoot, gauntletStart);
    }

    #region Achievement

    public void AddToTimer(float amt)
    {
        timer += amt;
    }

    public void UpdateNoHit(bool noHit)
    {
        this.noHit = this.noHit && noHit;
    }

    #endregion
}
