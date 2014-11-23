using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// DStrings.h

namespace HereticXNA
{
	public static class dstring
	{
//---------------------------------------------------------------------------
//
// M_menu.c
//
//---------------------------------------------------------------------------
public const string PRESSKEY ="press a key.";
public const string PRESSYN ="press y or n.";
public const string TXT_PAUSED="PAUSED";
public const string QUITMSG	="are you sure you want to\nquit this great game?";
public const string LOADNET ="you can't do load while in a net game!\n\n" + PRESSKEY;
public const string QLOADNET="you can't quickload during a netgame!\n\n" + PRESSKEY;
public const string QSAVESPOT="you haven't picked a quicksave slot yet!\n\n" + PRESSKEY;
public const string SAVEDEAD ="you can't save if you aren't playing!\n\n" + PRESSKEY;
public const string QSPROMPT ="quicksave over your game named\n\n'" + PRESSYN + "'?\n\n";
public const string QLPROMPT="do you want to quickload the game named\n\n'" + PRESSYN + "'?\n\n";
public const string NEWGAME	="you can't start a new game\nwhile in a network game.\n\n" + PRESSKEY;
public const string NIGHTMARE = "are you sure? this skill level\nisn't even remotely fair.\n\n" + PRESSYN;
public const string SWSTRING="this is the shareware version of doom.\n\nyou need to order the entire trilogy.\n\n" + PRESSKEY;
public const string MSGOFF	="Messages OFF";
public const string MSGON	="Messages ON";
public const string NETEND	="you can't end a netgame!\n\n" + PRESSKEY;
public const string ENDGAME	="are you sure you want to end the game?\n\n" + PRESSYN;
public const string DOSY	="(press y to quit to dos.)";
public const string DETAILHI="High detail";
public const string DETAILLO="Low detail";
public const string GAMMALVL0="Gamma correction OFF";
public const string GAMMALVL1="Gamma correction level 1";
public const string GAMMALVL2="Gamma correction level 2";
public const string GAMMALVL3="Gamma correction level 3";
public const string GAMMALVL4="Gamma correction level 4";
public const string	EMPTYSTRING="empty slot";
//---------------------------------------------------------------------------
//
// P_inter.c
//
//---------------------------------------------------------------------------

// Keys

public const string TXT_GOTBLUEKEY		="BLUE KEY";
public const string TXT_GOTYELLOWKEY	="YELLOW KEY";
public const string TXT_GOTGREENKEY ="GREEN KEY";

// Artifacts

public const string TXT_ARTIHEALTH		="QUARTZ FLASK";
public const string TXT_ARTIFLY			="WINGS OF WRATH";
public const string TXT_ARTIINVULNERABILITY="RING OF INVINCIBILITY";
public const string TXT_ARTITOMEOFPOWER	="TOME OF POWER";
public const string TXT_ARTIINVISIBILITY="SHADOWSPHERE";
public const string TXT_ARTIEGG			="MORPH OVUM";
public const string TXT_ARTISUPERHEALTH	="MYSTIC URN";
public const string TXT_ARTITORCH		="TORCH";
public const string TXT_ARTIFIREBOMB	="TIME BOMB OF THE ANCIENTS";
public const string TXT_ARTITELEPORT	="CHAOS DEVICE";

// Items

public const string TXT_ITEMHEALTH		="CRYSTAL VIAL";
public const string TXT_ITEMBAGOFHOLDING="BAG OF HOLDING";
public const string TXT_ITEMSHIELD1		="SILVER SHIELD";
public const string TXT_ITEMSHIELD2		="ENCHANTED SHIELD";
public const string TXT_ITEMSUPERMAP	="MAP SCROLL";

// Ammo

public const string TXT_AMMOGOLDWAND1	="WAND CRYSTAL";
public const string TXT_AMMOGOLDWAND2	="CRYSTAL GEODE";
public const string TXT_AMMOMACE1		="MACE SPHERES";
public const string TXT_AMMOMACE2		="PILE OF MACE SPHERES";
public const string TXT_AMMOCROSSBOW1	="ETHEREAL ARROWS";
public const string TXT_AMMOCROSSBOW2	="QUIVER OF ETHEREAL ARROWS";
public const string TXT_AMMOBLASTER1	="CLAW ORB";
public const string TXT_AMMOBLASTER2	="ENERGY ORB";
public const string TXT_AMMOSKULLROD1	="LESSER RUNES";
public const string TXT_AMMOSKULLROD2	="GREATER RUNES";
public const string TXT_AMMOPHOENIXROD1	="FLAME ORB";
public const string TXT_AMMOPHOENIXROD2	="INFERNO ORB";

// Weapons

public const string TXT_WPNMACE			="FIREMACE";
public const string TXT_WPNCROSSBOW		="ETHEREAL CROSSBOW";
public const string TXT_WPNBLASTER		="DRAGON CLAW";
public const string TXT_WPNSKULLROD		="HELLSTAFF";
public const string TXT_WPNPHOENIXROD	="PHOENIX ROD";
public const string TXT_WPNGAUNTLETS	="GAUNTLETS OF THE NECROMANCER";

//---------------------------------------------------------------------------
//
// SB_bar.c
//
//---------------------------------------------------------------------------

public const string TXT_CHEATGODON		="GOD MODE ON";
public const string TXT_CHEATGODOFF		="GOD MODE OFF";
public const string TXT_CHEATNOCLIPON	="NO CLIPPING ON";
public const string TXT_CHEATNOCLIPOFF	="NO CLIPPING OFF";
public const string TXT_CHEATWEAPONS	="ALL WEAPONS";
public const string TXT_CHEATFLIGHTON	="FLIGHT ON";
public const string TXT_CHEATFLIGHTOFF	="FLIGHT OFF";
public const string TXT_CHEATPOWERON	="POWER ON";
public const string TXT_CHEATPOWEROFF	="POWER OFF";
public const string TXT_CHEATHEALTH		="FULL HEALTH";
public const string TXT_CHEATKEYS		="ALL KEYS";
public const string TXT_CHEATSOUNDON	="SOUND DEBUG ON";
public const string TXT_CHEATSOUNDOFF	="SOUND DEBUG OFF";
public const string TXT_CHEATTICKERON	="TICKER ON";
public const string TXT_CHEATTICKEROFF	="TICKER OFF";
public const string TXT_CHEATARTIFACTS1	="CHOOSE AN ARTIFACT ( A - J )";
public const string TXT_CHEATARTIFACTS2	="HOW MANY ( 1 - 9 )";
public const string TXT_CHEATARTIFACTS3	="YOU GOT IT";
public const string TXT_CHEATARTIFACTSFAIL="BAD INPUT";
public const string TXT_CHEATWARP		="LEVEL WARP";
public const string TXT_CHEATSCREENSHOT	="SCREENSHOT";
public const string TXT_CHEATCHICKENON	="CHICKEN ON";
public const string TXT_CHEATCHICKENOFF	="CHICKEN OFF";
public const string TXT_CHEATMASSACRE	="MASSACRE";
public const string TXT_CHEATIDDQD		="TRYING TO CHEAT, EH?  NOW YOU DIE!";
public const string TXT_CHEATIDKFA		="CHEATER - YOU DON'T DESERVE WEAPONS";

//---------------------------------------------------------------------------
//
// P_doors.c
//
//---------------------------------------------------------------------------

		public const string TXT_NEEDBLUEKEY ="YOU NEED A BLUE KEY TO OPEN THIS DOOR";
public const string TXT_NEEDGREENKEY ="YOU NEED A GREEN KEY TO OPEN THIS DOOR";
public const string TXT_NEEDYELLOWKEY ="YOU NEED A YELLOW KEY TO OPEN THIS DOOR";

//---------------------------------------------------------------------------
//
// G_game.c
//
//---------------------------------------------------------------------------

public const string TXT_GAMESAVED		="GAME SAVED";

//---------------------------------------------------------------------------
//
// HU_stuff.c
//
//---------------------------------------------------------------------------

public const string HUSTR_E1M1="E1M1: Hangar";
public const string HUSTR_E1M2="E1M2: Nuclear Plant";
public const string HUSTR_E1M3="E1M3: Toxin Refinery";
public const string HUSTR_E1M4="E1M4: Command Control";
public const string HUSTR_E1M5="E1M5: Phobos Lab";
public const string HUSTR_E1M6="E1M6: Central Processing";
public const string HUSTR_E1M7="E1M7: Computer Station";
public const string HUSTR_E1M8="E1M8: Phobos Anomaly";
public const string HUSTR_E1M9="E1M9: Military Base";

public const string HUSTR_E2M1="E2M1: Deimos Anomaly";
public const string HUSTR_E2M2="E2M2: Containment Area";
public const string HUSTR_E2M3="E2M3: Refinery";
public const string HUSTR_E2M4="E2M4: Deimos Lab";
public const string HUSTR_E2M5="E2M5: Command Center";
public const string HUSTR_E2M6="E2M6: Halls of the Damned";
public const string HUSTR_E2M7="E2M7: Spawning Vats";
public const string HUSTR_E2M8="E2M8: Tower of Babel";
public const string HUSTR_E2M9="E2M9: Fortress of Mystery";

public const string HUSTR_E3M1="E3M1: Hell Keep";
public const string HUSTR_E3M2="E3M2: Slough of Despair";
public const string HUSTR_E3M3="E3M3: Pandemonium";
public const string HUSTR_E3M4="E3M4: House of Pain";
public const string HUSTR_E3M5="E3M5: Unholy Cathedral";
public const string HUSTR_E3M6="E3M6: Mt. Erebus";
public const string HUSTR_E3M7="E3M7: Limbo";
public const string HUSTR_E3M8="E3M8: Dis";
public const string HUSTR_E3M9="E3M9: Warrens";

public const string HUSTR_CHATMACRO1 ="I'm ready to kick butt!";
public const string HUSTR_CHATMACRO2	="I'm OK.";
public const string HUSTR_CHATMACRO3	="I'm not looking too good!";
public const string HUSTR_CHATMACRO4	="Help!";
public const string HUSTR_CHATMACRO5	="You suck!";
public const string HUSTR_CHATMACRO6	="Next time, scumbag...";
public const string HUSTR_CHATMACRO7	="Come here!";
public const string HUSTR_CHATMACRO8	="I'll take care of it.";
public const string HUSTR_CHATMACRO9	="Yes";
public const string HUSTR_CHATMACRO0	="No";

public const string HUSTR_TALKTOSELF1="You mumble to yourself";
public const string HUSTR_TALKTOSELF2="Who's there?";
public const string HUSTR_TALKTOSELF3="You scare yourself";
public const string HUSTR_TALKTOSELF4="You start to rave";
public const string HUSTR_TALKTOSELF5="You've lost it...";

public const string HUSTR_MESSAGESENT="[Message Sent]";

// The following should NOT be changed unless it seems
// just AWFULLY necessary

public const string HUSTR_PLRGREEN="Green:=";
public const string HUSTR_PLRINDIGO="Indigo:=";
public const string HUSTR_PLRBROWN="Brown:=";
public const string HUSTR_PLRRED	="Red:=";

public const string HUSTR_KEYGREEN	="g";
public const string HUSTR_KEYINDIGO=	"i";
public const string HUSTR_KEYBROWN	="b";
public const string HUSTR_KEYRED	=	"r";

//---------------------------------------------------------------------------
//
// AM_map.c
//
//---------------------------------------------------------------------------

public const string AMSTR_FOLLOWON	="FOLLOW MODE ON";
public const string AMSTR_FOLLOWOFF	="FOLLOW MODE OFF";

public const string AMSTR_GRIDON	="Grid ON";
public const string AMSTR_GRIDOFF	="Grid OFF";

public const string AMSTR_MARKEDSPOT="Marked Spot";
public const string AMSTR_MARKSCLEARED="All Marks Cleared";

//---------------------------------------------------------------------------
//
// ST_stuff.c
//
//---------------------------------------------------------------------------

public const string STSTR_DQDON		="Degreelessness Mode On";
public const string STSTR_DQDOFF	="Degreelessness Mode Off";

public const string STSTR_KFAADDED	="Very Happy Ammo Added";

public const string STSTR_NCON		="No Clipping Mode ON";
public const string STSTR_NCOFF		="No Clipping Mode OFF";

public const string STSTR_BEHOLD	="inVuln, Str, Inviso, Rad, Allmap, or Lite-amp";
public const string STSTR_BEHOLDX	="Power-up Toggled";

public const string STSTR_CHOPPERS	="... doesn't suck - GM";
public const string STSTR_CLEV		="Changing Level...";

//---------------------------------------------------------------------------
//
// F_finale.c
//
//---------------------------------------------------------------------------

public const string E1TEXT="with the destruction of the iron\nliches and their minions, the last\nof the undead are cleared from this\nplane of existence.\n\nthose creatures had to come from\nsomewhere, though, and you have the\nsneaky suspicion that the fiery\nportal of hell's maw opens onto\ntheir home dimension.\n\nto make sure that more undead\n(or even worse things) don't come\nthrough, you'll have to seal hell's\nmaw from the other side. of course\nthis means you may get stuck in a\nvery unfriendly world, but no one\never said being a Heretic was easy!";

public const string E2TEXT="the mighty maulotaurs have proved\nto be no match for you, and as\ntheir steaming corpses slide to the\nground you feel a sense of grim\nsatisfaction that they have been\ndestroyed.\n\nthe gateways which they guarded\nhave opened, revealing what you\nhope is the way home. but as you\nstep through, mocking laughter\nrings in your ears.\n\nwas some other force controlling\nthe maulotaurs? could there be even\nmore horrific beings through this\ngate? the sweep of a crystal dome\noverhead where the sky should be is\ncertainly not a good sign....";

public const string E3TEXT="the death of d'sparil has loosed\nthe magical bonds holding his\ncreatures on this plane, their\ndying screams overwhelming his own\ncries of agony.\n\nyour oath of vengeance fulfilled,\nyou enter the portal to your own\nworld, mere moments before the dome\nshatters into a million pieces.\n\nbut if d'sparil's power is broken\nforever, why don't you feel safe?\nwas it that last shout just before\nhis death, the one that sounded\nlike a curse? or a summoning? you\ncan't really be sure, but it might\njust have been a scream.\n\nthen again, what about the other\nserpent riders?";

public const string E4TEXT	="you thought you would return to your\nown world after d'sparil died, but\nhis final act banished you to his\nown plane. here you entered the\nshattered remnants of lands\nconquered by d'sparil. you defeated\nthe last guardians of these lands,\nbut now you stand before the gates\nto d'sparil's stronghold. until this\nmoment you had no doubts about your\nability to face anything you might\nencounter, but beyond this portal\nlies the very heart of the evil\nwhich invaded your world. d'sparil\nmight be dead, but the pit where he\nwas spawned remains. now you must\nenter that pit in the hopes of\nfinding a way out. and somewhere,\nin the darkest corner of d'sparil's\ndemesne, his personal bodyguards\nawait your arrival ...";

public const string E5TEXT	="as the final maulotaur bellows his\ndeath-agony, you realize that you\nhave never come so close to your own\ndestruction. not even the fight with\nd'sparil and his disciples had been\nthis desperate. grimly you stare at\nthe gates which open before you,\nwondering if they lead home, or if\nthey open onto some undreamed-of\nhorror. you find yourself wondering\nif you have the strength to go on,\nif nothing but death and pain await\nyou. but what else can you do, if\nthe will to fight is gone? can you\nforce yourself to continue in the\nface of such despair? do you have\nthe courage? you find, in the end,\nthat it is not within you to\nsurrender without a fight. eyes\nwide, you go to meet your fate.";

	}
}
