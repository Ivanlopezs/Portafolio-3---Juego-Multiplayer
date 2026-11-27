using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLaunchController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    public void StartLocalGame()
    {
        if (GameSettings.SelectedMode != GameMode.Local)
        {
            Debug.LogWarning("La modalidad seleccionada no es local.");
            return;
        }

        Debug.Log("Iniciando partida local.");
        Debug.Log("Jugadores: " + GameSettings.PlayerCount);

        SceneManager.LoadScene(gameSceneName);
    }

    public void StartOnlineGame()
    {
        if (GameSettings.SelectedMode != GameMode.Online)
        {
            Debug.LogWarning("La modalidad seleccionada no es en línea.");
            return;
        }

        Debug.Log("La partida en línea requiere una sala.");
    }
}
