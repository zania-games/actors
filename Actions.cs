using System;
using UnityEngine;

namespace ProjectZombie
{
    [Flags]
    public enum Actions
    {
        None = 0,
        SlowMove = 1,
        NormalMove = 2,
        FastMove = 4,
        Jump = 8,
        Turn = 16,
        Movement = SlowMove | NormalMove | FastMove | Jump
    }

    public delegate void MoveMethod(Vector3 direction);
}
