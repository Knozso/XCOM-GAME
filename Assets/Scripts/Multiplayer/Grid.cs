using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mirror;

public class Grid : NetworkBehaviour
{
    public GameObject mapTile;

    public LayerMask unwalkableMask;
    public List<GameObject> mapElements;

    [SyncVar]
    public bool GridCreated;

    public bool dynamicCover = false;
    public int gridSizeX = 100;
    public int gridSizeY = 50;
    public float cellDiameter = 3f;
    public float cellHeight = 0.5f;
    public List<Vector2Int> playerUnitPositions;
    public List<Vector2Int> enemyUnitPositions;

    public static Cell[,] grid;

    public static float MaxCellValue = 0;

    public static List<Cell> CellsInRange;

    // Start is called before the first frame update

    public void InitGrid()
    {
        if(!GridCreated)
        {
            CreateGrid();
            AddRandomCover();
            //AddUnits();
            Stepper.Instance().SelectedUnitChanged += unit => ColorCellsAroundUnit(unit);
            CellsInRange = new List<Cell>();
            GridCreated = true;
        }
    }

    public void ChangeGridCreatedClientSide(bool oldGridCreated, bool gridCreated)
    {
        this.GridCreated = gridCreated;
    }

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        GridCreated = false;
        InitGrid();
        Debug.Log("Server started");
    }

    [Client]
    public override void OnStartClient()
    {
        base.OnStartClient();
        if(isServer)
        {
            CmdAddPlayer();
        }
        else
        {
            CmdAddEnemyPlayer();
            CmdAddUnits();
        }
        Debug.Log("Client started");
    }

    private void Update()
    {
        if (isServer && GridCreated && dynamicCover)
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
                //currTileGameObject.AddComponent<CellView>();
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
                NetworkServer.Spawn(currTileGameObject);
            }
        }

        UpdateCovers();

    }

    public void AddRandomCover()
    {
        /*
        GameObject mapElement = mapElements[0];
        Vector2 elementSize = mapElement.GetComponent<MapElementGridSize>().Size;
        Instantiate(mapElement, new Vector3(0-cellDiameter/2 + (elementSize.x * cellDiameter / 2), 1, 0-cellDiameter/2 + (elementSize.y * cellDiameter / 2)), new Quaternion(0, 0, 0, 0), null);
        */

        int currentlyCoveredCellNumber = 0;
        int cellNumber = gridSizeX * gridSizeY;
        while (currentlyCoveredCellNumber < cellNumber)
        {
            int randElementIndex = Random.Range(0, mapElements.Count - 1);
            int randElementRotation = Random.Range(0, 2);
            GameObject mapElement = mapElements[randElementIndex];
            Vector2 elementSize = mapElement.GetComponent<MapElementGridSize>().Size;

            Cell firstCellWithNoElement = null;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Cell cell = grid[i, j];
                    if (!cell.HasElement)
                    {
                        firstCellWithNoElement = cell;
                        break;
                    }
                }
                if(firstCellWithNoElement!=null)
                {
                    break;
                }
            }

            if (firstCellWithNoElement != null)
            {
                int gridX = firstCellWithNoElement.GetGridX();
                int gridY = firstCellWithNoElement.GetGridY();

                float elementX = (randElementRotation == 0 ? elementSize.x : elementSize.y);
                float elementY = (randElementRotation == 0 ? elementSize.y : elementSize.x);

                float x = firstCellWithNoElement.GetWorldX() - cellDiameter / 2 + (elementX * cellDiameter / 2);
                float y = firstCellWithNoElement.GetWorldY() - cellDiameter / 2 + (elementY * cellDiameter / 2);
                Vector3 pos = new Vector3(x, 1, y);

                bool fits = true;
                for (int i = gridX; i < gridX + elementX; i++)
                {
                    for (int j = gridY; j < gridY + elementY; j++)
                    {
                        if (i >= 0 && i < gridSizeX && j >= 0 && j < gridSizeY)
                        {
                            if(grid[i, j].HasElement)
                            {
                                fits = false;
                            }
                        }
                    }
                }

                bool rotatedFits = true;
                if (!fits)
                {
                    randElementRotation = randElementRotation==0?1:0;
                    elementX = (randElementRotation == 0 ? elementSize.x : elementSize.y);
                    elementY = (randElementRotation == 0 ? elementSize.y : elementSize.x);

                    x = firstCellWithNoElement.GetWorldX() - cellDiameter / 2 + (elementX * cellDiameter / 2);
                    y = firstCellWithNoElement.GetWorldY() - cellDiameter / 2 + (elementY * cellDiameter / 2);
                    pos = new Vector3(x, 1, y);
                    for (int i = gridX; i < gridX + elementX; i++)
                    {
                        for (int j = gridY; j < gridY + elementY; j++)
                        {
                            if (i >= 0 && i < gridSizeX && j >= 0 && j < gridSizeY)
                            {
                                if (grid[i, j].HasElement)
                                {
                                    rotatedFits = false;
                                }
                            }
                        }
                    }
                }

                if (fits || rotatedFits)
                {
                    GameObject go = Instantiate(mapElement, pos, new Quaternion(0, 0, 0, 0), null);
                    go.transform.Rotate(0, randElementRotation * 90, 0);

                    for (int i = gridX; i < gridX + elementX; i++)
                    {
                        for (int j = gridY; j < gridY + elementY; j++)
                        {
                            if (i >= 0 && i < gridSizeX && j >= 0 && j < gridSizeY)
                            {
                                grid[i, j].HasElement = true;
                                if (elementX % 2 == 0 || elementY % 2 == 0)
                                {
                                    grid[i, j].MapElements.Add(go);
                                }
                                else if (i > gridX && i < gridX + elementX - 1 && j > gridY && j < gridY + elementY - 1)
                                {
                                    grid[i, j].MapElements.Add(go);
                                }
                                currentlyCoveredCellNumber += 1;
                            }
                        }
                    }

                    NetworkServer.Spawn(go);
                }
                else
                {
                    for (int i = gridX; i < gridX + elementX; i++)
                    {
                        for (int j = gridY; j < gridY + elementY; j++)
                        {
                            if (i >= 0 && i < gridSizeX && j >= 0 && j < gridSizeY)
                            {
                                if (!grid[i, j].HasElement)
                                {
                                    grid[i, j].HasElement = true;
                                    currentlyCoveredCellNumber += 1;
                                }
                            }
                        }
                    }
                }
            }
        }
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

    public static void ColorCellsAroundUnit(Unit unit)
    {
        if (unit!=null && unit.Owner!=null && !unit.Owner.GetType().Equals(typeof(AiPlayer)))
        {
            List<Cell> moveList = new List<Cell>();
            moveList = Pathfinding.GetCellsInRange(unit.CurrentCell, unit.WalkableDistance * 2);

            ResetCellsColor();

            foreach (Cell cell in moveList)
            {
                if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance && unit.Actions >= 1 && cell.Walkable)
                {
                    cell.ChangeColor(UnityEngine.Color.yellow);
                    cell.Value += 50;
                }
                else if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance * 2 && unit.Actions >= 2 && cell.Walkable)
                {
                    cell.ChangeColor(UnityEngine.Color.blue);
                    cell.Value += 10;
                }
                else
                {
                    //cell.ChangeColor(color.FromArgb(1, 1, 1, 1));
                    cell.ChangeColor(UnityEngine.Color.clear);
                }
            }
        }
        CalculateValuesForCells(unit);
    }

    public static void ColorCellsAroundUnitSpecific(Unit unit, UnityEngine.Color color, int distance)
    {
        if (unit != null && unit.Owner != null && !unit.Owner.GetType().Equals(typeof(AiPlayer)))
        {
            List<Cell> moveList = new List<Cell>();
            moveList = Pathfinding.GetCellsInSquareRange(unit.CurrentCell, distance);

            ResetCellsColor();

            foreach (Cell cell in moveList)
            {
                if (Pathfinding.GetDistance(unit.CurrentCell, cell) <= distance)
                {
                    cell.ChangeColor(color);
                }
                else
                {
                    //cell.ChangeColor(color.FromArgb(1, 1, 1, 1));
                    cell.ChangeColor(UnityEngine.Color.clear);
                }
            }
        }
    }

    public static void CalculateValuesForCells(Unit unit)
    {
        ResetCellsValue();
        List<Cell> moveList = Pathfinding.GetCellsInSquareRange(unit.CurrentCell, unit.WalkableDistance * 2);

        foreach (Cell cell in moveList)
        {
            if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance && unit.Actions >= 1 && cell.Walkable)
            {
                cell.Value += 50;
            }
            else if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance * 2 && unit.Actions >= 2 && cell.Walkable)
            {
                cell.Value += 10;
            }
        }

        foreach (Unit enemyUnit in Stepper.Instance().GetOtherPlayer(unit.Owner).Units)
        {
            foreach (Cell cell in grid)
            {
                int percentage = 100;
                percentage -= Stepper.Instance().CalculatePercentageBasedOnDistance(enemyUnit.CurrentCell, cell);
                percentage -= Stepper.Instance().CalculatePercentageBasedOnCover(enemyUnit.CurrentCell, cell);
                if (percentage < 0)
                {
                    percentage = 0;
                }
                cell.Value -= percentage;
            }
        }

        foreach (Unit enemyUnit in Stepper.Instance().GetOtherPlayer(unit.Owner).Units)
        {
            foreach (Cell cell in grid)
            {
                int percentage = 100;
                percentage -= Stepper.Instance().CalculatePercentageBasedOnDistance(cell, enemyUnit.CurrentCell);
                percentage -= Stepper.Instance().CalculatePercentageBasedOnCover(cell, enemyUnit.CurrentCell);
                if (percentage < 0)
                {
                    percentage = 0;
                }
                cell.Value += percentage;
                if (cell.Value > MaxCellValue)
                {
                    MaxCellValue = cell.Value;
                }
            }
        }
    }

    public static void CalculateValuesForCellsBasedOnOtherUnits(Unit unit)
    {
        ResetCellsValue();

        List<Cell> moveList = new List<Cell>();
        moveList = Pathfinding.GetCellsInSquareRange(unit.CurrentCell, unit.WalkableDistance * 2);

        foreach (Cell cell in moveList)
        {
            if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance && unit.Actions >= 1 && cell.Walkable)
            {
                cell.Value += 50;
            }
            else if (Pathfinding.GetPathDistance(unit.CurrentCell, cell) <= unit.WalkableDistance * 2 && unit.Actions >= 2 && cell.Walkable)
            {
                cell.Value += 10;
            }
        }

        foreach (Unit enemyUnit in Stepper.Instance().GetEnemyPlayer().Units)
        {
            foreach (Cell cell in grid)
            {
                int percentage = 100;
                percentage -= Stepper.Instance().CalculatePercentageBasedOnDistance(enemyUnit.CurrentCell, cell);
                percentage -= Stepper.Instance().CalculatePercentageBasedOnCover(enemyUnit.CurrentCell, cell);
                if (percentage < 0)
                {
                    percentage = 0;
                }
                cell.Value -= percentage;
            }
        }

        foreach (Unit enemyUnit in Stepper.Instance().GetEnemyPlayer().Units)
        {
            foreach (Cell cell in grid)
            {
                if (cell.GetGridX() == 17 && cell.GetGridY() == 17)
                {
                    int a = 0;
                }
                int percentage = 100;
                percentage -= Stepper.Instance().CalculatePercentageBasedOnDistance(cell, enemyUnit.CurrentCell);
                percentage -= Stepper.Instance().CalculatePercentageBasedOnCover(cell, enemyUnit.CurrentCell);
                if (percentage < 0)
                {
                    percentage = 0;
                }
                cell.Value += percentage;
                if (cell.Value > MaxCellValue)
                {
                    MaxCellValue = cell.Value;
                }
            }
        }
    }

    public static void ResetCellsValue()
    {
        foreach (Cell cell in grid)
        {
            cell.Value = 0;
        }
    }

    public static void ResetCellsColor()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                //grid[i,j].ChangeColor(color.FromArgb(1, 1, 1, 1));
                grid[i, j].ChangeColor(UnityEngine.Color.clear);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddPlayer()
    {
        var player1 = new Player { Name = "Blue", PlayerColor = Color.blue };
        Stepper.Instance().Players.Add(player1);
        Stepper.Instance()._currentPlayerId = player1.Id;
    }

    [Command(requiresAuthority = false)]
    public void CmdAddEnemyPlayer()
    {
        var player2 = new Player { Name = "Red", PlayerColor = Color.red };
        Stepper.Instance().Players.Add(player2);
    }

    [Command(requiresAuthority = false)]
    public void CmdAddUnits()
    {
        Stepper.Instance().Players.ForEach(p =>
        {
            if (p.PlayerColor.Equals(Color.blue))
            {
                foreach (Vector2Int cellCoord in playerUnitPositions)
                {
                    if (cellCoord.x >= 0 && cellCoord.x < gridSizeX && cellCoord.y >= 0 && cellCoord.y < gridSizeY)
                    {
                        Cell cellForUnit = grid[cellCoord.x, cellCoord.y];
                        Unit unit = new Unit(false, true);
                        if (p.Units.Count == 0)
                        {
                            unit = new Grenadier(false, true);
                        }
                        if (p.Units.Count == 1)
                        {
                            unit = new Sniper(false, true);
                        }
                        if (p.Units.Count == 2)
                        {
                            unit = new Shotgunner(false, true);
                        }
                        unit.Owner = p;
                        p.Units.Add(unit);
                        //unit.PlaceUnit(cellForUnit);
                        PlaceUnit(unit, cellForUnit);
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
                        Unit unit = new Unit(true, true);
                        if (GameObject.FindWithTag("PlayerVSAi") != null)
                        {
                            unit = new Unit(true, true);
                        }
                        else if (p.Units.Count == 0)
                        {
                            unit = new Grenadier(true, true);
                        }
                        else if (p.Units.Count == 1)
                        {
                            unit = new Sniper(true, true);
                        }
                        else if (p.Units.Count == 2)
                        {
                            unit = new Shotgunner(true, true);
                        }
                        unit.Owner = p;
                        p.Units.Add(unit);
                        //unit.PlaceUnit(cellForUnit);
                        PlaceUnit(unit, cellForUnit);
                    }
                }
            }
        });
    }

    public void PlaceUnit(Unit unit, Cell cellForUnit)
    {
        //this.Unit = unit;
        unit.CurrentCell = cellForUnit;
        System.String prefabString = unit.PrefabString;
        float rotation = unit.IsEnemyUnit ? 180f : 0f;
        GameObject unitSprite = Instantiate(Resources.Load<GameObject>(prefabString), new Vector3(cellForUnit.GetWorldX(), 0.05f, cellForUnit.GetWorldY()), new Quaternion(0, rotation, 0, 0), null);
        //UnitView unitView = unitSprite.AddComponent(typeof(UnitView)) as UnitView;
        UnitView unitView = unitSprite.GetComponent<UnitView>();
        //unitView.InitUnit();
        unitView.setupUnit(unit);
        NetworkServer.Spawn(unitSprite);
        //InitUnit();
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
                Color otherColor = new Color(1, 1-(cell.Value / MaxCellValue), 1-(cell.Value / MaxCellValue));
                Gizmos.color = (cell.Traceable) ? Color.blue : (cell.Walkable) ? otherColor : Color.black;
                Gizmos.DrawCube(cell.WorldPosition, Vector3.one * (cellDiameter - cellDiameter / 2));
                Handles.Label(cell.WorldPosition + new Vector3(0, 2, 0), (cell.Value.ToString()));

                Gizmos.color = Color.white;
                if(cell.GetGridX()==0)
                {
                    Gizmos.color = cell.Cover[Direction.West] ? Color.black : Color.white;
                    Gizmos.DrawCube(cell.WorldPosition + new Vector3(-cellDiameter / 2, 0, 0), new Vector3(0.1f, cellDiameter / 2, cellDiameter));
                }
                if(cell.GetGridY()==0)
                {
                    Gizmos.color = cell.Cover[Direction.South] ? Color.black : Color.white;
                    Gizmos.DrawCube(cell.WorldPosition + new Vector3(0, 0, -cellDiameter / 2), new Vector3(cellDiameter, cellDiameter / 2, 0.1f));
                }

                Gizmos.color = cell.Cover[Direction.East] ? Color.black : Color.white;
                Gizmos.DrawCube(cell.WorldPosition + new Vector3(cellDiameter / 2, 0, 0), new Vector3(0.1f, cellDiameter / 2, cellDiameter));
                
                Gizmos.color = cell.Cover[Direction.North] ? Color.black : Color.white;
                Gizmos.DrawCube(cell.WorldPosition + new Vector3(0, 0, cellDiameter / 2), new Vector3(cellDiameter, cellDiameter / 2, 0.1f));
            }
        }
    }
}
