using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgunner : Unit
{
    public Shotgunner(bool isEnemyunit, bool multiplayer) : base(isEnemyunit, multiplayer)
    {
        Actions = 3;
        if (isEnemyunit && multiplayer)
        {
            PrefabString = "Prefabs/EnemyUnit";
        }
        else if (!isEnemyunit && multiplayer)
        {
            PrefabString = "Prefabs/PlayerUnit";
        }
        else if (isEnemyunit && !multiplayer)
        {
            PrefabString = "Prefabs/EnemyUnitSinglePlayer";
        }
        else if (!isEnemyunit && !multiplayer)
        {
            PrefabString = "Prefabs/PlayerUnitSinglePlayer";
        }
        AbilityName = "";
    }

    public override void PlaceUnit(Cell initCell)
    {
        initCell.AddUnit(this, IsEnemyUnit ? "Prefabs/ShotgunUnit" : "Prefabs/ShotgunUnit");
        CurrentCell = initCell;
    }

    public override void SpecialAction()
    {
        //base.SpecialAction();
        //Stepper.Instance().SetSniperMode(true);
    }

    public override void SelectUnit()
    {
        base.SelectUnit();
    }
}
