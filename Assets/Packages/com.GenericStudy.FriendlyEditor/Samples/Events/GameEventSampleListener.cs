using UnityEngine;

/// <summary>Recibe el GameEvent de la escena de ejemplo y muestra su payload.</summary>
public class GameEventSampleListener : MonoBehaviour
{
    [SerializeField, GameEventUsage(GameEventUsage.Listener)]
    private GameEvent sampleEvent;

    private void OnEnable()
    {
        sampleEvent.Subscribe(OnSampleEventRaised);
    }

    private void OnDisable()
    {
        sampleEvent.Unsubscribe(OnSampleEventRaised);
    }

    private void OnSampleEventRaised(EventData data)
    {
        data.TryGet("message", out string message);
        data.TryGet("frame", out int frame);
        Debug.Log($"[Event Channels Sample] Listener: '{message}' (frame {frame}).", this);
    }
}
