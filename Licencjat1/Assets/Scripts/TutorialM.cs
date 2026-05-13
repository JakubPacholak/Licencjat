using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

public enum TutorialPhase
{
    MoveCamera,
    ZoomCamera,
    ToggleInventory,
    Quest1_PlaceObjects,
    Quest2_MergeStatue,
    Quest3_MergeMushroom,
    Quest4_MergeBarrel,
    Quest5_FreeBuild
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI instructionText;
    public GameObject tutorialPanel;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public float dialogueDuration = 4.0f;

    [Header("Level Up UI")]
    public GameObject levelUpPanel;

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

    public bool IsStatueBlocked => currentPhase == TutorialPhase.Quest1_PlaceObjects;
    public bool IsFirstTaskActive => currentPhase == TutorialPhase.Quest1_PlaceObjects;
    public bool IsDialogueActive { get; private set; } = false;

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
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

        UpdateInstructionText();
        isSystemReady = true;
    }

    private void Update()
    {
        if (isFinished)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                if (levelUpPanel != null)
                {
                    levelUpPanel.SetActive(!levelUpPanel.activeSelf);
                }
            }
            return;
        }

        if (!isSystemReady) return;

        if (PauseMenuController.IsPaused) return;
        if (IsDialogueActive) return;

        if (phaseCooldown > 0f)
        {
            phaseCooldown -= Time.deltaTime;
            return;
        }

        CheckCurrentObjective();
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
                    if (!hasOpenedInventory)
                    {
                        hasOpenedInventory = true;
                        UpdateInstructionText();
                    }
                    else
                    {
                        NextPhase();
                    }
                }
                break;

            case TutorialPhase.Quest1_PlaceObjects:
                if (CountBuildings() >= targetBuildingCount)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.Quest2_MergeStatue:
                if (buildingSystem != null && buildingSystem.GetTotalMerges() >= targetMergeCount)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.Quest3_MergeMushroom:
                if (buildingSystem != null && buildingSystem.GetTotalMerges() >= targetMergeCount)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.Quest4_MergeBarrel:
                if (buildingSystem != null && buildingSystem.GetTotalMerges() >= targetMergeCount)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.Quest5_FreeBuild:
                if (creatureState != null)
                {
                    UpdateInstructionText();
                    if (creatureState.image.fillAmount >= 0.95f)
                    {
                        NextPhase();
                    }
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

        if (currentPhase == TutorialPhase.Quest1_PlaceObjects)
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
        }

        if (cameraTransform != null && currentPhase <= TutorialPhase.ZoomCamera)
        {
            startCameraPos = cameraTransform.position;
        }

        if (currentPhase >= TutorialPhase.Quest1_PlaceObjects && currentPhase <= TutorialPhase.Quest5_FreeBuild)
        {
            StartCoroutine(ShowDialogueRoutine(currentPhase));
        }
        else
        {
            UpdateInstructionText();
        }
    }

    private IEnumerator ShowDialogueRoutine(TutorialPhase phase)
    {
        IsDialogueActive = true;

        if (instructionText != null) instructionText.text = "";

        string text = "";
        switch (phase)
        {
            case TutorialPhase.Quest1_PlaceObjects:
                text = "The specimen is terrified. I have to give it some stability - use those objects on the side, I'll pick 4 for now.";
                break;
            case TutorialPhase.Quest2_MergeStatue:
                text = "I see the glowing mushroom and the runic statue longing to merge together...";
                break;
            case TutorialPhase.Quest3_MergeMushroom:
                text = "This specimen is exhausted from the journey from it's planet. Let's make a nest for it.";
                break;
            case TutorialPhase.Quest4_MergeBarrel:
                text = "The creature is well rested now, but hungry. I should take care of that...";
                break;
            case TutorialPhase.Quest5_FreeBuild:
                text = "I think little one needs more enrichment in its enclosure. I'll work on it.";
                break;
        }

        if (dialogueText != null) dialogueText.text = text;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        yield return new WaitForSeconds(dialogueDuration);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        IsDialogueActive = false;

        UpdateInstructionText();
    }

    private void FinishTutorialImmediately()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        StringBuilder sb = new StringBuilder();

        if (currentPhase < TutorialPhase.Quest1_PlaceObjects)
        {
            sb.AppendLine("<b>TUTORIAL:</b>\n");

            sb.AppendLine(currentPhase == TutorialPhase.MoveCamera ? "<b><color=#FFFFFF>[ ] Move camera (RMB)</color></b>" : "<color=#888888><s>[X] Move camera (RMB)</s></color>");

            if (currentPhase < TutorialPhase.ZoomCamera)
                sb.AppendLine("<color=#AAAAAA>[ ] Zoom camera (Scroll)</color>");
            else if (currentPhase == TutorialPhase.ZoomCamera)
                sb.AppendLine("<b><color=#FFFFFF>[ ] Zoom camera (Scroll)</color></b>");
            else
                sb.AppendLine("<color=#888888><s>[X] Zoom camera (Scroll)</s></color>");

            if (currentPhase < TutorialPhase.ToggleInventory)
            {
                sb.AppendLine("<color=#AAAAAA>[ ] Open & Close inventory (I)</color>");
            }
            else if (currentPhase == TutorialPhase.ToggleInventory)
            {
                if (!hasOpenedInventory)
                    sb.AppendLine("<b><color=#FFFFFF>[ ] Open inventory (I)</color></b>");
                else
                    sb.AppendLine("<b><color=#FFFFFF>[ ] Close inventory (I)</color></b>");
            }

            instructionText.text = sb.ToString();
            return;
        }

        sb.AppendLine("<b>OBJECTIVE:</b>\n");

        string mainText = "";
        string subText = "";

        switch (currentPhase)
        {
            case TutorialPhase.Quest1_PlaceObjects:
                mainText = "Place objects to decorate the terrarium.";
                break;

            case TutorialPhase.Quest2_MergeStatue:
                mainText = "Place 2 specific objects close to each other.";
                subText = "(try merging statue with mushroom)";
                break;

            case TutorialPhase.Quest3_MergeMushroom:
                mainText = "Place 2 specific objects close to each other.";
                subText = "(try merging mushroom with pole plant)";
                break;

            case TutorialPhase.Quest4_MergeBarrel:
                mainText = "Place 2 specific objects close to each other.";
                subText = "(try merging old barrel with pole plant)";
                break;

            case TutorialPhase.Quest5_FreeBuild:
                float fillPercent = creatureState != null ? creatureState.image.fillAmount * 100f : 0f;
                mainText = $"Fill the happiness bar ({fillPercent:0}% / 100%)";
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

    public void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    public void ConfirmLevelUp()
    {
        Debug.Log("Przechodz? do nast?pnego poziomu!");
    }
}