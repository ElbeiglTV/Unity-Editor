using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// esta clase se utiliza para enviar datos de forma generica y masiva atraves de un evento sin definir datos previos

public class EventData
{
    private Dictionary<string, object> data = new();

    public EventData Set<T>(string key, T value)
    {
        data[key] = value;
        return this;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (data.TryGetValue(key, out var obj) && obj is T t)
        {
            value = t;
            return true;
        }

        value = default;
        return false;
    }

    public bool Contains(string key)
    {
        return data.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return data.Remove(key);
    }

    public void Clear()
    {
        data.Clear();
    }

    /// <summary>
    /// Obtiene un valor comprobando su tipo en tiempo de ejecución, sin conversiones implícitas.
    /// </summary>
    public bool TryGet(string key, System.Type type, out object value)
    {
        if (type != null && data.TryGetValue(key, out var storedValue) &&
            storedValue != null && type.IsInstanceOfType(storedValue))
        {
            value = storedValue;
            return true;
        }

        value = null;
        return false;
    }
}
