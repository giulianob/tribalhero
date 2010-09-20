<?php
	/* Parameters
	 * time Time span to format
	 */
	if ($time <= 0) echo "--:--:--";
	else {
		$hours = floor($time / (60 * 60));
		$time -= $hours * 60 * 60;
		$minutes = floor($time / 60);
		$time -= $minutes * 60;
		$seconds = $time;

		echo ($hours <= 9 ? "0" . $hours : $hours) . ":" . ($minutes <= 9 ? "0" . $minutes : $minutes) . ":" . ($seconds <= 9 ? "0" . $seconds : $seconds);
	}