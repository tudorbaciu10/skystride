using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;
using skystride.objects.templates;

namespace skystride.scenes
{
    internal class ArcticScene : GlobalScene
    {
        public ArcticScene()
        {
            var platformSpawn = new Plane(new Vector3(0f, 0f, 0f), 20f, 20f, 1f, Color.AntiqueWhite, new Vector3(0f, 1f, 0f));
            AddEntity(platformSpawn);
            AddEntity(new Grid());
        }
    }
}
