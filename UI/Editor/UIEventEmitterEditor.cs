using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Xuf.UI;

[CustomEditor(typeof(CUIEventEmitter))]
public class UIEventEmitterEditor : Editor
{
    private SerializedProperty eventConfigsProp;
    private bool showEventConfigs = true;

    private Type[] argTypes;
    private string[] argTypeNames;

    private const float LABEL_WIDTH = 120f;
    private const float TYPE_WIDTH = 150f;

    private void OnEnable()
    {
        eventConfigsProp = serializedObject.FindProperty("eventConfigs");

        var baseType = typeof(EventArgBase);
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
                EditorGUILayout.BeginHorizontal();
                // Type dropdown
                EditorGUILayout.LabelField("Event Arg");
                int currentIndex = 0; // 0 means None
                if (eventArgProp.managedReferenceValue != null)
                {
                    var currentType = eventArgProp.managedReferenceValue.GetType();
                    int found = Array.FindIndex(argTypes, t => t == currentType);
                    if (found >= 0) currentIndex = found + 1;
                }
                int newIndex = EditorGUILayout.Popup(currentIndex, argTypeNames, GUILayout.Width(TYPE_WIDTH));
                // If type changed
                if (newIndex != currentIndex)
                {
                    if (newIndex == 0)
                        eventArgProp.managedReferenceValue = null;
                    else
                        eventArgProp.managedReferenceValue = Activator.CreateInstance(argTypes[newIndex - 1]);
                }
                // Value field on the same line (if not null)
                if (eventArgProp.managedReferenceValue != null && eventArgProp.hasVisibleChildren)
                {
                    var iterator = eventArgProp.Copy();
                    var end = iterator.GetEndProperty();
                    bool enterChildren = true;
                    while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
                    {
                        if (iterator.name == "value")
                        {
                            EditorGUILayout.PropertyField(iterator, GUIContent.none, true);
                            break;
                        }
                        enterChildren = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
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
