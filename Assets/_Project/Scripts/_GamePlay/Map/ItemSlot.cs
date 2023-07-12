using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Color colorSelected;
    [SerializeField] private Color normalColor;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private void Start()
    {
        SetSelected(false);
    }
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            spriteRenderer.color = colorSelected;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }
    }
    public bool isCollide;
    public int row;
    public int column;
}
