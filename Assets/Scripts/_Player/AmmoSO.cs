using UnityEngine;

[CreateAssetMenu(fileName = "AmmoSO", menuName = "Scriptable Objects/AmmoSO")]
public class AmmoSO : ScriptableObject
{
    public GameObject AmmoPrefab;
    public int AmmoValue = 10;
    public float RespawnTime = 15f;
}
