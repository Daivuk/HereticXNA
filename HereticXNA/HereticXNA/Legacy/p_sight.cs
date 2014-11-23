using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


///////////////////////////////
// PORT - DONE
///////////////////////////////


// P_sight.c

namespace HereticXNA
{
	public static class p_sight
	{
		/*
		==============================================================================

									P_CheckSight

		This uses specialized forms of the maputils routines for optimized performance

		==============================================================================
		*/

		public static int sightzstart;			// eye z of looker
		public static int topslope, bottomslope;	// slopes to top and bottom of target

		public static int[] sightcounts = new int[3];

		/*
		==============
		=
		= PTR_SightTraverse
		=
		==============
		*/

		public static bool PTR_SightTraverse(p_local.intercept_t in_)
		{
			if (in_ == null) return true;

			r_local.line_t li;
			int slope;

			li = in_.line;

			//
			// crosses a two sided line
			//
			p_maputl.P_LineOpening(li);

			if (p_maputl.openbottom >= p_maputl.opentop)	// quick test for totally closed doors
				return false;	// stop

			if (li.frontsector.floorheight != li.backsector.floorheight)
			{
				slope = d_main.FixedDiv(p_maputl.openbottom - sightzstart, in_.frac);
				if (slope > bottomslope)
					bottomslope = slope;
			}

			if (li.frontsector.ceilingheight != li.backsector.ceilingheight)
			{
				slope = d_main.FixedDiv(p_maputl.opentop - sightzstart, in_.frac);
				if (slope < topslope)
					topslope = slope;
			}

			if (topslope <= bottomslope)
				return false;	// stop

			return true;	// keep going
		}


		/*
		==================
		=
		= P_SightBlockLinesIterator
		=
		===================
		*/

		public static bool P_SightBlockLinesIterator(int x, int y)
		{
			int offset;
			short list;
			r_local.line_t ld;
			int s1, s2;
			p_local.divline_t dl = new p_local.divline_t();

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

				s1 = p_maputl.P_PointOnDivlineSide(ld.v1.x, ld.v1.y, p_maputl.trace);
				s2 = p_maputl.P_PointOnDivlineSide(ld.v2.x, ld.v2.y, p_maputl.trace);
				if (s1 == s2)
					continue;		// line isn't crossed
				p_maputl.P_MakeDivline(ld, dl);
				s1 = p_maputl.P_PointOnDivlineSide(p_maputl.trace.x, p_maputl.trace.y, dl);
				s2 = p_maputl.P_PointOnDivlineSide(p_maputl.trace.x + p_maputl.trace.dx, p_maputl.trace.y + p_maputl.trace.dy, dl);
				if (s1 == s2)
					continue;		// line isn't crossed

				// try to early out the check
				if (ld.backsector == null)
					return false;	// stop checking

				// store the line for later intersection testing
				p_maputl.intercepts[p_maputl.intercept_p].line = ld;
				p_maputl.intercept_p++;
			}

			return true;		// everything was checked
		}

		/*
		====================
		=
		= P_SightTraverseIntercepts
		=
		= Returns true if the traverser function returns true for all lines
		====================
		*/

		public static bool P_SightTraverseIntercepts()
		{
			int count;
			int dist;
			p_local.intercept_t scan, in_;
			p_local.divline_t dl = new p_local.divline_t();

			count = p_maputl.intercept_p;
			//
			// calculate intercept distance
			//
			for (int i = 0; i < p_maputl.intercept_p; ++i)
			{
				scan = p_maputl.intercepts[i];
				p_maputl.P_MakeDivline(scan.line, dl);
				scan.frac = p_maputl.P_InterceptVector(p_maputl.trace, dl);
			}

			//
			// go through in order
			//	
			in_ = null;			// shut up compiler warning

			while ((count--) != 0)
			{
				dist = DoomDef.MAXINT;
				for (int i = 0; i < p_maputl.intercept_p; ++i)
				{
					scan = p_maputl.intercepts[i];
					if (scan.frac < dist)
					{
						dist = scan.frac;
						in_ = scan;
					}
				}

				if (in_ == null) continue;

				if (!PTR_SightTraverse(in_))
					return false;			// don't bother going farther
				in_.frac = DoomDef.MAXINT;
			}

			return true;		// everything was traversed
		}

		/*
		==================
		=
		= P_SightPathTraverse
		=
		= Traces a line from x1,y1 to x2,y2, calling the traverser function for each
		= Returns true if the traverser function returns true for all lines
		==================
		*/

		public static bool P_SightPathTraverse(int x1, int y1, int x2, int y2)
		{
			int xt1, yt1, xt2, yt2;
			int xstep, ystep;
			int partial;
			int xintercept, yintercept;
			int mapx, mapy, mapxstep, mapystep;
			int count;

			r_main.validcount++;
			p_maputl.intercept_p = 0;

			if (((x1 - p_setup.bmaporgx) & (p_local.MAPBLOCKSIZE - 1)) == 0)
				x1 += DoomDef.FRACUNIT;				// don't side exactly on a line
			if (((y1 - p_setup.bmaporgy) & (p_local.MAPBLOCKSIZE - 1)) == 0)
				y1 += DoomDef.FRACUNIT;				// don't side exactly on a line
			p_maputl.trace.x = x1;
			p_maputl.trace.y = y1;
			p_maputl.trace.dx = x2 - x1;
			p_maputl.trace.dy = y2 - y1;

			x1 -= p_setup.bmaporgx;
			y1 -= p_setup.bmaporgy;
			xt1 = x1 >> p_local.MAPBLOCKSHIFT;
			yt1 = y1 >> p_local.MAPBLOCKSHIFT;

			x2 -= p_setup.bmaporgx;
			y2 -= p_setup.bmaporgy;
			xt2 = x2 >> p_local.MAPBLOCKSHIFT;
			yt2 = y2 >> p_local.MAPBLOCKSHIFT;

			// points should never be out of bounds, but check once instead of
			// each block
			if (xt1 < 0 || yt1 < 0 || xt1 >= p_setup.bmapwidth || yt1 >= p_setup.bmapheight
			|| xt2 < 0 || yt2 < 0 || xt2 >= p_setup.bmapwidth || yt2 >= p_setup.bmapheight)
				return false;

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
				if (!P_SightBlockLinesIterator(mapx, mapy))
				{
					sightcounts[1]++;
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
			// couldn't early out, so go through the sorted list
			//
			sightcounts[2]++;

			return P_SightTraverseIntercepts();
		}

		/*
		=====================
		=
		= P_CheckSight
		=
		= Returns true if a straight line between t1 and t2 is unobstructed
		= look from eyes of t1 to any part of t2
		=
		=====================
		*/

		public static bool P_CheckSight(DoomDef.mobj_t t1, DoomDef.mobj_t t2)
		{
			int s1, s2;
			int pnum, bytenum, bitnum;

			//
			// check for trivial rejection
			//
			s1 = Array.IndexOf(p_setup.sectors, t1.subsector.sector);
			s2 = Array.IndexOf(p_setup.sectors, t2.subsector.sector);
			pnum = s1 * p_setup.numsectors + s2;
			bytenum = pnum >> 3;
			bitnum = 1 << (pnum & 7);

			if ((p_setup.rejectmatrix[bytenum] & bitnum) != 0)
			{
				sightcounts[0]++;
				return false;		// can't possibly be connected
			}

			//
			// check precisely
			//		
			sightzstart = t1.z + t1.height - (t1.height >> 2);
			topslope = (t2.z + t2.height) - sightzstart;
			bottomslope = (t2.z) - sightzstart;

			return P_SightPathTraverse(t1.x, t1.y, t2.x, t2.y);
		}
	}
}
