using Unity.Entities;
using UnityEngine;


namespace ECS
{
    public class Cell : MonoBehaviour
    {
        public Entity Entity;
        SpriteRenderer _spriteRenderer;
        EntityManager _entityManager;

        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        void Update()
        {
            var cellData = _entityManager.GetComponentData<CellData>(Entity);
            _spriteRenderer.color = cellData.IsAlive ? Color.black : Color.white;
        }
    }
}