
int32_t lstrlenA = 0xd388;

int32_t lstrcatA = 0xd37c;

void fun_401000(void* a1, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13, void* a14, void* a15, ...) {
    int32_t esi16;
    int32_t edi17;
    int32_t esi18;
    void* eax19;
    void* eax20;

    esi16 = lstrlenA;
    eax19 = reinterpret_cast<void*>(esi16(a1, edi17, esi18, __return_address()));
    if (!(reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(eax19) < 0) | reinterpret_cast<uint1_t>(eax19 == 0)) && (*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(eax19) + reinterpret_cast<int32_t>(a1) - 1) != 92 && *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(eax19) + reinterpret_cast<int32_t>(a1) - 1) != 47)) {
        lstrcatA(a1, "\\", a1, edi17, esi18, __return_address());
    }
    lstrcatA();
    eax20 = reinterpret_cast<void*>(esi16());
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(eax20) + reinterpret_cast<int32_t>(a1) + 1) = 0;
    goto a1;
}

struct s0 {
    struct s0* f0;
    signed char[3] pad4;
    signed char f4;
    signed char[11] pad16;
    int32_t f10;
};

struct s0* g40ae30 = reinterpret_cast<struct s0*>(0);

void* fun_4060d0(struct s0* a1, int32_t a2, signed char* a3, void* a4, struct s0* a5, void* a6, struct s0* a7);

struct s0* fun_404f00(void* a1);

void fun_404eb0(struct s0* a1);

signed char* fun_406010(signed char* a1, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13, void* a14, void* a15, void* a16, void* a17) {
    struct s0* ebp18;
    int1_t zf19;
    signed char* ebx20;
    struct s0* eax21;
    void* eax22;
    struct s0* eax23;
    struct s0* eax24;
    void* eax25;
    struct s0* edi26;
    int32_t ecx27;
    void* eax28;
    uint32_t ecx29;
    uint32_t eax30;
    uint32_t ecx31;
    signed char* esi32;
    signed char* edi33;
    uint32_t ecx34;
    signed char* ebx35;
    signed char* eax36;

    ebp18 = reinterpret_cast<struct s0*>(0);
    zf19 = g40ae30 == 0;
    if (!zf19) {
        ebx20 = a1;
        eax21 = g40ae30;
        eax22 = fun_4060d0(eax21, 0x200, ebx20, 0xff, 0, 0, 0);
        if (eax22 && ((eax23 = fun_404f00(eax22), ebp18 = eax23, !!ebp18) && (eax24 = g40ae30, eax25 = fun_4060d0(eax24, 0x200, ebx20, 0xff, ebp18, eax22, 0), !!eax25))) {
            edi26 = ebp18;
            ecx27 = -1;
            eax28 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax25) - reinterpret_cast<uint32_t>(eax25));
            do {
                if (!ecx27) 
                    break;
                --ecx27;
                edi26 = reinterpret_cast<struct s0*>(&edi26->pad4);
            } while (*reinterpret_cast<struct s0**>(&edi26->f0) != *reinterpret_cast<struct s0**>(&eax28));
            ecx29 = reinterpret_cast<uint32_t>(~ecx27);
            eax30 = ecx29;
            ecx31 = ecx29 >> 2;
            esi32 = reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(edi26) - ecx29);
            edi33 = ebx20;
            while (ecx31) {
                --ecx31;
                *edi33 = *esi32;
                edi33 = edi33 + 4;
                esi32 = esi32 + 4;
            }
            ecx34 = eax30 & 3;
            while (ecx34) {
                --ecx34;
                *edi33 = *esi32;
                ++edi33;
                ++esi32;
            }
        }
        fun_404eb0(ebp18);
        return ebx20;
    } else {
        ebx35 = a1;
        eax36 = ebx35;
        if (*ebx35) {
            do {
                if (*eax36 >= 97 && *eax36 <= 0x7a) {
                    *eax36 = reinterpret_cast<signed char>(*eax36 - 32);
                }
                ++eax36;
            } while (*eax36);
        }
        return ebx35;
    }
}

struct s1 {
    unsigned char f0;
    unsigned char f1;
};

struct s2 {
    unsigned char f0;
    unsigned char f1;
    unsigned char f2;
    unsigned char f3;
};

struct s1* fun_4027a0(struct s1* a1, struct s2* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13, void* a14, void* a15, void* a16, void* a17, void* a18) {
    unsigned char dl19;
    struct s1* edi20;
    unsigned char dh21;
    uint32_t eax22;
    uint32_t ebx23;
    uint32_t eax24;
    struct s1* edx25;
    uint32_t ebx26;
    uint32_t ebx27;
    uint32_t ecx28;
    uint32_t esi29;
    uint32_t eax30;
    uint32_t eax31;
    uint32_t eax32;
    unsigned char al33;
    unsigned char* esi34;
    struct s2* ecx35;

    dl19 = a2->f0;
    edi20 = a1;
    if (!dl19) {
        return edi20;
    }
    dh21 = a2->f1;
    if (dh21) 
        goto addr_4027b8_4;
    eax22 = 0;
    *reinterpret_cast<unsigned char*>(&eax22) = dl19;
    ebx23 = eax22;
    eax24 = eax22 << 8;
    edx25 = a1;
    if (reinterpret_cast<uint32_t>(edx25) & 3) 
        goto addr_4036f8_7;
    addr_40370b_8:
    ebx26 = ebx23 | eax24;
    ebx27 = ebx26 << 16 | ebx26;
    while (1) {
        ecx28 = edx25->f0 ^ ebx27;
        esi29 = 0x7efefeff + edx25->f0;
        edx25 = edx25 + 2;
        if ((ecx28 ^ 0xffffffff ^ 0x7efefeff + ecx28) & 0x81010100) {
            eax30 = *reinterpret_cast<uint32_t*>(&(edx25 - 2)->f0);
            if (*reinterpret_cast<signed char*>(&eax30) == *reinterpret_cast<signed char*>(&ebx27)) 
                break;
            if (!*reinterpret_cast<signed char*>(&eax30)) 
                goto addr_403752_12;
            if (*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax30) + 1) == *reinterpret_cast<signed char*>(&ebx27)) 
                goto addr_40378e_14;
            if (!*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax30) + 1)) 
                goto addr_403752_12;
            eax31 = eax30 >> 16;
            if (*reinterpret_cast<signed char*>(&eax31) == *reinterpret_cast<signed char*>(&ebx27)) 
                goto addr_403787_17;
            if (!*reinterpret_cast<signed char*>(&eax31)) 
                goto addr_403752_12;
            if (*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax31) + 1) == *reinterpret_cast<signed char*>(&ebx27)) 
                goto addr_403780_20;
            if (!*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax31) + 1)) 
                goto addr_403752_12;
        } else {
            eax32 = (edx25->f0 ^ 0xffffffff ^ esi29) & 0x81010100;
            if (!eax32) 
                continue;
            if (eax32 & 0x1010100) 
                goto addr_403752_12;
            if (!(esi29 & 0x80000000)) 
                goto addr_403752_12;
        }
    }
    return edx25 - 2;
    addr_403752_12:
    addr_403754_27:
    return 0;
    addr_40378e_14:
    return reinterpret_cast<uint32_t>(edx25) - 3;
    addr_403787_17:
    return edx25 - 1;
    addr_403780_20:
    return reinterpret_cast<uint32_t>(edx25) - 1;
    do {
        addr_4036f8_7:
        edx25 = reinterpret_cast<struct s1*>(&edx25->f1);
        if (edx25->f0 == *reinterpret_cast<unsigned char*>(&ebx23)) 
            break;
        if (!edx25->f0) 
            goto addr_403754_27;
    } while (reinterpret_cast<uint32_t>(edx25) & 3);
    goto addr_40370b_8;
    return reinterpret_cast<uint32_t>(edx25) - 1;
    addr_4027d4_31:
    return 0;
    addr_402813_32:
    return reinterpret_cast<uint32_t>(edi20) - 1;
    while (1) {
        addr_4027cc_33:
        if (al33 == dl19) {
            do {
                al33 = *esi34;
                ++esi34;
                if (al33 != dh21) 
                    goto addr_4027cc_33;
                edi20 = reinterpret_cast<struct s1*>(esi34 - 1);
                do {
                    if (!ecx35->f2) 
                        goto addr_402813_32;
                    esi34 = esi34 + 2;
                    if (*esi34 != ecx35->f2) 
                        goto addr_4027b8_4;
                    if (!ecx35->f3) 
                        goto addr_402813_32;
                    ecx35 = reinterpret_cast<struct s2*>(&ecx35->f2);
                } while (ecx35->f3 == *(esi34 - 1));
                addr_4027b8_4:
                ecx35 = a2;
                esi34 = &edi20->f1;
            } while (edi20->f0 == dl19);
            if (!edi20->f0) 
                goto addr_4027d4_31;
        } else {
            if (!al33) 
                goto addr_4027d4_31;
        }
        al33 = *esi34;
        ++esi34;
    }
}

int32_t GetDlgItem = 0xd462;

int32_t EnableWindow = 0xd4c8;

void fun_4022c0(void* a1, int32_t a2, int32_t a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13, void* a14, void* a15, void* a16, void* a17, void* a18, void* a19, int32_t a20, void* a21, void* a22, int32_t a23, void* a24, void* a25, void* a26, void* a27, void* a28) {
    int32_t eax29;

    eax29 = reinterpret_cast<int32_t>(GetDlgItem(a1, a2, a3, __return_address()));
    EnableWindow();
    goto eax29;
}

int32_t wvsprintfA = 0xd482;

int32_t SetDlgItemTextA = 0xd470;

uint32_t fun_4010b0(void* a1, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13, void* a14, void* a15, void* a16, void* a17, void* a18, void* a19, void* a20, void* a21, void* a22, void* a23, ...) {
    void* esp24;
    void* v25;
    int32_t v26;
    int32_t v27;

    esp24 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x80);
    v25 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + 12);
    wvsprintfA(esp24, a2, v25);
    SetDlgItemTextA(v26, 0x3f1, reinterpret_cast<int32_t>(esp24) - 4 - 4 - 4 - 4 + 4, esp24, a2, v25);
    goto v27;
}

struct s3 {
    signed char f0;
    signed char f1;
};

uint32_t fun_4026e0(struct s3* a1);

uint32_t fun_402790(struct s3* a1) {
    uint32_t eax2;

    eax2 = fun_4026e0(a1);
    return eax2;
}

void* fun_4026a0(void* a1, void* a2, uint32_t a3, uint32_t a4, void* a5, void* a6, void* a7, void* a8, void* a9) {
    if (a4 | reinterpret_cast<uint32_t>(a2)) {
        return reinterpret_cast<uint32_t>(a1) * a3;
    } else {
        return reinterpret_cast<uint32_t>(a1) * a3;
    }
}

void* fun_4025f0(void* a1, void* a2, uint32_t a3, uint32_t a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13) {
    int32_t edi14;
    void* eax15;
    uint32_t eax16;
    uint32_t eax17;
    uint32_t ebx18;
    uint32_t ecx19;
    void* eax20;
    void* eax21;
    void* esi22;
    void* tmp32_23;
    void* eax24;

    edi14 = 0;
    if (__intrinsic()) {
        edi14 = 1;
        eax15 = reinterpret_cast<void*>(-reinterpret_cast<uint32_t>(a2));
        a2 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax15) - reinterpret_cast<uint1_t>(reinterpret_cast<uint32_t>(eax15) < static_cast<uint32_t>(reinterpret_cast<uint1_t>(!!a1))));
        a1 = reinterpret_cast<void*>(-reinterpret_cast<uint32_t>(a1));
    }
    eax16 = a4;
    if (__intrinsic()) {
        ++edi14;
        eax17 = -eax16;
        eax16 = eax17 - reinterpret_cast<uint1_t>(eax17 < static_cast<uint32_t>(reinterpret_cast<uint1_t>(!!a3)));
        a4 = eax16;
        a3 = -a3;
    }
    if (eax16) {
        ebx18 = eax16;
        ecx19 = a3;
        eax20 = a1;
        do {
            ebx18 = ebx18 >> 1;
            __asm__("rcr ecx, 1");
            __asm__("rcr eax, 1");
        } while (ebx18);
        eax21 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax20) / ecx19);
        esi22 = eax21;
        tmp32_23 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(__intrinsic()) + reinterpret_cast<uint32_t>(eax21) * a4);
        if (reinterpret_cast<uint32_t>(tmp32_23) < reinterpret_cast<uint32_t>(__intrinsic())) 
            goto addr_402685_9;
        if (reinterpret_cast<uint32_t>(tmp32_23) > reinterpret_cast<uint32_t>(a2)) 
            goto addr_402685_9;
        if (reinterpret_cast<uint32_t>(tmp32_23) < reinterpret_cast<uint32_t>(a2)) 
            goto addr_402686_12;
        if (a3 * reinterpret_cast<uint32_t>(esi22) <= reinterpret_cast<uint32_t>(a1)) 
            goto addr_402686_12;
    } else {
        eax24 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(a1) / a3);
        goto addr_40268a_15;
    }
    addr_402685_9:
    esi22 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi22) - 1);
    addr_402686_12:
    eax24 = esi22;
    addr_40268a_15:
    if (!(edi14 - 1)) {
        eax24 = reinterpret_cast<void*>(-reinterpret_cast<uint32_t>(eax24));
    }
    return eax24;
}

struct s4 {
    signed char f0;
    signed char f1;
};

struct s5 {
    struct s0* f0;
    signed char[3] pad4;
    struct s5* f4;
    signed char[2] pad8;
    struct s0* f8;
    signed char[3] pad12;
    uint32_t fc;
    uint32_t f10;
    struct s0* f14;
    signed char[3] pad24;
    void* f18;
};

int32_t fun_402b70(struct s5* a1, struct s4* a2, void* a3);

uint32_t fun_402a20(struct s5* a1, struct s5* a2);

int32_t fun_402580(signed char* a1, struct s4* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12) {
    void* esp13;
    void** esp14;
    int32_t eax15;

    esp13 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 32);
    esp14 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp13) - 4 - 4);
    eax15 = fun_402b70(esp14 + 2, a2, reinterpret_cast<int32_t>(esp13) + 44);
    if (0) {
        fun_402a20(0, esp14 - 1 - 1 - 1 + 1 + 3 + 1);
        return eax15;
    } else {
        *a1 = 0;
        return eax15;
    }
}

int32_t fun_4024b0(int32_t a1, int32_t a2, int32_t a3);

int32_t fun_402470(int32_t a1, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11) {
    int32_t eax12;

    eax12 = fun_4024b0(a1, 0, 0);
    return eax12;
}

int32_t CoCreateInstance = 0xd692;

struct s7 {
    signed char[12] pad12;
    int32_t fc;
};

struct s6 {
    struct s6* f0;
    int32_t f4;
    int32_t f8;
    struct s7* fc;
    signed char[64] pad80;
    int32_t f50;
};

uint40_t g0;

struct s8 {
    signed char[28] pad28;
    int32_t f1c;
};

struct s9 {
    signed char[36] pad36;
    int32_t f24;
};

int32_t MultiByteToWideChar = 0xd420;

struct s10 {
    signed char[24] pad24;
    int32_t f18;
};

struct s11 {
    signed char[8] pad8;
    int32_t f8;
};

struct s12 {
    signed char[8] pad8;
    int32_t f8;
};

struct s12* g407360 = reinterpret_cast<struct s12*>(0x10b);

void fun_402360(void* a1, void* a2, int32_t a3, int32_t a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11, void* a12, void* a13, void* a14, void* a15, void* a16, void* a17, void* a18, void* a19, void* a20, void* a21, void* a22, void* a23, void* a24, void* a25, void* a26, void* a27, void* a28, void* a29, void* a30, void* a31, void* a32, void* a33, void* a34, void* a35, void* a36, void* a37, void* a38, void* a39, void* a40, void* a41, void* a42, void* a43, void* a44, void* a45, void* a46, void* a47, void* a48, void* a49, void* a50, void* a51, void* a52, void* a53, void* a54, void* a55, void* a56, void* a57, void* a58, void* a59, void* a60, void* a61, void* a62, void* a63, void* a64, void* a65, void* a66, void* a67, void* a68, void* a69, ...) {
    int32_t eax70;
    struct s6* eax71;
    struct s8* eax72;
    struct s8** v73;
    struct s9* eax74;
    struct s9** v75;
    int32_t* eax76;
    int32_t** v77;
    int32_t eax78;
    void* v79;
    struct s10* esi80;
    struct s10** v81;
    int32_t eax82;
    int32_t v83;
    struct s11* eax84;
    struct s12* eax85;

    eax70 = reinterpret_cast<int32_t>(CoCreateInstance());
    if (eax70 >= 0) {
        eax71 = *reinterpret_cast<struct s6**>(&g0);
        eax71->f50(0);
        eax72 = *v73;
        eax72->f1c(v73);
        eax74 = *v75;
        eax74->f24(v75);
        eax76 = *v77;
        eax78 = reinterpret_cast<int32_t>(*eax76(v77));
        if (eax78 >= 0) {
            MultiByteToWideChar(0, 0);
            v79 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x210 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 4 + 12);
            esi80 = *v81;
            eax82 = reinterpret_cast<int32_t>(esi80->f18(v81, v79));
            if (eax82 < 0) {
                addr_402424_4:
                goto v83;
            } else {
                eax84 = *reinterpret_cast<struct s11**>(reinterpret_cast<int32_t>(&g0) + 1);
                eax84->f8(1, v81, v79);
            }
        }
        eax85 = g407360;
        eax85->f8(0x407360, v77);
    }
    goto addr_402424_4;
}

int32_t g40ba10;

int32_t LoadImageA = 0xd454;

int32_t SendMessageA = 0xd444;

int32_t DeleteObject = 0xd57c;

void fun_401050(void* a1, int32_t a2, void* a3, int32_t a4, int32_t a5, int32_t a6, int32_t a7, int32_t a8, void* a9, int32_t a10, int32_t a11, void* a12, int32_t a13, void* a14, int32_t a15, void* a16, int32_t a17, void* a18, void* a19, int32_t a20, void* a21, int32_t a22, void* a23, int32_t a24, int32_t a25, int32_t a26, void* a27, void* a28, int32_t a29, void* a30, void* a31, void* a32, void* a33, void* a34, void* a35, int32_t a36, int32_t a37, int32_t a38, void* a39, int32_t a40, int32_t a41, void* a42, int32_t a43, int32_t a44, void* a45, int32_t a46, void* a47, int32_t a48, int32_t a49, int32_t a50, void* a51, int32_t a52, int32_t a53, void* a54, int32_t a55, int32_t a56, int32_t a57, void* a58, int32_t a59, int32_t a60, int32_t a61, void* a62, int32_t a63, int32_t a64, int32_t a65, void* a66, int32_t a67, int32_t a68, int32_t a69, void* a70, int32_t a71, int32_t a72, int32_t a73, void* a74, int32_t a75, int32_t a76, void* a77, void* a78, void* a79, void* a80, void* a81, void* a82, void* a83, void* a84, void* a85, void* a86, void* a87, void* a88, void* a89, void* a90, void* a91, void* a92, void* a93, void* a94, void* a95, void* a96, void* a97, void* a98, void* a99, void* a100, void* a101, void* a102, void* a103, void* a104, void* a105, void* a106, void* a107, void* a108, void* a109, void* a110, void* a111, void* a112, void* a113, void* a114, void* a115, void* a116, void* a117, void* a118, void* a119, void* a120, void* a121, void* a122, void* a123, void* a124) {
    int32_t eax125;
    int32_t eax126;
    int32_t eax127;
    int32_t eax128;

    eax125 = reinterpret_cast<int32_t>(GetDlgItem());
    if (eax125 && ((eax126 = g40ba10, eax127 = reinterpret_cast<int32_t>(LoadImageA(eax126, a1, 0, a2, a3, 0x3020)), !!eax127) && (eax128 = reinterpret_cast<int32_t>(SendMessageA(eax125, 0x172, 0, eax127, eax126, a1, 0, a2, a3, 0x3020)), !!eax128))) {
        DeleteObject(eax128, eax125, 0x172, 0, eax127, eax126, a1, 0, a2, a3, 0x3020);
    }
    goto a2;
}

int32_t SHGetMalloc = 0xd5e4;

struct s13 {
    signed char[8] pad8;
    int32_t f8;
    signed char[8] pad20;
    int32_t f14;
};

void fun_4010f0(int32_t a1, int32_t a2, int32_t a3, void* a4, int32_t a5, void* a6, void* a7) {
    struct s13* v8;
    struct s13* v9;
    int32_t v10;

    SHGetMalloc();
    if (reinterpret_cast<int32_t>(__zero_stack_offset()) - 4) {
        v8->f14();
        v9->f8(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    }
    goto v10;
}

int32_t fun_402560(int32_t* a1, int32_t* a2) {
    int32_t* edi3;
    int32_t* esi4;
    int32_t eax5;

    edi3 = a2;
    esi4 = a1;
    if (reinterpret_cast<uint32_t>(edi3) > reinterpret_cast<uint32_t>(esi4)) {
        do {
            eax5 = *esi4;
            if (eax5) {
                eax5 = reinterpret_cast<int32_t>(eax5());
            }
            ++esi4;
        } while (reinterpret_cast<uint32_t>(edi3) > reinterpret_cast<uint32_t>(esi4));
    }
    return eax5;
}

/* (image base) */
int16_t* image_base_ = reinterpret_cast<int16_t*>(0x40a5fa);

int32_t fun_404d80(int32_t a1, void* a2, int32_t a3, void* a4, struct s0* a5, int32_t a6);

uint32_t fun_403630(int32_t a1, uint32_t a2) {
    int32_t ecx3;
    void* esp4;
    int32_t ebx5;
    int16_t* eax6;
    int32_t eax7;
    int32_t eax8;
    int16_t* edx9;
    uint32_t eax10;

    ecx3 = a1;
    esp4 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 8 - 4);
    if (ecx3 + 1 > 0x100) {
        ebx5 = 0;
        *reinterpret_cast<signed char*>(&ebx5) = *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&ecx3) + 1);
        eax6 = image_base_;
        if (!(*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(eax6 + ebx5) + 1) & 0x80)) {
            eax7 = 1;
        } else {
            eax7 = 2;
        }
        eax8 = fun_404d80(1, reinterpret_cast<int32_t>(esp4) - 4 - 4 - 4 - 4 + 24, eax7, reinterpret_cast<int32_t>(esp4) + 6, 0, 0);
        if (eax8) {
            return 0;
        } else {
            return 0;
        }
    } else {
        edx9 = image_base_;
        eax10 = 0;
        *reinterpret_cast<int16_t*>(&eax10) = edx9[ecx3];
        return eax10 & a2;
    }
}

int32_t g40a5dc = 0;

void fun_404550();

void fun_404590(int32_t a1);

/* (image base) */
int32_t image_base_ = 0x402490;

struct s0* fun_4029f0(int32_t a1) {
    int1_t zf2;
    struct s0* eax3;

    zf2 = g40a5dc == 1;
    if (zf2) {
        fun_404550();
    }
    fun_404590(a1);
    eax3 = reinterpret_cast<struct s0*>(image_base_(0xff));
    return eax3;
}

struct s0* g40ce60;

uint32_t g40cf60;

int32_t GetStartupInfoA = 0xd702;

int32_t GetStdHandle = 0xd81a;

struct s14 {
    uint32_t f0;
    unsigned char f4;
};

struct s15 {
    struct s0* f0;
    signed char[3] pad4;
    unsigned char f4;
};

int32_t GetFileType = 0xd80c;

int32_t SetHandleCount = 0xd7fa;

struct s16 {
    int32_t f0;
    unsigned char f4;
};

void fun_404250() {
    struct s0* eax1;
    struct s0* esi2;
    struct s0* eax3;
    int16_t v4;
    int32_t v5;
    int32_t esi6;
    int32_t ebx7;
    uint32_t esi8;
    struct s14* v9;
    unsigned char* edi10;
    int32_t* ebx11;
    int1_t less12;
    struct s15* tmp32_13;
    uint32_t eax14;
    struct s0* eax15;
    uint32_t eax16;
    uint32_t eax17;
    int32_t v18;
    uint32_t ebp19;
    int32_t v20;
    int32_t eax21;
    struct s16* ecx22;
    struct s0** ebp23;
    struct s0* eax24;
    uint32_t tmp32_25;
    int1_t less26;

    eax1 = fun_404f00(0x100);
    esi2 = eax1;
    if (!esi2) {
        fun_4029f0(27);
    }
    g40ce60 = esi2;
    g40cf60 = 32;
    if (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(esi2) + 0x100) > reinterpret_cast<unsigned char>(esi2)) {
        do {
            esi2->f4 = 0;
            esi2 = reinterpret_cast<struct s0*>(reinterpret_cast<unsigned char>(esi2) + 8);
            *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(esi2) + 0xfffffff8) = -1;
            *reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(esi2) + 0xfffffffd) = 10;
            eax3 = g40ce60;
        } while (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(eax3) + 0x100) > reinterpret_cast<unsigned char>(esi2));
    }
    GetStartupInfoA();
    if (!v4 || !v5) {
        addr_40439d_7:
        esi6 = 0;
        ebx7 = GetStdHandle;
    } else {
        esi8 = v9->f0;
        edi10 = &v9->f4;
        ebx11 = reinterpret_cast<int32_t*>(esi8 + reinterpret_cast<uint32_t>(edi10));
        if (reinterpret_cast<int32_t>(esi8) >= reinterpret_cast<int32_t>(0x800)) {
            esi8 = 0x800;
        }
        less12 = reinterpret_cast<int32_t>(g40cf60) < reinterpret_cast<int32_t>(esi8);
        if (!less12) 
            goto addr_404353_11; else 
            goto addr_4042f6_12;
    }
    do {
        tmp32_13 = reinterpret_cast<struct s15*>(esi6 * 8 + reinterpret_cast<unsigned char>(g40ce60));
        if (!reinterpret_cast<int1_t>(tmp32_13->f0 == 0xffffffff)) {
            tmp32_13->f4 = reinterpret_cast<unsigned char>(tmp32_13->f4 | 0x80);
        } else {
            eax14 = 0xfffffff6;
            tmp32_13->f4 = 0x81;
            if (esi6) {
                eax14 = 12 - reinterpret_cast<uint1_t>(esi6 + 0xffffffff < 1);
            }
            eax15 = reinterpret_cast<struct s0*>(ebx7(eax14));
            if (eax15 == 0xffffffff || (eax16 = reinterpret_cast<uint32_t>(GetFileType(eax15, eax14)), eax16 == 0)) {
                tmp32_13->f4 = reinterpret_cast<unsigned char>(tmp32_13->f4 | 64);
            } else {
                eax17 = eax16 & 0xff;
                tmp32_13->f0 = eax15;
                if (eax17 != 2) {
                    if (eax17 == 3) {
                        tmp32_13->f4 = reinterpret_cast<unsigned char>(tmp32_13->f4 | 8);
                    }
                } else {
                    tmp32_13->f4 = reinterpret_cast<unsigned char>(tmp32_13->f4 | 64);
                }
            }
        }
        ++esi6;
    } while (esi6 < 3);
    SetHandleCount();
    goto v18;
    addr_404353_11:
    ebp19 = 0;
    if (!(reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(esi8) < reinterpret_cast<int32_t>(0)) | reinterpret_cast<uint1_t>(esi8 == 0))) {
        do {
            if (*ebx11 != -1 && (*edi10 & 1 && (*edi10 & 8 || (v20 = *ebx11, eax21 = reinterpret_cast<int32_t>(GetFileType(v20)), !!eax21)))) {
                ecx22 = reinterpret_cast<struct s16*>(*reinterpret_cast<int32_t**>((reinterpret_cast<int32_t>(ebp19 & 0xffffffe7) >> 3) + 0x40ce60) + (ebp19 & 31) * 2);
                ecx22->f0 = *ebx11;
                ecx22->f4 = *edi10;
            }
            ++ebp19;
            ++edi10;
            ++ebx11;
        } while (reinterpret_cast<int32_t>(ebp19) < reinterpret_cast<int32_t>(esi8));
        goto addr_40439d_7;
    }
    addr_4042f6_12:
    ebp23 = reinterpret_cast<struct s0**>(0x40ce64);
    do {
        eax24 = fun_404f00(0x100);
        if (!eax24) 
            break;
        *ebp23 = eax24;
        tmp32_25 = g40cf60 + 32;
        g40cf60 = tmp32_25;
        if (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(eax24) + 0x100) > reinterpret_cast<unsigned char>(eax24)) {
            do {
                eax24->f4 = 0;
                eax24 = reinterpret_cast<struct s0*>(reinterpret_cast<unsigned char>(eax24) + 8);
                *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(eax24) + 0xfffffff8) = -1;
                *reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(eax24) + 0xfffffffd) = 10;
            } while (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*ebp23) + 0x100) > reinterpret_cast<unsigned char>(eax24));
        }
        ebp23 = ebp23 + 4;
        less26 = reinterpret_cast<int32_t>(g40cf60) < reinterpret_cast<int32_t>(esi8);
    } while (less26);
    goto addr_40434b_33;
    esi8 = g40cf60;
    goto addr_404353_11;
    addr_40434b_33:
    goto addr_404353_11;
}

int32_t fun_403f80(struct s0* a1);

int32_t fun_404240() {
    int32_t eax1;

    eax1 = fun_403f80(0xfd);
    return eax1;
}

int32_t GetEnvironmentStringsW = 0xd7a8;

struct s0** g40a8b0 = reinterpret_cast<struct s0**>(0);

int32_t GetEnvironmentStrings = 0xd776;

int32_t FreeEnvironmentStringsA = 0xd75c;

int32_t WideCharToMultiByte = 0xd7c2;

int32_t FreeEnvironmentStringsW = 0xd78e;

struct s0* fun_403df0() {
    struct s0** ebx1;
    struct s0** edi2;
    int32_t esi3;
    int1_t zf4;
    struct s0** eax5;
    struct s0** eax6;
    int1_t zf7;
    int1_t zf8;
    struct s0** eax9;
    struct s0** ebp10;
    void* ebp11;
    struct s0* eax12;
    struct s0* v13;
    struct s0* edi14;
    struct s0** esi15;
    uint32_t ecx16;
    uint32_t ecx17;
    struct s0** eax18;
    struct s0** esi19;
    void* eax20;
    struct s0* eax21;
    int32_t eax22;

    ebx1 = reinterpret_cast<struct s0**>(0);
    edi2 = reinterpret_cast<struct s0**>(0);
    esi3 = GetEnvironmentStringsW;
    zf4 = g40a8b0 == 0;
    if (zf4) {
        eax5 = reinterpret_cast<struct s0**>(esi3());
        edi2 = eax5;
        if (!eax5) {
            eax6 = reinterpret_cast<struct s0**>(GetEnvironmentStrings());
            ebx1 = eax6;
            if (!ebx1) {
                return 0;
            } else {
                g40a8b0 = reinterpret_cast<struct s0**>(2);
            }
        } else {
            g40a8b0 = reinterpret_cast<struct s0**>(1);
        }
    }
    zf7 = reinterpret_cast<int1_t>(g40a8b0 == 1);
    if (!zf7) {
        zf8 = reinterpret_cast<int1_t>(g40a8b0 == 2);
        if (!zf8) {
            return 0;
        } else {
            if (ebx1 || (eax9 = reinterpret_cast<struct s0**>(GetEnvironmentStrings()), ebx1 = eax9, !!ebx1)) {
                ebp10 = ebx1;
                if (*ebx1) {
                    addr_403f18_12:
                    ++ebp10;
                    if (*ebp10) 
                        goto addr_403f18_12;
                    ++ebp10;
                    if (*ebp10) 
                        goto addr_403f18_12;
                }
                ebp11 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebp10) - reinterpret_cast<uint32_t>(ebx1) + 1);
                eax12 = fun_404f00(ebp11);
                v13 = eax12;
                if (eax12) {
                    edi14 = v13;
                    esi15 = ebx1;
                    ecx16 = reinterpret_cast<uint32_t>(ebp11) >> 2;
                    while (ecx16) {
                        --ecx16;
                        *reinterpret_cast<struct s0**>(&edi14->f0) = *esi15;
                        edi14 = reinterpret_cast<struct s0*>(&edi14->f4);
                        esi15 = esi15 + 4;
                    }
                    ecx17 = reinterpret_cast<uint32_t>(ebp11) & 3;
                    while (ecx17) {
                        --ecx17;
                        *reinterpret_cast<struct s0**>(&edi14->f0) = *esi15;
                        edi14 = reinterpret_cast<struct s0*>(&edi14->pad4);
                        ++esi15;
                    }
                    FreeEnvironmentStringsA();
                    goto v13;
                } else {
                    FreeEnvironmentStringsA();
                    goto v13;
                }
            } else {
                return 0;
            }
        }
    } else {
        if (edi2 || (eax18 = reinterpret_cast<struct s0**>(esi3()), edi2 = eax18, !!edi2)) {
            esi19 = edi2;
            if (*edi2) {
                addr_403e6a_26:
                esi19 = esi19 + 2;
                if (*esi19) 
                    goto addr_403e6a_26;
                esi19 = esi19 + 2;
                if (*esi19) 
                    goto addr_403e6a_26;
            }
            eax20 = reinterpret_cast<void*>(WideCharToMultiByte());
            if (!eax20 || (eax21 = fun_404f00(eax20), eax21 == 0)) {
                FreeEnvironmentStringsW();
                goto 0;
            } else {
                eax22 = reinterpret_cast<int32_t>(WideCharToMultiByte());
                if (!eax22) {
                    fun_404eb0(eax21);
                }
                FreeEnvironmentStringsW();
                goto eax21;
            }
        } else {
            return 0;
        }
    }
}

int32_t fun_403a40(signed char a1, uint32_t a2, uint32_t a3);

int32_t fun_403a20(int32_t a1) {
    int32_t v2;
    int32_t eax3;

    v2 = a1;
    eax3 = fun_403a40(*reinterpret_cast<signed char*>(&v2), 0, 4);
    return eax3;
}

int32_t g40a5e0 = 2;

int32_t g40ab70 = 0;

void fun_404550() {
    int32_t eax1;
    int1_t zf2;
    int32_t eax3;

    eax1 = g40a5dc;
    if (eax1 == 1 || !eax1 && (zf2 = g40a5e0 == 1, zf2)) {
        fun_404590(0xfc);
        eax3 = g40ab70;
        if (eax3) {
            eax3();
        }
        fun_404590(0xff);
    }
    return;
}

int32_t WriteFile = 0xd854;

int32_t GetModuleFileNameA = 0xd40a;

unsigned char* fun_4057b0(unsigned char* a1, unsigned char* a2, uint32_t a3);

void fun_405710(void* a1, int32_t a2, int32_t a3);

void fun_404590(int32_t a1) {
    int32_t ecx2;
    int32_t* eax3;
    int32_t edx4;
    void* esp5;
    int32_t ebx6;
    int1_t zf7;
    int1_t zf8;
    int1_t zf9;
    int1_t zf10;
    struct s0* eax11;
    int32_t esi12;
    int32_t v13;
    int32_t eax14;
    signed char* edx15;
    void* eax16;
    signed char* edi17;
    void* v18;
    int32_t ecx19;
    int32_t eax20;
    int32_t ebp21;
    void* eax22;
    void* esp23;
    int32_t ecx24;
    unsigned char* ebp25;
    int32_t ecx26;
    unsigned char* eax27;
    signed char v28;
    int32_t ecx29;
    void* eax30;
    void* esp31;
    signed char v32;
    int32_t ecx33;
    unsigned char* edi34;
    int32_t ecx35;
    void* eax36;
    uint32_t edx37;
    int32_t ecx38;
    void* eax39;
    signed char v40;
    uint32_t ecx41;
    uint32_t ecx42;
    signed char* edi43;
    int32_t ecx44;
    void* eax45;
    uint32_t edx46;
    int32_t ecx47;
    void* eax48;
    signed char v49;
    uint32_t ecx50;
    uint32_t ecx51;
    signed char* edi52;
    int32_t ecx53;
    void* eax54;
    uint32_t edx55;
    int32_t ecx56;
    void* eax57;
    signed char v58;
    uint32_t ecx59;
    uint32_t ecx60;
    void* esp61;
    int32_t v62;

    ecx2 = 0;
    eax3 = reinterpret_cast<int32_t*>(0x40aae0);
    edx4 = a1;
    esp5 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x1a8 - 4 - 4 - 4 - 4);
    do {
        if (*eax3 == edx4) 
            break;
        eax3 = eax3 + 2;
        ++ecx2;
    } while (reinterpret_cast<uint32_t>(eax3) < 0x40ab70);
    ebx6 = ecx2 * 8;
    if (*reinterpret_cast<int32_t*>(ecx2 * 8 + 0x40aae0) == edx4) {
        zf7 = g40a5dc == 1;
        if (zf7 || (zf8 = g40a5dc == 0, zf8) && (zf9 = g40a5e0 == 1, zf9)) {
            zf10 = g40ce60 == 0;
            if (zf10 || (eax11 = g40ce60, esi12 = eax11->f10, esi12 == -1)) {
                v13 = 0xf4;
                eax14 = reinterpret_cast<int32_t>(GetStdHandle(0xf4));
                esp5 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp5) - 4 - 4 + 4);
                esi12 = eax14;
            }
            edx15 = *reinterpret_cast<signed char**>(ebx6 + reinterpret_cast<int32_t>("8w@"));
            v13 = 0;
            eax16 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp5) - 4 + 20);
            edi17 = edx15;
            v18 = eax16;
            ecx19 = -1;
            eax20 = reinterpret_cast<int32_t>(eax16) - reinterpret_cast<int32_t>(eax16);
            do {
                if (!ecx19) 
                    break;
                --ecx19;
                ++edi17;
                ++esi12;
            } while (*edi17 != *reinterpret_cast<signed char*>(&eax20));
            WriteFile(esi12, edx15, ~ecx19 - 1, v18, v13);
        } else {
            if (edx4 != 0xfc) {
                ebp21 = GetModuleFileNameA;
                eax22 = reinterpret_cast<void*>(ebp21());
                esp23 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp5) - 4 - 4 - 4 - 4 + 4);
                if (!eax22) {
                    ecx24 = 5;
                    while (ecx24) {
                        --ecx24;
                    }
                }
                ebp25 = reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(esp23) + 0xb4);
                ecx26 = -1;
                eax27 = reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(eax22) - reinterpret_cast<int32_t>(eax22));
                do {
                    if (!ecx26) 
                        break;
                    --ecx26;
                } while (v28 != *reinterpret_cast<signed char*>(&eax27));
                if (reinterpret_cast<uint32_t>(~ecx26) > 60) {
                    ecx29 = -1;
                    eax30 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax27) - reinterpret_cast<uint32_t>(eax27));
                    esp31 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp23) - 4);
                    do {
                        if (!ecx29) 
                            break;
                        --ecx29;
                    } while (v32 != *reinterpret_cast<signed char*>(&eax30));
                    ebp25 = reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(esp31) + ~ecx29 + 0x7c);
                    eax27 = fun_4057b0(ebp25, "...", 3);
                    esp23 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp31) - 4 - 4 - 4 + 4 + 12);
                }
                ecx33 = 6;
                while (ecx33) {
                    --ecx33;
                }
                edi34 = ebp25;
                ecx35 = -1;
                eax36 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax27) - reinterpret_cast<uint32_t>(eax27));
                do {
                    if (!ecx35) 
                        break;
                    --ecx35;
                    ++edi34;
                } while (*edi34 != *reinterpret_cast<unsigned char*>(&eax36));
                edx37 = reinterpret_cast<uint32_t>(~ecx35);
                ecx38 = -1;
                eax39 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax36) - reinterpret_cast<uint32_t>(eax36));
                do {
                    if (!ecx38) 
                        break;
                    --ecx38;
                } while (v40 != *reinterpret_cast<signed char*>(&eax39));
                ecx41 = edx37 >> 2;
                while (ecx41) {
                    --ecx41;
                }
                ecx42 = edx37 & 3;
                while (ecx42) {
                    --ecx42;
                }
                edi43 = "\n\n";
                ecx44 = -1;
                eax45 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax39) - reinterpret_cast<uint32_t>(eax39));
                do {
                    if (!ecx44) 
                        break;
                    --ecx44;
                    ++edi43;
                } while (*edi43 != *reinterpret_cast<signed char*>(&eax45));
                edx46 = reinterpret_cast<uint32_t>(~ecx44);
                ecx47 = -1;
                eax48 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax45) - reinterpret_cast<uint32_t>(eax45));
                do {
                    if (!ecx47) 
                        break;
                    --ecx47;
                } while (v49 != *reinterpret_cast<signed char*>(&eax48));
                ecx50 = edx46 >> 2;
                while (ecx50) {
                    --ecx50;
                }
                ecx51 = edx46 & 3;
                while (ecx51) {
                    --ecx51;
                }
                edi52 = *reinterpret_cast<signed char**>(ebx6 + reinterpret_cast<int32_t>("8w@"));
                ecx53 = -1;
                eax54 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax48) - reinterpret_cast<uint32_t>(eax48));
                do {
                    if (!ecx53) 
                        break;
                    --ecx53;
                    ++edi52;
                } while (*edi52 != *reinterpret_cast<signed char*>(&eax54));
                edx55 = reinterpret_cast<uint32_t>(~ecx53);
                ecx56 = -1;
                eax57 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax54) - reinterpret_cast<uint32_t>(eax54));
                do {
                    if (!ecx56) 
                        break;
                    --ecx56;
                } while (v58 != *reinterpret_cast<signed char*>(&eax57));
                ecx59 = edx55 >> 2;
                while (ecx59) {
                    --ecx59;
                }
                ecx60 = edx55 & 3;
                esp61 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp23) - 4 - 4);
                while (ecx60) {
                    --ecx60;
                }
                fun_405710(reinterpret_cast<int32_t>(esp61) + 28, "Microsoft Visual C++ Runtime Library", 0x12010);
                goto v62;
            }
        }
    }
    return;
}

uint32_t fun_404ad0(uint32_t a1) {
    int1_t cf2;
    uint32_t eax3;

    cf2 = a1 < g40cf60;
    if (cf2) {
        eax3 = 0;
        *reinterpret_cast<signed char*>(&eax3) = *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(*reinterpret_cast<void**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60)) + (a1 & 31) * 8 + 4);
        return eax3 & 64;
    } else {
        return 0;
    }
}

int32_t g40adf8 = 0;

void fun_404a80(struct s5* a1) {
    struct s0* eax2;

    ++g40adf8;
    eax2 = fun_404f00(0x1000);
    a1->f8 = eax2;
    if (!eax2) {
        a1->fc = a1->fc | 4;
        a1->f8 = reinterpret_cast<struct s0*>(&a1->f14);
        a1->f18 = reinterpret_cast<void*>(2);
    } else {
        a1->fc = a1->fc | 8;
        a1->f18 = reinterpret_cast<void*>(0x1000);
    }
    *reinterpret_cast<struct s0**>(&a1->f0) = a1->f8;
    a1->f4 = reinterpret_cast<struct s5*>(0);
    return;
}

int32_t g40a578 = 0;

uint32_t g40a57c = 0;

int32_t fun_4049c0(uint32_t a1, int32_t a2, int32_t a3);

struct s17 {
    int32_t f0;
    unsigned char f4;
};

int32_t GetLastError = 0xd860;

void fun_4058b0(uint32_t a1);

void* fun_404790(uint32_t a1, struct s0* a2, void* a3) {
    int1_t below_or_equal4;
    void** esp5;
    void* v6;
    void* ebp7;
    int32_t** v8;
    uint32_t eax9;
    int32_t* ebx10;
    uint32_t v11;
    unsigned char al12;
    void* esi13;
    void* ebp14;
    struct s17* ecx15;
    void* v16;
    void* v17;
    int32_t ecx18;
    struct s0* v19;
    int32_t v20;
    int32_t eax21;
    struct s0* ebx22;
    void* edi23;
    void* esp24;
    void* edi25;
    int32_t eax26;

    below_or_equal4 = g40cf60 <= a1;
    esp5 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x418 - 4 - 4 - 4 - 4);
    v6 = ebp7;
    if (below_or_equal4 || (v8 = reinterpret_cast<int32_t**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60), eax9 = (a1 & 31) << 3, ebx10 = *v8, v11 = eax9, al12 = *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(ebx10) + eax9 + 4), (al12 & 1) == 0)) {
        g40a578 = 9;
        g40a57c = 0;
        return 0xffffffff;
    }
    esi13 = reinterpret_cast<void*>(0);
    ebp14 = a3;
    if (!ebp14) {
        return 0;
    }
    if (al12 & 32) {
        fun_4049c0(a1, 0, 2);
        esp5 = esp5 - 1 - 1 - 1 - 1 + 1 + 3;
    }
    ecx15 = reinterpret_cast<struct s17*>(v11 + reinterpret_cast<int32_t>(*v8));
    if (ecx15->f4 & 0x80) 
        goto addr_40481e_8;
    v16 = reinterpret_cast<void*>(esp5 + 5);
    v17 = ebp14;
    ecx18 = ecx15->f0;
    v19 = a2;
    v20 = ecx18;
    eax21 = reinterpret_cast<int32_t>(WriteFile(v20, v19, v17, v16, 0));
    if (!eax21) {
        addr_4048d5_10:
        GetLastError(v20, v19, v17, v16, 0);
    }
    addr_4048df_12:
    if (0) {
        return -static_cast<uint32_t>(esi13);
    } else {
        if (1) {
            if (!(*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(*v8) + v11 + 4) & 64) || !reinterpret_cast<int1_t>(*reinterpret_cast<struct s0**>(&a2->f0) == 26)) {
                g40a578 = 28;
                g40a57c = 0;
                return 0xffffffff;
            } else {
                return 0;
            }
        } else {
            if (1) {
                fun_4058b0(0);
                return 0xffffffff;
            } else {
                g40a578 = 9;
                g40a57c = 0;
                return 0xffffffff;
            }
        }
    }
    addr_40481e_8:
    ebx22 = a2;
    do {
        if (reinterpret_cast<unsigned char>(ebx22) - reinterpret_cast<unsigned char>(a2) >= reinterpret_cast<uint32_t>(ebp14)) 
            goto addr_4048df_12;
        edi23 = reinterpret_cast<void*>(esp5 + 9);
        do {
            if (reinterpret_cast<unsigned char>(ebx22) - reinterpret_cast<unsigned char>(a2) >= reinterpret_cast<uint32_t>(ebp14)) 
                break;
            ebx22 = reinterpret_cast<struct s0*>(&ebx22->pad4);
            if (reinterpret_cast<int1_t>(*reinterpret_cast<struct s0**>(&ebx22->f0) == 10)) {
                esi13 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi13) + 1);
                edi23 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(edi23) + 1);
            }
            edi23 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(edi23) + 1);
        } while (reinterpret_cast<int32_t>(edi23) - reinterpret_cast<int32_t>(esp5 + 9) < 0x400);
        esp24 = reinterpret_cast<void*>(esp5 - 1);
        edi25 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(edi23) - reinterpret_cast<uint32_t>(esp5 + 9));
        v16 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp24) + 24);
        v17 = edi25;
        v19 = reinterpret_cast<struct s0*>(reinterpret_cast<int32_t>(esp24) + 40);
        v20 = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(*v8) + v11);
        eax26 = reinterpret_cast<int32_t>(WriteFile(v20, v19, v17, v16, 0));
        esp5 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp24) - 4 - 4 - 4 - 4 - 4 + 4);
        if (!eax26) 
            goto addr_4048d5_10;
    } while (reinterpret_cast<int32_t>(v6) >= reinterpret_cast<int32_t>(edi25));
    goto addr_4048df_12;
}

int32_t fun_4059b0(uint32_t a1);

int32_t SetFilePointer = 0xd870;

int32_t fun_4049c0(uint32_t a1, int32_t a2, int32_t a3) {
    int1_t below_or_equal4;
    void** edi5;
    uint32_t esi6;
    int32_t eax7;
    int32_t eax8;
    uint32_t eax9;

    below_or_equal4 = g40cf60 <= a1;
    if (below_or_equal4 || (edi5 = reinterpret_cast<void**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60), esi6 = (a1 & 31) * 8, (*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(*edi5) + esi6 + 4) & 1) == 0)) {
        g40a578 = 9;
        g40a57c = 0;
        return -1;
    } else {
        eax7 = fun_4059b0(a1);
        if (eax7 != -1) {
            eax8 = reinterpret_cast<int32_t>(SetFilePointer());
            eax9 = 0;
            if (eax8 == -1) {
                eax9 = reinterpret_cast<uint32_t>(GetLastError());
            }
            if (!eax9) {
                *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(*edi5) + esi6 + 4) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(*edi5) + esi6 + 4) & 0xfd);
                goto a3;
            } else {
                fun_4058b0(eax9);
                goto a3;
            }
        } else {
            g40a578 = 9;
            return -1;
        }
    }
}

struct s5* fun_4035f0(int32_t* a1) {
    int32_t ecx2;

    ecx2 = *a1 + 4;
    *a1 = ecx2;
    return *reinterpret_cast<struct s5**>(ecx2 - 4);
}

void fun_403520(struct s5* a1, struct s5* a2, int32_t* a3) {
    struct s5* eax4;
    uint32_t eax5;

    eax4 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(a2->f4) - 1);
    a2->f4 = eax4;
    if (reinterpret_cast<int16_t>(eax4) < reinterpret_cast<int16_t>(0)) {
        eax5 = fun_402a20(a1, a2);
    } else {
        *reinterpret_cast<struct s0**>(&(*reinterpret_cast<struct s0**>(&a2->f0))->f0) = *reinterpret_cast<struct s0**>(&a1);
        eax5 = 0;
        *reinterpret_cast<struct s0**>(&eax5) = *reinterpret_cast<struct s0**>(&(*reinterpret_cast<struct s0**>(&a2->f0))->f0);
        *reinterpret_cast<struct s0**>(&a2->f0) = reinterpret_cast<struct s0*>(&(*reinterpret_cast<struct s0**>(&a2->f0))->pad4);
    }
    if (eax5 != 0xffffffff) {
        *a3 = *a3 + 1;
        return;
    } else {
        *a3 = -1;
        return;
    }
}

int32_t* fun_403620(int32_t* a1) {
    int32_t* eax2;
    int32_t ecx3;

    eax2 = a1;
    ecx3 = *eax2 + 4;
    *eax2 = ecx3;
    *reinterpret_cast<struct s5**>(&eax2) = *reinterpret_cast<struct s5**>(ecx3 - 4);
    return eax2;
}

struct s0* g40ae40 = reinterpret_cast<struct s0*>(0);

struct s5* fun_404bf0(signed char* a1, uint16_t a2) {
    int1_t zf3;
    struct s0* eax4;
    int32_t eax5;

    if (a1) {
        zf3 = g40ae30 == 0;
        if (!zf3) {
            eax4 = g40ae40;
            eax5 = reinterpret_cast<int32_t>(WideCharToMultiByte());
            if (!eax5 || eax4) {
                g40a578 = 42;
            }
            goto 0x220;
        } else {
            if (a2 <= 0xff) {
                *a1 = *reinterpret_cast<signed char*>(&a2);
                return 1;
            } else {
                g40a578 = 42;
                return 0xffffffff;
            }
        }
    } else {
        return 0;
    }
}

struct s5* fun_403600(int32_t* a1) {
    int32_t ecx2;

    ecx2 = *a1 + 8;
    *a1 = ecx2;
    return *reinterpret_cast<struct s5**>(ecx2 - 8);
}

uint32_t fun_404d00(struct s5* a1, struct s5* a2, struct s5* a3, struct s5* a4) {
    struct s5* ecx5;
    struct s5* ebx6;
    struct s5* eax7;
    uint32_t eax8;
    struct s5* eax9;
    struct s5* tmp32_10;
    uint32_t eax11;

    if (a4) {
        ecx5 = a4;
        ebx6 = a3;
        eax7 = a1;
        do {
            ecx5 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ecx5) >> 1);
            __asm__("rcr ebx, 1");
            __asm__("rcr eax, 1");
        } while (ecx5);
        eax8 = reinterpret_cast<uint16_t>(eax7) / reinterpret_cast<uint16_t>(ebx6);
        eax9 = reinterpret_cast<struct s5*>(eax8 * reinterpret_cast<uint16_t>(a3));
        tmp32_10 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(__intrinsic()) + eax8 * reinterpret_cast<uint16_t>(a4));
        if (reinterpret_cast<uint16_t>(tmp32_10) < reinterpret_cast<uint16_t>(__intrinsic())) 
            goto addr_404d5a_5;
        if (reinterpret_cast<uint16_t>(tmp32_10) > reinterpret_cast<uint16_t>(a2)) 
            goto addr_404d5a_5;
        if (reinterpret_cast<uint16_t>(tmp32_10) < reinterpret_cast<uint16_t>(a2)) 
            goto addr_404d62_8;
        if (reinterpret_cast<uint16_t>(eax9) <= reinterpret_cast<uint16_t>(a1)) 
            goto addr_404d62_8;
    } else {
        eax11 = reinterpret_cast<uint16_t>(a1) % reinterpret_cast<uint16_t>(a3);
        goto addr_404d71_11;
    }
    addr_404d5a_5:
    eax9 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(eax9) - reinterpret_cast<uint16_t>(a3));
    addr_404d62_8:
    eax11 = -(reinterpret_cast<uint16_t>(eax9) - reinterpret_cast<uint16_t>(a1));
    addr_404d71_11:
    return eax11;
}

struct s5* fun_404c90(struct s5* a1, struct s5* a2, struct s5* a3, struct s5* a4) {
    struct s5* ecx5;
    struct s5* ebx6;
    struct s5* eax7;
    struct s5* eax8;
    struct s5* esi9;
    struct s5* tmp32_10;
    struct s5* eax11;

    if (a4) {
        ecx5 = a4;
        ebx6 = a3;
        eax7 = a1;
        do {
            ecx5 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ecx5) >> 1);
            __asm__("rcr ebx, 1");
            __asm__("rcr eax, 1");
        } while (ecx5);
        eax8 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(eax7) / reinterpret_cast<uint16_t>(ebx6));
        esi9 = eax8;
        tmp32_10 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(__intrinsic()) + reinterpret_cast<uint16_t>(eax8) * reinterpret_cast<uint16_t>(a4));
        if (reinterpret_cast<uint16_t>(tmp32_10) < reinterpret_cast<uint16_t>(__intrinsic())) 
            goto addr_404cee_5;
        if (reinterpret_cast<uint16_t>(tmp32_10) > reinterpret_cast<uint16_t>(a2)) 
            goto addr_404cee_5;
        if (reinterpret_cast<uint16_t>(tmp32_10) < reinterpret_cast<uint16_t>(a2)) 
            goto addr_404cef_8;
        if (reinterpret_cast<uint16_t>(reinterpret_cast<uint16_t>(a3) * reinterpret_cast<uint16_t>(esi9)) <= reinterpret_cast<uint16_t>(a1)) 
            goto addr_404cef_8;
    } else {
        eax11 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(a1) / reinterpret_cast<uint16_t>(a3));
        goto addr_404cf3_11;
    }
    addr_404cee_5:
    esi9 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(esi9) - 1);
    addr_404cef_8:
    eax11 = esi9;
    addr_404cf3_11:
    return eax11;
}

void fun_403570(struct s5* a1, struct s5* a2, struct s5* a3, int32_t* a4) {
    struct s5* esi5;
    struct s5* edi6;
    struct s5* ebx7;
    int32_t* ebp8;
    struct s5* eax9;

    esi5 = a1;
    edi6 = a2;
    ebx7 = a3;
    ebp8 = a4;
    do {
        eax9 = edi6;
        edi6 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(edi6) - 1);
        if (reinterpret_cast<uint1_t>(reinterpret_cast<int16_t>(eax9) < reinterpret_cast<int16_t>(0)) | reinterpret_cast<uint1_t>(eax9 == 0)) 
            break;
        fun_403520(esi5, ebx7, ebp8);
    } while (*ebp8 != -1);
    return;
}

void fun_4035b0(struct s5* a1, struct s5* a2, struct s5* a3, int32_t* a4) {
    struct s5* esi5;
    struct s5* edi6;
    struct s5* ebx7;
    int32_t* ebp8;
    struct s5* eax9;
    struct s5* eax10;
    struct s5* v11;

    esi5 = a1;
    edi6 = a2;
    ebx7 = a3;
    ebp8 = a4;
    do {
        eax9 = edi6;
        edi6 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(edi6) - 1);
        if (reinterpret_cast<uint1_t>(reinterpret_cast<int16_t>(eax9) < reinterpret_cast<int16_t>(0)) | reinterpret_cast<uint1_t>(eax9 == 0)) 
            break;
        eax10 = esi5;
        esi5 = reinterpret_cast<struct s5*>(&esi5->pad4);
        v11 = reinterpret_cast<struct s5*>(static_cast<int32_t>(reinterpret_cast<signed char>(*reinterpret_cast<struct s0**>(&eax10->f0))));
        fun_403520(v11, ebx7, ebp8);
    } while (*ebp8 != -1);
    return;
}

struct s18 {
    signed char[4] pad4;
    int32_t f4;
    int32_t* f8;
    int32_t fc;
};

void fun_403872(int32_t ecx, int32_t a2);

void fun_4037de(struct s18* a1, int32_t a2) {
    struct s6* v3;
    int32_t* ebx4;
    int32_t esi5;
    int32_t ecx6;

    v3 = *reinterpret_cast<struct s6**>(&g0);
    *reinterpret_cast<struct s6**>(&g0) = reinterpret_cast<struct s6*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 - 4 - 4 - 4 - 4 - 4 - 4);
    while ((ebx4 = a1->f8, a1->fc != -1) && a1->fc != a2) {
        esi5 = a1->fc + a1->fc * 2;
        ecx6 = ebx4[esi5];
        a1->fc = ecx6;
        if (!(ebx4 + esi5)[1]) {
            fun_403872(ecx6, 0x101);
            (ebx4 + esi5)[2]();
        }
    }
    *reinterpret_cast<struct s6**>(&g0) = v3;
    return;
}

int32_t g40a818 = 0;

struct s19 {
    signed char[8] pad8;
    int32_t f8;
};

int32_t g40a814 = 0;

int32_t g40a81c = 0;

void fun_403872(int32_t ecx, int32_t a2) {
    struct s19* ebp3;
    int32_t eax4;
    int32_t ebp5;

    g40a818 = ebp3->f8;
    g40a814 = eax4;
    g40a81c = ebp5;
    return;
}

int32_t g40b7f0 = 0;

struct s0* fun_404f20(void* a1, int32_t a2);

struct s0* fun_404f00(void* a1) {
    int32_t eax2;
    struct s0* eax3;

    eax2 = g40b7f0;
    eax3 = fun_404f20(a1, eax2);
    return eax3;
}

struct s20 {
    struct s20* f0;
    signed char[2060] pad2064;
    struct s0* f810;
};

signed char* fun_405260(struct s0* a1, struct s20** a2, uint32_t* a3);

int32_t g40ce54;

int32_t HeapFree = 0xd8a6;

struct s21 {
    signed char[2064] pad2064;
    int32_t f810;
};

void fun_4052c0(struct s21* a1, int32_t a2, signed char* a3);

void fun_404eb0(struct s0* a1) {
    void* esp2;
    signed char* eax3;
    int32_t eax4;
    struct s21* v5;
    int32_t v6;

    esp2 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 8 - 4);
    if (a1) {
        eax3 = fun_405260(a1, reinterpret_cast<int32_t>(esp2) + 8, reinterpret_cast<int32_t>(esp2) + 4);
        if (!eax3) {
            eax4 = g40ce54;
            HeapFree(eax4, 0, a1);
        } else {
            fun_4052c0(v5, v6, eax3);
            return;
        }
    }
    return;
}

void fun_403c10(struct s0** a1, struct s0* a2, struct s0* a3, int32_t* a4, int32_t* a5) {
    int32_t* ecx6;
    struct s0** esi7;
    struct s0* eax8;
    struct s0* edx9;
    int32_t edx10;
    struct s0* dl11;
    int32_t ebx12;
    uint32_t edi13;
    struct s0* edx14;
    int32_t ebx15;
    uint32_t ebp16;
    struct s0** edx17;
    uint32_t ebp18;
    uint32_t edx19;
    int32_t ebx20;
    int32_t ebx21;

    ecx6 = a5;
    esi7 = a1;
    eax8 = a3;
    *ecx6 = 0;
    *a4 = 1;
    if (a2) {
        edx9 = a2;
        a2 = reinterpret_cast<struct s0*>(&a2->f4);
        *reinterpret_cast<struct s0**>(&edx9->f0) = eax8;
    }
    if (*esi7 == 34) {
        ++esi7;
        if (*esi7 != 34) {
            do {
                if (!*esi7) 
                    break;
                edx10 = 0;
                *reinterpret_cast<struct s0**>(&edx10) = *esi7;
                if (*reinterpret_cast<unsigned char*>(edx10 + 0x40a8b9) & 4 && (*ecx6 = *ecx6 + 1, !!eax8)) {
                    ++esi7;
                    *reinterpret_cast<struct s0**>(&eax8->f0) = *esi7;
                    eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
                }
                *ecx6 = *ecx6 + 1;
                if (eax8) {
                    *reinterpret_cast<struct s0**>(&eax8->f0) = *esi7;
                    eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
                }
                ++esi7;
            } while (!reinterpret_cast<int1_t>(*esi7 == 34));
        }
        *ecx6 = *ecx6 + 1;
        if (eax8) {
            *reinterpret_cast<struct s0**>(&eax8->f0) = reinterpret_cast<struct s0*>(0);
            eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
        }
        if (reinterpret_cast<int1_t>(*esi7 == 34)) {
            ++esi7;
        }
    } else {
        do {
            *ecx6 = *ecx6 + 1;
            if (eax8) {
                *reinterpret_cast<struct s0**>(&eax8->f0) = *esi7;
                eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
            }
            dl11 = *esi7;
            ++esi7;
            ebx12 = 0;
            *reinterpret_cast<struct s0**>(&ebx12) = dl11;
            if (*reinterpret_cast<unsigned char*>(ebx12 + 0x40a8b9) & 4) {
                *ecx6 = *ecx6 + 1;
                if (eax8) {
                    *reinterpret_cast<struct s0**>(&eax8->f0) = *esi7;
                    eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
                }
                ++esi7;
            }
            if (dl11 == 32) 
                break;
            if (!dl11) 
                goto addr_403c80_23;
        } while (!reinterpret_cast<int1_t>(dl11 == 9));
        if (dl11) {
            if (eax8) {
                *reinterpret_cast<struct s0**>(reinterpret_cast<unsigned char>(eax8) + 0xffffffff) = reinterpret_cast<struct s0*>(0);
            }
        } else {
            addr_403c80_23:
            --esi7;
        }
    }
    edi13 = 0;
    while (*esi7) {
        while (*esi7 == 32 || reinterpret_cast<int1_t>(*esi7 == 9)) {
            ++esi7;
        }
        if (!*esi7) 
            break;
        if (a2) {
            edx14 = a2;
            a2 = reinterpret_cast<struct s0*>(&a2->f4);
            *reinterpret_cast<struct s0**>(&edx14->f0) = eax8;
        }
        *a4 = *a4 + 1;
        while (1) {
            ebx15 = 1;
            ebp16 = 0;
            if (reinterpret_cast<int1_t>(*esi7 == 92)) {
                do {
                    ++esi7;
                    ++ebp16;
                } while (*esi7 == 92);
            }
            if (reinterpret_cast<int1_t>(*esi7 == 34)) {
                if (!(ebp16 & 1)) {
                    if (!edi13 || (edx17 = esi7 + 1, !reinterpret_cast<int1_t>(*edx17 == 34))) {
                        ebx15 = 0;
                    } else {
                        esi7 = edx17;
                    }
                    edi13 = -(edi13 - (edi13 + reinterpret_cast<uint1_t>(edi13 < edi13 + reinterpret_cast<uint1_t>(edi13 < 1))));
                }
                ebp16 = ebp16 >> 1;
            }
            ebp18 = ebp16 - 1;
            if (ebp16) {
                do {
                    if (eax8) {
                        *reinterpret_cast<struct s0**>(&eax8->f0) = reinterpret_cast<struct s0*>(92);
                        eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
                    }
                    edx19 = ebp18;
                    *ecx6 = *ecx6 + 1;
                    --ebp18;
                } while (edx19);
            }
            if (!*esi7) 
                break;
            if (edi13) 
                goto addr_403d76_51;
            if (*esi7 == 32) 
                break;
            if (*esi7 == 9) 
                break;
            addr_403d76_51:
            if (ebx15) {
                if (!eax8) {
                    ebx20 = 0;
                    *reinterpret_cast<struct s0**>(&ebx20) = *esi7;
                    if (*reinterpret_cast<unsigned char*>(ebx20 + 0x40a8b9) & 4) {
                        ++esi7;
                        *ecx6 = *ecx6 + 1;
                    }
                    *ecx6 = *ecx6 + 1;
                } else {
                    ebx21 = 0;
                    *reinterpret_cast<struct s0**>(&ebx21) = *esi7;
                    if (*reinterpret_cast<unsigned char*>(ebx21 + 0x40a8b9) & 4) {
                        *reinterpret_cast<struct s0**>(&eax8->f0) = *esi7;
                        ++esi7;
                        eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
                        *ecx6 = *ecx6 + 1;
                    }
                    eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
                    ++esi7;
                    *reinterpret_cast<struct s0**>(reinterpret_cast<unsigned char>(eax8) + 0xffffffff) = *esi7;
                    *ecx6 = *ecx6 + 1;
                    continue;
                }
            }
            ++esi7;
        }
        if (eax8) {
            *reinterpret_cast<struct s0**>(&eax8->f0) = reinterpret_cast<struct s0*>(0);
            eax8 = reinterpret_cast<struct s0*>(&eax8->pad4);
        }
        *ecx6 = *ecx6 + 1;
    }
    if (a2) {
        *reinterpret_cast<struct s0**>(&a2->f0) = reinterpret_cast<struct s0*>(0);
    }
    *a4 = *a4 + 1;
    return;
}

struct s0* g40a9c8 = reinterpret_cast<struct s0*>(0);

struct s0* g40a9bc = reinterpret_cast<struct s0*>(0);

struct s0* g40a9c0 = reinterpret_cast<struct s0*>(0);

struct s0* g40a9cc = reinterpret_cast<struct s0*>(0);

struct s0* g40a9d0 = reinterpret_cast<struct s0*>(0);

void fun_404210() {
    struct s0** edi1;
    int32_t ecx2;

    edi1 = reinterpret_cast<struct s0**>(0x40a8b8);
    ecx2 = 64;
    while (ecx2) {
        --ecx2;
        *edi1 = reinterpret_cast<struct s0*>(0);
        edi1 = edi1 + 4;
    }
    *edi1 = reinterpret_cast<struct s0*>(0);
    g40a9c8 = reinterpret_cast<struct s0*>(0);
    g40a9bc = reinterpret_cast<struct s0*>(0);
    g40a9c0 = reinterpret_cast<struct s0*>(0);
    g40a9cc = reinterpret_cast<struct s0*>(0);
    g40a9d0 = reinterpret_cast<struct s0*>(0);
    return;
}

void fun_4041fc();

struct s0* fun_4041b0(struct s0* a1) {
    uint32_t eax2;
    int32_t ecx3;

    eax2 = reinterpret_cast<unsigned char>(a1) - 0x3a4;
    if (eax2 > 18) 
        goto addr_4041cd_2;
    ecx3 = 0;
    *reinterpret_cast<signed char*>(&ecx3) = *reinterpret_cast<signed char*>(eax2 + reinterpret_cast<int32_t>(fun_4041fc));
    switch (ecx3) {
    case 0:
        return 0x411;
    case 1:
        return 0x804;
    case 2:
        return 0x412;
    case 3:
        return 0x404;
        addr_4041cd_2:
    case 4:
        return 0;
    }
}

void fun_406000();

void fun_40379c(struct s18* a1) {
    fun_406000();
    goto a1->f4;
}

unsigned char* fun_4057b0(unsigned char* a1, unsigned char* a2, uint32_t a3) {
    uint32_t ecx4;
    uint32_t ebx5;
    unsigned char* esi6;
    unsigned char* edi7;
    uint32_t ecx8;
    unsigned char eax9;
    uint32_t ecx10;
    unsigned char edx11;

    ecx4 = a3;
    if (!ecx4) {
        addr_405833_2:
        return a1;
    } else {
        ebx5 = ecx4;
        esi6 = a2;
        edi7 = a1;
        if (!(reinterpret_cast<uint32_t>(esi6) & 3)) {
            ecx8 = ecx4 >> 2;
            if (!ecx8) {
                goto addr_4057f5_6;
            }
        }
        do {
            eax9 = *esi6;
            ++esi6;
            *edi7 = eax9;
            ++edi7;
            --ecx4;
            if (!ecx4) 
                goto addr_405802_8;
            if (!eax9) 
                break;
        } while (reinterpret_cast<uint32_t>(esi6) & 3);
        goto addr_4057e9_11;
    }
    if (reinterpret_cast<uint32_t>(edi7) & 3) {
        do {
            *edi7 = eax9;
            ++edi7;
            --ecx4;
            if (!ecx4) 
                goto addr_4058a6_14;
        } while (reinterpret_cast<uint32_t>(edi7) & 3);
    }
    ebx5 = ecx4;
    ecx10 = ecx4 >> 2;
    if (ecx10) 
        goto addr_405897_17; else 
        goto addr_40582b_18;
    addr_4057e9_11:
    ebx5 = ecx4;
    ecx8 = ecx4 >> 2;
    if (ecx8) {
        do {
            edx11 = *esi6;
            esi6 = esi6 + 4;
            if ((*esi6 ^ 0xffffffff ^ 0x7efefeff + *esi6) & 0x81010100) {
                if (!*reinterpret_cast<signed char*>(&edx11)) 
                    break;
                if (!*reinterpret_cast<signed char*>(&edx11 + 1)) 
                    goto addr_405881_22;
                if (!(edx11 & 0xff0000)) 
                    goto addr_405877_24;
                if (!(edx11 & 0xff000000)) 
                    goto addr_405873_26;
            }
            *edi7 = edx11;
            edi7 = edi7 + 4;
            --ecx8;
        } while (ecx8);
        goto addr_4057f0_28;
    } else {
        addr_4057f0_28:
        ebx5 = ebx5 & 3;
        if (!ebx5) {
            addr_405802_8:
            return a1;
        } else {
            do {
                addr_4057f5_6:
                eax9 = *esi6;
                ++esi6;
                *edi7 = eax9;
                ++edi7;
                if (!eax9) 
                    goto addr_40582e_29;
                --ebx5;
            } while (ebx5);
            goto addr_405802_8;
        }
    }
    *edi7 = reinterpret_cast<unsigned char>(0);
    addr_40588f_32:
    edi7 = edi7 + 4;
    eax9 = reinterpret_cast<unsigned char>(0);
    ecx10 = ecx8 - 1;
    if (!ecx10) {
        addr_4058a1_33:
        ebx5 = ebx5 & 3;
        if (ebx5) {
            do {
                addr_40582b_18:
                *edi7 = eax9;
                ++edi7;
                addr_40582e_29:
                --ebx5;
            } while (ebx5);
        } else {
            addr_4058a6_14:
            return a1;
        }
    } else {
        addr_405897_17:
        eax9 = reinterpret_cast<unsigned char>(0);
        goto addr_405899_34;
    }
    goto addr_405833_2;
    do {
        addr_405899_34:
        *edi7 = reinterpret_cast<unsigned char>(0);
        edi7 = edi7 + 4;
        --ecx10;
    } while (ecx10);
    goto addr_4058a1_33;
    addr_405881_22:
    *edi7 = reinterpret_cast<unsigned char>(edx11 & 0xff);
    goto addr_40588f_32;
    addr_405877_24:
    *edi7 = reinterpret_cast<unsigned char>(edx11 & 0xffff);
    goto addr_40588f_32;
    addr_405873_26:
    *edi7 = edx11;
    goto addr_40588f_32;
}

int32_t g40b668 = 0;

int32_t LoadLibraryA = 0xd8e0;

int32_t GetProcAddress = 0xd8ce;

int32_t g40b66c = 0;

int32_t g40b670 = 0;

void fun_405710(void* a1, int32_t a2, int32_t a3) {
    int32_t esi4;
    int1_t zf5;
    int32_t eax6;
    int32_t ebx7;
    int32_t eax8;
    int32_t ebx9;
    int32_t eax10;
    int32_t eax11;
    int32_t eax12;
    int32_t eax13;
    int1_t zf14;

    esi4 = 0;
    zf5 = g40b668 == 0;
    if (zf5) {
        eax6 = reinterpret_cast<int32_t>(LoadLibraryA());
        if (!eax6 || (ebx7 = GetProcAddress, eax8 = reinterpret_cast<int32_t>(ebx7(eax6, "MessageBoxA")), g40b668 = eax8, eax8 == 0)) {
            goto ebx9;
        } else {
            eax10 = reinterpret_cast<int32_t>(ebx7(eax6, "GetActiveWindow", eax6, "MessageBoxA"));
            g40b66c = eax10;
            eax11 = reinterpret_cast<int32_t>(ebx7(eax6, "GetLastActivePopup", eax6, "GetActiveWindow", eax6, "MessageBoxA"));
            g40b670 = eax11;
        }
    }
    eax12 = g40b66c;
    if (eax12) {
        eax13 = reinterpret_cast<int32_t>(eax12());
        esi4 = eax13;
    }
    if (esi4 && (zf14 = g40b670 == 0, !zf14)) {
        g40b670(esi4);
    }
    g40b668();
    goto a3;
}

struct s22 {
    int32_t f0;
    unsigned char f4;
};

int32_t fun_4059b0(uint32_t a1) {
    int1_t cf2;
    struct s22* eax3;

    cf2 = a1 < g40cf60;
    if (!cf2 || (eax3 = reinterpret_cast<struct s22*>(*reinterpret_cast<int32_t**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60) + (a1 & 31) * 2), (eax3->f4 & 1) == 0)) {
        g40a578 = 9;
        g40a57c = 0;
        return -1;
    } else {
        return eax3->f0;
    }
}

void fun_4058b0(uint32_t a1) {
    uint32_t edx2;
    int32_t eax3;
    uint32_t* ecx4;

    edx2 = a1;
    eax3 = 0;
    ecx4 = reinterpret_cast<uint32_t*>(0x40b680);
    g40a57c = edx2;
    do {
        if (*ecx4 == edx2) 
            break;
        ecx4 = ecx4 + 2;
        ++eax3;
    } while (reinterpret_cast<uint32_t>(ecx4) < 0x40b7e8);
    goto addr_4058d1_4;
    g40a578 = *reinterpret_cast<int32_t*>(eax3 * 8 + 0x40b684);
    return;
    addr_4058d1_4:
    if (edx2 < 19 || edx2 > 36) {
        if (edx2 < 0xbc || edx2 > 0xca) {
            g40a578 = 22;
            return;
        } else {
            g40a578 = 8;
            return;
        }
    } else {
        g40a578 = 13;
        return;
    }
}

int32_t HeapAlloc = 0xd8b2;

void* g40b664 = reinterpret_cast<void*>(0x1e0);

struct s0* fun_405310(void* a1);

int32_t fun_405de0(void* a1);

struct s0* fun_405a00(uint32_t a1, int32_t a2) {
    void* esi3;
    int32_t ebx4;
    struct s0* edx5;
    int1_t cf6;
    struct s0* eax7;
    int32_t eax8;
    struct s0* eax9;
    struct s0* edi10;
    uint32_t ecx11;
    uint32_t ecx12;
    int1_t zf13;
    int32_t eax14;

    esi3 = reinterpret_cast<void*>(a2 * a1);
    if (reinterpret_cast<uint32_t>(esi3) <= 0xffffffe0) {
        if (!esi3) {
            esi3 = reinterpret_cast<void*>(16);
        } else {
            esi3 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi3) + 15 & 0xfffffff0);
        }
    }
    ebx4 = HeapAlloc;
    do {
        edx5 = reinterpret_cast<struct s0*>(0);
        if (reinterpret_cast<uint32_t>(esi3) <= 0xffffffe0) {
            cf6 = reinterpret_cast<uint32_t>(g40b664) < reinterpret_cast<uint32_t>(esi3);
            if (cf6) {
                addr_405a5d_8:
                if (edx5) 
                    break; else 
                    goto addr_405a61_9;
            } else {
                eax7 = fun_405310(reinterpret_cast<uint32_t>(esi3) >> 4);
                edx5 = eax7;
                if (!edx5) {
                    addr_405a61_9:
                    eax8 = g40ce54;
                    eax9 = reinterpret_cast<struct s0*>(ebx4(eax8, 8, esi3));
                    edx5 = eax9;
                } else {
                    edi10 = edx5;
                    ecx11 = reinterpret_cast<uint32_t>(esi3) >> 2;
                    while (ecx11) {
                        --ecx11;
                        *reinterpret_cast<struct s0**>(&edi10->f0) = reinterpret_cast<struct s0*>(0);
                        edi10 = reinterpret_cast<struct s0*>(&edi10->f4);
                        esi3 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi3) + 4);
                    }
                    ecx12 = reinterpret_cast<uint32_t>(esi3) & 3;
                    while (ecx12) {
                        --ecx12;
                        *reinterpret_cast<struct s0**>(&edi10->f0) = reinterpret_cast<struct s0*>(0);
                        edi10 = reinterpret_cast<struct s0*>(&edi10->pad4);
                        esi3 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi3) + 1);
                    }
                    goto addr_405a5d_8;
                }
            }
        }
        if (edx5) 
            break;
        zf13 = g40b7f0 == 0;
        if (zf13) 
            break;
        eax14 = fun_405de0(esi3);
    } while (eax14);
    goto addr_405a88_20;
    return edx5;
    addr_405a88_20:
    return 0;
}

signed char* fun_405260(struct s0* a1, struct s20** a2, uint32_t* a3) {
    struct s20* ecx4;
    struct s0* edx5;
    uint32_t ecx6;

    ecx4 = reinterpret_cast<struct s20*>(0x40ae48);
    edx5 = a1;
    do {
        if (!ecx4->f810) 
            continue;
        if (reinterpret_cast<unsigned char>(ecx4->f810) >= reinterpret_cast<unsigned char>(edx5)) 
            continue;
        if (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(ecx4->f810) + 0x400000) > reinterpret_cast<unsigned char>(edx5)) 
            break;
        ecx4 = ecx4->f0;
    } while (!reinterpret_cast<int1_t>(ecx4 == 0x40ae48));
    goto addr_40528a_6;
    *a2 = ecx4;
    ecx6 = reinterpret_cast<unsigned char>(edx5) & 0xfffff000;
    *a3 = ecx6;
    return (reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(edx5) - ecx6 - 0x100) >> 4) + ecx6 + 8;
    addr_40528a_6:
    return 0;
}

struct s23 {
    signed char[16] pad16;
    signed char f10;
    signed char[1023] pad1040;
    signed char f410;
};

int32_t g40b660 = 0;

void fun_405180(int32_t a1);

void fun_4052c0(struct s21* a1, int32_t a2, signed char* a3) {
    struct s23* ecx4;
    int1_t zf5;

    ecx4 = reinterpret_cast<struct s23*>((a2 - a1->f810 >> 12) + reinterpret_cast<int32_t>(a1));
    ecx4->f10 = reinterpret_cast<signed char>(ecx4->f10 + *a3);
    *a3 = 0;
    ecx4->f410 = -15;
    if (ecx4->f10 == -16 && (++g40b660, zf5 = g40b660 == 32, zf5)) {
        fun_405180(16);
    }
    return;
}

struct s0* fun_404f70(void* a1) {
    void* esi2;
    int1_t below_or_equal3;
    struct s0* eax4;
    int32_t eax5;

    esi2 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(a1) + 15 & 0xfffffff0);
    below_or_equal3 = reinterpret_cast<uint32_t>(esi2) <= reinterpret_cast<uint32_t>(g40b664);
    if (!below_or_equal3 || (eax4 = fun_405310(reinterpret_cast<uint32_t>(esi2) >> 4), !eax4)) {
        eax5 = g40ce54;
        HeapAlloc(eax5, 0, esi2);
    }
    goto __return_address();
}

int32_t g40be3c;

int32_t fun_405de0(void* a1) {
    int32_t ecx2;
    int32_t eax3;

    ecx2 = g40be3c;
    if (!ecx2 || (eax3 = reinterpret_cast<int32_t>(ecx2(a1)), eax3 == 0)) {
        return 0;
    } else {
        return 1;
    }
}

struct s25 {
    signed char* f0;
    uint32_t f4;
    signed char f8;
    signed char[239] pad248;
    signed char ff8;
    signed char[7] pad256;
    struct s0* f100;
};

struct s24 {
    struct s24* f0;
    struct s24* f4;
    void* f8;
    void* fc;
    signed char f10;
    signed char f11;
    signed char[1021] pad1039;
    signed char f40f;
    signed char[1024] pad2064;
    struct s25* f810;
};

/* (image base) */
struct s24* image_base_ = reinterpret_cast<struct s24*>(0x40ae48);

struct s26 {
    signed char* f0;
    void* f4;
    signed char f8;
    signed char[239] pad248;
    signed char ff8;
};

struct s0* fun_405590(struct s26* a1, void* a2, void* a3);

int32_t VirtualAlloc = 0xd8be;

struct s27 {
    signed char* f0;
    signed char[4] pad8;
    signed char f8;
};

struct s28 {
    signed char[16] pad16;
    signed char f10;
    signed char[1023] pad1040;
    signed char f410;
};

struct s29 {
    signed char* f0;
    uint32_t f4;
    signed char f8;
};

struct s24* fun_404fb0();

struct s0* fun_405310(void* a1) {
    void* ebx2;
    int32_t v3;
    int32_t ebp4;
    struct s24* edi5;
    void* esi6;
    void* ebp7;
    void* eax8;
    void* ecx9;
    struct s26* v10;
    struct s0* eax11;
    void* ebp12;
    void* esi13;
    void* eax14;
    void* ecx15;
    struct s26* v16;
    struct s0* eax17;
    int1_t zf18;
    struct s24* esi19;
    void* edx20;
    void* ecx21;
    void* edi22;
    signed char** eax23;
    void* ecx24;
    struct s27* ebp25;
    struct s28* edx26;
    void* ecx27;
    struct s29* eax28;
    struct s24* eax29;
    struct s25* edx30;

    ebx2 = a1;
    v3 = ebp4;
    edi5 = image_base_;
    do {
        if (edi5->f810) {
            esi6 = edi5->f8;
            if (reinterpret_cast<int32_t>(esi6) < reinterpret_cast<int32_t>(0x400)) {
                ebp7 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi6) << 12);
                do {
                    eax8 = reinterpret_cast<void*>(0);
                    *reinterpret_cast<signed char*>(&eax8) = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi6) + 16);
                    if (reinterpret_cast<uint32_t>(eax8) >= reinterpret_cast<uint32_t>(ebx2) && (*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi6) + 16) != -1 && (ecx9 = reinterpret_cast<void*>(0), *reinterpret_cast<signed char*>(&ecx9) = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi6) + 0x410), reinterpret_cast<uint32_t>(ecx9) > reinterpret_cast<uint32_t>(ebx2)))) {
                        v10 = reinterpret_cast<struct s26*>(reinterpret_cast<uint32_t>(edi5->f810) + reinterpret_cast<uint32_t>(ebp7));
                        eax11 = fun_405590(v10, eax8, ebx2);
                        if (eax11) 
                            goto addr_405450_7;
                        *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi6) + 0x410) = *reinterpret_cast<signed char*>(&ebx2);
                    }
                    ebp7 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebp7) + 0x1000);
                    esi6 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi6) + 1);
                } while (reinterpret_cast<int32_t>(ebp7) < reinterpret_cast<int32_t>(0x400000));
            }
            ebp12 = reinterpret_cast<void*>(0);
            esi13 = reinterpret_cast<void*>(0);
            if (reinterpret_cast<int32_t>(edi5->f8) > reinterpret_cast<int32_t>(0)) {
                do {
                    eax14 = reinterpret_cast<void*>(0);
                    *reinterpret_cast<signed char*>(&eax14) = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi13) + 16);
                    if (reinterpret_cast<uint32_t>(eax14) >= reinterpret_cast<uint32_t>(ebx2) && (*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi13) + 16) != -1 && (ecx15 = reinterpret_cast<void*>(0), *reinterpret_cast<signed char*>(&ecx15) = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi13) + 0x410), reinterpret_cast<uint32_t>(ecx15) > reinterpret_cast<uint32_t>(ebx2)))) {
                        v16 = reinterpret_cast<struct s26*>(reinterpret_cast<uint32_t>(edi5->f810) + reinterpret_cast<uint32_t>(ebp12));
                        eax17 = fun_405590(v16, eax14, ebx2);
                        if (eax17) 
                            goto addr_405462_13;
                        *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi13) + 0x410) = *reinterpret_cast<signed char*>(&ebx2);
                    }
                    ebp12 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebp12) + 0x1000);
                    esi13 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi13) + 1);
                } while (reinterpret_cast<int32_t>(edi5->f8) > reinterpret_cast<int32_t>(esi13));
            }
        }
        edi5 = edi5->f0;
        zf18 = image_base_ == edi5;
    } while (!zf18);
    esi19 = reinterpret_cast<struct s24*>(0x40ae48);
    do {
        if (!esi19->f810) 
            continue;
        if (!reinterpret_cast<int1_t>(esi19->fc == 0xffffffff)) 
            break;
        esi19 = esi19->f0;
    } while (!reinterpret_cast<int1_t>(esi19 == 0x40ae48));
    goto addr_405410_21;
    edx20 = esi19->fc;
    ecx21 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edx20) + 16);
    if (reinterpret_cast<int32_t>(ecx21) >= reinterpret_cast<int32_t>(0x400)) {
        ecx21 = reinterpret_cast<void*>(0x400);
    }
    edi22 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edx20) + 1);
    if (reinterpret_cast<int32_t>(ecx21) > reinterpret_cast<int32_t>(edi22)) {
        do {
            if (*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi19) + reinterpret_cast<uint32_t>(edi22) + 16) != -1) 
                break;
            edi22 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edi22) + 1);
        } while (reinterpret_cast<int32_t>(ecx21) > reinterpret_cast<int32_t>(edi22));
    }
    eax23 = reinterpret_cast<signed char**>(VirtualAlloc());
    if (!reinterpret_cast<int1_t>(eax23 == (reinterpret_cast<uint32_t>(edx20) << 12) + reinterpret_cast<uint32_t>(esi19->f810))) {
        goto v3;
    } else {
        ecx24 = esi19->fc;
        ebp25 = reinterpret_cast<struct s27*>((reinterpret_cast<uint32_t>(ecx24) << 12) + reinterpret_cast<uint32_t>(esi19->f810));
        if (reinterpret_cast<int32_t>(ecx24) < reinterpret_cast<int32_t>(edi22)) {
            do {
                ecx24 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ecx24) + 1);
                ebp25->f0 = &ebp25->f8;
                ebp25 = reinterpret_cast<struct s27*>(reinterpret_cast<uint32_t>(ebp25) + 0x1000);
                edx26 = reinterpret_cast<struct s28*>(reinterpret_cast<uint32_t>(esi19) + reinterpret_cast<uint32_t>(ecx24) - 1);
                *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(ebp25) + 0xfffff004) = 0xf0;
                *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(ebp25) + 0xfffff0f8) = -1;
                edx26->f10 = -16;
                edx26->f410 = -15;
            } while (reinterpret_cast<int32_t>(ecx24) < reinterpret_cast<int32_t>(edi22));
        }
        image_base_ = esi19;
        if (reinterpret_cast<int32_t>(edi22) < reinterpret_cast<int32_t>(0x400)) {
            do {
                if (*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi19) + reinterpret_cast<uint32_t>(edi22) + 16) == -1) 
                    break;
                edi22 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edi22) + 1);
            } while (reinterpret_cast<int32_t>(edi22) < reinterpret_cast<int32_t>(0x400));
        }
        ecx27 = esi19->fc;
        esi19->fc = reinterpret_cast<void*>(0xffffffff);
        if (reinterpret_cast<int32_t>(edi22) < reinterpret_cast<int32_t>(0x400)) {
            esi19->fc = edi22;
        }
        eax28 = reinterpret_cast<struct s29*>(reinterpret_cast<uint32_t>(esi19->f810) + (reinterpret_cast<uint32_t>(ecx27) << 12));
        eax28->f8 = *reinterpret_cast<signed char*>(&ebx2);
        esi19->f8 = ecx27;
        *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi19) + reinterpret_cast<uint32_t>(ecx27) + 16) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi19) + reinterpret_cast<uint32_t>(ecx27) + 16) - *reinterpret_cast<signed char*>(&ebx2));
        eax28->f0 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(ebx2) + reinterpret_cast<uint32_t>(eax28) + 8);
        eax28->f4 = eax28->f4 - reinterpret_cast<uint32_t>(ebx2);
        goto v3;
    }
    addr_405410_21:
    eax29 = fun_404fb0();
    if (!eax29) {
        return 0;
    } else {
        edx30 = eax29->f810;
        edx30->f8 = *reinterpret_cast<signed char*>(&ebx2);
        image_base_ = eax29;
        edx30->f0 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(ebx2) + reinterpret_cast<uint32_t>(edx30) + 8);
        edx30->f4 = 0xf0 - reinterpret_cast<uint32_t>(ebx2);
        eax29->f10 = reinterpret_cast<signed char>(eax29->f10 - *reinterpret_cast<signed char*>(&ebx2));
        return &eax29->f810->f100;
    }
    addr_405462_13:
    image_base_ = edi5;
    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi13) + 16) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi13) + 16) - *reinterpret_cast<signed char*>(&ebx2));
    edi5->f8 = esi13;
    return eax17;
    addr_405450_7:
    image_base_ = edi5;
    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi6) + 16) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) + reinterpret_cast<uint32_t>(esi6) + 16) - *reinterpret_cast<signed char*>(&ebx2));
    edi5->f8 = esi6;
    return eax11;
}

int32_t VirtualFree = 0xd846;

int32_t g40b658 = 0;

void fun_405120(struct s24* a1) {
    int1_t zf2;
    struct s24* eax3;
    struct s24* ecx4;

    VirtualFree();
    zf2 = image_base_ == a1;
    if (zf2) {
        image_base_ = a1->f4;
    }
    if (a1 == 0x40ae48) {
        g40b658 = 0;
        goto 0;
    } else {
        eax3 = a1->f0;
        ecx4 = a1->f4;
        ecx4->f0 = eax3;
        a1->f0->f4 = a1->f4;
        HeapFree();
        goto 0;
    }
}

/* (image base) */
struct s24* image_base_ = reinterpret_cast<struct s24*>(0x40ae48);

void fun_405180(int32_t a1) {
    int32_t v2;
    int32_t ebx3;
    struct s24* esi4;
    int1_t zf5;
    void* ebp6;
    signed char* ebx7;
    uint32_t edi8;
    struct s25* eax9;
    int32_t eax10;
    struct s24* eax11;
    int32_t edx12;
    signed char* ecx13;

    v2 = ebx3;
    esi4 = image_base_;
    while (1) {
        if (!esi4->f810) {
            addr_40523c_3:
            zf5 = esi4 == image_base_;
            if (zf5) 
                break;
            if (a1 > 0) 
                continue; else 
                break;
        } else {
            ebp6 = reinterpret_cast<void*>(0x3ff);
            ebx7 = &esi4->f40f;
            edi8 = 0x3ff000;
            do {
                if (*ebx7 == -16 && (eax9 = esi4->f810, eax10 = reinterpret_cast<int32_t>(VirtualFree(reinterpret_cast<uint32_t>(eax9) + edi8, 0x1000, 0x4000)), !!eax10)) {
                    *ebx7 = -1;
                    --g40b660;
                    if (esi4->fc == 0xffffffff || reinterpret_cast<int32_t>(ebp6) < reinterpret_cast<int32_t>(esi4->fc)) {
                        esi4->fc = ebp6;
                    }
                    --v2;
                    if (!v2) 
                        break;
                }
                edi8 = edi8 - 0x1000;
                ebp6 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebp6) - 1);
                --ebx7;
            } while (reinterpret_cast<int32_t>(edi8) >= reinterpret_cast<int32_t>(0));
            eax11 = esi4;
            esi4 = esi4->f4;
            if (1) 
                goto addr_40523c_3;
            if (eax11->f10 != -1) 
                goto addr_40523c_3;
        }
        edx12 = 1;
        ecx13 = &eax11->f11;
        do {
            if (*ecx13 != -1) 
                break;
            ++edx12;
            ++ecx13;
        } while (edx12 < 0x400);
        if (edx12 != 0x400) 
            goto addr_40523c_3;
        fun_405120(eax11);
        goto addr_40523c_3;
    }
    return;
}

struct s0* fun_405590(struct s26* a1, void* a2, void* a3) {
    struct s26* eax4;
    void* edx5;
    void* ecx6;
    signed char* edi7;
    signed char* ebp8;
    signed char* ecx9;
    void* esi10;
    signed char* ebp11;
    void* ebx12;
    signed char* ecx13;
    void* ebx14;
    signed char* esi15;
    void* ebx16;
    signed char* ecx17;
    void* ebx18;
    signed char* esi19;

    eax4 = a1;
    edx5 = a3;
    ecx6 = eax4->f4;
    edi7 = eax4->f0;
    ebp8 = edi7;
    if (reinterpret_cast<uint32_t>(edx5) <= reinterpret_cast<uint32_t>(ecx6)) {
        *edi7 = *reinterpret_cast<signed char*>(&edx5);
        if (reinterpret_cast<uint32_t>(edx5) + reinterpret_cast<uint32_t>(edi7) >= reinterpret_cast<uint32_t>(&eax4->ff8)) {
            eax4->f4 = reinterpret_cast<void*>(0);
            eax4->f0 = &eax4->f8;
        } else {
            eax4->f0 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(eax4->f0) + reinterpret_cast<uint32_t>(edx5));
            eax4->f4 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax4->f4) - reinterpret_cast<uint32_t>(edx5));
        }
        return (reinterpret_cast<uint32_t>(edi7) - reinterpret_cast<uint32_t>(eax4) << 4) + reinterpret_cast<uint32_t>(eax4) + 0x80;
    }
    ecx9 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(ecx6) + reinterpret_cast<uint32_t>(edi7));
    if (*ecx9) {
        ebp8 = ecx9;
    }
    esi10 = a2;
    if (reinterpret_cast<uint32_t>(edx5) + reinterpret_cast<uint32_t>(ebp8) < reinterpret_cast<uint32_t>(&eax4->ff8)) 
        goto addr_4055f8_9;
    addr_40567e_10:
    ebp11 = &eax4->f8;
    if (reinterpret_cast<uint32_t>(edi7) <= reinterpret_cast<uint32_t>(&eax4->f8)) {
        addr_4056c8_11:
        return 0;
    } else {
        do {
            if (reinterpret_cast<uint32_t>(edx5) + reinterpret_cast<uint32_t>(ebp11) > reinterpret_cast<uint32_t>(eax4) + 0xf7) 
                goto addr_4056c8_11;
            if (*ebp11) {
                ebx12 = reinterpret_cast<void*>(0);
                *reinterpret_cast<signed char*>(&ebx12) = *ebp11;
                ebp11 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(ebp11) + reinterpret_cast<uint32_t>(ebx12));
            } else {
                ecx13 = ebp11 + 1;
                ebx14 = reinterpret_cast<void*>(1);
                if (!*ecx13) {
                    do {
                        ++ecx13;
                        ebx14 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebx14) + 1);
                    } while (!*ecx13);
                }
                if (reinterpret_cast<uint32_t>(ebx14) >= reinterpret_cast<uint32_t>(edx5)) 
                    break;
                esi10 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi10) - reinterpret_cast<uint32_t>(ebx14));
                if (reinterpret_cast<uint32_t>(edx5) > reinterpret_cast<uint32_t>(esi10)) 
                    goto addr_4056cf_19;
                ebp11 = ecx13;
            }
        } while (reinterpret_cast<uint32_t>(edi7) > reinterpret_cast<uint32_t>(ebp11));
        goto addr_4056c8_11;
    }
    esi15 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edx5) + reinterpret_cast<uint32_t>(ebp11));
    if (reinterpret_cast<uint32_t>(esi15) >= reinterpret_cast<uint32_t>(&eax4->ff8)) {
        eax4->f4 = reinterpret_cast<void*>(0);
        eax4->f0 = &eax4->f8;
    } else {
        eax4->f0 = esi15;
        eax4->f4 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebx14) - reinterpret_cast<uint32_t>(edx5));
    }
    *ebp11 = *reinterpret_cast<signed char*>(&edx5);
    return (reinterpret_cast<uint32_t>(ebp11) - reinterpret_cast<uint32_t>(eax4) << 4) + reinterpret_cast<uint32_t>(eax4) + 0x80;
    addr_4056cf_19:
    return 0;
    do {
        addr_4055f8_9:
        if (*ebp8) {
            ebx16 = reinterpret_cast<void*>(0);
            *reinterpret_cast<signed char*>(&ebx16) = *ebp8;
            ebp8 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(ebp8) + reinterpret_cast<uint32_t>(ebx16));
        } else {
            ecx17 = ebp8 + 1;
            ebx18 = reinterpret_cast<void*>(1);
            if (!*ecx17) {
                do {
                    ++ecx17;
                    ebx18 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebx18) + 1);
                } while (!*ecx17);
            }
            if (reinterpret_cast<uint32_t>(ebx18) >= reinterpret_cast<uint32_t>(edx5)) 
                goto addr_405648_30;
            if (edi7 != ebp8) {
                esi10 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi10) - reinterpret_cast<uint32_t>(ebx18));
                if (reinterpret_cast<uint32_t>(edx5) > reinterpret_cast<uint32_t>(esi10)) 
                    goto addr_405641_33;
                ebp8 = ecx17;
            } else {
                ebp8 = ecx17;
                eax4->f4 = ebx18;
            }
        }
    } while (reinterpret_cast<uint32_t>(edx5) + reinterpret_cast<uint32_t>(ebp8) < reinterpret_cast<uint32_t>(&eax4->ff8));
    goto addr_40567e_10;
    addr_405648_30:
    esi19 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edx5) + reinterpret_cast<uint32_t>(ebp8));
    if (reinterpret_cast<uint32_t>(esi19) >= reinterpret_cast<uint32_t>(&eax4->ff8)) {
        eax4->f4 = reinterpret_cast<void*>(0);
        eax4->f0 = &eax4->f8;
    } else {
        eax4->f0 = esi19;
        eax4->f4 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebx18) - reinterpret_cast<uint32_t>(edx5));
    }
    *ebp8 = *reinterpret_cast<signed char*>(&edx5);
    return (reinterpret_cast<uint32_t>(ebp8) - reinterpret_cast<uint32_t>(eax4) << 4) + reinterpret_cast<uint32_t>(eax4) + 0x80;
    addr_405641_33:
    return 0;
}

/* (image base) */
int32_t image_base_ = 0x40ae48;

struct s30 {
    signed char[16] pad16;
    signed char f10;
    signed char[1023] pad1040;
    signed char f410;
};

struct s24* fun_404fb0() {
    int1_t zf1;
    struct s24* eax2;
    struct s24* esi3;
    int32_t edi4;
    int32_t edi5;
    struct s25* eax6;
    struct s25* ebx7;
    int32_t eax8;
    struct s24* eax9;
    int1_t zf10;
    int1_t zf11;
    void* ebp12;
    struct s30* edi13;
    struct s25* edi14;
    int32_t ecx15;
    int32_t eax16;
    int32_t ebp17;

    zf1 = g40b658 == 0;
    if (!zf1) {
        eax2 = reinterpret_cast<struct s24*>(HeapAlloc());
        esi3 = eax2;
        if (!esi3) {
            goto edi4;
        }
    } else {
        esi3 = reinterpret_cast<struct s24*>(0x40ae48);
    }
    edi5 = VirtualAlloc;
    eax6 = reinterpret_cast<struct s25*>(edi5());
    ebx7 = eax6;
    if (ebx7) {
        eax8 = reinterpret_cast<int32_t>(edi5());
        if (!eax8) {
            VirtualFree(ebx7, 0, 0x8000);
        } else {
            if (!reinterpret_cast<int1_t>(esi3 == 0x40ae48)) {
                esi3->f0 = reinterpret_cast<struct s24*>(0x40ae48);
                eax9 = image_base_;
                esi3->f4 = eax9;
                image_base_ = esi3;
                esi3->f4->f0 = esi3;
            } else {
                zf10 = image_base_ == 0;
                if (zf10) {
                    image_base_ = 0x40ae48;
                }
                zf11 = image_base_ == 0;
                if (zf11) {
                    image_base_ = reinterpret_cast<struct s24*>(0x40ae48);
                }
            }
            ebp12 = reinterpret_cast<void*>(0);
            esi3->f810 = ebx7;
            esi3->f8 = reinterpret_cast<void*>(0);
            esi3->fc = reinterpret_cast<void*>(16);
            do {
                edi13 = reinterpret_cast<struct s30*>(reinterpret_cast<uint32_t>(esi3) + reinterpret_cast<uint32_t>(ebp12));
                if (reinterpret_cast<int32_t>(ebp12) >= reinterpret_cast<int32_t>(16)) {
                    edi13->f10 = -1;
                } else {
                    edi13->f10 = -16;
                }
                ebp12 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebp12) + 1);
                edi13->f410 = -15;
            } while (reinterpret_cast<int32_t>(ebp12) < reinterpret_cast<int32_t>(0x400));
            edi14 = ebx7;
            ecx15 = 0x4000;
            while (ecx15) {
                --ecx15;
                edi14->f0 = reinterpret_cast<signed char*>(0);
                edi14 = reinterpret_cast<struct s25*>(&edi14->f4);
                esi3 = reinterpret_cast<struct s24*>(&esi3->f4);
            }
            if (reinterpret_cast<uint32_t>(esi3->f810) + 0x10000 > reinterpret_cast<uint32_t>(ebx7)) {
                do {
                    ebx7->f0 = &ebx7->f8;
                    ebx7->f4 = 0xf0;
                    ebx7->ff8 = -1;
                    ebx7 = reinterpret_cast<struct s25*>(reinterpret_cast<uint32_t>(ebx7) + 0x1000);
                } while (reinterpret_cast<uint32_t>(esi3->f810) + 0x10000 > reinterpret_cast<uint32_t>(ebx7));
            }
            goto 0;
        }
    }
    if (esi3 != 0x40ae48) {
        eax16 = g40ce54;
        HeapFree(eax16, 0, esi3);
    }
    goto ebp17;
}

struct s31 {
    struct s0* f0;
    signed char[3] pad4;
    int32_t f4;
    struct s0* f8;
    signed char[3] pad12;
    uint32_t fc;
    unsigned char fd;
    uint32_t f10;
    signed char[7] pad28;
    struct s0* f1c;
};

int32_t fun_405b70(struct s31* a1);

void fun_405fc0(struct s31* a1);

int32_t fun_405ef0(uint32_t a1);

int32_t fun_405e10(struct s31* a1) {
    int32_t edi2;
    uint32_t eax3;
    int32_t eax4;
    uint32_t v5;
    int32_t eax6;
    struct s0* v7;

    edi2 = -1;
    eax3 = a1->fc;
    if (!(*reinterpret_cast<unsigned char*>(&eax3) & 64)) {
        if (*reinterpret_cast<unsigned char*>(&eax3) & 0x83) {
            eax4 = fun_405b70(a1);
            edi2 = eax4;
            fun_405fc0(a1);
            v5 = *reinterpret_cast<uint32_t*>(&a1->fd);
            eax6 = fun_405ef0(v5);
            if (eax6 >= 0) {
                if (a1->f1c) {
                    v7 = a1->f1c;
                    fun_404eb0(v7);
                    a1->f1c = reinterpret_cast<struct s0*>(0);
                }
            } else {
                edi2 = -1;
            }
        }
        a1->fc = 0;
        return edi2;
    } else {
        a1->fc = 0;
        return -1;
    }
}

uint32_t g40ce50;

struct s0* g40be40;

uint32_t fun_405b20(struct s31* a1);

uint32_t fun_405bf0(int32_t a1) {
    uint32_t ebx2;
    uint32_t edi3;
    uint32_t v4;
    int1_t less_or_equal5;
    int32_t esi6;
    void* ebp7;
    struct s0* eax8;
    uint32_t eax9;
    struct s31* v10;
    uint32_t eax11;
    struct s31* v12;
    uint32_t eax13;
    int1_t less14;
    uint32_t eax15;

    ebx2 = 0;
    edi3 = 0;
    v4 = 0;
    less_or_equal5 = reinterpret_cast<int32_t>(g40ce50) <= reinterpret_cast<int32_t>(0);
    if (less_or_equal5) {
        esi6 = a1;
    } else {
        ebp7 = reinterpret_cast<void*>(0);
        esi6 = a1;
        do {
            eax8 = g40be40;
            if (*reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax8) + reinterpret_cast<uint32_t>(ebp7)) && (eax9 = (*reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax8) + reinterpret_cast<uint32_t>(ebp7)))->fc, !!(*reinterpret_cast<unsigned char*>(&eax9) & 0x83))) {
                if (esi6 != 1) {
                    if (!esi6 && (*reinterpret_cast<unsigned char*>(&eax9) & 2 && (v10 = *reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax8) + reinterpret_cast<uint32_t>(ebp7)), eax11 = fun_405b20(v10), eax11 == 0xffffffff))) {
                        v4 = 0xffffffff;
                    }
                } else {
                    v12 = *reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax8) + reinterpret_cast<uint32_t>(ebp7));
                    eax13 = fun_405b20(v12);
                    if (eax13 != 0xffffffff) {
                        ++ebx2;
                    }
                }
            }
            ebp7 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebp7) + 4);
            ++edi3;
            less14 = reinterpret_cast<int32_t>(edi3) < reinterpret_cast<int32_t>(g40ce50);
        } while (less14);
    }
    eax15 = ebx2;
    if (esi6 != 1) {
        eax15 = v4;
    }
    return eax15;
}

int32_t FlushFileBuffers = 0xd900;

uint32_t fun_405e80(uint32_t a1) {
    int1_t below_or_equal2;
    uint32_t eax3;
    int32_t eax4;
    int32_t eax5;

    below_or_equal2 = g40cf60 <= a1;
    if (below_or_equal2 || !(*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(*reinterpret_cast<void**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60)) + (a1 & 31) * 8 + 4) & 1)) {
        g40a578 = 9;
        eax3 = 0xffffffff;
    } else {
        eax4 = fun_4059b0(a1);
        eax5 = reinterpret_cast<int32_t>(FlushFileBuffers());
        eax3 = 0;
        if (!eax5) {
            eax3 = reinterpret_cast<uint32_t>(GetLastError());
        }
        if (eax3) {
            g40a578 = 9;
            g40a57c = eax3;
            goto eax4;
        }
    }
    return eax3;
}

uint32_t fun_405b20(struct s31* a1) {
    int32_t eax2;
    uint32_t v3;
    uint32_t eax4;
    uint32_t eax5;

    if (a1) {
        eax2 = fun_405b70(a1);
        if (!eax2) {
            if (!(*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(a1) + 13) & 64)) {
                return 0;
            } else {
                v3 = *reinterpret_cast<uint32_t*>(&a1->fd);
                eax4 = fun_405e80(v3);
                return 1 - reinterpret_cast<uint1_t>(eax4 < 1);
            }
        } else {
            return 0xffffffff;
        }
    } else {
        eax5 = fun_405bf0(0);
        return eax5;
    }
}

int32_t fun_405b70(struct s31* a1) {
    int32_t edi2;
    uint32_t ecx3;
    struct s0* eax4;
    void* ebx5;
    uint32_t v6;
    void* eax7;
    uint32_t eax8;

    edi2 = 0;
    ecx3 = a1->fc;
    if ((*reinterpret_cast<unsigned char*>(&ecx3) & 3) == 2 && (a1->fc & 0x108 && (eax4 = a1->f8, ebx5 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(a1->f0) - reinterpret_cast<unsigned char>(eax4)), !(reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(ebx5) < reinterpret_cast<int32_t>(0)) | reinterpret_cast<uint1_t>(ebx5 == 0))))) {
        v6 = *reinterpret_cast<uint32_t*>(&a1->fd);
        eax7 = fun_404790(v6, eax4, ebx5);
        if (eax7 != ebx5) {
            a1->fc = a1->fc | 32;
            edi2 = -1;
        } else {
            eax8 = a1->fc;
            if (*reinterpret_cast<unsigned char*>(&eax8) & 0x80) {
                a1->fc = eax8 & 0xfffffffd;
            }
        }
    }
    a1->f0 = a1->f8;
    a1->f4 = 0;
    return edi2;
}

void fun_405fc0(struct s31* a1) {
    uint32_t eax2;
    struct s0* v3;

    eax2 = a1->fc;
    if (*reinterpret_cast<unsigned char*>(&eax2) & 0x83 && *reinterpret_cast<unsigned char*>(&eax2) & 8) {
        v3 = a1->f8;
        fun_404eb0(v3);
        a1->f0 = reinterpret_cast<struct s0*>(0);
        a1->fc = a1->fc & 0xfffffbf7;
        a1->f8 = reinterpret_cast<struct s0*>(0);
        a1->f4 = 0;
    }
    return;
}

int32_t CloseHandle = 0xd914;

int32_t fun_405920(uint32_t a1);

int32_t fun_405ef0(uint32_t a1) {
    int1_t below_or_equal2;
    void** ebx3;
    uint32_t esi4;
    int32_t eax5;
    int32_t eax6;
    int32_t eax7;
    int32_t eax8;
    uint32_t ebp9;
    uint32_t eax10;

    below_or_equal2 = g40cf60 <= a1;
    if (below_or_equal2 || (ebx3 = reinterpret_cast<void**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60), esi4 = (a1 & 31) * 8, (*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(*ebx3) + esi4 + 4) & 1) == 0)) {
        g40a578 = 9;
        g40a57c = 0;
        return -1;
    } else {
        if ((a1 == 1 || a1 == 2) && (eax5 = fun_4059b0(2), eax6 = fun_4059b0(1), eax5 == eax6) || (eax7 = fun_4059b0(a1), eax8 = reinterpret_cast<int32_t>(CloseHandle(eax7)), !!eax8)) {
            ebp9 = 0;
        } else {
            eax10 = reinterpret_cast<uint32_t>(GetLastError(eax7));
            ebp9 = eax10;
        }
        fun_405920(a1);
        if (!ebp9) {
            *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(*ebx3) + esi4 + 4) = 0;
            return 0;
        } else {
            fun_4058b0(ebp9);
            return -1;
        }
    }
}

void* g40b7f8 = reinterpret_cast<void*>(0);

int32_t LCMapStringA = 0xd922;

int32_t LCMapStringW = 0xd932;

void* fun_406300(signed char* a1, void* a2);

void* fun_4060d0(struct s0* a1, int32_t a2, signed char* a3, void* a4, struct s0* a5, void* a6, struct s0* a7) {
    void* eax8;
    int32_t edi9;
    int32_t eax10;
    int32_t eax11;
    void* esi12;
    void* eax13;
    struct s0* edi14;
    int32_t eax15;
    struct s0* eax16;
    struct s0* v17;
    int32_t eax18;
    struct s0* eax19;
    struct s0* eax20;
    int32_t eax21;
    int32_t ebp22;
    struct s0* eax23;
    int32_t ebp24;
    struct s0* eax25;
    int32_t eax26;

    eax8 = g40b7f8;
    if (eax8) {
        edi9 = LCMapStringA;
    } else {
        edi9 = LCMapStringA;
        eax10 = reinterpret_cast<int32_t>(edi9(0, 0x100, 0x40a4fc, 1, 0, 0));
        if (!eax10) {
            eax11 = reinterpret_cast<int32_t>(LCMapStringW());
            if (!eax11) {
                goto 0;
            } else {
                eax8 = reinterpret_cast<void*>(1);
            }
        } else {
            eax8 = reinterpret_cast<void*>(2);
        }
    }
    esi12 = a4;
    g40b7f8 = eax8;
    if (!(reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(esi12) < reinterpret_cast<int32_t>(0)) | reinterpret_cast<uint1_t>(esi12 == 0))) {
        eax13 = fun_406300(a3, esi12);
        esi12 = eax13;
        eax8 = g40b7f8;
    }
    g40b7f8 = eax8;
    if (reinterpret_cast<int1_t>(eax8 == 2)) {
        edi9();
        goto a5;
    }
    g40b7f8 = eax8;
    if (!reinterpret_cast<int1_t>(eax8 == 1)) {
        addr_40625f_13:
        return eax8;
    } else {
        edi14 = reinterpret_cast<struct s0*>(0);
        if (!a7) {
        }
        eax15 = reinterpret_cast<int32_t>(MultiByteToWideChar());
        if (!eax15) {
            goto 0;
        }
        eax16 = fun_404f00(eax15 * 2);
        if (!eax16) {
            goto 0;
        }
        v17 = a1;
        eax18 = reinterpret_cast<int32_t>(MultiByteToWideChar());
        if (!eax18) 
            goto addr_40624b_21;
        eax19 = reinterpret_cast<struct s0*>(LCMapStringW());
        if (!eax19) 
            goto addr_40624b_21;
        if (*reinterpret_cast<unsigned char*>(&v17 + 1) & 4) 
            goto addr_40621b_24;
    }
    eax20 = fun_404f00(reinterpret_cast<unsigned char>(eax19) * 2);
    edi14 = eax20;
    if (!edi14) 
        goto addr_40624b_21;
    eax21 = reinterpret_cast<int32_t>(LCMapStringW(0, v17, eax16, eax15, edi14, eax19));
    if (!eax21) 
        goto addr_40624b_21;
    if (!0) 
        goto addr_40629e_28;
    ebp22 = WideCharToMultiByte;
    eax23 = reinterpret_cast<struct s0*>(ebp22(0, 0x220, edi14, eax19, eax15, 0, 0, 0, 0, v17, eax16, eax15, edi14, eax19));
    if (!eax23) {
        addr_40624b_21:
        fun_404eb0(eax16);
        fun_404eb0(edi14);
        eax8 = reinterpret_cast<void*>(0);
        goto addr_40625f_13;
    } else {
        addr_4062e2_30:
        fun_404eb0(eax16);
        fun_404eb0(edi14);
        goto 0;
    }
    addr_40629e_28:
    ebp24 = WideCharToMultiByte;
    eax25 = reinterpret_cast<struct s0*>(ebp24(0, 0x220, edi14, eax19, 0, 0, 0, 0, 0, v17, eax16, eax15, edi14, eax19));
    if (eax25) 
        goto addr_4062e2_30;
    goto addr_40624b_21;
    addr_40621b_24:
    if (!eax16) 
        goto addr_4062e2_30;
    if (reinterpret_cast<signed char>(eax19) > reinterpret_cast<signed char>(eax16)) 
        goto addr_40624b_21;
    eax26 = reinterpret_cast<int32_t>(LCMapStringW(0, v17, eax16, eax15, esi12, eax16));
    if (eax26) 
        goto addr_4062e2_30; else 
        goto addr_40624b_21;
}

void* fun_406300(signed char* a1, void* a2) {
    signed char* ecx3;
    signed char* esi4;
    void* eax5;
    uint32_t edx6;
    uint32_t edi7;

    ecx3 = a1;
    esi4 = ecx3;
    eax5 = a2;
    edx6 = reinterpret_cast<uint32_t>(eax5) + 0xffffffff;
    if (eax5) {
        do {
            if (!*esi4) 
                goto addr_406325_3;
            ++esi4;
            edi7 = edx6;
            --edx6;
        } while (edi7);
    }
    if (*esi4) {
        addr_406329_6:
        return eax5;
    } else {
        addr_406325_3:
        eax5 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi4) - reinterpret_cast<uint32_t>(ecx3));
        goto addr_406329_6;
    }
}

void fun_402820(int32_t ecx, void* a2) {
    int32_t v3;
    void* ecx4;
    uint32_t eax5;
    uint32_t eax6;
    int32_t* esp7;

    v3 = reinterpret_cast<int32_t>(__return_address());
    ecx4 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 + 8);
    if (eax5 >= 0x1000) {
        do {
            ecx4 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(ecx4) - 0x1000);
            eax6 = eax6 - 0x1000;
        } while (eax6 >= 0x1000);
    }
    esp7 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(ecx4) - eax6 - 4);
    *esp7 = v3;
    goto *esp7;
}

int32_t g40a7fc = 1;

uint32_t fun_4026e0(struct s3* a1) {
    struct s3* esi2;
    int1_t less_or_equal3;
    int32_t edx4;
    int16_t* ecx5;
    uint32_t eax6;
    uint32_t eax7;
    int32_t eax8;
    int32_t ebx9;
    signed char* esi10;
    int32_t edi11;
    uint32_t ebp12;
    int1_t less_or_equal13;
    int16_t* ecx14;
    uint32_t eax15;
    uint32_t eax16;

    esi2 = a1;
    while (1) {
        less_or_equal3 = g40a7fc <= 1;
        if (less_or_equal3) {
            edx4 = 0;
            ecx5 = image_base_;
            *reinterpret_cast<signed char*>(&edx4) = esi2->f0;
            eax6 = 0;
            *reinterpret_cast<int16_t*>(&eax6) = ecx5[edx4];
            eax7 = eax6 & 8;
        } else {
            eax8 = 0;
            *reinterpret_cast<signed char*>(&eax8) = esi2->f0;
            eax7 = fun_403630(eax8, 8);
        }
        if (!eax7) 
            break;
        esi2 = reinterpret_cast<struct s3*>(&esi2->f1);
    }
    ebx9 = 0;
    *reinterpret_cast<signed char*>(&ebx9) = esi2->f0;
    esi10 = &esi2->f1;
    edi11 = ebx9;
    if (ebx9 == 45 || ebx9 == 43) {
        ebx9 = 0;
        *reinterpret_cast<signed char*>(&ebx9) = *esi10;
        ++esi10;
    }
    ebp12 = 0;
    while (1) {
        less_or_equal13 = g40a7fc <= 1;
        if (less_or_equal13) {
            ecx14 = image_base_;
            eax15 = 0;
            *reinterpret_cast<int16_t*>(&eax15) = ecx14[ebx9];
            eax16 = eax15 & 4;
        } else {
            eax16 = fun_403630(ebx9, 4);
        }
        if (!eax16) 
            break;
        ++esi10;
        ebp12 = ebx9 + (ebp12 + ebp12 * 4) * 2 + 0xffffffd0;
        ebx9 = 0;
        *reinterpret_cast<signed char*>(&ebx9) = *(esi10 - 1);
    }
    if (edi11 != 45) {
        return ebp12;
    } else {
        return -ebp12;
    }
}

int32_t DirectXSetupA = 0xd6b0;

uint32_t fun_40242c(void* a1, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9, void* a10, void* a11) {
    goto DirectXSetupA;
}

struct s32 {
    signed char f0;
    signed char f1;
};

void fun_403440(int32_t ecx);

void fun_403468();

void fun_4034dc(uint32_t ecx);

/* (image base) */
int32_t image_base_ = 0x405c80;

/* (image base) */
int32_t image_base_ = 0x405c80;

/* (image base) */
int32_t image_base_ = 0x405c80;

/* (image base) */
struct s5* image_base_ = reinterpret_cast<struct s5*>(0x746c);

/* (image base) */
struct s5* image_base_ = reinterpret_cast<struct s5*>(0x745c);

int32_t fun_402b70(struct s5* a1, struct s4* a2, void* a3) {
    int32_t v4;
    struct s32* v5;
    signed char bl6;
    void* esp7;
    int32_t v8;
    void* esi9;
    void* v10;
    struct s5* ebp11;
    struct s5* v12;
    struct s5* edi13;
    struct s5* v14;
    uint32_t eax15;
    uint32_t eax16;
    int32_t ecx17;
    int32_t eax18;
    int16_t* ecx19;
    void* v20;
    struct s5* edx21;
    int32_t v22;
    int32_t v23;
    struct s5* v24;
    struct s5* v25;
    uint32_t ecx26;
    int32_t eax27;
    struct s5* eax28;
    struct s5* eax29;
    uint32_t ecx30;
    struct s32* eax31;
    int32_t eax32;
    uint32_t ecx33;
    int32_t eax34;
    int32_t** esp35;
    int32_t* v36;
    struct s5* eax37;
    signed char v38;
    int32_t* eax39;
    int32_t** esp40;
    int32_t* v41;
    struct s5* eax42;
    struct s5* v43;
    int32_t* esp44;
    struct s5* eax45;
    uint32_t edi46;
    int32_t ecx47;
    void* eax48;
    signed char v49;
    struct s5* ebx50;
    struct s5* eax51;
    struct s5* eax52;
    struct s5* edi53;
    struct s5* ebx54;
    struct s5* eax55;
    struct s5* eax56;
    struct s5* edi57;
    struct s5* ebx58;
    struct s5* eax59;
    struct s5* eax60;
    struct s5* ecx61;
    struct s5* eax62;
    int32_t ecx63;
    struct s5* edi64;
    void* eax65;
    struct s5* v66;
    struct s5* eax67;
    struct s5* v68;
    int32_t** esp69;
    struct s5* ebx70;
    struct s5* v71;
    struct s5* eax72;
    struct s5* v73;
    struct s5* eax74;
    struct s5** esp75;
    struct s5* ecx76;
    int32_t** esp77;
    int32_t* v78;
    struct s5* eax79;
    struct s5* eax80;
    struct s5* v81;
    struct s5* v82;
    int32_t** esp83;
    int32_t* v84;
    struct s5* eax85;
    struct s5* eax86;
    struct s5* eax87;
    struct s5* v88;
    struct s5* v89;
    void* v90;
    struct s5* eax91;
    struct s5* edi92;
    uint32_t eax93;
    struct s5* eax94;

    v4 = reinterpret_cast<int32_t>(__return_address());
    v5 = reinterpret_cast<struct s32*>(&a2->f1);
    bl6 = a2->f0;
    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x248 - 4 - 4 - 4 - 4);
    v8 = 0;
    if (bl6) {
        esi9 = v10;
        ebp11 = v12;
        edi13 = v14;
        while (!0) {
            if (bl6 < 32 || bl6 > 0x78) {
                eax15 = 0;
            } else {
                eax16 = 0;
                *reinterpret_cast<signed char*>(&eax16) = *reinterpret_cast<signed char*>(bl6 + 0x407458);
                eax15 = eax16 & 15;
            }
            ecx17 = *reinterpret_cast<signed char*>(v8 + eax15 * 8 + 0x407478) >> 4;
            v8 = ecx17;
            switch (ecx17) {
                addr_402d43_9:
            case 0:
                eax18 = 0;
                ecx19 = image_base_;
                *reinterpret_cast<signed char*>(&eax18) = bl6;
                v20 = reinterpret_cast<void*>(0);
                if (*reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(ecx19 + eax18) + 1) & 0x80) {
                    fun_403520(static_cast<int32_t>(bl6), a1, reinterpret_cast<int32_t>(esp7) + 36);
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 + 4 + 12);
                    bl6 = v5->f0;
                    v5 = reinterpret_cast<struct s32*>(&v5->f1);
                }
                edx21 = reinterpret_cast<struct s5*>(static_cast<int32_t>(bl6));
                fun_403520(edx21, a1, reinterpret_cast<int32_t>(esp7) + 36);
                esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 + 4 + 12);
                goto addr_402d9e_12;
            case 1:
                v22 = 0;
                esi9 = reinterpret_cast<void*>(0);
                ebp11 = reinterpret_cast<struct s5*>(0xffffffff);
                v23 = 0;
                v24 = reinterpret_cast<struct s5*>(0);
                v25 = reinterpret_cast<struct s5*>(0);
                v20 = reinterpret_cast<void*>(0);
                goto addr_402d9e_12;
            case 2:
                ecx26 = bl6 - 32;
                if (ecx26 <= 16) {
                    eax27 = 0;
                    *reinterpret_cast<signed char*>(&eax27) = *reinterpret_cast<signed char*>(ecx26 + reinterpret_cast<int32_t>(fun_403440));
                    switch (eax27) {
                    case 0:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 2);
                        break;
                    case 1:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x80);
                        break;
                    case 2:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 1);
                        break;
                    case 3:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 4);
                        break;
                    case 4:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 8);
                    case 5:
                        goto 0x402d9e;
                    }
                    goto addr_402d9e_12;
                }
            case 3:
                if (bl6 != 42) {
                    edx21 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(v24) + reinterpret_cast<uint16_t>(v24) * 4);
                    v24 = reinterpret_cast<struct s5*>(bl6 + reinterpret_cast<uint16_t>(edx21) * 2 + 0xffffffd0);
                    goto addr_402d9e_12;
                } else {
                    eax28 = fun_4035f0(reinterpret_cast<int32_t>(esp7) + 0x264);
                    v24 = eax28;
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                    if (reinterpret_cast<int16_t>(eax28) < reinterpret_cast<int16_t>(0)) {
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 4);
                        v24 = reinterpret_cast<struct s5*>(-reinterpret_cast<uint16_t>(eax28));
                        goto addr_402d9e_12;
                    }
                }
            case 4:
                ebp11 = reinterpret_cast<struct s5*>(0);
                goto addr_402d9e_12;
            case 5:
                if (bl6 != 42) {
                    ebp11 = reinterpret_cast<struct s5*>(bl6 + (reinterpret_cast<uint16_t>(ebp11) + reinterpret_cast<uint16_t>(ebp11) * 4) * 2 + 0xffffffd0);
                    goto addr_402d9e_12;
                } else {
                    eax29 = fun_4035f0(reinterpret_cast<int32_t>(esp7) + 0x264);
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                    ebp11 = eax29;
                    if (reinterpret_cast<int16_t>(ebp11) < reinterpret_cast<int16_t>(0)) {
                        ebp11 = reinterpret_cast<struct s5*>(0xffffffff);
                        goto addr_402d9e_12;
                    }
                }
            case 6:
                ecx30 = bl6 - 73;
                if (ecx30 > 46) {
                    addr_402d9e_12:
                    eax31 = v5;
                    v5 = reinterpret_cast<struct s32*>(&v5->f1);
                    bl6 = eax31->f0;
                    if (bl6) 
                        break; else 
                        goto addr_402db6_31;
                } else {
                    eax32 = 0;
                    *reinterpret_cast<signed char*>(&eax32) = *reinterpret_cast<signed char*>(ecx30 + reinterpret_cast<int32_t>(fun_403468));
                    switch (eax32) {
                    case 0:
                        if (v5->f0 != 54 || v5->f1 != 52) {
                            v8 = 0;
                            goto addr_402d43_9;
                        } else {
                            ++v5;
                            esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x8000);
                            break;
                        }
                    case 1:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 32);
                        break;
                    case 2:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 16);
                        break;
                    case 3:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x800);
                    case 4:
                        goto 0x402d9e;
                    }
                    goto addr_402d9e_12;
                }
            case 7:
                ecx33 = bl6 - 67;
                if (ecx33 > 53) {
                    addr_4032a3_40:
                    if (v23) 
                        goto addr_402d9e_12;
                } else {
                    eax34 = 0;
                    *reinterpret_cast<signed char*>(&eax34) = *reinterpret_cast<signed char*>(ecx33 + reinterpret_cast<int32_t>(fun_4034dc));
                    switch (eax34) {
                    case 0:
                        if (!(reinterpret_cast<uint32_t>(esi9) & 0x830)) {
                            esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x800);
                        }
                    case 6:
                        esp35 = reinterpret_cast<int32_t**>(reinterpret_cast<int32_t>(esp7) - 4);
                        v36 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esp7) + 0x264);
                        if (!(reinterpret_cast<uint32_t>(esi9) & 0x810)) {
                            edi13 = reinterpret_cast<struct s5*>(1);
                            eax37 = fun_4035f0(v36);
                            v38 = *reinterpret_cast<signed char*>(&eax37);
                            esp7 = reinterpret_cast<void*>(esp35 - 1 + 1 + 1);
                        } else {
                            eax39 = fun_403620(v36);
                            esp40 = esp35 - 1 + 1 + 1 - 1;
                            v41 = eax39;
                            eax42 = fun_404bf0(esp40 + 23, *reinterpret_cast<uint16_t*>(&v41));
                            esp7 = reinterpret_cast<void*>(esp40 - 1 - 1 + 1 + 2);
                            edi13 = eax42;
                            if (reinterpret_cast<int16_t>(edi13) < reinterpret_cast<int16_t>(0)) {
                                v23 = 1;
                            }
                        }
                        v43 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(esp7) + 88);
                        break;
                    case 1:
                    case 2:
                        v22 = 1;
                        bl6 = reinterpret_cast<signed char>(bl6 + 32);
                    case 8:
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 64);
                        v43 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(esp7) + 88);
                        if (reinterpret_cast<int16_t>(ebp11) >= reinterpret_cast<int16_t>(0)) {
                            if (!ebp11 && bl6 == 0x67) {
                                ebp11 = reinterpret_cast<struct s5*>(1);
                            }
                        } else {
                            ebp11 = reinterpret_cast<struct s5*>(6);
                        }
                        edx21 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(esp7) + 88);
                        esp44 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4);
                        eax45 = reinterpret_cast<struct s5*>(image_base_(esp44 + 23, edx21, static_cast<int32_t>(bl6), ebp11, v22));
                        esp7 = reinterpret_cast<void*>(esp44 - 1 - 1 - 1 + 1 + 5);
                        edi46 = reinterpret_cast<uint32_t>(esi9) & 0x80;
                        if (edi46 && !ebp11) {
                            eax45 = reinterpret_cast<struct s5*>(image_base_(reinterpret_cast<int32_t>(esp7) + 88));
                            esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                        }
                        if (bl6 == 0x67 && !edi46) {
                            eax45 = reinterpret_cast<struct s5*>(image_base_(reinterpret_cast<int32_t>(esp7) + 88));
                            esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                        }
                        if (v38 == 45) {
                            esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x100);
                            eax45 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(esp7) + 89);
                            v43 = eax45;
                        }
                        ecx47 = -1;
                        eax48 = reinterpret_cast<void*>(reinterpret_cast<uint16_t>(eax45) - reinterpret_cast<uint16_t>(eax45));
                        do {
                            if (!ecx47) 
                                break;
                            --ecx47;
                            esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) + 1);
                        } while (v49 != *reinterpret_cast<signed char*>(&eax48));
                        edi13 = reinterpret_cast<struct s5*>(~ecx47 + 0xffffffff);
                        break;
                    case 3:
                        if (!(reinterpret_cast<uint32_t>(esi9) & 0x830)) {
                            esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x800);
                        }
                    case 13:
                        ebx50 = reinterpret_cast<struct s5*>(0x7fffffff);
                        if (ebp11 != 0xffffffff) {
                            ebx50 = ebp11;
                        }
                        eax51 = fun_4035f0(reinterpret_cast<int32_t>(esp7) + 0x264);
                        v43 = eax51;
                        esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                        if (!(reinterpret_cast<uint32_t>(esi9) & 0x810)) {
                            if (!v43) {
                                eax52 = image_base_;
                                v43 = eax52;
                            }
                            edi53 = v43;
                            ebx54 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ebx50) - 1);
                            if (ebx50) {
                                do {
                                    if (!*reinterpret_cast<struct s0**>(&edi53->f0)) 
                                        break;
                                    edi53 = reinterpret_cast<struct s5*>(&edi53->pad4);
                                    eax55 = ebx54;
                                    ebx54 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ebx54) - 1);
                                } while (eax55);
                            }
                            edi13 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(edi53) - reinterpret_cast<uint16_t>(v43));
                            break;
                        } else {
                            if (!eax51) {
                                eax56 = image_base_;
                                v43 = eax56;
                            }
                            edi57 = v43;
                            v20 = reinterpret_cast<void*>(1);
                            ebx58 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ebx50) - 1);
                            if (ebx50) {
                                do {
                                    if (!*reinterpret_cast<struct s0**>(&edi57->f0)) 
                                        break;
                                    edi57 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(edi57) + 2);
                                    eax59 = ebx58;
                                    ebx58 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ebx58) - 1);
                                } while (eax59);
                            }
                            edi13 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(reinterpret_cast<uint16_t>(edi57) - reinterpret_cast<uint16_t>(v43)) >> 1);
                            break;
                        }
                        addr_402fa8_81:
                    case 4:
                        goto addr_402fb0_82;
                    case 5:
                        eax60 = fun_4035f0(reinterpret_cast<int32_t>(esp7) + 0x264);
                        esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                        if (!eax60 || (ecx61 = eax60->f4, ecx61 == 0)) {
                            eax62 = image_base_;
                            ecx63 = -1;
                            edi64 = eax62;
                            v43 = eax62;
                            eax65 = reinterpret_cast<void*>(reinterpret_cast<uint16_t>(eax62) - reinterpret_cast<uint16_t>(eax62));
                            do {
                                if (!ecx63) 
                                    break;
                                --ecx63;
                                edi64 = reinterpret_cast<struct s5*>(&edi64->pad4);
                                esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) + 1);
                            } while (*reinterpret_cast<struct s0**>(&edi64->f0) != *reinterpret_cast<struct s0**>(&eax65));
                            edi13 = reinterpret_cast<struct s5*>(~ecx63 + 0xffffffff);
                            break;
                        } else {
                            if (!(reinterpret_cast<uint32_t>(esi9) & 0x800)) {
                                v20 = reinterpret_cast<void*>(0);
                                edi13 = reinterpret_cast<struct s5*>(static_cast<int32_t>(reinterpret_cast<int16_t>(*reinterpret_cast<struct s0**>(&eax60->f0))));
                                v43 = ecx61;
                                break;
                            } else {
                                v20 = reinterpret_cast<void*>(1);
                                edi13 = reinterpret_cast<struct s5*>(reinterpret_cast<uint32_t>(static_cast<int32_t>(reinterpret_cast<int16_t>(*reinterpret_cast<struct s0**>(&eax60->f0)))) >> 1);
                                v43 = ecx61;
                                break;
                            }
                        }
                    case 7:
                    case 9:
                        v66 = reinterpret_cast<struct s5*>(10);
                        esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 64);
                        goto addr_402fd7_92;
                    case 10:
                        eax67 = fun_4035f0(reinterpret_cast<int32_t>(esp7) + 0x264);
                        esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                        if (!(reinterpret_cast<uint32_t>(esi9) & 32)) {
                            *reinterpret_cast<struct s0**>(&eax67->f0) = reinterpret_cast<struct s0*>(0);
                        } else {
                            *reinterpret_cast<struct s0**>(&eax67->f0) = reinterpret_cast<struct s0*>(0);
                        }
                        v23 = 1;
                        break;
                    case 11:
                        v66 = reinterpret_cast<struct s5*>(8);
                        if (reinterpret_cast<uint32_t>(esi9) & 0x80) {
                            esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x200);
                            goto addr_402fd7_92;
                        }
                    case 12:
                        ebp11 = reinterpret_cast<struct s5*>(8);
                        goto addr_402fa8_81;
                    case 14:
                        v66 = reinterpret_cast<struct s5*>(10);
                        goto addr_402fd7_92;
                    case 15:
                        goto addr_402fb0_82;
                    case 16:
                        goto 0x4032a3;
                    }
                    goto addr_4032a3_40;
                }
                if (!(reinterpret_cast<uint32_t>(esi9) & 64)) 
                    goto addr_4032e9_103;
                if (!(reinterpret_cast<uint32_t>(esi9) & 0x100)) {
                    if (!(reinterpret_cast<uint32_t>(esi9) & 1)) {
                        if (!(reinterpret_cast<uint32_t>(esi9) & 2)) {
                            addr_4032e9_103:
                            v68 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(v24) - reinterpret_cast<uint16_t>(edi13) - reinterpret_cast<uint16_t>(v25));
                            if (!(reinterpret_cast<uint32_t>(esi9) & 12)) {
                                fun_403570(32, v68, a1, reinterpret_cast<int32_t>(esp7) + 36);
                                esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 - 4 + 4 + 16);
                            }
                        } else {
                            goto addr_4032e1_109;
                        }
                    } else {
                        goto addr_4032e1_109;
                    }
                } else {
                    goto addr_4032e1_109;
                }
                edx21 = v25;
                esp69 = reinterpret_cast<int32_t**>(reinterpret_cast<int32_t>(esp7) - 4);
                fun_4035b0(reinterpret_cast<int32_t>(esp69) + 22, edx21, a1, reinterpret_cast<int32_t>(esp7) + 36);
                esp7 = reinterpret_cast<void*>(esp69 - 1 - 1 - 1 - 1 + 1 + 4);
                if (reinterpret_cast<uint32_t>(esi9) & 8 && !(reinterpret_cast<uint32_t>(esi9) & 4)) {
                    edx21 = v68;
                    fun_403570(48, edx21, a1, reinterpret_cast<int32_t>(esp7) + 36);
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 - 4 + 4 + 16);
                }
                if (!v20 || reinterpret_cast<uint1_t>(reinterpret_cast<int16_t>(edi13) < reinterpret_cast<int16_t>(0)) | reinterpret_cast<uint1_t>(edi13 == 0)) {
                    edx21 = v43;
                    fun_4035b0(edx21, edi13, a1, reinterpret_cast<int32_t>(esp7) + 36);
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 - 4 + 4 + 16);
                } else {
                    ebx70 = v43;
                    v71 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(edi13) + 0xffffffff);
                    do {
                        eax72 = ebx70;
                        *reinterpret_cast<struct s0**>(&eax72) = *reinterpret_cast<struct s0**>(&eax72->f0);
                        ebx70 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ebx70) + 2);
                        v73 = eax72;
                        eax74 = fun_404bf0(reinterpret_cast<int32_t>(esp7) + 20, *reinterpret_cast<uint16_t*>(&v73));
                        esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 + 4 + 8);
                        if (reinterpret_cast<uint1_t>(reinterpret_cast<int16_t>(eax74) < reinterpret_cast<int16_t>(0)) | reinterpret_cast<uint1_t>(eax74 == 0)) 
                            break;
                        edx21 = a1;
                        esp75 = reinterpret_cast<struct s5**>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4);
                        fun_4035b0(esp75 + 16, eax74, edx21, reinterpret_cast<int32_t>(esp7) + 36);
                        ecx76 = v71;
                        v71 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(v71) - 1);
                        esp7 = reinterpret_cast<void*>(esp75 - 2 - 2 + 2 + 8);
                    } while (ecx76);
                    goto addr_4033bd_119;
                }
                addr_4033da_120:
                if (reinterpret_cast<uint32_t>(esi9) & 4) {
                    edx21 = v68;
                    fun_403570(32, edx21, a1, reinterpret_cast<int32_t>(esp7) + 36);
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 - 4 + 4 + 16);
                    goto addr_402d9e_12;
                }
                addr_4033bd_119:
                goto addr_4033da_120;
                addr_4032e1_109:
                v25 = reinterpret_cast<struct s5*>(1);
                goto addr_4032e9_103;
                addr_402fd7_92:
                if (!(reinterpret_cast<uint32_t>(esi9) & 0x8000)) {
                    if (!(reinterpret_cast<uint32_t>(esi9) & 32)) {
                        esp77 = reinterpret_cast<int32_t**>(reinterpret_cast<int32_t>(esp7) - 4);
                        v78 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esp7) + 0x264);
                        if (!(reinterpret_cast<uint32_t>(esi9) & 64)) {
                            eax79 = fun_4035f0(v78);
                            esp7 = reinterpret_cast<void*>(esp77 - 1 + 1 + 1);
                        } else {
                            eax80 = fun_4035f0(v78);
                            v81 = eax80;
                            esp7 = reinterpret_cast<void*>(esp77 - 1 + 1 + 1);
                            __asm__("cdq ");
                            v82 = edx21;
                            goto addr_403082_126;
                        }
                    } else {
                        esp83 = reinterpret_cast<int32_t**>(reinterpret_cast<int32_t>(esp7) - 4);
                        v84 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esp7) + 0x264);
                        if (!(reinterpret_cast<uint32_t>(esi9) & 64)) {
                            eax85 = fun_4035f0(v84);
                            eax79 = reinterpret_cast<struct s5*>(static_cast<uint32_t>(*reinterpret_cast<uint16_t*>(&eax85)));
                            esp7 = reinterpret_cast<void*>(esp83 - 1 + 1 + 1);
                        } else {
                            eax86 = fun_4035f0(v84);
                            v81 = reinterpret_cast<struct s5*>(static_cast<int32_t>(*reinterpret_cast<int16_t*>(&eax86)));
                            esp7 = reinterpret_cast<void*>(esp83 - 1 + 1 + 1);
                            __asm__("cdq ");
                            v82 = edx21;
                            goto addr_403082_126;
                        }
                    }
                } else {
                    eax87 = fun_403600(reinterpret_cast<int32_t>(esp7) + 0x264);
                    v81 = eax87;
                    v82 = edx21;
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 + 4 + 4);
                    goto addr_403082_126;
                }
                v81 = eax79;
                v82 = reinterpret_cast<struct s5*>(0);
                addr_403082_126:
                if (!(reinterpret_cast<uint32_t>(esi9) & 64) || (reinterpret_cast<int16_t>(v82) > reinterpret_cast<int16_t>(0) || reinterpret_cast<int16_t>(v82) >= reinterpret_cast<int16_t>(0) && reinterpret_cast<uint16_t>(v81) >= reinterpret_cast<uint16_t>(0))) {
                    v88 = v81;
                    v89 = v82;
                } else {
                    v88 = reinterpret_cast<struct s5*>(-reinterpret_cast<uint16_t>(v81));
                    esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) | 0x100);
                    v89 = reinterpret_cast<struct s5*>(-(reinterpret_cast<uint16_t>(&v82->f0) + reinterpret_cast<uint1_t>(!!v81)));
                }
                if (!(reinterpret_cast<uint32_t>(esi9) & 0x8000)) {
                    v88 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(v88) & 0xffffffff);
                    v89 = reinterpret_cast<struct s5*>(0);
                }
                if (reinterpret_cast<int16_t>(ebp11) >= reinterpret_cast<int16_t>(0)) {
                    esi9 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi9) & 0xfffffff7);
                } else {
                    ebp11 = reinterpret_cast<struct s5*>(1);
                }
                if (!v89 && !v88) {
                    v25 = reinterpret_cast<struct s5*>(0);
                }
                v90 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) + 0x257);
                while ((eax91 = ebp11, ebp11 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(ebp11) - 1), !reinterpret_cast<uint1_t>(reinterpret_cast<uint1_t>(reinterpret_cast<int16_t>(eax91) < reinterpret_cast<int16_t>(0)) | reinterpret_cast<uint1_t>(eax91 == 0))) || (v89 || v88)) {
                    __asm__("cdq ");
                    edi92 = edx21;
                    eax93 = fun_404d00(v88, v89, v66, edi92);
                    edx21 = v66;
                    eax94 = fun_404c90(v88, v89, edx21, edi92);
                    esp7 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp7) - 4 - 4 - 4 - 4 - 4 + 16 + 4 - 4 - 4 - 4 - 4 - 4 + 16 + 4);
                    v88 = eax94;
                    v89 = edx21;
                    if (reinterpret_cast<int32_t>(eax93 + 48) > reinterpret_cast<int32_t>(57)) {
                    }
                    v90 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(v90) - 1);
                }
                edi13 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(esp7) + 0x257 - reinterpret_cast<uint32_t>(v90));
                v43 = reinterpret_cast<struct s5*>(reinterpret_cast<uint32_t>(v90) + 1);
                if (reinterpret_cast<uint32_t>(esi9) & 0x200 && (*reinterpret_cast<signed char*>(&v4) != 48 || !edi13)) {
                    edi13 = reinterpret_cast<struct s5*>(&edi13->pad4);
                    v43 = reinterpret_cast<struct s5*>(reinterpret_cast<uint16_t>(v43) - 1);
                    goto addr_4032a3_40;
                }
                addr_402fb0_82:
                v66 = reinterpret_cast<struct s5*>(16);
                if (reinterpret_cast<uint32_t>(esi9) & 0x80) {
                    v25 = reinterpret_cast<struct s5*>(2);
                    goto addr_402fd7_92;
                }
            }
        }
    }
    addr_402db6_31:
    return 0;
}

int32_t g40a5c0 = 0;

int32_t GetCurrentProcess = 0xd6ee;

int32_t TerminateProcess = 0xd6da;

int32_t g40a5bc = 0;

signed char g40a5b8 = 0;

int32_t* g40cf6c;

void* g40cf68;

int32_t ExitProcess = 0xd6cc;

int32_t fun_4024b0(int32_t a1, int32_t a2, int32_t a3) {
    int1_t zf4;
    int32_t esi5;
    int32_t eax6;
    int32_t ebx7;
    int1_t zf8;
    void* edi9;
    int32_t* edi10;
    int1_t cf11;
    int32_t eax12;
    int1_t cf13;
    int32_t eax14;

    zf4 = g40a5c0 == 1;
    esi5 = a1;
    if (zf4) {
        eax6 = reinterpret_cast<int32_t>(GetCurrentProcess(esi5));
        TerminateProcess(eax6, esi5);
    }
    g40a5bc = 1;
    ebx7 = a3;
    g40a5b8 = *reinterpret_cast<signed char*>(&ebx7);
    if (!a2) {
        zf8 = g40cf6c == 0;
        if (!zf8 && (edi9 = g40cf68, edi10 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi9) - 4), cf11 = reinterpret_cast<uint32_t>(edi10) < reinterpret_cast<uint32_t>(g40cf6c), !cf11)) {
            do {
                eax12 = *edi10;
                if (eax12) {
                    eax12();
                }
                --edi10;
                cf13 = reinterpret_cast<uint32_t>(edi10) < reinterpret_cast<uint32_t>(g40cf6c);
            } while (!cf13);
        }
        fun_402560(0x408014, 0x40801c);
    }
    eax14 = fun_402560(0x408020, 0x408024);
    if (!ebx7) {
        g40a5c0 = 1;
        eax14 = reinterpret_cast<int32_t>(ExitProcess(esi5));
    }
    return eax14;
}

struct s33 {
    signed char[4] pad4;
    unsigned char f4;
};

uint32_t fun_402a20(struct s5* a1, struct s5* a2) {
    void* esp3;
    uint32_t edi4;
    uint32_t eax5;
    uint32_t eax6;
    void* ebp7;
    uint32_t eax8;
    uint32_t eax9;
    void* ebx10;
    void* eax11;
    struct s33* eax12;
    struct s0* v13;
    void* eax14;

    esp3 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 - 4 - 4 - 4);
    edi4 = a2->f10;
    eax5 = a2->fc;
    if (!(*reinterpret_cast<unsigned char*>(&eax5) & 0x82) || *reinterpret_cast<unsigned char*>(&eax5) & 64) {
        a2->fc = eax5 | 32;
        return 0xffffffff;
    } else {
        if (*reinterpret_cast<unsigned char*>(&eax5) & 1) {
            a2->f4 = reinterpret_cast<struct s5*>(0);
            eax6 = a2->fc;
            if (!(*reinterpret_cast<unsigned char*>(&eax6) & 16)) {
                a2->fc = eax6 | 32;
                return 0xffffffff;
            } else {
                *reinterpret_cast<struct s0**>(&a2->f0) = a2->f8;
                a2->fc = a2->fc & 0xfffffffe;
            }
        }
        ebp7 = reinterpret_cast<void*>(0);
        eax8 = a2->fc | 2;
        a2->fc = eax8;
        a2->fc = eax8 & 0xffffffef;
        a2->f4 = reinterpret_cast<struct s5*>(0);
        if (!(a2->fc & 0x10c) && (a2 != 0x40ab98 && !reinterpret_cast<int1_t>(a2 == 0x40abb8) || (eax9 = fun_404ad0(edi4), esp3 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp3) - 4 - 4 + 4 + 4), !eax9))) {
            fun_404a80(a2);
            esp3 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp3) - 4 - 4 + 4 + 4);
        }
        if (!(a2->fc & 0x108)) {
            ebx10 = reinterpret_cast<void*>(1);
            eax11 = fun_404790(edi4, reinterpret_cast<int32_t>(esp3) + 20, 1);
            ebp7 = eax11;
        } else {
            ebx10 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s0**>(&a2->f0)) - reinterpret_cast<unsigned char>(a2->f8));
            *reinterpret_cast<struct s0**>(&a2->f0) = reinterpret_cast<struct s0*>(&a2->f8->pad4);
            a2->f4 = reinterpret_cast<struct s5*>(reinterpret_cast<int32_t>(a2->f18) - 1);
            if (reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(ebx10) < reinterpret_cast<int32_t>(0)) | reinterpret_cast<uint1_t>(ebx10 == 0)) {
                eax12 = reinterpret_cast<struct s33*>(0x40aad0);
                if (edi4 != 0xffffffff) {
                    eax12 = reinterpret_cast<struct s33*>(reinterpret_cast<int32_t>(*reinterpret_cast<void**>((reinterpret_cast<int32_t>(edi4 & 0xffffffe7) >> 3) + 0x40ce60)) + (edi4 & 31) * 8);
                }
                if (eax12->f4 & 32) {
                    fun_4049c0(edi4, 0, 2);
                }
            } else {
                v13 = a2->f8;
                eax14 = fun_404790(edi4, v13, ebx10);
                ebp7 = eax14;
            }
            *reinterpret_cast<struct s0**>(&a2->f8->f0) = *reinterpret_cast<struct s0**>(&a1);
        }
        if (ebp7 == ebx10) {
            return reinterpret_cast<uint16_t>(a1) & 0xff;
        } else {
            a2->fc = a2->fc | 32;
            return 0xffffffff;
        }
    }
}

int32_t HeapCreate = 0xd838;

int32_t HeapDestroy = 0xd82a;

int32_t fun_404430() {
    int32_t eax1;
    struct s24* eax2;
    int32_t eax3;

    eax1 = reinterpret_cast<int32_t>(HeapCreate());
    g40ce54 = eax1;
    if (eax1) {
        eax2 = fun_404fb0();
        if (eax2) {
            goto 1;
        } else {
            eax3 = g40ce54;
            HeapDestroy();
            goto eax3;
        }
    } else {
        goto 1;
    }
}

struct s0* fun_404160(struct s0* a1);

struct s34 {
    signed char f0;
    signed char f1;
};

struct s35 {
    struct s0* f0;
    signed char[7] pad8;
    struct s0* f8;
};

int32_t GetCPInfo = 0xd7d8;

int32_t g40a9d4 = 0;

int32_t fun_403f80(struct s0* a1) {
    uint32_t v2;
    uint32_t ebx3;
    struct s0* eax4;
    struct s0* ebp5;
    int1_t zf6;
    int32_t v7;
    struct s0** eax8;
    unsigned char* edi9;
    unsigned char eax10;
    int32_t ecx11;
    int32_t edi12;
    struct s34* esi13;
    uint32_t edx14;
    uint32_t ebx15;
    unsigned char cl16;
    uint32_t ebx17;
    struct s0* eax18;
    int32_t eax19;
    struct s0* ebx20;
    struct s35* ecx21;
    struct s0* ecx22;
    int32_t eax23;
    int1_t zf24;
    int32_t v25;
    int32_t v26;
    signed char* edi27;
    int32_t ecx28;
    struct s0* eax29;
    int32_t v30;
    signed char v31;
    uint32_t ecx32;
    uint32_t edx33;
    signed char v34;
    uint32_t eax35;
    signed char v36;
    signed char v37;
    uint32_t eax38;

    v2 = ebx3;
    eax4 = fun_404160(a1);
    ebp5 = eax4;
    zf6 = ebp5 == g40a9bc;
    if (zf6) {
        return 0;
    }
    if (!ebp5) {
        fun_404210();
        return 0;
    }
    v7 = 0;
    eax8 = reinterpret_cast<struct s0**>(0x40a9e0);
    do {
        if (*eax8 == ebp5) 
            break;
        eax8 = eax8 + 48;
        ++v7;
    } while (reinterpret_cast<uint32_t>(eax8) < 0x40aad0);
    goto addr_403fde_8;
    edi9 = reinterpret_cast<unsigned char*>(0x40a8b8);
    eax10 = reinterpret_cast<unsigned char>(0);
    ecx11 = 64;
    while (ecx11) {
        --ecx11;
        *edi9 = reinterpret_cast<unsigned char>(0);
        edi9 = edi9 + 4;
    }
    *edi9 = 0;
    edi12 = (v7 + v7 * 2) * 2;
    do {
        esi13 = reinterpret_cast<struct s34*>((eax10 + edi12) * 8 + 0x40a9f0);
        if (esi13->f0) {
            do {
                if (!esi13->f1) 
                    break;
                edx14 = 0;
                ebx15 = 0;
                *reinterpret_cast<signed char*>(&edx14) = esi13->f0;
                *reinterpret_cast<signed char*>(&ebx15) = esi13->f1;
                if (ebx15 >= edx14) {
                    cl16 = *reinterpret_cast<unsigned char*>(eax10 + 0x40a9d8);
                    do {
                        *reinterpret_cast<unsigned char*>(edx14 + 0x40a8b9) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(edx14 + 0x40a8b9) | cl16);
                        ++edx14;
                        ebx17 = 0;
                        *reinterpret_cast<signed char*>(&ebx17) = esi13->f1;
                    } while (ebx17 >= edx14);
                }
                ++esi13;
            } while (esi13->f0);
        }
        eax10 = reinterpret_cast<unsigned char>(eax10 + 1);
    } while (eax10 < reinterpret_cast<unsigned char>(4));
    g40a9bc = ebp5;
    eax18 = fun_4041b0(ebp5);
    g40a9c0 = eax18;
    eax19 = v7 << 4;
    ebx20 = *reinterpret_cast<struct s0**>(eax19 + eax19 * 2 + reinterpret_cast<int32_t>("!"));
    ecx21 = reinterpret_cast<struct s35*>(eax19 + eax19 * 2 + 0x40a9e4);
    ecx22 = ecx21->f8;
    g40a9c8 = ecx21->f0;
    g40a9cc = ebx20;
    g40a9d0 = ecx22;
    return 0;
    addr_403fde_8:
    eax23 = reinterpret_cast<int32_t>(GetCPInfo());
    if (eax23 != 1) {
        zf24 = g40a9d4 == 0;
        if (zf24) {
            goto v25;
        } else {
            fun_404210();
            goto v26;
        }
    }
    edi27 = reinterpret_cast<signed char*>(0x40a8b8);
    ecx28 = 64;
    while (ecx28) {
        --ecx28;
        *edi27 = reinterpret_cast<signed char>(0);
        edi27 = edi27 + 4;
    }
    *edi27 = 0;
    if (v2 > 1) 
        goto addr_40400d_34;
    eax29 = reinterpret_cast<struct s0*>(0);
    g40a9bc = reinterpret_cast<struct s0*>(0);
    addr_40411a_36:
    g40a9c0 = eax29;
    g40a9c8 = reinterpret_cast<struct s0*>(0);
    g40a9cc = reinterpret_cast<struct s0*>(0);
    g40a9d0 = reinterpret_cast<struct s0*>(0);
    goto v30;
    addr_40400d_34:
    if (*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&v7) + 2)) {
        do {
            if (!v31) 
                break;
            ecx32 = 0;
            edx33 = 0;
            *reinterpret_cast<signed char*>(&ecx32) = v34;
            *reinterpret_cast<signed char*>(&edx33) = v31;
            if (edx33 >= ecx32) {
                do {
                    *reinterpret_cast<unsigned char*>(ecx32 + 0x40a8b9) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(ecx32 + 0x40a8b9) | 4);
                    ++ecx32;
                    eax35 = 0;
                    *reinterpret_cast<signed char*>(&eax35) = v36;
                } while (eax35 >= ecx32);
            }
        } while (v37);
    }
    eax38 = 1;
    do {
        *reinterpret_cast<unsigned char*>(eax38 + 0x40a8b9) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax38 + 0x40a8b9) | 8);
        ++eax38;
    } while (eax38 < 0xff);
    g40a9bc = ebp5;
    eax29 = fun_4041b0(ebp5);
    goto addr_40411a_36;
}

struct s0** g40cf64;

struct s0** g40a5b0 = reinterpret_cast<struct s0**>(0);

struct s0* g40a598 = reinterpret_cast<struct s0*>(0);

int32_t g40a594 = 0;

void fun_403b70() {
    struct s0** esi1;
    void* esp2;
    struct s0** eax3;
    void* edi4;
    struct s0* eax5;
    void* esp6;
    int32_t esi7;

    esi1 = reinterpret_cast<struct s0**>(0x40bd38);
    GetModuleFileNameA();
    esp2 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 8 - 4 - 4 - 4 - 4 - 4 - 4 + 4);
    eax3 = g40cf64;
    g40a5b0 = reinterpret_cast<struct s0**>(0x40bd38);
    if (*eax3) {
        esi1 = g40cf64;
    }
    fun_403c10(esi1, 0, 0, reinterpret_cast<int32_t>(esp2) + 8, reinterpret_cast<int32_t>(esp2) + 12);
    eax5 = fun_404f00(0x410 + reinterpret_cast<uint32_t>(edi4));
    esp6 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp2) - 4 - 4 - 4 - 4 - 4 - 4 + 4 + 20 - 4 - 4 + 4 + 4);
    if (!eax5) {
        fun_4029f0(8);
        esp6 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp6) - 4 - 4 + 4 + 4);
    }
    fun_403c10(esi1, eax5, eax5 + 52, reinterpret_cast<int32_t>(esp6) + 8, reinterpret_cast<int32_t>(esp6) + 12);
    g40a598 = eax5;
    g40a594 = 0x103;
    goto esi7;
}

struct s0* g40a5d0 = reinterpret_cast<struct s0*>(0);

struct s0* g40a5a0 = reinterpret_cast<struct s0*>(0);

void fun_403a80() {
    struct s0* edx1;
    struct s0** esi2;
    struct s0* edi3;
    int32_t ecx4;
    int32_t eax5;
    struct s0* eax6;
    struct s0* ebx7;
    struct s0* ebp8;
    struct s0* eax9;
    struct s0* edi10;
    int32_t ecx11;
    void* ecx12;
    void* v13;
    struct s0* eax14;
    struct s0* edi15;
    int32_t ecx16;
    void* eax17;
    struct s0* ecx18;
    uint32_t ecx19;
    struct s0* edi20;
    uint32_t ecx21;
    struct s0* eax22;

    edx1 = g40a5d0;
    esi2 = reinterpret_cast<struct s0**>(0);
    if (*reinterpret_cast<struct s0**>(&edx1->f0)) {
        do {
            if (*reinterpret_cast<struct s0**>(&edx1->f0) != 61) {
                ++esi2;
            }
            edi3 = edx1;
            ecx4 = -1;
            eax5 = eax5 - eax5;
            do {
                if (!ecx4) 
                    break;
                --ecx4;
                edi3 = reinterpret_cast<struct s0*>(&edi3->pad4);
                ++esi2;
            } while (*reinterpret_cast<struct s0**>(&edi3->f0) != *reinterpret_cast<struct s0**>(&eax5));
            edx1 = reinterpret_cast<struct s0*>(reinterpret_cast<unsigned char>(edx1) + ~ecx4);
        } while (*reinterpret_cast<struct s0**>(&edx1->f0));
    }
    eax6 = fun_404f00(reinterpret_cast<uint32_t>(esi2) * 4 + 4);
    g40a5a0 = eax6;
    ebx7 = eax6;
    if (!ebx7) {
        fun_4029f0(9);
    }
    ebp8 = g40a5d0;
    eax9 = ebp8;
    if (*reinterpret_cast<struct s0**>(&ebp8->f0)) {
        do {
            edi10 = ebp8;
            ecx11 = -1;
            eax9 = reinterpret_cast<struct s0*>(reinterpret_cast<unsigned char>(eax9) - reinterpret_cast<unsigned char>(eax9));
            do {
                if (!ecx11) 
                    break;
                --ecx11;
                edi10 = reinterpret_cast<struct s0*>(&edi10->pad4);
            } while (*reinterpret_cast<struct s0**>(&edi10->f0) != eax9);
            ecx12 = reinterpret_cast<void*>(~ecx11);
            v13 = ecx12;
            if (*reinterpret_cast<struct s0**>(&ebp8->f0) != 61) {
                eax14 = fun_404f00(ecx12);
                *reinterpret_cast<struct s0**>(&ebx7->f0) = eax14;
                if (!eax14) {
                    eax14 = fun_4029f0(9);
                }
                edi15 = ebp8;
                ecx16 = -1;
                eax17 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(eax14) - reinterpret_cast<unsigned char>(eax14));
                do {
                    if (!ecx16) 
                        break;
                    --ecx16;
                    edi15 = reinterpret_cast<struct s0*>(&edi15->pad4);
                } while (*reinterpret_cast<struct s0**>(&edi15->f0) != *reinterpret_cast<struct s0**>(&eax17));
                ecx18 = reinterpret_cast<struct s0*>(~ecx16);
                eax9 = ecx18;
                ecx19 = reinterpret_cast<unsigned char>(ecx18) >> 2;
                esi2 = reinterpret_cast<struct s0**>(reinterpret_cast<unsigned char>(edi15) - reinterpret_cast<unsigned char>(ecx18));
                edi20 = *reinterpret_cast<struct s0**>(&ebx7->f0);
                ebx7 = reinterpret_cast<struct s0*>(&ebx7->f4);
                while (ecx19) {
                    --ecx19;
                    *reinterpret_cast<struct s0**>(&edi20->f0) = *esi2;
                    edi20 = reinterpret_cast<struct s0*>(&edi20->f4);
                    esi2 = esi2 + 4;
                }
                ecx21 = reinterpret_cast<unsigned char>(eax9) & 3;
                while (ecx21) {
                    --ecx21;
                    *reinterpret_cast<struct s0**>(&edi20->f0) = *esi2;
                    edi20 = reinterpret_cast<struct s0*>(&edi20->pad4);
                    ++esi2;
                }
            }
            ebp8 = reinterpret_cast<struct s0*>(reinterpret_cast<unsigned char>(ebp8) + reinterpret_cast<uint32_t>(v13));
        } while (*reinterpret_cast<struct s0**>(&ebp8->f0));
    }
    eax22 = g40a5d0;
    fun_404eb0(eax22);
    g40a5d0 = reinterpret_cast<struct s0*>(0);
    *reinterpret_cast<struct s0**>(&ebx7->f0) = reinterpret_cast<struct s0*>(0);
    return;
}

int32_t g40cf70;

int32_t fun_402440() {
    int32_t eax1;
    int32_t eax2;

    eax1 = g40cf70;
    if (eax1) {
        eax1();
    }
    fun_402560(0x408008, 0x408010);
    eax2 = fun_402560(0x408000, 0x408004);
    return eax2;
}

int32_t fun_403a40(signed char a1, uint32_t a2, uint32_t a3) {
    int32_t edx4;
    uint32_t ecx5;
    uint32_t ecx6;
    uint32_t ecx7;

    edx4 = 0;
    ecx5 = 0;
    *reinterpret_cast<signed char*>(&edx4) = a1;
    *reinterpret_cast<signed char*>(&ecx5) = *reinterpret_cast<signed char*>(edx4 + 0x40a8b9);
    if (!(a3 & ecx5)) {
        ecx6 = 0;
        if (a2) {
            ecx7 = 0;
            *reinterpret_cast<int16_t*>(&ecx7) = *reinterpret_cast<int16_t*>(" " + edx4 * 2);
            ecx6 = ecx7 & a2;
        }
        if (!ecx6) {
            return 0;
        }
    }
    return 1;
}

signed char g40ba18;

int32_t CharNextA = 0xd564;

int32_t CoInitialize = 0xd682;

int32_t DialogBoxParamA = 0xd552;

int32_t CoUninitialize = 0xd670;

void fun_401130(int32_t ecx, void* a2, void* a3, void* a4, void* a5, void* a6, int32_t a7, int32_t a8, int32_t a9, void* a10, int32_t a11, int32_t a12, void* a13, int32_t a14, int32_t a15, void* a16, int32_t a17, void* a18, int32_t a19, int32_t a20, int32_t a21, void* a22, int32_t a23, int32_t a24, void* a25, int32_t a26, int32_t a27, int32_t a28, void* a29, int32_t a30, int32_t a31, int32_t a32, void* a33, int32_t a34, int32_t a35, int32_t a36, void* a37, int32_t a38, int32_t a39, int32_t a40, void* a41, int32_t a42, int32_t a43, int32_t a44, void* a45, int32_t a46, int32_t a47, void* a48, void* a49, void* a50, void* a51, void* a52, void* a53, void* a54, void* a55, void* a56, void* a57, void* a58, void* a59, void* a60, void* a61, void* a62, void* a63, void* a64, void* a65, void* a66, void* a67, void* a68, void* a69, void* a70, void* a71, void* a72, void* a73, void* a74, void* a75, void* a76, void* a77, void* a78, void* a79, void* a80, void* a81, void* a82, void* a83, void* a84, void* a85, void* a86, void* a87, void* a88, void* a89, void* a90, void* a91, void* a92, void* a93, void* a94, void* a95);

int32_t fun_4022e0(int32_t a1, int32_t a2, int32_t a3, struct s0** a4) {
    int32_t v5;
    int32_t v6;
    int32_t ebx7;
    int32_t v8;
    int32_t esi9;
    int32_t v10;
    int32_t edi11;
    signed char* esi12;
    int32_t v13;
    int1_t zf14;
    signed char* eax15;
    int32_t ebx16;

    v5 = reinterpret_cast<int32_t>(__return_address());
    v6 = ebx7;
    v8 = esi9;
    v10 = edi11;
    esi12 = reinterpret_cast<signed char*>(0x40ba18);
    g40ba10 = a1;
    v13 = a1;
    GetModuleFileNameA(v13, 0x40ba18, 0x104, v10, v8, v6, v5);
    zf14 = g40ba18 == 0;
    eax15 = reinterpret_cast<signed char*>(0x40ba18);
    if (!zf14) {
        ebx16 = CharNextA;
        do {
            if (*eax15 == 92 || *eax15 == 47) {
                esi12 = eax15;
            }
            eax15 = reinterpret_cast<signed char*>(ebx16(eax15, v13, 0x40ba18, 0x104, v10, v8, v6, v5));
        } while (*eax15);
    }
    *esi12 = 0;
    CoInitialize(0, v13, 0x40ba18, 0x104, v10, v8, v6, v5);
    DialogBoxParamA();
    CoUninitialize();
    goto fun_401130;
}

int32_t g40ae24 = 0;

int32_t GetStringTypeA = 0xd882;

int32_t GetStringTypeW = 0xd894;

int32_t fun_404d80(int32_t a1, void* a2, int32_t a3, void* a4, struct s0* a5, int32_t a6) {
    int32_t eax7;
    int32_t esi8;
    int32_t eax9;
    int32_t eax10;
    int32_t edi11;
    struct s0* esi12;
    struct s0* ebx13;
    int32_t eax14;
    struct s0* eax15;
    int32_t edi16;
    int32_t esi17;
    int32_t eax18;
    int32_t eax19;
    int32_t ebp20;

    eax7 = g40ae24;
    if (eax7) {
        esi8 = GetStringTypeA;
    } else {
        esi8 = GetStringTypeA;
        eax9 = reinterpret_cast<int32_t>(esi8(0));
        if (!eax9) {
            eax10 = reinterpret_cast<int32_t>(GetStringTypeW());
            if (!eax10) {
                goto 1;
            } else {
                eax7 = 1;
            }
        } else {
            eax7 = 2;
        }
    }
    g40ae24 = eax7;
    if (eax7 != 2) {
        g40ae24 = eax7;
        if (eax7 == 1) {
            edi11 = 0;
            esi12 = reinterpret_cast<struct s0*>(0);
            ebx13 = a5;
            if (!ebx13) {
                ebx13 = g40ae40;
            }
            eax14 = reinterpret_cast<int32_t>(MultiByteToWideChar());
            if (eax14 && ((eax15 = fun_405a00(2, eax14), esi12 = eax15, !!esi12) && (eax18 = reinterpret_cast<int32_t>(MultiByteToWideChar(ebx13, 1, edi16, esi17, esi12, eax14)), !!eax18))) {
                eax19 = reinterpret_cast<int32_t>(GetStringTypeW(ebx13, esi12, eax18, a3, ebx13, 1, edi16, esi17, esi12, eax14));
                edi11 = eax19;
            }
            fun_404eb0(esi12);
            eax7 = edi11;
        }
        return eax7;
    } else {
        if (!a6) {
        }
        esi8();
        goto ebp20;
    }
}

struct s0* fun_404f20(void* a1, int32_t a2) {
    void* esi3;
    int32_t edi4;
    struct s0* eax5;
    int32_t eax6;

    esi3 = a1;
    if (reinterpret_cast<uint32_t>(esi3) > 0xffffffe0) {
        return 0;
    }
    if (!esi3) {
        esi3 = reinterpret_cast<void*>(1);
    }
    edi4 = a2;
    do {
        eax5 = reinterpret_cast<struct s0*>(0);
        if (reinterpret_cast<uint32_t>(esi3) <= 0xffffffe0) {
            eax5 = fun_404f70(esi3);
        }
        if (eax5) 
            break;
        if (!edi4) 
            break;
        eax6 = fun_405de0(esi3);
    } while (eax6);
    goto addr_404f65_11;
    addr_404f67_12:
    return eax5;
    addr_404f65_11:
    eax5 = reinterpret_cast<struct s0*>(0);
    goto addr_404f67_12;
}

int32_t RtlUnwind = 0xd734;

void fun_406000() {
    goto RtlUnwind;
}

uint32_t fun_405be0() {
    uint32_t eax1;

    eax1 = fun_405bf0(1);
    return eax1;
}

struct s36 {
    int32_t f0;
    unsigned char f4;
};

int32_t SetStdHandle = 0xd8f0;

int32_t fun_405920(uint32_t a1) {
    int1_t cf2;
    int32_t** edi3;
    uint32_t esi4;
    struct s36* eax5;
    int1_t zf6;
    int32_t v7;

    cf2 = a1 < g40cf60;
    if (!cf2 || ((edi3 = reinterpret_cast<int32_t**>((reinterpret_cast<int32_t>(a1 & 0xffffffe7) >> 3) + 0x40ce60), esi4 = (a1 & 31) * 8, eax5 = reinterpret_cast<struct s36*>(reinterpret_cast<int32_t>(*edi3) + esi4), (eax5->f4 & 1) == 0) || eax5->f0 == -1)) {
        g40a578 = 9;
        g40a57c = 0;
        return -1;
    }
    zf6 = g40a5e0 == 1;
    if (!zf6) {
        addr_405986_4:
        *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(*edi3) + esi4) = -1;
        return 0;
    } else {
        if (!a1) {
            v7 = 0xf6;
        } else {
            if (a1 == 1) {
                v7 = 0xf5;
            } else {
                if (a1 == 2) {
                    v7 = 0xf4;
                } else {
                    goto addr_405986_4;
                }
            }
        }
    }
    SetStdHandle(v7, 0);
    goto addr_405986_4;
}

int32_t GetACP = 0xd7e4;

int32_t GetOEMCP = 0xd7ee;

struct s0* fun_404160(struct s0* a1) {
    struct s0* eax2;

    g40a9d4 = 0;
    eax2 = a1;
    if (!reinterpret_cast<int1_t>(eax2 == 0xfffffffe)) {
        if (!reinterpret_cast<int1_t>(eax2 == 0xfffffffd)) {
            if (reinterpret_cast<int1_t>(eax2 == 0xfffffffc)) {
                g40a9d4 = 1;
                eax2 = g40ae40;
            }
            return eax2;
        } else {
            g40a9d4 = 1;
            goto GetACP;
        }
    } else {
        g40a9d4 = 1;
        goto GetOEMCP;
    }
}

struct s37 {
    int32_t f0;
    int32_t f4;
    int32_t f8;
};

struct s37* fun_4039f0(int32_t a1);

int32_t UnhandledExceptionFilter = 0xd740;

int32_t g40a8a8 = 0;

int32_t g40a89c = 7;

int32_t g40a898 = 3;

int32_t g40a8a4 = 0x8c;

int32_t fun_403890(int32_t a1, int32_t a2) {
    struct s37* eax3;
    int32_t edx4;
    int32_t esi5;
    int32_t esi6;
    int32_t v7;
    int32_t ecx8;
    int32_t tmp32_9;
    int1_t less_or_equal10;
    int32_t ecx11;
    int32_t* edi12;
    int32_t ecx13;
    int32_t edi14;
    int32_t eax15;

    eax3 = fun_4039f0(a1);
    if (!eax3 || (edx4 = eax3->f8, edx4 == 0)) {
        UnhandledExceptionFilter();
        goto esi5;
    } else {
        if (edx4 != 5) {
            if (edx4 != 1) {
                esi6 = g40a8a8;
                g40a8a8 = a2;
                if (eax3->f4 != 8) {
                    eax3->f8 = 0;
                    v7 = eax3->f4;
                    edx4(v7);
                } else {
                    ecx8 = g40a89c;
                    tmp32_9 = ecx8 + g40a898;
                    less_or_equal10 = tmp32_9 <= g40a898;
                    if (!less_or_equal10) {
                        ecx11 = g40a898;
                        edi12 = reinterpret_cast<int32_t*>((ecx11 + ecx11 * 2) * 4 + 0x40a828);
                        ecx13 = g40a89c;
                        do {
                            *edi12 = 0;
                            edi12 = edi12 + 3;
                            --ecx13;
                        } while (ecx13);
                    }
                    edi14 = g40a8a4;
                    if (eax3->f0 != 0xc000008e) {
                        if (eax3->f0 != 0xc0000090) {
                            if (eax3->f0 != 0xc0000091) {
                                if (eax3->f0 != 0xc0000093) {
                                    if (eax3->f0 != 0xc000008d) {
                                        if (eax3->f0 != 0xc000008f) {
                                            if (eax3->f0 == 0xc0000092) {
                                                g40a8a4 = 0x8a;
                                            }
                                        } else {
                                            g40a8a4 = 0x86;
                                        }
                                    } else {
                                        g40a8a4 = 0x82;
                                    }
                                } else {
                                    g40a8a4 = 0x85;
                                }
                            } else {
                                g40a8a4 = 0x84;
                            }
                        } else {
                            g40a8a4 = 0x81;
                        }
                    } else {
                        g40a8a4 = 0x83;
                    }
                    eax15 = g40a8a4;
                    edx4(8, eax15);
                    g40a8a4 = edi14;
                }
                g40a8a8 = esi6;
                return -1;
            } else {
                return -1;
            }
        } else {
            eax3->f8 = 0;
            return 1;
        }
    }
}

int32_t g40a8a0 = 10;

struct s37* fun_4039f0(int32_t a1) {
    int32_t* edx2;
    int32_t ecx3;
    int32_t eax4;
    uint32_t eax5;

    edx2 = reinterpret_cast<int32_t*>(0x40a820);
    ecx3 = a1;
    do {
        if (*edx2 == ecx3) 
            break;
        edx2 = edx2 + 3;
        eax4 = g40a8a0;
    } while (reinterpret_cast<uint32_t>((eax4 + eax4 * 2) * 4 + 0x40a820) > reinterpret_cast<uint32_t>(edx2));
    eax5 = reinterpret_cast<uint32_t>(*edx2 - ecx3);
    return eax5 - (eax5 + reinterpret_cast<uint1_t>(eax5 < eax5 + reinterpret_cast<uint1_t>(eax5 < 1))) & reinterpret_cast<uint32_t>(edx2);
}

int32_t fun_402490(int32_t a1) {
    int32_t eax2;

    eax2 = fun_4024b0(a1, 1, 0);
    return eax2;
}

void fun_4021c0() {
    int32_t eax1;
    int32_t eax2;
    signed char al3;

    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax2) + al3);
}

signed char g5050105;

void fun_403440(int32_t ecx) {
    signed char tmp8_2;
    signed char al3;

    tmp8_2 = reinterpret_cast<signed char>(g5050105 + al3);
    g5050105 = tmp8_2;
}

void fun_403468() {
    int32_t eax1;
    int32_t eax2;
    signed char al3;
    int32_t eax4;
    signed char al5;

    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax2) + al3);
    *reinterpret_cast<signed char*>(&eax4) = reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(reinterpret_cast<signed char>(al5 + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4) + 4);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax4) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax4) + eax4;
}

struct s38 {
    signed char[269488144] pad269488144;
    unsigned char f10101010;
};

unsigned char g10100e10;

void fun_4034dc(uint32_t ecx) {
    signed char* eax2;
    signed char* eax3;
    signed char dl4;
    int32_t* eax5;
    int32_t* eax6;
    int32_t edx7;
    unsigned char tmp8_8;
    signed char dl9;
    signed char* eax10;
    unsigned char* edx11;
    unsigned char tmp8_12;
    signed char* eax13;
    unsigned char dl14;
    uint1_t cf15;
    unsigned char* eax16;
    unsigned char* eax17;
    unsigned char tmp8_18;
    signed char* eax19;
    uint1_t cf20;
    unsigned char* eax21;
    unsigned char* eax22;
    unsigned char tmp8_23;
    signed char* eax24;
    uint1_t cf25;
    unsigned char* eax26;
    unsigned char* eax27;
    unsigned char tmp8_28;
    signed char* eax29;
    uint1_t cf30;
    unsigned char* eax31;
    unsigned char* eax32;
    unsigned char* eax33;
    signed char* eax34;
    unsigned char* tmp32_35;
    void** eax36;
    unsigned char* edx37;
    unsigned char tmp8_38;
    signed char* eax39;
    uint1_t cf40;
    unsigned char* eax41;
    unsigned char* eax42;
    void* eax43;
    void* eax44;
    signed char al45;
    unsigned char* tmp32_46;
    struct s38* eax47;
    unsigned char tmp8_48;
    unsigned char* eax49;
    uint1_t cf50;
    unsigned char tmp8_51;
    uint1_t cf52;
    uint32_t ecx53;
    unsigned char tmp8_54;

    *eax2 = reinterpret_cast<signed char>(*eax3 + dl4);
    *eax5 = *eax6 + edx7;
    tmp8_8 = reinterpret_cast<unsigned char>(dl9 + *eax10);
    *reinterpret_cast<unsigned char*>(&edx11) = tmp8_8;
    tmp8_12 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*eax13 + *reinterpret_cast<unsigned char*>(&edx11)) + reinterpret_cast<uint1_t>(tmp8_8 < dl14));
    cf15 = reinterpret_cast<uint1_t>(tmp8_12 < *eax16);
    *eax17 = tmp8_12;
    tmp8_18 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*eax19 + *reinterpret_cast<unsigned char*>(&edx11)) + cf15);
    cf20 = reinterpret_cast<uint1_t>(tmp8_18 < *eax21);
    *eax22 = tmp8_18;
    tmp8_23 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*eax24 + *reinterpret_cast<unsigned char*>(&edx11)) + cf20);
    cf25 = reinterpret_cast<uint1_t>(tmp8_23 < *eax26);
    *eax27 = tmp8_23;
    tmp8_28 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*eax29 + *reinterpret_cast<unsigned char*>(&edx11)) + cf25);
    cf30 = reinterpret_cast<uint1_t>(tmp8_28 < *eax31);
    *eax32 = tmp8_28;
    *eax33 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*eax34 + *reinterpret_cast<unsigned char*>(&edx11)) + cf30);
    tmp32_35 = reinterpret_cast<unsigned char*>(reinterpret_cast<uint32_t>(edx11) + reinterpret_cast<int32_t>(*eax36));
    edx37 = tmp32_35;
    tmp8_38 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*eax39 + *reinterpret_cast<unsigned char*>(&edx37)) + reinterpret_cast<uint1_t>(reinterpret_cast<uint32_t>(tmp32_35) < reinterpret_cast<uint32_t>(edx11)));
    cf40 = reinterpret_cast<uint1_t>(tmp8_38 < *eax41);
    *eax42 = tmp8_38;
    *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(eax43) + reinterpret_cast<uint32_t>(edx37)) = reinterpret_cast<unsigned char>(reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(eax44) + reinterpret_cast<uint32_t>(edx37)) + al45) + cf40);
    tmp32_46 = reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(eax47) + 0x10101010);
    tmp8_48 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*tmp32_46 + *reinterpret_cast<unsigned char*>(&edx37)) + reinterpret_cast<uint1_t>(reinterpret_cast<uint32_t>(tmp32_46) < reinterpret_cast<uint32_t>(eax49)));
    cf50 = reinterpret_cast<uint1_t>(tmp8_48 < *tmp32_46);
    *tmp32_46 = tmp8_48;
    *tmp32_46 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*tmp32_46 + *reinterpret_cast<unsigned char*>(&edx37)) + cf50);
    *tmp32_46 = reinterpret_cast<unsigned char>(*tmp32_46 | *reinterpret_cast<unsigned char*>(&ecx));
    *tmp32_46 = reinterpret_cast<unsigned char>(*tmp32_46 | *reinterpret_cast<unsigned char*>(&edx37));
    *tmp32_46 = reinterpret_cast<unsigned char>(*tmp32_46 | reinterpret_cast<uint32_t>(edx37));
    tmp8_51 = reinterpret_cast<unsigned char>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(*tmp32_46 + *reinterpret_cast<unsigned char*>(&edx37))));
    cf52 = reinterpret_cast<uint1_t>(tmp8_51 < *tmp32_46);
    *tmp32_46 = tmp8_51;
    *edx37 = reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(*edx37 + *reinterpret_cast<unsigned char*>(&ecx)) + cf52);
    ecx53 = ecx | *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(tmp32_46) + reinterpret_cast<uint32_t>(edx37));
    tmp8_54 = reinterpret_cast<unsigned char>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(g10100e10 + *reinterpret_cast<signed char*>(&ecx53))));
    g10100e10 = tmp8_54;
}

void fun_4041fc() {
    int32_t eax1;
    int32_t eax2;
    signed char al3;

    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(__zero_stack_offset()) + eax2) + al3);
}

int32_t g40b808;

int32_t LoadCursorA = 0xd510;

int32_t SetCursor = 0xd504;

int32_t g40b804;

int32_t GetModuleHandleA = 0xd394;

int32_t LoadStringA = 0xd4aa;

int32_t SetWindowTextA = 0xd4d8;

int32_t SendDlgItemMessageA = 0xd53c;

int32_t SetFocus = 0xd530;

int32_t SHBrowseForFolderA = 0xd60a;

int32_t SHGetPathFromIDListA = 0xd5f2;

int32_t g40bc28;

int32_t ExitWindowsEx = 0xd4b8;

int32_t GetWindowTextA = 0xd51e;

int32_t GetWindowsDirectoryA = 0xd3f2;

int32_t lstrcpyA = 0xd3d2;

int32_t SHFileOperationA = 0xd650;

int32_t g40a048 = 0;

int32_t MessageBoxA = 0xd49c;

int32_t GetDiskFreeSpaceA = 0xd3de;

signed char g40bb21;

signed char g40bb22;

int32_t wsprintfA = 0xd4f8;

int32_t SetFileAttributesA = 0xd3bc;

int32_t RegCreateKeyExA = 0xd5c4;

void* g40a4e8 = reinterpret_cast<void*>(0);

int32_t RegSetValueExA = 0xd5b2;

int32_t RegCloseKey = 0xd5a4;

int32_t SHGetSpecialFolderLocation = 0xd632;

int32_t CreateDirectoryA = 0xd3a8;

int32_t SHChangeNotify = 0xd620;

/* (image base) */
int32_t image_base_ = 0x40a038;

/* (image base) */
int32_t image_base_ = 0x40a02c;

/* (image base) */
int32_t image_base_ = 0x40a020;

int32_t ShowWindow = 0xd4ea;

int32_t EndDialog = 0xd490;

void fun_401130(int32_t ecx, void* a2, void* a3, void* a4, void* a5, void* a6, int32_t a7, int32_t a8, int32_t a9, void* a10, int32_t a11, int32_t a12, void* a13, int32_t a14, int32_t a15, void* a16, int32_t a17, void* a18, int32_t a19, int32_t a20, int32_t a21, void* a22, int32_t a23, int32_t a24, void* a25, int32_t a26, int32_t a27, int32_t a28, void* a29, int32_t a30, int32_t a31, int32_t a32, void* a33, int32_t a34, int32_t a35, int32_t a36, void* a37, int32_t a38, int32_t a39, int32_t a40, void* a41, int32_t a42, int32_t a43, int32_t a44, void* a45, int32_t a46, int32_t a47, void* a48, void* a49, void* a50, void* a51, void* a52, void* a53, void* a54, void* a55, void* a56, void* a57, void* a58, void* a59, void* a60, void* a61, void* a62, void* a63, void* a64, void* a65, void* a66, void* a67, void* a68, void* a69, void* a70, void* a71, void* a72, void* a73, void* a74, void* a75, void* a76, void* a77, void* a78, void* a79, void* a80, void* a81, void* a82, void* a83, void* a84, void* a85, void* a86, void* a87, void* a88, void* a89, void* a90, void* a91, void* a92, void* a93, void* a94, void* a95) {
    void* v96;
    int32_t v97;
    int32_t ebx98;
    void* v99;
    void* esi100;
    uint32_t eax101;
    int32_t v102;
    void* v103;
    void* edi104;
    int32_t* esp105;
    int32_t v106;
    int32_t ebp107;
    int32_t ecx108;
    int1_t zf109;
    int32_t eax110;
    int32_t v111;
    void* eax112;
    int32_t ebx113;
    void* v114;
    int32_t eax115;
    int32_t eax116;
    void* esp117;
    void* v118;
    int32_t eax119;
    void* esp120;
    int32_t esi121;
    void* esp122;
    void* v123;
    int32_t eax124;
    void* v125;
    int32_t eax126;
    int32_t v127;
    int32_t ecx128;
    int32_t eax129;
    int1_t zf130;
    void* ebp131;
    void* v132;
    void* v133;
    int1_t zf134;
    void* v135;
    int32_t eax136;
    void* v137;
    void* v138;
    int32_t v139;
    int1_t zf140;
    void* v141;
    void* v142;
    void* v143;
    void* eax144;
    void* v145;
    void* v146;
    void* eax147;
    void* v148;
    void* eax149;
    void* v150;
    void* v151;
    void* eax152;
    void* v153;
    struct s1* eax154;
    void* esp155;
    signed char* edi156;
    int32_t ecx157;
    void* eax158;
    uint32_t ecx159;
    uint32_t edx160;
    signed char* esi161;
    int32_t ecx162;
    signed char* edi163;
    void* eax164;
    signed char* edi165;
    uint32_t ecx166;
    uint32_t ecx167;
    signed char* edi168;
    int32_t ecx169;
    void* eax170;
    uint32_t ecx171;
    uint32_t edx172;
    signed char* esi173;
    int32_t ecx174;
    signed char* edi175;
    void* eax176;
    signed char* edi177;
    uint32_t ecx178;
    uint32_t ecx179;
    void* esp180;
    void* v181;
    void* eax182;
    int32_t ebx183;
    void* esp184;
    void* esp185;
    void* v186;
    void* esp187;
    void* v188;
    void* esp189;
    void* v190;
    void* v191;
    void* esp192;
    void* v193;
    void* esp194;
    void* v195;
    void* edx196;
    void* v197;
    void* v198;
    int32_t eax199;
    void* esp200;
    void* v201;
    void* v202;
    void* esp203;
    int1_t zf204;
    uint32_t esi205;
    uint32_t v206;
    int32_t edi207;
    void* v208;
    void* eax209;
    void* esp210;
    void* v211;
    void* eax212;
    void* esp213;
    void* esp214;
    void* v215;
    void* v216;
    uint32_t eax217;
    void* v218;
    void* eax219;
    void* esp220;
    void* v221;
    void* eax222;
    void* esp223;
    void* esp224;
    void* esp225;
    void** esp226;
    void* v227;
    void* v228;
    void* v229;
    void* v230;
    void* eax231;
    void* eax232;
    void* esp233;
    void* v234;
    void* v235;
    void* v236;
    void* eax237;
    void* esp238;
    void* v239;
    uint32_t eax240;
    void* v241;
    void* v242;
    void* v243;
    void* eax244;
    void* v245;
    void* esp246;
    uint32_t eax247;
    void* esp248;
    int1_t zf249;
    int1_t zf250;
    void** esp251;
    void* v252;
    void** esp253;
    void* v254;
    void* v255;
    void* v256;
    void* v257;
    void* eax258;
    void* eax259;
    void** esp260;
    void* v261;
    int32_t* esp262;
    int32_t v263;
    int32_t eax264;
    void* v265;
    void* eax266;
    void* eax267;
    void* v268;
    void* v269;
    void* eax270;
    void* esp271;
    void** esp272;
    void* esp273;
    void* v274;
    void* eax275;
    void* esp276;
    void* v277;
    void* v278;
    void* eax279;
    void* eax280;
    void* eax281;
    void* v282;
    void* v283;
    void* v284;
    void* eax285;
    void* v286;
    void* esp287;
    void* v288;
    uint32_t eax289;
    void* esp290;
    void* v291;
    void* eax292;
    void* v293;
    void* esp294;
    void** esi295;
    void* v296;
    void* esp297;
    void* v298;
    void* esp299;
    void* v300;
    void* v301;
    void* esp302;
    void* v303;
    void* esp304;
    void* v305;
    void* v306;
    void* esp307;
    void* v308;
    void* eax309;
    void* v310;
    void* esp311;
    void* v312;
    void* v313;
    void* esp314;
    void* v315;
    int32_t eax316;
    void* v317;
    void* v318;
    int32_t eax319;
    void* esp320;
    void* v321;
    void* esp322;
    int32_t eax323;
    void* esp324;
    void* v325;
    void* eax326;
    void* v327;
    void* esp328;
    void* v329;
    int32_t eax330;
    void* esp331;
    signed char* edi332;
    int32_t ecx333;
    int32_t eax334;
    uint32_t ecx335;
    uint32_t eax336;
    uint32_t ecx337;
    uint32_t ecx338;
    int32_t ecx339;
    uint32_t eax340;
    signed char v341;
    void* v342;
    void* edx343;
    void* v344;
    void* v345;
    void* ecx346;
    void* v347;
    void* esp348;
    void* v349;
    void* eax350;
    void* v351;
    void* esp352;
    void* v353;
    void* esp354;
    void* v355;
    void* esp356;
    void* esp357;
    signed char* edi358;
    int32_t eax359;
    void* esp360;
    int32_t ecx361;
    int32_t eax362;
    uint32_t edx363;
    int32_t ecx364;
    int32_t eax365;
    signed char v366;
    uint32_t ecx367;
    void* esp368;
    uint32_t ecx369;
    void* v370;
    void* esp371;
    void* v372;
    void* esp373;
    int32_t ecx374;
    void* esp375;
    void* v376;
    void* eax377;
    void* esp378;
    void* esp379;
    void* esp380;
    void* v381;
    void* v382;
    void* esp383;
    int32_t ecx384;
    void* esp385;
    void* v386;
    void* eax387;
    void* esp388;
    void* esp389;
    void* v390;
    void* v391;
    void* esp392;
    int32_t ecx393;
    void* esp394;
    void* v395;
    void* eax396;
    void* esp397;
    void** esp398;
    void* esp399;
    void* v400;
    void* v401;
    int32_t v402;
    int32_t eax403;
    int32_t esi404;
    int32_t eax405;
    int32_t eax406;
    int32_t eax407;
    int32_t eax408;
    int32_t eax409;
    void* esp410;
    int1_t zf411;
    void** esp412;
    void* v413;
    int32_t eax414;
    void* esp415;
    void* v416;
    int32_t eax417;
    void* esp418;
    void* v419;
    int32_t eax420;
    void* v421;
    int32_t eax422;
    int32_t eax423;
    int32_t eax424;
    void* esp425;
    int32_t edi426;
    void* v427;
    int32_t eax428;
    void* esp429;
    void* v430;
    int32_t eax431;
    void* esp432;
    void* v433;
    int32_t eax434;
    void* esp435;
    void* v436;
    int32_t eax437;
    void* esp438;
    void* v439;
    int32_t eax440;
    int32_t esi441;
    void* esp442;
    void* v443;
    int32_t eax444;
    void* esp445;
    void* v446;
    int32_t eax447;
    void* esp448;
    void* v449;
    int32_t eax450;
    int32_t eax451;
    int32_t eax452;
    int32_t v453;
    void* v454;
    void* eax455;
    void* eax456;
    void* v457;
    void* v458;
    void* eax459;
    void* esp460;
    void** esp461;
    void* esp462;
    void* v463;
    int32_t eax464;
    void* esp465;
    int32_t eax466;
    int32_t eax467;
    int32_t v468;
    void* v469;
    int32_t eax470;
    int32_t ebx471;
    void* esp472;
    void* v473;
    int32_t eax474;
    void* esp475;
    void* v476;
    int32_t eax477;

    v96 = __return_address();
    fun_402820(ecx, v96);
    v97 = ebx98;
    v99 = esi100;
    eax101 = reinterpret_cast<uint32_t>(v102 - 32);
    v103 = edi104;
    esp105 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 + 4 - 4 - 4 - 4 - 4);
    v106 = ebp107;
    if (eax101 > 0xf2) 
        goto addr_40219c_2;
    ecx108 = 0;
    *reinterpret_cast<signed char*>(&ecx108) = *reinterpret_cast<signed char*>(eax101 + reinterpret_cast<int32_t>(fun_4021c0));
    switch (ecx108) {
    case 0:
        zf109 = g40b808 == 0;
        if (zf109) {
            addr_40219c_2:
        case 4:
        } else {
            eax110 = reinterpret_cast<int32_t>(LoadCursorA(0, 0x7f02));
            SetCursor(eax110, 0, 0x7f02);
        }
        goto v111;
    case 1:
        g40b808 = 0;
        g40b804 = 0;
        eax112 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x65, 0x40b810, 0x200));
        ebx113 = LoadStringA;
        ebx113(eax112, 0, 0x65, 0x40b810, 0x200);
        SetWindowTextA(v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        eax115 = reinterpret_cast<int32_t>(GetDlgItem(v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200));
        EnableWindow(eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        eax116 = reinterpret_cast<int32_t>(GetDlgItem(v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200));
        EnableWindow(eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        esp117 = reinterpret_cast<void*>(esp105 - 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1 - 1 - 1 - 1 + 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1);
        v118 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp117) + 0x68);
        eax119 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200));
        ebx113(eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        esp120 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp117) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        fun_401050(v114, 0x3ea, reinterpret_cast<int32_t>(esp120) + 0x68, 0xaf, 0xc3, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200, v106, v103, v99, v97, v96, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24, a25, a26, a27, a28, a29, a30, a31, a32, a33, a34, a35, a36, a37, a38, a39, a40, a41, a42, a43, a44, a45, a46, a47, a48, a49, a50, a51, a52, a53, a54, a55, a56, a57, a58, a59, a60, a61, a62, a63, a64, a65, a66, a67, a68, a69, a70, a71, a72, a73, a74, a75, a76, a77, a78, a79, a80, a81, a82, a83, a84, a85, a86, a87, a88, a89, a90, a91, a92, a93, a94, a95);
        esi121 = SendDlgItemMessageA;
        esi121(v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        esp122 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp120) - 4 - 4 - 4 - 4 - 4 - 4 + 4 + 20 - 4 - 4 - 4 - 4 - 4 - 4 + 4);
        v123 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp122) + 0x68);
        eax124 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 100, v123, 0x200, v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200));
        ebx113(eax124, 0, 100, v123, 0x200, v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        v125 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp122) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 + 0x68);
        SetDlgItemTextA(v114, 0x3e8, v125, eax124, 0, 100, v123, 0x200, v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        esi121(v114, 0x3e8, 0xb1, 0, 0x1000100, v114, 0x3e8, v125, eax124, 0, 100, v123, 0x200, v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        eax126 = reinterpret_cast<int32_t>(GetDlgItem(v114, 0x3e8, v114, 0x3e8, 0xb1, 0, 0x1000100, v114, 0x3e8, v125, eax124, 0, 100, v123, 0x200, v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200));
        SetFocus(eax126, v114, 0x3e8, v114, 0x3e8, 0xb1, 0, 0x1000100, v114, 0x3e8, v125, eax124, 0, 100, v123, 0x200, v114, 0x3e8, 0xc5, 0x104, 0, eax119, 0, 0x76, v118, 0x200, eax116, v114, 0x3ef, 0, eax115, v114, 0x3f0, 0, v114, 0x40b810, eax112, 0, 0x65, 0x40b810, 0x200);
        goto addr_40219c_2;
    case 2:
        if (v127 == 1) {
            ecx128 = g40b808;
            if (!reinterpret_cast<uint1_t>(reinterpret_cast<uint1_t>(ecx128 < 0) | reinterpret_cast<uint1_t>(ecx128 == 0))) 
                goto addr_40219c_2;
            eax129 = g40b804;
            if (!eax129) 
                goto addr_4012dd_11;
        } else {
            if (v127 == 2) {
                zf130 = g40b808 == 0;
                if (!zf130) 
                    goto addr_40219c_2;
                ebp131 = v132;
                v133 = reinterpret_cast<void*>(0xff);
                break;
            } else {
                if (v127 == 0x3f3) {
                    zf134 = g40b804 == 0;
                    if (zf134 && (v135 = reinterpret_cast<void*>(esp105 + 8), eax136 = reinterpret_cast<int32_t>(SHBrowseForFolderA(v135)), esp105 = esp105 - 1 - 1 + 1, !!eax136)) {
                        v137 = reinterpret_cast<void*>(esp105 + 0xdb);
                        SHGetPathFromIDListA(eax136, v137, v135);
                        v138 = reinterpret_cast<void*>(esp105 - 1 - 1 - 1 + 1 + 0xdb);
                        SetDlgItemTextA(v139, 0x3e8, v138, eax136, v137, v135);
                        fun_4010f0(eax136, v139, 0x3e8, v138, eax136, v137, v135);
                        goto addr_40219c_2;
                    }
                } else {
                    goto addr_40219c_2;
                }
            }
        }
        if (eax129 != 1) 
            goto addr_40219c_2;
        zf140 = g40bc28 == 0;
        v133 = reinterpret_cast<void*>(0);
        if (!zf140) 
            goto addr_402070_21;
        ebp131 = v141;
        break;
        addr_402070_21:
        ExitWindowsEx(2, 0);
        goto addr_40219c_2;
        addr_4012dd_11:
        v133 = reinterpret_cast<void*>(0);
        ebp131 = v142;
        v143 = ebp131;
        g40b808 = ecx128 + 1;
        eax144 = reinterpret_cast<void*>(GetDlgItem(v143, 1, 0));
        v145 = eax144;
        EnableWindow(v145, v143, 1, 0);
        v146 = ebp131;
        eax147 = reinterpret_cast<void*>(GetDlgItem(v146, 2, 0, v145, v143, 1, 0));
        v148 = eax147;
        EnableWindow(v148, v146, 2, 0, v145, v143, 1, 0);
        eax149 = reinterpret_cast<void*>(LoadCursorA(0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0));
        v150 = eax149;
        SetCursor(v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0);
        v151 = ebp131;
        eax152 = reinterpret_cast<void*>(GetDlgItem(v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0));
        v153 = eax152;
        GetWindowTextA(v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0);
        fun_406010(0x40bb20, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0);
        eax154 = fun_4027a0(0x40bb20, "\\IGNITION", v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0);
        esp155 = reinterpret_cast<void*>(esp105 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1 - 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1 - 1 - 1 + 1 + 1 - 1 - 1 - 1 + 1 + 2);
        if (!eax154) {
            edi156 = "\\IGNITION";
            ecx157 = -1;
            eax158 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax154) - reinterpret_cast<uint32_t>(eax154));
            do {
                if (!ecx157) 
                    break;
                --ecx157;
                ++edi156;
            } while (*edi156 != *reinterpret_cast<signed char*>(&eax158));
            ecx159 = reinterpret_cast<uint32_t>(~ecx157);
            edx160 = ecx159;
            esi161 = reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(edi156) - ecx159);
            ecx162 = -1;
            edi163 = reinterpret_cast<signed char*>(0x40bb20);
            eax164 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax158) - reinterpret_cast<uint32_t>(eax158));
            do {
                if (!ecx162) 
                    break;
                --ecx162;
                ++edi163;
                ++esi161;
            } while (*edi163 != *reinterpret_cast<signed char*>(&eax164));
            edi165 = edi163 - 1;
            ecx166 = edx160 >> 2;
            while (ecx166) {
                --ecx166;
                *edi165 = *esi161;
                edi165 = edi165 + 4;
                esi161 = esi161 + 4;
            }
            ecx167 = edx160 & 3;
            while (ecx167) {
                --ecx167;
                *edi165 = *esi161;
                ++edi165;
                ++esi161;
            }
            edi168 = reinterpret_cast<signed char*>(0x40a54c);
            ecx169 = -1;
            eax170 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax164) - reinterpret_cast<uint32_t>(eax164));
            do {
                if (!ecx169) 
                    break;
                --ecx169;
                ++edi168;
            } while (*edi168 != *reinterpret_cast<signed char*>(&eax170));
            ecx171 = reinterpret_cast<uint32_t>(~ecx169);
            edx172 = ecx171;
            esi173 = reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(edi168) - ecx171);
            ecx174 = -1;
            edi175 = reinterpret_cast<signed char*>(0x40bb20);
            eax176 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax170) - reinterpret_cast<uint32_t>(eax170));
            do {
                if (!ecx174) 
                    break;
                --ecx174;
                ++edi175;
                ++esi173;
            } while (*edi175 != *reinterpret_cast<signed char*>(&eax176));
            edi177 = edi175 - 1;
            ecx178 = edx172 >> 2;
            while (ecx178) {
                --ecx178;
                *edi177 = *esi173;
                edi177 = edi177 + 4;
                esi173 = esi173 + 4;
            }
            ecx179 = edx172 & 3;
            while (ecx179) {
                --ecx179;
                *edi177 = *esi173;
                ++edi177;
                ++esi173;
            }
        }
        fun_4022c0(ebp131, 0x3f3, 0, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0, v106, v103, v99, v97, v96, a2, a3, a4, a5);
        esp180 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp155) - 4 - 4 - 4 - 4 + 4);
        v181 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp180) + 0x74);
        eax182 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0));
        ebx183 = LoadStringA;
        ebx183(eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0);
        esp184 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp180) + 12 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        fun_4010b0(ebp131, reinterpret_cast<int32_t>(esp184) + 0x68, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, 0x7f02, v148, v146, 2, 0, v145, v143, 1, 0);
        esp185 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp184) - 4 - 4 - 4 + 4);
        v186 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp185) + 0x77c);
        GetWindowsDirectoryA();
        esp187 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp185) + 8 - 4 - 4 - 4 + 4);
        v188 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp187) + 0x774);
        fun_401000(v188, "WIN.INI", v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, v188, "WIN.INI", v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150);
        esp189 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp187) - 4 - 4 - 4 + 4);
        v190 = reinterpret_cast<void*>(0x40bb20);
        v191 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp189) + 0x270);
        lstrcpyA(v191);
        esp192 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp189) + 8 - 4 - 4 - 4 + 4);
        v193 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp192) + 0x268);
        fun_401000(v193, "SMAG.INI", v191, 0x40bb20, v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, v193, "SMAG.INI", v191, 0x40bb20, v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20);
        esp194 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp192) - 4 - 4 - 4 + 4);
        v195 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp194) + 0x77c);
        edx196 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp194) + 72);
        v197 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp194) + 0x270);
        v198 = edx196;
        eax199 = reinterpret_cast<int32_t>(SHFileOperationA(v198, v191));
        esp200 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp194) + 8 - 4 - 4 + 4);
        if (eax199) 
            goto addr_40148c_48;
        v201 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp200) + 0x268);
        v202 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp200) + 64);
        SHFileOperationA(v202, v198, v191);
        fun_4010b0(ebp131, 0x40a534, v202, v198, v191, 0x40bb20, v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, ebp131, 3, v201, 0, ebp131, 0x40a534, v202, v198, v191, 0x40bb20, v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, ebp131, 3, v201, 0);
        esp203 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp200) - 4 - 4 + 4 - 4 - 4 - 4 + 4 + 8);
        zf204 = g40a048 == 0;
        if (!zf204) {
            esi205 = v206;
            edi207 = MessageBoxA;
            goto addr_401776_51;
        }
        g40a048 = 1;
        v208 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) + 0xd84);
        eax209 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x6e, v208, 0x200, v202, v198, v191));
        ebx183(eax209, 0, 0x6e, v208, 0x200, v202, v198, v191);
        esp210 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        v211 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp210) + 0xb84);
        eax212 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191));
        ebx183(eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191);
        esp213 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp210) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        esp214 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp213) - 4);
        v215 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp213) + 0xb84);
        v216 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp214) + 0xd88);
        edi207 = MessageBoxA;
        eax217 = reinterpret_cast<uint32_t>(edi207(ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191));
        esi205 = eax217;
        fun_4010b0(ebp131, 0x40a534, ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191, 0x40bb20, v186, 0x104, eax182, ebp131, 0x40a534, ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191, 0x40bb20, v186, 0x104, eax182);
        esp203 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp214) - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 + 4 + 8);
        if (esi205 != 6) 
            goto addr_401776_51;
        v218 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) + 0x68);
        eax219 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x79, v218, 0x200, ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191));
        ebx183(eax219, 0, 0x79, v218, 0x200, ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, v191);
        esp220 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        v221 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp220) + 0x68);
        fun_4010b0(ebp131, v221, eax219, 0, 0x79, v218, 0x200, ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198, ebp131, v221, eax219, 0, 0x79, v218, 0x200, ebp131, v216, v215, 4, eax212, 0, 0x6f, v211, 0x200, eax209, 0, 0x6e, v208, 0x200, v202, v198);
        eax222 = reinterpret_cast<void*>(GetModuleHandleA());
        ebx183();
        esp223 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp220) - 4 - 4 - 4 + 4 + 8 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        fun_402790(reinterpret_cast<int32_t>(esp223) + 0x68);
        esp224 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp223) - 4 - 4 + 4);
        esp225 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp224) + 4);
        __asm__("cdq ");
        esp226 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp225) - 4);
        v227 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp224) + 100);
        v228 = reinterpret_cast<void*>(esp226 + 6);
        v229 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp225) + 24);
        v230 = reinterpret_cast<void*>(esp226 + 8);
        GetDiskFreeSpaceA("C:\\", v230, v229);
        eax231 = fun_4026a0(eax222, 0, 0, 0, "C:\\", v230, v229, v228, v227);
        eax232 = fun_4026a0(eax231, eax222, 0x81, 0, "C:\\", v230, v229, v228, v227);
        esp233 = reinterpret_cast<void*>(esp226 - 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 - 1 - 1 - 1 + 4 + 1 - 1 - 1 - 1 - 1 - 1 + 4 + 1);
        edx196 = v234;
        if (reinterpret_cast<int32_t>(eax222) > reinterpret_cast<int32_t>(edx196)) 
            goto addr_4016eb_54;
        if (reinterpret_cast<int32_t>(eax222) < reinterpret_cast<int32_t>(edx196)) 
            goto addr_401651_56;
        if (reinterpret_cast<uint32_t>(eax232) < reinterpret_cast<uint32_t>(v235)) 
            goto addr_401651_56;
        addr_4016eb_54:
        v236 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp233) + 0x68);
        eax237 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x7a, v236, 0x200, "C:\\", v230, v229));
        ebx183(eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229);
        esp238 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp233) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        v239 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp238) + 0x68);
        fun_4010b0(ebp131, v239, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229, v228, v227, eax222, 0, 0x81, eax232, eax222, eax219, 0, 0x79, v218, 0x200, ebp131, ebp131, v239, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229, v228, v227, eax222, 0, 0x81, eax232, eax222, eax219, 0, 0x79, v218, 0x200, ebp131);
        eax240 = fun_40242c(ebp131, 0, 0x10000a3f, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229);
        esi205 = eax240;
        fun_4010b0(ebp131, 0x40a534, ebp131, 0, 0x10000a3f, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229, v228, v227, eax222, 0, 0x81, eax232, eax222, eax219, 0, 0x79, ebp131, 0x40a534, ebp131, 0, 0x10000a3f, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229, v228, v227, eax222, 0, 0x81, eax232, eax222, eax219, 0, 0x79);
        esp203 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp238) - 4 - 4 - 4 + 4 + 8 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 + 4 + 8);
        if (reinterpret_cast<int32_t>(esi205) >= reinterpret_cast<int32_t>(0)) {
            addr_401776_51:
            v241 = reinterpret_cast<void*>(0x200);
            v242 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) + 0x68);
            v243 = reinterpret_cast<void*>(0);
            eax244 = reinterpret_cast<void*>(GetModuleHandleA());
            v245 = eax244;
            ebx183();
            esp246 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
            eax247 = fun_402790(reinterpret_cast<int32_t>(esp246) + 0x68);
            esp248 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp246) - 4 - 4 + 4 + 4);
            __asm__("cdq ");
            zf249 = g40bb21 == 58;
            if (!zf249 || (zf250 = g40bb22 == 92, !zf250)) {
                v190 = reinterpret_cast<void*>(0x6400000);
                v186 = reinterpret_cast<void*>(0);
            } else {
                esp251 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp248) - 4);
                v252 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp248) + 96);
                esp253 = esp251 - 1;
                v254 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp248) + 20);
                v255 = reinterpret_cast<void*>(esp253 + 8);
                v256 = reinterpret_cast<void*>(esp251 + 8);
                v257 = reinterpret_cast<void*>(esp253 + 6);
                GetDiskFreeSpaceA(v257, v256, v255, v254, v252);
                eax258 = fun_4026a0(v245, 0, 0, 0, v257, v256, v255, v254, v252);
                eax259 = fun_4026a0(eax258, 0, 0x82, 0, v257, v256, v255, v254, v252);
                esp248 = reinterpret_cast<void*>(esp253 - 1 - 1 - 1 - 1 + 1 - 1 - 1 - 1 - 1 - 1 + 4 + 1 - 1 - 1 - 1 - 1 - 1 + 4 + 1);
                v242 = eax259;
                v241 = reinterpret_cast<void*>(0);
            }
        } else {
            esp260 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp203) - 4 - 4);
            v261 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp203) + 0x68);
            if (esi205 != 0xfffffff3) {
                esp262 = reinterpret_cast<int32_t*>(esp260 - 1);
                v263 = 0x7b;
            } else {
                esp262 = reinterpret_cast<int32_t*>(esp260 - 1);
                v263 = 0x80;
            }
            eax264 = reinterpret_cast<int32_t>(GetModuleHandleA(0, v263, v261, 0x200, ebp131, 0, 0x10000a3f, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229));
            ebx183(eax264, 0, v263, v261, 0x200, ebp131, 0, 0x10000a3f, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229);
            edi207(ebp131, esp262 - 1 - 1 + 1 - 1 - 1 + 1 + 26, 0x40b810, 0, eax264, 0, v263, v261, 0x200, ebp131, 0, 0x10000a3f, eax237, 0, 0x7a, v236, 0x200, "C:\\", v230, v229);
            break;
        }
        if (reinterpret_cast<int32_t>(v186) <= reinterpret_cast<int32_t>(edx196) && (reinterpret_cast<int32_t>(v186) < reinterpret_cast<int32_t>(edx196) || reinterpret_cast<uint32_t>(v190) < eax247 << 20)) {
            v265 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp248) + 0xd84);
            eax266 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x70, v265, 0x200));
            ebx183(eax266, 0, 0x70, v265, 0x200);
            eax267 = fun_4025f0(v242, v241, 0x400, 0, eax266, 0, 0x70, v265, 0x200, v245, 0, 0x82, v242);
            eax270 = fun_4025f0(v268, v269, 0x400, 0, eax267, eax266, 0, 0x70, v265, 0x200, v245, 0, 0x82);
            esp271 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp248) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 + 16 + 4 - 4 - 4 - 4 - 4 - 4 - 4 + 16 + 4);
            esp272 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp271) - 4);
            fun_402580(esp272 + 0x4a4, reinterpret_cast<int32_t>(esp271) + 0xd88, eax270, eax267, eax266, 0, 0x70, v265, 0x200, v245, 0, 0x82);
            esp273 = reinterpret_cast<void*>(esp272 - 1 - 1 - 1 + 1);
            v274 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp273) + 0xb94);
            eax275 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200));
            ebx183(eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200);
            esp276 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp273) + 16 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
            v277 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp276) + 0xb84);
            v278 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp276) - 4 + 0x128c);
            edi207(ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200);
            eax279 = reinterpret_cast<void*>(GetDlgItem(ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200));
            EnableWindow(eax279, ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200);
            eax280 = reinterpret_cast<void*>(GetDlgItem(ebp131, 2, 1, eax279, ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200));
            EnableWindow(eax280, ebp131, 2, 1, eax279, ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200);
            eax281 = reinterpret_cast<void*>(LoadCursorA(0, 0x7f00, eax280, ebp131, 2, 1, eax279, ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200));
            SetCursor(eax281, 0, 0x7f00, eax280, ebp131, 2, 1, eax279, ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200);
            fun_4022c0(ebp131, 0x3f3, 1, eax281, 0, 0x7f00, eax280, ebp131, 2, 1, eax279, ebp131, 1, 1, ebp131, v278, v277, 0, eax275, 0, 0x71, v274, 0x200, eax266, 0, 0x70, v265, 0x200);
            --g40b808;
            goto addr_40219c_2;
        }
        v282 = reinterpret_cast<void*>(0x200);
        v283 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp248) + 0x68);
        v284 = reinterpret_cast<void*>(0x79);
        eax285 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x79, v283, 0x200));
        v286 = eax285;
        ebx183(v286, 0, 0x79, v283, 0x200);
        esp287 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp248) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        v288 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp287) + 0x68);
        fun_4010b0(ebp131, v288, v286, 0, 0x79, v283, 0x200, v245, 0, 0x82, v242, v241, v202, v198, v191, v190, v186, 0x104, eax182, 0, 0x78, v181, 0x200, ebp131, v288, v286, 0, 0x79, v283, 0x200, v245, 0, 0x82, v242, v241, v202, v198, v191, v190, v186, 0x104, eax182, 0, 0x78, v181, 0x200);
        eax289 = fun_4010b0(ebp131, 0x40a534, v286, 0, 0x79, v283, 0x200, v245, 0, 0x82, v242, v241, v202, v198, v191, v190, v186, 0x104, eax182, 0, 0x78, v181, 0x200, ebp131, 0x40a534, v286, 0, 0x79, v283, 0x200, v245, 0, 0x82, v242, v241, v202, v198, v191, v190, v186, 0x104, eax182, 0, 0x78, v181, 0x200);
        esp290 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp287) - 4 - 4 - 4 + 4 + 8 - 4 - 4 - 4 + 4);
        v291 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp290) + 0x70);
        g40bc28 = reinterpret_cast<int32_t>(-(eax289 - (eax289 + reinterpret_cast<uint1_t>(eax289 < eax289 + reinterpret_cast<uint1_t>(esi205 < 1)))));
        eax292 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x7c, v291, 0x200, v286, 0, 0x79, v283, 0x200));
        v293 = eax292;
        ebx183(v293, 0, 0x7c, v291, 0x200, v286, 0, 0x79, v283, 0x200);
        esp294 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp290) + 8 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        esi295 = reinterpret_cast<void**>(0x40a050);
        v296 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp294) + 0x68);
        fun_4010b0(ebp131, v296, v293, 0, 0x7c, v291, 0x200, v286, 0, 0x79, v283, 0x200, v245, 0, 0x82, v242, v241, v202, v198, v191, v190, v186, 0x104, ebp131, v296, v293, 0, 0x7c, v291, 0x200, v286, 0, 0x79, v283, 0x200, v245, 0, 0x82, v242, v241, v202, v198, v191, v190, v186, 0x104);
        esp297 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp294) - 4 - 4 - 4 + 4 + 8);
        while (1) {
            v298 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) + 0x774);
            lstrcpyA(v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245, *reinterpret_cast<int16_t*>(&v243));
            esp299 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) - 4 - 4 - 4 + 4);
            v300 = *esi295;
            v301 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp299) + 0x774);
            fun_401000(v301, v300, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245, v301, v300, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245);
            esp302 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp299) - 4 - 4 - 4 + 4);
            v303 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp302) + 0x270);
            lstrcpyA(v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245, *reinterpret_cast<int16_t*>(&v243));
            esp304 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp302) + 8 - 4 - 4 - 4 + 4);
            v305 = *esi295;
            v306 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp304) + 0x268);
            fun_401000(v306, v305, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v306, v305, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283);
            esp307 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp304) - 4 - 4 - 4 + 4);
            v308 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp307) + 0x70);
            eax309 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245, *reinterpret_cast<int16_t*>(&v243)));
            v310 = eax309;
            ebx183(v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245, *reinterpret_cast<int16_t*>(&v243));
            esp311 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp307) + 8 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
            v312 = *esi295;
            v313 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp311) + 0x68);
            fun_4010b0(ebp131, v313, v312, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245, ebp131, v313, v312, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, v283, v282, v245);
            esp314 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp311) - 4 - 4 - 4 - 4 + 4);
            v284 = ebp131;
            v282 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp314) + 0x780);
            v245 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp314) + 0x274);
            esp297 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp314) + 12);
            *reinterpret_cast<int16_t*>(&v243) = 0x214;
            v283 = reinterpret_cast<void*>(2);
            do {
                v315 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) + 64);
                eax316 = reinterpret_cast<int32_t>(SHFileOperationA(v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, 0x214));
                esp297 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) - 4 - 4 + 4);
                if (!eax316) 
                    goto addr_401aae_69;
                v317 = reinterpret_cast<void*>(0x200);
                v318 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) + 0x68);
                eax319 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x7e, v318, 0x200, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, 0x214));
                ebx183(eax319, 0, 0x7e, v318, 0x200, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, 0x214);
                esp320 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v321 = *esi295;
                wsprintfA(reinterpret_cast<int32_t>(esp320) + 0x36c, reinterpret_cast<int32_t>(esp320) + 0x68, v321, eax319, 0, 0x7e, v318, 0x200, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, 0x214);
                esp322 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp320) - 4 - 4 - 4 - 4 + 4);
                eax323 = reinterpret_cast<int32_t>(edi207(ebp131, reinterpret_cast<int32_t>(esp322) + 0x378, 0x40b810, 5, eax319, 0, 0x7e, v318, 0x200, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, 0x214));
                esp297 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp322) + 12 - 4 - 4 - 4 - 4 - 4 + 4);
            } while (eax323 != 2);
            if (1) {
                addr_401ac1_72:
                ++esi295;
                if (reinterpret_cast<uint32_t>(esi295) >= 0x40a4e8) 
                    break;
            } else {
                addr_401aae_69:
                v317 = reinterpret_cast<void*>(0x80);
                v318 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) + 0x268);
                SetFileAttributesA(v318, 0x80, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, 0x214);
                esp297 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) - 4 - 4 - 4 + 4);
                goto addr_401ac1_72;
            }
        }
        fun_4010b0(ebp131, 0x40a534, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, ebp131, 0x40a534, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2);
        esp324 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp297) - 4 - 4 - 4 + 4);
        v325 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp324) + 0x70);
        eax326 = reinterpret_cast<void*>(GetModuleHandleA());
        v327 = eax326;
        ebx183(v327);
        esp328 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp324) + 8 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        v329 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp328) + 0x68);
        fun_4010b0(ebp131, v329, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, ebp131, v329, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291);
        eax330 = reinterpret_cast<int32_t>(RegCreateKeyExA(0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        esp331 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp328) - 4 - 4 - 4 + 4 + 8 - 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 4);
        if (eax330) 
            goto addr_401b35_74;
        addr_401b3f_75:
        edi332 = reinterpret_cast<signed char*>(0x40bb20);
        ecx333 = -1;
        eax334 = eax330 - eax330;
        do {
            if (!ecx333) 
                break;
            --ecx333;
            ++edi332;
        } while (*edi332 != *reinterpret_cast<signed char*>(&eax334));
        ecx335 = reinterpret_cast<uint32_t>(~ecx333);
        eax336 = ecx335;
        ecx337 = ecx335 >> 2;
        while (ecx337) {
            --ecx337;
        }
        ecx338 = eax336 & 3;
        while (ecx338) {
            --ecx338;
        }
        ecx339 = -1;
        eax340 = eax336 - eax336;
        do {
            if (!ecx339) 
                break;
            --ecx339;
        } while (v341 != *reinterpret_cast<signed char*>(&eax340));
        v342 = reinterpret_cast<void*>(~ecx339 - 1);
        edx343 = g40a4e8;
        v344 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp331) + 0x1088);
        v345 = edx343;
        RegSetValueExA(v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        ecx346 = g40a4e8;
        v347 = ecx346;
        RegCloseKey(v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp348 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp331) - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        g40a4e8 = reinterpret_cast<void*>(0);
        v349 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp348) + 0x1688);
        eax350 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        v351 = eax350;
        ebx183(v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp352 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp348) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        v353 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp352) + 100);
        SHGetSpecialFolderLocation(0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp354 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp352) - 4 - 4 - 4 - 4 + 4);
        v355 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp354) + 0xf84);
        SHGetPathFromIDListA(0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp356 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp354) - 4 - 4 - 4 + 4);
        esp357 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp356) - 4 - 4 - 4);
        edi358 = reinterpret_cast<signed char*>(0x40a4fc);
        eax359 = reinterpret_cast<int32_t>(wsprintfA(reinterpret_cast<int32_t>(esp357) + 0x67c, "%s\\%s", reinterpret_cast<int32_t>(esp356) + 0xf84, reinterpret_cast<int32_t>(esp356) + 0x1688, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        esp360 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp357) - 4 - 4 + 4 + 16);
        ecx361 = -1;
        eax362 = eax359 - eax359;
        do {
            if (!ecx361) 
                break;
            --ecx361;
            ++edi358;
        } while (*edi358 != *reinterpret_cast<signed char*>(&eax362));
        edx363 = reinterpret_cast<uint32_t>(~ecx361);
        ecx364 = -1;
        eax365 = eax362 - eax362;
        do {
            if (!ecx364) 
                break;
            --ecx364;
        } while (v366 != *reinterpret_cast<signed char*>(&eax365));
        ecx367 = edx363 >> 2;
        while (ecx367) {
            --ecx367;
        }
        esp368 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp360) - 4);
        ecx369 = edx363 & 3;
        while (ecx369) {
            --ecx369;
        }
        v370 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp368) + 0x674);
        CreateDirectoryA(v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp371 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp368) - 4 - 4 + 4);
        v372 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp371) + 0x670);
        SHChangeNotify(8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp373 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp371) - 4 - 4 - 4 - 4 - 4 + 4);
        ecx374 = image_base_;
        wsprintfA(reinterpret_cast<int32_t>(esp373) + 0xa80, "%s\\%s", 0x40bb20, ecx374, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp375 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp373) - 4 - 4 - 4 - 4 - 4 + 4);
        v376 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp375) + 0x888);
        eax377 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        ebx183(eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp378 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp375) + 16 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        wsprintfA(reinterpret_cast<int32_t>(esp378) + 0x97c, "%s\\%s.lnk", reinterpret_cast<int32_t>(esp378) + 0x670, reinterpret_cast<int32_t>(esp378) + 0x878, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp379 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp378) - 4 - 4 - 4 - 4 - 4 + 4 + 16);
        esp380 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp379) - 4);
        v381 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp379) + 0x97c);
        v382 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp380) + 0xa84);
        fun_402360(v382, v381, 0x40a534, 0x40bb20, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245, v382, v381, 0x40a534, 0x40bb20, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, 0, v284, 2, v282, v245);
        esp383 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp380) - 4 - 4 - 4 - 4 + 4);
        ecx384 = image_base_;
        wsprintfA(reinterpret_cast<int32_t>(esp383) + 0xa90, "%s\\%s", 0x40bb20, ecx384, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp385 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp383) + 16 - 4 - 4 - 4 - 4 - 4 + 4);
        v386 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp385) + 0x888);
        eax387 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        ebx183(eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp388 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp385) + 16 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        wsprintfA(reinterpret_cast<int32_t>(esp388) + 0x97c, "%s\\%s.lnk", reinterpret_cast<int32_t>(esp388) + 0x670, reinterpret_cast<int32_t>(esp388) + 0x878, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp389 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp388) - 4 - 4 - 4 - 4 - 4 + 4);
        v390 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp389) + 0x98c);
        v391 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp389) + 0xa90);
        fun_402360(v391, v390, 0x40a534, 0x40bb20, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286, v391, v390, 0x40a534, 0x40bb20, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, 0, 0x7c, v291, 0x200, v286);
        esp392 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp389) + 16 - 4 - 4 - 4 - 4 - 4 + 4);
        ecx393 = image_base_;
        wsprintfA(reinterpret_cast<int32_t>(esp392) + 0xa90, "%s\\%s", 0x40bb20, ecx393, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp394 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp392) + 16 - 4 - 4 - 4 - 4 - 4 + 4);
        v395 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp394) + 0x888);
        eax396 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        ebx183(eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp397 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp394) + 16 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        esp398 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp397) - 4 - 4);
        wsprintfA(esp398 + 0x261, "%s\\%s.lnk", reinterpret_cast<int32_t>(esp397) + 0x670, reinterpret_cast<int32_t>(esp397) + 0x878, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp399 = reinterpret_cast<void*>(esp398 - 1 - 1 - 1 + 1);
        v400 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp399) + 0x98c);
        v401 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp399) + 0xa90);
        fun_402360(v401, v400, 0x40a534, 0x40a534, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293, v401, v400, 0x40a534, 0x40a534, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327, 0, 0x7f, v325, 0x200, v318, v317, v315, v310, 0, 0x7d, v308, 0x200, v303, 0x40bb20, v298, 0x40ba18, v293);
        fun_4010b0(ebp131, 0x40a534, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, ebp131, 0x40a534, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0);
        if (v402 >= 0) {
            eax403 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
            esi404 = ShowWindow;
            esi404(eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
            eax405 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
            esi404(eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
            eax406 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
            esi404(eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
            eax407 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
            esi404(eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
            eax408 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
            esi404(eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
            eax409 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
            esi404(eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
            esp410 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp399) + 16 - 4 - 4 - 4 - 4 - 4 + 4 + 16 - 4 - 4 - 4 + 4 + 8 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
            zf411 = g40bc28 == 0;
            esp412 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp410) - 4 - 4);
            v413 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp410) + 0x68);
            if (zf411) {
                eax414 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp415 = reinterpret_cast<void*>(esp412 - 1 - 1 - 1 + 1 - 1 - 1 + 1);
                v416 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp415) + 0x68);
                SetDlgItemTextA(ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                eax417 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                esi404(eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp418 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp415) - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v419 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp418) + 0x68);
                eax420 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x74, v419, 0x200, eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax420, 0, 0x74, v419, 0x200, eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                v421 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp418) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 + 0x68);
                eax422 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 1, v421, eax420, 0, 0x74, v419, 0x200, eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                SetWindowTextA(eax422, ebp131, 1, v421, eax420, 0, 0x74, v419, 0x200, eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                ++g40b804;
                eax423 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 1, 1, eax422, ebp131, 1, v421, eax420, 0, 0x74, v419, 0x200, eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                EnableWindow(eax423, ebp131, 1, 1, eax422, ebp131, 1, v421, eax420, 0, 0x74, v419, 0x200, eax417, ebp131, 0x3ee, 5, ebp131, 0x3ee, v416, eax414, 0, 0x3ed, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                --g40b808;
                goto addr_40219c_2;
            } else {
                eax424 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp425 = reinterpret_cast<void*>(esp412 - 1 - 1 - 1 + 1 - 1 - 1 + 1);
                edi426 = SetDlgItemTextA;
                v427 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp425) + 0x68);
                edi426(ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                eax428 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                esi404(eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp429 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp425) - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v430 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp429) + 0x68);
                eax431 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp432 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp429) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v433 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp432) + 0x68);
                edi426(ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                eax434 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                esi404(eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp435 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp432) - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v436 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp435) + 0x68);
                eax437 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp438 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp435) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v439 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp438) + 0x68);
                eax440 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                esi441 = SetWindowTextA;
                esi441(eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp442 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp438) - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v443 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp442) + 0x68);
                eax444 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax444, 0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp445 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp442) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v446 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp445) + 0x68);
                eax447 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 2, v446, eax444, 0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                esi441(eax447, ebp131, 2, v446, eax444, 0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                esp448 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp445) - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
                v449 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp448) + 0x68);
                eax450 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x77, v449, 0x200, eax447, ebp131, 2, v446, eax444, 0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
                ebx183(eax450, 0, 0x77, v449, 0x200, eax447, ebp131, 2, v446, eax444, 0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                fun_401050(ebp131, 0x3ea, reinterpret_cast<int32_t>(esp448) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 + 0x68, 0x10e, 0xc3, eax450, 0, 0x77, v449, 0x200, eax447, ebp131, 2, v446, eax444, 0, 0x83, v443, 0x200, eax440, ebp131, 1, v439, eax437, 0, 0x75, v436, 0x200, eax434, ebp131, 0x3ee, 5, ebp131, 0x3ee, v433, eax431, 0, 0x3ec, v430, 0x200, eax428, ebp131, 0x3ed, 5, ebp131, 0x3ed, v427, eax424, 0, 0x3eb, v413, 0x200, eax409, ebp131, 0x3f3, 0, eax408, ebp131, 0x3f1, 0, eax407, ebp131, 0x3e9, 0, eax406, ebp131, 0x3ee, 0, eax405, ebp131, 0x3ed, 0, eax403, ebp131, 0x3e8, 0, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
                ++g40b804;
            }
        }
        eax451 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 1, 1, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        EnableWindow(eax451, ebp131, 1, 1, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        eax452 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 2, 1, eax451, ebp131, 1, 1, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327));
        EnableWindow(eax452, ebp131, 2, 1, eax451, ebp131, 1, 1, eax396, 0, 0x69, v395, 0x104, eax387, 0, 0x68, v386, 0x104, eax377, 0, 0x67, v376, 0x104, 8, 1, v372, 0, v370, 0, 0, v355, 0, 2, v353, v351, 0, 0x66, v349, 0x200, v347, v345, "Ignition Path", 0, 1, v344, v342, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        --g40b808;
        if (v453 >= 0) 
            goto addr_40219c_2;
        break;
        addr_401b35_74:
        eax330 = fun_402470(1, 0x80000002, "Software\\UDS\\Ignition", 0, 0, 0, 0xf003f, 0, 0x40a4e8, 0x40b800, v327);
        esp331 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp331) - 4 - 4 + 4 + 4);
        goto addr_401b3f_75;
        addr_401651_56:
        v454 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp233) + 0xd84);
        eax455 = reinterpret_cast<void*>(GetModuleHandleA(0, 0x6c, v454, 0x200, "C:\\", v230, v229));
        ebx183(eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229);
        eax456 = fun_4025f0(v228, v227, 0x400, 0, eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229, v228);
        eax459 = fun_4025f0(v457, v458, 0x400, 0, eax456, eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229);
        esp460 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp233) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 + 16 + 4 - 4 - 4 - 4 - 4 - 4 - 4 + 16 + 4);
        esp461 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp460) - 4);
        fun_402580(esp461 + 0x4a4, reinterpret_cast<int32_t>(esp460) + 0xd88, eax459, eax456, eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229);
        esp462 = reinterpret_cast<void*>(esp461 - 1 - 1 - 1 + 1);
        v463 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp462) + 0xb94);
        eax464 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x6d, v463, 0x200, eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229));
        ebx183(eax464, 0, 0x6d, v463, 0x200, eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229);
        esp465 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp462) + 16 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        edi207(ebp131, reinterpret_cast<int32_t>(esp465) - 4 + 0x128c, reinterpret_cast<int32_t>(esp465) + 0xb84, 0, eax464, 0, 0x6d, v463, 0x200, eax455, 0, 0x6c, v454, 0x200, "C:\\", v230, v229);
        goto addr_402193_105;
        addr_40148c_48:
        fun_4010b0(ebp131, 0x40a534, v198, v191, 0x40bb20, v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, ebp131, 2, v195, v197, ebp131, 0x40a534, v198, v191, 0x40bb20, v186, 0x104, eax182, 0, 0x78, v181, 0x200, v153, v151, 0x3e8, 0x40bb20, 0x106, v150, 0, ebp131, 2, v195, v197);
        eax466 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 1, 1, v198, v191));
        EnableWindow(eax466, ebp131, 1, 1, v198, v191);
        eax467 = reinterpret_cast<int32_t>(GetDlgItem(ebp131, 2, 1, eax466, ebp131, 1, 1, v198, v191));
        EnableWindow(eax467, ebp131, 2, 1, eax466, ebp131, 1, 1, v198, v191);
        --g40b808;
        goto addr_40219c_2;
    case 3:
        if (v468 != 0xf060) 
            goto addr_40219c_2;
        v133 = reinterpret_cast<void*>(0x200);
        v469 = reinterpret_cast<void*>(esp105 + 0x361);
        eax470 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x6a, v469, 0x200));
        ebx471 = LoadStringA;
        ebx471(eax470, 0, 0x6a, v469, 0x200);
        esp472 = reinterpret_cast<void*>(esp105 - 1 - 1 - 1 - 1 - 1 + 1 - 1 - 1 + 1);
        v473 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp472) + 0xb84);
        eax474 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0x6b, v473, 0x200, eax470, 0, 0x6a, v469, 0x200));
        ebx471(eax474, 0, 0x6b, v473, 0x200, eax470, 0, 0x6a, v469, 0x200);
        esp475 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp472) - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4);
        ebp131 = v476;
        eax477 = reinterpret_cast<int32_t>(MessageBoxA(ebp131, reinterpret_cast<int32_t>(esp475) - 4 + 0xd88, reinterpret_cast<int32_t>(esp475) + 0xb84, 4, eax474, 0, 0x6b, v473, 0x200, eax470, 0, 0x6a, v469, 0x200));
        if (eax477 != 6) 
            goto addr_40219c_2; else 
            goto addr_402193_105;
    }
    addr_402195_108:
    EndDialog(ebp131, v133);
    goto addr_40219c_2;
    addr_402193_105:
    goto addr_402195_108;
}

int32_t GetVersion = 0xd726;

uint32_t g40a584 = 0;

int32_t g40a590 = 0;

uint32_t g40a58c = 0;

uint32_t g40a588 = 0;

int32_t GetCommandLineA = 0xd714;

void fun_402850() {
    struct s6* eax1;
    void* esp2;
    void* ebp3;
    struct s6* v4;
    void* v5;
    void* ebx6;
    void* v7;
    void* esi8;
    void* v9;
    void* edi10;
    uint32_t eax11;
    int32_t eax12;
    uint32_t eax13;
    uint32_t eax14;
    uint32_t tmp32_15;
    int32_t eax16;
    struct s0** eax17;
    struct s0* eax18;
    int1_t zf19;
    void* v20;
    void* v21;
    void* v22;
    void* v23;
    void* v24;
    void* v25;
    void* v26;
    struct s0** esi27;
    int32_t eax28;
    int32_t eax29;
    void* eax30;
    uint32_t v31;
    int32_t eax32;
    int32_t eax33;
    void* v34;
    void* v35;
    void* v36;
    void* v37;
    void* v38;

    eax1 = *reinterpret_cast<struct s6**>(&g0);
    esp2 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    ebp3 = esp2;
    v4 = eax1;
    *reinterpret_cast<struct s6**>(&g0) = reinterpret_cast<struct s6*>(reinterpret_cast<int32_t>(esp2) - 4 - 4 - 4 - 4);
    v5 = ebx6;
    v7 = esi8;
    v9 = edi10;
    eax11 = reinterpret_cast<uint32_t>(GetVersion());
    g40a584 = eax11;
    eax12 = 0;
    *reinterpret_cast<signed char*>(&eax12) = *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&g40a584) + 1);
    g40a590 = eax12;
    eax13 = g40a584;
    g40a584 = g40a584 >> 16;
    eax14 = eax13 & 0xff;
    g40a58c = eax14;
    tmp32_15 = (eax14 << 8) + g40a590;
    g40a588 = tmp32_15;
    eax16 = fun_404430();
    if (!eax16) {
        fun_4029f0(28);
    }
    fun_404250();
    fun_404240();
    eax17 = reinterpret_cast<struct s0**>(GetCommandLineA());
    g40cf64 = eax17;
    eax18 = fun_403df0();
    g40a5d0 = eax18;
    if (!eax18 || (zf19 = g40cf64 == 0, zf19)) {
        fun_402470(0xff, v9, v7, v5, v20, v21, v22, v23, v24, v25, v26);
    }
    fun_403b70();
    fun_403a80();
    fun_402440();
    esi27 = g40cf64;
    if (*esi27 == 34) {
        ++esi27;
        if (*esi27 == 34) {
            addr_402954_7:
            ++esi27;
        } else {
            do {
                if (!*esi27) 
                    break;
                eax28 = 0;
                *reinterpret_cast<struct s0**>(&eax28) = *esi27;
                eax29 = fun_403a20(eax28);
                if (eax29) {
                    ++esi27;
                }
                ++esi27;
            } while (!reinterpret_cast<int1_t>(*esi27 == 34));
            if (reinterpret_cast<int1_t>(*esi27 == 34)) 
                goto addr_402954_7;
        }
    } else {
        if (reinterpret_cast<unsigned char>(*esi27) > 32) {
            do {
                ++esi27;
            } while (reinterpret_cast<unsigned char>(*esi27) > 32);
        }
    }
    if (*esi27) {
        do {
            if (reinterpret_cast<unsigned char>(*esi27) > 32) 
                break;
            ++esi27;
        } while (*esi27);
    }
    GetStartupInfoA();
    eax30 = reinterpret_cast<void*>(10);
    if (!1) {
        eax30 = reinterpret_cast<void*>(v31 & 0xffff);
    }
    eax32 = reinterpret_cast<int32_t>(GetModuleHandleA(0, 0, esi27));
    eax33 = fun_4022e0(eax32, 0, 0, esi27);
    fun_402470(eax33, eax30, reinterpret_cast<int32_t>(ebp3) + 0xffffff90, v9, v7, v5, v34, v35, v36, v37, v38);
    *reinterpret_cast<struct s6**>(&g0) = v4;
    return;
}

uint32_t fun_404bd0() {
    uint32_t eax1;
    int1_t zf2;
    uint32_t esi3;
    uint32_t edi4;
    int1_t less_or_equal5;
    void* ebx6;
    struct s0* eax7;
    struct s31* v8;
    int32_t eax9;
    struct s0* eax10;
    struct s0* v11;
    struct s0* ecx12;
    int1_t less13;

    eax1 = fun_405be0();
    zf2 = g40a5b8 == 0;
    if (zf2) {
        return eax1;
    }
    esi3 = 0;
    edi4 = 3;
    less_or_equal5 = reinterpret_cast<int32_t>(g40ce50) <= reinterpret_cast<int32_t>(3);
    if (!less_or_equal5) 
        goto addr_405ab3_5;
    addr_405b0c_6:
    return esi3;
    addr_405ab3_5:
    ebx6 = reinterpret_cast<void*>(12);
    do {
        eax7 = g40be40;
        if (*reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax7) + reinterpret_cast<uint32_t>(ebx6))) {
            if ((*reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax7) + reinterpret_cast<uint32_t>(ebx6)))->fc & 0x83 && (v8 = *reinterpret_cast<struct s31**>(reinterpret_cast<unsigned char>(eax7) + reinterpret_cast<uint32_t>(ebx6)), eax9 = fun_405e10(v8), eax9 != -1)) {
                ++esi3;
            }
            if (reinterpret_cast<int32_t>(ebx6) >= reinterpret_cast<int32_t>(80)) {
                eax10 = g40be40;
                v11 = *reinterpret_cast<struct s0**>(reinterpret_cast<unsigned char>(eax10) + reinterpret_cast<uint32_t>(ebx6));
                fun_404eb0(v11);
                ecx12 = g40be40;
                *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(ecx12) + reinterpret_cast<uint32_t>(ebx6)) = 0;
            }
        }
        ebx6 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ebx6) + 4);
        ++edi4;
        less13 = reinterpret_cast<int32_t>(edi4) < reinterpret_cast<int32_t>(g40ce50);
    } while (less13);
    goto addr_405b0c_6;
}

struct s0* fun_405c80() {
    struct s0* eax1;

    eax1 = fun_4029f0(2);
    return eax1;
}

struct s39 {
    signed char[4] pad4;
    int32_t f4;
};

void fun_405ceb(int32_t a1) {
    int16_t* edi2;
    int16_t* esi3;
    struct s39* ebp4;

    *edi2 = *esi3;
    goto ebp4->f4;
}

struct s40 {
    signed char[4] pad4;
    int32_t f4;
};

void fun_405d2d(int32_t a1) {
    int32_t ecx2;
    signed char* edi3;
    signed char* esi4;
    struct s40* ebp5;

    while (ecx2) {
        --ecx2;
        *edi3 = *esi4;
        ++edi3;
        ++esi4;
    }
    goto ebp5->f4;
}

struct s41 {
    signed char[4] pad4;
    int32_t f4;
};

struct s42 {
    int32_t f0;
    signed char f1;
    int16_t f2;
    signed char f3;
};

struct s43 {
    int32_t f0;
    signed char f1;
    int16_t f2;
    signed char f3;
};

struct s44 {
    signed char[4] pad4;
    int32_t f4;
};

struct s45 {
    signed char[4] pad4;
    int32_t f4;
};

struct s46 {
    signed char[4] pad4;
    int32_t f4;
};

void fun_405d39(void* ecx, int32_t a2) {
    void* esi3;
    void* esi4;
    void* edi5;
    void* edi6;
    signed char* esi7;
    signed char* edi8;
    struct s41* ebp9;
    uint32_t edx10;
    int32_t edx11;
    void* eax12;
    uint32_t ecx13;
    uint32_t eax14;
    struct s42* esi15;
    struct s43* edi16;
    uint32_t ecx17;
    uint32_t edx18;
    uint32_t ecx19;
    struct s44* ebp20;
    struct s45* ebp21;
    struct s46* ebp22;

    esi3 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esi4) + reinterpret_cast<uint32_t>(ecx));
    edi5 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(edi6) + reinterpret_cast<uint32_t>(ecx));
    if (reinterpret_cast<uint32_t>(edi5) & 3) {
        esi7 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi3) - 1);
        edi8 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi5) - 1);
        if (reinterpret_cast<uint32_t>(ecx) <= 12) {
            while (ecx) {
                ecx = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ecx) - 1);
                *edi8 = *esi7;
                --edi8;
                --esi7;
            }
            goto ebp9->f4;
        }
        edx10 = reinterpret_cast<uint32_t>(-edx11) & 3;
        eax12 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(ecx) - edx10);
        ecx13 = edx10;
        while (ecx13) {
            --ecx13;
            *edi8 = *esi7;
            --edi8;
            --esi7;
        }
        eax14 = reinterpret_cast<uint32_t>(eax12) & 3;
        esi15 = reinterpret_cast<struct s42*>(esi7 - 3);
        edi16 = reinterpret_cast<struct s43*>(edi8 - 3);
        ecx17 = reinterpret_cast<uint32_t>(eax12) >> 2;
        while (ecx17) {
            --ecx17;
            edi16->f0 = esi15->f0;
            edi16 = reinterpret_cast<struct s43*>(reinterpret_cast<uint32_t>(edi16) - 4);
            esi15 = reinterpret_cast<struct s42*>(reinterpret_cast<uint32_t>(esi15) - 4);
        }
        goto *reinterpret_cast<int32_t*>(eax14 * 4 + 0x405d60);
    } else {
        edx18 = reinterpret_cast<uint32_t>(ecx) & 3;
        esi15 = reinterpret_cast<struct s42*>(reinterpret_cast<uint32_t>(esi3) - 4);
        edi16 = reinterpret_cast<struct s43*>(reinterpret_cast<uint32_t>(edi5) - 4);
        ecx19 = reinterpret_cast<uint32_t>(ecx) >> 2;
        while (ecx19) {
            --ecx19;
            edi16->f0 = esi15->f0;
            edi16 = reinterpret_cast<struct s43*>(reinterpret_cast<uint32_t>(edi16) - 4);
            esi15 = reinterpret_cast<struct s42*>(reinterpret_cast<uint32_t>(esi15) - 4);
        }
        goto *reinterpret_cast<int32_t*>(edx18 * 4 + 0x405d60);
    }
    addr_405d9e_19:
    goto ebp20->f4;
    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi16) + 3) = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi15) + 3);
    goto addr_405d9e_19;
    *reinterpret_cast<int16_t*>(reinterpret_cast<uint32_t>(edi16) + 2) = *reinterpret_cast<int16_t*>(reinterpret_cast<uint32_t>(esi15) + 2);
    goto ebp21->f4;
    *reinterpret_cast<int16_t*>(reinterpret_cast<uint32_t>(edi16) + 2) = *reinterpret_cast<int16_t*>(reinterpret_cast<uint32_t>(esi15) + 2);
    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi16) + 1) = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi15) + 1);
    goto ebp22->f4;
}

void fun_405d86() {
}

int32_t fun_4029a3() {
    int32_t ebp1;
    int32_t ebp2;
    int32_t v3;
    int32_t ebp4;
    int32_t v5;
    int32_t ebp6;
    int32_t eax7;

    *reinterpret_cast<int32_t*>(ebp1 - 32) = ***reinterpret_cast<int32_t***>(ebp2 - 20);
    v3 = *reinterpret_cast<int32_t*>(ebp4 - 20);
    v5 = *reinterpret_cast<int32_t*>(ebp6 - 32);
    eax7 = fun_403890(v5, v3);
    return eax7;
}

void fun_4036d5() {
}

struct s47 {
    signed char[4] pad4;
    uint32_t f4;
};

int32_t fun_4037bc(struct s47* a1, int32_t a2);

int32_t fun_403846() {
    int32_t eax1;
    struct s6* ecx2;

    eax1 = 0;
    ecx2 = *reinterpret_cast<struct s6**>(&g0);
    if (reinterpret_cast<int1_t>(ecx2->f4 == fun_4037bc) && ecx2->f8 == ecx2->fc->fc) {
        eax1 = 1;
    }
    return eax1;
}

void fun_403869(int32_t ecx) {
    goto 0x40387c;
}

struct s48 {
    signed char[4] pad4;
    uint32_t f4;
};

struct s49 {
    signed char[12] pad12;
    struct s18* fc;
};

int32_t fun_40446c(struct s48* a1, struct s18* a2, int32_t a3) {
    unsigned char* esi4;
    unsigned char dh5;
    unsigned char* eax6;
    struct s49* ebp7;
    struct s18* ebx8;
    int32_t eax9;
    int32_t esi10;
    int32_t* edi11;
    int32_t ecx12;
    int32_t eax13;
    int32_t* edi14;
    int32_t ecx15;

    *esi4 = reinterpret_cast<unsigned char>(*esi4 ^ reinterpret_cast<unsigned char>(dh5 ^ *eax6));
    ebp7 = reinterpret_cast<struct s49*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 + 4 - 4);
    ebx8 = a2;
    if (a1->f4 & 6) {
        fun_4037de(ebx8, 0xff);
        eax9 = 1;
    } else {
        *reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebx8) - 4) = reinterpret_cast<void*>(reinterpret_cast<int32_t>(ebp7) - 8);
        esi10 = ebx8->fc;
        edi11 = ebx8->f8;
        while (esi10 != -1) {
            ecx12 = esi10 + esi10 * 2;
            if ((edi11 + ecx12)[1] && (eax13 = reinterpret_cast<int32_t>((edi11 + ecx12)[1]()), ebp7 = ebp7, esi10 = esi10, ebx8 = ebp7->fc, !!eax13)) {
                if (__intrinsic()) 
                    goto addr_404506_7;
                edi14 = ebx8->f8;
                fun_40379c(ebx8);
                ebp7 = reinterpret_cast<struct s49*>(ebx8 + 1);
                fun_4037de(ebx8, esi10);
                ecx15 = esi10 + esi10 * 2;
                fun_403872(ecx15, 1);
                ebx8->fc = edi14[ecx15];
                (edi14 + ecx15)[2]();
            }
            edi11 = ebx8->f8;
            esi10 = edi11[esi10 + esi10 * 2];
        }
        goto addr_40450d_10;
    }
    addr_404529_11:
    return eax9;
    addr_40450d_10:
    eax9 = 1;
    goto addr_404529_11;
    addr_404506_7:
    eax9 = 0;
    goto addr_404529_11;
}

int32_t fun_4037bc(struct s47* a1, int32_t a2) {
    int32_t eax3;
    int32_t* v4;

    eax3 = 1;
    if (a1->f4 & 6) {
        *v4 = a2;
        eax3 = 3;
    }
    return eax3;
}

void fun_404b00() {
    int1_t zf1;
    int1_t less2;
    uint32_t eax3;
    struct s0* eax4;
    struct s0* eax5;
    int32_t ecx6;
    void* eax7;
    struct s0* edx8;
    uint32_t esi9;
    int32_t* edx10;

    zf1 = g40ce50 == 0;
    if (!zf1) {
        less2 = reinterpret_cast<int32_t>(g40ce50) < reinterpret_cast<int32_t>(20);
        if (less2) {
            g40ce50 = 20;
        }
    } else {
        g40ce50 = 0x200;
    }
    eax3 = g40ce50;
    eax4 = fun_405a00(eax3, 4);
    g40be40 = eax4;
    if (!eax4 && (g40ce50 = 20, eax5 = fun_405a00(20, 4), g40be40 = eax5, !eax5)) {
        fun_4029f0(26);
    }
    ecx6 = 0x40ab78;
    eax7 = reinterpret_cast<void*>(0);
    do {
        edx8 = g40be40;
        eax7 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax7) + 4);
        *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(edx8) + reinterpret_cast<uint32_t>(eax7) + 0xfffffffc) = ecx6;
        ecx6 = ecx6 + 32;
    } while (reinterpret_cast<int32_t>(eax7) < reinterpret_cast<int32_t>(80));
    esi9 = 0;
    edx10 = reinterpret_cast<int32_t*>(0x40ab88);
    do {
        if ((*reinterpret_cast<int32_t**>((reinterpret_cast<int32_t>(esi9 & 0xffffffe7) >> 3) + 0x40ce60))[(esi9 & 31) * 2] == -1 || !(*reinterpret_cast<int32_t**>((reinterpret_cast<int32_t>(esi9 & 0xffffffe7) >> 3) + 0x40ce60))[(esi9 & 31) * 2]) {
            *edx10 = -1;
        }
        edx10 = edx10 + 8;
        ++esi9;
    } while (reinterpret_cast<uint32_t>(edx10) < 0x40abe8);
    return;
}

struct s50 {
    signed char f0;
    signed char[1] pad2;
    signed char f2;
};

struct s50* fun_405c90(struct s50* a1, struct s50* a2, void* a3) {
    struct s50* esi4;
    struct s50* edi5;
    uint32_t edx6;
    void* eax7;
    uint32_t ecx8;
    uint32_t eax9;
    uint32_t ecx10;
    uint32_t edx11;
    uint32_t ecx12;

    esi4 = a2;
    edi5 = a1;
    if (reinterpret_cast<uint32_t>(edi5) > reinterpret_cast<uint32_t>(esi4)) {
        if (reinterpret_cast<uint32_t>(edi5) < reinterpret_cast<uint32_t>(esi4) + reinterpret_cast<uint32_t>(a3)) 
            goto 0x405d3c;
    }
    if (reinterpret_cast<uint32_t>(edi5) & 3) {
        if (reinterpret_cast<uint32_t>(a3) <= 12) 
            goto 0x405d30;
        edx6 = -reinterpret_cast<uint32_t>(edi5) & 3;
        eax7 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(a3) - edx6);
        ecx8 = edx6;
        while (ecx8) {
            --ecx8;
            edi5->f0 = esi4->f0;
            edi5 = reinterpret_cast<struct s50*>(&edi5->pad2);
            esi4 = reinterpret_cast<struct s50*>(&esi4->pad2);
        }
        eax9 = reinterpret_cast<uint32_t>(eax7) & 3;
        ecx10 = reinterpret_cast<uint32_t>(eax7) >> 2;
        while (ecx10) {
            --ecx10;
            edi5->f0 = esi4->f0;
            edi5 = reinterpret_cast<struct s50*>(reinterpret_cast<uint32_t>(edi5) + 4);
            esi4 = reinterpret_cast<struct s50*>(reinterpret_cast<uint32_t>(esi4) + 4);
        }
        goto *reinterpret_cast<int32_t*>(eax9 * 4 + 0x405cc8);
    } else {
        edx11 = reinterpret_cast<uint32_t>(a3) & 3;
        ecx12 = reinterpret_cast<uint32_t>(a3) >> 2;
        while (ecx12) {
            --ecx12;
            edi5->f0 = esi4->f0;
            edi5 = reinterpret_cast<struct s50*>(reinterpret_cast<uint32_t>(edi5) + 4);
            esi4 = reinterpret_cast<struct s50*>(reinterpret_cast<uint32_t>(esi4) + 4);
        }
        goto *reinterpret_cast<int32_t*>(edx11 * 4 + 0x405cc8);
    }
    addr_405d00_16:
    return a1;
    edi5->f0 = esi4->f0;
    goto addr_405d00_16;
    edi5->f0 = esi4->f0;
    edi5->f2 = esi4->f2;
    return a1;
}

void fun_405cf9() {
}

void fun_405da6() {
}

void fun_40632c() {
    signed char* eax1;
    signed char* eax2;
    signed char al3;
    signed char* eax4;
    signed char* eax5;
    signed char al6;
    signed char* eax7;
    signed char* eax8;
    signed char al9;
    signed char* eax10;
    signed char* eax11;
    signed char al12;
    signed char* eax13;
    signed char* eax14;
    signed char al15;
    signed char* eax16;
    signed char* eax17;
    signed char al18;
    signed char* eax19;
    signed char* eax20;
    signed char al21;
    signed char* eax22;
    signed char* eax23;
    signed char al24;
    signed char* eax25;
    signed char* eax26;
    signed char al27;
    signed char* eax28;
    signed char* eax29;
    signed char al30;
    signed char* eax31;
    signed char* eax32;
    signed char al33;
    signed char* eax34;
    signed char* eax35;
    signed char al36;
    signed char* eax37;
    signed char* eax38;
    signed char al39;
    signed char* eax40;
    signed char* eax41;
    signed char al42;
    signed char* eax43;
    signed char* eax44;
    signed char al45;
    signed char* eax46;
    signed char* eax47;
    signed char al48;
    signed char* eax49;
    signed char* eax50;
    signed char al51;
    signed char* eax52;
    signed char* eax53;
    signed char al54;
    signed char* eax55;
    signed char* eax56;
    signed char al57;
    signed char* eax58;
    signed char* eax59;
    signed char al60;
    signed char* eax61;
    signed char* eax62;
    signed char al63;
    signed char* eax64;
    signed char* eax65;
    signed char al66;
    signed char* eax67;
    signed char* eax68;
    signed char al69;
    signed char* eax70;
    signed char* eax71;
    signed char al72;
    signed char* eax73;
    signed char* eax74;
    signed char al75;
    signed char* eax76;
    signed char* eax77;
    signed char al78;
    signed char* eax79;
    signed char* eax80;
    signed char al81;
    signed char* eax82;
    signed char* eax83;
    signed char al84;
    signed char* eax85;
    signed char* eax86;
    signed char al87;
    signed char* eax88;
    signed char* eax89;
    signed char al90;
    signed char* eax91;
    signed char* eax92;
    signed char al93;
    signed char* eax94;
    signed char* eax95;
    signed char al96;
    signed char* eax97;
    signed char* eax98;
    signed char al99;
    signed char* eax100;
    signed char* eax101;
    signed char al102;
    signed char* eax103;
    signed char* eax104;
    signed char al105;
    signed char* eax106;
    signed char* eax107;
    signed char al108;
    signed char* eax109;
    signed char* eax110;
    signed char al111;
    signed char* eax112;
    signed char* eax113;
    signed char al114;
    signed char* eax115;
    signed char* eax116;
    signed char al117;
    signed char* eax118;
    signed char* eax119;
    signed char al120;
    signed char* eax121;
    signed char* eax122;
    signed char al123;
    signed char* eax124;
    signed char* eax125;
    signed char al126;
    signed char* eax127;
    signed char* eax128;
    signed char al129;
    signed char* eax130;
    signed char* eax131;
    signed char al132;
    signed char* eax133;
    signed char* eax134;
    signed char al135;
    signed char* eax136;
    signed char* eax137;
    signed char al138;
    signed char* eax139;
    signed char* eax140;
    signed char al141;
    signed char* eax142;
    signed char* eax143;
    signed char al144;
    signed char* eax145;
    signed char* eax146;
    signed char al147;
    signed char* eax148;
    signed char* eax149;
    signed char al150;
    signed char* eax151;
    signed char* eax152;
    signed char al153;
    signed char* eax154;
    signed char* eax155;
    signed char al156;
    signed char* eax157;
    signed char* eax158;
    signed char al159;
    signed char* eax160;
    signed char* eax161;
    signed char al162;
    signed char* eax163;
    signed char* eax164;
    signed char al165;
    signed char* eax166;
    signed char* eax167;
    signed char al168;
    signed char* eax169;
    signed char* eax170;
    signed char al171;
    signed char* eax172;
    signed char* eax173;
    signed char al174;
    signed char* eax175;
    signed char* eax176;
    signed char al177;
    signed char* eax178;
    signed char* eax179;
    signed char al180;
    signed char* eax181;
    signed char* eax182;
    signed char al183;
    signed char* eax184;
    signed char* eax185;
    signed char al186;
    signed char* eax187;
    signed char* eax188;
    signed char al189;
    signed char* eax190;
    signed char* eax191;
    signed char al192;
    signed char* eax193;
    signed char* eax194;
    signed char al195;
    signed char* eax196;
    signed char* eax197;
    signed char al198;
    signed char* eax199;
    signed char* eax200;
    signed char al201;
    signed char* eax202;
    signed char* eax203;
    signed char al204;
    signed char* eax205;
    signed char* eax206;
    signed char al207;
    signed char* eax208;
    signed char* eax209;
    signed char al210;
    signed char* eax211;
    signed char* eax212;
    signed char al213;
    signed char* eax214;
    signed char* eax215;
    signed char al216;
    signed char* eax217;
    signed char* eax218;
    signed char al219;
    signed char* eax220;
    signed char* eax221;
    signed char al222;
    signed char* eax223;
    signed char* eax224;
    signed char al225;
    signed char* eax226;
    signed char* eax227;
    signed char al228;
    signed char* eax229;
    signed char* eax230;
    signed char al231;
    signed char* eax232;
    signed char* eax233;
    signed char al234;
    signed char* eax235;
    signed char* eax236;
    signed char al237;
    signed char* eax238;
    signed char* eax239;
    signed char al240;
    signed char* eax241;
    signed char* eax242;
    signed char al243;
    signed char* eax244;
    signed char* eax245;
    signed char al246;
    signed char* eax247;
    signed char* eax248;
    signed char al249;
    signed char* eax250;
    signed char* eax251;
    signed char al252;
    signed char* eax253;
    signed char* eax254;
    signed char al255;
    signed char* eax256;
    signed char* eax257;
    signed char al258;
    signed char* eax259;
    signed char* eax260;
    signed char al261;
    signed char* eax262;
    signed char* eax263;
    signed char al264;
    signed char* eax265;
    signed char* eax266;
    signed char al267;
    signed char* eax268;
    signed char* eax269;
    signed char al270;
    signed char* eax271;
    signed char* eax272;
    signed char al273;
    signed char* eax274;
    signed char* eax275;
    signed char al276;
    signed char* eax277;
    signed char* eax278;
    signed char al279;
    signed char* eax280;
    signed char* eax281;
    signed char al282;
    signed char* eax283;
    signed char* eax284;
    signed char al285;
    signed char* eax286;
    signed char* eax287;
    signed char al288;
    signed char* eax289;
    signed char* eax290;
    signed char al291;
    signed char* eax292;
    signed char* eax293;
    signed char al294;
    signed char* eax295;
    signed char* eax296;
    signed char al297;
    signed char* eax298;
    signed char* eax299;
    signed char al300;
    signed char* eax301;
    signed char* eax302;
    signed char al303;
    signed char* eax304;
    signed char* eax305;
    signed char al306;
    signed char* eax307;
    signed char* eax308;
    signed char al309;
    signed char* eax310;
    signed char* eax311;
    signed char al312;
    signed char* eax313;
    signed char* eax314;
    signed char al315;
    signed char* eax316;
    signed char* eax317;
    signed char al318;

    *eax1 = reinterpret_cast<signed char>(*eax2 + al3);
    *eax4 = reinterpret_cast<signed char>(*eax5 + al6);
    *eax7 = reinterpret_cast<signed char>(*eax8 + al9);
    *eax10 = reinterpret_cast<signed char>(*eax11 + al12);
    *eax13 = reinterpret_cast<signed char>(*eax14 + al15);
    *eax16 = reinterpret_cast<signed char>(*eax17 + al18);
    *eax19 = reinterpret_cast<signed char>(*eax20 + al21);
    *eax22 = reinterpret_cast<signed char>(*eax23 + al24);
    *eax25 = reinterpret_cast<signed char>(*eax26 + al27);
    *eax28 = reinterpret_cast<signed char>(*eax29 + al30);
    *eax31 = reinterpret_cast<signed char>(*eax32 + al33);
    *eax34 = reinterpret_cast<signed char>(*eax35 + al36);
    *eax37 = reinterpret_cast<signed char>(*eax38 + al39);
    *eax40 = reinterpret_cast<signed char>(*eax41 + al42);
    *eax43 = reinterpret_cast<signed char>(*eax44 + al45);
    *eax46 = reinterpret_cast<signed char>(*eax47 + al48);
    *eax49 = reinterpret_cast<signed char>(*eax50 + al51);
    *eax52 = reinterpret_cast<signed char>(*eax53 + al54);
    *eax55 = reinterpret_cast<signed char>(*eax56 + al57);
    *eax58 = reinterpret_cast<signed char>(*eax59 + al60);
    *eax61 = reinterpret_cast<signed char>(*eax62 + al63);
    *eax64 = reinterpret_cast<signed char>(*eax65 + al66);
    *eax67 = reinterpret_cast<signed char>(*eax68 + al69);
    *eax70 = reinterpret_cast<signed char>(*eax71 + al72);
    *eax73 = reinterpret_cast<signed char>(*eax74 + al75);
    *eax76 = reinterpret_cast<signed char>(*eax77 + al78);
    *eax79 = reinterpret_cast<signed char>(*eax80 + al81);
    *eax82 = reinterpret_cast<signed char>(*eax83 + al84);
    *eax85 = reinterpret_cast<signed char>(*eax86 + al87);
    *eax88 = reinterpret_cast<signed char>(*eax89 + al90);
    *eax91 = reinterpret_cast<signed char>(*eax92 + al93);
    *eax94 = reinterpret_cast<signed char>(*eax95 + al96);
    *eax97 = reinterpret_cast<signed char>(*eax98 + al99);
    *eax100 = reinterpret_cast<signed char>(*eax101 + al102);
    *eax103 = reinterpret_cast<signed char>(*eax104 + al105);
    *eax106 = reinterpret_cast<signed char>(*eax107 + al108);
    *eax109 = reinterpret_cast<signed char>(*eax110 + al111);
    *eax112 = reinterpret_cast<signed char>(*eax113 + al114);
    *eax115 = reinterpret_cast<signed char>(*eax116 + al117);
    *eax118 = reinterpret_cast<signed char>(*eax119 + al120);
    *eax121 = reinterpret_cast<signed char>(*eax122 + al123);
    *eax124 = reinterpret_cast<signed char>(*eax125 + al126);
    *eax127 = reinterpret_cast<signed char>(*eax128 + al129);
    *eax130 = reinterpret_cast<signed char>(*eax131 + al132);
    *eax133 = reinterpret_cast<signed char>(*eax134 + al135);
    *eax136 = reinterpret_cast<signed char>(*eax137 + al138);
    *eax139 = reinterpret_cast<signed char>(*eax140 + al141);
    *eax142 = reinterpret_cast<signed char>(*eax143 + al144);
    *eax145 = reinterpret_cast<signed char>(*eax146 + al147);
    *eax148 = reinterpret_cast<signed char>(*eax149 + al150);
    *eax151 = reinterpret_cast<signed char>(*eax152 + al153);
    *eax154 = reinterpret_cast<signed char>(*eax155 + al156);
    *eax157 = reinterpret_cast<signed char>(*eax158 + al159);
    *eax160 = reinterpret_cast<signed char>(*eax161 + al162);
    *eax163 = reinterpret_cast<signed char>(*eax164 + al165);
    *eax166 = reinterpret_cast<signed char>(*eax167 + al168);
    *eax169 = reinterpret_cast<signed char>(*eax170 + al171);
    *eax172 = reinterpret_cast<signed char>(*eax173 + al174);
    *eax175 = reinterpret_cast<signed char>(*eax176 + al177);
    *eax178 = reinterpret_cast<signed char>(*eax179 + al180);
    *eax181 = reinterpret_cast<signed char>(*eax182 + al183);
    *eax184 = reinterpret_cast<signed char>(*eax185 + al186);
    *eax187 = reinterpret_cast<signed char>(*eax188 + al189);
    *eax190 = reinterpret_cast<signed char>(*eax191 + al192);
    *eax193 = reinterpret_cast<signed char>(*eax194 + al195);
    *eax196 = reinterpret_cast<signed char>(*eax197 + al198);
    *eax199 = reinterpret_cast<signed char>(*eax200 + al201);
    *eax202 = reinterpret_cast<signed char>(*eax203 + al204);
    *eax205 = reinterpret_cast<signed char>(*eax206 + al207);
    *eax208 = reinterpret_cast<signed char>(*eax209 + al210);
    *eax211 = reinterpret_cast<signed char>(*eax212 + al213);
    *eax214 = reinterpret_cast<signed char>(*eax215 + al216);
    *eax217 = reinterpret_cast<signed char>(*eax218 + al219);
    *eax220 = reinterpret_cast<signed char>(*eax221 + al222);
    *eax223 = reinterpret_cast<signed char>(*eax224 + al225);
    *eax226 = reinterpret_cast<signed char>(*eax227 + al228);
    *eax229 = reinterpret_cast<signed char>(*eax230 + al231);
    *eax232 = reinterpret_cast<signed char>(*eax233 + al234);
    *eax235 = reinterpret_cast<signed char>(*eax236 + al237);
    *eax238 = reinterpret_cast<signed char>(*eax239 + al240);
    *eax241 = reinterpret_cast<signed char>(*eax242 + al243);
    *eax244 = reinterpret_cast<signed char>(*eax245 + al246);
    *eax247 = reinterpret_cast<signed char>(*eax248 + al249);
    *eax250 = reinterpret_cast<signed char>(*eax251 + al252);
    *eax253 = reinterpret_cast<signed char>(*eax254 + al255);
    *eax256 = reinterpret_cast<signed char>(*eax257 + al258);
    *eax259 = reinterpret_cast<signed char>(*eax260 + al261);
    *eax262 = reinterpret_cast<signed char>(*eax263 + al264);
    *eax265 = reinterpret_cast<signed char>(*eax266 + al267);
    *eax268 = reinterpret_cast<signed char>(*eax269 + al270);
    *eax271 = reinterpret_cast<signed char>(*eax272 + al273);
    *eax274 = reinterpret_cast<signed char>(*eax275 + al276);
    *eax277 = reinterpret_cast<signed char>(*eax278 + al279);
    *eax280 = reinterpret_cast<signed char>(*eax281 + al282);
    *eax283 = reinterpret_cast<signed char>(*eax284 + al285);
    *eax286 = reinterpret_cast<signed char>(*eax287 + al288);
    *eax289 = reinterpret_cast<signed char>(*eax290 + al291);
    *eax292 = reinterpret_cast<signed char>(*eax293 + al294);
    *eax295 = reinterpret_cast<signed char>(*eax296 + al297);
    *eax298 = reinterpret_cast<signed char>(*eax299 + al300);
    *eax301 = reinterpret_cast<signed char>(*eax302 + al303);
    *eax304 = reinterpret_cast<signed char>(*eax305 + al306);
    *eax307 = reinterpret_cast<signed char>(*eax308 + al309);
    *eax310 = reinterpret_cast<signed char>(*eax311 + al312);
    *eax313 = reinterpret_cast<signed char>(*eax314 + al315);
    *eax316 = reinterpret_cast<signed char>(*eax317 + al318);
}

struct s51 {
    int32_t f0;
    int32_t f4;
};

void fun_4029be() {
    int32_t* esp1;
    int32_t ebp2;
    int32_t ebp3;
    struct s51* esp4;

    esp1 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(*reinterpret_cast<void**>(ebp2 - 24)) - 4);
    *esp1 = *reinterpret_cast<int32_t*>(ebp3 - 32);
    esp4 = reinterpret_cast<struct s51*>(esp1 - 1);
    esp4->f0 = 0x4029ca;
    fun_402490(esp4->f4);
}

struct s52 {
    signed char[24] pad24;
    struct s18* f18;
    int32_t f1c;
};

void fun_404531(struct s52* a1) {
    int32_t v2;
    struct s18* v3;

    v2 = a1->f1c;
    v3 = a1->f18;
    fun_4037de(v3, v2);
    return;
}
