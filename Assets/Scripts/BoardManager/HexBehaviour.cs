using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBehaviour : MonoBehaviour
{
    #region Public Properties

    public int IndexX => _indexX;
    public int IndexY => _indexY;
    public HexType HexType => _type;
    public Transform TargetSocket => _targetSocket;

    public bool IsMoving => !_isExploded && !_isOnTarget;

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

    private float _speed = 0.0f;
    private float _gravity;

    #endregion

    private void Awake()
    {
        _gravity = Physics2D.gravity.y;///10f;
    }

    public void Init(Transform socket, int x, int y, HexType type)
    {
        _targetSocket = socket;
        _indexX = x;
        _indexY = y;
        _type = type;
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

    public void Explode()
    {
        _isExploded = false;
        GetComponent<ParticleSystem>().Play();
        transform.GetChild(0).gameObject.SetActive(false);
        Destroy(gameObject, 3f);
        HexExplodedEvent?.Invoke();
    }

    public bool IsSameType(HexBehaviour other)
    {
        return this._type.Id == other.HexType.Id;
    }
}
