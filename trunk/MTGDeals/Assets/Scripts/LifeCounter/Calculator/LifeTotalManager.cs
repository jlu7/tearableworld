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
		InstantiateNewNumber(20);
    }

    public void InstantiateNewNumber(int num)
    {
        GameObject newNum = Instantiate(Resources.Load<GameObject>("LifeCounter/Number")) as GameObject;

		if (LifeTotalHistory.Count > 0)
		{
			LifeTotalObject newLTO = new LifeTotalObject(newNum, num + LifeTotalHistory.First.Value.Num); 
        	LifeTotalHistory.AddFirst(newLTO);
		}
		else
		{
			LifeTotalObject newLTO = new LifeTotalObject(newNum, num); 
        	LifeTotalHistory.AddFirst(newLTO);
		}

		newNum.name = "000" + 0;
        newNum.transform.parent = GridRef;
        newNum.transform.Find("Label").GetComponent<UILabel>().text = LifeTotalHistory.First.Value.Num.ToString(CultureInfo.InvariantCulture);
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
        GridRef.GetComponent<UIGrid>().repositionNow = true;
    }

	public void CreateAddingCalculator()
	{
		GameObject newCalc = Instantiate(Resources.Load<GameObject>("LifeCounter/Calculator")) as GameObject;
		newCalc.transform.parent = this.transform;
		newCalc.GetComponent<NumberToCalculate>().Setup(this, true);
		newCalc.transform.localScale = new Vector3(1, 1, 1);
	}

	public void CreateSubtractingCalculator()
	{
		GameObject newCalc = Instantiate(Resources.Load<GameObject>("LifeCounter/Calculator")) as GameObject;
		newCalc.transform.parent = this.transform;
		newCalc.GetComponent<NumberToCalculate>().Setup(this, false);
		newCalc.transform.localScale = new Vector3(1, 1, 1);
	}
}

public class LifeTotalObject
{
	public int Num = 0;
	public int Position = 0;
	public GameObject Life;

	public LifeTotalObject(GameObject life, int num)
	{
		Life = life;
		Num = num;
	}
}