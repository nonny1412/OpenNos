﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.PathFinder
{
    public class BestFirstSearch
    {
        #region Methods
        public static Node[,] LoadGrid(GridPos[,] Grid)
        {
            Node[,] grid = new Node[Grid.GetLength(0), Grid.GetLength(1)];
            for (short y = 0; y < grid.GetLength(1); y++)
            {
                for (short x = 0; x < grid.GetLength(0); x++)
                {
                    grid[x, y] = new Node()
                    {
                        Value = Grid[x, y].Value,
                        X = x,
                        Y = y
                    };
                }
            }
            return grid;
        }

        public static List<Node> FindPath(GridPos start, GridPos end, GridPos[,] Grid)
        {
            Node node = new Node();
            Node[,] grid = LoadGrid(Grid);
            Node Start = grid[start.X, start.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(Start);
            Start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                node = path.Pop();
                Grid[node.X, node.Y].Closed = true;

                //if reached the end position, construct the path and return it
                if (node.X == end.X && node.Y == end.Y)
                {
                    return Backtrace(node);
                }

                // get neigbours of the current node
                List<Node> neighbors = GetNeighbors(grid, node);

                for (int i = 0, l = neighbors.Count(); i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (!neighbor.Opened)
                    {
                        if (neighbor.F == 0)
                        {
                            neighbor.F = Heuristic.Octile(Math.Abs(neighbor.X - end.X), Math.Abs(neighbor.Y - end.Y));
                        }

                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }
                        else
                        {
                            neighbor.Parent = node;
                        }
                    }
                }
            }
            return new List<Node>();
        }

        public static void LoadBrushFire(GridPos user, ref Node[,] mapGrid, short MaxDistance = 22)
        {
            Node[,] grid = LoadGrid(mapGrid);

            Node node = new Node();
            Node Start = grid[user.X, user.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(Start);
            Start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                node = path.Pop();
                grid[node.X, node.Y].Closed = true;

                // get neigbours of the current node
                List<Node> neighbors = GetNeighbors(grid, node);

                for (int i = 0, l = neighbors.Count(); i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (!neighbor.Opened)
                    {
                        if (neighbor.F == 0)
                        {
                            double distance = Heuristic.Octile(Math.Abs(neighbor.X - node.X), Math.Abs(neighbor.Y - node.Y)) + node.F;
                            if (distance > MaxDistance)
                            {
                                neighbor.Value = 1;
                                continue;
                            }
                            else
                            {
                                neighbor.F = distance;
                            }
                            mapGrid[neighbor.X, neighbor.Y].F = neighbor.F;
                        }

                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }
                        else
                        {
                            neighbor.Parent = node;
                        }
                    }
                }
            }
        }

        public static List<Node> GetNeighbors(Node[,] Grid, Node node)
        {
            short x = node.X,
                y = node.Y;
            List<Node> neighbors = new List<Node>();
            bool s0 = false, d0 = false,
             s1 = false, d1 = false,
             s2 = false, d2 = false,
             s3 = false, d3 = false;
            int IndexX;
            int IndexY;


            // ↑
            IndexX = x;
            IndexY = y - 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && Grid[IndexX, IndexY].IsWalkable())
            {
                neighbors.Add(Grid[IndexX, IndexY]);
                s0 = true;
            }

            // →
            IndexX = x + 1;
            IndexY = y;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && Grid[IndexX, IndexY].IsWalkable())
            {
                neighbors.Add(Grid[IndexX, IndexY]);
                s1 = true;
            }


            // ↓
            IndexX = x;
            IndexY = y + 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && Grid[IndexX, IndexY].IsWalkable())
            {
                neighbors.Add(Grid[IndexX, IndexY]);
                s2 = true;
            }

            // ←
            IndexX = x - 1;
            IndexY = y;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && Grid[IndexX, IndexY].IsWalkable())
            {
                neighbors.Add(Grid[IndexX, IndexY]);
                s3 = true;
            }

            d0 = s3 || s0;
            d1 = s0 || s1;
            d2 = s1 || s2;
            d3 = s2 || s3;

            // ↖
            IndexX = x - 1;
            IndexY = y - 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d0 && Grid[IndexX, IndexY].IsWalkable() == true)
            {
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            // ↗
            IndexX = x + 1;
            IndexY = y - 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d1 && Grid[IndexX, IndexY].IsWalkable() == true)
            {
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            // ↘
            IndexX = x + 1;
            IndexY = y + 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d2 && Grid[IndexX, IndexY].IsWalkable() == true)
            {
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            // ↙
            IndexX = x - 1;
            IndexY = y + 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d3 && Grid[IndexX, IndexY].IsWalkable() == true)
            {
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            return neighbors;
        }

        public static List<Node> Backtrace(Node end)
        {
            List<Node> path = new List<Node>();
            while (end.Parent != null)
            {
                end = end.Parent;
                path.Add(end);
            }
            path.Reverse();
            return path;
        }

        public static List<Node> TracePath(Node node, Node[,] MapGrid)
        {
            Node currentnode = MapGrid[node.X,node.Y];
            List<Node> list = new List<Node>();
            while (currentnode.F != 1 && currentnode.F != 0)
            {
                Node newnode = null;
                newnode = BestFirstSearch.GetNeighbors(MapGrid, currentnode)?.OrderBy(s => s.F).FirstOrDefault();
                if (newnode != null)
                {
                    list.Add(newnode);
                    currentnode = newnode;
                }
            }
            return list;
        }
        #endregion
    }
}