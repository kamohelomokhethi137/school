using UnityEngine;

public class TalkInteractable : Interactable
{
    public string speakerName = "Student";
    public string[] lines = new string[] { "Hello!" };
    public float bubbleSeconds = 5f;

    private static TalkInteractable active;
    private static float hideAt;
    private int lineIndex = -1;

    public override string Prompt
    {
        get { return "Press E to talk to " + speakerName; }
    }

    public override bool CanInteract()
    {
        return true;
    }

    public override void Interact()
    {
        if (lines == null || lines.Length == 0) return;
        lineIndex = (lineIndex + 1) % lines.Length;
        active = this;
        hideAt = Time.time + bubbleSeconds;
    }

    void OnGUI()
    {
        if (active != this || lines == null || lines.Length == 0) return;
        if (Time.time > hideAt) return;

        string body = lines[Mathf.Clamp(lineIndex, 0, lines.Length - 1)];
        string text = speakerName + ": " + body;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 16;
        style.wordWrap = true;
        style.normal.textColor = Color.white;

        float w = 460f;
        float h = 62f;
        Rect r = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.74f, w, h);

        Color old = GUI.color;
        GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.75f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;

        Rect inner = new Rect(r.x + 10f, r.y + 6f, r.width - 20f, r.height - 12f);
        GUI.Label(inner, text, style);
    }
}
