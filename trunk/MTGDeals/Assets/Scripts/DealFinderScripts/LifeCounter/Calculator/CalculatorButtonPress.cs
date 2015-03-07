using UnityEngine;
using System.Collections;
using System;

public class CalculatorButtonPress : MonoBehaviour
{
	public int Num;
	private NumberToCalculate RefToCalc;

	public void Start()
	{
		Num = Convert.ToInt32(this.transform.parent.name);
		//probably just replace this
		RefToCalc = this.transform.parent.parent.GetComponentInParent<NumberToCalculate>();
	}

	void OnClick()
	{
		RefToCalc.IncreaseDigit(Num);
		Debug.Log(RefToCalc.NumToCalculate);
	}
}
