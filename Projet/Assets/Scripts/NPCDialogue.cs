using UnityEngine;
using TMPro;
using System.Collections;

public class NPCDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;

    [Header("Vitesse d'écriture")]
    public float delaiEntreLettres = 0.03f;

    [Header("Pause après une phrase")]
    public float pauseMinimale = 2f;

    private Coroutine dialogueCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogueCoroutine == null)
            {
                dialogueCoroutine = StartCoroutine(JouerDialogue());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
                dialogueCoroutine = null;
            }

            dialogueText.text = "";
            dialoguePanel.SetActive(false);
        }
    }

    private IEnumerator JouerDialogue()
    {
        dialoguePanel.SetActive(true);

        foreach (string phrase in dialogues)
        {
            yield return StartCoroutine(EcrireTexte(phrase));

            float tempsLecture = Mathf.Max(
                pauseMinimale,
                phrase.Length * 0.05f
            );

            yield return new WaitForSeconds(tempsLecture);
        }

        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        dialogueCoroutine = null;
    }

    private IEnumerator EcrireTexte(string texte)
    {
        dialogueText.text = "";

        foreach (char lettre in texte)
        {
            dialogueText.text += lettre;
            yield return new WaitForSeconds(delaiEntreLettres);
        }
    }
}