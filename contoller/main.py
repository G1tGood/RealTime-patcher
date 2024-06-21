import time
import traceback
import dllInjector
import messaging
import process_explorer
import os
from os.path import abspath as getAbsolutePath
dll32path=getAbsolutePath(r"dll worker\Debug\dll worker.dll")
dll64path=getAbsolutePath(r"dll worker\x64\Debug\dll worker.dll")
os.system("Notepad")
#os.system(r"start C:\Windows\SysWOW64\notepad.exe")
target_name="Notepad.exe"

target=process_explorer.find_process_by_name(target_name)
if target==None:
    raise Exception("coldn't find the target process")

dllInjector.inject(dll64path,target.pid)
print("injected")
main_messager=messaging.messager("main")
print("started")

State_Constants={0x1000:"MEM_COMMIT",
                 0x10000:"MEM_FREE",
                 0x2000:"MEM_RESERVE"}
Type_Constants={0x1000000:"MEM_IMAGE",
                 0x40000:"MEM_MAPPED",
                 0x20000:"MEM_PRIVATE"}
def get_Memory_Protection_Constants(identifer:int):
    Memory_Protection_Constants={0x10:"PAGE_EXECUTE",
                             0x20:"PAGE_EXECUTE_READ",
                             0x40:"PAGE_EXECUTE_READWRITE",
                             0x80:"PAGE_EXECUTE_WRITECOPY",
                             0x01:"PAGE_NOACCESS",
                             0x02:"PAGE_READONLY",
                             0x04:"PAGE_READWRITE",
                             0x08:"PAGE_WRITECOPY",
                             0x40000000:"PAGE_TARGETS_INVALID/PAGE_TARGETS_NO_UPDATE",
                             0x00:"0"}
    base=Memory_Protection_Constants[identifer&0xfffff8ff]
    return (base
            +("|PAGE_GUARD" if identifer&0x100 else "")
            +("|PAGE_NOCACHE" if identifer&0x200 else "")
            +("|PAGE_WRITECOMBINE" if identifer&0x400 else "")
            )
main_messager.write_message("place_holder".encode("ascii")+b'\x00')

with open("output.log","wb") as f:
    try:
        while True:
            x=main_messager.read_message()
            while x[1]==b"place_holder":
                x=main_messager.read_message()
            x=x[1]
            if x==b"finished":
                main_messager.write_message("place_holder".encode("ascii")+b'\x00')
                break
            f.write(x)
            main_messager.write_message("place_holder".encode("ascii")+b'\x00')
        while True:
            x=main_messager.read_message()
            while x[1].decode("ascii")=="place_holder":
                x=main_messager.read_message()[1]
            x=x[1].decode("ascii").split("\n")
            x[1]="Base Address: "+x[1]
            x[2]="Region Size: "+x[2]
            x[3]="Protect: "+get_Memory_Protection_Constants(int(x[3]))
            x[4]="Type: "+Type_Constants[int(x[4])]
            x[5]="State: "+State_Constants[int(x[5])]
            x[6]="AllocationProtect: "+get_Memory_Protection_Constants(int(x[6]))
            
            x=map(lambda i:i+"\n",x)
            f.write("".join(list(x)).encode("ascii"))
            main_messager.write_message("place_holder".encode("ascii")+b'\x00')
    except Exception:
        traceback.print_exc()
        os.system("taskkill -im Notepad.exe")
exit()
while True:
    cc=input()
    main_messager.write_message(cc.encode("ascii")+b'\x00')
    time.sleep(0.1)
    print(main_messager.read_message())


