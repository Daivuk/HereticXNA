using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA.Deferred
{
	static class FullscreenQuad
	{
		static VertexBuffer m_vb;
		static GraphicsDevice m_device;

		public static void init(GraphicsDevice in_device)
		{
			m_device = in_device;

			updateSettings();
		}

		public static void updateSettings()
		{
			if (m_vb != null) m_vb.Dispose();

			Vector2 halfPixels = new Vector2(
				.5f / (float)Settings.Default.resolution.X,
				.5f / (float)Settings.Default.resolution.Y);

			VertexPosition2Texture[] verts = new VertexPosition2Texture[4]
			{
				new VertexPosition2Texture(new Vector2(-1, -1), new Vector2(-halfPixels.X, 1-halfPixels.Y)),
				new VertexPosition2Texture(new Vector2(-1, 1), new Vector2(-halfPixels.X, -halfPixels.Y)),
				new VertexPosition2Texture(new Vector2(1, -1), new Vector2(1-halfPixels.X, 1-halfPixels.Y)),
				new VertexPosition2Texture(new Vector2(1, 1), new Vector2(1-halfPixels.X, -halfPixels.Y))
			};
			m_vb = new VertexBuffer(m_device, typeof(VertexPosition2Texture), 4, BufferUsage.WriteOnly);
			m_vb.SetData(verts);
		}

		public static void prepareDraw()
		{
			m_device.SetVertexBuffer(m_vb);
		}

		public static void draw()
		{
			m_device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		}
	}
}
