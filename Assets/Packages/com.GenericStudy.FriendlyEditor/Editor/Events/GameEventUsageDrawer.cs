using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameEventUsageAttribute))]
public class GameEventUsageDrawer : PropertyDrawer
{
    private const float Spacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorGUI.LabelField(position, label.text, "GameEventUsage solo puede usarse con un GameEvent.");
            return;
        }

        var usage = (GameEventUsageAttribute)attribute;
        var eventLabel = usage.Usage == GameEventUsage.Raiser
            ? new GUIContent(label.text, "Este componente dispara el canal.")
            : new GUIContent(label.text, "Este componente se suscribe al canal.");

        var eventFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(eventFieldRect, property, eventLabel);

        if (usage.Usage != GameEventUsage.Raiser || property.objectReferenceValue is not GameEvent gameEvent)
        {
            return;
        }

        var channelObject = new SerializedObject(gameEvent);
        var response = channelObject.FindProperty("persistentResponse");
        if (response == null)
        {
            return;
        }

        var responseRect = new Rect(
            position.x,
            eventFieldRect.yMax + Spacing,
            position.width,
            EditorGUI.GetPropertyHeight(response, true));

        channelObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(responseRect, response, new GUIContent("Unity Event"), true);
        if (EditorGUI.EndChangeCheck())
        {
            channelObject.ApplyModifiedProperties();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var usage = (GameEventUsageAttribute)attribute;
        if (usage.Usage != GameEventUsage.Raiser || property.objectReferenceValue is not GameEvent gameEvent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        var response = new SerializedObject(gameEvent).FindProperty("persistentResponse");
        return response == null
            ? EditorGUIUtility.singleLineHeight
            : EditorGUIUtility.singleLineHeight + Spacing + EditorGUI.GetPropertyHeight(response, true);
    }
}
