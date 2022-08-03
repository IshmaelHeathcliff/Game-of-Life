using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCellPool : MonoBehaviour
{
    [SerializeField] int poolSizeStep = 20;

    public GameObject boardCell;
    readonly List<GameObject> _cellPool = new List<GameObject>();

    int _currentCell = 0;

    void Awake()
    {
        IncreasePoolSize();
    }

    void IncreasePoolSize()
    {
        for (int i = 0; i < poolSizeStep; i++)
        {
            GameObject obj = Instantiate(boardCell, transform);
            obj.SetActive(false);
            _cellPool.Add(obj);
        }
    }
    
    public GameObject GetCell()
    {
        int i = _currentCell + 1 >= _cellPool.Count ? 0: _currentCell + 1; 
        while(i != _currentCell)
        {
            if (_cellPool[i].activeSelf)
            {
                i++;
                if (i >= _cellPool.Count) i = 0;
                continue;
            }
            _cellPool[i].SetActive(true);
            _currentCell = i;
            // Debug.Log($"currentCell:{_currentCell}");
            return _cellPool[i];
        }

        GameObject obj = Instantiate(boardCell, transform);
        _cellPool.Add(obj);
        _currentCell = _cellPool.Count - 1;
        IncreasePoolSize();
        return obj;
    }

    public void ClearCell()
    {
        foreach (GameObject cell in _cellPool)
        {
            cell.SetActive(false);
        }
    }
}
