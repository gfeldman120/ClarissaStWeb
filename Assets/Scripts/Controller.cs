using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private float teleportDelay = 0.1f;
    [SerializeField] private LayerMask teleportLayer;
    [SerializeField] private GameObject teleportPreview;
    [SerializeField] private LayerMask reconstructionMask;
    [SerializeField] private LayerMask geoMask;
    private bool usingGeoMask;
    private bool dragging;
    private Vector2 initialPosition;
    private Vector2 lastPosition;
    private float yaw;
    private float pitch;
    private float teleportTimer = 0.0f;
    private RaycastHit rayData;
    private bool rayHit = false;
    private bool flying = false;
    private bool stopPreview = false;

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
                    SaveRaycast();
                    break;
                // Finger being held down
                case TouchPhase.Moved:
                    rotation = touch.position - lastPosition;
                    lastPosition = touch.position;
                    if (rotation.magnitude > 0.05f)
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
                    // Need to run delay to avoid double teleporting issue
                    StartCoroutine(TeleportDelay());
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
                SaveRaycast();
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                DebugFly();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                usingGeoMask = !usingGeoMask;
                Camera.allCameras[0].cullingMask = usingGeoMask ? geoMask : reconstructionMask;
            }
        }
    }

    /// <summary>
    /// Save raycast data into variables, start teleport preview
    /// </summary>
    void SaveRaycast()
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(initialPosition);
        rayHit = Physics.Raycast(ray, out rayData, 1000.0f, teleportLayer);
        if (rayHit)
        {
            StartCoroutine(ShowTeleportPreview());
        }
    }

    /// <summary>
    /// Show the teleport preview after a delay
    /// </summary>
    IEnumerator ShowTeleportPreview()
    {
        stopPreview = false;
        yield return new WaitForSecondsRealtime(0.25f);
        if (!stopPreview)
        {
            teleportPreview.SetActive(true);
            teleportPreview.transform.position = new Vector3(rayData.point.x, teleportPreview.transform.position.y, rayData.point.z);
        }
    }

    /// <summary>
    /// Teleport the user to the initial position of the cursor
    /// </summary>
    void Teleport()
    {
        // Only teleport if allowed
        if (teleportTimer > 0.0f || !rayHit)
        {
            return;
        }
        transform.position = new Vector3(rayData.point.x, transform.position.y, rayData.point.z);
        stopPreview = true;
        teleportPreview.SetActive(false);
    }

    /// <summary>
    /// Stop the user from teleporting until the timer is done
    /// </summary>
    IEnumerator TeleportDelay()
    {
        teleportTimer = teleportDelay;
        while (teleportTimer > 0.0f)
        {
            teleportTimer -= Time.deltaTime;
            yield return null;
        }
        teleportTimer = 0.0f;
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
        stopPreview = true;
        teleportPreview.SetActive(false);
    }

    /// <summary>
    /// If on the ground, fly up
    /// </summary>
    void DebugFly()
    {
        if (flying)
        {
            transform.position -= new Vector3(0,50,0);
        }
        else
        {
            transform.position += new Vector3(0,50,0);
        }
        flying = !flying;
    }
}