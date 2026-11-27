using UnityEngine;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Player Configuration")]
    [SerializeField] private TMP_Dropdown playerCountDropdown;

    [Header("Game Mode")]
    [SerializeField] private TMP_Dropdown gameModeDropdown;

    [Header("Menu Reference")]
    [SerializeField] private MainMenuController mainMenuController;

    private void Start()
    {
        ConfigureDropdowns();
        LoadCurrentSettings();
    }

    private void ConfigureDropdowns()
    {
        playerCountDropdown.ClearOptions();

        playerCountDropdown.AddOptions(
            new System.Collections.Generic.List<string>
            {
                "2 jugadores",
                "3 jugadores",
                "4 jugadores"
            }
        );

        gameModeDropdown.ClearOptions();

        gameModeDropdown.AddOptions(
            new System.Collections.Generic.List<string>
            {
                "Partida local",
                "Partida en línea"
            }
        );
    }

    private void LoadCurrentSettings()
    {
        playerCountDropdown.value = GameSettings.PlayerCount - 2;

        gameModeDropdown.value =
            GameSettings.SelectedMode == GameMode.Local ? 0 : 1;

        playerCountDropdown.RefreshShownValue();
        gameModeDropdown.RefreshShownValue();
    }

    public void SaveSettings()
    {
        GameSettings.PlayerCount =
            playerCountDropdown.value + 2;

        GameSettings.SelectedMode =
            gameModeDropdown.value == 0
            ? GameMode.Local
            : GameMode.Online;

        Debug.Log("Configuración guardada.");
        Debug.Log("Jugadores: " + GameSettings.PlayerCount);
        Debug.Log("Modalidad: " + GameSettings.SelectedMode);

        mainMenuController.ShowMainMenu();
    }
}
