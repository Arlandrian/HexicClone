using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    #region Events

    #endregion

    #region Private Fields

    private UIManager _UIManager;
    private BoardController _boardController;

    private int _movement = 0;
    private int _score = 0;

    #endregion

    void Start()
    {
        _UIManager = PrefabFactory.Instance.CreateUIManager();
        _boardController = PrefabFactory.Instance.CreateBoard();
        RegisterBoard();
    }

    void RegisterBoard()
    {
        _boardController.MovementEvent += IncrementMovement;
        _boardController.ScoreEvent += AddScore;
    }

    private void AddScore(int score)
    {
        _score += score;
        _UIManager.SetScoreText(_score);
    }

    private void IncrementMovement()
    {
        _movement++;
        _UIManager.SetMovementText(_movement);
    }

}
