using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    [SerializeField] float lifeTime = 2f;

    void OnEnable()
    {
        CancelInvoke(nameof(Release));
        Invoke(nameof(Release), lifeTime);
    }
    void Release()
    {
        PoolManager.Instance.Release(gameObject);
    }
}
