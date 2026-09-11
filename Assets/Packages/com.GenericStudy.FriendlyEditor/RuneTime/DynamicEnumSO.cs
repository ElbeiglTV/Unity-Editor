using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DynamicEnum", menuName = "CustomVars/DynamicEnum")]
public class DynamicEnumSO : ScriptableObject
{
    [Tooltip("Lista de opciones que se van a utilisar en el enum EL INDICE 0 DEVE SER SIEMPRE UN ESTADO INDEFINIDO O GENERICO BASICAMENTE ES EL NADA SELECCIONADO/CUALQUIERA QUE NO DEFINAS MANUALMENTE")]
    public List<string> options = new List<string>();
   
}
