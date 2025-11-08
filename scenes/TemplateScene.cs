using skystride.objects;
using skystride.objects.templates;
using skystride.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace skystride.scenes
{
    internal class TemplateScene : GlobalScene
    {
        public TemplateScene()
        {
            // primitives can be added directly
            AddEntity(new Grid());
            AddEntity(new CheckboardTerrain());
            AddEntity(new Cube());

            // models use ModelEntity wrapper to carry transform
            AddEntity(new ModelEntity(
                new Model("/assets/models/frog.obj", "/assets/models/frog.jpg"),
                new Vector3(5f, 0.7f, 0f), 0.4f, -90f, 0f, -150f));
            AddEntity(new ModelEntity(
                new Model("/assets/models/iashik.obj", "/assets/models/iashik.jpg"),
                new Vector3(-5f, 0.7f, 0f), 3f, -90f, 0f, -150f));
        }
    }
}
