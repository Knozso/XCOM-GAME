using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model
{
    public class AiPlayer : Player
    {
        private const int DEPTH = 0;

        public AiPlayer() : base() { }

        public override void MoveUnitToCell(Unit unit, Cell cell) { }

        public override void TakeTurn()
        {
            List<Unit> availableUnits = GetAvailableUnits();
            Unit unit = availableUnits[0];

            if (unit.Actions == 1)
            {
                ShootWithUnit(unit);
            }
            else
            {
                GridSinglePlayer.CalculateValuesForCells(unit);

                List<Cell> cellList = GetTopCellsForUnitToMoveTo(unit);

                Cell bestMove = null;
                float maxValueCell = Single.NegativeInfinity;
                foreach (Cell move in cellList)
                {
                    float realValue = Search(DEPTH, unit, move, Single.NegativeInfinity, Single.PositiveInfinity, false);
                    if (realValue > maxValueCell)
                    {
                        maxValueCell = realValue;
                        bestMove = move;
                    }
                }

                if (unit.CurrentCell.Equals(bestMove))
                {
                    ShootWithUnit(unit);
                }
                else
                {
                    unit.MoveUnit(bestMove);
                }
            }
        }

        private float Search(int depth, Unit unit, Cell move, float alpha, float beta, bool maximizingPlayer)
        {
            if (depth==0)
            {
                return Evaluate(move);
            }

            float valueForAllPlayers = 0;
            Cell originalCell = unit.CurrentCell;
            unit.CurrentCell = move;

            if (maximizingPlayer)
            {
                Player enemyPlayer = Stepper.Instance().GetOtherPlayer(unit.Owner);
                foreach (Unit enemyUnit in enemyPlayer.Units)
                {
                    float value = Single.NegativeInfinity;
                    List<Cell> cellList = GetTopCellsForUnitToMoveTo(enemyUnit);
                    foreach (Cell cell in cellList)
                    {
                        value = Math.Max(value, Search(depth - 1, enemyUnit, cell, alpha, beta, false));
                        alpha = Math.Max(alpha, value);
                        if(alpha>=beta)
                        {
                            //Alfa vágás
                            break;
                        }
                    }
                    valueForAllPlayers += value;
                }
            }
            else
            {
                Player enemyPlayer = Stepper.Instance().GetOtherPlayer(unit.Owner);
                foreach (Unit enemyUnit in enemyPlayer.Units)
                {
                    float value = Single.PositiveInfinity;
                    List<Cell> cellList = GetTopCellsForUnitToMoveTo(enemyUnit);
                    foreach (Cell cell in cellList)
                    {
                        value = Math.Min(value, Search(depth - 1, enemyUnit, cell, alpha, beta, true));
                        alpha = Math.Min(beta, value);
                        if (beta<=alpha)
                        {
                            //Béta vágás
                            break;
                        }
                    }
                    valueForAllPlayers -= value;
                }
            }

            unit.CurrentCell = originalCell;
            return valueForAllPlayers;
        }

        private float Evaluate(Cell move)
        {
            return move.Value;
        }

        private List<Cell> GetTopCellsForUnitToMoveTo(Unit unit)
        {
            List<Cell> moveList = PathfindingSinglePlayer.GetCellsInSquareRange(unit.CurrentCell, unit.WalkableDistance * unit.Actions);
            List<Cell> sortedList = moveList.OrderByDescending(o => o.Value).ToList();
            List<Cell> top5List = sortedList.ToArray().Take(5).ToList();
            return top5List;
        }

        private List<Unit> GetAvailableUnits()
        {
            List<Unit> availableUnits = new List<Unit>();
            foreach (Unit unit in Units)
            {
                if (unit.Actions > 0)
                {
                    availableUnits.Add(unit);
                }
            }
            return availableUnits;
        }

        public void ShootWithUnit(Unit selectedUnit)
        {
            Player enemyPlayer = Stepper.Instance().GetEnemyPlayer();
            Unit shootThisUnit = null;
            foreach (Unit unit in enemyPlayer.Units)
            {
                if (shootThisUnit == null || unit.CurrentCell.Value > shootThisUnit.CurrentCell.Value)
                {
                    shootThisUnit = unit;
                }
            }

            int percentage = 100;
            percentage -= Stepper.Instance().CalculatePercentageBasedOnDistance(selectedUnit.CurrentCell, shootThisUnit.CurrentCell);
            percentage -= Stepper.Instance().CalculatePercentageBasedOnCover(selectedUnit.CurrentCell, shootThisUnit.CurrentCell);
            if (percentage < 0)
            {
                percentage = 0;
            }

            selectedUnit.Target(shootThisUnit);
            selectedUnit.Shoot(percentage, shootThisUnit, 3, 4);
        }
    }
}
