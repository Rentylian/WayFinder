using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class WayBuilder
    {
        private Cell[,] _cellTable;
        private CellConfig[,] _cellTableConfig;
        private List<CellConfig> _availableCells = new();
        private bool _isWayBuild;
        private int _columnCount;
        private int _rowCount;
        private int _rowUpperBound;
        private int _columnUpperBound;
        private readonly List<CellConfig> _correctWay = new();
        private readonly bool _isSquareField;
        private readonly int _startValueRow;
        private readonly int _maxValueRow;
        private readonly int _startValueColumn;
        private readonly int _maxValueColumn;

        public WayBuilder(bool isSquare, int startValueRow, int maxValueRow, int startValueColumn, int maxValueColumn)
        {
            _isSquareField = isSquare;
            _startValueRow = startValueRow;
            _maxValueRow = maxValueRow;
            _startValueColumn = startValueColumn;
            _maxValueColumn = maxValueColumn;
        }
        
        public List<CellConfig> GetCorrectWayList()
        {
            CellConfig[] correctWay = new CellConfig[_correctWay.Count];
            _correctWay.CopyTo(correctWay);
            return correctWay.ToList();
        }
    
        public Cell[,] GetCells()
        {
            return _cellTable;
        }
    
        public void CreateConfig()
        {
            // max exclusive, so plus 1
            _rowCount = Random.Range(_startValueRow, _maxValueRow + 1); 
            _columnCount = Random.Range(_startValueColumn, _maxValueColumn + 1);
            if (_isSquareField)
            {
                _rowCount = _columnCount;    
            }
            _cellTableConfig = new CellConfig[_rowCount, _columnCount];
            var currentCellNumber = 0;
            for (int i = 0; i < _rowCount; i++)
            {
                for (int j = 0; j < _columnCount; j++)
                {
                    currentCellNumber++;
                    _cellTableConfig[i, j] = new CellConfig();
                    CellConfig cell = _cellTableConfig[i, j];
                    cell.NumberCell =  currentCellNumber;
                    _availableCells.Add(cell);
                    cell.CellCoordinateInArray = new Vector2Int(i, j);
                }
            }
        
            _columnUpperBound = _cellTableConfig.GetUpperBound(1);
            _rowUpperBound = _cellTableConfig.GetUpperBound(0);
            FindCloseCells();
            BuildWay();
            CreateCells();
        }

        private void CreateCells()
        {
            _cellTable = new Cell[_rowCount, _columnCount];
            for (int i = 0; i < _rowCount; i++)
            {
                for (int j = 0; j < _columnCount; j++)
                {
                    Cell cell = new Cell(_cellTableConfig[i, j]);
                    _cellTable[i, j] = cell;
                }
            }
        }
    
        private void BuildWay()
        {
            CellConfig previousCell;
            var startPosition =  Random.Range(0, _columnUpperBound);
            Vector2Int currentCellCoord = new Vector2Int(0, startPosition);
            CellConfig currentCell = _cellTableConfig[currentCellCoord.x, currentCellCoord.y];
            _correctWay.Add(currentCell);
            
            while (!_isWayBuild)
            {
                previousCell = currentCell;
                // way shouldn`t return back
                RemoveLesserCellNumber(currentCell);
                currentCell = GetNewCell(currentCell);
                _availableCells = _availableCells.Except(previousCell.NearbyCellsConfig).ToList();
                if (_availableCells.Contains(previousCell))
                {
                    _availableCells.Remove(previousCell);
                }
                _correctWay.Add(currentCell);
                if (currentCell.CellCoordinateInArray.x == _rowUpperBound)
                {
                    _isWayBuild = true;
                }
            }
        }
        
        private void RemoveLesserCellNumber(CellConfig targetCell)
        {
            if (targetCell.NearbyCellsConfig.Count <= 1)
            {
                return;
            }
            int minCell = targetCell.NearbyCellsConfig.Min(x => x.NumberCell);
            CellConfig cellForRemove = targetCell.NearbyCellsConfig.Find(x => x.NumberCell == minCell);
            targetCell.NearbyCellsConfig.Remove(cellForRemove);
        }

        private List<CellConfig> GetIntersectCells(CellConfig currentCell)
        {
            List<CellConfig> availableNearbyCell = new List<CellConfig>();
            
            foreach (var c in _availableCells)
            {
                foreach (var v in currentCell.NearbyCellsConfig)
                {
                    if (v.NumberCell == c.NumberCell)
                    {
                        availableNearbyCell.Add(v);
                    }
                }
            }
            
            return availableNearbyCell;
        }
    
        private CellConfig GetNewCell(CellConfig cell)
        {
            CellConfig newCell = new CellConfig();
            List<CellConfig> availableNearbyCells = GetIntersectCells(cell);
        
            if (availableNearbyCells.Count > 0)
            {
                int nearbyCellsCount = availableNearbyCells.Count; 
                int random = Random.Range(0, nearbyCellsCount); 
                newCell = availableNearbyCells[random];
            }
            else
            {
                // try-catch?
                Debug.LogError($"NO AVAILABLE CELLS"); 
            }

            return newCell;
        }
    
        private void FindCloseCells() 
        {
            for (int i = 0; i < _rowCount; i++)
            {
                for (int j = 0; j < _columnCount; j++)
                {
                    CellConfig currentCell = _cellTableConfig[i, j];
                    FindNearbyCellByIndex(i, j, currentCell);
                }
            }
        }

        private void FindNearbyCellByIndex(int rowNumber, int columnNumber, CellConfig cell)
        {
            if (rowNumber < _rowUpperBound)
            {
                // bottom cell
                AddNearbyCell(cell, rowNumber + 1, columnNumber);
                // first cell have only one available cell
                if (rowNumber == 0)
                {
                    return;
                }
            }
            
            if (rowNumber > 0)
            {
                // top cell
                AddNearbyCell(cell, rowNumber - 1, columnNumber);
            }
        
            if (columnNumber < _columnUpperBound)
            {
                // right cell
                AddNearbyCell(cell, rowNumber, columnNumber + 1);
            }
        
            if (columnNumber > 0)
            {
                // left cell
                AddNearbyCell(cell, rowNumber, columnNumber - 1);
            }
        }

        private void AddNearbyCell(CellConfig cell, int rowIndex, int columnIndex)
        {
            cell.NearbyCellsConfig.Add(_cellTableConfig[rowIndex, columnIndex]);
        }
    }
}
