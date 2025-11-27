using skystride.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace skystride.objects.weapons.shotguns
{
    internal class Shotgun : Weapon
    {
        public Shotgun() : base("Shotgun", 18, 80)
        {
            this.ModelPath = "assets/models/weapons/shotgun.obj";
            this.TexturePath = "assets/models/weapons/shotgun.jpg";
            this.model = new Model(this.ModelPath, this.TexturePath);
            this.model.SetTextureScale(1f, 1f);

            this.viewOffset = new Vector3(0.9f, -0.7f, -1.8f); 
            this.scale = 0.25f;
            this.rotation = new Vector3(0f, -90f, 0f);
            this.recoilForce = 20f;
        }
    }
}
