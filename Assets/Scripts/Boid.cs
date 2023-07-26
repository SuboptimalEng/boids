using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct BoidSettings
{
    // simulation settings
    public int mapSize;
    public int rotationSpeed;

    // boid behavior settings
    public float separationRange;
    public float separationFactor;
    public float alignmentRange;
    public float alignmentFactor;

    // misc settings
    public float boidScale;
    public float minSpeed;
    public float maxSpeed;
}

// todos
// follow game object with camera
// add visualization for view dist, avoid dist, and neighbor dist
// add herding with controllable boid with custom boid settings (high avoidFactor)
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
        Gizmos.DrawWireSphere(transform.position, boidSettings.alignmentRange);
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
        // this vector represents the cumulative direction the
        // currBoid should take if it wants to avoid otherBoids
        // inside of its separationRange
        Vector3 separationVelocity = Separate(boids);

        // this vector represents the cumulative direction of
        // the flock within the currBoid's alignmentRange
        Vector3 alignmentVelocity = Align(boids);

        velocity += separationVelocity;
        velocity += alignmentVelocity;

        ClampBoidVelocity();
        UpdatePosition();
    }

    public Vector3 Separate(List<Boid> boids)
    {
        float numOfBoidsToAvoid = 0;
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
                numOfBoidsToAvoid++;
            }
        }

        if (numOfBoidsToAvoid > 0)
        {
            // scale velocity down based on number of boids in the separationRange
            separationVelocity /= numOfBoidsToAvoid;
        }

        // tune the velocity based on the customizable separationFactor
        separationVelocity *= boidSettings.separationFactor;

        return separationVelocity;
    }

    Vector3 Align(List<Boid> boids)
    {
        float numOfBoidsToAlignWith = 0;
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

        if (numOfBoidsToAlignWith > 0)
        {
            // average the alignmentVelocity of all boids in the alignmentRange to
            // get a vector that represents the average direction of the flock
            alignmentVelocity /= numOfBoidsToAlignWith;
        }

        // align the boid based on a customized alignmentFactor variable
        // high alignmentFactor means the the boid's alignment vector
        // will heavily influence the cumulative direction of the boid
        alignmentVelocity *= boidSettings.alignmentFactor;
        return alignmentVelocity;
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

        // outside of the right boundary
        if (transform.position.x > boidSettings.mapSize)
        {
            transform.position = new Vector3(-boidSettings.mapSize, 0, transform.position.z);
        }

        // outside of the left boundary
        if (transform.position.x < -boidSettings.mapSize)
        {
            transform.position = new Vector3(boidSettings.mapSize, 0, transform.position.z);
        }

        // outside of the top boundary
        if (transform.position.z > boidSettings.mapSize)
        {
            transform.position = new Vector3(transform.position.x, 0, -boidSettings.mapSize);
        }

        // outside of the bottom boundary
        if (transform.position.z < -boidSettings.mapSize)
        {
            transform.position = new Vector3(transform.position.x, 0, boidSettings.mapSize);
        }
    }
}
