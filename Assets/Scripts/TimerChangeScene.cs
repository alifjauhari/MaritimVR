using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TimerChangeScene : MonoBehaviour
{
    [Serializable]
    public class Trigger
    {
        [Min(0), Tooltip("When to fire (seconds). e.g. 90 = 1:30")]
        public float timeSeconds = 0f;

        [Tooltip("What to do when this time is reached.")]
        public UnityEvent onReached;

        [HideInInspector] public bool fired = false;
    }

    [Header("Clock")]
    [Tooltip("Count time even if Time.timeScale = 0.")]
    public bool useUnscaledTime = false;

    [Tooltip("Automatically stop after the last trigger fires.")]
    public bool stopAfterLastTrigger = false;

    [Tooltip("If true, will loop back to 0 after the last trigger and clear fired flags.")]
    public bool loop = false;

    [Header("Read-only")]
    [SerializeField, Tooltip("Elapsed time since StartTimer() (seconds).")]
    private float elapsed = 0f;

    [Header("Triggers (editable)")]
    public List<Trigger> triggers = new List<Trigger>()
    {
        new Trigger(){ timeSeconds = 90f },   // 1:30
        new Trigger(){ timeSeconds = 180f },  // 3:00
        new Trigger(){ timeSeconds = 240f },  // 4:00
    };

    [SerializeField] private bool onStart = false;

    // Internal
    private bool running = false;

    
    private void Start()
    {
        if (onStart)
            StartTimer();
    }

    // ————— Public API —————

    /// <summary>Start (or resume) the timer from its current elapsed value.</summary>
    public void StartTimer()
    {
        running = true;
    }

    /// <summary>Pause the timer (keeps elapsed).</summary>
    public void PauseTimer()
    {
        running = false;
    }

    /// <summary>Stop and reset to 0. Also clears all trigger fired flags.</summary>
    public void ResetTimer()
    {
        elapsed = 0f;
        running = false;
        for (int i = 0; i < triggers.Count; i++)
            triggers[i].fired = false;
    }

    /// <summary>Start from a custom offset (in seconds), then run.</summary>
    public void StartWithOffset(float offsetSeconds)
    {
        elapsed = Mathf.Max(0f, offsetSeconds);
        // Mark any triggers already passed as fired so they don't retro-fire.
        for (int i = 0; i < triggers.Count; i++)
            triggers[i].fired = triggers[i].timeSeconds <= elapsed;
        running = true;
    }

    /// <summary>Get the current elapsed seconds.</summary>
    public float ElapsedSeconds => elapsed;

    /// <summary>Convenience: returns mm:ss format of elapsed.</summary>
    public string ElapsedAsClock()
    {
        int s = Mathf.FloorToInt(elapsed);
        int m = s / 60;
        s = s % 60;
        return $"{m:00}:{s:00}";
    }

    // ————— Unity —————

    private void Update()
    {
        if (!running) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        float prev = elapsed;
        elapsed += dt;

        // Fire any triggers crossed this frame (robust vs frame skips)
        for (int i = 0; i < triggers.Count; i++)
        {
            var t = triggers[i];
            if (!t.fired && prev < t.timeSeconds && elapsed >= t.timeSeconds)
            {
                t.fired = true;
                try { t.onReached?.Invoke(); }
                catch (Exception e) { Debug.LogException(e, this); }
            }
        }

        // Handle end conditions
        float lastTime = GetLastTriggerTime();
        if (lastTime >= 0f)
        {
            bool allFired = AllFired();
            if (loop && allFired)
            {
                // Loop: wrap elapsed, clear flags, preserve overshoot
                float overshoot = Mathf.Max(0f, elapsed - lastTime);
                elapsed = overshoot; // restart from overshoot so precise
                ClearFired();
            }
            else if (stopAfterLastTrigger && allFired)
            {
                running = false;
            }
        }
    }

    // ————— Helpers —————
    private float GetLastTriggerTime()
    {
        float max = -1f;
        for (int i = 0; i < triggers.Count; i++)
            if (triggers[i].timeSeconds > max) max = triggers[i].timeSeconds;
        return max;
    }
    private bool AllFired()
    {
        for (int i = 0; i < triggers.Count; i++)
            if (!triggers[i].fired) return false;
        return true;
    }
    private void ClearFired()
    {
        for (int i = 0; i < triggers.Count; i++)
            triggers[i].fired = false;
    }

    // Optional: seed defaults again if you click "Reset" in Inspector
    private void Reset()
    {
        triggers = new List<Trigger>()
        {
            new Trigger(){ timeSeconds = 90f },
            new Trigger(){ timeSeconds = 180f },
            new Trigger(){ timeSeconds = 240f },
        };
        useUnscaledTime = false;
        stopAfterLastTrigger = false;
        loop = false;
        elapsed = 0f;
        running = false;
    }

    // Quality of life buttons in the Inspector (right-click component → Context Menu)
    [ContextMenu("Start Timer")] private void _CtxStart() => StartTimer();
    [ContextMenu("Pause Timer")] private void _CtxPause() => PauseTimer();
    [ContextMenu("Reset Timer")] private void _CtxReset() => ResetTimer();
}
