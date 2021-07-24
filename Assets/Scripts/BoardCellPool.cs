using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCellPool : MonoBehaviour
{
    [SerializeField] int poolSizeStep = 10;

    public GameObject boardCell;
    readonly List<GameObject> _cellPool = new List<GameObject>();

    int _currentCell = 0;

    void Awake()
    {
        IncreasePoolSize();
    }

    void IncreasePoolSize()
    {
        for (var i = 0; i < poolSizeStep; i++)
        {
            GameObject obj = Instantiate(boardCell, transform);
            obj.SetActive(false);
            _cellPool.Add(obj);
        }
    }
    
    public GameObject GetCell()
    {
        for(int i=_currentCell+1; i != _currentCell; i++)
        {
            if (i >= _cellPool.Count-1) i = 0;
            if (_cellPool[i].activeSelf) continue;
            _cellPool[i].SetActive(true);
            _currentCell = i;
            return _cellPool[i];
        }

        GameObject obj = Instantiate(boardCell, transform);
        _cellPool.Add(obj);
        _currentCell = _cellPool.Count - 1;
        IncreasePoolSize();
        return obj;
    }
}
