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

public class NodeEditorWindow : EditorWindow
{
    [SerializeField] private List<MyNode> nodes = new();
    private Vector2 scrollPos;

    [MenuItem("Window/Custom/Node Editor")]
    private static void Open() => GetWindow<NodeEditorWindow>("Node Editor");

    private void OnGUI()
    {
        Event e = Event.current;

        // Fondo con scroll
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        Rect canvas = new Rect(0, 0, 2000, 2000);
        GUILayoutUtility.GetRect(canvas.width, canvas.height);

        BeginWindows();

        // Dibujar nodos como ventanas
        for (int i = 0; i < nodes.Count; i++)
        {
            MyNode node = nodes[i];
            Rect rect = new Rect(node.position, new Vector2(250, 150));
            rect = GUI.Window(i, rect, id => DrawNodeWindow(id, node), node.title);
            node.position = rect.position;
        }

        EndWindows();
        EditorGUILayout.EndScrollView();

        // Botón para agregar nodos
        if (GUILayout.Button("Add Node"))
        {
            nodes.Add(new MyNode()
            {
                title = "New Node " + nodes.Count,
                position = new Vector2(100, 100)
            });
        }

        // Repaint cuando arrastramos
        if (e.type == EventType.MouseDrag)
            Repaint();
    }

    private void DrawNodeWindow(int id, MyNode node)
    {
        SerializedObject so = new SerializedObject(this);
        SerializedProperty nodesProp = so.FindProperty("nodes");
        SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(id);

        // Esto dibuja el nodo con sus propiedades como en el inspector
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(nodeProp, true);
        if (EditorGUI.EndChangeCheck())
            so.ApplyModifiedProperties();

        GUI.DragWindow(); // Permite arrastrar el nodo
    }
}
