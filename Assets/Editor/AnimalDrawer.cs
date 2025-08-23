using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(Animal), true)]
public class AnimalDrawer : PropertyDrawer
{
    private static Dictionary<string, Type> behaviourTypes;

    static AnimalDrawer()
    {
        behaviourTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(Animal)) && !t.IsAbstract)
            .ToDictionary(t => t.FullName, t => t);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Si ya hay instancia, usamos su nombre de clase como label
        if (property.managedReferenceValue != null)
        {
            string typeName = property.managedReferenceValue.GetType().Name;
            label = new GUIContent(typeName);
        }
        else
        {
            label = new GUIContent("Add Behaviour");
        }

        if (property.managedReferenceValue == null)
        {
            // Dropdown para elegir el tipo
            var typeNames = behaviourTypes.Keys.ToList();
            int index = EditorGUI.Popup(position, label.text, -1, typeNames.Select(n => n.Split('.').Last()).ToArray());
            if (index >= 0)
            {
                var instance = Activator.CreateInstance(behaviourTypes[typeNames[index]]);
                property.managedReferenceValue = instance;
            }
        }
        else
        {
            // Mostrar inspector normal del objeto con label personalizado
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;
        else
            return EditorGUI.GetPropertyHeight(property, label, true);
    }
}