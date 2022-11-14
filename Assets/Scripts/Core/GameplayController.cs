using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class GameplayController
    {
        public Action<int> UpdateLifeCount;
        
        private int _life;
        private readonly int _maxLife;
        private List<Cell> _selectedCells = new();
        private List<Cell> _availableCells = new();
        private List<CellConfig> _correctWay = new();
        private Cell[,] _cellsTable;
        
        private readonly CellHighlighter _cellHighlighter;
        private readonly WayBuilder _wayBuilder;
        private readonly CellViewController _cellViewController;
        private readonly SceneLoader _sceneLoader;

        public GameplayController(WayBuilder wayBuilder, 
                                    CellViewController cellViewController, 
                                    CellHighlighter cellHighlighter,
                                    SceneLoader sceneLoader,
                                    int life)
        {
            _cellHighlighter = cellHighlighter;
            _wayBuilder = wayBuilder;
            _cellViewController = cellViewController;
            _sceneLoader = sceneLoader;
            _maxLife = life;
            _life = life;
        }
    
        public void CallDefeat()
        {
            HandleDefeat();
        }

        public void Initialize()
        {
            SubscribeCells();
            UpdateCorrectWay();
            SetAvailableCells();
        }
        
        private void UpdateCorrectWay()
        {
            _correctWay = _wayBuilder.GetCorrectWayList();
        }

        private void SubscribeCells()
        {
            _cellsTable = _wayBuilder.GetCells();
            foreach (var cell in _cellsTable)
            {
                cell.CellTrigger += HandleChooseCell;
            }
        }

        private void SetAvailableCells()
        {
            for (int i = 0; i < _cellsTable.GetUpperBound(1) + 1; i++)
            {
                _availableCells.Add(_cellsTable[0, i]);
                _cellHighlighter.SwitchCellHighlight(_cellsTable[0, i], true);
            }
        }

        private void UpdateAvailableCells(Cell cell)
        {
            if (_availableCells.Count != 0)
            {
                foreach (var c in _availableCells)
                {
                    
                    _cellHighlighter.SwitchCellHighlight(c, false); // off previous cells
                }
                _availableCells.Clear();
            }
            
            foreach (var cellConfig in cell.CellConfig.NearbyCellsConfig)
            {
                Cell nearbyCell = _cellsTable[cellConfig.CellCoordinateInArray.x, cellConfig.CellCoordinateInArray.y];
                if (!_selectedCells.Contains(nearbyCell))
                {
                    _availableCells.Add(nearbyCell);
                    _cellHighlighter.SwitchCellHighlight(nearbyCell, true); // off previous cells
                }
            }
        }
        
        private void HandleChooseCell(Cell cell)
        {
            if (IsCorrectCell(cell))
            {
                _selectedCells.Add(cell);
                UpdateAvailableCells(cell);
                cell.HandlerCorrectWayCell.Invoke(); // Вызываем обработку метода
                _correctWay.Remove(_correctWay.First());
                CheckVictory();
            }
            else
            {
                if (IsCellUnselectable(cell))
                {
                    return;
                }
                cell.HandlerWrongWayCell.Invoke();
                DecreaseLife();
            }
        }

        private bool IsCellUnselectable(Cell targetCell)
        {
            return _selectedCells.Contains(targetCell) || !_availableCells.Contains(targetCell);
        }
        
        private bool IsCorrectCell(Cell cell)
        {
            return _correctWay.First().NumberCell == cell.CellConfig.NumberCell;
        } 
    
        private void DecreaseLife()
        {
            _life--;
            UpdateLifeCount.Invoke(_life);
            if (_life <= 0)
            {
                HandleDefeat();
            }
        }
    
        private void HandleDefeat()
        {
            UpdateCorrectWay();
            SetDefaultLife();
            UpdateLifeCount.Invoke(_life);
            SetSettingToDefault();
            _cellViewController.EnableAllCellsView();
            SetAvailableCells();
        }
    
        private void SetSettingToDefault()
        {
            _availableCells.Clear();
            _selectedCells.Clear();
            ResetCellsMaterial();
        }

        private void ResetCellsMaterial()
        {
            foreach (var cell in _cellsTable)
            {
                _cellHighlighter.SwitchCellHighlight(cell, false);
            }
        }
        
        private void SetDefaultLife()
        {
            _life = _maxLife;
        }
        
        private void CheckVictory()
        {
            if (_correctWay.Count == 0)
            {
                _sceneLoader.LoadNewLevel();
            }
        }
    }
}
