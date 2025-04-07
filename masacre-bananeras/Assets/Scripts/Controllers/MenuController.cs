using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
public class MenuController : MonoBehaviour
{
    public string gameScene;

    public TextMeshProUGUI musicValue;
    public AudioMixer musicMixer;

    private Animator animator;
    private int _window = 0;
    public void NewGame()
    {
        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowOptions()
    {
        animator.SetTrigger("ShowOptions");
        _window = 1;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _window == 1)
        {
            animator.SetTrigger("HideOptions");
            _window = 0;
        }
    }

    public void OnMusicChanged(float value)
    {
        musicValue.SetText(value + "%");
        musicMixer.SetFloat("volume", -50 + value / 2);
    }
}
