using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA.Deferred
{
	public struct VertexPosition2Texture : IVertexType
	{
		public Vector2 Position;
		public Vector2 TextureCoordinate;

		public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
			new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
		);

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}

		public VertexPosition2Texture(Vector2 position, Vector2 textureCoordinate)
		{
			Position = position;
			TextureCoordinate = textureCoordinate;
		}
	}
}
