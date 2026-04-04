using MyScripts.Runtime.Log;

namespace MyScripts.Runtime
{
    internal sealed class AnimalLeaveInvoker : MonoBehaviour
    {
        // 各スロット(0〜2)のUI Imageセット。表示順は displayOrder に従う
        [SerializeField] private Image[] displayImageBgBacks; // length 3
        [SerializeField] private Image[] displayImageBgs;     // length 3
        [SerializeField] private Image[] displayImages;       // length 3
        [SerializeField] private SAnima sAnima;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // 陸のアニマは陸のSOSサインを表示させるので、参照が必要
        private readonly HashSet<SOSSign> landSOSSigns = new(capacity: 1024);

        // スロットごとの憑依中 Character（データ層: 種別キー）
        private readonly Dictionary<CharacterType, Character> possessingCharacters = new()
        {
            { CharacterType.Land, null },
            { CharacterType.Sea, null },
            { CharacterType.Sky, null },
        };

        // スロットごとのタイマー CTS（種別キー）
        private readonly Dictionary<CharacterType, CancellationTokenSource> possessTimerCtses = new()
        {
            { CharacterType.Land, null },
            { CharacterType.Sea, null },
            { CharacterType.Sky, null },
        };

        // 初めて取得したタイミングで true になり、以降二度と false にならない
        // 一回だけログで知らせるために使う
        private readonly Dictionary<CharacterType, bool> hasPossessedForTheFirstTimeTable = new()
        {
            { CharacterType.Land, false },
            { CharacterType.Sea, false },
            { CharacterType.Sky, false },
        };

        // 表示層: 取得順を記録する（インデックス = 表示スロット番号）。最大3要素
        private readonly List<CharacterType> displayOrder = new(3);

        private void Awake()
        {
            RefreshAllDisplaySlots();
        }

        // SOSサインの生成タイミングがゲーム開始直後なので、それが終わったら生成されたSOSから自身を追加してもらう
        //! なるべく速く実行し終わること!!
        internal void AddLandSOSSign(SOSSign sosSign) => _ = landSOSSigns.Add(sosSign);

        // 任意スロットに1体でも憑依していれば true（SOSSignFindManager から使用）
        internal bool IsPossessing
        {
            get
            {
                foreach (var c in possessingCharacters.Values)
                    if (c != null)
                        return true;
                return false;
            }
        }

        // 指定種別のスロットに憑依中か（PlayerController から使用）
        internal bool IsPossessingType(CharacterType type) => possessingCharacters[type] != null;

        // 指定スロットのタイマーをリセットし、タイマー用のトークンを返す
        // 取得（上書き）するたびに必ず呼ぶ
        internal Ct ResetPossessTimer(CharacterType type)
        {
            possessTimerCtses[type]?.Cancel();
            possessTimerCtses[type]?.Dispose();
            possessTimerCtses[type] = new Cts();
            return possessTimerCtses[type].Token;
        }

        // アニマを取得する（対応スロットに格納）
        // アニマを見えなくする (当たり判定も無効化)
        //! PossessCharacter_ShowLogIfFirstTime() のなるべく直前で呼び出すこと!! (引数は同じにする)
        internal void PossessCharacter(Character character)
        {
            var type = character.CharacterType;

            // 同スロットに既存キャラがあれば内部解放（上書き取得）
            if (possessingCharacters[type] != null)
            {
                LeaveCharacterInternal(type);
            }

            possessingCharacters[type] = character;
            possessingCharacters[type].SetVisible(false);
            possessingCharacters[type].Collider.enabled = false;

            // 陸のアニマなら、陸のSOSサインを表示させる
            if (type == CharacterType.Land)
                foreach (var sosSign in landSOSSigns)
                    sosSign.TrySetActiveSmokeOnlyWhenLand(true);

            // 新規取得時のみ displayOrder に追加（上書き時は既存の表示位置を保持）
            if (!displayOrder.Contains(type))
                displayOrder.Add(type);

            RefreshAllDisplaySlots();
            // TODO: SOSサインのサウンドを使いまわす!
            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
        }

        // 初めて取得した場合、ログを表示する
        // 初取得かどうかのフラグテーブルから検索し、その値を更新する
        //! PossessCharacter() のなるべく直後で呼び出すこと!! (引数は同じにする)
        internal void PossessCharacter_ShowLogIfFirstTime(Character character)
        {
            var type = character.CharacterType;

            // 既に取得済みなら何もしない（上書き・タイマーリセット時の重複呼び出しに対して安全）
            if (hasPossessedForTheFirstTimeTable[type]) return;

            hasPossessedForTheFirstTimeTable[type] = true;

            LogManager2.Instance.ShowAutomatically(type switch
            {
                CharacterType.Land => "陸の移動速度が向上し、一部のSOSの位置が可視化された！",
                CharacterType.Sea  => "水上の移動速度が大幅に向上した！",
                CharacterType.Sky  => "空中で再度ジャンプすると大ジャンプし、落下時に滑空するようになった！",
                _                  => ""
            });
        }

        // 指定スロットのアニマから離脱する
        // アニマを見えるようにする (当たり判定も有効化)
        internal void LeaveCharacter(PlayerController pc, CharacterType type)
        {
            if (possessingCharacters[type] == null)
            {
                $"憑依中のアニマがありません。({type})".Print(LogSettings.Error);
                return;
            }

            LeaveCharacterInternal(type);

            // タイマー満了時など、CTS が残っている場合は解放する
            possessTimerCtses[type]?.Cancel();
            possessTimerCtses[type]?.Dispose();
            possessTimerCtses[type] = null;

            // displayOrder から削除することで残りがシフトされる
            displayOrder.Remove(type);

            RefreshAllDisplaySlots();
            // TODO: SOSサインのサウンドを使いまわす!
            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
        }

        internal void SetDisplayImageFillAmount(CharacterType type, float amount)
        {
            int slotIndex = displayOrder.IndexOf(type);
            if (slotIndex < 0) return; // LeaveCharacter 後に呼ばれても安全にスキップ
            amount = Mathf.Clamp01(amount);
            displayImageBgs[slotIndex].fillAmount = amount;
        }

        // ======== private helpers ========

        // UI 更新なし・サウンドなしでスロットを解放する内部処理
        private void LeaveCharacterInternal(CharacterType type)
        {
            var character = possessingCharacters[type];
            if (character == null) return;

            // 陸のアニマなら、陸のSOSサインを非表示にする
            if (type == CharacterType.Land)
                foreach (var sosSign in landSOSSigns)
                    sosSign.TrySetActiveSmokeOnlyWhenLand(false);

            character.SetVisible(true);
            character.Collider.enabled = true;
            possessingCharacters[type] = null;
        }

        // displayOrder の現在順に従いスロット0〜2を一括更新
        // タイマー満了・新規取得・上書き取得いずれのタイミングでも必ず呼ぶ
        private void RefreshAllDisplaySlots()
        {
            for (int i = 0; i < 3; i++)
            {
                var character = i < displayOrder.Count
                    ? possessingCharacters[displayOrder[i]]
                    : null;
                UpdateDisplayImageForSlot(i, character);
            }
        }

        private void UpdateDisplayImageForSlot(int slotIndex, Character possessingCharacter)
        {
            if (possessingCharacter)
            {
                displayImageBgBacks[slotIndex].enabled = true;
                displayImageBgs[slotIndex].enabled = true;
                displayImages[slotIndex].enabled = true;

                // 不透明にする
                Color color = possessingCharacter.MaterialColor;
                color.a = 1.0f;
                displayImageBgs[slotIndex].color = color;

                displayImages[slotIndex].sprite = possessingCharacter.CharacterType switch
                {
                    CharacterType.Land => sAnima.LandIcon,
                    CharacterType.Sea  => sAnima.SeaIcon,
                    CharacterType.Sky  => sAnima.SkyIcon,
                    _                  => null
                };
            }
            else
            {
                displayImageBgBacks[slotIndex].enabled = false;
                displayImageBgs[slotIndex].enabled = false;
                displayImages[slotIndex].enabled = false;
            }
        }
    }
}