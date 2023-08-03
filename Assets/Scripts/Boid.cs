using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct BoidSettings
{
    // simulation settings
    public float mapHeight;
    public float mapWidth;
    public Color boidColor;

    // boid behavior range
    public float separationRange;
    public float alignmentRange;
    public float cohesionRange;

    // boid behavior weights
    public float separationFactor;
    public float alignmentFactor;
    public float cohesionFactor;

    // misc settings
    public float boidScale;
    public float minSpeed;
    public float maxSpeed;
    public int rotationSpeed;
}

public struct BoidCompute
{
    public Vector3 velocity;
    public Vector3 position;
    public Vector3 separationVelocity;
    public Vector3 alignmentVelocity;
    public Vector3 cohesionVelocity;

    public static int Size
    {
        get { return sizeof(float) * 3 * 5; }
    }
}

public class Boid : MonoBehaviour
{
    public BoidSettings boidSettings;
    public Vector3 velocity;
    bool debugViewEnabled;

    public void Initialize(Vector3 position, Quaternion rotation, BoidSettings boidSettings)
    {
        this.boidSettings = boidSettings;
        this.velocity = rotation * Vector3.forward * this.boidSettings.maxSpeed;
        this.debugViewEnabled = false;
        this.UpdateLocalScale();
        this.UpdateBoidColor();
    }

    public BoidCompute GetComputeShaderData()
    {
        return new BoidCompute
        {
            velocity = this.velocity,
            position = transform.position,
            separationVelocity = Vector3.zero,
            alignmentVelocity = Vector3.zero,
            cohesionVelocity = Vector3.zero,
        };
    }

    public void UpdateLocalScale()
    {
        transform.localScale = new Vector3(
            this.boidSettings.boidScale,
            this.boidSettings.boidScale,
            this.boidSettings.boidScale
        );
    }

    float RandomRangeWithStep(float min, float max, float step)
    {
        float steps = Mathf.Floor((max - min) / step);
        int randomStepIndex = Random.Range(0, Mathf.FloorToInt(steps) + 1);
        float randomValue = min + randomStepIndex * step;
        return Mathf.Clamp(randomValue, min, max);
    }

    Color GetRandomizedColor()
    {
        float colorMultiplier = RandomRangeWithStep(0.7f, 0.9f, 0.05f);
        Color randomizedColor = boidSettings.boidColor * colorMultiplier;
        return randomizedColor;
    }

    public void UpdateBoidColor()
    {
        Color randomizedColor = GetRandomizedColor();
        Transform child = gameObject.transform.GetChild(0);
        Material material = child.GetComponent<Renderer>().material;
        material.SetColor("_BoidColor", randomizedColor);
        material.color = randomizedColor;
    }

    void UpdateRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            boidSettings.rotationSpeed * Time.deltaTime
        );
    }

    // public void UpdateBoidV0(List<Boid> boids)
    // {
    //     Vector3 separationVelocity = Separation(boids);
    //     Vector3 alignmentVelocity = Alignment(boids);
    //     Vector3 cohesionVelocity = Cohesion(boids);

    //     velocity += separationVelocity;
    //     velocity += alignmentVelocity;
    //     velocity += cohesionVelocity;

    //     ClampBoidVelocity();
    //     UpdatePosition();
    //     UpdateRotation();
    //     DrawDebugView();
    // }

    public void UpdateBoidV1(List<Boid> boids)
    {
        // this vector represents the cumulative direction the
        // currBoid should take if it wants to avoid otherBoids
        // inside of its separationRange
        Vector3 separationVelocity = Separation(boids);

        // this vector represents the cumulative direction of
        // the flock within the currBoid's alignmentRange
        Vector3 alignmentVelocity = Alignment(boids);

        // this vector represents the cumulative direction the
        // currBoid needs to travel if it wants to be at the
        // center of its flock mates
        Vector3 cohesionVelocity = Cohesion(boids);

        velocity += separationVelocity;
        velocity += alignmentVelocity;
        velocity += cohesionVelocity;

        ClampBoidVelocity();
        UpdatePosition();
        UpdateRotation();
        DrawDebugView();
    }

    public (Vector3, Vector3, Vector3) PerformThreeActions(List<Boid> boids)
    {
        Vector3 separationVelocity = Vector3.zero;
        Vector3 alignmentVelocity = Vector3.zero;
        Vector3 cohesionVelocity = Vector3.zero;

        int numOfBoidsToAvoid = 0;
        int numOfBoidsToAlignWith = 0;
        int numOfBoidsInFlock = 0;
        Vector3 currBoidPosition = transform.position;
        // cohesion variable
        Vector3 positionToMoveTowards = Vector3.zero;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;
            float dist = Vector3.Distance(currBoidPosition, otherBoidPosition);

            // separation check
            if (dist < boidSettings.separationRange)
            {
                Vector3 otherBoidToCurrBoid = currBoidPosition - otherBoidPosition;
                Vector3 dirToTravel = otherBoidToCurrBoid.normalized;
                dirToTravel /= dist;
                separationVelocity += dirToTravel;
                numOfBoidsToAvoid++;
            }

            // alignment check
            if (dist < boidSettings.alignmentRange)
            {
                alignmentVelocity += otherBoid.velocity;
                numOfBoidsToAlignWith++;
            }

            // cohesion check
            if (dist < boidSettings.cohesionRange)
            {
                // keep track of the positions of all otherBoids that are in
                // the currBoid's cohesionRange
                positionToMoveTowards += otherBoidPosition;
                numOfBoidsInFlock++;
            }
        }

        if (numOfBoidsToAvoid != 0)
        {
            separationVelocity /= (float)numOfBoidsToAvoid;
            separationVelocity *= boidSettings.separationFactor;
        }

        if (numOfBoidsToAlignWith != 0)
        {
            alignmentVelocity /= (float)numOfBoidsToAlignWith;
            alignmentVelocity *= boidSettings.alignmentFactor;
        }

        if (numOfBoidsInFlock != 0)
        {
            positionToMoveTowards /= (float)numOfBoidsInFlock;
            Vector3 cohesionDirection = positionToMoveTowards - currBoidPosition;
            cohesionDirection.Normalize();
            cohesionVelocity = cohesionDirection * boidSettings.cohesionFactor;
        }

        return (separationVelocity, alignmentVelocity, cohesionVelocity);
    }

    public void UpdateBoidV2(List<Boid> boids)
    {
        (Vector3 separationVelocity, Vector3 alignmentVelocity, Vector3 cohesionVelocity) =
            PerformThreeActions(boids);

        velocity += separationVelocity;
        velocity += alignmentVelocity;
        velocity += cohesionVelocity;

        ClampBoidVelocity();
        UpdatePosition();
        UpdateRotation();
        DrawDebugView();
    }

    public void UpdateBoidV3(
        Vector3 separationVelocity,
        Vector3 alignmentVelocity,
        Vector3 cohesionVelocity
    )
    {
        velocity += separationVelocity;
        velocity += alignmentVelocity;
        velocity += cohesionVelocity;

        ClampBoidVelocity();
        UpdatePosition();
        UpdateRotation();
        DrawDebugView();
    }

    public void ToggleDebugView()
    {
        debugViewEnabled = !debugViewEnabled;
    }

    public void DisableDebugView()
    {
        debugViewEnabled = false;
    }

    public void DrawDebugView()
    {
        if (!debugViewEnabled)
        {
            return;
        }

        Debug.DrawCircle(transform.position, boidSettings.separationRange, 24, Color.red);
        Debug.DrawCircle(transform.position, boidSettings.alignmentRange, 24, Color.green);
        Debug.DrawCircle(transform.position, boidSettings.cohesionRange, 24, Color.cyan);
    }

    // public Vector3 SeparationV0(List<Boid> boids)
    // {
    //     int numOfBoidsToAvoid = 0;
    //     Vector3 separationVelocity = Vector3.zero;
    //     Vector3 currBoidPosition = transform.position;

    //     foreach (Boid otherBoid in boids)
    //     {
    //         if (ReferenceEquals(gameObject, otherBoid.gameObject))
    //         {
    //             continue;
    //         }
    //         Vector3 otherBoidPosition = otherBoid.transform.position;
    //         float dist = Vector3.Distance(currBoidPosition, otherBoidPosition);
    //         if (dist < boidSettings.separationRange)
    //         {
    //             Vector3 otherBoidToCurrBoid = currBoidPosition - otherBoidPosition;
    //             Vector3 dirToTravel = otherBoidToCurrBoid.normalized;
    //             Vector3 weightedVelocity = dirToTravel / dist;
    //             separationVelocity += weightedVelocity;
    //             numOfBoidsToAvoid++;
    //         }
    //     }

    //     if (numOfBoidsToAvoid > 0)
    //     {
    //         separationVelocity /= (float)numOfBoidsToAvoid;
    //         separationVelocity *= boidSettings.separationFactor;
    //     }

    //     return separationVelocity;
    // }

    public Vector3 Separation(List<Boid> boids)
    {
        int numOfBoidsToAvoid = 0;
        Vector3 separationVelocity = Vector3.zero;
        Vector3 currBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;
            float dist = Vector3.Distance(currBoidPosition, otherBoidPosition);

            // check if the otherBoid is running too close to the currBoid
            if (dist < boidSettings.separationRange)
            {
                // this vector represents the direction the currBoid needs
                // to travel in order to avoid collision with the otherBoid
                Vector3 otherBoidToCurrBoid = currBoidPosition - otherBoidPosition;
                Vector3 dirToTravel = otherBoidToCurrBoid.normalized;

                // scale the direction based on how far the the other boid is
                // if one boid is closer than the other, then the closer boid
                // will be given more weight
                dirToTravel /= dist;

                // accumulate separation velocity
                separationVelocity += dirToTravel;

                // keep track of how many boids are in the separationRange
                numOfBoidsToAvoid++;
            }
        }

        if (numOfBoidsToAvoid == 0)
        {
            return Vector3.zero;
        }

        // scale velocity down based on number of boids in the separationRange
        separationVelocity /= (float)numOfBoidsToAvoid;

        // tune the velocity based on the customizable separationFactor
        separationVelocity *= boidSettings.separationFactor;

        return separationVelocity;
    }

    Vector3 Alignment(List<Boid> boids)
    {
        int numOfBoidsToAlignWith = 0;
        Vector3 alignmentVelocity = Vector3.zero;
        Vector3 currBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;
            float dist = Vector3.Distance(currBoidPosition, otherBoidPosition);

            // check if the otherBoid is within alignmentRange of the currBoid
            if (dist < boidSettings.alignmentRange)
            {
                // increment the alignmentVelocity based on the otherBoid's velocity
                alignmentVelocity += otherBoid.velocity;
                numOfBoidsToAlignWith++;
            }
        }

        if (numOfBoidsToAlignWith == 0)
        {
            return Vector3.zero;
        }

        // average the alignmentVelocity of all boids in the alignmentRange to
        // get a vector that represents the average direction of the flock
        alignmentVelocity /= (float)numOfBoidsToAlignWith;

        // align the boid based on a customized alignmentFactor variable
        // high alignmentFactor means the the boid's alignment vector
        // will heavily influence the cumulative direction of the boid
        alignmentVelocity *= boidSettings.alignmentFactor;
        return alignmentVelocity;
    }

    Vector3 Cohesion(List<Boid> boids)
    {
        int numOfBoidsInFlock = 0;
        Vector3 positionToMoveTowards = Vector3.zero;
        Vector3 currBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;
            float dist = Vector3.Distance(currBoidPosition, otherBoidPosition);

            if (dist < boidSettings.cohesionRange)
            {
                // keep track of the positions of all otherBoids that are in
                // the currBoid's cohesionRange
                positionToMoveTowards += otherBoidPosition;
                numOfBoidsInFlock++;
            }
        }

        if (numOfBoidsInFlock == 0)
        {
            return Vector3.zero;
        }

        // this represents the position that the currBoid needs to move towards
        // if it wants to be at the average position of its flock mates
        positionToMoveTowards /= (float)numOfBoidsInFlock;

        // cohesionDirection is the vector that points from the currBoid's position
        // to the center of the flock (which we just calculated)
        Vector3 cohesionDirection = positionToMoveTowards - currBoidPosition;

        // we normalize this vector because the distance can vary based on the cohesionRange
        cohesionDirection.Normalize();

        // align the boid based on a customized cohesionFactor variable
        // a high cohesionFactor means the the boid will strongly move
        // towards the center of the flock
        Vector3 cohesionVelocity = cohesionDirection * boidSettings.cohesionFactor;
        return cohesionVelocity;
    }

    void ClampBoidVelocity()
    {
        Vector3 position = transform.position;
        Vector3 direction = velocity.normalized;
        float speed = velocity.magnitude;
        speed = Mathf.Clamp(speed, boidSettings.minSpeed, boidSettings.maxSpeed);
        velocity = direction * speed;
    }

    void UpdatePosition()
    {
        transform.position = transform.position + velocity * Time.deltaTime;

        // outside of the top boundary
        if (transform.position.z > boidSettings.mapHeight)
        {
            transform.position = new Vector3(transform.position.x, 0, -boidSettings.mapHeight);
        }

        // outside of the bottom boundary
        if (transform.position.z < -boidSettings.mapHeight)
        {
            transform.position = new Vector3(transform.position.x, 0, boidSettings.mapHeight);
        }

        // outside of the right boundary
        if (transform.position.x > boidSettings.mapWidth)
        {
            transform.position = new Vector3(-boidSettings.mapWidth, 0, transform.position.z);
        }

        // outside of the left boundary
        if (transform.position.x < -boidSettings.mapWidth)
        {
            transform.position = new Vector3(boidSettings.mapWidth, 0, transform.position.z);
        }
    }
}
