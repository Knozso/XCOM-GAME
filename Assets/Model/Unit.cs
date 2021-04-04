using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Unit
    {
        public event Action<List<Cell>> UnitMoved;

        public int Id { get; set; }
        public string Name { get; set; }
        public int Actions { get; set; }
        public Cell CurrentCell { get; set; }
        public Player Owner { get; set; }

        public int WalkableDistance;

        public bool IsEnemyUnit;

        public Unit(bool isEnemyUnit)
        {
            Actions = 2;
            IsEnemyUnit = isEnemyUnit;
        }

        public void SelectUnit()
        {
            if (Owner.Equals(Stepper.Instance().GetCurrentPlayer()))
            {
                Stepper.Instance().SelectedUnit = this;
            }
        }

        public void MoveUnit(Cell destination)
        {
            if (CurrentCell != null && !CurrentCell.Equals(destination))
            {
                if (Pathfinding.GetPathDistance(CurrentCell, destination) <= WalkableDistance && Actions >= 1 && destination.Walkable)
                {
                    DoMove(destination);
                }
                else if (Pathfinding.GetPathDistance(CurrentCell, destination) <= WalkableDistance * 2 && Actions >= 2 && destination.Walkable)
                {
                    DoDoubleMove(destination);
                }
            }
            //Owner.ActionHappened();
        }

        private void DoMove(Cell destination)
        {
            Actions--;
            List<Cell> path = Pathfinding.FindPath(CurrentCell.WorldPosition, destination.WorldPosition);
            CurrentCell = destination;
            UnitMoved?.Invoke(path);
            Stepper.Instance().SelectedUnit = this;
            Debug.Log("DoMove");
        }

        private void DoDoubleMove(Cell destination)
        {
            Actions-=2;
            List<Cell> path = Pathfinding.FindPath(CurrentCell.WorldPosition, destination.WorldPosition);
            CurrentCell = destination;
            UnitMoved?.Invoke(path);
            Stepper.Instance().SelectedUnit = this;
            Debug.Log("DoDoubleMove");
        }

        public void PlaceUnit(Cell initCell)
        {
            initCell.AddUnit(this);
            CurrentCell = initCell;
        }
    }
}
