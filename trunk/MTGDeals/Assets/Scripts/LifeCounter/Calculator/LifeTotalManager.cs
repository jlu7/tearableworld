using System.Globalization;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LifeTotalManager : MonoBehaviour
{
    private Transform GridRef;
	public LinkedList<LifeTotalObject> LifeTotalHistory;
	
    void Start()
    {
        GridRef = transform.FindChild("DragablePanel/Grid");
        LifeTotalHistory = new LinkedList<LifeTotalObject>();
		InstantiateNewNumber(13);
		InstantiateNewNumber(12);
		InstantiateNewNumber(11);
		InstantiateNewNumber(10);
		InstantiateNewNumber(9);
		InstantiateNewNumber(8);
		InstantiateNewNumber(7);
		InstantiateNewNumber(6);
		InstantiateNewNumber(5);
		InstantiateNewNumber(4);
		InstantiateNewNumber(3);
		InstantiateNewNumber(2);
		InstantiateNewNumber(1);
		InstantiateNewNumber(0);
    }

    void InstantiateNewNumber(int num)
    {
        GameObject newNum = Instantiate(Resources.Load<GameObject>("LifeCounter/Number")) as GameObject;
		LifeTotalObject newLTO = new LifeTotalObject(newNum); 
		newNum.name = "000" + 0;
        newNum.transform.parent = GridRef;
        newNum.transform.Find("Label").GetComponent<UILabel>().text = num.ToString(CultureInfo.InvariantCulture);
        newNum.transform.localScale = new Vector3(1, 1, 1);
		foreach(LifeTotalObject x in LifeTotalHistory)
		{
			x.Position++;
			if (x.Position >= 10)
			{
				x.Life.name = "00" + x.Position;
			}
			else if (x.Position >= 100)
			{
				x.Life.name = "0" + x.Position;
			}
			else if (x.Position >= 1000)
			{
				x.Life.name = x.Position.ToString();
			}
			else if (x.Position >= 10000)
			{
				LifeTotalHistory.Remove(x);
			}
			else
			{
				x.Life.name = "000" + x.Position;
			}
		}
        LifeTotalHistory.AddFirst(newLTO);
        GridRef.GetComponent<UIGrid>().repositionNow = true;
    }
}

public class LifeTotalObject
{
	public int Position = 0;
	public GameObject Life;

	public LifeTotalObject(GameObject life)
	{
		Life = life;
	}
}