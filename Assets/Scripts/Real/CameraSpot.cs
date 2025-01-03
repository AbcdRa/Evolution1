
using System;
using UnityEngine;

public class CameraSpot : MonoBehaviour
{
    [SerializeField] private int _player_id;
    [SerializeField] private bool _is_private;

    internal int GetPlayerId()
    {
        return _player_id;
    }

    internal bool IsPrivate()
    {
        return _is_private;
    }
}

