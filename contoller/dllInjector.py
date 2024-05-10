import ctypes.wintypes
import win32api,win32process,win32event
from win32con import PROCESS_ALL_ACCESS,MEM_COMMIT,MEM_RESERVE, PAGE_READWRITE
from ctypes import create_unicode_buffer


def inject(dllPath:str,pid:int):
    wchar_t_dllPath=create_unicode_buffer(dllPath)

    kernel32=win32api.GetModuleHandle("kernel32.dll")
    if kernel32==0:
        raise Exception("couldn't get kernel32.dll handle")
    LoadLibrary=win32api.GetProcAddress(kernel32,"LoadLibraryW")
    if LoadLibrary==0:
        raise Exception("couldn't get LoadLibraryA address")
    targetProcess=win32api.OpenProcess(PROCESS_ALL_ACCESS,0,pid)
    if targetProcess==0:
        raise Exception("couldn't get handle to the target process")
    targetProcessMemory=win32process.VirtualAllocEx(targetProcess,0,len(wchar_t_dllPath)+1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE)
    if targetProcessMemory==0:
        raise Exception("couldn't allocate memory in target process")
    if win32process.WriteProcessMemory(targetProcess,targetProcessMemory,wchar_t_dllPath)==0:
        raise Exception("couldn't write to the memory of the target process")
    remoteThread=win32process.CreateRemoteThread(targetProcess,None,0,LoadLibrary,targetProcessMemory,0)
    if remoteThread[0]==None:
        raise Exception("CreateRemoteThread failed")
    win32event.WaitForSingleObject(remoteThread[0],win32event.INFINITE)
    win32api.CloseHandle(remoteThread[0])
    win32api.CloseHandle(targetProcess)