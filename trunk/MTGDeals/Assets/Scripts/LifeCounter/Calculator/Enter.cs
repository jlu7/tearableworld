using UnityEngine;
using System.Collections;
using System;

public class Enter : MonoBehaviour 
{
	private NumberToCalculate RefToCalc;
	
	public void Start()
	{
		RefToCalc = this.transform.parent.parent.GetComponentInParent<NumberToCalculate>();
	}
	
	void OnClick()
	{
		RefToCalc.ReturnNumber();
		Debug.Log(RefToCalc.TheNum);
		Destroy(RefToCalc.gameObject);
	}
}
