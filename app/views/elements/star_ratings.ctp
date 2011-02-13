<?php
	/* Parameters:
	 * $min Minimum value for range
	 * $max Maximum value for range
	 * $value Value to plot
	 * $numberOfStars Number of stars to show
	 */

	// normalize max and value so it goes from 0-max only
	$adjustedMax = $max - $min;
	$adjustedValue = $value - $min;

	$percentage = max(0, min(100, ($adjustedValue / $adjustedMax) * 100));
	$fullStarPercent = round(100 / $numberOfStars);			

	$fullStars = floor($percentage / $fullStarPercent);
	$halfStars = $fullStars == 0 ? 1 : floor(($percentage - ($fullStars * $fullStarPercent)) / ($fullStarPercent / 2));

	$rating = $fullStars + 0.5 * $halfStars;
	
	for ($i = 0; $i < $fullStars; $i++) {		
		echo $this->Html->image('db/icons/props/star.png', array('alt' => 'Rating - ' . $rating . ' star(s)'));
	}

	for ($i = 0; $i < $halfStars; $i++) {
		echo $this->Html->image('db/icons/props/star-half.png', array('alt' => 'Rating - ' . $rating . ' star(s)'));
	}
	
	for ($i = 0; $i < $numberOfStars - ($fullStars + $halfStars); $i++) {
		echo $this->Html->image('db/icons/props/star-empty.png', array('alt' => 'Rating - ' . $rating . ' star(s)'));
	}