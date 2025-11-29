using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skystride.objects.items
{
    internal class MedkitItem : Item
    {
        private int _healAmount;

        public MedkitItem(Vector3 position, float scale, int healAmount = 30)
            : base("assets/models/medkit.obj", "assets/models/medkit.png", position, scale)
        {
            _healAmount = healAmount;
        }

        public MedkitItem(Vector3 position, int healAmount = 30) : this(position, 1.0f, healAmount) { }

        public override void OnPickup(vendor.Player player)
        {
            if (player != null)
            {
                player.Heal(_healAmount);
                SetActive(false);
            }
        }
    }
}
