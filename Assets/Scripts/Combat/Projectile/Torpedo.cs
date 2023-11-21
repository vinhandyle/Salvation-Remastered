using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : Projectile
{
    [SerializeField] private float activationTime;
    [SerializeField] private float deactivationTime;
    private float timer;

    private int state = 0;

    protected override void Update()
    {
        base.Update();

        timer += Time.deltaTime;

        switch (state)
        {
            case 0:
                if (timer >= activationTime)
                {
                    homing = true;
                    timer = 0;
                    state++;
                }
                break;

            case 1:
                if (deactivationTime > 0 && timer >= deactivationTime)
                {
                    homing = false;
                    state++;
                }
                break;
        }
    }

    protected override void PointToTarget(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = AdjustAngle(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        float curAngle = AdjustAngle(transform.eulerAngles.z);

        transform.eulerAngles = Vector3.forward * MinorArcCenter(curAngle, angle);       
    }

    private float AdjustAngle(float a)
    {
        if (a < 0)  
            a += 360;
        else if (a >= 360) 
            a -= 360;
        return a;
    }

    private float MinorArcCenter(float a1, float a2)
    {
        if (a1 - a2 > 180)
            a2 += 360;
        else if (a2 - a1 > 180)
            a1 += 360;

        return (a1 + a2) / 2;
    }
}
