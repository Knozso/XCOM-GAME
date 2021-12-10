using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;

public class Sniper : Unit
{
    public Sniper(bool isEnemyunit, bool multiplayer) : base(isEnemyunit, multiplayer)
    {
        if (isEnemyunit && multiplayer)
        {
            PrefabString = "Prefabs/SniperUnit";
        }
        else if (!isEnemyunit && multiplayer)
        {
            PrefabString = "Prefabs/SniperUnit";
        }
        else if (isEnemyunit && !multiplayer)
        {
            PrefabString = "Prefabs/SniperUnitSinglePlayer";
        }
        else if (!isEnemyunit && !multiplayer)
        {
            PrefabString = "Prefabs/SniperUnitSinglePlayer";
        }
        AbilityName = "Snipe";
    }

    public override void PlaceUnit(Cell initCell)
    {
        initCell.AddUnit(this, IsEnemyUnit ? "Prefabs/SniperUnit" : "Prefabs/SniperUnit");
        CurrentCell = initCell;
    }

    public override void SpecialAction()
    {
        //base.SpecialAction();
        Stepper.Instance().SetSniperMode(true);
    }
}
