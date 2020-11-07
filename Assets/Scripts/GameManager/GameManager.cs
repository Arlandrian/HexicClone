using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    #region Serialized Fields

    [SerializeField]
    private int _scoreForSpawningBomb = 1000;

    #endregion

    #region Events

    public event Action SpawnBombEvent;
    public event Action GameOverEvent;

    #endregion

    #region Private Fields

    private UIManager _UIManager;
    private BoardController _boardController;

    private int _movement = 0;
    private int _score = 0;
    private int _lastBombSpawnedScore = 0;

    #endregion

    void Start()
    {
        _scoreForSpawningBomb = GlobalConfigs.ScoreForSpawningBomb;
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

        if(_score > _lastBombSpawnedScore + _scoreForSpawningBomb)
        {
            _lastBombSpawnedScore = _score + _scoreForSpawningBomb;
            SpawnBombEvent?.Invoke();
        }
    }

    private void IncrementMovement()
    {
        _movement++;
        _UIManager.SetMovementText(_movement);
    }

    public void GameOver()
    {
        GameOverEvent?.Invoke();
    }

    public void ShowGameOver()
    {
        _UIManager.OnGameOver(_score);
    }

}
