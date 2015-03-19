using UnityEngine;
using System.Collections;
using System;

public class Add : MonoBehaviour
{
	private LifeTotalManager LifeTotalManagerRef;
	
	public void Start()
	{
		//probably just replace this
		LifeTotalManagerRef = this.GetComponentInParent<LifeTotalManager>();
	}
	
	void OnClick()
	{
		LifeTotalManagerRef.CreateAddingCalculator();
	}
}
