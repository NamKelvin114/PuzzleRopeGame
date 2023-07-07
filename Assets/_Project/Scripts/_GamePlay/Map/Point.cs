using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Point : MonoBehaviour
{
    public Transform center;
    public Transform slotSelect;
    private Transform _currentSlotSelect;
    [SerializeField] private LayerMask checkWith;
    private float velocity;
    private Vector3 prvious;
    public bool canTouch;
    private void Start()
    {
        canTouch = true;
        if (slotSelect != null)
        {
            _currentSlotSelect = slotSelect;
        }
    }
    public void UpdateCurrentPosi(float maxLength)
    {
        if (slotSelect == null) return;
        var condition = slotSelect.gameObject.GetComponentInParent<ItemSlot>();
        if (condition.isCollide == false && (slotSelect.position - center.position).magnitude <= maxLength + transform.lossyScale.x / 2)
        {
            Movement(slotSelect.position);
            condition.isCollide = true;
            _currentSlotSelect = slotSelect;
        }
        else
        {
            var newPosi = new Vector3(_currentSlotSelect.position.x, _currentSlotSelect.position.y, 0);
            Movement(newPosi);
            slotSelect = _currentSlotSelect;
            slotSelect.gameObject.GetComponentInParent<ItemSlot>().isCollide = true;
        }
    }
    void Movement(Vector3 destination)
    {
        transform.DOMove(destination, 0.3f).OnUpdate((() => canTouch = false)).OnComplete((() =>
        {
            Observer.DoneMove?.Invoke();
            canTouch = true;
        }));
    }
    public void SetCenter(Vector3 centerPosi)
    {
        center.position = centerPosi;
        var s = slotSelect.position - center.position;
        center.right = s;
        transform.position = slotSelect.position;
        if (slotSelect != null)
        {
            slotSelect.gameObject.GetComponentInParent<ItemSlot>().isCollide = false;
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
        }
    }
}
