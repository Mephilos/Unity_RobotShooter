using UnityEngine;

public class EnemySight : MonoBehaviour
{
    [SerializeField] Transform eyePosition;
    [SerializeField] LayerMask viewLayerMask;
    public float viewAngle;
    public float viewDistance;

    public bool CanSeePlayer(Transform target)
    {
        if (target == null) return false;

        Vector3 targetDir = (target.position - eyePosition.position).normalized;

        float dist = Vector3.Distance(eyePosition.position, target.position);
        if (dist > viewDistance) return false; // 설정된 거리 보다 멀면 그냥 리턴

        float angle = Vector3.Angle(eyePosition.forward, targetDir);

        RaycastHit hit;
        if (angle < viewAngle / 2f)
        {
            if (Physics.Raycast(eyePosition.position, targetDir, out hit, dist, viewLayerMask))
            {
                if (hit.transform.CompareTag(Constants.PLAYER_TAG)) return true;
            }
        }
        return false;
    }
}
