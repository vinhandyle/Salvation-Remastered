using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// An object that the player can "Interact" with.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [SerializeField] protected PlayerController player = null;
    [SerializeField] protected bool playerInRange = false;

    [Header("Pop-up Text")]
    [SerializeField] private GameObject text;
    [SerializeField] protected int intTextInd;
    [SerializeField] private bool usePopUp;
    protected List<string> intTexts = new List<string>();

    protected event Action<PlayerController> OnInteract;
    protected event Action<Collider2D> OnEnterRange;
    protected event Action<Collider2D> OnExitRange;

    protected virtual void Awake()
    {
        text.SetActive(false);

        OnEnterRange += (collision) =>
        {
            text.SetActive(usePopUp);
            player = collision.GetComponentInParent<PlayerController>();
            playerInRange = true;
        };

        OnExitRange += (collision) =>
        {
            text.SetActive(false);
            playerInRange = false;
        };
    }

    protected virtual void Update()
    {
        if (intTexts.Count > intTextInd)
            text.GetComponent<TextMeshProUGUI>().text = intTexts[intTextInd];

        if (Input.GetKeyDown(KeyCode.W) && playerInRange)
        {
            OnInteract?.Invoke(player);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnEnterRange?.Invoke(collision);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnExitRange?.Invoke(collision);
        }
    }
}
