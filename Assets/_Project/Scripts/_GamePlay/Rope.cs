using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using Pancake;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [ReadOnly] public bool iscollide;
    [SerializeField] private bool isShowLength;
    private float _maxLength;
    [ReadOnly] public bool isCheckLength;
    [ReadOnly] public bool isDone;
    [SerializeField] private Material normal;
    [SerializeField] private Material maxLength;
    [SerializeField] private MeshRenderer ropeMesh;
    private ObiRope _rope;
    private void Start()
    {
        _rope = gameObject.GetComponent<ObiRope>();
        _maxLength = Mathf.CeilToInt(_rope.CalculateLength()) + 0.8f;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Constant.Rope))
        {
            iscollide = true;
            Observer.RopeCheck?.Invoke(this);
        }
        if (other.gameObject.CompareTag(Constant.Point))
        {
            Observer.RopeCheck?.Invoke(this);
        }
    }
    private void Update()
    {
        var a = gameObject.GetComponent<ObiRope>();
        if (isShowLength)
        {
            Debug.Log(a.CalculateLength());
            Debug.Log(_maxLength);
        }
        if (_rope.CalculateLength() > _maxLength)
        {
            ropeMesh.material = maxLength;
            if (isCheckLength)
            {
                Observer.MaxLength?.Invoke();
            }

        }
        else
        {
            ropeMesh.material = normal;
        }
    }
}
