//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem.Sample
{
    public class TargetMeasurement : MonoBehaviour
    {
        public bool drawTape;

        public Transform endPoint;

        private float lastDistance;

        public float maxDistanceToDraw = 6f;
        public Transform measurementTape;
        public Text measurementTextFT;
        public Text measurementTextM;
        public GameObject visualWrapper;

        private void Update()
        {
            if (Camera.main != null)
            {
                var fromPoint = Camera.main.transform.position;
                fromPoint.y = endPoint.position.y;

                var distance = Vector3.Distance(fromPoint, endPoint.position);

                var center = Vector3.Lerp(fromPoint, endPoint.position, 0.5f);

                transform.position = center;
                transform.forward = endPoint.position - fromPoint;
                measurementTape.localScale = new Vector3(0.05f, distance, 0.05f);

                if (Mathf.Abs(distance - lastDistance) > 0.01f)
                {
                    measurementTextM.text = distance.ToString("00.0m");
                    measurementTextFT.text = (distance * 3.28084).ToString("00.0ft");

                    lastDistance = distance;
                }

                if (drawTape)
                    visualWrapper.SetActive(distance < maxDistanceToDraw);
                else
                    visualWrapper.SetActive(false);
            }
        }
    }
}