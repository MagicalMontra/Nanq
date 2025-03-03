using System;
using UnityEngine;
using VitalRouter;
using System.Threading;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using SETHD.FantasySnake.Command;

namespace SETHD.FantasySnake.Input
{
    public class PlayerMovementInputController : IDisposable, IStartable
    {
        private readonly GameplayInput gameplayInput;

        private bool isPreciseMovement = true;
        private bool isButtonDown;
        private Vector2 heldDirection;
        private ICommandPublisher publisher;
        private CancellationTokenSource cancellationTokenSource;

        public PlayerMovementInputController(Router router, GameplayInput gameplayInput)
        {
            publisher = router;
            this.gameplayInput = gameplayInput;
            cancellationTokenSource = new CancellationTokenSource();
            this.gameplayInput.CharacterControls.Movement.started += OnButtonDown;
            this.gameplayInput.CharacterControls.Movement.performed += OnButtonHold;
            this.gameplayInput.CharacterControls.Movement.canceled += OnButtonUp;
        }
        
        public void Start()
        {
            Enable();
        }
        
        public void Dispose()
        {
            gameplayInput.CharacterControls.Movement.started -= OnButtonDown;
            gameplayInput.CharacterControls.Movement.performed -= OnButtonHold;
            gameplayInput.CharacterControls.Movement.canceled -= OnButtonUp;
            gameplayInput.Disable();
        }

        public void Enable()
        {
            gameplayInput.Enable();
        }

        public void Disable()
        {
            gameplayInput.Disable();
        }

        private void OnButtonDown(InputAction.CallbackContext callbackContext)
        {
            isButtonDown = true;
        }

        private void OnButtonHold(InputAction.CallbackContext callbackContext)
        {
            if (!isButtonDown)
                return;
            
            heldDirection = callbackContext.ReadValue<Vector2>();
            
            if (isPreciseMovement)
                return;
            
            SendCommand().Forget();
        }

        private void OnButtonUp(InputAction.CallbackContext callbackContext)
        {
            if (!isButtonDown)
                return;

            if (!isPreciseMovement)
            {
                cancellationTokenSource?.Cancel();
                return;
            }

            SendCommand().Forget();
        }
        
        private async UniTask SendCommand()
        {
            cancellationTokenSource ??= new CancellationTokenSource();
            var command = new PlayerMovementCommand{ X = heldDirection.x, Y = heldDirection.y };
            await publisher.PublishAsync(command, cancellationTokenSource.Token);
        }
    }
}
