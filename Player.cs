using UnityEngine;

namespace ProjectZombie
{
    [RequireComponent(typeof(PlayerController))]
    public class Player: Actor
    {
        PlayerController controller;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
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
            controller.Turn(Input.GetAxis("Mouse X"));
        }
    }
}
