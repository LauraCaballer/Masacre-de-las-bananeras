using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Asigna a cada StoryScene las voces (por nombre &lt;escena&gt;_&lt;NN&gt;), los efectos puntuales
/// y el ambiente de la escena. Menu: Earthcation/Asignar audios.
/// </summary>
public static class AudioAssigner
{
    // Ambiente en loop por escena (tabla de Arquitectura.md, seccion 4)
    private static readonly Dictionary<string, string> Ambiences = new Dictionary<string, string>();

    // Efectos puntuales: "<escena>:<indice de frase>" -> id del efecto
    private static readonly Dictionary<string, string> Sfx = new Dictionary<string, string>
    {
        { "6:0", "pasos_tierra" },
        { "7:0", "moneda" },
        { "10:0", "vendedor_periodico" },
        { "17:0", "machete" },
        { "21:0", "nina_rie" },
        { "23:0", "cena" },
        { "27:0", "gallo_madrugada" },
        { "28:0", "cafe" },
        { "29:0", "accidente" },
        { "31:0", "anuncio_jefe" },
        { "8.2.2:0", "cumpleanos" },
        { "8.2.3:0", "periodico_impacto" },
        { "9.1.6:0", "sirenas" },
        { "9.1.6:1", "soldados_marchan" },
        { "9.1.8:0", "latido" },
        { "9.1.9:0", "disparos" },
        { "9.1.9:1", "panico" },
    };

    static AudioAssigner()
    {
        AddRange("amb_pueblo", 6, 13);
        AddRange("amb_plantacion", 17, 19);
        AddRange("amb_plantacion", 28, 31);
        AddRange("amb_casa_noche", 20, 26);
        Ambiences["8.2.6"] = "multitud_protesta";
        Ambiences["9.1.6"] = "multitud_protesta";
        // La rama del bar (7.1.x) se resuelve por prefijo en AmbienceFor.
    }

    private static void AddRange(string id, int from, int to)
    {
        for (int i = from; i <= to; i++)
        {
            Ambiences[i.ToString()] = id;
        }
    }

    private static string AmbienceFor(string scene)
    {
        if (scene.StartsWith("7.1"))
        {
            return "amb_bar";
        }
        string id;
        return Ambiences.TryGetValue(scene, out id) ? id : null;
    }

    [MenuItem("Earthcation/Asignar audios")]
    public static void Assign()
    {
        Dictionary<string, AudioClip> voices = LoadClips("Assets/Audio/Voces");
        Dictionary<string, AudioClip> effects = LoadClips("Assets/Audio/SFX");

        int conVoz = 0, sinVoz = 0, conSfx = 0, conAmbiente = 0, escenas = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:StoryScene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StoryScene scene = AssetDatabase.LoadAssetAtPath<StoryScene>(path);
            if (scene == null)
            {
                continue;
            }
            escenas++;
            string sceneName = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < scene.sentences.Count; i++)
            {
                StoryScene.Sentence sentence = scene.sentences[i];
                AudioClip voice;
                if (voices.TryGetValue(string.Format("{0}_{1:00}", sceneName, i), out voice))
                {
                    sentence.voice = voice;
                    conVoz++;
                }
                else
                {
                    sentence.voice = null;
                    sinVoz++;
                }

                string sfxId;
                AudioClip sfxClip;
                if (Sfx.TryGetValue(sceneName + ":" + i, out sfxId) && effects.TryGetValue(sfxId, out sfxClip))
                {
                    sentence.sfx = sfxClip;
                    conSfx++;
                }
                else
                {
                    sentence.sfx = null;
                }
                scene.sentences[i] = sentence;
            }

            string ambienceId = AmbienceFor(sceneName);
            AudioClip ambience;
            scene.ambience = ambienceId != null && effects.TryGetValue(ambienceId, out ambience) ? ambience : null;
            if (scene.ambience != null)
            {
                conAmbiente++;
            }
            EditorUtility.SetDirty(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log(string.Format(
            "Audios asignados: {0} escenas, {1} voces, {2} frases sin voz, {3} efectos, {4} ambientes.",
            escenas, conVoz, sinVoz, conSfx, conAmbiente));
    }

    private static Dictionary<string, AudioClip> LoadClips(string folder)
    {
        var map = new Dictionary<string, AudioClip>();
        if (!Directory.Exists(folder))
        {
            Debug.LogWarning("No existe la carpeta " + folder);
            return map;
        }
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            map[Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return map;
    }
}
