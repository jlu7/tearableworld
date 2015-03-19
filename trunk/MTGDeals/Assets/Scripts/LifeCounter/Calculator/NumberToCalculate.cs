using UnityEngine;
using System.Collections;

public class NumberToCalculate : MonoBehaviour 
{
	/// <summary>
	/// This is a dumb class but I'm lazy so you need to check add or subract to be true
	/// </summary>
	/// 
	public int TheNum = 0;
	public bool FalseToSubtractTrueToAdd = false;
	private LifeTotalManager LTMRef;

	public void Setup(LifeTotalManager ltmRef, bool falseToSubtractTrueToAdd)
	{
		FalseToSubtractTrueToAdd = falseToSubtractTrueToAdd;
		LTMRef = ltmRef;
	}

	public void IncreaseDigit(int num)
	{
		if (TheNum <= 999999)
		{
			TheNum = TheNum * 10 + num;
		}
		this.transform.Find("Number").GetComponent<UILabel>().text = TheNum.ToString();
	}

	public void ClearDigit()
	{
		TheNum = 0;
		this.transform.Find("Number").GetComponent<UILabel>().text = TheNum.ToString();
	}

	public void ReturnNumber()
	{
		if (FalseToSubtractTrueToAdd == true)
		{
			LTMRef.InstantiateNewNumber(TheNum);
		}
		else 
		{
			LTMRef.InstantiateNewNumber(-TheNum);
		}
	}
}
