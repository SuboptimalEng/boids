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

    void Start()
    {
        moveDirection = Vector3.forward;
    }

    void Update()
    {
        Vector3 startPosition = transform.position;

        float fovDegreeIncrement = maxNumOfDegrees / fovPrecision;

        for (float degrees = 0; degrees < maxNumOfDegrees; degrees += fovDegreeIncrement)
        {
            float fovRadian = degrees * Mathf.Deg2Rad;
            Vector3 fovDirection = new Vector3(
                (float)Mathf.Cos(fovRadian),
                0,
                (float)Mathf.Sin(fovRadian)
            );

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
            }
        }
    }
}
