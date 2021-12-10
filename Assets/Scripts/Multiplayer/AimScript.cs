using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using TMPro;
using UnityEngine.EventSystems;
using Mirror;

public class AimScript : NetworkBehaviour, IPointerDownHandler
{
    public Camera mainCamera;
    public GameObject mainCanvas;
    public Camera shootCamera;
    public GameObject shootCanvas;

    public TextMeshProUGUI _resultText;
    public TextMeshProUGUI _chanceText;
    public TextMeshProUGUI _damageText;

    public bool SniperMode;

    [SyncVar]
    private Unit _targetedUnit;
    public Unit TargetedUnit {
        get
        {
            return _targetedUnit;
        }
        set
        {
            _targetedUnit = value;
        }
    }

    [SyncVar]
    private Vector3 targetPosition;

    [SyncVar]
    private Vector3 currentPosition;

    public Unit CurrentUnit { get; set; }

    private int _percentage;

    public int Percentage
    {
        get
        {
            return _percentage;
        }

        set
        {
            if (value < 0)
            {
                _percentage = 0;
            }
            else
            {
                _percentage = value;
            }
        }
    }
    
    [SyncVar]
    private bool _aimingMode;
    public bool AimingMode {
        get
        {
            return _aimingMode;
        }
        set
        {
            _aimingMode = value;
        }
    }

    private void Start()
    {
        AimingMode = false;
        Percentage = 100;
        Stepper.Instance().EnableButtonPressAction += () => { transform.parent.gameObject.SetActive(true); };
        Stepper.Instance().DisableButtonPressAction += () => { transform.parent.gameObject.SetActive(false); };
        Stepper.Instance().SniperMode += (mode) => SniperModeChanged(mode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Stepper stepper = Stepper.Instance();
            Player enemyPlayer = stepper.GetEnemyPlayer();
            Unit targetUnit = TargetedUnit;
            for (int i = 0; i < enemyPlayer.Units.Count; i++)
            {
                if (targetUnit.Equals(enemyPlayer.Units[i]))
                {
                    if (i + 1 == enemyPlayer.Units.Count)
                    {
                        targetUnit = enemyPlayer.Units[0];
                    }
                    else
                    {
                        targetUnit = enemyPlayer.Units[i + 1];
                    }
                    break;
                }
            }
            SetTargetedUnit(targetUnit);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }

        if (AimingMode)
        {
            shootCamera.transform.LookAt(targetPosition + new Vector3(0, 3f, 0));
            Vector3 pos = currentPosition;
            Vector3 camPos = (pos - targetPosition).normalized;
            Vector3 diag = new Vector3(-camPos.z, 4f, camPos.x).normalized;
            shootCamera.transform.position = new Vector3(pos.x - camPos.x*4 + diag.x*4, 4f, pos.z - camPos.z*4 + diag.z*4);
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isServer)
        {
            CmdAimOnServer();
        }
        else
        {
            CmdAimOnClient();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAimOnServer()
    {
        Stepper stepper = Stepper.Instance();
        if(stepper.GetCurrentPlayer().PlayerColor.Equals(Color.blue))
        {
            if (stepper.GetCurrentPlayer().Units.Contains(stepper.SelectedUnit) && stepper.SelectedUnit.Actions > 0)
            {
                Vector3 pos = stepper.SelectedUnit.CurrentCell.WorldPosition;
                shootCamera.enabled = true;
                shootCanvas.GetComponent<Canvas>().enabled = true;
                _resultText.text = "";

                mainCamera.enabled = false;
                mainCanvas.GetComponent<Canvas>().enabled = false;
                Player enemyPlayer = stepper.GetEnemyPlayer();

                TargetedUnit = enemyPlayer.Units[0];
                Vector3 targetPos = TargetedUnit.CurrentCell.WorldPosition;
                Vector3 camPos = (pos - TargetedUnit.CurrentCell.WorldPosition).normalized;
                Vector3 diag = new Vector3(-camPos.z, 1.7f, camPos.x).normalized;
                shootCamera.transform.position = new Vector3(pos.x + camPos.x + diag.x * 2, 1.7f, pos.z + camPos.z + diag.z * 2);

                shootCamera.transform.LookAt(TargetedUnit.CurrentCell.WorldPosition + new Vector3(0, 1.5f, 0));
                AimingMode = true;
                /*
                foreach (Unit unit in stepper.GetCurrentPlayer().Units)
                {
                    unit.SetCanBeSelected(false);
                }
                */
                CurrentUnit = stepper.SelectedUnit;
                currentPosition = CurrentUnit.CurrentCell.WorldPosition;
                SetTargetedUnit(TargetedUnit);
                //stepper.SelectedUnit = null;
                Grid.ResetCellsColor();

                List<Vector3> positions = new List<Vector3>();
                positions.Add(pos);
                positions.Add(targetPos);
                this.positions = positions;
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAimOnClient()
    {
        Stepper stepper = Stepper.Instance();
        if (stepper.GetCurrentPlayer().PlayerColor.Equals(Color.red))
        {
            if (stepper.GetCurrentPlayer().Units.Contains(stepper.SelectedUnit) && stepper.SelectedUnit.Actions > 0)
            {
                Vector3 pos = stepper.SelectedUnit.CurrentCell.WorldPosition;
                shootCamera.enabled = true;
                shootCanvas.GetComponent<Canvas>().enabled = true;
                _resultText.text = "";

                mainCamera.enabled = false;
                mainCanvas.GetComponent<Canvas>().enabled = false;
                Player enemyPlayer = stepper.GetEnemyPlayer();

                TargetedUnit = enemyPlayer.Units[0];
                Vector3 targetPos = TargetedUnit.CurrentCell.WorldPosition;
                Vector3 camPos = (pos - TargetedUnit.CurrentCell.WorldPosition).normalized;
                Vector3 diag = new Vector3(-camPos.z, 1.7f, camPos.x).normalized;
                shootCamera.transform.position = new Vector3(pos.x + camPos.x + diag.x * 2, 1.7f, pos.z + camPos.z + diag.z * 2);

                shootCamera.transform.LookAt(TargetedUnit.CurrentCell.WorldPosition + new Vector3(0, 1.5f, 0));
                AimingMode = true;
                /*
                foreach (Unit unit in stepper.GetCurrentPlayer().Units)
                {
                    unit.SetCanBeSelected(false);
                }
                */
                CurrentUnit = stepper.SelectedUnit;
                currentPosition = CurrentUnit.CurrentCell.WorldPosition;
                SetTargetedUnit(TargetedUnit);
                //stepper.SelectedUnit = null;
                Grid.ResetCellsColor();

                List<Vector3> positions = new List<Vector3>();
                positions.Add(pos);
                positions.Add(targetPos);
                this.positions = positions;
            }
        }
    }

    [SyncVar(hook = nameof(PositionListChanged))]
    private List<Vector3> positions;

    private void PositionListChanged(List<Vector3> oldPositions, List<Vector3> newPositions)
    {
        Vector3 pos = newPositions[0];
        Vector3 targetPos = newPositions[1];
        shootCamera.enabled = true;
        shootCanvas.GetComponent<Canvas>().enabled = true;
        _resultText.text = "";

        mainCamera.enabled = false;
        mainCanvas.GetComponent<Canvas>().enabled = false;
        Vector3 camPos = (pos - targetPos).normalized;
        Vector3 diag = new Vector3(camPos.z, 3.4f, -camPos.x).normalized;
        shootCamera.transform.position = new Vector3(pos.x + camPos.x + diag.x * 2, 3.4f, pos.z + camPos.z + diag.z * 2);
        shootCamera.transform.LookAt(targetPos + new Vector3(0, 1.5f, 0));
    }

    public void SetTargetedUnit(Unit unit)
    {
        TargetedUnit = unit;
        Stepper stepper = Stepper.Instance();
        stepper.TargetedUnit = TargetedUnit;

        targetPosition = TargetedUnit.CurrentCell.WorldPosition;

        Percentage = 100;
        Percentage -= Stepper.Instance().CalculatePercentageBasedOnDistance(CurrentUnit.CurrentCell, TargetedUnit.CurrentCell, CurrentUnit);
        if (!SniperMode)
        {
            Percentage -= Stepper.Instance().CalculatePercentageBasedOnCover(CurrentUnit.CurrentCell, TargetedUnit.CurrentCell);
        }
        _chanceText.text = "Chance to hit: " + Percentage + "%";
        if (SniperMode)
        {
            _damageText.text = "Damage: 4-5";
        }
        else
        {
            _damageText.text = "Damage: 3-4";
        }
    }

    private void CalculatePercentageBasedOnDistance()
    {
        int distance = Pathfinding.GetDistance(CurrentUnit.CurrentCell, TargetedUnit.CurrentCell);
        if (distance < 80)
        {
            Percentage -= 0;
        }
        else if (distance < 120)
        {
            Percentage -= 25;
        }
        else if (distance < 160)
        {
            Percentage -= 50;
        }
        else if (distance < 200)
        {
            Percentage -= 75;
        }
        else
        {
            Percentage -= 100;
        }
    }

    private void CalculatePercentageBasedOnCover()
    {
        //Debug.Log(GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit));
        if (TargetedUnit.InCoverFrom(Direction.North) && GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.North)
        {
            Percentage -= 50;
        }
        if (TargetedUnit.InCoverFrom(Direction.North) && (GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.West || GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.East) && CurrentUnit.CurrentCell.GetGridY() > TargetedUnit.CurrentCell.GetGridY())
        {
            Percentage -= 25;
        }
        if (TargetedUnit.InCoverFrom(Direction.South) && GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.South)
        {
            Percentage -= 50;
        }
        if (TargetedUnit.InCoverFrom(Direction.South) && (GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.West || GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.East) && CurrentUnit.CurrentCell.GetGridY() < TargetedUnit.CurrentCell.GetGridY())
        {
            Percentage -= 25;
        }
        if (TargetedUnit.InCoverFrom(Direction.East) && GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.East)
        {
            Percentage -= 50;
        }
        if (TargetedUnit.InCoverFrom(Direction.East) && (GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.North || GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.South) && CurrentUnit.CurrentCell.GetGridX() > TargetedUnit.CurrentCell.GetGridX())
        {
            Percentage -= 25;
        }
        if (TargetedUnit.InCoverFrom(Direction.West) && GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.West)
        {
            Percentage -= 50;
        }
        if (TargetedUnit.InCoverFrom(Direction.West) && (GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.North || GetDirectionRelativeToOtherUnit(CurrentUnit, TargetedUnit) == Direction.South) && CurrentUnit.CurrentCell.GetGridX() < TargetedUnit.CurrentCell.GetGridX())
        {
            Percentage -= 25;
        }
    }

    public Direction GetDirectionRelativeToOtherUnit(Unit unit1, Unit unit2)
    {
        if (unit1.CurrentCell.GetGridY() > unit2.CurrentCell.GetGridY() && Mathf.Abs(unit2.CurrentCell.GetGridY() - unit1.CurrentCell.GetGridY()) > Mathf.Abs(unit2.CurrentCell.GetGridX() - unit1.CurrentCell.GetGridX()))
        {
            return Direction.North;
        }
        if (unit1.CurrentCell.GetGridY() < unit2.CurrentCell.GetGridY() && Mathf.Abs(unit2.CurrentCell.GetGridY() - unit1.CurrentCell.GetGridY()) > Mathf.Abs(unit2.CurrentCell.GetGridX() - unit1.CurrentCell.GetGridX()))
        {
            return Direction.South;
        }
        if (unit1.CurrentCell.GetGridX() < unit2.CurrentCell.GetGridX() && Mathf.Abs(unit2.CurrentCell.GetGridX() - unit1.CurrentCell.GetGridX()) > Mathf.Abs(unit2.CurrentCell.GetGridY() - unit1.CurrentCell.GetGridY()))
        {
            return Direction.West;
        }
        return Direction.East;
    }

    public void Cancel()
    {
        if(isServer)
        {
            CmdServerCancel();
        }
        else
        {
            CmdClientCancel();
        }
    }

    public void CancelEveryWhere()
    {
        if (isServer)
        {
            SniperMode = false;
            AimingMode = false;
            shootCanvas.GetComponent<Canvas>().enabled = false;
            mainCamera.enabled = true;
            mainCanvas.GetComponent<Canvas>().enabled = true;
            shootCamera.enabled = false;
            /*
            foreach (Unit unit in Stepper.Instance().GetCurrentPlayer().Units)
            {
                unit.SetCanBeSelected(true);
            }
            */

            if (CurrentUnit != null)
            {
                Stepper.Instance().SelectedUnit = CurrentUnit;
            }
            Stepper.Instance().TargetedUnit = null;
            CancelRpc();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdServerCancel()
    {
        if(Stepper.Instance().GetCurrentPlayer().PlayerColor.Equals(Color.blue))
        {
            SniperMode = false;
            AimingMode = false;
            shootCanvas.GetComponent<Canvas>().enabled = false;
            mainCamera.enabled = true;
            mainCanvas.GetComponent<Canvas>().enabled = true;
            shootCamera.enabled = false;
            /*
            foreach (Unit unit in Stepper.Instance().GetCurrentPlayer().Units)
            {
                unit.SetCanBeSelected(true);
            }
            */

            if (CurrentUnit != null)
            {
                Stepper.Instance().SelectedUnit = CurrentUnit;
            }
            Stepper.Instance().TargetedUnit = null;
            CancelRpc();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdClientCancel()
    {
        if (Stepper.Instance().GetCurrentPlayer().PlayerColor.Equals(Color.red))
        {
            AimingMode = false;
            SniperMode = false;
            shootCanvas.GetComponent<Canvas>().enabled = false;
            mainCamera.enabled = true;
            mainCanvas.GetComponent<Canvas>().enabled = true;
            shootCamera.enabled = false;
            /*
            foreach (Unit unit in Stepper.Instance().GetCurrentPlayer().Units)
            {
                unit.SetCanBeSelected(true);
            }
            */

            if (CurrentUnit != null)
            {
                Stepper.Instance().SelectedUnit = CurrentUnit;
            }
            Stepper.Instance().TargetedUnit = null;
            CancelRpc();
        }
    }

    [ClientRpc]
    private void CancelRpc()
    {
        shootCanvas.GetComponent<Canvas>().enabled = false;
        mainCamera.enabled = true;
        mainCanvas.GetComponent<Canvas>().enabled = true;
        shootCamera.enabled = false;
    }

    private void SniperModeChanged(bool sniperMode)
    {
        SniperMode = sniperMode;
        if (SniperMode)
        {
            if (isServer)
            {
                CmdAimOnServer();
            }
            else
            {
                CmdAimOnClient();
            }
        }
        else
        {
            Cancel();
        }
    }
}
