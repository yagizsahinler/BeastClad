using UnityEngine;
using UnityEngine.UI;
using BeastClad.World;
using BeastClad.Security;

namespace BeastClad.UI
{
    /// <summary>
    /// Displays world transit prompts when approaching SceneTransitionTrigger gates,
    /// Municipal Security Scanners, or Biometric Quarantine Lockers.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneTransitionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private Text promptText;

        private void Awake()
        {
            if (promptPanel != null) promptPanel.SetActive(false);
        }

        private void OnEnable()
        {
            SceneTransitionTrigger.OnPromptChanged += HandleTransitionPrompt;
            MunicipalQuarantineLocker.OnPromptChanged += HandleLockerPrompt;
            MunicipalScannerZone.OnScannerPromptChanged += HandleScannerPrompt;
        }

        private void OnDisable()
        {
            SceneTransitionTrigger.OnPromptChanged -= HandleTransitionPrompt;
            MunicipalQuarantineLocker.OnPromptChanged -= HandleLockerPrompt;
            MunicipalScannerZone.OnScannerPromptChanged -= HandleScannerPrompt;
        }

        private void HandleTransitionPrompt(SceneTransitionTrigger trigger, bool show, string text)
        {
            SetPrompt(show, text);
        }

        private void HandleLockerPrompt(MunicipalQuarantineLocker locker, bool show, string text)
        {
            SetPrompt(show, text);
        }

        private void HandleScannerPrompt(MunicipalScannerZone scanner, bool show, string text)
        {
            SetPrompt(show, text);
        }

        private void SetPrompt(bool show, string text)
        {
            if (promptPanel == null) return;

            promptPanel.SetActive(show);
            if (show && promptText != null)
            {
                promptText.text = text;
            }
        }
    }
}

