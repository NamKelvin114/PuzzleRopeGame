using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoint
{
    public int maxLength { get; set; }
    public int maxHeight { get; set; }
    void Move();
}
