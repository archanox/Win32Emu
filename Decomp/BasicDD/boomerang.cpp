void *WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow);
__size32 proc_0x00401200();
__size32 proc_0x00401310(__size32 param1);
void proc_0x00401420(__size32 param1);
void proc_0x00401640(__size32 *param1, __size32 param2, __size32 param3, __size32 param4);
__size32 proc_0x004014d0(__size32 param1, __size32 param2);
__size32 proc_0x00401130();
__size32 proc_0x004017d0(__size32 param1);
__size32 proc_0x00401730(__size32 *param1, __size32 param2, __size32 param3, __size32 param4, __size32 param5, __size32 param6);
void proc_0x004017f0(__size32 param1);

unsigned int global_0x00409548 = 0;
int global_0x0040957c = 0;
__size32 global_0x00409580 = 0;// 4 bytes
__size32 global_0x00409584 = 0;// 4 bytes
__size32 global_0x0040958c = 0;// 4 bytes
__size32 global_0x00409590 = 0;// 4 bytes
__size32 global_0x00409594 = 0;// 4 bytes

/** address: 0x00401040 */
void *WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
    int eax; 		// r24
    __size32 ebp; 		// r29
    __size32 ebx; 		// r27
    int ecx; 		// r25
    __size32 edi; 		// r31
    int edx; 		// r26
    __size32 esi; 		// r30
    int esp; 		// r28
    unsigned int *esp_1; 		// r28{5}
    void *esp_10; 		// r28{63}
    void *esp_11; 		// r28{63}
    void *esp_12; 		// r28{70}
    void *esp_13; 		// r28{0}
    void *esp_14; 		// r28{0}
    void *esp_15; 		// r28{70}
    void *esp_16; 		// r28{70}
    void *esp_17; 		// r28{48}
    void *esp_18; 		// r28{73}
    void *esp_19; 		// r28{48}
    void *esp_2; 		// r28{12}
    void *esp_20; 		// r28{48}
    void *esp_3; 		// r28{12}
    void *esp_4; 		// r28{12}
    void *esp_5; 		// r28{33}
    void *esp_6; 		// r28{33}
    void *esp_7; 		// r28{33}
    void *esp_8; 		// r28{41}
    void *esp_9; 		// r28{63}
    HINSTANCE local0; 		// m[esp + 4]
    int local1; 		// m[esp + 16]
    int local10; 		// m[esp_13 + 4]{70}
    int local100; 		// %NF{64}
    int local101; 		// %NF{64}
    int local102; 		// %NF{70}
    int local103; 		// %NF{70}
    int local104; 		// %NF{70}
    int local105; 		// %NF{73}
    int local106; 		// %NF{73}
    int local107; 		// %NF{73}
    int local108; 		// %NF{55}
    int local109; 		// %NF{55}
    HINSTANCE local11; 		// m[esp_13 + 4]{1}
    int local110; 		// %OF{13}
    int local111; 		// %OF{13}
    int local112; 		// %OF{56}
    int local113; 		// %OF{13}
    int local114; 		// %OF{64}
    int local115; 		// %OF{64}
    int local116; 		// %OF{70}
    int local117; 		// %OF{70}
    int local118; 		// %OF{70}
    int local119; 		// %OF{73}
    HINSTANCE local12; 		// m[esp_13 + 4]{1}
    int local120; 		// %OF{73}
    int local121; 		// %OF{73}
    int local122; 		// %OF{56}
    int local123; 		// %OF{56}
    void *local124; 		// esp_17{48}
    int local125; 		// local56{52}
    int local126; 		// local70{53}
    int local127; 		// local84{54}
    int local128; 		// local98{55}
    int local129; 		// local112{56}
    int local13; 		// m[esp_13 + 8]{63}
    int local14; 		// m[esp_13 + 8]{63}
    int local15; 		// m[esp_13 + 8]{63}
    int local16; 		// m[esp_13 + 8]{70}
    int local17; 		// m[esp_13 + 8]{70}
    HINSTANCE local18; 		// m[esp_13 + 8]{0}
    HINSTANCE local19; 		// m[esp_13 + 8]{0}
    int local2; 		// m[esp - 32]
    int local20; 		// m[esp_13 + 12]{63}
    int local21; 		// m[esp_13 + 12]{63}
    int local22; 		// m[esp_13 + 12]{63}
    int local23; 		// m[esp_13 + 12]{70}
    int local24; 		// m[esp_13 + 12]{70}
    LPSTR local25; 		// m[esp_13 + 12]{0}
    LPSTR local26; 		// m[esp_13 + 12]{0}
    int local27; 		// m[esp_13 + 16]{63}
    int local28; 		// m[esp_13 + 16]{63}
    int local29; 		// m[esp_13 + 16]{63}
    unsigned int local3; 		// m[esp - 36]
    int local30; 		// m[esp_13 + 16]{70}
    int local31; 		// m[esp_13 + 16]{70}
    int local32; 		// m[esp_13 + 16]{0}
    int local33; 		// m[esp_13 + 16]{0}
    void *local34; 		// m[esp_9 - 4]{68}
    void *local35; 		// m[esp_9 - 4]{68}
    unsigned int local36; 		// m[esp_9 - 4]{74}
    unsigned int local37; 		// m[esp_9 - 4]{76}
    int local38; 		// m[esp_13 - 32]{63}
    int local39; 		// m[esp_13 - 32]{63}
    HINSTANCE local4; 		// m[esp + 8]
    int local40; 		// m[esp_13 - 32]{63}
    int local41; 		// m[esp_13 - 32]{0}
    int local42; 		// m[esp_13 - 32]{70}
    int local43; 		// m[esp_13 - 32]{70}
    int local44; 		// m[esp_13 - 32]{0}
    int local45; 		// m[esp_13 - 32]{0}
    int local46; 		// m[esp_13 - 36]{63}
    int local47; 		// m[esp_13 - 36]{63}
    int local48; 		// m[esp_13 - 36]{63}
    unsigned int local49; 		// m[esp_13 - 36]{0}
    LPSTR local5; 		// m[esp + 12]
    int local50; 		// m[esp_13 - 36]{70}
    int local51; 		// m[esp_13 - 36]{70}
    unsigned int local52; 		// m[esp_13 - 36]{0}
    unsigned int local53; 		// m[esp_13 - 36]{0}
    int local54; 		// %flags{13}
    int local55; 		// %flags{13}
    int local56; 		// %flags{52}
    int local57; 		// %flags{13}
    int local58; 		// %flags{64}
    int local59; 		// %flags{64}
    int local6; 		// m[esp_13 + 4]{63}
    int local60; 		// %flags{70}
    int local61; 		// %flags{70}
    int local62; 		// %flags{70}
    int local63; 		// %flags{73}
    int local64; 		// %flags{73}
    int local65; 		// %flags{73}
    int local66; 		// %flags{52}
    int local67; 		// %flags{52}
    int local68; 		// %ZF{13}
    int local69; 		// %ZF{13}
    int local7; 		// m[esp_13 + 4]{63}
    int local70; 		// %ZF{53}
    int local71; 		// %ZF{13}
    int local72; 		// %ZF{64}
    int local73; 		// %ZF{64}
    int local74; 		// %ZF{70}
    int local75; 		// %ZF{70}
    int local76; 		// %ZF{70}
    int local77; 		// %ZF{73}
    int local78; 		// %ZF{73}
    int local79; 		// %ZF{73}
    int local8; 		// m[esp_13 + 4]{63}
    int local80; 		// %ZF{53}
    int local81; 		// %ZF{53}
    int local82; 		// %CF{13}
    int local83; 		// %CF{13}
    int local84; 		// %CF{54}
    int local85; 		// %CF{13}
    int local86; 		// %CF{64}
    int local87; 		// %CF{64}
    int local88; 		// %CF{70}
    int local89; 		// %CF{70}
    int local9; 		// m[esp_13 + 4]{70}
    int local90; 		// %CF{70}
    int local91; 		// %CF{73}
    int local92; 		// %CF{73}
    int local93; 		// %CF{73}
    int local94; 		// %CF{54}
    int local95; 		// %CF{54}
    int local96; 		// %NF{13}
    int local97; 		// %NF{13}
    int local98; 		// %NF{55}
    int local99; 		// %NF{13}

    global_0x0040957c = hInstance;
    eax = proc_0x00401200(); /* Warning: also results in esp_1, esi */
    global_0x00409580 = eax;
    if (eax != 0) {
        eax = proc_0x00401310(edi); /* Warning: also results in edx, esp_2, edi */
        local54 = LOGICALFLAGS32(eax);
        local129 = local110;
        local128 = local96;
        local127 = local82;
        local126 = local68;
        local125 = local54;
        if (eax >= 0) {
            *(__size32*)(esp_2 - 4) = ebx;
            *(__size32*)(esp_2 - 8) = esi;
            *(__size32*)(esp_2 - 12) = edi;
            *(__size32*)(esp_2 - 16) = -1;
            *(__size32*)(esp_2 - 20) = 280;
            *(__size32*)(esp_2 - 24) = 1500;
            *(__size32*)(esp_2 - 28) = global_0x00409584;
            esp_5 = proc_0x00401640(*(esp_2 - 28), *(esp_2 - 24), *(esp_2 - 20), 0x409550);
            *(__size32*)(esp_5 - 4) = 280;
            *(__size32*)(esp_5 - 8) = 1500;
            *(__size32*)(esp_5 - 12) = 0;
            *(__size32*)(esp_5 - 16) = 0;
            *(__size32*)(esp_5 - 20) = 101;
            *(__size32*)(esp_5 - 24) = global_0x0040957c;
            eax = proc_0x004014d0(*(esp_5 - 8), 0x409550); /* Warning: also results in ecx, esp_8, ebp */
            local124 = esp_8;
            esi = PeekMessageA;
            edi = TranslateMessage;
            ebx = DispatchMessageA;
            for(;;) {
bb0x4010ec:
                esp_17 = local124;
                local56 = local125;
                local70 = local126;
                local84 = local127;
                local98 = local128;
                local112 = local129;
                *(__size32*)(esp_17 - 4) = 1;
                *(__size32*)(esp_17 - 8) = 0;
                *(__size32*)(esp_17 - 12) = 0;
                *(__size32*)(esp_17 - 16) = 0;
                *(void **)(esp_17 - 20) = esp_17 + 12;
                (*esi)(eax, ecx, esp_17 + 12, ebx, ebp, esi, edi, local56, local70, local84, local98, local112, hInstance, nCmdShow, local41, local49, hPrevInstance, lpCmdLine);
                local58 = LOGICALFLAGS32(eax);
                local129 = local114;
                local128 = local100;
                local127 = local86;
                local126 = local72;
                local125 = local58;
                if (eax != 0) {
                    break;
                }
                eax = proc_0x00401130(); /* Warning: also results in ecx, ebx, esp, ebp, esi, edi */
                local124 = esp;
            }
            tmp1 = *(esp_9 + 16) - 18;
            if (*(esp_9 + 16) != 18) {
                local34 = esp_9 + 12;
                (*edi)(esp_9 + 12, ecx, edx, ebx, ebp, esi, edi, <all>, SUBFLAGS32(*(esp_9 + 16), 18, tmp1), tmp1 == 0, *(esp_9 + 16) < 18, tmp1 < 0, *(esp_9 + 16) < 0 && tmp1 >= 0, local6, local27, local38, local46, local13, local20);
                *(void **)(esp_12 - 4) = esp_12 + 12;
                (*ebx)(eax, esp_12 + 12, edx, ebx, ebp, esi, edi, <all>, local60, local74, local88, local102, local116, local9, local30, local42, local50, local16, local23);
                local129 = local119;
                local128 = local105;
                local127 = local91;
                local126 = local77;
                local125 = local63;
                local124 = esp_18;
                goto bb0x4010ec;
            }
            proc_0x00401420(edx);
            eax = 0;
        }
        else {
            esp = proc_0x00401420(edx);
            *(__size32*)(esp - 4) = 48;
            *(__size32*)(esp - 8) = 0x40709c;
            *(__size32*)(esp - 12) = 0x407030;
            *(__size32*)(esp - 16) = global_0x00409580;
            MessageBoxA(*(esp - 16), *(esp - 12), *(esp - 8), *(esp - 4));
            eax = 0;
        }
    }
    else {
        eax = -1;
    }
    return eax;
}

/** address: 0x00401200 */
__size32 proc_0x00401200()
{
    HWND eax; 		// r24
    HICON eax_1; 		// r24{2}
    __size32 eax_2; 		// r24{4}
    int ecx; 		// r25
    int edx; 		// r26
    void * () *esi; 		// r30
    void (void) *esi_1; 		// r30
    int esp; 		// r28
    void *esp_1; 		// r28{6}
    void *esp_2; 		// r28{6}
    void *esp_3; 		// r28{6}
    union { unsigned int; void *; } esp_4; 		// r28{1}
    union { unsigned int; void *; } esp_5; 		// r28{1}
    HINSTANCE local0; 		// m[esp - 52]
    LPCSTR local1; 		// m[esp - 48]
    int local10; 		// m[esp - 40]
    __size32 local11; 		// m[esp - 44]
    int local12; 		// m[esp - 56]
    int local13; 		// m[esp - 60]
    int local14; 		// m[esp - 64]
    int local15; 		// m[esp - 68]
    int local16; 		// m[esp - 72]
    unsigned int local17; 		// m[esp - 76]
    int local2; 		// m[esp - 8]
    int local3; 		// m[esp - 12]
    __size32 local4; 		// m[esp - 16]
    HICON local5; 		// m[esp - 20]
    int local6; 		// m[esp - 24]
    int local7; 		// m[esp - 28]
    int local8; 		// m[esp - 32]
    int local9; 		// m[esp - 36]

    eax_1 = LoadIconA(global_0x0040957c, 0x7f00);
    LoadCursorA(0, 0x7f00);
    eax_2 = GetStockObject();
    eax = RegisterClassA(); /* Warning: also results in edx */
    (*GetSystemMetrics)(eax, global_0x0040957c, edx, GetSystemMetrics, SUBFLAGS32(esp_4, 40, esp_4 - 40), esp_4 == 40, esp_4 < 40, esp_4 < 40, esp_4 < 0 && esp_4 >= 40, 0x4070a4, 0x409598, eax_2, eax_1, global_0x0040957c, 0, 0, 0x4012d0, 3, esi, 4, esp_4 - 44, 0, global_0x0040957c, 0, 0, 1, pc);
    *(__size32*)(esp_1 - 4) = eax;
    *(__size32*)(esp_1 - 8) = 0;
    (*esi_1)(eax, ecx, edx, esi_1, <all>, flags, ZF, CF, NF, OF, *(esp_4 - 8), *(esp_4 - 12), *(esp_4 - 16), *(esp_4 - 20), *(esp_4 - 24), *(esp_4 - 28), *(esp_4 - 32), *(esp_4 - 36), *(esp_4 - 40), *(esp_4 - 44), *(esp_4 - 48), *(esp_4 - 52), *(esp_4 - 56), *(esp_4 - 60), *(esp_4 - 64), *(esp_4 - 68), *(esp_4 - 72), *(esp_4 - 76));
    *(__size32*)(esp - 4) = eax;
    local2 = 0;
    local3 = 0;
    local4 = 0x80000000;
    local5 = 0x4070a4;
    local6 = 0x4070a4;
    local7 = 8;
    eax = CreateWindowExA(*(esp - 28), *(esp - 24), *(esp - 20), *(esp - 16), *(esp - 12), *(esp - 8), *(esp - 4), *esp, *(esp + 4), *(esp + 8), *(esp + 12), *(esp + 16));
    edx = *(esp + 68);
    *(__size32*)(esp + 16) = edx;
    *(HWND*)(esp + 12) = eax;
    ShowWindow(*(esp + 12), *(esp + 16));
    *(HWND*)(esp + 16) = eax;
    UpdateWindow(*(esp + 16));
    *(HWND*)(esp + 16) = eax;
    SetFocus(*(esp + 16));
    esi = *(esp + 20);
    return eax; /* WARNING: Also returning: esi := esi */
}

/** address: 0x00401310 */
__size32 proc_0x00401310(__size32 param1)
{
    int eax; 		// r24
    __size32 *eax_1; 		// r24
    __size32 *eax_10; 		// r24{16}
    int eax_2; 		// r24{3}
    int eax_3; 		// r24{3}
    int eax_4; 		// r24{3}
    __size32 *eax_5; 		// r24{10}
    __size32 *eax_6; 		// r24{10}
    int eax_7; 		// r24{12}
    int eax_8; 		// r24{12}
    __size32 *eax_9; 		// r24{16}
    int ecx; 		// r25
    __size32 edi; 		// r31
    __size32 edx; 		// r26
    int esp; 		// r28
    void *esp_1; 		// r28{12}
    void *esp_10; 		// r28{1}
    void *esp_11; 		// r28{1}
    void *esp_2; 		// r28{12}
    void *esp_3; 		// r28{12}
    void *esp_4; 		// r28{25}
    void *esp_5; 		// r28{25}
    void *esp_6; 		// r28{25}
    __size32 *esp_7; 		// r28{43}
    __size32 *esp_8; 		// r28{43}
    __size32 *esp_9; 		// r28{43}
    int local0; 		// m[esp - 144]
    int local1; 		// m[esp - 148]
    int local10; 		// m[esp_10 - 148]{12}
    int local11; 		// m[esp_10 - 148]{25}
    int local12; 		// m[esp_10 - 152]{12}
    int local13; 		// m[esp_10 - 152]{25}
    int local14; 		// m[esp_10 - 156]{12}
    int local15; 		// m[esp_10 - 156]{25}
    int local16; 		// m[esp_10 - 160]{12}
    int local17; 		// m[esp_10 - 160]{25}
    int local18; 		// m[esp_10 - 164]{12}
    int local19; 		// m[esp_10 - 164]{25}
    int local2; 		// m[esp - 152]
    int local20; 		// m[esp_10 - 168]{12}
    int local21; 		// m[esp_10 - 168]{25}
    int local22; 		// m[esp_10 - 172]{12}
    int local23; 		// m[esp_10 - 172]{25}
    __size32 local24; 		// param1{8}
    int local3; 		// m[esp - 156]
    int local4; 		// m[esp - 160]
    int local5; 		// m[esp - 164]
    __size32 *local6; 		// m[esp - 168]
    unsigned int local7; 		// m[esp - 172]
    int local8; 		// m[esp_10 - 144]{12}
    int local9; 		// m[esp_10 - 144]{25}

    local24 = param1;
    eax_2 = DirectDrawCreateEx(); /* Warning: also results in edx */
    if (eax_2 == 0) {
        eax_5 = *0x409584;
        ecx = *eax_5;
        (**(*eax_5 + 80))(eax_5, ecx, global_0x00409580, LOGICALFLAGS32(eax_2), eax_2 == 0, 0, eax_2 < 0, 0, param1, 0, 0x406114, 0x409584, 0, 17, global_0x00409580, eax_5, pc);
        local24 = edi;
        if (eax_7 == 0) {
            eax_9 = *0x409584;
            *(__size32*)(esp_1 - 4) = 0;
            *(__size32*)(esp_1 - 8) = 0;
            *(__size32*)(esp_1 - 12) = 16;
            ecx = *eax_9;
            *(__size32*)(esp_1 - 16) = 480;
            *(__size32*)(esp_1 - 20) = 640;
            *(__size32 **)(esp_1 - 24) = eax_9;
            (**(*eax_9 + 84))(eax_9, ecx, edx, edi, <all>, LOGICALFLAGS32(eax_7), eax_7 == 0, 0, eax_7 < 0, 0, local8, local10, local12, local14, local16, local18, local20, local22);
            local24 = edi;
            if (eax == 0) {
                *(__size32*)(esp_4 - 4) = edi;
                *(__size32*)(esp_4 + 16) = 0;
                eax_1 = *0x409584;
                *(__size32*)(esp_4 - 8) = 0;
                *(__size32*)(esp_4 + 16) = 124;
                *(__size32*)(esp_4 + 20) = 33;
                *(__size32*)(esp_4 + 120) = 536;
                *(__size32*)(esp_4 + 36) = 1;
                edx = *eax_1;
                *(__size32*)(esp_4 - 12) = 0x409588;
                *(void **)(esp_4 - 16) = esp_4 + 16;
                *(__size32 **)(esp_4 - 20) = eax_1;
                (**(*eax_1 + 24))(eax_1, esp_4 + 16, edx, esp_4 + ( (DF == 0) ? 4 : -4) + 16, <all>, LOGICALFLAGS32(0), 1, 0, 0, 0, local9, local11, local13, local15, local17, local19, local21, local23);
                edi = *esp_7;
                local24 = edi;
                if (eax == 0) {
                    eax_1 = *0x409588;
                    *(__size32*)(esp_7 + 4) = 0;
                    *(__size32*)(esp_7 + 4) = 4;
                    *(__size32*)(esp_7 + 8) = 0;
                    *(__size32*)esp_7 = 0x40958c;
                    *(__size32*)(esp_7 + 12) = 0;
                    *(__size32*)(esp_7 + 16) = 0;
                    ecx = *eax_1;
                    *(void **)(esp_7 - 4) = esp_7 + 4;
                    *(__size32 **)(esp_7 - 8) = eax_1;
                    (**(*eax_1 + 48))(eax_1, ecx, esp_7 + 4, edi, <all>, LOGICALFLAGS32(0), 1, 0, 0, 0, *(esp_10 - 144), *(esp_10 - 148), *(esp_10 - 152), *(esp_10 - 156), *(esp_10 - 160), *(esp_10 - 164), *(esp_10 - 168), *(esp_10 - 172));
                    local24 = edi;
                    eax = 0 - (eax != 0);
                }
                else {
                    eax = -1;
                }
            }
            else {
                eax = -3;
            }
        }
        else {
            eax = -2;
        }
    }
    else {
        eax = -1;
    }
    param1 = local24;
    return eax; /* WARNING: Also returning: edx := edx, edi := param1 */
}

/** address: 0x00401420 */
void proc_0x00401420(__size32 param1)
{
    union { int; __size32 *; } eax; 		// r24
    __size32 ecx; 		// r25
    __size32 edx; 		// r26
    __size32 esi; 		// r30
    void *esp_1; 		// r28{2}
    void *esp_12; 		// r28{0}
    void *esp_4; 		// r28{8}
    void *esp_5; 		// r28{18}
    void *esp_6; 		// r28{11}
    void *esp_9; 		// r28{20}
    unsigned int local1; 		// m[esp_12 - 4]{0}
    void *local2; 		// esp_6{11}
    void *local3; 		// esp_9{20}

    ecx = proc_0x004017d0(0x409550); /* Warning: also results in esp_1, esi */
    local2 = esp_1;
    eax = *0x40958c;
    if (eax != 0) {
        ecx = *eax;
        *(union { int; __size32 *; }*)(esp_1 - 4) = eax;
        (**(*eax + 8))(eax, ecx, esi, LOGICALFLAGS32(eax), eax == 0, 0, eax < 0, 0, param1, pc);
        local2 = esp_4;
    }
    edx = param1;
    esp_6 = local2;
    local3 = esp_6;
    eax = *0x409588;
    if (eax != 0) {
        edx = *eax;
        *(union { int; __size32 *; }*)(esp_6 - 4) = eax;
        (**(*eax + 8))(eax, ecx, edx, esi, LOGICALFLAGS32(eax), eax == 0, 0, eax < 0, 0, *(esp_12 - 4));
        local3 = esp_5;
    }
    esp_9 = local3;
    eax = *0x409584;
    if (eax != 0) {
        ecx = *eax;
        *(union { int; __size32 *; }*)(esp_9 - 4) = eax;
        (**(*eax + 8))(eax, ecx, edx, esi, LOGICALFLAGS32(eax), eax == 0, 0, eax < 0, 0, local1);
    }
    return;
}

/** address: 0x00401640 */
void proc_0x00401640(__size32 *param1, __size32 param2, __size32 param3, __size32 param4)
{
    __size32 eax; 		// r24
    unsigned int eax_1; 		// r24{8}
    __size32 ebp; 		// r29
    __size32 ebx; 		// r27
    __size32 ecx; 		// r25
    __size32 edi; 		// r31
    union { int; void *; } edx; 		// r26
    __size32 *esi; 		// r30
    unsigned int esi_1; 		// r30
    union { int; void *; } esp; 		// r28
    __size32 *esp_1; 		// r28{29}
    union { int; void *; } esp_2; 		// r28{1}
    union { int; void *; } esp_3; 		// r28{1}
    __size32 *esp_4; 		// r28{22}
    __size32 *esp_5; 		// r28{8}
    __size32 *esp_6; 		// r28{8}
    __size32 *esp_7; 		// r28{8}
    __size32 *esp_8; 		// r28{29}
    __size32 *esp_9; 		// r28{29}
    __size32 *local0; 		// m[esp + 4]
    __size32 local1; 		// m[esp + 8]
    __size32 local10; 		// m[esp - 144]
    __size32 local11; 		// m[esp - 148]
    int local12; 		// m[esp - 152]
    __size32 local13; 		// m[esp - 156]
    union { int; void *; } local14; 		// m[esp - 160]
    __size32 *local15; 		// m[esp - 164]
    unsigned int local16; 		// m[esp - 168]
    int local17; 		// m[esp_2 + 4]{8}
    int local18; 		// m[esp_2 + 4]{8}
    __size32 *local19; 		// m[esp_2 + 4]{2}
    __size32 local2; 		// m[esp + 12]
    __size32 *local20; 		// m[esp_2 + 4]{2}
    int local21; 		// m[esp_2 - 20]{0}
    int local22; 		// m[esp_2 - 112]{0}
    int local23; 		// m[esp_2 - 116]{0}
    int local24; 		// m[esp_2 - 120]{0}
    int local25; 		// m[esp_2 - 124]{0}
    int local26; 		// m[esp_2 - 136]{0}
    int local27; 		// m[esp_2 - 140]{0}
    int local28; 		// m[esp_2 - 144]{0}
    int local29; 		// m[esp_2 - 148]{0}
    int local3; 		// m[esp - 20]
    int local30; 		// m[esp_2 - 152]{0}
    int local31; 		// m[esp_2 - 156]{0}
    int local32; 		// m[esp_2 - 160]{0}
    int local33; 		// m[esp_2 - 164]{0}
    int local34; 		// m[esp_2 - 168]{0}
    unsigned int local35; 		// eax{19}
    __size32 *local36; 		// esp_4{22}
    __size32 *local37; 		// esp_1{29}
    __size32 *local38; 		// esp{44}
    __size32 local4; 		// m[esp - 112]
    __size32 local5; 		// m[esp - 116]
    int local6; 		// m[esp - 120]
    int local7; 		// m[esp - 124]
    __size32 local8; 		// m[esp - 136]
    __size32 local9; 		// m[esp - 140]

    ecx = *param1;
    (**(*param1 + 24))(param3, ecx, esp_2 - 124, param4, param2, param1, param4 + 40, LOGICALFLAGS32(0), 1, 0, 0, 0, param1, param2, param3, 0x4040, param2, param3, 7, 124, ebx, ebp, esi, edi, 0, param4 + 40, esp_2 - 124, param1, pc);
    local37 = esp_5;
    local36 = esp_5;
    local35 = eax_1;
    if (eax_1 == 0) {
bb0x4016d9:
        esp_1 = local37;
        local38 = esp_1;
        esi_1 = *(esp_1 + 164);
        if (esi_1 != -1) {
            edi = *edi;
            *(unsigned int*)(esp_1 + 16) = esi_1;
            *(__size32*)(esp_1 + 20) = 0;
            edx = *edi;
            *(void **)(esp_1 - 4) = esp_1 + 16;
            *(__size32*)(esp_1 - 8) = 8;
            *(__size32 **)(esp_1 - 12) = edi;
            (**(*edi + 116))(esp_1 + 16, ecx, edx, ebx, ebp, esi_1, edi, <all>, SUBFLAGS32(esi_1, -1, esi_1 + 1), esi_1 == -1, esi_1 < (unsigned int)-1, (int)esi_1 < -1, (int)esi_1 >= 0 && (int)esi_1 < -1, local17, *(esp_2 + 8), *(esp_2 + 12), local21, local22, local23, local24, local25, local26, local27, local28, local29, local30, local31, local32, local33, local34);
            local38 = esp;
        }
        esp = local38;
        ecx = *(esp + 160);
        *(unsigned int*)(ebx + 28) = esi_1;
        *(__size32*)(ebx + 36) = ebp;
        *(__size32*)(ebx + 32) = ecx;
    }
    else {
        if (eax_1 == 0x8876017c) {
            eax = *esi;
            *(__size32*)(esp_5 - 4) = 0;
            *(__size32 **)(esp_5 - 8) = edi;
            *(void **)(esp_5 - 12) = esp_5 + 24;
            *(__size32 **)(esp_5 - 16) = esi;
            *(__size32*)(esp_5 + 128) = 0x840;
            (**(*esi + 24))(eax, esp_5 + 24, edx, ebx, ebp, esi, edi, <all>, SUBFLAGS32(eax_1, 0x8876017c, eax_1 + 0x7789fe84), eax_1 == 0x8876017c, eax_1 < (unsigned int)0x8876017c, (int)eax_1 < 0x8876017c, (int)eax_1 >= 0 && (int)eax_1 < 0x8876017c, local17, *(esp_2 + 8), *(esp_2 + 12), *(esp_2 - 20), *(esp_2 - 112), *(esp_2 - 116), *(esp_2 - 120), *(esp_2 - 124), *(esp_2 - 136), *(esp_2 - 140), *(esp_2 - 144), *(esp_2 - 148), *(esp_2 - 152), *(esp_2 - 156), *(esp_2 - 160), *(esp_2 - 164), *(esp_2 - 168));
            local36 = esp;
            local35 = eax;
        }
        eax = local35;
        esp_4 = local36;
        local37 = esp_4;
        if (eax == 0) {
            goto bb0x4016d9;
        }
    }
    return;
}

/** address: 0x004014d0 */
__size32 proc_0x004014d0(__size32 param1, __size32 param2)
{
    __size32 eax; 		// r24
    union { int; __size32 *; } eax_1; 		// r24{3}
    int eax_10; 		// r24{25}
    int eax_11; 		// r24{25}
    int eax_12; 		// r24{27}
    __size32 *eax_13; 		// r24{29}
    __size32 *eax_14; 		// r24{29}
    int eax_15; 		// r24{42}
    int eax_16; 		// r24{42}
    int eax_17; 		// r24{42}
    __size32 eax_18; 		// r24{45}
    __size32 eax_19; 		// r24{50}
    union { int; __size32 *; } eax_2; 		// r24{3}
    __size32 *eax_20; 		// r24{63}
    __size32 *eax_21; 		// r24{63}
    union { int; __size32 *; } eax_3; 		// r24{3}
    union { unsigned int; __size32 *; } eax_4; 		// r24{5}
    union { unsigned int; __size32 *; } eax_5; 		// r24{5}
    union { int; __size32 *; } eax_6; 		// r24{11}
    union { int; __size32 *; } eax_7; 		// r24{11}
    union { int; __size32 *; } eax_8; 		// r24{11}
    int eax_9; 		// r24{25}
    union { int; __size32 *; } ebp; 		// r29
    __size32 ebx; 		// r27
    int ecx; 		// r25
    __size32 ecx_1; 		// r25{3}
    __size32 ecx_2; 		// r25{3}
    __size32 ecx_3; 		// r25{3}
    __size32 ecx_4; 		// r25{2}
    __size32 ecx_5; 		// r25{2}
    __size32 edi; 		// r31
    int edx; 		// r26
    __size32 esi; 		// r30
    int esp; 		// r28
    __size32 *esp_1; 		// r28{3}
    void *esp_10; 		// r28{36}
    __size32 *esp_11; 		// r28{42}
    __size32 *esp_12; 		// r28{42}
    __size32 *esp_13; 		// r28{42}
    void *esp_14; 		// r28{0}
    void *esp_15; 		// r28{0}
    __size32 *esp_2; 		// r28{3}
    __size32 *esp_3; 		// r28{3}
    void *esp_4; 		// r28{8}
    void *esp_5; 		// r28{8}
    void *esp_6; 		// r28{8}
    __size32 *esp_7; 		// r28{11}
    void *esp_8; 		// r28{36}
    void *esp_9; 		// r28{36}
    __size32 local0; 		// m[esp + 4]
    int local1; 		// m[esp + 8]
    __size32 local10; 		// m[esp - 180]
    int local11; 		// m[esp - 184]
    int local12; 		// m[esp - 188]
    __size32 local13; 		// m[esp - 192]
    union { unsigned int; __size32 *; } local14; 		// m[esp - 196]
    unsigned int local15; 		// m[esp - 200]
    int local16; 		// m[esp_14 + 4]{8}
    int local17; 		// m[esp_14 + 4]{36}
    int local18; 		// m[esp_14 + 4]{42}
    int local19; 		// m[esp_14 + 8]{8}
    __size32 local2; 		// m[esp + 20]
    int local20; 		// m[esp_14 + 8]{36}
    int local21; 		// m[esp_14 + 8]{42}
    int local22; 		// m[esp_14 + 20]{8}
    int local23; 		// m[esp_14 + 20]{36}
    int local24; 		// m[esp_14 + 24]{8}
    int local25; 		// m[esp_14 + 24]{36}
    int local26; 		// m[esp_14 + 24]{42}
    unsigned int local27; 		// m[esp_4 - 8]{10}
    void *local28; 		// m[esp_4 - 8]{17}
    unsigned int local29; 		// m[esp_4 - 20]{20}
    __size32 local3; 		// m[esp + 24]
    void *local30; 		// m[esp_4 - 20]{33}
    int local31; 		// m[esp_14 - 156]{8}
    int local32; 		// m[esp_14 - 156]{36}
    int local33; 		// m[esp_14 - 160]{8}
    int local34; 		// m[esp_14 - 160]{36}
    int local35; 		// m[esp_14 - 164]{8}
    int local36; 		// m[esp_14 - 164]{36}
    int local37; 		// m[esp_14 - 168]{8}
    int local38; 		// m[esp_14 - 168]{36}
    int local39; 		// m[esp_14 - 172]{8}
    __size32 local4; 		// m[esp - 156]
    int local40; 		// m[esp_14 - 172]{36}
    int local41; 		// m[esp_14 - 176]{8}
    int local42; 		// m[esp_14 - 176]{36}
    int local43; 		// m[esp_14 - 180]{8}
    int local44; 		// m[esp_14 - 180]{36}
    int local45; 		// m[esp_14 - 184]{8}
    int local46; 		// m[esp_14 - 184]{36}
    int local47; 		// m[esp_14 - 188]{8}
    int local48; 		// m[esp_14 - 188]{36}
    int local49; 		// m[esp_14 - 192]{8}
    __size32 local5; 		// m[esp - 160]
    int local50; 		// m[esp_14 - 192]{36}
    int local51; 		// m[esp_14 - 196]{8}
    int local52; 		// m[esp_14 - 196]{36}
    int local53; 		// m[esp_14 - 200]{8}
    int local54; 		// m[esp_14 - 200]{36}
    __size32 *local55; 		// esp{71}
    __size32 local56; 		// ecx{94}
    __size32 *local57; 		// esp{95}
    __size32 local6; 		// m[esp - 164]
    __size32 local7; 		// m[esp - 168]
    int local8; 		// m[esp - 172]
    __size32 local9; 		// m[esp - 176]

    eax_1 = LoadImageA(); /* Warning: also results in ecx_1, edx, esp_1 */
    local57 = esp_1;
    local57 = esp_1;
    local56 = ecx_1;
    local56 = ecx_1;
    if (eax_1 == 0) {
bb0x401627:
        ecx = local56;
        esp = local57;
        ebp = *(esp + 8);
        eax = 0;
    }
    else {
        eax_4 = *(param2 + 40);
        if (eax_4 == 0) {
            goto bb0x401627;
        }
        else {
            ecx = *eax_4;
            (**(*eax_4 + 108))(eax_4, ecx, edx, param1, eax_1, param2, LOGICALFLAGS32(eax_4), eax_4 == 0, 0, eax_4 < 0, 0, edi, *(esp_14 + 4), *(esp_14 + 8), param1, *(esp_14 + 24), ebx, ebp, esi, edi, 0, *(esp_14 + 24), param1, 0, *(esp_14 + 8) & 0xffff, *(esp_14 + 4), eax_4, pc);
            *(__size32*)(esp_4 - 4) = 0;
            eax_6 = CreateCompatibleDC(); /* Warning: also results in ecx, esp_7 */
            local57 = esp_7;
            local56 = ecx;
            if (eax_6 == 0) {
                goto bb0x401627;
            }
            else {
                *(union { int; __size32 *; }*)(esp_4 - 8) = ebp;
                *(union { int; __size32 *; }*)(esp_4 - 12) = eax_6;
                SelectObject(*(esp_4 - 12), *(esp_4 - 8));
                local28 = esp_4 + 16;
                *(__size32*)(esp_4 - 12) = 24;
                *(union { int; __size32 *; }*)(esp_4 - 16) = ebp;
                GetObjectA();
                if (ebx == 0) {
                    ebx = *(esp_4 + 8);
                }
                eax_9 = *(esp_4 + 176);
                if (eax_9 == 0) {
                    eax_12 = *(esp_4 + 12);
                    *(int*)(esp_4 + 176) = eax_12;
                }
                eax_13 = *(esi + 40);
                *(__size32*)(esp_4 + 28) = 124;
                *(__size32*)(esp_4 + 32) = 6;
                ecx = *eax_13;
                local30 = esp_4 + 28;
                *(__size32 **)(esp_4 - 24) = eax_13;
                (**(*eax_13 + 88))(eax_13, ecx, esp_4 + 28, ebx, ebp, esi, eax_6, <all>, LOGICALFLAGS32(eax_9), eax_9 == 0, 0, eax_9 < 0, 0, local16, local19, local22, local24, local31, local33, local35, local37, local39, local41, local43, local45, local47, local49, local51, local53);
                eax = *(esi + 40);
                *(void **)(esp_8 - 4) = esp_8 + 16;
                *(__size32 **)(esp_8 - 8) = eax;
                ecx = *eax;
                (**(*eax + 68))(eax, ecx, esp_8 + 16, ebx, ebp, esi, edi, <all>, flags, ZF, CF, NF, OF, local17, local20, local23, local25, local32, local34, local36, local38, local40, local42, local44, local46, local48, local50, local52, local54);
                local55 = esp_11;
                ebp = *(esp_11 + 184);
                if (eax_15 == 0) {
                    eax_18 = *(esp_11 + 192);
                    ecx = *(esp_11 + 180);
                    edx = *(esp_11 + 52);
                    *(__size32*)(esp_11 - 4) = 0xcc0020;
                    *(__size32*)(esp_11 - 8) = eax_18;
                    eax_19 = *(esp_11 + 56);
                    *(__size32*)(esp_11 - 12) = ebx;
                    *(__size32*)(esp_11 - 16) = ebp;
                    *(__size32*)(esp_11 - 20) = ecx;
                    ecx = *(esp_11 + 16);
                    *(__size32*)(esp_11 - 24) = edi;
                    *(__size32*)(esp_11 - 28) = edx;
                    *(__size32*)(esp_11 - 32) = eax_19;
                    *(__size32*)(esp_11 - 36) = 0;
                    *(__size32*)(esp_11 - 40) = 0;
                    *(__size32*)(esp_11 - 44) = ecx;
                    StretchBlt();
                    eax_20 = *(esi + 40);
                    ecx = *(esp_11 - 28);
                    *(__size32*)(esp_11 - 48) = ecx;
                    *(__size32 **)(esp_11 - 52) = eax_20;
                    edx = *eax_20;
                    (**(*eax_20 + 104))(eax_20, ecx, edx, ebx, ebp, esi, edi, <all>, LOGICALFLAGS32(eax_15), eax_15 == 0, 0, eax_15 < 0, 0, local18, local21, *(esp_14 + 20), local26, *(esp_14 - 156), *(esp_14 - 160), *(esp_14 - 164), *(esp_14 - 168), *(esp_14 - 172), *(esp_14 - 176), *(esp_14 - 180), *(esp_14 - 184), *(esp_14 - 188), *(esp_14 - 192), *(esp_14 - 196), *(esp_14 - 200));
                    local55 = esp;
                }
                esp = local55;
                *(__size32*)(esp - 4) = edi;
                DeleteDC();
                edx = *(esp + 168);
                eax = *(esp + 172);
                ecx = *(esp + 176);
                *(__size32*)(esi + 4) = edx;
                edx = *(esp + 188);
                *(__size32*)(esi + 8) = eax;
                *(__size32*)(esi + 12) = ecx;
                *(__size32*)(esi + 16) = ebp;
                *(__size32*)(esi + 20) = ebx;
                *(__size32*)(esi + 24) = edx;
                ebp = *(esp + 4);
                eax = 1;
            }
        }
    }
    return eax; /* WARNING: Also returning: ecx := ecx, ebp := ebp */
}

/** address: 0x00401130 */
__size32 proc_0x00401130()
{
    unsigned int eax; 		// r24
    __size32 *eax_1; 		// r24
    __size32 *eax_10; 		// r24{36}
    __size32 *eax_11; 		// r24{36}
    unsigned int eax_2; 		// r24{31}
    unsigned int eax_3; 		// r24{31}
    unsigned int eax_4; 		// r24{31}
    unsigned int eax_5; 		// r24{1}
    unsigned int eax_6; 		// r24{1}
    unsigned int eax_7; 		// r24{1}
    unsigned int eax_8; 		// r24{2}
    unsigned int eax_9; 		// r24{2}
    __size32 ebp; 		// r29
    __size32 ebx; 		// r27
    __size32 ecx; 		// r25
    void * () *edi; 		// r31
    void (void) *edi_1; 		// r31
    __size32 edx; 		// r26
    __size32 esi; 		// r30
    void *esp; 		// r28
    void *esp_1; 		// r28{31}
    void *esp_10; 		// r28{16}
    void *esp_11; 		// r28{16}
    void *esp_12; 		// r28{44}
    void *esp_13; 		// r28{0}
    void *esp_14; 		// r28{0}
    void *esp_2; 		// r28{31}
    void *esp_3; 		// r28{31}
    __size32 *esp_4; 		// r28{1}
    __size32 *esp_5; 		// r28{1}
    __size32 *esp_6; 		// r28{1}
    void *esp_7; 		// r28{13}
    void *esp_8; 		// r28{40}
    void *esp_9; 		// r28{16}
    __size32 local0; 		// m[esp - 4]
    unsigned int local1; 		// m[esp - 8]
    int local10; 		// m[esp_13 - 8]{31}
    int local11; 		// m[esp_13 - 8]{31}
    int local12; 		// m[esp_13 - 8]{0}
    int local13; 		// m[esp_13 - 8]{0}
    void *local14; 		// esp_9{16}
    void *local15; 		// esp_12{44}
    unsigned int local16; 		// eax{67}
    __size32 *local17; 		// esp{70}
    int local2; 		// m[esp_13 - 4]{0}
    int local3; 		// m[esp_13 - 4]{0}
    int local4; 		// m[esp_13 - 4]{31}
    int local5; 		// m[esp_13 - 4]{31}
    int local6; 		// m[esp_13 - 4]{0}
    int local7; 		// m[esp_13 - 4]{0}
    int local8; 		// m[esp_13 - 8]{0}
    int local9; 		// m[esp_13 - 8]{0}

    (*GetTickCount)(GetTickCount, ecx, edx, ebx, ebp, esi, edi, pc);
    local17 = esp_4;
    eax_8 = eax_5 - global_0x00409548;
    local16 = eax_8;
    flags = SUBFLAGS32(eax_5 - global_0x00409548, 50, eax_5 - global_0x00409548 - 50);
    if ((unsigned int)(eax_5 - global_0x00409548) >= 50) {
        *(__size32*)(esp_4 - 4) = 140;
        *(__size32*)(esp_4 - 8) = 150;
        *(__size32*)(esp_4 - 12) = global_0x00409594;
        *(__size32*)(esp_4 - 16) = global_0x00409590;
        *(__size32*)(esp_4 - 20) = 170;
        *(__size32*)(esp_4 - 24) = 245;
        *(__size32*)(esp_4 - 28) = global_0x0040958c;
        edx = proc_0x00401730(*(esp_4 - 28), *(esp_4 - 24), *(esp_4 - 20), *(esp_4 - 8), *(esp_4 - 4), 0x409550); /* Warning: also results in ebx, esp_7, ebp, esi, edi_1 */
        local14 = esp_7;
        do {
            esp_9 = local14;
            eax_1 = *0x409588;
            *(__size32*)(esp_9 - 4) = 0;
            *(__size32*)(esp_9 - 8) = 0;
            *(__size32 **)(esp_9 - 12) = eax_1;
            ecx = *eax_1;
            (**(*eax_1 + 44))(eax_1, ecx, edx, ebx, ebp, esi, edi_1, <all>, flags, ZF, CF, NF, OF, local3, local9);
            local15 = esp_1;
            local15 = esp_1;
            local14 = esp_1;
            if (eax_2 == 0) {
                break;
            }
            if (eax_2 == 0x887601c2) {
                eax_10 = *0x409588;
                *(__size32 **)(esp_1 - 4) = eax_10;
                edx = *eax_10;
                (**(*eax_10 + 108))(eax_10, ecx, edx, ebx, ebp, esi, edi_1, <all>, SUBFLAGS32(eax_2, 0x887601c2, eax_2 + 0x7789fe3e), eax_2 == 0x887601c2, eax_2 < (unsigned int)0x887601c2, (int)eax_2 < 0x887601c2, (int)eax_2 >= 0 && (int)eax_2 < 0x887601c2, local4, local10);
                local15 = esp_8;
                goto bb0x4011a8;
            }
            flags = SUBFLAGS32(eax_2, 0x8876021c, eax_2 + 0x7789fde4);
        } while (eax_2 == 0x8876021c);
bb0x4011a8:
        esp_12 = local15;
        eax = global_0x00409590 + 150;
        flags = SUBFLAGS32(global_0x00409590 + 150, 1500, global_0x00409590 - 1350);
        global_0x00409590 += 150;
        if (global_0x00409590 + 150 >= 1500) {
            global_0x00409590 = 0;
            eax = global_0x00409594 + 140;
            flags = SUBFLAGS32(global_0x00409594 + 140, 280, global_0x00409594 - 140);
            global_0x00409594 += 140;
            if (global_0x00409594 + 140 >= 280) {
                global_0x00409594 = 0;
            }
        }
        (*edi_1)(eax, ecx, edx, ebx, ebp, esi, edi_1, <all>, flags, ZF, CF, NF, OF, local2, local8);
        local17 = esp;
        local16 = eax;
        global_0x00409548 = eax;
    }
    eax = local16;
    esp = local17;
    edi = *esp;
    return eax; /* WARNING: Also returning: ecx := ecx, ebx := ebx, ebp := ebp, esi := esi, edi := edi */
}

/** address: 0x004017d0 */
__size32 proc_0x004017d0(__size32 param1)
{
    union { int; __size32 *; } eax; 		// r24
    __size32 ecx; 		// r25
    __size32 esi; 		// r30
    void *esp; 		// r28
    __size32 local3; 		// param1{9}

    local3 = param1;
    esp--;
    eax = *(param1 + 40);
    if (eax != 0) {
        ecx = *eax;
        (**(*eax + 8))(eax, ecx, param1, LOGICALFLAGS32(eax), eax == 0, 0, eax < 0, 0, esi, eax, pc);
        local3 = ecx;
        *(__size32*)(esi + 40) = 0;
    }
    param1 = local3;
    esi = *esp;
    return param1; /* WARNING: Also returning: esi := esi */
}

/** address: 0x00401730 */
__size32 proc_0x00401730(__size32 *param1, __size32 param2, __size32 param3, __size32 param4, __size32 param5, __size32 param6)
{
    int eax; 		// r24
    int eax_1; 		// r24{20}
    __size32 eax_4; 		// r24{21}
    __size32 ebp; 		// r29
    __size32 ebx; 		// r27
    __size32 edi; 		// r31
    __size32 edx; 		// r26
    __size32 *esi; 		// r30
    __size32 *esp_1; 		// r28{8}
    void *esp_11; 		// r28{1}
    __size32 *esp_4; 		// r28{33}
    __size32 *esp_7; 		// r28{37}
    __size32 *esp_8; 		// r28{16}
    __size32 *local15; 		// esp_8{16}

    esp_1 = esp_11 - 32;
    local15 = esp_1;
    edi = param6;
    if (param4 != 0) {
    }
    if (param5 != 0) {
    }
    ebx = param3;
    ebp = param2;
    esi = param1;
    do {
bb0x401777:
        esp_8 = local15;
        eax_1 = *(edi + 28);
        eax_4 = *esi;
        if (eax_1 >= 0) {
            *(__size32*)(esp_8 - 4) = 1;
        }
        else {
            *(__size32*)(esp_8 - 4) = 0;
        }
        edx = *(edi + 40);
        *(void **)(esp_8 - 8) = esp_8 + 16;
        *(__size32*)(esp_8 - 12) = edx;
        *(__size32*)(esp_8 - 16) = ebx;
        *(__size32*)(esp_8 - 20) = ebp;
        *(__size32 **)(esp_8 - 24) = esi;
        (**(*esi + 28))(eax_4, esp_8 + 16, edx, ebx, ebp, esi, edi, LOGICALFLAGS32(eax_1), eax_1 == 0, 0, eax_1 < 0, 0, param1, param2, param3, *(esp_11 + 16), *(esp_11 + 20), param4, param5, *(esp_11 - 4), *(esp_11 - 8), *(esp_11 - 12), *(esp_11 - 16), *(esp_11 - 20), *(esp_11 - 24), *(esp_11 - 28), *(esp_11 - 32));
        local15 = esp_4;
        if (eax == 0) {
            break;
        }
        if (eax == 0x887601c2) {
            esp_7 = proc_0x004017f0(edi);
            local15 = esp_7;
            goto bb0x401777;
        }
    } while (eax == 0x8876021c);
    ebx = *(esp_4 + 12);
    ebp = *(esp_4 + 8);
    esi = *(esp_4 + 4);
    edi = *esp_4;
    return edx; /* WARNING: Also returning: ebx := ebx, ebp := ebp, esi := esi, edi := edi */
}

/** address: 0x004017f0 */
void proc_0x004017f0(__size32 param1)
{
    __size32 *eax; 		// r24
    __size32 ecx; 		// r25

    eax = *(param1 + 40);
    ecx = *eax;
    (**(*eax + 108))(eax, ecx, eax, pc);
    return;
}


