using System.Collections.Generic;
using UnityEngine;

public class TagSoundTrigger : MonoBehaviour
{
    [System.Serializable]
    public class TagAudio
    {
        public string tagName;       // Nama tag (misal: "Hati", "Koin")
        public AudioClip soundClip;  // Suara yang dimainkan jika menyentuh tag ini
    }

    [Header("List Tag dan Suara")]
    public List<TagAudio> tagAudioList = new List<TagAudio>();

    [Header("Audio Source (Drag dari Player)")]
    public AudioSource audioSource;

    private Dictionary<string, AudioClip> tagToClipMap;

    void Start()
    {
        // Inisialisasi dictionary dari list agar cepat diakses
        tagToClipMap = new Dictionary<string, AudioClip>();
        foreach (TagAudio ta in tagAudioList)
        {
            if (!tagToClipMap.ContainsKey(ta.tagName))
                tagToClipMap.Add(ta.tagName, ta.soundClip);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (tagToClipMap.ContainsKey(other.tag))
        {
            audioSource.PlayOneShot(tagToClipMap[other.tag]);
            Debug.Log("Menyentuh objek bertag: " + other.tag);
        }
    }
}
