using System;
using UnityEngine;

public enum GameEventUsage
{
    Raiser,
    Listener
}

/// <summary>
/// Declara el rol de una referencia GameEvent para que su Inspector muestre solo
/// los controles relevantes. Los campos Raiser exponen el UnityEvent del canal.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class GameEventUsageAttribute : PropertyAttribute
{
    public GameEventUsage Usage { get; }

    public GameEventUsageAttribute(GameEventUsage usage)
    {
        Usage = usage;
    }
}
