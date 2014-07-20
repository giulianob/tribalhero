package src.UI.Tutorial 
{
    import org.aswing.AsWingManager;

    import src.Comm.Commands.GeneralComm;
    import src.Map.Map;
    import src.UI.Tutorial.Steps.*;

    public class GameTutorial
	{		
		private const steps: Array = [
			BuildFarmStep,
			CityOverviewStep,
			AssignLaborerToFarmStep,
			BuildLumbermillStep,
			GatherWoodStep,
			UpgradeTownCenterStep,
			BuildTrainingGroundStep,
			UpgradeSwordTechStep,
			TrainFightersStep,
			AttackBarbariansStep
		];
		
		private var currentStepIndex: int = 0;
		private var currentStep: TutorialStep;
		private var map: Map;		
		private var generalComm:GeneralComm;
		
		/**
		 * Starts the game tutorial
		 * @param	map
		 */
		public function start(startStep: int, map: Map, generalComm: GeneralComm): void {
			this.generalComm = generalComm;
			this.map = map;
			currentStepIndex = startStep;
			executeStep(currentStepIndex);
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
							
			generalComm.saveTutorialStep(currentStepIndex);
			if (currentStepIndex < steps.length) {				
				executeStep(currentStepIndex);
			}
		}
		
		/**
		 * Executes a step in the tutorial
		 * @param	step The step index to execute
		 */
		private function executeStep(step: int): void {
			if (currentStepIndex >= steps.length) {
				return;
			}
			
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