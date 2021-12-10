using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Grenadier : Unit
    {
        public Grenadier(bool isEnemyunit, bool multiplayer) : base(isEnemyunit, multiplayer) 
        {
            if(isEnemyunit && multiplayer)
            {
                PrefabString = "Prefabs/GrenadierEnemyUnit";
            }
            else if(!isEnemyunit && multiplayer)
            {
                PrefabString = "Prefabs/GrenadierUnit";
            }
            else if (isEnemyunit && !multiplayer)
            {
                PrefabString = "Prefabs/GrenadierEnemyUnitSinglePlayer";
            }
            else if (!isEnemyunit && !multiplayer)
            {
                PrefabString = "Prefabs/GrenadierUnitSinglePlayer";
            }
            AbilityName = "Grenade";
        }

        public override void PlaceUnit(Cell initCell)
        {
            initCell.AddUnit(this, IsEnemyUnit ? "Prefabs/EnemyUnit" : "Prefabs/GrenadierUnit");
            CurrentCell = initCell;
        }

        public override void SpecialAction()
        {
            base.SpecialAction();
            Stepper.Instance().SetGrenadeMode(true);
        }
    }
}
