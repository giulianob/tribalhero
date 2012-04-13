<?php

class TimeAdvHelper extends AppHelper {
	
	function diff($diff) {
		$hours = floor($diff / 3600);
		$diff = $diff - ($hours * 3600);

		$minutes = floor($diff / 60);
		$diff = $diff - ($minutes * 60);

		$seconds = $diff;
		
		return str_pad($hours, 2, '0', STR_PAD_LEFT) . ':' . str_pad($minutes, 2, '0', STR_PAD_LEFT) . ':' . str_pad($seconds, 2, '0', STR_PAD_LEFT);
	}
	
}