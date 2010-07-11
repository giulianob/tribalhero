package src.UI.Components
{
	import org.aswing.JPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Prototypes.StructurePrototype;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class StructureStatBox extends JPanel
	{
		private var type: int;
		private var level: int;

		private var structurePrototype: StructurePrototype;

		private var lblArmor: JLabel; //Armor is not currently used
		private var lblWeapon: JLabel;
		private var lblHp: JLabel;
		private var lblDefense: JLabel;
		private var lblMaxLabor: JLabel;
		private var lblStealth: JLabel;
		private var lblRange: JLabel;
		private var lblRadius: JLabel;

		private var lblArmorTitle: JLabel;
		private var lblWeaponTitle: JLabel;
		private var lblHpTitle: JLabel;
		private var lblMaxLaborTitle: JLabel;
		private var lblDefenseTitle: JLabel;
		private var lblStealthTitle: JLabel;
		private var lblRangeTitle: JLabel;
		private var lblRadiusTitle: JLabel;

		public function StructureStatBox(type: int, level: int)
		{
			this.type = type;
			this.level = level;
			this.structurePrototype = StructureFactory.getPrototype(type, level);

			if (!this.structurePrototype) return;

			createUI();

			lblMaxLabor.setText(structurePrototype.maxlabor.toString());
			lblDefense.setText(structurePrototype.defense.toString());
			lblRange.setText(structurePrototype.range.toString());
			lblStealth.setText(structurePrototype.stealth.toString());
			lblHp.setText(structurePrototype.hp.toString());
			lblWeapon.setText(structurePrototype.weapon);
			lblRadius.setText(structurePrototype.radius.toString());
		}

		private function createUI() : void
		{
			setLayout(new GridLayout(3, 4, 5, 0));

			lblArmorTitle = titleLabelMaker("Armor");
			lblWeaponTitle = titleLabelMaker("Weapon");
			lblHpTitle = titleLabelMaker("HP");
			lblMaxLaborTitle = titleLabelMaker("Max Laborer");
			lblDefenseTitle = titleLabelMaker("Defense");
			lblStealthTitle = titleLabelMaker("Stealth");
			lblRangeTitle = titleLabelMaker("Range");
			lblRadiusTitle = titleLabelMaker("Radius");

			lblArmor = valueLabelMaker();
			lblWeapon = valueLabelMaker();
			lblHp = valueLabelMaker();
			lblMaxLabor = valueLabelMaker();
			lblDefense = valueLabelMaker();
			lblStealth = valueLabelMaker();
			lblRange = valueLabelMaker();
			lblRadius = valueLabelMaker();

			appendAll(lblHpTitle, lblHp, lblDefenseTitle, lblDefense);
			appendAll(lblWeaponTitle, lblWeapon, structurePrototype.maxlabor>0?lblMaxLaborTitle:new JLabel(), structurePrototype.maxlabor>0?lblMaxLabor:new JLabel());
			appendAll(lblRangeTitle, lblRange, lblStealthTitle, lblStealth);
			if (structurePrototype.radius > 0) {
				(getLayout() as GridLayout).setRows(4);
				appendAll(lblRadiusTitle, lblRadius, new JLabel(), new JLabel());
			}
		}

		private function titleLabelMaker(title: String) : JLabel {
			var lbl: JLabel = new JLabel(title, null, AsWingConstants.LEFT);
			lbl.mouseEnabled = false;
			GameLookAndFeel.changeClass(lbl, "Tooltip.text Label.small");
			return lbl;
		}

		private function valueLabelMaker(icon: Icon = null) : JLabel {
			var lbl: JLabel = new JLabel("", icon, AsWingConstants.LEFT);
			lbl.setIconTextGap(0);
			lbl.setHorizontalTextPosition(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lbl, "Tooltip.text Label.small");
			return lbl;
		}
	}

}

