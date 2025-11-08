
int32_t g409550 = 0;

int32_t g409578 = 0;

int32_t g40956c = 0;

int32_t* fun_401010() {
    g409550 = 0x406110;
    g409578 = 0;
    g40956c = -1;
    return 0x409550;
}

struct s1 {
    signed char[8] pad8;
    int32_t f8;
};

struct s0 {
    signed char[8] pad8;
    int32_t f8;
    signed char[16] pad28;
    int32_t f1c;
    signed char[8] pad40;
    struct s1** f28;
};

void fun_4017d0(struct s0* ecx);

struct s0** g40958c = reinterpret_cast<struct s0**>(0);

struct s2 {
    signed char[8] pad8;
    int32_t f8;
    signed char[32] pad44;
    int32_t f2c;
    int32_t f30;
    signed char[56] pad108;
    int32_t f6c;
};

struct s2** g409588 = reinterpret_cast<struct s2**>(0);

struct s3 {
    signed char[8] pad8;
    int32_t f8;
    signed char[12] pad24;
    int32_t f18;
    signed char[52] pad80;
    int32_t f50;
    int32_t f54;
};

struct s3** g409584 = reinterpret_cast<struct s3**>(0);

void fun_401420(void** ecx) {
    struct s0* ecx2;
    struct s0** eax3;
    struct s2** eax4;
    struct s2* edx5;
    struct s3** eax6;
    struct s3* ecx7;

    ecx2 = reinterpret_cast<struct s0*>(0x409550);
    fun_4017d0(0x409550);
    eax3 = g40958c;
    if (eax3) {
        ecx2 = *eax3;
        ecx2->f8(eax3);
    }
    eax4 = g409588;
    if (eax4) {
        edx5 = *eax4;
        edx5->f8(ecx2, eax4);
    }
    eax6 = g409584;
    if (eax6) {
        ecx7 = *eax6;
        ecx7->f8(eax6);
    }
    return;
}

struct s5 {
    signed char[108] pad108;
    int32_t f6c;
};

struct s4 {
    signed char[28] pad28;
    int32_t f1c;
    signed char[8] pad40;
    struct s5** f28;
};

void fun_4017f0(struct s4* ecx);

void fun_401730(struct s4* ecx, struct s0** a2, int32_t a3, int32_t a4, int32_t a5, int32_t a6, int32_t a7, int32_t a8) {
    int32_t v9;
    int32_t ebp10;
    struct s4* edi11;
    struct s0** esi12;
    struct s0* eax13;
    int32_t eax14;

    v9 = ebp10;
    edi11 = ecx;
    if (!a7) {
    }
    if (!a8) {
    }
    esi12 = a2;
    while (1) {
        eax13 = *esi12;
        if (edi11->f1c >= 0) {
        }
        eax14 = reinterpret_cast<int32_t>(eax13->f1c());
        if (!eax14) 
            break;
        if (eax14 != 0x887601c2) {
            if (eax14 != 0x8876021c) 
                goto addr_4017b0_12;
        } else {
            fun_4017f0(edi11);
        }
    }
    goto v9;
    addr_4017b0_12:
    goto v9;
}

void fun_401eb3(void** ecx, void** a2, void** a3, void** a4, void** a5);

void fun_4018b4(void** ecx, void** a2, void** a3) {
    fun_401eb3(ecx, __return_address(), __return_address(), a2, a3);
    return;
}

void fun_4017f0(struct s4* ecx) {
    struct s5** eax2;
    struct s5* ecx3;

    eax2 = ecx->f28;
    ecx3 = *eax2;
    ecx3->f6c();
    goto eax2;
}

struct s6 {
    signed char[16] pad16;
    void** f10;
};

void*** g409988 = reinterpret_cast<void***>(0);

struct s7 {
    struct s7* f0;
    signed char[12] pad16;
    void** f10;
    signed char[3] pad20;
    void** f14;
};

void** fun_4039af(void** ecx, void** a2, struct s7** a3, void*** a4);

struct s9 {
    uint32_t f0;
    signed char[63] pad67;
    signed char f43;
    uint32_t f44;
    signed char[124] pad196;
    uint32_t fc4;
};

struct s8 {
    struct s8* f0;
    signed char f1;
    signed char f2;
    signed char f3;
    uint32_t f4;
    uint32_t f8;
    void* fc;
    struct s9* f10;
};

struct s8* fun_402c54(void** ecx, void** a2, void** a3, void** a4, void** a5);

void** g409984 = reinterpret_cast<void**>(0);

int32_t HeapSize = 0x6880;

struct s6* fun_401da0(void** ecx, void** a2) {
    void* ebp3;
    void*** eax4;
    void** eax5;
    void** v6;
    struct s6* eax7;
    void** esi8;
    struct s8* eax9;
    void** v10;

    ebp3 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    eax4 = g409988;
    if (!reinterpret_cast<int1_t>(eax4 == 3)) {
        if (!reinterpret_cast<int1_t>(eax4 == 2) || (eax5 = fun_4039af(ecx, a2, reinterpret_cast<int32_t>(ebp3) - 8, reinterpret_cast<int32_t>(ebp3) - 4), eax5 == 0)) {
            v6 = a2;
        } else {
            eax7 = reinterpret_cast<struct s6*>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(eax5))) << 4);
            goto addr_401dfe_5;
        }
    } else {
        eax9 = fun_402c54(ecx, a2, esi8, ecx, ecx);
        ecx = a2;
        if (!eax9) {
            v6 = a2;
        } else {
            eax7 = reinterpret_cast<struct s6*>(*reinterpret_cast<void***>(a2 + 0xfffffffc) - 9);
            goto addr_401dfe_5;
        }
    }
    v10 = g409984;
    eax7 = reinterpret_cast<struct s6*>(HeapSize(ecx, v10, 0, v6));
    addr_401dfe_5:
    return eax7;
}

int32_t HeapReAlloc = 0x683e;

void** g40970c = reinterpret_cast<void**>(0);

int32_t fun_403e20(void** ecx);

void** g40922c = reinterpret_cast<void**>(0xe0);

uint32_t fun_403d77(void** ecx, void** a2, void** a3, void** a4, void** a5);

void** fun_403a4b(void** ecx, void** a2);

int32_t HeapAlloc = 0x684c;

void** fun_403e40(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7);

void fun_403a06(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10);

void** g409980 = reinterpret_cast<void**>(0);

int32_t fun_40345d(void** ecx, struct s8* a2, void** a3, void** a4);

void fun_402c7f(void** ecx, struct s8* a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9);

void** fun_402fa8(void** ecx, void** a2);

void** fun_401e01(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13);

void** fun_4019fe(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6) {
    void*** ebp7;
    void** v8;
    void** v9;
    void** ebx10;
    void** v11;
    void** esi12;
    void** v13;
    void** edi14;
    void** esi15;
    void*** eax16;
    void** eax17;
    void** v18;
    int1_t zf19;
    int32_t eax20;
    void** edi21;
    void** eax22;
    void** v23;
    void** eax24;
    int1_t cf25;
    void** edi26;
    uint32_t eax27;
    void** eax28;
    void** v29;
    void** eax30;
    int1_t zf31;
    void** eax32;
    void** eax33;
    int32_t eax34;
    int1_t zf35;
    struct s8* eax36;
    struct s8* ebx37;
    int1_t below_or_equal38;
    int32_t eax39;
    void** v40;
    void** eax41;
    void** v42;
    void** eax43;
    void** eax44;
    void** eax45;
    void** eax46;
    struct s8* eax47;
    int32_t eax48;
    void** ebp49;

    ebp7 = reinterpret_cast<void***>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    v8 = ecx;
    v9 = ebx10;
    v11 = esi12;
    v13 = edi14;
    if (a2) {
        esi15 = a3;
        if (esi15) {
            eax16 = g409988;
            if (!reinterpret_cast<int1_t>(eax16 == 3)) {
                if (!reinterpret_cast<int1_t>(eax16 == 2)) {
                    do {
                        eax17 = reinterpret_cast<void**>(0);
                        if (reinterpret_cast<unsigned char>(esi15) <= reinterpret_cast<unsigned char>(0xffffffe0)) {
                            if (!esi15) {
                                esi15 = reinterpret_cast<void**>(1);
                            }
                            esi15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esi15 + 15) & 0xfffffff0);
                            v18 = g409984;
                            eax17 = reinterpret_cast<void**>(HeapReAlloc(ecx, v18, 0, a2, esi15));
                            if (eax17) 
                                break;
                        }
                        zf19 = g40970c == 0;
                        if (zf19) 
                            break;
                        eax20 = fun_403e20(ecx);
                        ecx = esi15;
                    } while (eax20);
                    goto addr_401c97_11;
                } else {
                    if (reinterpret_cast<unsigned char>(esi15) <= reinterpret_cast<unsigned char>(0xffffffe0)) {
                        if (!esi15) {
                            esi15 = reinterpret_cast<void**>(16);
                        } else {
                            esi15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esi15 + 15) & 0xfffffff0);
                        }
                    }
                    do {
                        edi21 = reinterpret_cast<void**>(0);
                        if (reinterpret_cast<unsigned char>(esi15) > reinterpret_cast<unsigned char>(0xffffffe0)) 
                            goto addr_401c3b_17;
                        eax22 = fun_4039af(ecx, a2, ebp7 - 4, ebp7 + 12);
                        if (!eax22) {
                            v23 = g409984;
                            eax24 = reinterpret_cast<void**>(HeapReAlloc(ecx, v23, 0, a2, esi15));
                            edi21 = eax24;
                        } else {
                            cf25 = reinterpret_cast<unsigned char>(esi15) < reinterpret_cast<unsigned char>(g40922c);
                            if (!cf25) 
                                goto addr_401be3_21;
                            edi26 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi15) >> 4);
                            eax27 = fun_403d77(ecx, v8, a3, eax22, edi26);
                            if (!eax27) 
                                goto addr_401ba9_23; else 
                                goto addr_401ba4_24;
                        }
                        addr_401c33_25:
                        if (edi21) 
                            goto addr_401b37_26; else 
                            goto addr_401c3b_17;
                        addr_401ba9_23:
                        eax28 = fun_403a4b(ecx, edi26);
                        edi21 = eax28;
                        ecx = edi26;
                        if (!edi21) {
                            addr_401be3_21:
                            v29 = g409984;
                            eax30 = reinterpret_cast<void**>(HeapAlloc(ecx, v29, 0, esi15));
                            edi21 = eax30;
                            if (!edi21) {
                                addr_401c3b_17:
                                zf31 = g40970c == 0;
                                if (zf31) 
                                    goto addr_401b37_26; else 
                                    continue;
                            } else {
                                eax32 = reinterpret_cast<void**>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(eax22))) << 4);
                                if (reinterpret_cast<unsigned char>(eax32) >= reinterpret_cast<unsigned char>(esi15)) {
                                    eax32 = esi15;
                                }
                                fun_403e40(ecx, edi21, a2, eax32, v29, 0, esi15);
                                fun_403a06(ecx, v8, a3, eax22, edi21, a2, eax32, v29, 0, esi15);
                                goto addr_401c33_25;
                            }
                        } else {
                            eax33 = reinterpret_cast<void**>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(eax22))) << 4);
                            if (reinterpret_cast<unsigned char>(eax33) >= reinterpret_cast<unsigned char>(esi15)) {
                                eax33 = esi15;
                            }
                            fun_403e40(ecx, edi21, a2, eax33, v13, v11, v9);
                            fun_403a06(ecx, v8, a3, eax22, edi21, a2, eax33, v13, v11, v9);
                        }
                        addr_401bdb_33:
                        if (edi21) 
                            goto addr_401b37_26; else 
                            goto addr_401be3_21;
                        addr_401ba4_24:
                        edi21 = a2;
                        goto addr_401bdb_33;
                        eax34 = fun_403e20(ecx);
                        ecx = esi15;
                    } while (eax34);
                    goto addr_401c57_35;
                }
            } else {
                do {
                    edi21 = reinterpret_cast<void**>(0);
                    if (reinterpret_cast<unsigned char>(esi15) > reinterpret_cast<unsigned char>(0xffffffe0)) {
                        addr_401b1a_37:
                        zf35 = g40970c == 0;
                        if (zf35) 
                            goto addr_401b37_26; else 
                            continue;
                    } else {
                        eax36 = fun_402c54(ecx, a2, v13, v11, v9);
                        ebx37 = eax36;
                        ecx = a2;
                        if (!ebx37) 
                            goto addr_401af5_39;
                        below_or_equal38 = reinterpret_cast<unsigned char>(esi15) <= reinterpret_cast<unsigned char>(g409980);
                        if (!below_or_equal38) 
                            goto addr_401aae_41;
                        edi21 = a2;
                        eax39 = fun_40345d(ecx, ebx37, edi21, esi15);
                        if (!eax39) 
                            goto addr_401a74_43;
                    }
                    addr_401aaa_44:
                    if (edi21) {
                        addr_401af1_45:
                        if (ebx37) {
                            addr_401b16_46:
                            if (edi21) 
                                goto addr_401b37_26; else 
                                goto addr_401b1a_37;
                        } else {
                            addr_401af5_39:
                            if (!esi15) {
                                esi15 = reinterpret_cast<void**>(1);
                            }
                        }
                    } else {
                        addr_401aae_41:
                        if (!esi15) {
                            esi15 = reinterpret_cast<void**>(1);
                            goto addr_401ab5_49;
                        }
                    }
                    esi15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esi15 + 15) & 0xfffffff0);
                    v40 = g409984;
                    eax41 = reinterpret_cast<void**>(HeapReAlloc(ecx, v40, 0, a2, esi15));
                    edi21 = eax41;
                    goto addr_401b16_46;
                    addr_401ab5_49:
                    esi15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esi15 + 15) & 0xfffffff0);
                    v42 = g409984;
                    eax43 = reinterpret_cast<void**>(HeapAlloc(ecx, v42, 0, esi15));
                    edi21 = eax43;
                    if (edi21) {
                        ecx = a2;
                        eax44 = *reinterpret_cast<void***>(ecx + 0xfffffffc) - 1;
                        if (reinterpret_cast<unsigned char>(eax44) >= reinterpret_cast<unsigned char>(esi15)) {
                            eax44 = esi15;
                        }
                        fun_403e40(ecx, edi21, ecx, eax44, v42, 0, esi15);
                        fun_402c7f(ecx, ebx37, a2, edi21, ecx, eax44, v42, 0, esi15);
                        goto addr_401af1_45;
                    }
                    addr_401a74_43:
                    eax45 = fun_402fa8(ecx, esi15);
                    edi21 = eax45;
                    ecx = esi15;
                    if (!edi21) 
                        goto addr_401aae_41;
                    eax46 = *reinterpret_cast<void***>(a2 + 0xfffffffc) - 1;
                    if (reinterpret_cast<unsigned char>(eax46) >= reinterpret_cast<unsigned char>(esi15)) {
                        eax46 = esi15;
                    }
                    fun_403e40(ecx, edi21, a2, eax46, v13, v11, v9);
                    eax47 = fun_402c54(ecx, a2, edi21, a2, eax46);
                    ebx37 = eax47;
                    fun_402c7f(ecx, ebx37, a2, a2, edi21, a2, eax46, v13, v11);
                    goto addr_401aaa_44;
                    eax48 = fun_403e20(ecx);
                    ecx = esi15;
                } while (eax48);
                goto addr_401b32_58;
            }
        } else {
            fun_401eb3(ecx, a2, v13, v11, v9);
            goto addr_401c97_11;
        }
    } else {
        eax17 = fun_401e01(ecx, a3, v13, v11, v9, v8, ebp49, __return_address(), a2, a3, a4, a5, a6);
    }
    addr_401c99_61:
    return eax17;
    addr_401c97_11:
    eax17 = reinterpret_cast<void**>(0);
    goto addr_401c99_61;
    addr_401b37_26:
    eax17 = edi21;
    goto addr_401c99_61;
    addr_401c57_35:
    goto addr_401c97_11;
    addr_401b32_58:
    goto addr_401c97_11;
}

int32_t g4095a4 = 0;

void fun_402a80(void** ecx);

void fun_402ab9(void** ecx, int32_t a2);

int32_t g4070c4 = 0x401cdc;

void fun_4019b5(void** ecx) {
    int1_t zf2;

    zf2 = g4095a4 == 1;
    if (zf2) {
        fun_402a80(ecx);
    }
    fun_402ab9(ecx, __return_address());
    g4070c4();
    return;
}

int32_t ExitProcess = 0x6830;

void fun_4019da(void** ecx) {
    int1_t zf2;

    zf2 = g4095a4 == 1;
    if (zf2) {
        fun_402a80(ecx);
    }
    fun_402ab9(ecx, __return_address());
    ExitProcess(__return_address());
    goto 0xff;
}

void** g4099a0 = reinterpret_cast<void**>(0);

uint32_t g409aa0 = 0;

int32_t GetStartupInfoA = 0x67fe;

struct s10 {
    uint32_t f0;
    unsigned char f4;
};

struct s11 {
    void** f0;
    signed char[3] pad4;
    unsigned char f4;
};

int32_t GetStdHandle = 0x6958;

int32_t GetFileType = 0x6968;

int32_t SetHandleCount = 0x6946;

struct s12 {
    void** f0;
    signed char[3] pad4;
    unsigned char f4;
};

void fun_402530(void** ecx, void** a2, void** a3, void** a4) {
    void** v5;
    void** ebx6;
    void** v7;
    void** ebp8;
    void** v9;
    void** esi10;
    void** v11;
    void** edi12;
    void** v13;
    void** v14;
    void** v15;
    void** v16;
    void** v17;
    void** v18;
    void** v19;
    void** eax20;
    void** esi21;
    void** ecx22;
    void* esp23;
    void** eax24;
    void** eax25;
    void** v26;
    int16_t v27;
    struct s10* v28;
    int32_t ebx29;
    uint32_t esi30;
    unsigned char* ebp31;
    void*** ebx32;
    int1_t less33;
    void** eax34;
    struct s11* esi35;
    int32_t eax36;
    uint32_t eax37;
    uint32_t eax38;
    void** eax39;
    uint32_t eax40;
    uint32_t eax41;
    int32_t v42;
    uint32_t edi43;
    void** v44;
    int32_t eax45;
    struct s12* eax46;
    void*** edi47;
    void** v48;
    void** v49;
    void** v50;
    void** v51;
    void** v52;
    void** v53;
    void** eax54;
    uint32_t tmp32_55;
    int1_t less56;

    v5 = ebx6;
    v7 = ebp8;
    v9 = esi10;
    v11 = edi12;
    eax20 = fun_401e01(ecx, 0x100, v11, v9, v7, v5, v13, v14, v15, v16, v17, v18, v19);
    esi21 = eax20;
    ecx22 = reinterpret_cast<void**>(0x100);
    esp23 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 68 - 4 - 4 - 4 - 4 - 4 - 4 + 4 + 4);
    if (!esi21) {
        fun_4019b5(0x100);
        ecx22 = reinterpret_cast<void**>(27);
        esp23 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp23) - 4 - 4 + 4 + 4);
    }
    g4099a0 = esi21;
    g409aa0 = 32;
    eax24 = esi21 + 0x100;
    while (reinterpret_cast<unsigned char>(esi21) < reinterpret_cast<unsigned char>(eax24)) {
        *reinterpret_cast<void***>(esi21 + 4) = reinterpret_cast<void**>(0);
        *reinterpret_cast<void***>(esi21) = reinterpret_cast<void**>(0xffffffff);
        *reinterpret_cast<signed char*>(esi21 + 5) = 10;
        eax25 = g4099a0;
        esi21 = esi21 + 8;
        eax24 = eax25 + 0x100;
    }
    v26 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp23) + 16);
    GetStartupInfoA(ecx22);
    if (!v27 || !v28) {
        addr_402660_7:
        ebx29 = 0;
    } else {
        esi30 = v28->f0;
        ebp31 = &v28->f4;
        ebx32 = reinterpret_cast<void***>(esi30 + reinterpret_cast<uint32_t>(ebp31));
        if (reinterpret_cast<int32_t>(esi30) >= reinterpret_cast<int32_t>(0x800)) {
            esi30 = 0x800;
        }
        less33 = reinterpret_cast<int32_t>(g409aa0) < reinterpret_cast<int32_t>(esi30);
        if (!less33) 
            goto addr_402614_11; else 
            goto addr_4025c2_12;
    }
    do {
        eax34 = g4099a0;
        esi35 = reinterpret_cast<struct s11*>(eax34 + ebx29 * 8);
        if (!reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax34 + ebx29 * 8) == 0xffffffff)) {
            esi35->f4 = reinterpret_cast<unsigned char>(esi35->f4 | 0x80);
        } else {
            esi35->f4 = 0x81;
            if (ebx29) {
                eax36 = ebx29 - 1;
                eax37 = reinterpret_cast<uint32_t>(-eax36);
                eax38 = eax37 - (eax37 + reinterpret_cast<uint1_t>(eax37 < eax37 + reinterpret_cast<uint1_t>(!!eax36))) - 11;
            } else {
                eax38 = 0xf6;
            }
            eax39 = reinterpret_cast<void**>(GetStdHandle(ecx22, eax38));
            if (eax39 == 0xffffffff || ((eax40 = reinterpret_cast<uint32_t>(GetFileType(ecx22, eax39, eax38)), eax40 == 0) || (eax41 = eax40 & 0xff, esi35->f0 = eax39, eax41 == 2))) {
                esi35->f4 = reinterpret_cast<unsigned char>(esi35->f4 | 64);
            } else {
                if (eax41 == 3) {
                    esi35->f4 = reinterpret_cast<unsigned char>(esi35->f4 | 8);
                }
            }
        }
        ++ebx29;
    } while (ebx29 < 3);
    SetHandleCount(ecx22);
    goto v42;
    addr_402614_11:
    edi43 = 0;
    if (!(reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(esi30) < reinterpret_cast<int32_t>(0)) | reinterpret_cast<uint1_t>(esi30 == 0))) {
        do {
            if (*ebx32 != 0xffffffff && ((*reinterpret_cast<unsigned char*>(&ecx22) = *ebp31, !!(*reinterpret_cast<unsigned char*>(&ecx22) & 1)) && (*reinterpret_cast<unsigned char*>(&ecx22) & 8 || (v44 = *ebx32, eax45 = reinterpret_cast<int32_t>(GetFileType(v44)), !!eax45)))) {
                eax46 = reinterpret_cast<struct s12*>(*reinterpret_cast<void****>((reinterpret_cast<int32_t>(edi43) >> 5) * 4 + 0x4099a0) + (edi43 & 31) * 8);
                ecx22 = *ebx32;
                eax46->f0 = ecx22;
                *reinterpret_cast<unsigned char*>(&ecx22) = *ebp31;
                eax46->f4 = *reinterpret_cast<unsigned char*>(&ecx22);
            }
            ++edi43;
            ++ebp31;
            ebx32 = ebx32 + 4;
        } while (reinterpret_cast<int32_t>(edi43) < reinterpret_cast<int32_t>(esi30));
        goto addr_402660_7;
    }
    addr_4025c2_12:
    edi47 = reinterpret_cast<void***>(0x4099a4);
    do {
        eax54 = fun_401e01(ecx22, 0x100, v26, v11, v9, v7, v5, v48, v49, v50, v51, v52, v53);
        ecx22 = reinterpret_cast<void**>(0x100);
        if (!eax54) 
            break;
        tmp32_55 = g409aa0 + 32;
        g409aa0 = tmp32_55;
        *edi47 = eax54;
        ecx22 = eax54 + 0x100;
        while (reinterpret_cast<unsigned char>(eax54) < reinterpret_cast<unsigned char>(ecx22)) {
            *reinterpret_cast<void***>(eax54 + 4) = reinterpret_cast<void**>(0);
            *reinterpret_cast<void***>(eax54) = reinterpret_cast<void**>(0xffffffff);
            *reinterpret_cast<signed char*>(eax54 + 5) = 10;
            eax54 = eax54 + 8;
            ecx22 = *edi47 + 0x100;
        }
        edi47 = edi47 + 4;
        less56 = reinterpret_cast<int32_t>(g409aa0) < reinterpret_cast<int32_t>(esi30);
    } while (less56);
    goto addr_40260c_32;
    esi30 = g409aa0;
    goto addr_402614_11;
    addr_40260c_32:
    goto addr_402614_11;
}

void** g4096fc = reinterpret_cast<void**>(0);

int32_t GetEnvironmentStringsW = 0x692c;

int32_t GetEnvironmentStrings = 0x6914;

int32_t FreeEnvironmentStringsA = 0x68ca;

int32_t WideCharToMultiByte = 0x68fe;

int32_t FreeEnvironmentStringsW = 0x68e4;

void** fun_4023fe(void** ecx, void** a2, void** a3, void** a4, void** a5) {
    void** v6;
    void** v7;
    void** v8;
    void** eax9;
    void** v10;
    void** ebx11;
    void** v12;
    void** ebp13;
    int32_t ebp14;
    void** v15;
    void** esi16;
    void** v17;
    void** edi18;
    void** ebx19;
    void** esi20;
    void** edi21;
    void** eax22;
    void** eax23;
    void** eax24;
    void** eax25;
    void** eax26;
    void** ebp27;
    void** eax28;
    void** esi29;
    void** eax30;
    void** eax31;
    int32_t edi32;
    void** v33;
    void** v34;
    void** eax35;
    void** eax36;
    void** v37;
    int32_t eax38;

    v6 = reinterpret_cast<void**>(__return_address());
    v7 = ecx;
    v8 = ecx;
    eax9 = g4096fc;
    v10 = ebx11;
    v12 = ebp13;
    ebp14 = GetEnvironmentStringsW;
    v15 = esi16;
    v17 = edi18;
    ebx19 = reinterpret_cast<void**>(0);
    esi20 = reinterpret_cast<void**>(0);
    edi21 = reinterpret_cast<void**>(0);
    if (eax9) {
        if (!reinterpret_cast<int1_t>(eax9 == 1)) {
            if (!reinterpret_cast<int1_t>(eax9 == 2)) 
                goto addr_402527_4;
        } else {
            addr_402455_5:
            if (esi20) 
                goto addr_402465_6;
            eax22 = reinterpret_cast<void**>(ebp14());
            esi20 = eax22;
            if (!esi20) 
                goto addr_402527_4; else 
                goto addr_402465_6;
        }
    } else {
        eax23 = reinterpret_cast<void**>(ebp14());
        esi20 = eax23;
        if (!esi20) {
            eax24 = reinterpret_cast<void**>(GetEnvironmentStrings());
            edi21 = eax24;
            if (!edi21) 
                goto addr_402527_4;
            g4096fc = reinterpret_cast<void**>(2);
        } else {
            g4096fc = reinterpret_cast<void**>(1);
            goto addr_402455_5;
        }
    }
    if (edi21 || (eax25 = reinterpret_cast<void**>(GetEnvironmentStrings()), edi21 = eax25, !!edi21)) {
        eax26 = edi21;
        if (*reinterpret_cast<void***>(edi21)) {
            addr_4024f1_14:
            ++eax26;
            if (*reinterpret_cast<void***>(eax26)) 
                goto addr_4024f1_14;
            ++eax26;
            if (*reinterpret_cast<void***>(eax26)) 
                goto addr_4024f1_14;
        }
        ebp27 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(eax26) - reinterpret_cast<unsigned char>(edi21) + 1);
        eax28 = fun_401e01(ecx, ebp27, v17, v15, v12, v10, v8, v7, v6, a2, a3, a4, a5);
        esi29 = eax28;
        if (esi29) {
            fun_403e40(ebp27, esi29, edi21, ebp27, v17, v15, v12);
        } else {
            esi29 = reinterpret_cast<void**>(0);
        }
        FreeEnvironmentStringsA(ebp27, edi21);
        eax30 = esi29;
    } else {
        addr_402527_4:
        eax30 = reinterpret_cast<void**>(0);
    }
    addr_402529_20:
    return eax30;
    addr_402465_6:
    eax31 = esi20;
    if (*reinterpret_cast<void***>(esi20)) {
        addr_40246c_21:
        eax31 = eax31 + 1 + 1;
        if (*reinterpret_cast<void***>(eax31)) 
            goto addr_40246c_21;
        eax31 = eax31 + 1 + 1;
        if (*reinterpret_cast<void***>(eax31)) 
            goto addr_40246c_21;
    }
    edi32 = WideCharToMultiByte;
    v33 = reinterpret_cast<void**>(0);
    v34 = reinterpret_cast<void**>((reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(eax31) - reinterpret_cast<unsigned char>(esi20)) >> 1) + 1);
    eax35 = reinterpret_cast<void**>(edi32(0, 0, esi20, v34, 0, 0, 0, 0));
    if (eax35 && (eax36 = fun_401e01(ecx, eax35, 0, 0, esi20, v34, 0, 0, 0, 0, v17, v15, v12), ecx = eax35, v33 = eax36, !!eax36)) {
        v37 = eax36;
        eax38 = reinterpret_cast<int32_t>(edi32(ecx, 0, 0, esi20));
        if (!eax38) {
            fun_401eb3(ecx, v33, 0, 0, esi20);
            ecx = v33;
            v37 = reinterpret_cast<void**>(0);
        }
        ebx19 = v37;
    }
    FreeEnvironmentStringsW(ecx, esi20, 0, 0, esi20, v34, v33, 0, 0, 0);
    eax30 = ebx19;
    goto addr_402529_20;
}

void** g409aa8 = reinterpret_cast<void**>(0);

uint32_t fun_40457b(void** a1, void** a2, void** a3);

int32_t GetModuleFileNameA = 0x68b4;

void** g409ab8 = reinterpret_cast<void**>(0);

void** g4095e0 = reinterpret_cast<void**>(0);

void fun_40224a(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6);

void** g4095c8 = reinterpret_cast<void**>(0);

void** g4095c4 = reinterpret_cast<void**>(0);

void fun_4021b1(void** ecx, void** a2, void** a3, void** a4) {
    void* ebp5;
    int1_t zf6;
    void** edi7;
    void** esi8;
    void** ebx9;
    void** eax10;
    void** edi11;
    void** v12;
    void** v13;
    void** ecx14;
    void** eax15;

    ebp5 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    zf6 = g409aa8 == 0;
    if (zf6) {
        fun_40457b(edi7, esi8, ebx9);
    }
    GetModuleFileNameA();
    eax10 = g409ab8;
    g4095e0 = reinterpret_cast<void**>(0x4095f8);
    edi11 = reinterpret_cast<void**>(0x4095f8);
    if (*reinterpret_cast<void***>(eax10)) {
        edi11 = eax10;
    }
    v12 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp5) + 0xfffffff8);
    v13 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp5) + 0xfffffffc);
    fun_40224a(ecx, edi11, 0, 0, v13, v12);
    ecx14 = ecx;
    eax15 = fun_401e01(ecx14, ecx + reinterpret_cast<unsigned char>(ecx14) * 4, edi11, 0, 0, v13, v12, 0, 0x4095f8, 0x104, edi7, esi8, ebx9);
    if (!eax15) {
        fun_4019b5(ecx14);
        ecx14 = reinterpret_cast<void**>(8);
    }
    fun_40224a(ecx14, edi11, eax15, eax15 + reinterpret_cast<unsigned char>(ecx) * 4, reinterpret_cast<int32_t>(ebp5) + 0xfffffffc, reinterpret_cast<int32_t>(ebp5) + 0xfffffff8);
    g4095c8 = eax15;
    g4095c4 = ecx - 1;
    return;
}

void** g40959c = reinterpret_cast<void**>(0);

struct s13 {
    signed char[1] pad1;
    void** f1;
};

struct s13* fun_404690(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6);

void** g4095d0 = reinterpret_cast<void**>(0);

void** fun_4045a0(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7);

int32_t g409aa4 = 0;

void fun_4020f8(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8) {
    void** v9;
    void** v10;
    void** ebx11;
    int1_t zf12;
    void** v13;
    void** esi14;
    void** v15;
    void** edi16;
    void** esi17;
    int32_t edi18;
    struct s13* eax19;
    void** v20;
    void** eax21;
    void** esi22;
    void** ecx23;
    void** edi24;
    void** v25;
    void** ebp26;
    struct s13* eax27;
    void** ebp28;
    void** eax29;
    void** ecx30;
    void** v31;
    void** v32;

    v9 = reinterpret_cast<void**>(__return_address());
    v10 = ebx11;
    zf12 = g409aa8 == 0;
    v13 = esi14;
    v15 = edi16;
    if (zf12) {
        fun_40457b(v15, v13, v10);
    }
    esi17 = g40959c;
    edi18 = 0;
    while (*reinterpret_cast<void***>(esi17)) {
        if (*reinterpret_cast<void***>(esi17) != 61) {
            ++edi18;
        }
        eax19 = fun_404690(ecx, esi17, v15, v13, v10, v9);
        ecx = esi17;
        esi17 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi17) + reinterpret_cast<uint32_t>(eax19) + 1);
    }
    v20 = reinterpret_cast<void**>(edi18 * 4 + 4);
    eax21 = fun_401e01(ecx, v20, v15, v13, v10, v9, a2, a3, a4, a5, a6, a7, a8);
    esi22 = eax21;
    ecx23 = v20;
    g4095d0 = esi22;
    if (!esi22) {
        fun_4019b5(ecx23);
        ecx23 = reinterpret_cast<void**>(9);
    }
    edi24 = g40959c;
    if (*reinterpret_cast<void***>(edi24)) {
        v25 = ebp26;
        do {
            eax27 = fun_404690(ecx23, edi24, v25, v15, v13, v10);
            ecx23 = edi24;
            ebp28 = reinterpret_cast<void**>(&eax27->f1);
            if (*reinterpret_cast<void***>(edi24) != 61) {
                eax29 = fun_401e01(ecx23, ebp28, v25, v15, v13, v10, v9, a2, a3, a4, a5, a6, a7);
                ecx30 = ebp28;
                *reinterpret_cast<void***>(esi22) = eax29;
                if (!eax29) {
                    fun_4019b5(ecx30);
                    ecx30 = reinterpret_cast<void**>(9);
                }
                v31 = *reinterpret_cast<void***>(esi22);
                fun_4045a0(ecx30, v31, edi24, v25, v15, v13, v10);
                esi22 = esi22 + 4;
                ecx23 = edi24;
            }
            edi24 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(edi24) + reinterpret_cast<unsigned char>(ebp28));
        } while (*reinterpret_cast<void***>(edi24));
    }
    v32 = g40959c;
    fun_401eb3(ecx23, v32, v15, v13, v10);
    g40959c = reinterpret_cast<void**>(0);
    *reinterpret_cast<void***>(esi22) = reinterpret_cast<void**>(0);
    g409aa4 = 1;
    return;
}

int32_t g409ab4 = 0;

void fun_401d86(void** ecx, void** a2, void** a3, void** a4, void** a5);

void fun_401c9e(void** ecx, void** a2, void** a3, void** a4) {
    int32_t eax5;

    eax5 = g409ab4;
    if (eax5) {
        eax5();
    }
    fun_401d86(ecx, 0x40700c, 0x407018, __return_address(), a2);
    fun_401d86(ecx, 0x407000, 0x407008, 0x40700c, 0x407018);
    return;
}

uint32_t fun_404175(void** ecx);

void** fun_4020a0(void** ecx, void** a2, void** a3, void** a4, void** a5) {
    int1_t zf6;
    void** esi7;
    void** v8;
    uint32_t eax9;

    zf6 = g409aa8 == 0;
    if (zf6) {
        fun_40457b(__return_address(), a2, a3);
    }
    esi7 = g409ab8;
    if (!reinterpret_cast<int1_t>(*reinterpret_cast<void***>(esi7) == 34)) {
        if (reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi7)) > 32) {
            do {
                ++esi7;
            } while (reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi7)) > 32);
        }
    } else {
        while ((++esi7, *reinterpret_cast<void***>(esi7 + 1) != 34) && *reinterpret_cast<void***>(esi7 + 1)) {
            v8 = reinterpret_cast<void**>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi7 + 1))));
            eax9 = fun_404175(ecx);
            ecx = v8;
            if (!eax9) 
                continue;
            ++esi7;
        }
        if (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(esi7) == 34)) 
            goto addr_4020dd_10;
    }
    while (*reinterpret_cast<void***>(esi7) && reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi7)) <= 32) {
        addr_4020dd_10:
        ++esi7;
    }
    return esi7;
}

int32_t g4070c8 = 2;

int32_t g409700 = 0;

void fun_402a80(void** ecx) {
    int32_t eax2;
    int1_t zf3;
    int32_t eax4;

    eax2 = g4095a4;
    if (eax2 == 1 || !eax2 && (zf3 = g4070c8 == 1, zf3)) {
        fun_402ab9(ecx, 0xfc);
        eax4 = g409700;
        if (eax4) {
            eax4(0xfc);
        }
        fun_402ab9(0xfc, 0xff);
    }
    return;
}

int32_t WriteFile = 0x69d6;

void** fun_404b80(void** ecx, void** a2, void*** a3, uint32_t a4, void** a5);

void** fun_4045b0(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13);

void fun_404aef(void** ecx, void* a2, int32_t a3, int32_t a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12);

void fun_402ab9(void** ecx, int32_t a2) {
    void* ebp3;
    int32_t edx4;
    void** ecx5;
    int32_t* eax6;
    uint32_t esi7;
    int32_t eax8;
    int1_t zf9;
    void*** esi10;
    void** v11;
    void** v12;
    void** esi13;
    void** v14;
    struct s13* eax15;
    void** v16;
    int32_t eax17;
    void** v18;
    int32_t eax19;
    void** v20;
    void** edi21;
    void** edi22;
    struct s13* eax23;
    void** v24;
    struct s13* eax25;
    void** v26;
    void** v27;
    void** v28;
    void** v29;
    void** v30;
    void** v31;
    void** v32;
    void** v33;
    void** v34;

    ebp3 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    edx4 = a2;
    ecx5 = reinterpret_cast<void**>(0);
    eax6 = reinterpret_cast<int32_t*>(0x407178);
    do {
        if (edx4 == *eax6) 
            break;
        eax6 = eax6 + 2;
        ++ecx5;
    } while (reinterpret_cast<int32_t>(eax6) < 0x407208);
    esi7 = reinterpret_cast<unsigned char>(ecx5) << 3;
    if (edx4 == *reinterpret_cast<int32_t*>(esi7 + 0x407178)) {
        eax8 = g4095a4;
        if (eax8 == 1 || !eax8 && (zf9 = g4070c8 == 1, zf9)) {
            esi10 = reinterpret_cast<void***>(esi7 + 0x40717c);
            v11 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 8);
            v12 = *esi10;
            eax15 = fun_404690(ecx5, v12, v11, 0, esi13, v14);
            v16 = *esi10;
            eax17 = reinterpret_cast<int32_t>(GetStdHandle(v12, 0xf4, v16, eax15, v11, 0));
            WriteFile(v12, eax17, 0xf4, v16, eax15, v11, 0);
        } else {
            if (edx4 != 0xfc) {
                v18 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xfffffe5c);
                eax19 = reinterpret_cast<int32_t>(GetModuleFileNameA(0, v18, 0x104));
                if (!eax19) {
                    fun_4045a0(ecx5, reinterpret_cast<int32_t>(ebp3) + 0xfffffe5c, "<program name unknown>", 0, v18, 0x104, esi13);
                    ecx5 = reinterpret_cast<void**>("<program name unknown>");
                }
                v20 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xfffffe5c);
                edi21 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xfffffe5c);
                eax23 = fun_404690(ecx5, v20, edi22, 0, v18, 0x104);
                if (reinterpret_cast<unsigned char>(&eax23->f1) > reinterpret_cast<unsigned char>(60)) {
                    v24 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xfffffe5c);
                    eax25 = fun_404690(v20, v24, edi22, 0, v18, 0x104);
                    edi21 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(eax25) + (reinterpret_cast<int32_t>(ebp3) - 0x1a4 - 59));
                    fun_404b80(v20, edi21, "...", 3, v24);
                }
                v26 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xffffff60);
                fun_4045a0(v20, v26, "Runtime Error!\n\nProgram: ", edi22, 0, v18, 0x104);
                v27 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xffffff60);
                fun_4045b0(v20, v27, edi21, v26, "Runtime Error!\n\nProgram: ", edi22, 0, v18, 0x104, esi13, v28, v29, v30);
                v31 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xffffff60);
                fun_4045b0(v20, v31, "\n\n", v27, edi21, v26, "Runtime Error!\n\nProgram: ", edi22, 0, v18, 0x104, esi13, v32);
                v33 = *reinterpret_cast<void***>(esi7 + 0x40717c);
                v34 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp3) + 0xffffff60);
                fun_4045b0(v20, v34, v33, v31, "\n\n", v27, edi21, v26, "Runtime Error!\n\nProgram: ", edi22, 0, v18, 0x104);
                fun_404aef(v20, reinterpret_cast<int32_t>(ebp3) - 0xa0, "Microsoft Visual C++ Runtime Library", 0x12010, v34, v33, v31, "\n\n", v27, edi21, v26, "Runtime Error!\n\nProgram: ");
            }
        }
    }
    return;
}

void** fun_401e13(uint32_t a1, void** a2);

void** fun_401e01(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13) {
    void** v14;
    void** eax15;

    v14 = g40970c;
    eax15 = fun_401e13(__return_address(), v14);
    return eax15;
}

int32_t HeapFree = 0x688c;

void fun_401eb3(void** ecx, void** a2, void** a3, void** a4, void** a5) {
    void*** ebp6;
    void** v7;
    void*** eax8;
    void** eax9;
    void** v10;
    void** esi11;
    void** ebp12;
    struct s8* eax13;
    void** v14;

    ebp6 = reinterpret_cast<void***>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    v7 = ecx;
    if (a2) {
        eax8 = g409988;
        if (!reinterpret_cast<int1_t>(eax8 == 3)) {
            if (!reinterpret_cast<int1_t>(eax8 == 2) || (eax9 = fun_4039af(ecx, a2, ebp6 - 4, ebp6 + 8), eax9 == 0)) {
                v10 = a2;
                goto addr_401f0b_5;
            } else {
                fun_403a06(ecx, v7, a2, eax9, esi11, v7, ebp12, __return_address(), a2, a3);
            }
        } else {
            eax13 = fun_402c54(ecx, a2, esi11, v7, ebp12);
            ecx = a2;
            v10 = a2;
            if (!eax13) {
                addr_401f0b_5:
                v14 = g409984;
                HeapFree(ecx, v14, 0, v10);
            } else {
                fun_402c7f(ecx, eax13, v10, esi11, v7, ebp12, __return_address(), a2, a3);
            }
        }
    }
    return;
}

uint32_t g409978 = 0;

struct s8* g40997c = reinterpret_cast<struct s8*>(0);

struct s8* fun_402c54(void** ecx, void** a2, void** a3, void** a4, void** a5) {
    uint32_t eax6;
    struct s8* eax7;
    struct s8* ecx8;

    eax6 = g409978;
    eax7 = g40997c;
    ecx8 = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(eax7) + (eax6 + eax6 * 4) * 4);
    while (reinterpret_cast<unsigned char>(eax7) < reinterpret_cast<unsigned char>(ecx8)) {
        if (reinterpret_cast<unsigned char>(a2) - reinterpret_cast<uint32_t>(eax7->fc) < 0x100000) 
            goto addr_402c7e_4;
        ++eax7;
    }
    eax7 = reinterpret_cast<struct s8*>(0);
    addr_402c7e_4:
    return eax7;
}

struct s15 {
    signed char[4] pad4;
    struct s15* f4;
    struct s15* f8;
};

struct s14 {
    signed char[4] pad4;
    struct s15* f4;
    struct s15* f8;
};

struct s17 {
    signed char[4] pad4;
    struct s16* f4;
    struct s16* f8;
};

struct s16 {
    void* f0;
    struct s17* f4;
    struct s17* f8;
};

struct s18 {
    signed char[4] pad4;
    struct s18* f4;
    struct s18* f8;
};

int32_t fun_40345d(void** ecx, struct s8* a2, void** a3, void** a4) {
    struct s9* eax5;
    void** esi6;
    uint32_t edx7;
    void* v8;
    void** ecx9;
    void* ebx10;
    struct s14* edi11;
    void* v12;
    void* v13;
    void** ecx14;
    struct s16* ebx15;
    void* esi16;
    void* esi17;
    uint32_t ecx18;
    signed char* ecx19;
    uint32_t ebx20;
    void* ecx21;
    signed char* esi22;
    uint32_t ebx23;
    void* esi24;
    struct s17* ecx25;
    signed char cl26;
    uint32_t ecx27;
    uint32_t* eax28;
    void* ecx29;
    void* ecx30;
    int32_t eax31;
    void* ecx32;
    void* v33;
    uint32_t ecx34;
    signed char* ecx35;
    uint32_t ebx36;
    signed char* ecx37;
    uint32_t ebx38;
    void* v39;
    void** edx40;
    void* edi41;
    struct s18* ecx42;
    struct s18* ebx43;
    signed char cl44;
    uint32_t ecx45;
    uint32_t* eax46;
    void* ecx47;
    void* ecx48;
    void** eax49;
    void** eax50;

    eax5 = a2->f10;
    esi6 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(a4 + 23) & 0xfffffff0);
    edx7 = reinterpret_cast<unsigned char>(a3) - reinterpret_cast<uint32_t>(a2->fc) >> 15;
    v8 = reinterpret_cast<void*>(edx7 * 0x204 + reinterpret_cast<uint32_t>(eax5) + 0x144);
    ecx9 = *reinterpret_cast<void***>(a3 + 0xfffffffc) - 1;
    ebx10 = *reinterpret_cast<void**>(reinterpret_cast<unsigned char>(ecx9) + reinterpret_cast<unsigned char>(a3) + 0xfffffffc);
    edi11 = reinterpret_cast<struct s14*>(reinterpret_cast<unsigned char>(ecx9) + reinterpret_cast<unsigned char>(a3) + 0xfffffffc);
    v12 = ebx10;
    if (reinterpret_cast<signed char>(esi6) <= reinterpret_cast<signed char>(ecx9)) {
        if (reinterpret_cast<signed char>(esi6) < reinterpret_cast<signed char>(ecx9)) {
            v13 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(ecx9) - reinterpret_cast<unsigned char>(esi6));
            ecx14 = esi6 + 1;
            *reinterpret_cast<void***>(a3 + 0xfffffffc) = ecx14;
            ebx15 = reinterpret_cast<struct s16*>(reinterpret_cast<unsigned char>(a3) + reinterpret_cast<unsigned char>(esi6) + 0xfffffffc);
            esi16 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(v13) >> 4) - 1);
            *reinterpret_cast<void***>(reinterpret_cast<uint32_t>(ebx15) - 4) = ecx14;
            if (reinterpret_cast<uint32_t>(esi16) > 63) {
                esi16 = reinterpret_cast<void*>(63);
            }
            if (!(*reinterpret_cast<unsigned char*>(&v12) & 1)) {
                esi17 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(v12) >> 4) - 1);
                if (reinterpret_cast<uint32_t>(esi17) > 63) {
                    esi17 = reinterpret_cast<void*>(63);
                }
                if (edi11->f4 == edi11->f8) {
                    if (reinterpret_cast<uint32_t>(esi17) >= 32) {
                        ecx18 = reinterpret_cast<uint32_t>(esi17) - 32;
                        ecx19 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi17) + reinterpret_cast<uint32_t>(eax5) + 4);
                        ebx20 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx18));
                        *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 0xc4) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 0xc4) & ebx20;
                        *ecx19 = reinterpret_cast<signed char>(*ecx19 - 1);
                        if (!*ecx19) {
                            a2->f4 = a2->f4 & ebx20;
                        }
                    } else {
                        ecx21 = esi17;
                        esi22 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi17) + reinterpret_cast<uint32_t>(eax5) + 4);
                        ebx23 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx21));
                        *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 68) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 68) & ebx23;
                        *esi22 = reinterpret_cast<signed char>(*esi22 - 1);
                        if (!*esi22) {
                            *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) & ebx23);
                        }
                    }
                    ebx15 = ebx15;
                }
                edi11->f8->f4 = edi11->f4;
                edi11->f4->f8 = edi11->f8;
                esi24 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(v13) + reinterpret_cast<uint32_t>(v12));
                v13 = esi24;
                esi16 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(esi24) >> 4) - 1);
                if (reinterpret_cast<uint32_t>(esi16) > 63) {
                    esi16 = reinterpret_cast<void*>(63);
                }
            }
            ecx25 = reinterpret_cast<struct s17*>(reinterpret_cast<uint32_t>(v8) + reinterpret_cast<uint32_t>(esi16) * 8);
            ebx15->f4 = *reinterpret_cast<struct s17**>(reinterpret_cast<uint32_t>(v8) + reinterpret_cast<uint32_t>(esi16) * 8 + 4);
            ebx15->f8 = ecx25;
            ecx25->f4 = ebx15;
            ebx15->f4->f8 = ebx15;
            if (ebx15->f4 == ebx15->f8) {
                cl26 = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi16) + reinterpret_cast<uint32_t>(eax5) + 4);
                *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi16) + reinterpret_cast<uint32_t>(eax5) + 4) = reinterpret_cast<signed char>(cl26 + 1);
                if (reinterpret_cast<uint32_t>(esi16) >= 32) {
                    if (!cl26) {
                        ecx27 = reinterpret_cast<uint32_t>(esi16) - 32;
                        a2->f4 = a2->f4 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx27);
                    }
                    eax28 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 0xc4);
                    ecx29 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(esi16) - 32);
                } else {
                    if (!cl26) {
                        ecx30 = esi16;
                        *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx30));
                    }
                    eax28 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 68);
                    ecx29 = esi16;
                }
                *eax28 = *eax28 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx29);
            }
            ebx15->f0 = v13;
            *reinterpret_cast<void**>(reinterpret_cast<uint32_t>(v13) + reinterpret_cast<uint32_t>(ebx15) - 4) = v13;
        }
    } else {
        if (*reinterpret_cast<unsigned char*>(&ebx10) & 1 || reinterpret_cast<signed char>(esi6) > reinterpret_cast<signed char>(reinterpret_cast<uint32_t>(ebx10) + reinterpret_cast<unsigned char>(ecx9))) {
            eax31 = 0;
            goto addr_40374e_29;
        } else {
            ecx32 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(v12) >> 4) - 1);
            v33 = ecx32;
            if (reinterpret_cast<uint32_t>(ecx32) > 63) {
                ecx32 = reinterpret_cast<void*>(63);
                v33 = reinterpret_cast<void*>(63);
            }
            if (edi11->f4 == edi11->f8) {
                if (reinterpret_cast<uint32_t>(ecx32) >= 32) {
                    ecx34 = reinterpret_cast<uint32_t>(ecx32) - 32;
                    ecx35 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(v33) + reinterpret_cast<uint32_t>(eax5) + 4);
                    ebx36 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx34));
                    *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 0xc4) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 0xc4) & ebx36;
                    *ecx35 = reinterpret_cast<signed char>(*ecx35 - 1);
                    if (!*ecx35) {
                        a2->f4 = a2->f4 & ebx36;
                    }
                } else {
                    ecx37 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(v33) + reinterpret_cast<uint32_t>(eax5) + 4);
                    ebx38 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx32));
                    *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 68) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 68) & ebx38;
                    *ecx37 = reinterpret_cast<signed char>(*ecx37 - 1);
                    if (!*ecx37) {
                        *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) & ebx38);
                    }
                }
            }
            edi11->f8->f4 = edi11->f4;
            edi11->f4->f8 = edi11->f8;
            v39 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(v12) + (reinterpret_cast<unsigned char>(ecx9) - reinterpret_cast<unsigned char>(esi6)));
            if (reinterpret_cast<int32_t>(v39) <= reinterpret_cast<int32_t>(0)) {
                edx40 = a3;
            } else {
                edi41 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(v39) >> 4) - 1);
                ecx42 = reinterpret_cast<struct s18*>(reinterpret_cast<unsigned char>(a3) + reinterpret_cast<unsigned char>(esi6) + 0xfffffffc);
                if (reinterpret_cast<uint32_t>(edi41) > 63) {
                    edi41 = reinterpret_cast<void*>(63);
                }
                ebx43 = reinterpret_cast<struct s18*>(reinterpret_cast<uint32_t>(v8) + reinterpret_cast<uint32_t>(edi41) * 8);
                ecx42->f4 = ebx43->f4;
                ecx42->f8 = ebx43;
                ebx43->f4 = ecx42;
                ecx42->f4->f8 = ecx42;
                if (ecx42->f4 == ecx42->f8) {
                    cl44 = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi41) + reinterpret_cast<uint32_t>(eax5) + 4);
                    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edi41) + reinterpret_cast<uint32_t>(eax5) + 4) = reinterpret_cast<signed char>(cl44 + 1);
                    if (reinterpret_cast<uint32_t>(edi41) >= 32) {
                        if (!cl44) {
                            ecx45 = reinterpret_cast<uint32_t>(edi41) - 32;
                            a2->f4 = a2->f4 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx45);
                        }
                        eax46 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 0xc4);
                        ecx47 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edi41) - 32);
                    } else {
                        if (!cl44) {
                            ecx48 = edi41;
                            *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx48));
                        }
                        eax46 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax5) + edx7 * 4 + 68);
                        ecx47 = edi41;
                    }
                    *eax46 = *eax46 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx47);
                }
                edx40 = a3;
                eax49 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(edx40) + reinterpret_cast<unsigned char>(esi6) + 0xfffffffc);
                *eax49 = v39;
                *reinterpret_cast<void**>(reinterpret_cast<uint32_t>(v39) + reinterpret_cast<uint32_t>(eax49) - 4) = v39;
            }
            eax50 = esi6 + 1;
            *reinterpret_cast<void***>(edx40 + 0xfffffffc) = eax50;
            *reinterpret_cast<void***>(reinterpret_cast<unsigned char>(edx40) + reinterpret_cast<unsigned char>(esi6) + 0xfffffff8) = eax50;
        }
    }
    eax31 = 1;
    addr_40374e_29:
    return eax31;
}

struct s8* g409970 = reinterpret_cast<struct s8*>(0);

uint32_t fun_403362(uint32_t ecx, struct s8* a2);

struct s19 {
    uint32_t f0;
    signed char[128] pad132;
    uint32_t f84;
};

struct s20 {
    void* f0;
    struct s20* f4;
    struct s20* f8;
};

struct s21 {
    uint32_t f0;
    void** f4;
};

struct s8* g409974 = reinterpret_cast<struct s8*>(0);

uint32_t g40996c = 0;

struct s8* fun_4032b1();

void** fun_402fa8(void** ecx, void** a2) {
    uint32_t eax3;
    struct s8* edx4;
    struct s8* edi5;
    struct s8* v6;
    void* ecx7;
    void* v8;
    uint32_t ecx9;
    uint32_t esi10;
    uint32_t v11;
    uint32_t v12;
    struct s8* eax13;
    struct s8* ebx14;
    struct s8* v15;
    int1_t zf16;
    int1_t zf17;
    uint32_t eax18;
    struct s9* eax19;
    uint32_t edx20;
    uint32_t v21;
    uint32_t edx22;
    uint32_t esi23;
    struct s19* ecx24;
    uint32_t edx25;
    void** eax26;
    int1_t zf27;
    void* edi28;
    void** v29;
    uint32_t ecx30;
    uint32_t ecx31;
    struct s20* edx32;
    void* ecx33;
    void* esi34;
    uint32_t ecx35;
    signed char* edi36;
    uint32_t* ecx37;
    uint32_t ebx38;
    int1_t zf39;
    void* ecx40;
    signed char* edi41;
    uint32_t ebx42;
    struct s20* ecx43;
    signed char cl44;
    uint32_t ecx45;
    uint32_t* edi46;
    uint32_t ecx47;
    void* ecx48;
    void* ecx49;
    struct s21* edx50;
    uint32_t ecx51;
    int1_t zf52;
    int1_t zf53;
    int1_t zf54;
    struct s8* eax55;

    eax3 = g409978;
    edx4 = g40997c;
    edi5 = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(edx4) + (eax3 + eax3 * 4) * 4);
    v6 = edi5;
    ecx7 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(a2 + 23) & 0xfffffff0);
    v8 = ecx7;
    ecx9 = (reinterpret_cast<int32_t>(ecx7) >> 4) - 1;
    if (reinterpret_cast<int32_t>(ecx9) >= reinterpret_cast<int32_t>(32)) {
        ecx9 = ecx9 + 0xffffffe0;
        esi10 = 0;
        v11 = 0;
        v12 = 0xffffffff >> *reinterpret_cast<signed char*>(&ecx9);
    } else {
        esi10 = 0xffffffff >> *reinterpret_cast<signed char*>(&ecx9);
        v12 = 0xffffffff;
        v11 = esi10;
    }
    eax13 = g409970;
    ebx14 = eax13;
    v15 = ebx14;
    if (reinterpret_cast<unsigned char>(ebx14) < reinterpret_cast<unsigned char>(edi5)) {
        do {
            ecx9 = ebx14->f4 & v12 | reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&ebx14->f0)) & esi10;
            if (ecx9) 
                break;
            ++ebx14;
            v15 = ebx14;
        } while (reinterpret_cast<unsigned char>(ebx14) < reinterpret_cast<unsigned char>(v6));
    }
    if (ebx14 != v6) 
        goto addr_40309d_8;
    ebx14 = edx4;
    while (zf16 = ebx14 == eax13, v15 = ebx14, reinterpret_cast<unsigned char>(ebx14) < reinterpret_cast<unsigned char>(eax13)) {
        ecx9 = ebx14->f4 & v12 | reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&ebx14->f0)) & esi10;
        if (ecx9) 
            goto addr_403040_12;
        ++ebx14;
    }
    addr_403042_14:
    if (!zf16) 
        goto addr_40309d_8;
    while (zf17 = ebx14 == v6, reinterpret_cast<unsigned char>(ebx14) < reinterpret_cast<unsigned char>(v6)) {
        if (ebx14->f8) 
            goto addr_403057_17;
        ++ebx14;
        v15 = ebx14;
    }
    addr_40305a_19:
    if (!zf17) {
        addr_403082_20:
        eax18 = fun_403362(ecx9, ebx14);
        ebx14->f10->f0 = eax18;
        if (ebx14->f10->f0 != 0xffffffff) {
            addr_40309d_8:
            g409970 = ebx14;
            eax19 = ebx14->f10;
            edx20 = eax19->f0;
            v21 = edx20;
            if (edx20 == 0xffffffff || !(*reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + edx20 * 4 + 0xc4) & v12 | *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + edx20 * 4 + 68) & esi10)) {
                edx22 = eax19->fc4 & v12;
                esi23 = eax19->f44 & v11;
                v21 = 0;
                ecx24 = reinterpret_cast<struct s19*>(&eax19->f44);
                esi10 = v11;
                if (!(edx22 | esi23)) {
                    do {
                        edx25 = ecx24->f84;
                        ++v21;
                        ecx24 = reinterpret_cast<struct s19*>(&ecx24->pad132);
                    } while (!(edx25 & v12 | esi10 & ecx24->f0));
                }
                edx20 = v21;
            }
        } else {
            addr_403096_24:
            eax26 = reinterpret_cast<void**>(0);
            goto addr_4032ac_25;
        }
    } else {
        ebx14 = edx4;
        while (zf27 = ebx14 == eax13, v15 = ebx14, reinterpret_cast<unsigned char>(ebx14) < reinterpret_cast<unsigned char>(eax13)) {
            if (ebx14->f8) 
                goto addr_403070_29;
            ++ebx14;
        }
        goto addr_403072_31;
    }
    edi28 = reinterpret_cast<void*>(0);
    v29 = reinterpret_cast<void**>(edx20 * 0x204 + reinterpret_cast<uint32_t>(eax19) + 0x144);
    ecx30 = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + edx20 * 4 + 68) & esi10;
    if (!ecx30) {
        ecx31 = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + edx20 * 4 + 0xc4);
        ecx30 = ecx31 & v12;
        edi28 = reinterpret_cast<void*>(32);
    }
    while (reinterpret_cast<int32_t>(ecx30) >= reinterpret_cast<int32_t>(0)) {
        ecx30 = ecx30 << 1;
        edi28 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edi28) + 1);
    }
    edx32 = *reinterpret_cast<struct s20**>(v29 + reinterpret_cast<uint32_t>(edi28) * 2 + 1);
    ecx33 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edx32->f0) - reinterpret_cast<uint32_t>(v8));
    esi34 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(ecx33) >> 4) - 1);
    if (reinterpret_cast<int32_t>(esi34) > reinterpret_cast<int32_t>(63)) {
        esi34 = reinterpret_cast<void*>(63);
    }
    if (esi34 == edi28) {
        addr_40325f_39:
        if (ecx33) {
            edx32->f0 = ecx33;
            *reinterpret_cast<void**>(reinterpret_cast<uint32_t>(ecx33) + reinterpret_cast<uint32_t>(edx32) + 0xfffffffc) = ecx33;
        }
    } else {
        if (edx32->f4 == edx32->f8) {
            if (reinterpret_cast<int32_t>(edi28) >= reinterpret_cast<int32_t>(32)) {
                ecx35 = reinterpret_cast<uint32_t>(edi28) + 0xffffffe0;
                edi36 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(eax19) + reinterpret_cast<uint32_t>(edi28) + 4);
                ecx37 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + v21 * 4 + 0xc4);
                ebx38 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx35));
                *ecx37 = *ecx37 & ebx38;
                *edi36 = reinterpret_cast<signed char>(*edi36 - 1);
                zf39 = *edi36 == 0;
                if (!zf39) {
                    addr_4031b8_44:
                    ebx14 = v15;
                } else {
                    ebx14 = v15;
                    ebx14->f4 = ebx14->f4 & ebx38;
                }
            } else {
                ecx40 = edi28;
                edi41 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(eax19) + reinterpret_cast<uint32_t>(edi28) + 4);
                ebx42 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx40));
                *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + v21 * 4 + 68) = ebx42 & *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + v21 * 4 + 68);
                *edi41 = reinterpret_cast<signed char>(*edi41 - 1);
                if (*edi41) 
                    goto addr_4031b8_44;
                ebx14 = v15;
                *reinterpret_cast<struct s8**>(&ebx14->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&ebx14->f0)) & ebx42);
            }
        }
        edx32->f8->f4 = edx32->f4;
        edx32->f4->f8 = edx32->f8;
        if (!ecx33) {
            ecx33 = ecx33;
        } else {
            ecx43 = reinterpret_cast<struct s20*>(v29 + reinterpret_cast<uint32_t>(esi34) * 2);
            edx32->f4 = *reinterpret_cast<struct s20**>(v29 + reinterpret_cast<uint32_t>(esi34) * 2 + 1);
            edx32->f8 = ecx43;
            ecx43->f4 = edx32;
            edx32->f4->f8 = edx32;
            if (edx32->f4 == edx32->f8) {
                cl44 = *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi34) + reinterpret_cast<uint32_t>(eax19) + 4);
                if (reinterpret_cast<int32_t>(esi34) >= reinterpret_cast<int32_t>(32)) {
                    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi34) + reinterpret_cast<uint32_t>(eax19) + 4) = reinterpret_cast<signed char>(cl44 + 1);
                    if (!cl44) {
                        ecx45 = reinterpret_cast<uint32_t>(esi34) + 0xffffffe0;
                        ebx14->f4 = ebx14->f4 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx45);
                    }
                    edi46 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + v21 * 4 + 0xc4);
                    ecx47 = reinterpret_cast<uint32_t>(esi34) + 0xffffffe0;
                    *edi46 = *edi46 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx47);
                } else {
                    *reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(esi34) + reinterpret_cast<uint32_t>(eax19) + 4) = reinterpret_cast<signed char>(cl44 + 1);
                    if (!cl44) {
                        ecx48 = esi34;
                        *reinterpret_cast<struct s8**>(&ebx14->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&ebx14->f0)) | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx48));
                    }
                    ecx49 = esi34;
                    *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + v21 * 4 + 68) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax19) + v21 * 4 + 68) | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx49);
                }
            }
            ecx33 = ecx33;
            goto addr_40325f_39;
        }
    }
    edx50 = reinterpret_cast<struct s21*>(reinterpret_cast<uint32_t>(edx32) + reinterpret_cast<uint32_t>(ecx33));
    ecx51 = reinterpret_cast<uint32_t>(v8) + 1;
    edx50->f0 = ecx51;
    *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(edx50) + reinterpret_cast<uint32_t>(v8) + 0xfffffffc) = ecx51;
    zf52 = *v29 == 0;
    *v29 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(*v29) + 1);
    if (zf52 && ((zf53 = ebx14 == g409974, zf53) && (zf54 = v21 == g40996c, zf54))) {
        g409974 = reinterpret_cast<struct s8*>(0);
    }
    eax19->f0 = v21;
    eax26 = reinterpret_cast<void**>(&edx50->f4);
    addr_4032ac_25:
    return eax26;
    addr_403072_31:
    if (!zf27) 
        goto addr_403082_20;
    eax55 = fun_4032b1();
    ebx14 = eax55;
    v15 = ebx14;
    if (!ebx14) 
        goto addr_403096_24; else 
        goto addr_403082_20;
    addr_403070_29:
    zf27 = ebx14 == eax13;
    goto addr_403072_31;
    addr_403057_17:
    zf17 = ebx14 == v6;
    goto addr_40305a_19;
    addr_403040_12:
    zf16 = ebx14 == eax13;
    goto addr_403042_14;
}

int32_t g409708 = 0;

int32_t fun_403e20(void** ecx) {
    int32_t eax2;
    int32_t eax3;

    eax2 = g409708;
    if (!eax2 || (eax3 = reinterpret_cast<int32_t>(eax2()), eax3 == 0)) {
        return 0;
    } else {
        return 1;
    }
}

void** fun_4039af(void** ecx, void** a2, struct s7** a3, void*** a4) {
    void** eax5;
    struct s7* ecx6;
    void** ecx7;

    eax5 = a2;
    ecx6 = reinterpret_cast<struct s7*>(0x407208);
    while (reinterpret_cast<unsigned char>(eax5) <= reinterpret_cast<unsigned char>(ecx6->f10) || reinterpret_cast<unsigned char>(eax5) >= reinterpret_cast<unsigned char>(ecx6->f14)) {
        ecx6 = ecx6->f0;
        if (ecx6 == 0x407208) 
            goto addr_403a02_4;
    }
    if (!(*reinterpret_cast<unsigned char*>(&eax5) & 15) && (reinterpret_cast<unsigned char>(eax5) & 0xfff) >= 0x100) {
        *a3 = ecx6;
        ecx7 = eax5;
        *reinterpret_cast<uint16_t*>(&ecx7) = reinterpret_cast<uint16_t>(*reinterpret_cast<uint16_t*>(&ecx7) & 0xf000);
        *a4 = ecx7;
        return (reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(eax5) - reinterpret_cast<unsigned char>(ecx7) - 0x100) >> 4) + reinterpret_cast<unsigned char>(ecx7) + 8;
    }
    addr_403a02_4:
    return 0;
}

struct s22 {
    uint32_t f0;
    int32_t f4;
};

uint32_t fun_403d77(void** ecx, void** a2, void** a3, void** a4, void** a5) {
    void** edx6;
    void** ebx7;
    void** ecx8;
    uint32_t v9;
    struct s22* edi10;
    void** esi11;
    void** eax12;
    int1_t zf13;
    void** eax14;
    void** eax15;

    edx6 = a4;
    ebx7 = a3;
    ecx8 = reinterpret_cast<void**>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edx6))));
    v9 = 0;
    edi10 = reinterpret_cast<struct s22*>(reinterpret_cast<uint32_t>(a2 + (reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(ebx7) - reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(a2 + 16))) >> 12) * 8) + 24);
    if (reinterpret_cast<unsigned char>(ecx8) <= reinterpret_cast<unsigned char>(a5)) {
        if (reinterpret_cast<unsigned char>(ecx8) >= reinterpret_cast<unsigned char>(a5)) 
            goto addr_403e18_3;
        esi11 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(edx6) + reinterpret_cast<unsigned char>(a5));
        if (reinterpret_cast<unsigned char>(ebx7 + 0xf8) < reinterpret_cast<unsigned char>(esi11)) 
            goto addr_403e18_3;
        eax12 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(ecx8) + reinterpret_cast<unsigned char>(edx6));
        while (zf13 = eax12 == esi11, reinterpret_cast<unsigned char>(eax12) < reinterpret_cast<unsigned char>(esi11)) {
            if (*reinterpret_cast<void***>(eax12)) 
                goto addr_403dd2_8;
            ++eax12;
        }
    } else {
        eax14 = a5;
        *reinterpret_cast<void***>(edx6) = eax14;
        edi10->f0 = edi10->f0 + (reinterpret_cast<unsigned char>(ecx8) - reinterpret_cast<unsigned char>(eax14));
        edi10->f4 = 0xf1;
        goto addr_403e11_11;
    }
    addr_403dd4_12:
    if (!zf13) {
        addr_403e18_3:
        return v9;
    } else {
        *reinterpret_cast<void***>(edx6) = a5;
        if (reinterpret_cast<unsigned char>(edx6) <= reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ebx7)) && reinterpret_cast<unsigned char>(esi11) > reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ebx7))) {
            if (reinterpret_cast<unsigned char>(esi11) >= reinterpret_cast<unsigned char>(ebx7 + 0xf8)) {
                *reinterpret_cast<void***>(ebx7 + 4) = reinterpret_cast<void**>(0);
                *reinterpret_cast<void***>(ebx7) = ebx7 + 8;
            } else {
                eax15 = reinterpret_cast<void**>(0);
                *reinterpret_cast<void***>(ebx7) = esi11;
                if (!*reinterpret_cast<void***>(esi11)) {
                    do {
                        ++eax15;
                    } while (!*reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(esi11) + reinterpret_cast<unsigned char>(eax15)));
                }
                *reinterpret_cast<void***>(ebx7 + 4) = eax15;
            }
        }
        edi10->f0 = edi10->f0 + (reinterpret_cast<unsigned char>(ecx8) - reinterpret_cast<unsigned char>(a5));
    }
    addr_403e11_11:
    v9 = 1;
    goto addr_403e18_3;
    addr_403dd2_8:
    zf13 = eax12 == esi11;
    goto addr_403dd4_12;
}

void fun_401d86(void** ecx, void** a2, void** a3, void** a4, void** a5) {
    void** esi6;
    void** eax7;

    esi6 = a2;
    while (reinterpret_cast<unsigned char>(esi6) < reinterpret_cast<unsigned char>(a3)) {
        eax7 = *reinterpret_cast<void***>(esi6);
        if (eax7) {
            eax7();
        }
        esi6 = esi6 + 4;
    }
    return;
}

void fun_401ced(int32_t a1, int32_t a2, int32_t a3);

void fun_401ccb(void** ecx) {
    fun_401ced(__return_address(), 0, 0);
    return;
}

void** fun_401e3f(void** ecx, void** a2) {
    void*** eax3;
    void** esi4;
    int1_t below_or_equal5;
    void** eax6;
    void** esi7;
    int1_t below_or_equal8;
    void** v9;
    void** v10;
    void** eax11;

    eax3 = g409988;
    esi4 = a2;
    if (reinterpret_cast<int1_t>(eax3 == 3)) {
        below_or_equal5 = reinterpret_cast<unsigned char>(esi4) <= reinterpret_cast<unsigned char>(g409980);
        if (below_or_equal5 && (eax6 = fun_402fa8(ecx, esi4), ecx = esi4, !!eax6)) {
            return eax6;
        }
    }
    if (!reinterpret_cast<int1_t>(eax3 == 2)) {
        if (!esi4) {
            esi4 = reinterpret_cast<void**>(1);
        }
    } else {
        if (!a2) {
            esi7 = reinterpret_cast<void**>(16);
        } else {
            esi7 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(a2 + 15) & 0xfffffff0);
        }
        below_or_equal8 = reinterpret_cast<unsigned char>(esi7) <= reinterpret_cast<unsigned char>(g40922c);
        if (!below_or_equal8) 
            goto addr_401ea2_11; else 
            goto addr_401e83_12;
    }
    esi7 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esi4 + 15) & 0xfffffff0);
    addr_401ea2_11:
    v9 = g409984;
    HeapAlloc(ecx, v9, 0, esi7);
    goto addr_401eb1_14;
    addr_401e83_12:
    v10 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi7) >> 4);
    eax11 = fun_403a4b(ecx, v10);
    ecx = v10;
    if (eax11) {
        addr_401eb1_14:
        goto __return_address();
    } else {
        goto addr_401ea2_11;
    }
}

void** g409228 = reinterpret_cast<void**>(8);

void** fun_403c53(void** ecx, void** a2, void** a3, void** a4);

int32_t VirtualAlloc = 0x69e2;

void** fun_404fc0(void** a1, void** a2, void* a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9);

struct s23 {
    void** f0;
    signed char[243] pad244;
    unsigned char ff4;
};

void** fun_403753();

void** fun_403a4b(void** ecx, void** a2) {
    void** v3;
    void** esi4;
    void** esi5;
    void** v6;
    void** edi7;
    void** ebx8;
    void** edi9;
    void** eax10;
    void** v11;
    void** ecx12;
    void** eax13;
    void** eax14;
    void** ecx15;
    void** v16;
    void** v17;
    void** eax18;
    int1_t zf19;
    void** edi20;
    void** ebx21;
    uint32_t v22;
    void** eax23;
    void** esi24;
    void** v25;
    void** eax26;
    void** ecx27;
    struct s23* eax28;
    uint32_t v29;
    void** eax30;
    uint1_t cf31;
    void** eax32;
    void** eax33;
    void** ecx34;

    v3 = esi4;
    esi5 = g409228;
    v6 = edi7;
    while (1) {
        if (*reinterpret_cast<void***>(esi5 + 16) == 0xffffffff) {
            ebx8 = a2;
        } else {
            edi9 = *reinterpret_cast<void***>(esi5 + 8);
            eax10 = reinterpret_cast<void**>((reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(edi9) - reinterpret_cast<unsigned char>(esi5) - 24) >> 3 << 12) + reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi5 + 16)));
            v11 = eax10;
            if (reinterpret_cast<unsigned char>(edi9) >= reinterpret_cast<unsigned char>(esi5 + 0x2018)) {
                ebx8 = a2;
            } else {
                do {
                    ecx12 = *reinterpret_cast<void***>(edi9);
                    ebx8 = a2;
                    if (reinterpret_cast<signed char>(ecx12) >= reinterpret_cast<signed char>(ebx8) && reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edi9 + 4)) > reinterpret_cast<unsigned char>(ebx8)) {
                        eax13 = fun_403c53(ecx12, eax10, ecx12, ebx8);
                        if (eax13) 
                            goto addr_403b16_8;
                        eax10 = v11;
                        *reinterpret_cast<void***>(edi9 + 4) = ebx8;
                    }
                    edi9 = edi9 + 8;
                    eax10 = eax10 + 0x1000;
                    v11 = eax10;
                } while (reinterpret_cast<unsigned char>(edi9) < reinterpret_cast<unsigned char>(esi5 + 0x2018));
            }
            eax14 = *reinterpret_cast<void***>(esi5 + 8);
            ecx15 = *reinterpret_cast<void***>(esi5 + 16);
            edi9 = esi5 + 24;
            v16 = eax14;
            v17 = ecx15;
            if (reinterpret_cast<unsigned char>(edi9) < reinterpret_cast<unsigned char>(eax14)) {
                do {
                    eax18 = *reinterpret_cast<void***>(edi9);
                    if (reinterpret_cast<signed char>(eax18) >= reinterpret_cast<signed char>(ebx8) && reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edi9 + 4)) > reinterpret_cast<unsigned char>(ebx8)) {
                        eax13 = fun_403c53(ecx15, v17, eax18, ebx8);
                        if (eax13) 
                            goto addr_403b16_8;
                        *reinterpret_cast<void***>(edi9 + 4) = ebx8;
                    }
                    v17 = v17 + 0x1000;
                    edi9 = edi9 + 8;
                } while (reinterpret_cast<unsigned char>(edi9) < reinterpret_cast<unsigned char>(v16));
            }
        }
        esi5 = *reinterpret_cast<void***>(esi5);
        zf19 = esi5 == g409228;
        if (zf19) 
            break;
    }
    edi20 = reinterpret_cast<void**>(0x407208);
    while (*reinterpret_cast<void***>(edi20 + 16) == 0xffffffff || !*reinterpret_cast<void***>(edi20 + 12)) {
        edi20 = *reinterpret_cast<void***>(edi20);
        if (edi20 == 0x407208) 
            goto addr_403c1a_23;
    }
    ebx21 = *reinterpret_cast<void***>(edi20 + 12);
    v22 = 0;
    eax23 = ebx21;
    esi24 = reinterpret_cast<void**>((reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(ebx21) - reinterpret_cast<unsigned char>(edi20) - 24) >> 3 << 12) + reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edi20 + 16)));
    if (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(ebx21) == 0xffffffff)) {
        do {
            if (reinterpret_cast<int32_t>(v22) >= reinterpret_cast<int32_t>(16)) 
                break;
            eax23 = eax23 + 8;
            ++v22;
        } while (*reinterpret_cast<void***>(eax23) == 0xffffffff);
    }
    v25 = reinterpret_cast<void**>(v22 << 12);
    eax26 = reinterpret_cast<void**>(VirtualAlloc(esi24, v25, 0x1000, 4));
    if (eax26 != esi24) {
        addr_403c4c_29:
        eax13 = reinterpret_cast<void**>(0);
    } else {
        fun_404fc0(esi24, 0, 0, esi24, v25, 0x1000, 4, v6, v3);
        ecx27 = ebx21;
        if (!(reinterpret_cast<uint1_t>(reinterpret_cast<int32_t>(v22) < reinterpret_cast<int32_t>(0)) | reinterpret_cast<uint1_t>(v22 == 0))) {
            eax28 = reinterpret_cast<struct s23*>(esi24 + 4);
            v29 = v22;
            do {
                eax28->ff4 = 0xff;
                *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax28) - 4) = reinterpret_cast<uint32_t>(eax28) + 4;
                eax28->f0 = reinterpret_cast<void**>(0xf0);
                *reinterpret_cast<void***>(ecx27) = reinterpret_cast<void**>(0xf0);
                *reinterpret_cast<void***>(ecx27 + 4) = reinterpret_cast<void**>(0xf1);
                eax28 = reinterpret_cast<struct s23*>(reinterpret_cast<uint32_t>(eax28) + 0x1000);
                ecx27 = ecx27 + 8;
                --v29;
            } while (v29);
        }
        g409228 = edi20;
        eax30 = edi20 + 0x2018;
        while (cf31 = reinterpret_cast<uint1_t>(reinterpret_cast<unsigned char>(ecx27) < reinterpret_cast<unsigned char>(eax30)), cf31) {
            if (*reinterpret_cast<void***>(ecx27) == 0xffffffff) 
                goto addr_403bf5_36;
            ecx27 = ecx27 + 8;
        }
        goto addr_403bf7_38;
    }
    addr_403c4e_39:
    return eax13;
    addr_403bf7_38:
    *reinterpret_cast<void***>(edi20 + 12) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(eax30) - (reinterpret_cast<unsigned char>(eax30) + reinterpret_cast<uint1_t>(reinterpret_cast<unsigned char>(eax30) < reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(eax30) + cf31))) & reinterpret_cast<unsigned char>(ecx27));
    eax32 = a2;
    *reinterpret_cast<void***>(esi24 + 8) = eax32;
    *reinterpret_cast<void***>(edi20 + 8) = ebx21;
    *reinterpret_cast<void***>(ebx21) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ebx21)) - reinterpret_cast<unsigned char>(eax32));
    *reinterpret_cast<void***>(esi24 + 4) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi24 + 4)) - reinterpret_cast<unsigned char>(eax32));
    eax13 = esi24 + 0x100;
    *reinterpret_cast<void***>(esi24) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi24) + reinterpret_cast<unsigned char>(eax32) + 8);
    goto addr_403c4e_39;
    addr_403bf5_36:
    cf31 = reinterpret_cast<uint1_t>(reinterpret_cast<unsigned char>(ecx27) < reinterpret_cast<unsigned char>(eax30));
    goto addr_403bf7_38;
    addr_403c1a_23:
    eax33 = fun_403753();
    if (!eax33) 
        goto addr_403c4c_29;
    ecx34 = *reinterpret_cast<void***>(eax33 + 16);
    *reinterpret_cast<void***>(ecx34 + 8) = ebx8;
    g409228 = eax33;
    *reinterpret_cast<void***>(ecx34) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(ecx34) + reinterpret_cast<unsigned char>(ebx8) + 8);
    *reinterpret_cast<void***>(ecx34 + 4) = reinterpret_cast<void**>(0xf0 - reinterpret_cast<unsigned char>(ebx8));
    *reinterpret_cast<void***>(eax33 + 24) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(eax33 + 24)) - reinterpret_cast<unsigned char>(ebx8));
    eax13 = ecx34 + 0x100;
    goto addr_403c4e_39;
    addr_403b16_8:
    g409228 = esi5;
    *reinterpret_cast<void***>(edi9) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edi9)) - reinterpret_cast<unsigned char>(ebx8));
    *reinterpret_cast<void***>(esi5 + 8) = edi9;
    goto addr_403c4e_39;
}

struct s25 {
    signed char[4] pad4;
    struct s25* f4;
    struct s25* f8;
};

struct s24 {
    signed char[4] pad4;
    struct s25* f4;
    struct s25* f8;
};

int32_t VirtualFree = 0x69bc;

struct s8* fun_404c80(struct s8* a1, struct s8* a2, void* a3, void** a4, int32_t a5, struct s9* a6, void* a7, int32_t a8, int32_t a9, void* a10, int32_t a11, int32_t a12);

void fun_402c7f(void** ecx, struct s8* a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9) {
    struct s9* eax10;
    void** esi11;
    uint32_t edi12;
    void*** v13;
    void** ecx14;
    void** v15;
    struct s24* ebx16;
    void* v17;
    void** v18;
    void* edx19;
    struct s24* v20;
    void* edx21;
    void** ecx22;
    uint32_t ecx23;
    signed char* ecx24;
    uint32_t ebx25;
    void* ecx26;
    signed char* ecx27;
    uint32_t ebx28;
    struct s8* edx29;
    uint32_t ebx30;
    struct s8* ebx31;
    void** v32;
    void** ecx33;
    uint32_t ecx34;
    uint32_t esi35;
    struct s8* ecx36;
    uint32_t esi37;
    void** ecx38;
    signed char cl39;
    uint32_t ecx40;
    uint32_t ecx41;
    uint32_t* eax42;
    struct s8* ecx43;
    struct s8* ecx44;
    uint32_t* eax45;
    struct s8* eax46;
    uint32_t ecx47;
    int32_t esi48;
    void* ecx49;
    uint32_t ecx50;
    struct s8* eax51;
    struct s8* eax52;
    uint32_t ecx53;
    struct s8* eax54;
    struct s8* eax55;
    void* v56;
    struct s8* eax57;
    struct s9* v58;
    void** v59;
    uint32_t eax60;
    struct s8* edx61;
    struct s8* eax62;
    int1_t below_or_equal63;
    struct s8* eax64;

    eax10 = a2->f10;
    esi11 = a3 + 0xfffffffc;
    edi12 = reinterpret_cast<unsigned char>(a3) - reinterpret_cast<uint32_t>(a2->fc) >> 15;
    v13 = reinterpret_cast<void***>(edi12 * 0x204 + reinterpret_cast<uint32_t>(eax10) + 0x144);
    ecx14 = *reinterpret_cast<void***>(esi11) - 1;
    v15 = ecx14;
    if (!(*reinterpret_cast<unsigned char*>(&ecx14) & 1)) {
        ebx16 = reinterpret_cast<struct s24*>(reinterpret_cast<unsigned char>(ecx14) + reinterpret_cast<unsigned char>(esi11));
        v17 = *reinterpret_cast<void**>(reinterpret_cast<unsigned char>(ecx14) + reinterpret_cast<unsigned char>(esi11));
        v18 = *reinterpret_cast<void***>(esi11 + 0xfffffffc);
        edx19 = v17;
        v20 = ebx16;
        if (!(*reinterpret_cast<unsigned char*>(&edx19) & 1)) {
            edx21 = reinterpret_cast<void*>((reinterpret_cast<int32_t>(edx19) >> 4) - 1);
            if (reinterpret_cast<uint32_t>(edx21) > 63) {
                edx21 = reinterpret_cast<void*>(63);
            }
            if (ebx16->f4 != ebx16->f8) {
                ecx22 = v15;
            } else {
                if (reinterpret_cast<uint32_t>(edx21) >= 32) {
                    ecx23 = reinterpret_cast<uint32_t>(edx21) - 32;
                    ecx24 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edx21) + reinterpret_cast<uint32_t>(eax10) + 4);
                    ebx25 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx23));
                    *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 0xc4) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 0xc4) & ebx25;
                    *ecx24 = reinterpret_cast<signed char>(*ecx24 - 1);
                    if (!*ecx24) {
                        a2->f4 = a2->f4 & ebx25;
                    }
                } else {
                    ecx26 = edx21;
                    ecx27 = reinterpret_cast<signed char*>(reinterpret_cast<uint32_t>(edx21) + reinterpret_cast<uint32_t>(eax10) + 4);
                    ebx28 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx26));
                    *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 68) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 68) & ebx28;
                    *ecx27 = reinterpret_cast<signed char>(*ecx27 - 1);
                    if (!*ecx27) {
                        *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) & ebx28);
                    }
                }
                ecx22 = v15;
                ebx16 = v20;
            }
            ecx14 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(ecx22) + reinterpret_cast<uint32_t>(v17));
            ebx16->f8->f4 = ebx16->f4;
            v15 = ecx14;
            v20->f4->f8 = v20->f8;
        }
        edx29 = reinterpret_cast<struct s8*>((reinterpret_cast<signed char>(ecx14) >> 4) - 1);
        if (reinterpret_cast<unsigned char>(edx29) > reinterpret_cast<unsigned char>(63)) {
            edx29 = reinterpret_cast<struct s8*>(63);
        }
        ebx30 = reinterpret_cast<unsigned char>(v18) & 1;
        if (ebx30) {
            ebx31 = a2;
        } else {
            v32 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi11) - reinterpret_cast<unsigned char>(v18));
            ebx31 = reinterpret_cast<struct s8*>((reinterpret_cast<signed char>(v18) >> 4) - 1);
            if (reinterpret_cast<unsigned char>(ebx31) > reinterpret_cast<unsigned char>(63)) {
                ebx31 = reinterpret_cast<struct s8*>(63);
            }
            ecx33 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(ecx14) + reinterpret_cast<unsigned char>(v18));
            v15 = ecx33;
            edx29 = reinterpret_cast<struct s8*>((reinterpret_cast<signed char>(ecx33) >> 4) - 1);
            if (reinterpret_cast<unsigned char>(edx29) > reinterpret_cast<unsigned char>(63)) {
                edx29 = reinterpret_cast<struct s8*>(63);
            }
            if (ebx31 != edx29) {
                if (*reinterpret_cast<void***>(v32 + 4) == *reinterpret_cast<void***>(v32 + 8)) {
                    if (reinterpret_cast<unsigned char>(ebx31) >= reinterpret_cast<unsigned char>(32)) {
                        ecx34 = reinterpret_cast<unsigned char>(ebx31) + 0xffffffe0;
                        esi35 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx34));
                        *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 0xc4) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 0xc4) & esi35;
                        *reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(ebx31) + reinterpret_cast<uint32_t>(eax10) + 4) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(ebx31) + reinterpret_cast<uint32_t>(eax10) + 4) - 1);
                        if (!*reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(ebx31) + reinterpret_cast<uint32_t>(eax10) + 4)) {
                            a2->f4 = a2->f4 & esi35;
                        }
                    } else {
                        ecx36 = ebx31;
                        esi37 = ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx36));
                        *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 68) = *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 68) & esi37;
                        *reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(ebx31) + reinterpret_cast<uint32_t>(eax10) + 4) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(ebx31) + reinterpret_cast<uint32_t>(eax10) + 4) - 1);
                        if (!*reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(ebx31) + reinterpret_cast<uint32_t>(eax10) + 4)) {
                            *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) & esi37);
                        }
                    }
                }
                *reinterpret_cast<void***>(*reinterpret_cast<void***>(v32 + 8) + 4) = *reinterpret_cast<void***>(v32 + 4);
                *reinterpret_cast<void***>(*reinterpret_cast<void***>(v32 + 4) + 8) = *reinterpret_cast<void***>(v32 + 8);
            }
            esi11 = v32;
        }
        if ((ebx30 || ebx31 != edx29) && (ecx38 = reinterpret_cast<void**>(v13 + reinterpret_cast<unsigned char>(edx29) * 8), *reinterpret_cast<void***>(esi11 + 4) = (v13 + reinterpret_cast<unsigned char>(edx29) * 8)[4], *reinterpret_cast<void***>(esi11 + 8) = ecx38, *reinterpret_cast<void***>(ecx38 + 4) = esi11, *reinterpret_cast<void***>(*reinterpret_cast<void***>(esi11 + 4) + 8) = esi11, *reinterpret_cast<void***>(esi11 + 4) == *reinterpret_cast<void***>(esi11 + 8))) {
            cl39 = *reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(edx29) + reinterpret_cast<uint32_t>(eax10) + 4);
            *reinterpret_cast<signed char*>(reinterpret_cast<unsigned char>(edx29) + reinterpret_cast<uint32_t>(eax10) + 4) = reinterpret_cast<signed char>(cl39 + 1);
            if (reinterpret_cast<unsigned char>(edx29) >= reinterpret_cast<unsigned char>(32)) {
                if (!cl39) {
                    ecx40 = reinterpret_cast<unsigned char>(edx29) + 0xffffffe0;
                    a2->f4 = a2->f4 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx40);
                }
                ecx41 = reinterpret_cast<unsigned char>(edx29) + 0xffffffe0;
                eax42 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 0xc4);
                *eax42 = *eax42 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx41);
            } else {
                if (!cl39) {
                    ecx43 = edx29;
                    *reinterpret_cast<struct s8**>(&a2->f0) = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(*reinterpret_cast<struct s8**>(&a2->f0)) | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx43));
                }
                ecx44 = edx29;
                eax45 = reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax10) + edi12 * 4 + 68);
                *eax45 = *eax45 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx44);
            }
        }
        *reinterpret_cast<void***>(esi11) = v15;
        *reinterpret_cast<void***>(reinterpret_cast<unsigned char>(v15) + reinterpret_cast<unsigned char>(esi11) + 0xfffffffc) = v15;
        *v13 = *v13 - 1;
        if (!*v13) {
            eax46 = g409974;
            if (eax46) {
                ecx47 = g40996c;
                esi48 = VirtualFree;
                ecx49 = reinterpret_cast<void*>((ecx47 << 15) + reinterpret_cast<uint32_t>(eax46->fc));
                esi48(ecx49, 0x8000, 0x4000);
                ecx50 = g40996c;
                eax51 = g409974;
                eax51->f8 = eax51->f8 | 0x80000000 >> *reinterpret_cast<signed char*>(&ecx50);
                eax52 = g409974;
                ecx53 = g40996c;
                *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax52->f10) + ecx53 * 4 + 0xc4) = 0;
                eax54 = g409974;
                eax54->f10->f43 = reinterpret_cast<signed char>(eax54->f10->f43 - 1);
                eax55 = g409974;
                if (!eax55->f10->f43) {
                    eax55->f4 = eax55->f4 & 0xfffffffe;
                    eax55 = g409974;
                }
                if (eax55->f8 == 0xffffffff) {
                    v56 = eax55->fc;
                    esi48(v56, 0, 0x8000, ecx49, 0x8000, 0x4000);
                    eax57 = g409974;
                    v58 = eax57->f10;
                    v59 = g409984;
                    HeapFree(v59, 0, v58, v56, 0, 0x8000, ecx49, 0x8000, 0x4000);
                    eax60 = g409978;
                    edx61 = g40997c;
                    eax62 = g409974;
                    fun_404c80(eax62, eax62 + 1, (eax60 + eax60 * 4 << 2) - reinterpret_cast<unsigned char>(eax62) + reinterpret_cast<unsigned char>(edx61) + 0xffffffec, v59, 0, v58, v56, 0, 0x8000, ecx49, 0x8000, 0x4000);
                    --g409978;
                    below_or_equal63 = reinterpret_cast<unsigned char>(a2) <= reinterpret_cast<unsigned char>(g409974);
                    if (!below_or_equal63) {
                        --a2;
                    }
                    eax64 = g40997c;
                    g409970 = eax64;
                }
            }
            g40996c = edi12;
            g409974 = a2;
        }
    }
    return;
}

struct s26 {
    uint32_t f0;
    int32_t f4;
};

int32_t g409704 = 0;

void fun_4038ed(void** ecx);

void fun_403a06(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10) {
    struct s26* eax11;
    int1_t zf12;
    int1_t zf13;

    eax11 = reinterpret_cast<struct s26*>(reinterpret_cast<uint32_t>(a2 + (reinterpret_cast<int32_t>(reinterpret_cast<unsigned char>(a3) - reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(a2 + 16))) >> 12) * 8) + 24);
    eax11->f0 = eax11->f0 + reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(a4));
    *reinterpret_cast<void***>(a4) = reinterpret_cast<void**>(0);
    zf12 = eax11->f0 == 0xf0;
    eax11->f4 = 0xf1;
    if (zf12 && (++g409704, zf13 = g409704 == 32, zf13)) {
        fun_4038ed(a4);
    }
    return;
}

uint32_t fun_4041b7(void** a1);

uint32_t fun_40457b(void** a1, void** a2, void** a3) {
    int1_t zf4;
    uint32_t eax5;

    zf4 = g409aa8 == 0;
    if (zf4) {
        eax5 = fun_4041b7(0xfd);
        g409aa8 = reinterpret_cast<void**>(1);
    }
    return eax5;
}

uint32_t fun_404186(unsigned char a1, uint32_t a2, unsigned char a3);

uint32_t fun_404175(void** ecx) {
    int32_t v2;
    uint32_t eax3;

    v2 = reinterpret_cast<int32_t>(__return_address());
    eax3 = fun_404186(*reinterpret_cast<unsigned char*>(&v2), 0, 4);
    return eax3;
}

struct s13* fun_404690(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6) {
    void** ecx7;
    void** eax8;

    ecx7 = a2;
    if (!(reinterpret_cast<unsigned char>(ecx7) & 3)) {
        while (1) {
            addr_4046b0_2:
            ecx7 = ecx7 + 4;
            if (!((reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ecx7)) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(*reinterpret_cast<void***>(ecx7) + 0x7efefeff)) & 0x81010100)) 
                continue;
            eax8 = *reinterpret_cast<void***>(ecx7 + 0xfffffffc);
            if (!*reinterpret_cast<signed char*>(&eax8)) 
                break;
            if (!*reinterpret_cast<signed char*>(&eax8 + 1)) 
                goto addr_4046f7_5;
            if (!(reinterpret_cast<unsigned char>(eax8) & 0xff0000)) 
                goto addr_4046ed_7;
            if (!(reinterpret_cast<unsigned char>(eax8) & 0xff000000)) 
                goto addr_4046e3_9;
        }
    } else {
        do {
            ++ecx7;
            if (!*reinterpret_cast<void***>(ecx7)) 
                goto addr_4046e3_9;
        } while (reinterpret_cast<unsigned char>(ecx7) & 3);
        goto addr_4046ab_13;
    }
    return reinterpret_cast<unsigned char>(ecx7 + 0xfffffffc) - reinterpret_cast<unsigned char>(a2);
    addr_4046f7_5:
    return reinterpret_cast<unsigned char>(ecx7 + 0xfffffffd) - reinterpret_cast<unsigned char>(a2);
    addr_4046ed_7:
    return reinterpret_cast<unsigned char>(ecx7 + 0xfffffffe) - reinterpret_cast<unsigned char>(a2);
    addr_4046e3_9:
    return reinterpret_cast<unsigned char>(ecx7 + 0xffffffff) - reinterpret_cast<unsigned char>(a2);
    addr_4046ab_13:
    goto addr_4046b0_2;
}

void fun_40224a(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6) {
    void** ecx7;
    void** esi8;
    void** edi9;
    void** eax10;
    void** dl11;
    uint32_t v12;
    uint32_t v13;
    uint32_t ebx14;
    uint32_t edx15;
    uint32_t ebx16;

    ecx7 = a6;
    *reinterpret_cast<void***>(ecx7) = reinterpret_cast<void**>(0);
    esi8 = a4;
    edi9 = a3;
    *reinterpret_cast<void***>(a5) = reinterpret_cast<void**>(1);
    eax10 = a2;
    if (edi9) {
        *reinterpret_cast<void***>(edi9) = esi8;
        edi9 = edi9 + 4;
        a3 = edi9;
    }
    if (!reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax10) == 34)) {
        do {
            *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
            if (esi8) {
                *reinterpret_cast<void***>(esi8) = *reinterpret_cast<void***>(eax10);
                ++esi8;
            }
            dl11 = *reinterpret_cast<void***>(eax10);
            ++eax10;
            if (*reinterpret_cast<unsigned char*>(dl11 + 0x409861) & 4) {
                *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
                if (esi8) {
                    *reinterpret_cast<void***>(esi8) = *reinterpret_cast<void***>(eax10);
                    ++esi8;
                }
                ++eax10;
            }
            if (dl11 == 32) 
                break;
            if (!dl11) 
                goto addr_4022f5_12;
        } while (!reinterpret_cast<int1_t>(dl11 == 9));
        if (dl11) {
            if (esi8) {
                *reinterpret_cast<void***>(esi8 + 0xffffffff) = reinterpret_cast<void**>(0);
            }
        } else {
            addr_4022f5_12:
            --eax10;
        }
    } else {
        while ((++eax10, *reinterpret_cast<void***>(eax10 + 1) != 34) && *reinterpret_cast<void***>(eax10 + 1)) {
            if (*reinterpret_cast<unsigned char*>(*reinterpret_cast<void***>(eax10 + 1) + 0x409861) & 4 && (*reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1, !!esi8)) {
                *reinterpret_cast<void***>(esi8) = *reinterpret_cast<void***>(eax10);
                ++esi8;
                ++eax10;
            }
            *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
            if (!esi8) 
                continue;
            *reinterpret_cast<void***>(esi8) = *reinterpret_cast<void***>(eax10);
            ++esi8;
        }
        *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
        if (!esi8) 
            goto addr_4022b5_23; else 
            goto addr_4022b1_24;
    }
    addr_402300_25:
    v12 = 0;
    while (*reinterpret_cast<void***>(eax10)) {
        while (*reinterpret_cast<void***>(eax10) == 32 || reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax10) == 9)) {
            ++eax10;
        }
        if (!*reinterpret_cast<void***>(eax10)) 
            break;
        if (edi9) {
            *reinterpret_cast<void***>(edi9) = esi8;
            edi9 = edi9 + 4;
            a3 = edi9;
        }
        *reinterpret_cast<void***>(a5) = *reinterpret_cast<void***>(a5) + 1;
        while (1) {
            v13 = 1;
            ebx14 = 0;
            while (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax10) == 92)) {
                ++eax10;
                ++ebx14;
            }
            if (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax10) == 34)) {
                if (!(*reinterpret_cast<unsigned char*>(&ebx14) & 1)) {
                    if (!v12 || !reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax10 + 1) == 34)) {
                        v13 = 0;
                    } else {
                        ++eax10;
                    }
                    edi9 = a3;
                    edx15 = 0;
                    *reinterpret_cast<unsigned char*>(&edx15) = reinterpret_cast<uint1_t>(v12 == 0);
                    v12 = edx15;
                }
                ebx14 = ebx14 >> 1;
            }
            if (ebx14) {
                ebx16 = ebx14 - 1 + 1;
                do {
                    if (esi8) {
                        *reinterpret_cast<void***>(esi8) = reinterpret_cast<void**>(92);
                        ++esi8;
                    }
                    *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
                    --ebx16;
                } while (ebx16);
            }
            if (!*reinterpret_cast<void***>(eax10)) 
                break;
            if (v12) 
                goto addr_4023a4_50;
            if (*reinterpret_cast<void***>(eax10) == 32) 
                break;
            if (*reinterpret_cast<void***>(eax10) == 9) 
                break;
            addr_4023a4_50:
            if (v13) {
                if (!esi8) {
                    if (*reinterpret_cast<unsigned char*>(*reinterpret_cast<void***>(eax10) + 0x409861) & 4) {
                        ++eax10;
                        *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
                    }
                } else {
                    if (*reinterpret_cast<unsigned char*>(*reinterpret_cast<void***>(eax10) + 0x409861) & 4) {
                        *reinterpret_cast<void***>(esi8) = *reinterpret_cast<void***>(eax10);
                        ++esi8;
                        ++eax10;
                        *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
                    }
                    *reinterpret_cast<void***>(esi8) = *reinterpret_cast<void***>(eax10);
                    ++esi8;
                }
                *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
            }
            ++eax10;
        }
        if (esi8) {
            *reinterpret_cast<void***>(esi8) = reinterpret_cast<void**>(0);
            ++esi8;
        }
        *reinterpret_cast<void***>(ecx7) = *reinterpret_cast<void***>(ecx7) + 1;
    }
    if (edi9) {
        *reinterpret_cast<void***>(edi9) = reinterpret_cast<void**>(0);
    }
    *reinterpret_cast<void***>(a5) = *reinterpret_cast<void***>(a5) + 1;
    return;
    addr_4022b5_23:
    if (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(eax10) == 34)) {
        ++eax10;
        goto addr_402300_25;
    }
    addr_4022b1_24:
    *reinterpret_cast<void***>(esi8) = reinterpret_cast<void**>(0);
    ++esi8;
    goto addr_4022b5_23;
}

struct s27 {
    unsigned char f0;
    signed char f1;
};

int32_t GetModuleHandleA = 0x67ea;

struct s28 {
    int16_t f0;
    signed char[58] pad60;
    int32_t f3c;
};

struct s29 {
    signed char[26] pad26;
    unsigned char f1a;
    signed char f1b;
};

void* fun_4026db(void** ecx, struct s27* a2, int32_t a3, void* a4, int32_t a5) {
    struct s28* eax6;
    struct s29* eax7;
    int32_t esi8;

    a2->f0 = reinterpret_cast<unsigned char>(0);
    eax6 = reinterpret_cast<struct s28*>(GetModuleHandleA());
    if (eax6->f0 == 0x5a4d && eax6->f3c) {
        eax7 = reinterpret_cast<struct s29*>(reinterpret_cast<int32_t>(eax6) + eax6->f3c);
        a2->f0 = eax7->f1a;
        a2->f1 = eax7->f1b;
    }
    goto esi8;
}

int32_t fun_404a80(signed char* a1, signed char* a2, int32_t a3, int32_t a4, void* a5, int32_t a6) {
    int32_t ecx7;
    int32_t ebx8;
    signed char* edi9;
    signed char* esi10;
    int32_t ecx11;
    signed char* edi12;
    signed char* esi13;

    ecx7 = a3;
    if (ecx7) 
        goto addr_404ab1_2;
    ebx8 = ecx7;
    edi9 = a1;
    esi10 = edi9;
    do {
        if (!ecx7) 
            break;
        --ecx7;
        ++edi9;
        ++esi10;
    } while (*edi9);
    ecx11 = -ecx7 + ebx8;
    edi12 = esi10;
    esi13 = a2;
    do {
        if (!ecx11) 
            break;
        --ecx11;
        ++edi12;
        ++esi13;
    } while (*esi13 == *edi12);
    ecx7 = 0;
    if (*reinterpret_cast<unsigned char*>(esi13 - 1) <= *reinterpret_cast<unsigned char*>(edi12 - 1)) 
        goto addr_404aab_10;
    addr_404aaf_11:
    ecx7 = ~ecx7;
    goto addr_404ab1_2;
    addr_404aab_10:
    if (*reinterpret_cast<unsigned char*>(esi13 - 1) == *reinterpret_cast<unsigned char*>(edi12 - 1)) {
        addr_404ab1_2:
        return ecx7;
    } else {
        ecx7 = -2;
        goto addr_404aaf_11;
    }
}

void** fun_404a00(void** a1, void** a2, int32_t a3, void* a4, int32_t a5, int32_t a6, void* a7, int32_t a8) {
    void** dl9;
    void** edi10;
    void** dh11;
    uint32_t eax12;
    uint32_t ebx13;
    uint32_t eax14;
    void** edx15;
    uint32_t ebx16;
    uint32_t ebx17;
    uint32_t ecx18;
    void* esi19;
    void** eax20;
    uint32_t eax21;
    uint32_t eax22;
    void** al23;
    void** esi24;
    void** ecx25;

    dl9 = *reinterpret_cast<void***>(a2);
    edi10 = a1;
    if (!dl9) {
        return edi10;
    }
    dh11 = *reinterpret_cast<void***>(a2 + 1);
    if (dh11) 
        goto addr_404a18_4;
    eax12 = 0;
    *reinterpret_cast<void***>(&eax12) = dl9;
    ebx13 = eax12;
    eax14 = eax12 << 8;
    edx15 = a1;
    if (reinterpret_cast<unsigned char>(edx15) & 3) 
        goto addr_404958_7;
    addr_40496b_8:
    ebx16 = ebx13 | eax14;
    ebx17 = ebx16 << 16 | ebx16;
    while (1) {
        ecx18 = reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edx15)) ^ ebx17;
        esi19 = reinterpret_cast<void*>(*reinterpret_cast<void***>(edx15) + 0x7efefeff);
        edx15 = edx15 + 4;
        if ((ecx18 ^ 0xffffffff ^ 0x7efefeff + ecx18) & 0x81010100) {
            eax20 = *reinterpret_cast<void***>(edx15 + 0xfffffffc);
            if (*reinterpret_cast<signed char*>(&eax20) == *reinterpret_cast<signed char*>(&ebx17)) 
                break;
            if (!*reinterpret_cast<signed char*>(&eax20)) 
                goto addr_4049b2_12;
            if (*reinterpret_cast<signed char*>(&eax20 + 1) == *reinterpret_cast<signed char*>(&ebx17)) 
                goto addr_4049ee_14;
            if (!*reinterpret_cast<signed char*>(&eax20 + 1)) 
                goto addr_4049b2_12;
            eax21 = reinterpret_cast<unsigned char>(eax20) >> 16;
            if (*reinterpret_cast<signed char*>(&eax21) == *reinterpret_cast<signed char*>(&ebx17)) 
                goto addr_4049e7_17;
            if (!*reinterpret_cast<signed char*>(&eax21)) 
                goto addr_4049b2_12;
            if (*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax21) + 1) == *reinterpret_cast<signed char*>(&ebx17)) 
                goto addr_4049e0_20;
            if (!*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax21) + 1)) 
                goto addr_4049b2_12;
        } else {
            eax22 = (reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edx15)) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(esi19)) & 0x81010100;
            if (!eax22) 
                continue;
            if (eax22 & 0x1010100) 
                goto addr_4049b2_12;
            if (!(reinterpret_cast<uint32_t>(esi19) & 0x80000000)) 
                goto addr_4049b2_12;
        }
    }
    return edx15 + 0xfffffffc;
    addr_4049b2_12:
    addr_4049b4_27:
    return 0;
    addr_4049ee_14:
    return edx15 + 0xfffffffd;
    addr_4049e7_17:
    return edx15 + 0xfffffffe;
    addr_4049e0_20:
    return edx15 + 0xffffffff;
    do {
        addr_404958_7:
        ++edx15;
        if (*reinterpret_cast<void***>(edx15) == *reinterpret_cast<void***>(&ebx13)) 
            break;
        if (!*reinterpret_cast<void***>(edx15)) 
            goto addr_4049b4_27;
    } while (reinterpret_cast<unsigned char>(edx15) & 3);
    goto addr_40496b_8;
    return edx15 + 0xffffffff;
    addr_404a34_31:
    return 0;
    addr_404a73_32:
    return edi10 + 0xffffffff;
    while (1) {
        addr_404a2c_33:
        if (al23 == dl9) {
            do {
                al23 = *reinterpret_cast<void***>(esi24);
                ++esi24;
                if (al23 != dh11) 
                    goto addr_404a2c_33;
                edi10 = esi24 + 0xffffffff;
                do {
                    if (!*reinterpret_cast<void***>(ecx25 + 2)) 
                        goto addr_404a73_32;
                    esi24 = esi24 + 2;
                    if (*reinterpret_cast<void***>(esi24) != *reinterpret_cast<void***>(ecx25 + 2)) 
                        goto addr_404a18_4;
                    if (!*reinterpret_cast<void***>(ecx25 + 3)) 
                        goto addr_404a73_32;
                    ecx25 = ecx25 + 2;
                } while (*reinterpret_cast<void***>(ecx25 + 3) == *reinterpret_cast<void***>(esi24 + 0xffffffff));
                addr_404a18_4:
                ecx25 = a2;
                esi24 = edi10 + 1;
            } while (*reinterpret_cast<void***>(edi10) == dl9);
            if (!*reinterpret_cast<void***>(edi10)) 
                goto addr_404a34_31;
        } else {
            if (!al23) 
                goto addr_404a34_31;
        }
        al23 = *reinterpret_cast<void***>(esi24);
        ++esi24;
    }
}

void** fun_404940(void** ecx, void** a2, signed char a3, int32_t a4, void* a5, int32_t a6) {
    uint32_t eax7;
    uint32_t ebx8;
    uint32_t eax9;
    void** edx10;
    uint32_t ebx11;
    uint32_t ebx12;
    uint32_t ecx13;
    void* esi14;
    void** eax15;
    uint32_t eax16;
    uint32_t eax17;

    eax7 = 0;
    *reinterpret_cast<signed char*>(&eax7) = a3;
    ebx8 = eax7;
    eax9 = eax7 << 8;
    edx10 = a2;
    if (!(reinterpret_cast<unsigned char>(edx10) & 3)) {
        addr_40496b_3:
        ebx11 = ebx8 | eax9;
        ebx12 = ebx11 << 16 | ebx11;
    } else {
        do {
            ++edx10;
            if (*reinterpret_cast<void***>(edx10) == *reinterpret_cast<void***>(&ebx8)) 
                goto addr_404930_5;
            if (!*reinterpret_cast<void***>(edx10)) 
                goto addr_4049b4_7;
        } while (reinterpret_cast<unsigned char>(edx10) & 3);
        goto addr_40496b_3;
    }
    while (1) {
        ecx13 = reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edx10)) ^ ebx12;
        esi14 = reinterpret_cast<void*>(*reinterpret_cast<void***>(edx10) + 0x7efefeff);
        edx10 = edx10 + 4;
        if ((ecx13 ^ 0xffffffff ^ 0x7efefeff + ecx13) & 0x81010100) {
            eax15 = *reinterpret_cast<void***>(edx10 + 0xfffffffc);
            if (*reinterpret_cast<signed char*>(&eax15) == *reinterpret_cast<signed char*>(&ebx12)) 
                break;
            if (!*reinterpret_cast<signed char*>(&eax15)) 
                goto addr_4049b2_12;
            if (*reinterpret_cast<signed char*>(&eax15 + 1) == *reinterpret_cast<signed char*>(&ebx12)) 
                goto addr_4049ee_14;
            if (!*reinterpret_cast<signed char*>(&eax15 + 1)) 
                goto addr_4049b2_12;
            eax16 = reinterpret_cast<unsigned char>(eax15) >> 16;
            if (*reinterpret_cast<signed char*>(&eax16) == *reinterpret_cast<signed char*>(&ebx12)) 
                goto addr_4049e7_17;
            if (!*reinterpret_cast<signed char*>(&eax16)) 
                goto addr_4049b2_12;
            if (*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax16) + 1) == *reinterpret_cast<signed char*>(&ebx12)) 
                goto addr_4049e0_20;
            if (!*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax16) + 1)) 
                goto addr_4049b2_12;
        } else {
            eax17 = (reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(edx10)) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(esi14)) & 0x81010100;
            if (!eax17) 
                continue;
            if (eax17 & 0x1010100) 
                goto addr_4049b2_12;
            if (!(reinterpret_cast<uint32_t>(esi14) & 0x80000000)) 
                goto addr_4049b2_12;
        }
    }
    return edx10 + 0xfffffffc;
    addr_4049b2_12:
    addr_4049b4_7:
    return 0;
    addr_4049ee_14:
    return edx10 + 0xfffffffd;
    addr_4049e7_17:
    return edx10 + 0xfffffffe;
    addr_4049e0_20:
    return edx10 + 0xffffffff;
    addr_404930_5:
    return edx10 + 0xffffffff;
}

void*** fun_404722(void** a1, void** a2, void** a3, uint32_t a4);

void*** fun_40470b(void** a1, void** a2, int32_t a3, int32_t a4, void* a5, int32_t a6) {
    void*** eax7;

    eax7 = fun_404722(__return_address(), a1, a2, 0);
    return eax7;
}

void fun_404ac0(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13, void** a14, void** a15, void** a16, void** a17, void** a18, void** a19, void** a20);

int32_t GetVersionExA = 0x6990;

int32_t GetEnvironmentVariableA = 0x6976;

void*** fun_402708(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13, void** a14, void** a15, void** a16, void** a17, void** a18) {
    void*** ebp19;
    void** ebp20;
    int32_t v21;
    int32_t ebx22;
    int32_t eax23;
    int32_t v24;
    uint32_t v25;
    void* v26;
    int32_t eax27;
    void* eax28;
    void*** eax29;
    void** v30;
    void** v31;
    void** v32;
    int32_t eax33;
    void* v34;
    void** v35;
    void** v36;
    void** v37;
    void** v38;
    void** eax39;
    void** eax40;
    void** eax41;

    ebp19 = reinterpret_cast<void***>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    fun_404ac0(ecx, ebp20, __return_address(), a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18);
    v21 = ebx22;
    eax23 = reinterpret_cast<int32_t>(GetVersionExA());
    if (!eax23 || (v24 != 2 || v25 < 5)) {
        v26 = reinterpret_cast<void*>(ebp19 - 0x122c);
        eax27 = reinterpret_cast<int32_t>(GetEnvironmentVariableA("__MSVCRT_HEAP_SELECT", v26, 0x1090));
        if (!eax27) {
            addr_40283a_3:
            eax28 = fun_4026db(ecx, ebp19 - 4, "__MSVCRT_HEAP_SELECT", v26, 0x1090);
            eax29 = reinterpret_cast<void***>(reinterpret_cast<uint32_t>(eax28) - (reinterpret_cast<uint32_t>(eax28) + reinterpret_cast<uint1_t>(reinterpret_cast<uint32_t>(eax28) < reinterpret_cast<uint32_t>(eax28) + reinterpret_cast<uint1_t>(*reinterpret_cast<unsigned char*>(&v21) < 6))) + 3);
        } else {
            ecx = reinterpret_cast<void**>(ebp19 + 0xffffedd4);
            if (v30) {
                do {
                    if (reinterpret_cast<signed char>(v31) >= reinterpret_cast<signed char>(97) && reinterpret_cast<signed char>(v31) <= reinterpret_cast<signed char>(0x7a)) {
                    }
                    ++ecx;
                } while (v32);
            }
            eax33 = fun_404a80("__GLOBAL_HEAP_SELECTED", ebp19 - 0x122c, 22, "__MSVCRT_HEAP_SELECT", v26, 0x1090);
            if (eax33) {
                v34 = reinterpret_cast<void*>(ebp19 - 0x19c);
                GetModuleFileNameA(0, v34, 0x104, "__MSVCRT_HEAP_SELECT", v26, 0x1090);
                if (v35) {
                    do {
                        if (reinterpret_cast<signed char>(v36) >= reinterpret_cast<signed char>(97) && reinterpret_cast<signed char>(v36) <= reinterpret_cast<signed char>(0x7a)) {
                        }
                    } while (v37);
                }
                v38 = reinterpret_cast<void**>(ebp19 + 0xfffffe64);
                eax39 = fun_404a00(ebp19 + 0xffffedd4, v38, 0, v34, 0x104, "__MSVCRT_HEAP_SELECT", v26, 0x1090);
                ecx = v38;
            } else {
                eax39 = reinterpret_cast<void**>(ebp19 + 0xffffedd4);
            }
            if (!eax39) 
                goto addr_40283a_3;
            eax40 = fun_404940(ecx, eax39, 44, "__MSVCRT_HEAP_SELECT", v26, 0x1090);
            ecx = reinterpret_cast<void**>(44);
            if (!eax40) 
                goto addr_40283a_3; else 
                goto addr_40280a_17;
        }
    } else {
        eax29 = reinterpret_cast<void***>(1);
    }
    addr_40284d_19:
    return eax29;
    addr_40280a_17:
    eax41 = eax40 + 1;
    ecx = eax41;
    if (*reinterpret_cast<void***>(eax41)) {
        do {
            if (!reinterpret_cast<int1_t>(*reinterpret_cast<void***>(ecx) == 59)) {
                ++ecx;
            } else {
                *reinterpret_cast<void***>(ecx) = reinterpret_cast<void**>(0);
            }
        } while (*reinterpret_cast<void***>(ecx));
    }
    eax29 = fun_40470b(eax41, 0, 10, "__MSVCRT_HEAP_SELECT", v26, 0x1090);
    if (eax29 == 2) 
        goto addr_40284d_19;
    if (eax29 == 3) 
        goto addr_40284d_19;
    if (eax29 == 1) 
        goto addr_40284d_19; else 
        goto addr_40283a_3;
}

uint32_t g409968 = 0;

void** fun_402c0c() {
    void** v1;
    struct s8* eax2;

    v1 = g409984;
    eax2 = reinterpret_cast<struct s8*>(HeapAlloc());
    g40997c = eax2;
    if (eax2) {
        g409974 = reinterpret_cast<struct s8*>(0);
        g409978 = 0;
        g409970 = eax2;
        g409980 = reinterpret_cast<void**>(0);
        g409968 = 16;
        goto v1;
    } else {
        goto v1;
    }
}

uint32_t g407218 = 0xffffffff;

void** g40720c = reinterpret_cast<void**>(8);

void** g407208 = reinterpret_cast<void**>(8);

struct s30 {
    signed char[8] pad8;
    int32_t f8;
    signed char[56] pad68;
    int32_t f44;
    signed char[16] pad88;
    int32_t f58;
    signed char[12] pad104;
    int32_t f68;
    int32_t f6c;
};

void** fun_403753() {
    int1_t zf1;
    int32_t v2;
    int32_t esi3;
    void** v4;
    void** eax5;
    void** esi6;
    int32_t ebp7;
    void** eax8;
    void** edi9;
    void** v10;
    void** v11;
    int32_t eax12;
    void** eax13;
    int1_t zf14;
    int1_t zf15;
    void** eax16;
    int32_t ebp17;
    int32_t edx18;

    zf1 = g407218 == 0xffffffff;
    v2 = esi3;
    if (!zf1) {
        v4 = g409984;
        eax5 = reinterpret_cast<void**>(HeapAlloc());
        esi6 = eax5;
        if (!esi6) {
            addr_403890_3:
        } else {
            addr_403784_4:
            ebp7 = VirtualAlloc;
            v4 = reinterpret_cast<void**>(0x400000);
            eax8 = reinterpret_cast<void**>(ebp7(0));
            edi9 = eax8;
            if (!edi9) {
                addr_403879_5:
                if (esi6 != 0x407208) {
                    v10 = g409984;
                    HeapFree(v10, 0, esi6, 0);
                    goto addr_403890_3;
                }
            } else {
                v11 = edi9;
                eax12 = reinterpret_cast<int32_t>(ebp7(v11, 0x10000, 0x1000, 4, 0));
                if (!eax12) {
                    VirtualFree(edi9, 0, 0x8000, v11, 0x10000, 0x1000, 4, 0);
                    goto addr_403879_5;
                } else {
                    if (!reinterpret_cast<int1_t>(esi6 == 0x407208)) {
                        *reinterpret_cast<void***>(esi6) = reinterpret_cast<void**>(0x407208);
                        eax13 = g40720c;
                        *reinterpret_cast<void***>(esi6 + 4) = eax13;
                        g40720c = esi6;
                        *reinterpret_cast<void***>(*reinterpret_cast<void***>(esi6 + 4)) = esi6;
                    } else {
                        zf14 = g407208 == 0;
                        if (zf14) {
                            g407208 = reinterpret_cast<void**>(0x407208);
                        }
                        zf15 = g40720c == 0;
                        if (zf15) {
                            g40720c = reinterpret_cast<void**>(0x407208);
                        }
                    }
                    *reinterpret_cast<struct s30***>(esi6 + 20) = reinterpret_cast<struct s30**>(edi9 + 0x400000);
                    eax16 = esi6 + 24;
                    *reinterpret_cast<void***>(esi6 + 12) = esi6 + 0x98;
                    *reinterpret_cast<void***>(esi6 + 16) = edi9;
                    *reinterpret_cast<void***>(esi6 + 8) = eax16;
                    ebp17 = 0;
                    do {
                        edx18 = 0;
                        *reinterpret_cast<unsigned char*>(&edx18) = reinterpret_cast<uint1_t>(ebp17 >= 16);
                        ++ebp17;
                        *reinterpret_cast<void***>(eax16) = reinterpret_cast<void**>((reinterpret_cast<uint32_t>(edx18 - 1) & reinterpret_cast<unsigned char>(0xf1)) - 1);
                        *reinterpret_cast<void***>(eax16 + 4) = reinterpret_cast<void**>(0xf1);
                        eax16 = eax16 + 8;
                    } while (ebp17 < 0x400);
                    fun_404fc0(edi9, 0, 0x10000, v11, 0x10000, 0x1000, 4, 0, v4);
                    while (reinterpret_cast<unsigned char>(edi9) < reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi6 + 16) + 0x10000)) {
                        *reinterpret_cast<void***>(edi9 + 0xf8) = reinterpret_cast<void**>(0xff);
                        *reinterpret_cast<void***>(edi9) = edi9 + 8;
                        *reinterpret_cast<void***>(edi9 + 4) = reinterpret_cast<void**>(0xf0);
                        edi9 = edi9 + 0x1000;
                    }
                }
            }
        }
        goto v2;
    } else {
        esi6 = reinterpret_cast<void**>(0x407208);
        goto addr_403784_4;
    }
}

struct s31 {
    signed char[4] pad4;
    int32_t f4;
    int32_t* f8;
    int32_t fc;
};

void** g0;

void fun_402986(int32_t ecx, int32_t a2);

void fun_4028f2(struct s31* a1, int32_t a2) {
    void** v3;
    int32_t* ebx4;
    int32_t esi5;
    int32_t ecx6;

    v3 = g0;
    g0 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 - 4 - 4 - 4 - 4 - 4 - 4);
    while ((ebx4 = a1->f8, a1->fc != -1) && a1->fc != a2) {
        esi5 = a1->fc + a1->fc * 2;
        ecx6 = ebx4[esi5];
        a1->fc = ecx6;
        if (!(ebx4 + esi5)[1]) {
            fun_402986(ecx6, 0x101);
            (ebx4 + esi5)[2]();
        }
    }
    g0 = v3;
    return;
}

int32_t g40716c = 0;

struct s32 {
    signed char[8] pad8;
    int32_t f8;
};

int32_t g407168 = 0;

int32_t g407170 = 0;

void fun_402986(int32_t ecx, int32_t a2) {
    struct s32* ebp3;
    int32_t eax4;
    int32_t ebp5;

    g40716c = ebp3->f8;
    g407168 = eax4;
    g407170 = ebp5;
    return;
}

void fun_4054f2();

void fun_4028b0(struct s31* a1) {
    fun_4054f2();
    goto a1->f4;
}

void** fun_4045a0(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7) {
    void** edi8;
    void** ecx9;
    void** edx10;

    edi8 = a2;
    ecx9 = a3;
    if (!(reinterpret_cast<unsigned char>(ecx9) & 3)) {
        while (1) {
            addr_404636_3:
            edx10 = *reinterpret_cast<void***>(ecx9);
            ecx9 = ecx9 + 4;
            if ((reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ecx9)) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(*reinterpret_cast<void***>(ecx9) + 0x7efefeff)) & 0x81010100) {
                if (!edx10) 
                    break;
                if (!*reinterpret_cast<signed char*>(&edx10 + 1)) 
                    goto addr_40467f_6;
                if (!(reinterpret_cast<unsigned char>(edx10) & 0xff0000)) 
                    goto addr_404672_8;
                if (!(reinterpret_cast<unsigned char>(edx10) & 0xff000000)) 
                    goto addr_40466a_10;
            }
            *reinterpret_cast<void***>(edi8) = edx10;
            edi8 = edi8 + 4;
        }
    } else {
        do {
            edx10 = *reinterpret_cast<void***>(ecx9);
            ++ecx9;
            if (!edx10) 
                break;
            *reinterpret_cast<void***>(edi8) = edx10;
            ++edi8;
        } while (reinterpret_cast<unsigned char>(ecx9) & 3);
        goto addr_40462f_15;
    }
    *reinterpret_cast<void***>(edi8) = edx10;
    return a2;
    addr_40467f_6:
    *reinterpret_cast<void***>(edi8) = edx10;
    return a2;
    addr_404672_8:
    *reinterpret_cast<void***>(edi8) = edx10;
    *reinterpret_cast<void***>(edi8 + 2) = reinterpret_cast<void**>(0);
    return a2;
    addr_40466a_10:
    *reinterpret_cast<void***>(edi8) = edx10;
    return a2;
    addr_40462f_15:
    goto addr_404636_3;
}

void** fun_404b80(void** ecx, void** a2, void*** a3, uint32_t a4, void** a5) {
    uint32_t ecx6;
    uint32_t ebx7;
    void*** esi8;
    void** edi9;
    uint32_t ecx10;
    void** eax11;
    uint32_t ecx12;
    void** edx13;

    ecx6 = a4;
    if (!ecx6) {
        addr_404c03_2:
        return a2;
    } else {
        ebx7 = ecx6;
        esi8 = a3;
        edi9 = a2;
        if (!(reinterpret_cast<uint32_t>(esi8) & 3)) {
            ecx10 = ecx6 >> 2;
            if (!ecx10) {
                goto addr_404bc5_6;
            }
        }
        do {
            eax11 = *esi8;
            ++esi8;
            *reinterpret_cast<void***>(edi9) = eax11;
            ++edi9;
            --ecx6;
            if (!ecx6) 
                goto addr_404bd2_8;
            if (!eax11) 
                break;
        } while (reinterpret_cast<uint32_t>(esi8) & 3);
        goto addr_404bb9_11;
    }
    if (reinterpret_cast<unsigned char>(edi9) & 3) {
        do {
            *reinterpret_cast<void***>(edi9) = eax11;
            ++edi9;
            --ecx6;
            if (!ecx6) 
                goto addr_404c76_14;
        } while (reinterpret_cast<unsigned char>(edi9) & 3);
    }
    ebx7 = ecx6;
    ecx12 = ecx6 >> 2;
    if (ecx12) 
        goto addr_404c67_17; else 
        goto addr_404bfb_18;
    addr_404bb9_11:
    ebx7 = ecx6;
    ecx10 = ecx6 >> 2;
    if (ecx10) {
        do {
            edx13 = *esi8;
            esi8 = esi8 + 4;
            if ((reinterpret_cast<unsigned char>(*esi8) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(*esi8 + 0x7efefeff)) & 0x81010100) {
                if (!*reinterpret_cast<signed char*>(&edx13)) 
                    break;
                if (!*reinterpret_cast<signed char*>(&edx13 + 1)) 
                    goto addr_404c51_22;
                if (!(reinterpret_cast<unsigned char>(edx13) & 0xff0000)) 
                    goto addr_404c47_24;
                if (!(reinterpret_cast<unsigned char>(edx13) & 0xff000000)) 
                    goto addr_404c43_26;
            }
            *reinterpret_cast<void***>(edi9) = edx13;
            edi9 = edi9 + 4;
            --ecx10;
        } while (ecx10);
        goto addr_404bc0_28;
    } else {
        addr_404bc0_28:
        ebx7 = ebx7 & 3;
        if (!ebx7) {
            addr_404bd2_8:
            return a2;
        } else {
            do {
                addr_404bc5_6:
                eax11 = *esi8;
                ++esi8;
                *reinterpret_cast<void***>(edi9) = eax11;
                ++edi9;
                if (!eax11) 
                    goto addr_404bfe_29;
                --ebx7;
            } while (ebx7);
            goto addr_404bd2_8;
        }
    }
    *reinterpret_cast<void***>(edi9) = reinterpret_cast<void**>(0);
    addr_404c5f_32:
    edi9 = edi9 + 4;
    eax11 = reinterpret_cast<void**>(0);
    ecx12 = ecx10 - 1;
    if (!ecx12) {
        addr_404c71_33:
        ebx7 = ebx7 & 3;
        if (ebx7) {
            do {
                addr_404bfb_18:
                *reinterpret_cast<void***>(edi9) = eax11;
                ++edi9;
                addr_404bfe_29:
                --ebx7;
            } while (ebx7);
        } else {
            addr_404c76_14:
            return a2;
        }
    } else {
        addr_404c67_17:
        eax11 = reinterpret_cast<void**>(0);
        goto addr_404c69_34;
    }
    goto addr_404c03_2;
    do {
        addr_404c69_34:
        *reinterpret_cast<void***>(edi9) = reinterpret_cast<void**>(0);
        edi9 = edi9 + 4;
        --ecx12;
    } while (ecx12);
    goto addr_404c71_33;
    addr_404c51_22:
    *reinterpret_cast<void***>(edi9) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(edx13) & 0xff);
    goto addr_404c5f_32;
    addr_404c47_24:
    *reinterpret_cast<void***>(edi9) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(edx13) & 0xffff);
    goto addr_404c5f_32;
    addr_404c43_26:
    *reinterpret_cast<void***>(edi9) = edx13;
    goto addr_404c5f_32;
}

void** fun_4045b0(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13) {
    void** ecx14;
    void** eax15;
    void** edi16;
    void** ecx17;
    void** edx18;

    ecx14 = a2;
    if (!(reinterpret_cast<unsigned char>(ecx14) & 3)) {
        while (1) {
            addr_4045cc_2:
            ecx14 = ecx14 + 4;
            if (!((reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ecx14)) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(*reinterpret_cast<void***>(ecx14) + 0x7efefeff)) & 0x81010100)) 
                continue;
            eax15 = *reinterpret_cast<void***>(ecx14 + 0xfffffffc);
            if (!*reinterpret_cast<signed char*>(&eax15)) 
                break;
            if (!*reinterpret_cast<signed char*>(&eax15 + 1)) 
                goto addr_404609_5;
            if (!(reinterpret_cast<unsigned char>(eax15) & 0xff0000)) 
                goto addr_404604_7;
            if (!(reinterpret_cast<unsigned char>(eax15) & 0xff000000)) 
                goto addr_4045ff_9;
        }
    } else {
        do {
            ++ecx14;
            if (!*reinterpret_cast<void***>(ecx14)) 
                goto addr_4045ff_9;
        } while (reinterpret_cast<unsigned char>(ecx14) & 3);
        goto addr_4045cc_2;
    }
    edi16 = ecx14 + 0xfffffffc;
    addr_404611_14:
    ecx17 = a3;
    if (!(reinterpret_cast<unsigned char>(ecx17) & 3)) {
        while (1) {
            addr_404636_15:
            edx18 = *reinterpret_cast<void***>(ecx17);
            ecx17 = ecx17 + 4;
            if ((reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ecx17)) ^ 0xffffffff ^ reinterpret_cast<uint32_t>(*reinterpret_cast<void***>(ecx17) + 0x7efefeff)) & 0x81010100) {
                if (!edx18) 
                    break;
                if (!*reinterpret_cast<signed char*>(&edx18 + 1)) 
                    goto addr_40467f_18;
                if (!(reinterpret_cast<unsigned char>(edx18) & 0xff0000)) 
                    goto addr_404672_20;
                if (!(reinterpret_cast<unsigned char>(edx18) & 0xff000000)) 
                    goto addr_40466a_22;
            }
            *reinterpret_cast<void***>(edi16) = edx18;
            edi16 = edi16 + 4;
        }
    } else {
        do {
            edx18 = *reinterpret_cast<void***>(ecx17);
            ++ecx17;
            if (!edx18) 
                break;
            *reinterpret_cast<void***>(edi16) = edx18;
            ++edi16;
        } while (reinterpret_cast<unsigned char>(ecx17) & 3);
        goto addr_40462f_27;
    }
    *reinterpret_cast<void***>(edi16) = edx18;
    return a2;
    addr_40467f_18:
    *reinterpret_cast<void***>(edi16) = edx18;
    return a2;
    addr_404672_20:
    *reinterpret_cast<void***>(edi16) = edx18;
    *reinterpret_cast<void***>(edi16 + 2) = reinterpret_cast<void**>(0);
    return a2;
    addr_40466a_22:
    *reinterpret_cast<void***>(edi16) = edx18;
    return a2;
    addr_40462f_27:
    goto addr_404636_15;
    addr_404609_5:
    edi16 = ecx14 + 0xfffffffd;
    goto addr_404611_14;
    addr_404604_7:
    edi16 = ecx14 + 0xfffffffe;
    goto addr_404611_14;
    addr_4045ff_9:
    edi16 = ecx14 + 0xffffffff;
    goto addr_404611_14;
}

int32_t g409714 = 0;

int32_t g409718 = 0;

int32_t g40971c = 0;

int32_t LoadLibraryA = 0x6a26;

int32_t GetProcAddress = 0x6a14;

void fun_404aef(void** ecx, void* a2, int32_t a3, int32_t a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12) {
    int32_t ebx13;
    int1_t zf14;
    int32_t eax15;
    int32_t eax16;
    int32_t eax17;
    int32_t eax18;
    int32_t eax19;
    int32_t esi20;
    int32_t eax21;
    int32_t eax22;
    int32_t eax23;
    int32_t ebx24;

    ebx13 = 0;
    zf14 = g409714 == 0;
    if (!zf14) {
        addr_404b3e_2:
        eax15 = g409718;
        if (eax15 && ((eax16 = reinterpret_cast<int32_t>(eax15()), ebx13 = eax16, !!ebx13) && (eax17 = g40971c, !!eax17))) {
            eax18 = reinterpret_cast<int32_t>(eax17(ebx13));
            ebx13 = eax18;
        }
    } else {
        eax19 = reinterpret_cast<int32_t>(LoadLibraryA());
        if (!eax19 || (esi20 = GetProcAddress, eax21 = reinterpret_cast<int32_t>(esi20(eax19, "MessageBoxA")), g409714 = eax21, eax21 == 0)) {
            goto addr_404b70_6;
        } else {
            eax22 = reinterpret_cast<int32_t>(esi20(eax19, "GetActiveWindow", eax19, "MessageBoxA"));
            g409718 = eax22;
            eax23 = reinterpret_cast<int32_t>(esi20(eax19, "GetLastActivePopup", eax19, "GetActiveWindow", eax19, "MessageBoxA"));
            g40971c = eax23;
            goto addr_404b3e_2;
        }
    }
    g409714(ebx13, __return_address(), a2);
    addr_404b70_6:
    goto ebx24;
}

struct s33 {
    struct s8* f0;
    signed char f1;
    signed char f2;
    signed char f3;
};

struct s8* fun_404c80(struct s8* a1, struct s8* a2, void* a3, void** a4, int32_t a5, struct s9* a6, void* a7, int32_t a8, int32_t a9, void* a10, int32_t a11, int32_t a12) {
    struct s8* esi13;
    struct s8* edi14;
    struct s8* eax15;
    uint32_t ecx16;
    uint32_t edx17;
    struct s33* esi18;
    struct s8* edi19;
    uint32_t ecx20;
    uint32_t edx21;

    esi13 = a2;
    edi14 = a1;
    eax15 = reinterpret_cast<struct s8*>(reinterpret_cast<uint32_t>(a3) + reinterpret_cast<unsigned char>(esi13));
    if (reinterpret_cast<unsigned char>(edi14) <= reinterpret_cast<unsigned char>(esi13) || reinterpret_cast<unsigned char>(edi14) >= reinterpret_cast<unsigned char>(eax15)) {
        if (reinterpret_cast<unsigned char>(edi14) & 3) {
            if (reinterpret_cast<uint32_t>(a3) < 4) {
                goto *reinterpret_cast<int32_t*>((reinterpret_cast<uint32_t>(a3) - 4) * 4 + 0x404dd8);
            } else {
                goto *reinterpret_cast<int32_t*>("M@" + (reinterpret_cast<unsigned char>(edi14) & 3) * 4);
            }
        }
        ecx16 = reinterpret_cast<uint32_t>(a3) >> 2;
        edx17 = reinterpret_cast<uint32_t>(a3) & 3;
        if (ecx16 >= 8) 
            goto addr_404cb3_7;
    } else {
        esi18 = reinterpret_cast<struct s33*>(reinterpret_cast<uint32_t>(a3) + reinterpret_cast<unsigned char>(esi13) + 0xfffffffc);
        edi19 = reinterpret_cast<struct s8*>(reinterpret_cast<uint32_t>(a3) + reinterpret_cast<unsigned char>(edi14) + 0xfffffffc);
        if (reinterpret_cast<unsigned char>(edi19) & 3) {
            eax15 = edi19;
            if (reinterpret_cast<uint32_t>(a3) >= 4) {
                goto *reinterpret_cast<int32_t*>("O@" + (reinterpret_cast<unsigned char>(eax15) & 3) * 4);
            }
            goto *reinterpret_cast<int32_t*>("pO@" + reinterpret_cast<uint32_t>(a3) * 4);
        } else {
            ecx20 = reinterpret_cast<uint32_t>(a3) >> 2;
            edx21 = reinterpret_cast<uint32_t>(a3) & 3;
            if (ecx20 < 8) {
                goto *reinterpret_cast<int32_t*>("WO@" + reinterpret_cast<int32_t>(-ecx20) * 4);
                goto *reinterpret_cast<int32_t*>("pO@" + edx21 * 4);
            } else {
                while (ecx20) {
                    --ecx20;
                    *reinterpret_cast<struct s8**>(&edi19->f0) = esi18->f0;
                    edi19 = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(edi19) + 0xfffffffc);
                    --esi18;
                }
                goto *reinterpret_cast<int32_t*>("pO@" + edx21 * 4);
            }
        }
    }
    switch (ecx16) {
        addr_404dbf_20:
    case 0:
        switch (edx17) {
        case 0:
            return eax15;
        case 1:
            *reinterpret_cast<struct s8**>(&edi14->f0) = *reinterpret_cast<struct s8**>(&esi13->f0);
            return a1;
        case 2:
            *reinterpret_cast<struct s8**>(&edi14->f0) = *reinterpret_cast<struct s8**>(&esi13->f0);
            edi14->f1 = esi13->f1;
            return a1;
        case 3:
            *reinterpret_cast<struct s8**>(&edi14->f0) = *reinterpret_cast<struct s8**>(&esi13->f0);
            edi14->f1 = esi13->f1;
            edi14->f2 = esi13->f2;
            return a1;
        }
        addr_404dac_25:
    case 1:
        *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(edi14) + ecx16 * 4 - 4) = *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(esi13) + ecx16 * 4 - 4);
        eax15 = reinterpret_cast<struct s8*>(ecx16 * 4);
        esi13 = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(esi13) + reinterpret_cast<unsigned char>(eax15));
        edi14 = reinterpret_cast<struct s8*>(reinterpret_cast<unsigned char>(edi14) + reinterpret_cast<unsigned char>(eax15));
        goto addr_404dbf_20;
        addr_404da4_26:
    case 2:
        *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(edi14) + ecx16 * 4 - 8) = *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(esi13) + ecx16 * 4 - 8);
        goto addr_404dac_25;
        addr_404d9c_27:
    case 3:
        *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(edi14) + ecx16 * 4 - 12) = *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(esi13) + ecx16 * 4 - 12);
        goto addr_404da4_26;
        addr_404d94_28:
    case 4:
        *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(edi14) + ecx16 * 4 - 16) = *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(esi13) + ecx16 * 4 - 16);
        goto addr_404d9c_27;
        addr_404d8c_29:
    case 5:
        *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(edi14) + ecx16 * 4 - 20) = *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(esi13) + ecx16 * 4 - 20);
        goto addr_404d94_28;
    case 6:
        *reinterpret_cast<struct s8**>(reinterpret_cast<unsigned char>(edi14) + ecx16 * 4 - 24) = eax15;
        goto addr_404d8c_29;
    case 7:
    }
    addr_404cb3_7:
    while (ecx16) {
        --ecx16;
        *reinterpret_cast<struct s8**>(&edi14->f0) = *reinterpret_cast<struct s8**>(&esi13->f0);
        edi14 = reinterpret_cast<struct s8*>(&edi14->f4);
        esi13 = reinterpret_cast<struct s8*>(&esi13->f4);
    }
    goto *reinterpret_cast<int32_t*>(edx17 * 4 + 0x404dc8);
    return eax15;
    edi19->f3 = esi18->f3;
    return a1;
    edi19->f3 = esi18->f3;
    edi19->f2 = esi18->f2;
    return a1;
    edi19->f3 = esi18->f3;
    edi19->f2 = esi18->f2;
    edi19->f1 = esi18->f1;
    return a1;
}

struct s34 {
    struct s8* f0;
    signed char[3] pad4;
    struct s8* f4;
    signed char[3] pad8;
    uint32_t f8;
    struct s8* fc;
    signed char[3] pad16;
    struct s8* f10;
};

struct s8* fun_4032b1() {
    uint32_t eax1;
    uint32_t ecx2;
    uint32_t v3;
    void** v4;
    struct s8* eax5;
    uint32_t tmp32_6;
    struct s8* ecx7;
    struct s34* esi8;
    struct s8* eax9;
    struct s8* eax10;
    struct s8* v11;
    void** v12;

    eax1 = g409978;
    ecx2 = g409968;
    if (eax1 == ecx2) {
        v3 = ecx2 + ecx2 * 4 + 80 << 2;
        v4 = g409984;
        eax5 = reinterpret_cast<struct s8*>(HeapReAlloc(v4));
        if (!eax5) 
            goto addr_403344_3;
        tmp32_6 = g409968 + 16;
        g409968 = tmp32_6;
        g40997c = eax5;
        eax1 = g409978;
    }
    ecx7 = g40997c;
    v3 = 0x41c4;
    esi8 = reinterpret_cast<struct s34*>(reinterpret_cast<unsigned char>(ecx7) + (eax1 + eax1 * 4) * 4);
    eax9 = reinterpret_cast<struct s8*>(HeapAlloc());
    esi8->f10 = eax9;
    if (!eax9) {
        addr_403344_3:
    } else {
        eax10 = reinterpret_cast<struct s8*>(VirtualAlloc(0, 0x100000, 0x2000, 4));
        esi8->fc = eax10;
        if (eax10) {
            esi8->f8 = 0xffffffff;
            esi8->f0 = reinterpret_cast<struct s8*>(0);
            esi8->f4 = reinterpret_cast<struct s8*>(0);
            ++g409978;
            *reinterpret_cast<struct s8**>(&esi8->f10->f0) = reinterpret_cast<struct s8*>(0xffffffff);
        } else {
            v11 = esi8->f10;
            v12 = g409984;
            HeapFree(v12, 0, v11, 0, 0x100000, 0x2000, 4);
            goto addr_403344_3;
        }
    }
    goto v3;
}

struct s35 {
    signed char[4] pad4;
    struct s35* f4;
    struct s35* f8;
};

struct s36 {
    signed char[16] pad16;
    uint32_t f10;
};

struct s37 {
    uint32_t f0;
    uint32_t f4;
    signed char[4064] pad4072;
    int32_t ffe8;
    uint32_t ffec;
};

struct s39 {
    signed char[4] pad4;
    struct s38* f4;
    struct s38* f8;
};

struct s38 {
    signed char[4] pad4;
    struct s39* f4;
    struct s39* f8;
};

uint32_t fun_403362(uint32_t ecx, struct s8* a2) {
    struct s8* ecx3;
    struct s9* esi4;
    uint32_t eax5;
    uint32_t ebx6;
    int32_t edx7;
    struct s35* eax8;
    struct s35* v9;
    struct s36* edi10;
    uint32_t eax11;
    struct s36* edx12;
    struct s37* eax13;
    struct s38* ecx14;
    struct s39* eax15;
    struct s38* ecx16;
    int1_t zf17;
    uint32_t ecx18;
    uint32_t eax19;

    ecx3 = a2;
    esi4 = ecx3->f10;
    eax5 = ecx3->f8;
    ebx6 = 0;
    while (reinterpret_cast<int32_t>(eax5) >= reinterpret_cast<int32_t>(0)) {
        eax5 = eax5 << 1;
        ++ebx6;
    }
    edx7 = 63;
    eax8 = reinterpret_cast<struct s35*>(ebx6 * 0x204 + reinterpret_cast<uint32_t>(esi4) + 0x144);
    v9 = eax8;
    do {
        eax8->f8 = eax8;
        eax8->f4 = eax8;
        eax8 = reinterpret_cast<struct s35*>(&eax8->f8);
        --edx7;
    } while (edx7);
    edi10 = reinterpret_cast<struct s36*>((ebx6 << 15) + reinterpret_cast<uint32_t>(ecx3->fc));
    eax11 = reinterpret_cast<uint32_t>(VirtualAlloc());
    if (eax11) {
        edx12 = reinterpret_cast<struct s36*>(reinterpret_cast<uint32_t>(edi10) + 0x7000);
        if (reinterpret_cast<uint32_t>(edi10) <= reinterpret_cast<uint32_t>(edx12)) {
            eax13 = reinterpret_cast<struct s37*>(&edi10->f10);
            do {
                *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(eax13) - 8) = 0xffffffff;
                eax13->ffec = 0xffffffff;
                *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(eax13) - 4) = 0xff0;
                eax13->f0 = reinterpret_cast<uint32_t>(eax13) + 0xffc;
                eax13->f4 = reinterpret_cast<uint32_t>(eax13) - 0x1004;
                eax13->ffe8 = 0xff0;
                eax13 = reinterpret_cast<struct s37*>(reinterpret_cast<uint32_t>(eax13) + 0x1000);
            } while (reinterpret_cast<uint32_t>(eax13) - 16 <= reinterpret_cast<uint32_t>(edx12));
        }
        ecx14 = reinterpret_cast<struct s38*>(reinterpret_cast<uint32_t>(edi10) + 12);
        eax15 = reinterpret_cast<struct s39*>(v9 + 42);
        eax15->f4 = ecx14;
        ecx14->f8 = eax15;
        ecx16 = reinterpret_cast<struct s38*>(reinterpret_cast<uint32_t>(edx12) + 12);
        eax15->f8 = ecx16;
        ecx16->f4 = eax15;
        *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(esi4) + ebx6 * 4 + 68) = 0;
        *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(esi4) + ebx6 * 4 + 0xc4) = 1;
        zf17 = esi4->f43 == 0;
        esi4->f43 = reinterpret_cast<signed char>(esi4->f43 + 1);
        if (zf17) {
            a2->f4 = a2->f4 | 1;
        }
        ecx18 = ebx6;
        a2->f8 = a2->f8 & ~(0x80000000 >> *reinterpret_cast<signed char*>(&ecx18));
        eax19 = ebx6;
    } else {
        eax19 = 0xffffffff;
    }
    return eax19;
}

void** fun_404fc0(void** a1, void** a2, void* a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9) {
    void* edx10;
    void** eax11;
    void** edi12;
    uint32_t ecx13;
    void*** eax14;
    void* ecx15;
    uint32_t ecx16;

    edx10 = a3;
    if (!edx10) {
        return a1;
    }
    eax11 = reinterpret_cast<void**>(0);
    eax11 = a2;
    edi12 = a1;
    if (reinterpret_cast<uint32_t>(edx10) < 4) {
        do {
            addr_405007_4:
            *reinterpret_cast<void***>(edi12) = eax11;
            ++edi12;
            edx10 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edx10) - 1);
        } while (edx10);
    } else {
        ecx13 = reinterpret_cast<uint32_t>(-reinterpret_cast<unsigned char>(a1)) & 3;
        if (ecx13) {
            edx10 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edx10) - ecx13);
            do {
                *reinterpret_cast<void***>(edi12) = eax11;
                ++edi12;
                --ecx13;
            } while (ecx13);
        }
        eax14 = reinterpret_cast<void***>((reinterpret_cast<unsigned char>(eax11) << 8) + reinterpret_cast<unsigned char>(eax11));
        eax11 = reinterpret_cast<void**>((reinterpret_cast<uint32_t>(eax14) << 16) + reinterpret_cast<uint32_t>(eax14));
        ecx15 = edx10;
        edx10 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(edx10) & 3);
        ecx16 = reinterpret_cast<uint32_t>(ecx15) >> 2;
        if (!ecx16) 
            goto addr_405007_4; else 
            goto addr_405001_9;
    }
    addr_40500d_10:
    return a1;
    addr_405001_9:
    while (ecx16) {
        --ecx16;
        *reinterpret_cast<void***>(edi12) = eax11;
        edi12 = edi12 + 4;
    }
    if (!edx10) 
        goto addr_40500d_10; else 
        goto addr_405007_4;
}

void fun_403897(void** a1) {
    int1_t zf2;
    void** eax3;
    void** ecx4;

    VirtualFree();
    zf2 = g409228 == a1;
    if (zf2) {
        g409228 = *reinterpret_cast<void***>(a1 + 4);
    }
    if (a1 == 0x407208) {
        g407218 = 0xffffffff;
        goto 0;
    } else {
        eax3 = *reinterpret_cast<void***>(a1 + 4);
        ecx4 = *reinterpret_cast<void***>(a1);
        *reinterpret_cast<void***>(eax3) = ecx4;
        *reinterpret_cast<void***>(*reinterpret_cast<void***>(a1) + 4) = *reinterpret_cast<void***>(a1 + 4);
        HeapFree();
        goto 0;
    }
}

void fun_4038ed(void** ecx) {
    void** esi2;
    int1_t zf3;
    int32_t v4;
    uint32_t v5;
    void** edi6;
    int32_t ebx7;
    uint32_t eax8;
    int32_t eax9;
    int32_t* eax10;
    int32_t edx11;

    esi2 = g40720c;
    while (1) {
        if (*reinterpret_cast<void***>(esi2 + 16) == 0xffffffff) {
            addr_403998_3:
            zf3 = esi2 == g40720c;
            if (zf3) 
                break;
            if (v4 > 0) 
                continue; else 
                break;
        } else {
            v5 = 0;
            edi6 = esi2 + 0x2010;
            ebx7 = 0x3ff000;
            do {
                if (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(edi6) == 0xf0) && (eax8 = ebx7 + reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi2 + 16)), eax9 = reinterpret_cast<int32_t>(VirtualFree(ecx, eax8, 0x1000, 0x4000)), !!eax9)) {
                    *reinterpret_cast<void***>(edi6) = reinterpret_cast<void**>(0xffffffff);
                    --g409704;
                    if (!*reinterpret_cast<void***>(esi2 + 12) || reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi2 + 12)) > reinterpret_cast<unsigned char>(edi6)) {
                        *reinterpret_cast<void***>(esi2 + 12) = edi6;
                    }
                    ++v5;
                    --v4;
                    if (!v4) 
                        break;
                }
                ebx7 = ebx7 - 0x1000;
                edi6 = edi6 - 8;
            } while (ebx7 >= 0);
            ecx = esi2;
            esi2 = *reinterpret_cast<void***>(esi2 + 4);
            if (!v5) 
                goto addr_403998_3;
            if (!reinterpret_cast<int1_t>(*reinterpret_cast<void***>(ecx + 24) == 0xffffffff)) 
                goto addr_403998_3;
        }
        eax10 = reinterpret_cast<int32_t*>(ecx + 32);
        edx11 = 1;
        do {
            if (*eax10 != -1) 
                break;
            ++edx11;
            eax10 = eax10 + 2;
        } while (edx11 < 0x400);
        if (edx11 != 0x400) 
            goto addr_403998_3;
        fun_403897(ecx);
        ecx = ecx;
        goto addr_403998_3;
    }
    return;
}

void** fun_403c53(void** ecx, void** a2, void** a3, void** a4) {
    void** ecx5;
    void** edx6;
    void** esi7;
    void** edi8;
    void** ebx9;
    void** v10;
    void** eax11;
    void** v12;
    void** esi13;
    void*** eax14;
    void** esi15;
    void** ebx16;
    void** eax17;
    void** eax18;
    void** ebx19;
    void** ebx20;
    void** esi21;
    void** ebx22;

    ecx5 = a2;
    edx6 = a4;
    esi7 = *reinterpret_cast<void***>(ecx5 + 4);
    edi8 = *reinterpret_cast<void***>(ecx5);
    ebx9 = ecx5 + 0xf8;
    v10 = edi8;
    eax11 = edi8;
    v12 = ebx9;
    if (reinterpret_cast<unsigned char>(esi7) < reinterpret_cast<unsigned char>(edx6)) {
        esi13 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi7) + reinterpret_cast<unsigned char>(edi8));
        if (*reinterpret_cast<void***>(esi13)) {
            eax11 = esi13;
        }
        if (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(eax11) + reinterpret_cast<unsigned char>(edx6)) < reinterpret_cast<unsigned char>(ebx9)) 
            goto addr_403ca8_5;
    } else {
        *reinterpret_cast<void***>(edi8) = edx6;
        if (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(edi8) + reinterpret_cast<unsigned char>(edx6)) >= reinterpret_cast<unsigned char>(ebx9)) {
            *reinterpret_cast<void***>(ecx5 + 4) = reinterpret_cast<void**>(0);
            *reinterpret_cast<void***>(ecx5) = ecx5 + 8;
        } else {
            *reinterpret_cast<void***>(ecx5) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ecx5)) + reinterpret_cast<unsigned char>(edx6));
            *reinterpret_cast<void***>(ecx5 + 4) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(ecx5 + 4)) - reinterpret_cast<unsigned char>(edx6));
        }
        eax14 = reinterpret_cast<void***>(edi8 + 8);
        goto addr_403d66_10;
    }
    addr_403ceb_11:
    esi15 = ecx5 + 8;
    while (reinterpret_cast<unsigned char>(esi15) < reinterpret_cast<unsigned char>(edi8) && reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(esi15) + reinterpret_cast<unsigned char>(edx6)) < reinterpret_cast<unsigned char>(v12)) {
        if (*reinterpret_cast<void***>(esi15)) {
            esi15 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi15) + reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(esi15)));
        } else {
            ebx16 = esi15 + 1;
            eax17 = reinterpret_cast<void**>(1);
            while (!*reinterpret_cast<void***>(ebx16)) {
                ++ebx16;
                ++eax17;
            }
            if (reinterpret_cast<unsigned char>(eax17) >= reinterpret_cast<unsigned char>(edx6)) 
                goto addr_403d47_19;
            a3 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(a3) - reinterpret_cast<unsigned char>(eax17));
            if (reinterpret_cast<unsigned char>(a3) < reinterpret_cast<unsigned char>(edx6)) 
                break;
            esi15 = ebx16;
        }
    }
    addr_403d70_22:
    eax18 = reinterpret_cast<void**>(0);
    addr_403d72_23:
    return eax18;
    addr_403d47_19:
    ebx19 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi15) + reinterpret_cast<unsigned char>(edx6));
    if (reinterpret_cast<unsigned char>(ebx19) >= reinterpret_cast<unsigned char>(v12)) {
        *reinterpret_cast<void***>(ecx5 + 4) = reinterpret_cast<void**>(0);
        *reinterpret_cast<void***>(ecx5) = ecx5 + 8;
    } else {
        *reinterpret_cast<void***>(ecx5) = ebx19;
        *reinterpret_cast<void***>(ecx5 + 4) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(eax17) - reinterpret_cast<unsigned char>(edx6));
    }
    *reinterpret_cast<void***>(esi15) = edx6;
    eax14 = reinterpret_cast<void***>(esi15 + 8);
    addr_403d66_10:
    eax18 = reinterpret_cast<void**>((reinterpret_cast<uint32_t>(eax14) << 4) - reinterpret_cast<unsigned char>(ecx5) * 15);
    goto addr_403d72_23;
    do {
        addr_403ca8_5:
        if (*reinterpret_cast<void***>(eax11)) {
            eax11 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(eax11) + reinterpret_cast<unsigned char>(*reinterpret_cast<void***>(eax11)));
            continue;
        } else {
            ebx20 = eax11 + 1;
            esi21 = reinterpret_cast<void**>(1);
            while (!*reinterpret_cast<void***>(ebx20)) {
                ++ebx20;
                ++esi21;
            }
            if (reinterpret_cast<unsigned char>(esi21) >= reinterpret_cast<unsigned char>(edx6)) 
                break;
            if (eax11 == v10) 
                goto addr_403cc6_33;
        }
        a3 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(a3) - reinterpret_cast<unsigned char>(esi21));
        if (reinterpret_cast<unsigned char>(a3) < reinterpret_cast<unsigned char>(edx6)) 
            goto addr_403d70_22;
        addr_403cd7_35:
        edi8 = v10;
        eax11 = ebx20;
        continue;
        addr_403cc6_33:
        *reinterpret_cast<void***>(ecx5 + 4) = esi21;
        goto addr_403cd7_35;
    } while (reinterpret_cast<unsigned char>(reinterpret_cast<unsigned char>(eax11) + reinterpret_cast<unsigned char>(edx6)) < reinterpret_cast<unsigned char>(v12));
    goto addr_403ceb_11;
    ebx22 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(eax11) + reinterpret_cast<unsigned char>(edx6));
    if (reinterpret_cast<unsigned char>(ebx22) >= reinterpret_cast<unsigned char>(v12)) {
        *reinterpret_cast<void***>(ecx5 + 4) = reinterpret_cast<void**>(0);
        *reinterpret_cast<void***>(ecx5) = ecx5 + 8;
    } else {
        *reinterpret_cast<void***>(ecx5) = ebx22;
        *reinterpret_cast<void***>(ecx5 + 4) = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi21) - reinterpret_cast<unsigned char>(edx6));
    }
    *reinterpret_cast<void***>(eax11) = edx6;
    eax14 = reinterpret_cast<void***>(eax11 + 8);
    goto addr_403d66_10;
}

void** fun_40439a(void** a1, void** a2, void* a3) {
    uint32_t eax4;
    uint32_t eax5;
    uint32_t eax6;

    eax4 = reinterpret_cast<uint32_t>(a1 - 0x3a4);
    if (!eax4) {
        return 0x411;
    } else {
        eax5 = eax4 - 4;
        if (!eax5) {
            return 0x804;
        } else {
            eax6 = eax5 - 13;
            if (!eax6) {
                return 0x412;
            } else {
                if (!(eax6 - 1)) {
                    return 0x404;
                } else {
                    return 0;
                }
            }
        }
    }
}

void** g409748 = reinterpret_cast<void**>(0);

void** g40975c = reinterpret_cast<void**>(0);

void** g409964 = reinterpret_cast<void**>(0);

void** g409750 = reinterpret_cast<void**>(0);

void** g409754 = reinterpret_cast<void**>(0);

void** g409758 = reinterpret_cast<void**>(0);

void fun_4043cd(void** ecx) {
    int32_t ecx2;
    signed char* edi3;

    ecx2 = 64;
    edi3 = reinterpret_cast<signed char*>(0x409860);
    while (ecx2) {
        --ecx2;
        *edi3 = reinterpret_cast<signed char>(0);
        edi3 = edi3 + 4;
    }
    *edi3 = 0;
    g409748 = reinterpret_cast<void**>(0);
    g40975c = reinterpret_cast<void**>(0);
    g409964 = reinterpret_cast<void**>(0);
    g409750 = reinterpret_cast<void**>(0);
    g409754 = reinterpret_cast<void**>(0);
    g409758 = reinterpret_cast<void**>(0);
    return;
}

int32_t GetCPInfo = 0x69f2;

void* fun_405267(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, int32_t a8, int16_t a9, void* a10, int32_t a11);

void** fun_405018(void** a1, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, int32_t a8, void** a9, void** a10, void** a11, void** a12, ...);

void fun_4043f6(void** ecx) {
    int32_t v2;
    int32_t ebp3;
    int32_t* ebp4;
    int32_t v5;
    int32_t esi6;
    void* v7;
    void** v8;
    int32_t eax9;
    uint32_t eax10;
    unsigned char cl11;
    void** eax12;
    void** esi13;
    unsigned char al14;
    unsigned char v15;
    void** v16;
    void** v17;
    void** v18;
    void** v19;
    void** v20;
    void** v21;
    void** v22;
    void** v23;
    void** v24;
    void** v25;
    void** v26;
    void** v27;
    void** eax28;
    int16_t dx29;
    int16_t v30;
    unsigned char dl31;
    unsigned char v32;
    void** eax33;
    signed char* edi34;
    void* ecx35;
    void* ebx36;
    uint32_t ecx37;

    v2 = ebp3;
    ebp4 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    v5 = esi6;
    v7 = reinterpret_cast<void*>(ebp4 - 5);
    v8 = g409748;
    eax9 = reinterpret_cast<int32_t>(GetCPInfo());
    if (eax9 != 1) {
        eax10 = 0;
        do {
            if (eax10 < 65 || eax10 > 90) {
                if (eax10 < 97 || eax10 > 0x7a) {
                    *reinterpret_cast<unsigned char*>(eax10 + 0x409760) = 0;
                    continue;
                } else {
                    *reinterpret_cast<unsigned char*>(eax10 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax10 + 0x409861) | 32);
                    cl11 = reinterpret_cast<unsigned char>(*reinterpret_cast<signed char*>(&eax10) - 32);
                }
            } else {
                *reinterpret_cast<unsigned char*>(eax10 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax10 + 0x409861) | 16);
                cl11 = reinterpret_cast<unsigned char>(*reinterpret_cast<signed char*>(&eax10) + 32);
            }
            *reinterpret_cast<unsigned char*>(eax10 + 0x409760) = cl11;
            ++eax10;
        } while (eax10 < 0x100);
    } else {
        eax12 = reinterpret_cast<void**>(0);
        esi13 = reinterpret_cast<void**>(0x100);
        do {
            *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(ebp4) + reinterpret_cast<unsigned char>(eax12) + 0xfffffeec) = *reinterpret_cast<signed char*>(&eax12);
            ++eax12;
        } while (reinterpret_cast<unsigned char>(eax12) < reinterpret_cast<unsigned char>(0x100));
        al14 = v15;
        if (!al14) 
            goto addr_404471_13; else 
            goto addr_40443a_14;
    }
    addr_404578_15:
    return;
    addr_404471_13:
    v16 = g409964;
    v17 = g409748;
    v18 = reinterpret_cast<void**>(ebp4 + 0xfffffebb);
    v19 = reinterpret_cast<void**>(ebp4 + 0xffffffbb);
    fun_405267(ecx, 1, v19, esi13, v18, v17, v16, 0, *reinterpret_cast<int16_t*>(&v8), v7, v5);
    v20 = g409748;
    v21 = reinterpret_cast<void**>(ebp4 + 0xffffff7b);
    v22 = reinterpret_cast<void**>(ebp4 + 0xffffffbb);
    v23 = g409964;
    fun_405018(v23, esi13, v22, esi13, v21, esi13, v20, 0, 1, v19, esi13, v18, v23, esi13, v22, esi13, v21, esi13, v20, 0, 1, v19, esi13, v18);
    v24 = g409748;
    v25 = reinterpret_cast<void**>(ebp4 + 0xffffff3b);
    v26 = reinterpret_cast<void**>(ebp4 + 0xffffffbb);
    v27 = g409964;
    fun_405018(v27, 0x200, v26, esi13, v25, esi13, v24, 0, v23, esi13, v22, esi13, v27, 0x200, v26, esi13, v25, esi13, v24, 0, v23, esi13, v22, esi13);
    eax28 = reinterpret_cast<void**>(0);
    do {
        dx29 = v30;
        if (!(*reinterpret_cast<unsigned char*>(&dx29) & 1)) {
            if (!(*reinterpret_cast<unsigned char*>(&dx29) & 2)) {
                *reinterpret_cast<unsigned char*>(eax28 + 0x409760) = 0;
                continue;
            } else {
                *reinterpret_cast<unsigned char*>(eax28 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax28 + 0x409861) | 32);
                dl31 = *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(ebp4) + reinterpret_cast<unsigned char>(eax28) + 0xfffffcec);
            }
        } else {
            *reinterpret_cast<unsigned char*>(eax28 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax28 + 0x409861) | 16);
            dl31 = *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(ebp4) + reinterpret_cast<unsigned char>(eax28) + 0xfffffdec);
        }
        *reinterpret_cast<unsigned char*>(eax28 + 0x409760) = dl31;
        ++eax28;
    } while (reinterpret_cast<unsigned char>(eax28) < reinterpret_cast<unsigned char>(esi13));
    goto addr_404578_15;
    addr_40443a_14:
    do {
        ecx = reinterpret_cast<void**>(static_cast<uint32_t>(v32));
        eax33 = reinterpret_cast<void**>(static_cast<uint32_t>(al14));
        if (reinterpret_cast<unsigned char>(eax33) <= reinterpret_cast<unsigned char>(ecx)) {
            edi34 = reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(ebp4) + reinterpret_cast<unsigned char>(eax33) + 0xfffffeec);
            ecx35 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(ecx) - reinterpret_cast<unsigned char>(eax33) + 1);
            ebx36 = ecx35;
            ecx37 = reinterpret_cast<uint32_t>(ecx35) >> 2;
            while (ecx37) {
                --ecx37;
                *edi34 = reinterpret_cast<signed char>(0x20202020);
                edi34 = edi34 + 4;
                esi13 = esi13 + 4;
            }
            ecx = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(ebx36) & 3);
            while (ecx) {
                --ecx;
                *edi34 = 32;
                ++edi34;
                ++esi13;
            }
        }
        al14 = *reinterpret_cast<unsigned char*>(&v2);
    } while (al14);
    goto addr_404471_13;
}

void** fun_404350(void** a1);

struct s40 {
    unsigned char f0;
    unsigned char f1;
};

struct s41 {
    void** f0;
    signed char[3] pad4;
    void** f4;
};

struct s42 {
    void** f0;
    signed char[3] pad4;
    void** f4;
};

void** g409710 = reinterpret_cast<void**>(0);

uint32_t fun_4041b7(void** a1) {
    void* ebp2;
    void* v3;
    void* esi4;
    void** v5;
    void** edi6;
    void** eax7;
    void** esi8;
    void** ecx9;
    int1_t zf10;
    void** v11;
    uint32_t eax12;
    int32_t edx13;
    void*** eax14;
    int32_t ecx15;
    signed char* edi16;
    void** v17;
    int32_t esi18;
    struct s40* ebx19;
    struct s40* ecx20;
    uint32_t eax21;
    uint32_t edi22;
    unsigned char dl23;
    void** eax24;
    struct s41* esi25;
    struct s42* esi26;
    void* v27;
    void** v28;
    uint32_t eax29;
    int1_t zf30;
    signed char* edi31;
    int1_t below_or_equal32;
    uint32_t v33;
    void** esi34;
    signed char v35;
    unsigned char v36;
    uint32_t eax37;
    unsigned char v38;
    uint32_t edx39;
    signed char v40;
    uint32_t eax41;
    void** eax42;

    ebp2 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    v3 = esi4;
    v5 = edi6;
    eax7 = fun_404350(a1);
    esi8 = eax7;
    ecx9 = a1;
    zf10 = esi8 == g409748;
    v11 = esi8;
    if (zf10) {
        addr_404344_2:
        eax12 = 0;
    } else {
        if (!esi8) 
            goto addr_40433a_4;
        edx13 = 0;
        eax14 = reinterpret_cast<void***>(0x409238);
        do {
            if (*eax14 == esi8) 
                goto addr_404261_7;
            eax14 = eax14 + 48;
            ++edx13;
        } while (reinterpret_cast<int32_t>(eax14) < 0x409328);
        goto addr_4041fa_9;
    }
    addr_40434b_10:
    return eax12;
    addr_404261_7:
    ecx15 = 64;
    edi16 = reinterpret_cast<signed char*>(0x409860);
    while (ecx15) {
        --ecx15;
        *edi16 = reinterpret_cast<signed char>(0);
        edi16 = edi16 + 4;
    }
    v17 = reinterpret_cast<void**>(0);
    *edi16 = 0;
    esi18 = (edx13 + edx13 * 2 << 4) + 1;
    ebx19 = reinterpret_cast<struct s40*>(esi18 + 0x409248);
    do {
        ecx20 = ebx19;
        if (ebx19->f0) {
            do {
                if (!ecx20->f1) 
                    break;
                eax21 = ecx20->f0;
                edi22 = ecx20->f1;
                if (eax21 <= edi22) {
                    dl23 = *reinterpret_cast<unsigned char*>(v17 + 0x409230);
                    do {
                        *reinterpret_cast<unsigned char*>(eax21 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax21 + 0x409861) | dl23);
                        ++eax21;
                    } while (eax21 <= edi22);
                }
                ecx20 = reinterpret_cast<struct s40*>(&ecx20->f1 + 1);
            } while (ecx20->f0);
        }
        ++v17;
        ebx19 = ebx19 + 4;
    } while (reinterpret_cast<unsigned char>(v17) < reinterpret_cast<unsigned char>(4));
    g40975c = reinterpret_cast<void**>(1);
    g409748 = v11;
    eax24 = fun_40439a(v11, v5, v3);
    esi25 = reinterpret_cast<struct s41*>(esi18 + 0x40923c);
    g409750 = esi25->f0;
    esi26 = reinterpret_cast<struct s42*>(&esi25->f4);
    g409754 = esi26->f0;
    ecx9 = v11;
    g409964 = eax24;
    g409758 = esi26->f4;
    addr_40433f_34:
    fun_4043f6(ecx9);
    goto addr_404344_2;
    addr_4041fa_9:
    v27 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(ebp2) - 24);
    v28 = esi8;
    eax29 = reinterpret_cast<uint32_t>(GetCPInfo(ecx9, v28, v27));
    if (eax29 != 1) {
        zf30 = g409710 == 0;
        if (zf30) {
            eax12 = 0xffffffff;
            goto addr_40434b_10;
        } else {
            addr_40433a_4:
            fun_4043cd(ecx9);
            goto addr_40433f_34;
        }
    } else {
        ecx9 = reinterpret_cast<void**>(64);
        edi31 = reinterpret_cast<signed char*>(0x409860);
        below_or_equal32 = v33 <= 1;
        g409748 = esi8;
        while (ecx9) {
            --ecx9;
            *edi31 = reinterpret_cast<signed char>(0);
            edi31 = edi31 + 4;
            esi8 = esi8 + 4;
        }
        *edi31 = 0;
        esi34 = esi8 + 1;
        g409964 = reinterpret_cast<void**>(0);
        if (!below_or_equal32) 
            goto addr_404231_44;
    }
    g40975c = reinterpret_cast<void**>(0);
    addr_404326_46:
    g409750 = reinterpret_cast<void**>(0);
    g409754 = reinterpret_cast<void**>(0);
    g409758 = reinterpret_cast<void**>(0);
    goto addr_40433f_34;
    addr_404231_44:
    if (v35) {
        do {
            if (!v36) 
                break;
            eax37 = v38;
            edx39 = v36;
            while (eax37 <= edx39) {
                *reinterpret_cast<unsigned char*>(eax37 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax37 + 0x409861) | 4);
                ++eax37;
            }
        } while (v40);
    }
    eax41 = 1;
    do {
        *reinterpret_cast<unsigned char*>(eax41 + 0x409861) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(eax41 + 0x409861) | 8);
        ++eax41;
    } while (eax41 < 0xff);
    eax42 = fun_40439a(esi34, v28, v27);
    ecx9 = esi34;
    g409964 = eax42;
    g40975c = reinterpret_cast<void**>(1);
    goto addr_404326_46;
}

void** g409330 = reinterpret_cast<void**>(58);

void* fun_40547c(void** ecx, void* a2, void** a3) {
    void** ebp4;
    void* eax5;
    void** esi6;
    int32_t ecx7;
    int16_t v8;
    void** v9;
    void** ecx10;
    void* ebp11;
    void* eax12;
    uint32_t eax13;
    void** ecx14;

    ebp4 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    eax5 = a2;
    if (reinterpret_cast<uint32_t>(eax5) + 1 > 0x100) {
        esi6 = g409330;
        ecx7 = reinterpret_cast<int32_t>(eax5) >> 8;
        if (!(*reinterpret_cast<unsigned char*>(reinterpret_cast<uint32_t>(esi6 + *reinterpret_cast<unsigned char*>(&ecx7) * 2) + 1) & 0x80)) {
            *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(&v8) + 1) = 0;
            *reinterpret_cast<unsigned char*>(&v8) = *reinterpret_cast<unsigned char*>(&eax5);
            v9 = reinterpret_cast<void**>(1);
        } else {
            *reinterpret_cast<unsigned char*>(&v8) = *reinterpret_cast<unsigned char*>(&ecx7);
            *reinterpret_cast<unsigned char*>(reinterpret_cast<int32_t>(&v8) + 1) = *reinterpret_cast<unsigned char*>(&eax5);
            v9 = reinterpret_cast<void**>(2);
        }
        ecx10 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebp4) + 10);
        eax12 = fun_405267(ecx10, 1, ebp4 + 0xffffffff, v9, ecx10, 0, 0, 1, v8, ebp11, __return_address());
        if (eax12) {
            eax13 = *reinterpret_cast<uint16_t*>(reinterpret_cast<int32_t>(&a2) + 2);
        } else {
            return eax12;
        }
    } else {
        ecx14 = g409330;
        eax13 = reinterpret_cast<uint16_t>(*reinterpret_cast<void***>(ecx14 + reinterpret_cast<uint32_t>(eax5) * 2));
    }
    return eax13 & reinterpret_cast<unsigned char>(a3);
}

void** g409728 = reinterpret_cast<void**>(0);

int32_t g40953c = 1;

void* fun_4053b0(void** ecx, void* a2) {
    void*** ebp3;
    void** v4;
    int1_t zf5;
    int1_t less_or_equal6;
    void** eax7;
    void* eax8;
    void** edx9;
    int32_t eax10;
    void** v11;
    void** v12;
    void** v13;
    void** v14;
    void** ebx15;
    void** ebp16;
    void** eax17;
    void* eax18;

    ebp3 = reinterpret_cast<void***>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    v4 = ecx;
    zf5 = g409728 == 0;
    if (!zf5) {
        if (reinterpret_cast<int32_t>(a2) < reinterpret_cast<int32_t>(0x100)) {
            less_or_equal6 = g40953c <= 1;
            if (less_or_equal6) {
                eax7 = g409330;
                eax7 = *reinterpret_cast<void***>(eax7 + reinterpret_cast<uint32_t>(a2) * 2);
                eax8 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(eax7) & 2);
            } else {
                eax8 = fun_40547c(ecx, a2, 2);
            }
            if (!eax8) 
                goto addr_40540a_7;
        }
        edx9 = g409330;
        eax10 = reinterpret_cast<int32_t>(a2) >> 8;
        if (!(*reinterpret_cast<unsigned char*>(reinterpret_cast<uint32_t>(edx9 + *reinterpret_cast<unsigned char*>(&eax10) * 2) + 1) & 0x80)) {
            v11 = reinterpret_cast<void**>(1);
        } else {
            v11 = reinterpret_cast<void**>(2);
        }
        v12 = reinterpret_cast<void**>(ebp3 + 0xfffffffc);
        v13 = reinterpret_cast<void**>(ebp3 + 8);
        v14 = g409728;
        eax17 = fun_405018(v14, 0x200, v13, v11, v12, 3, 0, 1, ebx15, v4, ebp16, __return_address(), v14, 0x200, v13, v11, v12, 3, 0, 1, ebx15, v4, ebp16, __return_address());
        if (!eax17) {
            addr_40540a_7:
            eax18 = a2;
        } else {
            if (!reinterpret_cast<int1_t>(eax17 == 1)) {
                eax18 = reinterpret_cast<void*>(static_cast<uint32_t>(*reinterpret_cast<unsigned char*>(&v4 + 1)) << 8 | static_cast<uint32_t>(*reinterpret_cast<unsigned char*>(&v4)));
            } else {
                eax18 = reinterpret_cast<void*>(static_cast<uint32_t>(*reinterpret_cast<unsigned char*>(&v4)));
            }
        }
    } else {
        eax18 = a2;
        if (reinterpret_cast<int32_t>(eax18) >= reinterpret_cast<int32_t>(97) && reinterpret_cast<int32_t>(eax18) <= reinterpret_cast<int32_t>(0x7a)) {
            eax18 = reinterpret_cast<void*>(reinterpret_cast<uint32_t>(eax18) - 32);
        }
    }
    return eax18;
}

void** fun_40523c(void** a1, void** a2) {
    void** edx3;
    void** eax4;
    void** ecx5;
    void** esi6;

    edx3 = a2;
    eax4 = a1;
    ecx5 = edx3 + 0xffffffff;
    if (edx3) {
        do {
            if (!*reinterpret_cast<void***>(eax4)) 
                break;
            ++eax4;
            esi6 = ecx5;
            --ecx5;
        } while (esi6);
    }
    if (*reinterpret_cast<void***>(eax4)) {
        return edx3;
    } else {
        return reinterpret_cast<unsigned char>(eax4) - reinterpret_cast<unsigned char>(a1);
    }
}

void fun_404ac0(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13, void** a14, void** a15, void** a16, void** a17, void** a18, void** a19, void** a20) {
    int32_t v21;
    void* ecx22;
    uint32_t eax23;
    uint32_t eax24;
    int32_t* esp25;

    v21 = reinterpret_cast<int32_t>(__return_address());
    ecx22 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 + 8);
    if (eax23 >= 0x1000) {
        do {
            ecx22 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(ecx22) - 0x1000);
            eax24 = eax24 - 0x1000;
        } while (eax24 >= 0x1000);
    }
    esp25 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(ecx22) - eax24 - 4);
    *esp25 = v21;
    goto *esp25;
}

void** fun_401806(void** ecx, void** a2);

int32_t fun_401873(void** ecx) {
    void** eax2;
    uint32_t eax3;

    eax2 = fun_401806(ecx, __return_address());
    eax3 = -reinterpret_cast<unsigned char>(eax2);
    return reinterpret_cast<int32_t>(-(eax3 - (eax3 + reinterpret_cast<uint1_t>(eax3 < eax3 + reinterpret_cast<uint1_t>(!!eax2))))) - 1;
}

int32_t fun_401800(int32_t a1, int32_t a2, int32_t a3, int32_t a4);

int32_t g409580 = 0;

int32_t fun_401310() {
    int32_t eax1;
    int32_t v2;
    struct s3** eax3;
    int32_t edx4;
    int32_t v5;
    struct s3* ecx6;
    struct s3** v7;
    int32_t eax8;
    int32_t v9;
    struct s3** eax10;
    struct s3* ecx11;
    struct s3** v12;
    int32_t eax13;
    int32_t v14;
    void* esp15;
    int32_t ecx16;
    struct s3** eax17;
    struct s3* edx18;
    int32_t eax19;
    struct s2** eax20;
    struct s2* ecx21;
    int32_t v22;
    int32_t v23;

    eax1 = fun_401800(0, 0x409584, 0x406114, 0);
    if (eax1) {
        goto v2;
    }
    eax3 = g409584;
    edx4 = g409580;
    v5 = edx4;
    ecx6 = *eax3;
    v7 = eax3;
    eax8 = reinterpret_cast<int32_t>(ecx6->f50(v7, v5, 17, 0, 0x409584, 0x406114, 0));
    if (eax8) {
        goto v9;
    }
    eax10 = g409584;
    ecx11 = *eax10;
    v12 = eax10;
    eax13 = reinterpret_cast<int32_t>(ecx11->f54(v12, 0x280, 0x1e0, 16, 0, 0, v7, v5, 17, 0, 0x409584, 0x406114, 0));
    if (eax13) {
        goto v14;
    }
    esp15 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x8c - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4);
    ecx16 = 31;
    while (ecx16) {
        --ecx16;
    }
    eax17 = g409584;
    edx18 = *eax17;
    eax19 = reinterpret_cast<int32_t>(edx18->f18());
    if (eax19) 
        goto addr_4013d3_11;
    eax20 = g409588;
    ecx21 = *eax20;
    ecx21->f30(eax20, reinterpret_cast<int32_t>(esp15) - 4 - 4 - 4 - 4 - 4 + 4 + 4 - 4 + 4, 0x40958c, 4, 0, 0, 0, v12, 0x280, 0x1e0, 16, 0x7c, 33, v7, v5, 17, 1, 0x409584, 0x406114, 0);
    goto v22;
    addr_4013d3_11:
    goto v23;
}

void fun_4017d0(struct s0* ecx) {
    struct s1* ecx2;
    struct s1** v3;

    if (ecx->f28) {
        ecx2 = *ecx->f28;
        v3 = ecx->f28;
        ecx2->f8(v3);
        ecx->f28 = reinterpret_cast<struct s1**>(0);
    }
    return;
}

struct s44 {
    signed char[116] pad116;
    int32_t f74;
};

struct s43 {
    signed char[28] pad28;
    int32_t f1c;
    int32_t f20;
    int32_t f24;
    struct s44** f28;
};

void fun_401640(struct s43* ecx, struct s3** a2, int32_t a3, int32_t a4, int32_t a5, int32_t a6, int32_t a7, int32_t a8) {
    int32_t v9;
    struct s3** esi10;
    struct s43* ebx11;
    void* esp12;
    int32_t ebp13;
    int32_t ecx14;
    struct s3* ecx15;
    struct s44*** edi16;
    int32_t eax17;
    void* esp18;
    struct s3* eax19;
    void* esp20;
    int32_t v21;
    struct s44** edi22;
    struct s44* edx23;
    int32_t v24;
    int32_t v25;

    v9 = reinterpret_cast<int32_t>(__return_address());
    esi10 = a2;
    ebx11 = ecx;
    esp12 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 0x84 - 4 - 4 - 4 - 4);
    ebp13 = a3;
    ecx14 = 31;
    while (ecx14) {
        --ecx14;
        ++esi10;
    }
    ecx15 = *esi10;
    edi16 = &ebx11->f28;
    eax17 = reinterpret_cast<int32_t>(ecx15->f18());
    esp18 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp12) - 4 - 4 - 4 - 4 - 4 + 4);
    if (eax17) {
        if (eax17 == 0x8876017c) {
            eax19 = *esi10;
            esp20 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp18) - 4);
            eax17 = reinterpret_cast<int32_t>(eax19->f18(esi10, reinterpret_cast<int32_t>(esp20) + 28, edi16, 0));
            esp18 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp20) - 4 - 4 - 4 - 4 + 4);
        }
        if (eax17) {
            goto v21;
        }
    }
    if (v9 != -1) {
        edi22 = *edi16;
        edx23 = *edi22;
        edx23->f74(edi22, 8, reinterpret_cast<int32_t>(esp18) + 16);
    }
    ebx11->f1c = v9;
    ebx11->f24 = ebp13;
    ebx11->f20 = v24;
    goto v25;
}

int32_t LoadImageA = 0x6746;

int32_t CreateCompatibleDC = 0x67ca;

int32_t SelectObject = 0x67ba;

int32_t GetObjectA = 0x67ac;

int32_t StretchBlt = 0x679e;

int32_t DeleteDC = 0x6792;

void fun_4014d0(void** ecx, int32_t a2, int32_t a3, int32_t a4, int32_t a5, struct s30** a6, int32_t a7, int32_t a8, int32_t a9, int32_t a10) {
    struct s30** ebx11;
    struct s30** v12;
    int32_t eax13;
    struct s30* ecx14;
    struct s30** v15;
    int32_t eax16;
    int32_t v17;
    int32_t v18;
    struct s30** eax19;
    struct s30* ecx20;
    struct s30** eax21;
    struct s30* ecx22;
    int32_t eax23;
    struct s30** eax24;
    struct s30* edx25;
    void** v26;
    void** v27;
    void** v28;
    void** v29;
    void** v30;
    int32_t v31;

    ebx11 = a6;
    v12 = ebx11;
    eax13 = reinterpret_cast<int32_t>(LoadImageA());
    if (!eax13 || (!*reinterpret_cast<struct s30***>(ecx + 40) || (ecx14 = **reinterpret_cast<struct s30***>(ecx + 40), v15 = *reinterpret_cast<struct s30***>(ecx + 40), ecx14->f6c(), eax16 = reinterpret_cast<int32_t>(CreateCompatibleDC(0)), eax16 == 0))) {
        goto v17;
    } else {
        SelectObject(eax16, eax13, 0);
        GetObjectA();
        if (!ebx11) {
            ebx11 = v15;
        }
        if (!v18) {
        }
        eax19 = *reinterpret_cast<struct s30***>(ecx + 40);
        ecx20 = *eax19;
        ecx20->f58();
        eax21 = *reinterpret_cast<struct s30***>(ecx + 40);
        ecx22 = *eax21;
        eax23 = reinterpret_cast<int32_t>(ecx22->f44());
        if (!eax23) {
            StretchBlt(eax13, 0, 0, v12);
            eax24 = *reinterpret_cast<struct s30***>(ecx + 40);
            edx25 = *eax24;
            edx25->f68(eax24, 0, eax13, 0, 0, v12);
        }
        DeleteDC();
        *reinterpret_cast<void***>(ecx + 4) = v26;
        *reinterpret_cast<void***>(ecx + 8) = v27;
        *reinterpret_cast<void***>(ecx + 12) = v28;
        *reinterpret_cast<void***>(ecx + 16) = v29;
        *reinterpret_cast<struct s30***>(ecx + 20) = ebx11;
        *reinterpret_cast<void***>(ecx + 24) = v30;
        goto v31;
    }
}

int32_t GetTickCount = 0x662c;

int32_t g409548 = 0;

int32_t g409594 = 0;

int32_t g409590 = 0;

void fun_401130(void** ecx) {
    int32_t edi2;
    int32_t eax3;
    uint32_t eax4;
    int32_t eax5;
    int32_t ecx6;
    struct s0** edx7;
    struct s2** eax8;
    struct s2* ecx9;
    int32_t eax10;
    int32_t eax11;
    int32_t eax12;
    int32_t eax13;
    int32_t eax14;
    int32_t eax15;
    struct s2** eax16;
    struct s2* edx17;

    edi2 = GetTickCount;
    eax3 = reinterpret_cast<int32_t>(edi2());
    eax4 = reinterpret_cast<uint32_t>(eax3 - g409548);
    if (eax4 < 50) {
        addr_4011ef_2:
        return;
    } else {
        eax5 = g409594;
        ecx6 = g409590;
        edx7 = g40958c;
        fun_401730(0x409550, edx7, 0xf5, 0xaa, ecx6, eax5, 0x96, 0x8c);
        while (eax8 = g409588, ecx9 = *eax8, eax10 = reinterpret_cast<int32_t>(ecx9->f2c(eax8, 0, 0)), !!eax10) {
            if (eax10 == 0x887601c2) 
                goto addr_40119d_6;
            if (eax10 != 0x8876021c) 
                break;
        }
    }
    addr_4011a8_9:
    eax11 = g409590;
    eax12 = eax11 + 0x96;
    g409590 = eax12;
    if (eax12 >= 0x5dc && (eax13 = g409594, g409590 = 0, eax14 = eax13 + 0x8c, g409594 = eax14, eax14 >= 0x118)) {
        g409594 = 0;
    }
    eax15 = reinterpret_cast<int32_t>(edi2(eax8, 0, 0));
    g409548 = eax15;
    goto addr_4011ef_2;
    addr_40119d_6:
    eax16 = g409588;
    edx17 = *eax16;
    edx17->f6c(eax16, eax8, 0, 0);
    goto addr_4011a8_9;
}

int32_t g40957c = 0;

int32_t fun_401200(void** a1);

int32_t PeekMessageA = 0x6688;

int32_t TranslateMessage = 0x6674;

int32_t DispatchMessageA = 0x6660;

int32_t MessageBoxA = 0x6698;

int32_t fun_401040(void** ecx, int32_t a2, int32_t a3, int32_t a4, void** a5, uint32_t a6, void** a7, void** a8, void** a9, void** a10) {
    int32_t eax11;
    int32_t eax12;
    struct s3** eax13;
    int32_t edi14;
    int32_t esi15;
    int32_t ebx16;
    int32_t ecx17;
    void** ecx18;
    void* esp19;
    int32_t esi20;
    int32_t edi21;
    int32_t ebx22;
    int32_t eax23;
    void* esp24;
    void* v25;
    void* esp26;
    int32_t v27;
    int32_t edx28;
    int32_t v29;

    g40957c = a2;
    eax11 = fun_401200(a5);
    g409580 = eax11;
    if (eax11) {
        eax12 = fun_401310();
        if (eax12 >= 0) {
            eax13 = g409584;
            fun_401640(0x409550, eax13, 0x5dc, 0x118, 0xff, edi14, esi15, ebx16);
            ecx17 = g40957c;
            ecx18 = reinterpret_cast<void**>(0x409550);
            fun_4014d0(0x409550, ecx17, 0x65, 0, 0, 0x5dc, 0x118, edi14, esi15, ebx16);
            esp19 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 28 - 4 - 4 + 4 + 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 16 + 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 24 + 4);
            esi20 = PeekMessageA;
            edi21 = TranslateMessage;
            ebx22 = DispatchMessageA;
            while (1) {
                eax23 = reinterpret_cast<int32_t>(esi20(ecx18));
                esp24 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp19) - 4 - 4 - 4 - 4 - 4 - 4 + 4);
                if (!eax23) {
                    fun_401130(ecx18);
                    esp19 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp24) - 4 + 4);
                } else {
                    if (0) 
                        break;
                    v25 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp24) + 12);
                    edi21(ecx18, v25);
                    esp26 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp24) - 4 - 4 + 4);
                    ecx18 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp26) + 12);
                    ebx22(ecx18, v25);
                    esp19 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp26) - 4 - 4 + 4);
                }
            }
            fun_401420(ecx18);
            goto v27;
        } else {
            fun_401420(a5);
            edx28 = g409580;
            MessageBoxA(edx28, "Could start DirectX engine in your computer. Make sure you have at least version 7 of DirectX installed.", "Error", 48);
            goto v29;
        }
    } else {
        return -1;
    }
}

struct s45 {
    void** f0;
    int32_t f4;
    uint32_t f8;
};

struct s45* fun_40205d(void** a1);

int32_t UnhandledExceptionFilter = 0x6898;

struct s45* g4095f4 = reinterpret_cast<struct s45*>(0);

int32_t g407148 = 3;

int32_t g40714c = 7;

int32_t g407154 = 0x8c;

uint32_t fun_401f1c(void** a1, struct s45* a2) {
    struct s45* eax3;
    uint32_t ebx4;
    uint32_t eax5;
    struct s45* ecx6;
    struct s45* v7;
    int32_t ecx8;
    int32_t edx9;
    int32_t edx10;
    int32_t edx11;
    uint32_t* esi12;
    int32_t esi13;

    eax3 = fun_40205d(a1);
    if (!eax3 || (ebx4 = eax3->f8, ebx4 == 0)) {
        eax5 = reinterpret_cast<uint32_t>(UnhandledExceptionFilter(a1, a2));
    } else {
        if (ebx4 != 5) {
            if (ebx4 != 1) {
                ecx6 = g4095f4;
                v7 = ecx6;
                g4095f4 = a2;
                if (eax3->f4 != 8) {
                    eax3->f8 = 0;
                    ebx4();
                } else {
                    ecx8 = g407148;
                    edx9 = g40714c;
                    edx10 = edx9 + ecx8;
                    if (ecx8 < edx10) {
                        edx11 = edx10 - ecx8;
                        esi12 = reinterpret_cast<uint32_t*>((ecx8 + ecx8 * 2) * 4 + 0x4070d8);
                        do {
                            *esi12 = 0;
                            esi12 = esi12 + 3;
                            --edx11;
                        } while (edx11);
                    }
                    esi13 = g407154;
                    if (!reinterpret_cast<int1_t>(eax3->f0 == 0xc000008e)) {
                        if (!reinterpret_cast<int1_t>(eax3->f0 == 0xc0000090)) {
                            if (!reinterpret_cast<int1_t>(eax3->f0 == 0xc0000091)) {
                                if (!reinterpret_cast<int1_t>(eax3->f0 == 0xc0000093)) {
                                    if (!reinterpret_cast<int1_t>(eax3->f0 == 0xc000008d)) {
                                        if (!reinterpret_cast<int1_t>(eax3->f0 == 0xc000008f)) {
                                            if (reinterpret_cast<int1_t>(eax3->f0 == 0xc0000092)) {
                                                g407154 = 0x8a;
                                            }
                                        } else {
                                            g407154 = 0x86;
                                        }
                                    } else {
                                        g407154 = 0x82;
                                    }
                                } else {
                                    g407154 = 0x85;
                                }
                            } else {
                                g407154 = 0x84;
                            }
                        } else {
                            g407154 = 0x81;
                        }
                    } else {
                        g407154 = 0x83;
                    }
                    ebx4();
                    g407154 = esi13;
                }
                g4095f4 = v7;
            }
            eax5 = 0xffffffff;
        } else {
            eax3->f8 = 0;
            eax5 = 1;
        }
    }
    return eax5;
}

void** fun_401e13(uint32_t a1, void** a2) {
    void** v3;
    void** eax4;
    void** ecx5;
    int32_t eax6;

    v3 = reinterpret_cast<void**>(__return_address());
    if (a1 > 0xffffffe0) {
        addr_401e3c_2:
        eax4 = reinterpret_cast<void**>(0);
    } else {
        do {
            eax4 = fun_401e3f(ecx5, v3);
            if (eax4) 
                break;
            if (a2 == eax4) 
                break;
            eax6 = fun_403e20(v3);
            ecx5 = v3;
        } while (eax6);
        goto addr_401e3c_2;
    }
    return eax4;
}

struct s46 {
    void** f0;
    void** f1;
    void** f2;
    void** f3;
};

void** fun_403e40(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7) {
    void** esi8;
    void** edi9;
    void** eax10;
    uint32_t ecx11;
    uint32_t edx12;
    struct s46* esi13;
    void** edi14;
    uint32_t ecx15;
    uint32_t edx16;

    esi8 = a3;
    edi9 = a2;
    eax10 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(a4) + reinterpret_cast<unsigned char>(esi8));
    if (reinterpret_cast<unsigned char>(edi9) <= reinterpret_cast<unsigned char>(esi8) || reinterpret_cast<unsigned char>(edi9) >= reinterpret_cast<unsigned char>(eax10)) {
        if (reinterpret_cast<unsigned char>(edi9) & 3) {
            if (reinterpret_cast<unsigned char>(a4) < reinterpret_cast<unsigned char>(4)) {
                goto *reinterpret_cast<int32_t*>(reinterpret_cast<unsigned char>(a4 - 4) * 4 + 0x403f98);
            } else {
                goto *reinterpret_cast<int32_t*>("?@" + (reinterpret_cast<unsigned char>(edi9) & 3) * 4);
            }
        }
        ecx11 = reinterpret_cast<unsigned char>(a4) >> 2;
        edx12 = reinterpret_cast<unsigned char>(a4) & 3;
        if (ecx11 >= 8) 
            goto addr_403e73_7;
    } else {
        esi13 = reinterpret_cast<struct s46*>(reinterpret_cast<unsigned char>(a4) + reinterpret_cast<unsigned char>(esi8) + 0xfffffffc);
        edi14 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(a4) + reinterpret_cast<unsigned char>(edi9) + 0xfffffffc);
        if (reinterpret_cast<unsigned char>(edi14) & 3) {
            eax10 = edi14;
            if (reinterpret_cast<unsigned char>(a4) >= reinterpret_cast<unsigned char>(4)) {
                goto *reinterpret_cast<int32_t*>("A@" + (reinterpret_cast<unsigned char>(eax10) & 3) * 4);
            }
            goto *reinterpret_cast<int32_t*>("0A@" + reinterpret_cast<unsigned char>(a4) * 4);
        } else {
            ecx15 = reinterpret_cast<unsigned char>(a4) >> 2;
            edx16 = reinterpret_cast<unsigned char>(a4) & 3;
            if (ecx15 < 8) {
                goto *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(-ecx15) * 4 + 0x4040d0);
                goto *reinterpret_cast<int32_t*>("0A@" + edx16 * 4);
            } else {
                while (ecx15) {
                    --ecx15;
                    *reinterpret_cast<void***>(edi14) = esi13->f0;
                    edi14 = edi14 + 0xfffffffc;
                    --esi13;
                }
                goto *reinterpret_cast<int32_t*>("0A@" + edx16 * 4);
            }
        }
    }
    switch (ecx11) {
        addr_403f7f_20:
    case 0:
        switch (edx12) {
        case 0:
            return eax10;
        case 1:
            *reinterpret_cast<void***>(edi9) = *reinterpret_cast<void***>(esi8);
            return a2;
        case 2:
            *reinterpret_cast<void***>(edi9) = *reinterpret_cast<void***>(esi8);
            *reinterpret_cast<void***>(edi9 + 1) = *reinterpret_cast<void***>(esi8 + 1);
            return a2;
        case 3:
            *reinterpret_cast<void***>(edi9) = *reinterpret_cast<void***>(esi8);
            *reinterpret_cast<void***>(edi9 + 1) = *reinterpret_cast<void***>(esi8 + 1);
            *reinterpret_cast<void***>(edi9 + 2) = *reinterpret_cast<void***>(esi8 + 2);
            return a2;
        }
        addr_403f6c_25:
    case 1:
        *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(edi9 + ecx11 * 4) - 4) = *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(esi8 + ecx11 * 4) - 4);
        eax10 = reinterpret_cast<void**>(ecx11 * 4);
        esi8 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(esi8) + reinterpret_cast<unsigned char>(eax10));
        edi9 = reinterpret_cast<void**>(reinterpret_cast<unsigned char>(edi9) + reinterpret_cast<unsigned char>(eax10));
        goto addr_403f7f_20;
        addr_403f64_26:
    case 2:
        *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(edi9 + ecx11 * 4) - 8) = *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(esi8 + ecx11 * 4) - 8);
        goto addr_403f6c_25;
        addr_403f5c_27:
    case 3:
        *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(edi9 + ecx11 * 4) - 12) = *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(esi8 + ecx11 * 4) - 12);
        goto addr_403f64_26;
        addr_403f54_29:
    case 4:
        *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(edi9 + ecx11 * 4) - 16) = *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(esi8 + ecx11 * 4) - 16);
        goto addr_403f5c_27;
        addr_403f4c_30:
    case 5:
        *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(edi9 + ecx11 * 4) - 20) = *reinterpret_cast<int32_t*>(reinterpret_cast<uint32_t>(esi8 + ecx11 * 4) - 20);
        goto addr_403f54_29;
    case 6:
        *reinterpret_cast<void***>(reinterpret_cast<uint32_t>(edi9 + ecx11 * 4) - 24) = eax10;
        goto addr_403f4c_30;
    case 7:
    }
    addr_403e73_7:
    while (ecx11) {
        --ecx11;
        *reinterpret_cast<void***>(edi9) = *reinterpret_cast<void***>(esi8);
        edi9 = edi9 + 4;
        esi8 = esi8 + 4;
    }
    goto *reinterpret_cast<int32_t*>(edx12 * 4 + 0x403f88);
    return eax10;
    *reinterpret_cast<void***>(edi14 + 3) = esi13->f3;
    return a2;
    *reinterpret_cast<void***>(edi14 + 3) = esi13->f3;
    *reinterpret_cast<void***>(edi14 + 2) = esi13->f2;
    return a2;
    *reinterpret_cast<void***>(edi14 + 3) = esi13->f3;
    *reinterpret_cast<void***>(edi14 + 2) = esi13->f2;
    *reinterpret_cast<void***>(edi14 + 1) = esi13->f1;
    return a2;
}

int32_t g4095f0 = 0;

int32_t GetCurrentProcess = 0x686c;

int32_t TerminateProcess = 0x6858;

int32_t g4095ec = 0;

signed char g4095e8 = 0;

void** g409ab0 = reinterpret_cast<void**>(0);

void** g409aac = reinterpret_cast<void**>(0);

void fun_401ced(int32_t a1, int32_t a2, int32_t a3) {
    int32_t v4;
    void** v5;
    void** edi6;
    int1_t zf7;
    int32_t eax8;
    void** v9;
    void** ebx10;
    int32_t ebx11;
    void** eax12;
    void** ecx13;
    void** esi14;
    void** eax15;
    int1_t cf16;

    v4 = reinterpret_cast<int32_t>(__return_address());
    v5 = edi6;
    zf7 = g4095f0 == 1;
    if (zf7) {
        eax8 = reinterpret_cast<int32_t>(GetCurrentProcess(v4));
        TerminateProcess(eax8, v4);
    }
    v9 = ebx10;
    ebx11 = a3;
    g4095ec = 1;
    g4095e8 = *reinterpret_cast<signed char*>(&ebx11);
    if (!a2) {
        eax12 = g409ab0;
        if (eax12) {
            ecx13 = g409aac;
            esi14 = ecx13 + 0xfffffffc;
            if (reinterpret_cast<unsigned char>(esi14) >= reinterpret_cast<unsigned char>(eax12)) {
                do {
                    eax15 = *reinterpret_cast<void***>(esi14);
                    if (eax15) {
                        eax15();
                    }
                    esi14 = esi14 - 4;
                    cf16 = reinterpret_cast<unsigned char>(esi14) < reinterpret_cast<unsigned char>(g409ab0);
                } while (!cf16);
            }
        }
        fun_401d86(ecx13, 0x40701c, 0x407020, v9, v5);
    }
    fun_401d86(0x407020, 0x407024, 0x407028, v9, v5);
    if (!ebx11) {
        g4095f0 = 1;
        ExitProcess(0x407028, v4);
    }
    return;
}

void fun_401cdc() {
    fun_401ced(__return_address(), 1, 0);
    return;
}

int32_t g407150 = 10;

void** g4070d0 = reinterpret_cast<void**>(0xc0000005);

struct s45* fun_40205d(void** a1) {
    void** edx2;
    int32_t ecx3;
    int1_t zf4;
    struct s45* eax5;
    struct s45* esi6;

    edx2 = a1;
    ecx3 = g407150;
    zf4 = g4070d0 == edx2;
    eax5 = reinterpret_cast<struct s45*>(0x4070d0);
    if (!zf4) {
        esi6 = reinterpret_cast<struct s45*>((ecx3 + ecx3 * 2) * 4 + 0x4070d0);
        do {
            ++eax5;
            if (reinterpret_cast<uint32_t>(eax5) >= reinterpret_cast<uint32_t>(esi6)) 
                break;
        } while (eax5->f0 != edx2);
    }
    if (reinterpret_cast<uint32_t>(eax5) >= reinterpret_cast<uint32_t>((ecx3 + ecx3 * 2) * 4 + 0x4070d0) || eax5->f0 != edx2) {
        eax5 = reinterpret_cast<struct s45*>(0);
    }
    return eax5;
}

uint32_t fun_404186(unsigned char a1, uint32_t a2, unsigned char a3) {
    uint32_t eax4;
    uint32_t eax5;

    eax4 = a1;
    if (!(*reinterpret_cast<unsigned char*>(eax4 + 0x409861) & a3)) {
        if (!a2) {
            eax5 = 0;
        } else {
            eax5 = static_cast<uint32_t>(*reinterpret_cast<uint16_t*>(" " + eax4 * 2)) & a2;
        }
        if (!eax5) {
            return eax5;
        }
    }
    return 1;
}

int32_t HeapCreate = 0x69ae;

int32_t HeapDestroy = 0x69a0;

int32_t fun_402850(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11, void** a12, void** a13, void** a14) {
    void** eax15;
    void** eax16;
    void*** eax17;
    void** eax18;
    void** v19;

    eax15 = reinterpret_cast<void**>(0);
    *reinterpret_cast<unsigned char*>(&eax15) = reinterpret_cast<uint1_t>(a2 == 0);
    eax16 = reinterpret_cast<void**>(HeapCreate());
    g409984 = eax16;
    if (eax16) {
        eax17 = fun_402708(ecx, eax15, 0x1000, 0, __return_address(), a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
        g409988 = eax17;
        if (!reinterpret_cast<int1_t>(eax17 == 3)) {
            if (!reinterpret_cast<int1_t>(eax17 == 2)) 
                goto addr_4028a9_4;
            eax18 = fun_403753();
        } else {
            eax18 = fun_402c0c();
            ecx = reinterpret_cast<void**>(0x3f8);
        }
        if (eax18) {
            addr_4028a9_4:
            goto eax15;
        } else {
            v19 = g409984;
            HeapDestroy(ecx, v19);
        }
    }
    goto eax15;
}

int32_t g4095a8 = 0;

void*** fun_404722(void** a1, void** a2, void** a3, uint32_t a4) {
    void*** v5;
    void** edi6;
    void** bl7;
    void** esi8;
    int1_t less_or_equal9;
    void** ecx10;
    uint32_t eax11;
    void* eax12;
    void** v13;
    void*** eax14;
    void** ecx15;
    void*** v16;
    int1_t less_or_equal17;
    void* esi18;
    void** eax19;
    void* eax20;
    int1_t less_or_equal21;
    void** eax22;
    void* eax23;
    void** eax24;
    uint32_t ecx25;
    void** v26;
    uint32_t ecx27;
    uint32_t ecx28;
    unsigned char cl29;

    v5 = reinterpret_cast<void***>(0);
    edi6 = a1;
    bl7 = *reinterpret_cast<void***>(edi6);
    esi8 = edi6 + 1;
    while (1) {
        less_or_equal9 = g40953c <= 1;
        if (less_or_equal9) {
            ecx10 = g409330;
            eax11 = reinterpret_cast<unsigned char>(bl7);
            *reinterpret_cast<void***>(&eax11) = *reinterpret_cast<void***>(ecx10 + eax11 * 2);
            eax12 = reinterpret_cast<void*>(eax11 & 8);
        } else {
            eax12 = fun_40547c(ecx10, static_cast<uint32_t>(reinterpret_cast<unsigned char>(bl7)), 8);
            ecx10 = reinterpret_cast<void**>(8);
        }
        if (!eax12) 
            break;
        bl7 = *reinterpret_cast<void***>(esi8);
        ++esi8;
    }
    v13 = esi8;
    if (!reinterpret_cast<int1_t>(bl7 == 45)) {
        if (!reinterpret_cast<int1_t>(bl7 == 43)) {
            addr_404783_9:
            if (reinterpret_cast<signed char>(a3) < reinterpret_cast<signed char>(0) || (a3 == 1 || reinterpret_cast<signed char>(a3) > reinterpret_cast<signed char>(36))) {
                if (a2) {
                    *reinterpret_cast<void***>(a2) = edi6;
                }
                eax14 = reinterpret_cast<void***>(0);
            } else {
                ecx15 = reinterpret_cast<void**>(16);
                if (a3) {
                    addr_4047cb_14:
                    if (reinterpret_cast<int1_t>(a3 == 16) && (reinterpret_cast<int1_t>(bl7 == 48) && (*reinterpret_cast<void***>(esi8) == 0x78 || reinterpret_cast<int1_t>(*reinterpret_cast<void***>(esi8) == 88)))) {
                        bl7 = *reinterpret_cast<void***>(esi8 + 1);
                        v13 = esi8 + 1 + 1;
                    }
                } else {
                    if (bl7 == 48) {
                        if (*reinterpret_cast<void***>(esi8) == 0x78 || *reinterpret_cast<void***>(esi8) == 88) {
                            a3 = reinterpret_cast<void**>(16);
                            goto addr_4047cb_14;
                        } else {
                            a3 = reinterpret_cast<void**>(8);
                        }
                    } else {
                        a3 = reinterpret_cast<void**>(10);
                    }
                }
                v16 = reinterpret_cast<void***>(-1 / reinterpret_cast<unsigned char>(a3));
                while (1) {
                    less_or_equal17 = g40953c <= 1;
                    esi18 = reinterpret_cast<void*>(static_cast<uint32_t>(reinterpret_cast<unsigned char>(bl7)));
                    if (less_or_equal17) {
                        eax19 = g409330;
                        eax19 = *reinterpret_cast<void***>(eax19 + reinterpret_cast<uint32_t>(esi18) * 2);
                        eax20 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(eax19) & 4);
                    } else {
                        eax20 = fun_40547c(ecx15, esi18, 4);
                        ecx15 = reinterpret_cast<void**>(4);
                    }
                    if (!eax20) {
                        less_or_equal21 = g40953c <= 1;
                        if (less_or_equal21) {
                            eax22 = g409330;
                            eax22 = *reinterpret_cast<void***>(eax22 + reinterpret_cast<uint32_t>(esi18) * 2);
                            eax23 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(eax22) & 0x103);
                        } else {
                            eax23 = fun_40547c(ecx15, esi18, 0x103);
                            ecx15 = reinterpret_cast<void**>(0x103);
                        }
                        if (!eax23) 
                            break;
                        eax20 = fun_4053b0(ecx15, static_cast<int32_t>(reinterpret_cast<signed char>(bl7)));
                        ecx15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(eax20) - 55);
                    } else {
                        ecx15 = bl7 - 48;
                    }
                    if (reinterpret_cast<unsigned char>(ecx15) >= reinterpret_cast<unsigned char>(a3)) 
                        break;
                    a4 = a4 | 8;
                    if (reinterpret_cast<uint32_t>(v5) < reinterpret_cast<uint32_t>(v16) || v5 == v16 && reinterpret_cast<unsigned char>(ecx15) <= reinterpret_cast<unsigned char>(-1 % reinterpret_cast<unsigned char>(a3))) {
                        v5 = reinterpret_cast<void***>(reinterpret_cast<uint32_t>(v5) * reinterpret_cast<unsigned char>(a3) + reinterpret_cast<unsigned char>(ecx15));
                    } else {
                        a4 = a4 | 4;
                    }
                    eax24 = v13;
                    ++v13;
                    bl7 = *reinterpret_cast<void***>(eax24);
                }
                ecx25 = a4;
                v26 = v13 - 1;
                if (*reinterpret_cast<unsigned char*>(&ecx25) & 8) {
                    if (*reinterpret_cast<unsigned char*>(&ecx25) & 4 || !(*reinterpret_cast<unsigned char*>(&ecx25) & 1) && ((ecx27 = ecx25 & 2, !!ecx27) && reinterpret_cast<uint32_t>(v5) > 0x80000000 || !ecx27 && reinterpret_cast<uint32_t>(v5) > 0x7fffffff)) {
                        g4095a8 = 34;
                        if (!(*reinterpret_cast<unsigned char*>(&a4) & 1)) {
                            ecx28 = a4;
                            cl29 = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(&ecx28) & 2);
                            *reinterpret_cast<unsigned char*>(&ecx28) = -cl29;
                            v5 = reinterpret_cast<void***>(-(ecx28 - (ecx28 + reinterpret_cast<uint1_t>(ecx28 < ecx28 + reinterpret_cast<uint1_t>(!!cl29)))) + 0x7fffffff);
                        } else {
                            v5 = reinterpret_cast<void***>(0xffffffff);
                        }
                    }
                } else {
                    if (a2) {
                        v26 = a1;
                    }
                    v5 = reinterpret_cast<void***>(0);
                }
                if (a2) {
                    *reinterpret_cast<void***>(a2) = v26;
                }
                if (*reinterpret_cast<unsigned char*>(&a4) & 2) {
                    v5 = reinterpret_cast<void***>(-reinterpret_cast<uint32_t>(v5));
                }
                eax14 = v5;
            }
        } else {
            addr_40477d_50:
            bl7 = *reinterpret_cast<void***>(esi8);
            ++esi8;
            v13 = esi8;
            goto addr_404783_9;
        }
        return eax14;
    } else {
        a4 = a4 | 2;
        goto addr_40477d_50;
    }
}

int32_t RtlUnwind = 0x69ca;

void fun_4054f2() {
    goto RtlUnwind;
}

void** g409744 = reinterpret_cast<void**>(0);

int32_t GetStringTypeW = 0x6a7e;

int32_t GetStringTypeA = 0x6a6c;

void** g409738 = reinterpret_cast<void**>(0);

int32_t MultiByteToWideChar = 0x6a36;

void* fun_405267(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, int32_t a8, int16_t a9, void* a10, int32_t a11) {
    void* esp12;
    void** eax13;
    void** esp14;
    void** esp15;
    void** v16;
    void** eax17;
    void* v18;
    int32_t eax19;
    int32_t eax20;
    void* eax21;
    void** eax22;
    uint32_t eax23;
    void** v24;
    void** eax25;
    void** eax26;
    void** edi27;
    void** esi28;
    void** ebx29;
    void** v30;
    void** v31;
    void** v32;
    void** v33;
    void** esi34;
    void** eax35;

    esp12 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    eax13 = g0;
    esp14 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp12) - 4 - 4 - 4 - 4);
    g0 = esp14;
    esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp14 - 24) - 4 - 4 - 4);
    v16 = esp15;
    eax17 = g409744;
    if (!eax17) {
        v18 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(esp12) - 28);
        eax19 = reinterpret_cast<int32_t>(GetStringTypeW(1, 0x406494, 1, v18));
        esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 + 4 - 4 - 4 - 4 - 4 + 4);
        if (!eax19) {
            eax20 = reinterpret_cast<int32_t>(GetStringTypeA(0, 1, 0x406490, 1, reinterpret_cast<int32_t>(esp12) - 28, 1, 0x406494, 1, v18));
            if (!eax20) 
                goto addr_40539c_4;
            eax17 = reinterpret_cast<void**>(2);
            esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 - 4 - 4 - 4 - 4 + 4 - 4 + 4);
        } else {
            eax17 = reinterpret_cast<void**>(1);
        }
        g409744 = eax17;
    }
    if (!reinterpret_cast<int1_t>(eax17 == 2)) {
        if (!reinterpret_cast<int1_t>(eax17 == 1)) {
            addr_40539c_4:
            eax21 = reinterpret_cast<void*>(0);
        } else {
            if (!a6) {
                eax22 = g409738;
                a6 = eax22;
            }
            eax23 = reinterpret_cast<uint32_t>(-a8);
            v24 = reinterpret_cast<void**>((eax23 - (eax23 + reinterpret_cast<uint1_t>(eax23 < eax23 + reinterpret_cast<uint1_t>(!!a8))) & 8) + 1);
            eax25 = reinterpret_cast<void**>(MultiByteToWideChar(a6, v24, a3, a4, 0, 0));
            if (!eax25) 
                goto addr_40539c_4; else 
                goto addr_405339_13;
        }
    } else {
        eax26 = a7;
        if (!eax26) {
            eax26 = g409728;
        }
        eax21 = reinterpret_cast<void*>(GetStringTypeA(eax26, a2, a3, a4, a5));
    }
    addr_40539e_17:
    g0 = eax13;
    return eax21;
    addr_405339_13:
    fun_404ac0(ecx, a6, v24, a3, a4, 0, 0, edi27, esi28, ebx29, v30, v31, eax25, v32, v16, v33, eax13, 0x4029a8, 0x4064b0, 0);
    esi34 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4 + 4);
    fun_404fc0(esi34, 0, reinterpret_cast<unsigned char>(eax25) + reinterpret_cast<unsigned char>(eax25), a6, v24, a3, a4, 0, 0);
    if (!esi34) 
        goto addr_40539c_4;
    eax35 = reinterpret_cast<void**>(MultiByteToWideChar(a6, 1, a3, a4, esi34, eax25, a6, v24, a3, a4, 0, 0));
    if (!eax35) 
        goto addr_40539c_4;
    eax21 = reinterpret_cast<void*>(GetStringTypeW(a2, esi34, eax35, a5, a6, 1, a3, a4, esi34, eax25, a6, v24, a3, a4, 0, 0));
    goto addr_40539e_17;
}

void** g409740 = reinterpret_cast<void**>(0);

int32_t LCMapStringW = 0x6a5c;

int32_t LCMapStringA = 0x6a4c;

void** fun_405018(void** a1, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, int32_t a8, void** a9, void** a10, void** a11, void** a12, ...) {
    void** eax13;
    void** esp14;
    void** esp15;
    void** v16;
    int1_t zf17;
    int32_t eax18;
    int32_t eax19;
    void** eax20;
    void** ecx21;
    void** eax22;
    void** eax23;
    uint32_t eax24;
    void** v25;
    void** eax26;
    void** edi27;
    void** esi28;
    void** ebx29;
    void** v30;
    void** v31;
    void** v32;
    void** v33;
    void** v34;
    void** esp35;
    int32_t eax36;
    void** eax37;
    void** esi38;
    void** eax39;
    void** ebx40;
    int32_t eax41;
    void** v42;
    void** v43;
    void** eax44;
    int32_t eax45;

    eax13 = g0;
    esp14 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 - 4 - 4 - 4 - 4);
    g0 = esp14;
    esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp14 - 28) - 4 - 4 - 4);
    v16 = esp15;
    zf17 = g409740 == 0;
    if (zf17) {
        eax18 = reinterpret_cast<int32_t>(LCMapStringW(0, 0x100, 0x406494, 1, 0, 0));
        esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 + 4);
        if (!eax18) {
            eax19 = reinterpret_cast<int32_t>(LCMapStringA(0, 0x100, 0x406490, 1, 0, 0, 0, 0x100, 0x406494, 1, 0, 0));
            esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 - 4 - 4 - 4 - 4 - 4 + 4);
            if (!eax19) 
                goto addr_4051a6_4;
            g409740 = reinterpret_cast<void**>(2);
        } else {
            g409740 = reinterpret_cast<void**>(1);
        }
    }
    if (reinterpret_cast<signed char>(a4) > reinterpret_cast<signed char>(0)) {
        eax20 = fun_40523c(a3, a4);
        ecx21 = a4;
        esp15 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 - 4 + 4 + 4 + 4);
        a4 = eax20;
    }
    eax22 = g409740;
    if (!reinterpret_cast<int1_t>(eax22 == 2)) {
        if (!reinterpret_cast<int1_t>(eax22 == 1)) 
            goto addr_4051a6_4;
        if (!a7) {
            eax23 = g409738;
            a7 = eax23;
        }
        eax24 = reinterpret_cast<uint32_t>(-a8);
        v25 = reinterpret_cast<void**>((eax24 - (eax24 + reinterpret_cast<uint1_t>(eax24 < eax24 + reinterpret_cast<uint1_t>(!!a8))) & 8) + 1);
        eax26 = reinterpret_cast<void**>(MultiByteToWideChar(ecx21, a7, v25, a3, a4, 0, 0));
        if (!eax26) 
            goto addr_4051a6_4;
        fun_404ac0(ecx21, a7, v25, a3, a4, 0, 0, edi27, esi28, ebx29, v30, v31, v32, v33, eax26, v16, v34, eax13, 0x4029a8, 0x406498);
        esp35 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp15 - 4) - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4 + 4);
        if (!esp35) 
            goto addr_4051a6_4;
        eax36 = reinterpret_cast<int32_t>(MultiByteToWideChar(ecx21, a7, 1, a3, a4, esp35, eax26, a7, v25, a3, a4, 0, 0));
        if (!eax36) 
            goto addr_4051a6_4;
        eax37 = reinterpret_cast<void**>(LCMapStringW(ecx21, a1, a2, esp35, eax26, 0, 0, a7, 1, a3, a4, esp35, eax26, a7, v25, a3, a4, 0, 0));
        esi38 = eax37;
        if (!esi38) 
            goto addr_4051a6_4;
        if (*reinterpret_cast<unsigned char*>(&a2 + 1) & 4) 
            goto addr_40517a_19;
    } else {
        eax39 = reinterpret_cast<void**>(LCMapStringA(ecx21, a1, a2, a3, a4, a5, a6));
        goto addr_4051a8_21;
    }
    fun_404ac0(ecx21, a1, a2, esp35, eax26, 0, 0, a7, 1, a3, a4, esp35, eax26, a7, v25, a3, a4, 0, 0, edi27);
    ebx40 = reinterpret_cast<void**>(reinterpret_cast<uint32_t>(esp35 - 4) - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 - 4 - 4 - 4 - 4 + 4 - 4 + 4);
    if (!ebx40) 
        goto addr_4051a6_4;
    eax41 = reinterpret_cast<int32_t>(LCMapStringW(ecx21, a1, a2, esp35, eax26, ebx40, esi38, a1, a2, esp35, eax26, 0, 0, a7, 1, a3, a4, esp35, eax26, a7, v25, a3, a4, 0, 0));
    if (!eax41) 
        goto addr_4051a6_4;
    if (a6) {
        v42 = a6;
        v43 = a5;
    } else {
        v42 = reinterpret_cast<void**>(0);
        v43 = reinterpret_cast<void**>(0);
    }
    eax44 = reinterpret_cast<void**>(WideCharToMultiByte(ecx21, a7, 0x220, ebx40, esi38, v43, v42, 0, 0, a1, a2, esp35, eax26, ebx40, esi38, a1, a2, esp35, eax26, 0, 0, a7, 1, a3, a4, esp35, eax26, a7, v25, a3, a4, 0, 0));
    esi38 = eax44;
    if (!esi38) {
        addr_4051a6_4:
        eax39 = reinterpret_cast<void**>(0);
    } else {
        addr_405235_29:
        eax39 = esi38;
    }
    addr_4051a8_21:
    g0 = eax13;
    return eax39;
    addr_40517a_19:
    if (!a6) 
        goto addr_405235_29;
    if (reinterpret_cast<signed char>(esi38) > reinterpret_cast<signed char>(a6)) 
        goto addr_4051a6_4;
    eax45 = reinterpret_cast<int32_t>(LCMapStringW(ecx21, a1, a2, esp35, eax26, a5, a6, a1, a2, esp35, eax26, 0, 0, a7, 1, a3, a4, esp35, eax26, a7, v25, a3, a4, 0, 0));
    if (eax45) 
        goto addr_405235_29; else 
        goto addr_4051a6_4;
}

int32_t GetACP = 0x69fe;

int32_t GetOEMCP = 0x6a08;

void** fun_404350(void** a1) {
    void** eax2;

    eax2 = a1;
    g409710 = reinterpret_cast<void**>(0);
    if (!reinterpret_cast<int1_t>(eax2 == 0xfffffffe)) {
        if (!reinterpret_cast<int1_t>(eax2 == 0xfffffffd)) {
            if (reinterpret_cast<int1_t>(eax2 == 0xfffffffc)) {
                eax2 = g409738;
                g409710 = reinterpret_cast<void**>(1);
            }
            return eax2;
        } else {
            g409710 = reinterpret_cast<void**>(1);
            goto GetACP;
        }
    } else {
        g409710 = reinterpret_cast<void**>(1);
        goto GetOEMCP;
    }
}

void** fun_401806(void** ecx, void** a2) {
    void** v3;
    struct s6* eax4;
    void** edx5;
    void** ecx6;
    struct s6* eax7;
    void** v8;
    void** eax9;
    void** ecx10;
    void* ecx11;
    void** tmp32_12;

    v3 = g409ab0;
    eax4 = fun_401da0(ecx, v3);
    edx5 = g409ab0;
    ecx6 = g409aac;
    if (reinterpret_cast<uint32_t>(eax4) < reinterpret_cast<unsigned char>(ecx6) - reinterpret_cast<unsigned char>(edx5) + 4) {
        eax7 = fun_401da0(ecx6, edx5);
        v8 = g409ab0;
        eax9 = fun_4019fe(ecx6, v8, &eax7->f10, edx5, __return_address(), a2);
        if (eax9) {
            ecx10 = g409aac;
            ecx11 = reinterpret_cast<void*>(reinterpret_cast<unsigned char>(ecx10) - reinterpret_cast<unsigned char>(g409ab0));
            g409ab0 = eax9;
            ecx6 = eax9 + (reinterpret_cast<int32_t>(ecx11) >> 2) * 4;
            g409aac = ecx6;
        } else {
            return eax9;
        }
    }
    *reinterpret_cast<void***>(ecx6) = a2;
    tmp32_12 = g409aac + 4;
    g409aac = tmp32_12;
    return a2;
}

int32_t OutputDebugStringA = 0x663c;

void fun_4014a0(void** ecx) {
    struct s30** eax2;
    struct s30** eax3;
    struct s30* ecx4;

    eax2 = *reinterpret_cast<struct s30***>(ecx + 40);
    *reinterpret_cast<void***>(ecx) = reinterpret_cast<void**>(0x406110);
    if (eax2) {
        OutputDebugStringA("Surface Destroyed\n");
        eax3 = *reinterpret_cast<struct s30***>(ecx + 40);
        ecx4 = *eax3;
        ecx4->f8(eax3, "Surface Destroyed\n");
        *reinterpret_cast<struct s30***>(ecx + 40) = reinterpret_cast<struct s30**>(0);
    }
    return;
}

int32_t DirectDrawCreateEx = 0x6760;

int32_t fun_401800(int32_t a1, int32_t a2, int32_t a3, int32_t a4) {
    goto DirectDrawCreateEx;
}

int32_t LoadIconA = 0x6716;

void fun_4012d0(int32_t a1, int32_t a2, int32_t a3, int32_t a4);

int32_t LoadCursorA = 0x6708;

int32_t GetStockObject = 0x6780;

int32_t RegisterClassA = 0x66f6;

int32_t GetSystemMetrics = 0x66e2;

int32_t CreateWindowExA = 0x66d0;

int32_t ShowWindow = 0x66c2;

int32_t UpdateWindow = 0x66b2;

int32_t SetFocus = 0x66a6;

int32_t fun_401200(void** a1) {
    int32_t eax2;
    int32_t esi3;
    int32_t eax4;
    int32_t eax5;
    int32_t eax6;
    int32_t ecx7;
    int32_t esi8;
    int32_t eax9;
    int32_t eax10;

    eax2 = g40957c;
    eax4 = reinterpret_cast<int32_t>(LoadIconA(eax2, 0x7f00, esi3, 3, fun_4012d0, 0, 0, eax2));
    eax5 = reinterpret_cast<int32_t>(LoadCursorA(0, 0x7f00, eax2, 0x7f00, esi3, 3, fun_4012d0, 0, eax4, eax2));
    eax6 = reinterpret_cast<int32_t>(GetStockObject(4, 0, 0x7f00, eax2, 0x7f00, esi3, 3, fun_4012d0, eax5, eax4, eax2));
    RegisterClassA(reinterpret_cast<int32_t>(__zero_stack_offset()) - 40 - 4 - 4 - 4 - 4 + 4 - 4 - 4 - 4 + 4 - 4 - 4 + 4 + 4, 4, 0, 0x7f00, eax2, 0x7f00, esi3, 3, fun_4012d0, eax6, 0x409598, "Basic DD");
    ecx7 = g40957c;
    esi8 = GetSystemMetrics;
    esi8(1, 0, 0);
    eax9 = reinterpret_cast<int32_t>(esi8());
    eax10 = reinterpret_cast<int32_t>(CreateWindowExA(8, "Basic DD", "Basic DD", 0x80000000, 0, 0, eax9));
    ShowWindow(eax10, ecx7, 8, "Basic DD", "Basic DD", 0x80000000, 0, 0, eax9);
    UpdateWindow(eax10, eax10, ecx7, 8, "Basic DD", "Basic DD", 0x80000000, 0, 0, eax9);
    SetFocus();
    goto 0;
}

int32_t fun_401000(void** ecx) {
    int32_t eax2;

    fun_401010();
    eax2 = fun_401873(ecx);
    return eax2;
}

void fun_403f43() {
    __asm__("in al, 0x8b");
}

void fun_4040db(int32_t ecx) {
    int32_t eax2;
    signed char al3;
    unsigned char al4;
    uint1_t cf5;
    void* edi6;
    void* edi7;
    void* esi8;
    void* edi9;
    void* esi10;
    void* edi11;
    void* esi12;
    void* edi13;
    void* esi14;
    void* edi15;
    void* esi16;

    *reinterpret_cast<unsigned char*>(&eax2) = reinterpret_cast<unsigned char>(al3 - reinterpret_cast<unsigned char>(0x75 - reinterpret_cast<uint1_t>(al4 < reinterpret_cast<unsigned char>(0x75 - cf5))));
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi6) + ecx * 4 + 24) = eax2;
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi7) + ecx * 4 + 20) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi8) + ecx * 4 + 20);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi9) + ecx * 4 + 16) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi10) + ecx * 4 + 16);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi11) + ecx * 4 + 12) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi12) + ecx * 4 + 12);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi13) + ecx * 4 + 8) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi14) + ecx * 4 + 8);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi15) + ecx * 4 + 4) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi16) + ecx * 4 + 4);
}

void fun_404d83() {
    __asm__("in al, 0x8b");
}

void fun_404f1b(int32_t ecx) {
    int32_t eax2;
    signed char al3;
    unsigned char al4;
    uint1_t cf5;
    void* edi6;
    void* edi7;
    void* esi8;
    void* edi9;
    void* esi10;
    void* edi11;
    void* esi12;
    void* edi13;
    void* esi14;
    void* edi15;
    void* esi16;

    *reinterpret_cast<unsigned char*>(&eax2) = reinterpret_cast<unsigned char>(al3 - reinterpret_cast<unsigned char>(0x75 - reinterpret_cast<uint1_t>(al4 < reinterpret_cast<unsigned char>(0x75 - cf5))));
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi6) + ecx * 4 + 24) = eax2;
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi7) + ecx * 4 + 20) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi8) + ecx * 4 + 20);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi9) + ecx * 4 + 16) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi10) + ecx * 4 + 16);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi11) + ecx * 4 + 12) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi12) + ecx * 4 + 12);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi13) + ecx * 4 + 8) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi14) + ecx * 4 + 8);
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi15) + ecx * 4 + 4) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi16) + ecx * 4 + 4);
}

struct s47 {
    signed char[1] pad1;
    signed char f1;
};

struct s48 {
    signed char[1] pad1;
    signed char f1;
};

struct s49 {
    signed char[2] pad2;
    int32_t f2;
};

struct s50 {
    signed char[2] pad2;
    int32_t f2;
};

void fun_403ed9(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    signed char* edi4;
    signed char* esi5;
    uint32_t ecx6;
    struct s47* edi7;
    struct s48* esi8;
    int32_t* esi9;
    struct s49* esi10;
    int32_t* edi11;
    struct s50* edi12;

    edx2 = edx3 & ecx;
    *edi4 = *esi5;
    ecx6 = ecx >> 2;
    edi7->f1 = esi8->f1;
    esi9 = &esi10->f2;
    edi11 = &edi12->f2;
    if (ecx6 < 8) 
        goto 0x403e9c;
    while (ecx6) {
        --ecx6;
        *edi11 = *esi9;
        ++edi11;
        ++esi9;
    }
    switch (edx2) {
    case 3:
        goto 0x403fc0;
    case 2:
        goto 0x403fac;
    case 1:
        goto 0x403fa0;
    case 0:
        goto 0x403f98;
    }
}

struct s51 {
    signed char[1] pad1;
    int32_t f1;
};

struct s52 {
    signed char[1] pad1;
    int32_t f1;
};

void fun_403f00(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    signed char* edi4;
    signed char* esi5;
    int32_t* esi6;
    struct s51* esi7;
    uint32_t ecx8;
    int32_t* edi9;
    struct s52* edi10;

    edx2 = edx3 & ecx;
    *edi4 = *esi5;
    esi6 = &esi7->f1;
    ecx8 = ecx >> 2;
    edi9 = &edi10->f1;
    if (ecx8 < 8) 
        goto 0x403e9c;
    while (ecx8) {
        --ecx8;
        *edi9 = *esi6;
        ++edi9;
        ++esi6;
    }
    switch (edx2) {
    case 3:
        goto 0x403fc0;
    case 2:
        goto 0x403fac;
    case 1:
        goto 0x403fa0;
    case 0:
        goto 0x403f98;
    }
}

void fun_403f19(int32_t ecx) {
    int32_t ecx2;
    int1_t less_or_equal3;
    int32_t eax4;
    int32_t eax5;
    int32_t edi6;
    int32_t edi7;
    int32_t edi8;
    int32_t edi9;
    int32_t edi10;
    int32_t edi11;
    int32_t edi12;
    int32_t edi13;
    int32_t edi14;
    int32_t edi15;
    int32_t edi16;
    int32_t edi17;
    signed char bl18;
    int32_t edi19;
    int32_t edi20;
    int32_t edi21;
    int32_t edi22;
    signed char dl23;
    int32_t edi24;
    int32_t edi25;
    int32_t edi26;
    int32_t edi27;
    int32_t edi28;
    int32_t edi29;
    int32_t edi30;
    int32_t edi31;
    int32_t edi32;
    int32_t edi33;
    int32_t edi34;
    int32_t edi35;
    signed char bh36;
    int32_t ebx37;
    int32_t ebx38;

    ecx2 = ecx;
    if (!less_or_equal3) 
        goto 0x403f5d;
    eax4 = eax5 + 1;
    *reinterpret_cast<signed char*>(edi6 + edi7 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi8 + edi9 + 64) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&ecx2) + 1));
    *reinterpret_cast<signed char*>(edi10 + edi11 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi12 + edi13 + 64) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax4) + 1));
    *reinterpret_cast<signed char*>(edi14 + edi15 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi16 + edi17 + 64) + bl18);
    *reinterpret_cast<signed char*>(edi19 + edi20 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi21 + edi22 + 64) + dl23);
    *reinterpret_cast<signed char*>(edi24 + edi25 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi26 + edi27 + 64) + *reinterpret_cast<signed char*>(&ecx2));
    *reinterpret_cast<signed char*>(edi28 + edi29 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi30 + edi31 + 64) + *reinterpret_cast<signed char*>(&eax4));
    *reinterpret_cast<signed char*>(edi32 + edi33) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edi34 + edi35) + bh36);
    *reinterpret_cast<signed char*>(ebx37 - 0x761b71bc) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx38 - 0x761b71bc) + *reinterpret_cast<signed char*>(&ecx2));
}

void fun_403f86(signed char cl) {
    int32_t eax2;
    int16_t ax3;
    int32_t ebx4;
    int32_t ebx5;

    __asm__("aas ");
    eax2 = ax3 + 1;
    *reinterpret_cast<signed char*>(eax2 - 0x53ffbfc1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(eax2 - 0x53ffbfc1) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax2) + 1));
    __asm__("aas ");
    __asm__("aas ");
    *reinterpret_cast<signed char*>(ebx4 + 0x5f5e0845) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx5 + 0x5f5e0845) + cl);
}

void fun_403ffe() {
}

struct s53 {
    signed char[3] pad3;
    signed char f3;
};

struct s54 {
    signed char[3] pad3;
    signed char f3;
};

struct s55 {
    signed char[2] pad2;
    signed char f2;
};

struct s56 {
    signed char[2] pad2;
    signed char f2;
};

void fun_404055(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    struct s53* edi4;
    struct s54* esi5;
    uint32_t ecx6;
    struct s55* edi7;
    struct s56* esi8;
    int32_t* esi9;
    void* esi10;
    int32_t* edi11;
    void* edi12;

    edx2 = edx3 & ecx;
    edi4->f3 = esi5->f3;
    ecx6 = ecx >> 2;
    edi7->f2 = esi8->f2;
    esi9 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi10) - 2);
    edi11 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi12) - 2);
    if (ecx6 < 8) 
        goto 0x404000;
    while (ecx6) {
        --ecx6;
        *edi11 = *esi9;
        --edi11;
        --esi9;
    }
    switch (edx2) {
    case 3:
        goto 0x40415c;
    case 2:
        goto 0x404148;
    case 1:
        goto 0x404138;
    case 0:
        goto 0x404130;
    }
}

struct s57 {
    signed char[3] pad3;
    signed char f3;
};

struct s58 {
    signed char[3] pad3;
    signed char f3;
};

struct s59 {
    signed char[2] pad2;
    signed char f2;
};

struct s60 {
    signed char[2] pad2;
    signed char f2;
};

struct s61 {
    signed char[1] pad1;
    signed char f1;
};

struct s62 {
    signed char[1] pad1;
    signed char f1;
};

void fun_404080(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    struct s57* edi4;
    struct s58* esi5;
    struct s59* edi6;
    struct s60* esi7;
    uint32_t ecx8;
    struct s61* edi9;
    struct s62* esi10;
    int32_t* esi11;
    void* esi12;
    int32_t* edi13;
    void* edi14;

    edx2 = edx3 & ecx;
    edi4->f3 = esi5->f3;
    edi6->f2 = esi7->f2;
    ecx8 = ecx >> 2;
    edi9->f1 = esi10->f1;
    esi11 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi12) - 3);
    edi13 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi14) - 3);
    if (ecx8 < 8) 
        goto 0x404000;
    while (ecx8) {
        --ecx8;
        *edi13 = *esi11;
        --edi13;
        --esi11;
    }
    switch (edx2) {
    case 3:
        goto 0x40415c;
    case 2:
        goto 0x404148;
    case 1:
        goto 0x404138;
    case 0:
        goto 0x404130;
    }
}

void fun_4040b1(signed char* ecx) {
    signed char* ecx2;
    int32_t eax3;
    int32_t eax4;
    signed char bl5;
    int32_t eax6;
    int32_t eax7;
    int32_t eax8;
    signed char dh9;
    int32_t eax10;
    signed char bh11;
    int32_t eax12;
    signed char* edi13;
    signed char* edi14;
    signed char dl15;
    int32_t ecx16;
    int32_t ebx17;
    int32_t ebx18;

    ecx2 = ecx;
    __asm__("aam 0x40");
    eax3 = eax4 + 1;
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax3) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax3) + 1) + bl5);
    eax6 = eax3 + 1 + 1;
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax6) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax6) + 1) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax6) + 1));
    eax7 = eax6 + 1 + 1;
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax7) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax7) + 1) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&ecx2) + 1));
    eax8 = eax7 + 1 + 1;
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax8) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax8) + 1) + dh9);
    eax10 = eax8 + 1 + 1;
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax10) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax10) + 1) + bh11);
    eax12 = eax10 + 1 + 1;
    ecx2[eax12 * 2] = reinterpret_cast<signed char>(ecx2[eax12 * 2] + *reinterpret_cast<signed char*>(&eax12));
    *edi13 = reinterpret_cast<signed char>(*edi14 + dl15);
    ecx16 = reinterpret_cast<int32_t>(ecx2 + 1);
    *reinterpret_cast<signed char*>(ebx17 - 0x76e371bc) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx18 - 0x76e371bc) + *reinterpret_cast<signed char*>(&ecx16));
}

struct s63 {
    signed char[64] pad64;
    unsigned char f40;
};

struct s64 {
    signed char[65] pad65;
    signed char f41;
};

void fun_40411e(struct s63* ecx) {
    unsigned char al2;
    signed char* eax3;
    signed char* eax4;
    signed char bh5;
    void* ecx6;
    struct s64* eax7;
    void* eax8;
    void* eax9;
    signed char bl10;
    int32_t ebx11;
    int32_t ebx12;

    ecx->f40 = reinterpret_cast<unsigned char>(ecx->f40 ^ al2);
    *eax3 = reinterpret_cast<signed char>(*eax4 + bh5);
    ecx6 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(ecx) + 1);
    eax7 = reinterpret_cast<struct s64*>(reinterpret_cast<int32_t>(eax8) + 1);
    eax7->f41 = reinterpret_cast<signed char>(eax7->f41 + *reinterpret_cast<signed char*>(&ecx6));
    eax9 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(eax7) + 1);
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(ecx6) + reinterpret_cast<int32_t>(eax9) * 2 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(ecx6) + reinterpret_cast<int32_t>(eax9) * 2 + 64) + bl10);
    *reinterpret_cast<signed char*>(ebx11 + 0x5f5e0845) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx12 + 0x5f5e0845) + *reinterpret_cast<signed char*>(&ecx6));
}

struct s65 {
    signed char[1] pad1;
    signed char f1;
};

struct s66 {
    signed char[1] pad1;
    signed char f1;
};

struct s67 {
    signed char[2] pad2;
    int32_t f2;
};

struct s68 {
    signed char[2] pad2;
    int32_t f2;
};

void fun_404d19(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    signed char* edi4;
    signed char* esi5;
    uint32_t ecx6;
    struct s65* edi7;
    struct s66* esi8;
    int32_t* esi9;
    struct s67* esi10;
    int32_t* edi11;
    struct s68* edi12;

    edx2 = edx3 & ecx;
    *edi4 = *esi5;
    ecx6 = ecx >> 2;
    edi7->f1 = esi8->f1;
    esi9 = &esi10->f2;
    edi11 = &edi12->f2;
    if (ecx6 < 8) 
        goto 0x404cdc;
    while (ecx6) {
        --ecx6;
        *edi11 = *esi9;
        ++edi11;
        ++esi9;
    }
    switch (edx2) {
    case 3:
        goto 0x404e00;
    case 2:
        goto 0x404dec;
    case 1:
        goto 0x404de0;
    case 0:
        goto 0x404dd8;
    }
}

struct s69 {
    signed char[1] pad1;
    int32_t f1;
};

struct s70 {
    signed char[1] pad1;
    int32_t f1;
};

void fun_404d40(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    signed char* edi4;
    signed char* esi5;
    int32_t* esi6;
    struct s69* esi7;
    uint32_t ecx8;
    int32_t* edi9;
    struct s70* edi10;

    edx2 = edx3 & ecx;
    *edi4 = *esi5;
    esi6 = &esi7->f1;
    ecx8 = ecx >> 2;
    edi9 = &edi10->f1;
    if (ecx8 < 8) 
        goto 0x404cdc;
    while (ecx8) {
        --ecx8;
        *edi9 = *esi6;
        ++edi9;
        ++esi6;
    }
    switch (edx2) {
    case 3:
        goto 0x404e00;
    case 2:
        goto 0x404dec;
    case 1:
        goto 0x404de0;
    case 0:
        goto 0x404dd8;
    }
}

void fun_404d59(int32_t ecx) {
    int32_t ecx2;
    int32_t ebp3;
    int32_t ebp4;
    int32_t eax5;
    int32_t eax6;
    signed char dl7;
    int32_t eax8;
    int32_t ebx9;
    int32_t ebx10;

    ecx2 = ecx;
    ebp3 = ebp4 - 1;
    eax5 = eax6 + 1;
    *reinterpret_cast<signed char*>(ebp3 + ecx2 * 2 + 0x4d9c0040) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebp3 + ecx2 * 2 + 0x4d9c0040) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax5) + 1));
    *reinterpret_cast<signed char*>(ebp3 + ecx2 * 2 + 0x4d8c0040) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebp3 + ecx2 * 2 + 0x4d8c0040) + dl7);
    eax8 = eax5 + 1 + 1;
    *reinterpret_cast<signed char*>(ebp3 + ecx2 * 2 + 0x4d7c0040) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebp3 + ecx2 * 2 + 0x4d7c0040) + *reinterpret_cast<signed char*>(&eax8));
    *reinterpret_cast<signed char*>(ebx9 - 0x761b71bc) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx10 - 0x761b71bc) + *reinterpret_cast<signed char*>(&ecx2));
}

struct s71 {
    signed char[1] pad1;
    signed char f1;
};

void fun_404dc6(int16_t cx) {
    void* eax2;
    signed char al3;
    signed char ah4;
    struct s71* eax5;
    signed char* eax6;
    int32_t ebx7;
    int32_t ebx8;

    __asm__("fmul dword [ebp+0x40]");
    *reinterpret_cast<signed char*>(&eax2) = reinterpret_cast<signed char>(al3 + ah4);
    eax5 = reinterpret_cast<struct s71*>(reinterpret_cast<int32_t>(eax2) + 1);
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax5) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax5) + 1) + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&cx) + 1));
    eax6 = &eax5->f1;
    *eax6 = reinterpret_cast<signed char>(*eax6 + *reinterpret_cast<signed char*>(&eax6));
    *reinterpret_cast<signed char*>(ebx7 + 0x5f5e0845) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx8 + 0x5f5e0845) + *reinterpret_cast<signed char*>(&cx));
}

void fun_404e3e() {
}

struct s72 {
    signed char[3] pad3;
    signed char f3;
};

struct s73 {
    signed char[3] pad3;
    signed char f3;
};

struct s74 {
    signed char[2] pad2;
    signed char f2;
};

struct s75 {
    signed char[2] pad2;
    signed char f2;
};

void fun_404e95(uint32_t ecx) {
    uint32_t edx2;
    uint32_t edx3;
    struct s72* edi4;
    struct s73* esi5;
    uint32_t ecx6;
    struct s74* edi7;
    struct s75* esi8;
    int32_t* esi9;
    void* esi10;
    int32_t* edi11;
    void* edi12;

    edx2 = edx3 & ecx;
    edi4->f3 = esi5->f3;
    ecx6 = ecx >> 2;
    edi7->f2 = esi8->f2;
    esi9 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi10) - 2);
    edi11 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi12) - 2);
    if (ecx6 < 8) 
        goto 0x404e40;
    while (ecx6) {
        --ecx6;
        *edi11 = *esi9;
        --edi11;
        --esi9;
    }
    switch (edx2) {
    case 3:
        goto 0x404f9c;
    case 2:
        goto 0x404f88;
    case 1:
        goto 0x404f78;
    case 0:
        goto 0x404f70;
    }
}

struct s76 {
    signed char[79] pad79;
    signed char f4f;
};

struct s77 {
    signed char[79] pad79;
    signed char f4f;
};

void fun_404ef1(int32_t ecx) {
    int32_t ecx2;
    int32_t eax3;
    signed char al4;
    uint1_t cf5;
    signed char* edi6;
    signed char* edi7;
    signed char bl8;
    int32_t eax9;
    signed char* edi10;
    signed char* edi11;
    signed char* edi12;
    signed char* edi13;
    signed char* edi14;
    signed char* edi15;
    signed char dh16;
    signed char* edi17;
    signed char* edi18;
    signed char bh19;
    int32_t eax20;
    void* edi21;
    void* edi22;
    struct s76* edi23;
    struct s77* edi24;
    signed char dl25;
    int32_t ebx26;
    int32_t ebx27;

    ecx2 = ecx;
    *reinterpret_cast<unsigned char*>(&eax3) = reinterpret_cast<unsigned char>(reinterpret_cast<signed char>(al4 + 79) + cf5);
    edi6[ecx2 * 2] = reinterpret_cast<signed char>(edi7[ecx2 * 2] + bl8);
    eax9 = eax3 + 1 + 1;
    edi10[ecx2 * 2] = reinterpret_cast<signed char>(edi11[ecx2 * 2] + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax9) + 1));
    edi12[ecx2 * 2] = reinterpret_cast<signed char>(edi13[ecx2 * 2] + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&ecx2) + 1));
    edi14[ecx2 * 2] = reinterpret_cast<signed char>(edi15[ecx2 * 2] + dh16);
    edi17[ecx2 * 2] = reinterpret_cast<signed char>(edi18[ecx2 * 2] + bh19);
    eax20 = eax9 + 1 + 1 + 1 + 1;
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(edi21) + ecx2 * 2 + 64) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(edi22) + ecx2 * 2 + 64) + *reinterpret_cast<signed char*>(&eax20));
    edi23->f4f = reinterpret_cast<signed char>(edi24->f4f + dl25);
    *reinterpret_cast<signed char*>(ebx26 - 0x76e371bc) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx27 - 0x76e371bc) + *reinterpret_cast<signed char*>(&ecx2));
}

struct s78 {
    signed char[79] pad79;
    signed char f4f;
};

void fun_404f5e(signed char cl) {
    int1_t of2;
    struct s78* eax3;
    void* eax4;
    signed char bh5;
    void* eax6;
    int32_t ebx7;
    int32_t ebx8;

    if (of2) 
        goto 0x404fb1;
    eax3 = reinterpret_cast<struct s78*>(reinterpret_cast<int32_t>(eax4) + 1);
    eax3->f4f = reinterpret_cast<signed char>(eax3->f4f + bh5);
    eax6 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(eax3) + 1);
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(eax6) - 0x63ffbfb1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(eax6) - 0x63ffbfb1) + cl);
    *reinterpret_cast<signed char*>(ebx7 + 0x5f5e0845) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ebx8 + 0x5f5e0845) + cl);
}

int32_t fun_405128() {
    return 1;
}

int32_t fun_405360() {
    return 1;
}

void fun_401030() {
    goto fun_4014a0;
}

int32_t GetVersion = 0x6822;

void*** g4095c0 = reinterpret_cast<void***>(0);

uint32_t g4095bc = 0;

void** g4095b8 = reinterpret_cast<void**>(0);

uint32_t g4095b4 = 0;

int32_t GetCommandLineA = 0x6810;

void fun_4018bf() {
    void* esp1;
    void** edi2;
    void** esi3;
    void** ebx4;
    uint32_t eax5;
    void*** edx6;
    uint32_t ecx7;
    void** ecx8;
    void** v9;
    void** v10;
    void** v11;
    void** v12;
    void** v13;
    void** v14;
    void** v15;
    void** v16;
    void** v17;
    int32_t eax18;
    void** ecx19;
    void** eax20;
    void** v21;
    void** eax22;
    void** v23;
    void** v24;
    void** v25;
    void** v26;
    void** v27;
    void** eax28;
    uint32_t eax29;
    uint16_t v30;
    int32_t eax31;
    int32_t eax32;
    void** ecx33;
    struct s45* v34;

    esp1 = reinterpret_cast<void*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4);
    g0 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp1) - 4 - 4 - 4 - 4);
    eax5 = reinterpret_cast<uint32_t>(GetVersion(edi2, esi3, ebx4));
    edx6 = reinterpret_cast<void***>(0);
    *reinterpret_cast<signed char*>(&edx6) = *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax5) + 1);
    g4095c0 = edx6;
    ecx7 = eax5 & 0xff;
    g4095bc = ecx7;
    ecx8 = reinterpret_cast<void**>((ecx7 << 8) + reinterpret_cast<uint32_t>(edx6));
    g4095b8 = ecx8;
    g4095b4 = eax5 >> 16;
    eax18 = fun_402850(ecx8, 0, edi2, esi3, ebx4, v9, v10, v11, v12, v13, v14, v15, v16, v17);
    ecx19 = reinterpret_cast<void**>(0);
    if (!eax18) {
        fun_4019da(0);
        ecx19 = reinterpret_cast<void**>(28);
    }
    fun_402530(ecx19, edi2, esi3, ebx4);
    eax20 = reinterpret_cast<void**>(GetCommandLineA(ecx19, edi2, esi3, ebx4));
    g409ab8 = eax20;
    eax22 = fun_4023fe(ecx19, edi2, esi3, ebx4, v21);
    g40959c = eax22;
    fun_4021b1(ecx19, edi2, esi3, ebx4);
    fun_4020f8(ecx19, edi2, esi3, ebx4, v23, v24, v25, v26);
    fun_401c9e(ecx19, edi2, esi3, ebx4);
    v27 = reinterpret_cast<void**>(reinterpret_cast<int32_t>(esp1) + 0xffffffa4);
    GetStartupInfoA(ecx19, v27, edi2, esi3, ebx4);
    eax28 = fun_4020a0(ecx19, v27, edi2, esi3, ebx4);
    if (1) {
        eax29 = 10;
    } else {
        eax29 = v30;
    }
    eax31 = reinterpret_cast<int32_t>(GetModuleHandleA(ecx19, 0, 0, eax28, eax29, v27, edi2, esi3, ebx4));
    eax32 = fun_401040(ecx19, eax31, 0, 0, eax28, eax29, v27, edi2, esi3, ebx4);
    fun_401ccb(ecx19);
    ecx33 = *reinterpret_cast<void***>(v34->f0);
    fun_401f1c(ecx33, v34);
    goto eax32;
}

void fun_4019aa() {
    int32_t* esp1;
    int32_t ebp2;
    int32_t ebp3;

    esp1 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(*reinterpret_cast<void**>(ebp2 - 24)) - 4);
    *esp1 = *reinterpret_cast<int32_t*>(ebp3 - 0x68);
    *(esp1 - 1) = reinterpret_cast<int32_t>(fun_4019b5);
    fun_401cdc();
}

struct s79 {
    signed char[4] pad4;
    uint32_t f4;
};

int32_t fun_4028d0(struct s79* a1, int32_t a2);

int32_t fun_40295a() {
    int32_t eax1;
    void** ecx2;

    eax1 = 0;
    ecx2 = g0;
    if (reinterpret_cast<int1_t>(*reinterpret_cast<void***>(ecx2 + 4) == fun_4028d0) && *reinterpret_cast<void***>(ecx2 + 8) == *reinterpret_cast<void***>(*reinterpret_cast<void***>(ecx2 + 12) + 12)) {
        eax1 = 1;
    }
    return eax1;
}

struct s80 {
    signed char[4] pad4;
    uint32_t f4;
};

struct s81 {
    signed char[12] pad12;
    struct s31* fc;
};

int32_t fun_4029a0(struct s80* a1, struct s31* a2, int32_t a3) {
    unsigned char* esi4;
    unsigned char dh5;
    unsigned char* eax6;
    struct s81* ebp7;
    struct s31* ebx8;
    int32_t eax9;
    int32_t esi10;
    int32_t* edi11;
    int32_t ecx12;
    int32_t eax13;
    int32_t* edi14;
    int32_t ecx15;

    *esi4 = reinterpret_cast<unsigned char>(*esi4 ^ reinterpret_cast<unsigned char>(dh5 ^ *eax6));
    ebp7 = reinterpret_cast<struct s81*>(reinterpret_cast<int32_t>(__zero_stack_offset()) - 4 + 4 - 4);
    ebx8 = a2;
    if (a1->f4 & 6) {
        fun_4028f2(ebx8, 0xff);
        eax9 = 1;
    } else {
        *reinterpret_cast<void**>(reinterpret_cast<int32_t>(ebx8) - 4) = reinterpret_cast<void*>(reinterpret_cast<int32_t>(ebp7) - 8);
        esi10 = ebx8->fc;
        edi11 = ebx8->f8;
        while (esi10 != -1) {
            ecx12 = esi10 + esi10 * 2;
            if ((edi11 + ecx12)[1] && (eax13 = reinterpret_cast<int32_t>((edi11 + ecx12)[1]()), ebp7 = ebp7, esi10 = esi10, ebx8 = ebp7->fc, !!eax13)) {
                if (__intrinsic()) 
                    goto addr_402a3a_7;
                edi14 = ebx8->f8;
                fun_4028b0(ebx8);
                ebp7 = reinterpret_cast<struct s81*>(ebx8 + 1);
                fun_4028f2(ebx8, esi10);
                ecx15 = esi10 + esi10 * 2;
                fun_402986(ecx15, 1);
                ebx8->fc = edi14[ecx15];
                (edi14 + ecx15)[2]();
            }
            edi11 = ebx8->f8;
            esi10 = edi11[esi10 + esi10 * 2];
        }
        goto addr_402a41_10;
    }
    addr_402a5d_11:
    return eax9;
    addr_402a41_10:
    eax9 = 1;
    goto addr_402a5d_11;
    addr_402a3a_7:
    eax9 = 0;
    goto addr_402a5d_11;
}

void fun_40297d(int32_t ecx) {
    goto 0x402990;
}

int32_t fun_4028d0(struct s79* a1, int32_t a2) {
    int32_t eax3;
    int32_t* v4;

    eax3 = 1;
    if (a1->f4 & 6) {
        *v4 = a2;
        eax3 = 3;
    }
    return eax3;
}

void fun_4045a7() {
}

struct s82 {
    signed char[1] pad1;
    signed char f1;
};

struct s83 {
    signed char[1183449415] pad1183449415;
    int32_t f468a0147;
};

void fun_403ea4(int32_t ecx) {
    void* eax2;
    struct s82* eax3;
    signed char bl4;
    signed char* eax5;
    struct s83* eax6;
    signed char* ebx7;
    signed char* ebx8;

    *reinterpret_cast<signed char*>(&eax2) = 62;
    eax3 = reinterpret_cast<struct s82*>(reinterpret_cast<int32_t>(eax2) + 1);
    *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax3) + 1) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax3) + 1) + bl4);
    eax5 = &eax3->f1;
    *eax5 = reinterpret_cast<signed char>(*eax5 + *reinterpret_cast<signed char*>(&eax5));
    __asm__("aas ");
    eax6 = reinterpret_cast<struct s83*>(eax5 + 1);
    *ebx7 = reinterpret_cast<signed char>(*ebx8 + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax6) + 1));
    __asm__("ror dword [edx+0x8a078806], 1");
    *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(eax6) + 0x468a0147) = *reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(eax6) + 0x468a0147) + ecx;
}

void fun_403fbd(int32_t ecx) {
}

void fun_404009(int32_t ecx) {
}

void fun_404145(int32_t ecx) {
}

struct s84 {
    signed char[64] pad64;
    signed char f40;
};

struct s85 {
    signed char[64] pad64;
    signed char f40;
};

void fun_40402c() {
    struct s84* eax1;
    struct s85* eax2;
    signed char bl3;
    int32_t eax4;
    int32_t eax5;

    eax1->f40 = reinterpret_cast<signed char>(eax2->f40 + bl3);
    eax4 = eax5 + 1;
    *reinterpret_cast<signed char*>(eax4 - 0x75ffbfc0) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(eax4 - 0x75ffbfc0) + *reinterpret_cast<signed char*>(&eax4));
    __asm__("ror dword [eax+0xc14e0347], 1");
}

void fun_404935() {
}

void fun_404ce4(int32_t ecx) {
    signed char bl2;
    int32_t eax3;
    int32_t eax4;
    signed char* ebx5;
    signed char* ebx6;

    *reinterpret_cast<signed char*>(ecx * 2 + 0x4d400040) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(ecx * 2 + 0x4d400040) + bl2);
    eax3 = eax4 + 1 + 1;
    *ebx5 = reinterpret_cast<signed char>(*ebx6 + *reinterpret_cast<signed char*>(reinterpret_cast<int32_t>(&eax3) + 1));
    __asm__("ror dword [edx+0x8a078806], 1");
    *reinterpret_cast<int32_t*>(eax3 + 0x468a0147) = *reinterpret_cast<int32_t*>(eax3 + 0x468a0147) + ecx;
}

void fun_404dfd(int32_t ecx) {
}

void fun_404e49(int32_t ecx) {
}

void fun_404f85(int32_t ecx) {
}

struct s86 {
    signed char[3] pad3;
    signed char f3;
};

struct s87 {
    signed char[3] pad3;
    signed char f3;
};

struct s88 {
    signed char[2] pad2;
    signed char f2;
};

struct s89 {
    signed char[2] pad2;
    signed char f2;
};

struct s90 {
    signed char[1] pad1;
    signed char f1;
};

struct s91 {
    signed char[1] pad1;
    signed char f1;
};

struct s92 {
    signed char[3] pad3;
    signed char f3;
};

void fun_404e6c(uint32_t ecx) {
    int1_t sf2;
    uint32_t edx3;
    uint32_t edx4;
    struct s86* edi5;
    struct s87* esi6;
    struct s88* edi7;
    struct s89* esi8;
    uint32_t ecx9;
    struct s90* edi10;
    struct s91* esi11;
    int32_t* esi12;
    void* esi13;
    int32_t* edi14;
    void* edi15;
    int32_t eax16;
    int32_t eax17;
    signed char bl18;
    int32_t eax19;
    int32_t edx20;
    int32_t edx21;
    struct s92* edi22;
    int32_t* esi23;
    void* esi24;
    uint32_t ecx25;
    int32_t* edi26;
    void* edi27;
    int32_t edx28;

    if (sf2) {
        edx3 = edx4 & ecx;
        edi5->f3 = esi6->f3;
        edi7->f2 = esi8->f2;
        ecx9 = ecx >> 2;
        edi10->f1 = esi11->f1;
        esi12 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi13) - 3);
        edi14 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi15) - 3);
        if (ecx9 < 8) 
            goto 0x404e40;
        while (ecx9) {
            --ecx9;
            *edi14 = *esi12;
            --edi14;
            --esi12;
        }
        goto *reinterpret_cast<int32_t*>("pO@" + edx3 * 4);
    } else {
        eax16 = eax17 + 1;
        *reinterpret_cast<signed char*>(eax16 - 0x3fffbfb2) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(eax16 - 0x3fffbfb2) + bl18);
        eax19 = eax16 + 1;
        *reinterpret_cast<signed char*>(edx20 - 0x2edcfcba) = reinterpret_cast<signed char>(*reinterpret_cast<signed char*>(edx21 - 0x2edcfcba) + *reinterpret_cast<signed char*>(&ecx));
        edi22->f3 = *reinterpret_cast<signed char*>(&eax19);
        esi23 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(esi24) - 1 - 1);
        ecx25 = ecx >> 2;
        edi26 = reinterpret_cast<int32_t*>(reinterpret_cast<int32_t>(edi27) - 1);
        if (ecx25 < 8) 
            goto 0x404e40;
        while (ecx25) {
            --ecx25;
            *edi26 = *esi23;
            --edi26;
            --esi23;
        }
        goto *reinterpret_cast<int32_t*>("pO@" + edx28 * 4);
    }
}

void fun_40512c() {
    int32_t ebp1;
    int32_t ebp2;

    *reinterpret_cast<int32_t*>(ebp1 - 36) = 0;
    *reinterpret_cast<uint32_t*>(ebp2 - 4) = 0xffffffff;
}

int32_t fun_4051dc() {
    return 1;
}

void fun_405364() {
}

void** fun_401480(void** ecx, unsigned char a2) {
    void** esi3;

    fun_4014a0(ecx);
    if (a2 & 1) {
        fun_4018b4(ecx, ecx, esi3);
    }
    return ecx;
}

void fun_401885(void** ecx, void** a2, void** a3, void** a4, void** a5, void** a6, void** a7, void** a8, void** a9, void** a10, void** a11) {
    void** eax12;
    void** eax13;

    eax12 = fun_401e01(ecx, 0x80, __return_address(), a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
    g409ab0 = eax12;
    if (!eax12) {
        fun_4019b5(0x80);
        eax12 = g409ab0;
    }
    *reinterpret_cast<void***>(eax12) = reinterpret_cast<void**>(0);
    eax13 = g409ab0;
    g409aac = eax13;
    return;
}

int32_t PostQuitMessage = 0x6722;

int32_t DefWindowProcA = 0x6734;

void fun_4012d0(int32_t a1, int32_t a2, int32_t a3, int32_t a4) {
    int32_t eax5;

    eax5 = a2 - 2;
    if (!eax5 || !(eax5 - 0xfe) && a3 == 27) {
        PostQuitMessage();
        goto 0;
    } else {
        DefWindowProcA();
        goto a1;
    }
}

struct s93 {
    signed char[24] pad24;
    struct s31* f18;
    int32_t f1c;
};

void fun_402a65(struct s93* a1) {
    int32_t v2;
    struct s31* v3;

    v2 = a1->f1c;
    v3 = a1->f18;
    fun_4028f2(v3, v2);
    return;
}

void fun_4054f8() {
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
    signed char* eax319;
    signed char* eax320;
    signed char al321;
    signed char* eax322;
    signed char* eax323;
    signed char al324;
    signed char* eax325;
    signed char* eax326;
    signed char al327;
    signed char* eax328;
    signed char* eax329;
    signed char al330;
    signed char* eax331;
    signed char* eax332;
    signed char al333;
    signed char* eax334;
    signed char* eax335;
    signed char al336;
    signed char* eax337;
    signed char* eax338;
    signed char al339;
    signed char* eax340;
    signed char* eax341;
    signed char al342;
    signed char* eax343;
    signed char* eax344;
    signed char al345;
    signed char* eax346;
    signed char* eax347;
    signed char al348;
    signed char* eax349;
    signed char* eax350;
    signed char al351;
    signed char* eax352;
    signed char* eax353;
    signed char al354;
    signed char* eax355;
    signed char* eax356;
    signed char al357;
    signed char* eax358;
    signed char* eax359;
    signed char al360;
    signed char* eax361;
    signed char* eax362;
    signed char al363;
    signed char* eax364;
    signed char* eax365;
    signed char al366;
    signed char* eax367;
    signed char* eax368;
    signed char al369;
    signed char* eax370;
    signed char* eax371;
    signed char al372;
    signed char* eax373;
    signed char* eax374;
    signed char al375;
    signed char* eax376;
    signed char* eax377;
    signed char al378;
    signed char* eax379;
    signed char* eax380;
    signed char al381;
    signed char* eax382;
    signed char* eax383;
    signed char al384;
    signed char* eax385;
    signed char* eax386;
    signed char al387;
    signed char* eax388;
    signed char* eax389;
    signed char al390;
    signed char* eax391;
    signed char* eax392;
    signed char al393;
    signed char* eax394;
    signed char* eax395;
    signed char al396;
    signed char* eax397;
    signed char* eax398;
    signed char al399;
    signed char* eax400;
    signed char* eax401;
    signed char al402;
    signed char* eax403;
    signed char* eax404;
    signed char al405;
    signed char* eax406;
    signed char* eax407;
    signed char al408;
    signed char* eax409;
    signed char* eax410;
    signed char al411;
    signed char* eax412;
    signed char* eax413;
    signed char al414;
    signed char* eax415;
    signed char* eax416;
    signed char al417;
    signed char* eax418;
    signed char* eax419;
    signed char al420;
    signed char* eax421;
    signed char* eax422;
    signed char al423;
    signed char* eax424;
    signed char* eax425;
    signed char al426;
    signed char* eax427;
    signed char* eax428;
    signed char al429;
    signed char* eax430;
    signed char* eax431;
    signed char al432;
    signed char* eax433;
    signed char* eax434;
    signed char al435;
    signed char* eax436;
    signed char* eax437;
    signed char al438;
    signed char* eax439;
    signed char* eax440;
    signed char al441;
    signed char* eax442;
    signed char* eax443;
    signed char al444;
    signed char* eax445;
    signed char* eax446;
    signed char al447;
    signed char* eax448;
    signed char* eax449;
    signed char al450;
    signed char* eax451;
    signed char* eax452;
    signed char al453;
    signed char* eax454;
    signed char* eax455;
    signed char al456;
    signed char* eax457;
    signed char* eax458;
    signed char al459;
    signed char* eax460;
    signed char* eax461;
    signed char al462;
    signed char* eax463;
    signed char* eax464;
    signed char al465;
    signed char* eax466;
    signed char* eax467;
    signed char al468;
    signed char* eax469;
    signed char* eax470;
    signed char al471;
    signed char* eax472;
    signed char* eax473;
    signed char al474;
    signed char* eax475;
    signed char* eax476;
    signed char al477;
    signed char* eax478;
    signed char* eax479;
    signed char al480;
    signed char* eax481;
    signed char* eax482;
    signed char al483;
    signed char* eax484;
    signed char* eax485;
    signed char al486;
    signed char* eax487;
    signed char* eax488;
    signed char al489;
    signed char* eax490;
    signed char* eax491;
    signed char al492;
    signed char* eax493;
    signed char* eax494;
    signed char al495;
    signed char* eax496;
    signed char* eax497;
    signed char al498;
    signed char* eax499;
    signed char* eax500;
    signed char al501;
    signed char* eax502;
    signed char* eax503;
    signed char al504;
    signed char* eax505;
    signed char* eax506;
    signed char al507;
    signed char* eax508;
    signed char* eax509;
    signed char al510;
    signed char* eax511;
    signed char* eax512;
    signed char al513;
    signed char* eax514;
    signed char* eax515;
    signed char al516;
    signed char* eax517;
    signed char* eax518;
    signed char al519;
    signed char* eax520;
    signed char* eax521;
    signed char al522;
    signed char* eax523;
    signed char* eax524;
    signed char al525;
    signed char* eax526;
    signed char* eax527;
    signed char al528;
    signed char* eax529;
    signed char* eax530;
    signed char al531;
    signed char* eax532;
    signed char* eax533;
    signed char al534;
    signed char* eax535;
    signed char* eax536;
    signed char al537;
    signed char* eax538;
    signed char* eax539;
    signed char al540;
    signed char* eax541;
    signed char* eax542;
    signed char al543;
    signed char* eax544;
    signed char* eax545;
    signed char al546;
    signed char* eax547;
    signed char* eax548;
    signed char al549;
    signed char* eax550;
    signed char* eax551;
    signed char al552;
    signed char* eax553;
    signed char* eax554;
    signed char al555;
    signed char* eax556;
    signed char* eax557;
    signed char al558;
    signed char* eax559;
    signed char* eax560;
    signed char al561;
    signed char* eax562;
    signed char* eax563;
    signed char al564;
    signed char* eax565;
    signed char* eax566;
    signed char al567;
    signed char* eax568;
    signed char* eax569;
    signed char al570;
    signed char* eax571;
    signed char* eax572;
    signed char al573;
    signed char* eax574;
    signed char* eax575;
    signed char al576;
    signed char* eax577;
    signed char* eax578;
    signed char al579;
    signed char* eax580;
    signed char* eax581;
    signed char al582;
    signed char* eax583;
    signed char* eax584;
    signed char al585;
    signed char* eax586;
    signed char* eax587;
    signed char al588;
    signed char* eax589;
    signed char* eax590;
    signed char al591;
    signed char* eax592;
    signed char* eax593;
    signed char al594;
    signed char* eax595;
    signed char* eax596;
    signed char al597;
    signed char* eax598;
    signed char* eax599;
    signed char al600;
    signed char* eax601;
    signed char* eax602;
    signed char al603;
    signed char* eax604;
    signed char* eax605;
    signed char al606;
    signed char* eax607;
    signed char* eax608;
    signed char al609;
    signed char* eax610;
    signed char* eax611;
    signed char al612;
    signed char* eax613;
    signed char* eax614;
    signed char al615;
    signed char* eax616;
    signed char* eax617;
    signed char al618;
    signed char* eax619;
    signed char* eax620;
    signed char al621;
    signed char* eax622;
    signed char* eax623;
    signed char al624;
    signed char* eax625;
    signed char* eax626;
    signed char al627;
    signed char* eax628;
    signed char* eax629;
    signed char al630;
    signed char* eax631;
    signed char* eax632;
    signed char al633;
    signed char* eax634;
    signed char* eax635;
    signed char al636;
    signed char* eax637;
    signed char* eax638;
    signed char al639;
    signed char* eax640;
    signed char* eax641;
    signed char al642;
    signed char* eax643;
    signed char* eax644;
    signed char al645;
    signed char* eax646;
    signed char* eax647;
    signed char al648;
    signed char* eax649;
    signed char* eax650;
    signed char al651;
    signed char* eax652;
    signed char* eax653;
    signed char al654;
    signed char* eax655;
    signed char* eax656;
    signed char al657;
    signed char* eax658;
    signed char* eax659;
    signed char al660;
    signed char* eax661;
    signed char* eax662;
    signed char al663;
    signed char* eax664;
    signed char* eax665;
    signed char al666;
    signed char* eax667;
    signed char* eax668;
    signed char al669;
    signed char* eax670;
    signed char* eax671;
    signed char al672;
    signed char* eax673;
    signed char* eax674;
    signed char al675;
    signed char* eax676;
    signed char* eax677;
    signed char al678;
    signed char* eax679;
    signed char* eax680;
    signed char al681;
    signed char* eax682;
    signed char* eax683;
    signed char al684;
    signed char* eax685;
    signed char* eax686;
    signed char al687;
    signed char* eax688;
    signed char* eax689;
    signed char al690;
    signed char* eax691;
    signed char* eax692;
    signed char al693;
    signed char* eax694;
    signed char* eax695;
    signed char al696;
    signed char* eax697;
    signed char* eax698;
    signed char al699;
    signed char* eax700;
    signed char* eax701;
    signed char al702;
    signed char* eax703;
    signed char* eax704;
    signed char al705;
    signed char* eax706;
    signed char* eax707;
    signed char al708;
    signed char* eax709;
    signed char* eax710;
    signed char al711;
    signed char* eax712;
    signed char* eax713;
    signed char al714;
    signed char* eax715;
    signed char* eax716;
    signed char al717;
    signed char* eax718;
    signed char* eax719;
    signed char al720;
    signed char* eax721;
    signed char* eax722;
    signed char al723;
    signed char* eax724;
    signed char* eax725;
    signed char al726;
    signed char* eax727;
    signed char* eax728;
    signed char al729;
    signed char* eax730;
    signed char* eax731;
    signed char al732;
    signed char* eax733;
    signed char* eax734;
    signed char al735;
    signed char* eax736;
    signed char* eax737;
    signed char al738;
    signed char* eax739;
    signed char* eax740;
    signed char al741;
    signed char* eax742;
    signed char* eax743;
    signed char al744;
    signed char* eax745;
    signed char* eax746;
    signed char al747;
    signed char* eax748;
    signed char* eax749;
    signed char al750;
    signed char* eax751;
    signed char* eax752;
    signed char al753;
    signed char* eax754;
    signed char* eax755;
    signed char al756;
    signed char* eax757;
    signed char* eax758;
    signed char al759;
    signed char* eax760;
    signed char* eax761;
    signed char al762;
    signed char* eax763;
    signed char* eax764;
    signed char al765;
    signed char* eax766;
    signed char* eax767;
    signed char al768;
    signed char* eax769;
    signed char* eax770;
    signed char al771;
    signed char* eax772;
    signed char* eax773;
    signed char al774;
    signed char* eax775;
    signed char* eax776;
    signed char al777;
    signed char* eax778;
    signed char* eax779;
    signed char al780;
    signed char* eax781;
    signed char* eax782;
    signed char al783;
    signed char* eax784;
    signed char* eax785;
    signed char al786;
    signed char* eax787;
    signed char* eax788;
    signed char al789;
    signed char* eax790;
    signed char* eax791;
    signed char al792;
    signed char* eax793;
    signed char* eax794;
    signed char al795;
    signed char* eax796;
    signed char* eax797;
    signed char al798;
    signed char* eax799;
    signed char* eax800;
    signed char al801;
    signed char* eax802;
    signed char* eax803;
    signed char al804;
    signed char* eax805;
    signed char* eax806;
    signed char al807;
    signed char* eax808;
    signed char* eax809;
    signed char al810;
    signed char* eax811;
    signed char* eax812;
    signed char al813;
    signed char* eax814;
    signed char* eax815;
    signed char al816;
    signed char* eax817;
    signed char* eax818;
    signed char al819;
    signed char* eax820;
    signed char* eax821;
    signed char al822;
    signed char* eax823;
    signed char* eax824;
    signed char al825;
    signed char* eax826;
    signed char* eax827;
    signed char al828;
    signed char* eax829;
    signed char* eax830;
    signed char al831;
    signed char* eax832;
    signed char* eax833;
    signed char al834;
    signed char* eax835;
    signed char* eax836;
    signed char al837;
    signed char* eax838;
    signed char* eax839;
    signed char al840;
    signed char* eax841;
    signed char* eax842;
    signed char al843;
    signed char* eax844;
    signed char* eax845;
    signed char al846;
    signed char* eax847;
    signed char* eax848;
    signed char al849;
    signed char* eax850;
    signed char* eax851;
    signed char al852;
    signed char* eax853;
    signed char* eax854;
    signed char al855;
    signed char* eax856;
    signed char* eax857;
    signed char al858;
    signed char* eax859;
    signed char* eax860;
    signed char al861;
    signed char* eax862;
    signed char* eax863;
    signed char al864;
    signed char* eax865;
    signed char* eax866;
    signed char al867;
    signed char* eax868;
    signed char* eax869;
    signed char al870;
    signed char* eax871;
    signed char* eax872;
    signed char al873;
    signed char* eax874;
    signed char* eax875;
    signed char al876;
    signed char* eax877;
    signed char* eax878;
    signed char al879;
    signed char* eax880;
    signed char* eax881;
    signed char al882;
    signed char* eax883;
    signed char* eax884;
    signed char al885;
    signed char* eax886;
    signed char* eax887;
    signed char al888;
    signed char* eax889;
    signed char* eax890;
    signed char al891;
    signed char* eax892;
    signed char* eax893;
    signed char al894;
    signed char* eax895;
    signed char* eax896;
    signed char al897;
    signed char* eax898;
    signed char* eax899;
    signed char al900;
    signed char* eax901;
    signed char* eax902;
    signed char al903;
    signed char* eax904;
    signed char* eax905;
    signed char al906;
    signed char* eax907;
    signed char* eax908;
    signed char al909;
    signed char* eax910;
    signed char* eax911;
    signed char al912;
    signed char* eax913;
    signed char* eax914;
    signed char al915;
    signed char* eax916;
    signed char* eax917;
    signed char al918;
    signed char* eax919;
    signed char* eax920;
    signed char al921;
    signed char* eax922;
    signed char* eax923;
    signed char al924;
    signed char* eax925;
    signed char* eax926;
    signed char al927;
    signed char* eax928;
    signed char* eax929;
    signed char al930;
    signed char* eax931;
    signed char* eax932;
    signed char al933;
    signed char* eax934;
    signed char* eax935;
    signed char al936;
    signed char* eax937;
    signed char* eax938;
    signed char al939;
    signed char* eax940;
    signed char* eax941;
    signed char al942;
    signed char* eax943;
    signed char* eax944;
    signed char al945;
    signed char* eax946;
    signed char* eax947;
    signed char al948;
    signed char* eax949;
    signed char* eax950;
    signed char al951;
    signed char* eax952;
    signed char* eax953;
    signed char al954;
    signed char* eax955;
    signed char* eax956;
    signed char al957;
    signed char* eax958;
    signed char* eax959;
    signed char al960;
    signed char* eax961;
    signed char* eax962;
    signed char al963;
    signed char* eax964;
    signed char* eax965;
    signed char al966;
    signed char* eax967;
    signed char* eax968;
    signed char al969;
    signed char* eax970;
    signed char* eax971;
    signed char al972;
    signed char* eax973;
    signed char* eax974;
    signed char al975;
    signed char* eax976;
    signed char* eax977;
    signed char al978;
    signed char* eax979;
    signed char* eax980;
    signed char al981;
    signed char* eax982;
    signed char* eax983;
    signed char al984;
    signed char* eax985;
    signed char* eax986;
    signed char al987;
    signed char* eax988;
    signed char* eax989;
    signed char al990;
    signed char* eax991;
    signed char* eax992;
    signed char al993;
    signed char* eax994;
    signed char* eax995;
    signed char al996;
    signed char* eax997;
    signed char* eax998;
    signed char al999;
    signed char* eax1000;
    signed char* eax1001;
    signed char al1002;
    signed char* eax1003;
    signed char* eax1004;
    signed char al1005;
    signed char* eax1006;
    signed char* eax1007;
    signed char al1008;
    signed char* eax1009;
    signed char* eax1010;
    signed char al1011;
    signed char* eax1012;
    signed char* eax1013;
    signed char al1014;
    signed char* eax1015;
    signed char* eax1016;
    signed char al1017;
    signed char* eax1018;
    signed char* eax1019;
    signed char al1020;
    signed char* eax1021;
    signed char* eax1022;
    signed char al1023;
    signed char* eax1024;
    signed char* eax1025;
    signed char al1026;
    signed char* eax1027;
    signed char* eax1028;
    signed char al1029;
    signed char* eax1030;
    signed char* eax1031;
    signed char al1032;
    signed char* eax1033;
    signed char* eax1034;
    signed char al1035;
    signed char* eax1036;
    signed char* eax1037;
    signed char al1038;
    signed char* eax1039;
    signed char* eax1040;
    signed char al1041;
    signed char* eax1042;
    signed char* eax1043;
    signed char al1044;
    signed char* eax1045;
    signed char* eax1046;
    signed char al1047;
    signed char* eax1048;
    signed char* eax1049;
    signed char al1050;
    signed char* eax1051;
    signed char* eax1052;
    signed char al1053;
    signed char* eax1054;
    signed char* eax1055;
    signed char al1056;
    signed char* eax1057;
    signed char* eax1058;
    signed char al1059;
    signed char* eax1060;
    signed char* eax1061;
    signed char al1062;
    signed char* eax1063;
    signed char* eax1064;
    signed char al1065;
    signed char* eax1066;
    signed char* eax1067;
    signed char al1068;
    signed char* eax1069;
    signed char* eax1070;
    signed char al1071;
    signed char* eax1072;
    signed char* eax1073;
    signed char al1074;
    signed char* eax1075;
    signed char* eax1076;
    signed char al1077;
    signed char* eax1078;
    signed char* eax1079;
    signed char al1080;
    signed char* eax1081;
    signed char* eax1082;
    signed char al1083;
    signed char* eax1084;
    signed char* eax1085;
    signed char al1086;
    signed char* eax1087;
    signed char* eax1088;
    signed char al1089;
    signed char* eax1090;
    signed char* eax1091;
    signed char al1092;
    signed char* eax1093;
    signed char* eax1094;
    signed char al1095;
    signed char* eax1096;
    signed char* eax1097;
    signed char al1098;
    signed char* eax1099;
    signed char* eax1100;
    signed char al1101;
    signed char* eax1102;
    signed char* eax1103;
    signed char al1104;
    signed char* eax1105;
    signed char* eax1106;
    signed char al1107;
    signed char* eax1108;
    signed char* eax1109;
    signed char al1110;
    signed char* eax1111;
    signed char* eax1112;
    signed char al1113;
    signed char* eax1114;
    signed char* eax1115;
    signed char al1116;
    signed char* eax1117;
    signed char* eax1118;
    signed char al1119;
    signed char* eax1120;
    signed char* eax1121;
    signed char al1122;
    signed char* eax1123;
    signed char* eax1124;
    signed char al1125;
    signed char* eax1126;
    signed char* eax1127;
    signed char al1128;
    signed char* eax1129;
    signed char* eax1130;
    signed char al1131;
    signed char* eax1132;
    signed char* eax1133;
    signed char al1134;
    signed char* eax1135;
    signed char* eax1136;
    signed char al1137;
    signed char* eax1138;
    signed char* eax1139;
    signed char al1140;
    signed char* eax1141;
    signed char* eax1142;
    signed char al1143;
    signed char* eax1144;
    signed char* eax1145;
    signed char al1146;
    signed char* eax1147;
    signed char* eax1148;
    signed char al1149;
    signed char* eax1150;
    signed char* eax1151;
    signed char al1152;
    signed char* eax1153;
    signed char* eax1154;
    signed char al1155;
    signed char* eax1156;
    signed char* eax1157;
    signed char al1158;
    signed char* eax1159;
    signed char* eax1160;
    signed char al1161;
    signed char* eax1162;
    signed char* eax1163;
    signed char al1164;
    signed char* eax1165;
    signed char* eax1166;
    signed char al1167;
    signed char* eax1168;
    signed char* eax1169;
    signed char al1170;
    signed char* eax1171;
    signed char* eax1172;
    signed char al1173;
    signed char* eax1174;
    signed char* eax1175;
    signed char al1176;
    signed char* eax1177;
    signed char* eax1178;
    signed char al1179;
    signed char* eax1180;
    signed char* eax1181;
    signed char al1182;
    signed char* eax1183;
    signed char* eax1184;
    signed char al1185;
    signed char* eax1186;
    signed char* eax1187;
    signed char al1188;
    signed char* eax1189;
    signed char* eax1190;
    signed char al1191;
    signed char* eax1192;
    signed char* eax1193;
    signed char al1194;
    signed char* eax1195;
    signed char* eax1196;
    signed char al1197;
    signed char* eax1198;
    signed char* eax1199;
    signed char al1200;
    signed char* eax1201;
    signed char* eax1202;
    signed char al1203;
    signed char* eax1204;
    signed char* eax1205;
    signed char al1206;
    signed char* eax1207;
    signed char* eax1208;
    signed char al1209;
    signed char* eax1210;
    signed char* eax1211;
    signed char al1212;
    signed char* eax1213;
    signed char* eax1214;
    signed char al1215;
    signed char* eax1216;
    signed char* eax1217;
    signed char al1218;
    signed char* eax1219;
    signed char* eax1220;
    signed char al1221;
    signed char* eax1222;
    signed char* eax1223;
    signed char al1224;
    signed char* eax1225;
    signed char* eax1226;
    signed char al1227;
    signed char* eax1228;
    signed char* eax1229;
    signed char al1230;
    signed char* eax1231;
    signed char* eax1232;
    signed char al1233;
    signed char* eax1234;
    signed char* eax1235;
    signed char al1236;
    signed char* eax1237;
    signed char* eax1238;
    signed char al1239;
    signed char* eax1240;
    signed char* eax1241;
    signed char al1242;
    signed char* eax1243;
    signed char* eax1244;
    signed char al1245;
    signed char* eax1246;
    signed char* eax1247;
    signed char al1248;
    signed char* eax1249;
    signed char* eax1250;
    signed char al1251;
    signed char* eax1252;
    signed char* eax1253;
    signed char al1254;
    signed char* eax1255;
    signed char* eax1256;
    signed char al1257;
    signed char* eax1258;
    signed char* eax1259;
    signed char al1260;
    signed char* eax1261;
    signed char* eax1262;
    signed char al1263;
    signed char* eax1264;
    signed char* eax1265;
    signed char al1266;
    signed char* eax1267;
    signed char* eax1268;
    signed char al1269;
    signed char* eax1270;
    signed char* eax1271;
    signed char al1272;
    signed char* eax1273;
    signed char* eax1274;
    signed char al1275;
    signed char* eax1276;
    signed char* eax1277;
    signed char al1278;
    signed char* eax1279;
    signed char* eax1280;
    signed char al1281;
    signed char* eax1282;
    signed char* eax1283;
    signed char al1284;
    signed char* eax1285;
    signed char* eax1286;
    signed char al1287;
    signed char* eax1288;
    signed char* eax1289;
    signed char al1290;
    signed char* eax1291;
    signed char* eax1292;
    signed char al1293;
    signed char* eax1294;
    signed char* eax1295;
    signed char al1296;
    signed char* eax1297;
    signed char* eax1298;
    signed char al1299;
    signed char* eax1300;
    signed char* eax1301;
    signed char al1302;
    signed char* eax1303;
    signed char* eax1304;
    signed char al1305;
    signed char* eax1306;
    signed char* eax1307;
    signed char al1308;
    signed char* eax1309;
    signed char* eax1310;
    signed char al1311;
    signed char* eax1312;
    signed char* eax1313;
    signed char al1314;
    signed char* eax1315;
    signed char* eax1316;
    signed char al1317;
    signed char* eax1318;
    signed char* eax1319;
    signed char al1320;
    signed char* eax1321;
    signed char* eax1322;
    signed char al1323;
    signed char* eax1324;
    signed char* eax1325;
    signed char al1326;
    signed char* eax1327;
    signed char* eax1328;
    signed char al1329;
    signed char* eax1330;
    signed char* eax1331;
    signed char al1332;
    signed char* eax1333;
    signed char* eax1334;
    signed char al1335;
    signed char* eax1336;
    signed char* eax1337;
    signed char al1338;
    signed char* eax1339;
    signed char* eax1340;
    signed char al1341;
    signed char* eax1342;
    signed char* eax1343;
    signed char al1344;
    signed char* eax1345;
    signed char* eax1346;
    signed char al1347;
    signed char* eax1348;
    signed char* eax1349;
    signed char al1350;
    signed char* eax1351;
    signed char* eax1352;
    signed char al1353;
    signed char* eax1354;
    signed char* eax1355;
    signed char al1356;
    signed char* eax1357;
    signed char* eax1358;
    signed char al1359;
    signed char* eax1360;
    signed char* eax1361;
    signed char al1362;
    signed char* eax1363;
    signed char* eax1364;
    signed char al1365;
    signed char* eax1366;
    signed char* eax1367;
    signed char al1368;
    signed char* eax1369;
    signed char* eax1370;
    signed char al1371;
    signed char* eax1372;
    signed char* eax1373;
    signed char al1374;
    signed char* eax1375;
    signed char* eax1376;
    signed char al1377;
    signed char* eax1378;
    signed char* eax1379;
    signed char al1380;
    signed char* eax1381;
    signed char* eax1382;
    signed char al1383;
    signed char* eax1384;
    signed char* eax1385;
    signed char al1386;
    signed char* eax1387;
    signed char* eax1388;
    signed char al1389;
    signed char* eax1390;
    signed char* eax1391;
    signed char al1392;
    signed char* eax1393;
    signed char* eax1394;
    signed char al1395;
    signed char* eax1396;
    signed char* eax1397;
    signed char al1398;
    signed char* eax1399;
    signed char* eax1400;
    signed char al1401;
    signed char* eax1402;
    signed char* eax1403;
    signed char al1404;
    signed char* eax1405;
    signed char* eax1406;
    signed char al1407;
    signed char* eax1408;
    signed char* eax1409;
    signed char al1410;
    signed char* eax1411;
    signed char* eax1412;
    signed char al1413;
    signed char* eax1414;
    signed char* eax1415;
    signed char al1416;
    signed char* eax1417;
    signed char* eax1418;
    signed char al1419;
    signed char* eax1420;
    signed char* eax1421;
    signed char al1422;
    signed char* eax1423;
    signed char* eax1424;
    signed char al1425;
    signed char* eax1426;
    signed char* eax1427;
    signed char al1428;
    signed char* eax1429;
    signed char* eax1430;
    signed char al1431;
    signed char* eax1432;
    signed char* eax1433;
    signed char al1434;
    signed char* eax1435;
    signed char* eax1436;
    signed char al1437;
    signed char* eax1438;
    signed char* eax1439;
    signed char al1440;
    signed char* eax1441;
    signed char* eax1442;
    signed char al1443;
    signed char* eax1444;
    signed char* eax1445;
    signed char al1446;
    signed char* eax1447;
    signed char* eax1448;
    signed char al1449;
    signed char* eax1450;
    signed char* eax1451;
    signed char al1452;
    signed char* eax1453;
    signed char* eax1454;
    signed char al1455;
    signed char* eax1456;
    signed char* eax1457;
    signed char al1458;
    signed char* eax1459;
    signed char* eax1460;
    signed char al1461;
    signed char* eax1462;
    signed char* eax1463;
    signed char al1464;
    signed char* eax1465;
    signed char* eax1466;
    signed char al1467;
    signed char* eax1468;
    signed char* eax1469;
    signed char al1470;
    signed char* eax1471;
    signed char* eax1472;
    signed char al1473;
    signed char* eax1474;
    signed char* eax1475;
    signed char al1476;
    signed char* eax1477;
    signed char* eax1478;
    signed char al1479;
    signed char* eax1480;
    signed char* eax1481;
    signed char al1482;
    signed char* eax1483;
    signed char* eax1484;
    signed char al1485;
    signed char* eax1486;
    signed char* eax1487;
    signed char al1488;
    signed char* eax1489;
    signed char* eax1490;
    signed char al1491;
    signed char* eax1492;
    signed char* eax1493;
    signed char al1494;
    signed char* eax1495;
    signed char* eax1496;
    signed char al1497;
    signed char* eax1498;
    signed char* eax1499;
    signed char al1500;
    signed char* eax1501;
    signed char* eax1502;
    signed char al1503;
    signed char* eax1504;
    signed char* eax1505;
    signed char al1506;
    signed char* eax1507;
    signed char* eax1508;
    signed char al1509;
    signed char* eax1510;
    signed char* eax1511;
    signed char al1512;
    signed char* eax1513;
    signed char* eax1514;
    signed char al1515;
    signed char* eax1516;
    signed char* eax1517;
    signed char al1518;
    signed char* eax1519;
    signed char* eax1520;
    signed char al1521;
    signed char* eax1522;
    signed char* eax1523;
    signed char al1524;
    signed char* eax1525;
    signed char* eax1526;
    signed char al1527;
    signed char* eax1528;
    signed char* eax1529;
    signed char al1530;
    signed char* eax1531;
    signed char* eax1532;
    signed char al1533;
    signed char* eax1534;
    signed char* eax1535;
    signed char al1536;
    signed char* eax1537;
    signed char* eax1538;
    signed char al1539;
    signed char* eax1540;
    signed char* eax1541;
    signed char al1542;
    signed char* eax1543;
    signed char* eax1544;
    signed char al1545;
    signed char* eax1546;
    signed char* eax1547;
    signed char al1548;
    signed char* eax1549;
    signed char* eax1550;
    signed char al1551;
    signed char* eax1552;
    signed char* eax1553;
    signed char al1554;
    signed char* eax1555;
    signed char* eax1556;
    signed char al1557;
    signed char* eax1558;
    signed char* eax1559;
    signed char al1560;
    signed char* eax1561;
    signed char* eax1562;
    signed char al1563;
    signed char* eax1564;
    signed char* eax1565;
    signed char al1566;
    signed char* eax1567;
    signed char* eax1568;
    signed char al1569;
    signed char* eax1570;
    signed char* eax1571;
    signed char al1572;
    signed char* eax1573;
    signed char* eax1574;
    signed char al1575;
    signed char* eax1576;
    signed char* eax1577;
    signed char al1578;
    signed char* eax1579;
    signed char* eax1580;
    signed char al1581;
    signed char* eax1582;
    signed char* eax1583;
    signed char al1584;
    signed char* eax1585;
    signed char* eax1586;
    signed char al1587;
    signed char* eax1588;
    signed char* eax1589;
    signed char al1590;
    signed char* eax1591;
    signed char* eax1592;
    signed char al1593;
    signed char* eax1594;
    signed char* eax1595;
    signed char al1596;
    signed char* eax1597;
    signed char* eax1598;
    signed char al1599;
    signed char* eax1600;
    signed char* eax1601;
    signed char al1602;
    signed char* eax1603;
    signed char* eax1604;
    signed char al1605;
    signed char* eax1606;
    signed char* eax1607;
    signed char al1608;
    signed char* eax1609;
    signed char* eax1610;
    signed char al1611;
    signed char* eax1612;
    signed char* eax1613;
    signed char al1614;
    signed char* eax1615;
    signed char* eax1616;
    signed char al1617;
    signed char* eax1618;
    signed char* eax1619;
    signed char al1620;
    signed char* eax1621;
    signed char* eax1622;
    signed char al1623;
    signed char* eax1624;
    signed char* eax1625;
    signed char al1626;
    signed char* eax1627;
    signed char* eax1628;
    signed char al1629;
    signed char* eax1630;
    signed char* eax1631;
    signed char al1632;
    signed char* eax1633;
    signed char* eax1634;
    signed char al1635;
    signed char* eax1636;
    signed char* eax1637;
    signed char al1638;
    signed char* eax1639;
    signed char* eax1640;
    signed char al1641;
    signed char* eax1642;
    signed char* eax1643;
    signed char al1644;
    signed char* eax1645;
    signed char* eax1646;
    signed char al1647;
    signed char* eax1648;
    signed char* eax1649;
    signed char al1650;
    signed char* eax1651;
    signed char* eax1652;
    signed char al1653;
    signed char* eax1654;
    signed char* eax1655;
    signed char al1656;
    signed char* eax1657;
    signed char* eax1658;
    signed char al1659;
    signed char* eax1660;
    signed char* eax1661;
    signed char al1662;
    signed char* eax1663;
    signed char* eax1664;
    signed char al1665;
    signed char* eax1666;
    signed char* eax1667;
    signed char al1668;
    signed char* eax1669;
    signed char* eax1670;
    signed char al1671;
    signed char* eax1672;
    signed char* eax1673;
    signed char al1674;
    signed char* eax1675;
    signed char* eax1676;
    signed char al1677;
    signed char* eax1678;
    signed char* eax1679;
    signed char al1680;
    signed char* eax1681;
    signed char* eax1682;
    signed char al1683;
    signed char* eax1684;
    signed char* eax1685;
    signed char al1686;
    signed char* eax1687;
    signed char* eax1688;
    signed char al1689;
    signed char* eax1690;
    signed char* eax1691;
    signed char al1692;
    signed char* eax1693;
    signed char* eax1694;
    signed char al1695;
    signed char* eax1696;
    signed char* eax1697;
    signed char al1698;
    signed char* eax1699;
    signed char* eax1700;
    signed char al1701;
    signed char* eax1702;
    signed char* eax1703;
    signed char al1704;
    signed char* eax1705;
    signed char* eax1706;
    signed char al1707;
    signed char* eax1708;
    signed char* eax1709;
    signed char al1710;
    signed char* eax1711;
    signed char* eax1712;
    signed char al1713;
    signed char* eax1714;
    signed char* eax1715;
    signed char al1716;
    signed char* eax1717;
    signed char* eax1718;
    signed char al1719;
    signed char* eax1720;
    signed char* eax1721;
    signed char al1722;
    signed char* eax1723;
    signed char* eax1724;
    signed char al1725;
    signed char* eax1726;
    signed char* eax1727;
    signed char al1728;
    signed char* eax1729;
    signed char* eax1730;
    signed char al1731;
    signed char* eax1732;
    signed char* eax1733;
    signed char al1734;
    signed char* eax1735;
    signed char* eax1736;
    signed char al1737;
    signed char* eax1738;
    signed char* eax1739;
    signed char al1740;
    signed char* eax1741;
    signed char* eax1742;
    signed char al1743;
    signed char* eax1744;
    signed char* eax1745;
    signed char al1746;
    signed char* eax1747;
    signed char* eax1748;
    signed char al1749;
    signed char* eax1750;
    signed char* eax1751;
    signed char al1752;
    signed char* eax1753;
    signed char* eax1754;
    signed char al1755;
    signed char* eax1756;
    signed char* eax1757;
    signed char al1758;
    signed char* eax1759;
    signed char* eax1760;
    signed char al1761;
    signed char* eax1762;
    signed char* eax1763;
    signed char al1764;
    signed char* eax1765;
    signed char* eax1766;
    signed char al1767;
    signed char* eax1768;
    signed char* eax1769;
    signed char al1770;
    signed char* eax1771;
    signed char* eax1772;
    signed char al1773;
    signed char* eax1774;
    signed char* eax1775;
    signed char al1776;
    signed char* eax1777;
    signed char* eax1778;
    signed char al1779;
    signed char* eax1780;
    signed char* eax1781;
    signed char al1782;
    signed char* eax1783;
    signed char* eax1784;
    signed char al1785;
    signed char* eax1786;
    signed char* eax1787;
    signed char al1788;
    signed char* eax1789;
    signed char* eax1790;
    signed char al1791;
    signed char* eax1792;
    signed char* eax1793;
    signed char al1794;
    signed char* eax1795;
    signed char* eax1796;
    signed char al1797;
    signed char* eax1798;
    signed char* eax1799;
    signed char al1800;
    signed char* eax1801;
    signed char* eax1802;
    signed char al1803;
    signed char* eax1804;
    signed char* eax1805;
    signed char al1806;
    signed char* eax1807;
    signed char* eax1808;
    signed char al1809;
    signed char* eax1810;
    signed char* eax1811;
    signed char al1812;
    signed char* eax1813;
    signed char* eax1814;
    signed char al1815;
    signed char* eax1816;
    signed char* eax1817;
    signed char al1818;
    signed char* eax1819;
    signed char* eax1820;
    signed char al1821;
    signed char* eax1822;
    signed char* eax1823;
    signed char al1824;
    signed char* eax1825;
    signed char* eax1826;
    signed char al1827;
    signed char* eax1828;
    signed char* eax1829;
    signed char al1830;
    signed char* eax1831;
    signed char* eax1832;
    signed char al1833;
    signed char* eax1834;
    signed char* eax1835;
    signed char al1836;
    signed char* eax1837;
    signed char* eax1838;
    signed char al1839;
    signed char* eax1840;
    signed char* eax1841;
    signed char al1842;
    signed char* eax1843;
    signed char* eax1844;
    signed char al1845;
    signed char* eax1846;
    signed char* eax1847;
    signed char al1848;
    signed char* eax1849;
    signed char* eax1850;
    signed char al1851;
    signed char* eax1852;
    signed char* eax1853;
    signed char al1854;
    signed char* eax1855;
    signed char* eax1856;
    signed char al1857;
    signed char* eax1858;
    signed char* eax1859;
    signed char al1860;
    signed char* eax1861;
    signed char* eax1862;
    signed char al1863;
    signed char* eax1864;
    signed char* eax1865;
    signed char al1866;
    signed char* eax1867;
    signed char* eax1868;
    signed char al1869;
    signed char* eax1870;
    signed char* eax1871;
    signed char al1872;
    signed char* eax1873;
    signed char* eax1874;
    signed char al1875;
    signed char* eax1876;
    signed char* eax1877;
    signed char al1878;
    signed char* eax1879;
    signed char* eax1880;
    signed char al1881;
    signed char* eax1882;
    signed char* eax1883;
    signed char al1884;
    signed char* eax1885;
    signed char* eax1886;
    signed char al1887;
    signed char* eax1888;
    signed char* eax1889;
    signed char al1890;
    signed char* eax1891;
    signed char* eax1892;
    signed char al1893;
    signed char* eax1894;
    signed char* eax1895;
    signed char al1896;
    signed char* eax1897;
    signed char* eax1898;
    signed char al1899;
    signed char* eax1900;
    signed char* eax1901;
    signed char al1902;
    signed char* eax1903;
    signed char* eax1904;
    signed char al1905;
    signed char* eax1906;
    signed char* eax1907;
    signed char al1908;
    signed char* eax1909;
    signed char* eax1910;
    signed char al1911;
    signed char* eax1912;
    signed char* eax1913;
    signed char al1914;
    signed char* eax1915;
    signed char* eax1916;
    signed char al1917;
    signed char* eax1918;
    signed char* eax1919;
    signed char al1920;
    signed char* eax1921;
    signed char* eax1922;
    signed char al1923;
    signed char* eax1924;
    signed char* eax1925;
    signed char al1926;
    signed char* eax1927;
    signed char* eax1928;
    signed char al1929;
    signed char* eax1930;
    signed char* eax1931;
    signed char al1932;
    signed char* eax1933;
    signed char* eax1934;
    signed char al1935;
    signed char* eax1936;
    signed char* eax1937;
    signed char al1938;
    signed char* eax1939;
    signed char* eax1940;
    signed char al1941;
    signed char* eax1942;
    signed char* eax1943;
    signed char al1944;
    signed char* eax1945;
    signed char* eax1946;
    signed char al1947;
    signed char* eax1948;
    signed char* eax1949;
    signed char al1950;
    signed char* eax1951;
    signed char* eax1952;
    signed char al1953;
    signed char* eax1954;
    signed char* eax1955;
    signed char al1956;
    signed char* eax1957;
    signed char* eax1958;
    signed char al1959;
    signed char* eax1960;
    signed char* eax1961;
    signed char al1962;
    signed char* eax1963;
    signed char* eax1964;
    signed char al1965;
    signed char* eax1966;
    signed char* eax1967;
    signed char al1968;
    signed char* eax1969;
    signed char* eax1970;
    signed char al1971;
    signed char* eax1972;
    signed char* eax1973;
    signed char al1974;
    signed char* eax1975;
    signed char* eax1976;
    signed char al1977;
    signed char* eax1978;
    signed char* eax1979;
    signed char al1980;
    signed char* eax1981;
    signed char* eax1982;
    signed char al1983;
    signed char* eax1984;
    signed char* eax1985;
    signed char al1986;
    signed char* eax1987;
    signed char* eax1988;
    signed char al1989;
    signed char* eax1990;
    signed char* eax1991;
    signed char al1992;
    signed char* eax1993;
    signed char* eax1994;
    signed char al1995;
    signed char* eax1996;
    signed char* eax1997;
    signed char al1998;
    signed char* eax1999;
    signed char* eax2000;
    signed char al2001;
    signed char* eax2002;
    signed char* eax2003;
    signed char al2004;
    signed char* eax2005;
    signed char* eax2006;
    signed char al2007;
    signed char* eax2008;
    signed char* eax2009;
    signed char al2010;
    signed char* eax2011;
    signed char* eax2012;
    signed char al2013;
    signed char* eax2014;
    signed char* eax2015;
    signed char al2016;
    signed char* eax2017;
    signed char* eax2018;
    signed char al2019;
    signed char* eax2020;
    signed char* eax2021;
    signed char al2022;
    signed char* eax2023;
    signed char* eax2024;
    signed char al2025;
    signed char* eax2026;
    signed char* eax2027;
    signed char al2028;
    signed char* eax2029;
    signed char* eax2030;
    signed char al2031;
    signed char* eax2032;
    signed char* eax2033;
    signed char al2034;
    signed char* eax2035;
    signed char* eax2036;
    signed char al2037;
    signed char* eax2038;
    signed char* eax2039;
    signed char al2040;
    signed char* eax2041;
    signed char* eax2042;
    signed char al2043;
    signed char* eax2044;
    signed char* eax2045;
    signed char al2046;
    signed char* eax2047;
    signed char* eax2048;
    signed char al2049;
    signed char* eax2050;
    signed char* eax2051;
    signed char al2052;
    signed char* eax2053;
    signed char* eax2054;
    signed char al2055;
    signed char* eax2056;
    signed char* eax2057;
    signed char al2058;
    signed char* eax2059;
    signed char* eax2060;
    signed char al2061;
    signed char* eax2062;
    signed char* eax2063;
    signed char al2064;
    signed char* eax2065;
    signed char* eax2066;
    signed char al2067;
    signed char* eax2068;
    signed char* eax2069;
    signed char al2070;
    signed char* eax2071;
    signed char* eax2072;
    signed char al2073;
    signed char* eax2074;
    signed char* eax2075;
    signed char al2076;
    signed char* eax2077;
    signed char* eax2078;
    signed char al2079;
    signed char* eax2080;
    signed char* eax2081;
    signed char al2082;
    signed char* eax2083;
    signed char* eax2084;
    signed char al2085;
    signed char* eax2086;
    signed char* eax2087;
    signed char al2088;
    signed char* eax2089;
    signed char* eax2090;
    signed char al2091;
    signed char* eax2092;
    signed char* eax2093;
    signed char al2094;
    signed char* eax2095;
    signed char* eax2096;
    signed char al2097;
    signed char* eax2098;
    signed char* eax2099;
    signed char al2100;
    signed char* eax2101;
    signed char* eax2102;
    signed char al2103;
    signed char* eax2104;
    signed char* eax2105;
    signed char al2106;
    signed char* eax2107;
    signed char* eax2108;
    signed char al2109;
    signed char* eax2110;
    signed char* eax2111;
    signed char al2112;
    signed char* eax2113;
    signed char* eax2114;
    signed char al2115;
    signed char* eax2116;
    signed char* eax2117;
    signed char al2118;
    signed char* eax2119;
    signed char* eax2120;
    signed char al2121;
    signed char* eax2122;
    signed char* eax2123;
    signed char al2124;
    signed char* eax2125;
    signed char* eax2126;
    signed char al2127;
    signed char* eax2128;
    signed char* eax2129;
    signed char al2130;
    signed char* eax2131;
    signed char* eax2132;
    signed char al2133;
    signed char* eax2134;
    signed char* eax2135;
    signed char al2136;
    signed char* eax2137;
    signed char* eax2138;
    signed char al2139;
    signed char* eax2140;
    signed char* eax2141;
    signed char al2142;
    signed char* eax2143;
    signed char* eax2144;
    signed char al2145;
    signed char* eax2146;
    signed char* eax2147;
    signed char al2148;
    signed char* eax2149;
    signed char* eax2150;
    signed char al2151;
    signed char* eax2152;
    signed char* eax2153;
    signed char al2154;
    signed char* eax2155;
    signed char* eax2156;
    signed char al2157;
    signed char* eax2158;
    signed char* eax2159;
    signed char al2160;
    signed char* eax2161;
    signed char* eax2162;
    signed char al2163;
    signed char* eax2164;
    signed char* eax2165;
    signed char al2166;
    signed char* eax2167;
    signed char* eax2168;
    signed char al2169;
    signed char* eax2170;
    signed char* eax2171;
    signed char al2172;
    signed char* eax2173;
    signed char* eax2174;
    signed char al2175;
    signed char* eax2176;
    signed char* eax2177;
    signed char al2178;
    signed char* eax2179;
    signed char* eax2180;
    signed char al2181;
    signed char* eax2182;
    signed char* eax2183;
    signed char al2184;
    signed char* eax2185;
    signed char* eax2186;
    signed char al2187;
    signed char* eax2188;
    signed char* eax2189;
    signed char al2190;
    signed char* eax2191;
    signed char* eax2192;
    signed char al2193;
    signed char* eax2194;
    signed char* eax2195;
    signed char al2196;
    signed char* eax2197;
    signed char* eax2198;
    signed char al2199;
    signed char* eax2200;
    signed char* eax2201;
    signed char al2202;
    signed char* eax2203;
    signed char* eax2204;
    signed char al2205;
    signed char* eax2206;
    signed char* eax2207;
    signed char al2208;
    signed char* eax2209;
    signed char* eax2210;
    signed char al2211;
    signed char* eax2212;
    signed char* eax2213;
    signed char al2214;
    signed char* eax2215;
    signed char* eax2216;
    signed char al2217;
    signed char* eax2218;
    signed char* eax2219;
    signed char al2220;
    signed char* eax2221;
    signed char* eax2222;
    signed char al2223;
    signed char* eax2224;
    signed char* eax2225;
    signed char al2226;
    signed char* eax2227;
    signed char* eax2228;
    signed char al2229;
    signed char* eax2230;
    signed char* eax2231;
    signed char al2232;
    signed char* eax2233;
    signed char* eax2234;
    signed char al2235;
    signed char* eax2236;
    signed char* eax2237;
    signed char al2238;
    signed char* eax2239;
    signed char* eax2240;
    signed char al2241;
    signed char* eax2242;
    signed char* eax2243;
    signed char al2244;
    signed char* eax2245;
    signed char* eax2246;
    signed char al2247;
    signed char* eax2248;
    signed char* eax2249;
    signed char al2250;
    signed char* eax2251;
    signed char* eax2252;
    signed char al2253;
    signed char* eax2254;
    signed char* eax2255;
    signed char al2256;
    signed char* eax2257;
    signed char* eax2258;
    signed char al2259;
    signed char* eax2260;
    signed char* eax2261;
    signed char al2262;
    signed char* eax2263;
    signed char* eax2264;
    signed char al2265;
    signed char* eax2266;
    signed char* eax2267;
    signed char al2268;
    signed char* eax2269;
    signed char* eax2270;
    signed char al2271;
    signed char* eax2272;
    signed char* eax2273;
    signed char al2274;
    signed char* eax2275;
    signed char* eax2276;
    signed char al2277;
    signed char* eax2278;
    signed char* eax2279;
    signed char al2280;
    signed char* eax2281;
    signed char* eax2282;
    signed char al2283;
    signed char* eax2284;
    signed char* eax2285;
    signed char al2286;
    signed char* eax2287;
    signed char* eax2288;
    signed char al2289;
    signed char* eax2290;
    signed char* eax2291;
    signed char al2292;
    signed char* eax2293;
    signed char* eax2294;
    signed char al2295;
    signed char* eax2296;
    signed char* eax2297;
    signed char al2298;
    signed char* eax2299;
    signed char* eax2300;
    signed char al2301;
    signed char* eax2302;
    signed char* eax2303;
    signed char al2304;
    signed char* eax2305;
    signed char* eax2306;
    signed char al2307;
    signed char* eax2308;
    signed char* eax2309;
    signed char al2310;
    signed char* eax2311;
    signed char* eax2312;
    signed char al2313;
    signed char* eax2314;
    signed char* eax2315;
    signed char al2316;
    signed char* eax2317;
    signed char* eax2318;
    signed char al2319;
    signed char* eax2320;
    signed char* eax2321;
    signed char al2322;
    signed char* eax2323;
    signed char* eax2324;
    signed char al2325;
    signed char* eax2326;
    signed char* eax2327;
    signed char al2328;
    signed char* eax2329;
    signed char* eax2330;
    signed char al2331;
    signed char* eax2332;
    signed char* eax2333;
    signed char al2334;
    signed char* eax2335;
    signed char* eax2336;
    signed char al2337;
    signed char* eax2338;
    signed char* eax2339;
    signed char al2340;
    signed char* eax2341;
    signed char* eax2342;
    signed char al2343;
    signed char* eax2344;
    signed char* eax2345;
    signed char al2346;
    signed char* eax2347;
    signed char* eax2348;
    signed char al2349;
    signed char* eax2350;
    signed char* eax2351;
    signed char al2352;
    signed char* eax2353;
    signed char* eax2354;
    signed char al2355;
    signed char* eax2356;
    signed char* eax2357;
    signed char al2358;
    signed char* eax2359;
    signed char* eax2360;
    signed char al2361;
    signed char* eax2362;
    signed char* eax2363;
    signed char al2364;
    signed char* eax2365;
    signed char* eax2366;
    signed char al2367;
    signed char* eax2368;
    signed char* eax2369;
    signed char al2370;
    signed char* eax2371;
    signed char* eax2372;
    signed char al2373;
    signed char* eax2374;
    signed char* eax2375;
    signed char al2376;
    signed char* eax2377;
    signed char* eax2378;
    signed char al2379;
    signed char* eax2380;
    signed char* eax2381;
    signed char al2382;
    signed char* eax2383;
    signed char* eax2384;
    signed char al2385;
    signed char* eax2386;
    signed char* eax2387;
    signed char al2388;
    signed char* eax2389;
    signed char* eax2390;
    signed char al2391;
    signed char* eax2392;
    signed char* eax2393;
    signed char al2394;
    signed char* eax2395;
    signed char* eax2396;
    signed char al2397;
    signed char* eax2398;
    signed char* eax2399;
    signed char al2400;
    signed char* eax2401;
    signed char* eax2402;
    signed char al2403;
    signed char* eax2404;
    signed char* eax2405;
    signed char al2406;
    signed char* eax2407;
    signed char* eax2408;
    signed char al2409;
    signed char* eax2410;
    signed char* eax2411;
    signed char al2412;
    signed char* eax2413;
    signed char* eax2414;
    signed char al2415;
    signed char* eax2416;
    signed char* eax2417;
    signed char al2418;
    signed char* eax2419;
    signed char* eax2420;
    signed char al2421;
    signed char* eax2422;
    signed char* eax2423;
    signed char al2424;
    signed char* eax2425;
    signed char* eax2426;
    signed char al2427;
    signed char* eax2428;
    signed char* eax2429;
    signed char al2430;
    signed char* eax2431;
    signed char* eax2432;
    signed char al2433;
    signed char* eax2434;
    signed char* eax2435;
    signed char al2436;
    signed char* eax2437;
    signed char* eax2438;
    signed char al2439;
    signed char* eax2440;
    signed char* eax2441;
    signed char al2442;
    signed char* eax2443;
    signed char* eax2444;
    signed char al2445;
    signed char* eax2446;
    signed char* eax2447;
    signed char al2448;
    signed char* eax2449;
    signed char* eax2450;
    signed char al2451;
    signed char* eax2452;
    signed char* eax2453;
    signed char al2454;
    signed char* eax2455;
    signed char* eax2456;
    signed char al2457;
    signed char* eax2458;
    signed char* eax2459;
    signed char al2460;
    signed char* eax2461;
    signed char* eax2462;
    signed char al2463;
    signed char* eax2464;
    signed char* eax2465;
    signed char al2466;
    signed char* eax2467;
    signed char* eax2468;
    signed char al2469;
    signed char* eax2470;
    signed char* eax2471;
    signed char al2472;
    signed char* eax2473;
    signed char* eax2474;
    signed char al2475;
    signed char* eax2476;
    signed char* eax2477;
    signed char al2478;
    signed char* eax2479;
    signed char* eax2480;
    signed char al2481;
    signed char* eax2482;
    signed char* eax2483;
    signed char al2484;
    signed char* eax2485;
    signed char* eax2486;
    signed char al2487;
    signed char* eax2488;
    signed char* eax2489;
    signed char al2490;
    signed char* eax2491;
    signed char* eax2492;
    signed char al2493;
    signed char* eax2494;
    signed char* eax2495;
    signed char al2496;
    signed char* eax2497;
    signed char* eax2498;
    signed char al2499;
    signed char* eax2500;
    signed char* eax2501;
    signed char al2502;
    signed char* eax2503;
    signed char* eax2504;
    signed char al2505;
    signed char* eax2506;
    signed char* eax2507;
    signed char al2508;
    signed char* eax2509;
    signed char* eax2510;
    signed char al2511;
    signed char* eax2512;
    signed char* eax2513;
    signed char al2514;
    signed char* eax2515;
    signed char* eax2516;
    signed char al2517;
    signed char* eax2518;
    signed char* eax2519;
    signed char al2520;
    signed char* eax2521;
    signed char* eax2522;
    signed char al2523;
    signed char* eax2524;
    signed char* eax2525;
    signed char al2526;
    signed char* eax2527;
    signed char* eax2528;
    signed char al2529;
    signed char* eax2530;
    signed char* eax2531;
    signed char al2532;
    signed char* eax2533;
    signed char* eax2534;
    signed char al2535;
    signed char* eax2536;
    signed char* eax2537;
    signed char al2538;
    signed char* eax2539;
    signed char* eax2540;
    signed char al2541;
    signed char* eax2542;
    signed char* eax2543;
    signed char al2544;
    signed char* eax2545;
    signed char* eax2546;
    signed char al2547;
    signed char* eax2548;
    signed char* eax2549;
    signed char al2550;
    signed char* eax2551;
    signed char* eax2552;
    signed char al2553;
    signed char* eax2554;
    signed char* eax2555;
    signed char al2556;
    signed char* eax2557;
    signed char* eax2558;
    signed char al2559;
    signed char* eax2560;
    signed char* eax2561;
    signed char al2562;
    signed char* eax2563;
    signed char* eax2564;
    signed char al2565;
    signed char* eax2566;
    signed char* eax2567;
    signed char al2568;
    signed char* eax2569;
    signed char* eax2570;
    signed char al2571;
    signed char* eax2572;
    signed char* eax2573;
    signed char al2574;
    signed char* eax2575;
    signed char* eax2576;
    signed char al2577;
    signed char* eax2578;
    signed char* eax2579;
    signed char al2580;
    signed char* eax2581;
    signed char* eax2582;
    signed char al2583;
    signed char* eax2584;
    signed char* eax2585;
    signed char al2586;
    signed char* eax2587;
    signed char* eax2588;
    signed char al2589;
    signed char* eax2590;
    signed char* eax2591;
    signed char al2592;
    signed char* eax2593;
    signed char* eax2594;
    signed char al2595;
    signed char* eax2596;
    signed char* eax2597;
    signed char al2598;
    signed char* eax2599;
    signed char* eax2600;
    signed char al2601;
    signed char* eax2602;
    signed char* eax2603;
    signed char al2604;
    signed char* eax2605;
    signed char* eax2606;
    signed char al2607;
    signed char* eax2608;
    signed char* eax2609;
    signed char al2610;
    signed char* eax2611;
    signed char* eax2612;
    signed char al2613;
    signed char* eax2614;
    signed char* eax2615;
    signed char al2616;
    signed char* eax2617;
    signed char* eax2618;
    signed char al2619;
    signed char* eax2620;
    signed char* eax2621;
    signed char al2622;
    signed char* eax2623;
    signed char* eax2624;
    signed char al2625;
    signed char* eax2626;
    signed char* eax2627;
    signed char al2628;
    signed char* eax2629;
    signed char* eax2630;
    signed char al2631;
    signed char* eax2632;
    signed char* eax2633;
    signed char al2634;
    signed char* eax2635;
    signed char* eax2636;
    signed char al2637;
    signed char* eax2638;
    signed char* eax2639;
    signed char al2640;
    signed char* eax2641;
    signed char* eax2642;
    signed char al2643;
    signed char* eax2644;
    signed char* eax2645;
    signed char al2646;
    signed char* eax2647;
    signed char* eax2648;
    signed char al2649;
    signed char* eax2650;
    signed char* eax2651;
    signed char al2652;
    signed char* eax2653;
    signed char* eax2654;
    signed char al2655;
    signed char* eax2656;
    signed char* eax2657;
    signed char al2658;
    signed char* eax2659;
    signed char* eax2660;
    signed char al2661;
    signed char* eax2662;
    signed char* eax2663;
    signed char al2664;
    signed char* eax2665;
    signed char* eax2666;
    signed char al2667;
    signed char* eax2668;
    signed char* eax2669;
    signed char al2670;
    signed char* eax2671;
    signed char* eax2672;
    signed char al2673;
    signed char* eax2674;
    signed char* eax2675;
    signed char al2676;
    signed char* eax2677;
    signed char* eax2678;
    signed char al2679;
    signed char* eax2680;
    signed char* eax2681;
    signed char al2682;
    signed char* eax2683;
    signed char* eax2684;
    signed char al2685;
    signed char* eax2686;
    signed char* eax2687;
    signed char al2688;
    signed char* eax2689;
    signed char* eax2690;
    signed char al2691;
    signed char* eax2692;
    signed char* eax2693;
    signed char al2694;
    signed char* eax2695;
    signed char* eax2696;
    signed char al2697;
    signed char* eax2698;
    signed char* eax2699;
    signed char al2700;
    signed char* eax2701;
    signed char* eax2702;
    signed char al2703;
    signed char* eax2704;
    signed char* eax2705;
    signed char al2706;
    signed char* eax2707;
    signed char* eax2708;
    signed char al2709;
    signed char* eax2710;
    signed char* eax2711;
    signed char al2712;
    signed char* eax2713;
    signed char* eax2714;
    signed char al2715;
    signed char* eax2716;
    signed char* eax2717;
    signed char al2718;
    signed char* eax2719;
    signed char* eax2720;
    signed char al2721;
    signed char* eax2722;
    signed char* eax2723;
    signed char al2724;
    signed char* eax2725;
    signed char* eax2726;
    signed char al2727;
    signed char* eax2728;
    signed char* eax2729;
    signed char al2730;
    signed char* eax2731;
    signed char* eax2732;
    signed char al2733;
    signed char* eax2734;
    signed char* eax2735;
    signed char al2736;
    signed char* eax2737;
    signed char* eax2738;
    signed char al2739;
    signed char* eax2740;
    signed char* eax2741;
    signed char al2742;
    signed char* eax2743;
    signed char* eax2744;
    signed char al2745;
    signed char* eax2746;
    signed char* eax2747;
    signed char al2748;
    signed char* eax2749;
    signed char* eax2750;
    signed char al2751;
    signed char* eax2752;
    signed char* eax2753;
    signed char al2754;
    signed char* eax2755;
    signed char* eax2756;
    signed char al2757;
    signed char* eax2758;
    signed char* eax2759;
    signed char al2760;
    signed char* eax2761;
    signed char* eax2762;
    signed char al2763;
    signed char* eax2764;
    signed char* eax2765;
    signed char al2766;
    signed char* eax2767;
    signed char* eax2768;
    signed char al2769;
    signed char* eax2770;
    signed char* eax2771;
    signed char al2772;
    signed char* eax2773;
    signed char* eax2774;
    signed char al2775;
    signed char* eax2776;
    signed char* eax2777;
    signed char al2778;
    signed char* eax2779;
    signed char* eax2780;
    signed char al2781;
    signed char* eax2782;
    signed char* eax2783;
    signed char al2784;
    signed char* eax2785;
    signed char* eax2786;
    signed char al2787;
    signed char* eax2788;
    signed char* eax2789;
    signed char al2790;
    signed char* eax2791;
    signed char* eax2792;
    signed char al2793;
    signed char* eax2794;
    signed char* eax2795;
    signed char al2796;
    signed char* eax2797;
    signed char* eax2798;
    signed char al2799;
    signed char* eax2800;
    signed char* eax2801;
    signed char al2802;
    signed char* eax2803;
    signed char* eax2804;
    signed char al2805;
    signed char* eax2806;
    signed char* eax2807;
    signed char al2808;
    signed char* eax2809;
    signed char* eax2810;
    signed char al2811;
    signed char* eax2812;
    signed char* eax2813;
    signed char al2814;
    signed char* eax2815;
    signed char* eax2816;
    signed char al2817;
    signed char* eax2818;
    signed char* eax2819;
    signed char al2820;
    signed char* eax2821;
    signed char* eax2822;
    signed char al2823;
    signed char* eax2824;
    signed char* eax2825;
    signed char al2826;
    signed char* eax2827;
    signed char* eax2828;
    signed char al2829;
    signed char* eax2830;
    signed char* eax2831;
    signed char al2832;
    signed char* eax2833;
    signed char* eax2834;
    signed char al2835;
    signed char* eax2836;
    signed char* eax2837;
    signed char al2838;
    signed char* eax2839;
    signed char* eax2840;
    signed char al2841;
    signed char* eax2842;
    signed char* eax2843;
    signed char al2844;
    signed char* eax2845;
    signed char* eax2846;
    signed char al2847;
    signed char* eax2848;
    signed char* eax2849;
    signed char al2850;
    signed char* eax2851;
    signed char* eax2852;
    signed char al2853;
    signed char* eax2854;
    signed char* eax2855;
    signed char al2856;
    signed char* eax2857;
    signed char* eax2858;
    signed char al2859;
    signed char* eax2860;
    signed char* eax2861;
    signed char al2862;
    signed char* eax2863;
    signed char* eax2864;
    signed char al2865;
    signed char* eax2866;
    signed char* eax2867;
    signed char al2868;
    signed char* eax2869;
    signed char* eax2870;
    signed char al2871;
    signed char* eax2872;
    signed char* eax2873;
    signed char al2874;
    signed char* eax2875;
    signed char* eax2876;
    signed char al2877;
    signed char* eax2878;
    signed char* eax2879;
    signed char al2880;
    signed char* eax2881;
    signed char* eax2882;
    signed char al2883;
    signed char* eax2884;
    signed char* eax2885;
    signed char al2886;
    signed char* eax2887;
    signed char* eax2888;
    signed char al2889;
    signed char* eax2890;
    signed char* eax2891;
    signed char al2892;
    signed char* eax2893;
    signed char* eax2894;
    signed char al2895;
    signed char* eax2896;
    signed char* eax2897;
    signed char al2898;
    signed char* eax2899;
    signed char* eax2900;
    signed char al2901;
    signed char* eax2902;
    signed char* eax2903;
    signed char al2904;
    signed char* eax2905;
    signed char* eax2906;
    signed char al2907;
    signed char* eax2908;
    signed char* eax2909;
    signed char al2910;
    signed char* eax2911;
    signed char* eax2912;
    signed char al2913;
    signed char* eax2914;
    signed char* eax2915;
    signed char al2916;
    signed char* eax2917;
    signed char* eax2918;
    signed char al2919;
    signed char* eax2920;
    signed char* eax2921;
    signed char al2922;
    signed char* eax2923;
    signed char* eax2924;
    signed char al2925;
    signed char* eax2926;
    signed char* eax2927;
    signed char al2928;
    signed char* eax2929;
    signed char* eax2930;
    signed char al2931;
    signed char* eax2932;
    signed char* eax2933;
    signed char al2934;
    signed char* eax2935;
    signed char* eax2936;
    signed char al2937;
    signed char* eax2938;
    signed char* eax2939;
    signed char al2940;
    signed char* eax2941;
    signed char* eax2942;
    signed char al2943;
    signed char* eax2944;
    signed char* eax2945;
    signed char al2946;
    signed char* eax2947;
    signed char* eax2948;
    signed char al2949;
    signed char* eax2950;
    signed char* eax2951;
    signed char al2952;
    signed char* eax2953;
    signed char* eax2954;
    signed char al2955;
    signed char* eax2956;
    signed char* eax2957;
    signed char al2958;
    signed char* eax2959;
    signed char* eax2960;
    signed char al2961;
    signed char* eax2962;
    signed char* eax2963;
    signed char al2964;
    signed char* eax2965;
    signed char* eax2966;
    signed char al2967;
    signed char* eax2968;
    signed char* eax2969;
    signed char al2970;
    signed char* eax2971;
    signed char* eax2972;
    signed char al2973;
    signed char* eax2974;
    signed char* eax2975;
    signed char al2976;
    signed char* eax2977;
    signed char* eax2978;
    signed char al2979;
    signed char* eax2980;
    signed char* eax2981;
    signed char al2982;
    signed char* eax2983;
    signed char* eax2984;
    signed char al2985;
    signed char* eax2986;
    signed char* eax2987;
    signed char al2988;
    signed char* eax2989;
    signed char* eax2990;
    signed char al2991;
    signed char* eax2992;
    signed char* eax2993;
    signed char al2994;
    signed char* eax2995;
    signed char* eax2996;
    signed char al2997;
    signed char* eax2998;
    signed char* eax2999;
    signed char al3000;
    signed char* eax3001;
    signed char* eax3002;
    signed char al3003;
    signed char* eax3004;
    signed char* eax3005;
    signed char al3006;
    signed char* eax3007;
    signed char* eax3008;
    signed char al3009;
    signed char* eax3010;
    signed char* eax3011;
    signed char al3012;
    signed char* eax3013;
    signed char* eax3014;
    signed char al3015;
    signed char* eax3016;
    signed char* eax3017;
    signed char al3018;
    signed char* eax3019;
    signed char* eax3020;
    signed char al3021;
    signed char* eax3022;
    signed char* eax3023;
    signed char al3024;
    signed char* eax3025;
    signed char* eax3026;
    signed char al3027;
    signed char* eax3028;
    signed char* eax3029;
    signed char al3030;
    signed char* eax3031;
    signed char* eax3032;
    signed char al3033;
    signed char* eax3034;
    signed char* eax3035;
    signed char al3036;
    signed char* eax3037;
    signed char* eax3038;
    signed char al3039;
    signed char* eax3040;
    signed char* eax3041;
    signed char al3042;
    signed char* eax3043;
    signed char* eax3044;
    signed char al3045;
    signed char* eax3046;
    signed char* eax3047;
    signed char al3048;
    signed char* eax3049;
    signed char* eax3050;
    signed char al3051;
    signed char* eax3052;
    signed char* eax3053;
    signed char al3054;
    signed char* eax3055;
    signed char* eax3056;
    signed char al3057;
    signed char* eax3058;
    signed char* eax3059;
    signed char al3060;
    signed char* eax3061;
    signed char* eax3062;
    signed char al3063;
    signed char* eax3064;
    signed char* eax3065;
    signed char al3066;
    signed char* eax3067;
    signed char* eax3068;
    signed char al3069;
    signed char* eax3070;
    signed char* eax3071;
    signed char al3072;
    signed char* eax3073;
    signed char* eax3074;
    signed char al3075;
    signed char* eax3076;
    signed char* eax3077;
    signed char al3078;
    signed char* eax3079;
    signed char* eax3080;
    signed char al3081;
    signed char* eax3082;
    signed char* eax3083;
    signed char al3084;
    signed char* eax3085;
    signed char* eax3086;
    signed char al3087;
    signed char* eax3088;
    signed char* eax3089;
    signed char al3090;
    signed char* eax3091;
    signed char* eax3092;
    signed char al3093;
    signed char* eax3094;
    signed char* eax3095;
    signed char al3096;
    signed char* eax3097;
    signed char* eax3098;
    signed char al3099;
    signed char* eax3100;
    signed char* eax3101;
    signed char al3102;
    signed char* eax3103;
    signed char* eax3104;
    signed char al3105;
    signed char* eax3106;
    signed char* eax3107;
    signed char al3108;
    signed char* eax3109;
    signed char* eax3110;
    signed char al3111;
    signed char* eax3112;
    signed char* eax3113;
    signed char al3114;
    signed char* eax3115;
    signed char* eax3116;
    signed char al3117;
    signed char* eax3118;
    signed char* eax3119;
    signed char al3120;
    signed char* eax3121;
    signed char* eax3122;
    signed char al3123;
    signed char* eax3124;
    signed char* eax3125;
    signed char al3126;
    signed char* eax3127;
    signed char* eax3128;
    signed char al3129;
    signed char* eax3130;
    signed char* eax3131;
    signed char al3132;
    signed char* eax3133;
    signed char* eax3134;
    signed char al3135;
    signed char* eax3136;
    signed char* eax3137;
    signed char al3138;
    signed char* eax3139;
    signed char* eax3140;
    signed char al3141;
    signed char* eax3142;
    signed char* eax3143;
    signed char al3144;
    signed char* eax3145;
    signed char* eax3146;
    signed char al3147;
    signed char* eax3148;
    signed char* eax3149;
    signed char al3150;
    signed char* eax3151;
    signed char* eax3152;
    signed char al3153;
    signed char* eax3154;
    signed char* eax3155;
    signed char al3156;
    signed char* eax3157;
    signed char* eax3158;
    signed char al3159;
    signed char* eax3160;
    signed char* eax3161;
    signed char al3162;
    signed char* eax3163;
    signed char* eax3164;
    signed char al3165;
    signed char* eax3166;
    signed char* eax3167;
    signed char al3168;
    signed char* eax3169;
    signed char* eax3170;
    signed char al3171;
    signed char* eax3172;
    signed char* eax3173;
    signed char al3174;
    signed char* eax3175;
    signed char* eax3176;
    signed char al3177;
    signed char* eax3178;
    signed char* eax3179;
    signed char al3180;
    signed char* eax3181;
    signed char* eax3182;
    signed char al3183;
    signed char* eax3184;
    signed char* eax3185;
    signed char al3186;
    signed char* eax3187;
    signed char* eax3188;
    signed char al3189;
    signed char* eax3190;
    signed char* eax3191;
    signed char al3192;
    signed char* eax3193;
    signed char* eax3194;
    signed char al3195;
    signed char* eax3196;
    signed char* eax3197;
    signed char al3198;
    signed char* eax3199;
    signed char* eax3200;
    signed char al3201;
    signed char* eax3202;
    signed char* eax3203;
    signed char al3204;
    signed char* eax3205;
    signed char* eax3206;
    signed char al3207;
    signed char* eax3208;
    signed char* eax3209;
    signed char al3210;
    signed char* eax3211;
    signed char* eax3212;
    signed char al3213;
    signed char* eax3214;
    signed char* eax3215;
    signed char al3216;
    signed char* eax3217;
    signed char* eax3218;
    signed char al3219;
    signed char* eax3220;
    signed char* eax3221;
    signed char al3222;
    signed char* eax3223;
    signed char* eax3224;
    signed char al3225;
    signed char* eax3226;
    signed char* eax3227;
    signed char al3228;
    signed char* eax3229;
    signed char* eax3230;
    signed char al3231;
    signed char* eax3232;
    signed char* eax3233;
    signed char al3234;
    signed char* eax3235;
    signed char* eax3236;
    signed char al3237;
    signed char* eax3238;
    signed char* eax3239;
    signed char al3240;
    signed char* eax3241;
    signed char* eax3242;
    signed char al3243;
    signed char* eax3244;
    signed char* eax3245;
    signed char al3246;
    signed char* eax3247;
    signed char* eax3248;
    signed char al3249;
    signed char* eax3250;
    signed char* eax3251;
    signed char al3252;
    signed char* eax3253;
    signed char* eax3254;
    signed char al3255;
    signed char* eax3256;
    signed char* eax3257;
    signed char al3258;
    signed char* eax3259;
    signed char* eax3260;
    signed char al3261;
    signed char* eax3262;
    signed char* eax3263;
    signed char al3264;
    signed char* eax3265;
    signed char* eax3266;
    signed char al3267;
    signed char* eax3268;
    signed char* eax3269;
    signed char al3270;
    signed char* eax3271;
    signed char* eax3272;
    signed char al3273;
    signed char* eax3274;
    signed char* eax3275;
    signed char al3276;
    signed char* eax3277;
    signed char* eax3278;
    signed char al3279;
    signed char* eax3280;
    signed char* eax3281;
    signed char al3282;
    signed char* eax3283;
    signed char* eax3284;
    signed char al3285;
    signed char* eax3286;
    signed char* eax3287;
    signed char al3288;
    signed char* eax3289;
    signed char* eax3290;
    signed char al3291;
    signed char* eax3292;
    signed char* eax3293;
    signed char al3294;
    signed char* eax3295;
    signed char* eax3296;
    signed char al3297;
    signed char* eax3298;
    signed char* eax3299;
    signed char al3300;
    signed char* eax3301;
    signed char* eax3302;
    signed char al3303;
    signed char* eax3304;
    signed char* eax3305;
    signed char al3306;
    signed char* eax3307;
    signed char* eax3308;
    signed char al3309;
    signed char* eax3310;
    signed char* eax3311;
    signed char al3312;
    signed char* eax3313;
    signed char* eax3314;
    signed char al3315;
    signed char* eax3316;
    signed char* eax3317;
    signed char al3318;
    signed char* eax3319;
    signed char* eax3320;
    signed char al3321;
    signed char* eax3322;
    signed char* eax3323;
    signed char al3324;
    signed char* eax3325;
    signed char* eax3326;
    signed char al3327;
    signed char* eax3328;
    signed char* eax3329;
    signed char al3330;
    signed char* eax3331;
    signed char* eax3332;
    signed char al3333;
    signed char* eax3334;
    signed char* eax3335;
    signed char al3336;
    signed char* eax3337;
    signed char* eax3338;
    signed char al3339;
    signed char* eax3340;
    signed char* eax3341;
    signed char al3342;
    signed char* eax3343;
    signed char* eax3344;
    signed char al3345;
    signed char* eax3346;
    signed char* eax3347;
    signed char al3348;
    signed char* eax3349;
    signed char* eax3350;
    signed char al3351;
    signed char* eax3352;
    signed char* eax3353;
    signed char al3354;
    signed char* eax3355;
    signed char* eax3356;
    signed char al3357;
    signed char* eax3358;
    signed char* eax3359;
    signed char al3360;
    signed char* eax3361;
    signed char* eax3362;
    signed char al3363;
    signed char* eax3364;
    signed char* eax3365;
    signed char al3366;
    signed char* eax3367;
    signed char* eax3368;
    signed char al3369;
    signed char* eax3370;
    signed char* eax3371;
    signed char al3372;
    signed char* eax3373;
    signed char* eax3374;
    signed char al3375;
    signed char* eax3376;
    signed char* eax3377;
    signed char al3378;
    signed char* eax3379;
    signed char* eax3380;
    signed char al3381;
    signed char* eax3382;
    signed char* eax3383;
    signed char al3384;
    signed char* eax3385;
    signed char* eax3386;
    signed char al3387;
    signed char* eax3388;
    signed char* eax3389;
    signed char al3390;
    signed char* eax3391;
    signed char* eax3392;
    signed char al3393;
    signed char* eax3394;
    signed char* eax3395;
    signed char al3396;
    signed char* eax3397;
    signed char* eax3398;
    signed char al3399;
    signed char* eax3400;
    signed char* eax3401;
    signed char al3402;
    signed char* eax3403;
    signed char* eax3404;
    signed char al3405;
    signed char* eax3406;
    signed char* eax3407;
    signed char al3408;
    signed char* eax3409;
    signed char* eax3410;
    signed char al3411;
    signed char* eax3412;
    signed char* eax3413;
    signed char al3414;
    signed char* eax3415;
    signed char* eax3416;
    signed char al3417;
    signed char* eax3418;
    signed char* eax3419;
    signed char al3420;
    signed char* eax3421;
    signed char* eax3422;
    signed char al3423;
    signed char* eax3424;
    signed char* eax3425;
    signed char al3426;
    signed char* eax3427;
    signed char* eax3428;
    signed char al3429;
    signed char* eax3430;
    signed char* eax3431;
    signed char al3432;
    signed char* eax3433;
    signed char* eax3434;
    signed char al3435;
    signed char* eax3436;
    signed char* eax3437;
    signed char al3438;
    signed char* eax3439;
    signed char* eax3440;
    signed char al3441;
    signed char* eax3442;
    signed char* eax3443;
    signed char al3444;
    signed char* eax3445;
    signed char* eax3446;
    signed char al3447;
    signed char* eax3448;
    signed char* eax3449;
    signed char al3450;
    signed char* eax3451;
    signed char* eax3452;
    signed char al3453;
    signed char* eax3454;
    signed char* eax3455;
    signed char al3456;
    signed char* eax3457;
    signed char* eax3458;
    signed char al3459;
    signed char* eax3460;
    signed char* eax3461;
    signed char al3462;
    signed char* eax3463;
    signed char* eax3464;
    signed char al3465;
    signed char* eax3466;
    signed char* eax3467;
    signed char al3468;
    signed char* eax3469;
    signed char* eax3470;
    signed char al3471;
    signed char* eax3472;
    signed char* eax3473;
    signed char al3474;
    signed char* eax3475;
    signed char* eax3476;
    signed char al3477;
    signed char* eax3478;
    signed char* eax3479;
    signed char al3480;
    signed char* eax3481;
    signed char* eax3482;
    signed char al3483;
    signed char* eax3484;
    signed char* eax3485;
    signed char al3486;
    signed char* eax3487;
    signed char* eax3488;
    signed char al3489;
    signed char* eax3490;
    signed char* eax3491;
    signed char al3492;
    signed char* eax3493;
    signed char* eax3494;
    signed char al3495;
    signed char* eax3496;
    signed char* eax3497;
    signed char al3498;
    signed char* eax3499;
    signed char* eax3500;
    signed char al3501;
    signed char* eax3502;
    signed char* eax3503;
    signed char al3504;
    signed char* eax3505;
    signed char* eax3506;
    signed char al3507;
    signed char* eax3508;
    signed char* eax3509;
    signed char al3510;
    signed char* eax3511;
    signed char* eax3512;
    signed char al3513;
    signed char* eax3514;
    signed char* eax3515;
    signed char al3516;
    signed char* eax3517;
    signed char* eax3518;
    signed char al3519;
    signed char* eax3520;
    signed char* eax3521;
    signed char al3522;
    signed char* eax3523;
    signed char* eax3524;
    signed char al3525;
    signed char* eax3526;
    signed char* eax3527;
    signed char al3528;
    signed char* eax3529;
    signed char* eax3530;
    signed char al3531;
    signed char* eax3532;
    signed char* eax3533;
    signed char al3534;
    signed char* eax3535;
    signed char* eax3536;
    signed char al3537;
    signed char* eax3538;
    signed char* eax3539;
    signed char al3540;
    signed char* eax3541;
    signed char* eax3542;
    signed char al3543;
    signed char* eax3544;
    signed char* eax3545;
    signed char al3546;
    signed char* eax3547;
    signed char* eax3548;
    signed char al3549;
    signed char* eax3550;
    signed char* eax3551;
    signed char al3552;
    signed char* eax3553;
    signed char* eax3554;
    signed char al3555;
    signed char* eax3556;
    signed char* eax3557;
    signed char al3558;
    signed char* eax3559;
    signed char* eax3560;
    signed char al3561;
    signed char* eax3562;
    signed char* eax3563;
    signed char al3564;
    signed char* eax3565;
    signed char* eax3566;
    signed char al3567;
    signed char* eax3568;
    signed char* eax3569;
    signed char al3570;
    signed char* eax3571;
    signed char* eax3572;
    signed char al3573;
    signed char* eax3574;
    signed char* eax3575;
    signed char al3576;
    signed char* eax3577;
    signed char* eax3578;
    signed char al3579;
    signed char* eax3580;
    signed char* eax3581;
    signed char al3582;
    signed char* eax3583;
    signed char* eax3584;
    signed char al3585;
    signed char* eax3586;
    signed char* eax3587;
    signed char al3588;
    signed char* eax3589;
    signed char* eax3590;
    signed char al3591;
    signed char* eax3592;
    signed char* eax3593;
    signed char al3594;
    signed char* eax3595;
    signed char* eax3596;
    signed char al3597;
    signed char* eax3598;
    signed char* eax3599;
    signed char al3600;
    signed char* eax3601;
    signed char* eax3602;
    signed char al3603;
    signed char* eax3604;
    signed char* eax3605;
    signed char al3606;
    signed char* eax3607;
    signed char* eax3608;
    signed char al3609;
    signed char* eax3610;
    signed char* eax3611;
    signed char al3612;
    signed char* eax3613;
    signed char* eax3614;
    signed char al3615;
    signed char* eax3616;
    signed char* eax3617;
    signed char al3618;
    signed char* eax3619;
    signed char* eax3620;
    signed char al3621;
    signed char* eax3622;
    signed char* eax3623;
    signed char al3624;
    signed char* eax3625;
    signed char* eax3626;
    signed char al3627;
    signed char* eax3628;
    signed char* eax3629;
    signed char al3630;
    signed char* eax3631;
    signed char* eax3632;
    signed char al3633;
    signed char* eax3634;
    signed char* eax3635;
    signed char al3636;
    signed char* eax3637;
    signed char* eax3638;
    signed char al3639;
    signed char* eax3640;
    signed char* eax3641;
    signed char al3642;
    signed char* eax3643;
    signed char* eax3644;
    signed char al3645;
    signed char* eax3646;
    signed char* eax3647;
    signed char al3648;
    signed char* eax3649;
    signed char* eax3650;
    signed char al3651;
    signed char* eax3652;
    signed char* eax3653;
    signed char al3654;
    signed char* eax3655;
    signed char* eax3656;
    signed char al3657;
    signed char* eax3658;
    signed char* eax3659;
    signed char al3660;
    signed char* eax3661;
    signed char* eax3662;
    signed char al3663;
    signed char* eax3664;
    signed char* eax3665;
    signed char al3666;
    signed char* eax3667;
    signed char* eax3668;
    signed char al3669;
    signed char* eax3670;
    signed char* eax3671;
    signed char al3672;
    signed char* eax3673;
    signed char* eax3674;
    signed char al3675;
    signed char* eax3676;
    signed char* eax3677;
    signed char al3678;
    signed char* eax3679;
    signed char* eax3680;
    signed char al3681;
    signed char* eax3682;
    signed char* eax3683;
    signed char al3684;
    signed char* eax3685;
    signed char* eax3686;
    signed char al3687;
    signed char* eax3688;
    signed char* eax3689;
    signed char al3690;
    signed char* eax3691;
    signed char* eax3692;
    signed char al3693;
    signed char* eax3694;
    signed char* eax3695;
    signed char al3696;
    signed char* eax3697;
    signed char* eax3698;
    signed char al3699;
    signed char* eax3700;
    signed char* eax3701;
    signed char al3702;
    signed char* eax3703;
    signed char* eax3704;
    signed char al3705;
    signed char* eax3706;
    signed char* eax3707;
    signed char al3708;
    signed char* eax3709;
    signed char* eax3710;
    signed char al3711;
    signed char* eax3712;
    signed char* eax3713;
    signed char al3714;
    signed char* eax3715;
    signed char* eax3716;
    signed char al3717;
    signed char* eax3718;
    signed char* eax3719;
    signed char al3720;
    signed char* eax3721;
    signed char* eax3722;
    signed char al3723;
    signed char* eax3724;
    signed char* eax3725;
    signed char al3726;
    signed char* eax3727;
    signed char* eax3728;
    signed char al3729;
    signed char* eax3730;
    signed char* eax3731;
    signed char al3732;
    signed char* eax3733;
    signed char* eax3734;
    signed char al3735;
    signed char* eax3736;
    signed char* eax3737;
    signed char al3738;
    signed char* eax3739;
    signed char* eax3740;
    signed char al3741;
    signed char* eax3742;
    signed char* eax3743;
    signed char al3744;
    signed char* eax3745;
    signed char* eax3746;
    signed char al3747;
    signed char* eax3748;
    signed char* eax3749;
    signed char al3750;
    signed char* eax3751;
    signed char* eax3752;
    signed char al3753;
    signed char* eax3754;
    signed char* eax3755;
    signed char al3756;
    signed char* eax3757;
    signed char* eax3758;
    signed char al3759;
    signed char* eax3760;
    signed char* eax3761;
    signed char al3762;
    signed char* eax3763;
    signed char* eax3764;
    signed char al3765;
    signed char* eax3766;
    signed char* eax3767;
    signed char al3768;
    signed char* eax3769;
    signed char* eax3770;
    signed char al3771;
    signed char* eax3772;
    signed char* eax3773;
    signed char al3774;
    signed char* eax3775;
    signed char* eax3776;
    signed char al3777;
    signed char* eax3778;
    signed char* eax3779;
    signed char al3780;
    signed char* eax3781;
    signed char* eax3782;
    signed char al3783;
    signed char* eax3784;
    signed char* eax3785;
    signed char al3786;
    signed char* eax3787;
    signed char* eax3788;
    signed char al3789;
    signed char* eax3790;
    signed char* eax3791;
    signed char al3792;
    signed char* eax3793;
    signed char* eax3794;
    signed char al3795;
    signed char* eax3796;
    signed char* eax3797;
    signed char al3798;
    signed char* eax3799;
    signed char* eax3800;
    signed char al3801;
    signed char* eax3802;
    signed char* eax3803;
    signed char al3804;
    signed char* eax3805;
    signed char* eax3806;
    signed char al3807;
    signed char* eax3808;
    signed char* eax3809;
    signed char al3810;
    signed char* eax3811;
    signed char* eax3812;
    signed char al3813;
    signed char* eax3814;
    signed char* eax3815;
    signed char al3816;
    signed char* eax3817;
    signed char* eax3818;
    signed char al3819;
    signed char* eax3820;
    signed char* eax3821;
    signed char al3822;
    signed char* eax3823;
    signed char* eax3824;
    signed char al3825;
    signed char* eax3826;
    signed char* eax3827;
    signed char al3828;
    signed char* eax3829;
    signed char* eax3830;
    signed char al3831;
    signed char* eax3832;
    signed char* eax3833;
    signed char al3834;
    signed char* eax3835;
    signed char* eax3836;
    signed char al3837;
    signed char* eax3838;
    signed char* eax3839;
    signed char al3840;
    signed char* eax3841;
    signed char* eax3842;
    signed char al3843;
    signed char* eax3844;
    signed char* eax3845;
    signed char al3846;
    signed char* eax3847;
    signed char* eax3848;
    signed char al3849;
    signed char* eax3850;
    signed char* eax3851;
    signed char al3852;
    signed char* eax3853;
    signed char* eax3854;
    signed char al3855;
    signed char* eax3856;
    signed char* eax3857;
    signed char al3858;
    signed char* eax3859;
    signed char* eax3860;
    signed char al3861;
    signed char* eax3862;
    signed char* eax3863;
    signed char al3864;
    signed char* eax3865;
    signed char* eax3866;
    signed char al3867;
    signed char* eax3868;
    signed char* eax3869;
    signed char al3870;
    signed char* eax3871;
    signed char* eax3872;
    signed char al3873;
    signed char* eax3874;
    signed char* eax3875;
    signed char al3876;
    signed char* eax3877;
    signed char* eax3878;
    signed char al3879;
    signed char* eax3880;
    signed char* eax3881;
    signed char al3882;
    signed char* eax3883;
    signed char* eax3884;
    signed char al3885;
    signed char* eax3886;
    signed char* eax3887;
    signed char al3888;
    signed char* eax3889;
    signed char* eax3890;
    signed char al3891;
    signed char* eax3892;
    signed char* eax3893;
    signed char al3894;
    signed char* eax3895;
    signed char* eax3896;
    signed char al3897;
    signed char* eax3898;
    signed char* eax3899;
    signed char al3900;
    signed char* eax3901;
    signed char* eax3902;
    signed char al3903;
    signed char* eax3904;
    signed char* eax3905;
    signed char al3906;
    signed char* eax3907;
    signed char* eax3908;
    signed char al3909;
    signed char* eax3910;
    signed char* eax3911;
    signed char al3912;
    signed char* eax3913;
    signed char* eax3914;
    signed char al3915;
    signed char* eax3916;
    signed char* eax3917;
    signed char al3918;
    signed char* eax3919;
    signed char* eax3920;
    signed char al3921;
    signed char* eax3922;
    signed char* eax3923;
    signed char al3924;
    signed char* eax3925;
    signed char* eax3926;
    signed char al3927;
    signed char* eax3928;
    signed char* eax3929;
    signed char al3930;
    signed char* eax3931;
    signed char* eax3932;
    signed char al3933;
    signed char* eax3934;
    signed char* eax3935;
    signed char al3936;
    signed char* eax3937;
    signed char* eax3938;
    signed char al3939;
    signed char* eax3940;
    signed char* eax3941;
    signed char al3942;
    signed char* eax3943;
    signed char* eax3944;
    signed char al3945;
    signed char* eax3946;
    signed char* eax3947;
    signed char al3948;
    signed char* eax3949;
    signed char* eax3950;
    signed char al3951;
    signed char* eax3952;
    signed char* eax3953;
    signed char al3954;
    signed char* eax3955;
    signed char* eax3956;
    signed char al3957;
    signed char* eax3958;
    signed char* eax3959;
    signed char al3960;
    signed char* eax3961;
    signed char* eax3962;
    signed char al3963;
    signed char* eax3964;
    signed char* eax3965;
    signed char al3966;
    signed char* eax3967;
    signed char* eax3968;
    signed char al3969;
    signed char* eax3970;
    signed char* eax3971;
    signed char al3972;
    signed char* eax3973;
    signed char* eax3974;
    signed char al3975;
    signed char* eax3976;
    signed char* eax3977;
    signed char al3978;
    signed char* eax3979;
    signed char* eax3980;
    signed char al3981;
    signed char* eax3982;
    signed char* eax3983;
    signed char al3984;
    signed char* eax3985;
    signed char* eax3986;
    signed char al3987;
    signed char* eax3988;
    signed char* eax3989;
    signed char al3990;
    signed char* eax3991;
    signed char* eax3992;
    signed char al3993;
    signed char* eax3994;
    signed char* eax3995;
    signed char al3996;
    signed char* eax3997;
    signed char* eax3998;
    signed char al3999;
    signed char* eax4000;
    signed char* eax4001;
    signed char al4002;
    signed char* eax4003;
    signed char* eax4004;
    signed char al4005;
    signed char* eax4006;
    signed char* eax4007;
    signed char al4008;
    signed char* eax4009;
    signed char* eax4010;
    signed char al4011;
    signed char* eax4012;
    signed char* eax4013;
    signed char al4014;
    signed char* eax4015;
    signed char* eax4016;
    signed char al4017;
    signed char* eax4018;
    signed char* eax4019;
    signed char al4020;
    signed char* eax4021;
    signed char* eax4022;
    signed char al4023;
    signed char* eax4024;
    signed char* eax4025;
    signed char al4026;
    signed char* eax4027;
    signed char* eax4028;
    signed char al4029;
    signed char* eax4030;
    signed char* eax4031;
    signed char al4032;
    signed char* eax4033;
    signed char* eax4034;
    signed char al4035;
    signed char* eax4036;
    signed char* eax4037;
    signed char al4038;
    signed char* eax4039;
    signed char* eax4040;
    signed char al4041;
    signed char* eax4042;
    signed char* eax4043;
    signed char al4044;
    signed char* eax4045;
    signed char* eax4046;
    signed char al4047;
    signed char* eax4048;
    signed char* eax4049;
    signed char al4050;
    signed char* eax4051;
    signed char* eax4052;
    signed char al4053;
    signed char* eax4054;
    signed char* eax4055;
    signed char al4056;
    signed char* eax4057;
    signed char* eax4058;
    signed char al4059;
    signed char* eax4060;
    signed char* eax4061;
    signed char al4062;
    signed char* eax4063;
    signed char* eax4064;
    signed char al4065;
    signed char* eax4066;
    signed char* eax4067;
    signed char al4068;
    signed char* eax4069;
    signed char* eax4070;
    signed char al4071;
    signed char* eax4072;
    signed char* eax4073;
    signed char al4074;
    signed char* eax4075;
    signed char* eax4076;
    signed char al4077;
    signed char* eax4078;
    signed char* eax4079;
    signed char al4080;
    signed char* eax4081;
    signed char* eax4082;
    signed char al4083;
    signed char* eax4084;
    signed char* eax4085;
    signed char al4086;
    signed char* eax4087;
    signed char* eax4088;
    signed char al4089;
    signed char* eax4090;
    signed char* eax4091;
    signed char al4092;
    signed char* eax4093;
    signed char* eax4094;
    signed char al4095;
    signed char* eax4096;
    signed char* eax4097;
    signed char al4098;
    signed char* eax4099;
    signed char* eax4100;
    signed char al4101;
    signed char* eax4102;
    signed char* eax4103;
    signed char al4104;
    signed char* eax4105;
    signed char* eax4106;
    signed char al4107;
    signed char* eax4108;
    signed char* eax4109;
    signed char al4110;
    signed char* eax4111;
    signed char* eax4112;
    signed char al4113;
    signed char* eax4114;
    signed char* eax4115;
    signed char al4116;
    signed char* eax4117;
    signed char* eax4118;
    signed char al4119;
    signed char* eax4120;
    signed char* eax4121;
    signed char al4122;
    signed char* eax4123;
    signed char* eax4124;
    signed char al4125;
    signed char* eax4126;
    signed char* eax4127;
    signed char al4128;
    signed char* eax4129;
    signed char* eax4130;
    signed char al4131;
    signed char* eax4132;
    signed char* eax4133;
    signed char al4134;
    signed char* eax4135;
    signed char* eax4136;
    signed char al4137;
    signed char* eax4138;
    signed char* eax4139;
    signed char al4140;
    signed char* eax4141;
    signed char* eax4142;
    signed char al4143;
    signed char* eax4144;
    signed char* eax4145;
    signed char al4146;
    signed char* eax4147;
    signed char* eax4148;
    signed char al4149;
    signed char* eax4150;
    signed char* eax4151;
    signed char al4152;
    signed char* eax4153;
    signed char* eax4154;
    signed char al4155;
    signed char* eax4156;
    signed char* eax4157;
    signed char al4158;
    signed char* eax4159;
    signed char* eax4160;
    signed char al4161;
    signed char* eax4162;
    signed char* eax4163;
    signed char al4164;
    signed char* eax4165;
    signed char* eax4166;
    signed char al4167;
    signed char* eax4168;
    signed char* eax4169;
    signed char al4170;
    signed char* eax4171;
    signed char* eax4172;
    signed char al4173;
    signed char* eax4174;
    signed char* eax4175;
    signed char al4176;
    signed char* eax4177;
    signed char* eax4178;
    signed char al4179;
    signed char* eax4180;
    signed char* eax4181;
    signed char al4182;
    signed char* eax4183;
    signed char* eax4184;
    signed char al4185;
    signed char* eax4186;
    signed char* eax4187;
    signed char al4188;
    signed char* eax4189;
    signed char* eax4190;
    signed char al4191;
    signed char* eax4192;
    signed char* eax4193;
    signed char al4194;
    signed char* eax4195;
    signed char* eax4196;
    signed char al4197;
    signed char* eax4198;
    signed char* eax4199;
    signed char al4200;
    signed char* eax4201;
    signed char* eax4202;
    signed char al4203;
    signed char* eax4204;
    signed char* eax4205;
    signed char al4206;
    signed char* eax4207;
    signed char* eax4208;
    signed char al4209;
    signed char* eax4210;
    signed char* eax4211;
    signed char al4212;
    signed char* eax4213;
    signed char* eax4214;
    signed char al4215;
    signed char* eax4216;
    signed char* eax4217;
    signed char al4218;
    signed char* eax4219;
    signed char* eax4220;
    signed char al4221;
    signed char* eax4222;
    signed char* eax4223;
    signed char al4224;
    signed char* eax4225;
    signed char* eax4226;
    signed char al4227;
    signed char* eax4228;
    signed char* eax4229;
    signed char al4230;
    signed char* eax4231;
    signed char* eax4232;
    signed char al4233;
    signed char* eax4234;
    signed char* eax4235;
    signed char al4236;

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
    *eax319 = reinterpret_cast<signed char>(*eax320 + al321);
    *eax322 = reinterpret_cast<signed char>(*eax323 + al324);
    *eax325 = reinterpret_cast<signed char>(*eax326 + al327);
    *eax328 = reinterpret_cast<signed char>(*eax329 + al330);
    *eax331 = reinterpret_cast<signed char>(*eax332 + al333);
    *eax334 = reinterpret_cast<signed char>(*eax335 + al336);
    *eax337 = reinterpret_cast<signed char>(*eax338 + al339);
    *eax340 = reinterpret_cast<signed char>(*eax341 + al342);
    *eax343 = reinterpret_cast<signed char>(*eax344 + al345);
    *eax346 = reinterpret_cast<signed char>(*eax347 + al348);
    *eax349 = reinterpret_cast<signed char>(*eax350 + al351);
    *eax352 = reinterpret_cast<signed char>(*eax353 + al354);
    *eax355 = reinterpret_cast<signed char>(*eax356 + al357);
    *eax358 = reinterpret_cast<signed char>(*eax359 + al360);
    *eax361 = reinterpret_cast<signed char>(*eax362 + al363);
    *eax364 = reinterpret_cast<signed char>(*eax365 + al366);
    *eax367 = reinterpret_cast<signed char>(*eax368 + al369);
    *eax370 = reinterpret_cast<signed char>(*eax371 + al372);
    *eax373 = reinterpret_cast<signed char>(*eax374 + al375);
    *eax376 = reinterpret_cast<signed char>(*eax377 + al378);
    *eax379 = reinterpret_cast<signed char>(*eax380 + al381);
    *eax382 = reinterpret_cast<signed char>(*eax383 + al384);
    *eax385 = reinterpret_cast<signed char>(*eax386 + al387);
    *eax388 = reinterpret_cast<signed char>(*eax389 + al390);
    *eax391 = reinterpret_cast<signed char>(*eax392 + al393);
    *eax394 = reinterpret_cast<signed char>(*eax395 + al396);
    *eax397 = reinterpret_cast<signed char>(*eax398 + al399);
    *eax400 = reinterpret_cast<signed char>(*eax401 + al402);
    *eax403 = reinterpret_cast<signed char>(*eax404 + al405);
    *eax406 = reinterpret_cast<signed char>(*eax407 + al408);
    *eax409 = reinterpret_cast<signed char>(*eax410 + al411);
    *eax412 = reinterpret_cast<signed char>(*eax413 + al414);
    *eax415 = reinterpret_cast<signed char>(*eax416 + al417);
    *eax418 = reinterpret_cast<signed char>(*eax419 + al420);
    *eax421 = reinterpret_cast<signed char>(*eax422 + al423);
    *eax424 = reinterpret_cast<signed char>(*eax425 + al426);
    *eax427 = reinterpret_cast<signed char>(*eax428 + al429);
    *eax430 = reinterpret_cast<signed char>(*eax431 + al432);
    *eax433 = reinterpret_cast<signed char>(*eax434 + al435);
    *eax436 = reinterpret_cast<signed char>(*eax437 + al438);
    *eax439 = reinterpret_cast<signed char>(*eax440 + al441);
    *eax442 = reinterpret_cast<signed char>(*eax443 + al444);
    *eax445 = reinterpret_cast<signed char>(*eax446 + al447);
    *eax448 = reinterpret_cast<signed char>(*eax449 + al450);
    *eax451 = reinterpret_cast<signed char>(*eax452 + al453);
    *eax454 = reinterpret_cast<signed char>(*eax455 + al456);
    *eax457 = reinterpret_cast<signed char>(*eax458 + al459);
    *eax460 = reinterpret_cast<signed char>(*eax461 + al462);
    *eax463 = reinterpret_cast<signed char>(*eax464 + al465);
    *eax466 = reinterpret_cast<signed char>(*eax467 + al468);
    *eax469 = reinterpret_cast<signed char>(*eax470 + al471);
    *eax472 = reinterpret_cast<signed char>(*eax473 + al474);
    *eax475 = reinterpret_cast<signed char>(*eax476 + al477);
    *eax478 = reinterpret_cast<signed char>(*eax479 + al480);
    *eax481 = reinterpret_cast<signed char>(*eax482 + al483);
    *eax484 = reinterpret_cast<signed char>(*eax485 + al486);
    *eax487 = reinterpret_cast<signed char>(*eax488 + al489);
    *eax490 = reinterpret_cast<signed char>(*eax491 + al492);
    *eax493 = reinterpret_cast<signed char>(*eax494 + al495);
    *eax496 = reinterpret_cast<signed char>(*eax497 + al498);
    *eax499 = reinterpret_cast<signed char>(*eax500 + al501);
    *eax502 = reinterpret_cast<signed char>(*eax503 + al504);
    *eax505 = reinterpret_cast<signed char>(*eax506 + al507);
    *eax508 = reinterpret_cast<signed char>(*eax509 + al510);
    *eax511 = reinterpret_cast<signed char>(*eax512 + al513);
    *eax514 = reinterpret_cast<signed char>(*eax515 + al516);
    *eax517 = reinterpret_cast<signed char>(*eax518 + al519);
    *eax520 = reinterpret_cast<signed char>(*eax521 + al522);
    *eax523 = reinterpret_cast<signed char>(*eax524 + al525);
    *eax526 = reinterpret_cast<signed char>(*eax527 + al528);
    *eax529 = reinterpret_cast<signed char>(*eax530 + al531);
    *eax532 = reinterpret_cast<signed char>(*eax533 + al534);
    *eax535 = reinterpret_cast<signed char>(*eax536 + al537);
    *eax538 = reinterpret_cast<signed char>(*eax539 + al540);
    *eax541 = reinterpret_cast<signed char>(*eax542 + al543);
    *eax544 = reinterpret_cast<signed char>(*eax545 + al546);
    *eax547 = reinterpret_cast<signed char>(*eax548 + al549);
    *eax550 = reinterpret_cast<signed char>(*eax551 + al552);
    *eax553 = reinterpret_cast<signed char>(*eax554 + al555);
    *eax556 = reinterpret_cast<signed char>(*eax557 + al558);
    *eax559 = reinterpret_cast<signed char>(*eax560 + al561);
    *eax562 = reinterpret_cast<signed char>(*eax563 + al564);
    *eax565 = reinterpret_cast<signed char>(*eax566 + al567);
    *eax568 = reinterpret_cast<signed char>(*eax569 + al570);
    *eax571 = reinterpret_cast<signed char>(*eax572 + al573);
    *eax574 = reinterpret_cast<signed char>(*eax575 + al576);
    *eax577 = reinterpret_cast<signed char>(*eax578 + al579);
    *eax580 = reinterpret_cast<signed char>(*eax581 + al582);
    *eax583 = reinterpret_cast<signed char>(*eax584 + al585);
    *eax586 = reinterpret_cast<signed char>(*eax587 + al588);
    *eax589 = reinterpret_cast<signed char>(*eax590 + al591);
    *eax592 = reinterpret_cast<signed char>(*eax593 + al594);
    *eax595 = reinterpret_cast<signed char>(*eax596 + al597);
    *eax598 = reinterpret_cast<signed char>(*eax599 + al600);
    *eax601 = reinterpret_cast<signed char>(*eax602 + al603);
    *eax604 = reinterpret_cast<signed char>(*eax605 + al606);
    *eax607 = reinterpret_cast<signed char>(*eax608 + al609);
    *eax610 = reinterpret_cast<signed char>(*eax611 + al612);
    *eax613 = reinterpret_cast<signed char>(*eax614 + al615);
    *eax616 = reinterpret_cast<signed char>(*eax617 + al618);
    *eax619 = reinterpret_cast<signed char>(*eax620 + al621);
    *eax622 = reinterpret_cast<signed char>(*eax623 + al624);
    *eax625 = reinterpret_cast<signed char>(*eax626 + al627);
    *eax628 = reinterpret_cast<signed char>(*eax629 + al630);
    *eax631 = reinterpret_cast<signed char>(*eax632 + al633);
    *eax634 = reinterpret_cast<signed char>(*eax635 + al636);
    *eax637 = reinterpret_cast<signed char>(*eax638 + al639);
    *eax640 = reinterpret_cast<signed char>(*eax641 + al642);
    *eax643 = reinterpret_cast<signed char>(*eax644 + al645);
    *eax646 = reinterpret_cast<signed char>(*eax647 + al648);
    *eax649 = reinterpret_cast<signed char>(*eax650 + al651);
    *eax652 = reinterpret_cast<signed char>(*eax653 + al654);
    *eax655 = reinterpret_cast<signed char>(*eax656 + al657);
    *eax658 = reinterpret_cast<signed char>(*eax659 + al660);
    *eax661 = reinterpret_cast<signed char>(*eax662 + al663);
    *eax664 = reinterpret_cast<signed char>(*eax665 + al666);
    *eax667 = reinterpret_cast<signed char>(*eax668 + al669);
    *eax670 = reinterpret_cast<signed char>(*eax671 + al672);
    *eax673 = reinterpret_cast<signed char>(*eax674 + al675);
    *eax676 = reinterpret_cast<signed char>(*eax677 + al678);
    *eax679 = reinterpret_cast<signed char>(*eax680 + al681);
    *eax682 = reinterpret_cast<signed char>(*eax683 + al684);
    *eax685 = reinterpret_cast<signed char>(*eax686 + al687);
    *eax688 = reinterpret_cast<signed char>(*eax689 + al690);
    *eax691 = reinterpret_cast<signed char>(*eax692 + al693);
    *eax694 = reinterpret_cast<signed char>(*eax695 + al696);
    *eax697 = reinterpret_cast<signed char>(*eax698 + al699);
    *eax700 = reinterpret_cast<signed char>(*eax701 + al702);
    *eax703 = reinterpret_cast<signed char>(*eax704 + al705);
    *eax706 = reinterpret_cast<signed char>(*eax707 + al708);
    *eax709 = reinterpret_cast<signed char>(*eax710 + al711);
    *eax712 = reinterpret_cast<signed char>(*eax713 + al714);
    *eax715 = reinterpret_cast<signed char>(*eax716 + al717);
    *eax718 = reinterpret_cast<signed char>(*eax719 + al720);
    *eax721 = reinterpret_cast<signed char>(*eax722 + al723);
    *eax724 = reinterpret_cast<signed char>(*eax725 + al726);
    *eax727 = reinterpret_cast<signed char>(*eax728 + al729);
    *eax730 = reinterpret_cast<signed char>(*eax731 + al732);
    *eax733 = reinterpret_cast<signed char>(*eax734 + al735);
    *eax736 = reinterpret_cast<signed char>(*eax737 + al738);
    *eax739 = reinterpret_cast<signed char>(*eax740 + al741);
    *eax742 = reinterpret_cast<signed char>(*eax743 + al744);
    *eax745 = reinterpret_cast<signed char>(*eax746 + al747);
    *eax748 = reinterpret_cast<signed char>(*eax749 + al750);
    *eax751 = reinterpret_cast<signed char>(*eax752 + al753);
    *eax754 = reinterpret_cast<signed char>(*eax755 + al756);
    *eax757 = reinterpret_cast<signed char>(*eax758 + al759);
    *eax760 = reinterpret_cast<signed char>(*eax761 + al762);
    *eax763 = reinterpret_cast<signed char>(*eax764 + al765);
    *eax766 = reinterpret_cast<signed char>(*eax767 + al768);
    *eax769 = reinterpret_cast<signed char>(*eax770 + al771);
    *eax772 = reinterpret_cast<signed char>(*eax773 + al774);
    *eax775 = reinterpret_cast<signed char>(*eax776 + al777);
    *eax778 = reinterpret_cast<signed char>(*eax779 + al780);
    *eax781 = reinterpret_cast<signed char>(*eax782 + al783);
    *eax784 = reinterpret_cast<signed char>(*eax785 + al786);
    *eax787 = reinterpret_cast<signed char>(*eax788 + al789);
    *eax790 = reinterpret_cast<signed char>(*eax791 + al792);
    *eax793 = reinterpret_cast<signed char>(*eax794 + al795);
    *eax796 = reinterpret_cast<signed char>(*eax797 + al798);
    *eax799 = reinterpret_cast<signed char>(*eax800 + al801);
    *eax802 = reinterpret_cast<signed char>(*eax803 + al804);
    *eax805 = reinterpret_cast<signed char>(*eax806 + al807);
    *eax808 = reinterpret_cast<signed char>(*eax809 + al810);
    *eax811 = reinterpret_cast<signed char>(*eax812 + al813);
    *eax814 = reinterpret_cast<signed char>(*eax815 + al816);
    *eax817 = reinterpret_cast<signed char>(*eax818 + al819);
    *eax820 = reinterpret_cast<signed char>(*eax821 + al822);
    *eax823 = reinterpret_cast<signed char>(*eax824 + al825);
    *eax826 = reinterpret_cast<signed char>(*eax827 + al828);
    *eax829 = reinterpret_cast<signed char>(*eax830 + al831);
    *eax832 = reinterpret_cast<signed char>(*eax833 + al834);
    *eax835 = reinterpret_cast<signed char>(*eax836 + al837);
    *eax838 = reinterpret_cast<signed char>(*eax839 + al840);
    *eax841 = reinterpret_cast<signed char>(*eax842 + al843);
    *eax844 = reinterpret_cast<signed char>(*eax845 + al846);
    *eax847 = reinterpret_cast<signed char>(*eax848 + al849);
    *eax850 = reinterpret_cast<signed char>(*eax851 + al852);
    *eax853 = reinterpret_cast<signed char>(*eax854 + al855);
    *eax856 = reinterpret_cast<signed char>(*eax857 + al858);
    *eax859 = reinterpret_cast<signed char>(*eax860 + al861);
    *eax862 = reinterpret_cast<signed char>(*eax863 + al864);
    *eax865 = reinterpret_cast<signed char>(*eax866 + al867);
    *eax868 = reinterpret_cast<signed char>(*eax869 + al870);
    *eax871 = reinterpret_cast<signed char>(*eax872 + al873);
    *eax874 = reinterpret_cast<signed char>(*eax875 + al876);
    *eax877 = reinterpret_cast<signed char>(*eax878 + al879);
    *eax880 = reinterpret_cast<signed char>(*eax881 + al882);
    *eax883 = reinterpret_cast<signed char>(*eax884 + al885);
    *eax886 = reinterpret_cast<signed char>(*eax887 + al888);
    *eax889 = reinterpret_cast<signed char>(*eax890 + al891);
    *eax892 = reinterpret_cast<signed char>(*eax893 + al894);
    *eax895 = reinterpret_cast<signed char>(*eax896 + al897);
    *eax898 = reinterpret_cast<signed char>(*eax899 + al900);
    *eax901 = reinterpret_cast<signed char>(*eax902 + al903);
    *eax904 = reinterpret_cast<signed char>(*eax905 + al906);
    *eax907 = reinterpret_cast<signed char>(*eax908 + al909);
    *eax910 = reinterpret_cast<signed char>(*eax911 + al912);
    *eax913 = reinterpret_cast<signed char>(*eax914 + al915);
    *eax916 = reinterpret_cast<signed char>(*eax917 + al918);
    *eax919 = reinterpret_cast<signed char>(*eax920 + al921);
    *eax922 = reinterpret_cast<signed char>(*eax923 + al924);
    *eax925 = reinterpret_cast<signed char>(*eax926 + al927);
    *eax928 = reinterpret_cast<signed char>(*eax929 + al930);
    *eax931 = reinterpret_cast<signed char>(*eax932 + al933);
    *eax934 = reinterpret_cast<signed char>(*eax935 + al936);
    *eax937 = reinterpret_cast<signed char>(*eax938 + al939);
    *eax940 = reinterpret_cast<signed char>(*eax941 + al942);
    *eax943 = reinterpret_cast<signed char>(*eax944 + al945);
    *eax946 = reinterpret_cast<signed char>(*eax947 + al948);
    *eax949 = reinterpret_cast<signed char>(*eax950 + al951);
    *eax952 = reinterpret_cast<signed char>(*eax953 + al954);
    *eax955 = reinterpret_cast<signed char>(*eax956 + al957);
    *eax958 = reinterpret_cast<signed char>(*eax959 + al960);
    *eax961 = reinterpret_cast<signed char>(*eax962 + al963);
    *eax964 = reinterpret_cast<signed char>(*eax965 + al966);
    *eax967 = reinterpret_cast<signed char>(*eax968 + al969);
    *eax970 = reinterpret_cast<signed char>(*eax971 + al972);
    *eax973 = reinterpret_cast<signed char>(*eax974 + al975);
    *eax976 = reinterpret_cast<signed char>(*eax977 + al978);
    *eax979 = reinterpret_cast<signed char>(*eax980 + al981);
    *eax982 = reinterpret_cast<signed char>(*eax983 + al984);
    *eax985 = reinterpret_cast<signed char>(*eax986 + al987);
    *eax988 = reinterpret_cast<signed char>(*eax989 + al990);
    *eax991 = reinterpret_cast<signed char>(*eax992 + al993);
    *eax994 = reinterpret_cast<signed char>(*eax995 + al996);
    *eax997 = reinterpret_cast<signed char>(*eax998 + al999);
    *eax1000 = reinterpret_cast<signed char>(*eax1001 + al1002);
    *eax1003 = reinterpret_cast<signed char>(*eax1004 + al1005);
    *eax1006 = reinterpret_cast<signed char>(*eax1007 + al1008);
    *eax1009 = reinterpret_cast<signed char>(*eax1010 + al1011);
    *eax1012 = reinterpret_cast<signed char>(*eax1013 + al1014);
    *eax1015 = reinterpret_cast<signed char>(*eax1016 + al1017);
    *eax1018 = reinterpret_cast<signed char>(*eax1019 + al1020);
    *eax1021 = reinterpret_cast<signed char>(*eax1022 + al1023);
    *eax1024 = reinterpret_cast<signed char>(*eax1025 + al1026);
    *eax1027 = reinterpret_cast<signed char>(*eax1028 + al1029);
    *eax1030 = reinterpret_cast<signed char>(*eax1031 + al1032);
    *eax1033 = reinterpret_cast<signed char>(*eax1034 + al1035);
    *eax1036 = reinterpret_cast<signed char>(*eax1037 + al1038);
    *eax1039 = reinterpret_cast<signed char>(*eax1040 + al1041);
    *eax1042 = reinterpret_cast<signed char>(*eax1043 + al1044);
    *eax1045 = reinterpret_cast<signed char>(*eax1046 + al1047);
    *eax1048 = reinterpret_cast<signed char>(*eax1049 + al1050);
    *eax1051 = reinterpret_cast<signed char>(*eax1052 + al1053);
    *eax1054 = reinterpret_cast<signed char>(*eax1055 + al1056);
    *eax1057 = reinterpret_cast<signed char>(*eax1058 + al1059);
    *eax1060 = reinterpret_cast<signed char>(*eax1061 + al1062);
    *eax1063 = reinterpret_cast<signed char>(*eax1064 + al1065);
    *eax1066 = reinterpret_cast<signed char>(*eax1067 + al1068);
    *eax1069 = reinterpret_cast<signed char>(*eax1070 + al1071);
    *eax1072 = reinterpret_cast<signed char>(*eax1073 + al1074);
    *eax1075 = reinterpret_cast<signed char>(*eax1076 + al1077);
    *eax1078 = reinterpret_cast<signed char>(*eax1079 + al1080);
    *eax1081 = reinterpret_cast<signed char>(*eax1082 + al1083);
    *eax1084 = reinterpret_cast<signed char>(*eax1085 + al1086);
    *eax1087 = reinterpret_cast<signed char>(*eax1088 + al1089);
    *eax1090 = reinterpret_cast<signed char>(*eax1091 + al1092);
    *eax1093 = reinterpret_cast<signed char>(*eax1094 + al1095);
    *eax1096 = reinterpret_cast<signed char>(*eax1097 + al1098);
    *eax1099 = reinterpret_cast<signed char>(*eax1100 + al1101);
    *eax1102 = reinterpret_cast<signed char>(*eax1103 + al1104);
    *eax1105 = reinterpret_cast<signed char>(*eax1106 + al1107);
    *eax1108 = reinterpret_cast<signed char>(*eax1109 + al1110);
    *eax1111 = reinterpret_cast<signed char>(*eax1112 + al1113);
    *eax1114 = reinterpret_cast<signed char>(*eax1115 + al1116);
    *eax1117 = reinterpret_cast<signed char>(*eax1118 + al1119);
    *eax1120 = reinterpret_cast<signed char>(*eax1121 + al1122);
    *eax1123 = reinterpret_cast<signed char>(*eax1124 + al1125);
    *eax1126 = reinterpret_cast<signed char>(*eax1127 + al1128);
    *eax1129 = reinterpret_cast<signed char>(*eax1130 + al1131);
    *eax1132 = reinterpret_cast<signed char>(*eax1133 + al1134);
    *eax1135 = reinterpret_cast<signed char>(*eax1136 + al1137);
    *eax1138 = reinterpret_cast<signed char>(*eax1139 + al1140);
    *eax1141 = reinterpret_cast<signed char>(*eax1142 + al1143);
    *eax1144 = reinterpret_cast<signed char>(*eax1145 + al1146);
    *eax1147 = reinterpret_cast<signed char>(*eax1148 + al1149);
    *eax1150 = reinterpret_cast<signed char>(*eax1151 + al1152);
    *eax1153 = reinterpret_cast<signed char>(*eax1154 + al1155);
    *eax1156 = reinterpret_cast<signed char>(*eax1157 + al1158);
    *eax1159 = reinterpret_cast<signed char>(*eax1160 + al1161);
    *eax1162 = reinterpret_cast<signed char>(*eax1163 + al1164);
    *eax1165 = reinterpret_cast<signed char>(*eax1166 + al1167);
    *eax1168 = reinterpret_cast<signed char>(*eax1169 + al1170);
    *eax1171 = reinterpret_cast<signed char>(*eax1172 + al1173);
    *eax1174 = reinterpret_cast<signed char>(*eax1175 + al1176);
    *eax1177 = reinterpret_cast<signed char>(*eax1178 + al1179);
    *eax1180 = reinterpret_cast<signed char>(*eax1181 + al1182);
    *eax1183 = reinterpret_cast<signed char>(*eax1184 + al1185);
    *eax1186 = reinterpret_cast<signed char>(*eax1187 + al1188);
    *eax1189 = reinterpret_cast<signed char>(*eax1190 + al1191);
    *eax1192 = reinterpret_cast<signed char>(*eax1193 + al1194);
    *eax1195 = reinterpret_cast<signed char>(*eax1196 + al1197);
    *eax1198 = reinterpret_cast<signed char>(*eax1199 + al1200);
    *eax1201 = reinterpret_cast<signed char>(*eax1202 + al1203);
    *eax1204 = reinterpret_cast<signed char>(*eax1205 + al1206);
    *eax1207 = reinterpret_cast<signed char>(*eax1208 + al1209);
    *eax1210 = reinterpret_cast<signed char>(*eax1211 + al1212);
    *eax1213 = reinterpret_cast<signed char>(*eax1214 + al1215);
    *eax1216 = reinterpret_cast<signed char>(*eax1217 + al1218);
    *eax1219 = reinterpret_cast<signed char>(*eax1220 + al1221);
    *eax1222 = reinterpret_cast<signed char>(*eax1223 + al1224);
    *eax1225 = reinterpret_cast<signed char>(*eax1226 + al1227);
    *eax1228 = reinterpret_cast<signed char>(*eax1229 + al1230);
    *eax1231 = reinterpret_cast<signed char>(*eax1232 + al1233);
    *eax1234 = reinterpret_cast<signed char>(*eax1235 + al1236);
    *eax1237 = reinterpret_cast<signed char>(*eax1238 + al1239);
    *eax1240 = reinterpret_cast<signed char>(*eax1241 + al1242);
    *eax1243 = reinterpret_cast<signed char>(*eax1244 + al1245);
    *eax1246 = reinterpret_cast<signed char>(*eax1247 + al1248);
    *eax1249 = reinterpret_cast<signed char>(*eax1250 + al1251);
    *eax1252 = reinterpret_cast<signed char>(*eax1253 + al1254);
    *eax1255 = reinterpret_cast<signed char>(*eax1256 + al1257);
    *eax1258 = reinterpret_cast<signed char>(*eax1259 + al1260);
    *eax1261 = reinterpret_cast<signed char>(*eax1262 + al1263);
    *eax1264 = reinterpret_cast<signed char>(*eax1265 + al1266);
    *eax1267 = reinterpret_cast<signed char>(*eax1268 + al1269);
    *eax1270 = reinterpret_cast<signed char>(*eax1271 + al1272);
    *eax1273 = reinterpret_cast<signed char>(*eax1274 + al1275);
    *eax1276 = reinterpret_cast<signed char>(*eax1277 + al1278);
    *eax1279 = reinterpret_cast<signed char>(*eax1280 + al1281);
    *eax1282 = reinterpret_cast<signed char>(*eax1283 + al1284);
    *eax1285 = reinterpret_cast<signed char>(*eax1286 + al1287);
    *eax1288 = reinterpret_cast<signed char>(*eax1289 + al1290);
    *eax1291 = reinterpret_cast<signed char>(*eax1292 + al1293);
    *eax1294 = reinterpret_cast<signed char>(*eax1295 + al1296);
    *eax1297 = reinterpret_cast<signed char>(*eax1298 + al1299);
    *eax1300 = reinterpret_cast<signed char>(*eax1301 + al1302);
    *eax1303 = reinterpret_cast<signed char>(*eax1304 + al1305);
    *eax1306 = reinterpret_cast<signed char>(*eax1307 + al1308);
    *eax1309 = reinterpret_cast<signed char>(*eax1310 + al1311);
    *eax1312 = reinterpret_cast<signed char>(*eax1313 + al1314);
    *eax1315 = reinterpret_cast<signed char>(*eax1316 + al1317);
    *eax1318 = reinterpret_cast<signed char>(*eax1319 + al1320);
    *eax1321 = reinterpret_cast<signed char>(*eax1322 + al1323);
    *eax1324 = reinterpret_cast<signed char>(*eax1325 + al1326);
    *eax1327 = reinterpret_cast<signed char>(*eax1328 + al1329);
    *eax1330 = reinterpret_cast<signed char>(*eax1331 + al1332);
    *eax1333 = reinterpret_cast<signed char>(*eax1334 + al1335);
    *eax1336 = reinterpret_cast<signed char>(*eax1337 + al1338);
    *eax1339 = reinterpret_cast<signed char>(*eax1340 + al1341);
    *eax1342 = reinterpret_cast<signed char>(*eax1343 + al1344);
    *eax1345 = reinterpret_cast<signed char>(*eax1346 + al1347);
    *eax1348 = reinterpret_cast<signed char>(*eax1349 + al1350);
    *eax1351 = reinterpret_cast<signed char>(*eax1352 + al1353);
    *eax1354 = reinterpret_cast<signed char>(*eax1355 + al1356);
    *eax1357 = reinterpret_cast<signed char>(*eax1358 + al1359);
    *eax1360 = reinterpret_cast<signed char>(*eax1361 + al1362);
    *eax1363 = reinterpret_cast<signed char>(*eax1364 + al1365);
    *eax1366 = reinterpret_cast<signed char>(*eax1367 + al1368);
    *eax1369 = reinterpret_cast<signed char>(*eax1370 + al1371);
    *eax1372 = reinterpret_cast<signed char>(*eax1373 + al1374);
    *eax1375 = reinterpret_cast<signed char>(*eax1376 + al1377);
    *eax1378 = reinterpret_cast<signed char>(*eax1379 + al1380);
    *eax1381 = reinterpret_cast<signed char>(*eax1382 + al1383);
    *eax1384 = reinterpret_cast<signed char>(*eax1385 + al1386);
    *eax1387 = reinterpret_cast<signed char>(*eax1388 + al1389);
    *eax1390 = reinterpret_cast<signed char>(*eax1391 + al1392);
    *eax1393 = reinterpret_cast<signed char>(*eax1394 + al1395);
    *eax1396 = reinterpret_cast<signed char>(*eax1397 + al1398);
    *eax1399 = reinterpret_cast<signed char>(*eax1400 + al1401);
    *eax1402 = reinterpret_cast<signed char>(*eax1403 + al1404);
    *eax1405 = reinterpret_cast<signed char>(*eax1406 + al1407);
    *eax1408 = reinterpret_cast<signed char>(*eax1409 + al1410);
    *eax1411 = reinterpret_cast<signed char>(*eax1412 + al1413);
    *eax1414 = reinterpret_cast<signed char>(*eax1415 + al1416);
    *eax1417 = reinterpret_cast<signed char>(*eax1418 + al1419);
    *eax1420 = reinterpret_cast<signed char>(*eax1421 + al1422);
    *eax1423 = reinterpret_cast<signed char>(*eax1424 + al1425);
    *eax1426 = reinterpret_cast<signed char>(*eax1427 + al1428);
    *eax1429 = reinterpret_cast<signed char>(*eax1430 + al1431);
    *eax1432 = reinterpret_cast<signed char>(*eax1433 + al1434);
    *eax1435 = reinterpret_cast<signed char>(*eax1436 + al1437);
    *eax1438 = reinterpret_cast<signed char>(*eax1439 + al1440);
    *eax1441 = reinterpret_cast<signed char>(*eax1442 + al1443);
    *eax1444 = reinterpret_cast<signed char>(*eax1445 + al1446);
    *eax1447 = reinterpret_cast<signed char>(*eax1448 + al1449);
    *eax1450 = reinterpret_cast<signed char>(*eax1451 + al1452);
    *eax1453 = reinterpret_cast<signed char>(*eax1454 + al1455);
    *eax1456 = reinterpret_cast<signed char>(*eax1457 + al1458);
    *eax1459 = reinterpret_cast<signed char>(*eax1460 + al1461);
    *eax1462 = reinterpret_cast<signed char>(*eax1463 + al1464);
    *eax1465 = reinterpret_cast<signed char>(*eax1466 + al1467);
    *eax1468 = reinterpret_cast<signed char>(*eax1469 + al1470);
    *eax1471 = reinterpret_cast<signed char>(*eax1472 + al1473);
    *eax1474 = reinterpret_cast<signed char>(*eax1475 + al1476);
    *eax1477 = reinterpret_cast<signed char>(*eax1478 + al1479);
    *eax1480 = reinterpret_cast<signed char>(*eax1481 + al1482);
    *eax1483 = reinterpret_cast<signed char>(*eax1484 + al1485);
    *eax1486 = reinterpret_cast<signed char>(*eax1487 + al1488);
    *eax1489 = reinterpret_cast<signed char>(*eax1490 + al1491);
    *eax1492 = reinterpret_cast<signed char>(*eax1493 + al1494);
    *eax1495 = reinterpret_cast<signed char>(*eax1496 + al1497);
    *eax1498 = reinterpret_cast<signed char>(*eax1499 + al1500);
    *eax1501 = reinterpret_cast<signed char>(*eax1502 + al1503);
    *eax1504 = reinterpret_cast<signed char>(*eax1505 + al1506);
    *eax1507 = reinterpret_cast<signed char>(*eax1508 + al1509);
    *eax1510 = reinterpret_cast<signed char>(*eax1511 + al1512);
    *eax1513 = reinterpret_cast<signed char>(*eax1514 + al1515);
    *eax1516 = reinterpret_cast<signed char>(*eax1517 + al1518);
    *eax1519 = reinterpret_cast<signed char>(*eax1520 + al1521);
    *eax1522 = reinterpret_cast<signed char>(*eax1523 + al1524);
    *eax1525 = reinterpret_cast<signed char>(*eax1526 + al1527);
    *eax1528 = reinterpret_cast<signed char>(*eax1529 + al1530);
    *eax1531 = reinterpret_cast<signed char>(*eax1532 + al1533);
    *eax1534 = reinterpret_cast<signed char>(*eax1535 + al1536);
    *eax1537 = reinterpret_cast<signed char>(*eax1538 + al1539);
    *eax1540 = reinterpret_cast<signed char>(*eax1541 + al1542);
    *eax1543 = reinterpret_cast<signed char>(*eax1544 + al1545);
    *eax1546 = reinterpret_cast<signed char>(*eax1547 + al1548);
    *eax1549 = reinterpret_cast<signed char>(*eax1550 + al1551);
    *eax1552 = reinterpret_cast<signed char>(*eax1553 + al1554);
    *eax1555 = reinterpret_cast<signed char>(*eax1556 + al1557);
    *eax1558 = reinterpret_cast<signed char>(*eax1559 + al1560);
    *eax1561 = reinterpret_cast<signed char>(*eax1562 + al1563);
    *eax1564 = reinterpret_cast<signed char>(*eax1565 + al1566);
    *eax1567 = reinterpret_cast<signed char>(*eax1568 + al1569);
    *eax1570 = reinterpret_cast<signed char>(*eax1571 + al1572);
    *eax1573 = reinterpret_cast<signed char>(*eax1574 + al1575);
    *eax1576 = reinterpret_cast<signed char>(*eax1577 + al1578);
    *eax1579 = reinterpret_cast<signed char>(*eax1580 + al1581);
    *eax1582 = reinterpret_cast<signed char>(*eax1583 + al1584);
    *eax1585 = reinterpret_cast<signed char>(*eax1586 + al1587);
    *eax1588 = reinterpret_cast<signed char>(*eax1589 + al1590);
    *eax1591 = reinterpret_cast<signed char>(*eax1592 + al1593);
    *eax1594 = reinterpret_cast<signed char>(*eax1595 + al1596);
    *eax1597 = reinterpret_cast<signed char>(*eax1598 + al1599);
    *eax1600 = reinterpret_cast<signed char>(*eax1601 + al1602);
    *eax1603 = reinterpret_cast<signed char>(*eax1604 + al1605);
    *eax1606 = reinterpret_cast<signed char>(*eax1607 + al1608);
    *eax1609 = reinterpret_cast<signed char>(*eax1610 + al1611);
    *eax1612 = reinterpret_cast<signed char>(*eax1613 + al1614);
    *eax1615 = reinterpret_cast<signed char>(*eax1616 + al1617);
    *eax1618 = reinterpret_cast<signed char>(*eax1619 + al1620);
    *eax1621 = reinterpret_cast<signed char>(*eax1622 + al1623);
    *eax1624 = reinterpret_cast<signed char>(*eax1625 + al1626);
    *eax1627 = reinterpret_cast<signed char>(*eax1628 + al1629);
    *eax1630 = reinterpret_cast<signed char>(*eax1631 + al1632);
    *eax1633 = reinterpret_cast<signed char>(*eax1634 + al1635);
    *eax1636 = reinterpret_cast<signed char>(*eax1637 + al1638);
    *eax1639 = reinterpret_cast<signed char>(*eax1640 + al1641);
    *eax1642 = reinterpret_cast<signed char>(*eax1643 + al1644);
    *eax1645 = reinterpret_cast<signed char>(*eax1646 + al1647);
    *eax1648 = reinterpret_cast<signed char>(*eax1649 + al1650);
    *eax1651 = reinterpret_cast<signed char>(*eax1652 + al1653);
    *eax1654 = reinterpret_cast<signed char>(*eax1655 + al1656);
    *eax1657 = reinterpret_cast<signed char>(*eax1658 + al1659);
    *eax1660 = reinterpret_cast<signed char>(*eax1661 + al1662);
    *eax1663 = reinterpret_cast<signed char>(*eax1664 + al1665);
    *eax1666 = reinterpret_cast<signed char>(*eax1667 + al1668);
    *eax1669 = reinterpret_cast<signed char>(*eax1670 + al1671);
    *eax1672 = reinterpret_cast<signed char>(*eax1673 + al1674);
    *eax1675 = reinterpret_cast<signed char>(*eax1676 + al1677);
    *eax1678 = reinterpret_cast<signed char>(*eax1679 + al1680);
    *eax1681 = reinterpret_cast<signed char>(*eax1682 + al1683);
    *eax1684 = reinterpret_cast<signed char>(*eax1685 + al1686);
    *eax1687 = reinterpret_cast<signed char>(*eax1688 + al1689);
    *eax1690 = reinterpret_cast<signed char>(*eax1691 + al1692);
    *eax1693 = reinterpret_cast<signed char>(*eax1694 + al1695);
    *eax1696 = reinterpret_cast<signed char>(*eax1697 + al1698);
    *eax1699 = reinterpret_cast<signed char>(*eax1700 + al1701);
    *eax1702 = reinterpret_cast<signed char>(*eax1703 + al1704);
    *eax1705 = reinterpret_cast<signed char>(*eax1706 + al1707);
    *eax1708 = reinterpret_cast<signed char>(*eax1709 + al1710);
    *eax1711 = reinterpret_cast<signed char>(*eax1712 + al1713);
    *eax1714 = reinterpret_cast<signed char>(*eax1715 + al1716);
    *eax1717 = reinterpret_cast<signed char>(*eax1718 + al1719);
    *eax1720 = reinterpret_cast<signed char>(*eax1721 + al1722);
    *eax1723 = reinterpret_cast<signed char>(*eax1724 + al1725);
    *eax1726 = reinterpret_cast<signed char>(*eax1727 + al1728);
    *eax1729 = reinterpret_cast<signed char>(*eax1730 + al1731);
    *eax1732 = reinterpret_cast<signed char>(*eax1733 + al1734);
    *eax1735 = reinterpret_cast<signed char>(*eax1736 + al1737);
    *eax1738 = reinterpret_cast<signed char>(*eax1739 + al1740);
    *eax1741 = reinterpret_cast<signed char>(*eax1742 + al1743);
    *eax1744 = reinterpret_cast<signed char>(*eax1745 + al1746);
    *eax1747 = reinterpret_cast<signed char>(*eax1748 + al1749);
    *eax1750 = reinterpret_cast<signed char>(*eax1751 + al1752);
    *eax1753 = reinterpret_cast<signed char>(*eax1754 + al1755);
    *eax1756 = reinterpret_cast<signed char>(*eax1757 + al1758);
    *eax1759 = reinterpret_cast<signed char>(*eax1760 + al1761);
    *eax1762 = reinterpret_cast<signed char>(*eax1763 + al1764);
    *eax1765 = reinterpret_cast<signed char>(*eax1766 + al1767);
    *eax1768 = reinterpret_cast<signed char>(*eax1769 + al1770);
    *eax1771 = reinterpret_cast<signed char>(*eax1772 + al1773);
    *eax1774 = reinterpret_cast<signed char>(*eax1775 + al1776);
    *eax1777 = reinterpret_cast<signed char>(*eax1778 + al1779);
    *eax1780 = reinterpret_cast<signed char>(*eax1781 + al1782);
    *eax1783 = reinterpret_cast<signed char>(*eax1784 + al1785);
    *eax1786 = reinterpret_cast<signed char>(*eax1787 + al1788);
    *eax1789 = reinterpret_cast<signed char>(*eax1790 + al1791);
    *eax1792 = reinterpret_cast<signed char>(*eax1793 + al1794);
    *eax1795 = reinterpret_cast<signed char>(*eax1796 + al1797);
    *eax1798 = reinterpret_cast<signed char>(*eax1799 + al1800);
    *eax1801 = reinterpret_cast<signed char>(*eax1802 + al1803);
    *eax1804 = reinterpret_cast<signed char>(*eax1805 + al1806);
    *eax1807 = reinterpret_cast<signed char>(*eax1808 + al1809);
    *eax1810 = reinterpret_cast<signed char>(*eax1811 + al1812);
    *eax1813 = reinterpret_cast<signed char>(*eax1814 + al1815);
    *eax1816 = reinterpret_cast<signed char>(*eax1817 + al1818);
    *eax1819 = reinterpret_cast<signed char>(*eax1820 + al1821);
    *eax1822 = reinterpret_cast<signed char>(*eax1823 + al1824);
    *eax1825 = reinterpret_cast<signed char>(*eax1826 + al1827);
    *eax1828 = reinterpret_cast<signed char>(*eax1829 + al1830);
    *eax1831 = reinterpret_cast<signed char>(*eax1832 + al1833);
    *eax1834 = reinterpret_cast<signed char>(*eax1835 + al1836);
    *eax1837 = reinterpret_cast<signed char>(*eax1838 + al1839);
    *eax1840 = reinterpret_cast<signed char>(*eax1841 + al1842);
    *eax1843 = reinterpret_cast<signed char>(*eax1844 + al1845);
    *eax1846 = reinterpret_cast<signed char>(*eax1847 + al1848);
    *eax1849 = reinterpret_cast<signed char>(*eax1850 + al1851);
    *eax1852 = reinterpret_cast<signed char>(*eax1853 + al1854);
    *eax1855 = reinterpret_cast<signed char>(*eax1856 + al1857);
    *eax1858 = reinterpret_cast<signed char>(*eax1859 + al1860);
    *eax1861 = reinterpret_cast<signed char>(*eax1862 + al1863);
    *eax1864 = reinterpret_cast<signed char>(*eax1865 + al1866);
    *eax1867 = reinterpret_cast<signed char>(*eax1868 + al1869);
    *eax1870 = reinterpret_cast<signed char>(*eax1871 + al1872);
    *eax1873 = reinterpret_cast<signed char>(*eax1874 + al1875);
    *eax1876 = reinterpret_cast<signed char>(*eax1877 + al1878);
    *eax1879 = reinterpret_cast<signed char>(*eax1880 + al1881);
    *eax1882 = reinterpret_cast<signed char>(*eax1883 + al1884);
    *eax1885 = reinterpret_cast<signed char>(*eax1886 + al1887);
    *eax1888 = reinterpret_cast<signed char>(*eax1889 + al1890);
    *eax1891 = reinterpret_cast<signed char>(*eax1892 + al1893);
    *eax1894 = reinterpret_cast<signed char>(*eax1895 + al1896);
    *eax1897 = reinterpret_cast<signed char>(*eax1898 + al1899);
    *eax1900 = reinterpret_cast<signed char>(*eax1901 + al1902);
    *eax1903 = reinterpret_cast<signed char>(*eax1904 + al1905);
    *eax1906 = reinterpret_cast<signed char>(*eax1907 + al1908);
    *eax1909 = reinterpret_cast<signed char>(*eax1910 + al1911);
    *eax1912 = reinterpret_cast<signed char>(*eax1913 + al1914);
    *eax1915 = reinterpret_cast<signed char>(*eax1916 + al1917);
    *eax1918 = reinterpret_cast<signed char>(*eax1919 + al1920);
    *eax1921 = reinterpret_cast<signed char>(*eax1922 + al1923);
    *eax1924 = reinterpret_cast<signed char>(*eax1925 + al1926);
    *eax1927 = reinterpret_cast<signed char>(*eax1928 + al1929);
    *eax1930 = reinterpret_cast<signed char>(*eax1931 + al1932);
    *eax1933 = reinterpret_cast<signed char>(*eax1934 + al1935);
    *eax1936 = reinterpret_cast<signed char>(*eax1937 + al1938);
    *eax1939 = reinterpret_cast<signed char>(*eax1940 + al1941);
    *eax1942 = reinterpret_cast<signed char>(*eax1943 + al1944);
    *eax1945 = reinterpret_cast<signed char>(*eax1946 + al1947);
    *eax1948 = reinterpret_cast<signed char>(*eax1949 + al1950);
    *eax1951 = reinterpret_cast<signed char>(*eax1952 + al1953);
    *eax1954 = reinterpret_cast<signed char>(*eax1955 + al1956);
    *eax1957 = reinterpret_cast<signed char>(*eax1958 + al1959);
    *eax1960 = reinterpret_cast<signed char>(*eax1961 + al1962);
    *eax1963 = reinterpret_cast<signed char>(*eax1964 + al1965);
    *eax1966 = reinterpret_cast<signed char>(*eax1967 + al1968);
    *eax1969 = reinterpret_cast<signed char>(*eax1970 + al1971);
    *eax1972 = reinterpret_cast<signed char>(*eax1973 + al1974);
    *eax1975 = reinterpret_cast<signed char>(*eax1976 + al1977);
    *eax1978 = reinterpret_cast<signed char>(*eax1979 + al1980);
    *eax1981 = reinterpret_cast<signed char>(*eax1982 + al1983);
    *eax1984 = reinterpret_cast<signed char>(*eax1985 + al1986);
    *eax1987 = reinterpret_cast<signed char>(*eax1988 + al1989);
    *eax1990 = reinterpret_cast<signed char>(*eax1991 + al1992);
    *eax1993 = reinterpret_cast<signed char>(*eax1994 + al1995);
    *eax1996 = reinterpret_cast<signed char>(*eax1997 + al1998);
    *eax1999 = reinterpret_cast<signed char>(*eax2000 + al2001);
    *eax2002 = reinterpret_cast<signed char>(*eax2003 + al2004);
    *eax2005 = reinterpret_cast<signed char>(*eax2006 + al2007);
    *eax2008 = reinterpret_cast<signed char>(*eax2009 + al2010);
    *eax2011 = reinterpret_cast<signed char>(*eax2012 + al2013);
    *eax2014 = reinterpret_cast<signed char>(*eax2015 + al2016);
    *eax2017 = reinterpret_cast<signed char>(*eax2018 + al2019);
    *eax2020 = reinterpret_cast<signed char>(*eax2021 + al2022);
    *eax2023 = reinterpret_cast<signed char>(*eax2024 + al2025);
    *eax2026 = reinterpret_cast<signed char>(*eax2027 + al2028);
    *eax2029 = reinterpret_cast<signed char>(*eax2030 + al2031);
    *eax2032 = reinterpret_cast<signed char>(*eax2033 + al2034);
    *eax2035 = reinterpret_cast<signed char>(*eax2036 + al2037);
    *eax2038 = reinterpret_cast<signed char>(*eax2039 + al2040);
    *eax2041 = reinterpret_cast<signed char>(*eax2042 + al2043);
    *eax2044 = reinterpret_cast<signed char>(*eax2045 + al2046);
    *eax2047 = reinterpret_cast<signed char>(*eax2048 + al2049);
    *eax2050 = reinterpret_cast<signed char>(*eax2051 + al2052);
    *eax2053 = reinterpret_cast<signed char>(*eax2054 + al2055);
    *eax2056 = reinterpret_cast<signed char>(*eax2057 + al2058);
    *eax2059 = reinterpret_cast<signed char>(*eax2060 + al2061);
    *eax2062 = reinterpret_cast<signed char>(*eax2063 + al2064);
    *eax2065 = reinterpret_cast<signed char>(*eax2066 + al2067);
    *eax2068 = reinterpret_cast<signed char>(*eax2069 + al2070);
    *eax2071 = reinterpret_cast<signed char>(*eax2072 + al2073);
    *eax2074 = reinterpret_cast<signed char>(*eax2075 + al2076);
    *eax2077 = reinterpret_cast<signed char>(*eax2078 + al2079);
    *eax2080 = reinterpret_cast<signed char>(*eax2081 + al2082);
    *eax2083 = reinterpret_cast<signed char>(*eax2084 + al2085);
    *eax2086 = reinterpret_cast<signed char>(*eax2087 + al2088);
    *eax2089 = reinterpret_cast<signed char>(*eax2090 + al2091);
    *eax2092 = reinterpret_cast<signed char>(*eax2093 + al2094);
    *eax2095 = reinterpret_cast<signed char>(*eax2096 + al2097);
    *eax2098 = reinterpret_cast<signed char>(*eax2099 + al2100);
    *eax2101 = reinterpret_cast<signed char>(*eax2102 + al2103);
    *eax2104 = reinterpret_cast<signed char>(*eax2105 + al2106);
    *eax2107 = reinterpret_cast<signed char>(*eax2108 + al2109);
    *eax2110 = reinterpret_cast<signed char>(*eax2111 + al2112);
    *eax2113 = reinterpret_cast<signed char>(*eax2114 + al2115);
    *eax2116 = reinterpret_cast<signed char>(*eax2117 + al2118);
    *eax2119 = reinterpret_cast<signed char>(*eax2120 + al2121);
    *eax2122 = reinterpret_cast<signed char>(*eax2123 + al2124);
    *eax2125 = reinterpret_cast<signed char>(*eax2126 + al2127);
    *eax2128 = reinterpret_cast<signed char>(*eax2129 + al2130);
    *eax2131 = reinterpret_cast<signed char>(*eax2132 + al2133);
    *eax2134 = reinterpret_cast<signed char>(*eax2135 + al2136);
    *eax2137 = reinterpret_cast<signed char>(*eax2138 + al2139);
    *eax2140 = reinterpret_cast<signed char>(*eax2141 + al2142);
    *eax2143 = reinterpret_cast<signed char>(*eax2144 + al2145);
    *eax2146 = reinterpret_cast<signed char>(*eax2147 + al2148);
    *eax2149 = reinterpret_cast<signed char>(*eax2150 + al2151);
    *eax2152 = reinterpret_cast<signed char>(*eax2153 + al2154);
    *eax2155 = reinterpret_cast<signed char>(*eax2156 + al2157);
    *eax2158 = reinterpret_cast<signed char>(*eax2159 + al2160);
    *eax2161 = reinterpret_cast<signed char>(*eax2162 + al2163);
    *eax2164 = reinterpret_cast<signed char>(*eax2165 + al2166);
    *eax2167 = reinterpret_cast<signed char>(*eax2168 + al2169);
    *eax2170 = reinterpret_cast<signed char>(*eax2171 + al2172);
    *eax2173 = reinterpret_cast<signed char>(*eax2174 + al2175);
    *eax2176 = reinterpret_cast<signed char>(*eax2177 + al2178);
    *eax2179 = reinterpret_cast<signed char>(*eax2180 + al2181);
    *eax2182 = reinterpret_cast<signed char>(*eax2183 + al2184);
    *eax2185 = reinterpret_cast<signed char>(*eax2186 + al2187);
    *eax2188 = reinterpret_cast<signed char>(*eax2189 + al2190);
    *eax2191 = reinterpret_cast<signed char>(*eax2192 + al2193);
    *eax2194 = reinterpret_cast<signed char>(*eax2195 + al2196);
    *eax2197 = reinterpret_cast<signed char>(*eax2198 + al2199);
    *eax2200 = reinterpret_cast<signed char>(*eax2201 + al2202);
    *eax2203 = reinterpret_cast<signed char>(*eax2204 + al2205);
    *eax2206 = reinterpret_cast<signed char>(*eax2207 + al2208);
    *eax2209 = reinterpret_cast<signed char>(*eax2210 + al2211);
    *eax2212 = reinterpret_cast<signed char>(*eax2213 + al2214);
    *eax2215 = reinterpret_cast<signed char>(*eax2216 + al2217);
    *eax2218 = reinterpret_cast<signed char>(*eax2219 + al2220);
    *eax2221 = reinterpret_cast<signed char>(*eax2222 + al2223);
    *eax2224 = reinterpret_cast<signed char>(*eax2225 + al2226);
    *eax2227 = reinterpret_cast<signed char>(*eax2228 + al2229);
    *eax2230 = reinterpret_cast<signed char>(*eax2231 + al2232);
    *eax2233 = reinterpret_cast<signed char>(*eax2234 + al2235);
    *eax2236 = reinterpret_cast<signed char>(*eax2237 + al2238);
    *eax2239 = reinterpret_cast<signed char>(*eax2240 + al2241);
    *eax2242 = reinterpret_cast<signed char>(*eax2243 + al2244);
    *eax2245 = reinterpret_cast<signed char>(*eax2246 + al2247);
    *eax2248 = reinterpret_cast<signed char>(*eax2249 + al2250);
    *eax2251 = reinterpret_cast<signed char>(*eax2252 + al2253);
    *eax2254 = reinterpret_cast<signed char>(*eax2255 + al2256);
    *eax2257 = reinterpret_cast<signed char>(*eax2258 + al2259);
    *eax2260 = reinterpret_cast<signed char>(*eax2261 + al2262);
    *eax2263 = reinterpret_cast<signed char>(*eax2264 + al2265);
    *eax2266 = reinterpret_cast<signed char>(*eax2267 + al2268);
    *eax2269 = reinterpret_cast<signed char>(*eax2270 + al2271);
    *eax2272 = reinterpret_cast<signed char>(*eax2273 + al2274);
    *eax2275 = reinterpret_cast<signed char>(*eax2276 + al2277);
    *eax2278 = reinterpret_cast<signed char>(*eax2279 + al2280);
    *eax2281 = reinterpret_cast<signed char>(*eax2282 + al2283);
    *eax2284 = reinterpret_cast<signed char>(*eax2285 + al2286);
    *eax2287 = reinterpret_cast<signed char>(*eax2288 + al2289);
    *eax2290 = reinterpret_cast<signed char>(*eax2291 + al2292);
    *eax2293 = reinterpret_cast<signed char>(*eax2294 + al2295);
    *eax2296 = reinterpret_cast<signed char>(*eax2297 + al2298);
    *eax2299 = reinterpret_cast<signed char>(*eax2300 + al2301);
    *eax2302 = reinterpret_cast<signed char>(*eax2303 + al2304);
    *eax2305 = reinterpret_cast<signed char>(*eax2306 + al2307);
    *eax2308 = reinterpret_cast<signed char>(*eax2309 + al2310);
    *eax2311 = reinterpret_cast<signed char>(*eax2312 + al2313);
    *eax2314 = reinterpret_cast<signed char>(*eax2315 + al2316);
    *eax2317 = reinterpret_cast<signed char>(*eax2318 + al2319);
    *eax2320 = reinterpret_cast<signed char>(*eax2321 + al2322);
    *eax2323 = reinterpret_cast<signed char>(*eax2324 + al2325);
    *eax2326 = reinterpret_cast<signed char>(*eax2327 + al2328);
    *eax2329 = reinterpret_cast<signed char>(*eax2330 + al2331);
    *eax2332 = reinterpret_cast<signed char>(*eax2333 + al2334);
    *eax2335 = reinterpret_cast<signed char>(*eax2336 + al2337);
    *eax2338 = reinterpret_cast<signed char>(*eax2339 + al2340);
    *eax2341 = reinterpret_cast<signed char>(*eax2342 + al2343);
    *eax2344 = reinterpret_cast<signed char>(*eax2345 + al2346);
    *eax2347 = reinterpret_cast<signed char>(*eax2348 + al2349);
    *eax2350 = reinterpret_cast<signed char>(*eax2351 + al2352);
    *eax2353 = reinterpret_cast<signed char>(*eax2354 + al2355);
    *eax2356 = reinterpret_cast<signed char>(*eax2357 + al2358);
    *eax2359 = reinterpret_cast<signed char>(*eax2360 + al2361);
    *eax2362 = reinterpret_cast<signed char>(*eax2363 + al2364);
    *eax2365 = reinterpret_cast<signed char>(*eax2366 + al2367);
    *eax2368 = reinterpret_cast<signed char>(*eax2369 + al2370);
    *eax2371 = reinterpret_cast<signed char>(*eax2372 + al2373);
    *eax2374 = reinterpret_cast<signed char>(*eax2375 + al2376);
    *eax2377 = reinterpret_cast<signed char>(*eax2378 + al2379);
    *eax2380 = reinterpret_cast<signed char>(*eax2381 + al2382);
    *eax2383 = reinterpret_cast<signed char>(*eax2384 + al2385);
    *eax2386 = reinterpret_cast<signed char>(*eax2387 + al2388);
    *eax2389 = reinterpret_cast<signed char>(*eax2390 + al2391);
    *eax2392 = reinterpret_cast<signed char>(*eax2393 + al2394);
    *eax2395 = reinterpret_cast<signed char>(*eax2396 + al2397);
    *eax2398 = reinterpret_cast<signed char>(*eax2399 + al2400);
    *eax2401 = reinterpret_cast<signed char>(*eax2402 + al2403);
    *eax2404 = reinterpret_cast<signed char>(*eax2405 + al2406);
    *eax2407 = reinterpret_cast<signed char>(*eax2408 + al2409);
    *eax2410 = reinterpret_cast<signed char>(*eax2411 + al2412);
    *eax2413 = reinterpret_cast<signed char>(*eax2414 + al2415);
    *eax2416 = reinterpret_cast<signed char>(*eax2417 + al2418);
    *eax2419 = reinterpret_cast<signed char>(*eax2420 + al2421);
    *eax2422 = reinterpret_cast<signed char>(*eax2423 + al2424);
    *eax2425 = reinterpret_cast<signed char>(*eax2426 + al2427);
    *eax2428 = reinterpret_cast<signed char>(*eax2429 + al2430);
    *eax2431 = reinterpret_cast<signed char>(*eax2432 + al2433);
    *eax2434 = reinterpret_cast<signed char>(*eax2435 + al2436);
    *eax2437 = reinterpret_cast<signed char>(*eax2438 + al2439);
    *eax2440 = reinterpret_cast<signed char>(*eax2441 + al2442);
    *eax2443 = reinterpret_cast<signed char>(*eax2444 + al2445);
    *eax2446 = reinterpret_cast<signed char>(*eax2447 + al2448);
    *eax2449 = reinterpret_cast<signed char>(*eax2450 + al2451);
    *eax2452 = reinterpret_cast<signed char>(*eax2453 + al2454);
    *eax2455 = reinterpret_cast<signed char>(*eax2456 + al2457);
    *eax2458 = reinterpret_cast<signed char>(*eax2459 + al2460);
    *eax2461 = reinterpret_cast<signed char>(*eax2462 + al2463);
    *eax2464 = reinterpret_cast<signed char>(*eax2465 + al2466);
    *eax2467 = reinterpret_cast<signed char>(*eax2468 + al2469);
    *eax2470 = reinterpret_cast<signed char>(*eax2471 + al2472);
    *eax2473 = reinterpret_cast<signed char>(*eax2474 + al2475);
    *eax2476 = reinterpret_cast<signed char>(*eax2477 + al2478);
    *eax2479 = reinterpret_cast<signed char>(*eax2480 + al2481);
    *eax2482 = reinterpret_cast<signed char>(*eax2483 + al2484);
    *eax2485 = reinterpret_cast<signed char>(*eax2486 + al2487);
    *eax2488 = reinterpret_cast<signed char>(*eax2489 + al2490);
    *eax2491 = reinterpret_cast<signed char>(*eax2492 + al2493);
    *eax2494 = reinterpret_cast<signed char>(*eax2495 + al2496);
    *eax2497 = reinterpret_cast<signed char>(*eax2498 + al2499);
    *eax2500 = reinterpret_cast<signed char>(*eax2501 + al2502);
    *eax2503 = reinterpret_cast<signed char>(*eax2504 + al2505);
    *eax2506 = reinterpret_cast<signed char>(*eax2507 + al2508);
    *eax2509 = reinterpret_cast<signed char>(*eax2510 + al2511);
    *eax2512 = reinterpret_cast<signed char>(*eax2513 + al2514);
    *eax2515 = reinterpret_cast<signed char>(*eax2516 + al2517);
    *eax2518 = reinterpret_cast<signed char>(*eax2519 + al2520);
    *eax2521 = reinterpret_cast<signed char>(*eax2522 + al2523);
    *eax2524 = reinterpret_cast<signed char>(*eax2525 + al2526);
    *eax2527 = reinterpret_cast<signed char>(*eax2528 + al2529);
    *eax2530 = reinterpret_cast<signed char>(*eax2531 + al2532);
    *eax2533 = reinterpret_cast<signed char>(*eax2534 + al2535);
    *eax2536 = reinterpret_cast<signed char>(*eax2537 + al2538);
    *eax2539 = reinterpret_cast<signed char>(*eax2540 + al2541);
    *eax2542 = reinterpret_cast<signed char>(*eax2543 + al2544);
    *eax2545 = reinterpret_cast<signed char>(*eax2546 + al2547);
    *eax2548 = reinterpret_cast<signed char>(*eax2549 + al2550);
    *eax2551 = reinterpret_cast<signed char>(*eax2552 + al2553);
    *eax2554 = reinterpret_cast<signed char>(*eax2555 + al2556);
    *eax2557 = reinterpret_cast<signed char>(*eax2558 + al2559);
    *eax2560 = reinterpret_cast<signed char>(*eax2561 + al2562);
    *eax2563 = reinterpret_cast<signed char>(*eax2564 + al2565);
    *eax2566 = reinterpret_cast<signed char>(*eax2567 + al2568);
    *eax2569 = reinterpret_cast<signed char>(*eax2570 + al2571);
    *eax2572 = reinterpret_cast<signed char>(*eax2573 + al2574);
    *eax2575 = reinterpret_cast<signed char>(*eax2576 + al2577);
    *eax2578 = reinterpret_cast<signed char>(*eax2579 + al2580);
    *eax2581 = reinterpret_cast<signed char>(*eax2582 + al2583);
    *eax2584 = reinterpret_cast<signed char>(*eax2585 + al2586);
    *eax2587 = reinterpret_cast<signed char>(*eax2588 + al2589);
    *eax2590 = reinterpret_cast<signed char>(*eax2591 + al2592);
    *eax2593 = reinterpret_cast<signed char>(*eax2594 + al2595);
    *eax2596 = reinterpret_cast<signed char>(*eax2597 + al2598);
    *eax2599 = reinterpret_cast<signed char>(*eax2600 + al2601);
    *eax2602 = reinterpret_cast<signed char>(*eax2603 + al2604);
    *eax2605 = reinterpret_cast<signed char>(*eax2606 + al2607);
    *eax2608 = reinterpret_cast<signed char>(*eax2609 + al2610);
    *eax2611 = reinterpret_cast<signed char>(*eax2612 + al2613);
    *eax2614 = reinterpret_cast<signed char>(*eax2615 + al2616);
    *eax2617 = reinterpret_cast<signed char>(*eax2618 + al2619);
    *eax2620 = reinterpret_cast<signed char>(*eax2621 + al2622);
    *eax2623 = reinterpret_cast<signed char>(*eax2624 + al2625);
    *eax2626 = reinterpret_cast<signed char>(*eax2627 + al2628);
    *eax2629 = reinterpret_cast<signed char>(*eax2630 + al2631);
    *eax2632 = reinterpret_cast<signed char>(*eax2633 + al2634);
    *eax2635 = reinterpret_cast<signed char>(*eax2636 + al2637);
    *eax2638 = reinterpret_cast<signed char>(*eax2639 + al2640);
    *eax2641 = reinterpret_cast<signed char>(*eax2642 + al2643);
    *eax2644 = reinterpret_cast<signed char>(*eax2645 + al2646);
    *eax2647 = reinterpret_cast<signed char>(*eax2648 + al2649);
    *eax2650 = reinterpret_cast<signed char>(*eax2651 + al2652);
    *eax2653 = reinterpret_cast<signed char>(*eax2654 + al2655);
    *eax2656 = reinterpret_cast<signed char>(*eax2657 + al2658);
    *eax2659 = reinterpret_cast<signed char>(*eax2660 + al2661);
    *eax2662 = reinterpret_cast<signed char>(*eax2663 + al2664);
    *eax2665 = reinterpret_cast<signed char>(*eax2666 + al2667);
    *eax2668 = reinterpret_cast<signed char>(*eax2669 + al2670);
    *eax2671 = reinterpret_cast<signed char>(*eax2672 + al2673);
    *eax2674 = reinterpret_cast<signed char>(*eax2675 + al2676);
    *eax2677 = reinterpret_cast<signed char>(*eax2678 + al2679);
    *eax2680 = reinterpret_cast<signed char>(*eax2681 + al2682);
    *eax2683 = reinterpret_cast<signed char>(*eax2684 + al2685);
    *eax2686 = reinterpret_cast<signed char>(*eax2687 + al2688);
    *eax2689 = reinterpret_cast<signed char>(*eax2690 + al2691);
    *eax2692 = reinterpret_cast<signed char>(*eax2693 + al2694);
    *eax2695 = reinterpret_cast<signed char>(*eax2696 + al2697);
    *eax2698 = reinterpret_cast<signed char>(*eax2699 + al2700);
    *eax2701 = reinterpret_cast<signed char>(*eax2702 + al2703);
    *eax2704 = reinterpret_cast<signed char>(*eax2705 + al2706);
    *eax2707 = reinterpret_cast<signed char>(*eax2708 + al2709);
    *eax2710 = reinterpret_cast<signed char>(*eax2711 + al2712);
    *eax2713 = reinterpret_cast<signed char>(*eax2714 + al2715);
    *eax2716 = reinterpret_cast<signed char>(*eax2717 + al2718);
    *eax2719 = reinterpret_cast<signed char>(*eax2720 + al2721);
    *eax2722 = reinterpret_cast<signed char>(*eax2723 + al2724);
    *eax2725 = reinterpret_cast<signed char>(*eax2726 + al2727);
    *eax2728 = reinterpret_cast<signed char>(*eax2729 + al2730);
    *eax2731 = reinterpret_cast<signed char>(*eax2732 + al2733);
    *eax2734 = reinterpret_cast<signed char>(*eax2735 + al2736);
    *eax2737 = reinterpret_cast<signed char>(*eax2738 + al2739);
    *eax2740 = reinterpret_cast<signed char>(*eax2741 + al2742);
    *eax2743 = reinterpret_cast<signed char>(*eax2744 + al2745);
    *eax2746 = reinterpret_cast<signed char>(*eax2747 + al2748);
    *eax2749 = reinterpret_cast<signed char>(*eax2750 + al2751);
    *eax2752 = reinterpret_cast<signed char>(*eax2753 + al2754);
    *eax2755 = reinterpret_cast<signed char>(*eax2756 + al2757);
    *eax2758 = reinterpret_cast<signed char>(*eax2759 + al2760);
    *eax2761 = reinterpret_cast<signed char>(*eax2762 + al2763);
    *eax2764 = reinterpret_cast<signed char>(*eax2765 + al2766);
    *eax2767 = reinterpret_cast<signed char>(*eax2768 + al2769);
    *eax2770 = reinterpret_cast<signed char>(*eax2771 + al2772);
    *eax2773 = reinterpret_cast<signed char>(*eax2774 + al2775);
    *eax2776 = reinterpret_cast<signed char>(*eax2777 + al2778);
    *eax2779 = reinterpret_cast<signed char>(*eax2780 + al2781);
    *eax2782 = reinterpret_cast<signed char>(*eax2783 + al2784);
    *eax2785 = reinterpret_cast<signed char>(*eax2786 + al2787);
    *eax2788 = reinterpret_cast<signed char>(*eax2789 + al2790);
    *eax2791 = reinterpret_cast<signed char>(*eax2792 + al2793);
    *eax2794 = reinterpret_cast<signed char>(*eax2795 + al2796);
    *eax2797 = reinterpret_cast<signed char>(*eax2798 + al2799);
    *eax2800 = reinterpret_cast<signed char>(*eax2801 + al2802);
    *eax2803 = reinterpret_cast<signed char>(*eax2804 + al2805);
    *eax2806 = reinterpret_cast<signed char>(*eax2807 + al2808);
    *eax2809 = reinterpret_cast<signed char>(*eax2810 + al2811);
    *eax2812 = reinterpret_cast<signed char>(*eax2813 + al2814);
    *eax2815 = reinterpret_cast<signed char>(*eax2816 + al2817);
    *eax2818 = reinterpret_cast<signed char>(*eax2819 + al2820);
    *eax2821 = reinterpret_cast<signed char>(*eax2822 + al2823);
    *eax2824 = reinterpret_cast<signed char>(*eax2825 + al2826);
    *eax2827 = reinterpret_cast<signed char>(*eax2828 + al2829);
    *eax2830 = reinterpret_cast<signed char>(*eax2831 + al2832);
    *eax2833 = reinterpret_cast<signed char>(*eax2834 + al2835);
    *eax2836 = reinterpret_cast<signed char>(*eax2837 + al2838);
    *eax2839 = reinterpret_cast<signed char>(*eax2840 + al2841);
    *eax2842 = reinterpret_cast<signed char>(*eax2843 + al2844);
    *eax2845 = reinterpret_cast<signed char>(*eax2846 + al2847);
    *eax2848 = reinterpret_cast<signed char>(*eax2849 + al2850);
    *eax2851 = reinterpret_cast<signed char>(*eax2852 + al2853);
    *eax2854 = reinterpret_cast<signed char>(*eax2855 + al2856);
    *eax2857 = reinterpret_cast<signed char>(*eax2858 + al2859);
    *eax2860 = reinterpret_cast<signed char>(*eax2861 + al2862);
    *eax2863 = reinterpret_cast<signed char>(*eax2864 + al2865);
    *eax2866 = reinterpret_cast<signed char>(*eax2867 + al2868);
    *eax2869 = reinterpret_cast<signed char>(*eax2870 + al2871);
    *eax2872 = reinterpret_cast<signed char>(*eax2873 + al2874);
    *eax2875 = reinterpret_cast<signed char>(*eax2876 + al2877);
    *eax2878 = reinterpret_cast<signed char>(*eax2879 + al2880);
    *eax2881 = reinterpret_cast<signed char>(*eax2882 + al2883);
    *eax2884 = reinterpret_cast<signed char>(*eax2885 + al2886);
    *eax2887 = reinterpret_cast<signed char>(*eax2888 + al2889);
    *eax2890 = reinterpret_cast<signed char>(*eax2891 + al2892);
    *eax2893 = reinterpret_cast<signed char>(*eax2894 + al2895);
    *eax2896 = reinterpret_cast<signed char>(*eax2897 + al2898);
    *eax2899 = reinterpret_cast<signed char>(*eax2900 + al2901);
    *eax2902 = reinterpret_cast<signed char>(*eax2903 + al2904);
    *eax2905 = reinterpret_cast<signed char>(*eax2906 + al2907);
    *eax2908 = reinterpret_cast<signed char>(*eax2909 + al2910);
    *eax2911 = reinterpret_cast<signed char>(*eax2912 + al2913);
    *eax2914 = reinterpret_cast<signed char>(*eax2915 + al2916);
    *eax2917 = reinterpret_cast<signed char>(*eax2918 + al2919);
    *eax2920 = reinterpret_cast<signed char>(*eax2921 + al2922);
    *eax2923 = reinterpret_cast<signed char>(*eax2924 + al2925);
    *eax2926 = reinterpret_cast<signed char>(*eax2927 + al2928);
    *eax2929 = reinterpret_cast<signed char>(*eax2930 + al2931);
    *eax2932 = reinterpret_cast<signed char>(*eax2933 + al2934);
    *eax2935 = reinterpret_cast<signed char>(*eax2936 + al2937);
    *eax2938 = reinterpret_cast<signed char>(*eax2939 + al2940);
    *eax2941 = reinterpret_cast<signed char>(*eax2942 + al2943);
    *eax2944 = reinterpret_cast<signed char>(*eax2945 + al2946);
    *eax2947 = reinterpret_cast<signed char>(*eax2948 + al2949);
    *eax2950 = reinterpret_cast<signed char>(*eax2951 + al2952);
    *eax2953 = reinterpret_cast<signed char>(*eax2954 + al2955);
    *eax2956 = reinterpret_cast<signed char>(*eax2957 + al2958);
    *eax2959 = reinterpret_cast<signed char>(*eax2960 + al2961);
    *eax2962 = reinterpret_cast<signed char>(*eax2963 + al2964);
    *eax2965 = reinterpret_cast<signed char>(*eax2966 + al2967);
    *eax2968 = reinterpret_cast<signed char>(*eax2969 + al2970);
    *eax2971 = reinterpret_cast<signed char>(*eax2972 + al2973);
    *eax2974 = reinterpret_cast<signed char>(*eax2975 + al2976);
    *eax2977 = reinterpret_cast<signed char>(*eax2978 + al2979);
    *eax2980 = reinterpret_cast<signed char>(*eax2981 + al2982);
    *eax2983 = reinterpret_cast<signed char>(*eax2984 + al2985);
    *eax2986 = reinterpret_cast<signed char>(*eax2987 + al2988);
    *eax2989 = reinterpret_cast<signed char>(*eax2990 + al2991);
    *eax2992 = reinterpret_cast<signed char>(*eax2993 + al2994);
    *eax2995 = reinterpret_cast<signed char>(*eax2996 + al2997);
    *eax2998 = reinterpret_cast<signed char>(*eax2999 + al3000);
    *eax3001 = reinterpret_cast<signed char>(*eax3002 + al3003);
    *eax3004 = reinterpret_cast<signed char>(*eax3005 + al3006);
    *eax3007 = reinterpret_cast<signed char>(*eax3008 + al3009);
    *eax3010 = reinterpret_cast<signed char>(*eax3011 + al3012);
    *eax3013 = reinterpret_cast<signed char>(*eax3014 + al3015);
    *eax3016 = reinterpret_cast<signed char>(*eax3017 + al3018);
    *eax3019 = reinterpret_cast<signed char>(*eax3020 + al3021);
    *eax3022 = reinterpret_cast<signed char>(*eax3023 + al3024);
    *eax3025 = reinterpret_cast<signed char>(*eax3026 + al3027);
    *eax3028 = reinterpret_cast<signed char>(*eax3029 + al3030);
    *eax3031 = reinterpret_cast<signed char>(*eax3032 + al3033);
    *eax3034 = reinterpret_cast<signed char>(*eax3035 + al3036);
    *eax3037 = reinterpret_cast<signed char>(*eax3038 + al3039);
    *eax3040 = reinterpret_cast<signed char>(*eax3041 + al3042);
    *eax3043 = reinterpret_cast<signed char>(*eax3044 + al3045);
    *eax3046 = reinterpret_cast<signed char>(*eax3047 + al3048);
    *eax3049 = reinterpret_cast<signed char>(*eax3050 + al3051);
    *eax3052 = reinterpret_cast<signed char>(*eax3053 + al3054);
    *eax3055 = reinterpret_cast<signed char>(*eax3056 + al3057);
    *eax3058 = reinterpret_cast<signed char>(*eax3059 + al3060);
    *eax3061 = reinterpret_cast<signed char>(*eax3062 + al3063);
    *eax3064 = reinterpret_cast<signed char>(*eax3065 + al3066);
    *eax3067 = reinterpret_cast<signed char>(*eax3068 + al3069);
    *eax3070 = reinterpret_cast<signed char>(*eax3071 + al3072);
    *eax3073 = reinterpret_cast<signed char>(*eax3074 + al3075);
    *eax3076 = reinterpret_cast<signed char>(*eax3077 + al3078);
    *eax3079 = reinterpret_cast<signed char>(*eax3080 + al3081);
    *eax3082 = reinterpret_cast<signed char>(*eax3083 + al3084);
    *eax3085 = reinterpret_cast<signed char>(*eax3086 + al3087);
    *eax3088 = reinterpret_cast<signed char>(*eax3089 + al3090);
    *eax3091 = reinterpret_cast<signed char>(*eax3092 + al3093);
    *eax3094 = reinterpret_cast<signed char>(*eax3095 + al3096);
    *eax3097 = reinterpret_cast<signed char>(*eax3098 + al3099);
    *eax3100 = reinterpret_cast<signed char>(*eax3101 + al3102);
    *eax3103 = reinterpret_cast<signed char>(*eax3104 + al3105);
    *eax3106 = reinterpret_cast<signed char>(*eax3107 + al3108);
    *eax3109 = reinterpret_cast<signed char>(*eax3110 + al3111);
    *eax3112 = reinterpret_cast<signed char>(*eax3113 + al3114);
    *eax3115 = reinterpret_cast<signed char>(*eax3116 + al3117);
    *eax3118 = reinterpret_cast<signed char>(*eax3119 + al3120);
    *eax3121 = reinterpret_cast<signed char>(*eax3122 + al3123);
    *eax3124 = reinterpret_cast<signed char>(*eax3125 + al3126);
    *eax3127 = reinterpret_cast<signed char>(*eax3128 + al3129);
    *eax3130 = reinterpret_cast<signed char>(*eax3131 + al3132);
    *eax3133 = reinterpret_cast<signed char>(*eax3134 + al3135);
    *eax3136 = reinterpret_cast<signed char>(*eax3137 + al3138);
    *eax3139 = reinterpret_cast<signed char>(*eax3140 + al3141);
    *eax3142 = reinterpret_cast<signed char>(*eax3143 + al3144);
    *eax3145 = reinterpret_cast<signed char>(*eax3146 + al3147);
    *eax3148 = reinterpret_cast<signed char>(*eax3149 + al3150);
    *eax3151 = reinterpret_cast<signed char>(*eax3152 + al3153);
    *eax3154 = reinterpret_cast<signed char>(*eax3155 + al3156);
    *eax3157 = reinterpret_cast<signed char>(*eax3158 + al3159);
    *eax3160 = reinterpret_cast<signed char>(*eax3161 + al3162);
    *eax3163 = reinterpret_cast<signed char>(*eax3164 + al3165);
    *eax3166 = reinterpret_cast<signed char>(*eax3167 + al3168);
    *eax3169 = reinterpret_cast<signed char>(*eax3170 + al3171);
    *eax3172 = reinterpret_cast<signed char>(*eax3173 + al3174);
    *eax3175 = reinterpret_cast<signed char>(*eax3176 + al3177);
    *eax3178 = reinterpret_cast<signed char>(*eax3179 + al3180);
    *eax3181 = reinterpret_cast<signed char>(*eax3182 + al3183);
    *eax3184 = reinterpret_cast<signed char>(*eax3185 + al3186);
    *eax3187 = reinterpret_cast<signed char>(*eax3188 + al3189);
    *eax3190 = reinterpret_cast<signed char>(*eax3191 + al3192);
    *eax3193 = reinterpret_cast<signed char>(*eax3194 + al3195);
    *eax3196 = reinterpret_cast<signed char>(*eax3197 + al3198);
    *eax3199 = reinterpret_cast<signed char>(*eax3200 + al3201);
    *eax3202 = reinterpret_cast<signed char>(*eax3203 + al3204);
    *eax3205 = reinterpret_cast<signed char>(*eax3206 + al3207);
    *eax3208 = reinterpret_cast<signed char>(*eax3209 + al3210);
    *eax3211 = reinterpret_cast<signed char>(*eax3212 + al3213);
    *eax3214 = reinterpret_cast<signed char>(*eax3215 + al3216);
    *eax3217 = reinterpret_cast<signed char>(*eax3218 + al3219);
    *eax3220 = reinterpret_cast<signed char>(*eax3221 + al3222);
    *eax3223 = reinterpret_cast<signed char>(*eax3224 + al3225);
    *eax3226 = reinterpret_cast<signed char>(*eax3227 + al3228);
    *eax3229 = reinterpret_cast<signed char>(*eax3230 + al3231);
    *eax3232 = reinterpret_cast<signed char>(*eax3233 + al3234);
    *eax3235 = reinterpret_cast<signed char>(*eax3236 + al3237);
    *eax3238 = reinterpret_cast<signed char>(*eax3239 + al3240);
    *eax3241 = reinterpret_cast<signed char>(*eax3242 + al3243);
    *eax3244 = reinterpret_cast<signed char>(*eax3245 + al3246);
    *eax3247 = reinterpret_cast<signed char>(*eax3248 + al3249);
    *eax3250 = reinterpret_cast<signed char>(*eax3251 + al3252);
    *eax3253 = reinterpret_cast<signed char>(*eax3254 + al3255);
    *eax3256 = reinterpret_cast<signed char>(*eax3257 + al3258);
    *eax3259 = reinterpret_cast<signed char>(*eax3260 + al3261);
    *eax3262 = reinterpret_cast<signed char>(*eax3263 + al3264);
    *eax3265 = reinterpret_cast<signed char>(*eax3266 + al3267);
    *eax3268 = reinterpret_cast<signed char>(*eax3269 + al3270);
    *eax3271 = reinterpret_cast<signed char>(*eax3272 + al3273);
    *eax3274 = reinterpret_cast<signed char>(*eax3275 + al3276);
    *eax3277 = reinterpret_cast<signed char>(*eax3278 + al3279);
    *eax3280 = reinterpret_cast<signed char>(*eax3281 + al3282);
    *eax3283 = reinterpret_cast<signed char>(*eax3284 + al3285);
    *eax3286 = reinterpret_cast<signed char>(*eax3287 + al3288);
    *eax3289 = reinterpret_cast<signed char>(*eax3290 + al3291);
    *eax3292 = reinterpret_cast<signed char>(*eax3293 + al3294);
    *eax3295 = reinterpret_cast<signed char>(*eax3296 + al3297);
    *eax3298 = reinterpret_cast<signed char>(*eax3299 + al3300);
    *eax3301 = reinterpret_cast<signed char>(*eax3302 + al3303);
    *eax3304 = reinterpret_cast<signed char>(*eax3305 + al3306);
    *eax3307 = reinterpret_cast<signed char>(*eax3308 + al3309);
    *eax3310 = reinterpret_cast<signed char>(*eax3311 + al3312);
    *eax3313 = reinterpret_cast<signed char>(*eax3314 + al3315);
    *eax3316 = reinterpret_cast<signed char>(*eax3317 + al3318);
    *eax3319 = reinterpret_cast<signed char>(*eax3320 + al3321);
    *eax3322 = reinterpret_cast<signed char>(*eax3323 + al3324);
    *eax3325 = reinterpret_cast<signed char>(*eax3326 + al3327);
    *eax3328 = reinterpret_cast<signed char>(*eax3329 + al3330);
    *eax3331 = reinterpret_cast<signed char>(*eax3332 + al3333);
    *eax3334 = reinterpret_cast<signed char>(*eax3335 + al3336);
    *eax3337 = reinterpret_cast<signed char>(*eax3338 + al3339);
    *eax3340 = reinterpret_cast<signed char>(*eax3341 + al3342);
    *eax3343 = reinterpret_cast<signed char>(*eax3344 + al3345);
    *eax3346 = reinterpret_cast<signed char>(*eax3347 + al3348);
    *eax3349 = reinterpret_cast<signed char>(*eax3350 + al3351);
    *eax3352 = reinterpret_cast<signed char>(*eax3353 + al3354);
    *eax3355 = reinterpret_cast<signed char>(*eax3356 + al3357);
    *eax3358 = reinterpret_cast<signed char>(*eax3359 + al3360);
    *eax3361 = reinterpret_cast<signed char>(*eax3362 + al3363);
    *eax3364 = reinterpret_cast<signed char>(*eax3365 + al3366);
    *eax3367 = reinterpret_cast<signed char>(*eax3368 + al3369);
    *eax3370 = reinterpret_cast<signed char>(*eax3371 + al3372);
    *eax3373 = reinterpret_cast<signed char>(*eax3374 + al3375);
    *eax3376 = reinterpret_cast<signed char>(*eax3377 + al3378);
    *eax3379 = reinterpret_cast<signed char>(*eax3380 + al3381);
    *eax3382 = reinterpret_cast<signed char>(*eax3383 + al3384);
    *eax3385 = reinterpret_cast<signed char>(*eax3386 + al3387);
    *eax3388 = reinterpret_cast<signed char>(*eax3389 + al3390);
    *eax3391 = reinterpret_cast<signed char>(*eax3392 + al3393);
    *eax3394 = reinterpret_cast<signed char>(*eax3395 + al3396);
    *eax3397 = reinterpret_cast<signed char>(*eax3398 + al3399);
    *eax3400 = reinterpret_cast<signed char>(*eax3401 + al3402);
    *eax3403 = reinterpret_cast<signed char>(*eax3404 + al3405);
    *eax3406 = reinterpret_cast<signed char>(*eax3407 + al3408);
    *eax3409 = reinterpret_cast<signed char>(*eax3410 + al3411);
    *eax3412 = reinterpret_cast<signed char>(*eax3413 + al3414);
    *eax3415 = reinterpret_cast<signed char>(*eax3416 + al3417);
    *eax3418 = reinterpret_cast<signed char>(*eax3419 + al3420);
    *eax3421 = reinterpret_cast<signed char>(*eax3422 + al3423);
    *eax3424 = reinterpret_cast<signed char>(*eax3425 + al3426);
    *eax3427 = reinterpret_cast<signed char>(*eax3428 + al3429);
    *eax3430 = reinterpret_cast<signed char>(*eax3431 + al3432);
    *eax3433 = reinterpret_cast<signed char>(*eax3434 + al3435);
    *eax3436 = reinterpret_cast<signed char>(*eax3437 + al3438);
    *eax3439 = reinterpret_cast<signed char>(*eax3440 + al3441);
    *eax3442 = reinterpret_cast<signed char>(*eax3443 + al3444);
    *eax3445 = reinterpret_cast<signed char>(*eax3446 + al3447);
    *eax3448 = reinterpret_cast<signed char>(*eax3449 + al3450);
    *eax3451 = reinterpret_cast<signed char>(*eax3452 + al3453);
    *eax3454 = reinterpret_cast<signed char>(*eax3455 + al3456);
    *eax3457 = reinterpret_cast<signed char>(*eax3458 + al3459);
    *eax3460 = reinterpret_cast<signed char>(*eax3461 + al3462);
    *eax3463 = reinterpret_cast<signed char>(*eax3464 + al3465);
    *eax3466 = reinterpret_cast<signed char>(*eax3467 + al3468);
    *eax3469 = reinterpret_cast<signed char>(*eax3470 + al3471);
    *eax3472 = reinterpret_cast<signed char>(*eax3473 + al3474);
    *eax3475 = reinterpret_cast<signed char>(*eax3476 + al3477);
    *eax3478 = reinterpret_cast<signed char>(*eax3479 + al3480);
    *eax3481 = reinterpret_cast<signed char>(*eax3482 + al3483);
    *eax3484 = reinterpret_cast<signed char>(*eax3485 + al3486);
    *eax3487 = reinterpret_cast<signed char>(*eax3488 + al3489);
    *eax3490 = reinterpret_cast<signed char>(*eax3491 + al3492);
    *eax3493 = reinterpret_cast<signed char>(*eax3494 + al3495);
    *eax3496 = reinterpret_cast<signed char>(*eax3497 + al3498);
    *eax3499 = reinterpret_cast<signed char>(*eax3500 + al3501);
    *eax3502 = reinterpret_cast<signed char>(*eax3503 + al3504);
    *eax3505 = reinterpret_cast<signed char>(*eax3506 + al3507);
    *eax3508 = reinterpret_cast<signed char>(*eax3509 + al3510);
    *eax3511 = reinterpret_cast<signed char>(*eax3512 + al3513);
    *eax3514 = reinterpret_cast<signed char>(*eax3515 + al3516);
    *eax3517 = reinterpret_cast<signed char>(*eax3518 + al3519);
    *eax3520 = reinterpret_cast<signed char>(*eax3521 + al3522);
    *eax3523 = reinterpret_cast<signed char>(*eax3524 + al3525);
    *eax3526 = reinterpret_cast<signed char>(*eax3527 + al3528);
    *eax3529 = reinterpret_cast<signed char>(*eax3530 + al3531);
    *eax3532 = reinterpret_cast<signed char>(*eax3533 + al3534);
    *eax3535 = reinterpret_cast<signed char>(*eax3536 + al3537);
    *eax3538 = reinterpret_cast<signed char>(*eax3539 + al3540);
    *eax3541 = reinterpret_cast<signed char>(*eax3542 + al3543);
    *eax3544 = reinterpret_cast<signed char>(*eax3545 + al3546);
    *eax3547 = reinterpret_cast<signed char>(*eax3548 + al3549);
    *eax3550 = reinterpret_cast<signed char>(*eax3551 + al3552);
    *eax3553 = reinterpret_cast<signed char>(*eax3554 + al3555);
    *eax3556 = reinterpret_cast<signed char>(*eax3557 + al3558);
    *eax3559 = reinterpret_cast<signed char>(*eax3560 + al3561);
    *eax3562 = reinterpret_cast<signed char>(*eax3563 + al3564);
    *eax3565 = reinterpret_cast<signed char>(*eax3566 + al3567);
    *eax3568 = reinterpret_cast<signed char>(*eax3569 + al3570);
    *eax3571 = reinterpret_cast<signed char>(*eax3572 + al3573);
    *eax3574 = reinterpret_cast<signed char>(*eax3575 + al3576);
    *eax3577 = reinterpret_cast<signed char>(*eax3578 + al3579);
    *eax3580 = reinterpret_cast<signed char>(*eax3581 + al3582);
    *eax3583 = reinterpret_cast<signed char>(*eax3584 + al3585);
    *eax3586 = reinterpret_cast<signed char>(*eax3587 + al3588);
    *eax3589 = reinterpret_cast<signed char>(*eax3590 + al3591);
    *eax3592 = reinterpret_cast<signed char>(*eax3593 + al3594);
    *eax3595 = reinterpret_cast<signed char>(*eax3596 + al3597);
    *eax3598 = reinterpret_cast<signed char>(*eax3599 + al3600);
    *eax3601 = reinterpret_cast<signed char>(*eax3602 + al3603);
    *eax3604 = reinterpret_cast<signed char>(*eax3605 + al3606);
    *eax3607 = reinterpret_cast<signed char>(*eax3608 + al3609);
    *eax3610 = reinterpret_cast<signed char>(*eax3611 + al3612);
    *eax3613 = reinterpret_cast<signed char>(*eax3614 + al3615);
    *eax3616 = reinterpret_cast<signed char>(*eax3617 + al3618);
    *eax3619 = reinterpret_cast<signed char>(*eax3620 + al3621);
    *eax3622 = reinterpret_cast<signed char>(*eax3623 + al3624);
    *eax3625 = reinterpret_cast<signed char>(*eax3626 + al3627);
    *eax3628 = reinterpret_cast<signed char>(*eax3629 + al3630);
    *eax3631 = reinterpret_cast<signed char>(*eax3632 + al3633);
    *eax3634 = reinterpret_cast<signed char>(*eax3635 + al3636);
    *eax3637 = reinterpret_cast<signed char>(*eax3638 + al3639);
    *eax3640 = reinterpret_cast<signed char>(*eax3641 + al3642);
    *eax3643 = reinterpret_cast<signed char>(*eax3644 + al3645);
    *eax3646 = reinterpret_cast<signed char>(*eax3647 + al3648);
    *eax3649 = reinterpret_cast<signed char>(*eax3650 + al3651);
    *eax3652 = reinterpret_cast<signed char>(*eax3653 + al3654);
    *eax3655 = reinterpret_cast<signed char>(*eax3656 + al3657);
    *eax3658 = reinterpret_cast<signed char>(*eax3659 + al3660);
    *eax3661 = reinterpret_cast<signed char>(*eax3662 + al3663);
    *eax3664 = reinterpret_cast<signed char>(*eax3665 + al3666);
    *eax3667 = reinterpret_cast<signed char>(*eax3668 + al3669);
    *eax3670 = reinterpret_cast<signed char>(*eax3671 + al3672);
    *eax3673 = reinterpret_cast<signed char>(*eax3674 + al3675);
    *eax3676 = reinterpret_cast<signed char>(*eax3677 + al3678);
    *eax3679 = reinterpret_cast<signed char>(*eax3680 + al3681);
    *eax3682 = reinterpret_cast<signed char>(*eax3683 + al3684);
    *eax3685 = reinterpret_cast<signed char>(*eax3686 + al3687);
    *eax3688 = reinterpret_cast<signed char>(*eax3689 + al3690);
    *eax3691 = reinterpret_cast<signed char>(*eax3692 + al3693);
    *eax3694 = reinterpret_cast<signed char>(*eax3695 + al3696);
    *eax3697 = reinterpret_cast<signed char>(*eax3698 + al3699);
    *eax3700 = reinterpret_cast<signed char>(*eax3701 + al3702);
    *eax3703 = reinterpret_cast<signed char>(*eax3704 + al3705);
    *eax3706 = reinterpret_cast<signed char>(*eax3707 + al3708);
    *eax3709 = reinterpret_cast<signed char>(*eax3710 + al3711);
    *eax3712 = reinterpret_cast<signed char>(*eax3713 + al3714);
    *eax3715 = reinterpret_cast<signed char>(*eax3716 + al3717);
    *eax3718 = reinterpret_cast<signed char>(*eax3719 + al3720);
    *eax3721 = reinterpret_cast<signed char>(*eax3722 + al3723);
    *eax3724 = reinterpret_cast<signed char>(*eax3725 + al3726);
    *eax3727 = reinterpret_cast<signed char>(*eax3728 + al3729);
    *eax3730 = reinterpret_cast<signed char>(*eax3731 + al3732);
    *eax3733 = reinterpret_cast<signed char>(*eax3734 + al3735);
    *eax3736 = reinterpret_cast<signed char>(*eax3737 + al3738);
    *eax3739 = reinterpret_cast<signed char>(*eax3740 + al3741);
    *eax3742 = reinterpret_cast<signed char>(*eax3743 + al3744);
    *eax3745 = reinterpret_cast<signed char>(*eax3746 + al3747);
    *eax3748 = reinterpret_cast<signed char>(*eax3749 + al3750);
    *eax3751 = reinterpret_cast<signed char>(*eax3752 + al3753);
    *eax3754 = reinterpret_cast<signed char>(*eax3755 + al3756);
    *eax3757 = reinterpret_cast<signed char>(*eax3758 + al3759);
    *eax3760 = reinterpret_cast<signed char>(*eax3761 + al3762);
    *eax3763 = reinterpret_cast<signed char>(*eax3764 + al3765);
    *eax3766 = reinterpret_cast<signed char>(*eax3767 + al3768);
    *eax3769 = reinterpret_cast<signed char>(*eax3770 + al3771);
    *eax3772 = reinterpret_cast<signed char>(*eax3773 + al3774);
    *eax3775 = reinterpret_cast<signed char>(*eax3776 + al3777);
    *eax3778 = reinterpret_cast<signed char>(*eax3779 + al3780);
    *eax3781 = reinterpret_cast<signed char>(*eax3782 + al3783);
    *eax3784 = reinterpret_cast<signed char>(*eax3785 + al3786);
    *eax3787 = reinterpret_cast<signed char>(*eax3788 + al3789);
    *eax3790 = reinterpret_cast<signed char>(*eax3791 + al3792);
    *eax3793 = reinterpret_cast<signed char>(*eax3794 + al3795);
    *eax3796 = reinterpret_cast<signed char>(*eax3797 + al3798);
    *eax3799 = reinterpret_cast<signed char>(*eax3800 + al3801);
    *eax3802 = reinterpret_cast<signed char>(*eax3803 + al3804);
    *eax3805 = reinterpret_cast<signed char>(*eax3806 + al3807);
    *eax3808 = reinterpret_cast<signed char>(*eax3809 + al3810);
    *eax3811 = reinterpret_cast<signed char>(*eax3812 + al3813);
    *eax3814 = reinterpret_cast<signed char>(*eax3815 + al3816);
    *eax3817 = reinterpret_cast<signed char>(*eax3818 + al3819);
    *eax3820 = reinterpret_cast<signed char>(*eax3821 + al3822);
    *eax3823 = reinterpret_cast<signed char>(*eax3824 + al3825);
    *eax3826 = reinterpret_cast<signed char>(*eax3827 + al3828);
    *eax3829 = reinterpret_cast<signed char>(*eax3830 + al3831);
    *eax3832 = reinterpret_cast<signed char>(*eax3833 + al3834);
    *eax3835 = reinterpret_cast<signed char>(*eax3836 + al3837);
    *eax3838 = reinterpret_cast<signed char>(*eax3839 + al3840);
    *eax3841 = reinterpret_cast<signed char>(*eax3842 + al3843);
    *eax3844 = reinterpret_cast<signed char>(*eax3845 + al3846);
    *eax3847 = reinterpret_cast<signed char>(*eax3848 + al3849);
    *eax3850 = reinterpret_cast<signed char>(*eax3851 + al3852);
    *eax3853 = reinterpret_cast<signed char>(*eax3854 + al3855);
    *eax3856 = reinterpret_cast<signed char>(*eax3857 + al3858);
    *eax3859 = reinterpret_cast<signed char>(*eax3860 + al3861);
    *eax3862 = reinterpret_cast<signed char>(*eax3863 + al3864);
    *eax3865 = reinterpret_cast<signed char>(*eax3866 + al3867);
    *eax3868 = reinterpret_cast<signed char>(*eax3869 + al3870);
    *eax3871 = reinterpret_cast<signed char>(*eax3872 + al3873);
    *eax3874 = reinterpret_cast<signed char>(*eax3875 + al3876);
    *eax3877 = reinterpret_cast<signed char>(*eax3878 + al3879);
    *eax3880 = reinterpret_cast<signed char>(*eax3881 + al3882);
    *eax3883 = reinterpret_cast<signed char>(*eax3884 + al3885);
    *eax3886 = reinterpret_cast<signed char>(*eax3887 + al3888);
    *eax3889 = reinterpret_cast<signed char>(*eax3890 + al3891);
    *eax3892 = reinterpret_cast<signed char>(*eax3893 + al3894);
    *eax3895 = reinterpret_cast<signed char>(*eax3896 + al3897);
    *eax3898 = reinterpret_cast<signed char>(*eax3899 + al3900);
    *eax3901 = reinterpret_cast<signed char>(*eax3902 + al3903);
    *eax3904 = reinterpret_cast<signed char>(*eax3905 + al3906);
    *eax3907 = reinterpret_cast<signed char>(*eax3908 + al3909);
    *eax3910 = reinterpret_cast<signed char>(*eax3911 + al3912);
    *eax3913 = reinterpret_cast<signed char>(*eax3914 + al3915);
    *eax3916 = reinterpret_cast<signed char>(*eax3917 + al3918);
    *eax3919 = reinterpret_cast<signed char>(*eax3920 + al3921);
    *eax3922 = reinterpret_cast<signed char>(*eax3923 + al3924);
    *eax3925 = reinterpret_cast<signed char>(*eax3926 + al3927);
    *eax3928 = reinterpret_cast<signed char>(*eax3929 + al3930);
    *eax3931 = reinterpret_cast<signed char>(*eax3932 + al3933);
    *eax3934 = reinterpret_cast<signed char>(*eax3935 + al3936);
    *eax3937 = reinterpret_cast<signed char>(*eax3938 + al3939);
    *eax3940 = reinterpret_cast<signed char>(*eax3941 + al3942);
    *eax3943 = reinterpret_cast<signed char>(*eax3944 + al3945);
    *eax3946 = reinterpret_cast<signed char>(*eax3947 + al3948);
    *eax3949 = reinterpret_cast<signed char>(*eax3950 + al3951);
    *eax3952 = reinterpret_cast<signed char>(*eax3953 + al3954);
    *eax3955 = reinterpret_cast<signed char>(*eax3956 + al3957);
    *eax3958 = reinterpret_cast<signed char>(*eax3959 + al3960);
    *eax3961 = reinterpret_cast<signed char>(*eax3962 + al3963);
    *eax3964 = reinterpret_cast<signed char>(*eax3965 + al3966);
    *eax3967 = reinterpret_cast<signed char>(*eax3968 + al3969);
    *eax3970 = reinterpret_cast<signed char>(*eax3971 + al3972);
    *eax3973 = reinterpret_cast<signed char>(*eax3974 + al3975);
    *eax3976 = reinterpret_cast<signed char>(*eax3977 + al3978);
    *eax3979 = reinterpret_cast<signed char>(*eax3980 + al3981);
    *eax3982 = reinterpret_cast<signed char>(*eax3983 + al3984);
    *eax3985 = reinterpret_cast<signed char>(*eax3986 + al3987);
    *eax3988 = reinterpret_cast<signed char>(*eax3989 + al3990);
    *eax3991 = reinterpret_cast<signed char>(*eax3992 + al3993);
    *eax3994 = reinterpret_cast<signed char>(*eax3995 + al3996);
    *eax3997 = reinterpret_cast<signed char>(*eax3998 + al3999);
    *eax4000 = reinterpret_cast<signed char>(*eax4001 + al4002);
    *eax4003 = reinterpret_cast<signed char>(*eax4004 + al4005);
    *eax4006 = reinterpret_cast<signed char>(*eax4007 + al4008);
    *eax4009 = reinterpret_cast<signed char>(*eax4010 + al4011);
    *eax4012 = reinterpret_cast<signed char>(*eax4013 + al4014);
    *eax4015 = reinterpret_cast<signed char>(*eax4016 + al4017);
    *eax4018 = reinterpret_cast<signed char>(*eax4019 + al4020);
    *eax4021 = reinterpret_cast<signed char>(*eax4022 + al4023);
    *eax4024 = reinterpret_cast<signed char>(*eax4025 + al4026);
    *eax4027 = reinterpret_cast<signed char>(*eax4028 + al4029);
    *eax4030 = reinterpret_cast<signed char>(*eax4031 + al4032);
    *eax4033 = reinterpret_cast<signed char>(*eax4034 + al4035);
    *eax4036 = reinterpret_cast<signed char>(*eax4037 + al4038);
    *eax4039 = reinterpret_cast<signed char>(*eax4040 + al4041);
    *eax4042 = reinterpret_cast<signed char>(*eax4043 + al4044);
    *eax4045 = reinterpret_cast<signed char>(*eax4046 + al4047);
    *eax4048 = reinterpret_cast<signed char>(*eax4049 + al4050);
    *eax4051 = reinterpret_cast<signed char>(*eax4052 + al4053);
    *eax4054 = reinterpret_cast<signed char>(*eax4055 + al4056);
    *eax4057 = reinterpret_cast<signed char>(*eax4058 + al4059);
    *eax4060 = reinterpret_cast<signed char>(*eax4061 + al4062);
    *eax4063 = reinterpret_cast<signed char>(*eax4064 + al4065);
    *eax4066 = reinterpret_cast<signed char>(*eax4067 + al4068);
    *eax4069 = reinterpret_cast<signed char>(*eax4070 + al4071);
    *eax4072 = reinterpret_cast<signed char>(*eax4073 + al4074);
    *eax4075 = reinterpret_cast<signed char>(*eax4076 + al4077);
    *eax4078 = reinterpret_cast<signed char>(*eax4079 + al4080);
    *eax4081 = reinterpret_cast<signed char>(*eax4082 + al4083);
    *eax4084 = reinterpret_cast<signed char>(*eax4085 + al4086);
    *eax4087 = reinterpret_cast<signed char>(*eax4088 + al4089);
    *eax4090 = reinterpret_cast<signed char>(*eax4091 + al4092);
    *eax4093 = reinterpret_cast<signed char>(*eax4094 + al4095);
    *eax4096 = reinterpret_cast<signed char>(*eax4097 + al4098);
    *eax4099 = reinterpret_cast<signed char>(*eax4100 + al4101);
    *eax4102 = reinterpret_cast<signed char>(*eax4103 + al4104);
    *eax4105 = reinterpret_cast<signed char>(*eax4106 + al4107);
    *eax4108 = reinterpret_cast<signed char>(*eax4109 + al4110);
    *eax4111 = reinterpret_cast<signed char>(*eax4112 + al4113);
    *eax4114 = reinterpret_cast<signed char>(*eax4115 + al4116);
    *eax4117 = reinterpret_cast<signed char>(*eax4118 + al4119);
    *eax4120 = reinterpret_cast<signed char>(*eax4121 + al4122);
    *eax4123 = reinterpret_cast<signed char>(*eax4124 + al4125);
    *eax4126 = reinterpret_cast<signed char>(*eax4127 + al4128);
    *eax4129 = reinterpret_cast<signed char>(*eax4130 + al4131);
    *eax4132 = reinterpret_cast<signed char>(*eax4133 + al4134);
    *eax4135 = reinterpret_cast<signed char>(*eax4136 + al4137);
    *eax4138 = reinterpret_cast<signed char>(*eax4139 + al4140);
    *eax4141 = reinterpret_cast<signed char>(*eax4142 + al4143);
    *eax4144 = reinterpret_cast<signed char>(*eax4145 + al4146);
    *eax4147 = reinterpret_cast<signed char>(*eax4148 + al4149);
    *eax4150 = reinterpret_cast<signed char>(*eax4151 + al4152);
    *eax4153 = reinterpret_cast<signed char>(*eax4154 + al4155);
    *eax4156 = reinterpret_cast<signed char>(*eax4157 + al4158);
    *eax4159 = reinterpret_cast<signed char>(*eax4160 + al4161);
    *eax4162 = reinterpret_cast<signed char>(*eax4163 + al4164);
    *eax4165 = reinterpret_cast<signed char>(*eax4166 + al4167);
    *eax4168 = reinterpret_cast<signed char>(*eax4169 + al4170);
    *eax4171 = reinterpret_cast<signed char>(*eax4172 + al4173);
    *eax4174 = reinterpret_cast<signed char>(*eax4175 + al4176);
    *eax4177 = reinterpret_cast<signed char>(*eax4178 + al4179);
    *eax4180 = reinterpret_cast<signed char>(*eax4181 + al4182);
    *eax4183 = reinterpret_cast<signed char>(*eax4184 + al4185);
    *eax4186 = reinterpret_cast<signed char>(*eax4187 + al4188);
    *eax4189 = reinterpret_cast<signed char>(*eax4190 + al4191);
    *eax4192 = reinterpret_cast<signed char>(*eax4193 + al4194);
    *eax4195 = reinterpret_cast<signed char>(*eax4196 + al4197);
    *eax4198 = reinterpret_cast<signed char>(*eax4199 + al4200);
    *eax4201 = reinterpret_cast<signed char>(*eax4202 + al4203);
    *eax4204 = reinterpret_cast<signed char>(*eax4205 + al4206);
    *eax4207 = reinterpret_cast<signed char>(*eax4208 + al4209);
    *eax4210 = reinterpret_cast<signed char>(*eax4211 + al4212);
    *eax4213 = reinterpret_cast<signed char>(*eax4214 + al4215);
    *eax4216 = reinterpret_cast<signed char>(*eax4217 + al4218);
    *eax4219 = reinterpret_cast<signed char>(*eax4220 + al4221);
    *eax4222 = reinterpret_cast<signed char>(*eax4223 + al4224);
    *eax4225 = reinterpret_cast<signed char>(*eax4226 + al4227);
    *eax4228 = reinterpret_cast<signed char>(*eax4229 + al4230);
    *eax4231 = reinterpret_cast<signed char>(*eax4232 + al4233);
    *eax4234 = reinterpret_cast<signed char>(*eax4235 + al4236);
}

struct s94 {
    signed char[3] pad3;
    int32_t f3;
};

struct s95 {
    signed char[3] pad3;
    int32_t f3;
};

void fun_403ec5(uint32_t ecx) {
    int32_t* esi2;
    struct s94* esi3;
    int32_t* edi4;
    struct s95* edi5;
    int32_t edx6;

    esi2 = &esi3->f3;
    edi4 = &edi5->f3;
    if (ecx < 8) 
        goto 0x403e9c;
    while (ecx) {
        --ecx;
        *edi4 = *esi2;
        ++edi4;
        ++esi2;
    }
    switch (edx6) {
    case 3:
        goto 0x403fc0;
    case 2:
        goto 0x403fac;
    case 1:
        goto 0x403fa0;
    case 0:
        goto 0x403f98;
    }
}

void fun_404047() {
    int32_t edx1;
    int32_t edx2;
    unsigned char dh3;
    int32_t ecx4;
    int32_t* edi5;
    int32_t* esi6;
    int32_t edx7;

    *reinterpret_cast<unsigned char*>(edx1 - 74) = reinterpret_cast<unsigned char>(*reinterpret_cast<unsigned char*>(edx2 - 74) | dh3);
    while (ecx4) {
        --ecx4;
        *edi5 = *esi6;
        --edi5;
        --esi6;
    }
    switch (edx7) {
    case 3:
        goto 0x40415c;
    case 2:
        goto 0x404148;
    case 1:
        goto 0x404138;
    case 0:
        goto 0x404130;
    }
}

struct s96 {
    signed char[3] pad3;
    int32_t f3;
};

struct s97 {
    signed char[3] pad3;
    int32_t f3;
};

void fun_404d05(uint32_t ecx) {
    int32_t* esi2;
    struct s96* esi3;
    int32_t* edi4;
    struct s97* edi5;
    int32_t edx6;

    esi2 = &esi3->f3;
    edi4 = &edi5->f3;
    if (ecx < 8) 
        goto 0x404cdc;
    while (ecx) {
        --ecx;
        *edi4 = *esi2;
        ++edi4;
        ++esi2;
    }
    switch (edx6) {
    case 3:
        goto 0x404e00;
    case 2:
        goto 0x404dec;
    case 1:
        goto 0x404de0;
    case 0:
        goto 0x404dd8;
    }
}

void fun_4051e0() {
    int32_t ebp1;

    *reinterpret_cast<uint32_t*>(ebp1 - 4) = 0xffffffff;
}

