using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectZombie
{
    [RequireComponent(typeof(ChargerController))]
    [RequireComponent(typeof(MeleeWeapon))]
    public class Charger: AIActor
    {
        #pragma warning disable 0649
        [SerializeField] float secondsBetweenTurns;
        [SerializeField] float secondsBeforeTurn;
        [SerializeField] List<float> turnSequence = new List<float>();
        [SerializeField] float visibleDistance;
        [SerializeField] float maxVisibleAngle;
        [SerializeField] float maxSecondsPerCharge;
        [SerializeField] float waypointDistance;
        [SerializeField] float searchRadius;
        [SerializeField] float secondsToDie;
        #pragma warning restore 0649

        ChargerController controller;
        Stopwatch stopwatch;
        IWeapon weapon;
        Transform target = null;
        bool idle = false;
        bool charging = false;

        bool SelectVisibleTarget()
        {
            Transform[] visiblePlayers =
            (
                from player in GameObject.FindGameObjectsWithTag("Player")
                where IsVisible(player.transform, visibleDistance, maxVisibleAngle)
                select player.transform
            ).ToArray();
            if (visiblePlayers.Length == 0)
                return false;
            else
            {
                target = visiblePlayers[Random.Range(0, visiblePlayers.Length)];
                return true;
            }
        }

        IEnumerable<float> TurnGenerator()
        {
            while (true)
            {
                foreach (float turn in turnSequence)
                    yield return turn;
            }
        }

        IEnumerator IdleRoutine()
        {
            idle = true;
            yield return new WaitForSeconds(secondsBeforeTurn);
            yield return controller.TurnInPlace(TurnGenerator(), secondsBetweenTurns, SelectVisibleTarget);
        }

        IEnumerator AttackRoutine()
        {
            bool attackFailed = false;
            do
            {
                if (weapon.CanAttack)
                    yield return SmartCoroutine.Create(controller.Attack(weapon), () => attackFailed = true);
                else
                    yield return null;
            }
            while (!attackFailed);
        }

        IEnumerator ChargeRoutine()
        {
            idle = false;
            float startTime = stopwatch.ElapsedSeconds;
            yield return controller.Follow(target, Actions.NormalMove, waypointDistance, searchRadius,
                breakCondition: () => stopwatch.ElapsedSeconds - startTime > maxSecondsPerCharge,
                onReach: () => StartCoroutine(AttackRoutine()));
            target = null;
            charging = false;
        }

        void Update()
        {
            if (hitPoints <= 0)
                OnDeath();
            else if (controller.SetupComplete && (CurrentActions & Actions.Attack) == 0)
            {
                if (!idle && target == null)
                    StartCoroutine(IdleRoutine());
                else if (target != null && !charging)
                {
                    charging = true;
                    StartCoroutine(ChargeRoutine());
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            controller = (ChargerController)Controller;
            stopwatch = GameObject.FindWithTag("Globals").GetComponent<Stopwatch>();
            weapon = GetComponent<MeleeWeapon>();
        }

        public override void OnDeath()
        {
            StopAllCoroutines();
            Object.Destroy(gameObject, secondsToDie);
        }
    }
}
