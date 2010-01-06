package src.UI.Components
{
	import flash.display.DisplayObjectContainer;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.JPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.City;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.Troop.TroopTemplate;
	import src.Objects.Troop.UnitTemplate;
	import src.Objects.Troop.UnitTemplateManager;
	import src.UI.GameLookAndFeel;
	import src.Util.BinaryList.BinaryList;
	import src.Util.Util;

	public class UnitStatBox extends JPanel
	{		
		private var type: int;
		//The template can be either a trooptemplate or unittemplate depending on who
		//is using this box
		private var template: BinaryList;
		
		private var lblArmor: JLabel;
		private var lblWeapon: JLabel;
		private var lblHp: JLabel;
		private var lblAttack: JLabel;
		private var lblDefense: JLabel;
		private var lblStealth: JLabel;
		private var lblRange: JLabel;
		private var lblSpeed: JLabel;
		private var lblUpkeep: JLabel;
		
		private var lblArmorTitle: JLabel;
		private var lblWeaponTitle: JLabel;
		private var lblHpTitle: JLabel;
		private var lblAttackTitle: JLabel;
		private var lblDefenseTitle: JLabel;
		private var lblStealthTitle: JLabel;
		private var lblRangeTitle: JLabel;
		private var lblSpeedTitle: JLabel;
		private var lblUpkeepTitle: JLabel;		

		public function UnitStatBox(type: int, city: City, template: BinaryList)
		{
			createUI();
			
			var armor: String;
			var weapon: String;
			var hp: int;
			var upkeep: int;
			var attack: int;
			var defense: int;
			var stealth: int;
			var range: int;
			var speed: int;
			
			var unitPrototype: UnitPrototype;
			if (template is UnitTemplateManager) {				
				var unitTemplate: UnitTemplate = template.get(type);
				unitPrototype = UnitFactory.getPrototype(unitTemplate.type, unitTemplate.level);
				//TODO: we should get the city modifiers here when it's available
				
				attack = unitPrototype.attack;
				defense = unitPrototype.defense;
				speed = unitPrototype.speed;
				range = unitPrototype.range;
				hp = unitPrototype.hp;
				stealth = unitPrototype.stealth;
				upkeep = unitPrototype.upkeep;
				armor = unitPrototype.armor;
				weapon = unitPrototype.weapon;						
			}
			else {
				var troopTemplate: TroopTemplate = template.get(type);
				unitPrototype = UnitFactory.getPrototype(troopTemplate.type, troopTemplate.level);
				
				attack = troopTemplate.attack;
				defense = troopTemplate.defense;
				speed = troopTemplate.speed;
				range = troopTemplate.range;
				hp = troopTemplate.maxHp;
				stealth = troopTemplate.stealth;
				upkeep = unitPrototype.upkeep;
				armor = unitPrototype.armor;
				weapon = unitPrototype.weapon;				
			}
			
			lblAttack.setText(attack.toString());
			lblDefense.setText(defense.toString());
			lblSpeed.setText(speed.toString());
			lblRange.setText(range.toString());
			lblStealth.setText(stealth.toString());
			lblHp.setText(hp.toString());
			lblUpkeep.setText("-" + upkeep.toString());
			lblWeapon.setText(weapon);
			lblArmor.setText(armor);
		}

		private function createUI() : void
		{
			setLayout(new GridLayout(5, 4, 5, 0));			
			
			lblArmorTitle = titleLabelMaker("Armor");
			lblWeaponTitle = titleLabelMaker("Weapon");
			lblHpTitle = titleLabelMaker("HP");
			lblAttackTitle = titleLabelMaker("Attack");
			lblDefenseTitle = titleLabelMaker("Defense");
			lblStealthTitle = titleLabelMaker("Stealth");
			lblRangeTitle = titleLabelMaker("Range");
			lblSpeedTitle = titleLabelMaker("Speed");
			lblUpkeepTitle = titleLabelMaker("Upkeep");
			
			lblArmor = valueLabelMaker();
			lblWeapon = valueLabelMaker();
			lblHp = valueLabelMaker();
			lblAttack = valueLabelMaker();
			lblDefense = valueLabelMaker();
			lblStealth = valueLabelMaker();
			lblRange = valueLabelMaker();
			lblSpeed = valueLabelMaker();
			lblUpkeep = valueLabelMaker(new AssetIcon(new ICON_CROP()));
			
			appendAll(lblHpTitle, lblHp, new JLabel(), new JLabel());
			appendAll(lblAttackTitle, lblAttack, lblDefenseTitle, lblDefense);
			appendAll(lblWeaponTitle, lblWeapon, lblArmorTitle, lblArmor);
			appendAll(lblRangeTitle, lblRange, lblStealthTitle, lblStealth);			
			appendAll(lblSpeedTitle, lblSpeed, lblUpkeepTitle, lblUpkeep);
		}
		
		private function titleLabelMaker(title: String) : JLabel {
			var lbl: JLabel = new JLabel(title, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lbl, "Tooltip.text");
			return lbl;
		}
		
		private function valueLabelMaker(icon: Icon = null) : JLabel {
			var lbl: JLabel = new JLabel("", icon, AsWingConstants.LEFT);		
			lbl.setIconTextGap(0);
			lbl.setHorizontalTextPosition(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lbl, "Tooltip.text");
			return lbl;
		}
	}

}

