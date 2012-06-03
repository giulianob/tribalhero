package src.UI.Tutorial 
{
	import flash.utils.getDefinitionByName;
	import org.aswing.AsWingManager;
	import src.Map.Map;
	import src.UI.Tutorial.Steps.*;
	 
	public class GameTutorial 
	{		
		private const steps: Array = [
			BuildFarmStep,
			CityOverviewStep,
			AssignLaborerToFarmStep,
			BuildLumbermillStep,
			GatherWoodStep
		];
		
		private var currentStepIndex: int = 0;
		private var currentStep: TutorialStep;
		private var map: Map;		
		
		/**
		 * Starts the game tutorial
		 * @param	map
		 */
		public function start(map: Map): void {
			this.map = map;
			currentStepIndex = 0;
			executeStep(0);
		}
		
		public function stop(): void {
			if (this.currentStep) {
				this.currentStep.dispose();
				this.currentStep = null;
			}
		}
		
		/**
		 * Executes the next step in the sequence
		 */
		private function resume(): void {
			currentStepIndex++;
			if (currentStepIndex < steps.length) {
				executeStep(currentStepIndex);
			}			
		}
		
		/**
		 * Executes a step in the tutorial
		 * @param	step The step index to execute
		 */
		private function executeStep(step: int): void {
			var tutorialStepClass:Class = steps[step] as Class;
			var tutorialStep: TutorialStep = new tutorialStepClass();
			tutorialStep.resume = resume;
			tutorialStep.map = map;
			if (this.currentStep) {
				this.currentStep.hideAllMessages();
				this.currentStep = null;
			}
			
			this.currentStep = tutorialStep;
			
			AsWingManager.callLater(tutorialStep.execute);
		}		
	}

}