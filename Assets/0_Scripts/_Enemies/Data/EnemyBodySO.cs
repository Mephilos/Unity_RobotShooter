using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyBodyData", menuName = "Enemy/BodyData")]
public class EnemyBodySO : ScriptableObject
{
    public float MaxHP = 100f;
    public float MoveSpeed = 3.5f;

    public float DetectionRadius = 20f;
    public float ViewAngle = 110f;
    public float LostTargetSearchTime = 5f;
}
