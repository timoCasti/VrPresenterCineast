﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class LinearDrive : MonoBehaviour
    {
        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;
        public Transform endPosition;

        protected float initialMappingOffset;

        protected Interactable interactable;
        public LinearMapping linearMapping;
        public bool maintainMomemntum = true;
        protected float mappingChangeRate;
        protected float[] mappingChangeSamples;
        public float momemtumDampenRate = 5.0f;
        protected int numMappingChangeSamples = 5;
        protected float prevMapping;
        public bool repositionGameObject = true;
        protected int sampleCount;
        public Transform startPosition;


        protected virtual void Awake()
        {
            mappingChangeSamples = new float[numMappingChangeSamples];
            interactable = GetComponent<Interactable>();
        }

        protected virtual void Start()
        {
            if (linearMapping == null) linearMapping = GetComponent<LinearMapping>();

            if (linearMapping == null) linearMapping = gameObject.AddComponent<LinearMapping>();

            initialMappingOffset = linearMapping.value;

            if (repositionGameObject) UpdateLinearMapping(transform);
        }

        protected virtual void HandHoverUpdate(Hand hand)
        {
            var startingGrabType = hand.GetGrabStarting();

            if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            {
                initialMappingOffset = linearMapping.value - CalculateLinearMapping(hand.transform);
                sampleCount = 0;
                mappingChangeRate = 0.0f;

                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
        }

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            UpdateLinearMapping(hand.transform);

            if (hand.IsGrabEnding(gameObject)) hand.DetachObject(gameObject);
        }

        protected virtual void OnDetachedFromHand(Hand hand)
        {
            CalculateMappingChangeRate();
        }


        protected void CalculateMappingChangeRate()
        {
            //Compute the mapping change rate
            mappingChangeRate = 0.0f;
            var mappingSamplesCount = Mathf.Min(sampleCount, mappingChangeSamples.Length);
            if (mappingSamplesCount != 0)
            {
                for (var i = 0; i < mappingSamplesCount; ++i) mappingChangeRate += mappingChangeSamples[i];
                mappingChangeRate /= mappingSamplesCount;
            }
        }

        protected void UpdateLinearMapping(Transform updateTransform)
        {
            prevMapping = linearMapping.value;
            linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

            mappingChangeSamples[sampleCount % mappingChangeSamples.Length] =
                1.0f / Time.deltaTime * (linearMapping.value - prevMapping);
            sampleCount++;

            if (repositionGameObject)
                transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
        }

        protected float CalculateLinearMapping(Transform updateTransform)
        {
            var direction = endPosition.position - startPosition.position;
            var length = direction.magnitude;
            direction.Normalize();

            var displacement = updateTransform.position - startPosition.position;

            return Vector3.Dot(displacement, direction) / length;
        }


        protected virtual void Update()
        {
            if (maintainMomemntum && mappingChangeRate != 0.0f)
            {
                //Dampen the mapping change rate and apply it to the mapping
                mappingChangeRate = Mathf.Lerp(mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime);
                linearMapping.value = Mathf.Clamp01(linearMapping.value + mappingChangeRate * Time.deltaTime);

                if (repositionGameObject)
                    transform.position =
                        Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
            }
        }
    }
}