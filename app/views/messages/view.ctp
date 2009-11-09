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
		<div id="message" class="span-16 last">
			<div class="span-16 last">
				<h1><?php echo $message['Message']['subject']; ?></h1>
			</div>		
			<div class="box span-16 last">				
				<div class="span-8">
					<div class="quiet inline">From</div>
					<?php echo $session->read('Auth.Player.id') == $message['Sender']['id'] ? 'Me' : $message['Sender']['name'] ?>  
					<div class="quiet inline">to</div>
					<?php echo $session->read('Auth.Player.id') == $message['Recipient']['id'] ? 'me' : $message['Recipient']['name'] ?>  
				</div>
				<div class="span-8 last text-right">
					<?php echo $time->niceShort($message['Message']['created']); ?>  
				</div>			
				<div class="span-16 last padding-top2">
					<?php echo $message['Message']['message']; ?>
				</div>
				<div class="span-16 last prepend-top">
					<hr class="no-margin" />
					<?php echo $form->create('Message', array('action' => 'new')); ?>
					<?php echo $form->hidden('subject', array('value' => substr('Re: ' . $message['Message']['subject'], 0, 90))); ?>
					<?php echo $form->hidden('recipient', array('value' => $session->read('Auth.Player.id') == $message['Sender']['id'] ? $message['Recipient']['id'] : $message['Sender']['id'])); ?>
					<?php echo $form->input('message', array('type' => 'textarea')); ?>
					<?php echo $form->end('Reply');?>
				</div>
			</div>			
		</div>
	</div>
	
</div>
