using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [Header("Button Setup")]
    public Button[] buttons = new Button[42];

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.green;

    // Track which buttons are currently pressed
    private HashSet<int> activeButtons = new HashSet<int>();

    bool c_chord = false;
    bool g_chord = false;
    bool d_chord = false;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture index for the click event
            if (buttons[index] != null)
            {
                SetButtonColor(buttons[index], normalColor);
                buttons[index].onClick.AddListener(() => OnButtonClicked(index));
            }
        }
    }

    void OnButtonClicked(int index)
    {
        if (activeButtons.Contains(index))
        {
            // Unpress
            activeButtons.Remove(index);
            SetButtonColor(buttons[index], normalColor);
            Debug.Log($"Released: Button {index}");
        }
        else
        {
            // Press
            activeButtons.Add(index);
            SetButtonColor(buttons[index], pressedColor);
            Debug.Log($"Pressed: Button {index}");
        }

        PrintCurrentCombo();
        CheckSpecialCombos();
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

    void CheckSpecialCombos()
    {
        if (activeButtons.Contains(24) && activeButtons.Contains(8) && activeButtons.Contains(13) && activeButtons.Contains(36))
        {
            Debug.Log("C CHORD SELECTED)");
            c_chord = true;
        }
        
        if (activeButtons.Contains(19) && activeButtons.Contains(31) && activeButtons.Contains(26) && activeButtons.Contains(36) && activeButtons.Contains(37))
        {
            Debug.Log("D CHORD SELECTED)");
        }   d_chord = true;

        if (activeButtons.Contains(32) && activeButtons.Contains(7) && activeButtons.Contains(2))
        {
            Debug.Log("G CHORD SELECTED)");
            d_chord = true;
        }

        // mute buttons
        if (activeButtons.Contains(40))
        {
            Debug.Log("HIGH E STRING MUTED");
        }
        
        if (activeButtons.Contains(41))
        {
            Debug.Log("B STRING MUTED");
        }

        if (activeButtons.Contains(39))
        {
            Debug.Log("G STRING MUTED");
        }

        if (activeButtons.Contains(38))
        {
            Debug.Log("D STRING MUTED");
        }

        if (activeButtons.Contains(37))
        {
            Debug.Log("A STRING MUTED");
        }

        if (activeButtons.Contains(36))
        {
            Debug.Log("LOW E STRING MUTED");
        }

        // Example: all buttons pressed
        if (activeButtons.Count == buttons.Length)
        {
            Debug.Log("All buttons pressed! Ultimate combo detected.");
        }
    }

    void SetButtonColor(Button button, Color color)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.selectedColor = color;
        button.colors = cb;
    }
}
