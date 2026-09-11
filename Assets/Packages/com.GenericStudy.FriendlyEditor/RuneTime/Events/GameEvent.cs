using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Canal de eventos compartido. Cada asset de GameEvent mantiene sus propios listeners de runtime.
/// </summary>
[CreateAssetMenu(fileName = "GameEvent", menuName = "Friendly Editor/Events/Game Event")]
public class GameEvent : ScriptableObject
{
    // Callbacks persistentes editables desde Inspector. Unity serializa la configuración,
    // pero EventData sigue siendo solamente un payload de runtime.
    [SerializeField] private EventDataUnityEvent persistentResponse = new();

    // El estado de las suscripciones nunca se serializa en el asset.
    [NonSerialized] private readonly HashSet<Action<EventData>> listeners = new();

    public void Raise()
    {
        Raise(new EventData());
    }

    public void Raise(EventData data)
    {
        data ??= new EventData();
        RemoveDestroyedListeners();

        // Una copia permite que un listener se suscriba o desuscriba durante la notificación.
        var listenersSnapshot = new List<Action<EventData>>(listeners);
        foreach (var listener in listenersSnapshot)
        {
            if (!IsAlive(listener))
            {
                listeners.Remove(listener);
                continue;
            }

            try
            {
                listener(data);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        try
        {
            persistentResponse?.Invoke(data);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    public void Subscribe(Action<EventData> listener)
    {
        if (listener != null)
        {
            listeners.Add(listener);
        }
    }

    public void Unsubscribe(Action<EventData> listener)
    {
        if (listener != null)
        {
            listeners.Remove(listener);
        }
    }

    /// <summary>Información de depuración para el inspector; no se guarda en el asset.</summary>
    public IEnumerable<string> GetRuntimeListenerNames()
    {
        RemoveDestroyedListeners();
        foreach (var listener in listeners)
        {
            yield return GetListenerName(listener);
        }
    }

    private void OnDisable()
    {
        listeners.Clear();
    }

    private void RemoveDestroyedListeners()
    {
        listeners.RemoveWhere(listener => !IsAlive(listener));
    }

    private static bool IsAlive(Action<EventData> listener)
    {
        return listener != null &&
               (listener.Target is not UnityEngine.Object unityObject || unityObject != null);
    }

    private static string GetListenerName(Action<EventData> listener)
    {
        if (listener.Target is Component component)
        {
            return $"{component.gameObject.name} ({component.GetType().Name})";
        }

        if (listener.Target is UnityEngine.Object unityObject)
        {
            return $"{unityObject.name} ({unityObject.GetType().Name})";
        }

        return listener.Target == null
            ? $"Static: {listener.Method.DeclaringType?.Name}.{listener.Method.Name}"
            : $"{listener.Target.GetType().Name}.{listener.Method.Name}";
    }
}
