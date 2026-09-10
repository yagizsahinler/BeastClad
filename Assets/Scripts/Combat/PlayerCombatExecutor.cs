using UnityEngine;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Combat
{
    /// <summary>
    /// Executes physical 2D combat actions (spawning hitboxes and surface hazards)
    /// in response to PlayerInfuseManager skill events.
    /// </summary>
    [RequireComponent(typeof(PlayerInfuseManager))]
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerCombatExecutor : MonoBehaviour
    {
        [Header("Hazard Configuration")]
        [SerializeField] private ElementalTypeSO hydroElement;
        [SerializeField] private ElementalTypeSO voltElement;
        [SerializeField] private Sprite puddleSprite;

        private PlayerInfuseManager infuseManager;
        private PlayerController2D playerController;
        private PlayerStatsComponent playerStats;

        private void Awake()
        {
            infuseManager = GetComponent<PlayerInfuseManager>();
            playerController = GetComponent<PlayerController2D>();
            playerStats = GetComponent<PlayerStatsComponent>();

            if (hydroElement == null)
            {
                hydroElement = Resources.Load<ElementalTypeSO>("Data/Elements/Element_Hydro");
            }
            if (voltElement == null)
            {
                voltElement = Resources.Load<ElementalTypeSO>("Data/Elements/Element_Volt");
            }
        }

        private void OnEnable()
        {
            if (infuseManager != null)
            {
                infuseManager.OnSkillExecuted += HandleSkillExecuted;
            }
        }

        private void OnDisable()
        {
            if (infuseManager != null)
            {
                infuseManager.OnSkillExecuted -= HandleSkillExecuted;
            }
        }

        private void HandleSkillExecuted(EquipmentSlot slot, InfusePartSO module)
        {
            if (module == null) return;

            Vector2 facingDir = playerController != null ? playerController.FacingDirection : Vector2.down;
            if (facingDir.sqrMagnitude < 0.01f) facingDir = Vector2.down;

            switch (slot)
            {
                case EquipmentSlot.LeftArm:
                    ExecuteMeleeAttack(module, facingDir, 1.3f, 1.1f);
                    if (module.activeSkill != null && module.activeSkill.spawnsSurfaceHazard)
                    {
                        SpawnWaterPuddle(facingDir);
                    }
                    break;

                case EquipmentSlot.RightArm:
                    ExecuteMeleeAttack(module, facingDir, 1.8f, 0.9f);
                    break;

                case EquipmentSlot.Chest:
                    ExecuteChestSkill(module);
                    break;

                case EquipmentSlot.Head:
                    ExecuteHeadSkill(module);
                    break;
            }
        }

        private void ExecuteMeleeAttack(InfusePartSO module, Vector2 facingDir, float distance, float radius)
        {
            Vector3 spawnPos = transform.position + (Vector3)(facingDir * distance);

            var hitboxObj = new GameObject("MeleeHitbox_" + module.targetSlot);
            hitboxObj.transform.position = spawnPos;

            var col = hitboxObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;

            float damage = (playerStats != null ? playerStats.Attack : 15f) + (module.activeSkill != null ? module.activeSkill.baseDamage : 10f);

            // Check for Contraband / Overclock recoil
            if (infuseManager != null)
            {
                var regStatus = infuseManager.GetSlotRegistration(module.targetSlot);
                var instance = infuseManager.GetEquippedInstance(module.targetSlot);

                if (regStatus == RegistrationStatus.Contraband || (instance != null && instance.stability < 100f))
                {
                    float overclockBonus = 15f;
                    float recoilSelfDamage = 4f;

                    if (instance != null && instance.stability <= 55f)
                    {
                        overclockBonus = 20f;
                        recoilSelfDamage = 6f;
                    }

                    damage += overclockBonus;

                    if (playerStats != null && recoilSelfDamage > 0f)
                    {
                        playerStats.TakeTrueDamage(recoilSelfDamage);
                        Debug.LogWarning($"<color=#FF0044>[Bio-Recoil Backlash!]</color> Overclocked strike with <b>{module.targetSlot}</b> dealt <b>+{overclockBonus} Damage</b>, but inflicted <b>-{recoilSelfDamage} HP</b> bio-stress!");
                    }
                }
            }

            var element = module.activeSkill != null ? module.activeSkill.element : null;

            var payload = new DamagePayload(
                damage,
                element,
                facingDir,
                5.0f,
                gameObject
            );

            var hitbox = hitboxObj.AddComponent<Hitbox2D>();
            float lifetime = module.activeSkill != null ? module.activeSkill.hitboxDuration : 0.15f;
            hitbox.Initialize(payload, lifetime);

            // Ephemeral visual flash for the swing
            CreateAttackVisualCue(spawnPos, radius, element != null ? element.elementColor : Color.white, lifetime);
        }

        private void SpawnWaterPuddle(Vector2 facingDir)
        {
            Vector3 puddlePos = transform.position + (Vector3)(facingDir * 1.2f);

            var puddleObj = new GameObject("SurfaceHazard_WaterPuddle");
            puddleObj.transform.position = puddlePos;
            puddleObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

            var sr = puddleObj.AddComponent<SpriteRenderer>();
            sr.sprite = puddleSprite;
            sr.color = new Color(0.15f, 0.65f, 1.0f, 0.6f);
            sr.sortingOrder = 0;

            var col = puddleObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var hazard = puddleObj.AddComponent<SurfaceHazard>();
            hazard.Initialize(hydroElement, voltElement, 8.0f);

            Debug.Log("<color=#00BFFF>[Terraforming]</color> Deposited <b>Water Puddle</b> on arena floor!");
        }

        private void ExecuteChestSkill(InfusePartSO module)
        {
            if (playerStats != null)
            {
                playerStats.Heal(15f);
            }
            Debug.Log("<color=#D2B48C>[Infuse Skill]</color> <b>Iron Barrier Activated!</b> Fortified defense & restored 15 HP.");
        }

        private void ExecuteHeadSkill(InfusePartSO module)
        {
            if (playerStats != null)
            {
                playerStats.Heal(30f);
            }
            Debug.Log("<color=#00FFFF>[Infuse Skill]</color> <b>Wyrm Sensory Crest Pulse!</b> Purged debuffs & restored 30 HP.");
        }

        private void CreateAttackVisualCue(Vector3 pos, float radius, Color tint, float duration)
        {
            var cue = new GameObject("AttackVisualCue");
            cue.transform.position = pos;
            cue.transform.localScale = new Vector3(radius * 1.8f, radius * 1.8f, 1f);

            var sr = cue.AddComponent<SpriteRenderer>();
            sr.sprite = puddleSprite;
            sr.color = new Color(tint.r, tint.g, tint.b, 0.5f);
            sr.sortingOrder = 4;

            Destroy(cue, duration);
        }

        public void SetReferences(ElementalTypeSO hydro, ElementalTypeSO volt, Sprite sprite)
        {
            hydroElement = hydro;
            voltElement = volt;
            puddleSprite = sprite;
        }
    }
}
