using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TorchGodTweaks
{
	//TODO campfire support?
	//TODO convert regular torches (if no other biome present) into bone torch
	public class TGTSystem : ModSystem
	{
		public static HashSet<int> VanillaBiomeTorchItems;
		public static HashSet<int> ModdedBiomeTorchItems;

		/// <summary>
		/// Item to place style of the tile. Multiply by <see cref="FrameY"/> to get the proper frameY
		/// </summary>
		public static Dictionary<int, int> VanillaBiomeTorchItemToPlaceStyle;

		/// <summary>
		/// place style of the tile to item
		/// </summary>
		public static Dictionary<int, int> VanillaPlaceStyleToBiomeTorchItem;

		public static int PreHMEvilTorchRecipeGroup { get; private set; }

		public static int GoldRecipeGroup { get; private set; }

		public static LocalizedText AcceptClientChangesText { get; private set; }

		public static LocalizedText AnyEvilTorchRecipeGroupText { get; private set; }
		public static LocalizedText AnyGoldBarRecipeGroupText { get; private set; }

		/// <summary>
		/// The TileFrameY offset for the vanilla torch tile for each style
		/// </summary>
		public const int FrameY = 22;

		public override void OnModLoad()
		{
			var vanillaTorchItems = new HashSet<int>()
			{
				ItemID.Torch,
				ItemID.IceTorch,
				ItemID.DesertTorch,
				ItemID.JungleTorch,
				ItemID.HallowedTorch,
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
				ItemID.MushroomTorch,

				//ItemID.CursedTorch,
				//ItemID.IchorTorch,
				//ItemID.CoralTorch,
				//ItemID.DemonTorch,
				//ItemID.ShimmerTorch,
				//ItemID.BoneTorch,
			};

			var config = Config.Instance;
			if (config.ReverseTorchSwapForDemonTorch)
			{
				//Added opt-in because it's placed by TGF, but is not contributing to luck
				vanillaTorchItems.Add(ItemID.DemonTorch);
			}

			if (config.ReverseTorchSwapForAetherTorch)
			{
				//Added opt-in because it's placed by TGF, but is not contributing to luck
				vanillaTorchItems.Add(ItemID.ShimmerTorch);
			}

			if (config.ReverseTorchSwapForBoneTorch)
			{
				//Added opt-in because it's placed by TGF, but applies in many places (my own decision)
				vanillaTorchItems.Add(ItemID.BoneTorch);
			}

			VanillaBiomeTorchItems = vanillaTorchItems;

			VanillaBiomeTorchItemToPlaceStyle = new Dictionary<int, int>();
			VanillaPlaceStyleToBiomeTorchItem = new Dictionary<int, int>();
			foreach (var type in VanillaBiomeTorchItems)
			{
				//ContentSamples only populated with vanilla items here, but that's fine
				int style = ContentSamples.ItemsByType[type].placeStyle;
				VanillaBiomeTorchItemToPlaceStyle.Add(type, style);
				VanillaPlaceStyleToBiomeTorchItem.Add(style, type);
			}

			ModdedBiomeTorchItems = new HashSet<int>();

			foreach (var modBiome in ModContent.GetContent<ModBiome>())
			{
				int item = modBiome.BiomeTorchItemType;
				if (item > 0 /*&& ItemID.Sets.Torches[item]*/) //Crashes here, but in PostSetupContent then it crashes in AppliesToEntity
				{
					ModdedBiomeTorchItems.Add(item);
				}
			}

			string category = $"Configs.Common.";
			AcceptClientChangesText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}AcceptClientChanges"));

			AnyEvilTorchRecipeGroupText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"RecipeGroups.AnyEvilTorchRecipeGroup"));
			AnyGoldBarRecipeGroupText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"RecipeGroups.AnyGoldBarRecipeGroup"));
		}

		public override void AddRecipeGroups()
		{
			if (!Config.Instance.TorchGodsFavorRecipe)
			{
				return;
			}

			string any = Language.GetTextValue("LegacyMisc.37");
			PreHMEvilTorchRecipeGroup = RecipeGroup.RegisterGroup("TGT:EvilTorch", new RecipeGroup(() => AnyEvilTorchRecipeGroupText.Format(any), new int[]
			{
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
			}));

			GoldRecipeGroup = RecipeGroup.RegisterGroup(nameof(ItemID.GoldBar), new RecipeGroup(() => AnyGoldBarRecipeGroupText.Format(any), new int[]
			{
				ItemID.GoldBar,
				ItemID.PlatinumBar,
			}));
		}

		public override void Unload()
		{
			VanillaBiomeTorchItems = null;
			VanillaBiomeTorchItemToPlaceStyle = null;
			VanillaPlaceStyleToBiomeTorchItem = null;

			PreHMEvilTorchRecipeGroup = 0;
			GoldRecipeGroup = 0;
		}

		public override void ModifyHardmodeTasks(List<GenPass> list)
		{
			if (!Config.Instance.ConvertTorchesUponHardmode)
			{
				return;
			}

			list.Add(new TorchConversionGenPass());
		}
	}
}
