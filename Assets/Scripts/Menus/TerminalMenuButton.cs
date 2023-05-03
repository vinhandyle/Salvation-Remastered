using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalMenuButton : MonoBehaviour
{
    [SerializeField] private GameObject noHit;
    [SerializeField] private GameObject expertNoHit;
    [SerializeField] private GameObject challenge;
    [SerializeField] private GameObject expertChallenge;
    [SerializeField] private string key;

    private void OnEnable()
    {
        if (noHit)
            noHit.SetActive(
                PlayerData.Instance.noHits.Contains(key));

        if (expertNoHit)
            expertNoHit.SetActive(
                PlayerData.Instance.expertNoHits.Contains(key));

        if (challenge)
            challenge.SetActive(
                PlayerData.Instance.challenges.Contains(key));

        if (expertChallenge)
            expertChallenge.SetActive(
                PlayerData.Instance.expertChallenges.Contains(key));
    }

    public void LoadLevel()
    {
        SceneController.Instance.LoadScene(key);
    }

    public void OpenMenu(TerminalMenu menu)
    {
        menu.gameObject.SetActive(true);
        menu.DisplayInfo(key);
    }

    public void CloseMenu(TerminalMenu menu)
    {
        menu.gameObject.SetActive(false);
    }
}
