using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    [SerializeField] private float moveSpeedMult;
    [SerializeField] private float fallMultiplierBoost;
    [SerializeField] private float terminalVelocityMult;
    [SerializeField] private float jumpHeightMult;
    [SerializeField] private float wallJumpMult;
    [SerializeField] private float dashSpeedMult;

    [SerializeField] private bool gravityMod;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player)
        {
            if (gravityMod)
            {
                Vector2 v = player.GetComponent<Rigidbody2D>().velocity;
                player.GetComponent<Rigidbody2D>().velocity = new Vector2(v.x, 0);
            }

            player.moveSpeedMult = moveSpeedMult;
            player.fallMultiplierBoost = fallMultiplierBoost;
            player.terminalVelocityMult = terminalVelocityMult;
            player.jumpHeightMult = jumpHeightMult;
            player.wallJumpMult = wallJumpMult;
            player.dashSpeedMult = dashSpeedMult;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player)
        {
            player.moveSpeedMult = 1;
            player.fallMultiplierBoost = 1;
            player.terminalVelocityMult = 1;
            player.jumpHeightMult = 1;
            player.wallJumpMult = 1;
            player.dashSpeedMult = 1;
        }
    }
}
