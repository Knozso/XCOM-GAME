using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace Model
{
    public class Unit
    {
        public event Action<List<Cell>> UnitMoved;
        public event Action<Cell> UnitMove;
        public event Action<Unit> TargetChanged;
        public event Action<int, Unit, int, int> UnitShoots;
        public event Action UnitDodged;
        public event Action UnitHit;
        public event Action UnitDied;
        public event Action<Cell> UnitThrow;

        public int Id { get; set; }
        public string Name { get; set; }
        public int Actions { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public Cell CurrentCell { get; set; }
        public Player Owner { get; set; }

        public int WalkableDistance;

        public bool IsEnemyUnit;

        public string AbilityName { get; set; }

        public String PrefabString { get; set; }

        public Unit() { }

        public Unit(bool isEnemyUnit, bool multiplayer)
        {
            Actions = 2;
            Health = 10;
            MaxHealth = 10;
            IsEnemyUnit = isEnemyUnit;
            if(isEnemyUnit && multiplayer)
            {
                PrefabString = "Prefabs/EnemyUnit";
            }
            else if(!isEnemyUnit && multiplayer)
            {
                PrefabString = "Prefabs/PlayerUnit";
            }
            else if(isEnemyUnit && !multiplayer)
            {
                PrefabString = "Prefabs/EnemyUnitSinglePlayer";
            }
            else if(!isEnemyUnit && !multiplayer)
            {
                PrefabString = "Prefabs/PlayerUnitSinglePlayer";
            }
        }

        public virtual void SelectUnit()
        {
            if (Owner.Equals(Stepper.Instance().GetCurrentPlayer()))
            {
                Stepper.Instance().SelectedUnit = this;
            }
        }

        public bool InCoverFrom(Direction dir)
        {
            return CurrentCell.HasCoverFrom(dir);
        }

        public void MoveUnit(Cell destination)
        {
            if (CurrentCell != null && !CurrentCell.Equals(destination))
            {
                UnitMove?.Invoke(destination);
                /*
                if (Pathfinding.GetPathDistance(CurrentCell, destination) <= WalkableDistance && Actions >= 1 && destination.Walkable)
                {
                    DoMove(destination);
                }
                else if (Pathfinding.GetPathDistance(CurrentCell, destination) <= WalkableDistance * 2 && Actions >= 2 && destination.Walkable)
                {
                    DoDoubleMove(destination);
                }
                */
            }
        }

        private void DoMove(Cell destination)
        {
            Actions--;
            List<Cell> path = Pathfinding.FindPath(CurrentCell.WorldPosition, destination.WorldPosition);
            //CurrentCell.Walkable = true;
            CurrentCell = destination;
            //CurrentCell.Walkable = false;
            UnitMoved?.Invoke(path);
            Stepper.Instance().SelectedUnit = this;
            Stepper.Instance().DisableButtonPress();
            //Debug.Log("DoMove");
        }

        private void DoDoubleMove(Cell destination)
        {
            Actions-=2;
            List<Cell> path = Pathfinding.FindPath(CurrentCell.WorldPosition, destination.WorldPosition);
            //CurrentCell.Walkable = true;
            CurrentCell = destination;
            //CurrentCell.Walkable = false;
            UnitMoved?.Invoke(path);
            Stepper.Instance().SelectedUnit = this;
            Stepper.Instance().DisableButtonPress();
            //Debug.Log("DoDoubleMove");
        }

        public virtual void PlaceUnit(Cell initCell)
        {
            //UnitPlaced?.Invoke(initCell);
            //initCell.AddUnit(this, IsEnemyUnit ? "Prefabs/EnemyUnit" : "Prefabs/PlayerUnit");
            //CurrentCell = initCell;
        }

        public void TurnDone()
        {
            if (Stepper.Instance().SelectedUnit!=null && Stepper.Instance().SelectedUnit.Equals(this))
            {
                Stepper.Instance().EnableButtonPress();
                Stepper.Instance().Step();
            }
        }

        public virtual void SpecialAction() 
        {
            Stepper.Instance().DisableButtonPress();
        }

        public virtual void ThrowGrenade(Cell target) 
        {
            UnitThrow?.Invoke(target);
        }

        public void Target(Unit targetUnit)
        {
            TargetChanged?.Invoke(targetUnit);
        }

        public void Shoot(int percentage, Unit enemyUnit, int minDamage, int maxDamage)
        {
            Stepper.Instance().DisableButtonPress();
            UnitShoots?.Invoke(percentage, enemyUnit, minDamage, maxDamage);
        }

        public void Dodge()
        {
            Stepper.Instance().DisableButtonPress();
            UnitDodged?.Invoke();
        }

        public void Hit()
        {
            Stepper.Instance().DisableButtonPress();
            UnitHit?.Invoke();
        }

        public void Died()
        {
            Stepper.Instance().DisableButtonPress();
            Owner.UnitDied(this);
            UnitDied?.Invoke();
        }

        public void HealthLost(int lostHealth)
        {
            if (Health - lostHealth < 0)
            {
                Health = 0;
                Died();
            }
            else
            {
                Health -= lostHealth;
                Hit();
            }
        }
    }
}
