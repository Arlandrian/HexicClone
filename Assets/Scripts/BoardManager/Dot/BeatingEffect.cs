using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatingEffect : MonoBehaviour
{
    public float beatingAmplitude = 0.05f;
    public float beatingFrequency = 10.0f;

    private DotBehaviour _dotBehaviour;

    private void Start() {
        _dotBehaviour = transform.parent.GetComponent<DotBehaviour>();
        _dotBehaviour.DotDeselectedEvent += OnDotDeselected;
    }

    void FixedUpdate()
    {
        if (_dotBehaviour.IsSelected)
        {
            transform.localScale = Vector3.one + Vector3.one * beatingAmplitude * Mathf.Abs(Mathf.Sin(beatingFrequency * 0.5f * Time.time));
        }
    }

    private void OnDotDeselected()
    {
        transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        if (_dotBehaviour != null)
        {
            _dotBehaviour.DotDeselectedEvent -= OnDotDeselected;
        }
    }

}
