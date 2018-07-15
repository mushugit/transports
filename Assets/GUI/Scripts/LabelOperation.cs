using UnityEngine;
using UnityEngine.EventSystems;

public class LabelOperation : MonoBehaviour, IPointerClickHandler
{
	public Component objectRenderer;

	public void OnPointerClick(PointerEventData eventData)
	{
        var constructionObjectRenderer = objectRenderer.GetComponentInChildren<IUnityObjectRenderer>();
        constructionObjectRenderer.NestedConstruction.ClickCallback(eventData);
	}
}
