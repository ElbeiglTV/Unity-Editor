using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MyNode
{
    public string title;
    public Vector2 position;
 
    [SerializeReference]public Animal Animal;
}
[System.Serializable]
    public class Animal
    {
    public string title;
    }
[System.Serializable]
public class Perro : Animal
{
    public int GuauVolume;
}

#region Nodes
public enum NodeTypes
{
    Decision,
    Behaviour
}

[System.Serializable]
public abstract class BaseNode
{
    public string title;
    public Rect rect;
    public NodeTypes type;
    public bool isExpanded;
}

[System.Serializable]
public class DecisionNode : BaseNode
{
    public DecisionNode()
    {
        type = NodeTypes.Decision;
        title = "Decision Node";
    }

    // Salidas
    public BaseNode trueNode;
    public BaseNode falseNode;
}

[System.Serializable]
public class BehaviourNode : BaseNode
{
    public BehaviourNode()
    {
        type = NodeTypes.Behaviour;
        title = "Behaviour Node";
    }
}

#endregion

public class NodeEditorWindow : EditorWindow
{
    [SerializeField] private List<MyNode> nodes = new();
    private Vector2 scrollPos;

    [MenuItem("Window/Custom/Node Editor")]
    private static void Open() => GetWindow<NodeEditorWindow>("Node Editor");

    private void OnGUI()
    {
        Event e = Event.current;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        Rect canvas = new Rect(0, 0, 2000, 2000);
        GUILayoutUtility.GetRect(canvas.width, canvas.height);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty nodesProp = so.FindProperty("nodes");

        BeginWindows();

        for (int i = 0; i < nodes.Count; i++)
        {
            SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(i);

            // 🔥 Altura dinámica
            float propertyHeight = EditorGUI.GetPropertyHeight(nodeProp, true);
            float headerHeight = 30f; // espacio para título y circulito
            float dynamicHeight = propertyHeight + headerHeight;

            MyNode node = nodes[i];
            Rect rect = new Rect(node.position, new Vector2(250, dynamicHeight));

            rect = GUI.Window(i, rect, id => DrawNodeWindow(id, node, nodesProp), node.title);
            node.position = rect.position;
        }

        EndWindows();
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Node"))
        {
            nodes.Add(new MyNode()
            {
                title = "New Node " + nodes.Count,
                position = new Vector2(100, 100)
            });
        }

        if (e.type == EventType.MouseDrag) Repaint();
    }

    private void DrawNodeWindow(int id, MyNode node, SerializedProperty nodesProp)
    {
        SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(id);

        // El rect completo de la ventana
        Rect fullRect = new Rect(0, 0, 250, EditorGUI.GetPropertyHeight(nodeProp, true) + 30);

        // --- Fondo ---
        GUI.Box(fullRect, GUIContent.none);

        // --- Circulito arriba izquierda ---
        Handles.color = Color.gray;
        Handles.DrawSolidDisc(new Vector3(12, 15, 0), Vector3.forward, 6);
        Handles.color = Color.cyan; // O NodeColor(node)
        Handles.DrawSolidDisc(new Vector3(12, 15, 0), Vector3.forward, 4);

        // --- Inspector interno ---
        Rect contentRect = new Rect(5, 25, fullRect.width - 10, fullRect.height - 30);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(contentRect, nodeProp, true);
        if (EditorGUI.EndChangeCheck())
            nodeProp.serializedObject.ApplyModifiedProperties();

        GUI.DragWindow();
    }

    private void DrawNode(BaseNode node)
    {
        GUIStyle style = new GUIStyle("window");
        switch (node.type)
        {
            case NodeTypes.Decision:
                style.normal.background = MakeTex(2, 2, new Color(0.3f, 0.6f, 1f)); // azul
                break;
            case NodeTypes.Behaviour:
                style.normal.background = MakeTex(2, 2, new Color(0.6f, 1f, 0.3f)); // verde
                break;
        }

        node.rect = GUILayout.Window(GetHashCode(), node.rect, (id) =>
        {
            GUILayout.Label(node.title, EditorStyles.boldLabel);

            if (node is DecisionNode decision)
            {
                GUILayout.Label("True -> " + (decision.trueNode != null ? decision.trueNode.title : "None"));
                GUILayout.Label("False -> " + (decision.falseNode != null ? decision.falseNode.title : "None"));
            }
            else if (node is BehaviourNode behaviour)
            {
                GUILayout.Label("End Behaviour");
            }

            GUI.DragWindow();

        }, node.title, style);
    }
    private bool CanConnect(BaseNode from, BaseNode to)
    {
        if (from is DecisionNode decision)
        {
            // Puede tener 2 salidas: True y False
            return (decision.trueNode == null || decision.falseNode == null);
        }
        else if (from is BehaviourNode)
        {
            // Behaviour es final -> no puede tener salidas
            return false;
        }
        return false;
    }

    private void ConnectNodes(BaseNode from, BaseNode to, bool asTrue)
    {
        if (from is DecisionNode decision)
        {
            if (asTrue && decision.trueNode == null)
                decision.trueNode = to;
            else if (!asTrue && decision.falseNode == null)
                decision.falseNode = to;
        }
    }

    /// Helper para fondo sólido
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }


}
