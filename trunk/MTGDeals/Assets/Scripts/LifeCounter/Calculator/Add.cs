using UnityEngine;
using System.Collections;
using System;

public class Add : MonoBehaviour
{
	private NumberToCalculate RefToCalc;
	
	public void Start()
	{
		//probably just replace this
		RefToCalc = this.transform.parent.parent.GetComponentInParent<NumberToCalculate>();
	}
	
	void OnClick()
	{
		RefToCalc.Add();
		Debug.Log(RefToCalc.NumToCalculate);
	}
}
