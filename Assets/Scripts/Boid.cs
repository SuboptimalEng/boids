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

    [Range(1, 8)]
    public int rotationSpeed;

    public LayerMask hittableLayerMask;

    public static float maxNumOfDegrees = 360f;

    int prevFovDirIndex;
    int currFovDirIndex;

    List<Vector3> fovDirections;

    void Initialize()
    {
        prevFovDirIndex = 0;
        currFovDirIndex = 0;
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

    void Update()
    {
        Move();

        CheckAndUpdateDirection();

        DrawDebugLines();
    }

    void Move()
    {
        transform.position += fovDirections[currFovDirIndex] * speed * Time.deltaTime * 0.75f;

        Quaternion desiredRotation = Quaternion.LookRotation(fovDirections[currFovDirIndex]);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
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
            fovDirections[currFovDirIndex],
            0.5f
        );

        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];
            if (Vector3.Angle(fovDirection, bisectVector) < 12.5f)
            {
                currFovDirIndex = i;
            }
        }
    }

    void DrawDebugLines()
    {
        Vector3 startPosition = transform.position;
        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];

            RaycastHit hit;
            bool raycastHitObstacle = Physics.Raycast(
                startPosition,
                fovDirection,
                out hit,
                lookAheadDist,
                hittableLayerMask
            );

            bool currFovDir = i == currFovDirIndex;

            if (raycastHitObstacle)
            {
                Debug.DrawRay(startPosition, fovDirection * hit.distance, Color.red);
            }
            else if (currFovDir)
            {
                Debug.DrawRay(startPosition, fovDirection * lookAheadDist, Color.blue);
            }
            else
            {
                Debug.DrawRay(startPosition, fovDirection * lookAheadDist, Color.green);
            }
        }
    }
}
