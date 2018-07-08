using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownItem : MonoBehaviour {
	public DropdownManager manager;

	private void Start()
	{	
		if (manager.DisabledItems.Contains(transform.GetSiblingIndex()-1))
		{
			var toggle = GetComponent<Toggle>();
			toggle.interactable = false;
		}
	}
}
