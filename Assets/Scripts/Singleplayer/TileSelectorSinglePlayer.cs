using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelectorSinglePlayer : MonoBehaviour
{
    private bool grenadeMode = false;

    void Start()
    {
        grenadeMode = false;
        Stepper.Instance().GrenadeMode += g => GrenadeModeChanged(g);
    }

    private void GrenadeModeChanged(bool grenadeMode)
    {
        this.grenadeMode = grenadeMode;
        if (grenadeMode)
        {
            GridSinglePlayer.ColorCellsAroundUnitSpecific(Stepper.Instance().SelectedUnit, Color.cyan, Stepper.Instance().SelectedUnit.WalkableDistance * 3);
            CellViewSinglePlayer cellView = this.GetComponentInParent<CellViewSinglePlayer>();
            cellView.resetColor = GetComponent<Renderer>().material.color;
        }
        else
        {
            CellViewSinglePlayer cellView = this.GetComponentInParent<CellViewSinglePlayer>();
            cellView.resetColor = cellView.alphaColor;
            GridSinglePlayer.ResetCellsColor();
        }
    }

    void OnMouseEnter()
    {
        CellViewSinglePlayer cellView = this.GetComponentInParent<CellViewSinglePlayer>();
        cellView.originalColor = GetComponent<Renderer>().material.color;
        Cell cell = cellView.Cell;
        if (grenadeMode && PathfindingSinglePlayer.GetDistance(Stepper.Instance().SelectedUnit.CurrentCell, cell) <= Stepper.Instance().SelectedUnit.WalkableDistance*3)
        {
            GridSinglePlayer.ResetCellsColor();
            GetComponent<Renderer>().material.color = Color.red;
            cell.ChangeColor(UnityEngine.Color.red);
            foreach (KeyValuePair<Direction, Cell> entry in cell.GetNeighbours())
            {
                entry.Value.ChangeColor(UnityEngine.Color.red);
            }
        }
        else
        {
            //transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            //GetComponent<Renderer>().material.color = Color.cyan;
            //cellView.ChangeColor(Color.cyan);
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            cellView.originalColor = GetComponent<Renderer>().material.color;
            cellView.ChangeColor(Color.cyan);
        }
    }

    void OnMouseExit()
    {
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        //CellView cellView = this.GetComponentInParent<CellView>();
        //Cell cell = cellView.Cell;
        //cell.ChangeColor(Color.clear);
        //cellView.ChangeColor(cellView.originalColor);
        //GetComponent<Renderer>().material.color = cellView.originalColor;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        CellViewSinglePlayer cellView = this.GetComponentInParent<CellViewSinglePlayer>();
        cellView.ChangeColor(cellView.originalColor);
    }
}
