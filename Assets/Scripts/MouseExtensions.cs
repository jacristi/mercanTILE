using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class MouseExtensions
{
    private static Camera _mainCamera;

    public static Vector3 GetWorldPosition(this Mouse mouse, Camera camera = null)
    {
        if (camera == null && _mainCamera == null)
            _mainCamera = Camera.main;

        var targetCamera = camera != null ? camera : _mainCamera;

        if (targetCamera == null)
            throw new InvalidOperationException("No camera was provided and main camera is not available");

        var screenPosition = mouse.position.ReadValue();
        var worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        return worldPosition;
    }

    public static bool IsPointerOverUI(this Mouse mouse)
    {
        if (EventSystem.current == null)
            return false;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = mouse.position.ReadValue()
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        return raycastResults.Count > 0;
    }
}
