// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "messaging.h"
#include<windows.h>
#include<psapi.h>
#include "handlers.h"

HMODULE this_dll_handle=NULL;
void close_tracker() {
    FreeLibraryAndExitThread(this_dll_handle,0);//may not run dtors...
}
void send_message(Messager& mainMessager,char* message) {
    BYTE buffer[100];
    if (mainMessager.read_message(buffer, 40) == -1) {
        close_tracker();
    }
    while (strcmp("place_holder", (char*)buffer)) {
        if (mainMessager.read_message(buffer, 40) == -1) {
            close_tracker();
        }
    }
    mainMessager.write_message((BYTE*)message, strlen(message));
}
void sectionlister(Messager& mainMessager, LPVOID baseAddress = 0) {
    HANDLE hproc = GetCurrentProcess(); // OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetCurrentProcessId());
    if (hproc == NULL) {
        throw "Failed to open process.";
    }
    MEMORY_BASIC_INFORMATION64  memInfo;//MEMORY_BASIC_INFORMATION64
    while (VirtualQuery(baseAddress, (PMEMORY_BASIC_INFORMATION)&memInfo, sizeof(memInfo))) {
        
        if (memInfo.State != MEM_FREE) {
            char ret[500];
            char path[200];
            HMODULE hmodule;
            GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (LPCSTR)baseAddress,&hmodule);
            if (hmodule) {
                GetModuleFileNameA(hmodule,path, 200);
                if (hmodule == GetModuleHandle(0)) {
                    sprintf_s(ret, 500, "%s\n%p\n%llu\n%d\n%d\n%d\n%d\n%s\n----------------------------------\n",
                        "Memory Region: ",
                        memInfo.BaseAddress,
                        memInfo.RegionSize,
                        memInfo.Protect,
                        memInfo.Type,
                        memInfo.State,
                        memInfo.AllocationProtect,
                        path);
                    send_message(mainMessager,ret);
                }
            }
            else {
                path[0] = '\0';
            }
        }
        baseAddress = (LPVOID)(memInfo.BaseAddress + memInfo.RegionSize);
    }
    CloseHandle(hproc);
}
void getPEinfo(Messager& mainMessager){
    PIMAGE_DOS_HEADER dosHeader;
    PIMAGE_NT_HEADERS NTHeader;
#ifdef _WIN64
    DWORD64 baseAddress = (DWORD64)GetModuleHandle(NULL);
#else
    DWORD32 baseAddress = (DWORD32)GetModuleHandle(NULL);
#endif 
    dosHeader = (PIMAGE_DOS_HEADER)(baseAddress);
    if (((*dosHeader).e_magic) != IMAGE_DOS_SIGNATURE) {//check that the signature is MZ
        throw "e_magic is'nt MZ";
    }
    NTHeader = (PIMAGE_NT_HEADERS)(baseAddress + dosHeader->e_lfanew);
    if (((*NTHeader).Signature) != IMAGE_NT_SIGNATURE) {//check that the signature is PE
        throw "NTHeader Signature is'nt PE";
    }
    char ret[200];
    PIMAGE_SECTION_HEADER sectionHeader= (PIMAGE_SECTION_HEADER)(baseAddress+dosHeader->e_lfanew + sizeof(IMAGE_NT_HEADERS));
    for (DWORD i = 0; i < NTHeader->FileHeader.NumberOfSections; ++i) {
        sprintf_s(ret, 200,"%s%s%s%s%p%s%d\n----------------------------------\n",
            "Sections:",
            "\nSection Name: ", sectionHeader[i].Name,
            "\nVirtual Address: ", baseAddress+sectionHeader[i].VirtualAddress,
            "\nVirtual Size: ", sectionHeader[i].Misc.VirtualSize
            );
        send_message(mainMessager, ret);
    }
    send_message(mainMessager, (char*)"finished");
}
void get_modules(Messager& mainMessager) {
    DWORD lpcbNeeded, lpcbNeeded2;
again:
    if (!EnumProcessModulesEx(GetCurrentProcess(),0,0, &lpcbNeeded,LIST_MODULES_ALL)) {
        throw "EnumProcessModulesEx not working";
    }
    if (lpcbNeeded <= 0) {
        return;
    }
    HMODULE* hModules =(HMODULE*)malloc(lpcbNeeded);
    if (hModules == NULL) {
        throw "failed to allocate memmory";// __LINE__
    }
    if (!EnumProcessModulesEx(GetCurrentProcess(), hModules, lpcbNeeded, &lpcbNeeded2, LIST_MODULES_ALL)) {
        throw "EnumProcessModulesEx 2 not working";
    }
    if (lpcbNeeded2 != lpcbNeeded) {
        free(hModules);
        goto again;
    }
    lpcbNeeded /= sizeof(HMODULE);
    send_message(mainMessager, (char*)"-----------dlls-----------\n");
    for (DWORD i = 0; i < lpcbNeeded; i++) {
        char lpFilename[150];
        if (GetModuleFileNameExA(GetCurrentProcess(), hModules[i], lpFilename, 150)) {
            send_message(mainMessager, lpFilename);
            send_message(mainMessager, (char*)"\n");
        }
        else {
            _itoa_s(GetLastError(), lpFilename,10);
            MessageBoxA(0, lpFilename,"get last error:", 0);
        }
    }
    free(hModules);
    send_message(mainMessager, (char*)"--------------------------\n");
   // send_message(mainMessager, (char*)"finished");
}
void WINAPI workerMain(LPVOID PARAM) {
    try {
        Sleep(100);//be sure that the pipe opened
        Messager mainMessager(L"main");
        get_modules(mainMessager);
        getPEinfo(mainMessager);
        sectionlister(mainMessager, 0);
        //sectionlister(mainMessager, (LPVOID)0xFFFF800000000001);
    }
    catch (char* msg) {
        MessageBoxA(0, msg, 0, 0);
        close_tracker();
    }
    catch (...) {
        MessageBoxA(0, "unknown error",0, 0);
        close_tracker();
    }
}
/*
void WINAPI workerMain(LPVOID PARAM) {
    try {
        Messager mainMessager(L"main");
        BYTE buffer[4096];
        while (1) {
            if (mainMessager.read_message(buffer, 40) == -1) {
                close_tracker();
            }
            if (buffer[0] != 0) {
                MessageBoxA(0, (char*)buffer, (char*)buffer, 0);
                mainMessager.write_message(buffer,strlen((char*)buffer)+1);
            }
        }
    }
    catch (char* msg) {
        MessageBoxA(0, msg, msg, 0);
        close_tracker();
    }
    catch (...) {
        MessageBoxA(0, "unknown error",0, 0);
        close_tracker();
    }
}
*/


BOOL APIENTRY DllMain(HMODULE hModule,DWORD  ul_reason_for_call,LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        this_dll_handle = hModule;
        //CreateThread(0,0, (LPTHREAD_START_ROUTINE)workerMain,0,0,0);
        //MessageBoxA(0,"injected", "injected",0);
        initilizeHandlers();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        clearHandlers();
        break;
    }
    return TRUE;
}

