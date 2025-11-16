using OpenTK;
using OpenTK.Input;
using skystride.objects;
using skystride.objects.templates;
using skystride.shaders;
using skystride.vendor;
using skystride.vendor.collision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skystride.scenes
{
    internal class ArcticScene : GlobalScene
    {
        private Skybox _sky;

        public ArcticScene()
        {
            Plane platform = new Plane(new Vector3(0f, 0f, 0f), 40f, 70f, 1f, Color.IndianRed, new Vector3(0f, 1f, 0f));
            platform.SetTexture("assets/textures/snow.png");
            platform.SetTextureScale(5, 5);
            AddEntity(platform);

            _sky = new Skybox("assets/textures/skybox_arctic.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/copac.obj", "/assets/models/copac.png"),
                new Vector3(12f, 6f, 22f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/copac.obj", "/assets/models/copac.png"),
                new Vector3(-13f, 6f, 14f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/copac.obj", "/assets/models/copac.png"),
                new Vector3(-4f, 6f, -22f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-27f, -2f, 0f), 5f, 0f, 90f, -360f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-37f, -2f, 4f), 5f, 0f, 90f, -360f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-47f, -2f, 0f), 5f, 0f, 90f, -360f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-57f, -2f, 4f), 5f, 0f, 90f, -360f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-67f, -2f, 2f), 5f, 0f, 90f, -360f));

            AddEntity(new Snow(count: 7500, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 1.5f, maxSpeed: 4.5f));
        }

        public override void Update(float dt, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse);
            if (currentKeyboard[Key.R])
            {
                camera.SetPosition(new Vector3(0f, 5f, 10f));
            }

            camera.ResolveCollisions(Colliders);
        }

        public override void Render()
        {
            if (_sky != null) _sky.Render();
            base.Render();
        }
    }
}
