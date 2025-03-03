using VitalRouter;
using UnityEngine;
using VContainer;
using System.Threading.Tasks;
using System.Collections.Generic;
using SETHD.FantasySnake.Command;

namespace SETHD.FantasySnake.Map
{
    [Routes]
    public partial class SimpleMapController
    {
        private readonly ICommandPublisher router;
        private readonly IMapData mapData;
        private readonly RuntimeBlockFactory factory;
        private readonly Dictionary<int, RuntimeBlock> blocks;
        
        private Vector3 mapOrigin;

        [Inject]
        public SimpleMapController(IMapData mapData, Router router, RuntimeBlockFactory factory)
        {
            this.router = router;
            this.mapData = mapData;
            this.factory = factory;
            blocks = new Dictionary<int, RuntimeBlock>();
        }

        public Vector3? GetCellPositionByPosition(Vector3 position)
        {
            Vector3 localPosition = position - mapOrigin;
            
            int cellX = Mathf.FloorToInt(localPosition.x / mapData.BlockUnit);
            int cellY = Mathf.FloorToInt(localPosition.z / mapData.BlockUnit);
            
            if (cellX < 0 || cellY < 0 || cellX >= mapData.SizeX || cellY > mapData.BlockUnit * cellY)
                return null;

            if (cellY * mapData.SizeX + cellX > blocks.Count)
                return null;
            
            if (cellY * mapData.SizeX + cellX < 0)
                return null;
            
            return blocks[cellY * mapData.SizeX + cellX].transform.position;
        }

        public Vector3 GetRandomPosition()
        {
            return blocks[Random.Range(0, mapData.SizeX * mapData.SizeY)].transform.position;
        }

        public Vector3? GetPositionByMapIndex(int mapIndex)
        {
            if (mapIndex < 0 || mapIndex >= blocks.Count)
                return null;
            
            return blocks[mapIndex].transform.position;
        }

        public int GetMapIndexByPosition(Vector3 position)
        {
            Vector3 localPosition = position - mapOrigin;
            
            int cellX = Mathf.FloorToInt(localPosition.x / mapData.BlockUnit);
            int cellY = Mathf.FloorToInt(localPosition.z / mapData.BlockUnit);
            
            if (cellX < 0 || cellY < 0 || cellX >= mapData.SizeX || cellY > mapData.BlockUnit * cellY)
                return -1;

            if (cellY * mapData.SizeX + cellX > blocks.Count)
                return -1;
            
            if (cellY * mapData.SizeX + cellX < 0)
                return -1;
            
            return cellY * mapData.SizeX + cellX;
        }
        
        [Route]
        private async ValueTask Create(MapCreateCommand command)
        {
            float xStartPosition = -(mapData.SizeX * 0.5f) * mapData.BlockUnit;
            float zStartPosition = -(mapData.SizeY * 0.5f) * mapData.BlockUnit;
            mapOrigin = new Vector3(xStartPosition, 0, zStartPosition);

            for (byte i = 0; i < mapData.SizeX; i++)
            {
                for (byte j = 0; j < mapData.SizeY; j++)
                {
                    float x = i * mapData.BlockUnit + xStartPosition;
                    float z = j * mapData.BlockUnit + zStartPosition;
                    var prefab = mapData.BlockDatas[Random.Range(0, mapData.BlockDatas.Length)];
                    var instance = factory.Create(new Vector3(z, 0, x), prefab);
                    instance.name = blocks.Count.ToString();
                    blocks.Add(i * mapData.SizeX + j, instance);
                }
            }
            
            await router.PublishAsync(new GameReadyCommand{ MapController = this });
        }
    }
}