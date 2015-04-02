using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DealFinder.Network.Models;

class CardDataManager
{
    private static CardDataManager CDM;

    public static CardDataManager GetInstance()
    {
        if (CDM == null)
        {
            CDM = new CardDataManager();
        }
        return CDM;
    }


    public List<TcgCard> Cards;

    public void Initialize(List<TcgCard> cards)
    {
        Cards = cards;
    }
}
