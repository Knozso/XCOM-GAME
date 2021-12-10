using Mirror;
using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : NetworkBehaviour
{
    private bool grenadeMode = false;

    void Start()
    {
        grenadeMode = false;
        Stepper.Instance().GrenadeMode+=g=>GrenadeModeChanged(g);
    }

    private void GrenadeModeChanged(bool grenadeMode)
    {
        this.grenadeMode = grenadeMode;
        if (grenadeMode)
        {
            Grid.ColorCellsAroundUnitSpecific(Stepper.Instance().SelectedUnit, Color.cyan, Stepper.Instance().SelectedUnit.WalkableDistance * 3);
            CellView cellView = this.GetComponentInParent<CellView>();
            cellView.resetColor = GetComponent<Renderer>().material.color;
        }
        else
        {
            CellView cellView = this.GetComponentInParent<CellView>();
            cellView.resetColor = cellView.alphaColor;
            Grid.ResetCellsColor();
        }
    }

    void OnMouseEnter()
    {
        CmdEnterOnServer();
    }

    [Command(requiresAuthority = false)]
    void CmdEnterOnServer()
    {
        CellView cellView = this.GetComponentInParent<CellView>();
        cellView.originalColor = GetComponent<Renderer>().material.color;
        Cell cell = cellView.Cell;
        if (grenadeMode && Pathfinding.GetDistance(Stepper.Instance().SelectedUnit.CurrentCell, cell) <= Stepper.Instance().SelectedUnit.WalkableDistance * 3)
        {
            Grid.ResetCellsColor();
            GetComponent<Renderer>().material.color = Color.red;
            cell.ChangeColor(UnityEngine.Color.red);
            foreach (KeyValuePair<Direction, Cell> entry in cell.GetNeighbours())
            {
                entry.Value.ChangeColor(UnityEngine.Color.red);
            }
        }
        else
        {
            CmdSelectOnServer();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSelectOnServer()
    {
        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        CellView cellView = this.GetComponentInParent<CellView>();
        cellView.originalColor = GetComponent<Renderer>().material.color;
        cellView.ChangeColor(Color.cyan);
    }
    
    void OnMouseExit()
    {
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        //CellView cellView = this.GetComponentInParent<CellView>();
        //Cell cell = cellView.Cell;
        //cell.ChangeColor(Color.clear);
        //cellView.ChangeColor(cellView.originalColor);
        //GetComponent<Renderer>().material.color = cellView.originalColor;
        CmdDeSelectOnServer();
    }

    [Command(requiresAuthority = false)]
    void CmdDeSelectOnServer()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        CellView cellView = this.GetComponentInParent<CellView>();
        cellView.ChangeColor(cellView.originalColor);
    }
}
