using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker, target;
    private static Grid Grid;

    void Awake()
    {
        Grid = GetComponent<Grid>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }

    public static List<Cell> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell startCell = Grid.CellFromWorldPosition(startPos);
        Cell targetCell = Grid.CellFromWorldPosition(targetPos);
        Grid.ResetTraceableCells();

        List<Cell> openList = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();
        openList.Add(startCell);

        while (openList.Count > 0)
        {
            Cell currentCell = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentCell.FCost || (openList[i].FCost == currentCell.FCost && openList[i].hCost < currentCell.hCost))
                {
                    currentCell = openList[i];
                }
            }

            openList.Remove(currentCell);
            closedSet.Add(currentCell);

            if (currentCell == targetCell)
            {
                return Retracepath(startCell, targetCell);
            }

            foreach (KeyValuePair<Direction, Cell> entry in currentCell.GetNeighbours())
            {

                Cell neighbour = entry.Value;
                bool directionHasCover = currentCell.Cover.ContainsKey(entry.Key) ? currentCell.Cover[entry.Key] : false;
                bool canNotWalkToNeighbour = !CanWalkToNeighbour(currentCell, entry);

                if (!neighbour.Walkable || closedSet.Contains(neighbour) || directionHasCover || canNotWalkToNeighbour)
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentCell.gCost + GetDistance(currentCell, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetCell);
                    neighbour.Parent = currentCell;

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }
        return new List<Cell>();
    }

    private static bool CanWalkToNeighbour(Cell currentCell, KeyValuePair<Direction, Cell> entry)
    {
        bool canWalkToNeighbour = true;
        if ((entry.Key.Equals(Direction.NorthEast) && ((entry.Value.Cover[Direction.South] && currentCell.Cover[Direction.North]) || (entry.Value.Cover[Direction.West] && currentCell.Cover[Direction.East])))
           || (entry.Key.Equals(Direction.NorthEast) && (currentCell.Cover[Direction.North] && currentCell.Cover[Direction.East]))
           || (entry.Key.Equals(Direction.NorthEast) && (entry.Value.Cover[Direction.South] && entry.Value.Cover[Direction.West])))
        {
            canWalkToNeighbour = false;
        }
        if ((entry.Key.Equals(Direction.NorthWest) && ((entry.Value.Cover[Direction.South] && currentCell.Cover[Direction.North]) || (entry.Value.Cover[Direction.East] && currentCell.Cover[Direction.West])))
            || (entry.Key.Equals(Direction.NorthWest) && (currentCell.Cover[Direction.North] && currentCell.Cover[Direction.West]))
            || (entry.Key.Equals(Direction.NorthWest) && (entry.Value.Cover[Direction.South] && entry.Value.Cover[Direction.East])))
        {
            canWalkToNeighbour = false;
        }
        if ((entry.Key.Equals(Direction.SouthEast) && ((entry.Value.Cover[Direction.North] && currentCell.Cover[Direction.South]) || (entry.Value.Cover[Direction.West] && currentCell.Cover[Direction.East])))
            || (entry.Key.Equals(Direction.SouthEast) && (currentCell.Cover[Direction.South] && currentCell.Cover[Direction.East]))
            || (entry.Key.Equals(Direction.SouthEast) && (entry.Value.Cover[Direction.North] && entry.Value.Cover[Direction.West])))
        {
            canWalkToNeighbour = false;
        }
        if ((entry.Key.Equals(Direction.SouthWest) && ((entry.Value.Cover[Direction.North] && currentCell.Cover[Direction.South]) || (entry.Value.Cover[Direction.East] && currentCell.Cover[Direction.West])))
            || (entry.Key.Equals(Direction.SouthWest) && (currentCell.Cover[Direction.South] && currentCell.Cover[Direction.West]))
            || (entry.Key.Equals(Direction.SouthWest) && (entry.Value.Cover[Direction.North] && entry.Value.Cover[Direction.East])))
        {
            canWalkToNeighbour = false;
        }
        return canWalkToNeighbour;
    }

    public static List<Cell> Retracepath(Cell startCell, Cell endCell)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = endCell;
        while (currentCell != startCell)
        {
            currentCell.Traceable = true;
            path.Add(currentCell);
            currentCell = currentCell.Parent;
        }
        currentCell.Traceable = true;
        path.Reverse();
        return path;
    }

    public static int GetDistance(Cell cellA, Cell cellB)
    {
        int distX = Mathf.Abs(cellA.GetGridX() - cellB.GetGridX());
        int distY = Mathf.Abs(cellA.GetGridY() - cellB.GetGridY());

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        return 14 * distX + 10 * (distY - distX);
    }

    public static int GetPathDistance(Cell cellA, Cell cellB)
    {
        List<Cell> cellList = FindPath(cellA.WorldPosition, cellB.WorldPosition);
        int distance = 0;
        Cell prevCell = null;
        foreach (Cell cell in cellList)
        {
            if (prevCell != null)
            {
                if (prevCell.GetGridX() != cell.GetGridX() && prevCell.GetGridY() != cell.GetGridY())
                {
                    distance += 14;
                }
                if (prevCell.GetGridX() == cell.GetGridX() && prevCell.GetGridY() != cell.GetGridY() || prevCell.GetGridX() != cell.GetGridX() && prevCell.GetGridY() == cell.GetGridY())
                {
                    distance += 10;
                }
            }
            prevCell = cell;
        }
        return distance;
    }

    public static List<Cell> GetCellsInRange(List<Cell> cells, Cell cell, int range)
    {
        List<Cell> cellsInRange = new List<Cell>();
        cellsInRange.Add(cell);
        foreach (KeyValuePair<Direction, Cell> entry in cell.Neighbours)
        {
            bool directionHasCover = cell.Cover.ContainsKey(entry.Key) ? cell.Cover[entry.Key] : false;
            if (directionHasCover || !CanWalkToNeighbour(cell, entry) || !entry.Value.Walkable || !cells.Contains(entry.Value))
            {
                continue;
            }
            int newRange = range;
            newRange -= GetDistance(cell, entry.Value);
            if (newRange >= 0)
            {
                foreach (Cell c in GetCellsInRange(cells, entry.Value, newRange))
                {
                    if (!cellsInRange.Contains(c))
                        cellsInRange.Add(c);
                }
            }
        }
        return cellsInRange;
    }

    public static List<Cell> GetCellsInSquareRange(Cell cell, int range)
    {
        List<Cell> cellList = new List<Cell>();
        int squareDist = (int)(range / 10)+1;
        int xMin = cell.GetGridX() - squareDist >= 0 ? cell.GetGridX() - squareDist : 0;
        int yMin = cell.GetGridY() - squareDist >= 0 ? cell.GetGridY() - squareDist : 0;
        int xMax = cell.GetGridX() + squareDist < Grid.gridSizeX ? cell.GetGridX() + squareDist : Grid.gridSizeX - 1;
        int yMax = cell.GetGridY() + squareDist < Grid.gridSizeY ? cell.GetGridY() + squareDist : Grid.gridSizeY - 1;

        for(int i=xMin; i<=xMax; i++)
        {
            for(int j=yMin; j<=yMax; j++)
            {
                cellList.Add(Grid.grid[i, j]);
            }
        }
        return cellList;
    }
}