<div class="span-20 last">
    <h2>Send Us Feedback</h2>
    <p>Find a bug? Really want to see something new in the game? Having problems with the game?<br />Write anything you want below and we promise to read it. Your suggestions are extremely valuable so use this form as often as you need.</p>
    <?php echo $form->create('Feedback', array('url' => '/feedback')); ?>
    <?php echo $form->input('message', array('label' => false, 'type' => 'textarea')) ?>
    <div class="buttons">
        <?php echo $form->button('Send', array('alt' => 'Send', 'type' => 'submit', 'div' => false)); ?>
    </div>
    <?php echo $form->end(); ?>
</div>