using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject _bombPrefab;

    #region Public Properties

    public int IndexX => _indexX;
    public int IndexY => _indexY;
    public HexType HexType => _type;
    public Transform TargetSocket => _targetSocket;

    public bool IsMoving => !_isExploded && !_isOnTarget;
    public bool IsBomb => _isBomb;

    #endregion

    #region Events

    public event Action HexExplodedEvent;

    #endregion

    #region Private Fields

    private int _indexX;
    private int _indexY;
    private HexType _type;

    private Transform _targetSocket;
    private bool _isExploded = false;
    private bool _isOnTarget = false;

    private BombBehaviour _bomb;
    private bool _isBomb = false;
    private int _bombRemainingMovement;

    private float _speed = 0.0f;
    private float _gravity;
    #endregion

    private void Awake()
    {
        _gravity = Physics2D.gravity.y;
    }

    public void Init(Transform socket, int x, int y, HexType type)
    {
        _targetSocket = socket;
        _indexX = x;
        _indexY = y;
        _type = type;
    }

    public void InitBomb()
    {
        _isBomb = true;
        _bombRemainingMovement = GlobalConfigs.BombMovementLimit;
        GameObject go = Instantiate(_bombPrefab, transform.position, transform.rotation, transform);
        _bomb = go.GetComponent<BombBehaviour>();
        _bomb.Init();
    }
    
    void Update()
    {
        if (!_isOnTarget)
        {
            if(transform.position.y > _targetSocket.position.y)
            {
                transform.Translate(Vector3.up * _speed * Time.deltaTime, Space.World);
            }
            else
            {
                transform.position = _targetSocket.position;
                _speed = 0;
                _isOnTarget = true;
                OnFallEnd();
            }
        }
    }

    private void OnFallEnd()
    {
        //throw new NotImplementedException();
    }

    private void FixedUpdate()
    {
        if (!_isOnTarget)
        {
            _speed += _gravity*Time.fixedDeltaTime;
        }
    }

    public void SetPosition(int y, Transform newTartget)
    {
        if(y != _indexY)
        {
            _indexY = y;
            _targetSocket = newTartget;
            _isOnTarget = false;
        }
    }

    public void SetIndex(int x, int y, Transform newTartget)
    {
        _indexX = x;
        _indexY = y;
        _targetSocket = newTartget;
    }

    public void Explode()
    {
        _isExploded = false;
        GetComponent<ParticleSystem>().Play();
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        Destroy(gameObject, 3f);
        HexExplodedEvent?.Invoke();
    }

    public bool IsSameType(HexBehaviour other)
    {
        return this._type.Id == other.HexType.Id;
    }

    public void OnMovementIncremented()
    {
        if (_bomb)
        {
            _bomb.OnMovementIncremented();
        }
    }
}
