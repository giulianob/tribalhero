del bin\Release\Game.swf
del bin\Game.swf
mkdir bin\Release
"C:\Program Files (x86)\FlashDevelop\Tools\fdbuild\fdbuild.exe" "C:\source\Game\Game.as3proj" -compiler "C:\flex_sdk\3.4.1.10084" -notrace -library "C:\Program Files (x86)\FlashDevelop\Library"
"C:\Program Files (x86)\secureSWF\ssCLI.exe" "C:\source\Game\Game.sspj" "c:\source\Game\bin\Release" -wrap:0