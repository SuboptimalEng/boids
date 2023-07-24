using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Range(1, 4)]
    public int movementSpeed;

    [Range(1, 8)]
    public int rotationSpeed;

    [Range(1, 4)]
    public int lookAheadDist;

    [Range(8, 32)]
    public int fovPrecision;
    float maxNumOfDegrees;
    float fovDegreeIncrement;

    public LayerMask hittableLayerMask;

    bool enableDebugLines;
    int fovDirectionIndex;
    List<Vector3> fovDirections;

    void Initialize()
    {
        enableDebugLines = true;
        fovDirectionIndex = 0;
        maxNumOfDegrees = 360f;
        fovDirections = new List<Vector3>();

        // e.g. precision -> 10, degree increment -> 36
        // e.g. precision -> 12, degree increment -> 30
        // e.g. precision -> 36, degree increment -> 10
        fovDegreeIncrement = maxNumOfDegrees / fovPrecision;

        for (float degrees = 0; degrees < maxNumOfDegrees; degrees += fovDegreeIncrement)
        {
            Vector3 fovDirection = new Vector3(
                Mathf.Cos(degrees * Mathf.Deg2Rad),
                0,
                Mathf.Sin(degrees * Mathf.Deg2Rad)
            );
            fovDirections.Add(fovDirection);
        }
    }

    void Move()
    {
        fovDirectionIndex = GetFovDirectionIndex();

        // note: decided to use Vector3.Lerp instead of manually setting the position
        // transform.position +=
        //     fovDirections[fovDirectionIndex] * movementSpeed * Time.deltaTime * 0.75f;

        Vector3 targetPosition = transform.position + fovDirections[fovDirectionIndex];
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );

        Quaternion targetRotation = Quaternion.LookRotation(fovDirections[fovDirectionIndex]);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    int GetFovDirectionIndex()
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

        if (closestHitIndex < 0 || closestHitDist > lookAheadDist / 2f)
        {
            return fovDirectionIndex;
        }

        Vector3 fovDirAboutToHit = fovDirections[closestHitIndex];
        Vector3 moveAwayDirection = -1 * fovDirAboutToHit;
        Vector3 bisectVector = Vector3.Lerp(
            moveAwayDirection,
            fovDirections[fovDirectionIndex],
            0.5f
        );

        int newFovDirectionIndex = fovDirectionIndex;
        for (int i = 0; i < fovDirections.Count; i++)
        {
            Vector3 fovDirection = fovDirections[i];
            if (Vector3.Angle(fovDirection, bisectVector) < fovDegreeIncrement)
            {
                newFovDirectionIndex = i;
            }
        }

        return newFovDirectionIndex;
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

        DrawDebugLines();
    }

    public void ToggleDebugLines()
    {
        enableDebugLines = !enableDebugLines;
    }

    void DrawDebugLines()
    {
        // todo: draw debug lines only when user clicks on a boid
        if (!enableDebugLines)
        {
            return;
        }

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

            bool isCurrFovDir = i == fovDirectionIndex;

            if (raycastHitObstacle)
            {
                Debug.DrawRay(startPosition, fovDirection * hit.distance, Color.red);
            }
            else if (isCurrFovDir)
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
