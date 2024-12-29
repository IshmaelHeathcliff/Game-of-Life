using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Random = UnityEngine.Random;

namespace ECS
{
    public partial class SpawnCellsSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<SpawnCellsConfig>();
        }
        protected override void OnUpdate()
        {
            this.Enabled = false;

            var spawnCellsConfig = SystemAPI.GetSingleton<SpawnCellsConfig>();

            for (int i = -spawnCellsConfig.GridSize.x; i < spawnCellsConfig.GridSize.x; i++)
            {
                for (int j = -spawnCellsConfig.GridSize.y; j < spawnCellsConfig.GridSize.y; j++)
                {
                    var entity = EntityManager.Instantiate(spawnCellsConfig.CellPrefabEntity);
                    SystemAPI.SetComponent(entity, new CellData
                    {
                        IsAlive = Random.Range(0, 1f) > 0.8,
                        X = i,
                        Y = j,
                        UpdateInterval = spawnCellsConfig.UpdateInterval,
                        NextUpdateTime = spawnCellsConfig.UpdateInterval
                    });
                    
                    SystemAPI.SetComponent(entity, new LocalTransform
                    {
                        Position = new float3(i, j, 0),
                        Scale = 1,
                        Rotation = quaternion.identity
                    });
                }
            }
        }
    }
}