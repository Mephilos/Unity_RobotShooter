using System.Collections;
using UnityEngine;

public class Grenadier : RangeEnemy
{
    [SerializeField] GameObject grenadePrefab;
    [SerializeField] Transform grenadeFirePoint;
    [SerializeField] float grenadeCooltime = 10f;
    [SerializeField] float throwAngle = 45f;
    [SerializeField] float throwRangeMin = 5f;
    [SerializeField] float throwRangeMax = 20f;
    [SerializeField] LayerMask findRouteLayer;

    public Vector3 ThrowPosition { get; set; }
    public float GrenadeCooltime => grenadeCooltime;
    public float NextGrenadeTime { get; set; }
    public BaseEnemyState ThrowState { get; private set; }

    protected override void InitializeState()
    {
        base.InitializeState();
        ThrowState = new GrenadeThrowState(this);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        NextGrenadeTime = Time.time + grenadeCooltime;
    }

    public override void OnCombatFinish()
    {
        float dist = Vector3.Distance(transform.position, LastPlayerPosition);

        if (CanThrowGrenade(dist))
        {
            ChangeState(ThrowState);
        }
        else
        {
            ChangeState(SearchState);
        }
    }

    public void OnAnimationThrow()
    {
        if (isDead) return;

        GameObject grenade = PoolManager.Instance.Get(grenadePrefab, grenadeFirePoint.position, Quaternion.identity);
        var newGrenade = grenade.GetComponent<Grenade>();

        Vector3 velocity = CalculateVelocity(ThrowPosition, grenadeFirePoint.position, throwAngle);
        newGrenade.Throw(velocity);
    }

    bool CanThrowGrenade(float dist)
    {
        if (Time.time < NextGrenadeTime) return false;
        if (dist < throwRangeMin || dist > throwRangeMax) return false;
        if (Physics.Raycast(grenadeFirePoint.position, Vector3.up, 2f, findRouteLayer)) return false;
        return true;
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 start, float angle)
    {
        Vector3 direction = target - start;
        float height = direction.y;
        direction.y = 0;
        float dist = direction.magnitude;
        float a = angle * Mathf.Deg2Rad;

        direction.y = dist * Mathf.Tan(a);
        dist += Mathf.Abs(height / Mathf.Tan(a));

        float vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        return vel * direction.normalized;
    }
}

