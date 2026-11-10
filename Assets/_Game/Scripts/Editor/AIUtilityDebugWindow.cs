using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class AIUtilityDebugWindow : EditorWindow
{
    [SerializeField] private EnemyAIBrain observedBrain;
    [SerializeField] private bool followSelection = true;
    private readonly List<OptionSample> samples = new List<OptionSample>();
    private Vector2 scrollPosition;
    private double nextSampleTime;
    private int bestIndex = -1;

    private readonly struct OptionSample
    {
        public AIUtilityOptionDefinition Option { get; }
        public AIUtilityEvaluation Evaluation { get; }
        public string Error { get; }

        public OptionSample(AIUtilityOptionDefinition option, AIUtilityEvaluation evaluation, string error)
        {
            Option = option;
            Evaluation = evaluation;
            Error = error;
        }
    }

    [MenuItem("Window/Game/AI Utility Debugger")]
    public static void OpenWindow()
    {
        GetWindow<AIUtilityDebugWindow>("AI 决策调试");
    }

    private void OnEnable()
    {
        minSize = new Vector2(480f, 360f);
        EditorApplication.update += UpdateSamples;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        if (followSelection) FollowSelectedEnemy();
        nextSampleTime = 0d;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateSamples;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        samples.Clear();
        bestIndex = -1;
        nextSampleTime = 0d;
        Repaint();
    }

    private void OnSelectionChange()
    {
        if (followSelection) FollowSelectedEnemy();
    }

    private void FollowSelectedEnemy()
    {
        GameObject selected = Selection.activeGameObject;
        EnemyAIBrain brain = selected != null ? selected.GetComponentInParent<EnemyAIBrain>(true) : null;
        // Clicking an option asset should not lose the enemy being inspected.
        if (brain != null) SetObservedBrain(brain);
    }

    private void SetObservedBrain(EnemyAIBrain brain)
    {
        observedBrain = brain;
        samples.Clear();
        bestIndex = -1;
        nextSampleTime = 0d;
        Repaint();
    }

    private void UpdateSamples()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
            EditorApplication.timeSinceStartup < nextSampleTime) return;

        nextSampleTime = EditorApplication.timeSinceStartup + 0.1d;
        samples.Clear();
        bestIndex = -1;
        if (observedBrain != null && observedBrain.gameObject.activeInHierarchy &&
            observedBrain.UtilityOptionSet != null)
        {
            var options = observedBrain.UtilityOptionSet.Options;
            if (options != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    AIUtilityOptionDefinition option = options[i];
                    AIUtilityEvaluation evaluation = default;
                    string error = null;
                    try
                    {
                        if (option == null) error = "候选资源缺失";
                        else if (!observedBrain.TryEvaluateUtilityOption(option, out evaluation))
                            error = "评估失败，请检查配置及 Console";
                        else if (evaluation.Intent.Key == null || float.IsNaN(evaluation.UtilityScore) ||
                                 float.IsInfinity(evaluation.UtilityScore))
                            error = "返回了无效意图或分数";
                    }
                    catch (Exception exception)
                    {
                        error = $"评估异常：{exception.Message}";
                    }

                    samples.Add(new OptionSample(option, evaluation, error));
                    // Match the decision maker: infeasible entries are excluded; ties keep the first.
                    if (error == null && evaluation.IsFeasible &&
                        (bestIndex < 0 || evaluation.UtilityScore > samples[bestIndex].Evaluation.UtilityScore))
                        bestIndex = i;
                }
            }
        }
        Repaint();
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            EditorGUI.BeginChangeCheck();
            bool follow = GUILayout.Toggle(followSelection, "跟随 Hierarchy 选择", EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                followSelection = follow;
                if (followSelection) FollowSelectedEnemy();
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("实时采样 10 次/秒", EditorStyles.miniLabel);
        }

        EditorGUI.BeginChangeCheck();
        EnemyAIBrain brain = (EnemyAIBrain)EditorGUILayout.ObjectField("观察敌人", observedBrain, typeof(EnemyAIBrain), true);
        if (EditorGUI.EndChangeCheck())
        {
            followSelection = false;
            SetObservedBrain(brain);
        }

        if (observedBrain == null)
        {
            EditorGUILayout.HelpBox("在 Hierarchy 选择带 EnemyAIBrain 的敌人，或把它拖入上方字段。", MessageType.Info);
            return;
        }
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("候选配置", observedBrain.UtilityOptionSet, typeof(AIUtilityOptionSetDefinition), false);

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("进入 Play Mode 后显示实际执行的选择和每个候选的实时评分。", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawCurrentTask();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("实时候选评分", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("这里用当前位置、目标和冷却重新评估，只用于观察，不会切换任务。最高分仅在可行候选中比较，同分保留配置顺序靠前项。", MessageType.None);

        if (!observedBrain.gameObject.activeInHierarchy)
            EditorGUILayout.HelpBox("敌人未激活，暂不采样。", MessageType.Info);
        else if (observedBrain.UtilityOptionSet == null || samples.Count == 0)
            EditorGUILayout.HelpBox("没有候选评分，请检查候选配置是否为空。", MessageType.Warning);
        else
        {
            if (bestIndex < 0)
                EditorGUILayout.HelpBox("当前没有可行候选。", MessageType.Warning);
            for (int i = 0; i < samples.Count; i++) DrawOption(i);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawCurrentTask()
    {
        EditorGUILayout.LabelField("实际执行", EditorStyles.boldLabel);
        string state = !observedBrain.isActiveAndEnabled ? "Brain 已禁用" :
            observedBrain.IsSuspended ? "外部打断中，AI 已挂起" :
            observedBrain.CurrentTaskStatus?.ToString() ?? "没有任务";
        EditorGUILayout.LabelField("运行状态", state);
        EditorGUILayout.LabelField("允许途中重评", observedBrain.CanReplanCurrentTask ? "是" : "否");
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("任务树", observedBrain.CurrentTaskDefinition, typeof(AITaskDefinition), false);

        if (!observedBrain.CurrentEvaluation.HasValue) return;
        AIUtilityEvaluation evaluation = observedBrain.CurrentEvaluation.Value;
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("意图", evaluation.Intent.Key, typeof(AIIntentKey), false);
            EditorGUILayout.ObjectField("具体动作", evaluation.Intent.Action, typeof(ActionDefinition), false);
            EditorGUILayout.ObjectField("目标", evaluation.Intent.Target, typeof(Transform), true);
        }
        EditorGUILayout.LabelField("选中时分数", evaluation.UtilityScore.ToString("F4"));
        EditorGUILayout.LabelField("选中时原因", evaluation.Reason, EditorStyles.wordWrappedLabel);
    }

    private void DrawOption(int index)
    {
        OptionSample sample = samples[index];
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(sample.Option, typeof(AIUtilityOptionDefinition), false);
                string state = sample.Error != null ? "错误" :
                    !sample.Evaluation.IsFeasible ? "不可行" : index == bestIndex ? "最高分" : "可行";
                GUILayout.Label(state, GUILayout.Width(55f));
                GUILayout.Label(sample.Error == null ? sample.Evaluation.UtilityScore.ToString("F4") : "—",
                    GUILayout.Width(65f));
            }
            if (sample.Error != null)
            {
                EditorGUILayout.LabelField(sample.Error, EditorStyles.wordWrappedLabel);
                return;
            }
            AIIntent intent = sample.Evaluation.Intent;
            EditorGUILayout.LabelField($"意图：{intent.Key.name}    动作：{(intent.Action != null ? intent.Action.name : "无")}",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField(sample.Evaluation.Reason, EditorStyles.wordWrappedLabel);
        }
    }
}
