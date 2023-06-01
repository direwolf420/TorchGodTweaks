using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TorchGodTweaks
{
	//TODO convert regular torches (if no other biome present) into bone torch
	public class TGTSystem : ModSystem
	{
		public static HashSet<int> VanillaBiomeTorchItems;
		public static HashSet<int> VanillaBiomeCampfireItems;
		private static HashSet<int> ModdedBiomeTorchItems; //Cache for ModBiome properties
		private static HashSet<int> ModdedBiomeCampfireItems; //Cache for ModBiome properties

		/// <summary>
		/// Item to place style of the tile. Multiply by <see cref="TorchFrameY"/> to get the proper frameY
		/// </summary>
		public static Dictionary<int, int> VanillaBiomeTorchItemToPlaceStyle;

		/// <summary>
		/// place style of the tile to item
		/// </summary>
		public static Dictionary<int, int> VanillaPlaceStyleToBiomeTorchItem;

		/// <summary>
		/// Item to place style of the tile. Multiply by <see cref="CampfireFrameX"/> to get the proper frameX
		/// </summary>
		public static Dictionary<int, int> VanillaBiomeCampfireItemToPlaceStyle;

		/// <summary>
		/// place style of the tile to item
		/// </summary>
		public static Dictionary<int, int> VanillaPlaceStyleToBiomeCampfireItem;

		public static int PreHMEvilTorchRecipeGroup { get; private set; }
		public static int GoldRecipeGroup { get; private set; }

		public static LocalizedText AcceptClientChangesText { get; private set; }

		public static LocalizedText AnyEvilTorchRecipeGroupText { get; private set; }
		public static LocalizedText AnyGoldBarRecipeGroupText { get; private set; }

		/// <summary>
		/// The TileFrameY offset for the vanilla torch tile for each style
		/// </summary>
		public const int TorchFrameY = 22;

		/// <summary>
		/// The TileFrameX offset for the vanilla campfire tile for each style
		/// </summary>
		public const int CampfireFrameX = 18 * 3;

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

			var vanillaCampfireItems = new HashSet<int>()
			{
				ItemID.Campfire,
				ItemID.FrozenCampfire,
				ItemID.DesertCampfire,
				ItemID.JungleCampfire,
				ItemID.HallowedCampfire,
				ItemID.CorruptCampfire,
				ItemID.CrimsonCampfire,
				ItemID.MushroomCampfire,

				//ItemID.CursedCampfire,
				//ItemID.IchorCampfire,
				//ItemID.CoralCampfire,
				//ItemID.DemonCampfire,
				//ItemID.ShimmerCampfire,
				//ItemID.BoneCampfire,
			};

			var config = Config.Instance;
			if (config.ReverseTorchSwapForDemonTorch)
			{
				//Added opt-in because it's placed by TGF, but is not contributing to luck
				vanillaTorchItems.Add(ItemID.DemonTorch);

				if (config.AffectCampfires)
				{
					vanillaCampfireItems.Add(ItemID.DemonCampfire);
				}
			}

			if (config.ReverseTorchSwapForAetherTorch)
			{
				//Added opt-in because it's placed by TGF, but is not contributing to luck
				vanillaTorchItems.Add(ItemID.ShimmerTorch);

				if (config.AffectCampfires)
				{
					vanillaCampfireItems.Add(ItemID.ShimmerCampfire);
				}
			}

			if (config.ReverseTorchSwapForBoneTorch)
			{
				//Added opt-in because it's placed by TGF, but applies in many places (my own decision)
				vanillaTorchItems.Add(ItemID.BoneTorch);

				if (config.AffectCampfires)
				{
					vanillaCampfireItems.Add(ItemID.BoneCampfire);
				}
			}

			VanillaBiomeTorchItems = vanillaTorchItems;
			VanillaBiomeCampfireItems = vanillaCampfireItems;

			VanillaBiomeTorchItemToPlaceStyle = new Dictionary<int, int>();
			VanillaPlaceStyleToBiomeTorchItem = new Dictionary<int, int>();
			foreach (var type in VanillaBiomeTorchItems)
			{
				//ContentSamples only populated with vanilla items here, but that's fine
				int style = ContentSamples.ItemsByType[type].placeStyle;
				VanillaBiomeTorchItemToPlaceStyle.Add(type, style);
				VanillaPlaceStyleToBiomeTorchItem.Add(style, type);
			}

			VanillaBiomeCampfireItemToPlaceStyle = new Dictionary<int, int>();
			VanillaPlaceStyleToBiomeCampfireItem = new Dictionary<int, int>();
			foreach (var type in VanillaBiomeCampfireItems)
			{
				int style = ContentSamples.ItemsByType[type].placeStyle;
				VanillaBiomeCampfireItemToPlaceStyle.Add(type, style);
				VanillaPlaceStyleToBiomeCampfireItem.Add(style, type);
			}

			ModdedBiomeTorchItems = new HashSet<int>();
			ModdedBiomeCampfireItems = new HashSet<int>();

			foreach (var modBiome in ModContent.GetContent<ModBiome>())
			{
				int torch = modBiome.BiomeTorchItemType;
				if (torch > 0 /*&& ItemID.Sets.Torches[torch]*/) //Crashes here, but in PostSetupContent then it crashes in AppliesToEntity
				{
					ModdedBiomeTorchItems.Add(torch);
				}

				int campfire = modBiome.BiomeCampfireItemType;
				if (campfire > 0)
				{
					ModdedBiomeCampfireItems.Add(campfire);
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
			ModdedBiomeTorchItems = null;
			VanillaBiomeTorchItemToPlaceStyle = null;
			VanillaPlaceStyleToBiomeTorchItem = null;

			VanillaBiomeCampfireItems = null;
			ModdedBiomeCampfireItems = null;
			VanillaBiomeCampfireItemToPlaceStyle = null;
			VanillaPlaceStyleToBiomeCampfireItem = null;

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

		public static bool IsModdedBiomeTorch(Item item)
		{
			return ItemID.Sets.Torches[item.type] && ModdedBiomeTorchItems.Contains(item.type);
		}

		public static bool IsModdedBiomeCampfire(Item item)
		{
			return item.createTile > -1 && TileID.Sets.Campfire[item.createTile] && ModdedBiomeCampfireItems.Contains(item.type);
		}
	}
}
