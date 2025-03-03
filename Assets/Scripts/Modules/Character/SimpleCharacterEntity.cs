using System;
using UnityEngine;
using VContainer.Unity;
using System.Collections.Generic;
using PrimeTween;
using VContainer;

namespace SETHD.FantasySnake.Character
{
    public class SimpleCharacterFactory
    {
        private readonly IObjectResolver container;
        
        [Inject]
        public SimpleCharacterFactory(IObjectResolver container)
        {
            this.container = container;
        }

        public ICharacterEntity Create(Vector3 position, CharacterData data)
        {
            var instance = container.Instantiate(data.Prefab, position, Quaternion.identity);
            instance.Initialize(data);
            return instance;
        }
    }
    
    public class SimpleCharacterEntity : MonoBehaviour, ICharacterEntity
    {
        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Attack => attack;
        public float Defence => defence;

        public float LookPercentage
        {
            get => lookPercentage;
            set => lookPercentage = value;
        }
        public Vector3 CenterPosition => centerObject.position;
        public Vector3 CurrentPosition => transform.position;
        public List<(Vector3, Quaternion)> TargetMovementData => targetMovementData;
        
        private const float SNAP_THRESHOLD = 0.1f;

        [SerializeField]
        private Transform centerObject;

        private bool isAtTargetPosition;
        private float health;
        private float maxHealth;
        private float attack;
        private float defence;
        private float moveSpeed;
        private float lookPercentage;
        
        private List<(Vector3, Quaternion)> targetMovementData;

        public void Initialize(CharacterData data)
        {
            isAtTargetPosition = true;
            health = maxHealth = data.Health;
            attack = data.Attack;
            defence = data.Defence;
            moveSpeed = data.MoveSpeed;
            gameObject.name = data.HeroName;
            targetMovementData = new List<(Vector3, Quaternion)>();
        }
        
        public void FixedUpdate()
        {
            if (targetMovementData.Count <= 0)
                return;
            
            if (isAtTargetPosition)
                return;
            
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetMovementData[0].Item1, moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetMovementData[0].Item2, moveSpeed * Time.fixedDeltaTime);
            var distance = Vector3.Distance(gameObject.transform.position, targetMovementData[0].Item1);
            
            if (distance > SNAP_THRESHOLD)
                return;

            gameObject.transform.position = targetMovementData[0].Item1;
            targetMovementData.RemoveAt(0);
            
            if (targetMovementData.Count > 0)
                return;
            
            isAtTargetPosition = true;
        }

        public void Heal(float heal)
        {
            health += heal;
            
            if (health > maxHealth)
                health = maxHealth;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            
            if (health < 0)
                health = 0;
        }

        public void MoveTo(Vector3 targetPosition)
        {
            isAtTargetPosition = false;
            targetMovementData.Add((targetPosition, Quaternion.LookRotation(targetPosition - transform.position)));
        }

        public void RotateTo(Vector3 targetPosition)
        {
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
        }
        
        public void TeleportTo(Vector3 targetPosition)
        {
            isAtTargetPosition = true;
            transform.position = targetPosition;
            targetMovementData.Clear();
        }
    }
}