using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Range(1, 20)]
    public int speed;

    [Range(1, 5)]
    public int lookAheadDist;

    [Range(1, 40)]
    public int fovPrecision;

    public LayerMask hittableLayerMask;

    public static float maxNumOfDegrees = 360f;

    Vector3 moveDirection;
    List<Vector3> fovDirections;

    void Initialize()
    {
        moveDirection = Vector3.forward;
        fovDirections = new List<Vector3>();

        float fovDegreeIncrement = maxNumOfDegrees / fovPrecision;
        for (float degrees = 0; degrees < maxNumOfDegrees; degrees += fovDegreeIncrement)
        {
            float fovRadian = degrees * Mathf.Deg2Rad;
            Vector3 fovDirection = new Vector3(
                (float)Mathf.Cos(fovRadian),
                0,
                (float)Mathf.Sin(fovRadian)
            );
            fovDirections.Add(fovDirection);
        }
    }

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        Initialize();
    }

    void SetDirection(Vector3 newDirection)
    {
        moveDirection = newDirection;
    }

    void Move()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void Update()
    {
        Move();

        Vector3 startPosition = transform.position;

        int startAngleCollisionIndex = -1;
        int endAngleCollisionIndex = -1;
        int closestAngleCollisionIndex = -1;

        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];
            Vector3 endPosition = startPosition + fovDirection * lookAheadDist;

            Debug.DrawLine(startPosition, endPosition, Color.green);

            RaycastHit hit;
            if (
                Physics.Raycast(
                    startPosition,
                    fovDirection,
                    out hit,
                    lookAheadDist,
                    hittableLayerMask
                )
            )
            {
                Debug.DrawRay(startPosition, fovDirection * hit.distance, Color.red);
                Debug.Log("Did Hit");

                if (startAngleCollisionIndex == -1)
                {
                    startAngleCollisionIndex = i;
                }
                endAngleCollisionIndex = i;

                float angle = Vector3.Angle(moveDirection, fovDirection);
                if (angle < 1f)
                {
                    closestAngleCollisionIndex = i;
                }
            }

            if (closestAngleCollisionIndex != -1)
            {
                int distFromStart = closestAngleCollisionIndex - startAngleCollisionIndex;
                int distFromEnd = endAngleCollisionIndex - closestAngleCollisionIndex;
                Vector3 newDirection;
                if (distFromEnd > distFromStart)
                {
                    int fovDirectionIndex = (startAngleCollisionIndex - 1) % fovDirections.Count;
                    newDirection = fovDirections[fovDirectionIndex];
                }
                else if (distFromEnd < distFromStart)
                {
                    int fovDirectionIndex = (endAngleCollisionIndex + 1) % fovDirections.Count;
                    newDirection = fovDirections[fovDirectionIndex];
                }
                else
                {
                    int fovDirectionIndex = (endAngleCollisionIndex - 1) % fovDirections.Count;
                    newDirection = fovDirections[fovDirectionIndex];
                }
                SetDirection(newDirection);
            }
        }

        Debug.Log(startAngleCollisionIndex + "->" + endAngleCollisionIndex);
    }
}
