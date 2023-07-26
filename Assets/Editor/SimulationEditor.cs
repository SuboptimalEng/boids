using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Simulation))]
public class SimulationEditor : Editor
{
    Simulation simulation;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();

            if (check.changed)
            {
                simulation.UpdateBoidSettings();
            }
        }
    }

    void OnEnable()
    {
        simulation = target as Simulation;
    }
}
