using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotBehaviour : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private GameObject _visual;

    #endregion

    #region Events

    public event Action DotDeselectedEvent;

    #endregion

    #region Fields

    public bool IsSelected { get; private set; }
    public int DotIndexX { get; private set; }
    public int DotIndexY { get; private set; }

    private float _rotateSpeed = 5f;
    public bool IsRotating { get; private set; }
    private bool _rotatingClockwise;

    private Vector3 _startRotation;
    private Vector3 _endRotation;
    private float _rotationTimer = 0f;

    private Action OnRotationEndCallback;
    #endregion

    public void Init(int x, int y)
    {
        DotIndexX = x;
        DotIndexY = y;
        IsSelected = false;
    }

    private void Update()
    {
        if (IsRotating)
        {
            _rotationTimer += Time.deltaTime * _rotateSpeed;
            if (_rotationTimer > 1f)
            {
                transform.eulerAngles = Vector3.Lerp(_startRotation, _endRotation, 1f);
                OnRotationEnd();
            }

            transform.eulerAngles = Vector3.Lerp(_startRotation, _endRotation, _rotationTimer);
        }
    }

    public void Select()
    {
        if (IsSelected)
            return;
        IsSelected = true;
        _visual.SetActive(true);
    }

    public void Deselect()
    {
        IsSelected = false;
        _visual.SetActive(false);
        DotDeselectedEvent?.Invoke();
    }

    public void Rotate(bool clockwise, Action callback)
    {
        IsRotating = true;
        _rotatingClockwise = clockwise;
        _rotationTimer = 0f;
        OnRotationEndCallback = callback;

        float angle = clockwise ? -120f : 120f;
        float currentAngle = transform.rotation.eulerAngles.z;
        float targetAngle = currentAngle + angle;

        _startRotation = transform.rotation.eulerAngles;
        _endRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.x, targetAngle);
    }

    private void OnRotationEnd()
    {
        IsRotating = false;
        OnRotationEndCallback.Invoke();
        OnRotationEndCallback = null;
    }

}
