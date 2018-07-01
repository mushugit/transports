using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownManager : MonoBehaviour
{

	public List<int> DisabledItems;
	public Dropdown otherDropDown;
	public Dropdown dropdown;

	private void Start()
	{
		var dropdown = GetComponent<Dropdown>();
		dropdown.onValueChanged.AddListener(delegate
		{
			OnValueChanged(dropdown);
		});
	}


	public void OnValueChanged(Dropdown change)
	{
		var myIndex = change.value;
		var otherIndex = otherDropDown.value;
		var otherDropdownManager = otherDropDown.GetComponent<DropdownManager>();
		if(myIndex == otherIndex)
		{
			otherDropdownManager.SelectNext();
		}
		otherDropdownManager.EnableAllItems();
		otherDropdownManager.DisableItem(myIndex);
	}

	public void SelectNext()
	{
		if (dropdown.value + 1 == dropdown.options.Count)
			dropdown.value = 0;
		else
			dropdown.value = dropdown.value + 1;
		dropdown.RefreshShownValue();
	}


	public void DisableItem(Int32 index)
	{
		if (!DisabledItems.Contains(index))
		{
			DisabledItems.Add(index);
		}
	}

	public void EnableAllItems()
	{
		DisabledItems.Clear();
	}
}
