namespace LuckyPills
{
    using Exiled.API.Interfaces;
    using System.Collections.Generic;
    using System.ComponentModel;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public string PickupMessage { get; set; } = "You have picked up SCP-5854";
        public float GrenadeVomitInterval { get; set; } = 0.1f;
        public float FlashVomitInterval { get; set; } = 0.1f;
        [Description("Amount of heath done to the player every flash interval. (Used to reduce flash time)")]
        public int FlashVomitHealth { get; set; } = 5;
        public float BallVomitInterval { get; set; } = 0.2f;

        public List<string> PossibleEffects { get; set; } = new List<string>()
        {
            "explode",
            "mutate",
            "god",
            "paper",
            "upsidedown",
            "flattened",
            "bombvomit",
            "flashvomit",
            "ballvomit",
            "scp268",
            "amnesia",
            "bleeding",
            "corroding",
            "decontaminating",
            "hemorrhage",
            "panic",
            "sinkhole"
        };
        public float MinDuration { get; set; } = 5f;

        public float MaxDuration { get; set; } = 30f;
    }
}