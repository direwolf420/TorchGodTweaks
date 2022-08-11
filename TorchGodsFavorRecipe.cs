using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	public class TorchGodsFavorRecipe : GlobalItem
	{
		public override void AddRecipes()
		{
			if (!Config.Instance.TorchGodsFavorRecipe)
			{
				return;
			}

			var goodTorches = new int[]
			{
				ItemID.IceTorch,
				ItemID.DesertTorch,
				ItemID.JungleTorch,
			};
			int torchAmount = 100;
			int otherAmount = 25;

			var recipe = Recipe.Create(ItemID.TorchGodsFavor);

			recipe.AddRecipeGroup(TGTSystem.GoldRecipeGroup, 4);
			recipe.AddIngredient(ItemID.Torch, torchAmount);
			foreach (var biomeTorch in goodTorches)
			{
				recipe.AddIngredient(biomeTorch, otherAmount);
			}
			recipe.AddRecipeGroup(TGTSystem.PreHMEvilTorchRecipeGroup, otherAmount);

			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
}
