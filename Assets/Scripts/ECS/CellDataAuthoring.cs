using Unity.Entities;
using UnityEngine;

namespace ECS
{
    public class CellDataAuthoring : MonoBehaviour
    {
        public float UpdateInterval;
        
        public class Baker : Baker<CellDataAuthoring>
        {
            public override void Bake(CellDataAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CellData
                {
                    UpdateInterval = authoring.UpdateInterval                   
                });
            }
        }
    }
    public struct CellData : IComponentData
    {
        public bool IsAlive;
        public bool NextState;
        public float UpdateInterval;
        public float NextUpdateTime;
        public int X;
        public int Y;
    }
}

