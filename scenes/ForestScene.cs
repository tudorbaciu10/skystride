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
using skystride.objects.weapons.pistols;

namespace skystride.scenes
{
    internal class ForestScene : GlobalScene
    {
        private Skybox _sky;
        private Glock _glock;

        private float _movingBoxDirection =1f; //1 = right, -1 = left
        private ModelEntity movingBox = new ModelEntity("/assets/models/box.obj", "/assets/models/box.jpg", new Vector3(-90f, -3f,96f),7f,0f,0f,0f,1f,1f);

        public ForestScene()
        {
            _sky = new Skybox("assets/textures/skybox/skybox_forest.jpg", 400f);
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

            Plane wallSpawn2 = new Plane(new Vector3(-17f, 5.2f, 27.5f), 100f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn2.SetTexture("assets/textures/bricks.jpg");
            wallSpawn2.SetTextureScale(25f, 2f);
            wallSpawn2.SetRotation(0f, 90f, 0f);
            AddEntity(wallSpawn2);

            Plane wallSpawn3 = new Plane(new Vector3(0f, 5.2f, -17f), 35f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn3.SetTexture("assets/textures/bricks.jpg");
            wallSpawn3.SetTextureScale(5f, 2f);
            AddEntity(wallSpawn3);

            AddEntity(new ModelEntity("/assets/models/tree.obj", "/assets/textures/grass.jpg", new Vector3(24f, 17f, 5f), 1f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/tree.obj", "/assets/textures/grass.jpg", new Vector3(-24f, 17f, -5f), 1f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/tree.obj", "/assets/textures/grass.jpg", new Vector3(-24f, 17f, 45f), 1f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/tree.obj", "/assets/textures/grass.jpg", new Vector3(24f, 17f, 85f), 1f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/tree.obj", "/assets/textures/grass.jpg", new Vector3(-24f, 17f, 115f), 1f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/tree.obj", "/assets/textures/grass.jpg", new Vector3(-5f, 17f, -24f), 1f, 0f, 0f, 0f, 1f, 1f));

            AddEntity(new ModelEntity("/assets/models/bush.obj", "/assets/models/bush.jpg", new Vector3(-10f, 2.9f, 16f), 2f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/bush.obj", "/assets/models/bush.jpg", new Vector3(10f, 2.9f, 16f), 2f, 0f, 0f, 0f, 1f, 1f));

            AddEntity(new ModelEntity("/assets/models/box.obj", "/assets/models/box.jpg", new Vector3(0f, -2f, 24f), 5f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/box.obj", "/assets/models/box.jpg", new Vector3(5f, -2f, 34f), 5f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/box.obj", "/assets/models/box.jpg", new Vector3(-5f, -2f, 44f), 5f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/box.obj", "/assets/models/box.jpg", new Vector3(0f, -2f, 54f), 5f, 0f, 0f, 0f, 1f, 1f));

            Plane platform2 = new Plane(new Vector3(0f, 0f, 70f), 35f, 15f, 0.4f, Color.Cyan, new Vector3(0f, 0f, 0f));
            platform2.SetTexture("assets/textures/grass.jpg");
            platform2.SetTextureScale(5f, 2f);
            AddEntity(platform2);

            AddEntity(new ModelEntity("/assets/models/bush.obj", "/assets/textures/rock.jpg", new Vector3(11f, 3f, 70f), 2f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/bush.obj", "/assets/textures/rock.jpg", new Vector3(-11f, 3f, 70f), 2f, 0f, 0f, 0f, 1f, 1f));

            AddEntity(new ModelEntity("/assets/models/barrel.obj", "/assets/models/barrel.jpg", new Vector3(0f, -5f, 87f), 10f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/barrel.obj", "/assets/models/barrel.jpg", new Vector3(-5f, -4f, 105f), 10f, 0f, 0f, 0f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/barrel.obj", "/assets/models/barrel.jpg", new Vector3(-20f, -4f, 95f), 10f, 0f, 0f, 0f, 1f, 1f));

            Plane wallSpawn4 = new Plane(new Vector3(-50f, 5.2f, 115f), 135f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn4.SetTexture("assets/textures/bricks.jpg");
            wallSpawn4.SetTextureScale(25f, 2f);
            AddEntity(wallSpawn4);

            Plane wallSpawn5 = new Plane(new Vector3(-85f, 5.2f, 77f), 135f, 1f, 10f, Color.Brown, new Vector3(0f, 0f, 0f));
            wallSpawn5.SetTexture("assets/textures/bricks.jpg");
            wallSpawn5.SetTextureScale(25f, 2f);
            AddEntity(wallSpawn5);

            Plane platform3 = new Plane(new Vector3(-38f, 0f, 95f), 40f, 15f, 0.4f, Color.Cyan, new Vector3(0f, 0f, 0f));
            platform3.SetTexture("assets/textures/grass.jpg");
            platform3.SetTextureScale(5f, 2f);
            platform3.SetRotation(0f, 90f, 0f);
            AddEntity(platform3);

            AddEntity(movingBox);

            Plane platform4 = new Plane(new Vector3(-110f, 0f, 95f), 40f, 15f, 0.4f, Color.Cyan, new Vector3(0f, 0f, 0f));
            platform4.SetTexture("assets/textures/grass.jpg");
            platform4.SetTextureScale(5f, 2f);
            platform4.SetRotation(0f, 90f, 0f);
            AddEntity(platform4);

            AddEntity(new Rain(count: 2000, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 12f, maxSpeed: 24f));

            _glock = new Glock();
        }

        public override void Update(float dt, Player player, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, player, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse); // ensures lazy evaluation & camera pos update

            // player can be null in editor contexts
            if (player != null)
            {
                if (player.position.Y < -8f)
                {
                    player.SetPosition(new Vector3(0f, 10f, 0f));
                }
            }

            // moving box logic
            if (movingBox != null)
            {
                float boxSpeed = 5f;
                Vector3 boxPos = movingBox.GetPosition();
                boxPos.X += _movingBoxDirection * boxSpeed * dt;
                if (boxPos.X > -55f)
                {
                    boxPos.X = -55f;
                    _movingBoxDirection = -1f;
                }
                else if (boxPos.X < -90f)
                {
                    boxPos.X = -90f;
                    _movingBoxDirection = 1f;
                }
                movingBox.SetPosition(boxPos);
            }

            if (player != null)
            {
                player.ResolveCollisions(Colliders);
            }

            // attach weapon once
            if (player != null && _glock != null && !player.HasAttachedWeapon())
            {
                player.AttachWeapon(_glock);
            }
        }

        public override void Render()
        {
            if (_sky != null) _sky.Render();
            base.Render();
        }
    }
}
