//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class HoverButton : MonoBehaviour
    {
        public bool buttonDown;
        public bool buttonUp;

        [Range(0, 1)] public float disengageAtPercent = 0.9f;

        private Vector3 endPosition;

        [Range(0, 1)] public float engageAtPercent = 0.95f;

        public bool engaged;

        private Vector3 handEnteredPosition;

        private bool hovering;

        private Hand lastHoveredHand;

        public Vector3 localMoveDistance = new Vector3(0, -0.1f, 0);
        public Transform movingPart;

        public HandEvent onButtonDown;
        public HandEvent onButtonIsPressed;
        public HandEvent onButtonUp;

        private Vector3 startPosition;

        private void Start()
        {
            if (movingPart == null && transform.childCount > 0)
                movingPart = transform.GetChild(0);

            startPosition = movingPart.localPosition;
            endPosition = startPosition + localMoveDistance;
            handEnteredPosition = endPosition;
        }

        private void HandHoverUpdate(Hand hand)
        {
            hovering = true;
            lastHoveredHand = hand;

            var wasEngaged = engaged;

            var currentDistance = Vector3.Distance(movingPart.parent.InverseTransformPoint(hand.transform.position),
                endPosition);
            var enteredDistance = Vector3.Distance(handEnteredPosition, endPosition);

            if (currentDistance > enteredDistance)
            {
                enteredDistance = currentDistance;
                handEnteredPosition = movingPart.parent.InverseTransformPoint(hand.transform.position);
            }

            var distanceDifference = enteredDistance - currentDistance;

            var lerp = Mathf.InverseLerp(0, localMoveDistance.magnitude, distanceDifference);

            if (lerp > engageAtPercent)
                engaged = true;
            else if (lerp < disengageAtPercent)
                engaged = false;

            movingPart.localPosition = Vector3.Lerp(startPosition, endPosition, lerp);

            InvokeEvents(wasEngaged, engaged);
        }

        private void LateUpdate()
        {
            if (hovering == false)
            {
                movingPart.localPosition = startPosition;
                handEnteredPosition = endPosition;

                InvokeEvents(engaged, false);
                engaged = false;
            }

            hovering = false;
        }

        private void InvokeEvents(bool wasEngaged, bool isEngaged)
        {
            buttonDown = wasEngaged == false && isEngaged;
            buttonUp = wasEngaged && isEngaged == false;

            if (buttonDown && onButtonDown != null)
                onButtonDown.Invoke(lastHoveredHand);
            if (buttonUp && onButtonUp != null)
                onButtonUp.Invoke(lastHoveredHand);
            if (isEngaged && onButtonIsPressed != null)
                onButtonIsPressed.Invoke(lastHoveredHand);
        }
    }
}