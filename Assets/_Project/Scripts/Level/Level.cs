using System;
using System.Collections.Generic;
using Pancake;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    [ReadOnly] public int bonusMoney;
    [SerializeField] private Point selectPoint;
    [SerializeField] private Camera camera;
    [SerializeField] private List<SetupSrope> ropeList = new List<SetupSrope>();
    [SerializeField] private SetupSrope currentRope;
    private bool _updatePosi;

    private bool _isFingerDown;
    private bool _isFingerDrag;

    private Camera Camera => GetComponentInChildren<Camera>(true);

    #if UNITY_EDITOR
    [Button]
    private void StartLevel()
    {
        Data.CurrentLevel = Utility.GetNumberInAString(gameObject.name);
        
        EditorApplication.isPlaying = true;
    }
    #endif

    void OnEnable()
    {
        Lean.Touch.LeanTouch.OnFingerDown += HandleFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp += HandleFingerUp;
        Lean.Touch.LeanTouch.OnFingerUpdate += HandleFingerUpdate;

    }

    void OnDisable()
    {
        Lean.Touch.LeanTouch.OnFingerDown -= HandleFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp -= HandleFingerUp;
        Lean.Touch.LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
    }

    void HandleFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (!finger.IsOverGui)
        {

            //Get Object raycast hit
            var ray = finger.GetRay(Camera);
            var hit = default(RaycastHit);

            if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
            { //ADDED LAYER SELECTION
                if (hit.collider.gameObject.CompareTag(Constant.Point))
                {
                    if (finger.Index == -42) return;
                    _isFingerDrag = true;
                    selectPoint = hit.collider.gameObject.GetComponentInParent<Point>();
                    foreach (var rope in ropeList)
                    {
                        if (rope.headOfRope == selectPoint)
                        {
                            selectPoint.SetCenter(rope.tailOfRope.transform.position);
                            currentRope = rope;
                        }
                        else if (rope.tailOfRope == selectPoint)
                        {
                            selectPoint.SetCenter(rope.headOfRope.transform.position);
                            currentRope = rope;
                        }
                    }
                    _isFingerDown = true;
                    _updatePosi = true;
                }
            }
        }
    }
    void RotateCenter(Lean.Touch.LeanFinger finger)
    {
        Vector3 fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
        var direction = fingerPos - selectPoint.center.position;
        selectPoint.center.right = direction;
    }

    void HandleFingerUp(Lean.Touch.LeanFinger finger)
    {
        _isFingerDown = false;
        Observer.OnFingerUp?.Invoke(currentRope.Length);
        selectPoint = null;
    }

    void HandleFingerUpdate(Lean.Touch.LeanFinger finger)
    {
        if (_isFingerDown)
        {
            if (finger.Index == -42) return;
            Vector3 fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
            if (((selectPoint.center.position - fingerPos).magnitude) <= currentRope.Length)
            {
                RotateCenter(finger);
                selectPoint.Move(fingerPos);
                selectPoint.ShootRaycast();
            }
            else
            {
                RotateCenter(finger);
                selectPoint.ShootRaycast();
            }

        }
    }

    private void Start()
    {
        Observer.WinLevel += OnWin;
        Observer.LoseLevel += OnLose;
    }

    private void OnDestroy()
    {
        Observer.WinLevel -= OnWin;
        Observer.LoseLevel -= OnLose;
    }

    public void OnWin(Level level)
    {

    }

    public void OnLose(Level level)
    {

    }
}
[Serializable]
public class SetupSrope
{
    public ERopeIndex ropeOrder;
    public Point headOfRope;
    public Point tailOfRope;
    public float Length;
}
public enum ERopeIndex
{
    first,
    second,
    third,
    fourth,
    fifth,
    sixth,
}
