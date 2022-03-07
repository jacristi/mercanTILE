using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(PixelPerfectCamera))]
public class CameraController : MonoBehaviour
{
    private Camera cam;
    private PixelPerfectCamera pixelCamera;

    private Vector3 newPosition;
    private Vector2 refResolution;

    private float nextScrollTime;
    private bool isDragging;
    private Vector3 dragOrigin;

    [Header("Target")]
    [SerializeField] private Transform initialTarget;

    [Header("Bounds")]
    public Vector2Int mapSize = new (100, 100);

    [Header("Scroll")]
    [SerializeField] private float normalScrollSpeed = 10;
    [SerializeField] private float fastScrollSpeed = 20;

    [Header("Edge Scrolling")]
    [SerializeField] private bool allowEdgeScrolling = true;
    [SerializeField] private float edgePadding = 2f;

    [Header("Dragging")]
    [SerializeField] private bool allowDragging = true;

    [Header("Zoom")]
    [SerializeField] private bool allowZooming = true;
    [SerializeField] private float zoomFactor = 1.0f;
    [SerializeField] private float zoomStep = 0.25f;
    [SerializeField] private float minZoomFactor = 0.25f;
    [SerializeField] private float maxZoomFactor = 2.0f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        pixelCamera = GetComponent<PixelPerfectCamera>();
    }

    private void Start()
    {
        newPosition = transform.position;
        refResolution = new Vector2(pixelCamera.refResolutionX, pixelCamera.refResolutionY);

        if (initialTarget != null)
            CenterOnPosition(initialTarget.transform.position, true);
    }

    private void Update()
    {
        HandleDragging();
        HandleKeyboardScrolling();
        HandleEdgeScrolling();
        HandleZoom();
    }

    private void LateUpdate()
    {
        var zoomResolution = refResolution * zoomFactor;
        pixelCamera.refResolutionX = Mathf.RoundToInt(zoomResolution.x);
        pixelCamera.refResolutionY = Mathf.RoundToInt(zoomResolution.y);

        newPosition = ClampToBounds(newPosition);
        transform.position = newPosition;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        var tilesX = Mathf.CeilToInt(pixelCamera.refResolutionX / (float)pixelCamera.assetsPPU);
        var tilesY = Mathf.CeilToInt(pixelCamera.refResolutionY / (float)pixelCamera.assetsPPU);

        var minX = Mathf.FloorToInt(tilesX / 2f) - 11;
        var minY = Mathf.FloorToInt(tilesY / 2f) - 1;
        var maxX = (mapSize.x - Mathf.FloorToInt(tilesX / 2f) + 2);
        var maxY = (mapSize.y - Mathf.FloorToInt(tilesY / 2f) + 3);

        return new Vector3(
            Mathf.Clamp(Mathf.RoundToInt(position.x), minX, maxX),
            Mathf.Clamp(Mathf.RoundToInt(position.y), minY, maxY),
            position.z
        );
    }

    private void CenterOnPosition(Vector3 position, bool adjustForUI = false)
    {
        if (adjustForUI)
        {
            position.x -= 5;
            position.y += 1;
        }

        position.z = transform.position.z;
        newPosition = position;
    }

    private void HandleKeyboardScrolling()
    {
        if (isDragging || nextScrollTime > Time.time)
            return;

        var scrollDirection = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            scrollDirection += Vector2.up;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            scrollDirection += Vector2.down;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            scrollDirection += Vector2.left;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            scrollDirection += Vector2.right;

        if (scrollDirection == Vector2.zero)
            return;

        newPosition.x += scrollDirection.x;
        newPosition.y += scrollDirection.y;

        var scrollSpeed = Keyboard.current.shiftKey.isPressed ? fastScrollSpeed : normalScrollSpeed;
        nextScrollTime = Time.time + 1f / scrollSpeed;
    }

    private void HandleEdgeScrolling()
    {
        if (!allowEdgeScrolling || isDragging || !Application.isFocused || nextScrollTime > Time.time)
            return;

        var mousePosition = Mouse.current.position.ReadValue();

        var edgeScrollDirection = Vector2.zero;
        if (mousePosition.y > Screen.height - edgePadding)
            edgeScrollDirection += Vector2.up;
        if (mousePosition.y < edgePadding)
            edgeScrollDirection += Vector2.down;
        if (mousePosition.x < edgePadding)
            edgeScrollDirection += Vector2.left;
        if (mousePosition.x > Screen.width - edgePadding)
            edgeScrollDirection += Vector2.right;

        if (edgeScrollDirection == Vector2.zero)
            return;

        newPosition.x += edgeScrollDirection.x;
        newPosition.y += edgeScrollDirection.y;

        var scrollSpeed = Keyboard.current.shiftKey.isPressed ? fastScrollSpeed : normalScrollSpeed;
        nextScrollTime = Time.time + 1f / scrollSpeed;
    }

    private void HandleDragging()
    {
        if (!allowDragging)
        {
            isDragging = false;
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Mouse.current.IsPointerOverUI())
                {Debug.Log("OVER UI");return;}

            isDragging = true;
            dragOrigin = Mouse.current.GetWorldPosition(cam);
        }

        if (isDragging && Mouse.current.rightButton.isPressed)
        {
            var worldPosition = Mouse.current.GetWorldPosition(cam);
            var dragDelta = dragOrigin - worldPosition;

            newPosition += dragDelta;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
            isDragging = false;
    }

    private void HandleZoom()
    {
        if (!allowZooming || isDragging)
            return;

        var scrollDelta = Mouse.current.scroll.y.ReadValue();
        if (scrollDelta == 0)
            return;

        if (Mouse.current.IsPointerOverUI())
            return;

        if (scrollDelta > 0)
            ZoomIn();
        else if (scrollDelta < 0)
            ZoomOut();

    }

    private void ZoomIn() => zoomFactor = Mathf.Clamp(zoomFactor - zoomStep, minZoomFactor, maxZoomFactor);

    private void ZoomOut() => zoomFactor = Mathf.Clamp(zoomFactor + zoomStep, minZoomFactor, maxZoomFactor);

    public void SetMaxZoomFactor(float val)
    {
        maxZoomFactor = val;
    }
}

