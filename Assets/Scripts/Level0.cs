using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Level0 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    private int height = 7;
    private int width = 7;
    private Node[,] board;

    void Awake()
    {
        board = new Node[width, height];
        // for (int y = 0; y < height; y++)
        // {
        //     for (int x = 0; x < width; x++)
        //     {
        //         board[x, y] = new Node((boardLayout.rows[y].row[x])? - 1:fillPiece(), new Point(x, y));
        //     }
        // }
    }
    
    void Update()
    {
        
    }

    private void fillPiece()
    {
        
    }

}
