using UnityEngine;

namespace ProjectZombie
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(MeleeWeapon))]
    public class Player: Actor
    {
        IWeapon equippedWeapon;

        void Update()
        {
            if (!Controller.SetupComplete)
                return;
            if ((CurrentActions & Actions.Jump) == 0)
            {
                if (Input.GetButtonDown("Jump"))
                    StartCoroutine(Controller.Jump());
                else
                {
                    Vector3 direction =
                        Vector3.Normalize(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
                    if (Input.GetButton("Walk"))
                        Controller.SlowMove(direction);
                    else if (Input.GetButton("Sprint"))
                        Controller.FastMove(direction);
                    else
                        Controller.NormalMove(direction);
                }
            }
            if ((CurrentActions & Actions.Attack) == 0 && equippedWeapon.CanAttack && Input.GetButtonDown("Fire1"))
                StartCoroutine(SmartCoroutine.Create(Controller.Attack(equippedWeapon)));
            Controller.Turn(Input.GetAxis("Mouse X"));
        }

        protected override void Awake()
        {
            base.Awake();
            equippedWeapon = GetComponent<MeleeWeapon>();
        }

        public override void OnDeath()
        {
            // TODO
        }
    }
}
