using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class RollingMatrix : MonoBehaviour
{
    public GameObject tile;
    public float updateInterval = 1f;

    int _index;
    float _nextUpdateTime;
    int _length = 500000000;

    int[] _posX;
    int[] _posY;

    bool _isTimeExported;
    
    int[] ArrayIndexToMatrixCoord(int index)
    {
        var k = Convert.ToInt32(Math.Round(Math.Sqrt(index / 4f)) - 1);
        if (index < 4 * k * (k + 1) || index > 4 * (k + 2) * (k + 1)) k++;
        if (index < 4 * k * (k + 1) || index > 4 * (k + 2) * (k + 1)) k -= 2;
        if (index < 4 * k * (k + 1) || index > 4 * (k + 2) * (k + 1))
        {
            Debug.Log("Wrong Result!!");
            return null;
        }

        int m = index - 4 * k * (k + 1);
        if (m >= 0 && m < 2 * (k + 1))
        {
            return new[] { k + 1, m - k - 1 };
        }

        if (2 * (k + 1) <= m && m < 4 * (k + 1))
        {
            return new[] { 3 * (k + 1) - m, k + 1 };
        }

        if (4 * (k + 1) <= m && m < 6 * (k + 1))
        {
            return new[] { -k - 1, 5 * (k + 1) - m };
        }

        return new[] { m - 7 * (k + 1), -k - 1 };

    }
    void Start()
    {
        _nextUpdateTime = updateInterval;
        _posX = new int[_length];
        _posY = new int[_length];
        
        //JobSystem
        // var xPosition = new NativeArray<int>(_length, Allocator.TempJob);
        // var yPosition = new NativeArray<int>(_length, Allocator.TempJob);
        // var myJob = new IndexJob
        // {
        //     xPos = xPosition,
        //     yPos = yPosition
        // };
        //
        // JobHandle handle = myJob.Schedule(_length, 32);
        // handle.Complete();
        //
        // xPosition.CopyTo(_posX);
        // yPosition.CopyTo(_posY);
        //
        // xPosition.Dispose();
        // yPosition.Dispose();

        //Normal
        for (int i = 0; i < _length; i++)
        {
            var pos = ArrayIndexToMatrixCoord(i);
            _posX[i] = pos[0];
            _posY[i] = pos[1];
        }
        
        Debug.Log(Time.realtimeSinceStartup);
        Debug.Log($"{_posX[_length-1]}, {_posY[_length-1]}");

    }

    struct IndexJob : IJobParallelFor
    {
        public NativeArray<int> xPos;
        public NativeArray<int> yPos;

        public void Execute(int index)
        {
            var k = Convert.ToInt32(Mathf.Round(Mathf.Sqrt(index / 4f)) - 1);
            if (index < 4 * k * (k + 1) || index > 4 * (k + 2) * (k + 1)) k++;
            if (index < 4 * k * (k + 1) || index > 4 * (k + 2) * (k + 1)) k -= 2;
            if (index < 4 * k * (k + 1) || index > 4 * (k + 2) * (k + 1))
            {
                Debug.Log("Wrong Result!!");
                return;
            }

            int m = index - 4 * k * (k + 1);
            if (m >= 0 && m < 2 * (k + 1))
            {
                xPos[index] = k + 1;
                yPos[index] = m - k - 1;
                return;
            }

            if (2 * (k + 1) <= m && m < 4 * (k + 1))
            {
                xPos[index] = 3 * (k + 1) - m;
                yPos[index] = k + 1;
                return;
            }

            if (4 * (k + 1) <= m && m < 6 * (k + 1))
            {
                xPos[index] = -k - 1;
                yPos[index] = 5 * (k + 1) - m;
                return;
            }
            
            xPos[index] = m - 7 * (k + 1);
            yPos[index] = -k - 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (_nextUpdateTime < 0)
        // {
        //     int[] pos = ArrayIndexToMatrixCoord(_index);
        //     Instantiate(tile, new Vector3(pos[0], pos[1], 0), Quaternion.identity);
        //     _index++;
        //     _nextUpdateTime = updateInterval;
        // }
        // else
        // {
        //     _nextUpdateTime -= Time.deltaTime;
        // }

        // for (int i = 0; i < _length; i++)
        // {
        //     int[] pos = ArrayIndexToMatrixCoord(i);
        //     Instantiate(tile, new Vector3(pos[0], pos[1], 0), Quaternion.identity);
        // }

        // for (int i = 0; i < _length; i++)
        // {
        //     Instantiate(tile, new Vector3(_posX[i], _posY[i], 0), Quaternion.identity);
        // }

        // if (!_isTimeExported)
        // {
        //     Debug.Log(Time.realtimeSinceStartup);
        //     _isTimeExported = true;
        // }
        
    }

    void OnDestroy()
    {
        GC.Collect();
    }
}
