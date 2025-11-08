using OpenTK;
using skystride.objects;
using skystride.objects.templates;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skystride.scenes
{
    internal class ArcticScene : GlobalScene
    {
        public ArcticScene()
        {
            AddEntity(new Grid());

            Plane spawnPlane = new Plane(new Vector3(0f, 0f, 0f), 20f, 10f, 1f, Color.IndianRed, new Vector3(0f, 1f, 0f));
            spawnPlane.SetTexture("assets/textures/survivor.png");
            AddEntity(spawnPlane);

            Plane spawnPlane2 = new Plane(new Vector3(0f, 0f, 20f), 20f, 10f, 1f, Color.IndianRed, new Vector3(0f, 1f, 0f));
            spawnPlane2.SetTexture("assets/textures/survivor.png");
            AddEntity(spawnPlane2);
        }
    }
}
