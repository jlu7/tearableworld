using UnityEngine;
using System.Collections;
using System;

public class Clear : MonoBehaviour 
{
	private NumberToCalculate RefToCalc;
	
	public void Start()
	{
		RefToCalc = this.transform.parent.parent.GetComponentInParent<NumberToCalculate>();
	}
	
	void OnClick()
	{
		RefToCalc.ClearDigit();
		Debug.Log(RefToCalc.TheNum);
	}
}
