using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public GameObject boidPrefab;

    void Start()
    {
        Instantiate(boidPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }
}
