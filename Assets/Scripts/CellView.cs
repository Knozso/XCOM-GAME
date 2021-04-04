using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Color = System.Drawing.Color;

namespace Model
{
    public class CellView : MonoBehaviour
    {
        public Cell Cell { get; private set; }

        public static float DefaultHeight = 0.05f;

        public void CreateCell()
        {
            Cell = new Cell();
            Cell.ColorChanged += c => ChangeColor(c);
            Cell.UnitAdded += u => UnitAdded(u);
        }

        public void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1))
            {
                var stepper = Stepper.Instance();
                var unitToMove = stepper.SelectedUnit;

                if (unitToMove == null)
                    return;

                unitToMove.MoveUnit(Cell);
            }
        }

        public void ChangeColor(Color color)
        {
            GetComponent<MaterialColorPicker>().SetColor(new UnityEngine.Color(color.R, color.G, color.B, color.A));
        }

        public void UnitAdded(Unit unit)
        {
            String prefabString = unit.IsEnemyUnit ? "Prefabs/EnemyUnit" : "Prefabs/PlayerUnit";
            float rotation = unit.IsEnemyUnit ? 180f : 0f;
            GameObject unitSprite = Instantiate(Resources.Load<GameObject>(prefabString), new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z), new Quaternion(0, rotation, 0, 0), null);
            UnitView unitView = unitSprite.GetComponent<UnitView>();
            unitView.setupUnit(unit);
        }
    }
}
