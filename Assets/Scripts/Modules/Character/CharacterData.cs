using System;
using UnityEngine;

namespace SETHD.FantasySnake.Character
{
    [Serializable]
    public class CharacterData
    {
        public float Health => health;
        public float Attack => attack;
        public float Defence => defence;
        public float MoveSpeed => moveSpeed;

        [SerializeField]
        private float health = 100f;

        [SerializeField]
        private float attack = 10f;

        [SerializeField]
        private float defence = 2.5f;

        [SerializeField]
        private float moveSpeed = 4f;
    }
}
