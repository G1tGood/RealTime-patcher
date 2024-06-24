#include "pch.h"
#ifndef YESODOT_UTILS_H
#define YESODOT_UTILS_H
#include<windows.h>
#include<string>
#define __FILENAME__ (strrchr(__FILE__, '\\') ? strrchr(__FILE__, '\\') + 1 : __FILE__)


extern HMODULE this_dll_handle;

void debugMessageNum(INT64 num) {
    char printbuf[20];
    _itoa_s(num, printbuf, 20);
    MessageBoxA(0, printbuf, "num", 0);
}

void showErr(const char* file,int line) {
#ifdef _DEBUG
    char errbuf[10];
    _itoa_s(GetLastError(), errbuf, 10);
    MessageBoxA(0, errbuf, "get last error:", 0);
    _itoa_s(line, errbuf, 10);
    MessageBoxA(0, errbuf, file, 0);
    //FreeLibraryAndExitThread(this_dll_handle, 0);
    //FreeLibrary(this_dll_handle);
#endif
}
HANDLE open_pipe(const wchar_t* name) {//             FILE_FLAG_OVERLAPPED
    WaitNamedPipeW(name, NMPWAIT_WAIT_FOREVER);
    HANDLE pipe = CreateFileW(name, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
    if (pipe == INVALID_HANDLE_VALUE) {
        if(GetLastError()!= ERROR_BROKEN_PIPE)
            showErr(__FILENAME__,__LINE__);
    }
    return pipe;
}
void wait_pipe_drain(HANDLE pipe) {
    while (true) {
        DWORD avalableBytes;
        if (!PeekNamedPipe(pipe, NULL, 0, 0, 0, &avalableBytes)) {
            showErr(__FILENAME__, __LINE__);
            return;
        }
        if (avalableBytes == 0) {
            break;
        }
    }
}
void  register_handler(const wchar_t* name, void(CALLBACK* handler)(PVOID, BOOLEAN)) {
    HANDLE hEvent = CreateEventW(NULL, 0, 0, name);
    if (hEvent == NULL)
    {
        showErr(__FILENAME__, __LINE__);
    }
    HANDLE hWaitHandle;
    if (!RegisterWaitForSingleObject(&hWaitHandle, hEvent, handler, NULL, INFINITE, WT_EXECUTEDEFAULT))
    {
        showErr(__FILENAME__, __LINE__);
    }
}
#endif