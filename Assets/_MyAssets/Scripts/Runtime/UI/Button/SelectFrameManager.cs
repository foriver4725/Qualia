namespace MyScripts.Runtime.UI.Button
{
    internal sealed class SelectFrameManager : ASingletonMonoBehaviour<SelectFrameManager>
    {
        [SerializeField] private Image image;
        [SerializeField] private RectTransform controllerIcon;
        [SerializeField] private SGameConfig gameConfig;

        // 9-sliced の外側8マスの幅 [px]
        // プログラム内で数値を決め打っておく
        // 外側の幅は、全て同じ前提
        private const int OuterSize = 32;

        private AButtonManager selectingButton = null;

        /// <summary>
        /// 指定したボタンが現在選択されているなら、<br/>
        /// 何も選択していない状態にする<br/>
        /// 重複実行は気にしなくて良い<br/>
        /// </summary>
        internal void Deselect(AButtonManager button)
        {
            if (!gameConfig.DoesAlwaysShowGamepadButtonFrame)
                if (InputManager.GetCurrentDevice() != InputManager.Device.Gamepad) return;

            if (!selectingButton) return;
            if (selectingButton != button) return;

            image.gameObject.SetActive(false);
            selectingButton = null;
        }

        /// <summary>
        /// 指定したボタンを新たに選択する<br/>
        /// 重複実行は気にしなくて良い<br/>
        /// </summary>
        internal void Reselect(AButtonManager button, float padding = 0.0f)
        {
            if (!gameConfig.DoesAlwaysShowGamepadButtonFrame)
                if (InputManager.GetCurrentDevice() != InputManager.Device.Gamepad) return;

            if (!selectingButton)
                image.gameObject.SetActive(true);
            selectingButton = button;

            image.rectTransform.anchoredPosition =
                button.Position
                + Vector2.right * ((button.RaycastPadding.x - button.RaycastPadding.z) * 0.5f) // RaycastPadding 分を補正 (Left != Right)
                + Vector2.up * ((button.RaycastPadding.y - button.RaycastPadding.w) * 0.5f); // RaycastPadding 分を補正 (Bottom != Up)

            image.rectTransform.sizeDelta =
                button.Size // ボタンの基本サイズ
                - Vector2.right * (button.RaycastPadding.x + button.RaycastPadding.z) // RaycastPadding 分を引く (Left, Right)
                - Vector2.up * (button.RaycastPadding.y + button.RaycastPadding.w) // RaycastPadding 分を引く (Bottom, Up)
                + Vector2.one * ((OuterSize + padding) * 2.0f); // 9-sliced の外側8マス分 + 余白 を足す
            controllerIcon.localPosition = (image.rectTransform.sizeDelta - Vector2.one * OuterSize) * 0.5f;
        }
    }
}
