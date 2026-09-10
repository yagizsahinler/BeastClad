using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BeastClad.Security;

namespace BeastClad.UI
{
    /// <summary>
    /// Screen-space HUD notification banner for municipal checkpoint scans.
    /// Displays security clearance, violation alerts, and municipal Heat updates.
    /// </summary>
    [DisallowMultipleComponent]
    public class MunicipalScannerHUD : MonoBehaviour
    {
        [Header("Banner Root")]
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private Image bannerBg;
        [SerializeField] private Image borderLine;

        [Header("Text Fields")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text detailsText;
        [SerializeField] private Text heatBadgeText;

        [Header("Color Schemes")]
        [SerializeField] private Color clearedBgColor = new Color(0.02f, 0.18f, 0.08f, 0.95f);
        [SerializeField] private Color clearedBorderColor = new Color(0f, 1f, 0.53f, 1f);

        [SerializeField] private Color violationBgColor = new Color(0.27f, 0.04f, 0.04f, 0.96f);
        [SerializeField] private Color violationBorderColor = new Color(1f, 0.15f, 0.2f, 1f);

        private Coroutine hideRoutine;

        private void Awake()
        {
            if (bannerRoot != null)
            {
                bannerRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            MunicipalScannerZone.OnScanCleared += HandleScanCleared;
            MunicipalScannerZone.OnScanViolation += HandleScanViolation;
            MunicipalScannerZone.OnScannerExited += HandleScannerExited;
        }

        private void OnDisable()
        {
            MunicipalScannerZone.OnScanCleared -= HandleScanCleared;
            MunicipalScannerZone.OnScanViolation -= HandleScanViolation;
            MunicipalScannerZone.OnScannerExited -= HandleScannerExited;
        }

        private void HandleScanCleared(MunicipalScannerZone scanner, ScanReport report)
        {
            if (bannerRoot == null) return;
            bannerRoot.SetActive(true);

            if (bannerBg != null) bannerBg.color = clearedBgColor;
            if (borderLine != null) borderLine.color = clearedBorderColor;

            if (titleText != null) titleText.text = $"CHECKPOINT: {scanner.CheckpointName.ToUpper()}";
            if (statusText != null) statusText.text = "<color=#00FFAA>✔ ACCESS GRANTED: ALL BIOMETRICS LICENSED</color>";
            if (detailsText != null) detailsText.text = $"Sector Clearance: <b>{scanner.SectorName}</b>. Zero contraband detected.";
            if (heatBadgeText != null) heatBadgeText.text = "<color=#00FFAA>HEAT: CLEAN (0)</color>";

            RestartHideTimer(3.5f);
        }

        private void HandleScanViolation(MunicipalScannerZone scanner, ScanReport report)
        {
            if (bannerRoot == null) return;
            bannerRoot.SetActive(true);

            if (bannerBg != null) bannerBg.color = violationBgColor;
            if (borderLine != null) borderLine.color = violationBorderColor;

            string violationHeader = report.contrabandCount > 0 ? "⚠ LEVEL 2 ALERT: CONTRABAND BREACH" : "⚠ LEVEL 1 WARNING: UNREGISTERED WEAPON";
            string heatStr = report.contrabandCount > 0 ? "<color=#FF0044>HEAT LEVEL: WANTED (+2)</color>" : "<color=#FFCC00>HEAT LEVEL: SUSPICIOUS (+1)</color>";

            if (titleText != null) titleText.text = $"SECURITY BREACH: {scanner.CheckpointName.ToUpper()}";
            if (statusText != null) statusText.text = $"<color=#FF3344>{violationHeader}</color>";

            if (detailsText != null)
            {
                string firstDetail = report.violationDetails.Count > 0 ? report.violationDetails[0] : "Illegal biological gear detected.";
                detailsText.text = $"{firstDetail} (Total Violations: {report.violationDetails.Count})";
            }

            if (heatBadgeText != null) heatBadgeText.text = heatStr;

            RestartHideTimer(4.5f);
        }

        private void HandleScannerExited(MunicipalScannerZone scanner)
        {
            RestartHideTimer(2.0f);
        }

        private void RestartHideTimer(float delay)
        {
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideBannerRoutine(delay));
        }

        private IEnumerator HideBannerRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bannerRoot != null)
            {
                bannerRoot.SetActive(false);
            }
        }

        public void SetReferences(
            GameObject root, Image bg, Image border,
            Text title, Text status, Text details, Text heat)
        {
            bannerRoot = root;
            bannerBg = bg;
            borderLine = border;
            titleText = title;
            statusText = status;
            detailsText = details;
            heatBadgeText = heat;
        }
    }
}
