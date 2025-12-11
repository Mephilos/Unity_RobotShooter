using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;

public class EnemyTactic : MonoBehaviour
{
    public enum Covering { Near, FarPlayer, NearPlayer }
    [SerializeField] LayerMask coverLayer;

    public Vector3 FindCover(Transform playerPosition, Covering action, float searchRadius = 20f)
    {
        Debug.Log("장애물 찾기");
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, coverLayer);
        if (colliders.Length == 0) return Vector3.zero;

        List<Collider> bestCover = null;

        switch (action)
        {
            case Covering.Near:
                // 나랑 제일 가까운 거
                bestCover = colliders.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
                Debug.Log("장애물 찾기 액션 체크중 그냥 가장 가까운 장애물");
                break;

            case Covering.FarPlayer:
                bestCover = colliders.OrderByDescending(c => Vector3.Distance(playerPosition.position, c.transform.position)).ToList();
                Debug.Log("장애물 찾기 액션 체크중플레이어로부터멈");
                break;

            case Covering.NearPlayer:
                bestCover = colliders.OrderBy(c => Vector3.Distance(playerPosition.position, c.transform.position)).ToList();
                Debug.Log("장애물 찾기 액션 체크중 플레이어에게가까움");
                break;
        }

        foreach (var c in bestCover)
        {
            Debug.Log("장애물 체크");
            Vector3 hideDir = (c.transform.position - playerPosition.position).normalized;
            Vector3 hidePos = c.transform.position + hideDir * 2.0f;

            if (NavMesh.SamplePosition(hidePos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        Debug.Log("못찾음");
        return transform.position;
    }

    public Vector3 GetRandomPatrolPoint(Vector3 center, float range)
    {
        Vector3 randomPosition = center + Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }
}