using UnityEngine;
using TMPro;

public class MergeIndicator : MonoBehaviour
{
    [Header("Przypisz tutaj komponent tekstowy")]
    [SerializeField] private TextMeshPro textComponent;

    private void Update()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    public void Show(Vector3 position, string letter)
    {
        gameObject.SetActive(true);
        transform.position = position + new Vector3(0, 2f, 0);

        if (textComponent != null)
        {
            textComponent.text = letter;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}