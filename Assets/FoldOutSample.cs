using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldOutSample : MonoBehaviour
{



    [Foldout("MyFoldOut", "myInt", "myString")]
    public int myInt;
    [HideInInspector]public string myString;
    
    

    public int myOtherInt;

}
