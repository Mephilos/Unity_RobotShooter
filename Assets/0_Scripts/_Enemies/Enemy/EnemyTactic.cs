using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyTactic : MonoBehaviour
{
    public enum Covering { Near, FarPlayer, NearPlayer }
    [SerializeField] LayerMask coverLayer;
    [SerializeField] float distToCover = 3f;
    private Collider[] coverColliders = new Collider[10];

    private List<Collider> checkkedCovers = new List<Collider>(10);

    public Vector3 FindCover(Transform playerPosition, Covering action, float searchRadius = 20f, float maxRange = -.1f)
    {
        int cnt = Physics.OverlapSphereNonAlloc(transform.position, searchRadius, coverColliders, coverLayer);

        if (cnt == 0) return Vector3.zero;

        checkkedCovers.Clear();

        float maxRangeSqr = maxRange > 0 ? maxRange * maxRange : -1f;
        Vector3 targetPosition = playerPosition.position;
        Vector3 myPosition = transform.position;

        for (int i = 0; i < cnt; i++)
        {
            Collider coverCol = coverColliders[i];

            if (maxRangeSqr > 0)
            {
                float distSqr = (targetPosition - coverCol.transform.position).sqrMagnitude;
                if (distSqr > maxRangeSqr) continue;
            }

            checkkedCovers.Add(coverCol);
        }
        if (checkkedCovers.Count == 0) return Vector3.zero;

        switch (action)
        {
            case Covering.Near:
                // 나랑 제일 가까운 거
                checkkedCovers.Sort((a, b) =>
                {
                    float distA = (myPosition - a.transform.position).sqrMagnitude;
                    float distB = (myPosition - b.transform.position).sqrMagnitude;
                    return distA.CompareTo(distB);
                });
                break;

            case Covering.FarPlayer:
                checkkedCovers.Sort((a, b) =>
                {
                    float distA = (myPosition - a.transform.position).sqrMagnitude;
                    float distB = (myPosition - b.transform.position).sqrMagnitude;
                    return distB.CompareTo(distA);
                });
                break;

            case Covering.NearPlayer:
                checkkedCovers.Sort((a, b) =>
                {
                    float distA = (myPosition - a.transform.position).sqrMagnitude;
                    float distB = (myPosition - b.transform.position).sqrMagnitude;
                    return distA.CompareTo(distB);
                });
                break;
        }

        for (int i = 0; i < checkkedCovers.Count; i++)
        {
            Transform coverTransform = checkkedCovers[i].transform;
            Vector3 hideDir = (coverTransform.position - playerPosition.position).normalized;
            Vector3 hidePos = coverTransform.position + hideDir * distToCover;

            if (NavMesh.SamplePosition(hidePos, out NavMeshHit hit, distToCover, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return transform.position;
    }

    public Vector3 GetRandomWayPoint(Vector3 center, float range)
    {
        Vector3 randomPosition = center + Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }
}

// using UnityEngine;
// using UnityEngine.AI;
// using System.Linq;
// using System.Collections.Generic;

// public class EnemyTactic : MonoBehaviour
// {
//     public enum Covering { Near, FarPlayer, NearPlayer }
//     [SerializeField] LayerMask coverLayer;

//     public Vector3 FindCover(Transform playerPosition, Covering action, float searchRadius = 20f, float maxRange = -.1f)
//     {
//         Debug.Log("장애물 찾기");
//         Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, coverLayer);

//         if (maxRange > 0)
//         {
//             colliders = colliders.Where(c => Vector3.Distance(playerPosition.position, c.transform.position) <= maxRange).ToArray();
//         }

//         if (colliders.Length == 0) return Vector3.zero;

//         List<Collider> bestCover = null;

//         switch (action)
//         {
//             case Covering.Near:
//                 // 나랑 제일 가까운 거
//                 bestCover = colliders.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
//                 Debug.Log("장애물 찾기 액션 체크중 그냥 가장 가까운 장애물");
//                 break;

//             case Covering.FarPlayer:
//                 bestCover = colliders.OrderByDescending(c => Vector3.Distance(playerPosition.position, c.transform.position)).ToList();
//                 Debug.Log("장애물 찾기 액션 체크중플레이어로부터멈");
//                 break;

//             case Covering.NearPlayer:
//                 bestCover = colliders.OrderBy(c => Vector3.Distance(playerPosition.position, c.transform.position)).ToList();
//                 Debug.Log("장애물 찾기 액션 체크중 플레이어에게가까움");
//                 break;
//         }

//         foreach (var c in bestCover)
//         {
//             Debug.Log("장애물 체크");
//             Vector3 hideDir = (c.transform.position - playerPosition.position).normalized;
//             Vector3 hidePos = c.transform.position + hideDir * 3.0f;

//             if (NavMesh.SamplePosition(hidePos, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
//             {
//                 return hit.position;
//             }
//         }
//         Debug.Log("못찾음");
//         return transform.position;
//     }

//     public Vector3 GetRandomWayPoint(Vector3 center, float range)
//     {
//         Vector3 randomPosition = center + Random.insideUnitSphere * range;
//         if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, range, NavMesh.AllAreas))
//         {
//             return hit.position;
//         }
//         return center;
//     }
// }