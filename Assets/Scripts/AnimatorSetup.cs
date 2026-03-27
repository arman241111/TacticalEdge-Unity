using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

public class AnimatorSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Shooter Animator")]
    public static void CreateAnimator()
    {
        string path = "Assets/Models/SwatGuy/ShooterAnimator.controller";

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsShooting", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsReloading", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsAiming", AnimatorControllerParameterType.Bool);

        var rootStateMachine = controller.layers[0].stateMachine;

        // Find animations
        string basePath = "Assets/Models/SwatGuy/";

        // States
        var idleState = AddState(rootStateMachine, "Idle", basePath + "rifle aiming idle.fbx");
        var walkState = AddState(rootStateMachine, "Walk", basePath + "walking.fbx");
        var runState = AddState(rootStateMachine, "Run", basePath + "rifle run.fbx");
        var crouchState = AddState(rootStateMachine, "Crouch", basePath + "strafe.fbx");
        var shootState = AddState(rootStateMachine, "Shoot", basePath + "firing rifle.fbx");
        var reloadState = AddState(rootStateMachine, "Reload", basePath + "reloading.fbx");
        var jumpState = AddState(rootStateMachine, "Jump", basePath + "rifle jump.fbx");

        rootStateMachine.defaultState = idleState;

        // Transitions: Idle -> Walk
        var t1 = idleState.AddTransition(walkState);
        t1.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
        t1.duration = 0.15f;

        // Idle -> Run
        var t2 = idleState.AddTransition(runState);
        t2.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");
        t2.duration = 0.15f;

        // Walk -> Idle
        var t3 = walkState.AddTransition(idleState);
        t3.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
        t3.duration = 0.15f;

        // Run -> Idle
        var t4 = runState.AddTransition(idleState);
        t4.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
        t4.duration = 0.15f;

        // Any -> Crouch
        var t5 = rootStateMachine.AddAnyStateTransition(crouchState);
        t5.AddCondition(AnimatorConditionMode.If, 0, "IsCrouching");
        t5.duration = 0.15f;

        // Crouch -> Idle
        var t6 = crouchState.AddTransition(idleState);
        t6.AddCondition(AnimatorConditionMode.IfNot, 0, "IsCrouching");
        t6.duration = 0.15f;

        // Any -> Reload
        var t7 = rootStateMachine.AddAnyStateTransition(reloadState);
        t7.AddCondition(AnimatorConditionMode.If, 0, "IsReloading");
        t7.duration = 0.1f;

        // Reload -> Idle
        var t8 = reloadState.AddTransition(idleState);
        t8.AddCondition(AnimatorConditionMode.IfNot, 0, "IsReloading");
        t8.duration = 0.15f;

        // Any -> Jump
        var t9 = rootStateMachine.AddAnyStateTransition(jumpState);
        t9.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        t9.duration = 0.1f;

        // Jump -> Idle
        var t10 = jumpState.AddTransition(idleState);
        t10.hasExitTime = true;
        t10.exitTime = 0.9f;
        t10.duration = 0.15f;

        AssetDatabase.SaveAssets();
        Debug.Log("Shooter Animator Controller created at: " + path);
    }

    static AnimatorState AddState(AnimatorStateMachine sm, string name, string clipPath)
    {
        var state = sm.AddState(name);
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            // Try to load from model
            var objs = AssetDatabase.LoadAllAssetsAtPath(clipPath);
            foreach (var obj in objs)
            {
                if (obj is AnimationClip ac && !ac.name.Contains("__preview__"))
                {
                    clip = ac;
                    break;
                }
            }
        }
        if (clip != null)
        {
            state.motion = clip;
            Debug.Log("Loaded clip: " + name + " from " + clipPath);
        }
        else
        {
            Debug.LogWarning("Clip not found: " + clipPath);
        }
        return state;
    }
#endif
}
