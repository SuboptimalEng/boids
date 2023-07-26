using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct BoidSettings
{
    // simulation settings
    public int mapSize;
    public int rotationSpeed;

    // visual settings
    public float seperationRange;
    public float alignmentRange;
    public float visualRange;
    public float turnFactor;
    public float avoidFactor;

    // misc settings
    public float boidScale;
    public float minSpeed;
    public float maxSpeed;
}

public class Boid : MonoBehaviour
{
    BoidSettings boidSettings;

    public Vector3 forward;
    public Vector3 velocity;

    bool isOutOfBounds = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, boidSettings.seperationRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, boidSettings.alignmentRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, boidSettings.visualRange);
    }

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
        Vector3 matchOtherBoidsVelocity = MatchOtherBoids(boids);

        velocity += avoidOtherBoidsVelocity;
        velocity += matchOtherBoidsVelocity;

        AvoidMapBoundary();
        ClampBoidSpeed();
        UpdatePosition();
    }

    public Vector3 MatchOtherBoids(List<Boid> boids)
    {
        Vector3 matchOtherBoidsVelocity = Vector3.zero;
        Vector3 avgDeltaVector = Vector3.zero;
        int neighborsCount = 0;
        Vector3 currentBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;

            Vector3 distToOtherBoid = otherBoidPosition - currentBoidPosition;
            // if (
            //     Mathf.Abs(distToOtherBoid.x) < boidSettings.visualRange
            //     && Mathf.Abs(distToOtherBoid.y) < boidSettings.visualRange
            // )
            // {
            float squareDist = distToOtherBoid.sqrMagnitude;
            // float squareDist = distToOtherBoid.magnitude;
            if (squareDist < boidSettings.alignmentRange)
            {
                avgDeltaVector += otherBoid.velocity;
                neighborsCount++;
            }
            // }

            if (neighborsCount > 0)
            {
                avgDeltaVector /= neighborsCount;
            }
        }

        matchOtherBoidsVelocity = (avgDeltaVector - this.velocity) * 0.5f;

        return matchOtherBoidsVelocity;
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
            // if (
            //     Mathf.Abs(distToOtherBoid.x) < boidSettings.visualRange
            //     && Mathf.Abs(distToOtherBoid.y) < boidSettings.visualRange
            // )
            // {
            float squareDist = distToOtherBoid.sqrMagnitude;
            // float squareDist = distToOtherBoid.magnitude;
            if (squareDist < boidSettings.seperationRange)
            {
                moveAwayDelta += currentBoidPosition - otherBoidPosition;
            }
            // }
        }

        // velocity += moveAwayDelta * boidSettings.avoidFactor;
        avoidOtherBoidsVelocity = moveAwayDelta * boidSettings.avoidFactor;
        return avoidOtherBoidsVelocity;
    }

    void AvoidMapBoundary()
    {
        isOutOfBounds = false;
        Vector3 position = transform.position;

        // outside top
        if (position.z > boidSettings.mapSize)
        {
            velocity.z -= boidSettings.turnFactor;
            isOutOfBounds = true;
        }

        // outside right
        if (position.x > boidSettings.mapSize)
        {
            velocity.x -= boidSettings.turnFactor;
            isOutOfBounds = true;
        }

        // outside left
        if (position.x < -boidSettings.mapSize)
        {
            velocity.x += boidSettings.turnFactor;
            isOutOfBounds = true;
        }

        // outside bottom
        if (position.z < -boidSettings.mapSize)
        {
            velocity.z += boidSettings.turnFactor;
            isOutOfBounds = true;
        }
    }

    void ClampBoidSpeed()
    {
        // Note: Do not clamp the speed of the boid if it is out of
        // bounds. This ensures that the boid smoothly returns back
        // onto the map.
        if (isOutOfBounds)
        {
            return;
        }

        Vector3 position = transform.position;
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
