package src.UI.Components
{
	import org.aswing.JPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.Troop.TroopTemplate;
	import src.Objects.Troop.TroopTemplateManager;
	import src.Objects.Troop.UnitTemplate;
	import src.Objects.Troop.UnitTemplateManager;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.BinaryList.BinaryList;

	public class UnitStatBox extends JPanel
	{
		private var type: int;
		//The template can be either a trooptemplate or unittemplate depending on who
		//is using this box
		private var template: BinaryList;

		private var lblArmor: JLabel;
		private var lblWeapon: JLabel;
		private var lblHp: JLabel;
		private var lblCarry: JLabel;
		private var lblUnitClass: JLabel;
		private var lblWeaponClass: JLabel;
		private var lblAttack: StarRating;
		private var lblDefense: StarRating;
		private var lblStealth: StarRating;
		private var lblRange: StarRating;
		private var lblSpeed: StarRating;
		private var lblUpkeep: JLabel;

		private var lblArmorTitle: JLabel;
		private var lblWeaponTitle: JLabel;
		private var lblHpTitle: JLabel;
		private var lblCarryTitle: JLabel;
		private var lblUnitClassTitle: JLabel;
		private var lblWeaponClassTitle: JLabel;
		private var lblAttackTitle: JLabel;
		private var lblDefenseTitle: JLabel;
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

			statBox.init(unitPrototype.carry, unitPrototype.armor, unitPrototype.weapon, unitPrototype.weaponClass, unitPrototype.unitClass, unitPrototype.hp, unitPrototype.upkeep, unitPrototype.attack, unitPrototype.defense, unitPrototype.stealth, unitPrototype.range, unitPrototype.speed);

			return statBox;			
		}

		public static function createFromTroopTemplate(type: int, template: TroopTemplateManager) : UnitStatBox {
				var troopTemplate: TroopTemplate = template.get(type);
				var unitPrototype: UnitPrototype = UnitFactory.getPrototype(troopTemplate.type, troopTemplate.level);

			var statBox: UnitStatBox = new UnitStatBox();

			statBox.init(unitPrototype.carry, unitPrototype.armor, unitPrototype.weapon, unitPrototype.weaponClass, unitPrototype.unitClass, troopTemplate.maxHp, unitPrototype.upkeep, troopTemplate.attack, troopTemplate.defense, troopTemplate.stealth, troopTemplate.range, troopTemplate.speed);

			return statBox;
		}

		public function UnitStatBox()
		{			
			createUI();
		}

		private function init(carry: int, armor: String, weapon: String, weaponClass: String, unitClass: String, hp: int, upkeep: int, attack: int, defense: int, stealth: int, range: int, speed: int) : void {
			lblAttack.setValue(attack / upkeep);
			lblCarry.setText(carry.toString());
			lblDefense.setValue(defense / upkeep);
			lblSpeed.setValue(speed);
			lblRange.setValue(range);
			lblStealth.setValue(stealth);
			lblHp.setText(hp.toString());
			lblUnitClass.setText(unitClass);
			lblWeaponClass.setText(weaponClass);
			lblUpkeep.setText("-" + upkeep.toString());
			lblWeapon.setText(weapon);
			lblArmor.setText(armor);
		}

		private function createUI() : void
		{
			setLayout(new GridLayout(6, 4, 3, 0));

			lblCarryTitle = titleLabelMaker("Carry");
			lblArmorTitle = titleLabelMaker("Armor");
			lblWeaponTitle = titleLabelMaker("Weapon");
			lblHpTitle = titleLabelMaker("HP");
			lblWeaponClassTitle = titleLabelMaker("Weapon Class");
			lblUnitClassTitle = titleLabelMaker("Unit Class");
			lblAttackTitle = titleLabelMaker("Attack");
			lblDefenseTitle = titleLabelMaker("Defense");
			lblStealthTitle = titleLabelMaker("Stealth");
			lblRangeTitle = titleLabelMaker("Range");
			lblSpeedTitle = titleLabelMaker("Speed");
			lblUpkeepTitle = titleLabelMaker("Upkeep");

			lblCarry = valueLabelMaker();
			lblArmor = valueLabelMaker();
			lblWeapon = valueLabelMaker();
			lblHp = valueLabelMaker();
			lblAttack = new StarRating(Constants.unitStatRanges.attack.min, Constants.unitStatRanges.attack.max, 0, 5);
			lblWeaponClass = valueLabelMaker();
			lblUnitClass = valueLabelMaker();
			lblDefense = new StarRating(Constants.unitStatRanges.defense.min, Constants.unitStatRanges.defense.max, 0, 5);
			lblStealth = new StarRating(Constants.unitStatRanges.stealth.min, Constants.unitStatRanges.stealth.max, 0, 5);
			lblRange = new StarRating(Constants.unitStatRanges.range.min, Constants.unitStatRanges.range.max, 0, 5);
			lblSpeed = new StarRating(Constants.unitStatRanges.speed.min, Constants.unitStatRanges.speed.max, 0, 5);
			lblUpkeep = valueLabelMaker(new AssetIcon(new ICON_CROP()));

			appendAll(lblHpTitle, lblHp, lblCarryTitle, lblCarry);
			appendAll(lblArmorTitle, lblArmor, lblUnitClassTitle, lblUnitClass);
			appendAll(lblWeaponTitle, lblWeapon, lblWeaponClassTitle, lblWeaponClass);
			appendAll(lblAttackTitle, lblAttack, lblDefenseTitle, lblDefense);
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

