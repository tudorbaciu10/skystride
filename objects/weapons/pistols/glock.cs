using skystride.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace skystride.objects.weapons.pistols
{
    internal class Glock : Weapon
    {
        public Glock() : base("Glock", 17, 25)
        {
            this.ModelPath = "assets/models/weapons/glock.obj";
            this.TexturePath = "assets/models/weapons/glock.jpg";
            this.model = new Model(this.ModelPath, this.TexturePath);
            this.model.SetTextureScale(1f,1f);
            
            this.viewOffset = new Vector3(0.9f, -0.7f, -1.8f);
            this.scale = 0.007f;
            this.rotation = new Vector3(0f, 170f, 0f);
            this.recoilForce = 20f;
        }
    }
}
