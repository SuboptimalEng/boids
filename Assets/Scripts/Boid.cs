using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoidSettings
{
    public int mapSize;
    public int visualRange;
    public int rotationSpeed;

    public float scale;
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

        forward = rotation * Vector3.forward;
        velocity = this.boidSettings.maxSpeed * forward;

        transform.localScale *= this.boidSettings.scale;
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

    public void avoidOtherBoids(List<Boid> boids)
    {
        // float closeDx = 0;
        // float closeDz = 0;
        Vector3 closeDelta = Vector3.zero;
        Vector3 currentBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;

            Vector3 delta = currentBoidPosition - otherBoidPosition;
            // float dx = currentBoidPosition.x - otherBoidPosition.x;
            // float dz = currentBoidPosition.z - otherBoidPosition.z;

            if (
                Mathf.Abs(delta.x) < boidSettings.visualRange
                && Mathf.Abs(delta.x) < boidSettings.visualRange
            )
            {
                float squareDist = delta.sqrMagnitude;
                // float squareDist = Mathf.Sqrt(delta.x * delta.x + delta.z * delta.z);
                if (squareDist < boidSettings.protectedRange)
                {
                    // closeDx += currentBoidPosition.x - otherBoidPosition.x;
                    // closeDz += currentBoidPosition.z - otherBoidPosition.z;
                    closeDelta += currentBoidPosition - otherBoidPosition;
                }
            }
        }

        // velocity.x += closeDx * avoidFactor;
        // velocity.z += closeDz * avoidFactor;

        velocity += closeDelta * boidSettings.avoidFactor;

        // outside top
        if (currentBoidPosition.z > boidSettings.mapSize)
        {
            velocity.z = velocity.z - boidSettings.turnFactor;
        }
        // outside right
        if (currentBoidPosition.x > boidSettings.mapSize)
        {
            velocity.x = velocity.x - boidSettings.turnFactor;
        }
        // outside left
        if (currentBoidPosition.x < -boidSettings.mapSize)
        {
            velocity.x = velocity.x + boidSettings.turnFactor;
        }
        // outside bottom
        if (currentBoidPosition.z < -boidSettings.mapSize)
        {
            velocity.z = velocity.z + boidSettings.turnFactor;
        }

        float speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
        // float speed = velocity.sqrMagnitude;
        if (speed < boidSettings.minSpeed)
        {
            velocity = (velocity / speed) * boidSettings.minSpeed;
        }
        if (speed > boidSettings.maxSpeed)
        {
            velocity = (velocity / speed) * boidSettings.maxSpeed;
        }

        transform.position = transform.position + velocity * Time.deltaTime;
    }
}
