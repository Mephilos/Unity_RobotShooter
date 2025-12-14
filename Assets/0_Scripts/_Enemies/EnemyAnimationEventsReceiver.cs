using UnityEngine;

public class EnemyAnimationEventsReceiver : MonoBehaviour
{
    private RangeEnemy rangeEnemy;

    void Awake()
    {
        rangeEnemy = GetComponentInParent<RangeEnemy>();
    }

    public void OnShootEvent()
    {
        rangeEnemy.OnAnimationShoot();
    }
    public void OnThrowEvent()
    {
        if (rangeEnemy is Grenadier grenadier)
            grenadier.OnAnimationThrow();
    }
}
