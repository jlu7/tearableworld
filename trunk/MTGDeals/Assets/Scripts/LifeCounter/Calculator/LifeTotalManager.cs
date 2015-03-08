using System.Globalization;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LifeTotalManager : MonoBehaviour
{
    private Transform GridRef;
	public LinkedList<GameObject> LifeTotalHistory;
    private int History;

    void Start()
    {
        GridRef = transform.FindChild("DragablePanel/Grid");
        LifeTotalHistory = new LinkedList<GameObject>();
        InstatiateNewNumber(0);
        InstatiateNewNumber(1);
        InstatiateNewNumber(2);
        InstatiateNewNumber(3);
        InstatiateNewNumber(4);
        InstatiateNewNumber(5);
        GridRef.GetComponent<UIGrid>().repositionNow = true;
    }

    void InstatiateNewNumber(int num)
    {
        GameObject newNum = Resources.Load<GameObject>("LifeCounter/Number");
        newNum.transform.parent = GridRef;
        newNum.transform.Find("Label").GetComponent<UILabel>().text = num.ToString(CultureInfo.InvariantCulture);
        newNum.transform.localScale = new Vector3(1, 1, 1);
        LifeTotalHistory.AddFirst(newNum);
    }
}
