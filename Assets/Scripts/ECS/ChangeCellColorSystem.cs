using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECS
{
    public partial struct ChangeCellColorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CellData>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (cellData, color) in SystemAPI.Query<RefRO<CellData>, RefRW<CellColor>>())
            {
                color.ValueRW.Color = cellData.ValueRO.IsAlive ? 
                    new float4(1, 1, 1, 1) : 
                    new float4(0, 0, 0, 1);
            }
        }
    }
}