using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    public Sprite[] pieces;
    public RectTransform[] gameBoard;
    [Header("Prefabs")] public GameObject nodePiece;
    private int height = 14;
    private int width = 9;
    private int seqNum = 0;
    private int[] fills; 
    private Node[,] board;
    private System.Random random;
    [SerializeField] private GameObject fiveCon;
    private List<NodePiece> update;
    private List<FlippedPieces> flipped;
    private List<NodePiece> dead;
    public GameObject connectPiece;
    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.UpdatePiece())
            {
                
                finishedUpdating.Add(piece);
            }
            Debug.Log(update.Count);
        }
        
        
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            FlippedPieces flip = getFlipped(piece);
            NodePiece flippedPiece = null;

            int x = (int) piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null);
            if (wasFlipped)
            {          
                flippedPiece = flip.getOtherPiece(piece); 
                AddPoints(ref connected,isConnected(flippedPiece.index,true));
            }

            if (connected.Count == 0)
            {
                if (wasFlipped)
                {
                    FlipPieces(piece.index,flippedPiece.index,false);
                }
            }
            else
            {
                foreach (Point pnt in connected)
                {
                    Node node = getNodeAtPoint(pnt);
                    NodePiece nodePiece = node.getPiece();
                    if (nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        dead.Add(nodePiece);
                    }
                    node.SetPiece(null);
                }

                ApplyGravityToBoard();
            }

            flipped.Remove(flip);
            update.Remove(piece);
        }
         
    }
    
    
    // ReSharper disable Unity.PerformanceAnalysis
    void ApplyGravityToBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = (height-1); y >=0 ; y --)
            {
                Point p = new Point(x, y);
                Node node = getNodeAtPoint(p);
                int val = getValueAtPoint(p);
                if (val != 0)
                {
                    continue;
                }

                for (int ny = (y-1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = getValueAtPoint(next);
                    if (nextVal == 0)
                    {
                        continue;
                    }

                    if (nextVal != -1)
                    {
                        Node got = getNodeAtPoint(next);
                        NodePiece piece = got.getPiece();
                        
                        node.SetPiece(piece);
                        update.Add(piece);
                        
                        got.SetPiece(null);
                    }
                    else
                    {
                        int newVal = fillPiece();
                        NodePiece piece;
                        Point fallPnt = new Point(x, (-1 - fills[x]));
                        if (dead.Count >0)
                        {
                            NodePiece revived = dead[0];
                            revived.gameObject.SetActive(true);
                            revived.rect.anchoredPosition = getPositionFromPoint(fallPnt);
                            piece = revived;
                            
                            dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard[0]);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            RectTransform rect = obj.GetComponent<RectTransform>();
                            rect.anchoredPosition = getPositionFromPoint(fallPnt);
                            piece = n;
                        }
                        piece.Initialize(newVal,p,pieces[newVal-1]);

                        Node hole = getNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[x]++;
                    }

                    
                    break;
                }
            }
        }
    }


    FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null;

        for (int i = 0; i < flipped.Count; i++)
        {
            
            if (flipped[i].getOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }

        return flip;
    }
    void StartGame()
    {
        fills = new int[width];
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();
        dead = new List<NodePiece>();
        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    void InitializeBoard()
    {
        board = new Node[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = new Node((boardLayout.rows[y].row[x])? - 1:fillPiece(), new Point(x, y));
            }
        }
    }

    void VerifyBoard()
    {
        List<int> remove = null;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = getValueAtPoint(p);
                if(val <= 0 ) continue;
                remove = new List<int>();
                while (isConnected(p,true).Count>0)
                {
                    val = getValueAtPoint(p);
                    if (!remove.Contains(val))
                    {
                        remove.Add(val);
                    }
                    setValueAtPoint(p,newValue(ref remove));
                }
            }
        }
    }

    void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = getNodeAtPoint(new Point(x, y));
                int val = node.value;
                if (val<=0)
                {
                    continue;
                }

                GameObject p = Instantiate(nodePiece, gameBoard[0]);
                
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y)); 
                piece.Initialize(val,new Point(x,y), pieces[val-1]);
                node.SetPiece(piece);
            }
        }
    }

    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition();
        update.Add(piece);
    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        if (getValueAtPoint(one) < 0)
        {
            return;
        }
        Node nodeOne = getNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two)>0)
        {
            Node nodeTwo = getNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);
            if (main)
            {
                flipped.Add(new FlippedPieces(pieceOne,pieceTwo));

            }
            update.Add(pieceOne);
            update.Add(pieceTwo); 
        }

        else
        {
            ResetPiece(pieceOne);
        }
    }

    List<Point> isConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>();
        
        int val = getValueAtPoint(p);
        Point[] directions =
        {
            Point.up,
            Point.right,
            Point.down,
            Point.left
        };
        
        foreach (Point dir in directions) // Checking if there is 2 or more same shapes in the directions
        {
            List<Point> line = new List<Point>();
            int same = 0;
            
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.add(p, Point.mult(dir, i));
                if (getValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;

                }
            }
            if (same>2)
            {
                Debug.Log("DENEMEEEEE");
            }
            if (same > 1)
            {
                AddPoints(ref connected, line);
                //GameObject connect = Instantiate(connectPiece);
                if (seqNum == 0)
                {
                    seqNum++;
                    //Debug.Log(seqNum*20);
                }
                
            }
            

            
        }

//bir şeklin önce üstüne sonra altına bakıyor. Daha sonra aynı şeklin önce sağına sonra soluna bakıyor. 4lü bakmıyor.
        for (int i = 0; i < 2; i++)
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point[] check = {Point.add(p, directions[i]), Point.add(p, directions[i + 2])};
            foreach (Point next in check)
            {
                if (getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }

                
            }

            if (same >1)
            {
                AddPoints(ref connected,line);
                
            }
        }

       
        for (int i = 0; i < 4; i++)
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if (next >= 4)
            {
                next -= 4;
                
            }

            Point[] check =
            {
                Point.add(p, directions[i]), Point.add(p, directions[next]),
                Point.add(p, Point.add(directions[i], directions[next]))
            };
            foreach (Point pnt in check)
            {
                if (getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }  

                
            }

       
            

            if (same > 2)
            {
                AddPoints(ref connected, square);
                
            } 
            
        }

        if (main)
        {
            for (int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, isConnected(connected[i],false));
                
            }
            
        }
        // if (connected.Count > 0)
        // {
        //     connected.Add(p);
        //         
        // }

        return connected;
    }

    void AddPoints(ref List<Point> points, List<Point> add)
    {
        if (points.Count>4)
        {
            Debug.Log("oldu");
            for (int i = 0; i < points.Count; i++)
            {
                Debug.Log(points[i]);
            }

            Instantiate(fiveCon, new Vector3(points[0].x,points[0].y,0),Quaternion.identity);
        }
        foreach (Point p in add)
        {
            bool doAdd = true;
            
            
            
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) 
            {
                points.Add(p);
            }

            
        }
    }
    int newValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < pieces.Length; i++)
        {
            available.Add(i+1);
                
        }

        foreach (int i in remove)
        {
            available.Remove(i);
        }

        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
    }
    int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return val;
    }

    int getValueAtPoint(Point p)
    {
        if (p.x<0 || p.x >=width || p.y<0 || p.y >= height) return -1;
        return board[p.x, p.y].value;
    }

    void setValueAtPoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }

    Node getNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
    }
    string getRandomSeed()
    {
        string seed = "";

        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxy1234567890!@?%&()*";
        for (int i = 0; i < 20; i++)
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        

        return seed;
    }
}

[System.Serializable]
public class Node
{
    public int value;
    public Point index;
    NodePiece piece;
    public Node(int v, Point i )
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null)
        {
            return;
        }
        piece.SetIndex(index);
    }

    public NodePiece getPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece getOtherPiece(NodePiece p)
    {
        if (p == one)
        {
            return two;
        }
        else if (p == two)
        {
            return one;
        }
        else
        {
            return null;
        }
    }
}