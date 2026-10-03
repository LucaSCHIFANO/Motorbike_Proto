using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool isInitialized = false;
    public bool IsInitialized { get => isInitialized; }

    private Entity.EntitySide side;
    public Entity.EntitySide Side { get => side; }

    private Vector3 direction;
    private float speed;
    private float height = 0;
    public float Height { get => height; }

    [SerializeField] private GameObject shadow;
    [SerializeField] private float damage = 1f;
    public float Damage { get => damage; }


    /// <summary>
    /// Initializes the projectile with the given parameters.
    /// </summary>
    public void InitProjectile(Vector3 _direction, float _speed, Entity.EntitySide _side, float _height)
    {
        direction = _direction;
        speed = _speed;
        side = _side;
        this.height = _height;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }

        shadow.transform.localPosition = new Vector2(shadow.transform.localPosition.x, shadow.transform.localPosition.y - height);
        isInitialized = true;
    }

    /// <summary>
    /// Called when the projectile hits something.
    /// </summary>
    public void Hit()
    {
        Destroy(gameObject);
    }
}
