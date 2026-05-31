using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Billboard: Nie znaleziono g?ównej kamery! Upewnij si?, ?e Twoja kamera ma tag 'MainCamera'.");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
}