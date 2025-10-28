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

    // chord audios
    public AudioClip cChordSound;
    public AudioClip gChordSound;
    public AudioClip dChordSound;
    public AudioClip aMinorSound;
    public AudioClip eMinorSound;
    public AudioClip dMinorSound;

    // note audios for hard mode
    public AudioClip noteCSound;
    public AudioClip noteGSound;
    public AudioClip noteFSound;
    public AudioClip noteDSound;
    public AudioClip noteASound;

    // broken string sound for erorrs
    public AudioClip errorSound;

    [Header("Teacher Reactions")] // animate teacher character
    public Image teacherImage;

    public Sprite teacherNeutral;
    public Sprite teacherHappy;
    public Sprite teacherSad;

    [Header("Speech Bubble")] // teacher's speech bubble
    public Image speechBubble;

    [Header("Hard Mode Visuals")] // pictures of the notes on the staff
    public Image staffNoteImage;
    public Sprite noteC;
    public Sprite noteG;
    public Sprite noteD;
    public Sprite noteA;
    public Sprite noteF;

    // timer vars
    private float currentTime = 0f;
    private bool timerRunning = false;
    private float bestTime = Mathf.Infinity;

    private HashSet<int> activeButtons = new HashSet<int>();
    private List<string> chordSequence = new List<string>();
    private int currentChordIndex = 0;
    private string gameMode = "Easy";

    void Start()
    {
        // load selected game mode from main menu
        gameMode = PlayerPrefs.GetString("GameMode", "Easy");
        Debug.Log("Game Mode: " + gameMode);

        // set chord/note list by mode chosen
        if (gameMode == "Easy")
        {
            chordSequence = new List<string>
            {
                "C Major",
                "D Major",
                "D Minor",
                "G Major",
                "A Minor",
                "E Minor"
            };
        }
        else if (gameMode == "Hard")
        {
            chordSequence = new List<string>
            {
                "Note C",
                "Note G",
                "Note F",
                "Note D",
                "Note A"
            };
        }

        // make buttons change colors/clickable
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            if (buttons[index] != null)
            {
                SetButtonColor(buttons[index], normalColor);
                buttons[index].onClick.AddListener(() => OnButtonClicked(index));
            }
        }

        // strum button
        if (strumButton != null)
            strumButton.onClick.AddListener(OnStrum);

        UpdateChordDisplay();

        if (feedbackText != null)
            feedbackText.text = "";

        // hide speech bubble until teacher speaks
        if (speechBubble != null)
            speechBubble.enabled = false;

        // load in best record time
        if (PlayerPrefs.HasKey("BestTime"))
        {
            bestTime = PlayerPrefs.GetFloat("BestTime");
            UpdateBestTimeDisplay();
        }
        else if (bestTimeText != null)
        {
            bestTimeText.text = "Best: --";
        }

        // make the teacher have the neutral face at the start
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

        if (resetBestTime)
        {
            resetBestTime = false;
            ResetBestTimeNow();
            Debug.Log("Best time reset manually from Inspector.");
        }
    }

    // change colors when clicked
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
            SetTeacherExpression("happy"); // make teachr happy emote
            PlaySound(targetChord);

            currentChordIndex++;

            if (currentChordIndex >= chordSequence.Count)
            {
                StopTimer();

                feedbackText.text = "You completed all chords!";
                currentChordText.text = "All chords complete!";

                // hide staff pic when finished during hard mode
                if (gameMode == "Hard" && staffNoteImage != null)
                    staffNoteImage.enabled = false; // no staff pic in easy mode

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
            StartCoroutine(ShowFeedback($"That’s not {targetChord}, try again!", Color.red)); // teacher error msg
            SetTeacherExpression("sad"); // make him emote sad
            PlayErrorSound();
        }
    }

    bool IsChordCorrect(string chord)
    {
        switch (chord)
        {
            // easy mode chords
            case "C Major":
                return activeButtons.SetEquals(new HashSet<int> { 24, 8, 13, 36 });

            case "D Major":
                return activeButtons.SetEquals(new HashSet<int> { 19, 31, 26, 36, 37 });

            case "D Minor":
                return activeButtons.SetEquals(new HashSet<int> { 19, 26, 30, 37, 36 });

            case "G Major":
                return activeButtons.SetEquals(new HashSet<int> { 32, 7, 2 });

            case "A Minor":
                return activeButtons.SetEquals(new HashSet<int> { 24, 19, 13, 36 });

            case "E Minor":
                return activeButtons.SetEquals(new HashSet<int> { 13, 7 });

            // hard mode notes
            case "Note C":
                return activeButtons.SetEquals(new HashSet<int> { 24 });

            case "Note G":
                return activeButtons.SetEquals(new HashSet<int> { 32 });

            case "Note F":
                return activeButtons.SetEquals(new HashSet<int> { 30 });

            case "Note D":
                return activeButtons.SetEquals(new HashSet<int> { 26 });

            case "Note A":
                return activeButtons.SetEquals(new HashSet<int> { 19 });

            default:
                return false;
        }
    }

// audio mapping for chords and notes
    void PlaySound(string chord)
    {
        if (audioSource == null) return;

        if (gameMode == "Easy")
        {
            switch (chord)
            {
                case "C Major":
                    if (cChordSound != null) audioSource.PlayOneShot(cChordSound);
                    break;

                case "D Major":
                    if (dChordSound != null) audioSource.PlayOneShot(dChordSound);
                    break;

                case "D Minor":
                    if (dMinorSound != null) audioSource.PlayOneShot(dMinorSound);
                    break;

                case "G Major":
                    if (gChordSound != null) audioSource.PlayOneShot(gChordSound);
                    break;

                case "A Minor":
                    if (aMinorSound != null) audioSource.PlayOneShot(aMinorSound);
                    break;

                case "E Minor":
                    if (eMinorSound != null) audioSource.PlayOneShot(eMinorSound);
                    break;

                default:
                    Debug.LogWarning("No chord sound assigned for " + chord);
                    break;
            }
        }
        else if (gameMode == "Hard")
        {
            switch (chord)
            {
                case "Note C":
                    if (noteCSound != null) audioSource.PlayOneShot(noteCSound);
                    break;

                case "Note G":
                    if (noteGSound != null) audioSource.PlayOneShot(noteGSound);
                    break;

                case "Note F":
                    if (noteFSound != null) audioSource.PlayOneShot(noteFSound);
                    break;

                case "Note D":
                    if (noteDSound != null) audioSource.PlayOneShot(noteDSound);
                    break;

                case "Note A":
                    if (noteASound != null) audioSource.PlayOneShot(noteASound);
                    break;

                default:
                    Debug.LogWarning("No note sound assigned for " + chord);
                    break;
            }
        }
    }

    void PlayErrorSound() // string break noise
    {
        if (audioSource == null) return;
        if (errorSound != null) audioSource.PlayOneShot(errorSound);
    }

    void SetTeacherExpression(string mood) // teacher emotes
    {
        if (teacherImage == null) return;

        switch (mood)
        {
            case "happy":
                if (teacherHappy != null) teacherImage.sprite = teacherHappy;
                break;

            case "sad":
                if (teacherSad != null) teacherImage.sprite = teacherSad;
                break;

            default:
                if (teacherNeutral != null) teacherImage.sprite = teacherNeutral;
                break;
        }

        StartCoroutine(ResetTeacherAfterDelay());
    }

    IEnumerator ResetTeacherAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (teacherImage != null && teacherNeutral != null)
            teacherImage.sprite = teacherNeutral;
    }

    // map staff pictures to notes
    void UpdateChordDisplay()
    {
        if (currentChordText != null)
        {
            if (gameMode == "Easy")
            {
                string displayName = chordSequence[currentChordIndex];
                currentChordText.text = "Play chord: " + displayName;
            }
            else if (gameMode == "Hard")
            {
                currentChordText.text = "Play this note:";
            }
        }

        if (gameMode == "Hard" && staffNoteImage != null)
        {
            staffNoteImage.enabled = true;
            switch (chordSequence[currentChordIndex])
            {
                case "Note C":
                    staffNoteImage.sprite = noteC;
                    break;

                case "Note G":
                    staffNoteImage.sprite = noteG;
                    break;

                case "Note F":
                    staffNoteImage.sprite = noteF;
                    break;

                case "Note D":
                    staffNoteImage.sprite = noteD;
                    break;

                case "Note A":
                    staffNoteImage.sprite = noteA;
                    break;
            }
        }
        else if (staffNoteImage != null)
        {
            staffNoteImage.enabled = false;
        }
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
    void PrintCurrentCombo() // print currently clicked buttons on fretboard
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
        if (feedbackText == null) yield break;

        // show speech bubble with teacher message
        if (speechBubble != null)
            speechBubble.enabled = true;

        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.alpha = 1;

        yield return new WaitForSeconds(2f);

        // clear teacher text
        feedbackText.text = "";
        feedbackText.alpha = 1;

        // hide speech bubble
        if (speechBubble != null)
            speechBubble.enabled = false;
    }

    public void ResetBestTimeNow()
    {
        PlayerPrefs.DeleteKey("BestTime");
        bestTime = Mathf.Infinity;
        if (bestTimeText != null)
            bestTimeText.text = "Best: --";
    }
}
