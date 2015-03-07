using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DealFinder.Network.Models;

public class AppStartup : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        StartCoroutine(Startup());
        
	
	}

    IEnumerator Startup()
    {
        Transaction<List<TcgCard>> t = new Transaction<List<TcgCard>>();
        yield return StartCoroutine(t.HttpGetRequest("http://gbackdesigns.com/dealfinder/dealfinder/mobile/"));
        List<TcgCard> cards = t.GetResponse();
        Debug.Log(cards[0].ProductName);
    }
}
