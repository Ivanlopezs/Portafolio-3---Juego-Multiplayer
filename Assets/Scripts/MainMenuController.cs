using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Menu Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject onlinePanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        rulesPanel.SetActive(false);
        creditsPanel.SetActive(false);
        onlinePanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
    }

    public void OpenRules()
    {
        HideAllPanels();
        rulesPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        HideAllPanels();
        creditsPanel.SetActive(true);
    }

    public void OpenOnlinePanel()
    {
        HideAllPanels();
        onlinePanel.SetActive(true);
    }

    public void OpenLobbyPanel()
    {
        HideAllPanels();
        lobbyPanel.SetActive(true);
    }

    
    public void OnPlayButtonClicked()
    {
        if (GameSettings.SelectedMode == GameMode.Local)
        {
            Debug.Log("Iniciando partida local con " +
                GameSettings.PlayerCount + " jugadores.");

            SceneManager.LoadScene(gameSceneName);
        }
        else if (GameSettings.SelectedMode == GameMode.Online)
        {
            Debug.Log("Abriendo menú de partidas en línea.");
            OpenOnlinePanel();
        }
    }


    public void ExitGame()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }

    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        rulesPanel.SetActive(false);
        creditsPanel.SetActive(false);
        onlinePanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }
}
