using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneViewCameraController : MonoBehaviour
{
    [RangeWithStep(0, 0.5f, 0.05f)]
    public float dragSpeed;

    [RangeWithStep(1, 5, 1)]
    public float rotationSpeed;

    [RangeWithStep(1, 5, 1)]
    public float zoomSpeed;

    [RangeWithStep(1, 10, 1)]
    public float moveSpeed;

    bool isGrabbing = false;
    Vector3 initialMousePosition;

    void Update()
    {
        // note: uncomment these to enable camera movement in the game scene

        // mouse left-click drag
        // HandleCameraDrag();

        // mouse right-click rotation
        // HandleCameraRotation();

        // scroll wheel zoom
        // HandleCameraZoom();

        // WASD keys movement
        // HandleCameraMovement();
    }

    void HandleCameraDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isGrabbing = true;
            initialMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isGrabbing)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 difference =
                (initialMousePosition - currentMousePosition).normalized * dragSpeed;

            transform.Translate(difference);
            initialMousePosition = currentMousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isGrabbing = false;
        }
    }

    Vector3 GetGroundIntersectionPoint()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }
    }

    void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * scroll * zoomSpeed, Space.Self);
    }

    void HandleCameraMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0f, vertical);
        moveDir.Normalize();
        if (moveDir != Vector3.zero)
        {
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
