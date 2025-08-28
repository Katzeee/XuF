using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Xuf.Utils
{
    /// <summary>
    /// Generic object inspector utility for recursively displaying all fields and properties of any object in Editor
    /// </summary>
    public static partial class EditorUtils
    {
#if UNITY_EDITOR
        /// <summary>
        /// Display any object with full control over include/exclude filters
        /// </summary>
        /// <param name="obj">Object to display</param>
        /// <param name="include">Only display these property paths (empty means display all)</param>
        /// <param name="exclude">Exclude these property paths</param>
        /// <param name="maxDepth">Maximum recursion depth</param>
        public static void DisplayAnyObject(
            object obj,
            List<string> include = null,
            List<string> exclude = null,
            int maxDepth = 3,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            if (obj == null)
            {
                EditorGUILayout.LabelField("null");
                return;
            }

            DisplayAnyObjectInternal(obj, 0, bindingAttr, include, exclude, "", maxDepth);
        }

        /// <summary>
        /// Internal core function: recursively display all fields and properties of any object
        /// </summary>
        /// <param name="obj">Object to display</param>
        /// <param name="depth">Current recursion depth</param>
        /// <param name="include">Only display these property paths (empty means display all)</param>
        /// <param name="exclude">Exclude these property paths</param>
        /// <param name="currentPath">Current property path</param>
        /// <param name="maxDepth">Maximum recursion depth</param>
        internal static void DisplayAnyObjectInternal(object obj, int depth, BindingFlags bindingAttr, List<string> include, List<string> exclude, string currentPath, int maxDepth)
        {
            if (obj == null || depth > maxDepth) return;

            Type type = obj.GetType();

            // Display all public fields
            foreach (var field in type.GetFields(bindingAttr))
            {
                try
                {
                    if (ShouldSkipType(field.FieldType)) continue;

                    string fieldPath = string.IsNullOrEmpty(currentPath) ? field.Name : $"{currentPath}.{field.Name}";

                    // Check include/exclude filtering
                    if (!ShouldDisplayProperty(fieldPath, include, exclude)) continue;

                    var value = field.GetValue(obj);
                    string name = field.Name;

                    DisplayValue(name, value, bindingAttr, field.FieldType, depth, include, exclude, fieldPath, maxDepth);
                }
                catch (Exception e)
                {
                    EditorGUILayout.LabelField($"{field.Name}: <Error: {e.Message}>");
                }
            }

            // Display all public properties
            foreach (var prop in type.GetProperties(bindingAttr))
            {
                try
                {
                    if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
                    if (ShouldSkipType(prop.PropertyType)) continue;

                    string propPath = string.IsNullOrEmpty(currentPath) ? prop.Name : $"{currentPath}.{prop.Name}";

                    // Check include/exclude filtering
                    if (!ShouldDisplayProperty(propPath, include, exclude)) continue;

                    var value = prop.GetValue(obj);
                    string name = prop.Name;

                    DisplayValue(name, value, bindingAttr, prop.PropertyType, depth, include, exclude, propPath, maxDepth);
                }
                catch (Exception e)
                {
                    EditorGUILayout.LabelField($"{prop.Name}: <Error: {e.Message}>");
                }
            }
        }

        #region Private Methods

        private static void DisplayValue(string name, object value, BindingFlags bindingAttr, Type valueType, int depth, List<string> include, List<string> exclude, string fieldPath, int maxDepth)
        {
            if (value == null)
            {
                EditorGUILayout.LabelField($"{name}: null");
            }
            else if (IsSimpleType(valueType))
            {
                EditorGUILayout.LabelField($"{name}: {FormatValue(value)}");
            }
            else
            {
                EditorGUILayout.LabelField($"{name}: {value.GetType().Name}", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                DisplayAnyObjectInternal(value, depth + 1, bindingAttr, include, exclude, fieldPath, maxDepth);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Determine whether a property should be displayed
        /// </summary>
        private static bool ShouldDisplayProperty(string propertyPath, List<string> include, List<string> exclude)
        {
            // If exclude list exists, check if property is excluded
            if (exclude != null && exclude.Count > 0)
            {
                foreach (string excludePath in exclude)
                {
                    // Exclude if current path matches exactly or is a child of excluded path
                    if (propertyPath.Equals(excludePath, StringComparison.OrdinalIgnoreCase) ||
                        propertyPath.StartsWith(excludePath + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            // If include list exists, check if property should be included
            if (include != null && include.Count > 0)
            {
                foreach (string includePath in include)
                {
                    // Include if current path matches exactly or is a child of included path
                    if (propertyPath.Equals(includePath, StringComparison.OrdinalIgnoreCase) ||
                        propertyPath.StartsWith(includePath + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false; // Include list is not empty but no match found, don't display
            }

            return true; // No include restriction and not excluded, display
        }

        /// <summary>
        /// Determine whether a type should be skipped
        /// </summary>
        private static bool ShouldSkipType(Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type) ||
                   type.Name.Contains("Event") ||
                   type == typeof(System.IntPtr) ||
                   type == typeof(System.UIntPtr);
        }

        /// <summary>
        /// Determine whether a type is simple (can display value directly)
        /// </summary>
        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type.IsEnum ||
                   type == typeof(Vector3) || type == typeof(Vector2) || type == typeof(Quaternion) ||
                   type == typeof(GameObject) || typeof(Component).IsAssignableFrom(type);
        }

        /// <summary>
        /// Format value for display
        /// </summary>
        private static string FormatValue(object value)
        {
            if (value is float f) return f.ToString("F2");
            if (value is GameObject go) return go.name;
            if (value is Component comp) return $"{comp.GetType().Name}({comp.gameObject.name})";
            return value.ToString();
        }

        #endregion
#endif
    }
}