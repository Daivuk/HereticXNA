using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

// G_game.c

//#include <stdio.h>
//#include <string.h>
//#include "DoomDef.h"
//#include "P_local.h"
//#include "soundst.h"

namespace HereticXNA
{
	public static class g_game
	{
		// Macros

		public const uint SVG_RAM = 0;
		public const uint SVG_FILE = 1;
		public const uint SAVE_GAME_TERMINATOR = 0x1d;
		public const uint AM_STARTKEY = 9;

		// Functions

public class sMonsterMissileInfo
{
	public info.mobjtype_t type;
	public int[] speed = new int[2];
}
public static sMonsterMissileInfo[] MonsterMissileInfo = new sMonsterMissileInfo[]
{
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_IMPBALL, speed = new int[]{10, 20} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_MUMMYFX1, speed = new int[]{9, 18} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_KNIGHTAXE, speed = new int[]{9, 18} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_REDAXE, speed = new int[]{9, 18} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_BEASTBALL, speed = new int[]{12, 20} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_WIZFX1, speed = new int[]{18, 24} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_SNAKEPRO_A, speed = new int[]{14, 20} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_SNAKEPRO_B, speed = new int[]{14, 20} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_HEADFX1, speed = new int[]{13, 20} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_HEADFX3, speed = new int[]{10, 18} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_MNTRFX1, speed = new int[]{20, 26} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_MNTRFX2, speed = new int[]{14, 20} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_SRCRFX1, speed = new int[]{20, 28} },
	new sMonsterMissileInfo { type = info.mobjtype_t.MT_SOR2FX1, speed = new int[]{20, 28} }
};
#if DOS

FILE *SaveGameFP;
#endif
public static int SaveGameType;

public static DoomDef.gameaction_t    gameaction;
		static public DoomDef.gamestate_t gamestate;
		static public DoomDef.skill_t gameskill;
		static public bool respawnmonsters;
static public int             gameepisode;
public static int             gamemap;
public static int                              prevmap;

public static bool         paused;
public static bool         sendpause;              // send a pause event next tic
public static bool         sendsave;               // send a save event next tic
public static bool         usergame;               // ok to save / end game

public static bool         timingdemo;             // if true, exit with report on completion
public static int             starttime;              // for comparative timing purposes

public static bool         viewactive;

public static bool         deathmatch;             // only if started as net death
public static bool         netgame;                // only true if packets are broadcast
public static bool []        playeringame = new bool[DoomDef.MAXPLAYERS];
public static DoomDef.player_t[] players = new DoomDef.player_t[DoomDef.MAXPLAYERS] { new DoomDef.player_t(), new DoomDef.player_t(), new DoomDef.player_t(), new DoomDef.player_t() };

public static int consoleplayer;          // player taking events and displaying
public static int displayplayer;          // view being displayed
public static int gametic;
public static int levelstarttic;          // gametic at level start
public static int totalkills, totalitems, totalsecret;    // for intermission

#if DOX
char            demoname[32];
byte            *demobuffer, *demo_p;
#endif
public static bool         demorecording;
public static bool         demoplayback;
public static bool         singledemo;             // quit after playing a demo from cmdline

public static bool         precache = true;        // if true, load all graphics at start

public static short [,]           consistancy = new short[DoomDef.MAXPLAYERS, DoomDef.BACKUPTICS];
#if DOX

byte            *savebuffer, *save_p;

#endif

//
// controls (have defaults)
//
static public int key_right, key_left, key_up, key_down;
static public int key_strafeleft, key_straferight;
static public int key_fire, key_use, key_strafe, key_speed;
static public int key_flyup, key_flydown, key_flycenter;
static public int key_lookup, key_lookdown, key_lookcenter;
static public int key_invleft, key_invright, key_useartifact;
static public m_misc.var_int mousebfire = new m_misc.var_int();
static public m_misc.var_int mousebstrafe = new m_misc.var_int();
static public m_misc.var_int mousebforward = new m_misc.var_int();

static public m_misc.var_int joybfire = new m_misc.var_int();
static public m_misc.var_int joybstrafe = new m_misc.var_int();
static public m_misc.var_int joybuse = new m_misc.var_int();
static public m_misc.var_int joybspeed = new m_misc.var_int();



public const int MAXPLMOVE = 0x32;

public static int[] forwardmove = new int[2] { 0x19, 0x32 };
public static int[] sidemove = new int[2] { 0x18, 0x28 };
public static int[] angleturn = new int[3] { 640, 1280, 320 };     // + slow turn
public const int SLOWTURNTICS =   6;

//TODO: public const int NUMKEYS 256
//static public bool   gamekeydown[NUMKEYS];
static public int turnheld;                   // for accelerative turning
static public int lookheld;
#if DOS


boolean         mousearray[4];
boolean         *mousebuttons = &mousearray[1];
	// allow [-1]
int             mousex, mousey;             // mouse values are used once
#endif
static public int dclicktime, dclickstate, dclicks;
static public int dclicktime2, dclickstate2, dclicks2;
#if DOS

int             joyxmove, joyymove;         // joystick values are repeated
boolean         joyarray[5];
boolean         *joybuttons = &joyarray[1];     // allow [-1]
#endif

static public int savegameslot;
#if DOS
char    savedescription[32];

int inventoryTics;

#endif
/*
====================
=
= G_BuildTiccmd
=
= Builds a ticcmd from all of the available inputs or reads it from the
= demo buffer.
= If recording a demo, write it out
====================
*/


public static bool usearti = true;

public static void G_BuildTiccmd (DoomDef.ticcmd_t cmd)
{
	// [dsl] We'll redo all movement here based on modern requirements

	if (Game1.instance.useFreeCam) return;

	int             i;
	bool         strafe, bstrafe;
	int             speed, tspeed, lspeed;
	int             forward, side;
	int look, arti;
	int flyheight;

	cmd.forwardmove = 0;
	cmd.sidemove = 0;
	cmd.angleturn = 0;
	cmd.angleupdown = 0;
	cmd.consistancy = 0;
	cmd.chatchar = 0;
	cmd.buttons = 0;
	cmd.lookfly = 0;
	cmd.arti = 0;

	cmd.consistancy =
		consistancy[consoleplayer, d_net.maketic%DoomDef.BACKUPTICS];
	// [dsl] Ignore that shit
	//if (isCyberPresent)
	//	I_ReadCyberCmd (cmd);


	strafe = false;// i_ibm.keyboardState.IsKeyDown((Keys)key_strafe);
	// Ignore joystick and mouse for now
	//||  mousebuttons[mousebstrafe]
	//	|| joybuttons[joybstrafe];
	speed = i_ibm.keyboardState.IsKeyDown(Keys.LeftShift) ? 1 : 0;// i_ibm.keyboardState.IsKeyDown((Keys)key_speed) ? 1 : 0;
	// Ignore joystick and mouse for now
	// || joybuttons[joybspeed]
	//	|| joybuttons[joybspeed];

	forward = side = look = arti = flyheight = 0;

//
// use two stage accelerative turning on the keyboard and joystick
//
	//if (joyxmove < 0 || joyxmove > 0
	//|| gamekeydown[key_right] || gamekeydown[key_left])
	if (i_ibm.keyboardState.IsKeyDown(Keys.Right) ||
		i_ibm.keyboardState.IsKeyDown(Keys.Left))
		turnheld += d_net.ticdup;
	else
		turnheld = 0;
	if (turnheld < SLOWTURNTICS)
		tspeed = 2;             // slow turn
	else
		tspeed = speed;

//	if(gamekeydown[key_lookdown] || gamekeydown[key_lookup])
//	{
//		lookheld += ticdup;
//	}
//	else
	{
		lookheld = 0;
	}
	if(lookheld < SLOWTURNTICS)
	{
		lspeed = 1;
	}
	else
	{
		lspeed = 2;
	}

//
// let movement keys cancel each other out
//
	if(strafe)
	{
	/*	if (i_ibm.keyboardState.IsKeyDown(Keys.Right))
			side += sidemove[speed];
		if (i_ibm.keyboardState.IsKeyDown(Keys.Left))
			side -= sidemove[speed];
		if (joyxmove > 0)
			side += sidemove[speed];
		if (joyxmove < 0)
			side -= sidemove[speed];*/
	}
	else
	{
	/*	if (i_ibm.keyboardState.IsKeyDown(Keys.Right))
			cmd.angleturn -= (short)angleturn[tspeed];
		if (i_ibm.keyboardState.IsKeyDown(Keys.Left))
			cmd.angleturn += (short)angleturn[tspeed];
		if (joyxmove > 0)
			cmd.angleturn -= angleturn[tspeed];
		if (joyxmove < 0)
			cmd.angleturn += angleturn[tspeed];*/
		cmd.angleturn -= (short)(i_ibm.mouseDelta.X * (Settings.Default.mouse_sensitivity * 10));
		cmd.angleupdown -= (short)(i_ibm.mouseDelta.Y * (Settings.Default.mouse_sensitivity * 10));
	}

	if (i_ibm.keyboardState.IsKeyDown(Keys.W))
		forward += forwardmove[speed];
	if (i_ibm.keyboardState.IsKeyDown(Keys.S))
		forward -= forwardmove[speed];
	/*if (joyymove < 0)
		forward += forwardmove[speed];
	if (joyymove > 0)
		forward -= forwardmove[speed];*/
	if (i_ibm.keyboardState.IsKeyDown(Keys.D))
		side += sidemove[speed];
	if (i_ibm.keyboardState.IsKeyDown(Keys.A))
		side -= sidemove[speed];

	// Look up/down/center keys
/*	if(gamekeydown[key_lookup])
	{
		look = lspeed;
	}
	if(gamekeydown[key_lookdown])
	{
		look = -lspeed;
	}
	if(gamekeydown[key_lookcenter])
	{
		look = p_local.TOCENTER;
	}*/


	// Fly up/down/drop keys
/*	if(gamekeydown[key_flyup])
	{
		flyheight = 5; // note that the actual flyheight will be twice this
	}
	if(gamekeydown[key_flydown])
	{
		flyheight = -5;
	}
	if(gamekeydown[key_flycenter])
	{
		flyheight = TOCENTER;
		look = TOCENTER;
	}*/

	// Use artifact key
	if (i_ibm.keyboardState.IsKeyDown(Keys.NumPad0))
	{
		if (i_ibm.keyboardState.IsKeyDown(Keys.LeftShift) && !d_main.noartiskip)
		{
			if (players[consoleplayer].inventory[sb_bar.inv_ptr].type != (int)DoomDef.artitype_t.arti_none)
			{
			//	gamekeydown[key_useartifact] = false;
				cmd.arti = 0xff; // skip artifact code
			}
		}
		else
		{
			if(sb_bar.inventory)
			{
				players[consoleplayer].readyArtifact = (DoomDef.artitype_t)
					players[consoleplayer].inventory[sb_bar.inv_ptr].type;
				sb_bar.inventory = false;
				cmd.arti = 0;
				usearti = false;
			}
			else if(usearti)
			{
				cmd.arti = (byte)players[consoleplayer].inventory[sb_bar.inv_ptr].type;
				usearti = false;
			}
		}
	}
/*	if(gamekeydown[127] && cmd.arti == 0
		&& !players[consoleplayer].powers[pw_weaponlevel2])
	{
		gamekeydown[127] = false;
		cmd.arti = arti_tomeofpower;
	}*/

//
// buttons
//
	//cmd.chatchar = CT_dequeueChatChar();
	if (i_ibm.mouseState.LeftButton == ButtonState.Pressed)
	//|| mousebuttons[mousebfire]
	//	|| joybuttons[joybfire])
		cmd.buttons |= DoomDef.BT_ATTACK;

	if (i_ibm.keyboardState.IsKeyDown(Keys.E))// || joybuttons[joybuse])
	{
		cmd.buttons |= DoomDef.BT_USE;
		dclicks = 0;                    // clear double clicks if hit use button
	}

	for (i = 0; i < (int)DoomDef.weapontype_t.NUMWEAPONS - 2; i++)
	{
		if (i_ibm.keyboardState.IsKeyDown(Keys.D1 + i))
		{
			cmd.buttons |= DoomDef.BT_CHANGE;
			cmd.buttons |= (byte)(i<<DoomDef.BT_WEAPONSHIFT);
			break;
		}
	}

//
// mouse
//
/*	if (mousebuttons[mousebforward])
	{
		forward += forwardmove[speed];
	}*/

//
// forward double click
//
/*	if (mousebuttons[mousebforward] != dclickstate && dclicktime > 1 )
	{
		dclickstate = mousebuttons[mousebforward];
		if (dclickstate)
			dclicks++;
		if (dclicks == 2)
		{
			cmd.buttons |= BT_USE;
			dclicks = 0;
		}
		else
			dclicktime = 0;
	}
	else*/
	{
		dclicktime += d_net.ticdup;
		if (dclicktime > 20)
		{
			dclicks = 0;
			dclickstate = 0;
		}
	}

//
// strafe double click
//
	bstrafe = false;
	//mousebuttons[mousebstrafe]
// || joybuttons[joybstrafe];
	if (bstrafe != (dclickstate2 != 0) && dclicktime2 > 1 )
	{
		dclickstate2 = bstrafe?1:0;
		if (dclickstate2 != 0)
			dclicks2++;
		if (dclicks2 == 2)
		{
			cmd.buttons |= DoomDef.BT_USE;
			dclicks2 = 0;
		}
		else
			dclicktime2 = 0;
	}
	else
	{
		dclicktime2 += d_net.ticdup;
		if (dclicktime2 > 20)
		{
			dclicks2 = 0;
			dclickstate2 = 0;
		}
	}

/*	if (strafe)
	{
		side += mousex*2;
	}
	else
	{
		cmd.angleturn -= mousex*0x8;
	}
	forward += mousey;
	mousex = mousey = 0;*/

	if (forward > MAXPLMOVE)
		forward = MAXPLMOVE;
	else if (forward < -MAXPLMOVE)
		forward = -MAXPLMOVE;
	if (side > MAXPLMOVE)
		side = MAXPLMOVE;
	else if (side < -MAXPLMOVE)
		side = -MAXPLMOVE;

	cmd.forwardmove += (sbyte)forward;
	cmd.sidemove += (sbyte)side;
	if (players[consoleplayer].playerstate == DoomDef.playerstate_t.PST_LIVE)
	{
		if(look < 0)
		{
			look += 16;
		}
		cmd.lookfly = (byte)look;
	}
	if(flyheight < 0)
	{
		flyheight += 16;
	}
	cmd.lookfly |= (byte)(flyheight<<4);

//
// special buttons
//
	if (sendpause)
	{
		sendpause = false;
		cmd.buttons = DoomDef.BT_SPECIAL | DoomDef.BTS_PAUSE;
	}

	if (sendsave)
	{
		sendsave = false;
		cmd.buttons = (byte)(DoomDef.BT_SPECIAL | DoomDef.BTS_SAVEGAME | (savegameslot << DoomDef.BTS_SAVESHIFT));
	}
}

/*
==============
=
= G_DoLoadLevel
=
==============
*/

public static void G_DoLoadLevel ()
{
	int             i, j;

	levelstarttic = gametic;        // for time calculation
	gamestate = DoomDef.gamestate_t.GS_LEVEL;
	for (i=0 ; i<DoomDef.MAXPLAYERS ; i++)
	{
		if (playeringame[i] && players[i].playerstate == DoomDef.playerstate_t.PST_DEAD)
			players[i].playerstate = DoomDef.playerstate_t.PST_REBORN;

		for (j=0 ; j<DoomDef.MAXPLAYERS ; j++)
		{
			players[i].frags[j] = 0;
		}
	}

	p_setup.P_SetupLevel (gameepisode, gamemap, 0, gameskill);
	displayplayer = consoleplayer;      // view the guy you are playing
	starttime = i_ibm.I_GetTime ();
	gameaction = DoomDef.gameaction_t.ga_nothing;
	//Z_CheckHeap ();

//
// clear cmd building stuff
//

#if DOS
	memset (gamekeydown, 0, sizeof(gamekeydown));
	joyxmove = joyymove = 0;
	mousex = mousey = 0;
	sendpause = sendsave = paused = false;
	memset (mousebuttons, 0, sizeof(mousebuttons));
	memset (joybuttons, 0, sizeof(joybuttons));
#else
	i_ibm.oldKeyboardState = i_ibm.keyboardState;
	i_ibm.oldMouseState = i_ibm.mouseState;
#endif
}

/*
===============================================================================
=
= G_Responder
=
= get info needed to make ticcmd_ts for the players
=
===============================================================================
*/

static public bool G_Responder(DoomDef.event_t ev)
{
#if DOS
	player_t *plr;
	extern boolean MenuActive;

	plr = &players[consoleplayer];
	if(ev.type == ev_keyup && ev.data1 == key_useartifact)
	{ // flag to denote that it's okay to use an artifact
		if(!inventory)
		{
			plr.readyArtifact = plr.inventory[inv_ptr].type;
		}
		usearti = true;
	}

	// Check for spy mode player cycle
	if(gamestate == GS_LEVEL && ev.type == ev_keydown
		&& ev.data1 == KEY_F12 && !deathmatch)
	{ // Cycle the display player
		do
		{
			displayplayer++;
			if(displayplayer == MAXPLAYERS)
			{
				displayplayer = 0;
			}
		} while(!playeringame[displayplayer]
			&& displayplayer != consoleplayer);
		return(true);
	}
#endif

	if (gamestate == DoomDef.gamestate_t.GS_LEVEL)
	{
		//if(CT_Responder(ev))
		//{ // Chat ate the event
		//    return(true);
		//}
		if(sb_bar.SB_Responder(ev))
		{ // Status bar ate the event
			return(true);
		}
		//if(AM_Responder(ev))
		//{ // Automap ate the event
		//    return(true);
		//}
	}
#if DOS

	switch(ev.type)
	{
		case ev_keydown:
			if(ev.data1 == key_invleft)
			{
				inventoryTics = 5*35;
				if(!inventory)
				{
					inventory = true;
					break;
				}
				inv_ptr--;
				if(inv_ptr < 0)
				{
					inv_ptr = 0;
				}
				else
				{
					curpos--;
					if(curpos < 0)
					{
						curpos = 0;
					}
				}
				return(true);
			}
			if(ev.data1 == key_invright)
			{
				inventoryTics = 5*35;
				if(!inventory)
				{
					inventory = true;
					break;
				}
				inv_ptr++;
				if(inv_ptr >= plr.inventorySlotNum)
				{
					inv_ptr--;
					if(inv_ptr < 0)
						inv_ptr = 0;
				}
				else
				{
					curpos++;
					if(curpos > 6)
					{
						curpos = 6;
					}
				}
				return(true);
			}
			if(ev.data1 == KEY_PAUSE && !MenuActive)
			{
				sendpause = true;
				return(true);
			}
			if(ev.data1 < NUMKEYS)
			{
				gamekeydown[ev.data1] = true;
			}
			return(true); // eat key down events

		case ev_keyup:
			if(ev.data1 < NUMKEYS)
			{
				gamekeydown[ev.data1] = false;
			}
			return(false); // always let key up events filter down

		case ev_mouse:
			mousebuttons[0] = ev.data1&1;
			mousebuttons[1] = ev.data1&2;
			mousebuttons[2] = ev.data1&4;
			mousex = ev.data2*(mouseSensitivity+5)/10;
			mousey = ev.data3*(mouseSensitivity+5)/10;
			return(true); // eat events

		case ev_joystick:
			joybuttons[0] = ev.data1&1;
			joybuttons[1] = ev.data1&2;
			joybuttons[2] = ev.data1&4;
			joybuttons[3] = ev.data1&8;
			joyxmove = ev.data2;
			joyymove = ev.data3;
			return(true); // eat events

		default:
			break;
	}
#endif
	return(false);
}
/*
===============================================================================
=
= G_Ticker
=
===============================================================================
*/

public static void G_Ticker ()
{
	int             i, buf;
	DoomDef.ticcmd_t        cmd;

//
// do player reborns if needed
//
	for (i=0 ; i<DoomDef.MAXPLAYERS ; i++)
		if (playeringame[i] && players[i].playerstate == DoomDef.playerstate_t.PST_REBORN)
			G_DoReborn (i);
//
// do things to change the game state
//
	while (gameaction != DoomDef.gameaction_t.ga_nothing)
	{
		switch (gameaction)
		{
			case DoomDef.gameaction_t.ga_loadlevel:
				//G_DoLoadLevel ();
				break;
			case DoomDef.gameaction_t.ga_newgame:
				G_DoNewGame ();
				break;
			case DoomDef.gameaction_t.ga_loadgame:
				//G_DoLoadGame ();
				break;
			case DoomDef.gameaction_t.ga_savegame:
				//G_DoSaveGame ();
				break;
			case DoomDef.gameaction_t.ga_playdemo:
				//G_DoPlayDemo ();
				break;
			case DoomDef.gameaction_t.ga_screenshot:
				//M_ScreenShot ();
				//gameaction = DoomDef.gameaction_t.ga_nothing;
				break;
			case DoomDef.gameaction_t.ga_completed:
				G_DoCompleted ();
				break;
			case DoomDef.gameaction_t.ga_worlddone:
				G_DoWorldDone();
				break;
			case DoomDef.gameaction_t.ga_victory:
				//F_StartFinale();
				break;
			default:
				break;
		}
	}

//
// get commands, check consistancy, and build new consistancy check
//
	buf = (gametic / d_net.ticdup) % DoomDef.BACKUPTICS;

	for (i=0 ; i<DoomDef.MAXPLAYERS ; i++)
		if (playeringame[i])
		{
			cmd = players[i].cmd;

			cmd.set(d_net.netcmds[i, buf]);

			//if (demoplayback)
			//	G_ReadDemoTiccmd (cmd);
			//if (demorecording)
			//	G_WriteDemoTiccmd (cmd);

			if (netgame && (gametic%d_net.ticdup) == 0 )
			{
				if (gametic > DoomDef.BACKUPTICS
				&& consistancy[i, buf] != cmd.consistancy)
				{
					i_ibm.I_Error("consistency failure (" + cmd.consistancy + " should be " + consistancy[i, buf] + ")");
				}
				if (players[i].mo != null)
					consistancy[i, buf] = (short)players[i].mo.x;
				else
					consistancy[i, buf] = (short)m_misc.rndindex;
			}
		}


#if DOS

//
// check for special buttons
//
	for (i=0 ; i<MAXPLAYERS ; i++)
		if (playeringame[i])
		{
			if (players[i].cmd.buttons & BT_SPECIAL)
			{
				switch (players[i].cmd.buttons & BT_SPECIALMASK)
				{
				case BTS_PAUSE:
					paused ^= 1;
					if(paused)
					{
						S_PauseSound();
					}
					else
					{
						S_ResumeSound();
					}
					break;

				case BTS_SAVEGAME:
					if (!savedescription[0])
					{
						if(netgame)
						{
							strcpy (savedescription, "NET GAME");
						}
						else
						{
							strcpy(savedescription, "SAVE GAME");
						}
					}
					savegameslot =
						(players[i].cmd.buttons & BTS_SAVEMASK)>>BTS_SAVESHIFT;
					gameaction = ga_savegame;
					break;
				}
			}
		}
	// turn inventory off after a certain amount of time
	if(inventory && !(--inventoryTics))
	{
		players[consoleplayer].readyArtifact =
			players[consoleplayer].inventory[inv_ptr].type;
		inventory = false;
		cmd.arti = 0;
	}
#endif
	//
// do main actions
//
//
// do main actions
//
	switch (gamestate)
	{
		case DoomDef.gamestate_t.GS_LEVEL:
			p_tick.P_Ticker ();
			sb_bar.SB_Ticker ();
		//	AM_Ticker ();
		//	CT_Ticker();
			break;
		case DoomDef.gamestate_t.GS_INTERMISSION:
			in_lude.IN_Ticker ();
			break;
		case DoomDef.gamestate_t.GS_FINALE:
		//	F_Ticker();
			break;
		case DoomDef.gamestate_t.GS_DEMOSCREEN:
		//	D_PageTicker ();
			break;
	}
}

#if DOS

/*
==============================================================================

						PLAYER STRUCTURE FUNCTIONS

also see P_SpawnPlayer in P_Things
==============================================================================
*/

/*
====================
=
= G_InitPlayer
=
= Called at the start
= Called by the game initialization functions
====================
*/

void G_InitPlayer (int player)
{
	player_t        *p;

// set up the saved info
	p = &players[player];

// clear everything else to defaults
	G_PlayerReborn (player);

}
#endif

/*
====================
=
= G_PlayerFinishLevel
=
= Can when a player completes a level
====================
*/

public static void G_PlayerFinishLevel(int player)
{
	DoomDef.player_t p;
	int i;

	// END HACK
	p = players[player];
	for(i=0; i<p.inventorySlotNum; i++)
	{
		p.inventory[i].count = 1;
	}
	p.artifactCount = p.inventorySlotNum;

	if(!deathmatch)
	{
		for(i = 0; i < 16; i++)
		{
			p_user.P_PlayerUseArtifact(p, DoomDef.artitype_t.arti_fly);
		}
	}
	for ( i =0 ; i <p.powers.Count(); ++i)
	{
		p.powers[i] = 0;
	}
	for ( i =0 ; i <p.keys.Count(); ++i)
	{
		p.keys[i] = false;
	}

	sb_bar.playerkeys = 0;
	if(p.chickenTics != 0)
	{
		p.readyweapon = (DoomDef.weapontype_t)p.mo.special1; // Restore weapon
		p.chickenTics = 0;
	}
	p.messageTics = 0;
	p.lookdir = 0;
	p.mo.flags &= ~DoomDef.MF_SHADOW; // Remove invisibility
	p.extralight = 0; // Remove weapon flashes
	p.fixedcolormap = 0; // Remove torch
	p.damagecount = 0; // No palette changes
	p.bonuscount = 0;
	p.rain1 = null;
	p.rain2 = null;
	if(p == players[consoleplayer])
	{
		sb_bar.SB_state = -1; // refresh the status bar
	}
}

/*
====================
=
= G_PlayerReborn
=
= Called after a player dies
= almost everything is cleared and initialized
====================
*/

public static void G_PlayerReborn(int player)
{
	DoomDef.player_t p;
	int i;
	int[] frags = new int[DoomDef.MAXPLAYERS];
	int killcount, itemcount, secretcount;
	bool secret;

	secret = false;
	for (i = 0; i < frags.Length; ++i)
	{
		frags[i] = g_game.players[player].frags[i];
	}
	killcount = players[player].killcount;
	itemcount = players[player].itemcount;
	secretcount = players[player].secretcount;

	p = g_game.players[player];
	if(p.didsecret)
	{
		secret = true;
	}
	p.reset();

	for (i = 0; i < frags.Length; ++i)
	{
		g_game.players[player].frags[i] = frags[i];
	}
	players[player].killcount = killcount;
	players[player].itemcount = itemcount;
	players[player].secretcount = secretcount;

	p.usedown = p.attackdown = 1; // don't do anything immediately
	p.playerstate = DoomDef.playerstate_t.PST_LIVE;
	p.health = p_local.MAXHEALTH;
	p.readyweapon = p.pendingweapon = DoomDef.weapontype_t.wp_goldwand;
	p.weaponowned[(int)DoomDef.weapontype_t.wp_staff] = true;
	p.weaponowned[(int)DoomDef.weapontype_t.wp_goldwand] = true;
	p.messageTics = 0;
	p.lookdir = 0;
	p.ammo[(int)DoomDef.ammotype_t.am_goldwand] = 50;
	for (i = 0; i < (int)DoomDef.ammotype_t.NUMAMMO; i++)
	{
		p.maxammo[i] = p_inter.maxammo[i];
	}
	if(gamemap == 9 || secret)
	{
		p.didsecret = true;
	}
	if(p == g_game.players[g_game.consoleplayer])
	{
		sb_bar.SB_state = -1; // refresh the status bar
		sb_bar.inv_ptr = 0; // reset the inventory pointer
		sb_bar.curpos = 0;
	}
}

/*
====================
=
= G_CheckSpot
=
= Returns false if the player cannot be respawned at the given mapthing_t spot
= because something is occupying it
====================
*/

public static bool G_CheckSpot (int playernum, DoomData.mapthing_t mthing)
{
	int         x,y;
	r_local.subsector_t ss;
	uint        an;
	DoomDef.mobj_t      mo;

	x = mthing.x << DoomDef.FRACBITS;
	y = mthing.y << DoomDef.FRACBITS;

	players[playernum].mo.flags2 &= ~DoomDef.MF2_PASSMOBJ;
	if (!p_map.P_CheckPosition (players[playernum].mo, x, y) )
	{
		players[playernum].mo.flags2 |= DoomDef.MF2_PASSMOBJ;
		return false;
	}
	players[playernum].mo.flags2 |= DoomDef.MF2_PASSMOBJ;

// spawn a teleport fog
	ss = r_main.R_PointInSubsector (x,y);
	an = (uint)((int)(DoomDef.ANG45 * (mthing.angle / 45)) >> (int)DoomDef.ANGLETOFINESHIFT);

	mo = p_mobj.P_SpawnMobj (x+20*r_main.finecosine(an), y+20*tables.finesine[an]
	, ss.sector.floorheight+DoomDef.TELEFOGHEIGHT
, info.mobjtype_t.MT_TFOG);

	if (players[consoleplayer].viewz != 1)
		i_ibm.S_StartSound (mo, (int)sounds.sfxenum_t.sfx_telept);  // don't start sound on first frame

	return true;
}

/*
====================
=
= G_DeathMatchSpawnPlayer
=
= Spawns a player at one of the random death match spots
= called at level load and each death
====================
*/

public static void G_DeathMatchSpawnPlayer (int playernum)
{
	int             i,j;
	int             selections;

	selections = p_setup.deathmatch_pi;
	if (selections < 4)
		i_ibm.I_Error("Only " + selections + " deathmatch spots, 4 required");

	for (j=0 ; j<20 ; j++)
	{
		i = m_misc.P_Random() % selections;
		if (G_CheckSpot (playernum, p_setup.deathmatchstarts[i]) )
		{
			p_setup.deathmatchstarts[i].type = (short)(playernum + 1);
			p_mobj.P_SpawnPlayer(p_setup.deathmatchstarts[i]);
			return;
		}
	}

// no good spot, so the player will probably get stuck
	p_mobj.P_SpawnPlayer(p_setup.playerstarts[playernum]);
}

/*
====================
=
= G_DoReborn
=
====================
*/

public static void G_DoReborn (int playernum)
{
#if DOS
	int                             i;

	if (G_CheckDemoStatus ())
		return;
	if (!netgame)
		gameaction = ga_loadlevel;                      // reload the level from scratch
	else
	{       // respawn at the start
		players[playernum].mo.player = NULL;   // dissasociate the corpse

		// spawn at random spot if in death match
		if (deathmatch)
		{
			G_DeathMatchSpawnPlayer (playernum);
			return;
		}

		if (G_CheckSpot (playernum, &playerstarts[playernum]) )
		{
			P_SpawnPlayer (&playerstarts[playernum]);
			return;
		}
		// try to spawn at one of the other players spots
		for (i=0 ; i<MAXPLAYERS ; i++)
			if (G_CheckSpot (playernum, &playerstarts[i]) )
			{
				playerstarts[i].type = playernum+1;             // fake as other player
				P_SpawnPlayer (&playerstarts[i]);
				playerstarts[i].type = i+1;                             // restore
				return;
			}
		// he's going to be inside something.  Too bad.
		P_SpawnPlayer (&playerstarts[playernum]);
	}
#endif
}

public static void G_ScreenShot ()
{
	gameaction = DoomDef.gameaction_t.ga_screenshot;
}

/*
====================
=
= G_DoCompleted
=
====================
*/

public static bool         secretexit;

public static void G_ExitLevel ()
{
	secretexit = false;
	gameaction = DoomDef.gameaction_t.ga_completed;
}

public static void G_SecretExitLevel ()
{
	secretexit = true;
	gameaction = DoomDef.gameaction_t.ga_completed;
}

	static int[] afterSecret = new int[5] { 7, 5, 5, 5, 4 };

public static void G_DoCompleted()
{
	int i;

	gameaction = DoomDef.gameaction_t.ga_nothing;
	//if(G_CheckDemoStatus())
	//{
	//    return;
	//}
	for(i = 0; i < DoomDef.MAXPLAYERS; i++)
	{
		if(playeringame[i])
		{
			G_PlayerFinishLevel(i);
		}
	}
	prevmap = gamemap;
	if(secretexit == true)
	{
		gamemap = 9;
	}
	else if(gamemap == 9)
	{ // Finished secret level
		gamemap = afterSecret[gameepisode-1];
	}
	else if(gamemap == 8)
	{
		gameaction = DoomDef.gameaction_t.ga_victory;
		return;
	}
	else
	{
		gamemap++;
	}
	gamestate = DoomDef.gamestate_t.GS_INTERMISSION;
	in_lude.IN_Start();
}

//============================================================================
//
// G_WorldDone
//
//============================================================================

public static void G_WorldDone()
{
	gameaction = DoomDef.gameaction_t.ga_worlddone;
}

//============================================================================
//
// G_DoWorldDone
//
//============================================================================

public static void G_DoWorldDone()
{
	gamestate = DoomDef.gamestate_t.GS_LEVEL;
	G_DoLoadLevel();
	gameaction = DoomDef.gameaction_t.ga_nothing;
	viewactive = true;
}

#if DOS

//---------------------------------------------------------------------------
//
// PROC G_LoadGame
//
// Can be called by the startup code or the menu task.
//
//---------------------------------------------------------------------------

char savename[256];

void G_LoadGame(char *name)
{
	strcpy(savename, name);
	gameaction = ga_loadgame;
}

//---------------------------------------------------------------------------
//
// PROC G_DoLoadGame
//
// Called by G_Ticker based on gameaction.
//
//---------------------------------------------------------------------------

//TODO: public const int VERSIONSIZE 16

void G_DoLoadGame(void)
{
	int length;
	int i;
	int a, b, c;
	char vcheck[VERSIONSIZE];

	gameaction = ga_nothing;

	length = M_ReadFile(savename, &savebuffer);
	save_p = savebuffer+SAVESTRINGSIZE;
	// Skip the description field
	memset(vcheck, 0, sizeof(vcheck));
	sprintf(vcheck, "version %i", VERSION);
	if (strcmp (save_p, vcheck))
	{ // Bad version
		return;
	}
	save_p += VERSIONSIZE;
	gameskill = *save_p++;
	gameepisode = *save_p++;
	gamemap = *save_p++;
	for(i = 0; i < MAXPLAYERS; i++)
	{
		playeringame[i] = *save_p++;
	}
	// Load a base level
	G_InitNew(gameskill, gameepisode, gamemap);

	// Create leveltime
	a = *save_p++;
	b = *save_p++;
	c = *save_p++;
	leveltime = (a<<16)+(b<<8)+c;

	// De-archive all the modifications
	P_UnArchivePlayers();
	P_UnArchiveWorld();
	P_UnArchiveThinkers();
	P_UnArchiveSpecials();

	if(*save_p != SAVE_GAME_TERMINATOR)
	{ // Missing savegame termination marker
		I_Error("Bad savegame");
	}
	Z_Free(savebuffer);
}
#endif

/*
====================
=
= G_InitNew
=
= Can be called by the startup code or the menu task
= consoleplayer, displayplayer, playeringame[] should be set
====================
*/

public static DoomDef.skill_t d_skill;
public static int d_episode;
public static int d_map;

public static void G_DeferedInitNew(DoomDef.skill_t skill, int episode, int map)
{
	d_skill = skill;
	d_episode = episode;
	d_map = map;
	gameaction = DoomDef.gameaction_t.ga_newgame;
}


public static void G_DoNewGame ()
{
	G_InitNew (d_skill, d_episode, d_map);
	gameaction = DoomDef.gameaction_t.ga_nothing;
}


	static string[] skyLumpNames = new string[]
	{
		"SKY1", "SKY2", "SKY3", "SKY1", "SKY3"
	};

public static void G_InitNew(DoomDef.skill_t skill, int episode, int map)
{
	int i;
	int speed;

	if(paused)
	{
		paused = false;
		i_ibm.S_ResumeSound();
	}
	if (skill < DoomDef.skill_t.sk_baby)
		skill = DoomDef.skill_t.sk_baby;
	if (skill > DoomDef.skill_t.sk_nightmare)
		skill = DoomDef.skill_t.sk_nightmare;
	if(episode < 1)
		episode = 1;
	// Up to 9 episodes for testing
	if(episode > 9)
		episode = 9;
	if(map < 1)
		map = 1;
	if(map > 9)
		map = 9;
	m_misc.M_ClearRandom();
	if(d_main.respawnparm)
	{
		respawnmonsters = true;
	}
	else
	{
		respawnmonsters = false;
	}
	// Set monster missile speeds
	speed = (skill == DoomDef.skill_t.sk_nightmare) ? 1 : 0;
	foreach (sMonsterMissileInfo missileInfo in MonsterMissileInfo)	{
		info.mobjinfo[(int)missileInfo.type].speed = missileInfo.speed[speed] << DoomDef.FRACBITS;
	}
	// Force players to be initialized upon first level load
	for(i = 0; i < DoomDef.MAXPLAYERS; i++)
	{
		players[i].playerstate = DoomDef.playerstate_t.PST_REBORN;
		players[i].didsecret = false;
	}
	// Set up a bunch of globals
	usergame = true; // will be set false if a demo
	paused = false;
	demorecording = false;
	demoplayback = false;
	viewactive = true;
	gameepisode = episode;
	gamemap = map;
	gameskill = skill;
	viewactive = true;
//	BorderNeedRefresh = true;

	// Set the sky map
	if(episode > 5)
	{
		r_plane.skytexture = r_data.R_TextureNumForName("SKY1");
	}
	else
	{
		r_plane.skytexture = r_data.R_TextureNumForName(skyLumpNames[episode - 1]);
	}

//
// give one null ticcmd_t
//
	G_DoLoadLevel();
}

#if DOS


/*
===============================================================================

							DEMO RECORDING

===============================================================================
*/

//TODO: public const int DEMOMARKER      0x80

void G_ReadDemoTiccmd (ticcmd_t *cmd)
{
	if (*demo_p == DEMOMARKER)
	{       // end of demo data stream
		G_CheckDemoStatus ();
		return;
	}
	cmd.forwardmove = ((signed char)*demo_p++);
	cmd.sidemove = ((signed char)*demo_p++);
	cmd.angleturn = ((unsigned char)*demo_p++)<<8;
	cmd.buttons = (unsigned char)*demo_p++;
	cmd.lookfly = (unsigned char)*demo_p++;
	cmd.arti = (unsigned char)*demo_p++;
}

void G_WriteDemoTiccmd (ticcmd_t *cmd)
{
	if (gamekeydown['q'])           // press q to end demo recording
		G_CheckDemoStatus ();
	*demo_p++ = cmd.forwardmove;
	*demo_p++ = cmd.sidemove;
	*demo_p++ = cmd.angleturn>>8;
	*demo_p++ = cmd.buttons;
	*demo_p++ = cmd.lookfly;
	*demo_p++ = cmd.arti;
	demo_p -= 6;
	G_ReadDemoTiccmd (cmd);         // make SURE it is exactly the same
}



/*
===================
=
= G_RecordDemo
=
===================
*/

void G_RecordDemo (skill_t skill, int numplayers, int episode, int map, char *name)
{
	int             i;

	G_InitNew (skill, episode, map);
	usergame = false;
	strcpy (demoname, name);
	strcat (demoname, ".lmp");
	demobuffer = demo_p = Z_Malloc (0x20000,PU_STATIC,NULL);
	*demo_p++ = skill;
	*demo_p++ = episode;
	*demo_p++ = map;

	for (i=0 ; i<MAXPLAYERS ; i++)
		*demo_p++ = playeringame[i];

	demorecording = true;
}


/*
===================
=
= G_PlayDemo
=
===================
*/

char    *defdemoname;

void G_DeferedPlayDemo (char *name)
{
	defdemoname = name;
	gameaction = ga_playdemo;
}

void G_DoPlayDemo (void)
{
	skill_t skill;
	int             i, episode, map;

	gameaction = ga_nothing;
	demobuffer = demo_p = W_CacheLumpName (defdemoname, PU_STATIC);
	skill = *demo_p++;
	episode = *demo_p++;
	map = *demo_p++;

	for (i=0 ; i<MAXPLAYERS ; i++)
		playeringame[i] = *demo_p++;

	precache = false;               // don't spend a lot of time in loadlevel
	G_InitNew (skill, episode, map);
	precache = true;
	usergame = false;
	demoplayback = true;
}


/*
===================
=
= G_TimeDemo
=
===================
*/

void G_TimeDemo (char *name)
{
	skill_t skill;
	int             episode, map;

	demobuffer = demo_p = W_CacheLumpName (name, PU_STATIC);
	skill = *demo_p++;
	episode = *demo_p++;
	map = *demo_p++;
	G_InitNew (skill, episode, map);
	usergame = false;
	demoplayback = true;
	timingdemo = true;
	singletics = true;
}


/*
===================
=
= G_CheckDemoStatus
=
= Called after a death or level completion to allow demos to be cleaned up
= Returns true if a new demo loop action will take place
===================
*/

boolean G_CheckDemoStatus (void)
{
	int             endtime;

	if (timingdemo)
	{
		endtime = I_GetTime ();
		I_Error ("timed %i gametics in %i realtics",gametic
		, endtime-starttime);
	}

	if (demoplayback)
	{
		if (singledemo)
			I_Quit ();

		Z_ChangeTag (demobuffer, PU_CACHE);
		demoplayback = false;
		D_AdvanceDemo ();
		return true;
	}

	if (demorecording)
	{
		*demo_p++ = DEMOMARKER;
		M_WriteFile (demoname, demobuffer, demo_p - demobuffer);
		Z_Free (demobuffer);
		demorecording = false;
		I_Error ("Demo %s recorded",demoname);
	}

	return false;
}

/**************************************************************************/
/**************************************************************************/

//==========================================================================
//
// G_SaveGame
//
// Called by the menu task.  <description> is a 24 byte text string.
//
//==========================================================================

void G_SaveGame(int slot, char *description)
{
	savegameslot = slot;
	strcpy(savedescription, description);
	sendsave = true;
}

//==========================================================================
//
// G_DoSaveGame
//
// Called by G_Ticker based on gameaction.
//
//==========================================================================

void G_DoSaveGame(void)
{
	int i;
	char name[100];
	char verString[VERSIONSIZE];
	char *description;

	if(cdrom)
	{
		sprintf(name, SAVEGAMENAMECD"%d.hsg", savegameslot);
	}
	else
	{
		sprintf(name, SAVEGAMENAME"%d.hsg", savegameslot);
	}
	description = savedescription;

	SV_Open(name);
	SV_Write(description, SAVESTRINGSIZE);
	memset(verString, 0, sizeof(verString));
	sprintf(verString, "version %i", VERSION);
	SV_Write(verString, VERSIONSIZE);
	SV_WriteByte(gameskill);
	SV_WriteByte(gameepisode);
	SV_WriteByte(gamemap);
	for(i = 0; i < MAXPLAYERS; i++)
	{
		SV_WriteByte(playeringame[i]);
	}
	SV_WriteByte(leveltime>>16);
	SV_WriteByte(leveltime>>8);
	SV_WriteByte(leveltime);
	P_ArchivePlayers();
	P_ArchiveWorld();
	P_ArchiveThinkers();
	P_ArchiveSpecials();
	SV_Close(name);

	gameaction = ga_nothing;
	savedescription[0] = 0;
	P_SetMessage(&players[consoleplayer], TXT_GAMESAVED, true);
}

//==========================================================================
//
// SV_Open
//
//==========================================================================

void SV_Open(char *fileName)
{
	MallocFailureOk = true;
	save_p = savebuffer = Z_Malloc(SAVEGAMESIZE, PU_STATIC, NULL);
	MallocFailureOk = false;
	if(savebuffer == NULL)
	{ // Not enough memory - use file save method
		SaveGameType = SVG_FILE;
		SaveGameFP = fopen(fileName, "wb");
	}
	else
	{
		SaveGameType = SVG_RAM;
	}
}

//==========================================================================
//
// SV_Close
//
//==========================================================================

void SV_Close(char *fileName)
{
	int length;

	SV_WriteByte(SAVE_GAME_TERMINATOR);
	if(SaveGameType == SVG_RAM)
	{
		length = save_p-savebuffer;
		if(length > SAVEGAMESIZE)
		{
			I_Error("Savegame buffer overrun");
		}
		M_WriteFile(fileName, savebuffer, length);
		Z_Free(savebuffer);
	}
	else
	{ // SVG_FILE
		fclose(SaveGameFP);
	}
}

//==========================================================================
//
// SV_Write
//
//==========================================================================

void SV_Write(void *buffer, int size)
{
	if(SaveGameType == SVG_RAM)
	{
		memcpy(save_p, buffer, size);
		save_p += size;
	}
	else
	{ // SVG_FILE
		fwrite(buffer, size, 1, SaveGameFP);
	}
}

void SV_WriteByte(byte val)
{
	SV_Write(&val, sizeof(byte));
}

void SV_WriteWord(unsigned short val)
{
	SV_Write(&val, sizeof(unsigned short));
}

void SV_WriteLong(unsigned int val)
{
	SV_Write(&val, sizeof(int));
}

#endif
	}
}
