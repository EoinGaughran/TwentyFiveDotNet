using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class PlayerPanelUI : MonoBehaviour
{
    [SerializeField] private PlayerUI humanUI;
    [SerializeField] private Transform opponentContainer;
    [SerializeField] private GameObject opponentPrefab;

    private readonly List<PlayerUI> opponentUIs = new();

    public Player Human { get; private set; }
    public PlayerUI HumanUI => humanUI;

    public void RenderPlayers(IReadOnlyList<Player> players)
    {
        Human = players[0];
        humanUI.Bind(Human);

        foreach (Transform child in opponentContainer)
        {
            Destroy(child.gameObject);
        }

        opponentUIs.Clear();

        for (int i = 1; i < players.Count; i++)
        {
            GameObject opponentGO = Instantiate(opponentPrefab, opponentContainer);

            PlayerUI ui = opponentGO.GetComponent<PlayerUI>();
            ui.Bind(players[i]);

            opponentUIs.Add(ui);
        }
    }
}