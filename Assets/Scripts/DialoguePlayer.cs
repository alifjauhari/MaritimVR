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

        [Tooltip("When this section starts (seconds from the clip's beginning).")]
        public float startTime;

        [Tooltip("If <= 0, ends at the next section's start, or at clip.length for the last section.")]
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

        [Tooltip("Optional: legacy single text (used only if Sections is empty).")]
        [TextArea(2, 6)] public string dialogueText;

        [Tooltip("The single audio clip for this entry (can be one long clip).")]
        public AudioClip audioClip;

        [Tooltip("Timed sections inside the clip.")]
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
    [SerializeField] private RectTransform layoutRoot;  // e.g., Textbox (with VerticalLayoutGroup)

    [Header("Global Events")]
    [SerializeField] private UnityEvent onEnd;

    [Header("Content")]
    [SerializeField] private DialogueEntry[] entries;

    [Header("Options")]
    [Tooltip("If checked: starts automatically from the beginning on Start(). If unchecked: wait for Trigger().")]
    public bool isFromStart = true;

    [Tooltip("Hide the panel when dialogue finishes.")]
    [SerializeField] private bool hidePanelOnEnd = true;

    [Tooltip("Used when there is no audio and a section has no duration.")]
    [SerializeField] private float defaultTextOnlyDuration = 3f;

    private AudioSource audioSource;
    private int entryIndex = 0;
    private int sectionIndex = 0;
    private bool isRunning = false;
    private Coroutine sectionRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (nextButton != null)
            nextButton.onClick.AddListener(Next);

        if (panel != null && hidePanelOnEnd)
            panel.SetActive(false);
    }

    private void Start()
    {
        if (isFromStart)
        {
            entryIndex = 0;
            sectionIndex = 0;
            Trigger();
        }
    }

    /// Begins (or resumes) the dialogue flow.
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

        StartCoroutine(Co_RebuildLayoutNextFrame());
        ShowCurrentSection();
    }

    /// Next button handler: go to next section; if none, next entry; if none, end.
    public void Next()
    {
        if (!isRunning) return;

        // Complete current section event (if any)
        var currentEntry = entries[entryIndex];
        var secs = GetSections(currentEntry);
        if (sectionIndex >= 0 && sectionIndex < secs.Length)
            secs[sectionIndex].onSectionComplete?.Invoke();

        sectionIndex++;

        if (sectionIndex >= secs.Length)
        {
            // Entry complete
            currentEntry.onEntryComplete?.Invoke();

            entryIndex++;
            sectionIndex = 0;

            if (entryIndex >= entries.Length)
            {
                EndDialogue();
                return;
            }
        }

        ShowCurrentSection();
    }

    /// Skip immediately to the next entry (completes current section + entry events).
    public void SkipToNextEntry()
    {
        if (!isRunning) return;

        var currentEntry = entries[entryIndex];
        var secs = GetSections(currentEntry);

        // complete current section (if any)
        if (sectionIndex >= 0 && sectionIndex < secs.Length)
            secs[sectionIndex].onSectionComplete?.Invoke();

        // complete current entry
        currentEntry.onEntryComplete?.Invoke();

        entryIndex++;
        sectionIndex = 0;

        if (entryIndex >= entries.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentSection();
    }

    private void ShowCurrentSection()
    {
        if (entryIndex < 0 || entryIndex >= entries.Length) return;

        var entry = entries[entryIndex];
        var secs = GetSections(entry);

        if (secs.Length == 0)
        {
            // treat empty entry as instant start/complete, then end or advance
            entry.onEntryStart?.Invoke();
            entry.onEntryComplete?.Invoke();

            entryIndex++;
            sectionIndex = 0;

            if (entryIndex >= entries.Length)
            {
                EndDialogue();
                return;
            }
            entry = entries[entryIndex];
            secs = GetSections(entry);
        }

        // Clamp section index
        sectionIndex = Mathf.Clamp(sectionIndex, 0, secs.Length - 1);
        var sec = secs[sectionIndex];

        // Entry start event only when we hit section 0
        if (sectionIndex == 0)
            entry.onEntryStart?.Invoke();

        // Update UI
        if (playerNameText) playerNameText.text = entry.speakerName;
        if (dialogueText) dialogueText.text = sec.text;

        // Rebuild layout after text changed
        if (sectionRoutine != null) StopCoroutine(sectionRoutine);
        StartCoroutine(Co_RebuildLayoutNextFrame());

        // Prepare audio window
        float clipLen = entry.audioClip ? entry.audioClip.length : 0f;
        float start = Mathf.Max(0f, sec.startTime);
        float end = ComputeSectionEnd(sec, secs, sectionIndex, clipLen);
        float dur = Mathf.Max(0.01f, end - start);

        if (audioSource != null)
        {
            audioSource.Stop();
            if (entry.audioClip != null)
            {
                if (audioSource.clip != entry.audioClip)
                    audioSource.clip = entry.audioClip;

                float safeStart = clipLen > 0f ? Mathf.Min(start, Mathf.Max(0f, clipLen - 0.01f)) : 0f;
                audioSource.time = safeStart;
                audioSource.Play();
            }
            else
            {
                audioSource.clip = null;
            }
        }

        if (nextButton != null) nextButton.interactable = true;

        // Section start event (after the text/audio are positioned)
        sec.onSectionStart?.Invoke();

        sectionRoutine = StartCoroutine(AutoAdvanceAfter(dur));
    }

    private float ComputeSectionEnd(Section current, Section[] all, int idx, float clipLen)
    {
        if (current.duration > 0f)
            return current.startTime + current.duration;

        bool hasNext = (idx + 1) < all.Length;
        if (hasNext) return Mathf.Max(current.startTime, all[idx + 1].startTime);

        if (clipLen > 0f) return clipLen;

        return current.startTime + Mathf.Max(0.01f, defaultTextOnlyDuration);
    }

    private Section[] GetSections(DialogueEntry entry)
    {
        if (entry.sections != null && entry.sections.Length > 0)
            return entry.sections;

        if (string.IsNullOrWhiteSpace(entry.dialogueText))
            return new Section[0];

        float length = entry.audioClip ? entry.audioClip.length : Mathf.Max(0.01f, defaultTextOnlyDuration);
        return new Section[]
        {
            new Section { text = entry.dialogueText, startTime = 0f, duration = length }
        };
    }

    private IEnumerator AutoAdvanceAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (!isRunning) yield break;
        Next();
    }

    private void EndDialogue()
    {
        isRunning = false;

        if (sectionRoutine != null)
        {
            StopCoroutine(sectionRoutine);
            sectionRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        // fire global end AFTER last entry completed
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
