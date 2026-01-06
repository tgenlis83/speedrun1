using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageAnnouncer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private Animator animator;

    public void SetText(string text)
    {
        announcementText.text = $"<color=white>~</color> {text} <color=white>~</color>";
        animator.Play("ObjectiveDisplay_Show");
    }
}