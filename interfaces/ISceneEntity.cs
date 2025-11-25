namespace skystride.scenes
{
    public interface ISceneEntity
    {
        void Render();
        OpenTK.Vector3 GetPosition();
        void SetPosition(OpenTK.Vector3 pos);
        OpenTK.Vector3 GetSize();
        void SetSize(OpenTK.Vector3 size);
    }
}
