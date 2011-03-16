<div class="span-20">
    <h2>Stacktraces</h2>
    <table cellpadding="0" cellspacing="0">
        <tr>
            <th>P. Id</th>
            <th>P. Name</th>
            <th>Message</th>
            <th>Flash V.</th>
            <th>Game V.</th>
            <th>Browser V.</th>
			<th>Times</th>
            <th>Last Time</th>			
        </tr>
        <?php
        $i = 0;
        foreach ($stacktraces as $stacktrace) {
            $class = null;
            if ($i++ % 2 == 0) {
                $class = ' class="altrow"';
            }
        ?>
            <tr<?php echo $class; ?>>
                <td><?php echo $stacktrace['Stacktrace']['player_id']; ?>&nbsp;</td>
                <td><?php echo $stacktrace['Stacktrace']['player_name']; ?>&nbsp;</td>
                <td><?php echo $stacktrace['Stacktrace']['message']; ?>&nbsp;</td>
                <td><?php echo $stacktrace['Stacktrace']['flash_version']; ?>&nbsp;</td>
                <td><?php echo $stacktrace['Stacktrace']['game_version']; ?>&nbsp;</td>
                <td><?php echo $stacktrace['Stacktrace']['browser_version']; ?>&nbsp;</td>
				<td><?php echo $stacktrace['Stacktrace']['occurrences']; ?>&nbsp;</td>
                <td><?php echo $stacktrace['Stacktrace']['updated']; ?>&nbsp;</td>
            </tr>
        <?php } ?>
    </table>
</div>

<div class="span-20 last">
    <div class="paging">
        <?php echo $this->Paginator->prev('<< ' . __('previous', true), array(), null, array('class' => 'disabled')); ?> |	<?php echo $this->Paginator->numbers(); ?> | <?php echo $this->Paginator->next(__('next', true) . ' >>', array(), null, array('class' => 'disabled')); ?>
    </div>
</div>