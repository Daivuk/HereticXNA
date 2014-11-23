using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

		// R_local.h

namespace HereticXNA
{
	public static class r_local
	{
		public const int ANGLETOSKYSHIFT = 22;	// sky map is 256*128*4 maps

public const int 	BASEYCENTER			=100;
	
public const int  MAXWIDTH		=	1120;
public const int 	MAXHEIGHT		=	832;

public const double 	PI				=	3.141592657;

public static int 	CENTERY			=	(DoomDef.SCREENHEIGHT/2);

public const int 	MINZ			=(DoomDef.FRACUNIT*4);

public const int 	FIELDOFVIEW		=2048;	// fineangles in the SCREENWIDTH wide window
//
// lighting constants
//
		public const int LIGHTLEVELS = 16;
public const int 	LIGHTSEGSHIFT	=	4;
public const int 	MAXLIGHTSCALE	=	48;
public const int 	LIGHTSCALESHIFT	=	12;
public const int 	MAXLIGHTZ		=	128;
public const int 	LIGHTZSHIFT		=	20;
public const int 	NUMCOLORMAPS	=	32;		// number of diminishing
public const int 	INVERSECOLORMAP	=	32;


/*
==============================================================================

					INTERNAL MAP TYPES

==============================================================================
*/

//================ used by play and refresh

public class vertex_t
{
	public int x,y;
	public int groupTag; // [dsl] We use that for ambient
	public int id; // [dsl] Also use to determine if a midpoint is part of it or not (For ambient)
	public int nextId;
	public line_t line;
} ;

		public struct VertexPositionNormalColorTexture : IVertexType
		{
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 TextureCoordinate;
			public Color Color;

			public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
			(
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
				new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
				new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0)
			);

			VertexDeclaration IVertexType.VertexDeclaration
			{
				get { return VertexDeclaration; }
			}

			public VertexPositionNormalColorTexture(Vector3 position, Vector3 normal, Color color, Vector2 textureCoordinate)
			{
				Position = position;
				Normal = normal;
				Color = color;
				TextureCoordinate = textureCoordinate;
			}
		}

		public class SectorBatch
		{
			public Texture2D texture;
			public int textureId;
			public VertexBuffer vb;
			public int triCount;
			public List<VertexPositionNormalColorTexture> vertsPNCT = new List<VertexPositionNormalColorTexture>();
			public List<VertexPositionNormalTexture> vertsPNT = new List<VertexPositionNormalTexture>();
			public List<VertexPositionColorTexture> vertsPCT = new List<VertexPositionColorTexture>();
			public List<VertexPositionTexture> vertsPT = new List<VertexPositionTexture>();
			public bool isSky = false;
			public bool isUVAnimated;

			public void Dispose()
			{
				if (vb != null) vb.Dispose();
			}

			public bool isAnimated()
			{
				int prevSource = r_data.texturetranslationPrev[textureId];
				int tempSource = r_data.texturetranslation[textureId];
				return prevSource != tempSource;
			}

			public bool isAnimatedFloor()
			{
				int prevSource = r_data.flattranslationPrev[textureId];
				int tempSource = r_data.flattranslation[textureId];
				return prevSource != tempSource;
			}

			public void createVB()
			{
				if (vertsPNCT.Count != 0)
				{
					triCount = vertsPNCT.Count / 3;
					vb = new VertexBuffer(Game1.instance.GraphicsDevice, typeof(VertexPositionNormalColorTexture),
						vertsPNCT.Count, BufferUsage.WriteOnly);
					vb.SetData(vertsPNCT.ToArray());
#if !DEBUG
					vertsPNCT.Clear();
#endif
				}
				else if (vertsPNT.Count != 0)
				{
					if (vertsPNT.Count == 0) return;
					triCount = vertsPNT.Count / 3;
					vb = new VertexBuffer(Game1.instance.GraphicsDevice, typeof(VertexPositionNormalTexture),
						vertsPNT.Count, BufferUsage.WriteOnly);
					vb.SetData(vertsPNT.ToArray());
#if !DEBUG
					batch.verts.Clear();
#endif
				}
				else if (vertsPCT.Count != 0)
				{
					if (vertsPCT.Count == 0) return;
					triCount = vertsPCT.Count / 3;
					vb = new VertexBuffer(Game1.instance.GraphicsDevice, typeof(VertexPositionColorTexture),
						vertsPCT.Count, BufferUsage.WriteOnly);
					vb.SetData(vertsPCT.ToArray());
#if !DEBUG
					batch.verts.Clear();
#endif
				}
				else if (vertsPT.Count != 0)
				{
					if (vertsPT.Count == 0) return;
					triCount = vertsPT.Count / 3;
					vb = new VertexBuffer(Game1.instance.GraphicsDevice, typeof(VertexPositionTexture),
						vertsPT.Count, BufferUsage.WriteOnly);
					vb.SetData(vertsPT.ToArray());
#if !DEBUG
					vertsPT.Clear();
#endif
				}
			}

			public void boundingBox(ref float bbminX, ref float bbminY, ref float bbmaxX, ref float bbmaxY)
			{
				if (vertsPNCT.Count != 0)
				{
					for (int i = 0; i < vertsPNCT.Count; ++i)
					{
						Vector3 pos = vertsPNCT[i].Position;
						bbminX = Math.Min(pos.X, bbminX);
						bbminY = Math.Min(pos.Y, bbminY);
						bbmaxX = Math.Max(pos.X, bbmaxX);
						bbmaxY = Math.Max(pos.Y, bbmaxY);
					}
				}
				else if (vertsPNT.Count != 0)
				{
					for (int i = 0; i < vertsPNT.Count; ++i)
					{
						Vector3 pos = vertsPNT[i].Position;
						bbminX = Math.Min(pos.X, bbminX);
						bbminY = Math.Min(pos.Y, bbminY);
						bbmaxX = Math.Max(pos.X, bbmaxX);
						bbmaxY = Math.Max(pos.Y, bbmaxY);
					}
				}
				else if (vertsPCT.Count != 0)
				{
					for (int i = 0; i < vertsPCT.Count; ++i)
					{
						Vector3 pos = vertsPCT[i].Position;
						bbminX = Math.Min(pos.X, bbminX);
						bbminY = Math.Min(pos.Y, bbminY);
						bbmaxX = Math.Max(pos.X, bbmaxX);
						bbmaxY = Math.Max(pos.Y, bbmaxY);
					}
				}
				else if (vertsPT.Count != 0)
				{
					for (int i = 0; i < vertsPT.Count; ++i)
					{
						Vector3 pos = vertsPT[i].Position;
						bbminX = Math.Min(pos.X, bbminX);
						bbminY = Math.Min(pos.Y, bbminY);
						bbmaxX = Math.Max(pos.X, bbmaxX);
						bbmaxY = Math.Max(pos.Y, bbmaxY);
					}
				}
			}
		};

		public class SectorLightData
		{
			public RenderTarget2D textureSpread = null;
			public sector_t sector;
			public Game1.Floorlightinfo lightInfo = null;
			public const float PRECISION = 16.0f;
			public Vector4 limits = Vector4.Zero;

			public SectorLightData(sector_t in_sector, Game1.Floorlightinfo in_lightInfo)
			{
				sector = in_sector;
				lightInfo = in_lightInfo;
				lightInfo.makeColor();

				// Generate the stuff
				float bbminX = 100000;
				float bbminY = 100000;
				float bbmaxX = -100000;
				float bbmaxY = -100000;

				sector.floorBatch.boundingBox(ref bbminX, ref bbminY, ref bbmaxX, ref bbmaxY);

				int texW = Math.Max(1, (int)Math.Ceiling(((bbmaxX - bbminX) / PRECISION) + lightInfo.spread * 16 + 4));
				int texH = Math.Max(1, (int)Math.Ceiling(((bbmaxY - bbminY) / PRECISION) + lightInfo.spread * 16 + 4));
				int pow2 = 1; while (pow2 < texW) pow2 *= 2; texW = pow2;
				pow2 = 1; while (pow2 < texH) pow2 *= 2; texH = pow2;

				// Center it
				limits.X = ((bbmaxX + bbminX) - ((float)texW * PRECISION)) * .5f;
				limits.Y = ((bbmaxY + bbminY) - ((float)texH * PRECISION)) * .5f;
				limits.Z = limits.X + (float)texW * PRECISION;
				limits.W = limits.Y + (float)texH * PRECISION;

				textureSpread = new RenderTarget2D(Game1.instance.GraphicsDevice, texW, texH);
				RenderTarget2D textureTemp = new RenderTarget2D(Game1.instance.GraphicsDevice, texW, texH);
				Game1.instance.fxSectorLightInfo.Parameters["texSize"].SetValue(new Vector2((float)texW, (float)texH));
				Game1.instance.fxSectorLightInfo.Parameters["spread"].SetValue(lightInfo.spread);

				// Render to target the sector
				Game1.instance.GraphicsDevice.SetRenderTarget(textureSpread);
				Game1.instance.GraphicsDevice.Clear(Color.Transparent);
				Game1.instance.fxSectorLightInfo.CurrentTechnique = Game1.instance.fxSectorLightInfo.Techniques["TechniquePlain"];
				Game1.instance.fxSectorLightInfo.Parameters["World"].SetValue(Matrix.Identity);
				Game1.instance.fxSectorLightInfo.Parameters["View"].SetValue(Matrix.Identity);
				Game1.instance.fxSectorLightInfo.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(limits.X, limits.Z, limits.Y, limits.W, -999, 999));
				Game1.instance.fxSectorLightInfo.CurrentTechnique.Passes[0].Apply();
				Game1.instance.GraphicsDevice.SetVertexBuffer(sector.floorBatch.vb);
				Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, sector.floorBatch.triCount);
				Game1.instance.GraphicsDevice.SetRenderTargets(null);

				// Grow the texels for the base, and start bluring for the spread
				Game1.instance.GraphicsDevice.SetRenderTarget(textureTemp);
				Game1.instance.GraphicsDevice.Clear(Color.Transparent);
				Game1.instance.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
					Game1.instance.fxSectorLightInfo);
				Game1.instance.fxSectorLightInfo.CurrentTechnique = Game1.instance.fxSectorLightInfo.Techniques["TechniqueBlurU"];
				Game1.instance.spriteBatch.Draw(textureSpread, new Rectangle(0, 0, textureSpread.Width, textureSpread.Height), Color.White);
				Game1.instance.spriteBatch.End();
				Game1.instance.GraphicsDevice.SetRenderTargets(null);

				// Lastly blur in V to finalize it
				Game1.instance.GraphicsDevice.SetRenderTarget(textureSpread);
				Game1.instance.GraphicsDevice.Clear(Color.Transparent);
				Game1.instance.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
					Game1.instance.fxSectorLightInfo);
				Game1.instance.fxSectorLightInfo.CurrentTechnique = Game1.instance.fxSectorLightInfo.Techniques["TechniqueBlurV"];
				Game1.instance.spriteBatch.Draw(textureTemp, new Rectangle(0, 0, textureTemp.Width, textureTemp.Height), Color.White);
				Game1.instance.spriteBatch.End();
				Game1.instance.GraphicsDevice.SetRenderTargets(null);

				textureTemp.Dispose();
			}

			public void Dispose()
			{
				sector = null;
				lightInfo = null;
				if (textureSpread != null) textureSpread.Dispose();
			}
		}

public class sector_t
{
	public SectorLightData lightData = null;
	public SectorLightData lightDataC = null;
	public void Dispose()
	{
		if (lightData != null) lightData.Dispose();
		if (lightDataC != null) lightDataC.Dispose();
		if (sectorBatches != null)
		{
			foreach (SectorBatch batch in sectorBatches)
			{
				batch.Dispose();
			}
		}
		if (floorBatch != null) floorBatch.Dispose();
		if (ceilingBatch != null) ceilingBatch.Dispose();
	}
	int		m_floorheight, m_ceilingheight;
	public int floorheight
	{
		get
		{
			return m_floorheight;
		}
		set
		{
			if (value != m_floorheight)
			{
				m_floorheight = value;
				invalidate();
			}
		}
	}
	public int ceilingheight
	{
		get
		{
			return m_ceilingheight;
		}
		set
		{
			if (value != m_ceilingheight)
			{
				m_ceilingheight = value;
				invalidate();
			}
		}
	}
	public short floorpic, ceilingpic;
	public short lightlevel;
	public short special, tag;

	public int soundtraversed;		// 0 = untraversed, 1,2 = sndlines -1
	public DoomDef.mobj_t soundtarget;		// thing that made a sound (or null)

	public int[] blockbox = new int[4];		// mapblock bounding box for height changes
	public DoomDef.degenmobj_t soundorg = new DoomDef.degenmobj_t();			// for any sounds played by the sector

	public int validcount;			// if == validcount, already checked
	public DoomDef.mobj_t thinglist;			// list of mobjs in sector
	public object specialdata;		// thinker_t for reversable actions
	public int linecount;
	public int linesi;			// [linecount] size

	// [dsl] Added for XNA rendering
	public List<List<r_local.vertex_t>> vertGroups;
	public List<r_data.Vector2AndTags> triangleList;
	public List<int> ambientTagList;
	public List<r_local.line_t> linesTemp;
	public sector_t parent;

	// Render batches
	public List<SectorBatch> sectorBatches = null;
	public SectorBatch floorBatch = null;
	public SectorBatch ceilingBatch = null;
	public List<seg_t> segs = null;

	public bool needUpdateAmbientMap = false;
	public bool justInvalidated = false;
	public int checkId;
	public int addedId;
	public void invalidate() { invalidate(true); }
	public void invalidate(bool doNeighbors)
	{
		needUpdateAmbientMap = true;
		if (floorBatch != null)
		{
			floorBatch.vb.Dispose();
			floorBatch = null;
		}
		if (ceilingBatch != null)
		{
			ceilingBatch.vb.Dispose();
			ceilingBatch = null;
		}
		if (sectorBatches != null)
		{
			foreach (SectorBatch batch in sectorBatches)
			{
				if (batch.vb != null)
					batch.vb.Dispose();
			}
			sectorBatches = null;
		}
		// Invalidate also neiborgh sectors
		if (doNeighbors)
		{
			if (segs != null)
			{
				foreach (seg_t seg in segs)
				{
					if (seg.backsector != null)
					{
						seg.backsector.invalidate(false);
					}
				}
			}

			// Check for lights that touch that sector, and invalidate their shadow
			for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
			{
				if (think == null) break;
				if (think.function == null) continue;
				DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
				if (mo == null) continue;
				if (mo.shadowInfo == null) continue;
				if (mo.sectorsInRadius.Contains(this))
					mo.shadowInfo.needUpdate = true;
			}
		}
		justInvalidated = true;
	}

	public void prepare()
	{
		if (sectorBatches == null)
		{
			sectorBatches = new List<SectorBatch>();

			// Build batches
			foreach (seg_t seg in segs)
			{
				r_segs.XNARenderWallToBatches(seg, sectorBatches);
			}
			foreach (SectorBatch batch in sectorBatches)
			{
				if (!batch.isAnimated())
				{
					int tempSource = r_data.texturetranslation[batch.textureId];
					batch.texture = Game1.instance.wallTexturesById[tempSource];
				}
				if (batch.vb == null)
				{
					batch.createVB();
				}
			}
		}
		if (floorBatch == null)
		{
			floorBatch = new SectorBatch();
			r_plane.XNARenderFloorToBatch(this, floorBatch);
			if (floorBatch.vb == null)
			{
				floorBatch.createVB();
			}
		}
		if (ceilingBatch == null)
		{
			ceilingBatch = new SectorBatch();
			r_plane.XNARenderCeilToBatch(this, ceilingBatch);
			if (ceilingBatch.vb == null)
			{
				ceilingBatch.createVB();
			}
		}
	}

	public void draw()
	{
/*		prepare();

		int prevSource;
		int tempSource;
		int textureId;
		Texture2D texture;

		// Draw batches
		float lightLevelf = (float)lightlevel / 255.0f;
		if (Game1.instance.useDeferred) lightLevelf = 1;
		if (justInvalidated)
		{
			if (Game1.instance.showSectorUpdate) lightLevelf = 2;
			justInvalidated = false;
		}
		foreach (SectorBatch batch in sectorBatches)
		{
			if (batch.verts.Count == 0) continue;
			if (batch.vb == null)
			{
				batch.createVB();
			}
			prevSource = r_data.texturetranslationPrev[batch.textureId];
			tempSource = r_data.texturetranslation[batch.textureId];
			textureId = tempSource;
			texture = Game1.instance.wallTexturesById[textureId];
			if (Game1.instance.useDeferred)
			{
				if (textureId >= 81 &&
					textureId <= 83) lightLevelf = 0;
				else if (textureId >= 85 &&
					textureId <= 89) lightLevelf = .3f;
				else lightLevelf = 1;
			}
			if (prevSource != tempSource)
			{
				Texture2D prevTexture = Game1.instance.wallTexturesById[prevSource];
				EffectTechnique prevTechnique = Game1.instance.fxWall.CurrentTechnique;
				Game1.instance.fxWall.CurrentTechnique = Game1.instance.fxWall.Techniques[Game1.instance.techniqueName("TechniqueAnimatedWall")];
				Game1.instance.fxWall.Parameters["AnimatedTexture1"].SetValue(texture);
				Game1.instance.fxWall.Parameters["AnimatedTexture2"].SetValue(prevTexture);
				float delta = r_data.texturetranslationDeltas[batch.textureId];
				Game1.instance.fxWall.Parameters["AnimT"].SetValue(delta);
				Game1.instance.fxWall.Parameters["lightLevel"].SetValue(lightLevelf);
				Game1.instance.fxWall.CurrentTechnique.Passes[0].Apply();
				Game1.instance.fxWall.CurrentTechnique = prevTechnique;
			}
			else
			{
				Game1.instance.fxWall.Parameters["DiffuseTexture"].SetValue(texture);
				Game1.instance.fxWall.Parameters["lightLevel"].SetValue(lightLevelf);
				Game1.instance.fxWall.CurrentTechnique.Passes[0].Apply();
			}
			Game1.instance.GraphicsDevice.SetVertexBuffer(batch.vb);
			Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, batch.triCount);
		}
		if (floorBatch == null)
		{
			floorBatch = new SectorBatch();
			r_plane.XNARenderFloorToBatch(this, floorBatch);
		}
		if (floorBatch.vb == null)
		{
			floorBatch.createVB();
		}
		Vector2 floorCeil;
		floorCeil.X = ((floorheight >> DoomDef.FRACBITS) + 1024.0f) / 2048.0f;
		floorCeil.Y = ((ceilingheight >> DoomDef.FRACBITS) + 1024.0f) / 2048.0f;
		if (floorBatch.isUVAnimated)
		{
			Vector2 uvOffset = Vector2.Zero;
			switch (special)
			{
				case 25:
				case 26:
				case 27:
				case 28:
				case 29: // Scroll_North
					break;
				case 20:
				case 21:
				case 22:
				case 23:
				case 24: // Scroll_East
					uvOffset.X = (float)((63 - ((p_tick.leveltime >> 1) & 63)) << (special - 20) & 63) / 64.0f;
					break;
				case 30:
				case 31:
				case 32:
				case 33:
				case 34: // Scroll_South
					break;
				case 35:
				case 36:
				case 37:
				case 38:
				case 39: // Scroll_West
					break;
				case 4: // Scroll_EastLavaDamage
					uvOffset.X = (float)(((63 - ((p_tick.leveltime >> 1) & 63)) << 3) & 63) / 64.0f;
					break;
				default:
					break;
			}

			Game1.instance.fxPlane.Parameters["uvOffset"].SetValue(uvOffset);
		}
		else
		{
			Game1.instance.fxPlane.Parameters["uvOffset"].SetValue(Vector2.Zero);
		}
		Game1.instance.fxPlane.Parameters["floorCeil"].SetValue(floorCeil);
		Game1.instance.fxPlane.Parameters["lightLevel"].SetValue(lightLevelf);
		prevSource = r_data.flattranslationPrev[floorpic];
		tempSource = r_data.flattranslation[floorpic];
		textureId = tempSource;
		texture = Game1.instance.floorTexturesById[textureId];

		if (Game1.instance.useDeferred)
		{
			if (Game1.instance.flatSelfIllumById.ContainsKey(floorpic)) lightLevelf = 1 - Game1.instance.flatSelfIllumById[floorpic];
			else lightLevelf = 1;
		}

		if (prevSource != tempSource)
		{
			Texture2D prevTexture = Game1.instance.floorTexturesById[prevSource];
			EffectTechnique prevTechnique = Game1.instance.fxPlane.CurrentTechnique;
			Game1.instance.fxPlane.CurrentTechnique = Game1.instance.fxPlane.Techniques[Game1.instance.techniqueName("TechniqueAnimatedFloor")];
			Game1.instance.fxPlane.Parameters["AnimatedTexture1"].SetValue(texture);
			Game1.instance.fxPlane.Parameters["AnimatedTexture2"].SetValue(prevTexture);
			float delta = r_data.flattranslationDeltas[floorpic];
			Game1.instance.fxPlane.Parameters["AnimT"].SetValue(delta);
			Game1.instance.fxPlane.Parameters["lightLevel"].SetValue(lightLevelf);
			Game1.instance.fxPlane.CurrentTechnique.Passes[0].Apply();
			Game1.instance.fxPlane.CurrentTechnique = prevTechnique;
		}
		else
		{
			Game1.instance.fxPlane.Parameters["DiffuseTexture"].SetValue(texture);
			Game1.instance.fxPlane.Parameters["lightLevel"].SetValue(lightLevelf);
			Game1.instance.fxPlane.CurrentTechnique.Passes[0].Apply();
		}
		Game1.instance.GraphicsDevice.SetVertexBuffer(floorBatch.vb);
		Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, floorBatch.triCount);

		if (Game1.instance.useDeferred)
		{
			if (Game1.instance.flatSelfIllumById.ContainsKey(ceilingpic)) lightLevelf = 1 - Game1.instance.flatSelfIllumById[ceilingpic];
			else lightLevelf = 1;
		}
		Game1.instance.fxPlane.Parameters["lightLevel"].SetValue(lightLevelf);

		if (ceilingBatch.vb == null)
		{
			ceilingBatch.createVB();
		}
		if (ceilingBatch.isSky)
		{
			Game1.instance.fxSky.Parameters["DiffuseTexture"].SetValue(ceilingBatch.texture);
			Game1.instance.fxSky.CurrentTechnique.Passes[0].Apply();
		}
		else
		{
			Game1.instance.fxPlane.Parameters["DiffuseTexture"].SetValue(ceilingBatch.texture);
			Game1.instance.fxPlane.Parameters["uvOffset"].SetValue(Vector2.Zero);
			Game1.instance.fxPlane.CurrentTechnique.Passes[0].Apply();
		}
		Game1.instance.GraphicsDevice.SetVertexBuffer(ceilingBatch.vb);
		Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, ceilingBatch.triCount);*/
	}

	internal void drawFlat()
	{
	/*	if (sectorBatches == null)
		{
			sectorBatches = new List<SectorBatch>();

			// Build batches
			foreach (seg_t seg in segs)
			{
				r_segs.XNARenderWallToBatches(seg, sectorBatches);
			}
		}

		// Draw batches
		foreach (SectorBatch batch in sectorBatches)
		{
			if (batch.vb == null)
			{
				batch.createVB();
			}
			Game1.instance.GraphicsDevice.SetVertexBuffer(batch.vb);
			Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, batch.triCount);
		}
		if (floorBatch == null)
		{
			floorBatch = new SectorBatch();
			r_plane.XNARenderFloorToBatch(this, floorBatch);
		}
		if (floorBatch.vb == null)
		{
			floorBatch.createVB();
		}
		Game1.instance.GraphicsDevice.SetVertexBuffer(floorBatch.vb);
		Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, floorBatch.triCount);
		if (ceilingBatch == null)
		{
			ceilingBatch = new SectorBatch();
			r_plane.XNARenderCeilToBatch(this, ceilingBatch);
		}
		if (ceilingBatch.vb == null)
		{
			ceilingBatch.createVB();
		}
		Game1.instance.GraphicsDevice.SetVertexBuffer(ceilingBatch.vb);
		Game1.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, ceilingBatch.triCount);*/
	}
} ;

public class side_t
{
	public int		textureoffset;		// add this to the calculated texture col
	public int rowoffset;			// add this to the calculated texture top
	public short toptexture, bottomtexture, midtexture;
	public sector_t sector;
} ;

public enum slopetype_t {ST_HORIZONTAL, ST_VERTICAL, ST_POSITIVE, ST_NEGATIVE} ;
public class line_t
{
	public vertex_t	v1, v2;
	public int dx, dy;				// v2 - v1 for side checking
	public short flags;
	public short special, tag;
	public short[] sidenum = new short[2];			// sidenum[1] will be -1 if one sided
	public int[] bbox = new int[4];
	public slopetype_t slopetype;			// to aid move clipping
	public sector_t frontsector, backsector;
	public int validcount;			// if == validcount, already checked
	public object specialdata;		// thinker_t for reversable actions
	public int buildGroupId;
	public int checkId;
}



public class subsector_t
{
	public sector_t sector;
	public short numlines;
	public short firstline;
	public List<r_data.Segment> bspSegs;
	public Color debugColor;
} ;

public class seg_t
{
	public vertex_t v1, v2;
	public int offset;
	public uint angle;
	public side_t sidedef;
	public line_t linedef;
	public sector_t frontsector;
	public sector_t backsector;		// NULL for one sided lines
	public List<seg_t> left = new List<seg_t>();
	public List<seg_t> right = new List<seg_t>();
	public Vector2 normal;
	public Vector2 dir;
	public int culling_checkId;
	public int culling_alreadyFullyIncluded;
} ;
	/*	
public class seg_tv
{
	public void set(seg_t other)
	{
		v1.x = other.v1.x;
		v1.y = other.v1.y;
		v2.x = other.v2.x;
		v2.y = other.v2.y;
		offset = other.offset;
		angle = other.angle;
		sidedef = other.sidedef;
		linedef = other.linedef;
		frontsector = other.frontsector;
		backsector = other.backsector;
		left = other.left;
		right = other.right;
		normal = other.normal;
		dir = other.dir;
	}
	public vertex_t v1 = new vertex_t(), v2 = new vertex_t();
	public int offset;
	public uint angle;
	public side_t sidedef;
	public line_t linedef;
	public sector_t frontsector;
	public sector_t backsector;		// NULL for one sided lines
	public seg_t left;
	public seg_t right;
	public Vector2 normal;
	public Vector2 dir;
} ;*/

public class node_t
{
	public int x, y, dx, dy;			// partition line
	public int[][] bbox = new int[2][] { new int[4], new int[4] };			// bounding box for each child
	public ushort[] children = new ushort[2];		// if NF_SUBSECTOR its a subsector
} ;


/*
==============================================================================

						OTHER TYPES

==============================================================================
*/

public const int 	MAXVISPLANES=	128;
public static int 	MAXOPENINGS	=	DoomDef.SCREENWIDTH*64;

public class visplane_t
{
	public int		height;
	public int		picnum;
	public int		lightlevel;
	public int		special;
	public int		minx, maxx;
	public byte		pad1;						// leave pads for [minx-1]/[maxx+1]
//	public byte		top[SCREENWIDTH]; [dsl] No need, no blitting
	public byte		pad2;
	public byte		pad3;
//	public byte		bottom[SCREENWIDTH]; [dsl] No need, no blitting
	public byte		pad4;
	public sector_t sector;
} ;
public class drawseg_t
{
	public seg_t		curline;
	public int			x1, x2;
	public int scale1, scale2, scalestep;
	public int			silhouette;			// 0=none, 1=bottom, 2=top, 3=both
	public int bsilheight;			// don't clip sprites above this
	public int tsilheight;			// don't clip sprites below this
// pointers to lists for sprite clipping
	public short[] sprtopclip;		// adjusted so [x1] is first value
	public short[] sprbottomclip;		// adjusted so [x1] is first value
	public short[] maskedtexturecol;	// adjusted so [x1] is first value
} ;
public const int SIL_NONE = 0;
public const int 	SIL_BOTTOM	=1;
public const int  SIL_TOP		=2;
public const int 	SIL_BOTH	=3;
public const int MAXDRAWSEGS = 256;

// A vissprite_t is a thing that will be drawn during a refresh
public class vissprite_t
{
	public vissprite_t prev, next;
	public int x1, x2;
	public int gx, gy;			// for line side calculation
	public int gz, gzt;		// global bottom / top for silhouette clipping
	public int startfrac;		// horizontal position of x1
	public int scale;
	public int xiscale;		// negative if flipped
	public int texturemid;
	public int patch;
	public int colormapi;
	public int mobjflags;		// for color translation and shadow draw
	public bool psprite;		// true if psprite
	public int footclip;		// foot clipping
	public DoomDef.mobj_t thing; // [dsl] Added so we have a pointer on the object we are rendering
	public bool flip; // [dsl] Added
	public float selfIllumT;
	public float selfIllumB;
} ;


// Sprites are patches with a special naming convention so they can be 
// recognized by R_InitSprites.  The sprite and frame specified by a 
// thing_t is range checked at run time.
// a sprite is a patch_t that is assumed to represent a three dimensional
// object and may have multiple rotations pre drawn.  Horizontal flipping 
// is used to save space. Some sprites will only have one picture used
// for all views.  

public class spriteframe_t
{
	public int		rotate;		// if false use 0 for any position
	public short[] lump = new short[8];	// lump to use for view angles 0-7
	public byte[] flip = new byte[8];	// flip (1 = flip) to use for view angles 0-7
} ;

public class spritedef_t
{
	public int numframes;
	public spriteframe_t[] spriteframes;
} ;

//
// R_things.c
//
public const int MAXVISSPRITES = 128;

//=============================================================================
//
// R_draw.c
//
//=============================================================================

	}
}
