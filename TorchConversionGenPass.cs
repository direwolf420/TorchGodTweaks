using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TorchGodTweaks
{
	/*
	public class TestItem : ModItem
	{
		public override LocalizedText DisplayName => Language.GetText("ItemName.IronPickaxe");
		public override LocalizedText Tooltip => LocalizedText.Empty;

		public override string Texture => "Terraria/Images/Item_1";

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.IronPickaxe);
			Item.pick = 0;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.autoReuse = false;
		}

		public override bool? UseItem(Player player)
		{
			int x = (int)Main.MouseWorld.X / 16;
			int y = (int)Main.MouseWorld.Y / 16;

			int radius = 100;
			int startX = x - radius;
			int startY = y - radius;
			int endX = x + radius;
			int endY = y + radius;
			TorchConversionGenPass.DoConversion(startX, startY, endX, endY);

			return true;
		}
	}*/

	public class TorchConversionGenPass : GenPass
	{
		public TorchConversionGenPass() : base("Convert Torches", 0.1f)
		{

		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			//---progress and configuration are both null when used for ModifyHardmodeTasks!---

			//Scan entire world
			//Find a non-evil torch
			//Check 4 (maybe 9, maybe 3x3 even) blocks around it for evil blocks
			//Convert to evil torch
			int startX = 20;
			int startY = 20;
			int endX = Main.maxTilesX - 20;
			int endY = Main.maxTilesY - 20;
			DoConversion(startX, startY, endX, endY);
		}

		public static void DoConversion(int startX, int startY, int endX, int endY)
		{
			//HashSet<int> evilTorchItems = new HashSet<int>()
			//{
			//	ItemID.HallowedTorch,
			//	ItemID.CorruptTorch,
			//	ItemID.CrimsonTorch,
			//};

			//HashSet<int> evilTorchPlaceStyles = TGTSystem.BiomeTorchItemToPlaceStyle.Where(pair => evilTorchItems.Contains(pair.Key)).Select(pair => pair.Value).ToHashSet();

			int convertedCount = 0;
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			for (int j = startY; j <= endY; j++)
			{
				for (int i = startX; i <= endX; i++)
				{
					Tile tile = Framing.GetTileSafely(i, j);
					if (!tile.HasTile)
					{
						//TODO only vanilla support atm
						continue;
					}

					//int placeStyle = tile.frameY / TGTSystem.FrameY;
					//if (evilTorchPlaceStyles.Contains(placeStyle))
					//{
					//	//Skip evil torches
					//	continue;
					//}

					//Performance critical
					const int scannedAreaSquareLength = 9; //Has to be uneven for best results

					if (tile.TileType == TileID.Torches && ConvertTorchBasedOnSurroundings(i, j, scannedAreaSquareLength))
					{
						convertedCount++;
					}

					if (tile.TileType == TileID.Campfire && Config.Instance.AffectCampfires && ConvertCampfireBasedOnSurroundings(i, j, scannedAreaSquareLength))
					{
						convertedCount++;
					}
				}
			}
			stopwatch.Stop();

			string msg = $"Converted {convertedCount} torches within {stopwatch.ElapsedMilliseconds / 1000f} seconds";
			//Main.NewText(msg);
			ModContent.GetInstance<TorchGodTweaks>().Logger.Info(msg);
		}

		private static bool IsSpecialWallRange(int wall, int specialType)
		{
			return wall >= specialType && wall < specialType + 4;
		}

		private static void ConvertTorch(int x, int y, int newPlaceStyle)
		{
			Main.tile[x, y].TileFrameY = (short)(newPlaceStyle * TGTSystem.TorchFrameY);
			//WorldGen.SquareTileFrame(x, y);
		}

		public static void ConvertCampfire(int x, int y, int newPlaceStyle)
		{
			Tile tile = Main.tile[x, y];
			int xOffset = tile.TileFrameX / 18 % 3;
			int yOffset = tile.TileFrameY / 18 % 2;
			int left = x - xOffset;
			int top = y - yOffset;

			for (int l = left; l < left + 3; l++)
			{
				for (int m = top; m < top + 2; m++)
				{
					tile = Framing.GetTileSafely(l, m);
					xOffset = tile.TileFrameX / 18 % 3;
					if (tile.HasTile && tile.TileType == TileID.Campfire)
					{
						tile.TileFrameX = (short)(xOffset * 18 + newPlaceStyle * TGTSystem.CampfireFrameX);
					}
				}
			}
		}

		private static bool ConvertTorchBasedOnSurroundings(int x, int y, int size)
		{
			int hallowStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.HallowedTorch];
			int crimsonStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CrimsonTorch];
			int corruptStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CorruptTorch];

			int newPlaceStyle = GetNewPlaceStyle(x, y, size, hallowStyle, crimsonStyle, corruptStyle);

			if (newPlaceStyle != -1)
			{
				ConvertTorch(x, y, newPlaceStyle);
				return true;
			}

			return false;
		}

		private static bool ConvertCampfireBasedOnSurroundings(int x, int y, int size)
		{
			int hallowStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.HallowedCampfire];
			int crimsonStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.CrimsonCampfire];
			int corruptStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.CorruptCampfire];

			int newPlaceStyle = GetNewPlaceStyle(x, y, size, hallowStyle, crimsonStyle, corruptStyle);

			if (newPlaceStyle != -1)
			{
				ConvertCampfire(x, y, newPlaceStyle);
				return true;
			}

			return false;
		}

		private static int GetNewPlaceStyle(int x, int y, int size, int hallowStyle, int crimsonStyle, int corruptStyle)
		{
			int hallowCount = 0;
			int corruptCount = 0;
			int crimsonCount = 0;
			int newPlaceStyle = -1;
			int totalCount = size * size;
			int thresholdForPrematureConversion = (int)((totalCount / 2) * 0.67f); //bonus factor due to alot of air around torches usually. Otherwise we would have to count all non-air tiles first before doing anything here
			int halfLength = size / 2;
			for (int j = y - halfLength; j <= y + halfLength; j++)
			{
				if (newPlaceStyle != -1)
				{
					break;
				}

				for (int i = x - halfLength; i <= x + halfLength; i++)
				{
					if (newPlaceStyle != -1)
					{
						break;
					}

					Tile tile = Framing.GetTileSafely(i, j);
					if (x == i && y == j)
					{
						//Only check the wall behind the torch

						ushort wall = tile.WallType;
						if (IsWallHallow(wall))
						{
							newPlaceStyle = hallowStyle;
						}
						else if (IsWallCrimson(wall))
						{
							newPlaceStyle = crimsonStyle;
						}
						else if (IsWallCorrupt(wall))
						{
							newPlaceStyle = corruptStyle;
						}

						continue;
					}

					if (!tile.HasTile)
					{
						continue;
					}

					int type = tile.TileType;
					if (TileID.Sets.Hallow[type])
					{
						hallowCount++;
					}
					else if (TileID.Sets.Crimson[type])
					{
						crimsonCount++;
					}
					else if (TileID.Sets.Corrupt[type])
					{
						corruptCount++;
					}

					//Prematurely set new placestyle if already counted enough
					if (hallowCount > thresholdForPrematureConversion)
					{
						newPlaceStyle = hallowStyle;
					}
					else if (crimsonCount > thresholdForPrematureConversion)
					{
						newPlaceStyle = crimsonStyle;
					}
					else if (corruptCount > thresholdForPrematureConversion)
					{
						newPlaceStyle = corruptStyle;
					}
				}
			}

			if (newPlaceStyle == -1)
			{
				//No predominant pick: choose the max (would require atleast 1 evil block)
				if (hallowCount > 0 && hallowCount >= crimsonCount && hallowCount >= corruptCount)
				{
					newPlaceStyle = hallowStyle;
				}
				else if (crimsonCount > 0 && crimsonCount >= hallowCount && crimsonCount >= corruptCount)
				{
					newPlaceStyle = crimsonStyle;
				}
				else if (corruptCount > 0 && corruptCount >= hallowCount && corruptCount >= crimsonCount)
				{
					newPlaceStyle = corruptStyle;
				}
			}

			return newPlaceStyle;
		}

		private static bool IsWallHallow(ushort wall)
		{
			return WallID.Sets.Hallow[wall] || IsSpecialWallRange(wall, WallID.HallowUnsafe1);
		}

		private static bool IsWallCrimson(ushort wall)
		{
			return WallID.Sets.Crimson[wall] || IsSpecialWallRange(wall, WallID.CrimsonUnsafe1);
		}

		private static bool IsWallCorrupt(ushort wall)
		{
			return WallID.Sets.Corrupt[wall] || IsSpecialWallRange(wall, WallID.CorruptionUnsafe1);
		}
	}
}
