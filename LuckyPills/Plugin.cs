namespace LuckyPills
{
    using System;
    using Exiled.API.Features;
    using PlayerEvent = Exiled.Events.Handlers.Player;
    
    public class LuckyPills : Plugin<Config>
    {
        public override string Author { get; } = "Parkeymon";
        public override string Name { get; } = "Lucky Pills";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 8, 0, 0);

        private EventHandlers _eventHandler;

        public override void OnEnabled()
        {
            _eventHandler = new EventHandlers();

            PlayerEvent.UsingMedicalItem += _eventHandler.OnEatThePill;
        }

        public override void OnDisabled()
        {
            _eventHandler = null;
            
            PlayerEvent.UsingMedicalItem -= _eventHandler.OnEatThePill;
        }
    }
}