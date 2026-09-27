using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool isInitialized = false;
    private Entity.EntitySide side;
    private Vector3 direction;
    private float speed;
    private float height = 0;
    [SerializeField] private GameObject shadow;
    [SerializeField] private float damage = 1f;

    public Entity.EntitySide Side { get => side; }
    public float Height { get => height; }
    public float Damage { get => damage; }
    public bool IsInitialized { get => isInitialized; set => isInitialized = value; }

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
