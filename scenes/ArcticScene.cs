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
            Plane platform = new Plane(new Vector3(0f, 0f, 0f), 200f, 200f, 2f, Color.IndianRed, new Vector3(0f, 1f, 0f));
            platform.SetTexture("assets/textures/snow.jpg");
            platform.SetTextureScale(5, 5);
            AddEntity(platform);

            _sky = new Skybox("assets/textures/skybox_arctic.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));
            //object brad
            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/tree.obj", "/assets/models/tree.png"),
            //    new Vector3(12f, 6f, 22f), 1f, 0f, 0f, 0f));

            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/tree.obj", "/assets/models/tree.png"),
            //    new Vector3(-13f, 6f, 14f), 1f, 0f, 0f, 0f));

            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/tree.obj", "/assets/models/tree.png"),
            //    new Vector3(-4f, 6f, -22f), 1f, 0f, 0f, 0f));

            //object cutii
            AddEntity(new ModelEntity(
            new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
                new Vector3(-20f, 2f, 31f), 2f, 0f, 180f, -360f));

            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
            //    new Vector3(-37f, -2f, 4f), 5f, 0f, 90f, -360f));

            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
            //    new Vector3(-47f, -2f, 0f), 5f, 0f, 90f, -360f));

            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
            //    new Vector3(-57f, -2f, 4f), 5f, 0f, 90f, -360f));

            //AddEntity(new ModelEntity(
            //    new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
            //    new Vector3(-67f, -2f, 2f), 5f, 0f, 90f, -360f));

            // =====================================================
            // 🔵 1. Blocuri mari de piatră (ca în IceWorld)
            // =====================================================

            AddEntity(new ModelEntity(
                new Model("assets/models/barrel.obj", "assets/textures/barrel.jpg"),
                new Vector3(-10f, 5f, -10f), 12f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/box.obj", "assets/models/box.jpg"),
                new Vector3(16f, 2f, -10f),   // pus la +6 pe axa X
                3f, 0f, 90f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/barrel.obj", "assets/textures/barrel.jpg"),
                new Vector3(10f, 5f, -10f), 12f, 0f, 45f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/box.obj", "assets/models/box.jpg"),
                new Vector3(-16f, 2f, -10f),    // pus la -6 pe axa X
                3f, 0f, 90f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/barrel.obj", "assets/textures/barrel.jpg"),
                new Vector3(10f, 5f, 10f), 12f, 0f, -45f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/box.obj", "assets/models/box.jpg"),
                new Vector3(16f, 2f, 10f),     // -6 pe X
                3f, 0f, 90f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/barrel.obj", "assets/textures/barrel.jpg"),
                new Vector3(-10f, 5f, 10f), 12f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/box.obj", "assets/models/box.jpg"),
                new Vector3(-16f, 2f, 10f),     // +6 pe X
                3f, 0f, 90f, 0f));




            // =====================================================
            // 🔵 2. Pereți exteriori (perimetru IceWorld)
            // =====================================================

            AddEntity(new ModelEntity(
                new Model("assets/models/wall.obj", "assets/textures/barrel.jpg"),
                new Vector3(0f, 4f, -48f),
                1f,
                0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/wall.obj", "assets/textures/barrel.jpg"),
                new Vector3(0f, 4f, 48f),
                1f,
                0f, 180f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/wall_long.obj", "assets/textures/barrel.jpg"),
                new Vector3(48f, 4f, 0f),
                1f,
                0f, 90f, 0f));

            AddEntity(new ModelEntity(
                new Model("assets/models/wall_long.obj", "assets/textures/barrel.jpg"),
                new Vector3(-48f, 4f, 0f),
                1f,
                0f, -90f, 0f));
            // =====================================================
            // ❄️ Zăpadă
            // =====================================================

            AddEntity(new Snow(count: 7500, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 1.5f, maxSpeed: 4.5f));
        }

        public override void Update(float dt, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse);
            if (camera.position.Y < -8f)
            {
                camera.SetPosition(new Vector3(0f, 10f, 0f));
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