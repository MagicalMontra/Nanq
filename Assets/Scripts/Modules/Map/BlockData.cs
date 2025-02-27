using System;
using UnityEngine;

namespace SETHD.FantasySnake.Map
{
    [Serializable]
    public class BlockData
    {
        public string Identifier => identifier;

        public RuntimeBlock Prefab => prefab;

        public Category Category => category;

        [SerializeField]
        private string identifier;

        [SerializeField]
        private RuntimeBlock prefab;
        
        [SerializeField]
        private Category category;
    }
}