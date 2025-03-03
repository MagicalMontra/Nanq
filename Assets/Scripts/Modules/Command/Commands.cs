using UnityEngine;
using VitalRouter;

namespace SETHD.FantasySnake.Command
{
    public readonly struct GameReadyCommand : ICommand
    {
        public object MapController { get; init; }
    }
    
    public readonly struct PlayerMovementCommand : ICommand
    {
        public float X { get; init; }
        public float Y { get; init; }
        public Vector3 Direction => new(X, 0, Y);
    }

    public readonly struct PreventMovementCommand : ICommand
    {
        public bool IsPrevented { get; init; }
    }
    
    public readonly struct MapCreateCommand : ICommand
    {
        
    }

    public readonly struct HeroHoverCommand : ICommand
    {
        public string Name { get; init; }
        public string Attack { get; init; }
        public string Defense { get; init; }
        public string Health { get; init; }
    }

    public readonly struct HoverCancelHoverCommand : ICommand
    {
        
    }
}
