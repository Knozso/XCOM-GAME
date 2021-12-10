using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Cell
    {
        public event Action<UnityEngine.Color> ColorChanged;
        public event Action<Unit, String> UnitAdded;

        public Vector3 WorldPosition { get; set; }
        private int gridX;
        private int gridY;

        public bool InRange { get; set; }

        public bool Walkable { get; set; }

        public bool Occupied { get; set; }

        public bool Traceable { get; set; }

        public bool HasElement { get; set; }

        public List<GameObject> MapElements;

        public Dictionary<Direction, Cell> Neighbours { get; set; }

        public Cell Parent;

        public Dictionary<Direction, bool> Cover;

        //public List<Unit> Units { get; set; }

        public int gCost;
        public int hCost;
        public int FCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public float Value;

        public Cell()
        {
            //Units = new List<Unit>();
            Neighbours = new Dictionary<Direction, Cell>();
            MapElements = new List<GameObject>();
            Walkable = true;
            Occupied = false;
            Traceable = false;
            HasElement = false;
            Value = 0;
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
            return WorldPosition.z;
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

        public bool HasCoverFrom(Direction dir)
        {
            return (Neighbours.ContainsKey(dir) && !Neighbours[dir].Walkable) || (Cover.ContainsKey(dir) && Cover[dir]);
        }

        public void AddUnit(Unit unit, String prefabString)
        {
            //Units.Add(unit);
            UnitAdded?.Invoke(unit, prefabString);
        }

        public void ChangeColor(UnityEngine.Color color)
        {
            ColorChanged?.Invoke(color);
        }
    }
}
