using UnityEngine;

namespace ProjectZombie
{
    public class Stopwatch: MonoBehaviour
    {
        public float ElapsedSeconds {get; private set;} = 0;

        void Update() => ElapsedSeconds += Time.deltaTime;
    }
}
