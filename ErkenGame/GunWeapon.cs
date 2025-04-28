using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ErkenGame.Models;
using System.Collections.Generic;

namespace ErkenGame
{
    public class GunWeapon : Weapon
    {
        public int Ammo { get; private set; }
        public float ReloadTime { get; private set; }

        public GunWeapon()
        {
            Name = "Pistol";
            Damage = 50;
            AttackSpeed = 0.5f;
            Range = 500f;
            Ammo = 10;
            ReloadTime = 2f;
        }

        public override void Attack(Player player, List<Zombie> zombies)
        {
            if (Ammo <= 0) return;

            Ammo--;
            // Реализация выстрела
        }
    }
}
