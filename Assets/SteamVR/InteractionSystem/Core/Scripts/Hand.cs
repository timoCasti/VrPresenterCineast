﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The hands used by the player in the vr interaction system
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    // Links with an appropriate SteamVR controller and facilitates
    // interactions with objects in the virtual world.
    //-------------------------------------------------------------------------
    public class Hand : MonoBehaviour
    {
        // The flags used to determine how an object is attached to the hand.
        [Flags]
        public enum AttachmentFlags
        {
            SnapOnAttach =
                1 << 0, // The object should snap to the position of the specified attachment point on the hand.
            DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
            DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
            ParentToHand = 1 << 3, // The object will be parented to the hand.

            VelocityMovement =
                1 << 4, // The object will attempt to move to match the position and rotation of the hand.
            TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
            TurnOffGravity = 1 << 6, // The object will not respond to external physics.

            AllowSidegrade =
                1 << 7 // The object is able to switch from a pinch grab to a grip grab. Decreases likelyhood of a good throw but also decreases likelyhood of accidental drop
        }

        public const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
                                                              AttachmentFlags.DetachOthers |
                                                              AttachmentFlags.DetachFromOtherHand |
                                                              AttachmentFlags.TurnOnKinematic |
                                                              AttachmentFlags.SnapOnAttach;

        private const int ColliderArraySize = 16;

        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;

        private Interactable _hoveringInteractable;

        private GameObject applicationLostFocusObject;

        private readonly List<AttachedObject> attachedObjects = new List<AttachedObject>();
        public string controllerHoverComponent = "tip";
        public float controllerHoverRadius = 0.075f;

        private TextMesh debugText;
        public SteamVR_Skeleton_JointIndexEnum fingerJointHover = SteamVR_Skeleton_JointIndexEnum.indexTip;
        public float fingerJointHoverRadius = 0.025f;

        [SteamVR_DefaultAction("GrabGrip")] public SteamVR_Action_Boolean grabGripAction;

        [SteamVR_DefaultAction("GrabPinch")] public SteamVR_Action_Boolean grabPinchAction;

        public SteamVR_Input_Sources handType;

        [SteamVR_DefaultAction("Haptic")] public SteamVR_Action_Vibration hapticAction;

        protected RenderModel hoverhighlightRenderModel;
        public LayerMask hoverLayerMask = -1;
        public float hoverSphereRadius = 0.05f;
        public Transform hoverSphereTransform;
        public float hoverUpdateInterval = 0.1f;

        private SteamVR_Events.Action inputFocusAction;
        protected RenderModel mainRenderModel;

        public Camera noSteamVRFallbackCamera;
        private float noSteamVRFallbackInteractorDistance = -1.0f;
        public float noSteamVRFallbackMaxDistanceNoItem = 10.0f;
        public float noSteamVRFallbackMaxDistanceWithItem = 0.5f;

        [Tooltip("A transform on the hand to center attached objects on")]
        public Transform objectAttachmentPoint;

        public Hand otherHand;
        private Collider[] overlappingColliders;

        private Player playerInstance;
        private int prevOverlappingColliders;

        public GameObject renderModelPrefab;
        protected List<RenderModel> renderModels = new List<RenderModel>();
        public bool showDebugInteractables;

        public bool showDebugText;
        public bool spewDebugText;

        public SteamVR_Behaviour_Pose trackedObject;

        [SteamVR_DefaultAction("InteractUI")] public SteamVR_Action_Boolean uiInteractAction;

        public bool useControllerHoverComponent = true;

        public bool useFingerJointHover = true;

        public bool useHoverSphere = true;

        public ReadOnlyCollection<AttachedObject> AttachedObjects
        {
            get { return attachedObjects.AsReadOnly(); }
        }

        public bool hoverLocked { get; private set; }

        public bool isActive
        {
            get { return trackedObject.isActive; }
        }

        public bool isPoseValid
        {
            get { return trackedObject.isValid; }
        }


        //-------------------------------------------------
        // The Interactable object this Hand is currently hovering over
        //-------------------------------------------------
        public Interactable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                if (_hoveringInteractable != value)
                {
                    if (_hoveringInteractable != null)
                    {
                        if (spewDebugText)
                            HandDebugLog("HoverEnd " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage("OnHandHoverEnd", this,
                            SendMessageOptions.DontRequireReceiver);

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverEnd message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                            BroadcastMessage("OnParentHandHoverEnd", _hoveringInteractable,
                                SendMessageOptions
                                    .DontRequireReceiver); // let objects attached to the hand know that a hover has ended
                    }

                    _hoveringInteractable = value;

                    if (_hoveringInteractable != null)
                    {
                        if (spewDebugText)
                            HandDebugLog("HoverBegin " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage("OnHandHoverBegin", this,
                            SendMessageOptions.DontRequireReceiver);

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverBegin message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                            BroadcastMessage("OnParentHandHoverBegin", _hoveringInteractable,
                                SendMessageOptions
                                    .DontRequireReceiver); // let objects attached to the hand know that a hover has begun
                    }
                }
            }
        }


        //-------------------------------------------------
        // Active GameObject attached to this Hand
        //-------------------------------------------------
        public GameObject currentAttachedObject
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0) return attachedObjects[attachedObjects.Count - 1].attachedObject;

                return null;
            }
        }

        public AttachedObject? currentAttachedObjectInfo
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0) return attachedObjects[attachedObjects.Count - 1];

                return null;
            }
        }

        public void ShowController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(true, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetControllerVisibility(true, permanent);
        }

        public void HideController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(false, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetControllerVisibility(false, permanent);
        }

        public void ShowSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(true, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetHandVisibility(true, permanent);
        }

        public void HideSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(false, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetHandVisibility(false, permanent);
        }

        public void Show()
        {
            SetVisibility(true);
        }

        public void Hide()
        {
            SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetVisibility(visible);
        }

        public void SetSkeletonRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            for (var renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
                renderModels[renderModelIndex].SetSkeletonRangeOfMotion(newRangeOfMotion, blendOverSeconds);
        }

        public void SetTemporarySkeletonRangeOfMotion(SkeletalMotionRangeChange temporaryRangeOfMotionChange,
            float blendOverSeconds = 0.1f)
        {
            for (var renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
                renderModels[renderModelIndex]
                    .SetTemporarySkeletonRangeOfMotion(temporaryRangeOfMotionChange, blendOverSeconds);
        }

        public void ResetTemporarySkeletonRangeOfMotion(float blendOverSeconds = 0.1f)
        {
            for (var renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
                renderModels[renderModelIndex].ResetTemporarySkeletonRangeOfMotion(blendOverSeconds);
        }

        public void SetAnimationState(int stateValue)
        {
            for (var renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
                renderModels[renderModelIndex].SetAnimationState(stateValue);
        }

        public void StopAnimation()
        {
            for (var renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
                renderModels[renderModelIndex].StopAnimation();
        }


        //-------------------------------------------------
        // Attach a GameObject to this GameObject
        //
        // objectToAttach - The GameObject to attach
        // flags - The flags to use for attaching the object
        // attachmentPoint - Name of the GameObject in the hierarchy of this Hand which should act as the attachment point for this GameObject
        //-------------------------------------------------
        public void AttachObject(GameObject objectToAttach, GrabTypes grabbedWithType,
            AttachmentFlags flags = defaultAttachmentFlags, Transform attachmentOffset = null)
        {
            var attachedObject = new AttachedObject();
            attachedObject.attachmentFlags = flags;
            attachedObject.attachedOffsetTransform = attachmentOffset;


            if (flags == 0) flags = defaultAttachmentFlags;

            //Make sure top object on stack is non-null
            CleanUpAttachedObjectStack();

            //Detach the object if it is already attached so that it can get re-attached at the top of the stack
            if (ObjectIsAttached(objectToAttach))
                DetachObject(objectToAttach);

            //Detach from the other hand if requested
            if (attachedObject.HasAttachFlag(AttachmentFlags.DetachFromOtherHand))
                otherHand.DetachObject(objectToAttach);

            if (attachedObject.HasAttachFlag(AttachmentFlags.DetachOthers))
                while (attachedObjects.Count > 0)
                    DetachObject(attachedObjects[0].attachedObject);

            if (currentAttachedObject)
                currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);

            attachedObject.attachedObject = objectToAttach;
            attachedObject.interactable = objectToAttach.GetComponent<Interactable>();
            attachedObject.handAttachmentPointTransform = transform;

            if (attachedObject.interactable != null)
            {
                if (attachedObject.interactable.useHandObjectAttachmentPoint)
                    attachedObject.handAttachmentPointTransform = objectAttachmentPoint;

                if (attachedObject.interactable.hideHandOnAttach)
                    Hide();

                if (attachedObject.interactable.hideSkeletonOnAttach && mainRenderModel != null &&
                    mainRenderModel.displayHandByDefault)
                    HideSkeleton();

                if (attachedObject.interactable.hideControllerOnAttach && mainRenderModel != null &&
                    mainRenderModel.displayControllerByDefault)
                    HideController();

                if (attachedObject.interactable.handAnimationOnPickup != 0)
                    SetAnimationState(attachedObject.interactable.handAnimationOnPickup);

                if (attachedObject.interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                    SetTemporarySkeletonRangeOfMotion(attachedObject.interactable.setRangeOfMotionOnPickup);
            }

            attachedObject.originalParent = objectToAttach.transform.parent != null
                ? objectToAttach.transform.parent.gameObject
                : null;

            attachedObject.attachedRigidbody = objectToAttach.GetComponent<Rigidbody>();
            if (attachedObject.attachedRigidbody != null)
            {
                if (attachedObject.interactable.attachedToHand != null) //already attached to another hand
                {
                    //if it was attached to another hand, get the flags from that hand

                    for (var attachedIndex = 0;
                        attachedIndex < attachedObject.interactable.attachedToHand.attachedObjects.Count;
                        attachedIndex++)
                    {
                        var attachedObjectInList =
                            attachedObject.interactable.attachedToHand.attachedObjects[attachedIndex];
                        if (attachedObjectInList.interactable == attachedObject.interactable)
                        {
                            attachedObject.attachedRigidbodyWasKinematic =
                                attachedObjectInList.attachedRigidbodyWasKinematic;
                            attachedObject.attachedRigidbodyUsedGravity =
                                attachedObjectInList.attachedRigidbodyUsedGravity;
                            attachedObject.originalParent = attachedObjectInList.originalParent;
                        }
                    }
                }
                else
                {
                    attachedObject.attachedRigidbodyWasKinematic = attachedObject.attachedRigidbody.isKinematic;
                    attachedObject.attachedRigidbodyUsedGravity = attachedObject.attachedRigidbody.useGravity;
                }
            }

            attachedObject.grabbedWithType = grabbedWithType;

            if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
            {
                //Parent the object to the hand
                objectToAttach.transform.parent = transform;
                attachedObject.isParentedToHand = true;
            }
            else
            {
                attachedObject.isParentedToHand = false;
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
            {
                if (attachmentOffset != null)
                {
                    //offset the object from the hand by the positional and rotational difference between the offset transform and the attached object
                    var rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) *
                                  objectToAttach.transform.rotation;
                    objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;

                    var posDiff = objectToAttach.transform.position - attachmentOffset.transform.position;
                    objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position + posDiff;
                }
                else
                {
                    //snap the object to the center of the attach point
                    objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation;
                    objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position;
                }

                var followPoint = objectToAttach.transform;
                if (attachedObject.interactable != null && attachedObject.interactable.handFollowTransform != null)
                    followPoint = attachedObject.interactable.handFollowTransform;

                attachedObject.initialPositionalOffset =
                    attachedObject.handAttachmentPointTransform.InverseTransformPoint(followPoint.position);
                attachedObject.initialRotationalOffset =
                    Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * followPoint.rotation;
            }
            else
            {
                if (attachmentOffset != null)
                {
                    //get the initial positional and rotational offsets between the hand and the offset transform
                    var rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) *
                                  objectToAttach.transform.rotation;
                    var targetRotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;
                    var rotationPositionBy = targetRotation * Quaternion.Inverse(objectToAttach.transform.rotation);

                    var posDiff = rotationPositionBy * objectToAttach.transform.position -
                                  rotationPositionBy * attachmentOffset.transform.position;

                    attachedObject.initialPositionalOffset =
                        attachedObject.handAttachmentPointTransform.InverseTransformPoint(
                            attachedObject.handAttachmentPointTransform.position + posDiff);
                    attachedObject.initialRotationalOffset =
                        Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) *
                        (attachedObject.handAttachmentPointTransform.rotation * rotDiff);
                }
                else
                {
                    attachedObject.initialPositionalOffset =
                        attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform
                            .position);
                    attachedObject.initialRotationalOffset =
                        Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) *
                        objectToAttach.transform.rotation;
                }
            }


            if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
                if (attachedObject.attachedRigidbody != null)
                {
                    attachedObject.collisionDetectionMode = attachedObject.attachedRigidbody.collisionDetectionMode;
                    if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                        attachedObject.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                    attachedObject.attachedRigidbody.isKinematic = true;
                }

            if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
                if (attachedObject.attachedRigidbody != null)
                    attachedObject.attachedRigidbody.useGravity = false;

            attachedObjects.Add(attachedObject);

            UpdateHovering();

            if (spewDebugText)
                HandDebugLog("AttachObject " + objectToAttach);
            objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
        }

        public bool ObjectIsAttached(GameObject go)
        {
            for (var attachedIndex = 0; attachedIndex < attachedObjects.Count; attachedIndex++)
                if (attachedObjects[attachedIndex].attachedObject == go)
                    return true;

            return false;
        }

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this Hand
        //
        // objectToDetach - The GameObject to detach from this Hand
        //-------------------------------------------------
        public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
        {
            var index = attachedObjects.FindIndex(l => l.attachedObject == objectToDetach);
            if (index != -1)
            {
                if (spewDebugText)
                    HandDebugLog("DetachObject " + objectToDetach);

                var prevTopObject = currentAttachedObject;


                if (attachedObjects[index].interactable != null)
                {
                    if (attachedObjects[index].interactable.hideHandOnAttach)
                        Show();

                    if (attachedObjects[index].interactable.hideSkeletonOnAttach && mainRenderModel != null &&
                        mainRenderModel.displayHandByDefault)
                        ShowSkeleton();

                    if (attachedObjects[index].interactable.hideControllerOnAttach && mainRenderModel != null &&
                        mainRenderModel.displayControllerByDefault)
                        ShowController();

                    if (attachedObjects[index].interactable.handAnimationOnPickup != 0)
                        StopAnimation();

                    if (attachedObjects[index].interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                        ResetTemporarySkeletonRangeOfMotion();
                }

                Transform parentTransform = null;
                if (attachedObjects[index].isParentedToHand)
                {
                    if (restoreOriginalParent && attachedObjects[index].originalParent != null)
                        parentTransform = attachedObjects[index].originalParent.transform;
                    attachedObjects[index].attachedObject.transform.parent = parentTransform;
                }

                if (attachedObjects[index].HasAttachFlag(AttachmentFlags.TurnOnKinematic))
                    if (attachedObjects[index].attachedRigidbody != null)
                    {
                        attachedObjects[index].attachedRigidbody.isKinematic =
                            attachedObjects[index].attachedRigidbodyWasKinematic;
                        attachedObjects[index].attachedRigidbody.collisionDetectionMode =
                            attachedObjects[index].collisionDetectionMode;
                    }

                if (attachedObjects[index].HasAttachFlag(AttachmentFlags.TurnOffGravity))
                    if (attachedObjects[index].attachedRigidbody != null)
                        attachedObjects[index].attachedRigidbody.useGravity =
                            attachedObjects[index].attachedRigidbodyUsedGravity;

                if (attachedObjects[index].interactable == null || attachedObjects[index].interactable != null &&
                    attachedObjects[index].interactable.isDestroying == false)
                {
                    attachedObjects[index].attachedObject.SetActive(true);
                    attachedObjects[index].attachedObject.SendMessage("OnDetachedFromHand", this,
                        SendMessageOptions.DontRequireReceiver);
                    attachedObjects.RemoveAt(index);
                }
                else
                {
                    attachedObjects.RemoveAt(index);
                }

                CleanUpAttachedObjectStack();

                var newTopObject = currentAttachedObject;

                hoverLocked = false;


                //Give focus to the top most object on the stack if it changed
                if (newTopObject != null && newTopObject != prevTopObject)
                {
                    newTopObject.SetActive(true);
                    newTopObject.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
                }
            }

            CleanUpAttachedObjectStack();

            if (mainRenderModel != null)
                mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.MatchHandToTransform(hoverhighlightRenderModel.transform);
        }


        //-------------------------------------------------
        // Get the world velocity of the VR Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectVelocity(float timeOffset = 0)
        {
            if (isActive)
            {
                if (timeOffset == 0)
                    return Player.instance.trackingOriginTransform.TransformVector(trackedObject.GetVelocity());

                Vector3 velocity;
                Vector3 angularVelocity;

                var success = trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                if (success)
                    return Player.instance.trackingOriginTransform.TransformVector(velocity);
            }

            return Vector3.zero;
        }


        //-------------------------------------------------
        // Get the world space angular velocity of the VR Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectAngularVelocity(float timeOffset = 0)
        {
            if (isActive)
            {
                if (timeOffset == 0)
                    return Player.instance.trackingOriginTransform.TransformDirection(
                        trackedObject.GetAngularVelocity());

                Vector3 velocity;
                Vector3 angularVelocity;

                var success = trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                if (success)
                    return Player.instance.trackingOriginTransform.TransformDirection(angularVelocity);
            }

            return Vector3.zero;
        }

        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            trackedObject.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            velocity = Player.instance.trackingOriginTransform.TransformVector(velocity);
            angularVelocity = Player.instance.trackingOriginTransform.TransformDirection(angularVelocity);
        }


        //-------------------------------------------------
        private void CleanUpAttachedObjectStack()
        {
            attachedObjects.RemoveAll(l => l.attachedObject == null);
        }


        //-------------------------------------------------
        protected virtual void Awake()
        {
            inputFocusAction = SteamVR_Events.InputFocusAction(OnInputFocus);

            if (hoverSphereTransform == null)
                hoverSphereTransform = transform;

            if (objectAttachmentPoint == null)
                objectAttachmentPoint = transform;

            applicationLostFocusObject = new GameObject("_application_lost_focus");
            applicationLostFocusObject.transform.parent = transform;
            applicationLostFocusObject.SetActive(false);

            if (trackedObject == null)
                trackedObject = gameObject.GetComponent<SteamVR_Behaviour_Pose>();

            trackedObject.onTransformUpdated.AddListener(OnTransformUpdated);
        }

        protected virtual void OnTransformUpdated(SteamVR_Action_Pose pose)
        {
            HandFollowUpdate();
        }

        //-------------------------------------------------
        protected virtual IEnumerator Start()
        {
            // save off player instance
            playerInstance = Player.instance;
            if (!playerInstance) Debug.LogError("No player instance found in Hand Start()");

            // allocate array for colliders
            overlappingColliders = new Collider[ColliderArraySize];

            // We are a "no SteamVR fallback hand" if we have this camera set
            // we'll use the right mouse to look around and left mouse to interact
            // - don't need to find the device
            if (noSteamVRFallbackCamera) yield break;

            //Debug.Log( "Hand - initializing connection routine" );

            while (true)
            {
                if (isPoseValid)
                {
                    InitController();
                    break;
                }

                yield return null;
            }
        }


        //-------------------------------------------------
        protected virtual void UpdateHovering()
        {
            if (noSteamVRFallbackCamera == null && isActive == false) return;

            if (hoverLocked)
                return;

            if (applicationLostFocusObject.activeSelf)
                return;

            var closestDistance = float.MaxValue;
            Interactable closestInteractable = null;

            if (useHoverSphere)
            {
                var scaledHoverRadius =
                    hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
                CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance,
                    ref closestInteractable, Color.green);
            }

            if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
            {
                var scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(transform));
                CheckHoveringForTransform(mainRenderModel.GetControllerPosition(controllerHoverComponent),
                    scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.blue);
            }

            if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
            {
                var scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(transform));
                CheckHoveringForTransform(mainRenderModel.GetBonePosition((int) fingerJointHover),
                    scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.yellow);
            }

            // Hover on this one
            hoveringInteractable = closestInteractable;
        }

        protected virtual bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius,
            ref float closestDistance, ref Interactable closestInteractable, Color debugColor)
        {
            var foundCloser = false;

            // null out old vals
            for (var i = 0; i < overlappingColliders.Length; ++i) overlappingColliders[i] = null;

            var numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders,
                hoverLayerMask.value);

            if (numColliding == ColliderArraySize)
                Debug.LogWarning("This hand is overlapping the max number of colliders: " + ColliderArraySize +
                                 ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

            // DebugVar
            var iActualColliderCount = 0;

            // Pick the closest hovering
            for (var colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
            {
                var collider = overlappingColliders[colliderIndex];

                if (collider == null)
                    continue;

                var contacting = collider.GetComponentInParent<Interactable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Ignore this collider for hovering
                var ignore = collider.GetComponent<IgnoreHovering>();
                if (ignore != null)
                    if (ignore.onlyIgnoreHand == null || ignore.onlyIgnoreHand == this)
                        continue;

                // Can't hover over the object if it's attached
                var hoveringOverAttached = false;
                for (var attachedIndex = 0; attachedIndex < attachedObjects.Count; attachedIndex++)
                    if (attachedObjects[attachedIndex].attachedObject == contacting.gameObject)
                    {
                        hoveringOverAttached = true;
                        break;
                    }

                if (hoveringOverAttached)
                    continue;

                // Occupied by another hand, so we can't touch it
                if (otherHand && otherHand.hoveringInteractable == contacting)
                    continue;

                // Best candidate so far...
                var distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                    foundCloser = true;
                }

                iActualColliderCount++;
            }

            if (showDebugInteractables && foundCloser)
                Debug.DrawLine(hoverPosition, closestInteractable.transform.position, debugColor, .05f, false);

            if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
            {
                prevOverlappingColliders = iActualColliderCount;

                if (spewDebugText)
                    HandDebugLog("Found " + iActualColliderCount + " overlapping colliders.");
            }

            return foundCloser;
        }


        //-------------------------------------------------
        protected virtual void UpdateNoSteamVRFallback()
        {
            if (noSteamVRFallbackCamera)
            {
                var ray = noSteamVRFallbackCamera.ScreenPointToRay(Input.mousePosition);

                if (attachedObjects.Count > 0)
                {
                    // Holding down the mouse:
                    // move around a fixed distance from the camera
                    transform.position = ray.origin + noSteamVRFallbackInteractorDistance * ray.direction;
                }
                else
                {
                    // Not holding down the mouse:
                    // cast out a ray to see what we should mouse over

                    // Don't want to hit the hand and anything underneath it
                    // So move it back behind the camera when we do the raycast
                    var oldPosition = transform.position;
                    transform.position = noSteamVRFallbackCamera.transform.forward * -1000.0f;

                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit, noSteamVRFallbackMaxDistanceNoItem))
                    {
                        transform.position = raycastHit.point;

                        // Remember this distance in case we click and drag the mouse
                        noSteamVRFallbackInteractorDistance =
                            Mathf.Min(noSteamVRFallbackMaxDistanceNoItem, raycastHit.distance);
                    }
                    else if (noSteamVRFallbackInteractorDistance > 0.0f)
                    {
                        // Move it around at the distance we last had a hit
                        transform.position = ray.origin +
                                             Mathf.Min(noSteamVRFallbackMaxDistanceNoItem,
                                                 noSteamVRFallbackInteractorDistance) * ray.direction;
                    }
                    else
                    {
                        // Didn't hit, just leave it where it was
                        transform.position = oldPosition;
                    }
                }
            }
        }


        //-------------------------------------------------
        private void UpdateDebugText()
        {
            if (showDebugText)
            {
                if (debugText == null)
                {
                    debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
                    debugText.fontSize = 120;
                    debugText.characterSize = 0.001f;
                    debugText.transform.parent = transform;

                    debugText.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                }

                if (handType == SteamVR_Input_Sources.RightHand)
                {
                    debugText.transform.localPosition = new Vector3(-0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Right;
                    debugText.anchor = TextAnchor.UpperRight;
                }
                else
                {
                    debugText.transform.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Left;
                    debugText.anchor = TextAnchor.UpperLeft;
                }

                debugText.text = string.Format(
                    "Hovering: {0}\n" +
                    "Hover Lock: {1}\n" +
                    "Attached: {2}\n" +
                    "Total Attached: {3}\n" +
                    "Type: {4}\n",
                    hoveringInteractable ? hoveringInteractable.gameObject.name : "null",
                    hoverLocked,
                    currentAttachedObject ? currentAttachedObject.name : "null",
                    attachedObjects.Count,
                    handType.ToString());
            }
            else
            {
                if (debugText != null) Destroy(debugText.gameObject);
            }
        }


        //-------------------------------------------------
        protected virtual void OnEnable()
        {
            inputFocusAction.enabled = true;

            // Stagger updates between hands
            var hoverUpdateBegin = otherHand != null && otherHand.GetInstanceID() < GetInstanceID()
                ? 0.5f * hoverUpdateInterval
                : 0.0f;
            InvokeRepeating("UpdateHovering", hoverUpdateBegin, hoverUpdateInterval);
            InvokeRepeating("UpdateDebugText", hoverUpdateBegin, hoverUpdateInterval);
        }


        //-------------------------------------------------
        protected virtual void OnDisable()
        {
            inputFocusAction.enabled = false;

            CancelInvoke();
        }


        //-------------------------------------------------
        protected virtual void Update()
        {
            UpdateNoSteamVRFallback();

            var attachedObject = currentAttachedObject;
            if (attachedObject != null)
                attachedObject.SendMessage("HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver);

            if (hoveringInteractable)
                hoveringInteractable.SendMessage("HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
        }

        protected virtual void HandFollowUpdate()
        {
            var attachedObject = currentAttachedObject;
            if (attachedObject != null)
                if (currentAttachedObjectInfo.Value.interactable != null &&
                    currentAttachedObjectInfo.Value.interactable.handFollowTransform != null)
                {
                    if (currentAttachedObjectInfo.Value.interactable.handFollowTransformRotation)
                    {
                        var offset = Quaternion.Inverse(transform.rotation) *
                                     currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation;
                        var targetHandRotation =
                            currentAttachedObjectInfo.Value.interactable.handFollowTransform.rotation *
                            Quaternion.Inverse(offset);

                        if (mainRenderModel != null)
                            mainRenderModel.SetHandRotation(targetHandRotation);
                        if (hoverhighlightRenderModel != null)
                            hoverhighlightRenderModel.SetHandRotation(targetHandRotation);
                    }

                    if (currentAttachedObjectInfo.Value.interactable.handFollowTransformPosition)
                    {
                        var worldOffset = transform.position -
                                          currentAttachedObjectInfo.Value.handAttachmentPointTransform.position;

                        var rotationDiff = mainRenderModel.GetHandRotation() * Quaternion.Inverse(transform.rotation);

                        var localOffset = rotationDiff * worldOffset;
                        var targetHandPosition =
                            currentAttachedObjectInfo.Value.interactable.handFollowTransform.position + localOffset;

                        if (mainRenderModel != null)
                            mainRenderModel.SetHandPosition(targetHandPosition);
                        if (hoverhighlightRenderModel != null)
                            hoverhighlightRenderModel.SetHandPosition(targetHandPosition);
                    }
                }
        }


        protected virtual void FixedUpdate()
        {
            if (currentAttachedObject != null)
            {
                var attachedInfo = currentAttachedObjectInfo.Value;
                if (attachedInfo.attachedObject != null)
                    if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
                        UpdateAttachedVelocity(attachedInfo);
            }
        }

        protected void UpdateAttachedVelocity(AttachedObject attachedObjectInfo)
        {
            var scale = SteamVR_Utils.GetLossyScale(currentAttachedObjectInfo.Value.handAttachmentPointTransform);

            var maxVelocityChange = MaxVelocityChange * scale;
            var velocityMagic = VelocityMagic;
            var angularVelocityMagic = AngularVelocityMagic;
            var maxAngularVelocityChange = MaxAngularVelocityChange * scale;

            var targetItemPosition =
                currentAttachedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(attachedObjectInfo
                    .initialPositionalOffset);
            var positionDelta = targetItemPosition - attachedObjectInfo.attachedRigidbody.position;
            var velocityTarget = positionDelta * velocityMagic * Time.deltaTime;

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
                attachedObjectInfo.attachedRigidbody.velocity =
                    Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.velocity, velocityTarget,
                        maxVelocityChange);


            var targetItemRotation = currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation *
                                     attachedObjectInfo.initialRotationalOffset;
            var rotationDelta = targetItemRotation *
                                Quaternion.Inverse(attachedObjectInfo.attachedObject.transform.rotation);


            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                var angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

                attachedObjectInfo.attachedRigidbody.angularVelocity = Vector3.MoveTowards(
                    attachedObjectInfo.attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }


        //-------------------------------------------------
        protected virtual void OnInputFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                DetachObject(applicationLostFocusObject, true);
                applicationLostFocusObject.SetActive(false);
                UpdateHovering();
                BroadcastMessage("OnParentHandInputFocusAcquired", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                applicationLostFocusObject.SetActive(true);
                AttachObject(applicationLostFocusObject, GrabTypes.Scripted, AttachmentFlags.ParentToHand);
                BroadcastMessage("OnParentHandInputFocusLost", SendMessageOptions.DontRequireReceiver);
            }
        }

        //-------------------------------------------------
        protected virtual void OnDrawGizmos()
        {
            if (useHoverSphere)
            {
                Gizmos.color = Color.green;
                var scaledHoverRadius =
                    hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
                Gizmos.DrawWireSphere(hoverSphereTransform.position, scaledHoverRadius / 2);
            }

            if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
            {
                Gizmos.color = Color.blue;
                var scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetControllerPosition(controllerHoverComponent),
                    scaledHoverRadius / 2);
            }

            if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
            {
                Gizmos.color = Color.yellow;
                var scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetBonePosition((int) fingerJointHover), scaledHoverRadius / 2);
            }
        }


        //-------------------------------------------------
        private void HandDebugLog(string msg)
        {
            if (spewDebugText) Debug.Log("Hand (" + name + "): " + msg);
        }


        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        //
        // interactable - The Interactable to hover over indefinitely.
        //-------------------------------------------------
        public void HoverLock(Interactable interactable)
        {
            if (spewDebugText)
                HandDebugLog("HoverLock " + interactable);
            hoverLocked = true;
            hoveringInteractable = interactable;
        }


        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        public void HoverUnlock(Interactable interactable)
        {
            if (spewDebugText)
                HandDebugLog("HoverUnlock " + interactable);

            if (hoveringInteractable == interactable) hoverLocked = false;
        }

        public void TriggerHapticPulse(ushort microSecondsDuration)
        {
            var seconds = microSecondsDuration / 1000000f;
            hapticAction.Execute(0, seconds, 1f / seconds, 1, handType);
        }

        public void TriggerHapticPulse(float duration, float frequency, float amplitude)
        {
            hapticAction.Execute(0, duration, frequency, amplitude, handType);
        }

        public void ShowGrabHint()
        {
            ControllerButtonHints.ShowButtonHint(this, grabGripAction); //todo: assess
        }

        public void HideGrabHint()
        {
            ControllerButtonHints.HideButtonHint(this, grabGripAction); //todo: assess
        }

        public void ShowGrabHint(string text)
        {
            ControllerButtonHints.ShowTextHint(this, grabGripAction, text);
        }

        public GrabTypes GetGrabStarting(GrabTypes explicitType = GrabTypes.None)
        {
            if (explicitType != GrabTypes.None)
            {
                if (explicitType == GrabTypes.Pinch && grabPinchAction.GetStateDown(handType))
                    return GrabTypes.Pinch;
                if (explicitType == GrabTypes.Grip && grabGripAction.GetStateDown(handType))
                    return GrabTypes.Grip;
            }
            else
            {
                if (grabPinchAction.GetStateDown(handType))
                    return GrabTypes.Pinch;
                if (grabGripAction.GetStateDown(handType))
                    return GrabTypes.Grip;
            }

            return GrabTypes.None;
        }

        public GrabTypes GetGrabEnding(GrabTypes explicitType = GrabTypes.None)
        {
            if (explicitType != GrabTypes.None)
            {
                if (explicitType == GrabTypes.Pinch && grabPinchAction.GetStateUp(handType))
                    return GrabTypes.Pinch;
                if (explicitType == GrabTypes.Grip && grabGripAction.GetStateUp(handType))
                    return GrabTypes.Grip;
            }
            else
            {
                if (grabPinchAction.GetStateUp(handType))
                    return GrabTypes.Pinch;
                if (grabGripAction.GetStateUp(handType))
                    return GrabTypes.Grip;
            }

            return GrabTypes.None;
        }

        public bool IsGrabEnding(GameObject attachedObject)
        {
            for (var attachedObjectIndex = 0; attachedObjectIndex < attachedObjects.Count; attachedObjectIndex++)
                if (attachedObjects[attachedObjectIndex].attachedObject == attachedObject)
                    return IsGrabbingWithType(attachedObjects[attachedObjectIndex].grabbedWithType) == false;

            return false;
        }

        public bool IsGrabbingWithType(GrabTypes type)
        {
            switch (type)
            {
                case GrabTypes.Pinch:
                    return grabPinchAction.GetState(handType);

                case GrabTypes.Grip:
                    return grabGripAction.GetState(handType);

                default:
                    return false;
            }
        }

        public bool IsGrabbingWithOppositeType(GrabTypes type)
        {
            switch (type)
            {
                case GrabTypes.Pinch:
                    return grabGripAction.GetState(handType);

                case GrabTypes.Grip:
                    return grabPinchAction.GetState(handType);

                default:
                    return false;
            }
        }

        public GrabTypes GetBestGrabbingType()
        {
            return GetBestGrabbingType(GrabTypes.None);
        }

        public GrabTypes GetBestGrabbingType(GrabTypes preferred, bool forcePreference = false)
        {
            if (preferred == GrabTypes.Pinch)
            {
                if (grabPinchAction.GetState(handType))
                    return GrabTypes.Pinch;
                if (forcePreference)
                    return GrabTypes.None;
            }

            if (preferred == GrabTypes.Grip)
            {
                if (grabGripAction.GetState(handType))
                    return GrabTypes.Grip;
                if (forcePreference)
                    return GrabTypes.None;
            }

            if (grabPinchAction.GetState(handType))
                return GrabTypes.Pinch;
            if (grabGripAction.GetState(handType))
                return GrabTypes.Grip;

            return GrabTypes.None;
        }


        //-------------------------------------------------
        private void InitController()
        {
            if (spewDebugText)
                HandDebugLog("Hand " + name + " connected with type " + handType);

            var hadOldRendermodel = mainRenderModel != null;
            var oldRM_rom = EVRSkeletalMotionRange.WithController;
            if (hadOldRendermodel)
                oldRM_rom = mainRenderModel.GetSkeletonRangeOfMotion;


            foreach (var r in renderModels)
                if (r != null)
                    Destroy(r.gameObject);

            renderModels.Clear();

            var renderModelInstance = Instantiate(renderModelPrefab);
            renderModelInstance.layer = gameObject.layer;
            renderModelInstance.tag = gameObject.tag;
            renderModelInstance.transform.parent = transform;
            renderModelInstance.transform.localPosition = Vector3.zero;
            renderModelInstance.transform.localRotation = Quaternion.identity;
            renderModelInstance.transform.localScale = renderModelPrefab.transform.localScale;

            //TriggerHapticPulse(800);  //pulse on controller init

            var deviceIndex = trackedObject.GetDeviceIndex();

            mainRenderModel = renderModelInstance.GetComponent<RenderModel>();
            renderModels.Add(mainRenderModel);

            if (hadOldRendermodel)
                mainRenderModel.SetSkeletonRangeOfMotion(oldRM_rom);

            BroadcastMessage("SetInputSource", handType,
                SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
            BroadcastMessage("OnHandInitialized", deviceIndex,
                SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
        }

        public void SetRenderModel(GameObject prefab)
        {
            renderModelPrefab = prefab;

            if (mainRenderModel != null && isPoseValid)
                InitController();
        }

        public void SetHoverRenderModel(RenderModel hoverRenderModel)
        {
            hoverhighlightRenderModel = hoverRenderModel;
            renderModels.Add(hoverRenderModel);
        }

        public int GetDeviceIndex()
        {
            return trackedObject.GetDeviceIndex();
        }

        public struct AttachedObject
        {
            public GameObject attachedObject;
            public Interactable interactable;
            public Rigidbody attachedRigidbody;
            public CollisionDetectionMode collisionDetectionMode;
            public bool attachedRigidbodyWasKinematic;
            public bool attachedRigidbodyUsedGravity;
            public GameObject originalParent;
            public bool isParentedToHand;
            public GrabTypes grabbedWithType;
            public AttachmentFlags attachmentFlags;
            public Vector3 initialPositionalOffset;
            public Quaternion initialRotationalOffset;
            public Transform attachedOffsetTransform;
            public Transform handAttachmentPointTransform;

            public bool HasAttachFlag(AttachmentFlags flag)
            {
                return (attachmentFlags & flag) == flag;
            }
        }
    }


    [Serializable]
    public class HandEvent : UnityEvent<Hand>
    {
    }


#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [CustomEditor(typeof(Hand))]
    public class HandEditor : Editor
    {
        //-------------------------------------------------
        // Custom Inspector GUI allows us to click from within the UI
        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var hand = (Hand) target;

            if (hand.otherHand)
            {
                if (hand.otherHand.otherHand != hand)
                    EditorGUILayout.HelpBox("The otherHand of this Hand's otherHand is not this Hand.",
                        MessageType.Warning);

                if (hand.handType == SteamVR_Input_Sources.LeftHand &&
                    hand.otherHand.handType != SteamVR_Input_Sources.RightHand)
                    EditorGUILayout.HelpBox("This is a left Hand but otherHand is not a right Hand.",
                        MessageType.Warning);

                if (hand.handType == SteamVR_Input_Sources.RightHand &&
                    hand.otherHand.handType != SteamVR_Input_Sources.LeftHand)
                    EditorGUILayout.HelpBox("This is a right Hand but otherHand is not a left Hand.",
                        MessageType.Warning);

                if (hand.handType == SteamVR_Input_Sources.Any && hand.otherHand.handType != SteamVR_Input_Sources.Any)
                    EditorGUILayout.HelpBox("This is an any-handed Hand but otherHand is not an any-handed Hand.",
                        MessageType.Warning);
            }
        }
    }
#endif
}