using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WindowFluxSetup : Window
{
	public WindowFluxSetup(GameObject windowObject, Vector3 initialPosition) : base(windowObject, initialPosition)
	{

	}

	public void TextContent(string richTextContent)
	{
		if (windowObject != null)
		{
			var wtc = windowObject.GetComponent<WindowTextContent>();
			if (wtc != null)
			{
				wtc.ContentText(richTextContent);
			}
		}
	}


	public override void Close()
	{
		
	}
}

