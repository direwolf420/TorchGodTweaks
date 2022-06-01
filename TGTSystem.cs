using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TorchGodTweaks
{
	public class TGTSystem : ModSystem
	{
		public static HashSet<int> BiomeTorchItems;

		/// <summary>
		/// Item to place style of the tile. Multiply by <see cref="FrameY"/> to get the proper frameY
		/// </summary>
		public static Dictionary<int, int> BiomeTorchItemToPlaceStyle;

		/// <summary>
		/// place style of the tile to item
		/// </summary>
		public static Dictionary<int, int> PlaceStyleToBiomeTorchItem;

		public const int FrameY = 22;

		public override void OnModLoad()
		{
			BiomeTorchItems = new HashSet<int>()
			{
				ItemID.IceTorch,
				ItemID.DesertTorch,
				ItemID.JungleTorch,
				ItemID.HallowedTorch,
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
				//ItemID.CursedTorch,
				//ItemID.IchorTorch,
				//ItemID.CoralTorch,
				//No bone torch
			};

			BiomeTorchItemToPlaceStyle = new Dictionary<int, int>();
			PlaceStyleToBiomeTorchItem = new Dictionary<int, int>();
			foreach (var type in BiomeTorchItems)
			{
				int style = ContentSamples.ItemsByType[type].placeStyle;
				BiomeTorchItemToPlaceStyle.Add(type, style);
				PlaceStyleToBiomeTorchItem.Add(style, type);
			}
		}

		public override void PostSetupContent()
		{
			//TODO modded
		}

		public override void Unload()
		{
			BiomeTorchItems = null;
			BiomeTorchItemToPlaceStyle = null;
			PlaceStyleToBiomeTorchItem = null;
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
