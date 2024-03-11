using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTrap : TriggerZone
{
    [SerializeField] private MovingObject mo;    
    [SerializeField] private float activationSpeed;
    [SerializeField] private float returnSpeed;
    [SerializeField] private Vector3 endPos;
    private float timeToTrigger;
    private float duration;
    private float timer;
    private bool active;

    [Header("Trap Activation Indicator")]
    [SerializeField] private GameObject warning;
    [SerializeField] private float warningLength;

    public void SetDefaults(float timeToTrigger, float duration)
    {
        this.timeToTrigger = timeToTrigger;
        this.duration = duration;
    }

    private void Update()
    {
        if (mo.stopped)
        {
            if (active && !playerInRange)
            {
                timer -= Time.deltaTime;

                if (timer <= 0)
                {
                    active = false;
                    timer = 0;
                    mo.Return(returnSpeed);
                }
            }
            else if (!active)
            {
                if (playerInRange)
                {
                    timer += Time.deltaTime;

                    if (timer >= timeToTrigger)
                    {
                        StartCoroutine(TriggerTrap());
                    }
                }
                else if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer < 0) timer = 0;
                }
            }
        }
    }

    protected virtual IEnumerator TriggerTrap()
    {
        if (warning != null)
        {
            warning.SetActive(true);
            yield return new WaitForSeconds(warningLength);
            warning.SetActive(false);
        }

        active = true;
        timer = duration;
        mo.Move(activationSpeed, endPos);
    }    
}
