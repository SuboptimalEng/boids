using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Range(1, 4)]
    public int speed;

    [Range(1, 4)]
    public int lookAheadDist;

    [Range(1, 32)]
    public int fovPrecision;

    public LayerMask hittableLayerMask;

    public static float maxNumOfDegrees = 360f;

    int fovDirectionIndex;
    List<Vector3> fovDirections;

    void Initialize()
    {
        fovDirectionIndex = 0;
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

    void SetDirection() { }

    void Move()
    {
        transform.position += fovDirections[fovDirectionIndex] * speed * Time.deltaTime * 0.75f;
        transform.LookAt(transform.position + fovDirections[fovDirectionIndex] * speed);
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

        RaycastHit closestHit;
        int closestHitIndex = -1;
        float closestHitDist = Mathf.Infinity;

        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];
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
                if (hit.distance < closestHitDist)
                {
                    closestHit = hit;
                    closestHitIndex = i;
                    closestHitDist = hit.distance;
                }
            }
        }

        if (closestHitIndex < 0 || closestHitDist > 0.5)
        {
            return;
        }

        Vector3 fovDirAboutToHit = fovDirections[closestHitIndex];
        Vector3 moveAwayDirection = -1 * fovDirAboutToHit;
        Vector3 bisectVector = Vector3.Lerp(
            moveAwayDirection,
            fovDirections[fovDirectionIndex],
            0.5f
        );

        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];
            if (Vector3.Angle(fovDirection, bisectVector) < 22.5f)
            {
                fovDirectionIndex = i;
            }
        }

        // note: slowly adjust directions
        // int leftIndex = closestHitIndex;
        // int rightIndex = closestHitIndex;
        // while (true)
        // {
        //     RaycastHit hit;
        //     bool leftHit = Physics.Raycast(
        //         startPosition,
        //         fovDirections[leftIndex],
        //         out hit,
        //         lookAheadDist,
        //         hittableLayerMask
        //     );
        //     bool rightHit = Physics.Raycast(
        //         startPosition,
        //         fovDirections[rightIndex],
        //         out hit,
        //         lookAheadDist,
        //         hittableLayerMask
        //     );
        //     // if both ray casts don't cause hit
        //     // choose a random one between them
        //     // if (!leftHit && !rightHit)
        //     // {
        //     //     int rand = Random.Range(0, 2);
        //     //     if (rand == 0)
        //     //     {
        //     //         fovDirectionIndex = leftIndex;
        //     //     }
        //     //     else
        //     //     {
        //     //         fovDirectionIndex = rightIndex;
        //     //     }
        //     //     break;
        //     // }
        //     if (!leftHit)
        //     {
        //         fovDirectionIndex = leftIndex;
        //         break;
        //     }
        //     if (!rightHit)
        //     {
        //         fovDirectionIndex = rightIndex;
        //         break;
        //     }
        //     int strength = Mathf.RoundToInt(fovDirections.Count / (1 / hit.distance));
        //     leftIndex = (leftIndex + strength) % fovDirections.Count;
        //     rightIndex = (rightIndex - strength) % fovDirections.Count;
        //     if (rightIndex < 0)
        //     {
        //         rightIndex += fovDirections.Count;
        //     }
        // }
    }

    void Update()
    {
        Move();

        CheckAndUpdateDirection();

        DrawDebugLines();
    }
}
