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
    public float separationRange;
    public float separationFactor;
    public float neighborDist;
    public float visualRange;

    // misc settings
    public float boidScale;
    public float minSpeed;
    public float maxSpeed;
}

// todos
// follow game object with camera
// add visualization for view dist, avoid dist, and neighbor dist
// add controls that allow immediate change in game mechanics
// randomize colors of each boid
// update boid model

public class Boid : MonoBehaviour
{
    BoidSettings boidSettings;

    public Vector3 forward;
    public Vector3 velocity;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, boidSettings.separationRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, boidSettings.neighborDist);
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
        Vector3 separationVelocity = Separate(boids);
        // Vector3 matchOtherBoidsVelocity = MatchOtherBoids(boids);

        velocity += separationVelocity;
        // velocity += matchOtherBoidsVelocity;

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

            float dist = Vector3.Distance(currentBoidPosition, otherBoidPosition);
            float squareDist = distToOtherBoid.sqrMagnitude;

            Debug.Log("dist: " + dist);
            Debug.Log("otherboid - currboid: " + distToOtherBoid);
            Debug.Log("square dist: " + squareDist);
            if (squareDist < boidSettings.neighborDist)
            {
                avgDeltaVector += otherBoid.velocity;
                neighborsCount++;
            }

            if (neighborsCount > 0)
            {
                avgDeltaVector /= neighborsCount;
            }
        }

        matchOtherBoidsVelocity = (avgDeltaVector - this.velocity) * 0.5f;

        return matchOtherBoidsVelocity;
    }

    public Vector3 Separate(List<Boid> boids)
    {
        int numberOfBoidsToAvoid = 0;
        Vector3 separationVelocity = Vector3.zero;
        Vector3 currBoidPosition = transform.position;

        foreach (Boid otherBoid in boids)
        {
            if (ReferenceEquals(gameObject, otherBoid.gameObject))
            {
                continue;
            }

            Vector3 otherBoidPosition = otherBoid.transform.position;

            // get the distance from the currBoid to the otherBoid
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
                numberOfBoidsToAvoid++;
            }
        }

        if (numberOfBoidsToAvoid > 0)
        {
            // scale velocity down based on number of boids in the separationRange
            separationVelocity /= numberOfBoidsToAvoid;
        }

        // tune the velocity based on the customizable separationFactor
        separationVelocity *= boidSettings.separationFactor;

        return separationVelocity;
    }

    void ClampBoidSpeed()
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

        if (transform.position.x > boidSettings.mapSize)
        {
            transform.position = new Vector3(-boidSettings.mapSize, 0, transform.position.z);
        }

        if (transform.position.x < -boidSettings.mapSize)
        {
            transform.position = new Vector3(boidSettings.mapSize, 0, transform.position.z);
        }

        if (transform.position.z > boidSettings.mapSize)
        {
            transform.position = new Vector3(transform.position.x, 0, -boidSettings.mapSize);
        }

        if (transform.position.z < -boidSettings.mapSize)
        {
            transform.position = new Vector3(transform.position.x, 0, boidSettings.mapSize);
        }
    }
}
