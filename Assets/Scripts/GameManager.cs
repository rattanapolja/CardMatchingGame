using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
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

    [Header("Panel")]
    [SerializeField] private GameObject m_GameSetupPanel;
    [SerializeField] private GameObject m_GameScorePanel;
    [SerializeField] private GameObject m_WinLosePanel;
    [SerializeField] private GameObject m_BlockerPanel;

    [Header("UI")]
    [SerializeField] private Button m_StartBtn;
    [SerializeField] private TMP_Text m_WinLoseText;
    [SerializeField] private TMP_Text m_FinalScoreText;
    [SerializeField] private TMP_InputField m_RowInputField;
    [SerializeField] private TMP_InputField m_ColumnInputField;

    [Header("Score")]
    [SerializeField] private TMP_Text m_ScoreText;
    [SerializeField] private TMP_Text m_TurnText;
    [SerializeField] private TMP_Text m_ComboText;
    [SerializeField] private TMP_Text m_MaxComboText;
    [SerializeField] private TMP_Text m_MisMatchStackText;

    private List<Card> m_CardList = new();
    private Queue<Card> m_CardQueue = new();

    private int m_Score = 0;
    private int m_Turn = 0;
    private int m_MaxMiss = 0;
    private int m_MisMatchStack = 0;
    private int m_ComboCount = 0;
    private int m_MaxCombo = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_RowInputField.onEndEdit.AddListener(OnRowInputFieldChanged);
        m_ColumnInputField.onEndEdit.AddListener(OnColumnInputFieldChanged);
        m_StartBtn.onClick.AddListener(OnClickStart);
    }

    private void OnRowInputFieldChanged(string text)
    {
        int inputRows = int.Parse(text);

        if (inputRows > 6)
        {
            inputRows = 6;
            m_RowInputField.text = inputRows.ToString();
        }

        m_Rows = inputRows;
    }

    private void OnColumnInputFieldChanged(string text)
    {
        int inputColumns = int.Parse(text);

        if (inputColumns > 7)
        {
            inputColumns = 7;
            m_ColumnInputField.text = inputColumns.ToString();
        }

        m_Columns = inputColumns;
    }

    public void SetupGame()
    {
        m_Score = 0;
        m_Turn = 0;
        m_MisMatchStack = 0;
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
        m_WinLosePanel.SetActive(false);
        m_BlockerPanel.SetActive(true);

        m_ComboCount = 0;
        m_MaxCombo = 0;
        UpdateCombo();

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

        m_MaxMiss = m_Rows + m_Columns;
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

        UpdateCardSize();

        int numberOfCardTypes = m_Columns * m_Rows;

        List<int> indices = new();
        for (int i = 0; i < 25; i++)
        {
            indices.Add(i);
        }
        Shuffle(indices);

        List<Sprite> cardSprites = new();
        for (int i = 0; i < numberOfCardTypes / 2; i++)
        {
            cardSprites.Add(m_CardSpriteList[indices[i]]);
        }
        cardSprites.AddRange(cardSprites);
        Shuffle(cardSprites);

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
        m_BlockerPanel.SetActive(false);
    }

    private void UpdateCardSize()
    {
        Canvas.ForceUpdateCanvases();

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
        AudioManager.Instance.PlayFlipSound();
        if (m_CardQueue.Count < 2)
        {
            m_CardQueue.Enqueue(selectedCard);
            if (m_CardQueue.Count == 2)
            {
                m_TurnText.text = $"Turns: {++m_Turn}";
                StartCoroutine(ProcessCardQueue());
            }
        }
    }

    private IEnumerator ProcessCardQueue()
    {
        while (m_CardQueue.Count >= 2)
        {
            Card firstCard = m_CardQueue.Dequeue();
            Card secondCard = m_CardQueue.Dequeue();

            if (firstCard.GetCardFace() == secondCard.GetCardFace())
            {
                AudioManager.Instance.PlayMatchSound();
                m_Score += 10;
                m_Score += ++m_ComboCount * 5;
                if (m_ComboCount > m_MaxCombo)
                {
                    m_MaxCombo = m_ComboCount;
                }
                m_MisMatchStack = 0;
                m_MisMatchStackText.text = $"Mismatch Stack: {m_MisMatchStack}";
                if (AllPairsMatched())
                {
                    DisplayWinScreen(true);
                    yield break;
                }
                else
                {
                    ResetCards();
                }
            }
            else
            {
                AudioManager.Instance.PlayMismatchSound();
                m_ComboCount = 0;
                m_MisMatchStackText.text = $"Mismatch Stack: {++m_MisMatchStack}";
                if (m_MisMatchStack >= m_MaxMiss)
                {
                    DisplayWinScreen(false);
                    yield break;
                }

                DOVirtual.DelayedCall(0.5f, () =>
                {
                    firstCard.FlipCard();
                    secondCard.FlipCard(() => ResetCards());
                });
            }
            UpdateCombo();
        }
        yield return null;
    }

    private bool AllPairsMatched()
    {
        foreach (Card card in m_CardList)
        {
            if (!card.IsFaceUp)
            {
                return false;
            }
        }
        return true;
    }

    private void DisplayWinScreen(bool isWin)
    {
        AudioManager.Instance.PlayGameOverSound();
        m_BlockerPanel.SetActive(true);
        m_WinLosePanel.SetActive(true);
        m_FinalScoreText.text = $"Final Score: : {m_Score}";
        m_MaxComboText.text = $"Max Combo x{m_MaxCombo}";

        if (isWin)
        {
            m_WinLoseText.text = "You Win!";
        }
        else
        {
            m_WinLoseText.text = "You Lose!";
        }
        RestartGame();
    }

    private void UpdateCombo()
    {
        m_ComboText.text = $"Combo x{m_ComboCount}";
    }

    private void ResetCards()
    {
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