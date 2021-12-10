using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Model;

public class UnitViewSinglePlayer : MonoBehaviour
{
    public Unit Unit;

    public int WalkableDistance = 31;

    private int currentMileage = 0;

    private List<Vector3> destinationList;
    private Animator animator;
    private Quaternion prevRotation;

    private bool canSelect;

    private bool moving;

    private Camera MainCamera;

    private bool grenadeMode = false;

    public void InitUnit()
    {
        SetKinematic(true);
        animator = GetComponent<Animator>();
        canSelect = true;
        MainCamera = Camera.main;
        grenadeMode = false;
        Stepper.Instance().GrenadeMode += b => GrenadeModeChanged(b);
    }

    void Start()
    {
        InitUnit();
    }

    private void Update()
    {
        if (destinationList != null && currentMileage >= 0 && currentMileage < destinationList.Count && destinationList[currentMileage] != null)
        {
            moving = true;
            Move();
        }
    }

    public void setupUnit(Unit unit)
    {
        unit.WalkableDistance = WalkableDistance;
        Unit = unit;
        unit.UnitMoved += u => UnitMoved(u);
        unit.UnitMove += c => UnitMove(c);
        unit.TargetChanged += u => TargetChanged(u);
        unit.UnitShoots += (percent, enemyUnit, minDamage, maxDamage) => Shoot(percent, enemyUnit, minDamage, maxDamage);
        unit.UnitDodged += () => Dodge();
        unit.UnitHit += () => Hit();
        unit.UnitThrow += (c) => ThrowGrenade(c);
        //unit.UnitDied += () => Die();
    }

    /*
    public void Die()
    {
        SetKinematic(false);
        GetComponent<Animator>().enabled = false;
        Destroy(gameObject, 5);
    }
    */

    public void OnMouseOver()
    {
        if (canSelect)
        {
            //Unit selected
            if (Input.GetMouseButtonDown(1)) //&& !grenadeMode)
            {
                Unit.SelectUnit();
            }
        }
    }

    void SetKinematic(bool newValue)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            rb.isKinematic = newValue;
        }
    }

    public void UnitMove(Cell destination)
    {
        if (PathfindingSinglePlayer.GetPathDistance(Unit.CurrentCell, destination) <= WalkableDistance && Unit.Actions >= 1 && destination.Walkable)
        {
            DoMove(destination);
        }
        else if (PathfindingSinglePlayer.GetPathDistance(Unit.CurrentCell, destination) <= WalkableDistance * 2 && Unit.Actions >= 2 && destination.Walkable)
        {
            DoDoubleMove(destination);
        }
    }

    private void DoMove(Cell destination)
    {
        Unit.Actions--;
        List<Cell> path = PathfindingSinglePlayer.FindPath(Unit.CurrentCell.WorldPosition, destination.WorldPosition);
        Unit.CurrentCell.Occupied = false;
        Unit.CurrentCell = destination;
        destination.Occupied = true;
        UnitMoved(path);
        Stepper.Instance().SelectedUnit = Unit;
        Stepper.Instance().DisableButtonPress();
        //Debug.Log("DoMove");
    }

    private void DoDoubleMove(Cell destination)
    {
        Unit.Actions -= 2;
        List<Cell> path = PathfindingSinglePlayer.FindPath(Unit.CurrentCell.WorldPosition, destination.WorldPosition);
        Unit.CurrentCell.Occupied = false;
        Unit.CurrentCell = destination;
        destination.Occupied = true;
        UnitMoved(path);
        Stepper.Instance().SelectedUnit = Unit;
        Stepper.Instance().DisableButtonPress();
        //Debug.Log("DoDoubleMove");
    }

    public void UnitMoved(List<Cell> destination)
    {
        currentMileage = 0;
        destinationList = new List<Vector3>();
        foreach (Cell cell in destination)
        {
            destinationList.Add(cell.WorldPosition);
        }
    }

    public void Move()
    {
        if (destinationList[currentMileage] != new Vector3() && Vector3.Distance(destinationList[currentMileage], transform.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destinationList[currentMileage], 10 * Time.deltaTime);

            Vector3 pos = transform.position;
            Vector3 newPos = new Vector3(pos.x, 0, pos.z);
            Camera.main.transform.parent.position = newPos;

            if (destinationList[currentMileage] - transform.position == new Vector3())
            {
                transform.rotation = prevRotation;
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(2 * (destinationList[currentMileage] - transform.position));
            }
            prevRotation = transform.rotation;
        }
        else
        {
            if (currentMileage < destinationList.Count)
            {
                currentMileage++;
            }
        }

        if (currentMileage < destinationList.Count)
        {
            animator.Play("Run");
        }
        else
        {
            if (moving)
            {
                animator.Play("Idle");
                Unit.TurnDone();
            }
            moving = false;
        }
    }

    public void TargetChanged(Unit targetUnit)
    {
        transform.LookAt(targetUnit.CurrentCell.WorldPosition);
    }

    public void Shoot(int percentage, Unit enemyUnit, int minDamage, int maxDamage)
    {
        animator.Play("Shoot");
        if (Unit is Shotgunner)
        {
            Vector3 magassag = Vector3.Normalize(Unit.CurrentCell.WorldPosition - enemyUnit.CurrentCell.WorldPosition) * 8 * 3;
            Vector3 mer1Vektor = Vector3.Normalize(new Vector3(-magassag.z, magassag.y, magassag.x)) * 8 * Mathf.Sqrt(3);
            Vector3 aPont = Stepper.Instance().SelectedUnit.CurrentCell.WorldPosition;
            Vector3 bPont = Stepper.Instance().SelectedUnit.CurrentCell.WorldPosition + magassag + mer1Vektor;
            Vector3 cPont = Stepper.Instance().SelectedUnit.CurrentCell.WorldPosition + magassag - mer1Vektor;

            foreach (Unit enemyUnits in Stepper.Instance().GetEnemyPlayer().Units)
            {
                if(enemyUnit.Equals(enemyUnits))
                {
                    int shotgunRand = Random.Range(1, 100);
                    if (shotgunRand <= percentage)
                    {
                        int damage = Random.Range(minDamage, maxDamage + 1);
                        StartCoroutine(WaitWhileGettingHit(1, enemyUnit, damage));
                    }
                    else
                    {
                        StartCoroutine(WaitWhileDodging(1, enemyUnit));
                    }
                }
                else
                {
                    if (GridSinglePlayer.PointInTriangle(enemyUnits.CurrentCell.WorldPosition, aPont, bPont, cPont))
                    {
                        int shotgunPercent = 100;
                        shotgunPercent -= Stepper.Instance().CalculatePercentageBasedOnCover(Unit.CurrentCell, enemyUnits.CurrentCell);
                        shotgunPercent -= Stepper.Instance().CalculatePercentageBasedOnDistance(Unit.CurrentCell, enemyUnits.CurrentCell, Unit);
                        int shotgunRand = Random.Range(1, 100);
                        //Debug.Log(percentage);
                        if (shotgunRand <= shotgunPercent)
                        {
                            int damage = Random.Range(minDamage, maxDamage + 1);
                            StartCoroutine(WaitWhileGettingHit(1, enemyUnits, damage));
                        }
                        else
                        {
                            StartCoroutine(WaitWhileDodging(1, enemyUnits));
                        }
                    }
                }
            }
            Unit.Actions--;
        }
        else
        {
            int random = Random.Range(1, 100);
            //Debug.Log(percentage);
            if (random <= percentage)
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                StartCoroutine(WaitWhileGettingHit(1, enemyUnit, damage));
                //_resultText.text = "HIT";
            }
            else
            {
                StartCoroutine(WaitWhileDodging(1, enemyUnit));
                //_resultText.text = "Miss";
            }
            Unit.Actions = 0;
        }

        StartCoroutine(WaitAfterShot(3));
    }

    public void Dodge()
    {
        animator.Play("Dodge");
    }

    public void Hit()
    {
        animator.Play("Hit");
    }

    public void ThrowGrenade(Cell target)
    {
        animator.Play("Throw");
        GameObject grenade = Instantiate(Resources.Load<GameObject>("Prefabs/GrenadeSinglePlayer"), new Vector3(Unit.CurrentCell.GetWorldX(), 2f, Unit.CurrentCell.GetWorldY()), new Quaternion(0, 0, 0, 0), null);
        GrenadeMovingSinglePlayerScript moveScript = grenade.GetComponent<GrenadeMovingSinglePlayerScript>();
        moveScript.StartMovement(Unit.CurrentCell, target);
        Stepper.Instance().SetGrenadeMode(false);
        Unit.Actions = 0;
        StartCoroutine(WaitAfterShot(3));
    }

    IEnumerator WaitAfterShot(int duration)
    {
        yield return new WaitForSeconds(duration);
        CameraControllerSinglePlayer ccsc = MainCamera.GetComponent<CameraControllerSinglePlayer>();
        Button aimButton = ccsc.AimButton;
        if (Camera.main == null)
        {
            aimButton.GetComponent<AimScriptSinglePlayer>().Cancel();
        }
        Unit.TurnDone();
    }


    IEnumerator WaitWhileDodging(int duration, Unit enemyUnit)
    {
        yield return new WaitForSeconds(duration);
        enemyUnit.Dodge();
    }

    IEnumerator WaitWhileGettingHit(int duration, Unit enemyUnit, int damage)
    {
        yield return new WaitForSeconds(duration);
        //Debug.Log(damage);
        enemyUnit.HealthLost(damage);
    }

    private void GrenadeModeChanged(bool grenadeMode)
    {
        this.grenadeMode = grenadeMode;
    }
}
