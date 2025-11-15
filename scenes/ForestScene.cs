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

        public ForestScene()
        {
            _sky = new Skybox("assets/textures/skybox/forest.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));

            Plane platform = new Plane(new Vector3(0f, 0f, 0f), 35f, 35f, 0.4f, Color.Cyan, new Vector3(0f, 0f, 0f));
            platform.SetTexture("assets/textures/grass.jpg");
            platform.SetTextureScale(20f, 20f);
            AddEntity(platform);

            Plane wallSpawn = new Plane(new Vector3(17f, 5.2f, 50f), 135f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn.SetTexture("assets/textures/bricks.jpg");
            wallSpawn.SetTextureScale(25f, 2f);
            wallSpawn.SetRotation(0f, 90f, 0f);
            AddEntity(wallSpawn);

            Plane wallSpawn2 = new Plane(new Vector3(-17f, 5.2f, 50f), 135f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn2.SetTexture("assets/textures/bricks.jpg");
            wallSpawn2.SetTextureScale(25f, 2f);
            wallSpawn2.SetRotation(0f, 90f, 0f);
            AddEntity(wallSpawn2);

            Plane wallSpawn3 = new Plane(new Vector3(0f, 5.2f, -17f), 35f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn3.SetTexture("assets/textures/bricks.jpg");
            wallSpawn3.SetTextureScale(5f, 2f);
            AddEntity(wallSpawn3);

            AddEntity(new ModelEntity(
                new Model("/assets/models/tree.obj", "/assets/textures/grass.jpg"),
                new Vector3(24f, 17f, 5f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/tree.obj", "/assets/textures/grass.jpg"),
                new Vector3(-24f, 17f, -5f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/tree.obj", "/assets/textures/grass.jpg"),
                new Vector3(-24f, 17f, 45f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/tree.obj", "/assets/textures/grass.jpg"),
                new Vector3(24f, 17f, 85f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/tree.obj", "/assets/textures/grass.jpg"),
                new Vector3(-24f, 17f, 115f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/tree.obj", "/assets/textures/grass.jpg"),
                new Vector3(-5f, 17f, -24f), 1f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/bush.obj", "/assets/models/bush.jpg"),
                new Vector3(-10f, 2.9f, 16f), 2f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/bush.obj", "/assets/models/bush.jpg"),
                new Vector3(10f, 2.9f, 16f), 2f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
                new Vector3(0f, -2f, 24f), 5f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
                new Vector3(5f, -2f, 34f), 5f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
                new Vector3(-5f, -2f, 44f), 5f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/box.obj", "/assets/models/box.jpg"),
                new Vector3(0f, -2f, 54f), 5f, 0f, 0f, 0f));

            Plane platform2 = new Plane(new Vector3(0f, 0f, 70f), 35f, 15f, 0.4f, Color.Cyan, new Vector3(0f, 0f, 0f));
            platform2.SetTexture("assets/textures/grass.jpg");
            platform2.SetTextureScale(5f, 2f);
            AddEntity(platform2);

            AddEntity(new ModelEntity(
                new Model("/assets/models/bush.obj", "/assets/textures/rock.jpg"),
                new Vector3(11f, 3f, 70f), 2f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/bush.obj", "/assets/textures/rock.jpg"),
                new Vector3(-11f, 3f, 70f), 2f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/barrel.obj", "/assets/models/barrel.jpg"),
                new Vector3(0f, -5f, 87f), 10f, 0f, 0f, 0f));

            AddEntity(new ModelEntity(
                new Model("/assets/models/barrel.obj", "/assets/models/barrel.jpg"),
                new Vector3(5f, -4f, 105f), 10f, 0f, 0f, 0f));

            AddEntity(new Rain(count: 2000, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 12f, maxSpeed: 24f));
        }

        public override void Update(float dt, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            if(camera.position.Y < -8f)
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
