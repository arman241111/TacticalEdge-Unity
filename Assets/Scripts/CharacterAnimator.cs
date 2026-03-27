using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    private EnemyAI enemyAI;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsReloadingHash = Animator.StringToHash("IsReloading");

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        enemyAI = GetComponent<EnemyAI>();
    }

    void Update()
    {
        if (animator == null) return;

        float speed = 0f;
        if (controller != null)
        {
            Vector3 vel = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            speed = vel.magnitude;
        }

        animator.SetFloat(SpeedHash, speed);

        // For player
        var pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            animator.SetBool(IsWalkingHash, pm.isWalking && speed > 0.1f);
            animator.SetBool(IsRunningHash, !pm.isWalking && !pm.isCrouching && speed > 2f);
            animator.SetBool(IsCrouchingHash, pm.isCrouching);
        }

        // For enemy - animate based on speed
        if (enemyAI != null)
        {
            bool moving = speed > 0.5f;
            bool running = speed > 2f;
            animator.SetBool(IsWalkingHash, moving && !running);
            animator.SetBool(IsRunningHash, running);
        }

        // Reload
        var shoot = GetComponent<SimpleShoot>();
        if (shoot != null)
            animator.SetBool(IsReloadingHash, shoot.isReloading);
    }
}
