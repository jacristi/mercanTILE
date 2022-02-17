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
    [SerializeField] private float tileSize;
    [SerializeField] private int smoothingMinThreshold;
    private Tile[,] tileArray;
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
            return tileArray[x,y];

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
                    if (tileArray[nX,nY] == tileToCheck)
                        tileCount++;
                }
            }
        }
        return tileCount;
    }

    private void SmoothTiles(Tile tileToSet, bool noStandAloneTiles)
    {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (tileArray[x,y] != blankTile)
                {
                    int surroundingTileCount = GetSurroundingTileCount(x, y, tileToSet);

                    if (surroundingTileCount >= smoothingMinThreshold) {
                        tileArray[x,y] = tileToSet;
                    } else if (tileArray[x,y] == tileToSet && surroundingTileCount == 1 && noStandAloneTiles == true) {
                        tileArray[x,y] = tilePrefabs[(int)Tile.TileType.Grassland];
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
        tileArray = new Tile[width, height];

        // Generate Grassland base map
        GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Grassland], 100, false);

        if (generateForest)
            GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Forest], forestFillPercent, useForestSmoothing, false);

        if (generateMountain)
            GenerateTerrain(pseudoRandom, tilePrefabs[(int)Tile.TileType.Mountain], mountainFillPercent, useMountainSmoothing, true);

        if (generateRiver)
            GenerateRiver(pseudoRandom, tilePrefabs[(int)Tile.TileType.Water]);
    }

    private void GenerateTerrain(System.Random pseudoRandom, Tile tileToPlace, float fillPercent, bool applySmoothing, bool noStandAloneTiles=false)
    {
        // Populate grid with mountain tiles
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                // if (!(x == 0 || x == width-1 || y <= 1 || y >= height-2)) {}
                tileArray[x,y] = (pseudoRandom.Next(0, 100) < fillPercent) ? tileToPlace : tileArray[x,y];

            }
        }

        // Apply smoothing if desired
        if (applySmoothing)
            SmoothTiles(tileToPlace, noStandAloneTiles);
    }

    private void GenerateRiver(System.Random pseudoRandom, Tile waterTile)
    {
        int x = Mathf.Clamp(riverStartX, 0, width-1);
        tileArray[x, 0] = waterTile;

        for (int y = 1; y < height-1; y++) {

            for (int i = 0; i < maxRiverPathChange; i++) {
                int nX;
                if (pseudoRandom.Next(0, 100) < 50)
                    nX = Mathf.Clamp(x+i, 0, width-1);
                else
                    nX = Mathf.Clamp(x-i, 0, width-1);

                tileArray[nX, y] = waterTile;

                if (pseudoRandom.Next(0, 100) > riverCurvyness)
                    break;
                else
                    x = nX;

                }
            }
        tileArray[x, height-1] = waterTile;
    }


    private void GenerateRoads() {
        // Take in a grid /2d array and populate road tile locations
        // Return the updated grid
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
        for (int x=0; x<tileArray.GetLength(0); x++) {
            for (int y=0; y<tileArray.GetLength(1); y++) {
                Tile thisTile = tileArray[x, y];
                var tileObj = Instantiate(thisTile, new Vector3(x+.5f, y+.5f), Quaternion.identity);
                tileObj.transform.parent = this.transform;
                tileObj.name = $"{tileObj.GetTileTypeVerbose()} {x},{y}";
                tileObj.gridPos = (x, y);
                tileArray[x,y] = tileObj;
                groundTileMap.SetTile(new Vector3Int(x, y, 0), thisTile.ruleTile);
            }
        }

        cam.transform.position = new Vector3((float)width * 0.5f, (float)height * 0.5f, -10);
    }
}
