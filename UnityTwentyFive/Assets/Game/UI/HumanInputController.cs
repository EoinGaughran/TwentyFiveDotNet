using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Models;

public class HumanInputController
{
    private readonly GameManager _manager;
    private readonly PlayerPanelUI _playerPanelUI;
    private readonly TablePanelUI _tablePanelUI;
    private readonly AnnouncementUI _announcementUI;
    private readonly ConsoleLogUI _consoleLogUI;

    private PlayerDecisionType? currentDecisionType;
    private IReadOnlyList<int> currentOptions;

    public HumanInputController(
        GameManager gM,
        PlayerPanelUI pP,
        TablePanelUI tP,
        AnnouncementUI a,
        ConsoleLogUI cL)
    {
        _manager = gM;
        _playerPanelUI = pP;
        _tablePanelUI = tP;
        _announcementUI = a;
        _consoleLogUI = cL;
    }

    public void BeginDecision(
        string playerName,
        PlayerDecisionType decisionType,
        IReadOnlyList<int> optionIds)
    {
        _consoleLogUI.AppendText($"PlayerDecisionType: {decisionType}");

        currentDecisionType = decisionType;
        currentOptions = optionIds;
        string message;

        switch (decisionType)
        {
            case PlayerDecisionType.FlipTrump:
                message = $"{playerName}, please flip over the trump card.";
                _consoleLogUI.AppendText(message);
                _announcementUI.Show(message);
                _tablePanelUI.AllowTrumpFlip();
                break;

            case PlayerDecisionType.LeadCard:
                message = $"{playerName}, please lead a card.";
                _consoleLogUI.AppendText(message);
                _announcementUI.Show(message);
                _playerPanelUI.HumanUI.AllowCardPlay();
                break;

            case PlayerDecisionType.StealTrump:
                message = $"{playerName}, please discard a card to steal the trump card.";
                _consoleLogUI.AppendText(message);
                _announcementUI.Show(message);
                _playerPanelUI.HumanUI.AllowCardPlay();
                break;

            case PlayerDecisionType.PlayCard:
                if (optionIds == null)
                    throw new InvalidOperationException("Options was null");

                message = $"{playerName}, please play a card.";
                _consoleLogUI.AppendText(message);
                _announcementUI.Show(message);
                _playerPanelUI.HumanUI.ShowPlayableCards(optionIds);
                _playerPanelUI.HumanUI.AllowCardPlay();
                break;
        }
    }

    public void SubmitSelectedCard()
    {
        if (_playerPanelUI.HumanUI.CanHumanPlayCards())
        {
            var cardID = _playerPanelUI.HumanUI.GetSelectedCardID();

            if (cardID == null)
            {
                _consoleLogUI.AppendText("No card selected.");
                return;
            }

            if (currentOptions != null && !currentOptions.Contains(cardID.Value))
            {
                _consoleLogUI.AppendText("That card is not playable.");
                return;
            }

            _consoleLogUI.AppendText($"Selected card was {cardID}.");

            _manager.SubmitPlayerAction(cardID);

            currentDecisionType = null;
            currentOptions = null;

            _playerPanelUI.HumanUI.DisableCardPlay();
            _playerPanelUI.HumanUI.ResetPlayableCards();
        }
        else
        {
            _announcementUI.Show("You cannot play cards, its not your turn.");
            _consoleLogUI.AppendText("You cannot play cards, its not your turn.");
        }
    }

    public void SubmitFlipTrump()
    {
        if (currentDecisionType != PlayerDecisionType.FlipTrump)
        {
            _announcementUI.Show("It's not your turn.");
            _consoleLogUI.AppendText("Tried to flip trump outside FlipTrump decision.");
            return;
        }

        _manager.SubmitPlayerAction(null);

        _consoleLogUI.AppendText($"Trump Flip Submitted.");

        currentDecisionType = null;
        currentOptions = null;
    }
}