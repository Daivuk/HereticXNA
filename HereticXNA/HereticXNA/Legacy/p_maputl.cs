using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

// P_maputl.c

namespace HereticXNA
{
	public static class p_maputl
	{



		/*
		===================
		=
		= P_AproxDistance
		=
		= Gives an estimation of distance (not exact)
		=
		===================
		*/

		public static int P_AproxDistance(int dx, int dy)
		{
			dx = Math.Abs(dx);
			dy = Math.Abs(dy);
			if (dx < dy)
				return dx + dy - (dx >> 1);
			return dx + dy - (dy >> 1);
		}

		/*
		==================
		=
		= P_PointOnLineSide
		=
		= Returns 0 or 1
		==================
		*/

		public static int P_PointOnLineSide(int x, int y, r_local.line_t line)
		{
			int dx, dy;
			int left, right;

			if (line.dx == 0)
			{
				if (x <= line.v1.x)
					return line.dy > 0 ? 1 : 0;
				return line.dy < 0 ? 1 : 0;
			}
			if (line.dy == 0)
			{
				if (y <= line.v1.y)
					return line.dx < 0 ? 1 : 0;
				return line.dx > 0 ? 1 : 0;
			}

			dx = (x - line.v1.x);
			dy = (y - line.v1.y);

			left = DoomDef.FixedMul(line.dy >> DoomDef.FRACBITS, dx);
			right = DoomDef.FixedMul(dy, line.dx >> DoomDef.FRACBITS);

			if (right < left)
				return 0;		// front side
			return 1;			// back side
		}

		/*
		=================
		=
		= P_BoxOnLineSide
		=
		= Considers the line to be infinite
		= Returns side 0 or 1, -1 if box crosses the line
		=================
		*/

		public static int P_BoxOnLineSide(int[] tmbox, r_local.line_t ld)
		{
			int p1 = 0, p2 = 0;

			switch (ld.slopetype)
			{
				case r_local.slopetype_t.ST_HORIZONTAL:
					p1 = tmbox[(int)DoomData.eUnknownEnumType2.BOXTOP] > ld.v1.y ? 1 : 0;
					p2 = tmbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] > ld.v1.y ? 1 : 0;
					if (ld.dx < 0)
					{
						p1 ^= 1;
						p2 ^= 1;
					}
					break;
				case r_local.slopetype_t.ST_VERTICAL:
					p1 = tmbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] < ld.v1.x ? 1 : 0;
					p2 = tmbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] < ld.v1.x ? 1 : 0;
					if (ld.dy < 0)
					{
						p1 ^= 1;
						p2 ^= 1;
					}
					break;
				case r_local.slopetype_t.ST_POSITIVE:
					p1 = p_maputl.P_PointOnLineSide(tmbox[(int)DoomData.eUnknownEnumType2.BOXLEFT], tmbox[(int)DoomData.eUnknownEnumType2.BOXTOP], ld);
					p2 = p_maputl.P_PointOnLineSide(tmbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT], tmbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM], ld);
					break;
				case r_local.slopetype_t.ST_NEGATIVE:
					p1 = p_maputl.P_PointOnLineSide(tmbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT], tmbox[(int)DoomData.eUnknownEnumType2.BOXTOP], ld);
					p2 = p_maputl.P_PointOnLineSide(tmbox[(int)DoomData.eUnknownEnumType2.BOXLEFT], tmbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM], ld);
					break;
			}

			if (p1 == p2)
				return p1;
			return -1;
		}

		/*
		==================
		=
		= P_PointOnDivlineSide
		=
		= Returns 0 or 1
		==================
		*/

		public static int P_PointOnDivlineSide(int x, int y, p_local.divline_t line)
		{
			int dx, dy;
			int left, right;

			if (line.dx == 0)
			{
				if (x <= line.x)
					return (line.dy > 0) ? 1 : 0;
				return (line.dy < 0) ? 1 : 0;
			}
			if (line.dy == 0)
			{
				if (y <= line.y)
					return (line.dx < 0) ? 1 : 0;
				return (line.dx > 0) ? 1 : 0;
			}

			dx = (x - line.x);
			dy = (y - line.y);

			// try to quickly decide by looking at sign bits
			if (((line.dy ^ line.dx ^ dx ^ dy) & 0x80000000) != 0)
			{
				if (((line.dy ^ dx) & 0x80000000) != 0)
					return 1;	// (left is negative)
				return 0;
			}

			left = DoomDef.FixedMul(line.dy >> 8, dx >> 8);
			right = DoomDef.FixedMul(dy >> 8, line.dx >> 8);

			if (right < left)
				return 0;		// front side
			return 1;			// back side
		}


		/*
		==============
		=
		= P_MakeDivline
		=
		==============
		*/

		public static void P_MakeDivline(r_local.line_t li, p_local.divline_t dl)
		{
			dl.x = li.v1.x;
			dl.y = li.v1.y;
			dl.dx = li.dx;
			dl.dy = li.dy;
		}

		/*
		===============
		=
		= P_InterceptVector
		=
		= Returns the fractional intercept point along the first divline
		=
		= This is only called by the addthings and addlines traversers
		===============
		*/

		public static int P_InterceptVector(p_local.divline_t v2, p_local.divline_t v1)
		{
			int frac, num, den;

			den = DoomDef.FixedMul(v1.dy >> 8, v2.dx) - DoomDef.FixedMul(v1.dx >> 8, v2.dy);
			if (den == 0)
				return 0;
			num = DoomDef.FixedMul((v1.x - v2.x) >> 8, v1.dy) +
				DoomDef.FixedMul((v2.y - v1.y) >> 8, v1.dx);
			frac = d_main.FixedDiv(num, den);

			return frac;
		}

		/*
		==================
		=
		= P_LineOpening
		=
		= Sets opentop and openbottom to the window through a two sided line
		= OPTIMIZE: keep this precalculated
		==================
		*/

		public static int opentop, openbottom, openrange;
		public static int lowfloor;

		public static void P_LineOpening(r_local.line_t linedef)
		{
			r_local.sector_t front, back;

			if (linedef.sidenum[1] == -1)
			{	// single sided line
				openrange = 0;
				return;
			}

			front = linedef.frontsector;
			back = linedef.backsector;

			if (front.ceilingheight < back.ceilingheight)
				opentop = front.ceilingheight;
			else
				opentop = back.ceilingheight;
			if (front.floorheight > back.floorheight)
			{
				openbottom = front.floorheight;
				lowfloor = back.floorheight;
			}
			else
			{
				openbottom = back.floorheight;
				lowfloor = front.floorheight;
			}

			openrange = opentop - openbottom;
		}

		/*
		===============================================================================

								THING POSITION SETTING

		===============================================================================
		*/

		/*
		===================
		=
		= P_UnsetThingPosition 
		=
		= Unlinks a thing from block map and sectors
		=
		===================
		*/

		public static void P_UnsetThingPosition(DoomDef.mobj_t thing)
		{
			int blockx, blocky;

			if ((thing.flags & DoomDef.MF_NOSECTOR) == 0)
			{	// inert things don't need to be in blockmap
				// unlink from subsector
				if (thing.snext != null)
					thing.snext.sprev = thing.sprev;
				if (thing.sprev != null)
					thing.sprev.snext = thing.snext;
				else
					thing.subsector.sector.thinglist = thing.snext;
			}

			if ((thing.flags & DoomDef.MF_NOBLOCKMAP) == 0)
			{	// inert things don't need to be in blockmap
				// unlink from block map
				if (thing.bnext != null)
					thing.bnext.bprev = thing.bprev;
				if (thing.bprev != null)
					thing.bprev.bnext = thing.bnext;
				else
				{
					blockx = (thing.x - p_setup.bmaporgx) >> p_local.MAPBLOCKSHIFT;
					blocky = (thing.y - p_setup.bmaporgy) >> p_local.MAPBLOCKSHIFT;
					if (blockx >= 0 && blockx < p_setup.bmapwidth
					&& blocky >= 0 && blocky < p_setup.bmapheight)
						p_setup.blocklinks[blocky * p_setup.bmapwidth + blockx] = thing.bnext;
				}
			}
		}

		/*
		===================
		=
		= P_SetThingPosition 
		=
		= Links a thing into both a block and a subsector based on it's x y
		= Sets thing.subsector properly
		=
		===================
		*/

		public static void P_SetThingPosition(DoomDef.mobj_t thing)
		{
			r_local.subsector_t ss;
			r_local.sector_t sec;
			int blockx, blocky;
			DoomDef.mobj_t link;
			int linki;

			//
			// link into subsector
			//
			ss = r_main.R_PointInSubsector(thing.x, thing.y);
			thing.subsector = ss;
			if ((thing.flags & DoomDef.MF_NOSECTOR) == 0)
			{	// invisible things don't go into the sector links
				sec = ss.sector;

				thing.sprev = null;
				thing.snext = sec.thinglist;
				if (sec.thinglist != null)
					sec.thinglist.sprev = thing;
				sec.thinglist = thing;
			}

			//
			// link into blockmap
			//
			if ((thing.flags & DoomDef.MF_NOBLOCKMAP) == 0)
			{	// inert things don't need to be in blockmap		
				blockx = (thing.x - p_setup.bmaporgx) >> p_local.MAPBLOCKSHIFT;
				blocky = (thing.y - p_setup.bmaporgy) >> p_local.MAPBLOCKSHIFT;
				if (blockx >= 0 && blockx < p_setup.bmapwidth && blocky >= 0 && blocky < p_setup.bmapheight)
				{
					linki = blocky * p_setup.bmapwidth + blockx;
					link = p_setup.blocklinks[linki];
					thing.bprev = null;
					thing.bnext = link;
					if (link != null)
						link.bprev = thing;
					p_setup.blocklinks[linki] = thing;
				}
				else
				{	// thing is off the map
					thing.bnext = thing.bprev = null;
				}
			}
		}


		/*
		===============================================================================

								BLOCK MAP ITERATORS

		For each line/thing in the given mapblock, call the passed function.
		If the function returns false, exit with false without checking anything else.

		===============================================================================
		*/

		/*
		==================
		=
		= P_BlockLinesIterator
		=
		= The validcount flags are used to avoid checking lines
		= that are marked in multiple mapblocks, so increment validcount before
		= the first call to P_BlockLinesIterator, then make one or more calls to it
		===================
		*/

		public class P_BlockLinesIterator_delegate
		{
			public virtual bool func(r_local.line_t line) { return false; }
		}

		public static bool P_BlockLinesIterator(int x, int y, P_BlockLinesIterator_delegate in_delegate)
		{
			int offset;
			short list;
			r_local.line_t ld;

			if (x < 0 || y < 0 || x >= p_setup.bmapwidth || y >= p_setup.bmapheight)
				return true;
			offset = y * p_setup.bmapwidth + x;

			offset = p_setup.blockmaplump[p_setup.blockmap + offset];

			for (int i = offset; ; i++)
			{
				list = p_setup.blockmaplump[i];
				if (list == -1) break;
				ld = p_setup.lines[list];
				if (ld.validcount == r_main.validcount)
					continue;		// line has already been checked
				ld.validcount = r_main.validcount;

				if (!in_delegate.func(ld))
					return false;
			}

			return true;		// everything was checked
		}

		/*
		==================
		=
		= P_BlockThingsIterator
		=
		==================
		*/
		public class P_BlockThingsIterator_delegate
		{
			public virtual bool func(DoomDef.mobj_t mob) { return false; }
		}
		public static bool P_BlockThingsIterator(int x, int y, P_BlockThingsIterator_delegate in_delegate)
		{
			DoomDef.mobj_t mobj;

			if (x < 0 || y < 0 || x >= p_setup.bmapwidth || y >= p_setup.bmapheight)
				return true;

			for (mobj = p_setup.blocklinks[y * p_setup.bmapwidth + x]; mobj != null; mobj = mobj.bnext)
				if (!in_delegate.func(mobj))
					return false;

			return true;
		}

		/*
		===============================================================================

							INTERCEPT ROUTINES

		===============================================================================
		*/

		public static p_local.intercept_t[] intercepts = new p_local.intercept_t[p_local.MAXINTERCEPTS] {
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),
	new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t(),new p_local.intercept_t()
};
		public static int intercept_p;
		public static p_local.divline_t trace = new p_local.divline_t();
		public static bool earlyout;
		public static int ptflags;

		/*
		==================
		=
		= PIT_AddLineIntercepts
		=
		= Looks for lines in the given block that intercept the given trace
		= to add to the intercepts list
		= A line is crossed if its endpoints are on opposite sides of the trace
		= Returns true if earlyout and a solid line hit
		==================
		*/

		public class cPIT_AddLineIntercepts : P_BlockLinesIterator_delegate
		{
			public override bool func(r_local.line_t ld)
			{
				int s1, s2;
				int frac;
				p_local.divline_t dl = new p_local.divline_t();

				// avoid precision problems with two routines
				if (trace.dx > DoomDef.FRACUNIT * 16 || trace.dy > DoomDef.FRACUNIT * 16
				|| trace.dx < -DoomDef.FRACUNIT * 16 || trace.dy < -DoomDef.FRACUNIT * 16)
				{
					s1 = p_maputl.P_PointOnDivlineSide(ld.v1.x, ld.v1.y, trace);
					s2 = p_maputl.P_PointOnDivlineSide(ld.v2.x, ld.v2.y, trace);
				}
				else
				{
					s1 = P_PointOnLineSide(trace.x, trace.y, ld);
					s2 = P_PointOnLineSide(trace.x + trace.dx, trace.y + trace.dy, ld);
				}
				if (s1 == s2)
					return true;		// line isn't crossed

				//
				// hit the line
				//
				p_maputl.P_MakeDivline(ld, dl);
				frac = p_maputl.P_InterceptVector(trace, dl);
				if (frac < 0)
					return true;		// behind source

				// try to early out the check
				if (earlyout && frac < DoomDef.FRACUNIT && ld.backsector == null)
					return false;	// stop checking

				intercepts[intercept_p].frac = frac;
				intercepts[intercept_p].isaline = true;
				intercepts[intercept_p].line = ld;
				intercept_p++;

				return true;		// continue
			}
		}

		public static cPIT_AddLineIntercepts PIT_AddLineIntercepts = new cPIT_AddLineIntercepts();


/*
==================
=
= PIT_AddThingIntercepts
=
==================
*/
public class cPIT_AddThingIntercepts : P_BlockThingsIterator_delegate
{
	public override bool func(DoomDef.mobj_t thing)
	{
		int x1, y1, x2, y2;
		int s1, s2;
		bool tracepositive;
		p_local.divline_t dl = new p_local.divline_t();
		int frac;

		tracepositive = (trace.dx ^ trace.dy) > 0;

		// check a corner to corner crossection for hit

		if (tracepositive)
		{
			x1 = thing.x - thing.radius;
			y1 = thing.y + thing.radius;

			x2 = thing.x + thing.radius;
			y2 = thing.y - thing.radius;
		}
		else
		{
			x1 = thing.x - thing.radius;
			y1 = thing.y - thing.radius;

			x2 = thing.x + thing.radius;
			y2 = thing.y + thing.radius;
		}
		s1 = p_maputl.P_PointOnDivlineSide(x1, y1, trace);
		s2 = p_maputl.P_PointOnDivlineSide(x2, y2, trace);
		if (s1 == s2)
			return true;	// line isn't crossed

		dl.x = x1;
		dl.y = y1;
		dl.dx = x2 - x1;
		dl.dy = y2 - y1;
		frac = P_InterceptVector(trace, dl);
		if (frac < 0)
			return true;		// behind source
		intercepts[intercept_p].frac = frac;
		intercepts[intercept_p].isaline = false;
		intercepts[intercept_p].thing = thing;
		intercept_p++;

		return true;			// keep going
	}
}

public static cPIT_AddThingIntercepts PIT_AddThingIntercepts = new cPIT_AddThingIntercepts();

/*
====================
=
= P_TraverseIntercepts
=
= Returns true if the traverser function returns true for all lines
====================
*/

public static bool P_TraverseIntercepts(P_PathTraverse_delegate func, int maxfrac)
{
	int				count;
	int			dist;
	p_local.intercept_t		scan, in_;
	
	count = intercept_p;
	in_ = null;			// shut up compiler warning
	
	while ((count--) != 0)
	{
		dist = DoomDef.MAXINT;
		for (int i = 0; i < intercept_p; ++i)
		{
			scan = intercepts[i];
			if (scan.frac < dist)
			{
				dist = scan.frac;
				in_ = scan;
			}
		}
			
		if (dist > maxfrac)
			return true;			// checked everything in range		

		if (!func.function(in_))
			return false;			// don't bother going farther
		in_.frac = DoomDef.MAXINT;
	}
	
	return true;		// everything was traversed
}

/*
==================
=
= P_PathTraverse
=
= Traces a line from x1,y1 to x2,y2, calling the traverser function for each
= Returns true if the traverser function returns true for all lines
==================
*/
		public class P_PathTraverse_delegate
		{
			public virtual bool function(p_local.intercept_t in_) { return false; }
		}

		public static bool P_PathTraverse(int x1, int y1, int x2, int y2,
			int flags, P_PathTraverse_delegate trav)
		{
			int xt1, yt1, xt2, yt2;
			int xstep, ystep;
			int partial;
			int xintercept, yintercept;
			int mapx, mapy, mapxstep, mapystep;
			int count;

			earlyout = (flags & p_local.PT_EARLYOUT) != 0;

			r_main.validcount++;
			intercept_p = 0;

			if (((x1 - p_setup.bmaporgx) & (p_local.MAPBLOCKSIZE - 1)) == 0)
				x1 += DoomDef.FRACUNIT;				// don't side exactly on a line
			if (((y1 - p_setup.bmaporgy) & (p_local.MAPBLOCKSIZE - 1)) == 0)
				y1 += DoomDef.FRACUNIT;				// don't side exactly on a line
			trace.x = x1;
			trace.y = y1;
			trace.dx = x2 - x1;
			trace.dy = y2 - y1;

			x1 -= p_setup.bmaporgx;
			y1 -= p_setup.bmaporgy;
			xt1 = x1 >> p_local.MAPBLOCKSHIFT;
			yt1 = y1 >> p_local.MAPBLOCKSHIFT;

			x2 -= p_setup.bmaporgx;
			y2 -= p_setup.bmaporgy;
			xt2 = x2 >> p_local.MAPBLOCKSHIFT;
			yt2 = y2 >> p_local.MAPBLOCKSHIFT;

			if (xt2 > xt1)
			{
				mapxstep = 1;
				partial = DoomDef.FRACUNIT - ((x1 >> p_local.MAPBTOFRAC) & (DoomDef.FRACUNIT - 1));
				ystep = d_main.FixedDiv(y2 - y1, Math.Abs(x2 - x1));
			}
			else if (xt2 < xt1)
			{
				mapxstep = -1;
				partial = (x1 >> p_local.MAPBTOFRAC) & (DoomDef.FRACUNIT - 1);
				ystep = d_main.FixedDiv(y2 - y1, Math.Abs(x2 - x1));
			}
			else
			{
				mapxstep = 0;
				partial = DoomDef.FRACUNIT;
				ystep = 256 * DoomDef.FRACUNIT;
			}
			yintercept = (y1 >> p_local.MAPBTOFRAC) + DoomDef.FixedMul(partial, ystep);


			if (yt2 > yt1)
			{
				mapystep = 1;
				partial = DoomDef.FRACUNIT - ((y1 >> p_local.MAPBTOFRAC) & (DoomDef.FRACUNIT - 1));
				xstep = d_main.FixedDiv(x2 - x1, Math.Abs(y2 - y1));
			}
			else if (yt2 < yt1)
			{
				mapystep = -1;
				partial = (y1 >> p_local.MAPBTOFRAC) & (DoomDef.FRACUNIT - 1);
				xstep = d_main.FixedDiv(x2 - x1, Math.Abs(y2 - y1));
			}
			else
			{
				mapystep = 0;
				partial = DoomDef.FRACUNIT;
				xstep = 256 * DoomDef.FRACUNIT;
			}
			xintercept = (x1 >> p_local.MAPBTOFRAC) + DoomDef.FixedMul(partial, xstep);


			//
			// step through map blocks
			// Count is present to prevent a round off error from skipping the break
			mapx = xt1;
			mapy = yt1;

			for (count = 0; count < 64; count++)
			{
				if ((flags & p_local.PT_ADDLINES) != 0)
				{
					if (!P_BlockLinesIterator(mapx, mapy, PIT_AddLineIntercepts))
						return false;	// early out
				}
				if ((flags & p_local.PT_ADDTHINGS) != 0)
				{
					if (!P_BlockThingsIterator(mapx, mapy, PIT_AddThingIntercepts))
						return false;	// early out
				}

				if (mapx == xt2 && mapy == yt2)
					break;

				if ((yintercept >> DoomDef.FRACBITS) == mapy)
				{
					yintercept += ystep;
					mapx += mapxstep;
				}
				else if ((xintercept >> DoomDef.FRACBITS) == mapx)
				{
					xintercept += xstep;
					mapy += mapystep;
				}
			}


			//
			// go through the sorted list
			//
			return p_maputl.P_TraverseIntercepts(trav, DoomDef.FRACUNIT);
		}


		// [dsl] Following calls added for shadow map helpers, and culling in deferred rendering
		static int checkId = 0;
		public static void GatherSectorsInRadius(Vector2 pos, r_local.sector_t sector, float radius, ref List<r_local.sector_t> out_sectors)
		{
			++checkId;
			sector.checkId = checkId;
			out_sectors.Add(sector);
			GatherSectorsInRadiusIter(pos, sector, radius, ref out_sectors);
		}

		public static void GatherSectorsInRadiusIter(Vector2 pos, r_local.sector_t sector, float radius, ref List<r_local.sector_t> out_sectors)
		{
			for (int i = 0; i < sector.linecount; ++i)
			{
				r_local.line_t li = p_setup.linebuffer[sector.linesi + i];
				GatherSectorsInRadiusIterThroughLine(pos, sector, li, radius, ref out_sectors);
			}
		}

		public static void GatherSectorsInRadiusIterThroughLine(
			Vector2 pos, 
			r_local.sector_t sector, 
			r_local.line_t li, 
			float radius, 
			ref List<r_local.sector_t> out_sectors)
		{
			r_local.sector_t newSector = null;
			Vector2 liNormal = Vector2.Zero;
			Vector2 v = new Vector2(
				li.v1.x >> DoomDef.FRACBITS,
				li.v1.y >> DoomDef.FRACBITS);
			Vector2 w = new Vector2(
				li.v2.x >> DoomDef.FRACBITS,
				li.v2.y >> DoomDef.FRACBITS);
			if (li.frontsector == sector)
			{
				if (li.backsector != null)
				{
					newSector = li.backsector;
					Vector2 dir = w - v;
					dir.Normalize();
					liNormal.X = dir.Y;
					liNormal.Y = -dir.X;
				}
			}
			else if (li.backsector == sector)
			{
				newSector = li.frontsector;
				Vector2 dir = v - w;
				dir.Normalize();
				liNormal.X = dir.Y;
				liNormal.Y = -dir.X;
			}
			if (newSector == null) return;
			if (newSector.checkId == checkId) return; // Already checked

			float dotWithNormal = Vector2.Dot(
				liNormal, v - pos);
			if (dotWithNormal > 0) return;

			// Is out line in the radius?
			float dis = minimum_distance(v, w, pos);
			if (dis > radius) return;

			newSector.checkId = checkId;
			out_sectors.Add(newSector);
			GatherSectorsInRadiusIter(pos, newSector, radius, ref out_sectors);
		}

		// http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
		public static float minimum_distance(Vector2 v, Vector2 w, Vector2 p)
		{
			// Return minimum distance between line segment vw and point p
			float l2 = Vector2.DistanceSquared(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
			if (l2 == 0.0) return Vector2.Distance(p, v);   // v == w case
			// Consider the line extending the segment, parameterized as v + t (w - v).
			// We find projection of point p onto the line. 
			// It falls where t = [(p-v) . (w-v)] / |w-v|^2
			float t = Vector2.Dot(p - v, w - v) / l2;
			if (t < 0.0) return Vector2.Distance(p, v);       // Beyond the 'v' end of the segment
			else if (t > 1.0) return Vector2.Distance(p, w);  // Beyond the 'w' end of the segment
			Vector2 projection = v + t * (w - v);  // Projection falls on the segment
			return Vector2.Distance(p, projection);
		}
	}
}
