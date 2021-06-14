//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class SkeletonHandler : ScriptableObject
{

    private const int
    // JointType
    JointType_Head = 0,
    JointType_Neck = 1,
    JointType_ShoulderRight = 2,
    JointType_ElbowRight = 3,
    JointType_WristRight = 4,
    JointType_ShoulderLeft = 5,
    JointType_ElbowLeft = 6,
    JointType_WristLeft = 7,
    JointType_HipRight = 8,
    JointType_KneeRight = 9,
    JointType_AnkleRight = 10,
    JointType_HipLeft = 11,
    JointType_KneeLeft = 12,
    JointType_AnkleLeft = 13,
    JointType_EyesRight = 14,
    JointType_EyesLeft = 15,
    JointType_HearRight = 16,
    JointType_HearLeft = 17,
    JointType_SpineBase = 18,  //Not in the list but created from 8 + 11
    JointType_Nose = 19,
    jointCount = 20;


    private static readonly int[] bonesList = new int[] {
    JointType_SpineBase, JointType_Neck,                 // Spine                     // Neck
    JointType_HipLeft, JointType_HipRight,
    JointType_HearRight, JointType_EyesRight,
    JointType_HearLeft, JointType_EyesLeft,
    JointType_EyesRight, JointType_Nose,
    JointType_EyesLeft, JointType_Nose,
    JointType_Nose, JointType_Neck,
	// left
    JointType_Neck, JointType_ShoulderLeft,
    JointType_ShoulderLeft, JointType_ElbowLeft,         // LeftUpperArm
	JointType_ElbowLeft, JointType_WristLeft,            // LeftLowerArm
	JointType_HipLeft, JointType_KneeLeft,               // LeftUpperLeg
	JointType_KneeLeft, JointType_AnkleLeft,             // LeftLowerLeg6
	// right
    JointType_Neck, JointType_ShoulderRight,
    JointType_ShoulderRight, JointType_ElbowRight,       // RightUpperArm
	JointType_ElbowRight, JointType_WristRight,          // RightLowerArm
	JointType_HipRight, JointType_KneeRight,             // RightUpperLeg
	JointType_KneeRight, JointType_AnkleRight,           // RightLowerLeg
	};

    private static readonly int[] sphereList = new int[] {
    JointType_SpineBase,
    JointType_Neck,
    JointType_HipLeft,
    JointType_HipRight,
    JointType_ShoulderLeft,
    JointType_ElbowLeft,
    JointType_WristLeft,
    JointType_KneeLeft,
    JointType_AnkleLeft,
    JointType_ShoulderRight,
    JointType_ElbowRight,
    JointType_WristRight,
    JointType_KneeRight,
    JointType_AnkleRight,
    JointType_EyesLeft,
    JointType_EyesRight,
    JointType_HearRight,
    JointType_HearLeft,
    JointType_Nose
    };

    public Vector3[] joint = new Vector3[jointCount];
    
    GameObject skeleton;
    public GameObject[] bones;
    public GameObject[] spheres;

    private static HumanBodyBones[] humanBone = new HumanBodyBones[] {
    HumanBodyBones.Hips,
    HumanBodyBones.Spine,
    HumanBodyBones.Chest,
    HumanBodyBones.UpperChest,
    HumanBodyBones.LeftShoulder,
    HumanBodyBones.LeftUpperArm,
    HumanBodyBones.LeftLowerArm,
    HumanBodyBones.LeftHand, // Left Wrist
    HumanBodyBones.LastBone, // Left Hand
    HumanBodyBones.LastBone, // Left HandTip
    HumanBodyBones.RightShoulder,
    HumanBodyBones.RightUpperArm,
    HumanBodyBones.RightLowerArm,
    HumanBodyBones.RightHand, // Right Wrist
    HumanBodyBones.LastBone, // Right Hand
    HumanBodyBones.LastBone, // Right HandTip
    HumanBodyBones.LeftUpperLeg,
    HumanBodyBones.LeftLowerLeg,
    HumanBodyBones.LeftFoot,
    HumanBodyBones.LeftToes,
    HumanBodyBones.RightUpperLeg,
    HumanBodyBones.RightLowerLeg,
    HumanBodyBones.RightFoot,
    HumanBodyBones.RightToes,
    HumanBodyBones.Neck,
    HumanBodyBones.Head,
    HumanBodyBones.LastBone, // Nose
    HumanBodyBones.LastBone, // Left Eye
    HumanBodyBones.LastBone, // Left Ear
    HumanBodyBones.LastBone, // Right Eye
    HumanBodyBones.LastBone, // Right Ear
    };

    private Color[] colors = new Color[]{
    new Color( 232.0f / 255.0f, 176.0f / 255.0f,59.0f / 255.0f),
    new Color(175.0f / 255.0f, 208.0f / 255.0f,25.0f / 255.0f),
    new Color(102.0f / 255.0f / 255.0f, 205.0f / 255.0f,105.0f / 255.0f),
    new Color(185.0f / 255.0f, 0.0f / 255.0f,255.0f / 255.0f),
    new Color(99.0f / 255.0f, 107.0f / 255.0f,252.0f / 255.0f),
    new Color(252.0f / 255.0f, 225.0f / 255.0f, 8.0f / 255.0f),
    new Color(167.0f / 255.0f, 130.0f / 255.0f, 141.0f / 255.0f),
    new Color(194.0f / 255.0f, 72.0f / 255.0f, 113.0f / 255.0f)
    };

    private GameObject humanoid;
    private Dictionary<HumanBodyBones, RigBone> rigBone = null;
    private Dictionary<HumanBodyBones, Quaternion> rigBoneTarget = null;

    private List<GameObject> sphere = new List<GameObject>();// = GameObject.CreatePrimitive (PrimitiveType.Sphere);

    private Vector3 targetBodyPosition = new Vector3(0.0f, 0.0f, 0.0f);
    public Quaternion targetBodyOrientation = Quaternion.identity;

    private bool isInit = false;

    public float SpineHeight = 0;
    private float smoothFactor = 0.5f;
    /// <summary>
    /// Sets the smooth factor.
    /// </summary>
    /// <param name="smooth">Smooth.</param>
    public void SetSmoothFactor(float smooth)
    {
        smoothFactor = smooth;
    }

    /// <summary>
    /// Create the avatar control
    /// </summary>
    /// <param name="h">The height.</param>
    public void Create(GameObject h)
    {
        humanoid = (GameObject)Instantiate(h, Vector3.zero, Quaternion.identity);
        SpineHeight =  humanoid.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position.y;

        var invisiblelayer = LayerMask.NameToLayer("tagInvisibleToZED");
        humanoid.layer = invisiblelayer;

        foreach (Transform child in humanoid.transform)
        {
            child.gameObject.layer = invisiblelayer;
        }

        rigBone = new Dictionary<HumanBodyBones, RigBone>();
        rigBoneTarget = new Dictionary<HumanBodyBones, Quaternion>();
        foreach (HumanBodyBones bone in humanBone)
        {
            rigBone[bone] = new RigBone(humanoid, bone);
            rigBoneTarget[bone] = Quaternion.identity;
        }
    }

    public void Destroy()
    {
        GameObject.Destroy(humanoid);
        GameObject.Destroy(skeleton);
        rigBone.Clear();
        rigBoneTarget.Clear();
        Array.Clear(bones, 0, bones.Length);
        Array.Clear(spheres, 0, spheres.Length);
    }

    /// <summary>
    /// Function that handles the humanoid position, rotation and bones movement
    /// </summary>
    /// <param name="position_center">Position center.</param>
    private void setHumanPoseControl(Vector3 rootPosition, Quaternion rootRotation, Quaternion[] jointsRotation)
    {
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.Hips] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.Hips)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.Spine] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.Spine)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.Chest] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.Chest)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.UpperChest] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.UpperChest)];

        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightShoulder] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightShoulder)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightUpperArm] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightUpperArm)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightLowerArm] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightLowerArm)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightHand] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightHand)];

        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftShoulder] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftShoulder)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftUpperArm] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftUpperArm)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftLowerArm] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftLowerArm)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftHand] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftHand)];

        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.Neck] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.Neck)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.Head] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.Head)];

        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightUpperLeg] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightUpperLeg)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightLowerLeg] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightLowerLeg)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.RightFoot] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.RightFoot)];

        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftUpperLeg] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftUpperLeg)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftLowerLeg] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftLowerLeg)];
        if (rigBone[HumanBodyBones.Hips].transform)
            rigBoneTarget[HumanBodyBones.LeftFoot] = jointsRotation[Array.IndexOf(humanBone, HumanBodyBones.LeftFoot)];

        targetBodyOrientation = rootRotation;
        targetBodyPosition = rootPosition;
    }

    public void initSkeleton(int person_id)
    {
        bones = new GameObject[bonesList.Length / 2];
        spheres = new GameObject[sphereList.Length];
        skeleton = new GameObject();
        skeleton.name = "Skeleton_ID_" + person_id;
        float width = 0.025f;

        Color color = colors[person_id % colors.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.GetComponent<Renderer>().material.color = color;
            cylinder.transform.parent = skeleton.transform;
            bones[i] = cylinder;
        }
        for (int j = 0; j < spheres.Length; j++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.transform.localScale = new Vector3(width * 2, width * 2, width * 2);
            sphere.transform.parent = skeleton.transform;
            spheres[j] = sphere;
        }
    }

    void updateSkeleton()
    {
        float width = 0.025f;

        for (int j = 0; j < spheres.Length; j++)
        {
            if (ZEDSupportFunctions.IsVector3NaN(joint[sphereList[j]]))
            {
                spheres[j].transform.position = Vector3.zero;
                spheres[j].SetActive(false);
            }
            else
            {
                spheres[j].transform.position = joint[sphereList[j]];
                spheres[j].SetActive(true);
            }
        }

        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 start = spheres[Array.IndexOf(sphereList, bonesList[2 * i])].transform.position;
            Vector3 end = spheres[Array.IndexOf(sphereList, bonesList[2 * i + 1])].transform.position;

            if (start == Vector3.zero || end == Vector3.zero)
            {
                bones[i].SetActive(false);
                continue;
            }

            bones[i].SetActive(true);
            Vector3 offset = end - start;
            Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
            Vector3 position = start + (offset / 2.0f);

            bones[i].transform.position = position;
            bones[i].transform.up = offset;
            bones[i].transform.localScale = scale;

        }
    }

    /// <summary>
    /// Sets the avatar control with joint position.
    /// </summary>
    /// <param name="jt">Jt.</param>
    /// <param name="position_center">Position center.</param>
    public void setControlWithJointPosition(Vector3[] jointsPosition, Vector3 position_center, Quaternion[] jointsRotation, Quaternion rootRotation, bool useAvatar)
    {
        for (int i = 0; i < jointCount; i++)
        {
            joint[i] = new Vector3(jointsPosition[i].x, jointsPosition[i].y, jointsPosition[i].z);
        }

        humanoid.SetActive(useAvatar);
        skeleton.SetActive(!useAvatar);

        if (useAvatar) setHumanPoseControl(position_center, rootRotation, jointsRotation);
        else updateSkeleton();
    }

    /// <summary>
    /// For Debug only. Set the joint position as sphere.
    /// </summary>
    /// <param name="jt">Jt.</param>
    public void setJointSpherePoint(Vector3[] jt)
    {
        if (sphere.Count != 18)
        {
            for (int i = 0; i < jointCount; i++)
            {
                sphere.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            }
        }

        for (int i = 0; i < jointCount; i++)
        {
            if (ZEDSupportFunctions.IsVector3NaN(joint[i])) continue;

            joint[i] = new Vector3(jt[i].x, jt[i].y, jt[i].z);

            sphere[i].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            sphere[i].transform.position = joint[i];
        }
    }

    /// <summary>
    /// Set Humanoid position. Called in Update() function
    /// </summary>
    public void MoveAvatar()
    {
        if (isInit)
        {
            humanoid.transform.position = smoothFactor != 0f ? Vector3.Lerp(humanoid.transform.position, targetBodyPosition, smoothFactor) : targetBodyPosition;
            humanoid.transform.rotation = smoothFactor != 0f ? Quaternion.Lerp(humanoid.transform.rotation, targetBodyOrientation, smoothFactor) : targetBodyOrientation;
        }
        else
        {
            humanoid.transform.position = targetBodyPosition;
            humanoid.transform.localRotation = targetBodyOrientation;
            isInit = true;
        }

        foreach (HumanBodyBones bone in humanBone)
        {
            if (smoothFactor != 0f)
                rigBone[bone].transform.localRotation = Quaternion.Slerp(rigBone[bone].transform.rotation, rigBoneTarget[bone], smoothFactor);
            else
                rigBone[bone].transform.localRotation = rigBoneTarget[bone];
        }
    }

    /// <summary>
    /// Update Engine function (move this avatar)
    /// </summary>
    public void Move()
    {
        MoveAvatar();
    }



}
