using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

namespace ProjectZombie
{
    [RequireComponent(typeof(ChargerController))]
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
        GameObject players;
        Stopwatch stopwatch;
        Transform target = null;
        SmartCoroutine chargeRoutine = null;
        bool idle = false;

        void Awake()
        {
            controller = GetComponent<ChargerController>();
            players = GameObject.FindWithTag("PlayerContainer");
            stopwatch = GameObject.FindWithTag("Globals").GetComponent<Stopwatch>();
        }

        bool SelectVisibleTarget()
        {
            Transform[] visiblePlayers =
            (
                from player in players.GetComponentsInChildren<Player>()
                select player.gameObject.transform into tform
                where IsVisible(tform, visibleDistance, maxVisibleAngle)
                select tform
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

        [SmartCoroutineEnabled]
        IEnumerator ChargeRoutine()
        {
            idle = false;
            float startTime = stopwatch.ElapsedSeconds;
            yield return controller.Follow(target, Actions.NormalMove, waypointDistance, searchRadius,
                exitCondition: () => stopwatch.ElapsedSeconds - startTime > maxSecondsPerCharge);
            target = null;
            chargeRoutine = null;
        }

        void Update()
        {
            if (!controller.SetupComplete)
                return;
            if (!idle && target == null)
                StartCoroutine(IdleRoutine());
            else if (target != null && chargeRoutine == null)
            {
                chargeRoutine = new SmartCoroutine(ChargeRoutine());
                StartCoroutine(chargeRoutine);
            }
        }
    }
}
