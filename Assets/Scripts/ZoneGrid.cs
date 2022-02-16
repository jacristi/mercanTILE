using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ZoneGrid<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridArray;
    private TextMeshPro[,] debugTextArray;

    public const int HEAT_MAP_MAX_VALUE = 100;
    public const int HEAT_MAP_MIN_VALUE = 0;

    public const int sortingOrderDefault = 5000;
    // Create Text in the World
    public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = sortingOrderDefault) {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAlignment, sortingOrder);
    }

    // Create Text in the World
    public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder) {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    public ZoneGrid(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new TGridObject[width, height];
        debugTextArray = new TextMeshPro[width, height];

        int count = 0;
        for (int x=0; x<gridArray.GetLength(0); x++) {
            for (int y=0; y<gridArray.GetLength(1); y++) {
                // SetValue(x, y, count);
                debugTextArray[x, y] = CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 2, Color.white, TextAlignmentOptions.Center);
                count += 1;
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y+1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x+1, y), Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    private void GetXY(Vector3 worldPosition, out int x , out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
    }

    private Vector3 GetWorldPosition(int x, int y, float cs=1f)
    {
        return new Vector3(x, y) * cs;
    }

    public void SetValue(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            {
                gridArray[x, y] = value;//Mathf.Clamp(value, HEAT_MAP_MIN_VALUE, HEAT_MAP_MAX_VALUE;
                debugTextArray[x, y].text = gridArray[x, y].ToString();
            }
    }

    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public TGridObject GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        } else {
            return default(TGridObject);
        }
    }
    public TGridObject GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x , out y);
        return GetValue(x, y);
    }

}
