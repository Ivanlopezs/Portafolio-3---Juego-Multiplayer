
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DominoTurnManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private DominoGameSetup gameSetup;
    [SerializeField] private DominoBoardManager boardManager;

    [Header("Game Settings")]
    [SerializeField] private bool initializeOnStart = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text currentPlayerText;
    [SerializeField] private TMP_Text reserveCountText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Hand UI")]
    [SerializeField] private PlayerHandUI playerHandUI;

    [Header("Placement Controls")]
    [SerializeField] private Button placeLeftButton;
    [SerializeField] private Button placeRightButton;

    private List<PlayerData> players;
    private List<DominoTile> reserve;

    private int currentPlayerIndex;

    private DominoRules rules;

    private DominoTile selectedTile;

    public PlayerData CurrentPlayer
    {
        get
        {
            if (players == null || players.Count == 0)
                return null;

            return players[currentPlayerIndex];
        }
    }

    public int ReserveCount => reserve != null ? reserve.Count : 0;

    private void Awake()
    {
        IOrganismCompatibilityStrategy compatibility = new EcosystemCompatibilityStrategy();
        rules = new DominoRules(compatibility);
    }

    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeTurnSystem();
        }
    }

    public void InitializeTurnSystem()
    {
        if (gameSetup == null || boardManager == null)
        {
            Debug.LogError("Faltan referencias del sistema de turnos.");
            return;
        }

        selectedTile = null;
        UpdatePlacementButtons();

        gameSetup.InitializeGame();

        boardManager.InitializeBoard();

        players = gameSetup.Players;
        reserve = gameSetup.Reserve;

        if (players == null || players.Count == 0)
        {
            Debug.LogError("No hay jugadores disponibles.");
            return;
        }

        if (reserve == null || reserve.Count == 0)
        {
            Debug.LogError("No hay fichas disponibles en la reserva.");
            return;
        }

        int randomIndex = Random.Range(0, reserve.Count);
        DominoTile firstTile = reserve[randomIndex];
        DominoBoardMoveResult result = boardManager.TryPlaceTile(firstTile, DominoPlacementSide.Right);

        if (!result.IsValid)
        {
            Debug.LogError("No se pudo colocar la primera ficha: " + result.Explanation);
            return;
        }

        reserve.RemoveAt(randomIndex);
        currentPlayerIndex = 0;

        UpdateTurnUI();

        ShowFeedback("¡Comienza la partida! Se colocó una ficha inicial aleatoria.");

        Debug.Log("Primera ficha colocada: " + firstTile.leftOrganism + " - " + firstTile.rightOrganism);
    }

    public bool CanPlayTile(DominoTile tile)
    {
        if (tile == null || boardManager == null)
            return false;

        DominoBoard board = boardManager.Board;

        if (board.IsEmpty)
            return true;

        DominoMoveResult rightResult = rules.ValidateMove(tile, board.RightOpenEnd);

        if (rightResult.IsValid)
            return true;

        DominoMoveResult leftResult = rules.ValidateMove(tile, board.LeftOpenEnd);

        return leftResult.IsValid;
    }

    public bool CanCurrentPlayerPlay()
    {
        if (CurrentPlayer == null)
            return false;

        foreach (DominoTile tile in CurrentPlayer.hand.Tiles)
        {
            if (CanPlayTile(tile))
                return true;
        }

        return false;
    }

    public void SelectTile(DominoTile tile)
    {
        if (tile == null || CurrentPlayer == null)
            return;

        if (!CurrentPlayer.hand.Tiles.Contains(tile))
            return;

        selectedTile = tile;

        bool canPlaceLeft = CanPlaceOnSide(tile, DominoPlacementSide.Left);

        bool canPlaceRight = CanPlaceOnSide(tile, DominoPlacementSide.Right);

        if (placeLeftButton != null)
            placeLeftButton.interactable = canPlaceLeft;

        if (placeRightButton != null)
            placeRightButton.interactable = canPlaceRight;

        ShowFeedback("Ficha seleccionada. Elige dónde quieres colocarla.");
    }

    private bool CanPlaceOnSide(
        DominoTile tile,
        DominoPlacementSide side)
    {
        if (tile == null || boardManager == null)
            return false;

        DominoBoard board = boardManager.Board;

        if (board.IsEmpty)
            return true;

        OrganismType openEnd =
            side == DominoPlacementSide.Left
            ? board.LeftOpenEnd
            : board.RightOpenEnd;

        return rules.ValidateMove(tile, openEnd).IsValid;
    }

    public void PlaceSelectedTileLeft()
    {
        PlaceSelectedTile(DominoPlacementSide.Left);
    }

    public void PlaceSelectedTileRight()
    {
        PlaceSelectedTile(DominoPlacementSide.Right);
    }

    private void PlaceSelectedTile(DominoPlacementSide side)
    {
        if (selectedTile == null)
        {
            ShowFeedback("Primero selecciona una ficha.");
            return;
        }

        if (!CanPlaceOnSide(selectedTile, side))
        {
            ShowFeedback("No existe compatibilidad en ese extremo.");
            return;
        }

        TryPlayTile(selectedTile, side);
    }

    public bool TryPlayTile(
        DominoTile tile,
        DominoPlacementSide side)
    {
        if (CurrentPlayer == null || tile == null)
            return false;

        if (!CurrentPlayer.hand.Tiles.Contains(tile))
        {
            ShowFeedback(
                "Esta ficha no pertenece al jugador actual."
            );
            return false;
        }

        if (!CanPlaceOnSide(tile, side))
        {
            ShowFeedback(
                "Esta ficha no puede conectarse a ese extremo."
            );
            return false;
        }

        DominoBoardMoveResult result =
            boardManager.TryPlaceTile(tile, side);

        if (result == null || !result.IsValid)
        {
            ShowFeedback(
                result != null
                ? result.Explanation
                : "No se pudo colocar la ficha."
            );

            return false;
        }

        CurrentPlayer.hand.RemoveTile(tile);

        Debug.Log(
            CurrentPlayer.playerName +
            " colocó una ficha en " + side
        );

        selectedTile = null;

        UpdatePlacementButtons();

        AdvanceTurn();

        return true;
    }

    // Roba fichas hasta encontrar una jugable o agotar la reserva.
    public void DrawUntilPlayable()
    {
        if (CurrentPlayer == null)
            return;

        if (CanCurrentPlayerPlay())
        {
            ShowFeedback(
                "Ya tienes una ficha que puedes colocar."
            );
            return;
        }

        if (reserve.Count == 0)
        {
            ShowFeedback("La reserva está vacía. Puedes pasar.");
            UpdateTurnUI();
            return;
        }

        int drawnTiles = 0;

        while (reserve.Count > 0)
        {
            int lastIndex = reserve.Count - 1;

            DominoTile drawnTile = reserve[lastIndex];

            reserve.RemoveAt(lastIndex);

            CurrentPlayer.hand.AddTile(drawnTile);

            drawnTiles++;

            if (CanPlayTile(drawnTile))
                break;
        }

        UpdateTurnUI();

        if (CanCurrentPlayerPlay())
        {
            ShowFeedback(
                "Robaste " + drawnTiles +
                " ficha(s). ¡Ya puedes jugar!"
            );
        }
        else
        {
            ShowFeedback(
                "Robaste " + drawnTiles +
                " ficha(s), pero todavía no puedes jugar."
            );
        }

        Debug.Log("Fichas robadas: " + drawnTiles);
    }

    // Permite pasar si no quedan fichas en la reserva.
    public void PassTurn()
    {
        if (CurrentPlayer == null)
            return;

        if (reserve.Count > 0)
        {
            ShowFeedback(
                "Todavía quedan fichas en la reserva."
            );
            return;
        }

        if (CanCurrentPlayerPlay())
        {
            ShowFeedback(
                "Tienes una jugada disponible. No puedes pasar."
            );
            return;
        }

        Debug.Log(CurrentPlayer.playerName + " pasó su turno.");

        selectedTile = null;

        UpdatePlacementButtons();

        AdvanceTurn();
    }

    // Cambia al siguiente jugador.
    private void AdvanceTurn()
    {
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
            currentPlayerIndex = 0;

        selectedTile = null;

        UpdatePlacementButtons();

        UpdateTurnUI();

        Debug.Log(
            "Ahora es el turno de " +
            CurrentPlayer.playerName
        );
    }

    // Actualiza los botones de colocación.
    private void UpdatePlacementButtons()
    {
        if (placeLeftButton != null)
            placeLeftButton.interactable = false;

        if (placeRightButton != null)
            placeRightButton.interactable = false;
    }

    // Actualiza la interfaz del juego.
    private void UpdateTurnUI()
    {
        if (CurrentPlayer != null && currentPlayerText != null)
        {
            currentPlayerText.text =
                "Turno: " + CurrentPlayer.playerName;
        }

        if (reserveCountText != null)
        {
            reserveCountText.text =
                "Reserva: " + ReserveCount;
        }

        if (playerHandUI != null && CurrentPlayer != null)
        {
            playerHandUI.ShowHand(CurrentPlayer.hand);
        }
    }

    // Botón Robar.
    public void OnDrawButtonClicked()
    {
        DrawUntilPlayable();
    }

    // Botón Pasar.
    public void OnPassButtonClicked()
    {
        PassTurn();
    }

    // Botón Reiniciar.
    public void OnRestartButtonClicked()
    {
        InitializeTurnSystem();

        ShowFeedback(
            "¡La partida ha comenzado nuevamente!"
        );
    }

    // Muestra mensajes en la interfaz y consola.
    private void ShowFeedback(string message)
    {
        Debug.Log(message);

        if (feedbackText != null)
            feedbackText.text = message;
    }

}