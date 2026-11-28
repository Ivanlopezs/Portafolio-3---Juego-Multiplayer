using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class DominoTurnManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private DominoGameSetup gameSetup;
    [SerializeField] private DominoBoardManager boardManager;
    [SerializeField] private PhotonGameSyncManager photonGameSyncManager;

    [Header("Observer")]
    [SerializeField] private DominoGameEvents gameEvents;

    [Header("Game Settings")]
    [SerializeField] private bool initializeOnStart = true;

    [Header("Placement Controls")]
    [SerializeField] private Button placeLeftButton;
    [SerializeField] private Button placeRightButton;

    private List<PlayerData> players;
    private List<DominoTile> reserve;

    private int currentPlayerIndex;
    private DominoRules rules;
    private DominoTile selectedTile;
    private int consecutivePasses = 0;
    private int onlineReserveCount = -1;

    public PlayerData CurrentPlayer
    {
        get
        {
            if (players == null || players.Count == 0)
                return null;

            return players[currentPlayerIndex];
        }
    }

    public int ReserveCount
    {
        get
        {
            if (GameSettings.SelectedMode == GameMode.Online &&
                onlineReserveCount >= 0)
            {
                return onlineReserveCount;
            }

            return reserve != null ? reserve.Count : 0;
        }
    }

    private void Awake()
    {
        if (gameEvents == null)
        {
            Debug.LogError(
                "DominoGameEvents no está asignado. DominoTurnManager necesita el sistema Observer para funcionar."
            );
            enabled = false;
            return;
        }

        IOrganismCompatibilityStrategy compatibility = new EcosystemCompatibilityStrategy();
        rules = new DominoRules(compatibility);
    }

    
    private void Start()
    {
        if (GameSettings.SelectedMode == GameMode.Local)
        {
            if (initializeOnStart)
            {
                InitializeTurnSystem();
            }
        }
        else if (GameSettings.SelectedMode == GameMode.Online)
        {
            Debug.Log("Modo online: preparando la partida.");

            if (PhotonNetwork.IsMasterClient)
            {
                InitializeOnlineTurnSystem();
            }
            else
            {
                ShowFeedback("Esperando a que el anfitrión prepare la partida...");
            }
        }
    }


    public void InitializeTurnSystem()
    {
        if (GameSettings.SelectedMode == GameMode.Online)
        {
            Debug.LogWarning(
                "No se puede inicializar una partida local durante el modo online."
            );

            return;
        }

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

    private void InitializeOnlineTurnSystem()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("No estás dentro de una sala online.");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el anfitrión puede preparar la partida.");
            return;
        }

        if (gameSetup == null ||
            boardManager == null ||
            photonGameSyncManager == null)
        {
            Debug.LogError("Faltan referencias para iniciar la partida online.");
            return;
        }

        int onlinePlayerCount = PhotonNetwork.CurrentRoom.MaxPlayers;

        gameSetup.InitializeOnlineGame(onlinePlayerCount);

        players = gameSetup.Players;
        reserve = gameSetup.Reserve;

        if (players == null || players.Count != onlinePlayerCount)
        {
            Debug.LogError("No se pudieron preparar los jugadores online.");
            return;
        }

        if (reserve == null)
        {
            Debug.LogError("No se pudo preparar la reserva.");
            return;
        }

        boardManager.InitializeOnlineBoard();

        if (reserve.Count == 0)
        {
            Debug.LogError("No hay fichas disponibles para iniciar el tablero.");
            return;
        }

        int randomIndex = Random.Range(0, reserve.Count);

        DominoTile firstTile = reserve[randomIndex];

        reserve.RemoveAt(randomIndex);

        onlineReserveCount = reserve.Count;
        consecutivePasses = 0;

        photonGameSyncManager.BroadcastInitialTile(firstTile);

        photonGameSyncManager.BroadcastReserveCount(reserve.Count);

        currentPlayerIndex = 0;
        selectedTile = null;

        UpdatePlacementButtons();

        photonGameSyncManager.DistributePrivateHands();

        ShowFeedback("Partida online preparada. Ficha inicial sincronizada.");

        Debug.Log("El anfitrión preparó la partida online correctamente.");
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

        if (GameSettings.SelectedMode == GameMode.Online)
        {
            if (photonGameSyncManager == null)
            {
                Debug.LogError("No se ha asignado PhotonGameSyncManager.");
                return false;
            }

            photonGameSyncManager.RequestOnlineMove(tile, side);

            ShowFeedback("Enviando jugada al anfitrión...");

            selectedTile = null;
            UpdatePlacementButtons();

            return true;
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

        gameEvents.NotifyTilePlaced(tile, currentPlayerIndex, side);

        selectedTile = null;

        UpdatePlacementButtons();

        AdvanceTurn();

        return true;
    }

    public void DrawUntilPlayable()
    {
        if (CurrentPlayer == null)
            return;

        if (GameSettings.SelectedMode == GameMode.Online)
        {
            if (photonGameSyncManager == null)
            {
                Debug.LogError("No se ha asignado PhotonGameSyncManager.");
                return;
            }

            photonGameSyncManager.RequestOnlineDraw();

            ShowFeedback("Solicitando fichas al anfitrión...");
            return;
        }

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

        if (drawnTiles > 0)
        {
            gameEvents.NotifyTilesDrawn(currentPlayerIndex, drawnTiles);
        }

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

    public void PassTurn()
    {
        if (CurrentPlayer == null)
            return;

        if (GameSettings.SelectedMode == GameMode.Online)
        {
            if (photonGameSyncManager == null)
            {
                Debug.LogError("No se ha asignado PhotonGameSyncManager.");
                return;
            }

            photonGameSyncManager.RequestOnlinePass();

            ShowFeedback("Solicitando pasar el turno...");
            return;
        }

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

        gameEvents.NotifyPlayerPassed(
            currentPlayerIndex,
            CurrentPlayer.playerName
        );

        selectedTile = null;

        UpdatePlacementButtons();

        AdvanceTurn();
    }

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

    private void UpdatePlacementButtons()
    {
        if (placeLeftButton != null)
            placeLeftButton.interactable = false;

        if (placeRightButton != null)
            placeRightButton.interactable = false;
    }

    private void UpdateTurnUI()
    {
        if (CurrentPlayer == null)
            return;

        PlayerHand handToShow = CurrentPlayer.hand;

        // En online, cada cliente muestra únicamente su propia mano.
        if (GameSettings.SelectedMode == GameMode.Online &&
            photonGameSyncManager != null)
        {
            int localIndex = photonGameSyncManager.LocalPlayerIndex;

            if (localIndex >= 0 && localIndex < players.Count)
            {
                handToShow = players[localIndex].hand;
            }
        }

        gameEvents.NotifyTurnChanged(
            currentPlayerIndex,
            CurrentPlayer.playerName,
            handToShow
        );

        gameEvents.NotifyReserveChanged(ReserveCount);
    }

    public void OnOnlineHandReceived(int playerIndex)
    {
        players = gameSetup.Players;
        reserve = gameSetup.Reserve;

        if (playerIndex < 0 || playerIndex >= players.Count)
        {
            Debug.LogError("Índice de jugador recibido inválido.");
            return;
        }

        currentPlayerIndex = 0;

        UpdateTurnUI();

        Debug.Log(
            "Mano online recibida correctamente para: " +
            players[playerIndex].playerName
        );

        ShowFeedback("Tus fichas fueron recibidas correctamente.");
    }


    public void ValidateOnlineMoveRequest(
        int actorNumber,
        int tileID,
        int sideValue)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (sideValue != (int)DominoPlacementSide.Left &&
            sideValue != (int)DominoPlacementSide.Right)
        {
            return;
        }

        Player[] roomPlayers = PhotonNetwork.PlayerList;
        int actorIndex = -1;

        for (int i = 0; i < roomPlayers.Length; i++)
        {
            if (roomPlayers[i].ActorNumber == actorNumber)
            {
                actorIndex = i;
                break;
            }
        }

        if (actorIndex == -1)
        {
            Debug.LogWarning("No se encontró al jugador que solicitó la jugada.");
            return;
        }

        if (actorIndex != currentPlayerIndex)
        {
            Debug.LogWarning("El jugador intentó jugar fuera de su turno.");
            return;
        }

        PlayerData requestingPlayer = players[actorIndex];

        DominoTile tile = requestingPlayer.hand.Tiles.Find(
            t => t.tileID == tileID
        );

        if (tile == null)
        {
            Debug.LogWarning("El jugador intentó utilizar una ficha que no posee.");
            return;
        }

        DominoPlacementSide side = (DominoPlacementSide)sideValue;

        if (!CanPlaceOnSide(tile, side))
        {
            Debug.LogWarning("La ficha no es compatible con el extremo seleccionado.");
            return;
        }

        photonGameSyncManager.BroadcastAcceptedMove(
            tile,
            side,
            actorIndex
        );
    }


    public void ValidateOnlineDrawRequest(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Player[] roomPlayers = PhotonNetwork.PlayerList;
        int actorIndex = -1;

        for (int i = 0; i < roomPlayers.Length; i++)
        {
            if (roomPlayers[i].ActorNumber == actorNumber)
            {
                actorIndex = i;
                break;
            }
        }

        if (actorIndex == -1 || actorIndex != currentPlayerIndex)
        {
            Debug.LogWarning("Solicitud de robo fuera de turno.");
            return;
        }

        if (reserve == null || reserve.Count == 0)
        {
            ShowFeedback("La reserva está vacía. Puedes pasar.");
            return;
        }

        if (CanCurrentPlayerPlay())
        {
            ShowFeedback("El jugador ya tiene una ficha que puede colocar.");
            return;
        }

        List<DominoTile> drawnTiles = new List<DominoTile>();

        while (reserve.Count > 0 && !CanCurrentPlayerPlay())
        {
            int lastIndex = reserve.Count - 1;

            DominoTile drawnTile = reserve[lastIndex];

            reserve.RemoveAt(lastIndex);

            CurrentPlayer.hand.AddTile(drawnTile);
            drawnTiles.Add(drawnTile);

            if (CanPlayTile(drawnTile))
                break;
        }

        if (drawnTiles.Count == 0)
            return;

        onlineReserveCount = reserve.Count;

        photonGameSyncManager.SendDrawnTiles(
            actorIndex,
            drawnTiles,
            reserve.Count
        );

        UpdateTurnUI();

        Debug.Log(
            "El jugador " + actorIndex +
            " robó " + drawnTiles.Count + " ficha(s)."
        );
    }


    public void ApplyOnlineDrawnTiles(
        int playerIndex,
        List<DominoTile> drawnTiles,
        int remainingReserve)
    {
        if (players == null ||
            playerIndex < 0 ||
            playerIndex >= players.Count)
        {
            Debug.LogError("Jugador inválido al recibir fichas robadas.");
            return;
        }

        if (drawnTiles == null)
            return;

        onlineReserveCount = remainingReserve;
        gameEvents.NotifyReserveChanged(remainingReserve);

        if (!PhotonNetwork.IsMasterClient &&
            photonGameSyncManager.LocalPlayerIndex == playerIndex)
        {
            foreach (DominoTile tile in drawnTiles)
            {
                players[playerIndex].hand.AddTile(tile);
            }

            UpdateTurnUI();
            gameEvents.NotifyTilesDrawn(playerIndex, drawnTiles.Count);

            ShowFeedback(
                "Robaste " + drawnTiles.Count +
                " ficha(s). Revisa tu mano."
            );
        }

        Debug.Log("Sincronización del robo completada.");
    }

    public void UpdateOnlineReserveCount(int count)
    {
        onlineReserveCount = count;
        gameEvents.NotifyReserveChanged(count);
        UpdateTurnUI();
    }


    public void ValidateOnlinePassRequest(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Player[] roomPlayers = PhotonNetwork.PlayerList;
        int actorIndex = -1;

        for (int i = 0; i < roomPlayers.Length; i++)
        {
            if (roomPlayers[i].ActorNumber == actorNumber)
            {
                actorIndex = i;
                break;
            }
        }

        if (actorIndex == -1 || actorIndex != currentPlayerIndex)
        {
            Debug.LogWarning("Solicitud de pase fuera de turno.");
            return;
        }

        if (reserve == null || reserve.Count > 0)
        {
            ShowFeedback("Todavía quedan fichas en la reserva.");
            return;
        }

        if (CanCurrentPlayerPlay())
        {
            ShowFeedback("Tienes una jugada disponible. No puedes pasar.");
            return;
        }

        consecutivePasses++;

        photonGameSyncManager.BroadcastAcceptedPass(
            actorIndex,
            consecutivePasses
        );
    }


    public void ApplySynchronizedOnlinePass(
    int playerIndex,
    int synchronizedPassCount)
    {
        if (players == null ||
            playerIndex < 0 ||
            playerIndex >= players.Count)
        {
            Debug.LogError("Jugador inválido al sincronizar el pase.");
            return;
        }

        if (playerIndex != currentPlayerIndex)
        {
            Debug.LogWarning("El pase recibido no corresponde al turno actual.");
            return;
        }

        consecutivePasses = synchronizedPassCount;

        gameEvents.NotifyPlayerPassed(
            playerIndex,
            players[playerIndex].playerName
        );

        Debug.Log(
            players[playerIndex].playerName + " pasó su turno."
        );

        selectedTile = null;

        UpdatePlacementButtons();

        AdvanceTurn();

        if (consecutivePasses >= players.Count)
        {
            gameEvents.NotifyGameBlocked();

            ShowFeedback(
                "¡Partida bloqueada! Todos los jugadores han pasado."
            );

            Debug.Log("La partida online está bloqueada.");
        }
        else
        {
            ShowFeedback("Turno pasado correctamente.");
        }
    }


    public void ApplySynchronizedOnlineMove(
        int playerIndex,
        DominoTile tile,
        DominoPlacementSide side)
    {
        if (players == null ||
            playerIndex < 0 ||
            playerIndex >= players.Count)
        {
            Debug.LogError("Jugador inválido al sincronizar la jugada.");
            return;
        }

        if (playerIndex != currentPlayerIndex)
        {
            Debug.LogWarning("La jugada recibida no corresponde al turno actual.");
            return;
        }

        DominoBoardMoveResult result = boardManager.TryPlaceTile(tile, side);

        if (result == null || !result.IsValid)
        {
            Debug.LogError("No se pudo sincronizar la jugada: " +
                        (result != null ? result.Explanation : "Resultado nulo."));
            return;
        }

        PlayerHand actorHand = players[playerIndex].hand;

        DominoTile ownedTile = actorHand.Tiles.Find(
            t => t.tileID == tile.tileID
        );

        if (ownedTile != null)
        {
            actorHand.RemoveTile(ownedTile);
        }

        gameEvents.NotifyTilePlaced(tile, playerIndex, side);

        consecutivePasses = 0;

        selectedTile = null;

        Debug.Log(
            players[playerIndex].playerName +
            " colocó una ficha en " + side
        );

        currentPlayerIndex = playerIndex;
        AdvanceTurn();
    }

    public void OnDrawButtonClicked()
    {
        DrawUntilPlayable();
    }

    public void OnPassButtonClicked()
    {
        PassTurn();
    }

    public void OnRestartButtonClicked()
    {
        InitializeTurnSystem();

        ShowFeedback(
            "¡La partida ha comenzado nuevamente!"
        );
    }

    private void ShowFeedback(string message)
    {
        Debug.Log(message);
        gameEvents.NotifyFeedback(message);
    }

}