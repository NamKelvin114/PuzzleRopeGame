using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Touch;
using Obi;
using Pancake;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    [ReadOnly] public int bonusMoney;
    [ReadOnly] [SerializeField] private Point selectPoint;
    [SerializeField] private Camera camera;
    [SerializeField] private List<SetupRope> ropeList = new();
    [ReadOnly] [SerializeField] private SetupRope currentRope;
    [ReadOnly] [SerializeField] private List<Rope> ropes = new();

    private bool _isFingerDown;
    private bool _isFingerDrag;
    private bool _isFingerUp;
    private int _maxRope;
    private Vector3 _previous;
    private Point _previousPoint;
    private bool _updatePosi;
    private float _velocity;

    private Camera Camera => GetComponentInChildren<Camera>(true);

    private void Start()
    {
        _maxRope = ropeList.Count;
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerDown += HandleFingerDown;
        LeanTouch.OnFingerUp += HandleFingerUp;
        LeanTouch.OnFingerUpdate += HandleFingerUpdate;
        Observer.RopeCheck += CheckCondition;
        Observer.MaxLength += MaxCondition;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= HandleFingerDown;
        LeanTouch.OnFingerUp -= HandleFingerUp;
        LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        Observer.RopeCheck -= CheckCondition;
        Observer.MaxLength -= MaxCondition;
    }

#if UNITY_EDITOR
    [Button]
    private void StartLevel()
    {
        Data.CurrentLevel = Utility.GetNumberInAString(gameObject.name);

        EditorApplication.isPlaying = true;
    }
#endif
    private void MaxCondition()
    {
        SetMaxCondition(_previousPoint);
    }

    private void SetMaxCondition(Point getPoint)
    {
        _isFingerUp = false;
        _isFingerDown = false;
        _isFingerDrag = false;
        if (getPoint != null) getPoint.SetMaxLength();
    }

    private void HandleFingerDown(LeanFinger finger)
    {
        if (!finger.IsOverGui)
        {
            _isFingerUp = true;
            var ray = finger.GetRay(Camera);
            // var hit = default(RaycastHit);
            var hit = Physics.RaycastAll(ray, float.PositiveInfinity);
            foreach (var hitCheck in hit)
                if (hitCheck.collider.gameObject.CompareTag(Constant.Point))
                {
                    var checkPoint = hitCheck.collider.gameObject.GetComponentInParent<Point>();
                    if (checkPoint.canTouch)
                    {
                        StopUpdateRope();
                        ropes.Clear();
                        selectPoint = checkPoint;
                        selectPoint.ePointState = EPointState.Move;
                        _previousPoint = selectPoint;
                        var fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
                        _previous = fingerPos;
                        foreach (var rope in ropeList)
                        {
                            var r = rope.rope.GetComponent<MeshCollider>();
                            if (r != null) Destroy(r);
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

    private void SetCurrentRope(SetupRope setupRope)
    {
        setupRope.rope.GetComponent<Rope>().isCheckLength = true;
        currentRope = setupRope;
    }

    private void StopUpdateRope()
    {
        foreach (var rope in ropeList)
            if (rope == currentRope)
                rope.rope.GetComponent<Rope>().isCheckLength = false;
    }

    private void RotateCenter(LeanFinger finger)
    {
        var fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
        var direction = fingerPos - selectPoint.center.position;
        selectPoint.center.right = direction;
    }

    private void HandleFingerUp(LeanFinger finger)
    {
        if (_isFingerUp) SetFingerUp();
    }

    private void HandleFingerUpdate(LeanFinger finger)
    {
        if (_isFingerDrag && selectPoint != null)
        {
            if (finger.Index == -42) return;
            var fingerPos = finger.GetWorldPosition(-camera.transform.position.z, camera);
            _velocity = (fingerPos - _previous).magnitude / Time.deltaTime;
            _previous = fingerPos;
            //Debug.Log(_velocity);
            if (_velocity < 45)
            {
                if ((selectPoint.center.position - fingerPos).magnitude <= currentRope.Length)
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

    private void SetFingerUp()
    {
        _isFingerDown = false;
        _isFingerDrag = false;
        if (selectPoint != null)
        {
            selectPoint.UpdateCurrentPosi(currentRope.Length);
            selectPoint = null;
        }
    }

    private void CheckCondition(Rope rope)
    {
        foreach (var ropeData in ropeList)
        {
            var r = ropeData.rope.GetComponent<Rope>();
            if (r == rope)
                if (!r.iscollide)
                {
                    ropeData.rope.stretchingScale = 0;
                    r.isDone = true;
                    ropeData.headOfRope.transform.DOMove(ropeData.tailOfRope.transform.position, 1).OnComplete(() =>
                    {
                        ropeData.rope.transform.parent.gameObject.SetActive(false);
                        ropeList.Remove(ropeData);
                        if (ropeList.Count == 0) OnWin();
                    });
                    ropeData.headOfRope.canTouch = false;
                    ResetSlot(ropeData.headOfRope.slotSelect);
                    ropeData.tailOfRope.canTouch = false;
                    ResetSlot(ropeData.tailOfRope.slotSelect);
                    _maxRope--;
                    if (ropeData.isHaveKey)
                    {
                        ropeData.keyItem.transform.SetParent(transform);
                        ropeData.keyItem.DoUnLock();
                    }
                }
        }

        ropes.Clear();
    }

    public void OnWin()
    {
        GameManager.Instance.OnWinGame();
    }

    public void ResetSlot(Transform slot)
    {
        if (slot.root != slot)
            slot.GetComponentInParent<ItemSlot>().isCollide = false;
        else
            slot.GetComponent<ItemSlot>().isCollide = false;
    }
}

[Serializable]
public class SetupRope
{
    public ObiRope rope;
    public Point headOfRope;
    public Point tailOfRope;
    public float Length;
    public bool isHaveKey;
    [ShowIf(nameof(isHaveKey))] public KeyItem keyItem;
}