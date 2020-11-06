using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private Text _movementText;
    [SerializeField]
    private Text _scoreText;

    #endregion

    public void Init()
    {
        SetScoreText(0);
        SetMovementText(0);
    }

    public void SetScoreText(int newScore)
    {
        _scoreText.text = $"Score: {newScore}";
    }

    public void SetMovementText(int newNumber)
    {
        _movementText.text = $"{newNumber}";
    }
}
