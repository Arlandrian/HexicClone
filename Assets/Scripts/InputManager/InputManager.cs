using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    #region

    #endregion

    #region Private Fields

    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    private Touch _touch;

    #endregion

    [SerializeField] private float _circleCastRadius = 1.0f;
    [SerializeField] private float _touchSwipeDistanceThreshold = 0.5f;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleInput()
    {
#if FALSE && (UNITY_EDITOR || UNITY_STANDALONE)

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
#elif UNITY_ANDROID || UNITY_IOS || TRUE
        
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

    private void OnSwipe()
    {
        throw new NotImplementedException();
    }

    private void OnTouch()
    {
        throw new NotImplementedException();
    }
}
