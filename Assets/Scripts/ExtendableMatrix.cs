using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendableMatrix<T>
    where T: new()
{
    public readonly List<List<T>> Matrix;
    public int Rows;
    public int Columns;
    
    public ExtendableMatrix(int rows=11, int columns=11)
    {
        Matrix = new List<List<T>>();
        Rows = rows;
        Columns = columns;
        ExtendMatColumns(columns);
    }

    public void ExtendMatRows(int rowPair=1)
    {
        foreach (var list in Matrix)
        {
            for (var i = 0; i < rowPair; i++)
            {
                list.Add(new T());
                list.Insert(0, new T());
            }
        }
        Rows += 2*rowPair;

    }

    public void ExtendMatColumns(int columnPair=1)
    {
        for (var i = 0; i < columnPair; i++)
        {        
            var list1 = new List<T>();
            var list2 = new List<T>();
            for (var k = 0; k < Rows; k++)
            {
                list1.Add(new T());
                list2.Add(new T());
            }
            Matrix.Add(list1);
            Matrix.Insert(0, list2);
        }

    }

    public void ExtendTo(int x, int y)
    {
        x = Math.Abs(x) + 1;
        y = Math.Abs(y) + 1;
        if (Matrix.Count < 2 * x + 1)
        {
            ExtendMatColumns(x < Matrix.Count? Matrix.Count/2 : x);
        }

        if (Rows < 2 * y + 1)
        {
            ExtendMatRows(y < Rows? Rows/2 : y);
        }
    }
}
