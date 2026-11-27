using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PhotonGameSyncManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] private DominoGameSetup gameSetup;
    [SerializeField] private DominoTurnManager turnManager;
    [SerializeField] private DominoBoardManager boardManager;

    private PhotonView syncView;

    private bool initialBoardReceived;

    public int LocalPlayerIndex { get; private set; } = -1;

    private void Awake()
    {
        syncView = GetComponent<PhotonView>();
    }

    public void DistributePrivateHands()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("No estás dentro de una sala online.");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Solo el anfitrión puede repartir las fichas.");
            return;
        }

        Player[] roomPlayers = PhotonNetwork.PlayerList;

        if (gameSetup.Players.Count != roomPlayers.Length)
        {
            Debug.LogError("La cantidad de jugadores preparados no coincide con la sala.");
            return;
        }

        for (int i = 0; i < roomPlayers.Length; i++)
        {
            Player targetPlayer = roomPlayers[i];

            PlayerHand hand = gameSetup.Players[i].hand;

            int count = hand.Tiles.Count;

            int[] tileIDs = new int[count];
            int[] leftOrganisms = new int[count];
            int[] rightOrganisms = new int[count];

            for (int j = 0; j < count; j++)
            {
                DominoTile tile = hand.Tiles[j];

                tileIDs[j] = tile.tileID;
                leftOrganisms[j] = (int)tile.leftOrganism;
                rightOrganisms[j] = (int)tile.rightOrganism;
            }

            syncView.RPC(
                nameof(ReceivePrivateHand),
                targetPlayer,
                i,
                tileIDs,
                leftOrganisms,
                rightOrganisms
            );
        }

        Debug.Log("Las manos fueron enviadas a sus respectivos jugadores.");
    }

    [PunRPC]
    private void ReceivePrivateHand(
        int playerIndex,
        int[] tileIDs,
        int[] leftOrganisms,
        int[] rightOrganisms,
        PhotonMessageInfo info)
    {
        if (info.Sender == null || !info.Sender.IsMasterClient)
        {
            Debug.LogWarning("Se rechazó una sincronización de mano no autorizada.");
            return;
        }

        if (tileIDs == null ||
            leftOrganisms == null ||
            rightOrganisms == null ||
            tileIDs.Length != leftOrganisms.Length ||
            tileIDs.Length != rightOrganisms.Length)
        {
            Debug.LogError("Los datos de las fichas recibidas no son válidos.");
            return;
        }

        if (gameSetup.Players.Count == 0)
        {
            gameSetup.PrepareOnlinePlayers(
                PhotonNetwork.CurrentRoom.MaxPlayers
            );
        }

        List<DominoTile> receivedTiles = new List<DominoTile>();

        for (int i = 0; i < tileIDs.Length; i++)
        {
            DominoTile tile = new DominoTile(
                tileIDs[i],
                (OrganismType)leftOrganisms[i],
                (OrganismType)rightOrganisms[i]
            );

            receivedTiles.Add(tile);
        }

        gameSetup.SetOnlinePlayerHand(playerIndex, receivedTiles);

        LocalPlayerIndex = playerIndex;

        if (turnManager != null)
        {
            turnManager.OnOnlineHandReceived(playerIndex);
        }

        Debug.Log("Tu índice de jugador online es: " + LocalPlayerIndex);
    }

    public void BroadcastInitialTile(DominoTile tile)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el anfitrión puede transmitir la ficha inicial.");
            return;
        }

        if (tile == null)
        {
            Debug.LogError("La ficha inicial no es válida.");
            return;
        }

        syncView.RPC(
            nameof(ReceiveInitialTile),
            RpcTarget.AllBuffered,
            tile.tileID,
            (int)tile.leftOrganism,
            (int)tile.rightOrganism
        );
    }

    [PunRPC]
    private void ReceiveInitialTile(
        int tileID,
        int leftOrganism,
        int rightOrganism,
        PhotonMessageInfo info)
    {
        if (info.Sender == null || !info.Sender.IsMasterClient)
        {
            Debug.LogWarning("Se rechazó una ficha inicial no autorizada.");
            return;
        }

        if (initialBoardReceived)
            return;

        if (boardManager == null)
        {
            Debug.LogError("No se ha asignado DominoBoardManager.");
            return;
        }

        boardManager.InitializeOnlineBoard();

        DominoTile firstTile = new DominoTile(
            tileID,
            (OrganismType)leftOrganism,
            (OrganismType)rightOrganism
        );

        DominoBoardMoveResult result = boardManager.TryPlaceTile(
            firstTile,
            DominoPlacementSide.Right
        );

        if (!result.IsValid)
        {
            Debug.LogError("No se pudo colocar la ficha inicial: " + result.Explanation);
            return;
        }

        initialBoardReceived = true;

        Debug.Log("Ficha inicial sincronizada: " +
                firstTile.leftOrganism + " - " +
                firstTile.rightOrganism);
    }

    public void RequestOnlineMove(DominoTile tile, DominoPlacementSide side)
    {
        if (!PhotonNetwork.InRoom || tile == null)
        {
            Debug.LogWarning("No se pudo enviar la solicitud de jugada.");
            return;
        }

        syncView.RPC(
            nameof(ReceiveMoveRequest),
            RpcTarget.MasterClient,
            tile.tileID,
            (int)side
        );
    }


    [PunRPC]
    private void ReceiveMoveRequest(
        int tileID,
        int sideValue,
        PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (info.Sender == null)
            return;

        if (turnManager == null)
        {
            Debug.LogError("No se ha asignado DominoTurnManager.");
            return;
        }

        turnManager.ValidateOnlineMoveRequest(
            info.Sender.ActorNumber,
            tileID,
            sideValue
        );
    }


    public void BroadcastAcceptedMove(
        DominoTile tile,
        DominoPlacementSide side,
        int playerIndex)
    {
        if (!PhotonNetwork.IsMasterClient || tile == null)
            return;

        syncView.RPC(
            nameof(ReceiveAcceptedMove),
            RpcTarget.AllBuffered,
            playerIndex,
            tile.tileID,
            (int)tile.leftOrganism,
            (int)tile.rightOrganism,
            (int)side
        );
    }


    [PunRPC]
    private void ReceiveAcceptedMove(
        int playerIndex,
        int tileID,
        int leftOrganism,
        int rightOrganism,
        int sideValue,
        PhotonMessageInfo info)
    {
        if (info.Sender == null || !info.Sender.IsMasterClient)
        {
            Debug.LogWarning("Se rechazó una jugada no autorizada.");
            return;
        }

        if (turnManager == null)
        {
            Debug.LogError("No se ha asignado DominoTurnManager.");
            return;
        }

        if (sideValue != (int)DominoPlacementSide.Left &&
            sideValue != (int)DominoPlacementSide.Right)
        {
            Debug.LogError("El extremo de colocación recibido no es válido.");
            return;
        }

        DominoTile tile = new DominoTile(
            tileID,
            (OrganismType)leftOrganism,
            (OrganismType)rightOrganism
        );

        turnManager.ApplySynchronizedOnlineMove(
            playerIndex,
            tile,
            (DominoPlacementSide)sideValue
        );
    }
    
}
