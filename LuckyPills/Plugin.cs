namespace LuckyPills
{
    using System;
    using Exiled.API.Features;
    using PlayerEvent = Exiled.Events.Handlers.Player;
    
    public class Plugin : Plugin<Config>
    {
        public override string Author { get; } = "Reddking improved by Parkeymon";
        public override string Name { get; } = "Lucky Pills";
        public override Version Version { get; } = new Version(2, 1, 2);
        public override Version RequiredExiledVersion { get; } = new Version(2, 11, 1, 0);

        private EventHandlers _eventHandler;

        public override void OnEnabled()
        {
            _eventHandler = new EventHandlers(this);

            PlayerEvent.UsingMedicalItem += _eventHandler.OnEatThePill;
            PlayerEvent.PickingUpItem += _eventHandler.OnPickupPill;
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PlayerEvent.UsingMedicalItem -= _eventHandler.OnEatThePill;
            PlayerEvent.PickingUpItem -= _eventHandler.OnPickupPill;
            
            _eventHandler = null;
            
            base.OnDisabled();
        }
    }
}