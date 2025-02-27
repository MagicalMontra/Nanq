using UnityEngine;
using UnityEngine.Serialization;

namespace SETHD.FantasySnake.Map
{
    [CreateAssetMenu(fileName = "SimpleMapData", menuName = "Map/SimpleMapData", order = 1)]
    public class SimpleMapData : ScriptableObject, IMapData
    {
        public float BlockUnit => blockUnit;
        public byte SizeX => sizeX;
        public byte SizeY => sizeY;
        
        public BlockData[] BlockDatas => blockDatas;
        
        [SerializeField]
        private float blockUnit = 4;

        [SerializeField]
        private byte sizeX = 16;

        [SerializeField]
        private byte sizeY = 16;

        [SerializeField]
        private BlockData[] blockDatas;
    }
}
