using UnityEngine;

public class Scraps : Entity
{
    [Header("References")]
    private Rigidbody2D rb;
    [SerializeField] private float rotationSpeed = 360f;
    Vector3 rotationDirection = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        originalMaterial = sr.material;
        isJumping = true;
        currentJumpTime = jumpTime;
    }

    public void Init(float height)
    {
        float randomXForce = Random.Range(0.5f, 10f);
        float randomXDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        rotationDirection = randomXDirection == 0 ? Vector3.forward : Vector3.back;

        float randomYForce = Random.Range(0.5f, 4f);
        float randomYDirection = Random.Range(0, 2) == 0 ? -1f : 1f;

        Vector2 direction = new Vector2(randomXDirection * randomXForce, randomYDirection * randomYForce);
        rb.AddForce(direction, ForceMode2D.Impulse);     

        sr.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        localVisualStartingPosition = visual.transform.localPosition;
        visual.transform.localPosition = new Vector2(visual.transform.localPosition.x, visual.transform.localPosition.y + height);
    }

    protected override void Update()
    {
        base.Update();
        sr.transform.Rotate(rotationDirection * rotationSpeed * Time.deltaTime);
    }

    protected override void ManageJumpTime()
    {
        currentJumpTime -= Time.deltaTime;
        if (currentJumpTime <= 0)
        {
            isJumping = false;
        }

        if (visual.transform.localPosition.y < localVisualStartingPosition.y)
        {
            visual.transform.localPosition = localVisualStartingPosition;
            isGrounded = true;
            jumpVelocity = 0;
            rb.linearVelocity = new Vector2(-30, 0);
            rotationDirection = Vector3.zero;
        }
    }
}
