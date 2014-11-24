using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

		// R_planes.c

namespace HereticXNA
{
	public static class r_plane
	{
#if DOS
planefunction_t		floorfunc, ceilingfunc;

#endif
		//
		// sky mapping
		//
		public static int skyflatnum;
		public static int skytexture;
		public static int skytexturemid;
		public static int skyiscale;
		//
		// opening
		//

		public static r_local.visplane_t[] visplanes = new r_local.visplane_t[r_local.MAXVISPLANES] {
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
	new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),new r_local.visplane_t(),
};
		public static int lastvisplane;
		public static r_local.visplane_t floorplane;
		public static r_local.visplane_t ceilingplane;

		public static short[] openings = new short[r_local.MAXOPENINGS];
		public static short lastopening;
#if DOS

//
// clip values are the solid pixel bounding the range
// floorclip starts out SCREENHEIGHT
// ceilingclip starts out -1
//
short		floorclip[SCREENWIDTH];
short		ceilingclip[SCREENWIDTH];

//
// spanstart holds the start of a plane span
// initialized to 0 at start
//
int			spanstart[SCREENHEIGHT];
int			spanstop[SCREENHEIGHT];

//
// texture mapping
//
lighttable_t	**planezlight;
int		planeheight;
#endif

		public static int[] yslope = new int[DoomDef.SCREENHEIGHT];
		public static int[] distscale = new int[DoomDef.SCREENWIDTH];
		public static int basexscale, baseyscale;
#if DOS

int		cachedheight[SCREENHEIGHT];
int		cacheddistance[SCREENHEIGHT];
int		cachedxstep[SCREENHEIGHT];
int		cachedystep[SCREENHEIGHT];

#endif

		/*
================
=
= R_InitSkyMap
=
= Called whenever the view size changes
=
================
*/

		public static void R_InitSkyMap()
		{
			skyflatnum = r_data.R_FlatNumForName("F_SKY1");
			skytexturemid = 200 * DoomDef.FRACUNIT;
			skyiscale = DoomDef.FRACUNIT;
		}

		/*
		====================
		=
		= R_InitPlanes
		=
		= Only at game startup
		====================
		*/

		public static void R_InitPlanes()
		{
		}

#if DOS
/*
================
=
= R_MapPlane
=
global vars:

planeheight
ds_source
basexscale
baseyscale
viewx
viewy

BASIC PRIMITIVE
================
*/

void R_MapPlane (int y, int x1, int x2)
{
	uint		angle;
	int		distance, length;
	unsigned	index;
	
	if (planeheight != cachedheight[y])
	{
		cachedheight[y] = planeheight;
		distance = cacheddistance[y] = FixedMul (planeheight, yslope[y]);

		ds_xstep = cachedxstep[y] = FixedMul (distance,basexscale);
		ds_ystep = cachedystep[y] = FixedMul (distance,baseyscale);
	}
	else
	{
		distance = cacheddistance[y];
		ds_xstep = cachedxstep[y];
		ds_ystep = cachedystep[y];
	}
	
	length = FixedMul (distance,distscale[x1]);
	angle = (viewangle + xtoviewangle[x1])>>ANGLETOFINESHIFT;
	ds_xfrac = viewx + FixedMul(finecosine[angle], length);
	ds_yfrac = -viewy - FixedMul(finesine[angle], length);

	if (fixedcolormap)
		ds_colormap = fixedcolormap;
	else
	{
		index = distance >> LIGHTZSHIFT;
		if (index >= MAXLIGHTZ )
			index = MAXLIGHTZ-1;
		ds_colormap = planezlight[index];
	}
	
	ds_y = y;
	ds_x1 = x1;
	ds_x2 = x2;
	
	spanfunc ();		// high or low detail
}
#endif
		//=============================================================================

		/*
		====================
		=
		= R_ClearPlanes
		=
		= At begining of frame
		====================
		*/

		public static void R_ClearPlanes()
		{
			int i;
			uint angle;

			//
			// opening / clipping determination
			//	
			// [dsl] We don't need that
			//for (i=0 ; i<r_draw.viewwidth ; i++)
			//{
			//    floorclip[i] = r_draw.viewheight;
			//    ceilingclip[i] = -1;
			//}

			lastvisplane = 0;
			//	lastopening = openings;

			//
			// texture calculation
			//
			//memset (cachedheight, 0, sizeof(cachedheight));	
			angle = (uint)((int)(r_main.viewangle - DoomDef.ANG90) >> (int)DoomDef.ANGLETOFINESHIFT);	// left to right mapping

			// scale will be unit scale at SCREENWIDTH/2 distance
			r_plane.basexscale = d_main.FixedDiv(r_main.finecosine(angle), r_main.centerxfrac);
			r_plane.baseyscale = -d_main.FixedDiv(tables.finesine[(uint)angle % tables.finesine.Count()], r_main.centerxfrac);
		}


		/*
		===============
		=
		= R_FindPlane
		=
		===============
		*/

		public static r_local.visplane_t R_FindPlane(int height, int picnum,
			int lightlevel, int special)
		{
			int check;

			if (picnum == skyflatnum)
			{
				// all skies map together
				height = 0;
				lightlevel = 0;
			}

			for (check = 0; check < lastvisplane; check++)
			{
				if (height == r_plane.visplanes[check].height
				&& picnum == r_plane.visplanes[check].picnum
				&& lightlevel == r_plane.visplanes[check].lightlevel
				&& special == r_plane.visplanes[check].special)
					break;
			}

			if (check < lastvisplane)
			{
				return (r_plane.visplanes[check]);
			}

			if (lastvisplane == r_local.MAXVISPLANES)
			{
				i_ibm.I_Error("R_FindPlane: no more visplanes");
			}

			lastvisplane++;
			r_plane.visplanes[check].height = height;
			r_plane.visplanes[check].picnum = picnum;
			r_plane.visplanes[check].lightlevel = lightlevel;
			r_plane.visplanes[check].special = special;
			r_plane.visplanes[check].minx = DoomDef.SCREENWIDTH;
			r_plane.visplanes[check].maxx = -1;
			//memset(check.top,0xff,sizeof(check.top)); // [dsl] We don't need that info, it's for blitting
			return (r_plane.visplanes[check]);
		}

/*
===============
=
= R_CheckPlane
=
===============
*/

public static r_local.visplane_t R_CheckPlane (r_local.visplane_t pl, int start, int stop)
{
	int			intrl, intrh;
	int			unionl, unionh;
	int			x;
	
	if (start < pl.minx)
	{
		intrl = pl.minx;
		unionl = start;
	}
	else
	{
		unionl = pl.minx;
		intrl = start;
	}
	
	if (stop > pl.maxx)
	{
		intrh = pl.maxx;
		unionh = stop;
	}
	else
	{
		unionh = pl.maxx;
		intrh = stop;
	}

	x = intrl;
	/*
	for (x=intrl ; x<= intrh ; x++)
		if (pl.top[x] != 0xff)
			break;
	*/
	if (x > intrh)
	{
		pl.minx = unionl;
		pl.maxx = unionh;
		return pl;			// use the same one
	}
	
// make a new visplane

	visplanes[lastvisplane].height = pl.height;
	visplanes[lastvisplane].picnum = pl.picnum;
	visplanes[lastvisplane].lightlevel = pl.lightlevel;
	visplanes[lastvisplane].special = pl.special;
	visplanes[lastvisplane].sector = pl.sector;
	pl = visplanes[lastvisplane++];
	pl.minx = start;
	pl.maxx = stop;
	//memset (pl.top,0xff,sizeof(pl.top));
		
	return pl;
}

#if DOS


//=============================================================================

/*
================
=
= R_MakeSpans
=
================
*/

void R_MakeSpans (int x, int t1, int b1, int t2, int b2)
{
	while (t1 < t2 && t1<=b1)
	{
		R_MapPlane (t1,spanstart[t1],x-1);
		t1++;
	}
	while (b1 > b2 && b1>=t1)
	{
		R_MapPlane (b1,spanstart[b1],x-1);
		b1--;
	}
	
	while (t2 < t1 && t2<=b2)
	{
		spanstart[t2] = x;
		t2++;
	}
	while (b2 > b1 && b2>=t2)
	{
		spanstart[b2] = x;
		b2--;
	}
}


#endif
/*
================
=
= R_DrawPlanes
=
= At the end of each frame
================
*/

public static List<r_local.sector_t> visibleSectors = new List<r_local.sector_t>();
		static VertexPositionColorTexture[] verts = new VertexPositionColorTexture[32];

		public static void XNARenderFlatToAmbientMap(r_local.sector_t sector)
		{
			if (sector.triangleList == null) return;
			if (sector.triangleList.Count == 0) return;
			GraphicsDevice device = Game1.instance.GraphicsDevice;
			if (verts.Count() < sector.triangleList.Count())
			{
				verts = new VertexPositionColorTexture[sector.triangleList.Count];
			}
			float floorH = sector.floorheight >> DoomDef.FRACBITS;
			float ceilH = sector.ceilingheight >> DoomDef.FRACBITS;

			for (int i = 0; i < sector.triangleList.Count; i += 3)
			{
				r_data.Vector2AndTags p1 = sector.triangleList[i + 0];
				r_data.Vector2AndTags p2 = sector.triangleList[i + 1];
				r_data.Vector2AndTags p3 = sector.triangleList[i + 2];
				Vector3 v1 = new Vector3(p1.p, floorH);
				Vector3 v2 = new Vector3(p2.p, floorH);
				Vector3 v3 = new Vector3(p3.p, floorH);
				Vector2 floorCeil;
				floorCeil.X = ((sector.floorheight >> DoomDef.FRACBITS) + 1024.0f) / 2048.0f;
				floorCeil.Y = ((sector.ceilingheight >> DoomDef.FRACBITS) + 1024.0f) / 2048.0f;
				verts[i + 0] = new VertexPositionColorTexture(
					v1,
					new Color(
						1.0f,
						1,
						1, 1),
					floorCeil);
				verts[i + 1] = new VertexPositionColorTexture(
					v2,
					new Color(
						1.0f,
						1,
						1, 1),
					floorCeil);
				verts[i + 2] = new VertexPositionColorTexture(
					v3,
					new Color(
						1.0f,
						1,
						1, 1),
					floorCeil);
			}
			device.DrawUserPrimitives<VertexPositionColorTexture>(
				PrimitiveType.TriangleList, verts, 0, sector.triangleList.Count / 3);
		}

		public static void XNARenderCeilToBatch(r_local.sector_t sector, r_local.SectorBatch batch)
		{
			if (sector.triangleList == null) return;
			if (sector.triangleList.Count == 0) return;
			float ceilH = sector.ceilingheight >> DoomDef.FRACBITS;
			{
				if (skyflatnum == sector.ceilingpic)
				{
					batch.isSky = true;
					//batch.texture = Game1.instance.wallTextures[r_plane.skytexture];
					batch.texture = Game1.instance.wallTexturesById[r_plane.skytexture];
				}
				else
				{
					batch.isSky = false;
					batch.texture = Game1.instance.floorTexturesById[sector.ceilingpic];
				}
				batch.textureId = sector.ceilingpic;
				for (int i = 0; i < sector.triangleList.Count; i += 3)
				{
					r_data.Vector2AndTags p1 = sector.triangleList[i + 0];
					r_data.Vector2AndTags p2 = sector.triangleList[i + 1];
					r_data.Vector2AndTags p3 = sector.triangleList[i + 2];
					int t1 = p1.groupTag;
					int t2 = p2.groupTag;
					int t3 = p3.groupTag;
					Vector3 v1 = new Vector3(
							p1.p.X,
							p1.p.Y,
							ceilH);
					Vector3 v2 = new Vector3(
							p2.p.X,
							p2.p.Y,
							ceilH);
					Vector3 v3 = new Vector3(
							p3.p.X,
							p3.p.Y,
							ceilH);
					if (Settings.Default.use_deferred)
					{
						batch.vertsPNT.Add(new VertexPositionNormalTexture(
							v2,
							-Vector3.UnitZ,
							new Vector2(v2.X / (float)batch.texture.Width, -v2.Y / (float)batch.texture.Height)));
						batch.vertsPNT.Add(new VertexPositionNormalTexture(
							v1,
							-Vector3.UnitZ,
							new Vector2(v1.X / (float)batch.texture.Width, -v1.Y / (float)batch.texture.Height)));
						batch.vertsPNT.Add(new VertexPositionNormalTexture(
							v3,
							-Vector3.UnitZ,
							new Vector2(v3.X / (float)batch.texture.Width, -v3.Y / (float)batch.texture.Height)));
					}
					else
					{
						batch.vertsPT.Add(new VertexPositionTexture(
							v2,
							new Vector2(v2.X / (float)batch.texture.Width, -v2.Y / (float)batch.texture.Height)));
						batch.vertsPT.Add(new VertexPositionTexture(
							v1,
							new Vector2(v1.X / (float)batch.texture.Width, -v1.Y / (float)batch.texture.Height)));
						batch.vertsPT.Add(new VertexPositionTexture(
							v3,
							new Vector2(v3.X / (float)batch.texture.Width, -v3.Y / (float)batch.texture.Height)));
					}
				}
			}
		}

		public static void XNARenderFloorToBatch(r_local.sector_t sector, r_local.SectorBatch batch)
		{
			if (sector.triangleList == null) return;
			if (sector.triangleList.Count == 0) return;
			float floorH = sector.floorheight >> DoomDef.FRACBITS;
			{
				batch.textureId = sector.floorpic;
				int prevSource = r_data.flattranslationPrev[sector.floorpic];
				int tempSource = r_data.flattranslation[sector.floorpic];
				float uOffset = 0;
				float vOffset = 0;
				int textureId = 0;// tempSource;
				batch.isUVAnimated = false;
				switch (sector.special)
				{
					case 25:
					case 26:
					case 27:
					case 28:
					case 29: // Scroll_North
						textureId = tempSource;
						break;
					case 20:
					case 21:
					case 22:
					case 23:
					case 24: // Scroll_East
						uOffset = (float)((63 - ((p_tick.leveltime >> 1) & 63)) << (sector.special - 20) & 63) / 64.0f;
						textureId = tempSource;
						batch.isUVAnimated = true;
						break;
					case 30:
					case 31:
					case 32:
					case 33:
					case 34: // Scroll_South
						textureId = tempSource;
						break;
					case 35:
					case 36:
					case 37:
					case 38:
					case 39: // Scroll_West
						textureId = tempSource;
						break;
					case 4: // Scroll_EastLavaDamage
						uOffset = (float)(((63 - ((p_tick.leveltime >> 1) & 63)) << 3) & 63) / 64.0f;
						textureId = tempSource;
						batch.isUVAnimated = true;
						break;
					default:
						textureId = tempSource;
						break;
				}

				Texture2D texture = Game1.instance.floorTexturesById[textureId];

				batch.texture = texture;
				for (int i = 0; i < sector.triangleList.Count; i += 3)
				{
					r_data.Vector2AndTags p1 = sector.triangleList[i + 0];
					r_data.Vector2AndTags p2 = sector.triangleList[i + 1];
					r_data.Vector2AndTags p3 = sector.triangleList[i + 2];
					Vector3 v1 = new Vector3(p1.p, floorH);
					Vector3 v2 = new Vector3(p2.p, floorH);
					Vector3 v3 = new Vector3(p3.p, floorH);
					if (Settings.Default.use_deferred)
					{
						batch.vertsPNT.Add(new VertexPositionNormalTexture(
							v1,
							Vector3.UnitZ,
							new Vector2(v1.X / 64.0f + uOffset, -v1.Y / 64.0f + vOffset)));
						batch.vertsPNT.Add(new VertexPositionNormalTexture(
							v2,
							Vector3.UnitZ,
							new Vector2(v2.X / 64.0f + uOffset, -v2.Y / 64.0f + vOffset)));
						batch.vertsPNT.Add(new VertexPositionNormalTexture(
							v3,
							Vector3.UnitZ,
							new Vector2(v3.X / 64.0f + uOffset, -v3.Y / 64.0f + vOffset)));
					}
					else
					{
						batch.vertsPT.Add(new VertexPositionTexture(
							v1,
							new Vector2(v1.X / 64.0f + uOffset, -v1.Y / 64.0f + vOffset)));
						batch.vertsPT.Add(new VertexPositionTexture(
							v2,
							new Vector2(v2.X / 64.0f + uOffset, -v2.Y / 64.0f + vOffset)));
						batch.vertsPT.Add(new VertexPositionTexture(
							v3,
							new Vector2(v3.X / 64.0f + uOffset, -v3.Y / 64.0f + vOffset)));
					}
				}
			}
		}

		public static void XNARenderFlat(r_local.sector_t sector)
		{
		/*	if (sector.triangleList == null) return;
			if (sector.triangleList.Count == 0) return;
			GraphicsDevice device = Game1.instance.GraphicsDevice;
			if (verts.Count() < sector.triangleList.Count())
			{
				verts = new VertexPositionColorTexture[sector.triangleList.Count];
			}
			float floorH = sector.floorheight >> DoomDef.FRACBITS;
			float ceilH = sector.ceilingheight >> DoomDef.FRACBITS;
			int light = (sector.lightlevel >> r_local.LIGHTSEGSHIFT) + r_main.extralight;
			if (light >= r_local.LIGHTLEVELS)
				light = r_local.LIGHTLEVELS - 1;
			if (light < 0)
				light = 0;

	//		planezlight = r_main.zlight[light];
			float lightLevel = (float)(sector.lightlevel + r_main.extralight) / 255.0f;
			Game1.instance.fxPlane.Parameters["lightLevel"].SetValue(lightLevel);
			Vector2 floorCeil;
			floorCeil.X = ((sector.floorheight >> DoomDef.FRACBITS) + 1024.0f) / 2048.0f;
			floorCeil.Y = ((sector.ceilingheight >> DoomDef.FRACBITS) + 1024.0f) / 2048.0f;
			Game1.instance.fxPlane.Parameters["floorCeil"].SetValue(floorCeil);
			{
				int prevSource = r_data.flattranslationPrev[sector.floorpic];
				int tempSource = r_data.flattranslation[sector.floorpic];
				float uOffset = 0;
				float vOffset = 0;
				int textureId = 0;// tempSource;
				switch (sector.special)
				{
					case 25:
					case 26:
					case 27:
					case 28:
					case 29: // Scroll_North
						textureId = tempSource;
						break;
					case 20:
					case 21:
					case 22:
					case 23:
					case 24: // Scroll_East
						uOffset = (float)((63 - ((p_tick.leveltime >> 1) & 63)) << (sector.special - 20) & 63) / 64.0f;
						textureId = tempSource;
						break;
					case 30:
					case 31:
					case 32:
					case 33:
					case 34: // Scroll_South
						textureId = tempSource;
						break;
					case 35:
					case 36:
					case 37:
					case 38:
					case 39: // Scroll_West
						textureId = tempSource;
						break;
					case 4: // Scroll_EastLavaDamage
						uOffset = (float)(((63 - ((p_tick.leveltime >> 1) & 63)) << 3) & 63) / 64.0f;
						textureId = tempSource;
						break;
					default:
						textureId = tempSource;
						break;
				}

				Texture2D texture = Game1.instance.floorTexturesById[textureId];

				if (prevSource != tempSource)
				{
					Texture2D prevTexture = Game1.instance.floorTexturesById[prevSource];
					EffectTechnique prevTechnique = Game1.instance.fxPlane.CurrentTechnique;
					Game1.instance.fxPlane.CurrentTechnique = Game1.instance.fxPlane.Techniques[Game1.instance.techniqueName("TechniqueAnimatedFloor")];
					Game1.instance.fxPlane.Parameters["AnimatedTexture1"].SetValue(texture);
					Game1.instance.fxPlane.Parameters["AnimatedTexture2"].SetValue(prevTexture);
					float delta = r_data.flattranslationDeltas[sector.floorpic];
					Game1.instance.fxPlane.Parameters["AnimT"].SetValue(delta);
					Game1.instance.fxPlane.CurrentTechnique.Passes[0].Apply();
					Game1.instance.fxPlane.CurrentTechnique = prevTechnique;
				}
				else
				{
					Game1.instance.fxPlane.Parameters["DiffuseTexture"].SetValue(texture);
					Game1.instance.fxPlane.CurrentTechnique.Passes[0].Apply();
				}
				for (int i = 0; i < sector.triangleList.Count; i += 3)
				{
					r_data.Vector2AndTags p1 = sector.triangleList[i + 0];
					r_data.Vector2AndTags p2 = sector.triangleList[i + 1];
					r_data.Vector2AndTags p3 = sector.triangleList[i + 2];
					Vector3 v1 = new Vector3(p1.p, floorH);
					Vector3 v2 = new Vector3(p2.p, floorH);
					Vector3 v3 = new Vector3(p3.p, floorH);
					verts[i + 0] = new VertexPositionColorTexture(
						v1,
						new Color(
							0.0f,
							1,
							1, 1),
						new Vector2(v1.X / 64.0f + uOffset, -v1.Y / 64.0f + vOffset));
					verts[i + 1] = new VertexPositionColorTexture(
						v2,
						new Color(
							0.0f,
							1, 
							1, 1),
						new Vector2(v2.X / 64.0f + uOffset, -v2.Y / 64.0f + vOffset));
					verts[i + 2] = new VertexPositionColorTexture(
						v3,
						new Color(
							0.0f,
							1, 
							1, 1),
						new Vector2(v3.X / 64.0f + uOffset, -v3.Y / 64.0f + vOffset));
				}
				device.DrawUserPrimitives<VertexPositionColorTexture>(
					PrimitiveType.TriangleList, verts, 0, sector.triangleList.Count / 3);
			}
			{
				Texture2D texture = null;
				if (skyflatnum == sector.ceilingpic)
				{
					//texture = Game1.instance.wallTextures[r_plane.skytexture];
					texture = Game1.instance.wallTexturesById[r_plane.skytexture];
					Game1.instance.fxSky.Parameters["DiffuseTexture"].SetValue(texture);
					Game1.instance.fxSky.CurrentTechnique.Passes[0].Apply();
				}
				else
				{
					texture = Game1.instance.floorTexturesById[sector.ceilingpic];
					Game1.instance.fxPlane.Parameters["DiffuseTexture"].SetValue(texture);
					Game1.instance.fxPlane.CurrentTechnique.Passes[0].Apply();
				}
				for (int i = 0; i < sector.triangleList.Count; i += 3)
				{
					r_data.Vector2AndTags p1 = sector.triangleList[i + 0];
					r_data.Vector2AndTags p2 = sector.triangleList[i + 1];
					r_data.Vector2AndTags p3 = sector.triangleList[i + 2];
					int t1 = p1.groupTag;
					int t2 = p2.groupTag;
					int t3 = p3.groupTag;
					Vector3 v1 = new Vector3(
							p1.p.X,
							p1.p.Y,
							ceilH);
					Vector3 v2 = new Vector3(
							p2.p.X,
							p2.p.Y,
							ceilH);
					Vector3 v3 = new Vector3(
							p3.p.X,
							p3.p.Y,
							ceilH);
					verts[i + 0] = new VertexPositionColorTexture(
						v1, 
						new Color(1.0f, 1, 1, 1),
						new Vector2(v1.X / (float)texture.Width, -v1.Y / (float)texture.Height));
					verts[i + 2] = new VertexPositionColorTexture(
						v2,
						new Color(1.0f, 1, 1, 1),
						new Vector2(v2.X / (float)texture.Width, -v2.Y / (float)texture.Height));
					verts[i + 1] = new VertexPositionColorTexture(
						v3,
						new Color(1.0f, 1, 1, 1),
						new Vector2(v3.X / (float)texture.Width, -v3.Y / (float)texture.Height));
				}
				device.DrawUserPrimitives<VertexPositionColorTexture>(
					PrimitiveType.TriangleList, verts, 0, sector.triangleList.Count / 3);
			}*/
		}

		public static void R_DrawPlanes()
		{
			foreach (r_local.sector_t sector in visibleSectors)
			{
				XNARenderFlat(sector);
			}
		/*	r_local.visplane_t pl;
			int light;
			int x, stop;
			int angle;
			w_wad.CacheInfo tempSource;

			//	byte *dest;
			int count;
			//	int frac, fracstep;

			for (int i = 0; i < lastvisplane; i++)
			{
				pl = visplanes[i];
				if (pl.minx > pl.maxx)
					continue;
				//
				// sky flat
				//
				if (pl.picnum == skyflatnum)
				{
					// Draw it with the sky shader

					// [dsl] Blit stuff, we ignore
					//            dc_iscale = skyiscale;
					//            dc_colormap = colormaps;// sky is allways drawn full bright
					//            dc_texturemid = skytexturemid;
					//            for (x=pl.minx ; x <= pl.maxx ; x++)
					//            {
					//                dc_yl = pl.top[x];
					//                dc_yh = pl.bottom[x];
					//                if (dc_yl <= dc_yh)
					//                {
					//                    angle = (viewangle + xtoviewangle[x])>>ANGLETOSKYSHIFT;
					//                    dc_x = x;
					//                    dc_source = R_GetColumn(skytexture, angle);

					//                    count = dc_yh - dc_yl;
					//                    if (count < 0)
					//                        return;

					//                    dest = ylookup[dc_yl] + columnofs[dc_x]; 

					//                    fracstep = 1;
					//                    frac = (dc_texturemid>>FRACBITS) + (dc_yl-centery);		
					//                    do
					//                    {
					//                        *dest = dc_source[frac];
					//                        dest += SCREENWIDTH;
					//                        frac += fracstep;
					//                    } while (count--);

					////					colfunc ();
					//                }
					//            }
					continue;
				}

				tempSource = w_wad.W_CacheLumpNum(r_data.firstflat + r_data.flattranslation[pl.picnum], DoomDef.PU_STATIC);

				XNARenderFlat();

				//
				// regular flat
				//
				// [dsl] We might have to dig in there to setup the UVs
				//tempSource = W_CacheLumpNum(firstflat +
				//    flattranslation[pl.picnum], PU_STATIC);

				//switch(pl.special)
				//{
				//    case 25: case 26: case 27: case 28: case 29: // Scroll_North
				//        ds_source = tempSource;
				//        break;
				//    case 20: case 21: case 22: case 23: case 24: // Scroll_East
				//        ds_source = tempSource+((63-((leveltime>>1)&63))<<
				//            (pl.special-20)&63);
				//        //ds_source = tempSource+((leveltime>>1)&63);
				//        break;
				//    case 30: case 31: case 32: case 33: case 34: // Scroll_South
				//        ds_source = tempSource;
				//        break;
				//    case 35: case 36: case 37: case 38: case 39: // Scroll_West
				//        ds_source = tempSource;
				//        break;
				//    case 4: // Scroll_EastLavaDamage
				//        ds_source = tempSource+(((63-((leveltime>>1)&63))<<3)&63);
				//        break;
				//    default:
				//        ds_source = tempSource;
				//}
				//planeheight = abs(pl.height-viewz);
				//light = (pl.lightlevel >> LIGHTSEGSHIFT)+extralight;
				//if (light >= LIGHTLEVELS)
				//    light = LIGHTLEVELS-1;
				//if (light < 0)
				//    light = 0;
				//planezlight = zlight[light];

				//pl.top[pl.maxx+1] = 0xff;
				//pl.top[pl.minx-1] = 0xff;

				//stop = pl.maxx + 1;
				//for (x=pl.minx ; x<= stop ; x++)
				//    R_MakeSpans (x,pl.top[x-1],pl.bottom[x-1]
				//    ,pl.top[x],pl.bottom[x]);

				//Z_ChangeTag (tempSource, PU_CACHE);
			}*/
		}
	}
}
