using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class Surface : MonoBehaviour
{
    [SerializeField] private GameObject slot;
    private float _lenght;
    private float _height;
    [SerializeField] int searchRow;
    [SerializeField] int searchColumn;
    [SerializeField] int maxLength;
    [SerializeField] int maxHeight;
    [SerializeField] ItemSlot[] _itemSlots;
    [SerializeField] private int distance;
    private ItemSlot[,] _slotArray;
    [SerializeField] private List<SlotSelected> slotSelected = new List<SlotSelected>();
    [SerializeField] private Transform point;
    public void SpawnSlot()
    {
        _slotArray = new ItemSlot[maxLength, maxHeight];
        _itemSlots = new ItemSlot[maxLength * maxHeight];
        var posi = transform.position;
        for (int j = 0; j < maxHeight; j++)
        {
            for (int i = 0; i < maxLength; i++)
            {
                var getObj = Instantiate(slot);
                getObj.transform.localPosition = new Vector3(distance * i, distance * j, 0);
                getObj.transform.SetParent(this.transform);
                getObj.GetComponent<ItemSlot>().column = i;
                getObj.GetComponent<ItemSlot>().row = j;
                _itemSlots[j * maxLength + i] = getObj.GetComponent<ItemSlot>();
                _slotArray[i, j] = getObj.GetComponent<ItemSlot>();
            }
        }

    }
    public void ClearSlots()
    {
        if (transform.childCount == 0) return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var getChild = transform.GetChild(i);
            if (getChild.gameObject.activeInHierarchy)
            {
                DestroyImmediate(getChild.gameObject);
            }
        }
    }
    public void SetUp()
    {
        LoadArray2D();
        if (_slotArray[searchColumn, searchRow] != null)
        {
            point.position = _slotArray[searchColumn, searchRow].transform.position;
            var setPoint = point.gameObject.GetComponentInChildren<Point>();
            setPoint.slotSelect = _slotArray[searchColumn, searchRow].transform;
            SlotSelected s = new SlotSelected();
            s.point = setPoint;
            s.itemSlot = _slotArray[searchColumn, searchRow];
            if (!slotSelected.Contains(s))
            {
                slotSelected.Add(s);
            }
        }
    }
    public void ClearSlotSelected()
    {
        foreach (var clearSlot in slotSelected)
        {
            clearSlot.point = null;
            clearSlot.itemSlot.isCollide = false;
        }
        slotSelected.Clear();
    }
    private void Awake()
    {
        LoadArray2D();
        foreach (var slot in _slotArray)
        {
            slot.isCollide = false;
        }
        foreach (var select in slotSelected)
        {
            select.itemSlot.isCollide = true;
        }
    }
    void LoadArray2D()
    {
        _slotArray = new ItemSlot[maxLength, maxHeight];
        for (int i = 0; i < maxHeight; i++)
        {
            for (int j = 0; j < maxLength; j++)
            {
                if (_itemSlots[(i * maxLength) + j])
                {
                    var getObj = _itemSlots[(i * maxLength) + j];
                    _slotArray[j, i] = getObj;
                }
            }
        }
    }
    [Serializable]
    public class SlotSelected
    {
        public Point point;
        public ItemSlot itemSlot;
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(Surface), true)]
    [CanEditMultipleObjects]
    public class SpawnSlots : Editor
    {
        private Surface _surface;
        private void OnEnable()
        {
            _surface = target as Surface;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("CreateSlots", GUILayout.MinHeight(40), GUILayout.MinWidth(100)))
            {
                _surface.SpawnSlot();
            }
            if (GUILayout.Button("Clear", GUILayout.MinHeight(40), GUILayout.MinWidth(100)))
            {
                _surface.ClearSlots();
            }
            if (GUILayout.Button("SetUp", GUILayout.MinHeight(40), GUILayout.MinWidth(100)))
            {
                _surface.SetUp();
            }
            if (GUILayout.Button("ClearSetUp", GUILayout.MinHeight(40), GUILayout.MinWidth(100)))
            {
                _surface.ClearSlotSelected();
            }
            base.OnInspectorGUI();
        }
    }
#endif
}
