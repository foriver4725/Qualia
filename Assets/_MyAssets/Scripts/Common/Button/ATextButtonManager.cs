using UnityEngine.EventSystems;

namespace MyScripts.Common.Button
{
    /// <summary>
    /// Image, Text で構成される
    /// 見た目の変化などは、基本的にこのクラス内で行う
    /// Awake, OnDisable を使用
    /// </summary>
    internal abstract class ATextButtonManager : AButton
    {
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI text;

        [SerializeField] private string displayText;
        [SerializeField] private Color textNormalColor;
        [SerializeField] private Color textHoveredColor;
        [SerializeField] private Color textClickedColor;
        [SerializeField] private Color backgroundNormalColor;
        [SerializeField] private Color backgroundHoveredColor;
        [SerializeField] private Color backgroundClickedColor;

        private Vector3 imageInitialScale;
        private Vector3 textInitialScale;

        private enum AppearanceState : byte
        {
            Default,      // 通常
            BeingHovered, // ホバーされている
            BeingClicked, // クリックされている
        }

        private AppearanceState appearanceState = AppearanceState.Default;

        // PointerUpの時、ホバー状態に戻すか・通常状態に戻すか、判別するためのもの
        private bool isPointerInside = false;

        // Down/Upの所では、最初にDownされたポインターのみを追跡するようにする
        // DownされてからUpされたら、追跡状態はリセット(-1)される
        private int trackingPointerId = -1;

        private void Awake()
        {
            if (backgroundImage != null)
            {
                imageInitialScale = backgroundImage.rectTransform.localScale;

                backgroundImage.color = backgroundNormalColor;
            }

            if (text != null)
            {
                textInitialScale = text.rectTransform.localScale;

                text.text  = displayText;
                text.color = textNormalColor;

                // ここでのみフォントサイズを変更している. そのため、派生クラスで以降いじってもOK
                text.fontSize = (displayText?.Length ?? 0) switch
                {
                    <= 4 => 120.0f,
                    5    => 90.0f,
                    6    => 78.0f,
                    7    => 66.0f,
                    8    => 60.0f,
                    _    => 12.0f
                };
            }

            if (eventTrigger != null)
            {
                eventTrigger.AddListener(EventTriggerType.PointerEnter, OnEnter);
                eventTrigger.AddListener(EventTriggerType.PointerExit, OnExit);
                eventTrigger.AddListener(EventTriggerType.PointerDown, OnDown);
                eventTrigger.AddListener(EventTriggerType.PointerUp, OnUp);
            }
        }

        private void OnDisable()
        {
            appearanceState   = AppearanceState.Default;
            isPointerInside   = false;
            trackingPointerId = -1;

            if (backgroundImage != null)
            {
                backgroundImage.color                    = backgroundNormalColor;
                backgroundImage.rectTransform.localScale = imageInitialScale;
            }

            if (text != null)
            {
                text.color                    = textNormalColor;
                text.rectTransform.localScale = textInitialScale;
            }

            OnExitImpl();
        }

        // 概ねPCのみ
        // カーソルが範囲内に入った
        // カーソルが中にあるかのフラグを更新
        public sealed override void OnEnter(PointerEventData data)
        {
            // モバイルのみ
            // 他の指からのEnterは無視
            if (trackingPointerId != -1 && trackingPointerId != data.pointerId)
                return;

            isPointerInside = true;

            if (!CanEnter) return;

            if (appearanceState != AppearanceState.Default) return;
            appearanceState = AppearanceState.BeingHovered;

            PlayHoverSe();
            UpdateAppearences();

            OnEnterImpl();
        }

        // 概ねPCのみ
        // カーソルが範囲内から出た
        // カーソルが中にあるかのフラグを更新
        public sealed override void OnExit(PointerEventData data)
        {
            // モバイルのみ
            // 他の指からのExitは無視
            if (trackingPointerId != -1 && trackingPointerId != data.pointerId)
                return;

            isPointerInside = false;

            if (!CanExit) return;

            if (appearanceState != AppearanceState.BeingHovered) return;
            appearanceState = AppearanceState.Default;

            UpdateAppearences();

            OnExitImpl();
        }

        // 範囲内でボタンを押す(タップ)した時
        public sealed override void OnDown(PointerEventData data)
        {
            // モバイルのみ
            // IDを追跡開始
            if (trackingPointerId != -1) return;
            trackingPointerId = data.pointerId;

            if (!CanDown) return;

            if (appearanceState != AppearanceState.BeingHovered) return;
            appearanceState = AppearanceState.BeingClicked;

            PlayClickSe();
            UpdateAppearences();

            OnDownImpl();
        }

        // PointerDown後にボタン(指)を放した時
        public sealed override void OnUp(PointerEventData data)
        {
            // モバイルのみ
            // IDを追跡終了
            if (trackingPointerId != data.pointerId) return;
            trackingPointerId = -1;

            if (!CanUp) return;

            if (appearanceState != AppearanceState.BeingClicked) return;
            appearanceState = isPointerInside ? AppearanceState.BeingHovered : AppearanceState.Default;

            UpdateAppearences();

            OnUpImpl();

            // 自身の範囲内でボタン(指)を放した場合、クリック成功
            if (isPointerInside)
                OnClickSucceeded();
        }

        private void UpdateAppearences()
        {
            (Color textColor, Color backgroundColor, float scaleCoef) = appearanceState switch
            {
                AppearanceState.Default      => (textNormalColor, backgroundNormalColor, 1.0f),
                AppearanceState.BeingHovered => (textHoveredColor, backgroundHoveredColor, 1.05f),
                AppearanceState.BeingClicked => (textClickedColor, backgroundClickedColor, 1.1f),
                _                            => (textNormalColor, backgroundNormalColor, 1.0f)
            };

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
                backgroundImage.rectTransform.DOScale(imageInitialScale * scaleCoef, 0.1f).SetEase(Ease.OutBack);
            }

            if (text != null)
            {
                text.color = textColor;
                text.rectTransform.DOScale(textInitialScale * scaleCoef, 0.1f).SetEase(Ease.OutBack);
            }
        }

        #region 派生クラスに公開

        // 各コールバック時、このプロパティがfalseを返すなら実行されない
        // ただし、フラグの管理などは行われる
        private protected virtual bool CanEnter => true;
        private protected virtual bool CanExit => true;
        private protected virtual bool CanDown => true;
        private protected virtual bool CanUp => true;

        private protected virtual void OnEnterImpl()
        {
        }

        private protected virtual void OnExitImpl()
        {
        }

        private protected virtual void OnDownImpl()
        {
        }

        private protected virtual void OnUpImpl()
        {
        }

        private protected virtual void OnClickSucceeded()
        {
        }

        private protected virtual void PlayHoverSe()
        {
        }

        private protected virtual void PlayClickSe()
        {
        }

        // このスクリプトでやっていないプロパティ操作を行いたい場合に限る.
        private protected EventTrigger EventTrigger => eventTrigger;
        private protected Image BackgroundImage => backgroundImage;
        private protected TextMeshProUGUI Text => text;
        private protected string DisplayText => displayText;
        private protected Color TextNormalColor => textNormalColor;
        private protected Color TextHoveredColor => textHoveredColor;

        #endregion
    }
}
