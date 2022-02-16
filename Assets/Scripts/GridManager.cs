using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{

    [Header("Tile Details")]
    [SerializeField] private Tile blankTile;
    [SerializeField] private Tile[] tilePrefabs;
    [SerializeField] private Tilemap groundTileMap;

    [Header("Grid Details")]
    [SerializeField] private float seed;
    [SerializeField] private int width, height;
    [SerializeField] private float tileSize;
    [SerializeField] private int smoothingMinThreshold;

    [Header("Forest Gen")]
    [Range(0, 100)]
    [SerializeField] private int forestFillPercent;
    [SerializeField] private bool useForestSmoothing;


    [SerializeField] Transform cam;

    private Dictionary<Vector2, Tile> tilesDict;

    private Tile[,] tileArray;

    private void Start()
    {
        GenerateGrid();
        if (useForestSmoothing)
            SmoothTiles(tilePrefabs[(int)Tile.TileType.Forest]);
        RenderMap();
    }

    private void Update() {
        if (Input.GetKeyDown("space"))
            ReloadGrid();
    }

    public void ReloadGrid()
    {
        groundTileMap.ClearAllTiles();
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }
        GenerateGrid();
        if (useForestSmoothing)
            SmoothTiles(tilePrefabs[(int)Tile.TileType.Forest]);
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
                bool isNotThisTile = (nX != gridX && nY != gridY);
                bool isTileToCheck = (tileArray[nX,nY] == tileToCheck);
                if (isInBounds && isNotThisTile && isTileToCheck) {
                    if (tileArray[nX,nY] == tileToCheck)
                        tileCount++;
                }
            }
        }
        return tileCount;
    }

    private void SmoothTiles(Tile tileToSet)
    {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (tileArray[x,y] != blankTile)
                {
                    int surroundingTileCount = GetSurroundingTileCount(x, y, tileToSet);

                    if (surroundingTileCount >= smoothingMinThreshold) {
                        tileArray[x,y] = tileToSet;
                    }
                }
            }
        }
    }

    private void GenerateGrid()
    {
        // seed = Time.time;
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        tileArray = new Tile[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Tile tileToSpawn = blankTile;
                if (!(x == 0 || x == width-1 || y <= 1 || y >= height-2)) {
                    // tileToSpawn = tilePrefabs[(int)Tile.TileType.Grassland];
                    tileToSpawn = (pseudoRandom.Next(0, 100) < forestFillPercent) ? tilePrefabs[(int)Tile.TileType.Forest] : tilePrefabs[(int)Tile.TileType.Grassland];
                }
                tileArray[x,y] = tileToSpawn;
            }
        }
    }

    private void GenerateForest() {}

    private void GenerateMountains() {}

    private void GenerateWater() {}

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
                groundTileMap.SetTile(new Vector3Int(x, y, 0), thisTile.ruleTile);
            }
        }

        cam.transform.position = new Vector3((float)width * 0.5f, (float)height * 0.5f, -10);
    }

    private Tile GetTileAtPosition(Vector2 pos)
    {
        if (tilesDict.TryGetValue(pos, out var tile)) {
            return tile;
        } else {
            return null;
        }
    }
}
