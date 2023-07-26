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
    public float cohesionRange;
    public float cohesionFactor;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, boidSettings.separationRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, boidSettings.alignmentRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, boidSettings.cohesionRange);
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
        if (ReferenceEquals(transform.gameObject, boids[0].gameObject))
        {
            Debug.DrawCircle(transform.position, 1f, 10, Color.black);
        }

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
    }

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
