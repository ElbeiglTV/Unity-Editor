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
    [SerializeField] private List<BaseNode> nodes = new();
    private BaseNode connectingNode = null; // nodo desde el que se arrastra la conexión
    private bool connectingTruePort = true; // si estamos conectando True o False
    private Vector2 scrollPos;

    [MenuItem("Window/Custom/Node Editor")]
    private static void Open() => GetWindow<NodeEditorWindow>("Node Editor");

    private void OnGUI()
    {
        Event e = Event.current;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        Rect canvas = new Rect(0, 0, 2000, 2000);
        GUILayoutUtility.GetRect(canvas.width, canvas.height);

        BeginWindows();
        for (int i = 0; i < nodes.Count; i++)
        {
            int index = i; // copia local
            nodes[index].rect = GUI.Window(index, nodes[index].rect, id => DrawNodeWindow(id, nodes[index]), nodes[index].title);
        }
        EndWindows();

        DrawConnections();
        HandleConnections(Event.current);
        EditorGUILayout.EndScrollView();

        // 🔌 Dibujar conexiones
        foreach (BaseNode node in nodes)
        {
            if (node is DecisionNode decision)
            {
                if (decision.trueNode != null)
                    DrawConnection(node.rect, decision.trueNode.rect, Color.green);
                if (decision.falseNode != null)
                    DrawConnection(node.rect, decision.falseNode.rect, Color.red);
            }
        }

        if (GUILayout.Button("Add Decision Node"))
            nodes.Add(new DecisionNode() { rect = new Rect(100, 100, 250, 120) });
        if (GUILayout.Button("Add Behaviour Node"))
            nodes.Add(new BehaviourNode() { rect = new Rect(200, 200, 250, 80) });

        if (e.type == EventType.MouseDrag) Repaint();
    }


    private void DrawNodeWindow(int id, BaseNode node)
    {
        // Fondo
        GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
        areaStyle.normal.background = node.type == NodeTypes.Decision
            ? MakeTex(2, 2, new Color(0.3f, 0.6f, 1f))
            : MakeTex(2, 2, new Color(0.6f, 1f, 0.3f));

        GUI.Box(new Rect(0, 0, node.rect.width, node.rect.height), GUIContent.none, areaStyle);

        // Circulito arriba izquierda
        Handles.color = Color.gray;
        Handles.DrawSolidDisc(new Vector3(12, 15, 0), Vector3.forward, 6);
        Handles.color = node.type == NodeTypes.Decision ? Color.cyan : Color.green;
        Handles.DrawSolidDisc(new Vector3(12, 15, 0), Vector3.forward, 4);

        // Foldout / Header
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        node.isExpanded = EditorGUILayout.Foldout(node.isExpanded, node.title);
        GUILayout.EndHorizontal();

        if (node.isExpanded)
        {
            GUILayout.Space(10);

            if (node is DecisionNode decision)
            {
                // Botones True / False
                DrawDecisionButtons(decision);
            }
            else if (node is BehaviourNode)
            {
                GUILayout.Label("End Behaviour");
            }
        }

        GUI.DragWindow();
    }
    private void DrawDecisionButtons(DecisionNode decision)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        if (GUILayout.Button(decision.trueNode != null ? "Restart True Node" : "New True Node", GUILayout.ExpandWidth(false)))
        {
            if (decision.trueNode != null)
            {
                nodes.Remove(decision.trueNode);
                decision.trueNode = null;
            }
            connectingNode = decision;
            connectingTruePort = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        if (GUILayout.Button(decision.falseNode != null ? "Restart False Node" : "New False Node", GUILayout.ExpandWidth(false)))
        {
            if (decision.falseNode != null)
            {
                nodes.Remove(decision.falseNode);
                decision.falseNode = null;
            }
            connectingNode = decision;
            connectingTruePort = false;
        }
        GUILayout.EndHorizontal();
    }
    private void HandleConnections(Event e)
    {
        if (connectingNode != null)
        {
            Vector3 mousePos = e.mousePosition;
            Handles.DrawBezier(
                new Vector3(connectingNode.rect.xMax, connectingNode.rect.center.y),
                mousePos,
                new Vector3(connectingNode.rect.xMax + 50, connectingNode.rect.center.y),
                mousePos,
                Color.yellow,
                null,
                3f
            );

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Verificar si se clickea sobre otro nodo
                foreach (var node in nodes)
                {
                    if (node == connectingNode) continue;
                    if (node.rect.Contains(e.mousePosition))
                    {
                        ConnectNodes(connectingNode, node, connectingTruePort);
                        connectingNode = null;
                        break;
                    }
                }
            }
        }
    }

    private void DrawConnections()
    {
        foreach (BaseNode node in nodes)
        {
            if (node is DecisionNode decision)
            {
                if (decision.trueNode != null)
                    DrawConnection(node.rect, decision.trueNode.rect, Color.green);
                if (decision.falseNode != null)
                    DrawConnection(node.rect, decision.falseNode.rect, Color.red);
            }
        }
    }

    private void DrawConnection(Rect from, Rect to, Color color)
    {
        Handles.DrawBezier(
            new Vector3(from.xMax, from.center.y),
            new Vector3(to.xMin, to.center.y),
            new Vector3(from.xMax + 50, from.center.y),
            new Vector3(to.xMin - 50, to.center.y),
            color,
            null,
            3f
        );
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
