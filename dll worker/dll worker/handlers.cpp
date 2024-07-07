#include "pch.h"
#include<windows.h>
#include<string>
#include<psapi.h>
#include "utils.h"
#define MY_MAX_PATH 500

VOID CALLBACK tsett(PVOID lpParameter, BOOLEAN TimerOrWaitFired)
{
    MessageBoxA(0, "blabla", "", 0);

    HANDLE hEvent = OpenEventW(EVENT_ALL_ACCESS, 0, L"Global\\yesodot_memory_Dynamic_Memory_Patcher_test");
    if (hEvent == NULL) {
        showErr(__FILENAME__, __LINE__);
    }
    if (!SetEvent(hEvent)) {
        showErr(__FILENAME__, __LINE__);
    }
    CloseHandle(hEvent);
}


void CALLBACK get_modules(PVOID lpParameter, BOOLEAN TimerOrWaitFired) {
    DWORD lpcbNeeded, lpcbNeeded2;
again:
    //get how much modules we have
    if (!EnumProcessModulesEx(GetCurrentProcess(), 0, 0, &lpcbNeeded, LIST_MODULES_ALL)) {
        showErr(__FILENAME__, __LINE__);
    }
    if (lpcbNeeded <= 0) {
        return;
    }
    //allocate memory for moudles handles
    HMODULE* hModules = (HMODULE*)malloc(lpcbNeeded);
    if (hModules == NULL) {
        showErr(__FILENAME__, __LINE__);
    }//get modules handles
    if (!EnumProcessModulesEx(GetCurrentProcess(), hModules, lpcbNeeded, &lpcbNeeded2, LIST_MODULES_ALL)) {
        showErr(__FILENAME__, __LINE__);
    }
    if (lpcbNeeded2 != lpcbNeeded) {//the amount of modules changed
        free(hModules);
        goto again;
    }
    lpcbNeeded /= sizeof(HMODULE);
    HANDLE pipe = open_pipe(L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_get_modules");
    //get name of each module
    for (DWORD i = 0; i < lpcbNeeded; i++) {
        wchar_t lpFilename[150];
        if (GetModuleFileNameExW(GetCurrentProcess(), hModules[i], lpFilename, 150)) {
            //send_message(mainMessager, lpFilename);
            //send_message(mainMessager, (char*)"\n");

        }
        else {
            showErr(__FILENAME__, __LINE__);
        }
    }
    free(hModules);
    // send_message(mainMessager, (char*)"finished");
}

HMODULE GetModuleHandleFromAddress(LPVOID address) {

    HMODULE hModules[1024];
    HANDLE procc = GetCurrentProcess();
    DWORD cbNeeded;
    if (EnumProcessModules(procc, hModules, sizeof(hModules), &cbNeeded)) {
        for (DWORD i = 0; i < (cbNeeded / sizeof(HMODULE)); i++) {
            MODULEINFO moduleInfo;
            if (GetModuleInformation(procc, hModules[i], &moduleInfo, sizeof(moduleInfo))) {
                if (reinterpret_cast<LPBYTE>(address) >= reinterpret_cast<LPBYTE>(moduleInfo.lpBaseOfDll) &&
                    reinterpret_cast<LPBYTE>(address) < reinterpret_cast<LPBYTE>(moduleInfo.lpBaseOfDll) + moduleInfo.SizeOfImage) {
                    return hModules[i];
                }
            }
            else {
                showErr(__FILENAME__, __LINE__);
            }
}
    }
    return NULL;
}
void CALLBACK get_module_sections(PVOID lpParameter, BOOLEAN TimerOrWaitFired){
#ifdef _WIN64
    MEMORY_BASIC_INFORMATION64  memInfo;//MEMORY_BASIC_INFORMATION64
#else
    MEMORY_BASIC_INFORMATION32  memInfo;//MEMORY_BASIC_INFORMATION64
#endif    
    LPVOID baseAddress = 0;
    HANDLE pipe=open_pipe(L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_get_module_sections");

    wchar_t module_name[MY_MAX_PATH];
    if (!ReadFile(pipe, module_name, MY_MAX_PATH,NULL, NULL)) {
        showErr(__FILENAME__, __LINE__);
    }

    while (VirtualQuery(baseAddress, (PMEMORY_BASIC_INFORMATION)&memInfo, sizeof(memInfo))) {
        if (memInfo.State != MEM_FREE && memInfo.State != MEM_RESERVE) {
            char ret[100];
            char path[MY_MAX_PATH];
            HMODULE hmodule;// = GetModuleHandleFromAddress(baseAddress);
            GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (LPCSTR)baseAddress, &hmodule);

            wchar_t lpFilename[MY_MAX_PATH];
            if (GetModuleFileNameW(hmodule, lpFilename, MY_MAX_PATH)) {

                //if (hmodule == GetModuleHandleW(module_name)) {
                if (!_wcsicmp(lpFilename, module_name)) {

                    //sprintf_s(ret, 500, "%p\n%llu\n%d\n%d\n%d\n%d\n",
                    sprintf_s(ret, 100, "%p\n%llu\n%d\n",
                        memInfo.BaseAddress,
                        memInfo.RegionSize,
                        memInfo.Protect
                        //memInfo.Type,
                        //memInfo.State,
                        //memInfo.AllocationProtect
                    );
                    if (!WriteFile(pipe, ret, 100, 0, NULL) || !FlushFileBuffers(pipe)) {
                        showErr(__FILENAME__, __LINE__);
                    }
                }


            }
            else {
                showErr(__FILENAME__, __LINE__);
            }
        }
        baseAddress = (LPVOID)(memInfo.BaseAddress + memInfo.RegionSize);
    }
    wait_pipe_drain(pipe);
    CloseHandle(pipe);
}

void CALLBACK get_module_PE_sections(PVOID lpParameter, BOOLEAN TimerOrWaitFired) {
    HANDLE pipe = open_pipe(L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_get_module_PE_sections");

    wchar_t module_name[MY_MAX_PATH];
    if (!ReadFile(pipe, module_name, MY_MAX_PATH, NULL, NULL)) {
        showErr(__FILENAME__, __LINE__);
    }
    PIMAGE_DOS_HEADER dosHeader;
    PIMAGE_NT_HEADERS NTHeader;
#ifdef _WIN64
    DWORD64 baseAddress = (DWORD64)GetModuleHandleW(module_name);
#else
    DWORD32 baseAddress = (DWORD32)GetModuleHandle(NULL);
#endif 
    dosHeader = (PIMAGE_DOS_HEADER)(baseAddress);
    if (((*dosHeader).e_magic) != IMAGE_DOS_SIGNATURE) {//check that the signature is MZ
        showErr(__FILENAME__, __LINE__);
    }
    NTHeader = (PIMAGE_NT_HEADERS)(baseAddress + dosHeader->e_lfanew);
    if (((*NTHeader).Signature) != IMAGE_NT_SIGNATURE) {//check that the signature is PE
        showErr(__FILENAME__, __LINE__);
    }
    char ret[100];
    PIMAGE_SECTION_HEADER sectionHeader = (PIMAGE_SECTION_HEADER)(baseAddress + dosHeader->e_lfanew + sizeof(IMAGE_NT_HEADERS));
    for (DWORD i = 0; i < NTHeader->FileHeader.NumberOfSections; ++i) {
        sprintf_s(ret, 100, "%s\n%p\n",
            sectionHeader[i].Name,
            baseAddress + sectionHeader[i].VirtualAddress
            //sectionHeader[i].Misc.VirtualSize
        );
        if (!WriteFile(pipe, ret,100, NULL, NULL)) {
            showErr(__FILENAME__, __LINE__);
        }
    }
    wait_pipe_drain(pipe);
    CloseHandle(pipe);
}


CRITICAL_SECTION read_memory_lock;
void CALLBACK read_memory(PVOID lpParameter, BOOLEAN TimerOrWaitFired) {
    EnterCriticalSection(&read_memory_lock);
    {
        HANDLE read_memory_pipe = open_pipe(L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_read_memory");

        PBYTE address;
        //#ifdef _WIN64
        //    DWORD64 len;
        //#else
        DWORD64 len;
        //#endif 
        BYTE addressLen[sizeof(address) + sizeof(len)];
        if (!ReadFile(read_memory_pipe, &addressLen, sizeof(address) + sizeof(len), NULL, NULL)) {
            showErr(__FILENAME__, __LINE__);
        }

        //memcpy(&address, addressLen, sizeof(address));
        address = *((PBYTE*)addressLen);
        //memcpy(&len, addressLen + sizeof(address), sizeof(len));
        len = *((decltype(len)*)(addressLen + sizeof(address)));
        //BYTE buff[16];
        //for (ULONG i = 0; i < len; i+=16) {
        //    for (ULONG j = 0; j < 4; j++) {
        //        ((int*)buff)[i] = *(address + (i + (4 * j) ));
        //    }
        DWORD dwOld = NULL;
        if (VirtualProtect(address, len, PAGE_EXECUTE_READWRITE, &dwOld)) {
            if (!WriteFile(read_memory_pipe, address, len, NULL, NULL)) {
                //if (!WriteFile(read_memory_pipe, buff,16, NULL, NULL)) {
                showErr(__FILENAME__, __LINE__);
            }
            //*address = addressLen[sizeof(address)];
            VirtualProtect(address, len, dwOld, NULL);
        }


        //}
        wait_pipe_drain(read_memory_pipe);
        CloseHandle(read_memory_pipe);
    }
    LeaveCriticalSection(&read_memory_lock);
}


CRITICAL_SECTION write_memmory_lock;
void CALLBACK write_memory(PVOID lpParameter, BOOLEAN TimerOrWaitFired) {
    EnterCriticalSection(&write_memmory_lock);
    HANDLE write_memory_pipe = open_pipe(L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_write_memory");
    PBYTE address;
//#ifdef _WIN64
//    DWORD64 len;
//#else
    DWORD32 len;
//#endif 

    BYTE addressLen[sizeof(address)+sizeof(len)];
    if (!ReadFile(write_memory_pipe, &addressLen, sizeof(address) + sizeof(len), NULL, NULL)) {
        showErr(__FILENAME__, __LINE__);
    }
    //memcpy(&address, addressLen, sizeof(address));
    address = *((PBYTE*)addressLen);
    //memcpy(&len, addressLen + sizeof(address), sizeof(len));
    len = *((decltype(len)*)(addressLen + sizeof(address)));

    DWORD dwOld = NULL;
    if (VirtualProtect(address, len, PAGE_EXECUTE_READWRITE, &dwOld)) {
        if (!ReadFile(write_memory_pipe, address, len, NULL, NULL)) {
            showErr(__FILENAME__, __LINE__);
        }
        //*address = addressLen[sizeof(address)];
        VirtualProtect(address, len, dwOld, NULL);
    }
    else {
        HANDLE cannotWriteEvent = OpenEventW(EVENT_ALL_ACCESS, 0, L"Global\\yesodot_memory_Dynamic_Memory_Patcher_cannotWriteEvent_show");
        if (cannotWriteEvent == NULL) {
            showErr(__FILENAME__, __LINE__);
        }
        if (!SetEvent(cannotWriteEvent)) {
            showErr(__FILENAME__, __LINE__);
        }
        CloseHandle(cannotWriteEvent);
    }
        
    CloseHandle(write_memory_pipe);
    LeaveCriticalSection(&write_memmory_lock);
}


void CALLBACK get_address_section(PVOID lpParameter, BOOLEAN TimerOrWaitFired) {
#ifdef _WIN64
    MEMORY_BASIC_INFORMATION64  memInfo;//MEMORY_BASIC_INFORMATION64
#else
    MEMORY_BASIC_INFORMATION32  memInfo;//MEMORY_BASIC_INFORMATION64
#endif    
    HANDLE pipe = open_pipe(L"\\\\.\\pipe\\yesodot_memory_Dynamic_Memory_Patcher_get_address_section");

    LPVOID address;

    if (!ReadFile(pipe, &address, MY_MAX_PATH, NULL, NULL)) {
        showErr(__FILENAME__, __LINE__);
    }

    if(VirtualQuery(address, (PMEMORY_BASIC_INFORMATION)&memInfo, sizeof(memInfo))) {
        if (memInfo.State != MEM_FREE) {
            char ret[100];
            char path[MY_MAX_PATH];
            HMODULE hmodule;
            GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (LPCSTR)address, &hmodule);
            //if (hmodule) {
                //sprintf_s(ret, 500, "%p\n%llu\n%d\n%d\n%d\n%d\n",
                sprintf_s(ret, 100, "%p\n%llu\n%d\n",
                    memInfo.BaseAddress,
                    memInfo.RegionSize,
                    memInfo.Protect
                    //memInfo.Type,
                    //memInfo.State,
                    //memInfo.AllocationProtect
                );
                if (!WriteFile(pipe, ret, 100, 0, NULL) || !FlushFileBuffers(pipe)) {
                    showErr(__FILENAME__, __LINE__);
                }
            //}
        }
    }
    wait_pipe_drain(pipe);
    CloseHandle(pipe);
}


void initilizeHandlers() {
    register_handler(L"Global\\yesodot_memory_Dynamic_Memory_Patcher_get_module_sections", get_module_sections);
    register_handler(L"Global\\yesodot_memory_Dynamic_Memory_Patcher_get_module_PE_sections", get_module_PE_sections);
    register_handler(L"Global\\yesodot_memory_Dynamic_Memory_Patcher_read_memory", read_memory);
    register_handler(L"Global\\yesodot_memory_Dynamic_Memory_Patcher_write_memory", write_memory);
    register_handler(L"Global\\yesodot_memory_Dynamic_Memory_Patcher_get_address_section", get_address_section);

    InitializeCriticalSection(&write_memmory_lock);
    InitializeCriticalSection(&read_memory_lock);
    //register_handler(L"Global\\yesodot_memory_Dynamic_Memory_Patcher_test2", tsett);
    //HANDLE hEvent = OpenEventW(EVENT_ALL_ACCESS, 0, L"Global\\yesodot_memory_Dynamic_Memory_Patcher_test");
    //if (hEvent == NULL) {
    //    showErr(__FILENAME__, __LINE__);
    //}
    //if (!SetEvent(hEvent)) {
    //    showErr(__FILENAME__, __LINE__);
    //}
    //CloseHandle(hEvent);

}
void clearHandlers() {
    DeleteCriticalSection(&write_memmory_lock);
    DeleteCriticalSection(&read_memory_lock);
    //TODO: take care of the events
}
