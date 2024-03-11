using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkingObject : MonoBehaviour
{
    [SerializeField] private float shrinkSpeed;

    private void Update()
    {
        if (transform.localScale.x > 0)
            transform.localScale -= shrinkSpeed * Time.deltaTime * Vector3.one;
        else
            Destroy(gameObject);
    }
}
