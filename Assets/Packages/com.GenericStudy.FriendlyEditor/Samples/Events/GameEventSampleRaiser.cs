using UnityEngine;

/// <summary>Emite un GameEvent una vez al iniciar la escena de ejemplo.</summary>
public class GameEventSampleRaiser : MonoBehaviour
{
    [SerializeField, GameEventUsage(GameEventUsage.Raiser)]
    private GameEvent sampleEvent;

    private void Start()
    {
        var data = new EventData()
            .Set("message", "Hello from the raiser")
            .Set("frame", Time.frameCount);

        Debug.Log("[Event Channels Sample] Raiser: sending the event.", this);
        sampleEvent.Raise(data);
    }
}
