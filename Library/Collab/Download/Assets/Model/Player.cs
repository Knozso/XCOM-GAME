using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;

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
            PlayerColor = Color.Red;
        }
        
        /*
        public void ActionHappened()
        {
            int remainingActions = 0;
            foreach(Unit unit in Units)
            {
                remainingActions += unit.Actions;
            }
            if(remainingActions==0)
            {
                Stepper.Instance().Step();
            }
        }
        */

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
                unit.Actions = 2;
            }
        }

        public void UnitDied(Unit unit)
        {

        }
    }
}
