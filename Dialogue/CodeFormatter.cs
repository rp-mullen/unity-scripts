using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Collections;

public class InvisibleInputFormatter : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public TMP_InputField hiddenInputField;   // The invisible input field for editing
    public TMP_Text formattedDisplay;         // The visible, formatted display

    // Use a unique placeholder token that won't conflict with formatting.
    private const string caretToken = "%%CARET%%";
    private bool showCaret = true;
    private int lastCaretPosition = -1;
    private Coroutine blinkCoroutine;

    void Start()
    {
        // Hide the input field's text and caret.
        hiddenInputField.textComponent.color = new Color(0, 0, 0, 0);
        hiddenInputField.caretColor = new Color(0, 0, 0, 0);

        // Listen for text changes and selection/deselection events.
        hiddenInputField.onValueChanged.AddListener(UpdateFormattedText);
        hiddenInputField.onSelect.AddListener(OnInputFieldSelected);
        hiddenInputField.onDeselect.AddListener(OnInputFieldDeselected);

        // Ensure rich text is enabled.
        formattedDisplay.richText = true;

        // Initial update.
        UpdateFormattedText(hiddenInputField.text);
    }

    void Update()
    {
        // Update display if the caret moves (via arrow keys, etc.)
        if (hiddenInputField.isFocused && hiddenInputField.caretPosition != lastCaretPosition)
        {
            lastCaretPosition = hiddenInputField.caretPosition;
            UpdateFormattedText(hiddenInputField.text);
        }
    }

    void UpdateFormattedText(string input)
    {
        // Remove any previous caret tokens.
        input = input.Replace(caretToken, "");

        // Insert our caret token at the current caret position.
        int caretPos = hiddenInputField.caretPosition;
        if (caretPos >= 0 && caretPos <= input.Length)
        {
            input = input.Insert(caretPos, caretToken);
        }

        // --- Formatting Rules ---

        // 1. Replace tab characters with 4 spaces highlighted with a light gray background.
        //    The <mark> tag is used for highlighting. (Ensure your TMP version supports <mark>.)
        input = input.Replace("\t", "<mark=#D3D3D3>    </mark>");

        // 2. For each line, color text preceding a colon as names (green).
        string[] lines = input.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            int colonIndex = lines[i].IndexOf(':');
            if (colonIndex != -1)
            {
                string namePart = lines[i].Substring(0, colonIndex);
                string restPart = lines[i].Substring(colonIndex); // includes the colon
                lines[i] = $"<color=green>{namePart}</color>{restPart}";
            }
        }
        input = string.Join("\n", lines);

        // 3. Format gestures: enclosed in () -> dark orange (#FF8C00)
        input = Regex.Replace(input, @"\((.*?)\)", "<color=#FF8C00>($1)</color>");

        // 4. Format titles: enclosed in {} -> dark blue (#00008B)
        input = Regex.Replace(input, @"\{(.*?)\}", "<color=#00008B>{$1}</color>");

        // 5. Format commands: enclosed in [] -> dark green (#006400)
        input = Regex.Replace(input, @"\[(.*?)\]", "<color=#006400>[$1]</color>");

        // 6. Format tags: enclosed in <> -> purple (#800080)
        //     Updated negative lookahead to ignore any tag starting with "color" or "mark".
        input = Regex.Replace(input, @"<(?!\/?(?:color|mark)\b)(.*?)>", "<color=#800080><$1></color>");

        // --- End of formatting rules ---
        // Replace the caret token with the blinking caret (or nothing if hidden).
        input = input.Replace(caretToken, showCaret ? "<color=orange>|</color>" : " ");

        // Update the display.
        formattedDisplay.text = input;
    }

    // When the formatted display is clicked, focus the hidden input field and set the caret.
    public void OnPointerClick(PointerEventData eventData)
    {
        hiddenInputField.ActivateInputField();

        RectTransform rectTransform = formattedDisplay.rectTransform;
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

        int charIndex = TMP_TextUtilities.GetCursorIndexFromPosition(formattedDisplay, localMousePos, eventData.pressEventCamera);
        hiddenInputField.caretPosition = charIndex;
        lastCaretPosition = hiddenInputField.caretPosition;
        UpdateFormattedText(hiddenInputField.text);
    }

    // Start blinking when the input field is selected.
    void OnInputFieldSelected(string text)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkCaret());
    }

    // Stop blinking when the input field is deselected.
    void OnInputFieldDeselected(string text)
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        showCaret = false;
        UpdateFormattedText(hiddenInputField.text);
    }

    IEnumerator BlinkCaret()
    {
        while (true)
        {
            showCaret = !showCaret;
            UpdateFormattedText(hiddenInputField.text);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
