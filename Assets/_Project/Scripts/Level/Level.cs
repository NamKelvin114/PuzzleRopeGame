using System;
using System.Collections.Generic;
using DG.Tweening;
using Obi;
using Pancake;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    [ReadOnly] public int bonusMoney;
    [ReadOnly] [SerializeField] private Point selectPoint;
    [SerializeField] private Camera camera;
    [SerializeField] private List<SetupRope> ropeList = new List<SetupRope>();
    [ReadOnly] [SerializeField] private SetupRope currentRope;
    [ReadOnly] [SerializeField] private List<Rope> ropes = new List<Rope>();
    private bool _updatePosi;
    private float _velocity;
    private Vector3 _previous;

    private bool _isFingerDown;
    private bool _isFingerDrag;
    private bool _isFingerUp;
    private int _countRope;
    private Point _previousPoint;

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
        Observer.RopeCheck += CheckCondition;
        Observer.DoneMove += CheckRopeCollide;
        Observer.MaxLength += MaxCondition;
    }

    void OnDisable()
    {
        Lean.Touch.LeanTouch.OnFingerDown -= HandleFingerDown;
        Lean.Touch.LeanTouch.OnFingerUp -= HandleFingerUp;
        Lean.Touch.LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        Observer.RopeCheck -= CheckCondition;
        Observer.DoneMove -= CheckRopeCollide;
        Observer.MaxLength -= MaxCondition;

    }
    private void Start()
    {
        _countRope = ropeList.Count;
    }
    void MaxCondition()
    {
        SetMaxCondition(_previousPoint);
    }
    void SetMaxCondition(Point getPoint)
    {
        _isFingerUp = false;
        _isFingerDown = false;
        _isFingerDrag = false;
        if (getPoint != null)
        {
            getPoint.SetMaxLength();
        }
    }

    void HandleFingerDown(Lean.Touch.LeanFinger finger)
    {
        if (!finger.IsOverGui)
        {
            _isFingerUp = true;
            var ray = finger.GetRay(Camera);
            var hit = default(RaycastHit);

            if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
            {
                if (hit.collider.gameObject.CompareTag(Constant.Point))
                {
                    var checkPoint = hit.collider.gameObject.GetComponent<Point>();
                    if (checkPoint.canTouch)
                    {
                        StopUpdateRope();
                        ropes.Clear();
                        selectPoint = checkPoint;
                        selectPoint.ePointState = EPointState.Move;
                        _previousPoint = selectPoint;
                        Vector3 fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
                        _previous = fingerPos;
                        foreach (var rope in ropeList)
                        {
                            var r = rope.rope.GetComponent<MeshCollider>();
                            if (r != null)
                            {
                                Destroy(r);
                            }
                            rope.tailOfRope.CancelReset();
                            rope.headOfRope.CancelReset();
                            if (rope.headOfRope == selectPoint)
                            {
                                selectPoint.SetCenter(rope.tailOfRope.transform.position);
                                SetCurrentRope(rope);
                            }
                            else if (rope.tailOfRope == selectPoint)
                            {
                                selectPoint.SetCenter(rope.headOfRope.transform.position);
                                SetCurrentRope(rope);
                            }
                        }
                        _isFingerDrag = true;
                        _updatePosi = true;
                    }
                }
            }
        }
    }
    void SetCurrentRope(SetupRope setupRope)
    {
        setupRope.rope.GetComponent<Rope>().isCheckLength = true;
        currentRope = setupRope;
    }
    void StopUpdateRope()
    {
        foreach (var rope in ropeList)
        {
            if (rope == currentRope)
            {
                rope.rope.GetComponent<Rope>().isCheckLength = false;
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
        if (_isFingerUp)
        {
            SetFingerUp();
        }
    }

    void HandleFingerUpdate(Lean.Touch.LeanFinger finger)
    {
        if (_isFingerDrag && selectPoint != null)
        {
            if (finger.Index == -42) return;
            Vector3 fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
            _velocity = ((fingerPos - _previous).magnitude) / Time.deltaTime;
            _previous = fingerPos;
            //Debug.Log(_velocity);
            if (_velocity < 45)
            {
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
            else
            {
                SetMaxCondition(selectPoint);
            }
        }
    }
    void SetFingerUp()
    {
        _isFingerDown = false;
        _isFingerDrag = false;
        if (selectPoint != null)
        {
            selectPoint.UpdateCurrentPosi(currentRope.Length);
            selectPoint = null;
        }
    }
    void CheckRopeCollide()
    {
        _previousPoint = selectPoint;
        foreach (var rope in ropeList)
        {
            if (rope.rope.GetComponent<Rope>().isDone == false && rope.rope.gameObject.GetComponent<MeshCollider>() == null)
            {
                var b = rope.rope.gameObject.AddComponent<MeshCollider>();
                b.convex = true;
                b.isTrigger = true;
            }
        }
    }
    void CheckCondition(Rope rope)
    {
        if (!ropes.Contains(rope))
        {
            ropes.Add(rope);
            if (ropes.Count == _countRope)
            {
                int count = 0;
                foreach (var arope in ropes)
                {
                    if (arope.iscollide == false)
                    {
                        foreach (var ropelist in ropeList)
                        {
                            var r = ropelist.rope.GetComponent<Rope>();
                            if (r == arope)
                            {
                                ropelist.rope.stretchingScale = 0;
                                r.isDone = true;
                                ropelist.headOfRope.transform.DOMove(ropelist.tailOfRope.transform.position, 1).OnComplete(() =>
                                {
                                    ropelist.rope.transform.parent.gameObject.SetActive(false);
                                });
                                ropelist.headOfRope.canTouch = false;
                                ResetSlot(ropelist.headOfRope.slotSelect);
                                ropelist.tailOfRope.canTouch = false;
                                ResetSlot(ropelist.tailOfRope.slotSelect);
                                _countRope--;
                                count++;
                            }
                        }
                    }
                }
                if (count == ropes.Count)
                {
                    OnWin();
                }
                foreach (var brope in ropes)
                {
                    Destroy(brope.gameObject.GetComponent<MeshCollider>());
                    brope.iscollide = false;
                }
            }
        }
    }
    public void OnWin()
    {
        GameManager.Instance.OnWinGame();
    }
    public void ResetSlot(Transform slot)
    {
        if (slot.root != slot)
        {
            slot.GetComponentInParent<ItemSlot>().isCollide = false;
        }
        else
        {
            slot.GetComponent<ItemSlot>().isCollide = false;
        }
    }
}
[Serializable]
public class SetupRope
{
    public ObiRope rope;
    public Point headOfRope;
    public Point tailOfRope;
    public float Length;
}
