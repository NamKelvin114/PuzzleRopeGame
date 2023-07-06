using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Point : MonoBehaviour
{
    public Transform center;
    public Transform slotSelect;
    private Transform _currentSlotSelect;
    [SerializeField] private LayerMask checkWith;
    private Vector3 _currentPosi;
    private void OnEnable()
    {
        Observer.OnFingerUp += UpdateCurrentPosi;
    }
    private void OnDisable()
    {
        Observer.OnFingerUp -= UpdateCurrentPosi;
    }
    private void Start()
    {
        _currentPosi = transform.position;
        if (slotSelect != null)
        {
            _currentSlotSelect = slotSelect;
        }
    }
    void UpdateCurrentPosi(float maxLength)
    {
        if (slotSelect == null) return;
        var condition = slotSelect.gameObject.GetComponentInParent<ItemSlot>();
        if (condition.isCollide == false && (slotSelect.position - center.position).magnitude <= maxLength + transform.lossyScale.x / 2)
        {
            Debug.Log("can");
            transform.position = slotSelect.transform.position;
            condition.isCollide = true;
            _currentPosi = transform.position;
            _currentSlotSelect = slotSelect;
        }
        else
        {
            transform.position = new Vector3(_currentPosi.x, _currentPosi.y, 0);
            slotSelect = _currentSlotSelect;
            slotSelect.gameObject.GetComponentInParent<ItemSlot>().isCollide = true;
        }
    }
    public void SetCenter(Vector3 centerPosi)
    {
        center.position = centerPosi;
        var s = _currentPosi - center.position;
        center.right = s;
        transform.position = _currentPosi;
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
