namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CloudFileUrl", menuName = "SO/OutGame/Cloud File Url")]
    internal sealed class SCloudFileUrl : ScriptableObject
    {
        [Header("画像")]
        [SerializeField] private string image_empty;
        [Space(10)]

        [Header("動画")]
        [SerializeField] private string movie_intro;
        [Space(10)]

        [Header("サウンド")]
        [SerializeField] private string sound_empty;
        [Space(10)]

        [Header("テキスト")]
        [SerializeField] private string text_empty;
        // [Space(10)]

        internal enum FileType : byte
        {
            Movie_Intro, // 導入 & チュートリアル
        }

        internal string Get(FileType type) => type switch
        {
            FileType.Movie_Intro => movie_intro,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
