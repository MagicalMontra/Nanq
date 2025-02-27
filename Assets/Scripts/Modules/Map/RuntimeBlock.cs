using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SETHD.FantasySnake.Map
{
    public class RuntimeBlock : MonoBehaviour
    {
        private string identifier;
        private Category category;

        public void Initialize(string identifier, Category category)
        {
            this.identifier = identifier;
            this.category = category;
        }
    }

    public class RuntimeBlockFactory
    {
        private readonly GameObject mapParent;
        private readonly IObjectResolver container;

        [Inject]
        public RuntimeBlockFactory(IObjectResolver container, GameObject mapParent)
        {
            this.container = container;
            this.mapParent = mapParent;
        }

        public RuntimeBlock Create(Vector3 position, BlockData blockData)
        {
            var instance = container.Instantiate(blockData.Prefab, position, Quaternion.identity, mapParent.transform);
            instance.Initialize(blockData.Identifier, blockData.Category);
            return instance;
        }
    }
}