using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class Grenade : MonoBehaviour
    {
        public int explodeCount = 10;
        public GameObject explodePartPrefab;

        private Interactable interactable;

        public float minMagnitudeToExplode = 1f;

        private void Start()
        {
            interactable = GetComponent<Interactable>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (interactable != null && interactable.attachedToHand != null) //don't explode in hand
                return;

            if (collision.impulse.magnitude > minMagnitudeToExplode)
            {
                for (var explodeIndex = 0; explodeIndex < explodeCount; explodeIndex++)
                {
                    var explodePart = Instantiate(explodePartPrefab, transform.position, transform.rotation);
                    explodePart.GetComponentInChildren<MeshRenderer>().material
                        .SetColor("_TintColor", Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
                }

                Destroy(gameObject);
            }
        }
    }
}