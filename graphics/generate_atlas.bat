set "PATH=C:/Program Files/CodeAndWeb/TexturePacker/bin;%PATH%"

set "DEFAULT_SETTINGS=--format sparrow --variant 0.5:1x --variant 0.75:1.5x --variant 1:2x"
set "DEFAULT_THEME_SETTINGS=%DEFAULT_SETTINGS% --ignore-files *_BANNER.png --ignore-files *_WALL.png"

:: Until we have icons that are actually HD
set "TMP_ICON_SETTINGS=--format sparrow --enable-rotation --variant 1:1x --variant 1.5:1.5x --variant 2:2x"

:: Tilemap
:: TexturePacker.exe %DEFAULT_SETTINGS%^
::  --data atlas\{v}\TILESET_ATLAS.xml^
::  --sheet atlas\{v}\TILESET_ATLAS.png^
::  TILESET.png

:: Default Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\{v}\DEFAULT_THEME_WALL_ATLAS.xml^
  --sheet atlas\{v}\DEFAULT_THEME_WALL_ATLAS.png^
  themes\default\DEFAULT_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\{v}\DEFAULT_THEME_ATLAS.xml^
  --sheet atlas\{v}\DEFAULT_THEME_ATLAS.png^
  themes\default

:: Pirate Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\{v}\PIRATES_THEME_WALL_ATLAS.xml^
  --sheet atlas\{v}\PIRATES_THEME_WALL_ATLAS.png^
  themes\pirates\PIRATES_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\{v}\PIRATES_THEME_ATLAS.xml^
  --sheet atlas\{v}\PIRATES_THEME_ATLAS.png^
  themes\pirates

:: Feudal Japan Theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\{v}\FEUDALJAPAN_THEME_WALL_ATLAS.xml^
  --sheet atlas\{v}\FEUDALJAPAN_THEME_WALL_ATLAS.png^
  themes\feudaljapan\FEUDALJAPAN_WALL.png

TexturePacker.exe %DEFAULT_THEME_SETTINGS%^
  --data atlas\{v}\FEUDALJAPAN_THEME_ATLAS.xml^
  --sheet atlas\{v}\FEUDALJAPAN_THEME_ATLAS.png^
  themes\feudaljapan

:: Icons
TexturePacker.exe %TMP_ICON_SETTINGS%^
  --data atlas\{v}\ICONS_ATLAS.xml^
  --sheet atlas\{v}\ICONS_ATLAS.png^
  icons\general^
  icons\menu^
  icons\achievements

:: Misc Objects (e.g. forests), War Elephant Theme, and Cobblestone Road theme
TexturePacker.exe %DEFAULT_SETTINGS%^
  --data atlas\{v}\OBJECTS_ATLAS.xml^
  --sheet atlas\{v}\OBJECTS_ATLAS.png^
  objects^
  themes\warelephant^
  themes\cobblestone