using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using BeastClad.Persistence;
using BeastClad.Data.Persistence;

namespace BeastClad.UI
{
    /// <summary>
    /// Controller for the Main Menu scene (MainMenu.unity).
    /// Handles profile inspection, continue transitions, fresh campaign creation,
    /// audio settings, operational controls manual, and application termination.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu Action Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button loreButton;
        [SerializeField] private Button quitButton;

        [Header("Continue Button Display")]
        [SerializeField] private Text continueButtonText;
        [SerializeField] private Text continueSubText;

        [Header("Modals")]
        [SerializeField] private GameObject settingsModalPanel;
        [SerializeField] private GameObject loreModalPanel;
        [SerializeField] private GameObject newGameConfirmModalPanel;

        [Header("Sub-Modal Buttons")]
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button closeLoreButton;
        [SerializeField] private Button confirmNewGameButton;
        [SerializeField] private Button cancelNewGameButton;

        [Header("Audio Settings")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeValueText;

        private void Awake()
        {
            Time.timeScale = 1f;
            HookListeners();
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (settingsModalPanel != null) settingsModalPanel.SetActive(false);
            if (loreModalPanel != null) loreModalPanel.SetActive(false);
            if (newGameConfirmModalPanel != null) newGameConfirmModalPanel.SetActive(false);

            RefreshSaveStatus();
            InitializeAudioSettings();
        }

        private void OnDestroy()
        {
            UnhookListeners();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (newGameConfirmModalPanel != null && newGameConfirmModalPanel.activeSelf)
                {
                    newGameConfirmModalPanel.SetActive(false);
                }
                else if (settingsModalPanel != null && settingsModalPanel.activeSelf)
                {
                    settingsModalPanel.SetActive(false);
                }
                else if (loreModalPanel != null && loreModalPanel.activeSelf)
                {
                    loreModalPanel.SetActive(false);
                }
            }
        }

        public void RefreshSaveStatus()
        {
            bool hasSave = false;
            try
            {
                hasSave = SaveManager.Instance.HasSaveFile();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MainMenuUI] Error checking save: {ex.Message}");
            }

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
            }

            if (hasSave)
            {
                if (continueButtonText != null)
                {
                    continueButtonText.text = "CONTINUE CAMPAIGN";
                }

                if (continueSubText != null)
                {
                    try
                    {
                        var data = SaveManager.Instance.LoadGame();
                        string sceneDisplay = data.lastSceneName switch
                        {
                            "District_CentralHub" => "Central Hub",
                            "Arena_Colosseum" => "Colosseum",
                            "Adventurer_Wilderness" => "Wilderness",
                            "Underground_BlackMarket" => "Underground",
                            _ => data.lastSceneName
                        };
                        continueSubText.text = $"{sceneDisplay}  •  {data.credits:N0} CR  •  {data.arenaRankTitle}";
                    }
                    catch
                    {
                        continueSubText.text = "Saved profile ready";
                    }
                }
            }
            else
            {
                if (continueButtonText != null)
                {
                    continueButtonText.text = "CONTINUE (NO SAVE)";
                }

                if (continueSubText != null)
                {
                    continueSubText.text = "No saved campaign found. Start New Game.";
                }
            }
        }

        private void InitializeAudioSettings()
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.RemoveListener(HandleVolumeChanged);
                volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
            }

            if (volumeValueText != null)
            {
                volumeValueText.text = $"{Mathf.RoundToInt(AudioListener.volume * 100f)}%";
            }
        }

        private void HandleVolumeChanged(float val)
        {
            AudioListener.volume = val;
            if (volumeValueText != null)
            {
                volumeValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
            }
        }

        public void HandleContinueClicked()
        {
            Time.timeScale = 1f;

            string targetScene = "District_CentralHub";
            try
            {
                if (SaveManager.Instance.HasSaveFile())
                {
                    var data = SaveManager.Instance.LoadGame();
                    if (!string.IsNullOrEmpty(data.lastSceneName))
                    {
                        targetScene = data.lastSceneName;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MainMenuUI] Error reading last saved scene: {ex.Message}");
            }

            SceneManager.LoadScene(targetScene);
        }

        public void HandleNewGameClicked()
        {
            if (SaveManager.Instance.HasSaveFile())
            {
                if (newGameConfirmModalPanel != null)
                {
                    newGameConfirmModalPanel.SetActive(true);
                }
                else
                {
                    StartNewGameDirect();
                }
            }
            else
            {
                StartNewGameDirect();
            }
        }

        public void HandleConfirmNewGameClicked()
        {
            if (newGameConfirmModalPanel != null)
            {
                newGameConfirmModalPanel.SetActive(false);
            }

            StartNewGameDirect();
        }

        public void HandleCancelNewGameClicked()
        {
            if (newGameConfirmModalPanel != null)
            {
                newGameConfirmModalPanel.SetActive(false);
            }
        }

        private void StartNewGameDirect()
        {
            try
            {
                SaveManager.Instance.DeleteSave();
                var freshData = PlayerSaveData.CreateDefault();
                SaveManager.Instance.CommitToDisk(freshData);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MainMenuUI] Warning while creating fresh save: {ex.Message}");
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene("District_CentralHub");
        }

        public void HandleSettingsClicked()
        {
            if (settingsModalPanel != null) settingsModalPanel.SetActive(true);
        }

        public void HandleCloseSettingsClicked()
        {
            if (settingsModalPanel != null) settingsModalPanel.SetActive(false);
        }

        public void HandleLoreClicked()
        {
            if (loreModalPanel != null) loreModalPanel.SetActive(true);
        }

        public void HandleCloseLoreClicked()
        {
            if (loreModalPanel != null) loreModalPanel.SetActive(false);
        }

        public void HandleQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HookListeners()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
                continueButton.onClick.AddListener(HandleContinueClicked);
            }

            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(HandleNewGameClicked);
                newGameButton.onClick.AddListener(HandleNewGameClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(HandleSettingsClicked);
                settingsButton.onClick.AddListener(HandleSettingsClicked);
            }

            if (loreButton != null)
            {
                loreButton.onClick.RemoveListener(HandleLoreClicked);
                loreButton.onClick.AddListener(HandleLoreClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuitClicked);
                quitButton.onClick.AddListener(HandleQuitClicked);
            }

            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.RemoveListener(HandleCloseSettingsClicked);
                closeSettingsButton.onClick.AddListener(HandleCloseSettingsClicked);
            }

            if (closeLoreButton != null)
            {
                closeLoreButton.onClick.RemoveListener(HandleCloseLoreClicked);
                closeLoreButton.onClick.AddListener(HandleCloseLoreClicked);
            }

            if (confirmNewGameButton != null)
            {
                confirmNewGameButton.onClick.RemoveListener(HandleConfirmNewGameClicked);
                confirmNewGameButton.onClick.AddListener(HandleConfirmNewGameClicked);
            }

            if (cancelNewGameButton != null)
            {
                cancelNewGameButton.onClick.RemoveListener(HandleCancelNewGameClicked);
                cancelNewGameButton.onClick.AddListener(HandleCancelNewGameClicked);
            }
        }

        private void UnhookListeners()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(HandleContinueClicked);
            if (newGameButton != null) newGameButton.onClick.RemoveListener(HandleNewGameClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
            if (loreButton != null) loreButton.onClick.RemoveListener(HandleLoreClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(HandleQuitClicked);
            if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(HandleCloseSettingsClicked);
            if (closeLoreButton != null) closeLoreButton.onClick.RemoveListener(HandleCloseLoreClicked);
            if (confirmNewGameButton != null) confirmNewGameButton.onClick.RemoveListener(HandleConfirmNewGameClicked);
            if (cancelNewGameButton != null) cancelNewGameButton.onClick.RemoveListener(HandleCancelNewGameClicked);
        }
    }
}
