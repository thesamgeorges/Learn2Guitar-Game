using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Dependencies")]
    public ButtonManager buttonManager; // reference to the existing ButtonManager

    [Header("UI Elements")]
    public TMP_Text tutorialText;
    public Button nextButton;

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    public Color muteColor = Color.red;
    public Color strumHighlightColor = Color.yellow; // highlight color for strum button

    private int stepIndex = 0;
    private bool waitingForStrum = false; // waits for player to hit strum

    private readonly List<string> tutorialSteps = new List<string> // tutorial dialogue
    {
        "Welcome to Learn2Guitar!\nLet's learn how the guitar works.",
        "These buttons represent the strings and frets on a guitar.",
        "The top layer of buttons mutes the strings.\nThey appear green when muted.",
        "The other buttons are used to fret notes and chords.\nLet's try pressing some chords!",
        "Here’s the D Major chord.\nTry pressing the Strum button to play it!",
        "Here’s the C Major chord.\nTry pressing the Strum button to play it!",
        "Here’s the G Major chord.\nTry pressing the Strum button to play it!",
        "You can find the rest in the Cheat Sheet page!",
        "That's it! You're ready to play!"
    };

    void Start()
    {
        if (buttonManager == null)
        {
            Debug.LogError("TutorialManager: ButtonManager not assigned!");
            return;
        }

        // enable tutorial mode on ButtonManager
        buttonManager.tutorialModeActive = true;

        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);

        // listen for Strum button press
        if (buttonManager.strumButton != null)
            buttonManager.strumButton.onClick.AddListener(OnStrumPressedDuringTutorial);

        ResetAllButtonColors();
        ShowStep(0);
    }

    // Reset all button colors
    void ResetAllButtonColors()
    {
        foreach (Button btn in buttonManager.buttons)
        {
            if (btn != null)
                SetButtonColor(btn, normalColor);
        }

        if (buttonManager.strumButton != null)
            SetButtonColor(buttonManager.strumButton, normalColor);
    }

    void SetButtonColor(Button button, Color color)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.selectedColor = color;
        button.colors = cb;
    }

    public void NextStep()
    {
        if (waitingForStrum)
        {
            tutorialText.text = "Try pressing the Strum button first!";
            return;
        }

        stepIndex++;
        if (stepIndex >= tutorialSteps.Count)
        {
            tutorialText.text = "Tutorial complete! Go back to the main menu to start playing.";
            nextButton.interactable = false;
            ResetAllButtonColors();

            // disable tutorial mode when finished
            buttonManager.tutorialModeActive = false;
            return;
        }

        ShowStep(stepIndex);
    }

    void ShowStep(int index) // tutorial button flow
    {
        tutorialText.text = tutorialSteps[index];
        ResetAllButtonColors();
        waitingForStrum = false;

        switch (index)
        {
            case 2:
                HighlightMuteButtons();
                break;

            case 3:
                HighlightExampleFrets();
                break;

            case 4:
                HighlightChord("D Major");
                waitingForStrum = true;
                HighlightStrumButton();
                break;

            case 5:
                HighlightChord("C Major");
                waitingForStrum = true;
                HighlightStrumButton();
                break;

            case 6:
                HighlightChord("G Major");
                waitingForStrum = true;
                HighlightStrumButton();
                break;
        }
    }

    void HighlightMuteButtons() 
    {
        for (int i = 36; i <= 41 && i < buttonManager.buttons.Length; i++)
        {
            Button btn = buttonManager.buttons[i];
            if (btn != null)
                SetButtonColor(btn, muteColor);
        }
    }

    void HighlightExampleFrets() // random example frets
    {
        int[] exampleIndices = { 5, 10, 15, 20 };
        foreach (int i in exampleIndices)
        {
            if (i < buttonManager.buttons.Length)
            {
                Button btn = buttonManager.buttons[i];
                if (btn != null)
                    SetButtonColor(btn, highlightColor);
            }
        }
    }

    void HighlightChord(string chordName) // highlight c, d, and g chords
    {
        List<int> chordButtons = new List<int>();

        switch (chordName)
        {
            case "D Major":
                chordButtons = new List<int> { 19, 31, 26, 36, 37 };
                break;

            case "C Major":
                chordButtons = new List<int> { 24, 8, 13, 36 };
                break;

            case "G Major":
                chordButtons = new List<int> { 32, 7, 2 };
                break;
        }

        foreach (int i in chordButtons)
        {
            if (i < buttonManager.buttons.Length)
            {
                Button btn = buttonManager.buttons[i];
                if (btn != null)
                    SetButtonColor(btn, highlightColor);
            }
        }
    }

    void HighlightStrumButton() // highlight the strum button
    {
        if (buttonManager.strumButton != null)
            SetButtonColor(buttonManager.strumButton, strumHighlightColor);
    }

    void OnStrumPressedDuringTutorial() // make player strum current note
    {
        if (!waitingForStrum)
            return;

        if (buttonManager.strumButton != null)
            SetButtonColor(buttonManager.strumButton, normalColor);

        string chordToPlay = "";
        switch (stepIndex)
        {
            case 4: chordToPlay = "D Major"; break;
            case 5: chordToPlay = "C Major"; break;
            case 6: chordToPlay = "G Major"; break;
        }

        if (!string.IsNullOrEmpty(chordToPlay))
            buttonManager.SendMessage("PlaySound", chordToPlay, SendMessageOptions.DontRequireReceiver);

        waitingForStrum = false;
        tutorialText.text = "Nice! You played " + chordToPlay + "! Press Next to continue.";
    }
}
