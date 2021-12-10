using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model
{
    public class Stepper
    {
        private static Stepper _instance;
        public static Stepper Instance()
        {
            if (_instance == null)
            {
                _instance = new Stepper();
            }

            return _instance;
        }

        public Guid _currentPlayerId;
        public int TurnNumber { get; set; }
        public List<Player> Players { get; }

        public event Action<Player> TurnChanged;
        public event Action<Player> GameOver;
        public event Action<Unit> SelectedUnitChanged;
        public event Action EnableButtonPressAction;
        public event Action DisableButtonPressAction;
        public event Action<bool> GrenadeMode;
        public event Action<bool> SniperMode;

        public Unit TargetedUnit { get; set; }

        private Unit _selectedUnit;
        public Unit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                SelectedUnitChanged?.Invoke(_selectedUnit);
            }
        }

        public Stepper()
        {
            Players = new List<Player>();

            //GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerVSAi");

            //var player1 = new Player { Name = "Blue", PlayerColor = Color.blue };
            //Players.Add(player1);
            //var player2 = new Player { Name = "Red", PlayerColor = Color.red };
            //if (objs.Length!=0)
            //{
            //    player2 = new AiPlayer { Name = "Red", PlayerColor = Color.red };
            //}
            //Players.Add(player2);
            //_currentPlayerId = player1.Id;

            TurnNumber = 1;
        }

        public static void ResetInstance()
        {
            _instance = new Stepper();
        }

        public Player GetCurrentPlayer()
        {
            return Players.FirstOrDefault(x => x.Id == _currentPlayerId);
        }

        public Player GetEnemyPlayer()
        {
            return Players.FirstOrDefault(x => x.Id != _currentPlayerId);
        }

        public void Step()
        {
            var currentPlayer = Players.FirstOrDefault(x => x.Id == _currentPlayerId);
            if (currentPlayer.GetRemainingActions() == 0)
            {
                var enemyPlayer = Players.FirstOrDefault(x => x.Id != _currentPlayerId);
                currentPlayer.ResetActions();
                GameObject[] objs = GameObject.FindGameObjectsWithTag("Multiplayer");
                if(objs.Length!=0)
                {
                    Grid.ResetCellsColor();
                }
                else
                {
                    GridSinglePlayer.ResetCellsColor();
                }
                Players.Remove(currentPlayer);
                Players.Insert(0, currentPlayer);
                currentPlayer = Players.LastOrDefault();
                _currentPlayerId = currentPlayer.Id;
                TurnNumber++;
                TurnChanged?.Invoke(currentPlayer);
                enemyPlayer.Units[0].SelectUnit();
            }
            else if (SelectedUnit != null && SelectedUnit.Actions == 0)
            {
                foreach (Unit u in currentPlayer.Units)
                {
                    if (u.Actions > 0)
                    {
                        u.SelectUnit();
                        break;
                    }
                }
            }
            currentPlayer.TakeTurn();
        }

        public int CalculatePercentageBasedOnDistance(Cell currrentCell, Cell targetCell)
        {
            int percentage = 0;
            int distance = Pathfinding.GetDistance(currrentCell, targetCell);

            if (distance < 80)
            {
                percentage = 98;
            }
            else if (distance < 120)
            {
                percentage = 49;
            }
            else if (distance < 160)
            {
                percentage = 0;
            }
            else if (distance < 200)
            {
                percentage = 10;
            }
            else
            {
                percentage = 20;
            }

            return percentage;
        }

        public int CalculatePercentageBasedOnDistance(Cell currrentCell, Cell targetCell, Unit unit)
        {
            int percentage = 0;
            int distance = Pathfinding.GetDistance(currrentCell, targetCell);
            if (unit is Sniper)
            {
                if (distance < 80)
                {
                    percentage = 98;
                }
                else if (distance < 120)
                {
                    percentage = 49;
                }
                else if (distance < 160)
                {
                    percentage = 0;
                }
                else if (distance < 200)
                {
                    percentage = 10;
                }
                else
                {
                    percentage = 20;
                }
            }
            else if(unit is Shotgunner)
            {
                if(distance < 60)
                {
                    percentage = 0;
                }
                else if (distance < 80)
                {
                    percentage = 20;
                }
                else
                {
                    percentage = 100;
                }
            }
            else
            {
                if (distance < 80)
                {
                    percentage = 0;
                }
                else if (distance < 120)
                {
                    percentage = 25;
                }
                else if (distance < 160)
                {
                    percentage = 50;
                }
                else if (distance < 200)
                {
                    percentage = 75;
                }
                else
                {
                    percentage = 100;
                }
            }
            return percentage;
        }

        public Player GetOtherPlayer(Player thisPlayer)
        {
            return Players.FirstOrDefault(x => x.Id != thisPlayer.Id);
        }

        public int CalculatePercentageBasedOnCover(Cell currentCell, Cell targetCell)
        {
            int percentage = 0;
            if (targetCell.HasCoverFrom(Direction.North) && GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.North)
            {
                percentage = 50;
            }
            if (targetCell.HasCoverFrom(Direction.North) && (GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.West || GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.East) && currentCell.GetGridY() > targetCell.GetGridY())
            {
                percentage = 25;
            }
            if (targetCell.HasCoverFrom(Direction.South) && GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.South)
            {
                percentage = 50;
            }
            if (targetCell.HasCoverFrom(Direction.South) && (GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.West || GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.East) && currentCell.GetGridY() < targetCell.GetGridY())
            {
                percentage = 25;
            }
            if (targetCell.HasCoverFrom(Direction.East) && GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.East)
            {
                percentage = 50;
            }
            if (targetCell.HasCoverFrom(Direction.East) && (GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.North || GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.South) && currentCell.GetGridX() > targetCell.GetGridX())
            {
                percentage = 25;
            }
            if (targetCell.HasCoverFrom(Direction.West) && GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.West)
            {
                percentage = 50;
            }
            if (targetCell.HasCoverFrom(Direction.West) && (GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.North || GetDirectionRelativeToOtherCell(currentCell, targetCell) == Direction.South) && currentCell.GetGridX() < targetCell.GetGridX())
            {
                percentage = 25;
            }
            return percentage;
        }

        public Direction GetDirectionRelativeToOtherCell(Cell cell1, Cell cell2)
        {
            if (cell1.GetGridY() > cell2.GetGridY() && Mathf.Abs(cell2.GetGridY() - cell1.GetGridY()) > Mathf.Abs(cell2.GetGridX() - cell1.GetGridX()))
            {
                return Direction.North;
            }
            if (cell1.GetGridY() < cell2.GetGridY() && Mathf.Abs(cell2.GetGridY() - cell1.GetGridY()) > Mathf.Abs(cell2.GetGridX() - cell1.GetGridX()))
            {
                return Direction.South;
            }
            if (cell1.GetGridX() < cell2.GetGridX() && Mathf.Abs(cell2.GetGridX() - cell1.GetGridX()) > Mathf.Abs(cell2.GetGridY() - cell1.GetGridY()))
            {
                return Direction.West;
            }
            return Direction.East;
        }

        public void RemovePlayer(Player player)
        {

        }

        public void EnableButtonPress()
        {
            EnableButtonPressAction?.Invoke();
        }

        public void DisableButtonPress()
        {
            DisableButtonPressAction?.Invoke();
        }

        public void SetGrenadeMode(bool grenadeMode)
        {
            GrenadeMode?.Invoke(grenadeMode);
        }

        public void SetSniperMode(bool sniperMode)
        {
            SniperMode?.Invoke(sniperMode);
        }
    }
}
