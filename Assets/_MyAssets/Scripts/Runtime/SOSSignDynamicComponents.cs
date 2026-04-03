namespace MyScripts.Runtime
{
    /// <summary>
    /// ゲームシーンから動的に取得する必要のある、SOSサインが参照するコンポーネント<br/>
    /// このクラスに参照をまとめておいて、各SOSサインが生成された際に参照を取得できるようにする<br/>
    /// </summary>
    internal sealed class SOSSignDynamicComponents : ASingletonMonoBehaviour<SOSSignDynamicComponents>
    {
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;

        internal AnimalLeaveInvoker AnimalLeaveInvoker => animalLeaveInvoker;
    }
}