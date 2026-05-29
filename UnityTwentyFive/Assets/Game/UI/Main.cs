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
    [SerializeField] private RuntimeSettings runtimeSettings;

    private bool autoAdvance = true;

    private GameManager _manager;

    void Start()
    {
        Debug.Log("Main.Start() called");

        TextAsset json = Resources.Load<TextAsset>("GameConfig");
        GameConfig config = ConfigLoader.LoadJsonText(json.text);

        runtimeSettings = new()
        {
            GameMode = GameMode.Main,
            HidePlayerHands = config.HidePlayerHands,
            DevMode = config.DevMode,
            Delay = config.DelayInMilliseconds,
            TestStateMode = config.TestStateMode,
            SnapshotOnLaunch = config.SnapshotOnLaunch,
        };

        Debug.Log($"SnapshotOnLaunch: {runtimeSettings.SnapshotOnLaunch}");

        RulesEngine rules = new(config);

        GameState gameState;

        if (GameSessionData.pendingGameState != null)
        {
            Debug.Log("Loading GameState from setup scene.");

            PendingGameState p = GameSessionData.pendingGameState;

            gameState = UnityGameBuilder.CreateFromSetup(
                rules,
                config,
                p.TotalPlayers,
                p.PlayerName
                );

            GameSessionData.pendingGameState = null;
        }
        else
        {
            Debug.Log("No setup GameState found. Creating default test game.");

            gameState = runtimeSettings.GameMode switch
            {
                GameMode.Main => UnityGameBuilder.CreateMainGame(rules, config),
                GameMode.Snapshot => TestGameBuilder.CreateBasicGame(rules),
                _ => throw new InvalidOperationException($"Unsupported GameMode: {runtimeSettings.GameMode}")
            };
        }

        _manager = new GameManager(rules, gameState);

        if (ui == null)
        {
            Debug.LogError("GameUI not assigned");
            return;
        }

        ui.Init(_manager, runtimeSettings);

        if (runtimeSettings.SnapshotOnLaunch)
        {
            _manager.PublishState();
        }

        GameApp.Start(_manager, ui);
    }

    void Update()
    {
        if (_manager != null &&
            autoAdvance)
        {
            GameApp.Tick(_manager);
        }
    }

    public void StepButton()
    {
        GameApp.Tick(_manager);
    }
    public void SetAutoAdvance(bool value)
    {
        autoAdvance = value;
    }
}