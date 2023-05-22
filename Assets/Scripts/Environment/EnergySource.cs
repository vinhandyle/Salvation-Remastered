using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySource : MonoBehaviour
{
    public float rechargeMult { get => _rechargeMult; }
    [SerializeField] private float _rechargeMult = 1;
}
