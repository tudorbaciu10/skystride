using OpenTK;
using skystride.objects.weapons;
using skystride.vendor;

namespace skystride.objects.items
{
    internal class WeaponItem : Item
    {
        private Weapon _weapon;

        public WeaponItem(Weapon weapon, Vector3 position, float scale) 
            : base(weapon.ModelPath, weapon.TexturePath, position, scale)
        {
            _weapon = weapon;
        }

        public WeaponItem(Weapon weapon, Vector3 position)
            : this(weapon, position, weapon.Scale)
        {
        }

        public override void OnPickup(Player player)
        {
            if (player != null)
            {
                player.AttachWeapon(_weapon);
                SetActive(false);
            }
        }
    }
}
