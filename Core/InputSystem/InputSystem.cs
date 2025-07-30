using UnityEngine.InputSystem;
using UnityEngine;

namespace Xuf.Core
{
    // public class CInputSystem : IGameSystem
    // {
    //     public int Priority => 1000;

    //     InputAction move;

    //     InputAction joinGame;
    //     // Track devices that have already joined to prevent duplicate player creation
    //     // private HashSet<InputDevice> _joinedDevices = new HashSet<InputDevice>();


    //     public CInputSystem(Transform gameEntry)
    //     {
    //         var inputManager = gameEntry.Find("InputManager");
    //         var playerInput = inputManager.GetComponent<PlayerInput>();
    //         playerInput.actions.FindActionMap("Player").Disable();
    //         // InputSystem.
    //         // var devices = InputSystem.devices;
    //         // Debug.Log(InputSystem.devices);
    //         move = InputSystem.actions.FindActionMap("Player").FindAction("Move");
    //         joinGame = InputSystem.actions.FindActionMap("UI").FindAction("JoinGame");
    //         joinGame.performed += OnJoinGamePerformed;
    //         // Debug.Log(move);
    //     }

    //     public void Update(float deltaTime, float unscaledDeltaTime)
    //     {
    //         if (move.IsPressed())
    //         {
    //             Debug.Log(move.ReadValue<Vector2>() * 10);
    //         }
    //     }

    //     private void OnJoinGamePerformed(InputAction.CallbackContext context)
    //     {
    //         Debug.Log($"OnJoinGamePerformed: {context.control.device}");
    //         // var device = context.control.device;
    //         // // Check if the device has already joined
    //         // if (_joinedDevices.Contains(device))
    //         // {
    //         //     Debug.Log($"Device {device.displayName} has already joined. Ignoring this request.");
    //         //     return;
    //         // }
    //         // _joinedDevices.Add(device);

    //         // Debug.Log($"JoinGame: {device}");

    //         // var player = Resources.Load<GameObject>("Prefabs/Player/Cube");
    //         // // Instantiate player and pair with the current device
    //         // var playerInstance = PlayerInput.Instantiate(player, pairWithDevice: device);
    //         // playerInstance.transform.position = new Vector3(0, 0, 0);
    //     }
    // }
}
