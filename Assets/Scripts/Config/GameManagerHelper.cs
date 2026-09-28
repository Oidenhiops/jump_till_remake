using System;
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
    public void LoadScene(int typeScene)
    {
        GameManager.TypeScene scene = (GameManager.TypeScene)typeScene;
        _ = GameManager.Instance.LoadGameScene(scene.ToString());
    }
    public void LoadScene(string sceneName)
    {
        _ = GameManager.Instance.LoadGameScene(sceneName);
    }
    public void ReloadScene()
    {
        _ = GameManager.Instance.ReloadGameScene();
    }
    public interface IMenuScene
    {
        public void PlayEndAnimation();
        public bool AnimationEnded();
    }
}
