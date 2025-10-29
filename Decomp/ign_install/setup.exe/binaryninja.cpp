int32_t sub_401000(PSTR arg1, PSTR arg2)
{
    int32_t eax = lstrlenA(arg1);
    
    if (eax > 0)
    {
        eax = arg1[eax - 1];
        
        if (eax != 0x5c && eax != 0x2f)
            lstrcatA(arg1, &data_40a4ec);
    }
    
    lstrcatA(arg1, arg2);
    int32_t result = lstrlenA(arg1);
    arg1[result + 1] = 0;
    return result;
}

HWND sub_401050(HWND arg1, int32_t arg2, PSTR arg3, int32_t arg4, int32_t arg5)
{
    HWND result = GetDlgItem(arg1, arg2);
    HWND hWnd = result;
    
    if (hWnd)
    {
        result = LoadImageA(data_40ba10, arg3, IMAGE_BITMAP, arg4, arg5, 
            LR_CREATEDIBSECTION | LR_LOADMAP3DCOLORS | LR_LOADTRANSPARENT);
        
        if (result)
        {
            result = SendMessageA(hWnd, 0x172, 0, result);
            
            if (result)
                return DeleteObject(result);
        }
    }
    
    return result;
}

BOOL sub_4010b0(HWND arg1, PSTR arg2)
{
    void var_80;
    void arglist;
    wvsprintfA(&var_80, arg2, &arglist);
    return SetDlgItemTextA(arg1, 0x3f1, &var_80);
}

HRESULT sub_4010f0(int32_t arg1)
{
    struct IMalloc ppMalloc;
    HRESULT result = SHGetMalloc(&ppMalloc);
    
    if (!ppMalloc)
        return result;
    
    struct IMalloc ppMalloc_1 = ppMalloc;
    (*(*ppMalloc_1 + 0x14))(ppMalloc_1, arg1);
    ppMalloc = __return_addr;
    return (*(*__return_addr + 8))(ppMalloc);
}

int32_t sub_401130(HWND arg1, int32_t arg2, int32_t arg3)
{
    sub_402820(0x1878);
    
    if (arg2 - 0x20 <= 0xf2)
    {
        int32_t ecx_1;
        ecx_1 = *(arg2 + 0x4021a0);
        char arg_58[0x200];
        void arg_b74;
        char arg_d74[0x200];
        
        switch (ecx_1)
        {
            case 0:
            {
                if (data_40b808)
                {
                    SetCursor(LoadCursorA(nullptr, 0x7f02));
                    return 1;
                }
                break;
            }
            case 1:
            {
                data_40b808 = 0;
                data_40b804 = 0;
                LoadStringA(GetModuleHandleA(nullptr), 0x65, 0x40b810, 0x200);
                SetWindowTextA(arg1, 0x40b810);
                EnableWindow(GetDlgItem(arg1, 0x3f0), 0);
                EnableWindow(GetDlgItem(arg1, 0x3ef), 0);
                LoadStringA(GetModuleHandleA(nullptr), 0x76, &arg_58, 0x200);
                sub_401050(arg1, 0x3ea, &arg_58, 0xaf, 0xc3);
                SendDlgItemMessageA(arg1, 0x3e8, 0xc5, 0x104, 0);
                LoadStringA(GetModuleHandleA(nullptr), 0x64, &arg_58, 0x200);
                SetDlgItemTextA(arg1, 0x3e8, &arg_58);
                SendDlgItemMessageA(arg1, 0x3e8, 0xb1, 0, 0x1000100);
                SetFocus(GetDlgItem(arg1, 0x3e8));
                break;
            }
            case 2:
            {
                struct BROWSEINFOA lpbi;
                int32_t nResult_1;
                
                if (arg3 == 1)
                {
                    int32_t ecx_2 = data_40b808;
                    
                    if (ecx_2 <= 0)
                    {
                        int32_t eax_11 = data_40b804;
                        
                        if (!eax_11)
                        {
                            data_40b808 = ecx_2 + 1;
                            EnableWindow(GetDlgItem(arg1, 1), 0);
                            EnableWindow(GetDlgItem(arg1, 2), 0);
                            SetCursor(LoadCursorA(nullptr, 0x7f02));
                            GetWindowTextA(GetDlgItem(arg1, 0x3e8), &data_40bb20, 0x106);
                            sub_406010(&data_40bb20);
                            
                            if (!sub_4027a0(&data_40bb20, "\IGNITION"))
                            {
                                char* edi_1 = "\IGNITION";
                                int32_t i = 0xffffffff;
                                
                                while (i)
                                {
                                    bool cond:2_1 = 0 != *edi_1;
                                    edi_1 = &edi_1[1];
                                    i -= 1;
                                    
                                    if (!cond:2_1)
                                        break;
                                }
                                
                                int32_t ecx_4 = ~i;
                                int32_t i_1 = 0xffffffff;
                                char* edi_3 = &data_40bb20;
                                
                                while (i_1)
                                {
                                    bool cond:3_1 = 0 != *edi_3;
                                    edi_3 = &edi_3[1];
                                    i_1 -= 1;
                                    
                                    if (!cond:3_1)
                                        break;
                                }
                                
                                int32_t esi_2;
                                int32_t edi_5;
                                edi_5 = __builtin_memcpy(edi_3 - 1, edi_1 - ecx_4, ecx_4 >> 2 << 2);
                                __builtin_memcpy(edi_5, esi_2, ecx_4 & 3);
                                int32_t edi_6 = 0x40a54c;
                                int32_t i_2 = 0xffffffff;
                                
                                while (i_2)
                                {
                                    bool cond:4_1 = 0 != *edi_6;
                                    edi_6 += 1;
                                    i_2 -= 1;
                                    
                                    if (!cond:4_1)
                                        break;
                                }
                                
                                int32_t ecx_9 = ~i_2;
                                int32_t i_3 = 0xffffffff;
                                char* edi_8 = &data_40bb20;
                                
                                while (i_3)
                                {
                                    bool cond:5_1 = 0 != *edi_8;
                                    edi_8 = &edi_8[1];
                                    i_3 -= 1;
                                    
                                    if (!cond:5_1)
                                        break;
                                }
                                
                                int32_t esi_4;
                                int32_t edi_10;
                                edi_10 =
                                    __builtin_memcpy(edi_8 - 1, edi_6 - ecx_9, ecx_9 >> 2 << 2);
                                __builtin_memcpy(edi_10, esi_4, ecx_9 & 3);
                            }
                            
                            sub_4022c0(arg1, 0x3f3, 0);
                            LoadStringA(GetModuleHandleA(nullptr), 0x78, &arg_58, 0x200);
                            sub_4010b0(arg1, &arg_58);
                            uint8_t arg_764[0x104];
                            GetWindowsDirectoryA(&arg_764, 0x104);
                            sub_401000(&arg_764, "WIN.INI");
                            void arg_258;
                            lstrcpyA(&arg_258, &data_40bb20);
                            sub_401000(&arg_258, "SMAG.INI");
                            SHFILEOPSTRUCTA fileOp;
                            fileOp.hwnd = arg1;
                            fileOp.pFrom = &arg_764;
                            fileOp.pTo = &arg_258;
                            fileOp.wFunc = 2;
                            fileOp.fFlags = 0x14;
                            
                            if (!SHFileOperationA(&fileOp))
                            {
                                fileOp.hwnd = arg1;
                                fileOp.pFrom = &arg_258;
                                fileOp.wFunc = 3;
                                fileOp.pTo = 0;
                                fileOp.fFlags = 0x14;
                                SHFileOperationA(&fileOp);
                                sub_4010b0(arg1, 0x40a534);
                                uint32_t numberOfFreeClusters;
                                uint32_t bytesPerSector;
                                uint32_t sectorsPerCluster;
                                arg_50;
                                void text;
                                enum MESSAGEBOX_RESULT nResult;
                                
                                if (data_40a048)
                                {
                                    nResult = nResult_1;
                                label_40178e:
                                    LoadStringA(GetModuleHandleA(nullptr), 0x82, &arg_58, 0x200);
                                    int32_t nResult_3;
                                    uint32_t edx_9;
                                    edx_9 = HIGHD(sub_402790(&arg_58) << 0x14);
                                    nResult_3 = LOWD(sub_402790(&arg_58) << 0x14);
                                    bool cond:6_1 = data_40bb21 != 0x3a;
                                    nResult_1 = nResult_3;
                                    uint32_t arg_360 = edx_9;
                                    
                                    if (cond:6_1 || data_40bb22 != 0x5c)
                                    {
                                        lpbi.hwndOwner = 0x6400000;
                                        lpbi.pidlRoot = 0;
                                    }
                                    else
                                    {
                                        nResult_3 = data_40bb20;
                                        edx_9 = data_40bb22;
                                        __return_addr = nResult_3;
                                        *__return_addr[1] = data_40bb21;
                                        *__return_addr[2] = edx_9;
                                        *__return_addr[3] = 0;
                                        GetDiskFreeSpaceA(&__return_addr, &sectorsPerCluster, 
                                            &bytesPerSector, &numberOfFreeClusters, &arg_50);
                                        int32_t eax_41;
                                        int32_t edx_11;
                                        eax_41 =
                                            __allmul(numberOfFreeClusters, 0, bytesPerSector, 0);
                                        int32_t eax_42;
                                        int32_t edx_12;
                                        eax_42 = __allmul(eax_41, edx_11, sectorsPerCluster, 0);
                                        lpbi.hwndOwner = eax_42;
                                        lpbi.pidlRoot = edx_12;
                                    }
                                    
                                    int32_t pidlRoot_1 = lpbi.pidlRoot;
                                    
                                    if (pidlRoot_1 > arg_360
                                        || (pidlRoot_1 >= arg_360 && lpbi.hwndOwner >= nResult_1))
                                    {
                                        LoadStringA(GetModuleHandleA(nullptr), 0x79, &arg_58, 
                                            0x200);
                                        sub_4010b0(arg1, &arg_58);
                                        BOOL eax_53 = sub_4010b0(arg1, 0x40a534);
                                        data_40bc28 = -((eax_53 - eax_53));
                                        LoadStringA(GetModuleHandleA(nullptr), 0x7c, &arg_58, 
                                            0x200);
                                        char (** i_4)[0xc] = &data_40a050;
                                        sub_4010b0(arg1, &arg_58);
                                        
                                        do
                                        {
                                            lstrcpyA(&arg_764, &data_40ba18);
                                            sub_401000(&arg_764, *i_4);
                                            lstrcpyA(&arg_258, &data_40bb20);
                                            sub_401000(&arg_258, *i_4);
                                            LoadStringA(GetModuleHandleA(nullptr), 0x7d, &arg_58, 
                                                0x200);
                                            char (* var_14_22)[0xc] = *i_4;
                                            sub_4010b0(arg1, &arg_58);
                                            fileOp.hwnd = arg1;
                                            fileOp.pFrom = &arg_764;
                                            fileOp.pTo = &arg_258;
                                            fileOp.fFlags = 0x214;
                                            fileOp.wFunc = 2;
                                            enum MESSAGEBOX_RESULT j;
                                            
                                            do
                                            {
                                                if (!SHFileOperationA(&fileOp))
                                                {
                                                    SetFileAttributesA(&arg_258, 
                                                        FILE_ATTRIBUTE_NORMAL);
                                                    break;
                                                }
                                                
                                                LoadStringA(GetModuleHandleA(nullptr), 0x7e, 
                                                    &arg_58, 0x200);
                                                wsprintfA(&nResult_1, &arg_58, *i_4);
                                                j = MessageBoxA(arg1, &nResult_1, 0x40b810, 
                                                    MB_RETRYCANCEL);
                                            } while (j != IDCANCEL);
                                            i_4 = &i_4[1];
                                        } while (i_4 < &data_40a4e8);
                                        
                                        sub_4010b0(arg1, 0x40a534);
                                        LoadStringA(GetModuleHandleA(nullptr), 0x7f, &arg_58, 
                                            0x200);
                                        sub_4010b0(arg1, &arg_58);
                                        enum WIN32_ERROR nResult_4 = RegCreateKeyExA(0x80000002, 
                                            "Software\UDS\Ignition", 0, nullptr, 
                                            REG_OPTION_RESERVED, KEY_ALL_ACCESS, nullptr, 
                                            &data_40a4e8, &data_40b800);
                                        nResult_1 = nResult_4;
                                        
                                        if (nResult_4)
                                            sub_402470(1);
                                        
                                        char* edi_11 = &data_40bb20;
                                        int32_t i_5 = 0xffffffff;
                                        
                                        while (i_5)
                                        {
                                            bool cond:9_1 = 0 != *edi_11;
                                            edi_11 = &edi_11[1];
                                            i_5 -= 1;
                                            
                                            if (!cond:9_1)
                                                break;
                                        }
                                        
                                        int32_t ecx_27 = ~i_5;
                                        uint8_t data[0x200];
                                        int32_t esi_6;
                                        int32_t edi_13;
                                        edi_13 = __builtin_memcpy(&data, edi_11 - ecx_27, 
                                            ecx_27 >> 2 << 2);
                                        __builtin_memcpy(edi_13, esi_6, ecx_27 & 3);
                                        uint8_t (* edi_14)[0x200] = &data;
                                        int32_t i_6 = 0xffffffff;
                                        
                                        while (i_6)
                                        {
                                            bool cond:10_1 = 0 != *edi_14;
                                            edi_14 = &(*edi_14)[1];
                                            i_6 -= 1;
                                            
                                            if (!cond:10_1)
                                                break;
                                        }
                                        
                                        RegSetValueExA(data_40a4e8, "Ignition Path", 0, REG_SZ, 
                                            &data, ~i_6 - 1);
                                        RegCloseKey(data_40a4e8);
                                        data_40a4e8 = 0;
                                        void buffer_1;
                                        LoadStringA(GetModuleHandleA(nullptr), 0x66, &buffer_1, 
                                            0x200);
                                        struct ITEMIDLIST* arg_54;
                                        SHGetSpecialFolderLocation(nullptr, 2, &arg_54);
                                        uint8_t pszPath[0x104];
                                        SHGetPathFromIDListA(arg_54, &pszPath);
                                        void* edi_15 = &data_40a4fc;
                                        void arg_660;
                                        wsprintfA(&arg_660, "%s\%s", &pszPath, &buffer_1);
                                        int32_t i_7 = 0xffffffff;
                                        
                                        while (i_7)
                                        {
                                            bool cond:11_1 = 0 != *edi_15;
                                            edi_15 += 1;
                                            i_7 -= 1;
                                            
                                            if (!cond:11_1)
                                                break;
                                        }
                                        
                                        int32_t ecx_34 = ~i_7;
                                        int32_t i_8 = 0xffffffff;
                                        void* edi_17 = &arg_660;
                                        
                                        while (i_8)
                                        {
                                            bool cond:12_1 = 0 != *edi_17;
                                            edi_17 += 1;
                                            i_8 -= 1;
                                            
                                            if (!cond:12_1)
                                                break;
                                        }
                                        
                                        int32_t esi_8;
                                        int32_t edi_19;
                                        edi_19 = __builtin_memcpy(edi_17 - 1, edi_15 - ecx_34, 
                                            ecx_34 >> 2 << 2);
                                        __builtin_memcpy(edi_19, esi_8, ecx_34 & 3);
                                        CreateDirectoryA(&arg_660, nullptr);
                                        SHChangeNotify(8, SHCNF_PATHA, &arg_660, nullptr);
                                        void param0_1;
                                        wsprintfA(&param0_1, "%s\%s", &data_40bb20, data_40a050);
                                        void buffer;
                                        LoadStringA(GetModuleHandleA(nullptr), 0x67, &buffer, 
                                            0x104);
                                        uint8_t param0[0x104];
                                        wsprintfA(&param0, "%s\%s.lnk", &arg_660, &buffer);
                                        sub_402360(&param0_1, &param0, 0x40a534, &data_40bb20);
                                        wsprintfA(&param0_1, "%s\%s", &data_40bb20, data_40a054);
                                        LoadStringA(GetModuleHandleA(nullptr), 0x68, &buffer, 
                                            0x104);
                                        wsprintfA(&param0, "%s\%s.lnk", &arg_660, &buffer);
                                        sub_402360(&param0_1, &param0, 0x40a534, &data_40bb20);
                                        wsprintfA(&param0_1, "%s\%s", &data_40bb20, data_40a058);
                                        LoadStringA(GetModuleHandleA(nullptr), 0x69, &buffer, 
                                            0x104);
                                        wsprintfA(&param0, "%s\%s.lnk", &arg_660, &buffer);
                                        sub_402360(&param0_1, &param0, 0x40a534, 0x40a534);
                                        sub_4010b0(arg1, 0x40a534);
                                        
                                        if (nResult_1 < 0)
                                            goto label_401fa2;
                                        
                                        ShowWindow(GetDlgItem(arg1, 0x3e8), SW_HIDE);
                                        ShowWindow(GetDlgItem(arg1, 0x3ed), SW_HIDE);
                                        ShowWindow(GetDlgItem(arg1, 0x3ee), SW_HIDE);
                                        ShowWindow(GetDlgItem(arg1, 0x3e9), SW_HIDE);
                                        ShowWindow(GetDlgItem(arg1, 0x3f1), SW_HIDE);
                                        ShowWindow(GetDlgItem(arg1, 0x3f3), SW_HIDE);
                                        char (* lpBuffer)[0x200] = &arg_58;
                                        
                                        if (!data_40bc28)
                                        {
                                            LoadStringA(GetModuleHandleA(nullptr), 0x3ed, lpBuffer, 
                                                0x200);
                                            SetDlgItemTextA(arg1, 0x3ee, &arg_58);
                                            ShowWindow(GetDlgItem(arg1, 0x3ee), SW_SHOW);
                                            LoadStringA(GetModuleHandleA(nullptr), 0x74, &arg_58, 
                                                0x200);
                                            SetWindowTextA(GetDlgItem(arg1, 1), &arg_58);
                                            data_40b804 += 1;
                                            EnableWindow(GetDlgItem(arg1, 1), 1);
                                            data_40b808 -= 1;
                                        }
                                        else
                                        {
                                            LoadStringA(GetModuleHandleA(nullptr), 0x3eb, lpBuffer, 
                                                0x200);
                                            SetDlgItemTextA(arg1, 0x3ed, &arg_58);
                                            ShowWindow(GetDlgItem(arg1, 0x3ed), SW_SHOW);
                                            LoadStringA(GetModuleHandleA(nullptr), 0x3ec, &arg_58, 
                                                0x200);
                                            SetDlgItemTextA(arg1, 0x3ee, &arg_58);
                                            ShowWindow(GetDlgItem(arg1, 0x3ee), SW_SHOW);
                                            LoadStringA(GetModuleHandleA(nullptr), 0x75, &arg_58, 
                                                0x200);
                                            SetWindowTextA(GetDlgItem(arg1, 1), &arg_58);
                                            LoadStringA(GetModuleHandleA(nullptr), 0x83, &arg_58, 
                                                0x200);
                                            SetWindowTextA(GetDlgItem(arg1, 2), &arg_58);
                                            LoadStringA(GetModuleHandleA(nullptr), 0x77, &arg_58, 
                                                0x200);
                                            sub_401050(arg1, 0x3ea, &arg_58, 0x10e, 0xc3);
                                            data_40b804 += 1;
                                        label_401fa2:
                                            EnableWindow(GetDlgItem(arg1, 1), 1);
                                            EnableWindow(GetDlgItem(arg1, 2), 1);
                                            data_40b808 -= 1;
                                            
                                            if (nResult_1 < 0)
                                                EndDialog(arg1, nResult_1);
                                        }
                                    }
                                    else
                                    {
                                        LoadStringA(GetModuleHandleA(nullptr), 0x70, &arg_d74, 
                                            0x200);
                                        uint32_t var_14_16 =
                                            __alldiv(lpbi.hwndOwner, lpbi.pidlRoot, 0x400, 0);
                                        uint32_t var_18_42 = __alldiv(nResult_1, arg_360, 0x400, 0);
                                        sub_402580(&text, &arg_d74);
                                        LoadStringA(GetModuleHandleA(nullptr), 0x71, &arg_b74, 
                                            0x200);
                                        MessageBoxA(arg1, &text, &arg_b74, MB_OK);
                                        EnableWindow(GetDlgItem(arg1, 1), 1);
                                        EnableWindow(GetDlgItem(arg1, 2), 1);
                                        SetCursor(LoadCursorA(nullptr, 0x7f00));
                                        sub_4022c0(arg1, 0x3f3, 1);
                                        data_40b808 -= 1;
                                    }
                                }
                                else
                                {
                                    data_40a048 = 1;
                                    LoadStringA(GetModuleHandleA(nullptr), 0x6e, &arg_d74, 0x200);
                                    LoadStringA(GetModuleHandleA(nullptr), 0x6f, &arg_b74, 0x200);
                                    nResult = MessageBoxA(arg1, &arg_d74, &arg_b74, MB_YESNO);
                                    sub_4010b0(arg1, 0x40a534);
                                    
                                    if (nResult != IDYES)
                                        goto label_40178e;
                                    
                                    LoadStringA(GetModuleHandleA(nullptr), 0x79, &arg_58, 0x200);
                                    sub_4010b0(arg1, &arg_58);
                                    LoadStringA(GetModuleHandleA(nullptr), 0x81, &arg_58, 0x200);
                                    int32_t nResult_2 = sub_402790(&arg_58) << 0x14;
                                    nResult_1 = nResult_2;
                                    int32_t eax_27;
                                    uint32_t edx_3;
                                    edx_3 = HIGHD(nResult_2);
                                    eax_27 = LOWD(nResult_2);
                                    GetDiskFreeSpaceA(&data_40a530, &sectorsPerCluster, 
                                        &bytesPerSector, &numberOfFreeClusters, &arg_50);
                                    int32_t eax_28;
                                    int32_t edx_5;
                                    eax_28 = __allmul(numberOfFreeClusters, 0, bytesPerSector, 0);
                                    int32_t eax_29;
                                    int32_t edx_6;
                                    eax_29 = __allmul(eax_28, edx_5, sectorsPerCluster, 0);
                                    lpbi.hwndOwner = eax_29;
                                    lpbi.pidlRoot = edx_6;
                                    int32_t pidlRoot = lpbi.pidlRoot;
                                    
                                    if (pidlRoot > edx_3
                                        || (pidlRoot >= edx_3 && lpbi.hwndOwner >= nResult_1))
                                    {
                                        LoadStringA(GetModuleHandleA(nullptr), 0x7a, &arg_58, 
                                            0x200);
                                        sub_4010b0(arg1, &arg_58);
                                        nResult = DirectXSetupA(arg1, 0, 0x10000a3f);
                                        sub_4010b0(arg1, 0x40a534);
                                        
                                        if (nResult >= 0)
                                            goto label_40178e;
                                        
                                        uint32_t uID;
                                        
                                        uID = nResult != 0xfffffff3 ? 0x7b : 0x80;
                                        
                                        LoadStringA(GetModuleHandleA(nullptr), uID, &arg_58, 0x200);
                                        MessageBoxA(arg1, &arg_58, 0x40b810, MB_OK);
                                        EndDialog(arg1, nResult);
                                    }
                                    else
                                    {
                                        LoadStringA(GetModuleHandleA(nullptr), 0x6c, &arg_d74, 
                                            0x200);
                                        uint32_t var_14_11 =
                                            __alldiv(lpbi.hwndOwner, lpbi.pidlRoot, 0x400, 0);
                                        uint32_t var_18_30 = __alldiv(nResult_1, edx_3, 0x400, 0);
                                        sub_402580(&text, &arg_d74);
                                        LoadStringA(GetModuleHandleA(nullptr), 0x6d, &arg_b74, 
                                            0x200);
                                        MessageBoxA(arg1, &text, &arg_b74, MB_OK);
                                        EndDialog(arg1, 0xffffffff);
                                    }
                                }
                            }
                            else
                            {
                                sub_4010b0(arg1, 0x40a534);
                                EnableWindow(GetDlgItem(arg1, 1), 1);
                                EnableWindow(GetDlgItem(arg1, 2), 1);
                                data_40b808 -= 1;
                            }
                        }
                        else if (eax_11 == 1)
                        {
                            if (!data_40bc28)
                                EndDialog(arg1, 0);
                            else
                                ExitWindowsEx(EWX_REBOOT, SHTDN_REASON_NONE);
                        }
                    }
                }
                else if (arg3 == 2)
                {
                    if (!data_40b808)
                        EndDialog(arg1, 0xffffffff);
                }
                else if (arg3 == 0x3f3 && !data_40b804)
                {
                    lpbi.hwndOwner = arg1;
                    lpbi.pidlRoot = 0;
                    lpbi.pszDisplayName = &nResult_1;
                    lpbi.lpszTitle = 0;
                    lpbi.ulFlags = 1;
                    lpbi.lpfn = 0;
                    lpbi.lParam = 0;
                    lpbi.iImage = 0;
                    struct ITEMIDLIST* pidl = SHBrowseForFolderA(&lpbi);
                    
                    if (pidl)
                    {
                        SHGetPathFromIDListA(pidl, &nResult_1);
                        SetDlgItemTextA(arg1, 0x3e8, &nResult_1);
                        sub_4010f0(pidl);
                    }
                }
                break;
            }
            case 3:
            {
                if (arg3 == 0xf060)
                {
                    LoadStringA(GetModuleHandleA(nullptr), 0x6a, &arg_d74, 0x200);
                    LoadStringA(GetModuleHandleA(nullptr), 0x6b, &arg_b74, 0x200);
                    
                    if (MessageBoxA(arg1, &arg_d74, &arg_b74, MB_YESNO) == IDYES)
                        EndDialog(arg1, 0xffffffff);
                }
                break;
            }
        }
    }
    
    return 0;
}

BOOL sub_4022c0(HWND arg1, int32_t arg2, BOOL arg3)
{
    return EnableWindow(GetDlgItem(arg1, arg2), arg3);
}

int32_t __stdcall sub_4022e0(HMODULE arg1)
{
    char* lpsz_1 = &data_40ba18;
    data_40ba10 = arg1;
    GetModuleFileNameA(arg1, &data_40ba18, 0x104);
    char* lpsz = &data_40ba18;
    
    if (data_40ba18)
    {
        do
        {
            char ecx_1 = *lpsz;
            
            if (ecx_1 == 0x5c || ecx_1 == 0x2f)
                lpsz_1 = lpsz;
            
            lpsz = CharNextA(lpsz);
        } while (*lpsz);
    }
    
    *lpsz_1 = 0;
    CoInitialize(nullptr);
    DialogBoxParamA(arg1, "DLG_MASTER", nullptr, sub_401130, 0);
    CoUninitialize();
    return 0;
}

HRESULT sub_402360(int32_t arg1, uint8_t* arg2, int32_t arg3, int32_t arg4)
{
    int32_t* ppv;
    HRESULT esi = CoCreateInstance(&data_407010, 0, CLSCTX_INPROC_SERVER, &data_407070, &ppv);
    
    if (esi >= 0)
    {
        int32_t* ppv_2 = ppv;
        (*(*ppv_2 + 0x50))(ppv_2, arg1);
        int32_t* ppv_3 = ppv;
        (*(*ppv_3 + 0x1c))(ppv_3, arg3);
        int32_t* ppv_4 = ppv;
        (*(*ppv_4 + 0x24))(ppv_4, arg4);
        int32_t* ppv_5 = ppv;
        int32_t* var_20c;
        esi = (**ppv_5)(ppv_5, 0x407360, &var_20c);
        
        if (esi >= 0)
        {
            wchar16 wideCharStr[0x104];
            MultiByteToWideChar(0, 0, arg2, 0xffffffff, &wideCharStr, 0x104);
            int32_t* eax_7 = var_20c;
            HRESULT eax_8 = (*(*eax_7 + 0x18))(eax_7, &wideCharStr, 1);
            esi = eax_8;
            
            if (esi < 0)
                return eax_8;
            
            int32_t* eax_9 = var_20c;
            (*(*eax_9 + 8))(eax_9);
        }
        
        int32_t* ppv_1 = ppv;
        (*(*ppv_1 + 8))(ppv_1);
    }
    
    return esi;
}

int32_t DirectXSetupA()
{
    /* tailcall */
    return DirectXSetupA();
}

int32_t sub_402440()
{
    int32_t eax_1 = data_40cf70;
    
    if (eax_1)
        eax_1();
    
    sub_402560(0x408008, 0x408010);
    return sub_402560(0x408000, 0x408004);
}

int32_t sub_402470(uint32_t arg1)
{
    return sub_4024b0(arg1, 0, 0);
}

int32_t sub_402490(uint32_t arg1)
{
    return sub_4024b0(arg1, 1, 0);
}

int32_t sub_4024b0(uint32_t arg1, int32_t arg2, int32_t arg3)
{
    if (data_40a5c0 == 1)
    {
        TerminateProcess(GetCurrentProcess(), arg1);
        /* no return */
    }
    
    data_40a5bc = 1;
    data_40a5b8 = arg3;
    
    if (!arg2)
    {
        if (data_40cf6c)
        {
            for (int32_t* i = data_40cf68 - 4; i >= data_40cf6c; i -= 4)
            {
                int32_t eax_2 = *i;
                
                if (eax_2)
                    eax_2();
            }
        }
        
        sub_402560(0x408014, 0x40801c);
    }
    
    int32_t result = sub_402560(0x408020, 0x408024);
    
    if (arg3)
        return result;
    
    data_40a5c0 = 1;
    ExitProcess(arg1);
    /* no return */
}

void sub_402560(int32_t* arg1, int32_t arg2)
{
    for (int32_t* i = arg1; arg2 > i; i = &i[1])
    {
        int32_t eax = *i;
        
        if (eax)
            eax();
    }
}

int32_t sub_402580(char* arg1, char* arg2)
{
    int32_t var_14 = 0x42;
    char* var_18 = arg1;
    void arg_c;
    void* var_28 = &arg_c;
    char* var_20 = arg1;
    int32_t var_1c = 0x7fffffff;
    int32_t result = sub_402b70(&var_20, arg2);
    int32_t var_1c_1 = var_1c - 1;
    
    if (var_1c - 1 < 0)
    {
        char** var_28_1 = &var_20;
        sub_402a20(nullptr);
        return result;
    }
    
    *var_20 = 0;
    var_20 = &var_20[1];
    return result;
}

uint32_t __stdcall __alldiv(int32_t arg1, uint32_t arg2, int32_t arg3, uint32_t arg4) __pure
{
    int32_t edi = 0;
    
    if (arg2 < 0)
    {
        edi = 1;
        arg2 = -(arg2) - 0;
        arg1 = -(arg1);
    }
    
    uint32_t i_1 = arg4;
    
    if (i_1 < 0)
    {
        edi += 1;
        i_1 = -(i_1) - 0;
        arg4 = i_1;
        arg3 = -(arg3);
    }
    
    uint32_t result;
    
    if (i_1)
    {
        uint32_t i = i_1;
        int32_t ecx_1 = arg3;
        uint32_t edx_8 = arg2;
        int32_t eax_10 = arg1;
        
        do
        {
            ecx_1 = RRCD(ecx_1, 1, i & 1);
            uint32_t temp6_1 = edx_8;
            edx_8 u>>= 1;
            eax_10 = RRCD(eax_10, 1, temp6_1 & 1);
            i u>>= 1;
        } while (i);
        
        uint32_t result_2 = COMBINE(edx_8, eax_10) / ecx_1;
        uint32_t result_1 = result_2;
        int32_t eax_12 = result_2 * arg4;
        int32_t eax_14;
        int32_t edx_9;
        edx_9 = HIGHD(arg3 * result_1);
        eax_14 = LOWD(arg3 * result_1);
        int32_t edx_10 = edx_9 + eax_12;
        
        if (edx_9 + eax_12 < edx_9 || edx_10 > arg2)
            result_1 -= 1;
        else if (edx_10 >= arg2 && eax_14 > arg1)
            result_1 -= 1;
        
        result = result_1;
    }
    else
        result = COMBINE(COMBINE(0, arg2) % arg3, arg1) / arg3;
    
    if (edi == 1)
        result = -(result);
    
    return result;
}

int32_t __stdcall __allmul(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4) __pure
{
    if (!(arg4 | arg2))
        return arg1 * arg3;
    
    int32_t result;
    int32_t edx;
    edx = HIGHD(arg1 * arg3);
    result = LOWD(arg1 * arg3);
    return result;
}

int32_t sub_4026e0(char* arg1)
{
    char* esi = arg1;
    
    while (true)
    {
        int32_t eax_2;
        
        if (data_40a7fc <= 1)
        {
            int32_t edx_1;
            edx_1 = *esi;
            int32_t eax_3;
            eax_3 = (**&data_40a5f0)[edx_1];
            eax_2 = eax_3 & 8;
        }
        else
        {
            int32_t eax_1;
            eax_1 = *esi;
            eax_2 = sub_403630(eax_1, 8);
        }
        
        if (!eax_2)
            break;
        
        esi = &esi[1];
    }
    
    int32_t ebx;
    ebx = *esi;
    void* esi_1 = &esi[1];
    int32_t edi = ebx;
    
    if (ebx == 0x2d || ebx == 0x2b)
    {
        ebx = *esi_1;
        esi_1 += 1;
    }
    
    int32_t result = 0;
    
    while (true)
    {
        int32_t eax_4;
        
        if (data_40a7fc <= 1)
        {
            int32_t eax_5;
            eax_5 = (**&data_40a5f0)[ebx];
            eax_4 = eax_5 & 4;
        }
        else
            eax_4 = sub_403630(ebx, 4);
        
        if (!eax_4)
            break;
        
        esi_1 += 1;
        result = ebx + result * 0xa - 0x30;
        ebx = *(esi_1 - 1);
    }
    
    if (edi != 0x2d)
        return result;
    
    return -(result);
}

int32_t sub_402790(char* arg1)
{
    return sub_4026e0(arg1);
}

char* sub_4027a0(char* arg1, void* arg2)
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
                label_4027cc:
                    
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
        
        goto label_4027cc;
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
            /* tailcall */
            return sub_4036d0(ecx_1, edx_1);
        
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

void* const __convention("regparm") sub_402820(int32_t arg1)
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

struct _EXCEPTION_REGISTRATION_RECORD* _start()
{
    int32_t var_8 = 0xffffffff;
    int32_t var_c = 0x407450;
    int32_t var_10 = 0x404474;
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    fsbase->NtTib.ExceptionList = &ExceptionList;
    int32_t __saved_edi;
    int32_t* var_1c = &__saved_edi;
    data_40a584 = GetVersion();
    int32_t eax_1;
    eax_1 = (*(data_40a584 + 1));
    data_40a590 = eax_1;
    char eax_2 = data_40a584;
    data_40a584 u>>= 0x10;
    uint32_t eax_3 = eax_2;
    data_40a58c = eax_3;
    data_40a588 = (eax_3 << 8) + data_40a590;
    
    if (!sub_404430())
        sub_4029f0(0x1c);
    
    int32_t var_8_1 = 0;
    sub_404250();
    sub_404240();
    data_40cf64 = GetCommandLineA();
    void* eax_8 = sub_403df0();
    data_40a5d0 = eax_8;
    
    if (!eax_8 || !data_40cf64)
        sub_402470(0xffffffff);
    
    sub_403b70();
    sub_403a80();
    sub_402440();
    char* esi = data_40cf64;
    char eax_9 = *esi;
    
    if (eax_9 == 0x22)
    {
        esi = &esi[1];
        
        if (*esi == 0x22)
            esi = &esi[1];
        else
        {
            char var_2c;
            int32_t ebx;
            ebx = var_2c;
            
            do
            {
                ebx = *esi;
                
                if (!ebx)
                    break;
                
                if (sub_403a20(ebx))
                    esi = &esi[1];
                
                esi = &esi[1];
            } while (*esi != 0x22);
            
            if (*esi == 0x22)
                esi = &esi[1];
        }
    }
    else if (eax_9 > 0x20)
    {
        do
            esi = &esi[1];
         while (*esi > 0x20);
    }
    
    if (*esi)
    {
        while (*esi <= 0x20)
        {
            esi = &esi[1];
            
            if (!*esi)
                break;
        }
    }
    
    STARTUPINFOA startupInfo;
    startupInfo.dwFlags = 0;
    GetStartupInfoA(&startupInfo);
    uint32_t cbReserved2;
    
    if (startupInfo.dwFlags & 1)
        cbReserved2 = startupInfo.cbReserved2;
    
    uint32_t cbReserved2_1 = cbReserved2;
    char* var_88 = esi;
    int32_t var_8c = 0;
    sub_4022e0(GetModuleHandleA(nullptr));
    sub_402470(0);
    int32_t var_8_2 = 0xffffffff;
    struct _EXCEPTION_REGISTRATION_RECORD* result = ExceptionList;
    fsbase->NtTib.ExceptionList = result;
    return result;
}

int32_t sub_4029a3(void* arg1 @ ebp)
{
    *(arg1 - 0x20) = ***(arg1 - 0x14);
    return sub_403890(*(arg1 - 0x20), *(arg1 - 0x14));
}

int32_t sub_4029f0(int32_t arg1)
{
    if (data_40a5dc == 1)
        sub_404550();
    
    sub_404590(arg1);
    return data_40a5d8(0xff);
}

uint32_t sub_402a20(int32_t* arg1)
{
    int32_t edi = arg1[4];
    int32_t eax = arg1[3];
    
    if (!(eax & 0x82) || eax & 0x40)
    {
        arg1[3] = eax | 0x20;
        return 0xffffffff;
    }
    
    if (eax & 1)
    {
        arg1[1] = 0;
        int32_t eax_1 = arg1[3];
        
        if (!(eax_1 & 0x10))
        {
            arg1[3] = eax_1 | 0x20;
            return 0xffffffff;
        }
        
        *arg1 = arg1[2];
        arg1[3] &= 0xfffffffe;
    }
    
    uint32_t ebp = 0;
    int32_t eax_4 = arg1[3] | 2;
    arg1[3] = eax_4;
    arg1[3] = eax_4 & 0xffffffef;
    arg1[1] = 0;
    
    if (!(arg1[3] & 0x10c))
    {
        if (arg1 == 0x40ab98 || arg1 == 0x40abb8)
        {
            if (!sub_404ad0(edi))
                sub_404a80(arg1);
        }
        else
            sub_404a80(arg1);
    }
    
    int32_t arg_4;
    uint32_t ebx_1;
    
    if (!(arg1[3] & 0x108))
    {
        ebx_1 = 1;
        ebp = sub_404790(edi, &arg_4, 1);
    }
    else
    {
        int32_t eax_7 = arg1[2];
        ebx_1 = *arg1 - eax_7;
        *arg1 = eax_7 + 1;
        arg1[1] = arg1[6] - 1;
        uint32_t eax_12;
        
        if (ebx_1 <= 0)
        {
            eax_12 = 0x40aad0;
            
            if (edi != 0xffffffff)
                eax_12 = *(((edi & 0xffffffe7) >> 3) + &data_40ce60) + ((edi & 0x1f) << 3);
            
            if (*(eax_12 + 4) & 0x20)
                sub_4049c0(edi, 0, FILE_END);
        }
        else
            ebp = sub_404790(edi, arg1[2], ebx_1);
        
        eax_12 = arg_4;
        *arg1[2] = eax_12;
    }
    
    if (ebp == ebx_1)
        return arg_4;
    
    arg1[3] |= 0x20;
    return 0xffffffff;
}

int32_t sub_402b70(int32_t* arg1, char* arg2)
{
    char* eax = arg2;
    arg2 = &arg2[1];
    int32_t i = 0;
    void* ebx;
    ebx = *eax;
    int32_t var_21c = 0;
    
    if (ebx)
    {
        int32_t* var_200;
        int32_t* esi_1 = var_200;
        int32_t* ebp_1 = var_200;
        int32_t* edi_1 = var_200;
        
        while (i >= 0)
        {
            int32_t eax_2;
            
            if (ebx < 0x20 || ebx > 0x78)
                eax_2 = 0;
            else
            {
                int32_t eax_1;
                eax_1 = *(ebx + &data_407458);
                eax_2 = eax_1 & 0xf;
            }
            
            eax_2 = *(var_21c + (eax_2 << 3) + 0x407478);
            eax_2 s>>= 4;
            int32_t ecx_3 = eax_2;
            var_21c = ecx_3;
            int32_t var_228;
            int32_t* var_224;
            int32_t var_220;
            int32_t var_210;
            int32_t var_20c;
            void* arg_c;
            
            switch (ecx_3)
            {
                case 0:
                {
                    goto label_402d4b;
                }
                case 1:
                {
                    var_20c = 0;
                    esi_1 = nullptr;
                    ebp_1 = 0xffffffff;
                    var_210 = 0;
                    var_228 = 0;
                    var_224 = nullptr;
                    var_220 = 0;
                    break;
                }
                case 2:
                {
                    if (ebx - 0x20 <= 0x10)
                    {
                        int32_t eax_4;
                        eax_4 = *(ebx + &jump_table_403408[6]);
                        
                        switch (eax_4)
                        {
                            case 0:
                            {
                                esi_1 |= 2;
                                break;
                            }
                            case 1:
                            {
                                esi_1 |= 0x80;
                                break;
                            }
                            case 2:
                            {
                                esi_1 |= 1;
                                break;
                            }
                            case 3:
                            {
                                esi_1 |= 4;
                                break;
                            }
                            case 4:
                            {
                                esi_1 |= 8;
                                break;
                            }
                        }
                    }
                    break;
                }
                case 3:
                {
                    if (ebx != 0x2a)
                        var_228 = ebx + var_228 * 0xa - 0x30;
                    else
                    {
                        int32_t eax_5 = sub_4035f0(&arg_c);
                        var_228 = eax_5;
                        
                        if (eax_5 < 0)
                        {
                            esi_1 |= 4;
                            var_228 = -(eax_5);
                        }
                    }
                    break;
                }
                case 4:
                {
                    ebp_1 = nullptr;
                    break;
                }
                case 5:
                {
                    if (ebx != 0x2a)
                        ebp_1 = ebx + ebp_1 * 0xa - 0x30;
                    else
                    {
                        ebp_1 = sub_4035f0(&arg_c);
                        
                        if (ebp_1 < 0)
                            ebp_1 = 0xffffffff;
                    }
                    break;
                }
                case 6:
                {
                    if (ebx - 0x49 <= 0x2e)
                    {
                        int32_t eax_10;
                        eax_10 = *(ebx + &*jump_table_403408[5][3]);
                        
                        switch (eax_10)
                        {
                            case 0:
                            {
                                if (*arg2 != 0x36 || arg2[1] != 0x34)
                                {
                                    var_21c = 0;
                                label_402d4b:
                                    int32_t eax_12;
                                    eax_12 = ebx;
                                    var_220 = 0;
                                    
                                    if (*(&(**&data_40a5f0)[eax_12] + 1) & 0x80)
                                    {
                                        sub_403520(ebx, arg1, &i);
                                        ebx = *arg2;
                                        arg2 = &arg2[1];
                                    }
                                    
                                    sub_403520(ebx, arg1, &i);
                                }
                                else
                                {
                                    arg2 = &arg2[2];
                                    esi_1 |= 0x8000;
                                }
                                break;
                            }
                            case 1:
                            {
                                esi_1 |= 0x20;
                                break;
                            }
                            case 2:
                            {
                                esi_1 |= 0x10;
                                break;
                            }
                            case 3:
                            {
                                esi_1 |= 0x800;
                                break;
                            }
                        }
                    }
                    break;
                }
                case 7:
                {
                    char var_246;
                    void* var_240;
                    
                    if (ebx - 0x43 <= 0x35)
                    {
                        int32_t eax_15;
                        eax_15 = *(ebx + &*jump_table_403498[0][1]);
                        int32_t var_218_1;
                        int32_t var_214;
                        
                        switch (eax_15)
                        {
                            case 0:
                            {
                                if (!(esi_1 & 0x830))
                                    esi_1 |= 0x800;
                                
                                goto label_402e10;
                            }
                            case 1:
                            case 2:
                            {
                                var_20c = 1;
                                ebx += 0x20;
                            label_402e4f:
                                esi_1 |= 0x40;
                                var_240 = &var_200;
                                
                                if (ebp_1 < 0)
                                    ebp_1 = 6;
                                else if (!ebp_1 && ebx == 0x67)
                                    ebp_1 = 1;
                                
                                arg_c += 8;
                                void* eax_47 = arg_c;
                                int32_t edx_11 = *(eax_47 - 4);
                                int32_t var_208 = *(eax_47 - 8);
                                int32_t var_204_1 = edx_11;
                                data_40ae00(&var_208, &var_200, ebx, ebp_1, var_20c);
                                void* edi_8 = esi_1 & 0x80;
                                
                                if (edi_8 && !ebp_1)
                                    data_40ae0c(&var_200);
                                
                                if (ebx == 0x67 && !edi_8)
                                    data_40ae04(&var_200);
                                
                                if (var_200 == 0x2d)
                                {
                                    esi_1 |= 0x100;
                                    var_240 = &*var_200[1];
                                }
                                
                                void* edi_9 = var_240;
                                int32_t j = 0xffffffff;
                                
                                while (j)
                                {
                                    bool cond:8_1 = 0 != *edi_9;
                                    edi_9 += 1;
                                    j -= 1;
                                    
                                    if (!cond:8_1)
                                        break;
                                }
                                
                                edi_1 = ~j - 1;
                                break;
                            }
                            case 3:
                            {
                                if (!(esi_1 & 0x830))
                                    esi_1 |= 0x800;
                                
                                goto label_402e7a;
                            }
                            case 4:
                            {
                                goto label_402fa8;
                            }
                            case 5:
                            {
                                int16_t* eax_22 = sub_4035f0(&arg_c);
                                void* ecx_17;
                                
                                if (eax_22)
                                    ecx_17 = *(eax_22 + 4);
                                
                                if (!eax_22 || !ecx_17)
                                {
                                    char const (* eax_23)[0x7] = data_40a5e4;
                                    int32_t j_1 = 0xffffffff;
                                    char const (* edi_5)[0x7] = eax_23;
                                    var_240 = eax_23;
                                    
                                    while (j_1)
                                    {
                                        bool cond:5_1 = 0 != *edi_5;
                                        edi_5 = &(*edi_5)[1];
                                        j_1 -= 1;
                                        
                                        if (!cond:5_1)
                                            break;
                                    }
                                    
                                    edi_1 = ~j_1 - 1;
                                }
                                else if (!(esi_1 & 0x800))
                                {
                                    var_220 = 0;
                                    edi_1 = *eax_22;
                                    var_240 = ecx_17;
                                }
                                else
                                {
                                    var_220 = 1;
                                    edi_1 = *eax_22 >> 1;
                                    var_240 = ecx_17;
                                }
                                break;
                            }
                            case 6:
                            {
                            label_402e10:
                                int32_t* var_25c_5 = &arg_c;
                                
                                if (!(esi_1 & 0x810))
                                {
                                    edi_1 = 1;
                                    var_200 = sub_4035f0(var_25c_5);
                                }
                                else
                                {
                                    edi_1 = sub_404bf0(&var_200, sub_403620(var_25c_5));
                                    
                                    if (edi_1 < 0)
                                        var_210 = 1;
                                }
                                
                                var_240 = &var_200;
                                break;
                            }
                            case 7:
                            case 9:
                            {
                                var_218_1 = 0xa;
                                esi_1 |= 0x40;
                            label_402fdd:
                                int32_t var_23c_1;
                                int32_t var_238_1;
                                
                                if (esi_1 & 0x8000)
                                {
                                    int32_t eax_26;
                                    int32_t edx_4;
                                    eax_26 = sub_403600(&arg_c);
                                    var_23c_1 = eax_26;
                                    var_238_1 = edx_4;
                                }
                                else if (!(esi_1 & 0x20))
                                {
                                    int32_t* var_25c_12 = &arg_c;
                                    
                                    if (!(esi_1 & 0x40))
                                    {
                                        var_23c_1 = sub_4035f0(var_25c_12);
                                        var_238_1 = 0;
                                    }
                                    else
                                    {
                                        int32_t eax_32 = sub_4035f0(var_25c_12);
                                        var_23c_1 = eax_32;
                                        int32_t eax_33;
                                        int32_t edx_6;
                                        edx_6 = HIGHD(eax_32);
                                        eax_33 = LOWD(eax_32);
                                        var_238_1 = edx_6;
                                    }
                                }
                                else
                                {
                                    int32_t* var_25c_11 = &arg_c;
                                    
                                    if (!(esi_1 & 0x40))
                                    {
                                        var_23c_1 = sub_4035f0(var_25c_11);
                                        var_238_1 = 0;
                                    }
                                    else
                                    {
                                        int32_t eax_28 = sub_4035f0(var_25c_11);
                                        var_23c_1 = eax_28;
                                        int32_t eax_29;
                                        int32_t edx_5;
                                        edx_5 = HIGHD(eax_28);
                                        eax_29 = LOWD(eax_28);
                                        var_238_1 = edx_5;
                                    }
                                }
                                
                                int32_t var_230_1;
                                int32_t var_22c_1;
                                
                                if (!(esi_1 & 0x40) || var_238_1 > 0
                                    || (var_238_1 >= 0 && var_23c_1 >= 0))
                                {
                                    var_230_1 = var_23c_1;
                                    var_22c_1 = var_238_1;
                                }
                                else
                                {
                                    var_230_1 = -(var_23c_1);
                                    esi_1 |= 0x100;
                                    var_22c_1 = -((var_238_1 + 0));
                                }
                                
                                if (!(esi_1 & 0x8000))
                                {
                                    var_230_1 &= 0xffffffff;
                                    var_22c_1 = 0;
                                }
                                
                                if (ebp_1 >= 0)
                                    esi_1 &= 0xfffffff7;
                                else
                                    ebp_1 = 1;
                                
                                if (!var_22c_1 && !var_230_1)
                                    var_224 = nullptr;
                                
                                void var_1;
                                void* var_240_1 = &var_1;
                                
                                while (true)
                                {
                                    int32_t* eax_37 = ebp_1;
                                    ebp_1 -= 1;
                                    
                                    if (eax_37 <= 0 && !var_22c_1 && !var_230_1)
                                        break;
                                    
                                    int32_t eax_39;
                                    uint32_t edx_7;
                                    edx_7 = HIGHD(var_218_1);
                                    eax_39 = LOWD(var_218_1);
                                    ebx = __aullrem(var_230_1, var_22c_1, eax_39, edx_7) + 0x30;
                                    uint32_t eax_43;
                                    int32_t edx_10;
                                    eax_43 = __aulldiv(var_230_1, var_22c_1, eax_39, edx_7);
                                    var_230_1 = eax_43;
                                    var_22c_1 = edx_10;
                                    
                                    if (ebx > 0x39)
                                        ebx += var_214;
                                    
                                    void* eax_44 = var_240_1;
                                    var_240_1 -= 1;
                                    *eax_44 = ebx;
                                }
                                
                                edi_1 = &var_1 - var_240_1;
                                var_240 = var_240_1 + 1;
                                
                                if (esi_1 & 0x200 && (*var_240 != 0x30 || !edi_1))
                                {
                                    edi_1 += 1;
                                    var_240 -= 1;
                                    *var_240 = 0x30;
                                }
                                break;
                            }
                            case 8:
                            {
                                goto label_402e4f;
                            }
                            case 0xa:
                            {
                                int16_t* eax_24 = sub_4035f0(&arg_c);
                                int32_t i_1 = i;
                                
                                if (!(esi_1 & 0x20))
                                    *eax_24 = i_1;
                                else
                                    *eax_24 = i_1;
                                
                                var_210 = 1;
                                break;
                            }
                            case 0xb:
                            {
                                var_218_1 = 8;
                                
                                if (esi_1 & 0x80)
                                    esi_1 |= 0x200;
                                
                                goto label_402fdd;
                            }
                            case 0xc:
                            {
                                ebp_1 = 8;
                            label_402fa8:
                                var_214 = 7;
                            label_402fb0:
                                var_218_1 = 0x10;
                                
                                if (esi_1 & 0x80)
                                {
                                    var_246 = 0x30;
                                    var_224 = 2;
                                    char var_245_1 = var_214 + 0x51;
                                }
                                
                                goto label_402fdd;
                            }
                            case 0xd:
                            {
                            label_402e7a:
                                int32_t* ebx_1 = 0x7fffffff;
                                
                                if (ebp_1 != 0xffffffff)
                                    ebx_1 = ebp_1;
                                
                                void* eax_18 = sub_4035f0(&arg_c);
                                var_240 = eax_18;
                                
                                if (!(esi_1 & 0x810))
                                {
                                    if (!var_240)
                                        var_240 = data_40a5e4;
                                    
                                    void* edi_10 = var_240;
                                    ebx = ebx_1 - 1;
                                    
                                    if (ebx_1)
                                    {
                                        while (*edi_10)
                                        {
                                            edi_10 += 1;
                                            void* eax_52 = ebx;
                                            ebx -= 1;
                                            
                                            if (!eax_52)
                                                break;
                                        }
                                    }
                                    
                                    edi_1 = edi_10 - var_240;
                                }
                                else
                                {
                                    if (!eax_18)
                                        var_240 = data_40a5e8;
                                    
                                    void* edi_2 = var_240;
                                    var_220 = 1;
                                    ebx = ebx_1 - 1;
                                    
                                    if (ebx_1)
                                    {
                                        while (*edi_2)
                                        {
                                            edi_2 += 2;
                                            void* eax_21 = ebx;
                                            ebx -= 1;
                                            
                                            if (!eax_21)
                                                break;
                                        }
                                    }
                                    
                                    edi_1 = (edi_2 - var_240) >> 1;
                                }
                                break;
                            }
                            case 0xe:
                            {
                                var_218_1 = 0xa;
                                goto label_402fdd;
                            }
                            case 0xf:
                            {
                                var_214 = 0x27;
                                goto label_402fb0;
                            }
                        }
                    }
                    
                    if (!var_210)
                    {
                        if (esi_1 & 0x40)
                        {
                            if (esi_1 & 0x100)
                            {
                                var_246 = 0x2d;
                                var_224 = 1;
                            }
                            else if (esi_1 & 1)
                            {
                                var_246 = 0x2b;
                                var_224 = 1;
                            }
                            else if (esi_1 & 2)
                            {
                                var_246 = 0x20;
                                var_224 = 1;
                            }
                        }
                        
                        int32_t* var_230_2 = var_228 - edi_1 - var_224;
                        
                        if (!(esi_1 & 0xc))
                            sub_403570(0x20, var_230_2, arg1, &i);
                        
                        sub_4035b0(&var_246, var_224, arg1, &i);
                        
                        if (esi_1 & 8 && !(esi_1 & 4))
                            sub_403570(0x30, var_230_2, arg1, &i);
                        
                        if (!var_220 || edi_1 <= 0)
                            sub_4035b0(var_240, edi_1, arg1, &i);
                        else
                        {
                            ebx = var_240;
                            void* j_3 = edi_1 - 1;
                            void* j_2;
                            
                            do
                            {
                                void* eax_57;
                                eax_57 = *ebx;
                                ebx += 2;
                                void var_244;
                                int32_t* eax_58 = sub_404bf0(&var_244, eax_57);
                                
                                if (eax_58 <= 0)
                                    break;
                                
                                sub_4035b0(&var_244, eax_58, arg1, &i);
                                j_2 = j_3;
                                j_3 -= 1;
                            } while (j_2);
                        }
                        
                        if (esi_1 & 4)
                            sub_403570(0x20, var_230_2, arg1, &i);
                    }
                    break;
                }
            }
            
            char* eax_13 = arg2;
            arg2 = &arg2[1];
            ebx = *eax_13;
            
            if (!ebx)
                break;
        }
    }
    
    return i;
}

int32_t* sub_403520(int32_t* arg1, int32_t* arg2, int32_t* arg3)
{
    int32_t eax = arg2[1];
    arg2[1] = eax - 1;
    uint32_t eax_3;
    
    if (eax - 1 < 0)
    {
        int32_t* var_4_1 = arg2;
        eax_3 = sub_402a20(arg1);
    }
    else
    {
        **arg2 = arg1;
        char* ecx_1 = *arg2;
        eax_3 = *ecx_1;
        *arg2 = &ecx_1[1];
    }
    
    if (eax_3 != 0xffffffff)
    {
        *arg3 += 1;
        return arg3;
    }
    
    *arg3 = 0xffffffff;
    return arg3;
}

int32_t* sub_403570(int32_t* arg1, int32_t* arg2, int32_t* arg3, int32_t* arg4)
{
    int32_t* result_1 = arg2;
    int32_t* result;
    
    do
    {
        result = result_1;
        result_1 -= 1;
        
        if (result <= 0)
            break;
        
        result = sub_403520(arg1, arg3, arg4);
    } while (*arg4 != 0xffffffff);
    
    return result;
}

int32_t* sub_4035b0(char* arg1, int32_t* arg2, int32_t* arg3, int32_t* arg4)
{
    char* esi = arg1;
    int32_t* result_1 = arg2;
    int32_t* result;
    
    do
    {
        result = result_1;
        result_1 -= 1;
        
        if (result <= 0)
            break;
        
        char* eax_1 = esi;
        esi = &esi[1];
        result = sub_403520(*eax_1, arg3, arg4);
    } while (*arg4 != 0xffffffff);
    
    return result;
}

int32_t sub_4035f0(int32_t* arg1)
{
    void* ecx_1 = *arg1 + 4;
    *arg1 = ecx_1;
    return *(ecx_1 - 4);
}

int32_t sub_403600(int32_t* arg1)
{
    void* ecx_1 = *arg1 + 8;
    *arg1 = ecx_1;
    *(ecx_1 - 4);
    return *(ecx_1 - 8);
}

int32_t* sub_403620(int32_t* arg1)
{
    void* ecx_1 = *arg1 + 4;
    *arg1 = ecx_1;
    int32_t* result;
    result = *(ecx_1 - 4);
    return result;
}

int32_t sub_403630(int32_t arg1, int32_t arg2)
{
    if (arg1 + 1 <= 0x100)
    {
        int32_t eax_1;
        eax_1 = (**&data_40a5f0)[arg1];
        return eax_1 & arg2;
    }
    
    char edx = *arg1[1];
    int32_t ebx;
    ebx = edx;
    int32_t eax_4;
    
    if (!(*(&(**&data_40a5f0)[ebx] + 1) & 0x80))
    {
        eax_4 = 1;
        char var_4_1 = arg1;
        char var_3_1 = 0;
    }
    else
    {
        eax_4 = 2;
        char var_4 = edx;
        char var_2_1 = 0;
        char var_3 = arg1;
    }
    
    int32_t var_6;
    
    if (sub_404d80(1, &*var_6[2], eax_4, &var_6, 0, 0))
        return var_6 & arg2;
    
    return 0;
}

int32_t __fastcall sub_4036d0(int32_t, int32_t arg2) __pure
{
    return arg2 - 1;
}

void* sub_4036e0(char* arg1, char arg2)
{
    int32_t eax;
    eax = arg2;
    char* edx = arg1;
    
    while (edx & 3)
    {
        int32_t ecx;
        ecx = *edx;
        edx = &edx[1];
        
        if (ecx == eax)
            /* tailcall */
            return sub_4036d0(ecx, edx);
        
        if (!ecx)
            return 0;
    }
    
    int32_t ebx_2 = eax | eax << 8;
    int32_t ebx_4 = ebx_2 << 0x10 | ebx_2;
    
    while (true)
    {
        int32_t ecx_1 = *edx;
        int32_t ecx_2 = ecx_1 ^ ebx_4;
        edx = &edx[4];
        
        if ((ecx_2 ^ 0xffffffff ^ (0x7efefeff + ecx_2)) & 0x81010100)
        {
            int32_t eax_10 = *(edx - 4);
            
            if (eax_10 == ebx_4)
                return &edx[0xfffffffc];
            
            if (!eax_10)
                break;
            
            if (*eax_10[1] == ebx_4)
                return &edx[0xfffffffd];
            
            if (!*eax_10[1])
                break;
            
            uint16_t eax_11 = eax_10 >> 0x10;
            
            if (eax_11 == ebx_4)
                return &edx[0xfffffffe];
            
            if (!eax_11)
                break;
            
            if (*eax_11[1] == ebx_4)
                return &edx[0xffffffff];
            
            if (!*eax_11[1])
                break;
        }
        else
        {
            int32_t eax_7 = (ecx_1 ^ 0xffffffff ^ (0x7efefeff + ecx_1)) & 0x81010100;
            
            if (eax_7)
            {
                if (eax_7 & 0x1010100)
                    break;
                
                if (!((0x7efefeff + ecx_1) & 0x80000000))
                    break;
            }
        }
    }
    
    return 0;
}

int32_t sub_40379c(int32_t arg1)
{
    int32_t ebp;
    int32_t var_4 = ebp;
    int32_t result = RtlUnwind(arg1, 0x4037b4, nullptr, nullptr);
    var_4;
    return result;
}

int32_t sub_4037bc(int32_t arg1, int32_t arg2, int32_t* arg3)
{
    if (!(*(arg1 + 4) & 6))
        return 1;
    
    *arg3 = arg2;
    return 3;
}

void* sub_4037de(void* arg1, int32_t arg2)
{
    void* var_10 = arg1;
    int32_t var_14 = 0xfffffffe;
    int32_t (* var_18)(int32_t arg1, int32_t arg2, int32_t* arg3) = sub_4037bc;
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
            sub_403872(*(ebx_1 + (esi_2 << 2) + 8), ebp);
            (*(ebx_1 + (esi_2 << 2) + 8))();
        }
    }
}

int32_t __abnormal_termination()
{
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    
    if (ExceptionList->Handler == sub_4037bc
            && *(ExceptionList + 8) == *(*(ExceptionList + 0xc) + 0xc))
        return 1;
    
    return 0;
}

void __convention("regparm") sub_403872(int32_t arg1, void* arg2 @ ebp)
{
    data_40a818 = *(arg2 + 8);
    data_40a814 = arg1;
    data_40a81c = arg2;
}

int32_t sub_403890(int32_t arg1, EXCEPTION_POINTERS* arg2)
{
    void* eax = sub_4039f0(arg1);
    
    if (eax)
    {
        int32_t edx_1 = *(eax + 8);
        
        if (edx_1)
        {
            if (edx_1 == 5)
            {
                *(eax + 8) = 0;
                return 1;
            }
            
            if (edx_1 == 1)
                return 0xffffffff;
            
            int32_t esi = data_40a8a8;
            data_40a8a8 = arg2;
            
            if (*(eax + 4) != 8)
            {
                *(eax + 8) = 0;
                edx_1(*(eax + 4));
            }
            else
            {
                if (data_40a89c + data_40a898 > data_40a898)
                {
                    void* edi_1 = data_40a898 * 0xc + &data_40a828;
                    int32_t i_1 = data_40a89c;
                    int32_t i;
                    
                    do
                    {
                        *edi_1 = 0;
                        edi_1 += 0xc;
                        i = i_1;
                        i_1 -= 1;
                    } while (i != 1);
                }
                
                int32_t edi_2 = data_40a8a4;
                
                switch (*eax)
                {
                    case 0xc000008d:
                    {
                        data_40a8a4 = 0x82;
                        break;
                    }
                    case 0xc000008e:
                    {
                        data_40a8a4 = 0x83;
                        break;
                    }
                    case 0xc000008f:
                    {
                        data_40a8a4 = 0x86;
                        break;
                    }
                    case 0xc0000090:
                    {
                        data_40a8a4 = 0x81;
                        break;
                    }
                    case 0xc0000091:
                    {
                        data_40a8a4 = 0x84;
                        break;
                    }
                    case 0xc0000092:
                    {
                        data_40a8a4 = 0x8a;
                        break;
                    }
                    case 0xc0000093:
                    {
                        data_40a8a4 = 0x85;
                        break;
                    }
                }
                
                edx_1(8, data_40a8a4);
                data_40a8a4 = edi_2;
            }
            
            data_40a8a8 = esi;
            return 0xffffffff;
        }
    }
    
    return UnhandledExceptionFilter(arg2);
}

void* sub_4039f0(int32_t arg1)
{
    void* edx = &data_40a820;
    
    while (*edx != arg1)
    {
        edx += 0xc;
        
        if (data_40a8a0 * 0xc + &data_40a820 <= edx)
            break;
    }
    
    int32_t eax_5 = *edx - arg1;
    return (eax_5 - eax_5) & edx;
}

int32_t sub_403a20(char arg1)
{
    return sub_403a40(arg1, 0, 4);
}

int32_t sub_403a40(char arg1, int32_t arg2, int32_t arg3)
{
    void* edx;
    edx = arg1;
    int32_t ecx;
    ecx = *(edx + 0x40a8b9);
    
    if (!(arg3 & ecx))
    {
        int32_t ecx_1 = 0;
        
        if (arg2)
        {
            int32_t ecx_2;
            ecx_2 = data_40a5fa[edx];
            ecx_1 = ecx_2 & arg2;
        }
        
        if (!ecx_1)
            return 0;
    }
    
    return 1;
}

int32_t sub_403a80()
{
    char* edx = data_40a5d0;
    int32_t esi = 0;
    
    while (*edx)
    {
        if (*edx != 0x3d)
            esi += 1;
        
        char* edi_1 = edx;
        int32_t i = 0xffffffff;
        
        while (i)
        {
            bool cond:0_1 = 0 != *edi_1;
            edi_1 = &edi_1[1];
            i -= 1;
            
            if (!cond:0_1)
                break;
        }
        
        edx = &edx[~i];
    }
    
    void* eax_1 = sub_404f00((esi << 2) + 4);
    data_40a5a0 = eax_1;
    void* ebx = eax_1;
    
    if (!ebx)
        sub_4029f0(9);
    
    char* ebp = data_40a5d0;
    
    while (*ebp)
    {
        char* edi_2 = ebp;
        int32_t i_1 = 0xffffffff;
        
        while (i_1)
        {
            bool cond:1_1 = 0 != *edi_2;
            edi_2 = &edi_2[1];
            i_1 -= 1;
            
            if (!cond:1_1)
                break;
        }
        
        int32_t ecx_2 = ~i_1;
        
        if (*ebp != 0x3d)
        {
            void* eax_3 = sub_404f00(ecx_2);
            *ebx = eax_3;
            
            if (!eax_3)
                sub_4029f0(9);
            
            char* edi_3 = ebp;
            int32_t i_2 = 0xffffffff;
            
            while (i_2)
            {
                bool cond:2_1 = 0 != *edi_3;
                edi_3 = &edi_3[1];
                i_2 -= 1;
                
                if (!cond:2_1)
                    break;
            }
            
            int32_t ecx_3 = ~i_2;
            int32_t edi_5 = *ebx;
            ebx += 4;
            int32_t esi_2;
            int32_t edi_6;
            edi_6 = __builtin_memcpy(edi_5, edi_3 - ecx_3, ecx_3 >> 2 << 2);
            __builtin_memcpy(edi_6, esi_2, ecx_3 & 3);
        }
        
        ebp = &ebp[ecx_2];
    }
    
    int32_t result = sub_404eb0(data_40a5d0);
    data_40a5d0 = 0;
    *ebx = 0;
    return result;
}

int32_t sub_403b70()
{
    void* esi = &data_40bd38;
    GetModuleFileNameA(nullptr, &data_40bd38, 0x104);
    char* eax = data_40cf64;
    data_40a5b0 = &data_40bd38;
    
    if (*eax)
        esi = data_40cf64;
    
    int32_t var_8;
    int32_t var_4;
    sub_403c10(esi, nullptr, nullptr, &var_8, &var_4);
    void* eax_4 = sub_404f00((var_8 << 2) + var_4);
    
    if (!eax_4)
        sub_4029f0(8);
    
    sub_403c10(esi, eax_4, eax_4 + (var_8 << 2), &var_8, &var_4);
    int32_t result = var_8 - 1;
    data_40a598 = eax_4;
    data_40a594 = result;
    return result;
}

char* sub_403c10(char* arg1, int32_t* arg2, char* arg3, int32_t* arg4, int32_t* arg5)
{
    int32_t ebx;
    int32_t var_4 = ebx;
    char* esi = arg1;
    char* result = arg3;
    *arg5 = 0;
    *arg4 = 1;
    int32_t* i;
    
    if (arg2)
    {
        i = arg2;
        arg2 = &arg2[1];
        *i = result;
    }
    
    if (*esi == 0x22)
    {
        esi = &esi[1];
        
        while (*esi != 0x22)
        {
            ebx = *esi;
            
            if (!ebx)
                break;
            
            i = ebx;
            
            if (*(i + 0x40a8b9) & 4)
            {
                *arg5 += 1;
                
                if (result)
                {
                    i = *esi;
                    esi = &esi[1];
                    *result = i;
                    result = &result[1];
                }
            }
            
            *arg5 += 1;
            
            if (result)
            {
                i = *esi;
                *result = i;
                result = &result[1];
            }
            
            esi = &esi[1];
        }
        
        *arg5 += 1;
        
        if (result)
        {
            *result = 0;
            result = &result[1];
        }
        
        if (*esi == 0x22)
            esi = &esi[1];
    }
    else
    {
        while (true)
        {
            *arg5 += 1;
            
            if (result)
            {
                i = *esi;
                *result = i;
                result = &result[1];
            }
            
            i = *esi;
            esi = &esi[1];
            void* ebx_1;
            ebx_1 = i;
            
            if (*(ebx_1 + 0x40a8b9) & 4)
            {
                *arg5 += 1;
                
                if (result)
                {
                    ebx_1 = *esi;
                    *result = ebx_1;
                    result = &result[1];
                }
                
                esi = &esi[1];
            }
            
            if (i == 0x20)
            {
            label_403c7c:
                
                if (i)
                {
                    if (result)
                        result[0xffffffff] = 0;
                    
                    break;
                }
            }
            else if (i)
            {
                if (i == 9)
                    goto label_403c7c;
                
                continue;
            }
            
            esi -= 1;
            break;
        }
    }
    
    int32_t edi = 0;
    
    while (*esi)
    {
        while (true)
        {
            i = *esi;
            
            if (i != 0x20 && i != 9)
                break;
            
            esi = &esi[1];
        }
        
        if (!*esi)
            break;
        
        if (arg2)
        {
            int32_t* edx = arg2;
            arg2 = &arg2[1];
            *edx = result;
        }
        
        *arg4 += 1;
        
        while (true)
        {
            int32_t ebx_2 = 1;
            uint32_t ebp_1 = 0;
            
            while (*esi == 0x5c)
            {
                esi = &esi[1];
                ebp_1 += 1;
            }
            
            if (*esi == 0x22)
            {
                if (!(ebp_1 & 1))
                {
                    if (!edi || esi[1] != 0x22)
                        ebx_2 = 0;
                    else
                        esi = &esi[1];
                    
                    edi = -((edi - edi));
                }
                
                ebp_1 u>>= 1;
            }
            
            int32_t* i_1 = ebp_1 - 1;
            
            if (ebp_1)
            {
                do
                {
                    if (result)
                    {
                        *result = 0x5c;
                        result = &result[1];
                    }
                    
                    i = i_1;
                    *arg5 += 1;
                    i_1 -= 1;
                } while (i);
            }
            
            i = *esi;
            
            if (!i)
                break;
            
            if (!edi)
            {
                if (i == 0x20)
                    break;
                
                if (i == 9)
                    break;
            }
            
            if (ebx_2)
            {
                if (!result)
                {
                    void* ebx_4;
                    ebx_4 = i;
                    
                    if (*(ebx_4 + 0x40a8b9) & 4)
                    {
                        esi = &esi[1];
                        *arg5 += 1;
                    }
                    
                    *arg5 += 1;
                }
                else
                {
                    void* ebx_3;
                    ebx_3 = i;
                    
                    if (*(ebx_3 + 0x40a8b9) & 4)
                    {
                        *result = i;
                        esi = &esi[1];
                        result = &result[1];
                        *arg5 += 1;
                    }
                    
                    i = *esi;
                    result = &result[1];
                    esi = &esi[1];
                    result[0xffffffff] = i;
                    *arg5 += 1;
                    continue;
                }
            }
            
            esi = &esi[1];
        }
        
        if (result)
        {
            *result = 0;
            result = &result[1];
        }
        
        *arg5 += 1;
    }
    
    if (arg2)
        *arg2 = 0;
    
    *arg4 += 1;
    return result;
}

void* sub_403df0()
{
    PSTR penv = nullptr;
    PWSTR edi = nullptr;
    
    if (!data_40a8b0)
    {
        PWSTR eax_1 = GetEnvironmentStringsW();
        edi = eax_1;
        
        if (!eax_1)
        {
            penv = GetEnvironmentStrings();
            
            if (!penv)
                return 0;
            
            data_40a8b0 = 2;
        }
        else
            data_40a8b0 = 1;
    }
    
    if (data_40a8b0 != 1)
    {
        if (data_40a8b0 != 2)
            return 0;
        
        if (!penv)
        {
            penv = GetEnvironmentStrings();
            
            if (!penv)
                return 0;
        }
        
        PSTR penv_1 = penv;
        
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
        
        void* eax_12 = sub_404f00(penv_1 - penv + 1);
        
        if (!eax_12)
        {
            FreeEnvironmentStringsA(penv);
            return 0;
        }
        
        int32_t esi_5;
        int32_t edi_2;
        edi_2 = __builtin_memcpy(eax_12, penv, (penv_1 - penv + 1) >> 2 << 2);
        __builtin_memcpy(edi_2, esi_5, (penv_1 - penv + 1) & 3);
        FreeEnvironmentStringsA(penv);
        return eax_12;
    }
    
    if (!edi)
    {
        edi = GetEnvironmentStringsW();
        
        if (!edi)
            return 0;
    }
    
    PWSTR esi = edi;
    
    if (*edi)
    {
        while (true)
        {
            esi = &esi[1];
            
            if (!*esi)
            {
                esi = &esi[1];
                
                if (!*esi)
                    break;
            }
        }
    }
    
    int32_t cbMultiByte =
        WideCharToMultiByte(0, 0, edi, ((esi - edi) >> 1) + 1, nullptr, 0, nullptr, nullptr);
    
    if (cbMultiByte)
    {
        void* lpMultiByteStr = sub_404f00(cbMultiByte);
        
        if (lpMultiByteStr)
        {
            if (!WideCharToMultiByte(0, 0, edi, ((esi - edi) >> 1) + 1, lpMultiByteStr, 
                cbMultiByte, nullptr, nullptr))
            {
                sub_404eb0(lpMultiByteStr);
                lpMultiByteStr = nullptr;
            }
            
            FreeEnvironmentStringsW(edi);
            return lpMultiByteStr;
        }
    }
    
    FreeEnvironmentStringsW(edi);
    return 0;
}

int32_t sub_403f80(int32_t arg1)
{
    uint32_t CodePage = sub_404160(arg1);
    
    if (CodePage == data_40a9bc)
        return 0;
    
    if (!CodePage)
    {
        sub_404210();
        return 0;
    }
    
    int32_t var_18 = 0;
    
    for (void* i = &data_40a9e0; i < 0x40aad0; )
    {
        if (*i == CodePage)
        {
            void* j = nullptr;
            *__builtin_memset(0x40a8b8, 0, 0x100) = 0;
            
            do
            {
                void* esi_2 = ((j + var_18 * 6) << 3) + &data_40a9f0;
                
                while (*esi_2)
                {
                    void* ecx_3;
                    ecx_3 = *(esi_2 + 1);
                    
                    if (!ecx_3)
                        break;
                    
                    void* edx_3;
                    edx_3 = *esi_2;
                    int32_t ebx_1;
                    ebx_1 = ecx_3;
                    
                    if (ebx_1 >= edx_3)
                    {
                        ecx_3 = *(j + 0x40a9d8);
                        int32_t ebx_2;
                        
                        do
                        {
                            *(edx_3 + 0x40a8b9) |= ecx_3;
                            edx_3 += 1;
                            ebx_2 = *(esi_2 + 1);
                        } while (ebx_2 >= edx_3);
                    }
                    
                    esi_2 += 2;
                }
                
                j += 1;
            } while (j < 4);
            
            data_40a9bc = CodePage;
            data_40a9c0 = sub_4041b0(CodePage);
            int32_t eax_7 = var_18 << 4;
            int32_t ebx_3 = *(eax_7 * 3 + &data_40a9e8);
            int32_t ecx_5 = *(eax_7 * 3 + 0x40a9ec);
            data_40a9c8 = *(eax_7 * 3 + 0x40a9e4);
            data_40a9cc = ebx_3;
            data_40a9d0 = ecx_5;
            return 0;
        }
        
        i += 0x30;
        var_18 += 1;
    }
    
    CPINFO cPInfo;
    
    if (GetCPInfo(CodePage, &cPInfo) != 1)
    {
        if (!data_40a9d4)
            return 0xffffffff;
        
        sub_404210();
        return 0;
    }
    
    *__builtin_memset(0x40a8b8, 0, 0x100) = 0;
    int32_t eax_4;
    
    if (cPInfo.MaxCharSize <= 1)
    {
        eax_4 = 0;
        data_40a9bc = 0;
    }
    else
    {
        var_e;
        void* esi_1 = &var_e;
        
        if (cPInfo.LeadByte[0])
        {
            do
            {
                int32_t eax_3;
                eax_3 = *(esi_1 + 1);
                
                if (!eax_3)
                    break;
                
                void* ecx_1;
                ecx_1 = *esi_1;
                int32_t edx_1;
                edx_1 = eax_3;
                
                if (edx_1 >= ecx_1)
                {
                    do
                    {
                        *(ecx_1 + 0x40a8b9) |= 4;
                        ecx_1 += 1;
                        eax_3 = *(esi_1 + 1);
                    } while (eax_3 >= ecx_1);
                }
                
                esi_1 += 2;
            } while (*esi_1);
        }
        
        for (void* i_1 = 1; i_1 < 0xff; i_1 += 1)
            *(i_1 + 0x40a8b9) |= 8;
        
        data_40a9bc = CodePage;
        eax_4 = sub_4041b0(CodePage);
    }
    
    data_40a9c0 = eax_4;
    data_40a9c8 = 0;
    data_40a9cc = 0;
    data_40a9d0 = 0;
    return 0;
}

int32_t sub_404160(int32_t arg1)
{
    data_40a9d4 = 0;
    
    if (arg1 == 0xfffffffe)
    {
        data_40a9d4 = 1;
        /* tailcall */
        return GetOEMCP();
    }
    
    if (arg1 == 0xfffffffd)
    {
        data_40a9d4 = 1;
        /* tailcall */
        return GetACP();
    }
    
    if (arg1 != 0xfffffffc)
        return arg1;
    
    data_40a9d4 = 1;
    return data_40ae40;
}

int32_t sub_4041b0(int32_t arg1)
{
    if (arg1 - 0x3a4 <= 0x12)
    {
        int32_t ecx_1;
        ecx_1 = *(arg1 + sub_403df0+0x68);
        
        switch (ecx_1)
        {
            case 0:
            {
                return 0x411;
                break;
            }
            case 1:
            {
                return 0x804;
                break;
            }
            case 2:
            {
                return 0x412;
                break;
            }
            case 3:
            {
                return 0x404;
                break;
            }
        }
    }
    
    return 0;
}

int32_t sub_404210()
{
    *__builtin_memset(0x40a8b8, 0, 0x100) = 0;
    data_40a9c8 = 0;
    data_40a9bc = 0;
    data_40a9c0 = 0;
    data_40a9cc = 0;
    data_40a9d0 = 0;
    return 0;
}

int32_t sub_404240()
{
    return sub_403f80(0xfffffffd);
}

uint32_t sub_404250()
{
    void* i = sub_404f00(0x100);
    
    if (!i)
        sub_4029f0(0x1b);
    
    data_40ce60 = i;
    data_40cf60 = 0x20;
    
    if (i + 0x100 > i)
    {
        do
        {
            *(i + 4) = 0;
            i += 8;
            *(i - 8) = 0xffffffff;
            *(i - 3) = 0xa;
        } while (data_40ce60 + 0x100 > i);
    }
    
    STARTUPINFOA startupInfo;
    GetStartupInfoA(&startupInfo);
    
    if (startupInfo.cbReserved2 && startupInfo.lpReserved2)
    {
        BYTE* lpReserved2 = startupInfo.lpReserved2;
        void* const i_1 = *lpReserved2;
        void* edi_1 = &lpReserved2[4];
        void* ebx_1 = i_1 + edi_1;
        
        if (i_1 >= 0x800)
            i_1 = 0x800;
        
        if (data_40cf60 < i_1)
        {
            void* ebp_1 = &data_40ce64;
            
            do
            {
                lpReserved2 = sub_404f00(0x100);
                
                if (!lpReserved2)
                {
                    i_1 = data_40cf60;
                    break;
                }
                
                *ebp_1 = lpReserved2;
                data_40cf60 += 0x20;
                
                if (&lpReserved2[0x100] > lpReserved2)
                {
                    do
                    {
                        lpReserved2[4] = 0;
                        lpReserved2 = &lpReserved2[8];
                        *(lpReserved2 - 8) = 0xffffffff;
                        lpReserved2[0xfffffffd] = 0xa;
                    } while (*ebp_1 + 0x100 > lpReserved2);
                }
                
                ebp_1 += 4;
            } while (data_40cf60 < i_1);
        }
        
        int32_t ebp_2 = 0;
        
        if (i_1 > 0)
        {
            do
            {
                HANDLE hFile_1 = *ebx_1;
                
                if (hFile_1 != 0xffffffff)
                {
                    lpReserved2 = *edi_1;
                    
                    if (lpReserved2 & 1)
                    {
                        if (lpReserved2 & 8)
                        {
                        label_40438a:
                            BYTE** ecx_4 = *(((ebp_2 & 0xffffffe7) >> 3) + &data_40ce60)
                                + ((ebp_2 & 0x1f) << 3);
                            *ecx_4 = *ebx_1;
                            int32_t edx_3;
                            edx_3 = *edi_1;
                            ecx_4[1] = edx_3;
                        }
                        else if (GetFileType(hFile_1))
                            goto label_40438a;
                    }
                }
                
                ebp_2 += 1;
                edi_1 += 1;
                ebx_1 += 4;
            } while (ebp_2 < i_1);
        }
    }
    
    for (int32_t i_2 = 0; i_2 < 3; i_2 += 1)
    {
        int32_t* edi_3 = (i_2 << 3) + data_40ce60;
        
        if (*edi_3 != 0xffffffff)
            edi_3[1] |= 0x80;
        else
        {
            enum STD_HANDLE nStdHandle = STD_INPUT_HANDLE;
            edi_3[1] = 0x81;
            
            if (i_2)
                nStdHandle = 0xfffffff5 - 1;
            
            HANDLE hFile = GetStdHandle(nStdHandle);
            enum FILE_TYPE eax_8;
            
            if (hFile != 0xffffffff)
                eax_8 = GetFileType(hFile);
            
            if (hFile == 0xffffffff || !eax_8)
                edi_3[1] |= 0x40;
            else
            {
                int32_t eax_9 = eax_8 & 0xff;
                *edi_3 = hFile;
                
                if (eax_9 == 2)
                    edi_3[1] |= 0x40;
                else if (eax_9 == 3)
                    edi_3[1] |= 8;
            }
        }
    }
    
    return SetHandleCount(data_40cf60);
}

int32_t sub_404430()
{
    HANDLE eax = HeapCreate(HEAP_NO_SERIALIZE, 0x1000, 0);
    data_40ce54 = eax;
    
    if (!eax)
        return 0;
    
    if (sub_404fb0())
        return 1;
    
    HeapDestroy(data_40ce54);
    return 0;
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
        sub_4037de(ebx_2, 0xffffffff);
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
                    sub_40379c(ebx_2);
                    ebp_1 = ebx_2 + 0x10;
                    sub_4037de(ebx_2, esi_1);
                    int32_t ecx_2 = esi_1 * 3;
                    int32_t var_20_4 = 1;
                    sub_403872(*(edi_2 + (ecx_2 << 2) + 8), ebp_1);
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
    return sub_4037de(arg1[6], arg1[7]);
}

void* sub_404550()
{
    void* result = data_40a5dc;
    
    if (result != 1 && (result || data_40a5e0 != 1))
        return result;
    
    sub_404590(0xfc);
    int32_t eax = data_40ab70;
    
    if (eax)
        eax();
    
    return sub_404590(0xff);
}

void* sub_404590(int32_t arg1)
{
    int32_t ecx = 0;
    void* result = &data_40aae0;
    
    while (*result != arg1)
    {
        result += 8;
        ecx += 1;
        
        if (result >= &data_40ab70)
            break;
    }
    
    if (*((ecx << 3) + &data_40aae0) == arg1)
    {
        OVERLAPPED* var_1bc;
        
        if (data_40a5dc == 1 || (!data_40a5dc && data_40a5e0 == 1))
        {
            HANDLE hFile;
            
            if (data_40ce60)
                hFile = *(data_40ce60 + 0x10);
            
            if (!data_40ce60 || hFile == 0xffffffff)
            {
                var_1bc = 0xfffffff4;
                hFile = GetStdHandle(var_1bc);
            }
            
            uint8_t* lpBuffer = (&data_40aae4)[ecx * 2];
            var_1bc = nullptr;
            uint8_t* lpBuffer_1 = lpBuffer;
            int32_t i = 0xffffffff;
            
            while (i)
            {
                bool cond:1_1 = 0 != *lpBuffer_1;
                lpBuffer_1 = &lpBuffer_1[1];
                i -= 1;
                
                if (!cond:1_1)
                    break;
            }
            
            uint32_t numberOfBytesWritten;
            return WriteFile(hFile, lpBuffer, ~i - 1, &numberOfBytesWritten, var_1bc);
        }
        
        if (arg1 != 0xfc)
        {
            var_1bc = 0x104;
            uint8_t filename[0x104];
            
            if (!GetModuleFileNameA(nullptr, &filename, var_1bc))
            {
                int16_t* esi_1;
                int16_t* edi_1;
                edi_1 = __builtin_strncpy(&filename, "<program name unknown>", 0x14);
                *edi_1 = *esi_1;
                edi_1[1] = esi_1[1];
            }
            
            char* ebp = &filename;
            uint8_t (* edi_4)[0x104] = &filename;
            int32_t i_1 = 0xffffffff;
            
            while (i_1)
            {
                bool cond:2_1 = 0 != *edi_4;
                edi_4 = &(*edi_4)[1];
                i_1 -= 1;
                
                if (!cond:2_1)
                    break;
            }
            
            if (~i_1 > 0x3c)
            {
                uint8_t (* edi_5)[0x104] = &filename;
                int32_t i_2 = 0xffffffff;
                var_1bc = 3;
                
                while (i_2)
                {
                    bool cond:9_1 = 0 != *edi_5;
                    edi_5 = &(*edi_5)[1];
                    i_2 -= 1;
                    
                    if (!cond:9_1)
                        break;
                }
                
                ebp = &var_1bc + ~i_2 + 0x7c;
                sub_4057b0(ebp, "...", var_1bc);
            }
            
            void var_1a4;
            int16_t* esi_4;
            int16_t* edi_6;
            edi_6 = __builtin_strncpy(&var_1a4, "Runtime Error!\n\nProgram: ", 0x18);
            *edi_6 = *esi_4;
            char* edi_8 = ebp;
            int32_t i_3 = 0xffffffff;
            
            while (i_3)
            {
                bool cond:3_1 = 0 != *edi_8;
                edi_8 = &edi_8[1];
                i_3 -= 1;
                
                if (!cond:3_1)
                    break;
            }
            
            int32_t ecx_3 = ~i_3;
            int32_t i_4 = 0xffffffff;
            void* edi_10 = &var_1a4;
            
            while (i_4)
            {
                bool cond:4_1 = 0 != *edi_10;
                edi_10 += 1;
                i_4 -= 1;
                
                if (!cond:4_1)
                    break;
            }
            
            int32_t esi_7;
            int32_t edi_12;
            edi_12 = __builtin_memcpy(edi_10 - 1, edi_8 - ecx_3, ecx_3 >> 2 << 2);
            __builtin_memcpy(edi_12, esi_7, ecx_3 & 3);
            void* const edi_13 = &data_407788;
            int32_t i_5 = 0xffffffff;
            
            while (i_5)
            {
                bool cond:5_1 = 0 != *edi_13;
                edi_13 += 1;
                i_5 -= 1;
                
                if (!cond:5_1)
                    break;
            }
            
            int32_t ecx_8 = ~i_5;
            int32_t i_6 = 0xffffffff;
            void* edi_15 = &var_1a4;
            
            while (i_6)
            {
                bool cond:6_1 = 0 != *edi_15;
                edi_15 += 1;
                i_6 -= 1;
                
                if (!cond:6_1)
                    break;
            }
            
            int32_t esi_9;
            int32_t edi_17;
            edi_17 = __builtin_memcpy(edi_15 - 1, edi_13 - ecx_8, ecx_8 >> 2 << 2);
            __builtin_memcpy(edi_17, esi_9, ecx_8 & 3);
            int32_t edi_18 = (&data_40aae4)[ecx * 2];
            int32_t i_7 = 0xffffffff;
            
            while (i_7)
            {
                bool cond:7_1 = 0 != *edi_18;
                edi_18 += 1;
                i_7 -= 1;
                
                if (!cond:7_1)
                    break;
            }
            
            int32_t ecx_13 = ~i_7;
            void* edi_20 = &var_1a4;
            int32_t i_8 = 0xffffffff;
            
            while (i_8)
            {
                bool cond:8_1 = 0 != *edi_20;
                edi_20 += 1;
                i_8 -= 1;
                
                if (!cond:8_1)
                    break;
            }
            
            int32_t esi_11;
            int32_t edi_22;
            edi_22 = __builtin_memcpy(edi_20 - 1, edi_18 - ecx_13, ecx_13 >> 2 << 2);
            var_1bc = 0x12010;
            __builtin_memcpy(edi_22, esi_11, ecx_13 & 3);
            return sub_405710(&var_1a4, "Microsoft Visual C++ Runtime Library", var_1bc);
        }
    }
    
    return result;
}

int32_t sub_404790(int32_t arg1, char* arg2, uint32_t arg3)
{
    if (data_40cf60 > arg1)
    {
        int32_t eax_7 = (arg1 & 0x1f) << 3;
        int32_t var_40c_1 = eax_7;
        eax_7 = *(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + eax_7 + 4);
        
        if (eax_7 & 1)
        {
            int32_t esi = 0;
            uint32_t numberOfBytesWritten_1 = 0;
            
            if (!arg3)
                return 0;
            
            if (eax_7 & 0x20)
                sub_4049c0(arg1, 0, FILE_END);
            
            int32_t* ecx_3 = var_40c_1 + *(((arg1 & 0xffffffe7) >> 3) + &data_40ce60);
            enum WIN32_ERROR var_418_1;
            uint32_t numberOfBytesWritten;
            
            if (!(ecx_3[1] & 0x80))
            {
                if (!WriteFile(*ecx_3, arg2, arg3, &numberOfBytesWritten, nullptr))
                {
                label_4048db:
                    var_418_1 = GetLastError();
                }
                else
                {
                    var_418_1 = NO_ERROR;
                    numberOfBytesWritten_1 = numberOfBytesWritten;
                }
            }
            else
            {
                var_418_1 = NO_ERROR;
                char* ebx_2 = arg2;
                
                while (ebx_2 - arg2 < arg3)
                {
                    uint8_t buffer[0x404];
                    uint8_t (* edi_1)[0x404] = &buffer;
                    
                    while (ebx_2 - arg2 < arg3)
                    {
                        void* eax_13;
                        eax_13 = *ebx_2;
                        ebx_2 = &ebx_2[1];
                        
                        if (eax_13 == 0xa)
                        {
                            *edi_1 = 0xd;
                            esi += 1;
                            edi_1 = &(*edi_1)[1];
                        }
                        
                        *edi_1 = eax_13;
                        edi_1 = &(*edi_1)[1];
                        
                        if (edi_1 - &buffer >= 0x400)
                            break;
                    }
                    
                    uint32_t nNumberOfBytesToWrite = edi_1 - &buffer;
                    
                    if (!WriteFile(*(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + var_40c_1), 
                            &buffer, nNumberOfBytesToWrite, &numberOfBytesWritten, nullptr))
                        goto label_4048db;
                    
                    uint32_t numberOfBytesWritten_2 = numberOfBytesWritten;
                    numberOfBytesWritten_1 += numberOfBytesWritten_2;
                    
                    if (numberOfBytesWritten_2 < nNumberOfBytesToWrite)
                        break;
                }
            }
            
            if (numberOfBytesWritten_1)
                return numberOfBytesWritten_1 - esi;
            
            if (!var_418_1)
            {
                if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + var_40c_1 + 4) & 0x40
                        && *arg2 == 0x1a)
                    return 0;
                
                data_40a578 = 0x1c;
                data_40a57c = 0;
                return 0xffffffff;
            }
            
            if (var_418_1 != ERROR_ACCESS_DENIED)
            {
                sub_4058b0(var_418_1);
                return 0xffffffff;
            }
            
            data_40a578 = 9;
            data_40a57c = var_418_1;
            return 0xffffffff;
        }
    }
    
    data_40a578 = 9;
    data_40a57c = 0;
    return 0xffffffff;
}

uint32_t sub_4049c0(int32_t arg1, int32_t arg2, enum SET_FILE_POINTER_MOVE_METHOD arg3)
{
    if (data_40cf60 > arg1)
    {
        int32_t esi_1 = (arg1 & 0x1f) << 3;
        
        if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + esi_1 + 4) & 1)
        {
            HANDLE hFile = sub_4059b0(arg1);
            
            if (hFile == 0xffffffff)
            {
                data_40a578 = 9;
                return 0xffffffff;
            }
            
            uint32_t result = SetFilePointer(hFile, arg2, nullptr, arg3);
            enum WIN32_ERROR eax_7 = NO_ERROR;
            
            if (result == 0xffffffff)
                eax_7 = GetLastError();
            
            if (eax_7)
            {
                sub_4058b0(eax_7);
                return 0xffffffff;
            }
            
            int32_t eax_9 = *(((arg1 & 0xffffffe7) >> 3) + &data_40ce60);
            *(eax_9 + esi_1 + 4) &= 0xfd;
            return result;
        }
    }
    
    data_40a578 = 9;
    data_40a57c = 0;
    return 0xffffffff;
}

int32_t sub_404a80(int32_t* arg1)
{
    data_40adf8 += 1;
    void* eax = sub_404f00(0x1000);
    arg1[2] = eax;
    
    if (!eax)
    {
        arg1[3] |= 4;
        arg1[2] = &arg1[5];
        arg1[6] = 2;
    }
    else
    {
        arg1[3] |= 8;
        arg1[6] = 0x1000;
    }
    
    int32_t result = arg1[2];
    *arg1 = result;
    arg1[1] = 0;
    return result;
}

int32_t sub_404ad0(int32_t arg1)
{
    if (arg1 >= data_40cf60)
        return 0;
    
    int32_t eax_4;
    eax_4 = *(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + ((arg1 & 0x1f) << 3) + 4);
    return eax_4 & 0x40;
}

int32_t sub_404b00()
{
    if (!data_40ce50)
        data_40ce50 = 0x200;
    else if (data_40ce50 < 0x14)
        data_40ce50 = 0x14;
    
    void* eax_1 = sub_405a00(data_40ce50, 4);
    data_40be40 = eax_1;
    
    if (!eax_1)
    {
        data_40ce50 = 0x14;
        void* eax_2 = sub_405a00(0x14, 4);
        data_40be40 = eax_2;
        
        if (!eax_2)
            sub_4029f0(0x1a);
    }
    
    void** ecx = &data_40ab78;
    
    for (int32_t i = 0; i < 0x50; )
    {
        i += 4;
        *(data_40be40 + i - 4) = ecx;
        ecx = &ecx[8];
    }
    
    int32_t esi = 0;
    int32_t result;
    
    for (void* i_1 = &data_40ab88; i_1 < 0x40abe8; )
    {
        result = *(*(((esi & 0xffffffe7) >> 3) + &data_40ce60) + ((esi & 0x1f) << 3));
        
        if (result == 0xffffffff || !result)
            *i_1 = 0xffffffff;
        
        i_1 += 0x20;
        esi += 1;
    }
    
    return result;
}

int32_t sub_404bd0()
{
    int32_t result = sub_405be0();
    
    if (!data_40a5b8)
        return result;
    
    /* tailcall */
    return sub_405aa0();
}

int32_t sub_404bf0(char* arg1, wchar16 arg2)
{
    if (!arg1)
        return 0;
    
    if (!data_40ae30)
    {
        if (arg2 <= 0xff)
        {
            *arg1 = arg2;
            return 1;
        }
        
        data_40a578 = 0x2a;
        return 0xffffffff;
    }
    
    int32_t cbMultiByte = data_40a7fc;
    BOOL usedDefaultChar = 0;
    int32_t result = WideCharToMultiByte(data_40ae40, 0x220, &arg2, 1, arg1, cbMultiByte, nullptr, 
        &usedDefaultChar);
    
    if (result && !usedDefaultChar)
        return result;
    
    data_40a578 = 0x2a;
    return 0xffffffff;
}

uint32_t __stdcall __aulldiv(int32_t arg1, uint32_t arg2, int32_t arg3, uint32_t arg4) __pure
{
    if (!arg4)
        return COMBINE(COMBINE(0, arg2) % arg3, arg1) / arg3;
    
    uint32_t i = arg4;
    int32_t ebx_1 = arg3;
    uint32_t edx_3 = arg2;
    int32_t eax_6 = arg1;
    
    do
    {
        ebx_1 = RRCD(ebx_1, 1, i & 1);
        uint32_t temp4_1 = edx_3;
        edx_3 u>>= 1;
        eax_6 = RRCD(eax_6, 1, temp4_1 & 1);
        i u>>= 1;
    } while (i);
    
    uint32_t result = COMBINE(edx_3, eax_6) / ebx_1;
    int32_t eax_8 = result * arg4;
    int32_t eax_10;
    int32_t edx_4;
    edx_4 = HIGHD(arg3 * result);
    eax_10 = LOWD(arg3 * result);
    int32_t edx_5 = edx_4 + eax_8;
    
    if (edx_4 + eax_8 >= edx_4 && edx_5 <= arg2 && (edx_5 < arg2 || eax_10 <= arg1))
        return result;
    
    return result - 1;
}

uint32_t __stdcall __aullrem(int32_t arg1, uint32_t arg2, int32_t arg3, uint32_t arg4) __pure
{
    uint32_t result;
    
    if (arg4)
    {
        uint32_t i = arg4;
        int32_t ebx_1 = arg3;
        uint32_t edx_4 = arg2;
        int32_t eax_4 = arg1;
        
        do
        {
            ebx_1 = RRCD(ebx_1, 1, i & 1);
            uint32_t temp4_1 = edx_4;
            edx_4 u>>= 1;
            eax_4 = RRCD(eax_4, 1, temp4_1 & 1);
            i u>>= 1;
        } while (i);
        
        uint32_t temp0_1 = COMBINE(edx_4, eax_4) / ebx_1;
        int32_t eax_6 = temp0_1 * arg4;
        int32_t eax_8;
        int32_t edx_5;
        edx_5 = HIGHD(temp0_1 * arg3);
        eax_8 = LOWD(temp0_1 * arg3);
        int32_t edx_6 = edx_5 + eax_6;
        
        if (edx_5 + eax_6 < edx_5 || edx_6 > arg2)
            eax_8 -= arg3;
        else if (edx_6 >= arg2 && eax_8 > arg1)
            eax_8 -= arg3;
        
        result = -((eax_8 - arg1));
    }
    else
        result = COMBINE(COMBINE(0, arg2) % arg3, arg1) % arg3;
    
    return result;
}

BOOL sub_404d80(uint32_t arg1, uint8_t* arg2, int32_t arg3, uint16_t* arg4, uint32_t arg5, uint32_t arg6)
{
    BOOL eax = data_40ae24;
    uint16_t charType;
    
    if (!eax)
    {
        if (!GetStringTypeA(0, 1, &data_40a4fc, 1, &charType))
        {
            if (!GetStringTypeW(1, &data_4077c4, 1, &charType))
                return 0;
            
            eax = 1;
        }
        else
            eax = 2;
    }
    data_40ae24 = eax;
    
    if (eax == 2)
    {
        uint32_t Locale = arg6;
        
        if (!Locale)
            Locale = data_40ae30;
        
        return GetStringTypeA(Locale, arg1, arg2, arg3, arg4);
    }
    
    data_40ae24 = eax;
    
    if (eax != 1)
        return eax;
    
    BOOL edi_1 = 0;
    void* esi_1 = nullptr;
    uint32_t CodePage = arg5;
    
    if (!CodePage)
        CodePage = data_40ae40;
    
    int32_t cchWideChar = MultiByteToWideChar(CodePage, MB_ERR_INVALID_CHARS | MB_PRECOMPOSED, 
        arg2, arg3, nullptr, 0);
    
    if (cchWideChar)
    {
        esi_1 = sub_405a00(2, cchWideChar);
        
        if (esi_1)
        {
            int32_t cchSrc =
                MultiByteToWideChar(CodePage, MB_PRECOMPOSED, arg2, arg3, esi_1, cchWideChar);
            
            if (cchSrc)
                edi_1 = GetStringTypeW(arg1, esi_1, cchSrc, arg4);
        }
    }
    
    sub_404eb0(esi_1);
    return edi_1;
}

void sub_404eb0(int32_t arg1)
{
    if (!arg1)
        return;
    
    int32_t var_8;
    void** var_4;
    char* eax_1 = sub_405260(arg1, &var_4, &var_8);
    
    if (eax_1)
    {
        sub_4052c0(var_4, var_8, eax_1);
        return;
    }
    
    HeapFree(data_40ce54, HEAP_NONE, arg1);
}

void* sub_404f00(int32_t arg1)
{
    return sub_404f20(arg1, data_40b7f0);
}

void* sub_404f20(int32_t arg1, int32_t arg2)
{
    int32_t esi = arg1;
    
    if (esi > 0xffffffe0)
        return 0;
    
    if (!esi)
        esi = 1;
    
    int32_t i;
    
    do
    {
        void* result = nullptr;
        
        if (esi <= 0xffffffe0)
            result = sub_404f70(esi);
        
        if (result || !arg2)
            return result;
        
        i = sub_405de0(esi);
    } while (i);
    return nullptr;
}

void* sub_404f70(int32_t arg1)
{
    uint32_t dwBytes = (arg1 + 0xf) & 0xfffffff0;
    
    if (dwBytes <= data_40b664)
    {
        void* result = sub_405310(dwBytes >> 4);
        
        if (result)
            return result;
    }
    
    return HeapAlloc(data_40ce54, HEAP_NONE, dwBytes);
}

void** sub_404fb0()
{
    void** lpMem;
    
    if (data_40b658)
    {
        lpMem = HeapAlloc(data_40ce54, HEAP_NONE, 0x814);
        
        if (!lpMem)
            return 0;
    }
    else
        lpMem = &data_40ae48;
    
    int32_t* lpAddress = VirtualAlloc(nullptr, &__dos_header, MEM_RESERVE, PAGE_READWRITE);
    
    if (lpAddress)
    {
        if (VirtualAlloc(lpAddress, 0x10000, MEM_COMMIT, PAGE_READWRITE))
        {
            if (lpMem != &data_40ae48)
            {
                *lpMem = &data_40ae48;
                lpMem[1] = data_40ae4c;
                data_40ae4c = lpMem;
                *lpMem[1] = lpMem;
            }
            else
            {
                if (!data_40ae48)
                    data_40ae48 = &data_40ae48;
                
                if (!data_40ae4c)
                    data_40ae4c = &data_40ae48;
            }
            
            int32_t i = 0;
            lpMem[0x204] = lpAddress;
            lpMem[2] = 0;
            lpMem[3] = 0x10;
            
            do
            {
                void* edi_1 = lpMem + i;
                
                if (i >= 0x10)
                    *(edi_1 + 0x10) = 0xff;
                else
                    *(edi_1 + 0x10) = 0xf0;
                
                i += 1;
                *(edi_1 + 0x410) = 0xf1;
            } while (i < 0x400);
            
            __builtin_memset(lpAddress, 0, 0x10000);
            
            for (; lpMem[0x204] + 0x10000 > lpAddress; lpAddress = &lpAddress[0x400])
            {
                *lpAddress = &lpAddress[2];
                lpAddress[1] = 0xf0;
                lpAddress[0x3e] = 0xff;
            }
            
            return lpMem;
        }
        
        VirtualFree(lpAddress, 0, MEM_RELEASE);
    }
    
    if (lpMem != &data_40ae48)
        HeapFree(data_40ce54, HEAP_NONE, lpMem);
    
    return 0;
}

BOOL sub_405120(int32_t* arg1)
{
    BOOL result = VirtualFree(arg1[0x204], 0, MEM_RELEASE);
    
    if (data_40b65c == arg1)
    {
        result = arg1[1];
        data_40b65c = result;
    }
    
    if (arg1 == &data_40ae48)
    {
        data_40b658 = 0;
        return result;
    }
    
    *arg1[1] = *arg1;
    *(*arg1 + 4) = arg1[1];
    return HeapFree(data_40ce54, HEAP_NONE, arg1);
}

void sub_405180(int32_t arg1)
{
    void* esi = data_40ae4c;
    
    do
    {
        if (*(esi + 0x810))
        {
            int32_t ebp_1 = 0x3ff;
            void* ebx_1 = esi + 0x40f;
            int32_t var_4_1 = 0;
            
            for (int32_t j = 0x3ff000; j >= 0; )
            {
                if (*ebx_1 == 0xf0 && VirtualFree(*(esi + 0x810) + j, 0x1000, MEM_DECOMMIT))
                {
                    *ebx_1 = 0xff;
                    data_40b660 -= 1;
                    int32_t eax_4 = *(esi + 0xc);
                    
                    if (eax_4 == 0xffffffff || ebp_1 < eax_4)
                        *(esi + 0xc) = ebp_1;
                    
                    var_4_1 += 1;
                    int32_t temp0_1 = arg1;
                    arg1 -= 1;
                    
                    if (temp0_1 == 1)
                        break;
                }
                
                j -= 0x1000;
                ebp_1 -= 1;
                ebx_1 -= 1;
            }
            
            void* eax = esi;
            esi = *(esi + 4);
            
            if (var_4_1 && *(eax + 0x10) == 0xff)
            {
                int32_t edx_1 = 1;
                void* ecx_1 = eax + 0x11;
                
                while (*ecx_1 == 0xff)
                {
                    edx_1 += 1;
                    ecx_1 += 1;
                    
                    if (edx_1 >= 0x400)
                        break;
                }
                
                if (edx_1 == 0x400)
                    sub_405120(eax);
            }
        }
        
        if (esi == data_40ae4c)
            break;
    } while (arg1 > 0);
}

int32_t sub_405260(int32_t arg1, void*** arg2, int32_t* arg3)
{
    void** i = &data_40ae48;
    
    do
    {
        int32_t eax_1 = i[0x204];
        
        if (eax_1 && eax_1 < arg1 && &__dos_header.e_magic[eax_1] > arg1)
        {
            *arg2 = i;
            int32_t ecx_1 = arg1 & 0xfffff000;
            *arg3 = ecx_1;
            return ((arg1 - ecx_1 - 0x100) >> 4) + ecx_1 + 8;
        }
        
        i = *i;
    } while (i != &data_40ae48);
    
    return 0;
}

char* sub_4052c0(void* arg1, int32_t arg2, char* arg3)
{
    void* ecx_2 = ((arg2 - *(arg1 + 0x810)) >> 0xc) + arg1;
    *(ecx_2 + 0x10) += *arg3;
    *arg3 = 0;
    *(ecx_2 + 0x410) = 0xf1;
    
    if (*(ecx_2 + 0x10) == 0xf0)
    {
        data_40b660 += 1;
        
        if (data_40b660 == 0x20)
            return sub_405180(0x10);
    }
    
    return arg3;
}

void* sub_405310(void* arg1)
{
    void* i = data_40b65c;
    
    do
    {
        if (*(i + 0x810))
        {
            int32_t esi_1 = *(i + 8);
            int32_t ecx;
            
            if (esi_1 < 0x400)
            {
                int32_t j = esi_1 << 0xc;
                
                do
                {
                    ecx = *(i + esi_1 + 0x10);
                    int32_t eax_1;
                    eax_1 = ecx;
                    
                    if (eax_1 >= arg1 && ecx != 0xff)
                    {
                        ecx = *(i + esi_1 + 0x410);
                        
                        if (ecx > arg1)
                        {
                            void* eax_4;
                            eax_4 = sub_405590(*(i + 0x810) + j, eax_1, arg1);
                            
                            if (eax_4)
                            {
                                data_40b65c = i;
                                *(i + esi_1 + 0x10) -= arg1;
                                *(i + 8) = esi_1;
                                return eax_4;
                            }
                            
                            *(i + esi_1 + 0x410) = arg1;
                        }
                    }
                    
                    j += 0x1000;
                    esi_1 += 1;
                } while (j < &__dos_header);
            }
            
            int32_t ebp_2 = 0;
            int32_t j_1 = 0;
            
            if (*(i + 8) > 0)
            {
                do
                {
                    ecx = *(i + j_1 + 0x10);
                    int32_t eax_5;
                    eax_5 = ecx;
                    
                    if (eax_5 >= arg1 && ecx != 0xff)
                    {
                        ecx = *(i + j_1 + 0x410);
                        
                        if (ecx > arg1)
                        {
                            void* eax_8;
                            eax_8 = sub_405590(*(i + 0x810) + ebp_2, eax_5, arg1);
                            
                            if (eax_8)
                            {
                                data_40b65c = i;
                                *(i + j_1 + 0x10) -= arg1;
                                *(i + 8) = j_1;
                                return eax_8;
                            }
                            
                            *(i + j_1 + 0x410) = arg1;
                        }
                    }
                    
                    ebp_2 += 0x1000;
                    j_1 += 1;
                } while (*(i + 8) > j_1);
            }
        }
        
        i = *i;
    } while (data_40b65c != i);
    
    void** i_1 = &data_40ae48;
    
    do
    {
        if (i_1[0x204] && i_1[3] != 0xffffffff)
        {
            int32_t edx_1 = i_1[3];
            int32_t ecx_4 = edx_1 + 0x10;
            
            if (ecx_4 >= 0x400)
                ecx_4 = 0x400;
            
            int32_t edi_1 = edx_1 + 1;
            
            if (ecx_4 > edi_1)
            {
                while (*(i_1 + edi_1 + 0x10) == 0xff)
                {
                    edi_1 += 1;
                    
                    if (ecx_4 <= edi_1)
                        break;
                }
            }
            
            int32_t ebp_4 = edx_1 << 0xc;
            
            if (VirtualAlloc(i_1[0x204] + ebp_4, (edi_1 - edx_1) << 0xc, MEM_COMMIT, PAGE_READWRITE)
                    != ebp_4 + i_1[0x204])
                return 0;
            
            int32_t ecx_5 = i_1[3];
            int32_t* ebp_8 = (ecx_5 << 0xc) + i_1[0x204];
            
            while (ecx_5 < edi_1)
            {
                ecx_5 += 1;
                *ebp_8 = &ebp_8[2];
                ebp_8 = &ebp_8[0x400];
                ebp_8[-0x3ff] = 0xf0;
                ebp_8[-0x3c2] = 0xff;
                *(i_1 + ecx_5 + 0xf) = 0xf0;
                *(i_1 + ecx_5 + 0x40f) = 0xf1;
            }
            
            data_40b65c = i_1;
            
            if (edi_1 < 0x400)
            {
                while (*(i_1 + edi_1 + 0x10) != 0xff)
                {
                    edi_1 += 1;
                    
                    if (edi_1 >= 0x400)
                        break;
                }
            }
            
            int32_t ecx_6 = i_1[3];
            i_1[3] = 0xffffffff;
            
            if (edi_1 < 0x400)
                i_1[3] = edi_1;
            
            int32_t edx_5 = ecx_6 << 0xc;
            int32_t* eax_19 = i_1[0x204] + edx_5;
            eax_19[2] = arg1;
            i_1[2] = ecx_6;
            *(i_1 + ecx_6 + 0x10) -= arg1;
            *eax_19 = arg1 + eax_19 + 8;
            eax_19[1] -= arg1;
            return i_1[0x204] + edx_5 + 0x100;
        }
        
        i_1 = *i_1;
    } while (i_1 != &data_40ae48);
    
    void** eax_9 = sub_404fb0();
    
    if (!eax_9)
        return 0;
    
    void** edx = eax_9[0x204];
    edx[2] = arg1;
    data_40b65c = eax_9;
    *edx = arg1 + edx + 8;
    edx[1] = 0xf0 - arg1;
    eax_9[4] -= arg1;
    return eax_9[0x204] + 0x100;
}

void* sub_405590(int32_t* arg1, int32_t arg2, void* arg3)
{
    void* ecx = arg1[1];
    char* edi = *arg1;
    void* ebp = edi;
    
    if (arg3 <= ecx)
    {
        *edi = arg3;
        
        if (arg3 + edi >= &arg1[0x3e])
        {
            arg1[1] = 0;
            *arg1 = &arg1[2];
        }
        else
        {
            *arg1 += arg3;
            arg1[1] -= arg3;
        }
        
        return &arg1[(edi - arg1) * 4 + 0x20];
    }
    
    void* ecx_3 = ecx + edi;
    
    if (*ecx_3)
        ebp = ecx_3;
    
    int32_t esi_2 = arg2;
    
    while (arg3 + ebp < &arg1[0x3e])
    {
        void* ecx_4;
        ecx_4 = *ebp;
        
        if (ecx_4)
        {
            int32_t ebx_2;
            ebx_2 = ecx_4;
            ebp += ebx_2;
        }
        else
        {
            void* ecx_5 = ebp + 1;
            int32_t ebx_1 = 1;
            
            while (!*ecx_5)
            {
                ecx_5 += 1;
                ebx_1 += 1;
            }
            
            if (ebx_1 >= arg3)
            {
                int32_t esi_3 = arg3 + ebp;
                
                if (esi_3 >= &arg1[0x3e])
                {
                    arg1[1] = 0;
                    *arg1 = &arg1[2];
                }
                else
                {
                    *arg1 = esi_3;
                    arg1[1] = ebx_1 - arg3;
                }
                
                *ebp = arg3;
                return &arg1[(ebp - arg1) * 4 + 0x20];
            }
            
            if (edi != ebp)
            {
                esi_2 -= ebx_1;
                
                if (arg3 > esi_2)
                    return 0;
                
                ebp = ecx_5;
            }
            else
            {
                ebp = ecx_5;
                arg1[1] = ebx_1;
            }
        }
    }
    
    char* ebp_3 = &arg1[2];
    
    if (edi > &arg1[2])
    {
        while (arg3 + ebp_3 <= arg1 + 0xf7)
        {
            void* ecx_8;
            ecx_8 = *ebp_3;
            
            if (ecx_8)
            {
                int32_t ebx_8;
                ebx_8 = ecx_8;
                ebp_3 = &ebp_3[ebx_8];
            }
            else
            {
                void* ecx_9 = &ebp_3[1];
                int32_t ebx_7 = 1;
                
                while (!*ecx_9)
                {
                    ecx_9 += 1;
                    ebx_7 += 1;
                }
                
                if (ebx_7 >= arg3)
                {
                    int32_t esi_4 = arg3 + ebp_3;
                    
                    if (esi_4 >= &arg1[0x3e])
                    {
                        arg1[1] = 0;
                        *arg1 = &arg1[2];
                    }
                    else
                    {
                        *arg1 = esi_4;
                        arg1[1] = ebx_7 - arg3;
                    }
                    
                    *ebp_3 = arg3;
                    return &arg1[(ebp_3 - arg1) * 4 + 0x20];
                }
                
                esi_2 -= ebx_7;
                
                if (arg3 > esi_2)
                    return 0;
                
                ebp_3 = ecx_9;
            }
            
            if (edi <= ebp_3)
                break;
        }
    }
    
    return 0;
}

int32_t sub_405710(int32_t arg1, int32_t arg2, int32_t arg3)
{
    int32_t esi = 0;
    
    if (!data_40b668)
    {
        HMODULE hModule = LoadLibraryA("user32.dll");
        int32_t eax_1;
        
        if (hModule)
        {
            eax_1 = GetProcAddress(hModule, "MessageBoxA");
            data_40b668 = eax_1;
        }
        
        if (!hModule || !eax_1)
            return 0;
        
        data_40b66c = GetProcAddress(hModule, "GetActiveWindow");
        data_40b670 = GetProcAddress(hModule, "GetLastActivePopup");
    }
    
    int32_t eax_4 = data_40b66c;
    
    if (eax_4)
        esi = eax_4();
    
    if (esi && data_40b670)
        esi = data_40b670(esi);
    
    return data_40b668(esi, arg1, arg2, arg3);
}

char* sub_4057b0(char* arg1, char* arg2, int32_t arg3)
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
                    goto label_40582b;
                
            label_405897:
                eax = 0;
                uint32_t i;
                
                do
                {
                    *edi_1 = 0;
                    edi_1 = &edi_1[4];
                    i = i_3;
                    i_3 -= 1;
                } while (i != 1);
            label_4058a1:
                ebx_1 &= 3;
                
                if (ebx_1)
                    goto label_40582b;
                
                return arg1;
            }
        } while (esi_1 & 3);
        
        ebx_1 = ecx;
        i_2 = ecx >> 2;
        
        if (i_2)
            goto label_40583f;
        
    label_4057f0:
        ebx_1 &= 3;
        
        if (ebx_1)
            goto label_4057f5;
    }
    else
    {
        i_2 = ecx >> 2;
        
        if (i_2)
        {
        label_40583f:
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
                    label_40588f:
                        edi_1 = &edi_1[4];
                        eax = 0;
                        i_3 = i_2 - 1;
                        
                        if (i_2 == 1)
                            goto label_4058a1;
                        
                        goto label_405897;
                    }
                    
                    if (!*edx_2[1])
                    {
                        *edi_1 = edx_2 & 0xff;
                        goto label_40588f;
                    }
                    
                    if (!(edx_2 & 0xff0000))
                    {
                        *edi_1 = edx_2 & 0xffff;
                        goto label_40588f;
                    }
                    
                    if (!(edx_2 & 0xff000000))
                    {
                        *edi_1 = edx_2;
                        goto label_40588f;
                    }
                }
                
                *edi_1 = edx_2;
                edi_1 = &edi_1[4];
                i_1 = i_2;
                i_2 -= 1;
            } while (i_1 != 1);
            goto label_4057f0;
        }
        
    label_4057f5:
        
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
                    
                label_40582b:
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

int32_t sub_4058b0(int32_t arg1)
{
    int32_t eax = 0;
    void* i = &data_40b680;
    data_40a57c = arg1;
    
    do
    {
        if (*i == arg1)
        {
            int32_t eax_1 = *((eax << 3) + &data_40b684);
            data_40a578 = eax_1;
            return eax_1;
        }
        
        i += 8;
        eax += 1;
    } while (i < 0x40b7e8);
    
    if (arg1 >= 0x13 && arg1 <= 0x24)
    {
        data_40a578 = 0xd;
        return eax;
    }
    
    if (arg1 >= 0xbc && arg1 <= 0xca)
    {
        data_40a578 = 8;
        return eax;
    }
    
    data_40a578 = 0x16;
    return eax;
}

int32_t sub_405920(int32_t arg1)
{
    if (arg1 < data_40cf60)
    {
        int32_t* eax_7 = *(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + ((arg1 & 0x1f) << 3);
        
        if (eax_7[1] & 1 && *eax_7 != 0xffffffff)
        {
            if (data_40a5e0 == 1)
            {
                if (!arg1)
                {
                    int32_t var_c_1 = 0;
                    SetStdHandle(STD_INPUT_HANDLE, nullptr);
                }
                else if (arg1 == 1)
                {
                    int32_t var_c_2 = 0;
                    SetStdHandle(STD_OUTPUT_HANDLE, nullptr);
                }
                else if (arg1 == 2)
                {
                    int32_t var_c_3 = 0;
                    SetStdHandle(STD_ERROR_HANDLE, nullptr);
                }
            }
            
            (*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60))[(arg1 & 0x1f) * 2] = 0xffffffff;
            return 0;
        }
    }
    
    data_40a578 = 9;
    data_40a57c = 0;
    return 0xffffffff;
}

int32_t sub_4059b0(int32_t arg1)
{
    if (arg1 < data_40cf60)
    {
        int32_t* eax_4 = *(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + ((arg1 & 0x1f) << 3);
        
        if (eax_4[1] & 1)
            return *eax_4;
    }
    
    data_40a578 = 9;
    data_40a57c = 0;
    return 0xffffffff;
}

void* sub_405a00(int32_t arg1, int32_t arg2)
{
    uint32_t dwBytes = arg2 * arg1;
    
    if (dwBytes <= 0xffffffe0)
    {
        dwBytes = !dwBytes ? 0x10 : (dwBytes + 0xf) & 0xfffffff0;
    }
    
    int32_t i;
    
    do
    {
        void* result = nullptr;
        
        if (dwBytes <= 0xffffffe0)
        {
            if (data_40b664 < dwBytes)
            {
            label_405a5d:
                
                if (result)
                    return result;
            }
            else
            {
                result = sub_405310(dwBytes >> 4);
                
                if (result)
                {
                    __builtin_memset(__builtin_memset(result, 0, dwBytes >> 2 << 2), 0, 
                        dwBytes & 3);
                    goto label_405a5d;
                }
            }
            
            result = HeapAlloc(data_40ce54, HEAP_ZERO_MEMORY, dwBytes);
        }
        
        if (result || !data_40b7f0)
            return result;
        
        i = sub_405de0(dwBytes);
    } while (i);
    return 0;
}

int32_t sub_405aa0()
{
    int32_t result = 0;
    int32_t i = 3;
    
    if (data_40ce50 > 3)
    {
        int32_t ebx_1 = 0xc;
        
        do
        {
            int32_t* eax_2 = *(data_40be40 + ebx_1);
            
            if (eax_2)
            {
                if (eax_2[3] & 0x83 && sub_405e10(eax_2) != 0xffffffff)
                    result += 1;
                
                if (ebx_1 >= 0x50)
                {
                    sub_404eb0(*(data_40be40 + ebx_1));
                    *(data_40be40 + ebx_1) = 0;
                }
            }
            
            ebx_1 += 4;
            i += 1;
        } while (i < data_40ce50);
    }
    
    return result;
}

int32_t sub_405b20(int32_t* arg1)
{
    if (!arg1)
        return sub_405bf0(0);
    
    if (sub_405b70(arg1))
        return 0xffffffff;
    
    if (!(*(arg1 + 0xd) & 0x40))
        return 0;
    
    return 0 - 1;
}

int32_t sub_405b70(int32_t* arg1)
{
    int32_t result = 0;
    int32_t eax = arg1[3];
    
    if ((eax & 3) == 2 && eax & 0x108)
    {
        char* eax_1 = arg1[2];
        uint32_t ebx_2 = *arg1 - eax_1;
        
        if (ebx_2 > 0)
        {
            if (sub_404790(arg1[4], eax_1, ebx_2) != ebx_2)
            {
                arg1[3] |= 0x20;
                result = 0xffffffff;
            }
            else
            {
                int32_t eax_4 = arg1[3];
                
                if (eax_4 & 0x80)
                    arg1[3] = eax_4 & 0xfffffffd;
            }
        }
    }
    
    *arg1 = arg1[2];
    arg1[1] = 0;
    return result;
}

int32_t sub_405be0()
{
    return sub_405bf0(1);
}

int32_t sub_405bf0(int32_t arg1)
{
    int32_t ebx = 0;
    int32_t i = 0;
    int32_t var_4 = 0;
    int32_t esi;
    
    if (data_40ce50 <= 0)
        esi = arg1;
    else
    {
        int32_t ebp_1 = 0;
        esi = arg1;
        
        do
        {
            int32_t* ecx_1 = *(data_40be40 + ebp_1);
            
            if (ecx_1)
            {
                char eax_2 = ecx_1[3];
                
                if (eax_2 & 0x83)
                {
                    if (esi == 1)
                    {
                        if (sub_405b20(ecx_1) != 0xffffffff)
                            ebx += 1;
                    }
                    else if (!esi && eax_2 & 2 && sub_405b20(ecx_1) == 0xffffffff)
                        var_4 = 0xffffffff;
                }
            }
            
            ebp_1 += 4;
            i += 1;
        } while (i < data_40ce50);
    }
    
    if (esi == 1)
        return ebx;
    
    return var_4;
}

int32_t sub_405c80()
{
    return sub_4029f0(2);
}

int32_t __convention("regparm") sub_405c90(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{
    if (arg3 > arg4 && arg3 < arg4 + arg5)
    {
        int32_t esi_3 = arg4 + arg5;
        int32_t edi_3 = arg3 + arg5;
        void* esi_5;
        void* edi_5;
        
        if (edi_3 & 3)
        {
            if (arg5 <= 0xc)
            {
                __builtin_memcpy(edi_3 - 1 - arg5, esi_3 - 1 - arg5, arg5);
                return arg3;
            }
            
            int32_t count_1 = -(arg2) & 3;
            int32_t ecx_7 = arg5 - count_1;
            int32_t esi_7;
            int32_t edi_7;
            edi_7 = __builtin_memcpy(edi_3 - 1 - count_1, esi_3 - 1 - count_1, count_1);
            uint32_t ecx_10 = ecx_7 >> 2;
            edi_5 =
                __builtin_memcpy(edi_7 - 3 - (ecx_10 << 2), esi_7 - 3 - (ecx_10 << 2), ecx_10 << 2);
            
            switch (ecx_7 & 3)
            {
                case 0:
                {
                    return arg3;
                    break;
                }
                case 1:
                {
                    goto label_405d98;
                }
                case 2:
                {
                    goto label_405d88;
                }
                case 3:
                {
                    goto label_405d70;
                }
            }
        }
        else
        {
            uint32_t ecx_6 = arg5 >> 2;
            edi_5 =
                __builtin_memcpy(edi_3 - 4 - (ecx_6 << 2), esi_3 - 4 - (ecx_6 << 2), ecx_6 << 2);
            
            switch (arg5 & 3)
            {
                case 0:
                {
                    return arg3;
                    break;
                }
                case 1:
                {
                label_405d98:
                    arg1 = *(esi_5 + 3);
                    *(edi_5 + 3) = arg1;
                    return arg3;
                    break;
                }
                case 2:
                {
                label_405d88:
                    arg1 = *(esi_5 + 2);
                    *(edi_5 + 2) = arg1;
                    return arg3;
                    break;
                }
                case 3:
                {
                label_405d70:
                    arg1 = *(esi_5 + 2);
                    *(edi_5 + 2) = arg1;
                    arg1 = *(esi_5 + 1);
                    *(edi_5 + 1) = arg1;
                    return arg3;
                    break;
                }
            }
        }
    }
    
    char* esi_1;
    char* edi_1;
    
    if (!(arg3 & 3))
    {
        edi_1 = __builtin_memcpy(arg3, arg4, arg5 >> 2 << 2);
        
        switch (arg5 & 3)
        {
            case 0:
            {
                return arg3;
                break;
            }
            case 1:
            {
            label_405cfc:
                arg1 = *esi_1;
                *edi_1 = arg1;
                return arg3;
                break;
            }
            case 2:
            {
            label_405cec:
                arg1 = *esi_1;
                *edi_1 = arg1;
                return arg3;
                break;
            }
            case 3:
            {
            label_405cd8:
                arg1 = *esi_1;
                *edi_1 = arg1;
                arg1 = esi_1[2];
                edi_1[2] = arg1;
                return arg3;
                break;
            }
        }
        
        return;
    }
    
    if (arg5 <= 0xc)
    {
        __builtin_memcpy(arg3, arg4, arg5);
        return arg3;
    }
    
    int32_t count = -(arg3) & 3;
    int32_t ecx_2 = arg5 - count;
    int32_t esi_2;
    int32_t edi_2;
    edi_2 = __builtin_memcpy(arg3, arg4, count);
    edi_1 = __builtin_memcpy(edi_2, esi_2, ecx_2 >> 2 << 2);
    
    switch (ecx_2 & 3)
    {
        case 0:
        {
            return arg3;
            break;
        }
        case 1:
        {
            goto label_405cfc;
        }
        case 2:
        {
            goto label_405cec;
        }
        case 3:
        {
            goto label_405cd8;
        }
    }
}

int32_t sub_405de0(int32_t arg1)
{
    int32_t ecx = data_40be3c;
    
    if (ecx && ecx(arg1))
        return 1;
    
    return 0;
}

int32_t sub_405e10(int32_t* arg1)
{
    int32_t result = 0xffffffff;
    char eax = arg1[3];
    
    if (eax & 0x40)
    {
        arg1[3] = 0;
        return 0xffffffff;
    }
    
    if (eax & 0x83)
    {
        result = sub_405b70(arg1);
        sub_405fc0(arg1);
        
        if (sub_405ef0(arg1[4]) >= 0)
        {
            int32_t eax_5 = arg1[7];
            
            if (eax_5)
            {
                sub_404eb0(eax_5);
                arg1[7] = 0;
            }
        }
        else
            result = 0xffffffff;
    }
    
    arg1[3] = 0;
    return result;
}

enum WIN32_ERROR sub_405e80(int32_t arg1)
{
    if (data_40cf60 > arg1)
    {
        int32_t eax_4;
        eax_4 = *(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + ((arg1 & 0x1f) << 3) + 4);
        eax_4 &= 1;
        
        if (eax_4)
        {
            enum WIN32_ERROR result = NO_ERROR;
            
            if (!FlushFileBuffers(sub_4059b0(arg1)))
                result = GetLastError();
            
            if (!result)
                return result;
            
            data_40a578 = 9;
            data_40a57c = result;
            return 0xffffffff;
        }
    }
    
    data_40a578 = 9;
    return ~NO_ERROR;
}

int32_t sub_405ef0(int32_t arg1)
{
    if (data_40cf60 > arg1)
    {
        int32_t esi_1 = (arg1 & 0x1f) << 3;
        
        if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + esi_1 + 4) & 1)
        {
            enum WIN32_ERROR ebp_2;
            
            if (arg1 == 1 || arg1 == 2)
            {
                if (sub_4059b0(2) != sub_4059b0(1))
                    goto label_405f55;
                
                ebp_2 = NO_ERROR;
            }
            else
            {
            label_405f55:
                
                if (CloseHandle(sub_4059b0(arg1)))
                    ebp_2 = NO_ERROR;
                else
                    ebp_2 = GetLastError();
            }
            
            sub_405920(arg1);
            
            if (!ebp_2)
            {
                *(*(((arg1 & 0xffffffe7) >> 3) + &data_40ce60) + esi_1 + 4) = 0;
                return 0;
            }
            
            sub_4058b0(ebp_2);
            return 0xffffffff;
        }
    }
    
    data_40a578 = 9;
    data_40a57c = 0;
    return 0xffffffff;
}

char sub_405fc0(int32_t* arg1)
{
    char result = arg1[3];
    
    if (result & 0x83 && result & 8)
    {
        result = sub_404eb0(arg1[2]);
        *arg1 = 0;
        arg1[3] &= 0xfffffbf7;
        arg1[2] = 0;
        arg1[1] = 0;
    }
    
    return result;
}

void __stdcall RtlUnwind(void* TargetFrame, void* TargetIp, EXCEPTION_RECORD* ExceptionRecord, void* ReturnValue)
{
    /* tailcall */
    return RtlUnwind(TargetFrame, TargetIp, ExceptionRecord, ReturnValue);
}

char* sub_406010(char* arg1)
{
    void* ebp = nullptr;
    
    if (!data_40ae30)
    {
        char* eax = arg1;
        
        if (*arg1)
        {
            do
            {
                char ecx = *eax;
                
                if (ecx >= 0x61 && ecx <= 0x7a)
                    *eax = ecx - 0x20;
                
                eax = &eax[1];
            } while (*eax);
        }
        
        return arg1;
    }
    
    int32_t eax_3 = sub_4060d0(data_40ae30, 0x200, arg1, 0xffffffff, nullptr, 0, 0);
    
    if (eax_3)
    {
        ebp = sub_404f00(eax_3);
        
        if (ebp && sub_4060d0(data_40ae30, 0x200, arg1, 0xffffffff, ebp, eax_3, 0))
        {
            void* edi_1 = ebp;
            int32_t i = 0xffffffff;
            
            while (i)
            {
                bool cond:0_1 = 0 != *edi_1;
                edi_1 += 1;
                i -= 1;
                
                if (!cond:0_1)
                    break;
            }
            
            int32_t ecx_1 = ~i;
            int32_t esi_2;
            int32_t edi_4;
            edi_4 = __builtin_memcpy(arg1, edi_1 - ecx_1, ecx_1 >> 2 << 2);
            __builtin_memcpy(edi_4, esi_2, ecx_1 & 3);
        }
    }
    
    sub_404eb0(ebp);
    return arg1;
}

int32_t sub_4060d0(uint32_t arg1, uint32_t arg2, uint8_t* arg3, void* arg4, PWSTR arg5, int32_t arg6, uint32_t arg7)
{
    int32_t eax = data_40b7f8;
    
    if (!eax)
    {
        if (!LCMapStringA(0, 0x100, &data_40a4fc, 1, nullptr, 0))
        {
            if (!LCMapStringW(0, 0x100, &data_4077c4, 1, nullptr, 0))
                return 0;
            
            eax = 1;
        }
        else
            eax = 2;
    }
    
    void* esi = arg4;
    data_40b7f8 = eax;
    
    if (esi > 0)
    {
        esi = sub_406300(arg3, esi);
        eax = data_40b7f8;
    }
    
    data_40b7f8 = eax;
    
    if (eax == 2)
        return LCMapStringA(arg1, arg2, arg3, esi, arg5, arg6);
    
    data_40b7f8 = eax;
    
    if (eax != 1)
        return eax;
    
    wchar16* edi_1 = nullptr;
    
    if (!arg7)
        arg7 = data_40ae40;
    
    int32_t eax_11 =
        MultiByteToWideChar(arg7, MB_ERR_INVALID_CHARS | MB_PRECOMPOSED, arg3, esi, nullptr, 0);
    
    if (!eax_11)
        return 0;
    
    void* eax_14 = sub_404f00(eax_11 << 1);
    
    if (!eax_14)
        return 0;
    
    if (MultiByteToWideChar(arg7, MB_PRECOMPOSED, arg3, esi, eax_14, eax_11))
    {
        int32_t esi_1 = LCMapStringW(arg1, arg2, eax_14, eax_11, nullptr, 0);
        
        if (esi_1)
        {
            if (!(*arg2[1] & 4))
            {
                edi_1 = sub_404f00(esi_1 << 1);
                
                if (edi_1 && LCMapStringW(arg1, arg2, eax_14, eax_11, edi_1, esi_1))
                {
                    if (arg6)
                    {
                        esi_1 = WideCharToMultiByte(arg7, 0x220, edi_1, esi_1, arg5, arg6, nullptr, 
                            nullptr);
                        
                        if (esi_1)
                            goto label_4062e3;
                    }
                    else
                    {
                        esi_1 = WideCharToMultiByte(arg7, 0x220, edi_1, esi_1, nullptr, 0, nullptr, 
                            nullptr);
                        
                        if (esi_1)
                            goto label_4062e3;
                    }
                }
            }
            else
            {
                if (!arg6)
                {
                label_4062e3:
                    sub_404eb0(eax_14);
                    sub_404eb0(edi_1);
                    return esi_1;
                }
                
                if (esi_1 <= arg6 && LCMapStringW(arg1, arg2, eax_14, eax_11, arg5, arg6))
                    goto label_4062e3;
            }
        }
    }
    
    sub_404eb0(eax_14);
    sub_404eb0(edi_1);
    return 0;
}

void* sub_406300(char* arg1, void* arg2)
{
    char* esi = arg1;
    void* i_1 = arg2 - 1;
    
    if (arg2)
    {
        void* i;
        
        do
        {
            if (!*esi)
                return esi - arg1;
            
            esi = &esi[1];
            i = i_1;
            i_1 -= 1;
        } while (i);
    }
    
    if (*esi)
        return arg2;
    
    return esi - arg1;
}

