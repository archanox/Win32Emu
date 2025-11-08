int32_t sub_401000()
{
    sub_401010();
    /* tailcall */
    return sub_401020();
}

int32_t sub_401010()
{
    /* tailcall */
    return sub_401460(&data_409550);
}

int32_t sub_401020()
{
    return sub_401873(sub_401030);
}

int32_t sub_401030()
{
    /* tailcall */
    return sub_4014a0(&data_409550);
}

int32_t __stdcall sub_401040(int32_t arg1, enum SHOW_WINDOW_CMD arg2)
{
    data_40957c = arg1;
    HWND eax_1 = sub_401200(arg2);
    data_409580 = eax_1;
    
    if (!eax_1)
        return 0xffffffff;
    
    if (sub_401310() < 0)
    {
        sub_401420();
        MessageBoxA(data_409580, 
            "Could start DirectX engine in your computer. Make sure you have at least version 7 of "
        "DirectX installed.", 
            "Error", MB_ICONEXCLAMATION);
        return 0;
    }
    
    sub_401640(&data_409550, data_409584, 0x5dc, 0x118, 0xffffffff);
    sub_4014d0(&data_409550, data_40957c, 0x65, 0, 0, 0x5dc, 0x118);
    MSG msg;
    
    while (true)
    {
        if (!PeekMessageA(&msg, nullptr, 0, 0, PM_REMOVE))
            sub_401130();
        else
        {
            if (msg.message == 0x12)
                break;
            
            TranslateMessage(&msg);
            DispatchMessageA(&msg);
        }
    }
    sub_401420();
    return 0;
}

uint32_t sub_401130()
{
    uint32_t result = GetTickCount() - data_409548;
    
    if (result >= 0x32)
    {
        sub_401730(&data_409550, data_40958c, 0xf5, 0xaa, data_409590, data_409594, 0x96, 0x8c);
        int32_t i;
        
        do
        {
            int32_t* eax_2 = data_409588;
            i = (*(*eax_2 + 0x2c))(eax_2, 0, 0);
            
            if (!i)
                break;
            
            if (i == 0x887601c2)
            {
                int32_t* eax_3 = data_409588;
                (*(*eax_3 + 0x6c))(eax_3);
                break;
            }
        } while (i == 0x8876021c);
        int32_t eax_5 = data_409590 + 0x96;
        data_409590 = eax_5;
        
        if (eax_5 >= 0x5dc)
        {
            int32_t eax_6 = data_409594;
            data_409590 = 0;
            data_409594 = eax_6 + 0x8c;
            
            if (eax_6 + 0x8c >= 0x118)
                data_409594 = 0;
        }
        
        result = GetTickCount();
        data_409548 = result;
    }
    
    return result;
}

HWND sub_401200(enum SHOW_WINDOW_CMD arg1)
{
    HINSTANCE hInstance = data_40957c;
    WNDCLASSA wndClass;
    wndClass.style = 3;
    wndClass.lpfnWndProc = sub_4012d0;
    wndClass.cbClsExtra = 0;
    wndClass.cbWndExtra = 0;
    wndClass.hInstance = hInstance;
    wndClass.hIcon = LoadIconA(hInstance, 0x7f00);
    wndClass.hCursor = LoadCursorA(nullptr, 0x7f00);
    wndClass.hbrBackground = GetStockObject(BLACK_BRUSH);
    wndClass.lpszMenuName = 0x409598;
    wndClass.lpszClassName = "Basic DD";
    RegisterClassA(&wndClass);
    HINSTANCE hInstance_1 = data_40957c;
    int32_t nHeight = GetSystemMetrics(SM_CYSCREEN);
    HWND hWnd = CreateWindowExA(WS_EX_TOPMOST, "Basic DD", "Basic DD", WS_POPUP, 0, 0, 
        GetSystemMetrics(SM_CXSCREEN), nHeight, nullptr, nullptr, hInstance_1, nullptr);
    ShowWindow(hWnd, arg1);
    UpdateWindow(hWnd);
    SetFocus(hWnd);
    return hWnd;
}

LRESULT __stdcall sub_4012d0(HWND arg1, uint32_t arg2, WPARAM arg3, LPARAM arg4)
{
    if (arg2 != 2 && (arg2 != 0x100 || arg3 != 0x1b))
        return DefWindowProcA(arg1, arg2, arg3, arg4);
    
    PostQuitMessage(0);
    return 0;
}

int32_t sub_401310()
{
    if (DirectDrawCreateEx(nullptr, &data_409584, &data_406114, 0))
        return 0xffffffff;
    
    int32_t* eax_2 = data_409584;
    
    if ((*(*eax_2 + 0x50))(eax_2, data_409580, 0x11))
        return 0xfffffffe;
    
    int32_t* eax_5 = data_409584;
    
    if ((*(*eax_5 + 0x54))(eax_5, 0x280, 0x1e0, 0x10, 0, 0))
        return 0xfffffffd;
    
    int32_t var_7c;
    __builtin_memset(&var_7c, 0, 0x7c);
    int32_t* eax_8 = data_409584;
    int32_t var_94_3 = 0;
    var_7c = 0x7c;
    int32_t var_78 = 0x21;
    int32_t var_14 = 0x218;
    int32_t var_68 = 1;
    
    if ((*(*eax_8 + 0x18))(eax_8, &var_7c, &data_409588, var_94_3))
        return 0xffffffff;
    
    int32_t* eax_11 = data_409588;
    int32_t var_8c = 0;
    var_8c = 4;
    int32_t var_88 = 0;
    int32_t var_84 = 0;
    int32_t var_80 = 0;
    int32_t eax_12 = (*(*eax_11 + 0x30))(eax_11, &var_8c, &data_40958c);
    int32_t eax_13 = -(eax_12);
    return eax_13 - eax_13;
}

int32_t* sub_401420()
{
    sub_4017d0(&data_409550);
    int32_t* eax = data_40958c;
    
    if (eax)
        (*(*eax + 8))(eax);
    
    int32_t* eax_1 = data_409588;
    
    if (eax_1)
        (*(*eax_1 + 8))(eax_1);
    
    int32_t* result = data_409584;
    
    if (!result)
        return result;
    
    return (*(*result + 8))(result);
}

void*** __fastcall sub_401460(void*** arg1)
{
    *arg1 = &data_406110;
    arg1[0xa] = 0;
    arg1[7] = 0xffffffff;
    return arg1;
}

int32_t* __thiscall sub_401480(int32_t* arg1, char arg2)
{
    sub_4014a0(arg1);
    
    if (arg2 & 1)
        sub_4018b4(arg1);
    
    return arg1;
}

int32_t __fastcall sub_4014a0(void*** arg1)
{
    int32_t result = arg1[0xa];
    *arg1 = &data_406110;
    
    if (result)
    {
        OutputDebugStringA("Surface Destroyed\n");
        int32_t* eax = arg1[0xa];
        result = (*(*eax + 8))(eax);
        arg1[0xa] = 0;
    }
    
    return result;
}

int32_t __thiscall sub_4014d0(void* arg1, HINSTANCE arg2, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t ebx = arg6;
    HANDLE h = LoadImageA(arg2, arg3, IMAGE_BITMAP, ebx, arg7, LR_DEFAULTCOLOR);
    
    if (h)
    {
        int32_t* eax = *(arg1 + 0x28);
        
        if (eax)
        {
            (*(*eax + 0x6c))(eax);
            HDC eax_1 = CreateCompatibleDC(nullptr);
            
            if (eax_1)
            {
                SelectObject(eax_1, h);
                void pv;
                GetObjectA(h, 0x18, &pv);
                int32_t var_90;
                
                if (!ebx)
                    ebx = var_90;
                int32_t var_8c;
                
                if (!arg7)
                    arg7 = var_8c;
                int32_t* eax_4 = *(arg1 + 0x28);
                int32_t var_7c = 0x7c;
                int32_t var_78 = 6;
                (*(*eax_4 + 0x58))(eax_4, &var_7c);
                int32_t* eax_5 = *(arg1 + 0x28);
                HDC hdcDest;
                
                if (!(*(*eax_5 + 0x44))(eax_5, &hdcDest))
                {
                    int32_t hDest;
                    int32_t wDest;
                    StretchBlt(hdcDest, 0, 0, wDest, hDest, eax_1, arg4, arg5, ebx, arg7, SRCCOPY);
                    int32_t* eax_9 = *(arg1 + 0x28);
                    (*(*eax_9 + 0x68))(eax_9, hdcDest);
                }
                
                DeleteDC(eax_1);
                *(arg1 + 4) = arg2;
                *(arg1 + 8) = arg3;
                *(arg1 + 0xc) = arg4;
                *(arg1 + 0x10) = arg5;
                *(arg1 + 0x14) = ebx;
                *(arg1 + 0x18) = arg7;
                return 1;
            }
        }
    }
    
    return 0;
}

int32_t __thiscall sub_401640(void* arg1, int32_t* arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{
    int32_t var_7c;
    __builtin_memset(&var_7c, 0, 0x7c);
    int32_t ecx = *arg2;
    int32_t var_98 = 0;
    int32_t* var_a0 = &var_7c;
    var_7c = 0x7c;
    int32_t var_78 = 7;
    int32_t var_14 = 0x4040;
    int32_t var_70 = arg3;
    int32_t var_74 = arg4;
    int32_t eax_1 = (*(ecx + 0x18))(arg2, var_a0, arg1 + 0x28, var_98);
    
    if (eax_1)
    {
        if (eax_1 == 0x8876017c)
        {
            int32_t var_14_1 = 0x840;
            eax_1 = (*(*arg2 + 0x18))(arg2, &var_7c, arg1 + 0x28, 0);
        }
        
        if (eax_1)
            return 0;
    }
    
    if (arg5 != 0xffffffff)
    {
        int32_t* edi_1 = *(arg1 + 0x28);
        int32_t var_84 = arg5;
        int32_t var_80_1 = 0;
        (*(*edi_1 + 0x74))(edi_1, 8, &var_84);
    }
    
    *(arg1 + 0x1c) = arg5;
    *(arg1 + 0x24) = arg3;
    *(arg1 + 0x20) = arg4;
    return 1;
}

int32_t __thiscall sub_401730(void* arg1, int32_t* arg2, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7, int32_t arg8)
{
    int32_t esi = arg7;
    
    if (!esi)
        esi = *(arg1 + 0x24);
    
    int32_t edx = arg8;
    
    if (!edx)
        edx = *(arg1 + 0x20);
    
    int32_t var_10 = arg5;
    int32_t var_c = arg6;
    int32_t var_8 = arg5 + esi;
    int32_t var_4 = arg6 + edx;
    
    while (true)
    {
        int32_t var_24_1;
        
        if (*(arg1 + 0x1c) >= 0)
            var_24_1 = 1;
        else
            var_24_1 = 0;
        
        int32_t eax_4 = (*(*arg2 + 0x1c))(arg2, arg3, arg4, *(arg1 + 0x28), &var_10, var_24_1);
        
        if (!eax_4)
            return 1;
        
        if (eax_4 != 0x887601c2)
        {
            if (eax_4 != 0x8876021c)
                break;
        }
        else
            sub_4017f0(arg1);
    }
    
    return 0;
}

int32_t* __fastcall sub_4017d0(void* arg1)
{
    int32_t* result = *(arg1 + 0x28);
    
    if (result)
    {
        result = (*(*result + 8))(result);
        *(arg1 + 0x28) = 0;
    }
    
    return result;
}

int32_t __fastcall sub_4017f0(void* arg1)
{
    int32_t* eax_1 = *(arg1 + 0x28);
    return (*(*eax_1 + 0x6c))(eax_1);
}

HRESULT __stdcall DirectDrawCreateEx(GUID* lpGuid, void** lplpDD, GUID* iid, struct IUnknown pUnkOuter)
{
    /* tailcall */
    return DirectDrawCreateEx(lpGuid, lplpDD, iid, pUnkOuter);
}

void* sub_401806(int32_t arg1)
{
    int32_t eax;
    int32_t* ecx;
    int32_t edx;
    uint32_t eax_1 = sub_401da0(eax, edx, ecx, data_409ab0);
    void* edx_1 = data_409ab0;
    int32_t* ecx_1 = data_409aac;
    
    if (eax_1 < ecx_1 - edx_1 + 4)
    {
        uint32_t eax_2;
        int32_t* ecx_2;
        int32_t edx_2;
        eax_2 = sub_401da0(eax_1, edx_1, ecx_1, edx_1);
        int32_t var_8_1 = eax_2 + 0x10;
        void* eax_4 = sub_4019fe(eax_2 + 0x10, edx_2, ecx_2, data_409ab0);
        
        if (!eax_4)
            return eax_4;
        
        int32_t ecx_4 = data_409aac - data_409ab0;
        data_409ab0 = eax_4;
        ecx_1 = eax_4 + (ecx_4 >> 2 << 2);
        data_409aac = ecx_1;
    }
    
    *ecx_1 = arg1;
    data_409aac += 4;
    return arg1;
}

int32_t sub_401873(int32_t arg1)
{
    void* eax = sub_401806(arg1);
    int32_t eax_1 = -(eax);
    return -((eax_1 - eax_1)) - 1;
}

int32_t sub_401885()
{
    void* eax = sub_401e01(0x80);
    data_409ab0 = eax;
    
    if (!eax)
    {
        sub_4019b5(0x18);
        eax = data_409ab0;
    }
    
    *eax = 0;
    int32_t result = data_409ab0;
    data_409aac = result;
    return result;
}

int32_t sub_4018b4(int32_t arg1)
{
    int32_t var_4 = arg1;
    int32_t* ecx;
    return sub_401eb3(ecx);
}

int32_t _start()
{
    int32_t ebp;
    int32_t var_4 = ebp;
    int32_t var_8 = 0xffffffff;
    int32_t var_c = 0x406128;
    int32_t var_10 = 0x4029a8;
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    fsbase->NtTib.ExceptionList = &ExceptionList;
    int32_t ebx;
    int32_t var_70 = ebx;
    int32_t esi;
    int32_t var_74 = esi;
    int32_t edi;
    int32_t var_78 = edi;
    int32_t* var_1c = &var_78;
    uint32_t eax_1 = GetVersion();
    int32_t edx;
    edx = *eax_1[1];
    data_4095c0 = edx;
    uint32_t ecx_1 = eax_1;
    data_4095bc = ecx_1;
    data_4095b8 = (ecx_1 << 8) + edx;
    data_4095b4 = eax_1 >> 0x10;
    
    if (!sub_402850(0))
    {
        sub_4019da(0x1c);
        /* no return */
    }
    
    int32_t var_8_1 = 0;
    sub_402530();
    data_409ab8 = GetCommandLineA();
    void* eax_5;
    int32_t ecx_5;
    eax_5 = sub_4023fe();
    data_40959c = eax_5;
    sub_4021b1(ecx_5);
    sub_4020f8();
    sub_401c9e();
    STARTUPINFOA startupInfo;
    startupInfo.dwFlags = 0;
    GetStartupInfoA(&startupInfo);
    char* eax_6 = sub_4020a0();
    uint32_t wShowWindow;
    
    wShowWindow = !(startupInfo.dwFlags & 1) ? 0xa : startupInfo.wShowWindow;
    
    uint32_t wShowWindow_1 = wShowWindow;
    char* var_80 = eax_6;
    uint32_t eax_8 = sub_401040(GetModuleHandleA(nullptr), SW_HIDE);
    uint32_t var_64 = eax_8;
    sub_401ccb(eax_8);
    int32_t* var_18;
    int32_t ecx_7 = **var_18;
    int32_t var_6c = ecx_7;
    return sub_401f1c(ecx_7, var_18);
}

int32_t sub_4019b5(uint32_t arg1)
{
    if (data_4095a4 == 1)
        sub_402a80();
    
    sub_402ab9(arg1);
    return data_4070c4(0xff);
}

void sub_4019da(uint32_t arg1) __noreturn
{
    if (data_4095a4 == 1)
        sub_402a80();
    
    sub_402ab9(arg1);
    ExitProcess(0xff);
    /* no return */
}

void* __convention("regparm") sub_4019fe(int32_t arg1, int32_t arg2, int32_t* arg3, char* arg4)
{
    int32_t* var_8 = arg3;
    uint32_t dwBytes_1;
    
    if (!arg4)
        return sub_401e01(dwBytes_1);
    
    uint32_t dwBytes = dwBytes_1;
    
    if (dwBytes)
    {
        int32_t eax_1 = data_409988;
        char* edi_1;
        
        if (eax_1 == 3)
        {
            while (true)
            {
                edi_1 = nullptr;
                
                if (dwBytes <= 0xffffffe0)
                {
                    void* ebx_1 = sub_402c54(arg4);
                    
                    if (!ebx_1)
                    {
                    label_401af7:
                        
                        if (!dwBytes)
                            dwBytes = 1;
                        
                        dwBytes = (dwBytes + 0xf) & 0xfffffff0;
                        edi_1 = HeapReAlloc(data_409984, HEAP_NONE, arg4, dwBytes);
                    }
                    else
                    {
                        if (dwBytes > data_409980)
                        {
                        label_401ab0:
                            
                            if (!dwBytes)
                                dwBytes = 1;
                            
                            dwBytes = (dwBytes + 0xf) & 0xfffffff0;
                            edi_1 = HeapAlloc(data_409984, HEAP_NONE, dwBytes);
                            
                            if (edi_1)
                            {
                                uint32_t dwBytes_3 = *(arg4 - 4) - 1;
                                
                                if (dwBytes_3 >= dwBytes)
                                    dwBytes_3 = dwBytes;
                                
                                sub_403e40(edi_1, arg4, dwBytes_3);
                                sub_402c7f(ebx_1, arg4);
                            }
                        }
                        else
                        {
                            edi_1 = arg4;
                            
                            if (!sub_40345d(ebx_1, edi_1, dwBytes))
                            {
                                edi_1 = sub_402fa8(dwBytes);
                                
                                if (!edi_1)
                                    goto label_401ab0;
                                
                                uint32_t dwBytes_2 = *(arg4 - 4) - 1;
                                
                                if (dwBytes_2 >= dwBytes)
                                    dwBytes_2 = dwBytes;
                                
                                sub_403e40(edi_1, arg4, dwBytes_2);
                                ebx_1 = sub_402c54(arg4);
                                sub_402c7f(ebx_1, arg4);
                                goto label_401aaa;
                            }
                            
                        label_401aaa:
                            
                            if (!edi_1)
                                goto label_401ab0;
                        }
                        
                        if (!ebx_1)
                            goto label_401af7;
                    }
                    
                    if (edi_1)
                        return edi_1;
                }
                
                if (!data_40970c)
                    return edi_1;
                
                if (!sub_403e20(dwBytes))
                    return nullptr;
            }
        }
        else if (eax_1 != 2)
        {
            while (true)
            {
                void* eax = nullptr;
                
                if (dwBytes <= 0xffffffe0)
                {
                    if (!dwBytes)
                        dwBytes = 1;
                    
                    dwBytes = (dwBytes + 0xf) & 0xfffffff0;
                    eax = HeapReAlloc(data_409984, HEAP_NONE, arg4, dwBytes);
                    
                    if (eax)
                        return eax;
                }
                
                if (!data_40970c)
                    return eax;
                
                if (!sub_403e20(dwBytes))
                    return nullptr;
            }
        }
        else
        {
            if (dwBytes <= 0xffffffe0)
            {
                dwBytes = dwBytes <= 0 ? 0x10 : (dwBytes + 0xf) & 0xfffffff0;
            }
            
            bool cond:3_1;
            
            do
            {
                edi_1 = nullptr;
                
                if (dwBytes > 0xffffffe0)
                {
                label_401c3b:
                    
                    if (!data_40970c)
                        return edi_1;
                }
                else
                {
                    char* eax_11 = sub_4039af(arg4, &var_8, &dwBytes_1);
                    
                    if (!eax_11)
                        edi_1 = HeapReAlloc(data_409984, HEAP_NONE, arg4, dwBytes);
                    else
                    {
                        if (dwBytes < data_40922c)
                        {
                            uint32_t edi_3 = dwBytes >> 4;
                            
                            if (!sub_403d77(var_8, dwBytes_1, eax_11, edi_3))
                            {
                                edi_1 = sub_403a4b(edi_3);
                                
                                if (edi_1)
                                {
                                    uint32_t dwBytes_4 = *eax_11 << 4;
                                    
                                    if (dwBytes_4 >= dwBytes)
                                        dwBytes_4 = dwBytes;
                                    
                                    sub_403e40(edi_1, arg4, dwBytes_4);
                                    sub_403a06(var_8, dwBytes_1, eax_11);
                                    goto label_401bdb;
                                }
                            }
                            else
                            {
                                edi_1 = arg4;
                            label_401bdb:
                                
                                if (edi_1)
                                    return edi_1;
                            }
                        }
                        
                        edi_1 = HeapAlloc(data_409984, HEAP_NONE, dwBytes);
                        
                        if (!edi_1)
                            goto label_401c3b;
                        
                        uint32_t dwBytes_5 = *eax_11 << 4;
                        
                        if (dwBytes_5 >= dwBytes)
                            dwBytes_5 = dwBytes;
                        
                        sub_403e40(edi_1, arg4, dwBytes_5);
                        sub_403a06(var_8, dwBytes_1, eax_11);
                    }
                    
                    if (edi_1 || !data_40970c)
                        return edi_1;
                }
                
                cond:3_1 = sub_403e20(dwBytes);
            } while (cond:3_1);
        }
    }
    else
    {
        char* var_18_2 = arg4;
        sub_401eb3(arg3);
    }
    
    return nullptr;
}

int32_t sub_401c9e()
{
    int32_t eax_1 = data_409ab4;
    
    if (eax_1)
        eax_1();
    
    sub_401d86(0x40700c, 0x407018);
    return sub_401d86(0x407000, 0x407008);
}

int32_t sub_401ccb(uint32_t arg1)
{
    return sub_401ced(arg1, 0, 0);
}

int32_t sub_401cdc(uint32_t arg1)
{
    return sub_401ced(arg1, 1, 0);
}

int32_t sub_401ced(uint32_t arg1, int32_t arg2, int32_t arg3)
{
    if (data_4095f0 == 1)
    {
        TerminateProcess(GetCurrentProcess(), arg1);
        /* no return */
    }
    
    data_4095ec = 1;
    data_4095e8 = arg3;
    
    if (!arg2)
    {
        int32_t eax_2 = data_409ab0;
        
        if (eax_2)
        {
            int32_t* i = data_409aac - 4;
            
            if (i >= eax_2)
            {
                do
                {
                    int32_t eax_3 = *i;
                    
                    if (eax_3)
                        eax_3();
                    
                    i -= 4;
                } while (i >= data_409ab0);
            }
        }
        
        sub_401d86(0x40701c, 0x407020);
    }
    
    int32_t result = sub_401d86(0x407024, 0x407028);
    
    if (arg3)
        return result;
    
    data_4095f0 = 1;
    ExitProcess(arg1);
    /* no return */
}

void sub_401d86(int32_t* arg1, int32_t arg2)
{
    for (int32_t* i = arg1; i < arg2; i = &i[1])
    {
        int32_t eax = *i;
        
        if (eax)
            eax();
    }
}

uint32_t __convention("regparm") sub_401da0(int32_t arg1, int32_t arg2, int32_t* arg3, void* arg4)
{
    int32_t* var_8 = arg3;
    int32_t* var_c = arg3;
    int32_t eax = data_409988;
    void* lpMem;
    
    if (eax != 3)
    {
        if (eax == 2)
        {
            char* eax_4 = sub_4039af(arg4, &var_c, &var_8);
            
            if (eax_4)
                return *eax_4 << 4;
        }
        
        lpMem = arg4;
    }
    else
    {
        if (sub_402c54(arg4))
            return *(arg4 - 4) - 9;
        
        lpMem = arg4;
    }
    
    return HeapSize(data_409984, HEAP_NONE, lpMem);
}

void* sub_401e01(int32_t* arg1)
{
    return sub_401e13(arg1, data_40970c);
}

void* sub_401e13(int32_t* arg1, int32_t arg2)
{
    if (arg1 <= 0xffffffe0)
    {
        bool cond:1_1;
        
        do
        {
            void* result = sub_401e3f(arg1);
            
            if (result || arg2 == result)
                return result;
            
            cond:1_1 = sub_403e20(arg1);
        } while (cond:1_1);
    }
    
    return nullptr;
}

void* sub_401e3f(int32_t* arg1)
{
    int32_t eax_5 = data_409988;
    int32_t* esi = arg1;
    
    if (eax_5 == 3)
    {
        if (esi <= data_409980)
        {
            void* eax = sub_402fa8(esi);
            
            if (eax)
                return eax;
        }
        
        goto label_401e97;
    }
    
    void* dwBytes;
    
    if (eax_5 != 2)
    {
    label_401e97:
        
        if (!esi)
            esi = 1;
        
        dwBytes = (esi + 0xf) & 0xfffffff0;
    }
    else
    {
        dwBytes = !arg1 ? 0x10 : (arg1 + 0xf) & 0xfffffff0;
        
        if (dwBytes <= data_40922c)
        {
            void* eax_4 = sub_403a4b(dwBytes >> 4);
            
            if (eax_4)
                return eax_4;
        }
    }
    
    return HeapAlloc(data_409984, HEAP_NONE, dwBytes);
}

void __fastcall sub_401eb3(int32_t* arg1)
{
    int32_t* var_8 = arg1;
    int32_t* lpMem_1;
    int32_t* lpMem = lpMem_1;
    
    if (!lpMem)
        return;
    
    int32_t eax_1 = data_409988;
    
    if (eax_1 != 3)
    {
        char* eax_3;
        
        if (eax_1 == 2)
            eax_3 = sub_4039af(lpMem, &var_8, &lpMem_1);
        
        if (eax_1 != 2 || !eax_3)
            HeapFree(data_409984, HEAP_NONE, lpMem);
        else
            sub_403a06(var_8, lpMem_1, eax_3);
    }
    else
    {
        void* eax_2 = sub_402c54(lpMem);
        
        if (!eax_2)
            HeapFree(data_409984, HEAP_NONE, lpMem);
        else
            sub_402c7f(eax_2, lpMem);
    }
}

int32_t sub_401f1c(int32_t arg1, EXCEPTION_POINTERS* arg2)
{
    int32_t* eax = sub_40205d(arg1);
    
    if (eax)
    {
        int32_t ebx_1 = eax[2];
        
        if (ebx_1)
        {
            if (ebx_1 == 5)
            {
                eax[2] = 0;
                return 1;
            }
            
            if (ebx_1 != 1)
            {
                int32_t ecx_1 = data_4095f4;
                data_4095f4 = arg2;
                int32_t ecx_3 = eax[1];
                
                if (ecx_3 != 8)
                {
                    eax[2] = 0;
                    ebx_1(ecx_3);
                }
                else
                {
                    int32_t ecx_4 = data_407148;
                    int32_t edx_2 = data_40714c + ecx_4;
                    
                    if (ecx_4 < edx_2)
                    {
                        int32_t i_1 = edx_2 - ecx_4;
                        void* esi_2 = ecx_4 * 0xc + &data_4070d8;
                        int32_t i;
                        
                        do
                        {
                            *esi_2 = 0;
                            esi_2 += 0xc;
                            i = i_1;
                            i_1 -= 1;
                        } while (i != 1);
                    }
                    
                    int32_t esi_3 = data_407154;
                    
                    switch (*eax)
                    {
                        case 0xc000008d:
                        {
                            data_407154 = 0x82;
                            break;
                        }
                        case 0xc000008e:
                        {
                            data_407154 = 0x83;
                            break;
                        }
                        case 0xc000008f:
                        {
                            data_407154 = 0x86;
                            break;
                        }
                        case 0xc0000090:
                        {
                            data_407154 = 0x81;
                            break;
                        }
                        case 0xc0000091:
                        {
                            data_407154 = 0x84;
                            break;
                        }
                        case 0xc0000092:
                        {
                            data_407154 = 0x8a;
                            break;
                        }
                        case 0xc0000093:
                        {
                            data_407154 = 0x85;
                            break;
                        }
                    }
                    
                    ebx_1(8, data_407154);
                    data_407154 = esi_3;
                }
                
                data_4095f4 = ecx_1;
            }
            
            return 0xffffffff;
        }
    }
    
    return UnhandledExceptionFilter(arg2);
}

int32_t* sub_40205d(int32_t arg1)
{
    int32_t ecx = data_407150;
    int32_t* result = &data_4070d0;
    
    if (data_4070d0 != arg1)
    {
        do
        {
            result = &result[3];
            
            if (result >= &(&data_4070d0)[ecx * 3])
                break;
        } while (*result != arg1);
    }
    
    if (result < &(&data_4070d0)[ecx * 3] && *result == arg1)
        return result;
    
    return nullptr;
}

char* sub_4020a0()
{
    if (!data_409aa8)
        sub_40457b();
    
    char* result = data_409ab8;
    int32_t eax;
    eax = *result;
    
    if (eax == 0x22)
    {
        while (true)
        {
            eax = result[1];
            result = &result[1];
            
            if (eax == 0x22)
                break;
            
            if (!eax)
                break;
            
            if (sub_404175(eax))
                result = &result[1];
        }
        
        if (*result == 0x22)
            goto label_4020dd;
    }
    else if (eax > 0x20)
    {
        do
            result = &result[1];
         while (*result > 0x20);
    }
    
    while (true)
    {
        eax = *result;
        
        if (!eax || eax > 0x20)
            return result;
        
    label_4020dd:
        result = &result[1];
    }
}

int32_t sub_4020f8()
{
    if (!data_409aa8)
        sub_40457b();
    
    char* esi = data_40959c;
    int32_t edi = 0;
    
    while (true)
    {
        void* eax;
        eax = *esi;
        
        if (!eax)
            break;
        
        if (eax != 0x3d)
            edi += 1;
        
        esi = esi + sub_404690(esi) + 1;
    }
    
    void* esi_1 = sub_401e01((edi << 2) + 4);
    void* ecx_2 = (edi << 2) + 4;
    data_4095d0 = esi_1;
    
    if (!esi_1)
    {
        sub_4019b5(9);
        ecx_2 = 9;
    }
    
    char* edi_1 = data_40959c;
    
    while (*edi_1)
    {
        ecx_2 = edi_1;
        void* ebp_2 = sub_404690(edi_1) + 1;
        
        if (*edi_1 != 0x3d)
        {
            void* eax_4 = sub_401e01(ebp_2);
            *esi_1 = eax_4;
            
            if (!eax_4)
                sub_4019b5(9);
            
            void* var_14_3 = edi_1;
            sub_4045a0(*esi_1, var_14_3);
            esi_1 += 4;
            ecx_2 = var_14_3;
        }
        
        edi_1 += ebp_2;
    }
    
    int32_t __saved_ebp_3 = data_40959c;
    int32_t result = sub_401eb3(ecx_2);
    data_40959c = 0;
    *esi_1 = 0;
    data_409aa4 = 1;
    return result;
}

int32_t __fastcall sub_4021b1(int32_t arg1)
{
    int32_t var_8 = arg1;
    int32_t var_c = arg1;
    
    if (!data_409aa8)
        sub_40457b();
    
    GetModuleFileNameA(nullptr, &data_4095f8, 0x104);
    char* eax = data_409ab8;
    data_4095e0 = 0x4095f8;
    char* edi = &data_4095f8;
    
    if (*eax)
        edi = eax;
    
    sub_40224a(edi, nullptr, nullptr, &var_8, &var_c);
    void* eax_3 = sub_401e01(var_c + (var_8 << 2));
    
    if (!eax_3)
        sub_4019b5(8);
    
    sub_40224a(edi, eax_3, eax_3 + (var_8 << 2), &var_8, &var_c);
    int32_t result = var_8 - 1;
    data_4095c8 = eax_3;
    data_4095c4 = result;
    return result;
}

int32_t* sub_40224a(char* arg1, int32_t* arg2, char* arg3, int32_t* arg4, int32_t* arg5)
{
    int32_t* ecx = arg5;
    *ecx = 0;
    char* esi = arg3;
    int32_t* edi = arg2;
    *arg4 = 1;
    char* eax_1 = arg1;
    
    if (edi)
    {
        *edi = esi;
        edi = &edi[1];
        arg2 = edi;
    }
    
    uint32_t edx;
    
    if (*eax_1 != 0x22)
    {
        while (true)
        {
            *ecx += 1;
            
            if (esi)
            {
                edx = *eax_1;
                *esi = edx;
                esi = &esi[1];
            }
            
            edx = *eax_1;
            eax_1 = &eax_1[1];
            
            if (*(edx + 0x409861) & 4)
            {
                *ecx += 1;
                
                if (esi)
                {
                    uint32_t ebx_1;
                    ebx_1 = *eax_1;
                    *esi = ebx_1;
                    esi = &esi[1];
                }
                
                eax_1 = &eax_1[1];
            }
            
            if (edx == 0x20)
            {
            label_4022f1:
                
                if (edx)
                {
                    if (esi)
                        esi[0xffffffff] = 0;
                    
                    break;
                }
            }
            else if (edx)
            {
                if (edx == 9)
                    goto label_4022f1;
                
                continue;
            }
            
            eax_1 -= 1;
            break;
        }
    }
    else
    {
        while (true)
        {
            edx = eax_1[1];
            eax_1 = &eax_1[1];
            
            if (edx == 0x22)
                break;
            
            if (!edx)
                break;
            
            if (*(edx + 0x409861) & 4)
            {
                *ecx += 1;
                
                if (esi)
                {
                    edx = *eax_1;
                    *esi = edx;
                    esi = &esi[1];
                    eax_1 = &eax_1[1];
                }
            }
            
            *ecx += 1;
            
            if (esi)
            {
                edx = *eax_1;
                *esi = edx;
                esi = &esi[1];
            }
        }
        
        *ecx += 1;
        
        if (esi)
        {
            *esi = 0;
            esi = &esi[1];
        }
        
        if (*eax_1 == 0x22)
            eax_1 = &eax_1[1];
    }
    
    arg5 = nullptr;
    
    while (*eax_1)
    {
        while (true)
        {
            edx = *eax_1;
            
            if (edx != 0x20 && edx != 9)
                break;
            
            eax_1 = &eax_1[1];
        }
        
        if (!*eax_1)
            break;
        
        if (edi)
        {
            *edi = esi;
            edi = &edi[1];
            arg2 = edi;
        }
        
        *arg4 += 1;
        
        while (true)
        {
            arg1 = 1;
            uint32_t i_2 = 0;
            
            while (*eax_1 == 0x5c)
            {
                eax_1 = &eax_1[1];
                i_2 += 1;
            }
            
            if (*eax_1 == 0x22)
            {
                if (!(i_2 & 1))
                {
                    if (!arg5 || eax_1[1] != 0x22)
                        arg1 = nullptr;
                    else
                        eax_1 = &eax_1[1];
                    
                    edi = arg2;
                    int32_t* edx_3;
                    edx_3 = !arg5;
                    arg5 = edx_3;
                }
                
                i_2 u>>= 1;
            }
            
            if (i_2)
            {
                int32_t i_1 = i_2;
                int32_t i;
                
                do
                {
                    if (esi)
                    {
                        *esi = 0x5c;
                        esi = &esi[1];
                    }
                    
                    *ecx += 1;
                    i = i_1;
                    i_1 -= 1;
                } while (i != 1);
            }
            
            edx = *eax_1;
            
            if (!edx)
                break;
            
            if (!arg5)
            {
                if (edx == 0x20)
                    break;
                
                if (edx == 9)
                    break;
            }
            
            if (arg1)
            {
                if (esi)
                {
                    if (*(edx + 0x409861) & 4)
                    {
                        *esi = edx;
                        esi = &esi[1];
                        eax_1 = &eax_1[1];
                        *ecx += 1;
                    }
                    
                    edx = *eax_1;
                    *esi = edx;
                    esi = &esi[1];
                }
                else if (*(edx + 0x409861) & 4)
                {
                    eax_1 = &eax_1[1];
                    *ecx += 1;
                }
                
                *ecx += 1;
            }
            
            eax_1 = &eax_1[1];
        }
        
        if (esi)
        {
            *esi = 0;
            esi = &esi[1];
        }
        
        *ecx += 1;
    }
    
    if (edi)
        *edi = 0;
    
    *arg4 += 1;
    return arg4;
}

void* sub_4023fe()
{
    int32_t ecx;
    int32_t var_4 = ecx;
    int32_t var_8 = ecx;
    int32_t eax = data_4096fc;
    void* lpMultiByteStr_3 = nullptr;
    PWSTR esi = nullptr;
    char* penv = nullptr;
    
    if (!eax)
    {
        esi = GetEnvironmentStringsW();
        
        if (!esi)
        {
            penv = GetEnvironmentStrings();
            
            if (!penv)
                return nullptr;
            
            data_4096fc = 2;
        label_4024dd:
            
            if (!penv)
            {
                penv = GetEnvironmentStrings();
                
                if (!penv)
                    return nullptr;
            }
            
            char* penv_1 = penv;
            
            if (*penv)
            {
                while (true)
                {
                    penv_1 = &penv_1[1];
                    
                    if (!*penv_1)
                    {
                        penv_1 = &penv_1[1];
                        
                        if (!*penv_1)
                            break;
                    }
                }
            }
            
            void* esi_1 = sub_401e01(penv_1 - penv + 1);
            
            if (esi_1)
                sub_403e40(esi_1, penv, penv_1 - penv + 1);
            else
                esi_1 = nullptr;
            
            FreeEnvironmentStringsA(penv);
            return esi_1;
        }
        
        data_4096fc = 1;
    }
    else if (eax != 1)
    {
        if (eax != 2)
            return nullptr;
        
        goto label_4024dd;
    }
    
    if (!esi)
    {
        esi = GetEnvironmentStringsW();
        
        if (!esi)
            return nullptr;
    }
    
    PWSTR eax_4 = esi;
    
    if (*esi)
    {
        while (true)
        {
            eax_4 = &eax_4[1];
            
            if (!*eax_4)
            {
                eax_4 = &eax_4[1];
                
                if (!*eax_4)
                    break;
            }
        }
    }
    
    int32_t cbMultiByte =
        WideCharToMultiByte(0, 0, esi, ((eax_4 - esi) >> 1) + 1, nullptr, 0, nullptr, nullptr);
    
    if (cbMultiByte)
    {
        void* lpMultiByteStr = sub_401e01(cbMultiByte);
        void* lpMultiByteStr_1 = lpMultiByteStr;
        
        if (lpMultiByteStr)
        {
            int32_t eax_10;
            int32_t* ecx_2;
            eax_10 = WideCharToMultiByte(0, 0, esi, ((eax_4 - esi) >> 1) + 1, lpMultiByteStr, 
                cbMultiByte, nullptr, nullptr);
            
            if (!eax_10)
            {
                void* lpMultiByteStr_2 = lpMultiByteStr_1;
                sub_401eb3(ecx_2);
                lpMultiByteStr_1 = nullptr;
            }
            
            lpMultiByteStr_3 = lpMultiByteStr_1;
        }
    }
    
    FreeEnvironmentStringsW(esi);
    return lpMultiByteStr_3;
}

uint32_t sub_402530()
{
    void* esi = sub_401e01(0x100);
    
    if (!esi)
        sub_4019b5(0x1b);
    
    data_4099a0 = esi;
    data_409aa0 = 0x20;
    
    for (void* i = esi + 0x100; esi < i; i = data_4099a0 + 0x100)
    {
        *(esi + 4) = 0;
        *esi = 0xffffffff;
        *(esi + 5) = 0xa;
        esi += 8;
    }
    
    STARTUPINFOA startupInfo;
    GetStartupInfoA(&startupInfo);
    
    if (startupInfo.cbReserved2)
    {
        BYTE* lpReserved2 = startupInfo.lpReserved2;
        
        if (lpReserved2)
        {
            void* const i_1 = *lpReserved2;
            void* ebp_1 = &lpReserved2[4];
            void* ebx_1 = i_1 + ebp_1;
            
            if (i_1 >= 0x800)
                i_1 = 0x800;
            
            void* j;
            
            if (data_409aa0 < i_1)
            {
                void** edi_1 = &data_4099a4;
                
                do
                {
                    void* eax_2 = sub_401e01(0x100);
                    
                    if (!eax_2)
                    {
                        i_1 = data_409aa0;
                        break;
                    }
                    
                    data_409aa0 += 0x20;
                    *edi_1 = eax_2;
                    
                    for (j = eax_2 + 0x100; eax_2 < j; j = *edi_1 + 0x100)
                    {
                        *(eax_2 + 4) = 0;
                        *eax_2 = 0xffffffff;
                        *(eax_2 + 5) = 0xa;
                        eax_2 += 8;
                    }
                    
                    edi_1 = &edi_1[1];
                } while (data_409aa0 < i_1);
            }
            
            int32_t edi_2 = 0;
            
            if (i_1 > 0)
            {
                do
                {
                    HANDLE hFile = *ebx_1;
                    
                    if (hFile != 0xffffffff)
                    {
                        j = *ebp_1;
                        
                        if (j & 1)
                        {
                            if (j & 8)
                            {
                            label_40264a:
                                void** eax_7 = (&data_4099a0)[edi_2 >> 5] + ((edi_2 & 0x1f) << 3);
                                *eax_7 = *ebx_1;
                                j = *ebp_1;
                                eax_7[1] = j;
                            }
                            else
                            {
                                enum FILE_TYPE eax_3;
                                eax_3 = GetFileType(hFile);
                                
                                if (eax_3)
                                    goto label_40264a;
                            }
                        }
                    }
                    
                    edi_2 += 1;
                    ebp_1 += 1;
                    ebx_1 += 4;
                } while (edi_2 < i_1);
            }
        }
    }
    
    for (int32_t i_2 = 0; i_2 < 3; i_2 += 1)
    {
        int32_t eax_8 = data_4099a0;
        int32_t* esi_1 = eax_8 + (i_2 << 3);
        
        if (*(eax_8 + (i_2 << 3)) != 0xffffffff)
            esi_1[1] |= 0x80;
        else
        {
            esi_1[1] = 0x81;
            enum STD_HANDLE nStdHandle;
            
            if (i_2)
            {
                int32_t eax_11 = -((i_2 - 1));
                nStdHandle = eax_11 - eax_11 - 0xb;
            }
            else
                nStdHandle = STD_INPUT_HANDLE;
            
            HANDLE hFile_1 = GetStdHandle(nStdHandle);
            
            if (hFile_1 == 0xffffffff)
                esi_1[1] |= 0x40;
            else
            {
                enum FILE_TYPE eax_13 = GetFileType(hFile_1);
                
                if (!eax_13)
                    esi_1[1] |= 0x40;
                else
                {
                    int32_t eax_14 = eax_13 & 0xff;
                    *esi_1 = hFile_1;
                    
                    if (eax_14 == 2)
                        esi_1[1] |= 0x40;
                    else if (eax_14 == 3)
                        esi_1[1] |= 8;
                }
            }
        }
    }
    
    return SetHandleCount(data_409aa0);
}

HMODULE sub_4026db(int32_t* arg1)
{
    *arg1 = 0;
    HMODULE result = GetModuleHandleA(nullptr);
    
    if (result->unused == 0x5a4d)
    {
        int32_t result_1 = result;
        
        if (result_1)
        {
            result += result_1;
            result_1 = *(result + 0x1a);
            *arg1 = result_1;
            result = *(result + 0x1b);
            *(arg1 + 1) = result;
        }
    }
    
    return result;
}

int32_t sub_402708()
{
    sub_404ac0(0x122c);
    int32_t ebx;
    int32_t var_8 = ebx;
    OSVERSIONINFOA var_9c;
    var_9c.szCSDVersion[0x7c] = &var_9c;
    var_9c.dwOSVersionInfoSize = 0x94;
    
    if (GetVersionExA(var_9c.szCSDVersion[0x7c]) && var_9c.dwPlatformId == 2
        && var_9c.dwMajorVersion >= 5)
    {
        var_9c.szCSDVersion[0x7c] = 1;
        var_9c.szCSDVersion[0x7d] = 0;
        var_9c.szCSDVersion[0x7e] = 0;
        var_9c.szCSDVersion[0x7f] = 0;
        return 1;
    }
    
    var_9c.szCSDVersion[0x7c] = 0x90;
    var_9c.szCSDVersion[0x7d] = 0x10;
    var_9c.szCSDVersion[0x7e] = 0;
    var_9c.szCSDVersion[0x7f] = 0;
    char var_1230;
    var_9c.szCSDVersion[0x78] = &var_1230;
    var_9c.szCSDVersion[0x74] = 0x4c;
    var_9c.szCSDVersion[0x75] = 0x61;
    var_9c.szCSDVersion[0x76] = 0x40;
    var_9c.szCSDVersion[0x77] = 0;
    
    if (GetEnvironmentVariableA(var_9c.szCSDVersion[0x74], var_9c.szCSDVersion[0x78], 
        var_9c.szCSDVersion[0x7c]))
    {
        char* ecx_1 = &var_1230;
        
        if (var_1230)
        {
            do
            {
                uint32_t eax_1;
                eax_1 = *ecx_1;
                
                if (eax_1 >= 0x61 && eax_1 <= 0x7a)
                {
                    eax_1 -= 0x20;
                    *ecx_1 = eax_1;
                }
                
                ecx_1 = &ecx_1[1];
            } while (*ecx_1);
        }
        
        var_9c.szCSDVersion[0x7c] = 0x16;
        var_9c.szCSDVersion[0x7d] = 0;
        var_9c.szCSDVersion[0x7e] = 0;
        var_9c.szCSDVersion[0x7f] = 0;
        var_9c.szCSDVersion[0x78] = &var_1230;
        var_9c.szCSDVersion[0x74] = 0x34;
        var_9c.szCSDVersion[0x75] = 0x61;
        var_9c.szCSDVersion[0x76] = 0x40;
        var_9c.szCSDVersion[0x77] = 0;
        char* eax_3;
        
        if (sub_404a80(var_9c.szCSDVersion[0x74], var_9c.szCSDVersion[0x78], 
            var_9c.szCSDVersion[0x7c]))
        {
            var_9c.szCSDVersion[0x7c] = 4;
            var_9c.szCSDVersion[0x7d] = 1;
            var_9c.szCSDVersion[0x7e] = 0;
            var_9c.szCSDVersion[0x7f] = 0;
            uint8_t var_1a0[0x104];
            var_9c.szCSDVersion[0x78] = &var_1a0;
            var_9c.szCSDVersion[0x74] = 0;
            var_9c.szCSDVersion[0x75] = 0;
            var_9c.szCSDVersion[0x76] = 0;
            var_9c.szCSDVersion[0x77] = 0;
            GetModuleFileNameA(var_9c.szCSDVersion[0x74], var_9c.szCSDVersion[0x78], 
                var_9c.szCSDVersion[0x7c]);
            uint8_t (* ecx_2)[0x104] = &var_1a0;
            
            if (var_1a0[0])
            {
                do
                {
                    uint8_t eax_4 = *ecx_2;
                    
                    if (eax_4 >= 0x61 && eax_4 <= 0x7a)
                        *ecx_2 = eax_4 - 0x20;
                    
                    ecx_2 = &(*ecx_2)[1];
                } while (*ecx_2);
            }
            
            var_9c.szCSDVersion[0x7c] = &var_1a0;
            var_9c.szCSDVersion[0x78] = &var_1230;
            eax_3 = sub_404a00(var_9c.szCSDVersion[0x78], var_9c.szCSDVersion[0x7c]);
            int32_t ecx_3;
            ecx_3 = var_9c.szCSDVersion[0x7c];
            *ecx_3[1] = var_9c.szCSDVersion[0x7d];
            *ecx_3[2] = var_9c.szCSDVersion[0x7e];
            *ecx_3[3] = var_9c.szCSDVersion[0x7f];
        }
        else
            eax_3 = &var_1230;
        
        if (eax_3)
        {
            var_9c.szCSDVersion[0x7c] = 0x2c;
            var_9c.szCSDVersion[0x7d] = 0;
            var_9c.szCSDVersion[0x7e] = 0;
            var_9c.szCSDVersion[0x7f] = 0;
            var_9c.szCSDVersion[0x78] = eax_3;
            var_9c.szCSDVersion[0x79] = *eax_3[1];
            var_9c.szCSDVersion[0x7a] = *eax_3[2];
            var_9c.szCSDVersion[0x7b] = *eax_3[3];
            void* eax_5 = sub_404940(var_9c.szCSDVersion[0x78], var_9c.szCSDVersion[0x7c]);
            int32_t ecx_4;
            ecx_4 = var_9c.szCSDVersion[0x7c];
            *ecx_4[1] = var_9c.szCSDVersion[0x7d];
            *ecx_4[2] = var_9c.szCSDVersion[0x7e];
            *ecx_4[3] = var_9c.szCSDVersion[0x7f];
            
            if (eax_5)
            {
                void* ecx_5 = eax_5 + 1;
                
                if (*(eax_5 + 1))
                {
                    do
                    {
                        if (*ecx_5 != 0x3b)
                            ecx_5 += 1;
                        else
                            *ecx_5 = 0;
                    } while (*ecx_5);
                }
                
                var_9c.szCSDVersion[0x7c] = 0xa;
                var_9c.szCSDVersion[0x7d] = 0;
                var_9c.szCSDVersion[0x7e] = 0;
                var_9c.szCSDVersion[0x7f] = 0;
                var_9c.szCSDVersion[0x78] = 0;
                var_9c.szCSDVersion[0x79] = 0;
                var_9c.szCSDVersion[0x7a] = 0;
                var_9c.szCSDVersion[0x7b] = 0;
                var_9c.szCSDVersion[0x74] = (eax_5 + 1);
                var_9c.szCSDVersion[0x75] = *(eax_5 + 1)[1];
                var_9c.szCSDVersion[0x76] = *(eax_5 + 1)[2];
                var_9c.szCSDVersion[0x77] = *(eax_5 + 1)[3];
                int32_t result = sub_40470b(var_9c.szCSDVersion[0x74], var_9c.szCSDVersion[0x78], 
                    var_9c.szCSDVersion[0x7c]);
                
                if (result == 2 || result == 3 || result == 1)
                    return result;
            }
        }
    }
    
    var_9c.szCSDVersion[0x7c] = &var_8;
    HMODULE eax_7 = sub_4026db(var_9c.szCSDVersion[0x7c]);
    int32_t ecx_6;
    ecx_6 = var_9c.szCSDVersion[0x7c];
    *ecx_6[1] = var_9c.szCSDVersion[0x7d];
    *ecx_6[2] = var_9c.szCSDVersion[0x7e];
    *ecx_6[3] = var_9c.szCSDVersion[0x7f];
    return eax_7 - eax_7 + 3;
}

int32_t sub_402850(int32_t arg1)
{
    enum HEAP_FLAGS flOptions;
    flOptions = !arg1;
    HANDLE eax = HeapCreate(flOptions, 0x1000, 0);
    data_409984 = eax;
    
    if (eax)
    {
        int32_t eax_1 = sub_402708();
        data_409988 = eax_1;
        void** eax_2;
        
        if (eax_1 != 3)
        {
            if (eax_1 != 2)
                return 1;
            
            eax_2 = sub_403753();
        }
        else
            eax_2 = sub_402c0c(0x3f8);
        
        if (eax_2)
            return 1;
        
        HeapDestroy(data_409984);
    }
    
    return 0;
}

int32_t sub_4028b0(int32_t arg1)
{
    int32_t ebp;
    int32_t var_4 = ebp;
    int32_t result = RtlUnwind(arg1, 0x4028c8, nullptr, nullptr);
    var_4;
    return result;
}

int32_t sub_4028d0(int32_t arg1, int32_t arg2, int32_t* arg3)
{
    if (!(*(arg1 + 4) & 6))
        return 1;
    
    *arg3 = arg2;
    return 3;
}

void* sub_4028f2(void* arg1, int32_t arg2)
{
    void* var_10 = arg1;
    int32_t var_14 = 0xfffffffe;
    int32_t (* var_18)(int32_t arg1, int32_t arg2, int32_t* arg3) = sub_4028d0;
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    fsbase->NtTib.ExceptionList = &ExceptionList;
    
    while (true)
    {
        int32_t ebx_1 = *(arg1 + 8);
        int32_t esi_1 = *(arg1 + 0xc);
        
        if (esi_1 == 0xffffffff || esi_1 == arg2)
        {
            fsbase->NtTib.ExceptionList = ExceptionList;
            return arg1;
        }
        
        int32_t esi_2 = esi_1 * 3;
        int32_t ecx_1 = *(ebx_1 + (esi_2 << 2));
        int32_t var_14_1 = ecx_1;
        *(arg1 + 0xc) = ecx_1;
        
        if (!*(ebx_1 + (esi_2 << 2) + 4))
        {
            int32_t var_20_1 = 0x101;
            void* ebp;
            sub_402986(*(ebx_1 + (esi_2 << 2) + 8), ebp);
            (*(ebx_1 + (esi_2 << 2) + 8))();
        }
    }
}

int32_t __abnormal_termination()
{
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    
    if (ExceptionList->Handler == sub_4028d0
            && *(ExceptionList + 8) == *(*(ExceptionList + 0xc) + 0xc))
        return 1;
    
    return 0;
}

void __convention("regparm") sub_402986(int32_t arg1, void* arg2 @ ebp)
{
    data_40716c = *(arg2 + 8);
    data_407168 = arg1;
    data_407170 = arg2;
}

int32_t __convention("regparm") $$000000(char* arg1, int16_t arg2, void* arg3, void* arg4, int32_t arg5)
{
    char* esi;
    char* var_4 = esi;
    *arg2[1] ^= *arg1;
    char* eax = var_4;
    *eax ^= *arg2[1];
    char* ebp;
    var_4 = ebp;
    char** ebp_1 = &var_4;
    int32_t entry_ebx;
    int32_t var_10 = entry_ebx + 2;
    char* var_14 = esi;
    char** var_1c = &var_4;
    void* ebx_2 = arg4;
    int32_t result;
    
    if (*(arg3 + 4) & 6)
    {
        char** var_20_5 = &var_4;
        sub_4028f2(ebx_2, 0xffffffff);
        result = 1;
    }
    else
    {
        void* var_c = arg3;
        int32_t var_8_1 = arg5;
        *(ebx_2 - 4) = &var_c;
        int32_t esi_1 = *(ebx_2 + 0xc);
        int32_t edi_1 = *(ebx_2 + 8);
        
        while (true)
        {
            if (esi_1 == 0xffffffff)
            {
                result = 1;
                break;
            }
            
            int32_t ecx_1 = esi_1 * 3;
            
            if (*(edi_1 + (ecx_1 << 2) + 4))
            {
                int32_t eax_3 = (*(edi_1 + (ecx_1 << 2) + 4))(ebp_1, esi_1, var_1c);
                ebx_2 = ebp_1[3];
                
                if (eax_3)
                {
                    if (eax_3 < 0)
                    {
                        result = 0;
                        break;
                    }
                    
                    int32_t edi_2 = *(ebx_2 + 8);
                    sub_4028b0(ebx_2);
                    ebp_1 = ebx_2 + 0x10;
                    sub_4028f2(ebx_2, esi_1);
                    int32_t ecx_2 = esi_1 * 3;
                    int32_t var_20_4 = 1;
                    sub_402986(*(edi_2 + (ecx_2 << 2) + 8), ebp_1);
                    *(ebx_2 + 0xc) = *(edi_2 + (ecx_2 << 2));
                    (*(edi_2 + (ecx_2 << 2) + 8))();
                }
            }
            
            edi_1 = *(ebx_2 + 8);
            esi_1 = *(edi_1 + esi_1 * 0xc);
        }
    }
    
    *var_1c;
    return result;
}

void* __stdcall __seh_longjmp_unwind@4(int32_t* arg1)
{
    *arg1;
    return sub_4028f2(arg1[6], arg1[7]);
}

void* sub_402a80()
{
    void* result = data_4095a4;
    
    if (result == 1)
    {
    label_402a9c:
        sub_402ab9(0xfc);
        int32_t eax = data_409700;
        
        if (eax)
            eax();
        
        result = sub_402ab9(0xff);
    }
    else if (!result && data_4070c8 == 1)
        goto label_402a9c;
    
    return result;
}

void* sub_402ab9(uint32_t arg1)
{
    uint32_t i = arg1;
    int32_t ecx = 0;
    void* result = &data_407178;
    
    while (i != *result)
    {
        result += 8;
        ecx += 1;
        
        if (result >= &data_407208)
            break;
    }
    
    if (i == *((ecx << 3) + &data_407178))
    {
        result = data_4095a4;
        
        if (result == 1)
        {
        label_402bec:
            OVERLAPPED* __saved_edi_3 = nullptr;
            uint32_t* lpNumberOfBytesWritten = &arg1;
            uint32_t nNumberOfBytesToWrite = sub_404690((&data_40717c)[ecx * 2]);
            return WriteFile(GetStdHandle(STD_ERROR_HANDLE), (&data_40717c)[ecx * 2], 
                nNumberOfBytesToWrite, lpNumberOfBytesWritten, __saved_edi_3);
        }
        
        if (!result && data_4070c8 == 1)
            goto label_402bec;
        
        if (i != 0xfc)
        {
            void filename;
            
            if (!GetModuleFileNameA(nullptr, &filename, 0x104))
                sub_4045a0(&filename, "<program name unknown>");
            
            void* edi_1 = &filename;
            
            if (sub_404690(&filename) + 1 > 0x3c)
            {
                int32_t var_1b8_2 = 3;
                void var_1e3;
                edi_1 = sub_404690(&filename) + &var_1e3;
                sub_404b80(edi_1, "...", var_1b8_2);
            }
            
            char var_a4[0xa0];
            sub_4045a0(&var_a4, "Runtime Error!\n\nProgram: ");
            sub_4045b0(&var_a4, edi_1);
            sub_4045b0(&var_a4, "\n\n");
            sub_4045b0(&var_a4, (&data_40717c)[ecx * 2]);
            return sub_404aef(&var_a4, "Microsoft Visual C++ Runtime Library", 0x12010);
        }
    }
    
    return result;
}

int32_t sub_402c0c(int32_t arg1)
{
    int32_t result = HeapAlloc(data_409984, HEAP_NONE, 0x140);
    data_40997c = result;
    
    if (!result)
        return result;
    
    data_409974 = 0;
    data_409978 = 0;
    data_409970 = result;
    data_409980 = arg1;
    data_409968 = 0x10;
    return 1;
}

void* sub_402c54(int32_t arg1)
{
    void* result = data_40997c;
    void* ecx_1 = result + data_409978 * 0x14;
    
    while (true)
    {
        if (result >= ecx_1)
            return nullptr;
        
        if (arg1 - *(result + 0xc) < 0x100000)
            break;
        
        result += 0x14;
    }
    
    return result;
}

int32_t* sub_402c7f(void* arg1, int32_t* arg2)
{
    int32_t* result = *(arg1 + 0x10);
    int32_t* esi_1 = &arg2[-1];
    uint32_t edi_2 = (arg2 - *(arg1 + 0xc)) >> 0xf;
    int32_t* ecx_5 = *esi_1 - 1;
    int32_t* var_8 = ecx_5;
    
    if (!(ecx_5 & 1))
    {
        int32_t edx_1 = *(ecx_5 + esi_1);
        int32_t* ebx_1 = ecx_5 + esi_1;
        int32_t edx_2 = esi_1[-1];
        arg2 = ebx_1;
        
        if (!(edx_1 & 1))
        {
            int32_t edx_5 = (edx_1 >> 4) - 1;
            
            if (edx_5 > 0x3f)
                edx_5 = 0x3f;
            
            int32_t* ecx_13;
            
            if (ebx_1[1] != ebx_1[2])
                ecx_13 = var_8;
            else
            {
                if (edx_5 >= 0x20)
                {
                    uint32_t ebx_5 = ~(0x80000000 >> (edx_5 - 0x20));
                    result[edi_2 + 0x31] &= ebx_5;
                    char temp0_1 = *(edx_5 + result + 4);
                    *(edx_5 + result + 4) -= 1;
                    
                    if (temp0_1 == 1)
                        *(arg1 + 4) &= ebx_5;
                }
                else
                {
                    uint32_t ebx_3 = ~(0x80000000 >> edx_5);
                    result[edi_2 + 0x11] &= ebx_3;
                    char temp1_1 = *(edx_5 + result + 4);
                    *(edx_5 + result + 4) -= 1;
                    
                    if (temp1_1 == 1)
                    {
                        int32_t* ecx_9 = arg1;
                        *ecx_9 &= ebx_3;
                    }
                }
                
                ecx_13 = var_8;
                ebx_1 = arg2;
            }
            
            ecx_5 = ecx_13 + edx_1;
            *(ebx_1[2] + 4) = ebx_1[1];
            var_8 = ecx_5;
            *(arg2[1] + 8) = arg2[2];
        }
        
        int32_t edx_11 = (ecx_5 >> 4) - 1;
        
        if (edx_11 > 0x3f)
            edx_11 = 0x3f;
        
        int32_t ebx_9 = edx_2 & 1;
        void* ebx_12;
        
        if (ebx_9)
            ebx_12 = arg1;
        else
        {
            arg2 = esi_1 - edx_2;
            ebx_12 = (edx_2 >> 4) - 1;
            
            if (ebx_12 > 0x3f)
                ebx_12 = 0x3f;
            
            int32_t* ecx_14 = ecx_5 + edx_2;
            var_8 = ecx_14;
            edx_11 = (ecx_14 >> 4) - 1;
            
            if (edx_11 > 0x3f)
                edx_11 = 0x3f;
            
            if (ebx_12 != edx_11)
            {
                if (arg2[1] == arg2[2])
                {
                    if (ebx_12 >= 0x20)
                    {
                        uint32_t esi_7 = ~(0x80000000 >> (ebx_12 - 0x20));
                        result[edi_2 + 0x31] &= esi_7;
                        char temp3_1 = *(ebx_12 + result + 4);
                        *(ebx_12 + result + 4) -= 1;
                        
                        if (temp3_1 == 1)
                            *(arg1 + 4) &= esi_7;
                    }
                    else
                    {
                        uint32_t esi_5 = ~(0x80000000 >> ebx_12);
                        result[edi_2 + 0x11] &= esi_5;
                        char temp4_1 = *(ebx_12 + result + 4);
                        *(ebx_12 + result + 4) -= 1;
                        
                        if (temp4_1 == 1)
                        {
                            int32_t* ecx_17 = arg1;
                            *ecx_17 &= esi_5;
                        }
                    }
                }
                
                *(arg2[2] + 4) = arg2[1];
                *(arg2[1] + 8) = arg2[2];
            }
            
            esi_1 = arg2;
        }
        
        if (ebx_9 || ebx_12 != edx_11)
        {
            void* ecx_25 = &result[edi_2 * 0x81 + 0x51 + edx_11 * 2];
            esi_1[1] = result[edi_2 * 0x81 + 0x51 + edx_11 * 2 + 1];
            esi_1[2] = ecx_25;
            *(ecx_25 + 4) = esi_1;
            *(esi_1[1] + 8) = esi_1;
            
            if (esi_1[1] == esi_1[2])
            {
                int32_t ecx_27;
                ecx_27 = *(edx_11 + result + 4);
                *arg2[3] = ecx_27;
                ecx_27 += 1;
                *(edx_11 + result + 4) = ecx_27;
                
                if (edx_11 >= 0x20)
                {
                    if (!*arg2[3])
                        *(arg1 + 4) |= 0x80000000 >> (edx_11 - 0x20);
                    
                    result[edi_2 + 0x31] |= 0x80000000 >> (edx_11 - 0x20);
                }
                else
                {
                    if (!*arg2[3])
                    {
                        int32_t* ecx_29 = arg1;
                        *ecx_29 |= 0x80000000 >> edx_11;
                    }
                    
                    result[edi_2 + 0x11] |= 0x80000000 >> edx_11;
                }
            }
        }
        
        *esi_1 = var_8;
        *(var_8 + esi_1 - 4) = var_8;
        result = &result[edi_2 * 0x81 + 0x51];
        int32_t temp2_1 = *result;
        *result -= 1;
        
        if (temp2_1 == 1)
        {
            int32_t eax_3 = data_409974;
            
            if (eax_3)
            {
                VirtualFree((data_40996c << 0xf) + *(eax_3 + 0xc), 0x8000, MEM_DECOMMIT);
                void* eax_4 = data_409974;
                *(eax_4 + 8) |= 0x80000000 >> data_40996c;
                *(*(data_409974 + 0x10) + (data_40996c << 2) + 0xc4) = 0;
                void* eax_8 = *(data_409974 + 0x10);
                *(eax_8 + 0x43) -= 1;
                void* eax_9 = data_409974;
                
                if (!*(*(eax_9 + 0x10) + 0x43))
                {
                    *(eax_9 + 4) &= 0xfffffffe;
                    eax_9 = data_409974;
                }
                
                if (*(eax_9 + 8) == 0xffffffff)
                {
                    VirtualFree(*(eax_9 + 0xc), 0, MEM_RELEASE);
                    HeapFree(data_409984, HEAP_NONE, *(data_409974 + 0x10));
                    char* eax_14 = data_409974;
                    sub_404c80(eax_14, &eax_14[0x14], 
                        data_409978 * 0x14 - eax_14 + data_40997c - 0x14);
                    data_409978 -= 1;
                    
                    if (arg1 > data_409974)
                        arg1 -= 0x14;
                    
                    data_409970 = data_40997c;
                }
            }
            
            result = arg1;
            data_40996c = edi_2;
            data_409974 = result;
        }
    }
    
    return result;
}

void* sub_402fa8(int32_t* arg1)
{
    int32_t* edx = data_40997c;
    void* edi = &edx[data_409978 * 5];
    void* ecx_1 = (arg1 + 0x17) & 0xfffffff0;
    uint32_t var_10;
    uint32_t var_c;
    uint32_t esi;
    
    if ((ecx_1 >> 4) - 1 >= 0x20)
    {
        esi = 0;
        var_10 = 0;
        var_c = 0xffffffff >> (((ecx_1 >> 4) - 1) - 0x20);
    }
    else
    {
        esi = 0xffffffff >> ((ecx_1 >> 4) - 1);
        var_c = 0xffffffff;
        var_10 = esi;
    }
    
    int32_t* eax_4 = data_409970;
    int32_t* ebx = eax_4;
    arg1 = ebx;
    
    if (ebx < edi)
    {
        while (!((ebx[1] & var_c) | (*ebx & esi)))
        {
            ebx = &ebx[5];
            arg1 = ebx;
            
            if (ebx >= edi)
                break;
        }
    }
    
    if (ebx == edi)
    {
        ebx = edx;
        bool cond:4_1;
        
        while (true)
        {
            cond:4_1 = ebx != eax_4;
            arg1 = ebx;
            
            if (ebx >= eax_4)
                break;
            
            if ((ebx[1] & var_c) | (*ebx & esi))
            {
                cond:4_1 = ebx != eax_4;
                break;
            }
            
            ebx = &ebx[5];
        }
        
        if (!cond:4_1)
        {
            bool cond:5_1;
            
            while (true)
            {
                cond:5_1 = ebx != edi;
                
                if (ebx >= edi)
                    break;
                
                if (ebx[2])
                {
                    cond:5_1 = ebx != edi;
                    break;
                }
                
                ebx = &ebx[5];
                arg1 = ebx;
            }
            
            if (!cond:5_1)
            {
                ebx = edx;
                bool cond:7_1;
                
                while (true)
                {
                    cond:7_1 = ebx != eax_4;
                    arg1 = ebx;
                    
                    if (ebx >= eax_4)
                        break;
                    
                    if (ebx[2])
                    {
                        cond:7_1 = ebx != eax_4;
                        break;
                    }
                    
                    ebx = &ebx[5];
                }
                
                if (!cond:7_1)
                {
                    ebx = sub_4032b1();
                    arg1 = ebx;
                    
                    if (!ebx)
                        return nullptr;
                }
            }
            
            *ebx[4] = sub_403362(ebx);
            
            if (*ebx[4] == 0xffffffff)
                return nullptr;
        }
    }
    
    data_409970 = ebx;
    int32_t* eax_8 = ebx[4];
    int32_t edx_1 = *eax_8;
    int32_t var_8_1 = edx_1;
    
    if (edx_1 == 0xffffffff || !((eax_8[edx_1 + 0x31] & var_c) | (eax_8[edx_1 + 0x11] & esi)))
    {
        var_8_1 = 0;
        void* ecx_15 = &eax_8[0x11];
        esi = var_10;
        
        if (!((eax_8[0x31] & var_c) | (eax_8[0x11] & var_10)))
        {
            int32_t edx_6;
            
            do
            {
                var_8_1 += 1;
                edx_6 = *(ecx_15 + 0x84) & var_c;
                ecx_15 += 4;
            } while (!(edx_6 | (esi & *ecx_15)));
        }
        
        edx_1 = var_8_1;
    }
    
    int32_t edi_9 = 0;
    int32_t i = eax_8[edx_1 + 0x11] & esi;
    
    if (!i)
    {
        i = eax_8[edx_1 + 0x31] & var_c;
        edi_9 = 0x20;
    }
    
    while (i >= 0)
    {
        i <<= 1;
        edi_9 += 1;
    }
    
    int32_t* edx_8 = eax_8[edx_1 * 0x81 + 0x51 + edi_9 * 2 + 1];
    void* ecx_23 = *edx_8 - ecx_1;
    int32_t esi_5 = (ecx_23 >> 4) - 1;
    
    if (esi_5 > 0x3f)
        esi_5 = 0x3f;
    
    if (esi_5 == edi_9)
    {
    label_40325f:
        
        if (ecx_23)
        {
            *edx_8 = ecx_23;
            *(ecx_23 + edx_8 - 4) = ecx_23;
        }
    }
    else
    {
        if (edx_8[1] == edx_8[2])
        {
            if (edi_9 >= 0x20)
            {
                uint32_t ebx_5 = ~(0x80000000 >> (edi_9 - 0x20));
                eax_8[var_8_1 + 0x31] &= ebx_5;
                char temp1_1 = *(eax_8 + edi_9 + 4);
                *(eax_8 + edi_9 + 4) -= 1;
                
                if (temp1_1 != 1)
                    ebx = arg1;
                else
                {
                    ebx = arg1;
                    ebx[1] &= ebx_5;
                }
            }
            else
            {
                uint32_t ebx_2 = ~(0x80000000 >> edi_9);
                eax_8[var_8_1 + 0x11] &= ebx_2;
                char temp2_1 = *(eax_8 + edi_9 + 4);
                *(eax_8 + edi_9 + 4) -= 1;
                
                if (temp2_1 != 1)
                    ebx = arg1;
                else
                {
                    ebx = arg1;
                    *ebx &= ebx_2;
                }
            }
        }
        
        *(edx_8[2] + 4) = edx_8[1];
        *(edx_8[1] + 8) = edx_8[2];
        
        if (ecx_23)
        {
            void* ecx_35 = &eax_8[edx_1 * 0x81 + 0x51 + esi_5 * 2];
            edx_8[1] = eax_8[edx_1 * 0x81 + 0x51 + esi_5 * 2 + 1];
            edx_8[2] = ecx_35;
            *(ecx_35 + 4) = edx_8;
            *(edx_8[1] + 8) = edx_8;
            
            if (edx_8[1] == edx_8[2])
            {
                int32_t ecx_37;
                ecx_37 = *(esi_5 + eax_8 + 4);
                *arg1[3] = ecx_37;
                
                if (esi_5 >= 0x20)
                {
                    ecx_37 += 1;
                    *(esi_5 + eax_8 + 4) = ecx_37;
                    
                    if (!*arg1[3])
                        ebx[1] |= 0x80000000 >> (esi_5 - 0x20);
                    
                    eax_8[var_8_1 + 0x31] |= 0x80000000 >> (esi_5 - 0x20);
                }
                else
                {
                    ecx_37 += 1;
                    *(esi_5 + eax_8 + 4) = ecx_37;
                    
                    if (!*arg1[3])
                        *ebx |= 0x80000000 >> esi_5;
                    
                    eax_8[var_8_1 + 0x11] |= 0x80000000 >> esi_5;
                }
            }
            
            goto label_40325f;
        }
    }
    
    void** edx_9 = edx_8 + ecx_23;
    *edx_9 = ecx_1 + 1;
    *(edx_9 + ecx_1 - 4) = ecx_1 + 1;
    int32_t ecx_45 = eax_8[edx_1 * 0x81 + 0x51];
    eax_8[edx_1 * 0x81 + 0x51] = ecx_45 + 1;
    
    if (!ecx_45 && ebx == data_409974 && var_8_1 == data_40996c)
        data_409974 = 0;
    
    *eax_8 = var_8_1;
    return &edx_9[1];
}

int32_t* sub_4032b1()
{
    int32_t eax_3 = data_409978;
    int32_t ecx = data_409968;
    
    if (eax_3 != ecx)
        goto label_40330a;
    
    int32_t eax_2 = HeapReAlloc(data_409984, HEAP_NONE, data_40997c, (ecx * 5 + 0x50) << 2);
    
    if (eax_2)
    {
        data_409968 += 0x10;
        data_40997c = eax_2;
        eax_3 = data_409978;
    label_40330a:
        int32_t* result = data_40997c + eax_3 * 0x14;
        int32_t eax_5 = HeapAlloc(data_409984, HEAP_ZERO_MEMORY, 0x41c4);
        result[4] = eax_5;
        
        if (eax_5)
        {
            int32_t eax_6 = VirtualAlloc(nullptr, 0x100000, MEM_RESERVE, PAGE_READWRITE);
            result[3] = eax_6;
            
            if (eax_6)
            {
                result[2] = 0xffffffff;
                *result = 0;
                result[1] = 0;
                data_409978 += 1;
                *result[4] = 0xffffffff;
                return result;
            }
            
            HeapFree(data_409984, HEAP_NONE, result[4]);
        }
    }
    
    return nullptr;
}

int32_t sub_403362(void* arg1)
{
    int32_t ecx;
    int32_t var_8 = ecx;
    void* esi = *(arg1 + 0x10);
    int32_t i = *(arg1 + 8);
    int32_t result = 0;
    
    while (i >= 0)
    {
        i <<= 1;
        result += 1;
    }
    
    int32_t i_2 = 0x3f;
    void* eax_2 = result * 0x204 + esi + 0x144;
    void* var_8_1 = eax_2;
    int32_t i_1;
    
    do
    {
        *(eax_2 + 8) = eax_2;
        *(eax_2 + 4) = eax_2;
        eax_2 += 8;
        i_1 = i_2;
        i_2 -= 1;
    } while (i_1 != 1);
    void* lpAddress = (result << 0xf) + *(arg1 + 0xc);
    
    if (!VirtualAlloc(lpAddress, 0x8000, MEM_COMMIT, PAGE_READWRITE))
        return 0xffffffff;
    
    if (lpAddress <= lpAddress + 0x7000)
    {
        void** eax_5 = lpAddress + 0x10;
        
        do
        {
            eax_5[-2] = 0xffffffff;
            eax_5[0x3fb] = 0xffffffff;
            eax_5[-1] = 0xff0;
            *eax_5 = &eax_5[0x3ff];
            eax_5[1] = &eax_5[-0x401];
            eax_5[0x3fa] = 0xff0;
            eax_5 = &eax_5[0x400];
        } while (&eax_5[-4] <= lpAddress + 0x7000);
    }
    
    *(var_8_1 + 0x1fc) = lpAddress + 0xc;
    *(lpAddress + 0x14) = var_8_1 + 0x1f8;
    *(var_8_1 + 0x200) = lpAddress + 0x700c;
    *(lpAddress + 0x7010) = var_8_1 + 0x1f8;
    *(esi + (result << 2) + 0x44) = 0;
    *(esi + (result << 2) + 0xc4) = 1;
    void* eax_7;
    eax_7 = *(esi + 0x43);
    void* ecx_6;
    ecx_6 = eax_7;
    ecx_6 += 1;
    *(esi + 0x43) = ecx_6;
    
    if (!eax_7)
        *(arg1 + 4) |= 1;
    
    *(arg1 + 8) &= ~(0x80000000 >> result);
    return result;
}

int32_t sub_40345d(int32_t* arg1, void** arg2, void* arg3)
{
    int32_t eax_1 = arg1[4];
    void* esi_1 = (arg3 + 0x17) & 0xfffffff0;
    uint32_t edx_2 = (arg2 - arg1[3]) >> 0xf;
    void* ecx_5 = arg2[-1] - 1;
    void* ebx = *(ecx_5 + arg2 - 4);
    void* edi_1 = ecx_5 + arg2 - 4;
    
    if (esi_1 > ecx_5)
    {
        if (ebx & 1 || esi_1 > ebx + ecx_5)
            return 0;
        
        int32_t ecx_8 = (ebx >> 4) - 1;
        int32_t var_c_1 = ecx_8;
        
        if (ecx_8 > 0x3f)
        {
            ecx_8 = 0x3f;
            var_c_1 = 0x3f;
        }
        
        if (*(edi_1 + 4) == *(edi_1 + 8))
        {
            if (ecx_8 >= 0x20)
            {
                uint32_t ebx_6 = ~(0x80000000 >> (ecx_8 - 0x20));
                *(eax_1 + (edx_2 << 2) + 0xc4) &= ebx_6;
                char temp0_1 = *(var_c_1 + eax_1 + 4);
                *(var_c_1 + eax_1 + 4) -= 1;
                
                if (temp0_1 == 1)
                    arg1[1] &= ebx_6;
            }
            else
            {
                uint32_t ebx_4 = ~(0x80000000 >> ecx_8);
                *(eax_1 + (edx_2 << 2) + 0x44) &= ebx_4;
                char temp1_1 = *(var_c_1 + eax_1 + 4);
                *(var_c_1 + eax_1 + 4) -= 1;
                
                if (temp1_1 == 1)
                    *arg1 &= ebx_4;
            }
        }
        
        *(*(edi_1 + 8) + 4) = *(edi_1 + 4);
        *(*(edi_1 + 4) + 8) = *(edi_1 + 8);
        void* var_8_1 = ebx + ecx_5 - esi_1;
        void** edx_4;
        
        if (var_8_1 <= 0)
            edx_4 = arg2;
        else
        {
            int32_t edi_5 = (var_8_1 >> 4) - 1;
            
            if (edi_5 > 0x3f)
                edi_5 = 0x3f;
            
            void* ebx_9 = edx_2 * 0x204 + eax_1 + 0x144 + (edi_5 << 3);
            *(arg2 + esi_1) = *(ebx_9 + 4);
            *(arg2 + esi_1 + 4) = ebx_9;
            *(ebx_9 + 4) = arg2 + esi_1 - 4;
            *(*(arg2 + esi_1) + 8) = arg2 + esi_1 - 4;
            
            if (*(arg2 + esi_1) == *(arg2 + esi_1 + 4))
            {
                void* ecx_21;
                ecx_21 = *(edi_5 + eax_1 + 4);
                *arg3[3] = ecx_21;
                ecx_21 += 1;
                *(edi_5 + eax_1 + 4) = ecx_21;
                int32_t* eax_2;
                char ecx_24;
                
                if (edi_5 >= 0x20)
                {
                    if (!*arg3[3])
                        arg1[1] |= 0x80000000 >> (edi_5 - 0x20);
                    
                    eax_2 = eax_1 + (edx_2 << 2) + 0xc4;
                    ecx_24 = edi_5 - 0x20;
                }
                else
                {
                    if (!*arg3[3])
                        *arg1 |= 0x80000000 >> edi_5;
                    
                    eax_2 = eax_1 + (edx_2 << 2) + 0x44;
                    ecx_24 = edi_5;
                }
                
                *eax_2 |= 0x80000000 >> ecx_24;
            }
            
            edx_4 = arg2;
            *(edx_4 + esi_1 - 4) = var_8_1;
            *(var_8_1 + edx_4 + esi_1 - 4 - 4) = var_8_1;
        }
        
        edx_4[-1] = esi_1 + 1;
        *(edx_4 + esi_1 - 8) = esi_1 + 1;
    }
    else if (esi_1 < ecx_5)
    {
        arg3 = ecx_5 - esi_1;
        arg2[-1] = esi_1 + 1;
        void** ebx_17 = arg2 + esi_1 - 4;
        int32_t esi_4 = (arg3 >> 4) - 1;
        ebx_17[-1] = esi_1 + 1;
        
        if (esi_4 > 0x3f)
            esi_4 = 0x3f;
        
        if (!(ebx & 1))
        {
            int32_t esi_7 = (ebx >> 4) - 1;
            
            if (esi_7 > 0x3f)
                esi_7 = 0x3f;
            
            if (*(edi_1 + 4) == *(edi_1 + 8))
            {
                if (esi_7 >= 0x20)
                {
                    uint32_t ebx_21 = ~(0x80000000 >> (esi_7 - 0x20));
                    *(eax_1 + (edx_2 << 2) + 0xc4) &= ebx_21;
                    char temp2_1 = *(esi_7 + eax_1 + 4);
                    *(esi_7 + eax_1 + 4) -= 1;
                    
                    if (temp2_1 == 1)
                        arg1[1] &= ebx_21;
                }
                else
                {
                    uint32_t ebx_19 = ~(0x80000000 >> esi_7);
                    *(eax_1 + (edx_2 << 2) + 0x44) &= ebx_19;
                    char temp3_1 = *(esi_7 + eax_1 + 4);
                    *(esi_7 + eax_1 + 4) -= 1;
                    
                    if (temp3_1 == 1)
                        *arg1 &= ebx_19;
                }
            }
            
            *(*(edi_1 + 8) + 4) = *(edi_1 + 4);
            *(*(edi_1 + 4) + 8) = *(edi_1 + 8);
            void* esi_12 = arg3 + ebx;
            arg3 = esi_12;
            esi_4 = (esi_12 >> 4) - 1;
            
            if (esi_4 > 0x3f)
                esi_4 = 0x3f;
        }
        
        void* ecx_38 = edx_2 * 0x204 + eax_1 + 0x144 + (esi_4 << 3);
        ebx_17[1] = *(edx_2 * 0x204 + eax_1 + 0x144 + (esi_4 << 3) + 4);
        ebx_17[2] = ecx_38;
        *(ecx_38 + 4) = ebx_17;
        *(ebx_17[1] + 8) = ebx_17;
        
        if (ebx_17[1] == ebx_17[2])
        {
            int32_t ecx_40;
            ecx_40 = *(esi_4 + eax_1 + 4);
            *arg2[3] = ecx_40;
            ecx_40 += 1;
            *(esi_4 + eax_1 + 4) = ecx_40;
            int32_t* eax_6;
            char ecx_43;
            
            if (esi_4 >= 0x20)
            {
                if (!*arg2[3])
                    arg1[1] |= 0x80000000 >> (esi_4 - 0x20);
                
                eax_6 = eax_1 + (edx_2 << 2) + 0xc4;
                ecx_43 = esi_4 - 0x20;
            }
            else
            {
                if (!*arg2[3])
                    *arg1 |= 0x80000000 >> esi_4;
                
                eax_6 = eax_1 + (edx_2 << 2) + 0x44;
                ecx_43 = esi_4;
            }
            
            *eax_6 |= 0x80000000 >> ecx_43;
        }
        
        *ebx_17 = arg3;
        *(arg3 + ebx_17 - 4) = arg3;
    }
    
    return 1;
}

void** sub_403753()
{
    void** lpMem;
    
    if (data_407218 != 0xffffffff)
    {
        lpMem = HeapAlloc(data_409984, HEAP_NONE, 0x2020);
        
        if (lpMem)
            goto label_40379a;
    }
    else
    {
        lpMem = &data_407208;
    label_40379a:
        char* lpAddress = VirtualAlloc(nullptr, &__dos_header, MEM_RESERVE, PAGE_READWRITE);
        
        if (lpAddress)
        {
            if (VirtualAlloc(lpAddress, 0x10000, MEM_COMMIT, PAGE_READWRITE))
            {
                if (lpMem != &data_407208)
                {
                    *lpMem = &data_407208;
                    lpMem[1] = data_40720c;
                    data_40720c = lpMem;
                    *lpMem[1] = lpMem;
                }
                else
                {
                    if (!data_407208)
                        data_407208 = &data_407208;
                    
                    if (!data_40720c)
                        data_40720c = &data_407208;
                }
                
                lpMem[5] = lpAddress + &__dos_header;
                void* eax_7 = &lpMem[6];
                lpMem[3] = &lpMem[0x26];
                lpMem[4] = lpAddress;
                lpMem[2] = eax_7;
                
                for (int32_t i = 0; i < 0x400; )
                {
                    int32_t edx_1;
                    edx_1 = i >= 0x10;
                    i += 1;
                    *eax_7 = ((edx_1 - 1) & 0xf1) - 1;
                    *(eax_7 + 4) = 0xf1;
                    eax_7 += 8;
                }
                
                sub_404fc0(lpAddress, 0, 0x10000);
                
                for (; lpAddress < lpMem[4] + 0x10000; lpAddress = &lpAddress[0x1000])
                {
                    lpAddress[0xf8] = 0xff;
                    *lpAddress = &lpAddress[8];
                    *(lpAddress + 4) = 0xf0;
                }
                
                return lpMem;
            }
            
            VirtualFree(lpAddress, 0, MEM_RELEASE);
        }
        
        if (lpMem != &data_407208)
            HeapFree(data_409984, HEAP_NONE, lpMem);
    }
    return nullptr;
}

BOOL sub_403897(int32_t* arg1)
{
    BOOL result = VirtualFree(arg1[4], 0, MEM_RELEASE);
    
    if (data_409228 == arg1)
    {
        result = arg1[1];
        data_409228 = result;
    }
    
    if (arg1 == &data_407208)
    {
        data_407218 = 0xffffffff;
        return result;
    }
    
    *arg1[1] = *arg1;
    *(*arg1 + 4) = arg1[1];
    return HeapFree(data_409984, HEAP_NONE, arg1);
}

void sub_4038ed(int32_t arg1)
{
    int32_t ecx;
    int32_t var_8 = ecx;
    int32_t* esi = data_40720c;
    
    do
    {
        if (esi[4] != 0xffffffff)
        {
            int32_t var_8_1 = 0;
            void* edi_1 = &esi[0x804];
            BOOL eax;
            
            for (int32_t j = 0x3ff000; j >= 0; )
            {
                if (*edi_1 == 0xf0 && VirtualFree(j + esi[4], 0x1000, MEM_DECOMMIT))
                {
                    *edi_1 = 0xffffffff;
                    data_409704 -= 1;
                    eax = esi[3];
                    
                    if (!eax || eax > edi_1)
                        esi[3] = edi_1;
                    
                    var_8_1 += 1;
                    int32_t temp0_1 = arg1;
                    arg1 -= 1;
                    
                    if (temp0_1 == 1)
                        break;
                }
                
                j -= 0x1000;
                edi_1 -= 8;
            }
            
            int32_t* ecx_1 = esi;
            esi = esi[1];
            
            if (var_8_1 && ecx_1[6] == 0xffffffff)
            {
                eax = &ecx_1[8];
                int32_t edx_1 = 1;
                
                while (*eax == 0xffffffff)
                {
                    edx_1 += 1;
                    eax += 8;
                    
                    if (edx_1 >= 0x400)
                        break;
                }
                
                if (edx_1 == 0x400)
                    sub_403897(ecx_1);
            }
        }
        
        if (esi == data_40720c)
            break;
    } while (arg1 > 0);
}

int32_t sub_4039af(int32_t arg1, void*** arg2, int32_t* arg3)
{
    void** i = &data_407208;
    
    do
    {
        if (arg1 > i[4] && arg1 < i[5])
        {
            if (arg1 & 0xf || (arg1 & 0xfff) < 0x100)
                break;
            
            *arg2 = i;
            int32_t ecx;
            ecx = arg1 & 0xf000;
            *arg3 = ecx;
            return ((arg1 - ecx - 0x100) >> 4) + ecx + 8;
        }
        
        i = *i;
    } while (i != &data_407208);
    
    return 0;
}

void* sub_403a06(void* arg1, int32_t arg2, char* arg3)
{
    void* result = arg1 + ((arg2 - *(arg1 + 0x10)) >> 0xc << 3) + 0x18;
    *result += *arg3;
    *arg3 = 0;
    bool cond:0 = *result != 0xf0;
    *(result + 4) = 0xf1;
    
    if (!cond:0)
    {
        data_409704 += 1;
        
        if (data_409704 == 0x20)
            result = sub_4038ed(0x10);
    }
    
    return result;
}

void* sub_403a4b(int32_t arg1)
{
    int32_t ecx;
    int32_t var_8 = ecx;
    int32_t var_c = ecx;
    void** esi = data_409228;
    void* result;
    
    while (true)
    {
        int32_t edx_1 = esi[4];
        int32_t ebx_1;
        
        if (edx_1 == 0xffffffff)
            ebx_1 = arg1;
        else
        {
            void* edi_1 = esi[2];
            void* eax_6 = ((edi_1 - esi - 0x18) >> 3 << 0xc) + edx_1;
            void* var_8_1 = eax_6;
            
            if (edi_1 < &esi[0x806])
            {
                while (true)
                {
                    int32_t ecx_2 = *edi_1;
                    ebx_1 = arg1;
                    
                    if (ecx_2 >= ebx_1 && *(edi_1 + 4) > ebx_1)
                    {
                        result = sub_403c53(eax_6, ecx_2, ebx_1);
                        
                        if (result)
                            break;
                        
                        eax_6 = var_8_1;
                        *(edi_1 + 4) = ebx_1;
                    }
                    
                    edi_1 += 8;
                    eax_6 += 0x1000;
                    var_8_1 = eax_6;
                    
                    if (edi_1 >= &esi[0x806])
                        goto label_403ac1;
                }
                
                goto label_403b16;
            }
            
            ebx_1 = arg1;
        label_403ac1:
            int32_t eax_7 = esi[2];
            edi_1 = &esi[6];
            void* var_8_2 = esi[4];
            
            if (edi_1 < eax_7)
            {
                while (true)
                {
                    int32_t eax_8 = *edi_1;
                    
                    if (eax_8 >= ebx_1 && *(edi_1 + 4) > ebx_1)
                    {
                        result = sub_403c53(var_8_2, eax_8, ebx_1);
                        
                        if (result)
                            break;
                        
                        *(edi_1 + 4) = ebx_1;
                    }
                    
                    var_8_2 += 0x1000;
                    edi_1 += 8;
                    
                    if (edi_1 >= eax_7)
                        goto label_403b07;
                }
                
            label_403b16:
                data_409228 = esi;
                *edi_1 -= ebx_1;
                esi[2] = edi_1;
                break;
            }
        }
        
    label_403b07:
        esi = *esi;
        
        if (esi == data_409228)
        {
            void** edi_2 = &data_407208;
            int32_t i;
            int32_t dwSize;
            int32_t* ebx_2;
            int32_t* lpAddress;
            
            while (true)
            {
                if (edi_2[4] == 0xffffffff || !edi_2[3])
                {
                    edi_2 = *edi_2;
                    
                    if (edi_2 == &data_407208)
                    {
                        void** eax_17 = sub_403753();
                        
                        if (eax_17)
                        {
                            void** ecx_7 = eax_17[4];
                            ecx_7[2] = ebx_1;
                            data_409228 = eax_17;
                            *ecx_7 = ecx_7 + ebx_1 + 8;
                            ecx_7[1] = 0xf0 - ebx_1;
                            eax_17[6] -= ebx_1;
                            return &ecx_7[0x40];
                        }
                    }
                    else
                        continue;
                }
                else
                {
                    ebx_2 = edi_2[3];
                    i = 0;
                    int32_t* eax_9 = ebx_2;
                    lpAddress = ((ebx_2 - edi_2 - 0x18) >> 3 << 0xc) + edi_2[4];
                    
                    if (*ebx_2 == 0xffffffff)
                    {
                        while (i < 0x10)
                        {
                            eax_9 = &eax_9[2];
                            i += 1;
                            
                            if (*eax_9 != 0xffffffff)
                                break;
                        }
                    }
                    
                    dwSize = i << 0xc;
                    
                    if (VirtualAlloc(lpAddress, dwSize, MEM_COMMIT, PAGE_READWRITE) == lpAddress)
                        break;
                }
                
                return nullptr;
            }
            
            sub_404fc0(lpAddress, dwSize, 0);
            int32_t* ecx_5 = ebx_2;
            
            if (i > 0)
            {
                void* eax_12 = &lpAddress[1];
                int32_t i_2 = i;
                int32_t i_1;
                
                do
                {
                    *(eax_12 + 0xf4) = 0xff;
                    *(eax_12 - 4) = eax_12 + 4;
                    *eax_12 = 0xf0;
                    *ecx_5 = 0xf0;
                    ecx_5[1] = 0xf1;
                    eax_12 += 0x1000;
                    ecx_5 = &ecx_5[2];
                    i_1 = i_2;
                    i_2 -= 1;
                } while (i_1 != 1);
            }
            
            data_409228 = edi_2;
            bool c_1;
            
            while (true)
            {
                c_1 = ecx_5 < &edi_2[0x806];
                
                if (!c_1)
                    break;
                
                if (*ecx_5 == 0xffffffff)
                {
                    c_1 = ecx_5 < &edi_2[0x806];
                    break;
                }
                
                ecx_5 = &ecx_5[2];
            }
            
            edi_2[3] = (&edi_2[0x806] - &edi_2[0x806]) & ecx_5;
            lpAddress[2] = arg1;
            edi_2[2] = ebx_2;
            *ebx_2 -= arg1;
            lpAddress[1] -= arg1;
            result = &lpAddress[0x40];
            *lpAddress = lpAddress + arg1 + 8;
            break;
        }
    }
    
    return result;
}

int32_t sub_403c53(void* arg1, int32_t arg2, int32_t arg3)
{
    int32_t ecx;
    int32_t var_8 = ecx;
    int32_t* ecx_1 = arg1;
    void* esi = ecx_1[1];
    void* edi = *ecx_1;
    void* var_8_1 = edi;
    void* eax = edi;
    void* eax_5;
    
    if (esi < arg3)
    {
        void* esi_1 = esi + edi;
        
        if (*esi_1)
            eax = esi_1;
        
        if (eax + arg3 >= &ecx_1[0x3e])
        {
        label_403ceb:
            char* esi_6 = &ecx_1[2];
            int32_t eax_4;
            
            while (true)
            {
                if (esi_6 >= edi)
                    return 0;
                
                if (&esi_6[arg3] >= &ecx_1[0x3e])
                    return 0;
                
                void* eax_3;
                eax_3 = *esi_6;
                
                if (eax_3)
                    esi_6 = &esi_6[eax_3];
                else
                {
                    void* ebx_1 = &esi_6[1];
                    eax_4 = 1;
                    
                    while (!*ebx_1)
                    {
                        ebx_1 += 1;
                        eax_4 += 1;
                    }
                    
                    if (eax_4 >= arg3)
                        break;
                    
                    arg2 -= eax_4;
                    
                    if (arg2 < arg3)
                        return 0;
                    
                    esi_6 = ebx_1;
                }
            }
            
            void* ebx_3 = &esi_6[arg3];
            
            if (ebx_3 >= &ecx_1[0x3e])
            {
                ecx_1[1] = 0;
                *ecx_1 = &ecx_1[2];
            }
            else
            {
                *ecx_1 = ebx_3;
                ecx_1[1] = eax_4 - arg3;
            }
            
            *esi_6 = arg3;
            eax_5 = &esi_6[8];
        }
        else
        {
            while (true)
            {
                void* ebx;
                ebx = *eax;
                
                if (ebx)
                    eax += ebx;
                else
                {
                    ebx = eax + 1;
                    int32_t esi_3 = 1;
                    
                    while (!*ebx)
                    {
                        ebx += 1;
                        esi_3 += 1;
                    }
                    
                    if (esi_3 >= arg3)
                    {
                        void* ebx_2 = eax + arg3;
                        
                        if (ebx_2 >= &ecx_1[0x3e])
                        {
                            ecx_1[1] = 0;
                            *ecx_1 = &ecx_1[2];
                        }
                        else
                        {
                            *ecx_1 = ebx_2;
                            ecx_1[1] = esi_3 - arg3;
                        }
                        
                        *eax = arg3;
                        eax_5 = eax + 8;
                        break;
                    }
                    
                    if (eax != var_8_1)
                    {
                        arg2 -= esi_3;
                        
                        if (arg2 < arg3)
                            return 0;
                    }
                    else
                        ecx_1[1] = esi_3;
                    
                    edi = var_8_1;
                    eax = ebx;
                }
                
                if (eax + arg3 >= &ecx_1[0x3e])
                    goto label_403ceb;
            }
        }
    }
    else
    {
        *edi = arg3;
        
        if (edi + arg3 >= &ecx_1[0x3e])
        {
            ecx_1[1] = 0;
            *ecx_1 = &ecx_1[2];
        }
        else
        {
            *ecx_1 += arg3;
            ecx_1[1] -= arg3;
        }
        
        eax_5 = edi + 8;
    }
    
    return (eax_5 << 4) - ecx_1 * 0xf;
}

int32_t sub_403d77(void* arg1, void** arg2, char* arg3, int32_t arg4)
{
    int32_t ecx;
    int32_t var_8 = ecx;
    uint32_t ecx_1 = *arg3;
    int32_t result = 0;
    void* edi_1 = arg1 + ((arg2 - *(arg1 + 0x10)) >> 0xc << 3) + 0x18;
    
    if (ecx_1 <= arg4)
    {
        if (ecx_1 >= arg4)
            return result;
        
        void* esi_1 = &arg3[arg4];
        
        if (&arg2[0x3e] < esi_1)
            return result;
        
        void* eax_6 = &arg3[ecx_1];
        bool cond:2_1;
        
        while (true)
        {
            cond:2_1 = eax_6 != esi_1;
            
            if (eax_6 >= esi_1)
                break;
            
            if (*eax_6)
            {
                cond:2_1 = eax_6 != esi_1;
                break;
            }
            
            eax_6 += 1;
        }
        
        if (cond:2_1)
            return result;
        
        eax_6 = arg4;
        *arg3 = eax_6;
        void* eax_7 = *arg2;
        
        if (arg3 <= eax_7 && esi_1 > eax_7)
        {
            if (esi_1 >= &arg2[0x3e])
            {
                arg2[1] = 0;
                *arg2 = &arg2[2];
            }
            else
            {
                int32_t eax_9 = 0;
                *arg2 = esi_1;
                
                if (!*esi_1)
                {
                    do
                        eax_9 += 1;
                     while (!*(esi_1 + eax_9));
                }
                
                arg2[1] = eax_9;
            }
        }
        
        *edi_1 += ecx_1 - arg4;
    }
    else
    {
        *arg3 = arg4;
        *edi_1 += ecx_1 - arg4;
        *(edi_1 + 4) = 0xf1;
    }
    
    return 1;
}

int32_t sub_403e20(int32_t arg1)
{
    int32_t eax_3 = data_409708;
    
    if (eax_3 && eax_3(arg1))
        return 1;
    
    return 0;
}

char* sub_403e40(char* arg1, char* arg2, int32_t arg3)
{
    char* esi = arg2;
    char* edi = arg1;
    uint32_t eax_1;
    
    if (edi > esi && edi < &esi[arg3])
    {
        void* esi_1 = &esi[arg3 - 4];
        void* edi_1 = &edi[arg3 - 4];
        int32_t edx_2;
        uint32_t ecx_4;
        
        if (!(edi_1 & 3))
        {
            ecx_4 = arg3 >> 2;
            edx_2 = arg3 & 3;
            
            if (ecx_4 >= 8)
            {
                edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), ecx_4 << 2);
                
                switch (edx_2)
                {
                    case 0:
                    {
                        return arg1;
                        break;
                    }
                    case 1:
                    {
                        goto label_404138;
                    }
                    case 2:
                    {
                        goto label_404148;
                    }
                    case 3:
                    {
                        goto label_40415c;
                    }
                }
            }
        }
        else if (arg3 < 4)
            switch (arg3)
            {
                case 0:
                {
                    return arg1;
                    break;
                }
                case 1:
                {
                label_404138:
                    eax_1 = *(esi_1 + 3);
                    *(edi_1 + 3) = eax_1;
                    return arg1;
                    break;
                }
                case 2:
                {
                label_404148:
                    eax_1 = *(esi_1 + 3);
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    *(edi_1 + 2) = eax_1;
                    return arg1;
                    break;
                }
                case 3:
                {
                label_40415c:
                    eax_1 = *(esi_1 + 3);
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    *(edi_1 + 2) = eax_1;
                    eax_1 = *(esi_1 + 1);
                    *(edi_1 + 1) = eax_1;
                    return arg1;
                    break;
                }
            }
        else
        {
            eax_1 = edi_1 & 3;
            int32_t ecx_6 = arg3 - eax_1;
            
            switch (jump_table_404028[eax_1])
            {
                case 0x404038:
                {
                    eax_1 = *(esi_1 + 3);
                    edx_2 = 3 & ecx_6;
                    *(edi_1 + 3) = eax_1;
                    esi_1 -= 1;
                    ecx_4 = ecx_6 >> 2;
                    edi_1 -= 1;
                    
                    if (ecx_4 >= 8)
                    {
                        edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), 
                            ecx_4 << 2);
                        
                        switch (edx_2)
                        {
                            case 0:
                            {
                                return arg1;
                                break;
                            }
                            case 1:
                            {
                                goto label_404138;
                            }
                            case 2:
                            {
                                goto label_404148;
                            }
                            case 3:
                            {
                                goto label_40415c;
                            }
                        }
                    }
                    break;
                }
                case 0x404058:
                {
                    eax_1 = *(esi_1 + 3);
                    edx_2 = 3 & ecx_6;
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    ecx_4 = ecx_6 >> 2;
                    *(edi_1 + 2) = eax_1;
                    esi_1 -= 2;
                    edi_1 -= 2;
                    
                    if (ecx_4 >= 8)
                    {
                        edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), 
                            ecx_4 << 2);
                        
                        switch (edx_2)
                        {
                            case 0:
                            {
                                return arg1;
                                break;
                            }
                            case 1:
                            {
                                goto label_404138;
                            }
                            case 2:
                            {
                                goto label_404148;
                            }
                            case 3:
                            {
                                goto label_40415c;
                            }
                        }
                    }
                    break;
                }
                case 0x404080:
                {
                    eax_1 = *(esi_1 + 3);
                    edx_2 = 3 & ecx_6;
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    *(edi_1 + 2) = eax_1;
                    eax_1 = *(esi_1 + 1);
                    ecx_4 = ecx_6 >> 2;
                    *(edi_1 + 1) = eax_1;
                    esi_1 -= 3;
                    edi_1 -= 3;
                    
                    if (ecx_4 >= 8)
                    {
                        edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), 
                            ecx_4 << 2);
                        
                        switch (edx_2)
                        {
                            case 0:
                            {
                                return arg1;
                                break;
                            }
                            case 1:
                            {
                                goto label_404138;
                            }
                            case 2:
                            {
                                goto label_404148;
                            }
                            case 3:
                            {
                                goto label_40415c;
                            }
                        }
                    }
                    break;
                }
            }
        }
        
        switch (edx_2)
        {
            case 0:
            {
                return arg1;
                break;
            }
            case 1:
            {
                goto label_404138;
            }
            case 2:
            {
                goto label_404148;
            }
            case 3:
            {
                goto label_40415c;
            }
        }
    }
    
    uint32_t ecx_1;
    int32_t edx_1;
    
    if (edi & 3)
    {
        if (arg3 < 4)
            /* jump -> *(((arg3 - 4) << 2) + &data_403f98) */
        
        eax_1 = edi & 3;
        int32_t ecx_3 = arg3 - 4 + eax_1;
        
        switch (jump_table_403ea0[eax_1])
        {
            case 0x403eb0:
            {
                edx_1 = 3 & ecx_3;
                eax_1 = *esi;
                *edi = eax_1;
                eax_1 = esi[1];
                edi[1] = eax_1;
                eax_1 = esi[2];
                ecx_1 = ecx_3 >> 2;
                edi[2] = eax_1;
                esi = &esi[3];
                edi = &edi[3];
                
                if (ecx_1 >= 8)
                {
                    edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
                    
                    switch (edx_1)
                    {
                        case 0:
                        {
                            return arg1;
                            break;
                        }
                        case 1:
                        {
                            goto label_403fa0;
                        }
                        case 2:
                        {
                            goto label_403fac;
                        }
                        case 3:
                        {
                            goto label_403fc0;
                        }
                    }
                }
                break;
            }
            case 0x403edc:
            {
                edx_1 = 3 & ecx_3;
                eax_1 = *esi;
                *edi = eax_1;
                eax_1 = esi[1];
                ecx_1 = ecx_3 >> 2;
                edi[1] = eax_1;
                esi = &esi[2];
                edi = &edi[2];
                
                if (ecx_1 >= 8)
                {
                    edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
                    
                    switch (edx_1)
                    {
                        case 0:
                        {
                            return arg1;
                            break;
                        }
                        case 1:
                        {
                            goto label_403fa0;
                        }
                        case 2:
                        {
                            goto label_403fac;
                        }
                        case 3:
                        {
                            goto label_403fc0;
                        }
                    }
                }
                break;
            }
            case 0x403f00:
            {
                edx_1 = 3 & ecx_3;
                eax_1 = *esi;
                *edi = eax_1;
                esi = &esi[1];
                ecx_1 = ecx_3 >> 2;
                edi = &edi[1];
                
                if (ecx_1 >= 8)
                {
                    edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
                    
                    switch (edx_1)
                    {
                        case 0:
                        {
                            return arg1;
                            break;
                        }
                        case 1:
                        {
                            goto label_403fa0;
                        }
                        case 2:
                        {
                            goto label_403fac;
                        }
                        case 3:
                        {
                            goto label_403fc0;
                        }
                    }
                }
                break;
            }
        }
    }
    else
    {
        ecx_1 = arg3 >> 2;
        edx_1 = arg3 & 3;
        
        if (ecx_1 >= 8)
        {
            edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
            
            switch (edx_1)
            {
                case 0:
                {
                    return arg1;
                    break;
                }
                case 1:
                {
                    goto label_403fa0;
                }
                case 2:
                {
                    goto label_403fac;
                }
                case 3:
                {
                    goto label_403fc0;
                }
            }
        }
    }
    
    switch (ecx_1)
    {
        case 0:
        {
            goto label_403f7f;
        }
        case 1:
        {
            goto label_403f70;
        }
        case 2:
        {
            goto label_403f68;
        }
        case 3:
        {
            goto label_403f60;
        }
        case 4:
        {
            goto label_403f58;
        }
        case 5:
        {
            goto label_403f50;
        }
        case 6:
        {
            goto label_403f48;
        }
        case 7:
        {
            *(edi + (ecx_1 << 2) - 0x1c) = *(esi + (ecx_1 << 2) - 0x1c);
        label_403f48:
            *(edi + (ecx_1 << 2) - 0x18) = *(esi + (ecx_1 << 2) - 0x18);
        label_403f50:
            *(edi + (ecx_1 << 2) - 0x14) = *(esi + (ecx_1 << 2) - 0x14);
        label_403f58:
            *(edi + (ecx_1 << 2) - 0x10) = *(esi + (ecx_1 << 2) - 0x10);
        label_403f60:
            *(edi + (ecx_1 << 2) - 0xc) = *(esi + (ecx_1 << 2) - 0xc);
        label_403f68:
            *(edi + (ecx_1 << 2) - 8) = *(esi + (ecx_1 << 2) - 8);
        label_403f70:
            *(edi + (ecx_1 << 2) - 4) = *(esi + (ecx_1 << 2) - 4);
            eax_1 = ecx_1 << 2;
            esi = &esi[eax_1];
            edi = &edi[eax_1];
        label_403f7f:
            
            switch (edx_1)
            {
                case 0:
                {
                    return arg1;
                    break;
                }
                case 1:
                {
                label_403fa0:
                    eax_1 = *esi;
                    *edi = eax_1;
                    return arg1;
                    break;
                }
                case 2:
                {
                label_403fac:
                    eax_1 = *esi;
                    *edi = eax_1;
                    eax_1 = esi[1];
                    edi[1] = eax_1;
                    return arg1;
                    break;
                }
                case 3:
                {
                label_403fc0:
                    eax_1 = *esi;
                    *edi = eax_1;
                    eax_1 = esi[1];
                    edi[1] = eax_1;
                    eax_1 = esi[2];
                    edi[2] = eax_1;
                    return arg1;
                    break;
                }
            }
            break;
        }
    }
}

int32_t sub_404175(char arg1)
{
    return sub_404186(arg1, 0, 4);
}

int32_t sub_404186(char arg1, int32_t arg2, char arg3)
{
    uint32_t eax_2 = arg1;
    
    if (!(*(eax_2 + 0x409861) & arg3))
    {
        int32_t result;
        
        if (!arg2)
            result = 0;
        else
            result = data_40933a[eax_2] & arg2;
        
        if (!result)
            return result;
    }
    
    return 1;
}

int32_t sub_4041b7(uint32_t arg1)
{
    uint32_t CodePage = sub_404350(arg1);
    
    if (CodePage != data_409748)
    {
        if (!CodePage)
        {
        label_40433a:
            sub_4043cd();
        }
        else
        {
            void* edx_1 = nullptr;
            int32_t* eax = &data_409238;
            
            while (true)
            {
                if (*eax == CodePage)
                {
                    void* i = nullptr;
                    int32_t esi_2 = edx_1 * 0x30;
                    *__builtin_memset(0x409860, 0, 0x100) = 0;
                    char* ebx_1 = esi_2 + 0x409248;
                    
                    do
                    {
                        char* ecx_2 = ebx_1;
                        
                        if (*ebx_1)
                        {
                            do
                            {
                                edx_1 = ecx_2[1];
                                
                                if (!edx_1)
                                    break;
                                
                                uint32_t eax_2 = *ecx_2;
                                uint32_t edi_5 = edx_1;
                                
                                if (eax_2 <= edi_5)
                                {
                                    edx_1 = *(i + 0x409230);
                                    
                                    do
                                    {
                                        *(eax_2 + 0x409861) |= edx_1;
                                        eax_2 += 1;
                                    } while (eax_2 <= edi_5);
                                }
                                
                                ecx_2 = &ecx_2[2];
                            } while (*ecx_2);
                        }
                        
                        i += 1;
                        ebx_1 = &ebx_1[8];
                    } while (i < 4);
                    
                    data_40975c = 1;
                    data_409748 = CodePage;
                    int32_t eax_4 = sub_40439a(CodePage);
                    data_409750 = *(esi_2 + 0x40923c);
                    void* edi_6 = &data_409754;
                    void* esi_4 = esi_2 + 0x409240;
                    *edi_6 = *esi_4;
                    data_409964 = eax_4;
                    *(edi_6 + 4) = *(esi_4 + 4);
                    break;
                }
                
                eax = &eax[0xc];
                edx_1 += 1;
                
                if (eax >= 0x409328)
                {
                    CPINFO cPInfo;
                    BOOL eax_1;
                    uint32_t edx_2;
                    eax_1 = GetCPInfo(CodePage, &cPInfo);
                    
                    if (eax_1 != 1)
                    {
                        if (!data_409710)
                            return 0xffffffff;
                        
                        goto label_40433a;
                    }
                    
                    bool cond:2_1 = cPInfo.MaxCharSize <= 1;
                    data_409748 = CodePage;
                    *__builtin_memset(0x409860, 0, 0x100) = 0;
                    data_409964 = 0;
                    
                    if (cond:2_1)
                        data_40975c = 0;
                    else
                    {
                        if (cPInfo.LeadByte[0])
                        {
                            var_15;
                            void* ecx_1 = &var_15;
                            
                            do
                            {
                                edx_2 = *ecx_1;
                                
                                if (!edx_2)
                                    break;
                                
                                for (uint32_t i_1 = *(ecx_1 - 1); i_1 <= edx_2; i_1 += 1)
                                    *(i_1 + 0x409861) |= 4;
                                
                                ecx_1 += 2;
                            } while (*(ecx_1 - 1));
                        }
                        
                        for (void* i_2 = 1; i_2 < 0xff; i_2 += 1)
                            *(i_2 + 0x409861) |= 8;
                        
                        data_409964 = sub_40439a(CodePage);
                        data_40975c = 1;
                    }
                    
                    data_409750 = 0;
                    void* edi_9 = &data_409754;
                    *edi_9 = 0;
                    *(edi_9 + 4) = 0;
                    goto label_40433f;
                }
            }
        }
        
    label_40433f:
        sub_4043f6();
    }
    
    return 0;
}

int32_t sub_404350(int32_t arg1)
{
    int32_t result = arg1;
    data_409710 = 0;
    
    if (result == 0xfffffffe)
    {
        data_409710 = 1;
        /* tailcall */
        return GetOEMCP();
    }
    
    if (result == 0xfffffffd)
    {
        data_409710 = 1;
        /* tailcall */
        return GetACP();
    }
    
    if (result == 0xfffffffc)
    {
        result = data_409738;
        data_409710 = 1;
    }
    
    return result;
}

int32_t sub_40439a(int32_t arg1) __pure
{
    if (arg1 == 0x3a4)
        return 0x411;
    
    if (arg1 == 0x3a8)
        return 0x804;
    
    if (arg1 == 0x3b5)
        return 0x412;
    
    if (arg1 == 0x3b6)
        return 0x404;
    
    return 0;
}

int32_t sub_4043cd()
{
    *__builtin_memset(0x409860, 0, 0x100) = 0;
    data_409748 = 0;
    data_40975c = 0;
    data_409964 = 0;
    data_409750 = 0;
    int32_t* edi_2 = &data_409754;
    *edi_2 = 0;
    edi_2[1] = 0;
    return 0;
}

void* sub_4043f6()
{
    CPINFO cPInfo;
    void* i;
    
    if (GetCPInfo(data_409748, &cPInfo) != 1)
    {
        for (i = nullptr; i < 0x100; i += 1)
        {
            if (i >= 0x41 && i <= 0x5a)
            {
                *(i + 0x409861) |= 0x10;
                *(i + 0x409760) = i + 0x20;
            }
            else if (i < 0x61 || i > 0x7a)
                *(i + 0x409760) = 0;
            else
            {
                *(i + 0x409861) |= 0x20;
                *(i + 0x409760) = i - 0x20;
            }
        }
    }
    else
    {
        uint8_t var_118[0x100];
        uint32_t i_1;
        
        for (i_1 = 0; i_1 < 0x100; i_1 += 1)
            var_118[i_1] = i_1;
        
        i_1 = cPInfo.LeadByte[0];
        var_118[0] = 0x20;
        
        if (i_1)
        {
            var_11;
            void* edx_1 = &var_11;
            
            do
            {
                uint32_t ecx_2 = *edx_1;
                i_1 = i_1;
                
                if (i_1 <= ecx_2)
                {
                    int32_t ecx_4 = ecx_2 - i_1 + 1;
                    int32_t __saved_ebp;
                    __builtin_memset(
                        __builtin_memset(&__saved_ebp + i_1 - 0x114, 0x20202020, ecx_4 >> 2 << 2), 
                        0x20, ecx_4 & 3);
                }
                
                edx_1 += 2;
                i_1 = *(edx_1 - 1);
            } while (i_1);
        }
        
        uint16_t var_518[0x100];
        sub_405267(1, &var_118, 0x100, &var_518, data_409748, data_409964, 0);
        char var_218[0x100];
        sub_405018(data_409964, 0x100, &var_118, 0x100, &var_218, 0x100, data_409748, 0);
        char var_318[0x100];
        sub_405018(data_409964, 0x200, &var_118, 0x100, &var_318, 0x100, data_409748, 0);
        i = nullptr;
        uint16_t (* ecx_8)[0x100] = &var_518;
        
        do
        {
            uint8_t edx_3 = *ecx_8;
            
            if (edx_3 & 1)
            {
                *(i + 0x409861) |= 0x10;
                *(i + 0x409760) = *(&var_218 + i);
            }
            else if (!(edx_3 & 2))
                *(i + 0x409760) = 0;
            else
            {
                *(i + 0x409861) |= 0x20;
                *(i + 0x409760) = *(&var_318 + i);
            }
            
            i += 1;
            ecx_8 = &(*ecx_8)[1];
        } while (i < 0x100);
    }
    
    return i;
}

void sub_40457b()
{
    if (!data_409aa8)
    {
        sub_4041b7(0xfffffffd);
        data_409aa8 = 1;
    }
}

char* sub_4045a0(char* arg1, char* arg2)
{
    char* edi = arg1;
    char* ecx = arg2;
    int32_t edx;
    
    while (ecx & 3)
    {
        edx = *ecx;
        ecx = &ecx[1];
        
        if (!edx)
            goto label_404688;
        
        *edi = edx;
        edi = &edi[1];
    }
    
    while (true)
    {
        int32_t eax_1 = *ecx;
        edx = *ecx;
        ecx = &ecx[4];
        
        if ((eax_1 ^ 0xffffffff ^ (0x7efefeff + eax_1)) & 0x81010100)
        {
            if (!edx)
                break;
            
            if (!*edx[1])
            {
                *edi = edx;
                return arg1;
            }
            
            if (!(edx & 0xff0000))
            {
                *edi = edx;
                edi[2] = 0;
                return arg1;
            }
            
            if (!(edx & 0xff000000))
            {
                *edi = edx;
                return arg1;
            }
        }
        
        *edi = edx;
        edi = &edi[4];
    }
    
label_404688:
    *edi = edx;
    return arg1;
}

char* sub_4045b0(char* arg1, char* arg2)
{
    char* ecx = arg1;
    void* edi;
    
    while (ecx & 3)
    {
        char eax = *ecx;
        ecx = &ecx[1];
        
        if (!eax)
        {
        label_4045ff:
            edi = &ecx[0xffffffff];
            goto label_404611;
        }
    }
    
    while (true)
    {
        int32_t eax_1 = *ecx;
        ecx = &ecx[4];
        
        if ((eax_1 ^ 0xffffffff ^ (0x7efefeff + eax_1)) & 0x81010100)
        {
            int32_t eax_4 = *(ecx - 4);
            
            if (!eax_4)
            {
                edi = &ecx[0xfffffffc];
                break;
            }
            
            if (!*eax_4[1])
            {
                edi = &ecx[0xfffffffd];
                break;
            }
            
            if (!(eax_4 & 0xff0000))
            {
                edi = &ecx[0xfffffffe];
                break;
            }
            
            if (!(eax_4 & 0xff000000))
                goto label_4045ff;
        }
    }
    
label_404611:
    char* ecx_1 = arg2;
    int32_t edx;
    
    while (ecx_1 & 3)
    {
        edx = *ecx_1;
        ecx_1 = &ecx_1[1];
        
        if (!edx)
            goto label_404688;
        
        *edi = edx;
        edi += 1;
    }
    
    while (true)
    {
        int32_t eax_5 = *ecx_1;
        edx = *ecx_1;
        ecx_1 = &ecx_1[4];
        
        if ((eax_5 ^ 0xffffffff ^ (0x7efefeff + eax_5)) & 0x81010100)
        {
            if (!edx)
                break;
            
            if (!*edx[1])
            {
                *edi = edx;
                return arg1;
            }
            
            if (!(edx & 0xff0000))
            {
                *edi = edx;
                *(edi + 2) = 0;
                return arg1;
            }
            
            if (!(edx & 0xff000000))
            {
                *edi = edx;
                return arg1;
            }
        }
        
        *edi = edx;
        edi += 4;
    }
    
label_404688:
    *edi = edx;
    return arg1;
}

void* sub_404690(char* arg1)
{
    char* ecx = arg1;
    
    while (ecx & 3)
    {
        int32_t eax;
        eax = *ecx;
        ecx = &ecx[1];
        
        if (!eax)
            return &ecx[0xffffffff] - arg1;
    }
    
    while (true)
    {
        int32_t eax_2 = *ecx;
        ecx = &ecx[4];
        
        if ((eax_2 ^ 0xffffffff ^ (0x7efefeff + eax_2)) & 0x81010100)
        {
            int32_t eax_5 = *(ecx - 4);
            
            if (!eax_5)
                return &ecx[0xfffffffc] - arg1;
            
            if (!*eax_5[1])
                return &ecx[0xfffffffd] - arg1;
            
            if (!(eax_5 & 0xff0000))
                return &ecx[0xfffffffe] - arg1;
            
            if (!(eax_5 & 0xff000000))
                break;
        }
    }
    
    return &ecx[0xffffffff] - arg1;
}

int32_t sub_40470b(void* arg1, void** arg2, int32_t arg3)
{
    return sub_404722(arg1, arg2, arg3, 0);
}

int32_t sub_404722(void* arg1, void** arg2, int32_t arg3, int32_t arg4)
{
    int32_t result = 0;
    char* edi = arg1;
    int32_t ebx;
    ebx = *edi;
    void* esi = &edi[1];
    void* var_8 = esi;
    
    while (true)
    {
        BOOL eax_2;
        wchar16 (* ecx)[0x21];
        
        if (data_40953c <= 1)
        {
            ecx = data_409330;
            uint32_t eax_3;
            eax_3 = (*ecx)[ebx];
            eax_2 = eax_3 & 8;
        }
        else
        {
            uint32_t eax_1 = ebx;
            int32_t edx;
            eax_2 = sub_40547c(eax_1, edx, ecx, eax_1);
            ecx = 8;
        }
        
        if (!eax_2)
            break;
        
        ebx = *esi;
        esi += 1;
    }
    
    void* var_8_1 = esi;
    
    if (ebx == 0x2d)
    {
        arg4 |= 2;
    label_40477d:
        ebx = *esi;
        esi += 1;
        var_8_1 = esi;
    }
    else if (ebx == 0x2b)
        goto label_40477d;
    
    if (arg3 < 0 || arg3 == 1 || arg3 > 0x24)
    {
        char** eax_19 = arg2;
        
        if (eax_19)
            *eax_19 = edi;
        
        return 0;
    }
    
    int32_t ecx_1 = 0x10;
    
    if (arg3)
        goto label_4047cb;
    
    if (ebx == 0x30)
    {
        int32_t eax_4;
        eax_4 = *esi;
        
        if (eax_4 == 0x78 || eax_4 == 0x58)
        {
            arg3 = 0x10;
        label_4047cb:
            
            if (arg3 == 0x10 && ebx == 0x30)
            {
                eax_4 = *esi;
                
                if (eax_4 == 0x78 || eax_4 == 0x58)
                {
                    ebx = *(esi + 1);
                    var_8_1 = esi + 2;
                }
            }
        }
        else
            arg3 = 8;
    }
    else
        arg3 = 0xa;
    
    int32_t eax_5 = 0xffffffff;
    int32_t edx_1 = 0;
    void* eax_6 = COMBINE(edx_1, eax_5) / arg3;
    uint32_t edx_2 = COMBINE(edx_1, eax_5) % arg3;
    void* var_10_1 = eax_6;
    
    while (true)
    {
        uint32_t esi_3 = ebx;
        BOOL eax_7;
        
        if (data_40953c <= 1)
        {
            wchar16 (* eax_8)[0x21];
            eax_8 = (**&data_409330)[esi_3];
            eax_7 = eax_8 & 4;
        }
        else
        {
            eax_7 = sub_40547c(eax_6, edx_2, ecx_1, esi_3);
            ecx_1 = 4;
        }
        
        if (!eax_7)
        {
            BOOL eax_9;
            
            if (data_40953c <= 1)
            {
                wchar16 (* eax_10)[0x21];
                eax_10 = (**&data_409330)[esi_3];
                eax_9 = eax_10 & 0x103;
            }
            else
            {
                eax_9 = sub_40547c(eax_7, edx_2, ecx_1, esi_3);
                ecx_1 = 0x103;
            }
            
            if (!eax_9)
                break;
            
            int32_t var_20_1 = ebx;
            uint32_t eax_12;
            eax_12 = sub_4053b0(ecx_1);
            ecx_1 = eax_12 - 0x37;
        }
        else
            ecx_1 = ebx - 0x30;
        
        if (ecx_1 >= arg3)
            break;
        
        arg4 |= 8;
        
        if (result >= var_10_1 && result == var_10_1)
            edx_2 = COMBINE(0, 0xffffffff) % arg3;
        
        if (result < var_10_1 || (result == var_10_1 && ecx_1 <= edx_2))
            result = result * arg3 + ecx_1;
        else
            arg4 |= 4;
        
        eax_6 = var_8_1;
        var_8_1 += 1;
        ebx = *eax_6;
    }
    
    void* var_8_2 = var_8_1 - 1;
    
    if (!(arg4 & 8))
    {
        if (arg2)
            var_8_2 = arg1;
        
        result = 0;
    }
    else if (arg4 & 4)
    {
    label_4048db:
        data_4095a8 = 0x22;
        
        if (!(arg4 & 1))
        {
            int32_t ecx_6;
            ecx_6 = arg4 & 2;
            char temp4_1 = ecx_6;
            ecx_6 = -(ecx_6);
            result = -((ecx_6 - ecx_6)) + 0x7fffffff;
        }
        else
            result = 0xffffffff;
    }
    else if (!(arg4 & 1))
    {
        int32_t ecx_5 = arg4 & 2;
        
        if (ecx_5 && result > 0x80000000)
            goto label_4048db;
        
        if (!ecx_5 && result > 0x7fffffff)
            goto label_4048db;
    }
    
    if (arg2)
        *arg2 = var_8_2;
    
    if (!(arg4 & 2))
        return result;
    
    return -(result);
}

void* sub_404940(char* arg1, char arg2)
{
    int32_t eax;
    eax = arg2;
    char* edx = arg1;
    
    while (edx & 3)
    {
        char ecx = *edx;
        edx = &edx[1];
        
        if (ecx == eax)
            return &edx[0xffffffff];
        
        if (!ecx)
            return 0;
    }
    
    int32_t ebx_1 = eax | eax << 8;
    int32_t ebx_3 = ebx_1 << 0x10 | ebx_1;
    
    while (true)
    {
        int32_t ecx_1 = *edx;
        int32_t ecx_2 = ecx_1 ^ ebx_3;
        edx = &edx[4];
        
        if ((ecx_2 ^ 0xffffffff ^ (0x7efefeff + ecx_2)) & 0x81010100)
        {
            int32_t eax_9 = *(edx - 4);
            
            if (eax_9 == ebx_3)
                return &edx[0xfffffffc];
            
            if (!eax_9)
                break;
            
            if (*eax_9[1] == ebx_3)
                return &edx[0xfffffffd];
            
            if (!*eax_9[1])
                break;
            
            uint16_t eax_10 = eax_9 >> 0x10;
            
            if (eax_10 == ebx_3)
                return &edx[0xfffffffe];
            
            if (!eax_10)
                break;
            
            if (*eax_10[1] == ebx_3)
                return &edx[0xffffffff];
            
            if (!*eax_10[1])
                break;
        }
        else
        {
            int32_t eax_6 = (ecx_1 ^ 0xffffffff ^ (0x7efefeff + ecx_1)) & 0x81010100;
            
            if (eax_6)
            {
                if (eax_6 & 0x1010100)
                    break;
                
                if (!((0x7efefeff + ecx_1) & 0x80000000))
                    break;
            }
        }
    }
    
    return 0;
}

char* sub_404a00(char* arg1, void* arg2)
{
    char* ecx_1 = arg2;
    int16_t edx;
    edx = *ecx_1;
    char* result = arg1;
    
    if (!edx)
        return result;
    
    *edx[1] = ecx_1[1];
    
    if (*edx[1])
    {
        while (true)
        {
            void* ecx = arg2;
            int16_t eax;
            eax = *result;
            void* esi_2 = &result[1];
            
            if (eax != edx)
            {
                if (!eax)
                    return 0;
                
                while (true)
                {
                    eax = *esi_2;
                    esi_2 += 1;
                label_404a2c:
                    
                    if (eax == edx)
                        break;
                    
                    if (!eax)
                        return 0;
                }
            }
            
            eax = *esi_2;
            esi_2 += 1;
            
            if (eax != *edx[1])
                break;
            
            result = esi_2 - 1;
            
            while (true)
            {
                *eax[1] = *(ecx + 2);
                
                if (*eax[1])
                {
                    eax = *esi_2;
                    esi_2 += 2;
                    
                    if (eax != *eax[1])
                        break;
                    
                    eax = *(ecx + 3);
                    
                    if (eax)
                    {
                        *eax[1] = *(esi_2 - 1);
                        ecx += 2;
                        
                        if (eax != *eax[1])
                            break;
                        
                        continue;
                    }
                }
                
                return &result[0xffffffff];
            }
        }
        
        goto label_404a2c;
    }
    
    int32_t eax_2;
    eax_2 = edx;
    int32_t ebx;
    int32_t var_4_1 = ebx;
    char* edx_1 = arg1;
    
    while (edx_1 & 3)
    {
        ecx_1 = *edx_1;
        edx_1 = &edx_1[1];
        
        if (ecx_1 == eax_2)
            return &edx_1[0xffffffff];
        
        if (!ecx_1)
            return 0;
    }
    
    int32_t ebx_7 = eax_2 | eax_2 << 8;
    int32_t edi;
    int32_t var_8_1 = edi;
    int32_t esi;
    int32_t var_c_1 = esi;
    int32_t ebx_9 = ebx_7 << 0x10 | ebx_7;
    
    while (true)
    {
        int32_t ecx_2 = *edx_1;
        int32_t ecx_3 = ecx_2 ^ ebx_9;
        edx_1 = &edx_1[4];
        
        if ((ecx_3 ^ 0xffffffff ^ (0x7efefeff + ecx_3)) & 0x81010100)
        {
            int32_t eax_14 = *(edx_1 - 4);
            
            if (eax_14 == ebx_9)
                return &edx_1[0xfffffffc];
            
            if (!eax_14)
                break;
            
            if (*eax_14[1] == ebx_9)
                return &edx_1[0xfffffffd];
            
            if (!*eax_14[1])
                break;
            
            uint16_t eax_15 = eax_14 >> 0x10;
            
            if (eax_15 == ebx_9)
                return &edx_1[0xfffffffe];
            
            if (!eax_15)
                break;
            
            if (*eax_15[1] == ebx_9)
                return &edx_1[0xffffffff];
            
            if (!*eax_15[1])
                break;
        }
        else
        {
            int32_t eax_11 = (ecx_2 ^ 0xffffffff ^ (0x7efefeff + ecx_2)) & 0x81010100;
            
            if (eax_11)
            {
                if (eax_11 & 0x1010100)
                    break;
                
                if (!((0x7efefeff + ecx_2) & 0x80000000))
                    break;
            }
        }
    }
    
    return 0;
}

int32_t sub_404a80(void* arg1, void* arg2, int32_t arg3)
{
    int32_t i = arg3;
    
    if (i)
    {
        int32_t i_2 = i;
        void* edi_1 = arg1;
        void* esi_1 = edi_1;
        
        while (i)
        {
            bool cond:0_1 = 0 != *edi_1;
            edi_1 += 1;
            i -= 1;
            
            if (!cond:0_1)
                break;
        }
        
        int32_t i_1 = -(i) + i_2;
        void* edi_2 = esi_1;
        void* esi_2 = arg2;
        
        while (i_1)
        {
            bool cond:1_1 = *esi_2 == *edi_2;
            esi_2 += 1;
            edi_2 += 1;
            i_1 -= 1;
            
            if (!cond:1_1)
                break;
        }
        
        char eax_1 = *(esi_2 - 1);
        i = 0;
        char temp0_1 = *(edi_2 - 1);
        
        if (eax_1 > temp0_1)
            return ~i;
        
        if (eax_1 != temp0_1)
            return ~0xfffffffe;
    }
    
    return i;
}

void* const __convention("regparm") sub_404ac0(int32_t arg1)
{
    void arg_4;
    void* ecx = &arg_4;
    
    while (arg1 >= 0x1000)
    {
        ecx -= 0x1000;
        arg1 -= 0x1000;
        *ecx;
    }
    
    void* ecx_1 = ecx - arg1;
    *ecx_1;
    *(ecx_1 - 4) = __return_addr;
    return __return_addr;
}

int32_t sub_404aef(int32_t arg1, int32_t arg2, int32_t arg3)
{
    int32_t ebx = 0;
    
    if (!data_409714)
    {
        HMODULE hModule = LoadLibraryA("user32.dll");
        int32_t eax_1;
        
        if (hModule)
        {
            eax_1 = GetProcAddress(hModule, "MessageBoxA");
            data_409714 = eax_1;
        }
        
        if (!hModule || !eax_1)
            return 0;
        
        data_409718 = GetProcAddress(hModule, "GetActiveWindow");
        data_40971c = GetProcAddress(hModule, "GetLastActivePopup");
    }
    
    int32_t eax_4 = data_409718;
    
    if (eax_4)
    {
        ebx = eax_4();
        
        if (ebx)
        {
            int32_t eax_6 = data_40971c;
            
            if (eax_6)
                ebx = eax_6(ebx);
        }
    }
    
    return data_409714(ebx, arg1, arg2, arg3);
}

char* sub_404b80(char* arg1, char* arg2, int32_t arg3)
{
    int32_t ecx = arg3;
    
    if (!ecx)
        return arg1;
    
    int32_t ebx_1 = ecx;
    char* esi_1 = arg2;
    char* edi_1 = arg1;
    int32_t eax;
    uint32_t i_2;
    uint32_t i_3;
    
    if (esi_1 & 3)
    {
        do
        {
            eax = *esi_1;
            esi_1 = &esi_1[1];
            *edi_1 = eax;
            edi_1 = &edi_1[1];
            int32_t temp0_1 = ecx;
            ecx -= 1;
            
            if (temp0_1 == 1)
                return arg1;
            
            if (!eax)
            {
                while (edi_1 & 3)
                {
                    *edi_1 = eax;
                    edi_1 = &edi_1[1];
                    int32_t temp2_1 = ecx;
                    ecx -= 1;
                    
                    if (temp2_1 == 1)
                        return arg1;
                }
                
                ebx_1 = ecx;
                i_3 = ecx >> 2;
                
                if (!i_3)
                    goto label_404bfb;
                
            label_404c67:
                eax = 0;
                uint32_t i;
                
                do
                {
                    *edi_1 = 0;
                    edi_1 = &edi_1[4];
                    i = i_3;
                    i_3 -= 1;
                } while (i != 1);
            label_404c71:
                ebx_1 &= 3;
                
                if (ebx_1)
                    goto label_404bfb;
                
                return arg1;
            }
        } while (esi_1 & 3);
        
        ebx_1 = ecx;
        i_2 = ecx >> 2;
        
        if (i_2)
            goto label_404c0f;
        
    label_404bc0:
        ebx_1 &= 3;
        
        if (ebx_1)
            goto label_404bc5;
    }
    else
    {
        i_2 = ecx >> 2;
        
        if (i_2)
        {
        label_404c0f:
            uint32_t i_1;
            
            do
            {
                int32_t eax_3 = *esi_1;
                int32_t edx_2 = *esi_1;
                esi_1 = &esi_1[4];
                
                if ((eax_3 ^ 0xffffffff ^ (0x7efefeff + eax_3)) & 0x81010100)
                {
                    if (!edx_2)
                    {
                        *edi_1 = 0;
                    label_404c5f:
                        edi_1 = &edi_1[4];
                        eax = 0;
                        i_3 = i_2 - 1;
                        
                        if (i_2 == 1)
                            goto label_404c71;
                        
                        goto label_404c67;
                    }
                    
                    if (!*edx_2[1])
                    {
                        *edi_1 = edx_2 & 0xff;
                        goto label_404c5f;
                    }
                    
                    if (!(edx_2 & 0xff0000))
                    {
                        *edi_1 = edx_2 & 0xffff;
                        goto label_404c5f;
                    }
                    
                    if (!(edx_2 & 0xff000000))
                    {
                        *edi_1 = edx_2;
                        goto label_404c5f;
                    }
                }
                
                *edi_1 = edx_2;
                edi_1 = &edi_1[4];
                i_1 = i_2;
                i_2 -= 1;
            } while (i_1 != 1);
            goto label_404bc0;
        }
        
    label_404bc5:
        
        while (true)
        {
            eax = *esi_1;
            esi_1 = &esi_1[1];
            *edi_1 = eax;
            edi_1 = &edi_1[1];
            
            if (!eax)
            {
                while (true)
                {
                    int32_t temp3_1 = ebx_1;
                    ebx_1 -= 1;
                    
                    if (temp3_1 == 1)
                        return arg1;
                    
                label_404bfb:
                    *edi_1 = eax;
                    edi_1 = &edi_1[1];
                }
            }
            else
            {
                int32_t temp4_1 = ebx_1;
                ebx_1 -= 1;
                
                if (temp4_1 == 1)
                    break;
            }
        }
    }
    return arg1;
}

char* sub_404c80(char* arg1, char* arg2, int32_t arg3)
{
    char* esi = arg2;
    char* edi = arg1;
    uint32_t eax_1;
    
    if (edi > esi && edi < &esi[arg3])
    {
        void* esi_1 = &esi[arg3 - 4];
        void* edi_1 = &edi[arg3 - 4];
        int32_t edx_2;
        uint32_t ecx_4;
        
        if (!(edi_1 & 3))
        {
            ecx_4 = arg3 >> 2;
            edx_2 = arg3 & 3;
            
            if (ecx_4 >= 8)
            {
                edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), ecx_4 << 2);
                
                switch (edx_2)
                {
                    case 0:
                    {
                        return arg1;
                        break;
                    }
                    case 1:
                    {
                        goto label_404f78;
                    }
                    case 2:
                    {
                        goto label_404f88;
                    }
                    case 3:
                    {
                        goto label_404f9c;
                    }
                }
            }
        }
        else if (arg3 < 4)
            switch (arg3)
            {
                case 0:
                {
                    return arg1;
                    break;
                }
                case 1:
                {
                label_404f78:
                    eax_1 = *(esi_1 + 3);
                    *(edi_1 + 3) = eax_1;
                    return arg1;
                    break;
                }
                case 2:
                {
                label_404f88:
                    eax_1 = *(esi_1 + 3);
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    *(edi_1 + 2) = eax_1;
                    return arg1;
                    break;
                }
                case 3:
                {
                label_404f9c:
                    eax_1 = *(esi_1 + 3);
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    *(edi_1 + 2) = eax_1;
                    eax_1 = *(esi_1 + 1);
                    *(edi_1 + 1) = eax_1;
                    return arg1;
                    break;
                }
            }
        else
        {
            eax_1 = edi_1 & 3;
            int32_t ecx_6 = arg3 - eax_1;
            
            switch (jump_table_404e68[eax_1])
            {
                case 0x404e78:
                {
                    eax_1 = *(esi_1 + 3);
                    edx_2 = 3 & ecx_6;
                    *(edi_1 + 3) = eax_1;
                    esi_1 -= 1;
                    ecx_4 = ecx_6 >> 2;
                    edi_1 -= 1;
                    
                    if (ecx_4 >= 8)
                    {
                        edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), 
                            ecx_4 << 2);
                        
                        switch (edx_2)
                        {
                            case 0:
                            {
                                return arg1;
                                break;
                            }
                            case 1:
                            {
                                goto label_404f78;
                            }
                            case 2:
                            {
                                goto label_404f88;
                            }
                            case 3:
                            {
                                goto label_404f9c;
                            }
                        }
                    }
                    break;
                }
                case 0x404e98:
                {
                    eax_1 = *(esi_1 + 3);
                    edx_2 = 3 & ecx_6;
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    ecx_4 = ecx_6 >> 2;
                    *(edi_1 + 2) = eax_1;
                    esi_1 -= 2;
                    edi_1 -= 2;
                    
                    if (ecx_4 >= 8)
                    {
                        edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), 
                            ecx_4 << 2);
                        
                        switch (edx_2)
                        {
                            case 0:
                            {
                                return arg1;
                                break;
                            }
                            case 1:
                            {
                                goto label_404f78;
                            }
                            case 2:
                            {
                                goto label_404f88;
                            }
                            case 3:
                            {
                                goto label_404f9c;
                            }
                        }
                    }
                    break;
                }
                case 0x404ec0:
                {
                    eax_1 = *(esi_1 + 3);
                    edx_2 = 3 & ecx_6;
                    *(edi_1 + 3) = eax_1;
                    eax_1 = *(esi_1 + 2);
                    *(edi_1 + 2) = eax_1;
                    eax_1 = *(esi_1 + 1);
                    ecx_4 = ecx_6 >> 2;
                    *(edi_1 + 1) = eax_1;
                    esi_1 -= 3;
                    edi_1 -= 3;
                    
                    if (ecx_4 >= 8)
                    {
                        edi_1 = __builtin_memcpy(edi_1 - (ecx_4 << 2), esi_1 - (ecx_4 << 2), 
                            ecx_4 << 2);
                        
                        switch (edx_2)
                        {
                            case 0:
                            {
                                return arg1;
                                break;
                            }
                            case 1:
                            {
                                goto label_404f78;
                            }
                            case 2:
                            {
                                goto label_404f88;
                            }
                            case 3:
                            {
                                goto label_404f9c;
                            }
                        }
                    }
                    break;
                }
            }
        }
        
        switch (edx_2)
        {
            case 0:
            {
                return arg1;
                break;
            }
            case 1:
            {
                goto label_404f78;
            }
            case 2:
            {
                goto label_404f88;
            }
            case 3:
            {
                goto label_404f9c;
            }
        }
    }
    
    uint32_t ecx_1;
    int32_t edx_1;
    
    if (edi & 3)
    {
        if (arg3 < 4)
            /* jump -> *(((arg3 - 4) << 2) + &data_404dd8) */
        
        eax_1 = edi & 3;
        int32_t ecx_3 = arg3 - 4 + eax_1;
        
        switch (jump_table_404ce0[eax_1])
        {
            case 0x404cf0:
            {
                edx_1 = 3 & ecx_3;
                eax_1 = *esi;
                *edi = eax_1;
                eax_1 = esi[1];
                edi[1] = eax_1;
                eax_1 = esi[2];
                ecx_1 = ecx_3 >> 2;
                edi[2] = eax_1;
                esi = &esi[3];
                edi = &edi[3];
                
                if (ecx_1 >= 8)
                {
                    edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
                    
                    switch (edx_1)
                    {
                        case 0:
                        {
                            return arg1;
                            break;
                        }
                        case 1:
                        {
                            goto label_404de0;
                        }
                        case 2:
                        {
                            goto label_404dec;
                        }
                        case 3:
                        {
                            goto label_404e00;
                        }
                    }
                }
                break;
            }
            case 0x404d1c:
            {
                edx_1 = 3 & ecx_3;
                eax_1 = *esi;
                *edi = eax_1;
                eax_1 = esi[1];
                ecx_1 = ecx_3 >> 2;
                edi[1] = eax_1;
                esi = &esi[2];
                edi = &edi[2];
                
                if (ecx_1 >= 8)
                {
                    edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
                    
                    switch (edx_1)
                    {
                        case 0:
                        {
                            return arg1;
                            break;
                        }
                        case 1:
                        {
                            goto label_404de0;
                        }
                        case 2:
                        {
                            goto label_404dec;
                        }
                        case 3:
                        {
                            goto label_404e00;
                        }
                    }
                }
                break;
            }
            case 0x404d40:
            {
                edx_1 = 3 & ecx_3;
                eax_1 = *esi;
                *edi = eax_1;
                esi = &esi[1];
                ecx_1 = ecx_3 >> 2;
                edi = &edi[1];
                
                if (ecx_1 >= 8)
                {
                    edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
                    
                    switch (edx_1)
                    {
                        case 0:
                        {
                            return arg1;
                            break;
                        }
                        case 1:
                        {
                            goto label_404de0;
                        }
                        case 2:
                        {
                            goto label_404dec;
                        }
                        case 3:
                        {
                            goto label_404e00;
                        }
                    }
                }
                break;
            }
        }
    }
    else
    {
        ecx_1 = arg3 >> 2;
        edx_1 = arg3 & 3;
        
        if (ecx_1 >= 8)
        {
            edi = __builtin_memcpy(edi, esi, ecx_1 << 2);
            
            switch (edx_1)
            {
                case 0:
                {
                    return arg1;
                    break;
                }
                case 1:
                {
                    goto label_404de0;
                }
                case 2:
                {
                    goto label_404dec;
                }
                case 3:
                {
                    goto label_404e00;
                }
            }
        }
    }
    
    switch (ecx_1)
    {
        case 0:
        {
            goto label_404dbf;
        }
        case 1:
        {
            goto label_404db0;
        }
        case 2:
        {
            goto label_404da8;
        }
        case 3:
        {
            goto label_404da0;
        }
        case 4:
        {
            goto label_404d98;
        }
        case 5:
        {
            goto label_404d90;
        }
        case 6:
        {
            goto label_404d88;
        }
        case 7:
        {
            *(edi + (ecx_1 << 2) - 0x1c) = *(esi + (ecx_1 << 2) - 0x1c);
        label_404d88:
            *(edi + (ecx_1 << 2) - 0x18) = *(esi + (ecx_1 << 2) - 0x18);
        label_404d90:
            *(edi + (ecx_1 << 2) - 0x14) = *(esi + (ecx_1 << 2) - 0x14);
        label_404d98:
            *(edi + (ecx_1 << 2) - 0x10) = *(esi + (ecx_1 << 2) - 0x10);
        label_404da0:
            *(edi + (ecx_1 << 2) - 0xc) = *(esi + (ecx_1 << 2) - 0xc);
        label_404da8:
            *(edi + (ecx_1 << 2) - 8) = *(esi + (ecx_1 << 2) - 8);
        label_404db0:
            *(edi + (ecx_1 << 2) - 4) = *(esi + (ecx_1 << 2) - 4);
            eax_1 = ecx_1 << 2;
            esi = &esi[eax_1];
            edi = &edi[eax_1];
        label_404dbf:
            
            switch (edx_1)
            {
                case 0:
                {
                    return arg1;
                    break;
                }
                case 1:
                {
                label_404de0:
                    eax_1 = *esi;
                    *edi = eax_1;
                    return arg1;
                    break;
                }
                case 2:
                {
                label_404dec:
                    eax_1 = *esi;
                    *edi = eax_1;
                    eax_1 = esi[1];
                    edi[1] = eax_1;
                    return arg1;
                    break;
                }
                case 3:
                {
                label_404e00:
                    eax_1 = *esi;
                    *edi = eax_1;
                    eax_1 = esi[1];
                    edi[1] = eax_1;
                    eax_1 = esi[2];
                    edi[2] = eax_1;
                    return arg1;
                    break;
                }
            }
            break;
        }
    }
}

char* sub_404fc0(char* arg1, char arg2, int32_t arg3)
{
    int32_t i_3 = arg3;
    
    if (!i_3)
        return arg1;
    
    int32_t eax;
    eax = arg2;
    char* edi = arg1;
    
    if (i_3 < 4)
    {
    label_40500b:
        int32_t i;
        
        do
        {
            *edi = eax;
            edi = &edi[1];
            i = i_3;
            i_3 -= 1;
        } while (i != 1);
    }
    else
    {
        int32_t i_2 = -(arg1) & 3;
        
        if (i_2)
        {
            i_3 -= i_2;
            int32_t i_1;
            
            do
            {
                *edi = eax;
                edi = &edi[1];
                i_1 = i_2;
                i_2 -= 1;
            } while (i_1 != 1);
        }
        
        eax *= 0x1010101;
        int32_t i_4 = i_3;
        i_3 &= 3;
        uint32_t ecx_4 = i_4 >> 2;
        
        if (!ecx_4)
            goto label_40500b;
        
        int32_t ecx_5;
        edi = __memfill_u32(edi, eax, ecx_4);
        
        if (i_3)
            goto label_40500b;
    }
    
    return arg1;
}

int32_t sub_405018(uint32_t arg1, uint32_t arg2, uint8_t* arg3, void* arg4, PSTR arg5, int32_t arg6, uint32_t arg7, int32_t arg8)
{
    int32_t var_8 = 0xffffffff;
    int32_t var_c = 0x406498;
    int32_t var_10 = 0x4029a8;
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    fsbase->NtTib.ExceptionList = &ExceptionList;
    int32_t __saved_edi;
    int32_t* var_1c = &__saved_edi;
    
    if (data_409740)
        goto label_405091;
    
    int32_t result;
    
    if (!LCMapStringW(0, 0x100, &data_406494, 1, nullptr, 0))
    {
        if (LCMapStringA(0, 0x100, &data_406490, 1, nullptr, 0))
        {
            data_409740 = 2;
            goto label_405091;
        }
        
        result = 0;
    }
    else
    {
        data_409740 = 1;
    label_405091:
        
        if (arg4 > 0)
            arg4 = sub_40523c(arg3, arg4);
        
        int32_t eax_4 = data_409740;
        
        if (eax_4 == 2)
            result = LCMapStringA(arg1, arg2, arg3, arg4, arg5, arg6);
        else if (eax_4 != 1)
            result = 0;
        else
        {
            if (!arg7)
                arg7 = data_409738;
            
            int32_t eax_7 = -(arg8);
            int32_t eax_11 =
                MultiByteToWideChar(arg7, ((eax_7 - eax_7) & 8) + 1, arg3, arg4, nullptr, 0);
            
            if (!eax_11)
                result = 0;
            else
            {
                int32_t var_8_1 = 0;
                int32_t eax_13;
                eax_13 = (eax_11 * 2 + 3) & 0xfc;
                sub_404ac0(eax_13);
                int32_t* var_1c_1 = &__saved_edi;
                int32_t* var_28_1 = &__saved_edi;
                int32_t var_8_2 = 0xffffffff;
                
                if (!var_28_1)
                    result = 0;
                else if (!MultiByteToWideChar(arg7, MB_PRECOMPOSED, arg3, arg4, var_28_1, eax_11))
                    result = 0;
                else
                {
                    int32_t result_1 = LCMapStringW(arg1, arg2, var_28_1, eax_11, nullptr, 0);
                    int32_t result_2 = result_1;
                    
                    if (!result_1)
                        result = 0;
                    else if (!(*arg2[1] & 4))
                    {
                        int32_t var_8_3 = 1;
                        int32_t eax_18;
                        eax_18 = (result_1 * 2 + 3) & 0xfc;
                        sub_404ac0(eax_18);
                        int32_t* var_1c_2 = &__saved_edi;
                        int32_t* var_24_1 = &__saved_edi;
                        int32_t var_8_4 = 0xffffffff;
                        
                        if (!&__saved_edi)
                            result = 0;
                        else if (
                                !LCMapStringW(arg1, arg2, var_28_1, eax_11, &__saved_edi, result_1))
                            result = 0;
                        else
                        {
                            PSTR lpMultiByteStr;
                            int32_t cbMultiByte;
                            
                            if (arg6)
                            {
                                cbMultiByte = arg6;
                                lpMultiByteStr = arg5;
                            }
                            else
                            {
                                cbMultiByte = 0;
                                lpMultiByteStr = nullptr;
                            }
                            
                            result_1 = WideCharToMultiByte(arg7, 0x220, &__saved_edi, result_1, 
                                lpMultiByteStr, cbMultiByte, nullptr, nullptr);
                            
                            result = !result_1 ? 0 : result_1;
                        }
                    }
                    else if (!arg6)
                        result = result_1;
                    else if (result_1 > arg6)
                        result = 0;
                    else if (LCMapStringW(arg1, arg2, var_28_1, eax_11, arg5, arg6))
                        result = result_1;
                    else
                        result = 0;
                }
            }
        }
    }
    
    fsbase->NtTib.ExceptionList = ExceptionList;
    return result;
}

int32_t sub_405128() __pure
{
    return 1;
}

int32_t sub_4051dc() __pure
{
    return 1;
}

void* sub_40523c(char* arg1, int32_t arg2)
{
    char* eax = arg1;
    int32_t ecx = arg2 - 1;
    
    if (arg2)
    {
        while (*eax)
        {
            eax = &eax[1];
            int32_t esi_1 = ecx;
            ecx -= 1;
            
            if (!esi_1)
                break;
        }
    }
    
    if (*eax)
        return arg2;
    
    return eax - arg1;
}

BOOL sub_405267(uint32_t arg1, uint8_t* arg2, int32_t arg3, uint16_t* arg4, uint32_t arg5, uint32_t arg6, int32_t arg7)
{
    int32_t var_8 = 0xffffffff;
    int32_t var_c = 0x4064b0;
    int32_t var_10 = 0x4029a8;
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    fsbase->NtTib.ExceptionList = &ExceptionList;
    int32_t __saved_edi;
    int32_t* var_1c = &__saved_edi;
    int32_t eax_1 = data_409744;
    
    if (eax_1)
        goto label_4052d6;
    
    uint16_t charType;
    BOOL result;
    
    if (!GetStringTypeW(1, &data_406494, 1, &charType))
    {
        if (GetStringTypeA(0, 1, &data_406490, 1, &charType))
        {
            eax_1 = 2;
            goto label_4052d1;
        }
        
        result = 0;
    }
    else
    {
        eax_1 = 1;
    label_4052d1:
        data_409744 = eax_1;
    label_4052d6:
        
        if (eax_1 == 2)
        {
            uint32_t Locale = arg6;
            
            if (!Locale)
                Locale = data_409728;
            
            result = GetStringTypeA(Locale, arg1, arg2, arg3, arg4);
        }
        else if (eax_1 != 1)
            result = 0;
        else
        {
            if (!arg5)
                arg5 = data_409738;
            
            int32_t eax_6 = -(arg7);
            int32_t cchWideChar =
                MultiByteToWideChar(arg5, ((eax_6 - eax_6) & 8) + 1, arg2, arg3, nullptr, 0);
            
            if (!cchWideChar)
                result = 0;
            else
            {
                int32_t var_8_1 = 0;
                int32_t edi_1 = cchWideChar * 2;
                int32_t eax_11;
                eax_11 = (edi_1 + 3) & 0xfc;
                sub_404ac0(eax_11);
                int32_t* var_1c_1 = &__saved_edi;
                int32_t* var_28_1 = &__saved_edi;
                sub_404fc0(&__saved_edi, 0, edi_1);
                int32_t var_8_2 = 0xffffffff;
                
                if (!&__saved_edi)
                    result = 0;
                else
                {
                    int32_t cchSrc = MultiByteToWideChar(arg5, MB_PRECOMPOSED, arg2, arg3, 
                        &__saved_edi, cchWideChar);
                    
                    if (!cchSrc)
                        result = 0;
                    else
                        result = GetStringTypeW(arg1, &__saved_edi, cchSrc, arg4);
                }
            }
        }
    }
    
    fsbase->NtTib.ExceptionList = ExceptionList;
    return result;
}

int32_t sub_405360() __pure
{
    return 1;
}

uint32_t __fastcall sub_4053b0(int32_t arg1)
{
    int32_t var_8 = arg1;
    uint32_t arg_4;
    
    if (!data_409728)
    {
        uint32_t eax_1 = arg_4;
        
        if (eax_1 >= 0x61 && eax_1 <= 0x7a)
            return eax_1 - 0x20;
        
        return eax_1;
    }
    
    uint32_t ebx_1 = arg_4;
    BOOL eax_2;
    
    if (ebx_1 < 0x100)
    {
        int32_t eax;
        int32_t edx;
        
        if (data_40953c <= 1)
        {
            wchar16 (* eax_3)[0x21];
            eax_3 = (**&data_409330)[ebx_1];
            eax_2 = eax_3 & 2;
        }
        else
            eax_2 = sub_40547c(eax, edx, arg1, ebx_1);
    }
    
    if (ebx_1 >= 0x100 || eax_2)
    {
        char eax_5 = ebx_1 >> 8;
        void* var_10_1;
        
        if (!(*(&(**&data_409330)[eax_5] + 1) & 0x80))
        {
            *arg_4[1] = 0;
            arg_4 = ebx_1;
            var_10_1 = 1;
        }
        else
        {
            *arg_4[2] = 0;
            arg_4 = eax_5;
            *arg_4[1] = ebx_1;
            var_10_1 = 2;
        }
        
        int32_t eax_7 = sub_405018(data_409728, 0x200, &arg_4, var_10_1, &var_8, 3, 0, 1);
        
        if (eax_7)
        {
            if (eax_7 != 1)
                return *var_8[1] << 8 | var_8;
            
            return var_8;
        }
    }
    
    return ebx_1;
}

BOOL __convention("regparm") sub_40547c(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    int32_t var_8 = arg3;
    int32_t arg_4;
    int32_t eax = arg_4;
    uint32_t eax_1;
    
    if (eax + 1 > 0x100)
    {
        char ecx_3 = eax >> 8;
        int32_t __saved_esi_1;
        
        if (!(*(&(**&data_409330)[ecx_3] + 1) & 0x80))
        {
            *var_8[1] = 0;
            var_8 = eax;
            __saved_esi_1 = 1;
        }
        else
        {
            *var_8[2] = 0;
            var_8 = ecx_3;
            *var_8[1] = eax;
            __saved_esi_1 = 2;
        }
        
        BOOL result = sub_405267(1, &var_8, __saved_esi_1, &*arg_4[2], 0, 0, 1);
        
        if (!result)
            return result;
        
        eax_1 = *arg_4[2];
    }
    else
        eax_1 = (**&data_409330)[eax];
    
    return eax_1 & arg4;
}

void __stdcall RtlUnwind(void* TargetFrame, void* TargetIp, EXCEPTION_RECORD* ExceptionRecord, void* ReturnValue)
{
    /* tailcall */
    return RtlUnwind(TargetFrame, TargetIp, ExceptionRecord, ReturnValue);
}


