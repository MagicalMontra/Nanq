using UnityEngine;
using VContainer.Unity;
using System.Collections.Generic;

namespace SETHD.FantasySnake.Character
{
    public class BaseCharacterEntity : ICharacterEntity, IFixedTickable
    {
        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Attack => attack;
        public float Defence => defence;
        public List<Vector3> LastPositions => lastPositions;
        
        private float SNAP_THRESHOLD = 0.1f;

        private bool isAtTargetPosition;
        private float health;
        private float maxHealth;
        private float attack;
        private float defence;
        private float moveSpeed;
        
        private Vector3 targetPosition;
        private GameObject gameObject;
        private List<Vector3> lastPositions;

        public BaseCharacterEntity(CharacterData data)
        {
            health = maxHealth = data.Health;
            attack = data.Attack;
            defence = data.Defence;
            moveSpeed = data.MoveSpeed;
            lastPositions = new List<Vector3>();
        }
        
        public void Heal(float heal)
        {
            health += heal;
            
            if (health > maxHealth)
                health = maxHealth;
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
            this.targetPosition = targetPosition;
            lastPositions.Add(gameObject.transform.position);
        }

        public void FixedTick()
        {
            if (isAtTargetPosition)
                return;
            
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, moveSpeed * Time.fixedTime);
            var distance = Vector3.Distance(gameObject.transform.position, targetPosition);
            
            if (distance > SNAP_THRESHOLD)
                return;

            isAtTargetPosition = true;
            lastPositions.RemoveAt(0);
            gameObject.transform.position = targetPosition;
        }
    }
}