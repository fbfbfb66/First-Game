using System;
using System.Text;
using UnityEngine;

public sealed class AIUtilityTooltip : MonoBehaviour
{
    [SerializeField] private EnemyAIBrain brain;
    [SerializeField] private Vector3 headOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField, Min(200f)] private float panelWidth = 400f;
    [SerializeField, Min(10)] private int fontSize = 30;
    [SerializeField] private Font font;
    private readonly StringBuilder text = new StringBuilder(256);
    private string content;
    private float nextRefresh;
    private Camera viewCamera;
    private GUIStyle labelStyle;

    private void Awake()
    {
        if (brain == null) brain = GetComponentInParent<EnemyAIBrain>();
    }

    private void OnEnable() => nextRefresh = 0f;

    private void Update()
    {
        if (brain == null || Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.1f;
        RefreshContent();
    }

    private void RefreshContent()
    {
        text.Clear();
        text.Append("当前：");
        if (brain.IsSuspended) text.Append("中断");
        else if (!brain.isActiveAndEnabled) text.Append("已停用");
        else if (brain.CurrentEvaluation.HasValue)
        {
            var intent = brain.CurrentEvaluation.Value.Intent;
            text.Append(DisplayName(intent.Action != null ? intent.Action : intent.Key));
        }
        else text.Append("无");

        var options = brain.UtilityOptionSet != null ? brain.UtilityOptionSet.Options : null;
        if (options != null)
        {
            foreach (var option in options)
            {
                text.Append('\n').Append(DisplayName(option)).Append("  ");
                try
                {
                    if (option == null || !brain.TryEvaluateUtilityOption(option, out var evaluation) ||
                        evaluation.Intent.Key == null || !float.IsFinite(evaluation.UtilityScore))
                        text.Append("配置错误");
                    else if (!evaluation.IsFeasible) text.Append("不可用");
                    else text.Append(evaluation.UtilityScore.ToString("F2"));
                }
                catch (Exception)
                {
                    text.Append("评估失败");
                }
            }
        }
        content = text.ToString();
    }

    // Display aliases only; resource identity and decision rules are unchanged.
    private static string DisplayName(UnityEngine.Object value)
    {
        if (value == null) return "缺失";
        string name = value.name;
        if (name.EndsWith("Attack01", StringComparison.Ordinal)) return "近战";
        if (name.EndsWith("Attack02", StringComparison.Ordinal)) return "突进";
        if (name.EndsWith("Attack03", StringComparison.Ordinal)) return "远程";
        if (name.EndsWith("Chase", StringComparison.Ordinal)) return "追击";
        if (name.EndsWith("Idle", StringComparison.Ordinal)) return "待机";
        if (name.EndsWith("Attack", StringComparison.Ordinal)) return "攻击";
        return name;
    }

    private void OnGUI()
    {
        if (brain == null || string.IsNullOrEmpty(content)) return;
        if (viewCamera == null || !viewCamera.isActiveAndEnabled) viewCamera = Camera.main;
        if (viewCamera == null) return;
        Vector3 anchor = viewCamera.WorldToScreenPoint(transform.position + headOffset);
        if (anchor.z <= 0f || anchor.x < 0f || anchor.x > Screen.width || anchor.y < 0f || anchor.y > Screen.height) return;

        if (labelStyle == null) labelStyle = new GUIStyle(GUI.skin.label) { wordWrap = false, richText = false };
        labelStyle.fontSize = fontSize;
        if (font != null) labelStyle.font = font;
        labelStyle.normal.textColor = Color.white;
        float scale = Mathf.Max(0.5f, Screen.height / 900f);
        float width = Mathf.Min(panelWidth, Screen.width / scale - 16f);
        float height = Mathf.Min(labelStyle.CalcHeight(new GUIContent(content), width - 24f) + 24f,
            Screen.height / scale * 0.65f);
        float x = Mathf.Clamp(anchor.x / scale - width / 2f, 8f, Screen.width / scale - width - 8f);
        float y = Mathf.Clamp((Screen.height - anchor.y) / scale - height, 8f, Screen.height / scale - height - 8f);
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousBackground = GUI.backgroundColor;
        try
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.blackTexture);
            GUI.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            GUI.Label(new Rect(x + 12f, y + 8f, width - 24f, height - 16f), content, labelStyle);
        }
        finally
        {
            GUI.matrix = previousMatrix;
            GUI.backgroundColor = previousBackground;
        }
    }
}
