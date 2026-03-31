using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindTarget = true;

    [Header("Isometric Settings")]
    [Tooltip("K?t nachylenia w dó?. 35.264 to prawdziwa izometria, 30 to popularny standard w grach.")]
    [SerializeField] private float elevationAngle = 35.264f;
    [SerializeField] private float YRotationMaxSpeed = 90f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 100f;

    [Header("Smooth")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Mouse sensitivity"), Range(0.1f, 10f)]
    [SerializeField] private float mouseSensitivity = 1f;

    private float YRotation = 45f;
    private float YRotationVelocity = 0f;

    private float zoomDistance = 15f;
    private float zoomVelocity = 0f;

    private float targetYRotation = 45f;
    private float targetZoomDistance = 15f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.Log("CameraControl: Zmieniam tryb kamery na Orthographic dla lepszego efektu izometrycznego.");
            cam.orthographic = true;
        }

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
                Debug.LogWarning("CameraControl: Nie znaleziono 'Floor'. Przypisz cel r?cznie.");
            }
        }

        if (target == null)
        {
            Debug.LogError("CameraControl: Target jest nullem!");
            enabled = false;
            return;
        }

        if (cam.orthographic)
        {
            zoomDistance = cam.orthographicSize;
            targetZoomDistance = zoomDistance;
        }
    }

    private void LateUpdate()
    {
        HandleRotation();
        HandleZoom();

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
            float scrollDelta = -scroll * zoomSpeed * Time.deltaTime * 50f;
            targetZoomDistance += scrollDelta;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }
        if (Mathf.Abs(targetZoomDistance - zoomDistance) > 0.001f)
        {
            zoomDistance = Mathf.SmoothDamp(zoomDistance, targetZoomDistance, ref zoomVelocity, smoothTime);

            if (cam.orthographic)
            {
                cam.orthographicSize = zoomDistance;
            }
        }
    }

    private void UpdateCameraPosition()
    {
        float angleRad = Mathf.Deg2Rad * YRotation;
        float elevRad = Mathf.Deg2Rad * elevationAngle;

        float rigDistance = 20f;

        Vector3 pos;

        float hDist = rigDistance * Mathf.Cos(elevRad);
        float vDist = rigDistance * Mathf.Sin(elevRad);

        pos.x = target.position.x + hDist * Mathf.Cos(angleRad);
        pos.z = target.position.z + hDist * Mathf.Sin(angleRad);
        pos.y = target.position.y + vDist;

        transform.position = pos;
        transform.LookAt(target.position, Vector3.up);
    }

    public void FocusOn(Vector3 worldPoint)
    {
        if (target != null) target.position = worldPoint;
    }

    public void FocusOnBuilding(Building building)
    {
    }
}