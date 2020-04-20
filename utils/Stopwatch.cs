using UnityEngine;

namespace ProjectZombie
{
    public class Stopwatch: MonoBehaviour
    {
        void Update() => ElapsedSeconds += Time.deltaTime;

        public float ElapsedSeconds {get; private set;} = 0;
    }
}
