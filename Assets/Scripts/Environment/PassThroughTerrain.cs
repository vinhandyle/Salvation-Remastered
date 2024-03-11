using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassThroughTerrain : MonoBehaviour
{
    private GameObject player;
    private CircleCollider2D playerCldr;
    private Vector3 playerBottom;

    private BoxCollider2D box;
    private Vector3 boxSurface;

    private void Awake()
    {
        player = GameObject.Find("Player");
        playerCldr = player.GetComponent<CircleCollider2D>();
        box = GetComponent<BoxCollider2D>();

        playerBottom = playerCldr.radius * Vector3.up;
        boxSurface = box.size.y / 2 * Vector3.up + transform.position;
    }

    void Update()
    {
        // Platform is solid only when the bottom of the player
        // is above the top of the platform
        Vector2 playerRelPos = player.transform.position - playerBottom - boxSurface;
        box.enabled = Vector2.Dot(playerRelPos - box.offset * transform.localScale, transform.up) > 0;
    }
}
