using VitalRouter;
using UnityEngine;
using System.Threading.Tasks;
using SETHD.FantasySnake.Command;

namespace SETHD.FantasySnake.Input
{
    public class PlayerMovementInputValidator : TypedCommandInterceptro<PlayerMovementCommand>
    {
        private Vector2? lastDirection;
        
        public override async ValueTask InvokeAsync(PlayerMovementCommand command, PublishContext context, PublishContinuation<PlayerMovementCommand> next)
        {
            var nextCommand = ValidateDiagonalMovement(command);
#if DEBUG
            Debug.Log(nextCommand.X + ", " + nextCommand.Y);            
#endif
            await next(nextCommand, context);
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
}