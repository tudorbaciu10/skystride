using OpenTK;
using skystride.objects;
using skystride.objects.templates;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skystride.shaders; // added for Snow

namespace skystride.scenes
{
    internal class ArcticScene : GlobalScene
    {
        public ArcticScene()
        {
            AddEntity(new Grid());

            Plane spawnPlane = new Plane(new Vector3(0f, 0f, 0f), 20f, 10f, 1f, Color.IndianRed, new Vector3(0f, 1f, 0f));
            spawnPlane.SetTexture("assets/textures/snow.png");
            AddEntity(spawnPlane);

            Plane spawnPlane2 = new Plane(new Vector3(0f, 0f, 20f), 20f, 10f, 1f, Color.IndianRed, new Vector3(0f, 1f, 0f));
            spawnPlane2.SetTexture("assets/textures/snow.png");
            AddEntity(spawnPlane2);

            // Add snow particle system (tuned for arctic environment)
            AddEntity(new Snow(count: 7500, areaSize: 120f, spawnHeight: 50f, groundY: 0f, minSpeed: 1.5f, maxSpeed: 4.5f));
        }
    }
}
