using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

public enum Level2Phase
{
    Quest1_PlaceObjects,
    Quest2_MergeCactusBones,
    Quest3_MergePalmBones,
    Quest4_MergeCactusSatellite,
    Quest5_FreeBuild
}

public class Level2QuestManager : MonoBehaviour
{
    [Header("UI G?ówne")]
    public TextMeshProUGUI instructionText;
    public GameObject tutorialPanel;

    [Header("UI Dialogów (Podepnij jeden panel)")]
    public GameObject dialoguePanel;
    public Image dialogueImage;

    [Header("Grafiki Dialogów (Przeci?gnij pliki PNG z okna Project)")]
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

    private Level2Phase currentPhase = Level2Phase.Quest1_PlaceObjects;
    private int startBuildingCount;
    private int startMergeCount;

    private int targetBuildingCount;
    private int targetMergeCount;

    private bool isSystemReady = false;
    private bool isFinished = false;
    private float phaseCooldown = 0f;

    public bool IsBuildingBlocked => false;
    public bool IsSatelliteBlocked => currentPhase == Level2Phase.Quest1_PlaceObjects;
    public bool IsFirstTaskActive => currentPhase == Level2Phase.Quest1_PlaceObjects;
    public bool IsDialogueActive { get; private set; } = false;

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        StartCoroutine(InitializeTutorialRoutine());
    }

    private IEnumerator InitializeTutorialRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        startBuildingCount = CountBuildings();
        targetBuildingCount = startBuildingCount + 4;

        if (buildingSystem != null)
        {
            startMergeCount = buildingSystem.GetTotalMerges();
        }

        UpdateInstructionText();
        isSystemReady = true;

        StartCoroutine(ShowDialogueRoutine(currentPhase));
    }

    private void Update()
    {
        if (isFinished) return;
        if (!isSystemReady) return;

        ClampHappinessBar();

        if (PauseMenuController.IsPaused) return;
        if (IsDialogueActive) return;

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
            case Level2Phase.Quest1_PlaceObjects:
                maxFill = 0.32f;
                break;
            case Level2Phase.Quest2_MergeCactusBones:
                maxFill = 0.44f;
                break;
            case Level2Phase.Quest3_MergePalmBones:
                maxFill = 0.59f;
                break;
            case Level2Phase.Quest4_MergeCactusSatellite:
                maxFill = 0.71f;
                break;
            case Level2Phase.Quest5_FreeBuild:
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
            case Level2Phase.Quest1_PlaceObjects:
                if (CountBuildings() >= targetBuildingCount)
                {
                    NextPhase();
                }
                break;

            case Level2Phase.Quest2_MergeCactusBones:
            case Level2Phase.Quest3_MergePalmBones:
            case Level2Phase.Quest4_MergeCactusSatellite:
                if (buildingSystem != null && buildingSystem.GetTotalMerges() >= targetMergeCount)
                {
                    NextPhase();
                }
                break;

            case Level2Phase.Quest5_FreeBuild:
                if (creatureState != null)
                {
                    UpdateInstructionText();
                    if (creatureState.image.fillAmount >= 0.99f)
                    {
                        NextPhase();
                    }
                }
                break;
        }
    }

    private void NextPhase()
    {
        if (currentPhase == Level2Phase.Quest5_FreeBuild)
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

        if (currentPhase >= Level2Phase.Quest2_MergeCactusBones && currentPhase <= Level2Phase.Quest4_MergeCactusSatellite)
        {
            if (buildingSystem != null)
            {
                startMergeCount = buildingSystem.GetTotalMerges();
                targetMergeCount = startMergeCount + 1;
            }
        }

        StartCoroutine(ShowDialogueRoutine(currentPhase));
    }

    private IEnumerator ShowDialogueRoutine(Level2Phase phase)
    {
        Sprite activeSprite = GetSpriteForPhase(phase);

        if (activeSprite == null || dialoguePanel == null || dialogueImage == null)
        {
            UpdateInstructionText();
            yield break;
        }

        IsDialogueActive = true;

        if (instructionText != null) instructionText.text = "";

        dialogueImage.sprite = activeSprite;
        dialoguePanel.SetActive(true);

        yield return new WaitForSeconds(dialogueDuration);

        dialoguePanel.SetActive(false);
        IsDialogueActive = false;

        UpdateInstructionText();
    }

    private Sprite GetSpriteForPhase(Level2Phase phase)
    {
        switch (phase)
        {
            case Level2Phase.Quest1_PlaceObjects: return spriteQuest1;
            case Level2Phase.Quest2_MergeCactusBones: return spriteQuest2;
            case Level2Phase.Quest3_MergePalmBones: return spriteQuest3;
            case Level2Phase.Quest4_MergeCactusSatellite: return spriteQuest4;
            case Level2Phase.Quest5_FreeBuild: return spriteQuest5;
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

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("<b>OBJECTIVE:</b>\n");

        string mainText = "";
        string subText = "";

        switch (currentPhase)
        {
            case Level2Phase.Quest1_PlaceObjects:
                mainText = "Place objects to decorate the terrarium.";
                break;

            case Level2Phase.Quest2_MergeCactusBones:
                mainText = "Place 2 specific objects close to each other.";
                subText = "(try merging cactus with bones)";
                break;

            case Level2Phase.Quest3_MergePalmBones:
                mainText = "Place 2 specific objects close to each other.";
                subText = "(try merging palm tree with bones)";
                break;

            case Level2Phase.Quest4_MergeCactusSatellite:
                mainText = "Place 2 specific objects close to each other.";
                subText = "(try merging cactus with satelite)";
                break;

            case Level2Phase.Quest5_FreeBuild:
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
}