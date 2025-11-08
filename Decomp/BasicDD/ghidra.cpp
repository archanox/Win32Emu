#include "out.h"



void FUN_00401010(void)

{
  FUN_00401460((undefined4 *)&DAT_00409550);
  return;
}



void FUN_00401020(void)

{
  FUN_00401873(0x401030);
  return;
}



undefined4 FUN_00401040(HINSTANCE param_1,undefined4 param_2,undefined4 param_3,int param_4)

{
  int iVar1;
  BOOL BVar2;
  tagMSG local_1c;
  
  DAT_0040957c = param_1;
  DAT_00409580 = FUN_00401200(param_4);
  if (DAT_00409580 == (HWND)0x0) {
    return 0xffffffff;
  }
  iVar1 = FUN_00401310();
  if (iVar1 < 0) {
    FUN_00401420();
    MessageBoxA(DAT_00409580,s_Could_start_DirectX_engine_in_yo_00407030,s_Error_0040709c,0x30);
    return 0;
  }
  FUN_00401640(&DAT_00409550,DAT_00409584,0x5dc,0x118);
  FUN_004014d0(&DAT_00409550,DAT_0040957c,0x65,0,0,0x5dc,0x118);
  while( true ) {
    while( true ) {
      BVar2 = PeekMessageA(&local_1c,(HWND)0x0,0,0,1);
      if (BVar2 != 0) break;
      FUN_00401130();
    }
    if (local_1c.message == 0x12) break;
    TranslateMessage(&local_1c);
    DispatchMessageA(&local_1c);
  }
  FUN_00401420();
  return 0;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_00401130(void)

{
  DWORD DVar1;
  int iVar2;
  
  DVar1 = GetTickCount();
  if (0x31 < DVar1 - _DAT_00409548) {
    FUN_00401730(&DAT_00409550,DAT_0040958c,0xf5,0xaa,DAT_00409590,DAT_00409594,0x96,0x8c);
    do {
      iVar2 = (**(code **)(*DAT_00409588 + 0x2c))(DAT_00409588,0,0);
      if (iVar2 == 0) break;
      if (iVar2 == -0x7789fe3e) {
        (**(code **)(*DAT_00409588 + 0x6c))(DAT_00409588);
        break;
      }
    } while (iVar2 == -0x7789fde4);
    DAT_00409590 = DAT_00409590 + 0x96;
    if (0x5db < DAT_00409590) {
      DAT_00409590 = 0;
      DAT_00409594 = DAT_00409594 + 0x8c;
      if (0x117 < DAT_00409594) {
        DAT_00409594 = 0;
      }
    }
    _DAT_00409548 = GetTickCount();
  }
  return;
}



HWND __cdecl FUN_00401200(int param_1)

{
  int nHeight;
  int nWidth;
  HWND pHVar1;
  HMENU hMenu;
  HINSTANCE hInstance;
  LPVOID lpParam;
  WNDCLASSA local_28;
  
  local_28.style = 3;
  local_28.lpfnWndProc = (WNDPROC)&LAB_004012d0;
  local_28.cbClsExtra = 0;
  local_28.cbWndExtra = 0;
  local_28.hInstance = DAT_0040957c;
  local_28.hIcon = LoadIconA(DAT_0040957c,(LPCSTR)0x7f00);
  local_28.hCursor = LoadCursorA((HINSTANCE)0x0,(LPCSTR)0x7f00);
  local_28.hbrBackground = (HBRUSH)GetStockObject(4);
  local_28.lpszMenuName = &DAT_00409598;
  local_28.lpszClassName = s_Basic_DD_004070a4;
  RegisterClassA(&local_28);
  lpParam = (LPVOID)0x0;
  hMenu = (HMENU)0x0;
  pHVar1 = (HWND)0x0;
  hInstance = DAT_0040957c;
  nHeight = GetSystemMetrics(1);
  nWidth = GetSystemMetrics(0);
  pHVar1 = CreateWindowExA(8,s_Basic_DD_004070a4,s_Basic_DD_004070a4,0x80000000,0,0,nWidth,nHeight,
                           pHVar1,hMenu,hInstance,lpParam);
  ShowWindow(pHVar1,param_1);
  UpdateWindow(pHVar1);
  SetFocus(pHVar1);
  return pHVar1;
}



int FUN_00401310(void)

{
  int iVar1;
  int **ppiVar2;
  int **ppiStack_d0;
  undefined4 *puStack_cc;
  undefined4 uStack_c8;
  int *apiStack_b0 [4];
  undefined4 uStack_a0;
  undefined4 uStack_9c;
  undefined4 *puStack_98;
  undefined *puStack_94;
  undefined4 uStack_90;
  undefined4 uStack_48;
  
  uStack_90 = 0;
  puStack_94 = &DAT_00406114;
  puStack_98 = &DAT_00409584;
  uStack_9c = 0;
  uStack_a0 = 0x401329;
  iVar1 = DirectDrawCreateEx();
  if (iVar1 != 0) {
    return -1;
  }
  uStack_a0 = 0x11;
  apiStack_b0[3] = (int *)DAT_00409580;
  apiStack_b0[2] = DAT_00409584;
  apiStack_b0[1] = (int *)0x40134b;
  iVar1 = (**(code **)(*DAT_00409584 + 0x50))();
  if (iVar1 != 0) {
    return -2;
  }
  apiStack_b0[1] = (int *)0x0;
  apiStack_b0[0] = (int *)0x0;
  iVar1 = (**(code **)(*DAT_00409584 + 0x54))();
  if (iVar1 != 0) {
    return -3;
  }
  ppiVar2 = apiStack_b0;
  for (iVar1 = 0x1f; iVar1 != 0; iVar1 = iVar1 + -1) {
    *ppiVar2 = (int *)0x0;
    ppiVar2 = ppiVar2 + 1;
  }
  uStack_c8 = 0;
  ppiStack_d0 = apiStack_b0;
  apiStack_b0[0] = (int *)0x7c;
  apiStack_b0[1] = (int *)0x21;
  uStack_48 = 0x218;
  uStack_9c = 1;
  puStack_cc = &DAT_00409588;
  iVar1 = (**(code **)(*DAT_00409584 + 0x18))(DAT_00409584);
  if (iVar1 != 0) {
    return -1;
  }
  ppiStack_d0 = (int **)0x4;
  puStack_cc = (undefined4 *)0x0;
  uStack_c8 = 0;
  iVar1 = (**(code **)(*DAT_00409588 + 0x30))(DAT_00409588,&ppiStack_d0,&DAT_0040958c);
  return -(uint)(iVar1 != 0);
}



void FUN_00401420(void)

{
  FUN_004017d0(0x409550);
  if (DAT_0040958c != (int *)0x0) {
    (**(code **)(*DAT_0040958c + 8))(DAT_0040958c);
  }
  if (DAT_00409588 != (int *)0x0) {
    (**(code **)(*DAT_00409588 + 8))(DAT_00409588);
  }
  if (DAT_00409584 != (int *)0x0) {
    (**(code **)(*DAT_00409584 + 8))(DAT_00409584);
  }
  return;
}



void __fastcall FUN_00401460(undefined4 *param_1)

{
  *param_1 = &PTR_FUN_00406110;
  param_1[10] = 0;
  param_1[7] = 0xffffffff;
  return;
}



undefined4 * __thiscall FUN_00401480(void *this,byte param_1)

{
  FUN_004014a0((undefined4 *)this);
  if ((param_1 & 1) != 0) {
    FUN_004018b4((undefined *)this);
  }
  return (undefined4 *)this;
}



void __fastcall FUN_004014a0(undefined4 *param_1)

{
  *param_1 = &PTR_FUN_00406110;
  if (param_1[10] != 0) {
    OutputDebugStringA(s_Surface_Destroyed_004070b0);
    (**(code **)(*(int *)param_1[10] + 8))((int *)param_1[10]);
    param_1[10] = 0;
  }
  return;
}



undefined4 __thiscall
FUN_004014d0(void *this,HINSTANCE param_1,uint param_2,undefined4 param_3,undefined4 param_4,
            int param_5,int param_6)

{
  HANDLE h;
  HDC hdc;
  int iVar1;
  HDC hdcDest;
  undefined1 auStack_98 [4];
  int iStack_94;
  int iStack_88;
  int iStack_84;
  undefined4 uStack_80;
  undefined4 uStack_7c;
  undefined4 uStack_10;
  undefined4 uStack_c;
  int iStack_8;
  int iStack_4;
  
  h = LoadImageA(param_1,(LPCSTR)(param_2 & 0xffff),0,param_5,param_6,0);
  if ((h != (HANDLE)0x0) && (hdcDest = *(HDC *)((int)this + 0x28), hdcDest != (HDC)0x0)) {
    (**(code **)(hdcDest->unused + 0x6c))();
    hdc = CreateCompatibleDC((HDC)0x0);
    if (hdc != (HDC)0x0) {
      SelectObject(hdc,h);
      GetObjectA(h,0x18,auStack_98);
      if (param_5 == 0) {
        param_5 = iStack_94;
      }
      uStack_80 = 0x7c;
      uStack_7c = 6;
      (**(code **)(**(int **)((int)this + 0x28) + 0x58))(*(int **)((int)this + 0x28),&uStack_80);
      iVar1 = (**(code **)(**(int **)((int)this + 0x28) + 0x44))
                        (*(int **)((int)this + 0x28),&stack0xffffff5c);
      if (iVar1 == 0) {
        StretchBlt(hdcDest,0,0,iStack_84,iStack_88,hdc,iStack_8,iStack_4,param_5,(int)param_1,
                   0xcc0020);
        (**(code **)(**(int **)((int)this + 0x28) + 0x68))(*(int **)((int)this + 0x28),hdcDest);
      }
      DeleteDC(hdc);
      *(undefined4 *)((int)this + 4) = uStack_10;
      *(undefined4 *)((int)this + 8) = uStack_c;
      *(int *)((int)this + 0xc) = iStack_8;
      *(int *)((int)this + 0x10) = iStack_4;
      *(int *)((int)this + 0x14) = param_5;
      *(HINSTANCE *)((int)this + 0x18) = param_1;
      return 1;
    }
  }
  return 0;
}



undefined4 __thiscall FUN_00401640(void *this,int *param_1,undefined4 param_2,undefined4 param_3)

{
  int iVar1;
  undefined4 *puVar2;
  int unaff_retaddr;
  undefined4 local_7c [22];
  undefined4 uStack_24;
  undefined4 local_14;
  undefined4 uStack_4;
  
  puVar2 = local_7c;
  for (iVar1 = 0x1f; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar2 = 0;
    puVar2 = puVar2 + 1;
  }
  puVar2 = (undefined4 *)((int)this + 0x28);
  local_7c[0] = 0x7c;
  local_7c[1] = 7;
  local_14 = 0x4040;
  local_7c[3] = param_2;
  local_7c[2] = param_3;
  iVar1 = (**(code **)(*param_1 + 0x18))(param_1,local_7c,puVar2,0);
  if (iVar1 != 0) {
    if (iVar1 == -0x7789fe84) {
      uStack_24 = 0x840;
      iVar1 = (**(code **)(*param_1 + 0x18))(param_1,&stack0xffffff74,puVar2,0);
    }
    if (iVar1 != 0) {
      return 0;
    }
  }
  if (unaff_retaddr != -1) {
    (**(code **)(*(int *)*puVar2 + 0x74))((int *)*puVar2,8,&stack0xffffff6c);
  }
  *(int *)((int)this + 0x1c) = unaff_retaddr;
  *(undefined4 *)((int)this + 0x24) = param_2;
  *(undefined4 *)((int)this + 0x20) = uStack_4;
  return 1;
}



undefined4 __thiscall
FUN_00401730(void *this,int *param_1,undefined4 param_2,undefined4 param_3,int param_4,int param_5,
            int param_6,int param_7)

{
  int iVar1;
  int local_10;
  int local_c;
  int local_8;
  int local_4;
  
  if (param_6 == 0) {
    param_6 = *(int *)((int)this + 0x24);
  }
  if (param_7 == 0) {
    param_7 = *(int *)((int)this + 0x20);
  }
  local_10 = param_4;
  local_c = param_5;
  local_8 = param_4 + param_6;
  local_4 = param_5 + param_7;
  do {
    while( true ) {
      iVar1 = (**(code **)(*param_1 + 0x1c))
                        (param_1,param_2,param_3,*(undefined4 *)((int)this + 0x28),&local_10,
                         -1 < *(int *)((int)this + 0x1c));
      if (iVar1 == 0) {
        return 1;
      }
      if (iVar1 != -0x7789fe3e) break;
      FUN_004017f0((int)this);
    }
  } while (iVar1 == -0x7789fde4);
  return 0;
}



void __fastcall FUN_004017d0(int param_1)

{
  int *piVar1;
  
  piVar1 = *(int **)(param_1 + 0x28);
  if (piVar1 != (int *)0x0) {
    (**(code **)(*piVar1 + 8))(piVar1);
    *(undefined4 *)(param_1 + 0x28) = 0;
  }
  return;
}



void __fastcall FUN_004017f0(int param_1)

{
  (**(code **)(**(int **)(param_1 + 0x28) + 0x6c))(*(int **)(param_1 + 0x28));
  return;
}



void DirectDrawCreateEx(void)

{
                    // WARNING: Could not recover jumptable at 0x00401800. Too many branches
                    // WARNING: Treating indirect jump as call
  DirectDrawCreateEx();
  return;
}



void __cdecl FUN_00401806(int param_1)

{
  SIZE_T SVar1;
  int *piVar2;
  void *this;
  
  SVar1 = FUN_00401da0((undefined *)DAT_00409ab0);
  if (SVar1 < (uint)((int)DAT_00409aac + (4 - (int)DAT_00409ab0))) {
    SVar1 = FUN_00401da0((undefined *)DAT_00409ab0);
    piVar2 = FUN_004019fe(this,DAT_00409ab0,(uint *)(SVar1 + 0x10));
    if (piVar2 == (int *)0x0) {
      return;
    }
    DAT_00409aac = piVar2 + ((int)DAT_00409aac - (int)DAT_00409ab0 >> 2);
    DAT_00409ab0 = piVar2;
  }
  *DAT_00409aac = param_1;
  DAT_00409aac = DAT_00409aac + 1;
  return;
}



int __cdecl FUN_00401873(int param_1)

{
  int iVar1;
  
  iVar1 = FUN_00401806(param_1);
  return (iVar1 != 0) - 1;
}



void __cdecl FUN_004018b4(undefined *param_1)

{
  FUN_00401eb3(param_1);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void entry(void)

{
  DWORD DVar1;
  int iVar2;
  byte *pbVar3;
  uint uVar4;
  HMODULE pHVar5;
  UINT UVar6;
  undefined4 uVar7;
  _STARTUPINFOA local_60;
  undefined1 *local_1c;
  _EXCEPTION_POINTERS *local_18;
  void *pvStack_14;
  undefined1 *puStack_10;
  undefined *puStack_c;
  undefined4 local_8;
  
  local_8 = 0xffffffff;
  puStack_c = &DAT_00406128;
  puStack_10 = &LAB_004029a8;
  pvStack_14 = ExceptionList;
  local_1c = &stack0xffffff88;
  ExceptionList = &pvStack_14;
  DVar1 = GetVersion();
  _DAT_004095c0 = DVar1 >> 8 & 0xff;
  _DAT_004095bc = DVar1 & 0xff;
  _DAT_004095b8 = _DAT_004095bc * 0x100 + _DAT_004095c0;
  _DAT_004095b4 = DVar1 >> 0x10;
  iVar2 = FUN_00402850(0);
  if (iVar2 == 0) {
    FUN_004019da(0x1c);
  }
  local_8 = 0;
  FUN_00402530();
  DAT_00409ab8 = GetCommandLineA();
  DAT_0040959c = FUN_004023fe();
  FUN_004021b1();
  FUN_004020f8();
  FUN_00401c9e();
  local_60.dwFlags = 0;
  GetStartupInfoA(&local_60);
  pbVar3 = FUN_004020a0();
  if ((local_60.dwFlags & 1) == 0) {
    uVar4 = 10;
  }
  else {
    uVar4 = (uint)local_60.wShowWindow;
  }
  uVar7 = 0;
  pHVar5 = GetModuleHandleA((LPCSTR)0x0);
  UVar6 = FUN_00401040(pHVar5,uVar7,pbVar3,uVar4);
  FUN_00401ccb(UVar6);
  FUN_00401f1c(local_18->ExceptionRecord->ExceptionCode,local_18);
  return;
}



void __cdecl FUN_004019b5(DWORD param_1)

{
  if (DAT_004095a4 == 1) {
    FUN_00402a80();
  }
  FUN_00402ab9(param_1);
  (*(code *)PTR___exit_004070c4)(0xff);
  return;
}



void __cdecl FUN_004019da(DWORD param_1)

{
  if (DAT_004095a4 == 1) {
    FUN_00402a80();
  }
  FUN_00402ab9(param_1);
                    // WARNING: Subroutine does not return
  ExitProcess(0xff);
}



int * __thiscall FUN_004019fe(void *this,int *param_1,uint *param_2)

{
  int *piVar1;
  uint *puVar2;
  int iVar3;
  uint *puVar4;
  byte *pbVar5;
  uint *puVar6;
  void *local_8;
  
  local_8 = this;
  if (param_1 == (int *)0x0) {
    piVar1 = (int *)_malloc((size_t)param_2);
  }
  else {
    if (param_2 == (uint *)0x0) {
      FUN_00401eb3((undefined *)param_1);
    }
    else {
      puVar6 = param_2;
      if (DAT_00409988 == 3) {
        do {
          piVar1 = (int *)0x0;
          if (puVar6 < (uint *)0xffffffe1) {
            puVar2 = (uint *)FUN_00402c54((int)param_1);
            if (puVar2 == (uint *)0x0) {
LAB_00401af5:
              if (puVar6 == (uint *)0x0) {
                puVar6 = (uint *)0x1;
              }
              puVar6 = (uint *)((int)puVar6 + 0xfU & 0xfffffff0);
              piVar1 = (int *)HeapReAlloc(DAT_00409984,0,param_1,(SIZE_T)puVar6);
            }
            else {
              if (DAT_00409980 < puVar6) {
LAB_00401aae:
                if (puVar6 == (uint *)0x0) {
                  puVar6 = (uint *)0x1;
                }
                puVar6 = (uint *)((int)puVar6 + 0xfU & 0xfffffff0);
                piVar1 = (int *)HeapAlloc(DAT_00409984,0,(SIZE_T)puVar6);
                if (piVar1 != (int *)0x0) {
                  puVar4 = (uint *)(param_1[-1] - 1U);
                  if (puVar6 <= (uint *)(param_1[-1] - 1U)) {
                    puVar4 = puVar6;
                  }
                  FUN_00403e40(piVar1,param_1,(uint)puVar4);
                  FUN_00402c7f(puVar2,(int)param_1);
                }
              }
              else {
                iVar3 = FUN_0040345d(puVar2,(int)param_1,(int)puVar6);
                piVar1 = param_1;
                if (iVar3 == 0) {
                  piVar1 = FUN_00402fa8(puVar6);
                  if (piVar1 == (int *)0x0) goto LAB_00401aae;
                  puVar2 = (uint *)(param_1[-1] - 1U);
                  if (puVar6 <= (uint *)(param_1[-1] - 1U)) {
                    puVar2 = puVar6;
                  }
                  FUN_00403e40(piVar1,param_1,(uint)puVar2);
                  puVar2 = (uint *)FUN_00402c54((int)param_1);
                  FUN_00402c7f(puVar2,(int)param_1);
                }
                if (piVar1 == (int *)0x0) goto LAB_00401aae;
              }
              if (puVar2 == (uint *)0x0) goto LAB_00401af5;
            }
            if (piVar1 != (int *)0x0) {
              return piVar1;
            }
          }
          if (DAT_0040970c == 0) {
            return piVar1;
          }
          iVar3 = FUN_00403e20(puVar6);
        } while (iVar3 != 0);
      }
      else if (DAT_00409988 == 2) {
        if (param_2 < (uint *)0xffffffe1) {
          if (param_2 == (uint *)0x0) {
            puVar6 = (uint *)0x10;
          }
          else {
            puVar6 = (uint *)((int)param_2 + 0xfU & 0xfffffff0);
          }
        }
        do {
          piVar1 = (int *)0x0;
          if (puVar6 < (uint *)0xffffffe1) {
            pbVar5 = (byte *)FUN_004039af((undefined *)param_1,&local_8,(uint *)&param_2);
            if (pbVar5 == (byte *)0x0) {
              piVar1 = (int *)HeapReAlloc(DAT_00409984,0,param_1,(SIZE_T)puVar6);
            }
            else {
              if (puVar6 < DAT_0040922c) {
                iVar3 = FUN_00403d77((int)local_8,(int *)param_2,pbVar5,(uint)puVar6 >> 4);
                piVar1 = param_1;
                if (iVar3 == 0) {
                  piVar1 = FUN_00403a4b((uint)puVar6 >> 4);
                  if (piVar1 == (int *)0x0) goto LAB_00401be3;
                  puVar2 = (uint *)((uint)*pbVar5 << 4);
                  if (puVar6 <= (uint *)((uint)*pbVar5 << 4)) {
                    puVar2 = puVar6;
                  }
                  FUN_00403e40(piVar1,param_1,(uint)puVar2);
                  FUN_00403a06((int)local_8,(int)param_2,pbVar5);
                }
                if (piVar1 != (int *)0x0) {
                  return piVar1;
                }
              }
LAB_00401be3:
              piVar1 = (int *)HeapAlloc(DAT_00409984,0,(SIZE_T)puVar6);
              if (piVar1 == (int *)0x0) goto LAB_00401c3b;
              puVar2 = (uint *)((uint)*pbVar5 << 4);
              if (puVar6 <= (uint *)((uint)*pbVar5 << 4)) {
                puVar2 = puVar6;
              }
              FUN_00403e40(piVar1,param_1,(uint)puVar2);
              FUN_00403a06((int)local_8,(int)param_2,pbVar5);
            }
            if (piVar1 != (int *)0x0) {
              return piVar1;
            }
          }
LAB_00401c3b:
          if (DAT_0040970c == 0) {
            return piVar1;
          }
          iVar3 = FUN_00403e20(puVar6);
        } while (iVar3 != 0);
      }
      else {
        do {
          piVar1 = (int *)0x0;
          if (puVar6 < (uint *)0xffffffe1) {
            if (puVar6 == (uint *)0x0) {
              puVar6 = (uint *)0x1;
            }
            puVar6 = (uint *)((int)puVar6 + 0xfU & 0xfffffff0);
            piVar1 = (int *)HeapReAlloc(DAT_00409984,0,param_1,(SIZE_T)puVar6);
            if (piVar1 != (int *)0x0) {
              return piVar1;
            }
          }
          if (DAT_0040970c == 0) {
            return piVar1;
          }
          iVar3 = FUN_00403e20(puVar6);
        } while (iVar3 != 0);
      }
    }
    piVar1 = (int *)0x0;
  }
  return piVar1;
}



void FUN_00401c9e(void)

{
  if (DAT_00409ab4 != (code *)0x0) {
    (*DAT_00409ab4)();
  }
  FUN_00401d86((undefined4 *)&DAT_0040700c,(undefined4 *)&DAT_00407018);
  FUN_00401d86((undefined4 *)&DAT_00407000,(undefined4 *)&DAT_00407008);
  return;
}



void __cdecl FUN_00401ccb(UINT param_1)

{
  FUN_00401ced(param_1,0,0);
  return;
}



// Library Function - Single Match
//  __exit
// 
// Library: Visual Studio 2003 Release

void __cdecl __exit(int _Code)

{
  FUN_00401ced(_Code,1,0);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void __cdecl FUN_00401ced(UINT param_1,int param_2,int param_3)

{
  HANDLE hProcess;
  undefined4 *puVar1;
  UINT uExitCode;
  
  if (DAT_004095f0 == 1) {
    uExitCode = param_1;
    hProcess = GetCurrentProcess();
    TerminateProcess(hProcess,uExitCode);
  }
  _DAT_004095ec = 1;
  DAT_004095e8 = (undefined1)param_3;
  if (param_2 == 0) {
    if ((DAT_00409ab0 != (undefined4 *)0x0) &&
       (puVar1 = (undefined4 *)(DAT_00409aac - 4), DAT_00409ab0 <= puVar1)) {
      do {
        if ((code *)*puVar1 != (code *)0x0) {
          (*(code *)*puVar1)();
        }
        puVar1 = puVar1 + -1;
      } while (DAT_00409ab0 <= puVar1);
    }
    FUN_00401d86((undefined4 *)&DAT_0040701c,(undefined4 *)&DAT_00407020);
  }
  FUN_00401d86((undefined4 *)&DAT_00407024,(undefined4 *)&DAT_00407028);
  if (param_3 != 0) {
    return;
  }
  DAT_004095f0 = 1;
                    // WARNING: Subroutine does not return
  ExitProcess(param_1);
}



void __cdecl FUN_00401d86(undefined4 *param_1,undefined4 *param_2)

{
  for (; param_1 < param_2; param_1 = param_1 + 1) {
    if ((code *)*param_1 != (code *)0x0) {
      (*(code *)*param_1)();
    }
  }
  return;
}



SIZE_T __cdecl FUN_00401da0(undefined *param_1)

{
  uint uVar1;
  byte *pbVar2;
  SIZE_T SVar3;
  undefined4 local_c;
  uint local_8;
  
  if (DAT_00409988 == 3) {
    uVar1 = FUN_00402c54((int)param_1);
    if (uVar1 != 0) {
      return *(int *)(param_1 + -4) - 9;
    }
  }
  else if ((DAT_00409988 == 2) &&
          (pbVar2 = (byte *)FUN_004039af(param_1,&local_c,&local_8), pbVar2 != (byte *)0x0)) {
    return (uint)*pbVar2 << 4;
  }
  SVar3 = HeapSize(DAT_00409984,0,param_1);
  return SVar3;
}



// Library Function - Single Match
//  _malloc
// 
// Library: Visual Studio 2003 Release

void * __cdecl _malloc(size_t _Size)

{
  void *pvVar1;
  
  pvVar1 = __nh_malloc(_Size,DAT_0040970c);
  return pvVar1;
}



// Library Function - Single Match
//  __nh_malloc
// 
// Library: Visual Studio 2003 Release

void * __cdecl __nh_malloc(size_t _Size,int _NhFlag)

{
  void *pvVar1;
  int iVar2;
  
  if (_Size < 0xffffffe1) {
    do {
      pvVar1 = (void *)FUN_00401e3f((uint *)_Size);
      if (pvVar1 != (void *)0x0) {
        return pvVar1;
      }
      if (_NhFlag == 0) {
        return (void *)0x0;
      }
      iVar2 = FUN_00403e20(_Size);
    } while (iVar2 != 0);
  }
  return (void *)0x0;
}



void __cdecl FUN_00401e3f(uint *param_1)

{
  int *piVar1;
  uint dwBytes;
  
  if (DAT_00409988 == 3) {
    if ((param_1 <= DAT_00409980) && (piVar1 = FUN_00402fa8(param_1), piVar1 != (int *)0x0)) {
      return;
    }
  }
  else if (DAT_00409988 == 2) {
    if (param_1 == (uint *)0x0) {
      dwBytes = 0x10;
    }
    else {
      dwBytes = (int)param_1 + 0xfU & 0xfffffff0;
    }
    if ((dwBytes <= DAT_0040922c) && (piVar1 = FUN_00403a4b(dwBytes >> 4), piVar1 != (int *)0x0)) {
      return;
    }
    goto LAB_00401ea2;
  }
  if (param_1 == (uint *)0x0) {
    param_1 = (uint *)0x1;
  }
  dwBytes = (int)param_1 + 0xfU & 0xfffffff0;
LAB_00401ea2:
  HeapAlloc(DAT_00409984,0,dwBytes);
  return;
}



void __cdecl FUN_00401eb3(undefined *param_1)

{
  undefined *lpMem;
  uint *puVar1;
  byte *pbVar2;
  int local_8;
  
  lpMem = param_1;
  if (param_1 != (undefined *)0x0) {
    if (DAT_00409988 == 3) {
      puVar1 = (uint *)FUN_00402c54((int)param_1);
      if (puVar1 != (uint *)0x0) {
        FUN_00402c7f(puVar1,(int)lpMem);
        return;
      }
    }
    else if ((DAT_00409988 == 2) &&
            (pbVar2 = (byte *)FUN_004039af(param_1,&local_8,(uint *)&param_1), pbVar2 != (byte *)0x0
            )) {
      FUN_00403a06(local_8,(int)param_1,pbVar2);
      return;
    }
    HeapFree(DAT_00409984,0,lpMem);
  }
  return;
}



LONG __cdecl FUN_00401f1c(int param_1,_EXCEPTION_POINTERS *param_2)

{
  code *pcVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  int *piVar4;
  LONG LVar5;
  int iVar6;
  undefined4 *puVar7;
  
  piVar4 = FUN_0040205d(param_1);
  uVar3 = DAT_004095f4;
  if ((piVar4 == (int *)0x0) || (pcVar1 = (code *)piVar4[2], pcVar1 == (code *)0x0)) {
    LVar5 = UnhandledExceptionFilter(param_2);
  }
  else if (pcVar1 == (code *)0x5) {
    piVar4[2] = 0;
    LVar5 = 1;
  }
  else {
    if (pcVar1 != (code *)0x1) {
      DAT_004095f4 = param_2;
      if (piVar4[1] == 8) {
        if (DAT_00407148 < DAT_0040714c + DAT_00407148) {
          iVar6 = (DAT_0040714c + DAT_00407148) - DAT_00407148;
          puVar7 = (undefined4 *)(DAT_00407148 * 0xc + 0x4070d8);
          do {
            *puVar7 = 0;
            puVar7 = puVar7 + 3;
            iVar6 = iVar6 + -1;
          } while (iVar6 != 0);
        }
        uVar2 = DAT_00407154;
        iVar6 = *piVar4;
        if (iVar6 == -0x3fffff72) {
          DAT_00407154 = 0x83;
        }
        else if (iVar6 == -0x3fffff70) {
          DAT_00407154 = 0x81;
        }
        else if (iVar6 == -0x3fffff6f) {
          DAT_00407154 = 0x84;
        }
        else if (iVar6 == -0x3fffff6d) {
          DAT_00407154 = 0x85;
        }
        else if (iVar6 == -0x3fffff73) {
          DAT_00407154 = 0x82;
        }
        else if (iVar6 == -0x3fffff71) {
          DAT_00407154 = 0x86;
        }
        else if (iVar6 == -0x3fffff6e) {
          DAT_00407154 = 0x8a;
        }
        (*pcVar1)(8,DAT_00407154);
        DAT_00407154 = uVar2;
      }
      else {
        piVar4[2] = 0;
        (*pcVar1)(piVar4[1]);
      }
    }
    LVar5 = -1;
    DAT_004095f4 = (_EXCEPTION_POINTERS *)uVar3;
  }
  return LVar5;
}



int * __cdecl FUN_0040205d(int param_1)

{
  int *piVar1;
  
  piVar1 = &DAT_004070d0;
  if (DAT_004070d0 != param_1) {
    do {
      piVar1 = piVar1 + 3;
      if (&DAT_004070d0 + DAT_00407150 * 3 <= piVar1) break;
    } while (*piVar1 != param_1);
  }
  if ((&DAT_004070d0 + DAT_00407150 * 3 <= piVar1) || (*piVar1 != param_1)) {
    piVar1 = (int *)0x0;
  }
  return piVar1;
}



byte * FUN_004020a0(void)

{
  byte bVar1;
  int iVar2;
  byte *pbVar3;
  byte *pbVar4;
  
  if (DAT_00409aa8 == 0) {
    FUN_0040457b();
  }
  bVar1 = *DAT_00409ab8;
  pbVar4 = DAT_00409ab8;
  if (bVar1 == 0x22) {
    while( true ) {
      pbVar3 = pbVar4;
      bVar1 = pbVar3[1];
      pbVar4 = pbVar3 + 1;
      if ((bVar1 == 0x22) || (bVar1 == 0)) break;
      iVar2 = FUN_00404175((uint)bVar1);
      if (iVar2 != 0) {
        pbVar4 = pbVar3 + 2;
      }
    }
    if (*pbVar4 == 0x22) goto LAB_004020dd;
  }
  else {
    while (0x20 < bVar1) {
      bVar1 = pbVar4[1];
      pbVar4 = pbVar4 + 1;
    }
  }
  for (; (*pbVar4 != 0 && (*pbVar4 < 0x21)); pbVar4 = pbVar4 + 1) {
LAB_004020dd:
  }
  return pbVar4;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004020f8(void)

{
  char cVar1;
  size_t sVar2;
  undefined4 *puVar3;
  void *pvVar4;
  int iVar5;
  uint *puVar6;
  
  if (DAT_00409aa8 == 0) {
    FUN_0040457b();
  }
  iVar5 = 0;
  for (puVar6 = DAT_0040959c; (char)*puVar6 != '\0'; puVar6 = (uint *)((int)puVar6 + sVar2 + 1)) {
    if ((char)*puVar6 != '=') {
      iVar5 = iVar5 + 1;
    }
    sVar2 = _strlen((char *)puVar6);
  }
  puVar3 = (undefined4 *)_malloc(iVar5 * 4 + 4);
  _DAT_004095d0 = puVar3;
  if (puVar3 == (undefined4 *)0x0) {
    FUN_004019b5(9);
  }
  cVar1 = (char)*DAT_0040959c;
  puVar6 = DAT_0040959c;
  while (cVar1 != '\0') {
    sVar2 = _strlen((char *)puVar6);
    if ((char)*puVar6 != '=') {
      pvVar4 = _malloc(sVar2 + 1);
      *puVar3 = pvVar4;
      if (pvVar4 == (void *)0x0) {
        FUN_004019b5(9);
      }
      FUN_004045a0((uint *)*puVar3,puVar6);
      puVar3 = puVar3 + 1;
    }
    puVar6 = (uint *)((int)puVar6 + sVar2 + 1);
    cVar1 = (char)*puVar6;
  }
  FUN_00401eb3((undefined *)DAT_0040959c);
  DAT_0040959c = (uint *)0x0;
  *puVar3 = 0;
  _DAT_00409aa4 = 1;
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004021b1(void)

{
  undefined4 *puVar1;
  byte *pbVar2;
  int local_c;
  int local_8;
  
  if (DAT_00409aa8 == 0) {
    FUN_0040457b();
  }
  GetModuleFileNameA((HMODULE)0x0,&DAT_004095f8,0x104);
  _DAT_004095e0 = &DAT_004095f8;
  pbVar2 = &DAT_004095f8;
  if (*DAT_00409ab8 != 0) {
    pbVar2 = DAT_00409ab8;
  }
  FUN_0040224a(pbVar2,(undefined4 *)0x0,(byte *)0x0,&local_8,&local_c);
  puVar1 = (undefined4 *)_malloc(local_c + local_8 * 4);
  if (puVar1 == (undefined4 *)0x0) {
    FUN_004019b5(8);
  }
  FUN_0040224a(pbVar2,puVar1,(byte *)(puVar1 + local_8),&local_8,&local_c);
  _DAT_004095c8 = puVar1;
  _DAT_004095c4 = local_8 + -1;
  return;
}



void __cdecl FUN_0040224a(byte *param_1,undefined4 *param_2,byte *param_3,int *param_4,int *param_5)

{
  byte bVar1;
  bool bVar2;
  bool bVar3;
  byte *pbVar4;
  byte *pbVar5;
  uint uVar6;
  undefined4 *puVar7;
  
  *param_5 = 0;
  *param_4 = 1;
  if (param_2 != (undefined4 *)0x0) {
    *param_2 = param_3;
    param_2 = param_2 + 1;
  }
  if (*param_1 == 0x22) {
    while( true ) {
      bVar1 = param_1[1];
      pbVar4 = param_1 + 1;
      if ((bVar1 == 0x22) || (bVar1 == 0)) break;
      if ((((&DAT_00409861)[bVar1] & 4) != 0) && (*param_5 = *param_5 + 1, param_3 != (byte *)0x0))
      {
        *param_3 = *pbVar4;
        param_3 = param_3 + 1;
        pbVar4 = param_1 + 2;
      }
      *param_5 = *param_5 + 1;
      param_1 = pbVar4;
      if (param_3 != (byte *)0x0) {
        *param_3 = *pbVar4;
        param_3 = param_3 + 1;
      }
    }
    *param_5 = *param_5 + 1;
    if (param_3 != (byte *)0x0) {
      *param_3 = 0;
      param_3 = param_3 + 1;
    }
    if (*pbVar4 == 0x22) {
      pbVar4 = param_1 + 2;
    }
  }
  else {
    do {
      *param_5 = *param_5 + 1;
      if (param_3 != (byte *)0x0) {
        *param_3 = *param_1;
        param_3 = param_3 + 1;
      }
      bVar1 = *param_1;
      pbVar4 = param_1 + 1;
      if (((&DAT_00409861)[bVar1] & 4) != 0) {
        *param_5 = *param_5 + 1;
        if (param_3 != (byte *)0x0) {
          *param_3 = *pbVar4;
          param_3 = param_3 + 1;
        }
        pbVar4 = param_1 + 2;
      }
      if (bVar1 == 0x20) break;
      if (bVar1 == 0) goto LAB_004022f5;
      param_1 = pbVar4;
    } while (bVar1 != 9);
    if (bVar1 == 0) {
LAB_004022f5:
      pbVar4 = pbVar4 + -1;
    }
    else if (param_3 != (byte *)0x0) {
      param_3[-1] = 0;
    }
  }
  bVar2 = false;
  puVar7 = param_2;
  while (*pbVar4 != 0) {
    for (; (*pbVar4 == 0x20 || (*pbVar4 == 9)); pbVar4 = pbVar4 + 1) {
    }
    if (*pbVar4 == 0) break;
    if (puVar7 != (undefined4 *)0x0) {
      *puVar7 = param_3;
      puVar7 = puVar7 + 1;
      param_2 = puVar7;
    }
    *param_4 = *param_4 + 1;
    while( true ) {
      bVar3 = true;
      uVar6 = 0;
      for (; *pbVar4 == 0x5c; pbVar4 = pbVar4 + 1) {
        uVar6 = uVar6 + 1;
      }
      if (*pbVar4 == 0x22) {
        pbVar5 = pbVar4;
        if ((uVar6 & 1) == 0) {
          if ((!bVar2) || (pbVar5 = pbVar4 + 1, pbVar4[1] != 0x22)) {
            bVar3 = false;
            pbVar5 = pbVar4;
          }
          bVar2 = !bVar2;
          puVar7 = param_2;
        }
        uVar6 = uVar6 >> 1;
        pbVar4 = pbVar5;
      }
      for (; uVar6 != 0; uVar6 = uVar6 - 1) {
        if (param_3 != (byte *)0x0) {
          *param_3 = 0x5c;
          param_3 = param_3 + 1;
        }
        *param_5 = *param_5 + 1;
      }
      bVar1 = *pbVar4;
      if ((bVar1 == 0) || ((!bVar2 && ((bVar1 == 0x20 || (bVar1 == 9)))))) break;
      if (bVar3) {
        if (param_3 == (byte *)0x0) {
          if (((&DAT_00409861)[bVar1] & 4) != 0) {
            pbVar4 = pbVar4 + 1;
            *param_5 = *param_5 + 1;
          }
        }
        else {
          if (((&DAT_00409861)[bVar1] & 4) != 0) {
            *param_3 = bVar1;
            param_3 = param_3 + 1;
            pbVar4 = pbVar4 + 1;
            *param_5 = *param_5 + 1;
          }
          *param_3 = *pbVar4;
          param_3 = param_3 + 1;
        }
        *param_5 = *param_5 + 1;
      }
      pbVar4 = pbVar4 + 1;
    }
    if (param_3 != (byte *)0x0) {
      *param_3 = 0;
      param_3 = param_3 + 1;
    }
    *param_5 = *param_5 + 1;
  }
  if (puVar7 != (undefined4 *)0x0) {
    *puVar7 = 0;
  }
  *param_4 = *param_4 + 1;
  return;
}



LPSTR FUN_004023fe(void)

{
  char cVar1;
  WCHAR WVar2;
  WCHAR *pWVar3;
  WCHAR *pWVar4;
  int iVar5;
  size_t _Size;
  LPSTR pCVar6;
  char *pcVar7;
  LPWCH lpWideCharStr;
  LPCH pCVar9;
  LPSTR local_8;
  char *pcVar8;
  
  lpWideCharStr = (LPWCH)0x0;
  pCVar9 = (LPCH)0x0;
  if (DAT_004096fc == 0) {
    lpWideCharStr = GetEnvironmentStringsW();
    if (lpWideCharStr != (LPWCH)0x0) {
      DAT_004096fc = 1;
LAB_00402455:
      if ((lpWideCharStr == (LPWCH)0x0) &&
         (lpWideCharStr = GetEnvironmentStringsW(), lpWideCharStr == (LPWCH)0x0)) {
        return (LPSTR)0x0;
      }
      WVar2 = *lpWideCharStr;
      pWVar4 = lpWideCharStr;
      while (WVar2 != L'\0') {
        do {
          pWVar3 = pWVar4;
          pWVar4 = pWVar3 + 1;
        } while (*pWVar4 != L'\0');
        pWVar4 = pWVar3 + 2;
        WVar2 = *pWVar4;
      }
      iVar5 = ((int)pWVar4 - (int)lpWideCharStr >> 1) + 1;
      _Size = WideCharToMultiByte(0,0,lpWideCharStr,iVar5,(LPSTR)0x0,0,(LPCSTR)0x0,(LPBOOL)0x0);
      local_8 = (LPSTR)0x0;
      if (((_Size != 0) && (pCVar6 = (LPSTR)_malloc(_Size), pCVar6 != (LPSTR)0x0)) &&
         (iVar5 = WideCharToMultiByte(0,0,lpWideCharStr,iVar5,pCVar6,_Size,(LPCSTR)0x0,(LPBOOL)0x0),
         local_8 = pCVar6, iVar5 == 0)) {
        FUN_00401eb3(pCVar6);
        local_8 = (LPSTR)0x0;
      }
      FreeEnvironmentStringsW(lpWideCharStr);
      return local_8;
    }
    pCVar9 = GetEnvironmentStrings();
    if (pCVar9 == (LPCH)0x0) {
      return (LPSTR)0x0;
    }
    DAT_004096fc = 2;
  }
  else {
    if (DAT_004096fc == 1) goto LAB_00402455;
    if (DAT_004096fc != 2) {
      return (LPSTR)0x0;
    }
  }
  if ((pCVar9 == (LPCH)0x0) && (pCVar9 = GetEnvironmentStrings(), pCVar9 == (LPCH)0x0)) {
    return (LPSTR)0x0;
  }
  cVar1 = *pCVar9;
  pcVar7 = pCVar9;
  while (cVar1 != '\0') {
    do {
      pcVar8 = pcVar7;
      pcVar7 = pcVar8 + 1;
    } while (*pcVar7 != '\0');
    pcVar7 = pcVar8 + 2;
    cVar1 = *pcVar7;
  }
  pCVar6 = (LPSTR)_malloc((size_t)(pcVar7 + (1 - (int)pCVar9)));
  if (pCVar6 == (LPSTR)0x0) {
    pCVar6 = (LPSTR)0x0;
  }
  else {
    FUN_00403e40((undefined4 *)pCVar6,(undefined4 *)pCVar9,(uint)(pcVar7 + (1 - (int)pCVar9)));
  }
  FreeEnvironmentStringsA(pCVar9);
  return pCVar6;
}



void FUN_00402530(void)

{
  undefined4 *puVar1;
  undefined4 *puVar2;
  undefined4 *puVar3;
  DWORD DVar4;
  HANDLE hFile;
  byte *pbVar5;
  int iVar6;
  UINT *pUVar7;
  UINT UVar8;
  UINT UVar9;
  uint uVar10;
  _STARTUPINFOA local_44;
  
  puVar2 = (undefined4 *)_malloc(0x100);
  if (puVar2 == (undefined4 *)0x0) {
    FUN_004019b5(0x1b);
  }
  DAT_00409aa0 = 0x20;
  DAT_004099a0 = puVar2;
  for (; puVar2 < DAT_004099a0 + 0x40; puVar2 = puVar2 + 2) {
    *(undefined1 *)(puVar2 + 1) = 0;
    *puVar2 = 0xffffffff;
    *(undefined1 *)((int)puVar2 + 5) = 10;
  }
  GetStartupInfoA(&local_44);
  if ((local_44.cbReserved2 != 0) && ((UINT *)local_44.lpReserved2 != (UINT *)0x0)) {
    UVar8 = *(UINT *)local_44.lpReserved2;
    pUVar7 = (UINT *)((int)local_44.lpReserved2 + 4);
    pbVar5 = (byte *)(UVar8 + (int)pUVar7);
    if (0x7ff < (int)UVar8) {
      UVar8 = 0x800;
    }
    UVar9 = UVar8;
    if ((int)DAT_00409aa0 < (int)UVar8) {
      puVar2 = &DAT_004099a4;
      do {
        puVar3 = (undefined4 *)_malloc(0x100);
        UVar9 = DAT_00409aa0;
        if (puVar3 == (undefined4 *)0x0) break;
        DAT_00409aa0 = DAT_00409aa0 + 0x20;
        *puVar2 = puVar3;
        puVar1 = puVar3;
        for (; puVar3 < puVar1 + 0x40; puVar3 = puVar3 + 2) {
          *(undefined1 *)(puVar3 + 1) = 0;
          *puVar3 = 0xffffffff;
          *(undefined1 *)((int)puVar3 + 5) = 10;
          puVar1 = (undefined4 *)*puVar2;
        }
        puVar2 = puVar2 + 1;
        UVar9 = UVar8;
      } while ((int)DAT_00409aa0 < (int)UVar8);
    }
    uVar10 = 0;
    if (0 < (int)UVar9) {
      do {
        if (((*(HANDLE *)pbVar5 != (HANDLE)0xffffffff) && ((*pUVar7 & 1) != 0)) &&
           (((*pUVar7 & 8) != 0 || (DVar4 = GetFileType(*(HANDLE *)pbVar5), DVar4 != 0)))) {
          puVar2 = (undefined4 *)((int)(&DAT_004099a0)[(int)uVar10 >> 5] + (uVar10 & 0x1f) * 8);
          *puVar2 = *(undefined4 *)pbVar5;
          *(byte *)(puVar2 + 1) = (byte)*pUVar7;
        }
        uVar10 = uVar10 + 1;
        pUVar7 = (UINT *)((int)pUVar7 + 1);
        pbVar5 = pbVar5 + 4;
      } while ((int)uVar10 < (int)UVar9);
    }
  }
  iVar6 = 0;
  do {
    puVar2 = DAT_004099a0 + iVar6 * 2;
    if (DAT_004099a0[iVar6 * 2] == -1) {
      *(undefined1 *)(puVar2 + 1) = 0x81;
      if (iVar6 == 0) {
        DVar4 = 0xfffffff6;
      }
      else {
        DVar4 = 0xfffffff5 - (iVar6 != 1);
      }
      hFile = GetStdHandle(DVar4);
      if ((hFile != (HANDLE)0xffffffff) && (DVar4 = GetFileType(hFile), DVar4 != 0)) {
        *puVar2 = hFile;
        if ((DVar4 & 0xff) != 2) {
          if ((DVar4 & 0xff) == 3) {
            *(byte *)(puVar2 + 1) = *(byte *)(puVar2 + 1) | 8;
          }
          goto LAB_004026c1;
        }
      }
      *(byte *)(puVar2 + 1) = *(byte *)(puVar2 + 1) | 0x40;
    }
    else {
      *(byte *)(puVar2 + 1) = *(byte *)(puVar2 + 1) | 0x80;
    }
LAB_004026c1:
    iVar6 = iVar6 + 1;
    if (2 < iVar6) {
      SetHandleCount(DAT_00409aa0);
      return;
    }
  } while( true );
}



void __cdecl FUN_004026db(undefined4 *param_1)

{
  int iVar1;
  HMODULE pHVar2;
  
  *param_1 = 0;
  pHVar2 = GetModuleHandleA((LPCSTR)0x0);
  if (((short)pHVar2->unused == 0x5a4d) && (iVar1 = pHVar2[0xf].unused, iVar1 != 0)) {
    *(undefined1 *)param_1 = *(undefined1 *)((int)&pHVar2[6].unused + iVar1 + 2);
    *(undefined1 *)((int)param_1 + 1) = *(undefined1 *)((int)&pHVar2[6].unused + iVar1 + 3);
  }
  return;
}



int FUN_00402708(void)

{
  char cVar1;
  byte bVar2;
  BOOL BVar3;
  DWORD DVar4;
  int iVar5;
  byte *pbVar6;
  char *pcVar7;
  byte *this;
  byte unaff_BL;
  char local_1230 [4240];
  char local_1a0 [260];
  DWORD local_9c;
  uint local_98;
  DWORD local_8c;
  CHAR aCStackY_18 [4];
  
  FUN_00404ac0();
  local_9c = 0x94;
  BVar3 = GetVersionExA((LPOSVERSIONINFOA)&local_9c);
  if (((BVar3 == 0) || (local_8c != 2)) || (local_98 < 5)) {
    builtin_memcpy(aCStackY_18,"b\'@",4);
    DVar4 = GetEnvironmentVariableA("__MSVCRT_HEAP_SELECT",local_1230,0x1090);
    if (DVar4 != 0) {
      pcVar7 = local_1230;
      while (local_1230[0] != '\0') {
        cVar1 = *pcVar7;
        if (('`' < cVar1) && (cVar1 < '{')) {
          *pcVar7 = cVar1 + -0x20;
        }
        pcVar7 = pcVar7 + 1;
        local_1230[0] = *pcVar7;
      }
      aCStackY_18[0] = -0x60;
      aCStackY_18[1] = '\'';
      aCStackY_18[2] = '@';
      aCStackY_18[3] = '\0';
      iVar5 = _strncmp("__GLOBAL_HEAP_SELECTED",local_1230,0x16);
      if (iVar5 == 0) {
        pcVar7 = local_1230;
      }
      else {
        aCStackY_18[0] = -0x3e;
        aCStackY_18[1] = '\'';
        aCStackY_18[2] = '@';
        aCStackY_18[3] = '\0';
        GetModuleFileNameA((HMODULE)0x0,local_1a0,0x104);
        pcVar7 = local_1a0;
        while (local_1a0[0] != '\0') {
          cVar1 = *pcVar7;
          if (('`' < cVar1) && (cVar1 < '{')) {
            *pcVar7 = cVar1 + -0x20;
          }
          pcVar7 = pcVar7 + 1;
          local_1a0[0] = *pcVar7;
        }
        pcVar7 = _strstr(local_1230,local_1a0);
      }
      if ((pcVar7 != (char *)0x0) && (pcVar7 = _strchr(pcVar7,0x2c), pcVar7 != (char *)0x0)) {
        pbVar6 = (byte *)(pcVar7 + 1);
        bVar2 = *pbVar6;
        this = pbVar6;
        while (bVar2 != 0) {
          if (*this == 0x3b) {
            *this = 0;
          }
          else {
            this = this + 1;
          }
          bVar2 = *this;
        }
        builtin_memcpy(aCStackY_18,"((@",4);
        iVar5 = FUN_0040470b(this,pbVar6,(int *)0x0,(void *)0xa);
        if (iVar5 == 2) {
          return 2;
        }
        if (iVar5 == 3) {
          return 3;
        }
        if (iVar5 == 1) {
          return 1;
        }
      }
    }
    FUN_004026db((undefined4 *)&stack0xfffffff8);
    iVar5 = 3 - (uint)(unaff_BL < 6);
  }
  else {
    iVar5 = 1;
  }
  return iVar5;
}



undefined4 __cdecl FUN_00402850(int param_1)

{
  undefined **ppuVar1;
  
  DAT_00409984 = HeapCreate((uint)(param_1 == 0),0x1000,0);
  if (DAT_00409984 != (HANDLE)0x0) {
    DAT_00409988 = FUN_00402708();
    if (DAT_00409988 == 3) {
      ppuVar1 = (undefined **)FUN_00402c0c(0x3f8);
    }
    else {
      if (DAT_00409988 != 2) {
        return 1;
      }
      ppuVar1 = FUN_00403753();
    }
    if (ppuVar1 != (undefined **)0x0) {
      return 1;
    }
    HeapDestroy(DAT_00409984);
  }
  return 0;
}



// Library Function - Single Match
//  __global_unwind2
// 
// Library: Visual Studio

void __cdecl __global_unwind2(PVOID param_1)

{
  RtlUnwind(param_1,(PVOID)0x4028c8,(PEXCEPTION_RECORD)0x0,(PVOID)0x0);
  return;
}



// Library Function - Single Match
//  __local_unwind2
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release, Visual Studio 2003 Debug, Visual
// Studio 2003 Release

void __cdecl __local_unwind2(int param_1,int param_2)

{
  int iVar1;
  int iVar2;
  void *pvStack_1c;
  undefined1 *puStack_18;
  undefined4 local_14;
  int iStack_10;
  
  iStack_10 = param_1;
  puStack_18 = &LAB_004028d0;
  pvStack_1c = ExceptionList;
  ExceptionList = &pvStack_1c;
  while( true ) {
    iVar1 = *(int *)(param_1 + 8);
    iVar2 = *(int *)(param_1 + 0xc);
    if ((iVar2 == -1) || (iVar2 == param_2)) break;
    local_14 = *(undefined4 *)(iVar1 + iVar2 * 0xc);
    *(undefined4 *)(param_1 + 0xc) = local_14;
    if (*(int *)(iVar1 + 4 + iVar2 * 0xc) == 0) {
      FUN_00402986();
      (**(code **)(iVar1 + 8 + iVar2 * 0xc))();
    }
  }
  ExceptionList = pvStack_1c;
  return;
}



void FUN_00402986(void)

{
  undefined4 in_EAX;
  int unaff_EBP;
  
  DAT_0040716c = *(undefined4 *)(unaff_EBP + 8);
  DAT_00407168 = in_EAX;
  DAT_00407170 = unaff_EBP;
  return;
}



void FUN_00402a65(int param_1)

{
  __local_unwind2(*(int *)(param_1 + 0x18),*(int *)(param_1 + 0x1c));
  return;
}



void FUN_00402a80(void)

{
  if ((DAT_004095a4 == 1) || ((DAT_004095a4 == 0 && (DAT_004070c8 == 1)))) {
    FUN_00402ab9(0xfc);
    if (DAT_00409700 != (code *)0x0) {
      (*DAT_00409700)();
    }
    FUN_00402ab9(0xff);
  }
  return;
}



void __cdecl FUN_00402ab9(DWORD param_1)

{
  undefined4 *puVar1;
  DWORD *pDVar2;
  DWORD DVar3;
  size_t sVar4;
  HANDLE hFile;
  int iVar5;
  uint *_Dest;
  undefined1 auStackY_1e3 [7];
  LPCVOID lpBuffer;
  LPOVERLAPPED lpOverlapped;
  uint local_1a8 [65];
  uint local_a4 [40];
  
  iVar5 = 0;
  pDVar2 = &DAT_00407178;
  do {
    if (param_1 == *pDVar2) break;
    pDVar2 = pDVar2 + 2;
    iVar5 = iVar5 + 1;
  } while ((int)pDVar2 < 0x407208);
  if (param_1 == (&DAT_00407178)[iVar5 * 2]) {
    if ((DAT_004095a4 == 1) || ((DAT_004095a4 == 0 && (DAT_004070c8 == 1)))) {
      pDVar2 = &param_1;
      puVar1 = (undefined4 *)(iVar5 * 8 + 0x40717c);
      lpOverlapped = (LPOVERLAPPED)0x0;
      sVar4 = _strlen((char *)*puVar1);
      lpBuffer = (LPCVOID)*puVar1;
      hFile = GetStdHandle(0xfffffff4);
      WriteFile(hFile,lpBuffer,sVar4,pDVar2,lpOverlapped);
    }
    else if (param_1 != 0xfc) {
      DVar3 = GetModuleFileNameA((HMODULE)0x0,(LPSTR)local_1a8,0x104);
      if (DVar3 == 0) {
        FUN_004045a0(local_1a8,(uint *)"<program name unknown>");
      }
      _Dest = local_1a8;
      sVar4 = _strlen((char *)local_1a8);
      if (0x3c < sVar4 + 1) {
        sVar4 = _strlen((char *)local_1a8);
        _Dest = (uint *)(auStackY_1e3 + sVar4);
        _strncpy((char *)_Dest,"...",3);
      }
      FUN_004045a0(local_a4,(uint *)"Runtime Error!\n\nProgram: ");
      FUN_004045b0(local_a4,_Dest);
      FUN_004045b0(local_a4,(uint *)&DAT_00406418);
      FUN_004045b0(local_a4,*(uint **)(iVar5 * 8 + 0x40717c));
      auStackY_1e3._3_4_ = 0x402bdd;
      FUN_00404aef(local_a4,"Microsoft Visual C++ Runtime Library",0x12010);
    }
  }
  return;
}



undefined4 FUN_00402c0c(undefined4 param_1)

{
  DAT_0040997c = HeapAlloc(DAT_00409984,0,0x140);
  if (DAT_0040997c == (LPVOID)0x0) {
    return 0;
  }
  DAT_00409974 = 0;
  DAT_00409978 = 0;
  DAT_00409970 = DAT_0040997c;
  DAT_00409980 = param_1;
  DAT_00409968 = 0x10;
  return 1;
}



uint __cdecl FUN_00402c54(int param_1)

{
  uint uVar1;
  
  uVar1 = DAT_0040997c;
  while( true ) {
    if (DAT_0040997c + DAT_00409978 * 0x14 <= uVar1) {
      return 0;
    }
    if ((uint)(param_1 - *(int *)(uVar1 + 0xc)) < 0x100000) break;
    uVar1 = uVar1 + 0x14;
  }
  return uVar1;
}



void __cdecl FUN_00402c7f(uint *param_1,int param_2)

{
  char *pcVar1;
  uint *puVar2;
  int *piVar3;
  char cVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  byte bVar8;
  uint uVar9;
  uint *puVar10;
  uint *puVar11;
  uint *puVar12;
  uint uVar13;
  uint uVar14;
  uint local_8;
  
  uVar5 = param_1[4];
  puVar12 = (uint *)(param_2 + -4);
  uVar14 = param_2 - param_1[3] >> 0xf;
  piVar3 = (int *)(uVar14 * 0x204 + 0x144 + uVar5);
  uVar13 = *puVar12;
  local_8 = uVar13 - 1;
  if ((local_8 & 1) == 0) {
    uVar6 = *(uint *)(local_8 + (int)puVar12);
    uVar7 = *(uint *)(param_2 + -8);
    if ((uVar6 & 1) == 0) {
      uVar9 = ((int)uVar6 >> 4) - 1;
      if (0x3f < uVar9) {
        uVar9 = 0x3f;
      }
      if (*(int *)((int)puVar12 + uVar13 + 3) == *(int *)((int)puVar12 + uVar13 + 7)) {
        if (uVar9 < 0x20) {
          pcVar1 = (char *)(uVar9 + 4 + uVar5);
          uVar9 = ~(0x80000000U >> ((byte)uVar9 & 0x1f));
          puVar10 = (uint *)(uVar5 + 0x44 + uVar14 * 4);
          *puVar10 = *puVar10 & uVar9;
          *pcVar1 = *pcVar1 + -1;
          if (*pcVar1 == '\0') {
            *param_1 = *param_1 & uVar9;
          }
        }
        else {
          pcVar1 = (char *)(uVar9 + 4 + uVar5);
          uVar9 = ~(0x80000000U >> ((byte)uVar9 - 0x20 & 0x1f));
          puVar10 = (uint *)(uVar5 + 0xc4 + uVar14 * 4);
          *puVar10 = *puVar10 & uVar9;
          *pcVar1 = *pcVar1 + -1;
          if (*pcVar1 == '\0') {
            param_1[1] = param_1[1] & uVar9;
          }
        }
      }
      local_8 = local_8 + uVar6;
      *(undefined4 *)(*(int *)((int)puVar12 + uVar13 + 7) + 4) =
           *(undefined4 *)((int)puVar12 + uVar13 + 3);
      *(undefined4 *)(*(int *)((int)puVar12 + uVar13 + 3) + 8) =
           *(undefined4 *)((int)puVar12 + uVar13 + 7);
    }
    puVar10 = (uint *)(((int)local_8 >> 4) - 1);
    if ((uint *)0x3f < puVar10) {
      puVar10 = (uint *)0x3f;
    }
    puVar11 = param_1;
    if ((uVar7 & 1) == 0) {
      puVar12 = (uint *)((int)puVar12 - uVar7);
      puVar11 = (uint *)(((int)uVar7 >> 4) - 1);
      if ((uint *)0x3f < puVar11) {
        puVar11 = (uint *)0x3f;
      }
      local_8 = local_8 + uVar7;
      puVar10 = (uint *)(((int)local_8 >> 4) - 1);
      if ((uint *)0x3f < puVar10) {
        puVar10 = (uint *)0x3f;
      }
      if (puVar11 != puVar10) {
        if (puVar12[1] == puVar12[2]) {
          if (puVar11 < (uint *)0x20) {
            uVar13 = ~(0x80000000U >> ((byte)puVar11 & 0x1f));
            puVar2 = (uint *)(uVar5 + 0x44 + uVar14 * 4);
            *puVar2 = *puVar2 & uVar13;
            pcVar1 = (char *)((int)puVar11 + uVar5 + 4);
            *pcVar1 = *pcVar1 + -1;
            if (*pcVar1 == '\0') {
              *param_1 = *param_1 & uVar13;
            }
          }
          else {
            uVar13 = ~(0x80000000U >> ((byte)puVar11 - 0x20 & 0x1f));
            puVar2 = (uint *)(uVar5 + 0xc4 + uVar14 * 4);
            *puVar2 = *puVar2 & uVar13;
            pcVar1 = (char *)((int)puVar11 + uVar5 + 4);
            *pcVar1 = *pcVar1 + -1;
            if (*pcVar1 == '\0') {
              param_1[1] = param_1[1] & uVar13;
            }
          }
        }
        *(uint *)(puVar12[2] + 4) = puVar12[1];
        *(uint *)(puVar12[1] + 8) = puVar12[2];
      }
    }
    if (((uVar7 & 1) != 0) || (puVar11 != puVar10)) {
      puVar12[1] = piVar3[(int)puVar10 * 2 + 1];
      puVar12[2] = (uint)(piVar3 + (int)puVar10 * 2);
      (piVar3 + (int)puVar10 * 2)[1] = (int)puVar12;
      *(uint **)(puVar12[1] + 8) = puVar12;
      if (puVar12[1] == puVar12[2]) {
        cVar4 = *(char *)((int)puVar10 + uVar5 + 4);
        *(char *)((int)puVar10 + uVar5 + 4) = cVar4 + '\x01';
        bVar8 = (byte)puVar10;
        if (puVar10 < (uint *)0x20) {
          if (cVar4 == '\0') {
            *param_1 = *param_1 | 0x80000000U >> (bVar8 & 0x1f);
          }
          puVar10 = (uint *)(uVar5 + 0x44 + uVar14 * 4);
          *puVar10 = *puVar10 | 0x80000000U >> (bVar8 & 0x1f);
        }
        else {
          if (cVar4 == '\0') {
            param_1[1] = param_1[1] | 0x80000000U >> (bVar8 - 0x20 & 0x1f);
          }
          puVar10 = (uint *)(uVar5 + 0xc4 + uVar14 * 4);
          *puVar10 = *puVar10 | 0x80000000U >> (bVar8 - 0x20 & 0x1f);
        }
      }
    }
    *puVar12 = local_8;
    *(uint *)((local_8 - 4) + (int)puVar12) = local_8;
    *piVar3 = *piVar3 + -1;
    if (*piVar3 == 0) {
      if (DAT_00409974 != (uint *)0x0) {
        VirtualFree((LPVOID)(DAT_0040996c * 0x8000 + DAT_00409974[3]),0x8000,0x4000);
        DAT_00409974[2] = DAT_00409974[2] | 0x80000000U >> ((byte)DAT_0040996c & 0x1f);
        *(undefined4 *)(DAT_00409974[4] + 0xc4 + DAT_0040996c * 4) = 0;
        *(char *)(DAT_00409974[4] + 0x43) = *(char *)(DAT_00409974[4] + 0x43) + -1;
        if (*(char *)(DAT_00409974[4] + 0x43) == '\0') {
          DAT_00409974[1] = DAT_00409974[1] & 0xfffffffe;
        }
        if (DAT_00409974[2] == 0xffffffff) {
          VirtualFree((LPVOID)DAT_00409974[3],0,0x8000);
          HeapFree(DAT_00409984,0,(LPVOID)DAT_00409974[4]);
          FUN_00404c80(DAT_00409974,DAT_00409974 + 5,
                       (DAT_00409978 * 0x14 - (int)DAT_00409974) + -0x14 + DAT_0040997c);
          DAT_00409978 = DAT_00409978 + -1;
          if (DAT_00409974 < param_1) {
            param_1 = param_1 + -5;
          }
          DAT_00409970 = DAT_0040997c;
        }
      }
      DAT_00409974 = param_1;
      DAT_0040996c = uVar14;
    }
  }
  return;
}



int * __cdecl FUN_00402fa8(uint *param_1)

{
  char *pcVar1;
  int *piVar2;
  char cVar3;
  int *piVar4;
  byte bVar5;
  uint uVar6;
  int iVar7;
  uint *puVar8;
  int iVar9;
  int *piVar10;
  uint *puVar11;
  uint *puVar12;
  uint uVar13;
  int iVar14;
  uint local_10;
  uint local_c;
  int local_8;
  
  puVar8 = DAT_0040997c + DAT_00409978 * 5;
  uVar6 = (int)param_1 + 0x17U & 0xfffffff0;
  iVar7 = ((int)((int)param_1 + 0x17U) >> 4) + -1;
  bVar5 = (byte)iVar7;
  if (iVar7 < 0x20) {
    local_10 = 0xffffffff >> (bVar5 & 0x1f);
    local_c = 0xffffffff;
  }
  else {
    local_c = 0xffffffff >> (bVar5 - 0x20 & 0x1f);
    local_10 = 0;
  }
  param_1 = DAT_00409970;
  if (DAT_00409970 < puVar8) {
    do {
      if ((param_1[1] & local_c) != 0 || (*param_1 & local_10) != 0) break;
      param_1 = param_1 + 5;
    } while (param_1 < puVar8);
  }
  puVar11 = DAT_0040997c;
  if (param_1 == puVar8) {
    for (; (puVar11 < DAT_00409970 && ((puVar11[1] & local_c) == 0 && (*puVar11 & local_10) == 0));
        puVar11 = puVar11 + 5) {
    }
    param_1 = puVar11;
    if (puVar11 == DAT_00409970) {
      for (; (puVar11 < puVar8 && (puVar11[2] == 0)); puVar11 = puVar11 + 5) {
      }
      puVar12 = DAT_0040997c;
      param_1 = puVar11;
      if (puVar11 == puVar8) {
        for (; (puVar12 < DAT_00409970 && (puVar12[2] == 0)); puVar12 = puVar12 + 5) {
        }
        param_1 = puVar12;
        if ((puVar12 == DAT_00409970) && (param_1 = FUN_004032b1(), param_1 == (uint *)0x0)) {
          return (int *)0x0;
        }
      }
      iVar7 = FUN_00403362((int)param_1);
      *(int *)param_1[4] = iVar7;
      if (*(int *)param_1[4] == -1) {
        return (int *)0x0;
      }
    }
  }
  piVar4 = (int *)param_1[4];
  local_8 = *piVar4;
  if ((local_8 == -1) ||
     ((piVar4[local_8 + 0x31] & local_c) == 0 && (piVar4[local_8 + 0x11] & local_10) == 0)) {
    local_8 = 0;
    puVar8 = (uint *)(piVar4 + 0x11);
    if ((piVar4[0x31] & local_c) == 0 && (piVar4[0x11] & local_10) == 0) {
      do {
        puVar11 = puVar8 + 0x21;
        local_8 = local_8 + 1;
        puVar8 = puVar8 + 1;
      } while ((*puVar11 & local_c) == 0 && (local_10 & *puVar8) == 0);
    }
  }
  iVar7 = 0;
  piVar2 = piVar4 + local_8 * 0x81 + 0x51;
  local_10 = piVar4[local_8 + 0x11] & local_10;
  if (local_10 == 0) {
    local_10 = piVar4[local_8 + 0x31] & local_c;
    iVar7 = 0x20;
  }
  for (; -1 < (int)local_10; local_10 = local_10 << 1) {
    iVar7 = iVar7 + 1;
  }
  piVar10 = (int *)piVar2[iVar7 * 2 + 1];
  iVar9 = *piVar10 - uVar6;
  iVar14 = (iVar9 >> 4) + -1;
  if (0x3f < iVar14) {
    iVar14 = 0x3f;
  }
  DAT_00409970 = param_1;
  if (iVar14 != iVar7) {
    if (piVar10[1] == piVar10[2]) {
      if (iVar7 < 0x20) {
        pcVar1 = (char *)((int)piVar4 + iVar7 + 4);
        uVar13 = ~(0x80000000U >> ((byte)iVar7 & 0x1f));
        piVar4[local_8 + 0x11] = uVar13 & piVar4[local_8 + 0x11];
        *pcVar1 = *pcVar1 + -1;
        if (*pcVar1 == '\0') {
          *param_1 = *param_1 & uVar13;
        }
      }
      else {
        pcVar1 = (char *)((int)piVar4 + iVar7 + 4);
        uVar13 = ~(0x80000000U >> ((byte)iVar7 - 0x20 & 0x1f));
        piVar4[local_8 + 0x31] = piVar4[local_8 + 0x31] & uVar13;
        *pcVar1 = *pcVar1 + -1;
        if (*pcVar1 == '\0') {
          param_1[1] = param_1[1] & uVar13;
        }
      }
    }
    *(int *)(piVar10[2] + 4) = piVar10[1];
    *(int *)(piVar10[1] + 8) = piVar10[2];
    if (iVar9 == 0) goto LAB_0040326e;
    piVar10[1] = piVar2[iVar14 * 2 + 1];
    piVar10[2] = (int)(piVar2 + iVar14 * 2);
    (piVar2 + iVar14 * 2)[1] = (int)piVar10;
    *(int **)(piVar10[1] + 8) = piVar10;
    if (piVar10[1] == piVar10[2]) {
      cVar3 = *(char *)(iVar14 + 4 + (int)piVar4);
      bVar5 = (byte)iVar14;
      if (iVar14 < 0x20) {
        *(char *)(iVar14 + 4 + (int)piVar4) = cVar3 + '\x01';
        if (cVar3 == '\0') {
          *param_1 = *param_1 | 0x80000000U >> (bVar5 & 0x1f);
        }
        piVar4[local_8 + 0x11] = piVar4[local_8 + 0x11] | 0x80000000U >> (bVar5 & 0x1f);
      }
      else {
        *(char *)(iVar14 + 4 + (int)piVar4) = cVar3 + '\x01';
        if (cVar3 == '\0') {
          param_1[1] = param_1[1] | 0x80000000U >> (bVar5 - 0x20 & 0x1f);
        }
        piVar4[local_8 + 0x31] = piVar4[local_8 + 0x31] | 0x80000000U >> (bVar5 - 0x20 & 0x1f);
      }
    }
  }
  if (iVar9 != 0) {
    *piVar10 = iVar9;
    *(int *)(iVar9 + -4 + (int)piVar10) = iVar9;
  }
LAB_0040326e:
  piVar10 = (int *)((int)piVar10 + iVar9);
  *piVar10 = uVar6 + 1;
  *(uint *)((int)piVar10 + (uVar6 - 4)) = uVar6 + 1;
  iVar7 = *piVar2;
  *piVar2 = iVar7 + 1;
  if (((iVar7 == 0) && (param_1 == DAT_00409974)) && (local_8 == DAT_0040996c)) {
    DAT_00409974 = (uint *)0x0;
  }
  *piVar4 = local_8;
  return piVar10 + 1;
}



undefined4 * FUN_004032b1(void)

{
  undefined4 *puVar1;
  LPVOID pvVar2;
  
  if (DAT_00409978 == DAT_00409968) {
    pvVar2 = HeapReAlloc(DAT_00409984,0,DAT_0040997c,(DAT_00409968 * 5 + 0x50) * 4);
    if (pvVar2 == (LPVOID)0x0) {
      return (undefined4 *)0x0;
    }
    DAT_00409968 = DAT_00409968 + 0x10;
    DAT_0040997c = pvVar2;
  }
  puVar1 = (undefined4 *)((int)DAT_0040997c + DAT_00409978 * 0x14);
  pvVar2 = HeapAlloc(DAT_00409984,8,0x41c4);
  puVar1[4] = pvVar2;
  if (pvVar2 != (LPVOID)0x0) {
    pvVar2 = VirtualAlloc((LPVOID)0x0,0x100000,0x2000,4);
    puVar1[3] = pvVar2;
    if (pvVar2 != (LPVOID)0x0) {
      puVar1[2] = 0xffffffff;
      *puVar1 = 0;
      puVar1[1] = 0;
      DAT_00409978 = DAT_00409978 + 1;
      *(undefined4 *)puVar1[4] = 0xffffffff;
      return puVar1;
    }
    HeapFree(DAT_00409984,0,(LPVOID)puVar1[4]);
  }
  return (undefined4 *)0x0;
}



int __cdecl FUN_00403362(int param_1)

{
  int *piVar1;
  char cVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  LPVOID pvVar6;
  int *piVar7;
  int iVar8;
  int iVar9;
  int *lpAddress;
  
  iVar3 = *(int *)(param_1 + 0x10);
  iVar9 = 0;
  for (iVar4 = *(int *)(param_1 + 8); -1 < iVar4; iVar4 = iVar4 << 1) {
    iVar9 = iVar9 + 1;
  }
  iVar8 = 0x3f;
  iVar4 = iVar9 * 0x204 + 0x144 + iVar3;
  iVar5 = iVar4;
  do {
    *(int *)(iVar5 + 8) = iVar5;
    *(int *)(iVar5 + 4) = iVar5;
    iVar5 = iVar5 + 8;
    iVar8 = iVar8 + -1;
  } while (iVar8 != 0);
  lpAddress = (int *)(iVar9 * 0x8000 + *(int *)(param_1 + 0xc));
  pvVar6 = VirtualAlloc(lpAddress,0x8000,0x1000,4);
  if (pvVar6 == (LPVOID)0x0) {
    iVar9 = -1;
  }
  else {
    if (lpAddress <= lpAddress + 0x1c00) {
      piVar7 = lpAddress + 4;
      do {
        piVar7[-2] = -1;
        piVar7[0x3fb] = -1;
        piVar7[-1] = 0xff0;
        *piVar7 = (int)(piVar7 + 0x3ff);
        piVar7[1] = (int)(piVar7 + -0x401);
        piVar7[0x3fa] = 0xff0;
        piVar1 = piVar7 + 0x3fc;
        piVar7 = piVar7 + 0x400;
      } while (piVar1 <= lpAddress + 0x1c00);
    }
    *(int **)(iVar4 + 0x1fc) = lpAddress + 3;
    lpAddress[5] = iVar4 + 0x1f8;
    *(int **)(iVar4 + 0x200) = lpAddress + 0x1c03;
    lpAddress[0x1c04] = iVar4 + 0x1f8;
    *(undefined4 *)(iVar3 + 0x44 + iVar9 * 4) = 0;
    *(undefined4 *)(iVar3 + 0xc4 + iVar9 * 4) = 1;
    cVar2 = *(char *)(iVar3 + 0x43);
    *(char *)(iVar3 + 0x43) = cVar2 + '\x01';
    if (cVar2 == '\0') {
      *(uint *)(param_1 + 4) = *(uint *)(param_1 + 4) | 1;
    }
    *(uint *)(param_1 + 8) = *(uint *)(param_1 + 8) & ~(0x80000000U >> ((byte)iVar9 & 0x1f));
  }
  return iVar9;
}



undefined4 __cdecl FUN_0040345d(uint *param_1,int param_2,int param_3)

{
  char *pcVar1;
  int *piVar2;
  int iVar3;
  char cVar4;
  uint uVar5;
  int iVar6;
  uint *puVar7;
  byte bVar8;
  int iVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  uint local_c;
  
  uVar5 = param_1[4];
  uVar12 = param_3 + 0x17U & 0xfffffff0;
  uVar10 = param_2 - param_1[3] >> 0xf;
  iVar3 = uVar10 * 0x204 + 0x144 + uVar5;
  iVar6 = *(int *)(param_2 + -4);
  iVar9 = iVar6 + -1;
  uVar13 = *(uint *)(iVar6 + -5 + param_2);
  iVar6 = iVar6 + -5 + param_2;
  if (iVar9 < (int)uVar12) {
    if (((uVar13 & 1) != 0) || ((int)(uVar13 + iVar9) < (int)uVar12)) {
      return 0;
    }
    local_c = ((int)uVar13 >> 4) - 1;
    if (0x3f < local_c) {
      local_c = 0x3f;
    }
    if (*(int *)(iVar6 + 4) == *(int *)(iVar6 + 8)) {
      if (local_c < 0x20) {
        pcVar1 = (char *)(local_c + 4 + uVar5);
        uVar11 = ~(0x80000000U >> ((byte)local_c & 0x1f));
        puVar7 = (uint *)(uVar5 + 0x44 + uVar10 * 4);
        *puVar7 = *puVar7 & uVar11;
        *pcVar1 = *pcVar1 + -1;
        if (*pcVar1 == '\0') {
          *param_1 = *param_1 & uVar11;
        }
      }
      else {
        pcVar1 = (char *)(local_c + 4 + uVar5);
        uVar11 = ~(0x80000000U >> ((byte)local_c - 0x20 & 0x1f));
        puVar7 = (uint *)(uVar5 + 0xc4 + uVar10 * 4);
        *puVar7 = *puVar7 & uVar11;
        *pcVar1 = *pcVar1 + -1;
        if (*pcVar1 == '\0') {
          param_1[1] = param_1[1] & uVar11;
        }
      }
    }
    *(undefined4 *)(*(int *)(iVar6 + 8) + 4) = *(undefined4 *)(iVar6 + 4);
    *(undefined4 *)(*(int *)(iVar6 + 4) + 8) = *(undefined4 *)(iVar6 + 8);
    iVar6 = uVar13 + (iVar9 - uVar12);
    if (0 < iVar6) {
      uVar13 = (iVar6 >> 4) - 1;
      iVar9 = param_2 + -4 + uVar12;
      if (0x3f < uVar13) {
        uVar13 = 0x3f;
      }
      iVar3 = iVar3 + uVar13 * 8;
      *(undefined4 *)(iVar9 + 4) = *(undefined4 *)(iVar3 + 4);
      *(int *)(iVar9 + 8) = iVar3;
      *(int *)(iVar3 + 4) = iVar9;
      *(int *)(*(int *)(iVar9 + 4) + 8) = iVar9;
      if (*(int *)(iVar9 + 4) == *(int *)(iVar9 + 8)) {
        cVar4 = *(char *)(uVar13 + 4 + uVar5);
        *(char *)(uVar13 + 4 + uVar5) = cVar4 + '\x01';
        bVar8 = (byte)uVar13;
        if (uVar13 < 0x20) {
          if (cVar4 == '\0') {
            *param_1 = *param_1 | 0x80000000U >> (bVar8 & 0x1f);
          }
          puVar7 = (uint *)(uVar5 + 0x44 + uVar10 * 4);
        }
        else {
          if (cVar4 == '\0') {
            param_1[1] = param_1[1] | 0x80000000U >> (bVar8 - 0x20 & 0x1f);
          }
          puVar7 = (uint *)(uVar5 + 0xc4 + uVar10 * 4);
          bVar8 = bVar8 - 0x20;
        }
        *puVar7 = *puVar7 | 0x80000000U >> (bVar8 & 0x1f);
      }
      piVar2 = (int *)(param_2 + -4 + uVar12);
      *piVar2 = iVar6;
      *(int *)(iVar6 + -4 + (int)piVar2) = iVar6;
    }
    *(uint *)(param_2 + -4) = uVar12 + 1;
    *(uint *)(param_2 + -8 + uVar12) = uVar12 + 1;
  }
  else if ((int)uVar12 < iVar9) {
    param_3 = iVar9 - uVar12;
    *(uint *)(param_2 + -4) = uVar12 + 1;
    piVar2 = (int *)(param_2 + -4 + uVar12);
    uVar11 = (param_3 >> 4) - 1;
    piVar2[-1] = uVar12 + 1;
    if (0x3f < uVar11) {
      uVar11 = 0x3f;
    }
    if ((uVar13 & 1) == 0) {
      uVar12 = ((int)uVar13 >> 4) - 1;
      if (0x3f < uVar12) {
        uVar12 = 0x3f;
      }
      if (*(int *)(iVar6 + 4) == *(int *)(iVar6 + 8)) {
        if (uVar12 < 0x20) {
          pcVar1 = (char *)(uVar12 + 4 + uVar5);
          uVar12 = ~(0x80000000U >> ((byte)uVar12 & 0x1f));
          puVar7 = (uint *)(uVar5 + 0x44 + uVar10 * 4);
          *puVar7 = *puVar7 & uVar12;
          *pcVar1 = *pcVar1 + -1;
          if (*pcVar1 == '\0') {
            *param_1 = *param_1 & uVar12;
          }
        }
        else {
          pcVar1 = (char *)(uVar12 + 4 + uVar5);
          uVar12 = ~(0x80000000U >> ((byte)uVar12 - 0x20 & 0x1f));
          puVar7 = (uint *)(uVar5 + 0xc4 + uVar10 * 4);
          *puVar7 = *puVar7 & uVar12;
          *pcVar1 = *pcVar1 + -1;
          if (*pcVar1 == '\0') {
            param_1[1] = param_1[1] & uVar12;
          }
        }
      }
      *(undefined4 *)(*(int *)(iVar6 + 8) + 4) = *(undefined4 *)(iVar6 + 4);
      *(undefined4 *)(*(int *)(iVar6 + 4) + 8) = *(undefined4 *)(iVar6 + 8);
      param_3 = param_3 + uVar13;
      uVar11 = (param_3 >> 4) - 1;
      if (0x3f < uVar11) {
        uVar11 = 0x3f;
      }
    }
    iVar6 = iVar3 + uVar11 * 8;
    piVar2[1] = *(int *)(iVar3 + 4 + uVar11 * 8);
    piVar2[2] = iVar6;
    *(int **)(iVar6 + 4) = piVar2;
    *(int **)(piVar2[1] + 8) = piVar2;
    if (piVar2[1] == piVar2[2]) {
      cVar4 = *(char *)(uVar11 + 4 + uVar5);
      *(char *)(uVar11 + 4 + uVar5) = cVar4 + '\x01';
      bVar8 = (byte)uVar11;
      if (uVar11 < 0x20) {
        if (cVar4 == '\0') {
          *param_1 = *param_1 | 0x80000000U >> (bVar8 & 0x1f);
        }
        puVar7 = (uint *)(uVar5 + 0x44 + uVar10 * 4);
      }
      else {
        if (cVar4 == '\0') {
          param_1[1] = param_1[1] | 0x80000000U >> (bVar8 - 0x20 & 0x1f);
        }
        puVar7 = (uint *)(uVar5 + 0xc4 + uVar10 * 4);
        bVar8 = bVar8 - 0x20;
      }
      *puVar7 = *puVar7 | 0x80000000U >> (bVar8 & 0x1f);
    }
    *piVar2 = param_3;
    *(int *)(param_3 + -4 + (int)piVar2) = param_3;
  }
  return 1;
}



undefined ** FUN_00403753(void)

{
  bool bVar1;
  int *lpAddress;
  LPVOID pvVar2;
  undefined **ppuVar3;
  int iVar4;
  undefined **lpMem;
  
  if (DAT_00407218 == -1) {
    lpMem = &PTR_LOOP_00407208;
  }
  else {
    lpMem = (undefined **)HeapAlloc(DAT_00409984,0,0x2020);
    if (lpMem == (undefined **)0x0) {
      return (undefined **)0x0;
    }
  }
  lpAddress = (int *)VirtualAlloc((LPVOID)0x0,0x400000,0x2000,4);
  if (lpAddress != (int *)0x0) {
    pvVar2 = VirtualAlloc(lpAddress,0x10000,0x1000,4);
    if (pvVar2 != (LPVOID)0x0) {
      if (lpMem == &PTR_LOOP_00407208) {
        if (PTR_LOOP_00407208 == (undefined *)0x0) {
          PTR_LOOP_00407208 = (undefined *)&PTR_LOOP_00407208;
        }
        if (PTR_LOOP_0040720c == (undefined *)0x0) {
          PTR_LOOP_0040720c = (undefined *)&PTR_LOOP_00407208;
        }
      }
      else {
        *lpMem = (undefined *)&PTR_LOOP_00407208;
        lpMem[1] = PTR_LOOP_0040720c;
        PTR_LOOP_0040720c = (undefined *)lpMem;
        *(undefined ***)lpMem[1] = lpMem;
      }
      lpMem[5] = (undefined *)(lpAddress + 0x100000);
      ppuVar3 = lpMem + 6;
      lpMem[3] = (undefined *)(lpMem + 0x26);
      lpMem[4] = (undefined *)lpAddress;
      lpMem[2] = (undefined *)ppuVar3;
      iVar4 = 0;
      do {
        bVar1 = 0xf < iVar4;
        iVar4 = iVar4 + 1;
        *ppuVar3 = (undefined *)((bVar1 - 1 & 0xf1) - 1);
        ppuVar3[1] = (undefined *)0xf1;
        ppuVar3 = ppuVar3 + 2;
      } while (iVar4 < 0x400);
      _memset(lpAddress,0,0x10000);
      for (; lpAddress < lpMem[4] + 0x10000; lpAddress = lpAddress + 0x400) {
        *(undefined1 *)(lpAddress + 0x3e) = 0xff;
        *lpAddress = (int)(lpAddress + 2);
        lpAddress[1] = 0xf0;
      }
      return lpMem;
    }
    VirtualFree(lpAddress,0,0x8000);
  }
  if (lpMem != &PTR_LOOP_00407208) {
    HeapFree(DAT_00409984,0,lpMem);
  }
  return (undefined **)0x0;
}



void __cdecl FUN_00403897(undefined **param_1)

{
  VirtualFree(param_1[4],0,0x8000);
  if ((undefined **)PTR_LOOP_00409228 == param_1) {
    PTR_LOOP_00409228 = param_1[1];
  }
  if (param_1 != &PTR_LOOP_00407208) {
    *(undefined **)param_1[1] = *param_1;
    *(undefined **)(*param_1 + 4) = param_1[1];
    HeapFree(DAT_00409984,0,param_1);
    return;
  }
  DAT_00407218 = 0xffffffff;
  return;
}



void __cdecl FUN_004038ed(int param_1)

{
  BOOL BVar1;
  undefined **ppuVar2;
  int iVar3;
  undefined **ppuVar4;
  undefined **ppuVar5;
  int local_8;
  
  ppuVar4 = (undefined **)PTR_LOOP_0040720c;
  do {
    ppuVar5 = ppuVar4;
    if (ppuVar4[4] != (undefined *)0xffffffff) {
      local_8 = 0;
      ppuVar5 = ppuVar4 + 0x804;
      iVar3 = 0x3ff000;
      do {
        if (*ppuVar5 == (undefined *)0xf0) {
          BVar1 = VirtualFree(ppuVar4[4] + iVar3,0x1000,0x4000);
          if (BVar1 != 0) {
            *ppuVar5 = (undefined *)0xffffffff;
            DAT_00409704 = DAT_00409704 + -1;
            if (((undefined **)ppuVar4[3] == (undefined **)0x0) || (ppuVar5 < ppuVar4[3])) {
              ppuVar4[3] = (undefined *)ppuVar5;
            }
            local_8 = local_8 + 1;
            param_1 = param_1 + -1;
            if (param_1 == 0) break;
          }
        }
        iVar3 = iVar3 + -0x1000;
        ppuVar5 = ppuVar5 + -2;
      } while (-1 < iVar3);
      ppuVar5 = (undefined **)ppuVar4[1];
      if ((local_8 != 0) && (ppuVar4[6] == (undefined *)0xffffffff)) {
        ppuVar2 = ppuVar4 + 8;
        iVar3 = 1;
        do {
          if (*ppuVar2 != (undefined *)0xffffffff) break;
          iVar3 = iVar3 + 1;
          ppuVar2 = ppuVar2 + 2;
        } while (iVar3 < 0x400);
        if (iVar3 == 0x400) {
          FUN_00403897(ppuVar4);
        }
      }
    }
    if ((ppuVar5 == (undefined **)PTR_LOOP_0040720c) || (ppuVar4 = ppuVar5, param_1 < 1)) {
      return;
    }
  } while( true );
}



int __cdecl FUN_004039af(undefined *param_1,undefined4 *param_2,uint *param_3)

{
  undefined **ppuVar1;
  uint uVar2;
  
  ppuVar1 = &PTR_LOOP_00407208;
  while ((param_1 <= ppuVar1[4] || (ppuVar1[5] <= param_1))) {
    ppuVar1 = (undefined **)*ppuVar1;
    if (ppuVar1 == &PTR_LOOP_00407208) {
      return 0;
    }
  }
  if (((uint)param_1 & 0xf) != 0) {
    return 0;
  }
  if (((uint)param_1 & 0xfff) < 0x100) {
    return 0;
  }
  *param_2 = ppuVar1;
  uVar2 = (uint)param_1 & 0xfffff000;
  *param_3 = uVar2;
  return ((int)(param_1 + (-0x100 - uVar2)) >> 4) + 8 + uVar2;
}



void __cdecl FUN_00403a06(int param_1,int param_2,byte *param_3)

{
  int *piVar1;
  
  piVar1 = (int *)(param_1 + 0x18 + (param_2 - *(int *)(param_1 + 0x10) >> 0xc) * 8);
  *piVar1 = *piVar1 + (uint)*param_3;
  *param_3 = 0;
  piVar1[1] = 0xf1;
  if ((*piVar1 == 0xf0) && (DAT_00409704 = DAT_00409704 + 1, DAT_00409704 == 0x20)) {
    FUN_004038ed(0x10);
  }
  return;
}



// WARNING: Type propagation algorithm not settling

int * __cdecl FUN_00403a4b(uint param_1)

{
  uint *puVar1;
  undefined **ppuVar2;
  undefined *puVar3;
  int *piVar4;
  int *piVar5;
  undefined **ppuVar6;
  int *piVar7;
  uint *puVar8;
  undefined **ppuVar9;
  int local_8;
  
  piVar7 = (int *)PTR_LOOP_00409228;
  do {
    if (piVar7[4] != -1) {
      puVar8 = (uint *)piVar7[2];
      piVar4 = (int *)(((int)puVar8 + (-0x18 - (int)piVar7) >> 3) * 0x1000 + piVar7[4]);
      if (puVar8 < piVar7 + 0x806) {
        do {
          if (((int)param_1 <= (int)*puVar8) && (param_1 < puVar8[1])) {
            piVar5 = (int *)FUN_00403c53(piVar4,*puVar8,param_1);
            if (piVar5 != (int *)0x0) goto LAB_00403b16;
            puVar8[1] = param_1;
          }
          puVar8 = puVar8 + 2;
          piVar4 = piVar4 + 0x400;
        } while (puVar8 < piVar7 + 0x806);
      }
      puVar1 = (uint *)piVar7[2];
      piVar4 = (int *)piVar7[4];
      for (puVar8 = (uint *)(piVar7 + 6); puVar8 < puVar1; puVar8 = puVar8 + 2) {
        if (((int)param_1 <= (int)*puVar8) && (param_1 < puVar8[1])) {
          piVar5 = (int *)FUN_00403c53(piVar4,*puVar8,param_1);
          if (piVar5 != (int *)0x0) {
LAB_00403b16:
            PTR_LOOP_00409228 = (undefined *)piVar7;
            *puVar8 = *puVar8 - param_1;
            piVar7[2] = (int)puVar8;
            return piVar5;
          }
          puVar8[1] = param_1;
        }
        piVar4 = piVar4 + 0x400;
      }
    }
    piVar7 = (int *)*piVar7;
    if (piVar7 == (int *)PTR_LOOP_00409228) {
      ppuVar9 = &PTR_LOOP_00407208;
      while ((ppuVar9[4] == (undefined *)0xffffffff || (ppuVar9[3] == (undefined *)0x0))) {
        ppuVar9 = (undefined **)*ppuVar9;
        if (ppuVar9 == &PTR_LOOP_00407208) {
          ppuVar9 = FUN_00403753();
          if (ppuVar9 == (undefined **)0x0) {
            return (int *)0x0;
          }
          piVar7 = (int *)ppuVar9[4];
          *(char *)(piVar7 + 2) = (char)param_1;
          PTR_LOOP_00409228 = (undefined *)ppuVar9;
          *piVar7 = (int)piVar7 + param_1 + 8;
          piVar7[1] = 0xf0 - param_1;
          ppuVar9[6] = ppuVar9[6] + -(param_1 & 0xff);
          return piVar7 + 0x40;
        }
      }
      ppuVar2 = (undefined **)ppuVar9[3];
      local_8 = 0;
      piVar7 = (int *)(ppuVar9[4] + ((int)ppuVar2 + (-0x18 - (int)ppuVar9) >> 3) * 0x1000);
      puVar3 = *ppuVar2;
      ppuVar6 = ppuVar2;
      for (; (puVar3 == (undefined *)0xffffffff && (local_8 < 0x10)); local_8 = local_8 + 1) {
        ppuVar6 = ppuVar6 + 2;
        puVar3 = *ppuVar6;
      }
      piVar4 = (int *)VirtualAlloc(piVar7,local_8 << 0xc,0x1000,4);
      if (piVar4 != piVar7) {
        return (int *)0x0;
      }
      _memset(piVar7,local_8 << 0xc,0);
      ppuVar6 = ppuVar2;
      if (0 < local_8) {
        piVar4 = piVar7 + 1;
        do {
          *(undefined1 *)(piVar4 + 0x3d) = 0xff;
          piVar4[-1] = (int)(piVar4 + 1);
          *piVar4 = 0xf0;
          *ppuVar6 = (undefined *)0xf0;
          ppuVar6[1] = (undefined *)0xf1;
          piVar4 = piVar4 + 0x400;
          ppuVar6 = ppuVar6 + 2;
          local_8 = local_8 + -1;
        } while (local_8 != 0);
      }
      for (; (ppuVar6 < ppuVar9 + 0x806 && (*ppuVar6 != (undefined *)0xffffffff));
          ppuVar6 = ppuVar6 + 2) {
      }
      PTR_LOOP_00409228 = (undefined *)ppuVar9;
      ppuVar9[3] = (undefined *)(-(uint)(ppuVar6 < ppuVar9 + 0x806) & (uint)ppuVar6);
      *(char *)(piVar7 + 2) = (char)param_1;
      ppuVar9[2] = (undefined *)ppuVar2;
      *ppuVar2 = *ppuVar2 + -param_1;
      piVar7[1] = piVar7[1] - param_1;
      *piVar7 = (int)piVar7 + param_1 + 8;
      return piVar7 + 0x40;
    }
  } while( true );
}



int __cdecl FUN_00403c53(int *param_1,uint param_2,uint param_3)

{
  byte *pbVar1;
  byte *pbVar2;
  byte bVar3;
  byte *pbVar4;
  uint uVar5;
  byte *pbVar6;
  
  pbVar2 = (byte *)*param_1;
  pbVar1 = (byte *)(param_1 + 0x3e);
  bVar3 = (byte)param_3;
  if ((uint)param_1[1] < param_3) {
    pbVar6 = pbVar2;
    if (pbVar2[param_1[1]] != 0) {
      pbVar6 = pbVar2 + param_1[1];
    }
    while( true ) {
      while( true ) {
        if (pbVar1 <= pbVar6 + param_3) {
          pbVar6 = (byte *)(param_1 + 2);
          while( true ) {
            while( true ) {
              if (pbVar2 <= pbVar6) {
                return 0;
              }
              if (pbVar1 <= pbVar6 + param_3) {
                return 0;
              }
              if (*pbVar6 == 0) break;
              pbVar6 = pbVar6 + *pbVar6;
            }
            uVar5 = 1;
            pbVar4 = pbVar6;
            while (pbVar4 = pbVar4 + 1, *pbVar4 == 0) {
              uVar5 = uVar5 + 1;
            }
            if (param_3 <= uVar5) break;
            param_2 = param_2 - uVar5;
            pbVar6 = pbVar4;
            if (param_2 < param_3) {
              return 0;
            }
          }
          if (pbVar6 + param_3 < pbVar1) {
            *param_1 = (int)(pbVar6 + param_3);
            param_1[1] = uVar5 - param_3;
          }
          else {
            param_1[1] = 0;
            *param_1 = (int)(param_1 + 2);
          }
          *pbVar6 = bVar3;
          pbVar2 = pbVar6 + 8;
          goto LAB_00403d66;
        }
        if (*pbVar6 == 0) break;
        pbVar6 = pbVar6 + *pbVar6;
      }
      uVar5 = 1;
      pbVar4 = pbVar6;
      while (pbVar4 = pbVar4 + 1, *pbVar4 == 0) {
        uVar5 = uVar5 + 1;
      }
      if (param_3 <= uVar5) break;
      if (pbVar6 == pbVar2) {
        param_1[1] = uVar5;
        pbVar6 = pbVar4;
      }
      else {
        param_2 = param_2 - uVar5;
        pbVar6 = pbVar4;
        if (param_2 < param_3) {
          return 0;
        }
      }
    }
    if (pbVar6 + param_3 < pbVar1) {
      *param_1 = (int)(pbVar6 + param_3);
      param_1[1] = uVar5 - param_3;
    }
    else {
      param_1[1] = 0;
      *param_1 = (int)(param_1 + 2);
    }
    *pbVar6 = bVar3;
    pbVar2 = pbVar6 + 8;
  }
  else {
    *pbVar2 = bVar3;
    if (pbVar2 + param_3 < pbVar1) {
      *param_1 = *param_1 + param_3;
      param_1[1] = param_1[1] - param_3;
    }
    else {
      param_1[1] = 0;
      *param_1 = (int)(param_1 + 2);
    }
    pbVar2 = pbVar2 + 8;
  }
LAB_00403d66:
  return (int)pbVar2 * 0x10 + (int)param_1 * -0xf;
}



undefined4 __cdecl FUN_00403d77(int param_1,int *param_2,byte *param_3,uint param_4)

{
  byte *pbVar1;
  int *piVar2;
  byte bVar3;
  byte *pbVar4;
  int iVar5;
  uint uVar6;
  
  uVar6 = (uint)*param_3;
  piVar2 = (int *)(param_1 + 0x18 + ((int)param_2 - *(int *)(param_1 + 0x10) >> 0xc) * 8);
  if (param_4 < uVar6) {
    *param_3 = (byte)param_4;
    *piVar2 = *piVar2 + (uVar6 - param_4);
    piVar2[1] = 0xf1;
  }
  else {
    if (param_4 <= uVar6) {
      return 0;
    }
    pbVar1 = param_3 + param_4;
    if (param_2 + 0x3e < pbVar1) {
      return 0;
    }
    for (pbVar4 = param_3 + uVar6; (pbVar4 < pbVar1 && (*pbVar4 == 0)); pbVar4 = pbVar4 + 1) {
    }
    if (pbVar4 != pbVar1) {
      return 0;
    }
    *param_3 = (byte)param_4;
    if ((param_3 <= (byte *)*param_2) && ((byte *)*param_2 < pbVar1)) {
      if (pbVar1 < param_2 + 0x3e) {
        iVar5 = 0;
        *param_2 = (int)pbVar1;
        bVar3 = *pbVar1;
        while (bVar3 == 0) {
          iVar5 = iVar5 + 1;
          bVar3 = pbVar1[iVar5];
        }
        param_2[1] = iVar5;
      }
      else {
        param_2[1] = 0;
        *param_2 = (int)(param_2 + 2);
      }
    }
    *piVar2 = *piVar2 + (uVar6 - param_4);
  }
  return 1;
}



undefined4 __cdecl FUN_00403e20(undefined4 param_1)

{
  int iVar1;
  
  if (DAT_00409708 != (code *)0x0) {
    iVar1 = (*DAT_00409708)(param_1);
    if (iVar1 != 0) {
      return 1;
    }
  }
  return 0;
}



undefined4 * __cdecl FUN_00403e40(undefined4 *param_1,undefined4 *param_2,uint param_3)

{
  uint uVar1;
  uint uVar2;
  undefined4 *puVar3;
  undefined4 *puVar4;
  
  if ((param_2 < param_1) && (param_1 < (undefined4 *)(param_3 + (int)param_2))) {
    puVar3 = (undefined4 *)((param_3 - 4) + (int)param_2);
    puVar4 = (undefined4 *)((param_3 - 4) + (int)param_1);
    if (((uint)puVar4 & 3) == 0) {
      uVar1 = param_3 >> 2;
      uVar2 = param_3 & 3;
      if (7 < uVar1) {
        for (; uVar1 != 0; uVar1 = uVar1 - 1) {
          *puVar4 = *puVar3;
          puVar3 = puVar3 + -1;
          puVar4 = puVar4 + -1;
        }
        switch(uVar2) {
        case 0:
          return param_1;
        case 2:
          goto switchD_00403ff7_caseD_2;
        case 3:
          goto switchD_00403ff7_caseD_3;
        }
        goto switchD_00403ff7_caseD_1;
      }
    }
    else {
      switch(param_3) {
      case 0:
        goto switchD_00403ff7_caseD_0;
      case 1:
        goto switchD_00403ff7_caseD_1;
      case 2:
        goto switchD_00403ff7_caseD_2;
      case 3:
        goto switchD_00403ff7_caseD_3;
      default:
        uVar1 = param_3 - ((uint)puVar4 & 3);
        switch((uint)puVar4 & 3) {
        case 1:
          uVar2 = uVar1 & 3;
          *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
          puVar3 = (undefined4 *)((int)puVar3 + -1);
          uVar1 = uVar1 >> 2;
          puVar4 = (undefined4 *)((int)puVar4 - 1);
          if (7 < uVar1) {
            for (; uVar1 != 0; uVar1 = uVar1 - 1) {
              *puVar4 = *puVar3;
              puVar3 = puVar3 + -1;
              puVar4 = puVar4 + -1;
            }
            switch(uVar2) {
            case 0:
              return param_1;
            case 2:
              goto switchD_00403ff7_caseD_2;
            case 3:
              goto switchD_00403ff7_caseD_3;
            }
            goto switchD_00403ff7_caseD_1;
          }
          break;
        case 2:
          uVar2 = uVar1 & 3;
          *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
          uVar1 = uVar1 >> 2;
          *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
          puVar3 = (undefined4 *)((int)puVar3 + -2);
          puVar4 = (undefined4 *)((int)puVar4 - 2);
          if (7 < uVar1) {
            for (; uVar1 != 0; uVar1 = uVar1 - 1) {
              *puVar4 = *puVar3;
              puVar3 = puVar3 + -1;
              puVar4 = puVar4 + -1;
            }
            switch(uVar2) {
            case 0:
              return param_1;
            case 2:
              goto switchD_00403ff7_caseD_2;
            case 3:
              goto switchD_00403ff7_caseD_3;
            }
            goto switchD_00403ff7_caseD_1;
          }
          break;
        case 3:
          uVar2 = uVar1 & 3;
          *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
          *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
          uVar1 = uVar1 >> 2;
          *(undefined1 *)((int)puVar4 + 1) = *(undefined1 *)((int)puVar3 + 1);
          puVar3 = (undefined4 *)((int)puVar3 + -3);
          puVar4 = (undefined4 *)((int)puVar4 - 3);
          if (7 < uVar1) {
            for (; uVar1 != 0; uVar1 = uVar1 - 1) {
              *puVar4 = *puVar3;
              puVar3 = puVar3 + -1;
              puVar4 = puVar4 + -1;
            }
            switch(uVar2) {
            case 0:
              return param_1;
            case 2:
              goto switchD_00403ff7_caseD_2;
            case 3:
              goto switchD_00403ff7_caseD_3;
            }
            goto switchD_00403ff7_caseD_1;
          }
        }
      }
    }
    switch(uVar1) {
    case 7:
      puVar4[7 - uVar1] = puVar3[7 - uVar1];
    case 6:
      puVar4[6 - uVar1] = puVar3[6 - uVar1];
    case 5:
      puVar4[5 - uVar1] = puVar3[5 - uVar1];
    case 4:
      puVar4[4 - uVar1] = puVar3[4 - uVar1];
    case 3:
      puVar4[3 - uVar1] = puVar3[3 - uVar1];
    case 2:
      puVar4[2 - uVar1] = puVar3[2 - uVar1];
    case 1:
      puVar4[1 - uVar1] = puVar3[1 - uVar1];
      puVar3 = puVar3 + -uVar1;
      puVar4 = puVar4 + -uVar1;
    }
    switch(uVar2) {
    case 1:
switchD_00403ff7_caseD_1:
      *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
      return param_1;
    case 2:
switchD_00403ff7_caseD_2:
      *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
      *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
      return param_1;
    case 3:
switchD_00403ff7_caseD_3:
      *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
      *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
      *(undefined1 *)((int)puVar4 + 1) = *(undefined1 *)((int)puVar3 + 1);
      return param_1;
    }
switchD_00403ff7_caseD_0:
    return param_1;
  }
  puVar3 = param_1;
  if (((uint)param_1 & 3) == 0) {
    uVar1 = param_3 >> 2;
    uVar2 = param_3 & 3;
    if (7 < uVar1) {
      for (; uVar1 != 0; uVar1 = uVar1 - 1) {
        *puVar3 = *param_2;
        param_2 = param_2 + 1;
        puVar3 = puVar3 + 1;
      }
      switch(uVar2) {
      case 0:
        return param_1;
      case 2:
        goto switchD_00403e75_caseD_2;
      case 3:
        goto switchD_00403e75_caseD_3;
      }
      goto switchD_00403e75_caseD_1;
    }
  }
  else {
    switch(param_3) {
    case 0:
      goto switchD_00403e75_caseD_0;
    case 1:
      goto switchD_00403e75_caseD_1;
    case 2:
      goto switchD_00403e75_caseD_2;
    case 3:
      goto switchD_00403e75_caseD_3;
    default:
      uVar1 = (param_3 - 4) + ((uint)param_1 & 3);
      switch((uint)param_1 & 3) {
      case 1:
        uVar2 = uVar1 & 3;
        *(undefined1 *)param_1 = *(undefined1 *)param_2;
        *(undefined1 *)((int)param_1 + 1) = *(undefined1 *)((int)param_2 + 1);
        uVar1 = uVar1 >> 2;
        *(undefined1 *)((int)param_1 + 2) = *(undefined1 *)((int)param_2 + 2);
        param_2 = (undefined4 *)((int)param_2 + 3);
        puVar3 = (undefined4 *)((int)param_1 + 3);
        if (7 < uVar1) {
          for (; uVar1 != 0; uVar1 = uVar1 - 1) {
            *puVar3 = *param_2;
            param_2 = param_2 + 1;
            puVar3 = puVar3 + 1;
          }
          switch(uVar2) {
          case 0:
            return param_1;
          case 2:
            goto switchD_00403e75_caseD_2;
          case 3:
            goto switchD_00403e75_caseD_3;
          }
          goto switchD_00403e75_caseD_1;
        }
        break;
      case 2:
        uVar2 = uVar1 & 3;
        *(undefined1 *)param_1 = *(undefined1 *)param_2;
        uVar1 = uVar1 >> 2;
        *(undefined1 *)((int)param_1 + 1) = *(undefined1 *)((int)param_2 + 1);
        param_2 = (undefined4 *)((int)param_2 + 2);
        puVar3 = (undefined4 *)((int)param_1 + 2);
        if (7 < uVar1) {
          for (; uVar1 != 0; uVar1 = uVar1 - 1) {
            *puVar3 = *param_2;
            param_2 = param_2 + 1;
            puVar3 = puVar3 + 1;
          }
          switch(uVar2) {
          case 0:
            return param_1;
          case 2:
            goto switchD_00403e75_caseD_2;
          case 3:
            goto switchD_00403e75_caseD_3;
          }
          goto switchD_00403e75_caseD_1;
        }
        break;
      case 3:
        uVar2 = uVar1 & 3;
        *(undefined1 *)param_1 = *(undefined1 *)param_2;
        param_2 = (undefined4 *)((int)param_2 + 1);
        uVar1 = uVar1 >> 2;
        puVar3 = (undefined4 *)((int)param_1 + 1);
        if (7 < uVar1) {
          for (; uVar1 != 0; uVar1 = uVar1 - 1) {
            *puVar3 = *param_2;
            param_2 = param_2 + 1;
            puVar3 = puVar3 + 1;
          }
          switch(uVar2) {
          case 0:
            return param_1;
          case 2:
            goto switchD_00403e75_caseD_2;
          case 3:
            goto switchD_00403e75_caseD_3;
          }
          goto switchD_00403e75_caseD_1;
        }
      }
    }
  }
  switch(uVar1) {
  case 7:
    puVar3[uVar1 - 7] = param_2[uVar1 - 7];
  case 6:
    puVar3[uVar1 - 6] = param_2[uVar1 - 6];
  case 5:
    puVar3[uVar1 - 5] = param_2[uVar1 - 5];
  case 4:
    puVar3[uVar1 - 4] = param_2[uVar1 - 4];
  case 3:
    puVar3[uVar1 - 3] = param_2[uVar1 - 3];
  case 2:
    puVar3[uVar1 - 2] = param_2[uVar1 - 2];
  case 1:
    puVar3[uVar1 - 1] = param_2[uVar1 - 1];
    param_2 = param_2 + uVar1;
    puVar3 = puVar3 + uVar1;
  }
  switch(uVar2) {
  case 1:
switchD_00403e75_caseD_1:
    *(undefined1 *)puVar3 = *(undefined1 *)param_2;
    return param_1;
  case 2:
switchD_00403e75_caseD_2:
    *(undefined1 *)puVar3 = *(undefined1 *)param_2;
    *(undefined1 *)((int)puVar3 + 1) = *(undefined1 *)((int)param_2 + 1);
    return param_1;
  case 3:
switchD_00403e75_caseD_3:
    *(undefined1 *)puVar3 = *(undefined1 *)param_2;
    *(undefined1 *)((int)puVar3 + 1) = *(undefined1 *)((int)param_2 + 1);
    *(undefined1 *)((int)puVar3 + 2) = *(undefined1 *)((int)param_2 + 2);
    return param_1;
  }
switchD_00403e75_caseD_0:
  return param_1;
}



void __cdecl FUN_00404175(undefined4 param_1)

{
  FUN_00404186(param_1,0,4);
  return;
}



undefined4 FUN_00404186(byte param_1,uint param_2,byte param_3)

{
  if (((&DAT_00409861)[param_1] & param_3) == 0) {
    if (param_2 == 0) {
      param_2 = 0;
    }
    else {
      param_2 = *(ushort *)(&DAT_0040933a + (uint)param_1 * 2) & param_2;
    }
    if (param_2 == 0) {
      return 0;
    }
  }
  return 1;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

undefined4 __cdecl FUN_004041b7(int param_1)

{
  BYTE *pBVar1;
  byte bVar2;
  byte bVar3;
  UINT CodePage;
  UINT *pUVar4;
  BOOL BVar5;
  uint uVar6;
  BYTE *pBVar7;
  int iVar8;
  byte *pbVar9;
  int iVar10;
  byte *pbVar11;
  undefined4 *puVar12;
  _cpinfo local_1c;
  uint local_8;
  
  CodePage = FUN_00404350(param_1);
  if (CodePage == DAT_00409748) {
    return 0;
  }
  if (CodePage != 0) {
    iVar10 = 0;
    pUVar4 = &DAT_00409238;
    do {
      if (*pUVar4 == CodePage) {
        puVar12 = (undefined4 *)&DAT_00409860;
        for (iVar8 = 0x40; iVar8 != 0; iVar8 = iVar8 + -1) {
          *puVar12 = 0;
          puVar12 = puVar12 + 1;
        }
        local_8 = 0;
        *(undefined1 *)puVar12 = 0;
        pbVar11 = &DAT_00409248 + iVar10 * 0x30;
        do {
          bVar2 = *pbVar11;
          pbVar9 = pbVar11;
          while ((bVar2 != 0 && (bVar2 = pbVar9[1], bVar2 != 0))) {
            uVar6 = (uint)*pbVar9;
            if (uVar6 <= bVar2) {
              bVar3 = (&DAT_00409230)[local_8];
              do {
                (&DAT_00409861)[uVar6] = (&DAT_00409861)[uVar6] | bVar3;
                uVar6 = uVar6 + 1;
              } while (uVar6 <= bVar2);
            }
            pbVar9 = pbVar9 + 2;
            bVar2 = *pbVar9;
          }
          local_8 = local_8 + 1;
          pbVar11 = pbVar11 + 8;
        } while (local_8 < 4);
        _DAT_0040975c = 1;
        DAT_00409748 = CodePage;
        DAT_00409964 = FUN_0040439a(CodePage);
        DAT_00409750 = (&DAT_0040923c)[iVar10 * 0xc];
        DAT_00409754 = (&DAT_00409240)[iVar10 * 0xc];
        DAT_00409758 = (&DAT_00409244)[iVar10 * 0xc];
        goto LAB_0040433f;
      }
      pUVar4 = pUVar4 + 0xc;
      iVar10 = iVar10 + 1;
    } while ((int)pUVar4 < 0x409328);
    BVar5 = GetCPInfo(CodePage,&local_1c);
    if (BVar5 == 1) {
      puVar12 = (undefined4 *)&DAT_00409860;
      DAT_00409748 = CodePage;
      for (iVar10 = 0x40; iVar10 != 0; iVar10 = iVar10 + -1) {
        *puVar12 = 0;
        puVar12 = puVar12 + 1;
      }
      *(undefined1 *)puVar12 = 0;
      DAT_00409964 = 0;
      if (local_1c.MaxCharSize < 2) {
        _DAT_0040975c = 0;
      }
      else {
        if (local_1c.LeadByte[0] != '\0') {
          pBVar7 = local_1c.LeadByte + 1;
          do {
            bVar2 = *pBVar7;
            if (bVar2 == 0) break;
            for (uVar6 = (uint)pBVar7[-1]; uVar6 <= bVar2; uVar6 = uVar6 + 1) {
              (&DAT_00409861)[uVar6] = (&DAT_00409861)[uVar6] | 4;
            }
            pBVar1 = pBVar7 + 1;
            pBVar7 = pBVar7 + 2;
          } while (*pBVar1 != 0);
        }
        uVar6 = 1;
        do {
          (&DAT_00409861)[uVar6] = (&DAT_00409861)[uVar6] | 8;
          uVar6 = uVar6 + 1;
        } while (uVar6 < 0xff);
        DAT_00409964 = FUN_0040439a(CodePage);
        _DAT_0040975c = 1;
      }
      DAT_00409750 = 0;
      DAT_00409754 = 0;
      DAT_00409758 = 0;
      goto LAB_0040433f;
    }
    if (DAT_00409710 == 0) {
      return 0xffffffff;
    }
  }
  FUN_004043cd();
LAB_0040433f:
  FUN_004043f6();
  return 0;
}



int __cdecl FUN_00404350(int param_1)

{
  int iVar1;
  bool bVar2;
  
  if (param_1 == -2) {
    DAT_00409710 = 1;
                    // WARNING: Could not recover jumptable at 0x0040436a. Too many branches
                    // WARNING: Treating indirect jump as call
    iVar1 = GetOEMCP();
    return iVar1;
  }
  if (param_1 == -3) {
    DAT_00409710 = 1;
                    // WARNING: Could not recover jumptable at 0x0040437f. Too many branches
                    // WARNING: Treating indirect jump as call
    iVar1 = GetACP();
    return iVar1;
  }
  bVar2 = param_1 == -4;
  if (bVar2) {
    param_1 = DAT_00409738;
  }
  DAT_00409710 = (uint)bVar2;
  return param_1;
}



undefined4 __cdecl FUN_0040439a(int param_1)

{
  if (param_1 == 0x3a4) {
    return 0x411;
  }
  if (param_1 == 0x3a8) {
    return 0x804;
  }
  if (param_1 == 0x3b5) {
    return 0x412;
  }
  if (param_1 != 0x3b6) {
    return 0;
  }
  return 0x404;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void FUN_004043cd(void)

{
  int iVar1;
  undefined4 *puVar2;
  
  puVar2 = (undefined4 *)&DAT_00409860;
  for (iVar1 = 0x40; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar2 = 0;
    puVar2 = puVar2 + 1;
  }
  *(undefined1 *)puVar2 = 0;
  DAT_00409748 = 0;
  _DAT_0040975c = 0;
  DAT_00409964 = 0;
  DAT_00409750 = 0;
  DAT_00409754 = 0;
  DAT_00409758 = 0;
  return;
}



void FUN_004043f6(void)

{
  BOOL BVar1;
  uint uVar2;
  char cVar3;
  uint uVar4;
  uint uVar5;
  ushort *puVar6;
  undefined1 uVar7;
  BYTE *pBVar8;
  CHAR *pCVar9;
  WORD local_518 [256];
  WCHAR local_318 [128];
  WCHAR local_218 [128];
  CHAR local_118 [256];
  _cpinfo local_18;
  
  BVar1 = GetCPInfo(DAT_00409748,&local_18);
  if (BVar1 == 1) {
    uVar2 = 0;
    do {
      local_118[uVar2] = (CHAR)uVar2;
      uVar2 = uVar2 + 1;
    } while (uVar2 < 0x100);
    local_118[0] = ' ';
    if (local_18.LeadByte[0] != 0) {
      pBVar8 = local_18.LeadByte + 1;
      do {
        uVar2 = (uint)local_18.LeadByte[0];
        if (uVar2 <= *pBVar8) {
          uVar4 = (*pBVar8 - uVar2) + 1;
          uVar5 = uVar4 >> 2;
          pCVar9 = local_118 + uVar2;
          while (uVar5 != 0) {
            uVar5 = uVar5 - 1;
            builtin_memcpy(pCVar9,"    ",4);
            pCVar9 = pCVar9 + 4;
          }
          for (uVar4 = uVar4 & 3; uVar4 != 0; uVar4 = uVar4 - 1) {
            *pCVar9 = ' ';
            pCVar9 = pCVar9 + 1;
          }
        }
        local_18.LeadByte[0] = pBVar8[1];
        pBVar8 = pBVar8 + 2;
      } while (local_18.LeadByte[0] != 0);
    }
    FUN_00405267(1,local_118,0x100,local_518,DAT_00409748,DAT_00409964,0);
    FUN_00405018(DAT_00409964,0x100,local_118,0x100,local_218,0x100,DAT_00409748,0);
    FUN_00405018(DAT_00409964,0x200,local_118,0x100,local_318,0x100,DAT_00409748,0);
    uVar2 = 0;
    puVar6 = local_518;
    do {
      if ((*puVar6 & 1) == 0) {
        if ((*puVar6 & 2) != 0) {
          (&DAT_00409861)[uVar2] = (&DAT_00409861)[uVar2] | 0x20;
          uVar7 = *(undefined1 *)((int)local_318 + uVar2);
          goto LAB_00404502;
        }
        (&DAT_00409760)[uVar2] = 0;
      }
      else {
        (&DAT_00409861)[uVar2] = (&DAT_00409861)[uVar2] | 0x10;
        uVar7 = *(undefined1 *)((int)local_218 + uVar2);
LAB_00404502:
        (&DAT_00409760)[uVar2] = uVar7;
      }
      uVar2 = uVar2 + 1;
      puVar6 = puVar6 + 1;
    } while (uVar2 < 0x100);
  }
  else {
    uVar2 = 0;
    do {
      if ((uVar2 < 0x41) || (0x5a < uVar2)) {
        if ((0x60 < uVar2) && (uVar2 < 0x7b)) {
          (&DAT_00409861)[uVar2] = (&DAT_00409861)[uVar2] | 0x20;
          cVar3 = (char)uVar2 + -0x20;
          goto LAB_0040454c;
        }
        (&DAT_00409760)[uVar2] = 0;
      }
      else {
        (&DAT_00409861)[uVar2] = (&DAT_00409861)[uVar2] | 0x10;
        cVar3 = (char)uVar2 + ' ';
LAB_0040454c:
        (&DAT_00409760)[uVar2] = cVar3;
      }
      uVar2 = uVar2 + 1;
    } while (uVar2 < 0x100);
  }
  return;
}



void FUN_0040457b(void)

{
  if (DAT_00409aa8 == 0) {
    FUN_004041b7(-3);
    DAT_00409aa8 = 1;
  }
  return;
}



uint * __cdecl FUN_004045a0(uint *param_1,uint *param_2)

{
  byte bVar1;
  uint uVar2;
  uint uVar3;
  uint *puVar4;
  
  uVar3 = (uint)param_2 & 3;
  puVar4 = param_1;
  while (uVar3 != 0) {
    bVar1 = (byte)*param_2;
    uVar3 = (uint)bVar1;
    param_2 = (uint *)((int)param_2 + 1);
    if (bVar1 == 0) goto LAB_00404688;
    *(byte *)puVar4 = bVar1;
    puVar4 = (uint *)((int)puVar4 + 1);
    uVar3 = (uint)param_2 & 3;
  }
  do {
    uVar2 = *param_2;
    uVar3 = *param_2;
    param_2 = param_2 + 1;
    if (((uVar2 ^ 0xffffffff ^ uVar2 + 0x7efefeff) & 0x81010100) != 0) {
      if ((char)uVar3 == '\0') {
LAB_00404688:
        *(byte *)puVar4 = (byte)uVar3;
        return param_1;
      }
      if ((char)(uVar3 >> 8) == '\0') {
        *(short *)puVar4 = (short)uVar3;
        return param_1;
      }
      if ((uVar3 & 0xff0000) == 0) {
        *(short *)puVar4 = (short)uVar3;
        *(byte *)((int)puVar4 + 2) = 0;
        return param_1;
      }
      if ((uVar3 & 0xff000000) == 0) {
        *puVar4 = uVar3;
        return param_1;
      }
    }
    *puVar4 = uVar3;
    puVar4 = puVar4 + 1;
  } while( true );
}



uint * __cdecl FUN_004045b0(uint *param_1,uint *param_2)

{
  byte bVar1;
  uint uVar2;
  uint *puVar3;
  uint uVar4;
  uint *puVar5;
  
  uVar4 = (uint)param_1 & 3;
  puVar3 = param_1;
  while (uVar4 != 0) {
    uVar4 = *puVar3;
    puVar3 = (uint *)((int)puVar3 + 1);
    if ((byte)uVar4 == 0) goto LAB_004045ff;
    uVar4 = (uint)puVar3 & 3;
  }
  do {
    do {
      puVar5 = puVar3;
      puVar3 = puVar5 + 1;
    } while (((*puVar5 ^ 0xffffffff ^ *puVar5 + 0x7efefeff) & 0x81010100) == 0);
    uVar4 = *puVar5;
    if ((char)uVar4 == '\0') goto LAB_00404611;
    if ((char)(uVar4 >> 8) == '\0') {
      puVar5 = (uint *)((int)puVar5 + 1);
      goto LAB_00404611;
    }
    if ((uVar4 & 0xff0000) == 0) {
      puVar5 = (uint *)((int)puVar5 + 2);
      goto LAB_00404611;
    }
  } while ((uVar4 & 0xff000000) != 0);
LAB_004045ff:
  puVar5 = (uint *)((int)puVar3 + -1);
LAB_00404611:
  uVar4 = (uint)param_2 & 3;
  while (uVar4 != 0) {
    bVar1 = (byte)*param_2;
    uVar4 = (uint)bVar1;
    param_2 = (uint *)((int)param_2 + 1);
    if (bVar1 == 0) goto LAB_00404688;
    *(byte *)puVar5 = bVar1;
    puVar5 = (uint *)((int)puVar5 + 1);
    uVar4 = (uint)param_2 & 3;
  }
  do {
    uVar2 = *param_2;
    uVar4 = *param_2;
    param_2 = param_2 + 1;
    if (((uVar2 ^ 0xffffffff ^ uVar2 + 0x7efefeff) & 0x81010100) != 0) {
      if ((char)uVar4 == '\0') {
LAB_00404688:
        *(byte *)puVar5 = (byte)uVar4;
        return param_1;
      }
      if ((char)(uVar4 >> 8) == '\0') {
        *(short *)puVar5 = (short)uVar4;
        return param_1;
      }
      if ((uVar4 & 0xff0000) == 0) {
        *(short *)puVar5 = (short)uVar4;
        *(byte *)((int)puVar5 + 2) = 0;
        return param_1;
      }
      if ((uVar4 & 0xff000000) == 0) {
        *puVar5 = uVar4;
        return param_1;
      }
    }
    *puVar5 = uVar4;
    puVar5 = puVar5 + 1;
  } while( true );
}



// Library Function - Single Match
//  _strlen
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

size_t __cdecl _strlen(char *_Str)

{
  uint uVar1;
  uint *puVar2;
  uint *puVar3;
  
  uVar1 = (uint)_Str & 3;
  puVar2 = (uint *)_Str;
  while (uVar1 != 0) {
    uVar1 = *puVar2;
    puVar2 = (uint *)((int)puVar2 + 1);
    if ((char)uVar1 == '\0') goto LAB_004046e3;
    uVar1 = (uint)puVar2 & 3;
  }
  do {
    do {
      puVar3 = puVar2;
      puVar2 = puVar3 + 1;
    } while (((*puVar3 ^ 0xffffffff ^ *puVar3 + 0x7efefeff) & 0x81010100) == 0);
    uVar1 = *puVar3;
    if ((char)uVar1 == '\0') {
      return (int)puVar3 - (int)_Str;
    }
    if ((char)(uVar1 >> 8) == '\0') {
      return (size_t)((int)puVar3 + (1 - (int)_Str));
    }
    if ((uVar1 & 0xff0000) == 0) {
      return (size_t)((int)puVar3 + (2 - (int)_Str));
    }
  } while ((uVar1 & 0xff000000) != 0);
LAB_004046e3:
  return (size_t)((int)puVar2 + (-1 - (int)_Str));
}



void __thiscall FUN_0040470b(void *this,byte *param_1,int *param_2,void *param_3)

{
  FUN_00404722(this,param_1,param_2,param_3,0);
  return;
}



// WARNING: Globals starting with '_' overlap smaller symbols at the same address

void * __thiscall FUN_00404722(void *this,byte *param_1,int *param_2,void *param_3,uint param_4)

{
  void *pvVar1;
  uint uVar2;
  void *pvVar3;
  uint uVar4;
  void *this_00;
  byte bVar5;
  undefined *puVar6;
  void *local_c;
  byte *local_8;
  
  local_c = (void *)0x0;
  bVar5 = *param_1;
  local_8 = param_1 + 1;
  while( true ) {
    if (DAT_0040953c < 2) {
      uVar2 = (byte)PTR_DAT_00409330[(uint)bVar5 * 2] & 8;
      this = PTR_DAT_00409330;
    }
    else {
      puVar6 = (undefined *)0x8;
      uVar2 = FUN_0040547c(this,(uint)bVar5,8);
      this = puVar6;
    }
    if (uVar2 == 0) break;
    bVar5 = *local_8;
    local_8 = local_8 + 1;
  }
  if (bVar5 == 0x2d) {
    param_4 = param_4 | 2;
LAB_0040477d:
    bVar5 = *local_8;
    local_8 = local_8 + 1;
  }
  else if (bVar5 == 0x2b) goto LAB_0040477d;
  if ((((int)param_3 < 0) || (param_3 == (void *)0x1)) || (0x24 < (int)param_3)) {
    if (param_2 != (int *)0x0) {
      *param_2 = (int)param_1;
    }
    return (void *)0x0;
  }
  this_00 = (void *)0x10;
  if (param_3 == (void *)0x0) {
    if (bVar5 != 0x30) {
      param_3 = (void *)0xa;
      goto LAB_004047e7;
    }
    if ((*local_8 != 0x78) && (*local_8 != 0x58)) {
      param_3 = (void *)0x8;
      goto LAB_004047e7;
    }
    param_3 = (void *)0x10;
  }
  if (((param_3 == (void *)0x10) && (bVar5 == 0x30)) && ((*local_8 == 0x78 || (*local_8 == 0x58))))
  {
    bVar5 = local_8[1];
    local_8 = local_8 + 2;
  }
LAB_004047e7:
  pvVar3 = (void *)(0xffffffff / ZEXT48(param_3));
  do {
    uVar2 = (uint)bVar5;
    if (DAT_0040953c < 2) {
      uVar4 = (byte)PTR_DAT_00409330[uVar2 * 2] & 4;
    }
    else {
      pvVar1 = (void *)0x4;
      uVar4 = FUN_0040547c(this_00,uVar2,4);
      this_00 = pvVar1;
    }
    if (uVar4 == 0) {
      if (DAT_0040953c < 2) {
        uVar2 = *(ushort *)(PTR_DAT_00409330 + uVar2 * 2) & 0x103;
      }
      else {
        pvVar1 = (void *)0x103;
        uVar2 = FUN_0040547c(this_00,uVar2,0x103);
        this_00 = pvVar1;
      }
      if (uVar2 == 0) {
LAB_00404893:
        local_8 = local_8 + -1;
        if ((param_4 & 8) == 0) {
          if (param_2 != (int *)0x0) {
            local_8 = param_1;
          }
          local_c = (void *)0x0;
        }
        else if (((param_4 & 4) != 0) ||
                (((param_4 & 1) == 0 &&
                 ((((param_4 & 2) != 0 && ((void *)0x80000000 < local_c)) ||
                  (((param_4 & 2) == 0 && ((void *)0x7fffffff < local_c)))))))) {
          _DAT_004095a8 = 0x22;
          if ((param_4 & 1) == 0) {
            local_c = (void *)(((param_4 & 2) != 0) + 0x7fffffff);
          }
          else {
            local_c = (void *)0xffffffff;
          }
        }
        if (param_2 != (int *)0x0) {
          *param_2 = (int)local_8;
        }
        if ((param_4 & 2) == 0) {
          return local_c;
        }
        return (void *)-(int)local_c;
      }
      uVar2 = FUN_004053b0(this_00,(int)(char)bVar5);
      this_00 = (void *)(uVar2 - 0x37);
    }
    else {
      this_00 = (void *)((char)bVar5 + -0x30);
    }
    if (param_3 <= this_00) goto LAB_00404893;
    if ((local_c < pvVar3) ||
       ((local_c == pvVar3 && (this_00 <= (void *)(0xffffffff % ZEXT48(param_3)))))) {
      local_c = (void *)((int)local_c * (int)param_3 + (int)this_00);
      param_4 = param_4 | 8;
    }
    else {
      param_4 = param_4 | 0xc;
    }
    bVar5 = *local_8;
    local_8 = local_8 + 1;
  } while( true );
}



// Library Function - Single Match
//  _strchr
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

char * __cdecl _strchr(char *_Str,int _Val)

{
  uint uVar1;
  char cVar2;
  uint uVar3;
  uint uVar4;
  uint *puVar5;
  
  uVar1 = (uint)_Str & 3;
  while (uVar1 != 0) {
    if ((char)*(uint *)_Str == (char)_Val) {
      return (char *)(uint *)_Str;
    }
    if ((char)*(uint *)_Str == '\0') {
      return (char *)0x0;
    }
    uVar1 = (uint)((int)_Str + 1) & 3;
    _Str = (char *)((int)_Str + 1);
  }
  while( true ) {
    while( true ) {
      uVar1 = *(uint *)_Str;
      uVar4 = uVar1 ^ CONCAT22(CONCAT11((char)_Val,(char)_Val),CONCAT11((char)_Val,(char)_Val));
      uVar3 = uVar1 ^ 0xffffffff ^ uVar1 + 0x7efefeff;
      puVar5 = (uint *)((int)_Str + 4);
      if (((uVar4 ^ 0xffffffff ^ uVar4 + 0x7efefeff) & 0x81010100) != 0) break;
      _Str = (char *)puVar5;
      if ((uVar3 & 0x81010100) != 0) {
        if ((uVar3 & 0x1010100) != 0) {
          return (char *)0x0;
        }
        if ((uVar1 + 0x7efefeff & 0x80000000) == 0) {
          return (char *)0x0;
        }
      }
    }
    uVar1 = *(uint *)_Str;
    if ((char)uVar1 == (char)_Val) {
      return (char *)(uint *)_Str;
    }
    if ((char)uVar1 == '\0') {
      return (char *)0x0;
    }
    cVar2 = (char)(uVar1 >> 8);
    if (cVar2 == (char)_Val) {
      return (char *)((int)_Str + 1);
    }
    if (cVar2 == '\0') break;
    cVar2 = (char)(uVar1 >> 0x10);
    if (cVar2 == (char)_Val) {
      return (char *)((int)_Str + 2);
    }
    if (cVar2 == '\0') {
      return (char *)0x0;
    }
    cVar2 = (char)(uVar1 >> 0x18);
    if (cVar2 == (char)_Val) {
      return (char *)((int)_Str + 3);
    }
    _Str = (char *)puVar5;
    if (cVar2 == '\0') {
      return (char *)0x0;
    }
  }
  return (char *)0x0;
}



// Library Function - Single Match
//  _strstr
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

char * __cdecl _strstr(char *_Str,char *_SubStr)

{
  char *pcVar1;
  char *pcVar2;
  char cVar3;
  uint uVar4;
  char cVar5;
  uint uVar6;
  uint uVar7;
  char *pcVar8;
  uint *puVar9;
  char *pcVar10;
  
  cVar3 = *_SubStr;
  if (cVar3 == '\0') {
    return _Str;
  }
  if (_SubStr[1] == '\0') {
    uVar4 = (uint)_Str & 3;
    while (uVar4 != 0) {
      if ((char)*(uint *)_Str == cVar3) {
        return (char *)(uint *)_Str;
      }
      if ((char)*(uint *)_Str == '\0') {
        return (char *)0x0;
      }
      uVar4 = (uint)((int)_Str + 1) & 3;
      _Str = (char *)((int)_Str + 1);
    }
    while( true ) {
      while( true ) {
        uVar4 = *(uint *)_Str;
        uVar7 = uVar4 ^ CONCAT22(CONCAT11(cVar3,cVar3),CONCAT11(cVar3,cVar3));
        uVar6 = uVar4 ^ 0xffffffff ^ uVar4 + 0x7efefeff;
        puVar9 = (uint *)((int)_Str + 4);
        if (((uVar7 ^ 0xffffffff ^ uVar7 + 0x7efefeff) & 0x81010100) != 0) break;
        _Str = (char *)puVar9;
        if ((uVar6 & 0x81010100) != 0) {
          if ((uVar6 & 0x1010100) != 0) {
            return (char *)0x0;
          }
          if ((uVar4 + 0x7efefeff & 0x80000000) == 0) {
            return (char *)0x0;
          }
        }
      }
      uVar4 = *(uint *)_Str;
      if ((char)uVar4 == cVar3) {
        return (char *)(uint *)_Str;
      }
      if ((char)uVar4 == '\0') {
        return (char *)0x0;
      }
      cVar5 = (char)(uVar4 >> 8);
      if (cVar5 == cVar3) {
        return (char *)((int)_Str + 1);
      }
      if (cVar5 == '\0') break;
      cVar5 = (char)(uVar4 >> 0x10);
      if (cVar5 == cVar3) {
        return (char *)((int)_Str + 2);
      }
      if (cVar5 == '\0') {
        return (char *)0x0;
      }
      cVar5 = (char)(uVar4 >> 0x18);
      if (cVar5 == cVar3) {
        return (char *)((int)_Str + 3);
      }
      _Str = (char *)puVar9;
      if (cVar5 == '\0') {
        return (char *)0x0;
      }
    }
    return (char *)0x0;
  }
  do {
    cVar5 = *_Str;
    do {
      while (_Str = _Str + 1, cVar5 != cVar3) {
        if (cVar5 == '\0') {
          return (char *)0x0;
        }
        cVar5 = *_Str;
      }
      cVar5 = *_Str;
      pcVar10 = _Str + 1;
      pcVar8 = _SubStr;
    } while (cVar5 != _SubStr[1]);
    do {
      if (pcVar8[2] == '\0') {
LAB_00404a73:
        return _Str + -1;
      }
      if (*pcVar10 != pcVar8[2]) break;
      pcVar1 = pcVar8 + 3;
      if (*pcVar1 == '\0') goto LAB_00404a73;
      pcVar2 = pcVar10 + 1;
      pcVar8 = pcVar8 + 2;
      pcVar10 = pcVar10 + 2;
    } while (*pcVar1 == *pcVar2);
  } while( true );
}



// Library Function - Single Match
//  _strncmp
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

int __cdecl _strncmp(char *_Str1,char *_Str2,size_t _MaxCount)

{
  char cVar1;
  char cVar2;
  size_t sVar3;
  int iVar4;
  uint uVar5;
  char *pcVar6;
  char *pcVar7;
  
  sVar3 = _MaxCount;
  pcVar6 = _Str1;
  if (_MaxCount != 0) {
    do {
      if (sVar3 == 0) break;
      sVar3 = sVar3 - 1;
      cVar1 = *pcVar6;
      pcVar6 = pcVar6 + 1;
    } while (cVar1 != '\0');
    iVar4 = _MaxCount - sVar3;
    do {
      pcVar6 = _Str2;
      pcVar7 = _Str1;
      if (iVar4 == 0) break;
      iVar4 = iVar4 + -1;
      pcVar7 = _Str1 + 1;
      pcVar6 = _Str2 + 1;
      cVar2 = *_Str1;
      cVar1 = *_Str2;
      _Str2 = pcVar6;
      _Str1 = pcVar7;
    } while (cVar1 == cVar2);
    uVar5 = 0;
    if ((byte)pcVar6[-1] <= (byte)pcVar7[-1]) {
      if (pcVar6[-1] == pcVar7[-1]) {
        return 0;
      }
      uVar5 = 0xfffffffe;
    }
    _MaxCount = ~uVar5;
  }
  return _MaxCount;
}



// WARNING: Unable to track spacebase fully for stack

void FUN_00404ac0(void)

{
  uint in_EAX;
  undefined1 *puVar1;
  undefined4 unaff_retaddr;
  
  puVar1 = &stack0x00000004;
  if (0xfff < in_EAX) {
    do {
      puVar1 = puVar1 + -0x1000;
      in_EAX = in_EAX - 0x1000;
    } while (0xfff < in_EAX);
  }
  *(undefined4 *)(puVar1 + (-4 - in_EAX)) = unaff_retaddr;
  return;
}



int __cdecl FUN_00404aef(undefined4 param_1,undefined4 param_2,undefined4 param_3)

{
  HMODULE hModule;
  int iVar1;
  
  iVar1 = 0;
  if (DAT_00409714 == (FARPROC)0x0) {
    hModule = LoadLibraryA("user32.dll");
    if (hModule != (HMODULE)0x0) {
      DAT_00409714 = GetProcAddress(hModule,"MessageBoxA");
      if (DAT_00409714 != (FARPROC)0x0) {
        DAT_00409718 = GetProcAddress(hModule,"GetActiveWindow");
        DAT_0040971c = GetProcAddress(hModule,"GetLastActivePopup");
        goto LAB_00404b3e;
      }
    }
    iVar1 = 0;
  }
  else {
LAB_00404b3e:
    if (DAT_00409718 != (FARPROC)0x0) {
      iVar1 = (*DAT_00409718)();
      if ((iVar1 != 0) && (DAT_0040971c != (FARPROC)0x0)) {
        iVar1 = (*DAT_0040971c)(iVar1);
      }
    }
    iVar1 = (*DAT_00409714)(iVar1,param_1,param_2,param_3);
  }
  return iVar1;
}



// Library Function - Single Match
//  _strncpy
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

char * __cdecl _strncpy(char *_Dest,char *_Source,size_t _Count)

{
  uint uVar1;
  uint uVar2;
  char cVar3;
  uint uVar4;
  uint *puVar5;
  
  if (_Count == 0) {
    return _Dest;
  }
  puVar5 = (uint *)_Dest;
  if (((uint)_Source & 3) != 0) {
    while( true ) {
      uVar4 = *(uint *)_Source;
      _Source = (char *)((int)_Source + 1);
      *(char *)puVar5 = (char)uVar4;
      puVar5 = (uint *)((int)puVar5 + 1);
      _Count = _Count - 1;
      if (_Count == 0) {
        return _Dest;
      }
      if ((char)uVar4 == '\0') break;
      if (((uint)_Source & 3) == 0) {
        uVar4 = _Count >> 2;
        goto joined_r0x00404bbe;
      }
    }
    do {
      if (((uint)puVar5 & 3) == 0) {
        uVar4 = _Count >> 2;
        cVar3 = '\0';
        if (uVar4 == 0) goto LAB_00404bfb;
        goto LAB_00404c69;
      }
      *(char *)puVar5 = '\0';
      puVar5 = (uint *)((int)puVar5 + 1);
      _Count = _Count - 1;
    } while (_Count != 0);
    return _Dest;
  }
  uVar4 = _Count >> 2;
  if (uVar4 != 0) {
    do {
      uVar1 = *(uint *)_Source;
      uVar2 = *(uint *)_Source;
      _Source = (char *)((int)_Source + 4);
      if (((uVar1 ^ 0xffffffff ^ uVar1 + 0x7efefeff) & 0x81010100) != 0) {
        if ((char)uVar2 == '\0') {
          *puVar5 = 0;
joined_r0x00404c65:
          while( true ) {
            uVar4 = uVar4 - 1;
            puVar5 = puVar5 + 1;
            if (uVar4 == 0) break;
LAB_00404c69:
            *puVar5 = 0;
          }
          cVar3 = '\0';
          _Count = _Count & 3;
          if (_Count != 0) goto LAB_00404bfb;
          return _Dest;
        }
        if ((char)(uVar2 >> 8) == '\0') {
          *puVar5 = uVar2 & 0xff;
          goto joined_r0x00404c65;
        }
        if ((uVar2 & 0xff0000) == 0) {
          *puVar5 = uVar2 & 0xffff;
          goto joined_r0x00404c65;
        }
        if ((uVar2 & 0xff000000) == 0) {
          *puVar5 = uVar2;
          goto joined_r0x00404c65;
        }
      }
      *puVar5 = uVar2;
      puVar5 = puVar5 + 1;
      uVar4 = uVar4 - 1;
joined_r0x00404bbe:
    } while (uVar4 != 0);
    _Count = _Count & 3;
    if (_Count == 0) {
      return _Dest;
    }
  }
  do {
    cVar3 = (char)*(uint *)_Source;
    _Source = (char *)((int)_Source + 1);
    *(char *)puVar5 = cVar3;
    puVar5 = (uint *)((int)puVar5 + 1);
    if (cVar3 == '\0') {
      while (_Count = _Count - 1, _Count != 0) {
LAB_00404bfb:
        *(char *)puVar5 = cVar3;
        puVar5 = (uint *)((int)puVar5 + 1);
      }
      return _Dest;
    }
    _Count = _Count - 1;
  } while (_Count != 0);
  return _Dest;
}



undefined4 * __cdecl FUN_00404c80(undefined4 *param_1,undefined4 *param_2,uint param_3)

{
  uint uVar1;
  uint uVar2;
  undefined4 *puVar3;
  undefined4 *puVar4;
  
  if ((param_2 < param_1) && (param_1 < (undefined4 *)(param_3 + (int)param_2))) {
    puVar3 = (undefined4 *)((param_3 - 4) + (int)param_2);
    puVar4 = (undefined4 *)((param_3 - 4) + (int)param_1);
    if (((uint)puVar4 & 3) == 0) {
      uVar1 = param_3 >> 2;
      uVar2 = param_3 & 3;
      if (7 < uVar1) {
        for (; uVar1 != 0; uVar1 = uVar1 - 1) {
          *puVar4 = *puVar3;
          puVar3 = puVar3 + -1;
          puVar4 = puVar4 + -1;
        }
        switch(uVar2) {
        case 0:
          return param_1;
        case 2:
          goto switchD_00404e37_caseD_2;
        case 3:
          goto switchD_00404e37_caseD_3;
        }
        goto switchD_00404e37_caseD_1;
      }
    }
    else {
      switch(param_3) {
      case 0:
        goto switchD_00404e37_caseD_0;
      case 1:
        goto switchD_00404e37_caseD_1;
      case 2:
        goto switchD_00404e37_caseD_2;
      case 3:
        goto switchD_00404e37_caseD_3;
      default:
        uVar1 = param_3 - ((uint)puVar4 & 3);
        switch((uint)puVar4 & 3) {
        case 1:
          uVar2 = uVar1 & 3;
          *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
          puVar3 = (undefined4 *)((int)puVar3 + -1);
          uVar1 = uVar1 >> 2;
          puVar4 = (undefined4 *)((int)puVar4 - 1);
          if (7 < uVar1) {
            for (; uVar1 != 0; uVar1 = uVar1 - 1) {
              *puVar4 = *puVar3;
              puVar3 = puVar3 + -1;
              puVar4 = puVar4 + -1;
            }
            switch(uVar2) {
            case 0:
              return param_1;
            case 2:
              goto switchD_00404e37_caseD_2;
            case 3:
              goto switchD_00404e37_caseD_3;
            }
            goto switchD_00404e37_caseD_1;
          }
          break;
        case 2:
          uVar2 = uVar1 & 3;
          *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
          uVar1 = uVar1 >> 2;
          *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
          puVar3 = (undefined4 *)((int)puVar3 + -2);
          puVar4 = (undefined4 *)((int)puVar4 - 2);
          if (7 < uVar1) {
            for (; uVar1 != 0; uVar1 = uVar1 - 1) {
              *puVar4 = *puVar3;
              puVar3 = puVar3 + -1;
              puVar4 = puVar4 + -1;
            }
            switch(uVar2) {
            case 0:
              return param_1;
            case 2:
              goto switchD_00404e37_caseD_2;
            case 3:
              goto switchD_00404e37_caseD_3;
            }
            goto switchD_00404e37_caseD_1;
          }
          break;
        case 3:
          uVar2 = uVar1 & 3;
          *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
          *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
          uVar1 = uVar1 >> 2;
          *(undefined1 *)((int)puVar4 + 1) = *(undefined1 *)((int)puVar3 + 1);
          puVar3 = (undefined4 *)((int)puVar3 + -3);
          puVar4 = (undefined4 *)((int)puVar4 - 3);
          if (7 < uVar1) {
            for (; uVar1 != 0; uVar1 = uVar1 - 1) {
              *puVar4 = *puVar3;
              puVar3 = puVar3 + -1;
              puVar4 = puVar4 + -1;
            }
            switch(uVar2) {
            case 0:
              return param_1;
            case 2:
              goto switchD_00404e37_caseD_2;
            case 3:
              goto switchD_00404e37_caseD_3;
            }
            goto switchD_00404e37_caseD_1;
          }
        }
      }
    }
    switch(uVar1) {
    case 7:
      puVar4[7 - uVar1] = puVar3[7 - uVar1];
    case 6:
      puVar4[6 - uVar1] = puVar3[6 - uVar1];
    case 5:
      puVar4[5 - uVar1] = puVar3[5 - uVar1];
    case 4:
      puVar4[4 - uVar1] = puVar3[4 - uVar1];
    case 3:
      puVar4[3 - uVar1] = puVar3[3 - uVar1];
    case 2:
      puVar4[2 - uVar1] = puVar3[2 - uVar1];
    case 1:
      puVar4[1 - uVar1] = puVar3[1 - uVar1];
      puVar3 = puVar3 + -uVar1;
      puVar4 = puVar4 + -uVar1;
    }
    switch(uVar2) {
    case 1:
switchD_00404e37_caseD_1:
      *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
      return param_1;
    case 2:
switchD_00404e37_caseD_2:
      *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
      *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
      return param_1;
    case 3:
switchD_00404e37_caseD_3:
      *(undefined1 *)((int)puVar4 + 3) = *(undefined1 *)((int)puVar3 + 3);
      *(undefined1 *)((int)puVar4 + 2) = *(undefined1 *)((int)puVar3 + 2);
      *(undefined1 *)((int)puVar4 + 1) = *(undefined1 *)((int)puVar3 + 1);
      return param_1;
    }
switchD_00404e37_caseD_0:
    return param_1;
  }
  puVar3 = param_1;
  if (((uint)param_1 & 3) == 0) {
    uVar1 = param_3 >> 2;
    uVar2 = param_3 & 3;
    if (7 < uVar1) {
      for (; uVar1 != 0; uVar1 = uVar1 - 1) {
        *puVar3 = *param_2;
        param_2 = param_2 + 1;
        puVar3 = puVar3 + 1;
      }
      switch(uVar2) {
      case 0:
        return param_1;
      case 2:
        goto switchD_00404cb5_caseD_2;
      case 3:
        goto switchD_00404cb5_caseD_3;
      }
      goto switchD_00404cb5_caseD_1;
    }
  }
  else {
    switch(param_3) {
    case 0:
      goto switchD_00404cb5_caseD_0;
    case 1:
      goto switchD_00404cb5_caseD_1;
    case 2:
      goto switchD_00404cb5_caseD_2;
    case 3:
      goto switchD_00404cb5_caseD_3;
    default:
      uVar1 = (param_3 - 4) + ((uint)param_1 & 3);
      switch((uint)param_1 & 3) {
      case 1:
        uVar2 = uVar1 & 3;
        *(undefined1 *)param_1 = *(undefined1 *)param_2;
        *(undefined1 *)((int)param_1 + 1) = *(undefined1 *)((int)param_2 + 1);
        uVar1 = uVar1 >> 2;
        *(undefined1 *)((int)param_1 + 2) = *(undefined1 *)((int)param_2 + 2);
        param_2 = (undefined4 *)((int)param_2 + 3);
        puVar3 = (undefined4 *)((int)param_1 + 3);
        if (7 < uVar1) {
          for (; uVar1 != 0; uVar1 = uVar1 - 1) {
            *puVar3 = *param_2;
            param_2 = param_2 + 1;
            puVar3 = puVar3 + 1;
          }
          switch(uVar2) {
          case 0:
            return param_1;
          case 2:
            goto switchD_00404cb5_caseD_2;
          case 3:
            goto switchD_00404cb5_caseD_3;
          }
          goto switchD_00404cb5_caseD_1;
        }
        break;
      case 2:
        uVar2 = uVar1 & 3;
        *(undefined1 *)param_1 = *(undefined1 *)param_2;
        uVar1 = uVar1 >> 2;
        *(undefined1 *)((int)param_1 + 1) = *(undefined1 *)((int)param_2 + 1);
        param_2 = (undefined4 *)((int)param_2 + 2);
        puVar3 = (undefined4 *)((int)param_1 + 2);
        if (7 < uVar1) {
          for (; uVar1 != 0; uVar1 = uVar1 - 1) {
            *puVar3 = *param_2;
            param_2 = param_2 + 1;
            puVar3 = puVar3 + 1;
          }
          switch(uVar2) {
          case 0:
            return param_1;
          case 2:
            goto switchD_00404cb5_caseD_2;
          case 3:
            goto switchD_00404cb5_caseD_3;
          }
          goto switchD_00404cb5_caseD_1;
        }
        break;
      case 3:
        uVar2 = uVar1 & 3;
        *(undefined1 *)param_1 = *(undefined1 *)param_2;
        param_2 = (undefined4 *)((int)param_2 + 1);
        uVar1 = uVar1 >> 2;
        puVar3 = (undefined4 *)((int)param_1 + 1);
        if (7 < uVar1) {
          for (; uVar1 != 0; uVar1 = uVar1 - 1) {
            *puVar3 = *param_2;
            param_2 = param_2 + 1;
            puVar3 = puVar3 + 1;
          }
          switch(uVar2) {
          case 0:
            return param_1;
          case 2:
            goto switchD_00404cb5_caseD_2;
          case 3:
            goto switchD_00404cb5_caseD_3;
          }
          goto switchD_00404cb5_caseD_1;
        }
      }
    }
  }
  switch(uVar1) {
  case 7:
    puVar3[uVar1 - 7] = param_2[uVar1 - 7];
  case 6:
    puVar3[uVar1 - 6] = param_2[uVar1 - 6];
  case 5:
    puVar3[uVar1 - 5] = param_2[uVar1 - 5];
  case 4:
    puVar3[uVar1 - 4] = param_2[uVar1 - 4];
  case 3:
    puVar3[uVar1 - 3] = param_2[uVar1 - 3];
  case 2:
    puVar3[uVar1 - 2] = param_2[uVar1 - 2];
  case 1:
    puVar3[uVar1 - 1] = param_2[uVar1 - 1];
    param_2 = param_2 + uVar1;
    puVar3 = puVar3 + uVar1;
  }
  switch(uVar2) {
  case 1:
switchD_00404cb5_caseD_1:
    *(undefined1 *)puVar3 = *(undefined1 *)param_2;
    return param_1;
  case 2:
switchD_00404cb5_caseD_2:
    *(undefined1 *)puVar3 = *(undefined1 *)param_2;
    *(undefined1 *)((int)puVar3 + 1) = *(undefined1 *)((int)param_2 + 1);
    return param_1;
  case 3:
switchD_00404cb5_caseD_3:
    *(undefined1 *)puVar3 = *(undefined1 *)param_2;
    *(undefined1 *)((int)puVar3 + 1) = *(undefined1 *)((int)param_2 + 1);
    *(undefined1 *)((int)puVar3 + 2) = *(undefined1 *)((int)param_2 + 2);
    return param_1;
  }
switchD_00404cb5_caseD_0:
  return param_1;
}



// Library Function - Single Match
//  _memset
// 
// Libraries: Visual Studio 1998 Debug, Visual Studio 1998 Release

void * __cdecl _memset(void *_Dst,int _Val,size_t _Size)

{
  uint uVar1;
  uint uVar2;
  size_t sVar3;
  uint *puVar4;
  
  if (_Size == 0) {
    return _Dst;
  }
  uVar1 = _Val & 0xff;
  puVar4 = (uint *)_Dst;
  if (3 < _Size) {
    uVar2 = -(int)_Dst & 3;
    sVar3 = _Size;
    if (uVar2 != 0) {
      sVar3 = _Size - uVar2;
      do {
        *(undefined1 *)puVar4 = (undefined1)_Val;
        puVar4 = (uint *)((int)puVar4 + 1);
        uVar2 = uVar2 - 1;
      } while (uVar2 != 0);
    }
    uVar1 = uVar1 * 0x1010101;
    _Size = sVar3 & 3;
    uVar2 = sVar3 >> 2;
    if (uVar2 != 0) {
      for (; uVar2 != 0; uVar2 = uVar2 - 1) {
        *puVar4 = uVar1;
        puVar4 = puVar4 + 1;
      }
      if (_Size == 0) {
        return _Dst;
      }
    }
  }
  do {
    *(char *)puVar4 = (char)uVar1;
    puVar4 = (uint *)((int)puVar4 + 1);
    _Size = _Size - 1;
  } while (_Size != 0);
  return _Dst;
}



int __cdecl
FUN_00405018(LCID param_1,uint param_2,char *param_3,int param_4,LPWSTR param_5,int param_6,
            UINT param_7,int param_8)

{
  int iVar1;
  int iVar2;
  void *local_14;
  undefined1 *puStack_10;
  undefined *puStack_c;
  undefined4 local_8;
  
  local_8 = 0xffffffff;
  puStack_c = &DAT_00406498;
  puStack_10 = &LAB_004029a8;
  local_14 = ExceptionList;
  ExceptionList = &local_14;
  if (DAT_00409740 == 0) {
    ExceptionList = &local_14;
    iVar1 = LCMapStringW(0,0x100,L"",1,(LPWSTR)0x0,0);
    if (iVar1 == 0) {
      iVar1 = LCMapStringA(0,0x100,"",1,(LPSTR)0x0,0);
      if (iVar1 == 0) {
        ExceptionList = local_14;
        return 0;
      }
      DAT_00409740 = 2;
    }
    else {
      DAT_00409740 = 1;
    }
  }
  if (0 < param_4) {
    param_4 = FUN_0040523c(param_3,param_4);
  }
  if (DAT_00409740 == 2) {
    iVar1 = LCMapStringA(param_1,param_2,param_3,param_4,(LPSTR)param_5,param_6);
    ExceptionList = local_14;
    return iVar1;
  }
  if (DAT_00409740 == 1) {
    if (param_7 == 0) {
      param_7 = DAT_00409738;
    }
    iVar1 = MultiByteToWideChar(param_7,(-(uint)(param_8 != 0) & 8) + 1,param_3,param_4,(LPWSTR)0x0,
                                0);
    if (iVar1 != 0) {
      local_8 = 0;
      FUN_00404ac0();
      local_8 = 0xffffffff;
      if ((&stack0x00000000 != (undefined1 *)0x3c) &&
         (iVar2 = MultiByteToWideChar(param_7,1,param_3,param_4,(LPWSTR)&stack0xffffffc4,iVar1),
         iVar2 != 0)) {
        iVar2 = LCMapStringW(param_1,param_2,(LPCWSTR)&stack0xffffffc4,iVar1,(LPWSTR)0x0,0);
        if (iVar2 != 0) {
          if ((param_2 & 0x400) == 0) {
            local_8 = 1;
            FUN_00404ac0();
            local_8 = 0xffffffff;
            if (&stack0x00000000 == (undefined1 *)0x3c) {
              ExceptionList = local_14;
              return 0;
            }
            iVar1 = LCMapStringW(param_1,param_2,(LPCWSTR)&stack0xffffffc4,iVar1,
                                 (LPWSTR)&stack0xffffffc4,iVar2);
            if (iVar1 == 0) {
              ExceptionList = local_14;
              return 0;
            }
            if (param_6 == 0) {
              param_6 = 0;
              param_5 = (LPWSTR)0x0;
            }
            iVar2 = WideCharToMultiByte(param_7,0x220,(LPCWSTR)&stack0xffffffc4,iVar2,(LPSTR)param_5
                                        ,param_6,(LPCSTR)0x0,(LPBOOL)0x0);
            iVar1 = iVar2;
          }
          else {
            if (param_6 == 0) {
              ExceptionList = local_14;
              return iVar2;
            }
            if (param_6 < iVar2) {
              ExceptionList = local_14;
              return 0;
            }
            iVar1 = LCMapStringW(param_1,param_2,(LPCWSTR)&stack0xffffffc4,iVar1,param_5,param_6);
          }
          if (iVar1 != 0) {
            ExceptionList = local_14;
            return iVar2;
          }
        }
      }
    }
  }
  ExceptionList = local_14;
  return 0;
}



int __cdecl FUN_0040523c(char *param_1,int param_2)

{
  char *pcVar1;
  int iVar2;
  
  pcVar1 = param_1;
  iVar2 = param_2;
  if (param_2 != 0) {
    do {
      iVar2 = iVar2 + -1;
      if (*pcVar1 == '\0') break;
      pcVar1 = pcVar1 + 1;
    } while (iVar2 != 0);
  }
  if (*pcVar1 == '\0') {
    return (int)pcVar1 - (int)param_1;
  }
  return param_2;
}



BOOL __cdecl
FUN_00405267(DWORD param_1,LPCSTR param_2,int param_3,LPWORD param_4,UINT param_5,LCID param_6,
            int param_7)

{
  undefined1 *puVar1;
  BOOL BVar2;
  int iVar3;
  WORD local_20 [2];
  undefined1 *local_1c;
  void *local_14;
  undefined1 *puStack_10;
  undefined *puStack_c;
  undefined4 local_8;
  
  local_8 = 0xffffffff;
  puStack_c = &DAT_004064b0;
  puStack_10 = &LAB_004029a8;
  local_14 = ExceptionList;
  local_1c = &stack0xffffffc8;
  iVar3 = DAT_00409744;
  ExceptionList = &local_14;
  puVar1 = &stack0xffffffc8;
  if (DAT_00409744 == 0) {
    ExceptionList = &local_14;
    BVar2 = GetStringTypeW(1,L"",1,local_20);
    iVar3 = 1;
    puVar1 = local_1c;
    if (BVar2 == 0) {
      BVar2 = GetStringTypeA(0,1,"",1,local_20);
      if (BVar2 == 0) {
        ExceptionList = local_14;
        return 0;
      }
      iVar3 = 2;
      puVar1 = local_1c;
    }
  }
  local_1c = puVar1;
  DAT_00409744 = iVar3;
  if (DAT_00409744 != 2) {
    if (DAT_00409744 == 1) {
      if (param_5 == 0) {
        param_5 = DAT_00409738;
      }
      iVar3 = MultiByteToWideChar(param_5,(-(uint)(param_7 != 0) & 8) + 1,param_2,param_3,
                                  (LPWSTR)0x0,0);
      if (iVar3 != 0) {
        local_8 = 0;
        FUN_00404ac0();
        local_1c = &stack0xffffffc8;
        _memset(&stack0xffffffc8,0,iVar3 * 2);
        local_8 = 0xffffffff;
        if ((&stack0x00000000 != (undefined1 *)0x38) &&
           (iVar3 = MultiByteToWideChar(param_5,1,param_2,param_3,(LPWSTR)&stack0xffffffc8,iVar3),
           iVar3 != 0)) {
          BVar2 = GetStringTypeW(param_1,(LPCWSTR)&stack0xffffffc8,iVar3,param_4);
          ExceptionList = local_14;
          return BVar2;
        }
      }
    }
    ExceptionList = local_14;
    return 0;
  }
  if (param_6 == 0) {
    param_6 = DAT_00409728;
  }
  BVar2 = GetStringTypeA(param_6,param_1,param_2,param_3,param_4);
  ExceptionList = local_14;
  return BVar2;
}



uint __thiscall FUN_004053b0(void *this,uint param_1)

{
  uint uVar1;
  uint uVar2;
  int iVar3;
  void *local_8;
  
  uVar1 = param_1;
  if (DAT_00409728 == 0) {
    if ((0x60 < (int)param_1) && ((int)param_1 < 0x7b)) {
      uVar1 = param_1 - 0x20;
    }
  }
  else {
    local_8 = this;
    if ((int)param_1 < 0x100) {
      if (DAT_0040953c < 2) {
        uVar2 = (byte)PTR_DAT_00409330[param_1 * 2] & 2;
      }
      else {
        uVar2 = FUN_0040547c(this,param_1,2);
      }
      if (uVar2 == 0) {
        return uVar1;
      }
    }
    if ((PTR_DAT_00409330[((int)uVar1 >> 8 & 0xffU) * 2 + 1] & 0x80) == 0) {
      param_1 = CONCAT31((int3)(param_1 >> 8),(char)uVar1) & 0xffff00ff;
      iVar3 = 1;
    }
    else {
      uVar2 = param_1 >> 0x10;
      param_1._0_2_ = CONCAT11((char)uVar1,(char)(uVar1 >> 8));
      param_1 = CONCAT22((short)uVar2,(undefined2)param_1) & 0xff00ffff;
      iVar3 = 2;
    }
    iVar3 = FUN_00405018(DAT_00409728,0x200,(char *)&param_1,iVar3,(LPWSTR)&local_8,3,0,1);
    if (iVar3 != 0) {
      if (iVar3 == 1) {
        uVar1 = (uint)local_8 & 0xff;
      }
      else {
        uVar1 = (uint)local_8 & 0xffff;
      }
    }
  }
  return uVar1;
}



uint __thiscall FUN_0040547c(void *this,int param_1,uint param_2)

{
  BOOL BVar1;
  int iVar2;
  undefined4 local_8;
  
  if (param_1 + 1U < 0x101) {
    param_1._2_2_ = *(ushort *)(PTR_DAT_00409330 + param_1 * 2);
  }
  else {
    if ((PTR_DAT_00409330[(param_1 >> 8 & 0xffU) * 2 + 1] & 0x80) == 0) {
      local_8 = CONCAT31((int3)((uint)this >> 8),(char)param_1) & 0xffff00ff;
      iVar2 = 1;
    }
    else {
      local_8._0_2_ = CONCAT11((char)param_1,(char)((uint)param_1 >> 8));
      local_8 = CONCAT22((short)((uint)this >> 0x10),(undefined2)local_8) & 0xff00ffff;
      iVar2 = 2;
    }
    BVar1 = FUN_00405267(1,(LPCSTR)&local_8,iVar2,(LPWORD)((int)&param_1 + 2),0,0,1);
    if (BVar1 == 0) {
      return 0;
    }
  }
  return param_1._2_2_ & param_2;
}



void RtlUnwind(PVOID TargetFrame,PVOID TargetIp,PEXCEPTION_RECORD ExceptionRecord,PVOID ReturnValue)

{
                    // WARNING: Could not recover jumptable at 0x004054f2. Too many branches
                    // WARNING: Treating indirect jump as call
  RtlUnwind(TargetFrame,TargetIp,ExceptionRecord,ReturnValue);
  return;
}




