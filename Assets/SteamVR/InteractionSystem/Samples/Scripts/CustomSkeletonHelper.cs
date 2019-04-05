//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    public class CustomSkeletonHelper : MonoBehaviour
    {
        public enum MirrorType
        {
            None,
            LeftToRight,
            RightToLeft
        }

        public Finger[] fingers;
        public Thumb[] thumbs;
        public Retargetable wrist;

        private void Update()
        {
            for (var fingerIndex = 0; fingerIndex < fingers.Length; fingerIndex++)
            {
                var finger = fingers[fingerIndex];
                finger.metacarpal.destination.rotation = finger.metacarpal.source.rotation;
                finger.proximal.destination.rotation = finger.proximal.source.rotation;
                finger.middle.destination.rotation = finger.middle.source.rotation;
                finger.distal.destination.rotation = finger.distal.source.rotation;
            }

            for (var thumbIndex = 0; thumbIndex < thumbs.Length; thumbIndex++)
            {
                var thumb = thumbs[thumbIndex];
                thumb.metacarpal.destination.rotation = thumb.metacarpal.source.rotation;
                thumb.middle.destination.rotation = thumb.middle.source.rotation;
                thumb.distal.destination.rotation = thumb.distal.source.rotation;
            }

            wrist.destination.position = wrist.source.position;
            wrist.destination.rotation = wrist.source.rotation;
        }

        [Serializable]
        public class Retargetable
        {
            public Transform destination;
            public Transform source;

            public Retargetable(Transform source, Transform destination)
            {
                this.source = source;
                this.destination = destination;
            }
        }

        [Serializable]
        public class Thumb
        {
            public Transform aux;
            public Retargetable distal;
            public Retargetable metacarpal;
            public Retargetable middle;

            public Thumb(Retargetable metacarpal, Retargetable middle, Retargetable distal, Transform aux)
            {
                this.metacarpal = metacarpal;
                this.middle = middle;
                this.distal = distal;
                this.aux = aux;
            }
        }

        [Serializable]
        public class Finger
        {
            public Transform aux;
            public Retargetable distal;
            public Retargetable metacarpal;
            public Retargetable middle;
            public Retargetable proximal;

            public Finger(Retargetable metacarpal, Retargetable proximal, Retargetable middle, Retargetable distal,
                Transform aux)
            {
                this.metacarpal = metacarpal;
                this.proximal = proximal;
                this.middle = middle;
                this.distal = distal;
                this.aux = aux;
            }
        }
    }
}