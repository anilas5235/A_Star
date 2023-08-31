using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.General;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts
{
    public class AStar : Singleton<AStar>
    {
        public Vector2Int size = new Vector2Int(10,10);
        public float blockProbability = .2f;
        public int stepsPerSecond = 10;
        public bool done = true;

        private Node[][] nodeGrid;
        private Node startNode , endNode;
        private Node[] currentPath;
        private Coroutine solveRoutine;

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                if(solveRoutine!=null) StopCoroutine(solveRoutine);
                AStarAlgorithm(Vector2Int.zero,new Vector2Int(size.x-1,size.y-1));
            }
        }

        private void AStarAlgorithm(Vector2Int startPos, Vector2Int endPos)
        {
            if(!IsPositionInGrid(startPos)||!IsPositionInGrid(endPos))return;
            done = false;
            currentPath = Array.Empty<Node>();
            nodeGrid = new Node[size.x][];
            for (int x = 0; x < size.x; x++)
            {
                nodeGrid[x]= new Node[size.y];
                for (int y = 0; y < size.y; y++)
                {
                    nodeGrid[x][y] = new Node(new Vector2Int(x, y));
                }
            }

            startNode = nodeGrid[startPos.x][startPos.y];
            endNode = nodeGrid[endPos.x][endPos.y];
            startNode.Block = false;
            endNode.Block = false;
            startNode.SetNeighbors();
            endNode.SetNeighbors();

            TileMapHandler.Instance.size = size;
            TileMapHandler.Instance.ResetField();
                        
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Node node =nodeGrid[x][y];
                    node.HValue = Vector2Int.Distance(node.Position, endNode.Position);
                    if(node.Block) TileMapHandler.Instance.SetTileColor((Vector3Int)node.Position, TileMapHandler.Tiles.Black);
                }
            }

            if (!startNode.Neighbors.Any() || !endNode.Neighbors.Any())
            {
                Debug.Log("unsolvable");
                done = true;
                return;
            }

            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();
            openList.Add(startNode);

            solveRoutine = StartCoroutine(Solve());

            IEnumerator Solve()
            {
                bool solvable = false;
                while (openList.Any())
                {
                    int lowestIndex = 0;

                    for (int i = 0; i < openList.Count; i++)
                    {
                        if (openList[i].FValue < openList[lowestIndex].FValue) lowestIndex = i;
                    }

                    Node current = openList[lowestIndex];
                    openList.Remove(current);
                    closedList.Add(current);

                    UpdatePath();

                    if (current == endNode)
                    {
                        //done
                        solvable = true;
                        break;
                    }

                    current.SetNeighbors();
                    TileMapHandler.Instance.SetTileColor((Vector3Int)current.Position, TileMapHandler.Tiles.Red);


                    foreach (Node neighbor in current.Neighbors)
                    {
                        if (closedList.Contains(neighbor) ||neighbor.Block ) continue;
                        float potNewGVal = current.GValue + ( Vector2Int.Distance(current.Position, neighbor.Position) > 1f ? 1.1f:1f);
                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                            TileMapHandler.Instance.SetTileColor((Vector3Int)neighbor.Position,TileMapHandler.Tiles.Green);
                            neighbor.GValue = potNewGVal;
                            neighbor.Previous = current;
                        }
                        else if (neighbor.GValue > potNewGVal)
                        {
                            neighbor.GValue = potNewGVal;
                            neighbor.Previous = current;
                        }
                    }

                    yield return new WaitForSeconds(1f/stepsPerSecond);

                    void UpdatePath()
                    {
                        if (currentPath != null && currentPath.Any())
                        {
                            foreach (Node node in currentPath)
                            {
                                TileMapHandler.Instance.SetTileColor((Vector3Int)node.Position,
                                    TileMapHandler.Tiles.Red);
                            }
                        }

                        Node tempPathPart = current;
                        List<Node> tempPath = new List<Node>();
                        while (tempPathPart.Previous != null)
                        {
                            tempPath.Add(tempPathPart);
                            tempPathPart = tempPathPart.Previous;
                        }

                        tempPath.Add(startNode);

                        currentPath = tempPath.ToArray();

                        foreach (Node node in currentPath)
                        {
                            TileMapHandler.Instance.SetTileColor((Vector3Int)node.Position, TileMapHandler.Tiles.Blue);
                        }
                    }
                }

                Debug.Log(solvable ? "Path found, finished" : "unsolvable");
                done = true;
                solveRoutine = null;
            }
        }

        public bool IsPositionInGrid(Vector2Int pos)
        {
            return pos.x < size.x && pos.x >= 0 && pos.y < size.y && pos.y >= 0;
        }

        public Node GetNodeInGrid(Vector2Int pos)
        {
            return IsPositionInGrid(pos) ? nodeGrid[pos.x][pos.y] : null;
        }
    }

    public class Node
    {
        private static Vector2Int[] neighborPositionOffsets = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up,
            Vector2Int.down,Vector2Int.one, Vector2Int.one*-1, new Vector2Int(-1,1),new Vector2Int(1,-1) };
        public Node(Vector2Int position)
        {
            Position = position;
            Block = Random.Range(0f, 1f) < AStar.Instance.blockProbability;
        }
        public bool Block = false;
        public float GValue, HValue;
        public float FValue => GValue+HValue;
        public Node Previous;
        public Vector2Int Position;

        public Node[] Neighbors;
        public void SetNeighbors()
        {
            List<Node> neighbors = new List<Node>();
            foreach (Vector2Int neighborPoseOffset in neighborPositionOffsets)
            {
                Vector2Int neighborPosition = neighborPoseOffset + Position;
                if (!AStar.Instance.IsPositionInGrid(neighborPosition)) continue;
                
                Node neighbor = AStar.Instance.GetNodeInGrid(neighborPosition);
                if(!neighbor.Block) neighbors.Add(neighbor);
            }
            Neighbors = neighbors.ToArray();
        }
    }
}
