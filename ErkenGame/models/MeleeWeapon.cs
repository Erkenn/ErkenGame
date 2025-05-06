using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ErkenGame.Models;
using System.Collections.Generic;

namespace ErkenGame
{
    public class MeleeWeapon : Weapon
    {
        public MeleeWeapon()
        {
            Name = "Baseball Bat";
            Damage = 30;
            AttackSpeed = 1.5f;
            Range = 100f;
        }

        public override void Attack(Player player, List<Zombie> zombies)
        {
            // Реализация ближней атаки
        }
    }
}
