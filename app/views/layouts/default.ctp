<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <?php echo $this->Html->charset(); ?>
        <title><?php echo $title_for_layout . ($this->name == 'Pages' && $this->action == 'index' ? '' : ' - Tribal Hero'); ?></title>
        <?php
        echo $this->Html->meta('description', 'Tribal Hero is a free multiplayer stategy game. Build your own empire, unite with other players, and wage wars.');
        echo $this->Html->meta('keywords', 'tribalhero, tribal hero, strategy game, play free, online game, online strategy game, browser game, flash game, game');

        echo $this->Html->css('blueprint/screen', null, array("media" => "screen, projection"));
        ?>
        <!--[if lt IE 8]><?php echo $this->Html->css('blueprint/ie', null, array("media" => "screen, projection")); ?><![endif]-->
        <?php
        echo $this->Html->css('style.main');
        ?>
    </head>
    <body>
        <div class="wrapper">
            <div id="header" class="container">
                <div id="login" class="push-18 span-6 last">
                    <cake:nocache>
                        <?php if ($session->check('Auth.Player.id')) {
                        ?>
                            <div>Hello, <strong><?php echo $session->read('Auth.Player.name') ?></strong>.</div>
                        <?php echo $this->Html->link('Play', '/play') ?> | <?php echo $this->Html->link('Account', '/players/account') ?> | <?php echo $this->Html->link('Logout', '/players/logout') ?>
                        <?php } else {
                        ?>
                        <?php echo $form->create('Player', array('class' => 'small no-label', 'action' => 'login', 'div' => false)); ?>
                            <div class="last input text">
                            <?php echo $form->input('Player.name', array('rel' => 'Username', 'label' => false, 'class' => 'default', 'error' => false)) ?>
                        </div>
                        <div class="last input password">
                            <?php echo $form->input('Player.password', array('rel' => 'Password', 'label' => false, 'error' => false, 'class' => 'default', 'div' => false)) ?>
                            <span><?php echo $this->Html->link("Forgot?", array('controller' => 'players', 'action' => 'forgot')); ?></span>
                        </div>
                        <div class="last input">
                            <div class="float-left buttons">
                                <?php echo $form->button('Login', array('alt' => 'Login', 'type' => 'submit', 'class' => 'small', 'div' => false)); ?>
                            </div>
                            <div class="">Not a member? <?php echo $this->Html->link("Register Here", array('controller' => 'players', 'action' => 'register')); ?></div>
                        </div>
                        <?php echo $form->end(); ?>
                        <?php } ?>
                        </cake:nocache>
                    </div>
                </div>
                <div id="content" class="container prepend-top">
                    <cake:nocache>
                    <?php if ($session->check('Message.flash')) : ?>
                                <div>
                        <?php echo $session->flash(); ?>
                            </div>
                    <?php endif; ?>
                            </cake:nocache>

                            <div class="span-4">
                                <ul id="main-nav">
                                    <li id="home"><?php echo $this->Html->link('', '/'); ?></li>
                                    <li id="help"><?php echo $this->Html->link('', '/database/'); ?></li>
                                    <li id="forums"><a href="http://forums.tribalhero.com"></a></li>
                                    <li id="blog"><a href="http://blog.tribalhero.com"></a></li>
                                    <li id="register"><?php echo $this->Html->link('', '/players/register'); ?></li>
                                </ul>

                    <?php if ($session->check('Auth.Player.id') && $session->read('Auth.Player.admin')) {
                    ?>
                                    <ul class="prepend-top">
                                        <li><?php echo $this->Html->link('Server Status', '/admin/systems/status'); ?></li>
                                        <li><?php echo $this->Html->link('Battle Status', '/admin/systems/battle_stats'); ?></li>
                                        <li><?php echo $this->Html->link('Stacktraces', '/admin/stacktraces'); ?></li>
                                    </ul>
                    <?php } ?>
                            </div>
                            <div class="span-20 last">
                    <?php echo $content_for_layout; ?>
                            </div>
                        </div>
                        <div class="push"></div>
                    </div>
                    <div id="footer">&nbsp;</div>

        <?php
                                if (!isset($use_jquery) || $use_jquery === TRUE) :
        ?>
                                    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>
                                    <script>!window.jQuery && document.write('<script src="<?php echo $this->Html->url('/js/jquery-1.4.2.min.js'); ?>"><\/script>')</script>
        <?php
                                    echo $this->Html->script('jquery/jquery.defaultvalue');
        ?>
                                    <script type="text/javascript">
                                        $(document).ready(function(){
                                            $('input.default').defaultValue();
                                        });
                                    </script>
        <?php
                                    endif;
        ?>

        <?php
                                    echo $scripts_for_layout;
                                    echo $js->writeBuffer();
        ?>

        <?php
                                    // Only show IE6 bar on main page
                                    if ($this->name == 'Pages' && $this->action == 'index') :
        ?>
                                        <!--[if IE 6]>
                                        <script type="text/javascript">
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            		/*Load jQuery if not already loaded*/ if(typeof jQuery == 'undefined'){ document.write("<script type=\"text/javascript\"   src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js\"></"+"script>"); var __noconflict = true; }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            		var IE6UPDATE_OPTIONS = {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            			icons_path: "<?php echo $this->Html->url('/js/ie6update/images/'); ?>"
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            		}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            	</script>
        <?php echo $this->Html->script('ie6update/ie6update.js'); ?>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            	<![endif]-->
        <?php endif; ?>

        <script type="text/javascript">
            google_analytics_uacct = "UA-17369212-2";
            google_analytics_domain_name = "tribalhero.com";
            var _gaq = _gaq || [];
            _gaq.push(['_setAccount', 'UA-17369212-2'],['_setDomainName', 'tribalhero.com'],['_trackPageview']);
            (function() {var ga = document.createElement('script');
                ga.type = 'text/javascript';
                ga.async = true;
                ga.src = 'http://www.google-analytics.com/ga.js';
                (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(ga);
            })();
        </script>
    </body>
</html>