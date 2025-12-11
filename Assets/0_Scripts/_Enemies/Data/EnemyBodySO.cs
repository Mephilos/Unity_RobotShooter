using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyBodyData", menuName = "Enemy/BodyData")]
public class EnemyBodySO : ScriptableObject
{
    public int MaxHP = 100;
    public float MoveSpeed = 3.5f;

    public float DetectionRadius = 20f;
    public float ViewAngle = 110f;
}
