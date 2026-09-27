using UnityEngine;

public class GameManagerHelper : MonoBehaviour
{
    public IMenuScene sceneData;
    public void LoadMenuScene(int typeScene)
    {
        GameManager.TypeScene scene = (GameManager.TypeScene)typeScene;
        _ = GameManager.Instance.LoadMenuScene(scene);
    }
    public void UnloadMenuScene(int typeScene)
    {
        GameManager.TypeScene scene = (GameManager.TypeScene)typeScene;
        _ = GameManager.Instance.UnloadMenuScene(scene, sceneData);
    }
    public interface IMenuScene
    {
        public void PlayEndAnimation();
        public bool AnimationEnded();
    }
}
