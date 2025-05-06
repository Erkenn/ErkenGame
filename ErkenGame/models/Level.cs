using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ErkenGame.Models
{
    public class Level
    {
        public bool IsTutorialLevel { get; set; }
        public bool TutorialCompleted { get; set; }
        public List<TutorialStep> TutorialSteps { get; set; } = new List<TutorialStep>();
        public List<string> TutorialMessages { get; set; } = new List<string>();
        public int CurrentTutorialStep { get; set; } = 0;
        public string Name { get; set; }
        public string MapTexture { get; set; }
        public Vector2 PlayerStartPosition { get; set; }
        public List<Vector2> ZombieSpawnPoints { get; set; }
        public List<Obstacle> Obstacles { get; set; }
        public Vector2 PortalPosition { get; set; }
        public int ZombiesToKill { get; set; }

        public Level()
        {
            ZombieSpawnPoints = new List<Vector2>();
            Obstacles = new List<Obstacle>();
        }
    }

    public class TutorialStep
    {
        public string Message { get; set; }
        public Action OnShow { get; set; }
        public Func<bool> CompletionCondition { get; set; }
        public bool WasShown { get; set; }  // Заменяем Active на WasShown
        public bool IsCompleted { get; set; }
    }
}