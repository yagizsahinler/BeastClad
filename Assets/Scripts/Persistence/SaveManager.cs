using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using BeastClad.Data;
using BeastClad.Data.Persistence;
using BeastClad.Player;
using BeastClad.World;
using BeastClad.Arena;
using BeastClad.Security;

namespace BeastClad.Persistence
{
    /// <summary>
    /// Master persistent service managing atomic JSON save and load operations.
    /// Preserves player currency, monster rosters, 5-socket infuse configurations,
    /// arena division ladder rank, quarantine storage, and story flags across game sessions.
    /// </summary>
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var found = FindAnyObjectByType<SaveManager>();
                    if (found != null)
                    {
                        instance = found;
                    }
                    else
                    {
                        var go = new GameObject("SaveManager");
                        instance = go.AddComponent<SaveManager>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private const string SaveFileName = "savegame.json";
        private const string BackupFileName = "savegame.json.bak";
        private const string TempFileName = "savegame.json.tmp";

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        public static string BackupFilePath => Path.Combine(Application.persistentDataPath, BackupFileName);
        public static string TempFilePath => Path.Combine(Application.persistentDataPath, TempFileName);

        [Header("Runtime State")]
        [SerializeField] private PlayerSaveData currentSaveData;
        private MonsterDatabaseSO database;

        public PlayerSaveData CurrentSaveData => currentSaveData;
        public MonsterDatabaseSO Database => database != null ? database : (database = MonsterDatabaseSO.LoadDefault());

        public event Action<PlayerSaveData> OnGameSaved;
        public event Action<PlayerSaveData> OnGameLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (instance == null)
            {
                var go = new GameObject("SaveManager");
                instance = go.AddComponent<SaveManager>();
                if (Application.isPlaying) DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

            database = MonsterDatabaseSO.LoadDefault();

            // Initial load check
            if (HasSaveFile())
            {
                LoadGame();
            }
            else
            {
                currentSaveData = PlayerSaveData.CreateDefault();
                Debug.Log("<color=#38BDF8>[SaveManager]</color> Initialized with fresh default profile.");
            }
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // F5 QuickSave
            if (kb.f5Key.wasPressedThisFrame)
            {
                QuickSave();
            }

            // F6 QuickLoad
            if (kb.f6Key.wasPressedThisFrame)
            {
                QuickLoad();
            }
        }

        public bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }

        #region Atomic File I/O

        /// <summary>
        /// Captures current scene state and commits save data atomically to disk.
        /// </summary>
        public bool SaveCurrentGame(string explicitSceneName = null)
        {
            try
            {
                CaptureLiveState(explicitSceneName);
                return CommitToDisk(currentSaveData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=#FF4444>[SaveManager]</color> Critical error while saving game: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Directly commits a PlayerSaveData object to disk using atomic temporary swap.
        /// </summary>
        public bool CommitToDisk(PlayerSaveData data)
        {
            if (data == null)
            {
                Debug.LogError("<color=#FF4444>[SaveManager]</color> Cannot save null PlayerSaveData!");
                return false;
            }

            try
            {
                data.lastSavedTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(data, true);

                string savePath = SaveFilePath;
                string tempPath = TempFilePath;
                string backupPath = BackupFilePath;

                // 1. Write to temporary file
                File.WriteAllText(tempPath, json);

                // 2. Manage backup: if existing save exists, preserve it as .bak
                if (File.Exists(savePath))
                {
                    try
                    {
                        if (File.Exists(backupPath))
                        {
                            File.Delete(backupPath);
                        }
                        File.Copy(savePath, backupPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"<color=#FF9900>[SaveManager]</color> Could not create backup copy: {ex.Message}");
                    }
                }

                // 3. Atomically move/replace temp file to save file
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                File.Move(tempPath, savePath);

                currentSaveData = data;
                Debug.Log($"<color=#00FFAA>[SaveManager]</color> <b>Game saved successfully!</b> File: {savePath} | Credits: {data.credits} | Roster: {data.rosterMonsters.Count} | Slots: {data.equippedSlots.Count}");
                OnGameSaved?.Invoke(currentSaveData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=#FF4444>[SaveManager]</color> Atomic write failed: {ex.Message}\n{ex.StackTrace}");
                if (File.Exists(TempFilePath))
                {
                    try { File.Delete(TempFilePath); } catch { }
                }
                return false;
            }
        }

        /// <summary>
        /// Reads and deserializes save data from disk, falling back to backup if corrupted.
        /// </summary>
        public PlayerSaveData LoadGame()
        {
            string primaryPath = SaveFilePath;
            string backupPath = BackupFilePath;

            PlayerSaveData loaded = null;

            // Attempt 1: Load primary save file
            if (File.Exists(primaryPath))
            {
                try
                {
                    string json = File.ReadAllText(primaryPath);
                    loaded = JsonUtility.FromJson<PlayerSaveData>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"<color=#FF9900>[SaveManager]</color> Primary save file corrupt: {ex.Message}. Attempting backup recovery.");
                }
            }

            // Attempt 2: Load backup file if primary failed
            if (loaded == null && File.Exists(backupPath))
            {
                try
                {
                    string json = File.ReadAllText(backupPath);
                    loaded = JsonUtility.FromJson<PlayerSaveData>(json);
                    Debug.Log("<color=#FFD700>[SaveManager]</color> Successfully recovered save from backup file!");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"<color=#FF4444>[SaveManager]</color> Backup recovery failed: {ex.Message}");
                }
            }

            // Fallback: If no valid save exists, initialize defaults
            if (loaded == null)
            {
                loaded = PlayerSaveData.CreateDefault();
                Debug.Log("<color=#38BDF8>[SaveManager]</color> No existing save file found. Initialized fresh default data.");
            }

            currentSaveData = loaded;
            OnGameLoaded?.Invoke(currentSaveData);
            return currentSaveData;
        }

        public void DeleteSave()
        {
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
            if (File.Exists(BackupFilePath)) File.Delete(BackupFilePath);
            if (File.Exists(TempFilePath)) File.Delete(TempFilePath);

            currentSaveData = PlayerSaveData.CreateDefault();
            Debug.Log("<color=#FF4444>[SaveManager]</color> Deleted savegame file. Reset to defaults.");
        }

        #endregion

        #region Live State Capture & Application

        /// <summary>
        /// Harvests state from live scene components into currentSaveData.
        /// </summary>
        public void CaptureLiveState(string explicitSceneName = null)
        {
            if (currentSaveData == null)
            {
                currentSaveData = PlayerSaveData.CreateDefault();
            }

            currentSaveData.lastSavedTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentSaveData.lastSceneName = !string.IsNullOrEmpty(explicitSceneName)
                ? explicitSceneName
                : SceneManager.GetActiveScene().name;

            // 1. Locate player components
            var player = FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                // Wallet
                var wallet = player.GetComponent<PlayerWallet>();
                if (wallet != null)
                {
                    currentSaveData.credits = wallet.Credits;
                }

                // Monster Roster
                var roster = player.GetComponent<PlayerMonsterRoster>();
                currentSaveData.rosterMonsters.Clear();
                if (roster != null && roster.Roster != null)
                {
                    foreach (var specimen in roster.Roster)
                    {
                        if (specimen != null)
                        {
                            currentSaveData.rosterMonsters.Add(SavedMonsterData.FromInstance(specimen));
                        }
                    }
                }

                // Equipped Infuse Modules
                var infuse = player.GetComponent<PlayerInfuseManager>();
                currentSaveData.equippedSlots.Clear();
                if (infuse != null)
                {
                    foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                    {
                        var inst = infuse.GetEquippedInstance(slot);
                        var templ = infuse.GetEquippedMonster(slot);

                        if (inst != null)
                        {
                            currentSaveData.equippedSlots.Add(new SavedEquippedSlot(slot, SavedMonsterData.FromInstance(inst)));
                        }
                        else if (templ != null)
                        {
                            var mockInst = new MonsterInstance(templ, templ.defaultRegistration);
                            currentSaveData.equippedSlots.Add(new SavedEquippedSlot(slot, SavedMonsterData.FromInstance(mockInst)));
                        }
                    }
                }
            }
            else if (PlayerSessionState.HasActiveSession)
            {
                // Fallback to session cache if player is currently in transition
                currentSaveData.credits = PlayerSessionState.Credits;
            }

            // 2. Arena Ladder Standing
            var arenaCtrl = FindAnyObjectByType<ArenaBoutController>();
            if (arenaCtrl != null)
            {
                currentSaveData.arenaRankTitle = arenaCtrl.CurrentRank;
            }

            // 3. Municipal Quarantine Locker
            var locker = FindAnyObjectByType<MunicipalQuarantineLocker>();
            if (locker != null && locker.StoredInstances != null)
            {
                currentSaveData.quarantinedMonsters.Clear();
                foreach (var stashed in locker.StoredInstances)
                {
                    if (stashed != null)
                    {
                        currentSaveData.quarantinedMonsters.Add(SavedMonsterData.FromInstance(stashed));
                    }
                }
            }

            // Sync with PlayerSessionState
            SyncToSessionState();
        }

        /// <summary>
        /// Hydrates live player and scene components using currentSaveData.
        /// </summary>
        public void ApplySaveToGame(GameObject targetPlayer = null)
        {
            if (currentSaveData == null)
            {
                LoadGame();
            }

            var db = Database;

            // 1. Locate Player GameObject
            GameObject playerObj = targetPlayer;
            if (playerObj == null)
            {
                var pCtrl = FindAnyObjectByType<PlayerController2D>();
                if (pCtrl != null) playerObj = pCtrl.gameObject;
            }

            if (playerObj != null)
            {
                // Hydrate Wallet
                var wallet = playerObj.GetComponent<PlayerWallet>();
                if (wallet != null)
                {
                    wallet.SetCredits(currentSaveData.credits);
                }

                // Hydrate Roster
                var roster = playerObj.GetComponent<PlayerMonsterRoster>();
                if (roster != null)
                {
                    roster.ClearRoster();
                    foreach (var savedSpecimen in currentSaveData.rosterMonsters)
                    {
                        if (savedSpecimen != null)
                        {
                            var inst = savedSpecimen.ToInstance(db);
                            roster.AddMonster(inst);
                        }
                    }
                }

                // Hydrate Equipped Sockets
                var infuse = playerObj.GetComponent<PlayerInfuseManager>();
                if (infuse != null)
                {
                    // Unequip current slots
                    foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                    {
                        infuse.UnequipMonster(slot);
                    }

                    // Equip saved slots
                    foreach (var savedSlot in currentSaveData.equippedSlots)
                    {
                        if (savedSlot != null && savedSlot.monster != null)
                        {
                            var inst = savedSlot.monster.ToInstance(db);
                            infuse.EquipMonster(savedSlot.slot, inst, false);
                        }
                    }

                    infuse.RecalculateAllStats();
                }

                Debug.Log($"<color=#00FFAA>[SaveManager]</color> Restored save state to player: <b>{currentSaveData.credits} Credits</b> | <b>{currentSaveData.rosterMonsters.Count} Specimens</b> | <b>{currentSaveData.equippedSlots.Count} Equipped Slots</b>.");
            }

            // 2. Hydrate Quarantine Locker
            var locker = FindAnyObjectByType<MunicipalQuarantineLocker>();
            if (locker != null && currentSaveData.quarantinedMonsters != null && currentSaveData.quarantinedMonsters.Count > 0)
            {
                var stashedList = new List<MonsterInstance>();
                foreach (var q in currentSaveData.quarantinedMonsters)
                {
                    if (q != null)
                    {
                        stashedList.Add(q.ToInstance(db));
                    }
                }
                locker.SetStoredInstances(stashedList);
            }

            // 3. Hydrate Arena Rank
            var arena = FindAnyObjectByType<ArenaBoutController>();
            if (arena != null)
            {
                arena.SetRankIndex(currentSaveData.arenaRankIndex);
            }

            // Sync with PlayerSessionState
            SyncToSessionState();
        }

        private void SyncToSessionState()
        {
            if (currentSaveData == null) return;

            PlayerSessionState.SetCreditsDirect(currentSaveData.credits);
            PlayerSessionState.SetArenaRankDirect(currentSaveData.arenaRankIndex, currentSaveData.arenaRankTitle);

            var db = Database;
            var sessionRoster = new List<MonsterInstance>();
            foreach (var m in currentSaveData.rosterMonsters)
            {
                if (m != null) sessionRoster.Add(m.ToInstance(db));
            }
            PlayerSessionState.SetRosterDirect(sessionRoster);

            var sessionSlots = new Dictionary<EquipmentSlot, MonsterInstance>();
            foreach (var s in currentSaveData.equippedSlots)
            {
                if (s != null && s.monster != null)
                {
                    sessionSlots[s.slot] = s.monster.ToInstance(db);
                }
            }
            PlayerSessionState.SetEquippedSlotsDirect(sessionSlots);
        }

        #endregion

        #region Story Flags & Quick Actions

        public bool HasFlag(string flag)
        {
            return currentSaveData != null && currentSaveData.HasFlag(flag);
        }

        public void SetFlag(string flag, bool state = true)
        {
            if (currentSaveData == null)
            {
                currentSaveData = PlayerSaveData.CreateDefault();
            }

            currentSaveData.SetFlag(flag, state);
            SaveCurrentGame();
        }

        public void QuickSave()
        {
            bool success = SaveCurrentGame();
            if (success)
            {
                Debug.Log("<color=#00FFAA>[SaveManager]</color> <b>QuickSave [F5]</b> complete.");
            }
        }

        public void QuickLoad()
        {
            LoadGame();
            ApplySaveToGame();
            Debug.Log("<color=#38BDF8>[SaveManager]</color> <b>QuickLoad [F6]</b> complete.");
        }

        #endregion
    }
}
