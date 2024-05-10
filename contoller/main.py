import time
import dllInjector
import messaging
import process_explorer
from os.path import abspath as getAbsolutePath
dll32path=getAbsolutePath(r"dll worker\Debug\dll worker.dll")#r"C:\Users\user\Documents\קורסים\יג\א\יסודות באבטחת תוכנה\פרוייקט\dll worker\Debug\dll worker.dll"
dll64path=getAbsolutePath(r"dll worker\x64\Debug\dll worker.dll")#r"C:\Users\user\Documents\קורסים\יג\א\יסודות באבטחת תוכנה\פרוייקט\dll worker\x64\Debug\dll worker.dll"

target_name="Notepad.exe"

target=process_explorer.find_process_by_name(target_name)
if target==None:
    raise Exception("coldn't find the target process")

dllInjector.inject(dll64path,target.pid)
#commands=("readMemory","writeMemory","list sections","exit")
print("injected")
main_messager=messaging.messager()
print("started")
while True:
    cc=input()
    main_messager.send_message(cc.encode("ascii")+b'\x00')
    time.sleep(0.1)
    print(main_messager.recieve_message())


