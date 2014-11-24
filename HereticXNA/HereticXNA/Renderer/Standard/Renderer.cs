using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA.Standard
{
	public static class Renderer
	{
		private static GraphicsDevice m_device;
		private static VertexBuffer m_spriteVB;

		public static void init(GraphicsDevice in_device)
		{
			m_device = in_device;
			createSpriteVB();
			Standard.Effects.init(m_device);
		}

		private static void createSpriteVB()
		{
			m_spriteVB = new VertexBuffer(
				m_device, typeof(HereticXNA.Deferred.VertexPosition2Texture), 4, BufferUsage.WriteOnly);
			HereticXNA.Deferred.VertexPosition2Texture[] verts = new HereticXNA.Deferred.VertexPosition2Texture[4] {
						new HereticXNA.Deferred.VertexPosition2Texture(
							new Vector2(.5f, 0),
							new Vector2(0, 0)),
						new HereticXNA.Deferred.VertexPosition2Texture(
							new Vector2(.5f, -1),
							new Vector2(0, 1)),
						new HereticXNA.Deferred.VertexPosition2Texture(
							new Vector2(-.5f, 0),
							new Vector2(1, 0)),
						new HereticXNA.Deferred.VertexPosition2Texture(
							new Vector2(-.5f, -1),
							new Vector2(1, 1))
					};
			m_spriteVB.SetData(verts);
		}

		public static void draw()
		{
			// Prepare shaders
			Standard.Effects.prepare();

			// Build visible sectors if they have been invalidated
			for (int i = 0; i < Frame.sectorCount; ++i)
			{
				Frame.sectors[i].prepare();
			}

			// Render everything opaque
			m_device.BlendState = BlendState.Opaque;

			// Prepare ambient map
			if (Settings.Default.ambient_enabled)
				AmbientMap.prepare();

			// Set render states on the device
			m_device.DepthStencilState = DepthStencilState.Default;
			if (Game1.instance.wireframe)
				m_device.RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
			else
				m_device.RasterizerState = RasterizerState.CullCounterClockwise;

			// Draw the walls first
			drawWalls();

			// Draw the floors and ceilings
			drawFlats();

			// We have nothing to gain in culling sprites. We will use this to swap them!
			m_device.RasterizerState = RasterizerState.CullNone;

			// Draw the sprites
			drawSprites();

			// Draw gun
			drawGun();
		}

		private static void drawWalls()
		{
			Texture2D animatedTex1;
			Texture2D animatedTex2;
			float animatedDelta;
			bool first;
			int i;
			r_local.sector_t sector;

			// Render non-animated walls first
			for (i = 0; i < Frame.sectorCount; ++i)
			{
				sector = Frame.sectors[i];

				first = true;
				foreach (r_local.SectorBatch batch in sector.sectorBatches)
				{
					if (batch.isAnimated()) continue; // We will render animated after

					if (first)
					{
						Effects.fxWall_uniform_lightLevel.SetValue((float)sector.lightlevel / 255.0f);
						first = false;
					}
					Effects.fxWall_uniform_DiffuseTexture.SetValue(batch.texture);
					Effects.fxWallPass.Apply();

					m_device.SetVertexBuffer(batch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, batch.triCount);
				}
			}

			// Render animated walls
			for (i = 0; i < Frame.sectorCount; ++i)
			{
				sector = Frame.sectors[i];

				first = true;
				foreach (r_local.SectorBatch batch in sector.sectorBatches)
				{
					if (!batch.isAnimated()) continue;

					if (first)
					{
						Effects.fxAnimatedWall_uniform_lightLevel.SetValue((float)sector.lightlevel / 255.0f);
						first = false;
					}
					animatedTex1 = Game1.instance.wallTexturesById[r_data.texturetranslationPrev[batch.textureId]];
					animatedTex2 = Game1.instance.wallTexturesById[r_data.texturetranslation[batch.textureId]];
					animatedDelta = r_data.texturetranslationDeltas[batch.textureId];

					Effects.fxAnimatedWall_uniform_DiffuseTexture1.SetValue(animatedTex1);
					Effects.fxAnimatedWall_uniform_DiffuseTexture2.SetValue(animatedTex2);
					Effects.fxAnimatedWall_uniform_animDelta.SetValue(animatedDelta);
					Effects.fxAnimatedWallPass.Apply();

					m_device.SetVertexBuffer(batch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, batch.triCount);
				}
			}
		}

		private static void drawFlats()
		{
			Texture2D animatedTex1;
			Texture2D animatedTex2;
			float animatedDelta;
			bool lightLevelSet;
			int i;
			r_local.sector_t sector;

			// Render non-animated floors and non-sky ceilings first
			for (i = 0; i < Frame.sectorCount; ++i)
			{
				sector = Frame.sectors[i];

				lightLevelSet = false;
				if (!sector.floorBatch.isAnimatedFloor()) // We will render animated after with a different shader
				{
					lightLevelSet = true;
					Effects.fxPlane_uniform_lightLevel.SetValue((float)sector.lightlevel / 255.0f);
					Effects.fxPlane_uniform_DiffuseTexture.SetValue(sector.floorBatch.texture);
					Effects.fxPlanePass.Apply();

					m_device.SetVertexBuffer(sector.floorBatch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.floorBatch.triCount);
				}

				if (!sector.ceilingBatch.isSky) // We render skies at the end, with a different shader
				{
					if (!lightLevelSet)
					{
						Effects.fxPlane_uniform_lightLevel.SetValue((float)sector.lightlevel / 255.0f);
					}
					Effects.fxPlane_uniform_DiffuseTexture.SetValue(sector.ceilingBatch.texture);
					Effects.fxPlanePass.Apply();

					m_device.SetVertexBuffer(sector.ceilingBatch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.ceilingBatch.triCount);
				}
			}

			// Render animated floors
			for (i = 0; i < Frame.sectorCount; ++i)
			{
				sector = Frame.sectors[i];

				if (!sector.floorBatch.isAnimatedFloor()) continue;

				animatedTex1 = Game1.instance.floorTexturesById[r_data.flattranslationPrev[sector.floorBatch.textureId]];
				animatedTex2 = Game1.instance.floorTexturesById[r_data.flattranslation[sector.floorBatch.textureId]];
				animatedDelta = r_data.flattranslationDeltas[sector.floorBatch.textureId];

				Effects.fxAnimatedFloor_uniform_lightLevel.SetValue((float)sector.lightlevel / 255.0f);
				Effects.fxAnimatedFloor_uniform_DiffuseTexture1.SetValue(animatedTex1);
				Effects.fxAnimatedFloor_uniform_DiffuseTexture2.SetValue(animatedTex2);
				Effects.fxAnimatedFloor_uniform_animDelta.SetValue(animatedDelta);
				Effects.fxAnimatedFloorPass.Apply();

				m_device.SetVertexBuffer(sector.floorBatch.vb);
				m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.floorBatch.triCount);
			}

			// Render sky
			Effects.fxSkyPass.Apply();
			for (i = 0; i < Frame.sectorCount; ++i)
			{
				sector = Frame.sectors[i];

				if (!sector.ceilingBatch.isSky) continue;

				m_device.SetVertexBuffer(sector.ceilingBatch.vb);
				m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.ceilingBatch.triCount);
			}
		}

		public static void drawSprites()
		{
			int lump;
			uint rot;
			uint ang;
			bool flip;
			int thingZ;
			r_local.spritedef_t sprdef;
			r_local.spriteframe_t sprframe;
			DoomDef.mobj_t thing = null;
			Vector4 spriteSize2spritePosXY, camRight2SpritePosZLightLevel1;
			int i;
			r_local.sector_t sector;

			m_device.SetVertexBuffer(m_spriteVB);

			for (i = 0; i < Frame.sectorCount; ++i)
			{
				sector = Frame.sectors[i];

				camRight2SpritePosZLightLevel1.W = (float)sector.lightlevel / 255.0f;

				for (thing = sector.thinglist; thing != null; thing = thing.snext)
				{
					// decide which patch to use for sprite reletive to player
					sprdef = r_thing.sprites[(int)thing.sprite];
					sprframe = sprdef.spriteframes[thing.frame & DoomDef.FF_FRAMEMASK];

					if (sprframe.rotate == 1)
					{	// choose a different rotation based on player view
						ang = r_main.R_PointToAngle(thing.x, thing.y);
						rot = (ang - thing.angle + (uint)(DoomDef.ANG45 / 2) * 9) >> 29;
						lump = sprframe.lump[rot];
						flip = sprframe.flip[rot] == 0 ? false : true;
					}
					else
					{	// use single rotation for all views
						lump = sprframe.lump[0];
						flip = sprframe.flip[0] == 0 ? false : true;
					}

					Texture2D texture = w_wad.W_CacheLumpNum(lump + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Texture2D;

					spriteSize2spritePosXY.X = texture.Width;
					spriteSize2spritePosXY.Y = texture.Height;
					spriteSize2spritePosXY.Z = thing.x >> DoomDef.FRACBITS;
					spriteSize2spritePosXY.W = thing.y >> DoomDef.FRACBITS;

					camRight2SpritePosZLightLevel1.X = Game1.instance.camRight.X;
					camRight2SpritePosZLightLevel1.Y = Game1.instance.camRight.Y;
					if (!flip)
					{
						camRight2SpritePosZLightLevel1.X = -camRight2SpritePosZLightLevel1.X;
						camRight2SpritePosZLightLevel1.Y = -camRight2SpritePosZLightLevel1.Y;
					}
					thingZ = thing.z + r_data.spritetopoffset[lump];
					camRight2SpritePosZLightLevel1.Z = (float)(thingZ >> DoomDef.FRACBITS) + 5;

					Effects.fxSprite_uniform_spriteSize2spritePosXY.SetValue(spriteSize2spritePosXY);
					Effects.fxSprite_uniform_camRight2SpritePosZLightLevel1.SetValue(camRight2SpritePosZLightLevel1);
					Effects.fxSprite_uniform_SpriteTexture.SetValue(texture);
					Effects.fxSpritePass.Apply();
					m_device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
				}
			}
		}

		private static void drawGun()
		{
			if (Game1.instance.useFreeCam) return; // No player gun in free cam

			r_thing.R_DrawPlayerSprites();
		}
	}
}
