using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    [RangeWithStep(5, 15, 0.5f)]
    public float mapHeight;

    [RangeWithStep(5, 15, 0.5f)]
    public float mapWidth;

    [Range(1, 10)]
    public int startRadius;

    [Range(1, 4000)]
    public int numberOfBoids;

    [RangeWithStep(0.1f, 1.0f, 0.1f)]
    public float boidScale;

    public GameObject boidPrefab;
    public Color boidColor;

    public enum SimulationType
    {
        v1Unoptimized,
        v2Optimized,
        v3Compute,
    }

    public SimulationType simulationType;

    [HideInInspector]
    public List<Boid> boids;

    [Header("Boid Behavior Range")]
    [RangeWithStep(0, 3, 0.5f)]
    public float separationRange;

    [RangeWithStep(0, 3, 0.5f)]
    public float alignmentRange;

    [RangeWithStep(0, 3, 0.5f)]
    public float cohesionRange;

    [Header("Boid Behavior Weights")]
    [RangeWithStep(0, 1, 0.1f)]
    public float separationFactor;

    [RangeWithStep(0, 1, 0.1f)]
    public float alignmentFactor;

    [RangeWithStep(0, 0.2f, 0.025f)]
    public float cohesionFactor;

    // todo: maybe normalize values
    // normalized values look less cool in the simulation
    // [RangeWithStep(0, 0.5f, 0.05f)]
    // public float separationFactor;
    // [RangeWithStep(0, 0.5f, 0.05f)]
    // public float alignmentFactor;
    // [RangeWithStep(0, 0.5f, 0.05f)]
    // public float cohesionFactor;

    [Header("Boid Speed Settings")]
    [RangeWithStep(0, 5, 0.25f)]
    public float minSpeed;

    [RangeWithStep(0, 5, 0.25f)]
    public float maxSpeed;

    [Range(0, 10)]
    public int rotationSpeed;

    public ComputeShader compute;

    BoidSettings CreateBoidSettings()
    {
        BoidSettings boidSettings = new BoidSettings
        {
            // simulation settings
            mapHeight = mapHeight,
            mapWidth = mapWidth,
            boidColor = boidColor,
            // boid behavior range
            separationRange = separationRange,
            alignmentRange = alignmentRange,
            cohesionRange = cohesionRange,
            // boid behavior weights
            separationFactor = separationFactor,
            alignmentFactor = alignmentFactor,
            cohesionFactor = cohesionFactor,
            // misc settings
            boidScale = boidScale,
            minSpeed = minSpeed,
            maxSpeed = maxSpeed,
            rotationSpeed = rotationSpeed,
        };
        return boidSettings;
    }

    public void CreateBoids()
    {
        GameObject liveSimulation = new GameObject("Live Simulation");

        boids = new List<Boid>();

        for (int i = 0; i < numberOfBoids; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfBoids;
            float x = Mathf.Cos(angle) * startRadius;
            float z = Mathf.Sin(angle) * startRadius;
            Vector3 position = transform.position + new Vector3(x, 0, z);

            float angleDegrees = -angle * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angleDegrees, 0);

            GameObject gameObject = Instantiate(boidPrefab, position, rotation);
            gameObject.transform.SetParent(liveSimulation.transform);

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

    public void UpdateBoidSettings()
    {
        BoidSettings updatedBoidSettings = CreateBoidSettings();
        foreach (Boid b in boids)
        {
            // boid will be undefined when the game is not running
            // adding this check prevents a console error
            if (b == null)
            {
                continue;
            }

            b.boidSettings = updatedBoidSettings;
            b.UpdateLocalScale();
            b.UpdateBoidColor();
        }
    }

    void Update()
    {
        CheckForUserInput();
        if (simulationType == SimulationType.v1Unoptimized)
        {
            RunBoidUpdateV1();
        }
        else if (simulationType == SimulationType.v2Optimized)
        {
            RunBoidUpdateV2();
        }
        else if (simulationType == SimulationType.v3Compute)
        {
            RunBoidUpdateV3();
        }
    }

    void RunBoidUpdateV1()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].UpdateBoidV1(boids);
        }
    }

    void RunBoidUpdateV2()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].UpdateBoidV2(boids);
        }
    }

    // void RunBoidUpdateV3Code()
    // {
    //     BoidCompute[] boidData = new BoidCompute[numberOfBoids];
    //     for (int i = 0; i < boids.Count; i++)
    //     {
    //         boidData[i] = boids[i].GetComputeShaderData();
    //     }
    //     ComputeBuffer boidBuffer = new ComputeBuffer(numberOfBoids, BoidCompute.Size);
    //     boidBuffer.SetData(boidData);
    //     compute.SetBuffer(0, "boids", boidBuffer);
    //     compute.SetInt("numberOfBoids", boids.Count);
    //     compute.SetFloat("separationRange", separationRange);
    //     compute.SetFloat("alignmentRange", alignmentRange);
    //     compute.SetFloat("cohesionRange", cohesionRange);
    //     compute.SetFloat("separationFactor", separationFactor);
    //     compute.SetFloat("alignmentFactor", alignmentFactor);
    //     compute.SetFloat("cohesionFactor", cohesionFactor);
    //     compute.Dispatch(0, 128, 1, 1);
    //     boidBuffer.GetData(boidData);
    //     for (int i = 0; i < boids.Count; i++)
    //     {
    //         boids[i].UpdateBoidV3(
    //             boidData[i].separationVelocity,
    //             boidData[i].alignmentVelocity,
    //             boidData[i].cohesionVelocity
    //         );
    //     }
    //     boidBuffer.Release();
    // }

    void RunBoidUpdateV3()
    {
        // note: set up compute shader data
        BoidCompute[] boidData = new BoidCompute[numberOfBoids];
        for (int i = 0; i < boids.Count; i++)
        {
            boidData[i] = boids[i].GetComputeShaderData();
        }
        ComputeBuffer boidBuffer = new ComputeBuffer(numberOfBoids, BoidCompute.Size);
        boidBuffer.SetData(boidData);
        compute.SetBuffer(0, "boids", boidBuffer);
        compute.SetInt("numberOfBoids", boids.Count);
        compute.SetFloat("separationRange", separationRange);
        compute.SetFloat("alignmentRange", alignmentRange);
        compute.SetFloat("cohesionRange", cohesionRange);
        compute.SetFloat("separationFactor", separationFactor);
        compute.SetFloat("alignmentFactor", alignmentFactor);
        compute.SetFloat("cohesionFactor", cohesionFactor);

        // note: run compute shader
        // todo: change this?
        // int threadGroupSize = 1024;
        // int threadGroups = Mathf.CeilToInt(numberOfBoids / (float)threadGroupSize);
        // compute.Dispatch(0, threadGroups, 1, 1);
        // compute.Dispatch(0, numberOfBoids, 1, 1);
        compute.Dispatch(0, 128, 1, 1);
        boidBuffer.GetData(boidData);

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].UpdateBoidV3(
                boidData[i].separationVelocity,
                boidData[i].alignmentVelocity,
                boidData[i].cohesionVelocity
            );
        }

        boidBuffer.Release();
    }

    void CheckForUserInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject boidGameObject = hit.collider.gameObject;
                Boid b = boidGameObject.GetComponent<Boid>();
                if (b != null)
                {
                    b.ToggleDebugView();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisableAllBoidDebugViews();
        }
    }

    void DisableAllBoidDebugViews()
    {
        foreach (Boid b in boids)
        {
            b.DisableDebugView();
        }
    }
}
