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

    public TMP_Dropdown mountainAmountDropdown;
    public int mountainAmount = 2;
    private int mountainAmountPercent;

    public TMP_Dropdown riverStartDropdown;
    public int riverStart = 2;
    public int riverStartX;

    private void Start() {
        mapSizeDropdown.value = mapSize;
        forestAmountDropdown.value = forestAmount;
        mountainAmountDropdown.value = mountainAmount;
        riverStartDropdown.value = riverStart;
        SetMapSize(1);
        SetForestAmount(1);
        SetMountainAmount(1);
        SetRiverStart(1);
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

    public void SetMountainAmount(int value) {
        mountainAmount = mountainAmountDropdown.value;
        switch (mountainAmount)
        {
            case 0:
                mountainAmountPercent = 0;
                break;
            case 1:
                mountainAmountPercent = 5;
                break;
            case 2:
                mountainAmountPercent = 15;
                break;
            case 3:
                mountainAmountPercent = 30;
                break;
            case 4:
                mountainAmountPercent = 50;
                break;
            default:
                mountainAmountPercent = 15;
                break;
        }
    }

    public void SetRiverStart(int value) {
        riverStart = riverStartDropdown.value;
        int mid = mapWidth/2;
        int qrt = mapWidth/4;
        int mult = mapSize + 1;
        switch (riverStart)
        {
            case 0:
                riverStartX = -1;
                break;
            case 1:
                riverStartX = Random.Range(0, 4*mult);
                break;
            case 2:
                riverStartX = Random.Range(mid-(qrt-qrt+4*mult), mid-(qrt+qrt-4*mult));
                break;
            case 3:
                riverStartX = Random.Range(mid-4, mid+4);
                break;
            case 4:
                riverStartX = Random.Range(mid+(qrt-qrt+4*mult), mid+(qrt+qrt-4*mult));
                break;
            default:
                riverStartX = Random.Range(mapWidth-4*mult, mapWidth);
                break;
        }
    }

    public void MakeGrid()
    {
        SetMapSize(1);
        SetForestAmount(1);
        SetMountainAmount(1);
        SetRiverStart(1);
        gridManager.NewGrid(mapWidth, mapHeight, forestAmountPercent, mountainAmountPercent, riverStartX);
    }
}
