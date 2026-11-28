using TMPro;
using UnityEngine;

public class DominoUIObserver : MonoBehaviour, IDominoObserver
{
    [Header("References")]
    [SerializeField] private DominoGameEvents gameEvents;

    [Header("UI References")]
    [SerializeField] private TMP_Text currentPlayerText;
    [SerializeField] private TMP_Text reserveCountText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Hand UI")]
    [SerializeField] private PlayerHandUI playerHandUI;

    private void OnEnable()
    {
        if (gameEvents != null)
        {
            gameEvents.RegisterObserver(this);
        }
    }

    private void OnDisable()
    {
        if (gameEvents != null)
        {
            gameEvents.UnregisterObserver(this);
        }
    }

    public void OnTurnChanged(int playerIndex, string playerName, PlayerHand handToShow)
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = "Turno de: " + playerName;
        }

        if (playerHandUI != null && handToShow != null)
        {
            playerHandUI.ShowHand(handToShow);
        }
    }

    public void OnTilePlaced(DominoTile tile, int playerIndex, DominoPlacementSide side)
    {
        Debug.Log("El jugador " + playerNameFromIndex(playerIndex) + " colocó una ficha.");
    }

    public void OnTilesDrawn(int playerIndex, int amount)
    {
        Debug.Log("El jugador " + playerIndex + " robó " + amount + " ficha(s).");
    }

    public void OnPlayerPassed(int playerIndex, string playerName)
    {
        if (feedbackText != null)
        {
            feedbackText.text = playerName + " pasó su turno.";
        }
    }

    public void OnReserveChanged(int remainingTiles)
    {
        if (reserveCountText != null)
        {
            reserveCountText.text = "Reserva: " + remainingTiles;
        }
    }

    public void OnGameBlocked()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "¡La partida está bloqueada!";
        }
    }

    public void OnFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private string playerNameFromIndex(int playerIndex)
    {
        return "Jugador " + (playerIndex + 1);
    }
}
