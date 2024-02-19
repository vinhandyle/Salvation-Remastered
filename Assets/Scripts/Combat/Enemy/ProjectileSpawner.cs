using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private float projSpeed;
    [SerializeField] private float spawnRadius;
    [SerializeField] private float spawnRate;
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            float x = transform.position.x;            
            float y = transform.position.y;
            float a = Mathf.Deg2Rad * transform.eulerAngles.z;
            float d = Random.Range(-spawnRadius, spawnRadius);           
            Vector2 pos = new Vector2(x + d * Mathf.Cos(a), y + d * Mathf.Sin(a));

            Projectile proj = Instantiate(projectile, pos, projectile.transform.rotation);
            proj.SetDefaults(null, 0, projSpeed);
            timer = 0;
        }
    }

    public void SetProjectileSpeed(float projSpeed)
    {
        this.projSpeed = projSpeed;
    }
}
