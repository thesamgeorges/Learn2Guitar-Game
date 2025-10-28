using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    [Header("Button Setup")]
    public Button[] buttons = new Button[42];
    public Button strumButton;

    [Header("UI Elements")]
    public TMP_Text currentChordText;
    public TMP_Text feedbackText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.green;

    [Header("Timer")]
    public TMP_Text timerText;
    public TMP_Text bestTimeText;

    [Header("Debug Options")]
    public bool resetBestTime = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cChordSound;
    public AudioClip gChordSound;
    public AudioClip dChordSound;
    public AudioClip errorSound; // plays when chord is wrong

    [Header("Teacher Reactions")]
    public Image teacherImage;
    public Sprite teacherNeutral;
    public Sprite teacherHappy;
    public Sprite teacherSad;

    private float currentTime = 0f;
    private bool timerRunning = false;
    private float bestTime = Mathf.Infinity;

    private HashSet<int> activeButtons = new HashSet<int>();
    private List<string> chordSequence = new List<string> { "C", "G", "D" };
    private int currentChordIndex = 0;

    void Start()
    {
        // setup button listeners
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            if (buttons[index] != null)
            {
                SetButtonColor(buttons[index], normalColor);
                buttons[index].onClick.AddListener(() => OnButtonClicked(index));
            }
        }

        // setup strum button
        if (strumButton != null)
            strumButton.onClick.AddListener(OnStrum);

        UpdateChordDisplay();

        if (feedbackText != null)
            feedbackText.text = "";

        // load best time
        if (PlayerPrefs.HasKey("BestTime"))
        {
            bestTime = PlayerPrefs.GetFloat("BestTime");
            UpdateBestTimeDisplay();
        }
        else if (bestTimeText != null)
        {
            bestTimeText.text = "Best: --";
        }

        // set teacher to neutral at start
        SetTeacherExpression("neutral");

        StartTimer();
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }

        // allow resetting best time manually from the Inspector
        if (resetBestTime)
        {
            resetBestTime = false; // uncheck automatically
            ResetBestTimeNow();
            Debug.Log("Best time reset manually from Inspector.");
        }
    }

    void OnButtonClicked(int index)
    {
        if (activeButtons.Contains(index))
        {
            activeButtons.Remove(index);
            SetButtonColor(buttons[index], normalColor);
        }
        else
        {
            activeButtons.Add(index);
            SetButtonColor(buttons[index], pressedColor);
        }

        PrintCurrentCombo();
    }

    void OnStrum()
    {
        string targetChord = chordSequence[currentChordIndex];

        if (IsChordCorrect(targetChord))
        {
            Debug.Log($"Correct! You played {targetChord}");
            StartCoroutine(ShowFeedback($"Correct! You played {targetChord}", Color.green));
            SetTeacherExpression("happy");

            // play the correct chord sound
            PlayChordSound(targetChord);

            currentChordIndex++;

            if (currentChordIndex >= chordSequence.Count)
            {
                StopTimer();

                feedbackText.text = "You completed all chords!";
                currentChordText.text = "All chords complete!";

                if (currentTime < bestTime)
                {
                    bestTime = currentTime;
                    PlayerPrefs.SetFloat("BestTime", bestTime);
                    PlayerPrefs.Save();
                    StartCoroutine(ShowFeedback("New Best Time!", Color.yellow));
                }

                UpdateBestTimeDisplay();
                return;
            }

            activeButtons.Clear();
            ResetButtonColors();
            UpdateChordDisplay();
        }
        else
        {
            Debug.Log($"That’s not {targetChord}, try again!");
            StartCoroutine(ShowFeedback($"That’s not {targetChord}, try again!", Color.red));
            SetTeacherExpression("sad");

            // play error sound
            PlayErrorSound();
        }
    }

    void PlayChordSound(string chord)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned to ButtonManager!");
            return;
        }

        switch (chord)
        {
            case "C":
                if (cChordSound != null)
                    audioSource.PlayOneShot(cChordSound);
                break;
            case "G":
                if (gChordSound != null)
                    audioSource.PlayOneShot(gChordSound);
                break;
            case "D":
                if (dChordSound != null)
                    audioSource.PlayOneShot(dChordSound);
                break;
            default:
                Debug.LogWarning("No sound assigned for chord: " + chord);
                break;
        }
    }

    void PlayErrorSound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned to ButtonManager!");
            return;
        }

        if (errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
        else
        {
            Debug.LogWarning("No error sound assigned.");
        }
    }

    void SetTeacherExpression(string mood)
    {
        if (teacherImage == null) return;

        switch (mood)
        {
            case "happy":
                if (teacherHappy != null)
                    teacherImage.sprite = teacherHappy;
                break;
            case "sad":
                if (teacherSad != null)
                    teacherImage.sprite = teacherSad;
                break;
            default:
                if (teacherNeutral != null)
                    teacherImage.sprite = teacherNeutral;
                break;
        }

        // return to neutral face after 2 seconds
        StartCoroutine(ResetTeacherAfterDelay());
    }

    IEnumerator ResetTeacherAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (teacherImage != null && teacherNeutral != null)
            teacherImage.sprite = teacherNeutral;
    }

    bool IsChordCorrect(string chord)
    {
        switch (chord)
        {
            case "C":
                return activeButtons.SetEquals(new HashSet<int> { 24, 8, 13, 36 });
            case "D":
                return activeButtons.SetEquals(new HashSet<int> { 19, 31, 26, 36, 37 });
            case "G":
                return activeButtons.SetEquals(new HashSet<int> { 32, 7, 2 });
            default:
                return false;
        }
    }

    void UpdateChordDisplay()
    {
        if (currentChordText != null)
            currentChordText.text = "Play chord: " + chordSequence[currentChordIndex];
    }

    void ResetButtonColors()
    {
        foreach (var btn in buttons)
        {
            if (btn != null)
                SetButtonColor(btn, normalColor);
        }
    }

    void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;

        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.selectedColor = color;
        button.colors = cb;
    }

    void PrintCurrentCombo()
    {
        if (activeButtons.Count == 0)
        {
            Debug.Log("No buttons pressed.");
            return;
        }

        string combo = string.Join(", ", activeButtons);
        Debug.Log("Current combo: " + combo);
    }

    void StartTimer()
    {
        currentTime = 0f;
        timerRunning = true;
        UpdateTimerDisplay();
    }

    void StopTimer()
    {
        timerRunning = false;
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
            timerText.text = "Time: " + currentTime.ToString("F2") + "s";
    }

    void UpdateBestTimeDisplay()
    {
        if (bestTimeText != null)
            bestTimeText.text = "Best: " + bestTime.ToString("F2") + "s";
    }

    IEnumerator ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            yield break;

        feedbackText.text = message;
        feedbackText.color = color;

        yield return new WaitForSeconds(2f);

        for (float t = 0; t < 1; t += Time.deltaTime / 1f)
        {
            if (feedbackText == null) yield break;
            feedbackText.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        feedbackText.text = "";
        feedbackText.alpha = 1;
    }

    public void ResetBestTimeNow()
    {
        PlayerPrefs.DeleteKey("BestTime");
        bestTime = Mathf.Infinity;
        if (bestTimeText != null)
            bestTimeText.text = "Best: --";
    }
}
