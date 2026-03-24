using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;
    public float moveSpeed = 3.5f;
    public float damage = 12f;
    public float fireRate = 0.6f;
    public float accuracy = 0.5f;
    public float detectionRange = 30f;
    public float shootRange = 25f;

    [Header("State")]
    public bool isDead = false;

    private Transform player;
    private CharacterController controller;
    private float nextShootTime;
    private float gravity = -15f;
    private float velocityY;
    private Vector3 patrolTarget;
    private float patrolTimer;
    private bool playerDetected = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0, 0.9f, 0);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        PickPatrolPoint();
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (controller.isGrounded)
            velocityY = -2f;
        else
            velocityY += gravity * Time.deltaTime;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Simple detection - just use distance, no raycast problems
        playerDetected = distToPlayer < detectionRange;

        if (playerDetected)
        {
            // COMBAT MODE
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

            if (distToPlayer > 8f)
            {
                Vector3 moveDir = lookDir.normalized;
                Vector3 move = moveDir * moveSpeed * Time.deltaTime;
                move.y = velocityY * Time.deltaTime;
                controller.Move(move);
            }
            else if (distToPlayer < 5f)
            {
                Vector3 moveDir = -lookDir.normalized;
                Vector3 move = moveDir * moveSpeed * 0.5f * Time.deltaTime;
                move.y = velocityY * Time.deltaTime;
                controller.Move(move);
            }
            else
            {
                Vector3 strafeDir = transform.right * Mathf.Sin(Time.time * 2f);
                Vector3 move = strafeDir * moveSpeed * 0.4f * Time.deltaTime;
                move.y = velocityY * Time.deltaTime;
                controller.Move(move);
            }

            // Shoot continuously while in range
            if (distToPlayer < shootRange && Time.time >= nextShootTime)
            {
                Shoot();
            }
        }
        else
        {
            // PATROL MODE
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0 || Vector3.Distance(transform.position, patrolTarget) < 2f)
                PickPatrolPoint();

            Vector3 dir = (patrolTarget - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir), Time.deltaTime * 3f);

            Vector3 patrol = dir * moveSpeed * 0.5f * Time.deltaTime;
            patrol.y = velocityY * Time.deltaTime;
            controller.Move(patrol);
        }
    }

    void Shoot()
    {
        nextShootTime = Time.time + fireRate + Random.Range(0f, 0.2f);

        if (Random.value < accuracy)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                Debug.Log("Enemy shot player! Player HP: " + ph.currentHealth);
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        // When hit, always detect player
        playerDetected = true;

        if (health <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }

    void Die()
    {
        enabled = false;
        if (controller != null)
            controller.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    System.Collections.IEnumerator DeathRoutine()
    {
        float timer = 0;
        Vector3 startRot = transform.eulerAngles;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f;
            transform.eulerAngles = new Vector3(
                Mathf.Lerp(0, 90, timer),
                startRot.y,
                startRot.z
            );
            transform.position += Vector3.down * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5f);
        Respawn();
    }

    void Respawn()
    {
        health = 100f;
        isDead = false;
        enabled = true;
        if (controller != null)
            controller.enabled = true;

        transform.eulerAngles = Vector3.zero;
        transform.position = new Vector3(
            Random.Range(-5f, 5f), 5f, Random.Range(-5f, 5f)
        );
        PickPatrolPoint();
    }

    void PickPatrolPoint()
    {
        patrolTarget = transform.position + new Vector3(
            Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f)
        );
        patrolTimer = Random.Range(3f, 6f);
    }
}
