﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

namespace Valve.VR.Extras
{
    public class SteamVR_GazeTracker : MonoBehaviour
    {
        public float gazeInCutoff = 0.15f;
        public float gazeOutCutoff = 0.4f;

        // Contains a HMD tracked object that we can use to find the user's gaze
        protected Transform hmdTrackedObject;
        public bool isInGaze;
        public event GazeEventHandler GazeOn;
        public event GazeEventHandler GazeOff;

        public virtual void OnGazeOn(GazeEventArgs gazeEventArgs)
        {
            if (GazeOn != null)
                GazeOn(this, gazeEventArgs);
        }

        public virtual void OnGazeOff(GazeEventArgs gazeEventArgs)
        {
            if (GazeOff != null)
                GazeOff(this, gazeEventArgs);
        }

        protected virtual void Update()
        {
            // If we haven't set up hmdTrackedObject find what the user is looking at
            if (hmdTrackedObject == null)
            {
                var trackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();
                foreach (var tracked in trackedObjects)
                    if (tracked.index == SteamVR_TrackedObject.EIndex.Hmd)
                    {
                        hmdTrackedObject = tracked.transform;
                        break;
                    }
            }

            if (hmdTrackedObject)
            {
                var ray = new Ray(hmdTrackedObject.position, hmdTrackedObject.forward);
                var plane = new Plane(hmdTrackedObject.forward, transform.position);

                var enter = 0.0f;
                if (plane.Raycast(ray, out enter))
                {
                    var intersect = hmdTrackedObject.position + hmdTrackedObject.forward * enter;
                    var dist = Vector3.Distance(intersect, transform.position);
                    //Debug.Log("Gaze dist = " + dist);
                    if (dist < gazeInCutoff && !isInGaze)
                    {
                        isInGaze = true;
                        GazeEventArgs gazeEventArgs;
                        gazeEventArgs.distance = dist;
                        OnGazeOn(gazeEventArgs);
                    }
                    else if (dist >= gazeOutCutoff && isInGaze)
                    {
                        isInGaze = false;
                        GazeEventArgs gazeEventArgs;
                        gazeEventArgs.distance = dist;
                        OnGazeOff(gazeEventArgs);
                    }
                }
            }
        }
    }

    public struct GazeEventArgs
    {
        public float distance;
    }

    public delegate void GazeEventHandler(object sender, GazeEventArgs gazeEventArgs);
}