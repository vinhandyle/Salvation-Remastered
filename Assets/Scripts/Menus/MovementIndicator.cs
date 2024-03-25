using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementIndicator : MonoBehaviour
{
    [SerializeField] private List<Image> images;

    private PlayerController player;

    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        images[1].gameObject.SetActive(PlayerData.Instance.canDash);
        images[2].gameObject.SetActive(PlayerData.Instance.canDoubleJump);

        SetState(images[0], player.onGround || player.sliding);
        SetState(images[1], player.canDash);
        SetState(images[2], player.canJumpMidair);
    }

    private void SetState(Image img, bool active)
    {
        if (active)
            img.color = Color.green;
        else
            img.color = Color.red;
    }
}
