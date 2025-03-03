using System;
using VContainer;
using VitalRouter;
using UnityEngine;
using VContainer.Unity;
using SETHD.FantasySnake.Map;
using UnityEngine.Assertions;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using SETHD.FantasySnake.Camera;
using SETHD.FantasySnake.Command;
using Random = UnityEngine.Random;
using SETHD.FantasySnake.Character;
using UnityEngine.SceneManagement;

namespace SETHD.FantasySnake.Game
{
    [Routes]
    public partial class GameEntityController : IFixedTickable
    {
        private const int ALLY_LIMIT = 5;
        private const int MAP_ENTITY_UPPER_LIMIT = 15;
        private const float SPAWN_MIN_TIME = 2.5f;
        private const float SPAWN_MAX_TIME = 5f;
        private const float LOOK_PERCENTAGE_THRESHOLD = 0.99f;
        
        private readonly SimpleCharacterFactory factory;

        private float countSpawnTime;
        private float currentSpawnTime;
        private bool hasGameStarted;
        private bool pauseUpdate;
        private int lastLookIndex;
        private Vector3 lastDirection;
        private Vector3 tailDirection;
        private ICommandPublisher router;
        private Vector3 purgatoryPosition = Vector3.one * 1000;
        private GameEntityData gameEntityData;
        private CameraController cameraController;
        private SimpleMapController mapController;
        private List<ICharacterEntity> playerCharacters;
        private List<Func<ICharacterEntity>> queuedHeroCollections;
        
        private Dictionary<int, (CharacterInteractionType, ICharacterEntity)> mapCharacters;

        [Inject]
        public GameEntityController(Router router, CameraController cameraController, GameEntityData gameEntityData, SimpleCharacterFactory factory)
        {
            this.router = router;
            this.factory = factory;
            this.gameEntityData = gameEntityData;
            this.cameraController = cameraController;
            playerCharacters = new List<ICharacterEntity>();
            queuedHeroCollections = new List<Func<ICharacterEntity>>();
            mapCharacters = new Dictionary<int, (CharacterInteractionType, ICharacterEntity)>();
        }

        [Route]
        private void OnGameReadied(GameReadyCommand command)
        {
            mapController = command.MapController as SimpleMapController;
            playerCharacters.Add(SpawnCharacter(CharacterInteractionType.Player));
            cameraController.Focus(playerCharacters[0].gameObject);
            hasGameStarted = true;
        }

        [Route(CommandOrdering.Drop)]
        private async UniTask OnMoveCommand(PlayerMovementCommand command)
        {
            if (-command.Direction == lastDirection)
                return;
            
            var desiredPosition = mapController.GetCellPositionByPosition(playerCharacters[0].CurrentPosition + (command.Direction * 4));
            
            if (!desiredPosition.HasValue)
                return;
            
            var headDestinationIndex = mapController.GetMapIndexByPosition(desiredPosition.Value);
            Assert.IsTrue(headDestinationIndex > -1);
            
            if (mapCharacters.ContainsKey(headDestinationIndex))
            {
                if (mapCharacters[headDestinationIndex].Item1 == CharacterInteractionType.Player)
                {
                    MoveSnake(headDestinationIndex, desiredPosition.Value, command.Direction);
                    await UniTask.WaitUntil(() => Vector3.Distance(playerCharacters[0].CurrentPosition, desiredPosition.Value) < 0.5f);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    return;
                }

                if (mapCharacters[headDestinationIndex].Item1 == CharacterInteractionType.Collectable)
                {
                    var tailPosition = mapController.GetCellPositionByPosition(playerCharacters[^1].CurrentPosition + tailDirection);

                    if (!tailPosition.HasValue)
                    {
                        TeleportCharacter(mapCharacters[headDestinationIndex].Item2, purgatoryPosition);
                        queuedHeroCollections.Add(() => TeleportCharacter(mapCharacters[headDestinationIndex].Item2));
                        mapCharacters.Remove(headDestinationIndex);
                        Debug.Log("Waiting new character to player");
                    }
                    else
                    {
                        var teleportedCharacter = TeleportCharacter(mapCharacters[headDestinationIndex].Item2);
                        playerCharacters.Add(teleportedCharacter);
                        int tailIndex = mapController.GetMapIndexByPosition(tailPosition.Value);
                        mapCharacters[tailIndex] = (CharacterInteractionType.Player, teleportedCharacter);
                        mapCharacters.Remove(headDestinationIndex);
                        Debug.Log("Added new character to player");
                    }
                    
                    MoveSnake(headDestinationIndex, desiredPosition.Value, command.Direction);
                    await UniTask.WaitUntil(() => Vector3.Distance(playerCharacters[0].CurrentPosition, desiredPosition.Value) < 0.1f);
                    return;
                }

                if (mapCharacters[headDestinationIndex].Item1 == CharacterInteractionType.Enemy)
                {
                    pauseUpdate = true;
                    playerCharacters[0].RotateTo(desiredPosition.Value);
                    var enemy = mapCharacters[headDestinationIndex].Item2;
                    while (enemy.Health > 0)
                    {
                        await Battle(playerCharacters[0], enemy);

                        int headIndex = mapController.GetMapIndexByPosition(playerCharacters[0].CurrentPosition);
                        
                        if (lastLookIndex == headDestinationIndex || lastLookIndex == headIndex)
                        {
                            var character = mapCharacters[headDestinationIndex];
                            
                            router.PublishAsync(new HeroHoverCommand
                            {
                                Name = character.Item2.gameObject.name + "(" + character.Item1 + ")" ,
                                Attack = character.Item2.Attack.ToString(), 
                                Defense = character.Item2.Defence.ToString(), 
                                Health = character.Item2.Health + "/" + character.Item2.MaxHealth
                            });
                        }
                        
                        Debug.Log($"player {playerCharacters[0].Health} enemy {enemy.Health}");

                        if (playerCharacters[0].Health <= 0)
                        {
                            if (playerCharacters.Count == 1)
                            {
                                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                                break;
                            }
                            
                            var replacePosition = playerCharacters[0].CurrentPosition;
                            var replacePositionIndex = mapController.GetMapIndexByPosition(replacePosition);
                            playerCharacters[0].Destroy();
                            playerCharacters.RemoveAt(0);
                            cameraController.Focus(playerCharacters[0].gameObject);
                            MoveSnake(replacePositionIndex, replacePosition, command.Direction);
                            await UniTask.WaitUntil(() => Vector3.Distance(playerCharacters[0].CurrentPosition, replacePosition) < 0.1f);
                        }

                        if (enemy.Health <= 0)
                        {
                            enemy.Destroy();
                            mapCharacters.Remove(headDestinationIndex);
                            MoveSnake(headDestinationIndex, desiredPosition.Value, command.Direction);
                            await UniTask.WaitUntil(() => Vector3.Distance(playerCharacters[0].CurrentPosition, desiredPosition.Value) < 0.1f);
                            break;
                        }
                    }

                    pauseUpdate = false;
                    return;
                }
            }

            ProcessCollectableHeroInQueue();
            MoveSnake(headDestinationIndex, desiredPosition.Value, command.Direction);
            
            await UniTask.WaitUntil(() => Vector3.Distance(playerCharacters[0].CurrentPosition, desiredPosition.Value) < 0.1);
        }
        
        private async UniTask Battle(ICharacterEntity player, ICharacterEntity opponent)
        {
            player.TakeDamage(opponent.Attack - player.Defence);
            opponent.TakeDamage(player.Attack - opponent.Defence);
            await UniTask.WaitForSeconds(1f);
        }

        private void MoveSnake(int headDestinationIndex, Vector3 desiredPosition, Vector3 direction)
        {
            playerCharacters[0].MoveTo(desiredPosition);
            mapCharacters[headDestinationIndex] = (CharacterInteractionType.Player, playerCharacters[0]);
            var headIndex = mapController.GetMapIndexByPosition(playerCharacters[0].CurrentPosition);
            mapCharacters.Remove(headIndex);

            if (playerCharacters.Count > 1)
            {
                for (int i = 1; i < playerCharacters.Count; i++)
                {
                    var previousIndex = mapController.GetMapIndexByPosition(playerCharacters[i].CurrentPosition);
                    var slidingDestination = mapController.GetMapIndexByPosition(playerCharacters[i - 1].CurrentPosition);
                    playerCharacters[i].MoveTo(playerCharacters[i - 1].CurrentPosition);
                    mapCharacters[slidingDestination] = (CharacterInteractionType.Player, playerCharacters[i]);
                    mapCharacters.Remove(previousIndex);
                }
            }
            
            lastDirection = direction;
            tailDirection = playerCharacters.Count > 1 ? -(playerCharacters[^2].CurrentPosition - playerCharacters[^1].CurrentPosition).normalized : -direction;
        }

        private void ProcessCollectableHeroInQueue()
        {
            List<int> markForRemove = new List<int>();
            
            for (int i = 0; i < queuedHeroCollections.Count; i++)
            {
                var tailPosition = mapController.GetCellPositionByPosition(playerCharacters[^1].CurrentPosition + tailDirection);

                if (!tailPosition.HasValue)
                    continue;
                
                var teleportedCharacter = queuedHeroCollections[i]?.Invoke();
                playerCharacters.Add(teleportedCharacter);
                int tailIndex = mapController.GetMapIndexByPosition(tailPosition.Value);
                mapCharacters[tailIndex] = (CharacterInteractionType.Player, teleportedCharacter);
                markForRemove.Add(i);
                Debug.Log("Added new character to player from queue");
            }

            for (int i = 0; i < markForRemove.Count; i++)
                queuedHeroCollections.RemoveAt(markForRemove[i]);
            
            markForRemove.Clear();
        }

        private ICharacterEntity TeleportCharacter(ICharacterEntity character, Vector3? desiredPosition = null)
        {
            character.TeleportTo(desiredPosition ?? playerCharacters[^1].CurrentPosition + tailDirection);
            return character;
        }

        private ICharacterEntity SpawnCharacter(CharacterInteractionType characterInteractionType)
        {
            var spawnPosition = mapController.GetRandomPosition();
            var spawnIndexPosition = mapController.GetMapIndexByPosition(spawnPosition);

            while (mapCharacters.ContainsKey(spawnIndexPosition))
            {
                spawnPosition = mapController.GetRandomPosition();
                spawnIndexPosition = mapController.GetMapIndexByPosition(spawnPosition);
            }

            Assert.IsTrue(spawnIndexPosition != -1);

            var randomDataIndex = Random.Range(0, characterInteractionType == CharacterInteractionType.Enemy
                ? gameEntityData.EnemyCharacters.Length
                : gameEntityData.PlayableCharacters.Length);
            var selectedData = characterInteractionType == CharacterInteractionType.Enemy ? gameEntityData.EnemyCharacters[randomDataIndex] : gameEntityData.PlayableCharacters[randomDataIndex];
            var spawnedEntity = factory.Create(spawnPosition, selectedData);
            mapCharacters.Add(spawnIndexPosition, (characterInteractionType, spawnedEntity));
            return spawnedEntity;
        }

        private void CheckHovering(Ray ray)
        {
            float closest = 0f;
            int targetKey = -1;
            
            foreach (var pair in mapCharacters)
            {
                if (pauseUpdate)
                    break;
                
                var instance = pair.Value.Item2;
                
                var v1 = ray.direction;
                var v2 = pair.Value.Item2.CenterPosition - ray.origin;

                float lookPercentage = Vector3.Dot(v1.normalized, v2.normalized);
                instance.LookPercentage = lookPercentage;

                if (lookPercentage > LOOK_PERCENTAGE_THRESHOLD && lookPercentage > closest)
                {
                    lastLookIndex = targetKey = pair.Key;
                    closest = lookPercentage;
                }
            }

            if (targetKey == -1)
            {
                router.PublishAsync(new HoverCancelHoverCommand());
                return;
            }

            if (mapCharacters.TryGetValue(targetKey, out var character))
            {
                router.PublishAsync(new HeroHoverCommand
                {
                    Name = character.Item2.gameObject.name + "(" + character.Item1 + ")" ,
                    Attack = character.Item2.Attack.ToString(), 
                    Defense = character.Item2.Defence.ToString(), 
                    Health = character.Item2.Health + "/" + character.Item2.MaxHealth
                });
            }
        }
        
        public void FixedTick()
        {
            if (!hasGameStarted)
                return;
            
            if (pauseUpdate)
                return;
            
            CheckHovering(cameraController.CastRayToMousePosition());
            
            if (Mathf.Abs(mapCharacters.Count - playerCharacters.Count) >= MAP_ENTITY_UPPER_LIMIT)
                return;
            
            if (countSpawnTime >= currentSpawnTime)
            {
                currentSpawnTime = Random.Range(SPAWN_MIN_TIME, SPAWN_MAX_TIME);
                countSpawnTime = 0;
                CharacterInteractionType characterType;
                int roll = Random.Range(0, 11);
                
                if (roll < gameEntityData.EnemyChance)
                    characterType = CharacterInteractionType.Enemy;
                else if (roll < gameEntityData.CollectableChance + gameEntityData.EnemyChance)
                    characterType = CharacterInteractionType.Collectable;
                else
                    return;
                
                SpawnCharacter(characterType);
                return;
            }
            
            countSpawnTime += Time.fixedDeltaTime;
        }
    }
}