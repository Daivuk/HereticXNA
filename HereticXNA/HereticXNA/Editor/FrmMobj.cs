using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HereticXNA
{
	public partial class FrmMobj : Form
	{
		public FrmMobj()
		{
			InitializeComponent();
		}

		info.mobjinfo_t getMoType()
		{
			if (Game1.instance.selectedMob.Target == null) return null;
			return info.mobjinfo[(int)(Game1.instance.selectedMob.Target as DoomDef.mobj_t).type];
		}

		internal void fillUIWithMob(DoomDef.mobj_t mo)
		{
			lblMobType.Text = mo.type.ToString();
			int lump;
			r_local.spritedef_t sprdef;
			r_local.spriteframe_t sprframe;
			info.mobjinfo_t mobT = info.mobjinfo[(int)mo.type];
			sprdef = r_thing.sprites[(int)mo.sprite];
			sprframe = sprdef.spriteframes[mo.frame & DoomDef.FF_FRAMEMASK];
			lump = sprframe.lump[0];
			Microsoft.Xna.Framework.Graphics.Texture2D texture = w_wad.W_CacheLumpNum(lump + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Microsoft.Xna.Framework.Graphics.Texture2D;

			byte[] pixels = new byte[texture.Width * texture.Height * 4];
			texture.GetData<byte>(pixels);

			for (int i = 0; i < texture.Width * texture.Height; ++i)
			{
				byte tmp = pixels[i * 4 + 0];
				pixels[i * 4 + 0] = pixels[i * 4 + 2];
				pixels[i * 4 + 2] = tmp;
			}

			Bitmap bmp = new Bitmap(texture.Width, texture.Height, texture.Width * 4, PixelFormat.Format32bppArgb,
                        GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());
			picMobSprite.Image = bmp;

			if (mobT.light == null)
			{
				btnRemoveColor.Text = "Add Light";
			}
			else
			{
				btnRemoveColor.Text = "Remove Light";
				sldRadius.Value = (int)mobT.light.radius;
				sldHue.Value = mobT.light.hue;
				sldSaturation.Value = mobT.light.saturation;
				txtMultiplier.Text = mobT.light.brightness.ToString();
				chkCastShadow.Checked = mobT.light.castShadow;
				cboLightType.SelectedIndex = mobT.light.type;
			}

			sldBottomIllumination.Value = (int)(mobT.selfIllumB * 100.0f);
			sldTopIllumination.Value = (int)(mobT.selfIllumT * 100.0f);
		}

		private void btnRemoveColor_Click(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;

			if (mobT.light == null)
			{
				mobT.light = new info.mobjlightinfo_t();
				mobT.light.radius = 128;
				mobT.light.color = new Microsoft.Xna.Framework.Vector4(1, 1, 1, 1);
				mobT.light.saturation = 0;
				mobT.light.hue = 0;
				mobT.light.brightness = 1;
				mobT.light.makeColor();
			}
			else
			{
				mobT.light = null;
				mobT.selfIllumT = 0;
				mobT.selfIllumB = 0;
			}
			fillUIWithMob(Game1.instance.selectedMob.Target as DoomDef.mobj_t);
			Game1.instance.NeedSave = true;
		}

		private void txtMultiplier_TextChanged(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;
			if (mobT.light == null) return;

			float ret = 0;
			if (float.TryParse(txtMultiplier.Text, out ret))
			{
				mobT.light.brightness = ret;
				mobT.light.makeColor();
				Game1.instance.NeedSave = true;
			}
		}

		private void sldTopIllumination_Scroll(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;

			mobT.selfIllumT = (float)sldTopIllumination.Value / 100.0f;
			Game1.instance.NeedSave = true;
		}

		private void sldBottomIllumination_Scroll(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;

			mobT.selfIllumB = (float)sldBottomIllumination.Value / 100.0f;
			Game1.instance.NeedSave = true;
		}

		private void sldRadius_Scroll(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;
			if (mobT.light == null) return;

			mobT.light.radius = (float)sldRadius.Value;
			mobT.light.makeColor();
			Game1.instance.NeedSave = true;

			DoomDef.mobj_t selectedMo = Game1.instance.selectedMob.Target as DoomDef.mobj_t;
			if (selectedMo == null) return;
			for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
			{
				if (think == null) break;
				if (think.function == null) continue;
				DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
				if (mo == null) continue;
				if (mo.shadowInfo == null) continue;
				if (mo.type != selectedMo.type) continue;
				mo.shadowInfo.needUpdate = true;
			}
		}

		private void sldHue_Scroll(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;
			if (mobT.light == null) return;

			mobT.light.hue = sldHue.Value;
			mobT.light.makeColor();
			Game1.instance.NeedSave = true;
		}

		private void sldSaturation_Scroll(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;
			if (mobT.light == null) return;

			mobT.light.saturation = sldSaturation.Value;
			mobT.light.makeColor();
			Game1.instance.NeedSave = true;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;
			if (mobT.light == null) return;

			mobT.light.type = cboLightType.SelectedIndex;
			mobT.light.makeColor();
			Game1.instance.NeedSave = true;
		}

		private void chkCastShadow_CheckedChanged(object sender, EventArgs e)
		{
			info.mobjinfo_t mobT = getMoType();
			if (mobT == null) return;
			if (mobT.light == null) return;

			mobT.light.castShadow = chkCastShadow.Checked;
			mobT.light.makeColor();
			Game1.instance.NeedSave = true;

			DoomDef.mobj_t selectedMo = Game1.instance.selectedMob.Target as DoomDef.mobj_t;
			if (selectedMo == null) return;
			for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
			{
				if (think == null) break;
				if (think.function == null) continue;
				DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
				if (mo == null) continue;
				if (mo.shadowInfo == null) continue;
				if (mo.type != selectedMo.type) continue;
				if (!mobT.light.castShadow)
				{
					mo.shadowInfo.Dispose();
					mo.shadowInfo = null;
				}
			}
		}
	}
}
