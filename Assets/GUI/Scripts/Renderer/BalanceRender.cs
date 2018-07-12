using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalanceRender : MonoBehaviour {
	public Text balanceLabel;

	void Update () {
		balanceLabel.text = $"{World.LocalEconomy?.Balance.ToString("C0")} ";
	}
}
