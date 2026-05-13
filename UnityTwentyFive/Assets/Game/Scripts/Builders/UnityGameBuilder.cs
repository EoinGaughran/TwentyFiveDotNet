using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class UnityGameBuilder
{
    public static readonly string UIPrefix = "[UnityGameBuilder] ";
    public static GameState CreateMainGame(RulesEngine rulesEngine, GameConfig config)
    {
        GameState gameState = new();

        Debug.Log($"GameTitle: {config.GameTitle}");
        Debug.Log("Welcome to the card game 25.");
        Debug.Log($"The game is for {config.MinPlayers} - {config.MaxPlayers} players.");

        int totalPlayers = 4;
        int totalHumans = 1;
        
        gameState.AddPlayer(new PlayerHuman("Eoin"));

        for (int i = totalHumans; i < totalPlayers; i++)
        {
            gameState.AddPlayer(new PlayerCPU($"CPU Player {i + 1}", rulesEngine));
        }

        for (int i = 0; i < totalPlayers; i++)
        {
            gameState.Players[i].SetID(i);
        }

        gameState.Deck.Add52CardsToDeck();
        gameState.Deck.Shuffle();

        return gameState;
    }
}
