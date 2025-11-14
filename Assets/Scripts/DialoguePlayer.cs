using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class DialoguePlayer : MonoBehaviour
{
    [System.Serializable]
    public struct Section
    {
        [TextArea(2, 6)]
        public string text;

        [Tooltip("Section start time (seconds from beginning of this entry's clip).")]
        public float startTime;

        [Tooltip("Display length. If <= 0, it lasts until the next section, or until entry end for the last section.")]
        public float duration;

        [Header("Events (optional)")]
        public UnityEvent onSectionStart;
        public UnityEvent onSectionComplete;
    }

    [System.Serializable]
    public struct DialogueEntry
    {
        [Tooltip("Speaker shown in the name UI.")]
        public string speakerName;

        [Tooltip("Used only if Sections is empty (becomes a single section at t=0).")]
        [TextArea(2, 6)] public string dialogueText;

        [Tooltip("The audio clip for the whole entry. Will play straight through from t=0.")]
        public AudioClip audioClip;

        [Tooltip("Timed sections that change the text (do not trim/seek the audio).")]
        public Section[] sections;

        [Header("Entry Events")]
        public UnityEvent onEntryStart;
        public UnityEvent onEntryComplete;
    }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private RectTransform layoutRoot;  // parent with VerticalLayoutGroup (your Textbox)

    [Header("Global Events")]
    [SerializeField] private UnityEvent onEnd;

    [Header("Content")]
    [SerializeField] private DialogueEntry[] entries;

    [Header("Options")]
    [Tooltip("If checked, the dialogue starts from the beginning automatically on Start().")]
    public bool isFromStart = true;

    [Tooltip("Hide the panel when dialogue finishes.")]
    [SerializeField] private bool hidePanelOnEnd = true;

    [Tooltip("Fallbacks when there is no audio.")]
    [SerializeField] private float defaultTextOnlyDuration = 3f;

    private AudioSource audioSource;
    private int entryIndex = 0;
    private int sectionIndex = 0;           // current section inside the entry (for events)
    private bool isRunning = false;

    private Coroutine entryFlow;            // drives section timing per entry
    private float entryStartTime = 0f;      // Time.time when current entry started
    private bool entryManuallySkipped = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (nextButton != null)
            nextButton.onClick.AddListener(Next); // Next now skips to next ENTRY

        if (panel != null && hidePanelOnEnd)
            panel.SetActive(false);
    }

    private void Start()
    {
        if (isFromStart)
        {
            entryIndex = 0;
            sectionIndex = 0;
            StartCoroutine(DelayOnStart(2f));
        }
    }

    public void SetRunning()
    {
        isRunning = false;
        entryIndex = 0;
        sectionIndex = 0;
    }

    IEnumerator DelayOnStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        Trigger();
    }

    /// Begin (or resume) the dialogue.
    public void Trigger()
    {
        if (entries == null || entries.Length == 0)
        {
            Debug.LogWarning($"{name}: No dialogue entries assigned.");
            return;
        }

        isRunning = true;

        if (panel != null)
            panel.SetActive(true);

        StartEntry(entryIndex);
    }

    /// Next button now SKIPS to NEXT ENTRY (ignores remaining sections).
    public void Next()
    {
        if (!isRunning) return;

        // Mark manual skip so the coroutine doesn't also auto-advance callbacks twice
        entryManuallySkipped = true;

        // Complete current section + entry events before skipping
        var entry = entries[entryIndex];
        var secs = GetSections(entry);

        // section complete (if we already started one)
        if (sectionIndex >= 0 && sectionIndex < secs.Length)
            secs[sectionIndex].onSectionComplete?.Invoke();

        // entry complete
        entry.onEntryComplete?.Invoke();

        // jump to next entry
        GoToNextEntry();
    }

    /// Skips the rest of the current entry and begins the next (public helper).
    public void SkipToNextEntry() => Next();

    // -------------------- Entry lifecycle --------------------

    private void StartEntry(int index)
    {
        if (index < 0 || index >= entries.Length)
        {
            EndDialogue();
            return;
        }

        var entry = entries[index];
        var secs = GetSections(entry);

        // Reset for new entry
        sectionIndex = 0;
        entryManuallySkipped = false;

        // UI: speaker name now
        if (playerNameText) playerNameText.text = entry.speakerName;

        // Set first section's text immediately if its startTime == 0 (nice UX)
        if (secs.Length > 0 && Mathf.Approximately(secs[0].startTime, 0f))
        {
            if (dialogueText) dialogueText.text = secs[0].text;
            // We'll still invoke onSectionStart at t=0 from the coroutine below
        }
        else
        {
            // clear or keep previous? We'll clear to avoid flicker from old text
            if (dialogueText) dialogueText.text = string.Empty;
        }

        // Layout refresh for initial state
        StartCoroutine(Co_RebuildLayoutNextFrame());

        // Start audio ONCE for this entry (no seeking for sections)
        if (audioSource != null)
        {
            audioSource.Stop();
            if (entry.audioClip != null)
            {
                audioSource.clip = entry.audioClip;
                audioSource.time = 0f; // start from beginning
                audioSource.Play();
            }
            else
            {
                audioSource.clip = null;
            }
        }

        // Fire entry start
        entry.onEntryStart?.Invoke();

        // Drive section timing & auto-advance
        if (entryFlow != null) StopCoroutine(entryFlow);
        entryFlow = StartCoroutine(EntryFlowCoroutine(entry));
    }

    private IEnumerator EntryFlowCoroutine(DialogueEntry entry)
    {
        entryStartTime = Time.time;

        var secs = GetSections(entry);
        float clipLen = entry.audioClip ? entry.audioClip.length : -1f;

        // Show each section at its startTime (relative to entry start)
        for (int i = 0; i < secs.Length; i++)
        {
            var s = secs[i];

            // Wait until this section's start time
            yield return WaitUntilEntryElapsed(s.startTime);
            if (entryManuallySkipped) yield break; // Next() pressed while waiting

            // Start section
            sectionIndex = i;
            if (dialogueText) dialogueText.text = s.text;
            StartCoroutine(Co_RebuildLayoutNextFrame());
            s.onSectionStart?.Invoke();

            // Determine when this section completes (for its onComplete)
            float sectionEnd = ComputeSectionEndForDisplay(s, secs, i, clipLen);
            float nowElapsed = Time.time - entryStartTime;
            float remain = Mathf.Max(0f, sectionEnd - nowElapsed);

            // Wait until the section display should complete (unless we hit manual skip)
            if (remain > 0f)
            {
                yield return new WaitForSeconds(remain);
                if (entryManuallySkipped) yield break;
            }

            s.onSectionComplete?.Invoke();
        }

        // After last section, wait until the entry end (clip length or fallback)
        float entryEnd = ComputeEntryEndTime(entry, secs, clipLen);
        float elapsed = Time.time - entryStartTime;
        if (entryEnd > elapsed)
        {
            yield return new WaitForSeconds(entryEnd - elapsed);
            if (entryManuallySkipped) yield break;
        }

        // Auto-complete entry
        entry.onEntryComplete?.Invoke();
        GoToNextEntry();
    }

    private void GoToNextEntry()
    {
        // Stop audio for cleanliness (optional)
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        entryIndex++;
        if (entryIndex >= entries.Length)
        {
            EndDialogue();
            return;
        }

        StartEntry(entryIndex);
    }

    // -------------------- Timing helpers --------------------

    private IEnumerator WaitUntilEntryElapsed(float targetElapsed)
    {
        // Use Time.time relative to entry start so it's robust with/without audio
        while ((Time.time - entryStartTime) < targetElapsed && !entryManuallySkipped)
            yield return null;
    }

    // Section display end: if a duration is specified, use it; else next section start; else clip end; else fallback
    private float ComputeSectionEndForDisplay(Section current, Section[] all, int idx, float clipLen)
    {
        if (current.duration > 0f)
            return current.startTime + current.duration;

        bool hasNext = (idx + 1) < all.Length;
        if (hasNext) return Mathf.Max(current.startTime, all[idx + 1].startTime);

        if (clipLen > 0f) return clipLen;

        return current.startTime + Mathf.Max(0.01f, defaultTextOnlyDuration);
    }

    // Entry end: prefer clip length; else last section's end; else default
    private float ComputeEntryEndTime(DialogueEntry entry, Section[] secs, float clipLen)
    {
        if (clipLen > 0f) return clipLen;

        if (secs.Length > 0)
        {
            var last = secs[secs.Length - 1];
            return ComputeSectionEndForDisplay(last, secs, secs.Length - 1, clipLen);
        }

        // No sections: show fallback duration
        return Mathf.Max(0.01f, defaultTextOnlyDuration);
    }

    private Section[] GetSections(DialogueEntry entry)
    {
        if (entry.sections != null && entry.sections.Length > 0)
            return entry.sections;

        if (!string.IsNullOrWhiteSpace(entry.dialogueText))
        {
            float length = entry.audioClip ? entry.audioClip.length : Mathf.Max(0.01f, defaultTextOnlyDuration);
            return new Section[]
            {
                new Section { text = entry.dialogueText, startTime = 0f, duration = length }
            };
        }

        return new Section[0];
    }

    // -------------------- End & layout --------------------

    private void EndDialogue()
    {
        isRunning = false;

        if (entryFlow != null)
        {
            StopCoroutine(entryFlow);
            entryFlow = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        onEnd?.Invoke();

        if (hidePanelOnEnd && panel != null)
            panel.SetActive(false);

        if (nextButton != null)
            nextButton.interactable = false;
    }

    private void RebuildLayoutImmediate()
    {
        if (!layoutRoot) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

        var groups = layoutRoot.GetComponentsInChildren<LayoutGroup>(true);
        for (int i = 0; i < groups.Length; i++)
        {
            var rt = groups[i].transform as RectTransform;
            if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator Co_RebuildLayoutNextFrame()
    {
        yield return null;

        if (playerNameText) playerNameText.ForceMeshUpdate();
        if (dialogueText) dialogueText.ForceMeshUpdate();

        RebuildLayoutImmediate();

        yield return null;
        RebuildLayoutImmediate();
    }

    // ---- Optional helper -----------------------------------------------------
    public void RestartFromBeginning()
    {
        entryIndex = 0;
        sectionIndex = 0;
        Trigger();
    }
}
