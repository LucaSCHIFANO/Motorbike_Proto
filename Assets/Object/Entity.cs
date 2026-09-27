using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject visual;
    [SerializeField] protected SpriteRenderer sr;
    protected Vector3 localVisualStartingPosition;

    [Header("Health")]
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] protected EntitySide side;
    protected Material originalMaterial;
    [SerializeField] protected Material blinkMaterial;
    [SerializeField] protected float blinkDuration;

    [Header("Death")]
    [SerializeField] protected List<Scraps> deathScraps = new List<Scraps>();


    [Header("Jump")]
    [SerializeField] protected float jumpTime;
    [SerializeField] protected float jumpForce;
    protected float jumpVelocity;
    protected bool isJumping = false;
    protected float currentJumpTime = 0f;
    [SerializeField] protected float fallMultiplier;
    protected bool isGrounded = true;


    [Header("Trail")]
    [SerializeField] protected GameObject trailPrefab;
    [SerializeField] protected Transform trailSpawnPoint;
    [SerializeField] protected float trailSpawnRate = 0.1f;
    protected float trailSpawnTimer = 0f;

    public enum EntitySide
    {
        Neutral,
        Player,
        Enemy,
    }

    #region Virtual Methods

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        localVisualStartingPosition = visual.transform.localPosition;
        originalMaterial = sr.material;
    }

    protected virtual void FixedUpdate()
    {
        Jump();
    }

    protected virtual void Update()
    {
        ManageJumpTime();
        ManagedTrailSpawn();
    }

    /// <summary>
    /// Manages the jump for the entity.
    /// </summary>
    protected virtual void ManageJumpTime()
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
        }
    }

    /// <summary>
    /// Handles the jump logic for the entity (keeping the shadow on the ground). 
    /// </summary>
    protected virtual void Jump()
    {
        if (isJumping)
        {
            jumpVelocity = jumpForce * Time.deltaTime;
            isGrounded = false;
        }
        else if (!isGrounded)
        {
            jumpVelocity -= fallMultiplier * Time.fixedDeltaTime;
        }

        visual.transform.position += Vector3.up * jumpVelocity;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (collision.TryGetComponent<Projectile>(out Projectile projectile) && projectile.IsInitialized
                && projectile.Side != side && IsAtSameHeight(projectile.Height))
            {
                projectile.Hit();
                OnTakeDamage(projectile.Damage);
            }
        }
    }

    /// <summary>
    /// Handles the logic when the entity takes damage. If health reaches zero, the entity is destroyed.
    /// </summary>
    protected virtual void OnTakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(ResetMaterial());
        }
    }

    /// <summary>
    /// Handles the death of the entity, destroying it.
    /// </summary>
    protected virtual void Die()
    {
        Destroy(gameObject);
        Debug.Log($"{gameObject.name} has died.");

        if (deathScraps.Count > 0)
        {
            foreach (Scraps scrap in deathScraps)
            {
                Scraps instantiatedScrap = Instantiate(scrap, transform.position, Quaternion.identity).GetComponent<Scraps>();
                instantiatedScrap.Init();
            }
        }   
    }

    #endregion

    /// <summary>
    /// Manages the spawning of trail effects while the entity is grounded.
    /// </summary>
    protected void ManagedTrailSpawn()
    {
        if (isGrounded)
        {
            trailSpawnTimer += Time.deltaTime;
            if (trailSpawnTimer >= trailSpawnRate)
            {
                SpawnTrail();
                trailSpawnTimer = 0f;
            }
        }
        else
        {
            trailSpawnTimer = trailSpawnRate;
        }
    }

    /// <summary>
    /// Spawns a trail effect at the specified spawn point. 
    /// The trail is destroyed after 1 second to prevent cluttering the scene.
    /// </summary>
    protected void SpawnTrail()
    {
        if (trailPrefab != null && trailSpawnPoint != null)
        {
            GameObject trail = Instantiate(trailPrefab, trailSpawnPoint.position, Quaternion.identity);
            Destroy(trail, 1f);
        }
    }

    /// <summary>
    /// Returns the current height of the entity's visual representation relative to the ground. 
    /// </summary>
    protected float GetHeight()
    {
        return Vector2.Distance(visual.transform.localPosition, localVisualStartingPosition);
    }

    /// <summary>
    /// Checks if the entity is at the same height as the specified height within a tolerance.
    /// </summary>
    protected bool IsAtSameHeight(float height)
    {
        return Mathf.Abs(GetHeight() - height) < 0.5f;
    }

    /// <summary>
    /// Resets the entity's material to the original after a brief blink effect when taking damage.
    /// </summary>
    protected IEnumerator ResetMaterial()
    {
        sr.material = blinkMaterial;
        yield return new WaitForSeconds(blinkDuration);
        sr.material = originalMaterial;
    }

}
