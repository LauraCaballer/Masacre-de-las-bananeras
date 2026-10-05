using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameScene currentScene;
    public BottomBarController bottomBar;
    public BackgroundController backgroundController;
    public ChooseController chooseController;

    private State state = State.IDLE;

    private enum State
    {
        IDLE, ANIMATE, CHOOSE
    }

    void Start()
    {
        if (currentScene is StoryScene)
        {
            StoryScene storyScene = currentScene as StoryScene;
            AudioController.Instance.SetAmbience(storyScene.ambience);
            bottomBar.PlayScene(storyScene);
            backgroundController.SetImage(storyScene.background);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (state != State.IDLE)
            {
                return;
            }
            if (!bottomBar.IsCompleted())
            {
                // Clic mientras se escribe: mostrar la frase completa, la voz sigue.
                bottomBar.SkipTyping();
                return;
            }
            // Clic con la frase completa: cortar la voz y avanzar sin esperar al audio.
            AudioController.Instance.StopVoice();
            if (bottomBar.IsLastSentence())
            {
                PlayScene((currentScene as StoryScene).nextScene);
            }
            else
            {
                bottomBar.PlayNextSentence();
            }
        }
    }

    public void PlayScene(GameScene scene)
    {
        StartCoroutine(SwitchScene(scene));
    }

    private IEnumerator SwitchScene(GameScene scene)
    {
        state = State.ANIMATE;
        currentScene = scene;
        AudioController.Instance.StopVoice();
        bottomBar.Hide();
        yield return new WaitForSeconds(1f);
        if (scene is StoryScene)
        {
            StoryScene storyScene = scene as StoryScene;
            AudioController.Instance.SetAmbience(storyScene.ambience);
            backgroundController.SwitchImage(storyScene.background);
            yield return new WaitForSeconds(1f);
            bottomBar.ClearText();
            bottomBar.Show();
            yield return new WaitForSeconds(1f);
            bottomBar.PlayScene(storyScene);
            state = State.IDLE;
        }
        else if (scene is ChooseScene)
        {
            state = State.CHOOSE;
            AudioController.Instance.StopVoice();
            chooseController.SetupChoose(scene as ChooseScene);
        }
    }
}
