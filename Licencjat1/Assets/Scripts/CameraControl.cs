using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Isometric Settings")]
    [Tooltip("K?t nachylenia w dó?. 35.264 to prawdziwa izometria, 30 to popularny standard w grach.")]
    [SerializeField] private float elevationAngle = 35.264f;
    [SerializeField] private float YRotationMaxSpeed = 90f;

    [Header("Pan (Przesuwanie kamery)")]
    [SerializeField] private float mouseDragPanSpeed = 15f;
    [SerializeField] private float panSmoothTime = 0.15f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 100f;

    [Header("Smooth")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Mouse sensitivity"), Range(0.1f, 10f)]
    [SerializeField] private float mouseSensitivity = 1f;

    private Transform target;

    private float YRotation = 45f;
    private float YRotationVelocity = 0f;

    private float zoomDistance = 15f;
    private float zoomVelocity = 0f;

    private float targetYRotation = 45f;
    private float targetZoomDistance = 15f;

    private Vector3 desiredTargetPosition;
    private Vector3 targetPositionVelocity;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.Log("CameraControl: Zmieniam tryb kamery na Orthographic dla lepszego efektu izometrycznego.");
            cam.orthographic = true;
        }

        GameObject grid = null;
        int groundLayer = LayerMask.NameToLayer("Ground");

        if (groundLayer != -1)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == groundLayer)
                {
                    grid = obj;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("CameraControl: Warstwa 'Ground' nie istnieje! Dodaj j? w Unity (Layers -> Edit Layers).");
        }

        GameObject cameraTargetObj = new GameObject("CameraTarget");
        Vector3 cameraTargetPos = Vector3.zero;

        if (grid != null)
        {
            Renderer gridRenderer = grid.GetComponent<Renderer>();
            if (gridRenderer != null)
            {
                cameraTargetPos.x = gridRenderer.bounds.center.x;
                cameraTargetPos.z = gridRenderer.bounds.center.z;
                cameraTargetPos.y = gridRenderer.bounds.max.y;
            }
            else
            {
                cameraTargetPos = grid.transform.position;
            }
            Debug.Log($"CameraControl: Pomy?lnie znaleziono pod?o?e ({grid.name}) na warstwie Ground.");
        }
        else
        {
            Debug.LogWarning("CameraControl: Nie znaleziono obiektu na warstwie 'Ground'. Ustawiam ?rodek kamery w punkcie X:0, Y:0, Z:0.");
        }

        cameraTargetObj.transform.position = cameraTargetPos;
        target = cameraTargetObj.transform;
        desiredTargetPosition = target.position;

        if (cam.orthographic)
        {
            zoomDistance = cam.orthographicSize;
            targetZoomDistance = zoomDistance;
        }
    }

    private void LateUpdate()
    {
        HandlePanning();
        HandleRotation();
        HandleZoom();

        UpdateCameraPosition();
    }

    private void HandlePanning()
    {
        Vector3 movement = Vector3.zero;

        if (Input.GetMouseButton(2))
        {
            float mouseX = -Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");

            movement = new Vector3(mouseX, 0, mouseY) * mouseDragPanSpeed * Time.deltaTime;
        }

        if (movement != Vector3.zero)
        {
            float yAngle = transform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, yAngle, 0);

            float zoomMultiplier = Mathf.Max(1f, zoomDistance / 5f);

            desiredTargetPosition += rotation * movement * zoomMultiplier;
        }

        if (Vector3.Distance(target.position, desiredTargetPosition) > 0.001f)
        {
            target.position = Vector3.SmoothDamp(target.position, desiredTargetPosition, ref targetPositionVelocity, panSmoothTime);
        }
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

            // Szukamy punktu 3D, nad którym aktualnie znajduje si? kursor myszy
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, desiredTargetPosition.y, 0));

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPosition = ray.GetPoint(distance);

                float previousTargetZoom = targetZoomDistance;
                targetZoomDistance += scrollDelta;
                targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);

                // Obliczamy proporcj? zmiany zooma i przesuwamy punkt docelowy w stron? myszki
                if (previousTargetZoom != targetZoomDistance)
                {
                    float zoomRatio = targetZoomDistance / previousTargetZoom;
                    desiredTargetPosition = mouseWorldPosition + (desiredTargetPosition - mouseWorldPosition) * zoomRatio;
                }
            }
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
        desiredTargetPosition = worldPoint;
    }
}