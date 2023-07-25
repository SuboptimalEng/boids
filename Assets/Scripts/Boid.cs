using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoidSettings
{
    public int mapSize;
    public int visualRange;
    public int rotationSpeed;

    public float boidScale;
    public float minSpeed;
    public float maxSpeed;
    public float turnFactor;
    public float avoidFactor;
    public float protectedRange;
}

public class Boid : MonoBehaviour
{
    BoidSettings boidSettings;

    public Vector3 forward;
    public Vector3 velocity;

    public void Initialize(Vector3 position, Quaternion rotation, BoidSettings boidSettings)
    {
        this.boidSettings = boidSettings;
        this.forward = rotation * Vector3.forward;
        this.velocity = this.boidSettings.maxSpeed * forward;

        transform.localScale *= this.boidSettings.boidScale;
    }

    void Update()
    {
        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            boidSettings.rotationSpeed * Time.deltaTime
        );
    }

    public void UpdateBoid(List<Boid> boids)
    {
        Vector3 avoidOtherBoidsVelocity = AvoidOtherBoids(boids);

        velocity += avoidOtherBoidsVelocity;

        AvoidMapBoundary();
        ClampVelocityBetweenMinMax();
        UpdatePosition();
    }

    public Vector3 AvoidOtherBoids(List<Boid> boids)
    {
        Vector3 avoidOtherBoidsVelocity = Vector3.zero;
        Vector3 moveAwayDelta = Vector3.zero;
        Vector3 currentBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;

            Vector3 distToOtherBoid = otherBoidPosition - currentBoidPosition;
            if (
                Mathf.Abs(distToOtherBoid.x) < boidSettings.visualRange
                && Mathf.Abs(distToOtherBoid.y) < boidSettings.visualRange
            )
            {
                float squareDist = distToOtherBoid.sqrMagnitude;
                if (squareDist < boidSettings.protectedRange)
                {
                    moveAwayDelta += currentBoidPosition - otherBoidPosition;
                }
            }
        }

        // velocity += moveAwayDelta * boidSettings.avoidFactor;
        avoidOtherBoidsVelocity = moveAwayDelta * boidSettings.avoidFactor;
        return avoidOtherBoidsVelocity;
    }

    void AvoidMapBoundary()
    {
        Vector3 position = transform.position;

        // outside top
        if (position.z > boidSettings.mapSize)
        {
            velocity.z = velocity.z - boidSettings.turnFactor;
        }

        // outside right
        if (position.x > boidSettings.mapSize)
        {
            velocity.x = velocity.x - boidSettings.turnFactor;
        }

        // outside left
        if (position.x < -boidSettings.mapSize)
        {
            velocity.x = velocity.x + boidSettings.turnFactor;
        }

        // outside bottom
        if (position.z < -boidSettings.mapSize)
        {
            velocity.z = velocity.z + boidSettings.turnFactor;
        }
    }

    void ClampVelocityBetweenMinMax()
    {
        Vector3 direction = velocity.normalized;
        float speed = velocity.magnitude;
        speed = Mathf.Clamp(speed, boidSettings.minSpeed, boidSettings.maxSpeed);
        velocity = direction * speed;
    }

    void UpdatePosition()
    {
        transform.position = transform.position + velocity * Time.deltaTime;
    }
}
