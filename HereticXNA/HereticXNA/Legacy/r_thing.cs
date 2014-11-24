using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

// R_things.c

namespace HereticXNA
{
	public static class r_thing
	{
#if DOS

void R_DrawColumn (void);
void R_DrawFuzzColumn (void);

typedef struct
{
	int		x1, x2;

	int		column;
	int		topclip;
	int		bottomclip;
} maskdraw_t;
#endif

/*

Sprite rotation 0 is facing the viewer, rotation 1 is one angle turn CLOCKWISE around the axis.
This is not the same as the angle, which increases counter clockwise
(protractor).  There was a lot of stuff grabbed wrong, so I changed it...

*/


		public static int pspritescale, pspriteiscale;
		public static byte[][] spritelights;

		// constant arrays used for psprite clipping and initializing clipping
		public static short[] negonearray = new short[DoomDef.SCREENWIDTH];
		public static short[] screenheightarray = new short[DoomDef.SCREENWIDTH];

		/*
		===============================================================================

								INITIALIZATION FUNCTIONS

		===============================================================================
		*/

		// variables used to look up and range check thing_t sprites patches
		public static r_local.spritedef_t[] sprites;
		public static int numsprites;

		public static r_local.spriteframe_t[] sprtemp = new r_local.spriteframe_t[26] {
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t(),
	new r_local.spriteframe_t()
};
		public static int maxframe;
		public static string spritename;



		/*
		=================
		=
		= R_InstallSpriteLump
		=
		= Local function for R_InitSprites
		=================
		*/

		public static void R_InstallSpriteLump(int lump, uint frame, uint rotation, bool flipped)
		{
			int r;

			if (frame >= 26 || rotation > 8)
				i_ibm.I_Error("R_InstallSpriteLump: Bad frame characters in lump " + lump);

			if ((int)frame > maxframe)
				maxframe = (int)frame;

			if (rotation == 0)
			{
				// the lump should be used for all rotations
				if (sprtemp[frame].rotate == 0)
					i_ibm.I_Error("R_InitSprites: Sprite " + spritename + " frame " + (char)('A' + frame) + " has multip rot=0 lump");
				if (sprtemp[frame].rotate == 1)
					i_ibm.I_Error("R_InitSprites: Sprite " + spritename + " frame " + (char)('A' + frame) + " has rotations and a rot=0 lump");

				sprtemp[frame].rotate = 0;
				for (r = 0; r < 8; r++)
				{
					sprtemp[frame].lump[r] = (short)(lump - r_data.firstspritelump);
					sprtemp[frame].flip[r] = (byte)(flipped ? 1 : 0);
				}
				return;
			}

			// the lump is only used for one rotation
			if (sprtemp[frame].rotate == 0)
				i_ibm.I_Error("R_InitSprites: Sprite " + spritename + " frame " + (char)('A' + frame) + " has rotations and a rot=0 lump");

			sprtemp[frame].rotate = 1;

			rotation--;		// make 0 based
			if (sprtemp[frame].lump[rotation] != -1)
				i_ibm.I_Error("R_InitSprites: Sprite " + spritename + " : " + (char)('A' + frame) + " : " + (char)('1' + rotation) + " has two lumps mapped to it");

			sprtemp[frame].lump[rotation] = (short)(lump - r_data.firstspritelump);
			sprtemp[frame].flip[rotation] = (byte)(flipped ? 1 : 0);
		}

		/*
		=================
		=
		= R_InitSpriteDefs
		=
		= Pass a null terminated list of sprite names (4 chars exactly) to be used
		= Builds the sprite rotation matrixes to account for horizontally flipped
		= sprites.  Will report an error if the lumps are inconsistant
		=
		Only called at startup
		=
		= Sprite lump names are 4 characters for the actor, a letter for the frame,
		= and a number for the rotation, A sprite that is flippable will have an
		= additional letter/number appended.  The rotation character can be 0 to
		= signify no rotations
		=================
		*/

		public static void R_InitSpriteDefs(string[] namelist)
		{
			int i, l, frame, rotation;
			string intname;
			int start, end;

			// count the number of sprite names
			numsprites = namelist.Length;

			if (numsprites == 0)
				return;

			sprites = new r_local.spritedef_t[numsprites];
			for (i = 0; i < numsprites; ++i)
			{
				sprites[i] = new r_local.spritedef_t();
			}


			start = r_data.firstspritelump - 1;
			end = r_data.lastspritelump + 1;

			// scan all the lump names for each of the names, noting the highest
			// frame letter
			// Just compare 4 characters as ints
			for (i = 0; i < numsprites; i++)
			{
				spritename = namelist[i];
				for (int k = 0; k < sprtemp.Length; ++k)
				{
					sprtemp[k].rotate = -1;
					sprtemp[k].lump[0] = -1;
					sprtemp[k].lump[1] = -1;
					sprtemp[k].lump[2] = -1;
					sprtemp[k].lump[3] = -1;
					sprtemp[k].lump[4] = -1;
					sprtemp[k].lump[5] = -1;
					sprtemp[k].lump[6] = -1;
					sprtemp[k].lump[7] = -1;
					sprtemp[k].flip[0] = 255;
					sprtemp[k].flip[1] = 255;
					sprtemp[k].flip[2] = 255;
					sprtemp[k].flip[3] = 255;
					sprtemp[k].flip[4] = 255;
					sprtemp[k].flip[5] = 255;
					sprtemp[k].flip[6] = 255;
					sprtemp[k].flip[7] = 255;
				}

				maxframe = -1;
				intname = namelist[i];

				//
				// scan the lumps, filling in the frames for whatever is found
				//
				for (l = start + 1; l < end; l++)
					if (w_wad.lumpinfo[l].name.Substring(0, 4) == intname)
					{
						frame = w_wad.lumpinfo[l].name[4] - 'A';
						rotation = w_wad.lumpinfo[l].name[5] - '0';
						R_InstallSpriteLump(l, (uint)frame, (uint)rotation, false);
						if (w_wad.lumpinfo[l].name.Length > 7 && w_wad.lumpinfo[l].name[6] != 0)
						{
							frame = w_wad.lumpinfo[l].name[6] - 'A';
							rotation = w_wad.lumpinfo[l].name[7] - '0';
							R_InstallSpriteLump(l, (uint)frame, (uint)rotation, true);
						}
					}

				//
				// check the frames that were found for completeness
				//
				if (maxframe == -1)
				{
					//continue;
					sprites[i].numframes = 0;
					if (d_main.shareware)
						continue;
					i_ibm.I_Error("R_InitSprites: No lumps found for sprite " + namelist[i]);
				}

				maxframe++;
				for (frame = 0; frame < maxframe; frame++)
				{
					if (sprtemp[frame] == null)
					{
						i_ibm.I_Error("R_InitSprites: No patches found for " + namelist[i] + " frame " + (char)(frame + 'A'));
					}
					else if (sprtemp[frame].rotate != 0)
					{
						for (rotation = 0; rotation < 8; rotation++)
							if (sprtemp[frame].lump[rotation] == -1)
								i_ibm.I_Error("R_InitSprites: Sprite " + namelist[i] + " frame " + (char)(frame + 'A') + " is missing rotations");
					}
				}

				//
				// allocate space for the frames present and copy sprtemp to it
				//
				sprites[i].numframes = maxframe;
				sprites[i].spriteframes = new r_local.spriteframe_t[maxframe];
				for (int k = 0; k < maxframe; ++k)
				{
					sprites[i].spriteframes[k] = new r_local.spriteframe_t();
					sprites[i].spriteframes[k].rotate = sprtemp[k].rotate;
					sprites[i].spriteframes[k].lump[0] = sprtemp[k].lump[0];
					sprites[i].spriteframes[k].lump[1] = sprtemp[k].lump[1];
					sprites[i].spriteframes[k].lump[2] = sprtemp[k].lump[2];
					sprites[i].spriteframes[k].lump[3] = sprtemp[k].lump[3];
					sprites[i].spriteframes[k].lump[4] = sprtemp[k].lump[4];
					sprites[i].spriteframes[k].lump[5] = sprtemp[k].lump[5];
					sprites[i].spriteframes[k].lump[6] = sprtemp[k].lump[6];
					sprites[i].spriteframes[k].lump[7] = sprtemp[k].lump[7];
					sprites[i].spriteframes[k].flip[0] = sprtemp[k].flip[0];
					sprites[i].spriteframes[k].flip[1] = sprtemp[k].flip[1];
					sprites[i].spriteframes[k].flip[2] = sprtemp[k].flip[2];
					sprites[i].spriteframes[k].flip[3] = sprtemp[k].flip[3];
					sprites[i].spriteframes[k].flip[4] = sprtemp[k].flip[4];
					sprites[i].spriteframes[k].flip[5] = sprtemp[k].flip[5];
					sprites[i].spriteframes[k].flip[6] = sprtemp[k].flip[6];
					sprites[i].spriteframes[k].flip[7] = sprtemp[k].flip[7];
				}
			}

		}

		/*
		===============================================================================

									GAME FUNCTIONS

		===============================================================================
		*/

		public static r_local.vissprite_t[] vissprites = new r_local.vissprite_t[r_local.MAXVISSPRITES] {
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),
	new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t(),new r_local.vissprite_t()
};
		public static int vissprite_p;
		public static int newvissprite;

		/*
		===================
		=
		= R_InitSprites
		=
		= Called at program start
		===================
		*/

		public static void R_InitSprites(string[] namelist)
		{
			// [dsl] We won't need that. It's for blitting stuff on the screen
			//int		i;

			//for (i=0 ; i<DoomDef.SCREENWIDTH ; i++)
			//{
			//    negonearray[i] = -1;
			//}

			R_InitSpriteDefs(namelist);
		}

		/*
		===================
		=
		= R_ClearSprites
		=
		= Called at frame start
		===================
		*/

		public static void R_ClearSprites()
		{
			vissprite_p = 0;
		}


		/*
		===================
		=
		= R_NewVisSprite
		=
		===================
		*/

		public static r_local.vissprite_t overflowsprite = new r_local.vissprite_t();

		public static r_local.vissprite_t R_NewVisSprite()
		{
			if (vissprite_p == r_local.MAXVISSPRITES)
				return overflowsprite;
			vissprite_p++;
			return vissprites[vissprite_p - 1];
		}

#if DOS

/*
================
=
= R_DrawMaskedColumn
=
= Used for sprites and masked mid textures
================
*/

short		*mfloorclip;
short		*mceilingclip;
int		spryscale;
int		sprtopscreen;
int		sprbotscreen;

void R_DrawMaskedColumn (column_t *column, signed int baseclip)
{
	int		topscreen, bottomscreen;
	int	basetexturemid;

	basetexturemid = dc_texturemid;

	for ( ; column.topdelta != 0xff ; )
	{
// calculate unclipped screen coordinates for post
		topscreen = sprtopscreen + spryscale*column.topdelta;
		bottomscreen = topscreen + spryscale*column.length;
		dc_yl = (topscreen+FRACUNIT-1)>>FRACBITS;
		dc_yh = (bottomscreen-1)>>FRACBITS;

		if (dc_yh >= mfloorclip[dc_x])
			dc_yh = mfloorclip[dc_x]-1;
		if (dc_yl <= mceilingclip[dc_x])
			dc_yl = mceilingclip[dc_x]+1;

		if(dc_yh >= baseclip && baseclip != -1)
			dc_yh = baseclip;

		if (dc_yl <= dc_yh)
		{
			dc_source = (byte *)column + 3;
			dc_texturemid = basetexturemid - (column.topdelta<<FRACBITS);
//			dc_source = (byte *)column + 3 - column.topdelta;
			colfunc ();		// either R_DrawColumn or R_DrawFuzzColumn
		}
		column = (column_t *)(  (byte *)column + column.length + 4);
	}

	dc_texturemid = basetexturemid;
}
#endif

/*
================
=
= R_DrawVisSprite
=
= mfloorclip and mceilingclip should also be set
================
*/

public static void R_DrawVisSprite(r_local.vissprite_t spr, int x1, int x2)
{
	DoomData.post_t column;
	int			texturecolumn;
	int		frac;
	w_wad.CacheInfo		patch;
	int		baseclip;


	patch = w_wad.W_CacheLumpNum(spr.patch + r_data.firstspritelump, DoomDef.PU_CACHE);
	if (patch == null) return;

	Texture2D texture = patch.cache as Texture2D;

	float lightLevel = (float)spr.thing.subsector.sector.lightlevel / 255.0f;
	Color color = new Color(lightLevel, lightLevel, lightLevel);
	if ((spr.mobjflags & DoomDef.MF_SHADOW) != 0)
	{
		color.A = 150;
	}

	float ratioy = (float)Game1.instance.GraphicsDevice.Viewport.Height / 200.0f;
	float ratiox = ratioy;
	float xOffset =
		((float)Game1.instance.GraphicsDevice.Viewport.Width -
		(320.0f * ratiox)) / 2;

	Game1.instance.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
	int x = (int)((float)x1 * ratiox + xOffset);
	int sbBarH = 200 - r_draw.viewheight;
	int dc_texturemid = spr.texturemid + DoomDef.FixedMul(((r_main.centery - r_draw.viewheight / 2) << DoomDef.FRACBITS), spr.xiscale);
	dc_texturemid >>= DoomDef.FRACBITS;

	int sprOffsetY = spr.texturemid >> DoomDef.FRACBITS;

	int y = (int)((float)r_draw.viewheight * ratioy);
	y -= (int)((float)texture.Height * ratioy * .5f);
	y += (int)((float)sprOffsetY * ratioy);
	y -= (int)((float)sbBarH * ratioy);
	Game1.instance.spriteBatch.Draw(texture,
		new Rectangle(x, y,
			(int)(Math.Ceiling((float)texture.Width * ratiox)),
			(int)(Math.Ceiling((float)texture.Height * ratioy))),
		null,
		color, 0, Vector2.Zero, (spr.flip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
	Game1.instance.spriteBatch.End();

#if DOS
	dc_colormap = vis.colormap;

//	if(!dc_colormap)
//		colfunc = fuzzcolfunc;	// NULL colormap = shadow draw

	if(vis.mobjflags&MF_SHADOW)
	{
		if(vis.mobjflags&MF_TRANSLATION)
		{
			colfunc = R_DrawTranslatedFuzzColumn;
			dc_translation = translationtables - 256 +
				((vis.mobjflags & MF_TRANSLATION) >> (MF_TRANSSHIFT-8));
		}
		else
		{ // Draw using shadow column function
			colfunc = fuzzcolfunc;
		}
	}
	else if(vis.mobjflags&MF_TRANSLATION)
	{
		// Draw using translated column function
		colfunc = R_DrawTranslatedColumn;
		dc_translation = translationtables - 256 +
			( (vis.mobjflags & MF_TRANSLATION) >> (MF_TRANSSHIFT-8) );
	}

	dc_iscale = abs(vis.xiscale)>>detailshift;
	dc_texturemid = vis.texturemid;
	frac = vis.startfrac;
	spryscale = vis.scale;

	sprtopscreen = centeryfrac - FixedMul(dc_texturemid,spryscale);

// check to see if weapon is a vissprite
	if(vis.psprite)
	{
		dc_texturemid += FixedMul(((centery-viewheight/2)<<FRACBITS),
			vis.xiscale);
		sprtopscreen += (viewheight/2-centery)<<FRACBITS;
	}

	if(vis.footclip && !vis.psprite)
	{
		sprbotscreen = sprtopscreen+FixedMul(patch.height<<FRACBITS,
			spryscale);
		baseclip = (sprbotscreen-FixedMul(vis.footclip<<FRACBITS,
			spryscale))>>FRACBITS;
	}
	else
	{
		baseclip = -1;
	}

	for (dc_x=vis.x1 ; dc_x<=vis.x2 ; dc_x++, frac += vis.xiscale)
	{
		texturecolumn = frac>>FRACBITS;
		column = (column_t *) ((byte *)patch +
		 LONG(patch.columnofs[texturecolumn]));
		 R_DrawMaskedColumn (column, baseclip);
	}

	colfunc = basecolfunc;
#endif
}


		/*
===================
=
= R_ProjectSprite
=
= Generates a vissprite for a thing if it might be visible
=
===================
*/

		public static void R_ProjectSprite(DoomDef.mobj_t thing)
		{
			int trx, tryi;
			int gxt, gyt;
			int tx, tz;
			int xscale;
			int x1, x2;
			r_local.spritedef_t sprdef;
			r_local.spriteframe_t sprframe;
			int lump;
			uint rot;
			bool flip;
			int index;
			r_local.vissprite_t vis;
			uint ang;
			int iscale;

			if ((thing.flags2 & DoomDef.MF2_DONTDRAW) != 0)
			{ // Never make a vissprite when MF2_DONTDRAW is flagged.
				return;
			}

			//
			// transform the origin point
			//
			trx = thing.x - r_main.viewx;
			tryi = thing.y - r_main.viewy;

			gxt = DoomDef.FixedMul(trx, r_main.viewcos);
			gyt = -DoomDef.FixedMul(tryi, r_main.viewsin);
			tz = gxt - gyt;

			if (tz < r_local.MINZ)
				return;		// thing is behind view plane
			xscale = d_main.FixedDiv(r_main.projection, tz);

			gxt = -DoomDef.FixedMul(trx, r_main.viewsin);
			gyt = DoomDef.FixedMul(tryi, r_main.viewcos);
			tx = -(gyt + gxt);

			if (Math.Abs(tx) > (tz << 2))
				return;		// too far off the side

			//
			// decide which patch to use for sprite reletive to player
			//
			sprdef = sprites[(int)thing.sprite];
			sprframe = sprdef.spriteframes[thing.frame & DoomDef.FF_FRAMEMASK];

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

			//
			// calculate edges of the shape
			//
			tx -= r_data.spriteoffset[lump];
			x1 = (r_main.centerxfrac + DoomDef.FixedMul(tx, xscale)) >> DoomDef.FRACBITS;
			if (x1 > r_draw.viewwidth)
				return;		// off the right side
			tx += r_data.spritewidth[lump];
			x2 = ((r_main.centerxfrac + DoomDef.FixedMul(tx, xscale)) >> DoomDef.FRACBITS) - 1;
			if (x2 < 0)
				return;		// off the left side


			//
			// store information in a vissprite
			//
			vis = R_NewVisSprite();
			vis.mobjflags = thing.flags;
			vis.thing = thing;
			vis.psprite = false;
			vis.scale = xscale << r_main.detailshift;
			vis.gx = thing.x;
			vis.gy = thing.y;
			vis.gz = thing.z;
			vis.gzt = thing.z + r_data.spritetopoffset[lump];
			vis.flip = flip;

			// foot clipping
			if ((thing.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0
				&& thing.z <= thing.subsector.sector.floorheight)
			{
				vis.footclip = 10;
			}
			else vis.footclip = 0;
			vis.texturemid = vis.gzt - r_main.viewz - (vis.footclip << DoomDef.FRACBITS);

			vis.x1 = x1 < 0 ? 0 : x1;
			vis.x2 = x2 >= r_draw.viewwidth ? r_draw.viewwidth - 1 : x2;
			iscale = d_main.FixedDiv(DoomDef.FRACUNIT, xscale);
			if (flip)
			{
				vis.startfrac = r_data.spritewidth[lump] - 1;
				vis.xiscale = -iscale;
			}
			else
			{
				vis.startfrac = 0;
				vis.xiscale = iscale;
			}
			if (vis.x1 > x1)
				vis.startfrac += vis.xiscale * (vis.x1 - x1);
			vis.patch = lump;
			//
			// get light level
			//

			// TODO: Colormap stuff
			//if (fixedcolormap)
			//    vis.colormap = fixedcolormap;	// fixed map
			//else if (thing.frame & FF_FULLBRIGHT)
			//    vis.colormap = colormaps;		// full bright
			//else
			//{									// diminished light
			//    index = xscale>>(LIGHTSCALESHIFT-detailshift);
			//    if (index >= MAXLIGHTSCALE)
			//        index = MAXLIGHTSCALE-1;
			//    vis.colormap = spritelights[index];
			//}
		}


		/*
		========================
		=
		= R_AddSprites
		=
		========================
		*/

		public static void R_AddSprites(r_local.sector_t sec)
		{
			DoomDef.mobj_t thing;
			int lightnum;

			if (sec.validcount == r_main.validcount)
				return;		// already added

			sec.validcount = r_main.validcount;

			lightnum = (sec.lightlevel >> r_local.LIGHTSEGSHIFT) + r_main.extralight;
			//TODO: Ignore the light stuff for now
			/*	if (lightnum < 0)
					spritelights = scalelight[0];
				else if (lightnum >= r_local.LIGHTLEVELS)
					spritelights = scalelight[r_local.LIGHTLEVELS - 1];
				else
					spritelights = scalelight[lightnum];*/


			for (thing = sec.thinglist; thing != null; thing = thing.snext)
				R_ProjectSprite(thing);
		}


/*
========================
=
= R_DrawPSprite
=
========================
*/

		public static int[] PSpriteSY = new int[(int)DoomDef.weapontype_t.NUMWEAPONS]
{
	0,				// staff
	5*DoomDef.FRACUNIT,		// goldwand
	15*DoomDef.FRACUNIT,	// crossbow
	15*DoomDef.FRACUNIT,	// blaster
	15*DoomDef.FRACUNIT,	// skullrod
	15*DoomDef.FRACUNIT,	// phoenix rod
	15*DoomDef.FRACUNIT,	// mace
	15*DoomDef.FRACUNIT,	// gauntlets
	15*DoomDef.FRACUNIT		// beak
};

public static void R_DrawPSprite (DoomDef.pspdef_t psp)
{
	int		tx;
	int			x1, x2;
	r_local.spritedef_t	sprdef;
	r_local.spriteframe_t sprframe;
	int			lump;
	bool		flip;
	r_local.vissprite_t vis;
	r_local.vissprite_t avis = new r_local.vissprite_t();

	int tempangle;

//
// decide which patch to use
//
	sprdef = sprites[(int)psp.state.sprite];
	sprframe = sprdef.spriteframes[ psp.state.frame & DoomDef.FF_FRAMEMASK ];

	lump = sprframe.lump[0];
	flip = sprframe.flip[0] == 0 ? false : true;

//
// calculate edges of the shape
//
	tx = psp.sx-160*DoomDef.FRACUNIT;

	tx -= r_data.spriteoffset[lump];
	if(r_main.viewangleoffset != 0)
	{
		tempangle = ((r_main.centerxfrac / 1024) * (r_main.viewangleoffset >> (int)DoomDef.ANGLETOFINESHIFT));
	}
	else
	{
		tempangle = 0;
	}
	x1 = (r_main.centerxfrac + DoomDef.FixedMul(tx, pspritescale) + tempangle) >> DoomDef.FRACBITS;
	if (x1 > r_draw.viewwidth)
		return;		// off the right side
	tx +=  r_data.spritewidth[lump];
	x2 = ((r_main.centerxfrac + DoomDef.FixedMul(tx, pspritescale) + tempangle) >> DoomDef.FRACBITS) - 1;
	if (x2 < 0)
		return;		// off the left side

//
// store information in a vissprite
//
	vis = avis;
	vis.mobjflags = 0;
	vis.psprite = true;
	//vis.texturemid = (r_local.BASEYCENTER << DoomDef.FRACBITS) + DoomDef.FRACUNIT / 2 - (psp.sy - r_data.spritetopoffset[lump]);
	vis.texturemid = psp.sy;// (r_local.BASEYCENTER << DoomDef.FRACBITS) + DoomDef.FRACUNIT / 2 - (psp.sy - r_data.spritetopoffset[lump]);
	if (r_draw.viewheight == DoomDef.SCREENHEIGHT)
	{
		vis.texturemid -= PSpriteSY[(int)g_game.players[g_game.consoleplayer].readyweapon];
	}
	vis.x1 = x1 < 0 ? 0 : x1;
	vis.x2 = x2 >= r_draw.viewwidth ? r_draw.viewwidth-1 : x2;
	vis.scale = pspritescale<<r_main.detailshift;
	if (flip)
	{
		vis.xiscale = -pspriteiscale;
		vis.startfrac = r_data.spritewidth[lump]-1;
	}
	else
	{
		vis.xiscale = pspriteiscale;
		vis.startfrac = 0;
	}
	if (vis.x1 > x1)
		vis.startfrac += vis.xiscale*(vis.x1-x1);
	vis.patch = lump;

	if(r_main.viewplayer.powers[(int)DoomDef.powertype_t.pw_invisibility] > 4*32 ||
	(r_main.viewplayer.powers[(int)DoomDef.powertype_t.pw_invisibility] & 8) != 0)
	{
		// Invisibility
	//	vis.colormap = spritelights[MAXLIGHTSCALE-1];
		vis.mobjflags |= DoomDef.MF_SHADOW;
	}
	else if(r_main.fixedcolormap != null)
	{
		// Fixed color
	//	vis.colormap = fixedcolormap;
	}
	else if((psp.state.frame & DoomDef.FF_FULLBRIGHT) != 0)
	{
		// Full bright
	//	vis.colormap = colormaps;
	}
	else
	{
		// local light
	//	vis.colormap = spritelights[r_local.MAXLIGHTSCALE-1];
	}

	vis.thing = g_game.players[g_game.consoleplayer].mo;
	R_DrawVisSprite(vis, vis.x1, vis.x2);
}

/*
========================
=
= R_DrawPlayerSprites
=
========================
*/

public static void R_DrawPlayerSprites ()
{
	int			i, lightnum;
	DoomDef.pspdef_t	psp;

//
// get light level
//
	lightnum = (r_main.viewplayer.mo.subsector.sector.lightlevel >> r_local.LIGHTSEGSHIFT)
		+r_main.extralight;
	//if (lightnum < 0)
	//    spritelights = r_main.scalelight[0];
	//else if (lightnum >= r_local.LIGHTLEVELS)
	//    spritelights = r_main.scalelight[r_local.LIGHTLEVELS - 1];
	//else
	//    spritelights = r_main.scalelight[lightnum];

//
// clip to screen bounds
//
	//mfloorclip = screenheightarray;
	//mceilingclip = negonearray;

//
// add all active psprites
//
	for (i = 0; i < (int)DoomDef.psprnum_t.NUMPSPRITES; i++)
	{
		psp = r_main.viewplayer.psprites[i];
		if (psp.state != null)
			R_DrawPSprite(psp);
	}

}

/*
========================
=
= R_SortVisSprites
=
========================
*/

		public static r_local.vissprite_t vsprsortedhead = new r_local.vissprite_t();

public static void R_SortVisSprites ()
{
	int			i, count;
	r_local.vissprite_t	ds, best;
	r_local.vissprite_t unsorted = new r_local.vissprite_t();
	int		bestscale;

	count = vissprite_p;

	unsorted.next = unsorted.prev = unsorted;
	if (count == 0)
		return;

	for (i = 0; i < vissprite_p; ++i)
	{
		ds = vissprites[i];
		if (i != vissprite_p - 1)
			ds.next = vissprites[i + 1];
		if (i != 0)
			ds.prev = vissprites[i - 1];
	}
	vissprites[0].prev = unsorted;
	unsorted.next = vissprites[0];
	vissprites[vissprite_p - 1].next = unsorted;
	unsorted.prev = vissprites[vissprite_p - 1];

//
// pull the vissprites out by scale
//
	best = null;		// shut up the compiler warning
	vsprsortedhead.next = vsprsortedhead.prev = vsprsortedhead;
	for (i=0 ; i<count ; i++)
	{
		bestscale = DoomDef.MAXINT;
		for (ds=unsorted.next ; ds != unsorted ; ds = ds.next)
		{
			if (ds.scale < bestscale)
			{
				bestscale = ds.scale;
				best = ds;
			}
		}
		best.next.prev = best.prev;
		best.prev.next = best.next;
		best.next = vsprsortedhead;
		best.prev = vsprsortedhead.prev;
		vsprsortedhead.prev.next = best;
		vsprsortedhead.prev = best;
	}
}


/*
========================
=
= R_DrawSprite
=
========================
*/

static HereticXNA.r_local.VertexPositionNormalColorTexture[] verts = new HereticXNA.r_local.VertexPositionNormalColorTexture[4];
public static void R_DrawSprite(r_local.vissprite_t spr)
{
	//int sprId = (int)spr.thing.sprite;
	Texture2D texture = w_wad.W_CacheLumpNum(spr.patch + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Texture2D;
	Game1.instance.fxSprite.Parameters["DiffuseTexture"].SetValue(texture);
	Game1.instance.fxSprite.CurrentTechnique.Passes[0].Apply();
	Vector3 v1 = new Vector3(
			(float)(spr.gx >> DoomDef.FRACBITS) - Game1.instance.camRight.X * (float)texture.Width * .5f,
			(float)(spr.gy >> DoomDef.FRACBITS) - Game1.instance.camRight.Y * (float)texture.Width * .5f,
			(float)(spr.gzt >> DoomDef.FRACBITS) - (float)texture.Height + 4);
	Vector3 v2 = new Vector3(
			(float)(spr.gx >> DoomDef.FRACBITS) + Game1.instance.camRight.X * (float)texture.Width * .5f,
			(float)(spr.gy >> DoomDef.FRACBITS) + Game1.instance.camRight.Y * (float)texture.Width * .5f,
			(float)(spr.gzt >> DoomDef.FRACBITS) + 4);
	// Draw from top default
	float lightLevelT = (float)spr.thing.subsector.sector.lightlevel / 255.0f;
	float lightLevelB = (float)spr.thing.subsector.sector.lightlevel / 255.0f;
	if (Settings.Default.use_deferred)
	{
		lightLevelT = spr.selfIllumT;
		lightLevelB = spr.selfIllumB;
	}
	Color colorT = new Color(lightLevelT, lightLevelT, lightLevelT);
	Color colorB = new Color(lightLevelB, lightLevelB, lightLevelB);
/*	if (Game1.instance.useFreeCam)
		if (Game1.instance.mouseHoverMob == spr.thing)
		{
			colorT = new Color(1, 0, 0);
			colorB = new Color(1, 0, 0);
		}*/
	if ((spr.mobjflags & DoomDef.MF_SHADOW) != 0)
	{
	//	color.A = 150;
	//	Game1.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
	}
	float u1 = 0;
	float u2 = 1;
	if (spr.flip)
	{
		u1 = 1;
		u2 = 0;
	}
	Vector3 normal;
	normal.Z = 0;
	normal.X = Game1.instance.camRight.Y;
	normal.Y = -Game1.instance.camRight.X;
	verts[0] = new HereticXNA.r_local.VertexPositionNormalColorTexture(
		new Vector3(v2.X, v2.Y, v2.Z),
		normal + Vector3.UnitZ + Game1.instance.camRight,
		colorT, new Vector2(u2, 0));
	verts[1] = new HereticXNA.r_local.VertexPositionNormalColorTexture(
		new Vector3(v2.X, v2.Y, v1.Z),
		normal - Vector3.UnitZ + Game1.instance.camRight,
		colorB, new Vector2(u2, 1));
	verts[2] = new HereticXNA.r_local.VertexPositionNormalColorTexture(
		new Vector3(v1.X, v1.Y, v2.Z),
		normal + Vector3.UnitZ - Game1.instance.camRight,
		colorT, new Vector2(u1, 0));
	verts[3] = new HereticXNA.r_local.VertexPositionNormalColorTexture(
		new Vector3(v1.X, v1.Y, v1.Z),
		normal - Vector3.UnitZ - Game1.instance.camRight,
		colorB, new Vector2(u1, 1));

	Game1.instance.GraphicsDevice.DrawUserPrimitives<HereticXNA.r_local.VertexPositionNormalColorTexture>(PrimitiveType.TriangleStrip, verts, 0, 2);
	if ((spr.mobjflags & DoomDef.MF_SHADOW) != 0)
	{
		Game1.instance.GraphicsDevice.BlendState = BlendState.Opaque;
	}
#if DOS
	drawseg_t		*ds;
	short			clipbot[SCREENWIDTH], cliptop[SCREENWIDTH];
	int				x, r1, r2;
	int			scale, lowscale;
	int				silhouette;

	for (x = spr.x1 ; x<=spr.x2 ; x++)
		clipbot[x] = cliptop[x] = -2;

//
// scan drawsegs from end to start for obscuring segs
// the first drawseg that has a greater scale is the clip seg
//
	for (ds=ds_p-1 ; ds >= drawsegs ; ds--)
	{
		//
		// determine if the drawseg obscures the sprite
		//
		if (ds.x1 > spr.x2 || ds.x2 < spr.x1 ||
		(!ds.silhouette && !ds.maskedtexturecol) )
			continue;			// doesn't cover sprite

		r1 = ds.x1 < spr.x1 ? spr.x1 : ds.x1;
		r2 = ds.x2 > spr.x2 ? spr.x2 : ds.x2;
		if (ds.scale1 > ds.scale2)
		{
			lowscale = ds.scale2;
			scale = ds.scale1;
		}
		else
		{
			lowscale = ds.scale1;
			scale = ds.scale2;
		}

		if (scale < spr.scale || ( lowscale < spr.scale
		&& !R_PointOnSegSide (spr.gx, spr.gy, ds.curline) ) )
		{
			if (ds.maskedtexturecol)	// masked mid texture
				R_RenderMaskedSegRange (ds, r1, r2);
			continue;			// seg is behind sprite
		}

//
// clip this piece of the sprite
//
		silhouette = ds.silhouette;
		if (spr.gz >= ds.bsilheight)
			silhouette &= ~SIL_BOTTOM;
		if (spr.gzt <= ds.tsilheight)
			silhouette &= ~SIL_TOP;

		if (silhouette == 1)
		{	// bottom sil
			for (x=r1 ; x<=r2 ; x++)
				if (clipbot[x] == -2)
					clipbot[x] = ds.sprbottomclip[x];
		}
		else if (silhouette == 2)
		{	// top sil
			for (x=r1 ; x<=r2 ; x++)
				if (cliptop[x] == -2)
					cliptop[x] = ds.sprtopclip[x];
		}
		else if (silhouette == 3)
		{	// both
			for (x=r1 ; x<=r2 ; x++)
			{
				if (clipbot[x] == -2)
					clipbot[x] = ds.sprbottomclip[x];
				if (cliptop[x] == -2)
					cliptop[x] = ds.sprtopclip[x];
			}
		}

	}

//
// all clipping has been performed, so draw the sprite
//

// check for unclipped columns
	for (x = spr.x1 ; x<=spr.x2 ; x++)
	{
		if (clipbot[x] == -2)
			clipbot[x] = viewheight;
		if (cliptop[x] == -2)
			cliptop[x] = -1;
	}

	mfloorclip = clipbot;
	mceilingclip = cliptop;
	R_DrawVisSprite (spr, spr.x1, spr.x2);
#endif
}

		/*
========================
=
= R_DrawMasked
=
========================
*/

		public static void R_DrawMasked()
		{
			r_local.vissprite_t spr;
			r_local.drawseg_t ds;

			R_SortVisSprites();

			if (vissprite_p > 0)
			{
				// draw all vissprites back to front

				for (spr = vsprsortedhead.next; spr != vsprsortedhead
				; spr = spr.next)
					R_DrawSprite(spr);
			}

			//
			// render any remaining masked mid textures
			//
			// [dsl] No those are rendered while rendering the map
		/*	for (ds = ds_p - 1; ds >= drawsegs; ds--)
				if (ds.maskedtexturecol)
					R_RenderMaskedSegRange(ds, ds.x1, ds.x2);
			*/

			//
			// draw the psprites on top of everything
			//
			// Added for the sideviewing with an external device
			if (r_main.viewangleoffset <= 1024 << (int)DoomDef.ANGLETOFINESHIFT || r_main.viewangleoffset >=
					-1024 << (int)DoomDef.ANGLETOFINESHIFT)
			{	// don't draw on side views
				R_DrawPlayerSprites();
			}
		}
	}
}
