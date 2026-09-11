using UnityEngine;
using UnityEngine.Events;

/// <summary>Listener para respuestas configurables desde Inspector que no requieren el payload.</summary>
public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private UnityEvent response;

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
        response?.Invoke();
    }
}
