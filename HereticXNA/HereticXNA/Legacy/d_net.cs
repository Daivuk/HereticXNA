using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// d_net.c
// This version has the fixed ticdup code

namespace HereticXNA
{
	public static class d_net
	{
		public const uint NCMD_EXIT = 0x80000000;
		public const int NCMD_RETRANSMIT = 0x40000000;
		public const int NCMD_SETUP = 0x20000000;
		public const int NCMD_KILL = 0x10000000;             // kill game
		public const int NCMD_CHECKSUM = 0x0fffffff;
#if DOS
		


doomcom_t               *doomcom;
doomdata_t              *netbuffer;             // points inside doomcom


/*
==============================================================================

							NETWORKING

gametic is the tic about to (or currently being) run
maketic is the tick that hasn't had control made for it yet
nettics[] has the maketics for all players

a gametic cannot be run until nettics[] > gametic for all players

==============================================================================
*/

define RESENDCOUNT     10
define PL_DRONE        0x80                            // bit flag in doomdata.player

ticcmd_t                localcmds[BACKUPTICS];

#endif
		public static DoomDef.ticcmd_t[,] netcmds = new DoomDef.ticcmd_t[DoomDef.MAXPLAYERS, DoomDef.BACKUPTICS] {
			{new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t()},
			{new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t()},
			{new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t()},
			{new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),
			new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t(),new DoomDef.ticcmd_t()}
};
		public static int[] nettics = new int[DoomDef.MAXNETNODES];
		public static bool[] nodeingame = new bool[DoomDef.MAXNETNODES];      // set false as nodes leave game
		public static bool[] remoteresend = new bool[DoomDef.MAXNETNODES];      // set when local needs tics
		public static int[] resendto = new int[DoomDef.MAXNETNODES];                  // set when remote needs tics
#if DOS
int                             resendcount[MAXNETNODES];

int                             nodeforplayer[MAXPLAYERS];
#endif
		public static int maketic;
		public static int lastnettic, skiptics;
		public static int ticdup;
		public static int maxsend;        // BACKUPTICS/(2*ticdup)-1
#if DOS

boolean                 reboundpacket;
doomdata_t              reboundstore;


int     NetbufferSize (void)
{
	return (int)&(((doomdata_t *)0).cmds[netbuffer.numtics]);
}

unsigned NetbufferChecksum (void)
{
	unsigned                c;
	int             i,l;

	c = 0x1234567;

	l = (NetbufferSize () - (int)&(((doomdata_t *)0).retransmitfrom))/4;
	for (i=0 ; i<l ; i++)
		c += ((unsigned *)&netbuffer.retransmitfrom)[i] * (i+1);

	return c & NCMD_CHECKSUM;
}

int ExpandTics (int low)
{
	int     delta;

	delta = low - (maketic&0xff);

	if (delta >= -64 && delta <= 64)
		return (maketic&~0xff) + low;
	if (delta > 64)
		return (maketic&~0xff) - 256 + low;
	if (delta < -64)
		return (maketic&~0xff) + 256 + low;

	I_Error ("ExpandTics: strange value %i at maketic %i",low,maketic);
	return 0;
}


//============================================================================


/*
==============
=
= HSendPacket
=
==============
*/

void HSendPacket (int node, int flags)
{
	netbuffer.checksum = NetbufferChecksum () | flags;

	if (!node)
	{
		reboundstore = *netbuffer;
		reboundpacket = true;
		return;
	}

	if (demoplayback)
		return;

	if (!netgame)
		I_Error ("Tried to transmit to another node");

	doomcom.command = CMD_SEND;
	doomcom.remotenode = node;
	doomcom.datalength = NetbufferSize ();

if (debugfile)
{
	int             i;
	int             realretrans;
	if (netbuffer.checksum & NCMD_RETRANSMIT)
		realretrans = ExpandTics (netbuffer.retransmitfrom);
	else
		realretrans = -1;
	fprintf (debugfile,"send (%i + %i, R %i) [%i] "
	,ExpandTics(netbuffer.starttic),netbuffer.numtics, realretrans, doomcom.datalength);
	for (i=0 ; i<doomcom.datalength ; i++)
		fprintf (debugfile,"%i ",((byte *)netbuffer)[i]);
	fprintf (debugfile,"\n");
}

	I_NetCmd ();
}

/*
==============
=
= HGetPacket
=
= Returns false if no packet is waiting
=
==============
*/

boolean HGetPacket (void)
{
	if (reboundpacket)
	{
		*netbuffer = reboundstore;
		doomcom.remotenode = 0;
		reboundpacket = false;
		return true;
	}

	if (!netgame)
		return false;
	if (demoplayback)
		return false;

	doomcom.command = CMD_GET;
	I_NetCmd ();
	if (doomcom.remotenode == -1)
		return false;

	if (doomcom.datalength != NetbufferSize ())
	{
		if (debugfile)
			fprintf (debugfile,"bad packet length %i\n",doomcom.datalength);
		return false;
	}

	if (NetbufferChecksum () != (netbuffer.checksum&NCMD_CHECKSUM) )
	{
		if (debugfile)
			fprintf (debugfile,"bad packet checksum\n");
		return false;
	}

if (debugfile)
{
	int             realretrans;
			int     i;

	if (netbuffer.checksum & NCMD_SETUP)
		fprintf (debugfile,"setup packet\n");
	else
	{
		if (netbuffer.checksum & NCMD_RETRANSMIT)
			realretrans = ExpandTics (netbuffer.retransmitfrom);
		else
			realretrans = -1;
		fprintf (debugfile,"get %i = (%i + %i, R %i)[%i] ",doomcom.remotenode,
		ExpandTics(netbuffer.starttic),netbuffer.numtics, realretrans, doomcom.datalength);
		for (i=0 ; i<doomcom.datalength ; i++)
			fprintf (debugfile,"%i ",((byte *)netbuffer)[i]);
		fprintf (debugfile,"\n");
	}
}
	return true;
}


/*
===================
=
= GetPackets
=
===================
*/

char    exitmsg[80];

void GetPackets (void)
{
	int             netconsole;
	int             netnode;
	ticcmd_t        *src, *dest;
	int             realend;
	int             realstart;

	while (HGetPacket ())
	{
		if (netbuffer.checksum & NCMD_SETUP)
			continue;               // extra setup packet

		netconsole = netbuffer.player & ~PL_DRONE;
		netnode = doomcom.remotenode;
		//
		// to save bytes, only the low byte of tic numbers are sent
		// Figure out what the rest of the bytes are
		//
		realstart = ExpandTics (netbuffer.starttic);
		realend = (realstart+netbuffer.numtics);

		//
		// check for exiting the game
		//
		if (netbuffer.checksum & NCMD_EXIT)
		{
			if (!nodeingame[netnode])
				continue;
			nodeingame[netnode] = false;
			playeringame[netconsole] = false;
			strcpy (exitmsg, "PLAYER 1 LEFT THE GAME");
			exitmsg[7] += netconsole;
			players[consoleplayer].message = exitmsg;
//			if (demorecording)
//				G_CheckDemoStatus ();
			continue;
		}

		//
		// check for a remote game kill
		//
		if (netbuffer.checksum & NCMD_KILL)
			I_Error ("Killed by network driver");

		nodeforplayer[netconsole] = netnode;

		//
		// check for retransmit request
		//
		if ( resendcount[netnode] <= 0
		&& (netbuffer.checksum & NCMD_RETRANSMIT) )
		{
			resendto[netnode] = ExpandTics(netbuffer.retransmitfrom);
if (debugfile)
fprintf (debugfile,"retransmit from %i\n", resendto[netnode]);
			resendcount[netnode] = RESENDCOUNT;
		}
		else
			resendcount[netnode]--;

		//
		// check for out of order / duplicated packet
		//
		if (realend == nettics[netnode])
			continue;

		if (realend < nettics[netnode])
		{
if (debugfile)
fprintf (debugfile,"out of order packet (%i + %i)\n" ,realstart,netbuffer.numtics);
			continue;
		}

		//
		// check for a missed packet
		//
		if (realstart > nettics[netnode])
		{
		// stop processing until the other system resends the missed tics
if (debugfile)
fprintf (debugfile,"missed tics from %i (%i - %i)\n", netnode, realstart, nettics[netnode]);
			remoteresend[netnode] = true;
			continue;
		}

//
// update command store from the packet
//
{
	int             start;

		remoteresend[netnode] = false;

		start = nettics[netnode] - realstart;
		src = &netbuffer.cmds[start];

		while (nettics[netnode] < realend)
		{
			dest = &netcmds[netconsole][nettics[netnode]%BACKUPTICS];
			nettics[netnode]++;
			*dest = *src;
			src++;
		}
	}
}

}
#endif
		/*
=============
=
= NetUpdate
=
= Builds ticcmds for console player
= sends out a packet
=============
*/

		public static int gametime;
		public static void NetUpdate()
		{
#if DOS
	int             nowtime;
	int             newtics;
	int                             i,j;
	int                             realstart;
	int                             gameticdiv;

//
// check time
//
	nowtime = I_GetTime ()/ticdup;
	newtics = nowtime - gametime;
	gametime = nowtime;

	if (newtics <= 0)                       // nothing new to update
		goto listen;

	if (skiptics <= newtics)
	{
		newtics -= skiptics;
		skiptics = 0;
	}
	else
	{
		skiptics -= newtics;
		newtics = 0;
	}


	netbuffer.player = consoleplayer;

//
// build new ticcmds for console player
//
	gameticdiv = gametic/ticdup;
	for (i=0 ; i<newtics ; i++)
	{
		I_StartTic ();
		D_ProcessEvents ();
		if (maketic - gameticdiv >= BACKUPTICS/2-1)
			break;          // can't hold any more
		G_BuildTiccmd (&localcmds[maketic%BACKUPTICS]);
		maketic++;
	}


	if (singletics)
		return;         // singletic update is syncronous

//
// send the packet to the other nodes
//
	for (i=0 ; i<doomcom.numnodes ; i++)
		if (nodeingame[i])
		{
			netbuffer.starttic = realstart = resendto[i];
			netbuffer.numtics = maketic - realstart;
			if (netbuffer.numtics > BACKUPTICS)
				I_Error ("NetUpdate: netbuffer.numtics > BACKUPTICS");

			resendto[i] = maketic - doomcom.extratics;

			for (j=0 ; j< netbuffer.numtics ; j++)
				netbuffer.cmds[j] =
					localcmds[(realstart+j)%BACKUPTICS];

			if (remoteresend[i])
			{
				netbuffer.retransmitfrom = nettics[i];
				HSendPacket (i, NCMD_RETRANSMIT);
			}
			else
			{
				netbuffer.retransmitfrom = 0;
				HSendPacket (i, 0);
			}
		}

//
// listen for other packets
//
listen:

	GetPackets ();
#endif
		}

#if DOS


/*
=====================
=
= CheckAbort
=
=====================
*/

void CheckAbort (void)
{
	event_t *ev;
	int             stoptic;

	stoptic = I_GetTime () + 2;
	while (I_GetTime() < stoptic)
		I_StartTic ();

	I_StartTic ();
	for ( ; eventtail != eventhead
	; eventtail = (++eventtail)&(MAXEVENTS-1) )
	{
		ev = &events[eventtail];
		if (ev.type == ev_keydown && ev.data1 == KEY_ESCAPE)
			I_Error ("Network game synchronization aborted.");
	}
}

/*
=====================
=
= D_ArbitrateNetStart
=
=====================
*/

void D_ArbitrateNetStart (void)
{
	int             i;
	boolean gotinfo[MAXNETNODES];

	autostart = true;
	memset (gotinfo,0,sizeof(gotinfo));

	if (doomcom.consoleplayer)
	{       // listen for setup info from key player
//		mprintf ("listening for network start info...\n");
		while (1)
		{
			CheckAbort ();
			if (!HGetPacket ())
				continue;
			if (netbuffer.checksum & NCMD_SETUP)
			{
				if (netbuffer.player != VERSION)
					I_Error ("Different DOOM versions cannot play a net game!");
				startskill = netbuffer.retransmitfrom & 15;
				deathmatch = (netbuffer.retransmitfrom & 0xc0) >> 6;
				nomonsters = (netbuffer.retransmitfrom & 0x20) > 0;
				respawnparm = (netbuffer.retransmitfrom & 0x10) > 0;
				//startmap = netbuffer.starttic & 0x3f;
				//startepisode = netbuffer.starttic >> 6;
				startmap = netbuffer.starttic&15;
				startepisode = netbuffer.starttic>>4;
				return;
			}
		}
	}
	else
 	{       // key player, send the setup info
//		mprintf ("sending network start info...\n");
		do
		{
			CheckAbort ();
			for (i=0 ; i<doomcom.numnodes ; i++)
			{
				netbuffer.retransmitfrom = startskill;
				if (deathmatch)
					netbuffer.retransmitfrom |= (deathmatch<<6);
				if (nomonsters)
					netbuffer.retransmitfrom |= 0x20;
				if (respawnparm)
					netbuffer.retransmitfrom |= 0x10;
				//netbuffer.starttic = startepisode * 64 + startmap;
				netbuffer.starttic = (startepisode<<4)+startmap;
				netbuffer.player = VERSION;
				netbuffer.numtics = 0;
				HSendPacket (i, NCMD_SETUP);
			}

			for(i = 10 ; i  &&  HGetPacket(); --i)
			{
 if((netbuffer.player&0x7f) < MAXNETNODES)
				gotinfo[netbuffer.player&0x7f] = true;
			}

			for (i=1 ; i<doomcom.numnodes ; i++)
				if (!gotinfo[i])
					break;
		} while (i < doomcom.numnodes);
	}
}

#endif
		/*
===================
=
= D_CheckNetGame
=
= Works out player numbers among the net participants
===================
*/

		public static void D_CheckNetGame()
		{
			int i;

			for (i = 0; i < DoomDef.MAXNETNODES; i++)
			{
				nodeingame[i] = false;
				nettics[i] = 0;
				remoteresend[i] = false;        // set when local needs tics
				resendto[i] = 0;                        // which tic to start sending
			}

			// I_InitNetwork sets doomcom and netgame
#if DOS
	I_InitNetwork ();
	if (doomcom.id != DOOMCOM_ID)
		I_Error ("Doomcom buffer invalid!");
	netbuffer = &doomcom.data;
#endif
			g_game.consoleplayer = g_game.displayplayer = 0;//TODO: doomcom.consoleplayer;
#if DOS
	if (g_game.netgame)
		D_ArbitrateNetStart ();
#endif

			// read values out of doomcom
			ticdup = 1;// doomcom.ticdup;
#if DOS
	maxsend = BACKUPTICS/(2*ticdup)-1;
	if (maxsend<1)
		maxsend = 1;

	for (i=0 ; i<doomcom.numplayers ; i++)
		playeringame[i] = true;
	for (i=0 ; i<doomcom.numnodes ; i++)
		nodeingame[i] = true;
#endif
			//TODO: Remove
			for (i = 0; i < 1; i++)
				g_game.playeringame[i] = true;
			for (i = 0; i < 1; i++)
				nodeingame[i] = true;
			/////////////////////////////////

		}
#if DOS
/*
==================
=
= D_QuitNetGame
=
= Called before quitting to leave a net game without hanging the
= other players
=
==================
*/

void D_QuitNetGame (void)
{
	int             i, j;

	if (debugfile)
		fclose (debugfile);

	if (!netgame || !usergame || consoleplayer == -1 || demoplayback)
		return;

// send a bunch of packets for security
	netbuffer.player = consoleplayer;
	netbuffer.numtics = 0;
	for (i=0 ; i<4 ; i++)
	{
		for (j=1 ; j<doomcom.numnodes ; j++)
			if (nodeingame[j])
				HSendPacket (j, NCMD_EXIT);
		I_WaitVBL (1);
	}
}


#endif
		/*
===============
=
= TryRunTics
=
===============
*/

#if DOS
int     frametics[4], 
int     frameskip[4];
int             oldnettics;
#endif
		public static int frameon;
		static int oldentertics;

		public static void TryRunTics()
		{
			int i;
			int lowtic;
			int entertic;
			int realtics, availabletics;
			int counts;
			int numplaying;

			//
			// get real tics
			//
#if DOS
	entertic = I_GetTime ()/ticdup;
	realtics = entertic - oldentertics;
	oldentertics = entertic;
#endif

			//
			// get available tics
			//
			NetUpdate();
#if DOS

	lowtic = MAXINT;
	numplaying = 0;
	for (i=0 ; i<doomcom.numnodes ; i++)
		if (nodeingame[i])
		{
			numplaying++;
			if (nettics[i] < lowtic)
				lowtic = nettics[i];
		}
	availabletics = lowtic - gametic/ticdup;


//
// decide how many tics to run
//
	if (realtics < availabletics-1)
		counts = realtics+1;
	else if (realtics < availabletics)
		counts = realtics;
	else
		counts = availabletics;
	if (counts < 1)
		counts = 1;
#endif
			counts = 1; // In XNA we always run 1 tick. XNA takes care of the framerate

			frameon++;
#if DOS

if (debugfile)
	fprintf (debugfile,"=======real: %i  avail: %i  game: %i\n",realtics, availabletics,counts);
#endif

			if (!g_game.demoplayback)
			{
				//=============================================================================
				//
				//      ideally nettics[0] should be 1 - 3 tics above lowtic
				//      if we are consistantly slower, speed up time
				//
				for (i = 0; i < DoomDef.MAXPLAYERS; i++)
					if (g_game.playeringame[i])
						break;
				if (g_game.consoleplayer == i)
				{       // the key player does not adapt
				}
				else
				{
#if DOS
			if (nettics[0] <= nettics[nodeforplayer[i]])
			{
				gametime--;
	//                      printf ("-");
			}
			frameskip[frameon&3] = (oldnettics > nettics[nodeforplayer[i]]);
			oldnettics = nettics[0];
			if (frameskip[0] && frameskip[1] && frameskip[2] && frameskip[3])
			{
				skiptics = 1;
	//                      printf ("+");
			}
#endif
				}
				//=============================================================================
			}       // demoplayback

			//
			// wait for new tics if needed
			//
#if DOS
		while (lowtic < gametic/ticdup + counts)
		{

			NetUpdate ();
			lowtic = MAXINT;

			for (i=0 ; i<doomcom.numnodes ; i++)
				if (nodeingame[i] && nettics[i] < lowtic)
					lowtic = nettics[i];

			if (lowtic < gametic/ticdup)
				I_Error ("TryRunTics: lowtic < gametic");

			// don't stay in here forever -- give the menu a chance to work
			if (I_GetTime ()/ticdup - entertic >= 20)
			{
				MN_Ticker ();
				return;
			}
		}
#endif

			//
			// run the count * ticdup dics
			//
			while (counts > 0)
			{
				for (i = 0; i < ticdup; i++)
				{
#if DOS
			if (gametic/ticdup > lowtic)
				I_Error ("gametic>lowtic");
#endif
					if (d_main.advancedemo)
						d_main.D_DoAdvanceDemo();
					mn_menu.MN_Ticker();
					g_game.G_Ticker();
					g_game.gametic++;
#if DOS
			//
			// modify command for duplicated tics
			//
			if (i != ticdup-1)
			{
				ticcmd_t        *cmd;
				int                     buf;
				int                     j;

				buf = (gametic/ticdup)%BACKUPTICS;
				for (j=0 ; j<MAXPLAYERS ; j++)
				{
					cmd = &netcmds[j][buf];
					cmd.chatchar = 0;
					if (cmd.buttons & BT_SPECIAL)
						cmd.buttons = 0;
				}
			}
#endif
				}
				NetUpdate();                                   // check for new console commands

				--counts;
			}
		}
	}
}
