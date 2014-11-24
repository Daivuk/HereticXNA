using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HereticXNA
{
	public static class p_switch
	{

		//==================================================================
		//
		//	CHANGE THE TEXTURE OF A WALL SWITCH TO ITS OPPOSITE
		//
		//==================================================================
		public static p_spec.switchlist_t[] alphSwitchList = new p_spec.switchlist_t[]
{
	new p_spec.switchlist_t {name1 = "SW1OFF",		name2="SW1ON",episode=	1},
	new p_spec.switchlist_t{name1 = "SW2OFF",	name2=	"SW2ON",	episode=1},

	new p_spec.switchlist_t{name1 = "",		name2=	"",	episode=	0}
};

		public static int[] switchlist = new int[p_spec.MAXSWITCHES * 2];
		public static int numswitches;
		public static p_spec.button_t[] buttonlist = new p_spec.button_t[p_spec.MAXBUTTONS] {
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t(),
			new p_spec.button_t()
		};

		/*
		===============
		=
		= P_InitSwitchList
		=
		= Only called at game initialization
		=
		===============
		*/

		public static void P_InitSwitchList()
		{
			int i;
			int index;
			int episode;

			episode = 1;
			if (!d_main.shareware)
				episode = 2;

			for (index = 0, i = 0; i < p_spec.MAXSWITCHES; i++)
			{
				if (alphSwitchList[i].episode == 0)
				{
					numswitches = index / 2;
					switchlist[index] = -1;
					break;
				}

				if (alphSwitchList[i].episode <= episode)
				{
					switchlist[index++] = r_data.R_TextureNumForName(alphSwitchList[i].name1);
					switchlist[index++] = r_data.R_TextureNumForName(alphSwitchList[i].name2);
				}
			}
		}

		//==================================================================
		//
		//	Start a button counting down till it turns off.
		//
		//==================================================================
		public static void P_StartButton(r_local.line_t line, p_spec.bwhere_e w, int texture, int time)
		{
			int i;

			for (i = 0; i < p_spec.MAXBUTTONS; i++)
				if (buttonlist[i].btimer == 0)
				{
					buttonlist[i].line = line;
					buttonlist[i].where = w;
					buttonlist[i].btexture = texture;
					buttonlist[i].btimer = time;
					buttonlist[i].soundorg = new DoomDef.mobj_t();
					buttonlist[i].soundorg.x = line.frontsector.soundorg.x;
					buttonlist[i].soundorg.y = line.frontsector.soundorg.y;
					buttonlist[i].soundorg.z = line.frontsector.soundorg.z;
					return;
				}

			i_ibm.I_Error("P_StartButton: no button slots left!");
		}

		//==================================================================
		//
		//	Function that changes wall texture.
		//	Tell it if switch is ok to use again (1=yes, it's a button).
		//
		//==================================================================
		public static void P_ChangeSwitchTexture(r_local.line_t line, int useAgain)
		{
			int texTop;
			int texMid;
			int texBot;
			int i;
			int sound;

			if (useAgain == 0)
				line.special = 0;

			texTop = p_setup.sides[line.sidenum[0]].toptexture;
			texMid = p_setup.sides[line.sidenum[0]].midtexture;
			texBot = p_setup.sides[line.sidenum[0]].bottomtexture;

			sound = (int)sounds.sfxenum_t.sfx_switch;

			for (i = 0; i < numswitches * 2; i++)
				if (switchlist[i] == texTop)
				{
					i_ibm.S_StartSound(buttonlist[0].soundorg, sound);
					p_setup.sides[line.sidenum[0]].toptexture = (short)p_switch.switchlist[i ^ 1];
					p_setup.sides[line.sidenum[0]].sector.invalidate(false);
					if (useAgain != 0)
						p_switch.P_StartButton(line, p_spec.bwhere_e.top, switchlist[i], p_spec.BUTTONTIME);
					return;
				}
				else
					if (switchlist[i] == texMid)
					{
						i_ibm.S_StartSound(buttonlist[0].soundorg, sound);
						p_setup.sides[line.sidenum[0]].midtexture = (short)p_switch.switchlist[i ^ 1];
						p_setup.sides[line.sidenum[0]].sector.invalidate(false);
						if (useAgain != 0)
							p_switch.P_StartButton(line, p_spec.bwhere_e.middle, switchlist[i], p_spec.BUTTONTIME);
						return;
					}
					else
						if (switchlist[i] == texBot)
						{
							i_ibm.S_StartSound(buttonlist[0].soundorg, sound);
							p_setup.sides[line.sidenum[0]].bottomtexture = (short)p_switch.switchlist[i ^ 1];
							p_setup.sides[line.sidenum[0]].sector.invalidate(false);
							if (useAgain != 0)
								p_switch.P_StartButton(line, p_spec.bwhere_e.bottom, switchlist[i], p_spec.BUTTONTIME);
							return;
						}
		}

		/*
==============================================================================
=
= P_UseSpecialLine
=
= Called when a thing uses a special line
= Only the front sides of lines are usable
===============================================================================
*/

		public static bool P_UseSpecialLine(DoomDef.mobj_t thing, r_local.line_t line)
		{
			//
			//	Switches that other things can activate
			//
			if (thing.player == null)
			{
				if ((line.flags & DoomData.ML_SECRET) != 0)
					return false;		// never open secret doors
				switch (line.special)
				{
					case 1:		// MANUAL DOOR RAISE
					case 32:	// MANUAL BLUE
					case 33:	// MANUAL RED
					case 34:	// MANUAL YELLOW
						break;
					default:
						return false;
				}
			}

			//
			// do something
			//	
			switch (line.special)
			{
				//===============================================
				//	MANUALS
				//===============================================
				case 1:			// Vertical Door
				case 26:		// Blue Door/Locked
				case 27:		// Yellow Door /Locked
				case 28:		// Red Door /Locked

				case 31:		// Manual door open
				case 32:		// Blue locked door open
				case 33:		// Red locked door open
				case 34:		// Yellow locked door open
					p_doors.EV_VerticalDoor(line, thing);
					break;
				//===============================================
				//	SWITCHES
				//===============================================
				case 7: // Switch_Build_Stairs (8 pixel steps)
					if (p_floor.EV_BuildStairs(line, 8 * DoomDef.FRACUNIT) != 0)
					{
						P_ChangeSwitchTexture(line, 0);
					}
					break;
				case 107: // Switch_Build_Stairs_16 (16 pixel steps)
					if (p_floor.EV_BuildStairs(line, 16 * DoomDef.FRACUNIT) != 0)
					{
						P_ChangeSwitchTexture(line, 0);
					}
					break;
				case 9:			// Change Donut
					//if (EV_DoDonut(line))
					//    P_ChangeSwitchTexture(line, 0);
					break;
				case 11:		// Exit level
					g_game.G_ExitLevel();
					P_ChangeSwitchTexture(line, 0);
					break;
				case 14:		// Raise Floor 32 and change texture
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseAndChange, 32) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 15:		// Raise Floor 24 and change texture
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseAndChange, 24) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 18:		// Raise Floor to next highest floor
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.raiseFloorToNearest) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 20:		// Raise Plat next highest floor and change texture
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseToNearestAndChange, 0) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 21:		// PlatDownWaitUpStay
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.downWaitUpStay, 0) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 23:		// Lower Floor to Lowest
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.lowerFloorToLowest) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 29:		// Raise Door
					if (p_doors.EV_DoDoor(line, p_spec.vldoor_e.normal, p_spec.VDOORSPEED) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 41:		// Lower Ceiling to Floor
					if (p_ceilng.EV_DoCeiling(line, p_spec.ceiling_e.lowerToFloor) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 71:		// Turbo Lower Floor
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.turboLower) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 49:		// Lower Ceiling And Crush
					if (p_ceilng.EV_DoCeiling(line, p_spec.ceiling_e.lowerAndCrush) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 50:		// Close Door
					if (p_doors.EV_DoDoor(line, p_spec.vldoor_e.close, p_spec.VDOORSPEED) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 51:		// Secret EXIT
					//G_SecretExitLevel();
					//P_ChangeSwitchTexture(line, 0);
					break;
				case 55:		// Raise Floor Crush
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.raiseFloorCrush) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 101:		// Raise Floor
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.raiseFloor) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				case 102:		// Lower Floor to Surrounding floor height
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.lowerFloor) != 0)
						P_ChangeSwitchTexture(line, 0);
					break;
				case 103:		// Open Door
					if (p_doors.EV_DoDoor(line, p_spec.vldoor_e.open, p_spec.VDOORSPEED) != 0)
					    P_ChangeSwitchTexture(line, 0);
					break;
				//===============================================
				//	BUTTONS
				//===============================================
				case 42:		// Close Door
					if (p_doors.EV_DoDoor(line, p_spec.vldoor_e.close, p_spec.VDOORSPEED) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 43:		// Lower Ceiling to Floor
					if (p_ceilng.EV_DoCeiling(line, p_spec.ceiling_e.lowerToFloor) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 45:		// Lower Floor to Surrounding floor height
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.lowerFloor) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 60:		// Lower Floor to Lowest
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.lowerFloorToLowest) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 61:		// Open Door
					if (p_doors.EV_DoDoor(line, p_spec.vldoor_e.open, p_spec.VDOORSPEED) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 62:		// PlatDownWaitUpStay
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.downWaitUpStay, 1) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 63:		// Raise Door
					if (p_doors.EV_DoDoor(line, p_spec.vldoor_e.normal, p_spec.VDOORSPEED) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 64:		// Raise Floor to ceiling
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.raiseFloor) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 66:		// Raise Floor 24 and change texture
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseAndChange, 24) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 67:		// Raise Floor 32 and change texture
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseAndChange, 32) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 65:		// Raise Floor Crush
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.raiseFloorCrush) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 68:		// Raise Plat to next highest floor and change texture
					if (p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseToNearestAndChange, 0) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 69:		// Raise Floor to next highest floor
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.raiseFloorToNearest) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
				case 70:		// Turbo Lower Floor
					if (p_floor.EV_DoFloor(line, p_spec.floor_e.turboLower) != 0)
					    P_ChangeSwitchTexture(line, 1);
					break;
			}

			return true;
		}
	}
}
