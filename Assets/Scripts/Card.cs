using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Button m_CardBtn;
    [SerializeField] private Sprite m_FaceDownSprite;

    public UnityEvent<Card> OnCardSelected;

    private CardData m_CardData;
    public CardData GetCardData { get => m_CardData; set => m_CardData = value; }

    private Sprite m_FaceUpSprite;

    public bool IsFaceUp = false;


    private void Awake()
    {
        m_CardBtn.onClick.AddListener(OnClick);
    }

    public void Init(Sprite sprite,int cardIndex, int index)
    {
        m_CardData = new()
        {
            SpriteIndex = index,
            CardIndex = cardIndex
        };
        m_FaceUpSprite = sprite;
        m_CardBtn.image.sprite = m_FaceUpSprite;
        IsFaceUp = true;
    }

    public Sprite GetCardFace()
    {
        return m_FaceUpSprite;
    }

    public void SetFaceUp(bool isFaceUp)
    {
        IsFaceUp = isFaceUp;
        m_CardBtn.image.sprite = IsFaceUp ? m_FaceUpSprite : m_FaceDownSprite; ;
    }

    public void OnClick()
    {
        if (!IsFaceUp)
        {
            FlipCard(() => OnCardSelected?.Invoke(this));
        }
    }

    public void FlipCard(TweenCallback onComplete = null)
    {
        transform.DORotate(new Vector3(0, 90, 0), 0.25f).OnComplete(() =>
        {
            m_CardData.IsFaceUp = IsFaceUp = !IsFaceUp;
            m_CardBtn.image.sprite = IsFaceUp ? m_FaceUpSprite : m_FaceDownSprite;
            transform.DORotate(new Vector3(0, 0, 0), 0.25f).OnComplete(onComplete);
        });
    }

    public void Reset()
    {
        m_FaceUpSprite = null;
        m_CardBtn.image.sprite = m_FaceDownSprite;
        m_CardData.IsFaceUp = IsFaceUp = false;
        OnCardSelected.RemoveAllListeners();
    }
}