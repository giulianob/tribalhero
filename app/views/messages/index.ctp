<div class="container">
	<div class="span-3">
		<ul id="messaging-menu">
			<li><?php echo $html->link("Compose Mail", "new")?></li>
			<li class="prepend-top"><?php echo $html->link("Inbox", "index", array("class" => "sel"))?></li>
			<li><?php echo $html->link("Sent", "sent")?></li>
			<li><?php echo $html->link("Trash", "trash")?></li>
		</ul>	
	</div>
	<div class="span-21 last">
		<?php if (empty($messages)) : ?>
			<div class="quiet">No messages in this folder yet...</div>
		<?php else : ?>
			<div class="span-21 last">
				<div class="span-10">
					<select class="no-margin">
						<option>With selected</option>
						<option>Delete</option>
						<option>Mark as unread</option>
					</select>
				</div>
				<div class="span-11 text-right last">
					<?php echo $paginator->prev('« Newer ', null, null, array('class' => 'hidden')); ?>
					<?php echo $paginator->counter(array('format' => '%start%-%end% of %count%')); ?> 					
					<?php echo $paginator->next(' Older »', null, null, array('class' => 'hidden')); ?>
				</div>
			</div>				
			<div class="span-21 last padding-top1">
				<table id="messaging">
					<tbody>
						<?php 
							$i = 0;
							foreach ($messages as $message) :	
								$subjectAllowedLength = min(strlen($message['Message']['subject']), 30);
								$messageAllowedLength = 90 - strlen($message['Message']['subject']);						 
						?>
							<tr>
								<td class="sel"><?php echo $form->input('selectedMessages.' . $i, array('type' => 'checkbox', 'label' => false)); ?></td>
								<td class="user"><?php echo $message['Sender']['name']?></td>
								<td class="prev">
									<a href="messages/view/<?php echo $message['Message']['id']?>"><div class="inline"><?php echo $text->truncate($message['Message']['subject'], $subjectAllowedLength); ?></div>&nbsp;&nbsp;<div class="quiet inline"><?php echo $text->truncate($message['Message']['message'], $messageAllowedLength); ?></div></a>
								</td>
								<td class="date"><?php echo $time->niceShort($message['Message']['created']); ?></td>
							</tr>		
						<?php
							++$i;
							endforeach; 
						?>	
					</tbody>	
				</table>
			</div>
			<div class="span-21 last">
				<div class="span-10">
					<select class="no-margin">
						<option>With selected</option>
						<option>Delete</option>
						<option>Mark as unread</option>
					</select>
				</div>
				<div class="span-11 text-right last">
					<?php echo $paginator->prev('« Newer ', null, null, array('class' => 'hidden')); ?>
					<?php echo $paginator->counter(array('format' => '%start%-%end% of %count%')); ?> 					
					<?php echo $paginator->next(' Older »', null, null, array('class' => 'hidden')); ?>
				</div>
			</div>		 			
		<? endif; ?>
	</div>
	
</div>
