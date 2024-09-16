using FriendlyEditor.UtilityAttributes;
using System;
using UnityEngine;

public class EditorAttributesTest : MonoBehaviour
{
    #region Highlight Example
    [Highlight(1f, 1f, 0f)]
    public int highlightedInt;
    #endregion

    #region TypePopup Example
    [TypePopup(typeof(AttributeTestParentClass))]
    public string typePopup;

    public Type SelectedType
    {
        get => Type.GetType(typePopup);
        set => typePopup = value?.FullName;
    }
    #endregion
}
