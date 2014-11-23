using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;


/////////////////////////////////////////////
// Port - Done
/////////////////////////////////////////////

namespace HereticXNA
{
	public static class in_lude
	{
		/*
========================
=
= IN_lude.c
=
========================
*/
		public enum gametype_t
		{
			SINGLE,
			COOPERATIVE,
			DEATHMATCH
		} ;

		public static bool intermission;

		static bool skipintermission;
		static int interstate = 0;
		static int intertime = -1;
		static int oldintertime = 0;
		static gametype_t gametype;

		static int cnt;

		static int time;
		static int hours;
		static int minutes;
		static int seconds;

		static int slaughterboy; // in DM, the player with the most kills

		static int[] killPercent = new int[DoomDef.MAXPLAYERS];
		static int[] bonusPercent = new int[DoomDef.MAXPLAYERS];
		static int[] secretPercent = new int[DoomDef.MAXPLAYERS];

		static w_wad.CacheInfo patchINTERPIC;
		static w_wad.CacheInfo patchBEENTHERE;
		static w_wad.CacheInfo patchGOINGTHERE;
		static w_wad.CacheInfo[] FontBNumbers = new w_wad.CacheInfo[10];
		static w_wad.CacheInfo FontBNegative;
		static w_wad.CacheInfo FontBSlash;
		static w_wad.CacheInfo FontBPercent;

		static int FontBLump;
		static int FontBLumpBase;
		static int patchFaceOkayBase;
		static int patchFaceDeadBase;

		static int[] totalFrags = new int[DoomDef.MAXPLAYERS];
		static int[] dSlideX = new int[DoomDef.MAXPLAYERS];
		static int[] dSlideY = new int[DoomDef.MAXPLAYERS];

		static string KillersText = "KILLERS";

		public class yahpt_t
		{
			public int x;
			public int y;
		} ;

		static yahpt_t[,] YAHspot = new yahpt_t[3, 9]
{
	{
		new yahpt_t{ x=172, y=78 },
		new yahpt_t{ x=86, y=90 },
		new yahpt_t{ x=73, y=66 },
		new yahpt_t{ x=159, y=95 },
		new yahpt_t{x= 148,y= 126 },
		new yahpt_t{ x=132, y=54 },
		new yahpt_t{ x=131, y=74 },
		new yahpt_t{ x=208, y=138 },
		new yahpt_t{ x=52,y= 101 }
	},
	{
		new yahpt_t{ x=218, y=57 },
		new yahpt_t{ x=137, y=81 },
		new yahpt_t{ x=155, y=124 },
		new yahpt_t{ x=171, y=68 },
		new yahpt_t{ x=250,y= 86 },
		new yahpt_t{ x=136, y=98 },
		new yahpt_t{x= 203, y=90 },
		new yahpt_t{ x=220, y=140 },
		new yahpt_t{x= 279, y=106 }
	},
	{
		new yahpt_t{ x=86, y=99 },
		new yahpt_t{ x=124, y=103 },
		new yahpt_t{ x=154, y=79 },
		new yahpt_t{ x=202,y= 83 },
		new yahpt_t{ x=178, y=59 },
		new yahpt_t{ x=142, y=58 },
		new yahpt_t{ x=219, y=66 },
		new yahpt_t{ x=247, y=57 },
		new yahpt_t{ x=107, y=80 }
	}
};


		//========================================================================
		//
		// IN_Start
		//
		//========================================================================

		public static void IN_Start()
		{
			//	I_SetPalette(w_wad.W_CacheLumpName("PLAYPAL", PU_CACHE));
			IN_LoadPics();
			IN_InitStats();
			intermission = true;
			interstate = -1;
			skipintermission = false;
			intertime = 0;
			oldintertime = 0;
			am_map.AM_Stop();
			i_ibm.S_StartSong((int)sounds.musicenum_t.mus_intr, true);
		}

		//========================================================================
		//
		// IN_WaitStop
		//
		//========================================================================

		public static void IN_WaitStop()
		{
			if ((--cnt) == 0)
			{
				IN_Stop();
				g_game.G_WorldDone();
			}
		}

		//========================================================================
		//
		// IN_Stop
		//
		//========================================================================

		public static void IN_Stop()
		{
			intermission = false;
			IN_UnloadPics();
			sb_bar.SB_state = -1;
			//BorderNeedRefresh = true;
		}

		//========================================================================
		//
		// IN_InitStats
		//
		//      Initializes the stats for single player mode
		//========================================================================

		public static void IN_InitStats()
		{
			int i;
			int j;
			int slaughterfrags;
			int posnum;
			int slaughtercount;
			int playercount;


			if (!g_game.netgame)
			{
				gametype = gametype_t.SINGLE;
				time = p_tick.leveltime / 35;
				hours = time / 3600;
				time -= hours * 3600;
				minutes = time / 60;
				time -= minutes * 60;
				seconds = time;
			}
			else if (g_game.netgame && !g_game.deathmatch)
			{
				gametype = gametype_t.COOPERATIVE;
				for (i = 0; i < DoomDef.MAXPLAYERS; ++i)
				{
					killPercent[i] = 0;
					bonusPercent[i] = 0;
					secretPercent[i] = 0;
				}
				for (i = 0; i < DoomDef.MAXPLAYERS; i++)
				{
					if (g_game.playeringame[i])
					{
						if (g_game.totalkills != 0)
						{
							killPercent[i] = g_game.players[i].killcount * 100 / g_game.totalkills;
						}
						if (g_game.totalitems != 0)
						{
							bonusPercent[i] = g_game.players[i].itemcount * 100 / g_game.totalitems;
						}
						if (g_game.totalsecret != 0)
						{
							secretPercent[i] = g_game.players[i].secretcount * 100 / g_game.totalsecret;
						}
					}
				}
			}
			else
			{
				gametype = gametype_t.DEATHMATCH;
				slaughterboy = 0;
				slaughterfrags = -9999;
				posnum = 0;
				playercount = 0;
				slaughtercount = 0;
				for (i = 0; i < DoomDef.MAXPLAYERS; i++)
				{
					totalFrags[i] = 0;
					if (g_game.playeringame[i])
					{
						playercount++;
						for (j = 0; j < DoomDef.MAXPLAYERS; j++)
						{
							if (g_game.playeringame[j])
							{
								totalFrags[i] += g_game.players[i].frags[j];
							}
						}
						dSlideX[i] = (43 * posnum * DoomDef.FRACUNIT) / 20;
						dSlideY[i] = (36 * posnum * DoomDef.FRACUNIT) / 20;
						posnum++;
					}
					if (totalFrags[i] > slaughterfrags)
					{
						slaughterboy = 1 << i;
						slaughterfrags = totalFrags[i];
						slaughtercount = 1;
					}
					else if (totalFrags[i] == slaughterfrags)
					{
						slaughterboy |= 1 << i;
						slaughtercount++;
					}
				}
				if (playercount == slaughtercount)
				{ // don't do the slaughter stuff if everyone is equal
					slaughterboy = 0;
				}
			}
		}

		//========================================================================
		//
		// IN_LoadPics
		//
		//========================================================================

		public static void IN_LoadPics()
		{
			int i;

			switch (g_game.gameepisode)
			{
				case 1:
					patchINTERPIC = w_wad.W_CacheLumpName("MAPE1", DoomDef.PU_STATIC);
					break;
				case 2:
					patchINTERPIC = w_wad.W_CacheLumpName("MAPE2", DoomDef.PU_STATIC);
					break;
				case 3:
					patchINTERPIC = w_wad.W_CacheLumpName("MAPE3", DoomDef.PU_STATIC);
					break;
				default:
					break;
			}
			patchBEENTHERE = w_wad.W_CacheLumpName("IN_X", DoomDef.PU_STATIC);
			patchGOINGTHERE = w_wad.W_CacheLumpName("IN_YAH", DoomDef.PU_STATIC);
			FontBLumpBase = w_wad.W_GetNumForName("FONTB16");
			for (i = 0; i < 10; i++)
			{
				FontBNumbers[i] = w_wad.W_CacheLumpNum(FontBLumpBase + i, DoomDef.PU_STATIC);
			}
			FontBLump = w_wad.W_GetNumForName("FONTB_S") + 1;
			FontBNegative = w_wad.W_CacheLumpName("FONTB13", DoomDef.PU_STATIC);

			FontBSlash = w_wad.W_CacheLumpName("FONTB15", DoomDef.PU_STATIC);
			FontBPercent = w_wad.W_CacheLumpName("FONTB05", DoomDef.PU_STATIC);
			patchFaceOkayBase = w_wad.W_GetNumForName("FACEA0");
			patchFaceDeadBase = w_wad.W_GetNumForName("FACEB0");
		}

		//========================================================================
		//
		// IN_UnloadPics
		//
		//========================================================================

		public static void IN_UnloadPics()
		{
			//int i;

			//if(patchINTERPIC)
			//{
			//    Z_ChangeTag(patchINTERPIC, PU_CACHE);
			//}
			//Z_ChangeTag(patchBEENTHERE, PU_CACHE);
			//Z_ChangeTag(patchGOINGTHERE, PU_CACHE);
			//for(i=0; i<10; i++)
			//{
			//    Z_ChangeTag(FontBNumbers[i], PU_CACHE);
			//}
			//Z_ChangeTag(FontBNegative, PU_CACHE);
			//Z_ChangeTag(FontBSlash, PU_CACHE);
			//Z_ChangeTag(FontBPercent, PU_CACHE);
		}

		//========================================================================
		//
		// IN_Ticker
		//
		//========================================================================

		public static void IN_Ticker()
		{
			if (!intermission)
			{
				return;
			}
			if (interstate == 3)
			{
				IN_WaitStop();
				return;
			}
			IN_CheckForSkip();
			intertime++;
			if (oldintertime < intertime)
			{
				interstate++;
				if (g_game.gameepisode > 3 && interstate >= 1)
				{ // Extended Wad levels:  skip directly to the next level
					interstate = 3;
				}
				switch (interstate)
				{
					case 0:
						oldintertime = intertime + 300;
						if (g_game.gameepisode > 3)
						{
							oldintertime = intertime + 1200;
						}
						break;
					case 1:
						oldintertime = intertime + 200;
						break;
					case 2:
						oldintertime = DoomDef.MAXINT;
						break;
					case 3:
						cnt = 10;
						break;
					default:
						break;
				}
			}
			if (skipintermission)
			{
				if (interstate == 0 && intertime < 150)
				{
					intertime = 150;
					skipintermission = false;
					return;
				}
				else if (interstate < 2 && g_game.gameepisode < 4)
				{
					interstate = 2;
					skipintermission = false;
					i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
					return;
				}
				interstate = 3;
				cnt = 10;
				skipintermission = false;
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
			}
		}

		//========================================================================
		//
		// IN_CheckForSkip
		//
		//      Check to see if any player hit a key
		//========================================================================

		public static void IN_CheckForSkip()
		{
			int i;
			DoomDef.player_t player;

			for (i = 0; i < DoomDef.MAXPLAYERS; i++)
			{
				player = g_game.players[i];
				if (g_game.playeringame[i])
				{
					if ((player.cmd.buttons & DoomDef.BT_ATTACK) != 0)
					{
						if (player.attackdown == 0)
						{
							skipintermission = true;
						}
						player.attackdown = 1;
					}
					else
					{
						player.attackdown = 0;
					}
					if ((player.cmd.buttons & DoomDef.BT_USE) != 0)
					{
						if (player.usedown == 0)
						{
							skipintermission = true;
						}
						player.usedown = 1;
					}
					else
					{
						player.usedown = 0;
					}
				}
			}
		}

		//========================================================================
		//
		// IN_Drawer
		//
		//========================================================================

		static int oldinterstate;

		public static void IN_Drawer()
		{
			if (!intermission)
			{
				return;
			}
			if (interstate == 3)
			{
				return;
			}
			i_ibm.UpdateState |= DoomDef.I_FULLSCRN;
			if (oldinterstate != 2 && interstate == 2)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_pstop);
			}
			oldinterstate = interstate;
			switch (interstate)
			{
				case 0: // draw stats
					IN_DrawStatBack();
					switch (gametype)
					{
						case gametype_t.SINGLE:
							IN_DrawSingleStats();
							break;
						case gametype_t.COOPERATIVE:
							IN_DrawCoopStats();
							break;
						case gametype_t.DEATHMATCH:
							IN_DrawDMStats();
							break;
					}
					break;
				case 1: // leaving old level
					if (g_game.gameepisode < 4)
					{
						v_video.V_DrawPatch(0, 0, patchINTERPIC);
						IN_DrawOldLevel();
					}
					break;
				case 2: // going to the next level
					if (g_game.gameepisode < 4)
					{
						v_video.V_DrawPatch(0, 0, patchINTERPIC);
						IN_DrawYAH();
					}
					break;
				case 3: // waiting before going to the next level
					if (g_game.gameepisode < 4)
					{
						v_video.V_DrawPatch(0, 0, patchINTERPIC);
					}
					break;
				default:
					i_ibm.I_Error("IN_lude:  Intermission state out of range.\n");
					break;
			}
		}

		//========================================================================
		//
		// IN_DrawStatBack
		//
		//========================================================================

		public static void IN_DrawStatBack()
		{
			int x;
			int y;

			w_wad.CacheInfo src;

			src = w_wad.W_CacheLumpName("FLOOR16", DoomDef.PU_CACHE);

			for (y = 0; y < DoomDef.SCREENHEIGHT; y += 64)
			{
				for (x = 0; x < DoomDef.SCREENWIDTH; x += 64)
				{
					v_video.V_DrawPatch(x, y, src);
				}
			}
		}


		//========================================================================
		//
		// IN_DrawOldLevel
		//
		//========================================================================

		public static void IN_DrawOldLevel()
		{
			int i;
			int x;

			x = 160 - mn_menu.MN_TextBWidth(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.prevmap - 1] + 7) / 2;
			IN_DrTextB(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.prevmap - 1] + 7, x, 3);
			x = 160 - mn_menu.MN_TextAWidth("FINISHED") / 2;
			mn_menu.MN_DrTextA("FINISHED", x, 25);

			if (g_game.prevmap == 9)
			{
				for (i = 0; i < g_game.gamemap - 1; i++)
				{
					v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, i].x, YAHspot[g_game.gameepisode - 1, i].y,
						patchBEENTHERE);
				}
				if ((intertime & 16) == 0)
				{
					v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, 8].x, YAHspot[g_game.gameepisode - 1, 8].y,
						patchBEENTHERE);
				}
			}
			else
			{
				for (i = 0; i < g_game.prevmap - 1; i++)
				{
					v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, i].x, YAHspot[g_game.gameepisode - 1, i].y,
						patchBEENTHERE);
				}
				if (g_game.players[g_game.consoleplayer].didsecret)
				{
					v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, 8].x, YAHspot[g_game.gameepisode - 1, 8].y,
						patchBEENTHERE);
				}
				if ((intertime & 16) == 0)
				{
					v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, g_game.prevmap - 1].x, YAHspot[g_game.gameepisode - 1, g_game.prevmap - 1].y,
						patchBEENTHERE);
				}
			}
		}

		//========================================================================
		//
		// IN_DrawYAH
		//
		//========================================================================

		public static void IN_DrawYAH()
		{
			int i;
			int x;

			x = 160 - mn_menu.MN_TextAWidth("NOW ENTERING:") / 2;
			mn_menu.MN_DrTextA("NOW ENTERING:", x, 10);
			x = 160 - mn_menu.MN_TextBWidth(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.gamemap - 1] + 7) / 2;
			IN_DrTextB(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.gamemap - 1] + 7, x, 20);

			if (g_game.prevmap == 9)
			{
				g_game.prevmap = g_game.gamemap - 1;
			}
			for (i = 0; i < g_game.prevmap; i++)
			{
				v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, i].x, YAHspot[g_game.gameepisode - 1, i].y,
					patchBEENTHERE);
			}
			if (g_game.players[g_game.consoleplayer].didsecret)
			{
				v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, 8].x, YAHspot[g_game.gameepisode - 1, 8].y,
					patchBEENTHERE);
			}
			if ((intertime & 16) == 0 || interstate == 3)
			{ // draw the destination 'X'
				v_video.V_DrawPatch(YAHspot[g_game.gameepisode - 1, g_game.gamemap - 1].x,
					YAHspot[g_game.gameepisode - 1, g_game.gamemap - 1].y, patchGOINGTHERE);
			}
		}

		//========================================================================
		//
		// IN_DrawSingleStats
		//
		//========================================================================

		static int soundsi;

		public static void IN_DrawSingleStats()
		{
			int x;
			IN_DrTextB("KILLS", 50, 65);
			IN_DrTextB("ITEMS", 50, 90);
			IN_DrTextB("SECRETS", 50, 115);

			x = 160 - mn_menu.MN_TextBWidth(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.prevmap - 1] + 7) / 2;
			IN_DrTextB(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.prevmap - 1] + 7, x, 3);
			x = 160 - mn_menu.MN_TextAWidth("FINISHED") / 2;
			mn_menu.MN_DrTextA("FINISHED", x, 25);

			if (intertime < 30)
			{
				soundsi = 0;
				return;
			}
			if (soundsi < 1 && intertime >= 30)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				soundsi++;
			}
			IN_DrawNumber(g_game.players[g_game.consoleplayer].killcount, 200, 65, 3);
			v_video.V_DrawShadowedPatch(237, 65, FontBSlash);
			IN_DrawNumber(g_game.totalkills, 248, 65, 3);
			if (intertime < 60)
			{
				return;
			}
			if (soundsi < 2 && intertime >= 60)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				soundsi++;
			}
			IN_DrawNumber(g_game.players[g_game.consoleplayer].itemcount, 200, 90, 3);
			v_video.V_DrawShadowedPatch(237, 90, FontBSlash);
			IN_DrawNumber(g_game.totalitems, 248, 90, 3);
			if (intertime < 90)
			{
				return;
			}
			if (soundsi < 3 && intertime >= 90)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				soundsi++;
			}
			IN_DrawNumber(g_game.players[g_game.consoleplayer].secretcount, 200, 115, 3);
			v_video.V_DrawShadowedPatch(237, 115, FontBSlash);
			IN_DrawNumber(g_game.totalsecret, 248, 115, 3);
			if (intertime < 150)
			{
				return;
			}
			if (soundsi < 4 && intertime >= 150)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				soundsi++;
			}

			if (!d_main.ExtendedWAD || g_game.gameepisode < 4)
			{
				IN_DrTextB("TIME", 85, 160);
				IN_DrawTime(155, 160, hours, minutes, seconds);
			}
			else
			{
				x = 160 - mn_menu.MN_TextAWidth("NOW ENTERING:") / 2;
				mn_menu.MN_DrTextA("NOW ENTERING:", x, 160);
				x = 160 - mn_menu.MN_TextBWidth(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.gamemap - 1] + 7) / 2;
				IN_DrTextB(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.gamemap - 1] + 7, x, 170);
				skipintermission = false;
			}
		}

		//========================================================================
		//
		// IN_DrawCoopStats
		//
		//========================================================================

		public static void IN_DrawCoopStats()
		{
			int i;
			int x;
			int ypos;

			IN_DrTextB("KILLS", 95, 35);
			IN_DrTextB("BONUS", 155, 35);
			IN_DrTextB("SECRET", 232, 35);
			x = 160 - mn_menu.MN_TextBWidth(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.prevmap - 1] + 7) / 2;
			IN_DrTextB(am_map.LevelNames[(g_game.gameepisode - 1) * 9 + g_game.prevmap - 1] + 7, x, 3);
			x = 160 - mn_menu.MN_TextAWidth("FINISHED") / 2;
			mn_menu.MN_DrTextA("FINISHED", x, 25);

			ypos = 50;
			for (i = 0; i < DoomDef.MAXPLAYERS; i++)
			{
				if (g_game.playeringame[i])
				{
					v_video.V_DrawShadowedPatch(25, ypos, w_wad.W_CacheLumpNum(patchFaceOkayBase + i, DoomDef.PU_CACHE));
					if (intertime < 40)
					{
						soundsi = 0;
						ypos += 37;
						continue;
					}
					else if (intertime >= 40 && soundsi < 1)
					{
						i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
						soundsi++;
					}
					IN_DrawNumber(killPercent[i], 85, ypos + 10, 3);
					v_video.V_DrawShadowedPatch(121, ypos + 10, FontBPercent);
					IN_DrawNumber(bonusPercent[i], 160, ypos + 10, 3);
					v_video.V_DrawShadowedPatch(196, ypos + 10, FontBPercent);
					IN_DrawNumber(secretPercent[i], 237, ypos + 10, 3);
					v_video.V_DrawShadowedPatch(273, ypos + 10, FontBPercent);
					ypos += 37;
				}
			}
		}

		//========================================================================
		//
		// IN_DrawDMStats
		//
		//========================================================================

		public static void IN_DrawDMStats()
		{
			int i;
			int j;
			int ypos;
			int xpos;
			int kpos;
			int x;

			xpos = 90;
			ypos = 55;

			IN_DrTextB("TOTAL", 265, 30);
			mn_menu.MN_DrTextA("VICTIMS", 140, 8);
			for (i = 0; i < 7; i++)
			{
				mn_menu.MN_DrTextA(KillersText.Substring(i, 1), 10, 80 + 9 * i);
			}
			if (intertime < 20)
			{
				for (i = 0; i < DoomDef.MAXPLAYERS; i++)
				{
					if (g_game.playeringame[i])
					{
						v_video.V_DrawShadowedPatch(40, ((ypos << DoomDef.FRACBITS) + dSlideY[i] * intertime)
							>> DoomDef.FRACBITS, w_wad.W_CacheLumpNum(patchFaceOkayBase + i, DoomDef.PU_CACHE));
						v_video.V_DrawShadowedPatch(((xpos << DoomDef.FRACBITS) + dSlideX[i] * intertime)
							>> DoomDef.FRACBITS, 18, w_wad.W_CacheLumpNum(patchFaceDeadBase + i, DoomDef.PU_CACHE));
					}
				}
				soundsi = 0;
				return;
			}
			if (intertime >= 20 && soundsi < 1)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				soundsi++;
			}
			if (intertime >= 100 && slaughterboy != 0 && soundsi < 2)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_wpnup);
				soundsi++;
			}
			for (i = 0; i < DoomDef.MAXPLAYERS; i++)
			{
				if (g_game.playeringame[i])
				{
					if (intertime < 100 || i == g_game.consoleplayer)
					{
						v_video.V_DrawShadowedPatch(40, ypos, w_wad.W_CacheLumpNum(patchFaceOkayBase + i, DoomDef.PU_CACHE));
						v_video.V_DrawShadowedPatch(xpos, 18, w_wad.W_CacheLumpNum(patchFaceDeadBase + i, DoomDef.PU_CACHE));
					}
					else
					{
						//	v_video.V_DrawFuzzPatch(40, ypos, w_wad.W_CacheLumpNum(patchFaceOkayBase + i, DoomDef.PU_CACHE));
						//	v_video.V_DrawFuzzPatch(xpos, 18, w_wad.W_CacheLumpNum(patchFaceDeadBase + i, DoomDef.PU_CACHE));
						v_video.V_DrawPatch(40, ypos, w_wad.W_CacheLumpNum(patchFaceOkayBase + i, DoomDef.PU_CACHE));
						v_video.V_DrawPatch(xpos, 18, w_wad.W_CacheLumpNum(patchFaceDeadBase + i, DoomDef.PU_CACHE));
					}
					kpos = 86;
					for (j = 0; j < DoomDef.MAXPLAYERS; j++)
					{
						if (g_game.playeringame[j])
						{
							IN_DrawNumber(g_game.players[i].frags[j], kpos, ypos + 10, 3);
							kpos += 43;
						}
					}
					if ((slaughterboy & (1 << i)) != 0)
					{
						if ((intertime & 16) == 0)
						{
							IN_DrawNumber(totalFrags[i], 263, ypos + 10, 3);
						}
					}
					else
					{
						IN_DrawNumber(totalFrags[i], 263, ypos + 10, 3);
					}
					ypos += 36;
					xpos += 43;
				}
			}
		}

		//========================================================================
		//
		// IN_DrawTime
		//
		//========================================================================

		public static void IN_DrawTime(int x, int y, int h, int m, int s)
		{
			if (h != 0)
			{
				IN_DrawNumber(h, x, y, 2);
				IN_DrTextB(":", x + 26, y);
			}
			x += 34;
			if (m != 0 || h != 0)
			{
				IN_DrawNumber(m, x, y, 2);
			}
			x += 34;
			if (s != 0)
			{
				IN_DrTextB(":", x - 8, y);
				IN_DrawNumber(s, x, y, 2);
			}
		}

		//========================================================================
		//
		// IN_DrawNumber
		//
		//========================================================================

		public static void IN_DrawNumber(int val, int x, int y, int digits)
		{
			w_wad.CacheInfo patch;
			int xpos;
			int oldval;
			int realdigits;
			bool neg;

			oldval = val;
			xpos = x;
			neg = false;
			realdigits = 1;

			if (val < 0)
			{ //...this should reflect negative frags
				val = -val;
				neg = true;
				if (val > 99)
				{
					val = 99;
				}
			}
			if (val > 9)
			{
				realdigits++;
				if (digits < realdigits)
				{
					realdigits = digits;
					val = 9;
				}
			}
			if (val > 99)
			{
				realdigits++;
				if (digits < realdigits)
				{
					realdigits = digits;
					val = 99;
				}
			}
			if (val > 999)
			{
				realdigits++;
				if (digits < realdigits)
				{
					realdigits = digits;
					val = 999;
				}
			}
			if (digits == 4)
			{
				patch = FontBNumbers[val / 1000];
				v_video.BuildPatchTextureIfNot(patch);
				v_video.V_DrawShadowedPatch(xpos + 6 - (patch.cache as v_video.PatchTexture).patch.width / 2 - 12, y, patch);
			}
			if (digits > 2)
			{
				if (realdigits > 2)
				{
					patch = FontBNumbers[val / 100];
					v_video.BuildPatchTextureIfNot(patch);
					v_video.V_DrawShadowedPatch(xpos + 6 - (patch.cache as v_video.PatchTexture).patch.width / 2, y, patch);
				}
				xpos += 12;
			}
			val = val % 100;
			if (digits > 1)
			{
				if (val > 9)
				{
					patch = FontBNumbers[val / 10];
					v_video.BuildPatchTextureIfNot(patch);
					v_video.V_DrawShadowedPatch(xpos + 6 - (patch.cache as v_video.PatchTexture).patch.width / 2, y, patch);
				}
				else if (digits == 2 || oldval > 99)
				{
					v_video.V_DrawShadowedPatch(xpos, y, FontBNumbers[0]);
				}
				xpos += 12;
			}
			val = val % 10;
			patch = FontBNumbers[val];
			v_video.BuildPatchTextureIfNot(patch);
			v_video.V_DrawShadowedPatch(xpos + 6 - (patch.cache as v_video.PatchTexture).patch.width / 2, y, patch);
			if (neg)
			{
				patch = FontBNegative;
				v_video.BuildPatchTextureIfNot(patch);
				v_video.V_DrawShadowedPatch(xpos + 6 - (patch.cache as v_video.PatchTexture).patch.width / 2 - 12 * (realdigits), y, patch);
			}
		}


		//========================================================================
		//
		// IN_DrTextB
		//
		//========================================================================

		public static void IN_DrTextB(string text, int x, int y)
		{
			int c;
			w_wad.CacheInfo p;

			foreach (char cr in text)
			{
				c = (int)cr;
				if (c < 33)
				{
					x += 8;
				}
				else
				{
					p = w_wad.W_CacheLumpNum(FontBLump + c - 33, DoomDef.PU_CACHE);
					v_video.V_DrawShadowedPatch(x, y, p);
					x += (p.cache as v_video.PatchTexture).patch.width - 1;
				}
			}
		}
	}
}
