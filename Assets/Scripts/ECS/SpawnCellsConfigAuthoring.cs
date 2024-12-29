using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS
{
    public class SpawnCellsConfigAuthoring : MonoBehaviour
    {
        public GameObject CellPrefab;
        public int2 GridSize;
        public float UpdateInterval;
        
        public class Baker : Baker<SpawnCellsConfigAuthoring>
        {
            public override void Bake(SpawnCellsConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnCellsConfig
                {
                    CellPrefabEntity = GetEntity(authoring.CellPrefab, TransformUsageFlags.Dynamic),
                    GridSize = authoring.GridSize,
                    UpdateInterval = authoring.UpdateInterval
                });
            }
        }
    }

    public partial struct SpawnCellsConfig : IComponentData
    {
        public Entity CellPrefabEntity;
        public int2 GridSize;
        public float UpdateInterval;

    }
}