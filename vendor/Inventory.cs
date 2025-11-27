using skystride.objects.weapons;
using System.Collections.Generic;

namespace skystride.vendor
{
    internal class Inventory
    {
        private List<Weapon> _weapons = new List<Weapon>();
        private int _currentWeaponIndex = -1;

        public void AddWeapon(Weapon weapon)
        {
            if (weapon == null) return;
            _weapons.Add(weapon);
            if (_currentWeaponIndex == -1)
            {
                _currentWeaponIndex = 0;
            }
        }

        public Weapon GetCurrentWeapon()
        {
            if (_currentWeaponIndex >= 0 && _currentWeaponIndex < _weapons.Count)
            {
                return _weapons[_currentWeaponIndex];
            }
            return null;
        }

        public void SwitchWeapon(int index)
        {
            if (index >= 0 && index < _weapons.Count)
            {
                _currentWeaponIndex = index;
            }
        }

        public int WeaponCount => _weapons.Count;
    }
}
