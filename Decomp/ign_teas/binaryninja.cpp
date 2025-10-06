int32_t sub_401000(int16_t* arg1, int32_t arg2)
{
    int32_t edx = 0;
    void* ebx = &data_452a20;
    int16_t* edi = arg1;
    
    for (int16_t* i = &data_452e30; i < 0x453030; )
    {
        int32_t eax;
        eax = *edi;
        ebx += 4;
        *i = eax;
        edi = &edi[1];
        i = &i[1];
        edx += 1;
        *(ebx - 4) = *(arg1 + edx + 0x1ff) << 4;
    }
    
    data_452e20 = 0x80;
    data_453034 = &arg1[0x180];
    data_453030 = 0;
    data_453038 = (arg2 << 1) + 0xfffffa00;
    data_45303c = 0;
    return 1;
}

int32_t sub_401080(int16_t* arg1, int32_t arg2)
{
    int32_t eax = data_453038;
    int32_t ebx;
    int32_t var_8 = ebx;
    int32_t esi;
    int32_t var_c = esi;
    int32_t edi;
    int32_t var_10 = edi;
    
    if (data_45303c == eax)
        return 0;
    
    int32_t var_14 = eax;
    int32_t var_18 = ebx;
    int32_t var_24 = esi;
    int32_t var_28 = edi;
    int32_t i = arg2;
    
    if (i > data_453038)
        i = data_453038;
    
    int16_t* edi_2 = arg1;
    int32_t edx = data_452e20;
    int32_t ebx_2 = data_45303c;
    char* esi_3 = data_453034 + (ebx_2 >> 1);
    uint32_t ebx_3;
    ebx_3 = data_453030;
    int32_t eax_3;
    int32_t i_1;
    
    if (ebx_2 & 1)
    {
        eax_3 = *esi_3;
        eax_3 u>>= 4;
        int32_t edx_1 = edx + eax_3;
        ebx_3 += *((edx_1 << 1) + &data_452e30);
        edx = *((edx_1 << 2) + &data_452a20);
        *edi_2 = ebx_3;
        edi_2 = &edi_2[1];
        esi_3 = &esi_3[1];
        i_1 = i;
        i -= 1;
    }
    
    if (!(ebx_2 & 1) || i_1 != 1)
    {
        do
        {
            eax_3 = *esi_3;
            eax_3 &= 0xf;
            int32_t edx_2 = edx + eax_3;
            ebx_3 += *((edx_2 << 1) + &data_452e30);
            int32_t edx_3 = *((edx_2 << 2) + &data_452a20);
            *edi_2 = ebx_3;
            eax_3 = *esi_3;
            eax_3 u>>= 4;
            int32_t edx_4 = edx_3 + eax_3;
            ebx_3 += *((edx_4 << 1) + &data_452e30);
            edx = *((edx_4 << 2) + &data_452a20);
            edi_2[1] = ebx_3;
            edi_2 = &edi_2[2];
            esi_3 = &esi_3[1];
            i -= 2;
        } while (i > 1);
        
        if (i >= 1)
        {
            eax_3 = *esi_3;
            eax_3 &= 0xf;
            int32_t edx_5 = edx + eax_3;
            ebx_3 += *((edx_5 << 1) + &data_452e30);
            edx = *((edx_5 << 2) + &data_452a20);
            *edi_2 = ebx_3;
        }
    }
    
    data_452e20 = edx;
    data_453030 = ebx_3;
    int32_t eax_5 = data_45303c + arg2;
    
    if (data_453038 >= eax_5)
    {
        data_45303c = eax_5;
        return arg2;
    }
    
    int32_t eax_7 = data_453038 - data_45303c;
    data_45303c = data_453038;
    return eax_7;
}

int32_t sub_4011a0()
{
    void* edi = nullptr;
    
    for (char* i = "data\IGN1.DPS"; i < 0x41c1fa; i = &i[0x32])
    {
        void* eax_1 = sub_4044d0(i);
        *(edi + 0x43c3a0) = eax_1;
        
        if (eax_1 < 0x300)
            return 0;
        
        void* eax_2 = sub_4043a0(i);
        *(edi + 0x43c3d8) = eax_2;
        
        if (!eax_2)
            return 0;
        
        edi += 4;
    }
    
    int32_t eax_5 = *((data_41c520 << 2) + &data_41c200) << 2;
    sub_401000(*(eax_5 + 0x43c3d8), *(eax_5 + 0x43c3a0));
    data_453098 = 0x14;
    
    if (sub_403d20() != 1)
        return 0;
    
    data_43c3c8 = data_453088;
    bool cond:0 = data_453080 < 0xabe0;
    data_43c400 = data_453084;
    data_43c3d0 = data_453090;
    int32_t eax_13 = data_45308c;
    data_43c3fc = eax_13;
    
    if (!cond:0)
    {
        int32_t eax_14;
        int32_t edx_1;
        edx_1 = HIGHD(eax_13);
        eax_14 = LOWD(eax_13);
        eax_13 = (eax_14 - edx_1) >> 1;
    }
    
    int32_t ecx_1 = data_453080;
    data_43c3fc = eax_13;
    data_43c3c4 = ecx_1;
    void* eax_17 = sub_403630(0, eax_13 * 2);
    data_43c3cc = eax_17;
    
    if (!eax_17)
        return 0;
    
    data_41c030 = 1;
    return 1;
}

int32_t sub_4012a0()
{
    if (data_41c030 == 1)
    {
        for (int32_t* i = sub_403910(); i; i = sub_403910())
        {
            int32_t eax_3 = sub_401080(data_43c3cc, data_43c3fc);
            
            if (data_43c3fc > eax_3)
            {
                data_41c520 += 1;
                
                if (*((data_41c520 << 2) + &data_41c200) == 0xffffffff)
                    data_41c520 = 9;
                
                int32_t eax_7 = *((data_41c520 << 2) + &data_41c200) << 2;
                sub_401000(*(eax_7 + 0x43c3d8), *(eax_7 + 0x43c3a0));
                sub_401080((eax_3 << 1) + data_43c3cc, data_43c3fc - eax_3);
            }
            
            sub_4013c0(*i, data_43c3cc, i[2], 0);
            void* edx_3 = i[1];
            
            if (edx_3)
            {
                int32_t ecx_4 = i[3];
                
                if (ecx_4)
                    sub_4013c0(edx_3, data_43c3cc, ecx_4, i[2]);
            }
            
            sub_403bf0();
        }
    }
    
    return 1;
}

int32_t sub_4013a0()
{
    data_41c030 = 1;
    sub_403cb0();
    data_41c030 = 0;
    return 1;
}

int32_t sub_4013c0(void* arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    int32_t i_16 = arg3;
    void* var_4;
    int32_t eax_14;
    int32_t edx;
    void* edi;
    
    if (data_43c3c4 < 0xabe0)
    {
        eax_14 = arg4;
        edi = var_4;
    }
    else
    {
        int32_t eax_1;
        int32_t edx_1;
        edx_1 = HIGHD(i_16);
        eax_1 = LOWD(i_16);
        edi = ((((eax_1 ^ edx_1) - edx_1) & 1) ^ edx_1) - edx_1;
        int32_t eax_8;
        int32_t edx_2;
        edx_2 = HIGHD(i_16);
        eax_8 = LOWD(i_16);
        i_16 = (eax_8 - edx_2) >> 1;
        int32_t eax_12;
        edx = HIGHD(arg4);
        eax_12 = LOWD(arg4);
        eax_14 = (eax_12 - edx) >> 1;
    }
    
    int32_t ecx = arg2;
    int16_t* eax_15 = ecx + (eax_14 << 1);
    void* ecx_1;
    void* ebx;
    
    if (data_43c400 != 0x10)
    {
    label_4014cd:
        
        if (data_43c400 != 0x10)
        {
        label_401585:
            
            if (data_43c400 != 0x10)
            {
            label_40163b:
                
                if (data_43c400 != 0x10)
                {
                label_4016e4:
                    
                    if (data_43c400 != 8)
                    {
                    label_401798:
                        
                        if (data_43c400 != 8)
                        {
                        label_40183d:
                            
                            if (data_43c400 != 8)
                            {
                            label_4018e8:
                                
                                if (data_43c400 != 8)
                                    ecx_1 = arg1;
                                else
                                {
                                    int32_t ecx_15;
                                    
                                    if (!data_43c3c8)
                                        ecx_15 = data_43c3d0 - 1;
                                    
                                    if (data_43c3c8 || -((ecx_15 - ecx_15)) >= 0xabe0)
                                    {
                                    label_401927:
                                        
                                        if (data_43c400 != 8 || data_43c3c8)
                                            ecx_1 = arg1;
                                        else
                                        {
                                            int32_t ecx_19 = data_43c3d0 - 1;
                                            
                                            if (-((ecx_19 - ecx_19)) < 0xabe0)
                                                ecx_1 = arg1;
                                            else
                                            {
                                                ecx_1 = arg1;
                                                
                                                if (i_16 > 0)
                                                {
                                                    int32_t i;
                                                    
                                                    do
                                                    {
                                                        edx = *eax_15;
                                                        ecx_1 += 2;
                                                        eax_15 = &eax_15[1];
                                                        i = i_16;
                                                        i_16 -= 1;
                                                        *(ecx_1 - 2) = *edx[1];
                                                        edx = eax_15[-1];
                                                        *(ecx_1 - 1) = *edx[1];
                                                    } while (i != 1);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ecx_1 = arg1;
                                        
                                        if (i_16 > 0)
                                        {
                                            int32_t i_1;
                                            
                                            do
                                            {
                                                edx = *eax_15;
                                                ecx_1 += 1;
                                                eax_15 = &eax_15[1];
                                                i_1 = i_16;
                                                i_16 -= 1;
                                                *(ecx_1 - 1) = *edx[1];
                                            } while (i_1 != 1);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!data_43c3c8)
                                    ecx = -((ecx - ecx));
                                
                                if (data_43c3c8 || ecx >= 0xabe0)
                                {
                                label_40188b:
                                    
                                    if (data_43c400 != 8)
                                        goto label_401927;
                                    
                                    if (data_43c3c8 || -((ecx - ecx)) < 0xabe0)
                                        goto label_4018e8;
                                    
                                    ecx_1 = arg1;
                                    
                                    if (i_16 > 0)
                                    {
                                        int32_t i_2;
                                        
                                        do
                                        {
                                            edx = *eax_15;
                                            edx s>>= 8;
                                            ecx_1 += 2;
                                            edx ^= 0x80;
                                            eax_15 = &eax_15[1];
                                            *(ecx_1 - 2) = edx;
                                            edx = eax_15[-1];
                                            edx s>>= 8;
                                            edx ^= 0x80;
                                            i_2 = i_16;
                                            i_16 -= 1;
                                            *(ecx_1 - 1) = edx;
                                        } while (i_2 != 1);
                                    }
                                }
                                else
                                {
                                    ecx_1 = arg1;
                                    
                                    if (i_16 > 0)
                                    {
                                        int32_t i_3;
                                        
                                        do
                                        {
                                            edx = *eax_15;
                                            ecx_1 += 1;
                                            edx s>>= 8;
                                            eax_15 = &eax_15[1];
                                            edx ^= 0x80;
                                            i_3 = i_16;
                                            i_16 -= 1;
                                            *(ecx_1 - 1) = edx;
                                        } while (i_3 != 1);
                                    }
                                }
                            }
                        }
                        else if (data_43c3c8 != 1 || data_43c3d0 != 1 || data_43c3c4 >= 0xabe0)
                        {
                        label_4017e9:
                            
                            if (data_43c400 != 8)
                                goto label_40188b;
                            
                            if (data_43c3c8 != 1 || data_43c3d0 != 1 || data_43c3c4 < 0xabe0)
                                goto label_40183d;
                            
                            ecx_1 = arg1;
                            
                            if (i_16 > 0)
                            {
                                int32_t i_4;
                                
                                do
                                {
                                    edx = *(eax_15 + 1);
                                    eax_15 = &eax_15[1];
                                    *ecx_1 = edx;
                                    *(ecx_1 + 1) = edx;
                                    *(ecx_1 + 2) = edx;
                                    *(ecx_1 + 3) = edx;
                                    ecx_1 += 4;
                                    i_4 = i_16;
                                    i_16 -= 1;
                                } while (i_4 != 1);
                            }
                        }
                        else
                        {
                            ecx_1 = arg1;
                            
                            if (i_16 > 0)
                            {
                                int32_t i_5;
                                
                                do
                                {
                                    edx = *eax_15;
                                    ecx_1 += 2;
                                    eax_15 = &eax_15[1];
                                    i_5 = i_16;
                                    i_16 -= 1;
                                    *(ecx_1 - 2) = *edx[1];
                                    edx = eax_15[-1];
                                    *(ecx_1 - 1) = *edx[1];
                                } while (i_5 != 1);
                            }
                        }
                    }
                    else if (data_43c3c8 != 1 || data_43c3d0 || data_43c3c4 >= 0xabe0)
                    {
                        if (data_43c400 != 8)
                            goto label_4017e9;
                        
                        if (data_43c3c8 != 1 || data_43c3d0 || data_43c3c4 < 0xabe0)
                            goto label_401798;
                        
                        ecx_1 = arg1;
                        
                        if (i_16 > 0)
                        {
                            int32_t i_6;
                            
                            do
                            {
                                edx = *eax_15;
                                edx s>>= 8;
                                edx ^= 0x80;
                                ecx_1 += 4;
                                eax_15 = &eax_15[1];
                                i_6 = i_16;
                                i_16 -= 1;
                                *(ecx_1 - 4) = edx;
                                *(ecx_1 - 3) = edx;
                                *(ecx_1 - 2) = edx;
                                *(ecx_1 - 1) = edx;
                            } while (i_6 != 1);
                        }
                    }
                    else
                    {
                        ecx_1 = arg1;
                        
                        if (i_16 > 0)
                        {
                            int32_t i_7;
                            
                            do
                            {
                                edx = *eax_15;
                                edx s>>= 8;
                                ecx_1 += 2;
                                edx ^= 0x80;
                                eax_15 = &eax_15[1];
                                *(ecx_1 - 2) = edx;
                                edx = eax_15[-1];
                                edx s>>= 8;
                                edx ^= 0x80;
                                i_7 = i_16;
                                i_16 -= 1;
                                *(ecx_1 - 1) = edx;
                            } while (i_7 != 1);
                        }
                    }
                    
                    ebx = var_4;
                }
                else if (data_43c3c8 || data_43c3d0 != 1 || data_43c3c4 >= 0xabe0)
                {
                label_401689:
                    
                    if (data_43c400 != 0x10 || data_43c3c8 || data_43c3d0 != 1
                            || data_43c3c4 < 0xabe0)
                        goto label_4016e4;
                    
                    ecx_1 = arg1;
                    ebx = ecx_1;
                    
                    if (i_16 > 0)
                    {
                        int32_t i_8;
                        
                        do
                        {
                            edx = *eax_15;
                            *ebx = edx;
                            ebx += 4;
                            edx = *eax_15;
                            eax_15 = &eax_15[1];
                            *(ebx - 2) = edx;
                            i_8 = i_16;
                            i_16 -= 1;
                        } while (i_8 != 1);
                    }
                }
                else
                {
                    ecx_1 = arg1;
                    ebx = ecx_1;
                    
                    if (i_16 > 0)
                    {
                        int32_t i_9;
                        
                        do
                        {
                            edx = *eax_15;
                            ebx += 2;
                            *(ebx - 2) = edx;
                            eax_15 = &eax_15[1];
                            i_9 = i_16;
                            i_16 -= 1;
                        } while (i_9 != 1);
                    }
                }
            }
            else if (data_43c3c8 || data_43c3d0 || data_43c3c4 >= 0xabe0)
            {
            label_4015d8:
                
                if (data_43c400 != 0x10)
                    goto label_401689;
                
                if (data_43c3c8 || data_43c3d0 || data_43c3c4 < 0xabe0)
                    goto label_40163b;
                
                ecx_1 = arg1;
                ebx = ecx_1;
                
                if (i_16 > 0)
                {
                    int32_t i_10;
                    
                    do
                    {
                        edx = *eax_15;
                        edx ^= 0x8000;
                        ebx += 4;
                        *(ebx - 4) = edx;
                        eax_15 = &eax_15[1];
                        edx = eax_15[-1];
                        edx ^= 0x8000;
                        i_10 = i_16;
                        i_16 -= 1;
                        *(ebx - 2) = edx;
                    } while (i_10 != 1);
                }
            }
            else
            {
                ecx_1 = arg1;
                ebx = ecx_1;
                
                if (i_16 > 0)
                {
                    int32_t i_11;
                    
                    do
                    {
                        edx = *eax_15;
                        ebx += 2;
                        edx ^= 0x8000;
                        eax_15 = &eax_15[1];
                        *(ebx - 2) = edx;
                        i_11 = i_16;
                        i_16 -= 1;
                    } while (i_11 != 1);
                }
            }
        }
        else if (data_43c3c8 != 1 || data_43c3d0 != 1 || data_43c3c4 >= 0xabe0)
        {
        label_401522:
            
            if (data_43c400 != 0x10)
                goto label_4015d8;
            
            if (data_43c3c8 != 1 || data_43c3d0 != 1 || data_43c3c4 < 0xabe0)
                goto label_401585;
            
            ecx_1 = arg1;
            ebx = ecx_1;
            
            if (i_16 > 0)
            {
                int32_t i_12;
                
                do
                {
                    edx = *eax_15;
                    *ebx = edx;
                    *(ebx + 2) = edx;
                    *(ebx + 4) = edx;
                    ebx += 8;
                    *(ebx - 2) = edx;
                    eax_15 = &eax_15[1];
                    i_12 = i_16;
                    i_16 -= 1;
                } while (i_12 != 1);
            }
        }
        else
        {
            ecx_1 = arg1;
            ebx = ecx_1;
            
            if (i_16 > 0)
            {
                int32_t i_13;
                
                do
                {
                    edx = *eax_15;
                    *ebx = edx;
                    ebx += 4;
                    *(ebx - 2) = edx;
                    eax_15 = &eax_15[1];
                    i_13 = i_16;
                    i_16 -= 1;
                } while (i_13 != 1);
            }
        }
    }
    else if (data_43c3c8 != 1 || data_43c3d0 || data_43c3c4 >= 0xabe0)
    {
        if (data_43c400 != 0x10)
            goto label_401522;
        
        if (data_43c3c8 != 1 || data_43c3d0 || data_43c3c4 < 0xabe0)
            goto label_4014cd;
        
        ecx_1 = arg1;
        ebx = ecx_1;
        
        if (i_16 > 0)
        {
            int32_t i_14;
            
            do
            {
                edx = *eax_15;
                edx ^= 0x8000;
                *ebx = edx;
                *(ebx + 2) = edx;
                ebx += 8;
                *(ebx - 4) = edx;
                eax_15 = &eax_15[1];
                *(ebx - 2) = edx;
                i_14 = i_16;
                i_16 -= 1;
            } while (i_14 != 1);
        }
    }
    else
    {
        ecx_1 = arg1;
        ebx = ecx_1;
        
        if (i_16 > 0)
        {
            int32_t i_15;
            
            do
            {
                edx = *eax_15;
                edx ^= 0x8000;
                ebx += 4;
                *(ebx - 4) = edx;
                eax_15 = &eax_15[1];
                *(ebx - 2) = edx;
                i_15 = i_16;
                i_16 -= 1;
            } while (i_15 != 1);
        }
    }
    
    if (data_43c3c4 >= 0xabe0 && edi == 1)
    {
        if (data_43c400 == 0x10)
            ecx_1 = ebx;
        
        if (data_43c400 == 8)
        {
            eax_15 = *(ecx_1 - 1);
            *ecx_1 = eax_15;
        }
        else if (data_43c400 != 0x10 || data_43c3c8)
            *ecx_1 = *(ecx_1 - 4);
        else
        {
            eax_15 = *(ecx_1 - 2);
            *ecx_1 = eax_15;
        }
    }
    
    return 1;
}

int32_t sub_4019d0(int32_t* arg1, int32_t arg2, int32_t* arg3)
{
    int32_t result = 0;
    int32_t eax = sub_4035a0("Script Player");
    data_452a10 = eax;
    
    if (eax == 0xffffffff)
        return 0xffffffff;
    
    int32_t* ecx = arg3;
    
    if (*ecx)
    {
        void* i = &data_43c420;
        
        while (i < &data_43c460)
        {
            int32_t edx_1 = *ecx;
            ecx = &ecx[1];
            *i = edx_1;
            i += 4;
            
            if (!*ecx)
                break;
        }
    }
    
    int32_t* ebp = arg1;
    
    if (*arg1 != 0x7878696c)
    {
        do
        {
            int32_t eax_2 = *ebp;
            
            if (eax_2 > 0x3130696c)
            {
                if (eax_2 == 0x3230696c)
                    sub_401c20(ebp, arg2);
                else
                {
                    if (eax_2 != 0x3330696c)
                        return 0xffffffff;
                    
                    sub_401d20(ebp, arg2, &data_43c420);
                }
            }
            else if (eax_2 == 0x3130696c)
                sub_401c00(ebp);
            else
            {
                if (eax_2 != 0x3030696c)
                    return 0xffffffff;
                
                result += 1;
                sub_401be0(ebp);
            }
            
            ebp += ebp[1] + 8;
        } while (*ebp != 0x7878696c);
    }
    
    int32_t eax_8;
    int32_t edx_2;
    edx_2 = HIGHD(data_43c460);
    eax_8 = LOWD(data_43c460);
    void* eax_12 = sub_403630(data_452a10, (eax_8 + (edx_2 & 3)) >> 2);
    data_43c414 = eax_12;
    
    if (!eax_12)
        return 0xffffffff;
    
    void* eax_15 = sub_403630(data_452a10, data_43c460);
    data_43c464 = eax_15;
    
    if (!eax_15)
        return 0xffffffff;
    
    void* eax_18 = sub_403630(data_452a10, data_43c40c);
    data_43c418 = eax_18;
    
    if (!eax_18)
        return 0xffffffff;
    
    void* eax_22 = sub_403630(data_452a10, result * 0xc);
    data_43c408 = eax_22;
    
    if (!eax_22)
        return 0xffffffff;
    
    sub_401b60(arg1);
    return result;
}

void sub_401b60(int32_t* arg1)
{
    int32_t* esi = arg1;
    
    if (*esi == 0x7878696c)
        return;
    
    int32_t edi_1 = 0;
    void* var_4;
    void* edx_1 = var_4;
    void* eax = var_4;
    
    do
    {
        int32_t ecx_1 = *esi;
        
        if (ecx_1 == 0x3030696c)
        {
            *(data_43c408 + edi_1) = &esi[2];
            edi_1 += 0xc;
            *(data_43c408 + edi_1 - 8) = edx_1;
            *(data_43c408 + edi_1 - 4) = eax;
        }
        else if (ecx_1 == 0x3130696c)
        {
            int32_t eax_2;
            int32_t edx_2;
            edx_2 = HIGHD(esi[1]);
            eax_2 = LOWD(esi[1]);
            edx_1 = &esi[2];
            eax = (eax_2 + (edx_2 & 3)) >> 2;
        }
        
        esi += esi[1] + 8;
    } while (*esi != 0x7878696c);
}

int32_t sub_401be0(void* arg1)
{
    int32_t result = *(arg1 + 4) * 6;
    
    if (result > data_43c460)
        data_43c460 = result;
    
    return result;
}

int32_t sub_401c00(void* arg1)
{
    int32_t result = *(arg1 + 4) * 2;
    
    if (data_43c40c < result)
        data_43c40c = result;
    
    return result;
}

int32_t sub_401c20(void* arg1, int32_t arg2)
{
    void* esi = arg1 + 8;
    int32_t eax_1;
    int32_t edx;
    edx = HIGHD(*(arg1 + 4));
    eax_1 = LOWD(*(arg1 + 4));
    int32_t i_2 = (eax_1 + (edx & 0xf)) >> 4;
    int32_t i_1 = i_2;
    void* eax_6 = sub_403630(data_452a10, ((i_2 << 3) - i_1) << 2);
    data_43c41c = eax_6;
    
    if (!eax_6)
        return 0xffffffff;
    
    if (i_1 > 0)
    {
        int32_t ecx_1 = 0;
        int32_t i;
        
        do
        {
            int32_t eax_8;
            eax_8 = *esi;
            *(data_43c41c + ecx_1) = eax_8;
            int32_t eax_9;
            eax_9 = *(esi + 2);
            *(data_43c41c + ecx_1 + 4) = eax_9;
            int32_t eax_10;
            eax_10 = *(esi + 4);
            *(data_43c41c + ecx_1 + 8) = eax_10;
            int32_t eax_11;
            eax_11 = *(esi + 6);
            esi += 0x10;
            *(data_43c41c + ecx_1 + 0xc) = eax_11;
            int32_t eax_12;
            eax_12 = *(esi - 8);
            *(data_43c41c + ecx_1 + 0x10) = eax_12;
            int32_t eax_13;
            eax_13 = *(esi - 6);
            *(data_43c41c + ecx_1 + 0x14) = eax_13;
            int32_t eax_14 = *(esi - 4);
            
            if (eax_14 > 0x1e || eax_14 < 0)
                eax_14 = 0;
            
            *(data_43c41c + ecx_1 + 0x18) = *(arg2 + (eax_14 << 2));
            ecx_1 += 0x1c;
            i = i_1;
            i_1 -= 1;
        } while (i != 1);
    }
    
    return 0;
}

int32_t sub_401d20(void* arg1, int32_t arg2, int32_t arg3)
{
    void* esi = arg1 + 8;
    int32_t eax_1;
    int32_t edx;
    edx = HIGHD(*(arg1 + 4));
    eax_1 = LOWD(*(arg1 + 4));
    int32_t i_2 = (eax_1 + (edx & 0xf)) >> 4;
    int32_t i_1 = i_2;
    void* eax_4 = sub_403630(data_452a10, i_2 << 5);
    data_43c410 = eax_4;
    
    if (!eax_4)
        return 0xffffffff;
    
    if (i_1 > 0)
    {
        int32_t ebx_1 = 0;
        int32_t i;
        
        do
        {
            ebx_1 += 0x20;
            *(data_43c410 + ebx_1 - 0x20) = *esi << 8;
            *(data_43c410 + ebx_1 - 0x1c) = *(esi + 2) << 8;
            int32_t eax_10;
            eax_10 = *(esi + 4);
            esi += 0x10;
            *(data_43c410 + ebx_1 - 0x18) = eax_10 << 8;
            int32_t eax_12;
            eax_12 = *(esi - 0xa);
            *(data_43c410 + ebx_1 - 0x14) = eax_12 << 8;
            int32_t eax_14;
            eax_14 = *(esi - 8);
            *(data_43c410 + ebx_1 - 0x10) = eax_14 << 8;
            int32_t eax_16;
            eax_16 = *(esi - 6);
            *(data_43c410 + ebx_1 - 0xc) = eax_16 << 8;
            int32_t eax_18;
            eax_18 = *(esi - 4);
            *(data_43c410 + ebx_1 - 8) = *(arg2 + (eax_18 << 2));
            int32_t eax_20;
            eax_20 = *(esi - 2);
            i = i_1;
            i_1 -= 1;
            *(data_43c410 + ebx_1 - 4) = *(arg3 + (eax_20 << 2));
        } while (i != 1);
    }
    
    return 0;
}

int32_t sub_401e30(int32_t arg1, int32_t arg2)
{
    void* esi = data_43c418;
    int32_t* ecx_2 = arg1 * 0xc + data_43c408;
    char* eax = *ecx_2;
    void* edx = ecx_2[1];
    int32_t i_11 = ecx_2[2];
    
    if (!arg2)
    {
        int32_t i_9 = i_11 << 1;
        
        if (i_9 > 0)
        {
            int32_t i;
            
            do
            {
                edx += 2;
                esi += 4;
                i = i_9;
                i_9 -= 1;
                *(esi - 4) = *(edx - 2) << 4;
            } while (i != 1);
        }
    }
    else if (arg2 == 1 && i_11 > 0)
    {
        int32_t i_1;
        
        do
        {
            *esi = *edx << 5;
            int32_t ecx_5 = *(edx + 2);
            edx += 4;
            esi += 8;
            i_1 = i_11;
            i_11 -= 1;
            *(esi - 4) = ecx_5 * 0x26;
        } while (i_1 != 1);
    }
    
    int32_t* esi_2 = data_43c414;
    int32_t* edi_2 = data_43c464;
    
    if (*eax != 0xff)
    {
        while (true)
        {
            eax = &eax[2];
            int32_t ecx_8;
            ecx_8 = eax[0xfffffffe];
            int32_t i_10;
            i_10 = eax[0xffffffff];
            
            if (ecx_8 - 7 <= 0xc)
            {
                int32_t ebx_1;
                ebx_1 = *(ecx_8 + &*jump_table_4023b4[6][1]);
                
                switch (ebx_1)
                {
                    case 0:
                    {
                        if (arg2)
                        {
                            if (arg2 == 1 && i_10 > 0)
                            {
                                int32_t i_2;
                                
                                do
                                {
                                    *esi_2 = edi_2;
                                    esi_2 = &esi_2[1];
                                    *edi_2 = ecx_8;
                                    edi_2 = &edi_2[9];
                                    int32_t ebx_11 = *eax << 5;
                                    eax = &eax[8];
                                    edi_2[-8] = ebx_11 + data_43c410;
                                    edi_2[-7] = &edi_2[-4];
                                    int32_t ebp_8;
                                    ebp_8 = *(eax - 6);
                                    edi_2[-4] = ebp_8 << 9;
                                    edi_2[-3] = 0;
                                    edi_2[-2] = 0;
                                    int32_t ebp_10;
                                    ebp_10 = *(eax - 4);
                                    i_2 = i_10;
                                    i_10 -= 1;
                                    edi_2[-1] = ebp_10 * 0x266;
                                    int32_t ebx_15 = *(eax - 2);
                                    edi_2[-6] = *(data_43c418 + (ebx_15 << 3));
                                    edi_2[-5] = *(data_43c418 + (ebx_15 << 3) + 4);
                                } while (i_2 != 1);
                            }
                        }
                        else if (i_10 > 0)
                        {
                            int32_t i_3;
                            
                            do
                            {
                                *esi_2 = edi_2;
                                esi_2 = &esi_2[1];
                                *edi_2 = ecx_8;
                                edi_2 = &edi_2[9];
                                int32_t ebx_3 = *eax << 5;
                                eax = &eax[8];
                                edi_2[-8] = ebx_3 + data_43c410;
                                edi_2[-7] = &edi_2[-4];
                                int32_t ebp_3;
                                ebp_3 = *(eax - 6);
                                edi_2[-4] = ebp_3 << 8;
                                edi_2[-3] = 0;
                                edi_2[-2] = 0;
                                int32_t ebx_6;
                                ebx_6 = *(eax - 4);
                                i_3 = i_10;
                                i_10 -= 1;
                                edi_2[-1] = ebx_6 << 8;
                                int32_t ebx_8 = *(eax - 2);
                                edi_2[-6] = *(data_43c418 + (ebx_8 << 3));
                                edi_2[-5] = *(data_43c418 + (ebx_8 << 3) + 4);
                            } while (i_3 != 1);
                        }
                        
                    label_402394:
                        
                        if (*eax == 0xff)
                            break;
                        
                        continue;
                    }
                    case 1:
                    {
                        if (i_10 > 0)
                        {
                            int32_t i_4;
                            
                            do
                            {
                                *esi_2 = edi_2;
                                esi_2 = &esi_2[1];
                                *edi_2 = ecx_8;
                                eax = &eax[5];
                                int32_t ebx_17 = *(eax - 5);
                                edi_2[1] = *(data_43c418 + (ebx_17 << 3));
                                edi_2 = &edi_2[9];
                                edi_2[-7] = *(data_43c418 + (ebx_17 << 3) + 4);
                                int32_t ebx_19 = *(eax - 3);
                                edi_2[-5] = *(data_43c418 + (ebx_19 << 3));
                                edi_2[-4] = *(data_43c418 + (ebx_19 << 3) + 4);
                                int32_t ebx_21;
                                ebx_21 = eax[0xffffffff];
                                i_4 = i_10;
                                i_10 -= 1;
                                edi_2[-2] = ebx_21;
                                edi_2[-1] = 0;
                            } while (i_4 != 1);
                        }
                        
                        goto label_402394;
                    }
                    case 2:
                    {
                        if (i_10 > 0)
                            eax = &eax[i_10 * 7];
                        
                        goto label_402394;
                    }
                    case 3:
                    {
                        if (i_10 > 0)
                        {
                            int32_t i_5;
                            
                            do
                            {
                                *esi_2 = edi_2;
                                esi_2 = &esi_2[1];
                                *edi_2 = ecx_8;
                                eax = &eax[7];
                                int32_t ebx_22 = *(eax - 7);
                                edi_2[1] = *(data_43c418 + (ebx_22 << 3));
                                edi_2 = &edi_2[0xc];
                                edi_2[-0xa] = *(data_43c418 + (ebx_22 << 3) + 4);
                                int32_t ebx_24 = *(eax - 5);
                                edi_2[-8] = *(data_43c418 + (ebx_24 << 3));
                                edi_2[-7] = *(data_43c418 + (ebx_24 << 3) + 4);
                                int32_t ebx_26 = *(eax - 3);
                                edi_2[-5] = *(data_43c418 + (ebx_26 << 3));
                                edi_2[-4] = *(data_43c418 + (ebx_26 << 3) + 4);
                                int32_t ebx_28;
                                ebx_28 = eax[0xffffffff];
                                i_5 = i_10;
                                i_10 -= 1;
                                edi_2[-2] = ebx_28;
                                edi_2[-1] = 0;
                            } while (i_5 != 1);
                        }
                        
                        goto label_402394;
                    }
                    case 4:
                    {
                        if (i_10 > 0)
                        {
                            int32_t i_6;
                            
                            do
                            {
                                *esi_2 = edi_2;
                                esi_2 = &esi_2[1];
                                *edi_2 = ecx_8;
                                eax = &eax[8];
                                int32_t ebx_29 = *(eax - 8);
                                edi_2[1] = *(data_43c418 + (ebx_29 << 3));
                                edi_2 = &edi_2[9];
                                edi_2[-7] = *(data_43c418 + (ebx_29 << 3) + 4);
                                int32_t ebx_31 = *(eax - 6);
                                edi_2[-6] = *(data_43c418 + (ebx_31 << 3));
                                edi_2[-5] = *(data_43c418 + (ebx_31 << 3) + 4);
                                int32_t ebx_33 = *(eax - 4);
                                edi_2[-4] = *(data_43c418 + (ebx_33 << 3));
                                edi_2[-3] = *(data_43c418 + (ebx_33 << 3) + 4);
                                int32_t ebx_36 = *(eax - 2) * 0x1c;
                                i_6 = i_10;
                                i_10 -= 1;
                                edi_2[-2] = data_43c41c + ebx_36;
                                edi_2[-1] = *(data_43c41c + ebx_36 + 0x18);
                            } while (i_6 != 1);
                        }
                        
                        goto label_402394;
                    }
                    case 5:
                    {
                        if (i_10 > 0)
                        {
                            int32_t i_7;
                            
                            do
                            {
                                *esi_2 = edi_2;
                                esi_2 = &esi_2[1];
                                *edi_2 = ecx_8;
                                eax = &eax[9];
                                int32_t ebx_38 = *(eax - 9);
                                edi_2[1] = *(data_43c418 + (ebx_38 << 3));
                                edi_2 = &edi_2[0xa];
                                edi_2[-8] = *(data_43c418 + (ebx_38 << 3) + 4);
                                int32_t ebx_40 = *(eax - 7);
                                edi_2[-7] = *(data_43c418 + (ebx_40 << 3));
                                edi_2[-6] = *(data_43c418 + (ebx_40 << 3) + 4);
                                int32_t ebx_42 = *(eax - 5);
                                edi_2[-5] = *(data_43c418 + (ebx_42 << 3));
                                edi_2[-4] = *(data_43c418 + (ebx_42 << 3) + 4);
                                int32_t ebx_45 = *(eax - 3) * 0x1c;
                                edi_2[-3] = data_43c41c + ebx_45;
                                edi_2[-2] = *(data_43c41c + ebx_45 + 0x18);
                                int32_t ebx_47;
                                ebx_47 = eax[0xffffffff];
                                i_7 = i_10;
                                i_10 -= 1;
                                edi_2[-1] = *((ebx_47 << 2) + &data_43c420);
                            } while (i_7 != 1);
                        }
                        
                        goto label_402394;
                    }
                    case 6:
                    {
                        if (i_10 > 0)
                        {
                            int32_t i_8;
                            
                            do
                            {
                                *esi_2 = edi_2;
                                esi_2 = &esi_2[1];
                                *edi_2 = ecx_8;
                                eax = &eax[8];
                                int32_t ebx_48 = *(eax - 8);
                                edi_2[1] = *(data_43c418 + (ebx_48 << 3));
                                edi_2 = &edi_2[9];
                                edi_2[-7] = *(data_43c418 + (ebx_48 << 3) + 4);
                                int32_t ebx_50 = *(eax - 6);
                                edi_2[-6] = *(data_43c418 + (ebx_50 << 3));
                                edi_2[-5] = *(data_43c418 + (ebx_50 << 3) + 4);
                                int32_t ebx_52 = *(eax - 4);
                                edi_2[-4] = *(data_43c418 + (ebx_52 << 3));
                                edi_2[-3] = *(data_43c418 + (ebx_52 << 3) + 4);
                                int32_t ebx_54;
                                ebx_54 = eax[0xfffffffe];
                                edi_2[-2] = ebx_54;
                                int32_t ebx_55;
                                ebx_55 = eax[0xffffffff];
                                i_8 = i_10;
                                i_10 -= 1;
                                edi_2[-1] = *((ebx_55 << 2) + &data_43c420);
                            } while (i_8 != 1);
                        }
                        
                        goto label_402394;
                    }
                }
            }
            
            return 0;
        }
    }
    
    *esi_2 = 0;
    return data_43c414;
}

int32_t sub_4023f0()
{
    sub_402540();
    sub_4025d0();
    sub_4027d0();
    sub_4011a0();
    return 1;
}

int32_t sub_402410()
{
    int16_t x87control = sub_402e30();
    sub_4012a0();
    int32_t ecx = data_41c548;
    int32_t eax_4;
    
    if (ecx)
        eax_4 = data_41c7b0;
    else
    {
        int32_t var_8_1 = 0x3ff00000;
        data_4528bc = (&data_4529d0)[data_41c544];
        int32_t eax_3;
        eax_3 = sub_402840(0f);
        ecx = data_41c548;
        
        if (eax_3 == 1)
            data_41c544 += 1;
        
        if (data_41c544 != 3)
            eax_4 = data_41c7b0;
        else
        {
            ecx += 1;
            eax_4 = data_41c7b0;
            data_452950 = data_41c7b0 * 0.02;
        }
    }
    
    data_41c548 = ecx;
    
    if (ecx == 1 || ecx == 2)
    {
        long double x87_r7_5 = eax_4 * 0.02 - data_452950;
        data_452a08 = x87_r7_5;
        sub_402aa0(__ftol(x87control, x87_r7_5));
        ecx = data_41c548;
    }
    
    data_41c548 = ecx;
    
    if (ecx == 3 || ecx == 4)
    {
        int32_t var_8_3 = 0x40140000;
        data_4528bc = (&data_4529d0)[data_41c544];
        bool cond:0_1 = sub_402840(0f) != 1;
        int32_t eax_9 = data_41c544;
        
        if (!cond:0_1)
            eax_9 += 1;
        
        data_41c544 = eax_9;
        
        if (eax_9 == 4)
            data_41c7a8 = 2;
    }
    
    return 0;
}

int32_t sub_402520()
{
    sub_4013a0();
    sub_403820(data_452960);
    return 0;
}

void* sub_402540()
{
    int32_t eax = sub_4035a0(nullptr);
    data_452960 = eax;
    int32_t edi;
    data_4528b0 = sub_4037d0(edi, eax, 0x4b000);
    data_4529f8 = sub_403630(data_452960, 0x1dffff);
    data_45295c = sub_403630(data_452960, 0x2ffff);
    data_452a00 = sub_403630(data_452960, 0x20000);
    void* result = sub_403630(data_452960, 0x20000);
    data_4528c0 = result;
    return result;
}

int32_t sub_4025d0()
{
    int32_t esi = 0;
    data_4528b8 = sub_4043a0("data\ign.pfm");
    data_4528c8 = sub_4043a0("data\ign.psq");
    data_4528b4 = sub_4043a0("data\ign.col");
    data_452948 = sub_4043a0("data\ign0.pic");
    data_4529d0 = sub_4043a0("data\ign1.pic");
    data_4529d4 = sub_4043a0("data\ign2.pic");
    data_4529d8 = sub_4043a0("data\ign4.pic");
    data_4529dc = sub_4043a0("data\ign3.pic");
    void* eax_10 = (data_452a00 + 0xffff) & 0xffff0000;
    data_452a00 = eax_10;
    char* i = "data\IGN1.TEX";
    sub_404320("data\ign.pan", eax_10, 0x10000, 0);
    void* ebx_2 = (data_4529f8 + 0xffff) & 0xffff0000;
    
    do
    {
        void* ebp_1 = sub_4044d0(i);
        
        if (ebp_1 <= 0)
            sub_410ec0(0);
        
        if (ebp_1 > 0x100000)
            ebp_1 = 0x100000;
        
        sub_404320(i, ebx_2, ebp_1, 0);
        
        if (ebp_1 > 0)
        {
            void** eax_12 = (esi << 2) + &data_4528d0;
            uint32_t j_2 = (ebp_1 + 0xffff) >> 0x10;
            esi += j_2;
            uint32_t j;
            
            do
            {
                *eax_12 = ebx_2;
                eax_12 = &eax_12[1];
                ebx_2 += 0x10000;
                j = j_2;
                j_2 -= 1;
            } while (j != 1);
        }
        
        i = &i[0x32];
    } while (i < "data\ign.shd");
    
    int32_t i_1 = 0;
    *((esi << 2) + &data_4528d0) = 0;
    void* esi_3 = (data_45295c + 0xffff) & 0xffff0000;
    data_452970 = esi_3;
    void* esi_4 = esi_3 + 0x10000;
    sub_404320("data\ign.shd", esi_3, 0x10000, 0);
    data_452974 = esi_4;
    
    do
    {
        *esi_4 = i_1;
        esi_4 += 1;
        i_1 += 1;
    } while (i_1 < 0x100);
    
    for (int32_t i_2 = 1; i_2 < 0x100; )
    {
        char eax_13 = i_2;
        int32_t ecx;
        ecx = eax_13;
        *ecx[1] = ecx;
        i_2 += 1;
        ecx = eax_13;
        int32_t eax_15;
        eax_15 = ecx;
        int32_t edi_1;
        edi_1 = __memfill_u32(esi_4, eax_15, 0x40);
        esi_4 += 0x100;
    }
    
    void* i_3 = nullptr;
    int32_t ebx_3 = 0;
    int32_t eax_18 = (data_4528c0 + 0xffff) & 0xffff0000;
    data_452978 = 0;
    data_4528c0 = eax_18;
    int32_t j_1;
    
    do
    {
        for (j_1 = 0; j_1 < 0x100; )
        {
            char* ecx_1 = i_3 + j_1;
            j_1 += 1;
            ecx_1[data_4528c0] = ebx_3;
        }
        
        i_3 += 0x100;
        ebx_3 += 1;
    } while (i_3 < 0x10000);
    
    return j_1;
}

void* sub_4027d0()
{
    sub_4019d0(data_4528b8, &data_4528d0, &data_452970);
    sub_402f70(1);
    int32_t var_4 = data_41c554;
    sub_4030c0(data_4528b4 + 8, data_41c550);
    sub_402a80(data_41c558, data_41c55c, 0);
    void* result = sub_4044d0("data\ign.psq");
    data_452958 = result;
    return result;
}

int32_t sub_402840(double arg1)
{
    int32_t eax = data_4528bc;
    
    if (data_4528c4 != eax)
    {
        int32_t i = 0;
        data_4528c4 = eax;
        int32_t var_c_1 = 0;
        data_4529fc = 0;
        data_41c550 = 0;
        data_41c554 = 0;
        sub_4030c0(eax + 0x48, 0f);
        
        do
        {
            i += 1;
            *(data_4528b0 + i - 1) = *(data_4528bc + i + 0x34d);
        } while (i < 0x4b000);
        
        return 0;
    }
    
    if (data_41c548 <= 2)
    {
        if (data_4529fc >= 0xa && data_4529fc < 0x28)
        {
            long double x87_r7_1 = 1;
            long double temp3_1 = *data_41c550;
            x87_r7_1 - temp3_1;
            eax = (x87_r7_1 < temp3_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_1, temp3_1) ? 1 : 0) << 0xa
                | (x87_r7_1 == temp3_1 ? 1 : 0) << 0xe;
            
            if (!(*eax[1] & 0x41))
            {
                *data_41c550 = *data_41c550 + 0.10000000000000001;
                int32_t var_c_2 = data_41c554;
                sub_4030c0(data_4528bc + 0x48, data_41c550);
            }
        }
        
        long double x87_r7_5 = arg1 * 100.0;
        long double x87_r6_1 = data_4529fc;
        x87_r6_1 - x87_r7_5;
        eax = (x87_r6_1 < x87_r7_5 ? 1 : 0) << 8 | (FCMP_UO(x87_r6_1, x87_r7_5) ? 1 : 0) << 0xa
            | (x87_r6_1 == x87_r7_5 ? 1 : 0) << 0xe;
        
        if (!(*eax[1] & 0x41))
        {
            long double x87_r7_6 = 0;
            long double temp2_1 = *data_41c550;
            x87_r7_6 - temp2_1;
            eax = (x87_r7_6 < temp2_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_6, temp2_1) ? 1 : 0) << 0xa
                | (x87_r7_6 == temp2_1 ? 1 : 0) << 0xe;
            
            if (*eax[1] & 1)
            {
                long double x87_r7_8 = *data_41c550 - 0.10000000000000001;
                long double temp5_1 = 0.0;
                x87_r7_8 - temp5_1;
                *data_41c550 = x87_r7_8;
                bool c1_1 = /* bool c1_1 = unimplemented  {fstp qword [&data_41c550], st0} */;
                eax = (x87_r7_8 < temp5_1 ? 1 : 0) << 8 | (c1_1 ? 1 : 0) << 9
                    | (FCMP_UO(x87_r7_8, temp5_1) ? 1 : 0) << 0xa
                    | (x87_r7_8 == temp5_1 ? 1 : 0) << 0xe;
                
                if (*eax[1] & 1)
                {
                    data_41c550 = 0;
                    data_41c554 = 0;
                }
                
                int32_t var_c_3 = data_41c554;
                sub_4030c0(data_4528bc + 0x48, data_41c550);
            }
        }
        
        long double x87_r7_10 = arg1 * 120.0;
        long double x87_r6_2 = data_4529fc;
        x87_r6_2 - x87_r7_10;
        eax = (x87_r6_2 < x87_r7_10 ? 1 : 0) << 8 | (FCMP_UO(x87_r6_2, x87_r7_10) ? 1 : 0) << 0xa
            | (x87_r6_2 == x87_r7_10 ? 1 : 0) << 0xe;
        
        if (!(*eax[1] & 0x41))
            return 1;
    }
    
    if (data_41c548 == 3)
    {
        long double x87_r7_11 = 1;
        long double temp0_1 = *data_41c550;
        x87_r7_11 - temp0_1;
        eax = (x87_r7_11 < temp0_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_11, temp0_1) ? 1 : 0) << 0xa
            | (x87_r7_11 == temp0_1 ? 1 : 0) << 0xe;
        
        if (!(*eax[1] & 0x41))
        {
            *data_41c550 = *data_41c550 + 0.10000000000000001;
            int32_t var_c_4 = data_41c554;
            sub_4030c0(data_4528bc + 0x48, data_41c550);
        }
    }
    
    if (data_41c548 == 4)
    {
        long double x87_r7_14 = 0;
        long double temp1_1 = *data_41c550;
        x87_r7_14 - temp1_1;
        eax = (x87_r7_14 < temp1_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_14, temp1_1) ? 1 : 0) << 0xa
            | (x87_r7_14 == temp1_1 ? 1 : 0) << 0xe;
        
        if (*eax[1] & 1)
        {
            *data_41c550 = *data_41c550 - 0.10000000000000001;
            int32_t var_c_5 = data_41c554;
            sub_4030c0(data_4528bc + 0x48, data_41c550);
        }
        
        long double x87_r7_17 = 0;
        long double temp4_1 = *data_41c550;
        x87_r7_17 - temp4_1;
        eax = (x87_r7_17 < temp4_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_17, temp4_1) ? 1 : 0) << 0xa
            | (x87_r7_17 == temp4_1 ? 1 : 0) << 0xe;
        
        if (!(*eax[1] & 1))
            return 1;
    }
    
    int32_t ecx_6 = data_41c558;
    sub_404600(data_4528b0, ecx_6, 0, 0, ecx_6, data_41c55c, &data_43c7f8, 0, 0);
    sub_4046a0(&data_43c7f8);
    data_4529fc += 1;
    return 0;
}

int32_t sub_402a80(int32_t arg1, int32_t arg2, int32_t arg3)
{
    int32_t ecx;
    int32_t edx;
    sub_40a4a0(arg1, edx, ecx);
    return 1;
}

int32_t sub_402aa0(int32_t arg1)
{
    if (data_41c548 == 1)
    {
        long double x87_r7_1 = 1;
        long double temp2_1 = *data_41c550;
        x87_r7_1 - temp2_1;
        
        if (!(*((x87_r7_1 < temp2_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_1, temp2_1) ? 1 : 0) << 0xa
            | (x87_r7_1 == temp2_1 ? 1 : 0) << 0xe)[1] & 0x41) && data_41c53c > 4)
        {
            *data_41c550 = *data_41c550 + 0.10000000000000001;
            int32_t var_c_1 = data_41c554;
            sub_4030c0(data_4528b4 + 8, data_41c550);
        }
    }
    
    if (data_41c548 == 2)
    {
        *data_41c550 = *data_41c550 - 0.10000000000000001;
        int32_t var_c_2 = data_41c554;
        sub_4030c0(data_4528b4 + 8, data_41c550);
        long double x87_r7_6 = 0;
        long double temp3_1 = *data_41c550;
        x87_r7_6 - temp3_1;
        
        if (!(*((x87_r7_6 < temp3_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_6, temp3_1) ? 1 : 0) << 0xa
            | (x87_r7_6 == temp3_1 ? 1 : 0) << 0xe)[1] & 0x41))
        {
            data_41c548 += 1;
            data_41c560 = 1;
            sub_402f70(1);
        }
    }
    
    int32_t ecx_4 = arg1 - data_41c540;
    int32_t* eax_11 = data_41c538 * 0xc + data_4528c8;
    int32_t esi_1 = *eax_11 + data_41c53c;
    data_41c53c = ecx_4;
    int32_t eax_12 = eax_11[1];
    
    if (ecx_4 >= eax_12)
    {
        data_41c540 += eax_12;
        data_41c53c = 0;
        int32_t eax_13 = data_452958;
        data_41c538 += 1;
        
        if (eax_13 / 0xc == data_41c538)
            data_41c538 = 0;
    }
    
    int32_t eax_17 = sub_401e30(esi_1, data_41c560);
    int32_t ecx_5 = data_4528c0;
    data_452980 = eax_17;
    int32_t eax_18 = data_4528b0;
    data_45298c = ecx_5;
    data_452984 = eax_18;
    data_452988 = eax_18;
    int32_t eax_19 = data_41c558;
    data_452990 = 0;
    data_452994 = 0;
    data_452998 = eax_19 - 1;
    data_45299c = data_41c55c - 1;
    sub_402e10(&data_452980);
    void* eax_23 = data_452948;
    int32_t var_2c;
    int32_t var_28;
    int32_t var_24;
    int32_t var_20;
    int32_t var_1c_1;
    int32_t var_18_1;
    int32_t var_10_7;
    void* var_c_7;
    
    if (data_41c560)
    {
        sub_402f00(0, 0x39, 0x28, 0x28, 0, 0, 0x140, data_41c558, eax_23 + 0x34e);
        int32_t ecx_11 = data_41c558;
        sub_402f00(0x28, 0x39, 0x28, 0x28, ecx_11 - 0x28, 0, 0x140, ecx_11, data_452948 + 0x34e);
        sub_402f00(0, 0x61, 0x28, 0x28, 0, data_41c55c - 0x28, 0x140, data_41c558, 
            data_452948 + 0x34e);
        int32_t ecx_13 = data_41c558;
        var_c_7 = data_452948 + 0x34e;
        var_10_7 = ecx_13;
        int32_t var_14_4 = 0x140;
        var_18_1 = data_41c55c - 0x28;
        var_1c_1 = ecx_13 - 0x28;
        var_20 = 0x28;
        var_24 = 0x28;
        var_28 = 0x61;
        var_2c = 0x28;
    }
    else
    {
        sub_402f00(0, 0, 0x14, 0x14, 0, 0, 0x140, data_41c558, eax_23 + 0x34e);
        int32_t ecx_7 = data_41c558;
        sub_402f00(0x14, 0, 0x14, 0x14, ecx_7 - 0x14, 0, 0x140, ecx_7, data_452948 + 0x34e);
        sub_402f00(0, 0x14, 0x14, 0x14, 0, data_41c55c - 0x14, 0x140, data_41c558, 
            data_452948 + 0x34e);
        int32_t ecx_9 = data_41c558;
        var_c_7 = data_452948 + 0x34e;
        var_10_7 = ecx_9;
        int32_t var_14_3 = 0x140;
        var_18_1 = data_41c55c - 0x14;
        var_1c_1 = ecx_9 - 0x14;
        var_20 = 0x14;
        var_24 = 0x14;
        var_28 = 0x14;
        var_2c = 0x14;
    }
    
    sub_402f00(var_2c, var_28, var_24, var_20, var_1c_1, var_18_1, 0x140, var_10_7, var_c_7);
    int32_t ecx_14 = data_41c558;
    int32_t eax_57;
    int32_t edx_4;
    edx_4 = HIGHD(ecx_14 - 0xad);
    eax_57 = LOWD(ecx_14 - 0xad);
    sub_402f00(0x28, 0, 0xad, 0x39, (eax_57 - edx_4) >> 1, data_41c55c * 2 / 0xc8, 0x140, ecx_14, 
        data_452948 + 0x34e);
    int32_t eax_60 = data_41c558;
    sub_404600(data_4528b0, eax_60, 0, 0, eax_60, data_41c55c, &data_43c7f8, 0, 0);
    sub_4046a0(&data_43c7f8);
    return 0;
}

int32_t sub_402e10(int32_t* arg1)
{
    int32_t esi;
    int32_t var_c = esi;
    int32_t edi;
    int32_t var_10 = edi;
    int32_t var_14 = esi;
    int32_t var_18 = edi;
    return sub_40a519(arg1);
}

int32_t sub_402e30()
{
    sub_404910();
    
    if (sub_404a90(0x1c) == 1)
    {
    label_402e62:
        int32_t eax_3 = data_41c548;
        
        if (eax_3 == 1)
            eax_3 += 1;
        
        data_41c548 = eax_3;
        
        if (eax_3 == 3)
            data_41c548 = eax_3 + 1;
    }
    else
    {
        if (sub_404a90(0x39) == 1)
            goto label_402e62;
        
        if (sub_404a90(1) == 1)
            goto label_402e62;
    }
    
    if (sub_404a90(0x4a) == 1 && data_41c560)
    {
        sub_402f70(0);
        int32_t var_4_1 = 0x3ff00000;
        sub_4030c0(data_4528b4 + 8, 0f);
    }
    
    int32_t result = sub_404a90(0x4e);
    
    if (result != 1 || data_41c560 == 1)
        return result;
    
    sub_402f70(1);
    int32_t var_4_2 = 0x3ff00000;
    return sub_4030c0(data_4528b4 + 8, 0f);
}

void sub_402f00(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7, int32_t arg8, void* arg9)
{
    char* ecx_1 = arg9 + arg7 * arg2 + arg1;
    char* esi_3 = arg8 * arg6 + arg5 + data_4528b0;
    int32_t i_1 = arg4;
    
    if (i_1 <= 0)
        return;
    
    int32_t i;
    
    do
    {
        int32_t edi_1 = 0;
        
        if (arg3 > 0)
        {
            do
            {
                int32_t edx_2;
                edx_2 = ecx_1[edi_1];
                int32_t ebx_1;
                ebx_1 = esi_3[edi_1];
                edi_1 += 1;
                char* edx_4;
                edx_4 = ((edx_2 << 8) + ebx_1)[data_452a00];
                esi_3[edi_1 - 1] = edx_4;
            } while (edi_1 < arg3);
        }
        
        ecx_1 = &ecx_1[arg7];
        esi_3 = &esi_3[arg8];
        i = i_1;
        i_1 -= 1;
    } while (i != 1);
}

int32_t sub_402f70(int32_t arg1)
{
    data_41c560 = arg1;
    
    if (!arg1)
    {
        data_41c558 = 0x140;
        data_41c55c = 0xc8;
    }
    
    if (arg1 == 1)
    {
        data_41c558 = 0x280;
        data_41c55c = 0x1e0;
    }
    
    int32_t var_4 = 0;
    sub_4030c0(data_4528b4 + 8, 0f);
    
    for (int32_t i = 0; i < 0x4b000; )
    {
        i += 1;
        *(data_4528b0 + i - 1) = 0;
    }
    
    int32_t ecx_2 = data_41c558;
    sub_404600(data_4528b0, ecx_2, 0, 0, ecx_2, data_41c55c, &data_43c7f8, 0, 0);
    sub_4046a0(&data_43c7f8);
    int32_t edx = data_41c558;
    sub_404600(data_4528b0, edx, 0, 0, edx, data_41c55c, &data_43c7f8, 0, 0);
    int32_t edx_1 = data_41c55c;
    data_41c870 = data_41c558;
    data_41c878 = 8;
    data_41c87c = 1;
    data_41c874 = edx_1;
    sub_404660();
    int32_t edx_2 = data_41c558;
    sub_4046b0(data_4528b0, edx_2, edx_2, data_41c55c, 8);
    sub_402a80(data_41c558, data_41c55c, 0);
    int32_t var_4_1 = 0;
    return sub_4030c0(data_4528b4 + 8, 0f);
}

int32_t sub_4030c0(int32_t arg1, double arg2)
{
    for (char* i = nullptr; i < 0x300; )
    {
        int32_t eax_1;
        eax_1 = i[arg1];
        long double x87_r7_2 = eax_1 * arg2;
        long double temp0_1 = 255.0;
        x87_r7_2 - temp0_1;
        int32_t var_8_1;
        var_8_1 = x87_r7_2;
        bool c1_1 = /* bool c1_1 = unimplemented  {fstp qword [esp+0x10], st0} */;
        eax_1 = (x87_r7_2 < temp0_1 ? 1 : 0) << 8 | (c1_1 ? 1 : 0) << 9
            | (FCMP_UO(x87_r7_2, temp0_1) ? 1 : 0) << 0xa | (x87_r7_2 == temp0_1 ? 1 : 0) << 0xe;
        
        if (!(*eax_1[1] & 0x41))
        {
            var_8_1 = 0;
            int32_t var_4_1 = 0x406fe000;
        }
        
        long double x87_r7_3 = 0;
        long double temp2_1 = var_8_1;
        x87_r7_3 - temp2_1;
        eax_1 = (x87_r7_3 < temp2_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_3, temp2_1) ? 1 : 0) << 0xa
            | (x87_r7_3 == temp2_1 ? 1 : 0) << 0xe;
        
        if (!(*eax_1[1] & 0x41))
        {
            var_8_1 = 0;
            int32_t var_4_2 = 0;
        }
        
        i = &i[1];
        char eax_2;
        int16_t x87control;
        eax_2 = __ftol(x87control, var_8_1);
        *(i + &*(data_43c464 + 3)) = eax_2;
    }
    
    return sub_404690(0x43c468);
}

uint32_t __stdcall sub_403140(int32_t arg1)
{
    data_43c7c0 = 0x41b1c0;
    data_43c7b8 = arg1;
    data_43c790 = LoadCursorA(nullptr, 0x7f00);
    WNDCLASSA wndClass;
    wndClass.cbClsExtra = 0;
    wndClass.cbWndExtra = 0;
    wndClass.hInstance = data_43c7b8;
    wndClass.style = 8;
    wndClass.lpfnWndProc = sub_403340;
    wndClass.hIcon = LoadIconA(nullptr, 0x7f00);
    wndClass.hCursor = data_43c790;
    wndClass.hbrBackground = GetStockObject(BLACK_BRUSH);
    wndClass.lpszMenuName = 0;
    wndClass.lpszClassName = "Ignition";
    
    if (!RegisterClassA(&wndClass))
        return 0;
    
    timeBeginPeriod(1);
    sub_404b00();
    
    if (!sub_403510())
        return 0;
    
    MSG msg;
    
    while (true)
    {
        if (!data_43c7a4)
        {
            if (!GetMessageA(&msg, nullptr, 0, 0))
                return msg.wParam;
            
            TranslateMessage(&msg);
            DispatchMessageA(&msg);
        }
        else if (!PeekMessageA(&msg, nullptr, 0, 0, PM_NOREMOVE))
        {
            if (!sub_4032a0())
                sub_403540();
        }
        else
        {
            if (!GetMessageA(&msg, nullptr, 0, 0))
                break;
            
            TranslateMessage(&msg);
            DispatchMessageA(&msg);
        }
    }
    
    return msg.wParam;
}

int32_t sub_4032a0()
{
    int32_t ecx = data_41c7a8;
    
    if (ecx || data_41c828)
    {
        int32_t eax_2 = data_41c82c;
        
        if (ecx == 1 && !eax_2)
        {
            data_41c7b0 = sub_4034d0();
            sub_402410();
            return 1;
        }
        
        if (ecx == 2 && !eax_2)
        {
            data_41c7b0 = sub_4034d0();
            sub_402520();
            sub_404b30();
            timeEndPeriod(1);
            data_41c82c = 1;
            return 0;
        }
    }
    else
    {
        data_41c828 = 1;
        data_41c7b0 = sub_4034d0();
        sub_4023f0();
        data_41c7a8 = 1;
    }
    
    return 1;
}

LRESULT __stdcall sub_403340(int32_t arg1 @ esi, int32_t arg2 @ edi, HWND arg3, uint32_t arg4, WPARAM arg5, LPARAM arg6)
{
    if (arg4 > 0x1c)
    {
        if (arg4 == 0x20)
        {
            if (!data_41c79c)
            {
                SetCursor(data_43c790);
                return 1;
            }
            
            SetCursor(nullptr);
            return 1;
        }
        
        if (arg4 == 0x100)
            return 0;
        
        if (arg4 == 0x105 && arg5 == 0xd && data_43c7b0 == 1)
        {
            data_41c7a0 = 1;
            sub_404890();
            sub_404670();
            BOOL eax_10 = DestroyWindow(data_43c7bc);
            data_41c79c = -((eax_10 - eax_10));
            sub_404640();
            sub_4046f0(arg2, arg1);
            data_41c7a0 = 0;
        }
        
        return DefWindowProcA(arg3, arg4, arg5, arg6);
    }
    
    if (arg4 == 0x1c)
    {
        data_43c7a4 = arg5;
        return DefWindowProcA(arg3, arg4, arg5, arg6);
    }
    
    if (arg4 - 1 > 4)
        return DefWindowProcA(arg3, arg4, arg5, arg6);
    
    switch (arg4)
    {
        case 1:
        {
            return 0;
            break;
        }
        case 2:
        {
            if (!data_41c7a0)
            {
                sub_403560();
                PostQuitMessage(0);
            }
            
            return 0;
            break;
        }
        case 3:
        case 5:
        {
            if (data_41c79c)
            {
                int32_t yBottom = GetSystemMetrics(SM_CYSCREEN);
                SetRect(&data_43c780, 0, 0, GetSystemMetrics(SM_CXSCREEN), yBottom);
                return DefWindowProcA(arg3, arg4, arg5, arg6);
            }
            
            GetClientRect(arg3, &data_43c780);
            ClientToScreen(arg3, &data_43c780);
            ClientToScreen(arg3, &data_43c788);
            return DefWindowProcA(arg3, arg4, arg5, arg6);
            break;
        }
        case 4:
        {
            return DefWindowProcA(arg3, arg4, arg5, arg6);
            break;
        }
    }
}

int32_t sub_4034d0()
{
    uint32_t eax_1 = timeGetTime();
    uint32_t edx = data_41c830;
    
    if (!edx)
        edx = eax_1;
    
    bool cond:0 = !data_43c7a4;
    data_41c830 = edx;
    
    if (!cond:0)
        data_41c824 += eax_1 - edx;
    
    data_41c830 = eax_1;
    return data_41c824;
}

int32_t sub_403510()
{
    sub_4045e0(0);
    
    if (sub_404640() && sub_4046f0())
    {
        data_43c7b0 = 1;
        return 1;
    }
    
    return 0;
}

BOOL sub_403540()
{
    return PostMessageA(data_43c7bc, 0x10, 0, 0);
}

int32_t sub_403560()
{
    sub_404670();
    /* tailcall */
    return sub_404890();
}

int32_t sub_403570()
{
    __builtin_memset(&data_4530d0, 0, 0x400);
    sub_4035a0("DEFAULT");
    return 1;
}

int32_t sub_4035a0(char* arg1)
{
    void* eax = &data_4530d0;
    int32_t result = 0;
    
    while (*eax)
    {
        eax += 4;
        result += 1;
        
        if (eax >= &data_4534d0)
            break;
    }
    
    if (result == 0x100)
        return 0xffffffff;
    
    void* eax_2 = sub_4111a0(0x140);
    
    if (!eax_2)
        return 0xffffffff;
    
    *((result << 2) + &data_4530d0) = eax_2;
    
    if (!arg1)
        *eax_2 = 0;
    else
    {
        int32_t i = 0;
        
        if (*arg1)
        {
            while (i < 0x3f)
            {
                *(eax_2 + i) = arg1[i];
                i += 1;
                
                if (!arg1[i])
                    break;
            }
        }
        
        *(eax_2 + i) = 0;
    }
    
    __builtin_memset(eax_2 + 0x40, 0, 0x100);
    return result;
}

void* sub_403630(int32_t arg1, int32_t arg2)
{
    void* ecx = *((arg1 << 2) + &data_4530d0);
    int32_t* ecx_1 = ecx + 0x40;
    int32_t edi = 0;
    bool cond:0_1;
    
    if (!*ecx_1)
    {
    label_40369c:
        cond:0_1 = edi != 0x40;
    }
    else
    {
        while (true)
        {
            cond:0_1 = edi != 0x40;
            
            if (edi >= 0x40)
                break;
            
            int32_t* eax = *ecx_1;
            int32_t ebx_1 = 0;
            bool cond:1_1;
            
            if (!*eax)
            {
            label_40368e:
                cond:1_1 = ebx_1 != 0x40;
            }
            else
            {
                int32_t* edx_1 = eax;
                
                while (true)
                {
                    cond:1_1 = ebx_1 != 0x40;
                    
                    if (ebx_1 >= 0x40)
                        break;
                    
                    int32_t esi_1 = *edx_1;
                    int32_t ebp_1 = 0;
                    int32_t* eax_1 = esi_1 + 4;
                    bool cond:2_1;
                    
                    if (!*eax_1)
                    {
                    label_403680:
                        cond:2_1 = ebp_1 != 0x10;
                    }
                    else
                    {
                        while (true)
                        {
                            cond:2_1 = ebp_1 != 0x10;
                            
                            if (ebp_1 >= 0x10)
                                break;
                            
                            eax_1 = &eax_1[2];
                            ebp_1 += 1;
                            
                            if (!*eax_1)
                                goto label_403680;
                        }
                    }
                    
                    if (cond:2_1)
                    {
                        void* eax_3 = sub_4111a0(arg2);
                        
                        if (!eax_3)
                            return 0;
                        
                        int32_t* ecx_2 = esi_1 + (ebp_1 << 3);
                        *ecx_2 = eax_3;
                        ecx_2[1] = arg2;
                        return eax_3;
                    }
                    
                    edx_1 = &edx_1[1];
                    ebx_1 += 1;
                    
                    if (!*edx_1)
                        goto label_40368e;
                }
            }
            
            if (cond:1_1)
            {
                void* eax_5 = sub_4111a0(0x80);
                
                if (!eax_5)
                    return 0;
                
                void* ecx_3 = eax_5 + 0xc;
                eax[ebx_1] = eax_5;
                int32_t i_2 = 0xf;
                int32_t i;
                
                do
                {
                    *ecx_3 = 0;
                    ecx_3 += 8;
                    i = i_2;
                    i_2 -= 1;
                } while (i != 1);
                void* eax_8 = sub_4111a0(arg2);
                
                if (!eax_8)
                    return 0;
                
                *eax_5 = eax_8;
                *(eax_5 + 4) = arg2;
                return eax_8;
            }
            
            ecx_1 = &ecx_1[1];
            edi += 1;
            
            if (!*ecx_1)
                goto label_40369c;
        }
    }
    
    if (!cond:0_1)
        return 0;
    
    void* eax_10 = sub_4111a0(0x100);
    
    if (!eax_10)
        return 0;
    
    *(ecx + (edi << 2) + 0x40) = eax_10;
    __builtin_memset(eax_10, 0, 0x100);
    void* eax_13 = sub_4111a0(0x80);
    
    if (!eax_13)
        return 0;
    
    int32_t i_3 = 0x10;
    *eax_10 = eax_13;
    void* ecx_4 = eax_13 + 4;
    int32_t i_1;
    
    do
    {
        *ecx_4 = 0;
        ecx_4 += 8;
        i_1 = i_3;
        i_3 -= 1;
    } while (i_1 != 1);
    void* eax_15 = sub_4111a0(arg2);
    
    if (!eax_15)
        return 0;
    
    *eax_13 = eax_15;
    *(eax_13 + 4) = arg2;
    return eax_15;
}

void* sub_4037d0(int32_t arg1 @ edi, int32_t arg2, int32_t arg3)
{
    int32_t var_c = arg1;
    void* result = sub_403630(arg2, arg3);
    bool p = /* bool p = unimplemented  {test eax, eax} */;
    bool a = /* undefined */;
    bool z = !result;
    
    if (!z)
    {
        int32_t var_10;
        bool d;
        *var_10[2] = (d ? 1 : 0) << 0xa | (result < 0 ? 1 : 0) << 7 | (z ? 1 : 0) << 6
            | (a ? 1 : 0) << 4 | (p ? 1 : 0) << 2;
        int32_t var_14 = arg1;
        __builtin_memset(__builtin_memset(result, 0, arg3 >> 2 << 2), 0, arg3 & 3);
    }
    
    return result;
}

int32_t sub_403820(int32_t arg1)
{
    void* ecx = *((arg1 << 2) + &data_4530d0);
    int32_t* var_10 = ecx + 0x40;
    int32_t i_1 = 0x40;
    int32_t i;
    
    do
    {
        int32_t* eax_2 = *var_10;
        
        if (eax_2)
        {
            int32_t* ebp_1 = eax_2;
            int32_t j_1 = 0x40;
            int32_t j;
            
            do
            {
                void* ebx_1 = *ebp_1;
                
                if (ebx_1)
                {
                    int32_t* edi_1 = ebx_1 + 4;
                    int32_t k_1 = 0x10;
                    int32_t k;
                    
                    do
                    {
                        if (*edi_1)
                            sub_411250(edi_1[-1]);
                        
                        edi_1 = &edi_1[2];
                        k = k_1;
                        k_1 -= 1;
                    } while (k != 1);
                    sub_411250(ebx_1);
                }
                
                ebp_1 = &ebp_1[1];
                j = j_1;
                j_1 -= 1;
            } while (j != 1);
            sub_411250(eax_2);
        }
        
        var_10 = &var_10[1];
        i = i_1;
        i_1 -= 1;
    } while (i != 1);
    sub_411250(ecx);
    *((arg1 << 2) + &data_4530d0) = 0;
    return 1;
}

int32_t sub_4038e0()
{
    for (int32_t i = 0; i < 0x100; i += 1)
    {
        if (*((i << 2) + &data_4530d0))
            sub_403820(i);
    }
    
    return 1;
}

int32_t sub_403910()
{
    if (data_41c848 != 1)
        return 0;
    
    if (data_43c7d8 != 1)
    {
        if (!data_41c844)
        {
            int32_t* eax_30 = data_4530a0;
            (*(*eax_30 + 0x30))(eax_30, 0, 0, 1);
            data_41c844 = 1;
        }
        
        int32_t* eax_32 = data_4530a0;
        (*(*eax_32 + 0x10))(eax_32, &data_43c7e4, &data_43c7cc);
        int32_t ecx_7 = data_43c7f4;
        int32_t eax_35 = data_43c7e4 + data_43c7c4;
        data_43c7cc = eax_35;
        
        if (eax_35 >= ecx_7)
            data_43c7cc -= ecx_7;
        
        int32_t ecx_9 = data_45308c * data_43c7f0;
        int32_t eax_37 = data_43c7cc + ecx_9;
        data_43c7c8 = eax_37;
        
        if (data_43c7f4 <= eax_37)
        {
            data_43c7c8 -= data_43c7f4;
            
            if (data_43c7e0 < data_43c7c8 || data_43c7cc <= data_43c7e0)
                return 0;
        }
        else if (data_43c7e0 < data_43c7c8 && data_43c7cc <= data_43c7e0)
            return 0;
        
        int32_t* ecx_10 = data_4530a0;
        int32_t eax_46 = (*(*ecx_10 + 0x2c))(ecx_10, data_43c7e0, ecx_9, &data_43c7d0, 
            &data_43c7e8, &data_43c7d4, &data_43c7ec, 0);
        data_43c7dc = eax_46;
        
        if (eax_46 == 0x88780096)
        {
            int32_t* eax_47 = data_4530a0;
            (*(*eax_47 + 0x50))(eax_47);
            int32_t* ecx_11 = data_4530a0;
            (*(*ecx_11 + 0x30))(ecx_11, 0, 0, 1);
            return 0;
        }
        
        if (data_43c7dc)
            return 0;
        
        int32_t ecx_12 = data_43c7d4;
        data_4530b0 = data_43c7d0;
        int32_t eax_53 = data_43c7e8;
        data_4530b4 = ecx_12;
        int32_t ecx_13 = data_43c7f0;
        data_4530b8 = COMBINE(0, eax_53) / ecx_13;
        data_4530bc = COMBINE(0, data_43c7ec) / ecx_13;
        return &data_4530b0;
    }
    
    if (!data_41c844)
    {
        int32_t* eax_2 = data_4530c0;
        (*(*eax_2 + 0x30))(eax_2, 0, 0, 1);
        data_41c844 = 1;
    }
    
    int32_t* eax_4 = data_4530c0;
    (*(*eax_4 + 0x10))(eax_4, &data_43c7e4, &data_43c7cc);
    int32_t ecx = data_43c7f4;
    int32_t eax_7 = data_43c7e4 + data_43c7c4;
    data_43c7cc = eax_7;
    
    if (eax_7 >= ecx)
        data_43c7cc -= ecx;
    
    int32_t ecx_2 = data_45308c * data_43c7f0;
    int32_t eax_9 = data_43c7cc + ecx_2;
    data_43c7c8 = eax_9;
    
    if (data_43c7f4 <= eax_9)
    {
        data_43c7c8 -= data_43c7f4;
        
        if (data_43c7e0 < data_43c7c8 || data_43c7cc <= data_43c7e0)
            return 0;
    }
    else if (data_43c7e0 < data_43c7c8 && data_43c7cc <= data_43c7e0)
        return 0;
    
    int32_t* ecx_3 = data_4530c0;
    int32_t eax_18 = (*(*ecx_3 + 0x2c))(ecx_3, data_43c7e0, ecx_2, &data_43c7d0, &data_43c7e8, 
        &data_43c7d4, &data_43c7ec, 0);
    data_43c7dc = eax_18;
    
    if (eax_18 == 0x88780096)
    {
        int32_t* eax_19 = data_4530c0;
        (*(*eax_19 + 0x50))(eax_19);
        int32_t* ecx_4 = data_4530c0;
        (*(*ecx_4 + 0x30))(ecx_4, 0, 0, 1);
        return 0;
    }
    
    if (data_43c7dc)
        return 0;
    
    int32_t ecx_5 = data_43c7d4;
    data_4530b0 = data_43c7d0;
    int32_t eax_25 = data_43c7e8;
    data_4530b4 = ecx_5;
    int32_t ecx_6 = data_43c7f0;
    data_4530b8 = COMBINE(0, eax_25) / ecx_6;
    data_4530bc = COMBINE(0, data_43c7ec) / ecx_6;
    return &data_4530b0;
}

int32_t sub_403bf0()
{
    if (data_41c848 != 1)
        return 0;
    
    int32_t eax_1 = data_43c7ec;
    int32_t ecx = data_43c7d4;
    int32_t edx = data_43c7e8;
    int32_t eax_2 = data_43c7d0;
    
    if (data_43c7d8 != 1)
    {
        int32_t* ecx_3 = data_4530a0;
        (*(*ecx_3 + 0x4c))(ecx_3, eax_2, edx, ecx, eax_1);
        data_43c7e0 += data_45308c * data_43c7f0;
        
        if (data_43c7f4 <= data_43c7e0)
        {
            int32_t i;
            
            do
            {
                i = data_43c7f4;
                data_43c7e0 -= i;
            } while (i <= data_43c7e0);
        }
        
        return 0;
    }
    
    int32_t* ecx_1 = data_4530c0;
    (*(*ecx_1 + 0x4c))(ecx_1, eax_2, edx, ecx, eax_1);
    data_43c7e0 += data_45308c * data_43c7f0;
    
    if (data_43c7f4 <= data_43c7e0)
    {
        int32_t i_1;
        
        do
        {
            i_1 = data_43c7f4;
            data_43c7e0 -= i_1;
        } while (i_1 <= data_43c7e0);
    }
    
    return 0;
}

int32_t sub_403cb0()
{
    if (data_41c848 != 1)
        return 0;
    
    data_41c848 = 0;
    
    if (data_453070)
    {
        if (data_4530a0)
        {
            int32_t* eax_2 = data_4530a0;
            (*(*eax_2 + 8))(eax_2);
            data_4530a0 = 0;
        }
        
        if (data_4530c0)
        {
            int32_t* eax_4 = data_4530c0;
            (*(*eax_4 + 8))(eax_4);
            data_4530c0 = 0;
        }
        
        int32_t* eax_5 = data_453070;
        (*(*eax_5 + 8))(eax_5);
        data_453070 = 0;
    }
    
    return 0;
}

int32_t sub_403d20()
{
    int32_t var_bc;
    __builtin_memcpy(&var_bc, 
        "\x01\x00\x00\x00\x02\x00\x00\x00\x01\x00\x00\x00\x01\x00\x00\x00\x02\x00\x00\x00\x02\x00\x00\x"
    "00\x22\x56\x00\x00\x22\x56\x00\x00\x44\xac\x00\x00\x22\x56\x00\x00\x22\x56\x00\x00\x44\xac\x00"
    "00\x10\x00\x00\x00\x10\x00\x00\x00\x10\x00\x00\x00\x08\x00\x00\x00\x08\x00\x00\x00\x08\x00\x00"
    "00", 
        0x48);
    
    if (data_41c848 != 1)
    {
        data_453094 = 4;
        data_4530c0 = 0;
        data_4530a0 = 0;
        data_43c7d8 = 0;
        data_41c848 = 0;
        
        if (!DirectSoundCreate(nullptr, &data_453070, 0))
        {
            int32_t* ecx_1 = data_453070;
            int32_t var_e8;
            int32_t var_d0;
            int32_t* var_c0;
            int32_t var_a4;
            int32_t var_8c;
            int32_t var_74;
            int32_t var_6c;
            
            if (!(*(*ecx_1 + 0x18))(ecx_1, data_41c7ac, 4))
            {
                data_43c7d8 = 1;
                __builtin_memset(&var_d0, 0, 0x14);
                var_d0 = 0x14;
                int32_t* ecx_9 = data_453070;
                int32_t var_cc_3 = 1;
                int32_t var_c8_3 = 0;
                var_c0 = nullptr;
                
                if (!(*(*ecx_9 + 0xc))(ecx_9, &var_d0, &data_4530c0, 0))
                {
                    int32_t eax_59;
                    uint32_t temp0_2;
                    
                    for (int32_t i = 0; i < 6; i += 1)
                    {
                        int32_t edi_2 = (&var_bc)[i];
                        int32_t ebp_2 = (&var_8c)[i];
                        int32_t ecx_10 = (&var_a4)[i];
                        var_e8 = 0;
                        int32_t var_e4_5 = 0;
                        int32_t var_e0_5 = 0;
                        int16_t var_dc_3 = 0;
                        *var_e8[2] = edi_2;
                        var_dc_3 = ebp_2;
                        var_e8 = 1;
                        int32_t eax_51;
                        int32_t edx_10;
                        edx_10 = HIGHD(ebp_2 * edi_2);
                        eax_51 = LOWD(ebp_2 * edi_2);
                        temp0_2 = COMBINE(0, ecx_10) / 0x64;
                        uint32_t ecx_12 = (eax_51 + (edx_10 & 7)) >> 3;
                        int32_t* edx_13 = data_4530c0;
                        data_43c7f0 = ecx_12;
                        int32_t var_e0_6 = ecx_10 * ecx_12;
                        eax_59 = (*(*edx_13 + 0x38))(edx_13, &var_e8);
                        data_453080 = (&var_a4)[i];
                        data_453088 = edi_2 - 1;
                        data_453084 = ebp_2;
                        int32_t eax_61;
                        int32_t edx_15;
                        edx_15 = HIGHD(ebp_2);
                        eax_61 = LOWD(ebp_2);
                        data_453090 = ((eax_61 + (edx_15 & 7)) >> 3) - 1;
                        
                        if (!eax_59)
                            i = 6;
                    }
                    
                    if (!eax_59)
                    {
                        int32_t* ecx_14 = data_4530c0;
                        var_74 = 0x14;
                        
                        if (!(*(*ecx_14 + 0xc))(ecx_14, &var_74))
                        {
                            data_45308c = temp0_2;
                            data_43c7f4 = var_6c;
                            int32_t esi_3 = temp0_2 * data_43c7f0;
                            data_43c7c4 = data_453098 * temp0_2 * data_43c7f0;
                            int32_t eax_72 = data_43c7f4 - esi_3;
                            
                            if (eax_72 < data_43c7c4)
                                data_43c7c4 = eax_72;
                            
                            int32_t* eax_73 = data_4530c0;
                            data_43c7e0 = 0;
                            (*(*eax_73 + 0x30))(eax_73, 0, 0, 1);
                            data_41c844 = 1;
                            data_41c848 = 1;
                            return 1;
                        }
                    }
                }
            }
            else
            {
                int32_t* ecx_2 = data_453070;
                
                if (!(*(*ecx_2 + 0x18))(ecx_2, data_41c7ac, 3))
                {
                    __builtin_memset(&var_d0, 0, 0x14);
                    var_d0 = 0x14;
                    int32_t* ecx_3 = data_453070;
                    int32_t var_cc_1 = 1;
                    int32_t var_c8_1 = 0;
                    var_c0 = nullptr;
                    int32_t eax_9;
                    int32_t ecx_4;
                    eax_9 = (*(*ecx_3 + 0xc))(ecx_3, &var_d0, &data_4530c0, 0);
                    
                    if (!eax_9)
                    {
                        int32_t edi_1 = 0;
                        
                        while (true)
                        {
                            ecx_4 = (&var_bc)[edi_1];
                            int32_t esi_1 = (&var_8c)[edi_1];
                            int32_t ebp_1 = (&var_a4)[edi_1];
                            var_e8 = 0;
                            int32_t var_e4_1 = 0;
                            int32_t var_e0_1 = 0;
                            int32_t var_dc_1 = 0;
                            *var_e8[2] = ecx_4;
                            *var_dc_1[2] = esi_1;
                            var_e8 = 1;
                            int32_t eax_12;
                            int32_t edx_1;
                            edx_1 = HIGHD(esi_1 * (&var_bc)[edi_1]);
                            eax_12 = LOWD(esi_1 * (&var_bc)[edi_1]);
                            int32_t var_e4_2 = ebp_1;
                            int16_t eax_14 = (eax_12 + (edx_1 & 7)) >> 3;
                            var_dc_1 = eax_14;
                            uint32_t ebx_2 = eax_14;
                            uint32_t temp0_1 = COMBINE(0, ebp_1) / 0x64;
                            int32_t* ecx_5 = data_4530c0;
                            data_43c7f0 = ebx_2;
                            int32_t ebx_3 = ebx_2 * ebp_1;
                            int32_t var_e0_2 = ebx_3;
                            ecx_4 = (*(*ecx_5 + 0x38))(ecx_5, &var_e8);
                            data_453080 = ebp_1;
                            int32_t eax_19 = (&var_bc)[edi_1];
                            data_453084 = esi_1;
                            data_453088 = eax_19 - 1;
                            int32_t eax_22;
                            int32_t edx_5;
                            edx_5 = HIGHD(esi_1);
                            eax_22 = LOWD(esi_1);
                            data_453090 = ((eax_22 + (edx_5 & 7)) >> 3) - 1;
                            
                            if (!ecx_4)
                            {
                                ecx_4 = (&var_bc)[edi_1];
                                int32_t edx_6;
                                edx_6 = eax_14;
                                var_e8 = 0;
                                int32_t var_e4_3 = 0;
                                int32_t var_e0_3 = 0;
                                int32_t var_dc_2 = 0;
                                *var_e8[2] = ecx_4;
                                var_dc_2 = edx_6;
                                *var_dc_2[2] = esi_1;
                                int32_t* edx_7 = data_453070;
                                int32_t var_e4_4 = ebp_1;
                                int32_t var_e0_4 = ebx_3;
                                __builtin_memset(&var_d0, 0, 0x14);
                                data_43c7f4 = ebx_3;
                                int32_t var_c8_2 = ebx_3;
                                var_c0 = &var_e8;
                                var_d0 = 0x14;
                                int32_t var_cc_2 = 0xe8;
                                var_e8 = 1;
                                ecx_4 = (*(*edx_7 + 0xc))(edx_7, &var_d0, &data_4530a0, 0);
                                
                                if (ecx_4)
                                    break;
                                
                                edi_1 = 6;
                            }
                            
                            edi_1 += 1;
                            
                            if (edi_1 >= 6)
                            {
                                if (!ecx_4)
                                {
                                    int32_t* edx_8 = data_4530a0;
                                    data_45308c = temp0_1;
                                    var_74 = 0x14;
                                    
                                    if (!(*(*edx_8 + 0xc))(edx_8, &var_74))
                                    {
                                        data_43c7f4 = var_6c;
                                        data_43c7c4 = (data_453098 + 0x1b) * temp0_1 * data_43c7f0;
                                        int32_t var_60;
                                        __builtin_memset(&var_60, 0, 0x60);
                                        int32_t* edx_9 = data_453070;
                                        var_60 = 0x60;
                                        char var_5c;
                                        
                                        if (!(*(*edx_9 + 0x10))(edx_9, &var_60) && !(var_5c & 0x20))
                                            data_43c7c4 = (data_453098 + 2) * temp0_1 * data_43c7f0;
                                        
                                        int32_t eax_43 = data_43c7f4 - temp0_1 * data_43c7f0;
                                        
                                        if (eax_43 < data_43c7c4)
                                            data_43c7c4 = eax_43;
                                        
                                        int32_t* eax_44 = data_4530c0;
                                        data_43c7e0 = 0;
                                        (*(*eax_44 + 0x30))(eax_44, 0, 0, 1);
                                        int32_t* ecx_8 = data_4530a0;
                                        (*(*ecx_8 + 0x30))(ecx_8, 0, 0, 1);
                                        data_41c844 = 1;
                                        data_41c848 = 1;
                                        return 1;
                                    }
                                }
                                
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    
    return 0;
}

int32_t sub_404320(PSTR arg1, void* arg2, int32_t arg3, int32_t arg4)
{
    int32_t* eax = sub_4114b0(arg1, "rb");
    
    if (!eax)
        return 0x7d0;
    
    int32_t eax_3;
    int32_t edx;
    edx = HIGHD(arg4);
    eax_3 = LOWD(arg4);
    int32_t var_8 = eax_3;
    int32_t var_4 = edx;
    sub_411460(eax, &var_8);
    
    if (arg3 != sub_411310(arg2, 1, arg3, eax))
        return 0x7da;
    
    sub_4112a0(eax);
    return 1;
}

void* sub_4043a0(PSTR arg1)
{
    if (sub_404500(arg1) != 1)
    {
        data_41c90c = 0x7ee;
        return 0;
    }
    
    void* eax_2 = sub_4044d0(arg1);
    
    if (!eax_2)
    {
        data_41c90c = 0x7f8;
        return 0;
    }
    
    void* result = sub_403630(0, eax_2);
    
    if (!result)
    {
        data_41c90c = 0x802;
        return 0;
    }
    
    int32_t* eax_5 = sub_4114b0(arg1, "rb");
    
    if (!eax_5)
    {
        data_41c90c = 0x7d0;
        return 0;
    }
    
    int32_t var_8 = 0;
    int32_t var_4 = 0;
    sub_411460(eax_5, &var_8);
    
    if (eax_2 == sub_411310(result, 1, eax_2, eax_5))
    {
        sub_4112a0(eax_5);
        return result;
    }
    
    data_41c90c = 0x7da;
    return 0;
}

void* sub_404490(int32_t* arg1)
{
    void* eax = sub_411570(arg1);
    sub_4114d0(arg1, nullptr, FILE_END);
    void* result = sub_411570(arg1);
    sub_4114d0(arg1, eax, FILE_BEGIN);
    return result;
}

void* sub_4044d0(PSTR arg1)
{
    int32_t* eax = sub_4114b0(arg1, U"r");
    void* result = sub_404490(eax);
    sub_4112a0(eax);
    return result;
}

int32_t sub_404500(PSTR arg1)
{
    int32_t* eax = sub_4114b0(arg1, U"r");
    
    if (!eax)
        return 0x7ef;
    
    sub_4112a0(eax);
    return 1;
}

int32_t sub_404530()
{
    for (int32_t* i = &data_43c8e8; i < &data_43c918; )
    {
        *i = 0;
        i = &i[0xc];
        __builtin_memset(&i[-0xb], 0, 0x2c);
    }
    
    for (void* i_1 = &data_43c7f8; i_1 < &data_43c8e8; )
    {
        *i_1 = 0;
        i_1 += 0x30;
        __builtin_memset(i_1 - 0x2c, 0, 0x24);
        *(i_1 - 8) = 1;
        *(i_1 - 4) = 0;
    }
    
    for (void* i_2 = &data_43c918; i_2 < &data_43ccd8; )
    {
        *i_2 = 0;
        i_2 += 0x30;
        __builtin_memset(i_2 - 0x2c, 0, 0x24);
        *(i_2 - 8) = 2;
        *(i_2 - 4) = 0;
    }
    
    return 1;
}

int32_t sub_4045e0(int32_t arg1)
{
    if (arg1)
        return 2;
    
    sub_405220();
    sub_404c60();
    return 1;
}

int32_t sub_404600(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7, int32_t arg8, int32_t arg9)
{
    return data_43ccf0(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
}

int32_t sub_404640()
{
    if (data_43ccdc())
        /* jump -> data_43cd10 */
    
    return 0;
}

int32_t sub_404660()
{
    /* jump -> data_43cce0 */
}

int32_t sub_404670()
{
    if (data_43cce4())
        /* jump -> data_43cd14 */
    
    return 0;
}

int32_t sub_404690(int32_t arg1)
{
    return data_43cd08(arg1);
}

int32_t sub_4046a0(int32_t arg1)
{
    return data_43ccf8(arg1);
}

int32_t sub_4046b0(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{
    return data_43cd20(arg1, arg2, arg3, arg4, arg5);
}

int32_t sub_4046e0()
{
    data_43cd64 = 0;
    data_43d5c8 = 0;
    return 0;
}

int32_t sub_4046f0(int32_t arg1, int32_t arg2)
{
    int32_t var_24 = 0x14;
    int32_t var_20 = 0x10;
    int32_t edx = data_43c7b8;
    int32_t var_1c = 0;
    int32_t var_18 = 0;
    int32_t var_14;
    __builtin_memcpy(&var_14, 
        "\x20\x00\x00\x00\x61\x2b\x1d\x6f\xa0\xd5\xcf\x11\xbf\xc7\x44\x45\x53\x54\x00\x00", 0x14);
    
    if (DirectInputCreateA(edx, 0x300, &data_43ceb0, 0))
        return 0;
    
    int32_t* ecx = data_43ceb0;
    int32_t var_10;
    
    if ((*(*ecx + 0xc))(ecx, &var_10, &data_43d1bc, 0))
        return 0;
    
    int32_t* eax_5 = data_43d1bc;
    
    if ((*(*eax_5 + 0x2c))(eax_5, 0x40a480))
        return 0;
    
    int32_t* ecx_1 = data_43d1bc;
    
    if ((*(*ecx_1 + 0x34))(ecx_1, data_43c7bc, 6))
        return 0;
    
    int32_t* ecx_2 = data_43d1bc;
    
    if ((*(*ecx_2 + 0x18))(ecx_2, 1, &var_24))
        return 0;
    
    int32_t* eax_16 = data_43d1bc;
    int32_t eax_18 = (*(*eax_16 + 0x1c))(eax_16);
    data_43d1c0 = 1;
    
    if (eax_18 < 0)
        data_43d1c0 = 0;
    
    void* i = nullptr;
    data_43cd60 = 0x43cd68;
    __builtin_memset(0x43cd68, 0xffffffff, 0x40);
    data_43cda8 = 0;
    __builtin_memset(0x43ceb8, 0, 0x100);
    __builtin_memset(0x43cdb0, 0, 0x100);
    
    do
    {
        *(i + 0x43cfb8) = 0;
        *((i << 2) + &data_43d1c8) = 0;
        *(i + 0x43d0b8) = 0;
        i += 1;
    } while (i < 0x100);
    
    int32_t edx_1 = data_41c7b0;
    data_43ceb4 = arg1;
    data_43d1c4 = arg2;
    data_43d1b8 = edx_1;
    data_41c894 = 0;
    data_41c890 = 0;
    return 1;
}

int32_t sub_404890()
{
    if (data_43d1c0)
    {
        int32_t* eax_1 = data_43d1bc;
        (*(*eax_1 + 0x20))(eax_1);
        data_43d1c0 = 0;
    }
    
    if (data_43d1bc)
    {
        int32_t* eax_3 = data_43d1bc;
        (*(*eax_3 + 8))(eax_3);
        data_43d1bc = 0;
    }
    
    if (data_43ceb0)
    {
        int32_t* eax_5 = data_43ceb0;
        (*(*eax_5 + 8))(eax_5);
        data_43ceb0 = 0;
    }
    
    if (data_41c894)
        timeKillEvent(data_41c890);
    
    return 1;
}

int32_t sub_404910()
{
    void* eax = nullptr;
    void* i = &data_43d1c8;
    int32_t var_204 = 0x20;
    
    do
    {
        if (*(eax + 0x43cdb0) == 1)
        {
            int32_t esi_2 = data_43ceb4 + data_43d1c4;
            int32_t ecx_3 = *i - data_43d1b8 + data_41c7b0;
            *i = ecx_3;
            
            if (esi_2 <= ecx_3)
            {
                int32_t ecx_4 = ecx_3 - data_43d1c4;
                *(eax + 0x43ceb8) = 1;
                *i = ecx_4;
            }
        }
        
        i += 4;
        eax += 1;
    } while (i < &data_43d5c8);
    
    int32_t* edx = data_43d1bc;
    void var_200;
    int32_t eax_2 = (*(*edx + 0x28))(edx, 0x10, &var_200, &var_204, 0);
    int32_t result;
    
    if (eax_2 != 0x8007001e)
    {
        if (!eax_2 || eax_2 == 1)
        {
            int32_t esi_3 = 0;
            
            if (var_204 > 0)
            {
                void* edi_1 = &var_200;
                
                do
                {
                    uint32_t eax_5 = *edi_1;
                    
                    if (!(*(edi_1 + 4) & 0x80))
                    {
                        *(eax_5 + 0x43cdb0) = 0;
                        *(eax_5 + 0x43cfb8) = 0;
                        *(eax_5 + 0x43d0b8) = 0;
                        *(eax_5 + 0x43ceb8) = 0;
                        *((eax_5 << 2) + &data_43d1c8) = 0;
                        sub_404aa0(0, eax_5);
                    }
                    else
                    {
                        *(eax_5 + 0x43cdb0) = 1;
                        
                        if (!*(eax_5 + 0x43d0b8))
                        {
                            *(eax_5 + 0x43cfb8) = 1;
                            *(eax_5 + 0x43d0b8) = 1;
                        }
                        
                        if (!*((eax_5 << 2) + &data_43d1c8))
                            *(eax_5 + 0x43ceb8) = 1;
                        
                        sub_404aa0(1, eax_5);
                        uint32_t var_218_3 = *edi_1;
                        sub_404ac0(1);
                    }
                    
                    edi_1 += 0x10;
                    esi_3 += 1;
                } while (esi_3 < var_204);
            }
        }
        
        result = data_41c7b0;
        data_43d1b8 = result;
    }
    else
    {
        data_43d1c0 = 0;
        int32_t* eax_3 = data_43d1bc;
        result = (*(*eax_3 + 0x1c))(eax_3);
        
        if (result >= 0)
            data_43d1c0 = 1;
    }
    
    return result;
}

int32_t sub_404a90(char arg1)
{
    void* ecx;
    ecx = arg1;
    int32_t result;
    result = *(ecx + 0x43cdb0);
    return result;
}

void sub_404aa0(int32_t arg1, int32_t arg2)
{
    int32_t ecx = data_43cd64;
    
    if (ecx)
        ecx(arg1, arg2);
}

void* sub_404ac0(void* arg1)
{
    void* result = arg1;
    
    if (result <= 0x53)
    {
        char ecx = *(result + 0x41c898);
        **&data_43cd60 = ecx;
        result = data_43cd60;
        *(result + 0x20) = ecx;
        data_43cd60 += 1;
        
        if (data_43cd60 == 0x43cd88)
            data_43cd60 = 0x43cd68;
    }
    
    return result;
}

int32_t sub_404b00()
{
    sub_404c30();
    sub_403570();
    sub_404b40();
    sub_4046e0();
    sub_406310();
    sub_4045e0(0);
    return 1;
}

int32_t sub_404b30()
{
    sub_404b90();
    sub_4038e0();
    return 1;
}

int32_t sub_404b40()
{
    if (data_41c910 == 1)
        return 1;
    
    int32_t i = 0;
    data_41c910 = 1;
    __builtin_memset(0x43e0c0, 0, 0x320);
    
    do
    {
        int16_t ecx_1 = i + 1;
        i += 1;
        *((i << 1) + &data_43dc0e) = ecx_1;
    } while (i < 0xc8);
    
    data_43ea20 = 0;
    return 1;
}

int32_t sub_404b90()
{
    if (!data_41c910)
        return 1;
    
    void* i = nullptr;
    data_41c910 = 0;
    
    do
    {
        if (*(i + 0x43e0c0) == 1 && *(i + 0x43e700) == 0x10000)
        {
            *(i + 0x43e0c0) = 0;
            (*(i + 0x43e3e0))(*(i + 0x43d8f0));
        }
        
        i += 4;
    } while (i < 0x320);
    
    for (void* i_1 = nullptr; i_1 < 0x320; i_1 += 4)
    {
        if (*(i_1 + 0x43e0c0) == 1 && *(i_1 + 0x43e700) == 0x20000)
        {
            *(i_1 + 0x43e0c0) = 0;
            (*(i_1 + 0x43e3e0))(*(i_1 + 0x43d8f0));
        }
    }
    
    return 1;
}

int32_t sub_404c30()
{
    char* var_4 = "Compilation 0.91.0";
    sub_4117a0("\nLisa 2 Development System, %s\n");
    sub_4117a0("Copyright (c) UDS, 1995-1996\n\n");
    return 0;
}

int32_t sub_404c60()
{
    data_43cd10 = sub_404d20;
    data_43cd14 = sub_404d30;
    data_43cd18 = sub_404d40;
    data_43cd1c = sub_404d50;
    data_43cd20 = sub_404d60;
    data_43cd24 = sub_404e20;
    data_43cd28 = sub_405050;
    data_43cd2c = sub_404fb0;
    data_43cd30 = sub_407000;
    data_43cd48 = sub_406860;
    data_43cd44 = sub_4070d0;
    data_43cd4c = sub_4070f0;
    data_43cd50 = sub_407110;
    data_43cd54 = sub_406f60;
    data_43cd5c = sub_404f10;
    data_43cd58 = sub_404f70;
    sub_407fe0();
    sub_407190();
    sub_406fc0();
    return 1;
}

int32_t sub_404d20() __pure
{
    return 1;
}

int32_t sub_404d30() __pure
{
    return 1;
}

int32_t sub_404d40() __pure
{
    return 1;
}

int32_t sub_404d50() __pure
{
    return 0;
}

int32_t sub_404d60(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{
    data_41c958 = arg1;
    data_43ea2c = arg2;
    data_43eb64 = arg3;
    data_43eb70 = arg4;
    data_43eb6c = arg5;
    data_43eb58 = 0;
    data_43ea28 = 0;
    int32_t eax_2 = arg3 << 8;
    data_43eb5c = 0;
    data_43ea38 = 0;
    int32_t ecx_1 = arg4 << 8;
    data_43eb60 = arg3;
    data_43ea30 = eax_2;
    data_43eb68 = arg4;
    data_43ea3c = ecx_1;
    data_453054 = 0;
    data_45304c = 0;
    data_453050 = arg3;
    data_453048 = arg4;
    data_453060 = 0;
    data_453044 = 0;
    data_453064 = eax_2;
    data_453058 = ecx_1;
    data_453068 = arg1;
    data_45305c = arg2;
    data_453040 = arg4;
    return 1;
}

int32_t sub_404e20(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    if (arg1 < 0 || arg2 < 0 || arg3 <= arg1 || arg4 <= arg2 || arg3 > data_43eb64
            || arg4 > data_43eb70)
        return 0;
    
    int32_t eax_4 = arg1 << 8;
    int32_t ecx_1 = arg2 << 8;
    data_43eb58 = arg1;
    data_43ea28 = eax_4;
    int32_t ebx_1 = arg3 << 8;
    data_43eb5c = arg2;
    data_43ea38 = ecx_1;
    data_43eb60 = arg3;
    data_43ea30 = ebx_1;
    data_43eb68 = arg4;
    int32_t ebp_1 = arg4 << 8;
    data_43ea3c = ebp_1;
    data_453054 = arg1;
    data_45304c = arg2;
    data_453050 = arg3;
    data_453048 = arg4;
    data_453060 = eax_4;
    data_453044 = ecx_1;
    data_453064 = ebx_1;
    data_453058 = ebp_1;
    return 1;
}

int32_t sub_404f10(int32_t* arg1)
{
    *arg1 = data_43eb64;
    arg1[1] = data_43eb70;
    arg1[2] = data_43eb6c;
    int32_t edx_1 = data_43ea2c;
    arg1[8] = 3;
    arg1[7] = edx_1;
    arg1[9] = data_41c958;
    arg1[3] = data_43eb58;
    arg1[4] = data_43eb5c;
    arg1[5] = data_43eb60;
    arg1[6] = data_43eb68;
    return 1;
}

int32_t sub_404f70(int32_t* arg1)
{
    sub_404d60(arg1[9], arg1[7], *arg1, arg1[1], arg1[2]);
    sub_404e20(arg1[3], arg1[4], arg1[5], arg1[6]);
    return 1;
}

int32_t sub_404fb0(int32_t arg1, int32_t arg2, float* arg3)
{
    data_41c968 = arg2;
    data_41c970 = 0;
    data_41c96c = arg1;
    
    if (arg3)
    {
        int32_t eax;
        int16_t x87control;
        int16_t x87control_1;
        eax = __ftol(x87control, *arg3 * 65536f);
        long double x87_r7_3 = arg3[1];
        long double x87_r6_1 = eax;
        long double st0_1;
        bool c2_1;
        st0_1 = __fcos(x87_r7_3);
        int32_t eax_1;
        int16_t x87control_2;
        eax_1 = __ftol(x87control_1, st0_1 * x87_r6_1);
        data_43ea48 = eax_1;
        long double st0_2;
        bool c2_2;
        st0_2 = __fsin(x87_r7_3);
        int32_t eax_2 = __ftol(x87control_2, x87_r6_1 * st0_2);
        data_43ea4c = eax_2;
        data_43ea50 = eax_2;
        data_41c970 = &data_43ea48;
        data_43ea54 = eax_1;
    }
    
    data_43ea34 = &data_41c968;
    sub_405170();
    return 1;
}

int32_t sub_405050(int32_t* arg1)
{
    int32_t* esi = arg1;
    
    while (*esi)
    {
        void* esi_1 = &esi[1];
        int32_t var_8_1 = *esi;
        
        while (*esi_1)
        {
            int32_t* eax_2 = *esi_1;
            esi_1 += 4;
            data_43ea34 = eax_2;
            
            if (*eax_2)
            {
                do
                {
                    sub_4050c0();
                    data_43ea34 += data_41c964 << 2;
                } while (**&data_43ea34);
            }
        }
        
        esi = esi_1 + 4;
    }
    
    return 1;
}

int32_t sub_4050b0() __pure
{
    return;
}

void* sub_4050c0()
{
    int32_t* eax_9 = data_43ea34;
    data_41c97c = **eax_9;
    data_41c980 = *(*eax_9 + 4);
    data_41c984 = *eax_9[1];
    data_41c988 = *(eax_9[1] + 4);
    void* ecx_4 = data_43ea34;
    data_41c98c = **(ecx_4 + 8);
    data_41c990 = *(*(ecx_4 + 8) + 4);
    void* ecx_7 = data_43ea34;
    data_41c994 = *(*(ecx_7 + 0xc) + 8);
    data_41c998 = *(*(ecx_7 + 0xc) + 0xc);
    data_41c99c = *(*(ecx_7 + 0xc) + 0x10);
    data_41c9a0 = *(*(ecx_7 + 0xc) + 0x14);
    data_41c9a4 = *(*(ecx_7 + 0xc) + 0x18);
    data_41c9a8 = *(*(ecx_7 + 0xc) + 0x1c);
    data_41c9ac = **(ecx_7 + 0xc);
    return sub_408040(0x41c978);
}

int32_t sub_405170()
{
    int32_t* eax = data_43ea34;
    data_41c9dc = *eax;
    data_41c9e4 = eax[2];
    int32_t* esi = eax[1];
    void var_40;
    sub_4067e0(&var_40, *esi);
    data_41c9b8 = esi[1];
    data_41c9bc = esi[2];
    int32_t var_30;
    data_41c9c0 = var_30 << 8;
    data_41c9c4 = var_30 & 0xff00;
    int32_t var_3c;
    int32_t eax_8 = (var_3c << 8) + data_41c9c0;
    data_41c9d0 = var_30 & 0xffff0000;
    data_41c9c8 = eax_8;
    int32_t var_38;
    data_41c9cc = (var_38 << 8) + data_41c9c4;
    int32_t result = data_45306c;
    data_41c9d4 = result;
    sub_408dc9(result, 0x41c9d8);
    return result;
}

int32_t sub_405220()
{
    data_43ccd8 = sub_4052c0;
    data_43ccdc = sub_4052d0;
    data_43cce0 = sub_405900;
    data_43cce4 = sub_405bf0;
    data_43cce8 = sub_405ce0;
    data_43ccec = sub_405cf0;
    data_43ccf0 = sub_405d60;
    data_43ccf4 = sub_405ef0;
    data_43ccf8 = sub_405fd0;
    data_43ccfc = sub_406040;
    data_43cd00 = sub_406050;
    data_43cd04 = sub_4061a0;
    data_43cd08 = sub_4061f0;
    data_43cd0c = sub_406250;
    return 1;
}

int32_t sub_4052c0() __pure
{
    return 2;
}

int32_t sub_4052d0()
{
    sub_404530();
    int32_t nHeight = GetSystemMetrics(SM_CYSCREEN);
    HWND eax_2 = CreateWindowExA(WS_EX_APPWINDOW, "Ignition", "Ignition", 0x80080000, 0, 0, 
        GetSystemMetrics(SM_CXSCREEN), nHeight, nullptr, nullptr, data_43c7b8, 0);
    data_43c7bc = eax_2;
    data_41c7ac = eax_2;
    
    if (!eax_2)
        return 0;
    
    UpdateWindow(data_43c7bc);
    SetFocus(data_43c7bc);
    
    if (DirectDrawCreate(nullptr, &data_43f780, 0))
        return 0;
    
    int32_t var_a8_4;
    
    if (!data_41c79c)
        var_a8_4 = 8;
    else
        var_a8_4 = 0x53;
    
    int32_t* ecx_1 = data_43f780;
    
    if ((*(*ecx_1 + 0x50))(ecx_1, data_43c7bc, var_a8_4))
        return 0;
    
    if (!data_41c79c)
    {
        HDC eax_15 = GetDC(nullptr);
        data_41c9ec = GetDeviceCaps(eax_15, 0xc) * GetDeviceCaps(eax_15, 0xe);
        ReleaseDC(nullptr, eax_15);
        int32_t eax_20 = GetWindowLongA(data_43c7bc, 0xfffffff0) & 0x7fffffff;
        SetWindowLongA(data_43c7bc, 0xfffffff0, eax_20 | 0xc60000);
        RECT var_90;
        SetRect(&var_90, 0, 0, 0x280, 0x1e0);
        int32_t dwExStyle = GetWindowLongA(data_43c7bc, 0xffffffec);
        HMENU eax_22 = GetMenu(data_43c7bc);
        AdjustWindowRectEx(&var_90, GetWindowLongA(data_43c7bc, 0xfffffff0), eax_22 - eax_22 + 1, 
            dwExStyle);
        SetWindowPos(data_43c7bc, nullptr, 0, 0, var_90.right - var_90.left, 
            var_90.bottom - var_90.top, 0x16);
        SetWindowPos(data_43c7bc, 0xfffffffe, 0, 0, 0, 0, 0x13);
        int32_t pvParam;
        SystemParametersInfoA(SPI_GETWORKAREA, 0, &pvParam, 0);
        GetWindowRect(data_43c7bc, &var_90);
        int32_t pvParam_1 = pvParam;
        
        if (var_90.left < pvParam_1)
            var_90.left = pvParam_1;
        
        int32_t var_78;
        
        if (var_90.top < var_78)
            var_90.top = var_78;
        
        SetWindowPos(data_43c7bc, nullptr, var_90.left, var_90.top, 0, 0, 0x15);
    }
    else
    {
        int32_t* eax_12 = data_43f780;
        
        if ((*(*eax_12 + 0x54))(eax_12, data_41c870, data_41c874, data_41c878))
            return 0;
    }
    
    int32_t var_6c;
    __builtin_memset(&var_6c, 0, 0x6c);
    var_6c = 0x6c;
    
    if (!data_41c79c)
    {
        int32_t* ecx_12 = data_43f780;
        int32_t var_68_2 = 1;
        int32_t var_4_2 = 0x200;
        
        if ((*(*ecx_12 + 0x18))(ecx_12, &var_6c, &data_43c914, 0))
            return 0;
        
        int32_t ecx_13 = data_41c870;
        int32_t edx_6 = data_41c874;
        data_43c8e8 = 1;
        data_43c8ec = 1;
        data_43c904 = ecx_13;
        int32_t eax_54 = data_41c878;
        data_43c908 = edx_6;
        int32_t var_68_3 = 7;
        int32_t var_4_3 = 0x40;
        int32_t var_60_1 = 0x280;
        int32_t var_64_1 = 0x1e0;
        bool cond:0_1 = data_41c87c < 5;
        data_43c90c = eax_54;
        
        if (!cond:0_1)
        {
            MessageBoxA(data_43c7bc, "The maximum amount of backbuffers is exceeded", nullptr, 
                MB_OK);
            return 0;
        }
        
        if (data_41c87c > 0)
        {
            int32_t i = 0;
            void* edi_1 = &data_43c824;
            
            do
            {
                int32_t* ecx_14 = data_43f780;
                
                if ((*(*ecx_14 + 0x18))(ecx_14, &var_6c, edi_1, 0))
                    return 0;
                
                *(edi_1 - 0x2c) = 1;
                int32_t eax_59 = data_41c870;
                *(edi_1 - 0x28) = 1;
                int32_t ecx_15 = data_41c874;
                *(edi_1 - 0x10) = eax_59;
                int32_t edx_7 = data_41c878;
                *(edi_1 - 0xc) = ecx_15;
                edi_1 += 0x30;
                i += 1;
                *(edi_1 - 0x38) = edx_7;
            } while (i < data_41c87c);
        }
        
        int32_t* eax_60 = data_43f780;
        
        if ((*(*eax_60 + 0x10))(eax_60, 0, &data_41c9e8, 0))
            return 0;
        
        int32_t* ecx_16 = data_41c9e8;
        
        if ((*(*ecx_16 + 0x20))(ecx_16, 0, data_43c7bc))
            return 0;
        
        int32_t* ecx_17 = data_43c914;
        
        if ((*(*ecx_17 + 0x70))(ecx_17, data_41c9e8))
            return 1;
    }
    else
    {
        int32_t* edx_2 = data_43f780;
        int32_t var_58_1 = data_41c87c;
        int32_t var_68 = 0x21;
        int32_t var_4 = 0x218;
        
        if ((*(*edx_2 + 0x18))(edx_2, &var_6c, &data_43c914, 0))
            return 0;
        
        int32_t ecx_10 = data_41c870;
        int32_t edx_3 = data_41c874;
        data_43c8e8 = 1;
        data_43c8ec = 1;
        data_43c904 = ecx_10;
        int32_t eax_37 = data_41c878;
        data_43c908 = edx_3;
        bool cond:1_1 = data_41c87c < 5;
        data_43c90c = eax_37;
        
        if (!cond:1_1)
        {
            MessageBoxA(data_43c7bc, "The maximum amount of backbuffers is exceeded", nullptr, 
                MB_OK);
            return 0;
        }
        
        if (data_41c87c > 0)
        {
            int32_t ebp_1 = 0;
            void* esi_2 = &data_43c824;
            int32_t* var_94 = data_43c914;
            int32_t var_80 = 4;
            bool cond:2_1;
            
            do
            {
                if (esi_2 != &data_43c824)
                    var_80 = 0x10;
                
                int32_t* edx_4 = var_94;
                
                if ((*(*edx_4 + 0x30))(edx_4, &var_80, &var_94))
                {
                    MessageBoxA(data_43c7bc, "Backbuffer couldn't be obtained", nullptr, MB_OK);
                    return 0;
                }
                
                int32_t ecx_11 = data_41c870;
                *esi_2 = var_94;
                int32_t edx_5 = data_41c874;
                *(esi_2 - 0x2c) = 1;
                int32_t eax_44 = data_41c878;
                *(esi_2 - 0x28) = 1;
                esi_2 += 0x30;
                ebp_1 += 1;
                *(esi_2 - 0x40) = ecx_11;
                *(esi_2 - 0x3c) = edx_5;
                cond:2_1 = ebp_1 < data_41c87c;
                *(esi_2 - 0x38) = eax_44;
            } while (cond:2_1);
        }
    }
    
    int32_t var_68_1 = 7;
    int32_t var_4_1 = 0x40;
    
    if (data_41c79c)
    {
        if (data_43ef78)
        {
            int32_t* eax_45 = data_43ef78;
            (*(*eax_45 + 8))(eax_45);
            data_43ef78 = 0;
        }
        
        data_43eb78 = 0;
        data_43eb79 = 0;
        data_43eb7a = 0;
        
        for (void* i_1 = &data_43eb7c; i_1 < &data_43ef78; )
        {
            *i_1 = 0xff;
            i_1 += 4;
            *(i_1 - 3) = 0xff;
            *(i_1 - 2) = 0xff;
        }
        
        int32_t* eax_46 = data_43f780;
        
        if ((*(*eax_46 + 0x14))(eax_46, 0x44, &data_43eb78, &data_43ef78, 0))
            return 0;
        
        int32_t* ecx_18 = data_43c914;
        
        if ((*(*ecx_18 + 0x7c))(ecx_18, data_43ef78))
            return 0;
    }
    
    sub_406250();
    ShowWindow(data_43c7bc, SW_SHOW);
    return 1;
}

int32_t sub_405900()
{
    for (int32_t* i = &data_43c914; i < &data_43c944; i = &i[0xc])
    {
        if (data_41c79c)
        {
            int32_t* eax_1 = *i;
            
            if (eax_1)
            {
                (*(*eax_1 + 8))(eax_1);
                *i = 0;
                i[-0xb] = 0;
            }
        }
    }
    
    for (void* i_1 = &data_43c944; i_1 < &data_43cd04; i_1 += 0x30)
    {
        int32_t* eax_3 = *i_1;
        
        if (eax_3)
        {
            (*(*eax_3 + 8))(eax_3);
            *i_1 = 0;
            *(i_1 - 0x2c) = 0;
        }
    }
    
    if (data_41c79c && data_43ef78)
    {
        int32_t* eax_5 = data_43ef78;
        (*(*eax_5 + 8))(eax_5);
        data_43ef78 = 0;
    }
    
    sub_404530();
    
    if (data_41c79c)
    {
        int32_t* eax_7 = data_43f780;
        
        if ((*(*eax_7 + 0x54))(eax_7, data_41c870, data_41c874, data_41c878))
            return 0;
    }
    
    int32_t var_6c;
    __builtin_memset(&var_6c, 0, 0x6c);
    var_6c = 0x6c;
    
    if (data_41c79c)
    {
        int32_t* edx_2 = data_43f780;
        int32_t var_58_1 = data_41c87c;
        int32_t var_68_1 = 0x21;
        int32_t var_4_1 = 0x218;
        
        if ((*(*edx_2 + 0x18))(edx_2, &var_6c, &data_43c914, 0))
            return 0;
        
        int32_t ecx_2 = data_41c870;
        int32_t edx_3 = data_41c874;
        data_43c8e8 = 1;
        data_43c8ec = 1;
        data_43c904 = ecx_2;
        int32_t eax_14 = data_41c878;
        data_43c908 = edx_3;
        bool cond:0_1 = data_41c87c < 5;
        data_43c90c = eax_14;
        
        if (!cond:0_1)
        {
            MessageBoxA(data_43c7bc, "The maximum amount of backbuffers is exceeded", nullptr, 
                MB_OK);
            return 0;
        }
        
        if (data_41c87c > 0)
        {
            int32_t esi = 0;
            void* edi_1 = &data_43c824;
            int32_t* var_74 = data_43c914;
            int32_t var_70 = 4;
            bool cond:2_1;
            
            do
            {
                if (edi_1 != &data_43c824)
                    var_70 = 0x10;
                
                int32_t* edx_4 = var_74;
                
                if ((*(*edx_4 + 0x30))(edx_4, &var_70, &var_74))
                {
                    MessageBoxA(data_43c7bc, "Backbuffer couldn't be obtained", nullptr, MB_OK);
                    return 0;
                }
                
                int32_t ecx_3 = data_41c870;
                *edi_1 = var_74;
                int32_t edx_5 = data_41c874;
                *(edi_1 - 0x2c) = 1;
                int32_t eax_21 = data_41c878;
                *(edi_1 - 0x28) = 1;
                edi_1 += 0x30;
                esi += 1;
                *(edi_1 - 0x40) = ecx_3;
                *(edi_1 - 0x3c) = edx_5;
                cond:2_1 = data_41c87c > esi;
                *(edi_1 - 0x38) = eax_21;
            } while (cond:2_1);
        }
    }
    
    int32_t var_68_2 = 7;
    int32_t var_4_2 = 0x40;
    
    if (data_41c79c)
    {
        if (data_43ef78)
        {
            int32_t* eax_22 = data_43ef78;
            (*(*eax_22 + 8))(eax_22);
            data_43ef78 = 0;
        }
        
        data_43f380 = 0;
        data_43f381 = 0;
        data_43f382 = 0;
        
        for (void* i_2 = &data_43f384; i_2 < &data_43f780; )
        {
            *i_2 = 0xff;
            i_2 += 4;
            *(i_2 - 3) = 0xff;
            *(i_2 - 2) = 0xff;
        }
        
        int32_t* eax_23 = data_43f780;
        
        if ((*(*eax_23 + 0x14))(eax_23, 0x44, &data_43f380, &data_43ef78, 0))
            return 0;
        
        int32_t* ecx_4 = data_43c914;
        
        if ((*(*ecx_4 + 0x7c))(ecx_4, data_43ef78))
            return 0;
    }
    
    sub_406250();
    ShowWindow(data_43c7bc, SW_SHOW);
    return 1;
}

int32_t sub_405bf0()
{
    if (!data_41c79c && data_41c9e8)
    {
        int32_t* eax_1 = data_41c9e8;
        (*(*eax_1 + 8))(eax_1);
    }
    
    for (int32_t* i = &data_43c914; i < &data_43c944; i = &i[0xc])
    {
        if (!data_41c79c)
        {
            int32_t* eax_2 = *i;
            
            if (eax_2)
            {
                (*(*eax_2 + 8))(eax_2);
                *i = 0;
                i[-0xb] = 0;
            }
        }
    }
    
    for (void* i_1 = &data_43c824; i_1 < &data_43c914; i_1 += 0x30)
    {
        if (!data_41c79c)
        {
            int32_t* eax_4 = *i_1;
            
            if (eax_4)
            {
                (*(*eax_4 + 8))(eax_4);
                *i_1 = 0;
                *(i_1 - 0x2c) = 0;
            }
        }
    }
    
    for (void* i_2 = &data_43c944; i_2 < &data_43cd04; i_2 += 0x30)
    {
        int32_t* eax_6 = *i_2;
        
        if (eax_6)
        {
            (*(*eax_6 + 8))(eax_6);
            *i_2 = 0;
            *(i_2 - 0x2c) = 0;
        }
    }
    
    if (data_41c79c && data_43ef78)
    {
        int32_t* eax_8 = data_43ef78;
        (*(*eax_8 + 8))(eax_8);
        data_43ef78 = 0;
    }
    
    if (data_43f780)
    {
        int32_t* eax_9 = data_43f780;
        (*(*eax_9 + 8))(eax_9);
        data_43f780 = 0;
    }
    
    return 1;
}

int32_t sub_405ce0() __pure
{
    return 2;
}

int32_t sub_405cf0(void* arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5, int32_t* arg6, int32_t arg7, int32_t arg8)
{
    if (!*arg1 || !*arg6)
        return 0;
    
    int32_t var_10 = arg2;
    int32_t var_c = arg3;
    int32_t* ecx = arg6[0xb];
    int32_t var_8 = arg2 + arg4;
    int32_t var_4 = arg5 + arg3;
    int32_t eax_6 = (*(*ecx + 0x1c))(ecx, arg7, arg8, *(arg1 + 0x2c), &var_10, 0x10);
    return -((eax_6 - eax_6));
}

int32_t sub_405d60(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t* arg7, int32_t arg8, int32_t arg9)
{
    if (!*arg7)
        return 0;
    
    int32_t eax_1 = arg7[3];
    
    if (!eax_1)
    {
        if (!sub_406050(arg7, 3))
            return 0;
    }
    else if (eax_1 == 1)
    {
        if (!sub_4061a0(arg7))
            return 0;
        
        if (!sub_406050(arg7, 3))
            return 0;
    }
    
    int32_t ebx_1 = arg1 + arg4 * arg2 + arg3;
    int32_t eax_8 = arg7[6];
    int32_t ebp_1 = arg8 + eax_8 * arg9 + arg7[5];
    int32_t count = -(ebp_1) & 3;
    int32_t count_1 = (ebp_1 + arg5) & 3;
    int32_t eax_14;
    int32_t edx_3;
    edx_3 = HIGHD(arg5 - count_1 - count);
    eax_14 = LOWD(arg5 - count_1 - count);
    int32_t i_1 = arg6;
    
    if (i_1 > 0)
    {
        int32_t i;
        
        do
        {
            int32_t esi_2;
            int32_t edi_2;
            edi_2 = __builtin_memcpy(ebp_1, ebx_1, count);
            int32_t esi_3;
            int32_t edi_3;
            edi_3 = __builtin_memcpy(edi_2, esi_2, (eax_14 + (edx_3 & 3)) >> 2 << 2);
            __builtin_memcpy(edi_3, esi_3, count_1);
            ebx_1 += arg2;
            ebp_1 += eax_8;
            i = i_1;
            i_1 -= 1;
        } while (i != 1);
    }
    
    if (arg7[3] != eax_1)
    {
        if (eax_1)
        {
            if (!sub_4061a0(arg7))
                return 0;
            
            if (!sub_406050(arg7, eax_1))
                return 0;
        }
        else if (!sub_4061a0(arg7))
            return 0;
    }
    
    return 1;
}

int32_t sub_405ef0(int32_t* arg1)
{
    if (!*arg1)
        return 0;
    
    int32_t edi = arg1[3];
    
    if (!edi)
    {
        if (!sub_406050(arg1, 3))
            return 0;
    }
    else if (edi == 1)
    {
        if (!sub_4061a0(arg1))
            return 0;
        
        if (!sub_406050(arg1, 3))
            return 0;
    }
    
    int32_t edx = arg1[5];
    int32_t i = 0;
    
    if (arg1[8] > 0)
    {
        do
        {
            int32_t j = 0;
            
            if (arg1[7] > 0)
            {
                do
                {
                    int32_t ebx_3 = arg1[6] * i + j;
                    j += 1;
                    *(edx + (ebx_3 << 2)) = 0;
                } while (arg1[7] > j);
            }
            
            i += 1;
        } while (arg1[8] > i);
    }
    
    if (arg1[3] != edi)
    {
        if (edi)
        {
            if (!sub_4061a0(arg1))
                return 0;
            
            if (!sub_406050(arg1, edi))
                return 0;
        }
        else if (!sub_4061a0(arg1))
            return 0;
    }
    
    return 1;
}

int32_t sub_405fd0(int32_t* arg1)
{
    if (arg1[0xa] != 1)
        return 0;
    
    if (!*arg1)
        return 0;
    
    int32_t* eax_2 = arg1[0xb];
    char var_4;
    (*(*eax_2 + 0x38))(eax_2, &var_4);
    
    if (!(var_4 & 0x10))
        return 0;
    
    int32_t i;
    
    do
    {
        int32_t* ecx_1 = data_43c914;
        i = (*(*ecx_1 + 0x2c))(ecx_1, arg1[0xb], 0);
    } while (i == 0x8876021c);
    
    return -((i - i));
}

int32_t sub_406040() __pure
{
    return 2;
}

int32_t sub_406050(int32_t* arg1, int32_t arg2)
{
    if (!*arg1)
        return 0;
    
    if (arg1[2] == 1)
    {
        if (arg1[3] == arg2)
            return 1;
        
        int32_t* eax_3 = arg1[0xb];
        
        if ((*(*eax_3 + 0x80))(eax_3, &arg1[0xb]))
            return 0;
        
        arg1[3] = 0;
    }
    
    int32_t* eax_6 = arg1[0xb];
    
    if ((*(*eax_6 + 0x60))(eax_6) == 0x887601c2)
    {
        int32_t* eax_8 = arg1[0xb];
        (*(*eax_8 + 0x6c))(eax_8);
        arg1[1] = 1;
    }
    
    int32_t var_6c = 0x6c;
    int32_t eax_10;
    void* edx;
    void* ebx_5;
    int32_t var_48;
    
    if (arg2 != 1)
    {
        int32_t ecx_3;
        
        if (arg2 != 2)
        {
            if (arg2 != 3)
                return 0;
            
            int32_t* eax_12 = arg1[0xb];
            eax_10 = (*(*eax_12 + 0x64))(eax_12, 0, &var_6c, 0x21, 0);
            edx = &arg1[4];
            ebx_5 = &arg1[5];
            ecx_3 = var_48;
            *edx = 0;
        }
        else
        {
            int32_t* eax_11 = arg1[0xb];
            eax_10 = (*(*eax_11 + 0x64))(eax_11, 0, &var_6c, 1, 0);
            edx = &arg1[4];
            ecx_3 = var_48;
            ebx_5 = &arg1[5];
            *edx = ecx_3;
        }
        
        *ebx_5 = ecx_3;
    }
    else
    {
        int32_t* eax_9 = arg1[0xb];
        eax_10 = (*(*eax_9 + 0x64))(eax_9, 0, &var_6c, 0x11, 0);
        edx = &arg1[4];
        ebx_5 = &arg1[5];
        *edx = var_48;
        *ebx_5 = 0;
    }
    
    if (!eax_10)
    {
        arg1[3] = arg2;
        int32_t var_5c;
        arg1[6] = var_5c;
        arg1[2] = 1;
        return 1;
    }
    
    *edx = 0;
    *ebx_5 = 0;
    arg1[2] = 0;
    return 0;
}

int32_t sub_4061a0(int32_t* arg1)
{
    if (!*arg1)
        return 0;
    
    if (!arg1[2])
        return 1;
    
    int32_t* eax_2 = arg1[0xb];
    
    if ((*(*eax_2 + 0x80))(eax_2, &arg1[0xb]))
        return 0;
    
    arg1[2] = 0;
    arg1[3] = 0;
    return 1;
}

int32_t sub_4061f0(char* arg1)
{
    if (!data_43ef78)
        return 0;
    
    char* ecx = arg1;
    
    for (void* i = &data_43ef80; i < &data_43f380; )
    {
        char edx = *ecx;
        ecx = &ecx[3];
        *i = edx;
        i += 4;
        *(i - 3) = ecx[0xfffffffe];
        *(i - 2) = ecx[0xffffffff];
    }
    
    int32_t* eax_1 = data_43ef78;
    int32_t eax_3 = (*(*eax_1 + 0x18))(eax_1, 0, 0, 0x100, &data_43ef80);
    return -((eax_3 - eax_3));
}

int32_t sub_406250()
{
    int32_t edi = 0;
    
    if (data_43c8e8 == 1)
    {
        int32_t* eax_1 = data_43c914;
        
        if ((*(*eax_1 + 0x60))(eax_1) == 0x887601c2)
        {
            int32_t* eax_3 = data_43c914;
            edi = 1;
            (*(*eax_3 + 0x6c))(eax_3);
            data_43c8ec = 1;
        }
    }
    
    for (void* i = &data_43c824; i < &data_43c914; i += 0x30)
    {
        if (*(i - 0x2c) == 1)
        {
            int32_t* eax_4 = *i;
            
            if ((*(*eax_4 + 0x60))(eax_4) == 0x887601c2)
            {
                int32_t* eax_7 = *i;
                edi += 1;
                (*(*eax_7 + 0x6c))(eax_7);
                *(i - 0x28) = 1;
            }
        }
    }
    
    for (void* i_1 = &data_43c944; i_1 < &data_43cd04; i_1 += 0x30)
    {
        if (*(i_1 - 0x2c) == 1)
        {
            int32_t* eax_9 = *i_1;
            
            if ((*(*eax_9 + 0x60))(eax_9) == 0x887601c2)
            {
                int32_t* eax_12 = *i_1;
                edi += 1;
                (*(*eax_12 + 0x6c))(eax_12);
                *(i_1 - 0x28) = 1;
            }
        }
    }
    
    if (edi > 0)
        return 0;
    
    return 1;
}

int32_t sub_406310()
{
    data_4449e8 = 0;
    data_4449ec = 0;
    data_4449f0 = 0;
    data_4449f4 = 0;
    data_444b80 = data_4449e8;
    data_444b84 = data_4449ec;
    int32_t ecx = data_4449f4;
    data_444b88 = data_4449f0;
    data_444b8c = ecx;
    data_4450a0 = data_4449e8;
    data_4450a4 = data_4449ec;
    int32_t ecx_1 = data_4449f4;
    data_4450a8 = data_4449f0;
    data_4450ac = ecx_1;
    data_445048 = data_4449e8;
    data_44504c = data_4449ec;
    int32_t ecx_2 = data_4449f4;
    data_445050 = data_4449f0;
    data_445054 = ecx_2;
    data_444b18 = data_4449e8;
    data_444b1c = data_4449ec;
    int32_t ecx_3 = data_4449f4;
    data_444b20 = data_4449f0;
    data_444b24 = ecx_3;
    data_444970 = data_4449e8;
    data_444974 = data_4449ec;
    int32_t ecx_4 = data_4449f4;
    data_444978 = data_4449f0;
    data_44497c = ecx_4;
    data_445088 = data_4449e8;
    data_44508c = data_4449ec;
    int32_t ecx_5 = data_4449f4;
    data_445090 = data_4449f0;
    int32_t eax_18 = data_4449e8;
    data_445094 = ecx_5;
    data_445068 = eax_18;
    data_44506c = data_4449ec;
    int32_t ecx_6 = data_4449f4;
    data_445070 = data_4449f0;
    data_445074 = ecx_6;
    sub_406460();
    sub_4065f0();
    ___acrt_initialize_timeset();
    sub_4069d0();
    sub_406630();
    tytiNil::();
    sub_406590();
    /* tailcall */
    return sub_406640();
}

int32_t sub_406460()
{
    return sub_406f70(0x444908);
}

void* sub_406470(int32_t arg1)
{
    if (arg1)
        return sub_4111a0(arg1);
    
    return 0;
}

int32_t sub_406490(void* arg1)
{
    int32_t* edi = *(arg1 + 0xc);
    int32_t i = 0;
    *(arg1 + 0xc) = sub_4111a0((*(arg1 + 8) << 2) + 0xfa0);
    
    if (*(arg1 + 8) > 0)
    {
        int32_t* ecx_1 = edi;
        int32_t eax_2 = 0;
        
        do
        {
            int32_t ebx_1 = *ecx_1;
            ecx_1 = &ecx_1[1];
            eax_2 += 4;
            i += 1;
            *(*(arg1 + 0xc) + eax_2 - 4) = ebx_1;
        } while (*(arg1 + 8) > i);
    }
    
    int32_t i_1 = *(arg1 + 8);
    
    if (i_1 + 0x3e8 > i_1)
    {
        int32_t ecx_2 = i_1 << 2;
        
        do
        {
            ecx_2 += 4;
            i_1 += 1;
            *(*(arg1 + 0xc) + ecx_2 - 4) = 0;
        } while (*(arg1 + 8) + 0x3e8 > i_1);
    }
    
    *(arg1 + 8) += 0x3e8;
    return sub_411250(edi);
}

int32_t tytiNil::()
{
    __builtin_memset(&data_4448b0, 0, 0x2c);
    data_445048 = 1;
    return 0;
}

int32_t sub_406570(int32_t arg1)
{
    if (!arg1)
        return arg1;
    
    return sub_411250(arg1);
}

int32_t sub_406590()
{
    data_444b18 = 8;
}

void sub_4065a0(char* arg1, int32_t* arg2)
{
    int32_t esi = 0;
    
    if (!arg1)
    {
        *arg2 = 0;
        return;
    }
    
    if (*arg1)
    {
        do
            esi += 1;
         while (arg1[esi]);
    }
    
    void* eax = sub_406470(esi + 1);
    *arg2 = eax;
    int32_t i = 0;
    
    if (esi + 1 <= 0)
        return;
    
    do
    {
        int32_t* ecx_1;
        ecx_1 = arg1[i];
        i += 1;
        *(i + eax - 1) = ecx_1;
    } while (i < esi + 1);
}

int32_t sub_4065f0()
{
    int32_t result;
    
    for (int32_t i = 0; i < 0x1000; )
    {
        int32_t i_1 = i;
        i += 1;
        long double st0_1;
        bool c2_1;
        st0_1 = __fsin(i_1 * 0.000244140625 * 6.2831919999999997);
        int16_t x87control;
        result = __ftol(x87control, st0_1 * 2147418112.0);
        *((i << 2) + &data_440844) = result;
    }
    
    return result;
}

int32_t sub_406630()
{
    data_444b80 = 1;
}

void* sub_406640()
{
    sub_406f70(0x445058);
    data_43f7c8 = "default";
    data_43f7d4 = 0x3f800000;
    data_43f7e4 = 0;
    data_43f7cc = 0;
    data_43f7e8 = 0;
    data_43f7e0 = 0x3ecccccd;
    data_43f7dc = 0x3f000000;
    data_43f7d8 = 0x3dcccccd;
    void* result = sub_406470(0x10);
    data_43f7e9 = result;
    *result = 0xc;
    *(result + 4) = 0;
    *(result + 8) = 0xfff;
    *(result + 0xc) = 0;
    data_4450a0 = 1;
    return result;
}

int32_t ___acrt_initialize_timeset()
{
    data_444870 = "default";
    __builtin_memset(&data_444874, 0, 0x18);
    data_445068 = 1;
    return 0;
}

void* sub_406710(int32_t* arg1, int32_t arg2)
{
    int32_t ecx = arg2;
    int32_t* ebp;
    
    if (ecx < 0 || ecx >= data_445070)
    {
        ecx = 0;
        int32_t* var_4;
        ebp = var_4;
    }
    else
    {
        if (!*(data_445074 + (ecx << 2)))
            ecx = 0;
        
        ebp = *(data_445074 + (ecx << 2));
    }
    
    if (!ecx)
        ebp = &data_444870;
    
    __builtin_memcpy(arg1, ebp, 0x40);
    sub_4065a0(*ebp, arg1);
    arg1[6] = *arg1;
    void* eax_7 = sub_406470(arg1[2] * arg1[1]);
    int32_t ecx_1 = arg1[1];
    arg1[4] = eax_7;
    arg1[5] = ecx_1;
    void* result = sub_4067a0(ebp, arg1);
    arg1[7] = arg1[4];
    return result;
}

void* sub_4067a0(void* arg1, void* arg2)
{
    int32_t ebx;
    int32_t var_4 = ebx;
    int32_t i = 0;
    int32_t edi = *(arg1 + 0x10);
    int32_t ebp = *(arg2 + 0x10);
    
    if (*(arg1 + 8) > 0)
    {
        do
        {
            char* j = nullptr;
            
            if (*(arg1 + 4) > 0)
            {
                do
                {
                    ebx = j[edi];
                    j[ebp] = ebx;
                    j = &j[1];
                } while (*(arg1 + 4) > j);
            }
            
            edi += *(arg1 + 0x14);
            ebp += *(arg2 + 0x14);
            i += 1;
        } while (*(arg1 + 8) > i);
    }
    
    return arg1;
}

int32_t sub_4067e0(void* arg1, int32_t arg2)
{
    if (!arg2)
    {
        __builtin_memcpy(arg1, &data_444870, 0x40);
        return arg2;
    }
    
    if (arg2 < 0 || arg2 >= data_445070)
    {
        __builtin_memcpy(arg1, &data_444870, 0x40);
        return arg2;
    }
    
    int32_t esi = *(data_445074 + (arg2 << 2));
    
    if (!esi)
    {
        __builtin_memcpy(arg1, &data_444870, 0x40);
        return arg2;
    }
    
    __builtin_memcpy(arg1, esi, 0x40);
    *(arg1 + 0x18) = 0;
    *(arg1 + 0x1c) = 0;
    return 0;
}

int32_t sub_406860(int32_t arg1, int32_t arg2)
{
    int32_t result = arg2;
    
    while (true)
    {
        if (!arg1)
        {
            if (result > 0 && data_445070 > result)
            {
                int32_t* eax_3 = *(data_445074 + (result << 2));
                
                if (eax_3)
                {
                    sub_406570(*eax_3);
                    sub_406d90(*(data_445074 + (result << 2)));
                }
                
                if (data_445068 > result)
                    data_445068 = result;
                
                sub_406570(*(data_445074 + (result << 2)));
                *(data_445074 + (result << 2)) = 0;
            }
            
            return 0;
        }
        
        if (result)
        {
            if (result < 0 || data_445070 <= result)
                return 0;
            
            break;
        }
        
        if (data_445070 > data_445068)
        {
            result = data_445068;
            int32_t eax_8 = result + 1;
            
            if (eax_8 < data_445070)
            {
                int32_t* ecx_5 = (eax_8 << 2) + data_445074;
                
                while (*ecx_5)
                {
                    ecx_5 = &ecx_5[1];
                    eax_8 += 1;
                    
                    if (eax_8 >= data_445070)
                        break;
                }
            }
            
            data_445068 = eax_8;
            break;
        }
        
        sub_406490(&data_445068);
    }
    
    int32_t* edi = data_445074 + (result << 2);
    int32_t* eax_10 = *edi;
    
    if (eax_10)
    {
        sub_406570(*eax_10);
        sub_406d90(*(data_445074 + (result << 2)));
    }
    else
        *edi = sub_406470(0x40);
    
    int32_t* ebp = *(data_445074 + (result << 2));
    __builtin_memcpy(ebp, arg1, 0x40);
    sub_4065a0(*ebp, ebp);
    sub_406a10(ebp);
    return result;
}

int32_t sub_4069b0(void* arg1)
{
    sub_406570(*(arg1 + 0x18));
    return sub_406570(*(arg1 + 0x1c));
}

int32_t sub_4069d0()
{
    __builtin_memset(&data_4448e8, 0, 0x18);
    __builtin_memset(&data_444c40, 0, 0x404);
    data_444b28 = 0;
    return 0;
}

void sub_406a10(void* arg1)
{
    if (*(arg1 + 8) <= 0)
        return;
    
    while (true)
    {
        int32_t edx_1 = *(arg1 + 8);
        
        if (edx_1 > 0x100)
            break;
        
        void** eax = *(arg1 + 4);
        
        if (eax <= 0)
            break;
        
        if (eax > 0x100)
            break;
        
        void** esi_1 = *((edx_1 << 2) + &data_444c40);
        int32_t var_60_1;
        void* var_5c_1;
        
        if (esi_1)
        {
            while (true)
            {
                void* ebp_1 = esi_1[1];
                int32_t* eax_3 = sub_406d50(*(arg1 + 4), *(ebp_1 + 0xc));
                
                if (eax_3)
                {
                    int32_t var_5c_3 = 0x18;
                    void* var_48_1;
                    
                    if (eax_3 != 0xffffffff)
                    {
                        void* eax_10 = sub_406470(var_5c_3);
                        var_48_1 = eax_10;
                        __builtin_memcpy(eax_10, &data_4448e8, 0x18);
                        *var_48_1 = eax_3[1];
                        *(var_48_1 + 4) = *(arg1 + 4) + eax_3[1];
                        *(var_48_1 + 8) = *(ebp_1 + 8) + *var_48_1;
                        sub_406b70(eax_3, var_48_1, eax_3[5]);
                    }
                    else
                    {
                        void* eax_4 = sub_406470(var_5c_3);
                        var_48_1 = eax_4;
                        __builtin_memcpy(eax_4, &data_4448e8, 0x18);
                        *(var_48_1 + 4) = *(arg1 + 4);
                        *(var_48_1 + 8) = *(ebp_1 + 8) + *var_48_1;
                        void* eax_8 = *(ebp_1 + 0xc);
                        *(var_48_1 + 0x14) = eax_8;
                        
                        if (eax_8)
                            *(eax_8 + 0x10) = var_48_1;
                        
                        *(ebp_1 + 0xc) = var_48_1;
                    }
                    
                    void var_40;
                    __builtin_memcpy(&var_40, arg1, 0x40);
                    void* var_60_4 = &var_40;
                    *(arg1 + 0x10) = *(var_48_1 + 8);
                    *(arg1 + 0x14) = 0x100;
                    sub_4067a0(var_60_4, arg1);
                    break;
                }
                
                esi_1 = *esi_1;
                
                if (!esi_1)
                {
                    var_5c_1 = data_444b28;
                    var_60_1 = *(arg1 + 8);
                    goto label_406ad6;
                }
            }
            
            break;
        }
        
        var_5c_1 = data_444b28;
        var_60_1 = edx_1;
    label_406ad6:
        sub_406ba0(var_60_1, var_5c_1);
        
        if (*(arg1 + 8) <= 0)
            return;
    }
}

void* sub_406b70(void* arg1, void* arg2, void* arg3)
{
    *(arg2 + 0x14) = arg3;
    *(arg2 + 0x10) = arg1;
    
    if (arg1)
        *(arg1 + 0x14) = arg2;
    
    if (arg3)
        *(arg3 + 0x10) = arg2;
    
    return arg1;
}

void** sub_406ba0(int32_t arg1, void* arg2)
{
    void* i = arg2;
    
    while (true)
    {
        if (i)
        {
            do
            {
                int32_t* eax_2 = sub_406d50(arg1, *(i + 0xc));
                
                if (eax_2)
                {
                    void* var_4;
                    
                    if (eax_2 != 0xffffffff)
                    {
                        void* eax_8 = sub_406470(0x18);
                        var_4 = eax_8;
                        __builtin_memcpy(eax_8, &data_4448e8, 0x18);
                        *var_4 = eax_2[1];
                        *(var_4 + 4) = eax_2[1] + arg1;
                        *(var_4 + 8) = *(i + 8) + (*var_4 << 8);
                        sub_406b70(eax_2, var_4, eax_2[5]);
                    }
                    else
                    {
                        void* eax_3 = sub_406470(0x18);
                        var_4 = eax_3;
                        __builtin_memcpy(eax_3, &data_4448e8, 0x18);
                        *(var_4 + 4) = arg1;
                        *(var_4 + 8) = *(i + 8) + (*var_4 << 8);
                        void* eax_7 = *(i + 0xc);
                        *(var_4 + 0x14) = eax_7;
                        
                        if (eax_7)
                            *(eax_7 + 0x10) = var_4;
                        
                        *(i + 0xc) = var_4;
                    }
                    
                    void** result = sub_406470(8);
                    *result = *((arg1 << 2) + &data_444c40);
                    result[1] = var_4;
                    *((arg1 << 2) + &data_444c40) = result;
                    return result;
                }
                
                i = *(i + 0x14);
            } while (i);
            
            sub_406cc0();
            i = data_444b28;
        }
        else
        {
            sub_406cc0();
            i = data_444b28;
        }
    }
}

void* sub_406cc0()
{
    void* eax = sub_406470(0x18);
    __builtin_memcpy(eax, &data_4448e8, 0x18);
    *(eax + 0x14) = data_444b28;
    void* result = sub_406d20(0x10000, 0x10000);
    *(eax + 8) = result;
    
    if (data_444b28)
    {
        result = data_444b28;
        *(result + 0x10) = eax;
    }
    
    data_444b28 = eax;
    return result;
}

void* sub_406d20(int32_t arg1, int32_t arg2)
{
    void* eax_2 = sub_406470(arg1 + arg2 + 4);
    void* edi_1 = eax_2 - COMBINE(0, eax_2 + 4) % arg2;
    *(edi_1 + arg2) = eax_2;
    return edi_1 + arg2 + 4;
}

int32_t* sub_406d50(int32_t arg1, int32_t* arg2)
{
    int32_t* result = arg2;
    
    if (!result)
        return 0xffffffff;
    
    if (*result >= arg1)
        return 0xffffffff;
    
    int32_t* i;
    
    do
    {
        i = result[5];
        int32_t esi_1 = 0x100;
        
        if (i)
            esi_1 = *i;
        
        if (esi_1 - result[1] >= arg1)
            return result;
        
        result = i;
    } while (i);
    return nullptr;
}

void sub_406d90(void* arg1)
{
    int32_t edx = *(arg1 + 8);
    
    if (edx <= 0 || edx > 0x100)
        return;
    
    int32_t ecx = *(arg1 + 4);
    
    if (ecx <= 0 || ecx > 0x100)
        return;
    
    void* edi_1 = data_444b28;
    int32_t i = *(arg1 + 0x10);
    
    if (!edi_1)
        return;
    
    while (*(edi_1 + 8) != (i & 0xffff0000))
    {
        edi_1 = *(edi_1 + 0x14);
        
        if (!edi_1)
            return;
    }
    
    void* i_1 = *(edi_1 + 0xc);
    
    while (*(i_1 + 8) != (i & 0xffffff00))
    {
        i_1 = *(i_1 + 0x14);
        
        if (!i_1)
            return;
    }
    
    void* esi_1 = *(i_1 + 0xc);
    
    while (*(esi_1 + 8) != i)
    {
        esi_1 = *(esi_1 + 0x14);
        
        if (!esi_1)
            return;
    }
    
    void** var_4_1 = nullptr;
    void** ebp_1 = *((edx << 2) + &data_444c40);
    
    while (ebp_1[1] != i_1)
    {
        var_4_1 = ebp_1;
        ebp_1 = *ebp_1;
        
        if (!ebp_1)
            return;
    }
    
    int32_t ecx_4 = *(esi_1 + 0x14);
    
    if (ecx_4 || *(esi_1 + 0x10))
    {
        void* eax_10 = *(esi_1 + 0x10);
        
        if (!eax_10)
            *(i_1 + 0xc) = ecx_4;
        else
            *(eax_10 + 0x14) = ecx_4;
        
        void* eax_11 = *(esi_1 + 0x14);
        
        if (eax_11)
            *(eax_11 + 0x10) = *(esi_1 + 0x10);
    }
    else
    {
        int32_t eax_2 = *(i_1 + 0x14);
        
        if (eax_2 || *(i_1 + 0x10))
        {
            void* ecx_7 = *(i_1 + 0x10);
            
            if (!ecx_7)
                *(edi_1 + 0xc) = eax_2;
            else
                *(ecx_7 + 0x14) = eax_2;
            
            void* eax_7 = *(i_1 + 0x14);
            
            if (eax_7)
                *(eax_7 + 0x10) = *(i_1 + 0x10);
        }
        else
        {
            sub_406f40(*(edi_1 + 8));
            void* eax_4 = *(edi_1 + 0x10);
            
            if (!eax_4)
                data_444b28 = *(edi_1 + 0x14);
            else
                *(eax_4 + 0x14) = *(edi_1 + 0x14);
            
            void* eax_6 = *(edi_1 + 0x14);
            
            if (eax_6)
                *(eax_6 + 0x10) = *(edi_1 + 0x10);
            
            sub_406570(edi_1);
        }
        
        if (!var_4_1)
            *((*(arg1 + 8) << 2) + &data_444c40) = *ebp_1;
        else
            *var_4_1 = *ebp_1;
        
        sub_406570(ebp_1);
        sub_406570(i_1);
    }
    
    sub_406570(esi_1);
}

int32_t sub_406f40(void* arg1)
{
    return sub_406570(*(arg1 - 4));
}

int32_t sub_406f60() __pure
{
    return 0;
}

int32_t sub_406f70(int32_t* arg1)
{
    int32_t i = 0;
    arg1[3] = sub_406470(0x80);
    int32_t result;
    
    do
    {
        i += 8;
        *(arg1[3] + i - 8) = 0;
        result = arg1[3];
        *(result + i - 4) = 0;
    } while (i < 0x80);
    
    arg1[2] = 0xf;
    arg1[1] = 0;
    *arg1 = 0x10;
    return result;
}

int32_t sub_406fc0()
{
    void* ecx = &data_447038;
    void* i = &data_4450b8;
    data_44cdf8 = &data_446ff8;
    
    do
    {
        *i = ecx;
        i += 4;
        *ecx = 0;
        ecx += 0xc;
    } while (i < &data_446ff8);
    
    sub_4067e0(&data_446ff8, 0);
    return 1;
}

int32_t* sub_407000(void* arg1, int32_t* arg2)
{
    int32_t* result = arg2;
    void* edi;
    
    if (result)
    {
        int32_t eax_7 = *result;
        
        if (!eax_7)
            return 0;
        
        edi = arg1;
        
        if (!edi)
        {
            sub_406860(0, eax_7);
            *result = 0;
            **&data_44cdf8 = result;
            data_44cdf8 += 4;
            return 0;
        }
    }
    else
    {
        edi = arg1;
        
        if (!edi)
            return 0;
        
        if (data_44cdf8 == &data_4450b8)
            return 0;
        
        data_44cdf8 -= 4;
        result = **&data_44cdf8;
        *result = 0;
    }
    
    data_446ff8 = 0;
    data_446ffc = *(edi + 4);
    data_447000 = *(edi + 8);
    data_447004 = *(edi + 0xc);
    data_447008 = *(edi + 0x10);
    data_44700c = *(edi + 0x14);
    *result = sub_406860(&data_446ff8, *result);
    result[1] = *(edi + 0x18);
    result[2] = *(edi + 0x1c);
    return result;
}

int32_t sub_4070d0(int32_t* arg1, int32_t arg2)
{
    sub_406710(arg1, arg2);
    return 1;
}

int32_t sub_4070f0(void* arg1)
{
    sub_4069b0(arg1);
    return 1;
}

int32_t sub_407110(void* arg1, int32_t arg2)
{
    sub_4067e0(arg1, arg2);
    return 1;
}

int32_t sub_407130(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    return sub_408fa4(arg1, arg4, arg3);
}

char* sub_407150(char* arg1, int32_t arg2, int32_t arg3, int32_t arg4, char* arg5)
{
    int16_t edx;
    return sub_408fca(arg5, edx, arg3, arg2, arg1);
}

char* sub_407170(char* arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    return sub_40901f(arg3, arg2, arg1);
}

void sub_407190()
{
    data_44ce2c = 0;
    
    if (data_41ca54)
        return;
    
    data_44ce38 = sub_4111a0(0x20d8);
    data_44ce3c = sub_4111a0(0x20d8);
    data_44ce40 = sub_4111a0(0x20d8);
    data_41ca54 = 1;
}

void* sub_4071f0(void* arg1)
{
    data_41caac = *(arg1 + 4);
    data_41cab0 = *(arg1 + 8);
    data_41cab4 = *(arg1 + 0xc);
    data_41cab8 = *(arg1 + 0x10);
    data_41cabc = *(arg1 + 0x14);
    data_41cac0 = *(arg1 + 0x18);
    data_41cac4 = *(arg1 + 0x1c);
    data_41cac8 = *(arg1 + 0x20);
    data_41cacc = *(arg1 + 0x24);
    data_41cad0 = *(arg1 + 0x28);
    data_41cad4 = *(arg1 + 0x2c);
    int32_t ecx_4 = data_453068;
    data_41cad8 = *(arg1 + 0x30);
    int32_t edx_4 = data_41cab4;
    bool cond:0 = data_41caac <= edx_4;
    data_44ce24 = *(arg1 + 0x34);
    data_44ce30 = ecx_4;
    
    if (cond:0)
    {
        int32_t ecx_5 = data_41caac;
        data_44ce58 = data_41cab4;
        data_44ce34 = ecx_5;
    }
    else
    {
        int32_t eax_5 = data_41caac;
        data_44ce34 = edx_4;
        data_44ce58 = eax_5;
    }
    
    int32_t eax_8 = data_41cabc;
    
    if (data_41cabc > data_44ce58)
        data_44ce58 = eax_8;
    else if (data_44ce34 > eax_8)
        data_44ce34 = eax_8;
    
    int32_t eax_9 = data_41cac0;
    
    if (data_41cab0 > eax_9)
    {
        int32_t edx_5 = data_41cad8;
        int32_t ebx_1 = data_41cabc;
        data_41cac0 = data_41cab0;
        int32_t ecx_7 = data_41caac;
        data_41cab0 = eax_9;
        int32_t eax_10 = data_41cac8;
        data_41cabc = ecx_7;
        data_41cad8 = eax_10;
        data_41cac8 = edx_5;
        int32_t eax_11 = data_41cad4;
        int32_t edx_6 = data_41cac4;
        data_41caac = ebx_1;
        data_41cad4 = edx_6;
        data_41cac4 = eax_11;
    }
    
    int32_t eax_12 = data_41cab8;
    
    if (data_41cab0 <= eax_12)
    {
        int32_t eax_15 = data_41cab8;
        
        if (data_41cac0 < eax_15)
        {
            int32_t edx_9 = data_41cad0;
            int32_t ebx_3 = data_41cab4;
            data_41cab8 = data_41cac0;
            int32_t ecx_11 = data_41cabc;
            data_41cac0 = eax_15;
            int32_t eax_16 = data_41cad8;
            data_41cab4 = ecx_11;
            data_41cad0 = eax_16;
            data_41cad8 = edx_9;
            int32_t eax_17 = data_41cacc;
            int32_t edx_10 = data_41cad4;
            data_41cabc = ebx_3;
            data_41cacc = edx_10;
            data_41cad4 = eax_17;
        }
    }
    else
    {
        int32_t edx_7 = data_41cad0;
        int32_t ebx_2 = data_41cab4;
        data_41cab8 = data_41cab0;
        int32_t ecx_9 = data_41caac;
        data_41cab0 = eax_12;
        int32_t eax_13 = data_41cac8;
        data_41cab4 = ecx_9;
        data_41cad0 = eax_13;
        data_41cac8 = edx_7;
        int32_t eax_14 = data_41cacc;
        int32_t edx_8 = data_41cac4;
        data_41caac = ebx_2;
        data_41cacc = edx_8;
        data_41cac4 = eax_14;
    }
    
    if (data_41cab0 < data_453044)
        return sub_407910(arg1);
    
    if (data_41cac0 > data_453058)
        return sub_407910(arg1);
    
    if (data_44ce58 > data_453064)
        return sub_407910(arg1);
    
    if (data_44ce34 < data_453060)
        return sub_407910(arg1);
    
    sub_456000();
    sub_407130(data_41cae0, data_41cadc, data_44ce24, *(arg1 + 0x38));
    int32_t eax_29 = sub_407f40(data_41caac, data_41cab0, data_41cabc, data_41cac0);
    int32_t eax_31 = sub_407f40(data_41caac, data_41cab0, data_41cab4, data_41cab8);
    int32_t edx_14 = data_41cab4;
    int32_t eax_32 = data_41caac;
    data_44ce04 = data_41cab0;
    int32_t ecx_18 = data_41cac4;
    data_44ce00 = eax_32;
    int32_t eax_33 = data_41cab8;
    data_44ce08 = edx_14;
    int32_t edx_15 = data_41cac8;
    data_44ce0c = eax_33;
    int32_t eax_34 = data_41cacc;
    data_44ce10 = ecx_18;
    int32_t ecx_19 = data_41cad0;
    data_44ce14 = edx_15;
    data_44ce18 = eax_34;
    data_44ce1c = ecx_19;
    void* eax_57;
    
    if (eax_29 <= eax_31)
    {
        data_44ce38;
        sub_456180(&data_44ce00);
        int32_t ecx_30 = data_41cab0;
        int32_t edx_30 = data_41cabc;
        data_44ce00 = data_41caac;
        int32_t eax_69 = data_41cac0;
        data_44ce04 = ecx_30;
        int32_t ecx_31 = data_41cac4;
        data_44ce08 = edx_30;
        int32_t edx_31 = data_41cac8;
        data_44ce0c = eax_69;
        int32_t eax_70 = data_41cad4;
        data_44ce10 = ecx_31;
        int32_t ecx_32 = data_41cad8;
        data_44ce14 = edx_31;
        data_44ce18 = eax_70;
        data_44ce1c = ecx_32;
        data_44ce3c;
        sub_4561c0(&data_44ce00);
        int32_t ecx_33 = data_41cab8;
        int32_t edx_32 = data_41cabc;
        data_44ce00 = data_41cab4;
        int32_t eax_72 = data_41cac0;
        data_44ce04 = ecx_33;
        int32_t ecx_34 = data_41cacc;
        data_44ce08 = edx_32;
        int32_t edx_33 = data_41cad0;
        data_44ce0c = eax_72;
        int32_t eax_73 = data_41cad4;
        data_44ce10 = ecx_34;
        int32_t ecx_35 = data_41cad8;
        data_44ce14 = edx_33;
        data_44ce18 = eax_73;
        data_44ce1c = ecx_35;
        data_44ce40;
        sub_456180(&data_44ce00);
        int32_t esi_3 = 0;
        int32_t i = 0;
        data_44ce30 += **&data_44ce3c * data_45305c;
        
        if (*(data_44ce38 + 4) > 0)
        {
            void* eax_89;
            
            do
            {
                void* ebx_14 = data_44ce38 + esi_3;
                char* ebp_8 = data_44ce3c + esi_3 + 8;
                esi_3 += 0xc;
                int32_t edx_34 = *(ebx_14 + 0xc);
                int32_t ecx_36 = *(ebp_8 + 8);
                i += 1;
                sub_407150(data_44ce30 + ecx_36 + 1, *(ebx_14 + 0x10) - ecx_36 - 1, 
                    *(ebx_14 + 8) + (edx_34 << 0x18), edx_34 >> 8, ebp_8);
                eax_89 = data_44ce38;
                data_44ce30 += data_45305c;
            } while (*(eax_89 + 4) > i);
        }
        
        eax_57 = data_44ce40;
        int32_t esi_4 = 0;
        int32_t i_1 = 0;
        
        if (*(eax_57 + 4) > 0)
        {
            int32_t edi_2 = i * 0xc;
            
            do
            {
                void* ebp_10 = data_44ce40 + esi_4;
                char* edx_38 = data_44ce3c + edi_2 + 8;
                edi_2 += 0xc;
                int32_t eax_91 = *(ebp_10 + 0xc);
                int32_t ecx_38 = *(edx_38 + 8);
                esi_4 += 0xc;
                i_1 += 1;
                sub_407150(data_44ce30 + ecx_38 + 1, *(ebp_10 + 0x10) - ecx_38 - 1, 
                    *(ebp_10 + 8) + (eax_91 << 0x18), eax_91 >> 8, edx_38);
                eax_57 = data_44ce40;
                data_44ce30 += data_45305c;
            } while (*(eax_57 + 4) > i_1);
        }
    }
    else
    {
        data_44ce38;
        sub_4561c0(&data_44ce00);
        int32_t ecx_20 = data_41cab0;
        int32_t edx_16 = data_41cabc;
        data_44ce00 = data_41caac;
        int32_t eax_36 = data_41cac0;
        data_44ce04 = ecx_20;
        int32_t ecx_21 = data_41cac4;
        data_44ce08 = edx_16;
        int32_t edx_17 = data_41cac8;
        data_44ce0c = eax_36;
        int32_t eax_37 = data_41cad4;
        data_44ce10 = ecx_21;
        int32_t ecx_22 = data_41cad8;
        data_44ce14 = edx_17;
        data_44ce18 = eax_37;
        data_44ce1c = ecx_22;
        data_44ce3c;
        sub_456180(&data_44ce00);
        int32_t ecx_23 = data_41cab8;
        int32_t edx_18 = data_41cabc;
        data_44ce00 = data_41cab4;
        int32_t eax_39 = data_41cac0;
        data_44ce04 = ecx_23;
        int32_t ecx_24 = data_41cacc;
        data_44ce08 = edx_18;
        int32_t edx_19 = data_41cad0;
        data_44ce0c = eax_39;
        int32_t eax_40 = data_41cad4;
        data_44ce10 = ecx_24;
        int32_t ecx_25 = data_41cad8;
        data_44ce14 = edx_19;
        data_44ce18 = eax_40;
        data_44ce1c = ecx_25;
        data_44ce40;
        sub_4561c0(&data_44ce00);
        int32_t i_2 = 0;
        data_44ce30 += **&data_44ce38 * data_45305c;
        
        if (*(data_44ce38 + 4) > 0)
        {
            int32_t edi_1 = 0;
            void* eax_56;
            
            do
            {
                void* ebx_8 = data_44ce3c + edi_1;
                char* ebp_3 = data_44ce38 + edi_1 + 8;
                edi_1 += 0xc;
                int32_t edx_20 = *(ebx_8 + 0xc);
                int32_t ecx_26 = *(ebp_3 + 8);
                i_2 += 1;
                sub_407150(data_44ce30 + ecx_26 + 1, *(ebx_8 + 0x10) - ecx_26 - 1, 
                    *(ebx_8 + 8) + (edx_20 << 0x18), edx_20 >> 8, ebp_3);
                eax_56 = data_44ce38;
                data_44ce30 += data_45305c;
            } while (*(eax_56 + 4) > i_2);
        }
        
        int32_t i_3 = 0;
        eax_57 = data_44ce40;
        
        if (*(eax_57 + 4) > 0)
        {
            int32_t esi_2 = 0;
            int32_t ebx_9 = i_2 * 0xc;
            void* eax_67;
            
            do
            {
                void* ebp_5 = data_44ce3c + ebx_9;
                char* edx_24 = data_44ce40 + esi_2 + 8;
                ebx_9 += 0xc;
                int32_t eax_59 = *(ebp_5 + 0xc);
                int32_t ecx_28 = *(edx_24 + 8);
                esi_2 += 0xc;
                i_3 += 1;
                sub_407150(data_44ce30 + ecx_28 + 1, *(ebp_5 + 0x10) - ecx_28 - 1, 
                    *(ebp_5 + 8) + (eax_59 << 0x18), eax_59 >> 8, edx_24);
                eax_67 = data_44ce40;
                data_44ce30 += data_45305c;
            } while (*(eax_67 + 4) > i_3);
            
            return eax_67;
        }
    }
    
    return eax_57;
}

void* sub_407910(void* arg1)
{
    void* eax = data_453044;
    
    if (data_41cac0 >= eax)
    {
        eax = data_453058;
        
        if (data_41cab0 <= eax)
        {
            eax = data_453064;
            
            if (data_44ce34 <= eax)
            {
                eax = data_453060;
                
                if (data_44ce58 >= eax)
                {
                    sub_456000();
                    sub_407130(data_41cae0, data_41cadc, data_44ce24, *(arg1 + 0x38));
                    int32_t eax_4 = sub_407f40(data_41cab4, data_41cab8, data_41cabc, data_41cac0);
                    int32_t eax_6 = sub_407f40(data_41caac, data_41cab0, data_41cab4, data_41cab8);
                    int32_t edx_4 = data_41cab4;
                    int32_t eax_7 = data_41caac;
                    data_44ce64 = data_41cab0;
                    int32_t ecx_8 = data_41cac4;
                    data_44ce60 = eax_7;
                    int32_t eax_8 = data_41cab8;
                    data_44ce68 = edx_4;
                    int32_t edx_5 = data_41cac8;
                    data_44ce6c = eax_8;
                    int32_t eax_9 = data_41cacc;
                    data_44ce70 = ecx_8;
                    int32_t ecx_9 = data_41cad0;
                    data_44ce74 = edx_5;
                    data_44ce78 = eax_9;
                    data_44ce7c = ecx_9;
                    
                    if (eax_4 <= eax_6)
                    {
                        data_44ce38;
                        sub_456520(&data_44ce60);
                        int32_t ecx_22 = data_41cab0;
                        int32_t edx_24 = data_41cabc;
                        data_44ce60 = data_41caac;
                        int32_t eax_63 = data_41cac0;
                        data_44ce64 = ecx_22;
                        int32_t ecx_23 = data_41cac4;
                        data_44ce68 = edx_24;
                        int32_t edx_25 = data_41cac8;
                        data_44ce6c = eax_63;
                        int32_t eax_64 = data_41cad4;
                        data_44ce70 = ecx_23;
                        int32_t ecx_24 = data_41cad8;
                        data_44ce74 = edx_25;
                        data_44ce78 = eax_64;
                        data_44ce7c = ecx_24;
                        data_44ce3c;
                        sub_456640(&data_44ce60);
                        int32_t ecx_25 = data_41cab8;
                        int32_t edx_26 = data_41cabc;
                        data_44ce60 = data_41cab4;
                        int32_t eax_66 = data_41cac0;
                        data_44ce64 = ecx_25;
                        int32_t ecx_26 = data_41cacc;
                        data_44ce68 = edx_26;
                        int32_t edx_27 = data_41cad0;
                        data_44ce6c = eax_66;
                        int32_t eax_67 = data_41cad4;
                        data_44ce70 = ecx_26;
                        int32_t ecx_27 = data_41cad8;
                        data_44ce74 = edx_27;
                        data_44ce78 = eax_67;
                        data_44ce7c = ecx_27;
                        data_44ce40;
                        sub_456520(&data_44ce60);
                        int32_t esi_3 = 0;
                        int32_t i = 0;
                        data_44ce30 += **&data_44ce3c * data_45305c;
                        
                        if (*(data_44ce38 + 4) > 0)
                        {
                            do
                            {
                                void* ebp_7 = data_44ce3c + esi_3;
                                int32_t ecx_28 = *(ebp_7 + 0x10);
                                
                                if (data_453054 <= ecx_28)
                                {
                                    void* ebx_11 = data_44ce38 + esi_3;
                                    int32_t edx_30 = *(ebx_11 + 0xc);
                                    sub_407150(data_44ce30 + ecx_28 + 1, 
                                        *(ebx_11 + 0x10) - ecx_28 - 1, 
                                        *(ebx_11 + 8) + (edx_30 << 0x18), edx_30 >> 8, ebp_7 + 8);
                                }
                                else
                                {
                                    void* edx_29 = data_44ce38 + esi_3;
                                    int32_t ecx_29 = *(edx_29 + 0xc);
                                    sub_407170(data_453054 + data_44ce30, 
                                        *(edx_29 + 0x10) - data_453054, 
                                        *(edx_29 + 8) + (ecx_29 << 0x18), ecx_29 >> 8);
                                }
                                
                                esi_3 += 0xc;
                                i += 1;
                                data_44ce30 += data_45305c;
                            } while (*(data_44ce38 + 4) > i);
                        }
                        
                        eax = data_44ce40;
                        int32_t esi_4 = 0;
                        int32_t i_1 = 0;
                        
                        if (*(eax + 4) > 0)
                        {
                            int32_t edi_2 = i * 0xc;
                            
                            do
                            {
                                void* edx_33 = data_44ce3c + edi_2;
                                int32_t ecx_31 = *(edx_33 + 0x10);
                                
                                if (data_453054 <= ecx_31)
                                {
                                    void* ebp_10 = data_44ce40 + esi_4;
                                    int32_t eax_104 = *(ebp_10 + 0xc);
                                    sub_407150(data_44ce30 + ecx_31 + 1, 
                                        *(ebp_10 + 0x10) - ecx_31 - 1, 
                                        *(ebp_10 + 8) + (eax_104 << 0x18), eax_104 >> 8, 
                                        edx_33 + 8);
                                }
                                else
                                {
                                    void* edx_35 = data_44ce40 + esi_4;
                                    int32_t ecx_32 = *(edx_35 + 0xc);
                                    sub_407170(data_453054 + data_44ce30, 
                                        *(edx_35 + 0x10) - data_453054, 
                                        *(edx_35 + 8) + (ecx_32 << 0x18), ecx_32 >> 8);
                                }
                                
                                edi_2 += 0xc;
                                data_44ce30 += data_45305c;
                                eax = data_44ce40;
                                esi_4 += 0xc;
                                i_1 += 1;
                            } while (*(eax + 4) > i_1);
                        }
                    }
                    else
                    {
                        data_44ce38;
                        sub_456640(&data_44ce60);
                        int32_t ecx_10 = data_41cab0;
                        int32_t edx_6 = data_41cabc;
                        data_44ce60 = data_41caac;
                        int32_t eax_11 = data_41cac0;
                        data_44ce64 = ecx_10;
                        int32_t ecx_11 = data_41cac4;
                        data_44ce68 = edx_6;
                        int32_t edx_7 = data_41cac8;
                        data_44ce6c = eax_11;
                        int32_t eax_12 = data_41cad4;
                        data_44ce70 = ecx_11;
                        int32_t ecx_12 = data_41cad8;
                        data_44ce74 = edx_7;
                        data_44ce78 = eax_12;
                        data_44ce7c = ecx_12;
                        data_44ce3c;
                        sub_456520(&data_44ce60);
                        int32_t ecx_13 = data_41cab8;
                        int32_t edx_8 = data_41cabc;
                        data_44ce60 = data_41cab4;
                        int32_t eax_14 = data_41cac0;
                        data_44ce64 = ecx_13;
                        int32_t ecx_14 = data_41cacc;
                        data_44ce68 = edx_8;
                        int32_t edx_9 = data_41cad0;
                        data_44ce6c = eax_14;
                        int32_t eax_15 = data_41cad4;
                        data_44ce70 = ecx_14;
                        int32_t ecx_15 = data_41cad8;
                        data_44ce74 = edx_9;
                        data_44ce78 = eax_15;
                        data_44ce7c = ecx_15;
                        data_44ce40;
                        sub_456640(&data_44ce60);
                        int32_t i_2 = 0;
                        data_44ce30 += **&data_44ce38 * data_45305c;
                        
                        if (*(data_44ce38 + 4) > 0)
                        {
                            int32_t edi_1 = 0;
                            
                            do
                            {
                                void* ebp_2 = data_44ce38 + edi_1;
                                int32_t ecx_16 = *(ebp_2 + 0x10);
                                
                                if (data_453054 <= ecx_16)
                                {
                                    void* ebx_5 = data_44ce3c + edi_1;
                                    int32_t edx_12 = *(ebx_5 + 0xc);
                                    sub_407150(data_44ce30 + ecx_16 + 1, 
                                        *(ebx_5 + 0x10) - ecx_16 - 1, 
                                        *(ebx_5 + 8) + (edx_12 << 0x18), edx_12 >> 8, ebp_2 + 8);
                                }
                                else
                                {
                                    void* edx_11 = data_44ce3c + edi_1;
                                    int32_t ecx_17 = *(edx_11 + 0xc);
                                    sub_407170(data_453054 + data_44ce30, 
                                        *(edx_11 + 0x10) - data_453054, 
                                        *(edx_11 + 8) + (ecx_17 << 0x18), ecx_17 >> 8);
                                }
                                
                                edi_1 += 0xc;
                                i_2 += 1;
                                data_44ce30 += data_45305c;
                            } while (*(data_44ce38 + 4) > i_2);
                        }
                        
                        int32_t i_3 = 0;
                        eax = data_44ce40;
                        
                        if (*(eax + 4) > 0)
                        {
                            int32_t esi_2 = 0;
                            int32_t ebx_6 = i_2 * 0xc;
                            void* eax_61;
                            
                            do
                            {
                                void* edx_15 = data_44ce40 + esi_2;
                                int32_t ecx_19 = *(edx_15 + 0x10);
                                
                                if (data_453054 <= ecx_19)
                                {
                                    void* ebp_5 = data_44ce3c + ebx_6;
                                    int32_t eax_52 = *(ebp_5 + 0xc);
                                    sub_407150(data_44ce30 + ecx_19 + 1, 
                                        *(ebp_5 + 0x10) - ecx_19 - 1, 
                                        *(ebp_5 + 8) + (eax_52 << 0x18), eax_52 >> 8, edx_15 + 8);
                                }
                                else
                                {
                                    void* edx_17 = data_44ce3c + ebx_6;
                                    int32_t ecx_20 = *(edx_17 + 0xc);
                                    sub_407170(data_453054 + data_44ce30, 
                                        *(edx_17 + 0x10) - data_453054, 
                                        *(edx_17 + 8) + (ecx_20 << 0x18), ecx_20 >> 8);
                                }
                                
                                ebx_6 += 0xc;
                                data_44ce30 += data_45305c;
                                eax_61 = data_44ce40;
                                esi_2 += 0xc;
                                i_3 += 1;
                            } while (*(eax_61 + 4) > i_3);
                            
                            return eax_61;
                        }
                    }
                }
            }
        }
    }
    
    return eax;
}

int32_t sub_407f40(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4) __pure
{
    int32_t result;
    
    if (arg4 != arg2)
        result = ((arg1 - arg3) << 8) / (arg2 - arg4);
    else
    {
        result = 0x80010000;
        
        if (arg3 >= arg1)
            return 0x7fff0000;
    }
    
    return result;
}

int32_t sub_407f80(int32_t arg1, int32_t arg2, int32_t arg3)
{
    int32_t edx;
    return sub_4090b8(arg1, edx, arg3);
}

int32_t sub_407fa0(char* arg1, int32_t arg2, int32_t arg3, int32_t arg4, char* arg5)
{
    int16_t edx;
    return sub_4090d8(arg5, edx, arg3, arg2, arg1);
}

int32_t sub_407fc0(int32_t* arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    return sub_40914e(arg3, arg2, arg1);
}

void sub_407fe0()
{
    data_44ceb0 = 0;
    
    if (data_41ca58)
        return;
    
    data_44cebc = sub_4111a0(0x20d8);
    data_44cec0 = sub_4111a0(0x20d8);
    data_44cec4 = sub_4111a0(0x20d8);
    data_41ca58 = 1;
}

void* sub_408040(void* arg1)
{
    data_41caac = *(arg1 + 4);
    data_41cab0 = *(arg1 + 8);
    data_41cab4 = *(arg1 + 0xc);
    data_41cab8 = *(arg1 + 0x10);
    data_41cabc = *(arg1 + 0x14);
    data_41cac0 = *(arg1 + 0x18);
    data_41cac4 = *(arg1 + 0x1c);
    data_41cac8 = *(arg1 + 0x20);
    data_41cacc = *(arg1 + 0x24);
    data_41cad0 = *(arg1 + 0x28);
    data_41cad4 = *(arg1 + 0x2c);
    int32_t edx_4 = data_453068;
    data_41cad8 = *(arg1 + 0x30);
    int32_t ebx_4 = data_41caac;
    bool cond:0 = data_41cab4 >= ebx_4;
    data_44cea8 = *(arg1 + 0x34);
    data_44ceb4 = edx_4;
    int32_t edx_5;
    
    if (cond:0)
    {
        edx_5 = data_41caac;
        data_44ced8 = data_41cab4;
    }
    else
    {
        edx_5 = data_41cab4;
        data_44ced8 = ebx_4;
    }
    
    int32_t eax_6 = data_44ced8;
    data_44ceb8 = edx_5;
    int32_t eax_7 = data_41cabc;
    
    if (data_41cabc > eax_6)
        data_44ced8 = eax_7;
    else if (data_44ceb8 > eax_7)
        data_44ceb8 = eax_7;
    
    int32_t eax_8 = data_41cac0;
    
    if (data_41cab0 > eax_8)
    {
        int32_t ebx_5 = data_41cad8;
        int32_t ebp_1 = data_41cabc;
        data_41cac0 = data_41cab0;
        int32_t edx_7 = data_41caac;
        data_41cab0 = eax_8;
        int32_t eax_9 = data_41cac8;
        data_41cabc = edx_7;
        data_41cad8 = eax_9;
        data_41cac8 = ebx_5;
        int32_t eax_10 = data_41cad4;
        int32_t ebx_6 = data_41cac4;
        data_41caac = ebp_1;
        data_41cad4 = ebx_6;
        data_41cac4 = eax_10;
    }
    
    int32_t eax_11 = data_41cab8;
    
    if (data_41cab0 <= eax_11)
    {
        int32_t eax_14 = data_41cab8;
        
        if (data_41cac0 < eax_14)
        {
            int32_t ebx_9 = data_41cad0;
            int32_t ebp_3 = data_41cab4;
            data_41cab8 = data_41cac0;
            int32_t edx_11 = data_41cabc;
            data_41cac0 = eax_14;
            int32_t eax_15 = data_41cad8;
            data_41cab4 = edx_11;
            data_41cad0 = eax_15;
            data_41cad8 = ebx_9;
            int32_t eax_16 = data_41cacc;
            int32_t ebx_10 = data_41cad4;
            data_41cabc = ebp_3;
            data_41cacc = ebx_10;
            data_41cad4 = eax_16;
        }
    }
    else
    {
        int32_t ebx_7 = data_41cad0;
        int32_t ebp_2 = data_41cab4;
        data_41cab8 = data_41cab0;
        int32_t edx_9 = data_41caac;
        data_41cab0 = eax_11;
        int32_t eax_12 = data_41cac8;
        data_41cab4 = edx_9;
        data_41cad0 = eax_12;
        data_41cac8 = ebx_7;
        int32_t eax_13 = data_41cacc;
        int32_t ebx_8 = data_41cac4;
        data_41caac = ebp_2;
        data_41cacc = ebx_8;
        data_41cac4 = eax_13;
    }
    
    if (data_41cab0 < data_453044)
    {
        void* var_14 = arg1;
        return sub_408750();
    }
    
    if (data_41cac0 > data_453058)
    {
        void* var_14_1 = arg1;
        return sub_408750();
    }
    
    if (data_44ced8 > data_453064)
    {
        void* var_14_2 = arg1;
        return sub_408750();
    }
    
    if (data_453060 > data_44ceb8)
    {
        void* var_14_3 = arg1;
        return sub_408750();
    }
    
    sub_456000();
    sub_407f80(data_41cae0, data_41cadc, data_44cea8);
    int32_t eax_27 = sub_408d70(data_41caac, data_41cab0, data_41cabc, data_41cac0);
    int32_t eax_29 = sub_408d70(data_41caac, data_41cab0, data_41cab4, data_41cab8);
    int32_t edx_15 = data_41cab4;
    int32_t eax_30 = data_41caac;
    data_44ce8c = data_41cab0;
    int32_t ecx_6 = data_41cac4;
    data_44ce88 = eax_30;
    int32_t eax_31 = data_41cab8;
    data_44ce90 = edx_15;
    int32_t edx_16 = data_41cac8;
    data_44ce94 = eax_31;
    int32_t eax_32 = data_41cacc;
    data_44ce98 = ecx_6;
    int32_t ecx_7 = data_41cad0;
    data_44ce9c = edx_16;
    data_44cea0 = eax_32;
    data_44cea4 = ecx_7;
    void* eax_55;
    
    if (eax_27 <= eax_29)
    {
        data_44cebc;
        sub_456180(&data_44ce88);
        int32_t ecx_18 = data_41cab0;
        int32_t edx_31 = data_41cabc;
        data_44ce88 = data_41caac;
        int32_t eax_67 = data_41cac0;
        data_44ce8c = ecx_18;
        int32_t ecx_19 = data_41cac4;
        data_44ce90 = edx_31;
        int32_t edx_32 = data_41cac8;
        data_44ce94 = eax_67;
        int32_t eax_68 = data_41cad4;
        data_44ce98 = ecx_19;
        int32_t ecx_20 = data_41cad8;
        data_44ce9c = edx_32;
        data_44cea0 = eax_68;
        data_44cea4 = ecx_20;
        data_44cec0;
        sub_4561c0(&data_44ce88);
        int32_t ecx_21 = data_41cab8;
        int32_t edx_33 = data_41cabc;
        data_44ce88 = data_41cab4;
        int32_t eax_70 = data_41cac0;
        data_44ce8c = ecx_21;
        int32_t ecx_22 = data_41cacc;
        data_44ce90 = edx_33;
        int32_t edx_34 = data_41cad0;
        data_44ce94 = eax_70;
        int32_t eax_71 = data_41cad4;
        data_44ce98 = ecx_22;
        int32_t ecx_23 = data_41cad8;
        data_44ce9c = edx_34;
        data_44cea0 = eax_71;
        data_44cea4 = ecx_23;
        data_44cec4;
        sub_456180(&data_44ce88);
        int32_t esi_2 = 0;
        int32_t i = 0;
        data_44ceb4 += **&data_44cec0 * data_45305c;
        
        if (*(data_44cebc + 4) > 0)
        {
            void* eax_87;
            
            do
            {
                void* ebx_21 = data_44cebc + esi_2;
                char* ebp_11 = data_44cec0 + esi_2 + 8;
                esi_2 += 0xc;
                int32_t edx_35 = *(ebx_21 + 0xc);
                int32_t ecx_24 = *(ebp_11 + 8);
                i += 1;
                sub_407fa0(data_44ceb4 + ecx_24 + 1, *(ebx_21 + 0x10) - ecx_24 - 1, 
                    *(ebx_21 + 8) + (edx_35 << 0x18), edx_35 >> 8, ebp_11);
                eax_87 = data_44cebc;
                data_44ceb4 += data_45305c;
            } while (*(eax_87 + 4) > i);
        }
        
        eax_55 = data_44cec4;
        int32_t esi_3 = 0;
        int32_t i_1 = 0;
        
        if (*(eax_55 + 4) > 0)
        {
            int32_t edi_2 = i * 0xc;
            
            do
            {
                void* ebp_13 = data_44cec4 + esi_3;
                char* edx_39 = data_44cec0 + edi_2 + 8;
                edi_2 += 0xc;
                int32_t eax_89 = *(ebp_13 + 0xc);
                int32_t ecx_26 = *(edx_39 + 8);
                esi_3 += 0xc;
                i_1 += 1;
                sub_407fa0(data_44ceb4 + ecx_26 + 1, *(ebp_13 + 0x10) - ecx_26 - 1, 
                    *(ebp_13 + 8) + (eax_89 << 0x18), eax_89 >> 8, edx_39);
                eax_55 = data_44cec4;
                data_44ceb4 += data_45305c;
            } while (*(eax_55 + 4) > i_1);
        }
    }
    else
    {
        data_44cebc;
        sub_4561c0(&data_44ce88);
        int32_t ecx_8 = data_41cab0;
        int32_t edx_17 = data_41cabc;
        data_44ce88 = data_41caac;
        int32_t eax_34 = data_41cac0;
        data_44ce8c = ecx_8;
        int32_t ecx_9 = data_41cac4;
        data_44ce90 = edx_17;
        int32_t edx_18 = data_41cac8;
        data_44ce94 = eax_34;
        int32_t eax_35 = data_41cad4;
        data_44ce98 = ecx_9;
        int32_t ecx_10 = data_41cad8;
        data_44ce9c = edx_18;
        data_44cea0 = eax_35;
        data_44cea4 = ecx_10;
        data_44cec0;
        sub_456180(&data_44ce88);
        int32_t ecx_11 = data_41cab8;
        int32_t edx_19 = data_41cabc;
        data_44ce88 = data_41cab4;
        int32_t eax_37 = data_41cac0;
        data_44ce8c = ecx_11;
        int32_t ecx_12 = data_41cacc;
        data_44ce90 = edx_19;
        int32_t edx_20 = data_41cad0;
        data_44ce94 = eax_37;
        int32_t eax_38 = data_41cad4;
        data_44ce98 = ecx_12;
        int32_t ecx_13 = data_41cad8;
        data_44ce9c = edx_20;
        data_44cea0 = eax_38;
        data_44cea4 = ecx_13;
        data_44cec4;
        sub_4561c0(&data_44ce88);
        int32_t i_2 = 0;
        data_44ceb4 += **&data_44cebc * data_45305c;
        
        if (*(data_44cebc + 4) > 0)
        {
            int32_t edi_1 = 0;
            void* eax_54;
            
            do
            {
                void* ebx_15 = data_44cec0 + edi_1;
                char* ebp_6 = data_44cebc + edi_1 + 8;
                edi_1 += 0xc;
                int32_t edx_21 = *(ebx_15 + 0xc);
                int32_t ecx_14 = *(ebp_6 + 8);
                i_2 += 1;
                sub_407fa0(data_44ceb4 + ecx_14 + 1, *(ebx_15 + 0x10) - ecx_14 - 1, 
                    *(ebx_15 + 8) + (edx_21 << 0x18), edx_21 >> 8, ebp_6);
                eax_54 = data_44cebc;
                data_44ceb4 += data_45305c;
            } while (*(eax_54 + 4) > i_2);
        }
        
        int32_t i_3 = 0;
        eax_55 = data_44cec4;
        
        if (*(eax_55 + 4) > 0)
        {
            int32_t esi_1 = 0;
            int32_t ebx_16 = i_2 * 0xc;
            void* eax_65;
            
            do
            {
                void* ebp_8 = data_44cec0 + ebx_16;
                char* edx_25 = data_44cec4 + esi_1 + 8;
                ebx_16 += 0xc;
                int32_t eax_57 = *(ebp_8 + 0xc);
                int32_t ecx_16 = *(edx_25 + 8);
                esi_1 += 0xc;
                i_3 += 1;
                sub_407fa0(data_44ceb4 + ecx_16 + 1, *(ebp_8 + 0x10) - ecx_16 - 1, 
                    *(ebp_8 + 8) + (eax_57 << 0x18), eax_57 >> 8, edx_25);
                eax_65 = data_44cec4;
                data_44ceb4 += data_45305c;
            } while (*(eax_65 + 4) > i_3);
            
            return eax_65;
        }
    }
    
    return eax_55;
}

void* sub_408750()
{
    void* eax = data_453044;
    
    if (data_41cac0 >= eax)
    {
        eax = data_453058;
        
        if (data_41cab0 <= eax)
        {
            eax = data_453064;
            
            if (data_44ceb8 <= eax)
            {
                eax = data_44ced8;
                
                if (data_453060 <= eax)
                {
                    sub_456000();
                    sub_407f80(data_41cae0, data_41cadc, data_44cea8);
                    int32_t eax_3 = sub_408d70(data_41cab4, data_41cab8, data_41cabc, data_41cac0);
                    int32_t eax_5 = sub_408d70(data_41caac, data_41cab0, data_41cab4, data_41cab8);
                    int32_t edx_4 = data_41cab4;
                    int32_t eax_6 = data_41caac;
                    data_44cee4 = data_41cab0;
                    int32_t ecx_7 = data_41cac4;
                    data_44cee0 = eax_6;
                    int32_t eax_7 = data_41cab8;
                    data_44cee8 = edx_4;
                    int32_t edx_5 = data_41cac8;
                    data_44ceec = eax_7;
                    int32_t eax_8 = data_41cacc;
                    data_44cef0 = ecx_7;
                    int32_t ecx_8 = data_41cad0;
                    data_44cef4 = edx_5;
                    data_44cef8 = eax_8;
                    data_44cefc = ecx_8;
                    
                    if (eax_3 <= eax_5)
                    {
                        data_44cebc;
                        sub_456520(&data_44cee0);
                        int32_t ecx_21 = data_41cab0;
                        int32_t edx_24 = data_41cabc;
                        data_44cee0 = data_41caac;
                        int32_t eax_62 = data_41cac0;
                        data_44cee4 = ecx_21;
                        int32_t ecx_22 = data_41cac4;
                        data_44cee8 = edx_24;
                        int32_t edx_25 = data_41cac8;
                        data_44ceec = eax_62;
                        int32_t eax_63 = data_41cad4;
                        data_44cef0 = ecx_22;
                        int32_t ecx_23 = data_41cad8;
                        data_44cef4 = edx_25;
                        data_44cef8 = eax_63;
                        data_44cefc = ecx_23;
                        data_44cec0;
                        sub_456640(&data_44cee0);
                        int32_t ecx_24 = data_41cab8;
                        int32_t edx_26 = data_41cabc;
                        data_44cee0 = data_41cab4;
                        int32_t eax_65 = data_41cac0;
                        data_44cee4 = ecx_24;
                        int32_t ecx_25 = data_41cacc;
                        data_44cee8 = edx_26;
                        int32_t edx_27 = data_41cad0;
                        data_44ceec = eax_65;
                        int32_t eax_66 = data_41cad4;
                        data_44cef0 = ecx_25;
                        int32_t ecx_26 = data_41cad8;
                        data_44cef4 = edx_27;
                        data_44cef8 = eax_66;
                        data_44cefc = ecx_26;
                        data_44cec4;
                        sub_456520(&data_44cee0);
                        int32_t esi_3 = 0;
                        int32_t i = 0;
                        data_44ceb4 += **&data_44cec0 * data_45305c;
                        
                        if (*(data_44cebc + 4) > 0)
                        {
                            do
                            {
                                void* ebp_7 = data_44cec0 + esi_3;
                                int32_t ecx_27 = *(ebp_7 + 0x10);
                                
                                if (data_453054 <= ecx_27)
                                {
                                    void* ebx_11 = data_44cebc + esi_3;
                                    int32_t edx_30 = *(ebx_11 + 0xc);
                                    sub_407fa0(data_44ceb4 + ecx_27 + 1, 
                                        *(ebx_11 + 0x10) - ecx_27 - 1, 
                                        *(ebx_11 + 8) + (edx_30 << 0x18), edx_30 >> 8, ebp_7 + 8);
                                }
                                else
                                {
                                    void* edx_29 = data_44cebc + esi_3;
                                    int32_t ecx_28 = *(edx_29 + 0xc);
                                    sub_407fc0(data_453054 + data_44ceb4, 
                                        *(edx_29 + 0x10) - data_453054, 
                                        *(edx_29 + 8) + (ecx_28 << 0x18), ecx_28 >> 8);
                                }
                                
                                esi_3 += 0xc;
                                i += 1;
                                data_44ceb4 += data_45305c;
                            } while (*(data_44cebc + 4) > i);
                        }
                        
                        eax = data_44cec4;
                        int32_t esi_4 = 0;
                        int32_t i_1 = 0;
                        
                        if (*(eax + 4) > 0)
                        {
                            int32_t edi_2 = i * 0xc;
                            
                            do
                            {
                                void* edx_33 = data_44cec0 + edi_2;
                                int32_t ecx_30 = *(edx_33 + 0x10);
                                
                                if (data_453054 <= ecx_30)
                                {
                                    void* ebp_10 = data_44cec4 + esi_4;
                                    int32_t eax_103 = *(ebp_10 + 0xc);
                                    sub_407fa0(data_44ceb4 + ecx_30 + 1, 
                                        *(ebp_10 + 0x10) - ecx_30 - 1, 
                                        *(ebp_10 + 8) + (eax_103 << 0x18), eax_103 >> 8, 
                                        edx_33 + 8);
                                }
                                else
                                {
                                    void* edx_35 = data_44cec4 + esi_4;
                                    int32_t ecx_31 = *(edx_35 + 0xc);
                                    sub_407fc0(data_453054 + data_44ceb4, 
                                        *(edx_35 + 0x10) - data_453054, 
                                        *(edx_35 + 8) + (ecx_31 << 0x18), ecx_31 >> 8);
                                }
                                
                                edi_2 += 0xc;
                                data_44ceb4 += data_45305c;
                                eax = data_44cec4;
                                esi_4 += 0xc;
                                i_1 += 1;
                            } while (*(eax + 4) > i_1);
                        }
                    }
                    else
                    {
                        data_44cebc;
                        sub_456640(&data_44cee0);
                        int32_t ecx_9 = data_41cab0;
                        int32_t edx_6 = data_41cabc;
                        data_44cee0 = data_41caac;
                        int32_t eax_10 = data_41cac0;
                        data_44cee4 = ecx_9;
                        int32_t ecx_10 = data_41cac4;
                        data_44cee8 = edx_6;
                        int32_t edx_7 = data_41cac8;
                        data_44ceec = eax_10;
                        int32_t eax_11 = data_41cad4;
                        data_44cef0 = ecx_10;
                        int32_t ecx_11 = data_41cad8;
                        data_44cef4 = edx_7;
                        data_44cef8 = eax_11;
                        data_44cefc = ecx_11;
                        data_44cec0;
                        sub_456520(&data_44cee0);
                        int32_t ecx_12 = data_41cab8;
                        int32_t edx_8 = data_41cabc;
                        data_44cee0 = data_41cab4;
                        int32_t eax_13 = data_41cac0;
                        data_44cee4 = ecx_12;
                        int32_t ecx_13 = data_41cacc;
                        data_44cee8 = edx_8;
                        int32_t edx_9 = data_41cad0;
                        data_44ceec = eax_13;
                        int32_t eax_14 = data_41cad4;
                        data_44cef0 = ecx_13;
                        int32_t ecx_14 = data_41cad8;
                        data_44cef4 = edx_9;
                        data_44cef8 = eax_14;
                        data_44cefc = ecx_14;
                        data_44cec4;
                        sub_456640(&data_44cee0);
                        int32_t i_2 = 0;
                        data_44ceb4 += **&data_44cebc * data_45305c;
                        
                        if (*(data_44cebc + 4) > 0)
                        {
                            int32_t edi_1 = 0;
                            
                            do
                            {
                                void* ebp_2 = data_44cebc + edi_1;
                                int32_t ecx_15 = *(ebp_2 + 0x10);
                                
                                if (data_453054 <= ecx_15)
                                {
                                    void* ebx_5 = data_44cec0 + edi_1;
                                    int32_t edx_12 = *(ebx_5 + 0xc);
                                    sub_407fa0(data_44ceb4 + ecx_15 + 1, 
                                        *(ebx_5 + 0x10) - ecx_15 - 1, 
                                        *(ebx_5 + 8) + (edx_12 << 0x18), edx_12 >> 8, ebp_2 + 8);
                                }
                                else
                                {
                                    void* edx_11 = data_44cec0 + edi_1;
                                    int32_t ecx_16 = *(edx_11 + 0xc);
                                    sub_407fc0(data_453054 + data_44ceb4, 
                                        *(edx_11 + 0x10) - data_453054, 
                                        *(edx_11 + 8) + (ecx_16 << 0x18), ecx_16 >> 8);
                                }
                                
                                edi_1 += 0xc;
                                i_2 += 1;
                                data_44ceb4 += data_45305c;
                            } while (*(data_44cebc + 4) > i_2);
                        }
                        
                        int32_t i_3 = 0;
                        eax = data_44cec4;
                        
                        if (*(eax + 4) > 0)
                        {
                            int32_t esi_2 = 0;
                            int32_t ebx_6 = i_2 * 0xc;
                            void* eax_60;
                            
                            do
                            {
                                void* edx_15 = data_44cec4 + esi_2;
                                int32_t ecx_18 = *(edx_15 + 0x10);
                                
                                if (data_453054 <= ecx_18)
                                {
                                    void* ebp_5 = data_44cec0 + ebx_6;
                                    int32_t eax_51 = *(ebp_5 + 0xc);
                                    sub_407fa0(data_44ceb4 + ecx_18 + 1, 
                                        *(ebp_5 + 0x10) - ecx_18 - 1, 
                                        *(ebp_5 + 8) + (eax_51 << 0x18), eax_51 >> 8, edx_15 + 8);
                                }
                                else
                                {
                                    void* ecx_20 = data_44cec0 + ebx_6;
                                    int32_t edx_16 = *(ecx_20 + 0xc);
                                    sub_407fc0(data_453054 + data_44ceb4, 
                                        *(ecx_20 + 0x10) - data_453054, 
                                        *(ecx_20 + 8) + (edx_16 << 0x18), edx_16 >> 8);
                                }
                                
                                ebx_6 += 0xc;
                                data_44ceb4 += data_45305c;
                                eax_60 = data_44cec4;
                                esi_2 += 0xc;
                                i_3 += 1;
                            } while (*(eax_60 + 4) > i_3);
                            
                            return eax_60;
                        }
                    }
                }
            }
        }
    }
    
    return eax;
}

int32_t sub_408d70(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4) __pure
{
    int32_t result;
    
    if (arg4 != arg2)
        result = ((arg1 - arg3) << 8) / (arg2 - arg4);
    else
    {
        result = 0x80010000;
        
        if (arg1 <= arg3)
            return 0x7fff0000;
    }
    
    return result;
}

int32_t __convention("regparm") sub_408da8(int32_t arg1)
{
    int32_t __saved_ebx;
    int32_t* var_14 = &__saved_ebx;
    sub_407fe0();
    sub_407190();
    return arg1;
}

int32_t __convention("regparm") sub_408dc9(int32_t arg1, void* arg2 @ esi)
{
    int32_t __saved_ebx;
    int32_t* var_14 = &__saved_ebx;
    void* var_1c = arg2;
    
    if (*(arg2 + 0xc))
    {
        int32_t* ebx = *(arg2 + 4);
        data_41ca74 = *ebx;
        data_41ca78 = ebx[1];
        data_41ca6c = *(arg2 + 8);
        data_41ca70 = *(arg2 + 0xc);
        sub_4091f8(0x41ca68);
        return arg1;
    }
    
    int32_t* eax_5 = *(arg2 + 8);
    eax_5[3];
    int32_t ebx_1;
    ebx_1 = *(eax_5 + 9);
    data_41ca5c = ebx_1 + eax_5[6];
    data_41ca60 = (eax_5[4] >> 8) - (eax_5[2] >> 8);
    data_41ca64 = (eax_5[5] >> 8) - (eax_5[3] >> 8);
    int32_t* edx = *(arg2 + 4);
    int32_t ebx_11 = (*edx - *eax_5) >> 8;
    int32_t ecx_6 = (edx[1] - eax_5[1]) >> 8;
    
    if (ecx_6 >= data_45304c)
        goto label_408e78;
    
    int32_t ecx_8 = -(ecx_6) + data_45304c;
    int32_t temp2_1 = data_41ca64;
    data_41ca64 -= ecx_8;
    
    if (temp2_1 > ecx_8)
    {
        data_41ca5c += ecx_8 << 8;
        ecx_6 = data_45304c;
    label_408e78:
        int32_t edx_2 = ecx_6 + data_41ca64;
        int32_t edx_3 = edx_2 - data_453048;
        int32_t temp3_1;
        
        if (edx_2 > data_453048)
        {
            temp3_1 = data_41ca64;
            data_41ca64 -= edx_3;
        }
        
        if (edx_2 <= data_453048 || temp3_1 > edx_3)
        {
            if (ebx_11 >= data_453054)
                goto label_408ebc;
            
            int32_t ebx_13 = -(ebx_11) + data_453054;
            int32_t temp6_1 = data_41ca60;
            data_41ca60 -= ebx_13;
            
            if (temp6_1 > ebx_13)
            {
                data_41ca5c += ebx_13;
                ebx_11 = data_453054;
            label_408ebc:
                int32_t edx_5 = ebx_11 + data_41ca60;
                int32_t edx_6 = edx_5 - data_453050;
                int32_t temp7_1;
                
                if (edx_5 > data_453050)
                {
                    temp7_1 = data_41ca60;
                    data_41ca60 -= edx_6;
                }
                
                if (edx_5 <= data_453050 || temp7_1 > edx_6)
                {
                    char* edi_1 = ebx_11 + ecx_6 * data_45305c + data_453068;
                    *(*(arg2 + 8) + 0x1c);
                    int32_t i_1 = data_41ca64;
                    char* esi_1 = data_41ca5c;
                    int32_t ecx_11 = data_41ca60;
                    int32_t i;
                    
                    do
                    {
                        int32_t ecx_12 = ecx_11 - 6;
                        int32_t eax_6;
                        char* edx_7;
                        char* ebx_16;
                        
                        if (ecx_11 >= 6)
                        {
                            ebx_16 = edi_1[ecx_12 + 5];
                            *edx_7[1] = esi_1[ecx_12 + 4];
                            *ebx_16[1] = esi_1[ecx_12 + 5];
                            edx_7 = edi_1[ecx_12 + 4];
                            *eax_6[1] = *ebx_16;
                            int32_t temp11_1;
                            
                            do
                            {
                                ebx_16 = edi_1[ecx_12 + 3];
                                eax_6 = *edx_7;
                                *ebx_16[1] = esi_1[ecx_12 + 3];
                                edx_7 = edi_1[ecx_12 + 2];
                                *edx_7[1] = esi_1[ecx_12 + 2];
                                *eax_6[1] = *ebx_16;
                                ebx_16 = edi_1[ecx_12 + 1];
                                eax_6 = *edx_7;
                                edx_7 = edi_1[ecx_12];
                                *ebx_16[1] = esi_1[ecx_12 + 1];
                                *(edi_1 + ecx_12 + 2) = eax_6;
                                *edx_7[1] = esi_1[ecx_12];
                                temp11_1 = ecx_12;
                                ecx_12 -= 4;
                                *eax_6[1] = *ebx_16;
                            } while (temp11_1 - 4 >= 0);
                            goto label_408f4a;
                        }
                        
                        if (ecx_12 + 4 >= 0)
                        {
                            ebx_16 = edi_1[ecx_12 + 5];
                            *edx_7[1] = esi_1[ecx_12 + 4];
                            *ebx_16[1] = esi_1[ecx_12 + 5];
                            edx_7 = edi_1[ecx_12 + 4];
                            *eax_6[1] = *ebx_16;
                        label_408f4a:
                            edi_1[ecx_12 + 5] = *eax_6[1];
                            int32_t ecx_13 = ecx_12 + 3;
                            *eax_6[1] = *edx_7;
                            
                            if (ecx_12 + 3 >= 0)
                            {
                                int32_t temp14_1;
                                
                                do
                                {
                                    *edx_7[1] = esi_1[ecx_13];
                                    edi_1[ecx_13 + 1] = *eax_6[1];
                                    edx_7 = edi_1[ecx_13];
                                    temp14_1 = ecx_13;
                                    ecx_13 -= 1;
                                    *eax_6[1] = *edx_7;
                                } while (temp14_1 - 1 >= 0);
                            }
                            
                            *edi_1 = *eax_6[1];
                        }
                        else if (ecx_12 + 5 >= 0)
                        {
                            edx_7 = *edi_1;
                            *edx_7[1] = *esi_1;
                            *eax_6[1] = *edx_7;
                            *edi_1 = *eax_6[1];
                        }
                        
                        esi_1 = &esi_1[0x100];
                        edi_1 = &edi_1[data_45305c];
                        i = i_1;
                        i_1 -= 1;
                        ecx_11 = data_41ca60;
                    } while (i != 1);
                }
            }
        }
    }
    
    return arg1;
}

uint16_t __convention("regparm") sub_408fa4(int32_t arg1, int32_t arg2, int32_t arg3)
{
    data_41caa4 = arg2;
    uint16_t result = -(arg1) >> 8;
    data_41caa0 = arg3;
    int32_t entry_ebx;
    data_41ca98 = RORD(-(entry_ebx), 0x10);
    int32_t ebx_1;
    ebx_1 = result;
    data_41ca9c = ebx_1;
    return result;
}

char* __convention("regparm") sub_408fca(char* arg1, int16_t arg2, int32_t arg3, int32_t arg4 @ esi, char* arg5 @ edi)
{
    data_41caa0;
    char* ebx;
    ebx = arg1[4];
    *ebx[1] = *arg1;
    *arg2[1] = *ebx;
    data_41caa8 = *arg2[1];
    int16_t entry_ebx;
    ebx = entry_ebx;
    arg2 = data_41ca98;
    int32_t ebp_1 = data_41ca9c;
    char* result = data_41caa4;
    int32_t esi = arg4 - 1;
    
    if (arg4 - 1 >= 0)
    {
        int32_t temp2_1;
        
        do
        {
            int32_t temp1_1 = arg3;
            arg3 += ebp_1;
            *ebx[1] = *arg3[1];
            ebx = ebx + arg2;
            result = arg5[esi];
            *result[1] = *ebx;
            *arg2[1] = *result;
            arg5[esi] = *arg2[1];
            temp2_1 = esi;
            esi -= 1;
        } while (temp2_1 - 1 >= 0);
    }
    
    if (esi + 1 >= 0)
    {
        *result[1] = data_41caa8;
        result = arg5[esi];
        *arg2[1] = *result;
        arg5[esi] = *arg2[1];
    }
    
    return result;
}

char* __fastcall sub_40901f(int32_t arg1, int32_t arg2 @ esi, char* arg3 @ edi)
{
    data_41caa0;
    int16_t entry_ebx;
    char* ebx;
    ebx = entry_ebx;
    int16_t edx;
    edx = data_41ca98;
    int32_t ebp_1 = data_41ca9c;
    char* result = data_41caa4;
    int32_t esi = arg2 - 1;
    
    if (arg2 - 1 >= 0)
    {
        int32_t temp2_1;
        
        do
        {
            int32_t temp1_1 = arg1;
            arg1 += ebp_1;
            *ebx[1] = *arg1[1];
            ebx = ebx + edx;
            *result[1] = *ebx;
            result = arg3[esi];
            *edx[1] = *result;
            arg3[esi] = *edx[1];
            temp2_1 = esi;
            esi -= 1;
        } while (temp2_1 - 1 >= 0);
    }
    
    return result;
}

void __fastcall sub_409054(void* arg1, int32_t arg2, int32_t arg3 @ ebp, int32_t arg4 @ esi, char* arg5 @ edi) __noreturn
{
    void* ecx_4 = arg1 + arg3;
    char* ebx;
    *ebx[1] = *ecx_4[1];
    ebx = ebx + arg2;
    char* eax;
    eax = *ebx;
    *eax[1] = ebx[0x10000];
    *arg2[1] = *eax;
    arg5[arg4] = *arg2[1];
    void* ecx = ecx_4 + eax;
    *ebx[1] = *ecx[1];
    ebx = ebx + arg2;
    *arg2[1] = *ebx;
    void* ecx_1 = ecx + eax;
    bool c_1 = ecx + eax < ecx;
    int32_t ebx_1;
    *ebx_1[1] = *(ebx + arg3)[1] + arg2;
    arg5 = ebx_1;
    *arg2[1] = *arg5;
    void* ecx_2 = ecx_1 + eax;
    bool c_3 = ecx_1 + eax < ecx_1;
    char* ebx_2;
    *ebx_2[1] = *(ebx_1 + arg3)[1] + arg2;
    arg5 = ebx_2;
    *arg2[1] = *arg5;
    *ebx_2[1] = *(ecx_2 + eax)[1];
    ebx_2 = ebx_2 + arg2;
    *arg2[1] = *ebx_2;
    bool c_6 = arg4 >= 0xffffff85;
    arg5 = ebx_2;
    *arg2[1] = *arg5;
    int16_t ebx_3;
    *ebx_3[1] = *(ebx_2 + 0x4d2)[1] + 4;
    arg5 = ebx_3;
    *arg2[1] = *arg5;
    uint32_t ebx_5;
    ebx_5 = *(arg2 + arg4 + 0x7b)[1];
    eax = *ebx_5;
    arg5 += 0x50;
    breakpoint();
}

uint16_t __convention("regparm") sub_4090b8(int32_t arg1, int32_t, int32_t arg3)
{
    uint16_t result = -(arg1) >> 8;
    data_41cc3c = arg3;
    int32_t entry_ebx;
    data_41cc34 = RORD(-(entry_ebx), 0x10);
    int32_t ebx_1;
    ebx_1 = result;
    data_41cc38 = ebx_1;
    return result;
}

void __convention("regparm") sub_4090d8(char* arg1, int16_t arg2, int32_t arg3, int32_t arg4 @ esi, char* arg5 @ edi)
{
    data_41cc3c;
    char* ebx;
    ebx = arg1[4];
    *ebx[1] = *arg1;
    *arg2[1] = *ebx;
    data_41cc44 = *arg2[1];
    int16_t entry_ebx;
    ebx = entry_ebx;
    arg2 = data_41cc34;
    int32_t ebp_1 = data_41cc38;
    int32_t esi = arg4 - 4;
    
    if (arg4 - 4 >= 0)
    {
        int32_t temp6_1;
        
        do
        {
            int32_t ecx = arg3 + ebp_1;
            *ebx[1] = *ecx[1];
            ebx = ebx + arg2;
            arg1 = *ebx;
            int32_t ecx_1 = ecx + ebp_1;
            *ebx[1] = *ecx_1[1];
            ebx = ebx + arg2;
            int32_t ecx_2 = ecx_1 + ebp_1;
            *arg1[1] = *ebx;
            *ebx[1] = *ecx_2[1];
            ebx = ebx + arg2;
            _bswap(arg1);
            arg3 = ecx_2 + ebp_1;
            *arg1[1] = *ebx;
            *ebx[1] = *arg3[1];
            ebx = ebx + arg2;
            arg1 = *ebx;
            *(arg5 + esi) = arg1;
            temp6_1 = esi;
            esi -= 4;
        } while (temp6_1 - 4 >= 0);
    }
    
    int32_t esi_1 = esi + 3;
    
    if (esi + 3 >= 0)
    {
        int32_t temp8_1;
        
        do
        {
            int32_t temp7_1 = arg3;
            arg3 += ebp_1;
            *ebx[1] = *arg3[1];
            ebx = ebx + arg2;
            arg1 = *ebx;
            arg5[esi_1] = arg1;
            temp8_1 = esi_1;
            esi_1 -= 1;
        } while (temp8_1 - 1 >= 0);
    }
    
    if (esi_1 + 1 >= 0)
    {
        arg1 = data_41cc44;
        arg5[esi_1] = arg1;
    }
}

void __fastcall sub_40914e(int32_t arg1, int32_t arg2 @ esi, int32_t* arg3 @ edi)
{
    data_41cc3c;
    int16_t entry_ebx;
    char* ebx;
    ebx = entry_ebx;
    char edx = data_41cc34;
    int32_t ebp_1 = data_41cc38;
    int32_t esi = arg2 - 4;
    int32_t eax;
    
    if (arg2 - 4 >= 0)
    {
        int32_t temp6_1;
        
        do
        {
            int32_t ecx = arg1 + ebp_1;
            *ebx[1] = *ecx[1];
            ebx = ebx + edx;
            int32_t ecx_1 = ecx + ebp_1;
            eax = *ebx;
            *ebx[1] = *ecx_1[1];
            ebx = ebx + edx;
            int32_t ecx_2 = ecx_1 + ebp_1;
            *eax[1] = *ebx;
            *ebx[1] = *ecx_2[1];
            ebx = ebx + edx;
            _bswap(eax);
            arg1 = ecx_2 + ebp_1;
            *eax[1] = *ebx;
            *ebx[1] = *arg1[1];
            ebx = ebx + edx;
            eax = *ebx;
            *(arg3 + esi) = eax;
            temp6_1 = esi;
            esi -= 4;
        } while (temp6_1 - 4 >= 0);
    }
    
    int32_t esi_1 = esi + 3;
    
    if (esi + 3 < 0)
        return;
    
    int32_t temp8_1;
    
    do
    {
        int32_t temp7_1 = arg1;
        arg1 += ebp_1;
        *ebx[1] = *arg1[1];
        ebx = ebx + edx;
        eax = *ebx;
        *(arg3 + esi_1) = eax;
        temp8_1 = esi_1;
        esi_1 -= 1;
    } while (temp8_1 - 1 >= 0);
}

int32_t __convention("regparm") sub_4091aa(int32_t arg1, void* arg2, int32_t arg3, int32_t arg4 @ ebp, void* arg5 @ esi)
{
    int32_t ecx_4 = arg3 + arg1;
    char* ebx;
    *ebx[1] = *ecx_4[1];
    ebx = ebx + arg2;
    *arg2[1] = *ebx;
    int32_t ecx = ecx_4 + arg1;
    bool c = ecx_4 + arg1 < ecx_4;
    int32_t ebx_1;
    *ebx_1[1] = *(ebx + arg4)[1] + arg2;
    char* edi;
    edi = ebx_1;
    *arg2[1] = *edi;
    int32_t ecx_1 = ecx + arg1;
    bool c_2 = ecx + arg1 < ecx;
    char* ebx_2;
    *ebx_2[1] = *(ebx_1 + arg4)[1] + arg2;
    edi = ebx_2;
    *arg2[1] = *edi;
    *ebx_2[1] = *(ecx_1 + arg1)[1];
    ebx_2 = ebx_2 + arg2;
    *arg2[1] = *ebx_2;
    bool c_5 = arg5 >= 0xffffff85;
    edi = ebx_2;
    *arg2[1] = *edi;
    int16_t ebx_3;
    *ebx_3[1] = *(ebx_2 + 0x4d2)[1] + 4;
    edi = ebx_3;
    *arg2[1] = *edi;
    uint32_t ebx_5;
    ebx_5 = *(arg2 + arg5 + 0x7b)[1];
    arg1 = *ebx_5;
    edi += 0x50;
    /* tailcall */
    return sub_4091f8(arg5 + 0x7b);
}

int32_t sub_4091f8(void* arg1 @ esi)
{
    int32_t* ebp = *(arg1 + 4);
    data_41cc68 = ebp[3];
    data_41cc6c = ebp[5] - 0x10;
    data_41cc70 = ebp[2];
    data_41cc74 = ebp[4] - 0x10;
    data_41cc78 = ebp[7];
    data_41cc7c = ebp[6];
    int32_t* ecx = *(arg1 + 8);
    int32_t eax_10;
    int16_t edx;
    edx = HIGHD(-(*ebp) * *ecx);
    eax_10 = LOWD(-(*ebp) * *ecx);
    eax_10 = edx;
    data_41cc48 = ROLD(eax_10, 0x10);
    int32_t eax_14;
    int16_t edx_1;
    edx_1 = HIGHD(-(ebp[1]) * ecx[2]);
    eax_14 = LOWD(-(ebp[1]) * ecx[2]);
    eax_14 = edx_1;
    data_41cc48 += ROLD(eax_14, 0x10);
    int32_t eax_18;
    int16_t edx_2;
    edx_2 = HIGHD(-(*ebp) * ecx[1]);
    eax_18 = LOWD(-(*ebp) * ecx[1]);
    eax_18 = edx_2;
    data_41cc4c = ROLD(eax_18, 0x10);
    int32_t eax_22;
    int16_t edx_3;
    edx_3 = HIGHD(-(ebp[1]) * ecx[3]);
    eax_22 = LOWD(-(ebp[1]) * ecx[3]);
    eax_22 = edx_3;
    data_41cc4c += ROLD(eax_22, 0x10);
    int32_t ebx_6 = ebp[4] - 0x10 - ebp[2];
    int32_t eax_25;
    int16_t edx_4;
    edx_4 = HIGHD(ecx[1] * ebx_6);
    eax_25 = LOWD(ecx[1] * ebx_6);
    eax_25 = edx_4;
    data_41cc54 = ROLD(eax_25, 0x10);
    int32_t eax_28;
    int16_t edx_5;
    edx_5 = HIGHD(*ecx * ebx_6);
    eax_28 = LOWD(*ecx * ebx_6);
    eax_28 = edx_5;
    data_41cc50 = ROLD(eax_28, 0x10);
    int32_t ebx_9 = ebp[5] - 0x10 - ebp[3];
    int32_t eax_31;
    int16_t edx_6;
    edx_6 = HIGHD(ecx[3] * ebx_9);
    eax_31 = LOWD(ecx[3] * ebx_9);
    eax_31 = edx_6;
    int32_t eax_33 = ROLD(eax_31, 0x10) + data_41cc4c;
    data_41cc5c = eax_33;
    data_41cc64 = eax_33 + data_41cc54;
    data_41cc54 += data_41cc4c;
    int32_t eax_38;
    int16_t edx_7;
    edx_7 = HIGHD(ecx[2] * ebx_9);
    eax_38 = LOWD(ecx[2] * ebx_9);
    eax_38 = edx_7;
    int32_t eax_40 = ROLD(eax_38, 0x10) + data_41cc48;
    data_41cc58 = eax_40;
    data_41cc60 = eax_40 + data_41cc50;
    data_41cc50 += data_41cc48;
    data_41cc48 += *(arg1 + 0xc);
    data_41cc4c += *(arg1 + 0x10);
    data_41cc50 += *(arg1 + 0xc);
    data_41cc54 += *(arg1 + 0x10);
    data_41cc58 += *(arg1 + 0xc);
    data_41cc5c += *(arg1 + 0x10);
    data_41cc60 += *(arg1 + 0xc);
    data_41cc64 += *(arg1 + 0x10);
    data_41cc84 = data_41cc48;
    data_41cc88 = data_41cc4c;
    data_41cc8c = data_41cc50;
    data_41cc90 = data_41cc54;
    data_41cc94 = data_41cc58;
    data_41cc98 = data_41cc5c;
    int32_t eax_66 = data_41cc68;
    data_41cca0 = eax_66;
    data_41cca8 = eax_66;
    data_41ccb0 = data_41cc6c;
    int32_t eax_68 = data_41cc70;
    data_41cc9c = eax_68;
    data_41ccac = eax_68;
    data_41cca4 = data_41cc74;
    data_41ccb4 = data_41cc7c;
    data_41ccb8 = data_41cc78;
    sub_4071f0(0x41cc80);
    data_41cc84 = data_41cc50;
    data_41cc88 = data_41cc54;
    data_41cc8c = data_41cc58;
    data_41cc90 = data_41cc5c;
    data_41cc94 = data_41cc60;
    data_41cc98 = data_41cc64;
    data_41cca0 = data_41cc68;
    int32_t eax_79 = data_41cc6c;
    data_41cca8 = eax_79;
    data_41ccb0 = eax_79;
    int32_t eax_80 = data_41cc74;
    data_41cc9c = eax_80;
    data_41ccac = eax_80;
    data_41cca4 = data_41cc70;
    data_41ccb4 = data_41cc7c;
    data_41ccb8 = data_41cc78;
    sub_4071f0(0x41cc80);
    return 0x41cc80;
}

HRESULT __stdcall DirectDrawCreate(GUID* lpGUID, struct IDirectDraw* lplpDD, struct IUnknown pUnkOuter)
{
    /* tailcall */
    return DirectDrawCreate(lpGUID, lplpDD, pUnkOuter);
}

HRESULT __stdcall DirectSoundCreate(GUID* pcGuidDevice, struct IDirectSound* ppDS, struct IUnknown pUnkOuter)
{
    /* tailcall */
    return DirectSoundCreate(pcGuidDevice, ppDS, pUnkOuter);
}

int32_t DirectInputCreateA()
{
    /* tailcall */
    return DirectInputCreateA();
}

int32_t __convention("regparm") sub_40a4a0(int32_t arg1, int32_t arg2, int32_t arg3)
{
    int32_t var_4 = arg1;
    int32_t var_8 = arg3;
    int32_t var_c = arg2;
    void** entry_ebx;
    void** var_10 = entry_ebx;
    int32_t* var_14 = &var_10;
    int32_t ebp;
    int32_t var_18 = ebp;
    int32_t esi;
    int32_t var_1c = esi;
    int32_t edi;
    int32_t var_20 = edi;
    
    if (arg1 > 0x578 || entry_ebx > 0x258)
        return 0xffffffff;
    
    data_41cdf0 = arg1;
    data_41cdf4 = entry_ebx;
    int16_t var_28 = arg3;
    int32_t var_2c = arg2;
    void** var_30 = entry_ebx;
    int32_t* var_34 = &var_30;
    int32_t var_38 = ebp;
    int32_t var_40 = edi;
    void* edi_1 = &data_41d75c;
    
    for (int32_t i = 1; i != 0x3a9b; i += 1)
    {
        *edi_1 = COMBINE(0, 0x10000) / i - 1;
        edi_1 += 4;
    }
    
    void** edi_2 = &data_42c21c;
    
    while (*edi_2)
    {
        var_30 = edi_2;
        (*edi_2)(var_30);
        edi_2 = &var_30[1];
    }
    
    void* edi_4 = &data_41cdf8;
    int32_t ebx_1 = 0;
    int16_t i_2 = 0x258;
    int16_t i_1;
    
    do
    {
        *edi_4 = ebx_1;
        edi_4 += 4;
        ebx_1 += arg1;
        i_1 = i_2;
        i_2 -= 1;
    } while (i_1 != 1);
    return 0;
}

int32_t sub_40a519(int32_t* arg1 @ esi)
{
    int32_t __saved_ebx;
    int32_t* var_14 = &__saved_ebx;
    int32_t* var_1c = arg1;
    data_41cd98 = arg1;
    int32_t* edi = *arg1;
    data_41cd9c = edi;
    int32_t ebx = arg1[3];
    data_41cda4 = arg1[1];
    data_41cdac = ebx;
    int32_t eax_1 = arg1[4];
    int32_t ebx_1 = arg1[5];
    data_41cdb0 = eax_1;
    data_41cdb4 = ebx_1;
    data_41cdc0 = eax_1;
    int32_t eax_2 = eax_1 << 8;
    data_41cdc4 = ebx_1;
    int32_t ebx_2 = ebx_1 << 8;
    data_41cdd0 = eax_2;
    data_41cdd4 = ebx_2;
    data_41cde0 = eax_2;
    data_41cde4 = ebx_2;
    int32_t eax_3 = arg1[6];
    int32_t ebx_3 = arg1[7];
    data_41cdb8 = eax_3;
    data_41cdbc = ebx_3;
    data_41cdc8 = eax_3 + 1;
    int32_t eax_5 = (eax_3 + 1) << 8;
    data_41cdcc = ebx_3 + 1;
    data_41cde8 = eax_5;
    int32_t ebx_5 = (ebx_3 + 1) << 8;
    data_41cdd8 = eax_5 - 1;
    data_41cdec = ebx_5;
    data_41cddc = ebx_5 - 1;
    int32_t* esi = *edi;
    data_41cda0 = esi;
    
    if (!esi)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi] */
}

int32_t __stdcall sub_40a5ca(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5) __pure
{
    return 0xffffffff;
}

void __convention("regparm") sub_40a608(int32_t arg1)
{
    data_43012c = arg1 + 1;
    data_430130 = -(arg1) + 1;
}

int32_t __stdcall sub_40a61f(void* arg1 @ esi, int32_t* arg2 @ edi, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t eax = *(arg1 + 4) >> 8;
    int32_t ecx_1 = *(arg1 + 0x10) >> 8;
    int32_t ebx = *(arg1 + 0x20);
    int32_t edx = *(arg1 + 0x24);
    
    if (eax > ecx_1)
    {
        int32_t temp0_1 = edx;
        edx = ebx;
        ebx = temp0_1;
    }
    
    data_42c258 = edx - ebx;
    data_43013c = ebx;
    data_42c254 = *(arg1 + 0x1c);
    int32_t ebx_3 = *(arg1 + 8) >> 8;
    int32_t edx_3 = *(arg1 + 0x14) >> 8;
    
    if (eax > ecx_1)
    {
        int32_t temp0_2 = ecx_1;
        ecx_1 = eax;
        eax = temp0_2;
        int32_t temp0_3 = edx_3;
        edx_3 = ebx_3;
        ebx_3 = temp0_3;
    }
    
    if (eax >= data_41cdb0)
        goto label_40a66d;
    
    if (ecx_1 >= data_41cdb0)
    {
        data_42c240 = eax;
        data_42c244 = ebx_3;
        data_42c248 = ecx_1;
        data_42c24c = edx_3;
        data_42c250 =
            (COMBINE(data_42c248 - data_41cdb0 + 1, 0) / (data_42c248 - data_42c240 + 1)) >> 1;
        int32_t eax_32 = data_42c24c - data_42c244;
        int32_t eax_34;
        int32_t edx_29;
        edx_29 = HIGHD(eax_32 * 2 * data_42c250);
        eax_34 = LOWD(eax_32 * 2 * data_42c250);
        data_42c244 += eax_32 - edx_29;
        data_42c240 = data_41cdb0;
        int32_t ebx_16 = data_43013c + data_42c258;
        int32_t eax_38;
        int32_t edx_30;
        edx_30 = HIGHD((data_42c258 << 1) * data_42c250);
        eax_38 = LOWD((data_42c258 << 1) * data_42c250);
        data_42c258 = edx_30;
        data_43013c = ebx_16 - edx_30;
        eax = data_42c240;
        ebx_3 = data_42c244;
        ecx_1 = data_42c248;
        edx_3 = data_42c24c;
    label_40a66d:
        
        if (ecx_1 <= data_41cdb8)
            goto label_40a679;
        
        if (eax <= data_41cdb8)
        {
            data_42c240 = eax;
            data_42c244 = ebx_3;
            data_42c248 = ecx_1;
            data_42c24c = edx_3;
            data_42c250 =
                (COMBINE(data_41cdb8 - data_42c240 + 1, 0) / (data_42c248 - data_42c240 + 1)) >> 1;
            int32_t eax_23;
            int32_t edx_23;
            edx_23 = HIGHD((data_42c24c - data_42c244) * 2 * data_42c250);
            eax_23 = LOWD((data_42c24c - data_42c244) * 2 * data_42c250);
            data_42c24c = edx_23 + data_42c244;
            data_42c248 = data_41cdb8;
            int32_t eax_27;
            int32_t edx_25;
            edx_25 = HIGHD((data_42c258 << 1) * data_42c250);
            eax_27 = LOWD((data_42c258 << 1) * data_42c250);
            data_42c258 = edx_25;
            eax = data_42c240;
            ebx_3 = data_42c244;
            ecx_1 = data_42c248;
            edx_3 = data_42c24c;
        label_40a679:
            
            if (ebx_3 > edx_3)
            {
                if (ebx_3 <= data_41cdbc)
                    goto label_40a6a2;
                
                if (edx_3 <= data_41cdbc)
                {
                    data_42c240 = eax;
                    data_42c244 = ebx_3;
                    data_42c248 = ecx_1;
                    data_42c24c = edx_3;
                    data_42c250 = (COMBINE(data_41cdbc - data_42c24c + 1, 0)
                        / (data_42c244 - data_42c24c + 1)) >> 1;
                    int32_t eax_56;
                    int32_t edx_40;
                    edx_40 = HIGHD((data_42c248 - data_42c240) * 2 * data_42c250);
                    eax_56 = LOWD((data_42c248 - data_42c240) * 2 * data_42c250);
                    data_42c240 = -(edx_40) + data_42c248;
                    data_42c244 = data_41cdbc;
                    int32_t ebx_25 = data_43013c + data_42c258;
                    int32_t eax_60;
                    int32_t edx_43;
                    edx_43 = HIGHD((data_42c258 << 1) * data_42c250);
                    eax_60 = LOWD((data_42c258 << 1) * data_42c250);
                    data_42c258 = edx_43;
                    data_43013c = ebx_25 - edx_43;
                    eax = data_42c240;
                    ebx_3 = data_42c244;
                    ecx_1 = data_42c248;
                    edx_3 = data_42c24c;
                label_40a6a2:
                    
                    if (edx_3 >= data_41cdb4)
                        goto label_40a6ae;
                    
                    if (ebx_3 >= data_41cdb4)
                    {
                        data_42c240 = eax;
                        data_42c244 = ebx_3;
                        data_42c248 = ecx_1;
                        data_42c24c = edx_3;
                        data_42c250 = (COMBINE(data_42c244 - data_41cdb4 + 1, 0)
                            / (data_42c244 - data_42c24c + 1)) >> 1;
                        int32_t eax_45;
                        int32_t edx_34;
                        edx_34 = HIGHD((data_42c248 - data_42c240) * 2 * data_42c250);
                        eax_45 = LOWD((data_42c248 - data_42c240) * 2 * data_42c250);
                        data_42c248 = edx_34 + data_42c240;
                        data_42c24c = data_41cdb4;
                        int32_t eax_49;
                        int32_t edx_36;
                        edx_36 = HIGHD((data_42c258 << 1) * data_42c250);
                        eax_49 = LOWD((data_42c258 << 1) * data_42c250);
                        data_42c258 = edx_36;
                        eax = data_42c240;
                        ebx_3 = data_42c244;
                        ecx_1 = data_42c248;
                        edx_3 = data_42c24c;
                    label_40a6ae:
                        *((ebx_3 << 2) + &data_41cdf8);
                        data_41cda4;
                        int32_t edx_4 = edx_3 - ebx_3;
                        
                        if (edx_4 < 0)
                        {
                            if (ecx_1 - eax + 1 > -(edx_4) + 1)
                            {
                                data_430136 = (-(edx_4) + 1);
                                data_43013a = data_42c258 / (ecx_1 - eax + 1);
                                uint16_t eax_16 = (ecx_1 - eax + 1);
                                int16_t temp2_7 = (-(edx_4) + 1);
                                data_430138 = COMBINE(0, eax_16) / temp2_7;
                                data_430134 =
                                    COMBINE(COMBINE(0, eax_16) % temp2_7, 0) / (-(edx_4) + 1);
                                int32_t ebx_6;
                                ebx_6 = data_430138;
                                data_42c254;
                                /* jump -> *((ebx_6 << 2) + &data_42eb38) */
                            }
                            
                            data_43013a = data_42c258 / (-(edx_4) + 1);
                            uint16_t eax_12 = (-(edx_4) + 1);
                            int16_t temp2_5 = (ecx_1 - eax + 1);
                            data_430138 = COMBINE(0, eax_12) / temp2_5;
                            data_430136 = (ecx_1 - eax + 1);
                            data_430134 =
                                COMBINE(COMBINE(0, eax_12) % temp2_5, 0) / (ecx_1 - eax + 1);
                            int32_t ebx_5;
                            ebx_5 = data_430138;
                            data_42c254;
                            /* jump -> *((ebx_5 << 2) + &data_42e1c4) */
                        }
                        
                        if (ecx_1 - eax + 1 <= edx_4 + 1)
                        {
                            data_43013a = data_42c258 / (edx_4 + 1);
                            uint16_t eax_4 = (edx_4 + 1);
                            int16_t temp2_1 = (ecx_1 - eax + 1);
                            data_430138 = COMBINE(0, eax_4) / temp2_1;
                            data_430136 = (ecx_1 - eax + 1);
                            data_430134 =
                                COMBINE(COMBINE(0, eax_4) % temp2_1, 0) / (ecx_1 - eax + 1);
                            int32_t ebx_4;
                            ebx_4 = data_430138;
                            data_42c254;
                            /* jump -> *((ebx_4 << 2) + &data_42c25c) */
                        }
                        
                        data_430136 = (edx_4 + 1);
                        ebx_3 = (edx_4 + 1);
                        data_43013a = data_42c258 / (ecx_1 - eax + 1);
                        uint16_t eax_8 = (ecx_1 - eax + 1);
                        int16_t temp2_3 = ebx_3;
                        data_430138 = COMBINE(0, eax_8) / temp2_3;
                        data_430134 = COMBINE(COMBINE(0, eax_8) % temp2_3, 0) / ebx_3;
                        ebx_3 = data_430138;
                        data_42c254;
                        /* jump -> *((ebx_3 << 2) + &data_42cbd0) */
                    }
                }
            }
            else
            {
                if (edx_3 <= data_41cdbc)
                    goto label_40a689;
                
                if (ebx_3 <= data_41cdbc)
                {
                    data_42c240 = eax;
                    data_42c244 = ebx_3;
                    data_42c248 = ecx_1;
                    data_42c24c = edx_3;
                    data_42c250 = (COMBINE(data_41cdbc - data_42c244 + 1, 0)
                        / (data_42c24c - data_42c244 + 1)) >> 1;
                    int32_t eax_67;
                    int32_t edx_47;
                    edx_47 = HIGHD((data_42c248 - data_42c240) * 2 * data_42c250);
                    eax_67 = LOWD((data_42c248 - data_42c240) * 2 * data_42c250);
                    data_42c248 = edx_47 + data_42c240;
                    data_42c24c = data_41cdbc;
                    int32_t eax_71;
                    int32_t edx_49;
                    edx_49 = HIGHD((data_42c258 << 1) * data_42c250);
                    eax_71 = LOWD((data_42c258 << 1) * data_42c250);
                    data_42c258 = edx_49;
                    eax = data_42c240;
                    ebx_3 = data_42c244;
                    ecx_1 = data_42c248;
                    edx_3 = data_42c24c;
                label_40a689:
                    
                    if (ebx_3 >= data_41cdb4)
                        goto label_40a6ae;
                    
                    if (edx_3 >= data_41cdb4)
                    {
                        data_42c240 = eax;
                        data_42c244 = ebx_3;
                        data_42c248 = ecx_1;
                        data_42c24c = edx_3;
                        data_42c250 = (COMBINE(data_42c24c - data_41cdb4 + 1, 0)
                            / (data_42c24c - data_42c244 + 1)) >> 1;
                        int32_t eax_78;
                        int32_t edx_53;
                        edx_53 = HIGHD((data_42c248 - data_42c240) * 2 * data_42c250);
                        eax_78 = LOWD((data_42c248 - data_42c240) * 2 * data_42c250);
                        data_42c240 = -(edx_53) + data_42c248;
                        data_42c244 = data_41cdb4;
                        int32_t ebx_34 = data_43013c + data_42c258;
                        int32_t eax_82;
                        int32_t edx_56;
                        edx_56 = HIGHD((data_42c258 << 1) * data_42c250);
                        eax_82 = LOWD((data_42c258 << 1) * data_42c250);
                        data_42c258 = edx_56;
                        data_43013c = ebx_34 - edx_56;
                        eax = data_42c240;
                        ebx_3 = data_42c244;
                        ecx_1 = data_42c248;
                        edx_3 = data_42c24c;
                        goto label_40a6ae;
                    }
                }
            }
        }
    }
    
    int32_t* esi_3 = *arg2;
    data_41cda0 = esi_3;
    
    if (!esi_3)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi_3] */
}

int32_t* sub_40b390(int32_t* arg1)
{
    int32_t ecx = arg1[1];
    int32_t eax = arg1[3];
    int32_t eax_1;
    
    if (ecx <= eax)
    {
        data_452714 = eax;
        eax_1 = arg1[1];
    }
    else
    {
        data_452714 = ecx;
        eax_1 = arg1[3];
    }
    
    int32_t ecx_1 = data_452714;
    data_45270c = eax_1;
    int32_t eax_2 = arg1[5];
    
    if (ecx_1 < eax_2)
        data_452714 = eax_2;
    else if (data_45270c > eax_2)
        data_45270c = eax_2;
    
    data_452710 = 1;
    data_4526fc = 3;
    data_452704 = 5;
    
    if (arg1[6] < arg1[2])
    {
        data_452710 = 5;
        data_452704 = 1;
    }
    
    int32_t eax_3 = data_452710;
    int32_t ecx_3 = arg1[4];
    
    if (arg1[eax_3 + 1] <= ecx_3)
    {
        int32_t eax_4 = data_452704;
        
        if (arg1[eax_4 + 1] < ecx_3)
        {
            data_452704 = 3;
            data_4526fc = eax_4;
        }
    }
    else
    {
        data_452710 = 3;
        data_4526fc = eax_3;
    }
    
    int32_t esi_2 = data_452710 << 2;
    int32_t ecx_5 = data_452704 << 2;
    int32_t edi = arg1[data_452710 + 1];
    int32_t eax_5 = arg1[data_452704 + 1];
    
    if (((data_45270c - data_41cde0) | (data_41cdec - eax_5) | (data_41cde8 - data_452714)
        | (edi - data_41cde4)) >= 0)
    {
        int32_t edi_2 = data_4526fc << 2;
        data_43a914 = arg1[data_452710];
        data_43a918 = arg1[data_452710 + 1];
        data_43a91c = arg1[data_4526fc];
        data_43a920 = arg1[data_4526fc + 1];
        data_43a924 = arg1[data_452704];
        data_43a928 = arg1[data_452704 + 1];
        int32_t edx_13 = arg1[7];
        data_43a92c = *(esi_2 + edx_13 - 4);
        data_43a930 = *(esi_2 + edx_13);
        int32_t esi_3 = *(edi_2 + edx_13 - 4);
        data_43a934 = esi_3;
        data_43a938 = *(edi_2 + edx_13);
        int32_t eax_11 = data_41cda4;
        int32_t ebx_13 = *(ecx_5 + edx_13 - 4);
        data_43a93c = ebx_13;
        int32_t ecx_6 = *(ecx_5 + edx_13);
        data_43a940 = ecx_6;
        data_452708 = eax_11;
        int32_t var_80_1 = eax_11;
        int32_t var_84_1 = ebx_13;
        int32_t var_88_1 = ecx_6;
        int32_t var_8c_1 = edx_13;
        sub_456b20();
        int32_t ebx_14 = data_43a960;
        data_43a94c = arg1[8];
        int32_t eax_13 = data_43a914;
        int32_t edx_15 = data_43a91c;
        data_43a910 = arg1[9];
        data_4526d8 = eax_13;
        int32_t ecx_8 = data_43a918;
        int32_t eax_14 = data_43a920;
        data_4526dc = ecx_8;
        data_4526e0 = edx_15;
        data_4526e4 = eax_14;
        
        if (ebx_14 <= 0)
        {
            int32_t edx_24 = data_43a930;
            int32_t eax_34 = data_43a934;
            data_4526e8 = data_43a92c;
            int32_t ecx_17 = data_43a938;
            data_4526ec = edx_24;
            data_4526f0 = eax_34;
            data_4526f4 = ecx_17;
            int32_t var_80_7 = eax_34;
            int32_t var_88_7 = ecx_17;
            data_4534d4;
            sub_456c70(&data_4526d8);
            int32_t eax_35 = data_43a924;
            int32_t ecx_18 = data_43a928;
            data_4526e0 = eax_35;
            data_4526e4 = ecx_18;
            int32_t var_80_8 = eax_35;
            int32_t var_88_8 = ecx_18;
            int32_t var_8c_8 = edx_24;
            data_4534d0;
            sub_456cb0(&data_4526d8);
            void* edx_26 = data_4534d4;
            int32_t ecx_20 = data_452708 + **&data_4534d0 * data_41cdf0;
            data_452708 = ecx_20;
            int32_t eax_39 = *(edx_26 + 4);
            int32_t* eax_42 = data_4534d0 + 8;
            int32_t* var_80_9 = eax_42;
            int32_t var_88_9 = ecx_20;
            void* var_8c_9 = edx_26;
            void* edi_12 = data_452708;
            sub_410cbe(eax_42, eax_39, edx_26 + 8, edi_12);
            data_452708 = edi_12;
            int32_t ecx_21 = data_43a920;
            int32_t edx_28 = data_43a934;
            data_4526d8 = data_43a91c;
            int32_t eax_45 = data_43a938;
            data_4526dc = ecx_21;
            int32_t ecx_22 = data_43a93c;
            data_4526e8 = edx_28;
            int32_t edx_29 = data_43a940;
            data_4526ec = eax_45;
            data_4526f0 = ecx_22;
            data_4526f4 = edx_29;
            int32_t var_80_10 = eax_45;
            int32_t var_88_10 = ecx_22;
            int32_t var_8c_10 = edx_29;
            data_4534d4;
            sub_456c70(&data_4526d8);
            void* eax_46 = data_4534d4;
            int32_t edx_30 = data_4534d0;
            int32_t ecx_23 = *(eax_46 + 4);
            int32_t var_78_11 = edi_2;
            int32_t var_7c_11 = esi_3;
            int32_t var_84_11 = ebx_14;
            int32_t var_88_11 = ecx_23;
            int32_t var_8c_11 = edx_30;
            void* edi_15 = data_452708;
            sub_410cbe(edx_30 + (eax_39 << 2) + 8, ecx_23, eax_46 + 8, edi_15);
            data_452708 = edi_15;
            return edx_30 + (eax_39 << 2) + 8;
        }
        
        int32_t var_80_2 = eax_14;
        int32_t var_88_2 = ecx_8;
        int32_t var_8c_2 = edx_15;
        data_4534d0;
        sub_456cb0(&data_4526d8);
        int32_t ecx_9 = data_43a928;
        int32_t edx_16 = data_43a92c;
        data_4526e0 = data_43a924;
        int32_t eax_16 = data_43a930;
        data_4526e4 = ecx_9;
        int32_t ecx_10 = data_43a93c;
        data_4526e8 = edx_16;
        int32_t edx_17 = data_43a940;
        data_4526ec = eax_16;
        data_4526f0 = ecx_10;
        data_4526f4 = edx_17;
        int32_t var_80_3 = eax_16;
        int32_t var_88_3 = ecx_10;
        int32_t var_8c_3 = edx_17;
        data_4534d4;
        sub_456c70(&data_4526d8);
        void* edx_18 = data_4534d0;
        int32_t ecx_12 = data_452708 + **&data_4534d0 * data_41cdf0;
        data_452708 = ecx_12;
        int32_t eax_20 = *(edx_18 + 4);
        void* var_80_4 = edx_18 + 8;
        int32_t var_88_4 = ecx_12;
        void* edi_6 = data_452708;
        sub_410cbe(edx_18 + 8, eax_20, data_4534d4 + 8, edi_6);
        data_452708 = edi_6;
        int32_t eax_25 = data_43a91c;
        int32_t ecx_13 = data_43a920;
        data_4526d8 = eax_25;
        data_4526dc = ecx_13;
        int32_t var_80_5 = eax_25;
        int32_t var_84_5 = ebx_14;
        int32_t var_88_5 = ecx_13;
        void* var_8c_5 = edx_18;
        data_4534d0;
        sub_456cb0(&data_4526d8);
        int32_t ebx_21 = data_4534d4;
        int32_t ecx_14 = *(data_4534d0 + 4);
        int32_t* eax_31 = data_4534d0 + 8;
        int32_t var_78_6 = edi_2;
        int32_t var_7c_6 = esi_3;
        int32_t var_84_6 = ebx_21;
        int32_t var_88_6 = ecx_14;
        int32_t var_8c_6 = eax_20;
        void* edi_9 = data_452708;
        sub_410cbe(eax_31, ecx_14, ebx_21 + eax_20 * 0xc + 8, edi_9);
        data_452708 = edi_9;
        return eax_31;
    }
    
    int32_t* eax_55 = data_452714 - data_41cde0;
    
    if (((data_41cde8 - data_45270c) | (data_41cdec - edi) | (eax_5 - data_41cde4) | eax_55) >= 0)
    {
        int32_t edi_18 = data_4526fc << 2;
        data_43a914 = arg1[data_452710];
        data_43a918 = arg1[data_452710 + 1];
        data_43a91c = arg1[data_4526fc];
        data_43a920 = arg1[data_4526fc + 1];
        data_43a924 = arg1[data_452704];
        data_43a928 = arg1[data_452704 + 1];
        int32_t edx_39 = arg1[7];
        data_43a92c = *(esi_2 + edx_39 - 4);
        data_43a930 = *(esi_2 + edx_39);
        int32_t esi_17 = *(edi_18 + edx_39 - 4);
        data_43a934 = esi_17;
        data_43a938 = *(edi_18 + edx_39);
        int32_t eax_61 = data_41cda4;
        int32_t ebx_37 = *(ecx_5 + edx_39 - 4);
        data_43a93c = ebx_37;
        int32_t ecx_25 = *(ecx_5 + edx_39);
        data_43a940 = ecx_25;
        data_452708 = eax_61;
        int32_t var_80_12 = eax_61;
        int32_t var_84_12 = ebx_37;
        int32_t var_88_12 = ecx_25;
        int32_t var_8c_12 = edx_39;
        sub_456b20();
        int32_t ebx_38 = data_43a960;
        data_43a94c = arg1[8];
        int32_t eax_63 = data_43a914;
        int32_t edx_41 = data_43a91c;
        data_43a910 = arg1[9];
        data_4526d8 = eax_63;
        int32_t ecx_27 = data_43a918;
        int32_t eax_64 = data_43a920;
        data_4526dc = ecx_27;
        data_4526e0 = edx_41;
        data_4526e4 = eax_64;
        
        if (ebx_38 > 0)
        {
            int32_t var_80_13 = eax_64;
            int32_t var_88_13 = ecx_27;
            int32_t var_8c_13 = edx_41;
            data_4534d0;
            sub_456e00(&data_4526d8);
            int32_t ecx_28 = data_43a918;
            int32_t edx_42 = data_43a924;
            data_4526d8 = data_43a914;
            int32_t eax_66 = data_43a928;
            data_4526dc = ecx_28;
            int32_t ecx_29 = data_43a92c;
            data_4526e0 = edx_42;
            int32_t edx_43 = data_43a930;
            data_4526e4 = eax_66;
            int32_t eax_67 = data_43a93c;
            data_4526e8 = ecx_29;
            int32_t ecx_30 = data_43a940;
            data_4526ec = edx_43;
            data_4526f0 = eax_67;
            data_4526f4 = ecx_30;
            int32_t var_80_14 = eax_67;
            int32_t var_88_14 = ecx_30;
            int32_t var_8c_14 = edx_43;
            data_4534d4;
            sub_456ce0(&data_4526d8);
            void* edx_44 = data_4534d0;
            int32_t ecx_32 = data_452708 + **&data_4534d0 * data_41cdf0;
            data_452708 = ecx_32;
            int32_t eax_71 = *(edx_44 + 4);
            void* var_80_15 = edx_44 + 8;
            int32_t var_88_15 = ecx_32;
            void* var_8c_15 = edx_44;
            void* edi_22 = data_452708;
            sub_410cbe(edx_44 + 8, eax_71, data_4534d4 + 8, edi_22);
            data_452708 = edi_22;
            int32_t ecx_33 = data_43a920;
            int32_t edx_46 = data_43a924;
            data_4526d8 = data_43a91c;
            int32_t eax_77 = data_43a928;
            data_4526dc = ecx_33;
            data_4526e0 = edx_46;
            data_4526e4 = eax_77;
            int32_t var_80_16 = eax_77;
            int32_t var_84_16 = ebx_38;
            int32_t var_88_16 = ecx_33;
            int32_t var_8c_16 = edx_46;
            data_4534d0;
            sub_456e00(&data_4526d8);
            int32_t ebx_45 = data_4534d4;
            int32_t ecx_34 = *(data_4534d0 + 4);
            int32_t* eax_83 = data_4534d0 + 8;
            int32_t var_78_17 = edi_18;
            int32_t var_7c_17 = esi_17;
            int32_t var_84_17 = ebx_45;
            int32_t var_88_17 = ecx_34;
            int32_t var_8c_17 = eax_71;
            void* edi_25 = data_452708;
            sub_410cbe(eax_83, ecx_34, ebx_45 + eax_71 * 0xc + 8, edi_25);
            data_452708 = edi_25;
            return eax_83;
        }
        
        int32_t edx_50 = data_43a930;
        int32_t eax_86 = data_43a934;
        data_4526e8 = data_43a92c;
        int32_t ecx_37 = data_43a938;
        data_4526ec = edx_50;
        data_4526f0 = eax_86;
        data_4526f4 = ecx_37;
        int32_t var_80_18 = eax_86;
        int32_t var_88_18 = ecx_37;
        int32_t var_8c_18 = edx_50;
        data_4534d4;
        sub_456ce0(&data_4526d8);
        int32_t ecx_38 = data_43a918;
        int32_t edx_51 = data_43a924;
        data_4526d8 = data_43a914;
        int32_t eax_88 = data_43a928;
        data_4526dc = ecx_38;
        data_4526e0 = edx_51;
        data_4526e4 = eax_88;
        int32_t var_80_19 = eax_88;
        int32_t var_88_19 = ecx_38;
        int32_t var_8c_19 = edx_51;
        data_4534d0;
        sub_456e00(&data_4526d8);
        void* edx_52 = data_4534d4;
        int32_t ecx_40 = data_452708 + **&data_4534d0 * data_41cdf0;
        data_452708 = ecx_40;
        int32_t eax_92 = *(edx_52 + 4);
        int32_t* eax_95 = data_4534d0 + 8;
        int32_t* var_80_20 = eax_95;
        int32_t var_88_20 = ecx_40;
        void* var_8c_20 = edx_52;
        void* edi_28 = data_452708;
        sub_410cbe(eax_95, eax_92, edx_52 + 8, edi_28);
        data_452708 = edi_28;
        int32_t ecx_41 = data_43a920;
        int32_t edx_54 = data_43a924;
        data_4526d8 = data_43a91c;
        int32_t eax_98 = data_43a928;
        data_4526dc = ecx_41;
        int32_t ecx_42 = data_43a934;
        data_4526e0 = edx_54;
        int32_t edx_55 = data_43a938;
        data_4526e4 = eax_98;
        int32_t eax_99 = data_43a93c;
        data_4526e8 = ecx_42;
        int32_t ecx_43 = data_43a940;
        data_4526ec = edx_55;
        data_4526f0 = eax_99;
        data_4526f4 = ecx_43;
        int32_t var_80_21 = eax_99;
        int32_t var_88_21 = ecx_43;
        int32_t var_8c_21 = edx_55;
        data_4534d4;
        sub_456ce0(&data_4526d8);
        void* eax_100 = data_4534d4;
        int32_t edx_56 = data_4534d0;
        int32_t ecx_44 = *(eax_100 + 4);
        int32_t var_78_22 = edi_18;
        int32_t var_7c_22 = esi_17;
        int32_t var_84_22 = ebx_38;
        int32_t var_88_22 = ecx_44;
        int32_t var_8c_22 = edx_56;
        void* edi_31 = data_452708;
        sub_410cbe(edx_56 + (eax_92 << 2) + 8, ecx_44, eax_100 + 8, edi_31);
        data_452708 = edi_31;
        eax_55 = edx_56 + (eax_92 << 2) + 8;
    }
    
    return eax_55;
}

int32_t __stdcall sub_40bcfc(void* arg1 @ esi, int32_t* arg2 @ edi, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t eax_3 = *(arg1 + 4);
    int32_t ebx = *(arg1 + 8);
    
    if (eax_3 >= data_41cdd0 && eax_3 <= data_41cdd8 && ebx >= data_41cdd4 && ebx <= data_41cddc)
    {
        int32_t ebx_3;
        *ebx_3[1] = *(arg1 + 0x10);
        ebx_3 = *(arg1 + 0x15);
        char* ebx_4;
        ebx_4 = *(ebx_3 + data_41cdac);
        *((eax_3 >> 8) + *((ebx >> 8 << 2) + &data_41cdf8) + data_41cda4) = ebx_4;
    }
    
    int32_t* esi = *arg2;
    data_41cda0 = esi;
    
    if (!esi)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi] */
}

int32_t sub_40c650()
{
    int32_t eax = data_453504;
    data_436728 = 0;
    int32_t esi = data_45352c;
    int32_t esi_1 = esi - eax;
    
    if (esi != eax)
        data_436728 = (data_453528 - data_453500) / esi_1;
    
    int32_t ecx_2 = data_453518;
    int32_t eax_3 = data_453504;
    data_43672c = 0;
    int32_t ecx_3 = ecx_2 - eax_3;
    
    if (ecx_2 != eax_3)
        data_43672c = (data_453514 - data_453500) / ecx_3;
    
    int32_t edx_2 = data_45352c;
    int32_t eax_6 = data_453518;
    data_453554 = 0;
    int32_t edx_3 = edx_2 - eax_6;
    
    if (edx_2 != eax_6)
        data_453554 = (data_453528 - data_453514) / edx_3;
    
    int32_t ebx_2 = data_45350c;
    int32_t ebp = data_453510;
    int32_t edi = data_453508;
    data_436740 = data_453534 - ebx_2;
    data_436744 = data_453538 - ebp;
    data_436750 = data_453530 - edi;
    data_436748 = data_453520 - ebx_2;
    int32_t ebx_3 = data_453520;
    data_43674c = data_453524 - ebp;
    data_436754 = data_45351c - edi;
    int32_t ebx_4 = data_453524;
    data_4534ec = data_453534 - ebx_3;
    int32_t edi_1 = data_453528;
    int32_t ebx_5 = data_45351c;
    data_4534f0 = data_453538 - ebx_4;
    data_4534d8 = data_453530 - ebx_5;
    int32_t eax_27 = data_453500;
    int32_t edi_2 = edi_1 - eax_27;
    
    if (edi_1 - eax_27 < 0)
        edi_2 = data_453500 - data_453528;
    
    int32_t eax_30 = esi_1;
    
    if (esi_1 < 0)
        eax_30 = data_453504 - data_45352c;
    
    if (edi_2 <= eax_30)
        edi_2 = eax_30;
    
    int32_t ebx_7 = data_453514;
    int32_t eax_31 = data_453500;
    int32_t ebx_8 = ebx_7 - eax_31;
    
    if (ebx_7 - eax_31 < 0)
        ebx_8 = data_453500 - data_453514;
    
    int32_t eax_34 = ecx_3;
    
    if (ecx_3 < 0)
        eax_34 = data_453504 - data_453518;
    
    if (ebx_8 <= eax_34)
        ebx_8 = eax_34;
    
    int32_t eax_35 = data_453528;
    int32_t ebp_2 = data_453514;
    int32_t eax_36 = eax_35 - ebp_2;
    
    if (eax_35 - ebp_2 < 0)
        eax_36 = data_453514 - data_453528;
    
    int32_t ebp_5 = edx_3;
    
    if (edx_3 < 0)
        ebp_5 = data_453518 - data_45352c;
    
    if (eax_36 <= ebp_5)
        eax_36 = ebp_5;
    
    long double x87_r7_7 = data_453508;
    long double x87_r6_4 = data_453530;
    data_453530;
    float var_c = edi_2;
    long double x87_r4_2 = data_45351c;
    float var_8 = ebx_8;
    long double x87_r3 = eax_36;
    data_436730 = var_c / x87_r7_7 * x87_r6_4;
    int32_t eax_39 = -(data_436750);
    int32_t eax_41 = -(data_436754);
    data_436738 = var_8 / x87_r7_7 * x87_r4_2;
    data_453560 = x87_r3 / x87_r4_2 * x87_r6_4;
    int32_t eax_43 = -(data_4534d8);
    data_436734 = eax_39 / x87_r7_7;
    data_43673c = eax_41 / x87_r7_7;
    bool cond:0 = data_45352c == data_453504;
    data_45355c = eax_43 / x87_r6_4;
    
    if (!cond:0)
        data_436704 = var_c / esi_1;
    
    if (data_453518 != data_453504)
        data_436710 = var_8 / ecx_3;
    
    if (data_45352c != data_453518)
        data_453558 = x87_r3 / edx_3;
    
    int32_t eax_47 = data_45354c - data_453548;
    int32_t ebp_7 = data_45354c;
    data_4534dc = ecx_3;
    data_4534e4 = eax_47;
    long double x87_r7_19 = data_43672c;
    long double temp7 = data_436728;
    x87_r7_19 - temp7;
    int32_t eax_48 = data_453550;
    data_4534e0 = edx_3;
    data_436758 = 0;
    data_4534e8 = eax_48 - ebp_7;
    int32_t result;
    result = (x87_r7_19 < temp7 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_19, temp7) ? 1 : 0) << 0xa
        | (x87_r7_19 == temp7 ? 1 : 0) << 0xe;
    
    if (*result[1] & 0x41)
        data_436758 = 1;
    
    if (!ecx_3)
    {
        int32_t ecx_4 = data_453500;
        int32_t edx_4 = data_453514;
        data_436758 = 0;
        
        if (edx_4 < ecx_4)
            data_436758 = 1;
    }
    
    return result;
}

int32_t sub_40c9e0(void* arg1)
{
    int32_t i_1 = 1;
    int32_t i = 2;
    int32_t i_2 = 1;
    int32_t* esi = arg1 + 0x18;
    
    do
    {
        int32_t eax_1 = *esi;
        
        if (*(arg1 + 8 + i_2 * 0xc - 8) > eax_1)
            i_2 = i;
        
        if (*(arg1 + 8 + i_1 * 0xc - 8) < eax_1)
            i_1 = i;
        
        esi = &esi[3];
        i += 1;
    } while (i < 4);
    
    int32_t esi_2 = i_2 - i_1;
    
    if (i_2 - i_1 < 0)
        esi_2 = i_1 - i_2;
    
    if (i_2 + i_1 == 3)
        esi_2 = 3;
    
    void* ebx = arg1 + 8 + i_2 * 0xc;
    void* ebx_1 = arg1 + 8 + esi_2 * 0xc;
    data_453500 = *(ebx - 0xc);
    data_453514 = *(ebx_1 - 0xc);
    int32_t eax_6 = i_1 * 3;
    void* ebp_4 = arg1 + 8 + (eax_6 << 2);
    data_453528 = *(arg1 + 8 + (eax_6 << 2) - 0xc);
    data_453504 = *(ebx - 8);
    data_453518 = *(ebx_1 - 8);
    data_45352c = *(ebp_4 - 8);
    data_453508 = *(ebx - 4);
    data_45351c = *(ebx_1 - 4);
    data_453530 = *(ebp_4 - 4);
    data_45350c = *(arg1 + 8 + (i_2 << 3) + 0x1c);
    data_453520 = *(arg1 + 8 + (esi_2 << 3) + 0x1c);
    data_453534 = *(arg1 + 8 + (i_1 << 3) + 0x1c);
    data_453510 = *(arg1 + 8 + (i_2 << 3) + 0x20);
    int32_t eax_17 = data_453500 >> 8;
    data_453524 = *(arg1 + 8 + (esi_2 << 3) + 0x20);
    data_453538 = *(arg1 + 8 + (i_1 << 3) + 0x20);
    data_45353c = eax_17;
    data_453540 = data_453514 >> 8;
    data_453544 = data_453528 >> 8;
    data_453548 = data_453504 >> 8;
    int32_t ecx_2 = data_453500;
    int32_t edx = data_453504;
    data_45354c = data_453518 >> 8;
    int32_t eax_26 = data_45352c;
    data_436768 = ecx_2;
    data_453550 = eax_26 >> 8;
    data_43676c = edx;
    int32_t result = data_453508;
    int32_t ecx_3 = data_45350c;
    int32_t edx_1 = data_453510;
    data_436770 = result;
    data_436774 = ecx_3;
    data_436778 = edx_1;
    return result;
}

int32_t sub_40cb70()
{
    int32_t eax = data_453504;
    int32_t esi = data_41cde4;
    int32_t esi_1 = esi - eax;
    data_4534f4 = esi_1;
    
    if (esi - eax < 0)
        esi_1 = 0;
    
    int32_t ecx = data_45352c;
    int32_t eax_1 = data_41cdec;
    data_4534f8 = ecx - eax_1;
    
    if (ecx - eax_1 < 0)
        data_4534f8 = 0;
    
    int32_t edx = data_4534dc;
    int32_t ecx_2;
    int16_t x87control;
    
    if (edx <= esi_1)
        ecx_2 = data_4534e0;
    else
    {
        ecx_2 = data_4534e0;
        
        if (data_4534e4)
        {
            int32_t eax_3 = data_4534f8;
            
            if (ecx_2 < eax_3)
                edx = edx - eax_3 + ecx_2;
            
            data_4534f4 = esi_1;
            int32_t esi_2;
            
            if (esi_1)
            {
                data_4534f4 = esi_1;
                esi_2 = esi_1 + 0x100;
            }
            else
                esi_2 = 0x100 - data_453504;
            
            data_4366fc = esi_2;
            data_436708 = esi_2;
            long double x87_r6_3 = data_436708 * data_436710;
            long double x87_r6_5 = data_436708 * data_436704;
            int32_t eax_6 = data_453500;
            data_4534dc = edx;
            data_43670c = x87_r6_3;
            data_436700 = x87_r6_5;
            data_436714 = eax_6;
            data_436718 = data_453504;
            data_43671c = data_453508;
            data_4366f8 = edx;
            data_436720 = data_45350c;
            data_436724 = data_453510;
            x87control = sub_40e9cc(x87control);
            esi_1 = data_4534f4;
            edx = data_4534dc;
            ecx_2 = data_4534e0;
        }
    }
    
    int32_t result = data_4534f8;
    data_4534f4 = esi_1;
    
    if (ecx_2 >= result)
    {
        result = data_4534e8;
        
        if (result)
        {
            data_436710 = data_453558;
            int32_t edi_1 = 0x100 - data_453518;
            data_436708 = edi_1;
            
            if (edi_1 == 0x100)
                edi_1 = 0;
            
            int32_t eax_14 = edx + edi_1;
            data_4534f4 = esi_1;
            data_436708 = edi_1;
            data_4366fc = eax_14;
            
            if (edx < esi_1)
            {
                eax_14 = esi_1 + 0x100;
                edi_1 = esi_1 - edx + 0x100;
            }
            
            data_4366fc = eax_14;
            data_436708 = edi_1;
            long double x87_r6_11 = data_436708 * data_453558;
            int32_t edx_2 = data_4534f8;
            int32_t eax_15 = data_453514;
            data_436700 = data_4366fc * data_436704;
            data_43670c = x87_r6_11;
            int32_t ecx_3 = ecx_2 - edx_2;
            data_436714 = eax_15;
            int32_t eax_16 = data_453518;
            data_4534e0 = ecx_3;
            data_436718 = eax_16;
            int32_t edx_3 = data_453554;
            data_43671c = data_45351c;
            int32_t ebx_1 = data_4534ec;
            data_436720 = data_453520;
            data_43672c = edx_3;
            int32_t edx_4 = data_4534d8;
            data_436724 = data_453524;
            data_436748 = ebx_1;
            int32_t ebx_2 = data_453560;
            data_43674c = data_4534f0;
            data_436754 = edx_4;
            int32_t eax_21 = data_45355c;
            data_436738 = ebx_2;
            data_43673c = eax_21;
            data_4534dc = ecx_3;
            data_4366f8 = ecx_3;
            return sub_40e9cc(x87control);
        }
    }
    
    return result;
}

int32_t sub_40cde0(int32_t* arg1)
{
    data_436764 = 0;
    
    if (*arg1 == 0x15)
    {
        data_436764 = 1;
        data_436760 = arg1[0x11];
    }
    
    data_43675c = arg1[1];
    sub_40c9e0(arg1);
    sub_40c650();
    /* tailcall */
    return sub_40cb70();
}

void __convention("regparm") sub_40ce20(int32_t arg1)
{
    data_434082 = arg1 + 1;
    data_434086 = -(arg1) + 1;
}

int32_t __stdcall sub_40ce37(void* arg1 @ esi, int32_t* arg2 @ edi, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t ebx;
    ebx = *(arg1 + 0x21);
    *ebx[1] = *(arg1 + 0x1c);
    char* ebx_1;
    ebx_1 = *(ebx + data_41cdac);
    data_4301b0 = ebx_1;
    int32_t eax_1 = *(arg1 + 4) >> 8;
    int32_t ebx_3 = *(arg1 + 8) >> 8;
    int32_t ecx_1 = *(arg1 + 0x10) >> 8;
    int32_t edx_1 = *(arg1 + 0x14) >> 8;
    
    if (eax_1 > ecx_1)
    {
        int32_t temp0_1 = ecx_1;
        ecx_1 = eax_1;
        eax_1 = temp0_1;
        int32_t temp0_2 = edx_1;
        edx_1 = ebx_3;
        ebx_3 = temp0_2;
    }
    
    if (eax_1 >= data_41cdb0)
        goto label_40ce78;
    
    if (ecx_1 >= data_41cdb0)
    {
        data_43019c = eax_1;
        data_4301a0 = ebx_3;
        data_4301a4 = ecx_1;
        data_4301a8 = edx_1;
        data_4301ac =
            (COMBINE(data_4301a4 - data_41cdb0 + 1, 0) / (data_4301a4 - data_43019c + 1)) >> 1;
        int32_t eax_18 = data_4301a8 - data_4301a0;
        int32_t eax_20;
        int32_t edx_18;
        edx_18 = HIGHD(eax_18 * 2 * data_4301ac);
        eax_20 = LOWD(eax_18 * 2 * data_4301ac);
        data_4301a0 += eax_18 - edx_18;
        data_43019c = data_41cdb0;
        eax_1 = data_43019c;
        ebx_3 = data_4301a0;
        ecx_1 = data_4301a4;
        edx_1 = data_4301a8;
    label_40ce78:
        
        if (ecx_1 <= data_41cdb8)
            goto label_40ce84;
        
        if (eax_1 <= data_41cdb8)
        {
            data_43019c = eax_1;
            data_4301a0 = ebx_3;
            data_4301a4 = ecx_1;
            data_4301a8 = edx_1;
            data_4301ac =
                (COMBINE(data_41cdb8 - data_43019c + 1, 0) / (data_4301a4 - data_43019c + 1)) >> 1;
            int32_t eax_12;
            int32_t edx_13;
            edx_13 = HIGHD((data_4301a8 - data_4301a0) * 2 * data_4301ac);
            eax_12 = LOWD((data_4301a8 - data_4301a0) * 2 * data_4301ac);
            data_4301a8 = edx_13 + data_4301a0;
            data_4301a4 = data_41cdb8;
            eax_1 = data_43019c;
            ebx_3 = data_4301a0;
            ecx_1 = data_4301a4;
            edx_1 = data_4301a8;
        label_40ce84:
            
            if (ebx_3 > edx_1)
            {
                if (ebx_3 <= data_41cdbc)
                    goto label_40cead;
                
                if (edx_1 <= data_41cdbc)
                {
                    data_43019c = eax_1;
                    data_4301a0 = ebx_3;
                    data_4301a4 = ecx_1;
                    data_4301a8 = edx_1;
                    data_4301ac = (COMBINE(data_41cdbc - data_4301a8 + 1, 0)
                        / (data_4301a0 - data_4301a8 + 1)) >> 1;
                    int32_t eax_36;
                    int32_t edx_27;
                    edx_27 = HIGHD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                    eax_36 = LOWD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                    data_43019c = -(edx_27) + data_4301a4;
                    data_4301a0 = data_41cdbc;
                    eax_1 = data_43019c;
                    ebx_3 = data_4301a0;
                    ecx_1 = data_4301a4;
                    edx_1 = data_4301a8;
                label_40cead:
                    
                    if (edx_1 >= data_41cdb4)
                        goto label_40ceb9;
                    
                    if (ebx_3 >= data_41cdb4)
                    {
                        data_43019c = eax_1;
                        data_4301a0 = ebx_3;
                        data_4301a4 = ecx_1;
                        data_4301a8 = edx_1;
                        data_4301ac = (COMBINE(data_4301a0 - data_41cdb4 + 1, 0)
                            / (data_4301a0 - data_4301a8 + 1)) >> 1;
                        int32_t eax_28;
                        int32_t edx_22;
                        edx_22 = HIGHD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                        eax_28 = LOWD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                        data_4301a4 = edx_22 + data_43019c;
                        data_4301a8 = data_41cdb4;
                        eax_1 = data_43019c;
                        ebx_3 = data_4301a0;
                        ecx_1 = data_4301a4;
                        edx_1 = data_4301a8;
                    label_40ceb9:
                        *((ebx_3 << 2) + &data_41cdf8);
                        data_41cda4;
                        int32_t edx_2 = edx_1 - ebx_3;
                        
                        if (edx_2 < 0)
                        {
                            if (ecx_1 - eax_1 + 1 <= -(edx_2) + 1)
                            {
                                uint16_t eax_4 = (-(edx_2) + 1);
                                int16_t temp2_5 = (ecx_1 - eax_1 + 1);
                                data_43408e = COMBINE(0, eax_4) / temp2_5;
                                data_43408c = (ecx_1 - eax_1 + 1);
                                data_43408a =
                                    COMBINE(COMBINE(0, eax_4) % temp2_5, 0) / (ecx_1 - eax_1 + 1);
                                uint16_t edx_8;
                                *edx_8[1] = data_4301b0;
                                ebx_3 = data_43408e;
                                /* jump -> *((ebx_3 << 2) + &data_43211a) */
                            }
                            
                            uint16_t eax_5 = (ecx_1 - eax_1 + 1);
                            data_43408c = (-(edx_2) + 1);
                            ebx_3 = (-(edx_2) + 1);
                            int16_t temp2_7 = ebx_3;
                            data_43408e = COMBINE(0, eax_5) / temp2_7;
                            data_43408a = COMBINE(COMBINE(0, eax_5) % temp2_7, 0) / ebx_3;
                            uint16_t edx_9;
                            *edx_9[1] = data_4301b0;
                            ebx_3 = data_43408e;
                            /* jump -> *((ebx_3 << 2) + &data_432a8e) */
                        }
                        
                        if (ecx_1 - eax_1 + 1 <= edx_2 + 1)
                        {
                            uint16_t eax_2 = (edx_2 + 1);
                            int16_t temp2_1 = (ecx_1 - eax_1 + 1);
                            data_43408e = COMBINE(0, eax_2) / temp2_1;
                            data_43408c = (ecx_1 - eax_1 + 1);
                            data_43408a =
                                COMBINE(COMBINE(0, eax_2) % temp2_1, 0) / (ecx_1 - eax_1 + 1);
                            uint16_t edx_4;
                            *edx_4[1] = data_4301b0;
                            ebx_3 = data_43408e;
                            /* jump -> *((ebx_3 << 2) + &data_4301b2) */
                        }
                        
                        uint16_t eax_3 = (ecx_1 - eax_1 + 1);
                        data_43408c = (edx_2 + 1);
                        ebx_3 = (edx_2 + 1);
                        int16_t temp2_3 = ebx_3;
                        data_43408e = COMBINE(0, eax_3) / temp2_3;
                        data_43408a = COMBINE(COMBINE(0, eax_3) % temp2_3, 0) / ebx_3;
                        uint16_t edx_5;
                        *edx_5[1] = data_4301b0;
                        ebx_3 = data_43408e;
                        /* jump -> *((ebx_3 << 2) + &data_430b26) */
                    }
                }
            }
            else
            {
                if (edx_1 <= data_41cdbc)
                    goto label_40ce94;
                
                if (ebx_3 <= data_41cdbc)
                {
                    data_43019c = eax_1;
                    data_4301a0 = ebx_3;
                    data_4301a4 = ecx_1;
                    data_4301a8 = edx_1;
                    data_4301ac = (COMBINE(data_41cdbc - data_4301a0 + 1, 0)
                        / (data_4301a8 - data_4301a0 + 1)) >> 1;
                    int32_t eax_44;
                    int32_t edx_33;
                    edx_33 = HIGHD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                    eax_44 = LOWD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                    data_4301a4 = edx_33 + data_43019c;
                    data_4301a8 = data_41cdbc;
                    eax_1 = data_43019c;
                    ebx_3 = data_4301a0;
                    ecx_1 = data_4301a4;
                    edx_1 = data_4301a8;
                label_40ce94:
                    
                    if (ebx_3 >= data_41cdb4)
                        goto label_40ceb9;
                    
                    if (edx_1 >= data_41cdb4)
                    {
                        data_43019c = eax_1;
                        data_4301a0 = ebx_3;
                        data_4301a4 = ecx_1;
                        data_4301a8 = edx_1;
                        data_4301ac = (COMBINE(data_4301a8 - data_41cdb4 + 1, 0)
                            / (data_4301a8 - data_4301a0 + 1)) >> 1;
                        int32_t eax_52;
                        int32_t edx_38;
                        edx_38 = HIGHD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                        eax_52 = LOWD((data_4301a4 - data_43019c) * 2 * data_4301ac);
                        data_43019c = -(edx_38) + data_4301a4;
                        data_4301a0 = data_41cdb4;
                        eax_1 = data_43019c;
                        ebx_3 = data_4301a0;
                        ecx_1 = data_4301a4;
                        edx_1 = data_4301a8;
                        goto label_40ceb9;
                    }
                }
            }
        }
    }
    
    int32_t* esi_3 = *arg2;
    data_41cda0 = esi_3;
    
    if (!esi_3)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi_3] */
}

int32_t* sub_40d750(int32_t* arg1)
{
    int32_t ecx = arg1[1];
    int32_t eax = arg1[3];
    int32_t eax_1;
    
    if (ecx <= eax)
    {
        data_452758 = eax;
        eax_1 = arg1[1];
    }
    else
    {
        data_452758 = ecx;
        eax_1 = arg1[3];
    }
    
    int32_t ecx_1 = data_452758;
    data_452750 = eax_1;
    int32_t eax_2 = arg1[5];
    
    if (ecx_1 < eax_2)
        data_452758 = eax_2;
    else if (data_452750 > eax_2)
        data_452750 = eax_2;
    
    data_452754 = 1;
    data_452740 = 3;
    data_452748 = 5;
    
    if (arg1[6] < arg1[2])
    {
        data_452754 = 5;
        data_452748 = 1;
    }
    
    int32_t eax_3 = data_452754;
    int32_t ecx_3 = arg1[4];
    
    if (arg1[eax_3 + 1] <= ecx_3)
    {
        int32_t eax_4 = data_452748;
        
        if (arg1[eax_4 + 1] < ecx_3)
        {
            data_452748 = 3;
            data_452740 = eax_4;
        }
    }
    else
    {
        data_452754 = 3;
        data_452740 = eax_3;
    }
    
    int32_t esi_2 = data_452754 << 2;
    int32_t ecx_5 = data_452748 << 2;
    int32_t edi = arg1[data_452754 + 1];
    int32_t eax_5 = arg1[data_452748 + 1];
    
    if (((data_452750 - data_41cde0) | (data_41cdec - eax_5) | (data_41cde8 - data_452758)
        | (edi - data_41cde4)) >= 0)
    {
        int32_t edi_2 = data_452740 << 2;
        data_43a914 = arg1[data_452754];
        data_43a918 = arg1[data_452754 + 1];
        data_43a91c = arg1[data_452740];
        data_43a920 = arg1[data_452740 + 1];
        data_43a924 = arg1[data_452748];
        data_43a928 = arg1[data_452748 + 1];
        int32_t edx_13 = arg1[7];
        data_43a92c = *(esi_2 + edx_13 - 4);
        data_43a930 = *(esi_2 + edx_13);
        int32_t esi_3 = *(edi_2 + edx_13 - 4);
        data_43a934 = esi_3;
        data_43a938 = *(edi_2 + edx_13);
        int32_t eax_11 = data_41cda4;
        int32_t ebx_13 = *(ecx_5 + edx_13 - 4);
        data_43a93c = ebx_13;
        int32_t ecx_6 = *(ecx_5 + edx_13);
        data_43a940 = ecx_6;
        data_45274c = eax_11;
        int32_t var_80_1 = eax_11;
        int32_t var_88_1 = ecx_6;
        int32_t var_8c_1 = edx_13;
        sub_456b20();
        bool cond:1 = data_43a960 <= 0;
        int32_t ecx_8 = data_43a918;
        int32_t edx_15 = data_43a91c;
        data_43a94c = arg1[8];
        data_45271c = ecx_8;
        int32_t eax_13 = data_43a914;
        data_452720 = edx_15;
        data_452718 = eax_13;
        int32_t eax_14 = data_43a920;
        data_452724 = eax_14;
        
        if (cond:1)
        {
            int32_t edx_24 = data_43a930;
            int32_t eax_34 = data_43a934;
            data_452728 = data_43a92c;
            int32_t ecx_17 = data_43a938;
            data_45272c = edx_24;
            data_452730 = eax_34;
            data_452734 = ecx_17;
            int32_t var_80_7 = eax_34;
            int32_t var_88_7 = ecx_17;
            data_4534d4;
            sub_456c70(&data_452718);
            int32_t eax_35 = data_43a924;
            int32_t ecx_18 = data_43a928;
            data_452720 = eax_35;
            data_452724 = ecx_18;
            int32_t var_80_8 = eax_35;
            int32_t var_88_8 = ecx_18;
            int32_t var_8c_8 = edx_24;
            data_4534d0;
            sub_456cb0(&data_452718);
            void* edx_26 = data_4534d4;
            int32_t ecx_20 = data_45274c + **&data_4534d0 * data_41cdf0;
            data_45274c = ecx_20;
            int32_t eax_39 = *(edx_26 + 4);
            int32_t* eax_42 = data_4534d0 + 8;
            int32_t* var_80_9 = eax_42;
            int32_t var_88_9 = ecx_20;
            void* var_8c_9 = edx_26;
            void* edi_12 = data_45274c;
            sub_410d5c(eax_42, eax_39, edx_26 + 8, edi_12);
            data_45274c = edi_12;
            int32_t ecx_21 = data_43a920;
            int32_t edx_28 = data_43a934;
            data_452718 = data_43a91c;
            int32_t eax_45 = data_43a938;
            data_45271c = ecx_21;
            int32_t ecx_22 = data_43a93c;
            data_452728 = edx_28;
            int32_t edx_29 = data_43a940;
            data_45272c = eax_45;
            data_452730 = ecx_22;
            data_452734 = edx_29;
            int32_t var_80_10 = eax_45;
            int32_t var_88_10 = ecx_22;
            int32_t var_8c_10 = edx_29;
            data_4534d4;
            sub_456c70(&data_452718);
            void* eax_46 = data_4534d4;
            int32_t edx_30 = data_4534d0;
            int32_t ecx_23 = *(eax_46 + 4);
            int32_t var_78_11 = edi_2;
            int32_t var_7c_11 = esi_3;
            int32_t var_84_11 = ebx_13;
            int32_t var_88_11 = ecx_23;
            int32_t var_8c_11 = edx_30;
            void* edi_15 = data_45274c;
            sub_410d5c(edx_30 + (eax_39 << 2) + 8, ecx_23, eax_46 + 8, edi_15);
            data_45274c = edi_15;
            return edx_30 + (eax_39 << 2) + 8;
        }
        
        int32_t var_80_2 = eax_14;
        int32_t var_88_2 = ecx_8;
        int32_t var_8c_2 = edx_15;
        data_4534d0;
        sub_456cb0(&data_452718);
        int32_t ecx_9 = data_43a928;
        int32_t edx_16 = data_43a92c;
        data_452720 = data_43a924;
        int32_t eax_16 = data_43a930;
        data_452724 = ecx_9;
        int32_t ecx_10 = data_43a93c;
        data_452728 = edx_16;
        int32_t edx_17 = data_43a940;
        data_45272c = eax_16;
        data_452730 = ecx_10;
        data_452734 = edx_17;
        int32_t var_80_3 = eax_16;
        int32_t var_88_3 = ecx_10;
        int32_t var_8c_3 = edx_17;
        data_4534d4;
        sub_456c70(&data_452718);
        void* edx_18 = data_4534d0;
        int32_t ecx_12 = data_45274c + **&data_4534d0 * data_41cdf0;
        data_45274c = ecx_12;
        int32_t eax_20 = *(edx_18 + 4);
        void* var_80_4 = edx_18 + 8;
        int32_t var_88_4 = ecx_12;
        void* edi_6 = data_45274c;
        sub_410d5c(edx_18 + 8, eax_20, data_4534d4 + 8, edi_6);
        data_45274c = edi_6;
        int32_t eax_25 = data_43a91c;
        int32_t ecx_13 = data_43a920;
        data_452718 = eax_25;
        data_45271c = ecx_13;
        int32_t var_80_5 = eax_25;
        int32_t var_84_5 = ebx_13;
        int32_t var_88_5 = ecx_13;
        void* var_8c_5 = edx_18;
        data_4534d0;
        sub_456cb0(&data_452718);
        int32_t ebx_21 = data_4534d4;
        int32_t ecx_14 = *(data_4534d0 + 4);
        int32_t* eax_31 = data_4534d0 + 8;
        int32_t var_78_6 = edi_2;
        int32_t var_7c_6 = esi_3;
        int32_t var_84_6 = ebx_21;
        int32_t var_88_6 = ecx_14;
        int32_t var_8c_6 = eax_20;
        void* edi_9 = data_45274c;
        sub_410d5c(eax_31, ecx_14, ebx_21 + eax_20 * 0xc + 8, edi_9);
        data_45274c = edi_9;
        return eax_31;
    }
    
    int32_t* eax_55 = data_452758 - data_41cde0;
    
    if (((data_41cde8 - data_452750) | (data_41cdec - edi) | (eax_5 - data_41cde4) | eax_55) >= 0)
    {
        int32_t edi_18 = data_452740 << 2;
        data_43a914 = arg1[data_452754];
        data_43a918 = arg1[data_452754 + 1];
        data_43a91c = arg1[data_452740];
        data_43a920 = arg1[data_452740 + 1];
        data_43a924 = arg1[data_452748];
        data_43a928 = arg1[data_452748 + 1];
        int32_t edx_39 = arg1[7];
        data_43a92c = *(esi_2 + edx_39 - 4);
        data_43a930 = *(esi_2 + edx_39);
        int32_t esi_17 = *(edi_18 + edx_39 - 4);
        data_43a934 = esi_17;
        data_43a938 = *(edi_18 + edx_39);
        int32_t eax_61 = data_41cda4;
        int32_t ebx_37 = *(ecx_5 + edx_39 - 4);
        data_43a93c = ebx_37;
        int32_t ecx_25 = *(ecx_5 + edx_39);
        data_43a940 = ecx_25;
        data_45274c = eax_61;
        int32_t var_80_12 = eax_61;
        int32_t var_88_12 = ecx_25;
        int32_t var_8c_12 = edx_39;
        sub_456b20();
        bool cond:2_1 = data_43a960 <= 0;
        int32_t ecx_27 = data_43a918;
        int32_t edx_41 = data_43a91c;
        data_43a94c = arg1[8];
        data_45271c = ecx_27;
        int32_t eax_63 = data_43a914;
        data_452720 = edx_41;
        data_452718 = eax_63;
        int32_t eax_64 = data_43a920;
        data_452724 = eax_64;
        
        if (!cond:2_1)
        {
            int32_t var_80_13 = eax_64;
            int32_t var_88_13 = ecx_27;
            int32_t var_8c_13 = edx_41;
            data_4534d0;
            sub_456e00(&data_452718);
            int32_t ecx_28 = data_43a918;
            int32_t edx_42 = data_43a924;
            data_452718 = data_43a914;
            int32_t eax_66 = data_43a928;
            data_45271c = ecx_28;
            int32_t ecx_29 = data_43a92c;
            data_452720 = edx_42;
            int32_t edx_43 = data_43a930;
            data_452724 = eax_66;
            int32_t eax_67 = data_43a93c;
            data_452728 = ecx_29;
            int32_t ecx_30 = data_43a940;
            data_45272c = edx_43;
            data_452730 = eax_67;
            data_452734 = ecx_30;
            int32_t var_80_14 = eax_67;
            int32_t var_88_14 = ecx_30;
            int32_t var_8c_14 = edx_43;
            data_4534d4;
            sub_456ce0(&data_452718);
            void* edx_44 = data_4534d0;
            int32_t ecx_32 = data_45274c + **&data_4534d0 * data_41cdf0;
            data_45274c = ecx_32;
            int32_t eax_71 = *(edx_44 + 4);
            void* var_80_15 = edx_44 + 8;
            int32_t var_88_15 = ecx_32;
            void* var_8c_15 = edx_44;
            void* edi_22 = data_45274c;
            sub_410d5c(edx_44 + 8, eax_71, data_4534d4 + 8, edi_22);
            data_45274c = edi_22;
            int32_t ecx_33 = data_43a920;
            int32_t edx_46 = data_43a924;
            data_452718 = data_43a91c;
            int32_t eax_77 = data_43a928;
            data_45271c = ecx_33;
            data_452720 = edx_46;
            data_452724 = eax_77;
            int32_t var_80_16 = eax_77;
            int32_t var_84_16 = ebx_37;
            int32_t var_88_16 = ecx_33;
            int32_t var_8c_16 = edx_46;
            data_4534d0;
            sub_456e00(&data_452718);
            int32_t ebx_45 = data_4534d4;
            int32_t ecx_34 = *(data_4534d0 + 4);
            int32_t* eax_83 = data_4534d0 + 8;
            int32_t var_78_17 = edi_18;
            int32_t var_7c_17 = esi_17;
            int32_t var_84_17 = ebx_45;
            int32_t var_88_17 = ecx_34;
            int32_t var_8c_17 = eax_71;
            void* edi_25 = data_45274c;
            sub_410d5c(eax_83, ecx_34, ebx_45 + eax_71 * 0xc + 8, edi_25);
            data_45274c = edi_25;
            return eax_83;
        }
        
        int32_t edx_50 = data_43a930;
        int32_t eax_86 = data_43a934;
        data_452728 = data_43a92c;
        int32_t ecx_37 = data_43a938;
        data_45272c = edx_50;
        data_452730 = eax_86;
        data_452734 = ecx_37;
        int32_t var_80_18 = eax_86;
        int32_t var_88_18 = ecx_37;
        int32_t var_8c_18 = edx_50;
        data_4534d4;
        sub_456ce0(&data_452718);
        int32_t ecx_38 = data_43a918;
        int32_t edx_51 = data_43a924;
        data_452718 = data_43a914;
        int32_t eax_88 = data_43a928;
        data_45271c = ecx_38;
        data_452720 = edx_51;
        data_452724 = eax_88;
        int32_t var_80_19 = eax_88;
        int32_t var_88_19 = ecx_38;
        int32_t var_8c_19 = edx_51;
        data_4534d0;
        sub_456e00(&data_452718);
        void* edx_52 = data_4534d4;
        int32_t ecx_40 = data_45274c + **&data_4534d0 * data_41cdf0;
        data_45274c = ecx_40;
        int32_t eax_92 = *(edx_52 + 4);
        int32_t* eax_95 = data_4534d0 + 8;
        int32_t* var_80_20 = eax_95;
        int32_t var_88_20 = ecx_40;
        void* var_8c_20 = edx_52;
        void* edi_28 = data_45274c;
        sub_410d5c(eax_95, eax_92, edx_52 + 8, edi_28);
        data_45274c = edi_28;
        int32_t ecx_41 = data_43a920;
        int32_t edx_54 = data_43a924;
        data_452718 = data_43a91c;
        int32_t eax_98 = data_43a928;
        data_45271c = ecx_41;
        int32_t ecx_42 = data_43a934;
        data_452720 = edx_54;
        int32_t edx_55 = data_43a938;
        data_452724 = eax_98;
        int32_t eax_99 = data_43a93c;
        data_452728 = ecx_42;
        int32_t ecx_43 = data_43a940;
        data_45272c = edx_55;
        data_452730 = eax_99;
        data_452734 = ecx_43;
        int32_t var_80_21 = eax_99;
        int32_t var_88_21 = ecx_43;
        int32_t var_8c_21 = edx_55;
        data_4534d4;
        sub_456ce0(&data_452718);
        void* eax_100 = data_4534d4;
        int32_t edx_56 = data_4534d0;
        int32_t ecx_44 = *(eax_100 + 4);
        int32_t var_78_22 = edi_18;
        int32_t var_7c_22 = esi_17;
        int32_t var_84_22 = ebx_37;
        int32_t var_88_22 = ecx_44;
        int32_t var_8c_22 = edx_56;
        void* edi_31 = data_45274c;
        sub_410d5c(edx_56 + (eax_92 << 2) + 8, ecx_44, eax_100 + 8, edi_31);
        data_45274c = edi_31;
        eax_55 = edx_56 + (eax_92 << 2) + 8;
    }
    
    return eax_55;
}

int32_t sub_40e0ac()
{
    data_4366b8 = 0x1075d7;
}

int32_t __stdcall sub_40e0bb(void* arg1 @ esi, int32_t* arg2 @ edi, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    void* ebx = &data_434098;
    int32_t* esi = arg1 + 4;
    int32_t i_1 = 3;
    int32_t i;
    
    do
    {
        int32_t eax_2 = *esi >> 8;
        
        if (eax_2 < 0xffffc180)
            eax_2 = 0xffffc180;
        
        if (eax_2 > 0x3e80)
            eax_2 = 0x3e80;
        
        *ebx = eax_2;
        int32_t eax_4 = esi[1] >> 8;
        
        if (eax_4 < 0xffffc180)
            eax_4 = 0xffffc180;
        
        if (eax_4 > 0x3e80)
            eax_4 = 0x3e80;
        
        *(ebx + 4) = eax_4;
        *(ebx + 8) = esi[2];
        *(ebx + 0xc) = esi[3];
        esi = &esi[4];
        ebx += 0x10;
        i = i_1;
        i_1 -= 1;
    } while (i != 1);
    *ebx = *esi;
    void* ecx = &data_4340a8;
    void* ebx_1 = &data_434098;
    int32_t eax_8 = data_4340ac;
    void* edx = &data_4340b8;
    
    if (eax_8 < data_43409c)
    {
        int32_t eax_10 = data_4340bc;
        ecx = &data_434098;
        ebx_1 = &data_4340a8;
        
        if (eax_10 <= data_43409c)
        {
            edx = &data_434098;
            ecx = &data_4340b8;
            
            if (eax_10 <= data_4340ac)
            {
                ecx = &data_4340a8;
                ebx_1 = &data_4340b8;
            }
        }
    }
    else if (eax_8 >= data_4340bc)
    {
        edx = &data_4340a8;
        ecx = &data_4340b8;
        
        if (data_43409c >= data_4340bc)
        {
            ecx = &data_434098;
            ebx_1 = &data_4340b8;
        }
    }
    
    if (*(edx + 4) - *(ebx_1 + 4) <= 0x3a98)
    {
        int32_t eax_13 = *(ebx_1 + 4);
        
        if (eax_13 == *(edx + 4))
        {
            if (*ebx_1 > *ecx)
            {
                void* temp0_1 = ecx;
                ecx = ebx_1;
                ebx_1 = temp0_1;
            }
            
            int32_t eax_114 = *edx;
            int32_t edi_11 = *(edx + 0xc);
            
            if (eax_114 <= *ecx)
            {
                if (eax_114 <= *ebx_1)
                    ebx_1 = edx;
                
                eax_114 = *ecx;
                edi_11 = *(ecx + 0xc);
            }
            
            *(data_434126 + 2) = eax_114;
            *(data_434120 + 2) = *ebx_1;
            data_434134 = *(ebx_1 + 4);
            data_43411c = 0;
            int32_t eax_118 = *(ebx_1 + 0xc) << 8;
            data_434130 = edi_11 << 8;
            data_43412c = eax_118;
            sub_40e79b(0x434094);
        }
        else if (eax_13 == *(ecx + 4))
        {
            if (*ebx_1 > *ecx)
            {
                void* temp0_3 = ecx;
                ecx = ebx_1;
                ebx_1 = temp0_3;
            }
            
            data_434134 = *(ebx_1 + 4);
            int32_t edi_14 = *(edx + 4) - *(ebx_1 + 4);
            data_43411c = edi_14;
            data_43412c = *(ebx_1 + 0xc) << 8;
            data_434130 = *(ecx + 0xc) << 8;
            *(data_434120 + 2) = *ebx_1;
            int32_t eax_126 = *ecx;
            *(data_434126 + 2) = eax_126;
            int32_t eax_128 = -((eax_126 - *edx));
            int32_t edi_15 = edi_14 + data_4366b8;
            int32_t eax_130;
            
            if (eax_128 <= 0)
            {
                *data_434126 = 0xffff;
                eax_130 = (eax_128 - 1) * *(edi_15 << 2);
            }
            else
            {
                *data_434126 = 0;
                eax_130 = (eax_128 + 1) * *(edi_15 << 2);
                data_434126 += eax_130;
            }
            
            data_43410c = eax_130;
            int32_t eax_133 = *edx - *ebx_1;
            int32_t eax_135;
            
            if (eax_133 >= 0)
            {
                *data_434120 = 0;
                eax_135 = (eax_133 + 1) * *(edi_15 << 2);
            }
            else
            {
                *data_434120 = 0xffff;
                eax_135 = (eax_133 - 1) * *(edi_15 << 2);
                data_434120 += eax_135;
            }
            
            data_434108 = eax_135;
            data_434110 = ((*(edx + 0xc) - *(ebx_1 + 0xc)) * *(edi_15 << 2)) >> 8;
            data_434114 = ((*(edx + 0xc) - *(ecx + 0xc)) * *(edi_15 << 2)) >> 8;
            sub_40e79b(0x434094);
        }
        else if (*(ecx + 4) == *(edx + 4))
        {
            if (*ecx > *edx)
            {
                void* temp0_4 = edx;
                edx = ecx;
                ecx = temp0_4;
            }
            
            int32_t eax_147 = *(ebx_1 + 0xc) << 8;
            data_43412c = eax_147;
            data_434130 = eax_147;
            data_434134 = *(ebx_1 + 4);
            int32_t edi_17 = *(edx + 4) - *(ebx_1 + 4);
            data_43411c = edi_17;
            int32_t eax_149 = *ebx_1;
            *(data_434120 + 2) = eax_149;
            *(data_434126 + 2) = eax_149;
            int32_t eax_151 = -((eax_149 - *edx));
            int32_t edi_18 = edi_17 + data_4366b8;
            int32_t eax_153;
            
            if (eax_151 <= 0)
            {
                *data_434126 = 0xffff;
                eax_153 = (eax_151 - 1) * *(edi_18 << 2);
            }
            else
            {
                *data_434126 = 0;
                eax_153 = (eax_151 + 1) * *(edi_18 << 2);
                data_434126 += eax_153;
            }
            
            data_43410c = eax_153;
            int32_t eax_156 = *ecx - *ebx_1;
            int32_t eax_158;
            
            if (eax_156 >= 0)
            {
                *data_434120 = 0;
                eax_158 = (eax_156 + 1) * *(edi_18 << 2);
            }
            else
            {
                *data_434120 = 0xffff;
                eax_158 = (eax_156 - 1) * *(edi_18 << 2);
                data_434120 += eax_158;
            }
            
            data_434108 = eax_158;
            data_434110 = ((*(ecx + 0xc) - *(ebx_1 + 0xc)) * *(edi_18 << 2)) >> 8;
            data_434114 = ((*(edx + 0xc) - *(ebx_1 + 0xc)) * *(edi_18 << 2)) >> 8;
            sub_40e79b(0x434094);
        }
        else
        {
            data_434134 = *(ebx_1 + 4);
            data_4340cc = *(ecx + 4);
            data_43411c = *(ecx + 4) - *(ebx_1 + 4);
            data_4340d0 = *(edx + 4) - *(ecx + 4);
            int32_t eax_21 = *ebx_1;
            *(data_434120 + 2) = eax_21;
            *(data_434126 + 2) = eax_21;
            int32_t eax_23 = *(ebx_1 + 0xc) << 8;
            data_43412c = eax_23;
            data_434130 = eax_23;
            int32_t edi_2 = *(edx + 4) - *(ebx_1 + 4) + data_4366b8;
            int32_t eax_30 =
                (((*edx - *ebx_1) * *(edi_2 << 2) * data_43411c + 0x8000) >> 0x10) + *ebx_1;
            
            if (eax_30 >= *ecx)
            {
                data_4340e4 = eax_30;
                data_4340f4 = (((*(edx + 0xc) - *(ebx_1 + 0xc)) * *(edi_2 << 2) * data_43411c) >> 8)
                    + (*(ebx_1 + 0xc) << 8);
                int32_t eax_74 = *edx - *ebx_1;
                int32_t eax_76;
                
                if (eax_74 >= 0)
                {
                    *data_434126 = 0;
                    eax_76 = (eax_74 + 1) * *(edi_2 << 2);
                    data_4340e2 = 0;
                    data_434126 += eax_76;
                    *data_4340e2 += eax_76;
                }
                else
                {
                    eax_76 = (eax_74 - 1) * *(edi_2 << 2);
                    *data_434126 = 0xffff;
                    data_4340e2 = 0xffff;
                }
                
                data_43410c = eax_76;
                data_4340d8 = eax_76;
                int32_t eax_81 =
                    ((*(edx + 0xc) - *(ebx_1 + 0xc)) * *((edi_2 << 2) + 0xfffffffc)) >> 8;
                data_434114 = eax_81;
                data_4340ec = eax_81;
                int32_t edi_8 = data_43411c + data_4366b8;
                int32_t eax_83 = *ecx - *ebx_1;
                int32_t eax_85;
                
                if (eax_83 <= 0)
                {
                    eax_85 = (eax_83 - 1) * *(edi_8 << 2);
                    *data_434120 = 0xffff;
                    data_434120 += eax_85;
                }
                else
                {
                    eax_85 = (eax_83 + 1) * *(edi_8 << 2);
                    *data_434120 = 0;
                }
                
                data_434108 = eax_85;
                data_434110 = ((*(ecx + 0xc) - *(ebx_1 + 0xc)) * *((edi_8 << 2) + 0xfffffffc)) >> 8;
                data_4340f0 = *(ecx + 0xc) << 8;
                data_4340de = *ecx;
                int32_t edi_10 = data_4340d0 + data_4366b8;
                int32_t eax_95 = *edx - *ecx;
                int32_t eax_97;
                
                if (eax_95 >= 0)
                {
                    eax_97 = (eax_95 + 1) * *(edi_10 << 2);
                    data_4340dc = 0;
                }
                else
                {
                    eax_97 = (eax_95 - 1) * *(edi_10 << 2);
                    data_4340dc = 0xffff;
                    *data_4340dc += eax_97;
                }
                
                data_4340d4 = eax_97;
                data_4340e8 = ((*(edx + 0xc) - *(ecx + 0xc)) * *((edi_10 << 2) + 0xfffffffc)) >> 8;
            }
            else
            {
                data_4340de = eax_30;
                data_4340f0 = (((*(edx + 0xc) - *(ebx_1 + 0xc)) * *((edi_2 << 2) + 0xfffffffc)
                    * data_43411c) >> 8) + (*(ebx_1 + 0xc) << 8);
                int32_t eax_38 = *edx - *ebx_1;
                int32_t eax_40;
                
                if (eax_38 <= 0)
                {
                    eax_40 = (eax_38 - 1) * *(edi_2 << 2);
                    *data_434120 = 0xffff;
                    data_4340dc = 0xffff;
                    data_434120 += eax_40;
                    *data_4340dc += eax_40;
                }
                else
                {
                    eax_40 = (eax_38 + 1) * *(edi_2 << 2);
                    *data_434120 = 0;
                    data_4340dc = 0;
                }
                
                data_434108 = eax_40;
                data_4340d4 = eax_40;
                int32_t eax_45 =
                    ((*(edx + 0xc) - *(ebx_1 + 0xc)) * *((edi_2 << 2) + 0xfffffffc)) >> 8;
                data_434110 = eax_45;
                data_4340e8 = eax_45;
                int32_t edi_4 = data_43411c + data_4366b8;
                int32_t eax_47 = *ecx - *ebx_1;
                int32_t eax_49;
                
                if (eax_47 >= 0)
                {
                    eax_49 = (eax_47 + 1) * *(edi_4 << 2);
                    *data_434126 = 0;
                    data_434126 += eax_49;
                }
                else
                {
                    eax_49 = (eax_47 - 1) * *(edi_4 << 2);
                    *data_434126 = 0xffff;
                }
                
                data_43410c = eax_49;
                data_434114 = ((*(ecx + 0xc) - *(ebx_1 + 0xc)) * *((edi_4 << 2) + 0xfffffffc)) >> 8;
                data_4340f4 = *(ecx + 0xc) << 8;
                data_4340e4 = *ecx;
                int32_t edi_6 = data_4340d0 + data_4366b8;
                int32_t eax_59 = *edx - *ecx;
                int32_t eax_61;
                
                if (eax_59 <= 0)
                {
                    eax_61 = (eax_59 - 1) * *(edi_6 << 2);
                    data_4340e2 = 0xffff;
                }
                else
                {
                    data_4340e2 = 0;
                    eax_61 = (eax_59 + 1) * *(edi_6 << 2);
                    *data_4340e2 += eax_61;
                }
                
                data_4340d8 = eax_61;
                data_4340ec = ((*(edx + 0xc) - *(ecx + 0xc)) * *((edi_6 << 2) + 0xfffffffc)) >> 8;
            }
            
            sub_40e79b(0x434094);
            data_434134 = data_4340cc;
            data_43411c = data_4340d0;
            *(data_434120 + 2) = data_4340de;
            *(data_434126 + 2) = data_4340e4;
            data_434108 = data_4340d4;
            data_43410c = data_4340d8;
            int32_t eax_108;
            eax_108 = data_4340dc;
            *data_434120 = eax_108;
            eax_108 = data_4340e2;
            *data_434126 = eax_108;
            data_434110 = data_4340e8;
            data_434114 = data_4340ec;
            data_43412c = data_4340f0;
            data_434130 = data_4340f4;
            sub_40e79b(0x434094);
        }
    }
    
    int32_t* esi_1 = *arg2;
    data_41cda0 = esi_1;
    
    if (!esi_1)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi_1] */
}

void sub_40e79b(void* arg1 @ esi)
{
    data_434124 = 0;
    data_43412a = 0;
    int32_t edi = data_434134;
    
    if (edi >= data_41cdb4)
        goto label_40e813;
    
    int32_t edi_2 = -((edi - data_41cdb4));
    int32_t temp2_1 = data_43411c;
    data_43411c -= edi_2;
    
    if (temp2_1 - edi_2 < 0)
        return;
    
    data_434126 += data_43410c * edi_2;
    data_434120 += data_434108 * edi_2;
    data_43412c += data_434110 * edi_2;
    data_434130 = &(data_434114 * edi_2)[data_434130];
    edi = data_41cdb4;
    data_434134 = edi;
label_40e813:
    int32_t edi_3 = edi + data_43411c;
    int32_t edi_4;
    int32_t temp3_1;
    
    if (edi_3 > data_41cdbc)
    {
        edi_4 = edi_3 - data_41cdbc;
        temp3_1 = data_43411c;
        data_43411c -= edi_4;
    }
    
    if (edi_3 > data_41cdbc && temp3_1 - edi_4 < 0)
        return;
    
    data_434104 = 0;
    data_4340fc = 0;
    data_434100 = data_434134 * data_41cdf0;
    void* edi_5 = &data_434138;
    int32_t temp6_1;
    
    do
    {
        int32_t eax_13 = data_434130;
        *(edi_5 + 8) = eax_13;
        int32_t ebx_1 = *(data_434126 + 2);
        
        if (ebx_1 != *(data_434120 + 2))
        {
            eax_13 = (data_43412c - data_434130) / (ebx_1 - *(data_434120 + 2));
            *(edi_5 + 0xc) = eax_13;
        }
        
        int32_t ebx_3 = *(data_434120 + 2);
        int32_t ecx_1 = *(data_434126 + 2);
        
        if (ebx_3 >= *data_41cdb0)
            goto label_40e8a7;
        
        if (ecx_1 >= *data_41cdb0)
        {
            ebx_3 = data_41cdb0;
        label_40e8a7:
            
            if (ecx_1 <= *data_41cdb8)
                goto label_40e8ce;
            
            if (ebx_3 <= *data_41cdb8)
            {
                *(edi_5 + 8) += eax_13 * (ecx_1 - data_41cdb8);
                ecx_1 = data_41cdb8;
            label_40e8ce:
                *(edi_5 + 4) = ecx_1 - ebx_3;
                int32_t eax_18 = ebx_3 + data_434100;
                int32_t eax_19 = eax_18 - data_434104;
                data_434104 = eax_18;
                *edi_5 = eax_19;
                edi_5 += 0x10;
                data_4340fc += 1;
            }
        }
        
        data_434100 += data_41cdf0;
        data_43412c += data_434110;
        data_434130 += data_434114;
        data_434126 += data_43410c;
        data_434120 = &data_434108[data_434120];
        temp6_1 = data_43411c;
        data_43411c -= 1;
    } while (temp6_1 - 1 >= 0);
    
    if (!data_4340fc)
        return;
    
    int32_t eax_24;
    *eax_24[1] = *(arg1 + 0x34);
    data_41cdac;
    void* edx_3 = &data_434138;
    char* edi_6 = data_41cda4;
    int32_t i;
    
    do
    {
        edi_6 = &edi_6[*edx_3];
        int32_t ebx_6 = RORD(*(edx_3 + 8), 8);
        int32_t esi_1 = RORD(*(edx_3 + 0xc), 8);
        int32_t ecx_3 = *(edx_3 + 4);
        void* temp7_1 = edx_3;
        edx_3 += 0x10;
        bool c_1 = temp7_1 >= 0xfffffff0;
        char* eax;
        eax = *ebx_6[1];
        int16_t temp9_1;
        
        do
        {
            eax = *eax;
            int32_t temp8_1 = ebx_6;
            ebx_6 = temp8_1 + esi_1;
            c_1 = temp8_1 + esi_1 < temp8_1 || (c_1 && temp8_1 + esi_1 == temp8_1);
            edi_6[ecx_3] = eax;
            temp9_1 = ecx_3;
            ecx_3 -= 1;
            eax = *ebx_6[1];
        } while (temp9_1 - 1 >= 0);
        i = data_4340fc;
        data_4340fc -= 1;
    } while (i != 1);
}

int32_t __stdcall sub_40e980(void* arg1 @ esi, int32_t* arg2 @ edi, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t eax_3 = *(arg1 + 4);
    int32_t ebx = *(arg1 + 8);
    
    if (eax_3 >= data_41cdd0 && eax_3 <= data_41cdd8 && ebx >= data_41cdd4 && ebx <= data_41cddc)
    {
        char* eax_2 = (eax_3 >> 8) + *((ebx >> 8 << 2) + &data_41cdf8) + data_41cda4;
        *(arg1 + 0x10);
        char* ebx_3;
        *ebx_3[1] = *(arg1 + 0xc);
        ebx_3 = *eax_2;
        ebx_3 = *ebx_3;
        *eax_2 = ebx_3;
    }
    
    int32_t* esi = *arg2;
    data_41cda0 = esi;
    
    if (!esi)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi] */
}

int32_t sub_40e9cc(int16_t arg1 @ x87control)
{
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    data_4366c8 = temp0;
    data_4366ca = data_4366c8 & 0xfcff;
    int16_t x87control;
    int16_t x87status_1;
    x87control = __fldcw_memmem16(data_4366ca);
    int32_t i;
    
    for (i = data_436708; i <= data_4366f8; i = data_436708)
    {
        long double x87_r7_3 = data_4366fc * data_436728 + data_436768;
        data_43679c = data_4366fc + data_43676c;
        data_436798 = x87_r7_3;
        long double x87_r7_6 = data_436708 * data_43672c + data_436714;
        data_4367c4 = data_436708 + data_436718;
        data_4367c0 = x87_r7_6;
        long double x87_r7_7 = data_436700;
        long double x87_r7_8 = x87_r7_7 / (x87_r7_7 * data_436734 + data_436730);
        int32_t eax_5;
        
        if (data_436758 & 0xffffffff)
        {
            eax_5 = data_436798;
            
            if (eax_5 < data_41cde0 || data_41cde8 <= data_4367c0
                    || !(((eax_5 >> 8) - (data_4367c0 >> 8)) & 0xffffffff))
                data_4366bc = x87_r7_8;
            else
            {
            label_40eae0:
                data_43677c = x87_r7_8 * data_436740 + data_436774;
                data_436780 = x87_r7_8 * data_436744 + data_436778;
                data_4367b0 = x87_r7_8 * data_436750 + data_436770;
                long double x87_r7_11 = data_43670c;
                long double x87_r7_12 = x87_r7_11 / (x87_r7_11 * data_43673c + data_436738);
                
                if (data_436758 & 0xffffffff)
                    data_4367c8 = data_436798 - data_4367c0;
                else
                    data_4367c8 = data_4367c0 - data_436798;
                
                data_436784 = x87_r7_12 * data_436748 + data_436720;
                data_436788 = x87_r7_12 * data_43674c + data_436724;
                data_4367b4 = x87_r7_12 * data_436754 + data_43671c;
                
                if (!(data_436758 & 0xffffffff))
                {
                    int32_t eax_12 = data_4367b0;
                    data_4367b0 = data_4367b4;
                    data_4367b4 = eax_12;
                    int32_t eax_13 = data_436798;
                    int32_t ebx_6 = data_43679c;
                    int32_t edx_1 = data_4367c4;
                    data_436798 = data_4367c0;
                    data_43679c = edx_1;
                    data_4367c0 = eax_13;
                    data_4367c4 = ebx_6;
                    int32_t eax_14 = data_43677c;
                    int32_t ebx_7 = data_436780;
                    int32_t edx_2 = data_436788;
                    data_43677c = data_436784;
                    data_436780 = edx_2;
                    data_436784 = eax_14;
                    data_436788 = ebx_7;
                }
                
                data_4367d4 = data_4367b4 - data_4367b0;
                long double x87_r7_20 = 1 / data_4367b0;
                int32_t ebx_9 = data_436788 - data_436780;
                data_4367b8 = data_436784 - data_43677c;
                data_4367bc = ebx_9;
                int32_t ebx_10 = data_4367c0;
                int32_t eax_22 =
                    data_436798 - (ebx_10 & 0xffffff00) - 0x100 + ((0x100 - ebx_10) & 0x100);
                data_4367ac = eax_22;
                data_4367cc = data_4367b4 * data_4367c8 * x87_r7_20;
                data_4367d0 = x87_r7_20 * -(data_4367d4);
                int32_t ebx_14 = data_4367c0;
                int32_t ecx_5 = data_41cde0;
                
                if (ebx_14 < ecx_5)
                {
                    data_4367ac = eax_22 - ((ecx_5 & 0xffffff00) - (ebx_14 & 0xffffff00));
                    data_4367c0 = data_41cde0;
                }
                
                long double x87_r7_22 = data_4367ac;
                long double x87_r7_23 = x87_r7_22 / (x87_r7_22 * data_4367d0 + data_4367cc);
                int32_t eax_24 = data_436798;
                int32_t eax_25 = eax_24 & 0xff;
                data_4367ac = eax_25;
                int32_t ecx_9 = data_41cde8;
                
                if (eax_24 >= ecx_9)
                {
                    data_4367ac = eax_25 + (eax_24 & 0xffffff00) - (ecx_9 & 0xffffff00);
                    data_436798 = ecx_9;
                }
                
                data_436784 = x87_r7_23 * data_4367b8 + data_43677c;
                data_436788 = x87_r7_23 * data_4367bc + data_436780;
                data_4367b4 = x87_r7_23 * data_4367d4 + data_4367b0;
                long double x87_r7_26 = data_4367ac;
                long double x87_r7_27 = x87_r7_26 / (x87_r7_26 * data_4367d0 + data_4367cc);
                int32_t eax_29 = (data_436798 & 0xffffff00) - 0x100;
                data_436798 = eax_29;
                data_4367a0 = (eax_29 >> 8) - (data_4367c0 >> 8);
                data_43677c = x87_r7_27 * data_4367b8 + data_43677c;
                data_436780 = x87_r7_27 * data_4367bc + data_436780;
                data_4367b0 = x87_r7_27 * data_4367d4 + data_4367b0;
                int32_t eax_32 = data_436798;
                
                if (eax_32 >= data_41cde0 && eax_32 < data_41cde8)
                {
                    int32_t eax_34 = data_43679c - 0x100;
                    
                    if (eax_34 >= data_41cde4 && eax_34 < data_41cdec)
                    {
                        data_43679c = eax_34;
                        long double x87_r7_31 = 1 / data_4367b0;
                        data_43678c = data_43677c << 0x10 | data_436780;
                        data_436794 = data_43675c;
                        data_4367a4 = data_4367b4 * data_4367a0 * x87_r7_31;
                        data_4367a8 = x87_r7_31 * (data_4367b0 - data_4367b4);
                        data_436790 = (data_43679c >> 8) * data_41cdf0 + data_41cda4;
                        
                        if (data_436764 & 1)
                        {
                            data_4366f0 = data_436784 - data_43677c;
                            data_4366f4 = data_436788 - data_436780;
                            
                            if (!(data_4366f4 & 0xffffffff))
                                data_4366f4 = 1;
                            
                            int32_t edx_30 = data_43678c;
                            int32_t esi_3 = data_436794;
                            int32_t j_6 = data_4367a0 + 1;
                            void* edi_7 = data_436790 + (data_436798 >> 8) + 1;
                            
                            if (j_6 < 0x10)
                                goto label_40f8a8;
                            
                            data_4366d4 = data_4366f0 / data_4366f4;
                            long double x87_r7_46 = data_4366f4;
                            long double x87_r6_42 = data_4367a4;
                            long double x87_r5_14 = data_4366d4;
                            long double x87_r4_2 = data_4367a8;
                            void* j_9 = edi_7 & 3;
                            data_4366bc = j_9;
                            long double x87_r3_8 = data_4366bc;
                            long double x87_r2_10 = data_4366c4;
                            data_4366e8 = j_6 - j_9;
                            void* j_7 = j_9;
                            long double x87_r1_21;
                            int80_t x87_r2_13;
                            long double x87_r3_10;
                            
                            if (!(j_7 & 3))
                            {
                                int32_t ecx_50 = data_4366e8;
                                uint32_t ebx_65 = ecx_50 >> 4;
                                int32_t ecx_51 = ecx_50 & 0xf;
                                
                                if (ecx_51 <= 0)
                                {
                                    ecx_51 = 0x10;
                                    ebx_65 -= 1;
                                }
                                
                                data_4366e8 = ebx_65;
                                data_4366ec = ecx_51;
                                
                                if (ebx_65 < 1)
                                    goto label_40f884;
                                
                                x87_r2_13 = x87_r2_10;
                                x87_r3_10 = x87_r3_8 + data_4366e4;
                                x87_r1_21 =
                                    x87_r3_10 * x87_r7_46 / (x87_r4_2 * x87_r3_10 + x87_r6_42);
                            label_40f62c:
                                data_4366bc = x87_r1_21 * x87_r5_14;
                                int32_t ebx_66 = data_4366bc;
                                data_4366bc = x87_r1_21;
                                int32_t ebx_67 = ebx_66 + data_43677c;
                                int16_t ecx_53 = data_4366bc + data_436780;
                                long double x87_r3_12 = x87_r2_13;
                                long double x87_r3_13 = x87_r3_10 + data_4366e4;
                                int32_t j;
                                
                                do
                                {
                                    data_4366bc = edx_30;
                                    uint32_t ecx_54 = ecx_53;
                                    data_4366d0 = ebx_67 << 0x10 | ecx_54;
                                    int32_t ebx_71 = (ebx_67 - (edx_30 >> 0x10)) >> 4 << 0x10
                                        | (ecx_54 - (edx_30 & 0xffff)) >> 4;
                                    int32_t edx_35;
                                    edx_35 = data_4366bc - ebx_71;
                                    int32_t ebx_72 = data_436760;
                                    int32_t edx_36 = edx_35 + ebx_71;
                                    uint32_t ecx_59;
                                    *ecx_59[1] = *edx_36[1];
                                    int32_t edx_37 = edx_36 + ebx_71;
                                    *ecx_59[1] = *(esi_3 + ecx_59);
                                    uint32_t eax_71;
                                    *eax_71[1] = *edx_37[1];
                                    ecx_59 = *(edi_7 - 1);
                                    *eax_71[1] = *(esi_3 + eax_71);
                                    eax_71 = *(edi_7 - 2);
                                    ecx_59 = *(ebx_72 + ecx_59);
                                    eax_71 = *(ebx_72 + eax_71);
                                    *(edi_7 - 1) = ecx_59;
                                    *(edi_7 - 2) = eax_71;
                                    int32_t edx_38 = edx_37 + ebx_71;
                                    uint32_t ecx_61;
                                    *ecx_61[1] = *edx_38[1];
                                    int32_t edx_39 = edx_38 + ebx_71;
                                    *ecx_61[1] = *(esi_3 + ecx_61);
                                    uint32_t eax_73;
                                    *eax_73[1] = *edx_39[1];
                                    ecx_61 = *(edi_7 - 3);
                                    *eax_73[1] = *(esi_3 + eax_73);
                                    eax_73 = *(edi_7 - 4);
                                    ecx_61 = *(ebx_72 + ecx_61);
                                    eax_73 = *(ebx_72 + eax_73);
                                    *(edi_7 - 3) = ecx_61;
                                    *(edi_7 - 4) = eax_73;
                                    int32_t edx_40 = edx_39 + ebx_71;
                                    uint32_t ecx_63;
                                    *ecx_63[1] = *edx_40[1];
                                    int32_t edx_41 = edx_40 + ebx_71;
                                    *ecx_63[1] = *(esi_3 + ecx_63);
                                    uint32_t eax_75;
                                    *eax_75[1] = *edx_41[1];
                                    ecx_63 = *(edi_7 - 5);
                                    *eax_75[1] = *(esi_3 + eax_75);
                                    long double x87_r1_27 =
                                        x87_r3_13 * x87_r7_46 / (x87_r3_13 * x87_r4_2 + x87_r6_42);
                                    eax_75 = *(edi_7 - 6);
                                    ecx_63 = *(ebx_72 + ecx_63);
                                    eax_75 = *(ebx_72 + eax_75);
                                    *(edi_7 - 5) = ecx_63;
                                    *(edi_7 - 6) = eax_75;
                                    int32_t edx_42 = edx_41 + ebx_71;
                                    uint32_t ecx_65;
                                    *ecx_65[1] = *edx_42[1];
                                    int32_t edx_43 = edx_42 + ebx_71;
                                    *ecx_65[1] = *(esi_3 + ecx_65);
                                    uint32_t eax_77;
                                    *eax_77[1] = *edx_43[1];
                                    ecx_65 = *(edi_7 - 7);
                                    *eax_77[1] = *(esi_3 + eax_77);
                                    eax_77 = *(edi_7 - 8);
                                    ecx_65 = *(ebx_72 + ecx_65);
                                    eax_77 = *(ebx_72 + eax_77);
                                    *(edi_7 - 7) = ecx_65;
                                    *(edi_7 - 8) = eax_77;
                                    int32_t edx_44 = edx_43 + ebx_71;
                                    uint32_t ecx_67;
                                    *ecx_67[1] = *edx_44[1];
                                    int32_t edx_45 = edx_44 + ebx_71;
                                    *ecx_67[1] = *(esi_3 + ecx_67);
                                    uint32_t eax_79;
                                    *eax_79[1] = *edx_45[1];
                                    ecx_67 = *(edi_7 - 9);
                                    *eax_79[1] = *(esi_3 + eax_79);
                                    eax_79 = *(edi_7 - 0xa);
                                    ecx_67 = *(ebx_72 + ecx_67);
                                    eax_79 = *(ebx_72 + eax_79);
                                    *(edi_7 - 9) = ecx_67;
                                    *(edi_7 - 0xa) = eax_79;
                                    int32_t edx_46 = edx_45 + ebx_71;
                                    uint32_t ecx_69;
                                    *ecx_69[1] = *edx_46[1];
                                    int32_t edx_47 = edx_46 + ebx_71;
                                    *ecx_69[1] = *(esi_3 + ecx_69);
                                    uint32_t eax_81;
                                    *eax_81[1] = *edx_47[1];
                                    ecx_69 = *(edi_7 - 0xb);
                                    *eax_81[1] = *(esi_3 + eax_81);
                                    eax_81 = *(edi_7 - 0xc);
                                    ecx_69 = *(ebx_72 + ecx_69);
                                    eax_81 = *(ebx_72 + eax_81);
                                    *(edi_7 - 0xb) = ecx_69;
                                    *(edi_7 - 0xc) = eax_81;
                                    int32_t edx_48 = edx_47 + ebx_71;
                                    uint32_t ecx_71;
                                    *ecx_71[1] = *edx_48[1];
                                    int32_t edx_49 = edx_48 + ebx_71;
                                    *ecx_71[1] = *(esi_3 + ecx_71);
                                    uint32_t eax_83;
                                    *eax_83[1] = *edx_49[1];
                                    ecx_71 = *(edi_7 - 0xd);
                                    *eax_83[1] = *(esi_3 + eax_83);
                                    eax_83 = *(edi_7 - 0xe);
                                    ecx_71 = *(ebx_72 + ecx_71);
                                    eax_83 = *(ebx_72 + eax_83);
                                    *(edi_7 - 0xd) = ecx_71;
                                    *(edi_7 - 0xe) = eax_83;
                                    data_4366bc = x87_r1_27 * x87_r5_14 + x87_r3_12;
                                    x87_r3_13 = x87_r3_13 + data_4366e4;
                                    int32_t edx_50 = edx_49 + ebx_71;
                                    uint32_t ecx_73;
                                    *ecx_73[1] = *edx_50[1];
                                    *ecx_73[1] = *(esi_3 + ecx_73);
                                    uint32_t eax_85;
                                    *eax_85[1] = *(edx_50 + ebx_71)[1];
                                    ecx_73 = *(edi_7 - 0xf);
                                    *eax_85[1] = *(esi_3 + eax_85);
                                    eax_85 = *(edi_7 - 0x10);
                                    edx_30 = data_4366d0;
                                    ecx_73 = *(ebx_72 + ecx_73);
                                    eax_85 = *(ebx_72 + eax_85);
                                    *(edi_7 - 0xf) = ecx_73;
                                    *(edi_7 - 0x10) = eax_85;
                                    ebx_67 = data_4366bc + data_43677c;
                                    data_4366bc = x87_r1_27 + x87_r3_12;
                                    edi_7 -= 0x10;
                                    ecx_53 = data_4366bc + data_436780;
                                    j = data_4366e8;
                                    data_4366e8 -= 1;
                                } while (j != 1);
                                
                                if (data_4366ec < 0x11)
                                    goto label_40f884;
                            }
                            else
                            {
                                long double x87_r1_18 =
                                    x87_r7_46 / (x87_r4_2 * x87_r3_8 + x87_r6_42);
                                data_4366bc = x87_r1_18 * x87_r5_14;
                                int32_t edx_31 = data_4366bc;
                                data_4366bc = x87_r1_18;
                                int32_t edx_33 = edx_31 << 0x10 | data_4366bc;
                                edx_30 = data_43678c;
                                x87_r2_13 = x87_r2_10;
                                x87_r3_10 = x87_r3_8 + data_4366e4;
                                x87_r1_21 =
                                    x87_r3_10 * x87_r7_46 / (x87_r4_2 * x87_r3_10 + x87_r6_42);
                                int32_t ebx_61 = data_436760;
                                void* j_1;
                                
                                do
                                {
                                    uint32_t eax_64;
                                    *eax_64[1] = *edx_30[1];
                                    *eax_64[1] = *(esi_3 + eax_64);
                                    eax_64 = *(edi_7 - 1);
                                    edx_30 += edx_33;
                                    eax_64 = *(ebx_61 + eax_64);
                                    *(edi_7 - 1) = eax_64;
                                    edi_7 -= 1;
                                    j_1 = j_7;
                                    j_7 -= 1;
                                } while (j_1 != 1);
                                int32_t ecx_48 = data_4366e8;
                                uint32_t ebx_63 = ecx_48 >> 4;
                                int32_t ecx_49 = ecx_48 & 0xf;
                                
                                if (ecx_49 <= 0)
                                {
                                    ecx_49 = 0x10;
                                    ebx_63 -= 1;
                                }
                                
                                data_4366e8 = ebx_63;
                                data_4366ec = ecx_49;
                                
                                if (ebx_63 >= 1)
                                    goto label_40f62c;
                                
                                data_4366bc = x87_r1_21;
                            label_40f884:
                                uint32_t eax_88 = edx_30 - data_43678c;
                                data_4366f0 -= (edx_30 >> 0x10) - (data_43678c >> 0x10);
                                data_4366f4 -= eax_88;
                                data_43678c = edx_30;
                                j_6 = data_4366ec;
                            label_40f8a8:
                                /* unimplemented  {fild st0, dword [&data_4366f0]} */
                                /* unimplemented  {fmul st0, dword [ebx+ecx*4]} */
                                /* unimplemented  {fild st0, dword [&data_4366f4]} */
                                /* unimplemented  {fmul st0, dword [ebx+ecx*4]} */
                                void* edi_9 = edi_7 - j_6 - 1;
                                /* unimplemented  {fxch st0, st1} */
                                /* unimplemented  {fxch st0, st1} */
                                data_4366bc = /* data_4366bc =
                                    unimplemented  {fistp dword [&data_4366bc], st0} */;
                                /* unimplemented  {fistp dword [&data_4366bc], st0} */
                                int32_t esi_4 = data_436794;
                                int32_t ebx_77 = data_4366bc;
                                data_4366bc = /* data_4366bc =
                                    unimplemented  {fistp dword [&data_4366bc], st0} */;
                                /* unimplemented  {fistp dword [&data_4366bc], st0} */
                                int32_t ebp_8 = ebx_77 << 0x10 | data_4366bc;
                                int32_t edx_54 = data_43678c;
                                int32_t ebx_79 = data_436760;
                                int32_t j_2;
                                
                                do
                                {
                                    uint32_t eax_90;
                                    *eax_90[1] = *edx_54[1];
                                    *eax_90[1] = *(esi_4 + eax_90);
                                    eax_90 = *(edi_9 + j_6);
                                    edx_54 += ebp_8;
                                    eax_90 = *(ebx_79 + eax_90);
                                    *(edi_9 + j_6) = eax_90;
                                    j_2 = j_6;
                                    j_6 -= 1;
                                } while (j_2 > 1);
                            }
                            int16_t x87control_3;
                            int16_t x87status_4;
                            x87control_3 = __fldcw_memmem16(data_4366c8);
                        }
                        else
                        {
                            data_4366f0 = data_436784 - data_43677c;
                            data_4366f4 = data_436788 - data_436780;
                            
                            if (!(data_4366f4 & 0xffffffff))
                                data_4366f4 = 1;
                            
                            int32_t edx_4 = data_43678c;
                            int32_t esi_1 = data_436794;
                            int32_t ecx_12 = data_4367a0 + 1;
                            void* edi_3 = data_436790 + (data_436798 >> 8) + 1;
                            
                            if (ecx_12 < 0x10)
                                goto label_40f321;
                            
                            data_4366d4 = data_4366f0 / data_4366f4;
                            long double x87_r7_39 = data_4366f4;
                            long double x87_r6_38 = data_4367a4;
                            long double x87_r5_13 = data_4366d4;
                            long double x87_r4_1 = data_4367a8;
                            void* j_8 = edi_3 & 3;
                            data_4366bc = j_8;
                            long double x87_r3_1 = data_4366bc;
                            long double x87_r2_1 = data_4366c4;
                            data_4366e8 = ecx_12 - j_8;
                            void* j_5 = j_8;
                            long double x87_r1_5;
                            int80_t x87_r2_4;
                            long double x87_r3_3;
                            
                            if (!(j_5 & 3))
                            {
                                int32_t ecx_16 = data_4366e8;
                                uint32_t ebx_28 = ecx_16 >> 4;
                                int32_t ecx_17 = ecx_16 & 0xf;
                                
                                if (ecx_17 <= 0)
                                {
                                    ecx_17 = 0x10;
                                    ebx_28 -= 1;
                                }
                                
                                data_4366e8 = ebx_28;
                                data_4366ec = ecx_17;
                                
                                if (ebx_28 < 1)
                                    goto label_40f2fd;
                                
                                x87_r2_4 = x87_r2_1;
                                x87_r3_3 = x87_r3_1 + data_4366e4;
                                x87_r1_5 = x87_r3_3 * x87_r7_39 / (x87_r4_1 * x87_r3_3 + x87_r6_38);
                            }
                            else
                            {
                                long double x87_r1_2 =
                                    x87_r7_39 / (x87_r4_1 * x87_r3_1 + x87_r6_38);
                                data_4366bc = x87_r1_2 * x87_r5_13;
                                int32_t edx_5 = data_4366bc;
                                data_4366bc = x87_r1_2;
                                x87_r2_4 = x87_r2_1;
                                x87_r3_3 = x87_r3_1 + data_4366e4;
                                int32_t edx_7 = edx_5 << 0x10 | data_4366bc;
                                edx_4 = data_43678c;
                                x87_r1_5 = x87_r3_3 * x87_r7_39 / (x87_r4_1 * x87_r3_3 + x87_r6_38);
                                void* j_3;
                                
                                do
                                {
                                    uint32_t ebx_24;
                                    *ebx_24[1] = *edx_4[1];
                                    j_8 = *(esi_1 + ebx_24);
                                    edx_4 += edx_7;
                                    *(edi_3 - 1) = j_8;
                                    edi_3 -= 1;
                                    j_3 = j_5;
                                    j_5 -= 1;
                                } while (j_3 != 1);
                                int32_t ecx_14 = data_4366e8;
                                uint32_t ebx_26 = ecx_14 >> 4;
                                int32_t ecx_15 = ecx_14 & 0xf;
                                
                                if (ecx_15 <= 0)
                                {
                                    ecx_15 = 0x10;
                                    ebx_26 -= 1;
                                }
                                
                                data_4366e8 = ebx_26;
                                data_4366ec = ecx_15;
                                
                                if (ebx_26 < 1)
                                {
                                    data_4366bc = x87_r1_5;
                                label_40f2fd:
                                    uint32_t eax_57 = edx_4 - data_43678c;
                                    data_4366f0 -= (edx_4 >> 0x10) - (data_43678c >> 0x10);
                                    data_4366f4 -= eax_57;
                                    data_43678c = edx_4;
                                    ecx_12 = data_4366ec;
                                label_40f321:
                                    /* unimplemented  {fild st0, dword [&data_4366f0]} */
                                    /* unimplemented  {fmul st0, dword [ebx+ecx*4]} */
                                    /* unimplemented  {fild st0, dword [&data_4366f4]} */
                                    /* unimplemented  {fmul st0, dword [ebx+ecx*4]} */
                                    int32_t eax_58 = 0x40f386 + *(&data_436828 + (ecx_12 << 2));
                                    /* unimplemented  {fxch st0, st1} */
                                    /* unimplemented  {fxch st0, st1} */
                                    data_4366bc = /* data_4366bc =
                                        unimplemented  {fistp dword [&data_4366bc], st0} */;
                                    /* unimplemented  {fistp dword [&data_4366bc], st0} */
                                    data_436794;
                                    data_4366bc;
                                    data_4366bc = /* data_4366bc =
                                        unimplemented  {fistp dword [&data_4366bc], st0} */;
                                    /* unimplemented  {fistp dword [&data_4366bc], st0} */
                                    data_4366bc;
                                    data_43678c;
                                    data_43678c;
                                    uint32_t ebx_58;
                                    ebx_58 = (*(data_43678c + 3));
                                    /* jump -> eax_58 */
                                }
                            }
                            
                            data_4366bc = x87_r1_5 * x87_r5_13;
                            int32_t ebx_29 = data_4366bc;
                            data_4366bc = x87_r1_5;
                            int32_t ebx_30 = ebx_29 + data_43677c;
                            int16_t ecx_19 = data_4366bc + data_436780;
                            long double x87_r3_5 = x87_r2_4;
                            long double x87_r3_6 = x87_r3_3 + data_4366e4;
                            int32_t j_4;
                            
                            do
                            {
                                data_4366bc = edx_4;
                                uint32_t ecx_20 = ecx_19;
                                data_4366d0 = ebx_30 << 0x10 | ecx_20;
                                int32_t ebx_34 = (ebx_30 - (edx_4 >> 0x10)) >> 4 << 0x10
                                    | (ecx_20 - (edx_4 & 0xffff)) >> 4;
                                int32_t edx_9;
                                edx_9 = data_4366bc - ebx_34;
                                int32_t edx_10 = edx_9 + ebx_34;
                                uint32_t ebx_36;
                                *ebx_36[1] = *edx_10[1];
                                int32_t edx_11 = edx_10 + ebx_34;
                                uint32_t eax_50;
                                *eax_50[1] = *(esi_1 + ebx_36);
                                uint32_t ecx_25;
                                *ecx_25[1] = *edx_11[1];
                                int32_t edx_12 = edx_11 + ebx_34;
                                eax_50 = *(esi_1 + ecx_25);
                                uint32_t ebx_38;
                                *ebx_38[1] = *edx_12[1];
                                long double x87_r1_11 =
                                    x87_r3_6 * x87_r7_39 / (x87_r3_6 * x87_r4_1 + x87_r6_38);
                                int32_t edx_13 = edx_12 + ebx_34;
                                uint32_t eax_51;
                                *eax_51[1] = *(esi_1 + ebx_38);
                                uint32_t ecx_27;
                                *ecx_27[1] = *edx_13[1];
                                int32_t edx_14 = edx_13 + ebx_34;
                                eax_51 = *(esi_1 + ecx_27);
                                *(edi_3 - 4) = eax_51;
                                uint32_t ebx_40;
                                *ebx_40[1] = *edx_14[1];
                                int32_t edx_15 = edx_14 + ebx_34;
                                *eax_51[1] = *(esi_1 + ebx_40);
                                uint32_t ecx_29;
                                *ecx_29[1] = *edx_15[1];
                                int32_t edx_16 = edx_15 + ebx_34;
                                eax_51 = *(esi_1 + ecx_29);
                                uint32_t ebx_42;
                                *ebx_42[1] = *edx_16[1];
                                int32_t edx_17 = edx_16 + ebx_34;
                                uint32_t eax_52;
                                *eax_52[1] = *(esi_1 + ebx_42);
                                uint32_t ecx_31;
                                *ecx_31[1] = *edx_17[1];
                                int32_t edx_18 = edx_17 + ebx_34;
                                eax_52 = *(esi_1 + ecx_31);
                                uint32_t ebx_44;
                                *ebx_44[1] = *edx_18[1];
                                *(edi_3 - 8) = eax_52;
                                int32_t edx_19 = edx_18 + ebx_34;
                                *eax_52[1] = *(esi_1 + ebx_44);
                                uint32_t ecx_33;
                                *ecx_33[1] = *edx_19[1];
                                int32_t edx_20 = edx_19 + ebx_34;
                                eax_52 = *(esi_1 + ecx_33);
                                uint32_t ebx_46;
                                *ebx_46[1] = *edx_20[1];
                                data_4366bc = x87_r1_11 * x87_r5_13 + x87_r3_5;
                                int32_t edx_21 = edx_20 + ebx_34;
                                x87_r3_6 = x87_r3_6 + data_4366e4;
                                uint32_t eax_53;
                                *eax_53[1] = *(esi_1 + ebx_46);
                                uint32_t ecx_35;
                                *ecx_35[1] = *edx_21[1];
                                int32_t edx_22 = edx_21 + ebx_34;
                                eax_53 = *(esi_1 + ecx_35);
                                *(edi_3 - 0xc) = eax_53;
                                uint32_t ebx_48;
                                *ebx_48[1] = *edx_22[1];
                                int32_t edx_23 = edx_22 + ebx_34;
                                *eax_53[1] = *(esi_1 + ebx_48);
                                uint32_t ecx_37;
                                *ecx_37[1] = *edx_23[1];
                                int32_t edx_24 = edx_23 + ebx_34;
                                eax_53 = *(esi_1 + ecx_37);
                                uint32_t ebx_50;
                                *ebx_50[1] = *edx_24[1];
                                uint32_t eax_54;
                                *eax_54[1] = *(esi_1 + ebx_50);
                                ebx_30 = data_4366bc + data_43677c;
                                uint32_t ecx_39;
                                *ecx_39[1] = *(edx_24 + ebx_34)[1];
                                edx_4 = data_4366d0;
                                data_4366bc = x87_r1_11 + x87_r3_5;
                                eax_54 = *(esi_1 + ecx_39);
                                ecx_19 = data_4366bc + data_436780;
                                *(edi_3 - 0x10) = eax_54;
                                edi_3 -= 0x10;
                                j_4 = data_4366e8;
                                data_4366e8 -= 1;
                            } while (j_4 != 1);
                            
                            if (data_4366ec < 0x11)
                                goto label_40f2fd;
                            
                            int16_t x87control_2;
                            int16_t x87status_3;
                            x87control_2 = __fldcw_memmem16(data_4366c8);
                        }
                    }
                }
            }
        }
        else
        {
            eax_5 = data_436798;
            
            if (eax_5 < data_41cde8 && data_41cde0 <= data_4367c0
                    && ((eax_5 >> 8) - (data_4367c0 >> 8)) & 0xffffffff)
                goto label_40eae0;
            
            data_4366bc = x87_r7_8;
        }
        data_4366fc += 0x100;
        data_436708 += 0x100;
        data_436700 = data_4366fc * data_436704;
        data_43670c = data_436708 * data_436710;
    }
    
    int16_t x87control_1;
    int16_t x87status_2;
    x87control_1 = __fldcw_memmem16(data_4366c8);
    return i;
}

int32_t sub_40f9dc(void* arg1 @ esi)
{
    *(arg1 + 0x20);
    int32_t edx;
    *edx[1] = *(arg1 + 0x1c);
    data_4368a0 = edx;
    void* ecx = arg1 + 0xc;
    void* ebx = arg1 + 4;
    int32_t eax = *(ecx + 4);
    void* edx_1 = arg1 + 0x14;
    
    if (eax < *(ebx + 4))
    {
        int32_t eax_2 = *(edx_1 + 4);
        void* temp0_3 = ecx;
        ecx = ebx;
        ebx = temp0_3;
        
        if (eax_2 <= *(ecx + 4))
        {
            void* temp0_4 = edx_1;
            edx_1 = ecx;
            ecx = temp0_4;
            
            if (eax_2 <= *(ebx + 4))
            {
                void* temp0_5 = ecx;
                ecx = ebx;
                ebx = temp0_5;
            }
        }
    }
    else if (eax >= *(edx_1 + 4))
    {
        void* temp0_1 = edx_1;
        edx_1 = ecx;
        ecx = temp0_1;
        
        if (*(ebx + 4) >= *(ecx + 4))
        {
            void* temp0_2 = ecx;
            ecx = ebx;
            ebx = temp0_2;
        }
    }
    
    int32_t result = *ebx;
    
    if (result >= 0xff830000 && result <= 0x7d0000)
    {
        data_4368a8 = result;
        
        if (result >= 0xff830000)
        {
            data_4368ac = *(ebx + 4);
            result = *ecx;
            
            if (result >= 0xff830000 && result <= 0x7d0000)
            {
                data_4368b0 = result;
                data_4368b4 = *(ecx + 4);
                result = *edx_1;
                
                if (result >= 0xff830000 && result <= 0x7d0000)
                {
                    data_4368b8 = result;
                    result = *(edx_1 + 4);
                    
                    if (result <= 0x7d0000)
                    {
                        data_4368bc = result;
                        int32_t ebx_2 = data_4368ac >> 8;
                        int32_t eax_6 = data_4368b4 >> 8;
                        data_436888 = ebx_2;
                        
                        if (eax_6 != ebx_2)
                        {
                            data_43688c = eax_6 - ebx_2;
                            sub_40fc98(&data_4368a8, &data_4368b0);
                            sub_40fcfe(&data_4368a8, &data_4368b8);
                            int32_t eax_8 = data_436898;
                            int32_t ebx_3 = data_43689c;
                            
                            if (eax_8 > ebx_3)
                            {
                                data_436898 = ebx_3;
                                data_43689c = eax_8;
                                int32_t eax_9 = data_436890;
                                data_436890 = data_436894;
                                data_436894 = eax_9;
                            }
                            
                            sub_40fba0();
                        }
                        
                        int32_t ebx_6 = data_4368b4 >> 8;
                        int32_t eax_11 = data_4368bc >> 8;
                        data_436888 = ebx_6;
                        result = eax_11 - ebx_6;
                        
                        if (eax_11 != ebx_6)
                        {
                            data_43688c = result;
                            sub_40fc98(&data_4368a8, &data_4368b8);
                            sub_40fcfe(&data_4368b0, &data_4368b8);
                            data_436890 += ((data_4368b4 >> 8) - (data_4368ac >> 8)) * data_436898;
                            int32_t eax_16 = data_436898;
                            int32_t ebx_9 = data_43689c;
                            
                            if (eax_16 < ebx_9)
                            {
                                data_436898 = ebx_9;
                                data_43689c = eax_16;
                                int32_t eax_17 = data_436890;
                                data_436890 = data_436894;
                                data_436894 = eax_17;
                            }
                            
                            return sub_40fba0();
                        }
                    }
                }
            }
        }
    }
    
    return result;
}

int32_t sub_40fba0()
{
    int32_t eax = data_436890;
    int32_t ebx = data_436894;
    int32_t edx = data_436888;
    
    if (edx >= data_41cdb4)
        goto label_40fbed;
    
    int32_t ecx_2 = data_41cdb4 - edx;
    int32_t temp0_1 = data_43688c;
    data_43688c -= ecx_2;
    
    if (temp0_1 > ecx_2)
    {
        eax += ecx_2 * data_436898;
        ebx += ecx_2 * data_43689c;
        edx = data_41cdb4;
    label_40fbed:
        int32_t ecx_5 = data_41cdcc - edx;
        
        if (ecx_5 >= data_43688c)
            goto label_40fc1f;
        
        if (edx <= data_41cdcc && ecx_5)
        {
            data_43688c = ecx_5;
        label_40fc1f:
            int32_t ebp_3 = *((edx << 2) + &data_41cdf8) + data_41cda4 - 1;
            data_4368a0;
            int32_t edi_1 = eax;
            int32_t i;
            
            do
            {
                int32_t ecx_7 = ebx >> 0x10;
                int32_t esi_2 = edi_1 >> 0x10;
                
                if (esi_2 >= data_41cdb0)
                    goto label_40fc48;
                
                if (ecx_7 >= data_41cdb0)
                {
                    esi_2 = data_41cdb0;
                label_40fc48:
                    
                    if (ecx_7 <= data_41cdc8)
                        goto label_40fc5e;
                    
                    if (esi_2 <= data_41cdc8)
                    {
                        ecx_7 = data_41cdc8;
                    label_40fc5e:
                        int32_t ecx_8 = ecx_7 - esi_2;
                        
                        if (ecx_7 > esi_2)
                        {
                            char* esi_3 = esi_2 + ebp_3;
                            char* edx_3;
                            edx_3 = esi_3[ecx_8];
                            int32_t j_1 = ecx_8 - 1;
                            
                            if (ecx_8 != 1)
                            {
                                int32_t j;
                                
                                do
                                {
                                    eax = *edx_3;
                                    edx_3 = esi_3[j_1];
                                    esi_3[j_1 + 1] = eax;
                                    j = j_1;
                                    j_1 -= 1;
                                } while (j != 1);
                            }
                            
                            eax = *edx_3;
                            esi_3[1] = eax;
                        }
                    }
                }
                
                edi_1 += data_436898;
                ebx += data_43689c;
                ebp_3 += data_41cdf0;
                i = data_43688c;
                data_43688c -= 1;
            } while (i != 1);
            return edi_1;
        }
    }
    
    return eax;
}

int32_t sub_40fc98(int32_t* arg1 @ esi, int32_t* arg2 @ edi)
{
    int32_t ecx = arg2[1] - arg1[1];
    
    if (ecx <= 0x40)
    {
        int32_t eax_1 = *arg2;
        int32_t eax_2 = eax_1 - *arg1;
        
        if (eax_1 <= *arg1)
            eax_2 = -(eax_2);
        
        if (eax_2 <= 0x10000)
            ecx = 0x41;
        else if (ecx <= 3)
            ecx = 3;
    }
    
    int32_t eax_4 = *arg2 - *arg1;
    data_436898 = COMBINE(eax_4 >> 0x10, eax_4 << 0x10) / ecx;
    int32_t result = data_436898 >> 8;
    data_436890 = (0x100 - arg1[1]) * result + (*arg1 << 8);
    return result;
}

int32_t sub_40fcfe(int32_t* arg1 @ esi, int32_t* arg2 @ edi)
{
    int32_t ecx = arg2[1] - arg1[1];
    
    if (ecx <= 0x40)
    {
        int32_t eax_1 = *arg2;
        int32_t eax_2 = eax_1 - *arg1;
        
        if (eax_1 <= *arg1)
            eax_2 = -(eax_2);
        
        if (eax_2 <= 0x10000)
            ecx = 0x41;
        else if (ecx <= 3)
            ecx = 3;
    }
    
    int32_t eax_4 = *arg2 - *arg1;
    data_43689c = COMBINE(eax_4 >> 0x10, eax_4 << 0x10) / ecx;
    int32_t result = data_43689c >> 8;
    data_436894 = (0x100 - arg1[1]) * result + (*arg1 << 8);
    return result;
}

void __convention("regparm") sub_40fd64(int32_t arg1)
{
    data_43a7ac = arg1 + 1;
    data_43a7b0 = -(arg1) + 1;
}

int32_t __stdcall sub_40fd7b(void* arg1 @ esi, int32_t* arg2 @ edi, int32_t arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t eax = *(arg1 + 4) >> 8;
    int32_t ecx_1 = *(arg1 + 0x10) >> 8;
    int32_t ebx = *(arg1 + 0x20);
    int32_t edx = *(arg1 + 0x24);
    
    if (eax > ecx_1)
    {
        int32_t temp0_1 = edx;
        edx = ebx;
        ebx = temp0_1;
    }
    
    data_4368d8 = edx - ebx;
    data_43a7bc = ebx;
    *ebx[1] = *(arg1 + 0x1c);
    data_4368d4 = ebx + data_41cdac;
    int32_t ebx_3 = *(arg1 + 8) >> 8;
    int32_t edx_3 = *(arg1 + 0x14) >> 8;
    
    if (eax > ecx_1)
    {
        int32_t temp0_2 = ecx_1;
        ecx_1 = eax;
        eax = temp0_2;
        int32_t temp0_3 = edx_3;
        edx_3 = ebx_3;
        ebx_3 = temp0_3;
    }
    
    if (eax >= data_41cdb0)
        goto label_40fdcf;
    
    if (ecx_1 >= data_41cdb0)
    {
        data_4368c0 = eax;
        data_4368c4 = ebx_3;
        data_4368c8 = ecx_1;
        data_4368cc = edx_3;
        data_4368d0 =
            (COMBINE(data_4368c8 - data_41cdb0 + 1, 0) / (data_4368c8 - data_4368c0 + 1)) >> 1;
        int32_t eax_32 = data_4368cc - data_4368c4;
        int32_t eax_34;
        int32_t edx_29;
        edx_29 = HIGHD(eax_32 * 2 * data_4368d0);
        eax_34 = LOWD(eax_32 * 2 * data_4368d0);
        data_4368c4 += eax_32 - edx_29;
        data_4368c0 = data_41cdb0;
        int32_t ebx_16 = data_43a7bc + data_4368d8;
        int32_t eax_38;
        int32_t edx_30;
        edx_30 = HIGHD((data_4368d8 << 1) * data_4368d0);
        eax_38 = LOWD((data_4368d8 << 1) * data_4368d0);
        data_4368d8 = edx_30;
        data_43a7bc = ebx_16 - edx_30;
        eax = data_4368c0;
        ebx_3 = data_4368c4;
        ecx_1 = data_4368c8;
        edx_3 = data_4368cc;
    label_40fdcf:
        
        if (ecx_1 <= data_41cdb8)
            goto label_40fddb;
        
        if (eax <= data_41cdb8)
        {
            data_4368c0 = eax;
            data_4368c4 = ebx_3;
            data_4368c8 = ecx_1;
            data_4368cc = edx_3;
            data_4368d0 =
                (COMBINE(data_41cdb8 - data_4368c0 + 1, 0) / (data_4368c8 - data_4368c0 + 1)) >> 1;
            int32_t eax_23;
            int32_t edx_23;
            edx_23 = HIGHD((data_4368cc - data_4368c4) * 2 * data_4368d0);
            eax_23 = LOWD((data_4368cc - data_4368c4) * 2 * data_4368d0);
            data_4368cc = edx_23 + data_4368c4;
            data_4368c8 = data_41cdb8;
            int32_t eax_27;
            int32_t edx_25;
            edx_25 = HIGHD((data_4368d8 << 1) * data_4368d0);
            eax_27 = LOWD((data_4368d8 << 1) * data_4368d0);
            data_4368d8 = edx_25;
            eax = data_4368c0;
            ebx_3 = data_4368c4;
            ecx_1 = data_4368c8;
            edx_3 = data_4368cc;
        label_40fddb:
            
            if (ebx_3 > edx_3)
            {
                if (ebx_3 <= data_41cdbc)
                    goto label_40fe04;
                
                if (edx_3 <= data_41cdbc)
                {
                    data_4368c0 = eax;
                    data_4368c4 = ebx_3;
                    data_4368c8 = ecx_1;
                    data_4368cc = edx_3;
                    data_4368d0 = (COMBINE(data_41cdbc - data_4368cc + 1, 0)
                        / (data_4368c4 - data_4368cc + 1)) >> 1;
                    int32_t eax_56;
                    int32_t edx_40;
                    edx_40 = HIGHD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                    eax_56 = LOWD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                    data_4368c0 = -(edx_40) + data_4368c8;
                    data_4368c4 = data_41cdbc;
                    int32_t ebx_25 = data_43a7bc + data_4368d8;
                    int32_t eax_60;
                    int32_t edx_43;
                    edx_43 = HIGHD((data_4368d8 << 1) * data_4368d0);
                    eax_60 = LOWD((data_4368d8 << 1) * data_4368d0);
                    data_4368d8 = edx_43;
                    data_43a7bc = ebx_25 - edx_43;
                    eax = data_4368c0;
                    ebx_3 = data_4368c4;
                    ecx_1 = data_4368c8;
                    edx_3 = data_4368cc;
                label_40fe04:
                    
                    if (edx_3 >= data_41cdb4)
                        goto label_40fe10;
                    
                    if (ebx_3 >= data_41cdb4)
                    {
                        data_4368c0 = eax;
                        data_4368c4 = ebx_3;
                        data_4368c8 = ecx_1;
                        data_4368cc = edx_3;
                        data_4368d0 = (COMBINE(data_4368c4 - data_41cdb4 + 1, 0)
                            / (data_4368c4 - data_4368cc + 1)) >> 1;
                        int32_t eax_45;
                        int32_t edx_34;
                        edx_34 = HIGHD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                        eax_45 = LOWD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                        data_4368c8 = edx_34 + data_4368c0;
                        data_4368cc = data_41cdb4;
                        int32_t eax_49;
                        int32_t edx_36;
                        edx_36 = HIGHD((data_4368d8 << 1) * data_4368d0);
                        eax_49 = LOWD((data_4368d8 << 1) * data_4368d0);
                        data_4368d8 = edx_36;
                        eax = data_4368c0;
                        ebx_3 = data_4368c4;
                        ecx_1 = data_4368c8;
                        edx_3 = data_4368cc;
                    label_40fe10:
                        *((ebx_3 << 2) + &data_41cdf8);
                        data_41cda4;
                        int32_t edx_4 = edx_3 - ebx_3;
                        
                        if (edx_4 < 0)
                        {
                            if (ecx_1 - eax + 1 > -(edx_4) + 1)
                            {
                                data_43a7b6 = (-(edx_4) + 1);
                                data_43a7ba = data_4368d8 / (ecx_1 - eax + 1);
                                uint16_t eax_16 = (ecx_1 - eax + 1);
                                int16_t temp2_7 = (-(edx_4) + 1);
                                data_43a7b8 = COMBINE(0, eax_16) / temp2_7;
                                data_43a7b4 =
                                    COMBINE(COMBINE(0, eax_16) % temp2_7, 0) / (-(edx_4) + 1);
                                int32_t ebx_6;
                                ebx_6 = data_43a7b8;
                                data_4368d4;
                                /* jump -> *((ebx_6 << 2) + &data_4391b8) */
                            }
                            
                            data_43a7ba = data_4368d8 / (-(edx_4) + 1);
                            uint16_t eax_12 = (-(edx_4) + 1);
                            int16_t temp2_5 = (ecx_1 - eax + 1);
                            data_43a7b8 = COMBINE(0, eax_12) / temp2_5;
                            data_43a7b6 = (ecx_1 - eax + 1);
                            data_43a7b4 =
                                COMBINE(COMBINE(0, eax_12) % temp2_5, 0) / (ecx_1 - eax + 1);
                            int32_t ebx_5;
                            ebx_5 = data_43a7b8;
                            data_4368d4;
                            /* jump -> *((ebx_5 << 2) + &data_438844) */
                        }
                        
                        if (ecx_1 - eax + 1 <= edx_4 + 1)
                        {
                            data_43a7ba = data_4368d8 / (edx_4 + 1);
                            uint16_t eax_4 = (edx_4 + 1);
                            int16_t temp2_1 = (ecx_1 - eax + 1);
                            data_43a7b8 = COMBINE(0, eax_4) / temp2_1;
                            data_43a7b6 = (ecx_1 - eax + 1);
                            data_43a7b4 =
                                COMBINE(COMBINE(0, eax_4) % temp2_1, 0) / (ecx_1 - eax + 1);
                            int32_t ebx_4;
                            ebx_4 = data_43a7b8;
                            data_4368d4;
                            /* jump -> *((ebx_4 << 2) + &data_4368dc) */
                        }
                        
                        data_43a7b6 = (edx_4 + 1);
                        ebx_3 = (edx_4 + 1);
                        data_43a7ba = data_4368d8 / (ecx_1 - eax + 1);
                        uint16_t eax_8 = (ecx_1 - eax + 1);
                        int16_t temp2_3 = ebx_3;
                        data_43a7b8 = COMBINE(0, eax_8) / temp2_3;
                        data_43a7b4 = COMBINE(COMBINE(0, eax_8) % temp2_3, 0) / ebx_3;
                        ebx_3 = data_43a7b8;
                        data_4368d4;
                        /* jump -> *((ebx_3 << 2) + &data_437250) */
                    }
                }
            }
            else
            {
                if (edx_3 <= data_41cdbc)
                    goto label_40fdeb;
                
                if (ebx_3 <= data_41cdbc)
                {
                    data_4368c0 = eax;
                    data_4368c4 = ebx_3;
                    data_4368c8 = ecx_1;
                    data_4368cc = edx_3;
                    data_4368d0 = (COMBINE(data_41cdbc - data_4368c4 + 1, 0)
                        / (data_4368cc - data_4368c4 + 1)) >> 1;
                    int32_t eax_67;
                    int32_t edx_47;
                    edx_47 = HIGHD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                    eax_67 = LOWD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                    data_4368c8 = edx_47 + data_4368c0;
                    data_4368cc = data_41cdbc;
                    int32_t eax_71;
                    int32_t edx_49;
                    edx_49 = HIGHD((data_4368d8 << 1) * data_4368d0);
                    eax_71 = LOWD((data_4368d8 << 1) * data_4368d0);
                    data_4368d8 = edx_49;
                    eax = data_4368c0;
                    ebx_3 = data_4368c4;
                    ecx_1 = data_4368c8;
                    edx_3 = data_4368cc;
                label_40fdeb:
                    
                    if (ebx_3 >= data_41cdb4)
                        goto label_40fe10;
                    
                    if (edx_3 >= data_41cdb4)
                    {
                        data_4368c0 = eax;
                        data_4368c4 = ebx_3;
                        data_4368c8 = ecx_1;
                        data_4368cc = edx_3;
                        data_4368d0 = (COMBINE(data_4368cc - data_41cdb4 + 1, 0)
                            / (data_4368cc - data_4368c4 + 1)) >> 1;
                        int32_t eax_78;
                        int32_t edx_53;
                        edx_53 = HIGHD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                        eax_78 = LOWD((data_4368c8 - data_4368c0) * 2 * data_4368d0);
                        data_4368c0 = -(edx_53) + data_4368c8;
                        data_4368c4 = data_41cdb4;
                        int32_t ebx_34 = data_43a7bc + data_4368d8;
                        int32_t eax_82;
                        int32_t edx_56;
                        edx_56 = HIGHD((data_4368d8 << 1) * data_4368d0);
                        eax_82 = LOWD((data_4368d8 << 1) * data_4368d0);
                        data_4368d8 = edx_56;
                        data_43a7bc = ebx_34 - edx_56;
                        eax = data_4368c0;
                        ebx_3 = data_4368c4;
                        ecx_1 = data_4368c8;
                        edx_3 = data_4368cc;
                        goto label_40fe10;
                    }
                }
            }
        }
    }
    
    int32_t* esi_3 = *arg2;
    data_41cda0 = esi_3;
    
    if (!esi_3)
        return 0;
    
    /* jump -> (&data_42c1c4)[*esi_3] */
}

int32_t sub_410a20(void* arg1 @ esi)
{
    int32_t* ebp = *(arg1 + 4);
    data_43a7e0 = ebp[3];
    data_43a7e4 = ebp[5];
    data_43a7e8 = ebp[2];
    data_43a7ec = ebp[4];
    data_43a7f0 = ebp[7];
    data_43a7f4 = ebp[6];
    int32_t* ecx = *(arg1 + 8);
    int32_t eax_8;
    int16_t edx;
    edx = HIGHD(-(*ebp) * *ecx);
    eax_8 = LOWD(-(*ebp) * *ecx);
    eax_8 = edx;
    data_43a7c0 = ROLD(eax_8, 0x10);
    int32_t eax_12;
    int16_t edx_1;
    edx_1 = HIGHD(-(ebp[1]) * ecx[2]);
    eax_12 = LOWD(-(ebp[1]) * ecx[2]);
    eax_12 = edx_1;
    data_43a7c0 += ROLD(eax_12, 0x10);
    int32_t eax_16;
    int16_t edx_2;
    edx_2 = HIGHD(-(*ebp) * ecx[1]);
    eax_16 = LOWD(-(*ebp) * ecx[1]);
    eax_16 = edx_2;
    data_43a7c4 = ROLD(eax_16, 0x10);
    int32_t eax_20;
    int16_t edx_3;
    edx_3 = HIGHD(-(ebp[1]) * ecx[3]);
    eax_20 = LOWD(-(ebp[1]) * ecx[3]);
    eax_20 = edx_3;
    data_43a7c4 += ROLD(eax_20, 0x10);
    int32_t ebx_5 = ebp[4] - ebp[2];
    int32_t eax_23;
    int16_t edx_4;
    edx_4 = HIGHD(ecx[1] * ebx_5);
    eax_23 = LOWD(ecx[1] * ebx_5);
    eax_23 = edx_4;
    data_43a7cc = ROLD(eax_23, 0x10);
    int32_t eax_26;
    int16_t edx_5;
    edx_5 = HIGHD(*ecx * ebx_5);
    eax_26 = LOWD(*ecx * ebx_5);
    eax_26 = edx_5;
    data_43a7c8 = ROLD(eax_26, 0x10);
    int32_t ebx_7 = ebp[5] - ebp[3];
    int32_t eax_29;
    int16_t edx_6;
    edx_6 = HIGHD(ecx[3] * ebx_7);
    eax_29 = LOWD(ecx[3] * ebx_7);
    eax_29 = edx_6;
    int32_t eax_31 = ROLD(eax_29, 0x10) + data_43a7c4;
    data_43a7d4 = eax_31;
    data_43a7dc = eax_31 + data_43a7cc;
    data_43a7cc += data_43a7c4;
    int32_t eax_36;
    int16_t edx_7;
    edx_7 = HIGHD(ecx[2] * ebx_7);
    eax_36 = LOWD(ecx[2] * ebx_7);
    eax_36 = edx_7;
    int32_t eax_38 = ROLD(eax_36, 0x10) + data_43a7c0;
    data_43a7d0 = eax_38;
    data_43a7d8 = eax_38 + data_43a7c8;
    data_43a7c8 += data_43a7c0;
    data_43a7c0 += *(arg1 + 0xc);
    data_43a7c4 += *(arg1 + 0x10);
    data_43a7c8 += *(arg1 + 0xc);
    data_43a7cc += *(arg1 + 0x10);
    data_43a7d0 += *(arg1 + 0xc);
    data_43a7d4 += *(arg1 + 0x10);
    data_43a7d8 += *(arg1 + 0xc);
    data_43a7dc += *(arg1 + 0x10);
    data_43a7fc = data_43a7c0;
    data_43a800 = data_43a7c4;
    data_43a804 = data_43a7c8;
    data_43a808 = data_43a7cc;
    data_43a80c = data_43a7d0;
    data_43a810 = data_43a7d4;
    data_43a814 = &data_43a820;
    int32_t eax_64 = data_43a7e0;
    data_43a824 = eax_64;
    data_43a82c = eax_64;
    data_43a834 = data_43a7e4;
    int32_t eax_66 = data_43a7e8;
    data_43a820 = eax_66;
    data_43a830 = eax_66;
    data_43a828 = data_43a7ec;
    data_43a818 = data_43a7f4;
    data_43a81c = data_43a7f0;
    sub_40b390(0x43a7f8);
    data_43a7fc = data_43a7c8;
    data_43a800 = data_43a7cc;
    data_43a804 = data_43a7d0;
    data_43a808 = data_43a7d4;
    data_43a80c = data_43a7d8;
    data_43a810 = data_43a7dc;
    data_43a814 = &data_43a820;
    data_43a824 = data_43a7e0;
    int32_t eax_77 = data_43a7e4;
    data_43a82c = eax_77;
    data_43a834 = eax_77;
    int32_t eax_78 = data_43a7ec;
    data_43a820 = eax_78;
    data_43a830 = eax_78;
    data_43a828 = data_43a7e8;
    data_43a818 = data_43a7f4;
    data_43a81c = data_43a7f0;
    sub_40b390(0x43a7f8);
    return 0x43a7f8;
}

uint16_t __convention("regparm") sub_410c98(int32_t arg1, int32_t arg2, int32_t arg3)
{
    data_43a910 = arg2;
    uint16_t result = -(arg1) >> 8;
    data_43a94c = arg3;
    int32_t entry_ebx;
    data_43a944 = RORD(-(entry_ebx), 0x10);
    int32_t ebx_1;
    ebx_1 = result;
    data_43a948 = ebx_1;
    return result;
}

void __convention("regparm") sub_410cbe(int32_t* arg1, int32_t arg2, void* arg3 @ esi, void* arg4 @ edi)
{
    if (arg2 - 1 < 0)
        return;
    
    data_43a94c;
    int32_t edx;
    edx = data_43a944;
    int32_t ebp_1 = data_43a948;
    int32_t temp2_1;
    
    do
    {
        *(arg3 + 4);
        int32_t ecx_2;
        ecx_2 = *(arg3 + 1);
        *ecx_2[1] = *(arg3 + 2);
        char* ebx_1;
        ebx_1 = *(arg3 + 6);
        int32_t eax = *arg1;
        int32_t esi_1 = *(arg3 + 8) - eax;
        char* edi = arg4 + eax;
        
        if (esi_1 - 1 >= 0)
        {
            data_43a910;
            *ebx_1[1] = *ecx_2[1];
            char* eax_1;
            eax_1 = edi[esi_1 - 1];
            int32_t ecx_3 = ecx_2 + ebp_1;
            bool c_1 = ecx_2 + ebp_1 < ecx_2;
            *eax_1[1] = *ebx_1;
            int32_t esi_3 = esi_1 - 2;
            
            if (esi_1 - 2 >= 0)
            {
                int32_t temp6_1;
                
                do
                {
                    ebx_1 = ebx_1 + edx;
                    *ebx_1[1] = *ecx_3[1];
                    *edx[1] = *eax_1;
                    eax_1 = edi[esi_3];
                    int32_t temp5_1 = ecx_3;
                    ecx_3 += ebp_1;
                    c_1 = temp5_1 + ebp_1 < temp5_1;
                    *eax_1[1] = *ebx_1;
                    edi[esi_3 + 1] = *edx[1];
                    temp6_1 = esi_3;
                    esi_3 -= 1;
                } while (temp6_1 - 1 >= 0);
            }
            
            *edx[1] = *eax_1;
            edi[esi_3 + 1] = *edx[1];
        }
        
        arg4 += data_41cdf0;
        arg3 += 0xc;
        arg1 = &arg1[1];
        temp2_1 = edx;
        edx -= 0x10000;
    } while (temp2_1 - 0x10000 >= 0);
}

uint16_t __convention("regparm") sub_410d3c(int32_t arg1, int32_t, int32_t arg3)
{
    uint16_t result = -(arg1) >> 8;
    data_43a94c = arg3;
    int32_t entry_ebx;
    data_43a944 = RORD(-(entry_ebx), 0x10);
    int32_t ebx_1;
    ebx_1 = result;
    data_43a948 = ebx_1;
    return result;
}

void __convention("regparm") sub_410d5c(int32_t* arg1, int32_t arg2, void* arg3 @ esi, void* arg4 @ edi)
{
    if (arg2 - 1 < 0)
        return;
    
    data_43a94c;
    int32_t edx;
    edx = data_43a944;
    int32_t ebp_1 = data_43a948;
    int32_t temp8_1;
    
    do
    {
        *(arg3 + 4);
        int32_t ecx_2;
        ecx_2 = *(arg3 + 1);
        *ecx_2[1] = *(arg3 + 2);
        char* ebx_1;
        ebx_1 = *(arg3 + 6);
        int32_t eax = *arg1;
        char* edi = arg4 + eax;
        int32_t esi_1 = *(arg3 + 8) - eax;
        *ebx_1[1] = *ecx_2[1];
        int32_t esi_2 = esi_1 - 4;
        
        if (esi_1 - 4 >= 0)
        {
            int32_t temp7_1;
            
            do
            {
                int32_t ecx_3 = ecx_2 + ebp_1;
                *eax[1] = *ebx_1;
                *ebx_1[1] = *ecx_3[1];
                ebx_1 = ebx_1 + edx;
                int32_t ecx_4 = ecx_3 + ebp_1;
                eax = *ebx_1;
                *ebx_1[1] = *ecx_4[1];
                ebx_1 = ebx_1 + edx;
                int32_t ecx_5 = ecx_4 + ebp_1;
                *eax[1] = *ebx_1;
                *ebx_1[1] = *ecx_5[1];
                ebx_1 = ebx_1 + edx;
                ecx_2 = ecx_5 + ebp_1;
                eax = *ebx_1;
                *ebx_1[1] = *ecx_2[1];
                ebx_1 = ebx_1 + edx;
                *(edi + esi_2) = eax;
                temp7_1 = esi_2;
                esi_2 -= 4;
            } while (temp7_1 - 4 >= 0);
        }
        
        int32_t esi_3 = esi_2 + 3;
        
        if (esi_2 + 3 >= 0)
        {
            int32_t ecx_6 = ecx_2 + ebp_1;
            bool c_5 = ecx_2 + ebp_1 < ecx_2;
            int32_t temp11_1;
            
            do
            {
                eax = *ebx_1;
                *ebx_1[1] = *ecx_6[1];
                ebx_1 = ebx_1 + edx;
                int32_t temp10_1 = ecx_6;
                ecx_6 += ebp_1;
                c_5 = temp10_1 + ebp_1 < temp10_1;
                edi[esi_3] = eax;
                temp11_1 = esi_3;
                esi_3 -= 1;
            } while (temp11_1 - 1 >= 0);
        }
        
        arg4 += data_41cdf0;
        arg1 = &arg1[1];
        arg3 += 0xc;
        temp8_1 = edx;
        edx -= 0x10000;
    } while (temp8_1 - 0x10000 >= 0);
}

int32_t __ftol(int16_t arg1 @ x87control, long double arg2 @ st0)
{
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int16_t eax;
    *eax[1] = *temp0[1] | 0xc;
    int16_t x87control;
    int16_t x87status_1;
    x87control = __fldcw_memmem16(eax);
    int16_t x87control_1;
    int16_t x87status_2;
    x87control_1 = __fldcw_memmem16(temp0);
    return arg2;
}

int32_t sub_410e20()
{
    sub_410e50();
    data_43aa48 = sub_411a70();
    int32_t result = sub_411a00();
    __fnclex();
    return result;
}

int32_t sub_410e40() __pure
{
    return;
}

int32_t sub_410e50()
{
    data_43aac4 = sub_411b10;
    data_43aac8 = sub_411b90;
    data_43aacc = sub_411aa0;
    data_43aad0 = sub_411b70;
    data_43aac0 = sub_411f30;
    data_43aad4 = sub_411f30;
    return sub_411f30;
}

int32_t sub_410e90()
{
    void* eax_1 = data_43aa4c;
    
    if (eax_1)
        eax_1();
    
    sub_410fb0(0x41c008, 0x41c010);
    return sub_410fb0(0x41c000, 0x41c004);
}

int32_t sub_410ec0(uint32_t arg1)
{
    return sub_410f00(arg1, 0, 0);
}

int32_t sub_410ee0(uint32_t arg1)
{
    return sub_410f00(arg1, 1, 0);
}

int32_t sub_410f00(uint32_t arg1, int32_t arg2, int32_t arg3)
{
    if (data_43aaa0 == 1)
    {
        TerminateProcess(GetCurrentProcess(), arg1);
        /* no return */
    }
    
    data_43aa9c = 1;
    data_43aa98 = arg3;
    
    if (!arg2)
    {
        if (data_45468c)
        {
            for (int32_t* i = data_454688 - 4; i >= data_45468c; i -= 4)
            {
                int32_t eax_2 = *i;
                
                if (eax_2)
                    eax_2();
            }
        }
        
        sub_410fb0(0x41c014, 0x41c01c);
    }
    
    int32_t result = sub_410fb0(0x41c020, 0x41c024);
    
    if (arg3)
        return result;
    
    data_43aaa0 = 1;
    ExitProcess(arg1);
    /* no return */
}

void sub_410fb0(int32_t* arg1, int32_t arg2)
{
    for (int32_t* i = arg1; arg2 > i; i = &i[1])
    {
        int32_t eax = *i;
        
        if (eax)
            eax();
    }
}

struct _EXCEPTION_REGISTRATION_RECORD* _start()
{
    int32_t var_8 = 0xffffffff;
    int32_t var_c = 0x41b2d0;
    int32_t var_10 = 0x412ca4;
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    fsbase->NtTib.ExceptionList = &ExceptionList;
    int32_t __saved_edi;
    int32_t* var_1c = &__saved_edi;
    data_43aa64 = GetVersion();
    int32_t eax_1;
    eax_1 = (*(data_43aa64 + 1));
    data_43aa70 = eax_1;
    char eax_2 = data_43aa64;
    data_43aa64 u>>= 0x10;
    uint32_t eax_3 = eax_2;
    data_43aa6c = eax_3;
    data_43aa68 = (eax_3 << 8) + data_43aa70;
    
    if (!sub_412c60())
        sub_411170(0x1c);
    
    int32_t var_8_1 = 0;
    sub_412a80();
    sub_412a70();
    data_454684 = GetCommandLineA();
    void* eax_8 = sub_412620();
    data_43aaa4 = eax_8;
    
    if (!eax_8 || !data_454684)
        sub_410ec0(0xffffffff);
    
    sub_4123a0();
    sub_4122b0();
    sub_410e90();
    char* esi = data_454684;
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
                
                if (sub_412250(ebx))
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
    sub_410ec0(sub_403140(GetModuleHandleA(nullptr)));
    int32_t var_8_2 = 0xffffffff;
    struct _EXCEPTION_REGISTRATION_RECORD* result = ExceptionList;
    fsbase->NtTib.ExceptionList = result;
    return result;
}

int32_t sub_411123(void* arg1 @ ebp)
{
    *(arg1 - 0x20) = ***(arg1 - 0x14);
    return sub_4120c0(*(arg1 - 0x20), *(arg1 - 0x14));
}

int32_t sub_411170(int32_t arg1)
{
    if (data_43aab0 == 1)
        sub_412d80();
    
    sub_412dc0(arg1);
    return data_43aaac(0xff);
}

void* sub_4111a0(int32_t arg1)
{
    return sub_4111c0(arg1, data_43ae4c);
}

void* sub_4111c0(int32_t arg1, int32_t arg2)
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
            result = sub_411210(esi);
        
        if (result || !arg2)
            return result;
        
        i = sub_412fc0(esi);
    } while (i);
    return nullptr;
}

void* sub_411210(int32_t arg1)
{
    uint32_t dwBytes = (arg1 + 0xf) & 0xfffffff0;
    
    if (dwBytes <= data_43b66c)
    {
        void* result = sub_413350(dwBytes >> 4);
        
        if (result)
            return result;
    }
    
    return HeapAlloc(data_454574, HEAP_NONE, dwBytes);
}

void sub_411250(int32_t arg1)
{
    if (!arg1)
        return;
    
    int32_t var_8;
    void** var_4;
    char* eax_1 = sub_4132a0(arg1, &var_4, &var_8);
    
    if (eax_1)
    {
        sub_413300(var_4, var_8, eax_1);
        return;
    }
    
    HeapFree(data_454574, HEAP_NONE, arg1);
}

int32_t sub_4112a0(int32_t* arg1)
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
        result = sub_4138b0(arg1);
        sub_413820(arg1);
        
        if (sub_413750(arg1[4]) >= 0)
        {
            int32_t eax_5 = arg1[7];
            
            if (eax_5)
            {
                sub_411250(eax_5);
                arg1[7] = 0;
            }
        }
        else
            result = 0xffffffff;
    }
    
    arg1[3] = 0;
    return result;
}

uint32_t sub_411310(void* arg1, int32_t arg2, int32_t arg3, int32_t* arg4)
{
    void* var_c = arg1;
    void* i_1 = arg3 * arg2;
    void* i = i_1;
    
    if (!i_1)
        return 0;
    
    int32_t var_4;
    
    if (!(arg4[3] & 0x10c))
        var_4 = 0x1000;
    else
        var_4 = arg4[6];
    
    if (i_1)
    {
        do
        {
            void* i_4;
            
            if (arg4[3] & 0x10c)
                i_4 = arg4[1];
            
            if (arg4[3] & 0x10c && i_4)
            {
                void* i_3 = i;
                
                if (i >= i_4)
                    i_3 = i_4;
                
                i -= i_3;
                int32_t esi_2;
                int32_t edi_2;
                edi_2 = __builtin_memcpy(var_c, *arg4, i_3 >> 2 << 2);
                __builtin_memcpy(edi_2, esi_2, i_3 & 3);
                arg4[1] -= i_3;
                *arg4 += i_3;
                var_c += i_3;
            }
            else if (i < var_4)
            {
                int32_t eax_8 = sub_4139c0(arg4);
                
                if (eax_8 == 0xffffffff)
                    return COMBINE(0, i_1 - i) / arg2;
                
                void* ecx_6 = var_c;
                i -= 1;
                var_c += 1;
                *ecx_6 = eax_8;
                var_4 = arg4[6];
            }
            else
            {
                void* i_2 = i;
                
                if (var_4)
                    i_2 = i - COMBINE(0, i) % var_4;
                
                void* eax_7 = sub_413ab0(arg4[4], var_c, i_2);
                
                if (!eax_7)
                {
                    arg4[3] |= 0x10;
                    return COMBINE(0, i_1 - i) / arg2;
                }
                
                if (eax_7 == 0xffffffff)
                {
                    arg4[3] |= 0x20;
                    return COMBINE(0, i_1 - i) / arg2;
                }
                
                i -= eax_7;
                var_c += eax_7;
            }
        } while (i);
    }
    
    return arg3;
}

int32_t sub_411460(int32_t* arg1, int32_t* arg2)
{
    return sub_413d20(arg1, *arg2, arg2[1], FILE_BEGIN);
}

int32_t* sub_411480(PSTR arg1, char* arg2, int32_t arg3)
{
    int32_t* eax_3 = sub_413fe0();
    
    if (eax_3)
        return sub_413dd0(arg1, arg2, arg3, eax_3);
    
    return 0;
}

int32_t* sub_4114b0(PSTR arg1, char* arg2)
{
    return sub_411480(arg1, arg2, 0x40);
}

int32_t sub_4114d0(int32_t* arg1, void* arg2, enum SET_FILE_POINTER_MOVE_METHOD arg3)
{
    int32_t eax = arg1[3];
    
    if (eax & 0x83)
    {
        enum SET_FILE_POINTER_MOVE_METHOD edi_1 = arg3;
        
        if (!edi_1 || edi_1 == FILE_CURRENT || edi_1 == FILE_END)
        {
            arg1[3] = eax & 0xffffffef;
            void* ebx_1;
            
            if (edi_1 != FILE_CURRENT)
                ebx_1 = arg2;
            else
            {
                edi_1 = FILE_BEGIN;
                ebx_1 = arg2 + sub_411570(arg1);
            }
            
            sub_4138b0(arg1);
            int32_t eax_3 = arg1[3];
            
            if (eax_3 & 0x80)
                arg1[3] = eax_3 & 0xfffffffc;
            else if (eax_3 & 1 && eax_3 & 8 && !(*eax_3[1] & 4))
                arg1[6] = 0x200;
            
            int32_t eax_7 = sub_4143e0(arg1[4], ebx_1, edi_1) + 1;
            return eax_7 - eax_7;
        }
    }
    
    data_43aa58 = 0x16;
    return 0xffffffff;
}

void* sub_411570(int32_t* arg1)
{
    int32_t eax = arg1[4];
    
    if (arg1[1] < 0)
        arg1[1] = 0;
    
    uint32_t edi = sub_4143e0(eax, 0, FILE_CURRENT);
    
    if (edi < 0)
        return 0xffffffff;
    
    int32_t eax_4 = arg1[3];
    char var_8 = eax_4;
    
    if (!(eax_4 & 0x108))
        return edi - arg1[1];
    
    int32_t edx = *arg1;
    char* ecx = arg1[2];
    void* eax_7 = edx - ecx;
    void* ebx = eax_7;
    
    if (!(var_8 & 3))
    {
        if (!(var_8 & 0x80))
        {
            data_43aa58 = 0x16;
            return 0xffffffff;
        }
    }
    else if (*(*(((eax & 0xffffffe7) >> 3) + &data_454580) + ((eax & 0x1f) << 3) + 4) & 0x80
        && ecx < edx)
    {
        do
        {
            if (*ecx == 0xa)
                ebx += 1;
            
            ecx = &ecx[1];
        } while (ecx < edx);
    }
    
    if (!edi)
        return ebx;
    
    if (var_8 & 1)
    {
        int32_t ecx_1 = arg1[1];
        
        if (!ecx_1)
            return edi;
        
        void* ebp_3 = eax_7 + ecx_1;
        int32_t eax_20 = (eax & 0x1f) << 3;
        
        if (*(*(((eax & 0xffffffe7) >> 3) + &data_454580) + eax_20 + 4) & 0x80)
        {
            if (sub_4143e0(eax, 0, FILE_END) != edi)
            {
                sub_4143e0(eax, edi, FILE_BEGIN);
                
                if (ebp_3 > 0x200)
                    ebp_3 = arg1[6];
                else
                {
                    int16_t eax_24 = arg1[3];
                    
                    if (!(eax_24 & 8))
                        ebp_3 = arg1[6];
                    else
                    {
                        ebp_3 = 0x200;
                        
                        if (*eax_24[1] & 4)
                            ebp_3 = arg1[6];
                    }
                }
                
                if (*(*(((eax & 0xffffffe7) >> 3) + &data_454580) + eax_20 + 4) & 4)
                    ebp_3 += 1;
            }
            else
            {
                void* i = arg1[2];
                
                for (void* ecx_3 = i + ebp_3; ecx_3 > i; i += 1)
                {
                    if (*i == 0xa)
                        ebp_3 += 1;
                }
                
                if (*(arg1 + 0xd) & 0x20)
                    ebp_3 += 1;
            }
        }
        
        edi -= ebp_3;
    }
    
    return ebx + edi;
}

char* sub_411720(char* arg1, void* arg2)
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
                label_41174c:
                    
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
        
        goto label_41174c;
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
            return sub_414510(ecx_1, edx_1);
        
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

int32_t sub_4117a0(char* arg1)
{
    int32_t eax = sub_4145e0(0x43b808);
    void arg_8;
    void* var_c = &arg_8;
    int32_t result = sub_4147c0(0x43b808, arg1);
    sub_414680(eax, 0x43b808);
    return result;
}

void* sub_4117e0(int32_t arg1, int32_t arg2)
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
            if (data_43b66c < dwBytes)
            {
            label_41183d:
                
                if (result)
                    return result;
            }
            else
            {
                result = sub_413350(dwBytes >> 4);
                
                if (result)
                {
                    __builtin_memset(__builtin_memset(result, 0, dwBytes >> 2 << 2), 0, 
                        dwBytes & 3);
                    goto label_41183d;
                }
            }
            
            result = HeapAlloc(data_454574, HEAP_ZERO_MEMORY, dwBytes);
        }
        
        if (result || !data_43ae4c)
            return result;
        
        i = sub_412fc0(dwBytes);
    } while (i);
    return 0;
}

char* sub_411880(char* arg1, char* arg2, int32_t arg3)
{
    int32_t i_3 = arg3;
    
    if (i_3)
    {
        char* edi_1 = arg1;
        void* edi_2;
        
        while (edi_1 & 3)
        {
            char eax = *edi_1;
            edi_1 = &edi_1[1];
            
            if (!eax)
            {
            label_4118db:
                edi_2 = edi_1 - 1;
                goto label_4118eb;
            }
        }
        
        while (true)
        {
            int32_t eax_1 = *edi_1;
            edi_1 = &edi_1[4];
            
            if ((eax_1 ^ 0xffffffff ^ (0x7efefeff + eax_1)) & 0x81010100)
            {
                int32_t eax_4 = *(edi_1 - 4);
                
                if (!eax_4)
                {
                    edi_2 = edi_1 - 4;
                    break;
                }
                
                if (!*eax_4[1])
                {
                    edi_2 = edi_1 - 3;
                    break;
                }
                
                if (!(eax_4 & 0xff0000))
                {
                    edi_2 = edi_1 - 2;
                    break;
                }
                
                if (!(eax_4 & 0xff000000))
                    goto label_4118db;
            }
        }
        
    label_4118eb:
        char* esi_1 = arg2;
        int32_t edx;
        int32_t i_4;
        uint32_t i_2;
        
        if (esi_1 & 3)
        {
            do
            {
                edx = *esi_1;
                esi_1 = &esi_1[1];
                
                if (!edx)
                {
                label_41193a:
                    *edi_2 = edx;
                    return arg1;
                }
                
                *edi_2 = edx;
                edi_2 += 1;
                int32_t i_5 = i_3;
                i_3 -= 1;
                
                if (i_5 == 1)
                    goto label_411930;
            } while (esi_1 & 3);
            
            i_4 = i_3;
            i_2 = i_3 >> 2;
            
            if (i_2)
                goto label_41194a;
        }
        else
        {
            i_4 = i_3;
            i_2 = i_3 >> 2;
            
            if (i_2)
            {
            label_41194a:
                uint32_t i;
                
                do
                {
                    int32_t eax_7 = *esi_1;
                    edx = *esi_1;
                    esi_1 = &esi_1[4];
                    
                    if ((eax_7 ^ 0xffffffff ^ (0x7efefeff + eax_7)) & 0x81010100)
                    {
                        if (!edx)
                            goto label_41193a;
                        
                        if (!*edx[1])
                        {
                            *edi_2 = edx;
                            return arg1;
                        }
                        
                        if (!(edx & 0xff0000))
                        {
                            *edi_2 = edx;
                            *(edi_2 + 2) = 0;
                            return arg1;
                        }
                        
                        if (!(edx & 0xff000000))
                        {
                            *edi_2 = edx;
                            return arg1;
                        }
                    }
                    
                    *edi_2 = edx;
                    edi_2 += 4;
                    i = i_2;
                    i_2 -= 1;
                } while (i != 1);
            }
        }
        i_3 = i_4 & 3;
        
        if (i_3)
        {
            int32_t i_1;
            
            do
            {
                edx = *esi_1;
                esi_1 = &esi_1[1];
                *edi_2 = edx;
                edi_2 += 1;
                
                if (!edx)
                    return arg1;
                
                i_1 = i_3;
                i_3 -= 1;
            } while (i_1 != 1);
        }
        
    label_411930:
        *edi_2 = i_3;
    }
    
    return arg1;
}

int32_t sub_4119b0()
{
    int16_t x87control;
    /* tailcall */
    return sub_415504(x87control);
}

int32_t sub_4119ba()
{
    int16_t x87control;
    /* tailcall */
    return sub_415675(x87control);
}

int32_t sub_4119d2()
{
    int16_t x87control;
    long double x87_r0;
    long double x87_r1;
    /* tailcall */
    return sub_415490(x87control, x87_r0, x87_r1);
}

int32_t sub_4119dc()
{
    int16_t x87control;
    long double x87_r0;
    /* tailcall */
    return sub_4154ce(x87control, x87_r0);
}

int32_t sub_411a00()
{
    return sub_415730(0x10000, 0x30000);
}

int32_t sub_411a20() __pure
{
    int32_t var_10 = 0x80000000;
    int32_t var_c = 0x4147ffff;
    int32_t var_18 = 0xc0000000;
    int32_t var_14 = 0x4150017e;
    long double x87_r7_4 = 1;
    long double temp0 = var_18 - var_18 / var_10 * var_10;
    x87_r7_4 - temp0;
    
    if (*((x87_r7_4 < temp0 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_4, temp0) ? 1 : 0) << 0xa
            | (x87_r7_4 == temp0 ? 1 : 0) << 0xe)[1] & 1)
        return 1;
    
    return 0;
}

int32_t sub_411a70()
{
    HMODULE hModule = GetModuleHandleA("KERNEL32");
    
    if (hModule)
    {
        int32_t eax = GetProcAddress(hModule, "IsProcessorFeaturePresent");
        
        if (eax)
            return eax(0);
    }
    
    /* tailcall */
    return sub_411a20();
}

int32_t sub_411aa0(char* arg1)
{
    char* esi = arg1;
    int32_t i;
    int32_t ecx;
    wchar16 (* edx)[0x21];
    i = sub_415940(*esi);
    
    if (i != 0x65)
    {
        do
        {
            esi = &esi[1];
            
            if (data_43bb80 <= 1)
            {
                int32_t eax_2;
                eax_2 = (**&data_43bb90)[*esi];
                i = eax_2 & 4;
            }
            else
                i = sub_4158a0(*esi, 4);
        } while (i);
    }
    
    ecx = *esi;
    i = data_43bb84;
    *esi = i;
    void* esi_1 = &esi[1];
    
    do
    {
        edx = *esi_1;
        i = ecx;
        *esi_1 = i;
        ecx = edx;
        esi_1 += 1;
    } while (i);
    
    return i;
}

char* sub_411b10(char* arg1)
{
    char* result_1 = arg1;
    
    if (*result_1)
    {
        while (*result_1 != data_43bb84)
        {
            result_1 = &result_1[1];
            
            if (!*result_1)
                break;
        }
    }
    
    char* result = result_1;
    void* result_2 = &result_1[1];
    
    if (*result)
    {
        while (*result_2)
        {
            result = *result_2;
            
            if (result == 0x65)
                break;
            
            if (result == 0x45)
                break;
            
            result_2 += 1;
        }
        
        result = result_2;
        char* edx = result_2 - 1;
        
        while (*edx == 0x30)
            edx -= 1;
        
        if (*edx == data_43bb84)
            edx -= 1;
        
        char i;
        
        do
        {
            i = *result;
            edx = &edx[1];
            result = &result[1];
            *edx = i;
        } while (i);
    }
    
    return result;
}

int32_t sub_411b70(double* arg1)
{
    long double x87_r7 = 0;
    long double temp0 = *arg1;
    x87_r7 - temp0;
    double* eax;
    eax = (x87_r7 < temp0 ? 1 : 0) << 8 | (FCMP_UO(x87_r7, temp0) ? 1 : 0) << 0xa
        | (x87_r7 == temp0 ? 1 : 0) << 0xe;
    
    if (*eax[1] & 0x41)
        return 1;
    
    return 0;
}

int32_t sub_411b90(int32_t arg1, int32_t* arg2, char* arg3)
{
    if (!arg1)
    {
        int32_t var_c;
        int32_t eax_1 = sub_415f10(&var_c, arg3);
        *arg2 = var_c;
        return eax_1;
    }
    
    int32_t var_8;
    sub_415ed0(&var_8, arg3);
    int32_t eax = var_8;
    *arg2 = eax;
    int32_t var_4;
    arg2[1] = var_4;
    return eax;
}

char* sub_411be0(int32_t* arg1, char* arg2, int32_t arg3, int32_t arg4)
{
    void* result_2;
    char* result;
    int32_t esi;
    int32_t* edi;
    
    if (!data_43aad8)
    {
        int32_t var_14_1 = arg1[1];
        int32_t var_18_1 = *arg1;
        sub_415fe0();
        esi = arg3;
        edi = &data_4528a0;
        int32_t ecx_2 = 1;
        int32_t eax_9 = data_4528a0 - 0x2d;
        result = arg2;
        
        if (esi <= 0)
            ecx_2 = 0;
        
        result_2 = sub_415f50(-((eax_9 - eax_9)) + ecx_2 + result, esi + 1, &data_4528a0);
    }
    else
    {
        edi = data_45275c;
        result = arg2;
        esi = arg3;
        int32_t eax_1 = *edi - 0x2d;
        int32_t ecx = 1;
        
        if (esi <= 0)
            ecx = 0;
        
        result_2 = sub_411fa0(&result[-((eax_1 - eax_1))], ecx);
    }
    
    void* result_1 = result;
    
    if (*edi == 0x2d)
    {
        result_1 = &result[1];
        *result = 0x2d;
    }
    
    if (esi > 0)
    {
        result_2 = result_1 + 1;
        *result_1 = *result_2;
        result_1 = result_2;
        int32_t ebx;
        ebx = data_43bb84;
        *result_2 = ebx;
    }
    
    void* edx_4 = -((result_2 - result_2)) + result_1 + esi;
    __builtin_strncpy(edx_4, "e+00", 4);
    *(edx_4 + 4) = 0x30;
    void* ecx_3 = result_1 + -((&data_410030 - &data_410030)) + esi;
    
    if (arg4)
        *ecx_3 = 0x45;
    
    if (*edi[3] != 0x30)
    {
        int32_t ebx_1 = edi[1];
        int32_t ebx_2 = ebx_1 - 1;
        
        if (ebx_1 - 1 < 0)
        {
            ebx_2 = -(ebx_2);
            *(ecx_3 + 1) = 0x2d;
        }
        
        if (ebx_2 >= 0x64)
        {
            *(ecx_3 + 2) += ebx_2 / 0x64;
            ebx_2 = ebx_2 % 0x64;
        }
        
        if (ebx_2 >= 0xa)
        {
            *(ecx_3 + 3) += ebx_2 / 0xa;
            ebx_2 = ebx_2 % 0xa;
        }
        
        *(ecx_3 + 4) += ebx_2;
    }
    
    return result;
}

char* sub_411d20(int32_t* arg1, char* arg2, int32_t arg3)
{
    char* result;
    int32_t ebp;
    int32_t* esi;
    
    if (!data_43aad8)
    {
        int32_t var_14_1 = arg1[1];
        int32_t var_18_1 = *arg1;
        sub_415fe0();
        ebp = arg3;
        esi = &data_4528a0;
        result = arg2;
        int32_t eax_10 = data_4528a0 - 0x2d;
        sub_415f50(&result[-((eax_10 - eax_10))], data_4528a4 + ebp, &data_4528a0);
    }
    else
    {
        esi = data_45275c;
        result = arg2;
        ebp = arg3;
        int32_t eax_1 = *esi - 0x2d;
        
        if (data_43aadc == ebp)
        {
            char* eax_5 = &result[-((eax_1 - eax_1)) + data_43aadc];
            *eax_5 = 0x30;
            eax_5[1] = 0;
        }
    }
    
    void* result_1 = result;
    
    if (*esi == 0x2d)
    {
        result_1 = &result[1];
        *result = 0x2d;
    }
    
    int32_t eax_14 = esi[1];
    void* edi;
    
    if (eax_14 > 0)
        edi = result_1 + eax_14;
    else
    {
        edi = result_1 + 1;
        sub_411fa0(result_1, 1);
        *(edi - 1) = 0x30;
    }
    
    if (ebp > 0)
    {
        sub_411fa0(edi, 1);
        *edi = data_43bb84;
        int32_t esi_1 = esi[1];
        
        if (esi_1 < 0)
        {
            int32_t esi_2;
            
            if (!data_43aad8)
            {
                esi_2 = -(esi_1);
                
                if (esi_2 >= ebp)
                    esi_2 = ebp;
            }
            else
                esi_2 = -(esi_1);
            
            sub_411fa0(edi + 1, esi_2);
            __builtin_memset(__builtin_memset(edi + 1, 0x30303030, esi_2 >> 2 << 2), 0x30, 
                esi_2 & 3);
        }
    }
    
    return result;
}

char* sub_411e20(int32_t* arg1, char* arg2, int32_t arg3, int32_t arg4)
{
    int32_t var_14 = arg1[1];
    int32_t var_18 = *arg1;
    sub_415fe0();
    data_45275c = 0x4528a0;
    int32_t* ecx_1 = data_45275c;
    data_43aadc = data_4528a4 - 1;
    int32_t eax_4 = *ecx_1 - 0x2d;
    char* ebx = &arg2[-((eax_4 - eax_4))];
    sub_415f50(ebx, arg3, ecx_1);
    void* ecx_2 = data_45275c;
    bool edx = *(ecx_2 + 4) - 1 > data_43aadc;
    data_43aae0 = edx;
    int32_t eax_10 = *(ecx_2 + 4) - 1;
    data_43aadc = eax_10;
    
    if (eax_10 < 0xfffffffc || arg3 <= eax_10)
        return sub_411ed0(arg1, arg2, arg3, arg4);
    
    if (edx)
    {
        char* eax_11;
        
        do
        {
            eax_11 = ebx;
            ebx = &ebx[1];
        } while (*eax_11);
        ebx[0xfffffffe] = 0;
    }
    
    return sub_411f00(arg1, arg2, arg3);
}

char* sub_411ed0(int32_t* arg1, char* arg2, int32_t arg3, int32_t arg4)
{
    data_43aad8 = 1;
    char* result = sub_411be0(arg1, arg2, arg3, arg4);
    data_43aad8 = 0;
    return result;
}

char* sub_411f00(int32_t* arg1, char* arg2, int32_t arg3)
{
    data_43aad8 = 1;
    char* result = sub_411d20(arg1, arg2, arg3);
    data_43aad8 = 0;
    return result;
}

char* sub_411f30(int32_t* arg1, char* arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{
    if (arg3 == 0x65 || arg3 == 0x45)
        return sub_411be0(arg1, arg2, arg4, arg5);
    
    if (arg3 != 0x66)
        return sub_411e20(arg1, arg2, arg4, arg5);
    
    return sub_411d20(arg1, arg2, arg4);
}

void sub_411fa0(int32_t arg1, int32_t arg2)
{
    if (!arg2)
        return;
    
    int32_t i = 0xffffffff;
    int32_t edi_1 = arg1;
    
    while (i)
    {
        bool cond:0_1 = 0 != *edi_1;
        edi_1 += 1;
        i -= 1;
        
        if (!cond:0_1)
            break;
    }
    
    int32_t ecx_1 = ~i;
    int32_t var_c_1 = ecx_1;
    int32_t edx_2 = arg1 + arg2;
    sub_416110(0, edx_2, ecx_1, edx_2, arg1);
}

int32_t sub_411fcc(int32_t arg1)
{
    int32_t ebp;
    int32_t var_4 = ebp;
    int32_t result = RtlUnwind(arg1, 0x411fe4, nullptr, nullptr);
    var_4;
    return result;
}

int32_t sub_411fec(int32_t arg1, int32_t arg2, int32_t* arg3)
{
    if (!(*(arg1 + 4) & 6))
        return 1;
    
    *arg3 = arg2;
    return 3;
}

void* sub_41200e(void* arg1, int32_t arg2)
{
    void* var_10 = arg1;
    int32_t var_14 = 0xfffffffe;
    int32_t (* var_18)(int32_t arg1, int32_t arg2, int32_t* arg3) = sub_411fec;
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
            sub_4120a2(*(ebx_1 + (esi_2 << 2) + 8), ebp);
            (*(ebx_1 + (esi_2 << 2) + 8))();
        }
    }
}

int32_t __abnormal_termination()
{
    TEB* fsbase;
    struct _EXCEPTION_REGISTRATION_RECORD* ExceptionList = fsbase->NtTib.ExceptionList;
    
    if (ExceptionList->Handler == sub_411fec
            && *(ExceptionList + 8) == *(*(ExceptionList + 0xc) + 0xc))
        return 1;
    
    return 0;
}

void __convention("regparm") sub_4120a2(int32_t arg1, void* arg2 @ ebp)
{
    data_43aaec = *(arg2 + 8);
    data_43aae8 = arg1;
    data_43aaf0 = arg2;
}

int32_t sub_4120c0(int32_t arg1, EXCEPTION_POINTERS* arg2)
{
    void* eax = sub_412220(arg1);
    
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
            
            int32_t esi = data_43ab80;
            data_43ab80 = arg2;
            
            if (*(eax + 4) != 8)
            {
                *(eax + 8) = 0;
                edx_1(*(eax + 4));
            }
            else
            {
                if (data_43ab74 + data_43ab70 > data_43ab70)
                {
                    void* edi_1 = data_43ab70 * 0xc + &data_43ab00;
                    int32_t i_1 = data_43ab74;
                    int32_t i;
                    
                    do
                    {
                        *edi_1 = 0;
                        edi_1 += 0xc;
                        i = i_1;
                        i_1 -= 1;
                    } while (i != 1);
                }
                
                int32_t edi_2 = data_43ab7c;
                
                switch (*eax)
                {
                    case 0xc000008d:
                    {
                        data_43ab7c = 0x82;
                        break;
                    }
                    case 0xc000008e:
                    {
                        data_43ab7c = 0x83;
                        break;
                    }
                    case 0xc000008f:
                    {
                        data_43ab7c = 0x86;
                        break;
                    }
                    case 0xc0000090:
                    {
                        data_43ab7c = 0x81;
                        break;
                    }
                    case 0xc0000091:
                    {
                        data_43ab7c = 0x84;
                        break;
                    }
                    case 0xc0000092:
                    {
                        data_43ab7c = 0x8a;
                        break;
                    }
                    case 0xc0000093:
                    {
                        data_43ab7c = 0x85;
                        break;
                    }
                }
                
                edx_1(8, data_43ab7c);
                data_43ab7c = edi_2;
            }
            
            data_43ab80 = esi;
            return 0xffffffff;
        }
    }
    
    return UnhandledExceptionFilter(arg2);
}

void* sub_412220(int32_t arg1)
{
    void* edx = &data_43aaf8;
    
    while (*edx != arg1)
    {
        edx += 0xc;
        
        if (data_43ab78 * 0xc + &data_43aaf8 <= edx)
            break;
    }
    
    int32_t eax_5 = *edx - arg1;
    return (eax_5 - eax_5) & edx;
}

int32_t sub_412250(char arg1)
{
    return sub_412270(arg1, 0, 4);
}

int32_t sub_412270(char arg1, int32_t arg2, int32_t arg3)
{
    void* edx;
    edx = arg1;
    int32_t ecx;
    ecx = *(edx + 0x43ab91);
    
    if (!(arg3 & ecx))
    {
        int32_t ecx_1 = 0;
        
        if (arg2)
        {
            int32_t ecx_2;
            ecx_2 = data_43bb9a[edx];
            ecx_1 = ecx_2 & arg2;
        }
        
        if (!ecx_1)
            return 0;
    }
    
    return 1;
}

int32_t sub_4122b0()
{
    char* edx = data_43aaa4;
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
    
    void* eax_1 = sub_4111a0((esi << 2) + 4);
    data_43aa80 = eax_1;
    void* ebx = eax_1;
    
    if (!ebx)
        sub_411170(9);
    
    char* ebp = data_43aaa4;
    
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
            void* eax_3 = sub_4111a0(ecx_2);
            *ebx = eax_3;
            
            if (!eax_3)
                sub_411170(9);
            
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
    
    int32_t result = sub_411250(data_43aaa4);
    data_43aaa4 = 0;
    *ebx = 0;
    return result;
}

int32_t sub_4123a0()
{
    void* esi = &data_452760;
    GetModuleFileNameA(nullptr, &data_452760, 0x104);
    char* eax = data_454684;
    data_43aa90 = &data_452760;
    
    if (*eax)
        esi = data_454684;
    
    int32_t var_8;
    int32_t var_4;
    sub_412440(esi, nullptr, nullptr, &var_8, &var_4);
    void* eax_4 = sub_4111a0((var_8 << 2) + var_4);
    
    if (!eax_4)
        sub_411170(8);
    
    sub_412440(esi, eax_4, eax_4 + (var_8 << 2), &var_8, &var_4);
    int32_t result = var_8 - 1;
    data_43aa78 = eax_4;
    data_43aa74 = result;
    return result;
}

char* sub_412440(char* arg1, int32_t* arg2, char* arg3, int32_t* arg4, int32_t* arg5)
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
            
            if (*(i + 0x43ab91) & 4)
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
            
            if (*(ebx_1 + 0x43ab91) & 4)
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
            label_4124ac:
                
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
                    goto label_4124ac;
                
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
                    
                    if (*(ebx_4 + 0x43ab91) & 4)
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
                    
                    if (*(ebx_3 + 0x43ab91) & 4)
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

void* sub_412620()
{
    PSTR penv = nullptr;
    PWSTR edi = nullptr;
    
    if (!data_43ab88)
    {
        PWSTR eax_1 = GetEnvironmentStringsW();
        edi = eax_1;
        
        if (!eax_1)
        {
            penv = GetEnvironmentStrings();
            
            if (!penv)
                return 0;
            
            data_43ab88 = 2;
        }
        else
            data_43ab88 = 1;
    }
    
    if (data_43ab88 != 1)
    {
        if (data_43ab88 != 2)
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
        
        void* eax_12 = sub_4111a0(penv_1 - penv + 1);
        
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
        void* lpMultiByteStr = sub_4111a0(cbMultiByte);
        
        if (lpMultiByteStr)
        {
            if (!WideCharToMultiByte(0, 0, edi, ((esi - edi) >> 1) + 1, lpMultiByteStr, 
                cbMultiByte, nullptr, nullptr))
            {
                sub_411250(lpMultiByteStr);
                lpMultiByteStr = nullptr;
            }
            
            FreeEnvironmentStringsW(edi);
            return lpMultiByteStr;
        }
    }
    
    FreeEnvironmentStringsW(edi);
    return 0;
}

int32_t sub_4127b0(int32_t arg1)
{
    uint32_t CodePage = sub_412990(arg1);
    
    if (CodePage == data_43ac94)
        return 0;
    
    if (!CodePage)
    {
        sub_412a40();
        return 0;
    }
    
    int32_t var_18 = 0;
    
    for (void* i = &data_43acb8; i < &data_43ada8; )
    {
        if (*i == CodePage)
        {
            void* j = nullptr;
            *__builtin_memset(0x43ab90, 0, 0x100) = 0;
            
            do
            {
                void* esi_2 = ((j + var_18 * 6) << 3) + &data_43acc8;
                
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
                        ecx_3 = *(j + 0x43acb0);
                        int32_t ebx_2;
                        
                        do
                        {
                            *(edx_3 + 0x43ab91) |= ecx_3;
                            edx_3 += 1;
                            ebx_2 = *(esi_2 + 1);
                        } while (ebx_2 >= edx_3);
                    }
                    
                    esi_2 += 2;
                }
                
                j += 1;
            } while (j < 4);
            
            data_43ac94 = CodePage;
            data_43ac98 = sub_4129e0(CodePage);
            int32_t eax_7 = var_18 << 4;
            int32_t ebx_3 = *(eax_7 * 3 + &data_43acc0);
            int32_t ecx_5 = *(eax_7 * 3 + 0x43acc4);
            data_43aca0 = *(eax_7 * 3 + 0x43acbc);
            data_43aca4 = ebx_3;
            data_43aca8 = ecx_5;
            return 0;
        }
        
        i += 0x30;
        var_18 += 1;
    }
    
    CPINFO cPInfo;
    
    if (GetCPInfo(CodePage, &cPInfo) != 1)
    {
        if (!data_43acac)
            return 0xffffffff;
        
        sub_412a40();
        return 0;
    }
    
    *__builtin_memset(0x43ab90, 0, 0x100) = 0;
    int32_t eax_4;
    
    if (cPInfo.MaxCharSize <= 1)
    {
        eax_4 = 0;
        data_43ac94 = 0;
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
                        *(ecx_1 + 0x43ab91) |= 4;
                        ecx_1 += 1;
                        eax_3 = *(esi_1 + 1);
                    } while (eax_3 >= ecx_1);
                }
                
                esi_1 += 2;
            } while (*esi_1);
        }
        
        for (void* i_1 = 1; i_1 < 0xff; i_1 += 1)
            *(i_1 + 0x43ab91) |= 8;
        
        data_43ac94 = CodePage;
        eax_4 = sub_4129e0(CodePage);
    }
    
    data_43ac98 = eax_4;
    data_43aca0 = 0;
    data_43aca4 = 0;
    data_43aca8 = 0;
    return 0;
}

int32_t sub_412990(int32_t arg1)
{
    data_43acac = 0;
    
    if (arg1 == 0xfffffffe)
    {
        data_43acac = 1;
        /* tailcall */
        return GetOEMCP();
    }
    
    if (arg1 == 0xfffffffd)
    {
        data_43acac = 1;
        /* tailcall */
        return GetACP();
    }
    
    if (arg1 != 0xfffffffc)
        return arg1;
    
    data_43acac = 1;
    return data_43bde8;
}

int32_t sub_4129e0(int32_t arg1)
{
    if (arg1 - 0x3a4 <= 0x12)
    {
        int32_t ecx_1;
        ecx_1 = *(arg1 + sub_412620+0x68);
        
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

int32_t sub_412a40()
{
    *__builtin_memset(0x43ab90, 0, 0x100) = 0;
    data_43aca0 = 0;
    data_43ac94 = 0;
    data_43ac98 = 0;
    data_43aca4 = 0;
    data_43aca8 = 0;
    return 0;
}

int32_t sub_412a70()
{
    return sub_4127b0(0xfffffffd);
}

uint32_t sub_412a80()
{
    void* i = sub_4111a0(0x100);
    
    if (!i)
        sub_411170(0x1b);
    
    data_454580 = i;
    data_454680 = 0x20;
    
    if (i + 0x100 > i)
    {
        do
        {
            *(i + 4) = 0;
            i += 8;
            *(i - 8) = 0xffffffff;
            *(i - 3) = 0xa;
        } while (data_454580 + 0x100 > i);
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
        
        if (data_454680 < i_1)
        {
            void* ebp_1 = &data_454584;
            
            do
            {
                lpReserved2 = sub_4111a0(0x100);
                
                if (!lpReserved2)
                {
                    i_1 = data_454680;
                    break;
                }
                
                *ebp_1 = lpReserved2;
                data_454680 += 0x20;
                
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
            } while (data_454680 < i_1);
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
                        label_412bba:
                            BYTE** ecx_4 = *(((ebp_2 & 0xffffffe7) >> 3) + &data_454580)
                                + ((ebp_2 & 0x1f) << 3);
                            *ecx_4 = *ebx_1;
                            int32_t edx_3;
                            edx_3 = *edi_1;
                            ecx_4[1] = edx_3;
                        }
                        else if (GetFileType(hFile_1))
                            goto label_412bba;
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
        int32_t* edi_3 = (i_2 << 3) + data_454580;
        
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
    
    return SetHandleCount(data_454680);
}

int32_t sub_412c60()
{
    HANDLE eax = HeapCreate(HEAP_NO_SERIALIZE, 0x1000, 0);
    data_454574 = eax;
    
    if (!eax)
        return 0;
    
    if (sub_412ff0())
        return 1;
    
    HeapDestroy(data_454574);
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
        sub_41200e(ebx_2, 0xffffffff);
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
                    sub_411fcc(ebx_2);
                    ebp_1 = ebx_2 + 0x10;
                    sub_41200e(ebx_2, esi_1);
                    int32_t ecx_2 = esi_1 * 3;
                    int32_t var_20_4 = 1;
                    sub_4120a2(*(edi_2 + (ecx_2 << 2) + 8), ebp_1);
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
    return sub_41200e(arg1[6], arg1[7]);
}

void* sub_412d80()
{
    void* result = data_43aab0;
    
    if (result != 1 && (result || data_43aab4 != 1))
        return result;
    
    sub_412dc0(0xfc);
    int32_t eax = data_43ae48;
    
    if (eax)
        eax();
    
    return sub_412dc0(0xff);
}

void* sub_412dc0(int32_t arg1)
{
    int32_t ecx = 0;
    void* result = &data_43adb8;
    
    while (*result != arg1)
    {
        result += 8;
        ecx += 1;
        
        if (result >= &data_43ae48)
            break;
    }
    
    if (*((ecx << 3) + &data_43adb8) == arg1)
    {
        OVERLAPPED* var_1bc;
        
        if (data_43aab0 == 1 || (!data_43aab0 && data_43aab4 == 1))
        {
            HANDLE hFile;
            
            if (data_454580)
                hFile = *(data_454580 + 0x10);
            
            if (!data_454580 || hFile == 0xffffffff)
            {
                var_1bc = 0xfffffff4;
                hFile = GetStdHandle(var_1bc);
            }
            
            uint8_t* lpBuffer = (&data_43adbc)[ecx * 2];
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
                sub_416300(ebp, "...", var_1bc);
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
            void* const edi_13 = &data_41b5e4;
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
            int32_t edi_18 = (&data_43adbc)[ecx * 2];
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
            return sub_416260(&var_1a4, "Microsoft Visual C++ Runtime Library", var_1bc);
        }
    }
    
    return result;
}

int32_t sub_412fc0(int32_t arg1)
{
    int32_t ecx = data_452864;
    
    if (ecx && ecx(arg1))
        return 1;
    
    return 0;
}

void** sub_412ff0()
{
    void** lpMem;
    
    if (data_43b660)
    {
        lpMem = HeapAlloc(data_454574, HEAP_NONE, 0x814);
        
        if (!lpMem)
            return 0;
    }
    else
        lpMem = &data_43ae50;
    
    int32_t* lpAddress = VirtualAlloc(nullptr, &__dos_header, MEM_RESERVE, PAGE_READWRITE);
    
    if (lpAddress)
    {
        if (VirtualAlloc(lpAddress, 0x10000, MEM_COMMIT, PAGE_READWRITE))
        {
            if (lpMem != &data_43ae50)
            {
                *lpMem = &data_43ae50;
                lpMem[1] = data_43ae54;
                data_43ae54 = lpMem;
                *lpMem[1] = lpMem;
            }
            else
            {
                if (!data_43ae50)
                    data_43ae50 = &data_43ae50;
                
                if (!data_43ae54)
                    data_43ae54 = &data_43ae50;
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
    
    if (lpMem != &data_43ae50)
        HeapFree(data_454574, HEAP_NONE, lpMem);
    
    return 0;
}

BOOL sub_413160(int32_t* arg1)
{
    BOOL result = VirtualFree(arg1[0x204], 0, MEM_RELEASE);
    
    if (data_43b664 == arg1)
    {
        result = arg1[1];
        data_43b664 = result;
    }
    
    if (arg1 == &data_43ae50)
    {
        data_43b660 = 0;
        return result;
    }
    
    *arg1[1] = *arg1;
    *(*arg1 + 4) = arg1[1];
    return HeapFree(data_454574, HEAP_NONE, arg1);
}

void sub_4131c0(int32_t arg1)
{
    void* esi = data_43ae54;
    
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
                    data_43b668 -= 1;
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
                    sub_413160(eax);
            }
        }
        
        if (esi == data_43ae54)
            break;
    } while (arg1 > 0);
}

int32_t sub_4132a0(int32_t arg1, void*** arg2, int32_t* arg3)
{
    void** i = &data_43ae50;
    
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
    } while (i != &data_43ae50);
    
    return 0;
}

char* sub_413300(void* arg1, int32_t arg2, char* arg3)
{
    void* ecx_2 = ((arg2 - *(arg1 + 0x810)) >> 0xc) + arg1;
    *(ecx_2 + 0x10) += *arg3;
    *arg3 = 0;
    *(ecx_2 + 0x410) = 0xf1;
    
    if (*(ecx_2 + 0x10) == 0xf0)
    {
        data_43b668 += 1;
        
        if (data_43b668 == 0x20)
            return sub_4131c0(0x10);
    }
    
    return arg3;
}

void* sub_413350(void* arg1)
{
    void* i = data_43b664;
    
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
                            eax_4 = sub_4135d0(*(i + 0x810) + j, eax_1, arg1);
                            
                            if (eax_4)
                            {
                                data_43b664 = i;
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
                            eax_8 = sub_4135d0(*(i + 0x810) + ebp_2, eax_5, arg1);
                            
                            if (eax_8)
                            {
                                data_43b664 = i;
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
    } while (data_43b664 != i);
    
    void** i_1 = &data_43ae50;
    
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
            
            data_43b664 = i_1;
            
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
    } while (i_1 != &data_43ae50);
    
    void** eax_9 = sub_412ff0();
    
    if (!eax_9)
        return 0;
    
    void** edx = eax_9[0x204];
    edx[2] = arg1;
    data_43b664 = eax_9;
    *edx = arg1 + edx + 8;
    edx[1] = 0xf0 - arg1;
    eax_9[4] -= arg1;
    return eax_9[0x204] + 0x100;
}

void* sub_4135d0(int32_t* arg1, int32_t arg2, void* arg3)
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

int32_t sub_413750(int32_t arg1)
{
    if (data_454680 > arg1)
    {
        int32_t esi_1 = (arg1 & 0x1f) << 3;
        
        if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + esi_1 + 4) & 1)
        {
            enum WIN32_ERROR ebp_2;
            
            if (arg1 == 1 || arg1 == 2)
            {
                if (sub_416600(2) != sub_416600(1))
                    goto label_4137b5;
                
                ebp_2 = NO_ERROR;
            }
            else
            {
            label_4137b5:
                
                if (CloseHandle(sub_416600(arg1)))
                    ebp_2 = NO_ERROR;
                else
                    ebp_2 = GetLastError();
            }
            
            sub_416570(arg1);
            
            if (!ebp_2)
            {
                *(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + esi_1 + 4) = 0;
                return 0;
            }
            
            sub_4144a0(ebp_2);
            return 0xffffffff;
        }
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

char sub_413820(int32_t* arg1)
{
    char result = arg1[3];
    
    if (result & 0x83 && result & 8)
    {
        result = sub_411250(arg1[2]);
        *arg1 = 0;
        arg1[3] &= 0xfffffbf7;
        arg1[2] = 0;
        arg1[1] = 0;
    }
    
    return result;
}

int32_t sub_413860(int32_t* arg1)
{
    if (!arg1)
        return sub_413930(0);
    
    if (sub_4138b0(arg1))
        return 0xffffffff;
    
    if (!(*(arg1 + 0xd) & 0x40))
        return 0;
    
    return 0 - 1;
}

int32_t sub_4138b0(int32_t* arg1)
{
    int32_t result = 0;
    int32_t eax = arg1[3];
    
    if ((eax & 3) == 2 && eax & 0x108)
    {
        char* eax_1 = arg1[2];
        uint32_t ebx_2 = *arg1 - eax_1;
        
        if (ebx_2 > 0)
        {
            if (sub_4141b0(arg1[4], eax_1, ebx_2) != ebx_2)
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

int32_t sub_413920()
{
    return sub_413930(1);
}

int32_t sub_413930(int32_t arg1)
{
    int32_t ebx = 0;
    int32_t i = 0;
    int32_t var_4 = 0;
    int32_t esi;
    
    if (data_454570 <= 0)
        esi = arg1;
    else
    {
        int32_t ebp_1 = 0;
        esi = arg1;
        
        do
        {
            int32_t* ecx_1 = *(data_453564 + ebp_1);
            
            if (ecx_1)
            {
                char eax_2 = ecx_1[3];
                
                if (eax_2 & 0x83)
                {
                    if (esi == 1)
                    {
                        if (sub_413860(ecx_1) != 0xffffffff)
                            ebx += 1;
                    }
                    else if (!esi && eax_2 & 2 && sub_413860(ecx_1) == 0xffffffff)
                        var_4 = 0xffffffff;
                }
            }
            
            ebp_1 += 4;
            i += 1;
        } while (i < data_454570);
    }
    
    if (esi == 1)
        return ebx;
    
    return var_4;
}

int32_t sub_4139c0(int32_t* arg1)
{
    int32_t eax = arg1[3];
    
    if (!(eax & 0x83) || eax & 0x40)
        return 0xffffffff;
    
    if (eax & 2)
    {
        arg1[3] = eax | 0x20;
        return 0xffffffff;
    }
    
    int32_t eax_3 = eax | 1;
    arg1[3] = eax_3;
    
    if (eax_3 & 0x10c)
        *arg1 = arg1[2];
    else
        sub_4166c0(arg1);
    
    void* eax_6 = sub_413ab0(arg1[4], arg1[2], arg1[6]);
    arg1[1] = eax_6;
    
    if (!eax_6 || eax_6 == 0xffffffff)
    {
        arg1[3] |= ((eax_6 - eax_6) & 0xfffffff0) + 0x20;
        arg1[1] = 0;
        return 0xffffffff;
    }
    
    int32_t edx_1 = arg1[3];
    
    if (!(edx_1 & 0x82))
    {
        int32_t ecx_1 = arg1[4];
        void* eax_7 = &data_43ada8;
        
        if (ecx_1 != 0xffffffff)
            eax_7 = *(((ecx_1 & 0xffffffe7) >> 3) + &data_454580) + ((ecx_1 & 0x1f) << 3);
        
        eax_7 = *(eax_7 + 4);
        eax_7 &= 0x82;
        
        if (eax_7 == 0x82)
            arg1[3] = edx_1 | 0x2000;
    }
    
    if (arg1[6] == 0x200)
    {
        int16_t eax_12 = arg1[3];
        
        if (eax_12 & 8 && !(*eax_12[1] & 4))
            arg1[6] = 0x1000;
    }
    
    arg1[1] -= 1;
    char* ecx_3 = *arg1;
    *arg1 = &ecx_3[1];
    int32_t result;
    result = *ecx_3;
    return result;
}

void* sub_413ab0(int32_t arg1, char* arg2, uint32_t arg3)
{
    if (data_454680 > arg1)
    {
        int32_t ebx_1 = (arg1 & 0x1f) << 3;
        void* eax_5 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ebx_1;
        
        if (*(eax_5 + 4) & 1)
        {
            char* lpBuffer = arg2;
            uint32_t nNumberOfBytesToRead = arg3;
            int32_t var_c = 0;
            
            if (!nNumberOfBytesToRead || *(eax_5 + 4) & 2)
                return 0;
            
            if (*(eax_5 + 4) & 0x48)
            {
                eax_5 = *(eax_5 + 5);
                
                if (eax_5 != 0xa)
                {
                    *lpBuffer = eax_5;
                    lpBuffer = &lpBuffer[1];
                    nNumberOfBytesToRead -= 1;
                    var_c = 1;
                    *(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 5) = 0xa;
                }
            }
            
            uint32_t numberOfBytesRead;
            
            if (!ReadFile((*(((arg1 & 0xffffffe7) >> 3) + &data_454580))[(arg1 & 0x1f) * 2], 
                lpBuffer, nNumberOfBytesToRead, &numberOfBytesRead, nullptr))
            {
                enum WIN32_ERROR eax_8 = GetLastError();
                
                if (eax_8 != ERROR_ACCESS_DENIED)
                {
                    if (eax_8 == ERROR_BROKEN_PIPE)
                        return 0;
                    
                    sub_4144a0(eax_8);
                    return 0xffffffff;
                }
                
                data_43aa5c = eax_8;
                data_43aa58 = 9;
                return 0xffffffff;
            }
            
            void* result = var_c + numberOfBytesRead;
            char* eax_13 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 4;
            void* ecx_1;
            ecx_1 = *eax_13;
            
            if (!(ecx_1 & 0x80))
                return result;
            
            if (!numberOfBytesRead || *arg2 != 0xa)
                ecx_1 &= 0xfb;
            else
                ecx_1 |= 4;
            
            char* edi_1 = arg2;
            *eax_13 = ecx_1;
            char* esi_1 = edi_1;
            void* eax_15 = result + edi_1;
            void* var_4_1 = eax_15;
            
            if (eax_15 > edi_1)
            {
                do
                {
                    eax_15 = *esi_1;
                    
                    if (eax_15 == 0x1a)
                    {
                        char* eax_19 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 4;
                        ecx_1 = *eax_19;
                        
                        if (!(ecx_1 & 0x40))
                        {
                            ecx_1 |= 2;
                            *eax_19 = ecx_1;
                        }
                        
                        break;
                    }
                    
                    if (eax_15 != 0xd)
                    {
                        esi_1 = &esi_1[1];
                        *edi_1 = eax_15;
                        edi_1 = &edi_1[1];
                    }
                    else if (var_4_1 - 1 <= esi_1)
                    {
                        esi_1 = &esi_1[1];
                        void* var_c_1 = nullptr;
                        uint8_t buffer;
                        eax_15 = ReadFile(
                            (*(((arg1 & 0xffffffe7) >> 3) + &data_454580))[(arg1 & 0x1f) * 2], 
                            &buffer, 1, &numberOfBytesRead, nullptr);
                        
                        if (!eax_15)
                        {
                            eax_15 = GetLastError();
                            var_c_1 = eax_15;
                        }
                        
                        if (var_c_1 || !numberOfBytesRead)
                        {
                            *edi_1 = 0xd;
                            edi_1 = &edi_1[1];
                        }
                        else if (
                            !(*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 4) & 0x48))
                        {
                            if (edi_1 != arg2 || buffer != 0xa)
                            {
                                eax_15 = sub_4143e0(arg1, 0xffffffff, FILE_CURRENT);
                                
                                if (buffer != 0xa)
                                {
                                    *edi_1 = 0xd;
                                    edi_1 = &edi_1[1];
                                }
                            }
                            else
                            {
                                *edi_1 = 0xa;
                                edi_1 = &edi_1[1];
                            }
                        }
                        else if (buffer != 0xa)
                        {
                            *edi_1 = 0xd;
                            edi_1 = &edi_1[1];
                            eax_15 = buffer;
                            *(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 5) = eax_15;
                        }
                        else
                        {
                            *edi_1 = 0xa;
                            edi_1 = &edi_1[1];
                        }
                    }
                    else
                    {
                        if (esi_1[1] != 0xa)
                        {
                            esi_1 = &esi_1[1];
                            *edi_1 = eax_15;
                        }
                        else
                        {
                            esi_1 = &esi_1[2];
                            *edi_1 = 0xa;
                        }
                        
                        edi_1 = &edi_1[1];
                    }
                } while (esi_1 < var_4_1);
            }
            
            return edi_1 - arg2;
        }
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

int32_t sub_413d20(int32_t* arg1, void* arg2, int32_t arg3, enum SET_FILE_POINTER_MOVE_METHOD arg4)
{
    int32_t eax = arg1[3];
    
    if (eax & 0x83)
    {
        enum SET_FILE_POINTER_MOVE_METHOD edi_1 = arg4;
        
        if (!edi_1 || edi_1 == FILE_CURRENT || edi_1 == FILE_END)
        {
            arg1[3] = eax & 0xffffffef;
            
            if (edi_1 == FILE_CURRENT)
            {
                void* eax_2;
                int32_t edx_1;
                eax_2 = sub_416800(arg1);
                void* temp0_1 = arg2;
                arg2 += eax_2;
                arg3 = arg3 + edx_1;
                edi_1 = FILE_BEGIN;
            }
            
            sub_4138b0(arg1);
            int32_t eax_3 = arg1[3];
            
            if (eax_3 & 0x80)
                arg1[3] = eax_3 & 0xfffffffc;
            else if (eax_3 & 1 && eax_3 & 8 && !(*eax_3[1] & 4))
                arg1[6] = 0x200;
            
            uint32_t eax_6;
            int32_t edx_3;
            eax_6 = sub_416710(arg1[4], arg2, arg3, edi_1);
            
            if (edx_3 == 0xffffffff && eax_6 == 0xffffffff)
                return 0xffffffff;
            
            return 0;
        }
    }
    
    data_43aa58 = 0x16;
    return 0xffffffff;
}

int32_t* sub_413dd0(PSTR arg1, char* arg2, int32_t arg3, int32_t* arg4)
{
    int32_t eax = *arg2;
    int32_t edx;
    int32_t esi_1;
    
    if (eax == 0x61)
    {
        edx = 0x109;
        esi_1 = data_43be00 | 2;
    }
    else if (eax == 0x72)
    {
        edx = 0;
        esi_1 = data_43be00 | 1;
    }
    else
    {
        if (eax != 0x77)
            return 0;
        
        edx = 0x301;
        esi_1 = data_43be00 | 2;
    }
    
    int32_t i = 1;
    void* ecx_1 = &arg2[1];
    
    if (*ecx_1)
    {
        while (i)
        {
            if (*ecx_1 - 0x2b > 0x49)
                i = 0;
            else if (!(edx & 2))
            {
                edx = (edx | 2) & 0xfffffffe;
                esi_1 = (esi_1 | 0x80) & 0xfffffffc;
            }
            else
                i = 0;
            
            ecx_1 += 1;
            
            if (!*ecx_1)
                break;
        }
    }
    
    int32_t eax_5 = sub_4169e0(arg1, edx, arg3, 0x1a4);
    
    if (eax_5 < 0)
        return 0;
    
    data_43ba68 += 1;
    arg4[3] = esi_1;
    arg4[1] = 0;
    *arg4 = 0;
    arg4[2] = 0;
    arg4[7] = 0;
    arg4[4] = eax_5;
    return arg4;
}

int32_t* __convention("regparm") sub_413e84(int32_t arg1, int32_t arg2, char* arg3, int32_t arg4 @ ebp, int32_t arg5 @ esi, int32_t arg6 @ edi, int32_t arg7, int32_t arg8, int32_t arg9, PSTR arg10, int32_t arg11, int32_t* arg12)
{
    while (true)
    {
        if (!(arg2 & 0x40))
            arg2 |= 0x40;
        else
            arg4 = 0;
        
        while (true)
        {
            arg3 = &arg3[1];
            
            if (!*arg3 || !arg4)
            {
                int32_t eax_1 = sub_4169e0(arg10, arg2, arg11, 0x1a4);
                
                if (eax_1 >= 0)
                {
                    data_43ba68 += 1;
                    arg12[3] = arg5;
                    arg12[1] = 0;
                    *arg12 = 0;
                    arg12[2] = 0;
                    arg12[7] = 0;
                    arg12[4] = eax_1;
                    return arg12;
                }
                
                return 0;
            }
            
            int32_t eax_5 = *arg3 - 0x2b;
            
            if (eax_5 <= 0x49)
            {
                int32_t arg_10 = 0;
                eax_5 = lookup_table_413f94[eax_5];
                arg_10 = eax_5;
                int32_t entry_ebx;
                
                switch (arg_10)
                {
                    case 0:
                    {
                        if (!(arg2 & 2))
                        {
                            arg2 = (arg2 | 2) & 0xfffffffe;
                            arg5 = (arg5 | 0x80) & 0xfffffffc;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 1:
                    {
                        break;
                        break;
                    }
                    case 2:
                    {
                        if (!entry_ebx)
                        {
                            entry_ebx = 1;
                            arg2 |= 0x10;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 3:
                    {
                        if (!entry_ebx)
                        {
                            entry_ebx = 1;
                            arg2 |= 0x20;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 4:
                    {
                        if (!(*arg2[1] & 0x10))
                        {
                            arg2 |= 0x1000;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 5:
                    {
                        if (!(*arg2[1] & 0xc0))
                        {
                            arg2 |= 0x8000;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 6:
                    {
                        if (!arg6)
                        {
                            arg6 = 1;
                            arg5 |= 0x4000;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 7:
                    {
                        if (!arg6)
                        {
                            arg6 = 1;
                            arg5 &= 0xffffbfff;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                    case 8:
                    {
                        if (!(*arg2[1] & 0xc0))
                        {
                            arg2 |= 0x4000;
                            continue;
                        }
                        else
                        {
                            arg4 = 0;
                            continue;
                        }
                        break;
                    }
                }
            }
            
            arg4 = 0;
            continue;
        }
    }
}

int32_t* sub_413fe0()
{
    int32_t* result = nullptr;
    int32_t i = 0;
    
    if (data_454570 > 0)
    {
        int32_t* ecx_1 = data_453564;
        
        do
        {
            void* eax_1 = *ecx_1;
            
            if (!eax_1)
            {
                *(data_453564 + (i << 2)) = sub_4111a0(0x20);
                int32_t* result_1 = *(data_453564 + (i << 2));
                
                if (result_1)
                    result = result_1;
                
                break;
            }
            
            if (!(*(eax_1 + 0xc) & 0x83))
            {
                result = *(data_453564 + (i << 2));
                break;
            }
            
            ecx_1 = &ecx_1[1];
            i += 1;
        } while (data_454570 > i);
    }
    
    if (result)
    {
        result[1] = 0;
        result[3] = 0;
        result[2] = 0;
        *result = 0;
        result[7] = 0;
        result[4] = 0xffffffff;
    }
    
    return result;
}

uint32_t sub_414060(int32_t* arg1)
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
    
    void* ebp = nullptr;
    int32_t eax_4 = arg1[3] | 2;
    arg1[3] = eax_4;
    arg1[3] = eax_4 & 0xffffffef;
    arg1[1] = 0;
    
    if (!(arg1[3] & 0x10c))
    {
        if (arg1 == 0x43b808 || arg1 == 0x43b828)
        {
            if (!sub_416e00(edi))
                sub_4166c0(arg1);
        }
        else
            sub_4166c0(arg1);
    }
    
    int32_t arg_4;
    uint32_t ebx_1;
    
    if (!(arg1[3] & 0x108))
    {
        ebx_1 = 1;
        ebp = sub_4141b0(edi, &arg_4, 1);
    }
    else
    {
        int32_t eax_7 = arg1[2];
        ebx_1 = *arg1 - eax_7;
        *arg1 = eax_7 + 1;
        arg1[1] = arg1[6] - 1;
        void* eax_12;
        
        if (ebx_1 <= 0)
        {
            eax_12 = &data_43ada8;
            
            if (edi != 0xffffffff)
                eax_12 = *(((edi & 0xffffffe7) >> 3) + &data_454580) + ((edi & 0x1f) << 3);
            
            if (*(eax_12 + 4) & 0x20)
                sub_4143e0(edi, 0, FILE_END);
        }
        else
            ebp = sub_4141b0(edi, arg1[2], ebx_1);
        
        eax_12 = arg_4;
        *arg1[2] = eax_12;
    }
    
    if (ebp == ebx_1)
        return arg_4;
    
    arg1[3] |= 0x20;
    return 0xffffffff;
}

int32_t sub_4141b0(int32_t arg1, char* arg2, uint32_t arg3)
{
    if (data_454680 > arg1)
    {
        int32_t eax_7 = (arg1 & 0x1f) << 3;
        int32_t var_40c_1 = eax_7;
        eax_7 = *(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + eax_7 + 4);
        
        if (eax_7 & 1)
        {
            int32_t esi = 0;
            uint32_t numberOfBytesWritten_1 = 0;
            
            if (!arg3)
                return 0;
            
            if (eax_7 & 0x20)
                sub_4143e0(arg1, 0, FILE_END);
            
            int32_t* ecx_3 = var_40c_1 + *(((arg1 & 0xffffffe7) >> 3) + &data_454580);
            enum WIN32_ERROR var_418_1;
            uint32_t numberOfBytesWritten;
            
            if (!(ecx_3[1] & 0x80))
            {
                if (!WriteFile(*ecx_3, arg2, arg3, &numberOfBytesWritten, nullptr))
                {
                label_4142fb:
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
                    
                    if (!WriteFile(*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + var_40c_1), 
                            &buffer, nNumberOfBytesToWrite, &numberOfBytesWritten, nullptr))
                        goto label_4142fb;
                    
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
                if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + var_40c_1 + 4) & 0x40
                        && *arg2 == 0x1a)
                    return 0;
                
                data_43aa58 = 0x1c;
                data_43aa5c = 0;
                return 0xffffffff;
            }
            
            if (var_418_1 != ERROR_ACCESS_DENIED)
            {
                sub_4144a0(var_418_1);
                return 0xffffffff;
            }
            
            data_43aa58 = 9;
            data_43aa5c = var_418_1;
            return 0xffffffff;
        }
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

uint32_t sub_4143e0(int32_t arg1, int32_t arg2, enum SET_FILE_POINTER_MOVE_METHOD arg3)
{
    if (data_454680 > arg1)
    {
        int32_t esi_1 = (arg1 & 0x1f) << 3;
        
        if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + esi_1 + 4) & 1)
        {
            HANDLE hFile = sub_416600(arg1);
            
            if (hFile == 0xffffffff)
            {
                data_43aa58 = 9;
                return 0xffffffff;
            }
            
            uint32_t result = SetFilePointer(hFile, arg2, nullptr, arg3);
            enum WIN32_ERROR eax_7 = NO_ERROR;
            
            if (result == 0xffffffff)
                eax_7 = GetLastError();
            
            if (eax_7)
            {
                sub_4144a0(eax_7);
                return 0xffffffff;
            }
            
            int32_t eax_9 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580);
            *(eax_9 + esi_1 + 4) &= 0xfd;
            return result;
        }
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

int32_t sub_4144a0(int32_t arg1)
{
    int32_t eax = 0;
    void* i = &data_43b670;
    data_43aa5c = arg1;
    
    do
    {
        if (*i == arg1)
        {
            int32_t eax_1 = *((eax << 3) + &data_43b674);
            data_43aa58 = eax_1;
            return eax_1;
        }
        
        i += 8;
        eax += 1;
    } while (i < 0x43b7d8);
    
    if (arg1 >= 0x13 && arg1 <= 0x24)
    {
        data_43aa58 = 0xd;
        return eax;
    }
    
    if (arg1 >= 0xbc && arg1 <= 0xca)
    {
        data_43aa58 = 8;
        return eax;
    }
    
    data_43aa58 = 0x16;
    return eax;
}

int32_t __fastcall sub_414510(int32_t, int32_t arg2) __pure
{
    return arg2 - 1;
}

void* sub_414520(char* arg1, char arg2)
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
            return sub_414510(ecx, edx);
        
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

int32_t sub_4145e0(int32_t* arg1)
{
    if (!sub_416e00(arg1[4]))
        return 0;
    
    int32_t eax_3;
    
    if (arg1 != 0x43b808)
    {
        if (arg1 != 0x43b828)
            return 0;
        
        eax_3 = 1;
    }
    else
        eax_3 = 0;
    
    data_43ba68 += 1;
    
    if (arg1[3] & 0x10c)
        return 0;
    
    if (!*((eax_3 << 2) + &data_43b7e0))
    {
        void* eax_5 = sub_4111a0(0x1000);
        *((eax_3 << 2) + &data_43b7e0) = eax_5;
        
        if (!eax_5)
            return 0;
    }
    
    int32_t eax_7 = *((eax_3 << 2) + &data_43b7e0);
    arg1[2] = eax_7;
    *arg1 = eax_7;
    arg1[6] = 0x1000;
    arg1[1] = 0x1000;
    arg1[3] |= 0x1102;
    return 1;
}

void sub_414680(int32_t arg1, int32_t* arg2)
{
    if (!arg1)
    {
        if (*(arg2 + 0xd) & 0x10)
            sub_4138b0(arg2);
    }
    else if (*(arg2 + 0xd) & 0x10)
    {
        sub_4138b0(arg2);
        arg2[3] &= 0xffffeeff;
        arg2[6] = 0;
        *arg2 = 0;
        arg2[2] = 0;
    }
}

int32_t sub_4146d0()
{
    if (!data_454570)
        data_454570 = 0x200;
    else if (data_454570 < 0x14)
        data_454570 = 0x14;
    
    void* eax_1 = sub_4117e0(data_454570, 4);
    data_453564 = eax_1;
    
    if (!eax_1)
    {
        data_454570 = 0x14;
        void* eax_2 = sub_4117e0(0x14, 4);
        data_453564 = eax_2;
        
        if (!eax_2)
            sub_411170(0x1a);
    }
    
    void** ecx = &data_43b7e8;
    
    for (int32_t i = 0; i < 0x50; )
    {
        i += 4;
        *(data_453564 + i - 4) = ecx;
        ecx = &ecx[8];
    }
    
    int32_t esi = 0;
    int32_t result;
    
    for (void* i_1 = &data_43b7f8; i_1 < 0x43b858; )
    {
        result = *(*(((esi & 0xffffffe7) >> 3) + &data_454580) + ((esi & 0x1f) << 3));
        
        if (result == 0xffffffff || !result)
            *i_1 = 0xffffffff;
        
        i_1 += 0x20;
        esi += 1;
    }
    
    return result;
}

int32_t sub_4147a0()
{
    int32_t result = sub_413920();
    
    if (!data_43aa98)
        return result;
    
    /* tailcall */
    return sub_416e30();
}

int32_t sub_4147c0(int32_t* arg1, char* arg2)
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
                eax_1 = data_41b608[0x10][ebx];
                eax_2 = eax_1 & 0xf;
            }
            
            eax_2 = *(var_21c + (eax_2 << 3) + 0x41b638);
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
                    goto label_41499b;
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
                        eax_4 = *(ebx + &jump_table_415058[6]);
                        
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
                        int32_t eax_5 = sub_415240(&arg_c);
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
                        ebp_1 = sub_415240(&arg_c);
                        
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
                        eax_10 = *(ebx + &*jump_table_415058[5][3]);
                        
                        switch (eax_10)
                        {
                            case 0:
                            {
                                if (*arg2 != 0x36 || arg2[1] != 0x34)
                                {
                                    var_21c = 0;
                                label_41499b:
                                    int32_t eax_12;
                                    eax_12 = ebx;
                                    var_220 = 0;
                                    
                                    if (*(&(**&data_43bb90)[eax_12] + 1) & 0x80)
                                    {
                                        sub_415170(ebx, arg1, &i);
                                        ebx = *arg2;
                                        arg2 = &arg2[1];
                                    }
                                    
                                    sub_415170(ebx, arg1, &i);
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
                        eax_15 = *(ebx + &*jump_table_4150e8[0][1]);
                        int32_t var_218_1;
                        int32_t var_214;
                        
                        switch (eax_15)
                        {
                            case 0:
                            {
                                if (!(esi_1 & 0x830))
                                    esi_1 |= 0x800;
                                
                                goto label_414a60;
                            }
                            case 1:
                            case 2:
                            {
                                var_20c = 1;
                                ebx += 0x20;
                            label_414a9f:
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
                                data_43aac0(&var_208, &var_200, ebx, ebp_1, var_20c);
                                void* edi_8 = esi_1 & 0x80;
                                
                                if (edi_8 && !ebp_1)
                                    data_43aacc(&var_200);
                                
                                if (ebx == 0x67 && !edi_8)
                                    data_43aac4(&var_200);
                                
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
                                
                                goto label_414aca;
                            }
                            case 4:
                            {
                                goto label_414bf8;
                            }
                            case 5:
                            {
                                int16_t* eax_22 = sub_415240(&arg_c);
                                void* ecx_17;
                                
                                if (eax_22)
                                    ecx_17 = *(eax_22 + 4);
                                
                                if (!eax_22 || !ecx_17)
                                {
                                    char const (* eax_23)[0x7] = data_43ba6c;
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
                            label_414a60:
                                int32_t* var_25c_5 = &arg_c;
                                
                                if (!(esi_1 & 0x810))
                                {
                                    edi_1 = 1;
                                    var_200 = sub_415240(var_25c_5);
                                }
                                else
                                {
                                    edi_1 = sub_416eb0(&var_200, sub_415270(var_25c_5));
                                    
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
                            label_414c2d:
                                int32_t var_23c_1;
                                int32_t var_238_1;
                                
                                if (esi_1 & 0x8000)
                                {
                                    int32_t eax_26;
                                    int32_t edx_4;
                                    eax_26 = sub_415250(&arg_c);
                                    var_23c_1 = eax_26;
                                    var_238_1 = edx_4;
                                }
                                else if (!(esi_1 & 0x20))
                                {
                                    int32_t* var_25c_12 = &arg_c;
                                    
                                    if (!(esi_1 & 0x40))
                                    {
                                        var_23c_1 = sub_415240(var_25c_12);
                                        var_238_1 = 0;
                                    }
                                    else
                                    {
                                        int32_t eax_32 = sub_415240(var_25c_12);
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
                                        var_23c_1 = sub_415240(var_25c_11);
                                        var_238_1 = 0;
                                    }
                                    else
                                    {
                                        int32_t eax_28 = sub_415240(var_25c_11);
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
                                goto label_414a9f;
                            }
                            case 0xa:
                            {
                                int16_t* eax_24 = sub_415240(&arg_c);
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
                                
                                goto label_414c2d;
                            }
                            case 0xc:
                            {
                                ebp_1 = 8;
                            label_414bf8:
                                var_214 = 7;
                            label_414c00:
                                var_218_1 = 0x10;
                                
                                if (esi_1 & 0x80)
                                {
                                    var_246 = 0x30;
                                    var_224 = 2;
                                    char var_245_1 = var_214 + 0x51;
                                }
                                
                                goto label_414c2d;
                            }
                            case 0xd:
                            {
                            label_414aca:
                                int32_t* ebx_1 = 0x7fffffff;
                                
                                if (ebp_1 != 0xffffffff)
                                    ebx_1 = ebp_1;
                                
                                void* eax_18 = sub_415240(&arg_c);
                                var_240 = eax_18;
                                
                                if (!(esi_1 & 0x810))
                                {
                                    if (!var_240)
                                        var_240 = data_43ba6c;
                                    
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
                                        var_240 = data_43ba70;
                                    
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
                                goto label_414c2d;
                            }
                            case 0xf:
                            {
                                var_214 = 0x27;
                                goto label_414c00;
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
                            sub_4151c0(0x20, var_230_2, arg1, &i);
                        
                        sub_415200(&var_246, var_224, arg1, &i);
                        
                        if (esi_1 & 8 && !(esi_1 & 4))
                            sub_4151c0(0x30, var_230_2, arg1, &i);
                        
                        if (!var_220 || edi_1 <= 0)
                            sub_415200(var_240, edi_1, arg1, &i);
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
                                int32_t* eax_58 = sub_416eb0(&var_244, eax_57);
                                
                                if (eax_58 <= 0)
                                    break;
                                
                                sub_415200(&var_244, eax_58, arg1, &i);
                                j_2 = j_3;
                                j_3 -= 1;
                            } while (j_2);
                        }
                        
                        if (esi_1 & 4)
                            sub_4151c0(0x20, var_230_2, arg1, &i);
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

int32_t* sub_415170(int32_t* arg1, int32_t* arg2, int32_t* arg3)
{
    int32_t eax = arg2[1];
    arg2[1] = eax - 1;
    uint32_t eax_3;
    
    if (eax - 1 < 0)
    {
        int32_t* var_4_1 = arg2;
        eax_3 = sub_414060(arg1);
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

int32_t* sub_4151c0(int32_t* arg1, int32_t* arg2, int32_t* arg3, int32_t* arg4)
{
    int32_t* result_1 = arg2;
    int32_t* result;
    
    do
    {
        result = result_1;
        result_1 -= 1;
        
        if (result <= 0)
            break;
        
        result = sub_415170(arg1, arg3, arg4);
    } while (*arg4 != 0xffffffff);
    
    return result;
}

int32_t* sub_415200(char* arg1, int32_t* arg2, int32_t* arg3, int32_t* arg4)
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
        result = sub_415170(*eax_1, arg3, arg4);
    } while (*arg4 != 0xffffffff);
    
    return result;
}

int32_t sub_415240(int32_t* arg1)
{
    void* ecx_1 = *arg1 + 4;
    *arg1 = ecx_1;
    return *(ecx_1 - 4);
}

int32_t sub_415250(int32_t* arg1)
{
    void* ecx_1 = *arg1 + 8;
    *arg1 = ecx_1;
    *(ecx_1 - 4);
    return *(ecx_1 - 8);
}

int32_t* sub_415270(int32_t* arg1)
{
    void* ecx_1 = *arg1 + 4;
    *arg1 = ecx_1;
    int32_t* result;
    result = *(ecx_1 - 4);
    return result;
}

int32_t __fastcall sub_415280(int16_t arg1, void* arg2 @ ebp, long double arg3 @ st0, long double arg4 @ st1, long double arg5 @ st2, long double arg6 @ st3)
{
    *(arg2 - 0x90) = 0xfe;
    *arg1[1] = *arg1[1];
    long double x87_r0;
    long double x87_r1;
    
    if (*arg1[1])
    {
        int32_t eax_1 = _isintTOS(arg3);
        
        if (!eax_1)
            /* tailcall */
            return __rtindfpop(arg2);
        
        *arg1[1] = 0;
        
        if (eax_1 != 2)
            *arg1[1] = 0xff;
        
        x87_r1 = arg3;
        x87_r0 = fabsl(arg4);
    }
    else
    {
        x87_r0 = arg4;
        x87_r1 = arg3;
    }
    
    int16_t ecx;
    char edx;
    ecx = __ffexpm1(arg1, arg2, __fyl2x(x87_r0, x87_r1));
    long double x87_r2 = arg5 + 1;
    
    if (*(arg2 - 0x9f) & 1)
    {
        long double x87_r1_3 = 1;
        
        if (data_43aa48 == 1)
            x87_r2 = sub_417d11(x87_r1_3, x87_r2);
        else
            x87_r2 = x87_r1_3 / x87_r1_3;
    }
    
    if (!(edx & 0x40))
        __fscale(x87_r2, arg6);
    
    *ecx[1] = *ecx[1];
    /* tailcall */
    return sub_417178();
}

int80_t sub_415291(void* arg1 @ ebp, long double arg2 @ st0, long double arg3 @ st1, long double arg4 @ st2)
{
    *(arg1 - 0x90) = 0xfe;
    int16_t ecx;
    *ecx[1] = 0;
    int16_t ecx_1;
    char edx;
    ecx_1 = __ffexpm1(ecx, arg1, arg2 * (1.4426950407214463 + 1.675171316223821e-10));
    long double x87_r1 = arg3 + 1;
    
    if (*(arg1 - 0x9f) & 1)
    {
        long double x87_r0_2 = 1;
        
        if (data_43aa48 == 1)
            x87_r1 = sub_417d11(x87_r0_2, x87_r1);
        else
            x87_r1 = x87_r0_2 / x87_r0_2;
    }
    
    if (!(edx & 0x40))
        __fscale(x87_r1, arg4);
    
    *ecx_1[1] = *ecx_1[1];
    /* tailcall */
    return sub_417178();
}

int32_t j_sub_41718b()
{
    /* tailcall */
    return sub_41718b();
}

int32_t sub_4152f8(void* arg1 @ ebp)
{
    data_43ba80;
    *(arg1 - 0x90) = 2;
}

int80_t sub_415324(void* arg1 @ ebp)
{
    *(arg1 - 0x90) = 2;
    return data_43ba8a;
}

void __fastcall sub_415334(char arg1, void* arg2 @ ebp)
{
    if (!arg1)
        return;
    
    data_43be10;
    
    if (*(arg2 - 0x90) > 0)
        return;
    
    /* tailcall */
    return sub_417239(arg2);
}

int32_t __fastcall sub_415339(char arg1, long double arg2 @ st0)
{
    void* ebp;
    
    if (arg1)
        /* tailcall */
        return __rtindfpop(ebp);
    __fyl2x(arg2, 0.30102999560767785 + 5.6303348065105986e-11);
}

int32_t __fastcall sub_41533d(char arg1, long double arg2 @ st0)
{
    void* ebp;
    
    if (arg1)
        /* tailcall */
        return __rtindfpop(ebp);
    __fyl2x(arg2, 0.69314718048553914 + 7.4406171098029793e-11);
}

int32_t j_sub_41718b()
{
    /* tailcall */
    return sub_41718b();
}

int32_t __fastcall sub_41534d(int16_t arg1, void* arg2 @ ebp)
{
    long double x87_r0;
    int32_t result = _isintTOS(x87_r0);
    arg1 = arg1;
    
    if (arg1)
    {
        *(arg2 - 0x90) = 2;
        data_43ba80;
        
        if (result == 1)
            *arg1[1] = *arg1[1];
    }
    else if (result == 1)
        *arg1[1] = *arg1[1];
    
    return result;
}

int32_t sub_415389(void* arg1 @ ebp)
{
    data_43be10;
    
    if (*(arg1 - 0x90) > 0)
        return;
    
    /* tailcall */
    return sub_417239(arg1);
}

int32_t sub_4153bf(void* arg1 @ ebp)
{
    data_43ba80;
    *(arg1 - 0x90) = 3;
}

int80_t __fastcall __rtforexpinf(char arg1)
{
    if (arg1)
        /* tailcall */
        return sub_417186();
    
    return data_43ba80;
}

void __fastcall __ffexpm1(int16_t arg1, void* arg2 @ ebp, long double arg3 @ st0)
{
    long double x87_r7 = fabsl(arg3);
    long double x87_r6 = data_43ba9e;
    x87_r6 - x87_r7;
    *(arg2 - 0xa0) = (x87_r6 < x87_r7 ? 1 : 0) << 8 | (FCMP_UO(x87_r6, x87_r7) ? 1 : 0) << 0xa
        | (x87_r6 == x87_r7 ? 1 : 0) << 0xe;
    
    if (*(arg2 - 0x9f) & 0x41)
    {
        long double temp1 = 0;
        arg3 - temp1;
        *(arg2 - 0xa0) = (arg3 < temp1 ? 1 : 0) << 8 | (FCMP_UO(arg3, temp1) ? 1 : 0) << 0xa
            | (arg3 == temp1 ? 1 : 0) << 0xe;
        
        if (*(arg2 - 0x9f) & 1)
        {
            *(arg2 - 0x90) = 4;
            /* tailcall */
            return sub_417186();
        }
        
        data_43ba80;
        *arg1[1] = *arg1[1];
        return;
    }
    
    long double x87_r7_2 = round(arg3, arg3);
    long double temp2 = 0;
    x87_r7_2 - temp2;
    *(arg2 - 0xa0) = (x87_r7_2 < temp2 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_2, temp2) ? 1 : 0) << 0xa
        | (x87_r7_2 == temp2 ? 1 : 0) << 0xe | 0x3800;
    int32_t edx;
    edx = *(arg2 - 0x9f);
    long double x87_r7_4 = arg3 - x87_r7_2;
    long double temp3 = 0;
    x87_r7_4 - temp3;
    *(arg2 - 0xa0) = (x87_r7_4 < temp3 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_4, temp3) ? 1 : 0) << 0xa
        | (x87_r7_4 == temp3 ? 1 : 0) << 0xe | 0x3800;
    __f2xm1(fabsl(x87_r7_4));
}

int32_t _isintTOS(long double arg1 @ st0)
{
    long double x87_r7 = round(arg1, arg1);
    x87_r7 - arg1;
    
    if (!TEST_BITB(
            *((x87_r7 < arg1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7, arg1) ? 1 : 0) << 0xa
                | (x87_r7 == arg1 ? 1 : 0) << 0xe)[1], 
            6))
        return 0;
    
    long double x87_r7_2 = arg1 * data_43bab2;
    long double x87_r6_2 = round(x87_r7_2, arg1);
    x87_r6_2 - x87_r7_2;
    
    if (TEST_BITB(
            *((x87_r6_2 < x87_r7_2 ? 1 : 0) << 8 | (FCMP_UO(x87_r6_2, x87_r7_2) ? 1 : 0) << 0xa
                | (x87_r6_2 == x87_r7_2 ? 1 : 0) << 0xe)[1], 
            6))
        return 2;
    
    return 1;
}

long double sub_415455(int16_t arg1 @ x87control, int16_t arg2 @ x87status, int16_t arg3 @ x87tag, long double arg4 @ st0, long double arg5 @ st1)
{
    int16_t x87control;
    int16_t x87status;
    int16_t x87tag;
    uint864_t temp0;
    temp0 = __fnsave_memmem108(arg1, arg3, arg2);
    double var_78;
    int32_t eax = sub_417250(arg5, arg4, &var_78);
    __frstor_memmem108(temp0);
    
    if (!eax)
        return var_78;
    
    void* ebp;
    /* tailcall */
    return sub_417239(ebp);
}

int80_t sub_415490(int16_t arg1 @ x87control, long double arg2 @ st0, long double arg3 @ st1)
{
    int32_t __saved_ebp;
    int32_t* ebp = &__saved_ebp;
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int16_t var_a8 = temp0;
    
    if (!data_43c0b8)
    {
        double var_8a_1 = arg3;
        double var_82_1 = arg2;
    }
    
    int32_t ecx;
    void* edx;
    __trandisp2(ecx, edx, ebp, arg2, arg3);
    data_452878 = 1;
    return sub_415545(ebp, arg2);
}

int80_t sub_4154ce(int16_t arg1 @ x87control, long double arg2 @ st0)
{
    int32_t __saved_ebp;
    int32_t* ebp = &__saved_ebp;
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int16_t var_a8 = temp0;
    
    if (!data_43c0b8)
        double var_8a_1 = arg2;
    
    int32_t ecx;
    void* edx;
    __trandisp1(ecx, edx, ebp, arg2);
    data_452878 = 1;
    return sub_415545(ebp, arg2);
}

long double sub_415504(int16_t arg1 @ x87control, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{
    int32_t var_2ac = arg3;
    int32_t entry_ebx;
    long double st0 = __fload(arg2, entry_ebx);
    int32_t var_2ac_1 = arg5;
    long double result = __fload(arg4);
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int16_t var_a8 = temp0;
    int32_t __saved_ebp;
    int32_t ecx;
    void* edx;
    __trandisp2(ecx, edx, &__saved_ebp, result, st0);
    sub_41553e();
    return result;
}

int32_t sub_41553e()
{
    data_452878 = 0;
    void* ebp;
    long double x87_r0;
    /* tailcall */
    return sub_415545(ebp, x87_r0);
}

void sub_415545(void* arg1 @ ebp, long double arg2 @ st0)
{
    if (!data_43aa44)
    {
        data_452870 = arg2;
        bool c1_1 = /* bool c1_1 = unimplemented  {fst qword [&data_452870], st0} */;
        int16_t eax;
        eax = *(arg1 - 0x90);
        eax = eax;
        
        if (!eax)
        {
        label_41557f:
            bool c0;
            bool c2;
            bool c3;
            
            if (!(*(arg1 - 0xa4) & 0x20) && ((c0 ? 1 : 0) << 8 | (c1_1 ? 1 : 0) << 9
                | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe) & 0x20)
            {
                *(arg1 - 0x8e) = 8;
            label_41561b:
                void* ebx_2 = *(arg1 - 0x94) + 1;
                *(arg1 - 0x8a) = ebx_2;
                
                if (!data_452878)
                {
                    *(arg1 - 0x86) = *(arg1 + 8);
                    *(arg1 - 0x82) = *(arg1 + 0xc);
                    
                    if (*(ebx_2 + 0xc) != 1)
                    {
                        *(arg1 - 0x7e) = *(arg1 + 0x10);
                        *(arg1 - 0x7a) = *(arg1 + 0x14);
                    }
                }
                
                *(arg1 - 0x76) = arg2;
                void* eax_2;
                eax_2 = *(*(arg1 - 0x94) + 0xe);
                sub_4182b0(eax_2, arg1 - 0x8e, arg1 - 0xa4);
                *(arg1 - 0x76);
            }
        }
        else
        {
            if (eax == 0xff || eax == 0xfe)
            {
                eax = (*(data_452870 + 6)) & 0x7ff0;
                
                if (!eax)
                {
                    *(arg1 - 0x8e) = 4;
                    long double x87_r7_2 = arg2;
                    arg2 = 1536.0;
                    __fscale(x87_r7_2, arg2);
                    long double x87_r7_5 = fabsl(arg2);
                    long double temp1_1 = 2.2250738585072014e-308;
                    x87_r7_5 - temp1_1;
                    
                    if (TEST_BITB(
                            *((x87_r7_5 < temp1_1 ? 1 : 0) << 8
                                | (FCMP_UO(x87_r7_5, temp1_1) ? 1 : 0) << 0xa
                                | (x87_r7_5 == temp1_1 ? 1 : 0) << 0xe)[1], 
                            0))
                        arg2 = arg2 * 0.0;
                    
                    goto label_41561b;
                }
                
                if (eax != 0x7ff0)
                    goto label_41557f;
                
                *(arg1 - 0x8e) = 3;
                long double x87_r7_7 = arg2;
                arg2 = -1536.0;
                __fscale(x87_r7_7, arg2);
                long double x87_r7_10 = fabsl(arg2);
                long double temp2_1 = 1.7976931348623157e+308;
                x87_r7_10 - temp2_1;
                eax = (x87_r7_10 < temp2_1 ? 1 : 0) << 8
                    | (FCMP_UO(x87_r7_10, temp2_1) ? 1 : 0) << 0xa
                    | (x87_r7_10 == temp2_1 ? 1 : 0) << 0xe;
                
                if (!TEST_BITB(*eax[1], 6) && !TEST_BITB(*eax[1], 0))
                    arg2 = arg2 * inf.0;
                
                goto label_41561b;
            }
            
            eax = eax;
            
            if (eax)
            {
                *(arg1 - 0x8e) = eax;
                goto label_41561b;
            }
        }
    }
    
    int16_t x87control;
    int16_t x87status;
    x87control = __fldcw_memmem16(*(arg1 - 0xa4));
}

long double sub_415675(int16_t arg1 @ x87control, int32_t arg2, int32_t arg3)
{
    int32_t var_2ac = arg3;
    int32_t entry_ebx;
    long double result = __fload(arg2, entry_ebx);
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int16_t var_a8 = temp0;
    int32_t __saved_ebp;
    int32_t ecx;
    void* edx;
    __trandisp1(ecx, edx, &__saved_ebp, result);
    sub_41553e();
    return result;
}

long double __fload(double arg1, int16_t arg2) __pure
{
    int32_t ebx;
    ebx = arg2;
    
    if ((arg2 & 0x7ff0) != 0x7ff0)
        return arg1;
    
    ebx |= 0x7fff;
    int16_t var_6_1 = ebx;
    int32_t ebx_1 = arg1;
    int32_t var_a_1 = *arg1[4] << 0xb | ebx_1 >> 0xffffffeb;
    return ebx_1;
}

int32_t sub_4156e0(int16_t arg1 @ x87control, int32_t arg2, int32_t arg3)
{
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int32_t result = (~arg3 & sub_415750(temp0)) | (arg2 & arg3);
    int16_t x87control;
    int16_t x87status_1;
    x87control = __fldcw_memmem16(sub_415800(result));
    return result;
}

int32_t sub_415730(int32_t arg1, int32_t arg2)
{
    int16_t x87control;
    return sub_4156e0(x87control, arg1, arg2 & 0xfff7ffff);
}

int32_t sub_415750(int16_t arg1) __pure
{
    int32_t result = 0;
    
    if (arg1 & 1)
        result = 0x10;
    
    if (arg1 & 4)
        result |= 8;
    
    if (arg1 & 8)
        result |= 4;
    
    if (arg1 & 0x10)
        result |= 2;
    
    if (arg1 & 0x20)
        result |= 1;
    
    if (arg1 & 2)
        result |= 0x80000;
    
    int32_t ecx;
    ecx = arg1;
    int32_t ecx_1 = ecx & 0xc00;
    
    if (ecx_1 == 0x400)
        result |= 0x100;
    else if (ecx_1 == 0x800)
        result |= 0x200;
    else if (ecx_1 == 0xc00)
        result |= 0x300;
    
    int32_t ecx_2;
    ecx_2 = arg1;
    int32_t ecx_3 = ecx_2 & 0x300;
    
    if (!ecx_3)
        result |= 0x20000;
    else if (ecx_3 == 0x200)
        result |= 0x10000;
    
    if (!(*arg1[1] & 0x10))
        return result;
    
    return result | 0x40000;
}

int32_t sub_415800(int32_t arg1) __pure
{
    int16_t result = 0;
    
    if (arg1 & 0x10)
        result = 1;
    
    if (arg1 & 8)
        result |= 4;
    
    if (arg1 & 4)
        result |= 8;
    
    if (arg1 & 2)
        result |= 0x10;
    
    if (arg1 & 1)
        result |= 0x20;
    
    if (arg1 & 0x80000)
        result |= 2;
    
    int32_t ecx_1 = arg1 & 0x300;
    
    if (ecx_1 == 0x100)
        *result[1] |= 4;
    else if (ecx_1 == 0x200)
        *result[1] |= 8;
    else if (ecx_1 == 0x300)
        *result[1] |= 0xc;
    
    int32_t ecx_3 = arg1 & 0x30000;
    
    if (!ecx_3)
        *result[1] |= 3;
    else if (ecx_3 == 0x10000)
        *result[1] |= 2;
    
    if (arg1 & 0x40000)
        *result[1] |= 0x10;
    
    return result;
}

int32_t sub_415890()
{
    return sub_411170(2);
}

int32_t sub_4158a0(int32_t arg1, int32_t arg2)
{
    if (arg1 + 1 <= 0x100)
    {
        int32_t eax_1;
        eax_1 = (**&data_43bb90)[arg1];
        return eax_1 & arg2;
    }
    
    char edx = *arg1[1];
    int32_t ebx;
    ebx = edx;
    int32_t eax_4;
    
    if (!(*(&(**&data_43bb90)[ebx] + 1) & 0x80))
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
    
    if (sub_4183d0(1, &*var_6[2], eax_4, &var_6, 0, 0))
        return var_6 & arg2;
    
    return 0;
}

int32_t sub_415940(int32_t arg1)
{
    if (!data_43bdd8)
    {
        if (arg1 >= 0x41 && arg1 <= 0x5a)
            return arg1 + 0x20;
        
        return arg1;
    }
    
    wchar16 (* ecx)[0x21];
    
    if (arg1 < 0x100)
    {
        int32_t eax_2;
        
        if (data_43bb80 <= 1)
        {
            int32_t eax_3;
            eax_3 = (**&data_43bb90)[arg1];
            eax_2 = eax_3 & 1;
        }
        else
            eax_2 = sub_4158a0(arg1, 1);
        
        if (!eax_2)
            return arg1;
    }
    
    ecx = *arg1[1];
    int32_t edx;
    edx = ecx;
    uint8_t var_4;
    void* eax_6;
    
    if (!(*(&(**&data_43bb90)[edx] + 1) & 0x80))
    {
        eax_6 = 1;
        var_4 = arg1;
        char var_3_1 = 0;
    }
    else
    {
        eax_6 = 2;
        var_4 = ecx;
        char var_2_1 = 0;
        char var_3 = arg1;
    }
    
    char var_8;
    int32_t eax_7 = sub_418500(data_43bdd8, 0x100, &var_4, eax_6, &var_8, 3, 0);
    
    if (!eax_7)
        return arg1;
    
    if (eax_7 == 1)
    {
        int32_t eax_9;
        eax_9 = var_8;
        return eax_9;
    }
    
    char var_7;
    int32_t eax_10;
    eax_10 = var_7;
    int32_t ecx_1;
    ecx_1 = var_8;
    return eax_10 << 8 | ecx_1;
}

int32_t sub_415a30(int32_t arg1, int32_t arg2)
{
    int32_t eax_1;
    int32_t edx;
    edx = HIGHD(arg2);
    eax_1 = LOWD(arg2);
    int32_t eax_3 = (eax_1 + (edx & 0x1f)) >> 5;
    char eax_5;
    char edx_2;
    edx_2 = HIGHD(arg2);
    eax_5 = LOWD(arg2);
    
    if (*(arg1 + (eax_3 << 2))
            & ~(0xffffffff << (0x1f - (((((eax_5 ^ edx_2) - edx_2) & 0x1f) ^ edx_2) - edx_2))))
        return 0;
    
    int32_t i = eax_3 + 1;
    
    if (i < 3)
    {
        int32_t* eax_13 = arg1 + (i << 2);
        
        do
        {
            if (*eax_13)
                return 0;
            
            eax_13 = &eax_13[1];
            i += 1;
        } while (i < 3);
    }
    
    return 1;
}

int32_t sub_415aa0(int32_t arg1, int32_t arg2)
{
    int32_t eax_1;
    int32_t edx;
    edx = HIGHD(arg2);
    eax_1 = LOWD(arg2);
    int32_t eax_3 = (eax_1 + (edx & 0x1f)) >> 5;
    char eax_5;
    char edx_2;
    edx_2 = HIGHD(arg2);
    eax_5 = LOWD(arg2);
    int32_t* eax_11 = arg1 + (eax_3 << 2);
    int32_t i = sub_418760(*eax_11, 
        1 << (0x1f - (((((eax_5 ^ edx_2) - edx_2) & 0x1f) ^ edx_2) - edx_2)), eax_11);
    int32_t esi_1 = eax_3 - 1;
    
    if (eax_3 - 1 >= 0)
    {
        int32_t* edi_2 = arg1 + (esi_1 << 2);
        
        while (i)
        {
            int32_t* var_10_1 = edi_2;
            int32_t eax_12 = *edi_2;
            edi_2 -= 4;
            i = sub_418760(eax_12, 1, var_10_1);
            int32_t temp1_1 = esi_1;
            esi_1 -= 1;
            
            if (temp1_1 - 1 < 0)
                break;
        }
    }
    
    return i;
}

int32_t sub_415b10(int32_t arg1, int32_t arg2)
{
    int32_t result = 0;
    int32_t eax_1;
    int32_t edx;
    edx = HIGHD(arg2);
    eax_1 = LOWD(arg2);
    int32_t eax_3 = (eax_1 + (edx & 0x1f)) >> 5;
    char eax_5;
    char edx_2;
    edx_2 = HIGHD(arg2);
    eax_5 = LOWD(arg2);
    int32_t* var_8 = arg1 + (eax_3 << 2);
    int32_t* ecx;
    ecx = 0x1f - (((((eax_5 ^ edx_2) - edx_2) & 0x1f) ^ edx_2) - edx_2);
    int32_t* ecx_1;
    
    if (*var_8 & 1 << ecx)
    {
        int32_t eax_14;
        eax_14 = sub_415a30(arg1, arg2 + 1);
        
        if (!eax_14)
        {
            int32_t result_1;
            result_1 = sub_415aa0(arg1, arg2 - 1);
            result = result_1;
        }
    }
    
    ecx_1 = 0x1f - (((((eax_5 ^ edx_2) - edx_2) & 0x1f) ^ edx_2) - edx_2);
    *var_8 &= 0xffffffff << ecx_1;
    
    if (eax_3 + 1 < 3)
        __builtin_memset(arg1 + ((eax_3 + 1) << 2), 0, (3 - (eax_3 + 1)) << 2);
    
    return result;
}

int32_t* sub_415bc0(int32_t* arg1, int32_t* arg2)
{
    int32_t* result = arg2;
    int32_t* ecx = arg1;
    int32_t i_1 = 3;
    int32_t i;
    
    do
    {
        int32_t esi_1 = *result;
        result = &result[1];
        *ecx = esi_1;
        ecx = &ecx[1];
        i = i_1;
        i_1 -= 1;
    } while (i != 1);
    return result;
}

int32_t sub_415be0(int32_t* arg1)
{
    *arg1 = 0;
    arg1[1] = 0;
    arg1[2] = 0;
    return 0;
}

int32_t sub_415bf0(int32_t* arg1)
{
    int32_t i = 0;
    int32_t* ecx = arg1;
    
    do
    {
        if (*ecx)
            return 0;
        
        ecx = &ecx[1];
        i += 1;
    } while (i < 3);
    
    return 1;
}

int32_t sub_415c10(int32_t* arg1, int32_t arg2)
{
    int32_t eax_1;
    int32_t edx;
    edx = HIGHD(arg2);
    eax_1 = LOWD(arg2);
    int32_t eax_3 = (eax_1 + (edx & 0x1f)) >> 5;
    char eax_5;
    char edx_2;
    edx_2 = HIGHD(arg2);
    eax_5 = LOWD(arg2);
    char eax_10 = ((((eax_5 ^ edx_2) - edx_2) & 0x1f) ^ edx_2) - edx_2;
    int32_t ecx;
    ecx = eax_10;
    int32_t edx_3 = 0xffffffff << ecx;
    int32_t* ebx = arg1;
    ecx = 0x20;
    ecx = 0x20 - eax_10;
    int32_t i_1 = 3;
    int32_t edx_5 = 0;
    int32_t i;
    
    do
    {
        int32_t edi_1 = *ebx;
        ebx = &ebx[1];
        int32_t ecx_2;
        ecx_2 = eax_10;
        uint32_t edi_2 = edi_1 >> ecx_2;
        ecx_2 = ecx;
        ebx[-1] = edi_2;
        int32_t edi_3 = edi_2 | edx_5;
        edx_5 = (edi_1 & ~edx_3) << ecx_2;
        i = i_1;
        i_1 -= 1;
        ebx[-1] = edi_3;
    } while (i != 1);
    int32_t ecx_3 = 2;
    int32_t* esi_1 = &arg1[2];
    int32_t result = eax_3 << 2;
    void* edx_9 = arg1 - result + 8;
    int32_t temp1_1;
    
    do
    {
        if (eax_3 > ecx_3)
            *esi_1 = 0;
        else
        {
            result = *edx_9;
            *esi_1 = result;
        }
        
        edx_9 -= 4;
        esi_1 -= 4;
        temp1_1 = ecx_3;
        ecx_3 -= 1;
    } while (temp1_1 - 1 >= 0);
    return result;
}

int32_t sub_415cc0(int16_t* arg1, int32_t* arg2, int32_t* arg3)
{
    int16_t eax = arg1[5];
    int32_t ebx;
    ebx = eax;
    int32_t ebx_2 = (ebx & 0x7fff) - 0x3fff;
    int32_t esi;
    esi = eax;
    int32_t edx = *(arg1 + 2);
    int32_t var_18 = *(arg1 + 6);
    int32_t eax_2;
    eax_2 = *arg1;
    int32_t var_10 = eax_2 << 0x10;
    int32_t result;
    int32_t ebx_3;
    int32_t* edi;
    
    if (ebx_2 != 0xffffc001)
    {
        void var_c;
        sub_415bc0(&var_c, &var_18);
        edi = arg3;
        
        if (sub_415b10(&var_18, edi[2]))
            ebx_2 += 1;
        
        int32_t ebp_1 = edi[1];
        
        if (ebp_1 - edi[2] > ebx_2)
        {
            ebx_3 = 0;
            sub_415be0(&var_18);
            result = 2;
        }
        else if (ebx_2 <= ebp_1)
        {
            sub_415bc0(&var_18, &var_c);
            sub_415c10(&var_18, ebp_1 - ebx_2);
            sub_415b10(&var_18, edi[2]);
            ebx_3 = 0;
            sub_415c10(&var_18, edi[3] + 1);
            result = 2;
        }
        else if (*edi > ebx_2)
        {
            ebx_3 = ebx_2 + edi[5];
            int32_t ecx_3 = edi[3];
            var_18 &= 0x7fffffff;
            sub_415c10(&var_18, ecx_3);
            result = 0;
        }
        else
        {
            sub_415be0(&var_18);
            int32_t eax_10 = edi[3];
            var_18 |= 0x80000000;
            sub_415c10(&var_18, eax_10);
            ebx_3 = edi[5] + *edi;
            result = 1;
        }
    }
    else
    {
        ebx_3 = 0;
        
        if (!sub_415bf0(&var_18))
        {
            sub_415be0(&var_18);
            edi = arg3;
            result = 2;
        }
        else
        {
            result = 0;
            edi = arg3;
        }
    }
    
    int32_t ecx_6 = edi[4];
    int32_t ebx_7 = ebx_3 << (0x1f - edi[3]) | ((0 - 1) & 0x80000000) | var_18;
    
    if (ecx_6 == 0x40)
    {
        arg2[1] = ebx_7;
        *arg2 = edx;
        return result;
    }
    
    if (ecx_6 == 0x20)
        *arg2 = ebx_7;
    
    return result;
}

int32_t sub_415e90(int16_t* arg1, int32_t* arg2)
{
    return sub_415cc0(arg1, arg2, 0x43bda0);
}

int32_t sub_415eb0(int16_t* arg1, int32_t* arg2)
{
    return sub_415cc0(arg1, arg2, 0x43bdb8);
}

int32_t sub_415ed0(int32_t* arg1, char* arg2)
{
    char* var_10;
    int16_t var_c;
    sub_418970(&var_c, &var_10, arg2, 0, 0, 0, 0);
    return sub_415e90(&var_c, arg1);
}

int32_t sub_415f10(int32_t* arg1, char* arg2)
{
    char* var_10;
    int16_t var_c;
    sub_418970(&var_c, &var_10, arg2, 0, 0, 0, 0);
    return sub_415eb0(&var_c, arg1);
}

int32_t sub_415f50(char* arg1, int32_t arg2, void* arg3)
{
    int32_t ebx;
    int32_t var_4 = ebx;
    int32_t i_2 = arg2;
    void* edi = &arg1[1];
    char* ebp = *(arg3 + 0xc);
    char* esi = edi;
    *arg1 = 0x30;
    
    if (i_2 > 0)
    {
        int32_t i;
        
        do
        {
            ebx = *ebp;
            
            if (!ebx)
                *esi = 0x30;
            else
            {
                ebp = &ebp[1];
                *esi = ebx;
            }
            
            esi = &esi[1];
            i = i_2;
            i_2 -= 1;
        } while (i != 1);
    }
    
    *esi = 0;
    
    if (i_2 >= 0 && *ebp >= 0x35)
    {
        void* esi_1 = esi - 1;
        
        while (*esi_1 == 0x39)
        {
            *esi_1 = 0x30;
            esi_1 -= 1;
        }
        
        *esi_1 += 1;
    }
    
    if (*arg1 == 0x31)
    {
        *(arg3 + 4) += 1;
        return i_2;
    }
    
    int32_t i_1 = 0xffffffff;
    
    while (i_1)
    {
        bool cond:0_1 = 0 != *edi;
        edi += 1;
        i_1 -= 1;
        
        if (!cond:0_1)
            break;
    }
    
    int32_t ecx = ~i_1;
    int32_t esi_3;
    int32_t edi_3;
    edi_3 = __builtin_memcpy(arg1, edi - ecx, ecx >> 2 << 2);
    __builtin_memcpy(edi_3, esi_3, ecx & 3);
    return ecx;
}

int32_t sub_415fe0()
{
    uint32_t var_c;
    void arg_4;
    sub_416050(&var_c, &arg_4);
    int32_t var_8;
    uint16_t var_4;
    data_4528a8 = sub_4190c0(var_c, var_8, var_4, 0x11, 0, &data_452880);
    data_4528ac = 0x452884;
    int32_t edx_1 = data_452880;
    data_4528a0 = data_452882;
    data_4528a4 = edx_1;
    return &data_4528a0;
}

uint32_t sub_416050(uint32_t* arg1, int32_t* arg2)
{
    int16_t edx = *(arg2 + 6);
    int32_t esi;
    esi = edx;
    esi &= 0x7ff0;
    esi u>>= 4;
    int32_t ebx = 0x80000000;
    int32_t ecx_1 = arg2[1] & 0xfffff;
    int32_t ebp = *arg2;
    uint32_t eax = esi;
    
    if (!eax)
    {
        if (!ecx_1 && !ebp)
        {
            arg1[2] = 0;
            arg1[1] = 0;
            *arg1 = 0;
            return 0;
        }
        
        esi += 0x3c01;
        ebx = 0;
    }
    else if (eax == 0x7ff)
        esi = 0x7fff;
    else
        esi += 0x3c00;
    
    uint32_t result = ebp >> 0x15;
    int32_t ecx_4 = ecx_1 << 0xb | result | ebx;
    *arg1 = ebp << 0xb;
    arg1[1] = ecx_4;
    
    if (!(ecx_4 & 0x80000000))
    {
        int32_t ebx_4;
        
        do
        {
            esi -= 1;
            uint32_t eax_2 = *arg1;
            result = eax_2 * 2;
            ebx_4 = (eax_2 & 0x80000000) >> 0x1f | (arg1[1] * 2);
            *arg1 = result;
            arg1[1] = ebx_4;
        } while (!(ebx_4 & 0x80000000));
    }
    
    esi |= edx & 0x8000;
    arg1[2] = esi;
    return result;
}

int32_t __convention("regparm") sub_416110(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
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
                    goto label_416218;
                }
                case 2:
                {
                    goto label_416208;
                }
                case 3:
                {
                    goto label_4161f0;
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
                label_416218:
                    arg1 = *(esi_5 + 3);
                    *(edi_5 + 3) = arg1;
                    return arg3;
                    break;
                }
                case 2:
                {
                label_416208:
                    arg1 = *(esi_5 + 2);
                    *(edi_5 + 2) = arg1;
                    return arg3;
                    break;
                }
                case 3:
                {
                label_4161f0:
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
            label_41617c:
                arg1 = *esi_1;
                *edi_1 = arg1;
                return arg3;
                break;
            }
            case 2:
            {
            label_41616c:
                arg1 = *esi_1;
                *edi_1 = arg1;
                return arg3;
                break;
            }
            case 3:
            {
            label_416158:
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
            goto label_41617c;
        }
        case 2:
        {
            goto label_41616c;
        }
        case 3:
        {
            goto label_416158;
        }
    }
}

int32_t sub_416260(int32_t arg1, int32_t arg2, int32_t arg3)
{
    int32_t esi = 0;
    
    if (!data_43bdec)
    {
        HMODULE hModule = LoadLibraryA("user32.dll");
        int32_t eax_1;
        
        if (hModule)
        {
            eax_1 = GetProcAddress(hModule, "MessageBoxA");
            data_43bdec = eax_1;
        }
        
        if (!hModule || !eax_1)
            return 0;
        
        data_43bdf0 = GetProcAddress(hModule, "GetActiveWindow");
        data_43bdf4 = GetProcAddress(hModule, "GetLastActivePopup");
    }
    
    int32_t eax_4 = data_43bdf0;
    
    if (eax_4)
        esi = eax_4();
    
    if (esi && data_43bdf4)
        esi = data_43bdf4(esi);
    
    return data_43bdec(esi, arg1, arg2, arg3);
}

char* sub_416300(char* arg1, char* arg2, int32_t arg3)
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
                    goto label_41637b;
                
            label_4163e7:
                eax = 0;
                uint32_t i;
                
                do
                {
                    *edi_1 = 0;
                    edi_1 = &edi_1[4];
                    i = i_3;
                    i_3 -= 1;
                } while (i != 1);
            label_4163f1:
                ebx_1 &= 3;
                
                if (ebx_1)
                    goto label_41637b;
                
                return arg1;
            }
        } while (esi_1 & 3);
        
        ebx_1 = ecx;
        i_2 = ecx >> 2;
        
        if (i_2)
            goto label_41638f;
        
    label_416340:
        ebx_1 &= 3;
        
        if (ebx_1)
            goto label_416345;
    }
    else
    {
        i_2 = ecx >> 2;
        
        if (i_2)
        {
        label_41638f:
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
                    label_4163df:
                        edi_1 = &edi_1[4];
                        eax = 0;
                        i_3 = i_2 - 1;
                        
                        if (i_2 == 1)
                            goto label_4163f1;
                        
                        goto label_4163e7;
                    }
                    
                    if (!*edx_2[1])
                    {
                        *edi_1 = edx_2 & 0xff;
                        goto label_4163df;
                    }
                    
                    if (!(edx_2 & 0xff0000))
                    {
                        *edi_1 = edx_2 & 0xffff;
                        goto label_4163df;
                    }
                    
                    if (!(edx_2 & 0xff000000))
                    {
                        *edi_1 = edx_2;
                        goto label_4163df;
                    }
                }
                
                *edi_1 = edx_2;
                edi_1 = &edi_1[4];
                i_1 = i_2;
                i_2 -= 1;
            } while (i_1 != 1);
            goto label_416340;
        }
        
    label_416345:
        
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
                    
                label_41637b:
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

int32_t sub_416400()
{
    int32_t result = 0xffffffff;
    int32_t edi = 0;
    int32_t ebx = 0;
    
    for (int32_t* i = &data_454580; i < &data_454680; )
    {
        void* j_1 = *i;
        
        if (!j_1)
        {
            void* j = sub_4111a0(0x100);
            
            if (!j)
                return result;
            
            data_454680 += 0x20;
            (&data_454580)[edi] = j;
            
            if (j + 0x100 > j)
            {
                do
                {
                    *(j + 4) = 0;
                    j += 8;
                    *(j - 8) = 0xffffffff;
                    *(j - 3) = 0xa;
                } while ((&data_454580)[edi] + 0x100 > j);
            }
            
            return edi << 5;
        }
        
        for (void* eax_1 = j_1 + 0x100; j_1 < eax_1; j_1 += 8)
        {
            if (!(*(j_1 + 4) & 1))
            {
                *j_1 = 0xffffffff;
                result = ((j_1 - *i) >> 3) + ebx;
                break;
            }
        }
        
        if (result != 0xffffffff)
            return result;
        
        ebx += 0x20;
        i = &i[1];
        edi += 1;
    }
    
    return result;
}

int32_t sub_4164c0(int32_t arg1, HANDLE arg2)
{
    if (arg1 < data_454680
        && (*(((arg1 & 0xffffffe7) >> 3) + &data_454580))[(arg1 & 0x1f) * 2] == 0xffffffff)
    {
        HANDLE hHandle;
        
        if (data_43aab4 != 1)
            hHandle = arg2;
        else if (!arg1)
        {
            hHandle = arg2;
            SetStdHandle(STD_INPUT_HANDLE, hHandle);
        }
        else if (arg1 == 1)
        {
            hHandle = arg2;
            SetStdHandle(STD_OUTPUT_HANDLE, hHandle);
        }
        else if (arg1 == 2)
        {
            hHandle = arg2;
            SetStdHandle(STD_ERROR_HANDLE, hHandle);
        }
        else
            hHandle = arg2;
        
        (*(((arg1 & 0xffffffe7) >> 3) + &data_454580))[(arg1 & 0x1f) * 2] = hHandle;
        return 0;
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

int32_t sub_416570(int32_t arg1)
{
    if (arg1 < data_454680)
    {
        int32_t* eax_7 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ((arg1 & 0x1f) << 3);
        
        if (eax_7[1] & 1 && *eax_7 != 0xffffffff)
        {
            if (data_43aab4 == 1)
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
            
            (*(((arg1 & 0xffffffe7) >> 3) + &data_454580))[(arg1 & 0x1f) * 2] = 0xffffffff;
            return 0;
        }
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

int32_t sub_416600(int32_t arg1)
{
    if (arg1 < data_454680)
    {
        int32_t* eax_4 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ((arg1 & 0x1f) << 3);
        
        if (eax_4[1] & 1)
            return *eax_4;
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

enum WIN32_ERROR sub_416650(int32_t arg1)
{
    if (data_454680 > arg1)
    {
        int32_t eax_4;
        eax_4 = *(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ((arg1 & 0x1f) << 3) + 4);
        eax_4 &= 1;
        
        if (eax_4)
        {
            enum WIN32_ERROR result = NO_ERROR;
            
            if (!FlushFileBuffers(sub_416600(arg1)))
                result = GetLastError();
            
            if (!result)
                return result;
            
            data_43aa58 = 9;
            data_43aa5c = result;
            return 0xffffffff;
        }
    }
    
    data_43aa58 = 9;
    return ~NO_ERROR;
}

int32_t sub_4166c0(int32_t* arg1)
{
    data_43ba68 += 1;
    void* eax = sub_4111a0(0x1000);
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

uint32_t sub_416710(int32_t arg1, int32_t arg2, int32_t arg3, enum SET_FILE_POINTER_MOVE_METHOD arg4)
{
    if (data_454680 > arg1)
    {
        int32_t esi_1 = (arg1 & 0x1f) << 3;
        
        if (*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + esi_1 + 4) & 1)
        {
            int32_t distanceToMoveHigh = arg3;
            HANDLE hFile = sub_416600(arg1);
            
            if (hFile == 0xffffffff)
            {
                data_43aa58 = 9;
                return 0xffffffff;
            }
            
            uint32_t result = SetFilePointer(hFile, arg2, &distanceToMoveHigh, arg4);
            
            if (result == 0xffffffff)
            {
                enum WIN32_ERROR eax_7 = GetLastError();
                
                if (eax_7)
                {
                    sub_4144a0(eax_7);
                    return 0xffffffff;
                }
            }
            
            int32_t eax_8 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580);
            *(eax_8 + esi_1 + 4) &= 0xfd;
            return result;
        }
    }
    
    data_43aa58 = 9;
    data_43aa5c = 0;
    return 0xffffffff;
}

void* sub_416800(int32_t* arg1)
{
    int32_t eax = arg1[4];
    
    if (arg1[1] < 0)
        arg1[1] = 0;
    
    uint32_t eax_2;
    int32_t edx;
    eax_2 = sub_416710(eax, 0, 0, FILE_CURRENT);
    uint32_t var_10 = eax_2;
    
    if (edx <= 0 && (edx < 0 || var_10 < 0))
        return 0xffffffff;
    
    int32_t edx_2 = arg1[3];
    
    if (!(edx_2 & 0x108))
    {
        int32_t eax_4;
        int32_t edx_3;
        edx_3 = HIGHD(arg1[1]);
        eax_4 = LOWD(arg1[1]);
        return var_10 - eax_4;
    }
    
    int32_t ecx_2 = *arg1;
    char* eax_6 = arg1[2];
    void* result_1 = ecx_2 - eax_6;
    void* result = result_1;
    
    if (!(edx_2 & 3))
    {
        if (!(edx_2 & 0x80))
        {
            data_43aa58 = 0x16;
            return 0xffffffff;
        }
    }
    else if (*(*(((eax & 0xffffffe7) >> 3) + &data_454580) + ((eax & 0x1f) << 3) + 4) & 0x80
        && eax_6 < ecx_2)
    {
        do
        {
            if (*eax_6 == 0xa)
                result += 1;
            
            eax_6 = &eax_6[1];
        } while (eax_6 < ecx_2);
    }
    
    if (!edx && !var_10)
        return result;
    
    if (edx_2 & 1)
    {
        void* eax_8 = arg1[1];
        
        if (eax_8)
        {
            void* edi_7 = eax_8 + result_1;
            int32_t ebx_1 = (eax & 0x1f) << 3;
            
            if (*(*(((eax & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 4) & 0x80)
            {
                uint32_t eax_15;
                int32_t edx_7;
                eax_15 = sub_416710(eax, 0, 0, FILE_END);
                
                if (edx_7 != edx || eax_15 != var_10)
                {
                    sub_416710(eax, var_10, edx, FILE_BEGIN);
                    
                    if (edi_7 > 0x200)
                        edi_7 = arg1[6];
                    else
                    {
                        int16_t eax_17 = arg1[3];
                        
                        if (!(eax_17 & 8))
                            edi_7 = arg1[6];
                        else
                        {
                            edi_7 = 0x200;
                            
                            if (*eax_17[1] & 4)
                                edi_7 = arg1[6];
                        }
                    }
                    
                    if (*(*(((eax & 0xffffffe7) >> 3) + &data_454580) + ebx_1 + 4) & 4)
                        edi_7 += 1;
                }
                else
                {
                    void* i = arg1[2];
                    
                    for (void* ecx_4 = i + edi_7; ecx_4 > i; i += 1)
                    {
                        if (*i == 0xa)
                            edi_7 += 1;
                    }
                    
                    if (*(arg1 + 0xd) & 0x20)
                        edi_7 += 1;
                }
            }
            
            uint32_t temp4_1 = var_10;
            var_10 -= edi_7;
            int32_t var_c = edx - 0;
        }
        else
            result = nullptr;
    }
    
    return result + var_10;
}

int32_t sub_4169e0(PSTR arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{
    SECURITY_ATTRIBUTES securityAttributes;
    securityAttributes.lpSecurityDescriptor = 0;
    securityAttributes.nLength = 0xc;
    int32_t ebx;
    
    if (!(arg2 & 0x80))
    {
        securityAttributes.bInheritHandle = 1;
        ebx = 0;
    }
    else
    {
        ebx = 0x10;
        securityAttributes.bInheritHandle = 0;
    }
    
    if (!(arg2 & 0x8000) && (arg2 & 0x4000 || data_43bfb0 != 0x8000))
        ebx |= 0x80;
    
    int32_t eax_1 = arg2 & 3;
    uint32_t dwDesiredAccess;
    
    if (!eax_1)
        dwDesiredAccess = 0x80000000;
    else if (eax_1 == 1)
        dwDesiredAccess = 0x40000000;
    else
    {
        if (eax_1 != 2)
        {
            data_43aa58 = 0x16;
            data_43aa5c = 0;
            return 0xffffffff;
        }
        
        dwDesiredAccess = 0xc0000000;
    }
    
    if (arg3 - 0x10 <= 0x30)
    {
        int32_t ecx_1;
        ecx_1 = *(arg3 + &jump_table_416db0[1]);
        enum FILE_SHARE_MODE dwShareMode;
        
        switch (ecx_1)
        {
            case 0:
            {
                dwShareMode = FILE_SHARE_NONE;
            label_416ae4:
                int32_t eax_7 = arg2 & 0x700;
                enum FILE_CREATION_DISPOSITION dwCreationDisposition;
                
                if (eax_7 > 0x100)
                {
                    if (eax_7 > 0x300)
                    {
                        if (eax_7 > 0x500)
                        {
                            if (eax_7 == 0x600)
                                dwCreationDisposition = TRUNCATE_EXISTING;
                            else
                            {
                                if (eax_7 != 0x700)
                                    goto label_416b30;
                                
                                dwCreationDisposition = CREATE_NEW;
                            }
                        }
                        else if (eax_7 == 0x500)
                            dwCreationDisposition = CREATE_NEW;
                        else
                        {
                            if (eax_7 != 0x400)
                                goto label_416b30;
                            
                            dwCreationDisposition = OPEN_EXISTING;
                        }
                    }
                    else if (eax_7 == 0x300)
                        dwCreationDisposition = CREATE_ALWAYS;
                    else
                    {
                        if (eax_7 != 0x200)
                            goto label_416b30;
                        
                        dwCreationDisposition = TRUNCATE_EXISTING;
                    }
                }
                else if (eax_7 == 0x100)
                    dwCreationDisposition = OPEN_ALWAYS;
                else
                {
                    if (eax_7)
                    {
                    label_416b30:
                        data_43aa58 = 0x16;
                        data_43aa5c = 0;
                        return 0xffffffff;
                    }
                    
                    dwCreationDisposition = OPEN_EXISTING;
                }
                
                enum FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes = FILE_ATTRIBUTE_NORMAL;
                
                if (arg2 & 0x100 && !(0x80 & ~data_43aa60 & arg4))
                    dwFlagsAndAttributes = FILE_ATTRIBUTE_READONLY;
                
                if (arg2 & 0x40)
                {
                    dwDesiredAccess |= 0x10000;
                    dwFlagsAndAttributes |= FILE_FLAG_DELETE_ON_CLOSE;
                }
                
                if (arg2 & 0x1000)
                    dwFlagsAndAttributes |= FILE_ATTRIBUTE_TEMPORARY;
                
                if (arg2 & 0x20)
                    dwFlagsAndAttributes |= FILE_FLAG_SEQUENTIAL_SCAN;
                else if (arg2 & 0x10)
                    dwFlagsAndAttributes |= FILE_FLAG_RANDOM_ACCESS;
                
                int32_t result = sub_416400();
                
                if (result == 0xffffffff)
                {
                    data_43aa58 = 0x18;
                    data_43aa5c = 0;
                    return 0xffffffff;
                }
                
                HANDLE eax_15 = CreateFileA(arg1, dwDesiredAccess, dwShareMode, 
                    &securityAttributes, dwCreationDisposition, dwFlagsAndAttributes, nullptr);
                
                if (eax_15 == 0xffffffff)
                {
                    sub_4144a0(GetLastError());
                    return 0xffffffff;
                }
                
                enum FILE_TYPE eax_18 = GetFileType(eax_15);
                
                if (!eax_18)
                {
                    CloseHandle(eax_15);
                    sub_4144a0(GetLastError());
                    return 0xffffffff;
                }
                
                if (eax_18 == FILE_TYPE_CHAR)
                    ebx |= 0x40;
                else if (eax_18 == FILE_TYPE_PIPE)
                    ebx |= 8;
                
                ebx |= 1;
                sub_4164c0(result, eax_15);
                int32_t eax_26 = (result & 0x1f) << 3;
                int32_t var_14 = eax_26;
                *(*(((result & 0xffffffe7) >> 3) + &data_454580) + eax_26 + 4) = ebx;
                eax_26 = ebx;
                eax_26 &= 0x48;
                dwShareMode = eax_26;
                
                if (!eax_26 && ebx & 0x80 && arg2 & 2)
                {
                    int32_t eax_27 = sub_4143e0(result, 0xffffffff, FILE_END);
                    
                    if (eax_27 != 0xffffffff)
                    {
                        char var_19 = 0;
                        
                        if (!sub_413ab0(result, &var_19, 1) && var_19 == 0x1a
                            && sub_419460(result, eax_27) == 0xffffffff)
                        {
                            sub_413750(result);
                            return 0xffffffff;
                        }
                        
                        if (sub_4143e0(result, 0, FILE_BEGIN) == 0xffffffff)
                        {
                            sub_413750(result);
                            return 0xffffffff;
                        }
                    }
                    else if (data_43aa5c != 0x83)
                    {
                        sub_413750(result);
                        return 0xffffffff;
                    }
                }
                
                if (!dwShareMode && arg2 & 8)
                {
                    int32_t eax_34 = *(((result & 0xffffffe7) >> 3) + &data_454580);
                    *(eax_34 + var_14 + 4) |= 0x20;
                }
                
                return result;
                break;
            }
            case 1:
            {
                dwShareMode = FILE_SHARE_READ;
                goto label_416ae4;
            }
            case 2:
            {
                dwShareMode = FILE_SHARE_WRITE;
                goto label_416ae4;
            }
            case 3:
            {
                dwShareMode = FILE_SHARE_READ | FILE_SHARE_WRITE;
                goto label_416ae4;
            }
        }
    }
    
    data_43aa58 = 0x16;
    data_43aa5c = 0;
    return 0xffffffff;
}

int32_t sub_416e00(int32_t arg1)
{
    if (arg1 >= data_454680)
        return 0;
    
    int32_t eax_4;
    eax_4 = *(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ((arg1 & 0x1f) << 3) + 4);
    return eax_4 & 0x40;
}

int32_t sub_416e30()
{
    int32_t result = 0;
    int32_t i = 3;
    
    if (data_454570 > 3)
    {
        int32_t ebx_1 = 0xc;
        
        do
        {
            int32_t* eax_2 = *(data_453564 + ebx_1);
            
            if (eax_2)
            {
                if (eax_2[3] & 0x83 && sub_4112a0(eax_2) != 0xffffffff)
                    result += 1;
                
                if (ebx_1 >= 0x50)
                {
                    sub_411250(*(data_453564 + ebx_1));
                    *(data_453564 + ebx_1) = 0;
                }
            }
            
            ebx_1 += 4;
            i += 1;
        } while (i < data_454570);
    }
    
    return result;
}

int32_t sub_416eb0(char* arg1, wchar16 arg2)
{
    if (!arg1)
        return 0;
    
    if (!data_43bdd8)
    {
        if (arg2 <= 0xff)
        {
            *arg1 = arg2;
            return 1;
        }
        
        data_43aa58 = 0x2a;
        return 0xffffffff;
    }
    
    int32_t cbMultiByte = data_43bb80;
    BOOL usedDefaultChar = 0;
    int32_t result = WideCharToMultiByte(data_43bde8, 0x220, &arg2, 1, arg1, cbMultiByte, nullptr, 
        &usedDefaultChar);
    
    if (result && !usedDefaultChar)
        return result;
    
    data_43aa58 = 0x2a;
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

int32_t __fastcall __trandisp1(int32_t arg1, void* arg2, void* arg3 @ ebp, long double arg4 @ st0)
{
    int16_t ebx;
    
    if (*(arg2 + 0xe) != 5)
        ebx = 0x133f;
    else
    {
        *ebx[1] = *(*(arg3 - 0xa4))[1] | 2;
        *ebx[1] &= 0xfe;
        ebx = 0x3f;
    }
    
    *(arg3 - 0xa2) = ebx;
    int16_t x87control;
    int16_t x87status;
    x87control = __fldcw_memmem16(*(arg3 - 0xa2));
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    c0 = __fxam(arg4);
    *(arg3 - 0x94) = arg2;
    *(arg3 - 0xa0) =
        (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    *(arg3 - 0x90) = 0;
    arg1 = *(arg3 - 0x9f);
    arg1 <<= 1;
    arg1 s>>= 1;
    arg1 = ROLB(arg1, 1);
    int32_t eax;
    eax = arg1;
    eax &= 0xf;
    eax = *(&data_43be2d + eax);
    /* jump -> *(arg2 + eax + 0x10) */
}

int32_t __fastcall __trandisp2(int32_t arg1, void* arg2, void* arg3 @ ebp, long double arg4 @ st0, long double arg5 @ st1)
{
    int16_t ebx;
    
    if (*(arg2 + 0xe) != 5)
        ebx = 0x133f;
    else
    {
        *ebx[1] = *(*(arg3 - 0xa4))[1] | 2;
        *ebx[1] &= 0xfe;
        ebx = 0x3f;
    }
    
    *(arg3 - 0xa2) = ebx;
    int16_t x87control;
    int16_t x87status;
    x87control = __fldcw_memmem16(*(arg3 - 0xa2));
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    c0 = __fxam(arg4);
    *(arg3 - 0x94) = arg2;
    *(arg3 - 0xa0) =
        (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    *(arg3 - 0x90) = 0;
    arg1 = *(arg3 - 0x9f);
    bool c0_1;
    bool c1_1;
    bool c2_1;
    bool c3_1;
    c0_1 = __fxam(arg5);
    *(arg3 - 0xa0) =
        (c0_1 ? 1 : 0) << 8 | (c1_1 ? 1 : 0) << 9 | (c2_1 ? 1 : 0) << 0xa | (c3_1 ? 1 : 0) << 0xe;
    *arg1[1] = *(arg3 - 0x9f);
    *arg1[1] <<= 1;
    *arg1[1] s>>= 1;
    *arg1[1] = ROLB(*arg1[1], 1);
    int32_t eax;
    eax = *arg1[1];
    eax &= 0xf;
    eax = *(&data_43be2d + eax);
    *eax[1] = eax;
    arg1 <<= 1;
    arg1 s>>= 1;
    arg1 = ROLB(arg1, 1);
    eax = arg1;
    eax &= 0xf;
    eax = *(&data_43be2d + eax);
    *eax[1] <<= 1;
    *eax[1] <<= 1;
    eax |= *eax[1];
    /* jump -> *(arg2 + eax + 0x10) */
}

int32_t sub_417178() __pure
{
    return;
}

long double sub_417186() __pure
{
    return 0;
}

int32_t sub_41718b() __pure
{
    return;
}

long double sub_417192(void* arg1 @ ebp, int80_t arg2 @ st0)
{
    *(arg1 - 0x9e) = arg2;
    long double result = *(arg1 - 0x9e);
    
    if (!(*(arg1 - 0x97) & 0x40))
    {
        *(arg1 - 0x90) = 1;
        return result + data_43be24;
    }
    
    *(arg1 - 0x90) = 7;
    return result;
}

long double sub_4171bd(void* arg1 @ ebp, long double arg2 @ st0, int80_t arg3 @ st1)
{
    *(arg1 - 0x9e) = arg3;
    long double x87_r0_1 = *(arg1 - 0x9e);
    
    if (!(*(arg1 - 0x97) & 0x40))
        *(arg1 - 0x90) = 1;
    else
        *(arg1 - 0x90) = 7;
    
    return arg2 + x87_r0_1;
}

long double __nan2(void* arg1 @ ebp, int80_t arg2 @ st0, long double arg3 @ st1)
{
    *(arg1 - 0x9e) = arg2;
    long double x87_r0 = *(arg1 - 0x9e);
    
    if (*(arg1 - 0x97) & 0x40)
    {
        long double x87_r0_1 = arg3;
        arg3 = x87_r0;
        *(arg1 - 0x9e) = x87_r0_1;
        x87_r0 = *(arg1 - 0x9e);
    }
    
    if (!(*(arg1 - 0x97) & 0x40) || !(*(arg1 - 0x97) & 0x40))
        *(arg1 - 0x90) = 1;
    else
        *(arg1 - 0x90) = 7;
    
    return arg3 + x87_r0;
}

int32_t __rtindfpop(void* arg1 @ ebp)
{
    data_43be10;
    
    if (*(arg1 - 0x90) > 0)
        return;
    
    /* tailcall */
    return sub_417239(arg1);
}

int32_t sub_417239(void* arg1 @ ebp)
{
    *(arg1 - 0x90) = 1;
}

void __fastcall sub_417243(char arg1, long double arg2 @ st0) __pure
{
    return;
}

int32_t sub_417250(double arg1, int32_t arg2, int32_t arg3, int32_t* arg4)
{
    double var_8 = fabsl(arg1);
    
    if (arg3 == 0x7ff00000 && !arg2)
    {
        long double x87_r7_1 = 1;
        long double temp0 = var_8;
        x87_r7_1 - temp0;
        
        if (*((x87_r7_1 < temp0 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_1, temp0) ? 1 : 0) << 0xa
            | (x87_r7_1 == temp0 ? 1 : 0) << 0xe)[1] & 1)
        {
            arg4[1] = data_43bfbc;
            *arg4 = data_43bfb8;
            return 0;
        }
        
        long double x87_r7_2 = 1;
        long double temp2 = var_8;
        x87_r7_2 - temp2;
        
        if (*((x87_r7_2 < temp2 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_2, temp2) ? 1 : 0) << 0xa
            | (x87_r7_2 == temp2 ? 1 : 0) << 0xe)[1] & 0x41)
        {
            arg4[1] = data_43bfc4;
            *arg4 = data_43bfc0;
            return 1;
        }
        
        *arg4 = 0;
        arg4[1] = 0;
        return 0;
    }
    
    if (arg3 == 0xfff00000 && !arg2)
    {
        long double x87_r7_3 = 1;
        long double temp1 = var_8;
        x87_r7_3 - temp1;
        
        if (*((x87_r7_3 < temp1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_3, temp1) ? 1 : 0) << 0xa
            | (x87_r7_3 == temp1 ? 1 : 0) << 0xe)[1] & 1)
        {
            *arg4 = 0;
            arg4[1] = 0;
            return 0;
        }
        
        long double x87_r7_4 = 1;
        long double temp4 = var_8;
        x87_r7_4 - temp4;
        
        if (*((x87_r7_4 < temp4 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_4, temp4) ? 1 : 0) << 0xa
            | (x87_r7_4 == temp4 ? 1 : 0) << 0xe)[1] & 0x41)
        {
            arg4[1] = data_43bfc4;
            *arg4 = data_43bfc0;
            return 1;
        }
        
        arg4[1] = data_43bfbc;
        *arg4 = data_43bfb8;
        return 0;
    }
    
    if (*arg1[4] == 0x7ff00000 && !arg1)
    {
        long double x87_r7_5 = 0;
        long double temp3 = arg2;
        x87_r7_5 - temp3;
        
        if (*((x87_r7_5 < temp3 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_5, temp3) ? 1 : 0) << 0xa
            | (x87_r7_5 == temp3 ? 1 : 0) << 0xe)[1] & 1)
        {
            arg4[1] = data_43bfbc;
            *arg4 = data_43bfb8;
            return 0;
        }
        
        long double x87_r7_6 = 0;
        long double temp6 = arg2;
        x87_r7_6 - temp6;
        *arg4 = 0;
        
        if (*((x87_r7_6 < temp6 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_6, temp6) ? 1 : 0) << 0xa
            | (x87_r7_6 == temp6 ? 1 : 0) << 0xe)[1] & 0x41)
        {
            arg4[1] = 0x3ff00000;
            return 0;
        }
        
        arg4[1] = 0;
        return 0;
    }
    
    if (*arg1[4] == 0xfff00000 && !arg1)
    {
        long double x87_r7_7 = 0;
        long double temp5_1 = arg2;
        x87_r7_7 - temp5_1;
        int32_t ecx_8 = sub_4174a0(arg2, arg3);
        int32_t eax_16;
        eax_16 = (x87_r7_7 < temp5_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_7, temp5_1) ? 1 : 0) << 0xa
            | (x87_r7_7 == temp5_1 ? 1 : 0) << 0xe;
        
        if (*eax_16[1] & 1)
        {
            if (ecx_8 != 1)
            {
                *var_8[4] = data_43bfbc;
                var_8 = data_43bfb8;
            }
            else
                var_8 = -(*data_43bfb8);
            
            arg4[1] = *var_8[4];
            *arg4 = var_8;
            return 0;
        }
        
        long double x87_r7_10 = 0;
        long double temp7_1 = arg2;
        x87_r7_10 - temp7_1;
        eax_16 = (x87_r7_10 < temp7_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_10, temp7_1) ? 1 : 0) << 0xa
            | (x87_r7_10 == temp7_1 ? 1 : 0) << 0xe;
        
        if (!(*eax_16[1] & 0x41))
        {
            if (ecx_8 != 1)
            {
                var_8 = 0;
                *var_8[4] = 0;
            }
            else
            {
                *var_8[4] = data_43bfdc;
                var_8 = data_43bfd8;
            }
            
            arg4[1] = *var_8[4];
            *arg4 = var_8;
            return 0;
        }
        
        *arg4 = 0;
        arg4[1] = 0x3ff00000;
    }
    
    return 0;
}

int32_t sub_4174a0(int32_t arg1, int32_t arg2)
{
    int16_t var_8;
    
    if (sub_419790(arg1, arg2, var_8) & 0x90)
        return 0;
    
    int32_t var_c_1 = arg2;
    long double st0 = sub_419770(arg1);
    long double temp0 = arg1;
    st0 - temp0;
    int32_t eax_2;
    eax_2 = (st0 < temp0 ? 1 : 0) << 8 | (FCMP_UO(st0, temp0) ? 1 : 0) << 0xa
        | (st0 == temp0 ? 1 : 0) << 0xe;
    
    if (!(*eax_2[1] & 0x40))
        return 0;
    
    double var_8_1 = arg1 * 0.5;
    int32_t var_c_2 = *var_8_1[4];
    long double st0_1 = sub_419770(var_8_1);
    long double temp1 = var_8_1;
    st0_1 - temp1;
    int32_t eax_3;
    eax_3 = (st0_1 < temp1 ? 1 : 0) << 8 | (FCMP_UO(st0_1, temp1) ? 1 : 0) << 0xa
        | (st0_1 == temp1 ? 1 : 0) << 0xe;
    
    if (!(*eax_3[1] & 0x40))
        return 1;
    
    return 2;
}

long double sub_417520(int16_t arg1 @ x87control, long double arg2, long double arg3)
{
    long double x87_r7 = arg3;
    long double x87_r6 = arg2;
    
    while (true)
    {
        int32_t eax_1 = *arg2[4];
        
        if (eax_1 * 2 < eax_1)
        {
            int32_t eax_3 = (eax_1 * 2) ^ 0xe000000;
            
            if (eax_3 & 0xe000000)
                return x87_r7 / x87_r6;
            
            if (!*((eax_3 >> 0x1c) + 0x43be40))
                return x87_r7 / x87_r6;
            
            int32_t eax_6 = *arg2[8] & 0x7fff;
            
            if (!eax_6 || eax_6 == 0x7fff)
                return x87_r7 / x87_r6;
            
            int16_t x87status_1;
            int16_t temp0_1;
            temp0_1 = __fnstcw_memmem16(arg1);
            int16_t x87control;
            int16_t x87status_2;
            x87control = __fldcw_memmem16((temp0_1 | 0x33f) & 0xf3ff);
            
            if ((*arg3[8] & 0x7fff) == 1)
            {
                long double x87_r7_6 = x87_r6 * data_43be54;
                long double x87_r7_7 = x87_r7 * data_43be54;
                int16_t x87control_2;
                int16_t x87status_4;
                x87control_2 = __fldcw_memmem16(temp0_1);
                return x87_r7_7 / x87_r7_6;
            }
            
            long double x87_r7_3 = x87_r6 * data_43be50;
            long double x87_r7_4 = x87_r7 * data_43be50;
            int16_t x87control_1;
            int16_t x87status_3;
            x87control_1 = __fldcw_memmem16(temp0_1);
            return x87_r7_4 / x87_r7_3;
        }
        
        if (!(arg2 | *arg2[4]) || *arg2[8] & 0x7fff)
            return x87_r7 / x87_r6;
        
        int16_t x87status_5;
        int16_t temp0_6;
        temp0_6 = __fnstcw_memmem16(arg1);
        int16_t x87control_3;
        int16_t x87status_6;
        x87control_3 = __fldcw_memmem16((temp0_6 | 0x33f) & 0xf3ff);
        int32_t eax_20 = *arg3[8] & 0x7fff;
        int16_t x87control_4;
        int16_t x87status_8;
        
        if (!eax_20)
        {
            int32_t eax_23 = *arg3[4];
            
            if (eax_23 * 2 < eax_23)
            {
                x87control_4 = __fldcw_memmem16(temp0_6);
                return x87_r7 / x87_r6;
            }
        }
        else
        {
            if (eax_20 == 0x7fff)
            {
                x87control_4 = __fldcw_memmem16(temp0_6);
                return x87_r7 / x87_r6;
            }
            
            int32_t eax_21 = *arg3[4];
            
            if (eax_21 * 2 >= eax_21)
            {
                x87control_4 = __fldcw_memmem16(temp0_6);
                return x87_r7 / x87_r6;
            }
        }
        arg2 = x87_r7 * data_43be58;
        x87_r6 = x87_r7;
        x87_r7 = arg3;
        int16_t x87status_7;
        arg1 = __fldcw_memmem16(temp0_6);
    }
}

int32_t __convention("regparm") sub_417637(int32_t arg1)
{
    /* jump -> (&data_43be9e)[arg1 & 0x3f] */
}

void sub_41764a() __noreturn
{
    trap(6);
}

void sub_417655() __noreturn
{
    trap(6);
}

int32_t sub_417660(long double arg1 @ st0) __pure
{
    return;
}

long double sub_417666(long double arg1 @ st0) __pure
{
    return arg1 / arg1;
}

int32_t sub_41766c(long double arg1 @ st0) __pure
{
    return;
}

int80_t sub_417672(long double arg1 @ st0, int80_t arg2 @ st1)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_41768e() __noreturn
{
    trap(6);
}

void sub_4176a9() __noreturn
{
    trap(6);
}

int80_t sub_4176ca(int80_t arg1 @ st0, long double arg2 @ st1)
{
    __return_addr = arg1;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg2);
}

long double sub_4176da(long double arg1 @ st0, int80_t arg2 @ st1)
{
    __return_addr = arg2;
    int16_t x87control;
    sub_417520(x87control, __return_addr, arg1);
    return arg1;
}

int80_t sub_4176ee(long double arg1 @ st0, int80_t arg2 @ st1)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

int80_t sub_4176fe(long double arg1 @ st0, int80_t arg2 @ st2)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_41771e() __noreturn
{
    trap(6);
}

void sub_41773d() __noreturn
{
    trap(6);
}

int80_t sub_417762(int80_t arg1 @ st0, int80_t arg2 @ st1, long double arg3 @ st2)
{
    __return_addr = arg1;
    int16_t x87control;
    sub_417520(x87control, __return_addr, arg3);
    return arg2;
}

long double sub_417776(long double arg1 @ st0, int80_t arg2 @ st2)
{
    __return_addr = arg2;
    int16_t x87control;
    sub_417520(x87control, __return_addr, arg1);
    return arg1;
}

int80_t sub_41778e(long double arg1 @ st0, int80_t arg2 @ st1, int80_t arg3 @ st2)
{
    __return_addr = arg3;
    int16_t x87control;
    sub_417520(x87control, __return_addr, arg1);
    return arg2;
}

int80_t sub_4177a2(long double arg1 @ st0, int80_t arg2 @ st3)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_4177c2() __noreturn
{
    trap(6);
}

void sub_4177e1() __noreturn
{
    trap(6);
}

int32_t sub_417806(int80_t arg1 @ st0, long double arg2 @ st3)
{
    __return_addr = arg1;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg2);
    return result;
}

int32_t sub_41781a(long double arg1 @ st0, int80_t arg2 @ st3)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int32_t sub_417832(long double arg1 @ st0, int80_t arg2 @ st3)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int80_t sub_417846(long double arg1 @ st0, int80_t arg2 @ st4)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_417866() __noreturn
{
    trap(6);
}

void sub_417885() __noreturn
{
    trap(6);
}

int32_t sub_4178aa(int80_t arg1 @ st0, long double arg2 @ st4)
{
    __return_addr = arg1;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg2);
    return result;
}

int32_t sub_4178be(long double arg1 @ st0, int80_t arg2 @ st4)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int32_t sub_4178d6(long double arg1 @ st0, int80_t arg2 @ st4)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int80_t sub_4178ea(long double arg1 @ st0, int80_t arg2 @ st5)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_41790a() __noreturn
{
    trap(6);
}

void sub_417929() __noreturn
{
    trap(6);
}

int32_t sub_41794e(int80_t arg1 @ st0, long double arg2 @ st5)
{
    __return_addr = arg1;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg2);
    return result;
}

int32_t sub_417962(long double arg1 @ st0, int80_t arg2 @ st5)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int32_t sub_41797a(long double arg1 @ st0, int80_t arg2 @ st5)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int80_t sub_41798e(long double arg1 @ st0, int80_t arg2 @ st6)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_4179ae() __noreturn
{
    trap(6);
}

void sub_4179cd() __noreturn
{
    trap(6);
}

int32_t sub_4179f2(int80_t arg1 @ st0, long double arg2 @ st6)
{
    __return_addr = arg1;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg2);
    return result;
}

int32_t sub_417a06(long double arg1 @ st0, int80_t arg2 @ st6)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int32_t sub_417a1e(long double arg1 @ st0, int80_t arg2 @ st6)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int80_t sub_417a32(long double arg1 @ st0, int80_t arg2 @ st7)
{
    __return_addr = arg2;
    int16_t x87control;
    return sub_417520(x87control, __return_addr, arg1);
}

void sub_417a52() __noreturn
{
    trap(6);
}

void sub_417a71() __noreturn
{
    trap(6);
}

int32_t sub_417a96(int80_t arg1 @ st0, long double arg2 @ st7)
{
    __return_addr = arg1;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg2);
    return result;
}

int32_t sub_417aaa(long double arg1 @ st0, int80_t arg2 @ st7)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int32_t sub_417ac2(long double arg1 @ st0, int80_t arg2 @ st7)
{
    __return_addr = arg2;
    int32_t result;
    int16_t x87control;
    int80_t st0;
    st0 = sub_417520(x87control, __return_addr, arg1);
    return result;
}

int80_t sub_417ad6(long double arg1 @ st0, long double arg2 @ st1)
{
    int16_t x87control;
    return sub_417520(x87control, arg1, arg2);
}

int80_t sub_417ae9(long double arg1 @ st0, long double arg2 @ st1)
{
    int16_t x87control;
    return sub_417520(x87control, arg2, arg1);
}

long double __stdcall sub_417afc(long double arg1 @ st0, float arg2)
{
    if ((arg2 & 0x7f800000) == 0x7f800000)
        return arg1 / arg2;
    
    int32_t eax_1;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax_1 = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ad6(arg2, arg1);
}

long double __stdcall sub_417b48(long double arg1 @ st0, double arg2, int32_t arg3)
{
    if ((arg3 & 0x7ff00000) == 0x7ff00000)
        return arg1 / arg2;
    
    int32_t eax_1;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax_1 = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ad6(arg2, arg1);
}

int80_t __stdcall sub_417b94(long double arg1 @ st0, int16_t arg2)
{
    int32_t eax;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ad6(arg2, arg1);
}

int80_t __stdcall sub_417bc8(long double arg1 @ st0, int32_t arg2)
{
    int32_t eax;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ad6(arg2, arg1);
}

long double __stdcall sub_417bfc(long double arg1 @ st0, float arg2)
{
    if ((arg2 & 0x7f800000) == 0x7f800000)
        return arg2 / arg1;
    
    int32_t eax_1;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax_1 = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ae9(arg2, arg1);
}

long double __stdcall sub_417c48(long double arg1 @ st0, double arg2, int32_t arg3)
{
    if ((arg3 & 0x7ff00000) == 0x7ff00000)
        return arg2 / arg1;
    
    int32_t eax_1;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax_1 = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ae9(arg2, arg1);
}

int80_t __stdcall sub_417c94(long double arg1 @ st0, int16_t arg2)
{
    int32_t eax;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ae9(arg2, arg1);
}

int80_t __stdcall sub_417cc8(long double arg1 @ st0, int32_t arg2)
{
    int32_t eax;
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    eax = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
    return sub_417ae9(arg2, arg1);
}

int80_t sub_417cfc(long double arg1 @ st0, long double arg2 @ st1)
{
    int16_t x87control;
    return sub_417520(x87control, arg1, arg2);
}

int80_t sub_417d11(long double arg1 @ st0, long double arg2 @ st1)
{
    int16_t x87control;
    return sub_417520(x87control, arg2, arg1);
}

long double __convention("regparm") sub_417d26(int32_t arg1, int32_t arg2, int16_t arg3 @ x87control, long double arg4, int32_t arg5, long double arg6, long double arg7)
{
    int32_t eax_1 = arg5 ^ 0x700;
    int16_t arg_28;
    long double result;
    long double result_3;
    bool c0;
    bool c1;
    bool c2_3;
    bool c3;
    
    if (eax_1 & 0x700 || !*((eax_1 >> 0xb & 0xf) + 0x43be5c) || (arg5 & 0x7fff0000) == 0x7fff0000)
    {
    label_417ec0:
        result_3 = arg4;
        long double result_1;
        uint8_t temp0_7;
        result_1 = __fprem(arg7, result_3);
        result = result_1;
        
        if (!c2_3)
        {
            c0 = temp0_7 & 4;
            c1 = temp0_7 & 1;
            c3 = temp0_7 & 2;
        }
    }
    else
    {
        int32_t eax_7 = *arg7[6] & 0x7fff0000;
        
        if (!eax_7 || eax_7 == 0x7fff0000)
            goto label_417ec0;
        
        int32_t eax_8 = *arg7[4];
        
        if (eax_8 != -(eax_8))
            goto label_417ec0;
        
        int32_t eax_10 = *arg4[4];
        
        if (eax_10 != -(eax_10))
            goto label_417ec0;
        
        if ((*arg7[8] & 0x7fff) <= (arg5 & 0x7fff) + 0x3f)
        {
            while ((*arg7[8] & 0x7fff) - ((arg5 & 0x7fff) + 0xa) >= 0)
            {
                int32_t eax_18 = arg5;
                int32_t ebx_8 = *arg7[8] & 0x7fff;
                arg5 = (ebx_8 - (((ebx_8 - eax_18) & 7) | 4)) | (eax_18 & 0x8000);
                arg5 = eax_18;
                long double st0_1;
                uint8_t temp0_2;
                bool c2_1;
                st0_1 = __fprem(arg7, arg4);
                
                if (!c2_1)
                {
                    c0 = temp0_2 & 4;
                    c1 = temp0_2 & 1;
                    c3 = temp0_2 & 2;
                }
                
                arg7 = st0_1;
            }
            
            goto label_417ec0;
        }
        
        if (!(arg2 & 2))
            arg6 = arg4;
        
        int16_t x87status_1;
        char temp0_3;
        temp0_3 = __fnstcw_memmem16(arg3);
        arg_28 = temp0_3;
        int16_t x87control;
        int16_t x87status_2;
        x87control = __fldcw_memmem16(arg_28 | 0x33f);
        int32_t i_1 = ((((*arg7[8] & 0x7fff) - (arg5 & 0x7fff)) & 0x3f) | 0x20) + 1;
        arg5 = (*arg7[8] & 0x7fff) | (arg5 & 0x8000);
        long double x87_r7_5 = fabsl(arg4);
        long double result_2 = fabsl(arg7);
        int32_t i;
        
        do
        {
            result_2 - x87_r7_5;
            c0 = result_2 < x87_r7_5;
            c1 = false;
            c3 = result_2 == x87_r7_5;
            int32_t eax_24;
            eax_24 = (c0 ? 1 : 0) << 8 | (FCMP_UO(result_2, x87_r7_5) ? 1 : 0) << 0xa
                | (c3 ? 1 : 0) << 0xe | 0x3000;
            
            if (!(eax_24 & 0x100))
                result_2 = result_2 - x87_r7_5;
            
            x87_r7_5 = x87_r7_5 * data_43be8c;
            i = i_1;
            i_1 -= 1;
        } while (i != 1);
        long double result_4;
        uint8_t temp0_6;
        result_4 = __fprem(data_43be94, arg6);
        
        if (!c2_3)
        {
            c0 = temp0_6 & 4;
            c1 = temp0_6 & 1;
            c3 = temp0_6 & 2;
        }
        
        result_3 = result_4;
        result = result_2;
        int16_t x87status_3;
        arg3 = __fldcw_memmem16(arg_28);
        
        if (*arg7[8] & 0x8000)
        {
            result = -(result);
            c1 = false;
        }
    }
    
    if (arg2 & 3)
    {
        if (arg2 & 1)
        {
            int16_t x87status_4;
            char temp0_8;
            temp0_8 = __fnstcw_memmem16(arg3);
            arg_28 = temp0_8;
            int16_t x87control_1;
            int16_t x87status_5;
            x87control_1 = __fldcw_memmem16(arg_28 | 0x300);
            data_43be7c;
            int16_t x87control_2;
            int16_t x87status_6;
            x87control_2 = __fldcw_memmem16(arg_28);
        }
        
        result = result_3;
        int16_t x87status_7;
        char temp0_11;
        temp0_11 = __fnstenv_memmem28();
        uint224_t var_28_1;
        *var_28_1[4] = *temp0_11[4] & 0xbcff;
        *var_28_1[4] |= ((c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2_3 ? 1 : 0) << 0xa
            | (c3 ? 1 : 0) << 0xe | 0x3000) & 0x4300;
        __fldenv_memmem28(var_28_1);
    }
    
    return result;
}

long double sub_417f2c(int16_t arg1 @ x87control, long double arg2 @ st0, long double arg3 @ st1)
{
    long double var_1c = arg2;
    int32_t eax = *arg3[6];
    
    if (eax & 0x7fff0000)
        return sub_417d26(eax, 0, arg1, arg3);
    
    if (!(arg3 | *arg3[4]))
    {
        long double result;
        int80_t temp0_4;
        bool c2;
        result = __fprem(var_1c, arg3);
        return result;
    }
    
    int32_t edx = 2;
    int16_t x87status;
    int16_t temp0_2;
    temp0_2 = __fnstcw_memmem16(arg1);
    int16_t var_10 = temp0_2;
    int32_t var_c = var_10 | 0x33f;
    int16_t x87control;
    int16_t x87status_1;
    x87control = __fldcw_memmem16(var_c);
    int32_t eax_6 = *var_1c[8] & 0x7fff;
    long double var_34_1;
    
    if (eax_6 > 0x7fbe)
    {
        int16_t x87status_2;
        int16_t temp0_3;
        temp0_3 = __fnstcw_memmem16(x87control);
        var_10 = temp0_3;
        eax_6 = var_10 | 0x300;
        var_c = eax_6;
        int16_t x87control_1;
        int16_t x87status_3;
        x87control_1 = __fldcw_memmem16(var_c);
        var_34_1 = var_1c * data_43be74;
    }
    else
    {
        edx = 3;
        var_1c = var_1c * data_43be74;
        var_34_1 = arg3 * data_43be74;
    }
    
    int16_t x87control_2;
    int16_t x87status_4;
    x87control_2 = __fldcw_memmem16(var_10);
    return sub_417d26(eax_6, edx, x87control_2, var_34_1, arg3, var_1c, var_10, var_c);
}

long double __convention("regparm") sub_417fde(int32_t arg1, int32_t arg2, int16_t arg3 @ x87control, long double arg4, int32_t arg5, long double arg6, long double arg7)
{
    int32_t eax_1 = arg5 ^ 0x700;
    int16_t arg_28;
    long double result;
    long double result_3;
    bool c0;
    bool c1;
    bool c2_3;
    bool c3;
    
    if (eax_1 & 0x700 || !*((eax_1 >> 0xb & 0xf) + 0x43be5c) || (arg5 & 0x7fff0000) == 0x7fff0000)
    {
    label_418178:
        result_3 = arg4;
        long double result_1;
        uint8_t temp0_7;
        result_1 = __fprem1(arg7, result_3);
        result = result_1;
        
        if (!c2_3)
        {
            c0 = temp0_7 & 4;
            c1 = temp0_7 & 1;
            c3 = temp0_7 & 2;
        }
    }
    else
    {
        int32_t eax_7 = *arg7[6] & 0x7fff0000;
        
        if (!eax_7 || eax_7 == 0x7fff0000)
            goto label_418178;
        
        int32_t eax_8 = *arg7[4];
        
        if (eax_8 != -(eax_8))
            goto label_418178;
        
        int32_t eax_10 = *arg4[4];
        
        if (eax_10 != -(eax_10))
            goto label_418178;
        
        int32_t ebx_2 = *arg7[8] & 0x7fff;
        
        if (ebx_2 <= (arg5 & 0x7fff) + 0x3f)
        {
            while ((*arg7[8] & 0x7fff) - ((arg5 & 0x7fff) + 0xa) >= 0)
            {
                int32_t eax_18 = arg5;
                int32_t ebx_8 = *arg7[8] & 0x7fff;
                arg5 = (ebx_8 - (((ebx_8 - eax_18) & 7) | 4)) | (eax_18 & 0x8000);
                arg5 = eax_18;
                long double st0_1;
                uint8_t temp0_2;
                bool c2_1;
                st0_1 = __fprem(arg7, arg4);
                
                if (!c2_1)
                {
                    c0 = temp0_2 & 4;
                    c1 = temp0_2 & 1;
                    c3 = temp0_2 & 2;
                }
                
                arg7 = st0_1;
            }
            
            goto label_418178;
        }
        
        if (!((ebx_2 - ((arg5 & 0x7fff) + 0x3f)) & 2))
            arg6 = arg4;
        
        int16_t x87status_1;
        char temp0_3;
        temp0_3 = __fnstcw_memmem16(arg3);
        arg_28 = temp0_3;
        int16_t x87control;
        int16_t x87status_2;
        x87control = __fldcw_memmem16(arg_28 | 0x33f);
        int32_t i_1 = ((((*arg7[8] & 0x7fff) - (arg5 & 0x7fff)) & 0x3f) | 0x20) + 1;
        arg5 = (*arg7[8] & 0x7fff) | (arg5 & 0x8000);
        long double x87_r7_5 = fabsl(arg4);
        long double result_2 = fabsl(arg7);
        int32_t i;
        
        do
        {
            result_2 - x87_r7_5;
            c0 = result_2 < x87_r7_5;
            c1 = false;
            c3 = result_2 == x87_r7_5;
            int32_t eax_24;
            eax_24 = (c0 ? 1 : 0) << 8 | (FCMP_UO(result_2, x87_r7_5) ? 1 : 0) << 0xa
                | (c3 ? 1 : 0) << 0xe | 0x3000;
            
            if (!(eax_24 & 0x100))
                result_2 = result_2 - x87_r7_5;
            
            x87_r7_5 = x87_r7_5 * data_43be8c;
            i = i_1;
            i_1 -= 1;
        } while (i != 1);
        long double result_4;
        uint8_t temp0_6;
        result_4 = __fprem1(data_43be94, arg6);
        
        if (!c2_3)
        {
            c0 = temp0_6 & 4;
            c1 = temp0_6 & 1;
            c3 = temp0_6 & 2;
        }
        
        result_3 = result_4;
        result = result_2;
        int16_t x87status_3;
        arg3 = __fldcw_memmem16(arg_28);
        
        if (*arg7[8] & 0x8000)
        {
            result = -(result);
            c1 = false;
        }
    }
    
    if (arg2 & 3)
    {
        if (arg2 & 1)
        {
            int16_t x87status_4;
            char temp0_8;
            temp0_8 = __fnstcw_memmem16(arg3);
            arg_28 = temp0_8;
            int16_t x87control_1;
            int16_t x87status_5;
            x87control_1 = __fldcw_memmem16(arg_28 | 0x300);
            data_43be7c;
            int16_t x87control_2;
            int16_t x87status_6;
            x87control_2 = __fldcw_memmem16(arg_28);
        }
        
        result = result_3;
        int16_t x87status_7;
        char temp0_11;
        temp0_11 = __fnstenv_memmem28();
        uint224_t var_28_1;
        *var_28_1[4] = *temp0_11[4] & 0xbcff;
        *var_28_1[4] |= ((c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2_3 ? 1 : 0) << 0xa
            | (c3 ? 1 : 0) << 0xe | 0x3000) & 0x4300;
        __fldenv_memmem28(var_28_1);
    }
    
    return result;
}

long double sub_4181e4(int16_t arg1 @ x87control, long double arg2 @ st0, long double arg3 @ st1)
{
    long double var_1c = arg2;
    int32_t eax = *arg3[6];
    
    if (eax & 0x7fff0000)
        return sub_417fde(eax, 0, arg1, arg3);
    
    if (!(arg3 | *arg3[4]))
    {
        long double result;
        int80_t temp0_4;
        bool c2;
        result = __fprem(var_1c, arg3);
        return result;
    }
    
    int32_t edx = 2;
    int16_t x87status;
    int16_t temp0_2;
    temp0_2 = __fnstcw_memmem16(arg1);
    int16_t var_10 = temp0_2;
    int32_t var_c = var_10 | 0x33f;
    int16_t x87control;
    int16_t x87status_1;
    x87control = __fldcw_memmem16(var_c);
    int32_t eax_6 = *var_1c[8] & 0x7fff;
    long double var_34_1;
    
    if (eax_6 > 0x7fbe)
    {
        int16_t x87status_2;
        int16_t temp0_3;
        temp0_3 = __fnstcw_memmem16(x87control);
        var_10 = temp0_3;
        eax_6 = var_10 | 0x300;
        var_c = eax_6;
        int16_t x87control_1;
        int16_t x87status_3;
        x87control_1 = __fldcw_memmem16(var_c);
        var_34_1 = var_1c * data_43be74;
    }
    else
    {
        edx = 3;
        var_1c = var_1c * data_43be74;
        var_34_1 = arg3 * data_43be74;
    }
    
    int16_t x87control_2;
    int16_t x87status_4;
    x87control_2 = __fldcw_memmem16(var_10);
    return sub_417fde(eax_6, edx, x87control_2, var_34_1, arg3, var_1c, var_10, var_c);
}

int80_t sub_418299()
{
    int16_t x87control;
    long double x87_r0;
    long double x87_r1;
    return sub_417f2c(x87control, x87_r0, x87_r1);
}

int80_t sub_41829f()
{
    int16_t x87control;
    long double x87_r0;
    long double x87_r1;
    return sub_4181e4(x87control, x87_r0, x87_r1);
}

long double sub_4182a5(long double arg1 @ st0, long double arg2 @ st1)
{
    return __fpatan(arg1, arg2);
}

int32_t sub_4182a8(long double arg1 @ st0)
{
    long double st0;
    bool c2;
    st0 = __fptan(arg1);
}

int32_t sub_4182b0(int32_t arg1, int32_t* arg2, int16_t* arg3)
{
    double* ecx;
    ecx = *arg3;
    double* var_5c = ecx;
    int32_t eax_1 = *arg2 - 1;
    int32_t edi;
    
    if (eax_1 > 7)
        edi = 0;
    else
        switch (eax_1)
        {
            case 0:
            case 4:
            {
                edi = 8;
                break;
            }
            case 1:
            {
                edi = 4;
                break;
            }
            case 2:
            {
                edi = 0x11;
                break;
            }
            case 3:
            {
                edi = 0x12;
                break;
            }
            case 5:
            {
                edi = 0;
                break;
            }
            case 6:
            {
                *arg2 = 1;
                edi = 0;
                break;
            }
            case 7:
            {
                edi = 0x10;
                break;
            }
        }
    
    if (edi)
    {
        double* eax_2 = var_5c;
        double* var_6c_1 = eax_2;
        int32_t* var_70_1 = &arg2[6];
        int32_t edx;
        
        if (!sub_419bb0(eax_2, edx, ecx, edi))
        {
            int32_t var_20;
            
            if (arg1 == 0x10 || arg1 == 0x16 || arg1 == 0x1d)
            {
                int32_t var_2c_1 = arg2[5];
                int32_t var_20_3 = ((var_20 | 1) & 0xffffffe3) | 2;
                int32_t var_30_1 = arg2[4];
            }
            else
                int32_t var_20_1 = var_20 & 0xfffffffe;
            void var_58;
            sub_419840(&var_58, &var_5c, edi, arg1, &arg2[2], &arg2[6]);
            /* no return */
        }
    }
    
    int16_t x87control;
    sub_419ef0(x87control, var_5c, 0xffff);
    
    if (*arg2 != 8 && !data_43c0b8)
    {
        int32_t* var_6c_3 = arg2;
        sub_419ea0();
    }
    
    return sub_419e70(*arg2);
}

BOOL sub_4183d0(uint32_t arg1, uint8_t* arg2, int32_t arg3, uint16_t* arg4, uint32_t arg5, uint32_t arg6)
{
    BOOL eax = data_43bfa4;
    uint16_t charType;
    
    if (!eax)
    {
        if (!GetStringTypeA(0, 1, &data_41b728, 1, &charType))
        {
            if (!GetStringTypeW(1, &data_41b72c, 1, &charType))
                return 0;
            
            eax = 1;
        }
        else
            eax = 2;
    }
    data_43bfa4 = eax;
    
    if (eax == 2)
    {
        uint32_t Locale = arg6;
        
        if (!Locale)
            Locale = data_43bdd8;
        
        return GetStringTypeA(Locale, arg1, arg2, arg3, arg4);
    }
    
    data_43bfa4 = eax;
    
    if (eax != 1)
        return eax;
    
    BOOL edi_1 = 0;
    void* esi_1 = nullptr;
    uint32_t CodePage = arg5;
    
    if (!CodePage)
        CodePage = data_43bde8;
    
    int32_t cchWideChar = MultiByteToWideChar(CodePage, MB_ERR_INVALID_CHARS | MB_PRECOMPOSED, 
        arg2, arg3, nullptr, 0);
    
    if (cchWideChar)
    {
        esi_1 = sub_4117e0(2, cchWideChar);
        
        if (esi_1)
        {
            int32_t cchSrc =
                MultiByteToWideChar(CodePage, MB_PRECOMPOSED, arg2, arg3, esi_1, cchWideChar);
            
            if (cchSrc)
                edi_1 = GetStringTypeW(arg1, esi_1, cchSrc, arg4);
        }
    }
    
    sub_411250(esi_1);
    return edi_1;
}

int32_t sub_418500(uint32_t arg1, uint32_t arg2, uint8_t* arg3, void* arg4, PWSTR arg5, int32_t arg6, uint32_t arg7)
{
    int32_t eax = data_43bfac;
    
    if (!eax)
    {
        if (!LCMapStringA(0, 0x100, &data_41b728, 1, nullptr, 0))
        {
            if (!LCMapStringW(0, 0x100, &data_41b72c, 1, nullptr, 0))
                return 0;
            
            eax = 1;
        }
        else
            eax = 2;
    }
    
    void* esi = arg4;
    data_43bfac = eax;
    
    if (esi > 0)
    {
        esi = sub_418730(arg3, esi);
        eax = data_43bfac;
    }
    
    data_43bfac = eax;
    
    if (eax == 2)
        return LCMapStringA(arg1, arg2, arg3, esi, arg5, arg6);
    
    data_43bfac = eax;
    
    if (eax != 1)
        return eax;
    
    wchar16* edi_1 = nullptr;
    
    if (!arg7)
        arg7 = data_43bde8;
    
    int32_t eax_11 =
        MultiByteToWideChar(arg7, MB_ERR_INVALID_CHARS | MB_PRECOMPOSED, arg3, esi, nullptr, 0);
    
    if (!eax_11)
        return 0;
    
    void* eax_14 = sub_4111a0(eax_11 << 1);
    
    if (!eax_14)
        return 0;
    
    if (MultiByteToWideChar(arg7, MB_PRECOMPOSED, arg3, esi, eax_14, eax_11))
    {
        int32_t esi_1 = LCMapStringW(arg1, arg2, eax_14, eax_11, nullptr, 0);
        
        if (esi_1)
        {
            if (!(*arg2[1] & 4))
            {
                edi_1 = sub_4111a0(esi_1 << 1);
                
                if (edi_1 && LCMapStringW(arg1, arg2, eax_14, eax_11, edi_1, esi_1))
                {
                    if (arg6)
                    {
                        esi_1 = WideCharToMultiByte(arg7, 0x220, edi_1, esi_1, arg5, arg6, nullptr, 
                            nullptr);
                        
                        if (esi_1)
                            goto label_418713;
                    }
                    else
                    {
                        esi_1 = WideCharToMultiByte(arg7, 0x220, edi_1, esi_1, nullptr, 0, nullptr, 
                            nullptr);
                        
                        if (esi_1)
                            goto label_418713;
                    }
                }
            }
            else
            {
                if (!arg6)
                {
                label_418713:
                    sub_411250(eax_14);
                    sub_411250(edi_1);
                    return esi_1;
                }
                
                if (esi_1 <= arg6 && LCMapStringW(arg1, arg2, eax_14, eax_11, arg5, arg6))
                    goto label_418713;
            }
        }
    }
    
    sub_411250(eax_14);
    sub_411250(edi_1);
    return 0;
}

void* sub_418730(char* arg1, void* arg2)
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

int32_t sub_418760(int32_t arg1, int32_t arg2, int32_t* arg3)
{
    int32_t result = 0;
    int32_t esi = arg1 + arg2;
    
    if (arg1 > esi || arg2 > esi)
        result = 1;
    
    *arg3 = esi;
    return result;
}

int32_t sub_418790(int32_t* arg1, int32_t* arg2)
{
    if (sub_418760(*arg1, *arg2, arg1) && sub_418760(arg1[1], 1, &arg1[1]))
        arg1[2] += 1;
    
    if (sub_418760(arg1[1], arg2[1], &arg1[1]))
        arg1[2] += 1;
    
    return sub_418760(arg1[2], arg2[2], &arg1[2]);
}

int32_t sub_418800(int32_t* arg1)
{
    int32_t eax = *arg1;
    int32_t ecx = arg1[1];
    *arg1 = eax * 2;
    arg1[1] = ecx << 1 | (eax & 0x80000000) >> 0x1f;
    int32_t result = (arg1[2] * 2) | (ecx & 0x80000000) >> 0x1f;
    arg1[2] = result;
    return result;
}

int32_t sub_418840(int32_t* arg1)
{
    int32_t ecx = arg1[2];
    int32_t eax = arg1[1];
    arg1[2] = ecx >> 1;
    arg1[1] = eax >> 1 | (ecx & 1) << 0x1f;
    int32_t result = *arg1 >> 1 | (eax & 1) << 0x1f;
    *arg1 = result;
    return result;
}

void sub_418880(void* arg1, int32_t arg2, int32_t* arg3)
{
    int32_t edi;
    edi = 0x404e;
    int32_t i = arg2;
    *arg3 = 0;
    arg3[1] = 0;
    arg3[2] = 0;
    
    if (i)
    {
        void* ebp_1 = arg1;
        
        do
        {
            int32_t edx_1 = arg3[1];
            int32_t var_c = *arg3;
            int32_t var_8_1 = edx_1;
            int32_t var_4_1 = arg3[2];
            sub_418800(arg3);
            i -= 1;
            ebp_1 += 1;
            sub_418800(arg3);
            sub_418790(arg3, &var_c);
            sub_418800(arg3);
            int32_t var_8_2 = 0;
            int32_t var_4_2 = 0;
            var_c = *(ebp_1 - 1);
            sub_418790(arg3, &var_c);
        } while (i);
    }
    
    if (!arg3[2])
    {
        uint32_t i_1;
        
        do
        {
            edi -= 0x10;
            int32_t edx_2 = arg3[1];
            int32_t eax_3 = *arg3;
            i_1 = edx_2 >> 0x10;
            arg3[2] = i_1;
            arg3[1] = edx_2 << 0x10 | eax_3 >> 0x10;
            *arg3 = eax_3 << 0x10;
        } while (!i_1);
    }
    
    while (!(*(arg3 + 9) & 0x80))
    {
        edi -= 1;
        sub_418800(arg3);
    }
    
    *(arg3 + 0xa) = edi;
}

int32_t sub_418970(int16_t* arg1, char** arg2, char* arg3, int32_t arg4, int32_t arg5, int32_t arg6, int32_t arg7)
{
    int32_t ebx;
    int32_t var_58 = ebx;
    int32_t var_3c = 1;
    int16_t var_28;
    int16_t* ebp = &var_28;
    int32_t edi = 0;
    int16_t var_52 = 0;
    int32_t var_50 = 0;
    int32_t var_44 = 0;
    int32_t var_34 = 0;
    int32_t var_38 = 0;
    int32_t var_30 = 0;
    int32_t i = 0;
    char* esi = arg3;
    int32_t var_2c = 0;
    int32_t var_48 = 0;
    int32_t result = 0;
    void* var_4c = esi;
    
    while (true)
    {
        char eax = *esi;
        
        if (eax != 0x20 && eax != 9 && eax != 0xa && eax != 0xd)
        {
            do
            {
                ebx = *esi;
                esi = &esi[1];
                
                switch (i)
                {
                    case 0:
                    {
                        if (ebx >= 0x31 && ebx <= 0x39)
                        {
                            i = 3;
                            esi -= 1;
                        }
                        else if (data_43bb84 != ebx)
                        {
                            int32_t eax_1 = ebx;
                            
                            if (eax_1 == 0x2b)
                            {
                                var_52 = 0;
                                i = 2;
                            }
                            else if (eax_1 == 0x2d)
                            {
                                var_52 = 0x8000;
                                i = 2;
                            }
                            else if (eax_1 == 0x30)
                                i = 1;
                            else
                            {
                                i = 0xa;
                                esi -= 1;
                            }
                        }
                        else
                            i = 5;
                        break;
                    }
                    case 1:
                    {
                        var_44 = 1;
                        
                        if (ebx >= 0x31 && ebx <= 0x39)
                        {
                            i = 3;
                            esi -= 1;
                        }
                        else if (data_43bb84 == ebx)
                            i = 4;
                        else if (ebx - 0x2b > 0x3a)
                        {
                            i = 0xa;
                            esi -= 1;
                        }
                        else
                        {
                            int32_t eax_2;
                            eax_2 = *(ebx + &*jump_table_418f98[7][1]);
                            
                            switch (eax_2)
                            {
                                case 0:
                                case 1:
                                {
                                    esi -= 1;
                                    i = 0xb;
                                    break;
                                }
                                case 2:
                                {
                                    i = 1;
                                    break;
                                }
                                case 3:
                                case 4:
                                {
                                    i = 6;
                                    break;
                                }
                                case 5:
                                {
                                    i = 0xa;
                                    esi -= 1;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    case 2:
                    {
                        if (ebx >= 0x31 && ebx <= 0x39)
                        {
                            i = 3;
                            esi -= 1;
                        }
                        else if (data_43bb84 == ebx)
                            i = 5;
                        else if (ebx == 0x30)
                            i = 1;
                        else
                        {
                            i = 0xa;
                            esi = var_4c;
                        }
                        break;
                    }
                    case 3:
                    {
                        var_44 = 1;
                        
                        while (true)
                        {
                            int32_t eax_5;
                            
                            if (data_43bb80 <= 1)
                            {
                                int32_t ecx_2;
                                ecx_2 = ebx;
                                int32_t eax_6;
                                eax_6 = (**&data_43bb90)[ecx_2];
                                eax_5 = eax_6 & 4;
                            }
                            else
                            {
                                int32_t eax_4;
                                eax_4 = ebx;
                                eax_5 = sub_4158a0(eax_4, 4);
                            }
                            
                            if (!eax_5)
                                break;
                            
                            if (var_50 >= 0x19)
                            {
                                ebx = *esi;
                                esi = &esi[1];
                                var_48 += 1;
                            }
                            else
                            {
                                ebx -= 0x30;
                                ebp += 1;
                                esi = &esi[1];
                                var_50 += 1;
                                *(ebp - 1) = ebx;
                                ebx = esi[0xffffffff];
                            }
                        }
                        
                        if (data_43bb84 == ebx)
                            i = 4;
                        else if (ebx - 0x2b > 0x3a)
                        {
                            i = 0xa;
                            esi -= 1;
                        }
                        else
                        {
                            int32_t eax_7;
                            eax_7 = lookup_table_418fe0[0x25 + ebx];
                            
                            switch (eax_7)
                            {
                                case 0:
                                case 1:
                                {
                                    esi -= 1;
                                    i = 0xb;
                                    break;
                                }
                                case 2:
                                case 3:
                                {
                                    i = 6;
                                    break;
                                }
                                case 4:
                                {
                                    i = 0xa;
                                    esi -= 1;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    case 4:
                    {
                        var_44 = 1;
                        var_34 = 1;
                        
                        if (!var_50 && ebx == 0x30)
                        {
                            do
                            {
                                ebx = *esi;
                                esi = &esi[1];
                                var_48 -= 1;
                            } while (ebx == 0x30);
                        }
                        
                        while (true)
                        {
                            int32_t eax_9;
                            
                            if (data_43bb80 <= 1)
                            {
                                int32_t ecx_5;
                                ecx_5 = ebx;
                                int32_t eax_10;
                                eax_10 = (**&data_43bb90)[ecx_5];
                                eax_9 = eax_10 & 4;
                            }
                            else
                            {
                                int32_t eax_8;
                                eax_8 = ebx;
                                eax_9 = sub_4158a0(eax_8, 4);
                            }
                            
                            if (!eax_9)
                                break;
                            
                            if (var_50 < 0x19)
                            {
                                ebx -= 0x30;
                                ebp += 1;
                                var_50 += 1;
                                var_48 -= 1;
                                *(ebp - 1) = ebx;
                            }
                            
                            ebx = *esi;
                            esi = &esi[1];
                        }
                        
                        if (ebx - 0x2b > 0x3a)
                        {
                            i = 0xa;
                            esi -= 1;
                        }
                        else
                        {
                            int32_t eax_11;
                            eax_11 = lookup_table_419030[0x25 + ebx];
                            
                            switch (eax_11)
                            {
                                case 0:
                                case 1:
                                {
                                    esi -= 1;
                                    i = 0xb;
                                    break;
                                }
                                case 2:
                                case 3:
                                {
                                    i = 6;
                                    break;
                                }
                                case 4:
                                {
                                    i = 0xa;
                                    esi -= 1;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    case 5:
                    {
                        var_34 = 1;
                        int32_t eax_13;
                        
                        if (data_43bb80 <= 1)
                        {
                            int32_t ecx_8;
                            ecx_8 = ebx;
                            int32_t eax_14;
                            eax_14 = (**&data_43bb90)[ecx_8];
                            eax_13 = eax_14 & 4;
                        }
                        else
                        {
                            int32_t eax_12;
                            eax_12 = ebx;
                            eax_13 = sub_4158a0(eax_12, 4);
                        }
                        
                        if (!eax_13)
                        {
                            i = 0xa;
                            esi = var_4c;
                        }
                        else
                        {
                            i = 4;
                            esi -= 1;
                        }
                        break;
                    }
                    case 6:
                    {
                        var_4c = &esi[0xfffffffe];
                        
                        if (ebx < 0x31 || ebx > 0x39)
                        {
                            int32_t eax_16 = ebx;
                            
                            if (eax_16 == 0x2b)
                                i = 7;
                            else if (eax_16 == 0x2d)
                            {
                                var_3c = 0xffffffff;
                                i = 7;
                            }
                            else if (eax_16 == 0x30)
                                i = 8;
                            else
                            {
                                i = 0xa;
                                esi = var_4c;
                            }
                        }
                        else
                        {
                            i = 9;
                            esi -= 1;
                        }
                        break;
                    }
                    case 7:
                    {
                        if (ebx >= 0x31 && ebx <= 0x39)
                        {
                            i = 9;
                            esi -= 1;
                        }
                        else if (ebx == 0x30)
                            i = 8;
                        else
                        {
                            i = 0xa;
                            esi = var_4c;
                        }
                        break;
                    }
                    case 8:
                    {
                        var_38 = 1;
                        
                        while (ebx == 0x30)
                        {
                            ebx = *esi;
                            esi = &esi[1];
                        }
                        
                        if (ebx < 0x31 || ebx > 0x39)
                        {
                            i = 0xa;
                            esi -= 1;
                        }
                        else
                        {
                            i = 9;
                            esi -= 1;
                        }
                        break;
                    }
                    case 9:
                    {
                        var_38 = 1;
                        edi = 0;
                        
                        while (true)
                        {
                            int32_t eax_19;
                            
                            if (data_43bb80 <= 1)
                            {
                                int32_t ecx_9;
                                ecx_9 = ebx;
                                int32_t eax_20;
                                eax_20 = (**&data_43bb90)[ecx_9];
                                eax_19 = eax_20 & 4;
                            }
                            else
                            {
                                int32_t eax_18;
                                eax_18 = ebx;
                                eax_19 = sub_4158a0(eax_18, 4);
                            }
                            
                            if (!eax_19)
                                break;
                            
                            edi = ebx + edi * 0xa - 0x30;
                            
                            if (edi > 0x1450)
                            {
                                edi = 0x1451;
                                break;
                            }
                            
                            ebx = *esi;
                            esi = &esi[1];
                        }
                        
                        while (true)
                        {
                            int32_t eax_23;
                            
                            if (data_43bb80 <= 1)
                            {
                                int32_t ecx_11;
                                ecx_11 = ebx;
                                int32_t eax_24;
                                eax_24 = (**&data_43bb90)[ecx_11];
                                eax_23 = eax_24 & 4;
                            }
                            else
                            {
                                int32_t eax_22;
                                eax_22 = ebx;
                                eax_23 = sub_4158a0(eax_22, 4);
                            }
                            
                            if (!eax_23)
                                break;
                            
                            ebx = *esi;
                            esi = &esi[1];
                        }
                        
                        i = 0xa;
                        esi -= 1;
                        break;
                    }
                    case 0xb:
                    {
                        if (!arg7)
                        {
                            i = 0xa;
                            esi -= 1;
                        }
                        else
                        {
                            var_4c = &esi[0xffffffff];
                            int32_t eax_26 = ebx;
                            
                            if (eax_26 == 0x2b)
                                i = 7;
                            else if (eax_26 == 0x2d)
                            {
                                var_3c = 0xffffffff;
                                i = 7;
                            }
                            else
                            {
                                i = 0xa;
                                esi = var_4c;
                            }
                        }
                        break;
                    }
                }
            } while (i != 0xa);
            
            *arg2 = esi;
            
            if (!var_44)
                goto label_418f10;
            
            if (var_50 > 0x18)
            {
                char var_11;
                
                if (var_11 >= 5)
                    char var_11_1 = var_11 + 1;
                
                ebp -= 1;
                var_48 += 1;
                var_50 = 0x18;
            }
            
            int32_t ecx_13;
            int32_t edx_6;
            
            if (!var_50)
            {
                esi = 0;
                edx_6 = 0;
                edi = 0;
                ecx_13 = 0;
            }
            else
            {
                void* ebp_1 = ebp - 1;
                
                while (!*ebp_1)
                {
                    ebp_1 -= 1;
                    var_50 -= 1;
                    var_48 += 1;
                }
                
                int16_t var_c;
                sub_418880(&var_28, var_50, &var_c);
                
                if (var_3c < 0)
                    edi = -(edi);
                
                edi += var_48;
                
                if (!var_38)
                    edi += arg5;
                
                if (!var_34)
                    edi -= arg6;
                
                if (edi <= 0x1450)
                {
                    if (edi < 0xffffebb0)
                    {
                        var_2c = 1;
                        goto label_418f10;
                    }
                    
                    sub_41a240(&var_c, edi, arg4);
                    esi = var_c;
                    int32_t var_a;
                    ecx_13 = var_a;
                    int16_t var_2;
                    edi = var_2;
                    int32_t var_6;
                    edx_6 = var_6;
                }
                else
                {
                    var_30 = 1;
                label_418f10:
                    esi = var_28;
                    ecx_13 = var_28;
                    edi = var_28;
                    edx_6 = var_28;
                }
            }
            
            if (!var_44)
            {
                esi = 0;
                edx_6 = 0;
                edi = 0;
                ecx_13 = 0;
                result = 4;
            }
            else if (var_30)
            {
                edi = 0x7fff;
                edx_6 = 0x80000000;
                esi = 0;
                ecx_13 = 0;
                result = 2;
            }
            else if (var_2c)
            {
                esi = 0;
                edx_6 = 0;
                edi = 0;
                ecx_13 = 0;
                result = 1;
            }
            
            edi |= var_52;
            *arg1 = esi;
            *(arg1 + 2) = ecx_13;
            arg1[5] = edi;
            *(arg1 + 6) = edx_6;
            return result;
        }
        
        esi = &esi[1];
    }
}

int32_t sub_4190c0(int32_t arg1, int32_t arg2, uint16_t arg3, int32_t arg4, char arg5, int16_t* arg6)
{
    char var_18 = 0xcc;
    char var_17 = 0xcc;
    char var_16 = 0xcc;
    char var_15 = 0xcc;
    char var_14 = 0xcc;
    char var_13 = 0xcc;
    char var_12 = 0xcc;
    char var_11 = 0xcc;
    char var_10 = 0xcc;
    char var_f = 0xcc;
    char var_e = 0xfb;
    char var_d = 0x3f;
    int32_t result = 1;
    uint16_t eax = arg3 & 0x8000;
    uint16_t ecx = arg3 & 0x7fff;
    
    if (!eax)
        arg6[1] = 0x20;
    else
        arg6[1] = 0x2d;
    
    if (!ecx && !arg2 && !arg1)
    {
        arg6[1] = 0x20;
        *arg6 = 0;
        *(arg6 + 3) = 1;
        arg6[2] = 0x30;
        *(arg6 + 5) = 0;
        return 1;
    }
    
    if (ecx != 0x7fff)
    {
        int16_t esi_7 =
            (((ecx >> 8) + (arg2 >> 0x18 << 1)) * 0x4d + ecx * 0x4d10 - 0x134312f4) >> 0x10;
        int16_t var_28 = 0;
        sub_41a240(&var_28, -(esi_7), 1);
        
        if (ecx >= 0x3fff)
        {
            esi_7 += 1;
            sub_419f90(&var_28, &var_18);
        }
        
        *arg6 = esi_7;
        int32_t esi_9;
        
        if (!(arg5 & 1))
            esi_9 = arg4;
        else
        {
            esi_9 = arg4 + esi_7;
            
            if (esi_9 <= 0)
            {
                arg6[1] = 0x20;
                *arg6 = 0;
                *(arg6 + 3) = 1;
                arg6[2] = 0x30;
                *(arg6 + 5) = 0;
                return 1;
            }
        }
        
        if (esi_9 > 0x15)
            esi_9 = 0x15;
        
        int32_t i_4 = 8;
        int32_t ebx_1;
        ebx_1 = ecx;
        int16_t var_1e_2 = 0;
        int32_t i;
        
        do
        {
            sub_418800(&var_28);
            i = i_4;
            i_4 -= 1;
        } while (i != 1);
        
        if (ebx_1 - 0x3ffe < 0)
        {
            uint32_t i_3 = -((ebx_1 - 0x3ffe));
            
            if (i_3 > 0)
            {
                uint32_t i_1;
                
                do
                {
                    sub_418840(&var_28);
                    i_1 = i_3;
                    i_3 -= 1;
                } while (i_1 != 1);
            }
        }
        
        int32_t i_5 = esi_9 + 1;
        void* ebx_4 = &arg6[2];
        
        if (i_5 > 0)
        {
            int32_t i_2;
            
            do
            {
                ebx_4 += 1;
                int32_t var_c = var_28;
                int32_t var_8_1 = arg1;
                int32_t var_4_1 = arg2;
                sub_418800(&var_28);
                sub_418800(&var_28);
                sub_418790(&var_28, &var_c);
                sub_418800(&var_28);
                i_2 = i_5;
                i_5 -= 1;
                *(ebx_4 - 1) = *var_1e_2[1] + 0x30;
                *var_1e_2[1] = 0;
            } while (i_2 != 1);
        }
        
        void* ebx_6 = ebx_4 - 2;
        
        if (*(ebx_4 - 1) < 0x35)
        {
            if (ebx_6 < &arg6[2])
            {
            label_419439:
                *arg6 = 0;
                arg6[2] = 0x30;
                arg6[1] = 0x20;
                *(arg6 + 3) = 1;
                *(arg6 + 5) = 0;
                return 1;
            }
            
            while (*ebx_6 == 0x30)
            {
                ebx_6 -= 1;
                
                if (ebx_6 < &arg6[2])
                    break;
            }
            
            if (ebx_6 < &arg6[2])
                goto label_419439;
        }
        else
        {
            if (ebx_6 < &arg6[2])
            {
                *arg6 += 1;
                ebx_6 += 1;
            }
            else
            {
                while (*ebx_6 == 0x39)
                {
                    *ebx_6 = 0x30;
                    ebx_6 -= 1;
                    
                    if (ebx_6 < &arg6[2])
                        break;
                }
                
                if (ebx_6 < &arg6[2])
                {
                    *arg6 += 1;
                    ebx_6 += 1;
                }
            }
            
            *ebx_6 += 1;
        }
        
        *(arg6 + 3) = ebx_6 - arg6 - 3;
        *(ebx_6 - arg6 - 3 + arg6 + 4) = 0;
    }
    else
    {
        *arg6 = 1;
        
        if ((arg2 != 0x80000000 || arg1) && !(*arg2[3] & 0x40))
        {
            result = 0;
            __builtin_strncpy(&arg6[2], "1#SNAN", 6);
            arg6[4] = 0x4e41;
            arg6[5] = nullptr;
            *(arg6 + 3) = 6;
        }
        else if (eax && arg2 == 0xc0000000 && !arg1)
        {
            result = 0;
            __builtin_strncpy(&arg6[2], "1#IN", 4);
            arg6[4] = 0x44;
            *(arg6 + 3) = 5;
        }
        else if (arg2 != 0x80000000 || arg1)
        {
            result = 0;
            __builtin_strncpy(&arg6[2], "1#QNAN", 6);
            arg6[5] = nullptr;
            *(arg6 + 3) = 6;
        }
        else
        {
            result = 0;
            __builtin_strncpy(&arg6[2], "1#IN", 4);
            arg6[4] = 0x46;
            *(arg6 + 3) = 5;
        }
    }
    
    return result;
}

int32_t __stdcall sub_419460(int32_t arg1, int32_t arg2)
{
    sub_41a340(0x1004);
    int32_t result = 0;
    
    if (data_454680 <= arg1
        || !(*(*(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ((arg1 & 0x1f) << 3) + 4) & 1))
    {
        data_43aa58 = 9;
        return 0xffffffff;
    }
    
    uint32_t eax_4 = sub_4143e0(arg1, 0, FILE_CURRENT);
    
    if (eax_4 != 0xffffffff)
    {
        uint32_t eax_5 = sub_4143e0(arg1, 0, FILE_END);
        
        if (eax_5 != 0xffffffff)
        {
            uint32_t i = arg2 - eax_5;
            
            if (i > 0)
            {
                int32_t var_14_2 = 0x8000;
                char arg_4[0x1000];
                __builtin_memset(&arg_4, 0, 0x1000);
                int32_t eax_6 = sub_41a2c0(arg1, var_14_2);
                
                do
                {
                    uint32_t i_1 = 0x1000;
                    
                    if (i < 0x1000)
                        i_1 = i;
                    
                    int32_t result_1 = sub_4141b0(arg1, &arg_4, i_1);
                    
                    if (result_1 == 0xffffffff)
                    {
                        if (data_43aa5c == 5)
                            data_43aa58 = 0xd;
                        
                        result = result_1;
                        break;
                    }
                    
                    i -= result_1;
                } while (i > 0);
                
                sub_41a2c0(arg1, eax_6);
            }
            else if (i < 0)
            {
                sub_4143e0(arg1, arg2, FILE_BEGIN);
                result = 0 - 0;
                
                if (result == 0xffffffff)
                {
                    data_43aa58 = 0xd;
                    data_43aa5c = GetLastError();
                }
            }
            
            sub_4143e0(arg1, eax_4, FILE_BEGIN);
            return result;
        }
    }
    
    return 0xffffffff;
}

long double sub_4195d0(int32_t arg1, int32_t arg2, int16_t arg3) __pure
{
    int32_t var_4 = arg2;
    int32_t ecx;
    ecx = *arg2[2];
    ecx &= 0x800f;
    *var_4[2] = (arg3 + 0x3fe) << 4 | ecx;
    return arg1;
}

int32_t sub_419610(int32_t arg1, int32_t arg2) __pure
{
    if (arg2 == 0x7ff00000 && !arg1)
        return 1;
    
    if (arg2 == 0xfff00000 && !arg1)
        return 2;
    
    int16_t eax = *arg2[2] & 0x7ff8;
    
    if (eax == 0x7ff8)
        return 3;
    
    if (eax == 0x7ff0 && (arg2 & 0x7ffff || arg1))
        return 4;
    
    return 0;
}

long double sub_419680(int32_t arg1, int32_t arg2, int32_t* arg3)
{
    int32_t var_8;
    int32_t esi_1;
    
    if ((arg2 & 0x7fffffff) | arg1)
    {
        int32_t esi;
        esi = *arg2[2];
        esi &= 0x7ff0;
        
        if (esi || (!(arg2 & 0xfffff) && !arg1))
        {
            esi u>>= 4;
            esi_1 = esi - 0x3fe;
            var_8 = sub_4195d0(arg1, arg2, 0);
        }
        else
        {
            long double x87_r7_1 = 0;
            long double temp0_1 = arg1;
            x87_r7_1 - temp0_1;
            esi_1 = 0xfffffc03;
            int32_t eax_1;
            eax_1 = (x87_r7_1 < temp0_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r7_1, temp0_1) ? 1 : 0) << 0xa
                | (x87_r7_1 == temp0_1 ? 1 : 0) << 0xe;
            int32_t eax_2 = 1;
            
            if (*eax_1[1] & 0x41)
                eax_2 = 0;
            
            if (!(*arg2[2] & 0x10))
            {
                do
                {
                    arg2 <<= 1;
                    
                    if (arg1 & 0x80000000)
                        arg2 |= 1;
                    
                    esi_1 -= 1;
                    arg1 <<= 1;
                } while (!(*arg2[2] & 0x10));
            }
            
            *arg2[2] &= 0xffef;
            
            if (eax_2)
                *arg2[3] |= 0x80;
            
            var_8 = sub_4195d0(arg1, arg2, 0);
        }
    }
    else
    {
        esi_1 = 0;
        var_8 = 0;
        int32_t var_4_1 = 0;
    }
    
    *arg3 = esi_1;
    return var_8;
}

long double sub_419770(double arg1) __pure
{
    return round(arg1, arg1);
}

int32_t sub_419790(int32_t arg1, int32_t arg2, int16_t arg3)
{
    int16_t ecx = arg3 & 0x7ff0;
    
    if (ecx != 0x7ff0)
    {
        int32_t eax_6;
        eax_6 = arg3;
        int32_t eax_7 = eax_6 & 0x8000;
        
        if (!((arg2 & 0x7fffffff) | arg1))
            return ((eax_7 - eax_7) & 0x20) + 0x20;
        
        if (!ecx && (arg2 & 0xfffff || arg1))
            return ((eax_7 - eax_7) & 0x70) + 0x10;
        
        return ((eax_7 - eax_7) & 0xf8) + 8;
    }
    
    int32_t eax_1 = sub_419610(arg1, arg2);
    
    if (eax_1 == 1)
        return 0x200;
    
    if (eax_1 == 2)
        return 4;
    
    if (eax_1 == 3)
        return 2;
    
    return 1;
}

void sub_419840(uint32_t arg1, int32_t* arg2, char arg3, int32_t arg4, int32_t* arg5, int32_t* arg6) __noreturn
{
    *(arg1 + 4) = 0;
    *(arg1 + 8) = 0;
    *(arg1 + 0xc) = 0;
    uint32_t dwExceptionCode;
    uint32_t dwExceptionCode_1;
    
    if (!(arg3 & 0x10))
        dwExceptionCode = dwExceptionCode_1;
    else
    {
        dwExceptionCode = 0xc000008f;
        uint32_t eax_2 = arg1;
        *(eax_2 + 4) |= 1;
    }
    
    if (arg3 & 2)
    {
        dwExceptionCode = 0xc0000093;
        uint32_t eax_3 = arg1;
        *(eax_3 + 4) |= 2;
    }
    
    if (arg3 & 1)
    {
        dwExceptionCode = 0xc0000091;
        uint32_t eax_4 = arg1;
        *(eax_4 + 4) |= 4;
    }
    
    if (arg3 & 4)
    {
        dwExceptionCode = 0xc000008e;
        uint32_t eax_5 = arg1;
        *(eax_5 + 4) |= 8;
    }
    
    if (arg3 & 8)
    {
        dwExceptionCode = 0xc0000090;
        uint32_t eax_6 = arg1;
        *(eax_6 + 4) |= 0x10;
    }
    
    int32_t ecx_1 = *(arg1 + 8);
    *(arg1 + 8) = (((~*arg2 & 1) << 4 ^ ecx_1) & 0x10) ^ ecx_1;
    int32_t eax_10 = *arg2 & 4;
    int32_t edx_7 = *(arg1 + 8);
    *(arg1 + 8) = ((-((eax_10 - eax_10)) << 3 ^ edx_7) & 8) ^ edx_7;
    int32_t eax_18 = *arg2 & 8;
    int32_t ecx_4 = *(arg1 + 8);
    *(arg1 + 8) = ((-((eax_18 - eax_18)) << 2 ^ ecx_4) & 4) ^ ecx_4;
    int32_t eax_26 = *arg2 & 0x10;
    int32_t ecx_5 = *(arg1 + 8);
    *(arg1 + 8) = (((-((eax_26 - eax_26)) * 2) ^ ecx_5) & 2) ^ ecx_5;
    int32_t eax_34 = *arg2 & 0x20;
    int32_t edx_12 = *(arg1 + 8);
    *(arg1 + 8) = ((-((eax_34 - eax_34)) ^ edx_12) & 1) ^ edx_12;
    char eax_40 = sub_419eb0();
    
    if (eax_40 & 1)
    {
        uint32_t ecx_8 = arg1;
        *(ecx_8 + 0xc) |= 0x10;
    }
    
    if (eax_40 & 4)
    {
        uint32_t ecx_9 = arg1;
        *(ecx_9 + 0xc) |= 8;
    }
    
    if (eax_40 & 8)
    {
        uint32_t ecx_10 = arg1;
        *(ecx_10 + 0xc) |= 4;
    }
    
    if (eax_40 & 0x10)
    {
        uint32_t ecx_11 = arg1;
        *(ecx_11 + 0xc) |= 2;
    }
    
    if (eax_40 & 0x20)
    {
        uint32_t eax_41 = arg1;
        *(eax_41 + 0xc) |= 1;
    }
    
    int32_t eax_43 = *arg2 & 0xc00;
    
    if (eax_43 == 0x800)
        *arg1 = (*arg1 & 0xfffffffe) | 2;
    else if (eax_43 == 0xc00)
    {
        uint32_t eax_53 = arg1;
        *eax_53 |= 3;
    }
    
    if (eax_43 == 0x400)
        *arg1 = (*arg1 & 0xfffffffd) | 1;
    else if (!eax_43)
    {
        uint32_t eax_44 = arg1;
        *eax_44 &= 0xfffffffc;
    }
    
    int32_t eax_55 = *arg2 & 0x300;
    
    if (!eax_55)
    {
        uint32_t eax_56 = arg1;
        *eax_56 = (*eax_56 & 0xffffffeb) | 8;
    }
    else if (eax_55 == 0x200)
        *arg1 = (*arg1 & 0xffffffe7) | 4;
    else if (eax_55 == 0x300)
    {
        uint32_t eax_61 = arg1;
        *eax_61 &= 0xffffffe3;
    }
    
    int32_t eax_63 = *arg1;
    *arg1 = ((arg4 << 5 ^ eax_63) & 0x1ffe0) ^ eax_63;
    uint32_t eax_65 = arg1;
    *(eax_65 + 0x20) |= 1;
    uint32_t eax_66 = arg1;
    *(eax_66 + 0x20) = (*(eax_66 + 0x20) & 0xffffffe3) | 2;
    uint32_t edx_13 = arg1;
    *(edx_13 + 0x14) = arg5[1];
    *(edx_13 + 0x10) = *arg5;
    uint32_t edx_14 = arg1;
    *(edx_14 + 0x50) |= 1;
    uint32_t eax_70 = arg1;
    *(eax_70 + 0x50) = (*(eax_70 + 0x50) & 0xffffffe3) | 2;
    uint32_t ecx_30 = arg1;
    *(ecx_30 + 0x44) = arg6[1];
    *(ecx_30 + 0x40) = *arg6;
    sub_419ed0();
    RaiseException(dwExceptionCode, 0, 1, &arg1);
    /* no return */
}

int32_t __stdcall sub_419ad0(int32_t* arg1 @ edi, int32_t arg2, int32_t arg3, int32_t* arg4)
{
    int32_t* entry_ebx;
    
    if (arg4[2] & 0x10)
        *entry_ebx &= 0xfffffffe;
    
    if (arg4[2] & 8)
        *entry_ebx &= 0xfffffffb;
    
    if (arg4[2] & 4)
        *entry_ebx &= 0xfffffff7;
    
    if (arg4[2] & 2)
        *entry_ebx &= 0xffffffef;
    
    if (arg4[2] & 1)
        *entry_ebx &= 0xffffffdf;
    
    switch (*arg4 & 3)
    {
        case 0:
        {
            *entry_ebx &= 0xfffff3ff;
            break;
        }
        case 1:
        {
            *entry_ebx = (*entry_ebx & 0xfffff7ff) | 0x400;
            break;
        }
        case 2:
        {
            *entry_ebx = (*entry_ebx & 0xfffffbff) | 0x800;
            break;
        }
        case 3:
        {
            *entry_ebx |= 0xc00;
            break;
        }
    }
    
    uint32_t eax_13 = (*arg4 & 0x1c) >> 2;
    
    if (!eax_13)
        *entry_ebx = (*entry_ebx & 0xfffff3ff) | 0x300;
    else if (eax_13 == 1)
        *entry_ebx = (*entry_ebx & 0xfffff3ff) | 0x200;
    else if (eax_13 == 2)
        *entry_ebx &= 0xfffff3ff;
    
    int32_t result = arg4[0x11];
    arg1[1] = result;
    *arg1 = arg4[0x10];
    return result;
}

int32_t __convention("regparm") sub_419bb0(int32_t arg1, int32_t arg2, double* arg3, char arg4)
{
    int32_t edi_1 = arg2 & 0x1f;
    long double st0;
    long double x87_r0;
    
    if (arg2 & 8 && arg4 & 1)
    {
        edi_1 &= 0xfffffff7;
        arg1 = sub_419f30(x87_r0, 1);
    }
    else if (arg2 & 4 && arg4 & 4)
    {
        edi_1 &= 0xfffffffb;
        arg1 = sub_419f30(x87_r0, 4);
    }
    else if (arg2 & 1 && arg4 & 8)
    {
        sub_419f30(x87_r0, 8);
        arg1 = arg4 & 0xc00;
        
        if (arg1 > 0x400)
        {
            if (arg1 == 0x800)
            {
                long double x87_r0_7 = 0;
                long double temp0_1 = *arg3;
                x87_r0_7 - temp0_1;
                arg1 = (x87_r0_7 < temp0_1 ? 1 : 0) << 8
                    | (FCMP_UO(x87_r0_7, temp0_1) ? 1 : 0) << 0xa
                    | (x87_r0_7 == temp0_1 ? 1 : 0) << 0xe | 0x800;
                
                if (!(*arg1[1] & 1))
                {
                    edi_1 &= 0xfffffffe;
                    *arg3 = -(data_43bfc8);
                }
                else
                {
                    arg1 = data_43bfbc;
                    edi_1 &= 0xfffffffe;
                    *(arg3 + 4) = arg1;
                    *arg3 = data_43bfb8;
                }
            }
            else if (arg1 == 0xc00)
            {
                long double x87_r0_10 = 0;
                long double temp2_1 = *arg3;
                x87_r0_10 - temp2_1;
                arg1 = (x87_r0_10 < temp2_1 ? 1 : 0) << 8
                    | (FCMP_UO(x87_r0_10, temp2_1) ? 1 : 0) << 0xa
                    | (x87_r0_10 == temp2_1 ? 1 : 0) << 0xe | 0x800;
                
                if (!(*arg1[1] & 1))
                {
                    edi_1 &= 0xfffffffe;
                    *arg3 = -(data_43bfc8);
                }
                else
                {
                    arg1 = *(data_43bfc8 + 4);
                    edi_1 &= 0xfffffffe;
                    *(arg3 + 4) = arg1;
                    *arg3 = *data_43bfc8;
                }
            }
            else
                edi_1 &= 0xfffffffe;
        }
        else if (arg1 == 0x400)
        {
            long double x87_r0_4 = 0;
            long double temp1_1 = *arg3;
            x87_r0_4 - temp1_1;
            arg1 = (x87_r0_4 < temp1_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r0_4, temp1_1) ? 1 : 0) << 0xa
                | (x87_r0_4 == temp1_1 ? 1 : 0) << 0xe | 0x800;
            
            if (!(*arg1[1] & 1))
            {
                edi_1 &= 0xfffffffe;
                *arg3 = -(*data_43bfb8);
            }
            else
            {
                arg1 = *(data_43bfc8 + 4);
                edi_1 &= 0xfffffffe;
                *(arg3 + 4) = arg1;
                *arg3 = *data_43bfc8;
            }
        }
        else if (!arg1)
        {
            long double x87_r0_1 = 0;
            long double temp3_1 = *arg3;
            x87_r0_1 - temp3_1;
            arg1 = (x87_r0_1 < temp3_1 ? 1 : 0) << 8 | (FCMP_UO(x87_r0_1, temp3_1) ? 1 : 0) << 0xa
                | (x87_r0_1 == temp3_1 ? 1 : 0) << 0xe | 0x800;
            
            if (!(*arg1[1] & 1))
            {
                edi_1 &= 0xfffffffe;
                *arg3 = -(*data_43bfb8);
            }
            else
            {
                arg1 = data_43bfbc;
                edi_1 &= 0xfffffffe;
                *(arg3 + 4) = arg1;
                *arg3 = data_43bfb8;
            }
        }
        else
            edi_1 &= 0xfffffffe;
    }
    else if (arg2 & 2 && arg4 & 0x10)
    {
        int32_t ebp_1 = 0;
        
        if (arg2 & 0x10)
            ebp_1 = 1;
        
        arg1 = (*(arg3 + 4) & 0x7fffffff) | *arg3;
        
        if (!arg1)
            ebp_1 = 1;
        else
        {
            int32_t var_4;
            st0 = sub_419680(*arg3, *(arg3 + 4), &var_4);
            double var_c_1 = st0;
            
            if (var_4 - 0x600 >= 0xfffffbce)
            {
                long double x87_r7_2 = 0;
                long double temp4_1 = var_c_1;
                x87_r7_2 - temp4_1;
                int32_t edx_2 = 1;
                
                if (*((x87_r7_2 < temp4_1 ? 1 : 0) << 8
                        | (FCMP_UO(x87_r7_2, temp4_1) ? 1 : 0) << 0xa
                        | (x87_r7_2 == temp4_1 ? 1 : 0) << 0xe)[1] & 0x41)
                    edx_2 = 0;
                
                *var_c_1[6] &= 0xf;
                *var_c_1[6] |= 0x10;
                
                if (var_4 - 0x600 < 0xfffffc03)
                {
                    int32_t i_1 = 0xfffffc03 - (var_4 - 0x600);
                    int32_t i;
                    
                    do
                    {
                        if (var_c_1 & 1 && !ebp_1)
                            ebp_1 = 1;
                        
                        var_c_1 u>>= 1;
                        
                        if (*var_c_1[4] & 1)
                            var_c_1 |= 0x80000000;
                        
                        *var_c_1[4] u>>= 1;
                        i = i_1;
                        i_1 -= 1;
                    } while (i != 1);
                }
                
                if (edx_2)
                    var_c_1 = -(var_c_1);
            }
            else
            {
                ebp_1 = 1;
                var_c_1 = 0;
                *var_c_1[4] = 0;
            }
            
            arg1 = *var_c_1[4];
            *(arg3 + 4) = arg1;
            *arg3 = var_c_1;
        }
        
        if (ebp_1)
            arg1 = sub_419f30(x87_r0, 0x10);
        
        edi_1 &= 0xfffffffd;
    }
    
    if (arg2 & 0x10 && arg4 & 0x20)
    {
        edi_1 &= 0xffffffef;
        arg1 = sub_419f30(st0, 0x20);
    }
    
    return -((arg1 - arg1));
}

int32_t sub_419e70(int32_t arg1)
{
    if (arg1 == 1)
    {
        data_43aa58 = 0x21;
        return arg1;
    }
    
    if (arg1 >= 2)
    {
        if (arg1 > 3)
            return arg1;
        
        data_43aa58 = 0x22;
    }
    
    return arg1;
}

int32_t sub_419ea0() __pure
{
    return 0;
}

int32_t sub_419eb0() __pure
{
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    return (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
}

int32_t sub_419ed0()
{
    __fnclex();
    bool c0;
    bool c1;
    bool c2;
    bool c3;
    return (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe;
}

int32_t sub_419ef0(int16_t arg1 @ x87control, int16_t arg2, int16_t arg3)
{
    int16_t x87status;
    int16_t temp0;
    temp0 = __fnstcw_memmem16(arg1);
    int16_t x87control;
    int16_t x87status_1;
    x87control = __fldcw_memmem16((~arg3 & temp0) | (arg3 & arg2));
    return temp0;
}

void sub_419f30(long double arg1 @ st0, char arg2)
{
    int16_t top;
    bool c1;
    
    if (arg2 & 1)
    {
        data_43c0c0;
        int32_t var_c_1 = arg1;
        top = 1;
        c1 = /* c1 = unimplemented  {fistp dword [esp], st0} */;
    }
    
    if (arg2 & 8)
    {
        int32_t eax;
        bool c0;
        bool c2;
        bool c3;
        eax = (c0 ? 1 : 0) << 8 | (c1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe
            | (top & 7) << 0xb;
        /* unimplemented  {fld st0, tword [&data_43c0c0]} */
        double temp1_1 = /* double temp1_1 = unimplemented  {fstp qword [esp+0x4], st0} */;
        /* unimplemented  {fstp qword [esp+0x4], st0} */
        double var_8_1 = temp1_1;
        bool c1_1 = /* bool c1_1 = unimplemented  {fstp qword [esp+0x4], st0} */;
        eax = (c0 ? 1 : 0) << 8 | (c1_1 ? 1 : 0) << 9 | (c2 ? 1 : 0) << 0xa | (c3 ? 1 : 0) << 0xe
            | (top & 7) << 0xb;
    }
    
    if (arg2 & 0x10)
    {
        /* unimplemented  {fld st0, tword [&data_43c0d0]} */
        double var_8_2 = /* double var_8_2 = unimplemented  {fstp qword [esp+0x4], st0} */;
        /* unimplemented  {fstp qword [esp+0x4], st0} */
    }
    
    if (arg2 & 4)
    {
        /* unimplemented  {fldz } */
        /* unimplemented  {fld1 } */
        /* unimplemented  {fdivrp st1, st0} */
        /* unimplemented  {fdivrp st1, st0} */
        /* unimplemented  {fstp st0, st0} */
        /* unimplemented  {fstp st0, st0} */
    }
    
    if (!(arg2 & 0x20))
        return;
    
    /* unimplemented  {fldpi } */
    double var_8_3 = /* double var_8_3 = unimplemented  {fstp qword [esp+0x4], st0} */;
    /* unimplemented  {fstp qword [esp+0x4], st0} */
}

void* sub_419f90(int32_t* arg1, int32_t* arg2)
{
    void* ecx;
    ecx = *(arg1 + 0xa);
    int32_t* edx;
    edx = *(arg2 + 0xa);
    int32_t var_18 = 0;
    int32_t var_14 = 0;
    int32_t var_10 = 0;
    int16_t eax = (edx ^ ecx) & 0x8000;
    ecx &= 0x7fff;
    edx &= 0x7fff;
    void* eax_1 = ecx + edx;
    int16_t var_1a = eax_1;
    
    if (ecx >= 0x7fff || edx >= 0x7fff || eax_1 > 0xbffd)
    {
        arg1[1] = 0;
        *arg1 = 0;
        arg1[2] = ((eax_1 - eax_1) & 0x80000000) - 0x8000;
        return ((eax_1 - eax_1) & 0x80000000) - 0x8000;
    }
    
    if (eax_1 <= 0x3fbf)
    {
        arg1[2] = 0;
        arg1[1] = 0;
        *arg1 = 0;
        return eax_1;
    }
    
    if (!ecx)
    {
        var_1a += 1;
        
        if (!(arg1[2] & 0x7fffffff) && !arg1[1] && !*arg1)
        {
            *(arg1 + 0xa) = 0;
            return eax_1;
        }
    }
    
    if (!edx)
    {
        var_1a += 1;
        
        if (!(arg2[2] & 0x7fffffff) && !arg2[1] && !*arg2)
        {
            arg1[2] = 0;
            arg1[1] = 0;
            *arg1 = 0;
            return 0;
        }
    }
    
    int32_t var_8 = 0;
    int16_t* edi_2;
    
    for (void* i = nullptr; i < 5; i += 1)
    {
        int16_t* ebx_1 = 8;
        edi_2 = i * 2;
        void* j_1 = 5 - i;
        
        if (5 - i > 0)
        {
            void* j;
            
            do
            {
                int32_t ecx_1;
                ecx_1 = *(ebx_1 + arg2);
                int32_t eax_4;
                eax_4 = *(edi_2 + arg1);
                int32_t __saved_ebp;
                
                if (sub_418760(*(&__saved_ebp + var_8 + 0x14), ecx_1 * eax_4, 
                        &__saved_ebp + var_8 + 0x14))
                    *(&var_14 + var_8) += 1;
                
                edi_2 = &edi_2[1];
                ebx_1 -= 2;
                j = j_1;
                j_1 -= 1;
            } while (j != 1);
        }
        
        var_8 += 2;
    }
    
    int16_t var_1a_1 = var_1a - 0x3ffe;
    
    if (var_1a_1 <= 0)
    {
    label_41a11e:
        int16_t temp1_1 = var_1a_1;
        var_1a_1 -= 1;
        int32_t ebx_2;
        
        if (temp1_1 - 1 >= 0)
            ebx_2 = var_18;
        else
        {
            edi_2 = var_1a_1;
            ebx_2 = var_18;
            edi_2 = -(edi_2);
            var_1a_1 += edi_2;
            int16_t i_1;
            
            do
            {
                if (var_18 & 1)
                    ebx_2 += 1;
                
                sub_418840(&var_18);
                i_1 = edi_2;
                edi_2 -= 1;
            } while (i_1 != 1);
        }
        
        if (ebx_2)
            var_18 |= 1;
    }
    else
    {
        while (!(var_10 & 0x80000000))
        {
            sub_418800(&var_18);
            var_1a_1 -= 1;
            
            if (var_1a_1 <= 0)
                break;
        }
        
        if (var_1a_1 <= 0)
            goto label_41a11e;
    }
    
    if (var_18 > 0x8000)
    {
        if (var_18 != 0xffffffff)
            var_18 += 1;
        else
        {
            var_18 = 0;
            
            if (var_14 != 0xffffffff)
                var_14 += 1;
            else
            {
                var_14 = 0;
                
                if (*var_10[2] != 0xffff)
                    *var_10[2] += 1;
                else
                {
                    *var_10[2] = 0x8000;
                    var_1a_1 += 1;
                }
            }
        }
    }
    
    if (var_1a_1 >= 0x7fff)
    {
        arg1[1] = 0;
        *arg1 = 0;
        arg1[2] = ((0x8000 - 0x8000) & 0x80000000) - 0x8000;
        return ((0x8000 - 0x8000) & 0x80000000) - 0x8000;
    }
    
    *arg1 = *var_18[2];
    int16_t eax_10 = var_1a_1 | eax;
    *(arg1 + 2) = var_14;
    *(arg1 + 0xa) = eax_10;
    *(arg1 + 6) = var_10;
    return eax_10;
}

void sub_41a240(int16_t* arg1, int32_t arg2, int32_t arg3)
{
    int32_t esi = 0x43c080;
    int32_t i = arg2;
    
    if (!i)
        return;
    
    if (i < 0)
    {
        i = -(i);
        esi = 0x43c1e0;
    }
    
    if (!arg3)
        *arg1 = 0;
    
    while (i)
    {
        esi += 0x54;
        int32_t i_1 = i;
        i s>>= 3;
        void* eax = i_1 & 7;
        
        if (eax)
        {
            int32_t* edx_1 = esi + eax * 0xc;
            
            if (*edx_1 >= 0x8000)
            {
                int32_t var_c = *edx_1;
                int32_t var_8_1 = edx_1[1];
                int32_t var_4_1 = edx_1[2];
                edx_1 = &var_c;
                var_c -= 1;
            }
            
            sub_419f90(arg1, edx_1);
        }
    }
}

int32_t sub_41a2c0(int32_t arg1, int32_t arg2)
{
    if (data_454680 > arg1)
    {
        char* ecx_1 = *(((arg1 & 0xffffffe7) >> 3) + &data_454580) + ((arg1 & 0x1f) << 3) + 4;
        int32_t edx_1;
        edx_1 = *ecx_1;
        
        if (edx_1 & 1)
        {
            int32_t eax_3;
            eax_3 = edx_1;
            eax_3 &= 0x80;
            int32_t ebx;
            ebx = eax_3;
            
            if (arg2 != 0x8000)
            {
                if (arg2 != 0x4000)
                {
                    data_43aa58 = 0x16;
                    return 0xffffffff;
                }
                
                edx_1 |= 0x80;
            }
            else
                edx_1 &= 0x7f;
            
            *ecx_1 = edx_1;
            return ((arg2 - arg2) & 0x4000) + 0x4000;
        }
    }
    
    data_43aa58 = 9;
    return 0xffffffff;
}

void* const __convention("regparm") sub_41a340(int32_t arg1)
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

void __stdcall RtlUnwind(void* TargetFrame, void* TargetIp, EXCEPTION_RECORD* ExceptionRecord, void* ReturnValue)
{
    /* tailcall */
    return RtlUnwind(TargetFrame, TargetIp, ExceptionRecord, ReturnValue);
}

int32_t __convention("regparm") sub_41a380(int32_t arg1, int32_t arg2, char arg3, char* arg4, char* arg5)
{
    char* esi = arg5;
    char* edi = arg4;
    
    if (data_43bdd8)
    {
        int32_t result = 0xff;
        int32_t eax_1;
        
        while (true)
        {
            result = result;
            
            if (!result)
                return result;
            
            result = *esi;
            esi = &esi[1];
            int32_t ebx_1;
            ebx_1 = *edi;
            edi = &edi[1];
            
            if (result != ebx_1)
            {
                eax_1 = sub_415940(ebx_1);
                result = sub_415940(result);
                
                if (eax_1 != result)
                    break;
            }
        }
        
        bool c_6 = /* bool c_6 = unimplemented  {sbb eax, eax} */;
        return result - result + 1;
    }
    
    int16_t eax = -0x4201;
    
    while (true)
    {
        eax = eax;
        
        if (!eax)
            break;
        
        eax = *esi;
        esi = &esi[1];
        *eax[1] = *edi;
        edi = &edi[1];
        
        if (*eax[1] != eax)
        {
            eax -= 0x41;
            arg3 = (arg3 - arg3) & 0x20;
            eax += arg3;
            eax += 0x41;
            char temp0_1 = eax;
            eax = *eax[1];
            *eax[1] = temp0_1;
            eax -= 0x41;
            arg3 = (arg3 - arg3) & 0x20;
            eax += arg3;
            eax += 0x41;
            
            if (eax != *eax[1])
            {
                eax = eax - eax;
                bool c_4 = /* bool c_4 = unimplemented  {sbb al, al} */;
                eax = eax + 1;
                break;
            }
        }
    }
    
    return eax;
}

int32_t sub_456000()
{
    data_41cb2c = data_41cab8 - data_41cab0;
    int32_t eax_3 = data_41cac0 - data_41cab0;
    data_41cb30 = eax_3;
    
    if (!eax_3)
        data_41cb30 = 1;
    
    int32_t edx = data_41cb2c;
    uint32_t eax_4;
    
    if (eax_3 != edx)
        eax_4 = (COMBINE(edx, 0) / data_41cb30) >> 1;
    else
        eax_4 = 0x7fffffff;
    
    data_41cb48 = eax_4;
    data_41cb38 = data_41cab4 - data_41caac;
    int32_t eax_12;
    int32_t edx_2;
    edx_2 = HIGHD((data_41cabc - data_41caac) * 2 * data_41cb48);
    eax_12 = LOWD((data_41cabc - data_41caac) * 2 * data_41cb48);
    data_41cb3c = edx_2;
    int32_t eax_16;
    int32_t edx_3;
    edx_3 = HIGHD((data_41cad4 - data_41cac4) * 2 * data_41cb48);
    eax_16 = LOWD((data_41cad4 - data_41cac4) * 2 * data_41cb48);
    data_41cb40 = edx_3;
    int32_t eax_20;
    int32_t edx_4;
    edx_4 = HIGHD((data_41cad8 - data_41cac8) * 2 * data_41cb48);
    eax_20 = LOWD((data_41cad8 - data_41cac8) * 2 * data_41cb48);
    data_41cb44 = edx_4;
    int32_t eax_22 = data_41cb3c - data_41cb38;
    
    if (eax_22 >= 0xfffffffe && eax_22 <= 2)
        eax_22 = (eax_22 >> 2 | 1) << 2;
    
    data_41cb34 = eax_22;
    int32_t eax_27 = data_41cb40 - data_41cacc + data_41cac4;
    int32_t temp0_2 = COMBINE(eax_27 >> 0x10, eax_27 << 0x10) / data_41cb34;
    data_41cadc = temp0_2;
    int32_t eax_31 = -(temp0_2) >> 3;
    data_41caec = 0;
    data_41caf0 = eax_31;
    int32_t edx_8 = eax_31 * 2;
    data_41caf4 = edx_8;
    int32_t edx_9 = edx_8 + eax_31;
    data_41caf8 = edx_9;
    int32_t edx_10 = edx_9 + eax_31;
    data_41cafc = edx_10;
    int32_t edx_11 = edx_10 + eax_31;
    data_41cb00 = edx_11;
    int32_t edx_12 = edx_11 + eax_31;
    data_41cb04 = edx_12;
    data_41cb08 = edx_12 + eax_31;
    int32_t eax_34 = data_41cb44 - data_41cad0 + data_41cac8;
    int32_t temp0_3 = COMBINE(eax_34 >> 0x10, eax_34 << 0x10) / data_41cb34;
    data_41cae0 = temp0_3;
    int32_t result = -(temp0_3) >> 3;
    data_41cb0c = 0;
    data_41cb10 = result;
    int32_t edx_17 = result * 2;
    data_41cb14 = edx_17;
    int32_t edx_18 = edx_17 + result;
    data_41cb18 = edx_18;
    int32_t edx_19 = edx_18 + result;
    data_41cb1c = edx_19;
    int32_t edx_20 = edx_19 + result;
    data_41cb20 = edx_20;
    int32_t edx_21 = edx_20 + result;
    data_41cb24 = edx_21;
    data_41cb28 = edx_21 + result;
    return result;
}

int32_t sub_456180(int32_t* arg1 @ esi)
{
    int32_t edx_1 = arg1[1] >> 8;
    int32_t* entry_ebx;
    *entry_ebx = edx_1;
    int32_t result = (arg1[3] >> 8) - edx_1;
    entry_ebx[1] = result;
    
    if (!result)
        return result;
    
    sub_456a50(arg1);
    return sub_4567a0(data_41cbe0, data_41cbf4, entry_ebx[1], data_41cbdc);
}

int32_t sub_4561c0(int32_t* arg1 @ esi)
{
    int32_t edx_1 = arg1[1] >> 8;
    int32_t* entry_ebx;
    *entry_ebx = edx_1;
    int32_t result = (arg1[3] >> 8) - edx_1;
    entry_ebx[1] = result;
    
    if (!result)
        return result;
    
    sub_456a50(arg1);
    return sub_456760(data_41cbe0, data_41cbf4, entry_ebx[1], data_41cbdc);
}

void* sub_4561fb(int32_t* arg1 @ esi)
{
    int32_t edx_1 = arg1[1] >> 8;
    int32_t* entry_ebx;
    *entry_ebx = edx_1;
    void* result = (arg1[3] >> 8) - edx_1;
    entry_ebx[1] = result;
    
    if (result)
    {
        int32_t ecx_2 = arg1[3] - arg1[1];
        
        if (ecx_2 > 8)
        {
            int32_t eax_4;
            int32_t edx_2;
            edx_2 = HIGHD(arg1[7] - arg1[5]);
            eax_4 = LOWD(arg1[7] - arg1[5]);
            data_41cba0 = COMBINE(edx_2 << 0x10 | eax_4 >> 0xfffffff0, eax_4 << 0x10) / ecx_2;
            int32_t eax_9;
            int32_t edx_4;
            edx_4 = HIGHD(arg1[6] - arg1[4]);
            eax_9 = LOWD(arg1[6] - arg1[4]);
            data_41cb9c = COMBINE(edx_4 << 0x10 | eax_9 >> 0xfffffff0, eax_9 << 0x10) / ecx_2;
            int32_t eax_14;
            int32_t edx_6;
            edx_6 = HIGHD(arg1[2] - *arg1);
            eax_14 = LOWD(arg1[2] - *arg1);
            data_41cbac = COMBINE(edx_6 << 0x10 | eax_14 >> 0xfffffff0, eax_14 << 0x10) / ecx_2;
        }
        else
        {
            data_41cb9c = 0;
            data_41cba0 = 0;
            data_41cbac = 0;
        }
        
        int32_t edx_9 = 0x100 - arg1[1];
        data_41cb94 = ((edx_9 * data_41cb9c) >> 8) + (arg1[4] << 8);
        data_41cb98 = ((edx_9 * data_41cba0) >> 8) + (arg1[5] << 8);
        data_41cba8 = ((edx_9 * data_41cbac) >> 8) + (*arg1 << 8);
        int16_t i_1 = entry_ebx[1];
        int32_t ebx_1 = data_41cb98;
        int32_t edx_13 = data_41cba8;
        int32_t esi = data_41cb94;
        result = &entry_ebx[2];
        int16_t i;
        
        do
        {
            *(result + 4) = esi >> 0x10;
            esi += data_41cb9c;
            *result = ebx_1 >> 0x10;
            ebx_1 += data_41cba0;
            int32_t ebp_5 = data_41cbac;
            *(result + 8) = edx_13 >> 0x10;
            edx_13 += ebp_5;
            result += 0xc;
            i = i_1;
            i_1 -= 1;
        } while (i > 1);
    }
    
    return result;
}

void* sub_45631e(int32_t* arg1 @ esi)
{
    void* result;
    void** entry_ebx;
    
    if (data_453044 >= arg1[3])
    {
        entry_ebx[1] = 0;
        result = data_45304c;
        *entry_ebx = result;
    }
    else if (data_453058 > arg1[1])
    {
        int32_t eax_2 = arg1[1];
        
        if (data_453044 > eax_2)
        {
            uint32_t eax_5 = (COMBINE(data_453044 - eax_2, 0) / (arg1[3] - eax_2)) >> 1;
            int32_t eax_9;
            int32_t edx_3;
            edx_3 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_5);
            eax_9 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_5);
            arg1[5] += edx_3;
            int32_t eax_13;
            int32_t edx_4;
            edx_4 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_5);
            eax_13 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_5);
            arg1[4] += edx_4;
            int32_t eax_17;
            int32_t edx_5;
            edx_5 = HIGHD(((arg1[2] - *arg1) << 1) * eax_5);
            eax_17 = LOWD(((arg1[2] - *arg1) << 1) * eax_5);
            *arg1 += edx_5;
            arg1[1] = data_453044;
        }
        
        int32_t edx_7 = arg1[3];
        
        if (data_453058 < edx_7)
        {
            uint32_t eax_20 = (COMBINE(edx_7 - data_453058, 0) / (edx_7 - arg1[1])) >> 1;
            int32_t eax_24;
            int32_t edx_9;
            edx_9 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_20);
            eax_24 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_20);
            arg1[7] -= edx_9;
            int32_t eax_28;
            int32_t edx_10;
            edx_10 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_20);
            eax_28 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_20);
            arg1[6] -= edx_10;
            int32_t eax_32;
            int32_t edx_11;
            edx_11 = HIGHD(((arg1[2] - *arg1) << 1) * eax_20);
            eax_32 = LOWD(((arg1[2] - *arg1) << 1) * eax_20);
            arg1[2] -= edx_11;
            arg1[3] = data_453058;
        }
        
        void* edx_14 = arg1[1] >> 8;
        *entry_ebx = edx_14;
        result = (arg1[3] >> 8) - edx_14;
        entry_ebx[1] = result;
        
        if (result)
        {
            int32_t ecx_8 = arg1[3] - arg1[1];
            
            if (ecx_8 > 8)
            {
                int32_t eax_37;
                int32_t edx_15;
                edx_15 = HIGHD(arg1[7] - arg1[5]);
                eax_37 = LOWD(arg1[7] - arg1[5]);
                data_41cbc4 =
                    COMBINE(edx_15 << 0x10 | eax_37 >> 0xfffffff0, eax_37 << 0x10) / ecx_8;
                int32_t eax_42;
                int32_t edx_17;
                edx_17 = HIGHD(arg1[6] - arg1[4]);
                eax_42 = LOWD(arg1[6] - arg1[4]);
                data_41cbc0 =
                    COMBINE(edx_17 << 0x10 | eax_42 >> 0xfffffff0, eax_42 << 0x10) / ecx_8;
                int32_t eax_47;
                int32_t edx_19;
                edx_19 = HIGHD(arg1[2] - *arg1);
                eax_47 = LOWD(arg1[2] - *arg1);
                data_41cbd0 =
                    COMBINE(edx_19 << 0x10 | eax_47 >> 0xfffffff0, eax_47 << 0x10) / ecx_8;
            }
            else
            {
                data_41cbc0 = 0;
                data_41cbc4 = 0;
                data_41cbd0 = 0;
            }
            
            int32_t edx_22 = 0x100 - arg1[1];
            data_41cbb8 = ((edx_22 * data_41cbc0) >> 8) + (arg1[4] << 8);
            data_41cbbc = ((edx_22 * data_41cbc4) >> 8) + (arg1[5] << 8);
            data_41cbcc = ((edx_22 * data_41cbd0) >> 8) + (*arg1 << 8);
            int16_t i_1 = entry_ebx[1];
            int32_t ebx = data_41cbbc;
            int32_t edx_26 = data_41cbcc;
            int32_t esi = data_41cbb8;
            result = &entry_ebx[2];
            int16_t i;
            
            do
            {
                *(result + 4) = esi >> 0x10;
                esi += data_41cbc0;
                *result = ebx >> 0x10;
                ebx += data_41cbc4;
                int32_t ebp_5 = data_41cbd0;
                *(result + 8) = edx_26 >> 0x10;
                edx_26 += ebp_5;
                result += 0xc;
                i = i_1;
                i_1 -= 1;
            } while (i > 1);
        }
    }
    else
    {
        entry_ebx[1] = 0;
        result = data_453048;
        *entry_ebx = result;
    }
    return result;
}

int32_t sub_456520(int32_t* arg1 @ esi)
{
    int32_t result;
    int32_t* entry_ebx;
    
    if (data_453044 >= arg1[3])
    {
        entry_ebx[1] = 0;
        result = data_45304c;
        *entry_ebx = result;
    }
    else if (data_453058 > arg1[1])
    {
        sub_4567f0(arg1);
        data_41cbf0 = &entry_ebx[2];
        int32_t edx_2 = arg1[1] >> 8;
        *entry_ebx = edx_2;
        result = (arg1[3] >> 8) - edx_2;
        entry_ebx[1] = result;
        
        if (result)
        {
            sub_456a50(arg1);
            result = sub_4567a0(data_41cbe0, data_41cbf4, entry_ebx[1], data_41cbdc);
            data_41cbf0 = &entry_ebx[2];
        }
        
        if (data_41cc00 == 1)
        {
            *arg1 = arg1[2];
            arg1[1] = arg1[3];
            arg1[4] = arg1[6];
            arg1[5] = arg1[7];
            arg1[2] = data_41cc04;
            arg1[3] = data_41cc08;
            arg1[6] = data_41cc0c;
            arg1[7] = data_41cc10;
            sub_456a09(arg1);
            result = (arg1[3] >> 8) - (arg1[1] >> 8);
            data_41cc14 = result;
            
            if (result)
            {
                entry_ebx[1] += result;
                sub_456a50(arg1);
                data_41cbf0;
                return sub_4567a0(data_41cbe0, data_41cbf4, data_41cc14, data_41cbdc);
            }
        }
    }
    else
    {
        entry_ebx[1] = 0;
        result = data_453048;
        *entry_ebx = result;
    }
    return result;
}

int32_t sub_456640(int32_t* arg1 @ esi)
{
    int32_t result;
    int32_t* entry_ebx;
    
    if (data_453044 >= arg1[3])
    {
        entry_ebx[1] = 0;
        result = data_45304c;
        *entry_ebx = result;
    }
    else if (data_453058 > arg1[1])
    {
        sub_4567f0(arg1);
        data_41cc1c = &entry_ebx[2];
        int32_t edx_2 = arg1[1] >> 8;
        *entry_ebx = edx_2;
        result = (arg1[3] >> 8) - edx_2;
        entry_ebx[1] = result;
        
        if (result)
        {
            sub_456a50(arg1);
            result = sub_456760(data_41cbe0, data_41cbf4, entry_ebx[1], data_41cbdc);
            data_41cc1c = &entry_ebx[2];
        }
        
        if (data_41cc00 == 1)
        {
            *arg1 = arg1[2];
            arg1[1] = arg1[3];
            arg1[4] = arg1[6];
            arg1[5] = arg1[7];
            arg1[2] = data_41cc04;
            arg1[3] = data_41cc08;
            arg1[6] = data_41cc0c;
            arg1[7] = data_41cc10;
            sub_456a09(arg1);
            result = (arg1[3] >> 8) - (arg1[1] >> 8);
            data_41cc20 = result;
            
            if (result)
            {
                entry_ebx[1] += result;
                sub_456a50(arg1);
                data_41cc1c;
                return sub_456760(data_41cbe0, data_41cbf4, data_41cc20, data_41cbdc);
            }
        }
    }
    else
    {
        entry_ebx[1] = 0;
        result = data_453048;
        *entry_ebx = result;
    }
    return result;
}

int32_t __convention("regparm") sub_456760(int32_t arg1, int32_t arg2, int16_t arg3, int32_t arg4 @ esi)
{
    int16_t i;
    
    do
    {
        void* entry_ebx;
        *(entry_ebx + 4) = arg4 >> 0x10;
        arg4 += data_41cbe4;
        *entry_ebx = arg1 >> 0x10;
        arg1 += data_41cbe8;
        int32_t ebp_4 = data_41cbf8;
        *(entry_ebx + 8) = arg2 >> 0x10;
        arg2 += ebp_4;
        entry_ebx += 0xc;
        i = arg3;
        arg3 -= 1;
    } while (i > 1);
    return arg1;
}

int32_t __convention("regparm") sub_4567a0(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4 @ esi)
{
    int32_t i;
    
    do
    {
        int32_t edi_2 = arg2 >> 0xb & 0x1c;
        void* entry_ebx;
        *(entry_ebx + 4) = (*(edi_2 + &data_41caec) + arg4) >> 8;
        arg4 += data_41cbe4;
        *entry_ebx = (*(edi_2 + &data_41cb0c) + arg1) >> 8;
        arg1 += data_41cbe8;
        int32_t ebp_7 = data_41cbf8;
        *(entry_ebx + 8) = arg2 >> 0x10;
        arg2 += ebp_7;
        entry_ebx += 0xc;
        i = arg3;
        arg3 -= 1;
    } while (i > 1);
    return arg1;
}

int32_t sub_4567f0(int32_t* arg1 @ esi)
{
    int32_t result = arg1[1];
    
    if (data_453044 > result)
    {
        uint32_t eax_2 = (COMBINE(data_453044 - result, 0) / (arg1[3] - result)) >> 1;
        int32_t eax_6;
        int32_t edx_3;
        edx_3 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_2);
        eax_6 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_2);
        arg1[5] += edx_3;
        int32_t eax_10;
        int32_t edx_4;
        edx_4 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_2);
        eax_10 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_2);
        arg1[4] += edx_4;
        int32_t edx_5;
        edx_5 = HIGHD(((arg1[2] - *arg1) << 1) * eax_2);
        result = LOWD(((arg1[2] - *arg1) << 1) * eax_2);
        *arg1 += edx_5;
        arg1[1] = data_453044;
    }
    
    int32_t edx_7 = arg1[3];
    
    if (data_453058 < edx_7)
    {
        uint32_t eax_16 = (COMBINE(edx_7 - data_453058, 0) / (edx_7 - arg1[1])) >> 1;
        int32_t eax_20;
        int32_t edx_9;
        edx_9 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_16);
        eax_20 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_16);
        arg1[7] -= edx_9;
        int32_t eax_24;
        int32_t edx_10;
        edx_10 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_16);
        eax_24 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_16);
        arg1[6] -= edx_10;
        int32_t edx_11;
        edx_11 = HIGHD(((arg1[2] - *arg1) << 1) * eax_16);
        result = LOWD(((arg1[2] - *arg1) << 1) * eax_16);
        arg1[2] -= edx_11;
        arg1[3] = data_453058;
    }
    
    data_41cc00 = 0;
    
    if (data_453064 < *arg1)
    {
        if (data_453064 > arg1[2])
        {
            data_41cc00 = 1;
            data_41cc04 = arg1[2];
            data_41cc08 = arg1[3];
            data_41cc0c = arg1[6];
            data_41cc10 = arg1[7];
            int32_t eax_39 = arg1[2];
            uint32_t eax_42 = (COMBINE(data_453064 - eax_39, 0) / (*arg1 - eax_39)) >> 1;
            int32_t eax_46;
            int32_t edx_19;
            edx_19 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_42);
            eax_46 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_42);
            arg1[6] -= edx_19;
            int32_t eax_50;
            int32_t edx_20;
            edx_20 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_42);
            eax_50 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_42);
            arg1[7] -= edx_20;
            int32_t eax_54;
            int32_t edx_21;
            edx_21 = HIGHD(((arg1[3] - arg1[1]) << 1) * eax_42);
            eax_54 = LOWD(((arg1[3] - arg1[1]) << 1) * eax_42);
            arg1[3] -= edx_21;
            arg1[2] = data_453064;
        }
        else
        {
            int32_t ecx_8 = arg1[2] - data_453064;
            int32_t eax_29;
            int16_t edx_15;
            edx_15 = HIGHD(ecx_8 * data_41cadc);
            eax_29 = LOWD(ecx_8 * data_41cadc);
            eax_29 = edx_15;
            arg1[6] -= RORD(eax_29, 0x10);
            int32_t eax_32;
            int16_t edx_16;
            edx_16 = HIGHD(ecx_8 * data_41cae0);
            eax_32 = LOWD(ecx_8 * data_41cae0);
            eax_32 = edx_16;
            arg1[7] -= RORD(eax_32, 0x10);
            arg1[2] = data_453064;
        }
        
        int32_t ecx_13 = *arg1 - data_453064;
        int32_t eax_57;
        int16_t edx_22;
        edx_22 = HIGHD(ecx_13 * data_41cadc);
        eax_57 = LOWD(ecx_13 * data_41cadc);
        eax_57 = edx_22;
        arg1[4] -= RORD(eax_57, 0x10);
        int32_t eax_60;
        int16_t edx_23;
        edx_23 = HIGHD(ecx_13 * data_41cae0);
        eax_60 = LOWD(ecx_13 * data_41cae0);
        eax_60 = edx_23;
        arg1[5] -= RORD(eax_60, 0x10);
        result = data_453064;
        *arg1 = result;
    }
    
    if (data_453064 < arg1[2])
    {
        data_41cc00 = 1;
        data_41cc04 = arg1[2];
        data_41cc08 = arg1[3];
        data_41cc0c = arg1[6];
        data_41cc10 = arg1[7];
        int32_t edx_25 = arg1[2];
        int32_t edx_26 = edx_25 - data_453064;
        int32_t ecx_15 = edx_25 - *arg1;
        uint32_t eax_67;
        
        eax_67 = edx_26 != ecx_15 ? COMBINE(edx_26, 0) / ecx_15 : 0xffffffff;
        
        uint32_t eax_68 = eax_67 >> 1;
        int32_t eax_72;
        int32_t edx_28;
        edx_28 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_68);
        eax_72 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_68);
        arg1[6] -= edx_28;
        int32_t eax_76;
        int32_t edx_29;
        edx_29 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_68);
        eax_76 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_68);
        arg1[7] -= edx_29;
        int32_t eax_80;
        int32_t edx_30;
        edx_30 = HIGHD(((arg1[3] - arg1[1]) << 1) * eax_68);
        eax_80 = LOWD(((arg1[3] - arg1[1]) << 1) * eax_68);
        arg1[3] -= edx_30;
        result = data_453064;
        arg1[2] = result;
    }
    
    return result;
}

int32_t sub_456a09(void* arg1 @ esi)
{
    int32_t result = *(arg1 + 8);
    
    if (data_453064 < result)
    {
        int32_t ecx_2 = *(arg1 + 8) - data_453064;
        int32_t eax_1;
        int16_t edx_1;
        edx_1 = HIGHD(ecx_2 * data_41cadc);
        eax_1 = LOWD(ecx_2 * data_41cadc);
        eax_1 = edx_1;
        *(arg1 + 0x18) -= RORD(eax_1, 0x10);
        int32_t eax_4;
        int16_t edx_2;
        edx_2 = HIGHD(ecx_2 * data_41cae0);
        eax_4 = LOWD(ecx_2 * data_41cae0);
        eax_4 = edx_2;
        *(arg1 + 0x1c) -= RORD(eax_4, 0x10);
        result = data_453064;
        *(arg1 + 8) = result;
    }
    
    return result;
}

int32_t sub_456a50(int32_t* arg1 @ esi)
{
    int32_t ecx = arg1[3] - arg1[1];
    
    if (ecx <= 0x40)
    {
        int32_t eax_2 = arg1[2] - *arg1;
        
        if (eax_2 <= 0)
            eax_2 = -(eax_2);
        
        if (eax_2 <= 0x10000)
            ecx = 0x41;
        else if (ecx <= 3)
            ecx = 3;
    }
    
    int32_t eax_4 = arg1[7] - arg1[5];
    data_41cbe8 = COMBINE(eax_4 >> 0x1f, eax_4 << 0x10) / ecx;
    int32_t eax_8 = arg1[6] - arg1[4];
    data_41cbe4 = COMBINE(eax_8 >> 0x1f, eax_8 << 0x10) / ecx;
    int32_t eax_12 = arg1[2] - *arg1;
    data_41cbf8 = COMBINE(eax_12 >> 0x10, eax_12 << 0x10) / ecx;
    int32_t edx_6 = 0x100 - arg1[1];
    data_41cbdc = ((edx_6 * data_41cbe4) >> 8) + (arg1[4] << 8);
    data_41cbe0 = ((edx_6 * data_41cbe8) >> 8) + (arg1[5] << 8);
    int32_t result = data_41cbf8 >> 8;
    data_41cbf4 = edx_6 * result + (*arg1 << 8);
    return result;
}

int32_t sub_456b20()
{
    int32_t eax = data_43a918;
    int32_t edx_1 = data_43a920 - eax;
    int32_t ecx_1 = data_43a928 - eax;
    uint32_t eax_1;
    
    eax_1 = ecx_1 != edx_1 ? (COMBINE(edx_1, 0) / ecx_1) >> 1 : 0x7fffffff;
    
    int32_t edx_3 = data_43a914;
    int32_t eax_7;
    int32_t edx_4;
    edx_4 = HIGHD((data_43a924 - edx_3) * 2 * eax_1);
    eax_7 = LOWD((data_43a924 - edx_3) * 2 * eax_1);
    int32_t edx_5 = edx_4 - (data_43a91c - edx_3);
    
    if (edx_5 >= 0xfffffffd && edx_5 <= 3)
        edx_5 = (edx_5 >> 2 | 1) << 2;
    
    int32_t ecx_4 = data_43a930;
    int32_t eax_8 = data_43a940;
    data_43a960 = edx_5;
    int32_t eax_11;
    int32_t edx_8;
    edx_8 = HIGHD((eax_8 - ecx_4) * 2 * eax_1);
    eax_11 = LOWD((eax_8 - ecx_4) * 2 * eax_1);
    int32_t ecx_5 = data_43a92c;
    int32_t eax_12 = data_43a93c;
    data_43a9a4 = edx_8;
    int32_t eax_15;
    int32_t edx_9;
    edx_9 = HIGHD((eax_12 - ecx_5) * 2 * eax_1);
    eax_15 = LOWD((eax_12 - ecx_5) * 2 * eax_1);
    int32_t edx_11 = edx_9 - data_43a934 + ecx_5;
    int32_t temp0_2 = COMBINE(edx_11 >> 0x10, edx_11 << 0x10) / data_43a960;
    data_43a950 = temp0_2;
    int32_t eax_20 = -(temp0_2);
    int32_t eax_21 = eax_20 >> 3;
    data_43a968 = eax_21;
    int32_t edx_14 = eax_21 * 2;
    data_43a96c = edx_14;
    int32_t edx_15 = edx_14 + eax_21;
    data_43a970 = edx_15;
    int32_t edx_16 = edx_15 + eax_21;
    data_43a974 = edx_16;
    int32_t edx_17 = edx_16 + eax_21;
    data_43a978 = edx_17;
    int32_t edx_18 = edx_17 + eax_21;
    data_43a97c = edx_18;
    data_43a980 = edx_18 + eax_21;
    int32_t eax_24 = data_43a9a4 - data_43a938 + data_43a930;
    int32_t temp0_3 = COMBINE(eax_24 >> 0x10, eax_24 << 0x10) / data_43a960;
    data_43a954 = temp0_3;
    int32_t eax_27 = -(temp0_3);
    data_43a944 = RORD(eax_20, 0x10);
    int32_t ecx_8;
    ecx_8 = eax_27 >> 8;
    data_43a948 = ecx_8;
    int32_t result = eax_27 >> 3;
    data_43a988 = result;
    int32_t edx_25 = result * 2;
    data_43a98c = edx_25;
    int32_t edx_26 = edx_25 + result;
    data_43a990 = edx_26;
    int32_t edx_27 = edx_26 + result;
    data_43a994 = edx_27;
    int32_t edx_28 = edx_27 + result;
    data_43a998 = edx_28;
    int32_t edx_29 = edx_28 + result;
    data_43a99c = edx_29;
    data_43a9a0 = edx_29 + result;
    return result;
}

int32_t sub_456c70(int32_t* arg1 @ esi)
{
    int32_t edx_1 = arg1[1] >> 8;
    int32_t* entry_ebx;
    *entry_ebx = edx_1;
    int32_t result = (arg1[3] >> 8) - edx_1;
    entry_ebx[1] = result;
    
    if (!result)
        return result;
    
    sub_457210(arg1);
    return sub_456f60(data_43a9f4, data_43aa08, entry_ebx[1], data_43a9f0);
}

int32_t sub_456cb0(int32_t* arg1 @ esi)
{
    int32_t edx_1 = arg1[1] >> 8;
    int32_t* entry_ebx;
    *entry_ebx = edx_1;
    int32_t result = (arg1[3] >> 8) - edx_1;
    entry_ebx[1] = result;
    
    if (!result)
        return result;
    
    sub_457440(arg1);
    return sub_456ef0(entry_ebx[1], data_43aa08);
}

int32_t sub_456ce0(int32_t* arg1 @ esi)
{
    int32_t result;
    int32_t* entry_ebx;
    
    if (data_41cde4 >= arg1[3])
    {
        entry_ebx[1] = 0;
        result = data_41cdc4;
        *entry_ebx = result;
    }
    else if (data_41cdec > arg1[1])
    {
        sub_456fb0(arg1);
        data_43aa04 = &entry_ebx[2];
        int32_t edx_2 = arg1[1] >> 8;
        *entry_ebx = edx_2;
        result = (arg1[3] >> 8) - edx_2;
        entry_ebx[1] = result;
        
        if (result)
        {
            sub_457210(arg1);
            result = sub_456f60(data_43a9f4, data_43aa08, entry_ebx[1], data_43a9f0);
            data_43aa04 = &entry_ebx[2];
        }
        
        if (data_43aa14 == 1)
        {
            int32_t edx_4 = arg1[3];
            *arg1 = arg1[2];
            arg1[1] = edx_4;
            int32_t edx_5 = arg1[7];
            arg1[4] = arg1[6];
            arg1[5] = edx_5;
            int32_t edx_6 = data_43aa1c;
            arg1[2] = data_43aa18;
            arg1[3] = edx_6;
            int32_t edx_7 = data_43aa24;
            arg1[6] = data_43aa20;
            arg1[7] = edx_7;
            sub_4571cb(arg1);
            result = (arg1[3] >> 8) - (arg1[1] >> 8);
            data_43aa28 = result;
            
            if (result)
            {
                entry_ebx[1] += result;
                sub_457210(arg1);
                data_43aa04;
                return sub_456f60(data_43a9f4, data_43aa08, data_43aa28, data_43a9f0);
            }
        }
    }
    else
    {
        entry_ebx[1] = 0;
        result = data_41cdcc;
        *entry_ebx = result;
    }
    return result;
}

int32_t sub_456e00(int32_t* arg1 @ esi)
{
    int32_t result;
    int32_t* entry_ebx;
    
    if (data_41cde4 >= arg1[3])
    {
        entry_ebx[1] = 0;
        result = data_41cdc4;
        *entry_ebx = result;
    }
    else if (data_41cdec > arg1[1])
    {
        sub_4572e0(arg1);
        data_43aa30 = &entry_ebx[2];
        int32_t edx_2 = arg1[1] >> 8;
        *entry_ebx = edx_2;
        result = (arg1[3] >> 8) - edx_2;
        entry_ebx[1] = result;
        
        if (result)
        {
            sub_457440(arg1);
            result = sub_456f20(entry_ebx[1], data_43aa08);
            data_43aa30 = &entry_ebx[2];
        }
        
        if (data_43aa14 == 1)
        {
            int32_t edx_4 = arg1[3];
            *arg1 = arg1[2];
            arg1[1] = edx_4;
            int32_t edx_5 = data_43aa1c;
            arg1[2] = data_43aa18;
            arg1[3] = edx_5;
            sub_45741a(arg1);
            result = (arg1[3] >> 8) - (arg1[1] >> 8);
            data_43aa34 = result;
            
            if (result)
            {
                entry_ebx[1] += result;
                sub_457440(arg1);
                data_43aa30;
                return sub_456f20(data_43aa34, data_43aa08);
            }
        }
    }
    else
    {
        entry_ebx[1] = 0;
        result = data_41cdcc;
        *entry_ebx = result;
    }
    return result;
}

int32_t __fastcall sub_456ef0(int32_t arg1, int32_t arg2)
{
    int32_t ebp_1 = data_43aa0c;
    int32_t edx = arg2 >> 0x10;
    int32_t ebp = ebp_1 << 0x10;
    int32_t result = ebp_1 >> 0x10;
    int32_t edi_1 = arg2 << 0x10;
    int32_t i;
    
    do
    {
        int32_t* entry_ebx;
        *entry_ebx = edx;
        int32_t temp0_1 = edi_1;
        edi_1 += ebp;
        edx = edx + result;
        entry_ebx = &entry_ebx[1];
        i = arg1;
        arg1 -= 1;
    } while (i > 1);
    return result;
}

int32_t __fastcall sub_456f20(int32_t arg1, int32_t arg2)
{
    int32_t ebp_1 = data_43aa0c;
    int32_t edx = arg2 >> 0x10;
    int32_t ebp = ebp_1 << 0x10;
    int32_t result = ebp_1 >> 0x10;
    int32_t edi_1 = arg2 << 0x10;
    int32_t esi = data_41cdc0;
    
    while (true)
    {
        int32_t* entry_ebx;
        
        if (edx - esi < 0)
        {
            *entry_ebx = esi;
            int32_t temp0_1 = edi_1;
            edi_1 += ebp;
            edx = edx + result;
            entry_ebx = &entry_ebx[1];
            int32_t temp1_1 = arg1;
            arg1 -= 1;
            
            if (temp1_1 <= 1)
                return result;
        }
        else
        {
            *entry_ebx = edx;
            int32_t temp2_1 = edi_1;
            edi_1 += ebp;
            edx = edx + result;
            entry_ebx = &entry_ebx[1];
            int32_t temp3_1 = arg1;
            arg1 -= 1;
            
            if (temp3_1 <= 1)
                break;
        }
    }
    
    return result;
}

int32_t __convention("regparm") sub_456f60(int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4 @ esi)
{
    void* edi_1 = arg2 >> 0xb & 0x1c;
    int32_t i;
    
    do
    {
        int32_t edi_3 = *(edi_1 + 0x43a984) + arg1;
        void* entry_ebx;
        *(entry_ebx + 4) = *(edi_1 + 0x43a964) + arg4;
        int32_t ebp_3 = data_43a9f8;
        *entry_ebx = edi_3;
        arg4 += ebp_3;
        int32_t edi_4 = arg2;
        arg2 += data_43aa0c;
        int32_t ebp_5 = data_43a9fc;
        *(entry_ebx + 8) = edi_4 >> 0x10;
        arg1 += ebp_5;
        edi_1 = arg2 >> 0xb & 0x1c;
        entry_ebx += 0xc;
        i = arg3;
        arg3 -= 1;
    } while (i > 1);
    return arg1;
}

int32_t sub_456fb0(int32_t* arg1 @ esi)
{
    int32_t result = arg1[1];
    
    if (data_41cde4 > result)
    {
        uint32_t eax_2 = (COMBINE(data_41cde4 - result, 0) / (arg1[3] - result)) >> 1;
        int32_t eax_6;
        int32_t edx_3;
        edx_3 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_2);
        eax_6 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_2);
        arg1[5] += edx_3;
        int32_t eax_10;
        int32_t edx_4;
        edx_4 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_2);
        eax_10 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_2);
        arg1[4] += edx_4;
        int32_t edx_5;
        edx_5 = HIGHD(((arg1[2] - *arg1) << 1) * eax_2);
        result = LOWD(((arg1[2] - *arg1) << 1) * eax_2);
        *arg1 += edx_5;
        arg1[1] = data_41cde4;
    }
    
    int32_t edx_7 = arg1[3];
    
    if (data_41cdec < edx_7)
    {
        uint32_t eax_16 = (COMBINE(edx_7 - data_41cdec, 0) / (edx_7 - arg1[1])) >> 1;
        int32_t eax_20;
        int32_t edx_9;
        edx_9 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_16);
        eax_20 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_16);
        arg1[7] -= edx_9;
        int32_t eax_24;
        int32_t edx_10;
        edx_10 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_16);
        eax_24 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_16);
        arg1[6] -= edx_10;
        int32_t edx_11;
        edx_11 = HIGHD(((arg1[2] - *arg1) << 1) * eax_16);
        result = LOWD(((arg1[2] - *arg1) << 1) * eax_16);
        arg1[2] -= edx_11;
        arg1[3] = data_41cdec;
    }
    
    data_43aa14 = 0;
    
    if (data_41cde8 < *arg1)
    {
        if (data_41cde8 > arg1[2])
        {
            data_43aa14 = 1;
            data_43aa18 = arg1[2];
            data_43aa1c = arg1[3];
            data_43aa20 = arg1[6];
            data_43aa24 = arg1[7];
            int32_t eax_39 = arg1[2];
            uint32_t eax_42 = (COMBINE(data_41cde8 - eax_39, 0) / (*arg1 - eax_39)) >> 1;
            int32_t eax_46;
            int32_t edx_19;
            edx_19 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_42);
            eax_46 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_42);
            arg1[6] -= edx_19;
            int32_t eax_50;
            int32_t edx_20;
            edx_20 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_42);
            eax_50 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_42);
            arg1[7] -= edx_20;
            int32_t eax_54;
            int32_t edx_21;
            edx_21 = HIGHD(((arg1[3] - arg1[1]) << 1) * eax_42);
            eax_54 = LOWD(((arg1[3] - arg1[1]) << 1) * eax_42);
            arg1[3] -= edx_21;
            arg1[2] = data_41cde8;
        }
        else
        {
            int32_t ecx_8 = arg1[2] - data_41cde8;
            int32_t eax_29;
            int16_t edx_15;
            edx_15 = HIGHD(ecx_8 * data_43a950);
            eax_29 = LOWD(ecx_8 * data_43a950);
            eax_29 = edx_15;
            arg1[6] -= RORD(eax_29, 0x10);
            int32_t eax_32;
            int16_t edx_16;
            edx_16 = HIGHD(ecx_8 * data_43a954);
            eax_32 = LOWD(ecx_8 * data_43a954);
            eax_32 = edx_16;
            arg1[7] -= RORD(eax_32, 0x10);
            arg1[2] = data_41cde8;
        }
        
        int32_t ecx_13 = *arg1 - data_41cde8;
        int32_t eax_57;
        int16_t edx_22;
        edx_22 = HIGHD(ecx_13 * data_43a950);
        eax_57 = LOWD(ecx_13 * data_43a950);
        eax_57 = edx_22;
        arg1[4] -= RORD(eax_57, 0x10);
        int32_t eax_60;
        int16_t edx_23;
        edx_23 = HIGHD(ecx_13 * data_43a954);
        eax_60 = LOWD(ecx_13 * data_43a954);
        eax_60 = edx_23;
        arg1[5] -= RORD(eax_60, 0x10);
        result = data_41cde8;
        *arg1 = result;
    }
    
    if (data_41cde8 < arg1[2])
    {
        data_43aa14 = 1;
        int32_t edx_25 = arg1[3];
        data_43aa18 = arg1[2];
        data_43aa1c = edx_25;
        int32_t edx_26 = arg1[7];
        data_43aa20 = arg1[6];
        data_43aa24 = edx_26;
        int32_t edx_27 = arg1[2];
        int32_t edx_28 = edx_27 - data_41cde8;
        int32_t ecx_15 = edx_27 - *arg1;
        uint32_t eax_65;
        
        eax_65 = edx_28 != ecx_15 ? COMBINE(edx_28, 0) / ecx_15 : 0xffffffff;
        
        uint32_t eax_66 = eax_65 >> 1;
        int32_t eax_70;
        int32_t edx_30;
        edx_30 = HIGHD(((arg1[6] - arg1[4]) << 1) * eax_66);
        eax_70 = LOWD(((arg1[6] - arg1[4]) << 1) * eax_66);
        arg1[6] -= edx_30;
        int32_t eax_74;
        int32_t edx_31;
        edx_31 = HIGHD(((arg1[7] - arg1[5]) << 1) * eax_66);
        eax_74 = LOWD(((arg1[7] - arg1[5]) << 1) * eax_66);
        arg1[7] -= edx_31;
        int32_t eax_78;
        int32_t edx_32;
        edx_32 = HIGHD(((arg1[3] - arg1[1]) << 1) * eax_66);
        eax_78 = LOWD(((arg1[3] - arg1[1]) << 1) * eax_66);
        arg1[3] -= edx_32;
        result = data_41cde8;
        arg1[2] = result;
    }
    
    return result;
}

int32_t sub_4571cb(void* arg1 @ esi)
{
    int32_t result = *(arg1 + 8);
    
    if (data_41cde8 < result)
    {
        int32_t ecx_2 = *(arg1 + 8) - data_41cde8;
        int32_t eax_1;
        int16_t edx_1;
        edx_1 = HIGHD(ecx_2 * data_43a950);
        eax_1 = LOWD(ecx_2 * data_43a950);
        eax_1 = edx_1;
        *(arg1 + 0x18) -= RORD(eax_1, 0x10);
        int32_t eax_4;
        int16_t edx_2;
        edx_2 = HIGHD(ecx_2 * data_43a954);
        eax_4 = LOWD(ecx_2 * data_43a954);
        eax_4 = edx_2;
        *(arg1 + 0x1c) -= RORD(eax_4, 0x10);
        result = data_41cde8;
        *(arg1 + 8) = result;
    }
    
    return result;
}

int32_t sub_457210(int32_t* arg1 @ esi)
{
    int32_t ecx = arg1[3] - arg1[1];
    
    if (ecx <= 0x40)
    {
        int32_t eax_2 = arg1[2] - *arg1;
        
        if (eax_2 <= 0)
            eax_2 = -(eax_2);
        
        if (eax_2 <= 0x10000)
            ecx = 0x41;
        else if (ecx <= 3)
            ecx = 3;
    }
    
    int32_t eax_4 = arg1[7] - arg1[5];
    data_43a9fc = COMBINE(eax_4 >> 0x1f, eax_4 << 0x10) / ecx;
    int32_t eax_8 = arg1[6] - arg1[4];
    data_43a9f8 = COMBINE(eax_8 >> 0x1f, eax_8 << 0x10) / ecx;
    int32_t eax_12 = arg1[2] - *arg1;
    data_43aa0c = COMBINE(eax_12 >> 0x10, eax_12 << 0x10) / ecx;
    int32_t edx_10 = 0x100 - arg1[1];
    data_43a9f0 = ((edx_10 * data_43a9f8) >> 8) + (arg1[4] << 8);
    data_43a9f4 = ((edx_10 * data_43a9fc) >> 8) + (arg1[5] << 8);
    int32_t result = data_43aa0c >> 8;
    data_43aa08 = edx_10 * result + (*arg1 << 8);
    return result;
}

int32_t sub_4572e0(int32_t* arg1 @ esi)
{
    int32_t result = arg1[1];
    
    if (data_41cde4 > result)
    {
        int32_t edx_3;
        edx_3 = HIGHD(((arg1[2] - *arg1) << 1)
            * ((COMBINE(data_41cde4 - result, 0) / (arg1[3] - result)) >> 1));
        result = LOWD(((arg1[2] - *arg1) << 1)
            * ((COMBINE(data_41cde4 - result, 0) / (arg1[3] - result)) >> 1));
        *arg1 += edx_3;
        arg1[1] = data_41cde4;
    }
    
    int32_t edx_5 = arg1[3];
    
    if (data_41cdec < edx_5)
    {
        int32_t edx_7;
        edx_7 = HIGHD(((arg1[2] - *arg1) << 1)
            * ((COMBINE(edx_5 - data_41cdec, 0) / (edx_5 - arg1[1])) >> 1));
        result = LOWD(((arg1[2] - *arg1) << 1)
            * ((COMBINE(edx_5 - data_41cdec, 0) / (edx_5 - arg1[1])) >> 1));
        arg1[2] -= edx_7;
        arg1[3] = data_41cdec;
    }
    
    data_43aa14 = 0;
    
    if (data_41cde8 < *arg1)
    {
        if (data_41cde8 > arg1[2])
        {
            data_43aa14 = 1;
            int32_t eax_13 = arg1[2];
            int32_t edx_11 = arg1[3];
            data_43aa18 = eax_13;
            data_43aa1c = edx_11;
            int32_t eax_20;
            int32_t edx_15;
            edx_15 = HIGHD(((arg1[3] - arg1[1]) << 1)
                * ((COMBINE(data_41cde8 - eax_13, 0) / (*arg1 - eax_13)) >> 1));
            eax_20 = LOWD(((arg1[3] - arg1[1]) << 1)
                * ((COMBINE(data_41cde8 - eax_13, 0) / (*arg1 - eax_13)) >> 1));
            arg1[3] -= edx_15;
            arg1[2] = data_41cde8;
        }
        else
            arg1[2] = data_41cde8;
        
        result = data_41cde8;
        *arg1 = result;
    }
    
    if (data_41cde8 < arg1[2])
    {
        int32_t edx_17 = arg1[3];
        data_43aa14 = 1;
        int32_t eax_22 = arg1[2];
        data_43aa1c = edx_17;
        int32_t edx_18 = arg1[2];
        data_43aa18 = eax_22;
        int32_t ecx_11 = edx_18 - *arg1;
        int32_t edx_19 = edx_18 - data_41cde8;
        uint32_t eax_25;
        
        eax_25 = edx_19 != ecx_11 ? COMBINE(edx_19, 0) / ecx_11 : 0xffffffff;
        
        int32_t eax_30;
        int32_t edx_21;
        edx_21 = HIGHD(((arg1[3] - arg1[1]) << 1) * (eax_25 >> 1));
        eax_30 = LOWD(((arg1[3] - arg1[1]) << 1) * (eax_25 >> 1));
        arg1[3] -= edx_21;
        result = data_41cde8;
        arg1[2] = result;
    }
    
    return result;
}

int32_t sub_45741a(void* arg1 @ esi)
{
    int32_t result = *(arg1 + 8);
    
    if (data_41cde8 < result)
    {
        *(arg1 + 8);
        data_41cde8;
        result = data_41cde8;
        *(arg1 + 8) = result;
    }
    
    return result;
}

int32_t sub_457440(int32_t* arg1 @ esi)
{
    int32_t ecx_1 = arg1[3] - arg1[1];
    
    if (ecx_1 <= 0x40)
    {
        int32_t eax_1 = arg1[2] - *arg1;
        
        if (eax_1 <= 0)
            eax_1 = -(eax_1);
        
        if (eax_1 <= 0x10000)
            ecx_1 = 0x41;
        else if (ecx_1 <= 3)
            ecx_1 = 3;
    }
    
    int32_t eax_3 = arg1[2] - *arg1;
    data_43aa0c = COMBINE(eax_3 >> 0x10, eax_3 << 0x10) / ecx_1;
    int32_t result = data_43aa0c >> 8;
    data_43aa08 = (0x100 - arg1[1]) * result + (*arg1 << 8);
    return result;
}