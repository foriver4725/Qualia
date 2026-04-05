namespace MyScripts.Runtime
{
    /// <summary>
    /// ゲームシーンから動的に取得する必要のある、アニマが参照するコンポーネント<br/>
    /// このクラスに参照をまとめておいて、各アニマが生成された際に参照を取得できるようにする<br/>
    /// </summary>
    internal sealed class AnimaDynamicComponents : ASingletonMonoBehaviour<AnimaDynamicComponents>
    {
        [SerializeField] private PlayerController pc;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        internal PlayerController Pc => pc;
        internal Camera PlayerCamera => playerCamera;
        internal AnimalLeaveInvoker AnimalLeaveInvoker => animalLeaveInvoker;
        internal SOSSoundPlayer SoundPlayer => soundPlayer;
    }
}