using UnityEngine;

public class UIRotator : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 360f;

    void Update()
    {
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}
