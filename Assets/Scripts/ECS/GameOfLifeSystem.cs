using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS
{
    public partial struct GameOfLifeSystem : ISystem
    {
        NativeArray<int2> _neighborOffsets;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CellData>();
            
            _neighborOffsets = new NativeArray<int2>(8, Allocator.Persistent)
            {
                [0] = new int2(-1, -1),
                [1] = new int2(0, -1),
                [2] = new int2(1, -1),
                [3] = new int2(-1, 0),
                [4] = new int2(1, 0),
                [5] = new int2(-1, 1),
                [6] = new int2(0, 1),
                [7] = new int2(1, 1)
            };
        }

        public void OnDestroy(ref SystemState state)
        {
            _neighborOffsets.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            // 创建一个本地副本来存储当前活细胞的位置
            var liveCells = new NativeHashMap<int2, bool>();
            
            
            var changeCellColorJob = new ChangeCellStatueJob()
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };
            
            
            state.Dependency = changeCellColorJob.ScheduleParallel(state.Dependency);
            
            liveCells.Dispose();
        }
    }

    public partial struct ChangeCellStatueJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(ref CellData cellData)
        {
            if (cellData.NextUpdateTime > 0)
            {
                cellData.NextUpdateTime -= deltaTime;
            }
            else
            {
                cellData.IsAlive = !cellData.IsAlive;
                cellData.NextUpdateTime = cellData.UpdateInterval;
            }
        }
    }
    
    public partial struct GetLiveCellJob : IJobEntity
    {
        public void Execute(in CellData cellData, ref NativeHashMap<int2, bool> liveCells)
        {
            if (cellData.IsAlive)
            {
                liveCells.TryAdd(new int2(cellData.X, cellData.Y), true);
            }
        }
    }

    public partial struct CountLiveNeighborJob : IJobEntity
    {
        public NativeArray<int2> NeighborOffsets;
        public NativeHashMap<int2, bool> LiveCells;
        public void Execute(ref CellData cellData)
        {
            var liveNeighbors = 0;
            var cellPos = new int2(cellData.X, cellData.Y);

            foreach (var offset in NeighborOffsets)
            {
                var neighborPos = cellPos + offset;
                if (LiveCells.ContainsKey(neighborPos))
                {
                    liveNeighbors++;
                }
            }
            

            var nextState = cellData.IsAlive;

            if (cellData.IsAlive && (liveNeighbors < 2 || liveNeighbors > 3))
            {
                nextState = false;
            }
            else if (!cellData.IsAlive && liveNeighbors == 3)
            {
                nextState = true;
            }
            
            cellData.NextState = nextState;
        }
    }

    public partial struct UpdateCellStateJob : IJobEntity
    {
        public void Execute(ref CellData cellData)
        {
            if (cellData.NextState)
            {
                cellData.IsAlive = true; 
            }
        }
    }
}
