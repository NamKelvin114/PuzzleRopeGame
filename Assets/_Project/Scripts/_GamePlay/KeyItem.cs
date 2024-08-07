using System;
using DG.Tweening;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
   [SerializeField] private GameObject lockObj;
   [SerializeField] private float durationFly;
   private Tween _tween;
   public void DoUnLock()
   {
      _tween=transform.DOMove(lockObj.transform.position, durationFly).OnComplete((() =>
      {
         lockObj.gameObject.SetActive(false);
         gameObject.SetActive(false);
      }));
   }

   private void OnDisable()
   {
      if (_tween!=null)
      {
         DOTween.Kill(_tween);
      }
   }
}
