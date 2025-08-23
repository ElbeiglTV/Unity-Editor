using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SOdecisionTree))]
public class DecisionTreeView : Editor
{

    /*private bool isDragging = false;
    private Vector2 mouseOffset;
    private Vector2 dragStartPosition;*/
    private Vector2 globalOffset = Vector2.zero;


    
    public override void OnInspectorGUI()
    {
        SOdecisionTree tree = (SOdecisionTree)target;
      //  Event currentEvent = Event.current;
        //Vector2 mousePosition = currentEvent.mousePosition;
        
        /*switch (currentEvent.type)
        {
            case EventType.MouseDown:
                if (currentEvent.button == 1) // Clic derecho
                {
                        Debug.Log("isDragin");
                    foreach (var kvp in tree.TreeData.Values)
                    {

                        isDragging = true;
                        dragStartPosition = currentEvent.mousePosition - kvp.position;

                        break;

                    }
                }
                break;
            case EventType.MouseUp:
                isDragging = false;
                break;
            case EventType.MouseDrag:
                if (isDragging)
                {
                    globalOffset = new Vector2(currentEvent.mousePosition.x - dragStartPosition.x, currentEvent.mousePosition.y - dragStartPosition.y);

                }

                // Repintar el inspector para mostrar los cambios
                Repaint();
                break;
        }*/






        foreach (var Tnode in tree.TreeData.Keys.Zip(tree.TreeData.Values, (k, v) => new { Key = k, Value = v }))
        {

            DrawNode(Tnode.Key);
            if (Tnode.Key.trueNode != null)
            {
                DibujarLinea(Tnode.Key, Tnode.Key.trueNode, Color.green);
            }
            else
            {
                foreach (DecisionNode node in tree.TreeData.Keys)
                {
                    if (Tnode.Key.trueNodeID == node.UniqueID)
                    {
                        Tnode.Key.trueNode = node;
                    }
                }
            }

            if (Tnode.Key.falseNode != null)
            {
                DibujarLinea(Tnode.Key, Tnode.Key.falseNode, Color.red);
            }
            else
            {
                foreach (DecisionNode node in tree.TreeData.Keys)
                {
                    if (Tnode.Key.falseNodeID == node.UniqueID)
                    {
                        Tnode.Key.falseNode = node;
                    }
                }
            }

        }



    }


    Rect GetNodeRect(DecisionNode node)
    {
        float alto = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 5;
        Rect rect = NodeRect(node);
        rect.x += globalOffset.x;
        rect.y += globalOffset.y;
        rect.width = 200;
        rect.height = alto;
        return rect;

    }
    void DibujarLinea(DecisionNode ActualNode, DecisionNode TargetNode, Color color)
    {
        Handles.color = color;
        Vector2 actualNodePos = NodeRect(ActualNode).position;
        Vector2 targetNodePos = NodeRect(TargetNode).position;

        Handles.DrawLine(new Vector2(actualNodePos.x + NodeRect(ActualNode).width, actualNodePos.y + EditorGUIUtility.singleLineHeight / 2),
                         new Vector2(targetNodePos.x, targetNodePos.y + EditorGUIUtility.singleLineHeight / 2));
    }
    void DrawNode(DecisionNode node)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(GetNodeRect(node), GUI.skin.box);
        
        GUILayout.Label("Node ID: " + node.GetHashCode().ToString());
        GUILayout.Label("Question " + node.Questions);
        GUILayout.Label(node.UniqueID);
       
        
        GUILayout.EndArea();
        // Dibujar líneas de conexión aquí
        Handles.EndGUI();

    }
    Rect NodeRect(DecisionNode node)
    {
        SOdecisionTree tree = (SOdecisionTree)target;
        foreach (var Tnode in tree.TreeData.Keys.Zip(tree.TreeData.Values, (k, v) => new { Key = k, Value = v }))
        {
            if (node.UniqueID == Tnode.Key.UniqueID)
            {

                return Tnode.Value;
            }
        }
        Debug.Log("no encontrado");
        return default(Rect);
    }

}
