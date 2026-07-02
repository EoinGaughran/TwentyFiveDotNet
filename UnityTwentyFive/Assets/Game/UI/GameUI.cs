using System.Collections.Generic;
using System.Linq;
using Core.Models;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;

    private UIActionQueue _actionQueue;
    private HumanInputController _humanInputController;

    [SerializeField] private Main _main;
    [SerializeField] private PhaseUI _phaseUI;
    [SerializeField] private PlayerPanelUI _playerPanelUI;
    [SerializeField] private TablePanelUI _tablePanelUI;
    [SerializeField] private ConsoleLogUI _consoleLogUI;
    [SerializeField] private CardUIFactory _cardUIFactory;
    [SerializeField] private AnnouncementUI _announcementUI;

    public void Init(GameManager manager)
    {
        _manager = manager;
        _actionQueue = gameObject.AddComponent<UIActionQueue>();
        _humanInputController = new(
            _manager,
            _playerPanelUI,
            _tablePanelUI,
            _announcementUI,
            _consoleLogUI
            );

        _playerPanelUI.OnNotification += HandleNotification;

        _manager.OnStateSnapshot += HandleStateSnapshot;
        _manager.OnDealingCompleted += HandleDealCompleted;
        _manager.OnTrumpResolved += HandleTrumpResolved;
        _manager.OnRolesSelected += HandleRolesSelected;
        _manager.OnCardDiscarded += HandleCardDiscarded;
        _manager.OnPlayerSteal += HandlePlayerSteal;
        _manager.OnPlayerTurnStarted += HandlePlayerTurnStarted;
        _manager.OnCardPlayed += HandleCardPlayed;
        _manager.OnPlayerInputRequest += HandlePlayerInput;
        _manager.OnScoreChanged += HandleScoreChanged;
        _manager.OnTrickNewWinner += HandleTrickNewWinner;
        _manager.OnTrickScored += HandleTrickScored;
        _manager.OnNewTrick += HandleNewTrick;
        _manager.OnHandEnd += HandleHandEnd;
        _manager.OnGamePhaseChange += HandleGamePhaseChange;
        _manager.OnGameOver += HandleGameOver;
        _manager.OnNewGame += HandleNewGame;
        _manager.OnProgramClosed += HandleProgramClosed;
    }

    public void HandleNotification(string message)
    {
        _announcementUI.Show(message);
    }

    public bool PlayAgain() => false;

    private void HandleStateSnapshot(GameState gS)
    {
        var gamePhase = gS.CurrentPhase.ToString();
        var players = gS.Players.ToList();

        _actionQueue.EnqueueUI(0f, () =>
        {
            _phaseUI.Render(gamePhase);
            _playerPanelUI.RenderPlayers(players);

            _announcementUI.FadeOut();
        });
    }

    private void HandleDealCompleted(GameState gameState)
    {
        var players = gameState.Players.ToList();

        var dealtHandsSnapshot = gameState.Players
            .Select(player => new DealtPlayerSnapshot
            {
                PlayerId = player.Id,
                Cards = player.Hand.ToList()
            })
            .ToList();

        var deckCount = gameState.Deck.Cards.Count;
        var deckTransform = _tablePanelUI.GetDeckSlot();
        var dealtCardUIs = new List<DealtPlayerCardUISnapshot>();

        _actionQueue.EnqueueUI(1f, () =>
        {
            _playerPanelUI.RenderPlayers(players);

            foreach (var player in dealtHandsSnapshot)
            {
                PlayerUI playerUI = _playerPanelUI.GetPlayerUI(player.PlayerId);

                bool isHuman = _playerPanelUI.Human != null &&
                    player.PlayerId == _playerPanelUI.Human.Id;

                var cardUIs = new List<CardUI>();

                foreach (var card in player.Cards)
                {
                    CardUI cardUI = _cardUIFactory.CreateHandCardUI(
                        card,
                        !isHuman,
                        playerUI.HandParent
                    );

                    cardUIs.Add( cardUI );

                    cardUI.SetHidden( true );
                    playerUI.SetUpCardInHand(cardUI);
                    cardUI.SetupRect();
                }

                dealtCardUIs.Add(new DealtPlayerCardUISnapshot
                {
                    PlayerId = player.PlayerId,
                    CardUIs = cardUIs,
                });
            }

            if (!_tablePanelUI.RenderDeckCount(deckCount))
                _consoleLogUI.AppendText("RenderDeckCount failed.");
        });

        _actionQueue.EnqueueUI(0f, () =>
            _playerPanelUI.AnimateDealToPlayers(
                dealtCardUIs,
                deckTransform.position,
                0,
                1
        ));
    }

    private void HandleTrumpResolved(TrumpData trumpData, Player player, Deck deck)
    {
        var playerName = player.Name;
        var trumpCard = trumpData._trumpCard;
        var deckCardCount = deck.Cards.Count;
        var deckName = deck.ToString();
        var deckTransform = _tablePanelUI.GetDeckSlot();

        _actionQueue.EnqueueUI(5.0f, () =>
        {

            CardUI cardUI = _cardUIFactory.CreateHandCardUI(trumpCard, false, deckTransform);

            _consoleLogUI.AppendText($"Dealer {playerName} flipped the trump card. It's the {trumpCard}.");

            _tablePanelUI.DrawCardUIFromDeckUI(cardUI);
            _tablePanelUI.RegisterStatusCardUI(cardUI, StatusCardType.TrumpCard);
            _tablePanelUI.RenderDeckCount(deckCardCount);
        });

        _actionQueue.EnqueueUI(3.0f, () =>
        {
            _tablePanelUI.TryGetStatusCardUI(StatusCardType.TrumpCard, out var cardUI);
            _tablePanelUI.MoveCardToStatusSlot(cardUI, StatusCardType.TrumpCard);

            _consoleLogUI.AppendText($"{deckCardCount} cards remain in {deckName}.");
        });
    }

    private void HandleRolesSelected(Player dealer, Player leader)
    {
        var dealerName = dealer.Name;
        var leaderName = leader.Name;

        _actionQueue.EnqueueUI(1f, () =>
        {
            _consoleLogUI.AppendText($"{dealerName} has been selected as the dealer." +
                $"\n{leaderName} is leading the trick.");

            _announcementUI.Show($"{dealerName} is dealing");
        });
    }

    private void HandleCardDiscarded(Card discardedCard, Player cardPlayer)
    {
        var player = cardPlayer;
        var cardPlayerName = cardPlayer.Name;
        var cardPlayerID = cardPlayer.Id;
        var discardedCardValue = discardedCard.ToString();
        var discardedCardID = discardedCard.Id;

        if (player is PlayerHuman)
        {
            _actionQueue.EnqueueUI(2.0f, () =>
            {
                if (_cardUIFactory.TryGetCardUI(discardedCardID, out var cardUI))
                {
                    cardUI.FlipCard();
                }
                else
                {
                    Debug.LogError($"No CardUI found for card ID {discardedCardID} belonging to {cardPlayerName}");
                    return;
                }
            });
        }

        _actionQueue.EnqueueUI(2.0f, () =>
        {
            if (_cardUIFactory.TryGetCardUI(discardedCardID, out var playedCardUI))
            {
                if (_playerPanelUI.MoveCardToPlayedCards(cardPlayerID, playedCardUI))
                {
                    _consoleLogUI.AppendText($"{cardPlayerName} discarded {discardedCardValue}.");
                }
                else
                    Debug.LogError($"{cardPlayerName} failed to discard {discardedCardValue}.");
            }
            else
                Debug.LogError($"TryGetCardUI failed to retrieve playedCardUI from cardPlayerID:{cardPlayerID}");
        });
    }

    private void HandlePlayerSteal(Card trumpCard, Player stealingPlayer)
    {
        var player = stealingPlayer;
        var stealingPlayerName = stealingPlayer.Name;
        var stealingPlayerID = stealingPlayer.Id;
        var trumpCardName = trumpCard.ToString();
        var trumpCardID = trumpCard.Id;
        var trumpTransform = _tablePanelUI.GetStatusCardTransform(StatusCardType.TrumpCard);

        _actionQueue.EnqueueUI(2.0f, () =>
        {
            if(_tablePanelUI.TryGetStatusCardUI(StatusCardType.TrumpCard, out var trumpCardUI))
            {
                _announcementUI.Show($"{stealingPlayerName} stole the Trump Card");

                _playerPanelUI.AddCardToPlayerHand(stealingPlayerID, trumpCardUI);
                _consoleLogUI.AppendText($"{stealingPlayerName} stole {trumpCardName}.");

                CardUI cardUI = _cardUIFactory.CreateAnimationCardUI(trumpCard, false, trumpTransform);
                cardUI.SetTransparentStyle();
                cardUI.SetupRect();
                //_tablePanelUI.AddCardToStatusSlot(cardUI, StatusCardType.TrumpCard);

            }
            else
            {
                Debug.LogError($"No CardUI found for card ID {trumpCardID}");
            }
        });

        if (player is PlayerCPU)
        {
            _actionQueue.EnqueueUI(2.0f, () =>
            {
                if (_cardUIFactory.TryGetCardUI(trumpCardID, out var playedCardUI))
                {
                    playedCardUI.FlipCard();
                }
                else
                {
                    Debug.LogError($"No CardUI found for card ID {trumpCardID} belonging to {stealingPlayerName}");
                    return;
                }
            });
        }
    }

    private void HandlePlayerTurnStarted(Player player)
    {
        var playerName = player.Name;

        _actionQueue.EnqueueUI(0f, () =>
        {
            _consoleLogUI.AppendText($"It's {playerName}'s turn.");
        });
    }

    private void HandleCardPlayed(CardPlayedEvent e)
    {
        var player = e.Player;
        var playerName = e.Player.Name;
        var playerID = e.Player.Id;
        var playedCardName = e.PlayedCard.ToString();
        var playedCardID = e.PlayedCard.Id;
        var ledSuit = e.PlayedCard.GetSuitSymbolUnicoded();
        var isLeader = e.IsLeader;
        var playedCard = e.PlayedCard;
        var ledCardTransform = _tablePanelUI.GetStatusCardTransform(StatusCardType.LedCard);

        if (player is PlayerCPU)
        {
            _actionQueue.EnqueueUI(2.0f, () =>
            {
                if (_cardUIFactory.TryGetCardUI(playedCardID, out var playedCardUI))
                {
                    playedCardUI.FlipCard();
                }
                else
                {
                    Debug.LogError($"No CardUI found for card ID {playedCardID} belonging to {playerName}");
                    return;
                }
            });
        }

        _actionQueue.EnqueueUI(2.0f, () =>
        {
            if (!_cardUIFactory.TryGetCardUI(playedCardID, out var playedCardUI))
            {
                Debug.LogError($"No CardUI found for card ID {playedCardID} belonging to {playerName}");
                return;
            }

            if (isLeader)
            {
                _consoleLogUI.AppendText($"{playerName} led with the {playedCardName}." +
                    $"\n Suit {ledSuit} is leading.");

                CardUI statusCardUI = _cardUIFactory.CreateAnimationCardUI(playedCard, false, ledCardTransform);

                _tablePanelUI.AddCardToStatusSlot(statusCardUI, StatusCardType.LedCard);
            }
            else
            {
                _consoleLogUI.AppendText($"{playerName} played {playedCard}");
            }
            
            if(!_playerPanelUI.MoveCardToPlayedCards(playerID, playedCardUI))
                Debug.LogError($"Failed to move cardID {playedCardID} belonging to {playerName} to the played card area.");
        });
    }

    private void HandlePlayerInput(
    Player p,
    PlayerDecisionType dT,
    IReadOnlyList<Card> o)
    {
        var playerName = p.Name;
        var decisionType = dT;
        var optionIds = o?.Select(c => c.Id).ToList();

        _actionQueue.EnqueueUI(0f, () =>
        {
            _humanInputController.BeginDecision(
                playerName,
                decisionType,
                optionIds
            );
        });
    }

    public void SubmitSelectedCard()
    {
        _humanInputController.SubmitSelectedCard();
    }

    public void SubmitFlipTrump()
    {
        _humanInputController.SubmitFlipTrump();
    }

    //Scoring
    private void HandleScoreChanged(IReadOnlyList<Player> players)
    {
        var scoreData = players
        .Select(p => new PlayerScoreViewData(
            p.Id,
            p.Name,
            p.Points))
        .ToList();

        _actionQueue.EnqueueUI(2f, () =>
        {
            _playerPanelUI.PrintPlayersScores(scoreData);
        });
        
    }

    private void HandleTrickNewWinner(Card wC, Player p, bool isD)
    {
        var playedCard = wC;
        var playerName = p.Name;
        var winningCardName = wC.ToString();
        var winningCardID = wC.Id;
        var isDealer = isD;
        var playerUI = _playerPanelUI.GetPlayerUI(p.Id);
        var winningCardTransform = _tablePanelUI.GetStatusCardTransform(StatusCardType.WinningCard);

        _actionQueue.EnqueueUI(2.0f, () =>
        {
            if (isDealer)
            {
                _consoleLogUI.AppendText($"{playerName} got their dealer's trick.");
                _announcementUI.Show($"{playerName} got their dealer's trick.");
            }

            CardUI winningCardUI = _cardUIFactory.CreateAnimationCardUI(playedCard, false, winningCardTransform);

            _consoleLogUI.AppendText($"{playerName} is currently winning with the {winningCardName}.");

            _tablePanelUI.DestroyStatusCard(StatusCardType.WinningCard);
            _tablePanelUI.AddCardToStatusSlot(winningCardUI, StatusCardType.WinningCard);
        });
    }

    private void HandleTrickScored(Card winningCard, Player player)
    {
        var playerName = player.Name;
        var winningCardName = winningCard.ToString();

        _actionQueue.EnqueueUI(2.0f, () =>
        {
            _consoleLogUI.AppendText($"{playerName} won with the {winningCardName}" +
            $"\n{playerName} has received the trick worth 5 points.");

            _announcementUI.Show($"{playerName} won the trick");
        });
        
    }

    private void HandleNewTrick(Player leader, int trickNumber)
    {
        var leaderName = leader.Name;

        _actionQueue.EnqueueUI(2.0f, () =>
        {
            _consoleLogUI.AppendText($"Trick {trickNumber} begins." +
            $"\n{leaderName} will lead the trick.");

            _announcementUI.Show($"Trick {trickNumber}: {leaderName} leads");

            _tablePanelUI.DestroyStatusCard(StatusCardType.WinningCard);
            _tablePanelUI.DestroyStatusCard(StatusCardType.LedCard);

        });
    }

    private void HandleHandEnd()
    {
        _actionQueue.EnqueueUI(5.0f, () =>
        {
            _announcementUI.Show("Dealing new hand");

            _cardUIFactory.ClearAllCardUIs();

            _tablePanelUI.DestroyStatusCard(StatusCardType.WinningCard);
            _tablePanelUI.DestroyStatusCard(StatusCardType.LedCard);
            _tablePanelUI.DestroyStatusCard(StatusCardType.TrumpCard);
        });
    }

    //Game State 
    private void HandleGamePhaseChange(GamePhase gamePhase)
    {
        var _gamePhase = gamePhase.ToString();

        _actionQueue.EnqueueUI(0f, () =>
        {
            _phaseUI.Render(_gamePhase);

            _consoleLogUI.AppendText($"GamePhase: {_gamePhase}");
        });
    }

    private void HandleGameOver(Player winner)
    {
        var playerName = winner.Name;

        _actionQueue.EnqueueUI(0f, () =>
        {
            _announcementUI.Show($"{playerName} wins the game!");

            _consoleLogUI.AppendText($"{playerName} wins!" +
                $"\nGame Over!");
        });
    }

    private void HandleNewGame()
    {
        _actionQueue.EnqueueUI(0f, () =>
        {
            _consoleLogUI.AppendText($"The deck has been removed." +
                $"\nPlayers hands have been cleared." +
                $"\nPlayers scores have been cleared." +
                $"\nThe list of played cards has been cleared.");
        });
    }

    private void HandleProgramClosed()
    {
        _actionQueue.EnqueueUI(0f, () =>
        {
            _consoleLogUI.AppendText($"The game has ended. The program will now close.");
        });
    }


    private void OnDestroy()
    {
        _playerPanelUI.OnNotification -= HandleNotification;

        if (_manager == null) return;

        _manager.OnStateSnapshot -= HandleStateSnapshot;
        _manager.OnDealingCompleted -= HandleDealCompleted;
        _manager.OnTrumpResolved -= HandleTrumpResolved;
        _manager.OnRolesSelected -= HandleRolesSelected;
        _manager.OnCardDiscarded -= HandleCardDiscarded;
        _manager.OnPlayerSteal -= HandlePlayerSteal;
        _manager.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
        _manager.OnCardPlayed -= HandleCardPlayed;
        _manager.OnPlayerInputRequest -= HandlePlayerInput;
        _manager.OnScoreChanged -= HandleScoreChanged;
        _manager.OnTrickNewWinner -= HandleTrickNewWinner;
        _manager.OnTrickScored -= HandleTrickScored;
        _manager.OnNewTrick -= HandleNewTrick;
        _manager.OnHandEnd -= HandleHandEnd;
        _manager.OnGamePhaseChange -= HandleGamePhaseChange;
        _manager.OnGameOver -= HandleGameOver;
        _manager.OnNewGame -= HandleNewGame;
        _manager.OnProgramClosed -= HandleProgramClosed;
    }
}