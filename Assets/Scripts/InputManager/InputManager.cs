using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TouchEventArgs : EventArgs
{
    public Vector2 Position { get; set; }
}

public class SwipeEventArgs : EventArgs
{
    public Vector2 FirstTouchPosition { get; set; }
    public Vector2 FinalTouchPosition { get; set; }
}

public class InputManager : Singleton<InputManager>
{
    #region Serialized Fields

    [SerializeField] private float _touchSwipeDistanceThreshold = 0.5f;

    #endregion

    #region

    public event Action<TouchEventArgs> TouchEvent;
    public event Action<SwipeEventArgs> SwipeEvent;

    #endregion

    #region Private Fields

    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    private Touch _touch;
    private RaycastHit2D[] _touchHits;
    private GameObject _lastSelectedDot;

    private TouchEventArgs _touchArgs;
    private SwipeEventArgs _swipeArgs;

    #endregion

    void Start()
    {
        _touchArgs = new TouchEventArgs();
        _swipeArgs = new SwipeEventArgs();
        GameManager.Instance.GameOverEvent += () => Destroy(gameObject);
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)

        if (Input.GetMouseButtonDown(0))
        {
            _firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }
        if (Input.GetMouseButtonUp(0))
        {
            _finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float dst = Vector2.Distance(_firstTouchPosition, _finalTouchPosition);

            if (dst < _touchSwipeDistanceThreshold)
            {
                OnTouch();
            }
            else
            {
                OnSwipe();
            }
        }

#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase) {

                case TouchPhase.Began:
                    // Record initial touch position.
                    _firstTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    break;

                case TouchPhase.Ended:

                    _finalTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    float dst = Vector2.Distance(_firstTouchPosition, _finalTouchPosition);
                    if(dst < _touchSwipeDistanceThreshold) {
                        OnTouch();
                    } else {
                        OnSwipe();
                    }
                    break;
            }
        }
#endif
    }

    private void OnTouch()
    {
        _touchArgs.Position = _finalTouchPosition;
        TouchEvent?.Invoke(_touchArgs);
    }

    private void OnSwipe()
    {
        _swipeArgs.FirstTouchPosition = _firstTouchPosition;
        _swipeArgs.FinalTouchPosition = _finalTouchPosition;
        SwipeEvent?.Invoke(_swipeArgs);
    }

}
