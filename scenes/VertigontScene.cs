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
using skystride.objects.weapons.snipers;
using skystride.objects.items;

namespace skystride.scenes
{
    internal class VertigontScene : GlobalScene
    {
        private Skybox _sky;
        private NPC boss = new NPC(new Vector3(30f, 0f, 20f), "Vertigont", 2000f, 3.0f, NPC.NPCType.Aggressive, damage:50);

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

            AddEntity(boss);

            AddEntity(new WeaponItem(new Sniper(), new Vector3(-5f, 1f, 5f)));

            AddEntity(new Snow(color: Color.Red, count: 7500, areaSize: 120f, spawnHeight: 50f, groundY: -10f, minSpeed: 1.5f, maxSpeed: 4.5f));
        }

        public override void OnLoad()
        {
            base.OnLoad();
            SoundManager.PlayMusic("../../assets/sounds/vertigont.wav");

            _engine.player.SetPosition(new Vector3(0f, 5f, -20f));

            _engine.player.AttachWeapon(new Sniper());
        }

        private float _spawnTimer = 0f;
        private float _spawnCooldown = 3.0f;
        private Random _random = new Random();

        public override void Update(float dt, Player player, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            base.Update(dt, player, camera, currentKeyboard, previousKeyboard, currentMouse, previousMouse);

            if (player != null)
            {
                player.ResolveCollisions(Colliders);
            }

            _spawnTimer += dt;
            if (_spawnTimer >= _spawnCooldown)
            {
                _spawnTimer = 0f;
                SpawnRandomEnemy();
            }
        }

        private void SpawnRandomEnemy()
        {
            float x = (float)(_random.NextDouble() * 100.0 - 50.0);
            float z = (float)(_random.NextDouble() * 100.0 - 50.0);
            
            Vector3 spawnPos = new Vector3(x, 5f, z);
            
            float size = 0.8f + (float)_random.NextDouble() * 0.4f;

            NPC enemy = new NPC(spawnPos, "Enemy", 100f, size, NPC.NPCType.Aggressive);
            
            enemy.OnDeath = (pos) =>
            {
                const double noDropChance = 0.60;
                const double shotgunChance = 0.30;
                const double glockChance = 0.30;
                const double medkitChance = 0.25; 
                const double sniperChance = 0.15;

                double roll = _random.NextDouble();
                if (roll < noDropChance)
                {
                    return;
                }

                double scaled = (roll - noDropChance) / (1.0 - noDropChance);

                if (scaled < shotgunChance)
                {
                    AddEntity(new WeaponItem(new Shotgun(), pos));
                }
                else if (scaled < shotgunChance + glockChance)
                {
                    AddEntity(new WeaponItem(new skystride.objects.weapons.pistols.Glock(), pos));
                }
                else if (scaled < shotgunChance + glockChance + medkitChance)
                {
                    AddEntity(new MedkitItem(pos, 0.01f, 30));
                }
                else if (scaled < shotgunChance + glockChance + medkitChance + sniperChance)
                {
                    AddEntity(new WeaponItem(new Sniper(), pos));
                }
            };

            AddEntity(enemy);
        }

        public override void Render()
        {
            if (_sky != null) _sky.Render();
            base.Render();
        }
    }
}
