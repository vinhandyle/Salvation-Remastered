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
    private bool active;

    [SerializeField] private bool flipSpriteX;
    [SerializeField] private bool flipSpriteY;

    [SerializeField] private bool recycleProjectiles;
    private Queue<Projectile> activeProjectiles;
    private Stack<Projectile> inactiveProjectiles;
    private bool isRecycling;

    private void Awake()
    {
        activeProjectiles = new Queue<Projectile>();
        inactiveProjectiles = new Stack<Projectile>();
;    }


    private void Update()
    {
        if (active)
        {
            timer += Time.deltaTime;

            if (timer >= spawnRate)
            {
                Projectile proj;
                float x = transform.position.x;
                float y = transform.position.y;
                float z = transform.position.z;
                float a = Mathf.Deg2Rad * transform.eulerAngles.z;
                float d = Random.Range(-spawnRadius, spawnRadius);
                Vector3 pos = new Vector3(x + d * Mathf.Cos(a), y + d * Mathf.Sin(a), z);

                if (isRecycling && inactiveProjectiles.Count > 0)
                {
                    proj = inactiveProjectiles.Pop();
                    proj.transform.position = pos;
                    proj.gameObject.SetActive(true);
                }
                else
                {
                    // Rotate the projectile so that it is
                    // opposite to the spawner's up vector
                    Quaternion rot = transform.rotation;
                    rot.eulerAngles += new Vector3(0, 0, -90);
                    proj = Instantiate(projectile, pos, rot);

                    proj.GetComponent<SpriteRenderer>().flipX = flipSpriteX;
                    proj.GetComponent<SpriteRenderer>().flipY = flipSpriteY;
                }

                if (recycleProjectiles) activeProjectiles.Enqueue(proj);
                proj.SetDefaults(null, 0, projSpeed);
                timer = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {        
        activeProjectiles.TryPeek(out Projectile proj);

        if (proj != null && collision.gameObject == proj.gameObject)
        {
            proj.gameObject.SetActive(false);
            inactiveProjectiles.Push(activeProjectiles.Dequeue());
            isRecycling = true;
        }
    }    

    public void SetProjectileSpeed(float projSpeed)
    {
        this.projSpeed = projSpeed;
    }

    public void Toggle()
    {
        active = !active;
    }

    public void Clear()
    {
        active = false;

        while (activeProjectiles.Count > 0)
        {
            Destroy(activeProjectiles.Dequeue());
        }
    }
}
