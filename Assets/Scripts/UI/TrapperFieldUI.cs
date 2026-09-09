using UnityEngine;
using UnityEngine.UI;
using BeastClad.Trapping;

namespace BeastClad.UI
{
    /// <summary>
    /// HUD element providing real-time prompts for field trapping:
    /// Deploying lures, setting wire snares, and subduing snared wild specimens.
    /// </summary>
    [DisallowMultipleComponent]
    public class TrapperFieldUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerTrapperComponent trapperComponent;
        [SerializeField] private Text promptText;
        [SerializeField] private Text crateCountText;

        [Header("Default Prompts")]
        [SerializeField] private string defaultPrompt = "[1] Deploy Lure  |  [2] Deploy Snare";

        private void Awake()
        {
            if (trapperComponent == null)
            {
                trapperComponent = FindAnyObjectByType<PlayerTrapperComponent>();
            }
        }

        private void OnEnable()
        {
            if (trapperComponent != null)
            {
                trapperComponent.OnFieldPromptChanged += HandlePromptChanged;
                trapperComponent.OnMonsterCaptured += HandleMonsterCaptured;
            }
        }

        private void OnDisable()
        {
            if (trapperComponent != null)
            {
                trapperComponent.OnFieldPromptChanged -= HandlePromptChanged;
                trapperComponent.OnMonsterCaptured -= HandleMonsterCaptured;
            }
        }

        private void Start()
        {
            UpdateDisplay(defaultPrompt);
            UpdateCrateCount();
        }

        private void HandlePromptChanged(string newPrompt)
        {
            UpdateDisplay(newPrompt);
        }

        private void HandleMonsterCaptured(BeastClad.Data.MonsterInstance instance)
        {
            UpdateCrateCount();
        }

        private void UpdateDisplay(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
            }
        }

        private void UpdateCrateCount()
        {
            if (crateCountText != null && trapperComponent != null)
            {
                crateCountText.text = $"Specimen Crate: {trapperComponent.CapturedMonsters.Count} captured";
            }
        }

        public void SetReferences(PlayerTrapperComponent trapper, Text prompt, Text count)
        {
            trapperComponent = trapper;
            promptText = prompt;
            crateCountText = count;
            UpdateDisplay(defaultPrompt);
            UpdateCrateCount();
        }
    }
}
