using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ErkenGame.Models;
using System.Collections.Generic;

namespace ErkenGame
{
    public abstract class Weapon
    {
        public Texture2D Texture { get; protected set; }
        public int Damage { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public float Range { get; protected set; }
        public string Name { get; protected set; }

        public abstract void Attack(Player player, List<Zombie> zombies);
    }
}
