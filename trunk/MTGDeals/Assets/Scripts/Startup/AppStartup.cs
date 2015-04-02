using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DealFinder.Network.Models;

public class AppStartup : MonoBehaviour
{
    public GameObject AnchorRef;

	// Use this for initialization
	void Start () 
    {
        StartCoroutine(Startup());
	}

    IEnumerator Startup()
    {
        //Initiate The Singleton
        Transaction<List<TcgCard>> t = new Transaction<List<TcgCard>>();
        yield return StartCoroutine(t.HttpGetRequest("http://gbackdesigns.com/dealfinder/dealfinder/mobile/"));
        List<TcgCard> cards = t.GetResponse();
        CardDataManager.GetInstance().Initialize(cards);
        Debug.Log(CardDataManager.GetInstance().Cards[0].ProductName);

        // Create The FrontPage
        GameObject FrontPage = Instantiate(Resources.Load<GameObject>("FrontPage/FrontPage")) as GameObject;
        FrontPage.transform.parent = AnchorRef.transform;
        FrontPage.transform.localScale = new Vector3(1, 1, 1);
        FrontPage.transform.localPosition = new Vector3(0, 220, 0);
    }
}
