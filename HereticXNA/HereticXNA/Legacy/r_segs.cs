using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
		
//**************************************************************************
//**
//** R_SEGS.C
//**
//** This version has the tall-sector-crossing-precision-bug fixed.
//**
//**************************************************************************

namespace HereticXNA
{
	public static class r_segs
	{

		// OPTIMIZE: closed two sided lines as single sided

		public static bool segtextured;    // true if any of the segs textures might be vis
		public static bool markfloor;              // false if the back side is the same plane
		public static bool markceiling;
		public static bool maskedtexture;
		public static int toptexture, bottomtexture, midtexture;


		public static uint rw_normalangle;
		public static int rw_angle1;              // angle to line origin

		//
		// wall
		//
		public static int rw_x;
		public static int rw_stopx;
		public static uint rw_centerangle;
		public static int rw_offset;
		public static int rw_distance;
		public static int rw_scale;
		public static int rw_scalestep;
		public static int rw_midtexturemid;
		public static int rw_toptexturemid;
		public static int rw_bottomtexturemid;

		public static int worldtop, worldbottom, worldhigh, worldlow;

		public static int pixhigh, pixlow;
		public static int pixhighstep, pixlowstep;
		public static int topfrac, topstep;
		public static int bottomfrac, bottomstep;


		public static int[] walllights;
		public static short maskedtexturecol;
#if DOS

/*
================
=
= R_RenderMaskedSegRange
=
================
*/

void R_RenderMaskedSegRange (drawseg_t *ds, int x1, int x2)
{
	unsigned        index;
	column_t        *col;
	int                     lightnum;
	int                     texnum;

//
// calculate light table
// use different light tables for horizontal / vertical / diagonal
// OPTIMIZE: get rid of LIGHTSEGSHIFT globally
	curline = ds.curline;
	frontsector = curline.frontsector;
	backsector = curline.backsector;
	texnum = texturetranslation[curline.sidedef.midtexture];

	lightnum = (frontsector.lightlevel >> LIGHTSEGSHIFT)+extralight;
	if (curline.v1.y == curline.v2.y)
		lightnum--;
	else if (curline.v1.x == curline.v2.x)
		lightnum++;
	if (lightnum < 0)
		walllights = scalelight[0];
	else if (lightnum >= LIGHTLEVELS)
		walllights = scalelight[LIGHTLEVELS-1];
	else
		walllights = scalelight[lightnum];

	maskedtexturecol = ds.maskedtexturecol;

	rw_scalestep = ds.scalestep;
	spryscale = ds.scale1 + (x1 - ds.x1)*rw_scalestep;
	mfloorclip = ds.sprbottomclip;
	mceilingclip = ds.sprtopclip;

//
// find positioning
//
	if (curline.linedef.flags & ML_DONTPEGBOTTOM)
	{
		dc_texturemid = frontsector.floorheight > backsector.floorheight
		? frontsector.floorheight : backsector.floorheight;
		dc_texturemid = dc_texturemid + textureheight[texnum] - viewz;
	}
	else
	{
		dc_texturemid =frontsector.ceilingheight<backsector.ceilingheight
		? frontsector.ceilingheight : backsector.ceilingheight;
		dc_texturemid = dc_texturemid - viewz;
	}
	dc_texturemid += curline.sidedef.rowoffset;

	if (fixedcolormap)
		dc_colormap = fixedcolormap;
//
// draw the columns
//
	for (dc_x = x1 ; dc_x <= x2 ; dc_x++)
	{
	// calculate lighting
		if (maskedtexturecol[dc_x] != MAXSHORT)
		{
			if (!fixedcolormap)
			{
				index = spryscale>>LIGHTSCALESHIFT;
				if (index >=  MAXLIGHTSCALE )
					index = MAXLIGHTSCALE-1;
				dc_colormap = walllights[index];
			}

			sprtopscreen = centeryfrac - FixedMul(dc_texturemid, spryscale);
			dc_iscale = 0xffffffffu / (unsigned)spryscale;

	//
	// draw the texture
	//
			col = (column_t *)(
				(byte *)R_GetColumn(texnum,maskedtexturecol[dc_x]) -3);

			R_DrawMaskedColumn (col, -1);
			maskedtexturecol[dc_x] = MAXSHORT;
		}
		spryscale += rw_scalestep;
	}

}
#endif
		/*
================
=
= R_RenderSegLoop
=
= Draws zero, one, or two textures (and possibly a masked texture) for walls
= Can draw or mark the starting pixel of floor and ceiling textures
=
= CALLED: CORE LOOPING ROUTINE
================
*/

		public const int HEIGHTBITS = 12;
		public const int HEIGHTUNIT = (1 << HEIGHTBITS);

		public static void R_RenderSegLoop()
		{
#if DOS
	// [dsl] We'll have to do it our own way here
	uint         angle;
	uint        index;
	int                     yl, yh, mid;
	int         texturecolumn;
	int                     top, bottom;

//      texturecolumn = 0;                              // shut up compiler warning

	for (; rw_x < rw_stopx; rw_x++)
	{
//
// mark floor / ceiling areas
//
		yl = (topfrac+HEIGHTUNIT-1)>>HEIGHTBITS;
		if (yl < ceilingclip[rw_x]+1)
			yl = ceilingclip[rw_x]+1;       // no space above wall
		if (markceiling)
		{
			top = ceilingclip[rw_x]+1;
			bottom = yl-1;
			if (bottom >= floorclip[rw_x])
				bottom = floorclip[rw_x]-1;
			if (top <= bottom)
			{
				ceilingplane.top[rw_x] = top;
				ceilingplane.bottom[rw_x] = bottom;
			}
		}

		yh = bottomfrac>>HEIGHTBITS;
		if (yh >= floorclip[rw_x])
			yh = floorclip[rw_x]-1;
		if (markfloor)
		{
			top = yh+1;
			bottom = floorclip[rw_x]-1;
			if (top <= ceilingclip[rw_x])
				top = ceilingclip[rw_x]+1;
			if (top <= bottom)
			{
				floorplane.top[rw_x] = top;
				floorplane.bottom[rw_x] = bottom;
			}
		}

//
// texturecolumn and lighting are independent of wall tiers
//
		if (segtextured)
		{
		// calculate texture offset
			angle = (rw_centerangle + xtoviewangle[rw_x])>>ANGLETOFINESHIFT;
			texturecolumn = rw_offset-FixedMul(finetangent[angle],rw_distance);
			texturecolumn >>= FRACBITS;
		// calculate lighting
			index = rw_scale>>LIGHTSCALESHIFT;
			if (index >=  MAXLIGHTSCALE )
				index = MAXLIGHTSCALE-1;
			dc_colormap = walllights[index];
			dc_x = rw_x;
			dc_iscale = 0xffffffffu / (unsigned)rw_scale;
		}

//
// draw the wall tiers
//
		if (midtexture)
		{       // single sided line
			dc_yl = yl;
			dc_yh = yh;
			dc_texturemid = rw_midtexturemid;
			dc_source = R_GetColumn(midtexture,texturecolumn);
			colfunc ();
			ceilingclip[rw_x] = viewheight;
			floorclip[rw_x] = -1;
		}
		else
		{       // two sided line
			if (toptexture)
			{       // top wall
				mid = pixhigh>>HEIGHTBITS;
				pixhigh += pixhighstep;
				if (mid >= floorclip[rw_x])
					mid = floorclip[rw_x]-1;
				if (mid >= yl)
				{
					dc_yl = yl;
					dc_yh = mid;
					dc_texturemid = rw_toptexturemid;
					dc_source = R_GetColumn(toptexture,texturecolumn);
					colfunc ();
					ceilingclip[rw_x] = mid;
				}
				else
					ceilingclip[rw_x] = yl-1;
			}
			else
			{       // no top wall
				if (markceiling)
					ceilingclip[rw_x] = yl-1;
			}

			if (bottomtexture)
			{       // bottom wall
				mid = (pixlow+HEIGHTUNIT-1)>>HEIGHTBITS;
				pixlow += pixlowstep;
				if (mid <= ceilingclip[rw_x])
					mid = ceilingclip[rw_x]+1;      // no space above wall
				if (mid <= yh)
				{
					dc_yl = mid;
					dc_yh = yh;
					dc_texturemid = rw_bottomtexturemid;
					dc_source = R_GetColumn(bottomtexture,
						 texturecolumn);
					colfunc ();
					floorclip[rw_x] = mid;
				}
				else
					floorclip[rw_x] = yh+1;
			}
			else
			{       // no bottom wall
				if (markfloor)
					floorclip[rw_x] = yh+1;
			}

			if (maskedtexture)
			{       // save texturecol for backdrawing of masked mid texture
				maskedtexturecol[rw_x] = texturecolumn;
			}
		}

		rw_scale += rw_scalestep;
		topfrac += topstep;
		bottomfrac += bottomstep;
	}


#endif
		}

		/*
=====================
=
= R_StoreWallRange
=
= A wall segment will be drawn between start and stop pixels (inclusive)
=
======================
*/
		static VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];

		public static float ambientEpsilon = 1024.0f;
		public static float ambientSize = 32.0f;
		public static float ambientAmount = .5f;

	//	static r_local.seg_tv segtmp = new r_local.seg_tv();

		public static void XNARenderWall(r_local.seg_t seg)
		{
			XNARenderWallSplit(seg);
		}

		static float vv1;
		static float vv2;
		static Texture2D texture = null;
		static float ceilH;
		static float floorH;
		static bool isSky;
		static float u1;
		static float u2;
		static Vector3 v1;
		static Vector3 v2;
		static float len;

		public class WallHSplit
		{
			public float leftAmbient = 0;
			public float rightAmbient = 0;
			public float height = 0;
			public float hPercent = 0;
		}

		const int maxSplit = 64;
		static int splitCount = 0;
		static WallHSplit[] wallHSplits = new WallHSplit[maxSplit] {
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),
			new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit(),new WallHSplit()
		};

		public static bool XNAWallRenderableAtHeight(r_local.seg_t seg, float height)
		{
			int min = seg.frontsector.floorheight;
			int max = seg.frontsector.ceilingheight;
			if (seg.backsector != null)
			{
				if (max > seg.backsector.floorheight)
				{
					max = seg.backsector.floorheight;
				}
			}
			if (min < max)
			{
				float minf = min >> DoomDef.FRACBITS;
				float maxf = max >> DoomDef.FRACBITS;
				if (height >= minf && height <= maxf) return true;
			}
			if (max < seg.frontsector.ceilingheight)
			{
				min = seg.frontsector.floorheight;
				max = seg.frontsector.ceilingheight;
				if (seg.backsector != null)
				{
					if (min < seg.backsector.ceilingheight)
					{
						min = seg.backsector.ceilingheight;
					}
				}
				if (min < max)
				{
					float minf = min >> DoomDef.FRACBITS;
					float maxf = max >> DoomDef.FRACBITS;
					if (height >= minf && height <= maxf) return true;
				}
			}

			return false;
		}

		public static bool XNAIsWallRenderable(r_local.seg_t seg)
		{
			int min = seg.frontsector.floorheight;
			int max = seg.frontsector.ceilingheight;
			if (seg.backsector != null)
			{
				if (max > seg.backsector.floorheight)
				{
					max = seg.backsector.floorheight;
				}
			}
			if (min < max) return true;
			if (max < seg.frontsector.ceilingheight)
			{
				min = seg.frontsector.floorheight;
				max = seg.frontsector.ceilingheight;
				if (seg.backsector != null)
				{
					if (min < seg.backsector.ceilingheight)
					{
						min = seg.backsector.ceilingheight;
					}
				}
				if (min < max)
				{
					return true;
				}
			}

			return false;
		}

		public static void XNAAddHSplit(r_local.seg_t seg, float height)
		{
			if (splitCount == maxSplit) return; // Can't split more
			if (height < v1.Z ||
				height > v2.Z) return; // Not in the wall part

			// Make sure we don't already have a split at that level.
			for (int i = 0; i < splitCount; ++i)
			{
				if (wallHSplits[i].height == height) return;
			}

			WallHSplit wallHSplit = wallHSplits[splitCount];

			wallHSplit.leftAmbient = 1;
			wallHSplit.rightAmbient = 1;

			float dot;

			// Determine best ambient for that height on both side
			foreach (r_local.seg_t other in seg.left)
			{
				if (XNAWallRenderableAtHeight(other, height))
				{
					dot = Vector2.Dot(other.dir, seg.dir);
					if (dot < 0) dot = 0;
					if (dot < wallHSplit.leftAmbient) wallHSplit.leftAmbient = dot;
				}
			}
			foreach (r_local.seg_t other in seg.right)
			{
				if (XNAWallRenderableAtHeight(other, height))
				{
					dot = Vector2.Dot(other.dir, seg.dir);
					if (dot < 0) dot = 0;
					if (dot < wallHSplit.rightAmbient) wallHSplit.rightAmbient = dot;
				}
			}

			wallHSplits[splitCount].height = height;
			wallHSplits[splitCount].hPercent = (height - v1.Z) / (v2.Z - v1.Z);
			++splitCount;
		}

		public class cSplitHComparer: IComparer<WallHSplit>
		{
			public int Compare(WallHSplit a, WallHSplit b)
			{
				if (a.height > b.height) return 1;
				else if (a.height < b.height) return -1;
				else return 0;
			}
		};

		static cSplitHComparer SplitHComparer = new cSplitHComparer();

		public static void XNARenderWallParts(r_local.seg_t seg)
		{
			splitCount = 0;

			// Add bottom and top split first
			XNAAddHSplit(seg, v1.Z);
			XNAAddHSplit(seg, v2.Z);

			float otherFloorH = 0;
			float otherCeilH = 0;
			float otherBFloorH = 0;
			float otherBCeilH = 0;

			// Add left splits
			foreach (r_local.seg_t other in seg.left)
			{
				if (!XNAIsWallRenderable(other)) continue;
				otherCeilH = other.frontsector.ceilingheight >> DoomDef.FRACBITS;
				otherFloorH = other.frontsector.floorheight >> DoomDef.FRACBITS;
				XNAAddHSplit(seg, otherFloorH);
				XNAAddHSplit(seg, otherCeilH);

				// Might have a hole so add it
				if (other.backsector != null)
				{
					otherBCeilH = other.backsector.ceilingheight >> DoomDef.FRACBITS;
					otherBFloorH = other.backsector.floorheight >> DoomDef.FRACBITS;
					XNAAddHSplit(seg, otherBFloorH);
					XNAAddHSplit(seg, otherBCeilH);
				}
			}

			// Add right splits
			foreach (r_local.seg_t other in seg.right)
			{
				if (!XNAIsWallRenderable(other)) continue;
				otherCeilH = other.frontsector.ceilingheight >> DoomDef.FRACBITS;
				otherFloorH = other.frontsector.floorheight >> DoomDef.FRACBITS;
				XNAAddHSplit(seg, otherFloorH);
				XNAAddHSplit(seg, otherCeilH);

				// Might have a hole so add it
				if (other.backsector != null)
				{
					otherBCeilH = other.backsector.ceilingheight >> DoomDef.FRACBITS;
					otherBFloorH = other.backsector.floorheight >> DoomDef.FRACBITS;
					XNAAddHSplit(seg, otherBFloorH);
					XNAAddHSplit(seg, otherBCeilH);
				}
			}

			// Sort splits
			Array.Sort(wallHSplits, 0, splitCount, SplitHComparer);

			// If a split is too big, split it more in 3 with ambientSize distance
			WallHSplit prev = wallHSplits[0];
			WallHSplit cur;
			int count = splitCount;
			for (int i = 1; i < count; ++i)
			{
				cur = wallHSplits[i];
				if (cur.height - prev.height > ambientSize)
				{
					if (cur.height - prev.height > ambientSize * 2)
					{
						// Split twice
						XNAAddHSplit(seg, prev.height + ambientSize);
						XNAAddHSplit(seg, cur.height - ambientSize);
					}
					else
					{
						// Just split once, in the middle
						XNAAddHSplit(seg, (cur.height + prev.height) * .5f);
					}
				}
			}

			// Sort again for the new splits!
			if (count != splitCount)
			{
				Array.Sort(wallHSplits, 0, splitCount, SplitHComparer);
			}

			// Render splits
			float vvv1 = 0;
			float vvv2 = 0;
			for (int i = 1; i < splitCount; ++i)
			{
				cur = wallHSplits[i];

				vvv1 = MathHelper.Lerp(vv1, vv2, prev.hPercent);
				vvv2 = MathHelper.Lerp(vv1, vv2, cur.hPercent);

				verts[0] = new VertexPositionColorTexture(
					new Vector3(v2.X, v2.Y, prev.height),
					new Color(
						(prev.height - floorH) / ambientEpsilon,
						isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
						((len + ambientSize * prev.rightAmbient) / ambientEpsilon),
						ambientSize * prev.rightAmbient / ambientEpsilon),
					new Vector2(u2 / (float)texture.Width, vvv1 / (float)texture.Height));

				verts[1] = new VertexPositionColorTexture(
					new Vector3(v1.X, v1.Y, prev.height),
					new Color(
						(prev.height - floorH) / ambientEpsilon,
						isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
						ambientSize * prev.leftAmbient / ambientEpsilon,
						(len + ambientSize * prev.leftAmbient) / ambientEpsilon),
					new Vector2(u1 / (float)texture.Width, vvv1 / (float)texture.Height));

				verts[2] = new VertexPositionColorTexture(
					new Vector3(v2.X, v2.Y, cur.height),
					new Color(
						(cur.height - floorH) / ambientEpsilon,
						isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
						(len + ambientSize * cur.rightAmbient) / ambientEpsilon,
						ambientSize * cur.rightAmbient / ambientEpsilon),
					new Vector2(u2 / (float)texture.Width, vvv2 / (float)texture.Height));

				verts[3] = new VertexPositionColorTexture(
					new Vector3(v1.X, v1.Y, cur.height),
					new Color(
						(cur.height - floorH) / ambientEpsilon,
						isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
						ambientSize * cur.leftAmbient / ambientEpsilon,
						(len + ambientSize * cur.leftAmbient) / ambientEpsilon),
					new Vector2(u1 / (float)texture.Width, vvv2 / (float)texture.Height));

				Game1.instance.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleStrip, verts, 0, 2);

				prev = cur;
			}
		}

		public static void XNARenderFlatWallBatch(r_local.seg_t seg, r_local.SectorBatch batch)
		{
			if (Settings.Default.use_deferred)
			{
				Vector3 normal = new Vector3(seg.normal, 0);

				batch.vertsPNT.Add(new VertexPositionNormalTexture(
					new Vector3(v2.X, v2.Y, v1.Z),
					normal,
					new Vector2(u2 / (float)texture.Width, vv1 / (float)texture.Height)));

				batch.vertsPNT.Add(new VertexPositionNormalTexture(
					new Vector3(v1.X, v1.Y, v1.Z),
					normal,
					new Vector2(u1 / (float)texture.Width, vv1 / (float)texture.Height)));

				batch.vertsPNT.Add(new VertexPositionNormalTexture(
					new Vector3(v2.X, v2.Y, v2.Z),
					normal,
					new Vector2(u2 / (float)texture.Width, vv2 / (float)texture.Height)));

				batch.vertsPNT.Add(new VertexPositionNormalTexture(
					new Vector3(v1.X, v1.Y, v1.Z),
					normal,
					new Vector2(u1 / (float)texture.Width, vv1 / (float)texture.Height)));

				batch.vertsPNT.Add(new VertexPositionNormalTexture(
					new Vector3(v1.X, v1.Y, v2.Z),
					normal,
					new Vector2(u1 / (float)texture.Width, vv2 / (float)texture.Height)));

				batch.vertsPNT.Add(new VertexPositionNormalTexture(
					new Vector3(v2.X, v2.Y, v2.Z),
					normal,
					new Vector2(u2 / (float)texture.Width, vv2 / (float)texture.Height)));
			}
			else
			{
				batch.vertsPT.Add(new VertexPositionTexture(
					new Vector3(v2.X, v2.Y, v1.Z),
					new Vector2(u2 / (float)texture.Width, vv1 / (float)texture.Height)));

				batch.vertsPT.Add(new VertexPositionTexture(
					new Vector3(v1.X, v1.Y, v1.Z),
					new Vector2(u1 / (float)texture.Width, vv1 / (float)texture.Height)));

				batch.vertsPT.Add(new VertexPositionTexture(
					new Vector3(v2.X, v2.Y, v2.Z),
					new Vector2(u2 / (float)texture.Width, vv2 / (float)texture.Height)));

				batch.vertsPT.Add(new VertexPositionTexture(
					new Vector3(v1.X, v1.Y, v1.Z),
					new Vector2(u1 / (float)texture.Width, vv1 / (float)texture.Height)));

				batch.vertsPT.Add(new VertexPositionTexture(
					new Vector3(v1.X, v1.Y, v2.Z),
					new Vector2(u1 / (float)texture.Width, vv2 / (float)texture.Height)));

				batch.vertsPT.Add(new VertexPositionTexture(
					new Vector3(v2.X, v2.Y, v2.Z),
					new Vector2(u2 / (float)texture.Width, vv2 / (float)texture.Height)));
			}
		}

		public static void XNARenderWallPartsToBatch(r_local.seg_t seg, r_local.SectorBatch batch)
		{
			splitCount = 0;

			// Add bottom and top split first
			XNAAddHSplit(seg, v1.Z);
			XNAAddHSplit(seg, v2.Z);

			float otherFloorH = 0;
			float otherCeilH = 0;
			float otherBFloorH = 0;
			float otherBCeilH = 0;

			// Add left splits
			foreach (r_local.seg_t other in seg.left)
			{
				if (!XNAIsWallRenderable(other)) continue;
				otherCeilH = other.frontsector.ceilingheight >> DoomDef.FRACBITS;
				otherFloorH = other.frontsector.floorheight >> DoomDef.FRACBITS;
				XNAAddHSplit(seg, otherFloorH);
				XNAAddHSplit(seg, otherCeilH);

				// Might have a hole so add it
				if (other.backsector != null)
				{
					otherBCeilH = other.backsector.ceilingheight >> DoomDef.FRACBITS;
					otherBFloorH = other.backsector.floorheight >> DoomDef.FRACBITS;
					XNAAddHSplit(seg, otherBFloorH);
					XNAAddHSplit(seg, otherBCeilH);
				}
			}

			// Add right splits
			foreach (r_local.seg_t other in seg.right)
			{
				if (!XNAIsWallRenderable(other)) continue;
				otherCeilH = other.frontsector.ceilingheight >> DoomDef.FRACBITS;
				otherFloorH = other.frontsector.floorheight >> DoomDef.FRACBITS;
				XNAAddHSplit(seg, otherFloorH);
				XNAAddHSplit(seg, otherCeilH);

				// Might have a hole so add it
				if (other.backsector != null)
				{
					otherBCeilH = other.backsector.ceilingheight >> DoomDef.FRACBITS;
					otherBFloorH = other.backsector.floorheight >> DoomDef.FRACBITS;
					XNAAddHSplit(seg, otherBFloorH);
					XNAAddHSplit(seg, otherBCeilH);
				}
			}

			// Sort splits
			Array.Sort(wallHSplits, 0, splitCount, SplitHComparer);

			// If a split is too big, split it more in 3 with ambientSize distance
			WallHSplit prev = wallHSplits[0];
			WallHSplit cur;
			int count = splitCount;
			for (int i = 1; i < count; ++i)
			{
				cur = wallHSplits[i];
				if (cur.height - prev.height > ambientSize)
				{
					if (cur.height - prev.height > ambientSize * 2)
					{
						// Split twice
						XNAAddHSplit(seg, prev.height + ambientSize);
						XNAAddHSplit(seg, cur.height - ambientSize);
					}
					else
					{
						// Just split once, in the middle
						XNAAddHSplit(seg, (cur.height + prev.height) * .5f);
					}
				}
			}

			// Sort again for the new splits!
			if (count != splitCount)
			{
				Array.Sort(wallHSplits, 0, splitCount, SplitHComparer);
			}

			// Render splits
			float vvv1 = 0;
			float vvv2 = 0;
			for (int i = 1; i < splitCount; ++i)
			{
				cur = wallHSplits[i];

				vvv1 = MathHelper.Lerp(vv1, vv2, prev.hPercent);
				vvv2 = MathHelper.Lerp(vv1, vv2, cur.hPercent);

				if (Settings.Default.use_deferred)
				{
					Vector3 normal = new Vector3(seg.normal, 0);

					batch.vertsPNCT.Add(new HereticXNA.r_local.VertexPositionNormalColorTexture(
						new Vector3(v2.X, v2.Y, prev.height),
						normal,
						new Color(
							(prev.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
							((len + ambientSize * prev.rightAmbient) / ambientEpsilon),
							ambientSize * prev.rightAmbient / ambientEpsilon),
						new Vector2(u2 / (float)texture.Width, vvv1 / (float)texture.Height)));

					batch.vertsPNCT.Add(new HereticXNA.r_local.VertexPositionNormalColorTexture(
						new Vector3(v1.X, v1.Y, prev.height),
						normal,
						new Color(
							(prev.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
							ambientSize * prev.leftAmbient / ambientEpsilon,
							(len + ambientSize * prev.leftAmbient) / ambientEpsilon),
						new Vector2(u1 / (float)texture.Width, vvv1 / (float)texture.Height)));

					batch.vertsPNCT.Add(new HereticXNA.r_local.VertexPositionNormalColorTexture(
						new Vector3(v2.X, v2.Y, cur.height),
						normal,
						new Color(
							(cur.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
							(len + ambientSize * cur.rightAmbient) / ambientEpsilon,
							ambientSize * cur.rightAmbient / ambientEpsilon),
						new Vector2(u2 / (float)texture.Width, vvv2 / (float)texture.Height)));

					batch.vertsPNCT.Add(new HereticXNA.r_local.VertexPositionNormalColorTexture(
						new Vector3(v1.X, v1.Y, prev.height),
						normal,
						new Color(
							(prev.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
							ambientSize * prev.leftAmbient / ambientEpsilon,
							(len + ambientSize * prev.leftAmbient) / ambientEpsilon),
						new Vector2(u1 / (float)texture.Width, vvv1 / (float)texture.Height)));

					batch.vertsPNCT.Add(new HereticXNA.r_local.VertexPositionNormalColorTexture(
						new Vector3(v1.X, v1.Y, cur.height),
						normal,
						new Color(
							(cur.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
							ambientSize * cur.leftAmbient / ambientEpsilon,
							(len + ambientSize * cur.leftAmbient) / ambientEpsilon),
						new Vector2(u1 / (float)texture.Width, vvv2 / (float)texture.Height)));

					batch.vertsPNCT.Add(new HereticXNA.r_local.VertexPositionNormalColorTexture(
						new Vector3(v2.X, v2.Y, cur.height),
						normal,
						new Color(
							(cur.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
							(len + ambientSize * cur.rightAmbient) / ambientEpsilon,
							ambientSize * cur.rightAmbient / ambientEpsilon),
						new Vector2(u2 / (float)texture.Width, vvv2 / (float)texture.Height)));
				}
				else
				{
					batch.vertsPCT.Add(new VertexPositionColorTexture(
						new Vector3(v2.X, v2.Y, prev.height),
						new Color(
							(prev.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
							((len + ambientSize * prev.rightAmbient) / ambientEpsilon),
							ambientSize * prev.rightAmbient / ambientEpsilon),
						new Vector2(u2 / (float)texture.Width, vvv1 / (float)texture.Height)));

					batch.vertsPCT.Add(new VertexPositionColorTexture(
						new Vector3(v1.X, v1.Y, prev.height),
						new Color(
							(prev.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
							ambientSize * prev.leftAmbient / ambientEpsilon,
							(len + ambientSize * prev.leftAmbient) / ambientEpsilon),
						new Vector2(u1 / (float)texture.Width, vvv1 / (float)texture.Height)));

					batch.vertsPCT.Add(new VertexPositionColorTexture(
						new Vector3(v2.X, v2.Y, cur.height),
						new Color(
							(cur.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
							(len + ambientSize * cur.rightAmbient) / ambientEpsilon,
							ambientSize * cur.rightAmbient / ambientEpsilon),
						new Vector2(u2 / (float)texture.Width, vvv2 / (float)texture.Height)));

					batch.vertsPCT.Add(new VertexPositionColorTexture(
						new Vector3(v1.X, v1.Y, prev.height),
						new Color(
							(prev.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - prev.height) / ambientEpsilon,
							ambientSize * prev.leftAmbient / ambientEpsilon,
							(len + ambientSize * prev.leftAmbient) / ambientEpsilon),
						new Vector2(u1 / (float)texture.Width, vvv1 / (float)texture.Height)));

					batch.vertsPCT.Add(new VertexPositionColorTexture(
						new Vector3(v1.X, v1.Y, cur.height),
						new Color(
							(cur.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
							ambientSize * cur.leftAmbient / ambientEpsilon,
							(len + ambientSize * cur.leftAmbient) / ambientEpsilon),
						new Vector2(u1 / (float)texture.Width, vvv2 / (float)texture.Height)));

					batch.vertsPCT.Add(new VertexPositionColorTexture(
						new Vector3(v2.X, v2.Y, cur.height),
						new Color(
							(cur.height - floorH) / ambientEpsilon,
							isSky ? 1 : (ceilH - cur.height) / ambientEpsilon,
							(len + ambientSize * cur.rightAmbient) / ambientEpsilon,
							ambientSize * cur.rightAmbient / ambientEpsilon),
						new Vector2(u2 / (float)texture.Width, vvv2 / (float)texture.Height)));
				}

				prev = cur;
			}
		}

		public static void XNARenderWallToBatches(r_local.seg_t seg, List<r_local.SectorBatch> batches)
		{
			// Render the segment
			if (seg.frontsector == null) return;

			if (!r_plane.visibleSectors.Contains(seg.frontsector))
				r_plane.visibleSectors.Add(seg.frontsector);
			GraphicsDevice device = Game1.instance.GraphicsDevice;
			Vector2 dir =
				new Vector2((float)(seg.v2.x >> DoomDef.FRACBITS), (float)(seg.v2.y >> DoomDef.FRACBITS)) -
				new Vector2((float)(seg.v1.x >> DoomDef.FRACBITS), (float)(seg.v1.y >> DoomDef.FRACBITS));
			len = dir.Length();
			dir = seg.dir;
			float uOffset = (float)(seg.sidedef.textureoffset >> DoomDef.FRACBITS);
			if (seg.left == null)
			{
				uOffset += len;
			}
			float vOffset = (float)(seg.sidedef.rowoffset >> DoomDef.FRACBITS);
			if (seg.offset != 0)
			{
				float offsetToLine = (float)(seg.offset >> DoomDef.FRACBITS);
				uOffset += offsetToLine;
			}
			u1 = uOffset;
			u2 = u1 + len;
			ceilH = seg.frontsector.ceilingheight >> DoomDef.FRACBITS;
			floorH = seg.frontsector.floorheight >> DoomDef.FRACBITS;
			isSky = (seg.sidedef.sector.ceilingpic == r_plane.skyflatnum);

			// Render bottom texture first
			int min = seg.frontsector.floorheight;
			int max = seg.frontsector.ceilingheight;
			if (seg.backsector != null)
			{
				if (max > seg.backsector.floorheight)
				{
					max = seg.backsector.floorheight;
				}
			}
			if (min < max)
			{
				int textureId = 0;
				if (max == seg.frontsector.ceilingheight)
				{
					textureId = seg.sidedef.midtexture;
					texture = Game1.instance.wallTexturesById[seg.sidedef.midtexture];
				}
				else
				{
					textureId = seg.sidedef.bottomtexture;
					texture = Game1.instance.wallTexturesById[seg.sidedef.bottomtexture];
				}

				r_local.SectorBatch batch = null;
				foreach (r_local.SectorBatch inBatch in batches)
				{
					if (inBatch.texture == texture)
					{
						batch = inBatch;
						break;
					}
				}
				if (batch == null)
				{
					batch = new r_local.SectorBatch();
					batch.texture = texture;
					batch.textureId = textureId;
					batches.Add(batch);
				}

				v1 = new Vector3(
						(float)(seg.v1.x >> DoomDef.FRACBITS),
						(float)(seg.v1.y >> DoomDef.FRACBITS),
						(float)(min >> DoomDef.FRACBITS));
				v2 = new Vector3(
						(float)(seg.v2.x >> DoomDef.FRACBITS),
						(float)(seg.v2.y >> DoomDef.FRACBITS),
						(float)(max >> DoomDef.FRACBITS));

				// Draw from top default
				vv2 = vOffset;
				vv1 = v2.Z - v1.Z + vOffset;
				if ((seg.linedef.flags & DoomData.ML_DONTPEGBOTTOM) != 0)
				{
					// Draw from the bottom
					vv1 = -vOffset;
					vv2 = v1.Z - v2.Z - vOffset;
				}

				if (Settings.Default.ambient_enabled)
					XNARenderWallPartsToBatch(seg, batch);
				else
					XNARenderFlatWallBatch(seg, batch);
			}

			// Render top texture
			if (max < seg.frontsector.ceilingheight)
			{
				min = seg.frontsector.floorheight;
				max = seg.frontsector.ceilingheight;
				if (seg.backsector != null)
				{
					if (min < seg.backsector.ceilingheight)
					{
						min = seg.backsector.ceilingheight;
					}
				}
				if (min < max)
				{
					float frontH = seg.frontsector.floorheight >> DoomDef.FRACBITS;
					float minf = min >> DoomDef.FRACBITS;
					bool needBottomAmbient = (minf <= frontH + ambientSize);

					texture = Game1.instance.wallTexturesById[seg.sidedef.toptexture];

					r_local.SectorBatch batch = null;
					foreach (r_local.SectorBatch inBatch in batches)
					{
						if (inBatch.texture == texture)
						{
							batch = inBatch;
							break;
						}
					}
					if (batch == null)
					{
						batch = new r_local.SectorBatch();
						batch.texture = texture;
						batch.textureId = seg.sidedef.toptexture;
						batches.Add(batch);
					}

					v1 = new Vector3(
							(float)(seg.v1.x >> DoomDef.FRACBITS),
							(float)(seg.v1.y >> DoomDef.FRACBITS),
							(float)(min >> DoomDef.FRACBITS));
					v2 = new Vector3(
							(float)(seg.v2.x >> DoomDef.FRACBITS),
							(float)(seg.v2.y >> DoomDef.FRACBITS),
							(float)(max >> DoomDef.FRACBITS));
					// Default draw from bottom
					vv1 = -vOffset;
					vv2 = v1.Z - v2.Z - vOffset;
					if ((seg.linedef.flags & DoomData.ML_DONTPEGTOP) != 0)
					{
						// Draw from top
						vv2 = vOffset;
						vv1 = v2.Z - v1.Z + vOffset;
					}
					if (Settings.Default.ambient_enabled)
						XNARenderWallPartsToBatch(seg, batch);
					else
						XNARenderFlatWallBatch(seg, batch);
				}
			}

			// Render the mid section if it has one
			if (seg.backsector != null)
			{
				min = Math.Max(seg.frontsector.floorheight, seg.backsector.floorheight);
				max = Math.Min(seg.frontsector.ceilingheight, seg.backsector.ceilingheight);
				if (seg.sidedef.midtexture != 0)
				{
					texture = Game1.instance.wallTexturesById[seg.sidedef.midtexture];

					r_local.SectorBatch batch = null;
					foreach (r_local.SectorBatch inBatch in batches)
					{
						if (inBatch.texture == texture)
						{
							batch = inBatch;
							break;
						}
					}
					if (batch == null)
					{
						batch = new r_local.SectorBatch();
						batch.texture = texture;
						batch.textureId = seg.sidedef.midtexture;
						batches.Add(batch);
					}

					v1 = new Vector3(
							(float)(seg.v1.x >> DoomDef.FRACBITS),
							(float)(seg.v1.y >> DoomDef.FRACBITS),
							(float)(min >> DoomDef.FRACBITS));
					v2 = new Vector3(
							(float)(seg.v2.x >> DoomDef.FRACBITS),
							(float)(seg.v2.y >> DoomDef.FRACBITS),
							(float)(max >> DoomDef.FRACBITS));
					// Draw from top default
					vv2 = vOffset;
					vv1 = v2.Z - v1.Z + vOffset;
					if ((seg.linedef.flags & DoomData.ML_DONTPEGBOTTOM) != 0)
					{
						// Draw from the bottom
						vv1 = -vOffset;
						vv2 = v1.Z - v2.Z - vOffset;
					}
					// No ambient here
					if (Settings.Default.ambient_enabled)
						XNARenderWallPartsToBatch(seg, batch);
					else
						XNARenderFlatWallBatch(seg, batch);
				}
			}
		}

		public static void XNARenderWallSplit(r_local.seg_t seg)
		{
			// Render the segment
			if (seg.frontsector == null) return;

			if (!r_plane.visibleSectors.Contains(seg.frontsector))
				r_plane.visibleSectors.Add(seg.frontsector);
			GraphicsDevice device = Game1.instance.GraphicsDevice;
			Vector2 dir =
				new Vector2((float)(seg.v2.x >> DoomDef.FRACBITS), (float)(seg.v2.y >> DoomDef.FRACBITS)) -
				new Vector2((float)(seg.v1.x >> DoomDef.FRACBITS), (float)(seg.v1.y >> DoomDef.FRACBITS));
			len = dir.Length();
			dir = seg.dir;
			float uOffset = (float)(seg.sidedef.textureoffset >> DoomDef.FRACBITS);
			if (seg.left == null)
			{
				uOffset += len;
			}
			float vOffset = (float)(seg.sidedef.rowoffset >> DoomDef.FRACBITS);
			if (seg.offset != 0)
			{
				float offsetToLine = (float)(seg.offset >> DoomDef.FRACBITS);
				uOffset += offsetToLine;
			}
			u1 = uOffset;
			u2 = u1 + len;
			float lightLevel = (float)seg.frontsector.lightlevel / 255.0f;
			ceilH = seg.frontsector.ceilingheight >> DoomDef.FRACBITS;
			floorH = seg.frontsector.floorheight >> DoomDef.FRACBITS;
			isSky = (seg.sidedef.sector.ceilingpic == r_plane.skyflatnum);
			Game1.instance.fxWall.Parameters["lightLevel"].SetValue(lightLevel);

			// Render bottom texture first
			int min = seg.frontsector.floorheight;
			int max = seg.frontsector.ceilingheight;
			if (seg.backsector != null)
			{
				if (max > seg.backsector.floorheight)
				{
					max = seg.backsector.floorheight;
				}
			}
			if (min < max)
			{
				//if (max == seg.frontsector.ceilingheight) texture = Game1.instance.wallTextures[seg.sidedef.midtexture];
				//else texture = Game1.instance.wallTextures[seg.sidedef.bottomtexture];
				if (max == seg.frontsector.ceilingheight) texture = Game1.instance.wallTexturesById[seg.sidedef.midtexture];
				else texture = Game1.instance.wallTexturesById[seg.sidedef.bottomtexture];
				Game1.instance.fxWall.Parameters["DiffuseTexture"].SetValue(texture);
				Game1.instance.fxWall.CurrentTechnique.Passes[0].Apply();
				v1 = new Vector3(
						(float)(seg.v1.x >> DoomDef.FRACBITS),
						(float)(seg.v1.y >> DoomDef.FRACBITS),
						(float)(min >> DoomDef.FRACBITS));
				v2 = new Vector3(
						(float)(seg.v2.x >> DoomDef.FRACBITS),
						(float)(seg.v2.y >> DoomDef.FRACBITS),
						(float)(max >> DoomDef.FRACBITS));

				// Draw from top default
				vv2 = vOffset;
				vv1 = v2.Z - v1.Z + vOffset;
				if ((seg.linedef.flags & DoomData.ML_DONTPEGBOTTOM) != 0)
				{
					// Draw from the bottom
					vv1 = -vOffset;
					vv2 = v1.Z - v2.Z - vOffset;
				}

				XNARenderWallParts(seg);
			}

			// Render top texture
			if (max < seg.frontsector.ceilingheight)
			{
				min = seg.frontsector.floorheight;
				max = seg.frontsector.ceilingheight;
				if (seg.backsector != null)
				{
					if (min < seg.backsector.ceilingheight)
					{
						min = seg.backsector.ceilingheight;
					}
				}
				if (min < max)
				{
					float frontH = seg.frontsector.floorheight >> DoomDef.FRACBITS;
					float minf = min >> DoomDef.FRACBITS;
					bool needBottomAmbient = (minf <= frontH + ambientSize);

					//texture = Game1.instance.wallTextures[seg.sidedef.toptexture];
					texture = Game1.instance.wallTexturesById[seg.sidedef.toptexture];
					Game1.instance.fxWall.Parameters["DiffuseTexture"].SetValue(texture);
					Game1.instance.fxWall.CurrentTechnique.Passes[0].Apply();
					v1 = new Vector3(
							(float)(seg.v1.x >> DoomDef.FRACBITS),
							(float)(seg.v1.y >> DoomDef.FRACBITS),
							(float)(min >> DoomDef.FRACBITS));
					v2 = new Vector3(
							(float)(seg.v2.x >> DoomDef.FRACBITS),
							(float)(seg.v2.y >> DoomDef.FRACBITS),
							(float)(max >> DoomDef.FRACBITS));
					// Default draw from bottom
					vv1 = -vOffset;
					vv2 = v1.Z - v2.Z - vOffset;
					if ((seg.linedef.flags & DoomData.ML_DONTPEGTOP) != 0)
					{
						// Draw from top
						vv2 = vOffset;
						vv1 = v2.Z - v1.Z + vOffset;
					}
					XNARenderWallParts(seg);
				}
			}

			// Render the mid section if it has one
			if (seg.backsector != null)
			{
				min = Math.Max(seg.frontsector.floorheight, seg.backsector.floorheight);
				max = Math.Min(seg.frontsector.ceilingheight, seg.backsector.ceilingheight);
				if (seg.sidedef.midtexture != 0)
				{
					//texture = Game1.instance.wallTextures[seg.sidedef.midtexture];
					texture = Game1.instance.wallTexturesById[seg.sidedef.midtexture];
					Game1.instance.fxWall.Parameters["DiffuseTexture"].SetValue(texture);
					Game1.instance.fxWall.CurrentTechnique.Passes[0].Apply();
					v1 = new Vector3(
							(float)(seg.v1.x >> DoomDef.FRACBITS),
							(float)(seg.v1.y >> DoomDef.FRACBITS),
							(float)(min >> DoomDef.FRACBITS));
					v2 = new Vector3(
							(float)(seg.v2.x >> DoomDef.FRACBITS),
							(float)(seg.v2.y >> DoomDef.FRACBITS),
							(float)(max >> DoomDef.FRACBITS));
					// Draw from top default
					vv2 = vOffset;
					vv1 = v2.Z - v1.Z + vOffset;
					if ((seg.linedef.flags & DoomData.ML_DONTPEGBOTTOM) != 0)
					{
						// Draw from the bottom
						vv1 = -vOffset;
						vv2 = v1.Z - v2.Z - vOffset;
					}
					// No ambient here
					XNARenderWallParts(seg);
				}
			}
		}

		public static void R_StoreWallRange(int start, int stop)
		{
			XNARenderWall(r_bsp.curline);
#if DOS
			int hyp;
			int sineval;
			uint distangle, offsetangle;
			int vtop;
			int lightnum;

			if (r_bsp.ds_p == r_local.MAXDRAWSEGS)
				return;         // don't overflow and crash

			r_bsp.sidedef = r_bsp.curline.sidedef;
			r_bsp.linedef = r_bsp.curline.linedef;

			// mark the segment as visible for auto map
			r_bsp.linedef.flags |= DoomData.ML_MAPPED;

			//
			// calculate rw_distance for scale calculation
			//
			r_segs.rw_normalangle = r_bsp.curline.angle + DoomDef.ANG90;
			offsetangle = (uint)Math.Abs(rw_normalangle - rw_angle1);
			if (offsetangle > DoomDef.ANG90)
				offsetangle = DoomDef.ANG90;
			distangle = DoomDef.ANG90 - offsetangle;
			hyp = r_main.R_PointToDist(r_bsp.curline.v1.x, r_bsp.curline.v1.y);
			sineval = tables.finesine[(int)distangle >> (int)DoomDef.ANGLETOFINESHIFT];
			rw_distance = DoomDef.FixedMul(hyp, sineval);


			r_bsp.drawsegs[r_bsp.ds_p].x1 = rw_x = start;
			r_bsp.drawsegs[r_bsp.ds_p].x2 = stop;
			r_bsp.drawsegs[r_bsp.ds_p].curline = r_bsp.curline;
			rw_stopx = stop + 1;

			//
			// calculate scale at both ends and step
			//
			r_bsp.drawsegs[r_bsp.ds_p].scale1 = rw_scale =
				r_main.R_ScaleFromGlobalAngle(r_main.viewangle + r_main.xtoviewangle[start]);
			if (stop > start)
			{
				r_bsp.drawsegs[r_bsp.ds_p].scale2 = r_main.R_ScaleFromGlobalAngle(r_main.viewangle + r_main.xtoviewangle[stop]);
				r_bsp.drawsegs[r_bsp.ds_p].scalestep = rw_scalestep =
					(r_bsp.drawsegs[r_bsp.ds_p].scale2 - rw_scale) / (stop - start);
			}
			else
			{
				//
				// try to fix the stretched line bug
				//
				r_bsp.drawsegs[r_bsp.ds_p].scale2 = r_bsp.drawsegs[r_bsp.ds_p].scale1;
			}


			//
			// calculate texture boundaries and decide if floor / ceiling marks
			// are needed
			//
			worldtop = r_bsp.frontsector.ceilingheight - r_main.viewz;
			worldbottom = r_bsp.frontsector.floorheight - r_main.viewz;

			midtexture = toptexture = bottomtexture = 0;
			maskedtexture = false;
			r_bsp.drawsegs[r_bsp.ds_p].maskedtexturecol = null;

			if (r_bsp.backsector == null)
			{
				//
				// single sided line
				//
				midtexture = r_data.texturetranslation[r_bsp.sidedef.midtexture];
				// a single sided line is terminal, so it must mark ends
				markfloor = markceiling = true;
				if ((r_bsp.linedef.flags & DoomData.ML_DONTPEGBOTTOM) != 0)
				{
					vtop = r_bsp.frontsector.floorheight + r_data.textureheight[r_bsp.sidedef.midtexture];
					rw_midtexturemid = vtop - r_main.viewz;        // bottom of texture at bottom
				}
				else
					rw_midtexturemid = worldtop;            // top of texture at top
				rw_midtexturemid += r_bsp.sidedef.rowoffset;
				r_bsp.drawsegs[r_bsp.ds_p].silhouette = r_local.SIL_BOTH;
				r_bsp.drawsegs[r_bsp.ds_p].sprtopclip = r_thing.screenheightarray;
				r_bsp.drawsegs[r_bsp.ds_p].sprbottomclip = r_thing.negonearray;
				r_bsp.drawsegs[r_bsp.ds_p].bsilheight = DoomDef.MAXINT;
				r_bsp.drawsegs[r_bsp.ds_p].tsilheight = DoomDef.MININT;
			}
			else
			{
				//
				// two sided line
				//
				r_bsp.drawsegs[r_bsp.ds_p].sprtopclip = r_bsp.drawsegs[r_bsp.ds_p].sprbottomclip = null;
				r_bsp.drawsegs[r_bsp.ds_p].silhouette = 0;
				if (r_bsp.frontsector.floorheight > r_bsp.backsector.floorheight)
				{
					r_bsp.drawsegs[r_bsp.ds_p].silhouette = r_local.SIL_BOTTOM;
					r_bsp.drawsegs[r_bsp.ds_p].bsilheight = r_bsp.frontsector.floorheight;
				}
				else if (r_bsp.backsector.floorheight > r_main.viewz)
				{
					r_bsp.drawsegs[r_bsp.ds_p].silhouette = r_local.SIL_BOTTOM;
					r_bsp.drawsegs[r_bsp.ds_p].bsilheight = DoomDef.MAXINT;
					//                      ds_p.sprbottomclip = negonearray;
				}
				if (r_bsp.frontsector.ceilingheight < r_bsp.backsector.ceilingheight)
				{
					r_bsp.drawsegs[r_bsp.ds_p].silhouette |= r_local.SIL_TOP;
					r_bsp.drawsegs[r_bsp.ds_p].tsilheight = r_bsp.frontsector.ceilingheight;
				}
				else if (r_bsp.backsector.ceilingheight < r_main.viewz)
				{
					r_bsp.drawsegs[r_bsp.ds_p].silhouette |= r_local.SIL_TOP;
					r_bsp.drawsegs[r_bsp.ds_p].tsilheight = DoomDef.MININT;
					//                      ds_p.sprtopclip = screenheightarray;
				}

				if (r_bsp.backsector.ceilingheight <= r_bsp.frontsector.floorheight)
				{
					r_bsp.drawsegs[r_bsp.ds_p].sprbottomclip = r_thing.negonearray;
					r_bsp.drawsegs[r_bsp.ds_p].bsilheight = DoomDef.MAXINT;
					r_bsp.drawsegs[r_bsp.ds_p].silhouette |= r_local.SIL_BOTTOM;
				}
				if (r_bsp.backsector.floorheight >= r_bsp.frontsector.ceilingheight)
				{
					r_bsp.drawsegs[r_bsp.ds_p].sprtopclip = r_thing.screenheightarray;
					r_bsp.drawsegs[r_bsp.ds_p].tsilheight = DoomDef.MININT;
					r_bsp.drawsegs[r_bsp.ds_p].silhouette |= r_local.SIL_TOP;
				}
				worldhigh = r_bsp.backsector.ceilingheight - r_main.viewz;
				worldlow = r_bsp.backsector.floorheight - r_main.viewz;

				// hack to allow height changes in outdoor areas
				if (r_bsp.frontsector.ceilingpic == r_plane.skyflatnum
				&& r_bsp.backsector.ceilingpic == r_plane.skyflatnum)
					worldtop = worldhigh;

				if (worldlow != worldbottom
				|| r_bsp.backsector.floorpic != r_bsp.frontsector.floorpic
				|| r_bsp.backsector.lightlevel != r_bsp.frontsector.lightlevel)
					markfloor = true;
				else
					markfloor = false;                              // same plane on both sides

				if (worldhigh != worldtop
				|| r_bsp.backsector.ceilingpic != r_bsp.frontsector.ceilingpic
				|| r_bsp.backsector.lightlevel != r_bsp.frontsector.lightlevel)
					markceiling = true;
				else
					markceiling = false;                    // same plane on both sides

				if (r_bsp.backsector.ceilingheight <= r_bsp.frontsector.floorheight
				|| r_bsp.backsector.floorheight >= r_bsp.frontsector.ceilingheight)
					markceiling = markfloor = true;         // closed door

				if (worldhigh < worldtop)
				{       // top texture
					toptexture = r_data.texturetranslation[r_bsp.sidedef.toptexture];
					if ((r_bsp.linedef.flags & DoomData.ML_DONTPEGTOP) != 0)
						rw_toptexturemid = worldtop;            // top of texture at top
					else
					{
						vtop = r_bsp.backsector.ceilingheight +
							r_data.textureheight[r_bsp.sidedef.toptexture];
						rw_toptexturemid = vtop - r_main.viewz;        // bottom of texture
					}
				}
				if (worldlow > worldbottom)
				{       // bottom texture
					bottomtexture = r_data.texturetranslation[r_bsp.sidedef.bottomtexture];
					if ((r_bsp.linedef.flags & DoomData.ML_DONTPEGBOTTOM) != 0)
					{               // bottom of texture at bottom
						rw_bottomtexturemid = worldtop;// top of texture at top
					}
					else    // top of texture at top
						rw_bottomtexturemid = worldlow;
				}
				rw_toptexturemid += r_bsp.sidedef.rowoffset;
				rw_bottomtexturemid += r_bsp.sidedef.rowoffset;

				//
				// allocate space for masked texture tables
				//
				if (r_bsp.sidedef.midtexture != 0)
				{       // masked midtexture
					maskedtexture = true;
					// [dsl] Well fuck, but who cares...
				//	r_bsp.drawsegs[r_bsp.ds_p].maskedtexturecol = maskedtexturecol = lastopening - rw_x;
					r_plane.lastopening += (short)(r_segs.rw_stopx - r_segs.rw_x);
				}
			}

			//
			// calculate rw_offset (only needed for textured lines)
			//
			segtextured = (r_segs.midtexture | r_segs.toptexture | r_segs.bottomtexture | ((r_segs.maskedtexture)?1:0)) != 0;

			if (segtextured)
			{
				offsetangle = (uint)(r_segs.rw_normalangle - r_segs.rw_angle1);
				if (offsetangle > DoomDef.ANG180)
					offsetangle = (uint)(-offsetangle);
				if (offsetangle > DoomDef.ANG90)
					offsetangle = DoomDef.ANG90;
				sineval = tables.finesine[offsetangle >> (int)DoomDef.ANGLETOFINESHIFT];
				rw_offset = DoomDef.FixedMul(hyp, sineval);
				if (rw_normalangle - rw_angle1 < DoomDef.ANG180)
					rw_offset = -rw_offset;
				rw_offset += r_bsp.sidedef.textureoffset + r_bsp.curline.offset;
				rw_centerangle = DoomDef.ANG90 + r_main.viewangle - rw_normalangle;

				//
				// calculate light table
				// use different light tables for horizontal / vertical / diagonal
				// OPTIMIZE: get rid of LIGHTSEGSHIFT globally
				if (r_main.fixedcolormap == null)
				{
					lightnum = (r_bsp.frontsector.lightlevel >> r_local.LIGHTSEGSHIFT) + r_main.extralight;
					if (r_bsp.curline.v1.y == r_bsp.curline.v2.y)
						lightnum--;
					else if (r_bsp.curline.v1.x == r_bsp.curline.v2.x)
						lightnum++;
					if (lightnum < 0)
						walllights = r_main.scalelight[0];
					else if (lightnum >= r_local.LIGHTLEVELS)
						walllights = r_main.scalelight[r_local.LIGHTLEVELS - 1];
					else
						walllights = r_main.scalelight[lightnum];
				}
			}


			//
			// if a floor / ceiling plane is on the wrong side of the view plane
			// it is definately invisible and doesn't need to be marked
			//
			if (r_bsp.frontsector.floorheight >= r_main.viewz)
				markfloor = false;                              // above view plane
			if (r_bsp.frontsector.ceilingheight <= r_main.viewz
			&& r_bsp.frontsector.ceilingpic != r_plane.skyflatnum)
				markceiling = false;                    // below view plane

			//
			// calculate incremental stepping values for texture edges
			//
			worldtop >>= 4;
			worldbottom >>= 4;

			topstep = -DoomDef.FixedMul(rw_scalestep, worldtop);
			topfrac = (r_main.centeryfrac >> 4) - DoomDef.FixedMul(worldtop, rw_scale);

			bottomstep = -DoomDef.FixedMul(rw_scalestep, worldbottom);
			bottomfrac = (r_main.centeryfrac >> 4) - DoomDef.FixedMul(worldbottom, rw_scale);

			if (r_bsp.backsector != null)
			{
				worldhigh >>= 4;
				worldlow >>= 4;

				if (worldhigh < worldtop)
				{
					pixhigh = (r_main.centeryfrac >> 4) - DoomDef.FixedMul(worldhigh, rw_scale);
					pixhighstep = -DoomDef.FixedMul(rw_scalestep, worldhigh);
				}
				if (worldlow > worldbottom)
				{
					pixlow = (r_main.centeryfrac >> 4) - DoomDef.FixedMul(worldlow, rw_scale);
					pixlowstep = -DoomDef.FixedMul(rw_scalestep, worldlow);
				}
			}

			//
			// render it
			//
			if (markceiling)
				r_plane.ceilingplane = r_plane.R_CheckPlane(r_plane.ceilingplane, rw_x, rw_stopx - 1);
			if (markfloor)
				r_plane.floorplane = r_plane.R_CheckPlane(r_plane.floorplane, rw_x, rw_stopx - 1);

			R_RenderSegLoop();

			//
			// save sprite clipping info
			//
			if (((r_bsp.drawsegs[r_bsp.ds_p].silhouette & r_local.SIL_TOP) !=0 || maskedtexture) && r_bsp.drawsegs[r_bsp.ds_p].sprtopclip == null)
			{
			//	memcpy(lastopening, ceilingclip + start, 2 * (rw_stopx - start));
			//	r_bsp.drawsegs[r_bsp.ds_p].sprtopclip = lastopening - start;
				r_plane.lastopening += (short)(rw_stopx - start);
			}
			if (((r_bsp.drawsegs[r_bsp.ds_p].silhouette & r_local.SIL_BOTTOM)!=0 || maskedtexture) && r_bsp.drawsegs[r_bsp.ds_p].sprbottomclip == null)
			{
			//	memcpy(lastopening, floorclip + start, 2 * (rw_stopx - start));
			//	r_bsp.drawsegs[r_bsp.ds_p].sprbottomclip = lastopening - start;
				r_plane.lastopening += (short)(rw_stopx - start);
			}
			if (maskedtexture && (r_bsp.drawsegs[r_bsp.ds_p].silhouette & r_local.SIL_TOP) == 0)
			{
				r_bsp.drawsegs[r_bsp.ds_p].silhouette |= r_local.SIL_TOP;
				r_bsp.drawsegs[r_bsp.ds_p].tsilheight = DoomDef.MININT;
			}
			if (maskedtexture && (r_bsp.drawsegs[r_bsp.ds_p].silhouette & r_local.SIL_BOTTOM) == 0)
			{
				r_bsp.drawsegs[r_bsp.ds_p].silhouette |= r_local.SIL_BOTTOM;
				r_bsp.drawsegs[r_bsp.ds_p].bsilheight = DoomDef.MAXINT;
			}
			r_bsp.ds_p++;
#endif
		}
	}
}
