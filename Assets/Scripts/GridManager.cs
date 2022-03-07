using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera cam;

    [Header("Tile Details")]
    [SerializeField] private Tile blankTile;
    [SerializeField] private Tile[] tilePrefabs;
    [SerializeField] private Tilemap groundTileMap;

    [Header("Grid Details")]
    [SerializeField] private float seed;
    [SerializeField] private bool useDynamicSeed;
    [SerializeField] private int width, height;
    [SerializeField] private bool generateForest;
    [SerializeField] private bool generateMountain;
    [SerializeField] private bool generateRiver;
    [SerializeField] private bool generateRoad;
    [SerializeField] private bool checkOnlyImmediateNeighbors;

    private Tile[,] tileGrid;
    private Tile selectedTile;

    [Header("Forest Gen")]
    [Range(0, 100)]
    [SerializeField] private int forestFillPercent;
    [Range(0, 100)]
    [SerializeField] private int singleForestTilePercent;
    [SerializeField] private int forestSmoothingMin;
    [SerializeField] private int forestSmoothingTimes;

    [Header("Mountain Gen")]
    [Range(0, 100)]
    [SerializeField] private int mountainFillPercent;
    [Range(0, 100)]
    [SerializeField] private int singleMountainTilePercent;
    [SerializeField] private int mountainSmoothingMin;
    [SerializeField] private int mountainSmoothingTimes;

    [Header("River Gen")]
    [Range(0, 100)]
    [SerializeField] private int riverCurvyness;
    [SerializeField] private int riverStartX;
    [SerializeField] private int maxRiverPathChange;


    [Header("Road Gen")]
    [SerializeField] private int roadHeight;
    [SerializeField] private int roadWidth;
    [SerializeField] private int roadDeviation;
    [SerializeField] private int maxRoadLegLength;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI selectedItemText;
    [SerializeField] private float zoomFactor;

    private void Start()
    {
        // zoomFactor = cam.orthographicSize;
        // GenerateGrid();
        // RenderMap();
    }

    private void Update() {

        // if (Keyboard.current.spaceKey.wasPressedThisFrame)
        //     ReloadGrid();
        if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (selectedTile != null)
                    selectedTile.Deselect();

                selectedTile = GetTileAtPosition(Mouse.current.GetWorldPosition(cam)); // GetWorldPosition

                if (selectedTile != null)
                {
                    selectedTile.Select();
                    selectedItemText.text = $"{selectedTile.GetTileTypeVerbose()} - {selectedTile.gridPos}";
                } else {
                    selectedItemText.text = "";
                }
            }

    }

    private Tile GetTileAtPosition(Vector3 mousePos) {
        int x = (int)Mathf.Floor(mousePos.x);
        int y = (int)Mathf.Floor(mousePos.y);

        if ((x >= 0 && x < width) && y >= 0 && y < height)
            return tileGrid[x,y];

        return null;
    }

    public void NewGrid(int newWidth, int newHeight, int forestAmount, int mountainAmountPercent, int riverX, int roadW, int roadH, int roadDev) {
        width = newWidth;
        height = newHeight;
        forestFillPercent = forestAmount;
        mountainFillPercent = mountainAmountPercent;
        if (riverX == -1)
            generateRiver = false;
        else
            generateRiver = true;
        riverStartX = riverX;
        roadWidth = roadW;
        roadHeight = roadH;
        if (roadW == -1 || roadH == -1)
            generateRoad = false;
        else
            generateRoad = true;
        roadDeviation = roadDev;
        ReloadGrid();
    }

    public void ReloadGrid()
    {
        groundTileMap.ClearAllTiles();
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }
        GenerateGrid();
        RenderMap();
    }

    public Tile PickRandomTile() {
        return tilePrefabs[Random.Range(0, tilePrefabs.Length)]; // if it's an array
    }

    private int GetSurroundingTileCount(int gridX, int gridY, Tile tileToCheck) {

        int tileCount = 0;
        if (checkOnlyImmediateNeighbors) {

            List<(int, int)> checkpoints = new List<(int, int)>();
            checkpoints.Add((gridX, gridY+1));
            checkpoints.Add((gridX-1, gridY));
            checkpoints.Add((gridX+1, gridY));
            checkpoints.Add((gridX, gridY-1));

            foreach ((int, int) checkPoint in checkpoints) {
                int nX = checkPoint.Item1;
                int nY = checkPoint.Item2;
                if (nX >= 0 && nX<width && nY >= 0 && nY < height && tileGrid[nX,nY] == tileToCheck) {
                    tileCount++;
                }
            }
        } else {

            for (int nX = gridX-1; nX <= gridX + 1; nX++) {
                for (int nY = gridY-1; nY <= gridY + 1; nY++) {
                    bool isInBounds = (nX >= 0 && nX<width && nY >= 0 && nY < height);
                    bool isNotThisTile = (nX != gridX && nY != gridY);

                    if (isInBounds && isNotThisTile) {
                        if (tileGrid[nX,nY] == tileToCheck)
                            tileCount++;
                    }
                }
            }
        }
        return tileCount;
    }

    private void SmoothTiles(Tile tileToSet, System.Random pseudoRandom, int singleTilePercent, int smoothMin = 0)
    {

        List<(int, int)> tilesToSmooth = new List<(int, int)>();
        List<(int, int)> tilesToRemove = new List<(int, int)>();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (tileGrid[x,y] != blankTile)
                {
                    int surroundingTileCount = GetSurroundingTileCount(x, y, tileToSet);

                    if (surroundingTileCount >= smoothMin) {
                        tilesToSmooth.Add((x,y));
                    } else if (tileGrid[x,y] == tileToSet && surroundingTileCount == 0) {
                        if (pseudoRandom.Next(0, 100) > singleTilePercent)
                            tilesToRemove.Add((x,y));
                    }
                }
            }
        }

        foreach ((int, int) i in tilesToSmooth) {
            tileGrid[i.Item1, i.Item2] = tileToSet;
        }
        foreach ((int, int) i in tilesToRemove) {
            tileGrid[i.Item1, i.Item2] = tilePrefabs[(int)Tile.TileType.Grassland];
        }
    }

    private void GenerateGrid()
    {
        if (useDynamicSeed)
            seed = Time.time;

        maxRoadLegLength = (width * height)/4;

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        tileGrid = new Tile[width, height];

        // Generate Grassland base map
        GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Grassland], 100, 0, 100);

        if (generateForest)
            GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Forest], forestFillPercent, forestSmoothingTimes, singleForestTilePercent, forestSmoothingMin);

        if (generateMountain)
            GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Mountain], mountainFillPercent, mountainSmoothingTimes, singleMountainTilePercent, mountainSmoothingMin);

        if (generateRiver)
            GenerateRiver(pseudoRandom, tilePrefabs[(int)Tile.TileType.Water]);

        if (generateRoad)
            GenerateRoads(pseudoRandom, tilePrefabs[(int)Tile.TileType.Road]);
    }

    private void GenerateTerrain(System.Random pseudoRandom, Tile tileToPlace, float fillPercent, int applySmoothingTimes, int singleTilePercent, int smoothingMin = 0)
    {
        // Populate grid with mountain tiles
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                tileGrid[x,y] = (pseudoRandom.Next(0, 100) < fillPercent) ? tileToPlace : tileGrid[x,y];
            }
        }

        // Apply smoothing i times
        for (int i = 0; i < applySmoothingTimes; i++) {
            SmoothTiles(tileToPlace, pseudoRandom, singleTilePercent, smoothingMin);
        }
    }

    private void GenerateRiver(System.Random pseudoRandom, Tile waterTile)
    {
        int x = Mathf.Clamp(riverStartX, 0, width-1);
        tileGrid[x, 0] = waterTile;
        groundTileMap.SetTile(new Vector3Int(x, -1, 0), waterTile.ruleTile);
        for (int y = 1; y < height-1; y++) {

            for (int i = 0; i < maxRiverPathChange; i++) {
                int nX;
                if (pseudoRandom.Next(0, 100) < 50)
                    nX = Mathf.Clamp(x+i, 0, width-1);
                else
                    nX = Mathf.Clamp(x-i, 0, width-1);

                tileGrid[nX, y] = waterTile;

                if (pseudoRandom.Next(0, 100) > riverCurvyness)
                    break;
                else
                    x = nX;

                }
            }
        tileGrid[x, height-1] = waterTile;
        groundTileMap.SetTile(new Vector3Int(x, height, 0), waterTile.ruleTile);
    }

    private void GenerateRoads(System.Random pseudoRandom, Tile roadTile)
    {
        int centerX = width/2;
        int centerY = height/2;
        int radius = Mathf.Clamp(roadWidth, 2, width - centerX - 4);

        int maxRoadDeviation = Mathf.Clamp(roadDeviation, 0, height/2 - 3);
        int maxRoadHeight = Mathf.Clamp(roadHeight, 1, height - (centerY + maxRoadDeviation + 2));

        int leftX = Mathf.Min(centerX - radius + Random.Range(-2, 2), centerX-2);
        int rightX = Mathf.Max(centerX + radius + Random.Range(-2, 2), centerX+2);
        int topY = centerY + maxRoadHeight;
        int bottomY = centerY - maxRoadHeight;

        GeneratePathLeftRight(pseudoRandom, roadTile, leftX, rightX, centerY, maxRoadHeight, maxRoadDeviation);
        GeneratePathRightLeft(pseudoRandom, roadTile, leftX, rightX, centerY, maxRoadHeight, maxRoadDeviation);
        GeneratePathDownUp(pseudoRandom, roadTile, bottomY, topY, leftX, maxRoadHeight, maxRoadDeviation);
        GeneratePathUpDown(pseudoRandom, roadTile, bottomY, topY, rightX, maxRoadHeight, maxRoadDeviation);

        // Generate simple left/right connection legs
        // for (int y = centerY-maxRoadHeight; y <= centerY+maxRoadHeight;y++) {
        //     y = Mathf.Clamp(y, 0, height-1);
        //     tileGrid[leftX, y] = roadTile;
        //     tileGrid[rightX, y] = roadTile;
        // }
    }

    void GeneratePathLeftRight(System.Random pseudoRandom, Tile roadTile, int leftX, int rightX, int centerY, int maxRoadHeight, int maxRoadDeviation)
    {
        int targetX = rightX;
        int targetY = centerY+maxRoadHeight;

        int x = Mathf.Clamp(leftX, 1, width-2);
        int y = Mathf.Clamp(targetY, 1, height-2);

        tileGrid[x, y] = roadTile;

        for (int i=0; i < maxRoadLegLength; i++)
        {
            var pickChance = pseudoRandom.Next(0, 100);
            bool canGoUp = (y < targetY + maxRoadDeviation) && (tileGrid[x-1, y+1] != roadTile && tileGrid[x+1, y+1] != roadTile && tileGrid[x, y+1] != roadTile && x+1 != targetX) && !(x+2 == targetX && y-2 == targetY) && !(x == targetX && y > targetY );
            bool canGoDown = (y > targetY) && (tileGrid[x-1, y-1] != roadTile && tileGrid[x+1, y-1] != roadTile && tileGrid[x, y-1] != roadTile && x+1 != targetX) && !(x+2 == targetX && y+2 == targetY);
            bool canGoRight = (x < targetX);

            if (canGoRight && pickChance > 66) {
                x++;
            } else if ((canGoDown && canGoUp) && pickChance > 66) {
                y++;
            } else if (canGoDown) {
                y--;
            } else if (canGoUp) {
                y++;
            } else if (canGoRight) {
                x++;
            }

            x = Mathf.Clamp(x, 1, width-2);
            y = Mathf.Clamp(y, 1, height-2);

            tileGrid[x, y] = roadTile;

            if (x == targetX && y == targetY) {
                break;
            }
        }
    }

    void GeneratePathRightLeft(System.Random pseudoRandom, Tile roadTile, int leftX, int rightX, int centerY, int maxRoadHeight, int maxRoadDeviation)
    {
        int targetX = leftX;
        int targetY = centerY-maxRoadHeight;

        int x = Mathf.Clamp(rightX, 1, width-2);
        int y = Mathf.Clamp(targetY, 1, height-2);

        tileGrid[x, y] = roadTile;

        for (int i=0; i < maxRoadLegLength; i++)
        {
            var pickChance = pseudoRandom.Next(0, 100);
            bool canGoUp = (y < targetY) && (tileGrid[x-1, y+1] != roadTile && tileGrid[x+1, y+1] != roadTile && tileGrid[x, y+1] != roadTile) && (x+1 != targetX) && !(x-2 == targetX && y-2 == targetY);
            bool canGoDown = (y > targetY-maxRoadDeviation) && (tileGrid[x-1, y-1] != roadTile && tileGrid[x+1, y-1] != roadTile && tileGrid[x, y-1] != roadTile) && (x-1 != targetX) && !(x-2 == targetX && y+2 == targetY) && !(x==targetX && y < targetY);
            bool canGoLeft = (x > targetX);

            if (canGoLeft && pickChance > 66) {
                x--;
            } else if ((canGoDown && canGoUp) && pickChance > 66) {
                y--;
            } else if (canGoUp) {
                y++;
            } else if (canGoDown) {
                y--;
            } else if (canGoLeft) {
                x--;
            }

            x = Mathf.Clamp(x, 1, width-2);
            y = Mathf.Clamp(y, 1, height-2);

            tileGrid[x, y] = roadTile;

            if (x == targetX && y == targetY) {
                break;
            }
        }
    }

    private void GenerateTilesCircle(System.Random pseudoRandom, Tile tileToSet)
    {
        int diameter = (int)Mathf.Floor(height * 0.65f);
        int radius = diameter /2;
        int centerX = width/2;
        int centerY = height/2;

        for (int x = centerX - radius ; x <= centerX; x++)
        {
            for (int y = centerY - radius ; y <= centerY; y++)
            {
                bool isOutlineInner = ((x - centerX)*(x - centerX) + (y - centerY)*(y - centerY) < radius*radius+radius+radius/2);
                // bool isOutlineOuter = ((x - centerX)*(x - centerX) + (y - centerY)*(y - centerY) > radius*radius-radius-radius/2);

                if (isOutlineInner)
                {
                    // Get values reflected on the x/y axis
                    var xRef = centerX + Mathf.Abs(centerX-x);
                    var yRef = centerY + Mathf.Abs(centerY-y);

                    tileGrid[x,y] = tileToSet;
                    tileGrid[xRef,y] = tileToSet;
                    tileGrid[x,yRef] = tileToSet;
                    tileGrid[xRef,yRef] = tileToSet;
                }
            }
        }

    }

    private void GeneratePlaces() {
        // Take in a grid/2d array and populate places (starting village, starting bandit camps, etc.)
        // Target village should be opposite from starting village
        // bandit camps should be between start and target villages
        // places replace tile with grassland
        // Return new places grid
    }

    void RenderMap()
    {
        for (int x=0; x<tileGrid.GetLength(0); x++) {
            for (int y=0; y<tileGrid.GetLength(1); y++) {
                Tile thisTile = tileGrid[x, y];
                var tileObj = Instantiate(thisTile, new Vector3(x+.5f, y+.5f), Quaternion.identity);
                tileObj.transform.parent = this.transform;
                tileObj.name = $"{tileObj.GetTileTypeVerbose()} {x},{y}";
                tileObj.gridPos = (x, y);
                tileGrid[x,y] = tileObj;
                groundTileMap.SetTile(new Vector3Int(x, y, 0), thisTile.ruleTile);
            }
        }
        cam.transform.position = new Vector3((float)width * 0.5f, (float)height * 0.5f, -10);
        cam.GetComponent<CameraController>().mapSize = new (width, height);
    }

    private void GeneratePathDownUp(System.Random pseudoRandom, Tile roadTile, int bottomY, int topY, int leftX, int maxRoadHeight, int maxRoadDeviation)
    {
        int targetX = leftX;
        int targetY = topY;

        int x = Mathf.Clamp(targetX, 1, width-2);
        int y = Mathf.Clamp(bottomY, 1, height-2);

        tileGrid[x, y] = roadTile;

        for (int i=0; i < maxRoadLegLength; i++)
        {
            var pickChance = pseudoRandom.Next(0, 100);

            bool xNotPastMaxDev = (x > targetX-maxRoadDeviation);
            bool noAdjRoadsToLeft = (tileGrid[x-1, y-1] != roadTile && tileGrid[x-1, y+1] != roadTile && tileGrid[x-1, y] != roadTile);
            bool targetNot1SpaceAbove = (y+1 != targetY);
            bool rightOfTargetY = (y == targetY && x > targetX);

            bool xLeftOfTargetX = (x < targetX);
            bool noAdjRoadsToRight = (tileGrid[x+1, y-1] != roadTile && tileGrid[x+1, y+1] != roadTile && tileGrid[x+1, y] != roadTile);
            bool leftOfTargetY = (y == targetY && x < targetX);

            bool canGoLeft = xNotPastMaxDev && noAdjRoadsToLeft && targetNot1SpaceAbove && !leftOfTargetY;
            bool canGoRight = xLeftOfTargetX && noAdjRoadsToRight && targetNot1SpaceAbove && !rightOfTargetY;
            bool canGoUp = (y < targetY);

            if (canGoUp && pickChance > 66) {
                y++;
            } else if ((canGoRight && canGoLeft) && pickChance > 66) {
                x--;
            } else if (canGoRight) {
                x++;
            } else if (canGoLeft) {
                x--;
            } else if (canGoUp) {
                y++;
            }

            x = Mathf.Clamp(x, 1, width-2);
            y = Mathf.Clamp(y, 1, height-2);

            tileGrid[x, y] = roadTile;

            if (x == targetX && y == targetY) {
                break;
            }
        }
    }

    private void GeneratePathUpDown(System.Random pseudoRandom, Tile roadTile, int bottomY, int topY, int rightX, int maxRoadHeight, int maxRoadDeviation)
    {
        int targetX = rightX;
        int targetY = bottomY;

        int x = Mathf.Clamp(targetX, 1, width-2);
        int y = Mathf.Clamp(topY, 1, height-2);

        tileGrid[x, y] = roadTile;

        for (int i=0; i < maxRoadLegLength; i++)
        {
            var pickChance = pseudoRandom.Next(0, 100);

            bool xNotPastMaxDev = (x < targetX+maxRoadDeviation);
            bool noAdjRoadsToLeft = (tileGrid[x-1, y-1] != roadTile && tileGrid[x-1, y+1] != roadTile && tileGrid[x-1, y] != roadTile);
            bool targetNot1SpaceBelow = (y-1 != targetY);
            bool rightOfTargetY = (y == targetY && x > targetX);

            bool xRightOfTargetX = (x > targetX);
            bool noAdjRoadsToRight = (tileGrid[x+1, y-1] != roadTile && tileGrid[x+1, y+1] != roadTile && tileGrid[x+1, y] != roadTile);
            bool leftOfTargetY = (y == targetY && x < targetX);

            bool canGoLeft = xRightOfTargetX && noAdjRoadsToLeft && targetNot1SpaceBelow && !leftOfTargetY;
            bool canGoRight = xNotPastMaxDev && noAdjRoadsToRight && targetNot1SpaceBelow && !rightOfTargetY;
            bool canGoDown = (y > targetY);

            if (canGoDown && pickChance > 66) {
                y--;
            } else if ((canGoRight && canGoLeft) && pickChance > 66) {
                x++;
            } else if (canGoLeft) {
                x--;
            } else if (canGoRight) {
                x++;
            } else if (canGoDown) {
                y--;
            }

            x = Mathf.Clamp(x, 1, width-2);
            y = Mathf.Clamp(y, 1, height-2);

            tileGrid[x, y] = roadTile;

            if (x == targetX && y == targetY) {
                break;
            }
        }
    }
}
