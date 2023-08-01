using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    [Range(1, 15)]
    public int mapHeight;

    [Range(1, 15)]
    public int mapWidth;

    [Range(1, 10)]
    public int startRadius;

    [Range(1, 384)]
    public int numberOfBoids;

    [RangeWithStep(0.5f, 1, 0.1f)]
    public float boidScale;

    public GameObject boidPrefab;
    public Color boidColor;

    [HideInInspector]
    public List<Boid> boids;

    [Header("Boid Behavior Flags")]
    public bool separationEnabled;
    public bool alignmentEnabled;
    public bool cohesionEnabled;

    [Header("Boid Behavior Range")]
    [RangeWithStep(0, 4, 0.5f)]
    public float separationRange;

    [RangeWithStep(0, 4, 0.5f)]
    public float alignmentRange;

    [RangeWithStep(0, 4, 0.5f)]
    public float cohesionRange;

    [Header("Boid Behavior Weights")]
    [RangeWithStep(0, 1, 0.1f)]
    public float separationFactor;

    [RangeWithStep(0, 1, 0.1f)]
    public float alignmentFactor;

    [RangeWithStep(0, 1, 0.1f)]
    public float cohesionFactor;

    [Header("Boid Speed Settings")]
    [RangeWithStep(0, 10, 0.5f)]
    public float minSpeed;

    [RangeWithStep(0, 10, 0.5f)]
    public float maxSpeed;

    [Range(0, 10)]
    public int rotationSpeed;

    BoidSettings CreateBoidSettings()
    {
        BoidSettings boidSettings = new BoidSettings
        {
            // simulation settings
            mapHeight = mapHeight,
            mapWidth = mapWidth,
            boidColor = boidColor,
            // flags for each phase
            separationEnabled = separationEnabled,
            alignmentEnabled = alignmentEnabled,
            cohesionEnabled = cohesionEnabled,
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

        foreach (Boid b in boids)
        {
            b.UpdateBoidV2(boids);
        }
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
