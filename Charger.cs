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
        #pragma warning restore 0649

        ChargerController controller;
        Stopwatch stopwatch;
        IWeapon weapon;
        Transform target = null;
        bool idle = false;
        bool charging = false;

        void Awake()
        {
            controller = GetComponent<ChargerController>();
            stopwatch = GameObject.FindWithTag("Globals").GetComponent<Stopwatch>();
            weapon = GetComponent<MeleeWeapon>();
        }

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
                {
                    SmartCoroutine attack = SmartCoroutine.Create(controller.Attack(weapon));
                    yield return attack;
                    if (attack.Status == SmartCoroutine.Result.WasExited)
                        attackFailed = true;
                }
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
            if (!controller.SetupComplete || (controller.CurrentActions & Actions.Attack) != 0)
                return;
            if (!idle && target == null)
                StartCoroutine(IdleRoutine());
            else if (target != null && !charging)
            {
                charging = true;
                StartCoroutine(ChargeRoutine());
            }
        }
    }
}
