namespace skystride.scenes
{
    public interface ISceneEntity
    {
        void Render();
        OpenTK.Vector3 GetPosition();
        void SetPosition(OpenTK.Vector3 pos);
    }
}
