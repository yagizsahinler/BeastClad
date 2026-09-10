using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Security
{
    /// <summary>
    /// Detailed diagnostic report generated when the player crosses a municipal scanner.
    /// </summary>
    public class ScanReport
    {
        public int legalCount;
        public int unregisteredCount;
        public int contrabandCount;
        public List<string> violationDetails = new List<string>();

        public bool IsClear => unregisteredCount == 0 && contrabandCount == 0;
        public RegistrationStatus HighestSeverity => contrabandCount > 0 ? RegistrationStatus.Contraband : (unregisteredCount > 0 ? RegistrationStatus.Unregistered : RegistrationStatus.Legal);

        public string GetSummaryText()
        {
            if (IsClear)
            {
                return "ALL BIOMETRICS LICENSED — ACCESS AUTHORIZED";
            }
            if (contrabandCount > 0)
            {
                return $"CRITICAL ALARM: CONTRABAND DETECTED ({contrabandCount} Black Market Items!)";
            }
            return $"SECURITY WARNING: UNREGISTERED SPECIMENS DETECTED ({unregisteredCount} Wild Items)";
        }
    }

    /// <summary>
    /// Interactive municipal checkpoint trigger archway.
    /// Performs biometric scans of equipped infuse limbs and player monster roster,
    /// triggering security alarms and alerting stationed enforcers upon violation.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MunicipalScannerZone : MonoBehaviour
    {
        [Header("Checkpoint Identity")]
        [SerializeField] private string checkpointName = "Municipal Security Gate 04";
        [SerializeField] private string sectorName = "Arena Civil Sector";

        [Header("Visual Indicators")]
        [SerializeField] private SpriteRenderer beamRenderer;
        [SerializeField] private SpriteRenderer statusLight;
        [SerializeField] private SpriteRenderer[] statusLights;

        [Header("Beam Colors")]
        [SerializeField] private Color idleColor = new Color(0.95f, 0.75f, 0.1f, 0.45f);
        [SerializeField] private Color clearedColor = new Color(0f, 1f, 0.5f, 0.85f);
        [SerializeField] private Color violationColor = new Color(1f, 0.15f, 0.2f, 0.95f);

        [Header("Stationed Enforcer")]
        [SerializeField] private MunicipalEnforcerAI2D stationedEnforcer;

        private Coroutine pulseRoutine;
        private Coroutine resetRoutine;

        public string CheckpointName => checkpointName;
        public string SectorName => sectorName;
        public MunicipalEnforcerAI2D StationedEnforcer => stationedEnforcer;

        // Static Events for decoupled HUD / Audio
        public static event Action<MunicipalScannerZone, ScanReport> OnScanCleared;
        public static event Action<MunicipalScannerZone, ScanReport> OnScanViolation;
        public static event Action<MunicipalScannerZone> OnScannerExited;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            SetBeamColor(idleColor);
            SetStatusLightColor(new Color(0.95f, 0.75f, 0.1f, 1f));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                var infuse = other.GetComponent<PlayerInfuseManager>();
                var roster = other.GetComponent<PlayerMonsterRoster>();
                PerformScan(infuse, roster);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                OnScannerExited?.Invoke(this);
                if (resetRoutine != null) StopCoroutine(resetRoutine);
                resetRoutine = StartCoroutine(ResetAfterDelayRoutine(1.8f));
            }
        }

        public ScanReport PerformScan(PlayerInfuseManager infuse, PlayerMonsterRoster roster)
        {
            if (infuse == null && roster == null)
            {
                infuse = FindAnyObjectByType<PlayerInfuseManager>();
                roster = FindAnyObjectByType<PlayerMonsterRoster>();
            }

            var report = new ScanReport();

            // 1. Scan Equipped Sockets
            if (infuse != null)
            {
                EquipmentSlot[] slots = (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
                foreach (var slot in slots)
                {
                    var monster = infuse.GetEquippedMonster(slot);
                    if (monster != null)
                    {
                        var status = infuse.GetSlotRegistration(slot);
                        CategorizeStatus(status, $"{slot} Socket: {monster.commonName}", report);
                    }
                }
            }

            // 2. Scan Player Monster Roster
            if (roster != null)
            {
                foreach (var instance in roster.Roster)
                {
                    if (instance != null)
                    {
                        CategorizeStatus(instance.registrationStatus, $"Roster Vault: {instance.nickname}", report);
                    }
                }
            }

            // Process Scan Results
            if (report.IsClear)
            {
                HandleCleared(report);
            }
            else
            {
                HandleViolation(report);
            }

            return report;
        }

        private void CategorizeStatus(RegistrationStatus status, string label, ScanReport report)
        {
            switch (status)
            {
                case RegistrationStatus.Legal:
                    report.legalCount++;
                    break;
                case RegistrationStatus.Unregistered:
                    report.unregisteredCount++;
                    report.violationDetails.Add($"[UNREGISTERED] {label}");
                    break;
                case RegistrationStatus.Contraband:
                    report.contrabandCount++;
                    report.violationDetails.Add($"[CONTRABAND] {label}");
                    break;
            }
        }

        private void HandleCleared(ScanReport report)
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);

            SetBeamColor(clearedColor);
            SetStatusLightColor(Color.green);

            Debug.Log($"<color=#00FFAA>[Municipal Scanner: {checkpointName}]</color> <b>✔ CLEARED!</b> All {report.legalCount} biometrics legal. Access granted to {sectorName}.");
            OnScanCleared?.Invoke(this, report);

            if (stationedEnforcer != null)
            {
                stationedEnforcer.HandleScannerCleared(this, report);
            }
        }

        private void HandleViolation(ScanReport report)
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseBeamRoutine(violationColor));

            SetStatusLightColor(Color.red);

            Debug.LogWarning($"<color=#FF3344>[Municipal Scanner: {checkpointName}]</color> <b>⚠ VIOLATION DETECTED!</b> " +
                             $"Unregistered: {report.unregisteredCount}, Contraband: {report.contrabandCount}. Breach logged!");

            OnScanViolation?.Invoke(this, report);

            if (stationedEnforcer != null)
            {
                stationedEnforcer.HandleScannerViolation(this, report);
            }
        }

        private IEnumerator PulseBeamRoutine(Color targetColor)
        {
            float elapsed = 0f;
            while (elapsed < 3f)
            {
                float t = (Mathf.Sin(Time.time * 12f) + 1f) * 0.5f;
                SetBeamColor(Color.Lerp(targetColor * 0.4f, targetColor, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetBeamColor(targetColor);
        }

        private IEnumerator ResetAfterDelayRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            SetBeamColor(idleColor);
            SetStatusLightColor(new Color(0.95f, 0.75f, 0.1f, 1f));
        }

        private void SetBeamColor(Color color)
        {
            if (beamRenderer != null)
            {
                beamRenderer.color = color;
            }
        }

        private void SetStatusLightColor(Color color)
        {
            if (statusLight != null) statusLight.color = color;
            if (statusLights != null)
            {
                for (int i = 0; i < statusLights.Length; i++)
                {
                    if (statusLights[i] != null) statusLights[i].color = color;
                }
            }
        }

        public void SetStatusLights(SpriteRenderer[] lights)
        {
            statusLights = lights;
        }

        public void SetReferences(SpriteRenderer beam, SpriteRenderer light, MunicipalEnforcerAI2D enforcer)
        {
            beamRenderer = beam;
            statusLight = light;
            stationedEnforcer = enforcer;
            SetBeamColor(idleColor);
        }
    }
}
