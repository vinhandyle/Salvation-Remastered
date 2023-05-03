using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : InteractableObject
{
    [SerializeField] private List<TerminalMenu> menus;

    protected override void Awake()
    {
        base.Awake();

        intTexts.Add("[OPEN TERMINAL]");

        OnExitRange += (player) =>
        {
            foreach (TerminalMenu menu in menus)
                menu.gameObject.SetActive(false);
        };

        OnInteract += (player) =>
        {
            menus[0].gameObject.SetActive(!menus[0].gameObject.activeSelf);
            menus[1].gameObject.SetActive(false);
        };
    }
}
