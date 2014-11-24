using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HereticXNA
{
	public static class Frame
	{
		public static r_local.sector_t[] sectors = new r_local.sector_t[256];
		public static int sectorCount = 0;
		public static BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);
		public static List<Vector4> debugRects = new List<Vector4>();

		private static int checkId = 0;
		private static Vector2 viewDir;
		private static Matrix projectMat;
		private static Vector2 screenSize;
		private static Vector3 depth;
#if DEBUG
		private static int sectorChecked = 0;
		private static int portalChecked = 0;
		private static int portalTraversed = 0;
		private static double cullingTime;
#endif

		public static void prepare()
		{
			setupCamera();
			findVisibleSectors();
		}

		private static void findVisibleSectors()
		{
			sectorCount = 0;

			// Find starting sector, the one we stand in
			r_local.sector_t startSector = r_main.R_PointInSubsector(
				(int)Game1.instance.camPos.X << DoomDef.FRACBITS,
				(int)Game1.instance.camPos.Y << DoomDef.FRACBITS).sector;
		/*	r_local.sector_t startSector = r_main.R_PointInSubsector(
				g_game.players[g_game.consoleplayer].mo.x,
				g_game.players[g_game.consoleplayer].mo.y).sector;*/
			projectMat = Game1.instance.view * Game1.instance.proj;

			++checkId;
#if DEBUG
			sectorChecked = 0;
			portalChecked = 0;
			portalTraversed = 0;
			DateTime t = DateTime.Now;
#endif

			screenSize = new Vector2((float)Game1.instance.GraphicsDevice.Viewport.Width, (float)Game1.instance.GraphicsDevice.Viewport.Height);
			depth = new Vector3(
				Game1.instance.GraphicsDevice.Viewport.MinDepth, 
				Game1.instance.GraphicsDevice.Viewport.MaxDepth,
				Game1.instance.GraphicsDevice.Viewport.MaxDepth - Game1.instance.GraphicsDevice.Viewport.MinDepth);
			Vector4 screenBB = new Vector4(0, 0, screenSize.X, screenSize.Y);

			debugRects.Clear();
			addSector(startSector, ref screenBB);

#if DEBUG
			cullingTime = (DateTime.Now - t).TotalMilliseconds;
#endif
		}

		private static void checkLines(r_local.sector_t sector, ref Vector4 in_prevScreenBB)
		{
			foreach (r_local.seg_t seg in sector.segs)
			{
				if (seg.culling_checkId == checkId) continue;
				if (seg.culling_alreadyFullyIncluded == checkId) continue;

				testLine(sector, seg, ref in_prevScreenBB);
			}
		}

		static public void Project(ref Vector3 source)
		{
			Vector4 projectResult;
			Vector4.Transform(ref source, ref projectMat, out projectResult);
			projectResult.Z = projectResult.Z * depth.Z;
			float invW = 1.0f / projectResult.W;
			projectResult *= invW;

			source.X = (1 + projectResult.X) * screenSize.X * .5f;
			source.Y = (1 - projectResult.Y) * screenSize.Y * .5f;
			source.Z = projectResult.Z + depth.X;
		}

		private const float camLimit = 1.0f;

		private static void testLine(r_local.sector_t sector, r_local.seg_t li, ref Vector4 in_prevScreenBB)
		{
#if DEBUG
			++portalChecked;
#endif

			r_local.sector_t nextSector;
			if (li.frontsector == sector)
			{
				if (li.backsector == null) return;
				nextSector = li.backsector;
			}
			else
			{
				return;
			}

			Vector3 v1, v2, screenSpace;
			v1.X = li.v1.x >> DoomDef.FRACBITS;
			v1.Y = li.v1.y >> DoomDef.FRACBITS;
			v2.X = li.v2.x >> DoomDef.FRACBITS;
			v2.Y = li.v2.y >> DoomDef.FRACBITS;
			v1.Z = Math.Max(li.frontsector.floorheight >> DoomDef.FRACBITS, li.backsector.floorheight >> DoomDef.FRACBITS);
			v2.Z = Math.Min(li.frontsector.ceilingheight >> DoomDef.FRACBITS, li.backsector.ceilingheight >> DoomDef.FRACBITS);
			if (v1.Z >= v2.Z) return;

		//	if (frustum.Contains(liBB) == ContainmentType.Disjoint) return;

			Vector4 screenBB;
			Vector2 originalBB;
			screenBB.X = screenSize.X + 10000;
			screenBB.Y = screenSize.Y + 10000;
			screenBB.Z = -10000;
			screenBB.W = -10000;
			bool allBehind = true;

			screenSpace = v1;
			Project(ref screenSpace);
			screenBB.X = screenSpace.X < screenBB.X ? screenSpace.X : screenBB.X;
			screenBB.W = screenSpace.Y > screenBB.W ? screenSpace.Y : screenBB.W;
			if (screenSpace.Z < camLimit) allBehind = false;

			screenSpace = v1;
			screenSpace.Z = v2.Z;
			Project(ref screenSpace);
			screenBB.X = screenSpace.X < screenBB.X ? screenSpace.X : screenBB.X;
			screenBB.Y = screenSpace.Y < screenBB.Y ? screenSpace.Y : screenBB.Y;
			if (screenSpace.Z < camLimit) allBehind = false;

			screenSpace = v2;
			screenSpace.Z = v1.Z;
			Project(ref screenSpace);
			screenBB.Z = screenSpace.X > screenBB.Z ? screenSpace.X : screenBB.Z;
			screenBB.W = screenSpace.Y > screenBB.W ? screenSpace.Y : screenBB.W;
			if (screenSpace.Z < camLimit) allBehind = false;

			screenSpace = v2;
			screenSpace.Z = v2.Z;
			Project(ref screenSpace);
			screenBB.Y = screenSpace.Y < screenBB.Y ? screenSpace.Y : screenBB.Y;
			screenBB.Z = screenSpace.X > screenBB.Z ? screenSpace.X : screenBB.Z;
			if (screenBB.X >= screenBB.Z) return;
			if (screenBB.Y >= screenBB.W) return;
			if (screenSpace.Z < camLimit) allBehind = false;

			if (allBehind) return;

			if (screenBB.X < 0) screenBB.X = 0;
			if (screenBB.Y < 0) screenBB.Y = 0;
			if (screenBB.Z > screenSize.X) screenBB.Z = screenSize.X;
			if (screenBB.W > screenSize.Y) screenBB.W = screenSize.Y;

			originalBB.X = screenBB.X;
			originalBB.Y = screenBB.Z;

			// Merge with previous bounding
			screenBB.X = screenBB.X > in_prevScreenBB.X ? screenBB.X : in_prevScreenBB.X;
			screenBB.Y = screenBB.Y > in_prevScreenBB.Y ? screenBB.Y : in_prevScreenBB.Y;
			screenBB.Z = screenBB.Z < in_prevScreenBB.Z ? screenBB.Z : in_prevScreenBB.Z;
			screenBB.W = screenBB.W < in_prevScreenBB.W ? screenBB.W : in_prevScreenBB.W;

			if (screenBB.X >= screenBB.Z) return;
			if (screenBB.Y >= screenBB.W) return;
			if (screenBB.Z < 0 || screenBB.X > screenSize.X) return;
			if (screenBB.W < 0 || screenBB.Y > screenSize.Y) return;

			if (originalBB.X == screenBB.X &&
				originalBB.Y == screenBB.Z)
			{
				li.culling_alreadyFullyIncluded = checkId;
			}

			debugRects.Add(screenBB);
#if DEBUG
			++portalTraversed;
#endif

			li.culling_checkId = checkId;
			addSector(nextSector, ref screenBB);
			li.culling_checkId = 0;
		}

		private static void addSector(r_local.sector_t sector, ref Vector4 in_prevScreenBB)
		{
			if (sector.checkId == checkId) return;
			if (sector.addedId != checkId) 
				sectors[sectorCount++] = sector;
#if DEBUG
			++sectorChecked;
#endif
			sector.addedId = checkId;
			sector.checkId = checkId;
			checkLines(sector, ref in_prevScreenBB);
			sector.checkId = 0;
		}

		private static void setupCamera()
		{
			// We setup our render matrices
			float ratio = (float)Game1.instance.GraphicsDevice.Viewport.Width / (float)Game1.instance.GraphicsDevice.Viewport.Height;

			// 90 horizontally, but modern rendering sets it vertically that's why we do this.
			// We hardcoded the values here, because we want the vertical fov to stay the same how ever large
			// your monitor is. So the fov will be greater on the sides in widescreen
			float fov = (768.0f / 1024.0f) * 90.0f; // 67.5 degrees vertically
			Game1.instance.proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), ratio, .1f, 10000);
			float camAngleX = 0;
			float camAngleZ = 0;
			Vector3 pos = Vector3.Zero;
			if (!Game1.instance.useFreeCam)
			{
				float playerX = (float)(r_main.viewx >> DoomDef.FRACBITS);
				float playerY = (float)(r_main.viewy >> DoomDef.FRACBITS);
				float playerZ = (float)(r_main.viewz >> DoomDef.FRACBITS);
				camAngleZ = (float)(((double)r_main.viewangle / ((double)DoomDef.ANG90 * 4.0))) * MathHelper.TwoPi;
				camAngleX = (float)(((double)r_main.viewanglez / ((double)DoomDef.ANG90 * 4.0))) * MathHelper.TwoPi;
				camAngleX = MathHelper.WrapAngle(camAngleX);
				pos = new Vector3(playerX, playerY, playerZ);
				Vector3 forward = Vector3.UnitX;
				forward = Vector3.Transform(forward, Matrix.CreateRotationY(-camAngleX));
				forward = Vector3.Transform(forward, Matrix.CreateRotationZ(camAngleZ));
				viewDir = new Vector2(forward.X, forward.Y);
				Matrix viewWorld = Matrix.CreateWorld(pos, forward, Vector3.UnitZ);
				Game1.instance.camRight = viewWorld.Right;
				Game1.instance.camPos = pos;
				Game1.instance.view = Matrix.CreateLookAt(
					pos,
					pos + forward,
					Vector3.UnitZ);
				frustum.Matrix = Game1.instance.view * Game1.instance.proj;
			}
			else
			{
				pos = Game1.instance.freeCam.Translation;
				r_main.viewx = (int)(pos.X * (float)DoomDef.FRACUNIT);
				r_main.viewy = (int)(pos.Y * (float)DoomDef.FRACUNIT);
				r_main.viewz = (int)(pos.Z * (float)DoomDef.FRACUNIT);
				camAngleZ = Game1.instance.angleZ;
				camAngleX = Game1.instance.angleX;
				Matrix viewWorld = Matrix.CreateWorld(Game1.instance.freeCam.Translation, Game1.instance.freeCam.Up, Vector3.UnitZ);
				Game1.instance.camRight = viewWorld.Right;
				Game1.instance.camPos = Game1.instance.freeCam.Translation;
				Game1.instance.view = Matrix.CreateLookAt(
					Game1.instance.freeCam.Translation,
					Game1.instance.freeCam.Translation + Game1.instance.freeCam.Up,
					Vector3.UnitZ);
			}

			Game1.instance.cameraAngles = new Vector2(camAngleZ, camAngleX);
		}

#if DEBUG
		public static void displayDebug()
		{
			Game1.instance.spriteBatch.Begin();
			Game1.instance.spriteBatch.DrawString(Game1.instance.font, "sectorChecked: " + sectorChecked, Vector2.Zero, Color.Yellow);
			Game1.instance.spriteBatch.DrawString(Game1.instance.font, "portalChecked: " + portalChecked, new Vector2(0, 20), Color.Yellow);
			Game1.instance.spriteBatch.DrawString(Game1.instance.font, "portalTraversed: " + portalTraversed, new Vector2(0, 40), Color.Yellow);
			Game1.instance.spriteBatch.DrawString(Game1.instance.font, "cullingTime: " + cullingTime, new Vector2(0, 60), Color.Yellow);
			Game1.instance.spriteBatch.DrawString(Game1.instance.font, "sectorCount: " + sectorCount, new Vector2(0, 80), Color.Yellow);
			Game1.instance.spriteBatch.End();
		}
#endif
	}
}
