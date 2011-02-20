<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <?php echo $html->charset(); ?>
        <title>
            <?php echo $title_for_layout; ?> - Tribal Hero
        </title>
        <?php
            echo $html->css('style.play');
            echo $scripts_for_layout;
            echo $js->writeBuffer();
        ?>            
        </head>
        <body>
        <?php echo $content_for_layout; ?>

    </body>
</html>