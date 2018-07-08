using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour {

	public GameObject select;
	public GameObject ok;
	public GameObject ko;
	public GameObject okDirectional;

	public void Select()
	{
		select.SetActive(true);
		ok.SetActive(false);
		ko.SetActive(false);
		okDirectional.SetActive(false);
	}

	public void Ok()
	{
		select.SetActive(false);
		ok.SetActive(true);
		ko.SetActive(false);
		okDirectional.SetActive(false);
	}

	public void KO()
	{
		select.SetActive(false);
		ok.SetActive(false);
		ko.SetActive(true);
		okDirectional.SetActive(false);
	}

	public void OkDirectional()
	{
		select.SetActive(false);
		ok.SetActive(false);
		ko.SetActive(false);
		okDirectional.SetActive(true);
	}
}
