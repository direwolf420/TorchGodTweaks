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
			//If a vanilla or modded biome torch or campfire, and if not a default torch or campfire
			return lateInstantiation && (
				entity.type != ItemID.Torch && (TGTSystem.VanillaBiomeTorchItems.Contains(entity.type) || TGTSystem.IsModdedBiomeTorch(entity))
				|| Config.Instance.AffectCampfires && entity.type != ItemID.Campfire && (TGTSystem.VanillaBiomeCampfireItems.Contains(entity.type) || TGTSystem.IsModdedBiomeCampfire(entity))
				);
		}

		public override bool ItemSpace(Item item, Player player)
		{
			if (!spawningATorch && player.UsingBiomeTorches && Config.Instance.ReverseTorchSwap)
			{
				return true; //If attempting to pick up a biome torch/campfire, it should work even if the inventory is full
			}

			return base.ItemSpace(item, player);
		}

		public override bool OnPickup(Item item, Player player)
		{
			if (!spawningATorch && player.UsingBiomeTorches && Config.Instance.ReverseTorchSwap)
			{
				spawningATorch = true;
				var source = item.GetSource_FromThis("ReverseTorchSwap");
				player.QuickSpawnItem(source, !ItemID.Sets.Torches[item.type] && Config.Instance.AffectCampfires ? ItemID.Campfire : ItemID.Torch, item.stack); //"Convert" to default torch/campfire
				spawningATorch = false;

				return false; //Despawn biome torch/campfire
			}

			return base.OnPickup(item, player);
		}
	}
}
