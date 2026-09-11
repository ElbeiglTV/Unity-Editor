using UnityEngine;

/// <summary>Listener para respuestas de Inspector que requieren el EventData de runtime.</summary>
public class GameEventDataListener : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private EventDataUnityEvent response;

    private void OnEnable()
    {
        if (gameEvent != null)
        {
            gameEvent.Subscribe(OnEventRaised);
        }
    }

    private void OnDisable()
    {
        if (gameEvent != null)
        {
            gameEvent.Unsubscribe(OnEventRaised);
        }
    }

    private void OnEventRaised(EventData data)
    {
        response?.Invoke(data);
    }
}
