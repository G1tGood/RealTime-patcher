import win32api,win32process,win32event
from win32con import PROCESS_ALL_ACCESS,MEM_COMMIT,MEM_RESERVE, PAGE_READWRITE,MEM_DECOMMIT
from ctypes import create_unicode_buffer

def inject(dllPath:str,pid:int):
    wchar_t_dllPath=create_unicode_buffer(dllPath)#converts the path to wchar_t encoding

    #get the adrress of LoadLibraryW
    kernel32=win32api.GetModuleHandle("kernel32.dll")
    if kernel32==0:
        raise Exception("couldn't get kernel32.dll handle")
    LoadLibrary=win32api.GetProcAddress(kernel32,"LoadLibraryW")
    if LoadLibrary==0:
        raise Exception("couldn't get LoadLibraryW address")
    #open the target process
    targetProcess=win32api.OpenProcess(PROCESS_ALL_ACCESS,0,pid)
    if targetProcess==0:
        raise Exception("couldn't get handle to the target process")
    #allocating memory in the target process
    targetProcessMemory=win32process.VirtualAllocEx(targetProcess,0,len(wchar_t_dllPath)+1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE)
    if targetProcessMemory==0:
        raise Exception("couldn't allocate memory in target process")
    #write the path to the allocated memory in the target process
    if win32process.WriteProcessMemory(targetProcess,targetProcessMemory,wchar_t_dllPath)==0:
        raise Exception("couldn't write to the memory of the target process")
    #create new thread in the target process 
    #that will run the address the we found earlier (of LoadLibraryW)
    # with the allocated memmory as parameter
    remoteThread=win32process.CreateRemoteThread(targetProcess,None,0,LoadLibrary,targetProcessMemory,0)
    if remoteThread[0]==None:
        raise Exception("CreateRemoteThread failed")
    #close handles
    win32event.WaitForSingleObject(remoteThread[0],win32event.INFINITE)
    #release the allocated memmory
    win32process.VirtualFreeEx(targetProcess,targetProcessMemory,len(wchar_t_dllPath),MEM_DECOMMIT)#not sure that this is the correct params
    win32api.CloseHandle(remoteThread[0])
    win32api.CloseHandle(targetProcess)