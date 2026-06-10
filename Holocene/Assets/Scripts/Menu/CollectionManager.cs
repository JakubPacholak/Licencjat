using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimalCard
    {
        public int cardID;
        public Button thumbnailButton;
        public GameObject lockedOverlay;
        public Sprite fullCardSprite;
    }

    public AnimalCard[] animalCards;
    public MainMenuController mainMenuController;
    public Image detailedCardImage;

    [Header("Referencja do panelu szczegó?ów")]
    public GameObject panelCollectionDetails;

    void OnEnable()
    {
        RefreshCards();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelCollectionDetails != null && panelCollectionDetails.activeInHierarchy)
            {
                mainMenuController.CloseCollectionDetails();
            }
            else
            {
                mainMenuController.OpenMainMenu();
            }
        }
    }

    public void RefreshCards()
    {
        foreach (var card in animalCards)
        {
            bool isUnlocked = PlayerPrefs.GetInt("UnlockedCard_" + card.cardID, 1) == 1;
            card.thumbnailButton.interactable = isUnlocked;

            if (card.lockedOverlay != null)
            {
                card.lockedOverlay.SetActive(!isUnlocked);
            }

            if (isUnlocked)
            {
                card.thumbnailButton.onClick.RemoveAllListeners();
                card.thumbnailButton.onClick.AddListener(() => ShowCardDetails(card.fullCardSprite));
            }
            else
            {
                card.thumbnailButton.onClick.RemoveAllListeners();
            }
        }
    }

    private void ShowCardDetails(Sprite cardSprite)
    {
        if (detailedCardImage != null)
        {
            detailedCardImage.sprite = cardSprite;
        }
        mainMenuController.OpenCollectionDetails();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        RefreshCards();
    }
}