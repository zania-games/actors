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
        Attack,
        Movement = SlowMove | NormalMove | FastMove | Jump
    }
}
