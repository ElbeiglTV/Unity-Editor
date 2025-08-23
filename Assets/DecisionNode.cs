using System;
using UnityEngine;
[Serializable]
public class DecisionNode
{
    [SerializeField]
    public string UniqueID;
    [SerializeField]
    public Questions Questions;

    [SerializeField]
    public DecisionNode trueNode;
    public string trueNodeID;

    [SerializeField]
    public DecisionNode falseNode;
    public string falseNodeID;

    [HideInInspector]
    public Vector2 position; // Nueva propiedad para almacenar la posición del nodo en el editor
    [HideInInspector]
    public bool isExpanded;
    public DecisionNode()
    {
        UniqueID = Guid.NewGuid().ToString(); ;
    }
}
public enum Questions
{
   isWall,isFloor
}