using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECS
{
    public class CellColorAuthoring : MonoBehaviour
    {
        public Color color;

        public class Baker : Baker<CellColorAuthoring>
        {
            public override void Bake(CellColorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new CellColor
                {
                    Color = new float4(authoring.color.r, authoring.color.g, authoring.color.b, authoring.color.a)
                });
            }
        }
    }
        
    [MaterialProperty("_Color")]
    public struct CellColor: IComponentData
    {
        public float4 Color;
    }
}