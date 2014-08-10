set "PATH=C:/Program Files/CodeAndWeb/TexturePacker/bin;%PATH%"
set "DEFAULT_SETTINGS=--format sparrow --enable-rotation"
set "DEFAULT_THEME_SETTINGS=%DEFAULT_SETTINGS% --ignore-files *_BANNER.png --ignore-files *_WALL.png"

:: Default Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\DEFAULT_THEME_WALL_ATLAS.xml^
  --sheet atlas\DEFAULT_THEME_WALL_ATLAS.png^
  themes\default\DEFAULT_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\DEFAULT_THEME_ATLAS.xml^
  --sheet atlas\DEFAULT_THEME_ATLAS.png^
  themes\default

:: Pirate Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\PIRATES_THEME_WALL_ATLAS.xml^
  --sheet atlas\PIRATES_THEME_WALL_ATLAS.png^
  themes\pirates\PIRATES_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\PIRATES_THEME_ATLAS.xml^
  --sheet atlas\PIRATES_THEME_ATLAS.png^
  themes\pirates

:: Feudal Japan Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\FEUDALJAPAN_THEME_WALL_ATLAS.xml^
  --sheet atlas\FEUDALJAPAN_THEME_WALL_ATLAS.png^
  themes\feudaljapan\FEUDALJAPAN_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\FEUDALJAPAN_THEME_ATLAS.xml^
  --sheet atlas\FEUDALJAPAN_THEME_ATLAS.png^
  themes\feudaljapan

:: Icons
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\ICONS_ATLAS.xml^
  --sheet atlas\ICONS_ATLAS.png^
  icons\general

:: Misc Objects (e.g. forests) and War Elephant Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\OBJECTS_ATLAS.xml^
  --sheet atlas\OBJECTS_ATLAS.png^
  objects^
  themes\warelephant