using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VitalRouter;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace SETHD.FantasySnake.Input
{
    public readonly struct PlayerMovementCommand : ICommand
    {
        public float X { get; init; }
        public float Y { get; init; }
    }

    public class PlayerMovementInputValidator : TypedCommandInterceptro<PlayerMovementCommand>
    {
        private Vector2? lastDirection;
        
        public override async ValueTask InvokeAsync(PlayerMovementCommand command, PublishContext context, PublishContinuation<PlayerMovementCommand> next)
        {
            await next(ValidateDiagonalMovement(command), context);
        }

        private PlayerMovementCommand ValidateDiagonalMovement(PlayerMovementCommand command)
        {
            //NOTE: If is only horizontal input then record it as the last input and allows
            if (command.X != 0 && command.Y == 0)
            {
                lastDirection = new Vector2(command.X, command.Y);
                return command;
            }

            //NOTE: If is only vertical input then record it as the last input and allows
            if (command.X == 0 && command.Y != 0)
            {
                lastDirection = new Vector2(command.X, command.Y);
                return command;
            }

            //NOTE: In case that player input a diagonal input and has previous input recorded then handle it accordingly
            if (lastDirection.HasValue)
            {
                //NOTE: If the last input was horizontal then we change the direction to vertical
                if (lastDirection.Value.x != 0 && lastDirection.Value.y == 0 && command.Y != 0)
                {
                    lastDirection = new Vector2(0, command.Y);
                    return new PlayerMovementCommand{ X = 0, Y = command.Y };
                }
                
                //NOTE: vice versa if the last input was vertical then switch it to horizontal
                if (lastDirection.Value.x == 0 && lastDirection.Value.y != 0 && command.X != 0)
                {
                    lastDirection = new Vector2(command.X, 0);
                    return new PlayerMovementCommand{ X = command.X, Y = 0 };
                }
                
                //NOTE: else then we just proceed with the last input
                return new PlayerMovementCommand{ X = lastDirection.Value.x, Y = lastDirection.Value.y };
            }
            
            return new PlayerMovementCommand{ X = 0, Y = 0 };
        }
    }
    
    public class PlayerMovementInputController : IDisposable
    {
        private readonly GameplayInput gameplayInput;

        private bool isPreciseMovement;
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

            Enable();
        }
        
        public void Dispose()
        {
            gameplayInput.CharacterControls.Movement.started -= OnButtonDown;
            gameplayInput.CharacterControls.Movement.performed -= OnButtonHold;
            gameplayInput.CharacterControls.Movement.canceled -= OnButtonUp;
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
