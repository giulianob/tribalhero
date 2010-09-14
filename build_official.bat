del bin\Release\Game.swf
del bin\Game.swf
mkdir bin\Release
"C:\Program Files (x86)\FlashDevelop\Tools\fdbuild\fdbuild.exe" "C:\source\Game\Game.as3proj" -ipc 25fc9510-a484-454d-9190-4e417f0ea841 -compiler "C:\flex_sdk\3.3.0.4582" -notrace -library "C:\Program Files (x86)\FlashDevelop\Library"
"C:\Program Files (x86)\secureSWF\ssCLI.exe" "C:\source\Game\Game.sspj" "c:\source\Game\bin\Release" -wrap:0