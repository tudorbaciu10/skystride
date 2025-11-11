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
        private Cube _cube; // Keep reference for logic

        public TemplateScene()
        {
            // primitives can be added directly
            AddEntity(new Grid());
            AddEntity(new CheckboardTerrain());
            _cube = new Cube();
            AddEntity(_cube);

            // models use ModelEntity wrapper to carry transform
            AddEntity(new ModelEntity(
                new Model("/assets/models/frog.obj", "/assets/models/frog.jpg"),
                new Vector3(5f, 0.7f, 0f), 0.4f, -90f, 0f, -150f));
            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-5f, 0.7f, 0f), 3f, -90f, 0f, -150f));
        }

        public void testAABB(Camera _camera, Cube _cube)
        {
            AABB cameraAABB = new AABB(_camera.position, new Vector3(1f, 2f, 1f));
            AABB cubeAABB = new AABB(_cube.GetPosition(), new Vector3(_cube.GetSize(), _cube.GetSize(), _cube.GetSize()));

            bool isColliding = cameraAABB.Intersects(cubeAABB);
            Console.WriteLine("Collision Detected: " + isColliding);
        }

        public override void Update(float dt, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            if (_cube != null && camera != null)
            {
                testAABB(camera, _cube);
            }
        }
    }
}
