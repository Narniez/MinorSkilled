using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static UnityAction<string, string[], BaseSO_Properties> sendDialogue;
    public static UnityAction OnDialogueStarted;
    public static UnityAction OnDialogueEnded;

    public static bool isTalking { get; private set; }

    private Queue<string> sentences = new();
    public DialogueState dialogueState;
    private bool isTyping;

    [SerializeField] private GameObject dialogCanvas;
    [SerializeField] private GameObject[] otherCanvases;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueTextBox;

    private BaseSO_Properties currentDialogueQuest;

    private void Awake()
    {
        sendDialogue += StartDialogue;
    }

    private void OnDisable()
    {
        sendDialogue -= StartDialogue;
    }

    private void Update()
    {
        DialogSettings();

        if (Input.GetKeyDown(KeyCode.Tab) && !isTyping)
        {
            NextSentence();
        }
    }

    public void StartDialogue(string npcName, string[] dialogue, BaseSO_Properties quest)
    {
        if (dialogue == null || dialogue.Length == 0)
        {
            Debug.LogError("Dialogue is empty or null. Cannot start dialogue.");
            return;
        }

        currentDialogueQuest = quest;

        isTalking = true;
        dialogueState = DialogueState.StartDialogue;
        OnDialogueStarted?.Invoke();
        ToggleCanvases(true);

        sentences.Clear();
        dialogueTextBox.text = "";
        npcNameText.text = string.IsNullOrEmpty(npcName) ? "Unknown NPC" : npcName;

        foreach (var sentence in dialogue)
        {
            sentences.Enqueue(sentence);
        }

        NextSentence();

        if (quest == null)
        {
            Debug.Log($"Dialogue with NPC '{npcName}' has no quest associated.");
        }
        else
        {
            Debug.Log($"Dialogue with NPC '{npcName}' started. Associated quest: {quest.questName}");
        }
    }

    private void NextSentence()
    {
        if (sentences.Count > 0)
        {
            dialogueState = DialogueState.Talking;
            StartCoroutine(TypingDialogue(sentences.Dequeue()));
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueState = DialogueState.EndDialogue;

        ToggleCanvases(false);
        ClearDialogueUI();
        if(currentDialogueQuest != null) QuestManager_v2.OnQuestActivated?.Invoke(currentDialogueQuest);

        isTalking = false;
        OnDialogueEnded?.Invoke();
        Debug.Log("Dialogue has ended.");
    }

    private IEnumerator TypingDialogue(string sentence)
    {
        isTyping = true;
        dialogueTextBox.text = "";

        foreach (char c in sentence)
        {
            dialogueTextBox.text += c;

            float typingSpeed = Input.GetKey(KeyCode.Tab) ? 0.01f : 0.04f;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void ToggleCanvases(bool dialogueActive)
    {
        dialogCanvas.SetActive(dialogueActive);
        foreach (var canvas in otherCanvases)
        {
            canvas.SetActive(!dialogueActive);
        }
    }

    private void ClearDialogueUI()
    {
        dialogueTextBox.text = "";
        npcNameText.text = "";
    }

    private void DialogSettings()
    {
        switch (dialogueState)
        {
            case DialogueState.StartDialogue:
                Time.timeScale = 0;
                break;
            case DialogueState.Talking:
                Debug.Log("Talking dialogue");
                break;
            case DialogueState.EndDialogue:
                Time.timeScale = 1;
                break;
        }
    }
}

public enum DialogueState
{
    None,
    StartDialogue,
    Talking,
    EndDialogue
}
