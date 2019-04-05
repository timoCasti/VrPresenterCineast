//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VelocityEstimator))]
    public class Throwable : MonoBehaviour
    {
        public bool attachEaseIn;
        protected Transform attachEaseInTransform;
        protected bool attached;

        [EnumFlags] [Tooltip("The flags used to attach this object to the hand.")]
        public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.ParentToHand |
                                                      Hand.AttachmentFlags.DetachFromOtherHand |
                                                      Hand.AttachmentFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform attachmentOffset;

        protected Vector3 attachPosition;
        protected Quaternion attachRotation;
        protected float attachTime;

        [Tooltip(
            "How fast must this object be moving to attach due to a trigger hold instead of a trigger press? (-1 to disable)")]
        public float catchingSpeedThreshold = -1;

        protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

        [HideInInspector] public Interactable interactable;

        public UnityEvent onDetachFromHand;

        public UnityEvent onPickUp;

        public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;

        [Tooltip("When detaching the object, should it return to its original parent?")]
        public bool restoreOriginalParent;

        protected new Rigidbody rigidbody;

        public float scaleReleaseVelocity = 1.1f;

        public bool snapAttachEaseInCompleted;
        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float snapAttachEaseInTime = 0.15f;

        protected VelocityEstimator velocityEstimator;


        //-------------------------------------------------
        protected virtual void Awake()
        {
            velocityEstimator = GetComponent<VelocityEstimator>();
            interactable = GetComponent<Interactable>();

            if (attachEaseIn) attachmentFlags &= ~Hand.AttachmentFlags.SnapOnAttach;

            rigidbody = GetComponent<Rigidbody>();
            rigidbody.maxAngularVelocity = 50.0f;


            if (attachmentOffset != null) interactable.handFollowTransform = attachmentOffset;
        }


        //-------------------------------------------------
        protected virtual void OnHandHoverBegin(Hand hand)
        {
            var showHint = false;

            // "Catch" the throwable by holding down the interaction button instead of pressing it.
            // Only do this if the throwable is moving faster than the prescribed threshold speed,
            // and if it isn't attached to another hand
            if (!attached && catchingSpeedThreshold != -1)
            {
                var catchingThreshold = catchingSpeedThreshold *
                                        SteamVR_Utils.GetLossyScale(Player.instance.trackingOriginTransform);

                var bestGrabType = hand.GetBestGrabbingType();

                if (bestGrabType != GrabTypes.None)
                    if (rigidbody.velocity.magnitude >= catchingThreshold)
                    {
                        hand.AttachObject(gameObject, bestGrabType, attachmentFlags);
                        showHint = false;
                    }
            }

            if (showHint) hand.ShowGrabHint();
        }


        //-------------------------------------------------
        protected virtual void OnHandHoverEnd(Hand hand)
        {
            hand.HideGrabHint();
        }


        //-------------------------------------------------
        protected virtual void HandHoverUpdate(Hand hand)
        {
            var startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None)
            {
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags, attachmentOffset);
                hand.HideGrabHint();
            }
        }

        //-------------------------------------------------
        protected virtual void OnAttachedToHand(Hand hand)
        {
            //Debug.Log("Pickup: " + hand.GetGrabStarting().ToString());

            hadInterpolation = rigidbody.interpolation;

            attached = true;

            onPickUp.Invoke();

            hand.HoverLock(null);

            rigidbody.interpolation = RigidbodyInterpolation.None;

            velocityEstimator.BeginEstimatingVelocity();

            attachTime = Time.time;
            attachPosition = transform.position;
            attachRotation = transform.rotation;

            if (attachEaseIn) attachEaseInTransform = hand.objectAttachmentPoint;

            snapAttachEaseInCompleted = false;
        }


        //-------------------------------------------------
        protected virtual void OnDetachedFromHand(Hand hand)
        {
            attached = false;

            onDetachFromHand.Invoke();

            hand.HoverUnlock(null);

            rigidbody.interpolation = hadInterpolation;

            Vector3 velocity;
            Vector3 angularVelocity;

            GetReleaseVelocities(hand, out velocity, out angularVelocity);

            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
        }


        public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            switch (releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    velocityEstimator.FinishEstimatingVelocity();
                    velocity = velocityEstimator.GetVelocityEstimate();
                    angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                    break;
                case ReleaseStyle.AdvancedEstimation:
                    hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                    break;
                case ReleaseStyle.GetFromHand:
                    velocity = hand.GetTrackedObjectVelocity(releaseVelocityTimeOffset);
                    angularVelocity = hand.GetTrackedObjectAngularVelocity(releaseVelocityTimeOffset);
                    break;
                default:
                case ReleaseStyle.NoChange:
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                    break;
            }

            if (releaseVelocityStyle != ReleaseStyle.NoChange)
                velocity *= scaleReleaseVelocity;
        }

        //-------------------------------------------------
        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (attachEaseIn)
            {
                var t = Util.RemapNumberClamped(Time.time, attachTime, attachTime + snapAttachEaseInTime, 0.0f, 1.0f);
                if (t < 1.0f)
                {
                    t = snapAttachEaseInCurve.Evaluate(t);
                    transform.position = Vector3.Lerp(attachPosition, attachEaseInTransform.position, t);
                    transform.rotation = Quaternion.Lerp(attachRotation, attachEaseInTransform.rotation, t);
                }
                else if (!snapAttachEaseInCompleted)
                {
                    gameObject.SendMessage("OnThrowableAttachEaseInCompleted", hand,
                        SendMessageOptions.DontRequireReceiver);
                    snapAttachEaseInCompleted = true;
                }
            }

            if (hand.IsGrabEnding(gameObject)) hand.DetachObject(gameObject, restoreOriginalParent);
        }


        //-------------------------------------------------
        protected virtual IEnumerator LateDetach(Hand hand)
        {
            yield return new WaitForEndOfFrame();

            hand.DetachObject(gameObject, restoreOriginalParent);
        }


        //-------------------------------------------------
        protected virtual void OnHandFocusAcquired(Hand hand)
        {
            gameObject.SetActive(true);
            velocityEstimator.BeginEstimatingVelocity();
        }


        //-------------------------------------------------
        protected virtual void OnHandFocusLost(Hand hand)
        {
            gameObject.SetActive(false);
            velocityEstimator.FinishEstimatingVelocity();
        }
    }

    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation,
        AdvancedEstimation
    }
}