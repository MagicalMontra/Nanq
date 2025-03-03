using System;
using UnityEngine;
using SETHD.Utilis;

namespace SETHD.FantasySnake.Character
{
    [Serializable]
    [CreateAssetMenu(menuName = "Character/Create CharacterData", fileName = "CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        public string HeroName => heroName;
        public float Health => health;
        public float Attack => attack;
        public float Defence => defence;
        public float MoveSpeed => moveSpeed;
        public SimpleCharacterEntity Prefab => prefab;

        [SerializeField]
        private string heroName;

        [SerializeField]
        private float health = 100f;

        [SerializeField]
        private float attack = 10f;

        [SerializeField]
        private float defence = 2.5f;

        [SerializeField]
        private float moveSpeed = 4f;

        [SerializeField]
        private SimpleCharacterEntity prefab;
    }
}
