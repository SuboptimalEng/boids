using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Range(1, 4)]
    public int radius = 1;

    [Range(1, 12)]
    public int numberOfObjects;

    public GameObject boidPrefab;
    public List<Boid> boids;

    void Start()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject gameObject = Instantiate(boidPrefab, Vector3.zero, Quaternion.identity);

            float angle = i * Mathf.PI * 2 / numberOfObjects;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 position = transform.position + new Vector3(x, 0, z);

            float angleDegrees = -angle * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angleDegrees, 0);

            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;

            Boid b = gameObject.GetComponent<Boid>();
            b.Initialize(position, rotation);

            boids.Add(gameObject.GetComponent<Boid>());
        }
    }

    void Update()
    {
        foreach (Boid b in boids)
        {
            b.avoidOtherBoids(boids);
        }
    }
}
