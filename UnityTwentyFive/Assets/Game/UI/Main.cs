using System;
using TwentyFiveDotNet.Core.Application;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Game.Builders;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private GameUI ui;

    private GameManager _manager;

    void Start()
    {
        Debug.Log("Main.Start() called");

        TextAsset json = Resources.Load<TextAsset>("GameConfig");
        GameConfig config = ConfigLoader.LoadJsonText(json.text);

        RuntimeSettings runtimeSettings = new()
        {
            GameMode = GameMode.Snapshot,
            HidePlayerHands = config.HidePlayerHands,
            DevMode = config.DevMode,
            Delay = config.DelayInMilliseconds
        };

        RulesEngine rules = new(config);

        GameState gameState = runtimeSettings.GameMode switch
        {
            GameMode.Main => TestGameBuilder.CreateBasicGame(rules), // temporary
            GameMode.Snapshot => TestGameBuilder.CreateBasicGame(rules),
            _ => throw new InvalidOperationException($"Unsupported GameMode: {runtimeSettings.GameMode}")
        };

        _manager = new GameManager(rules, gameState);

        if (ui == null)
        {
            Debug.LogError("GameUI not assigned");
            return;
        }

        ui.Init(_manager);

        GameApp.Start(_manager, ui);
    }

    void Update()
    {
        if (_manager != null)
        {
            GameApp.Tick(_manager);
        }
    }
}
