using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Xuf.Common
{

    public class SerializableDictionary { }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        SerializableDictionary,
        ISerializationCallbackReceiver,
        IDictionary<TKey, TValue>
    {
        [SerializeField] private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();

        [Serializable]
        private struct SerializableKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        private Dictionary<TKey, int> KeyPositions => _keyPositions.Value;
        private Lazy<Dictionary<TKey, int>> _keyPositions;

        public SerializableDictionary()
        {
            _keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
        }

        private Dictionary<TKey, int> MakeKeyPositions()
        {
            var dictionary = new Dictionary<TKey, int>(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                dictionary[list[i].Key] = i;
            }
            return dictionary;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
        }

        #region IDictionary<TKey, TValue>

        public TValue this[TKey key]
        {
            get => list[KeyPositions[key]].Value;
            set
            {
                var pair = new SerializableKeyValuePair(key, value);
                if (KeyPositions.ContainsKey(key))
                {
                    list[KeyPositions[key]] = pair;
                }
                else
                {
                    KeyPositions[key] = list.Count;
                    list.Add(pair);
                }
            }
        }

        public ICollection<TKey> Keys => list.Select(tuple => tuple.Key).ToArray();
        public ICollection<TValue> Values => list.Select(tuple => tuple.Value).ToArray();

        public void Add(TKey key, TValue value)
        {
            if (KeyPositions.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            else
            {
                KeyPositions[key] = list.Count;
                list.Add(new SerializableKeyValuePair(key, value));
            }
        }

        public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (KeyPositions.TryGetValue(key, out var index))
            {
                KeyPositions.Remove(key);

                list.RemoveAt(index);
                for (var i = index; i < list.Count; i++)
                    KeyPositions[list[i].Key] = i;

                return true;
            }
            else
                return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (KeyPositions.TryGetValue(key, out var index))
            {
                value = list[index].Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        #endregion

        #region ICollection <KeyValuePair<TKey, TValue>>

        public int Count => list.Count;
        public bool IsReadOnly => false;

        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        public void Clear() => list.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var numKeys = list.Count;
            if (array.Length - arrayIndex < numKeys)
                throw new ArgumentException("arrayIndex");
            for (var i = 0; i < numKeys; i++, arrayIndex++)
            {
                var entry = list[i];
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

        #endregion

        #region IEnumerable <KeyValuePair<TKey, TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return list.Select(ToKeyValuePair).GetEnumerator();

            static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
            {
                return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Conversion to Dictionary

        /// <summary>
        /// Converts this SerializableDictionary to a standard Dictionary.
        /// </summary>
        /// <returns>A new Dictionary with the same key-value pairs.</returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(list.Count);
            foreach (var kvp in list)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }

        /// <summary>
        /// Implicit conversion operator from SerializableDictionary to Dictionary.
        /// </summary>
        public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serializableDict)
        {
            return serializableDict?.ToDictionary() ?? new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Explicit conversion operator from Dictionary to SerializableDictionary.
        /// </summary>
        public static explicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            SerializableDictionary<TKey, TValue> serializableDict = new SerializableDictionary<TKey, TValue>();
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    serializableDict[kvp.Key] = kvp.Value;
                }
            }
            return serializableDict;
        }

        #endregion
    }

    [CustomPropertyDrawer(typeof(SerializableDictionary), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private SerializedProperty listProperty;

        private SerializedProperty getListProperty(SerializedProperty property) =>
            listProperty ??= property.FindPropertyRelative("list");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the list property
            SerializedProperty listProp = getListProperty(property);

            // Calculate rects
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Draw foldout header
            listProp.isExpanded = EditorGUI.Foldout(foldoutRect, listProp.isExpanded, label, true);

            if (listProp.isExpanded)
            {
                // Indent child elements
                EditorGUI.indentLevel++;

                // Calculate rects for Add and Validate buttons
                Rect buttonRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight,
                                           position.width - 60f, EditorGUIUtility.singleLineHeight);
                Rect validateRect = new Rect(position.x + position.width - 60f,
                                        position.y + EditorGUIUtility.singleLineHeight,
                                        25f, EditorGUIUtility.singleLineHeight);
                Rect addRect = new Rect(position.x + position.width - 30f,
                                        position.y + EditorGUIUtility.singleLineHeight,
                                        25f, EditorGUIUtility.singleLineHeight);

                // Display size info
                EditorGUI.LabelField(buttonRect, $"Size: {listProp.arraySize}");

                // Validate button
                if (GUI.Button(validateRect, "✓"))
                {
                    ValidateDictionary(listProp);
                }

                // Add button
                if (GUI.Button(addRect, "+"))
                {
                    listProp.arraySize++;

                    // Set the newly added element to be expanded by default
                    if (listProp.arraySize > 0)
                    {
                        SerializedProperty newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                        newElement.isExpanded = true;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }

                // Draw elements with individual delete buttons
                float elementY = position.y + (EditorGUIUtility.singleLineHeight * 2f);
                try
                {
                    for (int i = 0; i < listProp.arraySize; i++)
                    {
                        SerializedProperty elementProp = listProp.GetArrayElementAtIndex(i);
                        float elementHeight = EditorGUI.GetPropertyHeight(elementProp, true);

                        // Calculate rects for element and its remove button
                        Rect elementRect = new Rect(position.x, elementY, position.width - 30f, elementHeight);
                        Rect removeButtonRect = new Rect(position.x + position.width - 30f, elementY, 25f, EditorGUIUtility.singleLineHeight);

                        // Display element with index in label
                        EditorGUI.PropertyField(elementRect, elementProp, new GUIContent($"Element {i}"), true);

                        // Individual remove button for each element
                        if (GUI.Button(removeButtonRect, "×"))
                        {
                            // Record undo and directly delete the array element at this index
                            property.serializedObject.Update();

                            // Use Unity's built-in method to delete array element
                            listProp.DeleteArrayElementAtIndex(i);

                            // Apply the changes
                            property.serializedObject.ApplyModifiedProperties();

                            // Break to avoid issues with modified collection during iteration
                            break;
                        }

                        elementY += elementHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                catch (ExitGUIException)
                {
                    // Just rethrow ExitGUIException as this is expected when opening object pickers
                    throw;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProp = getListProperty(property);

            if (!listProp.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            // Base height for header and buttons
            float height = EditorGUIUtility.singleLineHeight * 2f;

            // Add height for each element
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elementProp = listProp.GetArrayElementAtIndex(i);
                height += EditorGUI.GetPropertyHeight(elementProp, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        private void ValidateDictionary(SerializedProperty listProperty)
        {
            // Dictionary to track keys we've seen
            Dictionary<object, List<int>> keyIndices = new Dictionary<object, List<int>>();
            bool hasDuplicates = false;

            // Iterate through all elements
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = element.FindPropertyRelative("Key");


                object keyValue = GetPropertyValue(keyProp);


                if (keyValue != null)
                {
                    // Track this key and its index
                    if (!keyIndices.ContainsKey(keyValue))
                    {
                        keyIndices[keyValue] = new List<int> { i };
                    }
                    else
                    {
                        // Found a duplicate!
                        keyIndices[keyValue].Add(i);
                        hasDuplicates = true;
                    }
                }
            }

            if (hasDuplicates)
            {
                // Report duplicates
                string message = "Duplicate keys found:\n";
                foreach (var kvp in keyIndices)
                {
                    if (kvp.Value.Count > 1)
                    {
                        message += $"Key '{kvp.Key}' at indices: {string.Join(", ", kvp.Value)}\n";
                    }
                }
                EditorUtility.DisplayDialog("Duplicate Keys", message, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Dictionary Validation", "No duplicate keys found.", "OK");
            }
        }

        private object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                // Add other property types as needed
                default:
                    // For complex types, use the instance ID as a fallback
                    // This is not perfect but helps detect duplicates in many cases
                    return property.propertyPath.GetHashCode();
            }
        }
    }
}
