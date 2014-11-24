using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

///////////////////////////////
// PORT - DONE
///////////////////////////////

// P_map.c

namespace HereticXNA
{
	class p_map
	{


		/*
		===============================================================================

		NOTES:


		===============================================================================
		*/

		/*
		===============================================================================

		mobj_t NOTES

		mobj_ts are used to tell the refresh where to draw an image, tell the world simulation when objects are contacted, and tell the sound driver how to position a sound.

		The refresh uses the next and prev links to follow lists of things in sectors as they are being drawn.  The sprite, frame, and angle elements determine which patch_t is used to draw the sprite if it is visible.  The sprite and frame values are allmost allways set from state_t structures.  The statescr.exe utility generates the states.h and states.c files that contain the sprite/frame numbers from the statescr.txt source file.  The xyz origin point represents a point at the bottom middle of the sprite (between the feet of a biped).  This is the default origin position for patch_ts grabbed with lumpy.exe.  A walking creature will have its z equal to the floor it is standing on.
 
		The sound code uses the x,y, and subsector fields to do stereo positioning of any sound effited by the mobj_t.

		The play simulation uses the blocklinks, x,y,z, radius, height to determine when mobj_ts are touching each other, touching lines in the map, or hit by trace lines (gunshots, lines of sight, etc). The mobj_t.flags element has various bit flags used by the simulation.


		Every mobj_t is linked into a single sector based on it's origin coordinates.
		The subsector_t is found with R_PointInSubsector(x,y), and the sector_t can be found with subsector.sector.  The sector links are only used by the rendering code,  the play simulation does not care about them at all.

		Any mobj_t that needs to be acted upon be something else in the play world (block movement, be shot, etc) will also need to be linked into the blockmap.  If the thing has the MF_NOBLOCK flag set, it will not use the block links. It can still interact with other things, but only as the instigator (missiles will run into other things, but nothing can run into a missile).   Each block in the grid is 128*128 units, and knows about every line_t that it contains a piece of, and every interactable mobj_t that has it's origin contained.  

		A valid mobj_t is a mobj_t that has the proper subsector_t filled in for it's xy coordinates and is linked into the subsector's sector or has the MF_NOSECTOR flag set (the subsector_t needs to be valid even if MF_NOSECTOR is set), and is linked into a blockmap block or has the MF_NOBLOCKMAP flag set.  Links should only be modified by the P_[Un]SetThingPosition () functions.  Do not change the MF_NO? flags while a thing is valid.


		===============================================================================
		*/

		public static int[] tmbbox = new int[4];
		public static DoomDef.mobj_t tmthing;
		public static int tmflags;
		public static int tmx, tmy;


		public static bool floatok;				// if true, move would be ok if
		// within tmfloorz - tmceilingz

		public static int tmfloorz, tmceilingz, tmdropoffz;

		// keep track of the line that lowers the ceiling, so missiles don't explode
		// against sky hack walls
		public static r_local.line_t ceilingline;
		// keep track of special lines as they are hit, but don't process them
		// until the move is proven valid
		public const int MAXSPECIALCROSS = 8;
		public static r_local.line_t[] spechit = new r_local.line_t[MAXSPECIALCROSS];
		public static int numspechit;

		public static DoomDef.mobj_t onmobj; //generic global onmobj...used for landing on pods/players

		/*
		===============================================================================

							TELEPORT MOVE
 
		===============================================================================
		*/

		/*
		==================
		=
		= PIT_StompThing
		=
		==================
		*/
		public class cPIT_StompThing : p_maputl.P_BlockThingsIterator_delegate
		{
			public override bool func(DoomDef.mobj_t thing)
			{
				int blockdist;

				if ((thing.flags & DoomDef.MF_SHOOTABLE) == 0)
					return true;

				blockdist = thing.radius + tmthing.radius;
				if (Math.Abs(thing.x - tmx) >= blockdist || Math.Abs(thing.y - tmy) >= blockdist)
					return true;		// didn't hit it

				if (thing == tmthing)
					return true;		// don't clip against self

				if ((tmthing.flags2 & DoomDef.MF2_TELESTOMP) == 0)
				{ // Not allowed to stomp things
					return (false);
				}

				p_inter.P_DamageMobj(thing, tmthing, tmthing, 10000);

				return true;
			}
		}

		public static cPIT_StompThing PIT_StompThing = new cPIT_StompThing();

		/*
		===================
		=
		= P_TeleportMove
		=
		===================
		*/

		public static bool P_TeleportMove(DoomDef.mobj_t thing, int x, int y)
		{
			int xl, xh, yl, yh, bx, by;
			r_local.subsector_t newsubsec;

			//
			// kill anything occupying the position
			//

			tmthing = thing;
			tmflags = thing.flags;

			tmx = x;
			tmy = y;

			tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] = y + tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = y - tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = x + tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] = x - tmthing.radius;

			newsubsec = r_main.R_PointInSubsector(x, y);
			ceilingline = null;

			//
			// the base floor / ceiling is from the subsector that contains the
			// point.  Any contacted lines the step closer together will adjust them
			//
			tmfloorz = tmdropoffz = newsubsec.sector.floorheight;
			tmceilingz = newsubsec.sector.ceilingheight;

			r_main.validcount++;
			numspechit = 0;

			//
			// stomp on any things contacted
			//
			xl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] - p_setup.bmaporgx - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			xh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] - p_setup.bmaporgx + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			yl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] - p_setup.bmaporgy - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			yh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] - p_setup.bmaporgy + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;

			for (bx = xl; bx <= xh; bx++)
				for (by = yl; by <= yh; by++)
					if (!p_maputl.P_BlockThingsIterator(bx, by, p_map.PIT_StompThing))
						return false;

			//
			// the move is ok, so link the thing into its new position
			//	
			p_maputl.P_UnsetThingPosition(thing);

			thing.floorz = tmfloorz;
			thing.ceilingz = tmceilingz;
			thing.x = x;
			thing.y = y;

			p_maputl.P_SetThingPosition(thing);

			return true;
		}

		/*
===============================================================================

					MOVEMENT ITERATOR FUNCTIONS
 
===============================================================================
*/

		/*
		==================
		=
		= PIT_CheckLine
		=
		= Adjusts tmfloorz and tmceilingz as lines are contacted
		==================
		*/


		public class PIT_CheckLine : p_maputl.P_BlockLinesIterator_delegate
		{
			public override bool func(r_local.line_t ld)
			{
				if (tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] <= ld.bbox[(int)DoomData.eUnknownEnumType2.BOXLEFT]
					|| tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] >= ld.bbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT]
					|| tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] <= ld.bbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM]
					|| tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] >= ld.bbox[(int)DoomData.eUnknownEnumType2.BOXTOP])
				{
					return (true);
				}
				if (p_maputl.P_BoxOnLineSide(tmbbox, ld) != -1)
				{
					return (true);
				}

				// a line has been hit
				/*
				=
				= The moving thing's destination position will cross the given line.
				= If this should not be allowed, return false.
				= If the line is special, keep track of it to process later if the move
				= 	is proven ok.  NOTE: specials are NOT sorted by order, so two special lines
				= 	that are only 8 pixels apart could be crossed in either order.
				*/

				if (ld.backsector == null)
				{ // One sided line
					if ((tmthing.flags & DoomDef.MF_MISSILE) != 0)
					{ // Missiles can trigger impact specials
						if (ld.special != 0)
						{
							spechit[numspechit] = ld;
							numspechit++;
						}
					}
					return false;
				}
				if ((tmthing.flags & DoomDef.MF_MISSILE) == 0)
				{
					if ((ld.flags & DoomData.ML_BLOCKING) != 0)
					{ // Explicitly blocking everything
						return (false);
					}
					if (tmthing.player == null && (ld.flags & DoomData.ML_BLOCKMONSTERS) != 0
						&& tmthing.type != info.mobjtype_t.MT_POD)
					{ // Block monsters only
						return (false);
					}
				}
				p_maputl.P_LineOpening(ld);		// set openrange, opentop, openbottom
				// adjust floor / ceiling heights
				if (p_maputl.opentop < tmceilingz)
				{
					tmceilingz = p_maputl.opentop;
					ceilingline = ld;
				}
				if (p_maputl.openbottom > tmfloorz)
				{
					tmfloorz = p_maputl.openbottom;
				}
				if (p_maputl.lowfloor < tmdropoffz)
				{
					tmdropoffz = p_maputl.lowfloor;
				}
				if (ld.special != 0)
				{ // Contacted a special line, add it to the list
					spechit[numspechit] = ld;
					numspechit++;
				}
				return (true);
			}
		}

		//---------------------------------------------------------------------------
		//
		// FUNC PIT_CheckThing
		//
		//---------------------------------------------------------------------------
		public class PIT_CheckThing : p_maputl.P_BlockThingsIterator_delegate
		{
			public override bool func(DoomDef.mobj_t thing)
			{
				int blockdist;
				bool solid;
				int damage;

				if ((thing.flags & (DoomDef.MF_SOLID | DoomDef.MF_SPECIAL | DoomDef.MF_SHOOTABLE)) == 0)
				{ // Can't hit thing
					return (true);
				}
				blockdist = thing.radius + tmthing.radius;
				if (Math.Abs(thing.x - tmx) >= blockdist || Math.Abs(thing.y - tmy) >= blockdist)
				{ // Didn't hit thing
					return (true);
				}
				if (thing == tmthing)
				{ // Don't clip against self
					return (true);
				}
				if ((tmthing.flags2 & DoomDef.MF2_PASSMOBJ) != 0)
				{ // check if a mobj passed over/under another object
					if ((tmthing.type == info.mobjtype_t.MT_IMP || tmthing.type == info.mobjtype_t.MT_WIZARD)
						&& (thing.type == info.mobjtype_t.MT_IMP || thing.type == info.mobjtype_t.MT_WIZARD))
					{ // don't let imps/wizards fly over other imps/wizards
						return false;
					}
					if (tmthing.z > thing.z + thing.height
						&& (thing.flags & DoomDef.MF_SPECIAL) == 0)
					{
						return (true);
					}
					else if (tmthing.z + tmthing.height < thing.z
						&& (thing.flags & DoomDef.MF_SPECIAL) == 0)
					{ // under thing
						return (true);
					}
				}
				// Check for skulls slamming into things
				if ((tmthing.flags & DoomDef.MF_SKULLFLY) != 0)
				{
					damage = ((m_misc.P_Random() % 8) + 1) * tmthing.damage;
					p_inter.P_DamageMobj(thing, tmthing, tmthing, damage);
					tmthing.flags &= ~DoomDef.MF_SKULLFLY;
					tmthing.momx = tmthing.momy = tmthing.momz = 0;
					p_mobj.P_SetMobjState(tmthing, (info.statenum_t)tmthing.infol.seestate);
					return (false);
				}
				// Check for missile
				if ((tmthing.flags & DoomDef.MF_MISSILE) != 0)
				{
					// Check for passing through a ghost
					if ((thing.flags & DoomDef.MF_SHADOW) != 0 && (tmthing.flags2 & DoomDef.MF2_THRUGHOST) != 0)
					{
						return (true);
					}
					// Check if it went over / under
					if (tmthing.z > thing.z + thing.height)
					{ // Over thing
						return (true);
					}
					if (tmthing.z + tmthing.height < thing.z)
					{ // Under thing
						return (true);
					}
					if (tmthing.target != null && tmthing.target.type == thing.type)
					{ // Don't hit same species as originator
						if (thing == tmthing.target)
						{ // Don't missile self
							return (true);
						}
						if (thing.type != info.mobjtype_t.MT_PLAYER)
						{ // Hit same species as originator, explode, no damage
							return (false);
						}
					}
					if ((thing.flags & DoomDef.MF_SHOOTABLE) == 0)
					{ // Didn't do any damage
						return (thing.flags & DoomDef.MF_SOLID) == 0;
					}
					if ((tmthing.flags2 & DoomDef.MF2_RIP) != 0)
					{
						if ((thing.flags & DoomDef.MF_NOBLOOD) == 0)
						{ // Ok to spawn some blood
							p_mobj.P_RipperBlood(tmthing);
						}
						i_ibm.S_StartSound(tmthing, (int)sounds.sfxenum_t.sfx_ripslop);
						damage = ((m_misc.P_Random() & 3) + 2) * tmthing.damage;
						p_inter.P_DamageMobj(thing, tmthing, tmthing.target, damage);
						if ((thing.flags2 & DoomDef.MF2_PUSHABLE) != 0
							&& (tmthing.flags2 & DoomDef.MF2_CANNOTPUSH) == 0)
						{ // Push thing
							thing.momx += tmthing.momx >> 2;
							thing.momy += tmthing.momy >> 2;
						}
						numspechit = 0;
						return (true);
					}
					// Do damage
					damage = ((m_misc.P_Random() % 8) + 1) * tmthing.damage;
					if (damage != 0)
					{
						if ((thing.flags & DoomDef.MF_NOBLOOD) == 0 && m_misc.P_Random() < 192)
						{
							p_mobj.P_BloodSplatter(tmthing.x, tmthing.y, tmthing.z, thing);
						}
						p_inter.P_DamageMobj(thing, tmthing, tmthing.target, damage);
					}
					return (false);
				}
				if ((thing.flags2 & DoomDef.MF2_PUSHABLE) != 0 && (tmthing.flags2 & DoomDef.MF2_CANNOTPUSH) == 0)
				{ // Push thing
					thing.momx += tmthing.momx >> 2;
					thing.momy += tmthing.momy >> 2;
				}
				// Check for special thing
				if ((thing.flags & DoomDef.MF_SPECIAL) != 0)
				{
					solid = (thing.flags & DoomDef.MF_SOLID) != 0;
					if ((tmflags & DoomDef.MF_PICKUP) != 0)
					{ // Can be picked up by tmthing
						p_inter.P_TouchSpecialThing(thing, tmthing); // Can remove thing
					}
					return (!solid);
				}
				return ((thing.flags & DoomDef.MF_SOLID) == 0);
			}
		}
		//---------------------------------------------------------------------------
		//
		// PIT_CheckOnmobjZ
		//
		//---------------------------------------------------------------------------
		public class PIT_CheckOnmobjZ : p_maputl.P_BlockThingsIterator_delegate
		{
			public override bool func(DoomDef.mobj_t thing)
			{
				int blockdist;

				if ((thing.flags & (DoomDef.MF_SOLID | DoomDef.MF_SPECIAL | DoomDef.MF_SHOOTABLE)) == 0)
				{ // Can't hit thing
					return (true);
				}
				blockdist = thing.radius + tmthing.radius;
				if (Math.Abs(thing.x - tmx) >= blockdist || Math.Abs(thing.y - tmy) >= blockdist)
				{ // Didn't hit thing
					return (true);
				}
				if (thing == tmthing)
				{ // Don't clip against self
					return (true);
				}
				if (tmthing.z > thing.z + thing.height)
				{
					return (true);
				}
				else if (tmthing.z + tmthing.height < thing.z)
				{ // under thing
					return (true);
				}
				if ((thing.flags & DoomDef.MF_SOLID) != 0)
				{
					onmobj = thing;
				}
				return ((thing.flags & DoomDef.MF_SOLID) == 0);
			}
		}

		/*
		===============================================================================

								MOVEMENT CLIPPING
 
		===============================================================================
		*/

		//----------------------------------------------------------------------------
		//
		// FUNC P_TestMobjLocation
		//
		// Returns true if the mobj is not blocked by anything at its current
		// location, otherwise returns false.
		//
		//----------------------------------------------------------------------------

		public static bool P_TestMobjLocation(DoomDef.mobj_t mobj)
		{
			int flags;

			flags = mobj.flags;
			mobj.flags &= ~DoomDef.MF_PICKUP;
			if (P_CheckPosition(mobj, mobj.x, mobj.y))
			{ // XY is ok, now check Z
				mobj.flags = flags;
				if ((mobj.z < mobj.floorz)
					|| (mobj.z + mobj.height > mobj.ceilingz))
				{ // Bad Z
					return (false);
				}
				return (true);
			}
			mobj.flags = flags;
			return (false);
		}

		/*
		==================
		=
		= P_CheckPosition
		=
		= This is purely informative, nothing is modified (except things picked up)

		in:
		a mobj_t (can be valid or invalid)
		a position to be checked (doesn't need to be related to the mobj_t.x,y)

		during:
		special things are touched if MF_PICKUP
		early out on solid lines?

		out:
		newsubsec
		floorz
		ceilingz
		tmdropoffz		the lowest point contacted (monsters won't move to a dropoff)
		speciallines[]
		numspeciallines

		==================
		*/

		public static bool P_CheckPosition(DoomDef.mobj_t thing, int x, int y)
		{
			int xl, xh, yl, yh, bx, by;
			r_local.subsector_t newsubsec;

			tmthing = thing;
			tmflags = thing.flags;

			tmx = x;
			tmy = y;

			tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] = y + tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = y - tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = x + tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] = x - tmthing.radius;

			newsubsec = r_main.R_PointInSubsector(x, y);
			ceilingline = null;

			//
			// the base floor / ceiling is from the subsector that contains the
			// point.  Any contacted lines the step closer together will adjust them
			//
			tmfloorz = tmdropoffz = newsubsec.sector.floorheight;
			tmceilingz = newsubsec.sector.ceilingheight;

			r_main.validcount++;
			numspechit = 0;

			if ((tmflags & DoomDef.MF_NOCLIP) != 0)
				return true;

			//
			// check things first, possibly picking things up
			// the bounding box is extended by MAXRADIUS because mobj_ts are grouped
			// into mapblocks based on their origin point, and can overlap into adjacent
			// blocks by up to MAXRADIUS units
			//
			xl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] - p_setup.bmaporgx - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			xh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] - p_setup.bmaporgx + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			yl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] - p_setup.bmaporgy - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			yh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] - p_setup.bmaporgy + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;

			for (bx = xl; bx <= xh; bx++)
				for (by = yl; by <= yh; by++)
					if (!p_maputl.P_BlockThingsIterator(bx, by, new PIT_CheckThing()))
						return false;
			//
			// check lines
			//
			xl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] - p_setup.bmaporgx) >> p_local.MAPBLOCKSHIFT;
			xh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] - p_setup.bmaporgx) >> p_local.MAPBLOCKSHIFT;
			yl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] - p_setup.bmaporgy) >> p_local.MAPBLOCKSHIFT;
			yh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] - p_setup.bmaporgy) >> p_local.MAPBLOCKSHIFT;

			for (bx = xl; bx <= xh; bx++)
				for (by = yl; by <= yh; by++)
					if (!p_maputl.P_BlockLinesIterator(bx, by, new PIT_CheckLine()))
						return false;

			return true;
		}
		//=============================================================================
		//
		// P_CheckOnmobj(mobj_t *thing)
		//
		// 		Checks if the new Z position is legal
		//=============================================================================

		public static DoomDef.mobj_t P_CheckOnmobj(DoomDef.mobj_t thing)
		{
			int xl, xh, yl, yh, bx, by;
			r_local.subsector_t newsubsec;
			int x;
			int y;
			DoomDef.mobj_t oldmo = new DoomDef.mobj_t();

			x = thing.x;
			y = thing.y;
			tmthing = thing;
			tmflags = thing.flags;
			oldmo.set(thing); // save the old mobj before the fake zmovement
			P_FakeZMovement(tmthing);

			tmx = x;
			tmy = y;

			tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] = y + tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = y - tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = x + tmthing.radius;
			tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] = x - tmthing.radius;

			newsubsec = r_main.R_PointInSubsector(x, y);
			ceilingline = null;

			//
			// the base floor / ceiling is from the subsector that contains the
			// point.  Any contacted lines the step closer together will adjust them
			//
			tmfloorz = tmdropoffz = newsubsec.sector.floorheight;
			tmceilingz = newsubsec.sector.ceilingheight;

			r_main.validcount++;
			numspechit = 0;

			if ((tmflags & DoomDef.MF_NOCLIP) != 0)
				return null;

			//
			// check things first, possibly picking things up
			// the bounding box is extended by MAXRADIUS because mobj_ts are grouped
			// into mapblocks based on their origin point, and can overlap into adjacent
			// blocks by up to MAXRADIUS units
			//
			xl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] - p_setup.bmaporgx - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			xh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] - p_setup.bmaporgx + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			yl = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] - p_setup.bmaporgy - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
			yh = (tmbbox[(int)DoomData.eUnknownEnumType2.BOXTOP] - p_setup.bmaporgy + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;

			for (bx = xl; bx <= xh; bx++)
				for (by = yl; by <= yh; by++)
					if (!p_maputl.P_BlockThingsIterator(bx, by, new PIT_CheckOnmobjZ()))
					{
						tmthing.set(oldmo);
						return onmobj;
					}
			tmthing.set(oldmo);
			return null;
		}

		//=============================================================================
		//
		// P_FakeZMovement
		//
		// 		Fake the zmovement so that we can check if a move is legal
		//=============================================================================

		public static void P_FakeZMovement(DoomDef.mobj_t mo)
		{
			int dist;
			int delta;
			//
			// adjust height
			//
			mo.z += mo.momz;
			if ((mo.flags & DoomDef.MF_FLOAT) != 0 && mo.target != null)
			{	// float down towards target if too close
				if ((mo.flags & DoomDef.MF_SKULLFLY) == 0 && (mo.flags & DoomDef.MF_INFLOAT) == 0)
				{
					dist = p_maputl.P_AproxDistance(mo.x - mo.target.x, mo.y - mo.target.y);
					delta = (mo.target.z + (mo.height >> 1)) - mo.z;
					if (delta < 0 && dist < -(delta * 3))
						mo.z -= p_local.FLOATSPEED;
					else if (delta > 0 && dist < (delta * 3))
						mo.z += p_local.FLOATSPEED;
				}
			}
			if (mo.player != null && (mo.flags2 & DoomDef.MF2_FLY) != 0 && !(mo.z <= mo.floorz)
				&& (p_tick.leveltime & 2) != 0)
			{
				mo.z += tables.finesine[(DoomDef.FINEANGLES / 20 * p_tick.leveltime >> 2) & DoomDef.FINEMASK];
			}

			//
			// clip movement
			//
			if (mo.z <= mo.floorz)
			{ // Hit the floor
				mo.z = mo.floorz;
				if (mo.momz < 0)
				{
					mo.momz = 0;
				}
				if ((mo.flags & DoomDef.MF_SKULLFLY) != 0)
				{ // The skull slammed into something
					mo.momz = -mo.momz;
				}
				if (mo.infol.crashstate != 0 && (mo.flags & DoomDef.MF_CORPSE) != 0)
				{
					return;
				}
			}
			else if ((mo.flags2 & DoomDef.MF2_LOGRAV) != 0)
			{
				if (mo.momz == 0)
					mo.momz = -(p_local.GRAVITY >> 3) * 2;
				else
					mo.momz -= p_local.GRAVITY >> 3;
			}
			else if ((mo.flags & DoomDef.MF_NOGRAVITY) == 0)
			{
				if (mo.momz == 0)
					mo.momz = -p_local.GRAVITY * 2;
				else
					mo.momz -= p_local.GRAVITY;
			}

			if (mo.z + mo.height > mo.ceilingz)
			{	// hit the ceiling
				if (mo.momz > 0)
					mo.momz = 0;
				mo.z = mo.ceilingz - mo.height;
				if ((mo.flags & DoomDef.MF_SKULLFLY) != 0)
				{	// the skull slammed into something
					mo.momz = -mo.momz;
				}
			}
		}

		//==========================================================================
		//
		// CheckMissileImpact
		//
		//==========================================================================

		public static void CheckMissileImpact(DoomDef.mobj_t mobj)
		{
			int i;

			if (numspechit == 0 || (mobj.flags & DoomDef.MF_MISSILE) == 0 || mobj.target == null)
			{
				return;
			}
			if (mobj.target.player == null)
			{
				return;
			}
			for (i = numspechit - 1; i >= 0; i--)
			{
				p_spec.P_ShootSpecialLine(mobj.target, spechit[i]);
			}
		}

		/*
		===================
		=
		= P_TryMove
		=
		= Attempt to move to a new position, crossing special lines unless MF_TELEPORT
		= is set
		=
		===================
		*/

		public static bool P_TryMove(DoomDef.mobj_t thing, int x, int y)
		{
			int oldx, oldy;
			int side, oldside;
			r_local.line_t ld;

			floatok = false;
			if (!P_CheckPosition(thing, x, y))
			{ // Solid wall or thing
				p_map.CheckMissileImpact(thing);
				return false;
			}
			if ((thing.flags & DoomDef.MF_NOCLIP) == 0)
			{
				if (tmceilingz - tmfloorz < thing.height)
				{ // Doesn't fit
					p_map.CheckMissileImpact(thing);
					return false;
				}
				floatok = true;
				if ((thing.flags & DoomDef.MF_TELEPORT) == 0
					&& tmceilingz - thing.z < thing.height
					&& (thing.flags2 & DoomDef.MF2_FLY) == 0)
				{ // mobj must lower itself to fit
					p_map.CheckMissileImpact(thing);
					return false;
				}
				if ((thing.flags2 & DoomDef.MF2_FLY) != 0)
				{
					if (thing.z + thing.height > tmceilingz)
					{
						thing.momz = -8 * DoomDef.FRACUNIT;
						return false;
					}
					else if (thing.z < tmfloorz && tmfloorz - tmdropoffz > 24 * DoomDef.FRACUNIT)
					{
						thing.momz = 8 * DoomDef.FRACUNIT;
						return false;
					}
				}
				if (((thing.flags & DoomDef.MF_TELEPORT) == 0)
					// The Minotaur floor fire (MT_MNTRFX2) can step up any amount
					&& thing.type != info.mobjtype_t.MT_MNTRFX2
					&& tmfloorz - thing.z > 24 * DoomDef.FRACUNIT)
				{ // Too big a step up
					p_map.CheckMissileImpact(thing);
					return false;
				}
				if ((thing.flags & DoomDef.MF_MISSILE) != 0 && tmfloorz > thing.z)
				{
					p_map.CheckMissileImpact(thing);
				}
				if ((thing.flags & (DoomDef.MF_DROPOFF | DoomDef.MF_FLOAT)) == 0
					&& tmfloorz - tmdropoffz > 24 * DoomDef.FRACUNIT)
				{ // Can't move over a dropoff
					return false;
				}
			}

			//
			// the move is ok, so link the thing into its new position
			//
			p_maputl.P_UnsetThingPosition(thing);

			oldx = thing.x;
			oldy = thing.y;
			thing.floorz = tmfloorz;
			thing.ceilingz = tmceilingz;
			thing.x = x;
			thing.y = y;

			p_maputl.P_SetThingPosition(thing);

			if ((thing.flags2 & DoomDef.MF2_FOOTCLIP) != 0 && p_mobj.P_GetThingFloorType(thing) != p_local.FLOOR_SOLID)
			{
				thing.flags2 |= DoomDef.MF2_FEETARECLIPPED;
			}
			else if ((thing.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				thing.flags2 &= ~DoomDef.MF2_FEETARECLIPPED;
			}

			//
			// if any special lines were hit, do the effect
			//
			if ((thing.flags & (DoomDef.MF_TELEPORT | DoomDef.MF_NOCLIP)) == 0)
				while ((p_map.numspechit--) != 0)
				{
					// see if the line was crossed
					ld = spechit[numspechit];
					side = p_maputl.P_PointOnLineSide(thing.x, thing.y, ld);
					oldside = p_maputl.P_PointOnLineSide(oldx, oldy, ld);
					if (side != oldside)
					{
						if (ld.special != 0)
						{
							int indexOf = p_setup.lines.ToList().IndexOf(ld);
							p_spec.P_CrossSpecialLine(indexOf, oldside, thing);
						}
					}
				}

			return true;
		}

		/*
		==================
		=
		= P_ThingHeightClip
		=
		= Takes a valid thing and adjusts the thing.floorz, thing.ceilingz,
		= anf possibly thing.z
		=
		= This is called for all nearby monsters whenever a sector changes height
		=
		= If the thing doesn't fit, the z will be set to the lowest value and
		= false will be returned
		==================
		*/

		static public bool P_ThingHeightClip(DoomDef.mobj_t thing)
		{
			bool onfloor;

			onfloor = (thing.z == thing.floorz);

			P_CheckPosition(thing, thing.x, thing.y);
			// what about stranding a monster partially off an edge?

			thing.floorz = tmfloorz;
			thing.ceilingz = tmceilingz;

			if (onfloor)
				// walking monsters rise and fall with the floor
				thing.z = thing.floorz;
			else
			{	// don't adjust a floating monster unless forced to
				if (thing.z + thing.height > thing.ceilingz)
					thing.z = thing.ceilingz - thing.height;
			}

			if (thing.ceilingz - thing.floorz < thing.height)
				return false;

			return true;
		}

		/*
		==============================================================================

									SLIDE MOVE

		Allows the player to slide along any angled walls

		==============================================================================
		*/

		public static int bestslidefrac, secondslidefrac;
		public static r_local.line_t bestslideline, secondslideline;
		public static DoomDef.mobj_t slidemo;

		public static int tmxmove, tmymove;

		/*
		==================
		=
		= P_HitSlideLine
		=
		= Adjusts the xmove / ymove so that the next move will slide along the wall
		==================
		*/

		public static void P_HitSlideLine(r_local.line_t ld)
		{
			int side;
			uint lineangle, moveangle, deltaangle;
			int movelen, newlen;


			if (ld.slopetype == r_local.slopetype_t.ST_HORIZONTAL)
			{
				tmymove = 0;
				return;
			}
			if (ld.slopetype == r_local.slopetype_t.ST_VERTICAL)
			{
				tmxmove = 0;
				return;
			}

			side = p_maputl.P_PointOnLineSide(slidemo.x, slidemo.y, ld);

			lineangle = r_main.R_PointToAngle2(0, 0, ld.dx, ld.dy);
			if (side == 1)
				lineangle += DoomDef.ANG180;
			moveangle = r_main.R_PointToAngle2(0, 0, tmxmove, tmymove);
			deltaangle = moveangle - lineangle;
			if (deltaangle > DoomDef.ANG180)
				deltaangle += DoomDef.ANG180;

			lineangle >>= (int)DoomDef.ANGLETOFINESHIFT;
			deltaangle >>= (int)DoomDef.ANGLETOFINESHIFT;

			movelen = p_maputl.P_AproxDistance(tmxmove, tmymove);
			newlen = DoomDef.FixedMul(movelen, r_main.finecosine(deltaangle));
			tmxmove = DoomDef.FixedMul(newlen, r_main.finecosine(lineangle));
			tmymove = DoomDef.FixedMul(newlen, tables.finesine[lineangle]);
		}

		/*
		==============
		=
		= PTR_SlideTraverse
		=
		==============
		*/

		public class cPTR_SlideTraverse : p_maputl.P_PathTraverse_delegate
		{
			public override bool function(p_local.intercept_t in_)
			{
				r_local.line_t li;

				if (!in_.isaline)
					i_ibm.I_Error("PTR_SlideTraverse: not a line?");

				li = in_.line;
				if ((li.flags & DoomData.ML_TWOSIDED) == 0)
				{
					if (p_maputl.P_PointOnLineSide(slidemo.x, slidemo.y, li) != 0)
						return true;		// don't hit the back side
					goto isblocking;
				}

				p_maputl.P_LineOpening(li);			// set openrange, opentop, openbottom
				if (p_maputl.openrange < slidemo.height)
					goto isblocking;		// doesn't fit

				if (p_maputl.opentop - slidemo.z < slidemo.height)
					goto isblocking;		// mobj is too high

				if (p_maputl.openbottom - slidemo.z > 24 * DoomDef.FRACUNIT)
					goto isblocking;		// too big a step up

				return true;		// this line doesn't block movement

			// the line does block movement, see if it is closer than best so far
			isblocking:
				if (in_.frac < bestslidefrac)
				{
					secondslidefrac = bestslidefrac;
					secondslideline = bestslideline;
					bestslidefrac = in_.frac;
					bestslideline = li;
				}

				return false;	// stop
			}
		}

		public static cPTR_SlideTraverse PTR_SlideTraverse = new cPTR_SlideTraverse();

		/*
		==================
		=
		= P_SlideMove
		=
		= The momx / momy move is bad, so try to slide along a wall
		=
		= Find the first line hit, move flush to it, and slide along it
		=
		= This is a kludgy mess.
		==================
		*/

		public static void P_SlideMove(DoomDef.mobj_t mo)
		{
			int leadx, leady;
			int trailx, traily;
			int newx, newy;
			int hitcount;

			slidemo = mo;
			hitcount = 0;
		retry:
			if (++hitcount == 3)
			{
				if (!P_TryMove(mo, mo.x, mo.y + mo.momy))
					P_TryMove(mo, mo.x + mo.momx, mo.y);
				return;
			}

			//
			// trace along the three leading corners
			//
			if (mo.momx > 0)
			{
				leadx = mo.x + mo.radius;
				trailx = mo.x - mo.radius;
			}
			else
			{
				leadx = mo.x - mo.radius;
				trailx = mo.x + mo.radius;
			}

			if (mo.momy > 0)
			{
				leady = mo.y + mo.radius;
				traily = mo.y - mo.radius;
			}
			else
			{
				leady = mo.y - mo.radius;
				traily = mo.y + mo.radius;
			}

			bestslidefrac = DoomDef.FRACUNIT + 1;

			p_maputl.P_PathTraverse(leadx, leady, leadx + mo.momx, leady + mo.momy,
			 p_local.PT_ADDLINES, PTR_SlideTraverse);
			p_maputl.P_PathTraverse(trailx, leady, trailx + mo.momx, leady + mo.momy,
			 p_local.PT_ADDLINES, PTR_SlideTraverse);
			p_maputl.P_PathTraverse(leadx, traily, leadx + mo.momx, traily + mo.momy,
			 p_local.PT_ADDLINES, PTR_SlideTraverse);

			//
			// move up to the wall
			//
			if (bestslidefrac == DoomDef.FRACUNIT + 1)
			{	// the move most have hit the middle, so stairstep
				//stairstep:
				if (!P_TryMove(mo, mo.x, mo.y + mo.momy))
					P_TryMove(mo, mo.x + mo.momx, mo.y);
				return;
			}

			bestslidefrac -= 0x800;	// fudge a bit to make sure it doesn't hit
			if (bestslidefrac > 0)
			{
				newx = DoomDef.FixedMul(mo.momx, bestslidefrac);
				newy = DoomDef.FixedMul(mo.momy, bestslidefrac);
				if (!P_TryMove(mo, mo.x + newx, mo.y + newy))
				{
					if (!P_TryMove(mo, mo.x, mo.y + mo.momy))
						P_TryMove(mo, mo.x + mo.momx, mo.y);
					return;
				}
			}

			//
			// now continue along the wall
			//
			bestslidefrac = DoomDef.FRACUNIT - (bestslidefrac + 0x800);	// remainder
			if (bestslidefrac > DoomDef.FRACUNIT)
				bestslidefrac = DoomDef.FRACUNIT;
			if (bestslidefrac <= 0)
				return;

			tmxmove = DoomDef.FixedMul(mo.momx, bestslidefrac);
			tmymove = DoomDef.FixedMul(mo.momy, bestslidefrac);

			P_HitSlideLine(bestslideline);				// clip the moves

			mo.momx = tmxmove;
			mo.momy = tmymove;

			if (!P_TryMove(mo, mo.x + tmxmove, mo.y + tmymove))
			{
				goto retry;
			}
		}


		/*
		==============================================================================

									P_LineAttack

		==============================================================================
		*/


		public static DoomDef.mobj_t linetarget;			// who got hit (or NULL)
		public static DoomDef.mobj_t shootthing;
		public static int shootz;					// height if not aiming up or down
		// ???: use slope for monsters?
		public static int la_damage;
		public static int attackrange;

		public static int aimslope;

		/*
		===============================================================================
		=
		= PTR_AimTraverse
		=
		= Sets linetaget and aimslope when a target is aimed at
		===============================================================================
		*/
		public class cPTR_AimTraverse : p_maputl.P_PathTraverse_delegate
		{
			public override bool function(p_local.intercept_t in_)
			{
				r_local.line_t li;
				DoomDef.mobj_t th;
				int slope, thingtopslope, thingbottomslope;
				int dist;

				if (in_.isaline)
				{
					li = in_.line;
					if ((li.flags & DoomData.ML_TWOSIDED) == 0)
						return false;		// stop
					//
					// crosses a two sided line
					// a two sided line will restrict the possible target ranges
					p_maputl.P_LineOpening(li);

					if (p_maputl.openbottom >= p_maputl.opentop)
						return false;		// stop

					dist = DoomDef.FixedMul(attackrange, in_.frac);

					if (li.frontsector.floorheight != li.backsector.floorheight)
					{
						slope = d_main.FixedDiv(p_maputl.openbottom - shootz, dist);
						if (slope > p_sight.bottomslope)
							p_sight.bottomslope = slope;
					}

					if (li.frontsector.ceilingheight != li.backsector.ceilingheight)
					{
						slope = d_main.FixedDiv(p_maputl.opentop - shootz, dist);
						if (slope < p_sight.topslope)
							p_sight.topslope = slope;
					}

					if (p_sight.topslope <= p_sight.bottomslope)
						return false;		// stop

					return true;		// shot continues
				}

				//
				// shoot a thing
				//
				th = in_.thing;
				if (th == shootthing)
					return true;		// can't shoot self
				if ((th.flags & DoomDef.MF_SHOOTABLE) == 0)
					return true;		// corpse or something
				if (th.type == info.mobjtype_t.MT_POD)
				{ // Can't auto-aim at pods
					return (true);
				}

				// check angles to see if the thing can be aimed at

				dist = DoomDef.FixedMul(attackrange, in_.frac);
				thingtopslope = d_main.FixedDiv(th.z + th.height - shootz, dist);
				if (thingtopslope < p_sight.bottomslope)
					return true;		// shot over the thing
				thingbottomslope = d_main.FixedDiv(th.z - shootz, dist);
				if (thingbottomslope > p_sight.topslope)
					return true;		// shot under the thing

				//
				// this thing can be hit!
				//
				if (thingtopslope > p_sight.topslope)
					thingtopslope = p_sight.topslope;
				if (thingbottomslope < p_sight.bottomslope)
					thingbottomslope = p_sight.bottomslope;

				aimslope = (thingtopslope + thingbottomslope) / 2;
				linetarget = th;

				return false;			// don't go any farther
			}
		}

		public static cPTR_AimTraverse PTR_AimTraverse = new cPTR_AimTraverse();

		/*
		==============================================================================
		=
		= PTR_ShootTraverse
		=
		==============================================================================
		*/
		public class cPTR_ShootTraverse : p_maputl.P_PathTraverse_delegate
		{
			public override bool function(p_local.intercept_t in_)
			{
				int x, y, z;
				int frac;
				r_local.line_t li;
				DoomDef.mobj_t th;
				int slope;
				int dist;
				int thingtopslope, thingbottomslope;
				DoomDef.mobj_t mo;

				if (in_.isaline)
				{
					li = in_.line;
					if (li.special != 0)
						p_spec.P_ShootSpecialLine(shootthing, li);
					if ((li.flags & DoomData.ML_TWOSIDED) == 0)
						goto hitline;

					//
					// crosses a two sided line
					//
					p_maputl.P_LineOpening(li);

					dist = DoomDef.FixedMul(attackrange, in_.frac);

					if (li.frontsector.floorheight != li.backsector.floorheight)
					{
						slope = d_main.FixedDiv(p_maputl.openbottom - shootz, dist);
						if (slope > aimslope)
							goto hitline;
					}

					if (li.frontsector.ceilingheight != li.backsector.ceilingheight)
					{
						slope = d_main.FixedDiv(p_maputl.opentop - shootz, dist);
						if (slope < aimslope)
							goto hitline;
					}

					return true;		// shot continues
				//
				// hit line
				//
				hitline:
					// position a bit closer
					frac = in_.frac - d_main.FixedDiv(4 * DoomDef.FRACUNIT, attackrange);
					x = p_maputl.trace.x + DoomDef.FixedMul(p_maputl.trace.dx, frac);
					y = p_maputl.trace.y + DoomDef.FixedMul(p_maputl.trace.dy, frac);
					z = shootz + DoomDef.FixedMul(aimslope, DoomDef.FixedMul(frac, attackrange));

					if (li.frontsector.ceilingpic == r_plane.skyflatnum)
					{
						if (z > li.frontsector.ceilingheight)
							return false;		// don't shoot the sky!
						if (li.backsector != null && li.backsector.ceilingpic == r_plane.skyflatnum)
							return false;		// it's a sky hack wall
					}

					p_mobj.P_SpawnPuff(x, y, z);
					return false;			// don't go any farther
				}

				//
				// shoot a thing
				//
				th = in_.thing;
				if (th == shootthing)
					return true;		// can't shoot self
				if ((th.flags & DoomDef.MF_SHOOTABLE) == 0)
					return true;		// corpse or something

				//
				// check for physical attacks on a ghost
				//
				if ((th.flags & DoomDef.MF_SHADOW) != 0 && shootthing.player.readyweapon == DoomDef.weapontype_t.wp_staff)
				{
					return (true);
				}

				// check angles to see if the thing can be aimed at
				dist = DoomDef.FixedMul(attackrange, in_.frac);
				thingtopslope = d_main.FixedDiv(th.z + th.height - shootz, dist);
				if (thingtopslope < aimslope)
					return true;		// shot over the thing
				thingbottomslope = d_main.FixedDiv(th.z - shootz, dist);
				if (thingbottomslope > aimslope)
					return true;		// shot under the thing

				//
				// hit thing
				//
				// position a bit closer
				frac = in_.frac - d_main.FixedDiv(10 * DoomDef.FRACUNIT, attackrange);
				x = p_maputl.trace.x + DoomDef.FixedMul(p_maputl.trace.dx, frac);
				y = p_maputl.trace.y + DoomDef.FixedMul(p_maputl.trace.dy, frac);
				z = shootz + DoomDef.FixedMul(aimslope, DoomDef.FixedMul(frac, attackrange));
				if (p_mobj.PuffType == info.mobjtype_t.MT_BLASTERPUFF1)
				{ // Make blaster big puff
					mo = p_mobj.P_SpawnMobj(x, y, z, info.mobjtype_t.MT_BLASTERPUFF2);
					i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_blshit);
				}
				else
				{
					p_mobj.P_SpawnPuff(x, y, z);
				}
				if (la_damage != 0)
				{
					if ((in_.thing.flags & DoomDef.MF_NOBLOOD) == 0 && m_misc.P_Random() < 192)
					{
						p_mobj.P_BloodSplatter(x, y, z, in_.thing);
					}
					p_inter.P_DamageMobj(th, shootthing, shootthing, la_damage);
				}
				return (false); // don't go any farther
			}
		}

		public static cPTR_ShootTraverse PTR_ShootTraverse = new cPTR_ShootTraverse();

		/*
		=================
		=
		= P_AimLineAttack
		=
		=================
		*/

		public static int P_AimLineAttack(DoomDef.mobj_t t1, uint angle, int distance)
		{
			int x2, y2;

			angle >>= (int)DoomDef.ANGLETOFINESHIFT;
			shootthing = t1;
			x2 = t1.x + (distance >> DoomDef.FRACBITS) * r_main.finecosine(angle);
			y2 = t1.y + (distance >> DoomDef.FRACBITS) * tables.finesine[angle];
			shootz = t1.z + (t1.height >> 1) + 8 * DoomDef.FRACUNIT;
			p_sight.topslope = 100 * DoomDef.FRACUNIT / 160;	// can't shoot outside view angles
			p_sight.bottomslope = -100 * DoomDef.FRACUNIT / 160;
			attackrange = distance;
			linetarget = null;

			p_maputl.P_PathTraverse(t1.x, t1.y, x2, y2
				, p_local.PT_ADDLINES | p_local.PT_ADDTHINGS, PTR_AimTraverse);

			if (linetarget != null)
				return aimslope;
			return 0;
		}


		/*
		=================
		=
		= P_LineAttack
		=
		= if damage == 0, it is just a test trace that will leave linetarget set
		=
		=================
		*/

		public static void P_LineAttack(DoomDef.mobj_t t1, uint angle, int distance, int slope, int damage)
		{
			int x2, y2;

			angle >>= (int)DoomDef.ANGLETOFINESHIFT;
			shootthing = t1;
			la_damage = damage;
			x2 = t1.x + (distance >> DoomDef.FRACBITS) * r_main.finecosine(angle);
			y2 = t1.y + (distance >> DoomDef.FRACBITS) * tables.finesine[angle];
			shootz = t1.z + (t1.height >> 1) + 8 * DoomDef.FRACUNIT;
			if ((t1.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				shootz -= p_local.FOOTCLIPSIZE;
			}
			attackrange = distance;
			aimslope = slope;

			p_maputl.P_PathTraverse(t1.x, t1.y, x2, y2
				, p_local.PT_ADDLINES | p_local.PT_ADDTHINGS, PTR_ShootTraverse);
		}


		/*
==============================================================================

							USE LINES

==============================================================================
*/

		public static DoomDef.mobj_t usething;
		public class cPTR_UseTraverse : p_maputl.P_PathTraverse_delegate
		{
			public override bool function(p_local.intercept_t in_)
			{
				if (in_.line.special == 0)
				{
					p_maputl.P_LineOpening(in_.line);
					if (p_maputl.openrange <= 0)
					{
						//S_StartSound (usething, sfx_noway);
						return false;	// can't use through a wall
					}
					return true;		// not a special line, but keep checking
				}

				if (p_maputl.P_PointOnLineSide(usething.x, usething.y, in_.line) == 1)
					return false;		// don't use back sides

				p_switch.P_UseSpecialLine(usething, in_.line);

				return false;			// can't use for than one special line in a row
			}
		}
		public static cPTR_UseTraverse PTR_UseTraverse = new cPTR_UseTraverse();

		/*
		================
		=
		= P_UseLines
		=
		= Looks for special lines in front of the player to activate
		================ 
		*/

		public static void P_UseLines(DoomDef.player_t player)
		{
			uint angle;
			int x1, y1, x2, y2;

			usething = player.mo;

			angle = player.mo.angle >> (int)DoomDef.ANGLETOFINESHIFT;
			x1 = player.mo.x;
			y1 = player.mo.y;
			x2 = x1 + (p_local.USERANGE >> DoomDef.FRACBITS) * r_main.finecosine(angle);
			y2 = y1 + (p_local.USERANGE >> DoomDef.FRACBITS) * tables.finesine[angle];

			p_maputl.P_PathTraverse(x1, y1, x2, y2, p_local.PT_ADDLINES, PTR_UseTraverse);
		}


		/*
		==============================================================================

									RADIUS ATTACK

		==============================================================================
		*/

		public static DoomDef.mobj_t bombsource;
		public static DoomDef.mobj_t bombspot;
		public static int bombdamage;

		/*
		=================
		=
		= PIT_RadiusAttack
		=
		= Source is the creature that casued the explosion at spot
		=================
		*/
		public class cPIT_RadiusAttack : p_maputl.P_BlockThingsIterator_delegate
		{
			public override bool func(DoomDef.mobj_t thing)
			{
				int dx, dy, dist;

				if ((thing.flags & DoomDef.MF_SHOOTABLE) == 0)
				{
					return true;
				}
				if (thing.type == info.mobjtype_t.MT_MINOTAUR || thing.type == info.mobjtype_t.MT_SORCERER1
					|| thing.type == info.mobjtype_t.MT_SORCERER2)
				{ // Episode 2 and 3 bosses take no damage from PIT_RadiusAttack
					return (true);
				}
				dx = Math.Abs(thing.x - bombspot.x);
				dy = Math.Abs(thing.y - bombspot.y);
				dist = dx > dy ? dx : dy;
				dist = (dist - thing.radius) >> DoomDef.FRACBITS;
				if (dist < 0)
				{
					dist = 0;
				}
				if (dist >= bombdamage)
				{ // Out of range
					return true;
				}
				if (p_sight.P_CheckSight(thing, bombspot))
				{ // OK to damage, target is in direct path
					p_inter.P_DamageMobj(thing, bombspot, bombsource, bombdamage - dist);
				}
				return (true);
			}
		}

		public static cPIT_RadiusAttack PIT_RadiusAttack = new cPIT_RadiusAttack();

		/*
=================
=
= P_RadiusAttack
=
= Source is the creature that casued the explosion at spot
=================
*/

		public static void P_RadiusAttack(DoomDef.mobj_t spot, DoomDef.mobj_t source, int damage)
		{
			int x, y, xl, xh, yl, yh;
			int dist;

			dist = (damage + p_local.MAXRADIUS) << DoomDef.FRACBITS;
			yh = (spot.y + dist - p_setup.bmaporgy) >> p_local.MAPBLOCKSHIFT;
			yl = (spot.y - dist - p_setup.bmaporgy) >> p_local.MAPBLOCKSHIFT;
			xh = (spot.x + dist - p_setup.bmaporgx) >> p_local.MAPBLOCKSHIFT;
			xl = (spot.x - dist - p_setup.bmaporgx) >> p_local.MAPBLOCKSHIFT;
			bombspot = spot;
			if (spot.type == info.mobjtype_t.MT_POD && spot.target != null)
			{
				bombsource = spot.target;
			}
			else
			{
				bombsource = source;
			}
			bombdamage = damage;
			for (y = yl; y <= yh; y++)
				for (x = xl; x <= xh; x++)
					p_maputl.P_BlockThingsIterator(x, y, PIT_RadiusAttack);
		}

		/*
==============================================================================

						SECTOR HEIGHT CHANGING

= After modifying a sectors floor or ceiling height, call this
= routine to adjust the positions of all things that touch the
= sector.
=
= If anything doesn't fit anymore, true will be returned.
= If crunch is true, they will take damage as they are being crushed
= If Crunch is false, you should set the sector height back the way it
= was and call P_ChangeSector again to undo the changes
==============================================================================
*/

		public static bool crushchange;
		public static bool nofit;

		/*
		===============
		=
		= PIT_ChangeSector
		=
		===============
		*/
		public class cPIT_ChangeSector : p_maputl.P_BlockThingsIterator_delegate
		{
			public override bool func(DoomDef.mobj_t thing)
			{
				DoomDef.mobj_t mo;

				if (P_ThingHeightClip(thing))
					return true;		// keep checking

				// crunch bodies to giblets
				if (thing.health <= 0)
				{
					//P_SetMobjState (thing, S_GIBS);
					thing.height = 0;
					thing.radius = 0;
					return true;		// keep checking
				}

				// crunch dropped items
				if ((thing.flags & DoomDef.MF_DROPPED) != 0)
				{
					p_mobj.P_RemoveMobj(thing);
					return true;		// keep checking
				}

				if ((thing.flags & DoomDef.MF_SHOOTABLE) == 0)
					return true;				// assume it is bloody gibs or something

				nofit = true;
				if (crushchange && (p_tick.leveltime & 3) == 0)
				{
					p_inter.P_DamageMobj(thing, null, null, 10);
					// spray blood in a random direction
					mo = p_mobj.P_SpawnMobj(thing.x, thing.y, thing.z + thing.height / 2, info.mobjtype_t.MT_BLOOD);
					mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 12;
					mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 12;
				}

				return true;		// keep checking (crush other things)	
			}
		}

		public static cPIT_ChangeSector PIT_ChangeSector = new cPIT_ChangeSector();


		/*
===============
=
= P_ChangeSector
=
===============
*/

		public static bool P_ChangeSector(r_local.sector_t sector, bool crunch)
		{
			int x, y;

			nofit = false;
			crushchange = crunch;

			// recheck heights for all things near the moving sector

			for (x = sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXLEFT]; x <= sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT]; x++)
				for (y = sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM]; y <= sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXTOP]; y++)
					p_maputl.P_BlockThingsIterator(x, y, PIT_ChangeSector);


			return nofit;
		}
	}
}
