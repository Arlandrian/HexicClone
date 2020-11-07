using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private Text _movementText;
    [SerializeField]
    private Text _scoreText;

    [SerializeField]
    private GameObject _gameOverUI;
    [SerializeField]
    private Text _finalScoreText;
    [SerializeField]
    private Button _playAgainText;

    #endregion

    public void Init()
    {
        SetScoreText(0);
        SetMovementText(0);
        _playAgainText.onClick.AddListener(OnClickPlayAgain);
    }

    public void SetScoreText(int newScore)
    {
        _scoreText.text = $"Score: {newScore}";
    }

    public void SetMovementText(int newNumber)
    {
        _movementText.text = $"{newNumber}";
    }

    public void OnGameOver(int score)
    {
        _gameOverUI.SetActive(true);
        _finalScoreText.text = "Score: "+score.ToString();
    }

    private void OnClickPlayAgain()
    {
        SceneManager.LoadScene(0);
    }
}
