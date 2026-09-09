using System;
using UnityEngine;

namespace BeastClad.Data
{
    public enum ContractStatus
    {
        Available,
        Active,
        Completed
    }

    /// <summary>
    /// Data-driven definition for Adventurer's Guild hunting & research contracts.
    /// Specifies specimen capture targets, objectives, dialogue briefings, and credit payouts.
    /// </summary>
    [CreateAssetMenu(fileName = "Contract_New", menuName = "BeastClad/Data/Guild Contract")]
    public class GuildContractSO : ScriptableObject
    {
        [Header("Contract Details")]
        public string contractId = "bounty_01";
        public string contractTitle = "Wilderness Bounty: Torrent Wyrm";
        public string clientName = "Guildmaster Vane";

        [TextArea(3, 5)]
        public string description = "A research institute requires a fresh, live Torrent Wyrm specimen from the quarantined wilderness clearing. Use Hydro pheromone bait to lure it into a wire snare.";

        [Header("Target Specimen Objective")]
        [Tooltip("The monster species required for contract completion.")]
        public MonsterDataSO targetSpecies;

        [Tooltip("Number of live specimens required.")]
        public int requiredCount = 1;

        [Header("Rewards")]
        [Tooltip("Credits awarded upon contract completion.")]
        public int rewardCredits = 250;

        [Tooltip("Text summary of rewards awarded.")]
        public string rewardSummary = "250 Credits + Adventurer Guild Rep";

        [Header("Dialogue Strings")]
        [TextArea(2, 4)]
        public string dialogueBriefing = "Greetings, trapper. The Institute needs a live Torrent Wyrm for hydro-conduction studies. Deploy your Hydro pheromones near the water pools and snare one alive. Don't let it tear you apart.";

        [TextArea(2, 4)]
        public string dialogueInProgress = "Still tracking? Remember to place the Hydro scent lure [1] to pull the Wyrm out into the open, then drop a heavy wire snare [2] right in its path. Subdue it [E] once it's tangled!";

        [TextArea(2, 4)]
        public string dialogueCompleted = "Remarkable work, trapper! That specimen is pristine. Here's your 250 Credits as promised. The Guild respects clean, non-lethal work.";
    }
}
