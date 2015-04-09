using System.Globalization;
using System.Linq;
using DealFinder.Network.Models;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrontPageController : MonoBehaviour
{
    private Transform GridRef;
    public List<CardObject> CardHistory = new List<CardObject>();

    void Start()
    {
        GridRef = transform.FindChild("Grid");
        
        foreach (TcgCard card in CardDataManager.GetInstance().Cards)
        {
            InstantiateNewCard(card);
        }

        SortByPrice();
        CreateList();
    }

    void CreateList()
    {
        int count = 0;
        foreach (CardObject x in CardHistory)
        {
            count++;
            x.Position = count;
            if (x.Position >= 10)
            {
                x.GO.name = "00" + x.Position;
            }
            else if (x.Position >= 100)
            {
                x.GO.name = "0" + x.Position;
            }
            else if (x.Position >= 1000)
            {
                x.GO.name = x.Position.ToString();
            }
            else if (x.Position >= 10000)
            {
                CardHistory.Remove(x);
            }
            else
            {
                x.GO.name = "000" + x.Position;
            }
        }
        GridRef.GetComponent<UIGrid>().repositionNow = true;
    }

    public void InstantiateNewCard(TcgCard newCard)
    {
        GameObject newGO = Instantiate(Resources.Load<GameObject>("FrontPage/Item")) as GameObject;

        CardObject tmp = new CardObject(newCard, newGO);

        CardHistory.Add(tmp);

        newGO.name = "000" + 0;
        newGO.transform.parent = GridRef;
        newGO.transform.Find("Label").localScale = new Vector3(24, 24, 1);
        newGO.transform.Find("Label").GetComponent<UILabel>().text = newCard.Name + " " + newCard.AvgPrice;
        newGO.transform.localScale = new Vector3(1, 1, 1);
    }

    void SortByPrice()
    {
        CardHistory.Sort((CardObject node1, CardObject node2) => node1.TheCard.AvgPrice.CompareTo(node2.TheCard.AvgPrice));
        CardHistory.Reverse();
    }
}

public class CardObject
{
    public int Position = 0;
    public TcgCard TheCard;
    public GameObject GO;

    public CardObject(TcgCard aCard, GameObject GORef)
    {
        TheCard = aCard;
        GO = GORef;
    }
}