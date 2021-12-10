using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Model;

public class CellViewSinglePlayer : MonoBehaviour
{
    public Cell Cell { get; private set; }

    private UnityEngine.Color color;

    public UnityEngine.Color alphaColor;

    public UnityEngine.Color resetColor;

    public UnityEngine.Color originalColor;

    public static float DefaultHeight = 0.05f;

    public bool GrenadeMode = false;

    public void CreateCell()
    {
        Cell = new Cell();
        Cell.ColorChanged += (c) => ChangeColor(c);
        Cell.UnitAdded += (u, p) => UnitAdded(u, p);
        GrenadeMode = false;
        Stepper.Instance().GrenadeMode += b => GrenadeModeChanged(b);
        alphaColor = gameObject.GetComponent<Renderer>().material.color;
        originalColor = gameObject.GetComponent<Renderer>().material.color;
        resetColor = gameObject.GetComponent<Renderer>().material.color;
    }

    public void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1) && !GrenadeMode)
        {
            var stepper = Stepper.Instance();
            var unitToMove = stepper.SelectedUnit;
            Player player = stepper.GetCurrentPlayer();

            if (unitToMove == null)
                return;

            if(!Cell.Occupied)
            {
                player.MoveUnitToCell(unitToMove, Cell);
            }
            //unitToMove.MoveUnit(Cell);

        }
        
        if (Input.GetMouseButtonDown(0) && GrenadeMode)
        {
            Stepper.Instance().SelectedUnit.ThrowGrenade(Cell);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Stepper.Instance().SetGrenadeMode(false);
            GridSinglePlayer.ColorCellsAroundUnit(Stepper.Instance().SelectedUnit);
            Stepper.Instance().EnableButtonPress();
        }
    }

    public void ChangeColor(UnityEngine.Color color)
    {
        this.color = color;
        if (color.Equals(UnityEngine.Color.clear))
        {
            gameObject.GetComponent<Renderer>().material.color = resetColor;
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.color = color;
        }
    }

    public void ChangeResetColorClientSide(UnityEngine.Color oldColor, UnityEngine.Color resetColor)
    {
        this.resetColor = resetColor;
    }

    public void ChangeOriginalColorClientSide(UnityEngine.Color oldColor, UnityEngine.Color originalColor)
    {
        this.originalColor = originalColor;
    }

    public void UnitAdded(Unit unit, String prefabString)
    {
        //this.Unit = unit;
        //String prefabString = unit.IsEnemyUnit ? "Prefabs/EnemyUnit" : "Prefabs/PlayerUnit";
        float rotation = unit.IsEnemyUnit ? 180f : 0f;
        GameObject unitSprite = Instantiate(Resources.Load<GameObject>(prefabString), new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z), new Quaternion(0, rotation, 0, 0), null);
        UnitView unitView = unitSprite.AddComponent(typeof(UnitView)) as UnitView;
        //UnitView unitView = unitSprite.GetComponent<UnitView>();
        //unitView.InitUnit();
        unitView.setupUnit(unit);
        //InitUnit();
    }

    //[Command(requiresAuthority = false)]
    /*
    private void InitUnit()
    {
        float rotation = Unit.IsEnemyUnit ? 180f : 0f;
        GameObject unitSprite = Instantiate(Resources.Load<GameObject>(prefabString), new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z), new Quaternion(0, rotation, 0, 0), null);
        //UnitView unitView = unitSprite.AddComponent(typeof(UnitView)) as UnitView;
        UnitView unitView = unitSprite.GetComponent<UnitView>();
        //unitView.InitUnit();
        unitView.setupUnit(Unit);
        NetworkServer.Spawn(unitSprite);
    }
    */
    private void GrenadeModeChanged(bool grenadeMode)
    {
        this.GrenadeMode = grenadeMode;
    }
}
