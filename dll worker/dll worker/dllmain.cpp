// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include<windows.h>


HANDLE hostMainPipe;
HANDLE connect_pipe_to_host(LPCWSTR pipeidentifier=L"main") {
    wchar_t pipeName[100]= L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_";
    wcscat_s(pipeName,100, pipeidentifier);
    HANDLE hostPipe = CreateFile(pipeName, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
    return hostPipe;
}
DWORD read_hostpipe(HANDLE pipe,char* buffer,size_t len) {
    DWORD bytesRead;
    if (!ReadFile(pipe, buffer, len, &bytesRead, NULL)) {
        return -1;
    }
    return bytesRead;
}
DWORD write_hostpipe(HANDLE pipe,char* buffer) {
    DWORD bytesRead;
    if(!WriteFile(pipe, buffer, strlen(buffer), &bytesRead, NULL)){
        return -1;
    }
    return bytesRead;
}
HMODULE this_dll_handle=NULL;
void close_tracker() {
    FreeLibraryAndExitThread(this_dll_handle,0);
}
void WINAPI workerMain(LPVOID PARAM) {
    Sleep(100);
    hostMainPipe = connect_pipe_to_host();
    if (hostMainPipe == INVALID_HANDLE_VALUE) {
        MessageBoxA(0, "faild to open pipe", 0, 0);
        return;
    }
    char buffer[4096];
    while (1) {
        if (read_hostpipe(hostMainPipe, buffer, 40) == -1) {
            close_tracker();
        }
        if (buffer[0] != 0) {
            MessageBoxA(0, buffer, buffer, 0);
            strcat_s(buffer, 4096, "buyaa!!!");
            write_hostpipe(hostMainPipe, buffer);
        }
    }
    if (hostMainPipe)
        CloseHandle(hostMainPipe);
}
/*
void serverccc() {
    HANDLE hPipe;
    LPCWSTR pipeName = L"\\\\.\\pipe\\MyPipe";

    hPipe = CreateNamedPipe(pipeName, PIPE_ACCESS_DUPLEX, PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT, 
                             PIPE_UNLIMITED_INSTANCES, 512, 512, NMPWAIT_USE_DEFAULT_WAIT, NULL);

    if (hPipe == INVALID_HANDLE_VALUE) {
        std::cerr << "Failed to create pipe. Error code: " << GetLastError() << std::endl;
        return 1;
    }

    if (ConnectNamedPipe(hPipe, NULL)) {
        std::cout << "Client connected. Sending data..." << std::endl;
        // Send data to the client
        const char* data = "Hello from server!";
        DWORD bytesWritten;
        WriteFile(hPipe, data, strlen(data) + 1, &bytesWritten, NULL);
        FlushFileBuffers(hPipe);
    }

}*/
BOOL APIENTRY DllMain( HMODULE hModule,DWORD  ul_reason_for_call,LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        this_dll_handle = hModule;
        CreateThread(0,0, (LPTHREAD_START_ROUTINE)workerMain,0,0,0);
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

