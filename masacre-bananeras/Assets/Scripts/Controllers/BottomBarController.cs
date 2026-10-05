using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BottomBarController : MonoBehaviour
{
    public TextMeshProUGUI barText;
    public TextMeshProUGUI personNameText;

    private int sentenceIndex = -1;
    private StoryScene currentScene;
    private State state = State.COMPLETED;
    private Animator animator;
    private bool isHidden = false;
    private bool skipTyping = false;

    private enum State
    {
        PLAYING, COMPLETED
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Hide()
    {
        if (!isHidden)
        {
            animator.SetTrigger("Hide");
            isHidden = true;
        }
    }

    public void Show()
    {
        animator.SetTrigger("Show");
        isHidden = false;
    }

    public void ClearText()
    {
        barText.text = "";
    }

    public void PlayScene(StoryScene scene)
    {
        currentScene = scene;
        sentenceIndex = -1;
        PlayNextSentence();
    }

    public void PlayNextSentence()
    {
        StoryScene.Sentence sentence = currentScene.sentences[++sentenceIndex];
        StartCoroutine(TypeText(sentence.text));
        personNameText.text = sentence.speaker.speakerName;
        personNameText.color = sentence.speaker.textColor;
        // Nunca suenan dos voces a la vez: PlayVoice corta la anterior.
        AudioController.Instance.PlayVoice(sentence.voice);
        AudioController.Instance.PlaySfx(sentence.sfx);
    }

    /// <summary>Completa de golpe el texto que se esta escribiendo; la voz sigue sonando.</summary>
    public void SkipTyping()
    {
        if (state == State.PLAYING)
        {
            skipTyping = true;
        }
    }

    public bool IsCompleted()
    {
        return state == State.COMPLETED;
    }

    public bool IsLastSentence()
    {
        return sentenceIndex + 1 == currentScene.sentences.Count;
    }

    private IEnumerator TypeText(string text)
    {
        barText.text = "";
        state = State.PLAYING;
        skipTyping = false;
        int wordIndex = 0;

        while (state != State.COMPLETED)
        {
            if (skipTyping)
            {
                barText.text = text;
                state = State.COMPLETED;
                break;
            }
            barText.text += text[wordIndex];
            yield return new WaitForSeconds(0.01f);
            if(++wordIndex == text.Length)
            {
                state = State.COMPLETED;
                break;
            }
        }
        skipTyping = false;
    }
}
