using FriendlyEditor.UtilityAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
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

    [TypePopup(typeof(AttributeTestParentClass))]
    public List<string> typePopupList = new List<string>();

    public List<Type> SelectedTypes
    {
        get => typePopupList.ConvertAll(Type.GetType);
        set => typePopupList = value.ConvertAll(t => t.FullName);
    }

    #endregion

    #region StringPopup Example

    //using string array for Options of popup
    [StringPopup(new[]{"carlos","juan"})]
    public string stringPopup;

    //using string array for Options of popup in list
    [StringPopup(new[] { "carlos", "juan" })]
    public List<string> stringPopupList;

    //using json file for Options of popup
    [StringPopup("Packages/com.GenericStudy.FriendlyEditor/Examples/JsonExamples/PopupJsonExample.Json")]
    public string jsonStringPopup;

    //using json file for Options of popup in list
    [StringPopup("Packages/com.GenericStudy.FriendlyEditor/Examples/JsonExamples/PopupJsonExample.Json")]
    public List<string> jsonStringPopupList = new List<string>();

    #endregion
    #region GetRequieredComponent Example

    // gets the required component and saves it in the use variable along with requireComponent 
    [GetRequieredComponent(typeof(BoxCollider))] //for this example we are using BoxCollider
    public Collider getCollider; //this will get the BoxCollider component and save it in the getCollider variable typof collider

    #endregion
}
