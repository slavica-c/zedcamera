using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ZEDManagerIK : MonoBehaviour
{
    protected Animator animator;

    public bool ikActive = false;
    public Transform LeftFootTransform = null;
    public Transform RightFootTransform = null;

    public Transform sphereRepereR = null;
    public Transform sphereRepereL = null;

    public Transform _rootJoint; 

    private SkeletonHandler skhandler = null;

    public SkeletonHandler Skhandler { get => skhandler; set => skhandler = value; }
    public Vector3 ankleHeightOffset = new Vector3(0, 0.102f, 0);

    [Tooltip("Distance (between ankle and environment under it) under which a foot is considered on the floor.")]
    public float thresholdEnterGroundedState = .14f;
    public float thresholdLeaveGroundedState = .18f;

    public LayerMask raycastDetectionLayers;

    private bool groundedL = false;
    private bool groundedR = false;

    private Vector3 currentPosFootL;
    private Vector3 currentPosFootR;
    //public float dampReachTime = .01f;
    //public float dampMaxSpeed = 1f;

    //public Vector3 velocityTargetL = Vector3.zero;
    //public Vector3 velocityTargetR = Vector3.zero;

    private Vector3 normalL = Vector3.up;
    private Vector3 normalR = Vector3.up;

    private Vector3 currentGroundedPosL = Vector3.zero;
    private Vector3 currentGroundedPosR = Vector3.zero;
    public float groundedFreeDistance = .05f;

    private bool freeL = true;
    private bool freeR = true;

    /**
     * LERP DATA FOR IK TARGETS
     */
    private Vector3 startLerpPosL;
    private Vector3 targetLerpPosL;
    // necessary because Move() will reset it
    private Vector3 curEffectorPosL;
    private float lastTValLerpL;
    private float curTValL;

    private Vector3 startLerpPosR;
    private Vector3 targetLerpPosR;
    // necessary because Move() will reset it
    private Vector3 curEffectorPosR;
    private float lastTValLerpR;
    private float curTValR;

    private float totalLerpTime;
    private float curLerpTimeL;
    private float curLerpTimeR;

    [Tooltip("Frequency of reception of new OD data, in FPS")]
    public float objectDetectionFrequency = 30f;

    [Tooltip("Latency of lerp. 1=no latency; 0=instant movement, no lerp;")]
    public float lerpLatency = 1.2f;

    // true if new targets (OnObjectDetection from camera)
    private bool poseWasUpdated = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentGroundedPosL = LeftFootTransform.position;
        currentGroundedPosR = RightFootTransform.position;
    }

    /**
     * a callback for calculating IK
     * 1) Apply bones rotations 2) Apply root position and rotations. 3) Do Foot IK.
    */
    void OnAnimatorIK()
    {
        if (skhandler)
        {
            // 1) Update target positions and rotations, and apply rotations.
            // This needs to be called each frame, else there will be a jitter between the resting and correct poses of the skeleton.
            skhandler.Move();

            // 2) Move actor position, effectively moving root position. (root relative position is (0,0,0) in the prefab)
            // This way, we have root motion and can easily apply effects following the main gameobject's transform.
            transform.position = skhandler.TargetBodyPositionWithHipOffset;
            transform.rotation = skhandler.TargetBodyOrientation;

            // 3) Manage Foot IK
            if (animator)
            {
                //if the IK is active, set the position and rotation directly to the goal.
                if (ikActive)
                {
                    // Set the right foot target position and rotation, if one has been assigned
                    if (RightFootTransform != null)
                    {
                        // blend weight
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

                        // init/prepare values
                        currentPosFootR = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
                        float rayHitHeight = float.MinValue;
                        Vector3 effectorTargetPos;

                        // move foot target
                        effectorTargetPos = AccumulateLerpVec3(startLerpPosR,targetLerpPosR,ref curTValR, ref curLerpTimeR);
                        Debug.Log("OAIK lerptime: " + curLerpTimeR + " / tval: " + curTValR);

                        Ray ray = new Ray(effectorTargetPos + (Vector3.up * (groundedR ? thresholdLeaveGroundedState : thresholdEnterGroundedState)), Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit hit, 2 * ( groundedR ? thresholdLeaveGroundedState : thresholdEnterGroundedState), raycastDetectionLayers))
                        {
                            // get floor height to adjust foot height if it appears under floor
                            //rayHitHeight = hit.point.y + ankleHeightOffset.y;

                            //
                            effectorTargetPos = hit.point + ankleHeightOffset;
                            normalR = hit.normal;
                            groundedR = true;

                            /*if(HorizontalDist(effectorTargetPos,currentGroundedPosR) < groundedFreeDistance)
                            {
                                effectorTargetPos = currentGroundedPosR;
                                freeR = false;
                            } else
                            {

                                currentGroundedPosR = effectorTargetPos;
                                freeR = true;
                            }*/
                        }
                        else
                        {

                            //freeR = true;
                            groundedR = false;
                            normalR = animator.GetBoneTransform(HumanBodyBones.RightFoot).up;
                            //effectorTargetPos = skhandler.joints[SkeletonHandler.JointType_AnkleRight];
                        }

                        // set IK position and rotation

                        //sphereRepereR.position = effectorTargetPos;
                        //sphereRepereR.position = currentPosFootR;
                        //sphereRepereL.position = effectorTargetPos;

                        //effectorTargetPos = Vector3.Lerp(currentPosFootR, effectorTargetPos, SpecialLerp(totLerpTime, ref curLerpTime,maxLerpSpeed));
                        //effectorTargetPos = Vector3.Lerp(currentPosFootR, effectorTargetPos, flatLerpValue);

                        //effectorTargetPos = LerpDepOnDist(effectorTargetPos,currentPosFootR,lerpMinTvalDistance,lerpMinTval, true);

                        // prevent foot from going below ground
                        //if(effectorTargetPos.y < rayHitHeight) { effectorTargetPos.Set(effectorTargetPos.x, rayHitHeight, effectorTargetPos.z); }

                        // update current effector position because next Move() will reset it to the skeleton pose
                        curEffectorPosR = effectorTargetPos;

                        //effectorTargetPos = Vector3.SmoothDamp(currentPosFootR,effectorTargetPos, ref velocityTargetR, dampReachTime, dampMaxSpeed);
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, effectorTargetPos);
                        animator.SetIKRotation(
                             AvatarIKGoal.RightFoot,
                             Quaternion.FromToRotation(
                                 animator.GetBoneTransform(HumanBodyBones.RightFoot).up, normalL) * animator.GetBoneTransform(HumanBodyBones.RightFoot).rotation);
                    }
                    // Set the left foot target position and rotation, if one has been assigned
                    if (LeftFootTransform != null)
                    {
                        float rayHitHeight = float.MinValue;

                        currentPosFootL = LeftFootTransform.position;

                        // blend weight
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);

                        Vector3 effectorTargetPos;

                        Ray ray = new Ray(skhandler.joints[SkeletonHandler.JointType_AnkleLeft] + (Vector3.up * (groundedL ? thresholdLeaveGroundedState : thresholdEnterGroundedState)), Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit info, 2 * (groundedL ? thresholdLeaveGroundedState : thresholdEnterGroundedState), raycastDetectionLayers))
                        {
                            rayHitHeight = info.point.y + ankleHeightOffset.y;

                            effectorTargetPos = info.point + ankleHeightOffset;
                            normalL = info.normal;
                            groundedL = true;
                            if (HorizontalDist(effectorTargetPos, currentGroundedPosL) < groundedFreeDistance)
                            {
                                freeL = false;
                                effectorTargetPos = currentGroundedPosL;
                            } else
                            {
                                freeL = true;
                                currentGroundedPosL = effectorTargetPos;
                            }
                        }
                        else
                        {
                            freeL = true;
                            groundedL = false;
                            normalL = animator.GetBoneTransform(HumanBodyBones.LeftFoot).up;
                            effectorTargetPos = skhandler.joints[SkeletonHandler.JointType_AnkleLeft];
                        }

                        // set IK position and rotation
                        //sphereRepereL.position = effectorTargetPos;
                        //effectorTargetPos = Vector3.Lerp(currentPosFootL, effectorTargetPos, .7f);
                        //effectorTargetPos = Vector3.SmoothDamp(currentPosFootL, effectorTargetPos, ref velocityTargetL, .5f);

                        //effectorTargetPos = LerpDepOnDist(effectorTargetPos, currentPosFootL, lerpMinTvalDistance, lerpMinTval, freeL);

                        // prevent foot from going below ground
                        if (effectorTargetPos.y < rayHitHeight) { effectorTargetPos.Set(effectorTargetPos.x, rayHitHeight, effectorTargetPos.z); }

                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, effectorTargetPos);
                        animator.SetIKRotation(
                            AvatarIKGoal.LeftFoot, 
                            Quaternion.FromToRotation(
                                animator.GetBoneTransform(HumanBodyBones.LeftFoot).up, normalL) * animator.GetBoneTransform(HumanBodyBones.LeftFoot).rotation);
                    }

                }

                //if the IK is not active, set the position and rotation of the hand and head back to the original position
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                }
            }
        }
    }

    private float SpecialLerp(float totalTime, ref float currentTime, float maxSpeed)
    {
        currentTime += Time.deltaTime;

        float t = currentTime / totalTime;

        return Mathf.Sin(t * Mathf.PI * 0.5f);
    }

    private Vector3 LerpDepOnDist(Vector3 targetEffectorPos, Vector3 currentBonePos, float distVitMin, float minTVal, bool freeFoot)
    {
        Vector3 effectorPosWithLerp = currentBonePos;

        if (freeFoot)
        {
            float tval = 0;
            float tvalmax = 1f;

            float dist = Vector3.Distance(targetEffectorPos, currentBonePos);
            if (dist >= distVitMin)
            {
                tval = minTVal;
            }
            else
            {
                try
                {
                    tval = (dist / distVitMin);
                    //tval = Mathf.Cos(tval * Mathf.PI * 0.5f);
                    tval = tvalmax - (tvalmax - minTVal) * (dist / distVitMin);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            effectorPosWithLerp = Vector3.Lerp(currentBonePos, targetEffectorPos, tval);
        }
        
        return effectorPosWithLerp;
    }

    // computes distance between the projections of vec1 and vec2 on an horizontal plane
    private float HorizontalDist(Vector3 vec1, Vector3 vec2)
    {
        return Vector2.Distance(new Vector2(vec1.x, vec1.z), new Vector2(vec2.x, vec2.z));
    }

    // Should be called each time the skeleton data is changed in the handler
    // prepares new lerp data.
    // Checks distance to filter feet parasite movements on floor.
    public void PoseWasUpdated()
    {
        startLerpPosL = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        targetLerpPosL = skhandler.joints[SkeletonHandler.JointType_AnkleLeft];
        targetLerpPosL = (groundedL && HorizontalDist(startLerpPosL, targetLerpPosL) < groundedFreeDistance) 
            ? startLerpPosL 
            : skhandler.joints[SkeletonHandler.JointType_AnkleLeft];
        lastTValLerpL = 0f;

        startLerpPosR = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
        targetLerpPosR = skhandler.joints[SkeletonHandler.JointType_AnkleRight];
        targetLerpPosR = (groundedR && HorizontalDist(startLerpPosR, targetLerpPosR) < groundedFreeDistance)
            ? startLerpPosR
            : skhandler.joints[SkeletonHandler.JointType_AnkleRight];
        lastTValLerpR = 0f;

        poseWasUpdated = true;

        // define totallerptime = 1/ODFrequency
        // reset curlerptime
        curLerpTimeL = 0;
        curLerpTimeR = 0;
        curTValL = 0;
        curTValR = 0;
        totalLerpTime = 1 / objectDetectionFrequency;
        totalLerpTime *= lerpLatency;
        Debug.Log("---------------------------------- totalLerpTime: " + totalLerpTime);
    }

    private Vector3 AccumulateLerpVec3(Vector3 startPos, Vector3 targetPos, ref float curTVal, ref float currentLerpTime)
    {
        currentLerpTime += Time.deltaTime;
        float t = currentLerpTime / totalLerpTime;
        t = (Mathf.Sin(t * Mathf.PI * 0.5f));
        curTVal = t;
        return Vector3.Lerp(startPos, targetPos, t);
    }

    private void OnDrawGizmos()
    {
        if (groundedR) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(RightFootTransform.position + (Vector3.up * thresholdEnterGroundedState), RightFootTransform.position - (Vector3.up * thresholdEnterGroundedState));

        if (groundedL) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(LeftFootTransform.position + (Vector3.up * thresholdEnterGroundedState), LeftFootTransform.position - (Vector3.up * thresholdEnterGroundedState));

        Gizmos.color = Color.blue;
        Gizmos.color = Color.magenta;
        Gizmos.color = Color.yellow;
    }
}
