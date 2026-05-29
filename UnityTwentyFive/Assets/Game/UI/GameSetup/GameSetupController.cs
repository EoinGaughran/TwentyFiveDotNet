using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Models;
using UnityEngine.UI;

public class GameSetupController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown totalPlayersDropdown;
    [SerializeField] private TMP_InputField playerNameInput;

    public void StartGame()
    {
        int totalPlayers = int.Parse(totalPlayersDropdown.options[totalPlayersDropdown.value].text);
        
        string playerName = string.IsNullOrWhiteSpace(playerNameInput.text)
            ? "Human Player"
            : playerNameInput.text;

        PendingGameState state = new(
            playerName,
            totalPlayers
        );

        GameSessionData.pendingGameState = state;

        SceneManager.LoadScene("SampleScene");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}