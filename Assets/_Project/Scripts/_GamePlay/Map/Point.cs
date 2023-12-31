using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Obi;
using Pancake;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Point : MonoBehaviour
{
    public Transform center;
    private bool _stopReset;
    public Transform slotSelect;
    private ItemSlot _previousSlotSelected;
    private Transform _currentSlotSelect;
    public EPointState ePointState;
    [SerializeField] private LayerMask checkWith;
    private bool _isBack;
    public bool canTouch;
    private void Start()
    {
        _isBack = false;
        canTouch = true;
        if (slotSelect != null)
        {
            _currentSlotSelect = slotSelect;
        }
    }
    public void SetMaxLength()
    {
        slotSelect.GetComponentInParent<ItemSlot>().SetSelected(false);
        if (_isBack == false && ePointState == EPointState.Move)
        {
            _isBack = true;
            Movement(_currentSlotSelect.position, (() =>
            {
                _isBack = false;
                slotSelect = _currentSlotSelect;
                GetItemSlot(_currentSlotSelect).isCollide = true;
            }));
        }
    }
    public void UpdateCurrentPosi(float maxLength)
    {
        slotSelect.GetComponentInParent<ItemSlot>().SetSelected(false);
        if (slotSelect == null) return;
        var condition = GetItemSlot(slotSelect);
        if (condition.isCollide == false && (slotSelect.position - center.position).magnitude <= maxLength + transform.lossyScale.x / 2)
        {
            Movement(slotSelect.position, (() =>
            {
                if (_isBack == false)
                {
                    condition.isCollide = true;
                    _currentSlotSelect = slotSelect;
                }
            }));
        }
        else
        {
            var newPosi = new Vector3(_currentSlotSelect.position.x, _currentSlotSelect.position.y, 0);
            Movement(newPosi, (() =>
            {
                slotSelect = _currentSlotSelect;
                GetItemSlot(slotSelect).isCollide = true;
            }));
        }
    }
    public void CancelReset()
    {
        _stopReset = true;
        StopCoroutine(WaitToResetRope());
    }
    ItemSlot GetItemSlot(Transform itemSLot) => itemSLot.gameObject.GetComponentInParent<ItemSlot>();
    void Movement(Vector3 destination, Action action)
    {
        _stopReset = false;
        transform.DOMove(destination, 0.1f).OnUpdate((() =>
        {
            canTouch = false;
        })).OnComplete((() =>
        {
            action?.Invoke();
            ePointState = EPointState.Idle;
            canTouch = true;
            if (_isBack == false)
            {
                StartCoroutine(WaitToResetRope());
            }
        }));
    }
    IEnumerator WaitToResetRope()
    {
        yield return new WaitForSeconds(0.4f);
        if (!_stopReset)
        {
            Observer.DoneMove?.Invoke();
        }
    }
    public void SetCenter(Vector3 centerPosi)
    {
        center.position = centerPosi;
        var s = slotSelect.position - center.position;
        center.right = s;
        transform.position = slotSelect.position;
        if (slotSelect != null)
        {
            GetItemSlot(slotSelect).isCollide = false;
        }
    }
    public void Move(Vector3 updatePosi)
    {
        transform.position = new Vector3(updatePosi.x, updatePosi.y, -1);
    }
    public void ShootRaycast()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        RaycastHit hit;
        Debug.DrawRay(transform.position, Vector3.forward * 100, Color.black);
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, float.PositiveInfinity, checkWith))
        {
            slotSelect = hit.transform;
            if (_previousSlotSelected != null)
            {
                _previousSlotSelected.SetSelected(false);
            }
            _previousSlotSelected = slotSelect.GetComponentInParent<ItemSlot>();
            slotSelect.GetComponentInParent<ItemSlot>().SetSelected(true);
        }
    }
}
public enum EPointState
{
    Idle,
    Move,
}
