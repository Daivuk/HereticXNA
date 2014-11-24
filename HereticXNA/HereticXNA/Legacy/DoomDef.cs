using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

// DoomDef.h

namespace HereticXNA
{
	public static class DoomDef
	{
		public const uint VERSION = 130;
		public const string VERSION_TEXT = "v1.3";

		//
		// most key data are simple ascii (uppercased)
		//
		public const int KEY_RIGHTARROW = 0xae;
		public const int KEY_LEFTARROW = 0xac;
		public const int KEY_UPARROW = 0xad;
		public const int KEY_DOWNARROW = 0xaf;
		public const int KEY_ESCAPE = 27;
		public const int KEY_ENTER = 13;
		public const int KEY_F1 = (0x80 + 0x3b);
		public const int KEY_F2 = (0x80 + 0x3c);
		public const int KEY_F3 = (0x80 + 0x3d);
		public const int KEY_F4 = (0x80 + 0x3e);
		public const int KEY_F5 = (0x80 + 0x3f);
		public const int KEY_F6 = (0x80 + 0x40);
		public const int KEY_F7 = (0x80 + 0x41);
		public const int KEY_F8 = (0x80 + 0x42);
		public const int KEY_F9 = (0x80 + 0x43);
		public const int KEY_F10 = (0x80 + 0x44);
		public const int KEY_F11 = (0x80 + 0x57);
		public const int KEY_F12 = (0x80 + 0x58);

		public const int KEY_BACKSPACE = 127;
		public const int KEY_PAUSE = 0xff;

		public const int KEY_EQUALS = 0x3d;
		public const int KEY_MINUS = 0x2d;

		public const int KEY_RSHIFT = (0x80 + 0x36);
		public const int KEY_RCTRL = (0x80 + 0x1d);
		public const int KEY_RALT = (0x80 + 0x38);

		public const int KEY_LALT = KEY_RALT;



		public const sbyte MAXCHAR = ((sbyte)0x7f);
		public const short MAXSHORT = ((short)0x7fff);
		public const int MAXINT = ((int)0x7fffffff);	/* max pos 32-bit int */
		public const long MAXLONG = ((long)0x7fffffff);

		public const sbyte MINCHAR = ((sbyte)-128);
		public const short MINSHORT = ((short)-32768);
		public const int MININT = ((int)-2147483648);	/* max negative 32-bit integer */
		public const long MINLONG = ((long)-2147483648);

		public const uint FINEANGLES = 8192;
		public const uint FINEMASK = (FINEANGLES - 1);
		public const uint ANGLETOFINESHIFT = 19;	// 0x100000000 to 0x2000

		public const string SAVEGAMENAME = "hticsav";
		public const string SAVEGAMENAMECD = "c:\\heretic.cd\\hticsav";

		/*
		===============================================================================

								GLOBAL TYPES

		===============================================================================
		*/

		public const int NUMARTIFCTS = 28;
		public const int MAXPLAYERS = 4;
		public const int TICRATE = 35;			// number of tics / second
		public const int TICSPERSEC = 35;

		public const int FRACBITS = 16;
		public const int FRACUNIT = (1 << FRACBITS);
		//typedef int int;

		public const uint ANGLE_1 = 0x01000000;
		public const uint ANGLE_45 = 0x20000000;
		public const uint ANGLE_90 = 0x40000000;
		public const uint ANGLE_180 = 0x80000000;
		public const uint ANGLE_MAX = 0xffffffff;

		public const uint ANG45 = 0x20000000;
		public const uint ANG90 = 0x40000000;
		public const uint ANG180 = 0x80000000;
		public const uint ANG270 = 0xc0000000;

		//typedef unsigned uint;

		public enum skill_t
		{
			sk_baby,
			sk_easy,
			sk_medium,
			sk_hard,
			sk_nightmare
		}

		public enum evtype_t
		{
			ev_keydown,
			ev_keyup,
			ev_mouse,
			ev_joystick
		}

		public class event_t
		{
			public evtype_t type;
			public int data1;		// keys / mouse/joystick buttons
			public int data2;		// mouse/joystick x move
			public int data3;		// mouse/joystick y move
		}

		public class ticcmd_t
		{
			public void set(ticcmd_t other)
			{
				forwardmove = other.forwardmove;
				sidemove = other.sidemove;
				angleturn = other.angleturn;
				angleupdown = other.angleupdown;
				consistancy = other.consistancy;
				chatchar = other.chatchar;
				buttons = other.buttons;
				lookfly = other.lookfly;
				arti = other.arti;
			}

			public sbyte forwardmove;		// *2048 for move
			public sbyte sidemove;			// *2048 for move
			public short angleturn;			// <<16 for angle delta
			public short angleupdown;			// Added for XNA
			public short consistancy;		// checks for net game
			public byte chatchar;
			public byte buttons;
			public byte lookfly;			// look/fly up/down/centering
			public byte arti;				// artitype_t to use
		}

		public const int BT_ATTACK = 1;
		public const int BT_USE = 2;
		public const int BT_CHANGE = 4;			// if true, the next 3 bits hold weapon num
		public const int BT_WEAPONMASK = (8 + 16 + 32);
		public const int BT_WEAPONSHIFT = 3;

		public const int BT_SPECIAL = 128;			// game events, not really buttons
		public const int BTS_SAVEMASK = (4 + 8 + 16);
		public const int BTS_SAVESHIFT = 2;
		public const int BT_SPECIALMASK = 3;
		public const int BTS_PAUSE = 1;			// pause the game
		public const int BTS_SAVEGAME = 2;			// save the game at each console
		// savegame slot numbers occupy the second byte of buttons

		public enum gamestate_t
		{
			GS_LEVEL,
			GS_INTERMISSION,
			GS_FINALE,
			GS_DEMOSCREEN
		}

		public enum gameaction_t
		{
			ga_nothing,
			ga_loadlevel,
			ga_newgame,
			ga_loadgame,
			ga_savegame,
			ga_playdemo,
			ga_completed,
			ga_victory,
			ga_worlddone,
			ga_screenshot
		}

		public enum wipe_t
		{
			wipe_0,
			wipe_1,
			wipe_2,
			wipe_3,
			wipe_4,
			NUMWIPES,
			wipe_random
		}

		/*
		===============================================================================

									MAPOBJ DATA

		===============================================================================
		*/

		// think_t is a function pointer to a routine to handle an actor
		public class think_t_delegate
		{
			public think_t_delegate(object in_obj)
			{
				obj = in_obj;
			}
			public object obj;
			//	public virtual void think_t() { }
			public virtual void function(object obj) { }
		}

		public class thinker_t
		{
			public thinker_t prev, next;
			public think_t_delegate function;
		}

		public class ShadowInfo
		{
			public RenderTargetCube shadowMap;
			mobj_t mo;
			public Vector3 lightPos;
			public bool needUpdate;

			public void Dispose()
			{
				if (shadowMap != null) shadowMap.Dispose();
				mo = null;
				shadowMap = null;
			}

			public ShadowInfo(mobj_t in_mo)
			{
				mo = in_mo;
				UpdateShadow();
			}

			public void UpdateShadow()
			{
				if (shadowMap == null)
				{
					shadowMap = new RenderTargetCube(Game1.instance.GraphicsDevice, 64, false, SurfaceFormat.Rg32, DepthFormat.Depth16);
				}
				needUpdate = false;

				Effect effect = Game1.instance.fxShadowPass;
				effect.CurrentTechnique = effect.Techniques[0];

				// We need to render the whole shabam 6 times
				Matrix projMatrix = Matrix.CreatePerspectiveFieldOfView(
					MathHelper.PiOver2, 1, 1, mo.infol.light.radius);
				Matrix viewMatrix = Matrix.Identity;

				int lump;
				r_local.spritedef_t sprdef;
				r_local.spriteframe_t sprframe;
				sprdef = r_thing.sprites[(int)mo.sprite];
				sprframe = sprdef.spriteframes[mo.frame & DoomDef.FF_FRAMEMASK];
				lump = sprframe.lump[0];
				Texture2D texture = w_wad.W_CacheLumpNum(lump + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Texture2D;
				lightPos = new Vector3(
					mo.x >> DoomDef.FRACBITS,
					mo.y >> DoomDef.FRACBITS,
					((mo.z + r_data.spritetopoffset[lump]) >> DoomDef.FRACBITS) - texture.Height / 2);
				effect.Parameters["lightPos"].SetValue(lightPos);
				effect.Parameters["radius"].SetValue(mo.infol.light.radius);

				for (int i = 0; i < 6; i++)
				{
					// render the scene to all cubemap faces
					CubeMapFace cubeMapFace = (CubeMapFace)i;

					switch (cubeMapFace)
					{
						case CubeMapFace.NegativeX:
							{
								viewMatrix = Matrix.CreateLookAt(lightPos, lightPos + Vector3.Left, Vector3.Up);
								break;
							}
						case CubeMapFace.NegativeY:
							{
								viewMatrix = Matrix.CreateLookAt(lightPos, lightPos + Vector3.Down, Vector3.Forward);
								break;
							}
						case CubeMapFace.NegativeZ:
							{
								viewMatrix = Matrix.CreateLookAt(lightPos, lightPos + Vector3.Backward, Vector3.Up);
								break;
							}
						case CubeMapFace.PositiveX:
							{
								viewMatrix = Matrix.CreateLookAt(lightPos, lightPos + Vector3.Right, Vector3.Up);
								break;
							}
						case CubeMapFace.PositiveY:
							{
								viewMatrix = Matrix.CreateLookAt(lightPos, lightPos + Vector3.Up, Vector3.Backward);
								break;
							}
						case CubeMapFace.PositiveZ:
							{
								viewMatrix = Matrix.CreateLookAt(lightPos, lightPos + Vector3.Forward, Vector3.Up);
								break;
							}
					}

					effect.Parameters["matWorldViewProj"].SetValue(viewMatrix * projMatrix);
					effect.CurrentTechnique.Passes[0].Apply();

					// Set the cubemap render target, using the selected face
					Game1.instance.GraphicsDevice.SetRenderTarget(shadowMap, cubeMapFace);
					Game1.instance.GraphicsDevice.Clear(Color.White);
					Game1.instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

					DrawScene();
				}
			}

			private void DrawScene()
			{
				// Lazy gathering touching sectors
				if (mo.sectorsInRadius == null)
				{
					mo.sectorsInRadius = new List<r_local.sector_t>();
					p_maputl.GatherSectorsInRadius(
						new Vector2(
							mo.x >> DoomDef.FRACBITS,
							mo.y >> DoomDef.FRACBITS),
						mo.subsector.sector,
						mo.infol.light.radius,
						ref mo.sectorsInRadius);
				}

				foreach (r_local.sector_t sec in mo.sectorsInRadius)
				{
					sec.drawFlat();
				}
			}
		}

		public class mobj_t
		{
			public void Dispose()
			{
				if (shadowInfo != null)
				{
					shadowInfo.Dispose();
				}
			}

			public void set(mobj_t other)
			{
				thinker.prev = other.thinker.prev;
				thinker.next = other.thinker.next;
				thinker.function = other.thinker.function;

				x = other.x;
				y = other.y;
				z = other.z;
				snext = other.snext;
				sprev = other.sprev;
				angle = other.angle;
				xangle = other.xangle;
				sprite = other.sprite;
				frame = other.frame;

				bnext = other.bnext;
				bprev = other.bprev;
				subsector = other.subsector;
				floorz = other.floorz;
				ceilingz = other.ceilingz;
				radius = other.radius;
				height = other.height;
				momx = other.momx;
				momy = other.momy;
				momz = other.momz;

				validcount = other.validcount;

				type = other.type;
				infol = other.infol;
				tics = other.tics;
				state = other.state;
				damage = other.damage;
				flags = other.flags;
				flags2 = other.flags2;
				special1 = other.special1;
				special1AsTarget = other.special1AsTarget;
				special2 = other.special2;
				special2AsTarget = other.special2AsTarget;
				health = other.health;
				movedir = other.movedir;
				movecount = other.movecount;
				target = other.target;

				reactiontime = other.reactiontime;

				threshold = other.threshold;

				player = other.player;
				lastlook = other.lastlook;

				spawnpoint.x = other.spawnpoint.x;
				spawnpoint.y = other.spawnpoint.y;
				spawnpoint.angle = other.spawnpoint.angle;
				spawnpoint.type = other.spawnpoint.type;
				spawnpoint.options = other.spawnpoint.options;
			}

			public thinker_t thinker = new thinker_t();			// thinker links

			// info for drawing
			public int x, y, z;
			public mobj_t snext, sprev;		// links in sector (if needed)
			public uint angle;
			public uint xangle;
			public info.spritenum_t sprite;				// used to find patch_t and flip value
			public int frame;				// might be ord with FF_FULLBRIGHT

			// interaction info
			public mobj_t bnext, bprev;		// links in blocks (if needed)
			public r_local.subsector_t subsector;
			public int floorz, ceilingz;	// closest together of contacted secs
			public int radius, height;		// for movement checking
			public int momx, momy, momz;	// momentums

			public int validcount;			// if == validcount, already checked

			public info.mobjtype_t type;
			public info.mobjinfo_t infol;				// &mobjinfo[mobj.type]
			public int tics;				// state tic counter
			public info.state_t state;
			public int damage;			// For missiles
			public int flags;
			public int flags2;			// Heretic flags
			public int special1;		// Special info
			public mobj_t special1AsTarget;		// Special info
			public int special2;		// Special info
			public mobj_t special2AsTarget;
			public int health;
			public int movedir;		// 0-7
			public int movecount;		// when 0, select a new dir
			public mobj_t target;		// thing being chased/attacked (or NULL)
			// also the originator for missiles
			public int reactiontime;	// if non 0, don't attack yet
			// used by player to freeze a bit after
			// teleporting
			public int threshold;		// if >0, the target will be chased
			// no matter what (even if shot)
			public player_t player;		// only valid if type == MT_PLAYER
			public int lastlook;		// player number last looked for

			public DoomData.mapthing_t spawnpoint = new DoomData.mapthing_t();		// for nightmare respawn
			public int rnd = m_misc.P_Random() * 8;
			public List<r_local.sector_t> sectorsInRadius;
			public ShadowInfo shadowInfo;
		}

		// each sector has a degenmobj_t in it's center for sound origin purposes
		public class degenmobj_t
		{
			public thinker_t thinker = new thinker_t();		// not used for anything
			public int x, y, z;
		}

		// Most damage defined using HITDICE
		static public int HITDICE(int a)
		{
			return ((1 + (Game1.random.Next() & 7)) * a);
		}

		//
		// frame flags
		//
		public const int FF_FULLBRIGHT = 0x8000;	// flag in thing.frame
		public const int FF_FRAMEMASK = 0x7fff;

		// --- mobj.flags ---
		public const int DEF_ZERO = 0;

		public const int MF_SPECIAL = 1;			// call P_SpecialThing when touched
		public const int MF_SOLID = 2;
		public const int MF_SHOOTABLE = 4;
		public const int MF_NOSECTOR = 8;			// don't use the sector links
		// (invisible but touchable)
		public const int MF_NOBLOCKMAP = 16;		// don't use the blocklinks
		// (inert but displayable)
		public const int MF_AMBUSH = 32;
		public const int MF_JUSTHIT = 64;		// try to attack right back
		public const int MF_JUSTATTACKED = 128;		// take at least one step before attacking
		public const int MF_SPAWNCEILING = 256;		// hang from ceiling instead of floor
		public const int MF_NOGRAVITY = 512;		// don't apply gravity every tic

		// movement flags
		public const int MF_DROPOFF = 0x400;		// allow jumps from high places
		public const int MF_PICKUP = 0x800;	// for players to pick up items
		public const int MF_NOCLIP = 0x1000;	// player cheat
		public const int MF_SLIDE = 0x2000;	// keep info about sliding along walls
		public const int MF_FLOAT = 0x4000;	// allow moves to any height, no gravity
		public const int MF_TELEPORT = 0x8000;	// don't cross lines or look at heights
		public const int MF_MISSILE = 0x10000;	// don't hit same species, explode on block

		public const int MF_DROPPED = 0x20000;		// dropped by a demon, not level spawned
		public const int MF_SHADOW = 0x40000;		// use fuzzy draw (shadow demons / invis)
		public const int MF_NOBLOOD = 0x80000;		// don't bleed when shot (use puff)
		public const int MF_CORPSE = 0x100000;	// don't stop moving halfway off a step
		public const int MF_INFLOAT = 0x200000;	// floating to a height for a move, don't
		// auto float to target's height

		public const int MF_COUNTKILL = 0x400000;	// count towards intermission kill total
		public const int MF_COUNTITEM = 0x800000;	// count towards intermission item total

		public const int MF_SKULLFLY = 0x1000000;	// skull in flight
		public const int MF_NOTDMATCH = 0x2000000;// don't spawn in death match (key cards)

		public const int MF_TRANSLATION = 0xc000000;// if 0x4 0x8 or 0xc, use a translation
		public const int MF_TRANSSHIFT = 26;// table for player colormaps

		// --- mobj.flags2 ---

		public const int MF2_LOGRAV = 0x00000001;// alternate gravity setting
		public const int MF2_WINDTHRUST = 0x00000002;// gets pushed around by the wind
		// specials
		public const int MF2_FLOORBOUNCE = 0x00000004;	// bounces off the floor
		public const int MF2_THRUGHOST = 0x00000008;// missile will pass through ghosts
		public const int MF2_FLY = 0x00000010;	// fly mode is active
		public const int MF2_FOOTCLIP = 0x00000020;	// if feet are allowed to be clipped
		public const int MF2_SPAWNFLOAT = 0x00000040;	// spawn random float z
		public const int MF2_NOTELEPORT = 0x00000080;	// does not teleport
		public const int MF2_RIP = 0x00000100;	// missile rips through solid
		// targets
		public const int MF2_PUSHABLE = 0x00000200;	// can be pushed by other moving
		// mobjs
		public const int MF2_SLIDE = 0x00000400;	// slides against walls
		public const int MF2_ONMOBJ = 0x00000800;	// mobj is resting on top of another
		// mobj
		public const int MF2_PASSMOBJ = 0x00001000;	// Enable z block checking.  If on,
		// this flag will allow the mobj to
		// pass over/under other mobjs.
		public const int MF2_CANNOTPUSH = 0x00002000;	// cannot push other pushable mobjs
		public const int MF2_FEETARECLIPPED = 0x00004000;	// a mobj's feet are now being cut
		public const int MF2_BOSS = 0x00008000;	// mobj is a major boss
		public const int MF2_FIREDAMAGE = 0x00010000;	// does fire damage
		public const int MF2_NODMGTHRUST = 0x00020000;	// does not thrust target when
		// damaging
		public const int MF2_TELESTOMP = 0x00040000;	// mobj can stomp another
		public const int MF2_FLOATBOB = 0x00080000;	// use float bobbing z movement
		public const int MF2_DONTDRAW = 0X00100000;	// don't generate a vissprite

		//=============================================================================
		public enum playerstate_t
		{
			PST_LIVE,			// playing
			PST_DEAD,			// dead on the ground
			PST_REBORN			// ready to restart
		}

		// psprites are scaled shapes directly on the view screen
		// coordinates are given for a 320*200 view screen
		public enum psprnum_t
		{
			ps_weapon,
			ps_flash,
			NUMPSPRITES
		}

		public class pspdef_t
		{
			public info.state_t state;		// a NULL state means not active
			public int tics;
			public int sx, sy;
		}
		public enum keytype_t
		{
			key_yellow,
			key_green,
			key_blue,
			NUMKEYS
		}

		public enum weapontype_t
		{
			wp_staff,
			wp_goldwand,
			wp_crossbow,
			wp_blaster,
			wp_skullrod,
			wp_phoenixrod,
			wp_mace,
			wp_gauntlets,
			wp_beak,
			NUMWEAPONS,
			wp_nochange
		}

		public const int AMMO_GWND_WIMPY = 10;
		public const int AMMO_GWND_HEFTY = 50;
		public const int AMMO_CBOW_WIMPY = 5;
		public const int AMMO_CBOW_HEFTY = 20;
		public const int AMMO_BLSR_WIMPY = 10;
		public const int AMMO_BLSR_HEFTY = 25;
		public const int AMMO_SKRD_WIMPY = 20;
		public const int AMMO_SKRD_HEFTY = 100;
		public const int AMMO_PHRD_WIMPY = 1;
		public const int AMMO_PHRD_HEFTY = 10;
		public const int AMMO_MACE_WIMPY = 20;
		public const int AMMO_MACE_HEFTY = 100;

		public enum ammotype_t
		{
			am_goldwand,
			am_crossbow,
			am_blaster,
			am_skullrod,
			am_phoenixrod,
			am_mace,
			NUMAMMO,
			am_noammo // staff, gauntlets
		}

		public class weaponinfo_t
		{
			public ammotype_t ammo;
			public int upstate;
			public int downstate;
			public int readystate;
			public int atkstate;
			public int holdatkstate;
			public int flashstate;
		}

		public enum artitype_t
		{
			arti_none,
			arti_invulnerability,
			arti_invisibility,
			arti_health,
			arti_superhealth,
			arti_tomeofpower,
			arti_torch,
			arti_firebomb,
			arti_egg,
			arti_fly,
			arti_teleport,
			NUMARTIFACTS
		}

		public enum powertype_t
		{
			pw_None,
			pw_invulnerability,
			pw_invisibility,
			pw_allmap,
			pw_infrared,
			pw_weaponlevel2,
			pw_flight,
			pw_shield,
			pw_health2,
			NUMPOWERS
		}

#if DOS

//TODO: public const int	INVULNTICS (30*35)
//TODO: public const int	INVISTICS (60*35)
//TODO: public const int	INFRATICS (120*35)
//TODO: public const int	IRONTICS (60*35)
//TODO: public const int WPNLEV2TICS (40*35)
//TODO: public const int FLIGHTTICS (60*35)

//TODO: public const int CHICKENTICS (40*35)
#endif
		public const int MESSAGETICS = (4 * 35);
		public const int BLINKTHRESHOLD = (4 * 32);

		public const int NUMINVENTORYSLOTS = 14;
		public class inventory_t
		{
			public int type;
			public int count;
		}
		/*
		================
		=
		= player_t
		=
		================
		*/

		public class player_t
		{
			public player_t()
			{
				for (int i = 0; i < NUMINVENTORYSLOTS; ++i)
				{
					inventory[i] = new inventory_t();
				}
				for (int i = 0; i < (int)psprnum_t.NUMPSPRITES; ++i)
				{
					psprites[i] = new pspdef_t();
				}
				reset();
			}

			public void reset()
			{
				mo = null;
				playerstate = playerstate_t.PST_LIVE;
				cmd.forwardmove = 0;
				cmd.sidemove = 0;
				cmd.angleturn = 0;
				cmd.angleupdown = 0;
				cmd.consistancy = 0;
				cmd.chatchar = 0;
				cmd.buttons = 0;
				cmd.lookfly = 0;
				cmd.arti = 0;
				viewz = 0;
				viewheight = 0;
				deltaviewheight = 0;
				bob = 0;
				flyheight = 0;
				lookdir = 0;
				centering = false;
				health = 0;
				armorpoints = 0;
				armortype = 0;
				for (int i = 0; i < NUMINVENTORYSLOTS; ++i)
				{
					inventory[i].type = 0;
					inventory[i].count = 0;
				}
				readyArtifact = artitype_t.arti_none;
				artifactCount = 0;
				inventorySlotNum = 0;
				for (int i = 0; i < (int)powertype_t.NUMPOWERS; ++i)
				{
					powers[i] = 0;
				}
				for (int i = 0; i < (int)keytype_t.NUMKEYS; ++i)
				{
					keys[i] = false;
				}
				backpack = false;
				for (int i = 0; i < MAXPLAYERS; ++i)
				{
					frags[i] = 0;
				}
				readyweapon = weapontype_t.wp_staff;
				pendingweapon = weapontype_t.wp_staff;
				for (int i = 0; i < (int)weapontype_t.NUMWEAPONS; ++i)
				{
					weaponowned[i] = false;
				}
				for (int i = 0; i < (int)ammotype_t.NUMAMMO; ++i)
				{
					ammo[i] = 0;
				}
				for (int i = 0; i < (int)ammotype_t.NUMAMMO; ++i)
				{
					maxammo[i] = 0;
				}
				attackdown = 0;
				usedown = 0;
				cheats = 0;
				refire = 0;
				killcount = 0;
				itemcount = 0;
				secretcount = 0;
				message = "";
				messageTics = 0;
				damagecount = 0;
				bonuscount = 0;
				flamecount = 0;
				attacker = null;
				extralight = 0;
				fixedcolormap = 0;
				colormap = 0;
				for (int i = 0; i < (int)psprnum_t.NUMPSPRITES; ++i)
				{
					psprites[i].state = null;
					psprites[i].tics = 0;
					psprites[i].sx = 0;
					psprites[i].sy = 0;
				}
				didsecret = false;
				chickenTics = 0;
				chickenPeck = 0;
				rain1 = null;
				rain2 = null;
			}

			public mobj_t mo;
			public playerstate_t playerstate;
			public ticcmd_t cmd = new ticcmd_t();

			public int viewz;					// focal origin above r.z
			public int viewheight;				// base height above floor for viewz
			public int deltaviewheight;		// squat speed
			public int bob;					// bounded/scaled total momentum

			public int flyheight;
			public int lookdir;
			public bool centering;
			public int health;					// only used between levels, mo.health
			// is used during levels
			public int armorpoints, armortype;	// armor type is 0-2

			public inventory_t[] inventory = new inventory_t[NUMINVENTORYSLOTS];
			public artitype_t readyArtifact;
			public int artifactCount;
			public int inventorySlotNum;
			public int[] powers = new int[(int)powertype_t.NUMPOWERS];
			public bool[] keys = new bool[(int)keytype_t.NUMKEYS];
			public bool backpack;
			public int[] frags = new int[MAXPLAYERS];		// kills of other players
			public weapontype_t readyweapon;
			public weapontype_t pendingweapon;		// wp_nochange if not changing
			public bool[] weaponowned = new bool[(int)weapontype_t.NUMWEAPONS];
			public int[] ammo = new int[(int)ammotype_t.NUMAMMO];
			public int[] maxammo = new int[(int)ammotype_t.NUMAMMO];
			public int attackdown, usedown;	// true if button down last tic
			public int cheats;					// bit flags

			public int refire;					// refired shots are less accurate

			public int killcount, itemcount, secretcount;		// for intermission
			public string message;				// hint messages
			public int messageTics;			// counter for showing messages
			public int damagecount, bonuscount;// for screen flashing
			public int flamecount;				// for flame thrower duration
			public mobj_t attacker;				// who did damage (NULL for floors)
			public int extralight;				// so gun flashes light up areas
			public int fixedcolormap;			// can be set to REDCOLORMAP, etc
			public int colormap;				// 0-3 for which color to draw player
			public pspdef_t[] psprites = new pspdef_t[(int)psprnum_t.NUMPSPRITES];	// view sprites (gun, etc)
			public bool didsecret;				// true if secret level has been done
			public int chickenTics;			// player is a chicken if > 0
			public int chickenPeck;			// chicken peck countdown
			public mobj_t rain1;					// active rain maker 1
			public mobj_t rain2;					// active rain maker 2
		}


		public const int CF_NOCLIP = 1;
		public const int CF_GODMODE = 2;
		public const int CF_NOMOMENTUM = 4; // not really a cheat, just a debug aid


		public const int BACKUPTICS = 12;		// CHANGED FROM 12 !?!?
#if DOS

typedef struct
{
	unsigned	checksum;					// high bit is retransmit request
	byte		retransmitfrom;				// only valid if NCMD_RETRANSMIT
	byte		starttic;
	byte		player, numtics;
	ticcmd_t	cmds[BACKUPTICS];
} doomdata_t;

typedef struct
{
	long	id;
	short	intnum;			// DOOM executes an int to execute commands

// communication between DOOM and the driver
	short	command;		// CMD_SEND or CMD_GET
	short	remotenode;		// dest for send, set by get (-1 = no packet)
	short	datalength;		// bytes in doomdata to be sent

// info common to all nodes
	short	numnodes;		// console is allways node 0
	short	ticdup;			// 1 = no duplication, 2-5 = dup for slow nets
	short	extratics;		// 1 = send a backup tic in every packet
	short	deathmatch;		// 1 = deathmatch
	short	savegame;		// -1 = new game, 0-5 = load savegame
	short	episode;		// 1-3
	short	map;			// 1-9
	short	skill;			// 1-5

// info specific to this node
	short	consoleplayer;
	short	numplayers;
	short	angleoffset;	// 1 = left, 0 = center, -1 = right
	short	drone;			// 1 = drone

// packet data to be sent
	doomdata_t	data;
} doomcom_t;

//TODO: public const int	DOOMCOM_ID		0x12345678l

//TODO: public const int	MAXNETNODES		8			// max computers in a game

//TODO: public const int	CMD_SEND	1
//TODO: public const int	CMD_GET		2
#endif

		public const int SBARHEIGHT = 42;	// status bar height at bottom of screen


		/*
		===============================================================================

							GLOBAL VARIABLES

		===============================================================================
		*/

		public const int TELEFOGHEIGHT = (32 * FRACUNIT);
		public const int MAXEVENTS = 64;
		public const int MAXNETNODES = 8;
#if DOS


//TODO: public const int SAVEGAMESIZE 0x30000
//TODO: public const int SAVESTRINGSIZE 24

#endif

		/*
===============================================================================

					GLOBAL FUNCTIONS

===============================================================================
*/

		//-----------
		//MEMORY ZONE
		//-----------
		// tags < 100 are not overwritten until freed
		// [dsl] Those constants are totally useless in our case
		public const int PU_STATIC = 1;			// static entire execution time
		public const int PU_SOUND = 2;			// static while playing
		public const int PU_MUSIC = 3;			// static while playing
		public const int PU_DAVE = 4;			// anything else Dave wants static
		public const int PU_LEVEL = 50;			// static until level exited
		public const int PU_LEVSPEC = 51;			// a special thinker in a level
		// tags >= 100 are purgable whenever needed
		public const int PU_PURGELEVEL = 100;
		public const int PU_CACHE = 101;

		//TODO:  [dsl] We will not need any of this
		//public class memblock_t
		//{
		//    int                     size;           // including the header and possibly tiny fragments
		//    void            **user;         // NULL if a free block
		//    int                     tag;            // purgelevel
		//    int                     id;                     // should be ZONEID
		//    memblock_t       next;
		//    memblock_t prev;
		//} ;

		//TODO: public const int Z_ChangeTag(p,t) \
		//{ \
		//if (( (memblock_t *)( (byte *)(p) - sizeof(memblock_t))).id!=0x1d4a11) \
		//    I_Error("Z_CT at "__FILE__":%i",__LINE__); \
		//Z_ChangeTag2(p,t); \
		//};

		//-------
		//WADFILE
		//-------
		public class lumpinfo_t
		{
			public string name;
			public BinaryReader handle;
			public int position;
			public int size;
		}

		//---------
		//SYSTEM IO
		//---------
		public static int SCREENWIDTH = 320;
		public static int SCREENHEIGHT = 200;


		//-------
		//REFRESH
		//-------
		// define the different areas for the dirty map
		public const int I_NOUPDATE = 0;
		public const int I_FULLVIEW = 1;
		public const int I_STATBAR = 2;
		public const int I_MESSAGES = 4;
		public const int I_FULLSCRN = 8;

		static public int FixedMul(int a, int b)         //asm
		{
			return (int)(((Int64)a * (Int64)b) >> FRACBITS);
		}
	}
}
