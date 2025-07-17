using UnityEngine.InputSystem;
using UnityEngine;

namespace Xuf.Core
{
    public class CInputSystem : IGameSystem
    {
        public int Priority => 100;

        InputAction move;

        InputAction joinGame;


        public CInputSystem()
        {
            var devices = InputSystem.devices;
            Debug.Log(InputSystem.devices);
            move = InputSystem.actions.FindActionMap("Player").FindAction("Move");
            joinGame = InputSystem.actions.FindActionMap("UI").FindAction("JoinGame");
            joinGame.performed += OnJoinGamePerformed;
            Debug.Log(move);
        }

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (move.IsPressed())
            {
                Debug.Log(move.ReadValue<Vector2>() * 10);
            }


        }

        private void OnJoinGamePerformed(InputAction.CallbackContext context)
        {
            Debug.Log($"JoinGame: {context.control.device}");


            var player = Resources.Load<GameObject>("Prefabs/Player/Cube");
            var playerInstance = PlayerInput.Instantiate(player, pairWithDevice: context.control.device);
            // var playerInput = playerInstance.AddComponent<PlayerInput>();
            // playerInput.actions = InputSystem.actions;

            playerInstance.transform.position = new Vector3(0, 0, 0);

            // playerInput.user.UnpairDevicesAndRemoveUser();
            // playerInput.user.AssociateActionsWithUser(playerInput.actions);
            // playerInput.user.p
        }
    }
}
