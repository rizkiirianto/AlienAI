using System;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class DirectorAI : MonoBehaviour
{
    public enum MenaceState
    {
        Calm,
        Suspicious,
        Hunting,
        Critical
    }

    [Header("Menace Gauge")]
    [SerializeField] private float menaceGauge = 0f;
    [SerializeField] private TextMeshProUGUI menaceGaugeText;
    [SerializeField] private float maxMenace = 100f;
    [SerializeField] private float menaceDecayPerSecond = 6f;

    [Header("Menace Gain Rates")]
    [SerializeField] private float directSightGainPerSecond = 25f;
    [SerializeField] private float runningNoiseGainPerSecond = 4f;
    [SerializeField] private float proximityGainPerSecond = 6f;
    [SerializeField] private float proximityRange = 8f;
    [SerializeField] private float runningSpeedThreshold = 4.5f;

    [Header("Pacing")]
    [SerializeField] private float calmPressureDelay = 18f;
    [SerializeField] private float calmPressureGainPerSecond = 2f;
    [SerializeField] private float carefulPlayBonusDecayPerSecond = 2f;

    [Header("Relief")]
    [SerializeField] private float spottedSpikeAmount = 8f;
    [SerializeField] private float lostTrackReliefAmount = 10f;
    [SerializeField] private float highIntensityThreshold = 85f;
    [SerializeField] private float maxHighIntensityDuration = 12f;
    [SerializeField] private float highIntensityReliefPerSecond = 8f;
    [SerializeField] private float breathingRoomDuration = 5f;
    [SerializeField] private float breathingRoomDecayBonusPerSecond = 4f;

    [Header("Hiding Pattern")]
    [SerializeField] private float repeatedHidingBaseGain = 6f;
    [SerializeField] private float sameSpotExtraGain = 4f;

    [Header("State Thresholds")]
    [SerializeField] private float suspiciousThreshold = 25f;
    [SerializeField] private float huntingThreshold = 60f;
    [SerializeField] private float criticalThreshold = 85f;

    [Header("Hunter Speed By State")]
    [SerializeField] private float huntingSpeedBonus = 2f;
    [SerializeField] private float criticalSpeedBonus = 4f;
    [SerializeField] private float criticalSpeedAbovePlayer = 0.5f;

    private Transform player;
    private Transform hunter;
    private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;
    private NavMeshAgent hunterAgent;
    private float baseHunterSpeed;
    private bool hasBaseHunterSpeed;

    private bool directSightActive;
    private MenaceState currentState = MenaceState.Calm;
    private float calmTimer;
    private float highIntensityTimer;
    private float breathingRoomTimer;
    private int lastHidingIndex = -1;
    private int sameHidingStreak;

    public float MenaceGauge => menaceGauge;
    public MenaceState CurrentState => currentState;

    public event Action<MenaceState, MenaceState> OnMenaceStateChanged;

    private void Start()
    {
        ResolveReferences();
        RefreshMenaceText();
        ApplyHunterSpeedForCurrentState();
    }

    private void FixedUpdate()
    {
        ResolveReferences();

        float dt = Time.fixedDeltaTime;
        float playerDistance = GetDistanceToPlayer();
        bool playerRunning = IsPlayerRunning();
        bool proximityHigh = playerDistance <= proximityRange;
        bool threatNow = directSightActive || proximityHigh || playerRunning;

        if (directSightActive)
            AddMenace(directSightGainPerSecond * dt);

        if (playerRunning)
            AddMenace(runningNoiseGainPerSecond * dt);

        if (proximityHigh)
        {
            float proximity01 = Mathf.Clamp01(1f - (playerDistance / proximityRange));
            AddMenace((proximityGainPerSecond * (1f + proximity01)) * dt);
        }

        UpdateCalmPressure(threatNow, dt);
        UpdateReliefSystems(threatNow, playerRunning, proximityHigh, dt);

        EvaluateState();
    }

    public void SetDirectSight(bool isVisible)
    {
        if (isVisible == directSightActive)
            return;

        if (isVisible)
            AddMenace(spottedSpikeAmount);
        else
            ReduceMenace(lostTrackReliefAmount);

        directSightActive = isVisible;
    }

    public void ReportHidingSpotUsed(int index)
    {
        if (index == lastHidingIndex)
            sameHidingStreak++;
        else
            sameHidingStreak = 1;

        lastHidingIndex = index;

        float extra = (sameHidingStreak - 1) * sameSpotExtraGain;
        AddMenace(repeatedHidingBaseGain + extra);
    }

    public void AddMenace(float amount)
    {
        menaceGauge = Mathf.Clamp(menaceGauge + amount, 0f, maxMenace);
        RefreshMenaceText();
        EvaluateState();
    }

    public void ReduceMenace(float amount)
    {
        menaceGauge = Mathf.Clamp(menaceGauge - amount, 0f, maxMenace);
        RefreshMenaceText();
        EvaluateState();
    }

    public void SetMenace(float value)
    {
        menaceGauge = Mathf.Clamp(value, 0f, maxMenace);
        RefreshMenaceText();
        EvaluateState();
    }

    private void RefreshMenaceText()
    {
        if (menaceGaugeText == null)
            return;

        menaceGaugeText.text = "Menace: " + menaceGauge.ToString("F1") + " / " + maxMenace.ToString("F0");
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerRigidbody = playerObj.GetComponent<Rigidbody>();
                playerMovement = playerObj.GetComponent<PlayerMovement>();
            }
        }

        if (hunter == null)
        {
            GameObject hunterObj = GameObject.Find("Hunter");
            if (hunterObj != null)
            {
                hunter = hunterObj.transform;
                hunterAgent = hunterObj.GetComponent<NavMeshAgent>();
                if (hunterAgent != null && !hasBaseHunterSpeed)
                {
                    baseHunterSpeed = hunterAgent.speed;
                    hasBaseHunterSpeed = true;
                }
                ApplyHunterSpeedForCurrentState();
            }
        }
    }

    private float GetDistanceToPlayer()
    {
        if (player == null || hunter == null)
            return Mathf.Infinity;

        return Vector3.Distance(player.position, hunter.position);
    }

    private bool IsPlayerRunning()
    {
        if (playerRigidbody == null)
            return false;

        Vector3 planarVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);
        return planarVelocity.magnitude >= runningSpeedThreshold;
    }

    private void UpdateCalmPressure(bool threatNow, float dt)
    {
        if (threatNow)
        {
            calmTimer = 0f;
            return;
        }

        calmTimer += dt;
        if (calmTimer >= calmPressureDelay)
            AddMenace(calmPressureGainPerSecond * dt);
    }

    private void UpdateReliefSystems(bool threatNow, bool playerRunning, bool proximityHigh, float dt)
    {
        float decayPerSecond = menaceDecayPerSecond;

        if (!playerRunning && !proximityHigh && !directSightActive)
            decayPerSecond += carefulPlayBonusDecayPerSecond;

        if (menaceGauge >= highIntensityThreshold)
            highIntensityTimer += dt;
        else
            highIntensityTimer = 0f;

        if (highIntensityTimer >= maxHighIntensityDuration)
            decayPerSecond += highIntensityReliefPerSecond;

        if (breathingRoomTimer > 0f)
        {
            breathingRoomTimer -= dt;
            decayPerSecond += breathingRoomDecayBonusPerSecond;
        }

        if (!threatNow)
            ReduceMenace(decayPerSecond * dt);
    }

    private void EvaluateState()
    {
        MenaceState nextState = GetStateForValue(menaceGauge);
        if (nextState == currentState)
            return;

        MenaceState previousState = currentState;
        currentState = nextState;

        if (previousState == MenaceState.Critical && currentState != MenaceState.Critical)
            breathingRoomTimer = breathingRoomDuration;

        ApplyHunterSpeedForCurrentState();

        Debug.Log("[DirectorAI] Menace state changed: " + previousState + " -> " + currentState + " (" + menaceGauge.ToString("F1") + ")");
        OnMenaceStateChanged?.Invoke(previousState, currentState);
    }

    private void ApplyHunterSpeedForCurrentState()
    {
        if (hunterAgent == null || !hasBaseHunterSpeed)
            return;

        float targetSpeed = baseHunterSpeed;

        if (currentState == MenaceState.Hunting)
        {
            targetSpeed = baseHunterSpeed + huntingSpeedBonus;
        }
        else if (currentState == MenaceState.Critical)
        {
            float criticalByBonus = baseHunterSpeed + criticalSpeedBonus;
            float playerSpeedTarget = playerMovement != null ? playerMovement.playerSpeed + criticalSpeedAbovePlayer : criticalByBonus;
            targetSpeed = Mathf.Max(criticalByBonus, playerSpeedTarget);
        }

        hunterAgent.speed = targetSpeed;
    }

    private MenaceState GetStateForValue(float value)
    {
        if (value >= criticalThreshold)
            return MenaceState.Critical;

        if (value >= huntingThreshold)
            return MenaceState.Hunting;

        if (value >= suspiciousThreshold)
            return MenaceState.Suspicious;

        return MenaceState.Calm;
    }
}
