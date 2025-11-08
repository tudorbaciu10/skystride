using System;
using System.Collections.Generic;
using OpenTK;
using skystride.vendor;

namespace skystride.scenes
{
    internal class GlobalScene : IDisposable
    {
        // Unified list for all entities
        protected readonly List<ISceneEntity> Entities = new List<ISceneEntity>();

        // Model entity with transform; other primitives can directly implement ISceneEntity themselves
        protected sealed class ModelEntity : ISceneEntity, IDisposable
        {
            private readonly Model _model;
            private readonly Vector3 _position;
            private readonly float _scale;
            private readonly float _rx, _ry, _rz;
            public ModelEntity(Model model, Vector3 position, float scale, float rx, float ry, float rz)
            {
                _model = model; _position = position; _scale = scale; _rx = rx; _ry = ry; _rz = rz;
            }
            public void Render()
            {
                if (_model != null && _model.Loaded)
                    _model.Render(_position, _scale, _rx, _ry, _rz);
            }
            public void Dispose()
            {
                try { _model?.Dispose(); } catch { }
            }
        }

        // Single generic add method
        protected void AddEntity(ISceneEntity entity) => Entities.Add(entity);

        public virtual void Render()
        {
            for (int i =0; i < Entities.Count; i++)
            {
                Entities[i].Render();
            }
        }

        public virtual void Dispose()
        {
            for (int i =0; i < Entities.Count; i++)
            {
                var disp = Entities[i] as IDisposable;
                if (disp != null)
                {
                    try { disp.Dispose(); } catch { }
                }
            }
        }
    }
}
