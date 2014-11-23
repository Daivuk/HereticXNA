using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
		
// SB_bar.c

namespace HereticXNA
{
	public static class sb_bar
	{
		// Macros

		static Keys CHEAT_ENCRYPT(int c)
		{
			return (Keys)((int)Keys.A + (c - 'a'));
		}

		// Types

		public class CheatDelegate
		{
			public virtual void func(DoomDef.player_t player, Cheat_t cheat) { }
		}

		public class Cheat_t
		{
			public CheatDelegate func;
			public Keys[] sequence;
			public int pos = -1;
			public Keys[] args = new Keys[2];
			public int currentArg;
		} ;

		// Public Data

		public static bool DebugSound; // debug flag for displaying sound info

		public static bool inventory;
		public static int curpos;
		public static int inv_ptr;
		public static int ArtifactFlash;

		// Private Data

		static int HealthMarker;
		static int ChainWiggle;
		static DoomDef.player_t CPlayer;
		public static int playpalette;

		public static w_wad.CacheInfo PatchLTFACE;
		public static w_wad.CacheInfo PatchRTFACE;
		public static w_wad.CacheInfo PatchBARBACK;
		public static w_wad.CacheInfo PatchCHAIN;
		public static w_wad.CacheInfo PatchSTATBAR;
		public static w_wad.CacheInfo PatchLIFEGEM;
		public static w_wad.CacheInfo PatchLTFCTOP;
		public static w_wad.CacheInfo PatchRTFCTOP;
		public static w_wad.CacheInfo PatchSELECTBOX;
		public static w_wad.CacheInfo PatchINVLFGEM1;
		public static w_wad.CacheInfo PatchINVLFGEM2;
		public static w_wad.CacheInfo PatchINVRTGEM1;
		public static w_wad.CacheInfo PatchINVRTGEM2;
		public static w_wad.CacheInfo[] PatchINumbers = new w_wad.CacheInfo[10];
		public static w_wad.CacheInfo PatchNEGATIVE;
		public static w_wad.CacheInfo[] PatchSmNumbers = new w_wad.CacheInfo[10];
		public static w_wad.CacheInfo PatchBLACKSQ;
		public static w_wad.CacheInfo PatchINVBAR;
		public static w_wad.CacheInfo PatchARMCLEAR;
		public static w_wad.CacheInfo PatchCHAINBACK;
		public static int FontBNumBase;
		public static int spinbooklump;
		public static int spinflylump;

		static Keys[] CheatLookup = new Keys[256];

		// Toggle god mode
		static Keys[] CheatGodSeq = new Keys[]
{
	CHEAT_ENCRYPT('q'),
	CHEAT_ENCRYPT('u'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('c'),
	CHEAT_ENCRYPT('k'),
	CHEAT_ENCRYPT('e'),
	CHEAT_ENCRYPT('n')
};

		// Toggle no clipping mode
		static Keys[] CheatNoClipSeq = new Keys[]
{
	CHEAT_ENCRYPT('k'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('t'),
	CHEAT_ENCRYPT('t'),
	CHEAT_ENCRYPT('y')
};

		// Get all weapons and ammo
		static Keys[] CheatWeaponsSeq = new Keys[]
{
	CHEAT_ENCRYPT('r'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('b'),
	CHEAT_ENCRYPT('o')
};

		// Toggle tome of power
		static Keys[] CheatPowerSeq = new Keys[]
{
	CHEAT_ENCRYPT('s'),
	CHEAT_ENCRYPT('h'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('z'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('m')
};

		// Get full health
		static Keys[] CheatHealthSeq = new Keys[]
{
	CHEAT_ENCRYPT('p'),
	CHEAT_ENCRYPT('o'),
	CHEAT_ENCRYPT('n'),
	CHEAT_ENCRYPT('c'),
	CHEAT_ENCRYPT('e')
};

		// Get all keys
		static Keys[] CheatKeysSeq = new Keys[]
{
	CHEAT_ENCRYPT('s'),
	CHEAT_ENCRYPT('k'),
	CHEAT_ENCRYPT('e'),
	CHEAT_ENCRYPT('l')
};

		// Toggle sound debug info
		static Keys[] CheatSoundSeq = new Keys[]
{
	CHEAT_ENCRYPT('n'),
	CHEAT_ENCRYPT('o'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('s'),
	CHEAT_ENCRYPT('e')
};

		// Toggle ticker
		static Keys[] CheatTickerSeq = new Keys[]
{
	CHEAT_ENCRYPT('t'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('c'),
	CHEAT_ENCRYPT('k'),
	CHEAT_ENCRYPT('e'),
	CHEAT_ENCRYPT('r')
};

		// Get an artifact 1st stage (ask for type)
		static Keys[] CheatArtifact1Seq = new Keys[]
{
	CHEAT_ENCRYPT('g'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('e')
};

		// Get an artifact 2nd stage (ask for count)
		static Keys[] CheatArtifact2Seq = new Keys[]
{
	CHEAT_ENCRYPT('g'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('e')
};

		// Get an artifact final stage
		static Keys[] CheatArtifact3Seq = new Keys[]
{
	CHEAT_ENCRYPT('g'),
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('e')
};

		// Warp to new level
		static Keys[] CheatWarpSeq = new Keys[]
{
	CHEAT_ENCRYPT('e'),
	CHEAT_ENCRYPT('n'),
	CHEAT_ENCRYPT('g'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('g'),
	CHEAT_ENCRYPT('e')
};

		// Save a screenshot
		static Keys[] CheatChickenSeq = new Keys[]
{
	CHEAT_ENCRYPT('c'),
	CHEAT_ENCRYPT('o'),
	CHEAT_ENCRYPT('c'),
	CHEAT_ENCRYPT('k'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('d'),
	CHEAT_ENCRYPT('o'),
	CHEAT_ENCRYPT('o'),
	CHEAT_ENCRYPT('d'),
	CHEAT_ENCRYPT('l'),
	CHEAT_ENCRYPT('e'),
	CHEAT_ENCRYPT('d'),
	CHEAT_ENCRYPT('o'),
	CHEAT_ENCRYPT('o')
};

		// Kill all monsters
		static Keys[] CheatMassacreSeq = new Keys[]
{
	CHEAT_ENCRYPT('m'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('s'),
	CHEAT_ENCRYPT('s'),
	CHEAT_ENCRYPT('a'),
	CHEAT_ENCRYPT('c'),
	CHEAT_ENCRYPT('r'),
	CHEAT_ENCRYPT('e')
};

		static Keys[] CheatIDKFASeq = new Keys[]
{
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('d'),
	CHEAT_ENCRYPT('k'),
	CHEAT_ENCRYPT('f'),
	CHEAT_ENCRYPT('a')
};

		static Keys[] CheatIDDQDSeq = new Keys[]
{
	CHEAT_ENCRYPT('i'),
	CHEAT_ENCRYPT('d'),
	CHEAT_ENCRYPT('d'),
	CHEAT_ENCRYPT('q'),
	CHEAT_ENCRYPT('d')
};
		static Keys[] CheatEMSeq = new Keys[]
{
	CHEAT_ENCRYPT('e'),
	Keys.D3,
	CHEAT_ENCRYPT('m'),
	Keys.D3
};

		public static Cheat_t[] Cheats = new Cheat_t[]
{
	new Cheat_t { func = new CheatGodFunc(), sequence = CheatGodSeq },
	new Cheat_t { func = new CheatNoClipFunc(), sequence = CheatNoClipSeq },
	new Cheat_t { func = new CheatWeaponsFunc(), sequence = CheatWeaponsSeq },
	new Cheat_t { func = new CheatPowerFunc(), sequence = CheatPowerSeq },
	new Cheat_t { func = new CheatHealthFunc(),sequence =  CheatHealthSeq },
	new Cheat_t { func = new CheatKeysFunc(), sequence = CheatKeysSeq },
	new Cheat_t { func = new CheatSoundFunc(), sequence = CheatSoundSeq },
	new Cheat_t { func = new CheatTickerFunc(), sequence = CheatTickerSeq },
	new Cheat_t { func = new CheatArtifact1Func(),sequence =  CheatArtifact1Seq },
	new Cheat_t { func = new CheatArtifact2Func(), sequence = CheatArtifact2Seq },
	new Cheat_t { func = new CheatArtifact3Func(), sequence = CheatArtifact3Seq },
	new Cheat_t { func = new CheatWarpFunc(), sequence = CheatWarpSeq },
	new Cheat_t { func = new CheatChickenFunc(), sequence = CheatChickenSeq },
	new Cheat_t { func = new CheatMassacreFunc(), sequence = CheatMassacreSeq },
	new Cheat_t { func = new CheatIDKFAFunc(), sequence = CheatIDKFASeq },
	new Cheat_t { func = new CheatIDDQDFunc(), sequence = CheatIDDQDSeq },
	new Cheat_t { func = new CheatEMFunc(), sequence = CheatEMSeq }
};

		//---------------------------------------------------------------------------
		//
		// PROC SB_Init
		//
		//---------------------------------------------------------------------------

		public static void SB_Init()
		{
			int i;
			int startLump;

			PatchLTFACE = w_wad.W_CacheLumpName("LTFACE", DoomDef.PU_STATIC);
			PatchRTFACE = w_wad.W_CacheLumpName("RTFACE", DoomDef.PU_STATIC);
			PatchBARBACK = w_wad.W_CacheLumpName("BARBACK", DoomDef.PU_STATIC);
			PatchINVBAR = w_wad.W_CacheLumpName("INVBAR", DoomDef.PU_STATIC);
			PatchCHAIN = w_wad.W_CacheLumpName("CHAIN", DoomDef.PU_STATIC);
			if (g_game.deathmatch)
			{
				PatchSTATBAR = w_wad.W_CacheLumpName("STATBAR", DoomDef.PU_STATIC);
			}
			else
			{
				PatchSTATBAR = w_wad.W_CacheLumpName("LIFEBAR", DoomDef.PU_STATIC);
			}
			if (!g_game.netgame)
			{ // single player game uses red life gem
				PatchLIFEGEM = w_wad.W_CacheLumpName("LIFEGEM2", DoomDef.PU_STATIC);
			}
			else
			{
				PatchLIFEGEM = w_wad.W_CacheLumpNum(w_wad.W_GetNumForName("LIFEGEM0")
					+ g_game.consoleplayer, DoomDef.PU_STATIC);
			}
			PatchLTFCTOP = w_wad.W_CacheLumpName("LTFCTOP", DoomDef.PU_STATIC);
			PatchRTFCTOP = w_wad.W_CacheLumpName("RTFCTOP", DoomDef.PU_STATIC);
			PatchSELECTBOX = w_wad.W_CacheLumpName("SELECTBOX", DoomDef.PU_STATIC);
			PatchINVLFGEM1 = w_wad.W_CacheLumpName("INVGEML1", DoomDef.PU_STATIC);
			PatchINVLFGEM2 = w_wad.W_CacheLumpName("INVGEML2", DoomDef.PU_STATIC);
			PatchINVRTGEM1 = w_wad.W_CacheLumpName("INVGEMR1", DoomDef.PU_STATIC);
			PatchINVRTGEM2 = w_wad.W_CacheLumpName("INVGEMR2", DoomDef.PU_STATIC);
			PatchBLACKSQ = w_wad.W_CacheLumpName("BLACKSQ", DoomDef.PU_STATIC);
			PatchARMCLEAR = w_wad.W_CacheLumpName("ARMCLEAR", DoomDef.PU_STATIC);
			PatchCHAINBACK = w_wad.W_CacheLumpName("CHAINBACK", DoomDef.PU_STATIC);
			startLump = w_wad.W_GetNumForName("IN0");
			for (i = 0; i < 10; i++)
			{
				sb_bar.PatchINumbers[i] = w_wad.W_CacheLumpNum(startLump + i, DoomDef.PU_STATIC);
			}
			PatchNEGATIVE = w_wad.W_CacheLumpName("NEGNUM", DoomDef.PU_STATIC);
			FontBNumBase = w_wad.W_GetNumForName("FONTB16");
			startLump = w_wad.W_GetNumForName("SMALLIN0");
			for (i = 0; i < 10; i++)
			{
				PatchSmNumbers[i] = w_wad.W_CacheLumpNum(startLump + i, DoomDef.PU_STATIC);
			}
			playpalette = w_wad.W_GetNumForName("PLAYPAL");
			spinbooklump = w_wad.W_GetNumForName("SPINBK0");
			spinflylump = w_wad.W_GetNumForName("SPFLY0");
			for (i = 0; i < 256; i++)
			{
				CheatLookup[i] = CHEAT_ENCRYPT(i);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SB_Ticker
		//
		//---------------------------------------------------------------------------

		public static void SB_Ticker()
		{
			int delta;
			int curHealth;

			if ((p_tick.leveltime & 1) != 0)
			{
				ChainWiggle = m_misc.P_Random() & 1;
			}
			curHealth = g_game.players[g_game.consoleplayer].mo.health;
			if (curHealth < 0)
			{
				curHealth = 0;
			}
			if (curHealth < HealthMarker)
			{
				delta = (HealthMarker - curHealth) >> 2;
				if (delta < 1)
				{
					delta = 1;
				}
				else if (delta > 8)
				{
					delta = 8;
				}
				HealthMarker -= delta;
			}
			else if (curHealth > HealthMarker)
			{
				delta = (curHealth - HealthMarker) >> 2;
				if (delta < 1)
				{
					delta = 1;
				}
				else if (delta > 8)
				{
					delta = 8;
				}
				HealthMarker += delta;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrINumber
		//
		// Draws a three digit number.
		//
		//---------------------------------------------------------------------------

		static void DrINumber(int val, int x, int y)
		{
			w_wad.CacheInfo patch;
			int oldval;

			oldval = val;
			if (val < 0)
			{
				if (val < -9)
				{
					v_video.V_DrawPatch(x + 1, y + 1, w_wad.W_CacheLumpName("LAME", DoomDef.PU_CACHE));
				}
				else
				{
					val = -val;
					v_video.V_DrawPatch(x + 18, y, PatchINumbers[val]);
					v_video.V_DrawPatch(x + 9, y, PatchNEGATIVE);
				}
				return;
			}
			if (val > 99)
			{
				patch = PatchINumbers[val / 100];
				v_video.V_DrawPatch(x, y, patch);
			}
			val = val % 100;
			if (val > 9 || oldval > 99)
			{
				patch = PatchINumbers[val / 10];
				v_video.V_DrawPatch(x + 9, y, patch);
			}
			val = val % 10;
			patch = PatchINumbers[val];
			v_video.V_DrawPatch(x + 18, y, patch);
		}

#if DOS

//---------------------------------------------------------------------------
//
// PROC DrBNumber
//
// Draws a three digit number using FontB
//
//---------------------------------------------------------------------------

static void DrBNumber(signed int val, int x, int y)
{
	patch_t *patch;
	int xpos;
	int oldval;

	oldval = val;
	xpos = x;
	if(val < 0)
	{
		val = 0;
	}
	if(val > 99)
	{
		patch = W_CacheLumpNum(FontBNumBase+val/100, PU_CACHE);
		V_DrawShadowedPatch(xpos+6-patch.width/2, y, patch);
	}
	val = val%100;
	xpos += 12;
	if(val > 9 || oldval > 99)
	{
		patch = W_CacheLumpNum(FontBNumBase+val/10, PU_CACHE);
		V_DrawShadowedPatch(xpos+6-patch.width/2, y, patch);
	}
	val = val%10;
	xpos += 12;
	patch = W_CacheLumpNum(FontBNumBase+val, PU_CACHE);
	V_DrawShadowedPatch(xpos+6-patch.width/2, y, patch);
}

#endif

		//---------------------------------------------------------------------------
		//
		// PROC DrSmallNumber
		//
		// Draws a small two digit number.
		//
		//---------------------------------------------------------------------------

		static void DrSmallNumber(int val, int x, int y)
		{
			w_wad.CacheInfo patch;

			if (val == 1)
			{
				return;
			}
			if (val > 9)
			{
				patch = PatchSmNumbers[val / 10];
				v_video.V_DrawPatch(x, y, patch);
			}
			val = val % 10;
			patch = PatchSmNumbers[val];
			v_video.V_DrawPatch(x + 4, y, patch);
		}

		//---------------------------------------------------------------------------
		//
		// PROC ShadeLine
		//
		//---------------------------------------------------------------------------

		static void ShadeLine(int x, int y, int height, int shade)
		{
			//byte *dest;
			//byte *shades;

			//shades = colormaps+9*256+shade*2*256;
			//dest = screen+y*SCREENWIDTH+x;
			//while(height--)
			//{
			//    *(dest) = *(shades+*dest);
			//    dest += SCREENWIDTH;
			//}
		}

		//---------------------------------------------------------------------------
		//
		// PROC ShadeChain
		//
		//---------------------------------------------------------------------------

		static void ShadeChain()
		{
			int i;

			for (i = 0; i < 16; i++)
			{
				ShadeLine(277 + i, 190, 10, i / 2);
				ShadeLine(19 + i, 190, 10, 7 - (i / 2));
			}
		}

#if DOS

//---------------------------------------------------------------------------
//
// PROC DrawSoundInfo
//
// Displays sound debugging information.
//
//---------------------------------------------------------------------------

static void DrawSoundInfo(void)
{
	int i;
	SoundInfo_t s;
	ChanInfo_t *c;
	char text[32];
	int x;
	int y;
	int xPos[7] = {1, 75, 112, 156, 200, 230, 260};

	if(leveltime&16)
	{
		MN_DrTextA("*** SOUND DEBUG INFO ***", xPos[0], 20);
	}
	S_GetChannelInfo(&s);
	if(s.channelCount == 0)
	{
		return;
	}
	x = 0;
	MN_DrTextA("NAME", xPos[x++], 30);
	MN_DrTextA("MO.T", xPos[x++], 30);
	MN_DrTextA("MO.X", xPos[x++], 30);
	MN_DrTextA("MO.Y", xPos[x++], 30);
	MN_DrTextA("ID", xPos[x++], 30);
	MN_DrTextA("PRI", xPos[x++], 30);
	MN_DrTextA("DIST", xPos[x++], 30);
	for(i = 0; i < s.channelCount; i++)
	{
		c = &s.chan[i];
		x = 0;
		y = 40+i*10;
		if(c.mo == NULL)
		{ // Channel is unused
			MN_DrTextA("------", xPos[0], y);
			continue;
		}
		sprintf(text, "%s", c.name);
		M_ForceUppercase(text);
		MN_DrTextA(text, xPos[x++], y);
		sprintf(text, "%d", c.mo.type);
		MN_DrTextA(text, xPos[x++], y);
		sprintf(text, "%d", c.mo.x>>FRACBITS);
		MN_DrTextA(text, xPos[x++], y);
		sprintf(text, "%d", c.mo.y>>FRACBITS);
		MN_DrTextA(text, xPos[x++], y);
		sprintf(text, "%d", c.id);
		MN_DrTextA(text, xPos[x++], y);
		sprintf(text, "%d", c.priority);
		MN_DrTextA(text, xPos[x++], y);
		sprintf(text, "%d", c.distance);
		MN_DrTextA(text, xPos[x++], y);
	}
	UpdateState |= I_FULLSCRN;
	BorderNeedRefresh = true;
}
#endif

		//---------------------------------------------------------------------------
		//
		// PROC SB_Drawer
		//
		//---------------------------------------------------------------------------

		public static string[] patcharti = new string[11]
{
	"ARTIBOX",    // none
	"ARTIINVU",   // invulnerability
	"ARTIINVS",   // invisibility
	"ARTIPTN2",   // health
	"ARTISPHL",   // superhealth
	"ARTIPWBK",   // tomeofpower
	"ARTITRCH",   // torch
	"ARTIFBMB",   // firebomb
	"ARTIEGGC",   // egg
	"ARTISOAR",   // fly
	"ARTIATLP"    // teleport
};

		public static string[] ammopic = new string[6]
{
	"INAMGLD",
	"INAMBOW",
	"INAMBST",
	"INAMRAM",
	"INAMPNX",
	"INAMLOB"
};

		public static int SB_state = -1;
		public static int oldarti = 0;
		public static int oldartiCount = 0;
		public static int oldfrags = -9999;
		public static int oldammo = -1;
		public static int oldarmor = -1;
		public static int oldweapon = -1;
		public static int oldhealth = -1;
		public static int oldlife = -1;
		public static int oldkeys = -1;
		public static int playerkeys = 0;

		static bool hitCenterFrame;
		public static void SB_Drawer()
		{
			if (Game1.instance.useFreeCam) return;

			int frame;

			// Sound info debug stuff
			//if(DebugSound == true)
			//{
			//    DrawSoundInfo();
			//}
			CPlayer = g_game.players[g_game.consoleplayer];
			//if(DoomDef.viewheight == DoomDef.SCREENHEIGHT && !am_map.automapactive)
			//{
			//    DrawFullScreenStuff();
			//    SB_state = -1;
			//}
			//else
			{
				//	if(SB_state == -1)
				{
					v_video.V_DrawPatch(0, 158, PatchBARBACK);
					if ((g_game.players[g_game.consoleplayer].cheats & DoomDef.CF_GODMODE) != 0)
					{
						v_video.V_DrawPatch(16, 167, w_wad.W_CacheLumpName("GOD1", DoomDef.PU_CACHE));
						v_video.V_DrawPatch(287, 167, w_wad.W_CacheLumpName("GOD2", DoomDef.PU_CACHE));
					}
					oldhealth = -1;
				}
				DrawCommonBar();
				if (!inventory)
				{
					//	if(SB_state != 0)
					{
						// Main interface
						v_video.V_DrawPatch(34, 160, PatchSTATBAR);
						oldarti = 0;
						oldammo = -1;
						oldarmor = -1;
						oldweapon = -1;
						oldfrags = -9999; //can't use -1, 'cuz of negative frags
						oldlife = -1;
						oldkeys = -1;
					}
					DrawMainBar();
					SB_state = 0;
				}
				else
				{
					//	if(SB_state != 1)
					{
						v_video.V_DrawPatch(34, 160, PatchINVBAR);
					}
					DrawInventoryBar();
					SB_state = 1;
				}
			}
			SB_PaletteFlash();

			// Flight icons
			if (CPlayer.powers[(int)DoomDef.powertype_t.pw_flight] != 0)
			{
				if (CPlayer.powers[(int)DoomDef.powertype_t.pw_flight] > DoomDef.BLINKTHRESHOLD
					|| (CPlayer.powers[(int)DoomDef.powertype_t.pw_flight] & 16) == 0)
				{
					frame = (p_tick.leveltime / 3) & 15;
					if ((CPlayer.mo.flags2 & DoomDef.MF2_FLY) != 0)
					{
						if (hitCenterFrame && (frame != 15 && frame != 0))
						{
							v_video.V_DrawPatch(20, 17, w_wad.W_CacheLumpNum(spinflylump + 15,
								DoomDef.PU_CACHE));
						}
						else
						{
							v_video.V_DrawPatch(20, 17, w_wad.W_CacheLumpNum(spinflylump + frame,
								DoomDef.PU_CACHE));
							hitCenterFrame = false;
						}
					}
					else
					{
						if (!hitCenterFrame && (frame != 15 && frame != 0))
						{
							v_video.V_DrawPatch(20, 17, w_wad.W_CacheLumpNum(spinflylump + frame,
								DoomDef.PU_CACHE));
							hitCenterFrame = false;
						}
						else
						{
							v_video.V_DrawPatch(20, 17, w_wad.W_CacheLumpNum(spinflylump + 15,
								DoomDef.PU_CACHE));
							hitCenterFrame = true;
						}
					}
					//BorderTopRefresh = true;
					i_ibm.UpdateState |= DoomDef.I_MESSAGES;
				}
				else
				{
					//BorderTopRefresh = true;
					i_ibm.UpdateState |= DoomDef.I_MESSAGES;
				}
			}

			if (CPlayer.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0 && CPlayer.chickenTics == 0)
			{
				if (CPlayer.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] > DoomDef.BLINKTHRESHOLD
					|| (CPlayer.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] & 16) == 0)
				{
					frame = (p_tick.leveltime / 3) & 15;
					v_video.V_DrawPatch(300, 17, w_wad.W_CacheLumpNum(spinbooklump + frame, DoomDef.PU_CACHE));
					//BorderTopRefresh = true;
					i_ibm.UpdateState |= DoomDef.I_MESSAGES;
				}
				else
				{
					//BorderTopRefresh = true;
					i_ibm.UpdateState |= DoomDef.I_MESSAGES;
				}
			}
		}

		// sets the new palette based upon current values of player.damagecount
		// and player.bonuscount
		static int sb_palette = 0;
		public static void SB_PaletteFlash()
		{
			int palette;
			byte[] pal;
			int pali;

			CPlayer = g_game.players[g_game.consoleplayer];

			if (CPlayer.damagecount != 0)
			{
				palette = (CPlayer.damagecount + 7) >> 3;
				if (palette >= p_local.NUMREDPALS)
				{
					palette = p_local.NUMREDPALS - 1;
				}
				palette += p_local.STARTREDPALS;
			}
			else if (CPlayer.bonuscount != 0)
			{
				palette = (CPlayer.bonuscount + 7) >> 3;
				if (palette >= p_local.NUMBONUSPALS)
				{
					palette = p_local.NUMBONUSPALS - 1;
				}
				palette += p_local.STARTBONUSPALS;
			}
			else
			{
				palette = 0;
			}
			if (palette != sb_palette)
			{
				sb_palette = palette;
				pali = palette * 768;
				pal = w_wad.W_CacheLumpNum(playpalette, DoomDef.PU_CACHE).data;
				//I_SetPalette(pal);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawCommonBar
		//
		//---------------------------------------------------------------------------

		static void DrawCommonBar()
		{
			int chainY;
			int healthPos;

			v_video.V_DrawPatch(0, 148, PatchLTFCTOP);
			v_video.V_DrawPatch(290, 148, PatchRTFCTOP);

			if (oldhealth != HealthMarker)
			{
				oldhealth = HealthMarker;
				healthPos = HealthMarker;
				if (healthPos < 0)
				{
					healthPos = 0;
				}
				if (healthPos > 100)
				{
					healthPos = 100;
				}
				healthPos = (healthPos * 256) / 100;
				chainY = (HealthMarker == CPlayer.mo.health) ? 191 : 191 + ChainWiggle;
				v_video.V_DrawPatch(0, 190, PatchCHAINBACK);
				v_video.V_DrawPatch(2 + (healthPos % 17), chainY, PatchCHAIN);
				v_video.V_DrawPatch(17 + healthPos, chainY, PatchLIFEGEM);
				v_video.V_DrawPatch(0, 190, PatchLTFACE);
				v_video.V_DrawPatch(276, 190, PatchRTFACE);
				ShadeChain();
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawMainBar
		//
		//---------------------------------------------------------------------------

		public static void DrawMainBar()
		{
			int i;
			int temp;

			// Ready artifact
			if (sb_bar.ArtifactFlash != 0)
			{
				v_video.V_DrawPatch(180, 161, PatchBLACKSQ);
				v_video.V_DrawPatch(182, 161, w_wad.W_CacheLumpNum(w_wad.W_GetNumForName("useartia")
					+ ArtifactFlash - 1, DoomDef.PU_CACHE));
				ArtifactFlash--;
				oldarti = -1; // so that the correct artifact fills in after the flash
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}
			else if (oldarti != (int)CPlayer.readyArtifact
				|| oldartiCount != CPlayer.inventory[inv_ptr].count)
			{
				v_video.V_DrawPatch(180, 161, PatchBLACKSQ);
				if (CPlayer.readyArtifact > 0)
				{
					v_video.V_DrawPatch(179, 160, w_wad.W_CacheLumpName(patcharti[(int)CPlayer.readyArtifact],
						DoomDef.PU_CACHE));
					DrSmallNumber(CPlayer.inventory[inv_ptr].count, 201, 182);
				}
				oldarti = (int)CPlayer.readyArtifact;
				oldartiCount = CPlayer.inventory[inv_ptr].count;
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}

			// Frags
			if (g_game.deathmatch)
			{
				temp = 0;
				for (i = 0; i < DoomDef.MAXPLAYERS; i++)
				{
					temp += CPlayer.frags[i];
				}
				if (temp != oldfrags)
				{
					v_video.V_DrawPatch(57, 171, PatchARMCLEAR);
					DrINumber(temp, 61, 170);
					oldfrags = temp;
					i_ibm.UpdateState |= DoomDef.I_STATBAR;
				}
			}
			else
			{
				temp = HealthMarker;
				if (temp < 0)
				{
					temp = 0;
				}
				else if (temp > 100)
				{
					temp = 100;
				}
				if (oldlife != temp)
				{
					oldlife = temp;
					v_video.V_DrawPatch(57, 171, PatchARMCLEAR);
					DrINumber(temp, 61, 170);
					i_ibm.UpdateState |= DoomDef.I_STATBAR;
				}
			}

			// Keys
			if (oldkeys != playerkeys)
			{
				if (CPlayer.keys[(int)DoomDef.keytype_t.key_yellow])
				{
					v_video.V_DrawPatch(153, 164, w_wad.W_CacheLumpName("ykeyicon", DoomDef.PU_CACHE));
				}
				if (CPlayer.keys[(int)DoomDef.keytype_t.key_green])
				{
					v_video.V_DrawPatch(153, 172, w_wad.W_CacheLumpName("gkeyicon", DoomDef.PU_CACHE));
				}
				if (CPlayer.keys[(int)DoomDef.keytype_t.key_blue])
				{
					v_video.V_DrawPatch(153, 180, w_wad.W_CacheLumpName("bkeyicon", DoomDef.PU_CACHE));
				}
				oldkeys = playerkeys;
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}
			// Ammo
			if ((int)p_pspr.wpnlev1info[(int)CPlayer.readyweapon].ammo >= CPlayer.ammo.Length)
				temp = 0;
			else
				temp = CPlayer.ammo[(int)p_pspr.wpnlev1info[(int)CPlayer.readyweapon].ammo];
			if (oldammo != temp || oldweapon != (int)CPlayer.readyweapon)
			{
				v_video.V_DrawPatch(108, 161, PatchBLACKSQ);
				if (temp != 0 && (int)CPlayer.readyweapon > 0 && (int)CPlayer.readyweapon < 7)
				{
					DrINumber(temp, 109, 162);
					v_video.V_DrawPatch(111, 172, w_wad.W_CacheLumpName(
						ammopic[(int)CPlayer.readyweapon - 1], DoomDef.PU_CACHE));
				}
				oldammo = temp;
				oldweapon = (int)CPlayer.readyweapon;
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}

			// Armor
			if (oldarmor != CPlayer.armorpoints)
			{
				v_video.V_DrawPatch(224, 171, PatchARMCLEAR);
				DrINumber(CPlayer.armorpoints, 228, 170);
				oldarmor = CPlayer.armorpoints;
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawInventoryBar
		//
		//---------------------------------------------------------------------------

		public static void DrawInventoryBar()
		{
			int i;
			int x;

			x = inv_ptr - curpos;
			i_ibm.UpdateState |= DoomDef.I_STATBAR;
			v_video.V_DrawPatch(34, 160, PatchINVBAR);
			for (i = 0; i < 7; i++)
			{
				//V_DrawPatch(50+i*31, 160, W_CacheLumpName("ARTIBOX", PU_CACHE));
				if (CPlayer.inventorySlotNum > x + i
					&& CPlayer.inventory[x + i].type != (int)DoomDef.artitype_t.arti_none)
				{
					v_video.V_DrawPatch(50 + i * 31, 160, w_wad.W_CacheLumpName(
						patcharti[CPlayer.inventory[x + i].type], DoomDef.PU_CACHE));
					DrSmallNumber(CPlayer.inventory[x + i].count, 69 + i * 31, 182);
				}
			}
			v_video.V_DrawPatch(50 + curpos * 31, 189, PatchSELECTBOX);
			if (x != 0)
			{
				v_video.V_DrawPatch(38, 159, (p_tick.leveltime & 4) == 0 ? PatchINVLFGEM1 :
					PatchINVLFGEM2);
			}
			if (CPlayer.inventorySlotNum - x > 7)
			{
				v_video.V_DrawPatch(269, 159, (p_tick.leveltime & 4) == 0 ?
					PatchINVRTGEM1 : PatchINVRTGEM2);
			}
		}

#if DOS

void DrawFullScreenStuff(void)
{
	int i;
	int x;
	int temp;

	UpdateState |= I_FULLSCRN;
	if(CPlayer.mo.health > 0)
	{
		DrBNumber(CPlayer.mo.health, 5, 180);
	}
	else
	{
		DrBNumber(0, 5, 180);
	}
	if(deathmatch)
	{
		temp = 0;
		for(i=0; i<MAXPLAYERS; i++)
		{
			if(playeringame[i])
			{
				temp += CPlayer.frags[i];
			}
		}
		DrINumber(temp, 45, 185);
	}
	if(!inventory)
	{
		if(CPlayer.readyArtifact > 0)
		{
			V_DrawFuzzPatch(286, 170, W_CacheLumpName("ARTIBOX",
				PU_CACHE));
			V_DrawPatch(286, 170,
				W_CacheLumpName(patcharti[CPlayer.readyArtifact], PU_CACHE));
			DrSmallNumber(CPlayer.inventory[inv_ptr].count, 307, 192);
		}
	}
	else
	{
		x = inv_ptr-curpos;
		for(i = 0; i < 7; i++)
		{
			V_DrawFuzzPatch(50+i*31, 168, W_CacheLumpName("ARTIBOX",
				PU_CACHE));
			if(CPlayer.inventorySlotNum > x+i
				&& CPlayer.inventory[x+i].type != arti_none)
			{
				V_DrawPatch(50+i*31, 168, W_CacheLumpName(
					patcharti[CPlayer.inventory[x+i].type], PU_CACHE));
				DrSmallNumber(CPlayer.inventory[x+i].count, 69+i*31, 190);
			}
		}
		V_DrawPatch(50+curpos*31, 197, PatchSELECTBOX);
		if(x != 0)
		{
			V_DrawPatch(38, 167, !(leveltime&4) ? PatchINVLFGEM1 :
				PatchINVLFGEM2);
		}
		if(CPlayer.inventorySlotNum-x > 7)
		{
			V_DrawPatch(269, 167, !(leveltime&4) ?
				PatchINVRTGEM1 : PatchINVRTGEM2);
		}
	}
}

#endif

		//--------------------------------------------------------------------------
		//
		// FUNC SB_Responder
		//
		//--------------------------------------------------------------------------

		public static bool SB_Responder(DoomDef.event_t e)
		{
			if (e.type == DoomDef.evtype_t.ev_keydown)
			{
				if (HandleCheats((Keys)e.data1))
				{ // Need to eat the key
					return (true);
				}
			}
			return (false);
		}

		//--------------------------------------------------------------------------
		//
		// FUNC HandleCheats
		//
		// Returns true if the caller should eat the key.
		//
		//--------------------------------------------------------------------------

		static public bool HandleCheats(Keys key)
		{
			int i;
			bool eat;

			if (g_game.netgame || g_game.gameskill == DoomDef.skill_t.sk_nightmare)
			{ // Can't cheat in a net-game, or in nightmare mode
				return (false);
			}
			if (g_game.players[g_game.consoleplayer].health <= 0)
			{ // Dead players can't cheat
				return (false);
			}
			eat = false;
			foreach (Cheat_t cheat in Cheats)
			{
				if (CheatAddKey(cheat, key, ref eat))
				{
					cheat.func.func(g_game.players[g_game.consoleplayer], cheat);
					i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				}
			}
			return (eat);
		}

		//--------------------------------------------------------------------------
		//
		// FUNC CheatAddkey
		//
		// Returns true if the added key completed the cheat, false otherwise.
		//
		//--------------------------------------------------------------------------

		static List<int> cheatNum = new List<int>();

		static bool CheatAddKey(Cheat_t cheat, Keys key, ref bool eat)
		{
			if (cheat.pos == -1)
			{
				cheat.pos = 0;
				cheat.currentArg = 0;
			}
			/*	if(cheat.pos == 0)
				{
					eat = true;
					cheat.args[cheat.currentArg++] = key;
					cheat.pos++;
				}
				else */
			if (cheat.sequence[cheat.pos] == Keys.D3 &&
				key >= Keys.D0 && key <= Keys.D9)
			{
				cheatNum.Add((int)(key - Keys.D0));
				cheat.pos++;
			}
			else if (cheat.sequence[cheat.pos] == key)
			{
				cheat.pos++;
			}
			else
			{
				cheat.pos = 0;
				cheat.currentArg = 0;
			}
			if (cheat.pos == cheat.sequence.Count())
			{
				cheat.pos = 0;
				cheat.currentArg = 0;
				return (true);
			}
			return (false);
		}


		//--------------------------------------------------------------------------
		//
		// CHEAT FUNCTIONS
		//
		//--------------------------------------------------------------------------
		public class CheatGodFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				player.cheats ^= DoomDef.CF_GODMODE;
				if ((player.cheats & DoomDef.CF_GODMODE) != 0)
				{
					p_inter.P_SetMessage(player, dstring.TXT_CHEATGODON, false);
				}
				else
				{
					p_inter.P_SetMessage(player, dstring.TXT_CHEATGODOFF, false);
				}
				SB_state = -1;
			}
		}
		public class CheatNoClipFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				player.cheats ^= DoomDef.CF_NOCLIP;
				if ((player.cheats & DoomDef.CF_NOCLIP) != 0)
				{
					p_inter.P_SetMessage(player, dstring.TXT_CHEATNOCLIPON, false);
				}
				else
				{
					p_inter.P_SetMessage(player, dstring.TXT_CHEATNOCLIPOFF, false);
				}
			}
		}

		public class CheatWeaponsFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				int i;

				player.armorpoints = 200;
				player.armortype = 2;
				if (!player.backpack)
				{
					for (i = 0; i < (int)DoomDef.ammotype_t.NUMAMMO; i++)
					{
						player.maxammo[i] *= 2;
					}
					player.backpack = true;
				}
				for (i = 0; i < (int)DoomDef.weapontype_t.NUMWEAPONS - 1; i++)
				{
					player.weaponowned[i] = true;
				}
				if (d_main.shareware)
				{
					player.weaponowned[(int)DoomDef.weapontype_t.wp_skullrod] = false;
					player.weaponowned[(int)DoomDef.weapontype_t.wp_phoenixrod] = false;
					player.weaponowned[(int)DoomDef.weapontype_t.wp_mace] = false;
				}
				for (i = 0; i < (int)DoomDef.ammotype_t.NUMAMMO; i++)
				{
					player.ammo[i] = player.maxammo[i];
				}
				p_inter.P_SetMessage(player, dstring.TXT_CHEATWEAPONS, false);
			}
		}

		public class CheatPowerFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//if(player.powers[pw_weaponlevel2])
				//{
				//    player.powers[pw_weaponlevel2] = 0;
				//    P_SetMessage(player, TXT_CHEATPOWEROFF, false);
				//}
				//else
				//{
				//    P_UseArtifact(player, arti_tomeofpower);
				//    P_SetMessage(player, TXT_CHEATPOWERON, false);
				//}
			}
		}

		public class CheatHealthFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//if(player.chickenTics)
				//{
				//    player.health = player.mo.health = MAXCHICKENHEALTH;
				//}
				//else
				//{
				//    player.health = player.mo.health = MAXHEALTH;
				//}
				//P_SetMessage(player, TXT_CHEATHEALTH, false);
			}
		}

		public class CheatKeysFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				player.keys[(int)DoomDef.keytype_t.key_yellow] = true;
				player.keys[(int)DoomDef.keytype_t.key_green] = true;
				player.keys[(int)DoomDef.keytype_t.key_blue] = true;
				playerkeys = 7; // Key refresh flags
				p_inter.P_SetMessage(player, dstring.TXT_CHEATKEYS, false);
			}
		}

		public class CheatSoundFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//DebugSound = !DebugSound;
				//if(DebugSound)
				//{
				//    P_SetMessage(player, TXT_CHEATSOUNDON, false);
				//}
				//else
				//{
				//    P_SetMessage(player, TXT_CHEATSOUNDOFF, false);
				//}
			}
		}

		public class CheatTickerFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//extern int DisplayTicker;

				//DisplayTicker = !DisplayTicker;
				//if(DisplayTicker)
				//{
				//    P_SetMessage(player, TXT_CHEATTICKERON, false);
				//}
				//else
				//{
				//    P_SetMessage(player, TXT_CHEATTICKEROFF, false);
				//}
			}
		}

		public class CheatArtifact1Func : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				p_inter.P_SetMessage(player, dstring.TXT_CHEATARTIFACTS1, false);
			}
		}

		public class CheatArtifact2Func : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				p_inter.P_SetMessage(player, dstring.TXT_CHEATARTIFACTS2, false);
			}
		}

		public class CheatArtifact3Func : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//int i;
				//int j;
				//artitype_t type;
				//int count;

				//type = cheat.args[0]-'a'+1;
				//count = cheat.args[1]-'0';
				//if(type == 26 && count == 0)
				//{ // All artifacts
				//    for(i = arti_none+1; i < NUMARTIFACTS; i++)
				//    {
				//        if(shareware && (i == arti_superhealth
				//            || i == arti_teleport))
				//        {
				//            continue;
				//        }
				//        for(j = 0; j < 16; j++)
				//        {
				//            P_GiveArtifact(player, i, NULL);
				//        }
				//    }
				//    P_SetMessage(player, TXT_CHEATARTIFACTS3, false);
				//}
				//else if(type > arti_none && type < NUMARTIFACTS
				//    && count > 0 && count < 10)
				//{
				//    if(shareware && (type == arti_superhealth || type == arti_teleport))
				//    {
				//        P_SetMessage(player, TXT_CHEATARTIFACTSFAIL, false);
				//        return;
				//    }
				//    for(i = 0; i < count; i++)
				//    {
				//        P_GiveArtifact(player, type, NULL);
				//    }
				//    P_SetMessage(player, TXT_CHEATARTIFACTS3, false);
				//}
				//else
				//{ // Bad input
				//    P_SetMessage(player, TXT_CHEATARTIFACTSFAIL, false);
				//}
			}
		}

		public class CheatWarpFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//int episode;
				//int map;

				//episode = cheat.args[0]-'0';
				//map = cheat.args[1]-'0';
				//if(M_ValidEpisodeMap(episode, map))
				//{
				//    G_DeferedInitNew(gameskill, episode, map);
				//    P_SetMessage(player, TXT_CHEATWARP, false);
				//}
			}
		}

		public class CheatChickenFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//extern boolean P_UndoPlayerChicken(player_t *player);

				//if(player.chickenTics)
				//{
				//    if(P_UndoPlayerChicken(player))
				//    {
				//        P_SetMessage(player, TXT_CHEATCHICKENOFF, false);
				//    }
				//}
				//else if(P_ChickenMorphPlayer(player))
				//{
				//    P_SetMessage(player, TXT_CHEATCHICKENON, false);
				//}
			}
		}

		public class CheatMassacreFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				p_enemy.P_Massacre();
				p_inter.P_SetMessage(player, dstring.TXT_CHEATMASSACRE, false);
			}
		}

		public class CheatIDKFAFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				//int i;
				//if(player.chickenTics)
				//{
				//    return;
				//}
				//for(i = 1; i < 8; i++)
				//{
				//    player.weaponowned[i] = false;
				//}
				//player.pendingweapon = wp_staff;
				//P_SetMessage(player, TXT_CHEATIDKFA, true);
			}
		}

		public class CheatIDDQDFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				p_inter.P_DamageMobj(player.mo, null, player.mo, 10000);
				p_inter.P_SetMessage(player, dstring.TXT_CHEATIDDQD, true);
			}
		}

		public class CheatEMFunc : CheatDelegate
		{
			public override void func(DoomDef.player_t player, Cheat_t cheat)
			{
				g_game.G_InitNew(g_game.d_skill, cheatNum[0], cheatNum[1]);
				cheatNum.Clear();
			}
		}
	}
}
