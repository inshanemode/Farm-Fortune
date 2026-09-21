using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dedicated Goal UI widget for displaying active milestone and progress.
/// </summary>
public class GoalUI : MonoBehaviour
{
    public TMP_Text goalLabel;
    public Image progressBar;

    private void Update()
    {
        if (GoalManager.Instance != null && Player.main != null)
        {
            string desc = GoalManager.Instance.GetGoalDescription(Player.main);
            float prog = GoalManager.Instance.GetGoalProgress01(Player.main);
            UpdateGoal(desc, prog);
        }
    }

    public void UpdateGoal(string description, float progress01)
    {
        if (goalLabel != null)
        {
            goalLabel.text = description;
            goalLabel.color = FarmUITheme.InkBrown;
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Clamp01(progress01);
            progressBar.color = FarmUITheme.GoldReward;
        }
    }
}
