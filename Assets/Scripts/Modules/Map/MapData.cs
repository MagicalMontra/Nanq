using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SETHD.FantasySnake.Map
{
    public class MapData : ScriptableObject
    {
        [SerializeField]
        private float floorUnit = 4;

        [SerializeField]
        private float sizeX = 16;

        [SerializeField]
        private float sizeY = 16;
    }
}
