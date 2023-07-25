using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    [Range(1, 4)]
    public int radius;

    [Range(1, 12)]
    public int numberOfObjects;

    [Range(1, 5)]
    public int mapSize;

    public GameObject boidPrefab;
    public List<Boid> boids;

    [Header("Boid Behavior Settings")]
    [Range(1, 4)]
    public int visualRange;

    [Range(0.5f, 1.5f)]
    public float turnFactor;

    [Range(0, 1)]
    public float avoidFactor;

    [Range(0, 2)]
    public float protectedRange;

    [Header("Boid Speed Settings")]
    [Range(6, 10)]
    public int rotationSpeed;

    [Range(0, 2)]
    public float minSpeed;

    [Range(2, 3)]
    public float maxSpeed;

    [Header("Boid Misc Settings")]
    [Range(0, 1)]
    public float scale;

    BoidSettings createBoidSettings()
    {
        BoidSettings boidSettings = new BoidSettings
        {
            scale = scale,
            mapSize = mapSize,
            visualRange = visualRange,
            rotationSpeed = rotationSpeed,
            minSpeed = minSpeed,
            maxSpeed = maxSpeed,
            turnFactor = turnFactor,
            avoidFactor = avoidFactor,
            protectedRange = protectedRange,
        };
        return boidSettings;
    }

    void createBoids()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 position = transform.position + new Vector3(x, 0, z);

            float angleDegrees = -angle * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angleDegrees, 0);

            GameObject gameObject = Instantiate(boidPrefab, position, rotation);

            Boid b = gameObject.GetComponent<Boid>();
            BoidSettings boidSettings = createBoidSettings();
            b.Initialize(position, rotation, boidSettings);

            boids.Add(gameObject.GetComponent<Boid>());
        }
    }

    void Start()
    {
        createBoids();
    }

    void Update()
    {
        foreach (Boid b in boids)
        {
            b.avoidOtherBoids(boids);
        }
    }
}
