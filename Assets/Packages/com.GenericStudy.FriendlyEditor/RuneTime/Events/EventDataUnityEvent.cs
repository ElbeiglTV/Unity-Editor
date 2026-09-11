using System;
using UnityEngine.Events;

/// <summary>
/// Evento de Unity que transporta el EventData de runtime sin intentar serializar su contenido.
/// </summary>
[Serializable]
public class EventDataUnityEvent : UnityEvent<EventData>
{
}
