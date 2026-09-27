using UnityEngine;

public class Trail : MonoBehaviour
{
    [SerializeField] protected float speed;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}
