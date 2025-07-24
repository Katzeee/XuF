using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Xuf.UI;
using Xuf.Core;

[CustomEditor(typeof(CUIEventEmitter))]
public class UIEventEmitterEditor : Editor
{
    private SerializedProperty eventConfigsProp;
    private bool showEventConfigs = true;

    private Type[] argTypes;
    private string[] argTypeNames;

    private const float LABEL_WIDTH = 200f;
    private const float TYPE_WIDTH = 150f;

    private void OnEnable()
    {
        eventConfigsProp = serializedObject.FindProperty("eventConfigs");

        var baseType = typeof(CEventArgBase);
        argTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType)
            .ToArray();
        // Add a 'None' option at the start
        argTypeNames = new string[argTypes.Length + 1];
        argTypeNames[0] = "None";
        for (int i = 0; i < argTypes.Length; i++)
            argTypeNames[i + 1] = argTypes[i].Name;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Custom foldout for event configs list
        showEventConfigs = EditorGUILayout.Foldout(showEventConfigs, "Event Configs", true);

        if (showEventConfigs)
        {
            EditorGUI.indentLevel++;

            // Display list size with a field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size", GUILayout.Width(60));

            int newSize = EditorGUILayout.IntField(eventConfigsProp.arraySize);
            if (newSize != eventConfigsProp.arraySize)
            {
                // Adjust array size
                while (eventConfigsProp.arraySize < newSize)
                {
                    eventConfigsProp.InsertArrayElementAtIndex(eventConfigsProp.arraySize);
                    // Initialize new element's eventArg with null
                    SerializedProperty newConfigProp = eventConfigsProp.GetArrayElementAtIndex(eventConfigsProp.arraySize - 1);
                    SerializedProperty newEventArgProp = newConfigProp.FindPropertyRelative("eventArg");
                    newEventArgProp.managedReferenceValue = null;
                }
                while (eventConfigsProp.arraySize > newSize)
                {
                    eventConfigsProp.DeleteArrayElementAtIndex(eventConfigsProp.arraySize - 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            // Handle each event config in the list
            for (int i = 0; i < eventConfigsProp.arraySize; i++)
            {
                SerializedProperty configProp = eventConfigsProp.GetArrayElementAtIndex(i);
                SerializedProperty eventTypeProp = configProp.FindPropertyRelative("eventType");
                SerializedProperty eventIdProp = configProp.FindPropertyRelative("eventId");
                SerializedProperty eventArgProp = configProp.FindPropertyRelative("eventArg");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Element header with remove button
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Event Config {i}", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    eventConfigsProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Save original label width
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = LABEL_WIDTH;

                // Event properties - aligned
                EditorGUILayout.PropertyField(eventTypeProp);
                EditorGUILayout.PropertyField(eventIdProp);

                // Consistent spacing before Event Arg line
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                // --- Event Arg foldout ---
                eventArgProp.isExpanded = EditorGUILayout.Foldout(eventArgProp.isExpanded, "Event Arg", true);
                if (eventArgProp.isExpanded)
                {
                    // Increase indent level for Event Arg content
                    EditorGUI.indentLevel++;

                    // --- Type dropdown with proper label ---
                    int currentIndex = 0; // 0 means None
                    if (eventArgProp.managedReferenceValue != null)
                    {
                        var currentType = eventArgProp.managedReferenceValue.GetType();
                        int found = Array.FindIndex(argTypes, t => t == currentType);
                        if (found >= 0) currentIndex = found + 1;
                    }

                    int newIndex = EditorGUILayout.Popup("Type", currentIndex, argTypeNames);
                    if (newIndex != currentIndex)
                    {
                        if (newIndex == 0)
                            eventArgProp.managedReferenceValue = null;
                        else
                            eventArgProp.managedReferenceValue = Activator.CreateInstance(argTypes[newIndex - 1]);
                    }

                    // --- Value field with proper indentation ---
                    if (eventArgProp.managedReferenceValue != null)
                    {
                        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                        EditorGUILayout.PropertyField(eventArgProp, new GUIContent("Value"), true);
                    }

                    // Restore indent level
                    EditorGUI.indentLevel--;
                }
                // Restore original label width
                EditorGUIUtility.labelWidth = originalLabelWidth;
                EditorGUILayout.EndVertical();
            }
            // Add button for new event config
            if (GUILayout.Button("Add Event Config"))
            {
                int index = eventConfigsProp.arraySize;
                eventConfigsProp.InsertArrayElementAtIndex(index);
                // Initialize the new element's eventArg with null
                SerializedProperty newConfigProp = eventConfigsProp.GetArrayElementAtIndex(index);
                SerializedProperty newEventArgProp = newConfigProp.FindPropertyRelative("eventArg");
                newEventArgProp.managedReferenceValue = null;
            }
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
