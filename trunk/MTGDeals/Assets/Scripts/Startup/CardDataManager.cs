using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DealFinder.Network.Models;

class CardDataManager : MonoBehaviour
{
    private static CardDataManager CDM;

    public static CardDataManager GetInstance()
    {
        if (CDM == null)
        {
            CDM = new GameObject("CardDataManager").AddComponent<CardDataManager>();
        }
        return CDM;
    }

    public List<TcgCard> Cards;

    public void Initialize(List<TcgCard> cards)
    {
        Cards = cards;
    }

    public IEnumerator CardListRequest()
    {
        Transaction<List<TcgCard>> t = new Transaction<List<TcgCard>>();
        yield return StartCoroutine(t.HttpGetRequest("http://gbackdesigns.com/dealfinder/dealfinder/mobile/"));
        Cards = t.GetResponse();
    }
}