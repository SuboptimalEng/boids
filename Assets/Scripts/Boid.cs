using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Range(1, 20)]
    public int speed;

    [Range(1, 5)]
    public int lookAheadDist;

    [Range(1, 64)]
    public int fovPrecision;

    public LayerMask hittableLayerMask;

    public static float maxNumOfDegrees = 360f;

    int fovDirectionIndex;
    List<Vector3> fovDirections;

    void Initialize()
    {
        // moveDirection = Vector3.forward;
        fovDirections = new List<Vector3>();
        fovDirectionIndex = 0;

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

    void Move()
    {
        transform.position += fovDirections[fovDirectionIndex] * speed * Time.deltaTime * 0.1f;
    }

    void DrawDebugLines()
    {
        Vector3 startPosition = transform.position;
        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];
            Vector3 endPosition = startPosition + fovDirection * lookAheadDist;

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
            }
            else
            {
                Debug.DrawLine(startPosition, endPosition, Color.green);
            }
        }
    }

    void CheckAndUpdateDirection()
    {
        Vector3 startPosition = transform.position;

        RaycastHit hit;
        if (
            Physics.Raycast(
                startPosition,
                fovDirections[fovDirectionIndex],
                out hit,
                lookAheadDist,
                hittableLayerMask
            )
        )
        {
            int leftIndex = fovDirectionIndex;
            int rightIndex = fovDirectionIndex;

            while (true)
            {
                bool leftHit = Physics.Raycast(
                    startPosition,
                    fovDirections[leftIndex],
                    out hit,
                    lookAheadDist,
                    hittableLayerMask
                );
                bool rightHit = Physics.Raycast(
                    startPosition,
                    fovDirections[rightIndex],
                    out hit,
                    lookAheadDist,
                    hittableLayerMask
                );

                // if both ray casts don't cause hit
                // choose a random one between them
                if (!leftHit && !rightHit)
                {
                    int rand = Random.Range(0, 2);
                    if (rand == 0)
                    {
                        fovDirectionIndex = leftIndex;
                    }
                    else
                    {
                        fovDirectionIndex = rightIndex;
                    }
                    break;
                }

                if (!leftHit)
                {
                    fovDirectionIndex = leftIndex;
                    break;
                }
                if (!rightHit)
                {
                    fovDirectionIndex = rightIndex;
                    break;
                }

                leftIndex = (leftIndex + 4) % fovDirections.Count;
                rightIndex = (rightIndex - 4) % fovDirections.Count;

                // unity modulo returns negative numbers!!!
                if (rightIndex < 0)
                {
                    rightIndex += fovDirections.Count;
                }
            }
        }
    }

    void Update()
    {
        Move();

        CheckAndUpdateDirection();

        DrawDebugLines();
    }
}
