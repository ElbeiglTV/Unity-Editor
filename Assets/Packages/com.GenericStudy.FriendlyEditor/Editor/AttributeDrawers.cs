using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

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
            if (jsonPath.Contains("/Resources/"))
            {
                var JsonName = jsonPath.Replace("/Resources/", "");

                try
                {
                    // Leer el archivo JSON
                    string json = Resources.Load<TextAsset>(JsonName).text;
                    // Suponemos que el JSON tiene un array de opciones
                    options = JsonUtility.FromJson<JsonOptionsData>(json).options;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading JSON file: {e.Message}");
                }

            }
            else
            {
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

    #region DynamicEnumSODrawer

    [CustomPropertyDrawer(typeof(DynamicEnumSelectorAttribute))]
    public class DynamicEnumSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Obtener la referencia del ScriptableObject `DynamicEnum` desde el componente
            var dynamicEnumField = property.serializedObject.FindProperty("EnumConfig");
            DynamicEnumSO dynamicEnum = dynamicEnumField?.objectReferenceValue as DynamicEnumSO;

            if (dynamicEnum != null && dynamicEnum.options.Count > 0)
            {
                // Convertir la lista de opciones en un array de strings para el Popup
                string[] options = dynamicEnum.options.ToArray();

                // Mostrar Popup basado en el índice actual de `selectedStateIndex`
                property.intValue = EditorGUI.Popup(position, label.text, property.intValue, options);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "No options available or DynamicEnum not assigned");
            }

            EditorGUI.EndProperty();
        }
    }
    #endregion
    #region BoolButtonAttributeDrawer
    [CustomPropertyDrawer(typeof(BoolButtonAttribute))]
    public class BoolButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.LabelField(position, label.text, "Use [BoolButton] with bool.");
                return;
            }

            // Obtener los textos del atributo
            BoolButtonAttribute buttonAttribute = (BoolButtonAttribute)attribute;
            string buttonText = property.boolValue ? buttonAttribute.TrueLabel : buttonAttribute.FalseLabel;

            // Dibujar el botón y alternar el valor booleano al hacer clic
            if (GUI.Button(position, buttonText))
            {
                property.boolValue = !property.boolValue;
            }
        }
    }
    #endregion
  
    #region ButtonAttributeEditor
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeEditor : Editor
    {
        private static readonly Dictionary<Type, MethodInfo[]> _methodCache = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            // Root object buttons
            DrawButtons(target, target);

            // SerializedReference buttons
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                {
                    object subObject = iterator.managedReferenceValue;
                    if (subObject != null)
                        DrawButtons(subObject, target);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawButtons(object instance, UnityEngine.Object owner)
        {
            var methods = GetButtonMethods(instance.GetType());

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ButtonAttribute>();

                if (!string.IsNullOrEmpty(attr.ControllingField))
                {
                    SerializedProperty controllingProperty =
                        new SerializedObject(owner).FindProperty(attr.ControllingField);

                    if (!ConditionalUtility.ShouldShow(controllingProperty, attr.DesiredValues))
                        continue; // fully hidden, no space
                }

                string label = string.IsNullOrEmpty(attr.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : attr.Label;

                if (GUILayout.Button(label))
                {
                    Undo.RecordObject(owner, $"Invoke {method.Name}");
                    method.Invoke(instance, null);
                    EditorUtility.SetDirty(owner);
                }
            }
        }

        private static MethodInfo[] GetButtonMethods(Type type)
        {
            if (_methodCache.TryGetValue(type, out var cached))
                return cached;

            var methods = type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            var list = new List<MethodInfo>();

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<ButtonAttribute>() == null)
                    continue;

                if (method.GetParameters().Length != 0)
                {
                    Debug.LogWarning(
                        $"[Button] Method '{method.Name}' on '{type.Name}' must be parameterless.");
                    continue;
                }

                list.Add(method);
            }

            _methodCache[type] = list.ToArray();
            return _methodCache[type];
        }
    }

    #endregion
    #region ButtonSOAttributeEditor
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeSOEditor : Editor
    {
        private static readonly Dictionary<Type, MethodInfo[]> _methodCache = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            DrawButtons(target, target);

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                {
                    object subObject = iterator.managedReferenceValue;
                    if (subObject != null)
                        DrawButtons(subObject, target);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawButtons(object instance, UnityEngine.Object owner)
        {
            var methods = GetButtonMethods(instance.GetType());

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ButtonAttribute>();

                if (!string.IsNullOrEmpty(attr.ControllingField))
                {
                    SerializedProperty controllingProperty =
                        new SerializedObject(owner).FindProperty(attr.ControllingField);

                    if (!ConditionalUtility.ShouldShow(controllingProperty, attr.DesiredValues))
                        continue; // fully hidden, no space
                }

                string label = string.IsNullOrEmpty(attr.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : attr.Label;

                if (GUILayout.Button(label))
                {
                    Undo.RecordObject(owner, $"Invoke {method.Name}");
                    method.Invoke(instance, null);
                    EditorUtility.SetDirty(owner);
                }
            }
        }

        private static MethodInfo[] GetButtonMethods(Type type)
        {
            if (_methodCache.TryGetValue(type, out var cached))
                return cached;

            var methods = type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            var list = new List<MethodInfo>();

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<ButtonAttribute>() == null)
                    continue;

                if (method.GetParameters().Length != 0)
                {
                    Debug.LogWarning(
                        $"[Button] Method '{method.Name}' on '{type.Name}' must be parameterless.");
                    continue;
                }

                list.Add(method);
            }

            _methodCache[type] = list.ToArray();
            return _methodCache[type];
        }
    }

    #endregion
    #region ConditionalAttributeDrawer
    [CustomPropertyDrawer(typeof(ConditionalAttribute))]
    public class ConditionalHideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalAttribute conditional = (ConditionalAttribute)attribute;
            SerializedProperty controllingProperty =
                ConditionalUtility.FindRelativeProperty(property, conditional.ControllingField);

            if (!ConditionalUtility.ShouldShow(controllingProperty, conditional.DesiredValues))
                return;

            if (conditional.UseHeader)
            {
                // Draw header
                Rect headerRect = new Rect(
                    position.x,
                    position.y,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                EditorGUI.LabelField(headerRect, conditional.HeaderText, EditorStyles.boldLabel);

                // Draw property below using its **own display name**
                Rect propRect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUI.GetPropertyHeight(property, true)
                );

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(propRect, property, new GUIContent(property.displayName), true);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel = conditional.Indent;
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.indentLevel = 0;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalAttribute conditional = (ConditionalAttribute)attribute;
            SerializedProperty controllingProperty =
                ConditionalUtility.FindRelativeProperty(property, conditional.ControllingField);

            if (!ConditionalUtility.ShouldShow(controllingProperty, conditional.DesiredValues))
                return -EditorGUIUtility.standardVerticalSpacing;

            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);

            if (conditional.UseHeader)
                return baseHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return baseHeight;
        }
    }

    public static class ConditionalUtility
    {
        public static SerializedProperty FindRelativeProperty(SerializedProperty property, string relativeFieldName)
        {
            if (string.IsNullOrEmpty(relativeFieldName)) return null;

            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot > 0)
                path = path.Substring(0, lastDot); // parent path
            else
                path = ""; // top-level

            string relativePath = string.IsNullOrEmpty(path) ? relativeFieldName : path + "." + relativeFieldName;
            return property.serializedObject.FindProperty(relativePath);
        }

        public static bool ShouldShow(SerializedProperty controllingProperty, object[] desiredValues)
        {
            if (controllingProperty == null) return true;
            if (desiredValues == null || desiredValues.Length == 0) return true;

            switch (controllingProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return desiredValues.Contains(controllingProperty.boolValue);
                case SerializedPropertyType.Enum:
                    return desiredValues.Contains(controllingProperty.enumValueIndex);
                case SerializedPropertyType.Integer:
                    return desiredValues.Contains(controllingProperty.intValue);
                case SerializedPropertyType.Float:
                    return desiredValues.Contains(controllingProperty.floatValue);
                case SerializedPropertyType.String:
                    return desiredValues.Contains(controllingProperty.stringValue);
                default:
                    Debug.LogWarning($"ConditionalUtility: unsupported property type {controllingProperty.propertyType}");
                    return true;
            }
        }
    }

    #endregion
    #region VisualizeAttributeDrawer
    [CustomPropertyDrawer(typeof(VisualizeAttribute))]
    public class VisualizeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> showValuesToggles = new();
        private static readonly Dictionary<string, bool> editValuesToggles = new();
        private static readonly Dictionary<string, bool> mainFoldouts = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            string key = property.propertyPath;
            var attr = (VisualizeAttribute)attribute;

            bool hasValue = property.objectReferenceValue != null;

            // Foldout collapsed
            if (attr.useFoldout)
            {
                if (!mainFoldouts.TryGetValue(key, out bool fold) || !fold) return line + 6f;
            }

            // Just the object field
            if (!hasValue) return line + 6f;

            float totalHeight = line; // Object field

            // Path label
            if (attr.showObjectPath && hasValue)
            {
                string path = GetObjectPath(property.objectReferenceValue);

                float pathHeight = GetPathHeight(path, EditorGUIUtility.currentViewWidth);

                totalHeight += pathHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // Show Values toggle
            totalHeight += line + spacing;

            bool showValues = GetBool(showValuesToggles, key + "_show");

            if (!showValues) return totalHeight + 6f;

            // Edit Values toggle
            totalHeight += line + spacing;

            SerializedObject so = new SerializedObject(property.objectReferenceValue);
            SerializedProperty iterator = so.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script") continue;

                    totalHeight += EditorGUI.GetPropertyHeight(iterator, true) + spacing;
                }
                while (iterator.NextVisible(false));
            }

            return totalHeight + 4f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (VisualizeAttribute)attribute;
            string key = property.propertyPath;
            int originalIndent = EditorGUI.indentLevel;

            EditorGUI.BeginProperty(position, label, property);

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Foldout or Inline PropertyField
            if (attr.useFoldout)
            {
                string foldoutName = string.IsNullOrEmpty(attr.foldoutName) ? property.displayName : attr.foldoutName;
                if (!mainFoldouts.ContainsKey(key)) mainFoldouts[key] = true;

                Rect foldoutRect = new Rect(fieldRect.x + 10f, fieldRect.y, 5f, fieldRect.height);

                float labelWidth = GetLabelWidth(foldoutName);

                Rect labelRect = new Rect(foldoutRect.xMax + 2f, fieldRect.y, labelWidth + 100f, fieldRect.height);

                float objectFieldX = labelRect.xMax + 4f;
                Rect fieldValueRect = new Rect(objectFieldX, fieldRect.y + 2, position.xMax - objectFieldX, fieldRect.height);

                Rect clickableRect = new Rect(foldoutRect.x, foldoutRect.y, fieldValueRect.x - foldoutRect.x, foldoutRect.height);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && clickableRect.Contains(Event.current.mousePosition))
                {
                    mainFoldouts[key] = !mainFoldouts[key];
                    Event.current.Use();
                }

                mainFoldouts[key] = EditorGUI.Foldout(foldoutRect, mainFoldouts[key], GUIContent.none, false); // no label
                EditorGUI.LabelField(labelRect, foldoutName); // custom label
                EditorGUI.ObjectField(fieldValueRect, property, GUIContent.none); // object picker

                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (!mainFoldouts[key])
                {
                    EditorGUI.EndProperty();
                    return;
                }
            }
            else
            {
                string name = string.IsNullOrEmpty(attr.foldoutName) ? property.displayName : attr.foldoutName;
                EditorGUI.PropertyField(fieldRect, property, new GUIContent(name));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // Only continue if object is assigned
            if (property.objectReferenceValue == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            if (attr.showObjectPath)
            {
                string path = GetObjectPath(property.objectReferenceValue);

                GUIStyle wrappedStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                string pathLabel = "Path";

                float labelWidth = GetLabelWidth(pathLabel);

                Rect labelRect = new Rect(fieldRect.x, fieldRect.y, labelWidth + 50f, EditorGUIUtility.singleLineHeight);
                float valueWidth = fieldRect.width - labelRect.width;

                float pathHeight = GetPathHeight(path, valueWidth);
                Rect valueRect = new Rect(labelRect.xMax, fieldRect.y, fieldRect.width - labelRect.width, pathHeight);

                EditorGUI.LabelField(labelRect, pathLabel);
                EditorGUI.LabelField(valueRect, path, wrappedStyle);

                fieldRect.y += pathHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            // Show Values toggle
            EditorGUI.indentLevel = originalIndent + 2;

            GUIStyle toggleStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };

            Rect showRect = fieldRect;
            bool show = GetBool(showValuesToggles, key + "_show");
            show = EditorGUI.ToggleLeft(showRect, new GUIContent("Show Values"), show, toggleStyle);
            showValuesToggles[key + "_show"] = show;
            fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!show)
            {
                fieldRect.y += 6f; // space under "Show Values" when false
                EditorGUI.indentLevel = originalIndent;
                EditorGUI.EndProperty();
                return;
            }

            // Edit Values toggle
            Rect editRect = fieldRect;
            bool edit = GetBool(editValuesToggles, key + "_edit");
            edit = EditorGUI.ToggleLeft(editRect, new GUIContent("Edit Values"), edit, toggleStyle);
            editValuesToggles[key + "_edit"] = edit;
            fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Draw internal properties of the ScriptableObject
            var target = property.objectReferenceValue;
            SerializedObject so = new SerializedObject(target);
            var iterator = so.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script") continue;
                    EditorGUI.indentLevel = originalIndent;
                    float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    Rect propRect = new(fieldRect.x, fieldRect.y, fieldRect.width, propHeight);

                    EditorGUI.BeginDisabledGroup(!edit);
                    EditorGUI.PropertyField(propRect, iterator, true);
                    EditorGUI.EndDisabledGroup();

                    fieldRect.y += propHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                while (iterator.NextVisible(false));
            }

            if (edit)
                so.ApplyModifiedProperties();

            EditorGUI.indentLevel = originalIndent;
            EditorGUI.EndProperty();
        }

        private bool GetBool(Dictionary<string, bool> dict, string key)
        {
            if (!dict.TryGetValue(key, out bool value))
                value = false;

            return value;
        }

        private static string GetObjectPath(UnityEngine.Object obj)
        {
            if (obj is Component component)
            {
                return $"({component.gameObject.scene.name})\n{GetIndentedPath(component.transform)}";
            }

            if (obj is GameObject go)
            {
                return $"({go.scene.name})\n{GetIndentedPath(go.transform)}";
            }

            return AssetDatabase.GetAssetPath(obj);
        }

        private float GetLabelWidth(string label)
        {
            GUIContent labelContent = new GUIContent(label);
            Vector2 size = GUI.skin.label.CalcSize(labelContent);
            return size.x;
        }

        private float GetPathHeight(string path, float width)
        {
            GUIStyle style = new(EditorStyles.wordWrappedLabel);
            return style.CalcHeight(new GUIContent(path), width);
        }

        private static string GetIndentedPath(Transform transform)
        {
            List<string> hierarchy = new();

            while (transform != null)
            {
                hierarchy.Add(transform.name);
                transform = transform.parent;
            }

            hierarchy.Reverse();

            StringBuilder sb = new();

            for (int i = 0; i < hierarchy.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append("↳ ");
                    sb.AppendLine(hierarchy[i]);
                }
                else
                {
                    sb.Append(new string(' ', (i - 1) * 3));
                    sb.Append("   ↳ ");
                    sb.AppendLine(hierarchy[i]);
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
    #endregion
    #region ReadAttributeDrawer
    [CustomPropertyDrawer(typeof(ReadAttribute))]
    public class ReadDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
    #endregion

    #region RelatedValuesDrawer
    [CustomPropertyDrawer(typeof(RelatedValues<,,>), true)]
    public class RelatedValuesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Handle prefix label & indent correctly
            EditorGUIUtility.labelWidth = 150;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Find fields
            var leftProp = property.FindPropertyRelative("Left");
            var centreProp = property.FindPropertyRelative("Centre");
            var rightProp = property.FindPropertyRelative("Right");

            // Dynamic width distribution (percentages of available width)
            float spacing = 5f;
            float totalWidth = position.width;
            float leftWidth = totalWidth * 0.3f;
            float equalWidth = 10f; // fixed width for "="
            float enumWidth = totalWidth * 0.3f;
            float rightWidth = totalWidth - (leftWidth + equalWidth + enumWidth) - spacing * 3;

            // Calculate rects
            Rect leftRect = new Rect(position.x, position.y, leftWidth, position.height);
            Rect equalRect = new Rect(leftRect.xMax + spacing, position.y, equalWidth, position.height);
            Rect enumRect = new Rect(equalRect.xMax + spacing, position.y, enumWidth, position.height);
            Rect rightRect = new Rect(enumRect.xMax + spacing, position.y, rightWidth, position.height);

            // Draw fields
            EditorGUI.PropertyField(leftRect, leftProp, GUIContent.none, true);
            EditorGUI.LabelField(equalRect, "=");
            EditorGUI.PropertyField(enumRect, centreProp, GUIContent.none, true);
            EditorGUI.PropertyField(rightRect, rightProp, GUIContent.none, true);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
    #endregion
    #region DrawTogetherDrawer
    [CustomPropertyDrawer(typeof(DrawTogetherAttribute))]
    public class DrawTogetherDrawer : PropertyDrawer
    {
        private const float MIN_COLUMN_WIDTH = 40f;
        private const float COLUMN_SPACING = 4f; // spacing between columns

        // Optional: customize header label style
        public static GUIStyle HeaderLabelStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = (DrawTogetherAttribute)attribute;
            if (attr.Type != DrawType.Struct)
                return EditorGUI.GetPropertyHeight(property, label, true);

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float headerHeight = EditorGUIUtility.singleLineHeight;

            var visibleChildren = GetVisibleChildren(property);
            float maxChildHeight = 0f;
            foreach (var child in visibleChildren)
            {
                float h = EditorGUI.GetPropertyHeight(child, true);
                if (h > maxChildHeight) maxChildHeight = h;
            }
            if (maxChildHeight <= 0f) maxChildHeight = EditorGUIUtility.singleLineHeight;

            return headerHeight + spacing + maxChildHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (DrawTogetherAttribute)attribute;

            if (attr.Type != DrawType.Struct)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float headerHeight = EditorGUIUtility.singleLineHeight;

            // Draw group label
            float groupLabelWidth = 0f;
            if (attr.DisplayGroupName && !string.IsNullOrEmpty(attr.DisplayName))
            {
                groupLabelWidth = EditorGUIUtility.labelWidth;
                Rect gRect = new Rect(position.x, position.y, groupLabelWidth, headerHeight);
                EditorGUI.LabelField(gRect, attr.DisplayName, EditorStyles.boldLabel);
            }

            var visibleChildren = GetVisibleChildren(property);
            if (visibleChildren.Count == 0) return;

            float availableWidth = position.width - groupLabelWidth - COLUMN_SPACING * (visibleChildren.Count - 1);
            float columnWidth = Mathf.Max(MIN_COLUMN_WIDTH, availableWidth / visibleChildren.Count);
            float x = position.x + groupLabelWidth;
            float valueY = position.y + headerHeight + spacing;

            // Draw headers
            GUIStyle headerStyle = HeaderLabelStyle ?? EditorStyles.label;
            for (int i = 0; i < visibleChildren.Count; i++)
            {
                var child = visibleChildren[i];
                string headerName = GetChildDisplayName(child);
                Rect headerRect = new Rect(x, position.y, columnWidth, headerHeight);
                EditorGUI.LabelField(headerRect, headerName, headerStyle);
                x += columnWidth + COLUMN_SPACING;
            }

            // Draw values
            x = position.x + groupLabelWidth;
            for (int i = 0; i < visibleChildren.Count; i++)
            {
                var child = visibleChildren[i];
                float childHeight = EditorGUI.GetPropertyHeight(child, true);
                Rect fieldRect = new Rect(x, valueY, columnWidth, childHeight);
                EditorGUI.PropertyField(fieldRect, child, GUIContent.none, true);
                x += columnWidth + COLUMN_SPACING;
            }
        }

        private List<SerializedProperty> GetVisibleChildren(SerializedProperty parent)
        {
            List<SerializedProperty> list = new List<SerializedProperty>();
            var iter = parent.Copy();
            var end = iter.GetEndProperty(true);

            // Move into first child
            if (!iter.NextVisible(true))
                return list;

            while (!SerializedProperty.EqualContents(iter, end))
            {
                // only children of the 'parent' (guard)
                if (!iter.propertyPath.StartsWith(parent.propertyPath + ".", StringComparison.Ordinal))
                    break;

                bool visible = true;

                // Find FieldInfo for this serialized child
                FieldInfo field = GetFieldInfo(iter);

                if (field != null)
                {
                    // check for ConditionalAttribute on the field (if present)
                    var cond = field.GetCustomAttributes(typeof(ConditionalAttribute), true)
                                    .FirstOrDefault() as ConditionalAttribute;

                    if (cond != null)
                    {
                        // Try to resolve the controlling property relative to the parent element
                        var controlling = FindControllingPropertyForChild(parent, iter, cond.ControllingField);

                        // If not found or condition not met -> hide this member
                        if (controlling == null || !ConditionalUtility.ShouldShow(controlling, cond.DesiredValues))
                            visible = false;
                    }
                }

                if (visible)
                    list.Add(iter.Copy());

                iter.NextVisible(false);
            }

            // Sort found children by DrawTogether order (falls back to 0)
            list.Sort((a, b) =>
            {
                int ao = GetDrawOrder(a);
                int bo = GetDrawOrder(b);
                return ao.CompareTo(bo);
            });

            return list;
        }

        private SerializedProperty FindControllingPropertyForChild(SerializedProperty parentElement, SerializedProperty child, string controllingField)
        {
            if (string.IsNullOrEmpty(controllingField))
                return null;

            var so = parentElement.serializedObject;
            string parentPath = parentElement.propertyPath; // e.g. "myList.Array.data[3]" or "someStruct"

            // Candidates: start with parent element local, then climb ancestors, then root
            var tried = new HashSet<string>(StringComparer.Ordinal);

            // 1) direct element-local candidate
            string candidate = parentPath + "." + controllingField;
            if (!tried.Contains(candidate)) tried.Add(candidate);
            var sp = so.FindProperty(candidate);
            if (sp != null) return sp;

            // 2) if controllingField itself contains dots, try parent.local + that path (already tried above),
            //    but also try the controllingField as absolute (root) path
            // try climbing up ancestors: parentPath, remove last segment iteratively
            string ancestor = parentPath;
            while (true)
            {
                int lastDot = ancestor.LastIndexOf('.');
                if (lastDot <= 0) break;
                ancestor = ancestor.Substring(0, lastDot);
                candidate = ancestor + "." + controllingField;
                if (!tried.Contains(candidate))
                {
                    tried.Add(candidate);
                    sp = so.FindProperty(candidate);
                    if (sp != null) return sp;
                }
            }

            // 3) finally try the controlling field as root (top-level) property name
            candidate = controllingField;
            if (!tried.Contains(candidate))
            {
                sp = so.FindProperty(candidate);
                if (sp != null) return sp;
            }

            // nothing found
            return null;
        }

        private int GetDrawOrder(SerializedProperty prop)
        {
            FieldInfo field = GetFieldInfo(prop);
            if (field == null) return 0;
            var att = field.GetCustomAttributes(typeof(DrawTogetherAttribute), false)
                           .FirstOrDefault() as DrawTogetherAttribute;
            if (att == null) return 0;
            return att.Order;
        }

        private FieldInfo GetFieldInfo(SerializedProperty prop)
        {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            string[] elements = path.Split('.');

            Type type = prop.serializedObject.targetObject.GetType();
            FieldInfo field = null;

            for (int i = 0; i < elements.Length; i++)
            {
                string element = elements[i];
                int bracket = element.IndexOf('[');

                if (bracket >= 0)
                {
                    // It's an array/list element
                    string name = element.Substring(0, bracket);
                    field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) return null;

                    // Dive into element type, NOT the instance
                    if (field.FieldType.IsArray)
                        type = field.FieldType.GetElementType();
                    else if (field.FieldType.IsGenericType)
                        type = field.FieldType.GetGenericArguments().First();
                    else
                        type = field.FieldType;
                }
                else
                {
                    field = type.GetField(element, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) return null;

                    // Dive into field type
                    type = field.FieldType;
                }
            }

            return field; // this should now always be the declaring field with attributes
        }

        private string GetChildDisplayName(SerializedProperty child)
        {
            FieldInfo field = GetFieldInfo(child);
            if (field == null) return ObjectNames.NicifyVariableName(child.name);

            var att = field.GetCustomAttributes(typeof(DrawTogetherAttribute), false)
                           .FirstOrDefault() as DrawTogetherAttribute;

            if (att != null && !string.IsNullOrEmpty(att.DisplayName))
                return att.DisplayName;

            return ObjectNames.NicifyVariableName(field.Name);
        }
    }
    #endregion
    #region FlexibleSliderDrawer
    [CustomPropertyDrawer(typeof(FlexibleSliderAttribute))]
    public class FlexibleSliderDrawer : PropertyDrawer
    {
        // --- Configurable variables ---
        private const float defaultSliderSpacing = 4f;           // space between vector sliders
        private const float defaultComponentLabelHeight = 12f;   // height of X/Y/Z/W labels
        private const float defaultVectorSliderHeight = 18f;     // height of sliders
        private const float defaultMinMaxFieldWidth = 50f;       // width of the min/max numeric fields
        private const float defaultMinMaxSliderProportion = 1f;  // proportion of total width slider occupies
        private const float defaultVectorSliderProportion = .5f;  // proportion of total width sliders occupy

        // Allow per-slider type width proportion (0..1)
        public static float MinMaxSliderProportion = defaultMinMaxSliderProportion;
        public static float VectorSliderProportion = defaultVectorSliderProportion;
        public static float SliderSpacing = defaultSliderSpacing;
        public static float ComponentLabelHeight = defaultComponentLabelHeight;
        public static float VectorSliderHeight = defaultVectorSliderHeight;
        public static float MinMaxFieldWidth = defaultMinMaxFieldWidth;

        private readonly string[] componentLabels = { "X", "Y", "Z", "W" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (FlexibleSliderAttribute)attribute;

            // Resolve min/max values
            float min = attr.Min;
            float max = attr.Max;
            float step = attr.Step;

            // Dynamic min/max from properties
            if (!string.IsNullOrEmpty(attr.MinProperty))
            {
                var minProp = FindPropertySmart(property, attr.MinProperty);
                if (minProp != null) min = minProp.floatValue;
            }

            if (!string.IsNullOrEmpty(attr.MaxProperty))
            {
                var maxProp = FindPropertySmart(property, attr.MaxProperty);
                if (maxProp != null) max = maxProp.floatValue;
            }

            // Vector2 range property
            if (!string.IsNullOrEmpty(attr.RangeVectorProperty))
            {
                var rangeProp = FindPropertySmart(property, attr.RangeVectorProperty);
                if (rangeProp != null && rangeProp.propertyType == SerializedPropertyType.Vector2)
                {
                    Vector2 range = rangeProp.vector2Value;
                    min = range.x;
                    max = range.y;
                }
            }

            // Step from property
            if (!string.IsNullOrEmpty(attr.StepProperty))
            {
                var stepProp = FindPropertySmart(property, attr.StepProperty);
                if (stepProp != null) step = stepProp.floatValue;
            }

            EditorGUI.BeginProperty(position, label, property);

            switch (attr.Type)
            {
                case SliderType.Normal:
                    DrawNormalSlider(position, property, label, min, max, step, attr.Clamp);
                    break;

                case SliderType.MinMax:
                    DrawMinMaxSlider(position, property, label, min, max, step, attr.Clamp);
                    break;

                case SliderType.Vector:
                    DrawVectorSlider(position, property, min, max, step, attr.Clamp);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private void DrawNormalSlider(Rect position, SerializedProperty property, GUIContent label,
                                      float min, float max, float step, ClampType clamp)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                float value = EditorGUI.Slider(position, label, property.floatValue, min, max);

                if (step > 0f) value = Mathf.Round(value / step) * step;
                if (clamp == ClampType.Clamped) value = Mathf.Clamp(value, min, max);

                property.floatValue = value;
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int minInt = Mathf.FloorToInt(min);
                int maxInt = Mathf.CeilToInt(max);

                int value = EditorGUI.IntSlider(position, label, property.intValue, minInt, maxInt);

                if (step > 0f) value = Mathf.RoundToInt(value / step) * (int)step;
                if (clamp == ClampType.Clamped) value = Mathf.Clamp(value, minInt, maxInt);

                property.intValue = value;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use FlexibleSlider with float or int");
            }
        }

        private void DrawMinMaxSlider(Rect position, SerializedProperty property, GUIContent label, float min, float max, float step, ClampType clamp)
        {
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            position = EditorGUI.PrefixLabel(position, label);

            float spacing = SliderSpacing * 3;
            float fieldHeight = EditorGUIUtility.singleLineHeight;

            Rect minFieldRect = new Rect(position.x, position.y, MinMaxFieldWidth, fieldHeight);

            Rect maxFieldRect = new Rect(position.xMax - MinMaxFieldWidth, position.y, MinMaxFieldWidth, fieldHeight);

            Rect sliderRect = new Rect(minFieldRect.xMax + spacing, position.y, maxFieldRect.xMin - minFieldRect.xMax - spacing * 2, fieldHeight);

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 val = property.vector2Value;

                val.x = EditorGUI.FloatField(minFieldRect, val.x);
                val.y = EditorGUI.FloatField(maxFieldRect, val.y);

                EditorGUI.MinMaxSlider(sliderRect, ref val.x, ref val.y, min, max);

                if (step > 0f)
                {
                    val.x = Mathf.Round(val.x / step) * step;
                    val.y = Mathf.Round(val.y / step) * step;
                }

                if (clamp == ClampType.Clamped)
                {
                    val.x = Mathf.Clamp(val.x, min, val.y);
                    val.y = Mathf.Clamp(val.y, val.x, max);
                }

                property.vector2Value = val;
            }

            EditorGUI.indentLevel = indent;
        }


        private void DrawVectorSlider(Rect position, SerializedProperty property,
                                      float min, float max, float step, ClampType clamp)
        {
            int componentCount = 0;
            bool isInt = false;
            if (property.propertyType == SerializedPropertyType.Vector2) componentCount = 2;
            else if (property.propertyType == SerializedPropertyType.Vector3) componentCount = 3;
            else if (property.propertyType == SerializedPropertyType.Vector4) componentCount = 4;
            else if (property.propertyType == SerializedPropertyType.Vector2Int) { componentCount = 2; isInt = true; }
            else if (property.propertyType == SerializedPropertyType.Vector3Int) { componentCount = 3; isInt = true; }

            // Reserve space for label
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(position.x, position.y, labelWidth * VectorSliderProportion, VectorSliderHeight);
            EditorGUI.LabelField(labelRect, property.displayName);

            // Full slider rect including the label space
            Rect slidersRect = new Rect(position.x + labelRect.width + SliderSpacing, position.y, position.width - labelRect.width, VectorSliderHeight);

            // Get current vector values
            Vector4 values = Vector4.zero;
            if (isInt)
            {
                if (componentCount == 2) { var v2 = property.vector2IntValue; values.x = v2.x; values.y = v2.y; }
                else if (componentCount == 3) { var v3 = property.vector3IntValue; values.x = v3.x; values.y = v3.y; values.z = v3.z; }
            }
            else
            {
                switch (componentCount)
                {
                    case 2: values = property.vector2Value; break;
                    case 3: values = property.vector3Value; break;
                    case 4: values = property.vector4Value; break;
                }
            }

            // Divide slidersRect among components
            float sliderWidth = (slidersRect.width - SliderSpacing * (componentCount - 1)) / componentCount;

            for (int i = 0; i < componentCount; i++)
            {
                float val = i == 0 ? values.x : i == 1 ? values.y : i == 2 ? values.z : values.w;
                Rect sliderRect = new Rect(slidersRect.x + i * (sliderWidth + SliderSpacing),
                                           slidersRect.y, sliderWidth, VectorSliderHeight);

                EditorGUI.BeginChangeCheck();
                if (isInt)
                {
                    int minInt = Mathf.FloorToInt(min);
                    int maxInt = Mathf.CeilToInt(max);
                    int fVal = EditorGUI.IntSlider(sliderRect, (int)val, minInt, maxInt);

                    if (EditorGUI.EndChangeCheck())
                        val = fVal;
                }
                else
                {
                    float fVal = EditorGUI.Slider(sliderRect, val, min, max);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (step > 0f)
                            fVal = Mathf.Round(fVal / step) * step;

                        if (clamp == ClampType.Clamped)
                            fVal = Mathf.Clamp(fVal, min, max);

                        val = fVal;
                    }
                }

                // Save back
                if (i == 0) values.x = val;
                else if (i == 1) values.y = val;
                else if (i == 2) values.z = val;
                else values.w = val;
            }

            // Write back
            if (isInt)
            {
                if (componentCount == 2) property.vector2IntValue = new Vector2Int((int)values.x, (int)values.y);
                else if (componentCount == 3) property.vector3IntValue = new Vector3Int((int)values.x, (int)values.y, (int)values.z);
            }
            else
            {
                switch (componentCount)
                {
                    case 2: property.vector2Value = new Vector2(values.x, values.y); break;
                    case 3: property.vector3Value = new Vector3(values.x, values.y, values.z); break;
                    case 4: property.vector4Value = new Vector4(values.x, values.y, values.z, values.w); break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Vector2 ||
                property.propertyType == SerializedPropertyType.Vector3 ||
                property.propertyType == SerializedPropertyType.Vector4 ||
                property.propertyType == SerializedPropertyType.Vector2Int ||
                property.propertyType == SerializedPropertyType.Vector3Int)
            {
                return VectorSliderHeight; // extra spacing
            }

            return EditorGUIUtility.singleLineHeight;
        }

        private SerializedProperty FindRelativeProperty(SerializedProperty property, string relativeName)
        {
            if (property == null || string.IsNullOrEmpty(relativeName))
                return null;

            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0)
                return property.serializedObject.FindProperty(relativeName);

            string parentPath = path.Substring(0, lastDot);
            return property.serializedObject.FindProperty(parentPath + "." + relativeName);
        }

        private SerializedProperty FindPropertySmart(SerializedProperty property, string name)
        {
            // 1️⃣ Try relative (same object / list element)
            var relative = FindRelativeProperty(property, name);
            if (relative != null)
                return relative;

            // 2️⃣ Fallback to absolute (root object)
            return property.serializedObject.FindProperty(name);
        }
    }
    #endregion

    #region DefaultStringDrawer
    [CustomPropertyDrawer(typeof(DefaultStringAttribute))]
    public class DefaultStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use only on string");
                return;
            }

            var attributeData = (DefaultStringAttribute)attribute;

            const float buttonWidth = 25f;

            Rect fieldRect = new Rect(
                position.x,
                position.y,
                position.width - buttonWidth - 2,
                position.height);

            Rect buttonRect = new Rect(
                fieldRect.xMax + 2,
                position.y,
                buttonWidth,
                position.height);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (GUI.Button(buttonRect, "↺"))
            {
                property.stringValue = attributeData.defaultValue;
            }
        }
    }
    #endregion

    #region TagFieldDrawer

    [CustomPropertyDrawer(typeof(TagsAttribute))]
    public class TagsDrawer : PropertyDrawer
    {
        private const float Padding = 4;
        private const float TagHeight = 20;
        private const float InputHeight = 18;

        private static readonly Dictionary<string, string> InputBuffer = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            List<string> tags = Parse(property.stringValue);

            float width = EditorGUIUtility.currentViewWidth - 40;

            float x = 0;
            float y = EditorGUIUtility.singleLineHeight + Padding;

            // Altura del input
            y += InputHeight + Padding;

            foreach (var tag in tags)
            {
                float w = GUI.skin.button.CalcSize(new GUIContent(tag)).x + 10;

                if (x + w > width)
                {
                    x = 0;
                    y += TagHeight + Padding;
                }

                x += w + Padding;
            }

            // Siempre dejamos espacio para una fila de tags
            y += TagHeight;

            return y;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Label
            Rect labelRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            List<string> tags = Parse(property.stringValue);

            float y = labelRect.yMax + Padding;

            //==========================
            // Input
            //==========================

            if (!InputBuffer.TryGetValue(property.propertyPath, out string input))
                input = "";

            Rect inputRect = new(position.x, y, position.width, InputHeight);

            GUI.SetNextControlName(property.propertyPath);

            string newInput = EditorGUI.TextField(inputRect, input);

            if (newInput != input)
                InputBuffer[property.propertyPath] = newInput;

            Event e = Event.current;

            if (GUI.GetNameOfFocusedControl() == property.propertyPath &&
                e.type == EventType.KeyDown &&
                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                if (!string.IsNullOrWhiteSpace(newInput))
                {
                    if (!newInput.StartsWith("_"))
                        newInput = "_" + newInput;

                    property.stringValue += newInput;

                    InputBuffer[property.propertyPath] = "";

                    property.serializedObject.ApplyModifiedProperties();

                    GUI.FocusControl(null);

                    e.Use();
                }
            }

            //==========================
            // Tags
            //==========================

            y += InputHeight + Padding;

            float x = position.x;
            float maxWidth = position.width;

            bool changed = false;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];

                float width = GUI.skin.button.CalcSize(new GUIContent(tag)).x + 10;

                if (x + width > position.x + maxWidth)
                {
                    x = position.x;
                    y += TagHeight + Padding;
                }

                Rect button = new(x, y, width, TagHeight);

                if (GUI.Button(button, tag))
                {
                    tags.RemoveAt(i);
                    changed = true;
                    break;
                }

                x += width + Padding;
            }

            if (changed)
            {
                property.stringValue = string.Concat(tags);
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        private static List<string> Parse(string value)
        {
            List<string> tags = new();

            if (string.IsNullOrEmpty(value))
                return tags;

            int start = -1;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '_')
                {
                    if (start != -1)
                        tags.Add(value.Substring(start, i - start));

                    start = i;
                }
            }

            if (start != -1)
                tags.Add(value.Substring(start));

            return tags;
        }
    }

    #endregion
}