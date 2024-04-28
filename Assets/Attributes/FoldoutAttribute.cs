using UnityEngine;
using UnityEditor;


[System.AttributeUsage(System.AttributeTargets.Field)]
public class FoldoutAttribute : PropertyAttribute
{
    public string GroupName { get; private set; }
    public string[] IncludedProperties { get; private set; }

    public FoldoutAttribute(string groupName, params string[] includedProperties)
    {
        GroupName = groupName;
        IncludedProperties = includedProperties;
    }
}


[CustomPropertyDrawer(typeof(FoldoutAttribute))]
public class FoldoutDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        FoldoutAttribute foldoutAttribute = (FoldoutAttribute)attribute;
        string foldoutName = foldoutAttribute.GroupName;
        string[] includedProperties = foldoutAttribute.IncludedProperties;

        SerializedProperty[] childProperties = new SerializedProperty[includedProperties.Length];
        for (int i = 0; i < includedProperties.Length; i++)
        {
            childProperties[i] = property.serializedObject.FindProperty(includedProperties[i]);
        }

        bool isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, property.isExpanded, foldoutName);
        if (isExpanded)
        {
            EditorGUI.indentLevel++;

            foreach (SerializedProperty childProperty in childProperties)
            {
                EditorGUILayout.PropertyField(childProperty, true);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndFoldoutHeaderGroup();
        property.isExpanded = isExpanded;

    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        FoldoutAttribute foldoutAttribute = (FoldoutAttribute)attribute;
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        float height = 0;
        SerializedProperty parentProperty = property.serializedObject.FindProperty(property.propertyPath.Split('.')[0]);
        SerializedProperty childProperty = parentProperty.Copy();
        bool enterChildren = true;
        while (childProperty.Next(enterChildren))
        {
            bool MachName  = false;
            foreach(string name in foldoutAttribute.IncludedProperties)
            {
                if(childProperty.name == name)
                {
                    MachName = true;
                break;
                }
                
            }
            if (childProperty.depth == 1)
                break;
            if (MachName)
            {

            height += EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            enterChildren = false;
            }
        }
        return height;
    }
}
