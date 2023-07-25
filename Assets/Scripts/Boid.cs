using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    float mapSize;

    float minSpeed = 0.5f;
    float maxSpeed = 2f;
    float visualRange = 2;
    float turnFactor = 1.5f;
    float avoidFactor = 0.1f;
    float protectedRange = 1.5f;
    float rotationSpeed = 8;

    Vector3 forward;
    Vector3 velocity;

    public void Initialize(Vector3 position, Quaternion rotation, int mapSize)
    {
        this.mapSize = mapSize;
        forward = rotation * Vector3.forward;
        velocity = maxSpeed * forward;
    }

    void Update()
    {
        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
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

            if (Mathf.Abs(delta.x) < visualRange && Mathf.Abs(delta.x) < visualRange)
            {
                float squareDist = delta.sqrMagnitude;
                // float squareDist = Mathf.Sqrt(delta.x * delta.x + delta.z * delta.z);
                if (squareDist < protectedRange)
                {
                    // closeDx += currentBoidPosition.x - otherBoidPosition.x;
                    // closeDz += currentBoidPosition.z - otherBoidPosition.z;
                    closeDelta += currentBoidPosition - otherBoidPosition;
                }
            }
        }

        // velocity.x += closeDx * avoidFactor;
        // velocity.z += closeDz * avoidFactor;

        velocity += closeDelta * avoidFactor;

        // outside top
        if (currentBoidPosition.z > mapSize)
        {
            velocity.z = velocity.z - turnFactor;
        }
        // outside right
        if (currentBoidPosition.x > mapSize)
        {
            velocity.x = velocity.x - turnFactor;
        }
        // outside left
        if (currentBoidPosition.x < -mapSize)
        {
            velocity.x = velocity.x + turnFactor;
        }
        // outside bottom
        if (currentBoidPosition.z < -mapSize)
        {
            velocity.z = velocity.z + turnFactor;
        }

        float speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
        // float speed = velocity.sqrMagnitude;
        if (speed < minSpeed)
        {
            velocity = (velocity / speed) * minSpeed;
        }
        if (speed > maxSpeed)
        {
            velocity = (velocity / speed) * maxSpeed;
        }

        transform.position = transform.position + velocity * Time.deltaTime;
    }
}
