using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindTarget = true;

    [Header("Rotation and elevation")]
    [SerializeField] private float YRotationMaxSpeed = 90f;
    [SerializeField] private float YPositionMax = 15f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 100f;

    [Header("Pan")]
    [SerializeField] private float panYSpeed = 1f;

    [Header("Smooth")]
    [SerializeField] private float smoothTime = 1f;

    [Header("Mouse sensitivity"), Range(0.1f, 10f)]
    [SerializeField] private float mouseSensitivity = 1f;

    private float YRotation = 45f;
    private float YRotationVelocity = 0f;
    private float yPosition = 5f;
    private float yVelocity = 0f;
    private float zoomDistance = 15f;
    private float zoomVelocity = 0f;

    private float targetYRotation = 45f;
    private float targetYPosition = 5f;
    private float targetZoomDistance = 15f;

    private Camera cam;
    private Vector3 rotation = Vector3.zero;
    private Vector3 targetPosition;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (autoFindTarget)
        {
            GameObject grid = GameObject.Find("Floor");

            if (grid != null)
            {
                GameObject cameraTargetObj = new GameObject("CameraTarget");

                Vector3 cameraTargetPos;
                
                cameraTargetPos.x = grid.GetComponent<Renderer>().bounds.center.x;
                cameraTargetPos.z = grid.GetComponent<Renderer>().bounds.center.z;
                cameraTargetPos.y = grid.GetComponent<Renderer>().bounds.max.y;

                cameraTargetObj.transform.position = cameraTargetPos;
                target = cameraTargetObj.transform;
            }
            else
            {
                Debug.LogWarning("CameraControl: No BuildingGrid found. Assign target manually.");
            }
        }

        if (target == null)
        {
            Debug.LogError("CameraControl: Target is null!");
            enabled = false;
            return;
        }

        targetPosition = target.position;
    }

    private void LateUpdate()
    {
        HandleRotation();
        HandleZoom();
        HandlePan();

        UpdateCameraPosition();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = -Input.GetAxis("Mouse X") * mouseSensitivity;            
            targetYRotation += mouseX;
            targetYRotation = targetYRotation % 360f;
            if (targetYRotation < 0) targetYRotation += 360f;            
        }
        if (Mathf.Abs(targetYRotation - YRotation) > 0.001f)
        {
            YRotation = Mathf.SmoothDampAngle(YRotation, targetYRotation, ref YRotationVelocity, smoothTime, YRotationMaxSpeed);            
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float scrollDelta = -scroll * zoomSpeed * Time.deltaTime * 4;
            targetZoomDistance += scrollDelta;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }
        if(Mathf.Abs(targetZoomDistance - zoomDistance) > 0.001f)
        {
            zoomDistance = Mathf.SmoothDamp(zoomDistance, targetZoomDistance, ref zoomVelocity, smoothTime);
        }
    }

    private void HandlePan()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseY = -Input.GetAxis("Mouse Y") * panYSpeed;
            targetYPosition = yPosition + mouseY;   
            targetYPosition = Mathf.Clamp(targetYPosition, 0f, YPositionMax);
        }
        if(Mathf.Abs(targetYPosition - yPosition) > 0.001f)
        {
            yPosition = Mathf.SmoothDamp(yPosition, targetYPosition, ref yVelocity, smoothTime);
        }

    }

    private void UpdateCameraPosition()
    {
        Vector3 pos;
        pos.x = target.position.x + Mathf.Cos(Mathf.Deg2Rad * YRotation) * zoomDistance;
        pos.z = target.position.z + Mathf.Sin(Mathf.Deg2Rad * YRotation) * zoomDistance;
        pos.y = target.position.y + yPosition;
        transform.position = pos;

        transform.LookAt(target.position, Vector3.up);
    }

    public void FocusOn(Vector3 worldPoint)
    {
    }

    public void FocusOnBuilding(Building building)
    {

    }
}