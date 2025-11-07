using skystride.objects;
using skystride.objects.templates;
using skystride.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skystride.scenes
{
    internal class TemplateScene
    {
        Grid grid = new Grid();
        CheckboardTerrain terrain = new CheckboardTerrain();
        Cube cube = new Cube();

        private static readonly Model frog = new Model("/assets/models/frog.obj", "/assets/textures/frog.jpg");

        public void Render()
        {
            grid.Render();
            terrain.Render();
            cube.Render();
            frog.Render(new OpenTK.Vector3(5f, 0.7f, 0), 0.4f, -90f, 0f, -150f);
        }
    }
}
