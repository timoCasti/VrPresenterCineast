﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object will get hover events and can be attached to the hands
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class Interactable : MonoBehaviour
    {
        public delegate void OnAttachedToHandDelegate(Hand hand);

        public delegate void OnDetachedFromHandDelegate(Hand hand);

        private static Material highlightMat;

        [Tooltip("Activates an action set on attach and deactivates on detach")]
        public SteamVR_ActionSet activateActionSetOnAttach;


        [NonSerialized] public Hand attachedToHand;

        private MeshRenderer[] existingRenderers;
        private SkinnedMeshRenderer[] existingSkinnedRenderers;

        [Tooltip("The integer in the animator to trigger on pickup. 0 for none")]
        public int handAnimationOnPickup;

        [Tooltip("If you want the hand to stick to an object while attached, set the transform to stick to here")]
        public Transform handFollowTransform;

        public bool handFollowTransformPosition = true;
        public bool handFollowTransformRotation = true;

        [Tooltip("Hide the controller part of the hand on attachment and show on detach")]
        public bool hideControllerOnAttach;

        [Tooltip("Hide the whole hand on attachment and show on detach")]
        public bool hideHandOnAttach = true;

        [Tooltip(
            "An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] hideHighlight;

        [Tooltip("Hide the skeleton part of the hand on attachment and show on detach")]
        public bool hideSkeletonOnAttach;

        private GameObject highlightHolder;


        [Tooltip("Set whether or not you want this interactible to highlight when hovering over it")]
        public bool highlightOnHover = true;

        private MeshRenderer[] highlightRenderers;
        private SkinnedMeshRenderer[] highlightSkinnedRenderers;

        [Tooltip("The range of motion to set on the skeleton. None for no change.")]
        public SkeletalMotionRangeChange setRangeOfMotionOnPickup = SkeletalMotionRangeChange.None;


        [Tooltip("Specify whether you want to snap to the hand's object attachment point, or just the raw hand")]
        public bool useHandObjectAttachmentPoint = true;

        public bool isDestroying { get; protected set; }

        public bool isHovering { get; protected set; }
        public bool wasHovering { get; protected set; }

        [HideInInspector] public event OnAttachedToHandDelegate onAttachedToHand;

        [HideInInspector] public event OnDetachedFromHandDelegate onDetachedFromHand;

        private void Start()
        {
            highlightMat = (Material) Resources.Load("SteamVR_HoverHighlight", typeof(Material));

            if (highlightMat == null)
                Debug.LogError(
                    "Hover Highlight Material is missing. Please create a material named 'SteamVR_HoverHighlight' and place it in a Resources folder");
        }

        private bool ShouldIgnoreHighlight(Component component)
        {
            return ShouldIgnore(component.gameObject);
        }

        private bool ShouldIgnore(GameObject check)
        {
            if (hideHighlight == null) return false;
            for (var ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
                if (check == hideHighlight[ignoreIndex])
                    return true;

            return false;
        }

        private void CreateHighlightRenderers()
        {
            existingSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            highlightHolder = new GameObject("Highlighter");
            highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

            for (var skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                var existingSkinned = existingSkinnedRenderers[skinnedIndex];

                if (ShouldIgnoreHighlight(existingSkinned))
                    continue;

                var newSkinnedHolder = new GameObject("SkinnedHolder");
                newSkinnedHolder.transform.parent = highlightHolder.transform;
                var newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
                var materials = new Material[existingSkinned.sharedMaterials.Length];
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    materials[materialIndex] = highlightMat;

                newSkinned.sharedMaterials = materials;
                newSkinned.sharedMesh = existingSkinned.sharedMesh;
                newSkinned.rootBone = existingSkinned.rootBone;
                newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
                newSkinned.bones = existingSkinned.bones;

                highlightSkinnedRenderers[skinnedIndex] = newSkinned;
            }

            var existingFilters = GetComponentsInChildren<MeshFilter>(true);
            existingRenderers = new MeshRenderer[existingFilters.Length];
            highlightRenderers = new MeshRenderer[existingFilters.Length];

            for (var filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
            {
                var existingFilter = existingFilters[filterIndex];
                var existingRenderer = existingFilter.GetComponent<MeshRenderer>();

                if (existingFilter == null || existingRenderer == null || ShouldIgnoreHighlight(existingFilter))
                    continue;

                var newFilterHolder = new GameObject("FilterHolder");
                newFilterHolder.transform.parent = highlightHolder.transform;
                var newFilter = newFilterHolder.AddComponent<MeshFilter>();
                newFilter.sharedMesh = existingFilter.sharedMesh;
                var newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

                var materials = new Material[existingRenderer.sharedMaterials.Length];
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    materials[materialIndex] = highlightMat;
                newRenderer.sharedMaterials = materials;

                highlightRenderers[filterIndex] = newRenderer;
                existingRenderers[filterIndex] = existingRenderer;
            }
        }

        private void UpdateHighlightRenderers()
        {
            if (highlightHolder == null)
                return;

            for (var skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                var existingSkinned = existingSkinnedRenderers[skinnedIndex];
                var highlightSkinned = highlightSkinnedRenderers[skinnedIndex];

                if (existingSkinned != null && highlightSkinned != null && attachedToHand == false)
                {
                    highlightSkinned.transform.position = existingSkinned.transform.position;
                    highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
                    highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
                    highlightSkinned.localBounds = existingSkinned.localBounds;
                    highlightSkinned.enabled = isHovering && existingSkinned.enabled &&
                                               existingSkinned.gameObject.activeInHierarchy;

                    var blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
                    for (var blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                        highlightSkinned.SetBlendShapeWeight(blendShapeIndex,
                            existingSkinned.GetBlendShapeWeight(blendShapeIndex));
                }
                else if (highlightSkinned != null)
                {
                    highlightSkinned.enabled = false;
                }
            }

            for (var rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
            {
                var existingRenderer = existingRenderers[rendererIndex];
                var highlightRenderer = highlightRenderers[rendererIndex];

                if (existingRenderer != null && highlightRenderer != null && attachedToHand == false)
                {
                    highlightRenderer.transform.position = existingRenderer.transform.position;
                    highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
                    highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
                    highlightRenderer.enabled = isHovering && existingRenderer.enabled &&
                                                existingRenderer.gameObject.activeInHierarchy;
                }
                else if (highlightRenderer != null)
                {
                    highlightRenderer.enabled = false;
                }
            }
        }

        private void HandHoverUpdate()
        {
            if (highlightOnHover)
                if (wasHovering == false)
                {
                    isHovering = true;
                    CreateHighlightRenderers();
                    UpdateHighlightRenderers();
                }

            isHovering = true;
        }


        private void Update()
        {
            wasHovering = isHovering;

            if (highlightOnHover)
            {
                UpdateHighlightRenderers();

                if (wasHovering == false && isHovering == false && highlightHolder != null)
                    Destroy(highlightHolder);

                isHovering = false;
            }
        }

        private void OnAttachedToHand(Hand hand)
        {
            if (activateActionSetOnAttach != null)
                activateActionSetOnAttach.ActivatePrimary();

            if (onAttachedToHand != null) onAttachedToHand.Invoke(hand);

            attachedToHand = hand;
        }

        private void OnDetachedFromHand(Hand hand)
        {
            if (activateActionSetOnAttach != null)
                if (hand.otherHand.currentAttachedObjectInfo.HasValue == false ||
                    hand.otherHand.currentAttachedObjectInfo.Value.interactable != null &&
                    hand.otherHand.currentAttachedObjectInfo.Value.interactable.activateActionSetOnAttach !=
                    activateActionSetOnAttach)
                    activateActionSetOnAttach.Deactivate();

            if (onDetachedFromHand != null) onDetachedFromHand.Invoke(hand);

            attachedToHand = null;
        }

        protected virtual void OnDestroy()
        {
            isDestroying = true;

            if (attachedToHand != null)
            {
                attachedToHand.ForceHoverUnlock();
                attachedToHand.DetachObject(gameObject, false);
            }
        }
    }
}