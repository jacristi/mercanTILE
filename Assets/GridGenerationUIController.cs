using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridGenerationUIController : MonoBehaviour
{

    public GridManager gridManager;

    public TMP_Dropdown mapSizeDropdown;
    public int mapSize = 1;
    private int mapWidth;
    private int mapHeight;

    public TMP_Dropdown forestAmountDropdown;
    public int forestAmount = 2;
    private int forestAmountPercent;

    private void Start() {
        mapSizeDropdown.value = mapSize;
        forestAmountDropdown.value = forestAmount;
        SetMapSize(1);
        SetForestAmount(1);
        MakeGrid();
    }

    public void SetMapSize(int value) {
        mapSize = mapSizeDropdown.value;
        switch (mapSize)
        {
            case 0:
                mapWidth = 24;
                mapHeight = 18;
                break;
            case 1:
                mapWidth = 32;
                mapHeight = 24;
                break;
            case 2:
                mapWidth = 48;
                mapHeight = 36;
                break;
            case 3:
                mapWidth = 60;
                mapHeight = 45;
                break;
            case 4:
                mapWidth = 72;
                mapHeight = 54;
                break;
            case 5:
                mapWidth = 100;
                mapHeight = 75;
                break;
            case 6:
                mapWidth = 200;
                mapHeight = 150;
                break;
            default:
                mapWidth = 48;
                mapHeight = 36;
                break;
        }
    }
    public void SetForestAmount(int value) {
        forestAmount = forestAmountDropdown.value;
        switch (forestAmount)
        {
            case 0:
                forestAmountPercent = 0;
                break;
            case 1:
                forestAmountPercent = 5;
                break;
            case 2:
                forestAmountPercent = 20;
                break;
            case 3:
                forestAmountPercent = 35;
                break;
            case 4:
                forestAmountPercent = 80;
                break;
            default:
                forestAmountPercent = 20;
                break;
        }
    }

    public void MakeGrid() {
        Debug.Log("GO");
        gridManager.NewGrid(mapWidth, mapHeight, forestAmountPercent);
    }
}
