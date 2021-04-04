using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class UnitView : MonoBehaviour
    {
        public Unit Unit;

        public int WalkableDistance = 50;
        private int currentMileage = 0;
        private List<Vector3> destinationList;
        private Animator animator;
        private Quaternion prevRotation;

        private bool canSelect;

        void Start()
        {
            SetKinematic(true);
            animator = GetComponent<Animator>();
            canSelect = true;
        }

        private void Update()
        {
            if (destinationList != null && currentMileage >= 0 && currentMileage < destinationList.Count && destinationList[currentMileage] != null)
            {
                Move();
            }
        }

        public void setupUnit(Unit unit)
        {
            unit.WalkableDistance = WalkableDistance;
            Unit = unit;
            unit.UnitMoved += u => UnitMoved(u);
        }

        public void OnMouseOver()
        {
            if (canSelect)
            {
                //Unit selected
                if (Input.GetMouseButtonDown(1))
                {
                    Unit.SelectUnit();
                    /*
                    Vector3 pos = transform.position;
                    Vector3 newPos = new Vector3(pos.x, 0, pos.z);
                    if (Camera.main != null)
                    {
                        Camera.main.transform.parent.position = newPos;
                        Camera.main.transform.LookAt(transform);
                    }
                    */
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
            bool moving = true;
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
                moving = true;
            }
            else
            {
                animator.Play("Idle");
                if(moving)
                {
                    Stepper.Instance().Step();
                    moving = false;
                }
            }
        }
    }
}
