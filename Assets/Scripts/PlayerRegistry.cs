// PlayerRegistry.cs
using System.Collections.Generic;
using UnityEngine;

public static class PlayerRegistry
{
    private static readonly List<PlayerHealth> players = new List<PlayerHealth>();
    public static IReadOnlyList<PlayerHealth> Players => players;

    public static void Register(PlayerHealth p)
    {
        if (p != null && !players.Contains(p))
            players.Add(p);
    }

    public static void Unregister(PlayerHealth p)
    {
        if (p != null)
            players.Remove(p);
    }
}
