using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use to make layer references more readable.
/// </summary>
namespace LayerManager
{
    public enum Layer
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Placeholder, // Placeholder
        Water,
        UI,
        Terrain,
        Player,
        PlayerAttack,
        Enemy,
        EnemyAttack,
        Camera,
        PassThroughTerrain
    }
}
