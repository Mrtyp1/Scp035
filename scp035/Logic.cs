﻿using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using ServerMod2.API;

namespace scp035
{
	partial class EventHandler
	{
		private void LoadConfigs()
		{
			possibleItems = instance.GetConfigIntList("035_possible_items").ToList();
			scpHealth = instance.GetConfigInt("035_health");
			scpInterval = instance.GetConfigFloat("035_rotate_interval");
			is035FriendlyFire = instance.GetConfigBool("035_scp_friendly_fire");
			possessedItemCount = instance.GetConfigInt("035_infected_item_count");
		}

		private Pickup GetRandomValidItem()
		{
			//if (scpPickup != null) scpPickup.info.durability = 0;
			List<Pickup> pickups = Object.FindObjectsOfType<Pickup>().Where(x => possibleItems.Contains(x.info.itemId) && !scpPickups.ContainsKey(x)).ToList();
			Pickup p = pickups[rand.Next(pickups.Count)];
			return p;
		}

		private void InfectPlayer(Player player, Smod2.API.Item pItem)
		{
			List<Player> pList = instance.Server.GetPlayers().Where(x => x.TeamRole.Team == Smod2.API.Team.SPECTATOR).ToList();
			if (pList.Count > 0)
			{
				pItem.Remove();
				Player p035 = pList[rand.Next(pList.Count)];
				p035.ChangeRole(player.TeamRole.Role);
				p035.Teleport(player.GetPosition());
				foreach (Smod2.API.Item item in player.GetInventory()) p035.GiveItem(item.ItemType);
				p035.SetHealth(scpHealth);
				p035.SetAmmo(AmmoType.DROPPED_5, player.GetAmmo(AmmoType.DROPPED_5));
				p035.SetAmmo(AmmoType.DROPPED_7, player.GetAmmo(AmmoType.DROPPED_7));
				p035.SetAmmo(AmmoType.DROPPED_9, player.GetAmmo(AmmoType.DROPPED_9));
				p035.SetRank("red", "SCP-035");
				p035.PersonalBroadcast(10, $"You are <color=\"red\">SCP-035!</color> You have infected a body and have gained control over it!", false);
				scpPlayer = p035;
				isRotating = false;

				player.ChangeRole(Role.SPECTATOR);
				player.PersonalBroadcast(10, $"You have picked up <color=\"red\">SCP-035.</color> He has infected your body and is now in control of you.", false);
			}
		}

		private IEnumerator<float> RotatePickup()
		{
			while (isRoundStarted)
			{
				if (isRotating)
				{
					for (int i = 0; i < scpPickups.Count; i++)
					{
						Pickup p = scpPickups.ElementAt(i).Key;
						p.info.durability = scpPickups[p];
					}
					scpPickups.Clear();
					for (int i = 0; i < possessedItemCount; i++)
					{
						Pickup p = GetRandomValidItem();
						scpPickups.Add(p, p.info.durability);
						p.info.durability = dur;
					}
				}
				yield return Timing.WaitForSeconds(scpInterval);
			}
		}
	}
}
