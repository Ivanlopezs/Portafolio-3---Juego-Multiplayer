using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonConnection : MonoBehaviourPunCallbacks
{
    [Header("Connection UI")]
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Connection Settings")]
    [SerializeField] private string gameVersion = "1.0";

    private bool connectionRequested;

    private void Awake()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            UpdateStatus("Conexión con Photon establecida.");
            return;
        }

        if (connectionRequested)
        {
            UpdateStatus("Conectando con Photon...");
            return;
        }

        connectionRequested = true;
        UpdateStatus("Conectando con Photon...");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        connectionRequested = false;

        UpdateStatus("¡Conexión exitosa con Photon!");

        Debug.Log("Conectado al servidor Master de Photon.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionRequested = false;

        UpdateStatus("Desconectado: " + cause);

        Debug.LogWarning("Desconexión de Photon: " + cause);
    }

    public override void OnConnected()
    {
        Debug.Log("Conexión inicial con Photon establecida.");
    }

    private void UpdateStatus(string message)
    {
        Debug.Log(message);

        if (connectionStatusText != null)
            connectionStatusText.text = message;
    }
}
