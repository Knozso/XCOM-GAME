using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Color PlayerColor { get; set; }
        public List<Unit> Units { get; set; }

        public Player()
        {
            Id = Guid.NewGuid();
            Units = new List<Unit>();
            PlayerColor = Color.red;
        }

        public virtual void MoveUnitToCell(Unit unit, Cell cell) {
            unit.MoveUnit(cell);
        }

        public virtual void TakeTurn() {

        }

        public int GetRemainingActions()
        {
            int remainingActions = 0;
            foreach (Unit unit in Units)
            {
                remainingActions += unit.Actions;
            }
            return remainingActions;
        }

        public void ResetActions()
        {
            foreach (Unit unit in Units)
            {
                if(unit is Shotgunner)
                {
                    unit.Actions = 3;
                }
                else
                {
                    unit.Actions = 2;
                }
            }
        }

        public void UnitDied(Unit unit)
        {

        }
    }
}
