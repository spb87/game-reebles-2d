// Runtime gate for the generated on-screen controls: the canvas hierarchy only
// activates on touch-capable devices (or when forceVisible is set for desktop
// testing), and its root is pinned to Screen.safeArea.

using UnityEngine;

namespace Reebles2D.UI
{
    /// <summary>
    /// Shows the mobile on-screen control hierarchy only when touch input is
    /// supported at runtime, and keeps it inside the display safe area.
    /// </summary>
    public class MobileControlsHud : MonoBehaviour
    {
        [SerializeField] private GameObject controlsRoot;
        [SerializeField] private RectTransform safeAreaTarget;
        [SerializeField] private bool forceVisible;

        private void Awake()
        {
            controlsRoot.SetActive(forceVisible || Input.touchSupported);
            ApplySafeArea();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (safeAreaTarget == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }
            Rect area = Screen.safeArea;
            Vector2 anchorMin = area.position;
            Vector2 anchorMax = area.position + area.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            safeAreaTarget.anchorMin = anchorMin;
            safeAreaTarget.anchorMax = anchorMax;
        }
    }
}
