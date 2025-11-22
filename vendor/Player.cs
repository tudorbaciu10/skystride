using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skystride.vendor
{
    public class Player
    {
        private int health;

        public Player()
        {
            this.health = 100;
        }

        public int GetHealth()
        {
            return health;
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health < 0)
            {
                health = 0;
            }
        }
    }
}
