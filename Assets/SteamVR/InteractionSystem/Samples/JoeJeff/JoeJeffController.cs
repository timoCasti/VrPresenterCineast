﻿using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    public class JoeJeffController : MonoBehaviour
    {
        [SteamVR_DefaultAction("Jump", "platformer")]
        public SteamVR_Action_Boolean a_jump;

        [SteamVR_DefaultAction("Move", "platformer")]
        public SteamVR_Action_Vector2 a_move;


        [SteamVR_DefaultActionSet("platformer")]
        public SteamVR_ActionSet actionSet;


        public JoeJeff character;
        private float glow;
        private SteamVR_Input_Sources hand;
        private Interactable interactable;
        public float joyMove = 0.1f;
        public Transform Joystick;
        private bool jump;

        public Renderer jumpHighlight;


        private Vector3 movement;

        private void Start()
        {
            interactable = GetComponent<Interactable>();
            interactable.activateActionSetOnAttach = actionSet;
        }

        private void Update()
        {
            if (interactable.attachedToHand)
            {
                hand = interactable.attachedToHand.handType;
                var m = a_move.GetAxis(hand);
                movement = new Vector3(m.x, 0, m.y);

                jump = a_jump.GetStateDown(hand);
                glow = Mathf.Lerp(glow, a_jump.GetState(hand) ? 1.5f : 1.0f, Time.deltaTime * 20);
            }
            else
            {
                movement = Vector2.zero;
                jump = false;
                glow = 0;
            }

            Joystick.localPosition = movement * joyMove;

            var rot = transform.eulerAngles.y;

            movement = Quaternion.AngleAxis(rot, Vector3.up) * movement;

            jumpHighlight.sharedMaterial.SetColor("_EmissionColor", Color.white * glow);

            character.Move(movement * 2, jump);
        }
    }
}