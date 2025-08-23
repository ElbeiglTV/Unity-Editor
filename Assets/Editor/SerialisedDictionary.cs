using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SerialisedDictionary<Tkey, Tvalue>
{
    public List<Tkey> Keys;
    public List<Tvalue> Values;
    
    public SerialisedDictionary()
    {
        Keys = new List<Tkey>();
        Values = new List<Tvalue>();
    }
    public void Add(Tkey key, Tvalue value)
    {
        Keys.Add(key);
        Values.Add(value);
    }
    public void IgualeTo(Dictionary<Tkey, Tvalue> NormalDictionary)
    {
        foreach (var kvp in NormalDictionary)
        {
            Add(kvp.Key, kvp.Value);
        }
    }
    public Dictionary<Tkey, Tvalue> Overrite()
    {
       Dictionary<Tkey,Tvalue> Dictionary = new Dictionary<Tkey,Tvalue>();
        foreach(var kvp in Keys.Zip(Values,(k,v)=>new {Key = k,Value= v }))
        {
            Dictionary.Add(kvp.Key, kvp.Value);
        }
        // Debug para ver las listas dentro del SerialisedDictionary
       

        return Dictionary;
    }
    public Tvalue GetRectForNode(Tkey key)
    {
        int index = Keys.IndexOf(key);
        if (index >= 0 && index < Values.Count)
        {
            return Values[index];
        }
        else
        {
            // Devolver un rectángulo vacío si la clave no se encuentra en el diccionario
            return default;
        }
    }


}
