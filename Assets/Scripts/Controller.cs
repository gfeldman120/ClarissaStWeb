using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private LayerMask teleportLayer;
    private bool dragging;
    private Vector2 initialPosition;
    private Vector2 lastPosition;
    private float yaw;
    private float pitch;

    // Update is called once per frame
    void Update()
    {
        Vector2 rotation = new Vector2(0.0f, 0.0f);
        // Do mobile controls
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                // Finger just put down
                case TouchPhase.Began:
                    initialPosition = touch.position;
                    lastPosition = Input.mousePosition;
                    dragging = false;
                    break;
                // Finger being held down
                case TouchPhase.Moved:
                    rotation = touch.position - lastPosition;
                    lastPosition = touch.position;
                    if (rotation.magnitude > 0.0f)
                    {
                        dragging = true;
                        Rotate(rotation);
                    }
                    break;
                // Finger released
                case TouchPhase.Ended:
                    if (dragging)
                    {
                        Teleport();
                    }
                    dragging = false;
                    break;
                // Finger otherwise no longer in use
                case TouchPhase.Canceled:
                    dragging = false;
                    break;
            }
        }
        // Do mouse controls
        else {
            // If this frame is the mouse button being pressed
            if (Input.GetMouseButtonDown(0))
            {
                initialPosition = Input.mousePosition;
                lastPosition = Input.mousePosition;
                dragging = false;
            }
            // If mouse down on multiple frames
            if (Input.GetMouseButton(0))
            {
                rotation = (Vector2)Input.mousePosition - lastPosition;
                lastPosition = Input.mousePosition;
                if (rotation.magnitude > 0.0f)
                {
                    dragging = true;
                    Rotate(rotation);
                }
            }
            // If this frame is the mouse button being released and not dragging cursor, teleport
            if (Input.GetMouseButtonUp(0) && !dragging)
            {
                Teleport();
            }
        }
    }

    /// <summary>
    /// Teleport the user to the initial position of the cursor
    /// </summary>
    void Teleport()
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(initialPosition);
        if (Physics.Raycast(ray, out RaycastHit rayData, 100f, teleportLayer))
        {
            transform.position = new Vector3(rayData.point.x, transform.position.y, rayData.point.z);
        }
    }
    
    /// <summary>
    /// Rotate the user by a rotation vector
    /// </summary>
    /// <param name="rotation">Vector by which to rotate camera</param>
    void Rotate(Vector2 rotation)
    {
        yaw += rotation.x * rotationSpeed;
        pitch = Mathf.Clamp(pitch - rotation.y * rotationSpeed, -80.0f, 80.0f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
    }
}