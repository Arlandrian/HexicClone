using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBehaviour : MonoBehaviour
{
    [SerializeField]
    private TextMesh _text;

    private int _remainingMovement;

    public void Init()
    {
        _remainingMovement = GlobalConfigs.BombMovementLimit;
        _text.text = _remainingMovement.ToString();
    }

    public void OnMovementIncremented()
    {
        _remainingMovement--;
        _text.text = _remainingMovement.ToString();
        if(_remainingMovement == 0)
        {
            GameManager.Instance.GameOver();
        }
    }
}
