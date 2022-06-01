using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	public class TGTTorchItem : GlobalItem
	{
		private static bool spawningATorch = false;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			//If a vanilla torch, and if not a default or bone torch
			//TODO figure out how to make modded torches work with this on custom biomes
			return lateInstantiation && TGTSystem.BiomeTorchItems.Contains(entity.type) /*&& ItemID.Sets.Torches[entity.type]*/;
		}

		public override bool ItemSpace(Item item, Player player)
		{
			if (!spawningATorch && player.UsingBiomeTorches && Config.Instance.ReverseTorchSwap)
			{
				return true; //If attempting to pick up a biome torch, it should work even if the inventory is full
			}

			return base.ItemSpace(item, player);
		}

		public override bool OnPickup(Item item, Player player)
		{
			if (!spawningATorch && player.UsingBiomeTorches && Config.Instance.ReverseTorchSwap)
			{
				spawningATorch = true;
				var source = item.GetSource_FromThis("ReverseTorchSwap");
				player.QuickSpawnItem(source, ItemID.Torch, item.stack); //"Convert" to default torch
				spawningATorch = false;

				return false; //Despawn biome torch
			}

			return base.OnPickup(item, player);
		}
	}
}
