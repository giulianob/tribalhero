package src.UI.Components
{
    import org.aswing.*;
    import org.aswing.border.*;

    import src.Constants;
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
		private var lblAttack: JLabel;
		private var lblMaxLabor: JLabel;
		private var lblStealth: JLabel;
		private var lblRange: JLabel;
		private var lblRadius: JLabel;

		private var lblArmorTitle: JLabel;
		private var lblWeaponTitle: JLabel;
		private var lblHpTitle: JLabel;
		private var lblMaxLaborTitle: JLabel;
		private var lblAttackTitle: JLabel;
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
			lblAttack.setText(structurePrototype.attack.toString());
			lblRange.setText(Constants.stealthRangeNames[structurePrototype.range]);
			lblStealth.setText(Constants.stealthRangeNames[structurePrototype.stealth]);
			lblHp.setText(structurePrototype.hp.toString());
			lblWeapon.setText(structurePrototype.weapon);
			lblRadius.setText(structurePrototype.radius.toString());
		}

		private function createUI() : void
		{
			setPreferredWidth(375);
			setBorder(new EmptyBorder(null, new Insets(5)));
			setLayout(new GridLayout(2, 4, 2, 2));

			lblArmorTitle = titleLabelMaker("Armor");
			lblWeaponTitle = titleLabelMaker("Weapon");
			lblHpTitle = titleLabelMaker("HP");
			lblMaxLaborTitle = titleLabelMaker("Max Laborer");
			lblAttackTitle = titleLabelMaker("Attack");
			lblStealthTitle = titleLabelMaker("Position");
			lblRangeTitle = titleLabelMaker("Range");
			lblRadiusTitle = titleLabelMaker("Radius");

			lblArmor = valueLabelMaker();
			lblWeapon = valueLabelMaker();
			lblHp = valueLabelMaker();
			lblMaxLabor = valueLabelMaker();
			lblAttack = valueLabelMaker();
			lblStealth = valueLabelMaker();
			lblRange = valueLabelMaker();
			lblRadius = valueLabelMaker();

			appendAll(lblHpTitle, lblHp, lblAttackTitle, lblAttack);
			appendAll(lblRangeTitle, lblRange, lblStealthTitle, lblStealth);
			if (structurePrototype.maxlabor > 0) {
				(getLayout() as GridLayout).setRows((getLayout() as GridLayout).getRows() + 1);
				appendAll(lblMaxLaborTitle, lblMaxLabor, new JLabel(), new JLabel());
			}
			
			if (structurePrototype.radius > 0) {
				(getLayout() as GridLayout).setRows((getLayout() as GridLayout).getRows() + 1);
				appendAll(lblRadiusTitle, lblRadius, new JLabel(), new JLabel());
			}
		}

		private function titleLabelMaker(title: String) : JLabel {
			var lbl: JLabel = new JLabel(title, null, AsWingConstants.LEFT);
			lbl.mouseEnabled = false;
			GameLookAndFeel.changeClass(lbl, ["Tooltip.text", "Label.small"]);
			return lbl;
		}

		private function valueLabelMaker(icon: Icon = null) : JLabel {
			var lbl: JLabel = new JLabel("", icon, AsWingConstants.LEFT);
			lbl.setIconTextGap(0);
			lbl.setHorizontalTextPosition(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lbl, ["Tooltip.text", "Label.small"]);
			return lbl;
		}
	}

}

