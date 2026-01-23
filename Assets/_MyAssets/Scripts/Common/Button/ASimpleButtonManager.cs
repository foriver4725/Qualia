using UnityEngine.EventSystems;

namespace MyScripts.Common.Button
{
    /// <summary>
    /// SpriteRenderer で構成される
    /// 見た目の変化などは、基本的にこのクラス内で行う
    /// Awake, OnDisable を使用
    /// </summary>
    internal abstract class ASimpleButtonManager : AButton
    {
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private SpriteRenderer image;

        private Vector3 imageInitialScale;

        private enum AppearanceState : byte
        {
            Default,      // 通常
            BeingHovered, // ホバーされている
            BeingClicked, // クリックされている
        }

        private AppearanceState appearanceState = AppearanceState.Default;

        private MotionHandle tween = default;

        // PointerUpの時、ホバー状態に戻すか・通常状態に戻すか、判別するためのもの
        private bool isPointerInside = false;

        // Down/Upの所では、最初にDownされたポインターのみを追跡するようにする
        // DownされてからUpされたら、追跡状態はリセット(-1)される
        private int trackingPointerId = -1;

        private void Awake()
        {
            OnJustBeforeAwake();

            if (image != null)
                imageInitialScale = image.transform.localScale;

            if (eventTrigger != null)
            {
                eventTrigger.AddListener(EventTriggerType.PointerEnter, OnEnter);
                eventTrigger.AddListener(EventTriggerType.PointerExit, OnExit);
                eventTrigger.AddListener(EventTriggerType.PointerDown, OnDown);
                eventTrigger.AddListener(EventTriggerType.PointerUp, OnUp);
            }

            OnJustAfterAwake();
        }

        private void OnDisable()
        {
            OnJustBeforeDisable();

            appearanceState = AppearanceState.Default;
            isPointerInside = false;
            trackingPointerId = -1;

            if (image != null)
            {
                image.transform.localScale = imageInitialScale;
            }

            OnExitImpl();

            OnJustAfterDisable();
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
            UpdateAppearances();

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

            UpdateAppearances();

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
            UpdateAppearances();

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

            UpdateAppearances();

            OnUpImpl();

            // 自身の範囲内でボタン(指)を放した場合、クリック成功
            if (isPointerInside)
                OnClickSucceeded();
        }

        private void UpdateAppearances()
        {
            float scaleCoef = appearanceState switch
            {
                AppearanceState.Default => DefaultScaleCoef,
                AppearanceState.BeingHovered => HoveredScaleCoef,
                AppearanceState.BeingClicked => ClickedScaleCoef,
                _ => DefaultScaleCoef
            };

            if (image)
            {
                if (tween.IsActive())
                    tween.Cancel();
                tween = LMotion.Create(image.transform.localScale, imageInitialScale * scaleCoef, ScaleAnimationDuration)
                    .WithEase(Ease.OutBack)
                    .BindToLocalScale(image.transform);
            }
        }

        #region 派生クラスに公開

        private protected virtual float DefaultScaleCoef => 1.0f;
        private protected virtual float HoveredScaleCoef => 1.05f;
        private protected virtual float ClickedScaleCoef => 1.1f;
        private protected virtual float ScaleAnimationDuration => 0.1f;

        // 各コールバック時、このプロパティがfalseを返すなら実行されない
        // ただし、フラグの管理などは行われる
        private protected virtual bool CanEnter => true;
        private protected virtual bool CanExit => true;
        private protected virtual bool CanDown => true;
        private protected virtual bool CanUp => true;

        private protected virtual void OnJustBeforeAwake()
        {
        }

        private protected virtual void OnJustAfterAwake()
        {
        }

        private protected virtual void OnJustBeforeDisable()
        {
        }

        private protected virtual void OnJustAfterDisable()
        {
        }

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
        private protected SpriteRenderer Image => image;

        #endregion
    }
}
