using UnityEngine.Video;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CutScene", menuName = "SO/InGame/CutScene")]
    internal sealed class SCutScene : ScriptableObject
    {
        [Header("ブラウザ上の動画URLを指定する")]

        [SerializeField] private string introUrl;

        internal enum CutSceneType : byte
        {
            Intro, // 導入 & チュートリアル
        }

        internal string Get(CutSceneType type) => type switch
        {
            CutSceneType.Intro => introUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
