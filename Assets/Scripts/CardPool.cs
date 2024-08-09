using UnityEngine;
using System.Collections.Generic;

public class CardPool : MonoBehaviour
{
    [SerializeField] private Card m_CardPrefab;
    private Queue<Card> m_CardPoolQueue = new();

    public Card GetCard()
    {
        if (m_CardPoolQueue.Count > 0)
        {
            Card card = m_CardPoolQueue.Dequeue();
            card.gameObject.SetActive(true);
            return card;
        }
        else
        {
            return Instantiate(m_CardPrefab);
        }
    }

    public void ReturnCard(Card card)
    {
        card.gameObject.SetActive(false);
        m_CardPoolQueue.Enqueue(card);
    }
}