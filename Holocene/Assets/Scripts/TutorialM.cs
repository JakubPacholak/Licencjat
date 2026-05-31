using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

public enum TutorialPhase
{
    MoveCamera,
    ZoomCamera,
    ToggleInventory,
    PlaceAndRotate,
    HappinessBarInfo,

    Quest1_Place4Objects,
    Quest2_MergeStatue,
    Quest3_MergeMushroom,
    Quest4_MergeBarrel,
    Quest5_FreeBuild
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI G?ówne")]
    public TextMeshProUGUI instructionText;
    public GameObject tutorialPanel;

    [Header("UI Dialogów (Podepnij jeden panel)")]
    public GameObject dialoguePanel;
    public Image dialogueImage;

    [Header("Grafiki Dialogów (Sekcja Tutorialu)")]
    public Sprite spriteMoveCamera;
    public Sprite spriteZoomCamera;
    public Sprite spriteInventory;
    public Sprite spritePlaceAndRotate; 
    public Sprite spriteHappiness; 

    [Header("Grafiki Dialogów (Sekcja Questów)")]
    public Sprite spriteQuest1;
    public Sprite spriteQuest2;
    public Sprite spriteQuest3;
    public Sprite spriteQuest4;
    public Sprite spriteQuest5;

    [Header("Ustawienia Dialogów")]
    public float dialogueDuration = 4.0f;

    [Header("References")]
    public GameObject uiInventoryPanel;
    public BuildingSystem buildingSystem;
    public CreatureState creatureState;
    public Transform cameraTransform;

    [Header("Settings")]
    public float moveThreshold = 2.0f;

    private TutorialPhase currentPhase = TutorialPhase.MoveCamera;
    private Vector3 startCameraPos;
    private int startBuildingCount;
    private int startMergeCount;

    private int targetBuildingCount;
    private int targetMergeCount;

    private bool isSystemReady = false;
    private bool isFinished = false;
    private float phaseCooldown = 0f;

    private bool hasOpenedInventory = false;
    private bool hasPlacedObject = false;

    public bool IsBuildingBlocked => currentPhase < TutorialPhase.PlaceAndRotate;
    public bool IsStatueBlocked => currentPhase < TutorialPhase.Quest2_MergeStatue;
    public bool IsFirstTaskActive => currentPhase == TutorialPhase.PlaceAndRotate;

    public bool IsDialogueActive { get; private set; } = false;

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        StartCoroutine(InitializeTutorialRoutine());
    }

    private IEnumerator InitializeTutorialRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            startCameraPos = cameraTransform.position;
        }

        startBuildingCount = CountBuildings();
        if (buildingSystem != null)
        {
            startMergeCount = buildingSystem.GetTotalMerges();
        }
        else
        {
            Debug.LogWarning("TutorialManager: Brak przypi?tego BuildingSystem w Inspektorze!");
        }

        isSystemReady = true;

        StartCoroutine(ShowDialogueRoutine(currentPhase));
    }

    private void Update()
    {
        if (isFinished || !isSystemReady) return;

        ClampHappinessBar();

        if (PauseMenuController.IsPaused || IsDialogueActive) return;

        UpdateInstructionText();

        if (phaseCooldown > 0f)
        {
            phaseCooldown -= Time.deltaTime;
            return;
        }

        CheckCurrentObjective();
    }

    private void ClampHappinessBar()
    {
        if (creatureState == null || creatureState.image == null) return;

        float maxFill = 1.0f;
        switch (currentPhase)
        {
            case TutorialPhase.MoveCamera:
            case TutorialPhase.ZoomCamera:
            case TutorialPhase.ToggleInventory:
            case TutorialPhase.PlaceAndRotate:
            case TutorialPhase.HappinessBarInfo:
                maxFill = 0.25f;
                break;
            case TutorialPhase.Quest1_Place4Objects:
                maxFill = 0.35f;
                break;
            case TutorialPhase.Quest2_MergeStatue:
                maxFill = 0.50f;
                break;
            case TutorialPhase.Quest3_MergeMushroom:
                maxFill = 0.65f;
                break;
            case TutorialPhase.Quest4_MergeBarrel:
                maxFill = 0.80f;
                break;
            case TutorialPhase.Quest5_FreeBuild:
                maxFill = 1.0f;
                break;
        }

        if (creatureState.image.fillAmount > maxFill)
        {
            creatureState.image.fillAmount = maxFill;
        }
    }

    private void CheckCurrentObjective()
    {
        switch (currentPhase)
        {
            case TutorialPhase.MoveCamera:
                if (cameraTransform == null) return;
                float dist = Vector3.Distance(startCameraPos, cameraTransform.position);
                if (dist > moveThreshold) NextPhase();
                break;

            case TutorialPhase.ZoomCamera:
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollInput) > 0.01f) NextPhase();
                break;

            case TutorialPhase.ToggleInventory:
                if (Input.GetKeyDown(KeyCode.I))
                {
                    if (!hasOpenedInventory) hasOpenedInventory = true;
                    else NextPhase();
                }
                break;

            case TutorialPhase.PlaceAndRotate:
                if (CountBuildings() > startBuildingCount)
                {
                    hasPlacedObject = true;
                    NextPhase();
                }
                break;

            case TutorialPhase.HappinessBarInfo:
                NextPhase();
                break;

            case TutorialPhase.Quest1_Place4Objects:
                if (CountBuildings() >= targetBuildingCount) NextPhase();
                break;

            case TutorialPhase.Quest2_MergeStatue:
            case TutorialPhase.Quest3_MergeMushroom:
            case TutorialPhase.Quest4_MergeBarrel:
                if (buildingSystem != null && buildingSystem.GetTotalMerges() >= targetMergeCount)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.Quest5_FreeBuild:
                if (creatureState != null && creatureState.image.fillAmount >= 0.99f)
                {
                    NextPhase();
                }
                break;
        }
    }

    private void NextPhase()
    {
        if (currentPhase == TutorialPhase.Quest5_FreeBuild)
        {
            if (!isFinished)
            {
                isFinished = true;
                FinishTutorialImmediately();
            }
            return;
        }

        currentPhase++;
        phaseCooldown = 1.0f;

        if (currentPhase == TutorialPhase.PlaceAndRotate)
        {
            startBuildingCount = CountBuildings();
            hasPlacedObject = false;
        }
        else if (currentPhase == TutorialPhase.Quest1_Place4Objects)
        {
            startBuildingCount = CountBuildings();
            targetBuildingCount = startBuildingCount + 4;
        }
        else if (currentPhase >= TutorialPhase.Quest2_MergeStatue && currentPhase <= TutorialPhase.Quest4_MergeBarrel)
        {
            if (buildingSystem != null)
            {
                startMergeCount = buildingSystem.GetTotalMerges();
                targetMergeCount = startMergeCount + 1;
            }
            else
            {
                targetMergeCount = 1;
            }
        }

        if (cameraTransform != null && currentPhase <= TutorialPhase.ZoomCamera)
        {
            startCameraPos = cameraTransform.position;
        }

        StartCoroutine(ShowDialogueRoutine(currentPhase));
    }

    private IEnumerator ShowDialogueRoutine(TutorialPhase phase)
    {
        Sprite activeSprite = GetSpriteForPhase(phase);

        if (activeSprite == null || dialoguePanel == null || dialogueImage == null)
        {
            yield break;
        }

        IsDialogueActive = true;

        if (instructionText != null) instructionText.text = "";

        dialogueImage.sprite = activeSprite;
        dialoguePanel.SetActive(true);

        yield return new WaitForSeconds(dialogueDuration);

        dialoguePanel.SetActive(false);
        IsDialogueActive = false;
    }

    private Sprite GetSpriteForPhase(TutorialPhase phase)
    {
        switch (phase)
        {
            case TutorialPhase.MoveCamera: return spriteMoveCamera;
            case TutorialPhase.ZoomCamera: return spriteZoomCamera;
            case TutorialPhase.ToggleInventory: return spriteInventory;
            case TutorialPhase.PlaceAndRotate: return spritePlaceAndRotate;
            case TutorialPhase.HappinessBarInfo: return spriteHappiness;

            case TutorialPhase.Quest1_Place4Objects: return spriteQuest1;
            case TutorialPhase.Quest2_MergeStatue: return spriteQuest2;
            case TutorialPhase.Quest3_MergeMushroom: return spriteQuest3;
            case TutorialPhase.Quest4_MergeBarrel: return spriteQuest4;
            case TutorialPhase.Quest5_FreeBuild: return spriteQuest5;
            default: return null;
        }
    }

    private void FinishTutorialImmediately()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    private string GetCheckboxText(string text, bool isActive, bool isCompleted)
    {
        if (isCompleted) return $"<color=#888888><s>[X] {text}</s></color>";
        if (isActive) return $"<b><color=#FFFFFF>[ ] {text}</color></b>";
        return $"<color=#AAAAAA>[ ] {text}</color>";
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        StringBuilder sb = new StringBuilder();

        if (currentPhase <= TutorialPhase.HappinessBarInfo)
        {
            sb.AppendLine("<b>TUTORIAL:</b>\n");

            sb.AppendLine(GetCheckboxText("Move camera (RMB)", currentPhase == TutorialPhase.MoveCamera, currentPhase > TutorialPhase.MoveCamera));
            sb.AppendLine(GetCheckboxText("Zoom camera (Scroll)", currentPhase == TutorialPhase.ZoomCamera, currentPhase > TutorialPhase.ZoomCamera));

            if (currentPhase < TutorialPhase.ToggleInventory)
                sb.AppendLine("<color=#AAAAAA>[ ] Open & Close inventory (I)</color>");
            else if (currentPhase == TutorialPhase.ToggleInventory)
                sb.AppendLine("<b><color=#FFFFFF>[ ] " + (!hasOpenedInventory ? "Open inventory (I)" : "Close inventory (I)") + "</color></b>");
            else
                sb.AppendLine("<color=#888888><s>[X] Open & Close inventory (I)</s></color>");

            sb.AppendLine(GetCheckboxText("Place an object", currentPhase == TutorialPhase.PlaceAndRotate, currentPhase > TutorialPhase.PlaceAndRotate));

            sb.AppendLine(GetCheckboxText("Locate the happiness bar", currentPhase == TutorialPhase.HappinessBarInfo, currentPhase > TutorialPhase.HappinessBarInfo));

            instructionText.text = sb.ToString();
            return;
        }

        sb.AppendLine("<b>OBJECTIVE:</b>\n");

        string mainText = "";
        string subText = "";

        switch (currentPhase)
        {
            case TutorialPhase.Quest1_Place4Objects:
                mainText = "The specimen is terrified. It needs some stability.";
                subText = "(pick 4 objects and place them in the terrarium)";
                break;

            case TutorialPhase.Quest2_MergeStatue:
                mainText = "I see the glowing mushroom and the runic statue longing to merge together...";
                subText = "(try merging statue with mushroom)";
                break;

            case TutorialPhase.Quest3_MergeMushroom:
                mainText = "This specimen is exhausted from the journey from its planet.";
                subText = "(try merging mushroom with pole plant to make a nest)";
                break;

            case TutorialPhase.Quest4_MergeBarrel:
                mainText = "The creature is well rested now, but hungry. I should take care of that.";
                subText = "(try merging old barrel with pole plant)";
                break;

            case TutorialPhase.Quest5_FreeBuild:
                float fillPercent = creatureState != null ? creatureState.image.fillAmount * 100f : 0f;
                mainText = $"I think little one needs more enrichment in its enclosure. ({fillPercent:0}% / 100%)";
                subText = "(add more of any objects you like in the terrarium)";
                break;
        }

        sb.AppendLine($"<color=#FFFFFF>[ ] {mainText}</color>");
        if (!string.IsNullOrEmpty(subText))
        {
            sb.AppendLine($"<color=#AAAAAA><size=80%>{subText}</size></color>");
        }

        instructionText.text = sb.ToString();
    }

    private int CountBuildings()
    {
        return FindObjectsByType<Building>(FindObjectsSortMode.None).Length;
    }
}