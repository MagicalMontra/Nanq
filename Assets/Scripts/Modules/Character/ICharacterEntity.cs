using System.Collections.Generic;
using UnityEngine;

namespace SETHD.FantasySnake.Character
{
    public interface ICharacterEntity
    {
        float Health { get; }
        float MaxHealth { get; }
        float Attack { get; }
        float Defence { get; }
        
        List<Vector3> LastPositions { get; }
        
        void Heal(float heal);
        void TakeDamage(float damage);
        void MoveTo(Vector3 position);
    }
}