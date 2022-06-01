using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	public class TorchGodTweaks : Mod
	{
		public static int PreHMEvilTorchRecipeGroup { get; private set; }

		public static int GoldRecipeGroup { get; private set; }

		public override void AddRecipeGroups()
		{
			if (!Config.Instance.TorchGodsFavorRecipe)
			{
				return;
			}

			string any = Language.GetTextValue("LegacyMisc.37") + " ";
			PreHMEvilTorchRecipeGroup = RecipeGroup.RegisterGroup("TGT:EvilTorch", new RecipeGroup(() => any + "Evil Torch", new int[]
			{
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
			}));

			GoldRecipeGroup = RecipeGroup.RegisterGroup("TGT:GoldBar", new RecipeGroup(() => any + Lang.GetItemNameValue(ItemID.GoldBar), new int[]
			{
				ItemID.GoldBar,
				ItemID.PlatinumBar,
			}));
		}

		public override void Unload()
		{
			PreHMEvilTorchRecipeGroup = 0;
			GoldRecipeGroup = 0;
		}
	}
}
