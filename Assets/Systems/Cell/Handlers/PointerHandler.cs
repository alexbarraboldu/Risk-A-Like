using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace ABB.Cell.Handlers
{
	public class PointerHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public event Action PointerClicked;

		public event Action<bool> PointerHovered;


		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			PointerClicked?.Invoke();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			PointerHovered?.Invoke(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			PointerHovered?.Invoke(false);
		}
	}
}
