package src.UI.Components
{
    import org.aswing.*;
    import org.aswing.border.*;

    import src.*;
    import src.Map.*;
    import src.Objects.Effects.*;
    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;
    import src.Objects.Troop.*;
    import src.UI.LookAndFeel.*;
    import src.Util.BinaryList.*;
    import src.Util.StringHelper;

    public class UnitStatBox extends JPanel
	{
		private var type: int;
		//The template can be either a trooptemplate or unittemplate depending on who
		//is using this box
		private var template: BinaryList;

		private var lblArmor: JLabel;
		private var lblWeapon: JLabel;
		private var lblSplash: JLabel;
		private var lblHp: JLabel;
		private var lblCarry: JLabel;
		private var lblUnitClass: JLabel;
		private var lblWeaponClass: JLabel;
		private var lblAttack: JLabel;
		private var lblStealth: JLabel;
		private var lblRange: JLabel;
		private var lblSpeed: JLabel;
		private var lblUpkeep: JLabel;

		private var lblArmorTitle: JLabel;
		private var lblSplashTitle: JLabel;
		private var lblWeaponTitle: JLabel;
		private var lblHpTitle: JLabel;
		private var lblCarryTitle: JLabel;
		private var lblUnitClassTitle: JLabel;
		private var lblWeaponClassTitle: JLabel;
		private var lblAttackTitle: JLabel;
		private var lblStealthTitle: JLabel;
		private var lblRangeTitle: JLabel;
		private var lblSpeedTitle: JLabel;
		private var lblUpkeepTitle: JLabel;

		public static function createFromCityTemplate(type: int, city: City) : UnitStatBox {
			var unitTemplate: UnitTemplate = city.template.get(type);
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unitTemplate.type, unitTemplate.level);

			return createFromCity(unitPrototype, city);
		}
		
		public static function createFromPrototype(unitPrototype: UnitPrototype, city: City) : UnitStatBox {
			return createFromCity(unitPrototype, city);
		}
		
		private static function createFromCity(unitPrototype: UnitPrototype, city: City) : UnitStatBox {
			//TODO: we should get the city modifiers here when it's available

			var statBox: UnitStatBox = new UnitStatBox();

			statBox.init(unitPrototype.carry, unitPrototype.armor, unitPrototype.weapon, unitPrototype.weaponClass, unitPrototype.unitClass, unitPrototype.hp, unitPrototype.upkeep, unitPrototype.attack, unitPrototype.splash, unitPrototype.defense, unitPrototype.stealth, unitPrototype.range, unitPrototype.speed);

			return statBox;			
		}

		public static function createFromTroopTemplate(type: int, template: TroopTemplateManager) : UnitStatBox {
				var troopTemplate: TroopTemplate = template.get(type);
				var unitPrototype: UnitPrototype = UnitFactory.getPrototype(troopTemplate.type, troopTemplate.level);

			var statBox: UnitStatBox = new UnitStatBox();

			statBox.init(unitPrototype.carry, unitPrototype.armor, unitPrototype.weapon, unitPrototype.weaponClass, unitPrototype.unitClass, troopTemplate.maxHp, unitPrototype.upkeep, troopTemplate.attack, troopTemplate.splash, troopTemplate.defense, troopTemplate.stealth, troopTemplate.range, troopTemplate.speed);

			return statBox;
		}

		public function UnitStatBox()
		{			
			createUI();
		}

		private function init(carry: int, armor: String, weapon: String, weaponClass: String, unitClass: String, hp: Number, upkeep: int, attack: Number, splash: int, defense: int, stealth: int, range: int, speed: int) : void {
			lblAttack.setText(attack.toString());
			lblCarry.setText(carry.toString());		
			lblSpeed.setText(Formula.moveTimeStringSimple(speed));
			lblSplash.setText(splash.toString() + StringHelper.makePlural(splash, " hit", " hits"));
			lblRange.setText(Constants.stealthRangeNames[range]);
			lblStealth.setText(Constants.stealthRangeNames[stealth]);
			lblHp.setText(hp.toString());
			lblUpkeep.setText("-" + (upkeep / Constants.secondsPerUnit).toString());
		}

		private function createUI() : void
		{
			setPreferredWidth(375);
			setBorder(new EmptyBorder(null, new Insets(5)));
			setLayout(new GridLayout(4, 4, 2, 2));

			lblCarryTitle = titleLabelMaker("Carry");
			lblArmorTitle = titleLabelMaker("Armor");
			lblWeaponTitle = titleLabelMaker("Weapon");
			lblHpTitle = titleLabelMaker("HP");
			lblWeaponClassTitle = titleLabelMaker("Weapon Class");
			lblUnitClassTitle = titleLabelMaker("Unit Class");
			lblAttackTitle = titleLabelMaker("Attack");
			lblStealthTitle = titleLabelMaker("Position");
			lblRangeTitle = titleLabelMaker("Range");
			lblSpeedTitle = titleLabelMaker("Speed");
			lblSplashTitle = titleLabelMaker("Splash");
			lblUpkeepTitle = titleLabelMaker("Upkeep");

			lblCarry = valueLabelMaker();
			lblHp = valueLabelMaker();
			lblAttack = valueLabelMaker();
			lblStealth = valueLabelMaker();
			lblRange = valueLabelMaker();
			lblSplash = valueLabelMaker();
			lblSpeed = valueLabelMaker();
			lblUpkeep = valueLabelMaker(new AssetIcon(SpriteFactory.getFlashSprite("ICON_CROP")));

			appendAll(lblHpTitle, lblHp, lblSplashTitle, lblSplash);
			appendAll(lblAttackTitle, lblAttack, lblCarryTitle, lblCarry);
			appendAll(lblRangeTitle, lblRange, lblStealthTitle, lblStealth);			
			appendAll(lblSpeedTitle, lblSpeed, lblUpkeepTitle, lblUpkeep);
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

