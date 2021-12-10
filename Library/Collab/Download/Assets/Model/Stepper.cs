using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = System.Drawing.Color;

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

        private Guid _currentPlayerId;
        public int TurnNumber { get; set; }
        public List<Player> Players { get; }

        public event Action<Player> TurnChanged;
        public event Action<Player> GameOver;
        public event Action<Unit> SelectedUnitChanged;

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

        private Stepper()
        {
            Players = new List<Player>();

            var player1 = new Player { Name = "Blue", PlayerColor = Color.Blue };
            Players.Add(player1);
            var player2 = new Player { Name = "Red", PlayerColor = Color.Red };
            Players.Add(player2);
            _currentPlayerId = player1.Id;

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
                Grid.ResetCellsColor();
                Players.Remove(currentPlayer);
                Players.Insert(0, currentPlayer);
                currentPlayer = Players.LastOrDefault();
                _currentPlayerId = currentPlayer.Id;
                TurnNumber++;
                TurnChanged?.Invoke(currentPlayer);
                SelectedUnit = enemyPlayer.Units[0];
            }
            else if(SelectedUnit!=null && SelectedUnit.Actions==0)
            {
                foreach(Unit unit in currentPlayer.Units)
                {
                    if(unit.Actions>0)
                    {
                        SelectedUnit = unit;
                        break;
                    }
                }
            }
        }

        public void RemovePlayer(Player player)
        {

        }
    }
}
