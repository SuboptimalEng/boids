using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// public class CameraFollowOnClick : Editor

// code by chatGPT
[InitializeOnLoad]
public class CameraFollowOnClick
{
    static GameObject targetObject;
    static bool isFollowing;

    static CameraFollowOnClick()
    {
        Debug.Log("camera follow on click static constructor");
        SceneView.duringSceneGui += OnSceneGUI;
        // SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if ((e.type == EventType.MouseDown && e.button == 0) || e.keyCode == KeyCode.Space)
        {
            if (Selection.activeGameObject != null)
            {
                isFollowing = !isFollowing;

                if (isFollowing)
                {
                    targetObject = Selection.activeGameObject;
                    Debug.Log("hi there: " + targetObject.gameObject);
                }

                // Stop other Scene view input from being processed while following.
                e.Use();
            }

            // // Old code
            // // Raycast from mouse position to find the GameObject under the click.
            // Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            // RaycastHit hitInfo;
            // if (Physics.Raycast(ray, out hitInfo))
            // {
            //     targetObject = hitInfo.collider.gameObject;
            //     isFollowing = true;
            //     // Stop other Scene view input from being processed while following.
            //     // e.Use();
            // }
        }
    }

    // [MenuItem("Window/Scene View Camera/Follow Selected Object")]
    // static void ToggleFollowObject()
    // {
    //     isFollowing = !isFollowing;
    //     if (isFollowing && Selection.activeGameObject != null)
    //     {
    //         targetObject = Selection.activeGameObject;
    //     }
    // }

    // [MenuItem("Window/Scene View Camera/Follow Selected Object", true)]
    // static bool ValidateToggleFollowObject()
    // {
    //     return Selection.activeGameObject != null;
    // }

    // [MenuItem("Window/Camera/Follow Selected Object")]
    // private static void ShowWindow()
    // {
    //     // Implement the custom functionality of your window here...
    //     CameraFollowOnClick window = EditorWindow.GetWindow<CameraFollowOnClick>();
    //     window.titleContent = new GUIContent("Follow Object");
    //     window.Show();
    // }

    void OnSceneGUI()
    {
        Debug.Log("On scene gui");
        Debug.Log("is following: " + isFollowing);
        Debug.Log("target object: " + targetObject);
        if (isFollowing && targetObject != null)
        {
            // Make the main camera follow the target object;
            // SceneView.lastActiveSceneView.pivot = targetObject.transform.position;
            // SceneView.lastActiveSceneView.pivot = new Vector3(
            //     targetObject.transform.position.x,
            //     SceneView.lastActiveSceneView.pivot.y,
            //     targetObject.transform.position.z
            // );
            // SceneView.lastActiveSceneView.Repaint();
            // SceneView.lastActiveSceneView.pivot = new Vector3(
            //     targetObject.transform.position.x,
            //     SceneView.lastActiveSceneView.pivot.y,
            //     targetObject.transform.position.z
            // );

            // SceneView.currentDrawingSceneView.pivot = targetObject.transform.position;
            // SceneView.currentDrawingSceneView.Repaint();

            Camera c = SceneView.lastActiveSceneView.camera;
            Debug.Log("camera pos: " + c.transform.position);
            Debug.Log("target pos: " + targetObject.transform.position);
            c.transform.position = targetObject.transform.position;

            // SceneView.lastActiveSceneView.pivot = targetObject.transform.position;
            // SceneView.lastActiveSceneView.Repaint();
        }
    }
}
