﻿using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerTeleporter : MonoBehaviour
{
    public Vector3 Destination = Vector3.zero;


    public float MinDistance = 1f;

    public void TeleportPlayer()
    {
        SteamVR_Fade.Start(Color.clear, 0.2f);

        var pgo = GameObject.FindWithTag("Player");
        if (pgo == null) Debug.LogWarning("No player found!");

        var _player = pgo.GetComponent<Player>();

        if (_player == null) Debug.LogWarning("No SteamVR Player attached!");


        var playerFeetOffset = _player.trackingOriginTransform.position - _player.feetPositionGuess;
        _player.trackingOriginTransform.position = Destination + playerFeetOffset;
    }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}