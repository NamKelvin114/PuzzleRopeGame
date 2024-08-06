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
    [SerializeField] private ObiRope rope;
    private float _maxLength;
    [ReadOnly] public bool isCheckLength;
    [ReadOnly] public bool isDone;
    [SerializeField] private Material normal;
    [SerializeField] private Material maxLength;
    [SerializeField] private MeshRenderer ropeMesh;
    [ReadOnly] public List<ParticleCollideCheck> particleCollideChecks;
    private ObiSolver _obiSolver;
    private void Start()
    {
        _maxLength = Mathf.CeilToInt(rope.CalculateLength()) + 1f;
        
    }
    private void OnEnable()
    {
        Observer.DoneMove += DoneMoveCallBack;
    }
    private void OnDisable()
    {
        Observer.DoneMove -= DoneMoveCallBack;
    }
    int _checkCollide;
    void DoneMoveCallBack()
    {
        _checkCollide = 0;
        foreach (var particle in particleCollideChecks)
        {
            if (particle.Iscollide())
            {
                _checkCollide++;
            }
        }
        if (_checkCollide==0)
        {
            iscollide = false;
        }
        else
        {
            iscollide = true;
        }
        Observer.RopeCheck?.Invoke(this);
    }
   
    private void Update()
    {
        var a = gameObject.GetComponent<ObiRope>();
        if (isShowLength)
        {
            Debug.Log(a.CalculateLength());
            Debug.Log(_maxLength);
        }
        if (rope.CalculateLength() > _maxLength)
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
