using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    [SerializeField] private Sprite m_FaceUpSprite;
    [SerializeField] private Sprite m_FaceDownSprite;

    public UnityEvent<Card> OnCardSelected;

    private SpriteRenderer m_SpriteRenderer;
    private bool m_IsFaceUp = false;

    private void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(Sprite sprite)
    {
        m_FaceUpSprite = sprite;
        m_SpriteRenderer.sprite = m_FaceDownSprite;
        m_IsFaceUp = false;
    }

    public Sprite GetCardFace()
    {
        return m_FaceUpSprite;
    }

    public void OnMouseDown()
    {
        if (!m_IsFaceUp)
        {
            FlipCard(() => OnCardSelected?.Invoke(this));
        }
    }

    public void FlipCard(TweenCallback onComplete = null)
    {
        transform.DORotate(new Vector3(0, 90, 0), 0.25f).OnComplete(() =>
        {
            m_IsFaceUp = !m_IsFaceUp;
            m_SpriteRenderer.sprite = m_IsFaceUp ? m_FaceUpSprite : m_FaceDownSprite;
            transform.DORotate(new Vector3(0, 0, 0), 0.25f).OnComplete(onComplete);
        });
    }
}