using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA
{
	public static class AmbientMap
	{
		private const float DETAIL = 8;

		private static GraphicsDevice m_device;
		private static bool m_textureCleared = false;
		private static Vector4 m_limits = Vector4.Zero;
		private static Vector2 m_zlimits = Vector2.Zero;
		private static Point m_texSize = Point.Zero;
		private static Color m_clearColor;

		public static RenderTarget2D ambientTexture = null;

		public static void init(GraphicsDevice in_device)
		{
			m_device = in_device;
		}

		public static void updateSettings()
		{
			if (ambientTexture != null) ambientTexture.Dispose();
			ambientTexture = null;
			if (Settings.Default.ambient_enabled)
			{
				ambientTexture = new RenderTarget2D(m_device, m_texSize.X, m_texSize.Y, false, SurfaceFormat.Rg32, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				m_textureCleared = false;
				Standard.Effects.fxPlane_uniform_AmbientTexture.SetValue(ambientTexture);
				Deferred.Effects.fxPlane_uniform_AmbientTexture.SetValue(ambientTexture);
				Deferred.Effects.fxAnimatedFloor_uniform_AmbientTexture.SetValue(ambientTexture);
			}
		}

		// This is called after a level is loaded.
		public static void prepareLevel() 
		{
			// Calculate the bouncing box of the level
			float bbminX = 100000;
			float bbminY = 100000;
			float bbminZ = 100000;
			float bbmaxX = -100000;
			float bbmaxY = -100000;
			float bbmaxZ = -100000;
			float x, y, z;

			foreach (r_local.vertex_t vert in p_setup.vertexes)
			{
				x = vert.x >> DoomDef.FRACBITS;
				y = vert.y >> DoomDef.FRACBITS;

				bbminX = Math.Min(x, bbminX);
				bbminY = Math.Min(y, bbminY);
				bbmaxX = Math.Max(x, bbmaxX);
				bbmaxY = Math.Max(y, bbmaxY);
			}

			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				z = sector.floorheight >> DoomDef.FRACBITS;
				bbminZ = Math.Min(z, bbminZ);
				z = sector.ceilingheight >> DoomDef.FRACBITS;
				bbmaxZ = Math.Max(z, bbmaxZ);
			}

			m_texSize.X = Math.Max(1, (int)Math.Ceiling((bbmaxX - bbminX) / DETAIL));
			m_texSize.Y = Math.Max(1, (int)Math.Ceiling((bbmaxY - bbminY) / DETAIL));
			int pow2 = 1; while (pow2 < m_texSize.X) pow2 *= 2; m_texSize.X = pow2;
			pow2 = 1; while (pow2 < m_texSize.Y) pow2 *= 2; m_texSize.Y = pow2;
			m_texSize.X = Math.Min(m_texSize.X, 2048);
			m_texSize.Y = Math.Min(m_texSize.Y, 2048);

			// Center it
			m_limits.X = ((bbmaxX + bbminX) - ((float)m_texSize.X * DETAIL)) * .5f;
			m_limits.Y = ((bbmaxY + bbminY) - ((float)m_texSize.Y * DETAIL)) * .5f;
			m_limits.Z = m_limits.X + (float)m_texSize.X * DETAIL;
			m_limits.W = m_limits.Y + (float)m_texSize.Y * DETAIL;

			// Apply some ambient attributes to some shaders, since they will never change again
			Matrix ambientMapProj = Matrix.CreateOrthographicOffCenter(
				m_limits.X, m_limits.Z, m_limits.Y, m_limits.W, -999, 999);
			Vector4 ambientLimits = m_limits;
			ambientLimits.Z -= m_limits.X;
			ambientLimits.W -= m_limits.Y;
			ambientLimits.W = -ambientLimits.W; // This will invert the texcoord in Y, we save an inst in the shader
			m_zlimits = new Vector2(bbminZ, bbmaxZ - bbminZ);

			Standard.Effects.fxAmbientMap_uniform_matProjection.SetValue(ambientMapProj);

			Standard.Effects.fxPlane_uniform_ambientLimits.SetValue(ambientLimits);
			Standard.Effects.fxPlane_uniform_ambientLimitsZPixelSize.SetValue(new Vector4(
				m_zlimits.X, m_zlimits.Y, .5f / (float)m_texSize.X, .5f / (float)m_texSize.Y));

			Deferred.Effects.fxPlane_uniform_ambientLimits.SetValue(ambientLimits);
			Deferred.Effects.fxPlane_uniform_ambientLimitsZPixelSize.SetValue(new Vector4(
				m_zlimits.X, m_zlimits.Y, .5f / (float)m_texSize.X, .5f / (float)m_texSize.Y));

			Deferred.Effects.fxAnimatedFloor_uniform_ambientLimits.SetValue(ambientLimits);
			Deferred.Effects.fxAnimatedFloor_uniform_ambientLimitsZPixelSize.SetValue(new Vector4(
				m_zlimits.X, m_zlimits.Y, .5f / (float)m_texSize.X, .5f / (float)m_texSize.Y));

			// Set clear color to match our bounding, to achieve maximum precision when rendering
			m_clearColor = new Color(255, 0, 0, 255);

			updateSettings();
		}

		public static void prepare()
		{
			bool restoreRenderTarget = false;
			bool ambientIsBound = false;
			Vector2 floorCeil;

			if (!m_textureCleared)
			{
				m_textureCleared = true;
				m_device.SetRenderTarget(ambientTexture);
				m_device.Clear(m_clearColor);
				restoreRenderTarget = true;
			}

			// Bake sectors that been updated
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				if (!sector.needUpdateAmbientMap) continue;
				sector.needUpdateAmbientMap = false;
				if (!ambientIsBound)
				{
					ambientIsBound = true;
					restoreRenderTarget = true;
					m_device.SetRenderTarget(ambientTexture);
					m_device.RasterizerState = RasterizerState.CullNone;
					m_device.DepthStencilState = DepthStencilState.None;
				}

				floorCeil.X = ((sector.floorheight >> DoomDef.FRACBITS) - m_zlimits.X) / m_zlimits.Y;
				floorCeil.Y = ((sector.ceilingheight >> DoomDef.FRACBITS) - m_zlimits.X) / m_zlimits.Y;

				Standard.Effects.fxAmbientMap_uniform_floorCeil.SetValue(floorCeil);
				Standard.Effects.fxAmbientMapPass.Apply();

				m_device.SetVertexBuffer(sector.floorBatch.vb);
				m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.floorBatch.triCount);
			}

			if (restoreRenderTarget)
			{
				m_device.SetRenderTarget(null);
			}
		}
	}
}
