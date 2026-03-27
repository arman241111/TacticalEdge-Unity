using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;
    public float moveSpeed = 3.5f;
    public float damage = 12f;
    public float fireRate = 0.6f;
    public float accuracy = 0.45f;
    public float detectionRange = 25f;
    public float shootRange = 22f;
    public float hearingRange = 15f;

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
    private float lostPlayerTimer = 0f;
    private Vector3 lastKnownPlayerPos;
    private float reactionTime = 0.4f;
    private float reactionTimer = 0f;
    private bool canShoot = false;

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

        // Detection: only through line of sight OR hearing
        bool canSeePlayer = false;
        bool canHearPlayer = false;

        // Line of sight check
        if (distToPlayer < detectionRange)
            canSeePlayer = HasLineOfSight();

        // Hearing: detect player if close and running (not walking)
        if (distToPlayer < hearingRange)
        {
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null && !pm.isWalking && !pm.isCrouching)
                canHearPlayer = true;
        }

        // Update detection state
        if (canSeePlayer)
        {
            playerDetected = true;
            lostPlayerTimer = 0f;
            lastKnownPlayerPos = player.position;
        }
        else if (canHearPlayer)
        {
            playerDetected = true;
            lostPlayerTimer = 0f;
            lastKnownPlayerPos = player.position;
        }
        else if (playerDetected)
        {
            // Lost sight - search for a while then give up
            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer > 5f)
            {
                playerDetected = false;
                canShoot = false;
                reactionTimer = 0f;
            }
        }

        if (playerDetected && canSeePlayer)
        {
            // COMBAT: can see player
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(lookDir), Time.deltaTime * 6f);

            // Reaction time before shooting
            reactionTimer += Time.deltaTime;
            canShoot = reactionTimer >= reactionTime;

            if (distToPlayer > 10f)
            {
                // Move towards player using cover
                MoveTowards(player.position);
            }
            else if (distToPlayer < 4f)
            {
                // Back up
                Vector3 moveDir = (transform.position - player.position).normalized;
                moveDir.y = 0;
                Move(moveDir * moveSpeed * 0.5f);
            }
            else
            {
                // Strafe
                float strafeDir = Mathf.Sin(Time.time * 1.5f + transform.position.x);
                Vector3 strafe = transform.right * strafeDir;
                Move(strafe * moveSpeed * 0.4f);
            }

            // Shoot if can see and reaction time passed
            if (canShoot && distToPlayer < shootRange && Time.time >= nextShootTime)
            {
                Shoot();
            }
        }
        else if (playerDetected && !canSeePlayer)
        {
            // SEARCH: go to last known position
            canShoot = false;
            reactionTimer = 0f;
            MoveTowards(lastKnownPlayerPos);

            if (Vector3.Distance(transform.position, lastKnownPlayerPos) < 2f)
            {
                // Reached last known pos, look around
                transform.Rotate(0, 120 * Time.deltaTime, 0);
            }
        }
        else
        {
            // PATROL
            canShoot = false;
            reactionTimer = 0f;
            patrolTimer -= Time.deltaTime;

            if (patrolTimer <= 0 || Vector3.Distance(transform.position, patrolTarget) < 2f)
                PickPatrolPoint();

            MoveTowards(patrolTarget, 0.5f);
        }
    }

    void MoveTowards(Vector3 target, float speedMult = 1f)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 4f);
        Move(dir * moveSpeed * speedMult);
    }

    void Move(Vector3 moveDir)
    {
        Vector3 move = moveDir * Time.deltaTime;
        move.y = velocityY * Time.deltaTime;
        controller.Move(move);
    }

    bool HasLineOfSight()
    {
        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = player.position + Vector3.up * 1f;
        Vector3 dir = (targetPos - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, targetPos);

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, dist))
        {
            // Hit something - check if it's the player
            if (hit.collider.CompareTag("Player") ||
                hit.collider.GetComponentInParent<PlayerHealth>() != null)
                return true;

            // Hit a wall or obstacle - can't see player
            return false;
        }

        // Nothing hit, clear line of sight
        return true;
    }

    void Shoot()
    {
        nextShootTime = Time.time + fireRate + Random.Range(0f, 0.2f);

        // Accuracy affected by distance and movement
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        float hitChance = accuracy;

        // Farther = harder to hit
        if (distToPlayer > 15f) hitChance *= 0.7f;
        else if (distToPlayer > 10f) hitChance *= 0.85f;

        // Moving targets harder to hit
        var playerVel = player.GetComponent<CharacterController>();
        if (playerVel != null && playerVel.velocity.magnitude > 2f)
            hitChance *= 0.75f;

        if (Random.value < hitChance)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        // Getting shot reveals player position
        playerDetected = true;
        lastKnownPlayerPos = player != null ? player.position : transform.position;
        lostPlayerTimer = 0f;

        if (health <= 0 && !isDead)
        {
            isDead = true;
            if (RoundManager.Instance != null)
                RoundManager.Instance.OnEnemyKilled();
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
            Random.Range(-3f, 8f), 2.5f, Random.Range(-3f, 8f)
        );
        playerDetected = false;
        canShoot = false;
        reactionTimer = 0f;
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
