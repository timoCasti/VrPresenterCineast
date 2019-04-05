using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    public class LockToPoint : MonoBehaviour
    {
        private Rigidbody body;

        private float dropTimer;
        private Interactable interactable;
        public float snapTime = 2;
        public Transform snapTo;

        private void Start()
        {
            interactable = GetComponent<Interactable>();
            body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var used = false;
            if (interactable != null)
                used = interactable.attachedToHand;

            if (used)
            {
                body.isKinematic = false;
                dropTimer = -1;
            }
            else
            {
                dropTimer += Time.deltaTime / (snapTime / 2);

                body.isKinematic = dropTimer > 1;

                if (dropTimer > 1)
                {
                    //transform.parent = snapTo;
                    transform.position = snapTo.position;
                    transform.rotation = snapTo.rotation;
                }
                else
                {
                    var t = Mathf.Pow(35, dropTimer);

                    body.velocity = Vector3.Lerp(body.velocity, Vector3.zero, Time.fixedDeltaTime * 4);
                    if (body.useGravity)
                        body.AddForce(-Physics.gravity);

                    transform.position = Vector3.Lerp(transform.position, snapTo.position, Time.fixedDeltaTime * t * 3);
                    transform.rotation =
                        Quaternion.Slerp(transform.rotation, snapTo.rotation, Time.fixedDeltaTime * t * 2);
                }
            }
        }
    }
}