using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip m_FlipSound;
    [SerializeField] private AudioClip m_MatchSound;
    [SerializeField] private AudioClip m_MismatchSound;
    [SerializeField] private AudioClip m_GameOverSound;

    private AudioSource m_AudioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            m_AudioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayFlipSound()
    {
        PlaySound(m_FlipSound);
    }

    public void PlayMatchSound()
    {
        PlaySound(m_MatchSound);
    }

    public void PlayMismatchSound()
    {
        PlaySound(m_MismatchSound);
    }

    public void PlayGameOverSound()
    {
        PlaySound(m_GameOverSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            m_AudioSource.PlayOneShot(clip);
        }
    }
}