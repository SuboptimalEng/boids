using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    float maxX = 6;
    float maxZ = 6;
    float minX = -6;
    float minZ = -6;
    float minSpeed = 1f;
    float maxSpeed = 2.5f;

    float visualRange = 2;
    float turnFactor = 1.5f;
    float avoidFactor = 0.1f;
    float protectedRange = 1.5f;
    Vector3 velocity = Vector3.forward;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward;
    }

    public void Initialize(Vector3 position, Quaternion rotation)
    {
        Vector3 forwardDirection = rotation * Vector3.forward;
        rb.velocity = forwardDirection * maxSpeed;
    }

    public void avoidOtherBoids(List<Boid> boids)
    {
        float closeDx = 0;
        float closeDz = 0;
        Vector3 currentBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;

            float dx = currentBoidPosition.x - otherBoidPosition.x;
            float dz = currentBoidPosition.z - otherBoidPosition.z;

            if (Mathf.Abs(dx) < visualRange && Mathf.Abs(dz) < visualRange)
            {
                float squareDist = dx * dx + dz * dz;

                if (squareDist < protectedRange)
                {
                    closeDx += currentBoidPosition.x - otherBoidPosition.x;
                    closeDz += currentBoidPosition.z - otherBoidPosition.z;
                }
            }
        }

        // rb.velocity = Vector3.forward;
        velocity.x += closeDx * avoidFactor;
        velocity.z += closeDz * avoidFactor;

        Rigidbody rb = transform.GetComponent<Rigidbody>();

        // outside top
        if (currentBoidPosition.z > maxZ)
        {
            velocity.z = velocity.z - turnFactor;
        }
        // outside right
        if (currentBoidPosition.x > maxX)
        {
            velocity.x = velocity.x - turnFactor;
        }
        // outside left
        if (currentBoidPosition.x < minX)
        {
            velocity.x = velocity.x + turnFactor;
        }
        // outside bottom
        if (currentBoidPosition.z < minZ)
        {
            velocity.z = velocity.z + turnFactor;
        }

        float speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
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
