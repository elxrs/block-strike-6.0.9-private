using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Center Scroll View on Click")]
public class UICenterOnClick : MonoBehaviour
{
	private void OnClick()
	{
		UICenterOnChild uICenterOnChild = NGUITools.FindInParents<UICenterOnChild>(gameObject);
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(gameObject);
		if (uICenterOnChild != null)
		{
			if (uICenterOnChild.enabled)
			{
				uICenterOnChild.CenterOn(transform);
			}
		}
		else if (uIPanel != null && uIPanel.clipping != 0)
		{
			UIScrollView component = uIPanel.GetComponent<UIScrollView>();
			Vector3 pos = -uIPanel.cachedTransform.InverseTransformPoint(transform.position);
			if (!component.canMoveHorizontally)
			{
				pos.x = uIPanel.cachedTransform.localPosition.x;
			}
			if (!component.canMoveVertically)
			{
				pos.y = uIPanel.cachedTransform.localPosition.y;
			}
			SpringPanel.Begin(uIPanel.cachedGameObject, pos, 6f);
		}
	}
}
