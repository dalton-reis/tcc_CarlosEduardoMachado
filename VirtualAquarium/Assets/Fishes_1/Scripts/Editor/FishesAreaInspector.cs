using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(FishArea))]
public class FishesAreaInspector : Editor
{
    public override void OnInspectorGUI()
    {
        FishArea area = (FishArea)target;
        area.Count = EditorGUILayout.IntField("Count", area.Count);
        SerializedProperty prop = serializedObject.FindProperty("prefabs");
        SerializedProperty speed = serializedObject.FindProperty("speed");
        SerializedProperty rotationSpeed = serializedObject.FindProperty("rotationSpeed");

        SerializedProperty raycastDistance = serializedObject.FindProperty("raycastDistance");
        SerializedProperty particleFood = serializedObject.FindProperty("particleFood");
        SerializedProperty feedPoint = serializedObject.FindProperty("feedPoint");
        SerializedProperty eggFish = serializedObject.FindProperty("eggFish");
        SerializedProperty bounds = serializedObject.FindProperty("bounds");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prop, true);
        EditorGUILayout.PropertyField(speed, true);
        EditorGUILayout.PropertyField(rotationSpeed, true);
        EditorGUILayout.PropertyField(particleFood, true);
        EditorGUILayout.PropertyField(feedPoint, true);
        EditorGUILayout.PropertyField(eggFish, true);
        EditorGUILayout.PropertyField(bounds, true);

        EditorGUILayout.PropertyField(raycastDistance, true);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(target, "Changed field");
        }
        EditorUtility.SetDirty(target);
        if (GUILayout.Button("Spawn"))
            area.SpawnFishes();
        if (GUILayout.Button("Remove All"))
            area.RemoveFishes();

        if (!Application.isPlaying)
            EditorSceneManager.MarkAllScenesDirty();
    }
    
}