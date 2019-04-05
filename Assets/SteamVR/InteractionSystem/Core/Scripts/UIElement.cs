//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class UIElement : MonoBehaviour
    {
        private Hand currentHand;
        public CustomEvents.UnityEventHand onHandClick;

        //-------------------------------------------------
        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button) button.onClick.AddListener(OnButtonClick);
        }


        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand)
        {
            currentHand = hand;
            InputModule.instance.HoverBegin(gameObject);
            ControllerButtonHints.ShowButtonHint(hand, hand.uiInteractAction);
        }


        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            InputModule.instance.HoverEnd(gameObject);
            ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);
            currentHand = null;
        }


        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            if (hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType))
            {
                InputModule.instance.Submit(gameObject);
                ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);
            }
        }


        //-------------------------------------------------
        private void OnButtonClick()
        {
            onHandClick.Invoke(currentHand);
        }
    }

#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [CustomEditor(typeof(UIElement))]
    public class UIElementEditor : Editor
    {
        //-------------------------------------------------
        // Custom Inspector GUI allows us to click from within the UI
        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var uiElement = (UIElement) target;
            if (GUILayout.Button("Click")) InputModule.instance.Submit(uiElement.gameObject);
        }
    }
#endif
}