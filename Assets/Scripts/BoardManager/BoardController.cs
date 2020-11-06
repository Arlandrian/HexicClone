﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController
{
    #region Events

    public event Action MovementEvent;
    public event Action<int> ScoreEvent;

    #endregion

    #region Private Fields

    private const float _circleCastRadius = 1.0f;

    private Board _board;
    private DotBehaviour _lastSelectedDot;

    private Touch _touch;
    private RaycastHit2D[] _touchHits;

    private bool _isBoardChanging = false;

    // cw or ccw
    private bool _lastRotationDirection = false;
    private int _rotationCounter = 0;
    private int _rotationTime = 3;

    #endregion

    public void Init(Board board)
    {
        _board = board;
        _board.HexExplodedEvent += OnHexExploded;
        InputManager.Instance.TouchEvent += OnTouch;
        InputManager.Instance.SwipeEvent += OnSwipe;
    }

    private void OnTouch(TouchEventArgs args)
    {
        if (_isBoardChanging)
        {
            return;
        }

        _touchHits = Physics2D.CircleCastAll(args.Position, _circleCastRadius, Vector2.zero);
        float minDist = 4.0f;
        GameObject selectedDot = null;
        foreach (RaycastHit2D hit in _touchHits)
        {
            Vector2 hitPos = hit.collider.gameObject.transform.position;
            float dst = SqrDistance(args.Position, hitPos);
            if (dst < minDist)
            {
                minDist = dst;
                selectedDot = hit.collider.gameObject;
            }
        }

        if (selectedDot == null)
        {
            return;
        }

        if (_lastSelectedDot == null)
        {
            _lastSelectedDot = selectedDot.GetComponent<DotBehaviour>();
        }

        if (selectedDot != _lastSelectedDot)
        {
            _lastSelectedDot.GetComponent<DotBehaviour>().Deselect();
        }

        _lastSelectedDot = selectedDot.GetComponent<DotBehaviour>();
        _lastSelectedDot.GetComponent<DotBehaviour>().Select();
    }

    private void OnSwipe(SwipeEventArgs args)
    {
        if (_isBoardChanging)
        {
            return;
        }

        if (_lastSelectedDot == null)
            return;

        Vector2 origin = _lastSelectedDot.transform.position;
        Vector2 OStart = args.FirstTouchPosition - origin;
        Vector2 OEnd = args.FinalTouchPosition - origin;

        
        _rotationCounter = 0;
        if (Vector2.SignedAngle(OStart, OEnd) < 0.0f)
        {
            _lastRotationDirection = true;
            OnClockwiseSwipe();
        }
        else
        {
            _lastRotationDirection = false;
            OnCounterClockwiseSwipe();
        }
    }

    private void OnClockwiseSwipe()
    {
        if (_isBoardChanging)
        {
            return;
        }
        _board.StartCoroutine(RotateDot(isClockwise: true));
    }

    private void OnCounterClockwiseSwipe()
    {
        if (_isBoardChanging)
        {
            return;
        }
        _board.StartCoroutine(RotateDot(isClockwise: false));
    }

    private IEnumerator RotateDot(bool isClockwise)
    {
        _isBoardChanging = true;

        while (_rotationCounter < _rotationTime)
        {
            _board.RotateDot(_lastSelectedDot, isClockwise);
            yield return new WaitWhile(() => _lastSelectedDot.IsRotating);

            List<Vector2Int> matches = _board.FindMatches();
            if(matches.Count > 0)
            {
                _board.ExplodeMatches(matches);
                break;
            }
            else
            {
                yield return new WaitForSeconds(.1f);
                _rotationCounter++;
            }
        }

        yield return new WaitWhile(()=>_board.IsBoardChanging());

        _isBoardChanging = false;
        _rotationCounter = 0;
    }

    private float SqrDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2);
    }

    #region Event Receivers

    private void OnHexExploded()
    {
        ScoreEvent?.Invoke(15);
    }

    #endregion

}