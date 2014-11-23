using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

		// R_main.c

namespace HereticXNA
{
	static public class r_main
	{
		/*

		*/

		public static int viewangleoffset;

		public static int validcount = 1;		// increment every time a check is made

		public static byte[] fixedcolormap;

		public static int centerx, centery;
		public static int centerxfrac, centeryfrac;
		public static int projection;
		public static int framecount;		// just for profiling purposes

		public static int sscount, linecount, loopcount;

		public static int viewx, viewy, viewz;
		public static uint viewanglez;
		public static uint viewangle;
		public static int viewcos, viewsin;
		public static DoomDef.player_t viewplayer;

		public static int detailshift;		// 0 = high, 1 = low

		//
		// precalculated math tables
		//
		public static uint clipangle;

		// The viewangletox[viewangle + FINEANGLES/4] lookup maps the visible view
		// angles  to screen X coordinates, flattening the arc to a flat projection
		// plane.  There will be many angles mapped to the same X.
		public static int[] viewangletox = new int[DoomDef.FINEANGLES / 2];

		// The xtoviewangleangle[] table maps a screen pixel to the lowest viewangle
		// that maps back to x ranges from clipangle to -clipangle
		public static uint[] xtoviewangle = new uint[DoomDef.SCREENWIDTH + 1];

		// the finetangentgent[angle+FINEANGLES/4] table holds the int tangent
		// values for view angles, ranging from MININT to 0 to MAXINT.
		public static int finecosine(uint i)
		{
			return tables.finesine[(uint)(DoomDef.FINEANGLES / 4 + i) % tables.finesine.Count()];
		}
#if DOS


#endif
		public static int[][] scalelight;// = new int[r_local.LIGHTLEVELS][r_local.MAXLIGHTSCALE]; // [dsl] It's an offset
		//	public static byte	*scalelightfixed[MAXLIGHTSCALE];
		public static byte[, ,] zlight = new byte[r_local.LIGHTLEVELS, r_local.MAXLIGHTZ, 256];

		public static int extralight;			// bumped light from gun blasts
#if DOS

void		(*colfunc) (void);
void		(*basecolfunc) (void);
void		(*fuzzcolfunc) (void);
void		(*transcolfunc) (void);
void		(*spanfunc) (void);

/*
===================
=
= R_AddPointToBox
=
===================
*/

void R_AddPointToBox (int x, int y, int *box)
{
	if (x< box[BOXLEFT])
		box[BOXLEFT] = x;
	if (x> box[BOXRIGHT])
		box[BOXRIGHT] = x;
	if (y< box[BOXBOTTOM])
		box[BOXBOTTOM] = y;
	if (y> box[BOXTOP])
		box[BOXTOP] = y;
}


#endif
		/*
===============================================================================
=
= R_PointOnSide
=
= Returns side 0 (front) or 1 (back)
===============================================================================
*/

		public static int R_PointOnSide(int x, int y, r_local.node_t node)
		{
			int dx, dy;
			int left, right;

			if (node.dx == 0)
			{
				if (x <= node.x)
					return node.dy > 0 ? 1 : 0;
				return node.dy < 0 ? 1 : 0;
			}
			if (node.dy == 0)
			{
				if (y <= node.y)
					return node.dx < 0 ? 1 : 0;
				return node.dx > 0 ? 1 : 0;
			}

			dx = (x - node.x);
			dy = (y - node.y);

			// try to quickly decide by looking at sign bits
			if (((node.dy ^ node.dx ^ dx ^ dy) & 0x80000000) != 0)
			{
				if (((node.dy ^ dx) & 0x80000000) != 0)
					return 1;	// (left is negative)
				return 0;
			}

			left = DoomDef.FixedMul(node.dy >> DoomDef.FRACBITS, dx);
			right = DoomDef.FixedMul(dy, node.dx >> DoomDef.FRACBITS);

			if (right < left)
				return 0;		// front side
			return 1;			// back side
		}

#if DOS
int	R_PointOnSegSide (int x, int y, seg_t *line)
{
	int	lx, ly;
	int	ldx, ldy;
	int	dx,dy;
	int	left, right;

	lx = line.v1.x;
	ly = line.v1.y;

	ldx = line.v2.x - lx;
	ldy = line.v2.y - ly;

	if (!ldx)
	{
		if (x <= lx)
			return ldy > 0;
		return ldy < 0;
	}
	if (!ldy)
	{
		if (y <= ly)
			return ldx < 0;
		return ldx > 0;
	}

	dx = (x - lx);
	dy = (y - ly);

// try to quickly decide by looking at sign bits
	if ( (ldy ^ ldx ^ dx ^ dy)&0x80000000 )
	{
		if  ( (ldy ^ dx) & 0x80000000 )
			return 1;	// (left is negative)
		return 0;
	}

	left = FixedMul ( ldy>>FRACBITS , dx );
	right = FixedMul ( dy , ldx>>FRACBITS );

	if (right < left)
		return 0;		// front side
	return 1;			// back side
}


#endif
		/*
===============================================================================
=
= R_PointToAngle
=
===============================================================================
*/

		// to get a global angle from cartesian coordinates, the coordinates are
		// flipped until they are in the first octant of the coordinate system, then
		// the y (<=x) is scaled and divided by x to get a tangent (slope) value
		// which is looked up in the tantoangle[] table.  The +1 size is to handle
		// the case when x==y without additional checking.
		public const int SLOPERANGE = 2048;
		public const int SLOPEBITS = 11;
		public const int DBITS = (DoomDef.FRACBITS - SLOPEBITS);

		public static int SlopeDiv(uint num, uint den)
		{
			uint ans;
			if (den < 512)
				return SLOPERANGE;
			ans = (num << 3) / (den >> 8);
			return (int)(ans <= SLOPERANGE ? ans : SLOPERANGE);
		}
		public static uint R_PointToAngle(int x, int y)
		{
			// [dsl] So many casts... something was wrong in Doom
			x -= viewx;
			y -= viewy;
			//x -= (int)(Game1.instance.camPos.X * 65536);
			//y -= (int)(Game1.instance.camPos.Y * 65536);
			if ((x == 0) && (y == 0))
				return 0;
			if (x >= 0)
			{	// x >=0
				if (y >= 0)
				{	// y>= 0
					if (x > y)
						return (uint)tables.tantoangle[SlopeDiv((uint)y, (uint)x)];     // octant 0
					else
						return DoomDef.ANG90 - 1 - (uint)tables.tantoangle[SlopeDiv((uint)x, (uint)y)];  // octant 1
				}
				else
				{	// y<0
					y = -y;
					if (x > y)
						return (uint)(-tables.tantoangle[SlopeDiv((uint)y, (uint)x)]);  // octant 8
					else
						return DoomDef.ANG270 + (uint)tables.tantoangle[SlopeDiv((uint)x, (uint)y)];  // octant 7
				}
			}
			else
			{	// x<0
				x = -x;
				if (y >= 0)
				{	// y>= 0
					if (x > y)
						return DoomDef.ANG180 - 1 - (uint)tables.tantoangle[SlopeDiv((uint)y, (uint)x)]; // octant 3
					else
						return DoomDef.ANG90 + (uint)tables.tantoangle[SlopeDiv((uint)x, (uint)y)];  // octant 2
				}
				else
				{	// y<0
					y = -y;
					if (x > y)
						return DoomDef.ANG180 + (uint)tables.tantoangle[SlopeDiv((uint)y, (uint)x)];  // octant 4
					else
						return DoomDef.ANG270 - 1 - (uint)tables.tantoangle[SlopeDiv((uint)x, (uint)y)];  // octant 5
				}
			}

			//return 0; // [dsl] Can't reach that
		}

public static uint R_PointToAngle2 (int x1, int y1, int x2, int y2)
{
	viewx = x1;
	viewy = y1;
	return R_PointToAngle (x2, y2);
}


		public static int R_PointToDist(int x, int y)
{
	int		angle;
	int dx, dy, temp;
	int dist;

	dx = Math.Abs(x - viewx);
	dy = Math.Abs(y - viewy);

	if (dy>dx)
	{
		temp = dx;
		dx = dy;
		dy = temp;
	}

	angle = (int)(tables.tantoangle[d_main.FixedDiv(dy, dx) >> DBITS] + DoomDef.ANG90) >> (int)DoomDef.ANGLETOFINESHIFT;

	dist = d_main.FixedDiv(dx, tables.finesine[angle]);	// use as cosine

	return dist;
}


		/*
=================
=
= R_InitPointToAngle
=
=================
*/

		public static void R_InitPointToAngle()
		{
			// now getting from tables.c
		}


//=============================================================================

/*
================
=
= R_ScaleFromGlobalAngle
=
= Returns the texture mapping scale for the current line at the given angle
= rw_distance must be calculated first
================
*/

public static int R_ScaleFromGlobalAngle (uint visangle)
{
	int scale;
	int			anglea, angleb;
	int			sinea, sineb;
	int num, den;


	anglea = (int)(DoomDef.ANG90 + (visangle-viewangle));
	angleb = (int)(DoomDef.ANG90 + (visangle - r_segs.rw_normalangle));
// bothe sines are allways positive
	sinea = tables.finesine[anglea>>(int)DoomDef.ANGLETOFINESHIFT];
	sineb = tables.finesine[angleb >> (int)DoomDef.ANGLETOFINESHIFT];
	num = DoomDef.FixedMul(projection, sineb) << detailshift;
	den = DoomDef.FixedMul(r_segs.rw_distance, sinea);
	if (den > num>>16)
	{
		scale = d_main.FixedDiv(num, den);
		if (scale > 64 * DoomDef.FRACUNIT)
			scale = 64 * DoomDef.FRACUNIT;
		else if (scale < 256)
			scale = 256;
	}
	else
		scale = 64 * DoomDef.FRACUNIT;

	return scale;
}


		/*
=================
=
= R_InitTables
=
=================
*/

		public static void R_InitTables()
		{
			// now getting from tables.c

		}

		/*
		=================
		=
		= R_InitTextureMapping
		=
		=================
		*/

		public static void R_InitTextureMapping()
		{
			int i;
			int x;
			int t;
			int focallength;


			//
			// use tangent table to generate viewangletox
			// viewangletox will give the next greatest x after the view angle
			//
			// calc focallength so FIELDOFVIEW angles covers SCREENWIDTH
			focallength = d_main.FixedDiv(centerxfrac, tables.finetangent[DoomDef.FINEANGLES / 4 + r_local.FIELDOFVIEW / 2]);

			for (i = 0; i < DoomDef.FINEANGLES / 2; i++)
			{
				if (tables.finetangent[i] > DoomDef.FRACUNIT * 2)
					t = -1;
				else if (tables.finetangent[i] < -DoomDef.FRACUNIT * 2)
					t = r_draw.viewwidth + 1;
				else
				{
					t = DoomDef.FixedMul(tables.finetangent[i], focallength);
					t = (centerxfrac - t + DoomDef.FRACUNIT - 1) >> DoomDef.FRACBITS;
					if (t < -1)
						t = -1;
					else if (t > r_draw.viewwidth + 1)
						t = r_draw.viewwidth + 1;
				}
				viewangletox[i] = t;
			}

			//
			// scan viewangletox[] to generate xtoviewangleangle[]
			//
			// xtoviewangle will give the smallest view angle that maps to x
			for (x = 0; x <= r_draw.viewwidth; x++)
			{
				i = 0;
				while (viewangletox[i] > x)
					i++;
				xtoviewangle[x] = (uint)((i << (int)DoomDef.ANGLETOFINESHIFT) - DoomDef.ANG90);
			}

			//
			// take out the fencepost cases from viewangletox
			//
			for (i = 0; i < DoomDef.FINEANGLES / 2; i++)
			{
				t = DoomDef.FixedMul(tables.finetangent[i], focallength);
				t = centerx - t;
				if (viewangletox[i] == -1)
					viewangletox[i] = 0;
				else if (viewangletox[i] == r_draw.viewwidth + 1)
					viewangletox[i] = r_draw.viewwidth;
			}

			clipangle = xtoviewangle[0];
		}

		//=============================================================================

		/*
		====================
		=
		= R_InitLightTables
		=
		= Only inits the zlight table, because the scalelight table changes
		= with view size
		=
		====================
		*/
		public const int DISTMAP = 2;

		public static void R_InitLightTables()
		{
			int i, j, level, startmap;
			int scale;

			//
			// Calculate the light levels to use for each level / distance combination
			//
			for (i = 0; i < r_local.LIGHTLEVELS; i++)
			{
				startmap = ((r_local.LIGHTLEVELS - 1 - i) * 2) * r_local.NUMCOLORMAPS / r_local.LIGHTLEVELS;
				for (j = 0; j < r_local.MAXLIGHTZ; j++)
				{
					scale = d_main.FixedDiv((DoomDef.SCREENWIDTH / 2 * DoomDef.FRACUNIT), (j + 1) << r_local.LIGHTZSHIFT);
					scale >>= r_local.LIGHTSCALESHIFT;
					level = startmap - scale / DISTMAP;
					if (level < 0)
						level = 0;
					if (level >= r_local.NUMCOLORMAPS)
						level = r_local.NUMCOLORMAPS - 1;
					for (int k = 0; k < 256; ++k)
					{
						zlight[i, j, k] = r_data.colormaps[level * 256 + k];
					}
				}
			}
		}

		/*
		==============
		=
		= R_SetViewSize
		=
		= Don't really change anything here, because i might be in the middle of
		= a refresh.  The change will take effect next refresh.
		=
		==============
		*/

		public static bool setsizeneeded;
		public static int setblocks, setdetail;

		public static void R_SetViewSize(int blocks, int detail)
		{
			setsizeneeded = true;
			setblocks = blocks;
			setdetail = detail;
		}
		/*
		==============
		=
		= R_ExecuteSetViewSize
		=
		==============
		*/

		public static void R_ExecuteSetViewSize()
		{
			int cosadj, dy;
			int i, j, level, startmap;

			setsizeneeded = false;

			if (setblocks == 11)
			{
				r_draw.scaledviewwidth = DoomDef.SCREENWIDTH;
				r_draw.viewheight = DoomDef.SCREENHEIGHT;
			}
			else
			{
				r_draw.scaledviewwidth = setblocks * 32;
				r_draw.viewheight = (setblocks * 158 / 10);
			}

			detailshift = setdetail;
			r_draw.viewwidth = r_draw.scaledviewwidth >> detailshift;

			centery = r_draw.viewheight / 2;
			centerx = r_draw.viewwidth / 2;
			centerxfrac = centerx << DoomDef.FRACBITS;
			centeryfrac = centery << DoomDef.FRACBITS;
			projection = centerxfrac;

			if (r_main.detailshift == 0)
			{
				// [dsl] We don't care rendering calls
				//	colfunc = basecolfunc = R_DrawColumn;
				//	fuzzcolfunc = R_DrawFuzzColumn;
				//	transcolfunc = R_DrawTranslatedColumn;
				//	spanfunc = R_DrawSpan;
			}
			else
			{
				// [dsl] We don't care rendering calls
				//	colfunc = basecolfunc = R_DrawColumnLow;
				//	fuzzcolfunc = R_DrawFuzzColumn;
				//	transcolfunc = R_DrawTranslatedColumn;
				//	spanfunc = R_DrawSpanLow;
			}

			r_draw.R_InitBuffer(r_draw.scaledviewwidth, r_draw.viewheight);

			R_InitTextureMapping();

			//
			// psprite scales
			//
			r_thing.pspritescale = DoomDef.FRACUNIT * r_draw.viewwidth / DoomDef.SCREENWIDTH;
			r_thing.pspriteiscale = DoomDef.FRACUNIT * DoomDef.SCREENWIDTH / r_draw.viewwidth;

			//
			// thing clipping
			//
			for (i = 0; i < r_draw.viewwidth; i++)
				r_thing.screenheightarray[i] = (short)r_draw.viewheight;

			//
			// planes
			//
			for (i = 0; i < r_draw.viewheight; i++)
			{
				dy = ((i - r_draw.viewheight / 2) << DoomDef.FRACBITS) + DoomDef.FRACUNIT / 2;
				dy = Math.Abs(dy);
				r_plane.yslope[i] = d_main.FixedDiv((r_draw.viewwidth << detailshift) / 2 * DoomDef.FRACUNIT, dy);
			}

			for (i = 0; i < r_draw.viewwidth; i++)
			{
				cosadj = Math.Abs(finecosine((uint)((int)xtoviewangle[i] >> (int)DoomDef.ANGLETOFINESHIFT)));
				r_plane.distscale[i] = d_main.FixedDiv(DoomDef.FRACUNIT, cosadj);
			}

			//
			// Calculate the light levels to use for each level / scale combination
			//
			for (i = 0; i < r_local.LIGHTLEVELS; i++)
			{
				startmap = ((r_local.LIGHTLEVELS - 1 - i) * 2) * r_local.NUMCOLORMAPS / r_local.LIGHTLEVELS;
				for (j = 0; j < r_local.MAXLIGHTSCALE; j++)
				{
					level = startmap - j * DoomDef.SCREENWIDTH / (r_draw.viewwidth << detailshift) / DISTMAP;
					if (level < 0)
						level = 0;
					if (level >= r_local.NUMCOLORMAPS)
						level = r_local.NUMCOLORMAPS - 1;
					scalelight[i][j] = level * 256;
				}
			}

			//
			// draw the border
			//
			//R_DrawViewBorder ();    // erase old menu stuff
		}


		/*
==============
=
= R_Init
=
==============
*/

		public static int detailLevel;
		public static m_misc.var_int screenblocks = new m_misc.var_int();
		private static byte[] lumData;
		private static float[] lumDataf = new float[3];
		public static float lumLast;
		public const float LUM_TARGET = .25f;
		public static float lumMultiplier = 1.0f;
		private const float LUM_ADAPT_SPEED_LIGHT = 12.0f;
		private const float LUM_ADAPT_SPEED_DARK = 20.0f;
		private const float LUM_MIN = .01f;
		private const float LUM_MAX = 5.0f;

		public static void R_Init()
		{
			r_data.R_InitData();
			R_InitPointToAngle();
			R_InitTables();
			// viewwidth / viewheight / detailLevel are set by the defaults
			R_SetViewSize(screenblocks.val, detailLevel);
			r_plane.R_InitPlanes();
			R_InitLightTables();
			r_plane.R_InitSkyMap();
			r_draw.R_InitTranslationTables();
			framecount = 0;
		}



		/*
		==============
		=
		= R_PointInSubsector
		=
		==============
		*/

		public static r_local.subsector_t R_PointInSubsector(int x, int y)
		{
			r_local.node_t node;
			int side, nodenum;

			if (p_setup.numnodes == 0)				// single subsector is a special case
				return p_setup.subsectors[0];

			nodenum = p_setup.numnodes - 1;

			while ((nodenum & DoomData.NF_SUBSECTOR) == 0)
			{
				node = p_setup.nodes[nodenum];
				side = r_main.R_PointOnSide(x, y, node);
				nodenum = node.children[side];
			}

			return p_setup.subsectors[nodenum & ~DoomData.NF_SUBSECTOR];

		}

		//----------------------------------------------------------------------------
		//
		// PROC R_SetupFrame
		//
		//----------------------------------------------------------------------------

		public static void R_SetupFrame(DoomDef.player_t player)
		{
			int i;
			int tableAngle;
			int tempCentery;

			//drawbsp = 1;
			viewplayer = player;
			viewangle = (uint)(player.mo.angle + viewangleoffset);
			tableAngle = (int)((uint)viewangle >> (int)DoomDef.ANGLETOFINESHIFT);
			if (player.chickenTics != 0 && player.chickenPeck != 0)
			{ // Set chicken attack view position
				viewx = player.mo.x + player.chickenPeck * r_main.finecosine((uint)tableAngle);
				viewy = player.mo.y + player.chickenPeck * tables.finesine[tableAngle];
			}
			else
			{ // Normal view position
				viewx = player.mo.x;
				viewy = player.mo.y;
			}
			extralight = player.extralight;
			viewanglez = player.mo.xangle; // We control the view differently for XNA
			viewz = player.viewz;
		//	int tableAngle2 = (int)((uint)viewanglez >> (int)DoomDef.ANGLETOFINESHIFT);
		//	viewz = tables.finesine[tableAngle2];

			// [dsl] that part is probaably useless for us
			tempCentery = r_draw.viewheight / 2 + (player.lookdir) * r_main.screenblocks.val / 10;
			if (centery != tempCentery)
			{
				centery = tempCentery;
				centeryfrac = centery << DoomDef.FRACBITS;
				for (i = 0; i < r_draw.viewheight; i++)
				{
					r_plane.yslope[i] = d_main.FixedDiv((r_draw.viewwidth << detailshift) / 2 * DoomDef.FRACUNIT,
						Math.Abs(((i - centery) << DoomDef.FRACBITS) + DoomDef.FRACUNIT / 2));
				}
			}
			viewsin = tables.finesine[(uint)tableAngle];
			viewcos = r_main.finecosine((uint)tableAngle);
			sscount = 0;
			// [dsl] Also probablement useless, light tables. We will use shaders for this
			//if(player.fixedcolormap != 0)
			//{
			//    fixedcolormap = colormaps+player.fixedcolormap
			//        *256*sizeof(lighttable_t);
			//    walllights = scalelightfixed;
			//    for(i = 0; i < MAXLIGHTSCALE; i++)
			//    {
			//        scalelightfixed[i] = fixedcolormap;
			//    }
			//}
			//else
			//{
			//    fixedcolormap = 0;
			//}
			framecount++;
			validcount++;
			// [dsl] We don't care for borders
			//if(BorderNeedRefresh)
			//{
			//    if(setblocks < 10)
			//    {
			//        R_DrawViewBorder();
			//    }
			//    BorderNeedRefresh = false;
			//    BorderTopRefresh = false;
			//    UpdateState |= I_FULLSCRN;
			//}
			//if(BorderTopRefresh)
			//{
			//    if(setblocks < 10)
			//    {
			//        R_DrawTopBorder();
			//    }
			//    BorderTopRefresh = false;
			//    UpdateState |= I_MESSAGES;
			//}
		}

		/*
		==============
		=
		= R_RenderView
		=
		==============
		*/

		public static void R_RenderPlayerView(DoomDef.player_t player)
		{
			DoomDef.thinker_t think;

			r_main.R_SetupFrame(player);
			r_bsp.R_ClearClipSegs();
			r_bsp.R_ClearDrawSegs();
			r_plane.R_ClearPlanes();
			r_thing.R_ClearSprites();

			d_net.NetUpdate();					// check for new console commands
			Frame.prepare();

			if (!Settings.Default.use_deferred)
			{
				Standard.Renderer.draw();
			}
			else
			{
				Deferred.Renderer.draw();

			/*	GraphicsDevice device = Game1.instance.GraphicsDevice;
				device.DepthStencilState = DepthStencilState.Default;

				// Prepare our shaders
				if (Game1.instance.showTextures)
					if (Settings.Default.ambient_enabled)
						Game1.instance.fxWall.CurrentTechnique = Game1.instance.fxWall.Techniques[Game1.instance.techniqueName("TechniqueMain")];
					else
						Game1.instance.fxWall.CurrentTechnique = Game1.instance.fxWall.Techniques[Game1.instance.techniqueName("TechniqueNoTexture")];
				else
					if (Settings.Default.ambient_enabled)
						Game1.instance.fxWall.CurrentTechnique = Game1.instance.fxWall.Techniques[Game1.instance.techniqueName("TechniqueNoTexture")];
					else
						Game1.instance.fxWall.CurrentTechnique = Game1.instance.fxWall.Techniques[Game1.instance.techniqueName("TechniqueNoTextureNoAmbient")];
				Game1.instance.fxWall.Parameters["View"].SetValue(Game1.instance.view);
				Game1.instance.fxWall.Parameters["Projection"].SetValue(Game1.instance.proj);
				Game1.instance.fxWall.Parameters["World"].SetValue(Matrix.Identity);
				Game1.instance.fxWall.Parameters["CamPos"].SetValue(pos);
				Game1.instance.fxWall.Parameters["tint"].SetValue(Color.White.ToVector4());
				Game1.instance.fxWall.Parameters["ambientEpsilon"].SetValue(r_segs.ambientEpsilon);
				Game1.instance.fxWall.Parameters["ambientSize"].SetValue(r_segs.ambientSize);
				Game1.instance.fxWall.Parameters["ambientAmount"].SetValue(r_segs.ambientAmount);

				if (Game1.instance.showTextures)
					if (Settings.Default.ambient_enabled)
						Game1.instance.fxPlane.CurrentTechnique = Game1.instance.fxPlane.Techniques[Game1.instance.techniqueName("TechniqueMain")];
					else
						Game1.instance.fxPlane.CurrentTechnique = Game1.instance.fxPlane.Techniques[Game1.instance.techniqueName("TechniqueNoAmbient")];
				else
					if (Settings.Default.ambient_enabled)
						Game1.instance.fxPlane.CurrentTechnique = Game1.instance.fxPlane.Techniques[Game1.instance.techniqueName("TechniqueNoTexture")];
					else
						Game1.instance.fxPlane.CurrentTechnique = Game1.instance.fxPlane.Techniques[Game1.instance.techniqueName("TechniqueNoTextureNoAmbient")];
				Game1.instance.fxPlane.Parameters["View"].SetValue(Game1.instance.view);
				Game1.instance.fxPlane.Parameters["Projection"].SetValue(Game1.instance.proj);
				Game1.instance.fxPlane.Parameters["World"].SetValue(Matrix.Identity);
				Game1.instance.fxPlane.Parameters["CamPos"].SetValue(pos);
				Game1.instance.fxPlane.Parameters["tint"].SetValue(Color.White.ToVector4());
				Game1.instance.fxPlane.Parameters["ambientEpsilon"].SetValue(r_segs.ambientEpsilon);
				Game1.instance.fxPlane.Parameters["ambientSize"].SetValue(r_segs.ambientSize);
				Game1.instance.fxPlane.Parameters["ambientAmount"].SetValue(r_segs.ambientAmount);

				Game1.instance.fxPlaneAmbient.CurrentTechnique = Game1.instance.fxPlaneAmbient.Techniques[0];
				Matrix ambientMapProj = Matrix.CreateOrthographic(
					65536 / 4,
					65536 / 4,
					-999, 999);
				Matrix ambientMapView = Matrix.Identity;
				Game1.instance.fxPlaneAmbient.Parameters["View"].SetValue(ambientMapView);
				Game1.instance.fxPlaneAmbient.Parameters["Projection"].SetValue(ambientMapProj);
				Game1.instance.fxPlaneAmbient.Parameters["World"].SetValue(Matrix.Identity);

				Game1.instance.fxSprite.CurrentTechnique = Game1.instance.fxSprite.Techniques[Game1.instance.techniqueName("Technique1")];
				Game1.instance.fxSprite.Parameters["View"].SetValue(Game1.instance.view);
				Game1.instance.fxSprite.Parameters["Projection"].SetValue(Game1.instance.proj);
				Game1.instance.fxSprite.Parameters["World"].SetValue(Matrix.Identity);
				Game1.instance.fxSprite.Parameters["CamPos"].SetValue(pos);

				Game1.instance.fxSky.CurrentTechnique = Game1.instance.fxSky.Techniques[Game1.instance.techniqueName("Technique1")];
				Game1.instance.fxSky.Parameters["View"].SetValue(Game1.instance.view);
				Game1.instance.fxSky.Parameters["Projection"].SetValue(Game1.instance.proj);
				Game1.instance.fxSky.Parameters["World"].SetValue(Matrix.Identity);
				Game1.instance.fxSky.Parameters["CameraAngles"].SetValue(new Vector2(camAngleZ, camAngleX));

				r_plane.visibleSectors.Clear(); // [dsl] XNA stuff
				d_net.NetUpdate();					// check for new console commands

				{
					device.BlendState = BlendState.Opaque;
					device.RasterizerState = new RasterizerState();

					// Update some render targets first used for sector lighting
					if (Game1.instance.useDeferred)
					{
						//--- Update flat sector illumination (Laval, skylight, etc)
						foreach (r_local.sector_t sector in p_setup.sectors)
						{
							if (sector == null) continue;
							if (sector.floorBatch == null) continue;
							if (sector.ceilingBatch == null) continue;
							if (Game1.instance.flatLightInfoById.ContainsKey(sector.floorpic))
							{
								// Generate light data if it doesn't have one
								if (sector.lightData == null) sector.lightData = new r_local.SectorLightData(sector, Game1.instance.flatLightInfoById[sector.floorpic]);
							}
							if (Game1.instance.flatCLightInfoById.ContainsKey(sector.ceilingpic))
							{
								// Generate light data if it doesn't have one
								if (sector.lightDataC == null) sector.lightDataC = new r_local.SectorLightData(sector, Game1.instance.flatCLightInfoById[sector.ceilingpic]);
							}
						}

						//--- Update shadow maps
						for (think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
						{
							if (think.function == null) continue;
							DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
							if (mo == null) continue;
							if (mo.infol.light == null) continue;
							if (!mo.infol.light.castShadow) continue;
							if (mo.shadowInfo == null)
							{
								mo.shadowInfo = new DoomDef.ShadowInfo(mo);
							}
							else if (mo.shadowInfo.needUpdate)
							{
								mo.shadowInfo.UpdateShadow();
							}
						}
					}

					device.BlendState = BlendState.Opaque;
					device.RasterizerState = new RasterizerState();

					// Render ambient map
					device.SetRenderTarget(AmbientMap.ambientTexture);
					device.Clear(new Color(255, 0, 0, 255));
					Game1.instance.fxPlaneAmbient.CurrentTechnique.Passes[0].Apply();
					foreach (r_local.sector_t sector in p_setup.sectors)
					{
						r_plane.XNARenderFlatToAmbientMap(sector);
					}
					device.SetRenderTarget(null);
					device.Clear(Color.Black);
					Game1.instance.fxPlane.Parameters["AmbientTexture"].SetValue(AmbientMap.ambientTexture);

					if (Game1.instance.wireframe)
						device.RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
					else
						device.RasterizerState = RasterizerState.CullCounterClockwise;

					if (Game1.instance.useDeferred)
					{
						Deferred.GBuffer.begin();
					}
					foreach (r_local.sector_t sector in p_setup.sectors)
					{
						sector.draw();
					}

					r_local.vissprite_t vis = new r_local.vissprite_t();
					int lump;
					uint rot;
					uint ang;
					bool flip;
					r_local.spritedef_t sprdef;
					r_local.spriteframe_t sprframe;
					for (think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
					{
						if (think.function == null) continue;
						DoomDef.mobj_t thing = think.function.obj as DoomDef.mobj_t;
						if (thing == null) continue;

						if ((thing.flags & DoomDef.MF_NOSECTOR) != 0)
						{
							if (!Game1.instance.useFreeCam) continue;
							sprdef = r_thing.sprites[(int)thing.sprite];
							sprframe = sprdef.spriteframes[thing.frame & DoomDef.FF_FRAMEMASK];
						}
						else
						{
							//
							// decide which patch to use for sprite reletive to player
							//
							sprdef = r_thing.sprites[(int)thing.sprite];
							sprframe = sprdef.spriteframes[thing.frame & DoomDef.FF_FRAMEMASK];
						}

						if (sprframe.rotate == 1)
						{	// choose a different rotation based on player view
							ang = r_main.R_PointToAngle(thing.x, thing.y);
							rot = (ang - thing.angle + (uint)(DoomDef.ANG45 / 2) * 9) >> 29;
							lump = sprframe.lump[rot];
							flip = sprframe.flip[rot] == 0 ? false : true;
						}
						else
						{	// use single rotation for all views
							lump = sprframe.lump[0];
							flip = sprframe.flip[0] == 0 ? false : true;
						}

						vis.mobjflags = thing.flags;
						vis.thing = thing;
						vis.psprite = false;
						vis.patch = lump;
						vis.flip = flip;
						vis.gx = thing.x;
						vis.gy = thing.y;
						vis.gz = thing.z;
						vis.gzt = thing.z + r_data.spritetopoffset[lump]; // [dsl] Added +8, all sprites were clipping a bit
						info.mobjinfo_t mobT = info.mobjinfo[(int)thing.type];
						vis.selfIllumT = mobT.selfIllumT;
						vis.selfIllumB = mobT.selfIllumB;
						r_thing.R_DrawSprite(vis);
					}

					if (Game1.instance.useDeferred)
					{
						if (Game1.instance.usePostProcess)
						{
							device.SetRenderTarget(Game1.instance.frameBuffer);
							device.Clear(Color.Transparent);
						}
						else
						{
							device.SetRenderTargets(null);
						}
					}

					// Render the deffered lighting now to construct the image
					if (Game1.instance.useDeferred)
					{
						device.Clear(Color.Black);
						Deferred.GBuffer.bind(Game1.instance.fxDeferred);
						Game1.instance.fxDeferred.Parameters["Viewport"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
						Game1.instance.fxDeferred.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Game1.instance.view * Game1.instance.proj));

						//--- Ambient
						Game1.instance.fxDeferred.CurrentTechnique = Game1.instance.fxDeferred.Techniques["TechniqueAmbient"];
						Game1.instance.fxDeferred.CurrentTechnique.Passes[0].Apply();
						device.SamplerStates[1] = SamplerState.PointClamp;
						Deferred.FullscreenQuad.prepareDraw();
						Deferred.FullscreenQuad.draw();
						device.BlendState = BlendState.Additive;

						//--- Point lights
						for (think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
						{
							if (think.function == null) continue;
							DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
							if (mo == null) continue;
							info.mobjinfo_t mobT = info.mobjinfo[(int)mo.type];
							if (mobT.light != null)
							{
								sprdef = r_thing.sprites[(int)mo.sprite];
								sprframe = sprdef.spriteframes[mo.frame & DoomDef.FF_FRAMEMASK];
								lump = sprframe.lump[0];
								Texture2D texture = w_wad.W_CacheLumpNum(lump + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Texture2D;
								Vector4 color = mobT.light.color;
								switch (mo.type)
								{
									case info.mobjtype_t.MT_KEYGIZMOFLOAT:
										switch (mo.sprite)
										{
											case info.spritenum_t.SPR_KGZY:
												color = new Vector4(1, 1, .5f, 1);
												break;
											case info.spritenum_t.SPR_KGZG:
												color = new Vector4(.75f, 1, .75f, 1);
												break;
											case info.spritenum_t.SPR_KGZB:
												color = new Vector4(.5f, .5f, 1, 1);
												break;
										}
										break;
								}
								Vector3 lPos = Vector3.Zero;
								if (mo.shadowInfo != null &&
									mo.shadowInfo.shadowMap != null)
								{
									Game1.instance.fxDeferred.CurrentTechnique = Game1.instance.fxDeferred.Techniques["TechniqueOmniShadow"];
									Game1.instance.fxDeferred.Parameters["ShadowCubeMap"].SetValue(mo.shadowInfo.shadowMap);
									lPos = mo.shadowInfo.lightPos;
								}
								else
								{
									Game1.instance.fxDeferred.CurrentTechnique = Game1.instance.fxDeferred.Techniques["TechniqueOmni"];
									lPos = new Vector3(
										(mo.x >> DoomDef.FRACBITS),
										(mo.y >> DoomDef.FRACBITS),
										((mo.z + r_data.spritetopoffset[lump]) >> DoomDef.FRACBITS) - texture.Height / 2
									);
								}
								Game1.instance.fxDeferred.Parameters["lPos"].SetValue(lPos);
								Game1.instance.fxDeferred.Parameters["lRadius"].SetValue(mobT.light.radius);

								switch (mobT.light.type)
								{
									case 1:
										{
											float percent = (float)(mo.rnd - ((mo.rnd >> 3) << 3)) / 8.0f;
											float rnd1 = (float)m_misc.P_RandomWithSeed(mo.rnd >> 3) / 255.0f;
											float rnd2 = (float)m_misc.P_RandomWithSeed(mo.rnd >> 3 + 1) / 255.0f;
											float rnd = MathHelper.Lerp(rnd1, rnd2, percent);
											rnd = rnd * .2f + .8f;
											color *= rnd;
											break;
										}
									case 2:
										{
											float percent = (float)(mo.rnd - ((mo.rnd >> 3) << 3)) / 8.0f;
											float rnd1 = (float)m_misc.P_RandomWithSeed(mo.rnd >> 3) / 255.0f;
											float rnd2 = (float)m_misc.P_RandomWithSeed(mo.rnd >> 3 + 1) / 255.0f;
											float rnd = MathHelper.Lerp(rnd1, rnd2, percent);
											rnd = rnd * .1f + .9f;
											color *= rnd;
											break;
										}
									case 3:
										{
											break;
										}
									case 4:
										{
											double percent = (double)(mo.rnd - ((mo.rnd >> 4) << 4)) / 16.0;
											percent = Math.Sin(percent * Math.PI * 2);
											percent = (percent + 1) * .5;
											percent = percent * .5 + .5;
											color *= (float)percent;
											break;
										}
									case 5:
										{
											double percent = (double)(mo.rnd - ((mo.rnd >> 6) << 6)) / 64.0;
											percent = Math.Sin(percent * Math.PI * 2);
											percent = (percent + 1) * .5;
											percent = percent * .5 + .5;
											color *= (float)percent;
											break;
										}
									case 0:
									default:
										break;
								}

								Game1.instance.fxDeferred.Parameters["lColor"].SetValue(color);
								Game1.instance.fxDeferred.CurrentTechnique.Passes[0].Apply();
								Deferred.FullscreenQuad.draw();
							}
						}

						//--- Floor and ceiling lights
						Game1.instance.fxDeferred.CurrentTechnique = Game1.instance.fxDeferred.Techniques["TechniqueFloorLight"];
						foreach (r_local.sector_t sector in p_setup.sectors)
						{
							// Generate light data if it doesn't have one
							if (sector.lightData != null)
							{
								Game1.instance.fxDeferred.Parameters["SpreadTexture"].SetValue(sector.lightData.textureSpread);
								Game1.instance.fxDeferred.Parameters["floorLimits"].SetValue(sector.lightData.limits);
								Game1.instance.fxDeferred.Parameters["floorCeil"].SetValue(new Vector2(sector.floorheight >> DoomDef.FRACBITS, sector.ceilingheight >> DoomDef.FRACBITS));
								Game1.instance.fxDeferred.Parameters["lRadius"].SetValue(sector.lightData.lightInfo.distance);
								Game1.instance.fxDeferred.Parameters["lColor"].SetValue(sector.lightData.lightInfo.color);
								Game1.instance.fxDeferred.CurrentTechnique.Passes[0].Apply();
								Deferred.FullscreenQuad.draw();
							}
						}
						Game1.instance.fxDeferred.CurrentTechnique = Game1.instance.fxDeferred.Techniques["TechniqueCeilingLight"];
						foreach (r_local.sector_t sector in p_setup.sectors)
						{
							// Generate light data if it doesn't have one
							if (sector.lightDataC != null)
							{
								Game1.instance.fxDeferred.Parameters["SpreadTexture"].SetValue(sector.lightDataC.textureSpread);
								Game1.instance.fxDeferred.Parameters["floorLimits"].SetValue(sector.lightDataC.limits);
								Game1.instance.fxDeferred.Parameters["floorCeil"].SetValue(new Vector2(sector.floorheight >> DoomDef.FRACBITS, sector.ceilingheight >> DoomDef.FRACBITS));
								Game1.instance.fxDeferred.Parameters["lRadius"].SetValue(sector.lightDataC.lightInfo.distance);
								Game1.instance.fxDeferred.Parameters["lColor"].SetValue(sector.lightDataC.lightInfo.color);
								Game1.instance.fxDeferred.CurrentTechnique.Passes[0].Apply();
								Deferred.FullscreenQuad.draw();
							}
						}
					}
				}

				if (!Game1.instance.useFreeCam)
					r_thing.R_DrawMasked();

				if (Game1.instance.useDeferred)
				{
					if (Game1.instance.usePostProcess)
					{
						lumData = new byte[4 * 16];
						Game1.instance.lastFrameLevels.GetData(lumData);
						lumDataf[0] = 0;
						lumDataf[1] = 0;
						lumDataf[2] = 0;
						for (int i = 0; i < 16; ++i)
						{
							float w = .3333f;
							if (i % 4 >= 1 &&
								i % 4 <= 2 &&
								i / 4 >= 1 &&
								i / 4 <= 2) w = 3;
							lumDataf[0] += (float)lumData[i * 4 + 0] / 255.0f * w;
							lumDataf[1] += (float)lumData[i * 4 + 1] / 255.0f * w;
							lumDataf[2] += (float)lumData[i * 4 + 2] / 255.0f * w;
						}

						lumLast = (float)gray(
							(double)lumDataf[0] / 16.0,
							(double)lumDataf[1] / 16.0,
							(double)lumDataf[2] / 16.0);

						if (lumLast < LUM_TARGET)
						{
							lumMultiplier += (LUM_TARGET - lumLast) * Game1.instance.dt * LUM_ADAPT_SPEED_LIGHT;
							if (lumMultiplier > LUM_MAX) lumMultiplier = LUM_MAX;
						}
						else if (lumLast > LUM_TARGET)
						{
							lumMultiplier -= (lumLast - LUM_TARGET) * Game1.instance.dt * LUM_ADAPT_SPEED_DARK;
							if (lumMultiplier < LUM_MIN) lumMultiplier = LUM_MIN;
						}

						// Render HDR
						device.SetRenderTarget(Game1.instance.frameBuffer2);
						Game1.instance.fxHDR.Parameters["Viewport"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
						Game1.instance.spriteBatch.Begin(
							SpriteSortMode.Immediate,
							BlendState.Opaque,
							SamplerState.PointClamp,
							DepthStencilState.None,
							RasterizerState.CullNone,
							Game1.instance.fxHDR);
						Game1.instance.fxHDR.CurrentTechnique = Game1.instance.fxHDR.Techniques["TechniqueHDR"];
						Game1.instance.fxHDR.Parameters["Texture0"].SetValue(Game1.instance.frameBuffer);
						Game1.instance.fxHDR.Parameters["lumMultiplier"].SetValue(lumMultiplier);
						Game1.instance.spriteBatch.Draw(
							Game1.instance.frameBuffer,
							new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height),
							Color.White);
						Game1.instance.spriteBatch.End();

						// Render last frame levels (mipmap 1x1)
						device.SetRenderTarget(Game1.instance.lastFrameLevels);
						Game1.instance.spriteBatch.Begin(
							SpriteSortMode.Immediate,
							BlendState.Opaque,
							SamplerState.LinearWrap,
							DepthStencilState.None,
							RasterizerState.CullNone);
						Game1.instance.spriteBatch.Draw(
							Game1.instance.frameBuffer2,
							new Rectangle(0, 0, 4, 4),
							Color.White);
						Game1.instance.spriteBatch.End();

						// Render to the bloom map
						device.SetRenderTarget(Game1.instance.bloom);
						Game1.instance.spriteBatch.Begin(
							SpriteSortMode.Immediate,
							BlendState.Opaque,
							SamplerState.PointClamp,
							DepthStencilState.None,
							RasterizerState.CullNone,
							Game1.instance.fxHDR);
						Game1.instance.fxHDR.CurrentTechnique = Game1.instance.fxHDR.Techniques["TechniqueBloom"];
						Game1.instance.fxHDR.Parameters["Viewport"].SetValue(new Vector2(Game1.instance.bloom.Width, Game1.instance.bloom.Height));
						Game1.instance.fxHDR.Parameters["Texture0"].SetValue(Game1.instance.frameBuffer2);
						Game1.instance.spriteBatch.Draw(
							Game1.instance.frameBuffer2,
							new Rectangle(0, 0, Game1.instance.bloom.Width, Game1.instance.bloom.Height),
							Color.White);
						Game1.instance.spriteBatch.End();

						// Blur bloom U
						device.SetRenderTarget(Game1.instance.bloom2);
						Game1.instance.spriteBatch.Begin(
							SpriteSortMode.Immediate,
							BlendState.Opaque,
							SamplerState.PointClamp,
							DepthStencilState.None,
							RasterizerState.CullNone,
							Game1.instance.fxHDR);
						Game1.instance.fxHDR.CurrentTechnique = Game1.instance.fxHDR.Techniques["TechniqueBloomBlurU"];
						Game1.instance.fxHDR.Parameters["Texture0"].SetValue(Game1.instance.bloom);
						Game1.instance.fxHDR.Parameters["texelSize"].SetValue(Vector2.One / new Vector2(Game1.instance.bloom.Width, Game1.instance.bloom.Height));
						Game1.instance.spriteBatch.Draw(
							Game1.instance.bloom,
							new Rectangle(0, 0, Game1.instance.bloom.Width, Game1.instance.bloom.Height),
							Color.White);
						Game1.instance.spriteBatch.End();

						// Render the final image on the screen, including the final bloom
						device.SetRenderTarget(null);
						Game1.instance.spriteBatch.Begin(
							SpriteSortMode.Immediate,
							BlendState.Opaque,
							SamplerState.PointClamp,
							DepthStencilState.None,
							RasterizerState.CullNone,
							Game1.instance.fxHDR);
						Game1.instance.fxHDR.CurrentTechnique = Game1.instance.fxHDR.Techniques["TechniqueFinalAndBlurBloomV"];
						Game1.instance.fxHDR.Parameters["Viewport"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
						Game1.instance.fxHDR.Parameters["Texture0"].SetValue(Game1.instance.frameBuffer2);
						Game1.instance.fxHDR.Parameters["BloomTexture"].SetValue(Game1.instance.bloom2);
						Game1.instance.spriteBatch.Draw(
							Game1.instance.frameBuffer2,
							new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height),
							Color.White);
						Game1.instance.spriteBatch.End();
					}
				}*/
			}

			// Draw crosshair on top of everything
			if (!Game1.instance.useFreeCam)
			{
				Game1.instance.spriteBatch.Begin();
				int sbBarH = 200 - r_draw.viewheight;
				Game1.instance.spriteBatch.Draw(Game1.instance.crosshair,
					new Rectangle(
						Game1.instance.GraphicsDevice.Viewport.Width / 2 - Game1.instance.crosshair.Width / 2,
						Game1.instance.GraphicsDevice.Viewport.Height / 2 - Game1.instance.crosshair.Height / 2 - sbBarH,
						Game1.instance.crosshair.Width, Game1.instance.crosshair.Height),
						Color.White);
				Game1.instance.spriteBatch.End();
			}
			d_net.NetUpdate();					// check for new console commands
		}

		// sRGB "gamma" function (approx 2.2)
		static double gam_sRGB(double v) {
			if(v<=0.0031308)
				v *= 12.92;
			else 
				v = 1.055*Math.Pow(v,1.0/2.4)-0.055;
			return v;
		}

		// Inverse of sRGB "gamma" function. (approx 2.2)
		static double inv_gam_sRGB(double ic)
		{
			double c = ic;
			if (c <= 0.04045)
				return c / 12.92;
			else
				return Math.Pow(((c + 0.055) / (1.055)), 2.4);
		}

		// sRGB luminance(Y) values
		const double rY = 0.212655;// * 2;
		const double gY = 0.715158;// - 0.072187 - 0.212655;
		const double bY = 0.072187;// * 2;

		// GRAY VALUE ("brightness")
		static double gray(double r, double g, double b)
		{
			return gam_sRGB(
					rY * inv_gam_sRGB(r) +
					gY * inv_gam_sRGB(g) +
					bY * inv_gam_sRGB(b)
			);
		}
	}
}
