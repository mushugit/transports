using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Screenshot : MonoBehaviour
{

	private static Screenshot instance;

	public static readonly string Extention = ".png";
	public static readonly string Folder = "screenshots";

	private static string screenshotPath;

	private Camera cam;
	private bool takeScreenshotOnNextFrame;

	private void Awake()
	{
		instance = this;
		screenshotPath = Application.persistentDataPath + Path.DirectorySeparatorChar + Folder + Path.DirectorySeparatorChar;
		Directory.CreateDirectory(screenshotPath);
		cam = gameObject.GetComponent<Camera>();
	}

	private void OnPostRender()
	{
		if (takeScreenshotOnNextFrame)
		{
			takeScreenshotOnNextFrame = false;
			var localRenderTexture = cam.targetTexture;

			var renderResult = new Texture2D(localRenderTexture.width, localRenderTexture.height, TextureFormat.ARGB32, false);
			var rect = new Rect(0, 0, localRenderTexture.width, localRenderTexture.height);
			renderResult.ReadPixels(rect, 0, 0);

			var screenshotData = renderResult.EncodeToPNG();
			var now = DateTime.Now;
			var fileName = $"screenshot_{now.ToString("yyy-MM-dd")}_{now.ToString("HH-mm-ss-ffff")}";
			var screenshotFullFilePath = screenshotPath + fileName + Extention;
			Debug.Log($"Screenshot sauvegardé dans {screenshotFullFilePath}");
			File.WriteAllBytes(screenshotFullFilePath, screenshotData);
			InfoText.Display($"Screenshot sauvegardé sous {fileName}");
		}
	}

	private void TakeScreenshot(int width, int height)
	{
		cam.targetTexture = RenderTexture.GetTemporary(width, height, 16);
		takeScreenshotOnNextFrame = true;
	}

	public static void Take(int width, int height)
	{
		instance.TakeScreenshot(width, height);
	}
}
