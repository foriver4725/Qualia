namespace MyScripts.Runtime;

internal interface IOutGameSceneManager
{
}

internal interface IOutGameSceneManagerSingleTransition : IOutGameSceneManager
{
    void TransitToNextScene();
}

internal interface IOutGameSceneManagerMultiTransition : IOutGameSceneManager
{
    void TransitToScene(Scene scene);
}
