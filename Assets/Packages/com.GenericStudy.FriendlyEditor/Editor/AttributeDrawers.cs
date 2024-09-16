using UnityEngine;
using UnityEditor;
using FriendlyEditor.UtilityAttributes;
using System.Collections.Generic;
using System.Linq;
using System;

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
    public class TypePopupDrawer : PropertyDrawer
    {
        private List<Type> filteredTypes = new List<Type>();
        private string[] typeNames;
        private int selectedIndex = 0;

        // OnGUI se encarga de dibujar el popup en el Inspector
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Asegúrate de que la propiedad sea de tipo string (que almacena el nombre del Type)
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [TypeFilter] with string.");
                return;
            }

            // Obtener el atributo y su baseType
            TypePopupAttribute typeFilter = (TypePopupAttribute)attribute;
            Type baseType = typeFilter.baseType;

            // Obtener todas las clases que heredan o implementan el tipo base
            if (filteredTypes.Count == 0)
            {
                filteredTypes = GetFilteredTypes(baseType);
                typeNames = filteredTypes.Select(t => t.FullName).ToArray();
            }

            // Obtener el índice seleccionado actual
            selectedIndex = Array.IndexOf(typeNames, property.stringValue);
            if (selectedIndex == -1) selectedIndex = 0; // Por defecto al primer elemento si no coincide

            // Dibujar el popup
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, typeNames);

            // Guardar el nombre del tipo seleccionado en la propiedad
            property.stringValue = typeNames[selectedIndex];
        }

        // Método para obtener todas las clases que heredan o implementan el tipo base
        private List<Type> GetFilteredTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass && !type.IsAbstract)
                .ToList();
        }
    }
    #endregion
}

