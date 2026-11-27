using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    [Header("Menu References")]
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Room UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text roomStatusText;

    [Header("Lobby UI")]
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text lobbyPlayersText;
    [SerializeField] private Button startGameButton;

    [Header("Room Settings")]
    [SerializeField] private int maxCreateAttempts = 5;

    private string generatedRoomCode;
    private int createAttempts;

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            UpdateStatus("Primero debes conectarte a Photon.");
            return;
        }

        createAttempts = 0;
        TryCreateRoom();
    }

    private void TryCreateRoom()
    {
        if (createAttempts >= maxCreateAttempts)
        {
            UpdateStatus("No se pudo crear una sala. Inténtalo nuevamente.");
            return;
        }

        createAttempts++;

        generatedRoomCode = "ECO-" + Random.Range(100000, 1000000);

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)GameSettings.PlayerCount,
            IsVisible = false,
            IsOpen = true
        };

        UpdateStatus("Creando sala...");

        PhotonNetwork.CreateRoom(
            generatedRoomCode,
            roomOptions,
            TypedLobby.Default
        );
    }

    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            UpdateStatus("Primero debes conectarte a Photon.");
            return;
        }

        if (roomCodeInput == null || string.IsNullOrWhiteSpace(roomCodeInput.text))
        {
            UpdateStatus("Introduce el código de la sala.");
            return;
        }

        string roomCode = roomCodeInput.text.Trim().ToUpper();

        UpdateStatus("Buscando sala...");

        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Sala creada: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Te has unido a la sala: " + PhotonNetwork.CurrentRoom.Name);

        if (mainMenuController != null)
            mainMenuController.OpenLobbyPanel();

        RefreshLobbyUI();
        UpdateStatus("Te has unido correctamente a la sala.");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Error al crear sala: " + message);

        if (createAttempts < maxCreateAttempts)
        {
            TryCreateRoom();
        }
        else
        {
            UpdateStatus("No se pudo crear la sala: " + message);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Error al unirse a la sala: " + message);
        UpdateStatus("No se pudo unir a la sala. Revisa el código.");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Nuevo jugador: " + newPlayer.NickName);
        RefreshLobbyUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Jugador salió: " + otherPlayer.NickName);
        RefreshLobbyUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        RefreshLobbyUI();
    }

    private void RefreshLobbyUI()
    {
        if (!PhotonNetwork.InRoom)
            return;

        Room currentRoom = PhotonNetwork.CurrentRoom;

        if (roomCodeText != null)
            roomCodeText.text = "Código de sala: " + currentRoom.Name;

        if (lobbyPlayersText != null)
        {
            StringBuilder playerList = new StringBuilder();

            playerList.AppendLine(
                "Jugadores: " + currentRoom.PlayerCount +
                " / " + currentRoom.MaxPlayers
            );

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                string hostLabel = player.IsMasterClient
                    ? " (Anfitrión)"
                    : "";

                playerList.AppendLine(
                    player.NickName + hostLabel
                );
            }

            lobbyPlayersText.text = playerList.ToString();
        }

        if (startGameButton != null)
        {
            startGameButton.interactable =
                PhotonNetwork.IsMasterClient &&
                currentRoom.PlayerCount == currentRoom.MaxPlayers;
        }
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            UpdateStatus("Saliendo de la sala...");
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Has salido de la sala.");

        if (mainMenuController != null)
            mainMenuController.ShowMainMenu();

        UpdateStatus("Has salido de la sala.");
    }

        
    public void HostStartGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            UpdateStatus("No estás dentro de una sala.");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            UpdateStatus("Solo el anfitrión puede iniciar la partida.");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount !=
            PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            UpdateStatus("La sala todavía no está completa.");
            return;
        }

        UpdateStatus("Iniciando partida...");

        Debug.Log("El anfitrión está iniciando la partida.");

        PhotonNetwork.LoadLevel("MainGame");
    }

    private void UpdateStatus(string message)
    {
        Debug.Log(message);

        if (roomStatusText != null)
            roomStatusText.text = message;
    }
}

