using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float initialZoom = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 300f;

    [Header("Pan Settings")]
    public float panSpeed = 1f;

    private Vector3 dragOrigin;

    void Start()
    {
        // Set initial zoom
        Camera.main.orthographicSize = Mathf.Clamp(initialZoom, minZoom, maxZoom);
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(2))  // Scroll/middle mouse button
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += difference;
        }
    }
}
