using OpenTK;
using skystride.scenes;
using skystride.vendor.collision;
using System;
using System.Collections.Generic;

namespace skystride.vendor
{
    internal class ModelEntity : ISceneEntity, IDisposable
    {
        private string _objectPath;
        private string _texturePath;
        private Vector3 _position;
        private float _scale;
        private float _rx, _ry, _rz;
        private float _texScaleU, _texScaleV;
        private float _loadDistance;
        private float _unloadDistance;
        private float _loadDistanceSq;
        private float _unloadDistanceSq;
        private Model _model;
        private bool _isLoaded;
        private AABB _dynamicCollider;
        private List<AABB> _collidersRef;

        public ModelEntity(string objectPath, string texturePath, Vector3 position, float scale,
        float rx, float ry, float rz, float texScaleU = 1f, float texScaleV = 1f,
        float? loadDistance = null, float? unloadDistance = null, List<AABB> collidersRef = null)
        {
            _objectPath = objectPath;
            _texturePath = texturePath;
            _position = position;
            _scale = scale;
            _rx = rx; _ry = ry; _rz = rz;
            _texScaleU = texScaleU; _texScaleV = texScaleV;
            _collidersRef = collidersRef;

            float ld = loadDistance ?? (GlobalScene.DrawDistance * 0.9f);
            float ud = unloadDistance ?? (GlobalScene.DrawDistance * 1.15f);
            if (ud <= ld) ud = ld + 25f;
            _loadDistance = ld;
            _unloadDistance = ud;
            _loadDistanceSq = _loadDistance * _loadDistance;
            _unloadDistanceSq = _unloadDistance * _unloadDistance;
        }

        public ModelEntity(Model model, Vector3 position, float scale, float rx, float ry, float rz, float texScaleU = 1f, float texScaleV = 1f)
        {
            _model = model;
            _objectPath = null;
            _texturePath = null;
            _position = position;
            _scale = scale;
            _rx = rx; _ry = ry; _rz = rz;
            _texScaleU = texScaleU; _texScaleV = texScaleV;
            _isLoaded = model != null && model.Loaded;
            _loadDistance = GlobalScene.DrawDistance * 0.9f;
            _unloadDistance = GlobalScene.DrawDistance * 1.15f;
            _loadDistanceSq = _loadDistance * _loadDistance;
            _unloadDistanceSq = _unloadDistance * _unloadDistance;
        }

        internal void AttachCollidersRef(List<AABB> collidersRef)
        {
            if (collidersRef == null) return;
            if (_collidersRef != null) return;
            _collidersRef = collidersRef;

            if (_isLoaded && _dynamicCollider == null)
            {
                var size = GetSize();
                if (size != Vector3.Zero)
                {
                    _dynamicCollider = new AABB(_position, size, this);
                    _collidersRef.Add(_dynamicCollider);
                }
            }
        }

        public void Evaluate(Vector3 cameraPos)
        {
            float distSq = Vector3.DistanceSquared(cameraPos, _position);

            if (!_isLoaded && distSq <= _loadDistanceSq)
            {
                if (_objectPath != null)
                {
                    // If model is null, start loading
                    if (_model == null)
                    {
                        try
                        {
                            _model = new Model(_objectPath, _texturePath);
                            _model.SetTextureScale(_texScaleU, _texScaleV);
                        }
                        catch { _isLoaded = false; }
                    }
                    
                    // Check if loading finished
                    if (_model != null && _model.Loaded)
                    {
                        _isLoaded = true;
                        if (_collidersRef != null)
                        {
                            var size = GetSize();
                            if (size != Vector3.Zero)
                            {
                                _dynamicCollider = new AABB(_position, size, this);
                                _collidersRef.Add(_dynamicCollider);
                            }
                        }
                    }
                }
            }
            else if (distSq > _unloadDistanceSq)
            {
                // Only unload if we have a model (loaded or loading)
                if (_model != null)
                {
                    try { _model.Dispose(); } catch { }
                    _model = null;
                    _isLoaded = false;
                    if (_dynamicCollider != null && _collidersRef != null)
                    {
                        _collidersRef.Remove(_dynamicCollider);
                        _dynamicCollider = null;
                    }
                }
            }
        }

        public virtual void Render()
        {
            if (Vector3.DistanceSquared(_position, GlobalScene.CurrentCameraPos) > GlobalScene.DrawDistanceSquared)
                return;
            if (_model != null)
                _model.Render(_position, _scale, _rx, _ry, _rz);
        }

        public Vector3 GetPosition() { return _position; }
        public Vector3 GetSize() { return _model != null ? _model.BoundsSize * _scale : Vector3.Zero; }
        public void SetSize(Vector3 size)
        {
            if (_model != null && _model.BoundsSize.X > 0)
            {
                _scale = size.X / _model.BoundsSize.X;
            }
        }
        public void SetTextureScale(float u, float v) { _model?.SetTextureScale(u, v); }
        public void SetPosition(Vector3 newPosition)
        {
            _position = newPosition;
            if (_dynamicCollider != null && _collidersRef != null)
            {
                _collidersRef.Remove(_dynamicCollider);
                _dynamicCollider = new AABB(_position, GetSize(), this);
                _collidersRef.Add(_dynamicCollider);
            }
        }

        public void SetRotation(float rx, float ry, float rz)
        {
            _rx = rx;
            _ry = ry;
            _rz = rz;
        }

        public Vector3 GetRotation()
        {
            return new Vector3(_rx, _ry, _rz);
        }

        public void Dispose()
        {
            try { _model?.Dispose(); } catch { }
            if (_dynamicCollider != null && _collidersRef != null)
            {
                _collidersRef.Remove(_dynamicCollider);
                _dynamicCollider = null;
            }
        }

        public string ModelPath { get { return _objectPath; } }
    }
}
