//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object will get hover events and can be attached to the hands
//
//=============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class InteractableDebug : MonoBehaviour
    {
        private const bool onlyColorOnChange = true;

        [NonSerialized] public Hand attachedToHand;

        private Collider[] colliders;

        private bool isSimulation;

        private Color lastColor;

        public new Rigidbody rigidbody;

        private Renderer[] selfRenderers;
        public bool setPositionsForSimulations;
        public float simulateReleasesEveryXSeconds = 0.005f;

        public float simulateReleasesForXSecondsAroundRelease;

        private Throwable throwable;

        private bool isThrowable
        {
            get { return throwable != null; }
        }

        private void Awake()
        {
            selfRenderers = GetComponentsInChildren<Renderer>();
            throwable = GetComponent<Throwable>();
            rigidbody = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
        }

        private void OnAttachedToHand(Hand hand)
        {
            attachedToHand = hand;

            CreateMarker(Color.green);
        }


        protected virtual void HandAttachedUpdate(Hand hand)
        {
            Color grabbedColor;
            switch (hand.currentAttachedObjectInfo.Value.grabbedWithType)
            {
                case GrabTypes.Grip:
                    grabbedColor = Color.blue;
                    break;
                case GrabTypes.Pinch:
                    grabbedColor = Color.green;
                    break;
                case GrabTypes.Trigger:
                    grabbedColor = Color.yellow;
                    break;
                case GrabTypes.Scripted:
                    grabbedColor = Color.red;
                    break;
                case GrabTypes.None:
                default:
                    grabbedColor = Color.white;
                    break;
            }

            if (onlyColorOnChange && grabbedColor != lastColor || onlyColorOnChange == false)
                ColorSelf(grabbedColor);

            lastColor = grabbedColor;
        }


        private void OnDetachedFromHand(Hand hand)
        {
            if (isThrowable)
            {
                Vector3 velocity;
                Vector3 angularVelocity;

                throwable.GetReleaseVelocities(hand, out velocity, out angularVelocity);

                CreateMarker(Color.cyan, velocity.normalized);
            }

            CreateMarker(Color.red);
            attachedToHand = null;

            if (isSimulation == false && simulateReleasesForXSecondsAroundRelease != 0)
            {
                var startTime = -simulateReleasesForXSecondsAroundRelease;
                var endTime = simulateReleasesForXSecondsAroundRelease;

                var list = new List<InteractableDebug>();
                list.Add(this);

                for (var offset = startTime; offset <= endTime; offset += simulateReleasesEveryXSeconds)
                {
                    var lerp = Mathf.InverseLerp(startTime, endTime, offset);
                    var copy = CreateSimulation(hand, offset, Color.Lerp(Color.red, Color.green, lerp));
                    list.Add(copy);
                }

                for (var index = 0; index < list.Count; index++)
                for (var otherIndex = 0; otherIndex < list.Count; otherIndex++)
                    list[index].IgnoreObject(list[otherIndex]);
            }
        }

        public Collider[] GetColliders()
        {
            return colliders;
        }

        public void IgnoreObject(InteractableDebug otherInteractable)
        {
            var otherColliders = otherInteractable.GetColliders();

            for (var myIndex = 0; myIndex < colliders.Length; myIndex++)
            for (var otherIndex = 0; otherIndex < otherColliders.Length; otherIndex++)
                Physics.IgnoreCollision(colliders[myIndex], otherColliders[otherIndex]);
        }

        public void SetIsSimulation()
        {
            isSimulation = true;
        }

        private InteractableDebug CreateSimulation(Hand fromHand, float timeOffset, Color copyColor)
        {
            var copy = Instantiate(gameObject);
            var debugCopy = copy.GetComponent<InteractableDebug>();
            debugCopy.SetIsSimulation();
            debugCopy.ColorSelf(copyColor);
            copy.name = string.Format("{0} [offset: {1:0.000}]", copy.name, timeOffset);

            var velocity = fromHand.GetTrackedObjectVelocity(timeOffset);
            velocity *= throwable.scaleReleaseVelocity;

            debugCopy.rigidbody.velocity = velocity;

            return debugCopy;
        }

        private void CreateMarker(Color markerColor, float destroyAfter = 10)
        {
            CreateMarker(markerColor, attachedToHand.GetTrackedObjectVelocity().normalized, destroyAfter);
        }

        private void CreateMarker(Color markerColor, Vector3 forward, float destroyAfter = 10)
        {
            var baseMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(baseMarker.GetComponent<Collider>());
            baseMarker.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

            var line = Instantiate(baseMarker);
            line.transform.localScale = new Vector3(0.01f, 0.01f, 0.25f);
            line.transform.parent = baseMarker.transform;
            line.transform.localPosition = new Vector3(0, 0, line.transform.localScale.z / 2f);

            baseMarker.transform.position = attachedToHand.transform.position;
            baseMarker.transform.forward = forward;

            ColorThing(markerColor, baseMarker.GetComponentsInChildren<Renderer>());

            if (destroyAfter > 0)
                Destroy(baseMarker, destroyAfter);
        }

        private void ColorSelf(Color newColor)
        {
            ColorThing(newColor, selfRenderers);
        }

        private void ColorThing(Color newColor, Renderer[] renderers)
        {
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                renderers[rendererIndex].material.color = newColor;
        }
    }
}