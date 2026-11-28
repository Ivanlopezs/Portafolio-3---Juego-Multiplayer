using System;
using System.Collections.Generic;
using UnityEngine;

public class DominoGameEvents : MonoBehaviour
{
    private readonly List<IDominoObserver> observers = new List<IDominoObserver>();

    public void RegisterObserver(IDominoObserver observer)
    {
        if (observer == null || observers.Contains(observer))
            return;

        observers.Add(observer);
        Debug.Log("Observador registrado: " + observer.GetType().Name);
    }

    public void UnregisterObserver(IDominoObserver observer)
    {
        if (observer == null)
            return;

        observers.Remove(observer);
        Debug.Log("Observador eliminado.");
    }

    private void NotifyObservers(Action<IDominoObserver> notification)
    {
        IDominoObserver[] observersCopy = observers.ToArray();
        foreach (IDominoObserver observer in observersCopy)
        {
            if (observer != null)
                notification(observer);
        }
    }

    public void NotifyTurnChanged(int playerIndex, string playerName, PlayerHand handToShow)
    {
        NotifyObservers(observer => observer.OnTurnChanged(playerIndex, playerName, handToShow));
    }

    public void NotifyTilePlaced(DominoTile tile, int playerIndex, DominoPlacementSide side)
    {
        NotifyObservers(observer => observer.OnTilePlaced(tile, playerIndex, side));
    }

    public void NotifyTilesDrawn(int playerIndex, int amount)
    {
        NotifyObservers(observer => observer.OnTilesDrawn(playerIndex, amount));
    }

    public void NotifyPlayerPassed(int playerIndex, string playerName)
    {
        NotifyObservers(observer => observer.OnPlayerPassed(playerIndex, playerName));
    }

    public void NotifyReserveChanged(int remainingTiles)
    {
        NotifyObservers(observer => observer.OnReserveChanged(remainingTiles));
    }

    public void NotifyGameBlocked()
    {
        NotifyObservers(observer => observer.OnGameBlocked());
    }

    public void NotifyFeedback(string message)
    {
        NotifyObservers(observer => observer.OnFeedback(message));
    }
}
