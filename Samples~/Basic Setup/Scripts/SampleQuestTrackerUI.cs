using System.Collections.Generic;
using RAXY.Quest;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SampleQuestTrackerUI : MonoBehaviour
{
    [SerializeField] QuestManagerBase questManager;
    [SerializeField] TextMeshProUGUI questNameTmp;
    [SerializeField] GameObject stepNameContainer;
    [SerializeField] TextMeshProUGUI stepNameTmp;
    [SerializeField] Transform objectivesContainer;
    [SerializeField] SampleQuestObjectiveRowUI objectiveRowPrefab;

    readonly List<SampleQuestObjectiveRowUI> _rows = new();
    CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false);
    }

    void Start()
    {
        if (questManager == null)
            questManager = QuestManagerBase.BaseInstance;

        if (questManager == null)
        {
            Debug.LogWarning($"[{nameof(SampleQuestTrackerUI)}] QuestManagerBase is not assigned.", this);
            return;
        }

        questManager.OnTrackedQuestChanged += OnQuestEvent;
        questManager.OnQuestStepChanged += OnQuestStepChanged;
        questManager.OnObjectiveProgressed += OnObjectiveEvent;
        questManager.OnObjectiveCompleted += OnObjectiveEvent;
        questManager.OnQuestTaken += OnQuestEvent;
        questManager.OnQuestCompleted += OnQuestEvent;

        Refresh();
    }

    void OnDestroy()
    {
        if (questManager == null)
            return;

        questManager.OnTrackedQuestChanged -= OnQuestEvent;
        questManager.OnQuestStepChanged -= OnQuestStepChanged;
        questManager.OnObjectiveProgressed -= OnObjectiveEvent;
        questManager.OnObjectiveCompleted -= OnObjectiveEvent;
        questManager.OnQuestTaken -= OnQuestEvent;
        questManager.OnQuestCompleted -= OnQuestEvent;
    }

    void OnQuestEvent(string _) => Refresh();

    void OnQuestStepChanged(string _, int __) => Refresh();

    void OnObjectiveEvent(QuestStepObjective_Runtime _) => Refresh();

    void Refresh()
    {
        if (questManager == null)
        {
            SetVisible(false);
            return;
        }

        var questId = questManager.TrackedQuest;
        if (string.IsNullOrEmpty(questId))
        {
            SetVisible(false);
            return;
        }

        var quest = questManager.GetActiveQuest(questId);
        var activeStep = quest?.ActiveStep;
        if (activeStep == null)
        {
            SetVisible(false);
            return;
        }

        if (questNameTmp != null)
            questNameTmp.text = quest.QuestData?.questNameProvider?.String ?? questId;

        var stepName = activeStep.StepName ?? string.Empty;
        if (stepNameTmp != null)
            stepNameTmp.text = stepName;

        if (stepNameContainer != null)
            stepNameContainer.SetActive(!string.IsNullOrEmpty(stepName));

        UpdateObjectiveRows(CollectVisibleObjectives(activeStep), stepName);
        SetVisible(true);
    }

    static List<QuestStepObjective_Runtime> CollectVisibleObjectives(QuestStep_Runtime step)
    {
        var result = new List<QuestStepObjective_Runtime>();

        if (step.MainObjectives != null)
        {
            foreach (var objective in step.MainObjectives)
            {
                if (objective.State == QuestCompletionState.InProgress)
                    result.Add(objective);
            }
        }

        if (step.OptionalObjectives != null)
        {
            foreach (var objective in step.OptionalObjectives)
            {
                if (objective.State == QuestCompletionState.InProgress)
                    result.Add(objective);
            }
        }

        return result;
    }

    void UpdateObjectiveRows(List<QuestStepObjective_Runtime> objectives, string stepFallback)
    {
        if (objectiveRowPrefab == null || objectivesContainer == null)
            return;

        while (_rows.Count < objectives.Count)
        {
            var row = Instantiate(objectiveRowPrefab, objectivesContainer);
            _rows.Add(row);
        }

        for (int i = 0; i < _rows.Count; i++)
        {
            if (i < objectives.Count)
            {
                _rows[i].gameObject.SetActive(true);
                _rows[i].Setup(objectives[i], stepFallback);
            }
            else
            {
                _rows[i].gameObject.SetActive(false);
            }
        }
    }

    void SetVisible(bool visible)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}
