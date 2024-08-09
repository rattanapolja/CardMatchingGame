using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Button m_CardBtn;
    [SerializeField] private Sprite m_FaceDownSprite;

    public UnityEvent<Card> OnCardSelected;

    private Sprite m_FaceUpSprite;

    private bool m_IsFaceUp = false;

    private void Awake()
    {
        m_CardBtn.onClick.AddListener(OnClick);
    }

    public void Init(Sprite sprite)
    {
        m_FaceUpSprite = sprite;
        m_CardBtn.image.sprite = m_FaceUpSprite;
        m_IsFaceUp = true;
    }

    public Sprite GetCardFace()
    {
        return m_FaceUpSprite;
    }

    public void OnClick()
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
            m_CardBtn.image.sprite = m_IsFaceUp ? m_FaceUpSprite : m_FaceDownSprite;
            transform.DORotate(new Vector3(0, 0, 0), 0.25f).OnComplete(onComplete);
        });
    }

    public void Reset()
    {
        m_FaceUpSprite = null;
        m_CardBtn.image.sprite = m_FaceDownSprite;
        m_IsFaceUp = false;
        OnCardSelected.RemoveAllListeners();
    }
}