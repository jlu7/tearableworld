using UnityEngine;
using System.Collections;
using System;

public class Subtract : MonoBehaviour
{
	private LifeTotalManager LifeTotalManagerRef;
	
	public void Start()
	{
		//probably just replace this
		LifeTotalManagerRef = this.GetComponentInParent<LifeTotalManager>();
	}
	
	void OnClick()
	{
		LifeTotalManagerRef.CreateSubtractingCalculator();
	}
}
