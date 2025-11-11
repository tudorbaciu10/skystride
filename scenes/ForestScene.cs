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
    internal class ForestScene : GlobalScene
    {
        private Skybox _sky;
        private Rain _rain;

        public ForestScene()
        {
            _sky = new Skybox("assets/textures/skybox/forest.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));

            _rain = new Rain(count: 2000, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 12f, maxSpeed: 24f);
            AddEntity(_rain);

            Plane ground = new Plane(new Vector3(0f, 0f, 0f), 50f, 50f, 1f, Color.ForestGreen, new Vector3(0f, 1f, 0f));
            ground.SetTexture("assets/textures/grass.png");
            ground.SetTextureScale(10, 10);
            AddEntity(ground);

            AddEntity(new ModelEntity(
               new Model("/assets/models/kustik.obj", "/assets/models/kustik.png"),
               new Vector3(12f, 1.8f, 9f), 1f, 0f, 0f, 0f), false);

            AddEntity(new ModelEntity(
               new Model("/assets/models/kustik.obj", "/assets/models/kustik.png"),
               new Vector3(-12f, 1.8f, 4f), 1f, 0f, 0f, 0f), false);

            AddEntity(new ModelEntity(
               new Model("/assets/models/kustik.obj", "/assets/models/kustik.png"),
               new Vector3(-4f, 1.8f, 16f), 1f, 0f, 0f, 0f), false);

            AddEntity(new ModelEntity(
               new Model("/assets/models/kustik.obj", "/assets/models/kustik.png"),
               new Vector3(4f, 1.8f, -16f), 1f, 0f, 0f, 0f), false);

            AddEntity(new ModelEntity(
               new Model("/assets/models/kustik.obj", "/assets/models/kustik.png"),
               new Vector3(-14f, 1.8f, -19f), 1f, 0f, 0f, 0f), false);

            AddEntity(new ModelEntity(
               new Model("/assets/models/bochka.obj", "/assets/models/bochka.png"),
               new Vector3(33f, -3f, 0f), 6f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
               new Model("/assets/models/bochka.obj", "/assets/models/bochka.png"),
               new Vector3(43f, -3f, 6f), 6f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
               new Model("/assets/models/bochka.obj", "/assets/models/bochka.png"),
               new Vector3(53f, -3f, -2f), 6f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
               new Model("/assets/models/bochka.obj", "/assets/models/bochka.png"),
               new Vector3(67f, -3f, 0f), 6f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
               new Model("/assets/models/peretesus.obj", "/assets/models/peretesus.png"),
               new Vector3(0,7, -30f),2.5f,0f,0f,0f,10f,10f));

            AddEntity(new ModelEntity(
               new Model("/assets/models/peretesus.obj", "/assets/models/peretesus.png"),
               new Vector3(0, 7, 30f), 2.5f, 0f, 180f, 0f, 10f, 10f));

            AddEntity(new ModelEntity(
               new Model("/assets/models/peretesus.obj", "/assets/models/peretesus.png"),
               new Vector3(-29, 7, 00f), 2.5f, 0f, 90f, 0f, 10f, 10f));
        }

        public override void Update(float dt, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            camera.ResolveCollisions(Colliders);
        }

        public override void Render()
        {
            if (_sky != null) _sky.Render();
            base.Render();
        }
    }
}
