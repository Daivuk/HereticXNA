using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HereticXNA
{
	public partial class FrmFloor : Form
	{
		public bool useFloor = true;

		public FrmFloor()
		{
			InitializeComponent();
		}

		internal void fillUIWithSector(r_local.sector_t selectedSector)
		{
			if (selectedSector == null) return;
			if (Game1.instance.flatSelfIllumById.ContainsKey(selectedSector.floorpic))
				sldSelfIllumination.Value = (int)(Game1.instance.flatSelfIllumById[selectedSector.floorpic] * 100.0f);
			else
				sldSelfIllumination.Value = 0;
			if (useFloor)
			{
				if (Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic))
				{
					btnAddLight.Text = "Remove Light";
					Game1.Floorlightinfo lightInfo = Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic];
					sldDistance.Value = (int)lightInfo.distance;
					sldHue.Value = lightInfo.hue;
					sldSaturation.Value = lightInfo.saturation;
					sldBrightness.Value = (int)(lightInfo.brightness * 100.0f);
					sldSpread.Value = (int)(lightInfo.spread * 10.0f);
				}
				else
				{
					btnAddLight.Text = "Add Light";
				}
			}
			else
			{
				if (Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic))
				{
					btnAddLight.Text = "Remove Light";
					Game1.Floorlightinfo lightInfo = Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic];
					sldDistance.Value = (int)lightInfo.distance;
					sldHue.Value = lightInfo.hue;
					sldSaturation.Value = lightInfo.saturation;
					sldBrightness.Value = (int)(lightInfo.brightness * 100.0f);
					sldSpread.Value = (int)(lightInfo.spread * 10.0f);
				}
				else
				{
					btnAddLight.Text = "Add Light";
				}
			}
		}

		private void sldSelfIllumination_Scroll(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;
			if (useFloor)
			{
				if (sldSelfIllumination.Value == 0)
				{
					if (Game1.instance.flatSelfIllumById.ContainsKey(Game1.instance.selectedSector.floorpic))
					{
						Game1.instance.flatSelfIllumById.Remove(Game1.instance.selectedSector.floorpic);
						Game1.instance.NeedSave = true;
					}
				}
				else
				{
					Game1.instance.flatSelfIllumById[Game1.instance.selectedSector.floorpic] = (float)sldSelfIllumination.Value / 100.0f;
					Game1.instance.NeedSave = true;
				}
			}
			else
			{
				if (sldSelfIllumination.Value == 0)
				{
					if (Game1.instance.flatSelfIllumById.ContainsKey(Game1.instance.selectedSector.ceilingpic))
					{
						Game1.instance.flatSelfIllumById.Remove(Game1.instance.selectedSector.ceilingpic);
						Game1.instance.NeedSave = true;
					}
				}
				else
				{
					Game1.instance.flatSelfIllumById[Game1.instance.selectedSector.ceilingpic] = (float)sldSelfIllumination.Value / 100.0f;
					Game1.instance.NeedSave = true;
				}
			}
		}

		private void btnAddLight_Click(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;
			if (useFloor)
			{
				if (Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic))
				{
					Game1.instance.flatLightInfoById.Remove(Game1.instance.selectedSector.floorpic);
					foreach (r_local.sector_t sector in p_setup.sectors)
					{
						if (sector.floorpic == Game1.instance.selectedSector.floorpic)
						{
							if (sector.lightData != null)
							{
								sector.lightData.Dispose();
								sector.lightData = null;
							}
						}
					}
					Game1.instance.NeedSave = true;
				}
				else
				{
					Game1.Floorlightinfo lightInfo = new Game1.Floorlightinfo();
					Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic] = lightInfo;
					Game1.instance.NeedSave = true;
				}
			}
			else
			{
				if (Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic))
				{
					Game1.instance.flatCLightInfoById.Remove(Game1.instance.selectedSector.ceilingpic);
					foreach (r_local.sector_t sector in p_setup.sectors)
					{
						if (sector.ceilingpic == Game1.instance.selectedSector.ceilingpic)
						{
							if (sector.lightDataC != null)
							{
								sector.lightDataC.Dispose();
								sector.lightDataC = null;
							}
						}
					}
					Game1.instance.NeedSave = true;
				}
				else
				{
					Game1.Floorlightinfo lightInfo = new Game1.Floorlightinfo();
					Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic] = lightInfo;
					Game1.instance.NeedSave = true;
				}
			}

			fillUIWithSector(Game1.instance.selectedSector);
		}

		private void sldDistance_Scroll(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;

			if (useFloor)
			{
				if (!Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic];
				lightInfo.distance = (float)sldDistance.Value;
				Game1.instance.NeedSave = true;
			}
			else
			{
				if (!Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic];
				lightInfo.distance = (float)sldDistance.Value;
				Game1.instance.NeedSave = true;
			}
		}

		private void sldHue_Scroll(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;

			if (useFloor)
			{
				if (!Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic];
				lightInfo.hue = sldHue.Value;
				lightInfo.makeColor();
				Game1.instance.NeedSave = true;
			}
			else
			{
				if (!Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic];
				lightInfo.hue = sldHue.Value;
				lightInfo.makeColor();
				Game1.instance.NeedSave = true;
			}
		}

		private void sldSaturation_Scroll(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;

			if (useFloor)
			{
				if (!Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic];
				lightInfo.saturation = sldSaturation.Value;
				lightInfo.makeColor();
				Game1.instance.NeedSave = true;
			}
			else
			{
				if (!Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic];
				lightInfo.saturation = sldSaturation.Value;
				lightInfo.makeColor();
				Game1.instance.NeedSave = true;
			}
		}

		private void sldSpread_Scroll(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;

			if (useFloor)
			{
				if (!Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic];
				lightInfo.spread = (float)sldSpread.Value / 10.0f;

				// This causes all sector with this floorpic to invalidate
				foreach (r_local.sector_t sector in p_setup.sectors)
				{
					if (sector.floorpic == Game1.instance.selectedSector.floorpic)
					{
						if (sector.lightData != null)
						{
							sector.lightData.Dispose();
							sector.lightData = null;
						}
					}
				}
				Game1.instance.NeedSave = true;
			}
			else
			{
				if (!Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic];
				lightInfo.spread = (float)sldSpread.Value / 10.0f;

				// This causes all sector with this floorpic to invalidate
				foreach (r_local.sector_t sector in p_setup.sectors)
				{
					if (sector.ceilingpic == Game1.instance.selectedSector.ceilingpic)
					{
						if (sector.lightData != null)
						{
							sector.lightData.Dispose();
							sector.lightData = null;
						}
					}
				}
				Game1.instance.NeedSave = true;
			}
		}

		private void sldBrightness_Scroll(object sender, EventArgs e)
		{
			if (Game1.instance.selectedSector == null) return;

			if (useFloor)
			{
				if (!Game1.instance.flatLightInfoById.ContainsKey(Game1.instance.selectedSector.floorpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatLightInfoById[Game1.instance.selectedSector.floorpic];
				lightInfo.brightness = (float)sldBrightness.Value / 100.0f;
				lightInfo.makeColor();
				Game1.instance.NeedSave = true;
			}
			else
			{
				if (!Game1.instance.flatCLightInfoById.ContainsKey(Game1.instance.selectedSector.ceilingpic)) return;

				Game1.Floorlightinfo lightInfo = Game1.instance.flatCLightInfoById[Game1.instance.selectedSector.ceilingpic];
				lightInfo.brightness = (float)sldBrightness.Value / 100.0f;
				lightInfo.makeColor();
				Game1.instance.NeedSave = true;
			}
		}

		private void cboType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboType.Text == "Ceiling") useFloor = false;
			else useFloor = true;

			fillUIWithSector(Game1.instance.selectedSector);
		}
	}
}
