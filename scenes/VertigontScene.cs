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
using skystride.objects.weapons.shotguns;
using skystride.objects.items;

namespace skystride.scenes
{
    internal class VertigontScene : GlobalScene
    {
        private Skybox _sky;

        public VertigontScene()
        {
            _sky = new Skybox("assets/textures/skybox/skybox_vertigont.jpg", 400f);
            _sky.SetPosition(new Vector3(0f, 20f, 0f));

            Plane platform = new Plane(new Vector3(0f, 0f, 0f), 120f, 120f, 0.4f, Color.Cyan, new Vector3(0f, 0f, 0f));
            platform.SetTexture("assets/textures/siege.jpg");
            platform.SetTextureScale(10f, 10f);
            AddEntity(platform);

            Plane wall1 = new Plane(new Vector3(-60f, 5f, 0f), 135f, 1f, 10f, Color.LightGray, new Vector3(0f, 90f, 0f));
            wall1.SetTexture("assets/textures/siege_wall.jpg");
            wall1.SetTextureScale(8f, 3f);
            wall1.SetRotation(0f, 90f, 0f);
            AddEntity(wall1);

            Plane wall2 = new Plane(new Vector3(60f, 5f, 0f), 135f, 1f, 10f, Color.LightGray, new Vector3(0f, 90f, 0f));
            wall2.SetTexture("assets/textures/siege_wall.jpg");
            wall2.SetTextureScale(8f, 3f);
            wall2.SetRotation(0f, 90f, 0f);
            AddEntity(wall2);

            Plane wall3 = new Plane(new Vector3(0f, 5f, 60f), 135f, 1f, 10f, Color.LightGray, new Vector3(0f, 90f, 0f));
            wall3.SetTexture("assets/textures/siege_wall.jpg");
            wall3.SetTextureScale(8f, 3f);
            AddEntity(wall3);
            Plane wall4 = new Plane(new Vector3(0f, 5f, -60f), 135f, 1f, 10f, Color.LightGray, new Vector3(0f, 90f, 0f));
            wall4.SetTexture("assets/textures/siege_wall.jpg");
            wall4.SetTextureScale(8f, 3f);
            AddEntity(wall4);

            AddEntity(new Snow(color: Color.Red, count: 7500, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 1.5f, maxSpeed: 4.5f));
        }

        public override void OnLoad()
        {
            base.OnLoad();
            
            if (_engine != null && _engine.player != null)
            {
                _engine.player.SetPosition(new Vector3(50f, 2f, 30f));
            }
        }

        public override void Update(float dt, Player player, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, player, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse);

            if (player != null)
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
