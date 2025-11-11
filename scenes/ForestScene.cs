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

            //AddEntity(new Snow(count: 7500, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 1.5f, maxSpeed: 4.5f));
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
