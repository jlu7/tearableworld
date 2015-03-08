using UnityEngine;
using System.Collections;

public class NumberToCalculate : MonoBehaviour {

	public int NumToCalculate = 0;

	public void IncreaseDigit(int num)
	{
		NumToCalculate = NumToCalculate * 10 + num;
	}

	public int Subtract()
	{
		return -NumToCalculate;
	}

	public int Add()
	{
		return NumToCalculate;
	}
}
