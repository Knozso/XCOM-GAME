using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using color = System.Drawing.Color;


public class Grid : MonoBehaviour
{
    public GameObject mapTile;

    public LayerMask unwalkableMask;

    public bool dynamicCover = false;
    public int gridSizeX = 100;
    public int gridSizeY = 50;
    public float cellDiameter = 3f;
    public float cellHeight = 0.5f;
    public List<Vector2Int> playerUnitPositions;
    public List<Vector2Int> enemyUnitPositions;

    public static Cell[,] grid;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        AddUnits();
        Stepper.Instance().SelectedUnitChanged += unit => ColorCellsAroundUnit(unit);
    }

    private void Update()
    {
        if (dynamicCover)
        {
            UpdateCovers();
        }
    }

    private void CreateGrid()
    {
        grid = new Cell[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                var pos = new Vector3(x * cellDiameter, 0, y * cellDiameter);
                mapTile.transform.localScale = new Vector3(cellDiameter - 0.1f, cellDiameter - 0.1f, cellHeight);
                GameObject currTileGameObject = Instantiate(mapTile, pos, Quaternion.Euler(-90, 0, 0), transform);
                currTileGameObject.AddComponent<CellView>();
                CellView cellView = currTileGameObject.GetComponent<CellView>();
                cellView.CreateCell();
                Cell cell = cellView.Cell;
                cell.WorldPosition = new Vector3(x * cellDiameter, 0f, y * cellDiameter);
                cell.SetGridX(x);
                cell.SetGridY(y);
                //Vector3 worldPoint = new Vector3(x * cellDiameter, 0, 0) + new Vector3(0, 0, y * cellDiameter);
                //cell.Walkable = !(Physics.CheckSphere(worldPoint, cellDiameter / 3, unwalkableMask));

                if (x > 0)
                {
                    cell.SetNeigbour(Direction.West, grid[x - 1, y]);
                }
                if (y > 0)
                {
                    cell.SetNeigbour(Direction.South, grid[x, y - 1]);
                }
                if (x > 0 && y > 0)
                {
                    cell.SetNeigbour(Direction.SouthWest, grid[x - 1, y - 1]);
                }
                if (x > 0 && y + 1 < gridSizeY)
                {
                    cell.SetNeigbour(Direction.NorthWest, grid[x - 1, y + 1]);
                }
                grid[x, y] = cell;
            }
        }

        UpdateCovers();
    }

    public void UpdateCovers()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = new Vector3(x * cellDiameter, 0, y * cellDiameter);
                bool walkable = !(Physics.CheckSphere(worldPoint, cellDiameter / 4, unwalkableMask));
                bool northWalkable = ((Physics.CheckCapsule(worldPoint + new Vector3(-cellDiameter / 5, 0, cellDiameter / 2), worldPoint + new Vector3(cellDiameter / 5, 0, cellDiameter / 2), cellDiameter / 10, unwalkableMask)));
                bool eastWalkable = ((Physics.CheckCapsule(worldPoint + new Vector3(cellDiameter / 2, 0, cellDiameter / 5), worldPoint + new Vector3(cellDiameter / 2, 0, -cellDiameter / 5), cellDiameter / 10, unwalkableMask)));
                Cell cell = grid[x, y];
                cell.Walkable = walkable;
                cell.Cover[Direction.North] = northWalkable;
                if (cell.GetNeighbours().ContainsKey(Direction.North))
                {
                    cell.GetNeighbours()[Direction.North].Cover[Direction.South] = northWalkable;
                }
                cell.Cover[Direction.East] = eastWalkable;
                if (cell.GetNeighbours().ContainsKey(Direction.East))
                {
                    cell.GetNeighbours()[Direction.East].Cover[Direction.West] = eastWalkable;
                }
            }
        }
    }

    public void ColorCellsAroundUnit(Unit unit)
    {
        List<Cell> moveList = new List<Cell>();
        moveList = Pathfinding.GetCellsInSquareRange(unit.CurrentCell, unit.WalkableDistance*2);

        ResetCellsColor();

        foreach (Cell cell in moveList)
        {
            if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance && unit.Actions >= 1 && cell.Walkable)
            {
                cell.ChangeColor(color.Yellow);
            }
            else if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance*2 && unit.Actions >= 2 && cell.Walkable)
            {
                cell.ChangeColor(color.Blue);
            }
            else
            {
                cell.ChangeColor(color.FromArgb(1, 1, 1, 1));
            }
        }
    }
    

    public static void ResetCellsColor()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i,j].ChangeColor(color.FromArgb(1, 1, 1, 1));
            }
        }
    }

    public void AddUnits()
    {
        Stepper.Instance().Players.ForEach(p =>
        {
            if (p.PlayerColor.Equals(color.Blue))
            {
                foreach (Vector2Int cellCoord in playerUnitPositions)
                {
                    if (cellCoord.x >= 0 && cellCoord.x < gridSizeX && cellCoord.y >= 0 && cellCoord.y < gridSizeY)
                    {
                        Cell cellForUnit = grid[cellCoord.x, cellCoord.y];
                        Unit unit = new Unit(false);
                        cellForUnit.AddUnit(unit);
                        unit.Owner = p;
                        p.Units.Add(unit);
                        unit.PlaceUnit(cellForUnit);
                    }
                }
            }
            else
            {
                foreach (Vector2Int cellCoord in enemyUnitPositions)
                {
                    if (cellCoord.x >= 0 && cellCoord.x < gridSizeX && cellCoord.y >= 0 && cellCoord.y < gridSizeY)
                    {
                        Cell cellForUnit = grid[cellCoord.x, cellCoord.y];
                        Unit unit = new Unit(true);
                        cellForUnit.AddUnit(unit);
                        unit.Owner = p;
                        p.Units.Add(unit);
                        unit.PlaceUnit(cellForUnit);
                    }
                }
            }
        });
    }

    public Cell CellFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellDiameter);
        int y = Mathf.RoundToInt(worldPosition.z / cellDiameter);
        if(x>=0 && x<gridSizeX && y>=0 && y<gridSizeY)
            return grid[x, y];
        return grid[0, 0];
    }

    public void ResetTraceableCells()
    {
        foreach(Cell cell in grid) {
            cell.Traceable = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(gridSizeX / 2 * cellDiameter - cellDiameter / 2, 0, gridSizeY / 2 * cellDiameter - cellDiameter / 2), new Vector3(gridSizeX * cellDiameter, 1, gridSizeY * cellDiameter));
        if (grid != null)
        {
            foreach (Cell cell in grid)
            {
                Gizmos.color = (cell.Traceable) ? Color.blue : (cell.Walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(cell.WorldPosition, Vector3.one * (cellDiameter - cellDiameter / 2));

                Gizmos.color = Color.white;
                if(cell.GetGridX()==0)
                {
                    Gizmos.color = cell.Cover[Direction.West] ? Color.red : Color.white;
                    Gizmos.DrawCube(cell.WorldPosition + new Vector3(-cellDiameter / 2, 0, 0), new Vector3(0.1f, cellDiameter / 2, cellDiameter));
                }
                if(cell.GetGridY()==0)
                {
                    Gizmos.color = cell.Cover[Direction.South] ? Color.red : Color.white;
                    Gizmos.DrawCube(cell.WorldPosition + new Vector3(0, 0, -cellDiameter / 2), new Vector3(cellDiameter, cellDiameter / 2, 0.1f));
                }

                Gizmos.color = cell.Cover[Direction.East] ? Color.red : Color.white;
                Gizmos.DrawCube(cell.WorldPosition + new Vector3(cellDiameter / 2, 0, 0), new Vector3(0.1f, cellDiameter / 2, cellDiameter));
                
                Gizmos.color = cell.Cover[Direction.North] ? Color.red : Color.white;
                Gizmos.DrawCube(cell.WorldPosition + new Vector3(0, 0, cellDiameter / 2), new Vector3(cellDiameter, cellDiameter / 2, 0.1f));
            }
        }
    }
}
