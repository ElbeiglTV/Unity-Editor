using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameEvent))]
public class GameEventEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        return GameEventIconUtility.CreatePreview(width, height);
    }

    protected override void OnHeaderGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(GameEventIconUtility.Icon, GUILayout.Width(32), GUILayout.Height(32));
        EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("persistentResponse"), new GUIContent("Unity Event"), true);
        serializedObject.ApplyModifiedProperties();

        var gameEvent = (GameEvent)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Event Channel", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Raise"))
            {
                gameEvent.Raise();
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Raise is available only in Play Mode and sends an empty EventData.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Listeners", EditorStyles.boldLabel);

        var hasListeners = false;
        foreach (var listenerName in gameEvent.GetRuntimeListenerNames())
        {
            hasListeners = true;
            EditorGUILayout.LabelField($"• {listenerName}");
        }

        if (!hasListeners)
        {
            EditorGUILayout.LabelField("No active listeners.", EditorStyles.miniLabel);
        }
    }
}
