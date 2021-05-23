namespace LuckyPills
{
    using Exiled.API.Interfaces;
    using System.Collections.Generic;
    
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        
        public List<string> PossibleEffects { get; set; }

        public float MinDuration { get; set; } = 5f;

        public float MaxDuration { get; set; } = 30f;
    }
}