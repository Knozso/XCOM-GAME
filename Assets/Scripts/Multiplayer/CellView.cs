using System;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Model
{
    public class CellView : NetworkBehaviour
    {
        public Cell Cell { get; private set; }

        [SyncVar(hook = nameof(ChangeColorClientSide))]
        private UnityEngine.Color color;

        [SyncVar(hook = nameof(ChangeAlphaColorClientSide))]
        public UnityEngine.Color alphaColor;

        [SyncVar(hook = nameof(ChangeResetColorClientSide))]
        public UnityEngine.Color resetColor;

        [SyncVar(hook = nameof(ChangeOriginalColorClientSide))]
        public UnityEngine.Color originalColor;

        public static float DefaultHeight = 0.05f;

        [SyncVar(hook = nameof(ChangeGrenadeModeClientSide))]
        public bool GrenadeMode = false;

        public void CreateCell()
        {
            Cell = new Cell();
            Cell.ColorChanged += (c) => ChangeColor(c);
            Cell.UnitAdded += (u, p) => UnitAdded(u,p);
            GrenadeMode = false;
            Stepper.Instance().GrenadeMode += b=>GrenadeModeChanged(b);
            alphaColor = gameObject.GetComponent<Renderer>().material.color;
            originalColor = gameObject.GetComponent<Renderer>().material.color;
            resetColor = gameObject.GetComponent<Renderer>().material.color;
        }

        public void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1) && !GrenadeMode)
            {
                if(isServer)
                {
                    CmdMoveUnitToCellByServer();
                }
                else
                {
                    CmdMoveUnitToCellByClient();
                }
            }

            if (Input.GetMouseButtonDown(0) && GrenadeMode)
            {
                CmdThrowGrenadeByServer();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CmdExitByServer();
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdMoveUnitToCellByServer()
        {
            var stepper = Stepper.Instance();
            var unitToMove = stepper.SelectedUnit;
            Player player = stepper.GetCurrentPlayer();

            if (unitToMove == null)
                return;

            if (player.PlayerColor.Equals(Color.blue))
            {
                player.MoveUnitToCell(unitToMove, Cell);
            }
            //unitToMove.MoveUnit(Cell);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdMoveUnitToCellByClient()
        {
            var stepper = Stepper.Instance();
            var unitToMove = stepper.SelectedUnit;
            Player player = stepper.GetCurrentPlayer();

            if (unitToMove == null)
                return;

            if (player.PlayerColor.Equals(Color.red))
            {
                player.MoveUnitToCell(unitToMove, Cell);
            }
            //unitToMove.MoveUnit(Cell);
        }

        [Command(requiresAuthority = false)]
        private void CmdThrowGrenadeByServer()
        {
            Stepper.Instance().SelectedUnit.ThrowGrenade(Cell);
        }

        [Command(requiresAuthority = false)]
        private void CmdExitByServer()
        {
            Stepper.Instance().SetGrenadeMode(false);
            Grid.ColorCellsAroundUnit(Stepper.Instance().SelectedUnit);
            Stepper.Instance().EnableButtonPress();
        }

        public void ChangeColor(UnityEngine.Color color)
        {
            this.color = color;
            if(color.Equals(UnityEngine.Color.clear))
            {
                gameObject.GetComponent<Renderer>().material.color = resetColor;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = color;
            }
        }

        public void ChangeColorClientSide(UnityEngine.Color oldColor, UnityEngine.Color color)
        {
            if (color.Equals(UnityEngine.Color.clear))
            {
                gameObject.GetComponent<Renderer>().material.color = resetColor;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = color;
            }
        }

        public void ChangeAlphaColorClientSide(UnityEngine.Color oldColor, UnityEngine.Color resetColor)
        {
            this.resetColor = resetColor;
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
            NetworkServer.Spawn(unitSprite);
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

        private void ChangeGrenadeModeClientSide(bool oldGrenadeMode, bool newGrenadeMode)
        {
            if (!newGrenadeMode)
            {
                gameObject.GetComponent<Renderer>().material.color = resetColor;
            }
        }
    }
}
