using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Cell
    {
        public event Action<System.Drawing.Color> ColorChanged;
        public event Action<Unit> UnitAdded;

        public Vector3 WorldPosition { get; set; }
        private int gridX;
        private int gridY;

        public bool Walkable { get; set; }

        public bool Traceable { get; set; }
        public Dictionary<Direction, Cell> Neighbours { get; set; }

        public Cell Parent;

        public Dictionary<Direction, bool> Cover;

        public List<Unit> Units { get; set; }

        public int gCost;
        public int hCost;
        public int FCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public Cell()
        {
            Units = new List<Unit>();
            Neighbours = new Dictionary<Direction, Cell>();
            Walkable = true;
            Traceable = false;
            Cover = new Dictionary<Direction, bool>();
            Cover.Add(Direction.North, false);
            Cover.Add(Direction.East, false);
            Cover.Add(Direction.South, false);
            Cover.Add(Direction.West, false);
        }

        public int GetGridX()
        {
            return gridX;
        }
        public int GetGridY()
        {
            return gridY;
        }

        public float GetWorldX()
        {
            return WorldPosition.x;
        }
        public float GetWorldY()
        {
            return WorldPosition.y;
        }

        public void SetNeigbour(Direction dir, Cell otherCell)
        {
            if (!Neighbours.ContainsKey(dir))
            {
                Neighbours.Add(dir, otherCell);
                int dirNum = (int)dir;
                dirNum += 4;
                dirNum = dirNum % 8;
                Direction oppositeDirection = (Direction)dirNum;
                otherCell.Neighbours.Add(oppositeDirection, this);
            }
        }

        public void RemoveNeighbour(Direction dir)
        {
            if (Neighbours.ContainsKey(dir))
            {
                Cell otherCell = Neighbours[dir];
                int dirNum = (int)dir;
                dirNum += 4;
                dirNum = dirNum % 8;
                Direction oppositeDirection = (Direction)dirNum;
                otherCell.Neighbours.Remove(oppositeDirection);

                Neighbours.Remove(dir);
            }
        }

        public Dictionary<Direction, Cell> GetNeighbours()
        {
            return Neighbours;
        }

        public void SetGridX(int x)
        {
            gridX = x;
        }

        public void SetGridY(int y)
        {
            gridY = y;
        }

        public void AddUnit(Unit unit)
        {
            Units.Add(unit);
            UnitAdded?.Invoke(unit);
        }

        public void ChangeColor(System.Drawing.Color color)
        {
            ColorChanged?.Invoke(color);
        }
    }
}
