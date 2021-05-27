namespace LuckyPills
{
  using Exiled.API.Enums;
  using Exiled.API.Features;
  using Exiled.Events.EventArgs;
  using Grenades;
  using MEC;
  using Mirror;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;
  
  public class EventHandlers
  {

    private readonly Plugin _plugin;

    public EventHandlers(Plugin plugin)
    {
      _plugin = plugin;
    }

    private static void SpawnGrenadeOnPlayer(Player ply,
      GrenadeType grenadeType,
      float timer,
      float velocity = 1f)
    {
      var plyGrenadeManager = ply.GrenadeManager;
      GrenadeSettings grenadeSettings;
      switch (grenadeType)
      {
        case GrenadeType.FragGrenade:
          grenadeSettings = plyGrenadeManager.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
          break;
        case GrenadeType.Flashbang:
          grenadeSettings = plyGrenadeManager.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFlash);
          break;
        case GrenadeType.Scp018:
          grenadeSettings = plyGrenadeManager.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(grenadeType), grenadeType, null);
      }
      Grenade grenade;
      if (grenadeType != GrenadeType.Scp018)
      {
        grenade = UnityEngine.Object.Instantiate(grenadeSettings?.grenadeInstance).GetComponent<Grenade>();
        grenade.fuseDuration = timer;
        grenade.InitData(plyGrenadeManager, Vector3.zero, plyGrenadeManager.hub.PlayerCameraReference.forward, velocity);
      }
      else
      {
        var relativeVelocity = new Vector3(UnityEngine.Random.Range(0.0f, 2f), UnityEngine.Random.Range(0.0f, 2f), UnityEngine.Random.Range(0.0f, 2f));
        grenade = UnityEngine.Object.Instantiate(grenadeSettings?.grenadeInstance).GetComponent<Scp018Grenade>();
        grenade.fuseDuration = timer;
        grenade.InitData(plyGrenadeManager, relativeVelocity, ply.ReferenceHub.PlayerCameraReference.forward, velocity);
      }
      NetworkServer.Spawn(grenade.gameObject);
    }

    private IEnumerator<float> GrenadeVomitTime(Player player, float randomTimer)
    {
      for (var i = 0; i < randomTimer * 10.0 && player.IsAlive; ++i)
      {
        yield return Timing.WaitForSeconds(_plugin.Config.GrenadeVomitInterval);
        SpawnGrenadeOnPlayer(player, GrenadeType.FragGrenade, 5f);
      }
    }
    
    private IEnumerator<float> FlashVomitTime(Player player, float randomTimer)
    {
      for (var i = 0; i < randomTimer * 10.0 && player.IsAlive; ++i)
      {
        yield return Timing.WaitForSeconds(_plugin.Config.FlashVomitInterval);
        player.Health -= 1;
        SpawnGrenadeOnPlayer(player, GrenadeType.Flashbang, 5f);
        if (!(player.Health < 0)) continue;
        player.Kill();
      }
    }
    
    private IEnumerator<float> BallVomitTime(Player player, float randomTimer)
    {
      for (var i = 0; i < randomTimer * 10.0 && player.IsAlive; ++i)
      {
        yield return Timing.WaitForSeconds(_plugin.Config.BallVomitInterval);
        SpawnGrenadeOnPlayer(player, GrenadeType.Scp018, 5f);
      }
    }

    public void OnPickupPill(PickingUpItemEventArgs ev)
    {
      if (ev.Pickup.ItemId == ItemType.Painkillers)
      {
        ev.Player.ShowHint(_plugin.Config.PickupMessage);
      }
    }
    
    public void OnEatThePill(UsingMedicalItemEventArgs ev)
    {
      Timing.RunCoroutine(RunPillCoroutine(ev));
    }

    private IEnumerator<float> RunPillCoroutine(UsingMedicalItemEventArgs ev)
    {
      yield return Timing.WaitForSeconds(3f);
      if (ev.Item != ItemType.Painkillers)
        yield break;
      var type = this.NextEffect();
      var num = UnityEngine.Random.Range(_plugin.Config.MinDuration, _plugin.Config.MaxDuration);
      ev.Player.RemoveItem();
      ev.Player.CurrentItem = new Inventory.SyncItemInfo()
      {
        id = ItemType.None
      };
      switch (type)
      {
        case "explode":
        {
          SpawnGrenadeOnPlayer(ev.Player, GrenadeType.FragGrenade, 0.1f);
          if (ev.Player.IsAlive)
            ev.Player.Kill(DamageTypes.Grenade);
          break;
        }
        case "mutate":
        {
          var cachedMutatorRole = ev.Player.Role;
          ev.Player.DropItems();
          ev.Player.SetRole(RoleType.Scp0492, true);
          Timing.CallDelayed(num, () => ev.Player.SetRole(cachedMutatorRole, true));
          break;
        }
        case "god":
          ev.Player.IsGodModeEnabled = true;
          Timing.CallDelayed(num, () => ev.Player.IsGodModeEnabled = false);
          break;
        
        case "paper":
          ev.Player.Scale = new Vector3(1f, 1f, 0.01f);
          Timing.CallDelayed(num, () => ev.Player.Scale = new Vector3(1f, 1f, 1f));
          break;
        
        case "upsidedown":
          ev.Player.Scale = new Vector3(1f, -1f, 1f);
          Timing.CallDelayed(num, () => ev.Player.Scale = new Vector3(1f, 1f, 1f));
          break;
        
        case "flattened":
          ev.Player.Scale = new Vector3(1f, 0.5f, 1f);
          Timing.CallDelayed(num, () => ev.Player.Scale = new Vector3(1f, 1f, 1f));
          break;
        
        case "bombvomit":
          Timing.RunCoroutine(GrenadeVomitTime(ev.Player, num));
          break;
        
        case "flashvomit":
          Timing.RunCoroutine(FlashVomitTime(ev.Player, num));
          break;
        
        case "ballvomit":
          Timing.RunCoroutine(BallVomitTime(ev.Player, num));
          break;
        
        case "scp268":
          ev.Player.IsInvisible = true;
          Timing.CallDelayed(num, () => ev.Player.IsInvisible = false);
          break;
      }

      ev.Player.ReferenceHub.playerEffectsController.EnableByString(type, num);
      switch (type)
      {
        case "amnesia":
          ev.Player.ShowHint($"You've been given amnesia for {num} seconds");
          break;
        
        case "bleeding":
          ev.Player.ShowHint($"You've been given bleeding for {num} seconds");
          break;
        
        case "bombvomit":
          ev.Player.ShowHint($"You've been given bomb vomit for {num} seconds");
          break;
        
        case "flashvomit":
          ev.Player.ShowHint($"You've been given flash vomit for {num} seconds");
          break;
        
        case "ballvomit":
          ev.Player.ShowHint($"You've been given flash vomit for {num} seconds");
          break;
        
        case "corroding":
          ev.Player.ShowHint("You've been sent to the pocket dimension");
          break;
        
        case "decontaminating":
          ev.Player.ShowHint($"You've been given decontamination for {num} seconds");
          break;
        
        case "explode":
          ev.Player.ShowHint("You've been exploded");
          break;
        
        case "flashed":
          ev.Player.ShowHint("You've been flashed");
          break;
        
        case "god":
          ev.Player.ShowHint($"You've been given god mode for {num} seconds");
          break;
        
        case "hemorrhage":
          ev.Player.ShowHint($"You've been hemorrhaged for {num} seconds");
          break;
        
        case "mutate":
          ev.Player.ShowHint($"You've been mutated for {num} seconds");
          break;
        
        case "panic":
          ev.Player.ShowHint($"You've been panicked for {num} seconds");
          break;
        
        case "paper":
          ev.Player.ShowHint($"You've been turned into paper for {num} seconds");
          break;
        
        case "sinkhole":
          ev.Player.ShowHint($"You've been given sinkhole effect for {num} seconds");
          break;
        
        case "upsidedown":
          ev.Player.ShowHint($"You've been converted to Australian for {num} seconds");
          break;
        
        default:
          ev.Player.ShowHint($"You've been {type} for {num} seconds");
          break;
      }
    }
    
    private string NextEffect() => _plugin.Config.PossibleEffects[UnityEngine.Random.Range(0, _plugin.Config.PossibleEffects.Count)];
  }
}