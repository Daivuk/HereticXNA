using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA.Deferred
{
	static class GBuffer
	{
#if DEBUG
		static SpriteBatch m_spriteBatch;
#endif
		static GraphicsDevice m_device;
		static RenderTarget2D m_gBufferDiffuse;
		static RenderTarget2D m_gBufferDepth;
		static RenderTarget2D m_gBufferNormal;
		static RenderTargetBinding[] m_gBufferBindings = new RenderTargetBinding[3];

		static public Texture2D ColorTexture { get { return m_gBufferDiffuse; } }
		static public Texture2D DepthTexture { get { return m_gBufferDepth; } }
		static public Texture2D NormalTexture { get { return m_gBufferNormal; } }

		public static void init(GraphicsDevice in_device)
		{
			m_device = in_device;
#if DEBUG
			m_spriteBatch = new SpriteBatch(m_device);
#endif
			updateSettings();
		}

#if DEBUG
		public static void displayDebug()
		{
			m_spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.Opaque,
				SamplerState.PointClamp,
				DepthStencilState.None,
				RasterizerState.CullNone);

			m_spriteBatch.Draw(
				m_gBufferDiffuse,
				new Rectangle(0, 0, m_device.Viewport.Width / 4, m_device.Viewport.Height / 4),
				Color.White);
			m_spriteBatch.Draw(
				m_gBufferDepth,
				new Rectangle(m_device.Viewport.Width / 4, 0, m_device.Viewport.Width / 4, m_device.Viewport.Height / 4),
				Color.White);
			m_spriteBatch.Draw(
				m_gBufferNormal,
				new Rectangle(0, m_device.Viewport.Height / 4, m_device.Viewport.Width / 4, m_device.Viewport.Height / 4),
				Color.White);

			m_spriteBatch.End();
		}
#endif

		public static void prepare()
		{
			m_device.SetRenderTargets(m_gBufferBindings);
			m_device.Clear(Color.Transparent);
		}

		public static void updateSettings()
		{
			if (m_gBufferDiffuse != null)
			{
				m_gBufferDiffuse.Dispose();
				m_gBufferDepth.Dispose();
				m_gBufferNormal.Dispose();
			}

			// Color buffer
			m_gBufferDiffuse = new RenderTarget2D(m_device, m_device.Viewport.Width, m_device.Viewport.Height,
				false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
			// Depth buffer
			m_gBufferDepth = new RenderTarget2D(m_device, m_device.Viewport.Width, m_device.Viewport.Height,
				false, SurfaceFormat.Single, DepthFormat.None);
			// Normal buffer
			m_gBufferNormal = new RenderTarget2D(m_device, m_device.Viewport.Width, m_device.Viewport.Height,
				false, SurfaceFormat.Color, DepthFormat.None);

			m_gBufferBindings[0] = new RenderTargetBinding(m_gBufferDiffuse);
			m_gBufferBindings[1] = new RenderTargetBinding(m_gBufferDepth);
			m_gBufferBindings[2] = new RenderTargetBinding(m_gBufferNormal);
		}
	}
}
