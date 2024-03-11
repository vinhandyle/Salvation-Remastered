using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows an object to drain energy [from the player] on contact.
/// </summary>
public class DrainingObject : MonoBehaviour
{
    [SerializeField] private int drainAmt;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerShooter>().DrainEnergy(drainAmt);
        }
    }
}
