using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// R_bsp.c

namespace HereticXNA
{
	public static class r_bsp
	{
		public static r_local.seg_t curline;
		public static r_local.side_t sidedef;
		public static r_local.line_t linedef;
		public static r_local.sector_t frontsector, backsector;


		public static r_local.drawseg_t[] drawsegs = new r_local.drawseg_t[r_local.MAXDRAWSEGS] {
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
	new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),new r_local.drawseg_t(),
};
		public static int ds_p;

		/*
		====================
		=
		= R_ClearDrawSegs
		=
		====================
		*/

		public static void R_ClearDrawSegs()
		{
			ds_p = 0;
		}

		//=============================================================================


		/*
		===============================================================================
		=
		= ClipWallSegment
		=
		= Clips the given range of columns and includes it in the new clip list
		===============================================================================
		*/

		public class cliprange_t
		{
			public int first, last;
		} ;

		public const int MAXSEGS = 32;

		public static cliprange_t[] solidsegs = new cliprange_t[MAXSEGS] {
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t(),
	new cliprange_t()
};
		public static int newend;
		public static void R_ClipSolidWallSegment(int first, int last)
		{
			// We ignore culling because they work per pixels, and we get poly disapearing
			r_segs.XNARenderWall(r_bsp.curline);
#if DOS
			int next;
			int start;

			// find the first range that touches the range (adjacent pixels are touching)
			start = 0; // = solidsegs
			while (solidsegs[start].last < first - 1)
				start++;

			if (first < solidsegs[start].first)
			{
				if (last < solidsegs[start].first - 1)
				{	// post is entirely visible (above start), so insert a new clippost
					r_segs.R_StoreWallRange(first, last);
					next = newend;
					newend++;
					while (next != start)
					{
						solidsegs[next].first = solidsegs[next - 1].first;
						solidsegs[next].last = solidsegs[next - 1].last;
						next--;
					}
					solidsegs[next].first = first;
					solidsegs[next].last = last;
					return;
				}

				// there is a fragment above *start
				r_segs.R_StoreWallRange(first, solidsegs[start].first - 1);
				solidsegs[start].first = first;		// adjust the clip size
			}

			if (last <= solidsegs[start].last)
				return;			// bottom contained in start

			next = start;
			while (last >= solidsegs[next + 1].first - 1)
			{
				// there is a fragment between two posts
				r_segs.R_StoreWallRange(solidsegs[next].last + 1, solidsegs[next + 1].first - 1);
				next++;
				if (last <= solidsegs[next].last)
				{	// bottom is contained in next
					solidsegs[start].last = solidsegs[next].last;	// adjust the clip size
					goto crunch;
				}
			}

			// there is a fragment after *next
			r_segs.R_StoreWallRange(solidsegs[next].last + 1, last);
			solidsegs[start].last = last;		// adjust the clip size


		// remove start+1 to next from the clip list,
		// because start now covers their area
		crunch:
			if (solidsegs[next] == solidsegs[start])
				return;			// post just extended past the bottom of one post

			while (next++ != newend)
			{	// remove a post
				++start;
				solidsegs[start].first = solidsegs[next].first;
				solidsegs[start].last = solidsegs[next].last;
			}
			newend = start + 1;
#endif
		}

		/*
		===============================================================================
		=
		= R_ClipPassWallSegment
		=
		= Clips the given range of columns, but does not includes it in the clip list
		===============================================================================
		*/

		public static void R_ClipPassWallSegment(int first, int last)
		{
			int start;

			// find the first range that touches the range (adjacent pixels are touching)
			start = 0; // = solidsegs;
			while (solidsegs[start].last < first - 1)
				start++;

			if (first < solidsegs[start].first)
			{
				if (last < solidsegs[start].first - 1)
				{	// post is entirely visible (above start)
					r_segs.R_StoreWallRange(first, last);
					return;
				}

				// there is a fragment above *start
				r_segs.R_StoreWallRange(first, solidsegs[start].first - 1);
			}

			if (last <= solidsegs[start].last)
				return;			// bottom contained in start

			while (last >= solidsegs[start + 1].first - 1)
			{
				// there is a fragment between two posts
				r_segs.R_StoreWallRange(solidsegs[start].last + 1, solidsegs[start + 1].first - 1);
				start++;
				if (last <= solidsegs[start].last)
					return;
			}

			// there is a fragment after *next
			r_segs.R_StoreWallRange(solidsegs[start].last + 1, last);
		}


		/*
====================
=
= R_ClearClipSegs
=
====================
*/

		public static void R_ClearClipSegs()
		{
			solidsegs[0].first = -0x7fffffff;
			solidsegs[0].last = -1;
			solidsegs[1].first = r_draw.viewwidth;
			solidsegs[1].last = 0x7fffffff;
			newend = 2;
		}

		//=============================================================================

		/*
		======================
		=
		= R_AddLine
		=
		= Clips the given segment and adds any visible pieces to the line list
		=
		======================
		*/

		public static void R_AddLine(r_local.seg_t line)
		{
			int x1, x2;
			uint angle1, angle2, span, tspan;

			curline = line;

			// OPTIMIZE: quickly reject orthogonal back sides

			angle1 = r_main.R_PointToAngle(line.v1.x, line.v1.y);
			angle2 = r_main.R_PointToAngle(line.v2.x, line.v2.y);

			//
			// clip to view edges
			// OPTIMIZE: make constant out of 2*clipangle (FIELDOFVIEW)
			span = angle1 - angle2;
			if (span >= DoomDef.ANG180)
				return;		// back side

			r_segs.rw_angle1 = (int)angle1;		// global angle needed by segcalc
			angle1 -= r_main.viewangle;
			angle2 -= r_main.viewangle;

			tspan = angle1 + r_main.clipangle;
			if (tspan > 2 * r_main.clipangle)
			{
				tspan -= 2 * r_main.clipangle;
				if (tspan >= span)
					return;	// totally off the left edge
				angle1 = r_main.clipangle;
			}
			tspan = r_main.clipangle - angle2;
			if (tspan > 2 * r_main.clipangle)
			{
				tspan -= 2 * r_main.clipangle;
				if (tspan >= span)
					return;	// totally off the left edge
				angle2 = (uint)(-r_main.clipangle);
			}

			//
			// the seg is in the view range, but not necessarily visible
			//
			angle1 = (uint)((int)(angle1 + DoomDef.ANG90) >> (int)DoomDef.ANGLETOFINESHIFT);
			angle2 = (uint)((int)(angle2 + DoomDef.ANG90) >> (int)DoomDef.ANGLETOFINESHIFT);
			x1 = r_main.viewangletox[angle1];
			x2 = r_main.viewangletox[angle2];
			if (x1 == x2)
				return;				// does not cross a pixel

			backsector = line.backsector;

			// [dsl] There were gotos here. Yup
			if (backsector == null)
			{
				R_ClipSolidWallSegment(x1, x2 - 1);		// single sided line
				return;
			}

			if (backsector.ceilingheight <= frontsector.floorheight
			|| backsector.floorheight >= frontsector.ceilingheight)
			{
				R_ClipSolidWallSegment(x1, x2 - 1);		// closed door
				return;
			}

			if (backsector.ceilingheight != frontsector.ceilingheight
			|| backsector.floorheight != frontsector.floorheight)
			{
				R_ClipPassWallSegment(x1, x2 - 1);		// window
				return;
			}

			// reject empty lines used for triggers and special events
			if (backsector.ceilingpic == frontsector.ceilingpic
			&& backsector.floorpic == frontsector.floorpic
			&& backsector.lightlevel == frontsector.lightlevel
			&& curline.sidedef.
			midtexture == 0)
				return;

			R_ClipPassWallSegment(x1, x2 - 1);
		}

		//============================================================================


		/*
		===============================================================================
		=
		= R_CheckBBox
		=
		= Returns true if some part of the bbox might be visible
		=
		===============================================================================
		*/

		public static int[,] checkcoord = new int[12, 4]{
{3,0, 2,1},
{3,0, 2,0},
{3,1, 2,0},
{0,0,0,0},
{2,0, 2,1},
{0,0,0,0},
{3,1, 3,0},
{0,0,0,0},
{2,0, 3,1},
{2,1, 3,1},
{2,1, 3,0},
{0,0,0,0} };


		public static bool R_CheckBBox(int[] bspcoord)
		{
			int boxx, boxy, boxpos;
			int x1, y1, x2, y2;
			uint angle1, angle2, span, tspan;
			int start;
			int sx1, sx2;


			// find the corners of the box that define the edges from current viewpoint
			if (r_main.viewx <= bspcoord[(int)DoomData.eUnknownEnumType2.BOXLEFT])
				boxx = 0;
			else if (r_main.viewx < bspcoord[(int)DoomData.eUnknownEnumType2.BOXRIGHT])
				boxx = 1;
			else
				boxx = 2;

			if (r_main.viewy >= bspcoord[(int)DoomData.eUnknownEnumType2.BOXTOP])
				boxy = 0;
			else if (r_main.viewy > bspcoord[(int)DoomData.eUnknownEnumType2.BOXBOTTOM])
				boxy = 1;
			else
				boxy = 2;

			boxpos = (boxy << 2) + boxx;
			if (boxpos == 5)
				return true;

			x1 = bspcoord[checkcoord[boxpos, 0]];
			y1 = bspcoord[checkcoord[boxpos, 1]];
			x2 = bspcoord[checkcoord[boxpos, 2]];
			y2 = bspcoord[checkcoord[boxpos, 3]];



			//
			// check clip list for an open space
			//	
			angle1 = r_main.R_PointToAngle(x1, y1) - r_main.viewangle;
			angle2 = r_main.R_PointToAngle(x2, y2) - r_main.viewangle;

			span = angle1 - angle2;
			if (span >= DoomDef.ANG180)
				return true;	// sitting on a line
			tspan = angle1 + r_main.clipangle;
			if (tspan > 2 * r_main.clipangle)
			{
				tspan -= 2 * r_main.clipangle;
				if (tspan >= span)
					return false;	// totally off the left edge
				angle1 = r_main.clipangle;
			}
			tspan = r_main.clipangle - angle2;
			if (tspan > 2 * r_main.clipangle)
			{
				tspan -= 2 * r_main.clipangle;
				if (tspan >= span)
					return false;	// totally off the left edge
				angle2 = (uint)(-r_main.clipangle);
			}


			// find the first clippost that touches the source post (adjacent pixels are touching)
			angle1 = (uint)((int)(angle1 + DoomDef.ANG90) >> (int)DoomDef.ANGLETOFINESHIFT);
			angle2 = (uint)((int)(angle2 + DoomDef.ANG90) >> (int)DoomDef.ANGLETOFINESHIFT);
			sx1 = r_main.viewangletox[angle1];
			sx2 = r_main.viewangletox[angle2];
			if (sx1 == sx2)
				return false;				// does not cross a pixel
			sx2--;

			start = 0;
			while (solidsegs[start].last < sx2)
				start++;
			if (sx1 >= solidsegs[start].first && sx2 <= solidsegs[start].last)
				return false;	// the clippost contains the new span

			return true;
		}

		/*
		================
		=
		= R_Subsector
		=
		= Draw one or more segments
		================
		*/

		public static void R_Subsector(int num)
		{
			int count;
			int line;
			r_local.subsector_t sub;

			r_main.sscount++;
			sub = p_setup.subsectors[num];
			frontsector = sub.sector;
			count = sub.numlines;
			line = sub.firstline;

			if (frontsector.floorheight < r_main.viewz)
				r_plane.floorplane = r_plane.R_FindPlane(frontsector.floorheight,
				frontsector.floorpic, frontsector.lightlevel,
				frontsector.special);
			else
				r_plane.floorplane = null;
			if (frontsector.ceilingheight > r_main.viewz
			|| frontsector.ceilingpic == r_plane.skyflatnum)
				r_plane.ceilingplane = r_plane.R_FindPlane(frontsector.ceilingheight,
				frontsector.ceilingpic, frontsector.lightlevel, 0);
			else
				r_plane.ceilingplane = null;

			r_thing.R_AddSprites(frontsector);

			while ((count--) != 0)
			{
				r_bsp.R_AddLine(p_setup.segs[line]);
				line++;
			}
		}

		/*
		===============================================================================
		=
		= RenderBSPNode
		=
		===============================================================================
		*/

		public static void R_RenderBSPNode(int bspnum)
		{
			r_local.node_t bsp;
			int side;

			if ((bspnum & DoomData.NF_SUBSECTOR) != 0)
			{
				if (bspnum == -1)
					R_Subsector(0);
				else
					R_Subsector(bspnum & (~DoomData.NF_SUBSECTOR));
				return;
			}

			bsp = p_setup.nodes[bspnum];

			//
			// decide which side the view point is on
			//
			side = r_main.R_PointOnSide(r_main.viewx, r_main.viewy, bsp);

			R_RenderBSPNode(bsp.children[side]); // recursively divide front space

			if (R_CheckBBox(bsp.bbox[side ^ 1]))	// possibly divide back space
				R_RenderBSPNode(bsp.children[side ^ 1]);
		}
	}
}
