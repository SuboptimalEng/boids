using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    [Range(1, 4)]
    public int radius;

    [Range(1, 24)]
    public int numberOfObjects;

    [Range(1, 8)]
    public int mapSize;

    [Range(0, 1)]
    public float boidScale;

    public GameObject boidPrefab;
    public List<Boid> boids;

    [Header("Boid Behavior Settings")]
    [Range(0, 1)]
    public float seperationRange;

    [Range(1, 2)]
    public float alignmentRange;

    [Range(2, 3)]
    public float visualRange;

    [Range(0, 0.1f)]
    public float turnFactor;

    [Range(0, 0.1f)]
    public float avoidFactor;

    [Header("Boid Speed Settings")]
    [Range(6, 10)]
    public int rotationSpeed;

    [Range(0, 2)]
    public float minSpeed;

    [Range(2, 3)]
    public float maxSpeed;

    BoidSettings CreateBoidSettings()
    {
        BoidSettings boidSettings = new BoidSettings
        {
            // simulation settings
            mapSize = mapSize,
            rotationSpeed = rotationSpeed,
            // visual settings
            seperationRange = seperationRange,
            alignmentRange = alignmentRange,
            visualRange = visualRange,
            turnFactor = turnFactor,
            avoidFactor = avoidFactor,
            // misc settings
            boidScale = boidScale,
            minSpeed = minSpeed,
            maxSpeed = maxSpeed,
        };
        return boidSettings;
    }

    void CreateBoids()
    {
        boids = new List<Boid>();

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
            BoidSettings boidSettings = CreateBoidSettings();
            b.Initialize(position, rotation, boidSettings);

            boids.Add(gameObject.GetComponent<Boid>());
        }
    }

    void Start()
    {
        CreateBoids();
    }

    void Update()
    {
        foreach (Boid b in boids)
        {
            b.UpdateBoid(boids);
        }
    }
}
