using UnityEngine;
using UnityEditor;
using FriendlyEditor.UtilityAttributes;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace FriendlyEditor.UtilityAttributes
{
    #region HighlightAttribute
    [CustomPropertyDrawer(typeof(HighlightAttribute))] // Agarro el drawer del tipo del atributo
    public class HighlightDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) // le digo como se dibuja
        {
            HighlightAttribute highlight = (HighlightAttribute)attribute; // esto me da acceso a los valores del atributo
            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = highlight.color;

            EditorGUI.PropertyField(position, property, label);

            GUI.backgroundColor = previousColor;
        }
    }
    #endregion
    #region TypePopupAttribute
    [CustomPropertyDrawer(typeof(TypePopupAttribute))]
    public class TypeFilterDrawer : PropertyDrawer
    {
        private List<Type> filteredTypes = new List<Type>();
        private string[] typeNames;
        private int selectedIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Obtener el atributo y su baseType
            TypePopupAttribute typeFilter = (TypePopupAttribute)attribute;
            Type baseType = typeFilter.baseType;

            // Obtener todas las clases que heredan o implementan el tipo base
            if (filteredTypes.Count == 0)
            {
                filteredTypes = GetFilteredTypes(baseType);
                typeNames = filteredTypes.Select(t => t.FullName).ToArray();
            }

            // Manejar strings y listas de strings
            if (property.propertyType == SerializedPropertyType.String)
            {
                // Si es un string, dibuja el popup normal
                DrawStringPopup(position, property, label);
            }
            else if (property.isArray && property.arrayElementType == "string")
            {
                // Si es una lista de strings, dibuja una lista de popups
                DrawStringListPopup(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [TypePopup] with string or List<string>.");
            }
        }

        // Método para dibujar el popup para un solo string
        private void DrawStringPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            // Obtener el valor actual como string
            string currentString = property.stringValue;
            selectedIndex = Array.IndexOf(typeNames, currentString);
            if (selectedIndex == -1) selectedIndex = 0;

            // Dibujar el popup
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, typeNames);

            // Actualizar el valor del string con la opción seleccionada
            property.stringValue = typeNames[selectedIndex];
        }

        // Método para dibujar el popup para una lista de strings
        private void DrawStringListPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label.text);

            // Indentar el contenido de la lista
            EditorGUI.indentLevel++;

            // Dibujar cada elemento de la lista
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                Rect elementRect = new Rect(position.x, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                string currentString = element.stringValue;
                selectedIndex = Array.IndexOf(typeNames, currentString);
                if (selectedIndex == -1) selectedIndex = 0;

                // Dibujar el popup para cada string en la lista
                selectedIndex = EditorGUI.Popup(elementRect, "Element " + i, selectedIndex, typeNames);

                // Actualizar el valor del string con la opción seleccionada
                element.stringValue = typeNames[selectedIndex];
            }

            // Permitir añadir o eliminar elementos de la lista
            Rect addButtonRect = new Rect(position.x, position.y + (property.arraySize + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            Rect removeButtonRect = new Rect(position.x + position.width * 0.5f, position.y + (property.arraySize + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(addButtonRect, "Add Element"))
            {
                property.InsertArrayElementAtIndex(property.arraySize);
            }
            if (GUI.Button(removeButtonRect, "Remove Element"))
            {
                if (property.arraySize > 0)
                {
                    property.DeleteArrayElementAtIndex(property.arraySize - 1);
                }
            }

            // Reducir la indentación
            EditorGUI.indentLevel--;
        }

        // Método para obtener todas las clases que heredan o implementan el tipo base
        private List<Type> GetFilteredTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
                .ToList();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isArray && property.arrayElementType == "string")
            {
                // Ajustar la altura para mostrar todos los elementos de la lista
                return (property.arraySize + 2) * EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
    #endregion
    #region StringPopupAttribute
    [CustomPropertyDrawer(typeof(StringPopupAttribute))]
    public class StringPopupDrawer : PropertyDrawer
    {
        private string[] options;
        private int selectedIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StringPopupAttribute stringPopup = (StringPopupAttribute)attribute;

            if (stringPopup.values != null)
            {
               options = stringPopup.values;
            }
            else if (!string.IsNullOrEmpty(stringPopup.jsonPath))
            {
                LoadOptions(stringPopup.jsonPath);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [StringPopup] with values or a JSON path.");
            }

            if (options.Length == 0) return;

            if (property.propertyType == SerializedPropertyType.String)
            {
                DrawStringPopup(position, property, label);
            }
            else if (property.isArray && property.arrayElementType == "string")
            {
                DrawStringListPopup(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [StringPopup] with a string field.");
            }


        }
        private void DrawStringListPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label.text);

            // Indentar el contenido de la lista
            EditorGUI.indentLevel++;

            // Dibujar cada elemento de la lista
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                Rect elementRect = new Rect(position.x, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                string currentString = element.stringValue;
                selectedIndex = Array.IndexOf(options, currentString);
                if (selectedIndex == -1) selectedIndex = 0;

                selectedIndex = EditorGUI.Popup(elementRect, "Element " + i, selectedIndex, options);
                element.stringValue = options[selectedIndex];
            }

            // Botones para añadir o eliminar elementos
            Rect addButtonRect = new Rect(position.x, position.y + (property.arraySize + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            Rect removeButtonRect = new Rect(position.x + position.width * 0.5f, position.y + (property.arraySize + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(addButtonRect, "Add Element"))
            {
                property.InsertArrayElementAtIndex(property.arraySize);
            }
            if (GUI.Button(removeButtonRect, "Remove Element"))
            {
                if (property.arraySize > 0)
                {
                    property.DeleteArrayElementAtIndex(property.arraySize - 1);
                }
            }

            // Reducir la indentación
            EditorGUI.indentLevel--;
        }
        private void DrawStringPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            // Mostrar popup para una sola opción
            string currentString = property.stringValue;
            selectedIndex = Array.IndexOf(options, currentString);
            if (selectedIndex == -1) selectedIndex = 0;

            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
            property.stringValue = options[selectedIndex];
        }
        private void LoadOptions(string jsonPath)
        {
            options = new string[0];

            // Construir la ruta completa del archivo JSON
            string fullPath = Path.Combine(Application.dataPath, jsonPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"JSON file not found at: {fullPath}");
                return;
            }

            try
            {
                // Leer el archivo JSON
                string json = File.ReadAllText(fullPath);
                // Suponemos que el JSON tiene un array de opciones
                options = JsonUtility.FromJson<JsonOptionsData>(json).options;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading JSON file: {e.Message}");
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isArray && property.arrayElementType == "string")
            {
                // Ajustar la altura para mostrar todos los elementos de la lista
                return (property.arraySize + 2) * EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight;
        }
        [Serializable]
        private class JsonOptionsData
        {
            public string[] options;
        }
    }


    #endregion
    #region GetRequieredComponentAttribute
    [CustomPropertyDrawer(typeof(GetRequieredComponentAttribute))]
    public class GetRequieredComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetRequieredComponentAttribute requiredComponent = (GetRequieredComponentAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.PropertyField(position, property, label);

                if (property.objectReferenceValue == null)
                {
                    Component component = (Component)property.serializedObject.targetObject;
                    property.objectReferenceValue = component.GetComponent(requiredComponent.requiredComponent);
                }
                else if (!requiredComponent.requiredComponent.IsAssignableFrom(property.objectReferenceValue.GetType()))
                {
                    property.objectReferenceValue = null;
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [GetRequieredComponent] with a Component field.");
            }
        }
    }
    #endregion
}


