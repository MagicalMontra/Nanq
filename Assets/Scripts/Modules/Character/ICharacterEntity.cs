using System;
using System.Collections.Generic;
using UnityEngine;

namespace SETHD.FantasySnake.Character
{
    [Serializable]
    public enum CharacterInteractionType
    {
        Player = 0,
        Collectable,
        Enemy
    }
    public interface ICharacterEntity
    {
        float Health { get; }
        float MaxHealth { get; }
        float Attack { get; }
        float Defence { get; }
        float LookPercentage { get; set; }
        GameObject gameObject { get; }
        Vector3 CenterPosition { get; }
        Vector3 CurrentPosition { get; }
        List<(Vector3, Quaternion)> TargetMovementData { get; }
        void Heal(float heal);
        void Destroy();
        void TakeDamage(float damage);
        void MoveTo(Vector3 position);
        void RotateTo(Vector3 position);
        void TeleportTo(Vector3 position);
    }
}