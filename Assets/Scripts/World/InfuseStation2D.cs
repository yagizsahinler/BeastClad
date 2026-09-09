using UnityEngine;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.World
{
    /// <summary>
    /// Interactive 2D test pedestal placed in the arena.
    /// When the player steps onto the station, it equips the designated monster module.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InfuseStation2D : MonoBehaviour
    {
        [Header("Monster Specimen")]
        [SerializeField] private MonsterDataSO monsterData;

        [Header("Equip Configuration")]
        [Tooltip("Slot to equip. If equipAllAvailable is true, equips across all compatible slots.")]
        [SerializeField] private EquipmentSlot targetSlot = EquipmentSlot.LeftArm;
        [SerializeField] private bool equipAllCompatibleSlots = false;

        [Header("Feedback")]
        [SerializeField] private Color stationColor = Color.white;
        [SerializeField] private SpriteRenderer stationRenderer;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (stationRenderer == null)
            {
                stationRenderer = GetComponent<SpriteRenderer>();
            }

            if (stationRenderer != null && monsterData != null && monsterData.primaryElement != null)
            {
                stationRenderer.color = monsterData.primaryElement.elementColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (monsterData == null) return;

            var infuseManager = other.GetComponent<PlayerInfuseManager>();
            if (infuseManager == null)
            {
                infuseManager = other.GetComponentInParent<PlayerInfuseManager>();
            }

            if (infuseManager != null)
            {
                if (equipAllCompatibleSlots)
                {
                    if (monsterData.headModule != null) infuseManager.EquipMonster(EquipmentSlot.Head, monsterData);
                    if (monsterData.chestModule != null) infuseManager.EquipMonster(EquipmentSlot.Chest, monsterData);
                    if (monsterData.leftArmModule != null) infuseManager.EquipMonster(EquipmentSlot.LeftArm, monsterData);
                    if (monsterData.rightArmModule != null) infuseManager.EquipMonster(EquipmentSlot.RightArm, monsterData);
                    if (monsterData.legsModule != null) infuseManager.EquipMonster(EquipmentSlot.Legs, monsterData);
                    Debug.Log($"<color=#00FFAA>[Infuse Station]</color> Equipped ALL modules of <b>{monsterData.commonName}</b>!");
                }
                else
                {
                    infuseManager.EquipMonster(targetSlot, monsterData);
                    Debug.Log($"<color=#00FFAA>[Infuse Station]</color> Equipped <b>{monsterData.commonName}</b> to <b>{targetSlot}</b>!");
                }
            }
        }

        public void SetStationData(MonsterDataSO monster, EquipmentSlot slot, bool equipAll = false)
        {
            monsterData = monster;
            targetSlot = slot;
            equipAllCompatibleSlots = equipAll;

            if (stationRenderer != null && monsterData != null && monsterData.primaryElement != null)
            {
                stationRenderer.color = monsterData.primaryElement.elementColor;
            }
        }
    }
}
