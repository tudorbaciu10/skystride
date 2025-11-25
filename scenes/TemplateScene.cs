using skystride.objects;
using skystride.objects.templates;
using skystride.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using skystride.vendor.collision;

namespace skystride.scenes
{
    internal class TemplateScene : GlobalScene
    {
        private Cube _cube;
        private Skybox _sky;

        public TemplateScene()
        {
            // primitives can be added directly
            AddEntity(new Grid());
            AddEntity(new CheckboardTerrain());
            _cube = new Cube();
            AddEntity(_cube);

            _sky = new Skybox("assets/textures/skybox_forest.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));

            // Immediate near models (legacy constructor)
            AddEntity(new ModelEntity(
                new Model("/assets/models/frog.obj", "/assets/models/frog.jpg"),
                new Vector3(5f, 0.7f, 0f), 0.4f, -90f, 0f, -150f));
            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-5f, 0.7f, 0f), 3f, -90f, 0f, -150f));

            // Distant models use lazy path-based constructor (auto load/unload)
            AddEntity(new ModelEntity("/assets/models/iashik.obj", "/assets/models/iashik.jpg", new Vector3(0f, 0.7f, 140f), 3f, -90f, 0f, -150f, 1f, 1f));
            AddEntity(new ModelEntity("/assets/models/iashik.obj", "/assets/models/iashik.jpg", new Vector3(15f, 0.7f, 155f), 3f, -90f, 0f, -150f, 1f, 1f));
        }

        public void testAABB(Player _player, Cube _cube)
        {
            AABB cubeAABB = new AABB(_cube.GetPosition(), _cube.GetSize());

            bool isColliding = _player.Hitbox().Intersects(cubeAABB);
            Console.WriteLine("Collision Detected: " + isColliding);
        }

        public override void Update(float dt, Player player, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, player, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse);
            if (_cube != null && player != null)
            {
                player.ResolveCollisions(Colliders);
            }
        }

        public override void Render()
        {
            if (_sky != null) _sky.Render();
            base.Render();
        }
    }
}
