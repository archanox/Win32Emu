// VA=0x401000
void __cdecl func_0x1000( void )
{
    data_0x9550 = &data_0x6110;
    data_0x9550+0x28 = 0;
    data_0x9550+0x1C = 4294967295;
    _atexit( &func_0x1030 );
}

// VA=0x401010
inline void __cdecl func_0x1010( void )
{
    data_0x9550 = &data_0x6110;
    data_0x9550+0x28 = 0;
    data_0x9550+0x1C = 4294967295;
}

// VA=0x401030
inline void __cdecl func_0x1030( void )
{
    func_0x14A0( &data_0x9550 );
}

// VA=0x401040
int32_t __stdcall func_0x1040( int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    uint32_t * v3;
    uint32_t * v4;
    uint32_t local_0x1C; // [esp-28]
    uint32_t local_0x18; // [esp-24]
    int32_t v1; // eax
    int32_t v2; // eax
    int v5; // eax

    data_0x957C = p1;
    v1 = func_0x1200( p4 );
    data_0x9580 = v1;
    if( v1 == 0 ) {
        return 4294967295;
    }
    v2 = func_0x1310();
    if( v2 < 0 ) {
        func_0x1420();
        MessageBoxA( data_0x9580, "Could start DirectX engine in your computer. Make sure you have at least version 7 of DirectX installed.", "Error", 48 );
        return 0;
    }
    func_0x1640( &data_0x9550, data_0x9584, 1500, 280, 4294967295 );
    func_0x14D0( &data_0x9550, data_0x957C, 101, 0, 0, 1500, 280 );
    v3 = TranslateMessage;
    v4 = DispatchMessageA;
    while( 1 ) {
        v5 = PeekMessageA( &local_0x1C, 0, 0, 0, 1 );
        if( v5 == 0 ) {
            func_0x1130();
            continue;
        }
        if( local_0x18 == 18 ) {
            break;
        }
        v3( &local_0x1C );
        v4( &local_0x1C );
    }
    func_0x1420();
    return 0;
}

// VA=0x401130
void __cdecl func_0x1130( void )
{
    uint32_t * v2;
    int32_t v1; // [esp-4]
    int32_t edi; // edi
    unsigned long v3; // eax
    int32_t v10; // eax
    unsigned long v11; // eax
    void * v4; // esp
    int32_t v7;
    int32_t v8;
    int32_t v9;
    void * v6;
    void * v5;

    v1 = edi;
    v2 = GetTickCount;
    v3 = v2();
    if( v3 - data_0x9548 > 49 ) {
        func_0x1730( &data_0x9550, data_0x958C, 245, 170, data_0x9590, data_0x9594, 150, 140 );
        v4 = &v1;
        while( 1 ) {
            v5 = (uint8_t *)v4 + (uint8_t)4294967280;
            v6 = (uint8_t *)v4 + (uint8_t)4294967284;
            *((uint8_t *)v4 + (uint8_t)4294967292) = 0;
            *((uint8_t *)v4 + (uint8_t)4294967288) = 0;
            *((uint8_t *)v4 + (uint8_t)4294967284) = data_0x9588;
            *((uint8_t *)v4 + (uint8_t)4294967280) = &code_0x117A+0xF;
            v10 = (*(*data_0x9588 + 44))( v7, v8, v9 );
            if( v10 == 0 ) {
                break;
            }
            if( v10 == -2005532222 ) {
                *((uint8_t *)v4 + (uint8_t)4294967280) = data_0x9588;
                *((uint8_t *)v4 + (uint8_t)4294967276) = &code_0x11A8;
                (*(*data_0x9588 + 108))( *((uint8_t *)v4 + (uint8_t)4294967280) );
                v6 = v5;
                break;
            }
            if( v10 == -2005532132 ) {
                (uint8_t *)v4 += (uint8_t)4294967284;
                continue;
            }
            break;
        }
        data_0x9590 += 150;
        if( data_0x9590 > 1349 ) {
            data_0x9590 = 0;
            data_0x9594 += 140;
            if( data_0x9594 > 139 ) {
                data_0x9594 = 0;
            }
        }
        *((uint8_t *)v6 + (uint8_t)4294967292) = &code_0x11E8+0x2;
        v11 = v2();
        data_0x9548 = v11;
    }
}

// VA=0x401200
int32_t __cdecl func_0x1200( int32_t p1 )
{
    struct HINSTANCE__ * hInstance;
    uint32_t * v4;
    uint32_t local_0x28; // [esp-40]
    uint32_t local_0x24; // [esp-36]
    uint32_t local_0x20; // [esp-32]
    uint32_t local_0x1C; // [esp-28]
    uint32_t local_0x18; // [esp-24]
    uint32_t local_0x14; // [esp-20]
    uint32_t local_0x10; // [esp-16]
    uint32_t local_0xC; // [esp-12]
    uint32_t local_0x8; // [esp-8]
    uint32_t local_0x4; // [esp-4]
    struct HICON__ * v1; // eax
    struct HICON__ * v2; // eax
    void * v3; // eax
    int nHeight; // eax
    int nWidth; // eax
    struct HWND__ * hWnd; // eax

    local_0x28 = 3;
    local_0x24 = &func_0x12D0;
    local_0x20 = 0;
    local_0x1C = 0;
    local_0x18 = data_0x957C;
    v1 = LoadIconA( data_0x957C, 32512 );
    local_0x14 = v1;
    v2 = LoadCursorA( 0, 32512 );
    local_0x10 = v2;
    v3 = GetStockObject( 4 );
    local_0xC = v3;
    local_0x8 = &data_0x9598;
    local_0x4 = "Basic DD";
    RegisterClassA( &local_0x28 );
    hInstance = data_0x957C;
    v4 = GetSystemMetrics;
    nHeight = v4( 1 );
    nWidth = v4( 0 );
    hWnd = CreateWindowExA( 8, "Basic DD", "Basic DD", 2147483648, 0, 0, nWidth, nHeight, 0, 0, hInstance, 0 );
    ShowWindow( hWnd, p1 );
    UpdateWindow( hWnd );
    SetFocus( hWnd );
    return hWnd;
}

// VA=0x4012d0
void __stdcall func_0x12D0( int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    if( p2 != 2 && (p2 != 256 || p3 != 27) ) {
        DefWindowProcA( p1, p2, p3, p4 );
        return;
    }
    PostQuitMessage( 0 );
}

// VA=0x401310
int32_t __cdecl func_0x1310( void )
{
    uint32_t local_0xD0; // [esp-208]
    uint32_t local_0xCC; // [esp-204]
    uint32_t local_0xC8; // [esp-200]
    uint32_t local_0xC4; // [esp-196]
    uint8_t local_0xC0[16]; // [esp-192]
    uint32_t local_0xB0; // [esp-176]
    uint32_t local_0xAC; // [esp-172]
    uint8_t local_0xA8[12]; // [esp-168]
    uint32_t local_0x9C; // [esp-156]
    uint8_t local_0x98[80]; // [esp-152]
    uint32_t local_0x48; // [esp-72]
    int32_t v1; // eax
    void * v4; // edi
    int32_t v2; // eax
    int32_t v3; // eax
    int32_t v5; // ecx
    int32_t v6; // eax
    int32_t v7; // eax

    local_0x98[8] = 0;
    local_0x98[4] = &data_0x6114;
    local_0x98[0] = &data_0x9584;
    local_0x9C = 0;
    local_0xA8[8] = &func_0x1310+0x19;
    v1 = DirectDrawCreateEx_1( 0, &data_0x9584, &data_0x6114, 0 );
    if( v1 == 0 ) {
        local_0xA8[8] = 17;
        local_0xA8[4] = data_0x9580;
        local_0xA8[0] = data_0x9584;
        local_0xAC = &code_0x1337+0x14;
        v2 = (*(*data_0x9584 + 80))( data_0x9584, data_0x9580, 17 );
        if( v2 == 0 ) {
            local_0xAC = 0;
            local_0xB0 = 0;
            local_0xC0[12] = 16;
            local_0xC0[8] = (uint8_t)480;
            local_0xC0[4] = (uint8_t)640;
            local_0xC0[0] = data_0x9584;
            local_0xC4 = &code_0x135B+0x1B;
            v3 = (*(*data_0x9584 + 84))( data_0x9584, 640, 480, 16, 0, 0 );
            if( v3 == 0 ) {
                v4 = &local_0xB0;
                v5 = 31;
                while( v5 != 0 ) {
                    *v4 = 0;
                    (uint8_t *)v4 += 4;
                    v5 += -1;
                }
                local_0xC8 = 0;
                local_0xB0 = 124;
                local_0xAC = 33;
                local_0x48 = 536;
                local_0x9C = 1;
                local_0xCC = &data_0x9588;
                local_0xD0 = &local_0xB0;
                v6 = (*(*data_0x9584 + 24))( data_0x9584, &local_0xB0 );
                if( v6 == 0 ) {
                    local_0xD0 = 0;
                    local_0xD0 = 4;
                    local_0xCC = 0;
                    local_0xC8 = 0;
                    local_0xC4 = 0;
                    v7 = (*(*data_0x9588 + 48))( data_0x9588, &local_0xD0, &data_0x958C, 4, 0, 0, 0 );
                    return -(v7 != 0);
                }
                return 4294967295;
            }
            return 4294967293;
        }
        return 4294967294;
    }
    return 4294967295;
}

// VA=0x401420
void __cdecl func_0x1420( void )
{
    int32_t v1;
    void * esp; // esp
    void * v2; // esp
    void * v4;
    void * v3;

    v1 = &func_0x1420+0xA;
    func_0x17D0( &data_0x9550 );
    v1 = data_0x958C;
    if( v1 == 0 ) {
        v1 = &func_0x1420+0xA;
        v2 = esp;
    } else {
        (*(*v1 + 8))( v1 );
        v2 = v3;
    }
    v3 = &v1;
    v4 = (uint8_t *)v2 + (uint8_t)4294967292;
    if( data_0x9588 != 0 ) {
        *((uint8_t *)v2 + (uint8_t)4294967292) = data_0x9588;
        *((uint8_t *)v2 + (uint8_t)4294967288) = &code_0x1448;
        (*(*data_0x9588 + 8))( *((uint8_t *)v2 + (uint8_t)4294967292) );
        v2 = v4;
    }
    if( data_0x9584 != 0 ) {
        *((uint8_t *)v2 + (uint8_t)4294967292) = data_0x9584;
        *((uint8_t *)v2 + (uint8_t)4294967288) = &code_0x1457;
        (*(*data_0x9584 + 8))( *((uint8_t *)v2 + (uint8_t)4294967292) );
    }
}

// VA=0x401480
int32_t __thiscall func_0x1480( void * this, int8_t p1 )
{
    func_0x14A0( this );
    if( (uint8_t)(p1 & 0x1) != 0 ) {
        func_0x18B4( this );
    }
    return this;
}

// VA=0x4014a0
void __thiscall func_0x14A0( void * this )
{
    *this = &data_0x6110;
    if( *((uint8_t *)this + 40) != 0 ) {
        OutputDebugStringA( "Surface Destroyed\n" );
        (*(**((uint8_t *)this + 40) + 8))( *((uint8_t *)this + 40) );
        *((uint8_t *)this + 40) = 0;
    }
}

// VA=0x4014d0
int32_t __thiscall func_0x14D0( void * this, int32_t p1, int32_t p2, int32_t p3, int32_t p4, int32_t p5, int32_t p6 )
{
    int32_t v4;
    int32_t v3; // [esp-180]
    int32_t v2; // [esp-176]
    void * v1; // [esp-172]
    struct HDC__ * local_0xA0; // [esp-160]
    uint32_t local_0x98; // [esp-152]
    uint32_t local_0x94; // [esp-148]
    int local_0x90; // [esp-144]
    int local_0x7C; // [esp-124]
    int local_0x78; // [esp-120]
    struct HDC__ * ebp; // ebp
    void * h; // eax
    struct HDC__ * hdcSrc; // eax
    int32_t v5; // eax
    void * v6; // esp
    void * v7;

    local_0xA0 = ebp;
    v1 = 0;
    v2 = p6;
    v3 = p5;
    v4 = 0;
    h = LoadImageA( p1, p2 & 0xFFFF, 0, p5, p6, 0 );
    if( h != 0 && *((uint8_t *)this + 40) != 0 ) {
        v1 = *((uint8_t *)this + 40);
        v2 = &code_0x151E+0x6;
        (*(**((uint8_t *)this + 40) + 108))( *((uint8_t *)this + 40) );
        v1 = 0;
        v2 = &code_0x151E+0xE;
        hdcSrc = CreateCompatibleDC( 0 );
        if( hdcSrc == 0 ) {
            return 0;
        }
        v1 = h;
        v2 = hdcSrc;
        v3 = &code_0x1536+0x8;
        SelectObject( hdcSrc, h );
        v1 = &local_0x94;
        v2 = 24;
        v3 = h;
        v4 = &code_0x1536+0x16;
        GetObjectA( h, 24, &local_0x94 );
        if( p5 != 0 ) {
            local_0x90 = p5;
        }
        local_0x7C = 124;
        local_0x78 = 6;
        v1 = &local_0x7C;
        v2 = *((uint8_t *)this + 40);
        v3 = &code_0x156A+0x1E;
        (*(**((uint8_t *)this + 40) + 88))( *((uint8_t *)this + 40), &local_0x7C );
        v1 = &local_0x98;
        v2 = *((uint8_t *)this + 40);
        v3 = &code_0x156A+0x2C;
        v5 = (*(**((uint8_t *)this + 40) + 68))( *((uint8_t *)this + 40), &local_0x98 );
        if( v5 == 0 ) {
            StretchBlt( local_0xA0, 0, 0, local_0x78, local_0x7C, hdcSrc, p1, p2, local_0x90, p4, 13369376 );
            v4 = *((uint8_t *)this + 40);
            (*(*v4 + 104))( v4, local_0xA0 );
            v6 = v7;
        } else {
            v6 = &v2;
        }
        v7 = &v4;
        *((uint8_t *)v6 + (uint8_t)4294967292) = hdcSrc;
        *((uint8_t *)v6 + (uint8_t)4294967288) = &code_0x15E0+0x7;
        DeleteDC( *((uint8_t *)v6 + (uint8_t)4294967292) );
        *((uint8_t *)this + 4) = *((uint8_t *)v6 + 172);
        *((uint8_t *)this + 8) = *((uint8_t *)v6 + 176);
        *((uint8_t *)this + 12) = *((uint8_t *)v6 + 180);
        *((uint8_t *)this + 16) = p2;
        *((uint8_t *)this + 20) = local_0x90;
        *((uint8_t *)this + 24) = *((uint8_t *)v6 + 192);
        return 1;
    }
    return 0;
}

// VA=0x401640
int32_t __thiscall func_0x1640( void * this, int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    uint32_t v13;
    int32_t v10; // [esp-180]
    int32_t v7; // [esp-168]
    int32_t v6; // [esp-164]
    void * v5; // [esp-160]
    void * v4; // [esp-156]
    int32_t v3; // [esp-152]
    uint32_t local_0x94; // [esp-148]
    uint32_t local_0x90; // [esp-144]
    uint32_t local_0x8C; // [esp-140]
    uint8_t local_0x88[12]; // [esp-136]
    uint32_t local_0x7C; // [esp-124]
    uint32_t local_0x78; // [esp-120]
    uint32_t local_0x74; // [esp-116]
    uint32_t local_0x70; // [esp-112]
    uint32_t local_0x24; // [esp-36]
    uint32_t local_0x14; // [esp-20]
    int32_t ebx; // ebx
    int32_t ebp; // ebp
    int32_t esi; // esi
    int32_t edi; // edi
    int32_t v2; // ecx
    void * v1; // edi
    int32_t v8; // eax
    void * v9; // esp
    void * v12;
    void * v11;

    local_0x88[0] = ebx;
    local_0x8C = ebp;
    local_0x90 = esi;
    local_0x94 = edi;
    v1 = &local_0x7C;
    v2 = 31;
    while( v2 != 0 ) {
        *v1 = 0;
        (uint8_t *)v1 += 4;
        v2 += -1;
    }
    v3 = 0;
    v4 = (uint8_t *)this + 40;
    v5 = &local_0x7C;
    v6 = p1;
    local_0x7C = 124;
    local_0x78 = 7;
    local_0x14 = 16448;
    local_0x70 = p2;
    local_0x74 = p3;
    v7 = &func_0x1640+0x62;
    v8 = (*(*p1 + 24))( p1, &local_0x7C, (uint8_t *)this + 40, 0, edi, esi, ebp, ebx );
    if( v8 == 0 ) {
        v9 = &v6;
    } else {
        if( v8 == -2005532292 ) {
            v10 = p1;
            local_0x24 = 2112;
            (*(*p1 + 24))( p1, &local_0x8C, (uint8_t *)this + 40, 0 );
            v9 = v11;
        } else {
            v9 = &v6;
        }
        v11 = &v10;
        if( v8 != 0 ) {
            return 0;
        }
    }
    v12 = (uint8_t *)v9 + (uint8_t)4294967284;
    v13 = *((uint8_t *)v9 + 164);
    if( v13 != 4294967295 ) {
        *((uint8_t *)v9 + 16) = v13;
        *((uint8_t *)v9 + 20) = 0;
        *((uint8_t *)v9 + (uint8_t)4294967292) = (uint8_t *)v9 + 16;
        *((uint8_t *)v9 + (uint8_t)4294967288) = 8;
        *((uint8_t *)v9 + (uint8_t)4294967284) = *((uint8_t *)this + 40);
        *((uint8_t *)v9 + (uint8_t)4294967280) = &code_0x1700;
        (*(**((uint8_t *)this + 40) + 116))( *((uint8_t *)v9 + (uint8_t)4294967284), *((uint8_t *)v9 + (uint8_t)4294967288), *((uint8_t *)v9 + (uint8_t)4294967292) );
        v9 = v12;
    }
    *((uint8_t *)this + 28) = v13;
    *((uint8_t *)this + 36) = p2;
    *((uint8_t *)this + 32) = *((uint8_t *)v9 + 160);
    return 1;
}

// VA=0x401730
int32_t __thiscall func_0x1730( void * this, int32_t p1, int32_t p2, int32_t p3, int32_t p4, int32_t p5, int32_t p6, int32_t p7 )
{
    int32_t v1; // [esp-32]
    uint32_t local_0x10; // [esp-16]
    uint32_t local_0xC; // [esp-12]
    uint32_t local_0x8; // [esp-8]
    uint32_t local_0x4; // [esp-4]
    int32_t edi; // edi
    int32_t v9; // eax
    void * v2; // esp
    int32_t v3;
    int32_t v4;
    int32_t v5;
    int32_t v6;
    int32_t v7;
    int32_t v8;

    v1 = edi;
    if( p6 == 0 ) {
        p6 = *((uint8_t *)this + 36);
    }
    if( p7 == 0 ) {
        p7 = *((uint8_t *)this + 32);
    }
    local_0x10 = p4;
    local_0xC = p5;
    local_0x8 = p4 + p6;
    local_0x4 = p5 + p7;
    v2 = &v1;
    while( 1 ) {
        if( *((uint8_t *)this + 28) < 0 ) {
            *((uint8_t *)v2 + (uint8_t)4294967292) = 0;
        } else {
            *((uint8_t *)v2 + (uint8_t)4294967292) = 1;
        }
        *((uint8_t *)v2 + (uint8_t)4294967288) = (uint8_t *)v2 + 16;
        *((uint8_t *)v2 + (uint8_t)4294967284) = *((uint8_t *)this + 40);
        *((uint8_t *)v2 + (uint8_t)4294967280) = p3;
        *((uint8_t *)v2 + (uint8_t)4294967276) = p2;
        *((uint8_t *)v2 + (uint8_t)4294967272) = p1;
        *((uint8_t *)v2 + (uint8_t)4294967268) = &code_0x1786+0xF;
        v9 = (*(*p1 + 28))( v3, v4, v5, v6, v7, v8 );
        if( v9 == 0 ) {
            break;
        }
        if( v9 == -2005532222 ) {
            *((uint8_t *)v2 + (uint8_t)4294967268) = &code_0x17A0+0x7;
            func_0x17F0( this );
            (uint8_t *)v2 += (uint8_t)4294967272;
        } else {
            if( v9 == -2005532132 ) {
                (uint8_t *)v2 += (uint8_t)4294967272;
                continue;
            }
            return 0;
        }
    }
    return 1;
}

// VA=0x4017d0
void __thiscall func_0x17D0( void * this )
{
    int32_t esi; // esi

    if( *((uint8_t *)this + 40) != 0 ) {
        (*(**((uint8_t *)this + 40) + 8))( *((uint8_t *)this + 40), esi );
        *((uint8_t *)this + 40) = 0;
    }
}

// VA=0x4017f0
void __thiscall func_0x17F0( void * this )
{
    (*(**((uint8_t *)this + 40) + 108))( *((uint8_t *)this + 40) );
}

// VA=0x401800
int32_t __cdecl DirectDrawCreateEx_1( int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    goto DirectDrawCreateEx;
}

// VA=0x401806
int32_t __cdecl func_0x1806( int32_t p1 )
{
    int32_t v2;
    int32_t v1; // eax
    int32_t v3; // eax
    int32_t v4; // eax

    v1 = func_0x1DA0( data_0x9AB0 );
    v2 = data_0x9AAC;
    if( v1 < v2 - data_0x9AB0 + 4 ) {
        v3 = func_0x1DA0( data_0x9AB0 );
        v4 = func_0x19FE( data_0x9AB0, v3 + 16 );
        if( v4 == 0 ) {
            return 0;
        }
        data_0x9AB0 = v4;
        v2 = (data_0x9AAC - data_0x9AB0 >> 2) * 4 + v4;
        data_0x9AAC = v2;
        *v2 = p1;
        data_0x9AAC += 4;
        return p1;
    }
    *v2 = p1;
    data_0x9AAC += 4;
    return p1;
}

// VA=0x401873
int32_t __cdecl _atexit( int32_t p1 )
{
    int32_t v1; // eax

    v1 = func_0x1806( p1 );
    return (v1 != 0) + -1;
}

// VA=0x401885
void __cdecl ___onexitinit( void )
{
    int32_t v1; // eax

    v1 = _malloc( 128 );
    data_0x9AB0 = v1;
    if( v1 == 0 ) {
        __amsg_exit( 24 );
        v1 = data_0x9AB0;
    }
    *v1 = 0;
    data_0x9AAC = data_0x9AB0;
}

// VA=0x4018b4
void __cdecl func_0x18B4( int32_t p1 )
{
    func_0x1EB3( p1 );
}

// VA=0x4018bf
void __cdecl func_0x18BF( void )
{
    uint32_t local_0x78; // [esp-120]
    uint32_t local_0x60; // [esp-96]
    uint8_t local_0x34; // [esp-52]
    uint16_t local_0x30; // [esp-48]
    struct _EH_EXCEPTION_REGISTRATION_RECORD ExceptionRegistration; // [esp-28]
    uint32_t edi; // edi
    void * fs; // fs
    unsigned long v1; // eax
    int32_t v2; // eax
    char * v3; // eax
    int32_t v4; // eax
    void * this; // ecx
    void * this1; // ecx
    int32_t v5; // eax
    int32_t v6; // eax
    int32_t v7; // eax
    int32_t _Code; // eax

    ExceptionRegistration.TryLevel = 4294967295;
    ExceptionRegistration.ScopeTable = &scope_table_2;
    ExceptionRegistration.Handler = &code_0x29A8;
    ExceptionRegistration.Next = *fs;
    *fs = &ExceptionRegistration.Next;
    local_0x78 = edi;
    ExceptionRegistration.SavedEsp = &local_0x78;
    v1 = GetVersion();
    data_0x95C0 = (uint8_t)v1 / 256;
    data_0x95BC = v1 & 0xFF;
    data_0x95B8 = (v1 & 0xFF) * 256 + (uint8_t)v1 / 256;
    data_0x95B4 = v1 >> 16;
    v2 = func_0x2850( 0 );
    if( v2 == 0 ) {
        _fast_error_exit( 28 );
    }
    ExceptionRegistration.TryLevel = 0;
    func_0x2530();
    v3 = GetCommandLineA();
    data_0x9AB8 = v3;
    v4 = ___crtGetEnvironmentStringsA( this );
    data_0x959C = v4;
    __setargv( this1 );
    __setenvp();
    __cinit();
    local_0x34 = 0;
    GetStartupInfoA( &local_0x60 );
    v5 = __wincmdln();
    if( (local_0x34 & 0x1) == 0 ) {
        v6 = 10;
    } else {
        v6 = local_0x30;
    }
    v7 = GetModuleHandleA( 0 );
    _Code = func_0x1040( v7, 0, v5, v6 );
    _exit( _Code );
    // Note: Program behavior is undefined if control flow reaches this location.
}

// VA=0x4019b5
void __cdecl __amsg_exit( int32_t p1 )
{
    if( data_0x95A4 == 1 ) {
        __FF_MSGBANNER();
    }
    __NMSG_WRITE( p1 );
    data_0x70C4( 255 );
}

// VA=0x4019da
void __cdecl _fast_error_exit( int32_t p1 )
{
    if( data_0x95A4 == 1 ) {
        __FF_MSGBANNER();
    }
    __NMSG_WRITE( p1 );
    ExitProcess( 255 );
}

// VA=0x4019fe
int32_t __cdecl func_0x19FE( int32_t p1, int32_t p2 )
{
    int32_t v12;
    int32_t v11;
    int32_t local_0x8; // [esp-8]
    int32_t ecx; // ecx
    int32_t v14; // eax
    int32_t v15; // eax
    int32_t v10; // eax
    int32_t v13; // eax
    unsigned long dwBytes; // esi
    int32_t v3; // esi
    int32_t v9; // eax
    int32_t v5; // eax
    int32_t v7; // eax
    int32_t v2; // eax
    int32_t v8; // eax
    int32_t v6; // eax
    int32_t v4; // eax
    int32_t v1; // eax

    local_0x8 = ecx;
    if( p1 == 0 ) {
        v2 = _malloc( p2 );
    } else {
        if( p2 == 0 ) {
            func_0x1EB3( p1 );
        } else if( data_0x9988 == 3 ) {
            do {
                if( p2 < -31 ) {
                    v10 = ___sbh_find_block( p1 );
                    if( v10 == 0 ) {
                        node_77:
                        if( p2 == 0 ) {
                            p2 = 1;
                        }
                        v11 = p2 + 15 & 0xFFFFFFF0;
                        v2 = HeapReAlloc( data_0x9984, 0, p1, p2 + 15 & 0xFFFFFFF0 );
                        p2 = v11;
                    } else {
                        if( p2 <= data_0x9980 ) {
                            v14 = ___sbh_resize_block( v10, p1, p2 );
                            if( v14 == 0 ) {
                                v2 = ___sbh_alloc_block( p2 );
                                if( v2 == 0 ) {
                                    goto node_123;
                                } else {
                                    if( *(p1 + -4) + -1 >= p2 ) {
                                        v15 = p2;
                                    } else {
                                        v15 = *(p1 + -4) + -1;
                                    }
                                    func_0x3E40( v2, p1, v15 );
                                    v10 = ___sbh_find_block( p1 );
                                    func_0x2C7F( v10, p1 );
                                }
                            } else {
                                v2 = p1;
                            }
                            if( v2 == 0 ) {
                                goto node_123;
                            }
                        } else {
                            node_123:
                            if( p2 == 0 ) {
                                p2 = 1;
                            }
                            v12 = p2 + 15 & 0xFFFFFFF0;
                            v2 = HeapAlloc( data_0x9984, 0, p2 + 15 & 0xFFFFFFF0 );
                            if( v2 == 0 ) {
                                p2 = v12;
                            } else {
                                if( *(p1 + -4) + -1 >= (p2 + 15 & 0xFFFFFFF0) ) {
                                    v13 = v12;
                                } else {
                                    v13 = *(p1 + -4) + -1;
                                }
                                func_0x3E40( v2, p1, v13 );
                                func_0x2C7F( v10, p1 );
                                p2 = v12;
                            }
                        }
                        if( v10 == 0 ) {
                            goto node_77;
                        }
                    }
                    if( v2 != 0 ) {
                        return v2;
                    }
                } else {
                    v2 = 0;
                }
                if( data_0x970C == 0 ) {
                    return v2;
                }
                v9 = __callnewh( p2 );
            } while( v9 != 0 );
        } else if( data_0x9988 != 2 ) {
            do {
                if( p2 > -32 ) {
                    v2 = 0;
                } else {
                    if( p2 == 0 ) {
                        v3 = 1;
                    } else {
                        v3 = p2;
                    }
                    p2 = v3 + 15 & 0xFFFFFFF0;
                    v2 = HeapReAlloc( data_0x9984, 0, p1, v3 + 15 & 0xFFFFFFF0 );
                    if( v2 != 0 ) {
                        return v2;
                    }
                }
                if( data_0x970C == 0 ) {
                    return v2;
                }
                v1 = __callnewh( p2 );
            } while( v1 != 0 );
        } else {
            if( p2 > -32 ) {
                dwBytes = p2;
            } else {
                if( p2 == 0 ) {
                    dwBytes = 16;
                } else {
                    dwBytes = p2 + 15 & 0xFFFFFFF0;
                }
            }
            do {
                if( dwBytes < 4294967265 ) {
                    v5 = func_0x39AF( p1, &local_0x8, &p2 );
                    if( v5 == 0 ) {
                        v2 = HeapReAlloc( data_0x9984, 0, p1, dwBytes );
                    } else if( dwBytes < data_0x922C ) {
                        v7 = func_0x3D77( local_0x8, p2, v5, dwBytes / 16 );
                        if( v7 == 0 ) {
                            v2 = func_0x3A4B( dwBytes / 16 );
                            if( v2 == 0 ) {
                                goto node_240;
                            } else {
                                if( *v5 << 4 >= dwBytes ) {
                                    v8 = dwBytes;
                                } else {
                                    v8 = *v5 << 4;
                                }
                                func_0x3E40( v2, p1, v8 );
                                func_0x3A06( local_0x8, p2, v5 );
                            }
                        } else {
                            v2 = p1;
                        }
                        if( v2 == 0 ) {
                            goto node_240;
                        } else {
                            return v2;
                        }
                    } else {
                        node_240:
                        v2 = HeapAlloc( data_0x9984, 0, dwBytes );
                        if( v2 == 0 ) {
                            goto node_145;
                        } else {
                            if( *v5 << 4 >= dwBytes ) {
                                v6 = dwBytes;
                            } else {
                                v6 = *v5 << 4;
                            }
                            func_0x3E40( v2, p1, v6 );
                            func_0x3A06( local_0x8, p2, v5 );
                        }
                    }
                    if( v2 != 0 ) {
                        return v2;
                    }
                } else {
                    v2 = 0;
                }
                node_145:
                if( data_0x970C == 0 ) {
                    return v2;
                }
                v4 = __callnewh( dwBytes );
            } while( v4 != 0 );
        }
        v2 = 0;
    }
    return v2;
}

// VA=0x401c9e
void __cdecl __cinit( void )
{
    if( data_0x9AB4 != 0 ) {
        data_0x9AB4();
    }
    __initterm( &data_0x700C, &data_0x7018 );
    __initterm( &data_0x7000, &data_0x7008 );
}

// VA=0x401ccb
noreturn void __cdecl _exit( int _Code )
{
    func_0x1CED( _Code, 0, 0 );
}

// VA=0x401cdc
void __cdecl __exit( int32_t p1 )
{
    func_0x1CED( p1, 1, 0 );
}

// VA=0x401ced
void __cdecl func_0x1CED( int32_t p1, int32_t p2, int32_t p3 )
{
    int32_t ebx; // ebx
    int32_t esi; // esi
    void * hProcess; // eax
    uint32_t v1; // esi

    if( data_0x95F0 == 1 ) {
        hProcess = GetCurrentProcess();
        TerminateProcess( hProcess, p1 );
    }
    data_0x95EC = 1;
    data_0x95E8 = p3;
    if( p2 == 0 ) {
        if( data_0x9AB0 != 0 && data_0x9AAC + 4294967292 >= data_0x9AB0 ) {
            v1 = data_0x9AAC + 4294967292;
            while( 1 ) {
                if( *v1 != 0 ) {
                    (*v1)( esi, ebx );
                }
                if( v1 + 4294967292 < data_0x9AB0 ) {
                    break;
                }
                v1 += 4294967292;
            }
        }
        __initterm( &data_0x701C, &data_0x7020 );
    }
    __initterm( &data_0x7024, &data_0x7028 );
    if( p3 == 0 ) {
        data_0x95F0 = 1;
        ExitProcess( p1 );
    }
}

// VA=0x401d86
void __cdecl __initterm( int32_t p1, int32_t p2 )
{
    int32_t esi; // esi

    while( p1 < p2 ) {
        if( *p1 != 0 ) {
            (*p1)( esi );
        }
        p1 += 4;
    }
}

// VA=0x401da0
int32_t __cdecl func_0x1DA0( int32_t p1 )
{
    uint8_t local_0xC; // [esp-12]
    uint32_t local_0x8; // [esp-8]
    uint32_t ecx; // ecx
    int32_t v3; // eax
    int32_t v1; // eax
    unsigned long v2; // eax

    local_0x8 = ecx;
    local_0xC = ecx;
    if( data_0x9988 == 3 ) {
        v3 = ___sbh_find_block( p1 );
        if( v3 != 0 ) {
            return *(p1 + -4) - 9;
        }
    } else if( data_0x9988 == 2 ) {
        v1 = func_0x39AF( p1, &local_0xC, &local_0x8 );
        if( v1 != 0 ) {
            return *v1 * 16;
        }
    }
    return HeapSize( data_0x9984, 0, p1 );
}

// VA=0x401e01
int32_t __cdecl _malloc( int32_t p1 )
{
    int32_t v1; // eax

    return __nh_malloc( p1, data_0x970C );
}

// VA=0x401e13
int32_t __cdecl __nh_malloc( int32_t p1, int32_t p2 )
{
    int32_t v1; // eax
    int32_t v2; // eax

    if( p1 < -31 ) {
        do {
            v1 = func_0x1E3F( p1 );
            if( v1 != 0 || p2 == 0 ) {
                return v1;
            }
            v2 = __callnewh( p1 );
        } while( v2 != 0 );
    }
    return 0;
}

// VA=0x401e3f
int32_t __cdecl func_0x1E3F( int32_t p1 )
{
    int32_t v2; // eax
    int32_t v1; // eax
    unsigned long dwBytes; // esi

    if( data_0x9988 == 3 ) {
        if( p1 <= data_0x9980 ) {
            v2 = ___sbh_alloc_block( p1 );
            if( v2 != 0 ) {
                return v2;
            }
        }
    } else if( data_0x9988 == 2 ) {
        if( p1 == 0 ) {
            dwBytes = 16;
        } else {
            dwBytes = p1 + 15 & 0xFFFFFFF0;
        }
        if( dwBytes <= data_0x922C ) {
            v1 = func_0x3A4B( dwBytes / 16 );
            if( v1 == 0 ) {
                goto node_47;
            } else {
                return v1;
            }
        } else {
            goto node_47;
        }
    }
    if( p1 == 0 ) {
        p1 = 1;
    }
    dwBytes = p1 + 15 & 0xFFFFFFF0;
    node_47:
    return HeapAlloc( data_0x9984, 0, dwBytes );
}

// VA=0x401eb3
void __cdecl func_0x1EB3( int32_t p1 )
{
    int32_t local_0x8; // [esp-8]
    int32_t ecx; // ecx
    int32_t v2; // eax
    int32_t v1; // eax

    local_0x8 = ecx;
    if( p1 != 0 ) {
        if( data_0x9988 == 3 ) {
            v2 = ___sbh_find_block( p1 );
            if( v2 != 0 ) {
                func_0x2C7F( v2, p1 );
                return;
            }
        } else if( data_0x9988 == 2 ) {
            v1 = func_0x39AF( p1, &local_0x8, &p1 );
            if( v1 != 0 ) {
                func_0x3A06( local_0x8, p1, v1 );
                return;
            }
        }
        HeapFree( data_0x9984, 0, p1 );
    }
}

// VA=0x401f1c
void __cdecl func_0x1F1C( int32_t p1, int32_t p2 )
{
    uint32_t v2;
    uint32_t v5;
    int32_t v1; // eax
    uint32_t v3; // esi
    uint32_t v4; // edx

    v1 = func_0x205D( p1 );
    if( v1 == 0 || *(v1 + 8) == 0 ) {
        UnhandledExceptionFilter( p2 );
    } else if( *(v1 + 8) == 5 ) {
        *(v1 + 8) = 0;
    } else if( *(v1 + 8) != 1 ) {
        v2 = data_0x95F4;
        data_0x95F4 = p2;
        if( *(v1 + 4) != 8 ) {
            *(v1 + 8) = 0;
            (*(v1 + 8))( *(v1 + 4) );
        } else {
            if( data_0x7148 < data_0x714C + data_0x7148 ) {
                v3 = data_0x7148 * 12 + &data_0x70D8;
                v4 = data_0x714C;
                while( 1 ) {
                    *v3 = 0;
                    if( v4 == 1 ) {
                        break;
                    }
                    v3 += 12;
                    v4 += 4294967295;
                }
            }
            v5 = data_0x7154;
            switch( *v1 ) {
                case 3221225614: {
                    data_0x7154 = 131;
                    break;
                }
                case 3221225616: {
                    data_0x7154 = 129;
                    break;
                }
                case 3221225617: {
                    data_0x7154 = 132;
                    break;
                }
                case 3221225619: {
                    data_0x7154 = 133;
                    break;
                }
                case 3221225613: {
                    data_0x7154 = 130;
                    break;
                }
                case 3221225615: {
                    data_0x7154 = 134;
                    break;
                }
                case 3221225618: {
                    data_0x7154 = 138;
                    break;
                }
            }
            (*(v1 + 8))( 8, data_0x7154 );
            data_0x7154 = v5;
        }
        data_0x95F4 = v2;
    }
}

// VA=0x40205d
int32_t __cdecl func_0x205D( int32_t p1 )
{
    void * v2; // eax
    int32_t v1;

    if( p1 != data_0x70D0 ) {
        v2 = &data_0x70D0;
        while( 1 ) {
            v1 = (uint8_t *)v2 + 12;
            if( (uint8_t *)v2 + 12 >= data_0x7150 * 12 + &data_0x70D0 || p1 == *((uint8_t *)v2 + 12) ) {
                break;
            }
            (uint8_t *)v2 += 12;
        }
    } else {
        v1 = &data_0x70D0;
    }
    if( v1 >= data_0x7150 * 12 + &data_0x70D0 || p1 != *v1 ) {
        v1 = 0;
    }
    return v1;
}

// VA=0x4020a0
int32_t __cdecl __wincmdln( void )
{
    int32_t v1;
    int32_t eax; // eax
    int32_t v2; // eax
    int32_t v4; // eax
    int32_t v6; // eax
    int32_t v3;
    int32_t v5;

    if( data_0x9AA8 == 0 ) {
        ___initmbctable();
    }
    v1 = data_0x9AB8;
    v2 = eax & 0xFFFFFF00 | *v1;
    if( (uint8_t)(eax & 0xFFFFFF00 | *v1) == 34 ) {
        v4 = 34;
        while( 1 ) {
            v5 = v1 + 1;
            v2 = v4 & 0xFFFFFF00 | *(v1 + 1);
            if( (uint8_t)(v4 & 0xFFFFFF00 | *(v1 + 1)) == 34 || (uint8_t)(v4 & 0xFFFFFF00 | *(v1 + 1)) == 0 ) {
                break;
            }
            v4 = __ismbblead( (uint8_t)v4 & 0xFFFFFF00 | *(v1 + 1) );
            if( v4 == 0 ) {
                v1 += 1;
            } else {
                v1 += 2;
            }
        }
        if( *(v1 + 1) != 34 ) {
            v1 = v5;
            goto node_46;
        }
    } else {
        if( (uint8_t)(eax & 0xFFFFFF00 | *v1) > 32 ) {
            while( 1 ) {
                v3 = v1 + 1;
                if( *(v1 + 1) < 33 ) {
                    break;
                }
                v1 += 1;
            }
        } else {
            goto node_46;
        }
        v1 = v3;
        goto node_46;
    }
    node_59:
    v1 = v5 + 1;
    node_46:
    v6 = v2 & 0xFFFFFF00 | *v1;
    if( (uint8_t)(v2 & 0xFFFFFF00 | *v1) != 0 && (uint8_t)(v2 & 0xFFFFFF00 | *v1) < 33 ) {
        v5 = v1;
        v2 = v6;
        goto node_59;
    }
    return v1;
}

// VA=0x4020f8
void __cdecl __setenvp( void )
{
    int32_t v2; // esi
    int32_t v1; // edi
    int32_t eax; // eax
    int32_t v4; // edi
    int32_t v3; // esi
    int32_t v6; // eax
    int32_t v7; // eax
    int32_t v5;

    if( data_0x9AA8 == 0 ) {
        ___initmbctable();
    }
    v1 = 0;
    v2 = data_0x959C;
    while( (uint8_t)(eax & 0xFFFFFF00 | *v2) != 0 ) {
        if( (uint8_t)(eax & 0xFFFFFF00 | *v2) != 61 ) {
            v1 += 1;
        }
        eax = _strlen( v2 );
        v2 = eax + v2 + 1;
    }
    v3 = _malloc( v1 * 4 + 4 );
    data_0x95D0 = v3;
    if( v3 == 0 ) {
        __amsg_exit( 9 );
    }
    if( *data_0x959C != 0 ) {
        v4 = data_0x959C;
        while( 1 ) {
            v5 = v3 + 4;
            v6 = _strlen( v4 );
            if( *v4 != 61 ) {
                v7 = _malloc( v6 + 1 );
                *v3 = v7;
                if( v7 == 0 ) {
                    __amsg_exit( 9 );
                }
                _strcpy( *v3, v4 );
                v3 = v5;
            }
            if( *(v4 + v6 + 1) == 0 ) {
                break;
            }
            v4 += v6 + 1;
        }
    }
    func_0x1EB3( data_0x959C );
    data_0x959C = 0;
    *v3 = 0;
    data_0x9AA4 = 1;
}

// VA=0x4021b1
void __thiscall __setargv( void * this )
{
    uint32_t local_0xC; // [esp-12]
    uint32_t local_0x8; // [esp-8]
    int32_t v1; // edi
    int32_t v2; // eax

    local_0x8 = this;
    local_0xC = this;
    if( data_0x9AA8 == 0 ) {
        ___initmbctable();
    }
    GetModuleFileNameA( 0, &data_0x95F8, 260 );
    data_0x95E0 = &data_0x95F8;
    if( *data_0x9AB8 == 0 ) {
        v1 = &data_0x95F8;
    } else {
        v1 = data_0x9AB8;
    }
    _parse_cmdline( v1, 0, 0, &local_0x8, &local_0xC );
    v2 = _malloc( local_0x8 * 4 + local_0xC );
    if( v2 == 0 ) {
        __amsg_exit( 8 );
    }
    _parse_cmdline( v1, v2, local_0x8 * 4 + v2, &local_0x8, &local_0xC );
    data_0x95C8 = v2;
    data_0x95C4 = local_0x8 + 4294967295;
}

// VA=0x40224a
// Decompilation timed out after 15 seconds.

// VA=0x4023fe
int32_t __thiscall ___crtGetEnvironmentStringsA( void * this )
{
    uint32_t * v1;
    uint32_t * v4;
    unsigned short * lpWideCharStr; // eax
    unsigned short * v3; // eax
    int cbMultiByte; // eax
    int v5; // eax
    char * penv; // eax
    char * v7; // eax
    int32_t lpMultiByteStr; // eax
    char * v6;
    unsigned short * v2;

    v1 = GetEnvironmentStringsW;
    switch( data_0x96FC ) {
        case 0: {
            lpWideCharStr = v1();
            if( lpWideCharStr == 0 ) {
                penv = GetEnvironmentStrings();
                if( penv == 0 ) {
                    goto node_46;
                } else {
                    data_0x96FC = 2;
                    goto node_60;
                }
            } else {
                data_0x96FC = 1;
            }
            break;
        }
        case 1: {
            lpWideCharStr = 0;
            break;
        }
        case 2: {
            penv = 0;
            goto node_60;
            break;
        }
        default: {
            goto node_46;
            break;
        }
    }
    if( lpWideCharStr == 0 ) {
        lpWideCharStr = v1();
        if( lpWideCharStr == 0 ) {
            goto node_46;
        } else {
            goto node_54;
        }
    } else {
        node_54:
        if( lpWideCharStr[0] == 0 ) {
            v2 = lpWideCharStr;
        } else {
            v3 = lpWideCharStr;
            while( 1 ) {
                v2 = &v3[2];
                if( v3[1] != 0 ) {
                    v3 = &v3[1];
                    continue;
                }
                if( v3[2] == 0 ) {
                    goto node_74;
                } else {
                    v3 = &v3[2];
                }
            }
        }
        node_74:
        v4 = WideCharToMultiByte;
        cbMultiByte = v4( 0, 0, lpWideCharStr, (v2 - lpWideCharStr >> 1) + 1, 0, 0, 0, 0 );
        if( cbMultiByte == 0 ) {
            lpMultiByteStr = 0;
        } else {
            lpMultiByteStr = _malloc( cbMultiByte );
            if( lpMultiByteStr == 0 ) {
                lpMultiByteStr = 0;
            } else {
                v5 = v4( 0, 0, lpWideCharStr, (v2 - lpWideCharStr >> 1) + 1, lpMultiByteStr, cbMultiByte, 0, 0 );
                if( v5 == 0 ) {
                    func_0x1EB3( lpMultiByteStr );
                    lpMultiByteStr = 0;
                }
            }
        }
        FreeEnvironmentStringsW( lpWideCharStr );
        return lpMultiByteStr;
    }
    node_60:
    if( penv == 0 ) {
        penv = GetEnvironmentStrings();
        if( penv != 0 ) {
            goto node_88;
        }
    } else {
        node_88:
        if( penv[0] == 0 ) {
            v6 = penv;
        } else {
            v7 = penv;
            while( 1 ) {
                v6 = &v7[2];
                if( v7[1] != 0 ) {
                    v7 = &v7[1];
                    continue;
                }
                if( v7[2] == 0 ) {
                    goto node_119;
                } else {
                    v7 = &v7[2];
                }
            }
        }
        node_119:
        lpMultiByteStr = _malloc( v6 - penv + 1 );
        if( lpMultiByteStr == 0 ) {
            lpMultiByteStr = 0;
        } else {
            func_0x3E40( lpMultiByteStr, penv, v6 - penv + 1 );
        }
        FreeEnvironmentStringsA( penv );
        return lpMultiByteStr;
    }
    node_46:
    return 0;
}

// VA=0x402530
void __cdecl func_0x2530( void )
{
    unsigned long nStdHandle;
    uint32_t local_0x44; // [esp-68]
    uint16_t local_0x12; // [esp-18]
    uint32_t local_0x10; // [esp-16]
    int32_t v1; // eax
    void * v9; // ebx
    void * v8; // ebp
    void * v4; // edi
    uint32_t v2; // eax
    int32_t v12; // ebx
    int32_t v7; // edi
    uint32_t v3; // esi
    int32_t v5; // eax
    unsigned long v11; // eax
    uint32_t v10; // ecx
    uint32_t v6; // ecx
    void * hFile; // eax
    unsigned long v13; // eax

    v1 = _malloc( 256 );
    if( v1 == 0 ) {
        __amsg_exit( 27 );
    }
    data_0x99A0 = v1;
    data_0x9AA0 = 32;
    v2 = v1 + 256;
    while( v1 < v2 ) {
        *(v1 + 4) = 0;
        *v1 = -1;
        *(v1 + 5) = 10;
        v1 += 8;
        v2 = data_0x99A0 + 256;
    }
    GetStartupInfoA( &local_0x44 );
    if( local_0x12 != 0 && local_0x10 != 0 ) {
        if( *local_0x10 > 2047 ) {
            v3 = 2048;
        } else {
            v3 = *local_0x10;
        }
        if( data_0x9AA0 < v3 ) {
            v4 = &data_0x99A4;
            while( 1 ) {
                v5 = _malloc( 256 );
                if( v5 == 0 ) {
                    break;
                }
                data_0x9AA0 += 32;
                *v4 = v5;
                v6 = v5 + 256;
                while( v5 < v6 ) {
                    *(v5 + 4) = 0;
                    *v5 = -1;
                    *(v5 + 5) = 10;
                    v6 = *v4 + 256;
                    v5 += 8;
                }
                if( data_0x9AA0 >= v3 ) {
                    goto node_64;
                } else {
                    (uint8_t *)v4 += 4;
                }
            }
            v3 = data_0x9AA0;
            v6 = 256;
        }
        node_64:
        if( v3 > 0 ) {
            v7 = 0;
            v8 = local_0x10 + 4;
            v9 = local_0x10 + 4 + *local_0x10;
            while( 1 ) {
                if( *v9 != 4294967295 ) {
                    v10 = v6 & 0xFFFFFF00 | *v8;
                    if( (uint8_t)((v6 & 0xFFFFFF00 | *v8) & 0x1) == 0 ) {
                        v6 = v10;
                    } else if( (uint8_t)((v6 & 0xFFFFFF00 | *v8) & 0x8) == 0 ) {
                        v11 = GetFileType( *v9 );
                        if( v11 != 0 ) {
                            goto node_165;
                        }
                    } else {
                        node_165:
                        *(*(&data_0x99A0 + (v7 >> 5) * 4) + (v7 & 0x1F) * 8) = *v9;
                        v6 = *v9 & 0xFFFFFF00 | *v8;
                        *(*(&data_0x99A0 + (v7 >> 5) * 4) + (v7 & 0x1F) * 8 + 4) = v6;
                    }
                }
                if( v7 + 1 >= v3 ) {
                    break;
                }
                v7 += 1;
                (uint8_t *)v8 += 1;
                (uint8_t *)v9 += 4;
            }
        }
    }
    v12 = 0;
    while( 1 ) {
        if( *(data_0x99A0 + v12 * 8) == 4294967295 ) {
            *(data_0x99A0 + v12 * 8 + 4) = 129;
            if( v12 == 0 ) {
                nStdHandle = 4294967286;
            } else {
                nStdHandle = -(v12 != 1) + -11;
            }
            hFile = GetStdHandle( nStdHandle );
            if( hFile == 4294967295 ) {
                node_127:
                *(data_0x99A0 + v12 * 8 + 4) |= 0x40;
            } else {
                v13 = GetFileType( hFile );
                if( v13 == 0 ) {
                    goto node_127;
                } else {
                    *(data_0x99A0 + v12 * 8) = hFile;
                    if( (v13 & 0xFF) == 2 ) {
                        goto node_127;
                    } else if( (v13 & 0xFF) == 3 ) {
                        *(data_0x99A0 + v12 * 8 + 4) |= 0x8;
                    }
                }
            }
        } else {
            *(data_0x99A0 + v12 * 8 + 4) |= 0x80;
        }
        if( v12 > 1 ) {
            break;
        }
        v12 += 1;
    }
    SetHandleCount( data_0x9AA0 );
}

// VA=0x4026db
int32_t __cdecl func_0x26DB( int32_t p1 )
{
    struct HINSTANCE__ * v1; // eax

    *p1 = 0;
    v1 = GetModuleHandleA( 0 );
    if( v1->unused == 23117 && v1[15].unused != 0 ) {
        *p1 = v1[15].unused & 0xFFFFFF00 | *(v1 + v1[15].unused + 26);
        v1 = v1 + v1[15].unused & 0xFFFFFF00 | *(v1 + v1[15].unused + 27);
        *(p1 + 1) = v1;
    }
    return v1;
}

// VA=0x402708
int32_t __cdecl func_0x2708( void )
{
    uint8_t local_0x1230; // [esp-4656]
    uint8_t local_0x1A0; // [esp-416]
    uint32_t local_0x9C; // [esp-156]
    uint32_t local_0x98; // [esp-152]
    uint32_t local_0x8C; // [esp-140]
    uint8_t local_0x8; // [esp-8]
    int v1; // eax
    void * v4; // ecx
    unsigned long v5; // eax
    void * v8; // ecx
    unsigned long v2; // eax
    int32_t v6; // eax
    void * v12; // ecx
    unsigned long v9; // eax
    unsigned long v7; // eax
    int32_t v10; // eax
    int32_t v11; // eax
    int32_t v3; // eax
    void * v13;

    local_0x8 = &func_0x2708+0xD;
    local_0x9C = 148;
    v1 = GetVersionExA( &local_0x9C );
    if( v1 != 0 && local_0x8C == 2 && local_0x98 > 4 ) {
        v3 = 1;
    } else {
        v2 = GetEnvironmentVariableA( "__MSVCRT_HEAP_SELECT", &local_0x1230, 4240 );
        if( v2 == 0 ) {
            func_0x26DB( &local_0x8 );
            v3 = -(local_0x8 < 6) + 3;
        } else {
            if( local_0x1230 != 0 ) {
                v4 = &local_0x1230;
                while( 1 ) {
                    v5 = v2 & 0xFFFFFF00 | *v4;
                    if( (int8_t)(v2 & 0xFFFFFF00 | *v4) > 96 && (int8_t)(v2 & 0xFFFFFF00 | *v4) < 123 ) {
                        v2 = (v2 & 0xFFFFFF00 | *v4) & 0xFFFFFF00 | (uint8_t)(v2 & 0xFFFFFF00 | *v4) - 32;
                        *v4 = v2;
                    } else {
                        v2 = v5;
                    }
                    if( *((uint8_t *)v4 + 1) == 0 ) {
                        break;
                    }
                    (uint8_t *)v4 += 1;
                }
            }
            v6 = _strncmp( "__GLOBAL_HEAP_SELECTED", &local_0x1230, 22 );
            if( v6 == 0 ) {
                v10 = &local_0x1230;
            } else {
                v7 = GetModuleFileNameA( 0, &local_0x1A0, 260 );
                if( local_0x1A0 != 0 ) {
                    v8 = &local_0x1A0;
                    while( 1 ) {
                        v9 = v7 & 0xFFFFFF00 | *v8;
                        if( (int8_t)(v7 & 0xFFFFFF00 | *v8) > 96 && (int8_t)(v7 & 0xFFFFFF00 | *v8) < 123 ) {
                            v7 = (v7 & 0xFFFFFF00 | *v8) & 0xFFFFFF00 | (uint8_t)(v7 & 0xFFFFFF00 | *v8) - 32;
                            *v8 = v7;
                        } else {
                            v7 = v9;
                        }
                        if( *((uint8_t *)v8 + 1) == 0 ) {
                            break;
                        }
                        (uint8_t *)v8 += 1;
                    }
                }
                v10 = func_0x4A00( &local_0x1230, &local_0x1A0 );
            }
            if( v10 == 0 ) {
                func_0x26DB( &local_0x8 );
                return -(local_0x8 < 6) + 3;
            }
            v11 = _strchr( v10, 44 );
            if( v11 == 0 ) {
                func_0x26DB( &local_0x8 );
                return -(local_0x8 < 6) + 3;
            }
            if( *(v11 + 1) != 0 ) {
                v12 = v11 + 1;
                do {
                    v13 = (uint8_t *)v12 + 1;
                    if( *v12 == 59 ) {
                        *v12 = 0;
                    } else {
                        v12 = v13;
                    }
                } while( *v12 != 0 );
            }
            v3 = func_0x470B( v11 + 1, 0, 10 );
            if( v3 != 2 && v3 != 3 && v3 != 1 ) {
                func_0x26DB( &local_0x8 );
                return -(local_0x8 < 6) + 3;
            }
        }
    }
    return v3;
}

// VA=0x402850
int32_t __cdecl func_0x2850( int32_t p1 )
{
    void * v1; // eax
    int32_t v2; // eax
    int32_t v3; // eax

    v1 = HeapCreate( p1 == 0, 4096, 0 );
    data_0x9984 = v1;
    if( v1 == 0 ) {
        return 0;
    }
    v2 = func_0x2708();
    data_0x9988 = v2;
    switch( v2 ) {
        case 3: {
            func_0x2C0C( 1016 );
            break;
        }
        case 2: {
            v3 = func_0x3753();
            break;
        }
        default: {
            return 1;
        }
    }
    if( v3 == 0 ) {
        HeapDestroy( data_0x9984 );
        return 0;
    }
    return 1;
}

// VA=0x4028b0
void __cdecl func_0x28B0( int32_t p1 )
{
    RtlUnwind_1( p1, &code_0x28C8, 0, 0 );
}

// VA=0x4028d0
int32_t __cdecl __unwind_handler( int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    int32_t v1; // eax

    if( (*(p1 + 4) & 0x6) == 0 ) {
        v1 = 1;
    } else {
        *p4 = p2;
        v1 = 3;
    }
    return v1;
}

// VA=0x4028f2
void __cdecl __local_unwind2( int32_t p1, int32_t p2 )
{
    uint32_t v2;
    uint32_t v4;
    uint32_t v3; // [esp-28]
    int32_t v1; // [esp-24]
    uint32_t local_0x14; // [esp-20]
    uint8_t local_0x10[16]; // [esp-16]
    uint32_t ebp; // ebp
    void * fs; // fs

    local_0x10[0] = p1;
    local_0x14 = 4294967294;
    v1 = &__unwind_handler;
    v2 = *fs;
    v3 = v2;
    *fs = &v3;
    while( *(p1 + 12) != -1 && p2 != *(p1 + 12) ) {
        *(p1 + 12) = *(*(p1 + 8) + *(p1 + 12) * 12);
        v4 = *(*(p1 + 8) + *(p1 + 12) * 12 + 4);
        if( v4 == 0 ) {
            data_0x7164+0x8 = *(ebp + 8);
            data_0x7164+0x4 = *(*(p1 + 8) + *(p1 + 12) * 12 + 8);
            data_0x7164+0xC = ebp;
            (*(*(p1 + 8) + *(p1 + 12) * 12 + 8))();
        }
    }
    *fs = v2;
}

// VA=0x40295a
int32_t __cdecl __abnormal_termination( void )
{
    uint32_t v2;
    void * fs; // fs
    int32_t v1; // eax

    if( &__unwind_handler == *(*fs + 4) ) {
        v2 = *(*(*fs + 12) + 12);
        if( v2 == *(*fs + 8) ) {
            v1 = 1;
        } else {
            v1 = 0;
        }
    } else {
        v1 = 0;
    }
    return v1;
}

// VA=0x40297d
inline void __thiscall __NLG_Notify1( void * this, int32_t p1 )
{
    uint32_t eax; // eax
    uint32_t ebp; // ebp

    data_0x7164+0x8 = this;
    data_0x7164+0x4 = eax;
    data_0x7164+0xC = ebp;
}

// VA=0x402986
inline void __thiscall __NLG_Notify( void * this, int32_t p1 )
{
    uint32_t eax; // eax
    uint32_t ebp; // ebp

    data_0x7164+0x8 = *(ebp + 8);
    data_0x7164+0x4 = eax;
    data_0x7164+0xC = ebp;
}

// VA=0x4029a0
void __cdecl func_0x29A0( void )
{
    uint8_t v1;
    int32_t v2;
    uint32_t v7;
    uint32_t * v3;
    int32_t v4;
    uint32_t stack_0x4; // [esp+4]
    uint32_t stack_0xC; // [esp+12]
    uint32_t local_0xC; // [esp-12]
    uint32_t local_0x8; // [esp-8]
    uint32_t local_0x4; // [esp-4]
    void * eax; // eax
    int32_t edx; // edx
    uint32_t ebp; // ebp
    uint32_t esi; // esi
    int32_t v6; // eax
    int32_t v5; // ebp

    local_0x4 = esi;
    v1 = *esi ^ (uint16_t)(edx & 0xFFFF00FF | (uint8_t)((uint16_t)edx >> 8 ^ *eax) << 8) >> 8;
    *esi = v1;
    local_0x4 = ebp;
    if( (*(stack_0x4 + 4) & 0x6) == 0 ) {
        local_0xC = stack_0x4;
        local_0x8 = stack_0xC;
        *(v2 + -4) = &local_0xC;
        v3 = *(v2 + 8);
        v4 = *(v2 + 12);
        v5 = &local_0x4;
        while( v4 != -1 ) {
            if( v3[v4 * 3 + 1] == 0 ) {
                node_37:
                v3 = *(v2 + 8);
                v4 = v3[v4 * 3];
                continue;
            }
            v6 = v3[v4 * 3 + 1]( v5, v4, &local_0x4 );
            v2 = *(v5 + 12);
            if( v6 == 0 ) {
                goto node_37;
            } else if( v6 >= 0 ) {
                v7 = *(v2 + 8);
                func_0x28B0( v2 );
                __local_unwind2( v2, v4 );
                data_0x7164+0x8 = *(v2 + 24);
                data_0x7164+0x4 = *(v7 + v4 * 12 + 8);
                data_0x7164+0xC = v2 + 16;
                *(v2 + 12) = *(v7 + v4 * 12);
                (*(v7 + v4 * 12 + 8))();
                v5 = v2 + 16;
                goto node_37;
            } else {
                return;
            }
        }
    } else {
        __local_unwind2( v2, 4294967295 );
    }
}

// VA=0x402a65
uint32_t __stdcall _seh_longjmp_unwind( uint32_t p1 )
{
    uint32_t v1; // eax

    __local_unwind2( *(p1 + 24), *(p1 + 28) );
    return v1;
}

// VA=0x402a80
void __cdecl __FF_MSGBANNER( void )
{
    if( data_0x95A4 == 1 || data_0x95A4 == 0 && data_0x70C8 == 1 ) {
        __NMSG_WRITE( 252 );
        if( data_0x9700 != 0 ) {
            data_0x9700();
        }
        __NMSG_WRITE( 255 );
    }
}

// VA=0x402ab9
void __cdecl __NMSG_WRITE( int32_t p1 )
{
    int32_t v3;
    void * lpBuffer;
    uint32_t local_0x1A8; // [esp-424]
    uint32_t local_0xA4; // [esp-164]
    void * v2; // eax
    int32_t v1; // ecx
    int32_t v6; // edi
    unsigned long v4; // eax
    int32_t v5; // eax
    int32_t v7; // eax
    unsigned long nNumberOfBytesToWrite; // eax
    void * hFile; // eax

    v1 = 0;
    v2 = &data_0x7178;
    while( *v2 != p1 ) {
        v3 = v1 + 1;
        if( v2 > &data_0x717C+0x83 ) {
            v1 = v3;
            break;
        }
        v1 += 1;
        (uint8_t *)v2 += 8;
    }
    if( *(&data_0x7178 + v1 * 8) == p1 ) {
        if( data_0x95A4 == 1 || data_0x95A4 == 0 && data_0x70C8 == 1 ) {
            nNumberOfBytesToWrite = _strlen( *(&data_0x717C + v1 * 8) );
            lpBuffer = *(&data_0x717C + v1 * 8);
            hFile = GetStdHandle( 4294967284 );
            WriteFile( hFile, lpBuffer, nNumberOfBytesToWrite, &p1, 0 );
        } else if( p1 != 252 ) {
            v4 = GetModuleFileNameA( 0, &local_0x1A8, 260 );
            if( v4 == 0 ) {
                _strcpy( &local_0x1A8, "<program name unknown>" );
            }
            v5 = _strlen( &local_0x1A8 );
            if( v5 > 59 ) {
                v7 = _strlen( &local_0x1A8 );
                _strncpy( v7 + &local_0x1A8 - 59, "...", 3 );
                v6 = v7 + &local_0x1A8 - 59;
            } else {
                v6 = &local_0x1A8;
            }
            _strcpy( &local_0xA4, "Runtime Error!\n\nProgram: " );
            _strcat( &local_0xA4, v6 );
            _strcat( &local_0xA4, &data_0x6418 );
            _strcat( &local_0xA4, *(&data_0x717C + v1 * 8) );
            ___crtMessageBoxA( &local_0xA4, "Microsoft Visual C++ Runtime Library", 73744 );
        }
    }
}

// VA=0x402c0c
void __cdecl func_0x2C0C( int32_t p1 )
{
    void * v1; // eax

    v1 = HeapAlloc( data_0x9984, 0, 320 );
    data_0x997C = v1;
    if( v1 == 0 ) {
        return;
    }
    data_0x9974 = 0;
    data_0x9978 = 0;
    data_0x9970 = v1;
    data_0x9980 = p1;
    data_0x9968 = 16;
}

// VA=0x402c54
int32_t __cdecl ___sbh_find_block( int32_t p1 )
{
    int32_t v1; // eax

    v1 = data_0x997C;
    while( v1 < data_0x997C + data_0x9978 * 20 ) {
        if( p1 - *(v1 + 12) < 1048576 ) {
            return v1;
        }
        v1 += 20;
    }
    return 0;
}

// VA=0x402c7f
void __cdecl func_0x2C7F( int32_t p1, int32_t p2 )
{
    uint32_t v13;
    uint32_t v5;
    uint32_t v8;
    uint32_t v7;
    int32_t v3;
    int32_t v9;
    uint32_t v12;
    int32_t v15;
    int32_t v14;
    int32_t v18;
    int32_t v17;
    uint32_t v19;
    uint32_t * v20;
    uint8_t v21;
    uint32_t v6; // edx
    uint8_t v2; // cc_dst
    uint32_t v10; // esi
    int32_t v4; // ecx
    int32_t v11; // ebx
    int32_t v16; // ecx
    int32_t v1;

    v1 = (p2 - *(p1 + 12)) / 32768 * 516 + 324;
    if( (uint8_t)(*(p2 + -4) + -1 & 0x1) == 0 ) {
        v2 = (uint8_t)*(p2 + *(p2 + -4) + -5) & 0x1;
        if( v2 == 0 ) {
            v5 = *(p2 + *(p2 + -4) + -5) >> 4;
            if( v5 > 64 ) {
                v6 = 63;
            } else {
                v6 = v5 + 4294967295;
            }
            if( *(*(p2 + -4) + p2 + 3) == *(*(p2 + -4) + p2 + -1) ) {
                if( v6 < 32 ) {
                    v8 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) & ~(2147483648 >> (v6 & 0x1F));
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) = v8;
                    *(*(p1 + 16) + v6 + 4) += -1;
                    if( *(*(p1 + 16) + v6 + 4) == 1 ) {
                        *p1 &= ~(2147483648 >> (v6 & 0x1F));
                    }
                } else {
                    v7 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) & ~(2147483648 >> (v6 + 4294967264 & 0x1F));
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) = v7;
                    *(*(p1 + 16) + v6 + 4) += -1;
                    if( *(*(p1 + 16) + v6 + 4) == 1 ) {
                        *(p1 + 4) &= ~(2147483648 >> (v6 + 4294967264 & 0x1F));
                    }
                }
            }
            v3 = *(p2 + -4) + -1 + *(p2 + *(p2 + -4) + -5);
            *(*(p2 + *(p2 + -4) + 3) + 4) = *(p2 + *(p2 + -4) + -1);
            *(*(p2 + *(p2 + -4) + -1) + 8) = *(p2 + *(p2 + -4) + 3);
            v4 = v3;
        } else {
            v3 = *(p2 + -4) + -1;
            v4 = *(p2 + -4) + -1;
        }
        v9 = (v4 >> 4) + -1;
        if( v4 >> 4 > 64 ) {
            v9 = 63;
        }
        if( (*(p2 + -8) & 0x1) != 0 ) {
            v10 = p2 + -4;
            v11 = p1;
        } else {
            if( *(p2 + -8) >> 4 > 64 ) {
                v11 = 63;
            } else {
                v11 = (*(p2 + -8) >> 4) + -1;
            }
            v3 = v4 + *(p2 + -8);
            v9 = (v4 + *(p2 + -8) >> 4) + -1;
            if( v4 + *(p2 + -8) >> 4 > 64 ) {
                v9 = 63;
            }
            if( v9 != v11 ) {
                v12 = *(p2 + -4 - *(p2 + -8) + 4);
                v13 = *(p2 + -4 - *(p2 + -8) + 8);
                if( v13 == v12 ) {
                    if( v11 < 32 ) {
                        v15 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) & ~(-2147483648 >> (v11 & 0x1F));
                        *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) = v15;
                        *(*(p1 + 16) + v11 + 4) += -1;
                        if( *(*(p1 + 16) + v11 + 4) == 1 ) {
                            *p1 &= ~(-2147483648 >> (v11 & 0x1F));
                        }
                    } else {
                        v14 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) & ~(-2147483648 >> (v11 + -32 & 0x1F));
                        *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) = v14;
                        *(*(p1 + 16) + v11 + 4) += -1;
                        if( *(*(p1 + 16) + v11 + 4) == 1 ) {
                            *(p1 + 4) &= ~(-2147483648 >> (v11 + -32 & 0x1F));
                        }
                    }
                }
                *(*(p2 + -4 - *(p2 + -8) + 8) + 4) = *(p2 + -4 - *(p2 + -8) + 4);
                *(*(p2 + -4 - *(p2 + -8) + 4) + 8) = *(p2 + -4 - *(p2 + -8) + 8);
            }
            v10 = p2 + -4 - *(p2 + -8);
        }
        if( (*(p2 + -8) & 0x1) != 0 || v9 != v11 ) {
            *(v10 + 4) = *(*(p1 + 16) + v1 + v9 * 8 + 4);
            *(v10 + 8) = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 516 + v9 * 8 + 324;
            *(*(p1 + 16) + v1 + v9 * 8 + 4) = v10;
            *(*(v10 + 4) + 8) = v10;
            if( *(v10 + 8) == *(v10 + 4) ) {
                v16 = *(v10 + 4) & 0xFFFFFF00 | *(*(p1 + 16) + v9 + 4);
                *(*(p1 + 16) + v9 + 4) = v16 & 0xFFFFFF00 | (uint8_t)v16 + 1;
                if( v9 < 32 ) {
                    if( (uint8_t)v16 == 0 ) {
                        *p1 |= -2147483648 >> (v9 & 0x1F);
                    }
                    v18 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) | -2147483648 >> (v9 & 0x1F);
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) = v18;
                } else {
                    if( (uint8_t)v16 == 0 ) {
                        *(p1 + 4) |= -2147483648 >> (v9 + -32 & 0x1F);
                    }
                    v17 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) | -2147483648 >> (v9 + -32 & 0x1F);
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) = v17;
                }
            }
        }
        *v10 = v3;
        *(v10 + v3 + -4) = v3;
        v19 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 516 + 324);
        *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 516 + 324) = v19 + 4294967295;
        if( v19 == 1 ) {
            if( data_0x9974 != 0 ) {
                v20 = VirtualFree;
                v20( data_0x996C * 32768 + *(data_0x9974 + 12), 32768, 16384 );
                *(data_0x9974 + 8) |= 2147483648 >> (data_0x996C & 0x1F);
                *(*(data_0x9974 + 16) + data_0x996C * 4 + 196) = 0;
                *(*(data_0x9974 + 16) + 67) += 4294967295;
                v21 = *(*(data_0x9974 + 16) + 67);
                if( v21 == 0 ) {
                    *(data_0x9974 + 4) &= 0xFFFFFFFE;
                }
                if( *(data_0x9974 + 8) == 4294967295 ) {
                    v20( *(data_0x9974 + 12), 0, 32768 );
                    HeapFree( data_0x9984, 0, *(data_0x9974 + 16) );
                    func_0x4C80( data_0x9974, data_0x9974 + 20, data_0x997C + data_0x9978 * 20 - data_0x9974 + 4294967276 );
                    data_0x9978 += 4294967295;
                    if( p1 > data_0x9974 ) {
                        p1 += -20;
                    }
                    data_0x9970 = data_0x997C;
                }
            }
            data_0x996C = (p2 - *(p1 + 12)) / 32768;
            data_0x9974 = p1;
        }
    }
}

// VA=0x402fa8
int32_t __cdecl ___sbh_alloc_block( int32_t p1 )
{
    uint32_t v1;
    uint32_t v4;
    uint32_t v22;
    uint32_t v7;
    uint32_t v11;
    uint32_t v8;
    uint32_t v9;
    uint32_t v10;
    uint32_t v12;
    uint32_t v14;
    int32_t v15;
    uint32_t v18;
    uint32_t v20;
    uint32_t local_0xC; // [esp-12]
    int32_t v25; // eax
    int32_t v2; // ebx
    uint32_t v13; // edi
    int32_t v21; // eax
    int32_t v16; // esi
    int32_t v17; // ebx
    int32_t v19; // ecx
    void * v23;
    int32_t v3;
    uint32_t * v6;
    int32_t v5;
    void * v24;

    if( (p1 + 23 & 0xFFFFFFF0) >> 4 < 33 ) {
        v1 = -1 >> (((p1 + 23 & 0xFFFFFFF0) >> 4) + -1 & 0x1F);
        local_0xC = 4294967295;
    } else {
        local_0xC = -1 >> (((p1 + 23 & 0xFFFFFFF0) >> 4) + -33 & 0x1F);
        v1 = 0;
    }
    if( data_0x9970 < data_0x997C + data_0x9978 * 20 ) {
        p1 = data_0x9970;
        v2 = data_0x9970;
        while( 1 ) {
            v3 = v2 + 20;
            v4 = *(v2 + 4) & local_0xC | *v2 & v1;
            if( v4 == 0 ) {
                if( v2 + 20 >= data_0x997C + data_0x9978 * 20 ) {
                    p1 = v3;
                    v2 = p1;
                    break;
                }
                v5 = v3;
                v2 += 20;
                continue;
            }
            break;
        }
    } else {
        p1 = data_0x9970;
        v2 = data_0x9970;
    }
    if( data_0x997C + data_0x9978 * 20 == v2 ) {
        p1 = data_0x997C;
        while( p1 < data_0x9970 ) {
            v22 = *(p1 + 4) & local_0xC | *p1 & v1;
            if( v22 == 0 ) {
                p1 += 20;
                continue;
            }
            break;
        }
        if( data_0x9970 == p1 ) {
            v2 = p1;
            while( 1 ) {
                v23 = v2 + 20;
                if( v2 >= data_0x997C + data_0x9978 * 20 || *(v2 + 8) != 0 ) {
                    break;
                }
                v24 = v23;
                v2 += 20;
            }
            if( data_0x997C + data_0x9978 * 20 == v2 ) {
                p1 = data_0x997C;
                while( p1 < data_0x9970 && *(p1 + 8) == 0 ) {
                    p1 += 20;
                }
                if( data_0x9970 == p1 ) {
                    p1 = ___sbh_alloc_new_region();
                    if( p1 != 0 ) {
                        v2 = p1;
                        goto node_129;
                    }
                } else {
                    v2 = p1;
                    goto node_129;
                }
            } else {
                node_129:
                v25 = ___sbh_alloc_new_group( v2 );
                **(v2 + 16) = v25;
                if( **(v2 + 16) != -1 ) {
                    goto node_40;
                }
            }
            v21 = 0;
        } else {
            v2 = p1;
            goto node_40;
        }
    } else {
        node_40:
        data_0x9970 = v2;
        v6 = *(v2 + 16) + 68;
        v7 = **(v2 + 16);
        if( v7 == 4294967295 ) {
            node_58:
            v8 = *(*(v2 + 16) + 196) & local_0xC | *(*(v2 + 16) + 68) & v1;
            if( v8 == 0 ) {
                v7 = 0;
                while( 1 ) {
                    v9 = v7 + 1;
                    v10 = v6[33] & local_0xC | v1 & v6[1];
                    if( v10 == 0 ) {
                        v7 += 1;
                        v6 = &v6[1];
                        continue;
                    }
                    break;
                }
                v7 = v9;
            } else {
                v7 = 0;
            }
        } else {
            v11 = *(*(v2 + 16) + v7 * 4 + 196) & local_0xC | *(*(v2 + 16) + v7 * 4 + 68) & v1;
            if( v11 == 0 ) {
                goto node_58;
            }
        }
        v12 = *(*(v2 + 16) + v7 * 4 + 68) & v1;
        if( v12 == 0 ) {
            v12 = *(*(v2 + 16) + v7 * 4 + 196) & local_0xC;
            v13 = 32;
        } else {
            v13 = 0;
        }
        while( v12 >= 0 ) {
            v13 += 1;
            v12 *= 2;
        }
        v14 = *(*(v2 + 16) + v7 * 516 + 324 + v13 * 8 + 4);
        v15 = *v14 - (p1 + 23 & 0xFFFFFFF0) >> 4;
        if( v15 > 64 ) {
            v16 = 63;
        } else {
            v16 = v15 + -1;
        }
        if( v13 == v16 ) {
            v17 = v2;
        } else {
            if( *(v14 + 8) != *(v14 + 4) ) {
                v17 = v2;
            } else {
                if( v13 < 32 ) {
                    *(*(v2 + 16) + v7 * 4 + 68) &= ~(2147483648 >> (v13 & 0x1F));
                    *(v13 + *(v2 + 16) + 4) += 4294967295;
                    if( *(v13 + *(v2 + 16) + 4) == 1 ) {
                        *p1 &= ~(2147483648 >> (v13 & 0x1F));
                        v17 = p1;
                        goto node_184;
                    }
                } else {
                    v18 = *(*(v2 + 16) + v7 * 4 + 196) & ~(2147483648 >> (v13 + 4294967264 & 0x1F));
                    *(*(v2 + 16) + v7 * 4 + 196) = v18;
                    *(v13 + *(v2 + 16) + 4) += 4294967295;
                    if( *(v13 + *(v2 + 16) + 4) == 1 ) {
                        *(p1 + 4) &= ~(2147483648 >> (v13 + 4294967264 & 0x1F));
                        v17 = p1;
                        goto node_184;
                    }
                }
                v17 = p1;
            }
            node_184:
            *(*(v14 + 8) + 4) = *(v14 + 4);
            *(*(v14 + 4) + 8) = *(v14 + 8);
            if( (p1 + 23 & 0xFFFFFFF0) != *v14 ) {
                *(v14 + 4) = *(*(v2 + 16) + v7 * 516 + 324 + v16 * 8 + 4);
                *(v14 + 8) = *(v2 + 16) + v7 * 516 + v16 * 8 + 324;
                *(*(v2 + 16) + v7 * 516 + 324 + v16 * 8 + 4) = v14;
                *(*(v14 + 4) + 8) = v14;
                if( *(v14 + 8) == *(v14 + 4) ) {
                    v19 = *(v14 + 4) & 0xFFFFFF00 | *(*(v2 + 16) + v16 + 4);
                    if( v16 < 32 ) {
                        *(*(v2 + 16) + v16 + 4) = v19 & 0xFFFFFF00 | (uint8_t)v19 + 1;
                        if( (uint8_t)v19 == 0 ) {
                            *v17 |= -2147483648 >> (v16 & 0x1F);
                        }
                        *(*(v2 + 16) + v7 * 4 + 68) |= -2147483648 >> (v16 & 0x1F);
                    } else {
                        *(*(v2 + 16) + v16 + 4) = v19 & 0xFFFFFF00 | (uint8_t)v19 + 1;
                        if( (uint8_t)v19 == 0 ) {
                            *(v17 + 4) |= -2147483648 >> (v16 + -32 & 0x1F);
                        }
                        *(*(v2 + 16) + v7 * 4 + 196) |= -2147483648 >> (v16 + -32 & 0x1F);
                    }
                }
            } else {
                *(v14 + *v14 - (p1 + 23 & 0xFFFFFFF0)) = (p1 + 23 & 0xFFFFFFF0) + 1;
                *((p1 + 23 & 0xFFFFFFF0) + v14 + *v14 - (p1 + 23 & 0xFFFFFFF0) + 4294967292) = (p1 + 23 & 0xFFFFFFF0) + 1;
                v20 = *(*(v2 + 16) + v7 * 516 + 324);
                *(*(v2 + 16) + v7 * 516 + 324) = v20 + 1;
                if( v20 == 0 && data_0x9974 == v17 && data_0x996C == v7 ) {
                    data_0x9974 = 0;
                }
                **(v2 + 16) = v7;
                return v14 + *v14 - (p1 + 23 & 0xFFFFFFF0) + 4;
            }
        }
        if( (p1 + 23 & 0xFFFFFFF0) != *v14 ) {
            *v14 -= p1 + 23 & 0xFFFFFFF0;
            *(v14 + *v14 - (p1 + 23 & 0xFFFFFFF0) + 4294967292) = *v14 - (p1 + 23 & 0xFFFFFFF0);
        }
        *(v14 + *v14 - (p1 + 23 & 0xFFFFFFF0)) = (p1 + 23 & 0xFFFFFFF0) + 1;
        *((p1 + 23 & 0xFFFFFFF0) + v14 + *v14 - (p1 + 23 & 0xFFFFFFF0) + 4294967292) = (p1 + 23 & 0xFFFFFFF0) + 1;
        v20 = *(*(v2 + 16) + v7 * 516 + 324);
        *(*(v2 + 16) + v7 * 516 + 324) = v20 + 1;
        if( v20 == 0 && data_0x9974 == v17 && data_0x996C == v7 ) {
            data_0x9974 = 0;
        }
        **(v2 + 16) = v7;
        v21 = v14 + *v14 - (p1 + 23 & 0xFFFFFFF0) + 4;
    }
    return v21;
}

// VA=0x4032b1
int32_t __cdecl ___sbh_alloc_new_region( void )
{
    void * v4; // eax
    void * v1; // eax
    void * v2; // eax
    int32_t v3; // eax

    if( data_0x9968 == data_0x9978 ) {
        v4 = HeapReAlloc( data_0x9984, 0, data_0x997C, (data_0x9968 * 5 + 80) * 4 );
        if( v4 != 0 ) {
            data_0x9968 += 16;
            data_0x997C = v4;
            goto node_15;
        }
    } else {
        node_15:
        v1 = HeapAlloc( data_0x9984, 8, 16836 );
        *(data_0x997C + data_0x9978 * 20 + 16) = v1;
        if( v1 != 0 ) {
            v2 = VirtualAlloc( 0, 1048576, 8192, 4 );
            *(data_0x997C + data_0x9978 * 20 + 12) = v2;
            if( v2 == 0 ) {
                HeapFree( data_0x9984, 0, *(data_0x997C + data_0x9978 * 20 + 16) );
            } else {
                *(data_0x997C + data_0x9978 * 20 + 8) = 4294967295;
                *(data_0x997C + data_0x9978 * 20) = 0;
                *(data_0x997C + data_0x9978 * 20 + 4) = 0;
                data_0x9978 += 1;
                **(data_0x997C + data_0x9978 * 20 + 16) = 4294967295;
                return data_0x997C + data_0x9978 * 20;
            }
        }
    }
    return 0;
}

// VA=0x403362
int32_t __cdecl ___sbh_alloc_new_group( int32_t p1 )
{
    uint32_t v1;
    uint32_t v8;
    uint32_t v9;
    int32_t v3; // eax
    int32_t v5; // edx
    int32_t v2; // ebx
    void * v7; // eax
    uint32_t * v6;
    uint32_t v4;

    v1 = *(p1 + 16);
    v2 = 0;
    v3 = *(p1 + 8);
    while( v3 >= 0 ) {
        v2 += 1;
        v3 *= 2;
    }
    v4 = v1 + v2 * 516 + 324;
    v5 = 63;
    while( 1 ) {
        *(v4 + 8) = v4;
        *(v4 + 4) = v4;
        if( v5 == 1 ) {
            break;
        }
        v5 += -1;
        v4 += 8;
    }
    v6 = *(p1 + 12) + v2 * 32768 + 16;
    v7 = VirtualAlloc( *(p1 + 12) + v2 * 32768, 32768, 4096, 4 );
    if( v7 == 0 ) {
        v2 = -1;
    } else {
        if( *(p1 + 12) + v2 * 32768 <= *(p1 + 12) + v2 * 32768 + 28672 ) {
            while( 1 ) {
                v6[-2] = 4294967295;
                v6[1019] = 4294967295;
                v6[-1] = 4080;
                v6[0] = &v6[1023];
                v6[1] = &v6[-1025];
                v6[1018] = 4080;
                if( &v6[1020] > *(p1 + 12) + v2 * 32768 + 28672 ) {
                    break;
                }
                v6 = &v6[1024];
            }
        }
        *(v1 + v2 * 516 + 832) = *(p1 + 12) + v2 * 32768 + 12;
        *(*(p1 + 12) + v2 * 32768 + 20) = v1 + v2 * 516 + 828;
        v8 = *(p1 + 12) + v2 * 32768 + 28672 + 12;
        *(v1 + v2 * 516 + 836) = v8;
        *(*(p1 + 12) + v2 * 32768 + 28688) = v1 + v2 * 516 + 828;
        *(v1 + v2 * 4 + 68) = 0;
        *(v1 + v2 * 4 + 196) = 1;
        v9 = (uint8_t)v1 + v2 * 516 + 828 & 0xFFFFFF00 | *(v1 + 67);
        *(v1 + 67) = (v8 & 0xFFFFFF00 | v9) & 0xFFFFFF00 | (uint8_t)(v8 & 0xFFFFFF00 | v9) + 1;
        if( v9 == 0 ) {
            *(p1 + 4) |= 0x1;
        }
        *(p1 + 8) &= ~(-2147483648 >> (v2 & 0x1F));
    }
    return v2;
}

// VA=0x40345d
int32_t __cdecl ___sbh_resize_block( int32_t p1, int32_t p2, int32_t p3 )
{
    uint32_t v14;
    uint32_t v17;
    uint32_t v16;
    int32_t v18;
    int32_t v20;
    int32_t v19;
    int32_t v2;
    uint32_t v5;
    uint32_t v8;
    uint32_t v7;
    int32_t v9;
    uint32_t local_0xC; // [esp-12]
    uint32_t * v22; // eax
    int32_t v3; // esi
    uint8_t v12; // cc_dst
    uint32_t * v11; // eax
    uint32_t v15; // ecx
    uint32_t v6; // esi
    int32_t v13; // eax
    int32_t v21; // ecx
    int32_t v10; // ecx
    uint8_t v4; // cc_dst
    int32_t v1;

    v1 = (p2 - *(p1 + 12)) / 32768 * 516 + 324;
    if( (p3 + 23 & 0xFFFFFFF0) > *(p2 + -4) + -1 ) {
        v12 = (uint8_t)*(p2 + *(p2 + -4) + -5) & 0x1;
        if( v12 == 0 && (p3 + 23 & 0xFFFFFFF0) <= *(p2 + *(p2 + -4) + -5) + *(p2 + -4) + -1 ) {
            v14 = *(p2 + *(p2 + -4) + -5) >> 4;
            if( v14 > 64 ) {
                local_0xC = 63;
                v15 = local_0xC;
            } else {
                local_0xC = v14 + 4294967295;
                v15 = v14 + 4294967295;
            }
            if( *(p2 + *(p2 + -4) + 3) == *(p2 + *(p2 + -4) + -1) ) {
                if( v15 < 32 ) {
                    v17 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) & ~(2147483648 >> (v15 & 0x1F));
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) = v17;
                    *(*(p1 + 16) + local_0xC + 4) += -1;
                    if( *(*(p1 + 16) + local_0xC + 4) == 1 ) {
                        *p1 &= ~(2147483648 >> (v15 & 0x1F));
                    }
                } else {
                    v16 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) & ~(2147483648 >> (v15 + 4294967264 & 0x1F));
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) = v16;
                    *(*(p1 + 16) + local_0xC + 4) += -1;
                    if( *(*(p1 + 16) + local_0xC + 4) == 1 ) {
                        *(p1 + 4) &= ~(2147483648 >> (v15 + 4294967264 & 0x1F));
                    }
                }
            }
            *(*(p2 + *(p2 + -4) + 3) + 4) = *(p2 + *(p2 + -4) + -1);
            *(*(p2 + *(p2 + -4) + -1) + 8) = *(p2 + *(p2 + -4) + 3);
            v18 = *(p2 + -4) + -1 - (p3 + 23 & 0xFFFFFFF0);
            if( *(p2 + *(p2 + -4) + -5) + v18 > 0 ) {
                v19 = *(p2 + *(p2 + -4) + -5) + v18 >> 4;
                if( v19 > 64 ) {
                    v20 = 63;
                } else {
                    v20 = v19 + -1;
                }
                *((p3 + 23 & 0xFFFFFFF0) + p2) = *(*(p1 + 16) + v1 + v20 * 8 + 4);
                *((p3 + 23 & 0xFFFFFFF0) + p2 + 4) = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 516 + v20 * 8 + 324;
                *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 516 + v20 * 8 + 328) = (p3 + 23 & 0xFFFFFFF0) + p2 + -4;
                *(*((p3 + 23 & 0xFFFFFFF0) + p2) + 8) = (p3 + 23 & 0xFFFFFFF0) + p2 + -4;
                if( *((p3 + 23 & 0xFFFFFFF0) + p2 + 4) == *((p3 + 23 & 0xFFFFFFF0) + p2) ) {
                    v21 = (p3 + 23 & 0xFFFFFFF0) + p2 + -4 & 0xFFFFFF00 | *(*(p1 + 16) + v20 + 4);
                    *(*(p1 + 16) + v20 + 4) = v21 & 0xFFFFFF00 | (uint8_t)v21 + 1;
                    if( v20 < 32 ) {
                        if( (uint8_t)v21 == 0 ) {
                            *p1 |= -2147483648 >> (v20 & 0x1F);
                        }
                        v22 = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68;
                    } else {
                        if( (uint8_t)v21 == 0 ) {
                            *(p1 + 4) |= -2147483648 >> (v20 + -32 & 0x1F);
                        }
                        v20 += -32;
                        v22 = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196;
                    }
                    v22[0] |= -2147483648 >> (v20 & 0x1F);
                }
                *((p3 + 23 & 0xFFFFFFF0) + p2 + -4) = *(p2 + *(p2 + -4) + -5) + v18;
                *(*(p2 + *(p2 + -4) + -5) + v18 + (p3 + 23 & 0xFFFFFFF0) + p2 + -8) = *(p2 + *(p2 + -4) + -5) + v18;
            }
            *(p2 + -4) = (p3 + 23 & 0xFFFFFFF0) + 1;
            *((p3 + 23 & 0xFFFFFFF0) + p2 + -8) = (p3 + 23 & 0xFFFFFFF0) + 1;
        } else {
            return 0;
        }
    } else if( (p3 + 23 & 0xFFFFFFF0) < *(p2 + -4) + -1 ) {
        v2 = *(p2 + -4) + -1 - (p3 + 23 & 0xFFFFFFF0);
        *(p2 + -4) = (p3 + 23 & 0xFFFFFFF0) + 1;
        *((p3 + 23 & 0xFFFFFFF0) + p2 + -8) = (p3 + 23 & 0xFFFFFFF0) + 1;
        if( v2 >> 4 > 64 ) {
            v3 = 63;
        } else {
            v3 = (v2 >> 4) + -1;
        }
        v4 = (uint8_t)*(p2 + *(p2 + -4) + -5) & 0x1;
        if( v4 == 0 ) {
            v5 = *(p2 + *(p2 + -4) + -5) >> 4;
            if( v5 > 64 ) {
                v6 = 63;
            } else {
                v6 = v5 + 4294967295;
            }
            if( *(p2 + *(p2 + -4) + 3) == *(p2 + *(p2 + -4) + -1) ) {
                if( v6 < 32 ) {
                    v8 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) & ~(2147483648 >> (v6 & 0x1F));
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68) = v8;
                    *(*(p1 + 16) + v6 + 4) += -1;
                    if( *(*(p1 + 16) + v6 + 4) == 1 ) {
                        *p1 &= ~(2147483648 >> (v6 & 0x1F));
                    }
                } else {
                    v7 = *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) & ~(2147483648 >> (v6 + 4294967264 & 0x1F));
                    *(*(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196) = v7;
                    *(*(p1 + 16) + v6 + 4) += -1;
                    if( *(*(p1 + 16) + v6 + 4) == 1 ) {
                        *(p1 + 4) &= ~(2147483648 >> (v6 + 4294967264 & 0x1F));
                    }
                }
            }
            *(*(p2 + *(p2 + -4) + 3) + 4) = *(p2 + *(p2 + -4) + -1);
            *(*(p2 + *(p2 + -4) + -1) + 8) = *(p2 + *(p2 + -4) + 3);
            v9 = v2 + *(p2 + *(p2 + -4) + -5) >> 4;
            if( v9 > 64 ) {
                p3 = v2 + *(p2 + *(p2 + -4) + -5);
                v3 = 63;
            } else {
                p3 = v2 + *(p2 + *(p2 + -4) + -5);
                v3 = v9 + -1;
            }
        } else {
            p3 = v2;
        }
        *((p3 + 23 & 0xFFFFFFF0) + p2) = *(*(p1 + 16) + v1 + v3 * 8 + 4);
        *((p3 + 23 & 0xFFFFFFF0) + p2 + 4) = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 516 + v3 * 8 + 324;
        *(*(p1 + 16) + v1 + v3 * 8 + 4) = (p3 + 23 & 0xFFFFFFF0) + p2 + -4;
        *(*((p3 + 23 & 0xFFFFFFF0) + p2) + 8) = (p3 + 23 & 0xFFFFFFF0) + p2 + -4;
        if( *((p3 + 23 & 0xFFFFFFF0) + p2 + 4) == *((p3 + 23 & 0xFFFFFFF0) + p2) ) {
            v10 = *((p3 + 23 & 0xFFFFFFF0) + p2) & 0xFFFFFF00 | *(*(p1 + 16) + v3 + 4);
            *(*(p1 + 16) + v3 + 4) = v10 & 0xFFFFFF00 | (uint8_t)v10 + 1;
            if( v3 < 32 ) {
                if( (uint8_t)v10 == 0 ) {
                    *p1 |= -2147483648 >> (v3 & 0x1F);
                }
                v11 = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 68;
            } else {
                if( (uint8_t)v10 == 0 ) {
                    *(p1 + 4) |= -2147483648 >> (v3 + -32 & 0x1F);
                }
                v3 += -32;
                v11 = *(p1 + 16) + (p2 - *(p1 + 12)) / 32768 * 4 + 196;
            }
            v11[0] |= -2147483648 >> (v3 & 0x1F);
        }
        *((p3 + 23 & 0xFFFFFFF0) + p2 + -4) = p3;
        *((p3 + 23 & 0xFFFFFFF0) + p2 + -4 + p3 + -4) = p3;
    }
    return 1;
}

// VA=0x403753
int32_t __cdecl func_0x3753( void )
{
    uint32_t * v2;
    uint32_t v5;
    void * lpMem; // eax
    void * lpAddress; // eax
    void * v3; // eax
    int32_t v4; // ebp
    void * v1;

    if( data_0x7218 == 4294967295 ) {
        lpMem = &data_0x7208;
    } else {
        lpMem = HeapAlloc( data_0x9984, 0, 8224 );
        if( lpMem == 0 ) {
            goto node_25;
        }
    }
    v1 = (uint8_t *)lpMem + 24;
    v2 = VirtualAlloc;
    lpAddress = v2( 0, &data_0x0, 8192, 4 );
    if( lpAddress == 0 ) {
        node_34:
        if( &data_0x7208 != lpMem ) {
            HeapFree( data_0x9984, 0, lpMem );
        }
    } else {
        v3 = v2( lpAddress, 65536, 4096, 4 );
        if( v3 == 0 ) {
            VirtualFree( lpAddress, 0, 32768 );
            goto node_34;
        } else {
            if( &data_0x7208 != lpMem ) {
                *lpMem = &data_0x7208;
                *((uint8_t *)lpMem + 4) = data_0x720C;
                data_0x720C = lpMem;
                **((uint8_t *)lpMem + 4) = lpMem;
            } else {
                if( data_0x7208 == 0 ) {
                    data_0x7208 = &data_0x7208;
                }
                if( data_0x720C == 0 ) {
                    data_0x720C = &data_0x7208;
                }
            }
            *((uint8_t *)lpMem + 20) = &data_0x0 + lpAddress;
            *((uint8_t *)lpMem + 12) = (uint8_t *)lpMem + 152;
            *((uint8_t *)lpMem + 16) = lpAddress;
            *((uint8_t *)lpMem + 8) = (uint8_t *)lpMem + 24;
            v4 = 0;
            while( 1 ) {
                v5 = ((((v4 ^ 0x10) & (v4 ^ v4 - 16)) >> 31 == v4 - 16 >> 31) + -1 & 0xF1) + -1;
                *v1 = v5;
                *((uint8_t *)v1 + 4) = 241;
                if( v4 > 1022 ) {
                    break;
                }
                v4 += 1;
                (uint8_t *)v1 += 8;
            }
            _memset( lpAddress, 0, 65536 );
            while( lpAddress < *((uint8_t *)lpMem + 16) + (uint8_t)65536 ) {
                *((uint8_t *)lpAddress + 248) |= 0xFF;
                *lpAddress = (uint8_t *)lpAddress + 8;
                *((uint8_t *)lpAddress + 4) = 240;
                (uint8_t *)lpAddress += (uint8_t)4096;
            }
            return lpMem;
        }
    }
    node_25:
    return 0;
}

// VA=0x403897
void __cdecl func_0x3897( int32_t p1 )
{
    VirtualFree( *(p1 + 16), 0, 32768 );
    if( p1 == data_0x9228 ) {
        data_0x9228 = *(p1 + 4);
    }
    if( &data_0x7208 != p1 ) {
        **(p1 + 4) = *p1;
        *(*p1 + 4) = *(p1 + 4);
        HeapFree( data_0x9984, 0, p1 );
        return;
    }
    data_0x7218 = 4294967295;
}

// VA=0x4038ed
void __cdecl func_0x38ED( int32_t p1 )
{
    int32_t v4;
    int32_t v7;
    int32_t v8;
    int32_t v10;
    void * v5; // ebx
    int v6; // eax
    int32_t v9; // edx
    int32_t v1; // esi
    void * v3;
    uint32_t * v2;

    v1 = data_0x720C;
    do {
        v2 = v1 + 8208;
        v3 = v1 + 32;
        if( *(v1 + 16) == -1 ) {
            node_10:
            continue;
        }
        v4 = 0;
        v5 = 4190208;
        while( 1 ) {
            if( v2[0] == 240 ) {
                v6 = VirtualFree( v5 + *(v1 + 16), 4096, 16384 );
                if( v6 != 0 ) {
                    v2[0] = 4294967295;
                    data_0x9704 += 4294967295;
                    if( *(v1 + 12) == 0 || *(v1 + 12) > v2 ) {
                        *(v1 + 12) = v2;
                    }
                    v4 += 1;
                    v7 = p1 + -1;
                    if( p1 == 1 ) {
                        p1 = v7;
                        break;
                    }
                    p1 = v7;
                }
            }
            if( v5 < 4096 ) {
                break;
            }
            v2 = &v2[-2];
            (uint8_t *)v5 -= (uint8_t)4096;
        }
        v8 = *(v1 + 4);
        if( v4 != 0 && *(v1 + 24) == -1 ) {
            v9 = 1;
            while( *v3 == 4294967295 ) {
                v10 = v9 + 1;
                if( v9 > 1022 ) {
                    v9 = v10;
                    break;
                }
                v9 += 1;
                (uint8_t *)v3 += 8;
            }
            if( v9 == 1024 ) {
                func_0x3897( v1 );
                v1 = v8;
                goto node_10;
            } else {
                v1 = v8;
                goto node_10;
            }
        } else {
            v1 = v8;
            goto node_10;
        }
    } while( data_0x720C != v1 && p1 > 0 );
}

// VA=0x4039af
int32_t __cdecl func_0x39AF( int32_t p1, int32_t p2, int32_t p3 )
{
    int32_t v2;
    uint32_t v1; // ecx

    v1 = &data_0x7208;
    while( p1 <= *(v1 + 16) || p1 >= *(v1 + 20) ) {
        if( &data_0x7208 == *v1 ) {
            return 0;
        }
        v1 = *v1;
    }
    if( (uint8_t)(p1 & 0xF) == 0 && (p1 & 0xFFF) > 255 ) {
        *p2 = v1;
        *p3 = p1 & 0xFFFF0000 | (uint16_t)p1 & 0xF000;
        return (p1 & 0xFFFF0000 | (uint16_t)p1 & 0xF000) + (p1 - (p1 & 0xFFFF0000 | (uint16_t)p1 & 0xF000) - 256 >> 4) + 8;
    }
    return 0;
}

// VA=0x403a06
void __cdecl func_0x3A06( int32_t p1, int32_t p2, int32_t p3 )
{
    uint32_t v1;
    uint32_t v2;

    v1 = *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24) + *p3;
    *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24) = v1;
    *p3 = 0;
    v2 = *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24);
    *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 28) = 241;
    if( v2 == 240 ) {
        data_0x9704 += 1;
        if( data_0x9704 == 32 ) {
            func_0x38ED( 16 );
        }
    }
}

// VA=0x403a4b
int32_t __cdecl func_0x3A4B( int32_t p1 )
{
    int32_t v14;
    uint32_t v15;
    int32_t v8;
    uint32_t v6;
    int32_t v10;
    int32_t v1; // esi
    uint32_t v3; // edi
    uint32_t v13; // ecx
    int32_t v5; // eax
    uint32_t v9; // eax
    void * v11; // eax
    int32_t v4; // eax
    int32_t v2;
    uint32_t * v7;
    uint32_t v12;

    v1 = data_0x9228;
    while( 1 ) {
        v2 = v1 + 24;
        if( *(v1 + 16) == -1 ) {
            node_23:
            if( data_0x9228 == *v1 ) {
                v3 = &data_0x7208;
                while( *(v3 + 16) == 4294967295 || *(v3 + 12) == 0 ) {
                    if( &data_0x7208 != *v3 ) {
                        v3 = *v3;
                        continue;
                    }
                    v4 = func_0x3753();
                    if( v4 == 0 ) {
                        goto node_146;
                    } else {
                        *(*(v4 + 16) + 8) = p1;
                        data_0x9228 = v4;
                        **(v4 + 16) = p1 + *(v4 + 16) + 8;
                        *(*(v4 + 16) + 4) = 240;
                        *(v4 + 24) -= (uint8_t)p1;
                        return *(v4 + 16) + 256;
                    }
                }
                v6 = *(v3 + 12);
                v7 = *(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096 + 4;
                v5 = *(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096 + 256;
                if( *v6 == 4294967295 ) {
                    v8 = 0;
                    v9 = v6;
                    while( v8 <= 15 ) {
                        v10 = v8 + 1;
                        if( *(v9 + 8) == 4294967295 ) {
                            v8 += 1;
                            v9 += 8;
                            continue;
                        }
                        v8 = v10;
                        break;
                    }
                } else {
                    v8 = 0;
                }
                v11 = VirtualAlloc( *(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096, v8 * 4096, 4096, 4 );
                if( *(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096 == v11 ) {
                    _memset( *(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096, v8 * 4096, 0 );
                    if( v8 > 0 ) {
                        v13 = v6;
                        while( 1 ) {
                            v12 = v13 + 8;
                            v7[61] |= 0xFF;
                            v7[-1] = &v7[1];
                            v7[0] = 240;
                            *v13 = 240;
                            *(v13 + 4) = 241;
                            if( v8 == 1 ) {
                                break;
                            }
                            v8 += -1;
                            v13 += 8;
                            v7 = &v7[1024];
                        }
                    } else {
                        v12 = v6;
                    }
                    data_0x9228 = v3;
                    while( v12 < v3 + 8216 && *v12 != 4294967295 ) {
                        v12 += 8;
                    }
                    *(v3 + 12) = -(v12 < v3 + 8216) & v12;
                    *(*(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096 + 8) = p1;
                    *(v3 + 8) = v6;
                    *v6 -= p1;
                    *(*(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096 + 4) -= p1;
                    *(*(v3 + 16) + (v6 - v3 - 24 >> 3) * 4096) = p1 + (v6 - v3 - 24 >> 3) * 4096 + *(v3 + 16) + 8;
                    return v5;
                }
                node_146:
                v5 = 0;
                break;
            }
            v1 = *v1;
            continue;
        }
        v14 = *(v1 + 8);
        v8 = (v14 - v1 - 24 >> 3) * 4096 + *(v1 + 16);
        if( v14 < v1 + 8216 ) {
            while( 1 ) {
                if( *v14 >= p1 && *(v14 + 4) > p1 ) {
                    v5 = func_0x3C53( v8, *v14, p1 );
                    if( v5 != 0 ) {
                        break;
                    }
                    *(v14 + 4) = p1;
                }
                v8 += 4096;
                if( v14 + 8 >= v1 + 8216 ) {
                    goto node_41;
                } else {
                    v14 += 8;
                }
            }
        } else {
            node_41:
            v15 = *(v1 + 8);
            v8 = *(v1 + 16);
            if( v1 + 24 < v15 ) {
                while( 1 ) {
                    if( *v2 >= p1 && *(v2 + 4) > p1 ) {
                        v5 = func_0x3C53( v8, *v2, p1 );
                        if( v5 == 0 ) {
                            *(v2 + 4) = p1;
                        } else {
                            v14 = v2;
                            break;
                        }
                    }
                    if( v2 + 8 >= v15 ) {
                        goto node_23;
                    } else {
                        v8 += 4096;
                        v2 += 8;
                    }
                }
            } else {
                goto node_23;
            }
        }
        data_0x9228 = v1;
        *v14 -= p1;
        *(v1 + 8) = v14;
        break;
    }
    return v5;
}

// VA=0x403c53
int32_t __cdecl func_0x3C53( int32_t p1, int32_t p2, int32_t p3 )
{
    uint8_t v1;
    int32_t v13;
    int32_t v7;
    int32_t v14;
    int32_t v10; // ebx
    int32_t v12; // esi
    int32_t v8; // ebx
    int32_t v2; // eax
    void * v3; // esi
    int32_t v6; // eax
    int32_t v11;
    int32_t v9;
    int32_t v5;
    void * v4;

    if( *(p1 + 4) >= p3 ) {
        **p1 = p3;
        if( p3 + *p1 < p1 + 248 ) {
            *p1 += p3;
            *(p1 + 4) -= p3;
        } else {
            *(p1 + 4) = 0;
            *p1 = p1 + 8;
        }
        v5 = *p1 + 8;
    } else {
        v1 = *(*(p1 + 4) + *p1);
        if( v1 == 0 ) {
            v2 = *p1;
        } else {
            v2 = *(p1 + 4) + *p1;
        }
        if( p3 + v2 < p1 + 248 ) {
            v8 = p1 + 248;
            do {
                v9 = v2 + 1;
                v5 = v2 + 8;
                v10 = v8 & 0xFFFFFF00 | *v2;
                v11 = v2 + (uint8_t)(v8 & 0xFFFFFF00 | *v2);
                if( (uint8_t)(v8 & 0xFFFFFF00 | *v2) == 0 ) {
                    v12 = 1;
                    v8 = v9;
                    while( *v8 == 0 ) {
                        v12 += 1;
                        v8 += 1;
                    }
                    if( v12 < p3 ) {
                        if( *p1 == v2 ) {
                            *(p1 + 4) = v12;
                        } else {
                            v13 = p2 - v12;
                            if( p2 - v12 >= p3 ) {
                                p2 = v13;
                            } else {
                                goto node_68;
                            }
                        }
                        v2 = v8;
                    } else {
                        if( p3 + v2 < p1 + 248 ) {
                            *p1 = p3 + v2;
                            *(p1 + 4) = v12 - p3;
                        } else {
                            *(p1 + 4) = 0;
                            *p1 = p1 + 8;
                        }
                        *v2 = p3;
                        goto node_39;
                    }
                } else {
                    v8 = v10;
                    v2 = v11;
                }
            } while( p3 + v2 < p1 + 248 );
        }
        v3 = p1 + 8;
        while( 1 ) {
            v4 = (uint8_t *)v3 + 1;
            v5 = (uint8_t *)v3 + 8;
            if( v3 >= *p1 || p3 + v3 >= p1 + 248 ) {
                break;
            }
            if( (uint8_t)(p3 + v3 & 0xFFFFFF00 | *v3) == 0 ) {
                v6 = 1;
                while( *v4 == 0 ) {
                    (uint8_t *)v4 += 1;
                    v6 += 1;
                }
                if( v6 < p3 ) {
                    v7 = p2 - v6;
                    if( p2 - v6 < p3 ) {
                        goto node_68;
                    } else {
                        p2 = v7;
                        v3 = v4;
                    }
                } else {
                    if( p3 + v3 < p1 + 248 ) {
                        *p1 = p3 + v3;
                        *(p1 + 4) = v6 - p3;
                    } else {
                        *(p1 + 4) = 0;
                        *p1 = p1 + 8;
                    }
                    *v3 = p3;
                    goto node_39;
                }
            } else {
                v3 += (uint8_t)p3 + v3 & 0xFFFFFF00 | *v3;
            }
        }
        node_68:
        return 0;
    }
    node_39:
    return v5 * 16 - p1 * 15;
}

// VA=0x403d77
int32_t __cdecl func_0x3D77( int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    int32_t v5;
    int32_t v4;
    int32_t local_0x8; // [esp-8]
    uint8_t v1; // eax
    void * v3; // eax
    uint32_t v2;

    if( *p3 > p4 ) {
        *p3 = p4;
        v5 = *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24) + *p3 - p4;
        *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24) = v5;
        *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 28) = 241;
    } else if( *p3 < p4 && p2 + 248 >= p4 + p3 ) {
        v1 = p3 + *p3;
        while( v1 < p4 + p3 && *v1 == 0 ) {
            v1 += 1;
        }
        if( p4 + p3 == v1 ) {
            *p3 = v1 & 0xFFFFFF00 | (uint8_t)p4;
            if( p3 <= *p2 && p4 + p3 > *p2 ) {
                if( p4 + p3 < p2 + 248 ) {
                    *p2 = p4 + p3;
                    if( *(p4 + p3) == 0 ) {
                        v3 = 0;
                        while( 1 ) {
                            v2 = (uint8_t *)v3 + 1;
                            if( *(v3 + 1 + p4 + p3) == 0 ) {
                                (uint8_t *)v3 += 1;
                                continue;
                            }
                            break;
                        }
                    } else {
                        v2 = 0;
                    }
                    *(p2 + 4) = v2;
                } else {
                    *(p2 + 4) = 0;
                    *p2 = p2 + 8;
                }
            }
            v4 = *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24) + *p3 - p4;
            *(p1 + (p2 - *(p1 + 16) >> 12) * 8 + 24) = v4;
        } else {
            return 0;
        }
    } else {
        return 0;
    }
    return 1;
}

// VA=0x403e20
int32_t __cdecl __callnewh( int32_t p1 )
{
    int32_t v1; // eax

    if( data_0x9708 == 0 ) {
        node_18:
        return 0;
    }
    v1 = data_0x9708( p1 );
    if( v1 == 0 ) {
        goto node_18;
    } else {
        return 1;
    }
}

// VA=0x403e40
// Decompilation timed out after 15 seconds.

// VA=0x404047
inline int32_t __fastcall func_0x4047( int32_t p1, int32_t p2 )
{
    int32_t v5;
    int32_t v6;
    int32_t v3;
    void * ebp; // ebp
    void * esi; // esi
    void * edi; // edi
    int32_t v4; // eax
    void * v2;
    void * v1;

    *(p2 + -74) |= (uint16_t)p2 >> 8;
    while( 1 ) {
        v1 = (uint8_t *)edi + (uint8_t)4294967294;
        v2 = (uint8_t *)esi + (uint8_t)4294967294;
        if( p1 == 0 ) {
            break;
        }
        *edi = *esi;
        (uint8_t *)edi += (uint8_t)4294967292;
        (uint8_t *)esi += (uint8_t)4294967292;
        p1 += -1;
    }
    if( p2 == 0 ) {
        node_19:
        return *((uint8_t *)ebp + 8);
    }
    if( p2 == 1 ) {
        node_13:
        *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
        return *((uint8_t *)ebp + 8);
    }
    if( p2 == 2 ) {
        node_15:
        *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
        *((uint8_t *)edi + 2) = (v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3)) & 0xFFFFFF00 | *((uint8_t *)esi + 2);
        return *((uint8_t *)ebp + 8);
    }
    if( p2 == 3 ) {
        node_17:
        *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
        v4 = (v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3)) & 0xFFFFFF00 | *((uint8_t *)esi + 2);
        *((uint8_t *)edi + 2) = v4;
        *((uint8_t *)edi + 1) = v4 & 0xFFFFFF00 | *((uint8_t *)esi + 1);
        return *((uint8_t *)ebp + 8);
    }
    *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
    v3 = (v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3)) & 0xFFFFFF00 | *((uint8_t *)esi + 2);
    v5 = p1 >> 2;
    *((uint8_t *)edi + 2) = v3;
    if( p1 >> 2 > 7 ) {
        edi = v1;
        esi = v2;
        while( v5 != 0 ) {
            *edi = *esi;
            (uint8_t *)edi += (uint8_t)4294967292;
            (uint8_t *)esi += (uint8_t)4294967292;
            v5 += -1;
        }
        if( &code_0x4130 == *(&jump_table_4 + (p2 & p1) * 4) ) {
            return *((uint8_t *)ebp + 8);
        } else if( &code_0x4138 == *(&jump_table_4 + (p2 & p1) * 4) ) {
            *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
            return *((uint8_t *)ebp + 8);
        } else if( &code_0x4148 == *(&jump_table_4 + (p2 & p1) * 4) ) {
            *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
            *((uint8_t *)edi + 2) = (v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3)) & 0xFFFFFF00 | *((uint8_t *)esi + 2);
            return *((uint8_t *)ebp + 8);
        } else if( &code_0x415C == *(&jump_table_4 + (p2 & p1) * 4) ) {
            *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
            v4 = (v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3)) & 0xFFFFFF00 | *((uint8_t *)esi + 2);
            *((uint8_t *)edi + 2) = v4;
            *((uint8_t *)edi + 1) = v4 & 0xFFFFFF00 | *((uint8_t *)esi + 1);
            return *((uint8_t *)ebp + 8);
        }
    }
    edi = v1;
    esi = v2;
    do {
        v6 = -v5;
        switch( v5 ) {
            case 7: {
                goto node_42;
                break;
            }
            case 6: {
                goto node_44;
                break;
            }
            case 5: {
                goto node_46;
                break;
            }
            case 4: {
                goto node_48;
                break;
            }
            case 3: {
                goto node_50;
                break;
            }
            case 2: {
                goto node_52;
                break;
            }
            case 1: {
                goto node_54;
                break;
            }
            case 0: {
                goto node_56;
                break;
            }
            default: {
                v5 = v6;
                while( v5 != 0 ) {
                    *edi = *esi;
                    (uint8_t *)edi += (uint8_t)4294967292;
                    (uint8_t *)esi += (uint8_t)4294967292;
                    v5 += -1;
                    continue;
                }
                if( &code_0x4130 == *(&jump_table_4 + (p2 & p1) * 4) ) {
                    return *((uint8_t *)ebp + 8);
                } else if( &code_0x4138 == *(&jump_table_4 + (p2 & p1) * 4) ) {
                    *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
                    return *((uint8_t *)ebp + 8);
                } else if( &code_0x4148 == *(&jump_table_4 + (p2 & p1) * 4) ) {
                    *((uint8_t *)edi + 3) = v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3);
                    *((uint8_t *)edi + 2) = (v3 & 0xFFFFFF00 | *((uint8_t *)esi + 3)) & 0xFFFFFF00 | *((uint8_t *)esi + 2);
                    return *((uint8_t *)ebp + 8);
                }
                break;
            }
        }
    } while( &code_0x415C != *(&jump_table_4 + (p2 & p1) * 4) );
    goto node_17;
    node_42:
    *(edi + -v5 * 4 + 28) = *(esi + -v5 * 4 + 28);
    node_44:
    *(edi + -v5 * 4 + 24) = *(esi + -v5 * 4 + 24);
    node_46:
    *(edi + -v5 * 4 + 20) = *(esi + -v5 * 4 + 20);
    node_48:
    *(edi + -v5 * 4 + 16) = *(esi + -v5 * 4 + 16);
    node_50:
    *(edi + -v5 * 4 + 12) = *(esi + -v5 * 4 + 12);
    node_52:
    *(edi + -v5 * 4 + 8) = *(esi + -v5 * 4 + 8);
    node_54:
    *(edi + -v5 * 4 + 4) = *(esi + -v5 * 4 + 4);
    v3 = -v5 * 4;
    edi += -v5 * 4;
    esi += -v5 * 4;
    node_56:
    if( &code_0x4130 == *(&jump_table_4 + (p2 & p1) * 4) ) {
        goto node_19;
    } else if( &code_0x4138 == *(&jump_table_4 + (p2 & p1) * 4) ) {
        goto node_13;
    } else if( &code_0x4148 == *(&jump_table_4 + (p2 & p1) * 4) ) {
        goto node_15;
    } else if( &code_0x415C != *(&jump_table_4 + (p2 & p1) * 4) ) {
        goto &data_0x411E;
    } else {
        goto node_17;
    }
}

// VA=0x404175
int32_t __cdecl __ismbblead( int32_t p1 )
{
    int32_t v1; // eax

    _x_ismbbtype( p1, 0, 4 );
    return v1;
}

// VA=0x404186
void __cdecl _x_ismbbtype( int8_t p1, int32_t p2, int8_t p3 )
{
    int32_t ecx; // ecx
    int32_t v1; // eax

    if( (uint8_t)(*(&data_0x9861 + (uint8_t)p1) & (ecx & 0xFFFFFF00 | (uint8_t)p3)) == 0 ) {
        if( p2 == 0 ) {
            v1 = 0;
        } else {
            v1 = *(" " + (uint8_t)p1 * 2) & p2;
        }
        if( v1 == 0 ) {
            return;
        }
        return;
    }
}

// VA=0x4041b7
int32_t __cdecl func_0x41B7( int32_t p1 )
{
    uint8_t v12;
    uint32_t v17;
    uint32_t v21;
    uint32_t local_0x1C; // [esp-28]
    uint8_t local_0x16; // [esp-22]
    uint8_t local_0x15; // [esp-21]
    uint32_t local_0x8; // [esp-8]
    unsigned int CodePage; // eax
    void * v7; // edi
    void * v5; // eax
    uint32_t v4; // edx
    int32_t v8; // ecx
    int32_t v10; // edx
    int v6; // eax
    void * v9; // ecx
    void * v15; // edi
    uint32_t v13; // eax
    int32_t v16; // ecx
    uint32_t v20; // edx
    int32_t v11; // cc_dst
    uint32_t v19; // ecx
    uint32_t v22; // edx
    uint32_t v18; // edx
    void * v2; // edi
    int32_t v3; // ecx
    uint32_t v14; // eax
    uint32_t v23; // eax
    int32_t v1; // eax

    CodePage = _getSystemCP( p1 );
    p1 = CodePage;
    if( data_0x9748 != CodePage ) {
        if( CodePage == 0 ) {
            node_20:
            v2 = &data_0x9860;
            v3 = 64;
            while( v3 != 0 ) {
                *v2 = 0;
                (uint8_t *)v2 += 4;
                v3 += -1;
            }
            *v2 = 0;
            data_0x9748 = 0;
            data_0x975C = 0;
            data_0x9964 = 0;
            data_0x9750 = 0;
            data_0x9750+0x4 = 0;
            data_0x9750+0x8 = 0;
        } else {
            v4 = 0;
            v5 = &data_0x9238;
            while( CodePage != *v5 ) {
                if( v5 > &data_0x9248+0xAF ) {
                    v6 = GetCPInfo( CodePage, &local_0x1C );
                    if( v6 == 1 ) {
                        data_0x9748 = CodePage;
                        v7 = &data_0x9860;
                        v8 = 64;
                        while( v8 != 0 ) {
                            *v7 = 0;
                            (uint8_t *)v7 += 4;
                            v8 += -1;
                        }
                        *v7 = 0;
                        data_0x9964 = 0;
                        if( local_0x1C < 2 ) {
                            data_0x975C = 0;
                        } else {
                            if( local_0x16 != 0 ) {
                                v9 = &local_0x15;
                                while( 1 ) {
                                    v11 = (uint8_t)v10 & 0xFFFFFF00 | *v9;
                                    if( (uint8_t)(v10 & 0xFFFFFF00 | *v9) == 0 ) {
                                        break;
                                    }
                                    v12 = *((uint8_t *)v9 + (uint8_t)4294967295);
                                    while( v12 <= (uint8_t)(v10 & 0xFFFFFF00 | *v9) ) {
                                        *(&data_0x9861 + v12) |= 0x4;
                                        v12 += 1;
                                    }
                                    if( *((uint8_t *)v9 + 1) == 0 ) {
                                        break;
                                    }
                                    v10 = v11;
                                    (uint8_t *)v9 += 2;
                                }
                            }
                            v13 = 1;
                            while( 1 ) {
                                *(&data_0x9861 + v13) |= 0x8;
                                if( v13 > 253 ) {
                                    break;
                                }
                                v13 += 1;
                            }
                            switch( CodePage ) {
                                case 932: {
                                    v14 = 1041;
                                    break;
                                }
                                case 936: {
                                    v14 = 2052;
                                    break;
                                }
                                case 949: {
                                    v14 = 1042;
                                    break;
                                }
                                case 950: {
                                    v14 = 1028;
                                    break;
                                }
                                default: {
                                    v14 = 0;
                                    break;
                                }
                            }
                            data_0x9964 = v14;
                            data_0x975C = 1;
                        }
                        data_0x9750 = 0;
                        data_0x9750+0x4 = 0;
                        data_0x9750+0x8 = 0;
                        goto node_31;
                    } else if( data_0x9710 == 0 ) {
                        return 4294967295;
                    } else {
                        goto node_20;
                    }
                } else {
                    v4 += 1;
                    (uint8_t *)v5 += 48;
                }
            }
            v15 = &data_0x9860;
            v16 = 64;
            while( v16 != 0 ) {
                *v15 = 0;
                (uint8_t *)v15 += 4;
                v16 += -1;
            }
            *v15 = 0;
            v17 = v4 * 48 + &data_0x9248;
            local_0x8 = 0;
            v18 = v4;
            while( 1 ) {
                if( *v17 != 0 ) {
                    v19 = v17;
                    while( 1 ) {
                        v20 = v18 & 0xFFFFFF00 | *(v19 + 1);
                        if( (uint8_t)(v18 & 0xFFFFFF00 | *(v19 + 1)) == 0 ) {
                            break;
                        }
                        v21 = *v19;
                        if( v21 <= (uint8_t)(v18 & 0xFFFFFF00 | *(v19 + 1)) ) {
                            v22 = local_0x8 & 0xFFFFFF00 | *(&data_0x9230 + local_0x8);
                            while( 1 ) {
                                *(&data_0x9861 + v21) |= local_0x8 & 0xFFFFFF00 | *(&data_0x9230 + local_0x8);
                                if( v21 + 1 > (uint8_t)(v18 & 0xFFFFFF00 | *(v19 + 1)) ) {
                                    break;
                                }
                                v21 += 1;
                            }
                            v18 = v22;
                        } else {
                            v18 = v20;
                        }
                        if( *(v19 + 2) == 0 ) {
                            goto node_95;
                        } else {
                            v19 += 2;
                        }
                    }
                    v18 = v20;
                }
                node_95:
                if( local_0x8 > 2 ) {
                    break;
                }
                v17 += 8;
                local_0x8 += 1;
            }
            data_0x975C = 1;
            data_0x9748 = CodePage;
            switch( CodePage ) {
                case 932: {
                    v23 = 1041;
                    break;
                }
                case 936: {
                    v23 = 2052;
                    break;
                }
                case 949: {
                    v23 = 1042;
                    break;
                }
                case 950: {
                    v23 = 1028;
                    break;
                }
                default: {
                    v23 = 0;
                    break;
                }
            }
            data_0x9750 = data_0x923C[v4 * 4];
            data_0x9750+0x4 = data_0x923C[v4 * 4];
            data_0x9964 = v23;
            data_0x9750+0x8 = data_0x923C[v4 * 4];
        }
        node_31:
        _setSBUpLow();
    }
    return 0;
}

// VA=0x404350
int32_t __cdecl _getSystemCP( int32_t p1 )
{
    data_0x9710 = 0;
    switch( p1 ) {
        case 4294967294: {
            data_0x9710 = 1;
            goto GetOEMCP;
            break;
        }
        case 4294967293: {
            data_0x9710 = 1;
            goto GetACP;
            break;
        }
        default: {
            if( p1 == -4 ) {
                data_0x9710 = 1;
                p1 = data_0x9738;
            }
            return p1;
        }
    }
}

// VA=0x40439a
inline int32_t __cdecl _CPtoLCID( int32_t p1 )
{
    if( p1 == 932 ) {
        return 1041;
    }
    if( p1 == 936 ) {
        return 2052;
    }
    if( p1 == 949 ) {
        return 1042;
    }
    if( p1 == 950 ) {
        return 1028;
    }
    return 0;
}

// VA=0x4043cd
inline void __cdecl _setSBCS( void )
{
    int32_t v2; // ecx
    void * v1; // edi

    v1 = &data_0x9860;
    v2 = 64;
    while( v2 != 0 ) {
        *v1 = 0;
        (uint8_t *)v1 += 4;
        v2 += -1;
    }
    *v1 = 0;
    data_0x9748 = 0;
    data_0x975C = 0;
    data_0x9964 = 0;
    data_0x9750 = 0;
    data_0x9750+0x4 = 0;
    data_0x9750+0x8 = 0;
}

// VA=0x4043f6
void __cdecl _setSBUpLow( void )
{
    uint8_t v7;
    uint32_t v9;
    int32_t v10;
    uint16_t local_0x518; // [esp-1304]
    uint8_t local_0x318; // [esp-792]
    uint8_t local_0x218; // [esp-536]
    uint8_t local_0x118; // [esp-280]
    uint32_t local_0x18; // [esp-24]
    uint8_t local_0x12; // [esp-18]
    uint8_t local_0x11; // [esp-17]
    int32_t v3; // ecx
    int v1; // eax
    void * v6; // edx
    uint8_t v4; // eax
    uint8_t v13; // edx
    uint8_t v5; // eax
    uint8_t v14; // edx
    void * v11; // ecx
    uint32_t v12; // eax
    uint32_t v2; // eax
    uint8_t * v8;

    v1 = GetCPInfo( data_0x9748, &local_0x18 );
    if( v1 == 1 ) {
        v4 = 0;
        while( 1 ) {
            local_0x118[v4] = v4;
            if( v4 > 254 ) {
                break;
            }
            v4 += 1;
        }
        v5 = v4 + 1 & 0xFFFFFF00 | local_0x12;
        local_0x118 = 32;
        if( (v4 + 1 & 0xFFFFFF00 | local_0x12) != 0 ) {
            v6 = &local_0x11;
            while( 1 ) {
                v7 = v5;
                v8 = &local_0x118[v5];
                if( v5 <= *v6 ) {
                    v9 = (*v6 - v5 + 1) / 4;
                    while( v9 != 0 ) {
                        v8[0] = (uint8_t)538976288;
                        v8 = &v8[4];
                        v9 += 4294967295;
                    }
                    v10 = *v6 - v5 + 1 & 0x3;
                    while( v10 != 0 ) {
                        v8[0] = 32;
                        v8 = &v8[1];
                        v10 += -1;
                    }
                    v7 = (uint8_t)538976288;
                }
                v5 = v7 & 0xFFFFFF00 | *((uint8_t *)v6 + 1);
                if( (v7 & 0xFFFFFF00 | *((uint8_t *)v6 + 1)) == 0 ) {
                    break;
                }
                (uint8_t *)v6 += 2;
            }
        }
        ___crtGetStringTypeA( 1, &local_0x118, 256, &local_0x518, data_0x9748, data_0x9964, 0 );
        ___crtLCMapStringA( data_0x9964, 256, &local_0x118, 256, &local_0x218, 256, data_0x9748, 0 );
        ___crtLCMapStringA( data_0x9964, 512, &local_0x118, 256, &local_0x318, 256, data_0x9748, 0 );
        v11 = &local_0x518;
        v12 = 0;
        while( 1 ) {
            v14 = v13 & 0xFFFF0000 | *v11;
            if( ((v13 & 0xFFFF0000 | *v11) & 0x1) != 0 ) {
                *(&data_0x9861 + v12) |= 0x10;
                v14 = (v13 & 0xFFFF0000 | *v11) & 0xFFFFFF00 | local_0x218[v12];
            } else {
                if( ((v13 & 0xFFFF0000 | *v11) & 0x2) == 0 ) {
                    *(&data_0x9760 + v12) = 0;
                    goto node_117;
                } else {
                    *(&data_0x9861 + v12) |= 0x20;
                    v14 = (v13 & 0xFFFF0000 | *v11) & 0xFFFFFF00 | local_0x318[v12];
                }
            }
            *(&data_0x9760 + v12) = v14;
            node_117:
            if( v12 > 254 ) {
                break;
            }
            v13 = v14;
            (uint8_t *)v11 += 2;
            v12 += 1;
        }
    } else {
        v2 = 0;
        while( 1 ) {
            if( v2 > 64 && v2 < 91 ) {
                *(&data_0x9861 + v2) |= 0x10;
                v3 = (v3 & 0xFFFFFF00 | (uint8_t)v2) & 0xFFFFFF00 | (uint8_t)(v3 & 0xFFFFFF00 | (uint8_t)v2) + 32;
            } else if( v2 > 96 && v2 < 123 ) {
                *(&data_0x9861 + v2) |= 0x20;
                v3 = (v3 & 0xFFFFFF00 | (uint8_t)v2) & 0xFFFFFF00 | (uint8_t)(v3 & 0xFFFFFF00 | (uint8_t)v2) - 32;
            } else {
                *(&data_0x9760 + v2) = 0;
                goto node_74;
            }
            *(&data_0x9760 + v2) = v3;
            node_74:
            if( v2 > 254 ) {
                break;
            }
            v2 += 1;
        }
    }
}

// VA=0x40457b
void __cdecl ___initmbctable( void )
{
    if( data_0x9AA8 == 0 ) {
        func_0x41B7( 4294967293 );
        data_0x9AA8 = 1;
    }
}

// VA=0x4045a0
int32_t __cdecl _strcpy( int32_t p1, int32_t p2 )
{
    uint32_t v4;
    uint32_t edx; // edx
    int32_t v2; // edi
    int32_t v5; // cc_dst
    int32_t v3;
    int32_t v1;

    if( (p2 & 0x3) == 0 ) {
        v1 = p1;
    } else {
        v2 = p1;
        while( 1 ) {
            v1 = v2 + 1;
            v3 = p2 + 1;
            v4 = edx & 0xFFFFFF00 | *p2;
            if( (uint8_t)(edx & 0xFFFFFF00 | *p2) == 0 ) {
                *v2 = v4;
                return p1;
            }
            *v2 = edx & 0xFFFFFF00 | *p2;
            if( (p2 + 1 & 0x3) == 0 ) {
                p2 = v3;
                break;
            }
            v2 += 1;
            edx = v4;
            p2 += 1;
        }
    }
    while( 1 ) {
        v4 = *p2;
        v5 = (~*p2 ^ 2130640639 + *p2) & 0x81010100;
        if( v5 == 0 ) {
            node_35:
            *v1 = v4;
            v1 += 4;
            p2 += 4;
            continue;
        }
        if( (uint8_t)v4 == 0 ) {
            break;
        }
        if( (uint8_t)((uint16_t)v4 >> 8 & (uint16_t)v4 >> 8) == 0 ) {
            *v1 = v4;
            return p1;
        }
        if( (v4 & 0xFF0000) == 0 ) {
            *v1 = v4;
            *(v1 + 2) = 0;
            return p1;
        }
        if( (v4 & 0xFF000000) == 0 ) {
            *v1 = v4;
            return p1;
        }
        goto node_35;
    }
    v2 = v1;
    *v2 = v4;
    return p1;
}

// VA=0x4045b0
int32_t __cdecl _strcat( int32_t p1, int32_t p2 )
{
    int32_t v8;
    uint32_t v4;
    int32_t eax; // eax
    int32_t v2; // ecx
    int32_t v3; // eax
    int32_t v12; // cc_dst
    uint8_t v13; // cc_dst
    int32_t v5; // cc_dst
    int32_t v7;
    int32_t v11;
    int32_t v10;
    int32_t v9;
    int32_t v1;
    int32_t v6;

    if( (p1 & 0x3) == 0 ) {
        v1 = p1;
    } else {
        v2 = p1;
        while( 1 ) {
            v1 = v2 + 1;
            v3 = eax & 0xFFFFFF00 | *v2;
            if( (uint8_t)(eax & 0xFFFFFF00 | *v2) == 0 ) {
                goto node_26;
            } else {
                if( (v2 + 1 & 0x3) == 0 ) {
                    break;
                }
                v2 += 1;
                eax = v3;
            }
        }
    }
    while( 1 ) {
        v9 = v1 + 1;
        v10 = v1 + 2;
        v11 = v1 + 4;
        v8 = 2130640639 + *v1;
        v12 = (~*v1 ^ 2130640639 + *v1) & 0x81010100;
        if( v12 == 0 ) {
            v1 += 4;
            continue;
        }
        if( (uint8_t)*v1 == 0 ) {
            break;
        }
        v13 = (uint8_t)(uint16_t)*v1 >> 8 & (uint16_t)*v1 >> 8;
        if( v13 == 0 ) {
            v1 = v9;
            break;
        }
        if( (*v1 & 0xFF0000) == 0 ) {
            v1 = v10;
            break;
        }
        if( (*v1 & 0xFF000000) == 0 ) {
            v1 = v11;
            goto node_26;
        } else {
            v1 += 4;
        }
    }
    node_26:
    v1 += -1;
    if( (p2 & 0x3) == 0 ) {
        while( 1 ) {
            node_53:
            v4 = *p2;
            v5 = (~*p2 ^ 2130640639 + *p2) & 0x81010100;
            if( v5 == 0 ) {
                node_80:
                *v1 = v4;
                v1 += 4;
                p2 += 4;
                continue;
            }
            if( (uint8_t)v4 == 0 ) {
                break;
            }
            if( (uint8_t)((uint16_t)v4 >> 8 & (uint16_t)v4 >> 8) == 0 ) {
                *v1 = v4;
                return p1;
            }
            if( (v4 & 0xFF0000) == 0 ) {
                *v1 = v4;
                *(v1 + 2) = 0;
                return p1;
            }
            if( (v4 & 0xFF000000) == 0 ) {
                *v1 = v4;
                return p1;
            }
            goto node_80;
        }
    } else {
        while( 1 ) {
            v6 = v1 + 1;
            v7 = p2 + 1;
            v4 = v8 & 0xFFFFFF00 | *p2;
            if( (uint8_t)(v8 & 0xFFFFFF00 | *p2) == 0 ) {
                break;
            }
            *v1 = v8 & 0xFFFFFF00 | *p2;
            if( (p2 + 1 & 0x3) == 0 ) {
                v1 = v6;
                p2 = v7;
                goto node_53;
            } else {
                v1 += 1;
                v8 = v4;
                p2 += 1;
            }
        }
    }
    *v1 = v4;
    return p1;
}

// VA=0x404690
int32_t __cdecl _strlen( int32_t p1 )
{
    int32_t eax; // eax
    int32_t v2; // ecx
    int32_t v3; // eax
    int32_t v5; // cc_dst
    uint8_t v6; // cc_dst
    int32_t v4;
    int32_t v1;

    if( (p1 & 0x3) == 0 ) {
        v1 = p1;
    } else {
        v2 = p1;
        while( 1 ) {
            v1 = v2 + 1;
            v3 = eax & 0xFFFFFF00 | *v2;
            if( (uint8_t)(eax & 0xFFFFFF00 | *v2) == 0 ) {
                return v1 + -1 - p1;
            }
            if( (v2 + 1 & 0x3) == 0 ) {
                break;
            }
            v2 += 1;
            eax = v3;
        }
    }
    while( 1 ) {
        v4 = v1 + 4;
        v5 = (~*v1 ^ 2130640639 + *v1) & 0x81010100;
        if( v5 == 0 ) {
            v1 += 4;
            continue;
        }
        if( (uint8_t)*v1 == 0 ) {
            return v1 - p1;
        }
        v6 = (uint8_t)(uint16_t)*v1 >> 8 & (uint16_t)*v1 >> 8;
        if( v6 == 0 ) {
            return v1 + 1 - p1;
        }
        if( (*v1 & 0xFF0000) == 0 ) {
            return v1 + 2 - p1;
        }
        if( (*v1 & 0xFF000000) == 0 ) {
            v1 = v4;
            return v1 + -1 - p1;
        }
        v1 += 4;
    }
    return v1 + -1 - p1;
    return v1 - p1;
}

// VA=0x40470b
int32_t __cdecl func_0x470B( int32_t p1, int32_t p2, int32_t p3 )
{
    int32_t v1; // eax

    return func_0x4722( p1, p2, p3, 0 );
}

// VA=0x404722
int32_t __cdecl func_0x4722( int32_t p1, int32_t p2, int32_t p3, int32_t p4 )
{
    int32_t v7;
    int32_t v12;
    int32_t v13;
    int32_t v6;
    int32_t ebx; // ebx
    int32_t v4; // eax
    void * v1; // esi
    uint8_t v2; // ebx
    void * v9; // eax
    void * v10; // eax
    int32_t v11; // eax
    int32_t v3;
    int32_t v5;
    int32_t v8;

    v1 = p1 + 1;
    v2 = ebx & 0xFFFFFF00 | *p1;
    while( 1 ) {
        v3 = (uint8_t *)v1 + 1;
        if( data_0x953C > 1 ) {
            __isctype( v2, 8 );
        } else {
            v4 = (v2 & 0xFFFFFF00 | *(data_0x9330 + v2 * 2)) & 0x8;
        }
        if( v4 == 0 ) {
            break;
        }
        v2 = v2 & 0xFFFFFF00 | *v1;
        (uint8_t *)v1 += 1;
    }
    if( v2 == 45 ) {
        p4 |= 0x2;
    } else if( v2 != 43 ) {
        v3 = v1;
        goto node_37;
    }
    v2 = v2 & 0xFFFFFF00 | *v1;
    node_37:
    v5 = v3 + 2;
    if( p3 >= 0 && p3 != 1 && p3 < 37 ) {
        if( p3 != 0 ) {
            v7 = p3;
        } else {
            if( v2 == 48 ) {
                v7 = *v3;
                if( v7 != 120 && v7 != 88 ) {
                    p3 = 8;
                    goto node_97;
                } else {
                    p3 = 16;
                }
            } else {
                p3 = 10;
                goto node_97;
            }
        }
        if( p3 == 16 && v2 == 48 && ((uint8_t)(v7 & 0xFFFFFF00 | *v3) == 120 || (uint8_t)(v7 & 0xFFFFFF00 | *v3) == 88) ) {
            v2 = *(v3 + 1);
            v3 = v5;
        }
        node_97:
        v6 = 0;
        while( 1 ) {
            v8 = v3 + -1;
            if( data_0x953C > 1 ) {
                __isctype( v2, 4 );
            } else {
                v9 = (data_0x9330 & 0xFFFFFF00 | *(data_0x9330 + v2 * 2)) & 0x4;
            }
            if( v9 != 0 ) {
                v12 = (int8_t)v2 - 48;
            } else {
                if( data_0x953C > 1 ) {
                    __isctype( v2, 259 );
                } else {
                    v10 = (data_0x9330 & 0xFFFF0000 | *(data_0x9330 + v2 * 2)) & 0x103;
                }
                if( v10 == 0 ) {
                    goto node_161;
                } else {
                    v11 = __toupper_lk( (int8_t)v2 );
                    v12 = v11 - 55;
                }
            }
            if( v12 >= p3 ) {
                break;
            }
            v13 = p4 | 0x8;
            if( v6 >= -1 / p3 && (-1 / p3 != v6 || v12 > -1 % p3) ) {
                p4 = p4 | 0x8 | 0x4;
            } else {
                v6 = p3 * v6 + v12;
                p4 = v13;
            }
            v2 = v2 & 0xFFFFFF00 | *v3;
            v3 += 1;
        }
        node_161:
        if( (uint8_t)(p4 & 0x8) == 0 ) {
            if( p2 == 0 ) {
                v3 = v8;
            } else {
                v3 = p1;
            }
            v6 = 0;
        } else if( (uint8_t)(p4 & 0x4) == 0 && ((uint8_t)(p4 & 0x1) != 0 || ((p4 & 0x2) == 0 || v6 < -2147483647) && ((p4 & 0x2) != 0 || v6 < -2147483648)) ) {
            v3 = v8;
        } else {
            data_0x95A8 = 34;
            if( (uint8_t)(p4 & 0x1) == 0 ) {
                v6 = ((uint8_t)(p4 & 0xFFFFFF00 | p4 & 0x2) != 0) + (uint8_t)2147483647;
                v3 = v8;
            } else {
                v3 = v8;
                v6 = -1;
            }
        }
        if( p2 != 0 ) {
            *p2 = v3;
        }
        if( (uint8_t)(p4 & 0x2) != 0 ) {
            v6 = -v6;
        }
    } else {
        if( p2 != 0 ) {
            *p2 = p1;
        }
        v6 = 0;
    }
    return v6;
}

// VA=0x404940
int32_t __cdecl _strchr( int32_t p1, int8_t p2 )
{
    int32_t v1;
    int32_t v2;
    int32_t v3;
    int32_t ecx; // ecx
    int32_t v7; // ecx
    uint8_t v4; // cc_dst
    uint8_t v5; // cc_dst
    int32_t v6;

    if( (p1 & 0x3) == 0 ) {
        node_15:
        v1 = ((uint8_t)p2 | (uint8_t)p2 << 8) << 16 | (uint8_t)p2 | (uint8_t)p2 << 8;
        while( 1 ) {
            v2 = ~*p1 ^ 2130640639 + *p1;
            v3 = (~(*p1 ^ v1) ^ 2130640639 + (*p1 ^ v1)) & 0x81010100;
            if( v3 == 0 ) {
                if( (v2 & 0x81010100) == 0 ) {
                    p1 += 4;
                    continue;
                }
                if( (v2 & 0x1010100) != 0 || (2130640639 + *p1 & 0x80000000) == 0 ) {
                    return 0;
                }
                p1 += 4;
            } else if( (uint8_t)v1 == (uint8_t)*p1 ) {
                return p1;
            } else if( (uint8_t)*p1 == 0 ) {
                return 0;
            } else if( (uint8_t)v1 != (uint8_t)*p1 / 256 ) {
                v4 = (uint8_t)(uint16_t)*p1 >> 8 & (uint16_t)*p1 >> 8;
                if( v4 == 0 ) {
                    return 0;
                } else if( (uint8_t)v1 == (uint8_t)*p1 >> 16 ) {
                    return p1 + 2;
                } else if( (uint8_t)*p1 >> 16 == 0 ) {
                    return 0;
                } else if( (uint8_t)v1 != (uint8_t)(*p1 >> 16) / 256 ) {
                    v5 = (uint8_t)(uint16_t)*p1 >> 16 >> 8 & (uint16_t)*p1 >> 16 >> 8;
                    if( v5 == 0 ) {
                        return 0;
                    }
                    p1 += 4;
                } else {
                    return p1 + 3;
                }
            } else {
                return p1 + 1;
            }
        }
    }
    while( 1 ) {
        v6 = p1 + 1;
        v7 = ecx & 0xFFFFFF00 | *p1;
        if( (uint8_t)p2 == (uint8_t)(ecx & 0xFFFFFF00 | *p1) ) {
            break;
        } else if( (uint8_t)(ecx & 0xFFFFFF00 | *p1) == 0 ) {
            return 0;
        } else if( (p1 + 1 & 0x3) == 0 ) {
            p1 = v6;
            goto node_15;
        } else {
            p1 += 1;
            ecx = v7;
        }
    }
    return p1;
    return 0;
}

// VA=0x404a00
int32_t __cdecl func_0x4A00( int32_t p1, int32_t p2 )
{
    int32_t v2;
    int32_t v3;
    int32_t v4;
    int32_t edx; // edx
    int32_t v12; // ecx
    uint32_t v1; // edx
    int32_t v8; // ecx
    int32_t v13; // cc_dst
    uint32_t eax; // eax
    uint32_t v10; // eax
    uint8_t v5; // cc_dst
    uint8_t v6; // cc_dst
    int32_t v9;
    int32_t v11;
    int32_t v7;

    if( (uint8_t)(edx & 0xFFFFFF00 | *p2) == 0 ) {
        return p1;
    }
    v1 = (edx & 0xFFFFFF00 | *p2) & 0xFFFF00FF | *(p2 + 1) << 8;
    if( (uint8_t)((uint16_t)v1 >> 8 & (uint16_t)v1 >> 8) != 0 ) {
        while( 1 ) {
            node_15:
            v9 = p1 + 1;
            v10 = eax & 0xFFFFFF00 | *p1;
            if( (uint8_t)v1 == (uint8_t)(eax & 0xFFFFFF00 | *p1) ) {
                p1 = v9;
            } else {
                if( (uint8_t)(eax & 0xFFFFFF00 | *p1) == 0 ) {
                    return 0;
                }
                goto node_42;
            }
            node_30:
            v11 = p1 + 1;
            eax = v10 & 0xFFFFFF00 | *p1;
            if( (uint8_t)v1 / 256 == (uint8_t)(v10 & 0xFFFFFF00 | *p1) ) {
                v12 = p2;
                while( 1 ) {
                    v13 = (uint16_t)(eax & 0xFFFF00FF | *(v12 + 2) << 8) >> 8 & (uint16_t)(eax & 0xFFFF00FF | *(v12 + 2) << 8) >> 8;
                    if( (uint8_t)v13 == 0 ) {
                        break;
                    }
                    eax = (eax & 0xFFFF00FF | *(v12 + 2) << 8) & 0xFFFFFF00 | *v11;
                    if( (uint8_t)eax / 256 == (uint8_t)eax ) {
                        if( (uint8_t)(eax & 0xFFFFFF00 | *(v12 + 3)) == 0 ) {
                            break;
                        }
                        eax = (eax & 0xFFFFFF00 | *(v12 + 3)) & 0xFFFF00FF | *(v11 + 1) << 8;
                        if( (uint8_t)eax / 256 == (uint8_t)eax ) {
                            v11 += 2;
                            v12 += 2;
                            continue;
                        }
                    }
                    goto node_15;
                }
                return p1 + -1;
            }
            p1 = v11;
            v10 = eax;
            node_45:
            if( (uint8_t)v1 == (uint8_t)v10 ) {
                goto node_30;
            } else {
                if( (uint8_t)v10 == 0 ) {
                    return 0;
                }
                v9 = p1;
            }
            node_42:
            p1 = v9 + 1;
            v10 = v10 & 0xFFFFFF00 | *v9;
            goto node_45;
        }
    } else {
        if( (p1 & 0x3) == 0 ) {
            node_57:
            v2 = ((uint8_t)v1 | (uint8_t)v1 << 8) << 16 | (uint8_t)v1 | (uint8_t)v1 << 8;
            while( 1 ) {
                v3 = ~*p1 ^ 2130640639 + *p1;
                v4 = (~(*p1 ^ v2) ^ 2130640639 + (*p1 ^ v2)) & 0x81010100;
                if( v4 == 0 ) {
                    if( (v3 & 0x81010100) == 0 ) {
                        p1 += 4;
                        continue;
                    }
                    if( (v3 & 0x1010100) != 0 || (2130640639 + *p1 & 0x80000000) == 0 ) {
                        return 0;
                    }
                    p1 += 4;
                } else if( (uint8_t)v2 == (uint8_t)*p1 ) {
                    return p1;
                } else if( (uint8_t)*p1 == 0 ) {
                    return 0;
                } else if( (uint8_t)v2 != (uint8_t)*p1 / 256 ) {
                    v5 = (uint8_t)(uint16_t)*p1 >> 8 & (uint16_t)*p1 >> 8;
                    if( v5 == 0 ) {
                        return 0;
                    } else if( (uint8_t)v2 == (uint8_t)*p1 >> 16 ) {
                        return p1 + 2;
                    } else if( (uint8_t)*p1 >> 16 == 0 ) {
                        return 0;
                    } else if( (uint8_t)v2 != (uint8_t)(*p1 >> 16) / 256 ) {
                        v6 = (uint8_t)(uint16_t)*p1 >> 16 >> 8 & (uint16_t)*p1 >> 16 >> 8;
                        if( v6 == 0 ) {
                            return 0;
                        }
                        p1 += 4;
                    } else {
                        return p1 + 3;
                    }
                } else {
                    return p1 + 1;
                }
            }
        }
        while( 1 ) {
            v7 = p1 + 1;
            v8 = p2 & 0xFFFFFF00 | *p1;
            if( (uint8_t)v1 == (uint8_t)(p2 & 0xFFFFFF00 | *p1) ) {
                break;
            } else if( (uint8_t)(p2 & 0xFFFFFF00 | *p1) == 0 ) {
                return 0;
            } else if( (p1 + 1 & 0x3) == 0 ) {
                p1 = v7;
                goto node_57;
            } else {
                p1 += 1;
                p2 = v8;
            }
        }
        return p1;
        return 0;
    }
    return 0;
}

// VA=0x404a80
int32_t __cdecl _strncmp( int32_t p1, int32_t p2, int32_t p3 )
{
}

// VA=0x404ac0
void __cdecl __alloca_probe( int32_t p1 )
{
}

// VA=0x404aef
void __cdecl ___crtMessageBoxA( int32_t p1, int32_t p2, int32_t p3 )
{
    uint32_t * v7;
    int32_t v4; // [esp-16]
    int32_t v1; // [esp-12]
    int32_t edi; // edi
    struct HINSTANCE__ * hModule; // eax
    int (__stdcall * v8)( ... ); // eax
    int (__stdcall * v9)( ... ); // eax
    int (__stdcall * v10)( ... ); // eax
    int32_t v3; // eax
    int32_t v5; // eax
    void * v2; // esp
    void * v6;

    v1 = edi;
    if( data_0x9714 == 0 ) {
        hModule = LoadLibraryA( "user32.dll" );
        if( hModule != 0 ) {
            v7 = GetProcAddress;
            v8 = v7( hModule, "MessageBoxA" );
            data_0x9714 = v8;
            if( v8 != 0 ) {
                v9 = v7( hModule, "GetActiveWindow" );
                data_0x9718 = v9;
                v10 = v7( hModule, "GetLastActivePopup" );
                data_0x971C = v10;
                v4 = "GetLastActivePopup";
                goto node_15;
            }
        }
    } else {
        node_15:
        if( data_0x9718 == 0 ) {
            v2 = &v1;
            v3 = 0;
        } else {
            v3 = data_0x9718();
            if( v3 == 0 ) {
                v4 = &code_0x4B47+0x2;
                v2 = &v1;
            } else if( data_0x971C == 0 ) {
                v4 = &code_0x4B47+0x2;
                v2 = &v1;
            } else {
                v5 = data_0x971C( v3 );
                v4 = v3;
                v2 = v6;
                v3 = v5;
            }
        }
        v6 = &v4;
        *((uint8_t *)v2 + (uint8_t)4294967292) = *((uint8_t *)v2 + 24);
        *((uint8_t *)v2 + (uint8_t)4294967288) = *((uint8_t *)v2 + 20);
        *((uint8_t *)v2 + (uint8_t)4294967284) = *((uint8_t *)v2 + 16);
        *((uint8_t *)v2 + (uint8_t)4294967280) = v3;
        *((uint8_t *)v2 + (uint8_t)4294967276) = &code_0x4B70;
        data_0x9714( *((uint8_t *)v2 + (uint8_t)4294967280), *((uint8_t *)v2 + (uint8_t)4294967284), *((uint8_t *)v2 + (uint8_t)4294967288), *((uint8_t *)v2 + (uint8_t)4294967292) );
    }
}

// VA=0x404b80
int32_t __cdecl _strncpy( int32_t p1, int32_t p2, int32_t p3 )
{
    int32_t v6;
    int32_t v11;
    int32_t v10;
    int32_t v4;
    int32_t v7;
    int32_t v18;
    int32_t v1; // edi
    int32_t v5; // eax
    int32_t v2; // edi
    uint8_t v14; // cc_dst
    int32_t v8; // cf
    uint32_t v16; // cf
    int32_t v3;
    int32_t v13;
    int32_t v17;
    int32_t v15;
    int32_t v12;
    int32_t v9;

    if( p3 == 0 ) {
        return p1;
    }
    if( (p2 & 0x3) != 0 ) {
        v1 = p1;
        while( 1 ) {
            v2 = v1 + 1;
            v3 = p2 + 1;
            v5 = v4 & 0xFFFFFF00 | *p2;
            *v1 = v4 & 0xFFFFFF00 | *p2;
            v6 = p3 + -1;
            if( p3 == 1 ) {
                return p1;
            } else if( (uint8_t)(v4 & 0xFFFFFF00 | *p2) == 0 ) {
                if( (v1 + 1 & 0x3) == 0 ) {
                    node_111:
                    v7 = v6 >> 2;
                    v8 = v6 >> 1 & 0x1;
                    if( v6 >> 2 == 0 ) {
                        p3 = v6;
                        goto node_118;
                    } else {
                        goto node_156;
                    }
                } else {
                    while( 1 ) {
                        v9 = v2 + 1;
                        *v2 = v4 & 0xFFFFFF00 | *p2;
                        v10 = v6 + -1;
                        if( v6 == 1 ) {
                            return p1;
                        } else if( (v2 + 1 & 0x3) == 0 ) {
                            v2 = v9;
                            v6 = v10;
                            goto node_111;
                        } else {
                            v2 += 1;
                            v6 += -1;
                        }
                    }
                }
            } else if( (p2 + 1 & 0x3) == 0 ) {
                v11 = p3 + -1 >> 2;
                if( p3 + -1 >> 2 == 0 ) {
                    v12 = v2;
                    p2 = v3;
                    v4 = v5;
                    goto node_93;
                } else {
                    p2 = v3;
                    p3 = v6;
                    break;
                }
            } else {
                v1 += 1;
                p2 += 1;
                p3 += -1;
                v4 = v5;
            }
        }
    } else {
        if( p3 >> 2 == 0 ) {
            v12 = p1;
            goto node_43;
        } else {
            v2 = p1;
            v11 = p3 >> 2;
        }
    }
    while( 1 ) {
        v12 = v2 + 4;
        v13 = p2 + 4;
        v4 = ~*p2 ^ 2130640639 + *p2;
        if( (v4 & 0x81010100) == 0 ) {
            node_55:
            *v2 = *p2;
            if( v11 == 1 ) {
                p2 = v13;
                v6 = p3;
                break;
            }
            v2 += 4;
            p2 += 4;
            v11 += -1;
            continue;
        }
        if( (uint8_t)*p2 == 0 ) {
            *v2 = 0;
        } else {
            v14 = (uint8_t)(uint16_t)*p2 >> 8 & (uint16_t)*p2 >> 8;
            if( v14 == 0 ) {
                *v2 = *p2 & 0xFF;
            } else if( (*p2 & 0xFF0000) == 0 ) {
                *v2 = *p2 & 0xFFFF;
            } else if( (*p2 & 0xFF000000) == 0 ) {
                *v2 = *p2;
            } else {
                goto node_55;
            }
        }
        v7 = v11 + -1;
        if( v11 == 1 ) {
            v2 = v12;
            v6 = p3;
            goto node_149;
        } else {
            v2 = v12;
            v6 = p3;
            goto node_156;
        }
    }
    node_93:
    p3 = v6 & 0x3;
    if( (v6 & 0x3) == 0 ) {
        return p1;
    }
    while( 1 ) {
        node_43:
        v15 = v12 + 1;
        v5 = v4 & 0xFFFFFF00 | *p2;
        *v12 = v4 & 0xFFFFFF00 | *p2;
        if( (uint8_t)(v4 & 0xFFFFFF00 | *p2) == 0 ) {
            break;
        }
        if( p3 == 1 ) {
            return p1;
        }
        v12 += 1;
        p2 += 1;
        p3 += -1;
        v4 = v5;
    }
    v16 = 0;
    goto node_80;
    while( 1 ) {
        node_156:
        v17 = v2 + 4;
        *v2 = 0;
        if( v7 == 1 ) {
            break;
        }
        v2 += 4;
        v7 += -1;
    }
    v2 = v17;
    node_149:
    p3 = v6 & 0x3;
    if( (v6 & 0x3) != 0 ) {
        v8 = 0;
        v5 = v8;
        goto node_118;
    }
    return p1;
    node_118:
    v15 = v2 + 1;
    *v2 = v5;
    v16 = v8 != 0;
    node_80:
    v18 = p3 + -1;
    v8 = v16 != 0;
    if( p3 == 1 ) {
        return p1;
    } else {
        p3 = v18;
        v2 = v15;
        goto node_118;
    }
    return p1;
}

// VA=0x404c80
// Decompilation timed out after 15 seconds.

// VA=0x404ed7
int32_t __fastcall func_0x4ED7( int32_t p1, int32_t p2 )
{
    int32_t v3;
    int32_t v4;
    void * ebp; // ebp
    int32_t esi; // esi
    int32_t edi; // edi
    int32_t v5; // eax
    int32_t v2;
    int32_t v1;

    if( p1 > 7 ) {
        v1 = edi - 3;
        v2 = esi - 3;
        while( p1 != 0 ) {
            *v1 = *v2;
            v1 += -4;
            v2 += -4;
            p1 += -1;
        }
        switch( p2 ) {
            case 0: {
                return *((uint8_t *)ebp + 8);
            }
            case 1: {
                *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
                return *((uint8_t *)ebp + 8);
            }
            case 2: {
                *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
                *(v1 + 2) = (v4 & 0xFFFFFF00 | *(v2 + 3)) & 0xFFFFFF00 | *(v2 + 2);
                return *((uint8_t *)ebp + 8);
            }
            case 3: {
                *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
                v5 = (v4 & 0xFFFFFF00 | *(v2 + 3)) & 0xFFFFFF00 | *(v2 + 2);
                *(v1 + 2) = v5;
                *(v1 + 1) = v5 & 0xFFFFFF00 | *(v2 + 1);
                return *((uint8_t *)ebp + 8);
            }
        }
    }
    v1 = edi - 3;
    v2 = esi - 3;
    do {
        v3 = -p1;
        switch( p1 ) {
            case 7: {
                goto node_25;
                break;
            }
            case 6: {
                goto node_27;
                break;
            }
            case 5: {
                goto node_29;
                break;
            }
            case 4: {
                goto node_31;
                break;
            }
            case 3: {
                goto node_33;
                break;
            }
            case 2: {
                goto node_35;
                break;
            }
            case 1: {
                goto node_37;
                break;
            }
            case 0: {
                goto node_39;
                break;
            }
            default: {
                p1 = v3;
                while( p1 != 0 ) {
                    *v1 = *v2;
                    v1 += -4;
                    v2 += -4;
                    p1 += -1;
                    continue;
                }
                if( p2 == 0 ) {
                    return *((uint8_t *)ebp + 8);
                } else if( p2 == 1 ) {
                    *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
                    return *((uint8_t *)ebp + 8);
                } else if( p2 == 2 ) {
                    *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
                    *(v1 + 2) = (v4 & 0xFFFFFF00 | *(v2 + 3)) & 0xFFFFFF00 | *(v2 + 2);
                    return *((uint8_t *)ebp + 8);
                }
                break;
            }
        }
    } while( p2 != 3 );
    goto node_24;
    node_25:
    *(v1 + -p1 * 4 + 28) = *(v2 + -p1 * 4 + 28);
    node_27:
    *(v1 + -p1 * 4 + 24) = *(v2 + -p1 * 4 + 24);
    node_29:
    *(v1 + -p1 * 4 + 20) = *(v2 + -p1 * 4 + 20);
    node_31:
    *(v1 + -p1 * 4 + 16) = *(v2 + -p1 * 4 + 16);
    node_33:
    *(v1 + -p1 * 4 + 12) = *(v2 + -p1 * 4 + 12);
    node_35:
    *(v1 + -p1 * 4 + 8) = *(v2 + -p1 * 4 + 8);
    node_37:
    *(v1 + -p1 * 4 + 4) = *(v2 + -p1 * 4 + 4);
    v4 = -p1 * 4;
    v1 += -p1 * 4;
    v2 += -p1 * 4;
    node_39:
    if( p2 == 0 ) {
        return *((uint8_t *)ebp + 8);
    } else if( p2 == 1 ) {
        *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
        return *((uint8_t *)ebp + 8);
    } else if( p2 == 2 ) {
        *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
        *(v1 + 2) = (v4 & 0xFFFFFF00 | *(v2 + 3)) & 0xFFFFFF00 | *(v2 + 2);
        return *((uint8_t *)ebp + 8);
    } else if( p2 != 3 ) {
        goto &data_0x4F5E;
    }
    node_24:
    *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
    v5 = (v4 & 0xFFFFFF00 | *(v2 + 3)) & 0xFFFFFF00 | *(v2 + 2);
    *(v1 + 2) = v5;
    *(v1 + 1) = v5 & 0xFFFFFF00 | *(v2 + 1);
    return *((uint8_t *)ebp + 8);
    *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
    *(v1 + 2) = (v4 & 0xFFFFFF00 | *(v2 + 3)) & 0xFFFFFF00 | *(v2 + 2);
    return *((uint8_t *)ebp + 8);
    *(v1 + 3) = v4 & 0xFFFFFF00 | *(v2 + 3);
    return *((uint8_t *)ebp + 8);
    return *((uint8_t *)ebp + 8);
}

// VA=0x404fc0
int32_t __cdecl _memset( int32_t p1, int8_t p2, int32_t p3 )
{
    int32_t v8;
    int32_t v6; // edi
    int32_t v4; // edx
    int32_t v7; // ecx
    uint8_t v3; // eax
    uint32_t v5; // cf
    int32_t v1; // cf
    int32_t v2;

    if( p3 == 0 ) {
        return p1;
    }
    if( p3 < 4 ) {
        v1 = p3 < 4;
        v2 = p1;
        v3 = (uint8_t)p2;
    } else {
        if( (-p1 & 0x3) == 0 ) {
            v2 = p1;
            v4 = p3;
        } else {
            v5 = p3 < (-p1 & 0x3);
            v6 = p1;
            v7 = -p1 & 0x3;
            while( 1 ) {
                v2 = v6 + 1;
                *v6 = (uint8_t)p2;
                if( v7 == 1 ) {
                    break;
                }
                v5 = v5 != 0;
                v6 += 1;
                v7 += -1;
            }
            v4 = p3 - (-p1 & 0x3);
        }
        p3 = v4 & 0x3;
        v8 = v4 >> 2;
        v1 = v4 >> 1 & 0x1;
        if( v4 >> 2 == 0 ) {
            v3 = ((uint8_t)p2 * (uint8_t)257 << 16) + (uint8_t)p2 * (uint8_t)257;
            goto node_19;
        } else {
            while( v8 != 0 ) {
                *v2 = ((uint8_t)p2 * (uint8_t)257 << 16) + (uint8_t)p2 * (uint8_t)257;
                v2 += 4;
                v8 += -1;
            }
        }
        if( (v4 & 0x3) == 0 ) {
            return p1;
        }
        v1 = 0;
        v3 = ((uint8_t)p2 * (uint8_t)257 << 16) + (uint8_t)p2 * (uint8_t)257;
    }
    while( 1 ) {
        node_19:
        *v2 = v3;
        if( p3 == 1 ) {
            break;
        }
        v1 = v1 != 0;
        v2 += 1;
        p3 += -1;
    }
    return p1;
}

// VA=0x405018
int32_t __cdecl ___crtLCMapStringA( int32_t p1, int32_t p2, int32_t p3, int32_t p4, int32_t p5, int32_t p6, int32_t p7, int32_t p8 )
{
    struct _EH_EXCEPTION_REGISTRATION_RECORD v3;
    struct _EH_EXCEPTION_REGISTRATION_RECORD v6;
    int32_t v1; // [esp-64]
    uint32_t local_0x3C; // [esp-60]
    uint32_t local_0x28; // [esp-40]
    uint32_t local_0x20; // [esp-32]
    struct _EH_EXCEPTION_REGISTRATION_RECORD ExceptionRegistration; // [esp-28]
    uint32_t local_0x4; // [esp-4]
    uint32_t ebp; // ebp
    uint32_t edi; // edi
    void * fs; // fs
    int v9; // eax
    int v10; // eax
    int v2; // eax
    int v4; // eax
    int v8; // eax
    int v7; // eax
    int v5; // eax

    local_0x4 = ebp;
    ExceptionRegistration.TryLevel = 4294967295;
    ExceptionRegistration.ScopeTable = &scope_table_1;
    ExceptionRegistration.Handler = &code_0x29A8;
    ExceptionRegistration.Next = *fs;
    *fs = &ExceptionRegistration.Next;
    local_0x3C = edi;
    ExceptionRegistration.SavedEsp = &local_0x3C;
    if( data_0x9740 == 0 ) {
        v9 = LCMapStringW( 0, 256, &data_0x6494, 1, 0, 0 );
        if( v9 == 0 ) {
            v10 = LCMapStringA( 0, 256, &data_0x6490, 1, 0, 0 );
            if( v10 != 0 ) {
                data_0x9740 = 2;
                goto node_15;
            }
        } else {
            data_0x9740 = 1;
            goto node_15;
        }
    } else {
        node_15:
        if( p4 > 0 ) {
            p4 = _strncnt( p3, p4 );
        }
        if( data_0x9740 == 2 ) {
            v5 = LCMapStringA( p1, p2, p3, p4, p5, p6 );
            *fs = ExceptionRegistration.Next;
            return v5;
        } else if( data_0x9740 == 1 ) {
            if( p7 == 0 ) {
                p7 = data_0x9738;
            }
            v1 = 0;
            v2 = MultiByteToWideChar( p7, (-(p8 != 0) & 0x8) + 1, p3, p4, 0, 0 );
            local_0x20 = v2;
            if( v2 != 0 ) {
                ExceptionRegistration.TryLevel = 0;
                v1 = &code_0x510D+0xD;
                v3.SavedEsp = &v1 - (v2 + v2 + 3 & 0xFFFFFF00 | (uint8_t)v2 + v2 + 3 & 0xFC);
                ExceptionRegistration.SavedEsp = v3.SavedEsp + 4;
                local_0x28 = v3.SavedEsp + 4;
                ExceptionRegistration.TryLevel = 4294967295;
                if( v3.SavedEsp != 4294967292 ) {
                    *(&local_0x4 + v3.SavedEsp) = v3.SavedEsp + 4;
                    *(&ExceptionRegistration.TryLevel + v3.SavedEsp) = p4;
                    *(&ExceptionRegistration.ScopeTable + v3.SavedEsp) = p3;
                    *(&ExceptionRegistration.Handler + v3.SavedEsp) = 1;
                    *(&ExceptionRegistration.Next + v3.SavedEsp) = p7;
                    *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp) = &code_0x5140+0x15;
                    v4 = MultiByteToWideChar( *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 4), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 8), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 12), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 16), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 20), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 24) );
                    if( v4 != 0 ) {
                        *(&local_0x4 + v3.SavedEsp) = 0;
                        *(&ExceptionRegistration.TryLevel + v3.SavedEsp) = v2;
                        *(&ExceptionRegistration.ScopeTable + v3.SavedEsp) = local_0x28;
                        *(&ExceptionRegistration.Handler + v3.SavedEsp) = p2;
                        *(&ExceptionRegistration.Next + v3.SavedEsp) = p1;
                        *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp) = &code_0x5159+0x12;
                        v5 = LCMapStringW( *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 4), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 8), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 12), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 16), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 20), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 24) );
                        if( v5 != 0 ) {
                            if( (uint8_t)(p2 & 4) == 0 ) {
                                ExceptionRegistration.TryLevel = 1;
                                v6.SavedEsp = v3.SavedEsp - (v5 + v5 + 3 & 0xFFFFFF00 | (uint8_t)v5 + v5 + 3 & 0xFC);
                                if( v6.SavedEsp != 4294967292 ) {
                                    *(&local_0x4 + v6.SavedEsp) = v6.SavedEsp + 4;
                                    *(&ExceptionRegistration.TryLevel + v6.SavedEsp) = local_0x20;
                                    *(&ExceptionRegistration.ScopeTable + v6.SavedEsp) = local_0x28;
                                    *(&ExceptionRegistration.Handler + v6.SavedEsp) = p2;
                                    *(&ExceptionRegistration.Next + v6.SavedEsp) = p1;
                                    *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp) = &code_0x51F2+0x14;
                                    v7 = LCMapStringW( *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp + 4), *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp + 8), *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp + 12), *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp + 16), *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp + 20), *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp + 24) );
                                    if( v7 != 0 ) {
                                        *(&local_0x4 + v6.SavedEsp) = 0;
                                        if( p6 == 0 ) {
                                            *(&ExceptionRegistration.TryLevel + v6.SavedEsp) = 0;
                                            *(&ExceptionRegistration.ScopeTable + v6.SavedEsp) = 0;
                                        } else {
                                            *(&ExceptionRegistration.TryLevel + v6.SavedEsp) = p6;
                                            *(&ExceptionRegistration.ScopeTable + v6.SavedEsp) = p5;
                                        }
                                        *(&ExceptionRegistration.Handler + v6.SavedEsp) = v5;
                                        *(&ExceptionRegistration.Next + v6.SavedEsp) = v6.SavedEsp + 4;
                                        *(&ExceptionRegistration.ExceptionPointers + v6.SavedEsp) = 544;
                                        *(&ExceptionRegistration.SavedEsp + v6.SavedEsp) = p7;
                                        *(&local_0x20 + v6.SavedEsp) = &code_0x521B+0x10;
                                        v5 = WideCharToMultiByte( *(&local_0x20 + v6.SavedEsp + 4), *(&local_0x20 + v6.SavedEsp + 8), *(&local_0x20 + v6.SavedEsp + 12), *(&local_0x20 + v6.SavedEsp + 16), *(&local_0x20 + v6.SavedEsp + 20), *(&local_0x20 + v6.SavedEsp + 24), *(&local_0x20 + v6.SavedEsp + 28), *(&local_0x20 + v6.SavedEsp + 32) );
                                        if( v5 != 0 ) {
                                            *fs = ExceptionRegistration.Next;
                                            return v5;
                                        }
                                    }
                                }
                            } else if( p6 == 0 ) {
                                *fs = ExceptionRegistration.Next;
                                return v5;
                            } else if( v5 <= p6 ) {
                                *(&local_0x4 + v3.SavedEsp) = p5;
                                *(&ExceptionRegistration.TryLevel + v3.SavedEsp) = v2;
                                *(&ExceptionRegistration.ScopeTable + v3.SavedEsp) = local_0x28;
                                *(&ExceptionRegistration.Handler + v3.SavedEsp) = p2;
                                *(&ExceptionRegistration.Next + v3.SavedEsp) = p1;
                                *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp) = &code_0x5188+0x16;
                                v8 = LCMapStringW( *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 4), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 8), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 12), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 16), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 20), *(&ExceptionRegistration.ExceptionPointers + v3.SavedEsp + 24) );
                                if( v8 != 0 ) {
                                    *fs = ExceptionRegistration.Next;
                                    return v5;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    v5 = 0;
    *fs = ExceptionRegistration.Next;
    return v5;
}

// VA=0x40523c
int32_t __cdecl _strncnt( int32_t p1, int32_t p2 )
{
    int32_t v1; // eax
    int32_t v2; // ecx
    int32_t v3;

    if( p2 == 0 ) {
        v1 = p1;
    } else {
        v2 = p2 + -1;
        v1 = p1;
        while( 1 ) {
            v3 = v1 + 1;
            if( *v1 == 0 ) {
                break;
            }
            if( v2 == 0 ) {
                v1 = v3;
                break;
            }
            v2 += -1;
            v1 += 1;
        }
    }
    if( *v1 == 0 ) {
        return v1 - p1;
    }
    return p2;
}

// VA=0x405267
int32_t __cdecl ___crtGetStringTypeA( int32_t p1, int32_t p2, int32_t p3, int32_t p4, int32_t p5, int32_t p6, int32_t p7 )
{
    uint32_t v1;
    int32_t v4;
    int32_t v2; // [esp-60]
    uint32_t local_0x38; // [esp-56]
    uint32_t local_0x24; // [esp-36]
    uint32_t local_0x20; // [esp-32]
    struct _EH_EXCEPTION_REGISTRATION_RECORD ExceptionRegistration; // [esp-28]
    uint32_t local_0x4; // [esp-4]
    uint32_t ebp; // ebp
    uint32_t edi; // edi
    void * fs; // fs
    int v7; // eax
    int v8; // eax
    int v3; // eax
    int v5; // eax
    int v6; // eax

    local_0x4 = ebp;
    ExceptionRegistration.TryLevel = 4294967295;
    ExceptionRegistration.ScopeTable = &scope_table;
    ExceptionRegistration.Handler = &code_0x29A8;
    ExceptionRegistration.Next = *fs;
    *fs = &ExceptionRegistration.Next;
    local_0x38 = edi;
    ExceptionRegistration.SavedEsp = &local_0x38;
    v1 = data_0x9744;
    if( v1 == 0 ) {
        v7 = GetStringTypeW( 1, &data_0x6494, 1, &local_0x20 );
        if( v7 == 0 ) {
            v8 = GetStringTypeA( 0, 1, &data_0x6490, 1, &local_0x20 );
            if( v8 == 0 ) {
                goto node_46;
            } else {
                v1 = 2;
            }
        } else {
            v1 = 1;
        }
        data_0x9744 = v1;
    }
    if( v1 == 2 ) {
        if( p6 == 0 ) {
            p6 = data_0x9728;
        }
        v6 = GetStringTypeA( p6, p1, p2, p3, p4 );
        *fs = ExceptionRegistration.Next;
        return v6;
    } else if( v1 == 1 ) {
        if( p5 == 0 ) {
            p5 = data_0x9738;
        }
        v2 = 0;
        v3 = MultiByteToWideChar( p5, (-(p7 != 0) & 0x8) + 1, p2, p3, 0, 0 );
        local_0x24 = v3;
        if( v3 != 0 ) {
            ExceptionRegistration.TryLevel = 0;
            v2 = &code_0x533C+0xF;
            v4 = &v2 - (v3 + v3 + 3 & 0xFFFFFF00 | (uint8_t)v3 + v3 + 3 & 0xFC);
            *(&local_0x4 + v4) = 0;
            *(&ExceptionRegistration.TryLevel + v4) = v4 + 4;
            *(&ExceptionRegistration.ScopeTable + v4) = &code_0x533C+0x1F;
            _memset( *(&ExceptionRegistration.ScopeTable + v4 + 4), *(&ExceptionRegistration.ScopeTable + v4 + 8), *(&ExceptionRegistration.ScopeTable + v4 + 12) );
            if( v4 != -4 ) {
                *(&local_0x4 + v4) = v4 + 4;
                *(&ExceptionRegistration.TryLevel + v4) = p3;
                *(&ExceptionRegistration.ScopeTable + v4) = p2;
                *(&ExceptionRegistration.Handler + v4) = 1;
                *(&ExceptionRegistration.Next + v4) = p5;
                *(&ExceptionRegistration.ExceptionPointers + v4) = &code_0x5373+0x15;
                v5 = MultiByteToWideChar( *(&ExceptionRegistration.ExceptionPointers + v4 + 4), *(&ExceptionRegistration.ExceptionPointers + v4 + 8), *(&ExceptionRegistration.ExceptionPointers + v4 + 12), *(&ExceptionRegistration.ExceptionPointers + v4 + 16), *(&ExceptionRegistration.ExceptionPointers + v4 + 20), *(&ExceptionRegistration.ExceptionPointers + v4 + 24) );
                if( v5 != 0 ) {
                    *(&local_0x4 + v4) = v5;
                    *(&ExceptionRegistration.TryLevel + v4) = v4 + 4;
                    *(&ExceptionRegistration.ScopeTable + v4) = p1;
                    *(&ExceptionRegistration.Handler + v4) = &code_0x538C+0xE;
                    v6 = GetStringTypeW( *(&ExceptionRegistration.Handler + v4 + 4), *(&ExceptionRegistration.Handler + v4 + 8), *(&ExceptionRegistration.Handler + v4 + 12), *(&ExceptionRegistration.Handler + v4 + 16) );
                    *fs = ExceptionRegistration.Next;
                    return v6;
                }
            }
        }
    }
    node_46:
    v6 = 0;
    *fs = ExceptionRegistration.Next;
    return v6;
}

// VA=0x4053b0
int32_t __cdecl __toupper_lk( int32_t p1 )
{
    int32_t v2; // [esp-16]
    uint8_t local_0x8; // [esp-8]
    uint8_t local_0x7; // [esp-7]
    uint8_t ecx; // ecx
    void * v4; // eax
    uint8_t v1; // cc_dst
    int32_t v3; // eax

    local_0x8 = ecx;
    if( data_0x9728 == 0 ) {
        if( p1 > 96 && p1 < 123 ) {
            p1 -= 32;
        }
    } else if( p1 > 255 ) {
        node_33:
        v1 = (uint8_t)*(data_0x9330 + (uint8_t)(p1 >> 8) * 2 + 1) & 0x80;
        if( v1 == 0 ) {
            p1 = (uint8_t)p1;
            v2 = 1;
        } else {
            p1 = (uint8_t)p1 >> 8;
            v2 = 2;
        }
        v3 = ___crtLCMapStringA( data_0x9728, 512, &p1, v2, &local_0x8, 3, 0, 1 );
        if( v3 != 0 ) {
            if( v3 == 1 ) {
                p1 = local_0x8;
            } else {
                p1 = local_0x7 << 8 | local_0x8;
            }
        }
    } else {
        if( data_0x953C > 1 ) {
            __isctype( p1, 2 );
        } else {
            v4 = (data_0x9330 & 0xFFFFFF00 | *(data_0x9330 + p1 * 2)) & 0x2;
        }
        if( v4 != 0 ) {
            goto node_33;
        }
    }
    return p1;
}

// VA=0x40547c
void __cdecl __isctype( int32_t p1, int32_t p2 )
{
    int32_t v2; // [esp-12]
    uint8_t local_0x8; // [esp-8]
    uint8_t v1; // cc_dst
    int32_t v3; // eax

    if( p1 > 255 ) {
        v1 = (uint8_t)*(data_0x9330 + (uint8_t)(p1 >> 8) * 2 + 1) & 0x80;
        if( v1 == 0 ) {
            local_0x8 = (uint8_t)p1;
            v2 = 1;
        } else {
            local_0x8 = (uint8_t)p1 >> 8;
            v2 = 2;
        }
        v3 = ___crtGetStringTypeA( 1, &local_0x8, v2, &p1, 0, 0, 1 );
        if( v3 == 0 ) {
            return;
        }
        return;
    }
}

// VA=0x4054f2
void __stdcall RtlUnwind_1( void * TargetFrame, void * TargetIp, struct _EXCEPTION_RECORD * ExceptionRecord, void * ReturnValue )
{
    goto RtlUnwind;
}



