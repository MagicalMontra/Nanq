using VContainer;
using UnityEngine;
using SETHD.Utilis;
using VContainer.Unity;
using System.Collections.Generic;

namespace SETHD.FantasySnake.Map
{
    public class SimpleMapProviderScope : LifetimeScope
    {
        [SerializeField]
        private GameObject mapParent;
        
        [SerializeField]
        private InterfaceReference<IMapData> mapData;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SimpleMapGenerator>(Lifetime.Scoped);
            builder.Register<RuntimeBlockFactory>(Lifetime.Scoped);
            builder.Register(_ => mapParent, Lifetime.Scoped);
            builder.Register(_ => mapData.Value, Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
        }
    }

    public class SimpleMapGenerator : IStartable
    {
        private readonly IMapData mapData;
        private readonly RuntimeBlockFactory factory;
        private readonly Dictionary<int, RuntimeBlock> blocks;

        [Inject]
        public SimpleMapGenerator(IMapData mapData, RuntimeBlockFactory factory)
        {
            this.mapData = mapData;
            this.factory = factory;
            blocks = new Dictionary<int, RuntimeBlock>();
        }
        
        public void Start()
        {
            float xStartPosition = -(mapData.SizeX * 0.5f) * mapData.BlockUnit;
            float zStartPosition = -(mapData.SizeY * 0.5f) * mapData.BlockUnit;
            
            for (byte i = 0; i < mapData.SizeX; i++)
            {
                for (byte j = 0; j < mapData.SizeY; j++)
                {
                    float x = i * mapData.BlockUnit + xStartPosition;
                    float z = j * mapData.BlockUnit + zStartPosition;
                    var prefab = mapData.BlockDatas[Random.Range(0, mapData.BlockDatas.Length)];
                    var instance = factory.Create(new Vector3(x, 0, z), prefab);
                    blocks.Add(i * mapData.SizeX + j, instance);
                }
            }
        }

        public Vector3 GetRandomPosition()
        {
            return blocks[Random.Range(0, mapData.SizeX * mapData.SizeY)].transform.position;
        }
    }
}