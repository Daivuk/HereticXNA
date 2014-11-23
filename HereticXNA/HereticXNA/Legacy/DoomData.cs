using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

		// DoomData.h

namespace HereticXNA
{
	public static class DoomData
	{
// all external data is defined here
// most of the data is loaded into different structures at run time

/*
===============================================================================

						map level types

===============================================================================
*/

// lump order in a map wad
public enum eUnknownEnumType1 {ML_LABEL, ML_THINGS, ML_LINEDEFS, ML_SIDEDEFS, ML_VERTEXES, ML_SEGS,
ML_SSECTORS, ML_NODES, ML_SECTORS , ML_REJECT, ML_BLOCKMAP};


public class mapvertex_t
{
	public short		x,y;
} 

public class mapsidedef_t
{
	public short		textureoffset;
	public short		rowoffset;
	public string		toptexture, bottomtexture, midtexture;
	public short		sector;				// on viewer's side
} ;

public class maplinedef_t
{
	public short		v1, v2;
	public short		flags;
	public short		special, tag;
	public short[]		sidenum = new short[2];			// sidenum[1] will be -1 if one sided
} 

public const int	ML_BLOCKING		=	1;
public const int		ML_BLOCKMONSTERS=	2;
public const int		ML_TWOSIDED		=	4;		// backside will not be present at all 
									// if not two sided

// if a texture is pegged, the texture will have the end exposed to air held
// constant at the top or bottom of the texture (stairs or pulled down things)
// and will move with a height change of one of the neighbor sectors
// Unpegged textures allways have the first row of the texture at the top
// pixel of the line for both top and bottom textures (windows)
public const int		ML_DONTPEGTOP	=	8;
public const int		ML_DONTPEGBOTTOM	=16;

public const int	 ML_SECRET		=	32;	// don't map as two sided: IT'S A SECRET!
public const int	 ML_SOUNDBLOCK	=	64;	// don't let sound cross two of these
public const int		ML_DONTDRAW		=	128;	// don't draw on the automap
public const int		ML_MAPPED		=	256;	// set if allready drawn in automap


public class mapsector_t
{
	public short		floorheight, ceilingheight;
	public string		floorpic, ceilingpic;
	public short		lightlevel;
	public short		special, tag;
} ;

public class mapsubsector_t
{
	public short		numsegs;
	public short		firstseg;			// segs are stored sequentially
} ;

public class mapseg_t
{
	public short		v1, v2;
	public short		angle;		
	public short		linedef, side;
	public short		offset;
} ;

public enum eUnknownEnumType2 { BOXTOP, BOXBOTTOM, BOXLEFT, BOXRIGHT };	// bbox coordinates

public const	int NF_SUBSECTOR=	0x8000;
public class mapnode_t
{
	public short		x,y,dx,dy;			// partition line
	public short[,]		bbox = new short[2,4];			// bounding box for each child
	public ushort[]	children = new ushort[2];		// if NF_SUBSECTOR its a subsector
} ;

public class mapthing_t
{
	public short		x,y;
	public short		angle;
	public short		type;
	public short		options;
} ;

public const	int MTF_EASY	=	1;
public const	int MTF_NORMAL	=	2;
public const	int MTF_HARD	=	4;
public const	int MTF_AMBUSH	=	8;

/*
===============================================================================

						texture definition

===============================================================================
*/
public class mappatch_t
{
	public short	originx;
	public short	originy;
	public short	patch;
	public short	stepdir;
	public short	colormap;
}

public class maptexture_t
{
	public string name;
	public bool		masked;	
	public short		width;
	public short height;
	//void** columndirectory;	// OBSOLETE
	public short		patchcount;
	public mappatch_t[]	patches = new mappatch_t[]{new mappatch_t()};
} 

/*
===============================================================================

							graphics

===============================================================================
*/

// posts are runs of non masked source pixels
public class post_t
{
	public byte topdelta;		// -1 is the last post in a column
	public byte length;
// length data bytes follows
}

// column_t is a list of 0 or more post_t, (byte)-1 terminated
// typedef post_t	column_t;

// a patch holds one or more columns
// patches are used for sprites and all masked pictures
public class patch_t
{
	public short		width;				// bounding box size
	public short height;
	public short leftoffset;			// pixels to the left of origin
	public short topoffset;			// pixels below the origin
	public int[] columnofs;		// only [width] used
									// the [0] is &columnofs[width]
}

// a pic is an unmasked block of pixels
public class pic_t
{
	public byte width, height;
	public byte data;
}

#if DOS



/*
===============================================================================

							status

===============================================================================
*/



#endif
	}
}
