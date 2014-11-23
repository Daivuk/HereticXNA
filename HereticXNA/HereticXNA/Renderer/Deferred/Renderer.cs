using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA.Deferred
{
	public static class Renderer
	{
		private static GraphicsDevice m_device;
		private static VertexBuffer m_spriteVB;

		public static void init(GraphicsDevice in_device)
		{
			m_device = in_device;
			createSpriteVB();

			GBuffer.init(m_device);
			FullscreenQuad.init(m_device);
			Effects.init(m_device);
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
			Deferred.Effects.prepare();

			// Build visible sectors if they have been invalidated
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				sector.prepare();
			}

			// Render everything opaque
			m_device.BlendState = BlendState.Opaque;

			// Prepare ambient map, and render invalidated sectors to it
			if (Settings.Default.ambient_enabled)
				AmbientMap.prepare();

			// Set render states on the device
			m_device.DepthStencilState = DepthStencilState.Default;
			if (Game1.instance.wireframe)
				m_device.RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
			else
				m_device.RasterizerState = RasterizerState.CullCounterClockwise;

			// Bind and clear the gbuffer
			Deferred.GBuffer.prepare();

			// Draw the walls first
			drawWalls();

			// Draw the floors and ceilings
			drawFlats();

			// Post process
			if (Settings.Default.postprocess_enabled)
			{
				// Bind frame buffer used for post process
			}
			else
			{
				// We will render on the main frame buffer
				m_device.SetRenderTargets(null);
			}

			// Ambient pass
			drawAmbient();

			// Apply post process effects
			if (Settings.Default.postprocess_enabled)
			{
			}
		}

		private static void drawAmbient()
		{
			Effects.fxAmbientPass.Apply();

			FullscreenQuad.prepareDraw();
			FullscreenQuad.draw();
		}

		private static void drawWalls()
		{
			Texture2D animatedTex1;
			Texture2D animatedTex2;
			float animatedDelta;

			// Render non-animated walls first
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				foreach (r_local.SectorBatch batch in sector.sectorBatches)
				{
					if (batch.isAnimated()) continue; // We will render animated after
					Effects.fxWall_uniform_DiffuseTexture.SetValue(batch.texture);
					Effects.fxWallPass.Apply();

					m_device.SetVertexBuffer(batch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, batch.triCount);
				}
			}

			// Render animated walls
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				foreach (r_local.SectorBatch batch in sector.sectorBatches)
				{
					if (!batch.isAnimated()) continue;

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

			// Render non-animated floors and non-sky ceilings first
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				if (!sector.floorBatch.isAnimatedFloor()) // We will render animated after with a different shader
				{
					Effects.fxPlane_uniform_DiffuseTexture.SetValue(sector.floorBatch.texture);
					Effects.fxPlanePass.Apply();

					m_device.SetVertexBuffer(sector.floorBatch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.floorBatch.triCount);
				}

				if (!sector.ceilingBatch.isSky) // We render skies at the end, with a different shader
				{
					if (sector.ceilingBatch.texture != sector.floorBatch.texture)
					{
						Effects.fxPlane_uniform_DiffuseTexture.SetValue(sector.ceilingBatch.texture);
						Effects.fxPlanePass.Apply();
					}

					m_device.SetVertexBuffer(sector.ceilingBatch.vb);
					m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.ceilingBatch.triCount);
				}
			}

			// Render animated floors
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				if (!sector.floorBatch.isAnimatedFloor()) continue;

				animatedTex1 = Game1.instance.floorTexturesById[r_data.flattranslationPrev[sector.floorBatch.textureId]];
				animatedTex2 = Game1.instance.floorTexturesById[r_data.flattranslation[sector.floorBatch.textureId]];
				animatedDelta = r_data.flattranslationDeltas[sector.floorBatch.textureId];

				Effects.fxAnimatedFloor_uniform_DiffuseTexture1.SetValue(animatedTex1);
				Effects.fxAnimatedFloor_uniform_DiffuseTexture2.SetValue(animatedTex2);
				Effects.fxAnimatedFloor_uniform_animDelta.SetValue(animatedDelta);
				Effects.fxAnimatedFloorPass.Apply();

				m_device.SetVertexBuffer(sector.floorBatch.vb);
				m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.floorBatch.triCount);
			}

			// Render sky
			Effects.fxSkyPass.Apply();
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				if (!sector.ceilingBatch.isSky) continue;

				m_device.SetVertexBuffer(sector.ceilingBatch.vb);
				m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.ceilingBatch.triCount);
			}
		}
	}
}
