using skystride.objects;
using skystride.objects.templates;
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

        public void Render()
        {
            grid.Render();
            terrain.Render();
        }
    }
}
