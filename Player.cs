using UnityEngine;

namespace ProjectZombie
{
    [RequireComponent(typeof(PlayerController))]
    public class Player: Actor
    {
        PlayerController controller;
        IWeapon equippedWeapon;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            equippedWeapon = GetComponent<MeleeWeapon>();
        }

        void Update()
        {
            if (!controller.SetupComplete)
                return;
            if ((controller.CurrentActions & Actions.Jump) == 0)
            {
                if (Input.GetButtonDown("Jump"))
                    StartCoroutine(controller.Jump());
                else
                {
                    Vector3 direction =
                        Vector3.Normalize(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
                    if (Input.GetButton("Walk"))
                        controller.SlowMove(direction);
                    else if (Input.GetButton("Sprint"))
                        controller.FastMove(direction);
                    else
                        controller.NormalMove(direction);
                }
            }
            if ((controller.CurrentActions & Actions.Attack) == 0 && equippedWeapon.CanAttack &&
                Input.GetButtonDown("Fire1"))
                StartCoroutine(SmartCoroutine.Create(controller.Attack(equippedWeapon)));
            controller.Turn(Input.GetAxis("Mouse X"));
        }
    }
}
