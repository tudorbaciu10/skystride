using OpenTK;
using OpenTK.Input;
using skystride.objects;
using skystride.objects.items;
using skystride.objects.templates;
using skystride.objects.weapons.shotguns;
using skystride.vendor;
using skystride.vendor.collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            AddEntity(new ModelEntity(
                new Model("/assets/models/frog.obj", "/assets/models/frog.jpg"),
                new Vector3(5f, 0.7f, 0f), 0.4f, -90f, 0f, -150f));

            // Add test NPC (white sphere with wandering AI)
            AddEntity(new NPC(new Vector3(10f, 1f, 0f), "Angry Sphere", 100f, NPC.NPCType.Aggressive, 0.5f, 10));
            AddEntity(new NPC(new Vector3(-10f, 1f, 0f), "Wandering Sphere", 100f, NPC.NPCType.Passive, 0.5f, 10));

            Shotgun shotgunItem = new Shotgun();
            AddEntity(new WeaponItem(shotgunItem, new Vector3(-5f, 1f, 5f), shotgunItem.Scale));
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
