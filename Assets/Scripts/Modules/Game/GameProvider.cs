using System;
using Cinemachine;
using SETHD.FantasySnake.Camera;
using VContainer;
using UnityEngine;
using VitalRouter;
using SETHD.Utilis;
using VContainer.Unity;
using SETHD.FantasySnake.Map;
using VitalRouter.VContainer;
using SETHD.FantasySnake.Input;
using SETHD.FantasySnake.Command;
using SETHD.FantasySnake.Character;
using SETHD.FantasySnake.UI;
using UnityEngine.Serialization;

namespace SETHD.FantasySnake.Game
{
    [Serializable]
    public class GameEntityData
    {
        public int EnemyChance => enemyChance;

        public int CollectableChance => collectableChance;
        public CharacterData[] EnemyCharacters => enemyCharacters;

        public CharacterData[] PlayableCharacters => playableCharacters;

        [SerializeField]
        private int enemyChance = 3;
        
        [SerializeField]
        private int collectableChance = 5;

        [SerializeField]
        private CharacterData[] enemyCharacters;
        
        [SerializeField]
        private CharacterData[] playableCharacters;
    }
    
    public class GameProvider : LifetimeScope
    {
        [SerializeField]
        private UIAsset uiAsset;
        
        [SerializeField]
        private GameEntityData characterData;
        
        [SerializeField]
        private GameObject mapParent;
        
        [SerializeField]
        private UnityEngine.Camera sceneCamera;
        
        [SerializeField]
        private InterfaceReference<IMapData> mapData;
        
        [SerializeField]
        private InterfaceReference<ICinemachineCamera> mainCamera;

        private Router router;
        
        protected override void Configure(IContainerBuilder builder)
        {
            router = new Router();
            builder.RegisterInstance(router);
            builder.RegisterVitalRouter(routing =>
            {
                routing.Map<UIController>();
                routing.MapEntryPoint<GameEntityController>();
            });
            builder.Register<CameraController>(Lifetime.Scoped);
            builder.Register<SimpleCharacterFactory>(Lifetime.Scoped);
            builder.Register(_ => uiAsset, Lifetime.Scoped);
            builder.Register(_ => sceneCamera, Lifetime.Scoped);
            builder.Register(_ => characterData, Lifetime.Scoped);
            builder.Register(_ => mainCamera.Value, Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
        }

        private void Start()
        {
            // For some reason you can't resolve something from child container on building phase
            // Had to move all of these below to Start()
            // Zenject probably better at this particular case
            
            var mapScope = CreateChild(b =>
            {
                b.RegisterVitalRouter(routing =>
                {
                    routing.Map<SimpleMapController>();
                });
                
                b.Register<RuntimeBlockFactory>(Lifetime.Scoped);
                b.Register(_ => mapParent, Lifetime.Scoped);
                b.Register(_ => mapData.Value, Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
            }, "MapScope");
            
            
            var playerMovementInputScope = CreateChild(b =>
            {
                b.RegisterVitalRouter(routing =>
                {
                    // routing.Filters.Add<CommandLogger>();
                    routing.Filters.Add<CommandExceptionHandling>();
                    routing.Filters.Add<PlayerMovementInputValidator>();
                });
                b.RegisterEntryPoint<PlayerMovementInputController>(Lifetime.Scoped);
                b.Register<GameplayInput>(Lifetime.Scoped);
            }, "PlayerMovementInputScope");

            router.PublishAsync(new MapCreateCommand());
        }
    }
}