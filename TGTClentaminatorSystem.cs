using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	//Only works with vanilla!!
	public class TGTClentaminatorSystem : ModSystem
	{
		public static Lazy<HashSet<int>> ConvertibleTorchPlaceStyles { get; private set; } = new Lazy<HashSet<int>>(() =>
		{
			//Only torches for which solutions exist
			var convertibleTorchItems = new HashSet<int>()
			{
				ItemID.Torch,
				ItemID.HallowedTorch,
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
				ItemID.IceTorch,
				ItemID.DesertTorch,
				ItemID.MushroomTorch,
			};

			return TGTSystem.VanillaBiomeTorchItemToPlaceStyle.Where(pair => convertibleTorchItems.Contains(pair.Key)).Select(pair => pair.Value).ToHashSet();
		});

		public static Lazy<HashSet<int>> ConvertibleCampfirePlaceStyles { get; private set; } = new Lazy<HashSet<int>>(() =>
		{
			//Only campfires for which solutions exist
			var convertibleCampfireItems = new HashSet<int>()
			{
				ItemID.Campfire,
				ItemID.HallowedCampfire,
				ItemID.CorruptCampfire,
				ItemID.CrimsonCampfire,
				ItemID.FrozenCampfire,
				ItemID.DesertCampfire,
				ItemID.MushroomCampfire,
			};

			return TGTSystem.VanillaBiomeCampfireItemToPlaceStyle.Where(pair => convertibleCampfireItems.Contains(pair.Key)).Select(pair => pair.Value).ToHashSet();
		});

		public override void Load()
		{
			//Purification powder does not use this, so need to manually convert in its AI
			//Works for clentaminator and thrown waters
			On_WorldGen.Convert_int_int_int_int += WorldGen_Convert;
		}

		public override void Unload()
		{
			ConvertibleTorchPlaceStyles = null;
			ConvertibleCampfirePlaceStyles = null;
		}

		public static void ConvertTorch(int x, int y, int newPlaceStyle)
		{
			Main.tile[x, y].TileFrameY = (short)(newPlaceStyle * TGTSystem.TorchFrameY);
			WorldGen.SquareTileFrame(x, y);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(-1, x, y);
			}
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

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(-1, left + 1, top, 3);
			}
		}

		private static void WorldGen_Convert(On_WorldGen.orig_Convert_int_int_int_int orig, int i, int j, int conversionType, int size)
		{
			orig(i, j, conversionType, size);

			if (!Config.Instance.ConvertTorchesWhenClentaminating)
			{
				return;
			}

			for (int y = j - size; y <= j + size; y++)
			{
				for (int x = i - size; x <= i + size; x++)
				{
					if (!WorldGen.InWorld(x, y, 1) || Math.Abs(x - i) + Math.Abs(y - j) >= 6)
					{
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (!tile.HasTile)
					{
						continue;
					}

					if (tile.TileType == TileID.Torches)
					{
						ConvertBiomeTorches(conversionType, y, x, tile);
					}

					if (Config.Instance.AffectCampfires && tile.TileType == TileID.Campfire)
					{
						ConvertBiomeCampfires(conversionType, y, x, tile);
					}
				}
			}
		}

		private static void ConvertBiomeTorches(int conversionType, int y, int x, Tile tile)
		{
			int placeStyle = tile.TileFrameY / TGTSystem.TorchFrameY;
			int intendedStyle = -1;

			switch (conversionType)
			{
				case BiomeConversionID.Purity:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.Torch];
					break;
				case BiomeConversionID.Corruption:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CorruptTorch];
					break;
				case BiomeConversionID.Hallow:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.HallowedTorch];
					break;
				case BiomeConversionID.GlowingMushroom:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.MushroomTorch];
					break;
				case BiomeConversionID.Crimson:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CrimsonTorch];
					break;
				case BiomeConversionID.Sand:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.DesertTorch];
					break;
				case BiomeConversionID.Snow:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.IceTorch];
					break;
				case BiomeConversionID.Dirt:
					intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.Torch];
					break;
				default:
					break;
			}

			if (intendedStyle > -1)
			{
				if (ConvertibleTorchPlaceStyles.Value.Contains(placeStyle) && placeStyle != intendedStyle)
				{
					ConvertTorch(x, y, intendedStyle);
				}
			}
		}

		private static void ConvertBiomeCampfires(int conversionType, int y, int x, Tile tile)
		{
			int placeStyle = tile.TileFrameX / TGTSystem.CampfireFrameX;
			int intendedStyle = -1;

			switch (conversionType)
			{
				case BiomeConversionID.Purity:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.Campfire];
					break;
				case BiomeConversionID.Corruption:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.CorruptCampfire];
					break;
				case BiomeConversionID.Hallow:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.HallowedCampfire];
					break;
				case BiomeConversionID.GlowingMushroom:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.MushroomCampfire];
					break;
				case BiomeConversionID.Crimson:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.CrimsonCampfire];
					break;
				case BiomeConversionID.Sand:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.DesertCampfire];
					break;
				case BiomeConversionID.Snow:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.FrozenCampfire];
					break;
				case BiomeConversionID.Dirt:
					intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.Campfire];
					break;
				default:
					break;
			}

			if (intendedStyle > -1)
			{
				if (ConvertibleCampfirePlaceStyles.Value.Contains(placeStyle) && placeStyle != intendedStyle)
				{
					ConvertCampfire(x, y, intendedStyle);
				}
			}
		}
	}

	public class TGTPurificationPowderProjectile : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return lateInstantiation && entity.type == ProjectileID.PurificationPowder;
		}

		public override void AI(Projectile projectile)
		{
			if (!Config.Instance.ConvertTorchesWhenClentaminating)
			{
				return;
			}

			if (Main.myPlayer != projectile.owner)
			{
				return;
			}

			int startX = Math.Clamp((int)(projectile.position.X / 16f) - 1, 0, Main.maxTilesY);
			int endXPlus1 = Math.Clamp((int)((projectile.position.X + (float)projectile.width) / 16f) + 2, 0, Main.maxTilesX);
			int startY = Math.Clamp((int)(projectile.position.Y / 16f) - 1, 0, Main.maxTilesY);
			int endYPlus1 = Math.Clamp((int)((projectile.position.Y + (float)projectile.height) / 16f) + 2, 0, Main.maxTilesY);

			Vector2 tilePos = default(Vector2);
			for (int y = startY; y < endYPlus1; y++)
			{
				for (int x = startX; x < endXPlus1; x++)
				{
					tilePos.X = x * 16;
					tilePos.Y = y * 16;
					if (!(projectile.position.X + (float)projectile.width > tilePos.X) || !(projectile.position.X < tilePos.X + 16f) || !(projectile.position.Y + (float)projectile.height > tilePos.Y) || !(projectile.position.Y < tilePos.Y + 16f))
					{
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (!tile.HasTile)
					{
						continue;
					}

					if (tile.TileType == TileID.Torches)
					{
						ConvertEvilTorches(y, x, tile);
					}

					if (Config.Instance.AffectCampfires && tile.TileType == TileID.Campfire)
					{
						ConvertEvilCampfires(y, x, tile);
					}
				}
			}
		}

		private static void ConvertEvilTorches(int y, int x, Tile tile)
		{
			int placeStyle = tile.TileFrameY / TGTSystem.TorchFrameY;

			int intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.Torch];
			int corruptionStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CorruptTorch];
			int crimsonStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CrimsonTorch];
			if ((placeStyle == corruptionStyle || placeStyle == crimsonStyle) && placeStyle != intendedStyle)
			{
				TGTClentaminatorSystem.ConvertTorch(x, y, intendedStyle);
			}
		}

		private static void ConvertEvilCampfires(int y, int x, Tile tile)
		{
			int placeStyle = tile.TileFrameX / TGTSystem.CampfireFrameX;

			int intendedStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.Campfire];
			int corruptionStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.CorruptCampfire];
			int crimsonStyle = TGTSystem.VanillaBiomeCampfireItemToPlaceStyle[ItemID.CrimsonCampfire];
			if ((placeStyle == corruptionStyle || placeStyle == crimsonStyle) && placeStyle != intendedStyle)
			{
				TGTClentaminatorSystem.ConvertCampfire(x, y, intendedStyle);
			}
		}
	}
}
