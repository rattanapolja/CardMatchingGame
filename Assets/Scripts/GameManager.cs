using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("Card Slot Setup")]
    [SerializeField] private List<Sprite> m_CardSpriteList;
    [SerializeField] private int m_Rows = 3;
    [SerializeField] private int m_Columns = 3;

    [Header("Prefab")]
    [SerializeField] private Card m_CardPrefab;
    [SerializeField] private GridLayoutGroup m_CardContentLayout;

    [Header("UI")]
    [SerializeField] private Text m_ScoreText;
    [SerializeField] private InputField m_RowInputField;
    [SerializeField] private InputField m_ColumnInputField;

    private Card m_FirstCard;
    private Card m_SecondCard;
    private int m_Score = 0;
    private bool m_IsCheckingCards = false;

    private void Start()
    {
        m_RowInputField.text = m_Rows.ToString();
        m_ColumnInputField.text = m_Columns.ToString();
        SetupGame();
    }

    public void SetupGame()
    {
        AdjustRowsAndColumns();
        ClearExistingCards();
        GenerateCards();
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
        foreach (Transform child in m_CardContentLayout.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void GenerateCards()
    {
        m_CardContentLayout.constraintCount = m_Columns;

        List<Sprite> cardSprites = new List<Sprite>(m_CardSpriteList);
        cardSprites.AddRange(cardSprites);
        Shuffle(cardSprites);

        for (int i = 0; i < m_Rows * m_Columns; i++)
        {
            Card card = Instantiate(m_CardPrefab, m_CardContentLayout.transform);
            card.Init(cardSprites[i]);
            card.OnCardSelected.AddListener(OnCardSelected);
        }
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

        if (m_FirstCard == null)
        {
            m_FirstCard = selectedCard;
        }
        else if (m_SecondCard == null)
        {
            m_SecondCard = selectedCard;
            CheckMatch();
        }
    }

    private void CheckMatch()
    {
        m_IsCheckingCards = true;

        m_SecondCard.FlipCard(() =>
        {
            if (m_FirstCard.GetCardFace() == m_SecondCard.GetCardFace())
            {
                AudioManager.Instance.PlayMatchSound();
                m_Score += 10;
                ResetCards();
            }
            else
            {
                AudioManager.Instance.PlayMismatchSound();
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    m_FirstCard.FlipCard();
                    m_SecondCard.FlipCard(() =>
                    {
                        ResetCards();
                    });
                });
            }
        });
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

    public void RestartGame()
    {
        m_Rows = int.Parse(m_RowInputField.text);
        m_Columns = int.Parse(m_ColumnInputField.text);
        SetupGame();
    }

    public void SaveGame()
    {
        GameData data = new GameData
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