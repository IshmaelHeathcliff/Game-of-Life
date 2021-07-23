using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardCellPool : MonoBehaviour
{
    [SerializeField] int poolSizeStep = 10;

    public GameObject boardCell;
    readonly List<GameObject> _cellPool = new List<GameObject>();

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
        foreach (GameObject cell in _cellPool.Where(cell => !cell.activeSelf))
        {
            cell.SetActive(true);
            return cell;
        }

        GameObject obj = Instantiate(boardCell, transform);
        _cellPool.Add(obj);
        IncreasePoolSize();
        return obj;
    }
}
