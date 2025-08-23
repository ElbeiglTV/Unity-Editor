using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "TreeData",menuName ="TreeDataSO")]
[Serializable]
public class SOdecisionTree : ScriptableObject
{
    [SerializeField]
    public DecisionNode RootNode;
    [SerializeField]
    public SerialisedDictionary<DecisionNode, Rect> TreeData = new SerialisedDictionary<DecisionNode, Rect>();
}
