using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class GameState : NetworkBehaviour
{
    static private Dictionary<ulong, uint> scoreboard = new Dictionary<ulong, uint>();
    public static GameState instance { get; private set; }
    string scoreboardText;
    uint maxScore = 3;
    int chatXPos;
    string highscoreText;
    [SerializeField] Transform[] respawnPoints;
    List<Transform> playerList = new List<Transform>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        chatXPos = Screen.width / 2;
        highscoreText = $"First to {maxScore}, wins!";
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(chatXPos, 20, 400, 100), highscoreText);
    }

    [Rpc(SendTo.Server)]
    public void ScoreRPC(ulong id)
    {
        if (IsServer)
        {
            if (scoreboard.ContainsKey(id))
            {
                ++scoreboard[id];
            }
            else
            {
                scoreboard.Add(id, 1);
            }
        }

        ulong leadingPlayerID = 0;
        uint highestScore = 0;
        foreach (var score in scoreboard)
        {
            if (score.Value > highestScore)
            {
                highestScore= score.Value;
                leadingPlayerID = id;
            }
        }


        if (IsServer && highestScore >= maxScore)
        {
            UpdateScoreTextRPC($"Player {leadingPlayerID} WIN! Beginning new round...");
            ResetGame();
        }
        else
        {
            UpdateScoreTextRPC($"#1 = Player {leadingPlayerID}. {highestScore}/{maxScore}");
        }
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateScoreTextRPC(string newScoreText)
    {
        highscoreText = newScoreText;
    }

    private void ResetGame()
    {
        foreach (var player in playerList)
        {
            scoreboard.Clear();
            RespawnPlayer(player);
        }
    }

    public void RespawnPlayer(Transform player)
    {
        if (IsServer)
        {
            player.GetComponent<Health>().Reset();

            uint index = (uint)UnityEngine.Random.Range(0, respawnPoints.Length - 1);
            player.transform.position = respawnPoints[index].transform.position;
            player.transform.rotation = respawnPoints[index].transform.rotation;
        }
    }

    public void AddPlayer(Transform player)
    {
        if (playerList.Contains(player) == false)
        {
            playerList.Add(player);
        }
    }
}
