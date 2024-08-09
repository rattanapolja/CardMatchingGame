using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Card Slot Setup")]
    [SerializeField] private List<Sprite> m_CardSpriteList;
    [SerializeField] private int m_Rows = 3;
    [SerializeField] private int m_Columns = 4;

    [Header("Prefab")]
    [SerializeField] private CardPool m_CardPool;
    [SerializeField] private GridLayoutGroup m_CardContentLayout;

    [Header("UI")]
    [SerializeField] private GameObject m_GameSetupPanel;
    [SerializeField] private GameObject m_GameScorePanel;
    [SerializeField] private Button m_StartBtn;
    [SerializeField] private TMP_Text m_ScoreText;
    [SerializeField] private TMP_Text m_TurnText;
    [SerializeField] private TMP_Text m_MisMatchStackText;
    [SerializeField] private TMP_InputField m_RowInputField;
    [SerializeField] private TMP_InputField m_ColumnInputField;

    private List<Card> m_CardList = new();

    private Card m_FirstCard;
    private Card m_SecondCard;

    private int m_Score = 0;
    private int m_Turn = 0;
    private int m_MisMatchStack = 0;
    public bool m_IsCheckingCards = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_RowInputField.onEndEdit.AddListener(text => m_Rows = int.Parse(text));
        m_ColumnInputField.onEndEdit.AddListener(text => m_Columns = int.Parse(text));
        m_StartBtn.onClick.AddListener(OnClickStart);
    }

    public void SetupGame()
    {
        m_ScoreText.text = "Score: 0";
        m_TurnText.text = "Turns: 0";
        m_MisMatchStackText.text = $"Mismatch Stack: 0";

        AdjustRowsAndColumns();
        ClearExistingCards();
        GenerateCards();
    }

    private void OnClickStart()
    {
        m_GameSetupPanel.SetActive(false);
        m_GameScorePanel.SetActive(true);

        SetupGame();
    }

    private void RestartGame()
    {
        m_GameSetupPanel.SetActive(true);
        m_GameScorePanel.SetActive(false);
    }

    private void AdjustRowsAndColumns()
    {
        int totalCards = m_Rows * m_Columns;

        if (totalCards % 2 != 0)
        {
            m_Columns++;
            m_ColumnInputField.text = m_Columns.ToString();
        }
    }

    private void ClearExistingCards()
    {
        foreach (Card card in m_CardList)
        {
            card.Reset();
            m_CardPool.ReturnCard(card);
        }
        m_CardList.Clear();
    }

    private void GenerateCards()
    {
        m_CardContentLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        m_CardContentLayout.constraintCount = m_Columns;

        // UpdateCardSize();

        int numberOfCardTypes = m_Columns * m_Rows;
        List<Sprite> cardSprites = m_CardSpriteList.GetRange(0, numberOfCardTypes / 2);
        cardSprites.AddRange(cardSprites);
        Shuffle(cardSprites);

        m_IsCheckingCards = true;

        for (int i = 0; i < numberOfCardTypes; i++)
        {
            Card card = m_CardPool.GetCard();
            card.transform.SetParent(m_CardContentLayout.transform, false);
            card.Init(cardSprites[i]);
            card.OnCardSelected.AddListener(OnCardSelected);
            m_CardList.Add(card);
        }
        StartCoroutine(FlipAllCardsDownAfterDelay(2f));
    }

    private IEnumerator FlipAllCardsDownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (Card card in m_CardList)
        {
            card.FlipCard();
        }

        m_IsCheckingCards = false;
    }

    private void UpdateCardSize()
    {
        RectTransform gridRect = m_CardContentLayout.GetComponent<RectTransform>();
        float width = gridRect.rect.width;
        float height = gridRect.rect.height;

        float targetAspectRatio = 16f / 25f;

        float maxCardWidth = width / m_Columns;
        float maxCardHeight = height / m_Rows;

        float cardWidth = Mathf.Min(maxCardWidth, maxCardHeight * targetAspectRatio);
        float cardHeight = cardWidth / targetAspectRatio;

        m_CardContentLayout.cellSize = new Vector2(cardWidth, cardHeight);
        m_CardContentLayout.spacing = new Vector2(cardWidth * 0.05f, cardHeight * 0.05f);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void OnCardSelected(Card selectedCard)
    {
        if (m_IsCheckingCards) return;
        // AudioManager.Instance.PlayFlipSound();

        if (m_FirstCard == null)
        {
            m_FirstCard = selectedCard;
        }
        else if (m_SecondCard == null)
        {
            m_SecondCard = selectedCard;
            m_TurnText.text = $"Turns: {++m_Turn}";
            CheckMatch();
        }
    }

    private void CheckMatch()
    {
        m_IsCheckingCards = true;

        if (m_FirstCard.GetCardFace() == m_SecondCard.GetCardFace())
        {
            // AudioManager.Instance.PlayMatchSound();
            m_Score += 10;
            m_MisMatchStack = 0;
            m_MisMatchStackText.text = $"Mismatch Stack: {m_MisMatchStack}";
            ResetCards();
        }
        else
        {
            // AudioManager.Instance.PlayMismatchSound();
            m_MisMatchStackText.text = $"Mismatch Stack: {++m_MisMatchStack}";
            if (m_MisMatchStack >= 5)
            {
                RestartGame();
                return;
            }
            DOVirtual.DelayedCall(0.5f, () =>
            {
                m_FirstCard.FlipCard();
                m_SecondCard.FlipCard(() =>
                {
                    ResetCards();
                });
            });
        }
    }

    private void ResetCards()
    {
        m_FirstCard = null;
        m_SecondCard = null;
        m_IsCheckingCards = false;

        UpdateScore();
    }

    private void UpdateScore()
    {
        m_ScoreText.text = "Score: " + m_Score;
    }

    public void SaveGame()
    {
        GameData data = new()
        {
            score = m_Score,
            rows = m_Rows,
            columns = m_Columns,
        };
        SaveSystem.SaveGame(data);
    }

    public void LoadGame()
    {
        GameData data = SaveSystem.LoadGame();

        if (data != null)
        {
            m_Score = data.score;
            m_Rows = data.rows;
            m_Columns = data.columns;
            SetupGame();
        }
        else
        {
            SetupGame();
        }
    }
}