using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    private GameObject proj;

    public ObjectPool<GameObject> GetProjectilePool(GameObject proj)
    {
        this.proj = proj;
        return new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject);
    }

    GameObject CreatePooledItem()
    {
        return Instantiate(proj);
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(GameObject proj)
    {
        proj.gameObject.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(GameObject proj)
    {
        proj.gameObject.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(GameObject proj)
    {
        Destroy(proj.gameObject);
    }
}
