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
        private Skybox _sky;

        public TemplateScene()
        {
            AddEntity(new Grid());
            AddEntity(new CheckboardTerrain());

            _sky = new Skybox("assets/textures/skybox/skybox_forest.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));

            // Immediate near models (legacy constructor)
            AddEntity(new ModelEntity(
                new Model("/assets/models/frog.obj", "/assets/models/frog.jpg"),
                new Vector3(5f, 0.7f, 0f), 0.4f, -90f, 0f, -150f));
        }

        public override void Update(float dt, Player player, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, player, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse);

            player.ResolveCollisions(Colliders);
        }

        public override void Render()
        {
            if (_sky != null) _sky.Render();
            base.Render();
        }
    }
}
