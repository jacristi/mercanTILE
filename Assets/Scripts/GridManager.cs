using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;


public class GridManager : MonoBehaviour
{

    [Header("Tile Details")]
    [SerializeField] private Tile blankTile;
    [SerializeField] private Tile[] tilePrefabs;
    [SerializeField] private Tilemap groundTileMap;

    [Header("Grid Details")]
    [SerializeField] private float seed;
    [SerializeField] private bool useDynamicSeed;
    [SerializeField] private int width, height;
    [SerializeField] private int smoothingMinThreshold;
    private Tile[,] tileGrid;
    private Tile selectedTile;

    [Header("Forest Gen")]
    [Range(0, 100)]
    [SerializeField] private int forestFillPercent;
    [SerializeField] private bool useForestSmoothing;
    [SerializeField] private bool generateForest;

    [Header("Mountain Gen")]
    [Range(0, 100)]
    [SerializeField] private int mountainFillPercent;
    [SerializeField] private bool useMountainSmoothing;
    [SerializeField] private bool generateMountain;

    [Header("River Gen")]
    [Range(0, 100)]
    [SerializeField] private int riverCurvyness;
    [SerializeField] private int riverStartX;
    [SerializeField] private int maxRiverPathChange;
    [SerializeField] private bool generateRiver;

    [Header("Road Gen")]
    [SerializeField] private bool generateRoad;
    [SerializeField] private int roadHeight;
    [SerializeField] private int roadDeviation;
    [SerializeField] private int maxRoadLegLength;

    [SerializeField] Transform cam;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI selectedItemText;

    private void Start()
    {
        GenerateGrid();
        RenderMap();
    }

    private void Update() {
        if (Input.GetKeyDown("space"))
            ReloadGrid();
        if (Input.GetMouseButtonDown(0))
            {
                if (selectedTile != null)
                    selectedTile.Deselect();

                selectedTile = GetTileAtPosition(MouseExtensions.GetMouseWorldPosition());

                if (selectedTile != null)
                {
                    selectedTile.Select();
                    selectedItemText.text = $"{selectedTile.GetTileTypeVerbose()} - {selectedTile.gridPos}";
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

        for (int nX = gridX-1; nX <= gridX + 1; nX++) {
            for (int nY = gridY-1; nY <= gridY + 1; nY++) {
                bool isInBounds = (nX >= 0 && nX<width && nY >= 0 && nY < height);
                // bool isNotThisTile = (nX != gridX && nY != gridY);

                if (isInBounds) { //} && isNotThisTile) {
                    if (tileGrid[nX,nY] == tileToCheck)
                        tileCount++;
                }
            }
        }
        return tileCount;
    }

    private void SmoothTiles(Tile tileToSet, bool noStandAloneTiles, int smoothMin = 0)
    {
        smoothMin = smoothMin == 0 ? smoothingMinThreshold : smoothMin;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (tileGrid[x,y] != blankTile)
                {
                    int surroundingTileCount = GetSurroundingTileCount(x, y, tileToSet);

                    if (surroundingTileCount >= smoothMin) {
                        tileGrid[x,y] = tileToSet;
                    } else if (tileGrid[x,y] == tileToSet && surroundingTileCount == 1 && noStandAloneTiles == true) {
                        tileGrid[x,y] = tilePrefabs[(int)Tile.TileType.Grassland];
                    }
                }
            }
        }
    }

    private void GenerateGrid()
    {
        if (useDynamicSeed)
            seed = Time.time;

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        tileGrid = new Tile[width, height];

        // Generate Grassland base map
        GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Grassland], 100, false);

        if (generateForest)
            GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Forest], forestFillPercent, useForestSmoothing, false);

        if (generateMountain)
            GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Mountain], mountainFillPercent, useMountainSmoothing, true);

        if (generateRiver)
            GenerateRiver(pseudoRandom, tilePrefabs[(int)Tile.TileType.Water]);

        if (generateRoad)
            GenerateRoads(pseudoRandom, tilePrefabs[(int)Tile.TileType.Road]);
    }

    private void GenerateTerrain(System.Random pseudoRandom, Tile tileToPlace, float fillPercent, bool applySmoothing, bool noStandAloneTiles=false)
    {
        // Populate grid with mountain tiles
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                tileGrid[x,y] = (pseudoRandom.Next(0, 100) < fillPercent) ? tileToPlace : tileGrid[x,y];
            }
        }

        // Apply smoothing if desired
        if (applySmoothing)
            SmoothTiles(tileToPlace, noStandAloneTiles);
    }

    private void GenerateRiver(System.Random pseudoRandom, Tile waterTile)
    {
        int x = Mathf.Clamp(riverStartX, 0, width-1);
        tileGrid[x, 0] = waterTile;

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
    }

    private void GenerateRoads(System.Random pseudoRandom, Tile roadTile)
    {

        int diameter = (int)Mathf.Floor(height * 0.65f);
        int radius = diameter /2;
        int centerX = width/2;
        int centerY = height/2;

        int leftX = centerX - radius + Random.Range(-2, 2);
        int rightX = centerX + radius + Random.Range(-2, 2);

        int maxRoadHeight = Mathf.Clamp(roadHeight, 0, height - (centerY + roadDeviation + 2));
        //centerY + deviation + maxRoadHeight cannot be larger than height
        // if (centerY + roadDeviation + maxRoadHeight + 2 > height)
        //     maxRoadHeight = height - (centerY + roadDeviation + 2)

        GenerateRoadTopLeg(pseudoRandom, roadTile, leftX, rightX, centerY, maxRoadHeight);
        GenerateRoadBottomLeg(pseudoRandom, roadTile, leftX, rightX, centerY, maxRoadHeight);

        // Generate simple left/right connection legs
        for (int y = centerY-maxRoadHeight; y <= centerY+maxRoadHeight;y++) {
            y = Mathf.Clamp(y, 0, height-1);
            tileGrid[leftX, y] = roadTile;
            tileGrid[rightX, y] = roadTile;
        }
    }

    void GenerateRoadTopLeg(System.Random pseudoRandom, Tile roadTile, int leftX, int rightX, int centerY, int maxRoadHeight)
    {
        int targetX = rightX;
        int targetY = centerY+maxRoadHeight;

        int x = Mathf.Clamp(leftX, 0, width-1);
        int y = Mathf.Clamp(targetY, 0, height-1);

        tileGrid[x, y] = roadTile;

        for (int i=0; i < maxRoadLegLength; i++)
        {
            var pickChance = pseudoRandom.Next(0, 100);
            bool canGoUp = (y < targetY + roadDeviation) && (tileGrid[x-1, y+1] != roadTile && tileGrid[x+1, y+1] != roadTile && tileGrid[x, y+1] != roadTile && x+1 != targetX) && !(x+2 == targetX && y-2 == targetY) && !(x == targetX && y > targetY );
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

            x = Mathf.Clamp(x, 0, width-1);
            y = Mathf.Clamp(y, 0, height-1);

            tileGrid[x, y] = roadTile;

            if (x == targetX && y == targetY) {
                break;
            }
        }
    }

    void GenerateRoadBottomLeg(System.Random pseudoRandom, Tile roadTile, int leftX, int rightX, int centerY, int maxRoadHeight)
    {
        int targetX = leftX;
        int targetY = centerY-maxRoadHeight;

        int x = Mathf.Clamp(rightX, 0, width-1);
        int y = Mathf.Clamp(targetY, 0, height-1);

        tileGrid[x, y] = roadTile;

        for (int i=0; i < maxRoadLegLength; i++) {
            var pickChance = pseudoRandom.Next(0, 100);

            bool canGoUp = (y < targetY) && (tileGrid[x-1, y+1] != roadTile && tileGrid[x+1, y+1] != roadTile && tileGrid[x, y+1] != roadTile) && (x+1 != targetX) && !(x-2 == targetX && y-2 == targetY);
            bool canGoDown = (y > targetY-roadDeviation) && (tileGrid[x-1, y-1] != roadTile && tileGrid[x+1, y-1] != roadTile && tileGrid[x, y-1] != roadTile) && (x-1 != targetX) && !(x-2 == targetX && y+2 == targetY) && !(x==targetX && y < targetY);
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

            x = Mathf.Clamp(x, 0, width-1);
            y = Mathf.Clamp(y, 0, height-1);

            tileGrid[x, y] = roadTile;

            if (x == targetX && y == targetY) {
                break;
            }
        }
    }


    private void GenerateRoadsCircle(System.Random pseudoRandom, Tile roadTile)
    {
        int diameter = (int)Mathf.Floor(height * 0.65f);
        int radius = diameter /2;
        int centerX = width/2;
        int centerY = height/2;

        for (int x = centerX - radius ; x <= centerX; x++)
        {
            for (int y = centerY - radius ; y <= centerY; y++)
            {
                bool isOutlineOuter = ((x - centerX)*(x - centerX) + (y - centerY)*(y - centerY) > radius*radius-radius-radius/2);
                bool isOutlineInner = ((x - centerX)*(x - centerX) + (y - centerY)*(y - centerY) < radius*radius+radius+radius/2);

                if (isOutlineOuter && isOutlineInner)
                {
                    // Get values reflected on the x/y axis
                    var xRef = centerX + Mathf.Abs(centerX-x);
                    var yRef = centerY + Mathf.Abs(centerY-y);

                    tileGrid[x,y] = roadTile;
                    tileGrid[xRef,y] = roadTile;
                    tileGrid[x,yRef] = roadTile;
                    tileGrid[xRef,yRef] = roadTile;
                }
            }
        }
        SmoothTiles(roadTile, false, 6);
        SmoothTiles(roadTile, false, 5);

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
    }
}
