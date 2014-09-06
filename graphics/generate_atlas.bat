set "PATH=C:/Program Files/CodeAndWeb/TexturePacker/bin;%PATH%"
set "NO_SCALE=--variant 1:noscale"
set "HD_SCALE_VARIANT=--variant 0.5:1x --variant 1:2x"

set "DEFAULT_SETTINGS=--format sparrow --enable-rotation --multipack"
set "DEFAULT_THEME_SETTINGS=%DEFAULT_SETTINGS% %NO_SCALE% --ignore-files *_BANNER.png --ignore-files *_WALL.png"

:: Tilemap
TexturePacker.exe %DEFAULT_SETTINGS% %NO_SCALE%^
  --data atlas\{v}\TILESET_ATLAS.xml^
  --sheet atlas\{v}\TILESET_ATLAS.png^
  TILESET.png

:: Default Theme
TexturePacker.exe %DEFAULT_SETTINGS% %NO_SCALE%^
  --data atlas\{v}\DEFAULT_THEME_WALL_ATLAS.xml^
  --sheet atlas\{v}\DEFAULT_THEME_WALL_ATLAS.png^
  themes\default\DEFAULT_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\{v}\DEFAULT_THEME_ATLAS.xml^
  --sheet atlas\{v}\DEFAULT_THEME_ATLAS.png^
  themes\default

:: Pirate Theme
TexturePacker.exe %DEFAULT_SETTINGS% %NO_SCALE%^
  --data atlas\{v}\PIRATES_THEME_WALL_ATLAS.xml^
  --sheet atlas\{v}\PIRATES_THEME_WALL_ATLAS.png^
  themes\pirates\PIRATES_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\{v}\PIRATES_THEME_ATLAS.xml^
  --sheet atlas\{v}\PIRATES_THEME_ATLAS.png^
  themes\pirates

:: Feudal Japan Theme
TexturePacker.exe %DEFAULT_SETTINGS% %NO_SCALE%^
  --data atlas\{v}\FEUDALJAPAN_THEME_WALL_ATLAS.xml^
  --sheet atlas\{v}\FEUDALJAPAN_THEME_WALL_ATLAS.png^
  themes\feudaljapan\FEUDALJAPAN_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\{v}\FEUDALJAPAN_THEME_ATLAS.xml^
  --sheet atlas\{v}\FEUDALJAPAN_THEME_ATLAS.png^
  themes\feudaljapan

:: Icons
TexturePacker.exe %DEFAULT_SETTINGS% %HD_SCALE_VARIANT%^
  --data atlas\{v}\ICONS_ATLAS.xml^
  --sheet atlas\{v}\ICONS_ATLAS.png^
  icons\general

:: Misc Objects (e.g. forests), War Elephant Theme, and Cobblestone Road theme
TexturePacker.exe %DEFAULT_SETTINGS% %NO_SCALE%^
  --data atlas\{v}\OBJECTS_ATLAS.xml^
  --sheet atlas\{v}\OBJECTS_ATLAS.png^
  objects^
  themes\warelephant^
  themes\cobblestone