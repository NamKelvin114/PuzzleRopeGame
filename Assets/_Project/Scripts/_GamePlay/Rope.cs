using System;
using System.Collections;
using System.Collections.Generic;
using Pancake;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [ReadOnly] public bool iscollide;
    [ReadOnly] public bool isDone;
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
}
