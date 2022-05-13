using System.Collections.Generic;
using UnityEngine;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using MEC;
using CustomPlayerEffects;

namespace LuckyPills
{
    public class EventHandlers
    {
        private readonly Plugin plugin;
        private readonly Config config;

        public EventHandlers(Plugin plugin)
        {
            this.plugin = plugin;
            this.config = plugin.Config;
        }

        private static void SpawnGrenadeOnPlayer(Player player, GrenadeType grenadeType, float timer, float velocity = 1f)
        {
            bool fullForce = velocity >= 1;
            player.ThrowGrenade(grenadeType, fullForce);
        }

        private IEnumerator<float> GrenadeVomitTime(Player player, float randomTimer)
        {
            for (var i = 0; i < randomTimer * 10.0 && player.IsAlive; ++i)
            {
                yield return Timing.WaitForSeconds(plugin.Config.GrenadeVomitInterval);
                SpawnGrenadeOnPlayer(player, GrenadeType.FragGrenade, 5f);
            }
        }

        private IEnumerator<float> FlashVomitTime(Player player, float randomTimer)
        {
            for (var i = 0; i < randomTimer * 10.0 && player.IsAlive; ++i)
            {
                yield return Timing.WaitForSeconds(plugin.Config.FlashVomitInterval);
                player.Hurt(1);
                SpawnGrenadeOnPlayer(player, GrenadeType.Flashbang, 5f);
            }
        }

        private IEnumerator<float> BallVomitTime(Player player, float randomTimer)
        {
            for (var i = 0; i < randomTimer * 10.0 && player.IsAlive; ++i)
            {
                yield return Timing.WaitForSeconds(plugin.Config.BallVomitInterval);
                player.Hurt(1);
                SpawnGrenadeOnPlayer(player, GrenadeType.Scp018, 5f);
            }
        }

        public void OnPickupPill(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup.Type == ItemType.Painkillers)
            {
                ev.Player.ShowHint(plugin.Config.PickupMessage);
            }
        }

        public void OnEatThePill(UsingItemEventArgs ev)
        {
            Timing.RunCoroutine(RunPillCoroutine(ev));
        }

        private IEnumerator<float> RunPillCoroutine(UsingItemEventArgs ev)
        {
            yield return Timing.WaitForSeconds(3f);
            
            Item item = ev.Item;
            Player player = ev.Player;

            if (item.Base.ItemTypeId == ItemType.Painkillers) yield break;

            string effectType = this.NextEffect();
            float duration = Mathf.Ceil(Random.Range(config.MinDuration, config.MaxDuration));

            player.RemoveItem(item);

            player.EnableEffect(effectType, duration, true);

            switch(effectType)
            {
                case "amnesia":
                    player.EnableEffect<Amnesia>(duration);
                    player.ShowHint($"You've been given amnesia for {duration} seconds");

                    break;
                case "bleeding":
                    player.EnableEffect<Bleeding>(duration);
                    player.ShowHint($"You've been given bleeding for {duration} seconds");

                    break;
                case "bombvomit":
                    Timing.RunCoroutine(GrenadeVomitTime(player, duration));
                    player.ShowHint($"You've been given bomb vomit for {duration} seconds");

                    break;
                case "ballvomit":
                    Timing.RunCoroutine(BallVomitTime(player, duration));
                    player.ShowHint($"You've been given ball vomit for {duration} seconds");

                    break;
                case "corroding":
                    player.EnableEffect<Corroding>(duration);
                    player.ShowHint("You've been sent to the pocket dimension");

                    break;
                case "decontaminating":
                    player.EnableEffect<Decontaminating>(duration);
                    player.ShowHint($"You've been given decontamination for {duration} seconds");

                    break;
                case "explode":
                    ExplosiveGrenade explosiveGrenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);

                    explosiveGrenade.FuseTime = .5f;
                    explosiveGrenade.SpawnActive(ev.Player.Position);

                    if (player.IsAlive)
                        player.Kill(DamageType.Explosion);

                    player.ShowHint("You've been exploded");

                    break;
                case "flashed":
                    FlashGrenade flashGrenade = (FlashGrenade)Item.Create(ItemType.GrenadeFlash);
                    flashGrenade.FuseTime = .5f;
                    flashGrenade.SpawnActive(ev.Player.Position);

                    player.ShowHint("You've been flashed");

                    break;
                case "flashvomit":
                    Timing.RunCoroutine(FlashVomitTime(player, duration));
                    player.ShowHint($"You've been given flash vomit for {duration} seconds");

                    break;
                case "god":
                    player.IsGodModeEnabled = true;
                    Timing.CallDelayed(duration, () => player.IsGodModeEnabled = false);
                    player.ShowHint($"You've been given god mode for {duration} seconds");

                    break;
                case "hemorrhage":
                    player.EnableEffect<Hemorrhage>(duration);
                    player.ShowHint($"You've been hemorrhaged for {duration} seconds");

                    break;
                case "mutate":
                    Exiled.API.Features.Roles.Role cachedMutatorRole = player.Role;

                    player.DropItems();
                    player.SetRole(RoleType.Scp0492, SpawnReason.ForceClass, true);

                    Timing.CallDelayed(duration, () => player.SetRole(cachedMutatorRole, SpawnReason.ForceClass, true));

                    player.ShowHint($"You've been mutated for {duration} seconds");

                    break;
                case "paper":
                    player.Scale = new Vector3(1f, 1f, 0.01f);
                    Timing.CallDelayed(duration, () => player.Scale = new Vector3(1f, 1f, 1f));
                    player.ShowHint($"You've been turned into paper for {duration} seconds");

                    break;
                case "sinkhole":
                    player.EnableEffect<SinkHole>(duration);
                    player.ShowHint($"You've been given sinkhole effect for {duration} seconds");

                    break;
                case "scp268":
                    player.IsInvisible = true;
                    Timing.CallDelayed(duration, () => player.IsInvisible = false);

                    player.ShowHint($"You've been turned invisible for {duration} seconds");

                    break;
                case "upsidedown":
                    player.Scale = new Vector3(1f, -1f, 1f);
                    Timing.CallDelayed(duration, () =>
                    {
                        player.Scale = new Vector3(1f, 1f, 1f);
                        player.Position += new Vector3(0, 1, 0);
                    });

                    player.ShowHint($"You've been converted to Australian for {duration} seconds");

                    break;
                default:
                    player.ShowHint($"You've been {effectType} for {duration} seconds");
                    break;
            }
        }

        private string NextEffect() => plugin.Config.PossibleEffects[Random.Range(0, config.PossibleEffects.Count)];
    }
}
