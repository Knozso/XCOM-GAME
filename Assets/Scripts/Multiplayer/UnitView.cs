using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Model
{
    public class UnitView : NetworkBehaviour
    {
        public Unit Unit;

        public int WalkableDistance = 31;

        [SyncVar(hook = nameof(ChangeCurrentMileageClientSide))]
        private int currentMileage = 0;

        [SyncVar(hook = nameof(ChangeDestinationListClientSide))]
        private List<Vector3> destinationList;
        private Animator animator;
        [SyncVar(hook = nameof(ChangePrevRotationClientSide))]
        private Quaternion prevRotation;

        [SyncVar(hook = nameof(ChangeCanSelectClientSide))]
        private bool canSelect;

        [SyncVar(hook = nameof(ChangeMovingClientSide))]
        private bool moving;

        private Camera MainCamera;

        [SyncVar(hook = nameof(ChangeGrenadeModeClientSide))]
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
                if (Input.GetMouseButtonDown(1))
                {
                    if(isServer)
                    {
                        CmdSelectByServer();
                    }
                    else
                    {
                        CmdSelectByClient();
                    }
                }
            }
        }

        [Command(requiresAuthority = false)]
        void CmdSelectByServer()
        {
            if(Unit.Owner.PlayerColor.Equals(Color.blue))
            {
                Unit.SelectUnit();
            }
        }

        [Command(requiresAuthority = false)]
        void CmdSelectByClient()
        {
            if (Unit.Owner.PlayerColor.Equals(Color.red))
            {
                Unit.SelectUnit();
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
            if (Pathfinding.GetPathDistance(Unit.CurrentCell, destination) <= WalkableDistance && Unit.Actions >= 1 && destination.Walkable)
            {
                DoMove(destination);
            }
            else if (Pathfinding.GetPathDistance(Unit.CurrentCell, destination) <= WalkableDistance * 2 && Unit.Actions >= 2 && destination.Walkable)
            {
                DoDoubleMove(destination);
            }
        }

        private void DoMove(Cell destination)
        {
            Unit.Actions--;
            List<Cell> path = Pathfinding.FindPath(Unit.CurrentCell.WorldPosition, destination.WorldPosition);
            //CurrentCell.Walkable = true;
            Unit.CurrentCell = destination;
            //CurrentCell.Walkable = false;
            UnitMoved(path);
            Stepper.Instance().SelectedUnit = Unit;
            Stepper.Instance().DisableButtonPress();
            //Debug.Log("DoMove");
        }

        private void DoDoubleMove(Cell destination)
        {
            Unit.Actions -= 2;
            List<Cell> path = Pathfinding.FindPath(Unit.CurrentCell.WorldPosition, destination.WorldPosition);
            //CurrentCell.Walkable = true;
            Unit.CurrentCell = destination;
            //CurrentCell.Walkable = false;
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
                    if (isServer)
                    {
                        Unit.TurnDone();
                    }
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
            int random = Random.Range(1, 100);
            //Debug.Log(percentage);
            if (random <= percentage)
            {
                int damage = Random.Range(minDamage, maxDamage+1);
                StartCoroutine(WaitWhileGettingHit(1, enemyUnit, damage));
                //_resultText.text = "HIT";
            }
            else
            {
                StartCoroutine(WaitWhileDodging(1, enemyUnit));
                //_resultText.text = "Miss";
            }
            Unit.Actions = 0;

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
            GameObject grenade = Instantiate(Resources.Load<GameObject>("Prefabs/Grenade"), new Vector3(Unit.CurrentCell.GetWorldX(), 2f, Unit.CurrentCell.GetWorldY()), new Quaternion(0, 0, 0, 0), null);
            NetworkServer.Spawn(grenade);
            GrenadeMovingScript moveScript = grenade.GetComponent<GrenadeMovingScript>();
            moveScript.StartMovement(Unit.CurrentCell, target);
            Stepper.Instance().SetGrenadeMode(false);
            Unit.Actions = 0;
            StartCoroutine(WaitAfterShot(3));
        }

        IEnumerator WaitAfterShot(int duration)
        {
            yield return new WaitForSeconds(duration);
            CameraControllerScript ccsc = MainCamera.GetComponent<CameraControllerScript>();
            Button aimButton = ccsc.AimButton;
            if (Camera.main == null)
            {
                aimButton.GetComponent<AimScript>().CancelEveryWhere();
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

        private void ChangeCurrentMileageClientSide(int oldMile, int currentMileage)
        {
            this.currentMileage = currentMileage;
        }

        private void ChangeDestinationListClientSide(List<Vector3> oldList, List<Vector3> destinationList)
        {
            this.destinationList = destinationList;
        }

        private void ChangePrevRotationClientSide(Quaternion oldRot, Quaternion prevRotation)
        {
            this.prevRotation = prevRotation;
        }

        private void ChangeCanSelectClientSide(bool oldCan, bool canSelect)
        {
            this.canSelect = canSelect;
        }

        private void ChangeMovingClientSide(bool oldMoving, bool moving)
        {
            this.moving = moving;
        }

        private void ChangeGrenadeModeClientSide(bool oldGrenade, bool grenadeMode)
        {
            this.grenadeMode = grenadeMode;
        }
    }
}
