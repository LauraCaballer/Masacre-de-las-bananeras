using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryScene", menuName ="Data/New Story Scene")]
[System.Serializable]
public class StoryScene : GameScene
{
    public List<Sentence> sentences;
    public Sprite background;
    public GameScene nextScene;
    // Ambiente en loop de la escena; null = se mantiene el anterior
    public AudioClip ambience;

    [System.Serializable]
    public struct Sentence
    {
        public string text;
        public Speaker speaker;
        public AudioClip voice;
        public AudioClip sfx;
    }
}

public class GameScene : ScriptableObject { }
